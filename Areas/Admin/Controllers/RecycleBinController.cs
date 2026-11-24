using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;

namespace DatLichKhamBenh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RecycleBinController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var deletedUsers = await userManager.Users
                .Where(u => !u.IsActive)
                .ToListAsync();
            var deletedSpecialties = await context.Specialties
                .Where(s => !s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            ViewBag.DeletedUsers = deletedUsers;
            ViewBag.DeletedSpecialties = deletedSpecialties;
            return View();
        }
    }
}
