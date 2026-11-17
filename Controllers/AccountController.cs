using Microsoft.AspNetCore.Mvc;
using DatLichKhamBenh.Models.ViewModels;

namespace DatLichKhamBenh.Controllers
{
    public class AccountController : Controller
    {
        // ==========================================
        // 1. CHỨC NĂNG ĐĂNG NHẬP (LOGIN)
        // ==========================================
        [HttpGet]
        public IActionResult Login()
        {
            return View(); // Trả về file Views/Account/Login.cshtml
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Xử lý logic đăng nhập (sẽ làm sau)
                return RedirectToAction("Index", "Home"); 
            }
            return View(model);
        }

        // ==========================================
        // 2. CHỨC NĂNG ĐĂNG KÝ (REGISTER)
        // ==========================================
        [HttpGet]
        public IActionResult Register()
        {
            // Phải truyền một instance mới của RegisterViewModel vào View
            // Nếu không, các thẻ asp-for sẽ bị lỗi NullReferenceException gây trắng trang
            return View(new RegisterViewModel()); 
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Xử lý lưu tài khoản vào Database (sẽ làm sau)
                // Ví dụ: Tạo user, gán role Bệnh nhân, chuyển hướng về trang Đăng nhập
                return RedirectToAction("Login", "Account");
            }
            // Nếu có lỗi nhập liệu, trả lại View kèm thông báo lỗi
            return View(model);
        }

        // ==========================================
        // 3. CHỨC NĂNG ĐĂNG NHẬP NGOÀI (GOOGLE)
        // ==========================================
        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Tạm thời để trống để nút bấm không bị lỗi 404
            // Sau này sẽ cấu hình OAuth Google
            return RedirectToAction("Login");
        }
    }
}