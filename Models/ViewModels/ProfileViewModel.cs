using System;
using System.ComponentModel.DataAnnotations;

namespace DatLichKhamBenh.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string Email { get; set; }

        [Required(ErrorMessage = "Họ đệm không được để trống")]
        [Display(Name = "Họ đệm")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Tên không được để trống")]
        [Display(Name = "Tên")]
        public string FirstName { get; set; }

        public string RoleName { get; set; } // Hiển thị Bác sĩ / Bệnh nhân / Admin

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        // Chỉ dùng cho Bệnh nhân
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? DateOfBirth { get; set; }
    }
}