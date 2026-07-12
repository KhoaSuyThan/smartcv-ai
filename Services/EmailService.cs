using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace DoAnCS.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public EmailService(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            // Bảo mật & Tiết kiệm tài nguyên: Bỏ qua gửi email thực đối với tài khoản kiểm thử E2E
            if (string.IsNullOrEmpty(email) || email.StartsWith("test_e2e_"))
            {
                return;
            }

            // Ở môi trường localhost (Development), chỉ gửi các email xác thực (OTP), Offer, Lịch phỏng vấn.
            if (_env.IsDevelopment() && !ShouldSendEmailOnLocal(subject, message))
            {
                Console.WriteLine($"[Email Filter] Đã chặn gửi email phụ ở localhost: Subject = '{subject}' gửi tới '{email}'");
                return;
            }

            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpServer = emailSettings["SmtpServer"];
            var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
            var senderName = emailSettings["SenderName"];
            var senderEmail = emailSettings["SenderEmail"];
            var senderPassword = emailSettings["SenderPassword"];

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail!, senderName),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }

        /// <summary>
        /// Bộ lọc kiểm tra các email được phép gửi ở môi trường Localhost (Development).
        /// Chỉ cho phép: OTP/Xác thực/Mật khẩu, Job Offer gửi cho ứng viên, và Lịch phỏng vấn.
        /// </summary>
        private bool ShouldSendEmailOnLocal(string subject, string message)
        {
            if (string.IsNullOrEmpty(subject)) return false;

            string subLower = subject.ToLower();

            // Loại trừ email phản hồi của ứng viên gửi cho Recruiter (không phải thư mời nhận việc gửi đi)
            if (subLower.Contains("phản hồi thư mời nhận việc") || subLower.Contains("phản hồi offer"))
            {
                return false;
            }

            // 1. Nhóm Xác thực / OTP / Mật khẩu
            if (subLower.Contains("otp") || 
                subLower.Contains("mã xác nhận") || 
                subLower.Contains("xác thực") || 
                subLower.Contains("verify") || 
                subLower.Contains("mật khẩu") || 
                subLower.Contains("password") || 
                subLower.Contains("kích hoạt") || 
                subLower.Contains("register") ||
                subLower.Contains("đổi mật khẩu"))
            {
                return true;
            }

            // 2. Nhóm Job Offer (Thư mời nhận việc gửi cho ứng viên)
            if (subLower.Contains("thư mời nhận việc") || 
                subLower.Contains("job offer") || 
                subLower.Contains("mời nhận việc") ||
                subLower.Contains("offer nhận việc"))
            {
                return true;
            }

            // 3. Nhóm Lịch phỏng vấn
            if (subLower.Contains("phỏng vấn") || 
                subLower.Contains("interview") || 
                subLower.Contains("lịch hẹn") ||
                subLower.Contains("lịch phỏng vấn"))
            {
                return true;
            }

            return false;
        }
    }
}
