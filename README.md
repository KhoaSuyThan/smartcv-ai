# CVBuilder Pro - Nền Tảng Tạo Và Quản Lý CV Chuyên Nghiệp

CVBuilder Pro là một ứng dụng Web Fullstack hiện đại cho phép người dùng tạo, tùy biến và xuất hồ sơ xin việc (CV) chuyên nghiệp một cách nhanh chóng. Dự án tích hợp các công nghệ kéo thả (Drag & Drop), hỗ trợ AI để gợi ý nội dung và các tính năng bảo mật toàn diện cho cả ứng viên lẫn nhà tuyển dụng.

## 🚀 Tính Năng Nổi Bật

- **Trình tạo CV Trực Quan (Kéo & Thả):** Giao diện tương tác cao với Vue.js và `vuedraggable`, cho phép tùy chỉnh thứ tự các mục trong CV dễ dàng.
- **Xuất PDF chất lượng cao:** Tích hợp `jspdf` và `html2canvas` để tải CV xuống với định dạng chuẩn, giữ nguyên bố cục CSS.
- **Đăng Nhập Một Chạm (OAuth):** Hỗ trợ đăng nhập nhanh qua Google, Facebook, GitHub và LinkedIn.
- **Hỗ trợ bởi AI (Trí Tuệ Nhân Tạo):** Tích hợp Gemini AI giúp gợi ý từ khóa, tinh chỉnh mô tả công việc và kỹ năng.
- **Quản lý đa quyền (Role-based Authorization):** Phân chia rõ ràng không gian làm việc giữa Admin, Nhà Tuyển Dụng (Recruiter) và Ứng Viên (User).
- **Hệ thống Bảo Mật Tối Ưu:** 
  - Lưu trữ file nhạy cảm ở thư mục ngoài (Private Storage).
  - Giới hạn tốc độ request (Rate Limiting) và Honeypot chống Spam form.
  - Bảo vệ chống CSRF, giới hạn một phiên đăng nhập trên mỗi tài khoản.
- **Thanh toán trực tuyến:** Tích hợp cổng thanh toán payOS cho các gói tài khoản Premium/Pro.

## 💻 Tech Stack (Công Nghệ Sử Dụng)

### Backend (Server-side)
- **Framework:** ASP.NET Core MVC & Web API (.NET 10.0)
- **Database:** Microsoft SQL Server & Entity Framework Core 10.0.3
- **Bảo mật:** ASP.NET Identity, BCrypt.Net-Next, OAuth 2.0
- **Real-time:** SignalR
- **Thanh toán:** payOS v2.1.0

### Frontend (Client-side)
- **Framework:** Vue.js 3
- **Build Tool:** Vite
- **Styling:** Tailwind CSS v4 & PostCSS
- **Thư viện chính:** `vuedraggable`, `html-to-image`, `jspdf`, `html2canvas`

### Deployment & Infrastructure
- **Containerization:** Docker & Docker Compose
- **Web Server / Hosting:** Tùy chọn triển khai dễ dàng qua Nginx hoặc IIS.

## 📂 Cấu Trúc Dự Án Cơ Bản

Dự án được chia thành 2 phần chính: Backend (ASP.NET Core) và Frontend (Vue.js).

```text
DoAnWeb/
├── CVBuilderApp/          # [Frontend] Chứa toàn bộ giao diện tạo CV (Vue 3 + Vite)
│   ├── src/components/    # Các phần tử giao diện tái sử dụng
│   └── src/templates/     # Các mẫu CV có sẵn
├── Controllers/           # [Backend] Xử lý logic và API của hệ thống
├── Models/                # [Backend] Định nghĩa cấu trúc cơ sở dữ liệu
├── Views/                 # [Backend] Giao diện các trang quản lý và người dùng
└── Program.cs             # [Backend] File cấu hình và khởi chạy máy chủ
```
## 📜 Giấy Phép
Dự án này là Đồ Án Học Thuật (Academic Project). Vui lòng không sử dụng với mục đích thương mại khi chưa có sự cho phép.
---
*Được phát triển bởi Nguyễn Võ Lê Khoa, Nguyễn Thành Nhất Nam & Trần Đức Huy with ❤️.*
