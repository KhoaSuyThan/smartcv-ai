using System;
using System.Collections.Generic;

namespace DoAnCS.Models
{
    public class Job
    {
        public int JobID { get; set; }
        public int RecruiterID { get; set; }
        public User Recruiter { get; set; } // Liên kết với bảng Users
        public string Title { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public string Salary { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Application> Applications { get; set; }
    }
}