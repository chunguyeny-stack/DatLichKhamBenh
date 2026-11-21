using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Data;
using DatLichKhamBenh.Services.Interfaces;
using System.Text.Encodings.Web;

namespace DatLichKhamBenh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;
        private readonly ILogger<UserManagementController> _logger;
        private readonly IEmailService _emailService;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IConfiguration configuration,
            IAuthService authService,
            ILogger<UserManagementController> logger,
            IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
            _authService = authService;
            _logger = logger;
            _emailService = emailService;
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

            if (!CreateStaffViewModel.AllowedRoles.Contains(model.Role, StringComparer.Ordinal))
            {
                ModelState.AddModelError("Role", "Vai trò không hợp lệ.");
                ViewBag.Specialties = await _context.Specialties.Where(s => s.IsActive).ToListAsync();
                return View(model);
            }

            if (model.Role == "Doctor"
                && !await _context.Specialties.AnyAsync(s => s.Id == model.SpecialtyId && s.IsActive))
            {
                ModelState.AddModelError("SpecialtyId", "Vui lòng chọn chuyên khoa đang hoạt động.");
                ViewBag.Specialties = await _context.Specialties.Where(s => s.IsActive).ToListAsync();
                return View(model);
            }

            var initialPassword = _configuration["Seed:StaffPassword"];
            if (string.IsNullOrWhiteSpace(initialPassword))
            {
                ModelState.AddModelError(string.Empty,
                    "Chưa cấu hình Seed:StaffPassword. Hãy đặt mật khẩu ban đầu qua User Secrets hoặc biến môi trường.");
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

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var result = await _userManager.CreateAsync(user, initialPassword);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                ViewBag.Specialties = await _context.Specialties.Where(s => s.IsActive).ToListAsync();
                return View(model);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                foreach (var error in roleResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                ViewBag.Specialties = await _context.Specialties.Where(s => s.IsActive).ToListAsync();
                return View(model);
            }

            if (model.Role == "Doctor")
            {
                _context.Doctors.Add(new DatLichKhamBenh.Models.Entities.Doctor
                {
                    UserId = user.Id,
                    SpecialtyId = model.SpecialtyId!.Value,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            var encodedEmail = HtmlEncoder.Default.Encode(user.Email!);
            var encodedPassword = HtmlEncoder.Default.Encode(initialPassword);
            var emailSent = await _emailService.SendAsync(
                user.Email!,
                "Tài khoản nhân sự MediSlot",
                $"<p>Tài khoản của bạn đã được tạo.</p><p>Email: <strong>{encodedEmail}</strong></p><p>Mật khẩu khởi tạo: <strong>{encodedPassword}</strong></p><p>Bạn phải đổi mật khẩu ở lần đăng nhập đầu tiên.</p>");

            await _authService.LogActionAsync(
                _userManager.GetUserId(User)!,
                "CreateStaff",
                user.Id,
                $"Tạo tài khoản vai trò {model.Role}");

            if (emailSent)
                TempData["SuccessMessage"] = $"Đã tạo tài khoản {model.Role} và gửi email hướng dẫn.";
            else
                TempData["WarningMessage"] = $"Đã tạo tài khoản {model.Role}, nhưng gửi email thất bại. Hãy thông báo mật khẩu khởi tạo qua kênh bảo mật.";

            return RedirectToAction(nameof(Index));
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

            var result = await _authService.LockUserAsync(_userManager.GetUserId(User)!, model);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // 4. MỞ KHÓA TÀI KHOẢN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return BadRequest();

            var result = await _authService.UnlockUserAsync(_userManager.GetUserId(User)!, userId);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return RedirectToAction(nameof(Index));
            }
            TempData["SuccessMessage"] = result.Message;
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

            var defaultPassword = GenerateTemporaryPassword();

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
                await _userManager.UpdateSecurityStampAsync(user);
                await _authService.LogActionAsync(
                    _userManager.GetUserId(User)!, "ResetPassword", user.Id,
                    "Admin đặt lại mật khẩu tạm thời");
                await _emailService.SendAsync(
                    user.Email!,
                    "Mật khẩu MediSlot đã được đặt lại",
                    $"<p>Mật khẩu tạm thời mới: <strong>{defaultPassword}</strong></p><p>Bạn phải đổi mật khẩu ở lần đăng nhập tiếp theo.</p>");

                TempData["SuccessMessage"] = $"Đã reset mật khẩu cho tài khoản {user.Email}. Mật khẩu tạm thời đã được gửi qua email.";
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

        private static string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";
            return "Med!" + new string(Enumerable.Range(0, 12)
                .Select(_ => chars[Random.Shared.Next(chars.Length)])
                .ToArray());
        }
    }
}