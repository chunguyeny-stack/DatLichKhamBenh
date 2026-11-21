using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Models.ViewModels;
using DatLichKhamBenh.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]           // ← Áp dụng cho toàn controller
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthService _authService;

    public AdminController(UserManager<ApplicationUser> userManager, IAuthService authService)
    {
        _userManager = userManager;
        _authService = authService;
    }

    // ---------- LOCK USER ----------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LockUser(LockUserViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var adminId = _userManager.GetUserId(User);
        if (model.Id == adminId)
        {
            ModelState.AddModelError("", "Không thể tự khóa chính mình.");
            return View(model);
        }
        var result = await _authService.LockUserAsync(adminId!, model);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Users));
    }

    // ---------- UNLOCK USER ----------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnlockUser(string id)
    {
        var result = await _authService.UnlockUserAsync(_userManager.GetUserId(User)!, id);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message);
            return RedirectToAction(nameof(Users));
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.ToListAsync();
        return View(users);
    }
}