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
            ViewBag.Specialties = await context.Specialties
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            ViewBag.FeaturedDoctors = await context.Doctors
                .AsNoTracking()
                .Include(d => d.Specialty)
                .Include(d => d.User)
                .Where(d => d.IsActive)
                .OrderByDescending(d => d.YearsOfExperience)
                .ThenBy(d => d.FullName)
                .Take(4)
                .ToListAsync();

            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Dashboard", "Admin", new { area = "Admin" });
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
