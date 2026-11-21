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

namespace DatLichKhamBenh.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    //[Authorize(Roles = "Receptionist,LeTan")]
    public class vien_phi : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public vien_phi(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public record PayRequest(int lich_hen_id, decimal? tong_tien, string? phuong_thuc);

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

        private static decimal PickDec(IDictionary<string, object?> r, decimal fb, params string[] keys)
        {
            foreach (var k in keys)
                if (r.TryGetValue(k, out var v) && v != null && v != DBNull.Value && decimal.TryParse(Convert.ToString(v), out var m)) return m;
            return fb;
        }

        private static DateTime PickDate(IDictionary<string, object?> r, params string[] keys)
        {
            foreach (var k in keys)
                if (r.TryGetValue(k, out var v) && v != null && v != DBNull.Value && DateTime.TryParse(Convert.ToString(v), out var d)) return d.Date;
            return DateTime.Today;
        }

        private IActionResult ResolveView(object? model = null)
        {
            ViewBag.ReceptionistName = User?.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(User.Identity.Name)
                ? User.Identity.Name
                : "Lễ tân";

            string[] candidates =
            {
                "~/Areas/Receptionist/Views/vien_phi/vien_phi.cshtml",
                "~/Areas/Receptionist/Views/Payment/vien_phi.cshtml",
                "~/Areas/Receptionist/Views/Invoice/vien_phi.cshtml",
                "~/Areas/Receptionist/Views/Home/vien_phi.cshtml",
                "~/Areas/Receptionist/Views/vien_phi/Index.cshtml"
            };

            foreach (var v in candidates)
            {
                var full = Path.Combine(_env.ContentRootPath, v.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(full))
                    return model == null ? View(v) : View(v, model);
            }

            var viewsRoot = Path.Combine(_env.ContentRootPath, "Areas", "Receptionist", "Views");
            if (Directory.Exists(viewsRoot))
            {
                var found = Directory.GetFiles(viewsRoot, "vien_phi.cshtml", SearchOption.AllDirectories).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(found))
                {
                    var rel = "~/" + Path.GetRelativePath(_env.ContentRootPath, found).Replace('\\', '/');
                    return model == null ? View(rel) : View(rel, model);
                }
            }

            return Content("Không tìm thấy file view vien_phi.cshtml trong Areas/Receptionist/Views.", "text/plain; charset=utf-8");
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? maLichHen)
        {
            if (string.IsNullOrWhiteSpace(maLichHen))
                return ResolveView();

            var cleanCode = maLichHen.Trim().ToUpper().Replace("LH", "");
            if (!int.TryParse(cleanCode, out int id))
            {
                TempData["Error"] = "Mã lịch hẹn không hợp lệ.";
                return ResolveView();
            }

            var lichHen = await _context.LichHens
                .Include(x => x.BenhNhan)
                .Include(x => x.BacSiThongTin)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (lichHen == null)
            {
                TempData["Error"] = $"Không tìm thấy lịch hẹn #{id}.";
                return ResolveView();
            }

            var daKhamXong = lichHen.TrangThai == "Hoàn thành"
                          || lichHen.TrangThai == "Đã hoàn thành"
                          || lichHen.TrangThai == "Đã thanh toán";

            if (!daKhamXong)
            {
                TempData["Error"] = "Chỉ thu viện phí cho lịch hẹn đã khám xong (Hoàn thành).";
                return ResolveView();
            }

            var (tienKham, tienThuoc, tienDichVu, tongTien) = await TinhTongTienAsync(lichHen);

            ViewBag.TienKham = tienKham;
            ViewBag.TienThuoc = tienThuoc;
            ViewBag.TienDichVu = tienDichVu;
            ViewBag.TongTien = tongTien;
            ViewBag.DaThanhToan = lichHen.TrangThai == "Đã thanh toán";

            return ResolveView(lichHen);
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoicesData()
        {
            var docRows = await QueryRowsAsync("SELECT * FROM [bac_si]");
            var docNameMap = docRows.ToDictionary(r => PickInt(r, 0, "id", "Id"), r => PickStr(r, "Bác sĩ", "ho_ten", "HoTen"));
            var docFeeMap = docRows.ToDictionary(r => PickInt(r, 0, "id", "Id"), r => PickDec(r, 300000m, "gia_kham", "GiaKham"));

            var dvRows = await QueryRowsAsync("SELECT * FROM [dich_vu]");
            var dvMap = dvRows.ToDictionary(
                r => PickInt(r, 0, "id", "Id"),
                r => (Name: PickStr(r, "Khám chuyên khoa", "ten_dich_vu", "TenDichVu"), Price: PickDec(r, 0m, "gia_tien", "GiaTien"))
            );

            var lhRows = await QueryRowsAsync("SELECT * FROM [lich_hen] ORDER BY [id] DESC");
            var invoices = new List<object>();
            var payments = new List<object>();

            foreach (var r in lhRows)
            {
                var id = PickInt(r, 0, "id", "Id");
                if (id <= 0) continue;
                var bacSiId = PickInt(r, 0, "bac_si_id", "BacSiId");
                var dichVuId = PickInt(r, 0, "dich_vu_id", "DichVuId");
                var status = PickStr(r, "Chờ xác nhận", "trang_thai", "TrangThai").Trim();
                var lower = status.ToLower();
                var ngay = PickDate(r, "ngay_kham", "NgayKham");
                var gio = PickStr(r, "08:00 - 08:30", "khung_gio", "KhungGio");

                var isPaid = lower.Contains("đã thanh toán") || lower.Contains("paid");
                var canPay = isPaid || lower.Contains("hoàn thành") || lower.Contains("completed") || lower.Contains("check-in") || lower.Contains("checked") || lower.Contains("đã đến");

                var fee = docFeeMap.TryGetValue(bacSiId, out var fk) && fk > 0 ? fk : 300000m;
                var dvInfo = dvMap.TryGetValue(dichVuId, out var dv) ? dv : (Name: "Dịch vụ khám", Price: 0m);
                var items = new List<object>
                {
                    new { name = "Phí khám bác sĩ", qty = 1, price = fee }
                };
                if (dvInfo.Price > 0)
                    items.Add(new { name = dvInfo.Name, qty = 1, price = dvInfo.Price });

                var total = fee + dvInfo.Price;
                var code = $"LH{id:D3}";

                invoices.Add(new
                {
                    rawId = id,
                    code,
                    doctor = docNameMap.TryGetValue(bacSiId, out var dn) ? dn : $"BS #{bacSiId}",
                    time = gio,
                    date = ngay.ToString("yyyy-MM-dd"),
                    dateDisplay = ngay.ToString("dd/MM/yyyy"),
                    specialty = dvInfo.Name,
                    statusText = status,
                    isPaid,
                    canPay,
                    items
                });

                if (isPaid)
                {
                    payments.Add(new
                    {
                        invoiceId = $"HD-{id:D4}",
                        appointmentCode = code,
                        totalAmount = total,
                        paidAt = ngay.ToString("yyyy-MM-dd"),
                        paidAtDisplay = ngay.ToString("dd/MM/yyyy")
                    });
                }
            }

            return Json(new { invoices, payments });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int? id, [FromBody] PayRequest? body)
        {
            var targetId = id ?? body?.lich_hen_id ?? 0;
            if (targetId <= 0)
                return body != null ? BadRequest(new { success = false, detail = "Mã lịch hẹn không hợp lệ." }) : NotFound();

            var lichHen = await _context.LichHens
                .Include(x => x.BacSiThongTin)
                .FirstOrDefaultAsync(x => x.Id == targetId);

            if (lichHen == null)
                return body != null ? NotFound(new { success = false, detail = "Không tìm thấy lịch hẹn." }) : NotFound();

            var st = (lichHen.TrangThai ?? "").Trim().ToLower();
            var allowed = st.Contains("hoàn thành") || st.Contains("completed") || st.Contains("check-in") || st.Contains("checked") || st.Contains("đã đến") || st.Contains("đã thanh toán");
            if (!allowed)
            {
                if (body != null) return BadRequest(new { success = false, detail = "Chỉ thu tiền cho lịch hẹn đã khám/check-in." });
                TempData["Error"] = "Lịch hẹn chưa ở trạng thái Hoàn thành, không thể xác nhận thanh toán.";
                return RedirectToAction(nameof(Index), new { maLichHen = targetId });
            }

            var (_, _, _, tongTien) = await TinhTongTienAsync(lichHen);
            if (tongTien <= 0 && body?.tong_tien is > 0) tongTien = body.tong_tien.Value;

            lichHen.TrangThai = "Đã thanh toán";
            await _context.SaveChangesAsync();

            if (body != null)
                return Json(new { success = true, tongTien });

            TempData["Success"] = $"Đã xác nhận thanh toán {tongTien:N0} VNĐ cho lịch hẹn #{targetId}.";
            return RedirectToAction(nameof(Index), new { maLichHen = targetId });
        }

        private async Task<(decimal tienKham, decimal tienThuoc, decimal tienDichVu, decimal tongTien)> TinhTongTienAsync(LichHen lichHen)
        {
            var tienKham = lichHen.BacSiThongTin?.GiaKham ?? 0m;
            var dichVu = await _context.DichVus.FindAsync(lichHen.DichVuId);
            var tienDichVu = dichVu?.GiaTien ?? 0m;
            decimal tienThuoc = 0m;
            var tongTien = tienKham + tienThuoc + tienDichVu;
            return (tienKham, tienThuoc, tienDichVu, tongTien);
        }
    }
}