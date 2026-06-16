import React from 'react';

const ModernBlueSidebar = ({ resumeData }) => {
    return (
        <div className="cv-template-ModernBlueSidebar">
            <div className="cv-container"><div className="cv-sidebar">

	<div className="profile-header">{resumeData?.avatarUrl && <img src={resumeData.avatarUrl} className="profile-pic" />}</div>

	<ul className="contact-info">
	<li><i className="fas fa-phone"></i> {resumeData?.phone || 'SĐT'}</li>
	<li><i className="fas fa-envelope"></i> {resumeData?.email || 'Email'}</li>
	<li><i className="fas fa-calendar-alt"></i> {resumeData?.birthDate || 'Ngày sinh'}</li>
	<li><i className="fas fa-map-marker-alt"></i> {resumeData?.address || 'Địa chỉ'}</li>
	</ul>

	<div className="sidebar-section"><h3>Học vấn</h3><div className="edu-item">{resumeData?.educations?.map((item, idx) => (

        <div className="exp-item" key={idx}>
            <span className="exp-year">• {item.year}</span>
            <div className="exp-content">
                <strong>{item.school}</strong><br/>
                {item.major}<br/>
                {item.gradType ? `Xếp loại: ${item.gradType}` : ''}
            </div>
        </div>
))}</div></div>

	<div className="sidebar-section"><h3>Tin học</h3><div><ul className="skill-list-items">{resumeData?.skills?.map((item, idx) => (
<li>• {item.name}: {item.level}</li>
))}</ul></div></div>

	<div className="sidebar-section"><h3>Ngoại ngữ</h3><div><ul className="lang-list-items">{resumeData?.languages?.map((item, idx) => (
<li style={{display:'flex', justifyContent:'space-between'}}><span>• {item.name}</span><span style={{fontStyle:'italic', opacity:0.8}}>{item.level}</span></li>
))}</ul></div></div>

        <div className="sidebar-section">
            <h3>Chứng chỉ</h3>
            <div className="side-content">
                {resumeData?.certifications?.map((item, idx) => (
                    <div key={idx} style={{ marginBottom: '6px', fontSize: '12.5px' }}>
                        <strong>• {item.name}</strong> {item.year ? `(${item.year})` : ''}
                    </div>
                ))}
            </div>
        </div>

        <div className="sidebar-section">
            <h3>Sở thích</h3>
            <div className="side-content">
                <ul style={{ paddingLeft: '15px', margin: 0, listStyleType: 'none' }}>
                    {resumeData?.hobbies?.map((item, idx) => (
                        <li key={idx} style={{ fontSize: '12.5px' }}>• {item.name}</li>
                    ))}
                </ul>
            </div>
        </div>

	<div className="sidebar-section"><h3>Người tham chiếu</h3><div className="ref-content">{resumeData?.references?.map((item, idx) => (
<p style={{marginBottom:'5px', fontSize:'12px'}}>• {item.info}</p>
))}</div></div>

	</div>

	<div className="cv-main">

	<div className="main-header">
	<h1>{resumeData?.fullName || 'HỌ TÊN'}</h1>
	<h2>{resumeData?.jobTitle || 'VỊ TRÍ'}</h2>
	</div>

	<div className="main-section"><h3>Mục tiêu nghề nghiệp</h3><p>{resumeData?.summary || 'Mục tiêu nghề nghiệp'}</p></div>

	<div className="main-section"><h3>Kinh nghiệm làm việc</h3><div className="exp-container">{resumeData?.experiences?.map((item, idx) => (

        <div className="exp-item" key={idx}>
            <span className="exp-year">• {item.time}</span>
            <div className="exp-content">
                <div className="info-line"><strong>Công ty:</strong> {item.company}</div>
                <div className="info-line"><strong>Vị trí:</strong> {item.role}</div>
                <div className="desc-text" dangerouslySetInnerHTML={{__html: item.desc}}></div>
            </div>
        </div>
))}</div></div>

        <div className="main-section">
            <h3>Dự án nổi bật</h3>
            <div className="exp-container">
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
            <h3>Hoạt động</h3>
            <div className="exp-container">
                {resumeData?.activities?.map((item, idx) => (
                    <div className="exp-item" key={idx}>
                        <span className="exp-year">• {item.time}</span>
                        <div className="exp-content">
                            <div className="info-line"><strong>Tổ chức/Sự kiện:</strong> {item.name}</div>
                            <div className="desc-text" dangerouslySetInnerHTML={{__html: item.desc}}></div>
                        </div>
                    </div>
                ))}
            </div>
        </div>

	<div className="main-section"><h3>Giải thưởng</h3><div className="award-container"><ul style={{paddingLeft:'15px', margin:0}}>{resumeData?.awards?.map((item, idx) => (
<li>{item.name}</li>
))}</ul></div></div>

	<div className="main-section"><h3>Kỹ năng khác</h3><div className="other-skills"><ul className="other-skill-list-items">{resumeData?.otherSkills?.map((item, idx) => (
<li>• {item.name} {item.level ? `(${item.level})` : ''}</li>
))}</ul></div></div>

	</div></div>

    
<style>{`
/* Reset và cấu trúc chung */
    .cv-container { display: flex; background: white; height: 297mm; font-family: "Segoe UI", sans-serif; line-height: 1.3; } 
    .cv-sidebar { flex: 3.2; background: #f7f9fc; padding: 15px; border-right: 1px solid #eee; } 
    .cv-main { flex: 6.8; padding: 15px 35px; overflow: hidden; }

    /* Ảnh đại diện */
    .profile-pic { width: 120px; height: 120px; border-radius: 50%; object-fit: cover; border: 3px solid white; box-shadow: 0 4px 10px rgba(0,0,0,0.1); margin-bottom: 8px; display: block; margin-left: auto; margin-right: auto; }

    /* Header: Tên cực to và sát vị trí */
    .main-header h1 { margin: 0; font-size: 42px; text-transform: uppercase; color: #333; line-height: 1; font-weight: 800; }
    .main-header h2 { margin: 0 0 8px 0; font-size: 18px; color: #666; font-weight: normal; text-transform: uppercase; letter-spacing: 1px; }

    /* Tiêu đề các mục: margin-top tạo khoảng cách "1 dòng", margin-bottom sát nội dung */
    .sidebar-section h3, .main-section h3 { 
        font-size: 14px; border-bottom: 1px solid #0d6efd; color: #0d6efd; 
        padding-bottom: 1px; 
        margin: 10px 0 2px 0 !important; /* 10px trên tạo khoảng cách, 2px dưới sát chữ */
        text-transform: uppercase; font-weight: bold; 
    }

    /* Nội dung: Triệt tiêu hoàn toàn margin mặc định */
    .main-section p, .exp-container, .award-container, .other-skills,
    .sidebar-section p, .sidebar-section ul, .sidebar-section li, .ref-content { 
        font-size: 12.5px; color: #333; 
        margin: 0 !important; /* Ép sát vào tiêu đề bên trên */
        padding: 0;
        word-wrap: break-word; overflow-wrap: break-word; 
        white-space: pre-line; text-align: justify;
    }

    /* Tối ưu Sidebar */
    .contact-info { list-style: none; padding: 0; font-size: 11.5px; margin-bottom: 10px; }
    .contact-info li { margin-bottom: 3px; display: flex; align-items: center; }
    .contact-info i { width: 18px; color: #0d6efd; margin-right: 6px; text-align: center; }
    .grad-type { color: #0d6efd; font-weight: 500; font-size: 11px; margin-top: 1px !important; }
`}</style>

        </div>
    );
};
export default ModernBlueSidebar;
