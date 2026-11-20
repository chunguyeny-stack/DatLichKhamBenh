using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Data;

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
                MustChangePassword = true, // Bắt buộc đổi MK lần đầu
                LockoutEnabled = true
            };

            const string defaultPassword = "MedBooking@123";
            var result = await _userManager.CreateAsync(user, defaultPassword);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);

                if (model.Role == "Doctor")
                {
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

        // =======================================================
        // 5. RESET MẬT KHẨU (MỚI THÊM THEO YÊU CẦU)
        // =======================================================
        
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.Email = user.Email;
            ViewBag.UserId = user.Id;
            ViewBag.FullName = $"{user.MiddleName} {user.FirstName}".Trim();
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId, string confirmAction)
        {
            if (string.IsNullOrEmpty(userId)) return NotFound();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Mật khẩu mặc định khi reset
            const string defaultPassword = "MedBooking@123";

            // 1. Tạo token và reset mật khẩu
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, defaultPassword);

            if (result.Succeeded)
            {
                // 2. 🔥 BẬT CỜ BẮT BUỘC ĐỔI MẬT KHẨU 🔥
                user.MustChangePassword = true;

                // 3. Mở khóa tài khoản (nếu đang bị khóa) để họ có thể đăng nhập lại
                await _userManager.SetLockoutEndDateAsync(user, null);
                user.LockoutReason = null;

                await _userManager.UpdateAsync(user);

                TempData["SuccessMessage"] = $"Đã reset mật khẩu cho tài khoản {user.Email}. Mật khẩu mới: {defaultPassword}. Người dùng sẽ phải đổi mật khẩu khi đăng nhập.";
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                ViewBag.Email = user.Email;
                ViewBag.UserId = user.Id;
                ViewBag.FullName = $"{user.MiddleName} {user.FirstName}".Trim();
                return View();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}