using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCS.Data;
using DoAnCS.Models;
using DoAnCS.Services;
using X.PagedList;
using X.PagedList.Extensions;

namespace DoAnCS.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly JobApiService _jobApiService;
        private readonly IWebHostEnvironment _env;

        public HomeController(AppDbContext context, JobApiService jobApiService, IWebHostEnvironment env)
        {
            _context = context;
            _jobApiService = jobApiService;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Lấy Job từ Database
            var jobsFromDb = await _context.Jobs
                .Include(j => j.Company)
                .Where(j => j.Status == 1)
                .OrderByDescending(j => j.CreatedAt)
                .Take(6)
                .ToListAsync();

            // 2. Mapping sang JobDto
            var mappedJobs = jobsFromDb.Select(j => new JobDto
            {
                job_id = j.JobID.ToString(),
                job_title = j.Title,
                employer_name = j.Company?.Name,
                employer_logo = j.Company?.LogoUrl,
                job_city = !string.IsNullOrEmpty(j.Location) ? j.Location : (j.Company != null ? j.Company.Address : "Toàn quốc"),
                job_description = j.Description,
                job_apply_link = j.Company?.Website ?? "#"
            }).ToList();

            // 3. Nạp vào đúng thuộc tính LatestJobs
            var viewModel = new HomeViewModel
            {
                LatestJobs = mappedJobs.ToPagedList(1, 6), // Đổ vào đây nè Khoa!
                PopularTemplates = _env.IsDevelopment() ? await _context.Templates
                    .Where(t => t.IsActive == true)
                    .Take(4)
                    .ToListAsync() : new List<Template>(),
                VueTemplates = await _context.VueTemplates
                    .Where(t => t.IsActive == true)
                    .ToListAsync(),
                PartnerCompanies = await _context.Companies
                    .Where(c => !string.IsNullOrEmpty(c.LogoUrl))
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(15)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // SỬA: Thêm tham số int? page
        public async Task<IActionResult> Jobs(int? page, string searchQuery, List<string> specialties, List<string> selectedCompanies, string selectedProvince, string selectedDistrict, string sortBy, int? selectedResumeId)
        {
            // 1. Khởi tạo Query lấy từ Database
            IQueryable<Job> query = _context.Jobs.Include(j => j.Company)
            .Where(j => j.Status == 1);

            // 2. Bộ lọc tìm kiếm theo từ khóa (Tiêu đề hoặc Mô tả)
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(j => j.Title.Contains(searchQuery) || j.Description.Contains(searchQuery));
            }

            // 3. Bộ lọc theo Chuyên môn (Checkboxes)
            if (specialties != null && specialties.Any())
            {
                // Lọc những Job mà Tiêu đề hoặc Mô tả có chứa các từ khóa chuyên môn
                query = query.Where(j => specialties.Any(s => j.Title.Contains(s) || j.Description.Contains(s)));
            }
            // 4. Bộ lọc theo Công ty (Checkboxes)
            if (selectedCompanies != null && selectedCompanies.Any())
            {
                query = query.Where(j => selectedCompanies.Contains(j.Company.Name));
            }

            // 4.5. Bộ lọc theo Địa điểm (Tỉnh/Thành phố và Quận/Huyện)
            if (!string.IsNullOrEmpty(selectedProvince))
            {
                if (!string.IsNullOrEmpty(selectedDistrict))
                {
                    query = query.Where(j => 
                        (!string.IsNullOrEmpty(j.Location) && j.Location.Contains(selectedDistrict) && j.Location.Contains(selectedProvince)) ||
                        (string.IsNullOrEmpty(j.Location) && j.Company != null && !string.IsNullOrEmpty(j.Company.Address) && j.Company.Address.Contains(selectedDistrict) && j.Company.Address.Contains(selectedProvince))
                    );
                }
                else
                {
                    query = query.Where(j => 
                        (!string.IsNullOrEmpty(j.Location) && j.Location.Contains(selectedProvince)) ||
                        (string.IsNullOrEmpty(j.Location) && j.Company != null && !string.IsNullOrEmpty(j.Company.Address) && j.Company.Address.Contains(selectedProvince))
                    );
                }
            }

            // 4. Sắp xếp
            if (sortBy == "salary")
            {
                // Sắp xếp theo lương (chuỗi): Thử mẹo sắp xếp theo độ dài trước để số lớn hơn đứng đầu
                query = query.OrderByDescending(j => j.Salary.Length).ThenByDescending(j => j.Salary);
            }
            else
            {
                query = query.OrderByDescending(j => j.CreatedAt);
            }

            // === MATCHING LOGIC: So khớp kỹ năng CV với yêu cầu công việc ===
            var matchScores = new Dictionary<string, int>(); // job_id (string) -> MatchScore %
            var userIdClaim = User.FindFirst("UserID")?.Value;
            List<Resume> userResumes = null;
            Resume selectedResume = null;
            List<string> userSkills = null;

            if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
            {
                // Lấy danh sách CV của user để cho user chọn
                userResumes = await _context.Resumes
                    .Where(r => r.UserID == userId)
                    .OrderByDescending(r => r.UpdatedAt)
                    .ToListAsync();

                // Xác định CV được chọn (mặc định = CV mới nhất)
                if (selectedResumeId.HasValue)
                {
                    selectedResume = userResumes.FirstOrDefault(r => r.ResumeID == selectedResumeId.Value);
                }
                // Ưu tiên lấy CV mà người dùng đang bật "Công khai" (IsPublic) trên Profile
                selectedResume ??= userResumes.FirstOrDefault(r => r.IsPublic) ?? userResumes.FirstOrDefault();

                if (selectedResume != null)
                {
                    // Trích xuất kỹ năng từ CV
                    userSkills = ExtractSkillsFromResume(selectedResume);
                    ViewBag.SelectedResumeId = selectedResume.ResumeID;
                }
            }

            // Cấu hình phân trang
            int pageSize = 10; // Mỗi trang hiện 10 tin
            int pageNumber = page ?? 1;
            int totalItemCount = 0;
            List<Job> jobsRaw = null;

            // Xác định xem có cần sắp xếp theo độ tương thích (Match Score) hay không
            // Điều kiện: người dùng đăng nhập, có CV, và không yêu cầu sắp xếp theo lương (sortBy == "salary")
            bool sortByMatchScore = (selectedResume != null && userSkills != null && userSkills.Any() && sortBy != "salary");

            if (sortByMatchScore)
            {
                // Tải tất cả các job đang tuyển dụng để tính match score trước (phân trang trên bộ nhớ để đảm bảo tính đúng đắn khi sắp xếp theo Score)
                var allActiveJobs = await query.ToListAsync();

                // Tính match score cho tất cả jobs
                foreach (var job in allActiveJobs)
                {
                    int score = CalculateMatchScore(userSkills, job);
                    matchScores[job.JobID.ToString()] = score;
                }

                // Sắp xếp danh sách Jobs theo match score trước, sau đó theo thời gian tạo
                var sortedJobs = allActiveJobs
                    .OrderByDescending(j => matchScores.ContainsKey(j.JobID.ToString()) ? matchScores[j.JobID.ToString()] : 0)
                    .ThenByDescending(j => j.CreatedAt)
                    .ToList();

                totalItemCount = sortedJobs.Count;

                // Phân trang trên bộ nhớ
                jobsRaw = sortedJobs.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                // Phân trang thực sự trên Database
                totalItemCount = await query.CountAsync();
                jobsRaw = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

                // Nếu người dùng có CV, tính match score chỉ cho 10 tin hiển thị hiện tại để tối ưu hiệu năng
                if (selectedResume != null && userSkills != null && userSkills.Any())
                {
                    foreach (var job in jobsRaw)
                    {
                        int score = CalculateMatchScore(userSkills, job);
                        matchScores[job.JobID.ToString()] = score;
                    }
                }
            }

            ViewBag.MatchScores = matchScores;
            ViewBag.UserResumes = userResumes;

            // 6. Mapping sang JobDto (chỉ map tối đa 10 tin hiển thị trên trang hiện tại)
            var jobDtos = jobsRaw.Select(j => new JobDto
            {
                job_id = j.JobID.ToString(),
                job_title = j.Title,
                employer_name = j.Company != null ? j.Company.Name : "N/A",
                employer_logo = j.Company != null ? j.Company.LogoUrl : null,
                job_salary = j.Salary,
                job_city = !string.IsNullOrEmpty(j.Location) ? j.Location : (j.Company != null ? j.Company.Address : "Toàn quốc"),
                job_description = j.Description,
                job_apply_link = j.Company != null ? j.Company.Website : "#"
            }).ToList();

            var pagedList = new StaticPagedList<JobDto>(jobDtos, pageNumber, pageSize, totalItemCount);

            // 7. Tạo danh sách chuyên môn (allSpecs) để hiển thị trong bộ lọc
            var allSpecs = new List<string>{".NET Engineer",
                "Accounting Intern",
                "Administrative Intern",
                "Agriculture Technician / Farm Intern",
                "AI Developer",
                "AI Prompt Engineering",
                "AI Team Lead",
                "Android Developer",
                "Backend Developer",
                "BI Database Developer Intern",
                "Bridge System Engineer",
                "Business Analyst",
                "Business Development Specialist",
                "Business Intern",
                "Business Support Intern",
                "Category Manager",
                "Cloud Engineer",
                "Cobol Developer",
                "Communication / Marketing Intern",
                "Cybersecurity Engineer",
                "Data Analyst",
                "Data Engineer",
                "Database Administrator",
                "Deputy Head of Customer Applications",
                "Deputy Head of Internal Applications",
                "Developer Intern",
                "DevOps Engineer",
                "Digital Marketing Specialist",
                "ERP Consultant",
                "Flutter Developer Intern",
                "Fresher Developer",
                "Frontend Developer",
                "Fullstack Developer",
                "Graphic Designer",
                "Head of Application Development",
                "Head of Data & AI",
                "Head of Information Security",
                "Head of Infrastructure & Platform",
                "Head of IT Governance & Compliance / PMO",
                "HR Intern (Recruitment)",
                "HR Specialist / HRBP",
                "Implementation Consultant",
                "Import-Export / Logistics Intern",
                "IoT Engineer",
                "IT & Product Designer",
                "IT Governance Specialist",
                "IT Helpdesk Specialist",
                "IT Operations Specialist",
                "IT Service Quality",
                "Java Developer",
                "JS Engineer",
                "Lead Cybersecurity Engineer",
                "Lead Data Engineer",
                "Mobile Developer",
                "Network Engineer",
                "Operations Executive",
                "Platform Engineer",
                "PMO Specialist",
                "Production / Manufacturing Staff",
                "QA/QC Automation Engineer",
                "QC/Tester",
                "R&D Specialist (Product/Food/Bio)",
                "Senior IT Operations Engineer",
                "Senior IT System Engineer",
                "Senior Platform Engineer",
                "Service Desk Consultant",
                "Social Media",
                "STEM Instructor / Teacher",
                "System Operations Specialist",
                "Technical Architect",
                "UI/UX Designer"};

            var companyNames = await _context.Companies
                .Select(c => c.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                RealJobs = pagedList,
                SearchQuery = searchQuery,
                SelectedSpecialties = specialties ?? new List<string>(), // Lưu lại các checkbox đã chọn
                SelectedCompanies = selectedCompanies ?? new List<string>(),
                AllSpecialties = allSpecs.OrderBy(s => s).ToList(),
                AllCompanies = await _context.Companies.Select(c => c.Name).Distinct().ToListAsync(),
                AllLocations = new List<string>(),
                SelectedLocations = new List<string>(),
                SelectedProvince = selectedProvince,
                SelectedDistrict = selectedDistrict,
                SortBy = sortBy ?? "latest"
            };

            Console.WriteLine($"[DEBUG] User: {User.Identity?.Name}, IsAuthenticated: {User.Identity?.IsAuthenticated}, IsInRole('User'): {User.IsInRole("User")}, ResumesCount: {userResumes?.Count ?? 0}");
            return View(viewModel);
        }

        // Đổi tham số từ string sang int vì JobID trong DB của Khoa là kiểu int
        public async Task<IActionResult> Details(int id)
        {
            // 1. Tìm Job trong Database kèm theo thông tin Công ty (Include)
            var jobDb = await _context.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(m => m.JobID == id);

            // 2. Nếu không tìm thấy trong DB thì báo lỗi
            if (jobDb == null) return NotFound();

            var jobDto = new JobDto
            {
                job_id = jobDb.JobID.ToString(), // Chắc chắn map JobID để gửi đơn ứng tuyển
                job_title = jobDb.Title,
                employer_name = jobDb.Company?.Name,
                employer_logo = jobDb.Company?.LogoUrl,
                job_city = !string.IsNullOrEmpty(jobDb.Location) ? jobDb.Location : (jobDb.Company != null ? jobDb.Company.Address : "Toàn quốc"),
                job_description = jobDb.Description,
                job_requirements = jobDb.Requirements,
                job_salary = jobDb.Salary,
                job_deadline = jobDb.Deadline,
                job_created_at = jobDb.CreatedAt,
                job_apply_link = jobDb.Company?.Website ?? "#"
            };

            // Lấy danh sách CV của người dùng (nếu đã đăng nhập)
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst("UserID")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    ViewBag.UserResumes = await _context.Resumes
                        .Where(r => r.UserID == userId)
                        .OrderByDescending(r => r.UpdatedAt)
                        .ToListAsync();
                }
            }

            // 4. Lưu CompanyID vào ViewBag để liên kết sang trang chi tiết công ty
            ViewBag.CompanyID = jobDb.CompanyID;

            // 5. Trả về View với Model là đối tượng JobDto
            return View(jobDto);
        }

        // ==========================================
        // FEEDBACK / RATING
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> SubmitFeedback(double rating, string comment)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để đánh giá." });
            }

            var userIdClaim = User.FindFirst("UserID")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Json(new { success = false, message = "Lỗi xác thực người dùng." });
            }

            if (rating < 0.5f || rating > 5f)
            {
                return Json(new { success = false, message = "Điểm đánh giá không hợp lệ." });
            }

            try
            {
                var existingFeedback = await _context.SiteFeedbacks.FirstOrDefaultAsync(f => f.UserID == userId);

                if (existingFeedback != null)
                {
                    existingFeedback.Rating = rating;
                    existingFeedback.Comment = comment;
                    existingFeedback.UpdatedAt = DateTime.Now;
                    _context.SiteFeedbacks.Update(existingFeedback);
                }
                else
                {
                    var feedback = new SiteFeedback
                    {
                        UserID = userId,
                        Rating = rating,
                        Comment = comment,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    await _context.SiteFeedbacks.AddAsync(feedback);
                }

                await _context.SaveChangesAsync();
                
                // Tính điểm trung bình mới
                var newAverage = await _context.SiteFeedbacks.AverageAsync(f => f.Rating);

                return Json(new { success = true, message = "Cảm ơn bạn đã gửi đánh giá!", newAverage = Math.Round(newAverage, 1) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMyFeedback()
        {
            if (!User.Identity.IsAuthenticated) return Json(new { success = false });

            var userIdClaim = User.FindFirst("UserID")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId)) return Json(new { success = false });

            var feedback = await _context.SiteFeedbacks.FirstOrDefaultAsync(f => f.UserID == userId);
            if (feedback != null)
            {
                return Json(new { success = true, rating = feedback.Rating, comment = feedback.Comment });
            }
            return Json(new { success = false });
        }

        // ==========================================
        // PRIVATE: Logic so khớp kỹ năng CV & Job
        // ==========================================

        /// <summary>
        /// Trích xuất danh sách từ khóa/kỹ năng từ Resume (JsonContent + Title + JobTitle)
        /// </summary>
        private List<string> ExtractSkillsFromResume(Resume resume)
        {
            var skills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Lấy từ JsonContent (nếu có - đây là CV tạo bằng Vue Builder)
            if (!string.IsNullOrEmpty(resume.JsonContent))
            {
                try
                {
                    var json = System.Text.Json.JsonDocument.Parse(resume.JsonContent);
                    var root = json.RootElement;

                    // Lấy JobTitle
                    if (root.TryGetProperty("general", out var general))
                    {
                        if (general.TryGetProperty("jobTitle", out var jt))
                        {
                            var jobTitle = StripHtml(jt.GetString());
                            if (!string.IsNullOrEmpty(jobTitle)) skills.Add(jobTitle.Trim());
                        }
                    }

                    // Lấy kỹ năng từ sections
                    if (root.TryGetProperty("sections", out var sections))
                    {
                        foreach (var section in sections.EnumerateArray())
                        {
                            var id = section.TryGetProperty("id", out var sId) ? sId.GetString() : "";
                            var isVisible = section.TryGetProperty("isVisible", out var vis) && vis.GetBoolean();
                            
                            if (!isVisible) continue;

                            if ((id == "skills" || id == "it_skills" || id == "languages") 
                                && section.TryGetProperty("items", out var items))
                            {
                                foreach (var item in items.EnumerateArray())
                                {
                                    if (item.TryGetProperty("name", out var name))
                                    {
                                        var skillName = StripHtml(name.GetString())?.Trim();
                                        if (!string.IsNullOrEmpty(skillName)) skills.Add(skillName);
                                    }
                                }
                            }

                            // Trích xuất từ mô tả kinh nghiệm/dự án
                            if ((id == "experience" || id == "project") && section.TryGetProperty("items", out var expItems))
                            {
                                foreach (var item in expItems.EnumerateArray())
                                {
                                    if (item.TryGetProperty("desc", out var desc))
                                    {
                                        var text = StripHtml(desc.GetString());
                                        if (!string.IsNullOrEmpty(text))
                                        {
                                            // Trích xuất các từ khóa kỹ thuật phổ biến
                                            ExtractTechKeywords(text, skills);
                                        }
                                    }
                                    if (item.TryGetProperty("role", out var role))
                                    {
                                        var roleText = StripHtml(role.GetString())?.Trim();
                                        if (!string.IsNullOrEmpty(roleText)) skills.Add(roleText);
                                    }
                                }
                            }
                        }
                    }
                }
                catch { /* Bỏ qua lỗi parse JSON */ }
            }

            // 2. Fallback: Lấy từ Title và JobTitle của Resume
            if (!string.IsNullOrEmpty(resume.Title)) skills.Add(resume.Title.Trim());
            if (!string.IsNullOrEmpty(resume.JobTitle)) skills.Add(resume.JobTitle.Trim());

            return skills.ToList();
        }

        /// <summary>
        /// Tính điểm match score (0-100) dựa trên keyword matching
        /// </summary>
        private int CalculateMatchScore(List<string> userSkills, Job job)
        {
            if (userSkills == null || !userSkills.Any()) return 0;

            var jobText = $"{job.Title} {job.Description} {job.Requirements}".ToLower();

            int matched = 0;
            // Chỉ lấy các kỹ năng có ý nghĩa (độ dài >= 2)
            var validUserSkills = userSkills.Where(s => !string.IsNullOrWhiteSpace(s) && s.Trim().Length >= 2).ToList();
            int total = validUserSkills.Count;

            if (total == 0) return 0;

            foreach (var skill in validUserSkills)
            {
                var skillLower = skill.ToLower().Trim();
                
                // Tránh lỗi khi người dùng nhập chuỗi vô nghĩa như "aaaaaaaaa"
                if (skillLower.Length > 20 && !skillLower.Contains(" ")) continue; 

                // Chỉ tính là khớp nếu toàn bộ cụm từ xuất hiện trong JD (VD: "vue.js", "frontend")
                if (jobText.Contains(skillLower))
                {
                    matched++;
                }
            }

            // Tính thêm: Có bao nhiêu keywords trong Job mà CV có
            var jobKeywords = ExtractJobKeywords(job);
            int jobMatched = 0;
            foreach (var keyword in jobKeywords)
            {
                var keywordLower = keyword.ToLower();
                // Phải khớp toàn bộ từ khóa công nghệ, không chơi chứa một phần (Contains) để tránh "a" khớp với "Java"
                if (validUserSkills.Any(s => s.ToLower().Trim() == keywordLower || s.ToLower().Contains(keywordLower)))
                {
                    jobMatched++;
                }
            }

            // Điểm = trung bình giữa (% kỹ năng user khớp) và (% yêu cầu job khớp)
            double userRate = total > 0 ? (double)matched / total : 0;
            double jobRate = jobKeywords.Count > 0 ? (double)jobMatched / jobKeywords.Count : 0;

            int score = (int)Math.Round((userRate * 40 + jobRate * 60) * 100); 
            return Math.Min(score, 100);
        }

        /// <summary>
        /// Trích xuất từ khóa yêu cầu từ Job
        /// </summary>
        private List<string> ExtractJobKeywords(Job job)
        {
            var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var text = $"{job.Title} {job.Requirements} {job.Description}";
            ExtractTechKeywords(text, keywords);
            return keywords.ToList();
        }

        /// <summary>
        /// Trích xuất các từ khóa kỹ thuật phổ biến từ text
        /// </summary>
        private void ExtractTechKeywords(string text, HashSet<string> keywords)
        {
            if (string.IsNullOrEmpty(text)) return;

            // Danh sách các từ khóa kỹ thuật phổ biến trong IT
            var techTerms = new[] {
                "C#", ".NET", "ASP.NET", "ASP.NET Core", "Entity Framework", "LINQ",
                "Java", "Spring Boot", "Spring", "Hibernate",
                "Python", "Django", "Flask", "FastAPI", "TensorFlow", "PyTorch",
                "JavaScript", "TypeScript", "React", "Angular", "Vue", "Vue.js", "Node.js", "Express",
                "Next.js", "Nuxt.js", "jQuery", "Bootstrap", "Tailwind",
                "HTML", "CSS", "SASS", "SCSS",
                "PHP", "Laravel", "WordPress",
                "Ruby", "Rails", "Go", "Golang", "Rust", "Kotlin", "Swift",
                "Flutter", "React Native", "Xamarin", "MAUI",
                "SQL", "SQL Server", "MySQL", "PostgreSQL", "MongoDB", "Redis", "Firebase",
                "Oracle", "SQLite", "NoSQL", "Elasticsearch",
                "Docker", "Kubernetes", "K8s", "AWS", "Azure", "GCP", "Google Cloud",
                "CI/CD", "Jenkins", "GitHub Actions", "GitLab CI", "Terraform",
                "Git", "GitHub", "GitLab", "Bitbucket", "SVN",
                "REST", "RESTful", "GraphQL", "gRPC", "WebSocket", "API",
                "Microservices", "Monolith", "MVC", "MVVM", "Clean Architecture",
                "Agile", "Scrum", "Kanban", "Jira", "Trello",
                "Linux", "Ubuntu", "Windows Server", "Nginx", "Apache",
                "AI", "Machine Learning", "Deep Learning", "NLP", "Computer Vision", "ChatGPT", "LLM",
                "Data Science", "Big Data", "Hadoop", "Spark",
                "IoT", "Embedded", "Arduino", "Raspberry Pi",
                "Figma", "Adobe XD", "Sketch", "Photoshop", "Illustrator",
                "UI/UX", "UX", "UI", "Responsive Design",
                "DevOps", "SRE", "Cloud", "Serverless", "Lambda",
                "Cybersecurity", "Penetration Testing", "OWASP",
                "Blockchain", "Web3", "Solidity", "Smart Contract",
                "Power BI", "Tableau", "Excel", "VBA",
                "SAP", "ERP", "CRM", "Salesforce",
                "RabbitMQ", "Kafka", "SignalR", "MQTT",
                "Selenium", "Cypress", "Jest", "xUnit", "NUnit", "JUnit",
                "Postman", "Swagger", "OpenAPI",
                "OOP", "SOLID", "Design Patterns", "TDD", "BDD", "DDD",
                "English", "Communication", "Leadership", "Teamwork", "Problem Solving"
            };

            var textLower = text.ToLower();
            foreach (var term in techTerms)
            {
                if (textLower.Contains(term.ToLower()))
                {
                    keywords.Add(term);
                }
            }
        }

        /// <summary>
        /// Xóa thẻ HTML khỏi chuỗi
        /// </summary>
        private string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return System.Text.RegularExpressions.Regex.Replace(input, "<[^>]*>", "").Replace("&nbsp;", " ").Trim();
        }
    }
}