using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DatLichKhamBenh.Models.Entities;

namespace DatLichKhamBenh.Data.Configurations
{
    public class BacSiConfiguration : IEntityTypeConfiguration<BacSi>
    {
        public void Configure(EntityTypeBuilder<BacSi> builder)
        {
            builder.ToTable("bac_si");
            
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
            
        }
    }
}
