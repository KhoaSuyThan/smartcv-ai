// Cập nhật tên thời gian thực
document.getElementById('FullNameInput').addEventListener('input', function(e) {
    document.getElementById('preview-name').innerText = e.target.value || "HỌ VÀ TÊN";
});

// Cập nhật tóm tắt
document.getElementById('SummaryInput').addEventListener('input', function(e) {
    document.getElementById('preview-summary').innerText = e.target.value;
}); 