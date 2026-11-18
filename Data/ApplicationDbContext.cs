using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Models.Entities;

namespace DatLichKhamBenh.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Các bảng nghiệp vụ
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Specialty> Specialties { get; set; } // <-- THÊM MỚI

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Cấu hình quan hệ User - Doctor (1-1)
            builder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne(u => u.DoctorProfile)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Cấu hình quan hệ User - Patient (1-1)
            builder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.PatientProfile)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Cấu hình quan hệ Specialty - Doctor (1-N) <-- THÊM MỚI
            builder.Entity<Doctor>()
                .HasOne(d => d.Specialty)
                .WithMany(s => s.Doctors)
                .HasForeignKey(d => d.SpecialtyId)
                .OnDelete(DeleteBehavior.SetNull); // Nếu xóa chuyên khoa, bác sĩ vẫn còn nhưng SpecialtyId = null
        }
    }
}