namespace DoAnCS.Models.ViewModels;

public class AdminDashboardVM
{
    public int TotalUsers { get; set; }
    public int TotalCompanies { get; set; }
    public int TotalJobs { get; set; }
    public int TotalResumes { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public List<Job> Jobs { get; set; } = new List<Job>();
    public List<Template> Templates { get; set; } = new List<Template>();
}
