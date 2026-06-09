namespace DoAnCS.Models;

public class JobDto
{
    public string job_id { get; set; }
    public string employer_name { get; set; }
    public string employer_logo { get; set; }
    public string job_title { get; set; }
    public string job_description { get; set; }
    public string job_city { get; set; }
    public string job_employment_type { get; set; }
    public string job_apply_link { get; set; }
    public string job_salary { get; set; }
    public string job_requirements { get; set; }
    public DateTime? job_deadline { get; set; }
    public DateTime job_created_at { get; set; }
    public List<string> job_highlights { get; set; }
}
