USE master;
GO

-- 1. XÓA VÀ TẠO MỚI DATABASE (Đảm bảo môi trường sạch)
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'DoAnWebCS')
BEGIN
    ALTER DATABASE DoAnWebCS SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE DoAnWebCS;
END
GO

CREATE DATABASE DoAnWebCS;
GO

USE DoAnWebCS;
GO

-- 2. BẢNG NGƯỜI DÙNG
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Phone VARCHAR(20),
    Role NVARCHAR(20) CHECK (Role IN ('Admin', 'User', 'Recruiter')) DEFAULT 'User',
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 3. BẢNG MẪU CV (Lưu trữ cấu trúc giao diện)
CREATE TABLE Templates (
    TemplateID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL,
    HtmlContent NVARCHAR(MAX), 
    CssContent NVARCHAR(MAX),  
    PreviewImageUrl NVARCHAR(500),
    IsActive BIT DEFAULT 1
);

-- 4. BẢNG CV CHÍNH (Chứa thông tin cá nhân "tĩnh" - Khớp Editor)
CREATE TABLE Resumes (
    ResumeID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    TemplateID INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,    -- Tiêu đề bản CV (ví dụ: CV .NET Intern)
    
    -- Thông tin cá nhân (Theo mẫu Quynh My)
    FullName NVARCHAR(100),         -- Tên hiển thị to nhất
    JobTitle NVARCHAR(100),         -- Vị trí ứng tuyển
    Email VARCHAR(100),             -- Email liên hệ riêng
    Phone VARCHAR(20),              -- Số điện thoại riêng
    Address NVARCHAR(500),          -- Địa chỉ cư trú
    BirthDate DATETIME,             -- Ngày tháng năm sinh
    AvatarUrl NVARCHAR(500),        -- Link ảnh chân dung
    
    Summary NVARCHAR(MAX),          -- Mục tiêu nghề nghiệp (Summary)
    ThemeColor VARCHAR(10) DEFAULT '#0d6efd',
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    FOREIGN KEY (TemplateID) REFERENCES Templates(TemplateID)
);

-- 5. BẢNG NỘI DUNG CHI TIẾT (Lưu danh sách "động" như Experience, Education dạng JSON)
CREATE TABLE ResumeSections (
    SectionID INT PRIMARY KEY IDENTITY(1,1),
    ResumeID INT NOT NULL,
    SectionType NVARCHAR(50), -- 'Experience', 'Education', 'Projects', 'Other'
    ContentJSON NVARCHAR(MAX), -- Lưu mảng đối tượng JSON để linh hoạt cao
    SortOrder INT DEFAULT 0,
    FOREIGN KEY (ResumeID) REFERENCES Resumes(ResumeID) ON DELETE CASCADE
);

-- 6. HỆ THỐNG KỸ NĂNG VÀ VIỆC LÀM (Phục vụ Matching AI)
CREATE TABLE Skills (
    SkillID INT PRIMARY KEY IDENTITY(1,1),
    SkillName NVARCHAR(100) UNIQUE NOT NULL
);

CREATE TABLE ResumeSkills (
    ResumeID INT NOT NULL,
    SkillID INT NOT NULL,
    Proficiency NVARCHAR(50), -- Beginner, Intermediate, Advanced
    PRIMARY KEY (ResumeID, SkillID),
    FOREIGN KEY (ResumeID) REFERENCES Resumes(ResumeID) ON DELETE CASCADE,
    FOREIGN KEY (SkillID) REFERENCES Skills(SkillID) ON DELETE CASCADE
);

CREATE TABLE Jobs (
    JobID INT PRIMARY KEY IDENTITY(1,1),
    RecruiterID INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Requirements NVARCHAR(MAX), 
    Salary NVARCHAR(100),
    Deadline DATETIME,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RecruiterID) REFERENCES Users(UserID)
);

CREATE TABLE Applications (
    ApplicationID INT PRIMARY KEY IDENTITY(1,1),
    JobID INT NOT NULL,
    ResumeID INT NOT NULL,
    AppliedAt DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT 'Pending', -- Pending, Reviewing, Accepted, Rejected
    FOREIGN KEY (JobID) REFERENCES Jobs(JobID),
    FOREIGN KEY (ResumeID) REFERENCES Resumes(ResumeID) ON DELETE NO ACTION -- Tránh vòng lặp Cascade
);

-- 7. LOG HỆ THỐNG AI
CREATE TABLE AILogs (
    LogID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    RequestType NVARCHAR(50), -- 'OptimizeSummary', 'SuggestSkills'
    InputText NVARCHAR(MAX),
    OutputText NVARCHAR(MAX),
    UsedTokens INT,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);
GO

-- Chèn mẫu CV Modern Blue với đầy đủ tính năng
INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive)
VALUES (
    N'Modern Blue Sidebar', 
    -- 1. HtmlContent: Khung xương đầy đủ 10 Token
    N'<div class="cv-container">
        <div class="cv-sidebar">
            <div class="profile-header">
                <img src="{{AvatarUrl}}" class="profile-pic">
            </div>
            <ul class="contact-info">
                <li><i class="fas fa-phone"></i> {{Phone}}</li>
                <li><i class="fas fa-envelope"></i> {{Email}}</li>
                <li><i class="fas fa-calendar-alt"></i> {{BirthDate}}</li>
                <li><i class="fas fa-map-marker-alt"></i> {{Address}}</li>
            </ul>
            
            <div class="sidebar-section">
                <h3>HỌC VẤN</h3>
                {{Education}}
            </div>

            <div class="sidebar-section">
                <h3>TIN HỌC</h3>
                {{Skills}}
            </div>

            <div class="sidebar-section">
                <h3>NGOẠI NGỮ</h3>
                {{Languages}}
            </div>

            <div class="sidebar-section">
                <h3>KỸ NĂNG KHÁC</h3>
                {{OtherSkills}}
            </div>
        </div>

        <div class="cv-main">
            <div class="main-header">
                <h1>{{FullName}}</h1>
                <h2>{{JobTitle}}</h2>
            </div>
            <div class="main-section">
                <h3>MỤC TIÊU NGHỀ NGHIỆP</h3>
                <p>{{Summary}}</p>
            </div>
            <div class="main-section">
                <h3>KINH NGHIỆM LÀM VIỆC</h3>
                {{Experience}}
            </div>
        </div>
    </div>',

    -- 2. CssContent: Tối ưu khoảng cách cho các mục sidebar
    N'.cv-container { display: flex; background: white; min-height: 297mm; font-family: "Segoe UI", Tahoma, Geneva, Verdana, sans-serif; } 
    .cv-sidebar { flex: 3; background: #f7f9fc; padding: 25px; border-right: 1px solid #eee; } 
    .profile-pic { width: 150px; height: 150px; border-radius: 50%; object-fit: cover; border: 4px solid white; box-shadow: 0 4px 10px rgba(0,0,0,0.1); margin-bottom: 20px; display: block; margin-left: auto; margin-right: auto; }
    .cv-main { flex: 7; padding: 40px; }
    .sidebar-section h3, .main-section h3 { font-size: 15px; border-bottom: 1px solid #0d6efd; color: #0d6efd; padding-bottom: 5px; margin-top: 22px; text-transform: uppercase; font-weight: bold; }
    .contact-info { list-style: none; padding: 0; font-size: 13px; margin-bottom: 20px; }
    .contact-info li { margin-bottom: 12px; display: flex; align-items: center; }
    .contact-info i { width: 22px; color: #0d6efd; margin-right: 10px; text-align: center; }
    .main-header h1 { margin: 0; font-size: 32px; text-transform: uppercase; color: #333; }
    .main-header h2 { margin: 5px 0 20px 0; font-size: 18px; color: #666; font-weight: normal; }
    .sidebar-section p, .sidebar-section ul { font-size: 13px; line-height: 1.6; color: #444; margin-top: 10px; }
    .main-section p { font-size: 14px; line-height: 1.6; color: #333; }',

    -- 3. Ảnh xem trước
    '/images/templates/modern-blue.jpg',

    -- 4. Trạng thái hoạt động
    1
);
GO

-- Nạp Kỹ năng IT
INSERT INTO Skills (SkillName) VALUES ('.NET'), ('SQL Server'), ('C#'), ('Flutter'), ('React');
GO
