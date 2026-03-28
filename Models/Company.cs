namespace DoAnCS.Models;

public class Company
{
    public int CompanyID { get; set; }
    public string Name { get; set; }
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public virtual ICollection<User>? Users { get; set; }
}
