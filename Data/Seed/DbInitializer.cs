using Microsoft.AspNetCore.Identity;
using DatLichKhamBenh.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Data.Seed
{
    public static class DbInitializer
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            // Thêm vào DbInitializer.cs, phía trên phần tạo tài khoản Admin
// ==========================================
// 0. TẠO DỮ LIỆU MẪU CHUYÊN KHOA
// ==========================================
try
{
    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
    if (!await context.Specialties.AnyAsync())
    {
        var specialties = new List<Specialty>
        {
            new Specialty { Name = "Tim mạch", Description = "Khám và điều trị các bệnh về tim mạch" },
            new Specialty { Name = "Nhi khoa", Description = "Khám và điều trị bệnh cho trẻ em" },
            new Specialty { Name = "Da liễu", Description = "Khám và điều trị các bệnh về da" },
            new Specialty { Name = "Răng Hàm Mặt", Description = "Khám và điều trị răng miệng" },
            new Specialty { Name = "Tai Mũi Họng", Description = "Khám và điều trị bệnh tai mũi họng" }
        };
        context.Specialties.AddRange(specialties);
        await context.SaveChangesAsync();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[Seed] Lỗi tạo Specialty: {ex.Message}");
}
            // ==========================================
            // 1. TẠO CÁC ROLE (QUYỀN) CƠ BẢN
            // ==========================================
            string[] roleNames = { "Admin", "Doctor", "Receptionist", "Patient" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // ==========================================
            // 2. TẠO TÀI KHOẢN ADMIN MẶC ĐỊNH
            // ==========================================
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

            // ==========================================
            // 3. TẠO TÀI KHOẢN BỆNH NHÂN MẪU
            // ==========================================
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

                    // Tạo hồ sơ Patient (nếu bạn đã có DbSet<Patient>)
                    try
                    {
                        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                        var patientExists = await context.Patients
                            .AnyAsync(p => p.UserId == newPatient.Id);

                        if (!patientExists)
                        {
                            context.Patients.Add(new Patient
                            {
                                UserId = newPatient.Id,
                                // Các trường khác nếu Patient entity có (DateOfBirth, Address...)
                            });
                            await context.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Bỏ qua nếu bảng Patient chưa có trong DB (trước khi migration)
                        Console.WriteLine($"[Seed] Không thể tạo hồ sơ Patient: {ex.Message}");
                    }
                }
            }

            // ==========================================
            // 4. (TÙY CHỌN) TẠO TÀI KHOẢN BÁC SĨ MẪU
            // ==========================================
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

                    try
                    {
                        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                        var doctorExists = await context.Doctors
                            .AnyAsync(d => d.UserId == newDoctor.Id);

                        if (!doctorExists)
                        {
                            context.Doctors.Add(new Doctor
                            {
                                UserId = newDoctor.Id,
                                // Các trường khác nếu Doctor entity có (SpecialtyId, Experience...)
                            });
                            await context.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Seed] Không thể tạo hồ sơ Doctor: {ex.Message}");
                    }
                }
            }
        }
    }
}