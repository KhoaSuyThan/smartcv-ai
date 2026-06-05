using Microsoft.AspNetCore.Mvc;
using DoAnCS.Models;
using DoAnCS.Data;
namespace DoAnCS.Controllers;
using System.Net;
using System.Net.Mail;

public class SupportController : Controller
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    public SupportController(AppDbContext context, IConfiguration config){
        _context = context;
        _config = config;
    }

    // Trang Hướng dẫn sử dụng
    public IActionResult Guide() => View();

    // Trang Liên hệ (Giao diện)
    [HttpGet]
    public IActionResult Contact() => View();

    // Trang Liên hệ (Xử lý gửi form)
    [HttpPost]
    public async Task<IActionResult> Contact(ContactMessage model, IFormFile? Attachment)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // 1. Xử lý lưu file nếu có đính kèm
                if (Attachment != null && Attachment.Length > 0)
                {
                    var baseUploadsFolder = _config["StorageSettings:UploadsFolder"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "DoAnWeb_Uploads");
                    string uploadsFolder = Path.Combine(baseUploadsFolder, "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Attachment.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Attachment.CopyToAsync(fileStream);
                    }
                    model.AttachmentUrl = "/uploads/" + uniqueFileName;
                }

                _context.ContactMessages.Add(model);
                await _context.SaveChangesAsync();

                // 2. Gửi Email (Truyền thêm file nếu có)
                await SendEmailToAdmin(model, Attachment);

                TempData["Success"] = "Tin nhắn đã được gửi thành công!";
                return RedirectToAction("Contact");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi Contact: " + ex.Message);
                ModelState.AddModelError("", "Lỗi hệ thống, hãy liên hệ admin để giải quyết");
            }
        }
        return View(model);
    }

    // Hàm hỗ trợ gửi thư
    private async Task SendEmailToAdmin(ContactMessage contact, IFormFile? attachment)
    {
        // 1. Lấy thông tin từ "két sắt" hoặc appsettings.json
        var senderEmail = _config["EmailSettings:SenderEmail"] ?? "khoanv249@gmail.com";
        var senderPassword = _config["EmailSettings:SenderPassword"]; // Lấy mật khẩu đã ẩn
        
        var fromAddress = new MailAddress(senderEmail, "CVBuilder Web");
        var toAddress = new MailAddress(senderEmail); // Gửi về chính mình

        // 2. Nội dung Email (Giữ nguyên logic của bạn)
        string body = $@"
            <h3>Bạn có tin nhắn liên hệ mới từ Web!</h3>
            <p><b>Người gửi:</b> {contact.Name}</p>
            <p><b>Email:</b> {contact.Email}</p>
            <p><b>Tiêu đề:</b> {contact.Subject}</p>
            <p><b>Nội dung:</b><br/>{contact.Message}</p>
            <hr/>
            <p>Tin nhắn này được gửi tự động từ hệ thống CVBuilder.</p>";

        // 3. Cấu hình SMTP
        using (var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, senderPassword) // Dùng mật khẩu ẩn
        })
        {   
            using (var message = new MailMessage(fromAddress, toAddress))
            {
                message.Subject = "[CVBuilder] Liên hệ mới: " + (contact.Subject ?? "Không có tiêu đề");
                message.Body = body;
                message.IsBodyHtml = true;
                message.ReplyToList.Add(new MailAddress(contact.Email));

                // NẾU CÓ FILE THÌ ĐÍNH KÈM VÀO EMAIL
                if (attachment != null && attachment.Length > 0)
                {
                    var stream = attachment.OpenReadStream();
                    message.Attachments.Add(new Attachment(stream, attachment.FileName));
                }

                await smtp.SendMailAsync(message);
            }
        }
    }
    public IActionResult Terms()
    {
        return View();
    }
}