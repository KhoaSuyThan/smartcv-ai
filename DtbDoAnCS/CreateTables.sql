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

-- 1. Thông tin công ty
CREATE TABLE Companies (
    CompanyID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) NOT NULL,
    LogoUrl NVARCHAR(500),
    Website NVARCHAR(255),
    Description NVARCHAR(MAX),
    Address NVARCHAR(500),
    Industry NVARCHAR(100), -- Ngành nghề (IT, Marketing,...)
    TaxCode NVARCHAR(MAX),
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
    [GroqApiKey] nvarchar(max) NULL,
    [ModelName] nvarchar(max) NOT NULL DEFAULT 'gemini-2.5-flash',
    [Temperature] float NOT NULL DEFAULT 0.7,
    [MaxOutputTokens] int NOT NULL DEFAULT 2048,
    [SystemInstruction] nvarchar(max) NULL,
    [ChatbotSystemInstruction] nvarchar(max) NULL,
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
	TopCandidatesCount INT NOT NULL DEFAULT 6, -- Số ứng viên tối đa Smart Match gửi vào LLM phân tích
    CONSTRAINT [PK_GeminiConfigs] PRIMARY KEY ([Id])
);

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
	ProExpirationDate DATETIME NULL,
	PasswordChangeToken NVARCHAR(MAX) NULL,
	PasswordChangeTokenExpires DATETIME2 NULL,
	PendingPasswordHash NVARCHAR(MAX) NULL,
	Summary NVARCHAR(MAX) NULL,
	Skills NVARCHAR(MAX) NULL,
	PasswordResetOTP NVARCHAR(6) NULL,
    OTPExpires DATETIME NULL,
    OTPFailCount INT DEFAULT 0,
    ProfessionalTitle NVARCHAR(MAX) NULL,
    PortfolioLinks NVARCHAR(MAX) NULL,
    YearsOfExperience NVARCHAR(MAX) NULL,
    Address NVARCHAR(MAX) NULL,
    ExpectedLocation NVARCHAR(MAX) NULL,
    ExpectedSalary INT NULL,
    LastLoginTime BIGINT NULL, -- Timestamp đăng nhập mới nhất (Ticks), dùng kiểm tra phiên đa thiết bị
    CONSTRAINT FK_Users_Companies FOREIGN KEY (CompanyID) REFERENCES Companies(CompanyID) ON DELETE SET NULL
);

-- 7. BẢNG CV CHÍNH (Chứa thông tin cá nhân "tĩnh" - Khớp Editor)
CREATE TABLE Resumes (
    ResumeID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    TemplateID INT NULL,
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

	IsPublic BIT NOT NULL DEFAULT 0,
    Slug NVARCHAR(255) NULL,
    ViewCount INT NOT NULL DEFAULT 0,
	FileUploadUrl NVARCHAR(MAX) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),

	JsonContent NVARCHAR(MAX) NULL,

    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
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

-- 15. Cập nhật yêu cầu
CREATE TABLE [UpgradeRequests] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [UserID]           INT            NOT NULL,
    [RequestDate]      DATETIME       NOT NULL DEFAULT (GETDATE()),
    [Status]           INT            NOT NULL DEFAULT (0),
    [EvidenceImageUrl] NVARCHAR (MAX) NULL,
    [Notes]            NVARCHAR (MAX) NULL,
    [DecisionDate]     DATETIME       NULL,
	TransactionId nvarchar(255) NULL, --Lưu Momo
    PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Upgrade_User] FOREIGN KEY ([UserID]) REFERENCES [Users]([UserID])
);

-- 16. Lưu mẫu CV đã làm
CREATE TABLE SavedCandidates (
    Id INT PRIMARY KEY IDENTITY(1,1),
    RecruiterId INT NOT NULL,
    ResumeId INT NOT NULL,
    SavedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_SavedCandidates_Recruiter FOREIGN KEY (RecruiterId) REFERENCES Users(UserID),
    CONSTRAINT FK_SavedCandidates_Resume FOREIGN KEY (ResumeId) REFERENCES Resumes(ResumeID)
);

-- 17. Test templates dùng Vue
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

-- 18. Kết quả Smart CV Matcher (AI so khớp CV với Job Description)
CREATE TABLE CVMatchResults (
    Id INT PRIMARY KEY IDENTITY(1,1),
    JobID INT NOT NULL,
    ResumeID INT NOT NULL,
    MatchScore INT NOT NULL DEFAULT 0,
    MatchedSkills NVARCHAR(MAX) NULL,
    MissingSkills NVARCHAR(MAX) NULL,
    Suggestions NVARCHAR(MAX) NULL,
    Strengths NVARCHAR(MAX) NULL,
    Summary NVARCHAR(MAX) NULL,
    Recommendation NVARCHAR(100) NULL,
    AnalyzedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_CVMatchResults_Jobs FOREIGN KEY (JobID) REFERENCES Jobs(JobID) ON DELETE CASCADE,
    CONSTRAINT FK_CVMatchResults_Resumes FOREIGN KEY (ResumeID) REFERENCES Resumes(ResumeID) ON DELETE CASCADE
);

-- 19. Lưu Vector nhúng (Text Embeddings) phục vụ RAG
CREATE TABLE CVEmbeddings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ResumeID INT NOT NULL,
    VectorJson NVARCHAR(MAX) NOT NULL DEFAULT '[]',
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_CVEmbeddings_Resumes FOREIGN KEY (ResumeID) REFERENCES Resumes(ResumeID) ON DELETE CASCADE
);

-- 20. Đánh giá trang web (1-5 Sao)
CREATE TABLE SiteFeedbacks (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    Rating FLOAT NOT NULL,
    Comment NVARCHAR(MAX) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_SiteFeedbacks_Users FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);

-- 21. Chỉ mục tìm kiếm (Database Indexes) để tối ưu hóa hiệu năng
CREATE INDEX IX_Resumes_IsPublic ON Resumes(IsPublic);
CREATE INDEX IX_Resumes_UserID ON Resumes(UserID);
CREATE INDEX IX_Jobs_Status ON Jobs(Status);
CREATE INDEX IX_AILogs_UserID_CreatedAt ON AILogs(UserID, CreatedAt);
CREATE INDEX IX_CVEmbeddings_ResumeID ON CVEmbeddings(ResumeID);

-- 22. Quản lý lịch sử kiểm thử tự động (Automation Testing History)
CREATE TABLE TestRuns (
    TestRunID INT PRIMARY KEY IDENTITY(1,1),
    ExecutionTime DATETIME DEFAULT GETDATE(),
    SuiteName NVARCHAR(100) NOT NULL, -- 'System Health Check' hoặc 'E2E Flow Testing'
    TotalCases INT NOT NULL,
    PassedCases INT NOT NULL,
    FailedCases INT NOT NULL,
    AvgResponseTimeMs BIGINT NOT NULL
);
-- 23.
CREATE TABLE TestCaseDetails (
    TestCaseID INT PRIMARY KEY IDENTITY(1,1),
    TestRunID INT NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    Method NVARCHAR(50) NOT NULL,
    Url NVARCHAR(500) NULL,
    Status NVARCHAR(50) NOT NULL, -- 'Success' hoặc 'Failed'
    ResponseTimeMs BIGINT NOT NULL,
    ExpectedResult NVARCHAR(MAX) NULL,
    ActualResult NVARCHAR(MAX) NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    CONSTRAINT FK_TestCaseDetails_TestRuns FOREIGN KEY (TestRunID) REFERENCES TestRuns(TestRunID) ON DELETE CASCADE
);

-- 24. Các bước của kịch bản kiểm thử động (Dynamic Test Steps)
CREATE TABLE TestSteps (
    StepID INT IDENTITY(1,1) PRIMARY KEY,
    ScenarioName NVARCHAR(50) NOT NULL, -- e.g. 'Auth E2E' hoặc 'Jobs E2E'
    StepOrder INT NOT NULL,              -- Thứ tự chạy của bước
    ActionType NVARCHAR(20) NOT NULL,    -- Navigate, Fill, Click, Select, AssertUrl, AssertText
    TargetSelector NVARCHAR(250) NULL,   -- CSS selector để thao tác
    Value NVARCHAR(MAX) NULL,            -- Giá trị truyền vào hoặc so khớp
    Description NVARCHAR(500) NULL       -- Mô tả bước kiểm thử
);