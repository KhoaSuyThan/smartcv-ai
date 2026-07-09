using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Models;
using System.Threading.Tasks;
using System.Text.Json;
using DoAnCS.Data; 
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using Ganss.Xss; // Dùng cho HTML Sanitization
using Newtonsoft.Json.Linq; // Sử dụng JToken để duyệt JSON linh hoạt

namespace DoAnCS.Controllers
{
    [ApiController]
    [Route("api/cvbuilder")]
    public class CVBuilderApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CVBuilderApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/cvbuilder/data/5
        [HttpGet("data/{id}")]
        public async Task<IActionResult> GetCVData(int id)
        {
            var resume = await _context.Resumes
                .Include(r => r.Template)
                .FirstOrDefaultAsync(r => r.ResumeID == id);
            
            if (resume == null)
            {
                return NotFound(new { message = "Không tìm thấy CV." });
            }

            // KIỂM TRA BẢO MẬT: BOLA/IDOR
            if (!resume.IsPublic)
            {
                var userIdClaim = User.FindFirst("UserID")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int currentUserId = int.TryParse(userIdClaim, out int uid) ? uid : 0;

                if (resume.UserID != currentUserId && !User.IsInRole("Admin"))
                {
                    return Unauthorized(new { message = "Bạn không có quyền truy cập CV này." });
                }
            }

            // Thử tìm trong VueTemplates trước để lấy ComponentName chính xác
            var vueTemplate = await _context.VueTemplates.FirstOrDefaultAsync(t => t.Id == resume.TemplateID);
            
            // Kiểm tra trạng thái Pro của chủ sở hữu CV
            var owner = await _context.Users.FirstOrDefaultAsync(u => u.UserID == resume.UserID);
            bool isPro = owner?.IsPro ?? false;

            // Trả về dữ liệu JSON hoặc cấu trúc trúc rỗng nếu chưa có
            return Ok(new 
            { 
                id = resume.ResumeID,
                title = resume.Title,
                templateId = resume.TemplateID,
                templateName = vueTemplate?.TemplateName ?? resume.Template?.Name, 
                componentName = vueTemplate?.ComponentName, // Trả thêm ComponentName để Vue dùng trực tiếp
                jsonContent = resume.JsonContent ?? "{}",
                isPro = isPro // Trạng thái Pro để frontend quyết định hiển thị watermark
            });
        }

        // POST: api/cvbuilder/save/5
        [HttpPost("save/{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> SaveCVData(int id, [FromBody] SaveDataRequest request)
        {
            var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.ResumeID == id);
            if (resume == null)
            {
                return NotFound(new { message = "Không tìm thấy CV." });
            }

            // KIỂM TRA BẢO MẬT: Phải là chủ sở hữu mới được lưu CV
            var userIdClaim = User.FindFirst("UserID")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int currentUserId = int.TryParse(userIdClaim, out int uid) ? uid : 0;

            if (resume.UserID != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Lọc mã độc HTML/XSS từ nội dung JSON tự do do người dùng nhập trước khi lưu xuống DB
            string sanitizedJson = SanitizeJsonContent(request.JsonContent);

            // Lưu toàn bộ cấu trúc Vue vào trường JsonContent đã làm sạch
            resume.JsonContent = sanitizedJson;
            resume.UpdatedAt = System.DateTime.Now;
            resume.IsDraft = true; // Lưu nháp

            // Trích xuất dữ liệu General từ JSON để lưu vào các cột tương ứng (Hỗ trợ tìm kiếm sau này)
            if (!string.IsNullOrEmpty(sanitizedJson))
            {
                try
                {
                    using (JsonDocument doc = JsonDocument.Parse(sanitizedJson))
                    {
                        // Đồng bộ TemplateID trong database nếu người dùng đổi sang mẫu CV mới
                        if (doc.RootElement.TryGetProperty("overrideTemplate", out JsonElement overrideTpl))
                        {
                            string compName = overrideTpl.GetString();
                            if (!string.IsNullOrEmpty(compName))
                            {
                                var vt = await _context.VueTemplates.FirstOrDefaultAsync(t => t.ComponentName == compName);
                                if (vt != null && resume.TemplateID != vt.Id)
                                {
                                    resume.TemplateID = vt.Id;
                                }
                            }
                        }

                        if (doc.RootElement.TryGetProperty("general", out JsonElement general))
                        {
                            if (general.TryGetProperty("fullName", out JsonElement fn)) resume.FullName = fn.GetString();
                            if (general.TryGetProperty("jobTitle", out JsonElement jt)) resume.JobTitle = jt.GetString();
                            if (general.TryGetProperty("email", out JsonElement em)) resume.Email = em.GetString();
                            if (general.TryGetProperty("phone", out JsonElement ph)) resume.Phone = ph.GetString();
                            if (general.TryGetProperty("address", out JsonElement addr)) resume.Address = addr.GetString();
                            if (general.TryGetProperty("summary", out JsonElement sum)) resume.Summary = sum.GetString();
                        }

                        // Đồng bộ phân mục (Sections) vào bảng ResumeSections cũ
                        if (doc.RootElement.TryGetProperty("sections", out JsonElement sectionsJson))
                        {
                            foreach (JsonElement section in sectionsJson.EnumerateArray())
                            {
                                if (section.TryGetProperty("id", out JsonElement secId) && 
                                    section.TryGetProperty("items", out JsonElement itemsJson))
                                {
                                    string typeId = secId.GetString() ?? "";
                                    if (string.IsNullOrEmpty(typeId)) continue;

                                    // Chuẩn hóa tên khóa (ex: experience -> Experience, project -> Projects)
                                    string mappedType = char.ToUpper(typeId[0]) + typeId.Substring(1);
                                    if (mappedType == "Project") mappedType = "Projects";
                                    
                                    string itemsStr = itemsJson.GetRawText();

                                    var existingSection = await _context.ResumeSections
                                        .FirstOrDefaultAsync(s => s.ResumeID == id && s.SectionType == mappedType);

                                    if (existingSection == null)
                                    {
                                        _context.ResumeSections.Add(new ResumeSection
                                        {
                                            ResumeID = id,
                                            SectionType = mappedType,
                                            ContentJSON = itemsStr
                                        });
                                    }
                                    else
                                    {
                                        existingSection.ContentJSON = itemsStr;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Bỏ qua lỗi Parse JSON nếu cấu trúc hỏng
                }
            }

            // Xóa cache vector cũ của CV này do nội dung đã thay đổi
            var oldCache = await _context.CVEmbeddings.Where(e => e.ResumeID == id).ToListAsync();
            if (oldCache.Any())
            {
                _context.CVEmbeddings.RemoveRange(oldCache);
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã lưu CV thành công." });
        }

        // GET: api/cvbuilder/templates
        // Lấy danh sách các mẫu CV Vue đang hoạt động để hiển thị trong Modal đổi mẫu
        [HttpGet("templates")]
        public async Task<IActionResult> GetVueTemplates()
        {
            var templates = await _context.VueTemplates
                .Where(t => t.IsActive == true)
                .Select(t => new
                {
                    id = t.Id,
                    templateName = t.TemplateName,
                    componentName = t.ComponentName,
                    thumbnailUrl = t.ThumbnailUrl,
                    isPremium = t.IsPremium,
                    category = t.Category
                })
                .ToListAsync();

            return Ok(templates);
        }

        /// <summary>
        /// Giải mã và lọc sạch các chuỗi HTML/XSS trong toàn bộ cấu trúc JSON của CV
        /// </summary>
        private string SanitizeJsonContent(string jsonStr)
        {
            if (string.IsNullOrWhiteSpace(jsonStr)) return jsonStr;
            try
            {
                var token = JToken.Parse(jsonStr);
                var sanitizer = new HtmlSanitizer();
                
                // Lọc bỏ mã độc JavaScript, thẻ script, các sự kiện onerror, onload...
                SanitizeJToken(token, sanitizer);
                return token.ToString(Newtonsoft.Json.Formatting.None);
            }
            catch
            {
                return jsonStr;
            }
        }

        /// <summary>
        /// Duyệt đệ quy qua JToken để làm sạch mọi thuộc tính có kiểu dữ liệu chuỗi (String)
        /// </summary>
        private void SanitizeJToken(JToken token, HtmlSanitizer sanitizer)
        {
            if (token is JObject obj)
            {
                foreach (var property in obj.Properties())
                {
                    if (property.Value.Type == JTokenType.String)
                    {
                        var val = property.Value.ToString();
                        property.Value = sanitizer.Sanitize(val);
                    }
                    else
                    {
                        SanitizeJToken(property.Value, sanitizer);
                    }
                }
            }
            else if (token is JArray arr)
            {
                for (int i = 0; i < arr.Count; i++)
                {
                    if (arr[i].Type == JTokenType.String)
                    {
                        var val = arr[i].ToString();
                        arr[i] = sanitizer.Sanitize(val);
                    }
                    else
                    {
                        SanitizeJToken(arr[i], sanitizer);
                    }
                }
            }
        }
    }

    public class SaveDataRequest
    {
        public string JsonContent { get; set; }
    }
}
