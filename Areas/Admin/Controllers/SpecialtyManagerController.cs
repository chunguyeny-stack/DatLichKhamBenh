
using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SpecialtyManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SpecialtyManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _context.Specialties
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Specialty model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Name = model.Name.Trim();

            bool exists = await _context.Specialties
                .AnyAsync(s => s.Name.ToLower() == model.Name.ToLower());

            if (exists)
            {
                ModelState.AddModelError("Name", "Chuyên khoa đã tồn tại.");
                return View(model);
            }

            _context.Specialties.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thêm chuyên khoa thành công.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var specialty = await _context.Specialties.FindAsync(id);

            if (specialty == null)
                return NotFound();

            return View(specialty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Specialty model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var specialty = await _context.Specialties.FindAsync(id);

            if (specialty == null)
                return NotFound();

            specialty.Name = model.Name.Trim();
            specialty.Description = model.Description;
            specialty.CommonSymptoms = model.CommonSymptoms;
            specialty.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật chuyên khoa thành công.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var specialty = await _context.Specialties.FindAsync(id);

            if (specialty == null)
                return NotFound();

            specialty.IsActive = !specialty.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật trạng thái chuyên khoa.";

            return RedirectToAction(nameof(Index));
        }
    }
}
