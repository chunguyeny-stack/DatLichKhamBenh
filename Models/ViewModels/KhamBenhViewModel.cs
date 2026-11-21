using System.ComponentModel.DataAnnotations;

namespace MediSlot.Models.ViewModels
{
    public class KhamBenhViewModel
    {
        public int LichHenId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập kết quả lâm sàng")]
        public string KetQuaLamSang { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập chẩn đoán")]
        public string ChanDoan { get; set; }
        public string? LoiDan { get; set; }
        public List<DonThuocInput> DanhSachThuoc { get; set; } = new List<DonThuocInput>();
    }
    public class DonThuocInput
    {
        public string TenThuoc { get; set; }
        public string? LieuLuong { get; set; }
        public int SoLuong { get; set; }
    }
}