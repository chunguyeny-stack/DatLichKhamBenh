using System.ComponentModel.DataAnnotations;

namespace DatLichKhamBenh.Models.Entities
{
    public class Specialty
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên chuyên khoa không được để trống")]
        [StringLength(100)]
        [Display(Name = "Tên chuyên khoa")]
        public string Name { get; set; } = null!; // Ví dụ: "Tim mạch", "Nhi khoa"

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        // Navigation property: Một chuyên khoa có nhiều bác sĩ
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}