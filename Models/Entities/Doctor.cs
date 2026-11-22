using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatLichKhamBenh.Models.Entities
{
    [Table("Doctor")]
    public class Doctor
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("UserId")]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        [Column("SpecialtyId")]
        [Required]
        [Display(Name = "Chuyên khoa")]
        public int SpecialtyId { get; set; }

        [ForeignKey(nameof(SpecialtyId))]
        public virtual Specialty? Specialty { get; set; }

        [Required]
        [Column("FullName")]
        [MaxLength(200)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Column("Degree")]
        [MaxLength(100)]
        [Display(Name = "Học vị / Chức danh")]
        public string? Degree { get; set; }

        [Column("Bio")]
        [StringLength(1000)]
        [Display(Name = "Giới thiệu")]
        public string? Bio { get; set; }

        [Column("YearsOfExperience")]
        [Range(0, 60, ErrorMessage = "Số năm kinh nghiệm không hợp lệ")]
        [Display(Name = "Số năm kinh nghiệm")]
        public int YearsOfExperience { get; set; }

        [Column("Workplace")]
        [StringLength(200)]
        [Display(Name = "Nơi công tác")]
        public string? Workplace { get; set; }

        [Column("ConsultationFee", TypeName = "decimal(18, 2)")]
        [Display(Name = "Phí khám")]
        public decimal ConsultationFee { get; set; }

        [Column("IsActive")]
        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public virtual ICollection<DoctorSchedule> DoctorSchedules { get; set; } = new List<DoctorSchedule>();

        [NotMapped]
        public int MaBacSi => Id;
    }
}