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
-- Nạp Kỹ năng IT
INSERT INTO Skills (SkillName) VALUES ('.NET'), ('SQL Server'), ('C#'), ('Flutter'), ('React');
GO

-- Test

