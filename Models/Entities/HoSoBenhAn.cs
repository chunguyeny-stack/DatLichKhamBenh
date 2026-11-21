using System.ComponentModel.DataAnnotations;

namespace MediSlot.Models.Entities
{
    public class HoSoBenhAn
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int LichHenId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập kết quả lâm sàng")]
        public string KetQuaLamSang { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập chẩn đoán")]
        public string ChanDoan { get; set; }
        public string? LoiDan { get; set; }
        public DateTime NgayKham { get; set; } = DateTime.Now;
        public virtual ICollection<DonThuoc> DanhSachThuoc { get; set; } = new List<DonThuoc>();
    }
}