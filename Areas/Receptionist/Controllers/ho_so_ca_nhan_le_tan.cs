using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatLichKhamBenh.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    [Authorize(Roles = "Receptionist")]
    public class ho_so_ca_nhan_le_tan : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Profile", "Account", new { area = "" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile()
        {
            return RedirectToAction("Profile", "Account", new { area = "" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword()
        {
            return RedirectToAction("ChangePassword", "Account", new { area = "" });
        }
    }
}
