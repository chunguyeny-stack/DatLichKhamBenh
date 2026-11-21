using DatLichKhamBenh.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Data.Seed
{
    public static class DbInitializer
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider, IWebHostEnvironment env)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Seeder");

            // 0. SPECIALTIES
            if (!await context.Specialties.AnyAsync())
            {
                context.Specialties.AddRange(
                    new Specialty { Name = "Tim mạch", Description = "Khám và điều trị bệnh tim mạch" },
                    new Specialty { Name = "Nhi khoa", Description = "Khám và điều trị bệnh cho trẻ em" },
                    new Specialty { Name = "Da liễu", Description = "Khám và điều trị bệnh về da" },
                    new Specialty { Name = "Răng Hàm Mặt", Description = "Khám và điều trị răng miệng" },
                    new Specialty { Name = "Tai Mũi Họng", Description = "Khám và điều trị bệnh tai mũi họng" }
                );
                await context.SaveChangesAsync();
            }

            // 1. ROLES
            foreach (var roleName in new[] { "Admin", "Doctor", "Receptionist", "Patient" })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // 2. ADMIN — mật khẩu từ cấu hình, KHÔNG hardcode
            var adminEmail = config["Seed:AdminEmail"] ?? "admin@medislot.com";
            var adminPassword = config["Seed:AdminPassword"];

            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                logger.LogWarning("Seed:AdminPassword chưa được cấu hình → BỎ QUA tạo Admin. " +
                                  "Hãy set bằng user-secrets hoặc biến môi trường.");
            }
            else if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Quản trị",
                    MiddleName = "Viên",
                    PhoneNumber = "0123456789",
                    MustChangePassword = true,     // BẮT BUỘC đổi lần đầu
                    LockoutEnabled = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
                else
                    logger.LogError("Tạo Admin thất bại: {Errors}",
                        string.Join("; ", result.Errors.Select(e => e.Description)));
            }
            else
            {
                var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
                if (existingAdmin != null && env.IsDevelopment() && !string.IsNullOrWhiteSpace(adminPassword))
                {
                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(existingAdmin);
                    await userManager.ResetPasswordAsync(existingAdmin, resetToken, adminPassword);
                    existingAdmin.EmailConfirmed = true;
                    existingAdmin.MustChangePassword = true;
                    await userManager.UpdateAsync(existingAdmin);
                }
            }

            // 3. USER MẪU — CHỈ Ở DEVELOPMENT
            if (env.IsDevelopment())
            {
                await SeedDemoPatient(userManager, context, config, logger);
                await SeedDemoDoctor(userManager, context, config, logger);
                await SeedDemoReceptionist(userManager, config, logger);
            }
        }

        private static async Task SeedDemoPatient(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IConfiguration config,
            ILogger logger)
        {
            const string email = "patient@gmail.com";
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                var testPassword = config["Seed:DemoPassword"];
                if (!string.IsNullOrWhiteSpace(testPassword))
                {
                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(existing);
                    await userManager.ResetPasswordAsync(existing, resetToken, testPassword);
                    existing.EmailConfirmed = true;
                    existing.MustChangePassword = true;
                    await userManager.UpdateAsync(existing);
                }
                return;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = "Văn A",
                MiddleName = "Nguyễn",
                PhoneNumber = "0987654321",
                MustChangePassword = true,
                LockoutEnabled = true
            };

            var password = config["Seed:DemoPassword"];
            if (string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("Seed:DemoPassword chưa được cấu hình → bỏ qua tạo tài khoản bệnh nhân mẫu.");
                return;
            }
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                logger.LogError("Seed patient lỗi: {E}",
                    string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }

            await userManager.AddToRoleAsync(user, "Patient");
            context.Patients.Add(new Patient { UserId = user.Id });
            await context.SaveChangesAsync();
        }

        private static async Task SeedDemoDoctor(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IConfiguration config,
            ILogger logger)
        {
            const string email = "doctor@medislot.com";
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                var testPassword = config["Seed:DemoPassword"];
                if (!string.IsNullOrWhiteSpace(testPassword))
                {
                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(existing);
                    await userManager.ResetPasswordAsync(existing, resetToken, testPassword);
                    existing.EmailConfirmed = true;
                    existing.MustChangePassword = true;
                    await userManager.UpdateAsync(existing);
                }
                return;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = "B",
                MiddleName = "Trần Thị",
                PhoneNumber = "0912345678",
                MustChangePassword = true,
                LockoutEnabled = true
            };

            var password = config["Seed:DemoPassword"];
            if (string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("Seed:DemoPassword chưa được cấu hình → bỏ qua tạo tài khoản bác sĩ mẫu.");
                return;
            }
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                logger.LogError("Seed doctor lỗi: {E}",
                    string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }

            await userManager.AddToRoleAsync(user, "Doctor");
            var specialty = await context.Specialties.FirstOrDefaultAsync();
            if (specialty == null)
            {
                logger.LogWarning("Không có chuyên khoa để gán cho bác sĩ mẫu.");
                await userManager.DeleteAsync(user);
                return;
            }
            context.Doctors.Add(new Doctor
            {
                UserId = user.Id,
                SpecialtyId = specialty.Id,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            await context.SaveChangesAsync();
        }

        private static async Task SeedDemoReceptionist(
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            ILogger logger)
        {
            const string email = "receptionist@medislot.com";
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                var testPassword = config["Seed:DemoPassword"];
                if (!string.IsNullOrWhiteSpace(testPassword))
                {
                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(existing);
                    await userManager.ResetPasswordAsync(existing, resetToken, testPassword);
                    existing.EmailConfirmed = true;
                    existing.MustChangePassword = true;
                    await userManager.UpdateAsync(existing);
                }
                return;
            }

            var password = config["Seed:DemoPassword"];
            if (string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("Seed:DemoPassword chưa được cấu hình → bỏ qua tạo tài khoản lễ tân mẫu.");
                return;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = "Lễ tân",
                MiddleName = "Mẫu",
                LockoutEnabled = true,
                MustChangePassword = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                logger.LogError("Seed receptionist lỗi: {Errors}",
                    string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }

            await userManager.AddToRoleAsync(user, "Receptionist");
        }
    }
}