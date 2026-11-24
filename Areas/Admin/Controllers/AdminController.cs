using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Data.Context;

namespace DatLichKhamBenh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController(ApplicationDbContext context) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var users = await context.Users.CountAsync();
            var activeUsers = await context.Users.CountAsync(u => u.IsActive);
            var specialties = await context.Specialties.CountAsync(s => s.IsActive);
            var doctors = await context.Doctors.CountAsync(d => d.IsActive);
            var appointments = await context.Appointments.CountAsync();
            var completedAppointments = await context.Appointments.CountAsync(a =>
                a.Status == "Hoàn thành" || a.Status == "Đã thanh toán");

            ViewBag.TotalUsers = users;
            ViewBag.ActiveUsers = activeUsers;
            ViewBag.Specialties = specialties;
            ViewBag.Doctors = doctors;
            ViewBag.Appointments = appointments;
            ViewBag.CompletedAppointments = completedAppointments;
            ViewBag.Revenue = await context.Appointments
                .Where(a => a.Status == "Hoàn thành" || a.Status == "Đã thanh toán")
                .SumAsync(a => (decimal?)a.ConsultationFee) ?? 0m;

            return View();
        }
    }
}