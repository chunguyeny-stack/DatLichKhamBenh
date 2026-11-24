using DatLichKhamBenh.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace DatLichKhamBenh.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> LoginAsync(LoginViewModel model);
        Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model);
        Task<(bool Success, string Message)> ChangePasswordAsync(string userId, ChangePasswordViewModel model);

        // Hàm Lock/Unlock dùng cho Admin
        Task<(bool Success, string Message)> LockUserAsync(string adminId, LockUserViewModel model);
        Task<(bool Success, string Message)> UnlockUserAsync(string adminId, string targetUserId);

        // Hàm ghi log (Audit)
        Task LogActionAsync(string actorId, string action, string targetId, string? details);
    }
}