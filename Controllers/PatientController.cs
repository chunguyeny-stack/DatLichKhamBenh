using System;
using System.Linq;
using System.Threading.Tasks;
using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Controllers
{
    [Authorize]
    public class PatientController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager) : Controller
    {
        // Quản lý lịch hẹn cá nhân: Xem danh sách (Chờ duyệt / Đã khám / Đã hủy)
        [HttpGet]
        public async Task<IActionResult> Appointments(string? trangThaiLoc)
        {
            var userId = userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var benhNhan = await context.BenhNhans
                .Include(bn => bn.User)
                .FirstOrDefaultAsync(bn => bn.UserId == userId);

            if (benhNhan == null)
            {
                benhNhan = new BenhNhan { UserId = userId };
                context.BenhNhans.Add(benhNhan);
                await context.SaveChangesAsync();
                benhNhan = await context.BenhNhans
                    .Include(bn => bn.User)
                    .FirstAsync(bn => bn.Id == benhNhan.Id);
            }

            var query = context.LichHens
                .AsNoTracking()
                .Include(l => l.BacSiThongTin)
                .ThenInclude(b => b!.ChuyenKhoa)
                .Where(l => l.BenhNhanId == benhNhan.Id);

            if (!string.IsNullOrWhiteSpace(trangThaiLoc) && trangThaiLoc != "TatCa")
            {
                query = trangThaiLoc switch
                {
                    "ChoDuyet" => query.Where(l =>
                        l.TrangThai == "Chờ duyệt" || l.TrangThai == "Chờ xác nhận" || l.TrangThai == "ChoXacNhan"),
                    "DaKham" => query.Where(l =>
                        l.TrangThai == "Đã khám" || l.TrangThai == "Hoàn thành" || l.TrangThai == "HoanThanh"),
                    "DaHuy" => query.Where(l => l.TrangThai == "Đã hủy" || l.TrangThai == "DaHuy"),
                    _ => query.Where(l => l.TrangThai == trangThaiLoc)
                };
            }

            var lichHens = await query
                .OrderByDescending(l => l.NgayKham)
                .ThenByDescending(l => l.Id)
                .ToListAsync();

            ViewBag.BenhNhan = benhNhan;
            ViewBag.TrangThaiLoc = string.IsNullOrWhiteSpace(trangThaiLoc) ? "TatCa" : trangThaiLoc;

            return View(lichHens);
        }

        // Hủy lịch (trước 24h)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var userId = userManager.GetUserId(User);
            var benhNhan = await context.BenhNhans.FirstOrDefaultAsync(bn => bn.UserId == userId);
            if (benhNhan == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var lichHen = await context.LichHens
                .FirstOrDefaultAsync(l => l.Id == id && l.BenhNhanId == benhNhan.Id);

            if (lichHen == null)
            {
                TempData["Error"] = "Không tìm thấy lịch hẹn.";
                return RedirectToAction(nameof(Appointments));
            }

            if ((lichHen.NgayKham.Date - DateTime.Today).TotalHours < 24)
            {
                TempData["Error"] = "Chỉ có thể hủy lịch hẹn trước thời điểm khám ít nhất 24 giờ.";
                return RedirectToAction(nameof(Appointments));
            }

            lichHen.TrangThai = "Đã hủy";
            await context.SaveChangesAsync();

            TempData["Success"] = $"Đã hủy lịch hẹn #{id}.";
            return RedirectToAction(nameof(Appointments));
        }

        // Xác nhận đổi lịch khi BS nghỉ đột xuất
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReschedule(int id, DateTime ngayMoi, CaKham caMoi, string? khungGioMoi)
        {
            var userId = userManager.GetUserId(User);
            var benhNhan = await context.BenhNhans.FirstOrDefaultAsync(bn => bn.UserId == userId);
            if (benhNhan == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var lichHen = await context.LichHens
                .FirstOrDefaultAsync(l => l.Id == id && l.BenhNhanId == benhNhan.Id);

            if (lichHen == null)
            {
                TempData["Error"] = "Không tìm thấy lịch hẹn.";
                return RedirectToAction(nameof(Appointments));
            }

            if (ngayMoi.Date < DateTime.Today || ngayMoi.Date > DateTime.Today.AddDays(14))
            {
                TempData["Error"] = "Ngày đổi lịch phải nằm trong giới hạn 14 ngày tới.";
                return RedirectToAction(nameof(Appointments));
            }

            var slotMacDinh = caMoi switch
            {
                CaKham.Chieu => "13:30 - 14:30",
                CaKham.Toi => "18:00 - 19:00",
                _ => "08:00 - 09:00"
            };
            var gioChon = string.IsNullOrWhiteSpace(khungGioMoi) ? slotMacDinh : khungGioMoi.Trim();

            lichHen.NgayKham = ngayMoi.Date;
            lichHen.KhungGio = $"[{caMoi}] {gioChon}";
            lichHen.TrangThai = "Chờ duyệt";
            await context.SaveChangesAsync();

            TempData["Success"] = $"Đã xác nhận đổi lịch hẹn #{id} sang {ngayMoi:dd/MM/yyyy} ({caMoi} - {gioChon}).";
            return RedirectToAction(nameof(Appointments));
        }
    }
}
