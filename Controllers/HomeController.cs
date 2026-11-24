using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Controllers
{
    [Authorize]
    public class HomeController(ApplicationDbContext context) : Controller
    {
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
                    return RedirectToAction("Index", "dashboard_le_tan", new { area = "Receptionist" });
            }

            return View();
        }
    }
}
