using DatLichKhamBenh.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatLichKhamBenh.Data.Configurations
{
    public class BenhNhanConfiguration : IEntityTypeConfiguration<BenhNhan>
    {
        public void Configure(EntityTypeBuilder<BenhNhan> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.HoTen)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(b => b.SoDienThoai)
                .IsRequired()
                .HasMaxLength(15);

            builder.Property(b => b.Email)
                .HasMaxLength(100);

            builder.Property(b => b.DiaChi)
                .HasMaxLength(200);

            builder.Property(b => b.GioiTinh)
                .HasMaxLength(10);

            builder.ToTable("BenhNhans");
        }
    }
}
