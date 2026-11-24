using Microsoft.AspNetCore.Mvc;
using DatLichKhamBenh.Models.ViewModels;
using DatLichKhamBenh.Services.Interfaces;
using DatLichKhamBenh.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Models.Entities;

namespace MediSlot.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = "Doctor")]
    public class ExaminationController : Controller
    {
        private readonly IMedicalRecordService _service;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        
        public ExaminationController(
            IMedicalRecordService service,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        { 
            _service = service; 
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int id = 0)
        {
            var userId = _userManager.GetUserId(User);
            var doctor = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.UserId == userId);
            if (id == 0)
            {
                var appointments = doctor == null
                    ? new List<Appointment>()
                    : await _context.Appointments
                        .AsNoTracking()
                        .Include(a => a.Patient)
                        .Where(a => a.DoctorId == doctor.Id && a.Status == "Đã đến")
                        .OrderBy(a => a.AppointmentDate)
                        .ThenBy(a => a.TimeSlot)
                        .ToListAsync();
                return View("~/Areas/Doctor/Views/Examination/Queue.cshtml", appointments);
            }
            var appointment = doctor == null ? null : await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.Id);

            if (appointment == null)
                return NotFound();
            if (appointment.Status != "Đã đến")
            {
                TempData["Error"] = "Lễ tân cần xác nhận bệnh nhân đã đến trước khi bắt đầu khám.";
                return RedirectToAction("Index", "Doctor", new { area = "Doctor" });
            }

            PopulateAppointmentViewData(appointment);

            var model = new KhamBenhViewModel { LichHenId = id };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(KhamBenhViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var doctor = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.UserId == userId);
            var appointment = doctor == null ? null : await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == model.LichHenId
                    && a.DoctorId == doctor.Id
                    && a.Status == "Đã đến");
            if (appointment == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var appointmentForView = await _context.Appointments
                    .AsNoTracking()
                    .Include(a => a.Patient)
                    .FirstAsync(a => a.Id == model.LichHenId);
                PopulateAppointmentViewData(appointmentForView);
                return View("Index", model);
            }

            if (!_service.LuuHoSoBenhAn(model))
            {
                ModelState.AddModelError("", "Không thể lưu hồ sơ hoặc lịch hẹn không còn ở trạng thái đã check-in.");
                var appointmentForView = await _context.Appointments
                    .AsNoTracking()
                    .Include(a => a.Patient)
                    .FirstAsync(a => a.Id == model.LichHenId);
                PopulateAppointmentViewData(appointmentForView);
                return View("Index", model);
            }

            TempData["SuccessMessage"] = $"Đã hoàn thành ca khám LH{appointment.Id:D3} và lưu hồ sơ.";
            return RedirectToAction("Index", "Doctor", new { area = "Doctor" });
        }

        private void PopulateAppointmentViewData(Appointment appointment)
        {
            ViewBag.PatientName = appointment.Patient?.FullName ?? "Bệnh nhân";
            ViewBag.PatientPhone = appointment.Patient?.PhoneNumber ?? "Chưa có SĐT";
            ViewBag.PatientAge = appointment.Patient?.DateOfBirth is DateTime dateOfBirth
                ? (DateTime.Today.Year - dateOfBirth.Year).ToString()
                : "Chưa cập nhật";
            ViewBag.AppointmentCode = $"LH{appointment.Id:D3}";
            ViewBag.AppointmentDate = appointment.AppointmentDate.ToString("dd/MM/yyyy");
            ViewBag.Symptoms = appointment.Symptoms ?? "Chưa có triệu chứng";
        }
    }
}