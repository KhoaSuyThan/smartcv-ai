import React from 'react';

const DinhXuanThao = ({ resumeData }) => {
    const getSkillPercent = (level) => {
        if (!level) return '80%';
        if (level.includes('%')) return level;
        const lvl = level.toLowerCase();
        if (lvl.includes('xuất sắc') || lvl.includes('expert') || lvl.includes('master')) return '95%';
        if (lvl.includes('tốt') || lvl.includes('thành thạo') || lvl.includes('advanced') || lvl.includes('fluent')) return '85%';
        if (lvl.includes('khá') || lvl.includes('intermediate')) return '70%';
        if (lvl.includes('trung bình') || lvl.includes('cơ bản') || lvl.includes('basic')) return '50%';
        return '75%';
    };

    return (
        <div className="cv-template-DinhXuanThao">
            <div className="cv-thao-wrapper">
                <div className="cv-header">
                    <div className="header-content">
                        <div className="avatar-box">
                            <img src={resumeData?.avatarUrl || "/images/default-avatar.png"} className="avatar-img" />
                        </div>
                        <div className="title-box">
                            <h1 className="fullname">{resumeData?.fullName || 'HỌ TÊN'}</h1>
                            <h2 className="job-title">{resumeData?.jobTitle || 'VỊ TRÍ'}</h2>
                        </div>
                    </div>
                </div>
                
                <div className="cv-body">
                    <div className="cv-sidebar">
                        <div className="sidebar-section">
                            <h3 className="side-title">LIÊN HỆ</h3>
                            <div className="side-content contact-info">
                                <p><i className="fas fa-calendar-alt"></i> {resumeData?.birthDate || 'Ngày sinh'}</p>
                                <p><i className="fas fa-map-marker-alt"></i> {resumeData?.address || 'Địa chỉ'}</p>
                                <p><i className="fas fa-phone"></i> {resumeData?.phone || 'SĐT'}</p>
                                <p><i className="fas fa-envelope"></i> {resumeData?.email || 'Email'}</p>
                                <p><i className="fas fa-globe"></i> {resumeData?.website || 'Website'}</p>
                            </div>
                        </div>

                        <div className="sidebar-section">
                            <h3 className="side-title">KỸ NĂNG</h3>
                            <div className="side-content">
                                <div className="skills-bars-container">
                                    {resumeData?.skills?.map((item, idx) => (
                                        <div className="skill-progress-item" key={idx} style={{ marginBottom: '10px' }}>
                                            <div className="skill-name-label" style={{ fontSize: '11px', fontWeight: 'bold', textTransform: 'uppercase', marginBottom: '4px', color: '#444' }}>{item.name}</div>
                                            <div className="skill-bar-bg" style={{ width: '100%', height: '6px', background: '#e0e0e0', borderRadius: '3px' }}>
                                                <div className="skill-bar-fill" style={{ width: getSkillPercent(item.level), height: '100%', background: '#2c5a4b', borderRadius: '3px' }}></div>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        </div>

                        <div className="sidebar-section">
                            <h3 className="side-title">CHỨNG CHỈ</h3>
                            <div className="side-content">
                                {resumeData?.certifications?.map((item, idx) => (
                                    <div style={{ marginBottom: '8px' }} key={idx}>
                                        <div className="cert-year-div" style={{ fontWeight: 'bold', fontSize: '11px', color: '#2c5a4b' }}>{item.year}</div>
                                        <div className="cert-name-div" style={{ fontSize: '12.5px', color: '#444' }}>{item.name}</div>
                                    </div>
                                ))}
                            </div>
                        </div>
                        
                        <div className="sidebar-section">
                            <h3 className="side-title">NGOẠI NGỮ</h3>
                            <div className="side-content">
                                <ul className="lang-list-items" style={{ listStyle: 'none', padding: 0 }}>
                                    {resumeData?.languages?.map((item, idx) => (
                                        <li style={{ display: 'flex', justifyContent: 'space-between', padding: '3px 0', borderBottom: '1px dashed #eee' }} key={idx}>
                                            <span>• {item.name}</span>
                                            <span style={{ fontStyle: 'italic', opacity: 0.8 }}>{item.level}</span>
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        </div>

                        <div className="sidebar-section">
                            <h3 className="side-title">KỸ NĂNG KHÁC</h3>
                            <div className="side-content">
                                <ul className="other-skill-list-items" style={{ listStyle: 'none', padding: 0 }}>
                                    {resumeData?.otherSkills?.map((item, idx) => (
                                        <li style={{ padding: '3px 0', borderBottom: '1px dashed #eee' }} key={idx}>
                                            • {item.name} {item.level ? `(${item.level})` : ''}
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        </div>
                    </div>

                    <div className="cv-main">
                        <div className="main-section">
                            <h3 className="main-title">GIỚI THIỆU</h3>
                            <div className="main-content">{resumeData?.summary || 'Mục tiêu nghề nghiệp'}</div>
                        </div>

                        <div className="main-section">
                            <h3 className="main-title">KINH NGHIỆM LÀM VIỆC</h3>
                            <div className="main-content">
                                {resumeData?.experiences?.map((item, idx) => (
                                    <div className="exp-item" key={idx} style={{ marginBottom: '15px' }}>
                                        <span className="exp-year" style={{ color: '#2c5a4b', fontWeight: 'bold' }}>♦ {item.time}</span>
                                        <div className="exp-content">
                                            <div className="info-line"><strong>{item.role}</strong> - {item.company}</div>
                                            <div className="desc-text" dangerouslySetInnerHTML={{__html: item.desc}}></div>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>

                        <div className="main-section">
                            <h3 className="main-title">HỌC VẤN</h3>
                            <div className="main-content">
                                {resumeData?.educations?.map((item, idx) => (
                                    <div className="exp-item" key={idx} style={{ marginBottom: '10px' }}>
                                        <span className="exp-year" style={{ color: '#2c5a4b', fontWeight: 'bold' }}>♦ {item.year}</span>
                                        <div className="exp-content">
                                            <strong>{item.school}</strong><br/>
                                            {item.major}<br/>
                                            {item.gradType ? `Xếp loại: ${item.gradType}` : ''}
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>

                        <div className="main-section">
                            <h3 className="main-title">DỰ ÁN</h3>
                            <div className="main-content">
                                {resumeData?.projects?.map((item, idx) => (
                                    <div className="exp-item" key={idx} style={{ marginBottom: '15px' }}>
                                        <span className="exp-year" style={{ color: '#2c5a4b', fontWeight: 'bold' }}>♦ {item.time}</span>
                                        <div className="exp-content">
                                            <strong>{item.name}</strong> ({item.role})
                                            <div className="desc-text" dangerouslySetInnerHTML={{__html: item.desc}}></div>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>

                        <div className="main-section">
                            <h3 className="main-title">HOẠT ĐỘNG</h3>
                            <div className="main-content">
                                {resumeData?.activities?.map((item, idx) => (
                                    <div className="exp-item" key={idx} style={{ marginBottom: '12px' }}>
                                        <span className="exp-year" style={{ color: '#2c5a4b', fontWeight: 'bold' }}>♦ {item.time}</span>
                                        <div className="exp-content">
                                            <strong>{item.name}</strong>
                                            <div className="desc-text" dangerouslySetInnerHTML={{__html: item.desc}}></div>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>

                        <div className="main-section">
                            <h3 className="main-title">GIẢI THƯỞNG</h3>
                            <div className="main-content">
                                <ul style={{ paddingLeft: '15px', margin: 0 }}>
                                    {resumeData?.awards?.map((item, idx) => (
                                        <li key={idx} style={{ marginBottom: '4px' }}>{item.name}</li>
                                    ))}
                                </ul>
                            </div>
                        </div>

                        <div className="main-section">
                            <h3 className="main-title">NGƯỜI THAM CHIẾU</h3>
                            <div className="main-content">
                                {resumeData?.references?.map((item, idx) => (
                                    <p key={idx} style={{ marginBottom: '4px' }}>• {item.info}</p>
                                ))}
                            </div>
                        </div>

                        <div className="main-section">
                            <h3 className="main-title">SỞ THÍCH</h3>
                            <div className="main-content">
                                <ul style={{ paddingLeft: '15px', margin: 0 }}>
                                    {resumeData?.hobbies?.map((item, idx) => (
                                        <li key={idx} style={{ marginBottom: '4px' }}>{item.name}</li>
                                    ))}
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <style>{`
                /* RESET TUYỆT ĐỐI */
                .cv-template-DinhXuanThao * { margin: 0; padding: 0; box-sizing: border-box; }
                
                .cv-thao-wrapper { 
                    width: 210mm; min-height: 297mm; background: white; 
                    font-family: "Segoe UI", sans-serif; line-height: 1.3;
                }
                
                /* Header & Avatar */
                .cv-header { background-color: #2c5a4b; height: 140px; display: flex; align-items: center; position: relative; }
                .header-content { display: flex; align-items: center; padding-left: 50px; width: 100%; }
                .avatar-box { margin-top: 55px; z-index: 10; }
                .avatar-img { width: 170px; height: 170px; border-radius: 50%; border: 5px solid white; object-fit: cover; }
                .title-box { margin-left: 25px; margin-top: 15px; color: white; }
                .fullname { font-size: 32px; font-weight: 800; text-transform: uppercase; }
                .job-title { font-size: 15px; opacity: 0.9; letter-spacing: 2px; text-transform: uppercase; }
                
                /* Layout Body */
                .cv-body { display: flex; padding: 35px 40px 20px 40px; }
                .cv-sidebar { flex: 3.5; padding-right: 20px; border-right: 1px solid #f2f2f2; }
                .cv-main { flex: 6.5; padding-left: 30px; }
                
                /* Sidebar Sections */
                .sidebar-section { margin-bottom: 15px; }
                .side-title { 
                    background: #2c5a4b; color: white; font-size: 12px; font-weight: bold; 
                    padding: 4px 15px; border-radius: 0 20px 20px 0; margin-left: -40px; 
                    display: inline-block; margin-bottom: 8px; text-transform: uppercase;
                }
                
                .side-content, .main-content {
                    font-size: 12px;
                    color: #444;
                }
                
                .contact-info p {
                    margin-bottom: 6px;
                    display: flex;
                    align-items: center;
                    gap: 8px;
                }
                .contact-info i {
                    color: #2c5a4b;
                    width: 14px;
                }

                /* Main Sections */
                .main-section { margin-bottom: 20px; }
                .main-title { 
                    color: #2c5a4b; font-size: 15px; font-weight: 800; 
                    border-left: 4px solid #2c5a4b; padding-left: 10px; 
                    margin-bottom: 10px; text-transform: uppercase;
                }
                
                /* List items spacing */
                .main-content ul {
                    padding-left: 15px;
                }
                .main-content li {
                    margin-bottom: 4px;
                }
                
                p, div, h1, h2, h3, li { word-break: break-word; overflow-wrap: break-word; }
            `}</style>
        </div>
    );
};

export const defaultVisibility = {
    summary: true,
    experiences: true,
    educations: false,
    skills: true,
    otherSkills: false,
    projects: false,
    activities: false,
    certifications: true,
    awards: false,
    references: false,
    hobbies: false,
    languages: false
};

export default DinhXuanThao;
