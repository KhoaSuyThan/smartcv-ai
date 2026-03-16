// Hàm hỗ trợ cập nhật text an toàn
function safeSetInnerText(id, value) {
    let element = document.getElementById(id);
    if (element) {
        element.innerText = value;
    }
}

// Cập nhật tên thời gian thực
let nameInput = document.getElementById('FullNameInput');
if (nameInput) {
    nameInput.addEventListener('input', function(e) {
        safeSetInnerText('preview-name', e.target.value || "HỌ VÀ TÊN");
        // Đánh dấu có thay đổi để Auto-save biết mà lưu
        if (typeof isDirty !== 'undefined') isDirty = true; 
    });
}

// Cập nhật tóm tắt
let summaryInput = document.getElementById('SummaryInput');
if (summaryInput) {
    summaryInput.addEventListener('input', function(e) {
        safeSetInnerText('preview-summary', e.target.value);
        if (typeof isDirty !== 'undefined') isDirty = true;
    });
}