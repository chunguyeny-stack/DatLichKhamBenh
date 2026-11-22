using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Data.Context;

namespace DatLichKhamBenh.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    // [Authorize(Roles = "Receptionist")]
    public class dashboard_le_tan : Controller
    {
        private readonly ApplicationDbContext _context;

        public dashboard_le_tan(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .AsNoTracking()
                .Select(x => new { x.Status, x.TimeSlot })
                .ToListAsync();

            int tongLichHen = appointments.Count;

            int daHoanThanh = appointments.Count(x =>
            {
                var st = (x.Status ?? "").Trim().ToLower();
                return st == "hoàn thành" || st == "đã hoàn thành" || st == "đã thanh toán" || st == "completed";
            });

            int dangChoXuLy = appointments.Count(x =>
            {
                var st = (x.Status ?? "").Trim().ToLower();
                return st == "chờ" || st == "chờ xác nhận" || st == "đã xác nhận" || st == "đã đến"
                    || st == "chờ đổi lịch" || st.Contains("đổi lịch") || st.Contains("dời lịch")
                    || st == "waiting" || st == "pending";
            });

            int countSang = 0, countChieu = 0, countToi = 0;
            foreach (var appt in appointments)
            {
                var timeStr = appt.TimeSlot ?? "08:30-09:00";
                var hourPart = timeStr.Split('-', ':')[0].Trim();
                var startHour = int.TryParse(hourPart, out var h) ? h : 8;

                if (startHour < 12) countSang++;
                else if (startHour < 18) countChieu++;
                else countToi++;
            }

            ViewBag.ReceptionistName = User?.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(User.Identity.Name)
                ? User.Identity.Name
                : "Lễ tân";
            ViewBag.TongLichHen = tongLichHen;
            ViewBag.DaHoanThanh = daHoanThanh;
            ViewBag.DangChoXuLy = dangChoXuLy;
            ViewBag.CountSang = countSang;
            ViewBag.CountChieu = countChieu;
            ViewBag.CountToi = countToi;

            return View("~/Areas/Receptionist/Views/Home/doashkboard_le_tan.cshtml");
        }

        // Các action chuyển tiếp sang 4 controller còn lại trong Area Receptionist
        [HttpGet]
        public IActionResult QuanLyLichHen() => RedirectToAction("Index", "quan_ly_lich_hen_BN", new { area = "Receptionist" });

        [HttpGet]
        public IActionResult DanhSachCaTruc() => RedirectToAction("Index", "quan_ly_lich_lam_viec_BS", new { area = "Receptionist" });

        [HttpGet]
        public IActionResult VienPhi() => RedirectToAction("Index", "vien_phi", new { area = "Receptionist" });

        [HttpGet]
        public IActionResult HoSoCaNhan() => RedirectToAction("Index", "ho_so_ca_nhan_le_tan", new { area = "Receptionist" });
    }
}