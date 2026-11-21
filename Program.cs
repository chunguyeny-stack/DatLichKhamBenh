using Microsoft.EntityFrameworkCore;
using MediSlot.Data;
using MediSlot.Services.Interfaces;
using MediSlot.Services.Impl;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Lỗi: Không tìm thấy 'DefaultConnection' trong appsettings.json. Vui lòng kiểm tra lại file cấu hình!");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
    
// Đăng ký Service
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Route cho Areas (Bắt buộc phải có để vào được Doctor)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();