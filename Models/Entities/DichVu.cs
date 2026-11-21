using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatLichKhamBenh.Models.Entities
{
    [Table("dich_vu")]
    public class DichVu
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("chuyen_khoa_id")]
        public int ChuyenKhoaId { get; set; }
        [ForeignKey("ChuyenKhoaId")]
        public virtual ChuyenKhoa ChuyenKhoa { get; set; }

        [Column("ten_dich_vu")]
        public string TenDichVu { get; set; }

        [Column("gia_tien")]
        public decimal GiaTien { get; set; }

        [Column("mo_ta")]
        public string MoTa { get; set; }

        [Column("dang_hoat_dong")]
        public bool DangHoatDong { get; set; } = true;

        [Column("trang_thai")]
        public string TrangThai { get; set; } = "active";
    }
}