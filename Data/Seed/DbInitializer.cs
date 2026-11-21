<<<<<<< HEAD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
<<<<<<< HEAD
using DatLichKhamBenh.Data; // Rất quan trọng để gọi DbContext
=======
using Microsoft.Extensions.DependencyInjection;
using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;
>>>>>>> 79b2187 (Cập nhật tiếp nhận , điều phối lễ tânL)
=======
using DatLichKhamBenh.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
>>>>>>> 5b38487 (L5_B5: Fix: Luồng MCV và đồng nhất giao diện)

namespace DatLichKhamBenh.Data.Seed
{
    public static class DbInitializer
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider, IWebHostEnvironment env)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
<<<<<<< HEAD
<<<<<<< HEAD
=======

            // ==========================================
            // 0. TẠO DỮ LIỆU MẪU CHUYÊN KHOA
            // ==========================================
            try
            {
                if (!await context.ChuyenKhoas.AnyAsync())
                {
                    var specialties = new List<ChuyenKhoa>
                    {
                        new ChuyenKhoa { TenChuyenKhoa = "Tim mạch", MoTa = "Khám và điều trị các bệnh về tim mạch", TrangThai = "active" },
                        new ChuyenKhoa { TenChuyenKhoa = "Nhi khoa", MoTa = "Khám và điều trị bệnh cho trẻ em", TrangThai = "active" },
                        new ChuyenKhoa { TenChuyenKhoa = "Da liễu", MoTa = "Khám và điều trị các bệnh về da", TrangThai = "active" },
                        new ChuyenKhoa { TenChuyenKhoa = "Răng Hàm Mặt", MoTa = "Khám và điều trị răng miệng", TrangThai = "active" },
                        new ChuyenKhoa { TenChuyenKhoa = "Tai Mũi Họng", MoTa = "Khám và điều trị bệnh tai mũi họng", TrangThai = "active" }
                    };
                    context.ChuyenKhoas.AddRange(specialties);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Seed] Lỗi tạo ChuyenKhoa: {ex.Message}");
            }

            // ==========================================
            // 1. TẠO CÁC ROLE (QUYỀN) CƠ BẢN
            // ==========================================
            string[] roleNames = { "Admin", "Doctor", "Receptionist", "Patient" };
>>>>>>> 79b2187 (Cập nhật tiếp nhận , điều phối lễ tânL)
=======
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Seeder");
>>>>>>> 5b38487 (L5_B5: Fix: Luồng MCV và đồng nhất giao diện)

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
<<<<<<< HEAD
                    UserName = patientEmail,
                    Email = patientEmail,
                    EmailConfirmed = true,
                    FirstName = "Văn A",
                    MiddleName = "Nguyễn",
                    PhoneNumber = "0987654321",
                    MustChangePassword = false,
                    LockoutEnabled = true
                };

                const string patientPassword = "Patient@123";
                var createPatientResult = await userManager.CreateAsync(newPatient, patientPassword);
                if (createPatientResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newPatient, "Patient");
<<<<<<< HEAD
                    if (!await context.Patients.AnyAsync(p => p.UserId == newPatient.Id))
                    {
                        context.Patients.Add(new Patient { UserId = newPatient.Id });
                        await context.SaveChangesAsync();
=======

                    try
                    {
                        var patientExists = await context.BenhNhans
                            .AnyAsync(p => p.UserId == newPatient.Id);

                        if (!patientExists)
                        {
                            context.BenhNhans.Add(new BenhNhan
                            {
                                UserId = newPatient.Id,
                                Address = "Hà Nội",
                                Gender = "Nam"
                            });
                            await context.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Seed] Không thể tạo hồ sơ BenhNhan: {ex.Message}");
>>>>>>> 79b2187 (Cập nhật tiếp nhận , điều phối lễ tânL)
                    }
=======
                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(existingAdmin);
                    await userManager.ResetPasswordAsync(existingAdmin, resetToken, adminPassword);
                    existingAdmin.EmailConfirmed = true;
                    existingAdmin.MustChangePassword = true;
                    await userManager.UpdateAsync(existingAdmin);
>>>>>>> 5b38487 (L5_B5: Fix: Luồng MCV và đồng nhất giao diện)
                }
            }

            // 3. USER MẪU — CHỈ Ở DEVELOPMENT
            if (env.IsDevelopment())
            {
<<<<<<< HEAD
                var newDoctor = new ApplicationUser
                {
                    UserName = doctorEmail,
                    Email = doctorEmail,
                    EmailConfirmed = true,
                    FirstName = "B",
                    MiddleName = "Trần Thị",
                    PhoneNumber = "0912345678",
                    MustChangePassword = false,
                    LockoutEnabled = true
                };

                const string doctorPassword = "Doctor@123";
                var createDoctorResult = await userManager.CreateAsync(newDoctor, doctorPassword);
                if (createDoctorResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newDoctor, "Doctor");
                    var defaultSpecialty = await context.Specialties.FirstOrDefaultAsync();
                    if (!await context.Doctors.AnyAsync(d => d.UserId == newDoctor.Id))
                    {
<<<<<<< HEAD
                        context.Doctors.Add(new Doctor
                        {
                            UserId = newDoctor.Id,
                            SpecialtyId = defaultSpecialty?.Id,
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        });
                        await context.SaveChangesAsync();
=======
                        var doctorExists = await context.BacSis
                            .AnyAsync(d => d.ApplicationUserId == newDoctor.Id);

                        if (!doctorExists)
                        {
                            var firstSpecialtyId = await context.ChuyenKhoas
                                .Select(s => s.Id)
                                .FirstOrDefaultAsync();

                            if (firstSpecialtyId > 0)
                            {
                                context.BacSis.Add(new BacSi
                                {
                                    ApplicationUserId = newDoctor.Id,
                                    ChuyenKhoaId = firstSpecialtyId,
                                    HoTen = "Trần Thị B",
                                    Email = doctorEmail,
                                    SoDienThoai = "0912345678",
                                    HocVi = "BS.CKI",
                                    KinhNghiem = "5 năm",
                                    GiaKham = 200000m
                                });
                                await context.SaveChangesAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Seed] Không thể tạo hồ sơ BacSi: {ex.Message}");
>>>>>>> 79b2187 (Cập nhật tiếp nhận , điều phối lễ tânL)
                    }
                }
=======
                await SeedDemoPatient(userManager, context, config, logger);
                await SeedDemoDoctor(userManager, context, config, logger);
                await SeedDemoReceptionist(userManager, config, logger);
>>>>>>> 5b38487 (L5_B5: Fix: Luồng MCV và đồng nhất giao diện)
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
                    existing.MustChangePassword = false;
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
                MustChangePassword = false,
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
                    existing.MustChangePassword = false;
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
                MustChangePassword = false,
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
                    existing.MustChangePassword = false;
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
                MustChangePassword = false
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