using DoAnCS.Controllers;
namespace DoAnCS.Models.ViewModels;

public class AdminDashboardVM
{
    public int TotalUsers { get; set; }
    public int TotalCompanies { get; set; }
    public int TotalJobs { get; set; }
    public int TotalResumes { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<decimal> MonthlyRevenue { get; set; } = new List<decimal>(new decimal[12]);
    public List<decimal> CurrentMonthRevenue { get; set; } = new List<decimal>();
    public List<decimal> WeeklyRevenue { get; set; } = new List<decimal>(new decimal[7]);
    public List<decimal> YearlyRevenue { get; set; } = new List<decimal>();
    public List<int> YearlyLabels { get; set; } = new List<int>();
    
    // Job Statistics
    public int TotalApplications { get; set; }
    public int PendingApps { get; set; }
    public int ReviewingApps { get; set; }
    public int AcceptedApps { get; set; }
    public int RejectedApps { get; set; }
    public List<string> TimelineLabels { get; set; } = new List<string>();
    public List<int> TimelineValues { get; set; } = new List<int>();
    public List<string> TimelineMonthLabels { get; set; } = new List<string>();
    public List<int> TimelineMonthValues { get; set; } = new List<int>();
    public List<SkillStat> TopSkills { get; set; } = new List<SkillStat>();

    public List<UpgradeRequest> RecentUpgrades { get; set; } = new List<UpgradeRequest>();
    public List<Job> Jobs { get; set; } = new List<Job>();
    public List<Template> Templates { get; set; } = new List<Template>();
    public List<VueTemplate> VueTemplates { get; set; } = new List<VueTemplate>();
    public List<Company> Companies { get; set; } = new List<Company>();
}
