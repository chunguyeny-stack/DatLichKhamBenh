using System;
using Microsoft.AspNetCore.Identity;

namespace DatLichKhamBenh.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Họ tên đầy đủ của người dùng/nhân viên/bác sĩ/lễ tân
        public string? HoTen { get; set; }

        // Ngày sinh
        public DateTime? NgaySinh { get; set; }

        // (Lưu ý: Các trường như Email, PhoneNumber, UserName... đã có sẵn trong IdentityUser nên không cần khai báo lại)
    }
}