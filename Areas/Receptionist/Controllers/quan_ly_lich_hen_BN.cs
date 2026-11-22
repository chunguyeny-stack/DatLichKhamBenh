using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Data.Context;

namespace DatLichKhamBenh.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    [Authorize(Roles = "Receptionist")]
    public class quan_ly_lich_hen_BN : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public quan_ly_lich_hen_BN(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 50)
        {
            page = Math.Max(1, page);
            pageSize = Math.Max(1, pageSize);

            var expired = await _context.LichHens
                .Where(x => x.NgayKham.Date < DateTime.Today && x.TrangThai != null
                         && !x.TrangThai.ToLower().Contains("hủy") && !x.TrangThai.ToLower().Contains("cancel")
                         && !x.TrangThai.ToLower().Contains("hoàn thành") && !x.TrangThai.ToLower().Contains("completed")
                         && !x.TrangThai.ToLower().Contains("check-in") && !x.TrangThai.ToLower().Contains("checked")
                         && !x.TrangThai.ToLower().Contains("đã đến"))
                .ToListAsync();

            if (expired.Count > 0)
            {
                foreach (var e in expired)
                {
                    e.TrangThai = "Đã hủy";
                    e.GhiChu = string.IsNullOrWhiteSpace(e.GhiChu) ? "Tự động hủy do quá ngày khám."
                        : (e.GhiChu.Contains("quá ngày khám") ? e.GhiChu : $"{e.GhiChu} | Tự động hủy do quá ngày khám.");
                }
                await _context.SaveChangesAsync();
            }

            var query = _context.LichHens.AsNoTracking().Include(x => x.BacSiThongTin).OrderBy(x => x.NgayKham).ThenBy(x => x.KhungGio);
            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var bnList = await _context.BenhNhans.AsNoTracking()
                .Select(bn => new { bn.Id, bn.UserId, Phone = bn.User != null ? bn.User.PhoneNumber : null }).ToListAsync();
            var byId = bnList.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().Phone);
            var byUid = bnList.Where(x => !string.IsNullOrWhiteSpace(x.UserId)).GroupBy(x => x.UserId!).ToDictionary(g => g.Key, g => g.First().Phone);

            var sdtMap = new Dictionary<int, string>();
            foreach (var a in items)
            {
                var p = byId.TryGetValue(a.BenhNhanId, out var p1) ? p1 : (byUid.TryGetValue(a.BenhNhanId.ToString(), out var p2) ? p2 : null);
                sdtMap[a.Id] = !string.IsNullOrWhiteSpace(p) ? p : "Chưa có SĐT";
            }

            ViewBag.ReceptionistName = User?.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(User.Identity.Name) ? User.Identity.Name : "Lễ tân";
            ViewBag.SoDienThoaiBenhNhanMap = sdtMap;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize));
            ViewBag.TotalCount = totalCount;

            string[] candidates =
            {
                "~/Areas/Receptionist/Views/Appointment/quan_ly_lich_hen_BN.cshtml",
                "~/Areas/Receptionist/Views/Home/quan_ly_lich_hen_BN.cshtml",
                "~/Areas/Receptionist/Views/quan_ly_lich_hen_BN/quan_ly_lich_hen_BN.cshtml",
                "~/Areas/Receptionist/Views/Appointment/Index.cshtml"
            };
            foreach (var v in candidates)
            {
                var full = Path.Combine(_env.ContentRootPath, v.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(full)) return View(v, items);
            }
            return View(candidates[0], items);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var lh = await _context.LichHens.FindAsync(id);
            if (lh == null) return NotFound();
            lh.TrangThai = "Đã xác nhận";
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã xác nhận lịch hẹn #{id}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int lichHenId, DateTime ngayMoi)
        {
            var lh = await _context.LichHens.FindAsync(lichHenId);
            if (lh == null || !lh.BacSiId.HasValue) return NotFound();

            var lichLamViec = await _context.LichLamViecs
                .Where(x => x.BacSiId == lh.BacSiId.Value && x.NgayKham.Date == ngayMoi.Date && x.ConTrong && string.IsNullOrEmpty(x.LyDoBan))
                .Select(x => x.KhungGio).ToListAsync();
            var daDat = await _context.LichHens
                .Where(x => x.BacSiId == lh.BacSiId.Value && x.NgayKham.Date == ngayMoi.Date && x.Id != lichHenId && x.TrangThai != "Đã hủy")
                .Select(x => x.KhungGio).ToListAsync();

            return Json(lichLamViec.Where(k => !string.IsNullOrWhiteSpace(k) && !daDat.Contains(k)).Distinct().ToList());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(int id, DateTime ngayMoi, string khungGioMoi)
        {
            var lh = await _context.LichHens.FindAsync(id);
            if (lh == null) return NotFound();

            var biTrung = await _context.LichHens.AnyAsync(x => x.BacSiId == lh.BacSiId && x.NgayKham.Date == ngayMoi.Date && x.KhungGio == khungGioMoi && x.Id != id && x.TrangThai != "Đã hủy");
            if (biTrung)
            {
                TempData["Error"] = "Khung giờ đã có bệnh nhân đặt, vui lòng chọn khung giờ trống khác.";
                return RedirectToAction(nameof(Index));
            }

            lh.NgayKham = ngayMoi.Date;
            lh.KhungGio = khungGioMoi;
            lh.TrangThai = "Đã xác nhận";
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã đổi lịch hẹn #{id} sang {ngayMoi:dd/MM/yyyy} ({khungGioMoi}).";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOverdue(int id)
        {
            var lh = await _context.LichHens.FindAsync(id);
            if (lh == null) return NotFound();
            lh.TrangThai = "Đã hủy";
            lh.GhiChu = "Hủy lịch do bệnh nhân quá giờ hẹn.";
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã hủy lịch hẹn #{id} do quá giờ hẹn.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id)
        {
            var lh = await _context.LichHens.FindAsync(id);
            if (lh == null) return NotFound();
            if (lh.TrangThai != "Đã xác nhận")
            {
                TempData["Error"] = "Chỉ xác nhận đến khám cho lịch hẹn đã được xác nhận.";
                return RedirectToAction(nameof(Index));
            }
            lh.TrangThai = "Đã đến";
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Bệnh nhân của lịch hẹn #{id} đã đến khám.";
            return RedirectToAction(nameof(Index));
        }
    }
}