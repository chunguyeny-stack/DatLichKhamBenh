using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedBooking.Models;

namespace MedBooking.Configurations
{
    public class BacSiConfiguration : IEntityTypeConfiguration<BacSi>
    {
        public void Configure(EntityTypeBuilder<BacSi> builder)
        {
            builder.ToTable("BacSi");
            
            builder.HasKey(b => b.Id);
            
            builder.Property(b => b.HoTen)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(b => b.SoDienThoai)
                .HasMaxLength(20);
            
            builder.Property(b => b.Email)
                .HasMaxLength(100);
            
            // Quan hệ: N-1 với ChuyenKhoa
            builder.HasOne(b => b.ChuyenKhoa)
                .WithMany(c => c.BacSis)
                .HasForeignKey(b => b.ChuyenKhoaId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            
            // Quan hệ: 1-N với LichKham
            builder.HasMany(b => b.LichKhams)
                .WithOne(l => l.BacSi)
                .HasForeignKey(l => l.BacSiId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
