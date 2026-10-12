# BẢNG THEO DÕI SỬ DỤNG TRÍ TUỆ NHÂN TẠO (AI USAGE LOG)

Bảng này được lập nhằm tuân thủ quy định học thuật và hướng dẫn của giảng viên hướng dẫn về việc minh bạch sử dụng các công cụ Trí tuệ Nhân tạo (Generative AI) trong quá trình nghiên cứu và phát triển đề tài tốt nghiệp SmartCV.

> **Quy định học thuật:**
> 1. Được phép sử dụng AI hỗ trợ: tìm hiểu khái niệm, brainstorm ý tưởng, hỗ trợ viết/sửa code, debug, sinh dữ liệu mẫu/test case, viết tài liệu.
> 2. Không được “copy AI rồi nộp”: Sinh viên phải hiểu rõ bản chất code, luồng input/output, kiến trúc hệ thống và chịu trách nhiệm 100% về mã nguồn.
> 3. Bắt buộc ghi chép đầy đủ vào bảng nhật ký bên dưới mỗi khi sử dụng AI.

---

## Nhật ký chi tiết (AI Usage Log Table)

| Thời gian | AI | Nội dung yêu cầu | AI tạo ra | Sinh viên xử lý |
| :--- | :--- | :--- | :--- | :--- |
| 12/10/2026 | Antigravity (Gemini) | Xử lý nút "Lưu ứng viên" cho hồ sơ Benchmark AI không can thiệp Database SQL Server | Module lưu trữ JSON độc lập `Data/SavedBenchmark/`, action `ToggleSaveCandidate`, đồng bộ `isSaved` trên SmartMatch và hiển thị trên `/Talent` | Kiểm tra biên dịch C# thành công (0 Warning, 0 Error), xác nhận an toàn tuyệt đối khóa ngoại Foreign Key và kiểm thử luồng ghi/xóa JSON |
| 12/10/2026 | Antigravity (Gemini) | Nối động cơ Hybrid Matcher vào C# Web, hỗ trợ xem PDF ứng viên benchmark không cần tài khoản DB | Code C# `SmartMatchController.cs` (AnalyzeJob, ViewCandidatePdf), giao diện `Index.cshtml` | Kiểm tra biên dịch C# (0 Warning, 0 Error), chạy script test so khớp thực tế Top 5 ứng viên (474 ms) và xác nhận 100% tệp PDF tồn tại |
| 11/10/2026 | Antigravity (Gemini) | Triển khai động cơ Hybrid 2 vòng: Sinh Vector Cache 3.320 CV và module chấm điểm ATS | File npy ma trận 3320x384 (4.86 MB), `ats_scorer.py`, 2 endpoint FastAPI | Chạy test tự động xác nhận latency Vòng 1 (54.8 ms), Vòng 2 (89.5 ms), tổng thời gian 144 ms (< 0.15s) |
| 11/10/2026 | Antigravity (Gemini) | Thiết kế kiến trúc so khớp Hybrid 2 vòng và kế hoạch kỹ thuật chi tiết | File `IMPLEMENTATION_PLAN.md` (Sơ đồ 2 vòng, công thức điểm ATS, phân bổ vai trò 4 kỹ sư) | Đọc hiểu cơ chế 2-Stage Retrieval, duyệt công thức tính điểm và nghiệm thu kế hoạch |
| 11/10/2026 | Antigravity (Gemini) | Xây dựng và chuẩn hóa tập dữ liệu 35 tin tuyển dụng (JD) phục vụ đối chứng NCKH | File `Clean_Job_Descriptions.json` (35 JD, 66 KB, 6 nhóm ngành, song ngữ) | Kiểm tra tính duy nhất ID, xác thực cấu trúc trường kỹ năng và độ tương thích vector 384-dim |
| 11/10/2026 | Antigravity (Gemini) | Đóng gói API trích xuất PDF/che PII và kết nối luồng upload CV trên C# Web | Module `pii_extractor.py`, 2 endpoint FastAPI và tích hợp C# trong ResumeController / SmartMatchController | Kiểm tra kết quả test tự động (bóc tách 508 từ, che 3 PII, vector 384-dim), xác nhận dotnet build 0 error |
| 11/10/2026 | Antigravity (Gemini) | Bóc tách toàn bộ 3.320 CV sang JSON, tối ưu phân 2 cột, che PII và dọn dẹp file tạm | File `Clean_Anonymized_CVs.json` (18.45 MB), thuật toán phân cột ngang và kịch bản bóc tách batch | Kiểm tra chất lượng trích xuất (96.2%), xác thực khử lỗi ngắt dòng email, phê duyệt xóa các file test tạm thời |
| 11/10/2026 | Antigravity (Gemini) | Sửa lỗi terminal `dotnet run` không hiện link localhost sau khi chạy `npm run build` | Giải thích cơ chế dev server vs build tĩnh; đoạn cấu hình `Microsoft.Hosting.Lifetime` trong Serilog | Đọc hiểu cơ chế Serilog Override, chạy test `npm run dev` và khởi động lại Back-end |
| 10/10/2026 | Antigravity (Gemini) | Trực quan hóa sơ đồ khối kiến trúc hệ thống và quy trình NCKH cho đề tài | Sơ đồ Mermaid 5 khối xử lý và bảng thuyết minh kỹ thuật | Review tính phù hợp với cấu trúc microservice thực tế, nhúng vào `ROADMAP.md` |
| 09/10/2026 | Antigravity (Gemini) | Tự động hóa kiểm thử E2E kịch bản Validation đăng ký/đăng nhập và đăng tuyển | Dữ liệu TestSteps và kịch bản Playwright cho validation form | Chạy thử nghiệm runner trên giao diện web, kiểm tra độ chính xác của các assert |
| 28/07/2026 | Antigravity (Gemini) | Tối ưu điểm hiệu năng Google PageSpeed Insights cho trang chủ và viewer CV | Cấu hình Static File Cache-Control, nén Brotli/Gzip và tối ưu render-blocking assets | Kiểm tra điểm số trên Google PageSpeed (đạt 90+ Mobile/Desktop), test lại hiển thị giao diện |
| 15/07/2026 | ChatGPT | Hỗ trợ thiết kế thuật toán so khớp ngữ nghĩa CV và Job Description | Đề xuất mô hình Sentence-Transformers (phép đo Cosine Similarity trên embedding) | Xây dựng microservice Python FastAPI `AiMatchService`, tích hợp cache vector vào database |
| 20/06/2026 | ChatGPT | Tích hợp cổng thanh toán PayOS webhook cho gói nâng cấp Pro | Code mẫu xử lý checksum và webhook controller ASP.NET Core | Viết lại logic xác thực IPN, kiểm tra chữ ký HMAC SHA256 và cập nhật hạn dùng Pro vào DB |
| 10/06/2026 | ChatGPT | Thiết kế cơ chế SignalR theo dõi phiên đăng nhập của người dùng đa thiết bị | Mã nguồn `UserSessionHub` và cấu hình session tracking | Tích hợp vào middleware ASP.NET Core, bổ sung lưu vết IP/Device vào bảng Users |
