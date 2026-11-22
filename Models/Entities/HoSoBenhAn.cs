using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatLichKhamBenh.Models.Entities
{
    [Table("ho_so_benh_an")]
    public class HoSoBenhAn
    {
        [Key]
        public int Id { get; set; }
        public int LichHenId { get; set; }
        public string? KetQuaLamSang { get; set; }
        public string? ChanDoan { get; set; }
        public string? LoiDan { get; set; }
        
        // Navigation property
        public virtual ICollection<DonThuoc> DonThuocs { get; set; } = new List<DonThuoc>();
    }
}