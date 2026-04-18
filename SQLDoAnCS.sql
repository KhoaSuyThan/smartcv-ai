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

-- Test templates dùng Vue
CREATE TABLE VueTemplates (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TemplateName NVARCHAR(100) NOT NULL,    -- Tên hiển thị (VD: Mẫu Thanh Xuân)
    ComponentName NVARCHAR(100) NOT NULL,   -- Tên File Vue (VD: Template)
    ThumbnailUrl NVARCHAR(500),             -- Ảnh demo
    IsPremium BIT DEFAULT 0,                -- 0: Miễn phí, 1: Pro
    IsActive BIT DEFAULT 1,
	Category NVARCHAR(255) NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

INSERT INTO VueTemplates (TemplateName, ComponentName, ThumbnailUrl, IsPremium, Category)
VALUES (N'Trần Hoài Thu', 'Template', '/images/templates/template_476e4073.png', 1, N'IT, Thực tập, Chuyên nghiệp');
INSERT INTO VueTemplates (TemplateName, ComponentName, ThumbnailUrl, IsPremium, Category)
VALUES (N'Nguyễn Yến Nhi', 'NguyenYenNhi', 'https://marketplace.canva.com/EAGSZ3G6wMw/2/0/1131w/canva-s%C6%A1-y%E1%BA%BFu-l%C3%BD-l%E1%BB%8Bch-chuy%C3%AAn-nghi%E1%BB%87p-hi%E1%BB%87n-%C4%91%E1%BA%A1i-n%E1%BB%AF-t%C3%ADnh-thanh-l%E1%BB%8Bch-h%E1%BB%93ng-tr%E1%BA%AFng-N3-BrRHpD_E.jpg', 1, N'Kinh doanh, Marketing, Sáng tạo');

-- 1. Thông tin công ty
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

-- 2. BẢNG MẪU CV (Lưu trữ cấu trúc giao diện)
CREATE TABLE Templates (
    TemplateID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL,
    HtmlContent NVARCHAR(MAX), 
    CssContent NVARCHAR(MAX),  
    PreviewImageUrl NVARCHAR(500),
	IsProOnly BIT NOT NULL DEFAULT 0,
    IsActive BIT DEFAULT 1,
	Category NVARCHAR(MAX) NULL
);

-- 3. HỆ THỐNG KỸ NĂNG VÀ VIỆC LÀM (Phục vụ Matching AI)
CREATE TABLE Skills (
    SkillID INT PRIMARY KEY IDENTITY(1,1),
    SkillName NVARCHAR(100) UNIQUE NOT NULL
);

-- 4. Tạo bảng GeminiConfigs
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
	ProModelName NVARCHAR(MAX) NULL,
	ProTemperature FLOAT NOT NULL DEFAULT 0.9,
	ProMaxOutputTokens INT NOT NULL DEFAULT 4096,
	ProUserRateLimit INT NOT NULL DEFAULT 50,
	ChatbotApiKey nvarchar(max) NULL,
    CONSTRAINT [PK_GeminiConfigs] PRIMARY KEY ([Id])
);
GO

INSERT INTO [GeminiConfigs] ([Id], [ModelName], [Temperature], [MaxOutputTokens], [ProModelName], [ProTemperature], [ProMaxOutputTokens], [ProUserRateLimit])
VALUES (1, 'gemini-2.5-flash', 0.7, 2048, 'gemini-2.5-pro', 0.9, 4096, 50);
GO

INSERT INTO [GeminiConfigs] 
([Id], [ApiKey], [ModelName], [Temperature], [MaxOutputTokens], [SystemInstruction], [UserRateLimit], [TotalTokensUsed])
VALUES 
(1, N'Tự thêm API', N'gemini-2.5-flash', 0.7, 2048, N'Bạn là trợ lý ảo hỗ trợ đánh giá CV chuyên nghiệp.', 10, 0);
GO

-- 5. Liên hệ hỗ trợ
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

-- 6. BẢNG NGƯỜI DÙNG
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
	IsPro BIT NOT NULL DEFAULT 0,
	PasswordChangeToken NVARCHAR(MAX) NULL,
	PasswordChangeTokenExpires DATETIME2 NULL,
	PendingPasswordHash NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Users_Companies FOREIGN KEY (CompanyID) REFERENCES Companies(CompanyID) ON DELETE SET NULL
);

-- 7. BẢNG CV CHÍNH (Chứa thông tin cá nhân "tĩnh" - Khớp Editor)
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

	JsonContent NVARCHAR(MAX) NULL,

    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    FOREIGN KEY (TemplateID) REFERENCES Templates(TemplateID)
);

-- 8. BẢNG NỘI DUNG CHI TIẾT (Lưu danh sách "động" như Experience, Education dạng JSON)
CREATE TABLE ResumeSections (
    SectionID INT PRIMARY KEY IDENTITY(1,1),
    ResumeID INT NOT NULL,
    SectionType NVARCHAR(50), -- 'Experience', 'Education', 'Projects', 'Other'
    ContentJSON NVARCHAR(MAX), -- Lưu mảng đối tượng JSON để linh hoạt cao
    SortOrder INT DEFAULT 0,
    FOREIGN KEY (ResumeID) REFERENCES Resumes(ResumeID) ON DELETE CASCADE
);

-- 9. LƯU LỊCH SỬ XUẤT PDF
CREATE TABLE ResumeExports (
    ExportID INT PRIMARY KEY IDENTITY(1,1),
    ResumeID INT NOT NULL,
    ExportDate DATETIME DEFAULT GETDATE(),
    FileUrl NVARCHAR(500), -- Đường dẫn file PDF trên server (nếu có)
    DownloadCount INT DEFAULT 0,
    FOREIGN KEY (ResumeID) REFERENCES Resumes(ResumeID) ON DELETE CASCADE
);

-- 10. Lưu kỹ năng cho CV
CREATE TABLE ResumeSkills (
    ResumeID INT NOT NULL,
    SkillID INT NOT NULL,
    Proficiency NVARCHAR(50), -- Beginner, Intermediate, Advanced
    PRIMARY KEY (ResumeID, SkillID),
    FOREIGN KEY (ResumeID) REFERENCES Resumes(ResumeID) ON DELETE CASCADE,
    FOREIGN KEY (SkillID) REFERENCES Skills(SkillID) ON DELETE CASCADE
);


-- 11. Lưu jobs
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

-- 12. THÊM KỸ NĂNG YÊU CẦU CHO CÔNG VIỆC (DÀNH CHO AI MATCHING)
CREATE TABLE JobSkills (
    JobID INT NOT NULL,
    SkillID INT NOT NULL,
    RequiredProficiency NVARCHAR(50), -- Mức độ yêu cầu (Junior, Senior,...)
    PRIMARY KEY (JobID, SkillID),
    FOREIGN KEY (JobID) REFERENCES Jobs(JobID) ON DELETE CASCADE,
    FOREIGN KEY (SkillID) REFERENCES Skills(SkillID) ON DELETE CASCADE
);


-- 13. Duyệt CV
CREATE TABLE Applications (
    ApplicationID INT PRIMARY KEY IDENTITY(1,1),
    JobID INT NOT NULL,
    ResumeID INT NOT NULL,
    AppliedAt DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT 'Pending', -- Pending, Reviewing, Accepted, Rejected
    FOREIGN KEY (JobID) REFERENCES Jobs(JobID),
    FOREIGN KEY (ResumeID) REFERENCES Resumes(ResumeID) ON DELETE NO ACTION -- Tránh vòng lặp Cascade
);

-- 14. LOG HỆ THỐNG AI
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

-- 15. Cập nhật yêu cầu
CREATE TABLE [UpgradeRequests] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [UserID]           INT            NOT NULL,
    [RequestDate]      DATETIME       NOT NULL DEFAULT (GETDATE()),
    [Status]           INT            NOT NULL DEFAULT (0),
    [EvidenceImageUrl] NVARCHAR (MAX) NULL,
    [Notes]            NVARCHAR (MAX) NULL,
    [DecisionDate]     DATETIME       NULL,
    PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Upgrade_User] FOREIGN KEY ([UserID]) REFERENCES [Users]([UserID])
);

-- 1. Thêm thử 1 dòng
INSERT INTO ContactMessages (Name, Email, Subject, Message)
VALUES (N'Nguyễn Văn Test', 'test@gmail.com', N'Hỏi về cách tạo CV', N'Em muốn hỏi cách chỉnh sửa ảnh đại diện ạ');

-- 1. Xóa mẫu cũ nếu tồn tại
DELETE FROM Templates WHERE Name = N'Modern Blue Sidebar';
GO

-- 2. Chèn mẫu CV với các khoảng cách đã được tối ưu (giảm ~50%)
INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive, Category)
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
    1,
	N'Kinh tế, Marketing, Sáng tạo'
);
GO


DELETE FROM Templates WHERE Name = N'Modern Brown Professional';
GO

INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive, Category)
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
    1,
	N'IT, Kinh tế'
);
GO

DELETE FROM Templates WHERE Name = N'Elegant Accountant';
GO

INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive, Category)
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
    1,
	N'Marketing, Sáng tạo'
);
GO

-- 1. Xóa bản cũ để cập nhật bản mới có "Kỹ năng khác"
DELETE FROM Resumes 
WHERE TemplateID IN (SELECT TemplateID FROM Templates WHERE Name = N'Đảo Phú Quý - Vieclam24h');


INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive, Category)
VALUES (
    N'Đảo Phú Quý', 
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

    'https://cdn1.vieclam24h.vn/images/assets/img/072-blue-simple-professional.jpg?v=1',
    1,
	N'IT, Marketing'
);
GO
-- 1. Xóa mẫu cũ nếu trùng tên
DELETE FROM Resumes 
WHERE TemplateID IN (SELECT TemplateID FROM Templates WHERE Name = N'Mẫu CV Professional Blue - Ngô Hải Yến');

-- 2. Chèn mẫu mới
INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive, Category)
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
    1,
	N'IT, Kinh tế, Marketing'
);
GO

-- 1. Xóa mẫu cũ để cập nhật bản mới
DELETE FROM Templates WHERE Name = N'Mẫu CV Pink Elegant - Nguyễn Yên Nhi';

-- 2. Chèn mẫu mới với phần Kỹ năng tổng hợp
INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive, Category)
VALUES (
    N'Mẫu CV Pink Elegant - Nguyễn Yên Nhi', 
    N'<div class="pink-cv-container">
        <div class="deco-star star-1">✦</div>
        <div class="deco-star star-2">✦</div>

        <div class="cv-header">
            <div class="header-info">
                <h1 class="fullname">{{FullName}}</h1>
                <p class="job-title">{{JobTitle}}</p>
            </div>
            <div class="header-photo">
                <div class="photo-bg-circle"></div>
                <img src="{{AvatarUrl}}" class="avatar-img">
            </div>
        </div>

        <div class="cv-body">
            <div class="col-left">
                <div class="section">
                    <h1 class="section-title">MỤC TIÊU NGHỀ NGHIỆP</h1>
                    <div class="content-text">{{Summary}}</div>
                </div>

                <div class="section">
                    <h1 class="section-title">HỌC VẤN</h1>
                    <div class="timeline">
                        {{Education}}
                    </div>
                </div>

                <div class="section">
                    <h1 class="section-title">KỸ NĂNG & CHUYÊN MÔN</h1>
                    
                    <div class="skill-group mt-3">
                        <p class="skill-sub-label">💻 TIN HỌC</p>
                        <div class="skill-text-list">
                            {{Skills}}
                        </div>
                    </div>

                    <div class="skill-group mt-3">
                        <p class="skill-sub-label">🌍 NGOẠI NGỮ</p>
                        <div class="skill-text-list">
                            {{Languages}}
                        </div>
                    </div>

                    <div class="skill-group mt-3">
                        <p class="skill-sub-label">🎨 KỸ NĂNG KHÁC</p>
                        <div class="skill-text-list">
                            {{OtherSkills}}
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-right">
                <div class="section">
                    <h1 class="section-title">LIÊN HỆ</h1>
                    <div class="contact-list">
                        <div class="contact-item"><span>📞</span> {{Phone}}</div>
                        <div class="contact-item"><span>✉️</span> {{Email}}</div>
                        <div class="contact-item"><span>📍</span> {{Address}}</div>
                    </div>
                </div>

                <div class="section">
                    <h1 class="section-title">KINH NGHIỆM LÀM VIỆC</h1>
                    <div class="timeline">
                        {{Experience}}
                    </div>
                </div>
                
                <div class="section">
                    <h1 class="section-title">GIẢI THƯỞNG</h1>
                    <div class="content-text">{{Awards}}</div>
                </div>
            </div>
        </div>
        
        <div class="cv-footer">
            <div class="deco-star star-footer">✦</div>
        </div>
    </div>',

    N'/* Layout & Colors */
    .pink-cv-container {
        width: 210mm;
        min-height: 297mm;
        padding: 60px;
        background: #fff;
        position: relative;
        font-family: "Segoe UI", Tahoma, Geneva, Verdana, sans-serif;
        color: #333;
        box-sizing: border-box;
    }

    /* Header & Circle Avatar */
    .cv-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 40px; }
    .fullname { font-size: 52px; font-family: "Georgia", serif; font-weight: bold; margin: 0; color: #111; line-height: 1.1; }
    .job-title { font-size: 15px; text-transform: uppercase; letter-spacing: 4px; margin-top: 10px; color: #555; font-weight: 600; }
    
    .header-photo { position: relative; width: 200px; height: 200px; }
    .photo-bg-circle { 
        position: absolute; top: 0; right: -10px; 
        width: 200px; height: 200px; 
        background: radial-gradient(circle, #fcdde1 0%, #f497a9 100%); 
        border-radius: 50%; 
    }
    .avatar-img { 
        position: absolute; width: 180px; height: 180px; 
        border-radius: 50%; object-fit: cover; 
        top: 10px; right: 0; z-index: 2; 
    }

    /* Column System */
    .cv-body { display: flex; gap: 50px; }
    .col-left { flex: 1.1; }
    .col-right { flex: 0.9; }

    h1.section-title, 
    h1.main-title, 
    h1.side-title {
       font-size: 18px; /* Giữ kích thước vừa phải, không được to như tên */
       font-weight: 800;
       margin-bottom: 15px;
       margin-top: 25px;
       display: block; /* Đảm bảo nó luôn nằm riêng 1 dòng */
       /* Giữ nguyên các màu sắc/border cũ của Khoa */
   }
    .skill-sub-label { font-size: 12px; font-weight: bold; color: #f497a9; margin-bottom: 8px; text-transform: uppercase; letter-spacing: 1px; }

    /* Content Lists */
    .content-text, .skill-text-list, .contact-list { font-size: 13px; line-height: 1.6; }
    .skill-text-list { white-space: pre-line; margin-bottom: 15px; padding-left: 5px; border-left: 2px solid #fcdde1; }

    /* Timeline Styling */
    .timeline { border-left: 1px dashed #f497a9; padding-left: 20px; margin-left: 5px; }
    .timeline-item { position: relative; margin-bottom: 20px; font-size: 13px; }
    .timeline-item::before { content: "✦"; position: absolute; left: -28px; color: #f497a9; font-size: 14px; background: #fff; }

    /* Skill Bars */
    .skill-item { margin-bottom: 10px; }
    .skill-bar-bg { width: 100%; height: 5px; background: #f0f0f0; border-radius: 10px; margin-top: 4px; }
    .skill-bar-fill { height: 100%; background: #f497a9; border-radius: 10px; }

    /* Decoration Sparkles */
    .deco-star { position: absolute; color: #f497a9; opacity: 0.5; }
    .star-1 { top: 30px; left: 45%; font-size: 25px; }
    .star-2 { top: 120px; right: 40px; font-size: 18px; }
    .star-footer { bottom: 50px; left: 40%; font-size: 20px; }

    .cv-footer { position: absolute; bottom: 40px; right: 60px; font-size: 11px; color: #bbb; }',

    'https://marketplace.canva.com/EAGSZ3G6wMw/2/0/1131w/canva-s%C6%A1-y%E1%BA%BFu-l%C3%BD-l%E1%BB%8Bch-chuy%C3%AAn-nghi%E1%BB%87p-hi%E1%BB%87n-%C4%91%E1%BA%A1i-n%E1%BB%AF-t%C3%ADnh-thanh-l%E1%BB%8Bch-h%E1%BB%93ng-tr%E1%BA%AFng-N3-BrRHpD_E.jpg',
    1,
	N'Marketing, Sáng tạo'
);
GO

INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive, Category)
VALUES (
    N'Mẫu CV Academic Brown - Nguyễn Minh An', 
    N'<div class="academic-cv-wrapper">
        <div class="cv-main-col">
            <div class="header-area">
                <h1 class="fullname">{{FullName}}</h1>
                <p class="job-title">{{JobTitle}}</p>
            </div>

            <div class="section">
                <h1 class="section-title">KINH NGHIỆM LÀM VIỆC</h1>
                <div class="section-content">
                    {{Experience}}
                </div>
            </div>

            <div class="section">
                <h1 class="section-title">HỌC VẤN</h1>
                <div class="section-content">
                    {{Education}}
                </div>
            </div>
        </div>

        <div class="cv-side-col">
            <div class="avatar-box">
                <img src="{{AvatarUrl}}" class="avatar-img">
            </div>

            <div class="side-section">
                <h1 class="side-title">MỤC TIÊU LÀM VIỆC</h1>
                <div class="side-content">{{Summary}}</div>
            </div>

            <div class="side-section">
                <h1 class="side-title">GIẢI THƯỞNG</h1>
                <div class="side-content">{{Awards}}</div>
            </div>

            <div class="side-section">
                <h1 class="side-title">THÔNG TIN LIÊN HỆ</h1>
                <div class="contact-info">
                    <p>Di động: {{Phone}}</p>
                    <p>Email: {{Email}}</p>
                    <p>Địa chỉ: {{Address}}</p>
                </div>
            </div>

            <div class="side-section">
                <h1 class="side-title">KỸ NĂNG & CHUYÊN MÔN</h1>
                
                <div class="skill-group">
                    <div class="skill-list-main">{{Skills}}</div>
                </div>

                <div class="skill-group-extra">
                    <p class="extra-label">💻 TIN HỌC</p>
                    <div class="extra-content">{{ComputerSkills}}</div>
                </div>

                <div class="skill-group-extra">
                    <p class="extra-label">🌍 NGOẠI NGỮ</p>
                    <div class="extra-content">{{Languages}}</div>
                </div>

                <div class="skill-group-extra">
                    <p class="extra-label">🎨 KỸ NĂNG KHÁC</p>
                    <div class="extra-content">{{OtherSkills}}</div>
                </div>
            </div>
        </div>
    </div>',

    N'/* Layout chung */
    .academic-cv-wrapper { 
        display: flex; 
        width: 210mm; 
        min-height: 297mm; 
        background: #fdf5e6; /* Màu kem nhạt */
        font-family: "Arial", sans-serif;
        box-sizing: border-box;
    }

    /* Cột chính (Trái) */
    .cv-main-col { flex: 6; padding: 60px 40px; color: #4a3728; }
    .header-area { margin-bottom: 50px; }
    .fullname { font-size: 52px; font-weight: 900; color: #5d4e46; margin: 0; text-transform: uppercase; line-height: 1; }
    .job-title { font-size: 20px; color: #8b7355; margin-top: 10px; font-weight: 500; }

    h1.section-title { 
        font-size: 18px; font-weight: 800; color: #8b7355; 
        margin-bottom: 20px; margin-top: 40px; 
        text-transform: uppercase; letter-spacing: 1px;
    }
    .section-content { font-size: 14px; line-height: 1.7; text-align: justify; white-space: pre-line; }

    /* Cột phụ (Phải) */
    .cv-side-col { flex: 4; background: #5d4e46; color: #fff; padding: 60px 30px; }
    .avatar-box { text-align: center; margin-bottom: 40px; }
    .avatar-img { width: 180px; height: 180px; border-radius: 50%; border: 8px solid rgba(255,255,255,0.1); object-fit: cover; }

    .side-section { margin-bottom: 35px; }
    h1.side-title { 
        font-size: 16px; font-weight: 700; color: #fdf5e6; 
        margin-bottom: 15px; text-transform: uppercase; 
        border-bottom: 1px solid rgba(253, 245, 230, 0.3); padding-bottom: 5px;
    }
    .side-content, .contact-info { font-size: 13px; line-height: 1.6; color: #e8e8e8; white-space: pre-line; }

    /* Skill Group Extra */
    .skill-group-extra { margin-top: 15px; }
    .extra-label { font-size: 12px; font-weight: bold; color: #fdf5e6; margin-bottom: 5px; }
    .extra-content { font-size: 12px; color: #ddd; line-height: 1.4; padding-left: 5px; }

    /* Chống tràn văn bản */
    * { box-sizing: border-box; }
    .academic-cv-wrapper * { word-wrap: break-word; overflow-wrap: break-word; }',

    'https://careers.langmaster.edu.vn/storage/images/2023/05/11/mau-cv-dep-25.webp',
    1,
	N'Sáng tạo, Khác'
);
GO

INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive, Category)
VALUES (
    N'Professional Green - Đinh Xuân Thảo', 
    N'<div class="cv-thao-wrapper">
    <div class="cv-header">
        <div class="header-content">
            <div class="avatar-box">
                <img src="{{AvatarUrl}}" class="avatar-img">
            </div>
            <div class="title-box">
                <h1 class="fullname">{{FullName}}</h1>
                <h2 class="job-title">{{JobTitle}}</h2>
            </div>
        </div>
    </div>
    
    <div class="cv-body">
        <div class="cv-sidebar">
            <div class="sidebar-section">
                <h3 class="side-title">LIÊN HỆ</h3>
                <div class="side-content">
                    <p><i class="fas fa-phone"></i> {{Phone}}</p>
                    <p><i class="fas fa-envelope"></i> {{Email}}</p>
                    <p><i class="fas fa-map-marker-alt"></i> {{Address}}</p>
                </div>
            </div>

            <div class="sidebar-section">
                <h3 class="side-title">KỸ NĂNG</h3>
                <div class="side-content">
                    <div class="skill-main-block">{{Skills}}</div>
                    <div class="other-skill-header">KỸ NĂNG KHÁC</div>
                    <div class="other-skill-content">
                        {{OtherSkills}}
                    </div>
                </div>
            </div>
            <div class="sidebar-section">
                <h3 class="side-title">CHỨNG CHỈ</h3>
                <div class="side-content">{{Certifications}}</div>
            </div>
            
            <div class="sidebar-section">
                <h3 class="side-title">NGOẠI NGỮ</h3>
                <div class="side-content">{{Languages}}</div>
            </div>
        </div>

        <div class="cv-main">
            <div class="main-section">
                <h3 class="main-title">GIỚI THIỆU</h3>
                <div class="main-content">{{Summary}}</div>
            </div>

            <div class="main-section">
                <h3 class="main-title">KINH NGHIỆM LÀM VIỆC</h3>
                <div class="main-content">{{Experience}}</div>
            </div>

            <div class="main-section">
                <h3 class="main-title">HỌC VẤN</h3>
                <div class="main-content">{{Education}}</div>
            </div>
        </div>
    </div>
</div>',

    N'/* RESET TUYỆT ĐỐI */
* { margin: 0; padding: 0; box-sizing: border-box; }

.cv-thao-wrapper { 
    width: 210mm; min-height: 297mm; background: white; 
    font-family: "Segoe UI", sans-serif; line-height: 1.2; /* Siết độ giãn dòng cực thấp */
}

/* Header & Avatar */
.cv-header { background-color: #2c5a4b; height: 140px; display: flex; align-items: center; position: relative; }
.header-content { display: flex; align-items: center; padding-left: 50px; width: 100%; }
.avatar-box { margin-top: 55px; z-index: 10; }
.avatar-img { width: 170px; height: 170px; border-radius: 50%; border: 5px solid white; object-fit: cover; }
.title-box { margin-left: 25px; margin-top: 15px; color: white; }
.fullname { font-size: 32px; font-weight: 800; text-transform: uppercase; }
.job-title { font-size: 15px; opacity: 0.9; letter-spacing: 2px; text-transform: uppercase; }

/* Layout Body */
.cv-body { display: flex; padding: 35px 40px 20px 40px; }
.cv-sidebar { flex: 3.5; padding-right: 20px; border-right: 1px solid #f2f2f2; }
.cv-main { flex: 6.5; padding-left: 30px; }

/* Sidebar Sections */
.sidebar-section { margin-bottom: 12px; }
.side-title { 
    background: #2c5a4b; color: white; font-size: 12px; font-weight: bold; 
    padding: 4px 15px; border-radius: 0 20px 20px 0; margin-left: -40px; 
    display: inline-block; margin-bottom: 5px;
}

/* --- XỬ LÝ TRIỆT ĐỂ KHOẢNG TRẮNG NỘI DUNG --- */
.side-content, .main-content {
    font-size: 12px;
    color: #444;
    white-space: pre-line;
}

/* Ép tất cả các thẻ con (p, li, div) không được có margin dưới */
.side-content p, .main-content p,
.side-content li, .main-content li,
.side-content div, .main-content div {
    margin-bottom: 2px !important; /* Chỉ để lại 2px cho thoáng, không để trống */
    padding: 0 !important;
    line-height: 1.3 !important;
}

.side-content ul, .main-content ul {
    margin-left: 15px;
    margin-bottom: 2px;
}

/* Kỹ năng khác */
.other-skill-header { 
    font-size: 11px; font-weight: 800; color: #2c5a4b; 
    margin-top: 5px; margin-bottom: 2px; border-top: 1px solid #eee; padding-top: 4px;
}
.other-skill-content { font-size: 11px; font-style: italic; color: #555; }

/* Main Sections */
.main-section { margin-bottom: 12px; }
.main-title { 
    color: #2c5a4b; font-size: 15px; font-weight: 800; 
    border-left: 4px solid #2c5a4b; padding-left: 10px; 
    margin-bottom: 5px; text-transform: uppercase;
}

/* Chống tràn */
p, div, h1, h2, h3, li { word-break: break-word; overflow-wrap: break-word; }',

    'https://static.vietcv.io/image/vng/confidential/c4c87802f4cadd024416c22a12314e88.png?_=1645622482',
    1,
	N'Kinh tế, Marketing'
);
GO

INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive, Category)
VALUES (
    N'Trần Mạnh Dũng', 
    N'<div class="cv-template-6">
    <div class="cv-left">
        <div class="avatar-box">
            <img src="{{AvatarUrl}}" alt="Avatar">
        </div>
        <div class="profile-header">
            <h1 class="fullname">{{FullName}}</h1>
            <h2 class="jobtitle">{{JobTitle}}</h2>
        </div>
        
        <div class="left-divider"></div>
        
        <div class="contact-info">
            <div class="contact-item"><i class="fas fa-phone-alt"></i><span>{{Phone}}</span></div>
            <div class="contact-item"><i class="fas fa-calendar-alt"></i><span>{{BirthDate}}</span></div>
            <div class="contact-item"><i class="fas fa-envelope"></i><span>{{Email}}</span></div>
            <div class="contact-item"><i class="fas fa-map-marker-alt"></i><span>{{Address}}</span></div>
        </div>

        <div class="left-section">
            <h3 class="left-title"><span>Học vấn</span></h3>
            <div class="left-content edu-list">
                {{Education}}
            </div>
        </div>

        <div class="left-section">
            <h3 class="left-title"><span>Kỹ năng</span></h3>
            <div class="left-content skill-list">
                {{OtherSkills}}
            </div>
        </div>

        <div class="left-section">
            <h3 class="left-title"><span>Tin học</span></h3>
            <div class="left-content skill-list">
                {{Skills}}
            </div>
        </div>

        <div class="left-section">
            <h3 class="left-title"><span>Ngoại ngữ</span></h3>
            <div class="left-content skill-list">
                {{Languages}}
            </div>
        </div>

        <div class="left-section">
            <h3 class="left-title"><span>Sở thích</span></h3>
            <div class="left-content skill-list">
                {{Hobbies}}
            </div>
        </div>
    </div>

    <div class="cv-right">
        <div class="right-section">
            <h3 class="right-title"><span>Mục tiêu nghề nghiệp</span></h3>
            <div class="right-content summary-text">
                {{Summary}}
            </div>
        </div>

        <div class="right-section">
            <h3 class="right-title"><span>Kinh nghiệm làm việc</span></h3>
            <div class="right-content experience-list">
                {{Experience}}
            </div>
        </div>

        <div class="right-section">
            <h3 class="right-title"><span>Hoạt động</span></h3>
            <div class="right-content activity-list">
                {{Activities}}
            </div>
        </div>

        <div class="right-section">
            <h3 class="right-title"><span>Danh hiệu và giải thưởng</span></h3>
            <div class="right-content award-list">
                {{Awards}}
            </div>
        </div>

        <div class="right-section">
            <h3 class="right-title"><span>Chứng chỉ</span></h3>
            <div class="right-content cert-list">
                {{Certifications}}
            </div>
        </div>
        
        <div class="right-section">
            <h3 class="right-title"><span>Người tham chiếu</span></h3>
            <div class="right-content reference-list">
                {{References}}
            </div>
        </div>
    </div>
</div>
',

    N'@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');

:root {
    --primary-color: #574040;
    --text-main: #333;
    --text-light: #555;
    --bg-left: var(--primary-color);
    --bg-right: #ffffff;
}

/* KHUNG A4 CỐ ĐỊNH */
.cv-template-6 {
    display: flex;
    width: 210mm;
    max-width: 210mm;
    min-height: 297mm;
    background-color: var(--bg-right);
    font-family: 'Inter', sans-serif;
    color: var(--text-main);
    line-height: 1.4;
    box-sizing: border-box;
    margin: 0 auto;
    overflow: hidden; 
}

/* CHỐNG TRÀN CHỮ TUYỆT ĐỐI (FIX LỖI AAAAAA) */
.cv-template-6 * {
    box-sizing: border-box;
    word-wrap: break-word;
    overflow-wrap: anywhere; 
    word-break: break-word;
}

/* SIDEBAR TRÁI - GIỮ ĐỘ RỘNG 38% THEO Ý KHOA */
.cv-template-6 .cv-left {
    width: 38%; 
    background-color: var(--bg-left);
    color: #fff;
    padding: 35px 25px;
    display: flex;
    flex-direction: column;
    min-width: 38%; 
}

.cv-template-6 .avatar-box {
    text-align: center;
    margin-bottom: 20px;
}

.cv-template-6 .avatar-box img {
    width: 160px;
    height: 160px;
    border-radius: 50%;
    object-fit: cover;
    border: 4px solid rgba(255,255,255,0.1);
}

.cv-template-6 .fullname {
    font-size: 24px;
    font-weight: 700;
    margin: 0 0 5px 0;
    text-align: center;
    line-height: 1.2;
}

.cv-template-6 .jobtitle {
    font-size: 15px;
    text-align: center;
    margin-bottom: 25px;
    color: rgba(255, 255, 255, 0.9);
}

/* THÔNG TIN LIÊN HỆ - FIX LỖI DÍNH ICON */
.cv-template-6 .contact-info {
    margin-bottom: 25px;
}

.cv-template-6 .contact-item {
    display: flex;
    margin-bottom: 12px; /* Tăng khoảng cách giữa các dòng cho thoáng */
    font-size: 13.5px;
    align-items: center; /* Căn giữa icon và text theo chiều dọc */
    gap: 12px; /* TẠO KHOẢNG CÁCH GIỮA ICON VÀ TEXT */
}

.cv-template-6 .contact-item i {
    width: 20px; /* Khóa độ rộng icon để text luôn thẳng hàng dọc */
    text-align: center;
    font-size: 16px;
    flex-shrink: 0; /* Không cho icon bị bóp méo khi text dài */
    color: rgba(255, 255, 255, 0.8);
}

/* CÁC PHẦN BÊN TRÁI */
.cv-template-6 .left-section {
    margin-bottom: 15px; 
}

.cv-template-6 .left-title {
    margin-bottom: 10px !important;
}

.cv-template-6 .left-title span {
    display: inline-block;
    background-color: rgba(255, 255, 255, 0.15);
    padding: 6px 18px;
    border-radius: 20px;
    font-size: 13px;
    font-weight: 600;
    text-transform: uppercase;
}

.cv-template-6 .left-content {
    font-size: 13px;
    line-height: 1.4;
}

.cv-template-6 .left-content li {
    margin-bottom: 5px !important;
}

/* CỘT PHẢI */
.cv-template-6 .cv-right {
    width: 62%;
    background-color: var(--bg-right);
    padding: 40px 30px;
    min-width: 62%;
}

.cv-template-6 .right-section {
    margin-bottom: 20px; 
}

.cv-template-6 .right-title {
    display: flex;
    align-items: center;
    margin-bottom: 12px;
}

.cv-template-6 .right-title span {
    background-color: var(--primary-color);
    color: #fff;
    padding: 7px 20px;
    border-radius: 20px;
    font-size: 14.5px;
    font-weight: 600;
    text-transform: uppercase;
}

.cv-template-6 .right-title::after {
    content: "";
    flex-grow: 1;
    height: 1px;
    background-color: var(--primary-color);
    margin-left: 12px;
    opacity: 0.2;
}

/* TRIỆT TIÊU KHOẢNG TRỐNG THỪA TỪ AI */
.cv-template-6 p, .cv-template-6 ul, .cv-template-6 li {
    margin-top: 0 !important;
    margin-bottom: 3px !important; 
}

/* KHI IN PDF */
@media print {
    @page { size: A4; margin: 0; }
    body { margin: 0; padding: 0; }
    .cv-template-6 {
        width: 210mm;
        height: 297mm;
        margin: 0;
        box-shadow: none;
    }
}',

    '/images/templates/template_b5b27380.webp',
    1,
	N'IT, Kinh tế'
);
GO

INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive, Category)
VALUES (
    N'Lê Chiến', 
    N'<div class="cv-elegant-wrapper">
        <div class="header-area">
            <div class="avatar-box">
                <img src="{{AvatarUrl}}" class="avatar-img">
            </div>
            <div class="header-content">
                <h1 class="fullname">{{FullName}}</h1>
                <p class="job-title">{{JobTitle}}</p>
                <div class="summary-box">{{Summary}}</div>
            </div>
        </div>
        <div class="middle-grid">
            <div class="grid-col">
                <h3 class="section-title">THÔNG TIN CÁ NHÂN</h3>
                <ul class="contact-list">
                    <li><i class="fas fa-calendar-alt"></i> <span>{{BirthDate}}</span></li>
                    <li><i class="fas fa-envelope"></i> <span>{{Email}}</span></li>
                    <li><i class="fas fa-phone-alt"></i> <span>{{Phone}}</span></li>
                    <li><i class="fas fa-globe"></i> <span>{{Website}}</span></li>
                    <li><i class="fas fa-map-marker-alt"></i> <span>{{Address}}</span></li>
                </ul>
            </div>
            <div class="grid-col">
                <h3 class="section-title">HỌC VẤN</h3>
                <div class="content-text">{{Education}}</div>
            </div>
            <div class="grid-col">
                <h3 class="section-title">CHỨNG CHỈ</h3>
                <div class="content-text">{{Certifications}}</div>
            </div>
        </div>
        <div class="main-section">
            <h3 class="section-title">KINH NGHIỆM LÀM VIỆC</h3>
            <div class="timeline-container">{{Experience}}</div>
        </div>
        <!-- Khối DỰ ÁN được bổ sung để tận dụng hết form -->
        <div class="main-section">
            <h3 class="section-title">DỰ ÁN NỔI BẬT</h3>
            <div class="timeline-container">{{Projects}}</div>
        </div>
        <div class="footer-grid">
            <div class="footer-left">
                <h3 class="section-title">KỸ NĂNG</h3>
                <div class="content-text">
                  <div class="skill-group">
                    <p class="skill-type">💻 TIN HỌC</p>
                     {{Skills}}
                  </div>
        
                  <div class="skill-group">
                  <p class="skill-type">🌍 NGOẠI NGỮ</p>
                     {{Languages}}
                  </div>
        
                   <div class="skill-group">
                   <p class="skill-type">💡 KỸ NĂNG KHÁC</p>
                       {{OtherSkills}}
                   </div>
                </div>
                <div class="sub-section">
                    <h3 class="section-title">SỞ THÍCH</h3>
                    <div class="content-text">{{Hobbies}}</div>
                </div>
            </div>
            <div class="footer-right">
                <div class="sub-section">
                    <h3 class="section-title">DANH HIỆU & GIẢI THƯỞNG</h3>
                    <div class="content-text">{{Awards}}</div>
                </div>
                <div class="sub-section">
                    <h3 class="section-title">HOẠT ĐỘNG</h3>
                    <div class="timeline-container small-timeline">{{Activities}}</div>
                </div>
                <div class="sub-section">
                    <h3 class="section-title">NGƯỜI GIỚI THIỆU</h3>
                    <div class="content-text">{{References}}</div>
                </div>
            </div>
        </div>
    </div>',

    N'@import url("https://fonts.googleapis.com/css2?family=Be+Vietnam+Pro:wght@400;500;600;700&display=swap");
    /* CẤU TRÚC A4 CỐ ĐỊNH */
    .cv-elegant-wrapper { 
        width: 100%; min-height: 297mm; background: #fff; padding: 45px 50px; 
        font-family: "Be Vietnam Pro", sans-serif; color: #333; margin: 0 auto; 
        box-sizing: border-box; overflow: hidden; border-bottom: 5px solid #2e8b57;
    }
    .cv-elegant-wrapper * { box-sizing: border-box; word-wrap: break-word; word-break: break-word; }
    /* HEADER */
    .header-area { display: flex; gap: 40px; margin-bottom: 25px; }
    .avatar-img { width: 145px; height: 145px; border-radius: 50%; object-fit: cover; border: 1px solid #ddd; padding: 3px; }
    .header-content { flex: 1; display: flex; flex-direction: column; justify-content: center; }
    .fullname { font-size: 30px; color: #a40000; font-weight: 700; text-transform: uppercase; margin: 0 0 5px 0; letter-spacing: 0.5px; }
    .job-title { font-size: 16px; color: #222; font-weight: 500; margin: 0 0 12px 0; border-bottom: 2px solid #222; padding-bottom: 12px; display: inline-block; width: 100%; }
    .summary-box { font-size: 13.5px; line-height: 1.6; text-align: justify; color: #333; }
    /* TIÊU ĐỀ RED SECTION */
    .section-title { 
        font-size: 14.5px; font-weight: 700; color: #a40000; 
        border-bottom: 2px solid #a40000; padding-bottom: 6px; 
        margin: 0 0 15px 0 !important; text-transform: uppercase; letter-spacing: 0.5px;
    }
    .main-section { margin-bottom: 30px; }
    .main-section:empty { display: none; }
    /* 3 COL GRID (Thông tin, Học Vấn, Chứng chỉ) */
    .middle-grid { display: grid; grid-template-columns: 1.3fr 1fr 1fr; gap: 25px; margin-bottom: 30px; }
    
    /* CONTACT LIST */
    .contact-list { list-style: none; padding: 0; }
    .contact-list li { margin-bottom: 10px; display: flex; gap: 12px; align-items: center; font-size: 13px; color: #333; }
    .contact-list i { background: #a40000; color: white !important; width: 22px; height: 22px; border-radius: 4px; display: flex; align-items: center; justify-content: center; font-size: 11px; flex-shrink: 0; }
    /* GRID COL (Học vấn & Chứng chỉ) CSS Ghi đè cấu trúc C# */
    .grid-col .exp-item { display: flex; flex-direction: column; margin-bottom: 15px; }
    .grid-col .exp-year { order: 2; font-size: 12.5px; color: #555; margin-top: 4px; }
    .grid-col .exp-content { order: 1; font-size: 12.5px; line-height: 1.5; color: #444; }
    .grid-col .exp-content strong { font-size: 13.5px; text-transform: uppercase; color: #111; font-weight: 700; }
    
    .grid-col .content-text > div > div:first-child { color: #111 !important; font-size: 13.5px !important; margin-bottom: 2px; font-weight: bold; }
    .grid-col .content-text > div > div:nth-child(2) { color: #555 !important; font-size: 12.5px !important; text-transform: uppercase; line-height: 1.4; }
    /* -------------------------------------
       TIMELINE TRUNG TÂM (Trải nghiệm, Dự án) 
       Sử dụng sức mạnh CSS Absolute
       để tách 1 Row HTML thành 2 Cột !!
    -------------------------------------- */
    .timeline-container .exp-item {
        position: relative; padding-left: 30%; margin-bottom: 25px; min-height: 60px;
    }
    .timeline-container .exp-item::before {
        content: ""; position: absolute; left: 30%; top: 6px; width: 2px; height: calc(100% + 20px); background: #ccc;
    }
    .timeline-container .exp-item:last-child::before { display: none; }
    
    .timeline-container .exp-item::after {
        content: ""; position: absolute; left: calc(30% - 4px); top: 6px; width: 10px; height: 10px; border-radius: 50%; background: #a40000;
    }
    
    .timeline-container .exp-year {
        position: absolute; left: 0; top: 3px; width: 28%; font-weight: bold; font-size: 13.5px; color: #222; margin-left: -5px; /* Giấu nhẹ dấu chấm do C# sinh ra */
    }
    .timeline-container .exp-content { padding-left: 20px; }
    
    .timeline-container .info-line:first-child {
        position: absolute; left: 0; top: 24px; width: 28%; font-weight: bold; font-size: 13.5px; color: #111;
    }
    .timeline-container .info-line:first-child strong { display: none; }
    
    .timeline-container .info-line:nth-child(2) {
        font-weight: bold; font-size: 14.5px; color: #000; margin-bottom: 6px;
    }
    .timeline-container .info-line:nth-child(2) strong { display: none; }
    
    .timeline-container .desc-text { font-size: 13px; line-height: 1.6; color: #333; text-align: justify; }
    /* FOOTER 2 CỘT */
    .footer-grid { display: flex; gap: 50px; }
    .footer-left { flex: 4; }
    .footer-right { flex: 6; }
    .sub-section { margin-bottom: 25px; }
    /* SMALL TIMELINE CHO HOẠT ĐỘNG (Dồn lại 1 cột) */
    .small-timeline .exp-item { padding-left: 18px; margin-bottom: 15px; }
    .small-timeline .exp-item::before { left: 0; }
    .small-timeline .exp-item::after { left: -4px; width: 10px; height: 10px; top: 6px; }
    .small-timeline .exp-year { position: static; width: auto; font-weight: bold; font-size: 12.5px; margin-bottom: 3px; display: block; }
    .small-timeline .exp-content { padding-left: 0; }
    .small-timeline .info-line:first-child { position: static; width: auto; font-size: 14px; font-weight: bold; text-transform: uppercase; margin-bottom: 2px; }
    .small-timeline .info-line:nth-child(2) { font-size: 13.5px; color: #555; margin-bottom: 6px; font-weight: normal; }
    /* Danh sách kỹ năng, sở thích */
    .content-text ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
    .content-text li { font-size: 13px; color: #333; padding: 6px 0; display: flex; line-height: 1.4; border-bottom: 1px dashed #e0e0e0; margin: 0 !important;}
    .content-text li:last-child { border-bottom: none; }
    /* References */
    .content-text p { font-size: 13px; font-style: italic; color: #555; margin-bottom: 5px; }
/* Tối ưu cho nhóm kỹ năng gộp */
.skill-group {
    margin-bottom: 15px; /* Khoảng cách giữa các nhóm nhỏ */
}

.skill-group:last-child {
    margin-bottom: 0;
}

.skill-type {
    font-size: 12px !important;
    font-weight: 700 !important;
    margin-bottom: 5px !important;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    padding: 2px 8px;
    display: inline-block;
    border-radius: 4px;
}

/* Đảm bảo danh sách bên dưới label không bị margin quá lớn */
.skill-group ul {
    margin-top: 2px !important;
}

.skill-group li {
    border-bottom: 1px dashed #eee; /* Đường kẻ mờ phân cách các kỹ năng lẻ */
    padding: 4px 0 !important;
}',

    'https://www.topcv.vn/cv/snapshot/template-cv-position/mau-cv-lap-trinh-vien-mau-thanh-lich-Xl5SXVReBF0VGg0PRFkVBwhSXFADDQIBAAEGA1MNWlYEV1NXUloACgQDUVABBQ1VUAEHGRYEAVIBWwBV4801.webp?t=1749574801',
    1,
	N'IT'
);
GO

INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive, Category)
VALUES (
    N'Đặng Ngọc Linh', 
    N'<div class="cv-classic-wrapper">
        <div class="header-section">
            <h1 class="fullname">{{FullName}}</h1>
            <h2 class="job-title">{{JobTitle}}</h2>
            <div class="contact-info">
                <span><i class="fas fa-phone-alt"></i> {{Phone}}</span>
                <span><i class="fas fa-envelope"></i> {{Email}}</span>
                <span><i class="fas fa-globe"></i> {{Website}}</span>
                <span><i class="fas fa-map-marker-alt"></i> {{Address}}</span>
            </div>
        </div>
        <div class="section summary-section">
            <h3 class="section-title">MỤC TIÊU NGHỀ NGHỆP</h3>
            <div class="summary-text">{{Summary}}</div>
        </div>
        <div class="section education-section">
            <h3 class="section-title">HỌC VẤN</h3>
            <div class="content-area">{{Education}}</div>
        </div>
        <div class="section exp-section">
            <h3 class="section-title">KINH NGHIỆM LÀM VIỆC</h3>
            <div class="content-area">{{Experience}}</div>
        </div>
        
        <!-- Bổ sung DỰ ÁN theo chuẩn chung -->
        <div class="section project-section">
            <h3 class="section-title">DỰ ÁN NỔI BẬT</h3>
            <div class="content-area">{{Projects}}</div>
        </div>
        <div class="section skills-section">
            <h3 class="section-title">KỸ NĂNG</h3>
            <!-- Render cả kỹ năng đặc thù (IT) và kỹ năng khác -->
            <div class="content-area">{{Skills}}</div>
            <div class="content-area">{{OtherSkills}}</div>
        </div>
        <div class="section act-section">
            <h3 class="section-title">HOẠT ĐỘNG</h3>
            <div class="content-area">{{Activities}}</div>
        </div>
        <div class="section cert-section">
            <h3 class="section-title">CHỨNG CHỈ</h3>
            <div class="content-area">{{Certifications}}</div>
        </div>
        <div class="section awards-section">
            <h3 class="section-title">DANH HIỆU & GIẢI THƯỞNG</h3>
            <div class="content-area">{{Awards}}</div>
        </div>
        <div class="section ref-section">
            <h3 class="section-title">NGƯỜI GIỚI THIỆU</h3>
            <div class="content-area">{{References}}</div>
        </div>
        <div class="section hobbies-section">
            <h3 class="section-title">SỞ THÍCH</h3>
            <div class="content-area">{{Hobbies}}</div>
        </div>
    </div>',

    N'@import url("https://fonts.googleapis.com/css2?family=Lora:ital,wght@0,400;0,600;0,700;1,400;1,600&display=swap");
    /* CẤU TRÚC A4 CỐ ĐỊNH */
    .cv-classic-wrapper { 
        width: 100%; min-height: 297mm; background: #fff; padding: 45px 50px; 
        font-family: "Lora", "Times New Roman", serif; color: #111; margin: 0 auto; 
        box-sizing: border-box; overflow: hidden; line-height: 1.6;
    }
    .cv-classic-wrapper * { box-sizing: border-box; word-wrap: break-word; word-break: break-word; }
    /* HEADER */
    .header-section { text-align: center; margin-bottom: 25px; }
    .fullname { font-size: 28px; font-weight: 700; text-transform: uppercase; margin: 0 0 5px 0; color: #000; letter-spacing: 1px; }
    .job-title { font-size: 16px; font-weight: 600; color: #222; margin: 0 0 15px 0; }
    .contact-info { display: flex; justify-content: center; flex-wrap: wrap; gap: 15px; font-size: 13.5px; color: #111; }
    .contact-info span { display: flex; align-items: center; gap: 5px; }
    /* LAYOUT SECTION CƠ BẢN */
    .section { margin-bottom: 25px; }
    .section:empty, .content-area:empty { display: none !important; }
    /* SECTION TITLE (Gạch dưới Dài Full Box) */
    .section-title {
        font-size: 16px; font-weight: 700; text-transform: uppercase; color: #000;
        border-bottom: 1.5px solid #000; padding-bottom: 6px; margin: 0 0 15px 0 !important;
    }
    /* MỤC TIÊU NGHỀ NGHỆP */
    .summary-text { font-size: 13.5px; line-height: 1.6; text-align: justify; color: #222; }
    /* -------------------------------
       CẤU TRÚC FLEX LEFT-RIGHT 
       (Học vấn, Kinh nghiệm, Dự án) 
    ------------------------------- */
    .exp-section .exp-item, .project-section .exp-item, .education-section .exp-item {
        position: relative; margin-bottom: 18px; padding-right: 130px; /* Chừa biên phải cho NĂM */
    }
    /* NĂM (Đẩy sang cực phải) */
    .exp-year {
        display: block; position: absolute; right: 0; top: 0; width: 130px; text-align: right; 
        font-size: 14.5px; color: #333; font-weight: normal; margin-top: 1px;
    }
    /* Loại bỏ dấu chấm tròn tự động sinh ra tử JS */
    .exp-year::first-letter { font-size: 0; color: transparent; }
    /* TÊN CÔNG TY (Thẻ khối đầu tiên) */
    .exp-content .info-line:first-child { font-size: 15px; font-weight: 700; color: #000; margin-bottom: 3px; }
    .exp-content .info-line:first-child strong { display: none; }
    /* TÊN VỊ TRÍ (Thẻ khối số 2) */
    .exp-content .info-line:nth-child(2) { font-size: 14.5px; font-weight: 700; color: #111; margin-bottom: 6px; }
    .exp-content .info-line:nth-child(2) strong { display: none; }
    /* MÔ TẢ (Desc text dạng list/câu dài) */
    .desc-text { font-size: 13.5px; line-height: 1.6; text-align: justify; color: #222; }
    .desc-text ul { padding-left: 20px; }
    /* HỌC VẤN (Override style vì khác tag) */
    .education-section .exp-content strong { font-size: 15px; font-weight: 700; color: #000; display: block; margin-bottom: 3px; }
    .education-section .exp-content { font-size: 14px; line-height: 1.6; }
    /* -------------------------------
       CÁC PHẦN ĐƠN GIẢN (Kỹ năng, Hoạt động, Chứng chỉ) 
    ------------------------------- */
    /* KỸ NĂNG: Gạch kẻ chân dưới */
    .skills-section ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
    .skills-section li { padding: 8px 0; border-bottom: 1px dashed #ccc; font-size: 14px; }
    .skills-section li:last-child { border-bottom: none; }
    .skills-section li::first-letter { font-size: 0; color: transparent; }
    /* HOẠT ĐỘNG: Flexbox Header Trái Phải */
    .act-section .exp-header { display: flex; justify-content: space-between; margin-bottom: 6px; }
    .act-section .company-name { font-weight: 700; font-size: 15px; color: #000; }
    .act-section .date-badge { font-size: 14px; color: #333; }
    .act-section .exp-desc { font-size: 13.5px; line-height: 1.6; text-align: justify; }
    /* CHỨNG CHỈ (Flex đổi vị trí Tên ở Trái - Năm ở Phải) */
    /* Phải selector qua > div để target đúng vào cấu trúc render của C# */
    .cert-section .content-area > div { display: flex; justify-content: space-between; border-bottom: 1px dashed #e0e0e0; padding-bottom: 8px; margin-bottom: 12px !important; }
    .cert-section .content-area > div:last-child { border-bottom: none; }
    .cert-section .content-area > div > div:first-child { order: 2; font-size: 14px !important; font-weight: normal !important; color: #333 !important; }
    .cert-section .content-area > div > div:nth-child(2) { order: 1; font-size: 14.5px !important; font-weight: 700 !important; color: #000 !important; }
    /* DANH HIỆU & GIẢI THƯỞNG */
    .awards-section ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
    .awards-section li { padding: 6px 0; font-size: 14px; border-bottom: 1px dashed #e0e0e0; }
    .awards-section li:last-child { border-bottom: none; }
    .awards-section li::first-letter { font-size: 0; color: transparent; }
    /* NGƯỜI GIỚI THIỆU: Gắn gọn inline text */
    .ref-section p { font-size: 14px !important; margin-bottom: 8px !important; line-height: 1.5; color: #222; }
    .ref-section p::first-letter { font-size: 0; color: transparent; }
    /* SỞ THÍCH: Giao diện Inline như các tag */
    .hobbies-section ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
    .hobbies-section li { display: inline-block; margin: 0 15px 10px 0 !important; font-size: 14px; }
    .hobbies-section li::first-letter { font-size: 0; color: transparent; }',

    'https://www.topcv.vn/cv/snapshot/template-cv/mau-cv-senior-_B1tfBFEAUgALBAgBBl0MD1wECQBQAQFQAwIAAw44ec.webp?t=1756265781&color=000000&template_name=senior_v2&lang=vi',
    1,
	N'IT, Kinh tế'
);
GO
INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive, Category)
VALUES (
    N'Nguyễn Võ Lê Khoa', 
    N'<div class="cv-modern-wrapper">
        <div class="cv-sidebar">
            <div class="avatar-area">
                <img src="{{AvatarUrl}}" class="avatar-img">
            </div>
            
            <div class="sidebar-info">
                <div class="info-row"><i class="fas fa-phone-alt"></i> <span>{{Phone}}</span></div>
                <div class="info-row"><i class="fas fa-envelope"></i> <span>{{Email}}</span></div>
                <div class="info-row"><i class="fas fa-globe"></i> <span>{{Website}}</span></div>
                <div class="info-row"><i class="fas fa-map-marker-alt"></i> <span>{{Address}}</span></div>
            </div>
            <div class="side-section">
                <h3 class="side-title">KỸ NĂNG</h3>
                <div class="side-content">{{OtherSkills}}</div>
            </div>
            <div class="side-section">
                <h3 class="side-title">TIN HỌC</h3>
                <div class="side-content">{{Skills}}</div>
            </div>
            <div class="side-section">
                <h3 class="side-title">CHỨNG CHỈ</h3>
                <div class="side-content">{{Certifications}}</div>
            </div>
            <div class="side-section">
                <h3 class="side-title">GIẢI THƯỞNG</h3>
                <div class="side-content">{{Awards}}</div>
            </div>
            <div class="side-section">
                <h3 class="side-title">SỞ THÍCH</h3>
                <div class="side-content">{{Hobbies}}</div>
            </div>
        </div>
        <div class="cv-main">
            <div class="dark-header">
                <h1 class="name">{{FullName}}</h1>
                <h2 class="title">{{JobTitle}}</h2>
                <div class="summary">{{Summary}}</div>
            </div>
            <div class="main-body">
                <div class="main-section">
                    <h3 class="main-title">HỌC VẤN</h3>
                    <div class="content-list">{{Education}}</div>
                </div>
                <div class="main-section">
                    <h3 class="main-title">KINH NGHIỆM LÀM VIỆC</h3>
                    <div class="content-list">
                        {{Experience}} 
                    </div>
                </div>
                <div class="main-section">
                    <h3 class="main-title">DỰ ÁN</h3>
                    <div class="content-list">
                        {{Projects}}
                    </div>
                </div>
                <div class="main-section">
                    <h3 class="main-title">HOẠT ĐỘNG</h3>
                    <div class="content-list">
                        {{Activities}}
                    </div>
                </div>
            </div>
        </div>
    </div>',

    N'/* CẤU TRÚC TỔNG THỂ */
    @import url(''https://fonts.googleapis.com/css2?family=Inter:wght@400;600;800&display=swap'');
    .cv-modern-wrapper { display: flex; width: 100%; height: 100%; min-height: 297mm; background: white; font-family: "Inter", sans-serif; overflow: hidden; }
    .cv-modern-wrapper * { box-sizing: border-box; word-wrap: break-word; word-break: break-word; }
    /* SIDEBAR (38%) */
    .cv-sidebar { width: 38%; background: #eae6db; padding: 40px 25px; display: flex; flex-direction: column; border-right: 1px solid #dcd8cf; }
    .avatar-area { text-align: center; margin-bottom: 25px; }
    .avatar-img { width: 180px; height: 180px; border-radius: 50%; object-fit: cover; border: 4px solid white; box-shadow: 0 4px 10px rgba(0,0,0,0.1); }
    
    .sidebar-info { border-top: 1px solid #d4cfc4; border-bottom: 1px solid #d4cfc4; padding: 20px 0; margin-bottom: 25px; }
    .info-row { display: flex; align-items: center; justify-content: flex-start; gap: 12px; font-size: 13px; color: #222; margin-bottom: 12px; line-height: 1.4; display: flex; }
    .info-row:last-child { margin-bottom: 0; }
    .info-row i { width: 28px; height: 28px; background: #dedad0; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 12px; color: #4b5247; border: 1px solid #d0cbbc; flex-shrink: 0; }
    .side-title { font-size: 16px; font-weight: 800; color: #222; margin: 20px 0 12px 0 !important; text-transform: uppercase; border-bottom: 1.5px solid #d4cfc4; padding-bottom: 6px; letter-spacing: 0.5px; }
    .side-content { font-size: 13.5px; color: #333; line-height: 1.5; }
    .side-content p, .side-content li { margin: 0 0 8px 0 !important; }
    .side-content ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
    .side-content li { display: flex; justify-content: space-between; align-items: baseline; }
    .side-content li > span:first-child { font-weight: 600; color: #222; flex-grow: 1; }
    .side-content li > span:nth-child(2) { font-style: italic; color: #555; font-size: 11.5px; }
    /* Fix awards and hobbies */
    .award-list ul, .hobby-list ul { padding-left: 15px !important; }
    /* MAIN CONTENT (62%) */
    .cv-main { width: 62%; display: flex; flex-direction: column; background: #ffffff; }
    
    /* DARK HEADER */
    .dark-header { background: #5d6657; color: #f4f0e8; padding: 45px 35px; }
    .name { font-size: 38px; font-weight: 800; text-transform: uppercase; margin: 0 0 5px 0; line-height: 1.2; letter-spacing: 1px; color: #fff;}
    .title { font-size: 17px; font-weight: 600; margin: 0 0 15px 0; letter-spacing: 1.5px; border-bottom: 1px solid rgba(255,255,255,0.3); padding-bottom: 12px; display: inline-block; text-transform: uppercase;}
    .summary { font-size: 14px; line-height: 1.6; text-align: justify; opacity: 0.9; margin-top: 5px; }
    /* BODY */
    .main-body { padding: 30px 40px; }
    .main-section { margin-bottom: 25px; }
    /* Ẩn các khối trống */
    .main-section:empty, .content-list:empty { display: none !important; }
    .main-title { display: flex; align-items: center; font-size: 18px; font-weight: 800; color: #333; margin: 0 0 18px 0 !important; letter-spacing: 0.5px; text-transform: uppercase; }
    .main-title::after { content: ""; flex-grow: 1; height: 1.5px; background: #e0e0e0; margin-left: 15px; }
    
    /* CONTENT LIST & DYNAMIC ITEMS */
    .content-list { font-size: 14px; line-height: 1.5; color: #333; }
    
    .exp-item { position: relative; margin-bottom: 25px; }
    .exp-year { position: absolute; right: 0; top: -3px; background: #5d6657; color: white; padding: 4px 15px; border-radius: 20px; font-size: 12px; font-weight: 700; white-space: nowrap; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
    
    .exp-content { padding-right: 140px; display: flex; flex-direction: column; }
    .info-line:first-child { font-size: 15px; font-weight: 800; color: #222; margin-bottom: 4px; }
    .info-line:nth-child(2) { font-size: 14px; color: #555; margin-bottom: 6px; font-style: italic; }
    .desc-text { font-size: 13px; color: #444; line-height: 1.6; text-align: justify; }
    .desc-text ul { padding-left: 18px; margin-top: 5px; margin-bottom: 0; }
    .desc-text p { margin-bottom: 5px; }',

    '/images/templates/template_5119c646.png',
    1,
	N'IT, Sáng tạo'
);
GO

INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive, Category)
VALUES (
    N'Modern Professional Split', 
    N'<div class="cv-dual-wrapper">
        <div class="left-sidebar">
            <div class="avatar-container">
                <img class="avatar-img" src="{{AvatarUrl}}">
            </div>
            
            <div class="sidebar-padding">
                <div class="contact-box">
                    <ul>
                        <li><i class="fas fa-phone"></i> <span>{{Phone}}</span></li>
                        <li><i class="fas fa-envelope"></i> <span>{{Email}}</span></li>
                        <li><i class="fas fa-map-marker-alt"></i> <span>{{Address}}</span></li>
                        <li><i class="fas fa-link"></i> <span>{{Website}}</span></li>
                    </ul>
                </div>
                
                <div class="side-divider"></div>
                
                <div class="side-summary">{{Summary}}</div>
                
                <div class="side-divider"></div>

                <div class="side-section">
                    <h3 class="side-title">Kỹ năng</h3>
                    <div class="skills-list">{{Skills}}</div>
                    <div class="skills-list">{{OtherSkills}}</div>
                </div>

                <div class="side-divider"></div>

                <div class="side-section">
                    <h3 class="side-title">Chứng chỉ</h3>
                    <!-- CSS sẽ lật ngược Năm lên trên, Tên chứng chỉ xuống dưới -->
                    <div class="side-cert">{{Certifications}}</div>
                </div>

                <div class="side-divider"></div>

                <div class="side-section">
                    <h3 class="side-title">Sở thích</h3>
                    <div class="skills-list">{{Hobbies}}</div>
                </div>
            </div>
        </div>

        <div class="right-main">
            <div class="header-box">
                <h1 class="fullname">{{FullName}}</h1>
                <h2 class="job-title">{{JobTitle}}</h2>
            </div>

            <div class="main-content">
                <div class="content-section education-section">
                    <h3 class="main-title">Học vấn</h3>
                    <div class="content-area">{{Education}}</div>
                </div>

                <div class="content-section act-section">
                    <h3 class="main-title">Hoạt động</h3>
                    <div class="content-area">{{Activities}}</div>
                </div>

                <div class="content-section project-section">
                    <h3 class="main-title">Dự án</h3>
                    <div class="content-area">{{Projects}}</div>
                </div>

                <div class="content-section exp-section">
                    <h3 class="main-title">Kinh nghiệm làm việc</h3>
                    <div class="content-area">{{Experience}}</div>
                </div>
                
                <div class="content-section awards-section">
                    <h3 class="main-title">Giải thưởng & Tham chiếu</h3>
                    <div class="content-area">{{Awards}}</div>
                    <div class="content-area ref-block">{{References}}</div>
                </div>
            </div>
        </div>
    </div>',

    N'@import url("https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap");

    /* CẤU TRÚC A4 CỐ ĐỊNH SPLIT VIEW */
    .cv-dual-wrapper {
        display: flex; width: 100%; min-height: 297mm; background: #fff;
        font-family: "Inter", sans-serif; margin: 0 auto; box-sizing: border-box; overflow: hidden;
    }
    .cv-dual-wrapper * { box-sizing: border-box; word-wrap: break-word; }

    /* LAYOUT 2 CỘT */
    .left-sidebar { width: 33.5%; background: #5B626B; color: #fff; display: flex; flex-direction: column; }
    .right-main { width: 66.5%; background: #fff; display: flex; flex-direction: column; padding-bottom: 40px; }

    /* AVATAR & HEADER ĐỒNG BỘ CHIỀU CAO */
    .avatar-container { width: 100%; height: 215px; flex-shrink: 0; }
    .avatar-img { width: 100%; height: 100%; object-fit: cover; }
    .header-box { width: 100%; height: 215px; background: #B3BBC5; display: flex; flex-direction: column; justify-content: center; padding-left: 45px; flex-shrink: 0; }

    .fullname { font-size: 32px; font-weight: 700; color: #111; margin: 0 0 8px 0; letter-spacing: -0.5px; }
    .job-title { font-size: 16.5px; font-weight: 500; color: #333; margin: 0; }

    /* PADDING NỘI DUNG */
    .sidebar-padding { padding: 35px 30px; }
    .main-content { padding: 35px 45px; }

    /* ----- CỘT TRÁI (LEFT SIDEBAR) ----- */
    .contact-box ul { list-style: none; padding: 0; margin: 0; }
    .contact-box li { display: flex; align-items: flex-start; gap: 12px; margin-bottom: 12px; font-size: 12.5px; line-height: 1.5; }
    .contact-box i { width: 16px; text-align: center; font-size: 13px; margin-top: 3px; opacity: 0.9; }
    .side-divider { width: 100%; border-bottom: 1px solid rgba(255, 255, 255, 0.25); margin: 22px 0; }
    .side-summary { font-size: 13px; line-height: 1.6; text-align: justify; }

    .side-title { font-size: 16px; font-weight: 700; text-transform: uppercase; margin-bottom: 15px; letter-spacing: 0.5px; color: #fff; }
    
    .skills-list ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
    .skills-list li { margin-bottom: 10px; font-size: 13px; line-height: 1.5; color: #fff; display: flex; }
    .skills-list li::first-letter { font-size: 0; color: transparent; }

    .side-cert > div { display: flex; flex-direction: column; margin-bottom: 15px !important; }
    .side-cert .cert-year-div { order: 1; font-size: 12.5px !important; font-weight: 600 !important; color: #9AADC3 !important; margin-bottom: 4px; text-transform: uppercase; }
    .side-cert .cert-name-div { order: 2; font-size: 13px !important; font-weight: 400 !important; color: #fff !important; line-height: 1.5; }

    /* ----- CỘT PHẢI (RIGHT MAIN) ----- */
    .content-section { margin-bottom: 25px; }
    .content-section:empty, .content-area:empty { display: none !important; }
    .main-title {
        background: #88929B; color: #fff; padding: 9px 18px; font-size: 15.5px; font-weight: 600; text-transform: uppercase;
        margin: 0 0 20px 0 !important; display: block; width: 100%; letter-spacing: 0.5px;
    }

    /* Ẩn chữ Công ty: Vị trí: */
    .right-main .info-line strong { display: none; }
    .right-main .exp-year::first-letter { font-size: 0; color: transparent; } /* Ẩn Bullet • */

    /* MAGIC CSS THAY ĐỔI VỊ TRÍ (KINH NGHIỆM, DỰ ÁN) */
    .exp-section .exp-item, .project-section .exp-item { display: flex; flex-direction: column; margin-bottom: 22px; }
    .exp-section .exp-content, .project-section .exp-content { display: contents; } /* Giải nén hộp con */
    
    .exp-section .info-line:first-child, .project-section .info-line:first-child {
        order: 1; font-weight: 700; font-size: 15px; color: #111; margin-bottom: 4px; text-transform: uppercase;
    }
    .exp-section .info-line:nth-child(2), .project-section .info-line:nth-child(2) {
        order: 2; font-size: 14px; color: #5B626B; margin-bottom: 4px;
    }
    .exp-section .exp-year, .project-section .exp-year {
        order: 3; font-size: 13.5px; color: #666; margin-bottom: 10px; font-weight: 500;
    }
    .exp-section .desc-text, .project-section .desc-text {
        order: 4; font-size: 13.5px; line-height: 1.6; color: #333; text-align: justify;
    }

    /* SẮP XẾP VỊ TRÍ HOẠT ĐỘNG (Activity Header structure) */
    .act-section .exp-item { display: flex; flex-direction: column; margin-bottom: 22px; }
    .act-section .exp-header { display: contents; }
    .act-section .company-name { order: 1; font-weight: 700; font-size: 15px; color: #111; margin-bottom: 4px; }
    .act-section .date-badge { order: 2; font-size: 13.5px; color: #666; margin-bottom: 10px; font-weight: 500; }
    .act-section .exp-desc { order: 3; font-size: 13.5px; line-height: 1.6; color: #333; text-align: justify; }
    /* Fix bullet khoảng trắng cho ul li */
    .desc-text ul { padding-left: 20px; }

    /* SẮP XẾP VỊ TRÍ HỌC VẤN */
    .education-section .exp-item { display: flex; flex-direction: column; margin-bottom: 20px; }
    .education-section .exp-content { order: 1; font-weight: 400; color: #5B626B; font-size: 14px; line-height: 1.6; }
    .education-section .exp-content strong { color: #111; font-size: 15px; font-weight: 700; display: block; margin-bottom: 2px; }
    .education-section .exp-year { order: 2; color: #666; font-size: 13.5px; margin-top: 4px; font-weight: 500; }

    /* GIẢI THƯỞNG & THAM CHIẾU */
    .awards-section ul { list-style: none !important; padding: 0 !important; margin: 0 0 10px 0 !important; }
    .awards-section li { padding: 4px 0; font-size: 13.5px; color: #333; }
    .awards-section li::first-letter { font-size: 0; color: transparent; }

    .ref-block p { font-size: 13.5px !important; margin-bottom: 8px !important; line-height: 1.5; color: #333; }
    .ref-block p::first-letter { font-size: 0; color: transparent; }',

    '/images/templates/modern_split.jpg',
    1,
	N'IT, Marketing'
);
GO

INSERT INTO Templates (Name, HtmlContent, CssContent, PreviewImageUrl, IsActive, Category)
VALUES (
    N'Pastel Beige Blocks', 
    N'<div class="cv-pastel-wrapper">
        <!-- Block 1: Contact -->
        <div class="pastel-block contact-block">
            <div class="contact-item"><i class="fas fa-phone-alt"></i> <span>{{Phone}}</span></div>
            <div class="contact-item"><i class="fas fa-envelope"></i> <span>{{Email}}</span></div>
            <div class="contact-item"><i class="fas fa-globe"></i> <span>{{Website}}</span></div>
            <div class="contact-item"><i class="fas fa-map-marker-alt"></i> <span>{{Address}}</span></div>
        </div>

        <!-- Block 2: Profile -->
        <div class="pastel-block profile-block">
            <div class="profile-left">
                <h1 class="fullname">{{FullName}}</h1>
                <div class="job-title-wrapper">
                    <span class="job-title">{{JobTitle}}</span>
                    <span class="title-line"></span>
                </div>
                <div class="summary-text">{{Summary}}</div>
            </div>
            <div class="profile-right">
                <img src="{{AvatarUrl}}" class="avatar-img">
            </div>
        </div>

        <!-- Block 3: Education & Certs -->
        <div class="pastel-block">
            <div class="section-group education-group">
                <h3 class="section-title">Education</h3>
                <div class="title-line-full"></div>
                <div class="content-area">{{Education}}</div>
            </div>

            <div class="section-group cert-group">
                <h3 class="section-title">Certifications</h3>
                <div class="title-line-full"></div>
                <div class="content-area">{{Certifications}}</div>
            </div>
        </div>

        <!-- Block 4: Projects & Experience -->
        <div class="pastel-block project-group">
            <h3 class="section-title">Projects</h3>
            <div class="title-line-full"></div>
            <div class="timeline-area">{{Projects}}</div>
            
            <!-- Tận dụng không gian cho Kinh nghiệm làm việc dùng chung form timeline -->
            <div style="margin-top: 15px;"></div>
            <h3 class="section-title">Experience</h3>
            <div class="title-line-full"></div>
            <div class="timeline-area">{{Experience}}</div>
        </div>

        <!-- Block 5: Activities -->
        <div class="pastel-block act-group">
            <h3 class="section-title">Activities</h3>
            <div class="title-line-full"></div>
            <!-- Class riêng act-area do HTML sinh ra từ C# cho phần này khác với Projects -->
            <div class="act-area">{{Activities}}</div>
        </div>

        <!-- Block 6: Skills -->
        <div class="pastel-block skill-group">
            <h3 class="section-title">Skills</h3>
            <div class="title-line-full"></div>
            <div class="content-area">{{Skills}}</div>
            <div class="content-area">{{OtherSkills}}</div>
        </div>

        <!-- Block 7: Bottom Split -->
        <div class="pastel-block bottom-split">
            <div class="bottom-left hobbies-group">
                <h3 class="section-title">Interests</h3>
                <div class="title-line-full"></div>
                <div class="content-area">{{Hobbies}}</div>
            </div>
            <div class="bottom-right">
                <div class="awards-group">
                    <h3 class="section-title">Additional Info</h3>
                    <div class="title-line-full"></div>
                    <div class="content-area">{{Awards}}</div>
                    <div class="ref-group">{{References}}</div>
                </div>
            </div>
        </div>
    </div>',

    N'@import url("https://fonts.googleapis.com/css2?family=Segoe+UI:wght@400;600;700&display=swap");

    /* TỔNG THỂ */
    .cv-pastel-wrapper { 
        width: 100%; min-height: 297mm; background: #FFF; padding: 25px 35px; 
        font-family: "Segoe UI", Helvetica, Arial, sans-serif; box-sizing: border-box; overflow: hidden;
    }
    .cv-pastel-wrapper * { box-sizing: border-box; word-wrap: break-word; }

    /* KHỐI PASTEL */
    .pastel-block { background: #EFECE9; padding: 25px 30px; border-radius: 6px; margin-bottom: 20px; }

    /* KHỐI LIÊN HỆ DỌC THEO HÀNG */
    .contact-block { padding: 16px 30px; display: flex; justify-content: center; flex-wrap: wrap; gap: 35px; margin-bottom: 20px; }
    .contact-item { display: flex; align-items: center; gap: 8px; font-size: 11.5px; font-weight: 600; color: #111; }
    .contact-item i { color: #D6624B; font-size: 14px; }

    /* HỒ SƠ CÁ NHÂN */
    .profile-block { display: flex; gap: 40px; align-items: center; }
    .profile-left { flex: 1; display: flex; flex-direction: column; }
    
    .fullname { font-size: 26px; font-weight: 700; color: #5C322E; margin: 0 0 10px 0; letter-spacing: 0.5px; }
    .job-title-wrapper { display: flex; align-items: center; gap: 15px; margin-bottom: 12px; }
    .job-title { font-size: 14.5px; font-weight: 700; text-transform: uppercase; color: #111; }
    .title-line { flex: 1; max-width: 140px; height: 2px; background: #D6624B; }
    
    .summary-text { font-size: 12.5px; line-height: 1.6; text-align: justify; color: #222; }

    /* HIỆU ỨNG ẢNH ĐAI CAM CẮT GÓC */
    .profile-right { width: 135px; flex-shrink: 0; position: relative; padding-top: 10px; padding-left: 10px; }
    .profile-right::before { content: ""; position: absolute; left: 0; top: 0; width: 80px; height: 80px; background: #D6624B; z-index: 1; border-radius: 2px; }
    .avatar-img { position: relative; z-index: 2; width: 135px; height: 160px; object-fit: cover; border-radius: 4px; box-shadow: -2px 2px 10px rgba(0,0,0,0.1); }

    /* TIÊU ĐỀ RED SECTION */
    .section-title { font-size: 15.5px; font-weight: 700; color: #5C322E; margin: 0 0 6px 0 !important; }
    .title-line-full { width: 100%; height: 1.5px; background: #D6624B; margin-bottom: 18px; }

    /* HỌC VẤN (Cấu trúc Flat Text) */
    .education-group { margin-bottom: 30px; }
    .education-group .exp-item { display: flex; flex-direction: column; margin-bottom: 15px; }
    .education-group .exp-content { order: 1; font-size: 12.5px; color: #111; line-height: 1.6; }
    .education-group .exp-content strong { font-weight: 700; font-size: 13.5px; display: block; margin-bottom: 2px; }
    .education-group .exp-year { order: 2; font-weight: 700; font-size: 12.5px; margin-top: 4px; color: #111; }
    .education-group .exp-year::first-letter { font-size: 0; color: transparent; }

    /* CHỨNG CHỈ */
    .cert-group > div > div { margin-bottom: 15px !important; }
    .cert-group .cert-year-div { font-size: 12.5px !important; font-weight: 700 !important; color: #333 !important; margin-bottom: 3px !important; }
    .cert-group .cert-name-div { font-size: 13px !important; font-weight: 400 !important; color: #111 !important; line-height: 1.4; }

    /* TIMELINE DỰ ÁN & KINH NGHIỆM LÀM VIỆC (Lướt Grid tạo thành 4 phân vùng) */
    .timeline-area .exp-item {
        display: grid; grid-template-columns: 35% 65%; gap: 0; row-gap: 5px;
        position: relative; padding-left: 20px; margin-bottom: 28px;
    }
    /* Thanh dọc */
    .timeline-area .exp-item::before { content: ""; position: absolute; left: 4px; top: 12px; width: 1.5px; height: calc(100% + 15px); background: #C5BDBA; }
    .timeline-area .exp-item:last-child::before { display: none; }
    /* Chấm cam */
    .timeline-area .exp-item::after { content: ""; position: absolute; left: 0px; top: 10px; width: 9px; height: 9px; border-radius: 50%; background: #D6624B; }
    
    .timeline-area .exp-content { display: contents; } /* Gỡ bỏ bọc hộp */

    /* Định vị Grid */
    .timeline-area .exp-year { grid-column: 1; grid-row: 1; font-size: 12.5px; font-weight: 700; color: #444; margin-top: 5px; }
    .timeline-area .exp-year::first-letter { font-size: 0; color: transparent; }

    .timeline-area .info-line:nth-child(2) { grid-column: 2; grid-row: 1; font-size: 13px; font-weight: 700; color: #111; margin-top: 5px; }
    .timeline-area .info-line:nth-child(2) strong { display: none; }

    .timeline-area .info-line:first-child { grid-column: 1; grid-row: 2; font-size: 13px; font-weight: 700; color: #000; padding-right: 15px; }
    .timeline-area .info-line:first-child strong { display: none; }

    .timeline-area .desc-text { grid-column: 2; grid-row: 2; font-size: 12.5px; line-height: 1.6; color: #222; text-align: justify; }
    .desc-text ul { padding-left: 20px; }

    /* TIMELINE HOẠT ĐỘNG (Activity Header structure) */
    .act-area .exp-item { display: grid; grid-template-columns: 35% 65%; gap: 0; row-gap: 5px; position: relative; padding-left: 20px; margin-bottom: 25px; }
    .act-area .exp-item::before { content: ""; position: absolute; left: 4px; top: 12px; width: 1.5px; height: calc(100% + 15px); background: #C5BDBA; }
    .act-area .exp-item:last-child::before { display: none; }
    .act-area .exp-item::after { content: ""; position: absolute; left: 0px; top: 10px; width: 9px; height: 9px; border-radius: 50%; background: #D6624B; }
    .act-area .exp-header { display: contents; }

    .act-area .date-badge { grid-column: 1; grid-row: 1; font-size: 12.5px; font-weight: 700; color: #444; margin-top: 5px; }
    .act-area .company-name { grid-column: 1; grid-row: 2; font-size: 13px; font-weight: 700; color: #000; padding-right: 15px; }
    .act-area .exp-desc { grid-column: 2; grid-row: 1 / span 2; font-size: 12.5px; line-height: 1.6; color: #222; text-align: justify; margin-top: 5px; }

    /* KỸ NĂNG */
    .skill-group ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
    .skill-group li { font-size: 13px; line-height: 1.6; margin-bottom: 10px; color: #111; }
    .skill-group li::first-letter { font-size: 0; color: transparent; }

    /* KHỐI BOTTOM SPLIT (Giải thưởng & Liên hệ & Sở thích) */
    .bottom-split { display: grid; grid-template-columns: 1fr 1fr; gap: 40px; }
    
    .awards-group ul { list-style: none !important; padding: 0 !important; margin: 0 0 10px 0 !important; }
    .awards-group li { padding: 4px 0; font-size: 12.5px; color: #222; }
    .awards-group li::first-letter { font-size: 0; color: transparent; }

    .ref-group p { font-size: 12.5px !important; margin-bottom: 8px !important; line-height: 1.6; color: #222; }
    .ref-group p::first-letter { font-size: 0; color: transparent; }

    .hobbies-group ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
    .hobbies-group li { font-size: 12.5px; color: #222; display: inline-block; margin-right: 15px !important; margin-bottom: 6px; }
    .hobbies-group li::first-letter { font-size: 0; color: transparent; }',

    '/images/templates/pastel_blocks.jpg',
    1,
	N'IT, Sáng tạo'
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



