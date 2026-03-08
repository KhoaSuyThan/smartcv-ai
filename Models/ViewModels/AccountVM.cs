using System.ComponentModel.DataAnnotations;

public class RegisterVM {
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    public string FullName { get; set; }
    
    [Required, EmailAddress]
    public string Email { get; set; }
    
    [Required, MinLength(6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự")]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
    public string ConfirmPassword { get; set; }
}

public class LoginVM {
    [Required, EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}