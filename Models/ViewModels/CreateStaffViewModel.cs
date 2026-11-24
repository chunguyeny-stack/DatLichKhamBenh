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

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Range(0, 60, ErrorMessage = "Số năm kinh nghiệm phải từ 0 đến 60")]
        [Display(Name = "Số năm kinh nghiệm")]
        public int YearsOfExperience { get; set; }

        [StringLength(200, ErrorMessage = "Nơi công tác không được vượt quá 200 ký tự")]
        [Display(Name = "Nơi công tác")]
        public string? Workplace { get; set; }

        [Range(typeof(decimal), "0", "100000000", ErrorMessage = "Giá khám gốc không hợp lệ")]
        [Display(Name = "Giá khám gốc")]
        public decimal ConsultationFee { get; set; } = 200000m;

        [StringLength(100, ErrorMessage = "Học vị không được vượt quá 100 ký tự")]
        [Display(Name = "Học vị / Chức danh")]
        public string? Degree { get; set; }

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
                yield break;
            }

            if (Role == "Doctor")
            {
                if (string.IsNullOrWhiteSpace(PhoneNumber))
                    yield return new ValidationResult("Bác sĩ phải có số điện thoại.", new[] { nameof(PhoneNumber) });
                if (YearsOfExperience < 0 || YearsOfExperience > 60)
                    yield return new ValidationResult("Số năm kinh nghiệm phải từ 0 đến 60.", new[] { nameof(YearsOfExperience) });
                if (string.IsNullOrWhiteSpace(Workplace))
                    yield return new ValidationResult("Bác sĩ phải nhập nơi công tác.", new[] { nameof(Workplace) });
                if (ConsultationFee < 0)
                    yield return new ValidationResult("Giá khám gốc không được âm.", new[] { nameof(ConsultationFee) });
            }

            if (Role != "Doctor" && SpecialtyId != null && SpecialtyId > 0)
            {
                yield return new ValidationResult("Chỉ Bác sĩ mới được gán chuyên khoa.", new[] { nameof(SpecialtyId) });
            }
        }
    }
}