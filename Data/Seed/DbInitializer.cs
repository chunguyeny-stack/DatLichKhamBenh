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

            // 3. USER MẪU — CHỈ Ở DEVELOPMENT
            if (env.IsDevelopment())
            {
                await SeedDemoPatient(userManager, context, config, logger);
                await SeedDemoDoctor(userManager, context, config, logger);
                await SeedDemoReceptionist(userManager, config, logger);
                await SeedDoctorAvailability(context);
                await SeedLegacyDoctorDirectory(context);
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
                if (!await userManager.IsInRoleAsync(existing, "Patient"))
                    await userManager.AddToRoleAsync(existing, "Patient");

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
                PhoneNumber = "0987654321",
                IsActive = true,
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
                if (!await userManager.IsInRoleAsync(existing, "Doctor"))
                    await userManager.AddToRoleAsync(existing, "Doctor");

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
                if (!await userManager.IsInRoleAsync(existing, "Receptionist"))
                    await userManager.AddToRoleAsync(existing, "Receptionist");
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

        private static async Task SeedDoctorAvailability(ApplicationDbContext context)
        {
            var doctor = await context.Doctors
                .OrderBy(d => d.Id)
                .FirstOrDefaultAsync(d => d.IsActive);
            if (doctor == null)
                return;

            var slots = new[]
            {
                ("Ca sáng", "08:00 - 09:00"),
                ("Ca sáng", "09:00 - 10:00"),
                ("Ca sáng", "10:00 - 11:00"),
                ("Ca chiều", "13:30 - 14:30"),
                ("Ca chiều", "14:30 - 15:30"),
                ("Ca chiều", "15:30 - 16:30"),
                ("Ca tối", "18:00 - 19:00"),
                ("Ca tối", "19:00 - 20:00")
            };

            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(14);
            var existing = await context.DoctorSchedules
                .Where(s => s.DoctorId == doctor.Id
                    && s.WorkDate >= startDate
                    && s.WorkDate <= endDate)
                .Select(s => s.TimeSlot)
                .ToListAsync();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                foreach (var (shift, timeSlot) in slots)
                {
                    if (existing.Contains(timeSlot)
                        && await context.DoctorSchedules.AnyAsync(s =>
                            s.DoctorId == doctor.Id && s.WorkDate == date && s.TimeSlot == timeSlot))
                        continue;

                    context.DoctorSchedules.Add(new DoctorSchedule
                    {
                        DoctorId = doctor.Id,
                        WorkDate = date,
                        Shift = shift,
                        TimeSlot = timeSlot,
                        IsAvailable = true
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedLegacyDoctorDirectory(ApplicationDbContext context)
        {
            var doctors = await context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Where(d => d.IsActive)
                .ToListAsync();

            foreach (var doctor in doctors)
            {
                if (doctor.User == null || doctor.Specialty == null)
                    continue;

                var legacySpecialty = await context.ChuyenKhoas
                    .FirstOrDefaultAsync(s => s.TenChuyenKhoa == doctor.Specialty.Name);
                if (legacySpecialty == null)
                {
                    legacySpecialty = new ChuyenKhoa
                    {
                        TenChuyenKhoa = doctor.Specialty.Name,
                        MoTa = doctor.Specialty.Description,
                        TrangThai = "active"
                    };
                    context.ChuyenKhoas.Add(legacySpecialty);
                    await context.SaveChangesAsync();
                }

                var legacyDoctor = await context.BacSis
                    .FirstOrDefaultAsync(d => d.ApplicationUserId == doctor.UserId);
                if (legacyDoctor != null)
                    continue;

                context.BacSis.Add(new BacSi
                {
                    NguoiDungId = 0,
                    ApplicationUserId = doctor.UserId,
                    ChuyenKhoaId = legacySpecialty.Id,
                    HocVi = doctor.Degree,
                    KinhNghiem = doctor.YearsOfExperience.ToString(),
                    GiaKham = doctor.ConsultationFee,
                    Rating = 5,
                    SoLuongDanhGia = 1,
                    HoTen = doctor.FullName,
                    SoDienThoai = doctor.User.PhoneNumber,
                    Email = doctor.User.Email
                });
            }

            await context.SaveChangesAsync();
        }
    }
}