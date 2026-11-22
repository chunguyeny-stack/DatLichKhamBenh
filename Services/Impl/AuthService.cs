using System.Security.Cryptography;
using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Models.ViewModels;
using DatLichKhamBenh.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Services.Impl
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailService = emailService;
        }

        public async Task<(bool Success, string Message)> LoginAsync(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return (false, "Email hoặc mật khẩu không đúng.");

            // Check khóa tài khoản
            if (await _userManager.IsLockedOutAsync(user))
                return (false, "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                await LogActionAsync(user.Id, "Login", user.Id, "Đăng nhập thành công");
                return (true, "Đăng nhập thành công");
            }

            if (result.IsLockedOut)
                return (false, "Tài khoản bị tạm khóa do nhập sai quá nhiều lần.");

            return (false, "Email hoặc mật khẩu không đúng.");
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(string userId, ChangePasswordViewModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, "Không tìm thấy người dùng.");

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return (false, errors);
            }

            // QUAN TRỌNG: Tắt cờ đổi mật khẩu & Rotate Security Stamp
            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);
            await _userManager.UpdateSecurityStampAsync(user); // Vô hiệu hóa cookie cũ

            await _signInManager.RefreshSignInAsync(user); // Cấp lại cookie mới cho user hiện tại
            await LogActionAsync(userId, "ChangePassword", userId, "Đổi mật khẩu thành công");

            return (true, "Đổi mật khẩu thành công.");
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model)
        {
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
                return (false, "Email đã được sử dụng.");

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = model.DateOfBirth,
                EmailConfirmed = false,
                LockoutEnabled = true,
                MustChangePassword = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, "Patient");
            _context.Patients.Add(new Patient
            {
                UserId = user.Id,
                CreatedAt = DateTime.Now,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            var emailSent = await SendEmailOtpAsync(user);
            await LogActionAsync(user.Id, "Register", user.Id, "Đăng ký tài khoản bệnh nhân");
            return emailSent
                ? (true, "Đăng ký thành công. Vui lòng nhập mã OTP đã gửi đến email.")
                : (true, "Tài khoản đã được tạo nhưng chưa gửi được OTP. Vui lòng cấu hình email hoặc chọn Gửi lại OTP.");
        }

        public async Task<(bool Success, string Message)> VerifyEmailOtpAsync(string email, string otp)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
                return (false, "Email và mã OTP không được để trống.");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "Không tìm thấy tài khoản.");

            var token = await _context.EmailVerificationTokens
                .Where(t => t.UserId == user.Id && t.Token == otp && !t.IsUsed)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            if (token == null)
                return (false, "Mã OTP không chính xác.");
            if (token.ExpiredAt <= DateTime.UtcNow)
                return (false, "Mã OTP đã hết hạn.");

            token.IsUsed = true;
            user.EmailConfirmed = true;
            await _context.SaveChangesAsync();
            await _userManager.UpdateAsync(user);
            await LogActionAsync(user.Id, "VerifyEmail", user.Id, "Xác thực email thành công");
            return (true, "Kích hoạt tài khoản thành công.");
        }

        public async Task<(bool Success, string Message)> ResendEmailOtpAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (true, "Nếu tài khoản tồn tại, mã OTP mới đã được gửi.");
            if (user.EmailConfirmed)
                return (false, "Tài khoản đã được xác thực.");

            if (!await SendEmailOtpAsync(user))
                return (false, "Chưa gửi được OTP. Vui lòng kiểm tra cấu hình email và thử lại.");

            await LogActionAsync(user.Id, "ResendOtp", user.Id, "Yêu cầu gửi lại OTP");
            return (true, "Mã OTP mới đã được gửi.");
        }

        private async Task<bool> SendEmailOtpAsync(ApplicationUser user)
        {
            await _context.EmailVerificationTokens
                .Where(t => t.UserId == user.Id && !t.IsUsed)
                .ExecuteUpdateAsync(t => t.SetProperty(x => x.IsUsed, true));

            var otp = RandomNumberGenerator.GetInt32(100000, 1_000_000).ToString();
            _context.EmailVerificationTokens.Add(new EmailVerificationToken
            {
                UserId = user.Id,
                Token = otp,
                ExpiredAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            });
            await _context.SaveChangesAsync();

            return await _emailService.SendAsync(
                user.Email!,
                "Mã xác thực tài khoản MediSlot",
                $"<p>Mã OTP của bạn là <strong>{otp}</strong>.</p><p>Mã có hiệu lực trong 5 phút.</p>");
        }

        public async Task<(bool Success, string Message)> LockUserAsync(string adminId, LockUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return (false, "Không tìm thấy tài khoản.");
            if (user.Id == adminId) return (false, "Không thể tự khóa tài khoản của mình.");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var lockoutEnabled = await _userManager.SetLockoutEnabledAsync(user, true);
            if (!lockoutEnabled.Succeeded)
            {
                await transaction.RollbackAsync();
                return (false, string.Join("; ", lockoutEnabled.Errors.Select(error => error.Description)));
            }

            var lockoutResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            if (!lockoutResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return (false, string.Join("; ", lockoutResult.Errors.Select(error => error.Description)));
            }

            user.LockoutReason = model.Reason;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return (false, string.Join("; ", updateResult.Errors.Select(error => error.Description)));
            }

            var stampResult = await _userManager.UpdateSecurityStampAsync(user);
            if (!stampResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return (false, string.Join("; ", stampResult.Errors.Select(error => error.Description)));
            }

            await LogActionAsync(adminId, "LockUser", user.Id, model.Reason);
            await transaction.CommitAsync();
            return (true, "Đã khóa tài khoản.");
        }

        public async Task<(bool Success, string Message)> UnlockUserAsync(string adminId, string targetUserId)
        {
            var user = await _userManager.FindByIdAsync(targetUserId);
            if (user == null) return (false, "Không tìm thấy tài khoản.");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var lockoutEnabled = await _userManager.SetLockoutEnabledAsync(user, true);
            if (!lockoutEnabled.Succeeded)
            {
                await transaction.RollbackAsync();
                return (false, string.Join("; ", lockoutEnabled.Errors.Select(error => error.Description)));
            }

            var unlockResult = await _userManager.SetLockoutEndDateAsync(user, null);
            if (!unlockResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return (false, string.Join("; ", unlockResult.Errors.Select(error => error.Description)));
            }

            user.LockoutReason = null;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return (false, string.Join("; ", updateResult.Errors.Select(error => error.Description)));
            }

            var stampResult = await _userManager.UpdateSecurityStampAsync(user);
            if (!stampResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return (false, string.Join("; ", stampResult.Errors.Select(error => error.Description)));
            }

            await LogActionAsync(adminId, "UnlockUser", user.Id, "Mở khóa tài khoản");
            await transaction.CommitAsync();
            return (true, "Đã mở khóa tài khoản.");
        }

        public async Task LogActionAsync(string actorId, string action, string targetId, string? details)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = actorId,
                Action = action,
                TargetUserId = targetId,
                Details = details,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        // ... (Phần RegisterAsync giữ nguyên như code trước)
    }
}