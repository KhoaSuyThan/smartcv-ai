using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Data;
using DoAnCS.Models;
using Microsoft.AspNetCore.Authorization;

namespace DoAnCS.Controllers
{
    public class JobsController : BaseController
    {
        private readonly AppDbContext _context;

        public JobsController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. DÀNH CHO TẤT CẢ (Ứng viên xem tin)
        // ==========================================
        [AllowAnonymous] // Cho phép khách xem danh sách việc làm
        public async Task<IActionResult> Index()
        {
            var jobs = await _context.Jobs
                .Include(j => j.Company)
                .Where(j => j.Status == 1)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
            return View(jobs);
        }

        // ==========================================
        // 2. DÀNH CHO NHÀ TUYỂN DỤNG & ADMIN (Quản lý tin)
        // ==========================================
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> Manage()
        {
            // 1. Khởi tạo Query lấy kèm thông tin Công ty
            IQueryable<Job> query = _context.Jobs.Include(j => j.Company);

            // 2. Kiểm tra quyền của người dùng đang đăng nhập
            if (User.IsInRole("Recruiter"))
            {
                // Lấy CompanyID từ Claim (đã lưu lúc đăng nhập)
                var companyIdClaim = User.FindFirst("CompanyID")?.Value;

                if (companyIdClaim != null)
                {
                    int currentCompanyId = int.Parse(companyIdClaim);
                    // CHỈ LẤY các tin thuộc công ty này
                    query = query.Where(j => j.CompanyID == currentCompanyId);
                }
                else
                {
                    // Nếu không tìm thấy CompanyID, trả về danh sách rỗng để bảo mật
                    return View(new List<Job>());
                }
            }
            // Nếu là Admin thì không lọc (query giữ nguyên để thấy hết)

            // 3. Sắp xếp và thực thi truy vấn
            var jobs = await query
                .OrderBy(j => j.Status == 0 ? 0 : 1) // Tin chờ duyệt lên đầu
                .ThenByDescending(j => j.CreatedAt)
                .ToListAsync();

            // === DASHBOARD STATS CALCULATION ===
            var jobIds = jobs.Select(j => j.JobID).ToList();
            int totalApps = 0;
            int pendingApps = 0;
            int reviewingApps = 0;
            int acceptedApps = 0;
            int rejectedApps = 0;

            var timelineLabels = new List<string>();
            var timelineValues = new List<int>();
            var timelineMonthLabels = new List<string>();
            var timelineMonthValues = new List<int>();
            var topSkills = new List<SkillStat>();

            if (jobIds.Any())
            {
                // Total Applications
                totalApps = await _context.Applications.CountAsync(a => jobIds.Contains(a.JobID));

                // Application Status Counts
                var appStatusCounts = await _context.Applications
                    .Where(a => jobIds.Contains(a.JobID))
                    .GroupBy(a => a.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(g => g.Status, g => g.Count);

                pendingApps = appStatusCounts.ContainsKey("Pending") ? appStatusCounts["Pending"] : 0;
                reviewingApps = appStatusCounts.ContainsKey("Reviewing") ? appStatusCounts["Reviewing"] : 0;
                acceptedApps = appStatusCounts.ContainsKey("Accepted") ? appStatusCounts["Accepted"] : 0;
                rejectedApps = appStatusCounts.ContainsKey("Rejected") ? appStatusCounts["Rejected"] : 0;

                // Timeline: last 7 days
                var last7Days = Enumerable.Range(0, 7)
                    .Select(i => DateTime.Today.AddDays(-i))
                    .OrderBy(d => d)
                    .ToList();

                var timelineCounts = await _context.Applications
                    .Where(a => jobIds.Contains(a.JobID) && a.AppliedAt >= last7Days.First())
                    .GroupBy(a => a.AppliedAt.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(g => g.Date, g => g.Count);

                timelineLabels = last7Days.Select(d => d.ToString("dd/MM")).ToList();
                timelineValues = last7Days.Select(d => timelineCounts.ContainsKey(d) ? timelineCounts[d] : 0).ToList();

                // Timeline: last 4 weeks (Month)
                var last4Weeks = new List<(DateTime Start, DateTime End, string Label)>();
                for (int i = 3; i >= 0; i--)
                {
                    DateTime start = DateTime.Today.AddDays(-((i + 1) * 7 - 1));
                    DateTime end = DateTime.Today.AddDays(-(i * 7));
                    if (i == 3)
                    {
                        // Đảm bảo tuần đầu bao gồm trọn vẹn 30 ngày (9 ngày đầu tiên)
                        start = DateTime.Today.AddDays(-29);
                    }
                    string label = $"Tuần {(4 - i)} ({start:dd/MM}-{end:dd/MM})";
                    last4Weeks.Add((start, end, label));
                }

                var allMonthApps = await _context.Applications
                    .Where(a => jobIds.Contains(a.JobID) && a.AppliedAt >= DateTime.Today.AddDays(-29))
                    .Select(a => a.AppliedAt.Date)
                    .ToListAsync();

                timelineMonthLabels = new List<string>();
                timelineMonthValues = new List<int>();

                foreach (var week in last4Weeks)
                {
                    timelineMonthLabels.Add(week.Label);
                    int count = allMonthApps.Count(a => a >= week.Start && a <= week.End);
                    timelineMonthValues.Add(count);
                }

                // Top Skills Required
                var techTerms = new[] {
                    "C#", ".NET", "ASP.NET", "Java", "Spring Boot", "Python", "Django", "JavaScript", "TypeScript",
                    "React", "Angular", "Vue", "Node.js", "Express", "Next.js", "HTML", "CSS", "SQL", "SQL Server",
                    "MySQL", "PostgreSQL", "MongoDB", "Redis", "Docker", "Kubernetes", "AWS", "Azure", "CI/CD",
                    "Git", "GitHub", "REST", "API", "Microservices", "Figma", "UI/UX", "DevOps"
                };

                var skillCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var job in jobs)
                {
                    var jobText = $"{job.Title} {job.Description} {job.Requirements}".ToLower();
                    foreach (var term in techTerms)
                    {
                        if (jobText.Contains(term.ToLower()))
                        {
                            if (skillCounts.ContainsKey(term))
                                skillCounts[term]++;
                            else
                                skillCounts[term] = 1;
                        }
                    }
                }

                topSkills = skillCounts
                    .OrderByDescending(x => x.Value)
                    .Take(5)
                    .Select(x => new SkillStat 
                    { 
                        Skill = x.Key, 
                        Count = x.Value, 
                        Percentage = jobs.Any() ? (int)Math.Round((double)x.Value / jobs.Count * 100) : 0 
                    })
                    .ToList();
            }

            ViewBag.TotalJobs = jobs.Count;
            ViewBag.PendingJobs = jobs.Count(j => j.Status == 0);
            ViewBag.TotalApplications = totalApps;
            ViewBag.PendingApps = pendingApps;
            ViewBag.ReviewingApps = reviewingApps;
            ViewBag.AcceptedApps = acceptedApps;
            ViewBag.RejectedApps = rejectedApps;
            ViewBag.TimelineLabels = timelineLabels;
            ViewBag.TimelineValues = timelineValues;
            ViewBag.TimelineMonthLabels = timelineMonthLabels;
            ViewBag.TimelineMonthValues = timelineMonthValues;
            ViewBag.TopSkills = topSkills;

            return View(jobs);
        }

        // ==========================================
        // 3. ĐĂNG TIN MỚI 
        // ==========================================
        // [GET] Hiển thị form đăng tin
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> Create()
        {
            // LUÔN LUÔN nạp danh sách, kể cả không dùng đến để tránh lỗi Null ở View
            var companies = await _context.Companies.ToListAsync();
            ViewBag.Companies = companies ?? new List<Company>(); 
            
            return View();
        }

        // [POST] Xử lý lưu tin tuyển dụng
        [HttpPost]
        [Authorize(Roles = "Recruiter,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Job job)
        {
            // 1. Gỡ bỏ kiểm tra các trường không nhập từ Form
            ModelState.Remove("Recruiter");
            ModelState.Remove("Company");
            ModelState.Remove("Applications");

            if (ModelState.IsValid)
            {
                // 2. Thiết lập các thông tin mặc định
                job.CreatedAt = DateTime.Now;
                job.RecruiterID = CurrentUserId; // Giả định bạn đã có thuộc tính này trong BaseController
                
                // Admin đăng thì duyệt luôn (1), Recruiter đăng thì chờ duyệt (0)
                job.Status = User.IsInRole("Admin") ? 1 : 0;

                // 3. Xử lý ID Công ty dựa trên Role
                if (User.IsInRole("Recruiter"))
                {
                    // Lấy trực tiếp từ DB để đảm bảo an toàn dữ liệu
                    var userInDb = await _context.Users.FindAsync(CurrentUserId);
                    job.CompanyID = userInDb?.CompanyID ?? 0;
                }
                // Nếu là Admin thì job.CompanyID đã được lấy từ Dropdown qua Model Binding

                // 4. Kiểm tra ID công ty lần cuối trước khi lưu
                if (job.CompanyID == null || job.CompanyID == 0)
                {
                    ModelState.AddModelError("", "Lỗi: Không xác định được công ty. Vui lòng kiểm tra lại thông tin tài khoản.");
                }
                else
                {
                    _context.Add(job);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Đăng tin tuyển dụng thành công!";
                    return RedirectToAction(nameof(Manage));
                }
            }

            // FIX: Nếu có lỗi (ModelState không hợp lệ), PHẢI nạp lại ViewBag trước khi trả về View
            // Nếu không nạp lại, khi View load lại sẽ bị lỗi NullReferenceException ngay
            ViewBag.Companies = await _context.Companies.ToListAsync();
            
            return View(job);
        }

        [HttpGet]
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            // Check quyền sở hữu
            if (CurrentRole == "Recruiter" && job.CompanyID != CurrentCompanyId) return Forbid();

            if (CurrentRole == "Admin") ViewBag.Companies = await _context.Companies.ToListAsync();
            return View(job);
        }

        [HttpPost]
        [Authorize(Roles = "Recruiter,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Job job)
        {
            if (id != job.JobID) return NotFound();

            // 1. Tìm bản ghi gốc TRONG DATABASE (Không tin vào dữ liệu gửi từ View hoàn toàn)
            var jobInDb = await _context.Jobs.FindAsync(id);
            if (jobInDb == null) return NotFound();

            // 2. Check quyền sở hữu (Security check lần 2)
            if (User.IsInRole("Recruiter") && jobInDb.CompanyID != CurrentCompanyId) 
                return Forbid();

            // 3. Gỡ bỏ kiểm tra các trường không cần nhập từ Form để ModelState hợp lệ
            ModelState.Remove("Recruiter");
            ModelState.Remove("Company");
            ModelState.Remove("Applications");
            // Nếu vẫn lỗi, Khoa thêm đoạn này để debug xem trường nào đang 'hành' mình:
            // var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                try
                {
                    // 4. Chỉ cập nhật những gì người dùng được phép sửa
                    jobInDb.Title = job.Title;
                    jobInDb.Description = job.Description;
                    jobInDb.Requirements = job.Requirements;
                    jobInDb.Salary = job.Salary;
                    jobInDb.Deadline = job.Deadline;

                    // Xử lý trạng thái theo Role
                    if (User.IsInRole("Admin"))
                    {
                        // Admin chọn gì lưu nấy
                        jobInDb.Status = job.Status; 
                    }
                    else
                    {
                        // Nếu là Recruiter sửa bài, ép về 0 để chờ Admin duyệt lại (nếu hệ thống yêu cầu)
                        // Hoặc nếu muốn cho họ tự đóng/mở tin thì cũng dùng: jobInDb.Status = job.Status;
                        jobInDb.Status = 0; 
                    }

                    _context.Update(jobInDb);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thành công!";
                    
                    if (User.IsInRole("Admin"))
                    {
                        return RedirectToAction("Jobs", "Admin");
                    }
                    return RedirectToAction(nameof(Manage));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobExists(job.JobID)) return NotFound();
                    else throw;
                }
            }
            
            // Nếu lỗi, trả lại dữ liệu gốc từ DB để View không bị trắng các trường ẩn
            return View(job);
        }

        // Hàm bổ trợ kiểm tra sự tồn tại của Job
        private bool JobExists(int id)
        {
            // Kiểm tra xem trong bảng Jobs có bất kỳ dòng nào khớp với ID này không
            return _context.Jobs.Any(e => e.JobID == id);
        }
        
        // ==========================================
        // 4. XÓA TIN (Chủ sở hữu hoặc Admin)
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            // Bảo mật: Nếu là Recruiter, phải check xem tin này có phải của mình không
            if (CurrentRole == "Recruiter" && job.CompanyID != CurrentCompanyId)
            {
                return Forbid(); // Trả về lỗi 403 - Cấm truy cập
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage));
        }

        // ==========================================
        // 5. CHỨC NĂNG DÀNH RIÊNG CHO ADMIN DUYỆT TIN
        // ==========================================


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id, string returnUrl = null)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job != null)
            {
                job.Status = 1;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã duyệt bài!";
            }

            // Nếu có địa chỉ quay lại (từ Admin Dashboard) thì về đó, không thì về Manage
            if (!string.IsNullOrEmpty(returnUrl)) return LocalRedirect(returnUrl);
            return RedirectToAction(nameof(Manage));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id, string returnUrl = null)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job != null)
            {
                job.Status = 2;
                await _context.SaveChangesAsync();
                TempData["Error"] = "Đã từ chối bài!";
            }

            if (!string.IsNullOrEmpty(returnUrl)) return LocalRedirect(returnUrl);
            return RedirectToAction(nameof(Manage));
        }

        // ==========================================
        // 6. QUẢN LÝ ỨNG VIÊN (RECRUITER)
        // ==========================================
        [HttpGet]
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> Candidates(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            // Check quyền sở hữu nếu là Recruiter
            if (User.IsInRole("Recruiter") && job.CompanyID != CurrentCompanyId)
                return Forbid();

            var applications = await _context.Applications
                .Include(a => a.Resume)
                    .ThenInclude(r => r.User) // Để lấy thông tin liên hệ của ứng viên
                .Where(a => a.JobID == id)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            ViewBag.JobTitle = job.Title;
            ViewBag.JobId = job.JobID;
            return View(applications);
        }

        [HttpPost]
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> UpdateApplicationStatus(int applicationId, string status)
        {
            var application = await _context.Applications.Include(a => a.Job).FirstOrDefaultAsync(a => a.ApplicationID == applicationId);
            if (application == null) return Json(new { success = false, message = "Không tìm thấy đơn ứng tuyển." });

            if (User.IsInRole("Recruiter") && application.Job.CompanyID != CurrentCompanyId)
                return Json(new { success = false, message = "Bạn không có quyền thay đổi trạng thái đơn này." });

            application.Status = status;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
        }

        [HttpGet]
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> PreviewCV(int applicationId)
        {
            var application = await _context.Applications
                .Include(a => a.Job)
                .Include(a => a.Resume)
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);

            if (application == null) return NotFound("Không tìm thấy đơn ứng tuyển.");

            // Kiểm tra quyền: Chỉ Admin hoặc Recruiter sở hữu Job này mới được xem
            var companyIdClaim = User.FindFirst("CompanyID")?.Value;
            int currentCompanyId = !string.IsNullOrEmpty(companyIdClaim) ? int.Parse(companyIdClaim) : 0;

            if (User.IsInRole("Recruiter") && application.Job.CompanyID != currentCompanyId)
            {
                return Forbid();
            }

            ViewBag.ResumeId = application.ResumeID;
            return View("~/Views/Resume/PublicViewerCVVue.cshtml", application.Resume);
        }
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "User,Admin,Recruiter")]
        [IgnoreAntiforgeryToken] // Tạm thời Ignore token đối với body JSON nếu front-end chưa gửi kèm cookie, hoặc nên dùng [FromBody] cẩn thận
        public async Task<IActionResult> ApplyOneClick([FromBody] ApplyOneClickRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserID")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return Json(new { success = false, message = "Bạn phải đăng nhập để ứng tuyển." });

                // Validate Resume
                var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.ResumeID == request.ResumeId && r.UserID == userId);
                if (resume == null) return Json(new { success = false, message = "Lỗi: Không tìm thấy CV hoặc CV không thuộc về bạn." });

                // Validate Job
                var job = await _context.Jobs.FindAsync(request.JobId);
                if (job == null) return Json(new { success = false, message = "Lỗi: Tin tuyển dụng không tồn tại hoặc đã bị xóa." });

                // Check Multiple Apply
                bool alreadyApplied = await _context.Applications
                    .Include(a => a.Resume)
                    .AnyAsync(a => a.JobID == request.JobId && a.Resume.UserID == userId && (a.Status == "Pending" || a.Status == "Reviewing"));

                if (alreadyApplied)
                    return Json(new { success = false, message = "Bạn đã nộp đơn cho vị trí này rồi. Vui lòng chờ nhà tuyển dụng phản hồi." });

                // Register Application
                var application = new Application
                {
                    JobID = request.JobId,
                    ResumeID = request.ResumeId,
                    AppliedAt = DateTime.Now,
                    Status = "Pending"
                };

                _context.Applications.Add(application);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Ứng tuyển thành công!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi ApplyOneClick: " + ex.Message);
                return Json(new { success = false, message = "Lỗi hệ thống, hãy liên hệ admin để giải quyết" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> WithdrawApplication(int applicationId)
        {
            try
            {
                var userId = CurrentUserId;
                if (userId == 0) return Json(new { success = false, message = "Hết phiên đăng nhập." });

                var application = await _context.Applications
                    .Include(a => a.Resume)
                    .FirstOrDefaultAsync(a => a.ApplicationID == applicationId && a.Resume.UserID == userId);

                if (application == null)
                    return Json(new { success = false, message = "Không tìm thấy đơn ứng tuyển này." });

                // Chỉ cho phép hủy nếu đơn vẫn đang chờ hoặc đang xem
                if (application.Status != "Pending" && application.Status != "Reviewing")
                    return Json(new { success = false, message = "Không thể hủy đơn đã được xử lý (Đã chấp nhận hoặc Từ chối)." });

                _context.Applications.Remove(application);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã hủy đơn ứng tuyển thành công." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi WithdrawApplication: " + ex.Message);
                return Json(new { success = false, message = "Lỗi hệ thống, hãy liên hệ admin để giải quyết" });
            }
        }
    }

    public class ApplyOneClickRequest
    {
        public int JobId { get; set; }
        public int ResumeId { get; set; }
    }

    public class SkillStat
    {
        public string Skill { get; set; }
        public int Count { get; set; }
        public int Percentage { get; set; }
    }
}