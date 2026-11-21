using Microsoft.EntityFrameworkCore;
using MediSlot.Models.Entities; // Đổi namespace nếu cần

namespace MediSlot.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Thêm 2 dòng này:
        public DbSet<HoSoBenhAn> HoSoBenhAns { get; set; }
        public DbSet<DonThuoc> DonThuocs { get; set; }
    }
}