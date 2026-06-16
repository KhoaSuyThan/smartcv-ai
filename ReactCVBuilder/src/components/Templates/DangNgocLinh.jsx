import React from 'react';

const DangNgocLinh = ({ resumeData }) => {
    return (
        <div className="cv-template-DangNgocLinh">
            <div className="cv-classic-wrapper">
        <div className="header-section">
            <h1 className="fullname">{resumeData?.fullName || 'HỌ TÊN'}</h1>
            <h2 className="job-title">{resumeData?.jobTitle || 'VỊ TRÍ'}</h2>
            <div className="contact-info">
                <span><i className="fas fa-phone-alt"></i> {resumeData?.phone || 'SĐT'}</span>
                <span><i className="fas fa-envelope"></i> {resumeData?.email || 'Email'}</span>
                <span><i className="fas fa-globe"></i> {resumeData?.website || 'Website'}</span>
                <span><i className="fas fa-map-marker-alt"></i> {resumeData?.address || 'Địa chỉ'}</span>
            </div>
        </div>
        <div className="section summary-section">
            <h3 className="section-title">MỤC TIÊU NGHỀ NGHỆP</h3>
            <div className="summary-text">{resumeData?.summary || 'Mục tiêu nghề nghiệp'}</div>
        </div>
        <div className="section education-section">
            <h3 className="section-title">HỌC VẤN</h3>
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
        <div className="section exp-section">
            <h3 className="section-title">KINH NGHIỆM LÀM VIỆC</h3>
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
        
        {/* Bổ sung DỰ ÁN theo chuẩn chung */}
        <div className="section project-section">
            <h3 className="section-title">DỰ ÁN NỔI BẬT</h3>
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
        <div className="section skills-section">
            <h3 className="section-title">KỸ NĂNG</h3>
            {/* Render cả kỹ năng đặc thù (IT) và kỹ năng khác */}
            <div className="content-area"><ul className="skill-list-items">{resumeData?.skills?.map((item, idx) => (
<li>• {item.name}: {item.level}</li>
))}</ul></div>
            <div className="content-area"><ul className="other-skill-list-items">{resumeData?.otherSkills?.map((item, idx) => (
<li>• {item.name} {item.level ? `(${item.level})` : ''}</li>
))}</ul></div>
        </div>
        <div className="section act-section">
            <h3 className="section-title">HOẠT ĐỘNG</h3>
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
        <div className="section cert-section">
            <h3 className="section-title">CHỨNG CHỈ</h3>
            <div className="content-area">{resumeData?.certifications?.map((item, idx) => (

        <div style={{marginBottom:'8px'}}>
            <div className="cert-year-div" style={{fontWeight:'bold', fontSize:'11px', color:'#634c46'}}>{item.year}</div>
            <div className="cert-name-div" style={{fontSize:'12.5px'}}>{item.name}</div>
        </div>
))}</div>
        </div>
        <div className="section awards-section">
            <h3 className="section-title">DANH HIỆU & GIẢI THƯỞNG</h3>
            <div className="content-area"><ul style={{paddingLeft:'15px', margin:0}}>{resumeData?.awards?.map((item, idx) => (
<li>{item.name}</li>
))}</ul></div>
        </div>
        <div className="section ref-section">
            <h3 className="section-title">NGƯỜI GIỚI THIỆU</h3>
            <div className="content-area">{resumeData?.references?.map((item, idx) => (
<p style={{marginBottom:'5px', fontSize:'12px'}}>• {item.info}</p>
))}</div>
        </div>
        <div className="section hobbies-section">
            <h3 className="section-title">SỞ THÍCH</h3>
            <div className="content-area"><ul className="hobby-list-items">{resumeData?.hobbies?.map((item, idx) => (
<li>• {item.name}</li>
))}</ul></div>
        </div>
    </div>


    
<style>{`
@import url("https://fonts.googleapis.com/css2?family=Lora:ital,wght@0,400;0,600;0,700;1,400;1,600&display=swap");
    /* CẤU TRÚC A4 CỐ ĐỊNH */
    .cv-classic-wrapper { 
        width: 100%; min-height: 297mm; background: #fff; padding: 45px 50px; 
        font-family: "Lora", "Times New Roman", serif; color: #111; margin: 0 auto; 
        box-sizing: border-box; overflow: hidden; line-height: 1.6;
    }
    .cv-classic-wrapper * { box-sizing: border-box; word-wrap: break-word; word-break: break-word; }
    /* HEADER */
    .header-section { text-align: center; margin-bottom: 25px; }
    .fullname { font-size: 28px; font-weight: 700; text-transform: uppercase; margin: 0 0 5px 0; color: #000; letter-spacing: 1px; }
    .job-title { font-size: 16px; font-weight: 600; color: #222; margin: 0 0 15px 0; }
    .contact-info { display: flex; justify-content: center; flex-wrap: wrap; gap: 15px; font-size: 13.5px; color: #111; }
    .contact-info span { display: flex; align-items: center; gap: 5px; }
    /* LAYOUT SECTION CƠ BẢN */
    .section { margin-bottom: 25px; }
    .section:empty, .content-area:empty { display: none !important; }
    /* SECTION TITLE (Gạch dưới Dài Full Box) */
    .section-title {
        font-size: 16px; font-weight: 700; text-transform: uppercase; color: #000;
        border-bottom: 1.5px solid #000; padding-bottom: 6px; margin: 0 0 15px 0 !important;
    }
    /* MỤC TIÊU NGHỀ NGHỆP */
    .summary-text { font-size: 13.5px; line-height: 1.6; text-align: justify; color: #222; }
    /* -------------------------------
       CẤU TRÚC FLEX LEFT-RIGHT 
       (Học vấn, Kinh nghiệm, Dự án) 
    ------------------------------- */
    .exp-section .exp-item, .project-section .exp-item, .education-section .exp-item {
        position: relative; margin-bottom: 18px; padding-right: 130px; /* Chừa biên phải cho NĂM */
    }
    /* NĂM (Đẩy sang cực phải) */
    .exp-year {
        display: block; position: absolute; right: 0; top: 0; width: 130px; text-align: right; 
        font-size: 14.5px; color: #333; font-weight: normal; margin-top: 1px;
    }
    /* Loại bỏ dấu chấm tròn tự động sinh ra tử JS */
    .exp-year::first-letter { font-size: 0; color: transparent; }
    /* TÊN CÔNG TY (Thẻ khối đầu tiên) */
    .exp-content .info-line:first-child { font-size: 15px; font-weight: 700; color: #000; margin-bottom: 3px; }
    .exp-content .info-line:first-child strong { display: none; }
    /* TÊN VỊ TRÍ (Thẻ khối số 2) */
    .exp-content .info-line:nth-child(2) { font-size: 14.5px; font-weight: 700; color: #111; margin-bottom: 6px; }
    .exp-content .info-line:nth-child(2) strong { display: none; }
    /* MÔ TẢ (Desc text dạng list/câu dài) */
    .desc-text { font-size: 13.5px; line-height: 1.6; text-align: justify; color: #222; }
    .desc-text ul { padding-left: 20px; }
    /* HỌC VẤN (Override style vì khác tag) */
    .education-section .exp-content strong { font-size: 15px; font-weight: 700; color: #000; display: block; margin-bottom: 3px; }
    .education-section .exp-content { font-size: 14px; line-height: 1.6; }
    /* -------------------------------
       CÁC PHẦN ĐƠN GIẢN (Kỹ năng, Hoạt động, Chứng chỉ) 
    ------------------------------- */
    /* KỸ NĂNG: Gạch kẻ chân dưới */
    .skills-section ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
    .skills-section li { padding: 8px 0; border-bottom: 1px dashed #ccc; font-size: 14px; }
    .skills-section li:last-child { border-bottom: none; }
    .skills-section li::first-letter { font-size: 0; color: transparent; }
    /* HOẠT ĐỘNG: Flexbox Header Trái Phải */
    .act-section .exp-header { display: flex; justify-content: space-between; margin-bottom: 6px; }
    .act-section .company-name { font-weight: 700; font-size: 15px; color: #000; }
    .act-section .date-badge { font-size: 14px; color: #333; }
    .act-section .exp-desc { font-size: 13.5px; line-height: 1.6; text-align: justify; }
    /* CHỨNG CHỈ (Flex đổi vị trí Tên ở Trái - Năm ở Phải) */
    /* Phải selector qua > div để target đúng vào cấu trúc render của C# */
    .cert-section .content-area > div { display: flex; justify-content: space-between; border-bottom: 1px dashed #e0e0e0; padding-bottom: 8px; margin-bottom: 12px !important; }
    .cert-section .content-area > div:last-child { border-bottom: none; }
    .cert-section .content-area > div > div:first-child { order: 2; font-size: 14px !important; font-weight: normal !important; color: #333 !important; }
    .cert-section .content-area > div > div:nth-child(2) { order: 1; font-size: 14.5px !important; font-weight: 700 !important; color: #000 !important; }
    /* DANH HIỆU & GIẢI THƯỞNG */
    .awards-section ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
    .awards-section li { padding: 6px 0; font-size: 14px; border-bottom: 1px dashed #e0e0e0; }
    .awards-section li:last-child { border-bottom: none; }
    .awards-section li::first-letter { font-size: 0; color: transparent; }
    /* NGƯỜI GIỚI THIỆU: Gắn gọn inline text */
    .ref-section p { font-size: 14px !important; margin-bottom: 8px !important; line-height: 1.5; color: #222; }
    .ref-section p::first-letter { font-size: 0; color: transparent; }
    /* SỞ THÍCH: Giao diện Inline như các tag */
    .hobbies-section ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
    .hobbies-section li { display: inline-block; margin: 0 15px 10px 0 !important; font-size: 14px; }
    .hobbies-section li::first-letter { font-size: 0; color: transparent; }
`}</style>

        </div>
    );
};
export default DangNgocLinh;
