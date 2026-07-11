# Nhật ký thay đổi (Changelog)

Tất cả các thay đổi quan trọng của dự án sẽ được ghi nhận tại file này để phục vụ việc phát triển và bàn giao.
Các thay đổi mới nhất sẽ luôn được đưa lên đầu file.

---

## [2026-07-11] - Cải tiến Dark Mode & Sửa lỗi Thông báo Đăng nhập

### Added
- Bổ sung thông báo đăng nhập thành công cho luồng đăng nhập bằng tài khoản mạng xã hội (Google, Facebook...) trong `AccountController`.

### Fixed
- **Đồng bộ hóa tông màu Slate Blue trong Layout:** Cập nhật trực tiếp các thuộc tính CSS Dark Mode trong `_Layout.cshtml` (body sang `#0f172a`, card/navbar/footer/sidebar sang `#1e293b`) để đảm bảo không bị ghi đè CSS cũ, giúp giao diện tối nhất quán và mềm mại.
- **Sửa lỗi tương phản văn bản chi tiết:** Tinh chỉnh các phần tử `.bg-white` và `.bg-light` trong Dark Mode của layout để tự động chuyển sang nền tối phù hợp, khắc phục triệt để lỗi chữ xám mờ trên nền trắng tại trang chi tiết bài thi phỏng vấn (`AssignedInterview.cshtml`).
- **Sửa lỗi trắc nghiệm trong Dark Mode:** Khắc phục lỗi chữ phương án trả lời (A, B, C, D) bị đen mờ trên nền tối tại trang làm bài trắc nghiệm chuyên môn (`PracticeQuiz.cshtml`).
- **Sửa lỗi Dark Mode cho Modals tuyển dụng:** Khắc phục triệt để lỗi tương phản chữ và nền của các Modal "Thiết lập & Giao phỏng vấn", "Đánh giá bài làm phỏng vấn tự luận" (`Candidates.cshtml`) trong Dark Mode.
- **Khắc phục lỗi cuộn textarea nhận xét chi tiết (STAR):** Sửa lỗi chiều cao của textarea nhận xét bị tính toán sai (bằng 0 do tính toán khi modal đang ẩn) và tăng giới hạn chiều cao tối đa lên 400px để hiển thị đầy đủ nội dung.
- **Khắc phục khoảng trắng và xám chân trang khi xem CV PDF:** Điều chỉnh chiều cao `.content-wrapper` thành `calc(100vh - 72px)` để khớp hoàn toàn phần giao diện bên dưới navbar, khóa cuộn cửa sổ chính và ẩn footer ở trang xem CV (`PublicViewerCVVue.cshtml`) nhằm loại bỏ hoàn toàn khoảng trống xám/trắng dư thừa ở chân trang khi hiển thị PDF.
- **Khắc phục xung đột tệp tin thực thi:** Dọn dẹp tiến trình bị khóa và biên dịch lại hệ thống thành công.
- **Tối ưu bảng màu Dark Mode:** Thay đổi hình nền và màu nền Dark Mode từ đen tuyền (`#121212`) sang Slate Blue tối dịu mắt hơn (`#0f172a` và `#1e293b`), giúp giao diện trông mềm mại và sang trọng hơn.
- **Sửa lỗi hiển thị màu chữ trong Dark Mode:** Khắc phục triệt để lỗi chữ màu tối (gây ra bởi class `.text-dark` và `.transition-hover`) bị chìm trên nền tối tại trang luyện phỏng vấn và xem kết quả phỏng vấn.
- **Sửa lỗi hover danh sách:** Thiết lập lại hiệu ứng hover dòng trong các bảng danh sách (Assigned Interviews) ở chế độ tối để không bị loá nền trắng.

## [2026-07-11] - Hệ thống Thông báo (Notification Bell)

### Added
- Thêm biểu tượng chuông thông báo (🔔) trên thanh navbar, ngay bên cạnh nút tên người dùng.
- Tạo bảng Database mới `Notifications` lưu trữ toàn bộ thông báo cho từng user.
- Tạo `NotificationController` với 5 API: GetAll, UnreadCount, MarkAsRead, MarkAllAsRead, Delete.
- Tạo Model `Notification.cs` với các trường: Type, Title, Message, Link, IsRead, CreatedAt.
- Badge đỏ hiển thị số thông báo chưa đọc trên chuông (ẩn khi 0, tối đa 99+).
- Dropdown panel hiển thị 20 thông báo gần nhất với icon phân loại theo màu (Ứng tuyển, Phỏng vấn, Đăng nhập, Hệ thống, Admin).
- Nút "Đọc tất cả" đánh dấu toàn bộ thông báo là đã đọc.
- Tự động tạo thông báo khi: đăng nhập thành công, ứng tuyển thành công, Recruiter thay đổi trạng thái đơn ứng tuyển.
- Tích hợp SignalR real-time: thông báo mới được đẩy tức thì tới client mà không cần refresh trang.
- Hỗ trợ Dark Mode đầy đủ cho toàn bộ dropdown thông báo.
- Tự động dọn dẹp thông báo cũ quá 30 ngày khi khởi động server.
- Cập nhật `DtbDoAnCS/CreateTables.sql` thêm bảng Notifications + Index.
- Cập nhật `Program.cs` thêm migration tự động tạo bảng + logic cleanup 30 ngày.

### Changed
- `AccountController`: Inject `IHubContext<UserSessionHub>`, gọi `CreateNotification()` sau khi đăng nhập thành công.
- `JobsController`: Inject `IHubContext<UserSessionHub>`, gọi `CreateNotification()` khi ứng tuyển và khi cập nhật trạng thái đơn.
- `AppDbContext`: Thêm `DbSet<Notification>` và cấu hình FK cascade delete.
- `_Layout.cshtml`: Thêm HTML/CSS/JS cho notification bell, dropdown panel; sửa lỗi cú pháp CSS keyframes, gán biến SignalR toàn cục để bắt thông báo real-time.

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
