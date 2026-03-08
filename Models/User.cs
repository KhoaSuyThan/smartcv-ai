using System;
using System.Collections.Generic;

namespace DoAnCS.Models {
    public class User {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; } = "User"; // Mặc định là User
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<Resume> Resumes { get; set; }
    }
}