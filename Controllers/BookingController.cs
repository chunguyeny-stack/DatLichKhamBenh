using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Models.Enums;
using DatLichKhamBenh.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Controllers
{
    [Authorize]
    public class BookingController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager) : Controller
    {
        private const decimal GiaKhamMacDinh = 200000m;

        [HttpGet]
        public async Task<IActionResult> Create(int? doctorId, int? chuyenKhoaId)
        {
            var model = new DatLichViewModel
            {
                NgayKham = DateTime.Today.AddDays(1),
                CaKham = CaKham.Sang,
                KhungGio = "08:00 - 09:00",
                PhiCoBan = GiaKhamMacDinh,
                TongPhi = GiaKhamMacDinh
            };

            if (doctorId is > 0)
            {
                var selectedDoctor = await context.BacSis
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == doctorId.Value);

                if (selectedDoctor != null)
                {
                    model.BacSiId = selectedDoctor.Id;
                    model.ChuyenKhoaId = selectedDoctor.ChuyenKhoaId;
                    model.PhiCoBan = selectedDoctor.GiaKham > 0 ? selectedDoctor.GiaKham : GiaKhamMacDinh;
                }
            }
            else if (chuyenKhoaId is > 0)
            {
                model.ChuyenKhoaId = chuyenKhoaId.Value;
            }

            await NapDuLieuFormAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DatLichViewModel model)
        {
            var minDate = DateTime.Today;
            var maxDate = DateTime.Today.AddDays(14);

            if (model.NgayKham.Date < minDate || model.NgayKham.Date > maxDate)
            {
                ModelState.AddModelError(nameof(model.NgayKham),
                    $"Chỉ được đặt lịch từ {minDate:dd/MM/yyyy} đến {maxDate:dd/MM/yyyy} (giới hạn 14 ngày).");
            }

            var bacSi = await context.BacSis.FirstOrDefaultAsync(b => b.Id == model.BacSiId);
            if (bacSi == null)
            {
                ModelState.AddModelError(nameof(model.BacSiId), "Vui lòng chọn bác sĩ hợp lệ.");
            }

            if (bacSi != null && !string.IsNullOrWhiteSpace(model.KhungGio))
            {
                var daCoLich = await context.LichHens.AnyAsync(l =>
                    l.BacSiId == model.BacSiId &&
                    l.NgayKham.Date == model.NgayKham.Date &&
                    l.KhungGio == model.KhungGio &&
                    l.TrangThai != "Đã hủy" &&
                    l.TrangThai != "DaHuy");

                if (daCoLich)
                {
                    ModelState.AddModelError(nameof(model.KhungGio),
                        "Khung giờ này của bác sĩ đã được đặt. Vui lòng chọn khung giờ khác.");
                }
            }

            if (!ModelState.IsValid)
            {
                await NapDuLieuFormAsync(model);
                return View(model);
            }

            var userId = userManager.GetUserId(User) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var benhNhan = await context.BenhNhans.FirstOrDefaultAsync(bn => bn.UserId == userId);
            if (benhNhan == null)
            {
                benhNhan = new BenhNhan { UserId = userId };
                context.BenhNhans.Add(benhNhan);
                await context.SaveChangesAsync();
            }

            var dichVuId = await context.DichVus
                               .Where(dv => bacSi != null && dv.ChuyenKhoaId == bacSi.ChuyenKhoaId && dv.DangHoatDong)
                               .Select(dv => (int?)dv.Id)
                               .FirstOrDefaultAsync()
                           ?? await context.DichVus.Select(dv => (int?)dv.Id).FirstOrDefaultAsync()
                           ?? 1;

            var giaGoc = bacSi is { GiaKham: > 0 } ? bacSi.GiaKham : GiaKhamMacDinh;
            var tongPhi = model.CaKham == CaKham.Toi ? Math.Round(giaGoc * 1.1m, 0) : giaGoc;

            var lichHen = new LichHen
            {
                BenhNhanId = benhNhan.Id,
                BacSiId = model.BacSiId,
                DichVuId = dichVuId,
                NgayKham = model.NgayKham.Date,
                KhungGio = $"[{model.CaKham}] {model.KhungGio.Trim()}",
                TrangThai = "Chờ duyệt",
                GhiChu = model.GhiChu.Trim(),
                CreatedAt = DateTime.Now
            };

            context.LichHens.Add(lichHen);
            await context.SaveChangesAsync();

            TempData["Success"] =
                $"Đặt lịch thành công ngày {model.NgayKham.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} ({model.KhungGio}) • Phí dự kiến: {tongPhi.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))} đ.";

            return RedirectToAction("Appointments", "Patient");
        }

        [HttpGet]
        public IActionResult Appointments(string? trangThaiLoc)
        {
            return RedirectToAction("Appointments", "Patient", new { trangThaiLoc });
        }

        private async Task NapDuLieuFormAsync(DatLichViewModel model)
        {
            model.ChuyenKhoas = await context.ChuyenKhoas
                .AsNoTracking()
                .Where(ck => ck.TrangThai == "active")
                .OrderBy(ck => ck.TenChuyenKhoa)
                .ToListAsync();

            var query = context.BacSis
                .AsNoTracking()
                .Include(b => b.ChuyenKhoa)
                .Include(b => b.ApplicationUser)
                .AsQueryable();

            if (model.ChuyenKhoaId is > 0)
            {
                query = query.Where(b => b.ChuyenKhoaId == model.ChuyenKhoaId.Value);
            }

            model.BacSis = await query
                .OrderByDescending(b => b.Rating)
                .ToListAsync();

            var bacSiChon = model.BacSis.FirstOrDefault(b => b.Id == model.BacSiId);
            var giaGoc = bacSiChon is { GiaKham: > 0 } ? bacSiChon.GiaKham : model.PhiCoBan;
            if (giaGoc <= 0) giaGoc = GiaKhamMacDinh;

            model.PhiCoBan = giaGoc;
            model.TongPhi = model.CaKham == CaKham.Toi ? Math.Round(giaGoc * 1.1m, 0) : giaGoc;
        }
    }
}
