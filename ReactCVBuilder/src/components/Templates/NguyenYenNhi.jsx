import React from 'react';

const NguyenYenNhi = ({ resumeData }) => {
    return (
        <div className="cv-template-NguyenYenNhi">
            {<div className="pink-cv-container">
        <div className="deco-star star-1">✦</div>
        <div className="deco-star star-2">✦</div>

        <div className="cv-header">
            <div className="header-info">
                <h1 className="fullname">{resumeData?.fullName || 'HỌ TÊN'}</h1>
                <p className="job-title">{resumeData?.jobTitle || 'VỊ TRÍ'}</p>
            </div>
            <div className="header-photo">
                <div className="photo-bg-circle"></div>
                {resumeData?.avatarUrl && <img src={resumeData.avatarUrl} className="avatar-img" />}
            </div>
        </div>

        <div className="cv-body">
            <div className="col-left">
                <div className="section">
                    <h1 className="section-title">MỤC TIÊU NGHỀ NGHIỆP</h1>
                    <div className="content-text">{resumeData?.summary || 'Mục tiêu nghề nghiệp'}</div>
                </div>

                <div className="section">
                    <h1 className="section-title">HỌC VẤN</h1>
                    <div className="timeline">
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

                <div className="section">
                    <h1 className="section-title">KỸ NĂNG & CHUYÊN MÔN</h1>
                    
                    <div className="skill-group mt-3">
                        <p className="skill-sub-label">💻 TIN HỌC</p>
                        <div className="skill-text-list">
                            <ul className="skill-list-items">{resumeData?.skills?.map((item, idx) => (
<li>• {item.name}: {item.level}</li>
))}</ul>
                        </div>
                    </div>

                    <div className="skill-group mt-3">
                        <p className="skill-sub-label">🌍 NGOẠI NGỮ</p>
                        <div className="skill-text-list">
                            <ul className="lang-list-items">{resumeData?.languages?.map((item, idx) => (
<li style={{display:'flex', justifyContent:'space-between'}}><span>• {item.name}</span><span style={{fontStyle:'italic', opacity:0.8}}>{item.level}</span></li>
))}</ul>
                        </div>
                    </div>

                    <div className="skill-group mt-3">
                        <p className="skill-sub-label">🎨 KỸ NĂNG KHÁC</p>
                        <div className="skill-text-list">
                            <ul className="other-skill-list-items">{resumeData?.otherSkills?.map((item, idx) => (
<li>• {item.name} {item.level ? `(${item.level})` : ''}</li>
))}</ul>
                        </div>
                    </div>
                </div>
            </div>

            <div className="col-right">
                <div className="section">
                    <h1 className="section-title">LIÊN HỆ</h1>
                    <div className="contact-list">
                        <div className="contact-item"><span>📞</span> {resumeData?.phone || 'SĐT'}</div>
                        <div className="contact-item"><span>✉️</span> {resumeData?.email || 'Email'}</div>
                        <div className="contact-item"><span>📍</span> {resumeData?.address || 'Địa chỉ'}</div>
                    </div>
                </div>

                <div className="section">
                    <h1 className="section-title">KINH NGHIỆM LÀM VIỆC</h1>
                    <div className="timeline">
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
                    <h1 className="section-title">GIẢI THƯỞNG</h1>
                    <div className="content-text"><ul style={{paddingLeft:'15px', margin:0}}>{resumeData?.awards?.map((item, idx) => (
<li>{item.name}</li>
))}</ul></div>
                </div>
            </div>
        </div>
        
        <div className="cv-footer">
            <div className="deco-star star-footer">✦</div>
        </div>
    </div>}
    
<style>{`
/* Layout & Colors */
    .pink-cv-container {
        width: 210mm;
        min-height: 297mm;
        padding: 60px;
        background: #fff;
        position: relative;
        font-family: "Segoe UI", Tahoma, Geneva, Verdana, sans-serif;
        color: #333;
        box-sizing: border-box;
    }

    /* Header & Circle Avatar */
    .cv-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 40px; }
    .fullname { font-size: 52px; font-family: "Georgia", serif; font-weight: bold; margin: 0; color: #111; line-height: 1.1; }
    .job-title { font-size: 15px; text-transform: uppercase; letter-spacing: 4px; margin-top: 10px; color: #555; font-weight: 600; }
    
    .header-photo { position: relative; width: 200px; height: 200px; }
    .photo-bg-circle { 
        position: absolute; top: 0; right: -10px; 
        width: 200px; height: 200px; 
        background: radial-gradient(circle, #fcdde1 0%, #f497a9 100%); 
        border-radius: 50%; 
    }
    .avatar-img { 
        position: absolute; width: 180px; height: 180px; 
        border-radius: 50%; object-fit: cover; 
        top: 10px; right: 0; z-index: 2; 
    }

    /* Column System */
    .cv-body { display: flex; gap: 50px; }
    .col-left { flex: 1.1; }
    .col-right { flex: 0.9; }

    h1.section-title, 
    h1.main-title, 
    h1.side-title {
       font-size: 18px; /* Giữ kích thước vừa phải, không được to như tên */
       font-weight: 800;
       margin-bottom: 15px;
       margin-top: 25px;
       display: block; /* Đảm bảo nó luôn nằm riêng 1 dòng */
       /* Giữ nguyên các màu sắc/border cũ của Khoa */
   }
    .skill-sub-label { font-size: 12px; font-weight: bold; color: #f497a9; margin-bottom: 8px; text-transform: uppercase; letter-spacing: 1px; }

    /* Content Lists */
    .content-text, .skill-text-list, .contact-list { font-size: 13px; line-height: 1.6; }
    .skill-text-list { white-space: pre-line; margin-bottom: 15px; padding-left: 5px; border-left: 2px solid #fcdde1; }

    /* Timeline Styling */
    .timeline { border-left: 1px dashed #f497a9; padding-left: 20px; margin-left: 5px; }
    .timeline-item { position: relative; margin-bottom: 20px; font-size: 13px; }
    .timeline-item::before { content: "✦"; position: absolute; left: -28px; color: #f497a9; font-size: 14px; background: #fff; }

    /* Skill Bars */
    .skill-item { margin-bottom: 10px; }
    .skill-bar-bg { width: 100%; height: 5px; background: #f0f0f0; border-radius: 10px; margin-top: 4px; }
    .skill-bar-fill { height: 100%; background: #f497a9; border-radius: 10px; }

    /* Decoration Sparkles */
    .deco-star { position: absolute; color: #f497a9; opacity: 0.5; }
    .star-1 { top: 30px; left: 45%; font-size: 25px; }
    .star-2 { top: 120px; right: 40px; font-size: 18px; }
    .star-footer { bottom: 50px; left: 40%; font-size: 20px; }

    .cv-footer { position: absolute; bottom: 40px; right: 60px; font-size: 11px; color: #bbb; }
`}</style>

        </div>
    );
};
export default NguyenYenNhi;
