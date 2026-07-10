# Nhật ký thay đổi (Changelog)

Tất cả các thay đổi quan trọng của dự án sẽ được ghi nhận tại file này để phục vụ việc phát triển và bàn giao.
Các thay đổi mới nhất sẽ luôn được đưa lên đầu file.

---

## [2026-07-10] - Tối ưu hóa phòng phỏng vấn AI & Bảo mật đăng nhập

### Added
- Thêm ô nhập số câu hỏi (`input type="number"`) trực quan tại trang thiết lập phòng phỏng vấn.
- Thêm thanh tiến trình (Progress Bar) hiển thị số lượt phỏng vấn đã dùng trong ngày.
- Bổ sung logic kiểm tra giới hạn lượt phỏng vấn hàng ngày (Free: 3 lượt, Pro: 5 lượt) ở cả Frontend (vô hiệu hóa nút bấm) và Backend.

### Changed
- Cập nhật prompt AI sinh đề trắc nghiệm: Đóng vai nhà tuyển dụng hỏi kiến thức chuyên môn kỹ thuật trực tiếp dành cho ứng viên, thay vì hỏi các thông tin metadata lấy từ CV.
- Cải tiến bộ phân tích JSON (JsonDocument Parse) hỗ trợ không phân biệt chữ hoa/thường (case-insensitive) cho các key trả về từ AI (`question`, `options`, `correctAnswer`).
- Cấu hình chỉ hiển thị các CV ở trạng thái công khai (`IsPublic == true`) làm nguồn dữ liệu phỏng vấn.
- Tự động thay đổi giá trị nhập câu hỏi dựa theo tài khoản (STAR: 3-5/3-10, Quiz: 5-10/5-20) và chế độ phỏng vấn.

### Fixed
- Khắc phục lỗi đề thi trắc nghiệm trống rỗng do AI trả về key viết hoa/thường lệch chuẩn.
- Thêm cơ chế tự động hủy phiên (Rollback) và báo lỗi nếu AI trả về danh sách câu hỏi lỗi (không parse được tin nhắn nào).
- Đổi nhãn tiêu đề số lượng câu hỏi trắc nghiệm hiển thị động thay vì cố định 5 câu.

### Security
- Tắt tính năng tự động gửi email cảnh báo bảo mật khi đăng nhập đối với tài khoản vai trò `Admin` để tránh spam hộp thư khi test và chuyển đổi tài khoản liên tục.
