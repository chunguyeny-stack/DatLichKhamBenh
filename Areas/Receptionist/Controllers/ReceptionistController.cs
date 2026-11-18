using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DatLichKhamBenh.Models.Entities;

namespace DatLichKhamBenh.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    [Authorize(Roles = "Receptionist")] // Chỉ Lễ tân mới được truy cập
    public class ReceptionistController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ReceptionistController> _logger;

        public ReceptionistController(
            UserManager<ApplicationUser> userManager,
            ILogger<ReceptionistController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Receptionist/Receptionist/Index
        // Trang chủ (Dashboard) của Lễ tân
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            ViewBag.FullName = $"{user.MiddleName} {user.FirstName}".Trim();
            return View();
        }

        // GET: /Receptionist/Receptionist/Appointments
        // Quản lý toàn bộ lịch hẹn trong ngày / tuần
        [HttpGet]
        public IActionResult Appointments()
        {
            // TODO: Truy vấn tất cả lịch hẹn
            return View();
        }

        // GET: /Receptionist/Receptionist/CreateAppointment
        // Tạo lịch hẹn mới (đặt lịch hộ bệnh nhân)
        [HttpGet]
        public IActionResult CreateAppointment()
        {
            return View();
        }

        // POST: /Receptionist/Receptionist/CreateAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAppointment(object model) // TODO: thay bằng ViewModel thực tế
        {
            if (!ModelState.IsValid)
                return View(model);

            // TODO: Lưu lịch hẹn vào DB
            TempData["SuccessMessage"] = "Đã tạo lịch hẹn thành công.";
            return RedirectToAction(nameof(Appointments));
        }
    }
}