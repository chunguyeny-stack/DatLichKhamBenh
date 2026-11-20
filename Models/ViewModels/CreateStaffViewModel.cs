using System.ComponentModel.DataAnnotations;

namespace DatLichKhamBenh.Models.ViewModels
{
    public class CreateStaffViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Vui lòng nhập họ đệm")]
        [StringLength(50, ErrorMessage = "Họ đệm không được vượt quá 50 ký tự")]
        [Display(Name = "Họ đệm")]
        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên")]
        [StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự")]
        [Display(Name = "Tên")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Chuyên khoa")]
        public int? SpecialtyId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Role == "Doctor" && (SpecialtyId == null || SpecialtyId <= 0))
            {
                yield return new ValidationResult(
                    "Bác sĩ phải được gán ít nhất một chuyên khoa.",
                    new[] { nameof(SpecialtyId) });
            }

            if (Role == "Receptionist" && SpecialtyId != null && SpecialtyId > 0)
            {
                yield return new ValidationResult(
                    "Lễ tân không cần chọn chuyên khoa.",
                    new[] { nameof(SpecialtyId) });
            }
        }
    }
}