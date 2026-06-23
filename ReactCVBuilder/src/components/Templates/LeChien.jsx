import React from 'react';

const LeChien = ({ resumeData }) => {
    return (
        <div className="cv-template-LeChien">
            <div className="cv-elegant-wrapper">
        <div className="header-area">
            <div className="avatar-box">
                <img src={resumeData?.avatarUrl || "/images/default-avatar.png"} className="avatar-img" />
            </div>
            <div className="header-content">
                <h1 className="fullname">{resumeData?.fullName || 'HỌ TÊN'}</h1>
                <p className="job-title">{resumeData?.jobTitle || 'VỊ TRÍ'}</p>
                <div className="summary-box">{resumeData?.summary || 'Mục tiêu nghề nghiệp'}</div>
            </div>
        </div>
        <div className="middle-grid">
            {(resumeData?.birthDate || resumeData?.email || resumeData?.phone || resumeData?.website || resumeData?.address) && (
                <div className="grid-col">
                    <h3 className="section-title">THÔNG TIN CÁ NHÂN</h3>
                    <ul className="contact-list">
                        {resumeData?.birthDate && <li><i className="fas fa-calendar-alt"></i> <span>{resumeData.birthDate}</span></li>}
                        {resumeData?.email && <li><i className="fas fa-envelope"></i> <span>{resumeData.email}</span></li>}
                        {resumeData?.phone && <li><i className="fas fa-phone-alt"></i> <span>{resumeData.phone}</span></li>}
                        {resumeData?.website && <li><i className="fas fa-globe"></i> <span>{resumeData.website}</span></li>}
                        {resumeData?.address && <li><i className="fas fa-map-marker-alt"></i> <span>{resumeData.address}</span></li>}
                    </ul>
                </div>
            )}
            <div className="grid-col">
                <h3 className="section-title">HỌC VẤN</h3>
                <div className="content-text">{resumeData?.educations?.map((item, idx) => (

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
            <div className="grid-col">
                <h3 className="section-title">CHỨNG CHỈ</h3>
                <div className="content-text">{resumeData?.certifications?.map((item, idx) => (

        <div style={{marginBottom:'8px'}}>
            <div className="cert-year-div" style={{fontWeight:'bold', fontSize:'11px', color:'#634c46'}}>{item.year}</div>
            <div className="cert-name-div" style={{fontSize:'12.5px'}}>{item.name}</div>
        </div>
))}</div>
            </div>
        </div>
        <div className="main-section">
            <h3 className="section-title">KINH NGHIỆM LÀM VIỆC</h3>
            <div className="timeline-container">{resumeData?.experiences?.map((item, idx) => (

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
        {/* Khối DỰ ÁN được bổ sung để tận dụng hết form */}
        <div className="main-section">
            <h3 className="section-title">DỰ ÁN NỔI BẬT</h3>
            <div className="timeline-container">{resumeData?.projects?.map((item, idx) => (

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
        <div className="footer-grid">
            <div className="footer-left">
                <h3 className="section-title">KỸ NĂNG</h3>
                <div className="content-text">
                  <div className="skill-group">
                    <p className="skill-type">💻 TIN HỌC</p>
                     <ul className="skill-list-items">{resumeData?.skills?.map((item, idx) => (
<li>• {item.name}: {item.level}</li>
))}</ul>
                  </div>
        
                  <div className="skill-group">
                  <p className="skill-type">🌍 NGOẠI NGỮ</p>
                     <ul className="lang-list-items">{resumeData?.languages?.map((item, idx) => (
<li style={{display:'flex', justifyContent:'space-between'}}><span>• {item.name}</span><span style={{fontStyle:'italic', opacity:0.8}}>{item.level}</span></li>
))}</ul>
                  </div>
        
                   <div className="skill-group">
                   <p className="skill-type">💡 KỸ NĂNG KHÁC</p>
                       <ul className="other-skill-list-items">{resumeData?.otherSkills?.map((item, idx) => (
<li>• {item.name} {item.level ? `(${item.level})` : ''}</li>
))}</ul>
                   </div>
                </div>
                <div className="sub-section">
                    <h3 className="section-title">SỞ THÍCH</h3>
                    <div className="content-text"><ul className="hobby-list-items">{resumeData?.hobbies?.map((item, idx) => (
<li>• {item.name}</li>
))}</ul></div>
                </div>
            </div>
            <div className="footer-right">
                <div className="sub-section">
                    <h3 className="section-title">DANH HIỆU & GIẢI THƯỞNG</h3>
                    <div className="content-text"><ul style={{paddingLeft:'15px', margin:0}}>{resumeData?.awards?.map((item, idx) => (
<li>{item.name}</li>
))}</ul></div>
                </div>
                <div className="sub-section">
                    <h3 className="section-title">HOẠT ĐỘNG</h3>
                    <div className="timeline-container small-timeline">{resumeData?.activities?.map((item, idx) => (

        <div className="exp-item" key={idx}>
            <div className="exp-header">
                <span className="company-name">{item.name}</span>
                <span className="date-badge">{item.time}</span>
            </div>
            <div className="exp-desc" dangerouslySetInnerHTML={{__html: item.desc}}></div>
        </div>
))}</div>
                </div>
                <div className="sub-section">
                    <h3 className="section-title">NGƯỜI GIỚI THIỆU</h3>
                    <div className="content-text">{resumeData?.references?.map((item, idx) => (
<p style={{marginBottom:'5px', fontSize:'12px'}}>• {item.info}</p>
))}</div>
                </div>
            </div>
        </div>
    </div>

    
<style>{`
@import url("https://fonts.googleapis.com/css2?family=Be+Vietnam+Pro:wght@400;500;600;700&display=swap");
    /* CẤU TRÚC A4 CỐ ĐỊNH */
    .cv-elegant-wrapper { 
        width: 100%; min-height: 297mm; background: #fff; padding: 45px 50px; 
        font-family: "Be Vietnam Pro", sans-serif; color: #333; margin: 0 auto; 
        box-sizing: border-box; overflow: hidden; border-bottom: 5px solid #2e8b57;
    }
    .cv-elegant-wrapper * { box-sizing: border-box; word-wrap: break-word; word-break: break-word; }
    /* HEADER */
    .header-area { display: flex; gap: 40px; margin-bottom: 25px; }
    .avatar-img { width: 145px; height: 145px; border-radius: 50%; object-fit: cover; border: 1px solid #ddd; padding: 3px; }
    .header-content { flex: 1; display: flex; flex-direction: column; justify-content: center; }
    .fullname { font-size: 30px; color: #a40000; font-weight: 700; text-transform: uppercase; margin: 0 0 5px 0; letter-spacing: 0.5px; }
    .job-title { font-size: 16px; color: #222; font-weight: 500; margin: 0 0 12px 0; border-bottom: 2px solid #222; padding-bottom: 12px; display: inline-block; width: 100%; }
    .summary-box { font-size: 13.5px; line-height: 1.6; text-align: justify; color: #333; }
    /* TIÊU ĐỀ RED SECTION */
    .section-title { 
        font-size: 14.5px; font-weight: 700; color: #a40000; 
        border-bottom: 2px solid #a40000; padding-bottom: 6px; 
        margin: 0 0 15px 0 !important; text-transform: uppercase; letter-spacing: 0.5px;
    }
    .main-section { margin-bottom: 30px; }
    .main-section:empty { display: none; }
    /* 3 COL GRID (Thông tin, Học Vấn, Chứng chỉ) */
    .middle-grid { display: grid; grid-template-columns: 1.3fr 1fr 1fr; gap: 25px; margin-bottom: 30px; }
    
    /* CONTACT LIST */
    .contact-list { list-style: none; padding: 0; }
    .contact-list li { margin-bottom: 10px; display: flex; gap: 12px; align-items: center; font-size: 13px; color: #333; }
    .contact-list i { background: #a40000; color: white !important; width: 22px; height: 22px; border-radius: 4px; display: flex; align-items: center; justify-content: center; font-size: 11px; flex-shrink: 0; }
    /* GRID COL (Học vấn & Chứng chỉ) CSS Ghi đè cấu trúc C# */
    .grid-col .exp-item { display: flex; flex-direction: column; margin-bottom: 15px; }
    .grid-col .exp-year { order: 2; font-size: 12.5px; color: #555; margin-top: 4px; }
    .grid-col .exp-content { order: 1; font-size: 12.5px; line-height: 1.5; color: #444; }
    .grid-col .exp-content strong { font-size: 13.5px; text-transform: uppercase; color: #111; font-weight: 700; }
    
    .grid-col .content-text > div > div:first-child { color: #111 !important; font-size: 13.5px !important; margin-bottom: 2px; font-weight: bold; }
    .grid-col .content-text > div > div:nth-child(2) { color: #555 !important; font-size: 12.5px !important; text-transform: uppercase; line-height: 1.4; }
    /* -------------------------------------
       TIMELINE TRUNG TÂM (Trải nghiệm, Dự án) 
       Sử dụng sức mạnh CSS Absolute
       để tách 1 Row HTML thành 2 Cột !!
    -------------------------------------- */
    .timeline-container .exp-item {
        position: relative; padding-left: 30%; margin-bottom: 25px; min-height: 60px;
    }
    .timeline-container .exp-item::before {
        content: ""; position: absolute; left: 30%; top: 6px; width: 2px; height: calc(100% + 20px); background: #ccc;
    }
    .timeline-container .exp-item:last-child::before { display: none; }
    
    .timeline-container .exp-item::after {
        content: ""; position: absolute; left: calc(30% - 4px); top: 6px; width: 10px; height: 10px; border-radius: 50%; background: #a40000;
    }
    
    .timeline-container .exp-year {
        position: absolute; left: 0; top: 3px; width: 28%; font-weight: bold; font-size: 13.5px; color: #222; margin-left: -5px; /* Giấu nhẹ dấu chấm do C# sinh ra */
    }
    .timeline-container .exp-content { padding-left: 20px; }
    
    .timeline-container .info-line:first-child {
        position: absolute; left: 0; top: 24px; width: 28%; font-weight: bold; font-size: 13.5px; color: #111;
    }
    .timeline-container .info-line:first-child strong { display: none; }
    
    .timeline-container .info-line:nth-child(2) {
        font-weight: bold; font-size: 14.5px; color: #000; margin-bottom: 6px;
    }
    .timeline-container .info-line:nth-child(2) strong { display: none; }
    
    .timeline-container .desc-text { font-size: 13px; line-height: 1.6; color: #333; text-align: justify; }
    /* FOOTER 2 CỘT */
    .footer-grid { display: flex; gap: 50px; }
    .footer-left { flex: 4; }
    .footer-right { flex: 6; }
    .sub-section { margin-bottom: 25px; }
    /* SMALL TIMELINE CHO HOẠT ĐỘNG (Dồn lại 1 cột) */
    .small-timeline .exp-item { padding-left: 18px; margin-bottom: 15px; }
    .small-timeline .exp-item::before { left: 0; }
    .small-timeline .exp-item::after { left: -4px; width: 10px; height: 10px; top: 6px; }
    .small-timeline .exp-year { position: static; width: auto; font-weight: bold; font-size: 12.5px; margin-bottom: 3px; display: block; }
    .small-timeline .exp-content { padding-left: 0; }
    .small-timeline .info-line:first-child { position: static; width: auto; font-size: 14px; font-weight: bold; text-transform: uppercase; margin-bottom: 2px; }
    .small-timeline .info-line:nth-child(2) { font-size: 13.5px; color: #555; margin-bottom: 6px; font-weight: normal; }
    /* Danh sách kỹ năng, sở thích */
    .content-text ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
    .content-text li { font-size: 13px; color: #333; padding: 6px 0; display: flex; line-height: 1.4; border-bottom: 1px dashed #e0e0e0; margin: 0 !important;}
    .content-text li:last-child { border-bottom: none; }
    /* References */
    .content-text p { font-size: 13px; font-style: italic; color: #555; margin-bottom: 5px; }
/* Tối ưu cho nhóm kỹ năng gộp */
.skill-group {
    margin-bottom: 15px; /* Khoảng cách giữa các nhóm nhỏ */
}

.skill-group:last-child {
    margin-bottom: 0;
}

.skill-type {
    font-size: 12px !important;
    font-weight: 700 !important;
    margin-bottom: 5px !important;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    padding: 2px 8px;
    display: inline-block;
    border-radius: 4px;
}

/* Đảm bảo danh sách bên dưới label không bị margin quá lớn */
.skill-group ul {
    margin-top: 2px !important;
}

.skill-group li {
    border-bottom: 1px dashed #eee; /* Đường kẻ mờ phân cách các kỹ năng lẻ */
    padding: 4px 0 !important;
}
`}</style>

        </div>
    );
};
export const defaultVisibility = {
  summary: true, experiences: true, educations: true, skills: false, otherSkills: false,
  projects: false, activities: false, certifications: true, awards: false,
  references: false, hobbies: false, languages: false
};
export default LeChien;
