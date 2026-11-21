using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Models.ViewModels;
using DatLichKhamBenh.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;

[Authorize]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IAuthService _authService;
    private readonly ILogger<AccountController> _logger;
    private readonly IConfiguration _configuration;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IAuthService authService,
        ILogger<AccountController> logger,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
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

        user.MustChangePassword = false;
        await _userManager.UpdateAsync(user);
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
                TempData["Error"] = "Không nhận được thông tin đăng nhập từ Google. Vui lòng thử lại.";
                return RedirectToAction(nameof(Login));
            }

            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (user?.MustChangePassword == true)
                    return RedirectToAction(nameof(ChangePassword));

                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);
                return await RedirectToRoleDashboardAsync(user);
            }

            TempData["Error"] = result.IsLockedOut
                ? "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên."
                : "Tài khoản Google chưa được liên kết với tài khoản MediSlot.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xử lý callback đăng nhập Google.");
            TempData["Error"] = "Không thể hoàn tất đăng nhập bằng Google. Vui lòng thử lại sau.";
            return RedirectToAction(nameof(Login));
        }
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