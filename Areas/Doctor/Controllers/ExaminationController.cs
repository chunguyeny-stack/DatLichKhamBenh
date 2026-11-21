using Microsoft.AspNetCore.Mvc;
using MediSlot.Models.ViewModels;
using MediSlot.Services.Interfaces;

namespace MediSlot.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    public class ExaminationController : Controller
    {
        private readonly IMedicalRecordService _service;
        
        public ExaminationController(IMedicalRecordService service) 
        { 
            _service = service; 
        }

        // GET: Hiển thị form khám bệnh
        public IActionResult Index(int id)
        {
            // Nếu không có id truyền vào, mặc định gán là 1 để tránh lỗi
            if (id == 0) id = 1; 
            
            var model = new KhamBenhViewModel { LichHenId = id };
            return View(model);
        }

        // POST: Lưu kết quả khám
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Complete(KhamBenhViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_service.LuuHoSoBenhAn(model))
                {
                    TempData["SuccessMessage"] = "Đã hoàn thành ca khám và lưu hồ sơ thành công!";
                    
                    // Chuyển hướng về lại trang Khám bệnh (sau này bạn có thể đổi thành trang Danh sách)
                    return RedirectToAction("Index", "Examination", new { area = "Doctor" });
                }
                
                ModelState.AddModelError("", "Có lỗi xảy ra khi lưu vào Database. Vui lòng thử lại.");
            }
            
            // Nếu lỗi validate, trả lại form kèm dữ liệu đã nhập
            return View("Index", model);
        }
    }
}