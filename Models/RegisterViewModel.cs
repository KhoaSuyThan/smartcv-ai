using System.ComponentModel.DataAnnotations;

namespace DoAnLTWeb.Models
{
    public class RegisterViewModel // Model dùng để nhận dữ liệu từ form đăng ký
    {
        [Required(ErrorMessage = "Không được để trống họ tên")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Không được để trống email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Không được để trống mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string? Password { get; set; }
    }
}