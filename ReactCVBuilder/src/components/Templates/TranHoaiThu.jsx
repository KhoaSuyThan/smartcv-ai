import React from 'react';

const TranHoaiThu = ({ resumeData }) => {
    return (
        <div className="cv-template-TranHoaiThu">
            <div className="cv-blue-wrapper">
        {/* Khung viền trang trí Background */}
        <div className="shape-top-left"></div>
        <div className="shape-top-left-edge"></div>
        <div className="shape-bottom-right"></div>
        <div className="shape-bottom-right-edge"></div>
        
        <div className="layout-grid">
            <div className="left-col">
                <div className="avatar-box">
                    {resumeData?.avatarUrl && <img src={resumeData.avatarUrl} className="avatar-img"  />}
                </div>
                
                <div className="side-content">
                    <div className="side-block skills-section">
                        <h3 className="section-title">Kỹ năng</h3>
                        <div className="skills-list">
                            <ul className="skill-list-items">{resumeData?.skills?.map((item, idx) => (
<li>• {item.name}: {item.level}</li>
))}</ul>
                            <ul className="other-skill-list-items">{resumeData?.otherSkills?.map((item, idx) => (
<li>• {item.name} {item.level ? `(${item.level})` : ''}</li>
))}</ul>
                        </div>
                    </div>
                    <div className="side-block awards-section">
                        <h3 className="section-title">Giải thưởng</h3>
                        <div className="awards-list"><ul style={{paddingLeft:'15px', margin:0}}>{resumeData?.awards?.map((item, idx) => (
<li>{item.name}</li>
))}</ul></div>
                    </div>
                    <div className="side-block cert-section">
                        <h3 className="section-title">Chứng chỉ</h3>
                        <div className="cert-list">{resumeData?.certifications?.map((item, idx) => (

        <div style={{marginBottom:'8px'}}>
            <div className="cert-year-div" style={{fontWeight:'bold', fontSize:'11px', color:'#634c46'}}>{item.year}</div>
            <div className="cert-name-div" style={{fontSize:'12.5px'}}>{item.name}</div>
        </div>
))}</div>
                    </div>
                    <div className="side-block hobbies-section">
                        <h3 className="section-title">Sở thích</h3>
                        <div className="skills-list"><ul className="hobby-list-items">{resumeData?.hobbies?.map((item, idx) => (
<li>• {item.name}</li>
))}</ul></div>
                    </div>
                    <div className="side-block ref-section">
                        <h3 className="section-title">Người tham chiếu</h3>
                        <div className="ref-list">{resumeData?.references?.map((item, idx) => (
<p style={{marginBottom:'5px', fontSize:'12px'}}>• {item.info}</p>
))}</div>
                    </div>
                </div>
            </div>
            
            <div className="right-col">
                <div className="header-content">
                    <h1 className="fullname">{resumeData?.fullName || 'HỌ TÊN'}</h1>
                    <h2 className="job-title">{resumeData?.jobTitle || 'VỊ TRÍ'}</h2>
                    
                    <div className="contact-info">
                        <div className="contact-item"><i className="fas fa-phone-alt"></i> <span>{resumeData?.phone || 'SĐT'}</span></div>
                        <div className="contact-item"><i className="fas fa-envelope"></i> <span>{resumeData?.email || 'Email'}</span></div>
                        <div className="contact-item"><i className="fas fa-globe"></i> <span>{resumeData?.website || 'Website'}</span></div>
                        <div className="contact-item"><i className="fas fa-map-marker-alt"></i> <span>{resumeData?.address || 'Địa chỉ'}</span></div>
                    </div>
                </div>
                
                <div className="main-content">
                    <div className="content-block summary-section">
                        <h3 className="section-title">Mục tiêu nghề nghiệp</h3>
                        <div className="summary-text">{resumeData?.summary || 'Mục tiêu nghề nghiệp'}</div>
                    </div>
                    <div className="content-block education-section">
                        <h3 className="section-title">Trình độ học vấn</h3>
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
                    <div className="content-block exp-section">
                        <h3 className="section-title">Kinh nghiệm làm việc</h3>
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
                    <div className="content-block act-section">
                        <h3 className="section-title">Hoạt động</h3>
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
                    <div className="content-block project-section">
                        <h3 className="section-title">Dự án tham gia</h3>
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
                </div>
            </div>
        </div>
    </div>

    
<style>{`
@import url("https://fonts.googleapis.com/css2?family=Roboto:wght@400;500;700;900&display=swap");
    /* KHUNG BAO CV TOÀN BỘ */
    .cv-blue-wrapper {
        position: relative; width: 100%; min-height: 297mm; background: #FFF;
        font-family: "Roboto", Arial, sans-serif; box-sizing: border-box; overflow: hidden;
    }
    .cv-blue-wrapper * { box-sizing: border-box; word-wrap: break-word; }
    /* SHAPES TRANG TRÍ MÉP CV */
    .shape-top-left { position: absolute; top: 0; left: 0; width: 33%; height: 20px; background: #69A2C2; }
    .shape-top-left-edge { position: absolute; top: 0; left: 0; width: 22px; height: 180px; background: #69A2C2; border-bottom-right-radius: 6px; }
    
    .shape-bottom-right { position: absolute; bottom: 0; right: 0; width: 22px; height: 350px; background: #69A2C2; border-top-left-radius: 6px; }
    .shape-bottom-right-edge { position: absolute; bottom: 0; right: 0; width: 55%; height: 20px; background: #69A2C2; }
    /* KHUNG LƯỚI TRÁI / PHẢI */
    .layout-grid { display: flex; width: 100%; position: relative; z-index: 2; }
    .left-col { width: 34%; padding: 0 20px 30px 45px; }
    .right-col { width: 66%; padding: 35px 45px 30px 15px; }
    /* AVATAR TRÒN CÓ VIỀN */
    .avatar-box { display: flex; justify-content: center; margin-top: 45px; margin-bottom: 30px; }
    .avatar-img { 
        width: 175px; height: 175px; border-radius: 50%; object-fit: cover;
        border: 2.5px solid #111; padding: 5px; background: #fff; box-shadow: 0 4px 10px rgba(0,0,0,0.05);
    }
    /* HEADER: TÊN VÀ LIÊN HỆ BÊN PHẢI */
    .header-content { margin-bottom: 30px; }
    .fullname { font-size: 30px; font-weight: 900; text-transform: uppercase; margin: 0 0 6px 0; color: #111; letter-spacing: 0.5px; }
    .job-title { font-size: 14.5px; font-weight: 600; text-transform: uppercase; margin: 0 0 20px 0; color: #333; letter-spacing: 0.5px; }
    .contact-info { display: flex; flex-direction: column; gap: 10px; }
    .contact-item { display: flex; align-items: center; gap: 12px; font-size: 13.5px; color: #222; }
    .contact-item i {
        background: #69A2C2; color: #fff; width: 22px; height: 22px; border-radius: 50%;
        display: flex; align-items: center; justify-content: center; font-size: 11px;
    }
    /* TIÊU ĐỀ NỔI BẬT */
    .section-title { font-size: 17.5px; font-weight: 700; color: #2E86AB; text-transform: uppercase; margin: 0 0 8px 0 !important; }
    /* -------------------------------------
       CỘT TRÁI (LEFT CONTENT) 
    -------------------------------------- */
    .side-block { margin-bottom: 22px; }
    .side-block:empty { display: none; }
    
    /* Gửi lệnh gạch ngang thủ công cho danh sách, ẩn chấm bi từ JS C# */
    .skills-list ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
    .skills-list li { position: relative; padding-left: 10px; margin-bottom: 6px; font-size: 13.5px; color: #222; line-height: 1.5; }
    .skills-list li::first-letter { font-size: 0; color: transparent; }
    .skills-list li::before { content: "-"; position: absolute; left: 0; top: 0; color: #222; }
    .awards-list ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
    .awards-list li { font-size: 13.5px; color: #222; margin-bottom: 8px; line-height: 1.5; }
    .awards-list li::first-letter { font-size: 0; color: transparent; }
    .ref-list p { font-size: 13.5px; color: #222; margin-bottom: 8px; line-height: 1.5; }
    .ref-list p::first-letter { font-size: 0; color: transparent; }
    /* THIẾT KẾ CHỨNG CHỈ (Khung đơn) */
    .cert-section .cert-item-div { display: flex; flex-direction: column; margin-bottom: 8px; }
    .cert-section .cert-year-div { font-size: 13.5px; font-weight: 700; color: #333; margin-bottom: 2px; }
    .cert-section .cert-year-div:empty { display: none; } /* Nếu rỗng ẩn đi */
    .cert-section .cert-name-div { font-size: 13.5px; position: relative; padding-left: 10px; color: #222; line-height: 1.5; }
    .cert-section .cert-name-div::before { content: "-"; position: absolute; left: 0; top: 0; }
    /* -------------------------------------
       CỘT PHẢI (RIGHT CONTENT)
    -------------------------------------- */
    .content-block { margin-bottom: 22px; }
    .content-block:empty, .content-area:empty { display: none; }
    
    .summary-text { font-size: 14px; line-height: 1.6; text-align: justify; color: #333; }
    /* HỌC VẤN (SẮP XẾP RIÊNG BIỆT VỚI THỜI GIAN Ở DƯỚI) */
    .education-section .exp-item { display: flex; flex-direction: column; margin-bottom: 15px; }
    .education-section .exp-content { order: 1; font-size: 14px; line-height: 1.6; color: #333; }
    .education-section .exp-content strong { font-size: 14.5px; color: #111; display: block; margin-bottom: 2px; font-weight: 500;}
    .education-section .exp-year { order: 2; font-size: 14px; margin-top: 3px; color: #333; }
    .education-section .exp-year::first-letter { font-size: 0; color: transparent; }
    .education-section .exp-year::before { content: "Thời gian : "; }
    /* CHỈ NHẮM RIÊNG KINH NGHIỆM LÀM VIỆC & DỰ ÁN ĐỂ QUẢN LÝ THUỘC TÍNH FLEX-ORDER */
    .exp-section .exp-item, .project-section .exp-item { display: flex; flex-direction: column; margin-bottom: 20px; }
    .exp-section .exp-content, .project-section .exp-content { display: contents; }
    
    /* Ưu tiên 1. Tên công ty / Tên dự án (Bỏ tiền tố "Công ty:") */
    .exp-section .info-line:first-child, .project-section .info-line:first-child { order: 1; font-weight: 500; font-size: 14.5px; color: #111; margin-bottom: 3px; }
    .exp-section .info-line:first-child strong, .project-section .info-line:first-child strong { display: none; }
    
    /* Ưu tiên 2. Năm (Xóa bullet, Tự thêm nhãn "Thời gian : ") */
    .exp-section .exp-year, .project-section .exp-year { order: 2; font-size: 14px; color: #333; margin-bottom: 3px; }
    .exp-section .exp-year::first-letter, .project-section .exp-year::first-letter { font-size: 0; color: transparent; }
    .exp-section .exp-year::before, .project-section .exp-year::before { content: "Thời gian : "; }
    
    /* Ưu tiên 3. Vị trí / Vai trò (Giữ nguyên thẻ b của chữ Vị trí) */
    .exp-section .info-line:nth-child(2), .project-section .info-line:nth-child(2) { order: 3; font-size: 14px; color: #333; margin-bottom: 6px; }
    .exp-section .info-line:nth-child(2) strong, .project-section .info-line:nth-child(2) strong { font-weight: normal; }
    
    /* Ưu tiên 4. Mô tả text (List / Line) */
    .exp-section .desc-text, .project-section .desc-text { order: 4; font-size: 14px; color: #333; line-height: 1.6; text-align: justify; }
    .desc-text ul { padding-left: 20px; margin: 0; }
    .desc-text li { margin-bottom: 3px; }
    /* HOẠT ĐỘNG (Xử lý class header riêng) */
    .act-section .exp-item { display: flex; flex-direction: column; margin-bottom: 15px; }
    .act-section .exp-header { display: contents; }
    .act-section .company-name { order: 1; font-size: 14.5px; margin-bottom: 3px; color: #111; font-weight: 500; }
    .act-section .date-badge { order: 2; font-size: 14px; margin-bottom: 3px; color: #333; }
    .act-section .date-badge::before { content: "Thời gian : "; }  /* Fake Thời gian theo form */
    .act-section .exp-desc { order: 3; font-size: 14px; line-height: 1.6; margin-top: 2px; color: #333; }
`}</style>

        </div>
    );
};
export default TranHoaiThu;
