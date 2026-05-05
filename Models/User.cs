using System;
using System.Collections.Generic;

namespace DoAnCS.Models {
    public class User {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? Phone { get; set; }
        public int? CompanyID { get; set; }
        public virtual Company? Company { get; set; }
        public string? Role { get; set; } = "User"; // Mặc định là User
        public bool IsPro { get; set; } = false; // Tài khoản Pro
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Các trường phục vụ xác thực đổi mật khẩu
        public string? PasswordChangeToken { get; set; }
        public DateTime? PasswordChangeTokenExpires { get; set; }
        public string? PendingPasswordHash { get; set; }

        // Các trường phục vụ quên mật khẩu bằng OTP
        public string? PasswordResetOTP { get; set; }
        public DateTime? OTPExpires { get; set; }
        public int? OTPFailCount { get; set; } = 0;
        
        public string? Summary { get; set; } // Giới thiệu bản thân
        public string? Skills { get; set; }  // Các kỹ năng (ví dụ: C#, React, SQL)

        public virtual ICollection<Resume> Resumes { get; set; }   
    }
}