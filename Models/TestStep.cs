using System.ComponentModel.DataAnnotations;

namespace DoAnCS.Models
{
    // Lưu các bước của kịch bản kiểm thử động phục vụ Playwright
    public class TestStep
    {
        [Key]
        public int StepID { get; set; }

        // Tên kịch bản (ví dụ: 'Auth E2E' hoặc 'Jobs E2E')
        [Required]
        [MaxLength(50)]
        public string ScenarioName { get; set; }

        // Thứ tự thực hiện của bước
        public int StepOrder { get; set; }

        // Loại hành động: Navigate, Fill, Click, Select, AssertUrl, AssertText
        [Required]
        [MaxLength(20)]
        public string ActionType { get; set; }

        // CSS Selector để thao tác với phần tử
        [MaxLength(250)]
        public string? TargetSelector { get; set; }

        // Giá trị truyền vào hoặc so khớp
        public string? Value { get; set; }

        // Mô tả hành động để hiển thị lên console/giao diện
        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
