using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatLichKhamBenh.Models.Entities
{
        [Table("Patient")]
        public class Patient
        {
                [Key]
                [Column("Id")]
                public int Id { get; set; }

                // Liên kết 1 - 1 với AspNetUsers (cho phép null nếu khách đặt lịch trực tiếp tại quầy chưa tạo tài khoản)
                [Column("UserId")]
                [MaxLength(450)]
                public string? UserId { get; set; }

                [ForeignKey(nameof(UserId))]
                public virtual ApplicationUser? User { get; set; }

                [Required]
                [Column("FullName")]
                [MaxLength(200)]
                [Display(Name = "Họ và tên")]
                public string FullName { get; set; } = string.Empty;

                [Column("PhoneNumber")]
                [MaxLength(20)]
                [Display(Name = "Số điện thoại")]
                public string? PhoneNumber { get; set; }

                [Column("Email")]
                [MaxLength(256)]
                [Display(Name = "Email")]
                public string? Email { get; set; }

                [Column("DateOfBirth", TypeName = "date")]
                [Display(Name = "Ngày sinh")]
                public DateTime? DateOfBirth { get; set; }

                [Column("Address")]
                [StringLength(200)]
                [Display(Name = "Địa chỉ")]
                public string? Address { get; set; }

                [Column("Gender")]
                [StringLength(10)]
                [Display(Name = "Giới tính")]
                public string? Gender { get; set; } // "Nam" / "Nữ" / "Khác"

                [Column("HealthInsuranceNumber")]
                [StringLength(20)]
                [Display(Name = "Số BHYT")]
                public string? HealthInsuranceNumber { get; set; }

                [Column("MedicalHistory")]
                [StringLength(500)]
                [Display(Name = "Tiền sử bệnh")]
                public string? MedicalHistory { get; set; }

                [Column("EmergencyContact")]
                [StringLength(100)]
                [Display(Name = "Liên hệ khẩn cấp")]
                public string? EmergencyContact { get; set; } // Tên + SĐT người thân

                [Column("CreatedAt")]
                [Display(Name = "Ngày tạo hồ sơ")]
                public DateTime CreatedAt { get; set; } = DateTime.Now;

                [Column("IsActive")]
                [Display(Name = "Đang hoạt động")]
                public bool IsActive { get; set; } = true;

                public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

                [NotMapped]
                public int MaBenhNhan => Id;
        }
}