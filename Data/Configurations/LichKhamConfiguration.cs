using DatLichKhamBenh.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatLichKhamBenh.Data.Configurations
{
    public class LichKhamConfiguration : IEntityTypeConfiguration<LichKham>
    {
        public void Configure(EntityTypeBuilder<LichKham> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.NgayKham)
                .IsRequired();

            builder.Property(l => l.GioKham)
                .IsRequired();

            builder.Property(l => l.GhiChu)
                .HasMaxLength(500);

            builder.Property(l => l.TrangThai)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Chờ xác nhận");

            builder.HasOne(l => l.BenhNhan)
                .WithMany(b => b.LichKhams)
                .HasForeignKey(l => l.BenhNhanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(l => l.BacSi)
                .WithMany(b => b.LichKhams)
                .HasForeignKey(l => l.BacSiId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("LichKhams");
        }
    }
}
