using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedBooking.Models;

namespace MedBooking.Configurations
{
    public class ChuyenKhoaConfiguration : IEntityTypeConfiguration<ChuyenKhoa>
    {
        public void Configure(EntityTypeBuilder<ChuyenKhoa> builder)
        {
            builder.ToTable("ChuyenKhoa");
            
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.TenChuyenKhoa)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(c => c.MoTa)
                .HasMaxLength(500);
            
            // Quan hệ: 1-N
            builder.HasMany(c => c.BacSis)
                .WithOne(b => b.ChuyenKhoa)
                .HasForeignKey(b => b.ChuyenKhoaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
