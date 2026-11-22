using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Models.Entities;

namespace DatLichKhamBenh.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

        public DbSet<BacSi> BacSis { get; set; }
        public DbSet<BenhNhan> BenhNhans { get; set; }
        public DbSet<ChuyenKhoa> ChuyenKhoas { get; set; }
        public DbSet<DichVu> DichVus { get; set; }
        public DbSet<LichHen> LichHens { get; set; }
        public DbSet<LichLamViec> LichLamViecs { get; set; }

        public DbSet<HoSoBenhAn> HoSoBenhAns { get; set; }
        public DbSet<DonThuoc> DonThuocs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers");
            modelBuilder.Entity<Specialty>().ToTable("Specialty");
            modelBuilder.Entity<Service>().ToTable("Service");
            modelBuilder.Entity<Doctor>().ToTable("Doctor");
            modelBuilder.Entity<Patient>().ToTable("Patient");
            modelBuilder.Entity<DoctorSchedule>().ToTable("DoctorSchedule");

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.DoctorProfile)
                .WithOne(d => d.User)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.PatientProfile)
                .WithOne(p => p.User)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Specialty)
                .WithMany(s => s.Doctors)
                .HasForeignKey(d => d.SpecialtyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Service>()
                .HasOne(sv => sv.Specialty)
                .WithMany(s => s.Services)
                .HasForeignKey(sv => sv.SpecialtyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorSchedule>()
                .HasOne(ds => ds.Doctor)
                .WithMany(d => d.DoctorSchedules)
                .HasForeignKey(ds => ds.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorSchedule>()
                .HasIndex(ds => new { ds.DoctorId, ds.WorkDate, ds.TimeSlot })
                .IsUnique();

            modelBuilder.Entity<Service>().Property(sv => sv.Price).HasPrecision(18, 2);
            modelBuilder.Entity<Doctor>().Property(d => d.ConsultationFee).HasPrecision(18, 2);
            modelBuilder.Entity<Appointment>().Property(a => a.ConsultationFee).HasPrecision(18, 2);
            modelBuilder.Entity<Appointment>().Property(a => a.MedicineFee).HasPrecision(18, 2);
            modelBuilder.Entity<Appointment>().Property(a => a.ServiceFee).HasPrecision(18, 2);

            modelBuilder.Entity<BacSi>().ToTable("bac_si");
            modelBuilder.Entity<BenhNhan>().ToTable("benh_nhan");
            modelBuilder.Entity<ChuyenKhoa>().ToTable("chuyen_khoa");
            modelBuilder.Entity<DichVu>().ToTable("dich_vu");
            modelBuilder.Entity<LichHen>().ToTable("lich_hen");
            modelBuilder.Entity<LichLamViec>().ToTable("lich_lam_viec");

            modelBuilder.Entity<HoSoBenhAn>().ToTable("ho_so_benh_an");
            modelBuilder.Entity<DonThuoc>().ToTable("don_thuoc");
        }
    }
}