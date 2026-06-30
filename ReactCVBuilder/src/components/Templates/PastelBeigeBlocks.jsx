import React from 'react';

const PastelBeigeBlocks = ({ resumeData }) => {
    return (
        <div className="cv-template-PastelBeigeBlocks">
            <div className="cv-pastel-wrapper">
        {/* Block 1: Contact */}
        {(resumeData?.phone || resumeData?.email || resumeData?.website || resumeData?.address) && (
            <div className="pastel-block contact-block">
                {resumeData?.phone && <div className="contact-item"><i className="fas fa-phone-alt"></i> <span><span dangerouslySetInnerHTML={{ __html: resumeData?.phone }}></span></span></div>}
                {resumeData?.email && <div className="contact-item"><i className="fas fa-envelope"></i> <span><span dangerouslySetInnerHTML={{ __html: resumeData?.email }}></span></span></div>}
                {resumeData?.website && <div className="contact-item"><i className="fas fa-globe"></i> <span><span dangerouslySetInnerHTML={{ __html: resumeData?.website }}></span></span></div>}
                {resumeData?.address && <div className="contact-item"><i className="fas fa-map-marker-alt"></i> <span><span dangerouslySetInnerHTML={{ __html: resumeData?.address }}></span></span></div>}
            </div>
        )}

        {/* Block 2: Profile */}
        <div className="pastel-block profile-block">
            <div className="profile-left">
                <h1 className="fullname"><span dangerouslySetInnerHTML={{ __html: resumeData?.fullName || 'HỌ TÊN' }}></span></h1>
                <div className="job-title-wrapper">
                    <span className="job-title"><span dangerouslySetInnerHTML={{ __html: resumeData?.jobTitle || 'VỊ TRÍ' }}></span></span>
                    <span className="title-line"></span>
                </div>
                <div className="summary-text"><span dangerouslySetInnerHTML={{ __html: resumeData?.summary || 'Mục tiêu nghề nghiệp' }}></span></div>
            </div>
            <div className="profile-right">
                <img src={resumeData?.avatarUrl || "/images/default-avatar.png"} className="avatar-img" />
            </div>
        </div>

        {/* Block 3: Education & Certs */}
        <div className="pastel-block education-section">
            <div className="section-group education-group">
                <h3 className="section-title">Học vấn</h3>
                <div className="title-line-full"></div>
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

        </div>
        <div className="pastel-block cert-section section">
            <div className="section-group cert-group">
                <h3 className="section-title">Chứng chỉ</h3>
                <div className="title-line-full"></div>
                <div className="content-area">{resumeData?.certifications?.map((item, idx) => (

        <div style={{marginBottom:'8px'}}>
            <div className="cert-year-div" style={{fontWeight:'bold', fontSize:'11px', color:'#634c46'}}>{item.year}</div>
            <div className="cert-name-div" style={{fontSize:'12.5px'}}>{item.name}</div>
        </div>
))}</div>
            </div>
        </div>

        {/* Block 4: Projects & Experience */}
        <div className="pastel-block project-section project-group">
            <h3 className="section-title">Dự án</h3>
            <div className="title-line-full"></div>
            <div className="timeline-area">{resumeData?.projects?.map((item, idx) => (

        <div className="exp-item" key={idx}>
            <span className="exp-year">• {item.time}</span>
            <div className="exp-content">
                <div className="info-line"><strong>Dự án:</strong> {item.name}</div>
                <div className="info-line"><strong>Vai trò:</strong> {item.role}</div>
                <div className="desc-text" dangerouslySetInnerHTML={{__html: item.desc}}></div>
            </div>
        </div>
))}</div>
            
            {/* Tận dụng không gian cho Kinh nghiệm làm việc dùng chung form timeline */}
        </div>
        <div className="pastel-block exp-section">
            <h3 className="section-title">Kinh nghiệm làm việc</h3>
            <div className="title-line-full"></div>
            <div className="timeline-area">{resumeData?.experiences?.map((item, idx) => (

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

        {/* Block 5: Activities */}
        <div className="pastel-block act-section act-group">
            <h3 className="section-title">Hoạt động</h3>
            <div className="title-line-full"></div>
            {/* Class riêng act-area do HTML sinh ra từ C# cho phần này khác với Projects */}
            <div className="act-area">{resumeData?.activities?.map((item, idx) => (

        <div className="exp-item" key={idx}>
            <div className="exp-header">
                <span className="company-name">{item.name}</span>
                <span className="date-badge">{item.time}</span>
            </div>
            <div className="exp-desc" dangerouslySetInnerHTML={{__html: item.desc}}></div>
        </div>
))}</div>
        </div>

        {/* Block 6: Skills */}
        <div className="pastel-block skills-section section skill-group">
            <h3 className="section-title">Kỹ năng</h3>
            <div className="title-line-full"></div>
            <div className="content-area">
                <ul className="skill-list-items">
                    {resumeData?.skills?.map((item, idx) => (
                        <li key={idx} style={{ listStyle: 'none', marginBottom: '8px' }}>
                            <strong style={{ display: 'block', fontSize: '13px', color: '#111', marginBottom: '2px' }}>{item.name}</strong>
                            <span style={{ display: 'block', fontSize: '12px', color: '#333', lineHeight: '1.4' }}>{item.level}</span>
                        </li>
))}</ul></div>
            <div className="content-area">
                <ul className="other-skill-list-items">
                    {resumeData?.otherSkills?.map((item, idx) => (
                        <li key={idx} style={{ listStyle: 'none', marginBottom: '8px' }}>
                            <strong style={{ display: 'block', fontSize: '13px', color: '#111', marginBottom: '2px' }}>{item.name}</strong>
                            <span style={{ display: 'block', fontSize: '12px', color: '#333', lineHeight: '1.4' }}>{item.level}</span>
                        </li>
))}</ul></div>
        </div>

        {/* Block 7: Bottom Split */}
        <div className="pastel-block bottom-split">
            <div className="bottom-left hobbies-group">
                <h3 className="section-title">Sở thích</h3>
                <div className="title-line-full"></div>
                <div className="content-area"><ul className="hobby-list-items">{resumeData?.hobbies?.map((item, idx) => (
<li>• {item.name}</li>
))}</ul></div>
            </div>
            <div className="bottom-right languages-section section">
                <div className="awards-group">
                    <h3 className="section-title">Ngoại ngữ</h3>
                    <div className="title-line-full"></div>
                    <div className="content-area">
                        <ul style={{ paddingLeft: '15px', margin: 0, listStyle: 'none' }}>
                            {resumeData?.languages?.map((item, idx) => (
                                <li key={idx} style={{ fontSize: '12.5px', color: '#222', padding: '4px 0' }}>
                                    • {item.name} {item.level ? `- ${item.level}` : ''}
                                </li>
                            ))}
                        </ul>
                    </div>



                </div>
            </div>
        </div>
    </div>

    
<style>{`
@import url("https://fonts.googleapis.com/css2?family=Segoe+UI:wght@400;600;700&display=swap");

    /* TỔNG THỂ */
    .cv-pastel-wrapper { 
        width: 100%; min-height: 297mm; background: #FFF; padding: 20px 30px; 
        font-family: "Segoe UI", Helvetica, Arial, sans-serif; box-sizing: border-box; overflow: hidden;
    }
    .cv-pastel-wrapper * { box-sizing: border-box; word-wrap: break-word; }

    /* KHỐI PASTEL */
    .pastel-block { background: #EFECE9; padding: 14px 22px; border-radius: 6px; margin-bottom: 12px; }

    /* KHỐI LIÊN HỆ DỌC THEO HÀNG */
    .contact-block { padding: 12px; display: flex; justify-content: space-around; flex-wrap: wrap; gap: 15px; margin-bottom: 12px; }
    .contact-item { display: flex; align-items: center; gap: 8px; font-size: 11px; font-weight: 600; color: #111; }
    .contact-item i { color: #D6624B; font-size: 13px; }

    /* HỒ SƠ CÁ NHÂN */
    .profile-block { display: flex; gap: 30px; align-items: center; }
    .profile-left { flex: 1; display: flex; flex-direction: column; }
    
    .fullname { font-size: 24px; font-weight: 700; color: #5C322E; margin: 0 0 8px 0; letter-spacing: 0.5px; }
    .job-title-wrapper { display: flex; align-items: center; gap: 12px; margin-bottom: 10px; }
    .job-title { font-size: 13.5px; font-weight: 700; text-transform: uppercase; color: #111; }
    .title-line { flex: 1; max-width: 120px; height: 2px; background: #D6624B; }
    
    .summary-text { font-size: 12px; line-height: 1.5; text-align: justify; color: #222; }

    /* HIỆU ỨNG ẢNH ĐAI CAM CẮT GÓC */
    .profile-right { width: 110px; flex-shrink: 0; position: relative; padding-top: 8px; padding-left: 8px; }
    .profile-right::before { content: ""; position: absolute; left: 0; top: 0; width: 60px; height: 60px; background: #D6624B; z-index: 1; border-radius: 2px; }
    .avatar-img { position: relative; z-index: 2; width: 110px; height: 130px; object-fit: cover; border-radius: 4px; box-shadow: -2px 2px 10px rgba(0,0,0,0.1); }

    /* TIÊU ĐỀ RED SECTION */
    .section-title { font-size: 14.5px; font-weight: 700; color: #5C322E; margin: 0 0 4px 0 !important; }
    .title-line-full { width: 100%; height: 1.5px; background: #D6624B; margin-bottom: 10px; }

    /* HỌC VẤN (Cấu trúc Flat Text) */
    .education-group { margin-bottom: 15px; }
    .education-group .exp-item { display: flex; flex-direction: column; margin-bottom: 8px; }
    .education-group .exp-content { order: 1; font-size: 12px; color: #111; line-height: 1.5; }
    .education-group .exp-content strong { font-weight: 700; font-size: 13px; display: block; margin-bottom: 2px; }
    .education-group .exp-year { order: 2; font-weight: 700; font-size: 11.5px; margin-top: 3px; color: #111; }
    .education-group .exp-year::first-letter { font-size: 0; color: transparent; }

    /* CHỨNG CHỈ */
    .cert-group > div > div { margin-bottom: 8px !important; }
    .cert-group .cert-year-div { font-size: 11.5px !important; font-weight: 700 !important; color: #333 !important; margin-bottom: 2px !important; }
    .cert-group .cert-name-div { font-size: 12px !important; font-weight: 400 !important; color: #111 !important; line-height: 1.3; }

    /* TIMELINE DỰ ÁN & KINH NGHIỆM LÀM VIỆC (Lướt Grid tạo thành 4 phân vùng) */
    .timeline-area .exp-item {
        display: grid; grid-template-columns: 35% 65%; gap: 0; row-gap: 3px;
        position: relative; padding-left: 20px; margin-bottom: 12px;
    }
    /* Thanh dọc */
    .timeline-area .exp-item::before { content: ""; position: absolute; left: 4px; top: 12px; width: 1.5px; height: calc(100% + 8px); background: #C5BDBA; }
    .timeline-area .exp-item:last-child::before { display: none; }
    /* Chấm cam */
    .timeline-area .exp-item::after { content: ""; position: absolute; left: 0px; top: 10px; width: 9px; height: 9px; border-radius: 50%; background: #D6624B; }
    
    .timeline-area .exp-content { display: contents; } /* Gỡ bỏ bọc hộp */

    /* Định vị Grid */
    .timeline-area .exp-year { grid-column: 1; grid-row: 1; font-size: 11.5px; font-weight: 700; color: #444; margin-top: 3px; }
    .timeline-area .exp-year::first-letter { font-size: 0; color: transparent; }

    .timeline-area .info-line:nth-child(2) { grid-column: 2; grid-row: 1; font-size: 12px; font-weight: 700; color: #111; margin-top: 3px; }
    .timeline-area .info-line:nth-child(2) strong { display: none; }

    .timeline-area .info-line:first-child { grid-column: 1; grid-row: 2; font-size: 12px; font-weight: 700; color: #000; padding-right: 15px; }
    .timeline-area .info-line:first-child strong { display: none; }

    .timeline-area .desc-text { grid-column: 2; grid-row: 2; font-size: 12px; line-height: 1.5; color: #222; text-align: justify; }
    .desc-text ul { padding-left: 20px; }

    /* TIMELINE HOẠT ĐỘNG (Activity Header structure) */
    .act-area .exp-item { display: grid; grid-template-columns: 35% 65%; gap: 0; row-gap: 3px; position: relative; padding-left: 20px; margin-bottom: 12px; }
    .act-area .exp-item::before { content: ""; position: absolute; left: 4px; top: 12px; width: 1.5px; height: calc(100% + 8px); background: #C5BDBA; }
    .act-area .exp-item:last-child::before { display: none; }
    .act-area .exp-item::after { content: ""; position: absolute; left: 0px; top: 10px; width: 9px; height: 9px; border-radius: 50%; background: #D6624B; }
    .act-area .exp-header { display: contents; }

    .act-area .date-badge { grid-column: 1; grid-row: 1; font-size: 11.5px; font-weight: 700; color: #444; margin-top: 3px; }
    .act-area .company-name { grid-column: 1; grid-row: 2; font-size: 12px; font-weight: 700; color: #000; padding-right: 15px; }
    .act-area .exp-desc { grid-column: 2; grid-row: 1 / span 2; font-size: 12px; line-height: 1.5; color: #222; text-align: justify; margin-top: 3px; }

    /* KỸ NĂNG */
    .skill-group ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
    .skill-group li { font-size: 12px; line-height: 1.5; margin-bottom: 6px; color: #111; }
    .skill-group li::first-letter { font-size: 0; color: transparent; }

    /* KHỐI BOTTOM SPLIT (Giải thưởng & Liên hệ & Sở thích) */
    .bottom-split { display: grid; grid-template-columns: 1fr 1fr; gap: 30px; }
    
    .awards-group ul { list-style: none !important; padding: 0 !important; margin: 0 0 8px 0 !important; }
    .awards-group li { padding: 3px 0; font-size: 12px; color: #222; }
    .awards-group li::first-letter { font-size: 0; color: transparent; }

    .ref-group p { font-size: 12px !important; margin-bottom: 6px !important; line-height: 1.5; color: #222; }
    .ref-group p::first-letter { font-size: 0; color: transparent; }

    .hobbies-group ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
    .hobbies-group li { font-size: 12px; color: #222; display: inline-block; margin-right: 15px !important; margin-bottom: 4px; }
    .hobbies-group li::first-letter { font-size: 0; color: transparent; }
`}</style>

        </div>
    );
};
export const defaultVisibility = {
  summary: true, experiences: false, educations: true, skills: true, otherSkills: false,
  projects: true, activities: true, certifications: true, awards: false,
  references: false, hobbies: true, languages: true
};
export default PastelBeigeBlocks;
