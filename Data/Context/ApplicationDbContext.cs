using DatLichKhamBenh.Data.Configurations;
using DatLichKhamBenh.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Khai báo các bảng
        public DbSet<BenhNhan> BenhNhans { get; set; }
        public DbSet<BacSi> BacSis { get; set; }
        public DbSet<PhongKham> PhongKhams { get; set; }
        public DbSet<LichKham> LichKhams { get; set; }
        public DbSet<ChuyenKhoa> ChuyenKhoas { get; set; }

        // Áp dụng cấu hình từ thư mục Configurations
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new BenhNhanConfiguration());
            modelBuilder.ApplyConfiguration(new BacSiConfiguration());
            modelBuilder.ApplyConfiguration(new LichKhamConfiguration());
            modelBuilder.ApplyConfiguration(new ChuyenKhoaConfiguration());
        }
    }
}
