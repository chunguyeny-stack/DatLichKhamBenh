using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // =====================================================
        // 1. ĐĂNG NHẬP (Xử lý khóa tài khoản + Ép đổi MK lần đầu)
        // =====================================================

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            // 🔥 BƯỚC 1: Kiểm tra tài khoản có bị Admin khóa không (LockoutEnd > Now)
            if (user != null && user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now)
            {
                ModelState.AddModelError(string.Empty,
                    $"Tài khoản đã bị khóa. Lý do: {user.LockoutReason ?? "Không có lý do cụ thể"}");
                return View(model);
            }

            // 🔥 BƯỚC 2: Xác thực mật khẩu
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Người dùng {model.Email} đăng nhập thành công.");

                // 🔥 BƯỚC 3: Nếu là lần đầu đăng nhập (Admin tạo TK) → Bắt buộc đổi MK
                if (user != null && user.MustChangePassword)
                {
                    // Chuyển sang trang đổi mật khẩu (không cho vào Dashboard)
                    return RedirectToAction(nameof(ChangePassword));
                }

                // 🔥 BƯỚC 4: Điều hướng theo Role
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                        return RedirectToAction("Index", "UserManagement", new { area = "Admin" });
                    if (await _userManager.IsInRoleAsync(user, "Doctor"))
                        return RedirectToAction("Index", "Doctor", new { area = "Doctor" });
                    if (await _userManager.IsInRoleAsync(user, "Receptionist"))
                        return RedirectToAction("Index", "Receptionist", new { area = "Receptionist" });
                }

                // Nếu có returnUrl hợp lệ (trường hợp bị redirect do chưa login)
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            // Trường hợp đăng nhập sai quá số lần -> bị khóa tạm thời (15 phút)
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty,
                    "Tài khoản đã bị khóa tạm thời do đăng nhập sai quá nhiều lần. Vui lòng thử lại sau 15 phút.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            }

            return View(model);
        }

        // =====================================================
        // 2. ĐĂNG KÝ (Dành cho Bệnh nhân)
        // =====================================================

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true,
                MustChangePassword = false // Bệnh nhân tự đăng ký → không ép đổi MK
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Patient");
                _logger.LogInformation($"Bệnh nhân mới đăng ký: {model.Email}");

                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["SuccessMessage"] = "Đăng ký thành công! Chào mừng bạn đến với MediSlot.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // =====================================================
        // 3. ĐĂNG XUẤT
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Người dùng đã đăng xuất.");
            return RedirectToAction("Index", "Home");
        }

        // =====================================================
        // 4. TRANG TỪ CHỐI TRUY CẬP
        // =====================================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // =====================================================
        // 5. THÔNG TIN CÁ NHÂN
        // =====================================================

        // GET: /Account/Profile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var roles = await _userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault() switch
            {
                "Admin" => "Quản trị viên",
                "Doctor" => "Bác sĩ",
                "Receptionist" => "Lễ tân",
                "Patient" => "Bệnh nhân",
                _ => "Người dùng"
            };

            var model = new ProfileViewModel
            {
                MiddleName = user.MiddleName ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                RoleName = roleName,
                DateOfBirth = user.DateOfBirth
            };

            return View(model);
        }

        // POST: /Account/UpdateProfile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            // Chỉ cập nhật các trường được phép (không cho đổi Email/Role)
            user.MiddleName = model.MiddleName;
            user.FirstName = model.FirstName;
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công.";
            }
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View("Profile", model);
            }

            return RedirectToAction(nameof(Profile));
        }

        // =====================================================
        // 6. ĐỔI MẬT KHẨU (Có validate bảo mật)
        // =====================================================

        // GET: /Account/ChangePassword
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            // 🔥 BƯỚC QUAN TRỌNG: Nếu user đang bị ép đổi MK lần đầu,
            // không được phép đặt lại y nguyên mật khẩu mặc định (MedBooking@123)
            // vì như vậy sẽ vô hiệu hóa tính bảo mật của cơ chế "MustChangePassword"
            if (user.MustChangePassword && model.NewPassword == "MedBooking@123")
            {
                ModelState.AddModelError("NewPassword",
                    "Không được dùng lại mật khẩu mặc định. Vui lòng chọn mật khẩu khác.");
                return View(model);
            }

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            // 🔥 Tắt cờ MustChangePassword sau khi đổi thành công
            if (user.MustChangePassword)
            {
                user.MustChangePassword = false;
                await _userManager.UpdateAsync(user);
            }

            // Refresh lại cookie đăng nhập để tránh bị logout
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction(nameof(Profile));
        }

        // =====================================================
        // 7. ĐĂNG NHẬP BẰNG GOOGLE (External Login)
        // =====================================================

        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        // GET: /Account/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi từ nhà cung cấp: {remoteError}");
                return View("Login");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction(nameof(Login));

            // Thử đăng nhập nếu tài khoản đã liên kết Google
            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            // Nếu chưa có, tạo tài khoản mới với role Patient
            var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Không lấy được email từ Google.");
                return View("Login");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "Người dùng",
                    MiddleName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value,
                    MustChangePassword = false
                };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    foreach (var e in createResult.Errors)
                        ModelState.AddModelError(string.Empty, e.Description);
                    return View("Login");
                }
                await _userManager.AddToRoleAsync(user, "Patient");
            }

            await _userManager.AddLoginAsync(user, info);
            await _signInManager.SignInAsync(user, isPersistent: false);

            TempData["SuccessMessage"] = "Đăng nhập bằng Google thành công!";
            return RedirectToAction("Index", "Home");
        }
    }
}