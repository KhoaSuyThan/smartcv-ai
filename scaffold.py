import os

base_dir = r'c:\Users\aaa\Pictures\DoAnWeb_CS\DoAnWeb\ReactCVBuilder\src'
components_dir = os.path.join(base_dir, 'components')
form_dir = os.path.join(components_dir, 'Form')
os.makedirs(form_dir, exist_ok=True)

# App.jsx
app_jsx = '''import React, { useState, useEffect } from "react";
import "./App.css";
import TemplateRenderer from "./components/TemplateRenderer";
import LeftForm from "./components/Form/LeftForm";

function App() {
  const [resumeData, setResumeData] = useState({
    fullName: "Nguyễn Văn A",
    jobTitle: "Lập trình viên Frontend",
    phone: "0123 456 789",
    email: "nguyenvana@gmail.com",
    address: "Hà Nội, Việt Nam",
    website: "https://github.com",
    birthDate: "01/01/2000",
    summary: "Tôi là một lập trình viên...",
    avatarUrl: "https://i.imgur.com/8Km9tLL.png",
    experiences: [
      { id: 1, company: "Công ty ABC", role: "Frontend Dev", time: "2020 - 2022", desc: "Làm việc với ReactJS..." }
    ],
    educations: [
      { id: 1, school: "Đại học Bách Khoa", major: "CNTT", gradType: "Khá", year: "2018 - 2022" }
    ],
    projects: [],
    skills: [],
    languages: [],
    otherSkills: [],
    activities: [],
    certifications: [],
    awards: [],
    hobbies: [],
    references: []
  });

  const [templateName, setTemplateName] = useState("DangNgocLinh");

  // Đọc dữ liệu từ biến toàn cục nếu có (khi chạy trong ASP.NET)
  useEffect(() => {
    if (window.INITIAL_RESUME_DATA) {
      try {
        setResumeData(window.INITIAL_RESUME_DATA);
      } catch(e) { console.error(e); }
    }
    if (window.INITIAL_TEMPLATE_NAME) {
      setTemplateName(window.INITIAL_TEMPLATE_NAME);
    }
  }, []);

  const handleChange = (field, value) => {
    setResumeData(prev => ({ ...prev, [field]: value }));
  };

  return (
    <div className="builder-layout">
      <div className="builder-left-col">
         <LeftForm resumeData={resumeData} onChange={handleChange} />
      </div>
      <div className="builder-right-col">
         <div className="preview-container">
            <TemplateRenderer templateName={templateName} resumeData={resumeData} />
         </div>
      </div>
    </div>
  );
}

export default App;
'''
with open(os.path.join(base_dir, 'App.jsx'), 'w', encoding='utf-8') as f:
    f.write(app_jsx)

# App.css
app_css = '''
body { margin: 0; padding: 0; background: #f0f2f5; font-family: "Segoe UI", sans-serif; }
.builder-layout { display: flex; height: 100vh; overflow: hidden; }
.builder-left-col { width: 500px; background: #fff; overflow-y: auto; border-right: 1px solid #ddd; box-shadow: 2px 0 5px rgba(0,0,0,0.05); z-index: 10; display: flex; flex-direction: column; }
.builder-right-col { flex: 1; overflow-y: auto; display: flex; justify-content: center; padding: 40px; background: #525659; }
.preview-container { box-shadow: 0 4px 15px rgba(0,0,0,0.2); background: white; margin: 0 auto; }
'''
with open(os.path.join(base_dir, 'App.css'), 'w', encoding='utf-8') as f:
    f.write(app_css)

# TemplateRenderer.jsx
template_renderer_jsx = '''import React from 'react';
import DangNgocLinh from './Templates/DangNgocLinh';
import DaoPhuQuy from './Templates/DaoPhuQuy';
import DinhXuanThao from './Templates/DinhXuanThao';
import ElegantAccountant from './Templates/ElegantAccountant';
import LeChien from './Templates/LeChien';
import ModernBlueSidebar from './Templates/ModernBlueSidebar';
import ModernBrownProfessional from './Templates/ModernBrownProfessional';
import ModernProfessionalSplit from './Templates/ModernProfessionalSplit';
import NgoHaiYen from './Templates/NgoHaiYen';
import NguyenMinhAn from './Templates/NguyenMinhAn';
import NguyenVoLeKhoa from './Templates/NguyenVoLeKhoa';
import NguyenYenNhi from './Templates/NguyenYenNhi';
import PastelBeigeBlocks from './Templates/PastelBeigeBlocks';
import TranHoaiThu from './Templates/TranHoaiThu';
import TranManhDung from './Templates/TranManhDung';

const components = {
    DangNgocLinh, DaoPhuQuy, DinhXuanThao, ElegantAccountant, LeChien,
    ModernBlueSidebar, ModernBrownProfessional, ModernProfessionalSplit,
    NgoHaiYen, NguyenMinhAn, NguyenVoLeKhoa, NguyenYenNhi,
    PastelBeigeBlocks, TranHoaiThu, TranManhDung
};

const TemplateRenderer = ({ templateName, resumeData }) => {
    const ComponentToRender = components[templateName];
    
    if (!ComponentToRender) {
        return <div>Template {templateName} not found!</div>;
    }

    // Wrap in A4 container size for uniform preview
    return (
        <div style={{ width: '210mm', minHeight: '297mm', background: 'white' }}>
            <ComponentToRender resumeData={resumeData} />
        </div>
    );
};

export default TemplateRenderer;
'''
with open(os.path.join(components_dir, 'TemplateRenderer.jsx'), 'w', encoding='utf-8') as f:
    f.write(template_renderer_jsx)

# LeftForm.jsx (Basic)
left_form_jsx = '''import React from 'react';

const LeftForm = ({ resumeData, onChange }) => {
    return (
        <div style={{ padding: '20px' }}>
            <h2 style={{marginTop: 0}}>Chỉnh sửa CV</h2>
            
            <div className="form-group" style={{ marginBottom: '15px' }}>
                <label style={{ display: 'block', fontWeight: 'bold', marginBottom: '5px' }}>Họ và tên</label>
                <input 
                    type="text" 
                    value={resumeData.fullName} 
                    onChange={e => onChange('fullName', e.target.value)} 
                    style={{ width: '100%', padding: '8px', border: '1px solid #ccc', borderRadius: '4px' }}
                />
            </div>
            
            <div className="form-group" style={{ marginBottom: '15px' }}>
                <label style={{ display: 'block', fontWeight: 'bold', marginBottom: '5px' }}>Vị trí ứng tuyển</label>
                <input 
                    type="text" 
                    value={resumeData.jobTitle} 
                    onChange={e => onChange('jobTitle', e.target.value)} 
                    style={{ width: '100%', padding: '8px', border: '1px solid #ccc', borderRadius: '4px' }}
                />
            </div>
            
            {/* TODO: Add other fields like Phone, Email, Summary, Experience... */}
            <p><i>Các trường thông tin khác sẽ được bổ sung ở bước tiếp theo...</i></p>
            
            <div style={{ marginTop: '30px', paddingTop: '20px', borderTop: '1px solid #eee' }}>
                <button style={{ background: '#0d6efd', color: 'white', padding: '10px 20px', border: 'none', borderRadius: '4px', cursor: 'pointer', width: '100%' }}>
                    LƯU CV
                </button>
            </div>
        </div>
    );
};

export default LeftForm;
'''
with open(os.path.join(form_dir, 'LeftForm.jsx'), 'w', encoding='utf-8') as f:
    f.write(left_form_jsx)

print("Scaffolded App.jsx, App.css, TemplateRenderer.jsx, LeftForm.jsx")
