using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedBooking.Models;

namespace MedBooking.Configurations
{
    public class BenhNhanConfiguration : IEntityTypeConfiguration<BenhNhan>
    {
        public void Configure(EntityTypeBuilder<BenhNhan> builder)
        {
            builder.ToTable("BenhNhan");
            
            builder.HasKey(bn => bn.Id);
            
            builder.Property(bn => bn.HoTen)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(bn => bn.GioiTinh)
                .IsRequired()
                .HasMaxLength(10);
            
            builder.Property(bn => bn.SoDienThoai)
                .HasMaxLength(20);
            
            builder.Property(bn => bn.DiaChi)
                .HasMaxLength(255);
            
            // Quan hệ: 1-N với LichKham
            builder.HasMany(bn => bn.LichKhams)
                .WithOne(l => l.BenhNhan)
                .HasForeignKey(l => l.BenhNhanId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
