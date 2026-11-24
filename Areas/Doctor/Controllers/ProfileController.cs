using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatLichKhamBenh.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = "Doctor")]
    public class ProfileController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Profile", "Account", new { area = "" });
        }
    }
}
