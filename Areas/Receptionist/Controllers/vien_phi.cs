using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
<<<<<<< HEAD
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
=======
>>>>>>> 44ae3b0 (cập nhật lại dữ liệu, và giao diện lễ tân)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;

namespace DatLichKhamBenh.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
<<<<<<< HEAD
    [Authorize(Roles = "Receptionist")]
=======
    // [Authorize(Roles = "Receptionist")]
>>>>>>> 44ae3b0 (cập nhật lại dữ liệu, và giao diện lễ tân)
    public class vien_phi : Controller
    {
        private readonly ApplicationDbContext _context;

        public vien_phi(ApplicationDbContext context)
        {
            _context = context;
        }

        private DbSet<Appointment> AppointmentSet => _context.Set<Appointment>();

        public record PayRequest(int lich_hen_id, decimal? tong_tien, string? phuong_thuc);

        private IActionResult BuildView(object? model = null)
        {
            ViewBag.ReceptionistName = User?.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(User.Identity.Name)
                ? User.Identity.Name
                : "Lễ tân";

            return model == null ? View("~/Areas/Receptionist/Views/Billing/vien_phi.cshtml") : View("~/Areas/Receptionist/Views/Billing/vien_phi.cshtml", model);
        }

        private static bool IsCanPay(string? status)
        {
            var st = (status ?? string.Empty).Trim().ToLower();
            return st == "hoàn thành" || st == "đã hoàn thành" || st == "completed";
        }

        private static bool IsAlreadyPaid(string? status)
        {
            var st = (status ?? string.Empty).Trim().ToLower();
            return st == "đã thanh toán" || st == "paid";
        }

        private async Task<(decimal tienKham, decimal tienThuoc, decimal tienDichVu, decimal tongTien, string tenDichVu)> TinhTongTienAsync(Appointment appt)
        {
            var doctorFee = appt.Doctor?.ConsultationFee
                ?? await _context.Doctors
                    .AsNoTracking()
                    .Where(d => d.Id == appt.DoctorId)
                    .Select(d => (decimal?)d.ConsultationFee)
                    .FirstOrDefaultAsync()
                ?? 0m;

            decimal tienKham = appt.ConsultationFee > 0 ? appt.ConsultationFee : doctorFee;
            decimal tienThuoc = appt.MedicineFee;
            decimal tienDichVu = appt.ServiceFee;
            decimal tongTien = tienKham + tienThuoc + tienDichVu;
            string tenDichVu = tienDichVu > 0 ? "Dịch vụ khám" : "Khám bệnh";

            return (tienKham, tienThuoc, tienDichVu, tongTien, tenDichVu);
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? maLichHen)
        {
            if (string.IsNullOrWhiteSpace(maLichHen))
                return BuildView();

            var cleanCode = maLichHen.Trim().ToUpper().Replace("LH", "");
            if (!int.TryParse(cleanCode, out int id))
            {
                TempData["Error"] = "Mã lịch hẹn không hợp lệ.";
                return BuildView();
            }

            var appointment = await AppointmentSet
                .Include(x => x.Patient)
                    .ThenInclude(p => p!.User)
                .Include(x => x.Doctor)
                    .ThenInclude(d => d!.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (appointment == null)
            {
                TempData["Error"] = $"Không tìm thấy lịch hẹn #{id}.";
                return BuildView();
            }

            var isPaid = IsAlreadyPaid(appointment.Status);
            var canPay = IsCanPay(appointment.Status);

            if (!canPay && !isPaid)
            {
                TempData["Error"] = "Chỉ thu viện phí cho lịch hẹn đã khám xong (Hoàn thành).";
                return BuildView();
            }

            var (tienKham, tienThuoc, tienDichVu, tongTien, tenDichVu) = await TinhTongTienAsync(appointment);

            ViewBag.TienKham = tienKham;
            ViewBag.TienThuoc = tienThuoc;
            ViewBag.TienDichVu = tienDichVu;
            ViewBag.TongTien = tongTien;
            ViewBag.TenDichVu = tenDichVu;
            ViewBag.DaThanhToan = isPaid;

            return BuildView(appointment);
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoicesData()
        {
            var appointments = await AppointmentSet
                .AsNoTracking()
                .Include(a => a.Doctor)
                    .ThenInclude(d => d!.User)
                .Include(a => a.Patient)
                    .ThenInclude(p => p!.User)
                .OrderByDescending(a => a.Id)
                .ToListAsync();

            var invoices = new List<object>();
            var payments = new List<object>();

            foreach (var a in appointments)
            {
                var status = (a.Status ?? "Chờ").Trim();
                var isPaid = IsAlreadyPaid(status);
                var canPay = IsCanPay(status);

                var consultationFee = a.ConsultationFee > 0 ? a.ConsultationFee : (a.Doctor?.ConsultationFee ?? 0m);
                var serviceFee = a.ServiceFee;
                var medicineFee = a.MedicineFee;
                var serviceName = serviceFee > 0 ? "Dịch vụ khám" : "Khám bệnh";
                var doctorName = !string.IsNullOrWhiteSpace(a.Doctor?.FullName)
                    ? a.Doctor!.FullName
                    : (!string.IsNullOrWhiteSpace(a.Doctor?.User?.FullName) ? a.Doctor!.User!.FullName : $"BS #{a.DoctorId}");
                var patientName = !string.IsNullOrWhiteSpace(a.Patient?.FullName)
                    ? a.Patient!.FullName
                    : (!string.IsNullOrWhiteSpace(a.Patient?.User?.FullName) ? a.Patient!.User!.FullName : $"BN #{a.PatientId}");

                var items = new List<object>();
                if (consultationFee > 0)
                    items.Add(new { name = "Phí khám bác sĩ", qty = 1, price = consultationFee });
                if (serviceFee > 0)
                    items.Add(new { name = serviceName, qty = 1, price = serviceFee });
                if (medicineFee > 0)
                    items.Add(new { name = "Tiền thuốc", qty = 1, price = medicineFee });

                var total = consultationFee + serviceFee + medicineFee;
                var code = $"LH{a.Id:D3}";

                invoices.Add(new
                {
                    rawId = a.Id,
                    code,
                    doctor = doctorName,
                    patientName,
                    time = a.TimeSlot,
                    date = a.AppointmentDate.ToString("yyyy-MM-dd"),
                    dateDisplay = a.AppointmentDate.ToString("dd/MM/yyyy"),
                    specialty = serviceName,
                    statusText = status,
                    isPaid,
                    canPay,
                    totalAmount = total,
                    items
                });

                if (isPaid)
                {
                    payments.Add(new
                    {
                        invoiceId = $"HD-{a.Id:D4}",
                        appointmentCode = code,
                        totalAmount = total,
                        paidAt = a.AppointmentDate.ToString("yyyy-MM-dd"),
                        paidAtDisplay = a.AppointmentDate.ToString("dd/MM/yyyy")
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

            var appointment = await AppointmentSet
                .Include(x => x.Doctor)
                .FirstOrDefaultAsync(x => x.Id == targetId);

            if (appointment == null)
                return body != null ? NotFound(new { success = false, detail = "Không tìm thấy lịch hẹn." }) : NotFound();

            if (IsAlreadyPaid(appointment.Status))
            {
                if (body != null)
                    return BadRequest(new { success = false, detail = "Lịch hẹn này đã được thanh toán trước đó." });

                TempData["Error"] = "Lịch hẹn này đã được thanh toán trước đó.";
                return RedirectToAction(nameof(Index), new { maLichHen = targetId });
            }

            if (!IsCanPay(appointment.Status))
            {
                if (body != null)
                    return BadRequest(new { success = false, detail = "Chỉ thu tiền cho lịch hẹn đã hoàn thành khám." });

                TempData["Error"] = "Lịch hẹn chưa ở trạng thái Hoàn thành, không thể xác nhận thanh toán.";
                return RedirectToAction(nameof(Index), new { maLichHen = targetId });
            }

            var (_, _, _, tongTien, _) = await TinhTongTienAsync(appointment);
            if (tongTien <= 0 && body?.tong_tien is > 0)
                tongTien = body.tong_tien.Value;

            appointment.Status = "Đã thanh toán";
            await _context.SaveChangesAsync();

            if (body != null)
                return Json(new { success = true, tongTien });

            TempData["Success"] = $"Đã xác nhận thanh toán {tongTien:N0} VNĐ cho lịch hẹn #{targetId}.";
            return RedirectToAction(nameof(Index), new { maLichHen = targetId });
        }
    }
}