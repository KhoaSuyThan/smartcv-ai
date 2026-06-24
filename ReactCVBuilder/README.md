# 📄 React CV Builder (SmartCV AI)

Dự án con được xây dựng bằng **React + Vite** đóng vai trò làm **Trình Thiết Kế CV Trực Quan (Live CV Builder)** tích hợp bên trong hệ thống ASP.NET Core MVC.

## 🚀 Tính năng nổi bật
* **Cập nhật dữ liệu thời gian thực (Real-time Preview)**: Mọi thay đổi về thông tin cá nhân, học vấn, kinh nghiệm của người dùng trên thanh sidebar sẽ hiển thị ngay lập tức lên bản xem trước CV.
* **Tự động phân trang chuẩn A4 (A4 Auto-Pagination Engine)**: Thuật toán phân trang thông minh tự động dịch chuyển các phần tử bị tràn sang trang mới, tạo khoảng trắng lề đầu trang (top margin) và lề chân trang (bottom safe zone) chuyên nghiệp, đồng bộ 100% với giao diện xuất PDF.
* **Chống mồ côi tiêu đề (Orphan Header Prevention)**: Tự động gom nhóm tiêu đề mục đi liền với nội dung của nó, ngăn hiện tượng tiêu đề nằm ở cuối trang trước và nội dung ở đầu trang sau khi thay đổi tỉ lệ zoom hoặc chỉnh sửa dữ liệu.
* **Tích hợp sâu với ASP.NET Core**: Toàn bộ dữ liệu được tải và đồng bộ hai chiều từ cơ sở dữ liệu hệ thống thông qua các API.

## 📁 Cấu trúc thư mục chính
* `src/App.jsx`: Chứa luồng xử lý chính, thanh công cụ thiết kế (chọn phông chữ, màu sắc, căn lề, xuất PDF) và thuật toán phân trang tự động.
* `src/components/Templates/`: Thư mục chứa danh sách các mẫu CV React (ElegantAccountant, ModernBrownProfessional, ModernBlueSidebar,...).
* `vite.config.js`: Cấu hình đóng gói Vite để xuất trực tiếp các file build tĩnh (`app.js`, `styles.css`) sang thư mục `wwwroot/js/react-beta` của dự án ASP.NET Core.

## 🛠️ Hướng dẫn Phát triển & Build

### 1. Cài đặt các gói phụ thuộc
Di chuyển vào thư mục `ReactCVBuilder` và cài đặt các package:
```bash
npm install
```

### 2. Khởi chạy ở chế độ phát triển (Development Mode)
Khởi chạy server phát triển độc lập với HMR:
```bash
npm run dev
```

### 3. Biên dịch cho môi trường sản xuất (Production Build)
Biên dịch dự án React và tự động cập nhật vào dự án ASP.NET Core:
```bash
npm run build
```
*(Sau lệnh này, file `app.js` và `styles.css` mới nhất sẽ được sinh ra ở `wwwroot/js/react-beta/`)*.
