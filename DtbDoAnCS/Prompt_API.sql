USE DoAnWebCS;
GO

INSERT INTO [dbo].[GeminiConfigs] (
    [Id], 
    [ApiKey], 
    [ModelName], 
    [Temperature], 
    [MaxOutputTokens], 
    [SystemInstruction], 
    [UserRateLimit], 
    [TotalTokensUsed],
    [ProModelName], 
    [ProTemperature], 
    [ProMaxOutputTokens], 
    [ProUserRateLimit],
    [SkillTemplate], 
    [SummaryTemplate], 
    [GrammarTemplate]
)
VALUES (
    1, 
    N'Tự thêm API', 
    N'gemini-2.5-flash', 
    0.7, 
    2048, 
    N'Bạn là trợ lý ảo hỗ trợ đánh giá CV chuyên nghiệp.', 
    10, 
    0,
    N'gemini-2.5-pro', 
    0.9, 
    4096, 
    50,
    -- 3 Template prompt cho Gemini AI (được gộp trực tiếp từ các lệnh UPDATE cũ)
    N'Liệt kê đúng 5 kỹ năng quan trọng nhất cho vị trí {{content}}. YÊU CẦU BẮT BUỘC: Chỉ trả về tên các kỹ năng, ngăn cách nhau bằng duy nhất dấu phẩy. KHÔNG đánh số, KHÔNG lời dẫn, KHÔNG giải thích.',
    N'Viết duy nhất một đoạn văn mục tiêu nghề nghiệp (3-4 câu) cho vị trí {{context}} dựa trên các ý: {{content}}. YÊU CẦU BẮT BUỘC: Chỉ trả về nội dung đoạn văn. KHÔNG lời chào, KHÔNG tiêu đề, KHÔNG giải thích thêm.',
    N'Viết duy nhất một đoạn văn mô tả công việc (2-3 câu) sau cho vị trí {{context}} theo chuẩn STAR: {{content}}. YÊU CẦU BẮT BUỘC: Chỉ trả về các gạch đầu dòng nội dung. TUYỆT ĐỐI KHÔNG có lời dẫn, không có câu ''Dưới đây là...'', không tiêu đề.'
);
GO