using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Models.Entities;

namespace DatLichKhamBenh.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- BẢNG CỦA BẠN KIA (Hệ thống cũ) ---
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

        // --- BẢNG CỦA BẠN KIA (Hệ thống mới - Tiếng Việt) ---
        public DbSet<BacSi> BacSis { get; set; }
        public DbSet<BenhNhan> BenhNhans { get; set; }
        public DbSet<ChuyenKhoa> ChuyenKhoas { get; set; }
        public DbSet<DichVu> DichVus { get; set; }
        public DbSet<LichHen> LichHens { get; set; }
        public DbSet<LichLamViec> LichLamViecs { get; set; }

        // --- BẢNG CỦA BẠN (Luồng 3) ---
        public DbSet<HoSoBenhAn> HoSoBenhAns { get; set; }
        public DbSet<DonThuoc> DonThuocs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Cấu hình quan hệ của bạn kia (Tiếng Anh)
            builder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne(u => u.DoctorProfile)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.PatientProfile)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Doctor>()
                .HasOne(d => d.Specialty)
                .WithMany(s => s.Doctors)
                .HasForeignKey(d => d.SpecialtyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Cấu hình tên bảng của bạn kia (Tiếng Việt)
            builder.Entity<BacSi>().ToTable("bac_si");
            builder.Entity<BenhNhan>().ToTable("benh_nhan");
            builder.Entity<ChuyenKhoa>().ToTable("chuyen_khoa");
            builder.Entity<DichVu>().ToTable("dich_vu");
            builder.Entity<LichHen>().ToTable("lich_hen");
            builder.Entity<LichLamViec>().ToTable("lich_lam_viec");
            
            // (Tùy chọn) Cấu hình tên bảng của bạn
            builder.Entity<HoSoBenhAn>().ToTable("ho_so_benh_an");
            builder.Entity<DonThuoc>().ToTable("don_thuoc");
        }
    }
}