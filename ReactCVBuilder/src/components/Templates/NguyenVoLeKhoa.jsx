import React from 'react';

const NguyenVoLeKhoa = ({ resumeData }) => {
    return (
        <div className="cv-template-NguyenVoLeKhoa">
            {<div className="cv-modern-wrapper">
        <div className="cv-sidebar">
            <div className="avatar-area">
                <img src={resumeData?.avatarUrl || "/images/default-avatar.png"} className="avatar-img" />
            </div>
            
            {(resumeData?.phone || resumeData?.email || resumeData?.website || resumeData?.address) && (
                <div className="sidebar-info">
                    {resumeData?.phone && <div className="info-row"><i className="fas fa-phone-alt"></i> <span>{resumeData.phone}</span></div>}
                    {resumeData?.email && <div className="info-row"><i className="fas fa-envelope"></i> <span>{resumeData.email}</span></div>}
                    {resumeData?.website && <div className="info-row"><i className="fas fa-globe"></i> <span>{resumeData.website}</span></div>}
                    {resumeData?.address && <div className="info-row"><i className="fas fa-map-marker-alt"></i> <span>{resumeData.address}</span></div>}
                </div>
            )}
            <div className="side-section">
                <h3 className="side-title">KỸ NĂNG</h3>
                <div className="side-content"><ul className="other-skill-list-items">{resumeData?.otherSkills?.map((item, idx) => (
<li>• {item.name} {item.level ? `(${item.level})` : ''}</li>
))}</ul></div>
            </div>
            <div className="side-section">
                <h3 className="side-title">TIN HỌC</h3>
                <div className="side-content"><ul className="skill-list-items">{resumeData?.skills?.map((item, idx) => (
<li>• {item.name}: {item.level}</li>
))}</ul></div>
            </div>
            <div className="side-section">
                <h3 className="side-title">CHỨNG CHỈ</h3>
                <div className="side-content">{resumeData?.certifications?.map((item, idx) => (

        <div style={{marginBottom:'8px'}}>
            <div className="cert-year-div" style={{fontWeight:'bold', fontSize:'11px', color:'#634c46'}}>{item.year}</div>
            <div className="cert-name-div" style={{fontSize:'12.5px'}}>{item.name}</div>
        </div>
))}</div>
            </div>
            <div className="side-section">
                <h3 className="side-title">GIẢI THƯỞNG</h3>
                <div className="side-content"><ul style={{paddingLeft:'15px', margin:0}}>{resumeData?.awards?.map((item, idx) => (
<li>{item.name}</li>
))}</ul></div>
            </div>
            <div className="side-section">
                <h3 className="side-title">SỞ THÍCH</h3>
                <div className="side-content"><ul className="hobby-list-items">{resumeData?.hobbies?.map((item, idx) => (
<li>• {item.name}</li>
))}</ul></div>
            </div>
        </div>
        <div className="cv-main">
            <div className="dark-header">
                <h1 className="name">{resumeData?.fullName || 'HỌ TÊN'}</h1>
                <h2 className="title">{resumeData?.jobTitle || 'VỊ TRÍ'}</h2>
                <div className="summary">{resumeData?.summary || 'Mục tiêu nghề nghiệp'}</div>
            </div>
            <div className="main-body">
                <div className="main-section">
                    <h3 className="main-title">HỌC VẤN</h3>
                    <div className="content-list">{resumeData?.educations?.map((item, idx) => (

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
                <div className="main-section">
                    <h3 className="main-title">KINH NGHIỆM LÀM VIỆC</h3>
                    <div className="content-list">
                        {resumeData?.experiences?.map((item, idx) => (

        <div className="exp-item" key={idx}>
            <span className="exp-year">• {item.time}</span>
            <div className="exp-content">
                <div className="info-line"><strong>Công ty:</strong> {item.company}</div>
                <div className="info-line"><strong>Vị trí:</strong> {item.role}</div>
                <div className="desc-text" dangerouslySetInnerHTML={{__html: item.desc}}></div>
            </div>
        </div>
))} 
                    </div>
                </div>
                <div className="main-section">
                    <h3 className="main-title">DỰ ÁN</h3>
                    <div className="content-list">
                        {resumeData?.projects?.map((item, idx) => (

        <div className="exp-item" key={idx}>
            <span className="exp-year">• {item.time}</span>
            <div className="exp-content">
                <div className="info-line"><strong>Dự án:</strong> {item.name}</div>
                <div className="info-line"><strong>Vai trò:</strong> {item.role}</div>
                <div className="desc-text" dangerouslySetInnerHTML={{__html: item.desc}}></div>
            </div>
        </div>
))}
                    </div>
                </div>
                <div className="main-section">
                    <h3 className="main-title">HOẠT ĐỘNG</h3>
                    <div className="content-list">
                        {resumeData?.activities?.map((item, idx) => (

        <div className="exp-item" key={idx}>
            <div className="exp-header">
                <span className="company-name">{item.name}</span>
                <span className="date-badge">{item.time}</span>
            </div>
            <div className="exp-desc" dangerouslySetInnerHTML={{__html: item.desc}}></div>
        </div>
))}
                    </div>
                </div>
            </div>
        </div>
    </div>}

    
<style>{`
/* CẤU TRÚC TỔNG THỂ */
    @import url(''https://fonts.googleapis.com/css2?family=Inter:wght@400;600;800&display=swap'');
    .cv-modern-wrapper { display: flex; width: 100%; height: 100%; min-height: 297mm; background: white; font-family: "Inter", sans-serif; overflow: hidden; }
    .cv-modern-wrapper * { box-sizing: border-box; word-wrap: break-word; word-break: break-word; }
    /* SIDEBAR (38%) */
    .cv-sidebar { width: 38%; background: #eae6db; padding: 40px 25px; display: flex; flex-direction: column; border-right: 1px solid #dcd8cf; }
    .avatar-area { text-align: center; margin-bottom: 25px; }
    .avatar-img { width: 180px; height: 180px; border-radius: 50%; object-fit: cover; border: 4px solid white; box-shadow: 0 4px 10px rgba(0,0,0,0.1); }
    
    .sidebar-info { border-top: 1px solid #d4cfc4; border-bottom: 1px solid #d4cfc4; padding: 20px 0; margin-bottom: 25px; }
    .info-row { display: flex; align-items: center; justify-content: flex-start; gap: 12px; font-size: 13px; color: #222; margin-bottom: 12px; line-height: 1.4; display: flex; }
    .info-row:last-child { margin-bottom: 0; }
    .info-row i { width: 28px; height: 28px; background: #dedad0; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 12px; color: #4b5247; border: 1px solid #d0cbbc; flex-shrink: 0; }
    .side-title { font-size: 16px; font-weight: 800; color: #222; margin: 20px 0 12px 0 !important; text-transform: uppercase; border-bottom: 1.5px solid #d4cfc4; padding-bottom: 6px; letter-spacing: 0.5px; }
    .side-content { font-size: 13.5px; color: #333; line-height: 1.5; }
    .side-content p, .side-content li { margin: 0 0 8px 0 !important; }
    .side-content ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
    .side-content li { display: flex; justify-content: space-between; align-items: baseline; }
    .side-content li > span:first-child { font-weight: 600; color: #222; flex-grow: 1; }
    .side-content li > span:nth-child(2) { font-style: italic; color: #555; font-size: 11.5px; }
    /* Fix awards and hobbies */
    .award-list ul, .hobby-list ul { padding-left: 15px !important; }
    /* MAIN CONTENT (62%) */
    .cv-main { width: 62%; display: flex; flex-direction: column; background: #ffffff; }
    
    /* DARK HEADER */
    .dark-header { background: #5d6657; color: #f4f0e8; padding: 45px 35px; }
    .name { font-size: 38px; font-weight: 800; text-transform: uppercase; margin: 0 0 5px 0; line-height: 1.2; letter-spacing: 1px; color: #fff;}
    .title { font-size: 17px; font-weight: 600; margin: 0 0 15px 0; letter-spacing: 1.5px; border-bottom: 1px solid rgba(255,255,255,0.3); padding-bottom: 12px; display: inline-block; text-transform: uppercase;}
    .summary { font-size: 14px; line-height: 1.6; text-align: justify; opacity: 0.9; margin-top: 5px; }
    /* BODY */
    .main-body { padding: 30px 40px; }
    .main-section { margin-bottom: 25px; }
    /* Ẩn các khối trống */
    .main-section:empty, .content-list:empty { display: none !important; }
    .main-title { display: flex; align-items: center; font-size: 18px; font-weight: 800; color: #333; margin: 0 0 18px 0 !important; letter-spacing: 0.5px; text-transform: uppercase; }
    .main-title::after { content: ""; flex-grow: 1; height: 1.5px; background: #e0e0e0; margin-left: 15px; }
    
    /* CONTENT LIST & DYNAMIC ITEMS */
    .content-list { font-size: 14px; line-height: 1.5; color: #333; }
    
    .exp-item { position: relative; margin-bottom: 25px; }
    .exp-year { position: absolute; right: 0; top: -3px; background: #5d6657; color: white; padding: 4px 15px; border-radius: 20px; font-size: 12px; font-weight: 700; white-space: nowrap; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
    
    .exp-content { padding-right: 140px; display: flex; flex-direction: column; }
    .info-line:first-child { font-size: 15px; font-weight: 800; color: #222; margin-bottom: 4px; }
    .info-line:nth-child(2) { font-size: 14px; color: #555; margin-bottom: 6px; font-style: italic; }
    .desc-text { font-size: 13px; color: #444; line-height: 1.6; text-align: justify; }
    .desc-text ul { padding-left: 18px; margin-top: 5px; margin-bottom: 0; }
    .desc-text p { margin-bottom: 5px; }
`}</style>

        </div>
    );
};
export const defaultVisibility = {
  summary: true, experiences: false, educations: true, skills: false, otherSkills: true,
  projects: true, activities: true, certifications: true, awards: true,
  references: false, hobbies: true, languages: false
};
export default NguyenVoLeKhoa;
