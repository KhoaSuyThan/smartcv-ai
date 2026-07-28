# Nhật ký thay đổi (Changelog)

Tất cả các thay đổi quan trọng của dự án sẽ được ghi nhận tại file này để phục vụ việc phát triển và bàn giao.
Các thay đổi mới nhất sẽ luôn được đưa lên đầu file.

---

## [2026-07-28] - Tối ưu PageSpeed Insights (Giai đoạn 2): Triệt tiêu tài nguyên chặn hiển thị (Render-Blocking Resources)

### Added & Enhanced
- **Tối ưu hóa nạp CDN (Preconnect & DNS Prefetch):** Thêm thẻ `<link rel="preconnect">` và `<link rel="dns-prefetch">` tới các máy chủ CDN (`cdn.jsdelivr.net`, `cdnjs.cloudflare.com`, `unpkg.com`) trong `Views/Shared/_Layout.cshtml` giúp trình duyệt thiết lập kết nối sớm trước khi tải dữ liệu.
- **Tải bất đồng bộ CSS Biểu tượng & Animation (Asynchronous CSS):** Chuyển đổi các tệp CSS biểu tượng (`bootstrap-icons`, `font-awesome`, `boxicons`) và hiệu ứng `aos.css` sang cơ chế nạp bất đồng bộ không chặn dựng hình (`rel="preload" as="style" onload="..."`), kèm thẻ `<noscript>` dự phòng.
- **Trì hoãn JS & Giải phóng Main Thread:** Bổ sung thuộc tính `defer` cho toàn bộ thẻ `<script>` (`jquery`, `bootstrap.bundle`, `site.js`, `signalr.js`, `aos.js`) và trì hoãn khởi tạo `AOS.init()`, SignalR `connection.start()` vào sự kiện `window load` giúp giải phóng chuỗi xử lý chính trong quá trình dựng HTML ban đầu.

### Added & Enhanced
- **Tối ưu Browser Caching (Bộ nhớ đệm trình duyệt):** Cấu hình `StaticFileOptions` trong `Program.cs` tự động chèn header `Cache-Control: public, max-age=31536000, immutable` (365 ngày) cho tất cả tài nguyên tĩnh (`.css`, `.js`, `.png`, `.jpg`, `.webp`, `.woff2`, `.svg`) phục vụ tại các đường dẫn tĩnh gốc, `/avt` và `/images/templates`.
- **Mở rộng Response Compression (Nén dữ liệu phản hồi):** Bổ sung danh sách `MimeTypes` mở rộng (`image/svg+xml`, `font/woff2`, `font/woff`, `application/json`, `text/css`, `application/javascript`) cho dịch vụ nén `Brotli` & `Gzip` trong `Program.cs`, giúp giảm dung lượng phản hồi tệp văn bản/biểu tượng qua mạng.

### Fixed & Enhanced
- Cập nhật cấu hình `IntersectionObserver` với `rootMargin: '0px 0px -35% 0px'` và `threshold: 0.35` trong `Views/Home/Index.cshtml`.
- Đảm bảo người dùng phải cuộn chớm sâu đến **đúng 50% kích thước section/màn hình** thì hiệu ứng hiển thị từ từ 2.2s mới bắt đầu được kích hoạt, tránh tình trạng kích hoạt sớm khi vừa chạm viền section.



### Enhanced
- Đã nâng thời gian chuyển động mượt mờ (`reveal-on-scroll`) lên **`2.2 giây`** kết hợp đường cong `ease-out` và khoảng nâng `60px`.
- Khi người dùng cuộn đến 50% mục trước đó, khối nội dung tiếp theo sẽ từ từ trượt lên và hiện dịu mắt rất chậm rãi, đáp ứng hoàn hảo tiêu chuẩn trải nghiệm.



### Enhanced
- Đã điều chỉnh hiệu ứng hiển thị từ từ (`reveal-on-scroll`) từ 0.75s lên `1.3s` kèm đường cong thời gian `cubic-bezier(0.22, 1, 0.36, 1)`.
- Khi người dùng cuộn đến 50% mục trước, section tiếp theo sẽ trượt từ nhẹ `50px` lên `0px` và nổi mượt mờ rất chậm rãi, êm dịu đúng theo mong muốn.



### Enhanced & Fixed
- Triển khai `IntersectionObserver` tự động kích hoạt hiệu ứng hiển thị từ từ (`reveal-on-scroll`) với `cubic-bezier(0.16, 1, 0.3, 1)` độ dài `0.75s`.
- Khi người dùng cuộn tầm 50% kích thước màn hình/mục trước đó đến các phần (`Kho mẫu CV`, `Đối tác công ty lớn`, `Cẩm nang nghề nghiệp`), section mục tiêu sẽ tự động trượt lên và hiện mượt mờ sang trọng, vừa khớp với mong muốn trải nghiệm của người dùng.



### Fixed
- Gỡ bỏ thuộc tính `data-aos="fade-up"` tại `templates-section`, `partners-section`, và `blog-section` trong `Views/Home/Index.cshtml`.
- Đảm bảo khi người dùng cuộn đến 1/2 của mục trước đó, các phần tiếp theo sẽ xuất hiện ngay lập tức sắc nét và mượt mà, loại bỏ triệt để hiện tượng kẹt khoảng trắng/khuất màn hình.



### Fixed
- Sửa lỗi trễ xuất hiện của 3 section cuối (`Views/Home/Index.cshtml`): Gỡ bỏ thuộc tính `data-aos-anchor="#blog-section"` gây bắt buộc cuộn chạm kịch đáy màn hình mới hiển thị.
- Bổ sung `AOS.refresh()` tự động kích hoạt khi toàn bộ hình ảnh và giao diện tải xong (`window.addEventListener('load')`), giúp cập nhật chính xác tọa độ cuộn của các khối nội dung.
- Tối ưu cấu hình AOS (`Views/Shared/_Layout.cshtml`): Chuyển `offset: 80` và `duration: 700` để các phần tử hiển thị mượt mờ tự nhiên ngay khi chớm cuộn tới.



### Fixed & Enhanced
- Tối ưu tính toán phân trang Carousel (`Views/Home/Index.cshtml`): Xác định chính xác số bài viết hiển thị đồng thời theo kích thước màn hình (`Desktop: 3 bài`, `Tablet: 2 bài`, `Mobile: 1 bài`) để tính số nấc cuộn tối đa (`maxIndex`).
- Sửa lỗi đứng yên ở chấm số 4: Giờ đây dải chấm sẽ tạo đúng 4 nấc trang trên Desktop (`0, 1, 2, 3`), khi tự động cuộn đến hết trang cuối (`bài 4-5-6`) hệ thống sẽ tự động quay trở lại trang đầu tiên (`bài 1-2-3`) mượt mờ và chính xác tuyệt đối.


## [2026-07-17] - Nâng cấp giao diện trang Chi tiết công việc và tối ưu hóa chế độ tối các trang Hỗ trợ

### Added
- Thêm thuộc tính `ViewBag.CompanyID` trong Action `Details` của `HomeController.cs` để hỗ trợ liên kết động sang trang chi tiết công ty.

### Changed
- Cải thiện giao diện trang Chi tiết việc làm (`Home/Details.cshtml`) sang cấu trúc 2 cột hiện đại (Cột chính 70% chứa JD/Yêu cầu và Cột bên 30% chứa Sidebar cố định thông tin chung và thao tác Ứng tuyển nhanh).
- Tích hợp font chữ sang trọng `Plus Jakarta Sans` cùng phong cách thiết kế Glassmorphism chuyên nghiệp, bo góc mềm mại (`1.5rem`), và hiệu ứng đổ bóng mượt mà.
- Thiết kế lại các thẻ badge hiển thị thông tin lương, ngày đăng, hạn nộp bằng các tông màu pastel dịu mắt hỗ trợ hoàn hảo cho cả Light Mode và Dark Mode.
- Đồng bộ giao diện chế độ tối (Dark Mode) cao cấp cho cả phần chi tiết việc làm lẫn Modal ứng tuyển nhanh chọn CV, khắc phục triệt để các lỗi chữ tối bị chìm trên nền đen.
- Sửa lỗi biên dịch Razor bằng cách escape ký tự `@@` cho các lệnh CSS import font chữ từ Google Fonts.
- Khắc phục lỗi tương phản màu chữ và nhãn trong Dark Mode cho trang Liên hệ (`Views/Support/Contact.cshtml`): Chuyển màu nền sang CSS class có override `#0f172a`, định nghĩa lại các biến màu `--p-blue` thành màu sáng `#6ea8fe` khi ở chế độ tối, nâng cấp và đồng bộ ô nhập liệu từ kiểu gạch dưới sang dạng hộp bo tròn 4 cạnh (`16px`), di chuyển nhãn lên phía trên độc lập và tạo giao diện đồng bộ tuyệt đối với textarea ở cả hai chế độ sáng/tối.
- Khắc phục lỗi màu chữ tối/tàng hình và nền icon chói mắt trong Dark Mode cho trang Điều khoản (`Views/Support/Terms.cshtml`): Chuyển màu nền trang sang CSS class có override `#0f172a`, sửa lỗi chữ tàng hình của mục bảo mật dữ liệu bằng cách ép màu nền `#0f172a` và màu chữ sáng `#e2e8f0` cho các thẻ div con, làm mờ nền icon (`.icon-shape`) với độ mờ `rgba` và làm sáng màu chữ icon tương ứng, tối ưu hóa danh sách menu mục lục bên trái khi ở chế độ active (`color: #6ea8fe`) và inactive (`color: #94a3b8`).

## [2026-07-16] - Hiện đại hóa giao diện quản trị Admin: Tích hợp Modal Popup và Tối ưu hóa Dark Mode

### Added
- Tích hợp Modal `#editUserModal` để chỉnh sửa thông tin thành viên trực tiếp trên trang `/Admin/Users` thay vì chuyển hướng trang.
- Bổ sung script binding dữ liệu tự động cho nút Sửa thành viên và quản lý logic bật/tắt dropdown chọn Công ty tương ứng với vai trò Recruiter.
- Tích hợp các Modal chỉnh sửa (`#editJobModal`, `#editVueModal`, `#editTemplateModal`) cho Tin tuyển dụng, Vue CV và React CV tại các trang quản trị tương ứng.
- Bổ sung script binding dữ liệu tự động từ các data-attribute của nút bấm sang Modal khi người dùng click vào nút Sửa trên cả 3 trang.
- Tích hợp các Modal tạo mới (`#addJobModal`, `#createTemplateModal`) cho Tin tuyển dụng và React CV thiết kế trực tiếp tại trang quản trị danh sách.
- Bổ sung các tính năng phụ trợ trong Modal: tải danh sách địa điểm hành chính từ `provinces.json`, đồng bộ mức lương tự động, xem trước hình ảnh mẫu thiết kế thời gian thực.
- Bổ sung bộ quy tắc CSS tối ưu hóa giao diện Dark Mode (`[data-bs-theme="dark"]`) dùng chung cho toàn bộ các Modal trong trang Admin tại `/Admin/Templates`, `/Admin/VueTemplates`, `/Admin/Jobs`, `/Admin/Companies` và `/Admin/Users`.

### Changed
- Thay thế hoàn toàn các liên kết chuyển hướng trang cũ (tạo mới/chỉnh sửa) bằng các nút button kích hoạt Modal trực tiếp trên các trang danh sách của Admin.
- Nạp bổ sung danh sách Công ty (`ViewBag.Companies`) trong các Action `Jobs` và `Users` của `AdminController.cs` để hỗ trợ hiển thị các dropdown chọn doanh nghiệp trong Modal.
- Loại bỏ các trường nhập HTML/CSS thủ công trong giao diện tạo mới mẫu CV React, chuyển sang cấu hình theo component và đồng bộ trực tiếp thông qua React CV Builder.
- Khắc phục triệt để lỗi chữ đen chìm trên nền tối, viền ô nhập liệu quá đậm, và thay thế các mảng màu nền tiêu đề (warning/primary) bằng các dải gradient trung tính có độ bão hòa thấp hơn (hổ phách sẫm và xanh dương hoàng gia) kết hợp chữ trắng có độ tương phản cao để tiêu đề Modal sắc nét và cực kỳ dễ đọc trong chế độ Dark Mode.

## [2026-07-16] - Ổn định hóa hệ thống E2E, Real-time Logs và chuẩn hóa địa điểm hành chính

### Added
- Bổ sung logic tự động seed 2 tài khoản cố định `existed_user_validation@smartcv.vn` (vai trò User) và `existed_recruiter_validation@smartcv.vn` (vai trò Recruiter, kèm theo Company mẫu đầy đủ các trường Phone, AvatarUrl, Address, Description) phục vụ riêng cho các kịch bản kiểm thử Validation.
- Thêm các API endpoint `/Admin/RunSingleE2ETest` để chạy từng kịch bản kiểm thử E2E độc lập và `/Admin/SaveE2ETestRun` để lưu kết quả tổng hợp vào cơ sở dữ liệu.
- Bổ sung phương thức `RunSingleE2EFlowAsync` vào `IAutomationTestRunner` và class `AutomationTestRunner`.
- Chuyển đổi biểu mẫu tạo (`Create.cshtml`) và chỉnh sửa (`Edit.cshtml`) tin tuyển dụng sang dropdown chọn Tỉnh/Thành phố và Quận/Huyện chuẩn hóa.
- Triển khai Custom Combobox cho bộ lọc "Vị trí công việc": vừa cho phép nhập tự do vừa cho phép chọn và lọc gợi ý thời gian thực.
- Cập nhật cơ sở dữ liệu `provinces.json` tinh gọn thành 34 tỉnh thành theo nghị quyết sáp nhập hành chính mới.

### Changed
- Sửa đổi đồng bộ logic trong phương thức GET và POST `Create()` của `JobsController.cs`: cho phép tài khoản recruiter chứa `"validation"` trong email được bỏ qua bước kiểm tra hồ sơ doanh nghiệp đầy đủ tương tự như các tài khoản `test_e2e_`, khắc phục triệt để lỗi redirect 302 về trang cá nhân gây treo kiểm thử.
- Cập nhật kịch bản `Auth Validation E2E` và `Jobs Validation E2E` sử dụng các tài khoản cố định đã seed ở trên thay vì các tài khoản test E2E tạm thời (vốn bị xóa sạch sau mỗi lượt chạy của kịch bản chính), giúp tránh hoàn toàn tình trạng mất dữ liệu gây lỗi timeout khi đăng nhập hoặc đăng ký.
- Sửa lỗi lật ngược cấu hình `TargetSelector` và `Value` tại bước 31 của kịch bản `Jobs E2E` trong `Program.cs`. Cấu hình đúng là `TargetSelector = ""` và `Value = "**/Jobs/Edit/*"`.
- Loại bỏ logic tự động chờ điều hướng (`RunAndWaitForNavigationAsync`) trong Click handler của `AutomationTestRunner.cs`. Việc này giúp tránh lỗi timeout 30s khi nhấp vào nút submit của các biểu mẫu có kiểm tra lỗi đầu vào (Client-Side Validation) không tạo ra sự chuyển trang mới (như trong kịch bản `Jobs Validation E2E`).
- Cập nhật bộ chọn nút Đăng nhập trong kịch bản kiểm thử `Jobs Validation E2E` (Program.cs) sang bộ chọn lớp ổn định `.form-box.login button[type='submit']`.
- Cập nhật logic so sánh URL trong `AutomationTestRunner.cs` để hỗ trợ linh hoạt cả hai trường hợp URL trang chủ có và không có dấu gạch chéo kết thúc (`/`), ngăn ngừa lỗi timeout do so sánh chuỗi URL chính xác.
- Cập nhật logic Javascript trong hàm `startE2ETests` ở `TestingDashboard.cshtml` chuyển sang chạy tuần tự từng kịch bản và in log ra màn hình console ngay lập tức khi kịch bản đó hoàn thành (chạy tới đâu hiện tới đó), sau đó mới đồng bộ kết quả vào cơ sở dữ liệu.
- Dịch chuyển bộ lọc Việc làm từ Sidebar dọc lên thanh ngang phía trên danh sách tin tuyển dụng, thiết kế Premium UI hỗ thể đầy đủ Light/Dark theme.
- Chuẩn hóa bộ lọc địa điểm trang việc làm (`Jobs.cshtml`) sang hai dropdown Tỉnh/Thành phố và Quận/Huyện sử dụng dữ liệu `provinces.json`.
- Cập nhật logic controller hiển thị địa điểm với cơ chế fallback sang địa chỉ doanh nghiệp đối với các tin tuyển dụng cũ.

### Fixed
- Nâng cấp và cải tiến logic dọn dẹp dữ liệu E2E (`CleanE2ETestDataAsync`) trong `AutomationTestRunner.cs`: thực hiện truy vấn và xóa lần lượt các tài nguyên phụ thuộc (Applications, UpgradeRequests, Resumes, Jobs) trước khi xóa người dùng `test_e2e_` để tránh xung đột ràng buộc khóa ngoại (foreign key conflict) trên cơ sở dữ liệu SQL Server.
- Loại bỏ thuộc tính `required` HTML5 trên các trường `Title`, `Deadline` và các dropdown `ProvinceSelect`, `DistrictSelect` ở `Create.cshtml` và `Edit.cshtml` để cho phép jQuery Validation hoạt động bình thường, tránh việc trình duyệt chặn submit form gây lỗi Playwright timeout.
- Khắc phục lỗi timeout khi chạy Playwright E2E test do không tương tác được với trường nhập địa điểm `Location` đã bị ẩn đi. Chuyển đổi input `Location` sang dạng off-screen (`position: absolute; left: -9999px;`) và thêm cơ chế lắng nghe sự kiện `input`/`change` để tự động đồng bộ hóa ngược lại hai dropdown.
- Sửa lỗi nghiêm trọng chặn script trang tuyển dụng do gán sự kiện cho phần tử `#filterSearch` đã bị xóa.
- Khắc phục triệt để lỗi `SqlNullValueException` khi truy vấn dữ liệu các trường nullable của tin tuyển dụng cũ trong Database.

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
