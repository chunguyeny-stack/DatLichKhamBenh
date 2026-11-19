using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Data;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Data.Seed;
// using Microsoft.AspNetCore.Authentication.Google; // ĐÃ COMMENT DÒNG NÀY ĐỂ HẾT LỖI

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// 1. CẤU HÌNH DB CONTEXT (KẾT NỐI SQL SERVER)
// =====================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =====================================================
// 2. CẤU HÌNH ASP.NET CORE IDENTITY
// (Xử lý xác thực, phân quyền, khóa tài khoản, đổi MK)
// =====================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // --- Chính sách mật khẩu ---
    options.Password.RequireDigit = true;              // Bắt buộc có số
    options.Password.RequiredLength = 6;               // Tối thiểu 6 ký tự
    options.Password.RequireNonAlphanumeric = false;   // Không cần ký tự đặc biệt
    options.Password.RequireUppercase = false;         // Không bắt buộc chữ hoa
    options.Password.RequireLowercase = false;         // Không bắt buộc chữ thường

    // --- Chính sách khóa tài khoản (Lockout) ---
    options.Lockout.MaxFailedAccessAttempts = 5;       // Sai 5 lần → khóa
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Khóa 15 phút
    options.Lockout.AllowedForNewUsers = true;         // Áp dụng cho user mới

    // --- Chính sách người dùng ---
    options.User.RequireUniqueEmail = true;            // Email không được trùng
    options.SignIn.RequireConfirmedEmail = false;      // Không bắt buộc xác thực email
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// =====================================================
// 3. CẤU HÌNH COOKIE (Điều hướng khi chưa đăng nhập / bị từ chối)
// =====================================================
builder.Services.ConfigureApplicationCookie(options =>
{
    // Nếu chưa đăng nhập mà cố vào trang bảo vệ → chuyển về Login
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    // Nếu đã đăng nhập nhưng không đủ quyền → chuyển về AccessDenied
    options.AccessDeniedPath = "/Account/AccessDenied";
    // Thời gian sống của cookie (7 ngày nếu RememberMe = true)
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// =====================================================
// 4. CẤU HÌNH GOOGLE OAUTH (Đã comment vì chưa cài đặt)
// =====================================================
// builder.Services.AddAuthentication()
//     .AddGoogle(options =>
//     {
//         options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
//         options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
//     });

// =====================================================
// 5. ĐĂNG KÝ MVC CONTROLLERS & VIEWS
// =====================================================
builder.Services.AddControllersWithViews();

// =====================================================
// 6. BUILD APP
// =====================================================
var app = builder.Build();

// =====================================================
// 7. SEED DATA (Tạo Roles, Admin, Patient, Doctor mẫu)
// =====================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.SeedDataAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi khi khởi tạo dữ liệu mẫu.");
    }
}

// =====================================================
// 8. MIDDLEWARE PIPELINE
// =====================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔥 THỨ TỰ BẮT BUỘC: Authentication → Authorization
app.UseAuthentication();
app.UseAuthorization();

// 🔥 Cấu hình Route cho Areas (Admin, Doctor, Receptionist)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// 🔥 Cấu hình Route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();