import React from 'react';

const DinhXuanThao = ({ resumeData }) => {
    return (
        <div className="cv-template-DinhXuanThao">
            <div className="cv-thao-wrapper">
    <div className="cv-header">
        <div className="header-content">
            <div className="avatar-box">
                {resumeData?.avatarUrl && <img src={resumeData.avatarUrl} className="avatar-img" />}
            </div>
            <div className="title-box">
                <h1 className="fullname">{resumeData?.fullName || 'HỌ TÊN'}</h1>
                <h2 className="job-title">{resumeData?.jobTitle || 'VỊ TRÍ'}</h2>
            </div>
        </div>
    </div>
    
    <div className="cv-body">
        <div className="cv-sidebar">
            <div className="sidebar-section">
                <h3 className="side-title">LIÊN HỆ</h3>
                <div className="side-content">
                    <p><i className="fas fa-phone"></i> {resumeData?.phone || 'SĐT'}</p>
                    <p><i className="fas fa-envelope"></i> {resumeData?.email || 'Email'}</p>
                    <p><i className="fas fa-map-marker-alt"></i> {resumeData?.address || 'Địa chỉ'}</p>
                </div>
            </div>

            <div className="sidebar-section">
                <h3 className="side-title">KỸ NĂNG</h3>
                <div className="side-content">
                    <div className="skill-main-block"><ul className="skill-list-items">{resumeData?.skills?.map((item, idx) => (
<li>• {item.name}: {item.level}</li>
))}</ul></div>
                    <div className="other-skill-header">KỸ NĂNG KHÁC</div>
                    <div className="other-skill-content">
                        <ul className="other-skill-list-items">{resumeData?.otherSkills?.map((item, idx) => (
<li>• {item.name} {item.level ? `(${item.level})` : ''}</li>
))}</ul>
                    </div>
                </div>
            </div>
            <div className="sidebar-section">
                <h3 className="side-title">CHỨNG CHỈ</h3>
                <div className="side-content">{resumeData?.certifications?.map((item, idx) => (

        <div style={{marginBottom:'8px'}}>
            <div className="cert-year-div" style={{fontWeight:'bold', fontSize:'11px', color:'#634c46'}}>{item.year}</div>
            <div className="cert-name-div" style={{fontSize:'12.5px'}}>{item.name}</div>
        </div>
))}</div>
            </div>
            
            <div className="sidebar-section">
                <h3 className="side-title">NGOẠI NGỮ</h3>
                <div className="side-content"><ul className="lang-list-items">{resumeData?.languages?.map((item, idx) => (
<li style={{display:'flex', justifyContent:'space-between'}}><span>• {item.name}</span><span style={{fontStyle:'italic', opacity:0.8}}>{item.level}</span></li>
))}</ul></div>
            </div>
        </div>

        <div className="cv-main">
            <div className="main-section">
                <h3 className="main-title">GIỚI THIỆU</h3>
                <div className="main-content">{resumeData?.summary || 'Mục tiêu nghề nghiệp'}</div>
            </div>

            <div className="main-section">
                <h3 className="main-title">KINH NGHIỆM LÀM VIỆC</h3>
                <div className="main-content">{resumeData?.experiences?.map((item, idx) => (

        <div className="exp-item" key={idx}>
            <span className="exp-year">• {item.time}</span>
            <div className="exp-content">
                <div className="info-line"><strong>Công ty:</strong> {item.company}</div>
                <div className="info-line"><strong>Vị trí:</strong> {item.role}</div>
                <div className="desc-text" dangerouslySetInnerHTML={{__html: item.desc}}></div>
            </div>
        </div>
))}</div>
            </div>

            <div className="main-section">
                <h3 className="main-title">HỌC VẤN</h3>
                <div className="main-content">{resumeData?.educations?.map((item, idx) => (

        <div className="exp-item" key={idx}>
            <span className="exp-year">• {item.year}</span>
            <div className="exp-content">
                <strong>{item.school}</strong><br/>
                {item.major}<br/>
                {item.gradType ? `Xếp loại: ${item.gradType}` : ''}
            </div>
        </div>
))}</div>
            </div>
        </div>
    </div>
</div>

<style>{`
/* RESET TUYỆT ĐỐI */
* { margin: 0; padding: 0; box-sizing: border-box; }

.cv-thao-wrapper { 
    width: 210mm; min-height: 297mm; background: white; 
    font-family: "Segoe UI", sans-serif; line-height: 1.2; /* Siết độ giãn dòng cực thấp */
}

/* Header & Avatar */
.cv-header { background-color: #2c5a4b; height: 140px; display: flex; align-items: center; position: relative; }
.header-content { display: flex; align-items: center; padding-left: 50px; width: 100%; }
.avatar-box { margin-top: 55px; z-index: 10; }
.avatar-img { width: 170px; height: 170px; border-radius: 50%; border: 5px solid white; object-fit: cover; }
.title-box { margin-left: 25px; margin-top: 15px; color: white; }
.fullname { font-size: 32px; font-weight: 800; text-transform: uppercase; }
.job-title { font-size: 15px; opacity: 0.9; letter-spacing: 2px; text-transform: uppercase; }

/* Layout Body */
.cv-body { display: flex; padding: 35px 40px 20px 40px; }
.cv-sidebar { flex: 3.5; padding-right: 20px; border-right: 1px solid #f2f2f2; }
.cv-main { flex: 6.5; padding-left: 30px; }

/* Sidebar Sections */
.sidebar-section { margin-bottom: 12px; }
.side-title { 
    background: #2c5a4b; color: white; font-size: 12px; font-weight: bold; 
    padding: 4px 15px; border-radius: 0 20px 20px 0; margin-left: -40px; 
    display: inline-block; margin-bottom: 5px;
}

/* --- XỬ LÝ TRIỆT ĐỂ KHOẢNG TRẮNG NỘI DUNG --- */
.side-content, .main-content {
    font-size: 12px;
    color: #444;
    white-space: pre-line;
}

/* Ép tất cả các thẻ con (p, li, div) không được có margin dưới */
.side-content p, .main-content p,
.side-content li, .main-content li,
.side-content div, .main-content div {
    margin-bottom: 2px !important; /* Chỉ để lại 2px cho thoáng, không để trống */
    padding: 0 !important;
    line-height: 1.3 !important;
}

.side-content ul, .main-content ul {
    margin-left: 15px;
    margin-bottom: 2px;
}

/* Kỹ năng khác */
.other-skill-header { 
    font-size: 11px; font-weight: 800; color: #2c5a4b; 
    margin-top: 5px; margin-bottom: 2px; border-top: 1px solid #eee; padding-top: 4px;
}
.other-skill-content { font-size: 11px; font-style: italic; color: #555; }

/* Main Sections */
.main-section { margin-bottom: 12px; }
.main-title { 
    color: #2c5a4b; font-size: 15px; font-weight: 800; 
    border-left: 4px solid #2c5a4b; padding-left: 10px; 
    margin-bottom: 5px; text-transform: uppercase;
}

/* Chống tràn */
p, div, h1, h2, h3, li { word-break: break-word; overflow-wrap: break-word; }
`}</style>

        </div>
    );
};
export const defaultVisibility = {
  summary: true, experiences: true, educations: true, skills: true, otherSkills: true,
  projects: true, activities: true, certifications: true, awards: true,
  references: true, hobbies: true, languages: true
};
export default DinhXuanThao;
