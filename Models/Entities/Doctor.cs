using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatLichKhamBenh.Models.Entities
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        // Liên kết 1-1 với ApplicationUser
        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        // =====================================================
        // LIÊN KẾT CHUYÊN KHOA (MỚI)
        // =====================================================
        [Display(Name = "Chuyên khoa")]
        [Required]
        public int SpecialtyId { get; set; }

        [ForeignKey(nameof(SpecialtyId))]
        public Specialty Specialty { get; set; } = null!;

        // =====================================================
        // THÔNG TIN CHUYÊN MÔN (Giữ nguyên)
        // =====================================================
        [StringLength(100)]
        [Display(Name = "Học vị / Chức danh")]
        public string? Degree { get; set;}

        [StringLength(1000)]
        [Display(Name = "Giới thiệu")]
        public string? Bio { get; set; }

        [Display(Name = "Số năm kinh nghiệm")]
        [Range(0, 60, ErrorMessage = "Số năm kinh nghiệm không hợp lệ")]
        public int? YearsOfExperience { get; set; }

        [StringLength(200)]
        [Display(Name = "Nơi công tác")]
        public string? Workplace { get; set; }

        // =====================================================
        // TRẠNG THÁI
        // =====================================================
        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}