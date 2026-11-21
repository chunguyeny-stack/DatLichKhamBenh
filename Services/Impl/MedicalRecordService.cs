using MediSlot.Data;
using MediSlot.Models.Entities;
using MediSlot.Models.ViewModels;
using MediSlot.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediSlot.Services.Impl
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly ApplicationDbContext _context;
        public MedicalRecordService(ApplicationDbContext context) { _context = context; }

        public bool LuuHoSoBenhAn(KhamBenhViewModel model)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var hoSo = new HoSoBenhAn
                {
                    LichHenId = model.LichHenId,
                    KetQuaLamSang = model.KetQuaLamSang,
                    ChanDoan = model.ChanDoan,
                    LoiDan = model.LoiDan
                };
                _context.HoSoBenhAns.Add(hoSo);
                _context.SaveChanges();

                if (model.DanhSachThuoc != null)
                {
                    foreach (var t in model.DanhSachThuoc)
                    {
                        _context.DonThuocs.Add(new DonThuoc {
                            HoSoBenhAnId = hoSo.Id, TenThuoc = t.TenThuoc, LieuLuong = t.LieuLuong, SoLuong = t.SoLuong
                        });
                    }
                }
                _context.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch { transaction.Rollback(); return false; }
        }
    }
}