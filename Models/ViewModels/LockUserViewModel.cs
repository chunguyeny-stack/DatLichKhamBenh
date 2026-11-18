using System.ComponentModel.DataAnnotations;

namespace DatLichKhamBenh.Models.ViewModels
{
    public class LockUserViewModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lý do khóa tài khoản.")]
        [StringLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự.")]
        [Display(Name = "Lý do khóa")]
        public string Reason { get; set; } = string.Empty;
    }
}