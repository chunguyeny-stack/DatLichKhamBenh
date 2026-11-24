using System;
using System.ComponentModel.DataAnnotations;

namespace DatLichKhamBenh.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ đệm.")]
        [StringLength(100, ErrorMessage = "Họ đệm tối đa 100 ký tự.")]
        [Display(Name = "Họ đệm")]
        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên.")]
        [StringLength(100, ErrorMessage = "Tên tối đa 100 ký tự.")]
        [Display(Name = "Tên")]
        public string FirstName { get; set; } = string.Empty;

        // Giữ lại FullName đề phòng có code khác gọi tới
        public string FullName => $"{MiddleName} {FirstName}".Trim();

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ Gmail.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", 
            ErrorMessage = "Vui lòng dùng địa chỉ Gmail hợp lệ (ví dụ: ten@gmail.com).")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự.")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có tối thiểu 8 ký tự.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}