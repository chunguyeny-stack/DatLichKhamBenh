using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DatLichKhamBenh.Models.Entities;

namespace DatLichKhamBenh.Data.Configurations
{
    public class BenhNhanConfiguration : IEntityTypeConfiguration<BenhNhan>
    {
        public void Configure(EntityTypeBuilder<BenhNhan> builder)
        {
            builder.ToTable("benh_nhan");
            
            builder.HasKey(bn => bn.Id);
            
            builder.Property(p => p.UserId).IsRequired();
            builder.Property(p => p.Address).HasMaxLength(200);
            builder.Property(p => p.Gender).HasMaxLength(10);
            builder.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
