using DatLichKhamBenh.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatLichKhamBenh.Data.Configurations
{
    public class BacSiConfiguration : IEntityTypeConfiguration<BacSi>
    {
        public void Configure(EntityTypeBuilder<BacSi> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.HoTen)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(b => b.ChuyenMon)
                .HasMaxLength(50);

            builder.Property(b => b.SoDienThoai)
                .HasMaxLength(15);

            builder.HasOne(b => b.PhongKham)
                .WithMany(p => p.BacSis)
                .HasForeignKey(b => b.PhongKhamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("BacSis");
        }
    }
}
