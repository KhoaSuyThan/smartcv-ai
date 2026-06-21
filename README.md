# SmartCV AI - Hệ Thống Tạo CV & Gợi Ý Việc Làm Thông Minh

**SmartCV AI** là một nền tảng toàn diện tích hợp Trí tuệ Nhân tạo, hỗ trợ ứng viên thiết kế CV chuyên nghiệp, quản lý hồ sơ trực tuyến, đồng thời tự động kết nối và gợi ý các cơ hội việc làm phù hợp nhất từ nhà tuyển dụng dựa trên độ tương thích của kỹ năng và kinh nghiệm. 

Dự án là sự kết hợp tối ưu giữa mô hình **Monolith** mạnh mẽ (ASP.NET Core) và kiến trúc **Microservices** linh hoạt cho tác vụ xử lý ngôn ngữ tự nhiên (Python FastAPI).

---

## ✨ Các Tính Năng Nổi Bật

### 1. Trình Biên Tập CV Tương Tác Trực Quan (Interactive CV Builder)
*   **Kéo thả linh hoạt:** Sắp xếp thứ tự các mục thông tin (Kinh nghiệm, Học vấn, Kỹ năng, Chứng chỉ...) dễ dàng nhờ thư viện `vuedraggable`.
*   **Đa dạng giao diện mẫu:** Hơn 30 mẫu CV chuyên nghiệp được tối ưu hóa hiển thị (như mẫu *NguyenYenNhi*, *DinhXuanThao*, *ModernBrownProfessional*, *DaoPhuQuy*...).
*   **Xuất bản & Chia sẻ:** Hỗ trợ xuất file PDF chất lượng cao, giữ nguyên định dạng in ấn chuyên nghiệp (A4 portrait) và chế độ chia sẻ CV trực tuyến qua đường dẫn Slug bảo mật.

### 2. Gợi Ý Việc Làm Thông Minh (AI Smart Match)
*   **Xử lý ngôn ngữ tự nhiên:** Sử dụng mô hình AI chuyên dụng `sentence-transformers` viết bằng Python FastAPI để trích xuất từ khóa, phân tích nội dung CV và tin tuyển dụng.
*   **Chấm điểm tương thích:** Đánh giá mức độ phù hợp giữa ứng viên và công việc theo phần trăm độ khớp của kỹ năng, mức lương mong muốn và vị trí địa lý.

### 3. Phân Quyền & Quản Lý Hệ Thống
*   **Phân quyền chi tiết (Roles):** Quản lý luồng nghiệp vụ riêng biệt cho Ứng viên (Candidate), Nhà tuyển dụng (Recruiter), và Quản trị viên (Admin).
*   **Gói hội viên Pro (Premium Services):** Tích hợp dịch vụ nâng cấp tài khoản Pro để mở khóa các mẫu CV cao cấp và tăng giới hạn tính năng.

---

## 📂 Cấu Trúc Dự Án (Tóm Tắt)

*   `DoAnCS/` (Root): Mã nguồn chính ASP.NET Core MVC (Backend).
    *   `Controllers/`, `Models/`, `Views/`: Các thành phần điều hướng, thực thể dữ liệu và giao diện người dùng.
    *   `Services/`: Các dịch vụ xử lý logic nghiệp vụ.
    *   `wwwroot/`: Tài nguyên tĩnh (CSS, JS, hình ảnh, CV upload...).
*   `CVBuilderApp/`: Trình biên tập CV trực quan phát triển bằng Vue 3. Khi build sẽ đẩy sản phẩm vào `wwwroot/cvbuilder/`.
*   `AiMatchService/`: Dịch vụ gợi ý việc làm thông minh dựa trên AI (Python, FastAPI, Sentence-Transformers).
*   `DtbDoAnCS/` & `SQLDoAnCS.sql`: Các file script và seed dữ liệu khởi tạo cơ sở dữ liệu SQL Server.

---

## 🛠️ Yêu Cầu Hệ Thống

Trước khi bắt đầu, hãy đảm bảo máy tính của bạn đã được cài đặt:
1.  **.NET SDK 10.0** trở lên.
2.  **Node.js** (khuyên dùng LTS) để cài đặt dependency và build ứng dụng Vue.
3.  **Python 3.10+** cho dịch vụ AI.
4.  **SQL Server** làm Hệ quản trị cơ sở dữ liệu.

---

## 🚀 Hướng Dẫn Chạy Dự Án

### Bước 1: Khởi Tạo Cơ Sở Dữ Liệu
1. Mở SQL Server Management Studio (SSMS) hoặc công cụ tương đương.
2. Tạo database mới với tên: `DoAnWebCS`.
3. Thực thi file script `SQLDoAnCS.sql` ở thư mục gốc để khởi tạo bảng và dữ liệu mẫu.
4. (Tùy chọn) Thực thi thêm các script seed trong thư mục `DtbDoAnCS` nếu cần.
5. Cấu hình lại chuỗi kết nối (Connection String) trong file `appsettings.json` tại mục `DefaultConnection` sao cho khớp với tài khoản SQL Server của bạn.

---

### Bước 2: Chạy Backend ASP.NET Core
Tại thư mục gốc của dự án, mở cửa sổ dòng lệnh (Terminal/Cmd) và chạy:
```bash
# Phục hồi các package NuGet và khởi chạy server
dotnet run
```
Ứng dụng sẽ chạy ở cổng mặc định: `http://localhost:5170`

---

### Bước 3: Build & Phát Triển Trình Tạo CV (Vue 3)
*   **Phát triển cục bộ (Development):**
    ```bash
    npm run dev
    ```
*   **Biên dịch để chạy thực tế (Production):**
    Khi có bất kỳ thay đổi nào trong thư mục `CVBuilderApp`, bạn cần biên dịch lại để đẩy file tĩnh vào dự án ASP.NET Core:
    ```bash
    npm run build
    ```

---

### Bước 4: Chạy Server AI Gợi Ý Việc Làm (Python)
Tại thư mục gốc, bạn chỉ cần click đúp vào file `run_ai_server.bat` để chạy tự động.
Hoặc có thể chạy thủ công theo các lệnh sau:
```bash
cd AiMatchService
# Tạo môi trường ảo (nếu chưa có)
python -m venv venv

# Kích hoạt môi trường ảo:
# - Trên Windows CMD / PowerShell:
venv\Scripts\activate
# - Trên Git Bash / Linux / macOS:
source venv/Scripts/activate

# Cài đặt thư viện cần thiết
pip install fastapi uvicorn pydantic sentence-transformers

# Chạy server FastAPI
python main.py
```
Dịch vụ gợi ý việc làm sẽ hoạt động tại địa chỉ: `http://127.0.0.1:8000` hoặc `http://localhost:8000`

---

## 👥 Đội Ngũ Phát Triển (Authors)

Dự án được nghiên cứu và phát triển bởi:
*   **Nguyễn Võ Lê Khoa**
*   **Nguyễn Thành Nhất Nam**
*   **Trần Đức Huy**

---

## ⚠️ Tuyên bố miễn trừ trách nhiệm (Disclaimer)
Mọi thông tin liên quan đến các doanh nghiệp, tin tuyển dụng, hình ảnh và logo thương hiệu được sử dụng trong dự án này hoàn toàn là dữ liệu mẫu (mock data) được thu thập và giả lập với mục đích kiểm thử và minh họa tính năng (demo). Dự án không đại diện hoặc có bất kỳ mối quan hệ chính thức nào với các thương hiệu hoặc doanh nghiệp nói trên trong thực tế.

---

## 📝 Bản Quyền & Giấy Phép (License)
Dự án được phát triển cho mục đích Đồ án cơ sở & đồ án chuyên ngành Công nghệ thông tin. Mọi hành vi sao chép hoặc phân phối lại cho các mục đích thương mại cần có sự đồng ý bằng văn bản của đội ngũ tác giả.
