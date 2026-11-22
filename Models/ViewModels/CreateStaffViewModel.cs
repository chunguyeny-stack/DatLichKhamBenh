using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DatLichKhamBenh.Models.ViewModels
{
    public class CreateStaffViewModel : IValidatableObject
    {
        public static readonly string[] AllowedRoles = { "Doctor", "Receptionist", "Admin" };
        public const string StaffEmailDomain = "medislot.com";

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
            var email = Email?.Trim() ?? string.Empty;
            if (!email.EndsWith($"@{StaffEmailDomain}", StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult(
                    $"Email nhân sự phải sử dụng miền @{StaffEmailDomain}.",
                    new[] { nameof(Email) });
            }

            if (!AllowedRoles.Contains(Role))
            {
                yield return new ValidationResult(
                    "Vai trò được chọn không hợp lệ.",
                    new[] { nameof(Role) });
                yield break;
            }

            if (Role == "Doctor" && (SpecialtyId == null || SpecialtyId <= 0))
            {
                yield return new ValidationResult(
                    "Bác sĩ phải được gán ít nhất một chuyên khoa.",
                    new[] { nameof(SpecialtyId) });
            }

            if (Role != "Doctor" && SpecialtyId != null && SpecialtyId > 0)
            {
                yield return new ValidationResult("Chỉ Bác sĩ mới được gán chuyên khoa.", new[] { nameof(SpecialtyId) });
            }
        }
    }
}