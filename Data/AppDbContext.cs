using Microsoft.EntityFrameworkCore;
using DoAnCS.Models;

namespace DoAnCS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UpgradeRequest> UpgradeRequests { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Resume> Resumes { get; set; }
        public DbSet<ResumeSection> ResumeSections { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<ResumeSkill> ResumeSkills { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<AILog> AILogs { get; set; }
        public DbSet<ResumeExport> ResumeExports { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<GeminiConfig> GeminiConfigs { get; set; }
        public DbSet<VueTemplate> VueTemplates { get; set; }
        public DbSet<SavedCandidate> SavedCandidates { get; set; }
        public DbSet<CVMatchResult> CVMatchResults { get; set; }
        public DbSet<CVEmbedding> CVEmbeddings { get; set; }
        public DbSet<SiteFeedback> SiteFeedbacks { get; set; }
        public DbSet<TestRun> TestRuns { get; set; }
        public DbSet<TestCaseDetail> TestCaseDetails { get; set; }
        public DbSet<TestStep> TestSteps { get; set; }
        public DbSet<InterviewSession> InterviewSessions { get; set; }
        public DbSet<InterviewMessage> InterviewMessages { get; set; }
        public DbSet<RecruiterInterviewPrep> RecruiterInterviewPreps { get; set; }
        public DbSet<Notification> Notifications { get; set; } // Bảng thông báo hệ thống

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình quan hệ 1-N giữa TestRun và TestCaseDetail
            modelBuilder.Entity<TestCaseDetail>()
                .HasOne(td => td.TestRun)
                .WithMany(tr => tr.Details)
                .HasForeignKey(td => td.TestRunID)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình khóa chính hỗn hợp cho ResumeSkill
            modelBuilder.Entity<ResumeSkill>()
                .HasKey(rs => new { rs.ResumeID, rs.SkillID });

            // Ràng buộc 1-N giữa User và Resume
            modelBuilder.Entity<Resume>()
                .HasOne(r => r.User)
                .WithMany(u => u.Resumes)
                .HasForeignKey(r => r.UserID);

            // Gỡ ràng buộc bắt buộc cho TemplateID để hỗ trợ VueTemplate
            modelBuilder.Entity<Resume>()
                .HasOne(r => r.Template)
                .WithMany()
                .HasForeignKey(r => r.TemplateID)
                .IsRequired(false);

            // Cấu hình quan hệ không cascade delete để tránh Multiple Cascade Paths lỗi SQL Server
            modelBuilder.Entity<InterviewSession>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InterviewSession>()
                .HasOne(s => s.Job)
                .WithMany()
                .HasForeignKey(s => s.JobID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InterviewSession>()
                .HasOne(s => s.Resume)
                .WithMany()
                .HasForeignKey(s => s.ResumeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InterviewSession>()
                .HasOne(s => s.Recruiter)
                .WithMany()
                .HasForeignKey(s => s.AssignedByRecruiterID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InterviewMessage>()
                .HasOne(m => m.Session)
                .WithMany(s => s.Messages)
                .HasForeignKey(m => m.SessionID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecruiterInterviewPrep>()
                .HasOne(p => p.Recruiter)
                .WithMany()
                .HasForeignKey(p => p.RecruiterID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RecruiterInterviewPrep>()
                .HasOne(p => p.Resume)
                .WithMany()
                .HasForeignKey(p => p.ResumeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RecruiterInterviewPrep>()
                .HasOne(p => p.Job)
                .WithMany()
                .HasForeignKey(p => p.JobID)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình quan hệ Notification -> User (cascade delete khi xóa User)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed mặc định cho bảng cấu hình Gemini
            modelBuilder.Entity<GeminiConfig>().HasData(new GeminiConfig
            {
                Id = 1,
                ApiKey = "",
                ModelName = "gemini-2.5-flash",
                Temperature = 0.7,
                MaxOutputTokens = 2048,
                SystemInstruction = "Bạn là trợ lý ảo hỗ trợ đánh giá CV.",
                UserRateLimit = 10,
                TotalTokensUsed = 0
            });
        }
    }
}