using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatLichKhamBenh.Models.Entities
{
    [Table("lich_hen")]
    public class LichHen
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("benh_nhan_id")]
        public int BenhNhanId { get; set; }
        [ForeignKey(nameof(BenhNhanId))]
        public virtual BenhNhan? BenhNhan { get; set; }

        [Column("bac_si_id")]
        public int? BacSiId { get; set; }
        [ForeignKey(nameof(BacSiId))]
        public virtual BacSi? BacSiThongTin { get; set; }

        [Column("dich_vu_id")]
        public int DichVuId { get; set; }

        [Column("ngay_kham")]
        public DateTime NgayKham { get; set; }

        [Column("khung_gio")]
        public string KhungGio { get; set; } = string.Empty;

        [Column("trang_thai")]
        public string TrangThai { get; set; } = "Chờ xác nhận";

        [Column("ghi_chu")]
        public string? GhiChu { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("trieu_chung_thuc_the")]
        public string? TrieuChungThucThe { get; set; }

        [Column("chan_doan")]
        public string? ChanDoan { get; set; }

        [Column("loi_dan")]
        public string? LoiDan { get; set; }

        [Column("don_thuoc")]
        public string? DonThuoc { get; set; }

        [Column("thiet_bi_y_te")]
        public string? ThietBiYTe { get; set; }

        [NotMapped]
        public int MaLichHen => Id;
    }
}