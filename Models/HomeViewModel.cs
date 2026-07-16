using X.PagedList;
namespace DoAnCS.Models
{
    public class HomeViewModel
    {
        // Danh sách lấy từ DB
        public List<Template> PopularTemplates { get; set; } = new List<Template>();
        public List<VueTemplate> VueTemplates { get; set; } = new List<VueTemplate>();
        
        // Tạm thời để đây, chưa dùng DB
        //public List<Job> LatestJobs { get; set; } = new List<Job>();
        // Thuộc tính để lưu từ khóa tìm kiếm
        public string SearchQuery { get; set; }
        public IPagedList<JobDto> LatestJobs { get; set; }  
        public int TotalJobCount { get; set; }
        // Thuộc tính để lưu các chuyên ngành đã chọn (nếu có)
        public List<string> SelectedSpecialties { get; set; } = new List<string>();
        public List<string> AllSpecialties { get; set; } = new List<string>();
        // Danh sách lấy từ API
        public IPagedList<JobDto> RealJobs { get; set; } 
        public List<string> AllCompanies { get; set; } = new List<string>();
        public List<string> SelectedCompanies { get; set; } = new List<string>();
        public List<string> AllLocations { get; set; } = new List<string>();
        public List<string> SelectedLocations { get; set; } = new List<string>();
        public string? SelectedProvince { get; set; }
        public string? SelectedDistrict { get; set; }
        public string SortBy { get; set; }
        public List<Company> PartnerCompanies { get; set; } = new List<Company>();
    }
}