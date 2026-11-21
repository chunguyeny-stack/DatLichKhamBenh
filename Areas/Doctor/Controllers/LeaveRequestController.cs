using Microsoft.AspNetCore.Mvc;

namespace MediSlot.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    public class LeaveRequestController : Controller
    {
        public IActionResult Index() { return View(); }

        [HttpPost]
        public IActionResult Create(DateTime NgayNghi, string CaNghi, string LyDo)
        {
            // Tạm thời chỉ hiện thông báo thành công
            TempData["SuccessMessage"] = "Đã gửi đơn xin nghỉ phép thành công!";
            return RedirectToAction("Index");
        }
    }
}