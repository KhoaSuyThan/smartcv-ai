# AGENT RULES & PROJECT GUIDELINES (SMARTCV)

Tài liệu này đóng vai trò là "hiến pháp" và nguyên tắc hoạt động cốt lõi cho trợ lý AI (Agent) khi làm việc trong kho mã nguồn SmartCV.

> Chi tiết các quy tắc đã được đồng bộ tại: [.agents/rules/project_rules.md](file:///.agents/rules/project_rules.md)

---

## 1. NGUYÊN TẮC GIAO TIẾP
- Trả lời 100% bằng Tiếng Việt.
- Đi thẳng vào vấn đề kỹ thuật, khách quan, trung thực; không nịnh nọt hoặc dùng lời khen sáo rỗng.

---

## 2. QUY TẮC CƠ SỞ DỮ LIỆU (DATABASE)
- **BẮT BUỘC** lập kế hoạch chi tiết (Plan) và xin ý kiến xác nhận của người dùng trước khi thay đổi cấu trúc bảng, cột, kiểu dữ liệu, index hoặc schema.
- Khi sửa database: Cập nhật đồng thời script SQL tại thư mục `DtbDoAnCS/` và cập nhật logic seeding tự động trong `Program.cs`.

---

## 3. TIÊU CHUẨN VIẾT MÃ NGUỒN & PHÁT TRIỂN
- Luôn lập kế hoạch trước khi bắt tay vào sửa code.
- Thêm comments ngắn gọn, súc tích, giải thích logic cốt lõi. Giữ nguyên cấu trúc và chú thích cũ ở các đoạn code không liên quan.
- Chạy lệnh build kiểm tra (`npm run build` hoặc `dotnet build`) trước khi hoàn tất mỗi lượt.
- **Tự động kiểm thử (Được cấp quyền test chủ động):** Sau khi hoàn thành bất kỳ tính năng hoặc sửa lỗi nào, AI BẮT BUỘC phải tự động chạy kiểm thử lại (kiểm tra build, gọi API/Endpoint test, chạy Automation Runner hoặc xác minh luồng dữ liệu) để đảm bảo hệ thống hoạt động ổn định trước khi bàn giao.

---

## 4. QUY TRÌNH CHẠY HỆ THỐNG (DEV SERVERS)
- **Front-end (Vue 3 / Vite):** Dùng `npm run dev` (hoặc `npm --prefix CVBuilderApp run dev`) để khởi chạy dev server tại `http://localhost:5173`. Lệnh `npm run build` chỉ dùng để đóng gói production, không mở server.
- **Back-end (.NET Core):** Khởi chạy bằng `dotnet run` (port `https://localhost:7029` / `http://localhost:5168`). Không chạy `dotnet build` khi ứng dụng đang chạy để tránh lỗi xung đột khóa file `.exe`.
- **Python AI (FastAPI):** Khởi chạy qua `run_ai_server.bat` hoặc FastAPI tại `http://localhost:8000`.

---

## 5. AN TOÀN TỆP TIN & QUẢN TRỊ
- Tuyệt đối **KHÔNG** tự ý xóa, di chuyển hoặc đổi tên bất kỳ tệp tin/thư mục có sẵn nào nếu người dùng chưa cho phép rõ ràng.
- Tuyệt đối không tự ý commit mã nguồn lên Git (để người dùng tự kiểm tra và commit).
- **Quy chuẩn Git (Commit & Branch):**
  - **Commit Message:** Viết bằng **Tiếng Việt có dấu**, ngắn gọn, súc tích (Ví dụ: `git commit -m "Cập nhật cấu hình cổng Kestrel"`).
  - **Branch Name:** Đặt bằng **Tiếng Anh**, ngắn gọn (Ví dụ: `main`, `dev`, `feat/agent-rules`, `fix/login-issue`).
  - Mọi gợi ý lệnh Git cho người dùng đều phải tuân thủ chuẩn này.
- Giải phóng các cổng mạng (ports) sau khi kết thúc các tiến trình thử nghiệm.

---

## 6. TỐI ƯU GIAO DIỆN & SỬ DỤNG TOOL
- `toolAction` và `toolSummary` phải dùng tiếng Việt ngắn gọn (2-5 từ).
- Tuyệt đối không truyền lệnh shell/powershell quá dài inline vào `run_command` để tránh làm vỡ giao diện xác nhận của IDE. Tạo script tạm trong `scratch/` nếu cần xử lý phức tạp.
- Ưu tiên các công cụ có sẵn (`view_file`, `replace_file_content`, `grep_search`).

---

## 7. CẬP NHẬT CHANGELOG
- Tự động cập nhật tóm tắt thay đổi vào `CHANGELOG.md` tại thư mục gốc sau mỗi lần sửa code.
- **Quy tắc 1 ngày 1 mục:** Mỗi ngày làm việc chỉ duy trì **duy nhất 1 heading `## [YYYY-MM-DD]`**. Nếu ngày đó đã có mục thì chỉ gộp/bổ sung các bullet points vào mục của ngày đó, không tạo thêm heading ngày trùng lặp.
- **Ngắn gọn, súc tích:** Nội dung ghi chú phải thật cô đọng, đi thẳng vào thay đổi chính, không viết dài dòng.
- Chèn nội dung mới nhất ngay bên dưới dòng thứ 7 (dưới dấu phân cách `---`).

---

## 8. QUY TẮC NHẬT KÝ SỬ DỤNG AI (AI USAGE LOG - BẮT BUỘC)
- **Minh bạch học thuật:** Mọi hoạt động AI tham gia hỗ trợ (viết code, sửa lỗi, thiết kế kiến trúc, sinh test, viết tài liệu) đều phải có nhật ký theo dõi tại [AI_USAGE_LOG.md](file:///AI_USAGE_LOG.md).
- **Chuẩn biểu mẫu 5 cột của Giảng viên:** Bắt buộc duy trì đúng format:
  `Thời gian | AI | Nội dung yêu cầu | AI tạo ra | Sinh viên xử lý`
- **Nguyên tắc "Không copy AI rồi nộp":** Cột *Sinh viên xử lý* phải nêu rõ vai trò kiểm tra, tinh chỉnh, viết lại validation hoặc kiểm thử thực tế của sinh viên để đảm bảo hiểu sâu bản chất kỹ thuật khi bảo vệ đề tài.

