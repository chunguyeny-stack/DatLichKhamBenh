using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
// Thêm namespace chứa ApplicationDbContext của bạn
using DatLichKhamBenh.Data; 

namespace DatLichKhamBenh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context; // Inject thêm DbContext
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context, // Thêm vào Constructor
            ILogger<UserManagementController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        // ... (Giữ nguyên Action Index) ...

        // =====================================================
        // 2. TẠO TÀI KHOẢN NHÂN SỰ (Bác sĩ / Lễ tân)
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> CreateStaff()
        {
            // Truyền danh sách chuyên khoa đang hoạt động sang View
            ViewBag.Specialties = await _context.Specialties
                .Where(s => s.IsActive)
                .ToListAsync();
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
                ModelState.AddModelError("Role", "Vai trò không hợp lệ. Chỉ chấp nhận Doctor hoặc Receptionist.");
                ViewBag.Specialties = await _context.Specialties.Where(s => s.IsActive).ToListAsync();
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng trong hệ thống.");
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
                MustChangePassword = true,
                LockoutEnabled = true
            };

            const string defaultPassword = "MedBooking@123";
            var result = await _userManager.CreateAsync(user, defaultPassword);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                _logger.LogInformation($"Admin đã tạo tài khoản {model.Role}: {model.Email}");

                // NẾU LÀ BÁC SĨ -> TẠO HỒ SƠ VÀ GÁN CHUYÊN KHOA
                if (model.Role == "Doctor")
                {
                    var doctorProfile = new Doctor
                    {
                        UserId = user.Id,
                        SpecialtyId = model.SpecialtyId, // Gán chuyên khoa được chọn
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    _context.Doctors.Add(doctorProfile);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"Đã tạo tài khoản {model.Role} thành công. Mật khẩu mặc định: {defaultPassword}";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            ViewBag.Specialties = await _context.Specialties.Where(s => s.IsActive).ToListAsync();
            return View(model);
        }

        // ... (Giữ nguyên các Action LockUser, UnlockUser) ...
    }
}