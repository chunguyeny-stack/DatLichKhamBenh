using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Data; // Namespace chứa ApplicationDbContext

namespace DatLichKhamBenh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<UserManagementController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        // 1. DANH SÁCH NGƯỜI DÙNG
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, string>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "Chưa xác định";
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        // 2. TẠO TÀI KHOẢN NHÂN SỰ
        [HttpGet]
        public async Task<IActionResult> CreateStaff()
        {
            ViewBag.Specialties = await _context.Specialties.Where(s => s.IsActive).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStaff(CreateStaffViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Specialties = await _context.Specialties.Where(s => s.IsActive).ToListAsync();
                return View(model);
            }

            if (model.Role != "Doctor" && model.Role != "Receptionist")
            {
                ModelState.AddModelError("Role", "Vai trò không hợp lệ.");
                ViewBag.Specialties = await _context.Specialties.Where(s => s.IsActive).ToListAsync();
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                ViewBag.Specialties = await _context.Specialties.Where(s => s.IsActive).ToListAsync();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                EmailConfirmed = true,
                MustChangePassword = true,
                LockoutEnabled = true
            };

            const string defaultPassword = "MedBooking@123";
            var result = await _userManager.CreateAsync(user, defaultPassword);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);

                if (model.Role == "Doctor")
                {
                    // 🔥 ĐÃ SỬA TẠI ĐÂY: Thêm namespace đầy đủ để phân biệt với thư mục Areas/Doctor
                    _context.Doctors.Add(new DatLichKhamBenh.Models.Entities.Doctor
                    {
                        UserId = user.Id,
                        SpecialtyId = model.SpecialtyId,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    });
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"Đã tạo tài khoản {model.Role}. Mật khẩu: {defaultPassword}";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            ViewBag.Specialties = await _context.Specialties.Where(s => s.IsActive).ToListAsync();
            return View(model);
        }

        // 3. KHÓA TÀI KHOẢN
        [HttpGet]
        public async Task<IActionResult> LockUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var model = new LockUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FullName = $"{user.MiddleName} {user.FirstName}".Trim(),
                Reason = user.LockoutReason ?? ""
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(LockUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            user.LockoutReason = model.Reason;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = $"Đã khóa tài khoản {user.Email}.";
            return RedirectToAction(nameof(Index));
        }

        // 4. MỞ KHÓA TÀI KHOẢN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return BadRequest();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, null);
            user.LockoutReason = null;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = $"Đã mở khóa tài khoản {user.Email}.";
            return RedirectToAction(nameof(Index));
        }
    }
}