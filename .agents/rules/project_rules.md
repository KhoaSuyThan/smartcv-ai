# QUY TẮC & HƯỚNG DẪN DỰ ÁN (PROJECT RULES & GUIDELINES)

Tài liệu này quy định toàn bộ các nguyên tắc làm việc, tiêu chuẩn viết mã nguồn, an toàn hệ thống và quy trình vận hành mà trợ lý AI (Agent) bắt buộc phải tuân thủ nghiêm ngặt trong suốt quá trình phát triển dự án SmartCV.

---

## 1. NGUYÊN TẮC GIAO TIẾP & PHONG CÁCH LÀM VIỆC
- **Ngôn ngữ:** Luôn luôn trả lời bằng Tiếng Việt.
- **Thái độ:** Trả lời trung thực, khách quan, đi thẳng vào trọng tâm vấn đề kỹ thuật; không nịnh nọt, khen ngợi sáo rỗng hoặc dài dòng.
- **Minh bạch:** Giải thích rõ ràng nguyên nhân lỗi, phương án xử lý và các rủi ro tiềm ẩn (nếu có) trước khi thực thi.

---

## 2. QUY TẮC CƠ SỞ DỮ LIỆU (DATABASE & MIGRATION)
- **Hỏi ý kiến trước khi sửa:** Khi gặp yêu cầu thay đổi cấu trúc cơ sở dữ liệu (Schema, bảng, cột, kiểu dữ liệu, khóa ngoại...), **BẮT BUỘC** phải lập kế hoạch chi tiết (Plan) và hỏi ý kiến xác nhận của người dùng trước khi tiến hành thực hiện.
- **Đồng bộ hóa dữ liệu:** Khi cập nhật cơ sở dữ liệu, phải đảm bảo:
  1. Cập nhật các file script SQL tương ứng trong thư mục lưu trữ data (`DtbDoAnCS/`).
  2. Cập nhật mã nguồn khởi tạo dữ liệu tự động (seeding logic) trong `Program.cs` để ứng dụng có thể tự động chạy trên môi trường mới mà không bị crash.

---

## 3. TIÊU CHUẨN PHÁT TRIỂN & VIẾT MÃ NGUỒN (CODING STANDARDS)
- **Kế hoạch trước khi làm:** Luôn lập kế hoạch chi tiết (Implementation Plan) cho tất cả các yêu cầu mới trước khi bắt tay vào chỉnh sửa mã nguồn.
- **Chú thích (Comments):** Luôn thêm các dòng bình luận ngắn gọn, súc tích và rõ nghĩa vào code khi sinh ra để giải thích logic cốt lõi.
- **Bảo toàn mã nguồn:** Tuyệt đối giữ nguyên các chú thích cũ, tài liệu (docs) và cấu trúc code cũ ở các phần không liên quan trực tiếp đến thay đổi.
- **Kiểm tra biên dịch:** Sau khi chỉnh sửa code Front-end (Vue/React) hoặc Back-end (.NET Core), luôn tự động chạy lệnh build kiểm tra (`npm run build` hoặc `dotnet build`) để đảm bảo không có lỗi cú pháp/biên dịch trước khi kết thúc lượt hỗ trợ.
- **Tự động kiểm thử chủ động:** Được người dùng trao toàn quyền kiểm thử, AI BẮT BUỘC phải chủ động chạy kiểm tra lại hệ thống sau khi làm xong (test build, gọi endpoint API kiểm tra, kiểm tra kịch bản test runner hoặc xác minh tính đúng đắn của tính năng vừa sửa) trước khi hoàn tất lượt.

---

## 4. QUY TẮC VẬN HÀNH & KHỞI CHẠY HỆ THỐNG (DEV WORKFLOW)
- **Front-end (Vue 3 / Vite - Thư mục `CVBuilderApp`):**
  - Chạy môi trường phát triển (Dev Server): Dùng lệnh `npm run dev` (hoặc `npm --prefix CVBuilderApp run dev`) để mở server tại cổng `http://localhost:5173`.
  - Lưu ý: Lệnh `npm run build` chỉ dùng để đóng gói thành tệp tĩnh vào thư mục `wwwroot` hoặc `dist`, **không phải** là lệnh chạy server phát triển.
- **Back-end (ASP.NET Core Web API):**
  - Khởi chạy bằng `dotnet run`. Cổng mặc định: HTTPS `https://localhost:7029` và HTTP `http://localhost:5168`.
  - Tránh chạy `dotnet build` khi tiến trình `dotnet run` đang chạy trên terminal vì tệp thực thi `bin/Debug/net10.0/DoAnCS.exe` sẽ bị lock (lỗi MSB3026 / MSB3027). Khi cần build lại, hãy dừng server trước hoặc kiểm tra qua `dotnet build --no-incremental`.
- **Dịch vụ AI (Python / FastAPI - Thư mục `AiMatchService`):**
  - Khởi chạy qua script `run_ai_server.bat` hoặc FastAPI tại cổng `http://localhost:8000`.

---

## 5. AN TOÀN TỆP TIN & QUẢN TRỊ HỆ THỐNG
- **Bảo vệ tệp có sẵn:** Tuyệt đối **KHÔNG** tự ý xóa, di chuyển hoặc đổi tên bất kỳ tệp tin/thư mục sẵn có nào trong dự án cũng như trên máy tính của người dùng khi chưa có sự cho phép rõ ràng.
- **Quy trình khi đề xuất xóa tệp:** Liệt kê danh sách tệp cần xóa, nêu rõ lý do và hỏi ý kiến xác nhận của người dùng trước khi thực thi.
- **Dọn dẹp an toàn:** Chỉ được phép tự động xóa các tệp tin tạm (scratch / temp test scripts) do chính AI tự tạo ra trong phiên làm việc hiện tại để phục vụ kiểm thử.
- **Không tự ý commit Git:** Tuyệt đối không tự ý thực hiện commit mã nguồn lên Git; hãy để người dùng tự review và commit.
- **Quy chuẩn Git (Commit & Branch):**
  - **Commit Message:** Viết bằng **Tiếng Việt có dấu**, ngắn gọn, rõ nghĩa (Ví dụ: `git commit -m "Cập nhật cấu hình cổng Kestrel"`).
  - **Branch Name:** Đặt bằng **Tiếng Anh**, ngắn gọn (Ví dụ: `main`, `dev`, `feat/agent-rules`, `fix/login-bug`).
  - Mọi gợi ý câu lệnh Git gửi đến người dùng đều phải tuân thủ chuẩn này.
- **Giải phóng cổng mạng (Ports):** Nếu trong quá trình chạy thử nghiệm có khởi chạy bất kỳ tiến trình chiếm dụng port (Server AI, Dev Server...), bắt buộc phải tắt tiến trình đó để giải phóng cổng sau khi hoàn thành.

---

## 6. TỐI ƯU GIAO DIỆN & SỬ DỤNG CÔNG CỤ (AGENT TOOLS)
- **Tiêu đề công cụ:** Khi gọi các tool, bắt buộc phải đặt `toolAction` và `toolSummary` bằng tiếng Việt ngắn gọn, súc tích (từ 2-5 từ) để tiêu đề thông báo hiển thị đẹp mắt, không bị tràn khung.
- **Tránh lệnh dài trên Terminal:** Tuyệt đối **KHÔNG** truyền các câu lệnh shell/powershell/python inline quá dài (`python -c "..."` nhiều dòng) vào `run_command` vì sẽ làm tràn nút bấm và vỡ giao diện xác nhận quyền của IDE.
- **Thực thi script phức tạp:** Tạo file script ngắn trong thư mục scratch (`scratch/temp_task.py`) rồi chạy file đó bằng câu lệnh ngắn gọn (ví dụ: `python scratch/temp_task.py`), sau đó dọn dẹp file tạm.
- **Ưu tiên công cụ chuyên biệt:** Ưu tiên dùng `view_file`, `replace_file_content`, `grep_search`, `list_dir` thay vì chạy lệnh terminal dài dòng.

---

## 7. CẬP NHẬT NHẬT KÝ THAY ĐỔI (CHANGELOG)
- **Tự động cập nhật:** Sau mỗi lần hoàn thành thay đổi mã nguồn, bắt buộc phải tự động cập nhật tóm tắt các thay đổi vào file `CHANGELOG.md` tại thư mục gốc.
- **Quy tắc 1 ngày 1 mục:** Mỗi ngày làm việc chỉ duy trì **duy nhất 1 heading `## [YYYY-MM-DD]`**. Nếu ngày hôm đó đã có mục trong file rồi thì chỉ gộp/bổ sung các bullet points vào mục đó, tuyệt đối không tạo thêm heading ngày trùng lặp.
- **Ngắn gọn & súc tích:** Nội dung ghi chú phải thật cô đọng, đi thẳng vào bản chất thay đổi chính, không viết rườm rà dài dòng.
- **Vị trí chèn:** Các thông tin thay đổi mới bắt buộc phải được tự động chèn vào ngay bên dưới dòng thứ 7 (ngay sau dấu phân cách `---`) để luôn đưa nội dung mới nhất lên đầu file.

---

## 8. QUY TẮC NHẬT KÝ SỬ DỤNG AI (AI USAGE LOG - BẮT BUỘC)
- **Minh bạch học thuật:** Mọi hoạt động AI tham gia hỗ trợ dự án (viết code, giải thích khái niệm, sửa lỗi, thiết kế kiến trúc, sinh test case, viết báo cáo) đều phải có nhật ký theo dõi minh bạch tại `AI_USAGE_LOG.md`.
- **Chuẩn biểu mẫu 5 cột của Giảng viên:** Bắt buộc duy trì đúng cấu trúc 5 cột:
  `Thời gian | AI | Nội dung yêu cầu | AI tạo ra | Sinh viên xử lý`
- **Nguyên tắc "Không copy AI rồi nộp":** Cột *Sinh viên xử lý* phải nêu rõ vai trò kiểm tra, đánh giá, tinh chỉnh, viết lại validation hoặc kiểm thử thực tế của sinh viên; sinh viên phải hiểu rõ bản chất (Code làm gì, tại sao cần, input/output, kiến trúc phía sau, rủi ro tiềm ẩn) để phục vụ vấn đáp và bảo vệ đề tài.

