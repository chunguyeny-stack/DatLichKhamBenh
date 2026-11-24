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
        // Trang 1: Tìm kiếm BS -> Lọc theo Tên, Chuyên khoa
        [HttpGet]
        public async Task<IActionResult> Index(string? tenBacSi, int? specialtyId)
        {
            var query = context.Doctors
                .AsNoTracking()
                .Include(d => d.Specialty)
                .Include(d => d.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tenBacSi))
            {
                var keyword = tenBacSi.Trim();
                query = query.Where(d =>
                    EF.Functions.Like(d.FullName, $"%{keyword}%") ||
                    (d.Degree != null && EF.Functions.Like(d.Degree, $"%{keyword}%")) ||
                    (d.Specialty != null && EF.Functions.Like(d.Specialty.Name, $"%{keyword}%")));
            }

            if (specialtyId is > 0)
            {
                query = query.Where(d => d.SpecialtyId == specialtyId.Value);
            }

            query = query.Where(d => d.IsActive);

            ViewBag.Specialties = await context.Specialties
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
            ViewBag.TenBacSi = tenBacSi;
            ViewBag.SpecialtyId = specialtyId;

            var doctors = await query
                .OrderBy(d => d.FullName)
                .ToListAsync();

            return View(doctors);
        }

        // Trang 2: Chi tiết BS -> Xem thông tin -> Đặt lịch ngay
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var doctor = await context.Doctors
                .AsNoTracking()
                .Include(d => d.Specialty)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);

            if (doctor == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin bác sĩ.";
                return RedirectToAction(nameof(Index));
            }

            return View(doctor);
        }
    }
}
