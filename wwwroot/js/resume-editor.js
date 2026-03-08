// ==========================================
// 1. ĐỒNG BỘ DỮ LIỆU REAL-TIME
// ==========================================

// Cập nhật Họ tên
document.getElementById('FullNameInput')?.addEventListener('input', (e) => {
    const nameDisplay = document.getElementById('preview-name');
    if (nameDisplay) nameDisplay.innerText = e.target.value.toUpperCase() || "HỌ VÀ TÊN";
});

// Cập nhật Summary (Tóm tắt)
document.getElementById('SummaryInput')?.addEventListener('input', (e) => {
    const summaryDisplay = document.getElementById('preview-summary');
    if (summaryDisplay) summaryDisplay.innerText = e.target.value || "Giới thiệu bản thân của bạn sẽ hiển thị tại đây...";
});

// ==========================================
// 2. QUẢN LÝ KINH NGHIỆM (EXPERIENCES)
// ==========================================
let expCount = 0;

function addExperience() {
    const id = expCount++;

    // Tạo HTML cho Cột trái (Nhập liệu)
    const editorHtml = `
        <div class="experience-item mb-4 border-bottom pb-3" id="exp-editor-${id}">
            <div class="d-flex justify-content-between align-items-center mb-2">
                <h6 class="text-primary mb-0">Kinh nghiệm #${id + 1}</h6>
                <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeExperience(${id})">
                    <i class="fas fa-trash"></i> Xóa
                </button>
            </div>
            <input type="text" class="form-control mb-2 company-input" placeholder="Tên công ty" oninput="syncExperience(${id})">
            <input type="text" class="form-control mb-2 position-input" placeholder="Vị trí (Ví dụ: Intern .NET)" oninput="syncExperience(${id})">
            <textarea class="form-control desc-input" rows="3" placeholder="Mô tả công việc..." oninput="syncExperience(${id})"></textarea>
        </div>
    `;
    document.getElementById('experience-editor-list').insertAdjacentHTML('beforeend', editorHtml);

    // Tạo HTML cho Cột phải (Xem trước trên A4)
    const previewHtml = `
        <div class="cv-item mb-3" id="exp-preview-${id}">
            <div class="d-flex justify-content-between">
                <strong class="preview-company text-dark">Tên công ty</strong>
                <em class="preview-position text-muted">Vị trí</em>
            </div>
            <p class="preview-desc small mb-0" style="white-space: pre-line;">Mô tả công việc của bạn...</p>
        </div>
    `;
    document.getElementById('experience-preview-list').insertAdjacentHTML('beforeend', previewHtml);
}

function syncExperience(id) {
    const editor = document.getElementById(`exp-editor-${id}`);
    const preview = document.getElementById(`exp-preview-${id}`);

    if (editor && preview) {
        const company = editor.querySelector('.company-input').value;
        const position = editor.querySelector('.position-input').value;
        const desc = editor.querySelector('.desc-input').value;

        preview.querySelector('.preview-company').innerText = company || "Tên công ty";
        preview.querySelector('.preview-position').innerText = position || "Vị trí";
        preview.querySelector('.preview-desc').innerText = desc || "Mô tả công việc của bạn...";
    }
}

function removeExperience(id) {
    document.getElementById(`exp-editor-${id}`)?.remove();
    document.getElementById(`exp-preview-${id}`)?.remove();
}

// ==========================================
// 3. LƯU DỮ LIỆU VỀ SERVER
// ==========================================
async function saveResume() {
    console.log("Bắt đầu quy trình lưu...");
    
    // Thu thập danh sách kinh nghiệm từ các class chung
    const experiences = Array.from(document.querySelectorAll('.experience-item')).map(item => {
        return {
            Company: item.querySelector('.company-input').value,
            Position: item.querySelector('.position-input').value,
            Description: item.querySelector('.desc-input').value
        };
    });

    // Đóng gói dữ liệu gửi đi
    const resumeData = {
        Title: document.getElementById('ResumeTitle')?.value || "CV chưa đặt tên",
        Summary: document.getElementById('SummaryInput')?.value || "",
        TemplateID: 1, // Bạn có thể thêm input chọn template sau
        ThemeColor: "#3498db",
        Experiences: experiences,
        Educations: [], // Tương tự như Experiences
        Skills: []      // Tương tự như Experiences
    };

    try {
        const response = await fetch('/Resume/SaveResume', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(resumeData)
        });

        // Kiểm tra nếu HTTP status không phải 200-299
        if (!response.ok) {
            throw new Error(`Server trả về lỗi: ${response.status}`);
        }

        const result = await response.json();

        if (result.success) {
            // Sử dụng SweetAlert2 nếu bạn đã thêm thư viện, không thì dùng alert thường
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'Thành công!',
                    text: result.message,
                    icon: 'success',
                    confirmButtonText: 'Tuyệt vời'
                });
            } else {
                alert("Thành công: " + result.message);
            }
        } else {
            alert("Lỗi từ hệ thống: " + result.message);
        }

    } catch (error) {
        console.error("Chi tiết lỗi kết nối:", error);
        alert("Không thể kết nối đến máy chủ hoặc phản hồi không đúng định dạng JSON.");
    }
}