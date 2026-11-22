using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;

namespace DatLichKhamBenh.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    [Authorize(Roles = "Receptionist")]
    public class ho_so_ca_nhan_le_tan : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ho_so_ca_nhan_le_tan(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        public record ProfileUpdateRequest(string? fullname, string? phone, string? address, string? email, string? id);
        public record PasswordChangeRequest(string? mat_khau_cu, string? mat_khau_moi, string? oldPassword, string? newPassword, string? confirmPassword, string? id);

        private async Task<Dictionary<string, object?>> QueryFirstUserRowAsync(string? userId, string? userName)
        {
            var conn = _context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            if (!string.IsNullOrWhiteSpace(userId))
            {
                cmd.CommandText = "SELECT TOP 1 * FROM [AspNetUsers] WHERE [Id] = @p0";
                var p = cmd.CreateParameter();
                p.ParameterName = "@p0";
                p.Value = userId;
                cmd.Parameters.Add(p);
            }
            else if (!string.IsNullOrWhiteSpace(userName))
            {
                cmd.CommandText = "SELECT TOP 1 * FROM [AspNetUsers] WHERE [UserName] = @p0 OR [Email] = @p0";
                var p = cmd.CreateParameter();
                p.ParameterName = "@p0";
                p.Value = userName;
                cmd.Parameters.Add(p);
            }
            else
            {
                return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            }

            await using var reader = await cmd.ExecuteReaderAsync();
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            if (await reader.ReadAsync())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            return row;
        }

        private static string GetColStr(IDictionary<string, object?> row, params string[] keys)
        {
            foreach (var k in keys)
            {
                if (row.TryGetValue(k, out var val) && val != null && val != DBNull.Value)
                {
                    var s = Convert.ToString(val)?.Trim();
                    if (!string.IsNullOrWhiteSpace(s)) return s!;
                }
            }
            return "";
        }

        private async Task PopulateFromSqlAsync(ApplicationUser? user)
        {
            var claimUserId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var claimUserName = User?.Identity?.IsAuthenticated == true ? User.Identity.Name : null;

            var sqlRow = await QueryFirstUserRowAsync(user?.Id ?? claimUserId, user?.UserName ?? claimUserName);

            var fullNameDb = GetColStr(sqlRow, "FullName", "full_name", "HoTen", "ho_ten");
            var loginName = GetColStr(sqlRow, "UserName", "user_name") ?? (claimUserName ?? "");
            var fullName = !string.IsNullOrWhiteSpace(fullNameDb) ? fullNameDb : loginName;

            var email = GetColStr(sqlRow, "Email", "email");
            var phone = GetColStr(sqlRow, "PhoneNumber", "phone_number", "SoDienThoai", "so_dien_thoai");
            var address = GetColStr(sqlRow, "Address", "address", "DiaChi", "dia_chi");
            var code = GetColStr(sqlRow, "MaLeTan", "ma_le_tan", "Id", "id");

            ViewBag.ReceptionistName = fullName;
            ViewBag.ReceptionistEmail = email;
            ViewBag.ReceptionistPhone = phone;
            ViewBag.ReceptionistAddress = address;
            ViewBag.ReceptionistCode = code;
        }

        private async Task<IActionResult> ResolveProfileViewAsync(ApplicationUser? model = null)
        {
            await PopulateFromSqlAsync(model);

            string[] candidates =
            {
                "~/Areas/Receptionist/Views/ho_so_ca_nhan_le_tan/ho_so_ca_nhan_le_tan.cshtml",
                "~/Areas/Receptionist/Views/ho_so_ca_nhan_le_tan/Index.cshtml",
                "~/Areas/Receptionist/Views/ho_so_ca_nhan_le_tan/ho_so_ca_nhan.cshtml",
                "~/Areas/Receptionist/Views/Profile/ho_so_ca_nhan_le_tan.cshtml",
                "~/Areas/Receptionist/Views/Profile/Index.cshtml",
                "~/Areas/Receptionist/Views/Home/ho_so_ca_nhan_le_tan.cshtml"
            };

            foreach (var v in candidates)
            {
                var full = Path.Combine(_env.ContentRootPath, v.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(full))
                    return model == null ? View(v) : View(v, model);
            }

            var viewsRoot = Path.Combine(_env.ContentRootPath, "Areas", "Receptionist", "Views");
            if (Directory.Exists(viewsRoot))
            {
                var found = Directory.GetFiles(viewsRoot, "*.cshtml", SearchOption.AllDirectories)
                    .FirstOrDefault(f =>
                    {
                        var name = Path.GetFileNameWithoutExtension(f).ToLowerInvariant();
                        return name.Contains("ho_so") || name.Contains("profile");
                    });

                if (!string.IsNullOrWhiteSpace(found))
                {
                    var rel = "~/" + Path.GetRelativePath(_env.ContentRootPath, found).Replace('\\', '/');
                    return model == null ? View(rel) : View(rel, model);
                }
            }

            return Content("Không tìm thấy file view hồ sơ lễ tân trong Areas/Receptionist/Views.", "text/plain; charset=utf-8");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ApplicationUser? receptionist = null;
            if (User?.Identity?.IsAuthenticated == true)
                receptionist = await _userManager.GetUserAsync(User);

            return await ResolveProfileViewAsync(receptionist);
        }

        [HttpPost]
        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] ProfileUpdateRequest? body,
            string? id,
            string? fullName,
            string? phoneNumber,
            string? email)
        {
            var targetId = body?.id ?? id;
            var targetName = body?.fullname ?? fullName ?? "";
            var targetPhone = body?.phone ?? phoneNumber ?? "";
            var targetEmail = body?.email ?? email ?? "";

            ApplicationUser? user = null;
            if (!string.IsNullOrWhiteSpace(targetId))
                user = await _userManager.FindByIdAsync(targetId);
            if (user == null && User?.Identity?.IsAuthenticated == true)
                user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                if (body != null) return NotFound(new { success = false, detail = "Không tìm thấy tài khoản trong SQL." });
                TempData["Error"] = "Không tìm thấy tài khoản trong SQL.";
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrWhiteSpace(targetName))
                user.FullName = targetName.Trim();

            user.PhoneNumber = string.IsNullOrWhiteSpace(targetPhone) ? null : targetPhone.Trim();
            if (!string.IsNullOrWhiteSpace(targetEmail))
            {
                user.Email = targetEmail.Trim();
                user.UserName = targetEmail.Trim();
            }

            var result = await _userManager.UpdateAsync(user);
            if (body != null)
            {
                if (result.Succeeded) return Json(new { success = true });
                return BadRequest(new { success = false, detail = string.Join("; ", result.Errors.Select(e => e.Description)) });
            }

            if (result.Succeeded) TempData["Success"] = "Cập nhật hồ sơ thành công!";
            else TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            [FromBody] PasswordChangeRequest? body,
            string? id,
            string? oldPassword,
            string? newPassword,
            string? confirmPassword)
        {
            var targetId = body?.id ?? id;
            var currPass = body?.mat_khau_cu ?? body?.oldPassword ?? oldPassword ?? "";
            var nextPass = body?.mat_khau_moi ?? body?.newPassword ?? newPassword ?? "";
            var confPass = body?.confirmPassword ?? confirmPassword ?? nextPass;

            if (nextPass != confPass)
            {
                if (body != null) return BadRequest(new { success = false, detail = "Mật khẩu mới và xác nhận mật khẩu không khớp!" });
                TempData["Error"] = "Mật khẩu mới và xác nhận mật khẩu không khớp!";
                return RedirectToAction(nameof(Index));
            }

            ApplicationUser? user = null;
            if (!string.IsNullOrWhiteSpace(targetId))
                user = await _userManager.FindByIdAsync(targetId);
            if (user == null && User?.Identity?.IsAuthenticated == true)
                user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                if (body != null) return NotFound(new { success = false, detail = "Không tìm thấy tài khoản." });
                TempData["Error"] = "Không tìm thấy tài khoản.";
                return RedirectToAction(nameof(Index));
            }

            var changeResult = await _userManager.ChangePasswordAsync(user, currPass, nextPass);
            if (body != null)
            {
                if (changeResult.Succeeded) return Json(new { success = true });
                return BadRequest(new { success = false, detail = "Mật khẩu hiện tại không đúng hoặc mật khẩu mới chưa đạt yêu cầu." });
            }

            if (changeResult.Succeeded) TempData["Success"] = "Đổi mật khẩu thành công!";
            else TempData["Error"] = "Mật khẩu hiện tại không đúng hoặc mật khẩu mới chưa đạt yêu cầu.";
            return RedirectToAction(nameof(Index));
        }
    }
}