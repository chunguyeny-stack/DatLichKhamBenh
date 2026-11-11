using DatLichKhamBenh.Models;
using Microsoft.AspNetCore.Identity;

namespace DatLichKhamBenh.Data.Seed
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Đảm bảo cơ sở dữ liệu đã tạo
            await context.Database.EnsureCreatedAsync();

            // Tạo vai trò
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Tạo tài khoản Admin mặc định
            if (await userManager.FindByEmailAsync("admin@datlich.com") == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = "admin@datlich.com",
                    Email = "admin@datlich.com",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Thêm dữ liệu mẫu cho Chuyên Khoa
            if (!context.ChuyenKhoas.Any())
            {
                context.ChuyenKhoas.AddRange(new List<ChuyenKhoa>
                {
                    new ChuyenKhoa { TenChuyenKhoa = "Nội tiết", MoTa = "Khám và điều trị các bệnh nội tiết" },
                    new ChuyenKhoa { TenChuyenKhoa = "Nhi khoa", MoTa = "Chăm sóc sức khỏe trẻ em" },
                    new ChuyenKhoa { TenChuyenKhoa = "Da liễu", MoTa = "Điều trị các bệnh về da" },
                    new ChuyenKhoa { TenChuyenKhoa = "Tai mũi họng", MoTa = "Khám tai, mũi, họng" }
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
