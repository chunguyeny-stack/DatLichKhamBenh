using DatLichKhamBenh.Models.ViewModels;

namespace DatLichKhamBenh.Services.Interfaces
{
    public interface IMedicalRecordService
    {
        bool LuuHoSoBenhAn(KhamBenhViewModel model);
    }
}