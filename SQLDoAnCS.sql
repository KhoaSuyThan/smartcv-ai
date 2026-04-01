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

-- 1. Tạo bảng GeminiConfigs
CREATE TABLE [GeminiConfigs] (
    [Id] int NOT NULL,
    [ApiKey] nvarchar(max) NULL,
    [ModelName] nvarchar(max) NOT NULL DEFAULT 'gemini-2.5-flash',
    [Temperature] float NOT NULL DEFAULT 0.7,
    [MaxOutputTokens] int NOT NULL DEFAULT 2048,
    [SystemInstruction] nvarchar(max) NULL,
    [SkillTemplate] nvarchar(max) NULL,
    [SummaryTemplate] nvarchar(max) NULL,
    [GrammarTemplate] nvarchar(max) NULL,
    [UserRateLimit] int NOT NULL DEFAULT 10,
    [TotalTokensUsed] bigint NOT NULL DEFAULT 0,
    CONSTRAINT [PK_GeminiConfigs] PRIMARY KEY ([Id])
);
GO
-- 2. Chèn dữ liệu cấu hình mặc định (Bắt buộc phải có 1 dòng Id = 1)
INSERT INTO [GeminiConfigs] 
([Id], [ApiKey], [ModelName], [Temperature], [MaxOutputTokens], [SystemInstruction], [UserRateLimit], [TotalTokensUsed])
VALUES 
(1, N'Bỏ API vào', N'gemini-2.5-flash', 0.7, 2048, N'Bạn là trợ lý ảo hỗ trợ đánh giá CV chuyên nghiệp.', 10, 0);
GO
-- 1. LƯU LỊCH SỬ XUẤT PDF
CREATE TABLE ResumeExports (
    ExportID INT PRIMARY KEY IDENTITY(1,1),
    ResumeID INT NOT NULL,
    ExportDate DATETIME DEFAULT GETDATE(),
    FileUrl NVARCHAR(500), -- Đường dẫn file PDF trên server (nếu có)
    DownloadCount INT DEFAULT 0,
    FOREIGN KEY (ResumeID) REFERENCES Resumes(ResumeID) ON DELETE CASCADE
);


-- 2. BẢNG NGƯỜI DÙNG
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Phone VARCHAR(20),
    CompanyID INT NULL, 
	AvatarUrl NVARCHAR(MAX) NULL,
    Role NVARCHAR(20) CHECK (Role IN ('Admin', 'User', 'Recruiter')) DEFAULT 'User',
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Users_Companies FOREIGN KEY (CompanyID) REFERENCES Companies(CompanyID) ON DELETE SET NULL
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
    AvatarUrl NVARCHAR(MAX),        -- Link ảnh chân dung
    
    Summary NVARCHAR(MAX),          -- Mục tiêu nghề nghiệp (Summary)
    ThemeColor VARCHAR(10) DEFAULT '#0d6efd',

	IsDraft BIT DEFAULT 1,       -- 1: Bản nháp (Auto-save), 0: Bản chính thức
    Version INT DEFAULT 1,       -- Số phiên bản để sau này làm Undo/Redo

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
    SkillID INT PRIMARY KEY IDENTITY(1,1),CREATE TABLE JobSkills (
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
    CompanyID INT NOT NULL,
	Status INT NOT NULL DEFAULT 0,
    FOREIGN KEY (RecruiterID) REFERENCES Users(UserID),
	FOREIGN KEY (CompanyID) REFERENCES Companies(CompanyID)
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

-- 8. Liên hệ hỗ trợ
CREATE TABLE [ContactMessages] (
    [Id]            INT IDENTITY(1,1) NOT NULL,    -- ID tự tăng
    [Name]          NVARCHAR(200) NOT NULL,      -- Họ tên người gửi
    [Email]         NVARCHAR(255) NOT NULL,     -- Email liên hệ
    [Subject]       NVARCHAR(500) NULL,         -- Tiêu đề tin nhắn
    [Message]       NVARCHAR(MAX) NOT NULL,     -- Nội dung chi tiết
    [AttachmentUrl] NVARCHAR(MAX) NULL,         -- ĐƯỜNG DẪN FILE ĐÍNH KÈM (Mới thêm)
    [SentAt]        DATETIME2 DEFAULT GETDATE(),-- Thời gian gửi
    
    CONSTRAINT [PK_ContactMessages] PRIMARY KEY ([Id])
);
GO

-- 9. Thông tin công ty
CREATE TABLE Companies (
    CompanyID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) NOT NULL,
    LogoUrl NVARCHAR(500),
    Website NVARCHAR(255),
    Description NVARCHAR(MAX),
    Address NVARCHAR(500),
    Industry NVARCHAR(100), -- Ngành nghề (IT, Marketing,...)
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 10. THÊM KỸ NĂNG YÊU CẦU CHO CÔNG VIỆC (DÀNH CHO AI MATCHING)
CREATE TABLE JobSkills (
    JobID INT NOT NULL,
    SkillID INT NOT NULL,
    RequiredProficiency NVARCHAR(50), -- Mức độ yêu cầu (Junior, Senior,...)
    PRIMARY KEY (JobID, SkillID),
    FOREIGN KEY (JobID) REFERENCES Jobs(JobID) ON DELETE CASCADE,
    FOREIGN KEY (SkillID) REFERENCES Skills(SkillID) ON DELETE CASCADE
);


-- 1. Thêm thử 1 dòng
INSERT INTO ContactMessages (Name, Email, Subject, Message)
VALUES (N'Nguyễn Văn Test', 'test@gmail.com', N'Hỏi về cách tạo CV', N'Em muốn hỏi cách chỉnh sửa ảnh đại diện ạ');


-- 1. Xóa mẫu cũ nếu tồn tại
DELETE FROM Templates WHERE Name = N'Modern Blue Sidebar';
GO

-- 2. Chèn mẫu CV với các khoảng cách đã được tối ưu (giảm ~50%)
INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive)
VALUES (
    N'Modern Blue Sidebar', 
    N'<div class="cv-container"><div class="cv-sidebar">

	<div class="profile-header"><img src="{{AvatarUrl}}" class="profile-pic"></div>

	<ul class="contact-info">
	<li><i class="fas fa-phone"></i> {{Phone}}</li>
	<li><i class="fas fa-envelope"></i> {{Email}}</li>
	<li><i class="fas fa-calendar-alt"></i> {{BirthDate}}</li>
	<li><i class="fas fa-map-marker-alt"></i> {{Address}}</li>
	</ul>

	<div class="sidebar-section"><h3>Học vấn</h3><div class="edu-item">{{Education}}</div></div>

	<div class="sidebar-section"><h3>Tin học</h3><div>{{Skills}}</div></div>

	<div class="sidebar-section"><h3>Ngoại ngữ</h3><div>{{Languages}}</div></div>

	<div class="sidebar-section"><h3>Người tham chiếu</h3><div class="ref-content">{{References}}</div></div>

	</div>

	<div class="cv-main">

	<div class="main-header">
	<h1>{{FullName}}</h1>
	<h2>{{JobTitle}}</h2>
	</div>

	<div class="main-section"><h3>Mục tiêu nghề nghiệp</h3><p>{{Summary}}</p></div>

	<div class="main-section"><h3>Kinh nghiệm làm việc</h3><div class="exp-container">{{Experience}}</div></div>

	<div class="main-section"><h3>Giải thưởng</h3><div class="award-container">{{Awards}}</div></div>

	<div class="main-section"><h3>Kỹ năng khác</h3><div class="other-skills">{{OtherSkills}}</div></div>

	</div></div>',

    -- CSS ĐÃ THU GỌN KHOẢNG CÁCH
    N'/* Reset và cấu trúc chung */
    .cv-container { display: flex; background: white; height: 297mm; font-family: "Segoe UI", sans-serif; line-height: 1.3; } 
    .cv-sidebar { flex: 3.2; background: #f7f9fc; padding: 15px; border-right: 1px solid #eee; } 
    .cv-main { flex: 6.8; padding: 15px 35px; overflow: hidden; }

    /* Ảnh đại diện */
    .profile-pic { width: 120px; height: 120px; border-radius: 50%; object-fit: cover; border: 3px solid white; box-shadow: 0 4px 10px rgba(0,0,0,0.1); margin-bottom: 8px; display: block; margin-left: auto; margin-right: auto; }

    /* Header: Tên cực to và sát vị trí */
    .main-header h1 { margin: 0; font-size: 42px; text-transform: uppercase; color: #333; line-height: 1; font-weight: 800; }
    .main-header h2 { margin: 0 0 8px 0; font-size: 18px; color: #666; font-weight: normal; text-transform: uppercase; letter-spacing: 1px; }

    /* Tiêu đề các mục: margin-top tạo khoảng cách "1 dòng", margin-bottom sát nội dung */
    .sidebar-section h3, .main-section h3 { 
        font-size: 14px; border-bottom: 1px solid #0d6efd; color: #0d6efd; 
        padding-bottom: 1px; 
        margin: 10px 0 2px 0 !important; /* 10px trên tạo khoảng cách, 2px dưới sát chữ */
        text-transform: uppercase; font-weight: bold; 
    }

    /* Nội dung: Triệt tiêu hoàn toàn margin mặc định */
    .main-section p, .exp-container, .award-container, .other-skills,
    .sidebar-section p, .sidebar-section ul, .sidebar-section li, .ref-content { 
        font-size: 12.5px; color: #333; 
        margin: 0 !important; /* Ép sát vào tiêu đề bên trên */
        padding: 0;
        word-wrap: break-word; overflow-wrap: break-word; 
        white-space: pre-line; text-align: justify;
    }

    /* Tối ưu Sidebar */
    .contact-info { list-style: none; padding: 0; font-size: 11.5px; margin-bottom: 10px; }
    .contact-info li { margin-bottom: 3px; display: flex; align-items: center; }
    .contact-info i { width: 18px; color: #0d6efd; margin-right: 6px; text-align: center; }
    .grad-type { color: #0d6efd; font-weight: 500; font-size: 11px; margin-top: 1px !important; }',

    '/images/templates/templatesCV_1.jpg',
    1
);
GO


DELETE FROM Templates WHERE Name = N'Modern Brown Professional';
GO

INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive)
VALUES (
    N'Modern Brown Professional', 
    N'<div class="brown-cv">

	<div class="sidebar">

	<div class="avatar-box"><img src="{{AvatarUrl}}" class="avatar"></div>

	<div class="contact-list">
	<div class="contact-item"><i class="fas fa-phone"></i> {{Phone}}</div>
	<div class="contact-item"><i class="fas fa-envelope"></i> {{Email}}</div>
	<div class="contact-item"><i class="fas fa-map-marker-alt"></i> {{Address}}</div>
	</div>

	<div class="sidebar-section"><h3 class="side-title">Kỹ năng</h3><div class="side-content">{{Skills}}</div></div>

	<div class="sidebar-section"><h3 class="side-title">Chứng chỉ</h3><div class="side-content">{{Certifications}}</div></div>

	<div class="sidebar-section"><h3 class="side-title">Giải thưởng</h3><div class="side-content">{{Awards}}</div></div>

	<div class="sidebar-section"><h3 class="side-title">Học vấn</h3><div class="side-content">{{Education}}</div></div>

	<div class="sidebar-section"><h3 class="side-title">Người tham chiếu</h3><div class="side-content">{{References}}</div></div>

	</div>

	<div class="main-body">

	<div class="header-brown">
	<h1>{{FullName}}</h1>
	<h2>{{JobTitle}}</h2>
	<div class="summary-text">{{Summary}}</div>
	</div>

	<div class="content-padding">

	<div class="main-section"><h3 class="main-title">Kinh nghiệm làm việc</h3><div class="exp-list">{{Experience}}</div></div>

	<div class="main-section"><h3 class="main-title">Hoạt động</h3><div class="activity-list">{{Activities}}</div></div>

	</div>

	</div>

	</div>',

    -- CSS ĐỊNH DẠNG MÀU NÂU TÂY SANG TRỌNG
    N'/* Cấu trúc Layout */
    .brown-cv { display: flex; background: white; height: 297mm; font-family: "Segoe UI", sans-serif; }
    .sidebar { flex: 3.5; background: #e5ddd5; padding: 30px 20px; display: flex; flex-direction: column; gap: 20px; }
    .main-body { flex: 6.5; display: flex; flex-direction: column; }

    /* Avatar tròn */
    .avatar { width: 150px; height: 150px; border-radius: 50%; object-fit: cover; display: block; margin: 0 auto 20px; border: 5px solid #d6cdc4; }

    /* Contact & Sidebar Title */
    .contact-list { font-size: 12px; border-top: 1px solid #c9beae; border-bottom: 1px solid #c9beae; padding: 10px 0; }
    .contact-item { margin-bottom: 8px; display: flex; align-items: center; gap: 10px; }
    .contact-item i { width: 24px; height: 24px; background: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 10px; color: #634c46; }

    .side-title { font-size: 16px; font-weight: bold; color: #333; margin-bottom: 10px; border-bottom: 1px solid #c9beae; padding-bottom: 5px; }
    .side-content { font-size: 12px; line-height: 1.5; color: #444; }

    /* Header màu nâu đậm */
    .header-brown { background: #634c46; color: white; padding: 40px 35px; }
    .header-brown h1 { margin: 0; font-size: 36px; text-transform: capitalize; font-weight: 800; letter-spacing: 1px; }
    .header-brown h2 { margin: 5px 0 15px 0; font-size: 16px; text-transform: uppercase; font-weight: normal; border-bottom: 1px solid rgba(255,255,255,0.3); padding-bottom: 10px; }
    .summary-text { font-size: 12.5px; line-height: 1.6; opacity: 0.9; text-align: justify; }

    /* Nội dung chính */
    .content-padding { padding: 30px 35px; }
    .main-title { font-size: 18px; font-weight: bold; color: #333; border-bottom: 2px solid #634c46; padding-bottom: 5px; margin-bottom: 15px; text-transform: uppercase; }
    
    /* Style cho các item kinh nghiệm (có badge ngày tháng) */
    .exp-item { margin-bottom: 15px; position: relative; }
    .exp-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 5px; }
    .date-badge { background: #9b8a7e; color: white; padding: 2px 10px; border-radius: 12px; font-size: 11px; font-weight: bold; }
    
    .company-name { font-weight: bold; font-size: 14px; color: #333; }
    .job-pos { font-style: italic; font-size: 13px; color: #555; display: block; margin-top: 2px; }
    .exp-desc { font-size: 12.5px; line-height: 1.5; margin-top: 5px; color: #444; }
    .exp-desc li { margin-bottom: 4px; padding-left: 5px; }',

    N'/images/templates/templatesCV_2.jpg',
    1
);
GO

DELETE FROM Templates WHERE Name = N'Elegant Accountant';
GO

INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive)
VALUES (
    N'Elegant Accountant', 
    N'<div class="accountant-cv">

	<div class="decor-top-right"></div>

	<div class="header-section">

	<div class="header-left">
	<h1 class="name-display">{{FullName}}</h1>
	<div class="personal-details">
	<p><strong>Ngày sinh:</strong> {{BirthDate}}</p>
	<p><strong>Địa chỉ:</strong> {{Address}}</p>
	<p><strong>Email:</strong> {{Email}}</p>
	<p><strong>Số điện thoại:</strong> {{Phone}}</p>
	</div>
	</div>

	<div class="header-center">
	<div class="avatar-container"><img src="{{AvatarUrl}}" class="avatar-img"></div>
	</div>

	<div class="header-right">
	<h2 class="job-display">{{JobTitle}}</h2>
	<div class="job-line"></div>
	</div>

	</div>

	<div class="body-section">

	<div class="cv-block">
	<h3 class="block-title">KINH NGHIỆM LÀM VIỆC</h3>
	<div class="experience-grid">{{Experience}}</div>
	</div>

	<div class="cv-block">
	<h3 class="block-title">TRÌNH ĐỘ HỌC VẤN</h3>
	<div class="education-list">{{Education}}</div>
	</div>

	<div class="cv-block">
	<h3 class="block-title">KỸ NĂNG</h3>
	<div class="skills-flex">{{Skills}}</div>
	</div>

	</div>

	<div class="decor-bottom-right"></div>

	</div>',

    -- CSS TỐI ƯU KHOẢNG CÁCH VÀ KÍCH THƯỚC CHỮ
    N'/* Cấu trúc Layout */
    .accountant-cv { background: white; height: 297mm; padding: 50px; font-family: "Segoe UI", sans-serif; color: #333; position: relative; overflow: hidden; }
    
    /* Header (Neil Tran Style) */
    .header-section { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 25px; }
    .header-left { flex: 1.5; border-top: 1px solid #ddd; padding-top: 15px; }
    .header-center { flex: 1; display: flex; justify-content: center; }
    .header-right { flex: 1; text-align: right; border-top: 1px solid #333; padding-top: 15px; }

    .name-display { font-size: 55px; color: #556b8d; margin: 0; font-family: Georgia, serif; line-height: 1; font-weight: normal; }
    .job-display { font-size: 28px; color: #333; line-height: 1.1; margin: 0; }
    
    .personal-details { font-size: 14.5px; margin-top: 20px; line-height: 1.8; }
    .avatar-img { width: 180px; height: 180px; border-radius: 50%; object-fit: cover; background: #eee; }

    /* Nội dung các mục (Tăng cỡ chữ) */
    .block-title { font-size: 24px; font-weight: bold; margin: 35px 0 15px 0; color: #222; }
    
    /* CHIA 2 CỘT KINH NGHIỆM - TRIỆT TIÊU LỖI ĐÈ NHAU */
    .experience-grid { 
        display: grid; 
        grid-template-columns: 1fr 1fr; /* Chia đều 50% cho mỗi bên */
        column-gap: 50px; 
        row-gap: 25px; 
        align-items: flex-start;
    }
    
    /* Xử lý khi người dùng nhập chuỗi liên tục (như aaaaaa...) */
    .exp-item { 
        font-size: 15px; 
        word-wrap: break-word; 
        overflow-wrap: anywhere; 
        white-space: pre-line;
    }
    
    .exp-year { font-weight: bold; font-size: 17px; margin-bottom: 5px; display: block; }
    .exp-content { line-height: 1.6; text-align: justify; }

    /* Học vấn và Kỹ năng */
    .education-list { font-size: 15.5px; line-height: 1.7; }
    .skills-flex { display: grid; grid-template-columns: 1fr 1fr; gap: 20px 60px; }
    .skill-label { font-size: 15.5px; font-style: italic; margin-bottom: 8px; display: block; }
    .progress-bg { height: 10px; background: #e0e6ed; border-radius: 5px; }
    .progress-fill { height: 100%; background: #556b8d; border-radius: 5px; }

    /* Họa tiết trang trí (Scribbles) */
    .decor-top-right { position: absolute; top: 10px; right: 20px; width: 100px; height: 100px; background: url("data:image/svg+xml,%3Csvg xmlns=''http://www.w3.org/2000/svg'' width=''100'' height=''100''%3E%3Cpath d=''M10 10 Q 50 10 90 90'' fill=''none'' stroke=''black'' stroke-width=''1''/%3E%3C/svg%3E") no-repeat; opacity: 0.5; }
',
    N'/images/templates/templatesCV_3.jpg',
    1
);
GO

-- 1. Xóa bản cũ để cập nhật bản mới có "Kỹ năng khác"
DELETE FROM Resumes 
WHERE TemplateID IN (SELECT TemplateID FROM Templates WHERE Name = N'Đảo Phú Quý - Vieclam24h');


INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive)
VALUES (
    N'Đảo Phú Quý - Vieclam24h', 
    N'<div class="cv-wrapper">
        <div class="cv-sidebar">
            <div class="cv-avatar-section">
                <img src="{{AvatarUrl}}" class="cv-avatar">
            </div>
            <div class="cv-sidebar-content">
                <div class="cv-info-block">
                    <h3 class="cv-side-title">THÔNG TIN LIÊN HỆ</h3>
                    <p><i class="fas fa-phone"></i> {{Phone}}</p>
                    <p><i class="fas fa-envelope"></i> {{Email}}</p>
                    <p><i class="fas fa-map-marker-alt"></i> {{Address}}</p>
                </div>

                <div class="cv-info-block">
                    <h3 class="cv-side-title">HỌC VẤN</h3>
                    <div class="cv-edu-sidebar">{{Education}}</div>
                </div>

                <div class="cv-info-block">
                    <h3 class="cv-side-title">TIN HỌC</h3>
                    <div class="cv-skill-list">{{Skills}}</div>
                </div>
                <div class="cv-info-block">
                    <h3 class="cv-side-title">NGOẠI NGỮ</h3>
                    <div class="cv-lang-list">{{Languages}}</div>
                </div>
                <div class="cv-info-block">
                    <h3 class="cv-side-title">KỸ NĂNG KHÁC</h3>
                    <div class="cv-other-skills">{{OtherSkills}}</div>
                </div>
            </div>
        </div>

        <div class="cv-main">
            <div class="cv-header">
                <h1 class="cv-name">{{FullName}}</h1>
                <h2 class="cv-job">{{JobTitle}}</h2>
            </div>
            <div class="cv-section">
                <h3 class="cv-section-title"><i class="fas fa-user"></i> MỤC TIÊU NGHỀ NGHIỆP</h3>
                <div class="cv-section-content">{{Summary}}</div>
            </div>
            <div class="cv-section">
                <h3 class="cv-section-title"><i class="fas fa-briefcase"></i> KINH NGHIỆM LÀM VIỆC</h3>
                <div class="cv-section-content">{{Experience}}</div>
            </div>

            <div class="cv-section">
                <h3 class="cv-section-title"><i class="fas fa-users"></i> NGƯỜI THAM CHIẾU</h3>
                <div class="cv-section-content">{{References}}</div>
            </div>
        </div>
    </div>',

    N'/* Layout chung */
    .cv-wrapper { display: flex; width: 210mm; min-height: 297mm; background: white; font-family: "Segoe UI", sans-serif; }
    .cv-sidebar { flex: 3.5; background: #004C82; color: white; padding: 30px 20px; }
    .cv-main { flex: 6.5; padding: 40px; background: #ffffff; }

    .cv-avatar-section { text-align: center; margin-bottom: 30px; }
    .cv-avatar { width: 160px; height: 160px; border-radius: 50%; border: 5px solid rgba(255,255,255,0.2); object-fit: cover; }

    .cv-side-title { font-size: 16px; font-weight: bold; border-bottom: 1px solid rgba(255,255,255,0.3); padding-bottom: 5px; margin-bottom: 10px; margin-top: 20px; text-transform: uppercase; }
    
    /* STYLE CHO SIDEBAR VÀ CHỐNG TRÀN */
    .cv-info-block p, .cv-other-skills, .cv-edu-sidebar, .cv-lang-list, .cv-skill-list { 
        font-size: 13px; 
        margin-bottom: 8px; 
        line-height: 1.5;
        word-wrap: break-word; 
        overflow-wrap: break-word; 
        word-break: break-all; 
    }
    .cv-info-block i { width: 20px; text-align: center; }

    /* MAIN CONTENT */
    .cv-header { border-bottom: 3px solid #004C82; padding-bottom: 15px; margin-bottom: 30px; }
    .cv-name { font-size: 40px; font-weight: 800; color: #004C82; margin: 0; text-transform: uppercase; }
    .cv-job { font-size: 18px; color: #555; margin: 5px 0 0 0; text-transform: uppercase; letter-spacing: 2px; }

    .cv-section { margin-bottom: 25px; }
    .cv-section-title { font-size: 17px; font-weight: bold; color: #004C82; display: flex; align-items: center; gap: 10px; margin-bottom: 10px; border-bottom: 1px solid #eee; padding-bottom: 5px; }

    /* NỘI DUNG CHÍNH: CHỐNG TRÀN VÀ CÓ KHUNG */
    .cv-section-content { 
        font-size: 14px; 
        line-height: 1.6; 
        color: #333; 
        text-align: justify; 
        white-space: pre-line; 
        word-wrap: break-word; 
        overflow-wrap: break-word; 
        word-break: break-word; 
    }',

    '/images/templates/dao_phu_quy.jpg',
    1
);
GO
-- 1. Xóa mẫu cũ nếu trùng tên
DELETE FROM Resumes 
WHERE TemplateID IN (SELECT TemplateID FROM Templates WHERE Name = N'Mẫu CV Professional Blue - Ngô Hải Yến');

-- 2. Chèn mẫu mới
INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive)
VALUES (
    N'Mẫu CV Professional Blue - Ngô Hải Yến', 
    N'<div class="cv-yens-wrapper">
        <div class="cv-sidebar">
            <div class="avatar-box">
                <img src="{{AvatarUrl}}" class="avatar-img">
            </div>

            <div class="sidebar-section">
                <h3 class="sidebar-title">LIÊN HỆ VỚI TÔI</h3>
                <div class="contact-list">
                    <div class="contact-item"><i class="fas fa-map-marker-alt"></i> {{Address}}</div>
                    <div class="contact-item"><i class="fas fa-envelope"></i> {{Email}}</div>
                    <div class="contact-item"><i class="fas fa-phone"></i> {{Phone}}</div>
                </div>
            </div>

            <div class="sidebar-section">
                <h3 class="sidebar-title">TÓM TẮT KỸ NĂNG</h3>
                <div class="skill-group">
                    <p class="skill-label">TIN HỌC</p>
                    <div class="skill-content">{{Skills}}</div>
                </div>
                <div class="skill-group">
                    <p class="skill-label">NGOẠI NGỮ</p>
                    <div class="skill-content">{{Languages}}</div>
                </div>
                <div class="skill-group">
                    <p class="skill-label">KỸ NĂNG KHÁC</p>
                    <div class="skill-content">{{OtherSkills}}</div>
                </div>
            </div>

            <div class="sidebar-section">
                <h3 class="sidebar-title">GIẢI THƯỞNG</h3>
                <div class="award-list">{{Awards}}</div>
            </div>
        </div>

        <div class="cv-main">
            <div class="header-info">
                <h1 class="fullname">{{FullName}}</h1>
                <h2 class="job-title">{{JobTitle}}</h2>
            </div>

            <div class="main-section">
                <h3 class="section-title">MỤC TIÊU NGHỀ NGHIỆP</h3>
                <div class="section-content">{{Summary}}</div>
            </div>

            <div class="main-section">
                <h3 class="section-title">KINH NGHIỆM LÀM VIỆC</h3>
                <div class="section-content">{{Experience}}</div>
            </div>

            <div class="main-section">
                <h3 class="section-title">QUÁ TRÌNH HỌC VẤN</h3>
                <div class="section-content">{{Education}}</div>
            </div>
        </div>
    </div>',

    N'/* Layout và màu sắc chủ đạo */
    .cv-yens-wrapper { display: flex; width: 210mm; min-height: 297mm; background: white; font-family: "Segoe UI", Arial, sans-serif; }
    .cv-sidebar { flex: 3.5; background-color: #32507d; color: white; padding: 40px 25px; }
    .cv-main { flex: 6.5; padding: 50px 40px; }

    /* Avatar tròn */
    .avatar-box { text-align: center; margin-bottom: 40px; }
    .avatar-img { width: 160px; height: 160px; border-radius: 50%; border: 6px solid rgba(255,255,255,0.1); object-fit: cover; }

    /* Sidebar Styles */
    .sidebar-title { font-size: 16px; font-weight: bold; border-bottom: 1px solid rgba(255,255,255,0.3); padding-bottom: 8px; margin-bottom: 15px; margin-top: 30px; letter-spacing: 1px; }
    .contact-item { font-size: 13px; margin-bottom: 12px; display: flex; align-items: flex-start; gap: 10px; line-height: 1.4; word-break: break-word; }
    .contact-item i { margin-top: 3px; width: 15px; }
    
    .skill-label { font-size: 12px; font-weight: bold; margin-bottom: 5px; color: #a5b8d4; text-transform: uppercase; }
    .skill-content { font-size: 13px; margin-bottom: 15px; white-space: pre-line; word-break: break-all; }
    .award-list { font-size: 13px; line-height: 1.6; }

    /* Main Content Styles */
    .fullname { font-size: 48px; color: #32507d; margin: 0; font-weight: 800; text-transform: uppercase; }
    .job-title { font-size: 20px; color: #5fb4c4; margin: 5px 0 40px 0; letter-spacing: 3px; font-weight: bold; text-transform: uppercase; }

    .section-title { font-size: 16px; font-weight: bold; color: #5fb4c4; margin-bottom: 15px; margin-top: 35px; letter-spacing: 2px; }
    .section-content { 
        font-size: 14px; 
        line-height: 1.8; 
        color: #444; 
        text-align: justify; 
        white-space: pre-line; 
        word-break: break-word;
        border-left: 1px solid #eee;
        padding-left: 15px;
    }

    /* Đảm bảo nội dung không tràn khi nhập chuỗi dài */
    p, div, span { word-wrap: break-word; overflow-wrap: break-word; }',

    'https://images.careerviet.vn/content/images/tai-mau-cv-xin-viec-file-pdf-careerbuilder-5.jpg',
    1
);
GO
-- Nạp Kỹ năng IT
INSERT INTO Skills (SkillName) VALUES ('.NET'), ('SQL Server'), ('C#'), ('Flutter'), ('React');
GO

INSERT INTO Companies (Name, LogoUrl, Website, Address, Industry) VALUES 
(N'Baxter and Woodman Inc', 'https://serpapi.com/searches/69c69eec13f4b6286fa54fe0/images/gnezDA5qkvM_3V23SGiDNMhfOjjqA_Wj4jT-kPPNvlM.jpeg', 'https://www.indeed.com/viewjob?jk=3e227540a546a37c', N'Chicago, IL', N'IT Consulting'),
(N'Rsm Us Llp.', 'https://serpapi.com/searches/69c69eec13f4b6286fa54fe0/images/vLqbAgGTGajIdOCOXiWJPSgcVZX5T_Ohu_uq0wdzFUo.png', 'https://www.whatjobs.com/jobs/senior-project-manager-it/chicago-illinois?id=2570160112', N'Chicago, IL', N'Agile Delivery'),
(N'Contemporary Staffing', NULL, 'https://careers.contemporarystaffing.com/jobs/37462', N'Elk Grove Village, IL', N'IT Support'),
(N'McDonald''s Corporation', 'https://serpapi.com/searches/69c69eec13f4b6286fa54fe0/images/cM5DL5-_2HfX1X3jU9h2bwTwKIyXurvcAiKUJViuU3M.jpeg', 'https://www.jobzmall.com/mcdonald-s-corporation/job/analyst-penetration-testing', N'Chicago, IL', N'Cybersecurity'),
(N'Kirkland & Ellis', 'https://serpapi.com/searches/69c69eec13f4b6286fa54fe0/images/P423na0BD2YiXEZGVgmm7l-9rwDrdMdraMSCYUtESJ0.png', 'https://us.jobrapido.com/jobpreview/5072755767732338688', N'Chicago, IL', N'FinOps'),
(N'Lenovo', 'https://serpapi.com/searches/69c69eec13f4b6286fa54fe0/images/TkUNTkzdVjwXaTmgCkjX1Gad8pgzgHWHajnZnYymu7k.jpeg', 'https://www.indeed.com/viewjob?jk=9d95ff691e418663', N'Chicago, IL', N'Artificial Intelligence'),
(N'U.S. Navy', 'https://serpapi.com/searches/69c69eec13f4b6286fa54fe0/images/RRLfWjenk0dMUZ4AKgPAWGeNn2kbcmLkmwHVRJ0DySI.png', 'https://www.adzuna.com/details/5655660238', N'South Holland, IL', N'Information Warfare'),
(N'Motorola', NULL, 'https://www.ziprecruiter.com/c/Motorola-Solutions/Job/IT-Directory-Infrastructure-Lead-(Chicago-Schaumburg-Hybrid)/-in-Schaumburg,IL?jid=bc508c769e0fb0d8', N'Schaumburg, IL', N'Infrastructure'),
(N'Nexzentek Solutions', 'https://serpapi.com/searches/69c69eec13f4b6286fa54fe0/images/MC0CnGAbtG01fQQrkYgeznkEPWV7a-5IvkFbQdGIyTw.png', 'https://www.optnation.com/aws-developer-job-in-chicago-il-view-jobid-25038', N'Chicago, IL', N'AWS Cloud'),
(N'United Airlines', NULL, 'https://careers.united.com/us/en/job/WHQ00025769/Analyst-Identity-Access-Management', N'Chicago, IL', N'Aviation DT');

INSERT INTO Jobs (Title, Description, Requirements, Salary, Deadline, CompanyID, RecruiterID, Status) VALUES 
(N'IT Consultant', N'Thiết kế, hỗ trợ và bảo trì các giải pháp CNTT cho khách hàng đô thị.', N'8+ năm kinh nghiệm, thạo Windows Server, Active Directory, ảo hóa.', N'90K - 120K / year', '2026-12-31', 1, 1, 1),
(N'Senior Project Manager: IT & Agile Delivery', N'Dẫn dắt các dự án phức tạp ngân sách > $200k.', N'8-10 năm PM, 3-5 năm Agile. Ưu tiên chứng chỉ PMI.', N'Thỏa thuận', '2026-11-20', 2, 1, 1),
(N'IT Support Specialist Tier 2', N'Hỗ trợ kỹ thuật nâng cao onsite và remote cho sản xuất linh kiện ô tô.', N'Troubleshoot Windows/Mac, thạo Google Workspace/O365.', N'Thỏa thuận', '2026-10-15', 3, 1, 1),
(N'Analyst, Penetration Testing', N'Kiểm tra xâm nhập hệ thống và mạng toàn cầu để tìm lỗ hổng bảo mật.', N'Nền tảng Pentest mạnh, kỹ năng phân tích độc lập tốt.', N'98K - 120K / year', '2026-12-05', 4, 1, 1),
(N'FinOps IT Finance Analyst', N'Đảm bảo minh bạch tài chính cho các sáng kiến công nghệ và Cloud spend.', N'4-6 năm kinh nghiệm, cử nhân tài chính, thạo Excel/Analytical.', N'Cạnh tranh', '2026-09-12', 5, 1, 1),
(N'AI QA - Engineer', N'Đảm bảo hệ thống AI (LLM, Vision) chính xác và an toàn quy mô lớn.', N'3+ năm QA AI/ML, thạo Python, ML model evaluation.', N'110K - 150K / year', '2026-08-30', 6, 1, 1),
(N'Information Systems Technician', N'Vận hành và bảo vệ mạng lưới hạm đội Navy toàn cầu.', N'Cần quốc tịch Mỹ, vượt qua bài kiểm tra năng khiếu quân đội (ASVAB).', N'Theo quy định', '2026-12-31', 7, 1, 1),
(N'IT Directory Infrastructure Lead', N'Quản trị toàn cầu môi trường Active Directory và email backbone.', N'8-10 năm kinh nghiệm AD/DNS, 3+ năm vị trí lãnh đạo.', N'145K - 170K / year', '2026-07-25', 8, 1, 1),
(N'AWS Developer', N'Phát triển Cloud cho dự án dài hạn của United Airlines.', N'8+ năm kinh nghiệm, nền tảng .NET mạnh, giao tiếp tốt.', N'Theo hợp đồng', '2026-06-10', 9, 1, 1),
(N'Analyst - Identity & Access Management', N'Bảo mật hệ thống IAM, quản lý xác thực hiện đại (OIDC, SAML, SSO).', N'3+ năm kinh nghiệm, thạo Okta/Entra/SailPoint.', N'87K - 114K / year', '2026-05-18', 10, 1, 1);

-- Thêm 5 Công ty tiếp theo (ID từ 11 đến 15)
INSERT INTO Companies (Name, LogoUrl, Website, Address, Industry) VALUES 
(N'Google (Chicago Tower)', 'https://www.google.com/images/branding/googlelogo/2x/googlelogo_color_92x30dp.png', 'https://www.google.com/about/careers/applications/', N'Chicago, IL', N'Cloud & Search'),
(N'Zebra Technologies', 'https://www.zebra.com/content/dam/zebra_new_ia/en-us/solutions-verticals/product-logos/Zebra_Logo_Tagline_Black.png', 'https://www.zebra.com/us/en/about-zebra/careers.html', N'Lincolnshire, IL', N'Enterprise Technology'),
(N'Salesforce', 'https://a.sfdcstatic.com/shared/images/c360-nav/salesforce-with-type-logo.svg', 'https://careers.salesforce.com/en/jobs/', N'Chicago, IL', N'CRM & SaaS'),
(N'Grubhub', 'https://pwa-cdn.grubhub.com/beta/grubhub-assets/images/grubhub-logo-red.svg', 'https://careers.grubhub.com/', N'Chicago, IL', N'FoodTech'),
(N'Allstate', 'https://www.allstate.com/content/dam/allstate/allstate-logo-header.png', 'https://www.allstate.jobs/', N'Northbrook, IL', N'InsurTech');

-- Thêm 5 Công việc tương ứng (Liên kết với CompanyID 11-15)
INSERT INTO Jobs (Title, Description, Requirements, Salary, Deadline, CompanyID, RecruiterID, Status) VALUES 
(N'Senior Software Engineer - Site Reliability', N'Đảm bảo hệ thống hạ tầng Cloud của Google hoạt động ổn định và có khả năng mở rộng cao.', N'5+ năm kinh nghiệm với C++, Java hoặc Go. Thạo hệ thống Linux và hạ tầng mạng Cloud.', N'145K - 195K / year', '2026-12-25', 11, 1, 1),
(N'Embedded Software Engineer', N'Phát triển phần mềm nhúng cho các thiết bị quét mã vạch và máy tính di động công nghiệp.', N'Thạo C/C++, kiến trúc RTOS và vi điều khiển ARM. Hiểu biết về giao tiếp WiFi/Bluetooth.', N'95K - 135K / year', '2026-11-15', 12, 1, 1),
(N'Technical Solutions Architect', N'Thiết kế giải pháp tích hợp hệ thống CRM Salesforce cho các doanh nghiệp tài chính lớn.', N'Chứng chỉ Salesforce Architect, 8+ năm kinh nghiệm phần mềm, thạo Apex và LWC.', N'160K - 210K / year', '2026-10-30', 13, 1, 1),
(N'Senior Data Scientist', N'Phát triển các mô hình học máy để tối ưu hóa thời gian giao hàng và trải nghiệm thực khách.', N'Thạc sĩ/Tiến sĩ toán tin, 4+ năm kinh nghiệm Python/R, thạo SQL và các thư viện ML.', N'125K - 170K / year', '2026-09-20', 14, 1, 1),
(N'Cloud Security Engineer', N'Bảo mật hạ tầng Azure cho hệ thống bảo hiểm toàn cầu, quản lý Identity và mã hóa dữ liệu.', N'3+ năm bảo mật Cloud, chứng chỉ AZ-500 hoặc tương đương. Thạo Terraform/Ansible.', N'115K - 155K / year', '2026-08-05', 15, 1, 1);



