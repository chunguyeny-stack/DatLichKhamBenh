using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatLichKhamBenh.Models.Entities
{
    public class BenhNhan
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        // Liên kết 1-1 với ApplicationUser
        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [StringLength(200)]
        [Display(Name = "dia_chi")]
        public string? Address { get; set; }

        [StringLength(10)]
        [Display(Name = "gioitinh")]
        public string? Gender { get; set; } // "Nam" / "Nữ" / "Khác"

        public virtual ICollection<LichHen> LichHens { get; set; } = new List<LichHen>();
    }
}