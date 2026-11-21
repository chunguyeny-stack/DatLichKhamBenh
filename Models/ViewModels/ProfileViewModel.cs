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
    }
}