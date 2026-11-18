using System.ComponentModel.DataAnnotations;

namespace DatLichKhamBenh.Models.ViewModels
{
    public class CreateStaffViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ đệm")]
        [Display(Name = "Họ đệm")]
        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên")]
        [Display(Name = "Tên")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public string Role { get; set; } = string.Empty; // "Doctor" hoặc "Receptionist"

        // Thêm trường này để Admin chọn chuyên khoa (chỉ dùng khi Role = "Doctor")
        [Display(Name = "Chuyên khoa")]
        public int? SpecialtyId { get; set; } 
    }
}