using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace DatLichKhamBenh.Models.Entities
{
    // Kế thừa IdentityUser để có sẵn: Id, UserName, Email, PasswordHash, PhoneNumber...
    // Bổ sung các trường custom theo ERD
    public class ApplicationUser : IdentityUser
    {
        [StringLength(50)]
        [Display(Name = "Họ đệm")]
        public string? MiddleName { get; set; }

        [StringLength(50)]
        [Display(Name = "Tên")]
        public string? FirstName { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        // Cờ bắt buộc đổi mật khẩu ở lần đăng nhập đầu tiên
        // (Dùng khi Admin tạo tài khoản BS/LT với mật khẩu mặc định)
        [Display(Name = "Phải đổi mật khẩu")]
        public bool MustChangePassword { get; set; } = false;

        // Lý do khóa tài khoản (do Admin nhập khi khóa)
        [StringLength(500)]
        [Display(Name = "Lý do khóa")]
        public string? LockoutReason { get; set; }

        // Navigation property (tùy chọn - nếu muốn truy cập ngược)
        public BacSi? DoctorProfile { get; set; }
        public BenhNhan? PatientProfile { get; set; }

        // Helper: Họ tên đầy đủ
        public string FullName => $"{MiddleName} {FirstName}".Trim();

        [NotMapped]
        public virtual ICollection<BacSi> BacSis { get; set; } = new List<BacSi>();
    }
}