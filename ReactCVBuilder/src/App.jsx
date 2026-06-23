import React, { useState, useEffect, useRef } from 'react'
import TemplateRegistry from './components/TemplateRegistry'
import LeftForm from './components/LeftForm'

const fonts = ['Lora', 'Inter', 'Roboto', 'Open Sans', 'Montserrat', 'Playfair Display', 'Merriweather'];
const colors = [
  '#000000', // Classic black
  '#0d6efd', // Modern blue
  '#10b981', // Emerald green
  '#ec4899', // Pink
  '#f59e0b', // Amber
  '#8b5cf6', // Violet
  '#ef4444', // Red
];
const bgColors = [
  '#ffffff', // Pure White
  '#f1f5f9', // Slate Light Gray
  '#fef3c7', // Warm Amber/Cream
  '#fde8e8', // Soft Rose
  '#e0f2fe', // Soft Sky Blue
  '#dcfce7', // Soft Emerald Green
];
  
function App() {
  const [resumeData, setResumeData] = useState({
    fullName: 'Nguyễn Văn A',
    jobTitle: 'Phát triển phần mềm',
    phone: '0123456789',
    email: 'email@example.com',
    address: 'Hà Nội',
    website: 'github.com',
    birthDate: '01/01/2000',
    summary: 'Mục tiêu nghề nghiệp...',
    avatarUrl: 'https://i.imgur.com/8Km9tLL.png',
    themeColor: '',
    fontFamily: '',
    bgColor: '',
    textAlign: '',
    visibleSections: {},
    experiences: [],
    educations: [],
    skills: [],
    languages: [],
    otherSkills: [],
    projects: [],
    activities: [],
    certifications: [],
    awards: [],
    references: [],
    hobbies: []
  });

  const [templateName, setTemplateName] = useState('Đặng Ngọc Linh'); // Default template
  const [isSaving, setIsSaving] = useState(false);
  const [lastSavedTime, setLastSavedTime] = useState(null);
  const [zoom, setZoom] = useState(0.8);
  const [activeInput, setActiveInput] = useState(null);
  const isInitialMount = useRef(true);
  const hasAppliedDefaults = useRef(false);

  useEffect(() => {
    const updateActiveInput = (e) => {
      const el = e.target;
      
      // If clicking inside the toolbar, preserve the active input state so formatting works
      const isClickInsideToolbar = el && (el.closest('[data-toolbar="true"]') || el.tagName === 'BUTTON' || el.tagName === 'SVG' || el.tagName === 'PATH');
      if (isClickInsideToolbar) {
        return;
      }

      if (el && (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA')) {
        if (el.hasAttribute('data-field')) {
          setActiveInput({
            field: el.getAttribute('data-field'),
            index: el.getAttribute('data-index') !== null ? parseInt(el.getAttribute('data-index'), 10) : null,
            subfield: el.getAttribute('data-subfield'),
            selectionStart: el.selectionStart,
            selectionEnd: el.selectionEnd
          });
        } else {
          setActiveInput(null); // Clear active formatting target when clicking unsupported inputs
        }
      } else {
        setActiveInput(null); // Clear when clicking elsewhere
      }
    };

    document.addEventListener('focusin', updateActiveInput);
    document.addEventListener('keyup', updateActiveInput);
    document.addEventListener('mouseup', updateActiveInput);
    document.addEventListener('select', updateActiveInput);

    return () => {
      document.removeEventListener('focusin', updateActiveInput);
      document.removeEventListener('keyup', updateActiveInput);
      document.removeEventListener('mouseup', updateActiveInput);
      document.removeEventListener('select', updateActiveInput);
    };
  }, []);

  // Áp dụng visibleSections mặc định theo template khi CV mới (visibleSections rỗng)
  useEffect(() => {
    if (hasAppliedDefaults.current) return;
    const currentVisible = resumeData.visibleSections || {};
    // Chỉ áp dụng khi visibleSections hoàn toàn rỗng (CV mới tạo)
    if (Object.keys(currentVisible).length === 0) {
      const entry = TemplateRegistry[templateName];
      const defaults = entry?.defaultVisibility || {};
      setResumeData(prev => ({
        ...prev,
        visibleSections: { ...defaults }
      }));
      hasAppliedDefaults.current = true;
    }
  }, [templateName]);

  // Helper functions for color brightness adjustment (primary theme color to light accent theme color)
  const hexToRgb = (hex) => {
    hex = hex.replace(/^#/, '');
    if (hex.length === 3) {
      hex = hex.split('').map(c => c + c).join('');
    }
    const num = parseInt(hex, 16);
    return {
      r: (num >> 16) & 255,
      g: (num >> 8) & 255,
      b: num & 255
    };
  };

  const rgbToHex = (r, g, b) => {
    return '#' + [r, g, b].map(x => {
      const hex = Math.max(0, Math.min(255, Math.round(x))).toString(16);
      return hex.length === 1 ? '0' + hex : hex;
    }).join('');
  };

  const adjustBrightness = (hex, percent) => {
    try {
      if (!hex) return '#ffffff';
      const { r, g, b } = hexToRgb(hex);
      if (percent < 0) {
        const factor = 1 + percent;
        return rgbToHex(r * factor, g * factor, b * factor);
      } else {
        return rgbToHex(
          r + (255 - r) * percent,
          g + (255 - g) * percent,
          b + (255 - b) * percent
        );
      }
    } catch (e) {
      return hex;
    }
  };

  useEffect(() => {
    // Check if injected by ASP.NET backend
    if (window.INITIAL_RESUME_DATA) {
      const initialData = { ...window.INITIAL_RESUME_DATA };
      if (typeof initialData.visibleSections === 'string') {
        try {
          initialData.visibleSections = JSON.parse(initialData.visibleSections);
        } catch (e) {
          console.error("Failed to parse visibleSections:", e);
          initialData.visibleSections = {};
        }
      }
      setResumeData(initialData);
    }
    if (window.INITIAL_TEMPLATE_NAME) {
      setTemplateName(window.INITIAL_TEMPLATE_NAME);
    }
  }, []);

  // Section Visibility Logic
  useEffect(() => {
    const applySectionVisibility = () => {
      const preview = document.getElementById('cv-preview-area');
      if (!preview) return;
      
      const mappings = {
        summary: ['mục tiêu', 'giới thiệu', 'summary', 'về tôi'],
        experiences: ['kinh nghiệm', 'work experience', 'experience'],
        educations: ['học vấn', 'education'],
        skills: ['kỹ năng', 'skills', 'tin học'],
        languages: ['ngoại ngữ', 'languages'],
        otherSkills: ['kỹ năng khác', 'other skills'],
        projects: ['dự án', 'projects'],
        activities: ['hoạt động', 'activities'],
        certifications: ['chứng chỉ', 'certifications'],
        awards: ['giải thưởng', 'awards', 'danh hiệu'],
        references: ['tham chiếu', 'references', 'người tham chiếu'],
        hobbies: ['sở thích', 'hobbies']
      };

      // Reset display style of all elements in the preview area
      const allElements = preview.querySelectorAll('*');
      allElements.forEach(el => {
        if (el.style.display === 'none') {
          el.style.display = '';
        }
      });

      const sectionSelectors = [
        '.section', '.sidebar-section', '.main-section', '.cv-block', 
        '.cv-info-block', '.cv-section', '.pastel-block', '.info-row', 
        '.side-section', '.content-section', '.left-block', '.right-block', 
        '.section-block', '.info-block', '.side-section-wrapper', '.cv-block-section',
        '.education-section', '.act-section', '.project-section', '.exp-section', '.awards-section'
      ];
      const selectorStr = sectionSelectors.join(', ');

      const headerSelectors = [
        'h3', 'h4', '.section-title', '.cv-side-title', '.cv-section-title', 
        '.block-title', '.left-title', '.right-title', '.side-title', 
        '.main-title', '.sidebar-title', '.cv-title'
      ];
      const headerStr = headerSelectors.join(', ');

      // Now hide disabled ones
      Object.entries(mappings).forEach(([key, keywords]) => {
        const isVisible = resumeData.visibleSections?.[key] !== false;
        if (!isVisible) {
          // Find headers that match keywords
          const headers = preview.querySelectorAll(headerStr);
          headers.forEach(header => {
            const text = header.textContent.toLowerCase();
            const matches = keywords.some(kw => text.includes(kw));
            if (matches) {
              let block = header.closest(selectorStr);
              if (!block) {
                // Parent climbing fallback
                let current = header;
                while (current && current.parentElement) {
                  const parentClass = current.parentElement.className || '';
                  if (
                    parentClass.includes('cv-main') || 
                    parentClass.includes('left-sidebar') || 
                    parentClass.includes('cv-sidebar') || 
                    parentClass.includes('right-main') || 
                    parentClass.includes('cv-classic-wrapper') || 
                    parentClass.includes('cv-wrapper') || 
                    parentClass.includes('cv-dual-wrapper') ||
                    parentClass.includes('cv-container') ||
                    parentClass.includes('accountant-cv') ||
                    parentClass.includes('body-section') ||
                    parentClass.includes('main-content')
                  ) {
                    block = current;
                    break;
                  }
                  current = current.parentElement;
                }
              }
              if (block) {
                block.style.display = 'none';
              }
            }
          });
        }
      });
    };

    // Run after a short delay to ensure DOM is fully rendered
    const timer = setTimeout(applySectionVisibility, 100);
    return () => clearTimeout(timer);
  }, [resumeData.visibleSections, resumeData, templateName]);

  // Debounced Auto-Save
  useEffect(() => {
    if (isInitialMount.current) {
      isInitialMount.current = false;
      return;
    }

    const timer = setTimeout(() => {
      handleSave();
    }, 2000); // Wait 2s after typing stops to auto-save to DB

    return () => clearTimeout(timer);
  }, [resumeData]);

  const handleSave = async () => {
    const btn = document.getElementById('btn-save-cv');
    if (btn) btn.innerText = 'Đang lưu...';
    setIsSaving(true);

    const payload = {
      ...resumeData,
      ResumeID: window.INITIAL_RESUME_DATA?.ResumeID || 0,
      TemplateID: window.INITIAL_RESUME_DATA?.TemplateID || 0,
      Title: window.INITIAL_RESUME_DATA?.Title || 'CV React Beta'
    };

    try {
      const response = await fetch('/Resume/AutoSave', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
      });
      
      const result = await response.json();
      if (result.success) {
        setLastSavedTime(new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' }));
        if (btn) {
          btn.innerText = 'Đã lưu ✓';
          btn.style.backgroundColor = '#10b981';
          setTimeout(() => {
            btn.innerText = 'Lưu CV';
            btn.style.backgroundColor = '#28a745';
          }, 2000);
        }
      } else {
        console.error("Lỗi khi lưu: " + (result.message || "Không xác định"));
      }
    } catch (err) {
      console.error("Lỗi mạng khi lưu CV:", err);
    } finally {
      setIsSaving(false);
      if (btn && btn.innerText === 'Đang lưu...') {
        btn.innerText = 'Lưu CV';
      }
    }
  };

  const handleExportPDF = async () => {
    const cvEl = document.getElementById('cv-preview-area');
    if (!cvEl) return;

    const exportBtn = document.getElementById('btn-export-pdf');
    if (exportBtn) {
      exportBtn.innerText = '⏳ Đang xuất...';
      exportBtn.disabled = true;
    }

    try {
      // 1. Lưu lại zoom cũ và đưa zoom về 1.0 để html2canvas chụp chuẩn kích thước A4 gốc (210mm x 297mm)
      const originalZoom = zoom;
      setZoom(1.0);
      
      // Đợi DOM cập nhật lại scale 100%
      await new Promise(resolve => setTimeout(resolve, 500));

      // 2. Chụp canvas bằng html2canvas
      const canvas = await window.html2canvas(cvEl, {
        scale: 2, // Đảm bảo độ sắc nét cao (retina scale)
        useCORS: true, // Hỗ trợ tải ảnh đại diện từ domain khác (nếu có)
        allowTaint: true,
        backgroundColor: '#ffffff'
      });

      // 3. Khôi phục lại zoom của người dùng
      setZoom(originalZoom);

      // 4. Tạo file PDF bằng jsPDF
      const imgData = canvas.toDataURL('image/jpeg', 1.0);
      const { jsPDF } = window.jspdf;
      const pdf = new jsPDF('p', 'mm', 'a4');
      const imgWidth = 210; // Chiều rộng trang A4 (mm)
      const pageHeight = 297; // Chiều cao trang A4 (mm)
      const imgHeight = (canvas.height * imgWidth) / canvas.width;
      
      let heightLeft = imgHeight;
      let position = 0;

      // Trang 1
      pdf.addImage(imgData, 'JPEG', 0, position, imgWidth, imgHeight);
      heightLeft -= pageHeight;

      // Các trang tiếp theo nếu nội dung CV dài hơn 1 trang A4
      while (heightLeft >= 0) {
        position = heightLeft - imgHeight;
        pdf.addPage();
        pdf.addImage(imgData, 'JPEG', 0, position, imgWidth, imgHeight);
        heightLeft -= pageHeight;
      }

      // 5. Tải file về máy người dùng
      const fileName = `${resumeData.fullName || 'CV'}_${resumeData.jobTitle || 'Builder'}.pdf`;
      pdf.save(fileName);

      // 6. Ghi log export lên server để đổi trạng thái IsDraft = false
      try {
        await fetch(`/Resume/LogExport?resumeId=${window.INITIAL_RESUME_DATA?.ResumeID || 0}`, {
          method: 'POST'
        });
      } catch (logErr) {
        console.error("Lỗi ghi log export:", logErr);
      }

    } catch (err) {
      console.error("Lỗi xuất PDF:", err);
      alert("Đã xảy ra lỗi trong quá trình xuất PDF. Vui lòng thử lại!");
    } finally {
      if (exportBtn) {
        exportBtn.innerText = 'Xuất PDF';
        exportBtn.disabled = false;
      }
    }
  };

  const handleFormatText = (tag) => {
    if (!activeInput) {
      alert("Vui lòng click chọn và bôi đen phần văn bản trong các ô mô tả (Mô tả công việc, Mô tả dự án, Hoạt động hoặc Mục tiêu nghề nghiệp) ở cột trái trước khi định dạng!");
      return;
    }

    const { field, index, subfield, selectionStart, selectionEnd } = activeInput;

    // Lấy text hiện tại từ resumeData
    let text = '';
    if (index === null) {
      text = resumeData[field] || '';
    } else {
      text = resumeData[field]?.[index]?.[subfield] || '';
    }

    const start = selectionStart;
    const end = selectionEnd;
    const selectedText = text.substring(start, end);

    let formatted = '';
    if (tag === 'ul' || tag === 'ol') {
      const lines = selectedText.split('\n').filter(l => l.trim() !== '');
      if (lines.length === 0) {
        formatted = `<${tag}>\n  <li>${selectedText || 'Mục mới'}</li>\n</${tag}>`;
      } else {
        formatted = `<${tag}>\n${lines.map(line => `  <li>${line}</li>`).join('\n')}\n</${tag}>`;
      }
    } else {
      formatted = `<${tag}>${selectedText}</${tag}>`;
    }

    const newText = text.substring(0, start) + formatted + text.substring(end);

    // Cập nhật resumeData
    if (index === null) {
      setResumeData(prev => ({
        ...prev,
        [field]: newText
      }));
    } else {
      setResumeData(prev => {
        const arr = [...(prev[field] || [])];
        arr[index] = {
          ...arr[index],
          [subfield]: newText
        };
        return { ...prev, [field]: arr };
      });
    }

    // Cập nhật lại vùng chọn activeInput mới sau khi chèn thẻ HTML
    setActiveInput(prev => ({
      ...prev,
      selectionStart: start,
      selectionEnd: start + formatted.length
    }));

    // Khôi phục focus vào đúng phần tử trong DOM thực tế sau khi component re-render
    setTimeout(() => {
      let selector = `[data-field="${field}"]`;
      if (index !== null) {
        selector += `[data-index="${index}"]`;
      }
      if (subfield) {
        selector += `[data-subfield="${subfield}"]`;
      }
      const el = document.querySelector(selector);
      if (el) {
        el.focus();
        el.setSelectionRange(start, start + formatted.length);
      }
    }, 50);
  };

  const SelectedTemplate = TemplateRegistry[templateName]?.component;

  return (
    <div style={{ display: 'flex', width: '100%', height: '100%', fontFamily: 'Arial, sans-serif', overflow: 'hidden' }}>
      {/* CỘT TRÁI - FORM */}
      <div style={{ width: '500px', backgroundColor: '#f8f9fa', borderRight: '1px solid #ddd', display: 'flex', flexDirection: 'column', flexShrink: 0 }}>
        <div style={{ padding: '15px', backgroundColor: '#343a40', color: 'white', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h2 style={{ margin: 0, fontSize: '16px' }}>📝 Chỉnh sửa CV (Beta)</h2>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            {lastSavedTime && (
              <span style={{ fontSize: '11px', color: '#a7f3d0' }}>
                Tự động lưu: {lastSavedTime}
              </span>
            )}
            <button 
              id="btn-save-cv"
              onClick={handleSave} 
              disabled={isSaving}
              style={{ padding: '8px 15px', backgroundColor: '#28a745', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}
            >
              {isSaving ? 'Đang lưu...' : 'Lưu CV'}
            </button>
          </div>
        </div>
        
        <div style={{ padding: '15px', borderBottom: '1px solid #ddd' }}>
          <label style={{ fontWeight: 'bold', fontSize: '13px' }}>Đổi mẫu (Test): </label>
          <select value={templateName} onChange={(e) => setTemplateName(e.target.value)} style={{ padding: '5px', width: '200px' }}>
            {Object.keys(TemplateRegistry).map(key => (
              <option key={key} value={key}>{key}</option>
            ))}
          </select>
        </div>

        <div style={{ flex: 1, overflowY: 'auto', padding: '20px' }}>
          <LeftForm resumeData={resumeData} setResumeData={setResumeData} />
        </div>
      </div>

      {/* CỘT PHẢI - PREVIEW PANEL */}
      <div style={{ flex: 1, backgroundColor: '#525659', display: 'flex', flexDirection: 'column', height: '100%', overflow: 'hidden' }}>
        
        {/* TOOLBAR */}
        <div 
          data-toolbar="true"
          style={{
          width: '100%',
          backgroundColor: '#ffffff',
          borderBottom: '1px solid #e2e8f0',
          padding: '12px 24px',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          zIndex: 10,
          boxShadow: '0 2px 8px rgba(0,0,0,0.08)',
          flexShrink: 0
        }}>
          {/* Trái: Font & Color & Alignments */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '20px', flexWrap: 'wrap' }}>
            {/* Font */}
            <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <span style={{ fontSize: '11px', fontWeight: 'bold', color: '#475569', textTransform: 'uppercase', letterSpacing: '0.5px' }}>Font:</span>
              <select 
                value={resumeData.fontFamily || ''} 
                onChange={(e) => setResumeData(prev => ({ ...prev, fontFamily: e.target.value }))}
                style={{ padding: '5px 10px', borderRadius: '6px', border: '1px solid #cbd5e1', fontSize: '12px', backgroundColor: '#fff', cursor: 'pointer', outline: 'none', fontWeight: '500' }}
              >
                <option value="">Gốc (Template)</option>
                {fonts.map(f => <option key={f} value={f}>{f}</option>)}
              </select>
            </div>

            {/* Màu chủ đạo */}
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px', borderLeft: '1px solid #e2e8f0', paddingLeft: '16px' }}>
              <span style={{ fontSize: '11px', fontWeight: 'bold', color: '#475569', textTransform: 'uppercase', letterSpacing: '0.5px' }}>Màu chữ (Màu chính):</span>
              <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                <button
                  onClick={() => setResumeData(prev => ({ ...prev, themeColor: '' }))}
                  style={{
                    padding: '2px 8px',
                    fontSize: '11px',
                    borderRadius: '4px',
                    border: !resumeData.themeColor ? '2px solid #2563eb' : '1px solid #cbd5e1',
                    backgroundColor: !resumeData.themeColor ? '#eff6ff' : '#f8f9fa',
                    color: !resumeData.themeColor ? '#2563eb' : '#475569',
                    cursor: 'pointer',
                    fontWeight: 'bold',
                    height: '20px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    boxShadow: !resumeData.themeColor ? '0 1px 2px rgba(37, 99, 235, 0.1)' : 'none',
                    marginRight: '4px'
                  }}
                  title="Giữ màu gốc của mẫu CV"
                >
                  Mặc định
                </button>
                {colors.map(c => (
                  <button
                    key={c}
                    onClick={() => setResumeData(prev => ({ ...prev, themeColor: c }))}
                    style={{
                      width: '20px',
                      height: '20px',
                      borderRadius: '50%',
                      backgroundColor: c,
                      border: (resumeData.themeColor || '').toLowerCase() === c.toLowerCase() ? '2px solid #2563eb' : '1px solid #94a3b8',
                      cursor: 'pointer',
                      padding: 0,
                      boxShadow: '0 1px 3px rgba(0,0,0,0.15)',
                      transition: 'transform 0.1s'
                    }}
                    title={c}
                  />
                ))}
                <input 
                  type="color" 
                  value={resumeData.themeColor || '#000000'} 
                  onChange={(e) => setResumeData(prev => ({ ...prev, themeColor: e.target.value }))}
                  style={{ width: '22px', height: '22px', padding: 0, border: 'none', borderRadius: '4px', cursor: 'pointer', backgroundColor: 'transparent' }}
                  title="Chọn màu tự do"
                />
              </div>
            </div>

            {/* Màu nền CV */}
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px', borderLeft: '1px solid #e2e8f0', paddingLeft: '16px' }}>
              <span style={{ fontSize: '11px', fontWeight: 'bold', color: '#475569', textTransform: 'uppercase', letterSpacing: '0.5px' }}>Màu nền (Nền CV):</span>
              <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                <button
                  onClick={() => setResumeData(prev => ({ ...prev, bgColor: '' }))}
                  style={{
                    padding: '2px 8px',
                    fontSize: '11px',
                    borderRadius: '4px',
                    border: !resumeData.bgColor ? '2px solid #2563eb' : '1px solid #cbd5e1',
                    backgroundColor: !resumeData.bgColor ? '#eff6ff' : '#f8f9fa',
                    color: !resumeData.bgColor ? '#2563eb' : '#475569',
                    cursor: 'pointer',
                    fontWeight: 'bold',
                    height: '20px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    boxShadow: !resumeData.bgColor ? '0 1px 2px rgba(37, 99, 235, 0.1)' : 'none',
                    marginRight: '4px'
                  }}
                  title="Giữ màu nền gốc của mẫu CV"
                >
                  Mặc định
                </button>
                {bgColors.map(c => (
                  <button
                    key={c}
                    onClick={() => setResumeData(prev => ({ ...prev, bgColor: c }))}
                    style={{
                      width: '20px',
                      height: '20px',
                      borderRadius: '4px',
                      backgroundColor: c,
                      border: (resumeData.bgColor || '').toLowerCase() === c.toLowerCase() ? '2px solid #2563eb' : '1px solid #94a3b8',
                      cursor: 'pointer',
                      padding: 0,
                      boxShadow: 'inset 0 1px 1px rgba(0,0,0,0.08), 0 1px 2px rgba(0,0,0,0.1)',
                      transition: 'transform 0.1s'
                    }}
                    title={c}
                  />
                ))}
                <input 
                  type="color" 
                  value={resumeData.bgColor || '#ffffff'} 
                  onChange={(e) => setResumeData(prev => ({ ...prev, bgColor: e.target.value }))}
                  style={{ width: '22px', height: '22px', padding: 0, border: 'none', borderRadius: '4px', cursor: 'pointer', backgroundColor: 'transparent' }}
                  title="Chọn màu nền tự do"
                />
              </div>
            </div>

            {/* Căn lề */}
            <div style={{ display: 'flex', alignItems: 'center', gap: '6px', borderLeft: '1px solid #e2e8f0', paddingLeft: '16px' }}>
              <span style={{ fontSize: '11px', fontWeight: 'bold', color: '#475569', textTransform: 'uppercase', letterSpacing: '0.5px' }}>Căn lề:</span>
              <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                <button
                  onClick={() => setResumeData(prev => ({ ...prev, textAlign: '' }))}
                  style={{
                    padding: '2px 8px',
                    fontSize: '11px',
                    borderRadius: '4px',
                    border: !resumeData.textAlign ? '2px solid #2563eb' : '1px solid #cbd5e1',
                    backgroundColor: !resumeData.textAlign ? '#eff6ff' : '#f8f9fa',
                    color: !resumeData.textAlign ? '#2563eb' : '#475569',
                    cursor: 'pointer',
                    fontWeight: 'bold',
                    height: '20px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    boxShadow: !resumeData.textAlign ? '0 1px 2px rgba(37, 99, 235, 0.1)' : 'none',
                    marginRight: '4px'
                  }}
                  title="Giữ căn lề gốc của mẫu CV"
                >
                  Mặc định
                </button>
                <div style={{ display: 'flex', gap: '2px', backgroundColor: '#f1f5f9', padding: '2px', borderRadius: '6px', border: '1px solid #cbd5e1' }}>
                  {[
                    { value: 'left', label: 'Trái', icon: 'M4 6h16M4 12h10M4 18h16' },
                    { value: 'center', label: 'Giữa', icon: 'M4 6h16M7 12h10M4 18h16' },
                    { value: 'right', label: 'Phải', icon: 'M4 6h16M10 12h10M4 18h16' },
                    { value: 'justify', label: 'Đều', icon: 'M4 6h16M4 12h16M4 18h16' }
                  ].map(align => (
                    <button
                      key={align.value}
                      onClick={() => setResumeData(prev => ({ ...prev, textAlign: align.value }))}
                      style={{
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        width: '24px',
                        height: '24px',
                        border: 'none',
                        borderRadius: '4px',
                        backgroundColor: resumeData.textAlign === align.value ? '#ffffff' : 'transparent',
                        color: resumeData.textAlign === align.value ? '#2563eb' : '#64748b',
                        cursor: 'pointer',
                        boxShadow: resumeData.textAlign === align.value ? '0 1px 3px rgba(0,0,0,0.1)' : 'none',
                        transition: 'all 0.2s',
                        padding: 0
                      }}
                      title={`Căn ${align.label}`}
                    >
                      <svg width="12" height="12" fill="none" stroke="currentColor" strokeWidth="2.5" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" d={align.icon} />
                      </svg>
                    </button>
                  ))}
                </div>
              </div>
            </div>

            {/* Định dạng */}
            <div style={{ display: 'flex', alignItems: 'center', gap: '6px', borderLeft: '1px solid #e2e8f0', paddingLeft: '16px' }}>
              <span style={{ fontSize: '11px', fontWeight: 'bold', color: '#475569', textTransform: 'uppercase', letterSpacing: '0.5px' }}>Định dạng:</span>
              <div style={{ display: 'flex', gap: '2px', backgroundColor: '#f1f5f9', padding: '2px', borderRadius: '6px', border: '1px solid #cbd5e1' }}>
                {[
                  { value: 'b', label: 'In đậm', icon: 'M6 4h8a4 4 0 0 1 4 4 4 4 0 0 1-3 3.87A4 4 0 0 1 19 16a4 4 0 0 1-4 4H6V4z' },
                  { value: 'i', label: 'In nghiêng', icon: 'M19 4h-9M14 4l-4 16M8 20h6' },
                  { value: 'u', label: 'Gạch chân', icon: 'M6 3v7a6 6 0 0 0 12 0V3M4 21h16' },
                  { value: 'ul', label: 'Dấu mục', icon: 'M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01' },
                  { value: 'ol', label: 'Đánh số', icon: 'M10 6h11M10 12h11M10 18h11M4 6h1v4M3 10h2M3 14h3v2H3v2h3' }
                ].map(format => (
                  <button
                    key={format.value}
                    onMouseDown={(e) => {
                      e.preventDefault();
                      handleFormatText(format.value);
                    }}
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      width: '24px',
                      height: '24px',
                      border: 'none',
                      borderRadius: '4px',
                      backgroundColor: 'transparent',
                      color: '#64748b',
                      cursor: 'pointer',
                      transition: 'all 0.2s',
                      padding: 0
                    }}
                    title={format.label}
                  >
                    <svg width="12" height="12" fill="none" stroke="currentColor" strokeWidth="2.5" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" d={format.icon} />
                    </svg>
                  </button>
                ))}
              </div>
            </div>
          </div>

          {/* Phải: Zoom & Export */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '16px', flexShrink: 0 }}>
            {/* Zoom */}
            <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
              <button 
                onClick={() => setZoom(z => Math.max(0.5, z - 0.1))}
                style={{ width: '26px', height: '26px', borderRadius: '50%', border: '1px solid #cbd5e1', display: 'flex', alignItems: 'center', justifyContent: 'center', cursor: 'pointer', backgroundColor: '#fff', fontWeight: 'bold', outline: 'none' }}
              >-</button>
              <span style={{ fontSize: '12px', minWidth: '40px', textAlign: 'center', fontWeight: 'bold', color: '#475569' }}>
                {Math.round(zoom * 100)}%
              </span>
              <button 
                onClick={() => setZoom(z => Math.min(1.5, z + 0.1))}
                style={{ width: '26px', height: '26px', borderRadius: '50%', border: '1px solid #cbd5e1', display: 'flex', alignItems: 'center', justifyContent: 'center', cursor: 'pointer', backgroundColor: '#fff', fontWeight: 'bold', outline: 'none' }}
              >+</button>
            </div>

            {/* Xuất PDF */}
            <button
              id="btn-export-pdf"
              onClick={handleExportPDF}
              style={{
                padding: '6px 14px',
                backgroundColor: '#3b82f6',
                color: '#fff',
                border: 'none',
                borderRadius: '6px',
                cursor: 'pointer',
                fontWeight: 'bold',
                fontSize: '12px',
                display: 'flex',
                alignItems: 'center',
                gap: '6px',
                boxShadow: '0 4px 6px -1px rgba(59, 130, 246, 0.2)',
                outline: 'none',
                transition: 'all 0.2s'
              }}
            >
              📥 Xuất PDF
            </button>
          </div>
        </div>

        {/* VÙNG CHỨA PREVIEW (SCROLLABLE) */}
        <div style={{ flex: 1, width: '100%', overflowY: 'auto', display: 'flex', justifyContent: 'center', padding: '40px 0' }}>
          {/* Override Styles bằng cách chèn thẻ style động */}
          <style>{`
            ${resumeData.fontFamily ? `
              #cv-preview-area, #cv-preview-area * {
                font-family: "${resumeData.fontFamily}", "Segoe UI", sans-serif !important;
              }
            ` : ''}

            ${resumeData.textAlign ? `
            #cv-preview-area .summary-text,
            #cv-preview-area .desc-text,
            #cv-preview-area .html-content,
            #cv-preview-area p,
            #cv-preview-area .exp-desc,
            #cv-preview-area .summary-section,
            #cv-preview-area .cv-section-content {
              text-align: ${resumeData.textAlign} !important;
            }
            ` : ''}
            
            ${resumeData.themeColor ? `
              #cv-preview-area h3, 
              #cv-preview-area .section-title, 
              #cv-preview-area .fullname, 
              #cv-preview-area i, 
              #cv-preview-area .contact-info i,
              #cv-preview-area [class*="title"],
              #cv-preview-area [class*="name"],
              #cv-preview-area .job-title,
              #cv-preview-area .role {
                color: ${resumeData.themeColor} !important;
              }
              #cv-preview-area h3, 
              #cv-preview-area .section-title,
              #cv-preview-area [class*="title"] {
                border-color: ${resumeData.themeColor} !important;
              }
            ` : ''}
            
            ${resumeData.bgColor ? `
              #cv-preview-area,
              #cv-preview-area .cv-container,
              #cv-preview-area .cv-main,
              #cv-preview-area main,
              #cv-preview-area .right-main,
              #cv-preview-area .cv-classic-wrapper,
              #cv-preview-area .cv-wrapper,
              #cv-preview-area .cv-modern-wrapper,
              #cv-preview-area .cv-yens-wrapper,
              #cv-preview-area .brown-cv {
                background-color: ${resumeData.bgColor} !important;
              }

              /* Sidebar background lightened tint overrides */
              #cv-preview-area .cv-sidebar,
              #cv-preview-area .sidebar,
              #cv-preview-area .left-sidebar,
              #cv-preview-area .left-column {
                background-color: ${adjustBrightness(resumeData.bgColor, 0.95)} !important;
              }

              /* Templates with dark sidebars (e.g. NgoHaiYen, ModernProfessionalSplit) */
              #cv-preview-area .cv-yens-wrapper .cv-sidebar,
              #cv-preview-area .cv-template-ModernProfessionalSplit .left-sidebar {
                background-color: ${adjustBrightness(resumeData.bgColor, 0.9)} !important;
              }

              /* Header banners and decorative shapes */
              #cv-preview-area .header-box,
              #cv-preview-area .header-brown,
              #cv-preview-area .dark-header,
              #cv-preview-area .main-header-banner,
              #cv-preview-area .top-header-banner,
              #cv-preview-area .shape-top-left,
              #cv-preview-area .shape-top-left-edge,
              #cv-preview-area .shape-bottom-right,
              #cv-preview-area .shape-bottom-right-edge {
                background-color: ${adjustBrightness(resumeData.bgColor, 0.85)} !important;
              }

              /* Pastel blocks or badges */
              #cv-preview-area .pastel-block {
                background-color: ${adjustBrightness(resumeData.bgColor, 0.9)} !important;
              }

              /* Bullet list backgrounds or timeline dots */
              #cv-preview-area .exp-year,
              #cv-preview-area .date-badge,
              #cv-preview-area .timeline-area .exp-item::after,
              #cv-preview-area .act-area .exp-item::after {
                background-color: ${adjustBrightness(resumeData.bgColor, 0.8)} !important;
              }

              /* Skill meters */
              #cv-preview-area .skill-bar-fill,
              #cv-preview-area .skill-meter-fill {
                background-color: ${adjustBrightness(resumeData.bgColor, 0.75)} !important;
              }
            ` : ''}
          `}</style>

          {/* Khung chứa CV đã được zoom */}
          <div id="cv-preview-area" style={{ 
              boxShadow: '0 10px 30px rgba(0,0,0,0.35)', 
              backgroundColor: 'white',
              width: '210mm',
              minHeight: '297mm',
              transform: `scale(${zoom})`,
              transformOrigin: 'top center',
              marginBottom: `calc(297mm * (${zoom} - 1))`,
              flexShrink: 0,
              transition: 'transform 0.15s ease-out'
          }}>
            {SelectedTemplate ? <SelectedTemplate resumeData={resumeData} /> : <div style={{padding: 50}}>Không tìm thấy mẫu CV</div>}
          </div>
        </div>

      </div>
    </div>
  )
}

export default App
