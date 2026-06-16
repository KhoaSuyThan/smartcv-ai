import React from 'react';

const DaoPhuQuy = ({ resumeData }) => {
    return (
        <div className="cv-template-DaoPhuQuy">
            <div className="cv-wrapper">
        <div className="cv-sidebar">
            <div className="cv-avatar-section">
                {resumeData?.avatarUrl && <img src={resumeData.avatarUrl} className="cv-avatar" />}
            </div>
            <div className="cv-sidebar-content">
                <div className="cv-info-block">
                    <h3 className="cv-side-title">THÔNG TIN LIÊN HỆ</h3>
                    <p><i className="fas fa-phone"></i> {resumeData?.phone || 'SĐT'}</p>
                    <p><i className="fas fa-envelope"></i> {resumeData?.email || 'Email'}</p>
                    <p><i className="fas fa-map-marker-alt"></i> {resumeData?.address || 'Địa chỉ'}</p>
                </div>

                <div className="cv-info-block">
                    <h3 className="cv-side-title">HỌC VẤN</h3>
                    <div className="cv-edu-sidebar">{resumeData?.educations?.map((item, idx) => (

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

                <div className="cv-info-block">
                    <h3 className="cv-side-title">TIN HỌC</h3>
                    <div className="cv-skill-list"><ul className="skill-list-items">{resumeData?.skills?.map((item, idx) => (
<li>• {item.name}: {item.level}</li>
))}</ul></div>
                </div>
                <div className="cv-info-block">
                    <h3 className="cv-side-title">NGOẠI NGỮ</h3>
                    <div className="cv-lang-list"><ul className="lang-list-items">{resumeData?.languages?.map((item, idx) => (
<li style={{display:'flex', justifyContent:'space-between'}}><span>• {item.name}</span><span style={{fontStyle:'italic', opacity:0.8}}>{item.level}</span></li>
))}</ul></div>
                </div>
                <div className="cv-info-block">
                    <h3 className="cv-side-title">KỸ NĂNG KHÁC</h3>
                    <div className="cv-other-skills"><ul className="other-skill-list-items">{resumeData?.otherSkills?.map((item, idx) => (
<li>• {item.name} {item.level ? `(${item.level})` : ''}</li>
))}</ul></div>
                </div>
            </div>
        </div>

        <div className="cv-main">
            <div className="cv-header">
                <h1 className="cv-name">{resumeData?.fullName || 'HỌ TÊN'}</h1>
                <h2 className="cv-job">{resumeData?.jobTitle || 'VỊ TRÍ'}</h2>
            </div>
            <div className="cv-section">
                <h3 className="cv-section-title"><i className="fas fa-user"></i> MỤC TIÊU NGHỀ NGHIỆP</h3>
                <div className="cv-section-content">{resumeData?.summary || 'Mục tiêu nghề nghiệp'}</div>
            </div>
            <div className="cv-section">
                <h3 className="cv-section-title"><i className="fas fa-briefcase"></i> KINH NGHIỆM LÀM VIỆC</h3>
                <div className="cv-section-content">{resumeData?.experiences?.map((item, idx) => (

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

            <div className="cv-section">
                <h3 className="cv-section-title"><i className="fas fa-users"></i> NGƯỜI THAM CHIẾU</h3>
                <div className="cv-section-content">{resumeData?.references?.map((item, idx) => (
<p style={{marginBottom:'5px', fontSize:'12px'}}>• {item.info}</p>
))}</div>
            </div>
        </div>
    </div>
    
<style>{`
/* Layout chung */
    .cv-wrapper { display: flex; width: 210mm; min-height: 297mm; background: white; font-family: "Segoe UI", sans-serif; }
    .cv-sidebar { flex: 3.5; background: #004C82; color: white; padding: 30px 20px; }
    .cv-main { flex: 6.5; padding: 40px; background: #ffffff; }

    .cv-avatar-section { text-align: center; margin-bottom: 30px; }
    .cv-avatar { width: 160px; height: 160px; border-radius: 50%; border: 5px solid rgba(255,255,255,0.2); object-fit: cover; }

    .cv-side-title { font-size: 16px; font-weight: bold; border-bottom: 1px solid rgba(255,255,255,0.3); padding-bottom: 5px; margin-bottom: 10px; margin-top: 20px; text-transform: uppercase; }
    
    /* STYLE CHO SIDEBAR VÀ CHỐNG TRÀN */
    .cv-info-block p, .cv-other-skills, .cv-edu-sidebar, .cv-lang-list, .cv-skill-list { 
        font-size: 13px; 
        margin-bottom: 8px; 
        line-height: 1.5;
        word-wrap: break-word; 
        overflow-wrap: break-word; 
        word-break: break-all; 
    }
    .cv-info-block i { width: 20px; text-align: center; }

    /* MAIN CONTENT */
    .cv-header { border-bottom: 3px solid #004C82; padding-bottom: 15px; margin-bottom: 30px; }
    .cv-name { font-size: 40px; font-weight: 800; color: #004C82; margin: 0; text-transform: uppercase; }
    .cv-job { font-size: 18px; color: #555; margin: 5px 0 0 0; text-transform: uppercase; letter-spacing: 2px; }

    .cv-section { margin-bottom: 25px; }
    .cv-section-title { font-size: 17px; font-weight: bold; color: #004C82; display: flex; align-items: center; gap: 10px; margin-bottom: 10px; border-bottom: 1px solid #eee; padding-bottom: 5px; }

    /* NỘI DUNG CHÍNH: CHỐNG TRÀN VÀ CÓ KHUNG */
    .cv-section-content { 
        font-size: 14px; 
        line-height: 1.6; 
        color: #333; 
        text-align: justify; 
        white-space: pre-line; 
        word-wrap: break-word; 
        overflow-wrap: break-word; 
        word-break: break-word; 
    }
`}</style>

        </div>
    );
};
export default DaoPhuQuy;
