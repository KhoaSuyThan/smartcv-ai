using Microsoft.EntityFrameworkCore;
using DoAnCS.Models;

namespace DoAnCS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình khóa chính hỗn hợp cho ResumeSkill
            modelBuilder.Entity<ResumeSkill>()
                .HasKey(rs => new { rs.ResumeID, rs.SkillID });

            // Ràng buộc 1-N giữa User và Resume
            modelBuilder.Entity<Resume>()
                .HasOne(r => r.User)
                .WithMany(u => u.Resumes)
                .HasForeignKey(r => r.UserID);
        }
    }
}