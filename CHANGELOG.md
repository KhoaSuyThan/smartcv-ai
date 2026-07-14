# Nhật ký thay đổi (Changelog)

Tất cả các thay đổi quan trọng của dự án sẽ được ghi nhận tại file này để phục vụ việc phát triển và bàn giao.
Các thay đổi mới nhất sẽ luôn được đưa lên đầu file.

---

## [2026-07-14] - Cải tiến toàn diện giao diện trang cá nhân và khắc phục lỗi Dark Mode trang Builder CV

### Added
- Tích hợp ô nhập kỹ năng chính dạng tag động (`#skillsContainer`), hỗ trợ thêm nhanh bằng phím `Enter`/dấu phẩy `,` và xóa bằng phím `Backspace` hoặc nút đóng.
- Bổ sung CSS tùy biến (`.profile-info-row`, `.profile-info-label`, `.profile-info-value`, `.skill-badge`) thiết lập bố cục Key-Value hàng ngang siêu gọn, đồng bộ cấu trúc giữa chế độ Xem và Sửa.
- Thêm cơ chế MutationObserver tự động gán thuộc tính `data-bs-theme="light"` cô lập vùng preview CV khỏi Dark Mode của hệ thống.

### Changed
- Khắc phục lỗi tương phản màu chữ trong Dark Mode cho nhãn, email và các ô nhập liệu bị khóa.
- Tinh chỉnh chiều cao hàng (`min-height: 56px`) và chiều cao các input/select (`height: 38px`, padding `6px 12px`) giúp chuyển đổi Xem/Sửa mượt mà và loại bỏ hoàn toàn hiện tượng giật nhảy chiều cao (layout shift).
- Cập nhật JavaScript để chuyển đổi trạng thái View/Edit mượt mà, đồng thời tự động gộp các tag kỹ năng và link portfolio trước khi submit form để tương thích 100% với database.
- Cập nhật `site.css` loại trừ vùng in và hiển thị CV (`#cv-printable-area`, `.cv-preview-card`, `.cv-preview-wrapper`) khỏi các thuộc tính màu tối mặc định của Dark Mode, giúp giữ nguyên giao diện gốc của mẫu CV để dễ dàng preview trước khi in.
- Khắc phục lỗi tương phản màu chữ trong ô nhập liệu (input, textarea) của panel soạn thảo CV trong chế độ tối bằng cách ép màu nền tối trung tính (`#1e293b`) và màu chữ trắng sáng (`#f8fafc`).
- Khắc phục lỗi hiển thị nút đổi ngôn ngữ VI | EN trong toolbar ở chế độ tối bằng cách loại bỏ gradient nền sáng của Tailwind (`background-image: none !important;`), thay thế bằng màu nền tối trung tính (`#1e293b`) và màu chữ sáng để hiển thị rõ nét nhất.
- Tối ưu hóa giao diện Dark Mode cho các popup modal gồm: Modal nâng cấp tài khoản Pro (`#proUpgradeModal`) và Modal cảnh báo trùng đăng nhập (`#session-conflict-modal`), chuyển đổi màu nền tối và màu chữ tương phản cao giúp giao diện hiển thị đồng bộ.
- Chuyển `deploy.yml` sang kích hoạt thủ công (`workflow_dispatch`) thay vì tự động deploy mỗi khi push lên `main`, tránh web bị đơ trong quá trình phát triển.

## [2026-07-13] - Nâng cấp định vị IP đăng nhập, tối ưu hóa giao diện Header và Sticky Footer

### Added
- Tích hợp trường vị trí `LastLoginLocation` vào Model `User` và database schema (`CreateTables.sql` / `Program.cs` migration).
- Bổ sung hiển thị thông tin Lịch sử đăng nhập chi tiết (thiết bị, IP, vị trí tương đối) tại trang cá nhân (`Profile.cshtml`).

### Changed
- Tích hợp dịch vụ định vị IP qua API `ip-api.com` khi đăng nhập thiết bị lạ để gửi cảnh báo chứa vị trí đăng nhập tương đối.
- Tối giản hóa và thiết kế lại Header Card nhỏ gọn cho các trang: Mẫu CV Vue, Lời mời phỏng vấn, Luyện phỏng vấn AI và Danh sách công ty.
- Nâng cấp giao diện Chế độ tối (Dark Mode) cho các Card tiêu đề trên bằng phong cách **Glassmorphism** (nền bán trong suốt 10%, viền sáng màu đặc trưng) giúp các khung nổi bật, hiện đại và không bị chìm. Đồng thời giữ nguyên nền trắng cho khung chứa logo công ty (`.company-logo-wrapper`) để tránh các logo dạng transparent bị chìm trong chế độ tối.
- Sửa triệt để lỗi footer bị nhảy lơ lửng khi trang ít dữ liệu bằng cách bù trừ tỷ lệ zoom 80% (`min-height: 125vh` cho `html` và `body` trong `_Layout.cshtml`).

## [2026-07-12] - Tối ưu hóa thông báo đăng nhập theo IP và Thiết bị mới

### Added
- Bổ sung các thuộc tính `LastLoginIP` và `LastLoginDevice` vào model `User` (`Models/User.cs`).
- Thêm logic tự động Migration trong `Program.cs` để thêm 2 cột mới này vào bảng `Users`.
- Cập nhật script cơ sở dữ liệu `DtbDoAnCS/CreateTables.sql` để đồng bộ cấu trúc bảng `Users`.

### Changed
- Cập nhật logic đăng nhập thường và đăng nhập qua MXH trong `AccountController.cs`:
  - Trích xuất địa chỉ IP người dùng (hỗ trợ `X-Forwarded-For` thông qua proxy) và User-Agent thiết bị (đã rút gọn thành tên HĐH và trình duyệt dễ đọc).
  - So sánh IP/Thiết bị hiện tại với lần đăng nhập trước đó: Chỉ tạo thông báo hệ thống và gửi email cảnh báo bảo mật nếu phát hiện IP mới hoặc thiết bị mới. Bỏ qua hoàn toàn thông báo nếu đăng nhập từ cùng một IP/thiết bị cũ để tránh gây phiền nhiễu.

## [2026-07-12] - Cấu hình bộ lọc email tại localhost

### Changed
- Cập nhật dịch vụ gửi email (`Services/EmailService.cs`):
  - Tích hợp kiểm tra môi trường Development để nhận diện khi chạy ở localhost.
  - Thiết lập bộ lọc chỉ cho phép gửi đi các email thiết yếu: OTP/Xác thực tài khoản/Mật khẩu, Job Offer (Thư mời nhận việc gửi cho ứng viên) và Lịch phỏng vấn.
  - Tự động chặn các email phụ khác (như Chào mừng đăng ký, Cảnh báo đăng nhập, Phản hồi thư mời nhận việc từ ứng viên gửi nhà tuyển dụng...) khi chạy ở localhost để tránh làm trôi hòm thư kiểm thử.

## [2026-07-12] - Tự động hóa quy trình tuyển dụng sau khi chấp nhận hồ sơ (Đặt lịch Phỏng vấn & Thư mời nhận việc)

### Added
- Bổ sung các thuộc tính Navigation trong `Application.cs` đến `InterviewSchedule` và `JobOffer` để hỗ trợ Eager Loading đồng bộ dữ liệu.
- Cập nhật trang quản lý ứng viên của Nhà tuyển dụng (`Views/Jobs/Candidates.cshtml`):
  - Hiển thị badge trạng thái động cho các đơn ở trạng thái `Interviewing`, `Offered` (Đã gửi Offer) và `DeclinedOffer` (Từ chối Offer).
  - Tích hợp Modal Quyết định duyệt hồ sơ cho phép lựa chọn giữa **Đặt lịch phỏng vấn** (Online qua Google Meet/Zoom hoặc Offline trực tiếp) hoặc **Gửi Offer mời nhận việc trực tiếp**.
  - Tích hợp Modal xem chi tiết lịch hẹn phỏng vấn đã lên và Modal xem chi tiết Thư mời nhận việc đã gửi (lương đề xuất, ngày đi làm, điều khoản).
  - Thêm nút nhanh "Mời nhận việc" dành riêng cho ứng viên ở trạng thái `Interviewing`.
- Cập nhật trang đơn ứng tuyển của Ứng viên (`Views/Account/Applications.cshtml`):
  - Hiển thị các badge trạng thái phỏng vấn và Offer trực quan, cho phép ứng viên click vào để xem chi tiết.
  - Tích hợp Modal hiển thị thông tin lịch phỏng vấn chi tiết (Thời gian, hình thức, địa điểm/link họp và chỉ dẫn từ nhà tuyển dụng).
  - Tích hợp Modal thư mời nhận việc (Job Offer) chứa chi tiết lương, ngày bắt đầu và điều khoản, đồng thời cung cấp 2 nút phản hồi nhanh: **Đồng ý nhận việc** hoặc **Từ chối Offer**.
  - Gửi yêu cầu phản hồi bất đồng bộ thông qua API `RespondToOffer` và tự động cập nhật giao diện thời gian thực.

### Changed
- `JobsController.cs`: Cập nhật Action `Candidates` để Include thêm dữ liệu `InterviewSchedule` và `JobOffer`.
- `AccountController.cs`: Cập nhật Action `Applications` để Include thêm dữ liệu `InterviewSchedule` and `JobOffer`.

### Fixed
- Khắc phục xung đột tiến trình `DoAnCS.exe` bị treo bằng cách tự động tắt tiến trình trước khi thực hiện build hệ thống.
- Dọn dẹp các khối database migration thủ công cũ trong `Program.cs`, chỉ giữ lại đoạn tự động tạo 2 bảng mới (`InterviewSchedules` và `JobOffers`).

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
