import React from 'react';

const NguyenYenNhi = ({ resumeData }) => {
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
        <div className="cv-template-NguyenYenNhi">
            <div className="pink-cv-container">
                <div className="deco-star star-1">✦</div>
                <div className="deco-star star-2">✦</div>

                <div className="cv-header">
                    <div className="header-info">
                        <h1 className="fullname"><span dangerouslySetInnerHTML={{ __html: resumeData?.fullName || 'HỌ TÊN' }}></span></h1>
                        <p className="job-title"><span dangerouslySetInnerHTML={{ __html: resumeData?.jobTitle || 'VỊ TRÍ' }}></span></p>
                    </div>
                    <div className="header-photo">
                        <div className="photo-bg-circle"></div>
                        <img src={resumeData?.avatarUrl || "/images/default-avatar.png"} className="avatar-img" />
                    </div>
                </div>

                <div className="cv-body">
                    <div className="col-left">
                        <div className="section">
                            <h1 className="section-title">VỀ TÔI</h1>
                            <div className="content-text"><span dangerouslySetInnerHTML={{ __html: resumeData?.summary || 'Mục tiêu nghề nghiệp' }}></span></div>
                        </div>

                        <div className="section">
                            <h1 className="section-title">HỌC VẤN</h1>
                            <div className="timeline">
                                {resumeData?.educations?.map((item, idx) => (
                                    <div className="exp-item" key={idx}>
                                        <span className="exp-year">✦ {item.year}</span>
                                        <div className="exp-content">
                                            <strong>{item.major || 'Chuyên ngành'}</strong><br/>
                                            {item.school || 'Trường học'}<br/>
                                            {item.gradType ? `Xếp loại: ${item.gradType}` : ''}
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>

                        <div className="section">
                            <h1 className="section-title">KỸ NĂNG</h1>
                            <div className="skills-container">
                                {resumeData?.skills?.map((item, idx) => (
                                    <div className="skill-item" key={idx}>
                                        <div className="skill-info">
                                            <span>{item.name}</span>
                                        </div>
                                        <div className="skill-bar-bg">
                                            <div className="skill-bar-fill" style={{ width: getSkillPercent(item.level) }}></div>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>

                        <div className="section">
                            <h1 className="section-title">NGOẠI NGỮ</h1>
                            <div className="content-text">
                                <ul className="lang-list-items">
                                    {resumeData?.languages?.map((item, idx) => (
                                        <li style={{display:'flex', justifyContent:'space-between', padding:'4px 0', borderBottom:'1px dashed #fcdde1'}} key={idx}>
                                            <span>✦ {item.name}</span>
                                            <span style={{fontStyle:'italic', opacity:0.8}}>{item.level}</span>
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        </div>

                        <div className="section">
                            <h1 className="section-title">KỸ NĂNG KHÁC</h1>
                            <div className="content-text">
                                <ul className="other-skill-list-items">
                                    {resumeData?.otherSkills?.map((item, idx) => (
                                        <li style={{padding:'4px 0', borderBottom:'1px dashed #fcdde1'}} key={idx}>
                                            ✦ {item.name} {item.level ? `(${item.level})` : ''}
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        </div>

                        <div className="section">
                            <h1 className="section-title">DỰ ÁN</h1>
                            <div className="timeline">
                                {resumeData?.projects?.map((item, idx) => (
                                    <div className="exp-item" key={idx}>
                                        <span className="exp-year">✦ {item.time}</span>
                                        <div className="exp-content">
                                            <strong>{item.name}</strong> ({item.role})<br/>
                                            <div className="desc-text" dangerouslySetInnerHTML={{__html: item.desc}}></div>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </div>

                    <div className="col-right">
                        {(resumeData?.phone || resumeData?.email || resumeData?.address) && (
                            <div className="section">
                                <h1 className="section-title">LIÊN HỆ</h1>
                                <div className="contact-list">
                                    {resumeData?.phone && <div className="contact-item"><span>📞</span> <span dangerouslySetInnerHTML={{ __html: resumeData?.phone }}></span></div>}
                                    {resumeData?.email && <div className="contact-item"><span>✉️</span> <span dangerouslySetInnerHTML={{ __html: resumeData?.email }}></span></div>}
                                    {resumeData?.address && <div className="contact-item"><span>📍</span> <span dangerouslySetInnerHTML={{ __html: resumeData?.address }}></span></div>}
                                </div>
                            </div>
                        )}

                        <div className="section">
                            <h1 className="section-title">KINH NGHIỆM</h1>
                            <div className="timeline">
                                {resumeData?.experiences?.map((item, idx) => (
                                    <div className="exp-item" key={idx}>
                                        <span className="exp-year">✦ {item.time} | {item.company}</span>
                                        <div className="exp-content">
                                            <strong>{item.role}</strong>
                                            <div className="desc-text" dangerouslySetInnerHTML={{__html: item.desc}}></div>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                        
                        <div className="section">
                            <h1 className="section-title">GIẢI THƯỞNG</h1>
                            <div className="content-text">
                                <ul style={{paddingLeft:'15px', margin:0}}>
                                    {resumeData?.awards?.map((item, idx) => (
                                        <li key={idx} style={{marginBottom:'6px'}}>✦ {item.name}</li>
                                    ))}
                                </ul>
                            </div>
                        </div>

                        <div className="section">
                            <h1 className="section-title">HOẠT ĐỘNG</h1>
                            <div className="timeline">
                                {resumeData?.activities?.map((item, idx) => (
                                    <div className="exp-item" key={idx}>
                                        <span className="exp-year">✦ {item.time} | {item.name}</span>
                                        <div className="exp-content">
                                            <div className="desc-text" dangerouslySetInnerHTML={{__html: item.desc}}></div>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>

                        <div className="section">
                            <h1 className="section-title">CHỨNG CHỈ</h1>
                            <div className="content-text">
                                <ul style={{paddingLeft:'15px', margin:0}}>
                                    {resumeData?.certifications?.map((item, idx) => (
                                        <li key={idx} style={{marginBottom:'6px'}}>
                                            <strong>{item.year}:</strong> {item.name}
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        </div>

                        <div className="section">
                            <h1 className="section-title">NGƯỜI THAM CHIẾU</h1>
                            <div className="content-text">
                                {resumeData?.references?.map((item, idx) => (
                                    <p key={idx} style={{marginBottom:'5px'}}>✦ {item.info}</p>
                                ))}
                            </div>
                        </div>

                        <div className="section">
                            <h1 className="section-title">SỞ THÍCH</h1>
                            <div className="content-text">
                                <ul style={{paddingLeft:'15px', margin:0}}>
                                    {resumeData?.hobbies?.map((item, idx) => (
                                        <li key={idx} style={{marginBottom:'6px'}}>✦ {item.name}</li>
                                    ))}
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
                
                <div className="cv-footer">
                    <span className="footer-web">@reallygreatsite</span>
                </div>
            </div>
            
            <style>{`
                /* Layout & Colors */
                .pink-cv-container {
                    width: 210mm;
                    min-height: 297mm;
                    padding: 50px;
                    background: #fff;
                    position: relative;
                    font-family: "Segoe UI", Tahoma, Geneva, Verdana, sans-serif;
                    color: #333;
                    box-sizing: border-box;
                }

                /* Header & Circle Avatar */
                .cv-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 30px; }
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
                .cv-body { display: flex; gap: 40px; }
                .col-left { flex: 1.1; }
                .col-right { flex: 0.9; }

                h1.section-title {
                    font-size: 18px;
                    font-weight: bold;
                    margin-bottom: 15px;
                    margin-top: 25px;
                    color: #111;
                    border-bottom: 2px solid #333;
                    padding-bottom: 5px;
                    text-transform: uppercase;
                }

                /* Content Lists */
                .content-text, .contact-list { font-size: 13px; line-height: 1.6; }

                /* Contact Style */
                .contact-list {
                    display: flex;
                    flex-direction: column;
                    gap: 8px;
                }
                .contact-item {
                    display: flex;
                    align-items: center;
                    gap: 10px;
                }
                .contact-item span {
                    color: #f497a9;
                    font-size: 16px;
                }

                /* Timeline Styling */
                .timeline { border-left: 1px dashed #f497a9; padding-left: 20px; margin-left: 5px; }
                .exp-item { position: relative; margin-bottom: 20px; font-size: 13px; }
                .exp-year { font-weight: bold; color: #f497a9; display: block; margin-bottom: 4px; }
                .exp-content { line-height: 1.5; color: #555; }
                .desc-text { font-size: 12.5px; color: #666; margin-top: 4px; }

                /* Skill Bars */
                .skills-container {
                    display: flex;
                    flex-direction: column;
                    gap: 12px;
                }
                .skill-item {
                    display: flex;
                    flex-direction: column;
                    gap: 4px;
                }
                .skill-info {
                    font-size: 13px;
                    font-weight: 500;
                }
                .skill-bar-bg { width: 100%; height: 6px; background: #f0f0f0; border-radius: 10px; }
                .skill-bar-fill { height: 100%; background: #f497a9; border-radius: 10px; }

                /* Decoration Sparkles */
                .deco-star { position: absolute; color: #f497a9; opacity: 0.5; }
                .star-1 { top: 30px; left: 45%; font-size: 25px; }
                .star-2 { top: 120px; right: 40px; font-size: 18px; }

                .cv-footer { 
                    position: absolute; 
                    bottom: 30px; 
                    width: calc(100% - 100px); 
                    display: flex; 
                    justify-content: flex-end; 
                    font-size: 12px; 
                    color: #aaa; 
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
    otherSkills: false,
    projects: false,
    activities: false,
    certifications: false,
    awards: false,
    references: false,
    hobbies: false,
    languages: false
};

export default NguyenYenNhi;
