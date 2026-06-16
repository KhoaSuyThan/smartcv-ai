import React from 'react';

const NgoHaiYen = ({ resumeData }) => {
    return (
        <div className="cv-template-NgoHaiYen">
            <div className="cv-yens-wrapper">
        <div className="cv-sidebar">
            <div className="avatar-box">
                {resumeData?.avatarUrl && <img src={resumeData.avatarUrl} className="avatar-img" />}
            </div>

            <div className="sidebar-section">
                <h3 className="sidebar-title">LIÊN HỆ VỚI TÔI</h3>
                <div className="contact-list">
                    <div className="contact-item"><i className="fas fa-map-marker-alt"></i> {resumeData?.address || 'Địa chỉ'}</div>
                    <div className="contact-item"><i className="fas fa-envelope"></i> {resumeData?.email || 'Email'}</div>
                    <div className="contact-item"><i className="fas fa-phone"></i> {resumeData?.phone || 'SĐT'}</div>
                </div>
            </div>

            <div className="sidebar-section">
                <h3 className="sidebar-title">TÓM TẮT KỸ NĂNG</h3>
                <div className="skill-group">
                    <p className="skill-label">TIN HỌC</p>
                    <div className="skill-content"><ul className="skill-list-items">{resumeData?.skills?.map((item, idx) => (
<li>• {item.name}: {item.level}</li>
))}</ul></div>
                </div>
                <div className="skill-group">
                    <p className="skill-label">NGOẠI NGỮ</p>
                    <div className="skill-content"><ul className="lang-list-items">{resumeData?.languages?.map((item, idx) => (
<li style={{display:'flex', justifyContent:'space-between'}}><span>• {item.name}</span><span style={{fontStyle:'italic', opacity:0.8}}>{item.level}</span></li>
))}</ul></div>
                </div>
                <div className="skill-group">
                    <p className="skill-label">KỸ NĂNG KHÁC</p>
                    <div className="skill-content"><ul className="other-skill-list-items">{resumeData?.otherSkills?.map((item, idx) => (
<li>• {item.name} {item.level ? `(${item.level})` : ''}</li>
))}</ul></div>
                </div>
            </div>

            <div className="sidebar-section">
                <h3 className="sidebar-title">GIẢI THƯỞNG</h3>
                <div className="award-list"><ul style={{paddingLeft:'15px', margin:0}}>{resumeData?.awards?.map((item, idx) => (
<li>{item.name}</li>
))}</ul></div>
            </div>
        </div>

        <div className="cv-main">
            <div className="header-info">
                <h1 className="fullname">{resumeData?.fullName || 'HỌ TÊN'}</h1>
                <h2 className="job-title">{resumeData?.jobTitle || 'VỊ TRÍ'}</h2>
            </div>

            <div className="main-section">
                <h3 className="section-title">MỤC TIÊU NGHỀ NGHIỆP</h3>
                <div className="section-content">{resumeData?.summary || 'Mục tiêu nghề nghiệp'}</div>
            </div>

            <div className="main-section">
                <h3 className="section-title">KINH NGHIỆM LÀM VIỆC</h3>
                <div className="section-content">{resumeData?.experiences?.map((item, idx) => (

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
                <h3 className="section-title">QUÁ TRÌNH HỌC VẤN</h3>
                <div className="section-content">{resumeData?.educations?.map((item, idx) => (

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

    
<style>{`
/* Layout và màu sắc chủ đạo */
    .cv-yens-wrapper { display: flex; width: 210mm; min-height: 297mm; background: white; font-family: "Segoe UI", Arial, sans-serif; }
    .cv-sidebar { flex: 3.5; background-color: #32507d; color: white; padding: 40px 25px; }
    .cv-main { flex: 6.5; padding: 50px 40px; }

    /* Avatar tròn */
    .avatar-box { text-align: center; margin-bottom: 40px; }
    .avatar-img { width: 160px; height: 160px; border-radius: 50%; border: 6px solid rgba(255,255,255,0.1); object-fit: cover; }

    /* Sidebar Styles */
    .sidebar-title { font-size: 16px; font-weight: bold; border-bottom: 1px solid rgba(255,255,255,0.3); padding-bottom: 8px; margin-bottom: 15px; margin-top: 30px; letter-spacing: 1px; }
    .contact-item { font-size: 13px; margin-bottom: 12px; display: flex; align-items: flex-start; gap: 10px; line-height: 1.4; word-break: break-word; }
    .contact-item i { margin-top: 3px; width: 15px; }
    
    .skill-label { font-size: 12px; font-weight: bold; margin-bottom: 5px; color: #a5b8d4; text-transform: uppercase; }
    .skill-content { font-size: 13px; margin-bottom: 15px; white-space: pre-line; word-break: break-all; }
    .award-list { font-size: 13px; line-height: 1.6; }

    /* Main Content Styles */
    .fullname { font-size: 48px; color: #32507d; margin: 0; font-weight: 800; text-transform: uppercase; }
    .job-title { font-size: 20px; color: #5fb4c4; margin: 5px 0 40px 0; letter-spacing: 3px; font-weight: bold; text-transform: uppercase; }

    .section-title { font-size: 16px; font-weight: bold; color: #5fb4c4; margin-bottom: 15px; margin-top: 35px; letter-spacing: 2px; }
    .section-content { 
        font-size: 14px; 
        line-height: 1.8; 
        color: #444; 
        text-align: justify; 
        white-space: pre-line; 
        word-break: break-word;
        border-left: 1px solid #eee;
        padding-left: 15px;
    }

    /* Đảm bảo nội dung không tràn khi nhập chuỗi dài */
    p, div, span { word-wrap: break-word; overflow-wrap: break-word; }
`}</style>

        </div>
    );
};
export const defaultVisibility = {
  summary: true, experiences: true, educations: true, skills: true, otherSkills: true,
  projects: true, activities: true, certifications: true, awards: true,
  references: true, hobbies: true, languages: true
};
export default NgoHaiYen;
