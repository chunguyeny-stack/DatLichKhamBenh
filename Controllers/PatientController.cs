using System;
using System.Linq;
using System.Threading.Tasks;
using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Appointments(string? trangThaiLoc)
        {
            var userId = userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var patient = await context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                var user = await userManager.GetUserAsync(User);
                patient = new Patient
                {
                    UserId = userId,
                    FullName = user?.FullName ?? user?.Email ?? "Bệnh nhân",
                    PhoneNumber = user?.PhoneNumber,
                    Email = user?.Email
                };
                context.Patients.Add(patient);
                await context.SaveChangesAsync();
            }

            var query = context.Appointments
                .AsNoTracking()
                .Include(a => a.Doctor)
                    .ThenInclude(d => d!.Specialty)
                .Where(a => a.PatientId == patient.Id);

            query = trangThaiLoc switch
            {
                "ChoXacNhan" => query.Where(a => a.Status == "Chờ xác nhận" || a.Status == "Chờ"),
                "DaXacNhan" => query.Where(a => a.Status == "Đã xác nhận" || a.Status == "Đã đến"),
                "HoanThanh" => query.Where(a => a.Status == "Hoàn thành" || a.Status == "Đã thanh toán"),
                "DaHuy" => query.Where(a => a.Status == "Đã hủy" || a.Status == "Cancelled"),
                _ => query
            };

            ViewBag.TrangThaiLoc = trangThaiLoc;
            return View(await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.Id)
                .ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var userId = userManager.GetUserId(User);
            var patient = await context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            var appointment = patient == null
                ? null
                : await context.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.PatientId == patient.Id);

            if (appointment == null)
            {
                TempData["Error"] = "Không tìm thấy lịch hẹn.";
                return RedirectToAction(nameof(Appointments));
            }

            if ((appointment.AppointmentDate.Date - DateTime.Today).TotalHours < 24)
            {
                TempData["Error"] = "Chỉ có thể hủy lịch hẹn trước thời điểm khám ít nhất 24 giờ.";
                return RedirectToAction(nameof(Appointments));
            }

            if (appointment.Status == "Hoàn thành" || appointment.Status == "Đã thanh toán"
                || appointment.Status == "Đã hủy" || appointment.Status == "Cancelled")
            {
                TempData["Error"] = "Lịch hẹn này không thể hủy.";
                return RedirectToAction(nameof(Appointments));
            }

            appointment.Status = "Đã hủy";
            await context.SaveChangesAsync();
            TempData["Success"] = $"Đã hủy lịch hẹn #{id}.";
            return RedirectToAction(nameof(Appointments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReschedule(int id, DateTime ngayMoi, CaKham caMoi, string? khungGioMoi)
        {
            var userId = userManager.GetUserId(User);
            var patient = await context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            var appointment = patient == null
                ? null
                : await context.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.PatientId == patient.Id);

            if (appointment == null)
            {
                TempData["Error"] = "Không tìm thấy lịch hẹn.";
                return RedirectToAction(nameof(Appointments));
            }

            if (ngayMoi.Date < DateTime.Today || ngayMoi.Date > DateTime.Today.AddDays(14))
            {
                TempData["Error"] = "Ngày đổi lịch phải nằm trong giới hạn 14 ngày tới.";
                return RedirectToAction(nameof(Appointments));
            }

            var defaultSlot = caMoi switch
            {
                CaKham.Chieu => "13:30 - 14:30",
                CaKham.Toi => "18:00 - 19:00",
                _ => "08:00 - 09:00"
            };
            var selectedSlot = string.IsNullOrWhiteSpace(khungGioMoi) ? defaultSlot : khungGioMoi.Trim();

            var hasSchedule = await context.DoctorSchedules.AnyAsync(s =>
                s.DoctorId == appointment.DoctorId
                && s.WorkDate.Date == ngayMoi.Date
                && s.TimeSlot == selectedSlot
                && s.IsAvailable
                && string.IsNullOrEmpty(s.AbsenceReason));
            var hasConflict = await context.Appointments.AnyAsync(a =>
                a.DoctorId == appointment.DoctorId
                && a.AppointmentDate.Date == ngayMoi.Date
                && a.TimeSlot == selectedSlot
                && a.Id != id
                && a.Status != "Đã hủy"
                && a.Status != "Cancelled");

            if (!hasSchedule || hasConflict)
            {
                TempData["Error"] = "Khung giờ mới không còn trống. Vui lòng chọn lịch khác.";
                return RedirectToAction(nameof(Appointments));
            }

            appointment.AppointmentDate = ngayMoi.Date;
            appointment.TimeSlot = selectedSlot;
            appointment.Status = "Chờ xác nhận";
            await context.SaveChangesAsync();
            TempData["Success"] = $"Đã gửi yêu cầu đổi lịch hẹn #{id} sang {ngayMoi:dd/MM/yyyy} ({selectedSlot}).";
            return RedirectToAction(nameof(Appointments));
        }
    }
}
