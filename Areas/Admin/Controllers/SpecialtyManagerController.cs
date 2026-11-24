using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;

namespace DatLichKhamBenh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SpecialtyManagerController(ApplicationDbContext context) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var specialties = await context.Specialties
                .Include(s => s.Doctors)
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
            return View(specialties);
        }

        [HttpGet]
        public IActionResult Create() => View(new Specialty());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Specialty model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Name = model.Name.Trim();
            if (await context.Specialties.AnyAsync(s => s.Name.ToLower() == model.Name.ToLower()))
            {
                ModelState.AddModelError(nameof(model.Name), "Chuyên khoa đã tồn tại.");
                return View(model);
            }

            model.IsActive = true;
            context.Specialties.Add(model);
            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã thêm chuyên khoa.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var specialty = await context.Specialties.FindAsync(id);
            return specialty == null ? NotFound() : View(specialty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Specialty model)
        {
            var specialty = await context.Specialties.FindAsync(id);
            if (specialty == null)
                return NotFound();
            if (!ModelState.IsValid)
                return View(model);

            specialty.Name = model.Name.Trim();
            specialty.Description = model.Description;
            specialty.CommonSymptoms = model.CommonSymptoms;
            specialty.IsActive = model.IsActive;
            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật chuyên khoa.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var specialty = await context.Specialties.FindAsync(id);
            if (specialty == null)
                return NotFound();

            specialty.IsActive = false;
            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã chuyển chuyên khoa vào thùng rác.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var specialty = await context.Specialties.FindAsync(id);
            if (specialty == null)
                return NotFound();
            specialty.IsActive = true;
            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã khôi phục chuyên khoa.";
            return RedirectToAction("Index", "RecycleBin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentlyDelete(int id)
        {
            var specialty = await context.Specialties
                .Include(s => s.Doctors)
                .Include(s => s.Services)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (specialty == null)
                return NotFound();
            if (specialty.Doctors.Count != 0 || specialty.Services.Count != 0)
            {
                TempData["WarningMessage"] = "Không thể xóa vĩnh viễn chuyên khoa đang có bác sĩ hoặc dịch vụ.";
                return RedirectToAction("Index", "RecycleBin");
            }

            context.Specialties.Remove(specialty);
            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã xóa vĩnh viễn chuyên khoa.";
            return RedirectToAction("Index", "RecycleBin");
        }
    }
}