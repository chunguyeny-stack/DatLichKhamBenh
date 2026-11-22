using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedBooking.Models;

namespace MedBooking.Configurations
{
    public class LichKhamConfiguration : IEntityTypeConfiguration<LichKham>
    {
        public void Configure(EntityTypeBuilder<LichKham> builder)
        {
            builder.ToTable("LichKham");
            
            builder.HasKey(l => l.Id);
            
            builder.Property(l => l.NgayKham)
                .IsRequired();
            
            builder.Property(l => l.GioKham)
                .IsRequired();
            
            builder.Property(l => l.TrieuChung)
                .HasMaxLength(500);
            
            builder.Property(l => l.TrangThai)
                .IsRequired()
                .HasMaxLength(50);
            
            // Quan hệ: N-1 với BacSi
            builder.HasOne(l => l.BacSi)
                .WithMany(b => b.LichKhams)
                .HasForeignKey(l => l.BacSiId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            
            // Quan hệ: N-1 với BenhNhan
            builder.HasOne(l => l.BenhNhan)
                .WithMany(bn => bn.LichKhams)
                .HasForeignKey(l => l.BenhNhanId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

            
      
