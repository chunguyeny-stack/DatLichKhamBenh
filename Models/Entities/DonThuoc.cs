using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatLichKhamBenh.Models.Entities
{
    [Table("don_thuoc")]
    public class DonThuoc
    {
        [Key]
        public int Id { get; set; }
        public int HoSoBenhAnId { get; set; }
        public string? TenThuoc { get; set; }
        public string? LieuLuong { get; set; }
        public int SoLuong { get; set; }

        // Navigation property
        [ForeignKey("HoSoBenhAnId")]
        public virtual HoSoBenhAn? HoSoBenhAn { get; set; }
    }
}