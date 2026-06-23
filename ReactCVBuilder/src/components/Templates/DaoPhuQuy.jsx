import React from 'react';

const DaoPhuQuy = ({ resumeData }) => {
    return (
        <div className="cv-template-DaoPhuQuy">
            <div className="cv-wrapper">
                <div className="cv-sidebar">
                    <div className="cv-avatar-section">
                        <img src={resumeData?.avatarUrl || "/images/default-avatar.png"} className="cv-avatar" />
                    </div>
                    <div className="cv-sidebar-content">
                        {(resumeData?.phone || resumeData?.email || resumeData?.birthDate || resumeData?.address) && (
                            <div className="cv-info-block">
                                <h3 className="cv-side-title">LIÊN LẠC</h3>
                                {resumeData?.phone && <p><i className="fas fa-phone"></i> {resumeData.phone}</p>}
                                {resumeData?.email && <p><i className="fas fa-envelope"></i> {resumeData.email}</p>}
                                {resumeData?.birthDate && <p><i className="fas fa-calendar-alt"></i> {resumeData.birthDate}</p>}
                                {resumeData?.address && <p><i className="fas fa-map-marker-alt"></i> {resumeData.address}</p>}
                            </div>
                        )}

                        <div className="cv-info-block">
                            <h3 className="cv-side-title">HỌC VẤN</h3>
                            <div className="cv-edu-sidebar">
                                {resumeData?.educations?.map((item, idx) => (
                                    <div className="exp-item" key={idx} style={{ marginBottom: '10px' }}>
                                        <span className="exp-year" style={{ fontSize: '11px', display: 'block', opacity: 0.8 }}>{item.year}</span>
                                        <div className="exp-content" style={{ fontSize: '12px', marginTop: '2px' }}>
                                            <strong>{item.school}</strong><br/>
                                            {item.major}<br/>
                                            {item.gradType ? `Xếp loại: ${item.gradType}` : ''}
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>

                        <div className="cv-info-block">
                            <h3 className="cv-side-title">TIN HỌC</h3>
                            <div className="cv-skill-list">
                                <ul className="skill-list-items" style={{ listStyle: 'none', padding: 0 }}>
                                    {resumeData?.skills?.map((item, idx) => (
                                        <li style={{ padding: '3px 0', display: 'flex', justifyContent: 'space-between' }} key={idx}>
                                            <span>• {item.name}</span>
                                            <span style={{ color: '#ffcc00' }}>{item.level} <i className="fas fa-star" style={{ fontSize: '10px' }}></i></span>
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        </div>

                        <div className="cv-info-block">
                            <h3 className="cv-side-title">NGOẠI NGỮ</h3>
                            <div className="cv-lang-list">
                                <ul className="lang-list-items" style={{ listStyle: 'none', padding: 0 }}>
                                    {resumeData?.languages?.map((item, idx) => (
                                        <li style={{ display: 'flex', justifyContent: 'space-between', padding: '3px 0' }} key={idx}>
                                            <span>• {item.name}</span>
                                            <span style={{ fontStyle: 'italic', opacity: 0.8 }}>{item.level}</span>
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        </div>

                        <div className="cv-info-block">
                            <h3 className="cv-side-title">SỞ THÍCH</h3>
                            <div className="cv-lang-list">
                                <ul className="hobby-list-items" style={{ listStyle: 'none', padding: 0 }}>
                                    {resumeData?.hobbies?.map((item, idx) => (
                                        <li style={{ padding: '3px 0' }} key={idx}>• {item.name}</li>
                                    ))}
                                </ul>
                            </div>
                        </div>

                        <div className="cv-info-block">
                            <h3 className="cv-side-title">GIẢI THƯỞNG</h3>
                            <div className="cv-lang-list">
                                <ul style={{ paddingLeft: '15px', margin: 0 }}>
                                    {resumeData?.awards?.map((item, idx) => (
                                        <li key={idx} style={{ marginBottom: '4px', fontSize: '12px' }}>{item.name}</li>
                                    ))}
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>

                <div className="cv-main">
                    <div className="cv-header">
                        <h1 className="cv-name">{resumeData?.fullName || 'HỌ TÊN'}</h1>
                        <h2 className="cv-job">{resumeData?.jobTitle || 'VỊ TRÍ'}</h2>
                    </div>

                    <div className="cv-section">
                        <h3 className="cv-section-title"><i className="fas fa-user"></i> MỤC TIÊU NGHỀ NGHIỆP</h3>
                        <div className="cv-section-content">{resumeData?.summary || 'Mục tiêu nghề nghiệp'}</div>
                    </div>

                    <div className="cv-section">
                        <h3 className="cv-section-title"><i className="fas fa-briefcase"></i> KINH NGHIỆM LÀM VIỆC</h3>
                        <div className="cv-section-content">
                            {resumeData?.experiences?.map((item, idx) => (
                                <div className="exp-item" key={idx} style={{ marginBottom: '15px' }}>
                                    <span className="exp-year" style={{ color: '#004C82', fontWeight: 'bold' }}>• {item.time}</span>
                                    <div className="exp-content" style={{ marginTop: '4px' }}>
                                        <strong>{item.company}</strong> - {item.role}
                                        <div className="desc-text" style={{ fontSize: '13px', color: '#555', marginTop: '4px' }} dangerouslySetInnerHTML={{__html: item.desc}}></div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>

                    <div className="cv-section">
                        <h3 className="cv-section-title"><i className="fas fa-project-diagram"></i> DỰ ÁN NỔI BẬT</h3>
                        <div className="cv-section-content">
                            {resumeData?.projects?.map((item, idx) => (
                                <div className="exp-item" key={idx} style={{ marginBottom: '15px' }}>
                                    <span className="exp-year" style={{ color: '#004C82', fontWeight: 'bold' }}>• {item.time}</span>
                                    <div className="exp-content" style={{ marginTop: '4px' }}>
                                        <strong>{item.name}</strong> - {item.role}
                                        <div className="desc-text" style={{ fontSize: '13px', color: '#555', marginTop: '4px' }} dangerouslySetInnerHTML={{__html: item.desc}}></div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>

                    <div className="cv-section">
                        <h3 className="cv-section-title"><i className="fas fa-history"></i> HOẠT ĐỘNG</h3>
                        <div className="cv-section-content">
                            {resumeData?.activities?.map((item, idx) => (
                                <div className="exp-item" key={idx} style={{ marginBottom: '12px' }}>
                                    <span className="exp-year" style={{ color: '#004C82', fontWeight: 'bold' }}>• {item.time}</span>
                                    <div className="exp-content" style={{ marginTop: '4px' }}>
                                        <strong>{item.name}</strong>
                                        <div className="desc-text" style={{ fontSize: '13px', color: '#555', marginTop: '4px' }} dangerouslySetInnerHTML={{__html: item.desc}}></div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>

                    <div className="cv-section">
                        <h3 className="cv-section-title"><i className="fas fa-certificate"></i> CHỨNG CHỈ</h3>
                        <div className="cv-section-content">
                            <ul style={{ paddingLeft: '15px', margin: 0 }}>
                                {resumeData?.certifications?.map((item, idx) => (
                                    <li key={idx} style={{ marginBottom: '4px' }}>
                                        <strong>{item.year}:</strong> {item.name}
                                    </li>
                                ))}
                            </ul>
                        </div>
                    </div>

                    <div className="cv-section">
                        <h3 className="cv-section-title"><i className="fas fa-users"></i> NGƯỜI THAM CHIẾU</h3>
                        <div className="cv-section-content">
                            {resumeData?.references?.map((item, idx) => (
                                <p key={idx} style={{ marginBottom: '5px', fontSize: '13px' }}>• {item.info}</p>
                            ))}
                        </div>
                    </div>

                    <div className="cv-section">
                        <h3 className="cv-section-title"><i className="fas fa-cog"></i> KỸ NĂNG KHÁC</h3>
                        <div className="cv-section-content">
                            <ul style={{ listStyleType: 'disc', paddingLeft: '20px', margin: 0 }}>
                                {resumeData?.otherSkills?.map((item, idx) => (
                                    <li key={idx} style={{ marginBottom: '4px' }}>
                                        {item.name} {item.level ? `(${item.level})` : ''}
                                    </li>
                                ))}
                            </ul>
                        </div>
                    </div>
                </div>
            </div>
            
            <style>{`
                /* Layout chung */
                .cv-template-DaoPhuQuy * { box-sizing: border-box; margin: 0; padding: 0; }
                .cv-wrapper { display: flex; width: 210mm; min-height: 297mm; background: white; font-family: "Segoe UI", sans-serif; }
                .cv-sidebar { flex: 3.5; background: #004C82; color: white; padding: 30px 20px; }
                .cv-main { flex: 6.5; padding: 40px; background: #ffffff; }

                .cv-avatar-section { text-align: center; margin-bottom: 30px; }
                .cv-avatar { width: 160px; height: 160px; border-radius: 50%; border: 5px solid rgba(255,255,255,0.2); object-fit: cover; }

                .cv-side-title { font-size: 16px; font-weight: bold; border-bottom: 1px solid rgba(255,255,255,0.3); padding-bottom: 5px; margin-bottom: 10px; margin-top: 20px; text-transform: uppercase; }
                
                /* STYLE CHO SIDEBAR VÀ CHỐNG TRÀN */
                .cv-info-block p, .cv-other-skills, .cv-edu-sidebar, .cv-lang-list, .cv-skill-list { 
                    font-size: 13px; 
                    margin-bottom: 8px; 
                    line-height: 1.5;
                    word-wrap: break-word; 
                    overflow-wrap: break-word; 
                }
                .cv-info-block i { width: 20px; text-align: center; color: #ffcc00; }

                /* MAIN CONTENT */
                .cv-header { border-bottom: 3px solid #004C82; padding-bottom: 15px; margin-bottom: 30px; }
                .cv-name { font-size: 40px; font-weight: 800; color: #004C82; margin: 0; text-transform: uppercase; }
                .cv-job { font-size: 18px; color: #555; margin: 5px 0 0 0; text-transform: uppercase; letter-spacing: 2px; }

                .cv-section { margin-bottom: 25px; }
                .cv-section-title { font-size: 17px; font-weight: bold; color: #004C82; display: flex; align-items: center; gap: 10px; margin-bottom: 10px; border-bottom: 1px solid #eee; padding-bottom: 5px; text-transform: uppercase; }

                .cv-section-content { 
                    font-size: 14px; 
                    line-height: 1.6; 
                    color: #333; 
                    text-align: justify; 
                    word-wrap: break-word; 
                    overflow-wrap: break-word; 
                }
            `}</style>
        </div>
    );
};

export const defaultVisibility = {
    summary: true,
    experiences: true,
    educations: true,
    skills: true,
    otherSkills: true,
    projects: false,
    activities: false,
    certifications: false,
    awards: false,
    references: false,
    hobbies: false,
    languages: true
};

export default DaoPhuQuy;
