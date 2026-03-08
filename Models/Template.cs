namespace DoAnCS.Models
{
    public class Template
    {
        public int TemplateID { get; set; }
        public string Name { get; set; }
        public string? HtmlContent { get; set; }
        public string? CssContent { get; set; }
        public string? PreviewImageUrl { get; set; } //
        public bool IsActive { get; set; } = true;    //
    }
}