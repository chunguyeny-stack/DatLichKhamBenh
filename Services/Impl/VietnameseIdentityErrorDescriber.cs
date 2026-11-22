using Microsoft.AspNetCore.Identity;

namespace DatLichKhamBenh.Services.Impl
{
    public sealed class VietnameseIdentityErrorDescriber : IdentityErrorDescriber
    {
        private static IdentityError Error(string code, string description) =>
            new() { Code = code, Description = description };

        public override IdentityError DefaultError() =>
            Error(nameof(DefaultError), "Đã xảy ra lỗi. Vui lòng thử lại.");

        public override IdentityError ConcurrencyFailure() =>
            Error(nameof(ConcurrencyFailure), "Dữ liệu đã thay đổi. Vui lòng thử lại.");

        public override IdentityError PasswordMismatch() =>
            Error(nameof(PasswordMismatch), "Mật khẩu hiện tại không chính xác.");

        public override IdentityError InvalidToken() =>
            Error(nameof(InvalidToken), "Mã xác thực không hợp lệ hoặc đã hết hạn.");

        public override IdentityError LoginAlreadyAssociated() =>
            Error(nameof(LoginAlreadyAssociated), "Tài khoản đăng nhập này đã được liên kết với người dùng khác.");

        public override IdentityError InvalidUserName(string? userName) =>
            Error(nameof(InvalidUserName), "Tên đăng nhập không hợp lệ.");

        public override IdentityError InvalidEmail(string? email) =>
            Error(nameof(InvalidEmail), "Địa chỉ email không hợp lệ.");

        public override IdentityError DuplicateUserName(string userName) =>
            Error(nameof(DuplicateUserName), "Tên đăng nhập đã được sử dụng.");

        public override IdentityError DuplicateEmail(string email) =>
            Error(nameof(DuplicateEmail), "Địa chỉ email đã được sử dụng.");

        public override IdentityError InvalidRoleName(string? role) =>
            Error(nameof(InvalidRoleName), "Tên vai trò không hợp lệ.");

        public override IdentityError DuplicateRoleName(string role) =>
            Error(nameof(DuplicateRoleName), "Tên vai trò đã tồn tại.");

        public override IdentityError UserAlreadyHasPassword() =>
            Error(nameof(UserAlreadyHasPassword), "Tài khoản đã có mật khẩu.");

        public override IdentityError UserLockoutNotEnabled() =>
            Error(nameof(UserLockoutNotEnabled), "Tính năng khóa tài khoản chưa được bật.");

        public override IdentityError UserAlreadyInRole(string role) =>
            Error(nameof(UserAlreadyInRole), "Người dùng đã thuộc vai trò này.");

        public override IdentityError UserNotInRole(string role) =>
            Error(nameof(UserNotInRole), "Người dùng không thuộc vai trò này.");

        public override IdentityError PasswordTooShort(int length) =>
            Error(nameof(PasswordTooShort), $"Mật khẩu phải có ít nhất {length} ký tự.");

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
            Error(nameof(PasswordRequiresUniqueChars), $"Mật khẩu phải có ít nhất {uniqueChars} ký tự khác nhau.");

        public override IdentityError PasswordRequiresNonAlphanumeric() =>
            Error(nameof(PasswordRequiresNonAlphanumeric), "Mật khẩu phải có ít nhất một ký tự đặc biệt.");

        public override IdentityError PasswordRequiresDigit() =>
            Error(nameof(PasswordRequiresDigit), "Mật khẩu phải có ít nhất một chữ số.");

        public override IdentityError PasswordRequiresLower() =>
            Error(nameof(PasswordRequiresLower), "Mật khẩu phải có ít nhất một chữ cái thường.");

        public override IdentityError PasswordRequiresUpper() =>
            Error(nameof(PasswordRequiresUpper), "Mật khẩu phải có ít nhất một chữ cái in hoa.");
    }
}
