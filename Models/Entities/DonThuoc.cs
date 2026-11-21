using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediSlot.Models.Entities
{
    public class DonThuoc
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int HoSoBenhAnId { get; set; }
        [Required(ErrorMessage = "Tên thuốc không được để trống")]
        public string TenThuoc { get; set; }
        public string? LieuLuong { get; set; }
        public int SoLuong { get; set; }
        [ForeignKey("HoSoBenhAnId")]
        public virtual HoSoBenhAn? HoSoBenhAn { get; set; }
    }
}