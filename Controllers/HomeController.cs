using DatLichKhamBenh.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Controllers
{
    [Authorize]
    public class HomeController(ApplicationDbContext context) : Controller
    {
        // GET: / (Trang chủ)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            ViewBag.Specialties = await context.ChuyenKhoas
                .AsNoTracking()
                .Where(ck => ck.TrangThai == "active" || string.IsNullOrEmpty(ck.TrangThai))
                .OrderBy(ck => ck.TenChuyenKhoa)
                .ToListAsync();

            ViewBag.FeaturedDoctors = await context.BacSis
                .AsNoTracking()
                .Include(b => b.ChuyenKhoa)
                .Include(b => b.ApplicationUser)
                .OrderByDescending(b => b.Rating)
                .ThenByDescending(b => b.SoLuongDanhGia)
                .Take(3)
                .ToListAsync();

            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Index", "UserManagement", new { area = "Admin" });
                if (User.IsInRole("Doctor"))
                    return RedirectToAction("Index", "Doctor", new { area = "Doctor" });
                if (User.IsInRole("Receptionist"))
                    return RedirectToAction("Index", "Receptionist", new { area = "Receptionist" });
            }

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
