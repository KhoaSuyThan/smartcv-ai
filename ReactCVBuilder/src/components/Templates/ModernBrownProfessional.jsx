import React from 'react';

const ModernBrownProfessional = ({ resumeData }) => {
    return (
        <div className="cv-template-ModernBrownProfessional">
            <div className="brown-cv">

	<div className="sidebar">

	<div className="avatar-box">{resumeData?.avatarUrl && <img src={resumeData.avatarUrl} className="avatar" />}</div>

	<div className="contact-list">
	<div className="contact-item"><i className="fas fa-phone"></i> {resumeData?.phone || 'SĐT'}</div>
	<div className="contact-item"><i className="fas fa-envelope"></i> {resumeData?.email || 'Email'}</div>
	<div className="contact-item"><i className="fas fa-map-marker-alt"></i> {resumeData?.address || 'Địa chỉ'}</div>
	</div>

	<div className="sidebar-section"><h3 className="side-title">Kỹ năng</h3><div className="side-content"><ul className="skill-list-items">{resumeData?.skills?.map((item, idx) => (
<li>• {item.name}: {item.level}</li>
))}</ul></div></div>

	<div className="sidebar-section"><h3 className="side-title">Chứng chỉ</h3><div className="side-content">{resumeData?.certifications?.map((item, idx) => (

        <div style={{marginBottom:'8px'}}>
            <div className="cert-year-div" style={{fontWeight:'bold', fontSize:'11px', color:'#634c46'}}>{item.year}</div>
            <div className="cert-name-div" style={{fontSize:'12.5px'}}>{item.name}</div>
        </div>
))}</div></div>

	<div className="sidebar-section"><h3 className="side-title">Giải thưởng</h3><div className="side-content"><ul style={{paddingLeft:'15px', margin:0}}>{resumeData?.awards?.map((item, idx) => (
<li>{item.name}</li>
))}</ul></div></div>

	<div className="sidebar-section"><h3 className="side-title">Học vấn</h3><div className="side-content">{resumeData?.educations?.map((item, idx) => (

        <div className="exp-item" key={idx}>
            <span className="exp-year">• {item.year}</span>
            <div className="exp-content">
                <strong>{item.school}</strong><br/>
                {item.major}<br/>
                {item.gradType ? `Xếp loại: ${item.gradType}` : ''}
            </div>
        </div>
))}</div></div>

	<div className="sidebar-section"><h3 className="side-title">Người tham chiếu</h3><div className="side-content">{resumeData?.references?.map((item, idx) => (
<p style={{marginBottom:'5px', fontSize:'12px'}}>• {item.info}</p>
))}</div></div>

	</div>

	<div className="main-body">

	<div className="header-brown">
	<h1>{resumeData?.fullName || 'HỌ TÊN'}</h1>
	<h2>{resumeData?.jobTitle || 'VỊ TRÍ'}</h2>
	<div className="summary-text">{resumeData?.summary || 'Mục tiêu nghề nghiệp'}</div>
	</div>

	<div className="content-padding">

	<div className="main-section"><h3 className="main-title">Kinh nghiệm làm việc</h3><div className="exp-list">{resumeData?.experiences?.map((item, idx) => (

        <div className="exp-item" key={idx}>
            <span className="exp-year">• {item.time}</span>
            <div className="exp-content">
                <div className="info-line"><strong>Công ty:</strong> {item.company}</div>
                <div className="info-line"><strong>Vị trí:</strong> {item.role}</div>
                <div className="desc-text" dangerouslySetInnerHTML={{__html: item.desc}}></div>
            </div>
        </div>
))}</div></div>

	<div className="main-section"><h3 className="main-title">Hoạt động</h3><div className="activity-list">{resumeData?.activities?.map((item, idx) => (

        <div className="exp-item" key={idx}>
            <div className="exp-header">
                <span className="company-name">{item.name}</span>
                <span className="date-badge">{item.time}</span>
            </div>
            <div className="exp-desc" dangerouslySetInnerHTML={{__html: item.desc}}></div>
        </div>
))}</div></div>

	</div>

	</div>

	</div>

    
<style>{`
/* Cấu trúc Layout */
    .brown-cv { display: flex; background: white; height: 297mm; font-family: "Segoe UI", sans-serif; }
    .sidebar { flex: 3.5; background: #e5ddd5; padding: 30px 20px; display: flex; flex-direction: column; gap: 20px; }
    .main-body { flex: 6.5; display: flex; flex-direction: column; }

    /* Avatar tròn */
    .avatar { width: 150px; height: 150px; border-radius: 50%; object-fit: cover; display: block; margin: 0 auto 20px; border: 5px solid #d6cdc4; }

    /* Contact & Sidebar Title */
    .contact-list { font-size: 12px; border-top: 1px solid #c9beae; border-bottom: 1px solid #c9beae; padding: 10px 0; }
    .contact-item { margin-bottom: 8px; display: flex; align-items: center; gap: 10px; }
    .contact-item i { width: 24px; height: 24px; background: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 10px; color: #634c46; }

    .side-title { font-size: 16px; font-weight: bold; color: #333; margin-bottom: 10px; border-bottom: 1px solid #c9beae; padding-bottom: 5px; }
    .side-content { font-size: 12px; line-height: 1.5; color: #444; }

    /* Header màu nâu đậm */
    .header-brown { background: #634c46; color: white; padding: 40px 35px; }
    .header-brown h1 { margin: 0; font-size: 36px; text-transform: capitalize; font-weight: 800; letter-spacing: 1px; }
    .header-brown h2 { margin: 5px 0 15px 0; font-size: 16px; text-transform: uppercase; font-weight: normal; border-bottom: 1px solid rgba(255,255,255,0.3); padding-bottom: 10px; }
    .summary-text { font-size: 12.5px; line-height: 1.6; opacity: 0.9; text-align: justify; }

    /* Nội dung chính */
    .content-padding { padding: 30px 35px; }
    .main-title { font-size: 18px; font-weight: bold; color: #333; border-bottom: 2px solid #634c46; padding-bottom: 5px; margin-bottom: 15px; text-transform: uppercase; }
    
    /* Style cho các item kinh nghiệm (có badge ngày tháng) */
    .exp-item { margin-bottom: 15px; position: relative; }
    .exp-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 5px; }
    .date-badge { background: #9b8a7e; color: white; padding: 2px 10px; border-radius: 12px; font-size: 11px; font-weight: bold; }
    
    .company-name { font-weight: bold; font-size: 14px; color: #333; }
    .job-pos { font-style: italic; font-size: 13px; color: #555; display: block; margin-top: 2px; }
    .exp-desc { font-size: 12.5px; line-height: 1.5; margin-top: 5px; color: #444; }
    .exp-desc li { margin-bottom: 4px; padding-left: 5px; }
`}</style>

        </div>
    );
};
export const defaultVisibility = {
  summary: true, experiences: true, educations: true, skills: true, otherSkills: true,
  projects: true, activities: true, certifications: true, awards: true,
  references: true, hobbies: true, languages: true
};
export default ModernBrownProfessional;
