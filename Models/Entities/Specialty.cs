using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatLichKhamBenh.Models.Entities
{
    [Table("chuyen_khoa")]
    public class ChuyenKhoa
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("ten_chuyen_khoa")]
        public string TenChuyenKhoa { get; set; } = string.Empty;

        [Column("mo_ta")]
        public string? MoTa { get; set; }

        [Column("trang_thai")]
        public string TrangThai { get; set; } = "active";

        [Column("ly_do_xoa")]
        public string? LyDoXoa { get; set; }

        public virtual ICollection<BacSi> BacSis { get; set; } = new List<BacSi>();
    }
}