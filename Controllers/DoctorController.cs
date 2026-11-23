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
            var query = context.BacSis
                .AsNoTracking()
                .Include(b => b.ChuyenKhoa)
                .Include(b => b.ApplicationUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tenBacSi))
            {
                var keyword = tenBacSi.Trim();
                query = query.Where(b =>
                    EF.Functions.Like(b.HoTen, $"%{keyword}%") ||
                    (b.HocVi != null && EF.Functions.Like(b.HocVi, $"%{keyword}%")) ||
                    (b.ChuyenKhoa != null && EF.Functions.Like(b.ChuyenKhoa.TenChuyenKhoa, $"%{keyword}%")));
            }

            if (specialtyId is > 0)
            {
                query = query.Where(b => b.ChuyenKhoaId == specialtyId.Value);
            }

            ViewBag.ChuyenKhoas = await context.ChuyenKhoas
                .AsNoTracking()
                .Where(ck => ck.TrangThai == "active")
                .OrderBy(ck => ck.TenChuyenKhoa)
                .ToListAsync();
            ViewBag.TenBacSi = tenBacSi;
            ViewBag.SpecialtyId = specialtyId;

            var doctors = await query
                .OrderByDescending(b => b.Rating)
                .ThenBy(b => b.HoTen)
                .ToListAsync();

            return View(doctors);
        }

        // Trang 2: Chi tiết BS -> Xem thông tin -> Đặt lịch ngay
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
                TempData["Error"] = "Không tìm thấy thông tin bác sĩ.";
                return RedirectToAction(nameof(Index));
            }

            return View(doctor);
        }
    }
}
