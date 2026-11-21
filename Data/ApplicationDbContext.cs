using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Models.Entities;

namespace DatLichKhamBenh.Data 
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cấu hình 1-1 cho Doctor
            builder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne(u => u.DoctorProfile)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình 1-1 cho Patient
            builder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.PatientProfile)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ Chuyên khoa - Bác sĩ
            builder.Entity<Doctor>()
                .HasOne(d => d.Specialty)
                .WithMany(s => s.Doctors)
                .HasForeignKey(d => d.SpecialtyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}