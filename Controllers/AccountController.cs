using System.Security.Claims;
using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Models.ViewModels;
using DatLichKhamBenh.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

[Authorize]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly IAuthService _authService;
    private readonly ILogger<AccountController> _logger;
    private readonly IConfiguration _configuration;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context,
        IAuthService authService,
        ILogger<AccountController> logger,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _authService = authService;
        _logger = logger;
        _configuration = configuration;
    }

    // ---------- LOGIN ----------
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]     // ← Rate limit
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var result = await _authService.LoginAsync(model);
        if (result.Success)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && user.MustChangePassword)
                return RedirectToAction(nameof(ChangePassword));
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);
            return await RedirectToRoleDashboardAsync(user);
        }
        ModelState.AddModelError("", result.Message);
        return View(model);
    }

    // ---------- CHANGE PASSWORD ----------
    [HttpGet]
    public async Task<IActionResult> ChangePassword()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        ViewBag.IsFirstLogin = user.MustChangePassword;
        return View(new ChangePasswordViewModel());
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        var roles = await _userManager.GetRolesAsync(user);
        var model = new ProfileViewModel
        {
            FirstName = user.FirstName ?? string.Empty,
            MiddleName = user.MiddleName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            RoleName = roles.FirstOrDefault() ?? string.Empty
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        model.Email = user.Email ?? string.Empty;
        var roles = await _userManager.GetRolesAsync(user);
        model.RoleName = roles.FirstOrDefault() ?? string.Empty;

        if (!ModelState.IsValid)
            return View("Profile", model);

        user.MiddleName = model.MiddleName.Trim();
        user.FirstName = model.FirstName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();
        user.DateOfBirth = model.DateOfBirth;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View("Profile", model);
        }

        await _userManager.UpdateSecurityStampAsync(user);
        TempData["Success"] = "Cập nhật thông tin thành công.";
        return RedirectToAction(nameof(Profile));
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return RedirectToAction(nameof(Register));

        ViewBag.Email = email;
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null && user.EmailConfirmed)
        {
            TempData["Success"] = "Email đã được xác thực.";
            return RedirectToAction(nameof(Login));
        }
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmEmail(string email, string otp)
    {
        var result = await _authService.VerifyEmailOtpAsync(email, otp);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            ViewBag.Email = email;
            return View();
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp(string email)
    {
        var result = await _authService.ResendEmailOtpAsync(email);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(ConfirmEmail), new { email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
        {
            ViewBag.IsFirstLogin = user.MustChangePassword;
            return View(model);
        }

        var result = await _authService.ChangePasswordAsync(user.Id, model);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message);
            ViewBag.IsFirstLogin = user.MustChangePassword;
            return View(model);
        }

        TempData["Success"] = "Đổi mật khẩu thành công.";
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _authService.RegisterAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message);
            return View(model);
        }
        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(ConfirmEmail), new { email = model.Email });
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied() => View();

    // ---------- FORGOT PASSWORD ----------
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("forgot")]     // ← Rate limit
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action(
                    nameof(ResetPassword),
                    "Account",
                    new { userId = user.Id, token },
                    Request.Scheme);
                await HttpContext.RequestServices
                    .GetRequiredService<DatLichKhamBenh.Services.Interfaces.IEmailService>()
                    .SendAsync(email, "Đặt lại mật khẩu MediSlot",
                        $"<p>Nhấn <a href=\"{callbackUrl}\">vào đây</a> để đặt lại mật khẩu.</p>");
            }
        }

        TempData["SuccessMessage"] =
            "Nếu email tồn tại, liên kết đặt lại mật khẩu đã được gửi.";
        return RedirectToAction(nameof(ForgotPassword));
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ResetPassword(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            return RedirectToAction(nameof(Login));

        ViewBag.UserId = userId;
        ViewBag.Token = token;
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return RedirectToAction(nameof(Login));
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string userId, string token, string password, string confirmPassword)
    {
        if (password != confirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Mật khẩu xác nhận không khớp.");
            ViewBag.UserId = userId;
            ViewBag.Token = token;
            return View();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return RedirectToAction(nameof(Login));

        var result = await _userManager.ResetPasswordAsync(user, token, password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            ViewBag.UserId = userId;
            ViewBag.Token = token;
            return View();
        }

        user.MustChangePassword = true;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            ViewBag.UserId = userId;
            ViewBag.Token = token;
            return View();
        }
        await _userManager.UpdateSecurityStampAsync(user);
        TempData["Success"] = "Đặt lại mật khẩu thành công.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData["Success"] = "Bạn đã đăng xuất.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLogin(string provider, string? returnUrl = null)
    {
        if (!string.Equals(provider, "Google", StringComparison.Ordinal))
        {
            TempData["Error"] = "Nhà cung cấp đăng nhập không hợp lệ.";
            return RedirectToAction(nameof(Login));
        }

        if (string.IsNullOrWhiteSpace(_configuration["Authentication:Google:ClientId"])
            || string.IsNullOrWhiteSpace(_configuration["Authentication:Google:ClientSecret"]))
        {
            TempData["Error"] = "Tính năng đăng nhập bằng Google chưa được cấu hình.";
            return RedirectToAction(nameof(Login));
        }

        try
        {
            var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
            if (!schemes.Any(scheme => scheme.Name == provider))
            {
                TempData["Error"] = "Tính năng đăng nhập bằng Google chưa được cấu hình.";
                return RedirectToAction(nameof(Login));
            }

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể bắt đầu đăng nhập ngoài bằng {Provider}.", provider);
            TempData["Error"] = "Không thể bắt đầu đăng nhập bằng Google. Vui lòng thử lại sau.";
            return RedirectToAction(nameof(Login));
        }
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(
        string? returnUrl = null,
        string? remoteError = null,
        string? error = null)
    {
        var authenticationError = remoteError ?? error;
        if (!string.IsNullOrWhiteSpace(authenticationError))
        {
            _logger.LogWarning("Đăng nhập Google thất bại: {RemoteError}", authenticationError);
            TempData["Error"] = "Đăng nhập bằng Google không thành công. Vui lòng thử lại.";
            return RedirectToAction(nameof(Login));
        }

        try
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                TempData["Error"] = "Không nhận được thông tin đăng nhập từ Google. Vui lòng thử lại.";
                return RedirectToAction(nameof(Login));
            }
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var emailVerified = info.Principal.FindFirstValue("email_verified");
            if (string.IsNullOrWhiteSpace(email)
                || !bool.TryParse(emailVerified, out var isEmailVerified)
                || !isEmailVerified)
            {
                TempData["Error"] = "Google chưa xác minh địa chỉ email. Hãy dùng tài khoản Google có email đã xác minh.";
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user != null)
            {
                if (!user.EmailConfirmed
                    && string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    user.EmailConfirmed = true;
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                        return ExternalLoginFailure("Không thể xác minh email cho tài khoản hiện có.", updateResult.Errors);
                }

                return await SignInExternalUserAsync(user, info.LoginProvider, info.ProviderKey, returnUrl);
            }

            user = await _userManager.FindByEmailAsync(email);
            var isNewUser = user == null;
            if (user != null && await _userManager.IsLockedOutAsync(user))
            {
                TempData["Error"] = "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.";
                return RedirectToAction(nameof(Login));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            if (user == null)
            {
                var googleName = info.Principal.FindFirstValue(ClaimTypes.Name);
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName)
                        ?? googleName
                        ?? email,
                    MiddleName = info.Principal.FindFirstValue(ClaimTypes.Surname),
                    LockoutEnabled = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    return ExternalLoginFailure("Không thể tạo tài khoản MediSlot từ Google.", createResult.Errors);

                var roleResult = await _userManager.AddToRoleAsync(user, "Patient");
                if (!roleResult.Succeeded)
                    return ExternalLoginFailure("Không thể gán vai trò Bệnh nhân cho tài khoản Google.", roleResult.Errors);

                _context.Patients.Add(new Patient
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                });
                await _context.SaveChangesAsync();
            }
            else if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    return ExternalLoginFailure("Không thể xác minh email cho tài khoản hiện có.", updateResult.Errors);
            }

            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
                return ExternalLoginFailure("Không thể liên kết tài khoản Google với MediSlot.", addLoginResult.Errors);

            await transaction.CommitAsync();

            _logger.LogInformation(
                "Tài khoản {UserId} đã được {Action} bằng Google.",
                user.Id,
                isNewUser ? "tạo mới" : "liên kết");
            return await SignInExternalUserAsync(user, info.LoginProvider, info.ProviderKey, returnUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xử lý callback đăng nhập Google.");
            TempData["Error"] = "Không thể hoàn tất đăng nhập bằng Google. Vui lòng thử lại sau.";
            return RedirectToAction(nameof(Login));
        }
    }

    private async Task<IActionResult> SignInExternalUserAsync(
        ApplicationUser user,
        string loginProvider,
        string providerKey,
        string? returnUrl)
    {
        if (await _userManager.IsLockedOutAsync(user))
        {
            TempData["Error"] = "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.";
            return RedirectToAction(nameof(Login));
        }

        var result = await _signInManager.ExternalLoginSignInAsync(
            loginProvider,
            providerKey,
            isPersistent: false,
            bypassTwoFactor: false);
        if (result.IsLockedOut)
        {
            TempData["Error"] = "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.";
            return RedirectToAction(nameof(Login));
        }
        if (result.RequiresTwoFactor)
        {
            await HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            TempData["Error"] = "Tài khoản bật xác thực hai bước nhưng ứng dụng chưa hỗ trợ bước xác minh này.";
            return RedirectToAction(nameof(Login));
        }
        if (!result.Succeeded)
        {
            TempData["Error"] = "Tài khoản hiện không được phép đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

        if (user.MustChangePassword)
            return RedirectToAction(nameof(ChangePassword));
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return await RedirectToRoleDashboardAsync(user);
    }

    private IActionResult ExternalLoginFailure(
        string message,
        IEnumerable<IdentityError> errors)
    {
        _logger.LogError(
            "Lỗi tạo/liên kết tài khoản Google: {Errors}",
            string.Join("; ", errors.Select(error => error.Description)));
        TempData["Error"] = message;
        return RedirectToAction(nameof(Login));
    }

    private async Task<IActionResult> RedirectToRoleDashboardAsync(ApplicationUser? user)
    {
        if (user == null)
            return RedirectToAction(nameof(Login));

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Admin"))
            return RedirectToAction("Index", "UserManagement", new { area = "Admin" });
        if (roles.Contains("Doctor"))
            return RedirectToAction("Index", "Doctor", new { area = "Doctor" });
        if (roles.Contains("Receptionist"))
            return RedirectToAction("Index", "Receptionist", new { area = "Receptionist" });

        return RedirectToAction("Index", "Home");
    }
}