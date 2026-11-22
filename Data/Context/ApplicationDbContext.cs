using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Models.Entities;

namespace DatLichKhamBenh.Data.Context
{
    // Kế thừa IdentityDbContext để hỗ trợ đăng nhập
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- BẢNG CỦA BẠN KIA (Hệ thống cũ - Tiếng Anh) ---
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

        // --- BẢNG CỦA BẠN (Luồng khám bệnh) ---
        public DbSet<HoSoBenhAn> HoSoBenhAns { get; set; }
        public DbSet<DonThuoc> DonThuocs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình quan hệ của bạn kia (Tiếng Anh)
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne(u => u.DoctorProfile)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.PatientProfile)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Specialty)
                .WithMany(s => s.Doctors)
                .HasForeignKey(d => d.SpecialtyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Cấu hình tên bảng của bạn kia (Tiếng Việt)
            modelBuilder.Entity<BacSi>().ToTable("bac_si");
            modelBuilder.Entity<BenhNhan>().ToTable("benh_nhan");
            modelBuilder.Entity<ChuyenKhoa>().ToTable("chuyen_khoa");
            modelBuilder.Entity<DichVu>().ToTable("dich_vu");
            modelBuilder.Entity<LichHen>().ToTable("lich_hen");
            modelBuilder.Entity<LichLamViec>().ToTable("lich_lam_viec");
            
            // 3. Cấu hình tên bảng của bạn (Luồng khám bệnh)
            modelBuilder.Entity<HoSoBenhAn>().ToTable("ho_so_benh_an");
            modelBuilder.Entity<DonThuoc>().ToTable("don_thuoc");
        }
    }
}