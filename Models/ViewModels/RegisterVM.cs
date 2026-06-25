using System.ComponentModel.DataAnnotations;

namespace DoAnCS.Models.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$", ErrorMessage = "Mật khẩu phải từ 8 ký tự trở lên, chứa ít nhất 1 chữ cái, 1 chữ số và 1 ký tự đặc biệt")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }

        public string Role { get; set; } = "User"; 
        public string? CompanyID { get; set; }
        public string? CompanyName { get; set; }
        public string? TaxCode { get; set; }
    }
}