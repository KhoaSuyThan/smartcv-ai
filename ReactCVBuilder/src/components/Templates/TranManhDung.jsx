import React from 'react';

const TranManhDung = ({ resumeData }) => {
    return (
        <div className="cv-template-TranManhDung">
            <div className="cv-template-6">
    <div className="cv-left">
        <div className="avatar-box">
            {resumeData?.avatarUrl && <img src={resumeData.avatarUrl} alt="Avatar" />}
        </div>
        <div className="profile-header">
            <h1 className="fullname">{resumeData?.fullName || 'HỌ TÊN'}</h1>
            <h2 className="jobtitle">{resumeData?.jobTitle || 'VỊ TRÍ'}</h2>
        </div>
        
        <div className="left-divider"></div>
        
        <div className="contact-info">
            <div className="contact-item"><i className="fas fa-phone-alt"></i><span>{resumeData?.phone || 'SĐT'}</span></div>
            <div className="contact-item"><i className="fas fa-calendar-alt"></i><span>{resumeData?.birthDate || 'Ngày sinh'}</span></div>
            <div className="contact-item"><i className="fas fa-envelope"></i><span>{resumeData?.email || 'Email'}</span></div>
            <div className="contact-item"><i className="fas fa-map-marker-alt"></i><span>{resumeData?.address || 'Địa chỉ'}</span></div>
        </div>

        <div className="left-section">
            <h3 className="left-title"><span>Học vấn</span></h3>
            <div className="left-content edu-list">
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

        <div className="left-section">
            <h3 className="left-title"><span>Kỹ năng</span></h3>
            <div className="left-content skill-list">
                <ul className="other-skill-list-items">{resumeData?.otherSkills?.map((item, idx) => (
<li>• {item.name} {item.level ? `(${item.level})` : ''}</li>
))}</ul>
            </div>
        </div>

        <div className="left-section">
            <h3 className="left-title"><span>Tin học</span></h3>
            <div className="left-content skill-list">
                <ul className="skill-list-items">{resumeData?.skills?.map((item, idx) => (
<li>• {item.name}: {item.level}</li>
))}</ul>
            </div>
        </div>

        <div className="left-section">
            <h3 className="left-title"><span>Ngoại ngữ</span></h3>
            <div className="left-content skill-list">
                <ul className="lang-list-items">{resumeData?.languages?.map((item, idx) => (
<li style={{display:'flex', justifyContent:'space-between'}}><span>• {item.name}</span><span style={{fontStyle:'italic', opacity:0.8}}>{item.level}</span></li>
))}</ul>
            </div>
        </div>

        <div className="left-section">
            <h3 className="left-title"><span>Sở thích</span></h3>
            <div className="left-content skill-list">
                <ul className="hobby-list-items">{resumeData?.hobbies?.map((item, idx) => (
<li>• {item.name}</li>
))}</ul>
            </div>
        </div>
    </div>

    <div className="cv-right">
        <div className="right-section">
            <h3 className="right-title"><span>Mục tiêu nghề nghiệp</span></h3>
            <div className="right-content summary-text">
                {resumeData?.summary || 'Mục tiêu nghề nghiệp'}
            </div>
        </div>

        <div className="right-section">
            <h3 className="right-title"><span>Kinh nghiệm làm việc</span></h3>
            <div className="right-content experience-list">
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

        <div className="right-section">
            <h3 className="right-title"><span>Hoạt động</span></h3>
            <div className="right-content activity-list">
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

        <div className="right-section">
            <h3 className="right-title"><span>Danh hiệu và giải thưởng</span></h3>
            <div className="right-content award-list">
                <ul style={{paddingLeft:'15px', margin:0}}>{resumeData?.awards?.map((item, idx) => (
<li>{item.name}</li>
))}</ul>
            </div>
        </div>

        <div className="right-section">
            <h3 className="right-title"><span>Chứng chỉ</span></h3>
            <div className="right-content cert-list">
                {resumeData?.certifications?.map((item, idx) => (

        <div style={{marginBottom:'8px'}}>
            <div className="cert-year-div" style={{fontWeight:'bold', fontSize:'11px', color:'#634c46'}}>{item.year}</div>
            <div className="cert-name-div" style={{fontSize:'12.5px'}}>{item.name}</div>
        </div>
))}
            </div>
        </div>
        
        <div className="right-section">
            <h3 className="right-title"><span>Người tham chiếu</span></h3>
            <div className="right-content reference-list">
                {resumeData?.references?.map((item, idx) => (
<p style={{marginBottom:'5px', fontSize:'12px'}}>• {item.info}</p>
))}
            </div>
        </div>
    </div>
</div>



<style>{`
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');

:root {
    --primary-color: #574040;
    --text-main: #333;
    --text-light: #555;
    --bg-left: var(--primary-color);
    --bg-right: #ffffff;
}

/* KHUNG A4 CỐ ĐỊNH */
.cv-template-6 {
    display: flex;
    width: 210mm;
    max-width: 210mm;
    min-height: 297mm;
    background-color: var(--bg-right);
    font-family: 'Inter', sans-serif;
    color: var(--text-main);
    line-height: 1.4;
    box-sizing: border-box;
    margin: 0 auto;
    overflow: hidden; 
}

/* CHỐNG TRÀN CHỮ TUYỆT ĐỐI (FIX LỖI AAAAAA) */
.cv-template-6 * {
    box-sizing: border-box;
    word-wrap: break-word;
    overflow-wrap: anywhere; 
    word-break: break-word;
}

/* SIDEBAR TRÁI - GIỮ ĐỘ RỘNG 38% THEO Ý KHOA */
.cv-template-6 .cv-left {
    width: 38%; 
    background-color: var(--bg-left);
    color: #fff;
    padding: 35px 25px;
    display: flex;
    flex-direction: column;
    min-width: 38%; 
}

.cv-template-6 .avatar-box {
    text-align: center;
    margin-bottom: 20px;
}

.cv-template-6 .avatar-box img {
    width: 160px;
    height: 160px;
    border-radius: 50%;
    object-fit: cover;
    border: 4px solid rgba(255,255,255,0.1);
}

.cv-template-6 .fullname {
    font-size: 24px;
    font-weight: 700;
    margin: 0 0 5px 0;
    text-align: center;
    line-height: 1.2;
}

.cv-template-6 .jobtitle {
    font-size: 15px;
    text-align: center;
    margin-bottom: 25px;
    color: rgba(255, 255, 255, 0.9);
}

/* THÔNG TIN LIÊN HỆ - FIX LỖI DÍNH ICON */
.cv-template-6 .contact-info {
    margin-bottom: 25px;
}

.cv-template-6 .contact-item {
    display: flex;
    margin-bottom: 12px; /* Tăng khoảng cách giữa các dòng cho thoáng */
    font-size: 13.5px;
    align-items: center; /* Căn giữa icon và text theo chiều dọc */
    gap: 12px; /* TẠO KHOẢNG CÁCH GIỮA ICON VÀ TEXT */
}

.cv-template-6 .contact-item i {
    width: 20px; /* Khóa độ rộng icon để text luôn thẳng hàng dọc */
    text-align: center;
    font-size: 16px;
    flex-shrink: 0; /* Không cho icon bị bóp méo khi text dài */
    color: rgba(255, 255, 255, 0.8);
}

/* CÁC PHẦN BÊN TRÁI */
.cv-template-6 .left-section {
    margin-bottom: 15px; 
}

.cv-template-6 .left-title {
    margin-bottom: 10px !important;
}

.cv-template-6 .left-title span {
    display: inline-block;
    background-color: rgba(255, 255, 255, 0.15);
    padding: 6px 18px;
    border-radius: 20px;
    font-size: 13px;
    font-weight: 600;
    text-transform: uppercase;
}

.cv-template-6 .left-content {
    font-size: 13px;
    line-height: 1.4;
}

.cv-template-6 .left-content li {
    margin-bottom: 5px !important;
}

/* CỘT PHẢI */
.cv-template-6 .cv-right {
    width: 62%;
    background-color: var(--bg-right);
    padding: 40px 30px;
    min-width: 62%;
}

.cv-template-6 .right-section {
    margin-bottom: 20px; 
}

.cv-template-6 .right-title {
    display: flex;
    align-items: center;
    margin-bottom: 12px;
}

.cv-template-6 .right-title span {
    background-color: var(--primary-color);
    color: #fff;
    padding: 7px 20px;
    border-radius: 20px;
    font-size: 14.5px;
    font-weight: 600;
    text-transform: uppercase;
}

.cv-template-6 .right-title::after {
    content: "";
    flex-grow: 1;
    height: 1px;
    background-color: var(--primary-color);
    margin-left: 12px;
    opacity: 0.2;
}

/* TRIỆT TIÊU KHOẢNG TRỐNG THỪA TỪ AI */
.cv-template-6 p, .cv-template-6 ul, .cv-template-6 li {
    margin-top: 0 !important;
    margin-bottom: 3px !important; 
}

/* KHI IN PDF */
@media print {
    @page { size: A4; margin: 0; }
    body { margin: 0; padding: 0; }
    .cv-template-6 {
        width: 210mm;
        height: 297mm;
        margin: 0;
        box-shadow: none;
    }
}
`}</style>

        </div>
    );
};
export default TranManhDung;
