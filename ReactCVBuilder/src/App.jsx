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
const isHtmlEmpty = (str) => {
  if (!str) return true;
  const clean = str.replace(/<[^>]*>/g, '').replace(/&nbsp;/g, '').trim();
  return clean === '';
};

const preprocessResumeData = (data) => {
  if (!data) return data;
  const processed = { ...data };
  
  // Clean basic text fields so that empty html strings (e.g. <br>) are normalized to empty string
  const textFields = ['fullName', 'jobTitle', 'email', 'phone', 'birthDate', 'address', 'website', 'summary'];
  textFields.forEach(field => {
    if (processed[field] !== undefined && isHtmlEmpty(processed[field])) {
      processed[field] = '';
    }
  });

  return processed;
};

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
  const [showTemplateSelector, setShowTemplateSelector] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [lastSavedTime, setLastSavedTime] = useState(null);
  const [zoom, setZoom] = useState(0.8);
  const [pageCount, setPageCount] = useState(1);
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

      if (el && (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA' || el.classList.contains('rich-text-editor') || el.hasAttribute('contenteditable'))) {
        if (el.hasAttribute('data-field')) {
          setActiveInput({
            el: el,
            field: el.getAttribute('data-field'),
            index: el.getAttribute('data-index') !== null ? parseInt(el.getAttribute('data-index'), 10) : null,
            subfield: el.getAttribute('data-subfield')
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

  // Pagination logic to mirror A4 print layout in Live Preview
  useEffect(() => {
    const cvRoot = document.getElementById('cv-preview-area');
    if (!cvRoot) return;

    let paginateTimer = null;

    const requestPagination = () => {
      if (paginateTimer) clearTimeout(paginateTimer);
      paginateTimer = setTimeout(doPagination, 150); // Small delay to let React DOM update
    };

    const doPagination = () => {
      const cvRoot = document.getElementById('cv-preview-area');
      if (!cvRoot) return;

      // 1. Reset all previously set margin-tops on all descendants to measure natural flow
      const allDescendants = cvRoot.getElementsByTagName('*');
      for (let i = 0; i < allDescendants.length; i++) {
        const el = allDescendants[i];
        if (el.style.marginTop) {
          el.style.marginTop = '';
        }
      }

      // Reset height to let container expand naturally
      cvRoot.style.height = 'auto';

      const offsetW = cvRoot.offsetWidth; // Layout width in pixels
      if (!offsetW) return;

      const cvRect = cvRoot.getBoundingClientRect();
      const scale = cvRect.width / offsetW;

      const A4_WIDTH_MM = 210;
      const A4_HEIGHT_MM = 297;
      const pxPerMm = offsetW / A4_WIDTH_MM;
      const pageH = A4_HEIGHT_MM * pxPerMm;

      // Margins/Safe zones for page splitting (exactly like Vue template calculations)
      const bottomSafeZone = 14 * pxPerMm; 
      const topMargin = 16 * pxPerMm;      

      // 2. Select block-level layout elements that should not cross pages
      const selectors = [
        'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
        '.header-section', '.header-brown', '.header-blue', '.header-classic',
        '.personal-info', '.contact-info', '.contact-list', '.contact-item',
        '.summary-text', '.summary-section', '.exp-item', 'li', 'p',
        '.skill-item-block', '.skill-dot-item', '.cert-year-div', '.cert-name-div',
        '.cv-block > div', '.cv-section-content > div', '.info-row'
      ].join(', ');

      let items = Array.from(cvRoot.querySelectorAll(selectors)).filter(el => {
        // Ignore hidden elements
        if (el.offsetHeight === 0) return false;
        
        // Ignore nested items (we only paginate the highest level containers to avoid breaking nested structures)
        let parent = el.parentElement;
        while (parent && parent !== cvRoot) {
          // If the parent matches selectors, but is NOT .pastel-block, we filter this element out
          if (parent.matches(selectors) && !parent.classList.contains('pastel-block')) return false;
          parent = parent.parentElement;
        }
        return true;
      });

      // 3. Paginate items by shifting elements down if they cross A4 boundary
      let stable = false;
      let passes = 0;

      while (!stable && passes < 30) {
        stable = true;
        passes++;

        const currentCvRect = cvRoot.getBoundingClientRect();

        for (let i = 0; i < items.length; i++) {
          const el = items[i];
          if (el.offsetHeight === 0) continue;

          const elRect = el.getBoundingClientRect();
          const top = (elRect.top - currentCvRect.top) / scale;
          const height = elRect.height / scale;

          const pageIndex = Math.floor(top / pageH);
          const topInPage = top - (pageIndex * pageH);
          const bottomInPage = topInPage + height;

          // Rule A: If the element is on page 2+ (pageIndex > 0) and is too close to the top of the page
          if (pageIndex > 0 && topInPage < topMargin) {
            const distToTopMargin = topMargin - topInPage;
            const currentMt = parseFloat(el.style.marginTop || '0');
            el.style.setProperty('margin-top', `${currentMt + distToTopMargin}px`, 'important');
            stable = false;
            break; // Restart calculation with new margins
          }

          // Rule B: If the element fits in a single page but overflows the current page
          if (height <= (pageH - bottomSafeZone - topMargin)) {
            if (bottomInPage > (pageH - bottomSafeZone)) {
              // Check if we should push its previous sibling instead (header grouping)
              let targetEl = el;
              const prev = el.previousElementSibling;
              
              // If the previous sibling is a header/title element, push it instead of this content block
              if (prev && (
                prev.matches('h1, h2, h3, h4, h5, h6, .block-title, .section-title, [class*="title"], [class*="header"]')
              )) {
                targetEl = prev;
              }

              // Custom check for Pastel Beige Blocks: if targetEl is the first element in a .pastel-block, push the entire block
              const pastelBlock = targetEl.closest('.pastel-block');
              if (pastelBlock) {
                const innerItems = Array.from(pastelBlock.querySelectorAll(selectors));
                if (innerItems.length > 0 && (innerItems[0] === targetEl || innerItems[0] === el)) {
                  targetEl = pastelBlock;
                }
              }

              const targetRect = targetEl.getBoundingClientRect();
              const targetTop = (targetRect.top - currentCvRect.top) / scale;
              const targetTopInPage = targetTop - (pageIndex * pageH);

              const distToNextPage = pageH - targetTopInPage + topMargin;
              const currentMt = parseFloat(targetEl.style.marginTop || '0');
              targetEl.style.setProperty('margin-top', `${currentMt + distToNextPage}px`, 'important');
              stable = false;
              break; // Restart calculation with new margins
            }
          }
        }
      }

      // 4. Calculate total page count and apply to container height
      const finalCvRect = cvRoot.getBoundingClientRect();
      let maxBottom = 0;
      items.forEach(el => {
        const bottom = (el.getBoundingClientRect().bottom - finalCvRect.top) / scale;
        if (bottom > maxBottom) maxBottom = bottom;
      });

      const finalPageCount = Math.max(1, Math.ceil(maxBottom / pageH));
      cvRoot.style.height = `${finalPageCount * 297}mm`;
      setPageCount(finalPageCount);
    };

    // Initial run
    requestPagination();

    // Listeners for layout recalculation
    window.addEventListener('resize', requestPagination);
    window.addEventListener('load', requestPagination);

    const observer = new MutationObserver(requestPagination);
    observer.observe(cvRoot, { childList: true, subtree: true, characterData: true });

    return () => {
      window.removeEventListener('resize', requestPagination);
      window.removeEventListener('load', requestPagination);
      observer.disconnect();
      if (paginateTimer) clearTimeout(paginateTimer);
    };
  }, [resumeData, templateName, zoom]);

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
    try {
      window.print();

      // Ghi log export lên server để đổi trạng thái IsDraft = false
      try {
        await fetch(`/Resume/LogExport?resumeId=${window.INITIAL_RESUME_DATA?.ResumeID || 0}`, {
          method: 'POST'
        });
      } catch (logErr) {
        console.error("Lỗi ghi log export:", logErr);
      }
    } catch (err) {
      console.error("Lỗi in CV:", err);
    }
  };

  const handleFormatText = (tag) => {
    if (!activeInput) {
      alert("Vui lòng click chọn và bôi đen phần văn bản trong các ô nhập liệu ở cột trái trước khi định dạng!");
      return;
    }

    const { el, field, index, subfield } = activeInput;
    let targetEl = el;
    if (!targetEl) {
      let selector = `[data-field="${field}"]`;
      if (index !== null) selector += `[data-index="${index}"]`;
      if (subfield) selector += `[data-subfield="${subfield}"]`;
      targetEl = document.querySelector(selector);
    }

    if (targetEl) {
      targetEl.focus();
      let command = '';
      if (tag === 'b') command = 'bold';
      else if (tag === 'i') command = 'italic';
      else if (tag === 'u') command = 'underline';
      else if (tag === 'ul') command = 'insertUnorderedList';
      else if (tag === 'ol') command = 'insertOrderedList';

      if (command) {
        document.execCommand(command, false, null);
      }
    }
  };

  const selectTemplate = (newName) => {
    const entry = TemplateRegistry[newName];
    if (entry) {
      if (entry.isProOnly && !window.INITIAL_RESUME_DATA?.isPro) {
        if (window.confirm("Mẫu CV này chỉ dành cho thành viên PRO. Bạn có muốn nâng cấp tài khoản để sử dụng mẫu này?")) {
          window.open("/Account/Upgrade", "_blank");
        }
        return;
      }
      
      setTemplateName(newName);
      if (window.INITIAL_RESUME_DATA) {
        window.INITIAL_RESUME_DATA.TemplateID = entry.id;
      }
      
      // Update resumeData state to trigger auto-save
      setResumeData(prev => ({
        ...prev,
        templateId: entry.id
      }));
    }
  };

  const SelectedTemplate = TemplateRegistry[templateName]?.component;

  return (
    <div style={{ display: 'flex', width: '100%', height: '100%', fontFamily: 'Arial, sans-serif', overflow: 'hidden' }}>
      {/* CỘT TRÁI - FORM */}
      <div style={{ width: '500px', backgroundColor: '#f8f9fa', borderRight: '1px solid #ddd', display: 'flex', flexDirection: 'column', flexShrink: 0, position: 'relative' }}>
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
        
        <div style={{ padding: '12px 15px', borderBottom: '1px solid #e2e8f0', display: 'flex', alignItems: 'center', justifyContent: 'space-between', backgroundColor: '#f8fafc' }}>
          <div>
            <span style={{ fontWeight: 'bold', fontSize: '12px', color: '#64748b', textTransform: 'uppercase', letterSpacing: '0.5px' }}>Mẫu đang dùng: </span>
            <span style={{ fontSize: '13px', fontWeight: 'bold', color: '#0d6efd', backgroundColor: '#e0f2fe', padding: '4px 8px', borderRadius: '6px', marginLeft: '5px' }}>
              {templateName}
            </span>
          </div>
          <button 
            onClick={() => setShowTemplateSelector(prev => !prev)}
            style={{ 
              padding: '6px 12px', 
              backgroundColor: '#0d6efd', 
              color: 'white', 
              border: 'none', 
              borderRadius: '6px', 
              cursor: 'pointer', 
              fontWeight: 'bold', 
              fontSize: '12px',
              display: 'flex',
              alignItems: 'center',
              gap: '6px',
              boxShadow: '0 2px 4px rgba(13, 110, 253, 0.2)',
              transition: 'background-color 0.2s'
            }}
            onMouseOver={(e) => e.currentTarget.style.backgroundColor = '#0b5ed7'}
            onMouseOut={(e) => e.currentTarget.style.backgroundColor = '#0d6efd'}
          >
            <i className="fas fa-palette"></i> Đổi mẫu CV
          </button>
        </div>

        {/* MODAL CHỌN MẪU CV (MỞ BÊN NGOÀI) */}
        {showTemplateSelector && (
          <div style={{
            position: 'absolute',
            top: '95px',
            left: '515px',
            width: '460px',
            height: 'calc(100% - 120px)',
            backgroundColor: '#ffffff',
            borderRadius: '16px',
            boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.15), 0 10px 10px -5px rgba(0, 0, 0, 0.04), 0 0 1px 1px rgba(0,0,0,0.1)',
            display: 'flex',
            flexDirection: 'column',
            animation: 'slideInRight 0.3s cubic-bezier(0.34, 1.56, 0.64, 1)',
            overflow: 'hidden',
            zIndex: 1000,
            border: '1px solid #e2e8f0'
          }}>
            {/* Embedded styles for modal animation */}
            <style>{`
              @keyframes slideInRight {
                from { transform: translateX(30px); opacity: 0; }
                to { transform: translateX(0); opacity: 1; }
              }
            `}</style>

            {/* Header */}
            <div style={{ 
              padding: '18px 20px', 
              borderBottom: '1px solid #e2e8f0', 
              display: 'flex', 
              justifyContent: 'space-between', 
              alignItems: 'center',
              backgroundColor: '#f8fafc'
            }}>
              <div>
                <h3 style={{ margin: 0, fontSize: '15px', fontWeight: 'bold', color: '#0f172a', display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <i className="fas fa-th-large" style={{ color: '#0d6efd' }}></i> Thư viện mẫu CV
                </h3>
                <p style={{ margin: '4px 0 0 0', fontSize: '11px', color: '#64748b' }}>Chọn phong cách thiết kế bạn muốn</p>
              </div>
              <button 
                onClick={() => setShowTemplateSelector(false)}
                style={{
                  border: 'none',
                  background: 'none',
                  cursor: 'pointer',
                  fontSize: '22px',
                  color: '#94a3b8',
                  padding: '0',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  borderRadius: '50%',
                  width: '28px',
                  height: '28px',
                  transition: 'all 0.2s'
                }}
                onMouseOver={(e) => {
                  e.currentTarget.style.backgroundColor = '#f1f5f9';
                  e.currentTarget.style.color = '#475569';
                }}
                onMouseOut={(e) => {
                  e.currentTarget.style.backgroundColor = 'transparent';
                  e.currentTarget.style.color = '#94a3b8';
                }}
              >
                &times;
              </button>
            </div>

            {/* Grid mẫu CV - exactly 2 columns */}
            <div style={{ flex: 1, overflowY: 'auto', padding: '16px', backgroundColor: '#f8fafc' }}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
                {Object.entries(TemplateRegistry).map(([key, val]) => {
                  const isCurrent = key === templateName;
                  const isProOnly = val.isProOnly;
                  
                  // Look up preview image from database metadata passed from C# controller
                  const dbTemplate = window.TEMPLATE_METADATA?.find(t => t.id === val.id);
                  const previewImg = dbTemplate?.previewUrl || `/images/templates/templatesCV_${val.id}.jpg`;
                  
                  return (
                    <div 
                      key={key}
                      onClick={() => {
                        selectTemplate(key);
                      }}
                      style={{
                        border: isCurrent ? '2.5px solid #0d6efd' : '1px solid #e2e8f0',
                        borderRadius: '12px',
                        overflow: 'hidden',
                        cursor: 'pointer',
                        position: 'relative',
                        transition: 'all 0.25s ease',
                        boxShadow: isCurrent ? '0 10px 15px -3px rgba(13, 110, 253, 0.2)' : '0 4px 6px -1px rgba(0, 0, 0, 0.05)',
                        backgroundColor: '#ffffff'
                      }}
                      onMouseOver={(e) => {
                        if (!isCurrent) {
                          e.currentTarget.style.borderColor = '#0d6efd';
                          e.currentTarget.style.transform = 'translateY(-4px)';
                          e.currentTarget.style.boxShadow = '0 10px 15px -3px rgba(0, 0, 0, 0.1)';
                        }
                      }}
                      onMouseOut={(e) => {
                        if (!isCurrent) {
                          e.currentTarget.style.borderColor = '#e2e8f0';
                          e.currentTarget.style.transform = 'none';
                          e.currentTarget.style.boxShadow = '0 4px 6px -1px rgba(0, 0, 0, 0.05)';
                        }
                      }}
                    >
                      {/* Pro Badge */}
                      {isProOnly && (
                        <div style={{
                          position: 'absolute',
                          top: '6px',
                          right: '6px',
                          backgroundColor: '#f59e0b',
                          color: '#ffffff',
                          fontSize: '8px',
                          fontWeight: 'bold',
                          padding: '2px 6px',
                          borderRadius: '4px',
                          zIndex: 5,
                          boxShadow: '0 2px 4px rgba(0,0,0,0.15)',
                          display: 'flex',
                          alignItems: 'center',
                          gap: '2px'
                        }}>
                          <i className="fas fa-crown" style={{ fontSize: '7px' }}></i> PRO
                        </div>
                      )}
                      
                      {/* Image wrapper keeping A4 ratio */}
                      <div style={{
                        padding: '8px',
                        backgroundColor: '#f8fafc',
                        display: 'flex',
                        justifyContent: 'center',
                        alignItems: 'center',
                        borderBottom: '1px solid #e2e8f0'
                      }}>
                        <div style={{
                          width: '100%',
                          aspectRatio: '1 / 1.414', // Exact A4 aspect ratio
                          borderRadius: '6px',
                          overflow: 'hidden',
                          boxShadow: '0 2px 6px rgba(0,0,0,0.06)',
                          border: '1px solid #e2e8f0',
                          position: 'relative',
                          backgroundColor: '#ffffff'
                        }}>
                          <img 
                            src={previewImg} 
                            alt={key}
                            style={{
                              width: '100%',
                              height: '100%',
                              objectFit: 'cover',
                              objectPosition: 'top center',
                              transition: 'transform 0.3s ease'
                            }}
                            onError={(e) => {
                              e.currentTarget.src = 'https://images.unsplash.com/photo-1586281380117-5a60ae2050cc?q=80&w=400';
                            }}
                          />
                          
                          {/* Checkmark overlay when selected */}
                          {isCurrent && (
                            <div style={{
                              position: 'absolute',
                              top: 0,
                              left: 0,
                              right: 0,
                              bottom: 0,
                              backgroundColor: 'rgba(13, 110, 253, 0.12)',
                              display: 'flex',
                              alignItems: 'center',
                              justifyContent: 'center'
                            }}>
                              <div style={{
                                backgroundColor: '#0d6efd',
                                color: '#ffffff',
                                borderRadius: '50%',
                                width: '32px',
                                height: '32px',
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center',
                                boxShadow: '0 4px 8px rgba(13, 110, 253, 0.3)'
                              }}>
                                <i className="fas fa-check" style={{ fontSize: '14px' }}></i>
                              </div>
                            </div>
                          )}
                        </div>
                      </div>
                      
                      {/* Title block */}
                      <div style={{ 
                        padding: '10px 8px', 
                        fontSize: '11px', 
                        fontWeight: 'bold', 
                        color: isCurrent ? '#0d6efd' : '#1e293b',
                        textAlign: 'center',
                        textOverflow: 'ellipsis',
                        overflow: 'hidden',
                        whiteSpace: 'nowrap'
                      }}>
                        {key}
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          </div>
        )}

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
        <div style={{ flex: 1, width: '100%', overflow: 'auto', display: 'flex', flexDirection: 'column', alignItems: 'center', padding: '40px 0' }}>
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
              zoom: zoom,
              flexShrink: 0,
              transition: 'zoom 0.15s ease-out',
              position: 'relative'
          }}>
            {SelectedTemplate ? <SelectedTemplate resumeData={preprocessResumeData(resumeData)} /> : <div style={{padding: 50}}>Không tìm thấy mẫu CV</div>}
            
            {/* Page break markers */}
            {Array.from({ length: pageCount - 1 }).map((_, idx) => {
              const p = idx + 1;
              return (
                <div
                  key={p}
                  className="page-break-marker no-print"
                  style={{
                    position: 'absolute',
                    left: '-10px',
                    width: 'calc(100% + 20px)',
                    top: `calc(${p * 297}mm - 12px)`,
                    height: '24px',
                    backgroundColor: '#525659', // Blends with editor background to create a visual gap
                    zIndex: 50,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    pointerEvents: 'none',
                    boxShadow: '0 -4px 6px -1px rgba(0,0,0,0.15), 0 4px 6px -1px rgba(0,0,0,0.15)'
                  }}
                >
                  {/* Subtle split line */}
                  <div style={{
                    width: '100%',
                    height: '1px',
                    backgroundColor: 'rgba(255,255,255,0.06)',
                    position: 'absolute',
                    top: '50%'
                  }} />
                  
                  {/* Badge */}
                  <span style={{
                    position: 'absolute',
                    fontSize: '9px',
                    textTransform: 'uppercase',
                    fontWeight: 'bold',
                    color: '#cbd5e1',
                    letterSpacing: '1px',
                    backgroundColor: '#383b3d',
                    padding: '3px 12px',
                    borderRadius: '20px',
                    border: '1px solid #4a4e51',
                    boxShadow: '0 4px 12px rgba(0,0,0,0.3)',
                    display: 'flex',
                    alignItems: 'center',
                    gap: '6px'
                  }}>
                    <span style={{ display: 'inline-block', width: '6px', height: '6px', borderRadius: '50%', backgroundColor: '#ef4444' }}></span>
                    Ngắt trang {p + 1}
                  </span>
                </div>
              );
            })}
          </div>
          {/* Khoảng cách an toàn phía dưới để CV không chạm sát viền màn hình */}
          <div style={{ height: '60px', flexShrink: 0 }} />
        </div>

      </div>
    </div>
  )
}

export default App
