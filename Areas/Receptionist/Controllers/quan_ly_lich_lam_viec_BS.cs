using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Data.Context;
using DatLichKhamBenh.Models.Entities;
using DoctorEntity = DatLichKhamBenh.Models.Entities.Doctor;

namespace DatLichKhamBenh.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    [Authorize(Roles = "Receptionist")]
    public class quan_ly_lich_lam_viec_BS : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public quan_ly_lich_lam_viec_BS(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private DbSet<Appointment> AppointmentSet => _context.Set<Appointment>();

        public record SchedulePayload(
            int bac_si_id,
            string ngay_lam_viec,
            string? ca_lam_viec,
            string? gio_bat_dau,
            string? gio_ket_thuc,
            string? trang_thai,
            string? ly_do_ban
        );

        private static string GetDoctorDisplayName(DoctorEntity? doc, int doctorId)
        {
            if (!string.IsNullOrWhiteSpace(doc?.FullName))
                return doc!.FullName;
            if (!string.IsNullOrWhiteSpace(doc?.User?.FullName))
                return doc!.User!.FullName;
            return $"BS #{doctorId}";
        }

        private static bool TryParseWorkDate(string? rawDate, out DateTime parsedDate)
        {
            parsedDate = default;
            if (string.IsNullOrWhiteSpace(rawDate))
                return false;

            string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy" };
            if (DateTime.TryParseExact(rawDate.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                parsedDate = parsedDate.Date;
                return true;
            }

            if (DateTime.TryParse(rawDate.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                parsedDate = parsedDate.Date;
                return true;
            }

            return false;
        }

        private static string BuildTimeSlot(string? gioBatDau, string? gioKetThuc, string? caLamViec)
        {
            var start = (gioBatDau ?? "").Trim();
            var end = (gioKetThuc ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(start) && !string.IsNullOrWhiteSpace(end))
                return $"{start} - {end}";
            if (!string.IsNullOrWhiteSpace(start))
                return start;

            var ca = (caLamViec ?? "").Trim().ToLower();
            if (ca.Contains("chiều") || ca.Contains("chieu"))
                return "13:30 - 18:30";
            if (ca.Contains("tối") || ca.Contains("toi"))
                return "18:30 - 22:30";
            return "07:30 - 11:30";
        }

        private static (string startTime, string endTime) SplitTimeSlot(string? timeSlot)
        {
            var raw = (timeSlot ?? "07:30 - 11:30").Trim();
            var parts = raw.Split('-', 2);
            var s = parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]) ? parts[0].Trim() : "07:30";
            var e = parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]) ? parts[1].Trim() : "11:30";
            return (s, e);
        }

        private static object MapScheduleDto(DoctorSchedule s)
        {
            var (startT, endT) = SplitTimeSlot(s.TimeSlot);
            return new
            {
                id = s.Id,
                bac_si_id = s.DoctorId,
                ngay_lam_viec = s.WorkDate.ToString("yyyy-MM-dd"),
                ca_lam_viec = string.IsNullOrWhiteSpace(s.Shift) ? "Ca sáng" : s.Shift,
                khung_gio = s.TimeSlot,
                gio_bat_dau = startT,
                gio_ket_thuc = endT,
                phong_kham = s.RoomName ?? "",
                is_available = s.IsAvailable,
                trang_thai = s.IsAvailable ? "da_xac_nhan" : "ban",
                ly_do_ban = s.AbsenceReason ?? ""
            };
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.ReceptionistName = User?.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(User.Identity.Name)
                ? User.Identity.Name
                : "Lễ tân";

            var doctors = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .OrderBy(d => d.Id)
                .ToListAsync();

            var schedules = await _context.DoctorSchedules
                .AsNoTracking()
                .OrderBy(s => s.WorkDate)
                .ThenBy(s => s.Id)
                .ToListAsync();

            ViewBag.Doctors = doctors;

            string[] candidates =
            {
                "~/Areas/Receptionist/Views/quan_ly_lich_lam_viec_BS/quan_ly_lich_lam_viec_BS.cshtml",
                "~/Areas/Receptionist/Views/Schedule/quan_ly_lich_lam_viec_BS.cshtml",
                "~/Areas/Receptionist/Views/Home/quan_ly_lich_lam_viec_BS.cshtml",
                "~/Areas/Receptionist/Views/quan_ly_lich_lam_viec_BS/Index.cshtml"
            };

            foreach (var v in candidates)
            {
                var full = Path.Combine(_env.ContentRootPath, v.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(full))
                    return View(v, schedules);
            }

            return View(schedules);
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _context.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .OrderBy(d => d.Id)
                .ToListAsync();

            return Json(doctors.Select(d => new
            {
                id = d.Id,
                name = GetDoctorDisplayName(d, d.Id)
            }).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> GetSchedules()
        {
            var schedules = await _context.DoctorSchedules
                .AsNoTracking()
                .OrderBy(s => s.WorkDate)
                .ThenBy(s => s.Id)
                .ToListAsync();

            return Json(schedules.Select(MapScheduleDto).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] SchedulePayload? payload)
        {
            if (payload == null || payload.bac_si_id <= 0)
                return BadRequest(new { success = false, detail = "Thiếu thông tin bác sĩ hoặc dữ liệu gửi lên không hợp lệ." });

            if (!TryParseWorkDate(payload.ngay_lam_viec, out var workDate))
                return BadRequest(new { success = false, detail = "Ngày làm việc không hợp lệ." });

            var doctorExists = await _context.Doctors.AsNoTracking().AnyAsync(d => d.Id == payload.bac_si_id);
            if (!doctorExists)
                return BadRequest(new { success = false, detail = $"Không tìm thấy bác sĩ #{payload.bac_si_id}." });

            var statusLower = (payload.trang_thai ?? "").Trim().ToLower();
            bool isBusy = statusLower.Contains("ban") || statusLower.Contains("bận") || !string.IsNullOrWhiteSpace(payload.ly_do_ban);

            var entity = new DoctorSchedule
            {
                DoctorId = payload.bac_si_id,
                WorkDate = workDate,
                Shift = string.IsNullOrWhiteSpace(payload.ca_lam_viec) ? "Ca sáng" : payload.ca_lam_viec.Trim(),
                TimeSlot = BuildTimeSlot(payload.gio_bat_dau, payload.gio_ket_thuc, payload.ca_lam_viec),
                RoomName = null,
                IsAvailable = !isBusy,
                AbsenceReason = isBusy ? (string.IsNullOrWhiteSpace(payload.ly_do_ban) ? "Bận đột xuất" : payload.ly_do_ban.Trim()) : null
            };

            _context.DoctorSchedules.Add(entity);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = entity.Id, data = MapScheduleDto(entity) });
        }

        [HttpPut]
        [HttpPost]
        [Route("Receptionist/quan_ly_lich_lam_viec_BS/UpdateSchedule/{id:int}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] SchedulePayload? payload)
        {
            if (id <= 0 || payload == null)
                return BadRequest(new { success = false, detail = "Dữ liệu cập nhật lịch làm việc không hợp lệ." });

            var entity = await _context.DoctorSchedules.FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
                return NotFound(new { success = false, detail = $"Không tìm thấy lịch làm việc #{id}." });

            if (payload.bac_si_id > 0)
            {
                var doctorExists = await _context.Doctors.AsNoTracking().AnyAsync(d => d.Id == payload.bac_si_id);
                if (!doctorExists)
                    return BadRequest(new { success = false, detail = $"Không tìm thấy bác sĩ #{payload.bac_si_id}." });
                entity.DoctorId = payload.bac_si_id;
            }

            if (TryParseWorkDate(payload.ngay_lam_viec, out var workDate))
                entity.WorkDate = workDate;

            if (!string.IsNullOrWhiteSpace(payload.ca_lam_viec))
                entity.Shift = payload.ca_lam_viec.Trim();

            if (!string.IsNullOrWhiteSpace(payload.gio_bat_dau) || !string.IsNullOrWhiteSpace(payload.gio_ket_thuc))
                entity.TimeSlot = BuildTimeSlot(payload.gio_bat_dau, payload.gio_ket_thuc, entity.Shift);

            var statusLower = (payload.trang_thai ?? "").Trim().ToLower();
            bool hasBusyFlag = statusLower.Contains("ban") || statusLower.Contains("bận") || !string.IsNullOrWhiteSpace(payload.ly_do_ban);
            bool hasAvailableFlag = statusLower.Contains("xac_nhan") || statusLower.Contains("xác nhận") || statusLower.Contains("available") || statusLower.Contains("lam_viec");

            if (hasBusyFlag)
            {
                entity.IsAvailable = false;
                entity.AbsenceReason = string.IsNullOrWhiteSpace(payload.ly_do_ban) ? "Bận đột xuất" : payload.ly_do_ban.Trim();
            }
            else if (hasAvailableFlag)
            {
                entity.IsAvailable = true;
                entity.AbsenceReason = null;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, id = entity.Id, data = MapScheduleDto(entity) });
        }

        [HttpDelete]
        [HttpPost]
        [Route("Receptionist/quan_ly_lich_lam_viec_BS/DeleteSchedule/{id:int}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            if (id <= 0)
                return BadRequest(new { success = false, detail = "ID lịch làm việc không hợp lệ." });

            var entity = await _context.DoctorSchedules.FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
                return NotFound(new { success = false, detail = $"Không tìm thấy lịch làm việc #{id}." });

            _context.DoctorSchedules.Remove(entity);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id });
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            var rows = await AppointmentSet
                .AsNoTracking()
                .Include(a => a.Patient)
                    .ThenInclude(p => p!.User)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.TimeSlot)
                .ToListAsync();

            return Json(new
            {
                data = rows.Select(r => new
                {
                    id = r.Id,
                    ma_lich_hen = $"LH{r.Id:D3}",
                    bac_si_id = r.DoctorId,
                    benh_nhan = !string.IsNullOrWhiteSpace(r.Patient?.FullName)
                        ? r.Patient!.FullName
                        : (!string.IsNullOrWhiteSpace(r.Patient?.User?.FullName) ? r.Patient!.User!.FullName : $"BN #{r.PatientId}"),
                    so_dien_thoai = !string.IsNullOrWhiteSpace(r.Patient?.PhoneNumber)
                        ? r.Patient!.PhoneNumber
                        : (r.Patient?.User?.PhoneNumber ?? ""),
                    ngay_kham = r.AppointmentDate.ToString("yyyy-MM-dd"),
                    khung_gio = r.TimeSlot,
                    trang_thai = r.Status ?? ""
                }).ToList()
            });
        }
    }
}