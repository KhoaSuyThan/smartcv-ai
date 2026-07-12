using System;

namespace DoAnCS.Models
{
    public class Application
    {
        public int ApplicationID { get; set; }
        public int JobID { get; set; }
        public Job Job { get; set; }

        public int ResumeID { get; set; }
        public Resume Resume { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";

        // Các thông tin bổ sung sau khi duyệt
        public InterviewSchedule? InterviewSchedule { get; set; }
        public JobOffer? JobOffer { get; set; }
    }
}