using DatLichKhamBenh.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatLichKhamBenh.Data.Configurations
{
    public class ChuyenKhoaConfiguration : IEntityTypeConfiguration<ChuyenKhoa>
    {
        public void Configure(EntityTypeBuilder<ChuyenKhoa> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.TenChuyenKhoa)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.MoTa)
                .HasMaxLength(500);

            builder.ToTable("ChuyenKhoas");
        }
    }
}
