
using DatLichKhamBenh.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalDoctors = await _context.Doctors.CountAsync();
            ViewBag.TotalPatients = await _context.Patients.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalSpecialties = await _context.Specialties.CountAsync();

            var appointments = await _context.Appointments.ToListAsync();

            ViewBag.TotalAppointments = appointments.Count;

            // Doanh thu tính theo các khoản phí đã ghi nhận
            var revenueByMonth = appointments
                .Where(a => a.Status == "Hoàn thành")
                .GroupBy(a => a.AppointmentDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(a =>
                        a.ConsultationFee +
                        a.MedicineFee +
                        a.ServiceFee)
                })
                .ToList();

            var monthlyRevenue = Enumerable.Range(1, 12)
                .Select(month => revenueByMonth
                    .FirstOrDefault(x => x.Month == month)?.Revenue ?? 0)
                .ToList();

            ViewBag.MonthLabels = System.Text.Json.JsonSerializer.Serialize(
                Enumerable.Range(1, 12)
                    .Select(m => "Tháng " + m));

            ViewBag.MonthRevenue =
                System.Text.Json.JsonSerializer.Serialize(monthlyRevenue);

            var currentYear = DateTime.Now.Year;

            var quarterlyRevenue = Enumerable.Range(1, 4)
                .Select(quarter =>
                {
                    var revenue = appointments
                        .Where(a =>
                            a.Status == "Hoàn thành" &&
                            a.AppointmentDate.Year == currentYear &&
                            ((a.AppointmentDate.Month - 1) / 3 + 1) == quarter)
                        .Sum(a =>
                            a.ConsultationFee +
                            a.MedicineFee +
                            a.ServiceFee);

                    return revenue;
                })
                .ToList();

            ViewBag.QuarterRevenue =
                System.Text.Json.JsonSerializer.Serialize(quarterlyRevenue);

            return View();
        }
    }
}
