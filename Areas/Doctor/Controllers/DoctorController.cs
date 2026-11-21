using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DatLichKhamBenh.Models.Entities;

namespace DatLichKhamBenh.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = "Doctor")] // Chỉ Bác sĩ mới được truy cập
    public class DoctorController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DoctorController> _logger;

        public DoctorController(
            UserManager<ApplicationUser> userManager,
            ILogger<DoctorController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Doctor/Doctor/Index
        // Trang chủ (Dashboard) của Bác sĩ
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge(); // Buộc đăng nhập lại nếu không tìm thấy

            ViewBag.FullName = $"{user.MiddleName} {user.FirstName}".Trim();
            return View();
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