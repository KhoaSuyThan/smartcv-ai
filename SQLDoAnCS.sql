USE master;
GO
-- Tạo Database nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DoAnWebCS')
BEGIN
    CREATE DATABASE DoAnWebCS;
END
GO

USE DoAnWebCS;
GO

-- 1. Bảng Người dùng
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Phone VARCHAR(20),
    Role NVARCHAR(20) CHECK (Role IN ('Admin', 'User', 'Recruiter')),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 2. Bảng Mẫu CV (Template)
CREATE TABLE Templates (
    TemplateID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL,
    HtmlContent NVARCHAR(MAX), -- Lưu cấu trúc HTML mẫu
    CssContent NVARCHAR(MAX),  -- Lưu style mẫu
    PreviewImageUrl NVARCHAR(500),
    IsActive BIT DEFAULT 1
);

-- 3. Bảng CV chính
CREATE TABLE Resumes (
    ResumeID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT FOREIGN KEY REFERENCES Users(UserID),
    TemplateID INT FOREIGN KEY REFERENCES Templates(TemplateID),
    Title NVARCHAR(200) NOT NULL, -- Ví dụ: CV Thực tập Backend
    Summary NVARCHAR(MAX),        -- Professional Summary do AI tạo
    ThemeColor VARCHAR(10),       -- Mã màu Hex (ví dụ: #3498db)
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);

-- 4. Bảng Nội dung chi tiết CV (Học vấn, Kinh nghiệm, Dự án...)
CREATE TABLE ResumeSections (
    SectionID INT PRIMARY KEY IDENTITY(1,1),
    ResumeID INT FOREIGN KEY REFERENCES Resumes(ResumeID) ON DELETE CASCADE,
    SectionType NVARCHAR(50), -- Experience, Education, Project, Certification
    ContentJSON NVARCHAR(MAX), -- Lưu dữ liệu động dạng JSON để linh hoạt
    SortOrder INT DEFAULT 0
);

-- 5. Danh mục Kỹ năng hệ thống
CREATE TABLE Skills (
    SkillID INT PRIMARY KEY IDENTITY(1,1),
    SkillName NVARCHAR(100) UNIQUE NOT NULL
);

-- 6. Liên kết Kỹ năng vào CV
CREATE TABLE ResumeSkills (
    ResumeID INT FOREIGN KEY REFERENCES Resumes(ResumeID) ON DELETE CASCADE,
    SkillID INT FOREIGN KEY REFERENCES Skills(SkillID),
    Proficiency NVARCHAR(50), -- Beginner, Intermediate, Advanced
    PRIMARY KEY (ResumeID, SkillID)
);

-- 7. Bảng Tin tuyển dụng
CREATE TABLE Jobs (
    JobID INT PRIMARY KEY IDENTITY(1,1),
    RecruiterID INT FOREIGN KEY REFERENCES Users(UserID),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Requirements NVARCHAR(MAX), -- Chứa text để AI phân tích
    Salary NVARCHAR(100),
    Deadline DATETIME,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 8. Bảng Ứng tuyển
CREATE TABLE Applications (
    ApplicationID INT PRIMARY KEY IDENTITY(1,1),
    JobID INT FOREIGN KEY REFERENCES Jobs(JobID),
    ResumeID INT FOREIGN KEY REFERENCES Resumes(ResumeID),
    AppliedAt DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT 'Pending' -- Pending, Interview, Accepted, Rejected
);

-- 9. Log lịch sử dùng AI
CREATE TABLE AILogs (
    LogID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT FOREIGN KEY REFERENCES Users(UserID),
    RequestType NVARCHAR(50), -- Rewrite, SuggestSkill, MatchScore
    InputText NVARCHAR(MAX),
    OutputText NVARCHAR(MAX),
    UsedTokens INT,            -- Theo dõi chi phí
    CreatedAt DATETIME DEFAULT GETDATE()
);

USE DoAnWebCS;
GO

-- 1. Chèn dữ liệu Người dùng (Mật khẩu '123' đã hash mẫu)
INSERT INTO Users (FullName, Email, PasswordHash, Phone, Role) VALUES
(N'Nguyễn Quản Trị', 'admin@bettercv.com', 'hashed_pass_1', '090111222', 'Admin'),
(N'Công ty Công nghệ ABC', 'hr@abc-tech.com', 'hashed_pass_2', '090333444', 'Recruiter'),
(N'Trần Văn Sinh Viên', 'sinhvien@gmail.com', 'hashed_pass_3', '090555666', 'User'),
(N'Lê Thị Ứng Viên', 'levien@gmail.com', 'hashed_pass_4', '090777888', 'User');

-- 2. Chèn mẫu CV (Templates)
INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl) VALUES
(N'Modern Blue', '<div>HTML Structure here...</div>', '.cv-container { color: blue; }', 'https://example.com/modern-blue.png'),
(N'Classic Black', '<div>HTML Structure here...</div>', '.cv-container { font-family: Serif; }', 'https://example.com/classic.png');

-- 3. Chèn Kỹ năng hệ thống
INSERT INTO Skills (SkillName) VALUES 
('C#'), ('.NET Core'), ('SQL Server'), ('ReactJS'), ('HTML/CSS'), ('JavaScript'), ('AI Prompting');

-- 4. Chèn CV của người dùng
INSERT INTO Resumes (UserID, TemplateID, Title, Summary, ThemeColor) VALUES
(3, 1, N'CV Thực tập Backend', N'Sinh viên năm cuối ngành CNTT, đam mê lập trình C# và hệ thống.', '#2980b9'),
(3, 2, N'CV Freelance Web Design', N'Chuyên thiết kế giao diện web hiện đại với ReactJS.', '#2c3e50');

-- 5. Chèn nội dung chi tiết cho CV (Dữ liệu JSON)
INSERT INTO ResumeSections (ResumeID, SectionType, ContentJSON, SortOrder) VALUES
(1, 'Education', N'[{"School":"Đại học Công nghệ","Major":"CNTT","Year":"2022-2026"}]', 1),
(1, 'Experience', N'[{"Company":"FPT Software","Role":"Intern","Duration":"3 months"}]', 2),
(2, 'Project', N'[{"Name":"E-commerce Website","Tech":"React, Nodejs","Desc":"Bán hàng trực tuyến"}]', 1);

-- 6. Gán kỹ năng vào CV
INSERT INTO ResumeSkills (ResumeID, SkillID, Proficiency) VALUES
(1, 1, 'Intermediate'), -- CV 1 có C#
(1, 2, 'Beginner'),     -- CV 1 có .NET
(1, 3, 'Intermediate'), -- CV 1 có SQL
(2, 4, 'Advanced'),     -- CV 2 có ReactJS
(2, 5, 'Advanced');     -- CV 2 có HTML/CSS

-- 7. Chèn Tin tuyển dụng
INSERT INTO Jobs (RecruiterID, Title, Description, Requirements, Salary, Deadline) VALUES
(2, N'Lập trình viên .NET Junior', N'Làm việc tại Quận 1, hỗ trợ dự án ngân hàng.', N'Yêu cầu C#, SQL Server, hiểu biết về MVC.', '10-15 Million', '2026-05-30'),
(2, N'Frontend Developer (React)', N'Làm việc Remote, thiết kế UI/UX.', N'Thành thạo ReactJS, HTML/CSS.', 'Negotiable', '2026-06-15');

-- 8. Chèn dữ liệu Ứng tuyển
INSERT INTO Applications (JobID, ResumeID, Status) VALUES
(1, 1, 'Reviewing'),
(2, 2, 'Pending');

-- 9. Log mẫu AI
INSERT INTO AILogs (UserID, RequestType, InputText, OutputText, UsedTokens) VALUES
(3, 'Rewrite', 'I know C# and SQL', 'Expert in developing backend systems using C# and SQL Server...', 50);

GO