using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Models.Enums;

namespace DatLichKhamBenh.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    public class quan_ly_lich_lam_viec_BS : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public quan_ly_lich_lam_viec_BS(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public record SchedulePayload(int bac_si_id, string ngay_lam_viec, string? ca_lam_viec, string? gio_bat_dau, string? gio_ket_thuc, string? trang_thai, string? ly_do_ban);

        private static T GetCol<T>(IDataRecord r, string name, T fallback)
        {
            try
            {
                var i = r.GetOrdinal(name);
                if (r.IsDBNull(i)) return fallback;
                var v = r.GetValue(i);
                if (v == null || v == DBNull.Value) return fallback;
                if (typeof(T) == typeof(string)) return (T)(object)(Convert.ToString(v) ?? "");
                if (typeof(T) == typeof(int)) return (T)(object)Convert.ToInt32(v);
                if (typeof(T) == typeof(bool)) return (T)(object)Convert.ToBoolean(v);
                if (typeof(T) == typeof(DateTime)) return (T)(object)Convert.ToDateTime(v).Date;
                return (T)v;
            }
            catch { return fallback; }
        }

        private async Task<List<Dictionary<string, object?>>> QueryRowsAsync(string sql)
        {
            var rows = new List<Dictionary<string, object?>>();
            var conn = _context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                rows.Add(row);
            }
            return rows;
        }

        private static string PickStr(IDictionary<string, object?> r, string fb, params string[] keys)
        {
            foreach (var k in keys)
                if (r.TryGetValue(k, out var v) && v != null && v != DBNull.Value)
                {
                    var s = Convert.ToString(v);
                    if (!string.IsNullOrWhiteSpace(s)) return s!;
                }
            return fb;
        }

        private static int PickInt(IDictionary<string, object?> r, int fb, params string[] keys)
        {
            foreach (var k in keys)
                if (r.TryGetValue(k, out var v) && v != null && v != DBNull.Value && int.TryParse(Convert.ToString(v), out var n)) return n;
            return fb;
        }

        private static bool PickBool(IDictionary<string, object?> r, bool fb, params string[] keys)
        {
            foreach (var k in keys)
                if (r.TryGetValue(k, out var v) && v != null && v != DBNull.Value) return Convert.ToBoolean(v);
            return fb;
        }

        private static DateTime PickDate(IDictionary<string, object?> r, params string[] keys)
        {
            foreach (var k in keys)
                if (r.TryGetValue(k, out var v) && v != null && v != DBNull.Value && DateTime.TryParse(Convert.ToString(v), out var d)) return d.Date;
            return DateTime.Today;
        }

        private async Task<List<(int Id, string HoTen, string SoDienThoai, string Email)>> ReadDoctorsSafeAsync() =>
            (await QueryRowsAsync("SELECT * FROM [bac_si]"))
            .Select(r => (PickInt(r, 0, "id", "Id"), PickStr(r, $"Bác sĩ #{PickInt(r, 0, "id", "Id")}", "ho_ten", "HoTen"), PickStr(r, "", "so_dien_thoai", "SoDienThoai"), PickStr(r, "", "email", "Email")))
            .Where(x => x.Item1 > 0).ToList();

        private async Task<List<(int Id, int BacSiId, DateTime NgayKham, string KhungGio, bool ConTrong, string LyDoBan, string TrangThaiChiTiet)>> ReadSchedulesSafeAsync() =>
            (await QueryRowsAsync("SELECT * FROM [lich_lam_viec] ORDER BY [ngay_kham], [khung_gio]"))
            .Select(r => (PickInt(r, 0, "id", "Id"), PickInt(r, 0, "bac_si_id", "BacSiId"), PickDate(r, "ngay_kham", "NgayKham"), PickStr(r, "07:30 - 11:30", "khung_gio", "KhungGio"), PickBool(r, true, "con_trong", "ConTrong"), PickStr(r, "", "ly_do_ban", "LyDoBan"), PickStr(r, "", "trang_thai_chi_tiet", "TrangThaiChiTiet")))
            .ToList();

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.ReceptionistName = User?.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(User.Identity.Name) ? User.Identity.Name : "Lễ tân";
            var docs = await ReadDoctorsSafeAsync();
            var doctorsSafe = docs.Select(d => new BacSi { Id = d.Id, HoTen = d.HoTen, SoDienThoai = d.SoDienThoai, Email = d.Email }).ToList();
            var docMap = doctorsSafe.ToDictionary(d => d.Id, d => d.HoTen);
            var schedules = (await ReadSchedulesSafeAsync()).Select(s => new LichLamViec
            {
                Id = s.Id, BacSiId = s.BacSiId, NgayKham = s.NgayKham, KhungGio = s.KhungGio, ConTrong = s.ConTrong,
                LyDoBan = s.LyDoBan, TrangThaiChiTiet = s.TrangThaiChiTiet,
                BacSi = new BacSi { Id = s.BacSiId, HoTen = docMap.TryGetValue(s.BacSiId, out var n) ? n : $"BS #{s.BacSiId}" }
            }).ToList();

            ViewBag.Doctors = doctorsSafe;
            string[] candidates = { "~/Areas/Receptionist/Views/quan_ly_lich_lam_viec_BS/quan_ly_lich_lam_viec_BS.cshtml", "~/Areas/Receptionist/Views/Schedule/quan_ly_lich_lam_viec_BS.cshtml", "~/Areas/Receptionist/Views/Home/quan_ly_lich_lam_viec_BS.cshtml", "~/Areas/Receptionist/Views/quan_ly_lich_lam_viec_BS/Index.cshtml" };
            foreach (var v in candidates)
            {
                var full = Path.Combine(_env.ContentRootPath, v.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(full)) return View(v, schedules);
            }
            return View(schedules);
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctors() => Json((await ReadDoctorsSafeAsync()).Select(d => new { id = d.Id, name = d.HoTen }).ToList());

        [HttpGet]
        public async Task<IActionResult> GetSchedules()
        {
            var docMap = (await ReadDoctorsSafeAsync()).ToDictionary(d => d.Id, d => d.HoTen);
            return Json((await ReadSchedulesSafeAsync()).Select(x => new
            {
                id = x.Id, bac_si_id = x.BacSiId, doctorName = docMap.TryGetValue(x.BacSiId, out var n) ? n : $"BS #{x.BacSiId}",
                ngay_lam_viec = x.NgayKham.ToString("yyyy-MM-dd"), khung_gio = x.KhungGio, gio_bat_dau = x.KhungGio,
                status = !string.IsNullOrWhiteSpace(x.LyDoBan) || !x.ConTrong ? "bac_si_huy" : "da_xac_nhan",
                trang_thai_chi_tiet = x.TrangThaiChiTiet, ly_do_ban = x.LyDoBan
            }).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            var rows = await QueryRowsAsync("SELECT * FROM [lich_hen] ORDER BY [ngay_kham], [khung_gio]");
            return Json(new { data = rows.Select(r => {
                var id = PickInt(r, 0, "id", "Id");
                return new { id, ma_lich_hen = $"LH{id:D3}", bac_si_id = PickInt(r, 0, "bac_si_id", "BacSiId"), benh_nhan = $"LH{id:D3}", so_dien_thoai = "", ngay_kham = PickDate(r, "ngay_kham", "NgayKham").ToString("yyyy-MM-dd"), khung_gio = PickStr(r, "", "khung_gio", "KhungGio"), trang_thai = PickStr(r, "", "trang_thai", "TrangThai") };
            }).ToList() });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSchedule([FromBody] SchedulePayload payload)
        {
            if (payload == null || payload.bac_si_id <= 0 || !DateTime.TryParse(payload.ngay_lam_viec, out var day))
                return BadRequest(new { success = false, detail = "Dữ liệu ca trực không hợp lệ." });
            var slot = !string.IsNullOrWhiteSpace(payload.gio_bat_dau) && !string.IsNullOrWhiteSpace(payload.gio_ket_thuc) ? $"{payload.gio_bat_dau} - {payload.gio_ket_thuc}" : (payload.ca_lam_viec ?? "07:30 - 11:30");
            var entity = new LichLamViec { BacSiId = payload.bac_si_id, NgayKham = day.Date, KhungGio = slot, ConTrong = true, TrangThaiChiTiet = payload.ca_lam_viec ?? "Đã xác nhận", LyDoBan = payload.ly_do_ban ?? "" };
            _context.LichLamViecs.Add(entity);
            await _context.SaveChangesAsync();
            return Json(new { success = true, id = entity.Id });
        }

        [HttpPut, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] SchedulePayload payload)
        {
            var entity = await _context.LichLamViecs.FindAsync(id);
            if (entity == null) return NotFound(new { success = false, detail = "Không tìm thấy ca trực." });
            if (payload != null)
            {
                if (payload.bac_si_id > 0) entity.BacSiId = payload.bac_si_id;
                if (DateTime.TryParse(payload.ngay_lam_viec, out var day)) entity.NgayKham = day.Date;
                if (!string.IsNullOrWhiteSpace(payload.gio_bat_dau) && !string.IsNullOrWhiteSpace(payload.gio_ket_thuc)) entity.KhungGio = $"{payload.gio_bat_dau} - {payload.gio_ket_thuc}";
                if (!string.IsNullOrWhiteSpace(payload.ca_lam_viec)) entity.TrangThaiChiTiet = payload.ca_lam_viec;
                if (payload.ly_do_ban != null) entity.LyDoBan = payload.ly_do_ban;
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpDelete, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var entity = await _context.LichLamViecs.FindAsync(id);
            if (entity == null) return NotFound(new { success = false, detail = "Không tìm thấy ca trực để xóa." });
            _context.LichLamViecs.Remove(entity);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateScheduleRange(int doctorId, DateTime tuNgay, DateTime denNgay, List<CaKham> danhSachCa, int thoiLuongPhut)
        {
            var soNgay = (denNgay.Date - tuNgay.Date).Days + 1;
            if (soNgay < 7 || soNgay > 14) { TempData["Error"] = "Khoảng thời gian lên lịch cho bác sĩ phải từ 7 đến 14 ngày."; return RedirectToAction(nameof(Index)); }
            if (thoiLuongPhut != 15 && thoiLuongPhut != 30 && thoiLuongPhut != 45) { TempData["Error"] = "Thời lượng phiên khám chỉ hỗ trợ 15, 30 hoặc 45 phút."; return RedirectToAction(nameof(Index)); }

            var caList = (danhSachCa?.Count > 0) ? danhSachCa : new List<CaKham> { CaKham.Sang, CaKham.Chieu, CaKham.Toi };
            var newItems = new List<LichLamViec>();
            for (var day = tuNgay.Date; day <= denNgay.Date; day = day.AddDays(1))
                foreach (var ca in caList)
                    foreach (var slot in TaoCacKhungGioTheoCa(ca, thoiLuongPhut))
                        if (!await _context.LichLamViecs.AnyAsync(x => x.BacSiId == doctorId && x.NgayKham.Date == day && x.KhungGio == slot))
                            newItems.Add(new LichLamViec { BacSiId = doctorId, NgayKham = day, KhungGio = slot, ConTrong = true, TrangThaiChiTiet = $"Làm việc ({ca} - {thoiLuongPhut}p)" });

            _context.LichLamViecs.AddRange(newItems);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã tạo {newItems.Count} phiên làm việc từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAbsent(int id, string? lyDo)
        {
            var lv = await _context.LichLamViecs.FindAsync(id);
            if (lv == null) return NotFound();
            var reason = string.IsNullOrWhiteSpace(lyDo) ? "Bác sĩ nghỉ đột xuất" : lyDo.Trim();
            lv.ConTrong = false; lv.LyDoBan = reason; lv.TrangThaiChiTiet = "Nghỉ đột xuất";

            var affected = await _context.LichHens.Where(x => x.BacSiId == lv.BacSiId && x.NgayKham.Date == lv.NgayKham.Date && x.KhungGio == lv.KhungGio && x.TrangThai != "Đã hủy" && x.TrangThai != "Hoàn thành" && x.TrangThai != "Đã hoàn thành").ToListAsync();
            var msg = $"Bác sĩ nghỉ đột xuất ({reason}) vào {lv.NgayKham:dd/MM/yyyy} ({lv.KhungGio}). Vui lòng liên hệ/chọn lịch đổi mới.";
            foreach (var lh in affected) { lh.TrangThai = "Chờ đổi lịch"; lh.GhiChu = string.IsNullOrWhiteSpace(lh.GhiChu) ? msg : $"{lh.GhiChu} | {msg}"; }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã ghi nhận bác sĩ nghỉ đột xuất và đánh dấu {affected.Count} lịch hẹn cần đổi lịch.";
            return RedirectToAction(nameof(Index));
        }

        private static List<string> TaoCacKhungGioTheoCa(CaKham ca, int stepMinutes)
        {
            var (start, end) = ca switch
            {
                CaKham.Sang => (new TimeSpan(7, 30, 0), new TimeSpan(11, 30, 0)),
                CaKham.Chieu => (new TimeSpan(13, 30, 0), new TimeSpan(17, 30, 0)),
                _ => (new TimeSpan(18, 0, 0), new TimeSpan(21, 30, 0))
            };
            var res = new List<string>();
            for (var cur = start; cur.Add(TimeSpan.FromMinutes(stepMinutes)) <= end; cur = cur.Add(TimeSpan.FromMinutes(stepMinutes)))
                res.Add($"{cur:hh\\:mm} - {cur.Add(TimeSpan.FromMinutes(stepMinutes)):hh\\:mm}");
            return res;
        }
    }
}