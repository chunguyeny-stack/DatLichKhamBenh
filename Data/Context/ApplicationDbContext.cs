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

        public DbSet<BacSi> BacSis { get; set; }
        public DbSet<BenhNhan> BenhNhans { get; set; }
        public DbSet<ChuyenKhoa> ChuyenKhoas { get; set; }
        public DbSet<DichVu> DichVus { get; set; }
        public DbSet<LichHen> LichHens { get; set; }
        public DbSet<LichLamViec> LichLamViecs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Khớp tên bảng thực tế trong SQL Server (viết thường, có gạch dưới)
            modelBuilder.Entity<BacSi>().ToTable("bac_si");
            modelBuilder.Entity<BenhNhan>().ToTable("benh_nhan");
            modelBuilder.Entity<ChuyenKhoa>().ToTable("chuyen_khoa");
            modelBuilder.Entity<DichVu>().ToTable("dich_vu");
            modelBuilder.Entity<LichHen>().ToTable("lich_hen");
            modelBuilder.Entity<LichLamViec>().ToTable("lich_lam_viec");
        }
    }
}