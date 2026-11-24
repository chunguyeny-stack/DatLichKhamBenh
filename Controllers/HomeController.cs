using System.Linq;
using System.Threading.Tasks;
using DatLichKhamBenh.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Controllers
{
    [AllowAnonymous]
    public class DoctorController(ApplicationDbContext context) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(string? tenBacSi, int? specialtyId)
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

            if (specialtyId is > 0)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Dashboard", "Admin", new { area = "Admin" });
                if (User.IsInRole("Doctor"))
                    return RedirectToAction("Index", "Doctor", new { area = "Doctor" });
                if (User.IsInRole("Receptionist"))
                    return RedirectToAction("Index", "Receptionist", new { area = "Receptionist" });
            }

            if (!string.IsNullOrWhiteSpace(tenBacSi))
            {
                var keyword = tenBacSi.Trim().ToLower();
                query = query.Where(b =>
                    b.HoTen.ToLower().Contains(keyword) ||
                    (b.HocVi != null && b.HocVi.ToLower().Contains(keyword)));
            }

            var doctors = await query
                .OrderByDescending(b => b.Rating)
                .ToListAsync();

            return View(doctors);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var doctor = await context.BacSis
                .AsNoTracking()
                .Include(b => b.ChuyenKhoa)
                .Include(b => b.ApplicationUser)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }
    }
}
