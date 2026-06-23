import React from 'react';

const NguyenMinhAn = ({ resumeData }) => {
    return (
        <div className="cv-template-NguyenMinhAn">
            <div className="academic-cv-wrapper">
        <div className="cv-main-col">
            <div className="header-area">
                <h1 className="fullname"><span dangerouslySetInnerHTML={{ __html: resumeData?.fullName || 'HỌ TÊN' }}></span></h1>
                <p className="job-title"><span dangerouslySetInnerHTML={{ __html: resumeData?.jobTitle || 'VỊ TRÍ' }}></span></p>
            </div>

            <div className="section">
                <h1 className="section-title">KINH NGHIỆM LÀM VIỆC</h1>
                <div className="section-content">
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

            <div className="section">
                <h1 className="section-title">HỌC VẤN</h1>
                <div className="section-content">
                    {resumeData?.educations?.map((item, idx) => (

        <div className="exp-item" key={idx}>
            <span className="exp-year">• {item.year}</span>
            <div className="exp-content">
                <strong>{item.school}</strong><br/>
                {item.major}<br/>
                {item.gradType ? `Xếp loại: ${item.gradType}` : ''}
            </div>
        </div>
))}
                </div>
            </div>
        </div>

        <div className="cv-side-col">
            <div className="avatar-box">
                <img src={resumeData?.avatarUrl || "/images/default-avatar.png"} className="avatar-img" />
            </div>

            <div className="side-section">
                <h1 className="side-title">MỤC TIÊU LÀM VIỆC</h1>
                <div className="side-content"><span dangerouslySetInnerHTML={{ __html: resumeData?.summary || 'Mục tiêu nghề nghiệp' }}></span></div>
            </div>

            <div className="side-section">
                <h1 className="side-title">GIẢI THƯỞNG</h1>
                <div className="side-content"><ul style={{paddingLeft:'15px', margin:0}}>{resumeData?.awards?.map((item, idx) => (
<li>{item.name}</li>
))}</ul></div>
            </div>

            {(resumeData?.phone || resumeData?.email || resumeData?.address) && (
                <div className="side-section">
                    <h1 className="side-title">THÔNG TIN LIÊN HỆ</h1>
                    <div className="contact-info">
                        {resumeData?.phone && <p>Di động: <span dangerouslySetInnerHTML={{ __html: resumeData?.phone }}></span></p>}
                        {resumeData?.email && <p>Email: <span dangerouslySetInnerHTML={{ __html: resumeData?.email }}></span></p>}
                        {resumeData?.address && <p>Địa chỉ: <span dangerouslySetInnerHTML={{ __html: resumeData?.address }}></span></p>}
                    </div>
                </div>
            )}

            <div className="side-section">
                <h1 className="side-title">KỸ NĂNG & CHUYÊN MÔN</h1>
                
                <div className="skill-group">
                    <div className="skill-list-main"><ul className="skill-list-items">{resumeData?.skills?.map((item, idx) => (
<li>• {item.name}: {item.level}</li>
))}</ul></div>
                </div>

                <div className="skill-group-extra">
                    <p className="extra-label">💻 TIN HỌC</p>
                    <div className="extra-content"><ul className="skill-list-items">{resumeData?.skills?.map((item, idx) => (
<li>• {item.name}: {item.level}</li>
))}</ul></div>
                </div>

                <div className="skill-group-extra">
                    <p className="extra-label">🌍 NGOẠI NGỮ</p>
                    <div className="extra-content"><ul className="lang-list-items">{resumeData?.languages?.map((item, idx) => (
<li style={{display:'flex', justifyContent:'space-between'}}><span>• {item.name}</span><span style={{fontStyle:'italic', opacity:0.8}}>{item.level}</span></li>
))}</ul></div>
                </div>

                <div className="skill-group-extra">
                    <p className="extra-label">🎨 KỸ NĂNG KHÁC</p>
                    <div className="extra-content"><ul className="other-skill-list-items">{resumeData?.otherSkills?.map((item, idx) => (
<li>• {item.name} {item.level ? `(${item.level})` : ''}</li>
))}</ul></div>
                </div>
            </div>
        </div>
    </div>

    
<style>{`
/* RESET CHUNG */
* { 
    box-sizing: border-box; 
    margin: 0; 
    padding: 0; 
}

/* KHUNG WRAPPER CHUẨN A4 */
.academic-cv-wrapper { 
    display: flex; 
    width: 210mm; 
    max-width: 210mm; /* Khóa cứng chiều rộng tối đa */
    min-height: 297mm; 
    background: #fdf5e6; 
    font-family: "Arial", sans-serif; 
    overflow: hidden; /* Cắt bỏ mọi thứ cố tình tràn ra ngoài */
    box-shadow: 0 0 10px rgba(0,0,0,0.1); /* Tạo bóng nhẹ khi xem trên web */
    margin: 0 auto;
}

/* CỘT CHÍNH (BÊN TRÁI) */
.cv-main-col { 
    flex: 6; 
    padding: 30px 40px !important; 
    color: #4a3728; 
    min-width: 0; /* ÉP FLEXBOX KHÔNG ĐƯỢC GIÃN THEO NỘI DUNG */
    background: #fdf5e6;
}

/* CỘT PHỤ (BÊN PHẢI) */
.cv-side-col { 
    flex: 4; 
    background: #5d4e46; 
    color: #fff; 
    padding: 30px 25px !important; 
    min-width: 0; /* ÉP FLEXBOX KHÔNG ĐƯỢC GIÃN THEO NỘI DUNG */
}

/* XỬ LÝ VĂN BẢN QUÁ DÀI (FIX LỖI TRÀN KHUNG) */
.section-content, 
.side-content, 
.contact-info, 
.extra-content,
.job-title {
    word-wrap: break-word; 
    overflow-wrap: break-word; 
    word-break: break-word; /* Ép xuống hàng ngay cả khi không có khoảng trắng */
    white-space: normal;
    line-height: 1.5;
}

/* TIÊU ĐỀ HỌ TÊN */
.fullname { 
    font-size: 40px; /* Giảm nhẹ để tránh tên quá dài bị nhảy dòng xấu */
    font-weight: 900; 
    color: #5d4e46; 
    margin: 0; 
    text-transform: uppercase; 
    word-break: break-word;
}

.job-title { 
    font-size: 18px; 
    color: #8b7355; 
    margin-top: 5px; 
}

/* AVATAR */
.avatar-box { text-align: center; margin-bottom: 20px; }
.avatar-img { 
    width: 150px; 
    height: 150px; 
    border-radius: 50%; 
    border: 5px solid rgba(255,255,255,0.1); 
    object-fit: cover; 
}

/* CÁC SECTION CỘT TRÁI */
h1.section-title { 
    font-size: 18px; 
    font-weight: 800; 
    color: #8b7355; 
    margin-bottom: 8px !important; 
    margin-top: 15px !important; 
    text-transform: uppercase; 
    border-bottom: 2px solid #8b7355; 
    padding-bottom: 3px;
}

.section-content { 
    font-size: 14px; 
    margin-bottom: 15px !important; 
}

/* TRIỆT TIÊU MARGIN DƯ THỪA TỪ AI RENDER */
.section-content p, .section-content ul, .section-content li,
.side-content p, .side-content ul, .side-content li {
    margin-top: 0 !important;
    margin-bottom: 4px !important; 
    padding-left: 0 !important;
}

/* CÁC SECTION CỘT PHẢI */
.side-section { margin-bottom: 15px !important; } 

h1.side-title { 
    font-size: 16px; 
    font-weight: 700; 
    color: #fdf5e6; 
    margin-bottom: 8px !important; 
    margin-top: 5px !important;
    text-transform: uppercase; 
    border-bottom: 1px solid rgba(253, 245, 230, 0.3); 
    padding-bottom: 3px;
}

.side-content, .contact-info { 
    font-size: 13px; 
    color: #e8e8e8; 
}

.contact-info p { margin-bottom: 3px !important; }

/* KỸ NĂNG & THÔNG TIN THÊM */
.skill-group-extra { margin-top: 8px; }
.extra-label { 
    font-size: 12px; 
    font-weight: bold; 
    color: #fdf5e6; 
    margin-bottom: 2px; 
    display: block;
}
.extra-content { font-size: 12px; color: #ddd; }

/* CẤU HÌNH KHI IN PDF (QUAN TRỌNG) */
@media print {
    body { background: none; }
    .academic-cv-wrapper { 
        box-shadow: none; 
        margin: 0;
        width: 210mm;
        height: 297mm;
    }
}
`}</style>

        </div>
    );
};
export const defaultVisibility = {
  summary: true, experiences: true, educations: true, skills: false, otherSkills: true,
  projects: false, activities: false, certifications: false, awards: true,
  references: false, hobbies: false, languages: false
};
export default NguyenMinhAn;
