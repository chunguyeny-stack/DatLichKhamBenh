using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatLichKhamBenh.Models.Entities
{
    [Table("Specialty")]
    public class Specialty
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên chuyên khoa không được để trống")]
        [Column("Name")]
        [MaxLength(200)]
        [Display(Name = "Tên chuyên khoa")]
        public string Name { get; set; } = string.Empty;

        [Column("Description")]
        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Column("CommonSymptoms")]
        public string? CommonSymptoms { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();

        public virtual ICollection<Service> Services { get; set; } = new List<Service>();

        [NotMapped]
        public int MaChuyenKhoa => Id;
    }
}