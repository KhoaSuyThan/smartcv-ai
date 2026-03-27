using X.PagedList;
namespace DoAnCS.Models
{
    public class HomeViewModel
    {
        // Danh sách lấy từ DB
        public List<Template> PopularTemplates { get; set; } = new List<Template>();
        
        // Tạm thời để đây, chưa dùng DB
        //public List<Job> LatestJobs { get; set; } = new List<Job>();
        // Thuộc tính để lưu từ khóa tìm kiếm
        public string SearchQuery { get; set; }
        // Thuộc tính để lưu các chuyên ngành đã chọn (nếu có)
        public List<string> SelectedSpecialties { get; set; } = new List<string>();
        // Danh sách lấy từ API
        public IPagedList<JobDto> RealJobs { get; set; }
    }
}