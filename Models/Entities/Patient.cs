using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatLichKhamBenh.Models.Entities
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }

        // Liên kết 1-1 với ApplicationUser
        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        // =====================================================
        // THÔNG TIN CÁ NHÂN (bổ sung ngoài ApplicationUser)
        // =====================================================

        [StringLength(200)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [StringLength(10)]
        [Display(Name = "Giới tính")]
        public string? Gender { get; set; } // "Nam" / "Nữ" / "Khác"

        [StringLength(20)]
        [Display(Name = "Số BHYT")]
        public string? HealthInsuranceNumber { get; set; }

        [StringLength(500)]
        [Display(Name = "Tiền sử bệnh")]
        public string? MedicalHistory { get; set; }

        [StringLength(100)]
        [Display(Name = "Liên hệ khẩn cấp")]
        public string? EmergencyContact { get; set; } // Tên + SĐT người thân

        // =====================================================
        // TRẠNG THÁI
        // =====================================================

        [Display(Name = "Ngày tạo hồ sơ")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        // =====================================================
        // NAVIGATION (mở rộng sau này)
        // =====================================================
        // public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}