using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Models.ViewModels;
using DatLichKhamBenh.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DatLichKhamBenh.Services.Impl
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MedicalRecordService> _logger;
        public MedicalRecordService(ApplicationDbContext context, ILogger<MedicalRecordService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public bool LuuHoSoBenhAn(KhamBenhViewModel model)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var appointment = _context.Appointments.FirstOrDefault(a => a.Id == model.LichHenId);
                if (appointment == null || appointment.Status != "Đã đến")
                    return false;

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
                appointment.Status = "Hoàn thành";
                _context.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Lưu hồ sơ khám cho lịch hẹn {AppointmentId} thất bại.", model.LichHenId);
                return false;
            }
        }
    }
}