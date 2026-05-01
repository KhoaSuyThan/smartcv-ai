using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Models;
using System.Threading.Tasks;
using System.Text.Json;
using DoAnCS.Data; 
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
        public async Task<IActionResult> SaveCVData(int id, [FromBody] SaveDataRequest request)
        {
            var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.ResumeID == id);
            if (resume == null)
            {
                return NotFound(new { message = "Không tìm thấy CV." });
            }

            // Lưu toàn bộ cấu trúc Vue vào trường JsonContent
            resume.JsonContent = request.JsonContent;
            resume.UpdatedAt = System.DateTime.Now;
            resume.IsDraft = true; // Lưu nháp

            // Trích xuất dữ liệu General từ JSON để lưu vào các cột tương ứng (Hỗ trợ tìm kiếm sau này)
            if (!string.IsNullOrEmpty(request.JsonContent))
            {
                try
                {
                    using (JsonDocument doc = JsonDocument.Parse(request.JsonContent))
                    {
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

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã lưu CV thành công." });
        }
    }

    public class SaveDataRequest
    {
        public string JsonContent { get; set; }
    }
}
