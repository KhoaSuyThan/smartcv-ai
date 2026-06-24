# 🟢 Vue CV Builder App (SmartCV AI - Legacy/Alternative Builder)

Dự án con được xây dựng bằng **Vue 3 + Vite** đóng vai trò làm **Trình Thiết Kế CV (Vue-based Builder)** phục vụ cho các mẫu CV được phát triển bằng Vue.

## 🚀 Tính năng nổi bật
* **Vue 3 SFC (`<script setup>`)**: Sử dụng cú pháp lập trình Single File Component hiện đại của Vue 3 giúp mã nguồn cô đọng, hiệu năng cao và dễ bảo trì.
* **Đồng bộ hóa dữ liệu trực quan**: Cho phép người dùng chỉnh sửa thông tin CV trực tiếp trên giao diện thiết kế kéo thả và xem trước kết quả lập tức.
* **Cơ chế phân trang Vue**: Sử dụng thuật toán đo đạc DOM phân chia nội dung CV thành các trang A4 khi in hoặc xuất file PDF.

## 📁 Cấu trúc thư mục chính
* `src/main.js`: File khởi tạo và cấu hình ứng dụng Vue.
* `src/components/`: Chứa các component giao diện nhập liệu và các template CV Vue cũ.
* `vite.config.js`: Cấu hình build của Vue để tích hợp tài nguyên tĩnh vào thư mục `wwwroot` của ASP.NET.

## 🛠️ Hướng dẫn Phát triển & Build

### 1. Cài đặt các gói phụ thuộc
Di chuyển vào thư mục `CVBuilderApp` và cài đặt các package:
```bash
npm install
```

### 2. Khởi chạy ở chế độ phát triển (Development Mode)
Khởi chạy server phát triển Vue độc lập:
```bash
npm run dev
```

### 3. Biên dịch cho môi trường sản xuất (Production Build)
Biên dịch dự án và đẩy tài nguyên tĩnh sang thư mục phân phối chính:
```bash
npm run build
```
