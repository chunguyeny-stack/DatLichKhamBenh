using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DatLichKhamBenh.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Editable(false)]
        [BindNever]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ đệm không được để trống")]
        [StringLength(50, ErrorMessage = "Họ đệm không được vượt quá 50 ký tự")]
        [Display(Name = "Họ đệm")]
        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên không được để trống")]
        [StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự")]
        [Display(Name = "Tên")]
        public string FirstName { get; set; } = string.Empty;

        [Editable(false)]
        [BindNever]
        [Display(Name = "Vai trò")]
        public string RoleName { get; set; } = string.Empty; // Chỉ dùng để hiển thị

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        // Chỉ dùng cho Bệnh nhân
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(100)]
        [Display(Name = "Học vị / Chức danh")]
        public string? Degree { get; set; }

        [Range(0, 60, ErrorMessage = "Số năm kinh nghiệm phải từ 0 đến 60")]
        [Display(Name = "Số năm kinh nghiệm")]
        public int YearsOfExperience { get; set; }

        [Range(typeof(decimal), "0", "100000000", ErrorMessage = "Giá khám không hợp lệ")]
        [Display(Name = "Giá khám gốc")]
        public decimal ConsultationFee { get; set; }

        [StringLength(200)]
        [Display(Name = "Nơi công tác")]
        public string? Workplace { get; set; }
    }
}