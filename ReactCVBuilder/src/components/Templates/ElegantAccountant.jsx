import React from 'react';

const ElegantAccountant = ({ resumeData }) => {
    return (
        <div className="cv-template-ElegantAccountant">
            <div className="accountant-cv">

	<div className="decor-top-right"></div>

	<div className="header-section">

	<div className="header-left">
	<h1 className="name-display">{resumeData?.fullName || 'HỌ TÊN'}</h1>
	<div className="personal-details">
	<p><strong>Ngày sinh:</strong> {resumeData?.birthDate || 'Ngày sinh'}</p>
	<p><strong>Địa chỉ:</strong> {resumeData?.address || 'Địa chỉ'}</p>
	<p><strong>Email:</strong> {resumeData?.email || 'Email'}</p>
	<p><strong>Số điện thoại:</strong> {resumeData?.phone || 'SĐT'}</p>
	</div>
	</div>

	<div className="header-center">
	<div className="avatar-container">{resumeData?.avatarUrl && <img src={resumeData.avatarUrl} className="avatar-img" />}</div>
	</div>

	<div className="header-right">
	<h2 className="job-display">{resumeData?.jobTitle || 'VỊ TRÍ'}</h2>
	<div className="job-line"></div>
	</div>

	</div>

	<div className="body-section">

	<div className="cv-block">
	<h3 className="block-title">KINH NGHIỆM LÀM VIỆC</h3>
	<div className="experience-grid">{resumeData?.experiences?.map((item, idx) => (

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

	<div className="cv-block">
	<h3 className="block-title">TRÌNH ĐỘ HỌC VẤN</h3>
	<div className="education-list">{resumeData?.educations?.map((item, idx) => (

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

	<div className="cv-block">
	<h3 className="block-title">KỸ NĂNG</h3>
	<div className="skills-flex"><ul className="skill-list-items">{resumeData?.skills?.map((item, idx) => (
<li>• {item.name}: {item.level}</li>
))}</ul></div>
	</div>

	</div>

	<div className="decor-bottom-right"></div>

	</div>

    
<style>{`
/* Cấu trúc Layout */
    .accountant-cv { background: white; height: 297mm; padding: 50px; font-family: "Segoe UI", sans-serif; color: #333; position: relative; overflow: hidden; }
    
    /* Header (Neil Tran Style) */
    .header-section { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 25px; }
    .header-left { flex: 1.5; border-top: 1px solid #ddd; padding-top: 15px; }
    .header-center { flex: 1; display: flex; justify-content: center; }
    .header-right { flex: 1; text-align: right; border-top: 1px solid #333; padding-top: 15px; }

    .name-display { font-size: 55px; color: #556b8d; margin: 0; font-family: Georgia, serif; line-height: 1; font-weight: normal; }
    .job-display { font-size: 28px; color: #333; line-height: 1.1; margin: 0; }
    
    .personal-details { font-size: 14.5px; margin-top: 20px; line-height: 1.8; }
    .avatar-img { width: 180px; height: 180px; border-radius: 50%; object-fit: cover; background: #eee; }

    /* Nội dung các mục (Tăng cỡ chữ) */
    .block-title { font-size: 24px; font-weight: bold; margin: 35px 0 15px 0; color: #222; }
    
    /* CHIA 2 CỘT KINH NGHIỆM - TRIỆT TIÊU LỖI ĐÈ NHAU */
    .experience-grid { 
        display: grid; 
        grid-template-columns: 1fr 1fr; /* Chia đều 50% cho mỗi bên */
        column-gap: 50px; 
        row-gap: 25px; 
        align-items: flex-start;
    }
    
    /* Xử lý khi người dùng nhập chuỗi liên tục (như aaaaaa...) */
    .exp-item { 
        font-size: 15px; 
        word-wrap: break-word; 
        overflow-wrap: anywhere; 
        white-space: pre-line;
    }
    
    .exp-year { font-weight: bold; font-size: 17px; margin-bottom: 5px; display: block; }
    .exp-content { line-height: 1.6; text-align: justify; }

    /* Học vấn và Kỹ năng */
    .education-list { font-size: 15.5px; line-height: 1.7; }
    .skills-flex { display: grid; grid-template-columns: 1fr 1fr; gap: 20px 60px; }
    .skill-label { font-size: 15.5px; font-style: italic; margin-bottom: 8px; display: block; }
    .progress-bg { height: 10px; background: #e0e6ed; border-radius: 5px; }
    .progress-fill { height: 100%; background: #556b8d; border-radius: 5px; }

    /* Họa tiết trang trí (Scribbles) */
    .decor-top-right { position: absolute; top: 10px; right: 20px; width: 100px; height: 100px; background: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='100' height='100'%3E%3Cpath d='M10 10 Q 50 10 90 90' fill='none' stroke='black' stroke-width='1'/%3E%3C/svg%3E") no-repeat; opacity: 0.5; }
`}</style>


        </div>
    );
};
export default ElegantAccountant;
