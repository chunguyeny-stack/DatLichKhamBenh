using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Models.Enums;
using DatLichKhamBenh.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Controllers
{
    [Authorize(Roles = "Patient")]
    public class BookingController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager) : Controller
    {
        private const decimal GiaKhamMacDinh = 200000m;
        private static readonly TimeSpan ThoiLuongKham = TimeSpan.FromHours(1);

        private sealed record BookingSlot(string TimeSlot, CaKham CaKham);

        [HttpGet]
        public async Task<IActionResult> Create(int? doctorId, int? chuyenKhoaId)
        {
            var model = new DatLichViewModel
            {
                NgayKham = DateTime.Today.AddDays(1),
                CaKham = CaKham.Sang,
                KhungGio = "08:00 - 09:00",
                PhiCoBan = GiaKhamMacDinh,
                TongPhi = GiaKhamMacDinh
            };

            if (doctorId is > 0)
            {
                var selectedDoctor = await context.Doctors
                    .AsNoTracking()
                    .Include(d => d.Specialty)
                    .FirstOrDefaultAsync(d => d.Id == doctorId.Value && d.IsActive);

                if (selectedDoctor != null)
                {
                        model.BacSiId = selectedDoctor.Id;
                        model.ChuyenKhoaId = selectedDoctor.SpecialtyId;
                        model.PhiCoBan = selectedDoctor.ConsultationFee > 0 ? selectedDoctor.ConsultationFee : GiaKhamMacDinh;
                }
            }
            else if (chuyenKhoaId is > 0)
            {
                model.ChuyenKhoaId = chuyenKhoaId.Value;
            }

            await NapDuLieuFormAsync(model);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, DateTime appointmentDate)
        {
            if (doctorId <= 0 || appointmentDate.Date < DateTime.Today || appointmentDate.Date > DateTime.Today.AddDays(14))
            {
                return BadRequest(new { detail = "Bác sĩ hoặc ngày khám không hợp lệ." });
            }

            var slots = await GetAvailableBookingSlotsAsync(doctorId, appointmentDate.Date);
            return Json(slots.Select(slot => new
            {
                timeSlot = slot.TimeSlot,
                caKham = (int)slot.CaKham
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DatLichViewModel model)
        {
            var minDate = DateTime.Today;
            var maxDate = DateTime.Today.AddDays(14);

            if (model.NgayKham.Date < minDate || model.NgayKham.Date > maxDate)
            {
                ModelState.AddModelError(nameof(model.NgayKham),
                    $"Chỉ được đặt lịch từ {minDate:dd/MM/yyyy} đến {maxDate:dd/MM/yyyy} (giới hạn 14 ngày).");
            }

            var bacSi = await context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == model.BacSiId && d.IsActive);
            if (bacSi == null)
            {
                ModelState.AddModelError(nameof(model.BacSiId), "Vui lòng chọn bác sĩ hợp lệ.");
            }

            var doctor = bacSi == null
                ? null
                : bacSi;
            if (bacSi != null && doctor == null)
            {
                ModelState.AddModelError(nameof(model.BacSiId), "Bác sĩ hiện chưa thể nhận lịch khám.");
            }

            if (doctor != null && !string.IsNullOrWhiteSpace(model.KhungGio))
            {
                var requestedSlot = model.KhungGio.Trim();
                var scheduleSlots = await GetScheduleBookingSlotsAsync(doctor.Id, model.NgayKham.Date);
                if (!scheduleSlots.Any(slot => slot.TimeSlot == requestedSlot && slot.CaKham == model.CaKham))
                {
                    ModelState.AddModelError(nameof(model.KhungGio),
                        "Bác sĩ không có lịch làm việc ở khung giờ này. Vui lòng chọn khung giờ khác.");
                }

                var appointments = await context.Appointments
                    .AsNoTracking()
                    .Where(a => a.DoctorId == doctor.Id
                        && a.AppointmentDate.Date == model.NgayKham.Date
                        && a.TimeSlot == requestedSlot)
                    .Select(a => a.Status)
                    .ToListAsync();
                var daCoLich = appointments.Any(status => !IsCancelled(status));

                if (daCoLich)
                {
                    ModelState.AddModelError(nameof(model.KhungGio),
                        "Khung giờ này của bác sĩ đã được đặt. Vui lòng chọn khung giờ khác.");
                }
            }

            if (!ModelState.IsValid)
            {
                await NapDuLieuFormAsync(model);
                return View(model);
            }

            var userId = userManager.GetUserId(User) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var user = await userManager.GetUserAsync(User);
            var benhNhan = await context.Patients.FirstOrDefaultAsync(bn => bn.UserId == userId);
            if (benhNhan == null)
            {
                benhNhan = new Patient
                {
                    UserId = userId,
                    FullName = user?.FullName ?? user?.Email ?? "Bệnh nhân",
                    PhoneNumber = user?.PhoneNumber,
                    Email = user?.Email
                };
                context.Patients.Add(benhNhan);
                await context.SaveChangesAsync();
            }

            var giaGoc = bacSi is { ConsultationFee: > 0 } ? bacSi.ConsultationFee : GiaKhamMacDinh;
            var tongPhi = model.CaKham == CaKham.Toi ? Math.Round(giaGoc * 1.1m, 0) : giaGoc;

            var lichHen = new Appointment
            {
                PatientId = benhNhan.Id,
                DoctorId = doctor!.Id,
                AppointmentDate = model.NgayKham.Date,
                TimeSlot = model.KhungGio.Trim(),
                Symptoms = model.GhiChu.Trim(),
                Status = "Chờ xác nhận",
                Notes = $"Đặt bởi bệnh nhân lúc {DateTime.Now:dd/MM/yyyy HH:mm}",
                ConsultationFee = tongPhi
            };

            context.Appointments.Add(lichHen);
            await context.SaveChangesAsync();

            TempData["Success"] =
                $"Đặt lịch thành công ngày {model.NgayKham.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} ({model.KhungGio}) • Phí dự kiến: {tongPhi.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))} đ.";

            return RedirectToAction("Appointments", "Patient");
        }

        [HttpGet]
        public IActionResult Appointments(string? trangThaiLoc)
        {
            return RedirectToAction("Appointments", "Patient", new { trangThaiLoc });
        }

        private async Task NapDuLieuFormAsync(DatLichViewModel model)
        {
            model.ChuyenKhoas = await context.Specialties
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            var query = context.Doctors
                .AsNoTracking()
                .Include(d => d.Specialty)
                .Include(d => d.User)
                .Where(d => d.IsActive)
                .AsQueryable();

            if (model.ChuyenKhoaId is > 0)
            {
                query = query.Where(d => d.SpecialtyId == model.ChuyenKhoaId.Value);
            }

            model.BacSis = await query
                .OrderBy(d => d.FullName)
                .ToListAsync();

            var bacSiChon = model.BacSis.FirstOrDefault(d => d.Id == model.BacSiId);
            var giaGoc = bacSiChon is { ConsultationFee: > 0 } ? bacSiChon.ConsultationFee : model.PhiCoBan;
            if (giaGoc <= 0) giaGoc = GiaKhamMacDinh;

            model.PhiCoBan = giaGoc;
            model.TongPhi = model.CaKham == CaKham.Toi ? Math.Round(giaGoc * 1.1m, 0) : giaGoc;
        }

        private async Task<List<BookingSlot>> GetAvailableBookingSlotsAsync(int doctorId, DateTime workDate)
        {
            var scheduleSlots = await GetScheduleBookingSlotsAsync(doctorId, workDate);
            var bookedSlots = await context.Appointments
                .AsNoTracking()
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == workDate.Date)
                .Select(a => new { a.TimeSlot, a.Status })
                .ToListAsync();

            return scheduleSlots
                .Where(slot => !bookedSlots.Any(appointment =>
                    appointment.TimeSlot == slot.TimeSlot && !IsCancelled(appointment.Status)))
                .ToList();
        }

        private async Task<List<BookingSlot>> GetScheduleBookingSlotsAsync(int doctorId, DateTime workDate)
        {
            var schedules = await context.DoctorSchedules
                .AsNoTracking()
                .Where(schedule => schedule.DoctorId == doctorId
                    && schedule.WorkDate.Date == workDate.Date
                    && schedule.IsAvailable)
                .ToListAsync();

            var slots = new List<BookingSlot>();
            foreach (var schedule in schedules)
            {
                if (!string.IsNullOrWhiteSpace(schedule.AbsenceReason))
                {
                    continue;
                }

                var times = schedule.TimeSlot.Split('-', 2);
                if (times.Length != 2
                    || !TimeSpan.TryParse(times[0].Trim(), CultureInfo.InvariantCulture, out var start)
                    || !TimeSpan.TryParse(times[1].Trim(), CultureInfo.InvariantCulture, out var end))
                {
                    continue;
                }

                for (var slotStart = start; slotStart + ThoiLuongKham <= end; slotStart += ThoiLuongKham)
                {
                    var slotEnd = slotStart + ThoiLuongKham;
                    slots.Add(new BookingSlot(
                        $"{slotStart.ToString(@"hh\:mm", CultureInfo.InvariantCulture)} - {slotEnd.ToString(@"hh\:mm", CultureInfo.InvariantCulture)}",
                        GetCaKham(schedule.Shift, slotStart)));
                }
            }

            return slots
                .GroupBy(slot => new { slot.TimeSlot, slot.CaKham })
                .Select(group => group.First())
                .OrderBy(slot => slot.CaKham)
                .ThenBy(slot => slot.TimeSlot)
                .ToList();
        }

        private static CaKham GetCaKham(string? shift, TimeSpan start)
        {
            var normalizedShift = (shift ?? string.Empty).Trim().ToLowerInvariant();
            if (normalizedShift.Contains("chiều") || normalizedShift.Contains("chieu"))
            {
                return CaKham.Chieu;
            }

            if (normalizedShift.Contains("tối") || normalizedShift.Contains("toi"))
            {
                return CaKham.Toi;
            }

            if (normalizedShift.Contains("sáng") || normalizedShift.Contains("sang"))
            {
                return CaKham.Sang;
            }

            return start.Hours >= 18 ? CaKham.Toi : start.Hours >= 12 ? CaKham.Chieu : CaKham.Sang;
        }

        private static bool IsCancelled(string? status)
        {
            var normalizedStatus = (status ?? string.Empty).Trim();
            return normalizedStatus.Equals("Đã hủy", StringComparison.OrdinalIgnoreCase)
                || normalizedStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase);
        }
    }
}
