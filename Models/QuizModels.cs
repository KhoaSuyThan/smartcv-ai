using System;
using System.Collections.Generic;

namespace DoAnCS.Models
{
    /// <summary>
    /// DTO đại diện cho một câu hỏi trắc nghiệm do AI sinh ra
    /// </summary>
    public class QuizQuestionDto
    {
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
        public string CorrectAnswer { get; set; } = string.Empty; // A, B, C, D
        public string Explanation { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
    }

    /// <summary>
    /// Metadata đóng gói giải thích và chủ đề câu hỏi lưu vào cột Feedback của InterviewMessages
    /// Định dạng JSON có version (v: 1) giúp tái sử dụng cấu trúc DB hiện tại mà không cần sửa schema
    /// </summary>
    public class QuizFeedbackMetadata
    {
        public int V { get; set; } = 1;
        public string Topic { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel cho trang làm bài thi trắc nghiệm PracticeQuiz.cshtml
    /// Bảo vệ đáp án: Khi Status == 0 (đang làm bài), CorrectAnswer và Explanation hoàn toàn bằng NULL
    /// </summary>
    public class PracticeQuizViewModel
    {
        public int SessionID { get; set; }
        public string ResumeTitle { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public int Status { get; set; } // 0: Đang làm, 1: Đã hoàn thành
        public int? OverallScore { get; set; }
        public string? AiEvaluation { get; set; }
        public string? AiProvider { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? Deadline { get; set; } // Hạn chót nộp bài từ server
        public int TotalTimeSeconds { get; set; } // Đề xuất: 90s/câu
        public int RemainingSeconds { get; set; } // Số giây còn lại thực tế tính từ server
        public SkillDiagnosticDto? SkillDiagnostic { get; set; } // Chẩn đoán kỹ năng CV vs JD
        public QuizResultSummaryDto? ResultSummary { get; set; } // Thống kê chi tiết & tư vấn nhóm lỗi Giai đoạn 4
        public List<PracticeQuizItemViewModel> Questions { get; set; } = new List<PracticeQuizItemViewModel>();
    }

    /// <summary>
    /// GIAI ĐOẠN 4: DTO Thống kê toàn diện và tư vấn ôn tập cá nhân hóa sau khi nộp bài
    /// </summary>
    public class QuizResultSummaryDto
    {
        public int TotalQuestions { get; set; }
        public int AnsweredCount { get; set; }
        public int SkippedCount { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public int ScorePercentage { get; set; }
        public int DurationSeconds { get; set; }
        public string DurationFormatted { get; set; } = "00:00";
        public bool IsOverdue { get; set; }
        public List<QuizTopicStatDto> TopicStats { get; set; } = new List<QuizTopicStatDto>();
        public List<string> Strengths { get; set; } = new List<string>();
        public List<string> Weaknesses { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    /// <summary>
    /// Thống kê tỷ lệ chính xác cho từng chủ đề / chuyên ngành kỹ thuật
    /// </summary>
    public class QuizTopicStatDto
    {
        public string Topic { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Correct { get; set; }
        public int Percentage => Total > 0 ? (int)Math.Round((double)Correct / Total * 100) : 0;
        public bool IsMastered => Percentage >= 75;
    }

    /// <summary>
    /// DTO chứa thông tin chẩn đoán kỹ năng giữa hồ sơ ứng viên và tin tuyển dụng
    /// </summary>
    public class SkillDiagnosticDto
    {
        public string TargetMode { get; set; } = "cv"; // "cv" hoặc "jd"
        public List<string> MatchedSkills { get; set; } = new List<string>();
        public List<string> GapSkills { get; set; } = new List<string>(); // Kỹ năng JD yêu cầu nhưng chưa thấy trong CV
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel cho từng câu hỏi hiển thị trên giao diện
    /// </summary>
    public class PracticeQuizItemViewModel
    {
        public int MessageID { get; set; }
        public int QuestionIndex { get; set; }
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
        public string? CandidateAnswer { get; set; }
        
        // CHỈ CÓ DỮ LIỆU KHI Status == 1 (Đã nộp bài). Tuyệt đối không gửi xuống Client khi Status == 0.
        public string? CorrectAnswer { get; set; }
        public string? Explanation { get; set; }
        public string? Topic { get; set; }
        public bool? IsCorrect { get; set; }
    }
}
