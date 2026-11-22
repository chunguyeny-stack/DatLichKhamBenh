using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;

namespace DatLichKhamBenh.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    [Authorize(Roles = "Receptionist")]
    public class quan_ly_lich_hen_BN : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public quan_ly_lich_hen_BN(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 50)
        {
            page = Math.Max(1, page);
            pageSize = Math.Max(1, pageSize);

            var expired = await _context.Appointments
                .Where(x => x.AppointmentDate.Date < DateTime.Today && x.Status != null
                         && !x.Status.ToLower().Contains("hủy") && !x.Status.ToLower().Contains("cancel")
                         && !x.Status.ToLower().Contains("hoàn thành") && !x.Status.ToLower().Contains("completed")
                         && !x.Status.ToLower().Contains("đã thanh toán") && !x.Status.ToLower().Contains("paid")
                         && !x.Status.ToLower().Contains("check-in") && !x.Status.ToLower().Contains("checked")
                         && !x.Status.ToLower().Contains("đã đến"))
                .ToListAsync();

            if (expired.Count > 0)
            {
                foreach (var e in expired)
                {
                    e.Status = "Đã hủy";
                    e.Notes = string.IsNullOrWhiteSpace(e.Notes)
                        ? "Tự động hủy do quá ngày khám."
                        : (e.Notes.Contains("quá ngày khám") ? e.Notes : $"{e.Notes} | Tự động hủy do quá ngày khám.");
                }
                await _context.SaveChangesAsync();
            }

            var query = _context.Appointments
                .AsNoTracking()
                .Include(x => x.Doctor)
                    .ThenInclude(d => d!.User)
                .Include(x => x.Patient)
                    .ThenInclude(p => p!.User)
                .OrderBy(x => x.AppointmentDate)
                .ThenBy(x => x.TimeSlot);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var sdtMap = new Dictionary<int, string>();
            foreach (var a in items)
            {
                var p = !string.IsNullOrWhiteSpace(a.Patient?.PhoneNumber)
                    ? a.Patient!.PhoneNumber
                    : a.Patient?.User?.PhoneNumber;
                sdtMap[a.Id] = !string.IsNullOrWhiteSpace(p) ? p! : "Chưa có SĐT";
            }

            ViewBag.ReceptionistName = User?.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(User.Identity.Name)
                ? User.Identity.Name
                : "Lễ tân";
            ViewBag.SoDienThoaiBenhNhanMap = sdtMap;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize));
            ViewBag.TotalCount = totalCount;

            string[] candidates =
            {
                "~/Areas/Receptionist/Views/Appointment/quan_ly_lich_hen_BN.cshtml",
                "~/Areas/Receptionist/Views/Home/quan_ly_lich_hen_BN.cshtml",
                "~/Areas/Receptionist/Views/quan_ly_lich_hen_BN/quan_ly_lich_hen_BN.cshtml",
                "~/Areas/Receptionist/Views/Appointment/Index.cshtml"
            };

            foreach (var v in candidates)
            {
                var full = Path.Combine(_env.ContentRootPath, v.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(full))
                    return View(v, items);
            }

            return View(candidates[0], items);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            appt.Status = "Đã xác nhận";
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xác nhận lịch hẹn #{id}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int lichHenId, DateTime ngayMoi)
        {
            var appt = await _context.Appointments.FindAsync(lichHenId);
            if (appt == null || appt.DoctorId <= 0) return NotFound();

            var availableSlots = await _context.DoctorSchedules
                .AsNoTracking()
                .Where(x => x.DoctorId == appt.DoctorId
                         && x.WorkDate.Date == ngayMoi.Date
                         && x.IsAvailable
                         && string.IsNullOrWhiteSpace(x.AbsenceReason))
                .Select(x => x.TimeSlot)
                .ToListAsync();

            var bookedSlots = await _context.Appointments
                .AsNoTracking()
                .Where(x => x.DoctorId == appt.DoctorId
                         && x.AppointmentDate.Date == ngayMoi.Date
                         && x.Id != lichHenId
                         && x.Status != "Đã hủy")
                .Select(x => x.TimeSlot)
                .ToListAsync();

            return Json(availableSlots.Where(k => !string.IsNullOrWhiteSpace(k) && !bookedSlots.Contains(k)).Distinct().ToList());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(int id, DateTime ngayMoi, string khungGioMoi)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            var isConflict = await _context.Appointments
                .AnyAsync(x => x.DoctorId == appt.DoctorId
                            && x.AppointmentDate.Date == ngayMoi.Date
                            && x.TimeSlot == khungGioMoi
                            && x.Id != id
                            && x.Status != "Đã hủy");

            if (isConflict)
            {
                TempData["Error"] = "Khung giờ đã có bệnh nhân đặt, vui lòng chọn khung giờ trống khác.";
                return RedirectToAction(nameof(Index));
            }

            appt.AppointmentDate = ngayMoi.Date;
            appt.TimeSlot = khungGioMoi;
            appt.Status = "Đã xác nhận";
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã đổi lịch hẹn #{id} sang {ngayMoi:dd/MM/yyyy} ({khungGioMoi}).";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOverdue(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            appt.Status = "Đã hủy";
            appt.Notes = string.IsNullOrWhiteSpace(appt.Notes)
                ? "Hủy lịch do bệnh nhân quá giờ hẹn."
                : $"{appt.Notes} | Hủy lịch do bệnh nhân quá giờ hẹn.";
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã hủy lịch hẹn #{id} do quá giờ hẹn.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            var st = (appt.Status ?? string.Empty).Trim().ToLower();
            if (st != "đã xác nhận" && st != "confirmed")
            {
                TempData["Error"] = "Chỉ xác nhận đến khám cho lịch hẹn đã được xác nhận.";
                return RedirectToAction(nameof(Index));
            }

            appt.Status = "Đã đến";
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Bệnh nhân của lịch hẹn #{id} đã đến khám.";
            return RedirectToAction(nameof(Index));
        }
    }
}