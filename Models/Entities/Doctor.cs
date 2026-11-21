using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatLichKhamBenh.Models.Entities
{
    [Table("bac_si")]
    public class BacSi
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("nguoi_dung_id")]
        public int NguoiDungId { get; set; }

        [Column("application_user_id")]
        public string? ApplicationUserId { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        public virtual ApplicationUser? ApplicationUser { get; set; }

        [Column("chuyen_khoa_id")]
        public int ChuyenKhoaId { get; set; }

        [ForeignKey(nameof(ChuyenKhoaId))]
        public virtual ChuyenKhoa? ChuyenKhoa { get; set; }

        [Column("hoc_vi")]
        public string? HocVi { get; set; }

        [Column("kinh_nghiem")]
        public string? KinhNghiem { get; set; }

        [Column("gia_kham")]
        public decimal GiaKham { get; set; }

        [Column("rating")]
        public double Rating { get; set; }

        [Column("so_luong_danh_gia")]
        public int SoLuongDanhGia { get; set; }

        [Column("ho_ten")]
        public string HoTen { get; set; } = string.Empty;

        [Column("so_dien_thoai")]
        public string? SoDienThoai { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("hinh_anh")]
        public string? HinhAnh { get; set; }

        [NotMapped]
        public virtual ICollection<LichLamViec> LichLamViecs { get; set; } = new List<LichLamViec>();

        [NotMapped]
        public virtual ICollection<LichHen> LichHens { get; set; } = new List<LichHen>();
    }
}