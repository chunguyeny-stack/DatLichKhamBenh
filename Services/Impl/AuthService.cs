using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Models.ViewModels;
using DatLichKhamBenh.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DatLichKhamBenh.Services.Impl
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        // ==========================================
        // 1. ĐĂNG NHẬP
        // ==========================================
        public async Task<(bool Success, string Message)> LoginAsync(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return (false, "Email hoặc mật khẩu không chính xác.");

            if (!user.IsActive)
                return (false, "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên.");

            if (await _userManager.IsLockedOutAsync(user))
                return (false, "Tài khoản đã bị khóa tạm thời do đăng nhập sai nhiều lần. Vui lòng thử lại sau.");

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
                return (true, "Đăng nhập thành công.");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Tài khoản {Email} bị khóa do đăng nhập sai nhiều lần.", model.Email);
                return (false, "Tài khoản đã bị khóa tạm thời do đăng nhập sai nhiều lần.");
            }

            return (false, "Email hoặc mật khẩu không chính xác.");
        }

        // ==========================================
        // 2. ĐĂNG KÝ (BỆNH NHÂN) - BỎ OTP
        // ==========================================
        public async Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model)
        {
            if (model == null)
                return (false, "Dữ liệu đăng ký không hợp lệ.");

            var email = model.Email?.Trim() ?? string.Empty;

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
                return (false, "Email này đã được sử dụng. Vui lòng đăng nhập hoặc dùng email khác.");

            var fullName = $"{model.MiddleName} {model.FirstName}".Trim();
            if (string.IsNullOrWhiteSpace(fullName)) fullName = email;

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                MiddleName = model.MiddleName?.Trim() ?? string.Empty,
                FirstName = model.FirstName?.Trim() ?? string.Empty,
                PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim(),
                DateOfBirth = model.DateOfBirth,
                IsActive = true,
                MustChangePassword = false,
                LockoutEnabled = true,
                LockoutEnd = null
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                await transaction.RollbackAsync();
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Đăng ký thất bại cho {Email}: {Errors}", email, errors);
                return (false, errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Patient");
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Gán role Patient thất bại cho {Email}: {Errors}", email, errors);
                return (false, errors);
            }

            _context.Patients.Add(new Patient
            {
                UserId = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                DateOfBirth = model.DateOfBirth,
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi tạo hồ sơ bệnh nhân cho {Email}.", email);
                await _userManager.DeleteAsync(user);
                return (false, "Không thể hoàn tất đăng ký. Vui lòng thử lại sau.");
            }

            _logger.LogInformation("Đã đăng ký tài khoản bệnh nhân {Email} thành công.", email);
            return (true, "Đăng ký thành công. Vui lòng đăng nhập.");
        }

        // ==========================================
        // 3. ĐỔI MẬT KHẨU
        // ==========================================
        public async Task<(bool Success, string Message)> ChangePasswordAsync(string userId, ChangePasswordViewModel model)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return (false, "Không xác định được tài khoản.");

            if (model == null || string.IsNullOrWhiteSpace(model.NewPassword))
                return (false, "Dữ liệu đổi mật khẩu không hợp lệ.");

            if (!string.Equals(model.NewPassword, model.ConfirmPassword, StringComparison.Ordinal))
                return (false, "Mật khẩu xác nhận không khớp.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, "Không tìm thấy tài khoản.");

            // Sử dụng OldPassword thay vì CurrentPassword theo ViewModel mới
            var result = await _userManager.ChangePasswordAsync(
                user,
                model.OldPassword ?? string.Empty,
                model.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Đổi mật khẩu thất bại cho {UserId}: {Errors}", userId, errors);
                return (false, errors);
            }

            // Tắt cờ đổi mật khẩu lần đầu
            if (user.MustChangePassword)
            {
                user.MustChangePassword = false;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                    _logger.LogError("Không thể tắt cờ MustChangePassword cho {UserId}: {Errors}", userId, errors);
                    return (false, "Đổi mật khẩu thành công nhưng không thể cập nhật trạng thái tài khoản. Vui lòng liên hệ quản trị viên.");
                }
            }

            // Vô hiệu hóa các session cũ
            await _userManager.UpdateSecurityStampAsync(user);

            _logger.LogInformation("Tài khoản {UserId} đã đổi mật khẩu thành công.", userId);
            return (true, "Đổi mật khẩu thành công.");
        }

        // ==========================================
        // 4. KHÓA TÀI KHOẢN
        // ==========================================
        public async Task<(bool Success, string Message)> LockUserAsync(string adminId, LockUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return (false, "Không tìm thấy tài khoản cần khóa.");

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return (false, "Không thể khóa tài khoản Quản trị viên.");

            user.LockoutEnd = DateTimeOffset.MaxValue;
            user.LockoutEnabled = true;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return (false, "Khóa tài khoản thất bại.");

            await LogActionAsync(adminId, "LockUser", user.Id, $"Khóa tài khoản {user.Email}. Lý do: {model.Reason}");
            return (true, "Đã khóa tài khoản thành công.");
        }

        // ==========================================
        // 5. MỞ KHÓA TÀI KHOẢN
        // ==========================================
        public async Task<(bool Success, string Message)> UnlockUserAsync(string adminId, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, "Không tìm thấy tài khoản.");

            user.LockoutEnd = null;
            user.AccessFailedCount = 0;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return (false, "Mở khóa tài khoản thất bại.");

            await LogActionAsync(adminId, "UnlockUser", user.Id, $"Mở khóa tài khoản {user.Email}");
            return (true, "Đã mở khóa tài khoản thành công.");
        }

        // ==========================================
        // 6. GHI LOG HÀNH ĐỘNG
        // ==========================================
        public async Task LogActionAsync(string actorUserId, string action, string targetUserId, string? details)
        {
            try
            {
                var log = new AuditLog
                {
                    ActorUserId = actorUserId,
                    Action = action,
                    TargetUserId = targetUserId,
                    Details = details,
                    CreatedAt = DateTime.Now
                };
                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể ghi AuditLog cho hành động {Action}.", action);
            }
        }
    }
}