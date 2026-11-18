using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using DatLichKhamBenh.Models.Entities;

namespace DatLichKhamBenh.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: / (Trang chủ)
        [HttpGet]
        public IActionResult Index()
        {
            // Nếu đã đăng nhập, điều hướng về đúng dashboard theo role
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Index", "UserManagement", new { area = "Admin" });
                if (User.IsInRole("Doctor"))
                    return RedirectToAction("Index", "Doctor", new { area = "Doctor" });
                if (User.IsInRole("Receptionist"))
                    return RedirectToAction("Index", "Receptionist", new { area = "Receptionist" });
            }

            // Chưa đăng nhập → hiển thị trang chủ giới thiệu
            return View();
        }

        // GET: /Home/Privacy
        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /Home/Error
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}