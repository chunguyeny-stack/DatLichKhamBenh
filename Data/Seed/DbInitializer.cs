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

namespace DatLichKhamBenh.Data.Seed
{
    public static class DbInitializer
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
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
            }
        }
    }
}