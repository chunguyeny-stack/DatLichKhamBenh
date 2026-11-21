using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatLichKhamBenh.Models.Entities
{
    [Table("lich_lam_viec")]
    public class LichLamViec
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("le_tan_ma_le_tan")]
        public int? LeTanMaLeTan { get; set; }

        [Column("bac_si_id")]
        public int BacSiId { get; set; }

        [ForeignKey(nameof(BacSiId))]
        public virtual BacSi BacSi { get; set; } = null!;

        [Column("ngay_kham")]
        public DateTime NgayKham { get; set; }

        [Column("khung_gio")]
        public string KhungGio { get; set; } = string.Empty;

        [Column("con_trong")]
        public bool ConTrong { get; set; } = true;

        [Column("ly_do_ban")]
        public string LyDoBan { get; set; } = string.Empty;

        [Column("trang_thai_chi_tiet")]
        public string TrangThaiChiTiet { get; set; } = string.Empty;

        [NotMapped]
        public virtual BacSi? BacSiThongTin { get; set; }
    }
}