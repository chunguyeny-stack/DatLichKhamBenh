using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
                    new Specialty
                    {
                        Name = "Tim mạch",
                        Description = "Tầm soát và điều trị bệnh lý mạch máu, cao huyết áp, rối loạn nhịp tim",
                        CommonSymptoms = "Đau tức ngực, Hồi hộp đánh trống ngực, Tăng huyết áp, Khó thở"
                    },
                    new Specialty
                    {
                        Name = "Nhi khoa",
                        Description = "Chăm sóc sức khỏe toàn diện và điều trị bệnh lý ở trẻ nhỏ",
                        CommonSymptoms = "Sốt cao, Ho khò khè, Biếng ăn, Nôn trớ, Tiêu chảy"
                    },
                    new Specialty
                    {
                        Name = "Da liễu",
                        Description = "Tư vấn và điều trị bệnh lý về da, dị ứng, mụn trứng cá, chàm",
                        CommonSymptoms = "Nổi mẩn đỏ ngứa, Mụn trứng cá, Dị ứng nổi mề đay, Viêm da"
                    },
                    new Specialty
                    {
                        Name = "Tai Mũi Họng",
                        Description = "Điều trị viêm xoang, viêm họng, các vấn đề thính lực và thanh quản",
                        CommonSymptoms = "Đau rát họng, Ù tai, Nghẹt mũi, Khàn tiếng, Viêm xoang"
                    },
                    new Specialty
                    {
                        Name = "Thần kinh",
                        Description = "Chẩn đoán và điều trị bệnh lý về nền thần kinh, mất ngủ, đau đầu",
                        CommonSymptoms = "Đau đầu nửa đầu, Mất ngủ kéo dài, Chóng mặt, Tê bì tay chân"
                    }
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
                    FullName = "Quản trị viên hệ thống",
                    IsActive = true,
                    EmailConfirmed = true,
                    PhoneNumber = "0123456789",
                    MustChangePassword = true,
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

                if (!await context.Patients.AnyAsync(p => p.UserId == existing.Id))
                {
                    context.Patients.Add(new Patient
                    {
                        UserId = existing.Id,
                        FullName = string.IsNullOrWhiteSpace(existing.FullName) ? "Nguyễn Văn A" : existing.FullName,
                        PhoneNumber = existing.PhoneNumber ?? "0987654321",
                        Email = existing.Email,
                        DateOfBirth = new DateTime(1998, 5, 12)
                    });
                    await context.SaveChangesAsync();
                }
                return;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = "Nguyễn Văn A",
                FirstName = "Văn A",
                MiddleName = "Nguyễn",
                PhoneNumber = "0987654321",
                IsActive = true,
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
            context.Patients.Add(new Patient
            {
                UserId = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                DateOfBirth = new DateTime(1998, 5, 12)
            });
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

                if (!await context.Doctors.AnyAsync(d => d.UserId == existing.Id))
                {
                    var defaultSpecialtyId = await context.Specialties.Select(s => s.Id).FirstOrDefaultAsync();
                    if (defaultSpecialtyId > 0)
                    {
                        context.Doctors.Add(new Doctor
                        {
                            UserId = existing.Id,
                            SpecialtyId = defaultSpecialtyId,
                            FullName = string.IsNullOrWhiteSpace(existing.FullName) ? "BS. Trần Thị B" : existing.FullName,
                            Degree = "Bác sĩ CKI",
                            YearsOfExperience = 5,
                            ConsultationFee = 200000m,
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        });
                        await context.SaveChangesAsync();
                    }
                }
                return;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = "BS. Trần Thị B",
                FirstName = "B",
                MiddleName = "Trần Thị",
                PhoneNumber = "0912345678",
                IsActive = true,
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
                FullName = user.FullName,
                Degree = "Bác sĩ CKI",
                YearsOfExperience = 5,
                ConsultationFee = 200000m,
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
                FullName = "Lễ tân Mẫu",
                FirstName = "Lễ tân",
                MiddleName = "Mẫu",
                IsActive = true,
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