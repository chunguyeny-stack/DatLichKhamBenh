using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DatLichKhamBenh.Data;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Data.Seed;
using Microsoft.AspNetCore.Authentication.Google; // 🔥 ĐÃ MỞ KHÓA

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// 1. CẤU HÌNH DB CONTEXT (KẾT NỐI SQL SERVER)
// =====================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =====================================================
// 2. CẤU HÌNH ASP.NET CORE IDENTITY
// =====================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // --- Chính sách mật khẩu ---
    options.Password.RequireDigit = true;              
    options.Password.RequiredLength = 6;               
    options.Password.RequireNonAlphanumeric = false;   
    options.Password.RequireUppercase = false;         
    options.Password.RequireLowercase = false;         

    // --- Chính sách khóa tài khoản ---
    options.Lockout.MaxFailedAccessAttempts = 5;       
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); 
    options.Lockout.AllowedForNewUsers = true;         

    // --- Chính sách người dùng ---
    options.User.RequireUniqueEmail = true;            
    options.SignIn.RequireConfirmedEmail = false;      
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// =====================================================
// 3. CẤU HÌNH COOKIE
// =====================================================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// =====================================================
// 4. CẤU HÌNH GOOGLE OAUTH (🔥 ĐÃ MỞ KHÓA)
// =====================================================
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    });

// =====================================================
// 5. ĐĂNG KÝ MVC CONTROLLERS & VIEWS
// =====================================================
builder.Services.AddControllersWithViews();

// =====================================================
// 6. BUILD APP
// =====================================================
var app = builder.Build();

// =====================================================
// 7. SEED DATA
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();