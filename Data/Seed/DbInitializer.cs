using Microsoft.AspNetCore.Identity;
using DatLichKhamBenh.Models.Entities;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Data; // Rất quan trọng để gọi DbContext

namespace DatLichKhamBenh.Data.Seed
{
    public static class DbInitializer
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 0. TẠO DỮ LIỆU MẪU CHUYÊN KHOA
            if (!await context.Specialties.AnyAsync())
            {
                var specialties = new List<Specialty>
                {
                    new Specialty { Name = "Tim mạch", Description = "Khám và điều trị các bệnh về tim mạch", IsActive = true },
                    new Specialty { Name = "Nhi khoa", Description = "Khám và điều trị bệnh cho trẻ em", IsActive = true },
                    new Specialty { Name = "Da liễu", Description = "Khám và điều trị các bệnh về da", IsActive = true },
                    new Specialty { Name = "Răng Hàm Mặt", Description = "Khám và điều trị răng miệng", IsActive = true },
                    new Specialty { Name = "Tai Mũi Họng", Description = "Khám và điều trị bệnh tai mũi họng", IsActive = true }
                };
                context.Specialties.AddRange(specialties);
                await context.SaveChangesAsync();
            }

            // 1. TẠO CÁC ROLE
            string[] roleNames = { "Admin", "Doctor", "Receptionist", "Patient" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. TẠO TÀI KHOẢN ADMIN
            string adminEmail = "admin@medislot.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Quản trị",
                    MiddleName = "Viên",
                    PhoneNumber = "0123456789",
                    MustChangePassword = false,
                    LockoutEnabled = true
                };

                const string adminPassword = "Admin@123";
                var createAdminResult = await userManager.CreateAsync(newAdmin, adminPassword);
                if (createAdminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            // 3. TẠO TÀI KHOẢN BỆNH NHÂN MẪU
            string patientEmail = "patient@gmail.com";
            var patientUser = await userManager.FindByEmailAsync(patientEmail);
            if (patientUser == null)
            {
                var newPatient = new ApplicationUser
                {
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
                    if (!await context.Patients.AnyAsync(p => p.UserId == newPatient.Id))
                    {
                        context.Patients.Add(new Patient { UserId = newPatient.Id });
                        await context.SaveChangesAsync();
                    }
                }
            }

            // 4. TẠO TÀI KHOẢN BÁC SĨ MẪU
            string doctorEmail = "doctor@medislot.com";
            var doctorUser = await userManager.FindByEmailAsync(doctorEmail);
            if (doctorUser == null)
            {
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
                        context.Doctors.Add(new Doctor
                        {
                            UserId = newDoctor.Id,
                            SpecialtyId = defaultSpecialty?.Id,
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        });
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}