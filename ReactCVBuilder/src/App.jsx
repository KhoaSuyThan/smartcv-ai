import React, { useState, useEffect } from 'react'
import TemplateRegistry from './components/TemplateRegistry'
import LeftForm from './components/LeftForm'

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

  useEffect(() => {
    // Check if injected by ASP.NET backend
    if (window.INITIAL_RESUME_DATA) {
      setResumeData(window.INITIAL_RESUME_DATA);
    }
    if (window.INITIAL_TEMPLATE_NAME) {
      setTemplateName(window.INITIAL_TEMPLATE_NAME);
    }
  }, []);


  const handleSave = async () => {
    const btn = document.getElementById('btn-save-cv');
    if (btn) btn.innerText = 'Đang lưu...';

    const payload = {
      ...resumeData,
      ResumeID: window.INITIAL_RESUME_DATA.ResumeID,
      TemplateID: window.INITIAL_RESUME_DATA.TemplateID,
      Title: window.INITIAL_RESUME_DATA.Title
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
        if (btn) {
          btn.innerText = 'Đã lưu ✓';
          btn.style.backgroundColor = '#10b981';
          setTimeout(() => {
            btn.innerText = 'Lưu CV';
            btn.style.backgroundColor = '#28a745';
          }, 2000);
        }
      } else {
        alert("Lỗi khi lưu: " + (result.message || "Không xác định"));
        if (btn) btn.innerText = 'Lưu CV';
      }
    } catch (err) {
      console.error(err);
      alert("Lỗi mạng khi lưu CV.");
      if (btn) btn.innerText = 'Lưu CV';
    }
  };

  const SelectedTemplate = TemplateRegistry[templateName];

  return (
    <div style={{ display: 'flex', width: '100%', height: '100%', fontFamily: 'Arial, sans-serif' }}>
      {/* CỘT TRÁI - FORM */}
      <div style={{ width: '500px', backgroundColor: '#f8f9fa', borderRight: '1px solid #ddd', display: 'flex', flexDirection: 'column', flexShrink: 0 }}>
        <div style={{ padding: '15px', backgroundColor: '#343a40', color: 'white', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h2 style={{ margin: 0, fontSize: '16px' }}>📝 Chỉnh sửa CV (Beta)</h2>
          <button 
            id="btn-save-cv"
            onClick={handleSave} 
            disabled={isSaving}
            style={{ padding: '8px 15px', backgroundColor: '#28a745', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}
          >
            {isSaving ? 'Đang lưu...' : 'Lưu CV'}
          </button>
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

      {/* CỘT PHẢI - PREVIEW */}
      <div style={{ flex: 1, backgroundColor: '#525659', overflowY: 'auto', display: 'flex', flexDirection: 'column', alignItems: 'center', padding: '40px 0' }}>
        {/* Khung cố định A4 */}
        <div style={{ 
            boxShadow: '0 0 25px rgba(0,0,0,0.5)', 
            backgroundColor: 'white',
            width: '210mm',
            minHeight: '297mm',
            flexShrink: 0
        }}>
          {SelectedTemplate ? <SelectedTemplate resumeData={resumeData} /> : <div style={{padding: 50}}>Không tìm thấy mẫu CV</div>}
        </div>
      </div>
    </div>
  )
}

export default App
