import React from 'react';

const NgoHaiYen = ({ resumeData }) => {
    const getSkillDots = (level) => {
        let filled = 4;
        if (level) {
            const lvl = level.toLowerCase();
            if (lvl.includes('%')) {
                const num = parseInt(lvl);
                filled = Math.round(num / 20);
            } else if (lvl.includes('xuất sắc') || lvl.includes('expert') || lvl.includes('master')) {
                filled = 5;
            } else if (lvl.includes('tốt') || lvl.includes('thành thạo') || lvl.includes('advanced') || lvl.includes('fluent')) {
                filled = 4;
            } else if (lvl.includes('khá') || lvl.includes('intermediate')) {
                filled = 3;
            } else if (lvl.includes('trung bình') || lvl.includes('cơ bản') || lvl.includes('basic')) {
                filled = 2;
            }
        }
        
        return (
            <span className="skill-dots" style={{ color: '#5fb4c4', letterSpacing: '2px' }}>
                {'●'.repeat(filled)}
                <span style={{ color: 'rgba(255,255,255,0.3)' }}>{'●'.repeat(5 - filled)}</span>
            </span>
        );
    };

    return (
        <div className="cv-template-NgoHaiYen">
            <div className="cv-yens-wrapper">
                <div className="cv-sidebar">
                    <div className="avatar-box">
                        <img src={resumeData?.avatarUrl || "/images/default-avatar.png"} className="avatar-img" />
                    </div>

                    {(resumeData?.address || resumeData?.email || resumeData?.phone || resumeData?.website) && (
                        <div className="sidebar-section">
                            <h3 className="sidebar-title">LIÊN HỆ VỚI TÔI</h3>
                            <div className="contact-list">
                                {resumeData?.address && <div className="contact-item"><i className="fas fa-map-marker-alt"></i> {resumeData.address}</div>}
                                {resumeData?.email && <div className="contact-item"><i className="fas fa-envelope"></i> {resumeData.email}</div>}
                                {resumeData?.phone && <div className="contact-item"><i className="fas fa-phone"></i> {resumeData.phone}</div>}
                                {resumeData?.website && <div className="contact-item"><i className="fas fa-globe"></i> {resumeData.website}</div>}
                            </div>
                        </div>
                    )}

                    <div className="sidebar-section">
                        <h3 className="sidebar-title">TÓM TẮT KỸ NĂNG</h3>
                        <div className="skills-dots-container">
                            {resumeData?.skills?.map((item, idx) => (
                                <div className="skill-dot-item" key={idx} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '8px', fontSize: '13px' }}>
                                    <span style={{ maxWidth: '60%', wordBreak: 'break-word' }}>{item.name}</span>
                                    {getSkillDots(item.level)}
                                </div>
                            ))}
                        </div>
                    </div>

                    <div className="sidebar-section">
                        <h3 className="sidebar-title">NGOẠI NGỮ</h3>
                        <div className="skill-content">
                            <ul className="lang-list-items" style={{ listStyle: 'none', padding: 0 }}>
                                {resumeData?.languages?.map((item, idx) => (
                                    <li style={{display:'flex', justifyContent:'space-between', marginBottom: '6px'}} key={idx}>
                                        <span>• {item.name}</span>
                                        <span style={{fontStyle:'italic', opacity:0.8}}>{item.level}</span>
                                    </li>
                                ))}
                            </ul>
                        </div>
                    </div>

                    <div className="sidebar-section">
                        <h3 className="sidebar-title">KỸ NĂNG KHÁC</h3>
                        <div className="skill-content">
                            <ul className="other-skill-list-items" style={{ listStyle: 'none', padding: 0 }}>
                                {resumeData?.otherSkills?.map((item, idx) => (
                                    <li style={{ marginBottom: '6px' }} key={idx}>
                                        • {item.name} {item.level ? `(${item.level})` : ''}
                                    </li>
                                ))}
                            </ul>
                        </div>
                    </div>

                    <div className="sidebar-section">
                        <h3 className="sidebar-title">GIẢI THƯỞNG</h3>
                        <div className="award-list">
                            <ul style={{paddingLeft:'15px', margin:0}}>
                                {resumeData?.awards?.map((item, idx) => (
                                    <li key={idx} style={{ marginBottom: '6px' }}>{item.name}</li>
                                ))}
                            </ul>
                        </div>
                    </div>

                    <div className="sidebar-section">
                        <h3 className="sidebar-title">SỞ THÍCH</h3>
                        <div className="skill-content">
                            <ul className="hobby-list-items" style={{ listStyle: 'none', padding: 0 }}>
                                {resumeData?.hobbies?.map((item, idx) => (
                                    <li style={{ marginBottom: '6px' }} key={idx}>• {item.name}</li>
                                ))}
                            </ul>
                        </div>
                    </div>
                </div>

                <div className="cv-main">
                    <div className="header-info">
                        <h1 className="fullname">{resumeData?.fullName || 'HỌ TÊN'}</h1>
                        <h2 className="job-title">{resumeData?.jobTitle || 'VỊ TRÍ'}</h2>
                    </div>

                    <div className="main-section">
                        <h3 className="section-title">GIỚI THIỆU</h3>
                        <div className="section-content">{resumeData?.summary || 'Mục tiêu nghề nghiệp'}</div>
                    </div>

                    <div className="main-section">
                        <h3 className="section-title">KINH NGHIỆM LÀM VIỆC</h3>
                        <div className="section-content">
                            {resumeData?.experiences?.map((item, idx) => (
                                <div className="exp-item" key={idx} style={{ marginBottom: '20px' }}>
                                    <span className="exp-year" style={{ color: '#5fb4c4', fontWeight: 'bold' }}>• {item.time}</span>
                                    <div className="exp-content" style={{ marginTop: '4px' }}>
                                        <strong>{item.role}</strong> - {item.company}
                                        <div className="desc-text" style={{ marginTop: '6px', fontSize: '13px', color: '#555' }} dangerouslySetInnerHTML={{__html: item.desc}}></div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>

                    <div className="main-section">
                        <h3 className="section-title">QUÁ TRÌNH HỌC VẤN</h3>
                        <div className="section-content">
                            {resumeData?.educations?.map((item, idx) => (
                                <div className="exp-item" key={idx} style={{ marginBottom: '15px' }}>
                                    <span className="exp-year" style={{ color: '#5fb4c4', fontWeight: 'bold' }}>• {item.year}</span>
                                    <div className="exp-content" style={{ marginTop: '4px' }}>
                                        <strong>{item.school}</strong><br/>
                                        {item.major}<br/>
                                        {item.gradType ? `Xếp loại: ${item.gradType}` : ''}
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>

                    <div className="main-section">
                        <h3 className="section-title">DỰ ÁN</h3>
                        <div className="section-content">
                            {resumeData?.projects?.map((item, idx) => (
                                <div className="exp-item" key={idx} style={{ marginBottom: '20px' }}>
                                    <span className="exp-year" style={{ color: '#5fb4c4', fontWeight: 'bold' }}>• {item.time}</span>
                                    <div className="exp-content" style={{ marginTop: '4px' }}>
                                        <strong>{item.name}</strong> ({item.role})
                                        <div className="desc-text" style={{ marginTop: '6px', fontSize: '13px', color: '#555' }} dangerouslySetInnerHTML={{__html: item.desc}}></div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>

                    <div className="main-section">
                        <h3 className="section-title">HOẠT ĐỘNG</h3>
                        <div className="section-content">
                            {resumeData?.activities?.map((item, idx) => (
                                <div className="exp-item" key={idx} style={{ marginBottom: '15px' }}>
                                    <span className="exp-year" style={{ color: '#5fb4c4', fontWeight: 'bold' }}>• {item.time}</span>
                                    <div className="exp-content" style={{ marginTop: '4px' }}>
                                        <strong>{item.name}</strong>
                                        <div className="desc-text" style={{ marginTop: '6px', fontSize: '13px', color: '#555' }} dangerouslySetInnerHTML={{__html: item.desc}}></div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>

                    <div className="main-section">
                        <h3 className="section-title">CHỨNG CHỈ</h3>
                        <div className="section-content">
                            <ul style={{ paddingLeft: '15px', margin: 0 }}>
                                {resumeData?.certifications?.map((item, idx) => (
                                    <li key={idx} style={{ marginBottom: '6px' }}>
                                        <strong>{item.year}:</strong> {item.name}
                                    </li>
                                ))}
                            </ul>
                        </div>
                    </div>

                    <div className="main-section">
                        <h3 className="section-title">NGƯỜI THAM CHIẾU</h3>
                        <div className="section-content">
                            {resumeData?.references?.map((item, idx) => (
                                <p key={idx} style={{ marginBottom: '6px' }}>• {item.info}</p>
                            ))}
                        </div>
                    </div>
                </div>
            </div>

            <style>{`
                /* Layout và màu sắc chủ đạo */
                .cv-yens-wrapper { display: flex; width: 210mm; min-height: 297mm; background: white; font-family: "Segoe UI", Arial, sans-serif; }
                .cv-sidebar { flex: 3.5; background-color: #32507d; color: white; padding: 40px 25px; }
                .cv-main { flex: 6.5; padding: 50px 40px; }

                /* Avatar tròn */
                .avatar-box { text-align: center; margin-bottom: 40px; }
                .avatar-img { width: 160px; height: 160px; border-radius: 50%; border: 6px solid rgba(255,255,255,0.1); object-fit: cover; }

                /* Sidebar Styles */
                .sidebar-title { font-size: 16px; font-weight: bold; border-bottom: 1px solid rgba(255,255,255,0.3); padding-bottom: 8px; margin-bottom: 15px; margin-top: 30px; letter-spacing: 1px; text-transform: uppercase; }
                .contact-list { display: flex; flex-direction: column; gap: 12px; }
                .contact-item { font-size: 13px; display: flex; align-items: flex-start; gap: 10px; line-height: 1.4; word-break: break-word; }
                .contact-item i { margin-top: 3px; width: 15px; color: #5fb4c4; }
                
                .skill-content { font-size: 13px; margin-bottom: 15px; }
                .award-list { font-size: 13px; line-height: 1.6; }

                /* Main Content Styles */
                .fullname { font-size: 48px; color: #32507d; margin: 0; font-weight: 800; text-transform: uppercase; }
                .job-title { font-size: 20px; color: #5fb4c4; margin: 5px 0 40px 0; letter-spacing: 3px; font-weight: bold; text-transform: uppercase; }

                .section-title { font-size: 16px; font-weight: bold; color: #5fb4c4; margin-bottom: 15px; margin-top: 35px; letter-spacing: 2px; text-transform: uppercase; }
                .section-content { 
                    font-size: 14px; 
                    line-height: 1.8; 
                    color: #444; 
                    text-align: justify; 
                    word-break: break-word;
                    border-left: 1px solid #eee;
                    padding-left: 15px;
                }

                p, div, span { word-wrap: break-word; overflow-wrap: break-word; }
            `}</style>
        </div>
    );
};

export const defaultVisibility = {
    summary: true,
    experiences: true,
    educations: true,
    skills: true,
    otherSkills: false,
    projects: false,
    activities: false,
    certifications: false,
    awards: true,
    references: false,
    hobbies: false,
    languages: false
};

export default NgoHaiYen;
