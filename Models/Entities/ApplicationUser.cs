using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace DatLichKhamBenh.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Column("FullName")]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [NotMapped]
        public string MiddleName
        {
            get
            {
                var parts = (FullName ?? string.Empty).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 1 ? string.Join(" ", parts[..^1]) : string.Empty;
            }
            set
            {
                var mid = (value ?? string.Empty).Trim();
                var first = FirstName;
                FullName = string.IsNullOrWhiteSpace(mid) ? first : $"{mid} {first}".Trim();
            }
        }

        [NotMapped]
        [StringLength(50)]
        [Display(Name = "Tên")]
        public string FirstName
        {
            get
            {
                var parts = (FullName ?? string.Empty).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 0 ? parts[^1] : string.Empty;
            }
            set
            {
                var first = (value ?? string.Empty).Trim();
                var mid = MiddleName;
                FullName = string.IsNullOrWhiteSpace(mid) ? first : $"{mid} {first}".Trim();
            }
        }

        [NotMapped]
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [NotMapped]
        [StringLength(500)]
        [Display(Name = "Lý do khóa")]
        public string? LockoutReason { get; set; }

        [NotMapped]
        [Display(Name = "Phải đổi mật khẩu")]
        public bool MustChangePassword { get; set; }

        public virtual Doctor? DoctorProfile { get; set; }

        public virtual Patient? PatientProfile { get; set; }
    }
}