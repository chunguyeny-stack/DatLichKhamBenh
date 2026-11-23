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
            ViewBag.TenBacSi = tenBacSi;
            ViewBag.SpecialtyId = specialtyId;
            ViewBag.Specialties = await context.ChuyenKhoas
                .AsNoTracking()
                .Where(ck => ck.TrangThai == "active" || ck.TrangThai == null)
                .OrderBy(ck => ck.TenChuyenKhoa)
                .ToListAsync();

            var query = context.BacSis
                .AsNoTracking()
                .Include(b => b.ChuyenKhoa)
                .Include(b => b.ApplicationUser)
                .AsQueryable();

            if (specialtyId is > 0)
            {
                query = query.Where(b => b.ChuyenKhoaId == specialtyId.Value);
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
