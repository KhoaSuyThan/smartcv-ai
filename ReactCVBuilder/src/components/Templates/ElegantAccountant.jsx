import React from 'react';

const ElegantAccountant = ({ resumeData }) => {
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
        <div className="cv-template-ElegantAccountant">
            <div className="accountant-cv">
                <div className="decor-top-right"></div>

                <div className="header-section">
                    <div className="header-left">
                        <h1 className="name-display"><span dangerouslySetInnerHTML={{ __html: resumeData?.fullName || 'HỌ TÊN' }}></span></h1>
                        {(resumeData?.birthDate || resumeData?.address || resumeData?.email || resumeData?.phone) && (
                            <div className="personal-details">
                                {resumeData?.birthDate && <p><strong>Ngày sinh:</strong> <span dangerouslySetInnerHTML={{ __html: resumeData?.birthDate }}></span></p>}
                                {resumeData?.address && <p><strong>Địa chỉ:</strong> <span dangerouslySetInnerHTML={{ __html: resumeData?.address }}></span></p>}
                                {resumeData?.email && <p><strong>Email:</strong> <span dangerouslySetInnerHTML={{ __html: resumeData?.email }}></span></p>}
                                {resumeData?.phone && <p><strong>Số điện thoại:</strong> <span dangerouslySetInnerHTML={{ __html: resumeData?.phone }}></span></p>}
                            </div>
                        )}
                    </div>

                    <div className="header-center">
                        <div className="avatar-container">
                            <img src={resumeData?.avatarUrl || "/images/default-avatar.png"} className="avatar-img" />
                        </div>
                    </div>

                    <div className="header-right">
                        <h2 className="job-display"><span dangerouslySetInnerHTML={{ __html: resumeData?.jobTitle || 'VỊ TRÍ' }}></span></h2>
                        <div className="job-line"></div>
                    </div>
                </div>

                <div className="body-section">
                    <div className="cv-block">
                        <h3 className="block-title">MỤC TIÊU NGHỀ NGHIỆP</h3>
                        <div className="desc-text" style={{ fontSize: '15px', lineHeight: '1.6', textAlign: 'justify' }}><span dangerouslySetInnerHTML={{ __html: resumeData?.summary || 'Mục tiêu nghề nghiệp' }}></span></div>
                    </div>

                    <div className="cv-block">
                        <h3 className="block-title">KINH NGHIỆM LÀM VIỆC</h3>
                        <div className="experience-grid">
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

                    <div className="cv-block">
                        <h3 className="block-title">TRÌNH ĐỘ HỌC VẤN</h3>
                        <div className="education-list">
                            {resumeData?.educations?.map((item, idx) => (
                                <div className="exp-item" key={idx} style={{ marginBottom: '15px' }}>
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

                    <div className="cv-block">
                        <h3 className="block-title">DỰ ÁN NỔI BẬT</h3>
                        <div className="experience-grid">
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

                    <div className="cv-block">
                        <h3 className="block-title">KỸ NĂNG</h3>
                        <div className="skills-flex">
                            {resumeData?.skills?.map((item, idx) => (
                                <div key={idx} className="skill-item-block" style={{ marginBottom: '15px' }}>
                                    <span className="skill-label">• {item.name}</span>
                                    <div className="progress-bg">
                                        <div className="progress-fill" style={{ width: getSkillPercent(item.level) }}></div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>

                    <div className="cv-block">
                        <h3 className="block-title">KỸ NĂNG KHÁC</h3>
                        <div className="skills-flex">
                            <ul className="skill-list-items" style={{ listStyleType: 'none', padding: 0 }}>
                                {resumeData?.otherSkills?.map((item, idx) => (
                                    <li key={idx} style={{ fontSize: '15px', marginBottom: '8px' }}>• {item.name} {item.level ? `(${item.level})` : ''}</li>
                                ))}
                            </ul>
                        </div>
                    </div>

                    <div className="cv-block">
                        <h3 className="block-title">NGOẠI NGỮ</h3>
                        <div className="skills-flex">
                            <ul className="skill-list-items" style={{ listStyleType: 'none', padding: 0, width: '100%' }}>
                                {resumeData?.languages?.map((item, idx) => (
                                    <li key={idx} style={{ display: 'flex', justifyContent: 'space-between', width: '100%', fontSize: '15px', marginBottom: '8px' }}>
                                        <span>• {item.name}</span>
                                        <span style={{ fontStyle: 'italic', opacity: 0.8 }}>{item.level}</span>
                                    </li>
                                ))}
                            </ul>
                        </div>
                    </div>

                    <div className="cv-block">
                        <h3 className="block-title">HOẠT ĐỘNG</h3>
                        <div className="experience-grid">
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

                    <div className="cv-block">
                        <h3 className="block-title">CHỨNG CHỈ</h3>
                        <div className="experience-grid">
                            {resumeData?.certifications?.map((item, idx) => (
                                <div className="exp-item" key={idx}>
                                    <span className="exp-year">• {item.year}</span>
                                    <div className="exp-content">
                                        <div className="info-line"><strong>{item.name}</strong></div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>

                    <div className="cv-block">
                        <h3 className="block-title">DANH HIỆU & GIẢI THƯỞNG</h3>
                        <ul style={{ paddingLeft: '20px', margin: 0 }}>
                            {resumeData?.awards?.map((item, idx) => (
                                <li key={idx} style={{ fontSize: '15px', marginBottom: '8px' }}>• {item.name}</li>
                            ))}
                        </ul>
                    </div>

                    <div className="cv-block">
                        <h3 className="block-title">NGƯỜI THAM CHIẾU</h3>
                        <div className="ref-content">
                            {resumeData?.references?.map((item, idx) => (
                                <p key={idx} style={{ marginBottom: '8px', fontSize: '15px' }}>• {item.info}</p>
                            ))}
                        </div>
                    </div>

                    <div className="cv-block">
                        <h3 className="block-title">SỞ THÍCH</h3>
                        <ul style={{ listStyleType: 'none', padding: 0, display: 'flex', flexWrap: 'wrap', gap: '15px' }}>
                            {resumeData?.hobbies?.map((item, idx) => (
                                <li key={idx} style={{ fontSize: '15px' }}>• {item.name}</li>
                            ))}
                        </ul>
                    </div>
                </div>

                <div className="decor-bottom-right"></div>
            </div>

            <style>{`
                /* Cấu trúc Layout */
                .accountant-cv { background: white; min-height: 297mm; padding: 50px; font-family: "Segoe UI", sans-serif; color: #333; position: relative; overflow: hidden; }
                
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
                .block-title { font-size: 24px; font-weight: bold; margin: 35px 0 15px 0; color: #222; border-bottom: 2px solid #556b8d; padding-bottom: 5px; text-transform: uppercase; }
                
                /* CHIA 2 CỘT KINH NGHIỆM */
                .experience-grid { 
                    display: grid; 
                    grid-template-columns: 1fr 1fr; 
                    column-gap: 50px; 
                    row-gap: 25px; 
                    align-items: flex-start;
                }
                
                .exp-item { 
                    font-size: 15px; 
                    word-wrap: break-word; 
                    overflow-wrap: anywhere; 
                }
                
                .exp-year { font-weight: bold; font-size: 17px; margin-bottom: 5px; display: block; color: #556b8d; }
                .exp-content { line-height: 1.6; text-align: justify; }

                /* Học vấn và Kỹ năng */
                .education-list { font-size: 15.5px; line-height: 1.7; }
                .skills-flex { display: grid; grid-template-columns: 1fr 1fr; gap: 20px 60px; }
                .skill-label { font-size: 15.5px; font-weight: 600; margin-bottom: 8px; display: block; }
                .progress-bg { height: 10px; background: #e0e6ed; border-radius: 5px; }
                .progress-fill { height: 100%; background: #556b8d; border-radius: 5px; }

                /* Họa tiết trang trí (Scribbles) */
                .decor-top-right { position: absolute; top: 10px; right: 20px; width: 100px; height: 100px; background: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='100' height='100'%3E%3Cpath d='M10 10 Q 50 10 90 90' fill='none' stroke='black' stroke-width='1'/%3E%3C/svg%3E") no-repeat; opacity: 0.5; }
            `}</style>
        </div>
    );
};

export const defaultVisibility = {
    summary: false,
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

export default ElegantAccountant;
