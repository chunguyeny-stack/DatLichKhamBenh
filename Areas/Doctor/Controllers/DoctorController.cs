using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = "Doctor")] // Chỉ Bác sĩ mới được truy cập
    public class DoctorController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DoctorController> _logger;
        private readonly ApplicationDbContext _context;

        public DoctorController(
            UserManager<ApplicationUser> userManager,
            ILogger<DoctorController> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        // GET: /Doctor/Doctor/Index
        // Trang chủ (Dashboard) của Bác sĩ
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge(); // Buộc đăng nhập lại nếu không tìm thấy

            var doctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == user.Id && d.IsActive);
            if (doctor == null)
                return NotFound("Tài khoản chưa được liên kết với hồ sơ bác sĩ.");

            var appointments = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.Id
                    && a.AppointmentDate.Date >= DateTime.Today
                    && a.Status != "Hoàn thành"
                    && a.Status != "Đã thanh toán"
                    && a.Status != "Đã hủy"
                    && a.Status != "Cancelled")
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.TimeSlot)
                .ToListAsync();

            ViewBag.FullName = user.FullName ?? $"{user.MiddleName} {user.FirstName}".Trim();
            ViewBag.WaitingCount = appointments.Count(a => a.Status == "Chờ xác nhận" || a.Status == "Chờ");
            ViewBag.CheckedInCount = appointments.Count(a => a.Status == "Đã đến");
            ViewBag.CompletedCount = await _context.Appointments.CountAsync(a =>
                a.DoctorId == doctor.Id
                && a.AppointmentDate.Date == DateTime.Today
                && (a.Status == "Hoàn thành" || a.Status == "Đã thanh toán"));
            return View("~/Areas/Doctor/Views/Home/Index.cshtml", appointments);
        }

        // GET: /Doctor/Doctor/Appointments
        // Danh sách lịch hẹn của bác sĩ (sẽ mở rộng sau)
        [HttpGet]
        public IActionResult Appointments()
        {
            // TODO: Truy vấn danh sách lịch hẹn từ DbContext
            // var appointments = _context.Appointments
            //     .Where(a => a.DoctorId == currentDoctorId)
            //     .ToList();
            return View();
        }

        // GET: /Doctor/Doctor/PatientRecords/{id}
        // Xem hồ sơ bệnh án của một bệnh nhân cụ thể (sẽ mở rộng sau)
        [HttpGet]
        public IActionResult PatientRecords(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            // TODO: Truy vấn hồ sơ bệnh án
            return View();
        }
    }
}