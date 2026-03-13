namespace DoAnCS.Models
{
    public class HomeViewModel
    {
        // Danh sách lấy từ DB
        public List<Template> PopularTemplates { get; set; } = new List<Template>();
        
        // Tạm thời để đây, chưa dùng DB
        public List<Job> LatestJobs { get; set; } = new List<Job>();
    }
}