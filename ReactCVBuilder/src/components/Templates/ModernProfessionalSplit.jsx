import React from 'react';

const ModernProfessionalSplit = ({ resumeData }) => {
    return (
        <div className="cv-template-ModernProfessionalSplit">
            <div className="cv-dual-wrapper">
        <div className="left-sidebar">
            <div className="avatar-container">
                <img className="avatar-img" src={resumeData?.avatarUrl || "/images/default-avatar.png"} />
            </div>
            
            <div className="sidebar-padding">
                {(resumeData?.phone || resumeData?.email || resumeData?.address || resumeData?.website) && (
                    <div className="contact-box">
                        <ul>
                            {resumeData?.phone && <li><i className="fas fa-phone"></i> <span>{resumeData.phone}</span></li>}
                            {resumeData?.email && <li><i className="fas fa-envelope"></i> <span>{resumeData.email}</span></li>}
                            {resumeData?.address && <li><i className="fas fa-map-marker-alt"></i> <span>{resumeData.address}</span></li>}
                            {resumeData?.website && <li><i className="fas fa-link"></i> <span>{resumeData.website}</span></li>}
                        </ul>
                    </div>
                )}
                
                <div className="side-divider"></div>
                
                <div className="section" style={{ display: resumeData.visibleSections?.summary !== false ? 'block' : 'none' }}>
                    <h3 style={{ display: 'none' }}>Về tôi</h3>
                    <div className="side-summary">{resumeData?.summary || 'Mục tiêu nghề nghiệp'}</div>
                
                    <div className="side-divider"></div>
                </div>

                <div className="side-section">
                    <h3 className="side-title">Kỹ năng</h3>
                    <div className="skills-list"><ul className="skill-list-items">{resumeData?.skills?.map((item, idx) => (
<li>• {item.name}: {item.level}</li>
))}</ul></div>
                    <div className="skills-list"><ul className="other-skill-list-items">{resumeData?.otherSkills?.map((item, idx) => (
<li>• {item.name} {item.level ? `(${item.level})` : ''}</li>
))}</ul></div>
                </div>

                <div className="side-divider"></div>

                <div className="side-section">
                    <h3 className="side-title">Chứng chỉ</h3>
                    {/* CSS sẽ lật ngược Năm lên trên, Tên chứng chỉ xuống dưới */}
                    <div className="side-cert">{resumeData?.certifications?.map((item, idx) => (

        <div style={{marginBottom:'8px'}}>
            <div className="cert-year-div" style={{fontWeight:'bold', fontSize:'11px', color:'#634c46'}}>{item.year}</div>
            <div className="cert-name-div" style={{fontSize:'12.5px'}}>{item.name}</div>
        </div>
))}</div>
                </div>

                <div className="side-divider"></div>

                <div className="side-section">
                    <h3 className="side-title">Sở thích</h3>
                    <div className="skills-list"><ul className="hobby-list-items">{resumeData?.hobbies?.map((item, idx) => (
<li>• {item.name}</li>
))}</ul></div>
                </div>
            </div>
        </div>

        <div className="right-main">
            <div className="header-box">
                <h1 className="fullname">{resumeData?.fullName || 'HỌ TÊN'}</h1>
                <h2 className="job-title">{resumeData?.jobTitle || 'VỊ TRÍ'}</h2>
            </div>

            <div className="main-content">
                <div className="content-section education-section">
                    <h3 className="main-title">Học vấn</h3>
                    <div className="content-area">{resumeData?.educations?.map((item, idx) => (

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

                <div className="content-section act-section">
                    <h3 className="main-title">Hoạt động</h3>
                    <div className="content-area">{resumeData?.activities?.map((item, idx) => (

        <div className="exp-item" key={idx}>
            <div className="exp-header">
                <span className="company-name">{item.name}</span>
                <span className="date-badge">{item.time}</span>
            </div>
            <div className="exp-desc" dangerouslySetInnerHTML={{__html: item.desc}}></div>
        </div>
))}</div>
                </div>

                <div className="content-section project-section">
                    <h3 className="main-title">Dự án</h3>
                    <div className="content-area">{resumeData?.projects?.map((item, idx) => (

        <div className="exp-item" key={idx}>
            <span className="exp-year">• {item.time}</span>
            <div className="exp-content">
                <div className="info-line"><strong>Dự án:</strong> {item.name}</div>
                <div className="info-line"><strong>Vai trò:</strong> {item.role}</div>
                <div className="desc-text" dangerouslySetInnerHTML={{__html: item.desc}}></div>
            </div>
        </div>
))}</div>
                </div>

                <div className="content-section exp-section">
                    <h3 className="main-title">Kinh nghiệm làm việc</h3>
                    <div className="content-area">{resumeData?.experiences?.map((item, idx) => (

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
                
                <div className="content-section awards-section">
                    <h3 className="main-title">Giải thưởng & Tham chiếu</h3>
                    <div className="content-area"><ul style={{paddingLeft:'15px', margin:0}}>{resumeData?.awards?.map((item, idx) => (
<li>{item.name}</li>
))}</ul></div>
                    <div className="content-area ref-block">{resumeData?.references?.map((item, idx) => (
<p style={{marginBottom:'5px', fontSize:'12px'}}>• {item.info}</p>
))}</div>
                </div>
            </div>
        </div>
    </div>

    
<style>{`
@import url("https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap");

    /* CẤU TRÚC A4 CỐ ĐỊNH SPLIT VIEW */
    .cv-dual-wrapper {
        display: flex; width: 100%; min-height: 297mm; background: #fff;
        font-family: "Inter", sans-serif; margin: 0 auto; box-sizing: border-box; overflow: hidden;
    }
    .cv-dual-wrapper * { box-sizing: border-box; word-wrap: break-word; }

    /* LAYOUT 2 CỘT */
    .left-sidebar { width: 33.5%; background: #5B626B; color: #fff; display: flex; flex-direction: column; }
    .right-main { width: 66.5%; background: #fff; display: flex; flex-direction: column; padding-bottom: 40px; }

    /* AVATAR & HEADER ĐỒNG BỘ CHIỀU CAO */
    .avatar-container { width: 100%; height: 215px; flex-shrink: 0; }
    .avatar-img { width: 100%; height: 100%; object-fit: cover; }
    .header-box { width: 100%; height: 215px; background: #B3BBC5; display: flex; flex-direction: column; justify-content: center; padding-left: 45px; flex-shrink: 0; }

    .fullname { font-size: 32px; font-weight: 700; color: #111; margin: 0 0 8px 0; letter-spacing: -0.5px; }
    .job-title { font-size: 16.5px; font-weight: 500; color: #333; margin: 0; }

    /* PADDING NỘI DUNG */
    .sidebar-padding { padding: 35px 30px; }
    .main-content { padding: 35px 45px; }

    /* ----- CỘT TRÁI (LEFT SIDEBAR) ----- */
    .contact-box ul { list-style: none; padding: 0; margin: 0; }
    .contact-box li { display: flex; align-items: flex-start; gap: 12px; margin-bottom: 12px; font-size: 12.5px; line-height: 1.5; }
    .contact-box i { width: 16px; text-align: center; font-size: 13px; margin-top: 3px; opacity: 0.9; }
    .side-divider { width: 100%; border-bottom: 1px solid rgba(255, 255, 255, 0.25); margin: 22px 0; }
    .side-summary { font-size: 13px; line-height: 1.6; text-align: justify; }

    .side-title { font-size: 16px; font-weight: 700; text-transform: uppercase; margin-bottom: 15px; letter-spacing: 0.5px; color: #fff; }
    
    .skills-list ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
    .skills-list li { margin-bottom: 10px; font-size: 13px; line-height: 1.5; color: #fff; display: flex; }
    .skills-list li::first-letter { font-size: 0; color: transparent; }

    .side-cert > div { display: flex; flex-direction: column; margin-bottom: 15px !important; }
    .side-cert .cert-year-div { order: 1; font-size: 12.5px !important; font-weight: 600 !important; color: #9AADC3 !important; margin-bottom: 4px; text-transform: uppercase; }
    .side-cert .cert-name-div { order: 2; font-size: 13px !important; font-weight: 400 !important; color: #fff !important; line-height: 1.5; }

    /* ----- CỘT PHẢI (RIGHT MAIN) ----- */
    .content-section { margin-bottom: 25px; }
    .content-section:empty, .content-area:empty { display: none !important; }
    .main-title {
        background: #88929B; color: #fff; padding: 9px 18px; font-size: 15.5px; font-weight: 600; text-transform: uppercase;
        margin: 0 0 20px 0 !important; display: block; width: 100%; letter-spacing: 0.5px;
    }

    /* Ẩn chữ Công ty: Vị trí: */
    .right-main .info-line strong { display: none; }
    .right-main .exp-year::first-letter { font-size: 0; color: transparent; } /* Ẩn Bullet • */

    /* MAGIC CSS THAY ĐỔI VỊ TRÍ (KINH NGHIỆM, DỰ ÁN) */
    .exp-section .exp-item, .project-section .exp-item { display: flex; flex-direction: column; margin-bottom: 22px; }
    .exp-section .exp-content, .project-section .exp-content { display: contents; } /* Giải nén hộp con */
    
    .exp-section .info-line:first-child, .project-section .info-line:first-child {
        order: 1; font-weight: 700; font-size: 15px; color: #111; margin-bottom: 4px; text-transform: uppercase;
    }
    .exp-section .info-line:nth-child(2), .project-section .info-line:nth-child(2) {
        order: 2; font-size: 14px; color: #5B626B; margin-bottom: 4px;
    }
    .exp-section .exp-year, .project-section .exp-year {
        order: 3; font-size: 13.5px; color: #666; margin-bottom: 10px; font-weight: 500;
    }
    .exp-section .desc-text, .project-section .desc-text {
        order: 4; font-size: 13.5px; line-height: 1.6; color: #333; text-align: justify;
    }

    /* SẮP XẾP VỊ TRÍ HOẠT ĐỘNG (Activity Header structure) */
    .act-section .exp-item { display: flex; flex-direction: column; margin-bottom: 22px; }
    .act-section .exp-header { display: contents; }
    .act-section .company-name { order: 1; font-weight: 700; font-size: 15px; color: #111; margin-bottom: 4px; }
    .act-section .date-badge { order: 2; font-size: 13.5px; color: #666; margin-bottom: 10px; font-weight: 500; }
    .act-section .exp-desc { order: 3; font-size: 13.5px; line-height: 1.6; color: #333; text-align: justify; }
    /* Fix bullet khoảng trắng cho ul li */
    .desc-text ul { padding-left: 20px; }

    /* SẮP XẾP VỊ TRÍ HỌC VẤN */
    .education-section .exp-item { display: flex; flex-direction: column; margin-bottom: 20px; }
    .education-section .exp-content { order: 1; font-weight: 400; color: #5B626B; font-size: 14px; line-height: 1.6; }
    .education-section .exp-content strong { color: #111; font-size: 15px; font-weight: 700; display: block; margin-bottom: 2px; }
    .education-section .exp-year { order: 2; color: #666; font-size: 13.5px; margin-top: 4px; font-weight: 500; }

    /* GIẢI THƯỞNG & THAM CHIẾU */
    .awards-section ul { list-style: none !important; padding: 0 !important; margin: 0 0 10px 0 !important; }
    .awards-section li { padding: 4px 0; font-size: 13.5px; color: #333; }
    .awards-section li::first-letter { font-size: 0; color: transparent; }

    .ref-block p { font-size: 13.5px !important; margin-bottom: 8px !important; line-height: 1.5; color: #333; }
    .ref-block p::first-letter { font-size: 0; color: transparent; }
`}</style>

        </div>
    );
};
export const defaultVisibility = {
  summary: true, experiences: false, educations: true, skills: true, otherSkills: true,
  projects: true, activities: true, certifications: true, awards: false,
  references: false, hobbies: true, languages: false
};
export default ModernProfessionalSplit;
