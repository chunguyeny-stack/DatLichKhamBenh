using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Models.Entities; // Thống nhất dùng namespace này theo code của bạn kia

namespace DatLichKhamBenh.Data.Context
{
    // Kế thừa IdentityDbContext để hỗ trợ đăng nhập (Tuần 8)
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- BẢNG CỦA BẠN KIA ---
        public DbSet<BacSi> BacSis { get; set; }
        public DbSet<BenhNhan> BenhNhans { get; set; }
        public DbSet<ChuyenKhoa> ChuyenKhoas { get; set; }
        public DbSet<DichVu> DichVus { get; set; }
        public DbSet<LichHen> LichHens { get; set; }
        public DbSet<LichLamViec> LichLamViecs { get; set; }

        // --- BẢNG CỦA BẠN (Luồng 3) ---
        public DbSet<HoSoBenhAn> HoSoBenhAns { get; set; }
        public DbSet<DonThuoc> DonThuocs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình tên bảng của bạn kia
            modelBuilder.Entity<BacSi>().ToTable("bac_si");
            modelBuilder.Entity<BenhNhan>().ToTable("benh_nhan");
            modelBuilder.Entity<ChuyenKhoa>().ToTable("chuyen_khoa");
            modelBuilder.Entity<DichVu>().ToTable("dich_vu");
            modelBuilder.Entity<LichHen>().ToTable("lich_hen");
            modelBuilder.Entity<LichLamViec>().ToTable("lich_lam_viec");

            // Bạn có thể thêm cấu hình tên bảng cho bảng của bạn ở đây (nếu muốn viết thường)
            // modelBuilder.Entity<HoSoBenhAn>().ToTable("ho_so_benh_an");
            // modelBuilder.Entity<DonThuoc>().ToTable("don_thuoc");
        }
    }
}