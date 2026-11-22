namespace DatLichKhamBenh.Models.ViewModels
{
    public class KhamBenhViewModel
    {
        public int LichHenId { get; set; }
        public string? KetQuaLamSang { get; set; }
        public string? ChanDoan { get; set; }
        public string? LoiDan { get; set; }
        
        // Danh sách thuốc bác sĩ kê
        public List<ChiTietThuocViewModel>? DanhSachThuoc { get; set; }
    }

    public class ChiTietThuocViewModel
    {
        public string? TenThuoc { get; set; }
        public string? LieuLuong { get; set; }
        public int SoLuong { get; set; }
    }
}