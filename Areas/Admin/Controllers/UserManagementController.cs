using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Models.ViewModels;
using DatLichKhamBenh.Services.Interfaces;

namespace DatLichKhamBenh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private const string DefaultAccountPassword = "Medislot@2026";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly ILogger<UserManagementController> _logger;
        private readonly IEmailService _emailService;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IAuthService authService,
            ILogger<UserManagementController> logger,
            IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _authService = authService;
            _logger = logger;
            _emailService = emailService;
        }

        // 1. DANH SÁCH NGƯỜI DÙNG
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .Where(u => u.IsActive)
                .ToListAsync();
            var userRoles = new Dictionary<string, string>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "Chưa xác định";
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["WarningMessage"] = "Không thể chuyển tài khoản quản trị viên vào thùng rác.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = false;
            user.LockoutEnd = DateTimeOffset.MaxValue;
            await _userManager.UpdateAsync(user);
            await _authService.LogActionAsync(
                _userManager.GetUserId(User)!,
                "MoveToRecycleBin",
                user.Id,
                $"Chuyển tài khoản {user.Email} vào thùng rác");

            TempData["SuccessMessage"] = "Đã chuyển tài khoản vào thùng rác.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = true;
            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);
            await _authService.LogActionAsync(
                _userManager.GetUserId(User)!,
                "RestoreUser",
                user.Id,
                $"Khôi phục tài khoản {user.Email}");

            TempData["SuccessMessage"] = "Đã khôi phục tài khoản.";
            return RedirectToAction("Index", "RecycleBin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentlyDeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["WarningMessage"] = "Không thể xóa vĩnh viễn tài khoản quản trị viên.";
                return RedirectToAction("Index", "RecycleBin");
            }

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (doctor != null)
            {
                var appointments = await _context.Appointments.Where(a => a.DoctorId == doctor.Id).ToListAsync();
                var schedules = await _context.DoctorSchedules.Where(s => s.DoctorId == doctor.Id).ToListAsync();
                _context.Appointments.RemoveRange(appointments);
                _context.DoctorSchedules.RemoveRange(schedules);
                _context.Doctors.Remove(doctor);
            }

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (patient != null)
            {
                var appointments = await _context.Appointments.Where(a => a.PatientId == patient.Id).ToListAsync();
                _context.Appointments.RemoveRange(appointments);
                _context.Patients.Remove(patient);
            }

            await _context.SaveChangesAsync();
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["WarningMessage"] = "Xóa vĩnh viễn tài khoản thất bại.";
                return RedirectToAction("Index", "RecycleBin");
            }

            await _authService.LogActionAsync(
                _userManager.GetUserId(User)!,
                "PermanentDeleteUser",
                user.Id,
                $"Xóa vĩnh viễn tài khoản {user.Email}");
            TempData["SuccessMessage"] = "Đã xóa vĩnh viễn tài khoản.";
            return RedirectToAction("Index", "RecycleBin");
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
            model.Email = model.Email?.Trim() ?? string.Empty;

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

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                ViewBag.Specialties = await _context.Specialties.Where(s => s.IsActive).ToListAsync();
                return View(model);
            }

            var fullName = $"{model.MiddleName} {model.FirstName}".Trim();
            if (string.IsNullOrWhiteSpace(fullName))
                fullName = model.Email;

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = fullName,
                PhoneNumber = model.Role == "Doctor" ? model.PhoneNumber?.Trim() : null,
                IsActive = true,
                EmailConfirmed = true,
                MustChangePassword = model.Role != "Patient",
                LockoutEnabled = true
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var result = await _userManager.CreateAsync(user, DefaultAccountPassword);

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
                    FullName = fullName,
                    Degree = model.Degree?.Trim(),
                    YearsOfExperience = model.YearsOfExperience,
                    Workplace = model.Workplace?.Trim(),
                    ConsultationFee = model.ConsultationFee,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            var encodedEmail = HtmlEncoder.Default.Encode(user.Email!);
            var encodedPassword = HtmlEncoder.Default.Encode(DefaultAccountPassword);
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

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new ProfileViewModel
            {
                MiddleName = user.MiddleName,
                FirstName = user.FirstName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber
            };

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == id);
            if (doctor != null)
            {
                model.Degree = doctor.Degree;
                model.YearsOfExperience = doctor.YearsOfExperience;
                model.ConsultationFee = doctor.ConsultationFee;
                model.Workplace = doctor.Workplace;
                ViewBag.IsDoctor = true;
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, ProfileViewModel model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == id);
            if (doctor != null)
                ViewBag.IsDoctor = true;
            if (!ModelState.IsValid)
                return View(model);

            var fullName = $"{model.MiddleName} {model.FirstName}".Trim();
            user.FullName = string.IsNullOrWhiteSpace(fullName) ? user.Email ?? "Người dùng" : fullName;
            user.PhoneNumber = model.PhoneNumber?.Trim();
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            if (doctor != null)
            {
                ViewBag.IsDoctor = true;
                doctor.Degree = model.Degree?.Trim();
                doctor.YearsOfExperience = model.YearsOfExperience;
                doctor.ConsultationFee = model.ConsultationFee;
                doctor.Workplace = model.Workplace?.Trim();
                await _context.SaveChangesAsync();
            }

            await _authService.LogActionAsync(
                _userManager.GetUserId(User)!,
                "EditUser",
                user.Id,
                $"Cập nhật thông tin tài khoản {user.Email}");
            TempData["SuccessMessage"] = "Đã cập nhật tài khoản.";
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
                FullName = user.FullName,
                Reason = ""
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

        // 5. RESET MẬT KHẨU
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.Email = user.Email;
            ViewBag.UserId = user.Id;
            ViewBag.FullName = user.FullName;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId, string confirmAction)
        {
            if (string.IsNullOrEmpty(userId)) return NotFound();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, DefaultAccountPassword);

            if (result.Succeeded)
            {
                user.MustChangePassword = true;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    foreach (var error in updateResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    ViewBag.Email = user.Email;
                    ViewBag.UserId = user.Id;
                    ViewBag.FullName = user.FullName;
                    return View();
                }

                var stampResult = await _userManager.UpdateSecurityStampAsync(user);
                if (!stampResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    foreach (var error in stampResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    ViewBag.Email = user.Email;
                    ViewBag.UserId = user.Id;
                    ViewBag.FullName = user.FullName;
                    return View();
                }
                await _authService.LogActionAsync(
                    _userManager.GetUserId(User)!, "ResetPassword", user.Id,
                    "Admin đặt lại mật khẩu về mật khẩu mặc định");
                await transaction.CommitAsync();
                var emailSent = await _emailService.SendAsync(
                    user.Email!,
                    "Mật khẩu MediSlot đã được đặt lại",
                    $"<p>Mật khẩu mặc định mới: <strong>{HtmlEncoder.Default.Encode(DefaultAccountPassword)}</strong></p><p>Bạn phải đổi mật khẩu ở lần đăng nhập tiếp theo.</p>");

                if (emailSent)
                {
                    TempData["SuccessMessage"] = $"Đã reset mật khẩu cho tài khoản {user.Email}. Mật khẩu tạm thời đã được gửi qua email.";
                }
                else
                {
                    _logger.LogWarning("Không gửi được email mật khẩu tạm thời tới {Email}.", user.Email);
                    TempData["WarningMessage"] =
                        $"Đã reset mật khẩu cho tài khoản {user.Email}, nhưng không gửi được email. Vui lòng kiểm tra cấu hình SMTP.";
                }
            }
            else
            {
                await transaction.RollbackAsync();
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                ViewBag.Email = user.Email;
                ViewBag.UserId = user.Id;
                ViewBag.FullName = user.FullName;
                return View();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}