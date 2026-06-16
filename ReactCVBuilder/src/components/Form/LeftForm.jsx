import React from 'react';

const LeftForm = ({ resumeData, onChange }) => {

    const handleFieldChange = (field, value) => {
        onChange(field, value);
    };

    const handleArrayChange = (field, index, key, value) => {
        const newArray = [...resumeData[field]];
        newArray[index] = { ...newArray[index], [key]: value };
        onChange(field, newArray);
    };

    const addArrayItem = (field, defaultItem) => {
        onChange(field, [...(resumeData[field] || []), { id: Date.now(), ...defaultItem }]);
    };

    const removeArrayItem = (field, index) => {
        const newArray = [...resumeData[field]];
        newArray.splice(index, 1);
        onChange(field, newArray);
    };

    const renderInput = (label, value, onChangeText, placeholder = "") => (
        <div style={{ marginBottom: '10px' }}>
            <label style={{ display: 'block', fontSize: '12px', fontWeight: 'bold', marginBottom: '4px', color: '#555' }}>{label}</label>
            <input 
                type="text" 
                value={value || ''} 
                onChange={e => onChangeText(e.target.value)} 
                placeholder={placeholder}
                style={{ width: '100%', padding: '8px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '13px' }}
            />
        </div>
    );

    const renderTextarea = (label, value, onChangeText, placeholder = "") => (
        <div style={{ marginBottom: '10px' }}>
            <label style={{ display: 'block', fontSize: '12px', fontWeight: 'bold', marginBottom: '4px', color: '#555' }}>{label}</label>
            <textarea 
                value={value || ''} 
                onChange={e => onChangeText(e.target.value)} 
                placeholder={placeholder}
                rows={4}
                style={{ width: '100%', padding: '8px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '13px', fontFamily: 'inherit' }}
            />
        </div>
    );

    return (
        <div style={{ padding: '20px' }}>
            <h3 style={{ marginTop: 0, color: '#0d6efd', borderBottom: '2px solid #0d6efd', paddingBottom: '10px' }}>
                ✍️ Chỉnh sửa thông tin
            </h3>

            {/* THÔNG TIN CÁ NHÂN */}
            <div style={{ background: '#f8f9fa', padding: '15px', borderRadius: '8px', marginBottom: '20px', border: '1px solid #e9ecef' }}>
                <h4 style={{ marginTop: 0, fontSize: '14px', color: '#333' }}>👤 Thông tin cá nhân</h4>
                {renderInput('Họ và tên', resumeData.fullName, val => handleFieldChange('fullName', val))}
                {renderInput('Vị trí ứng tuyển', resumeData.jobTitle, val => handleFieldChange('jobTitle', val))}
                {renderInput('Ảnh đại diện (URL)', resumeData.avatarUrl, val => handleFieldChange('avatarUrl', val))}
                <div style={{ display: 'flex', gap: '10px' }}>
                    <div style={{ flex: 1 }}>{renderInput('Số điện thoại', resumeData.phone, val => handleFieldChange('phone', val))}</div>
                    <div style={{ flex: 1 }}>{renderInput('Email', resumeData.email, val => handleFieldChange('email', val))}</div>
                </div>
                <div style={{ display: 'flex', gap: '10px' }}>
                    <div style={{ flex: 1 }}>{renderInput('Ngày sinh', resumeData.birthDate, val => handleFieldChange('birthDate', val))}</div>
                    <div style={{ flex: 1 }}>{renderInput('Địa chỉ', resumeData.address, val => handleFieldChange('address', val))}</div>
                </div>
                {renderInput('Website/Liên kết', resumeData.website, val => handleFieldChange('website', val))}
                {renderTextarea('Mục tiêu nghề nghiệp', resumeData.summary, val => handleFieldChange('summary', val))}
            </div>

            {/* KINH NGHIỆM LÀM VIỆC */}
            <div style={{ background: '#f8f9fa', padding: '15px', borderRadius: '8px', marginBottom: '20px', border: '1px solid #e9ecef' }}>
                <h4 style={{ marginTop: 0, fontSize: '14px', color: '#333' }}>💼 Kinh nghiệm làm việc</h4>
                {resumeData.experiences?.map((item, idx) => (
                    <div key={item.id || idx} style={{ background: 'white', padding: '10px', borderRadius: '5px', marginBottom: '10px', border: '1px solid #ddd', position: 'relative' }}>
                        <button onClick={() => removeArrayItem('experiences', idx)} style={{ position: 'absolute', top: '5px', right: '5px', background: 'none', border: 'none', color: '#dc3545', cursor: 'pointer' }}>✖</button>
                        {renderInput('Tên công ty', item.company, val => handleArrayChange('experiences', idx, 'company', val))}
                        {renderInput('Vị trí', item.role, val => handleArrayChange('experiences', idx, 'role', val))}
                        {renderInput('Thời gian', item.time, val => handleArrayChange('experiences', idx, 'time', val))}
                        {renderTextarea('Mô tả công việc', item.desc, val => handleArrayChange('experiences', idx, 'desc', val))}
                    </div>
                ))}
                <button onClick={() => addArrayItem('experiences', { company: '', role: '', time: '', desc: '' })} style={{ width: '100%', padding: '8px', background: 'white', border: '1px dashed #0d6efd', color: '#0d6efd', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}>+ Thêm kinh nghiệm</button>
            </div>

            {/* HỌC VẤN */}
            <div style={{ background: '#f8f9fa', padding: '15px', borderRadius: '8px', marginBottom: '20px', border: '1px solid #e9ecef' }}>
                <h4 style={{ marginTop: 0, fontSize: '14px', color: '#333' }}>🎓 Học vấn</h4>
                {resumeData.educations?.map((item, idx) => (
                    <div key={item.id || idx} style={{ background: 'white', padding: '10px', borderRadius: '5px', marginBottom: '10px', border: '1px solid #ddd', position: 'relative' }}>
                        <button onClick={() => removeArrayItem('educations', idx)} style={{ position: 'absolute', top: '5px', right: '5px', background: 'none', border: 'none', color: '#dc3545', cursor: 'pointer' }}>✖</button>
                        {renderInput('Trường', item.school, val => handleArrayChange('educations', idx, 'school', val))}
                        {renderInput('Chuyên ngành', item.major, val => handleArrayChange('educations', idx, 'major', val))}
                        <div style={{ display: 'flex', gap: '10px' }}>
                            <div style={{ flex: 1 }}>{renderInput('Thời gian', item.year, val => handleArrayChange('educations', idx, 'year', val))}</div>
                            <div style={{ flex: 1 }}>{renderInput('Xếp loại', item.gradType, val => handleArrayChange('educations', idx, 'gradType', val))}</div>
                        </div>
                    </div>
                ))}
                <button onClick={() => addArrayItem('educations', { school: '', major: '', year: '', gradType: '' })} style={{ width: '100%', padding: '8px', background: 'white', border: '1px dashed #0d6efd', color: '#0d6efd', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}>+ Thêm học vấn</button>
            </div>

            {/* DỰ ÁN NỔI BẬT */}
            <div style={{ background: '#f8f9fa', padding: '15px', borderRadius: '8px', marginBottom: '20px', border: '1px solid #e9ecef' }}>
                <h4 style={{ marginTop: 0, fontSize: '14px', color: '#333' }}>🚀 Dự án nổi bật</h4>
                {resumeData.projects?.map((item, idx) => (
                    <div key={item.id || idx} style={{ background: 'white', padding: '10px', borderRadius: '5px', marginBottom: '10px', border: '1px solid #ddd', position: 'relative' }}>
                        <button onClick={() => removeArrayItem('projects', idx)} style={{ position: 'absolute', top: '5px', right: '5px', background: 'none', border: 'none', color: '#dc3545', cursor: 'pointer' }}>✖</button>
                        {renderInput('Tên dự án', item.name, val => handleArrayChange('projects', idx, 'name', val))}
                        {renderInput('Vai trò', item.role, val => handleArrayChange('projects', idx, 'role', val))}
                        {renderInput('Thời gian', item.time, val => handleArrayChange('projects', idx, 'time', val))}
                        {renderTextarea('Mô tả dự án', item.desc, val => handleArrayChange('projects', idx, 'desc', val))}
                    </div>
                ))}
                <button onClick={() => addArrayItem('projects', { name: '', role: '', time: '', desc: '' })} style={{ width: '100%', padding: '8px', background: 'white', border: '1px dashed #0d6efd', color: '#0d6efd', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}>+ Thêm dự án</button>
            </div>

            {/* KỸ NĂNG IT */}
            <div style={{ background: '#f8f9fa', padding: '15px', borderRadius: '8px', marginBottom: '20px', border: '1px solid #e9ecef' }}>
                <h4 style={{ marginTop: 0, fontSize: '14px', color: '#333' }}>💻 Tin học / Kỹ năng IT</h4>
                {resumeData.skills?.map((item, idx) => (
                    <div key={item.id || idx} style={{ display: 'flex', gap: '10px', marginBottom: '5px', position: 'relative' }}>
                        <input type="text" value={item.name || ''} onChange={e => handleArrayChange('skills', idx, 'name', e.target.value)} placeholder="Tên kỹ năng" style={{ flex: 2, padding: '6px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '13px' }} />
                        <input type="text" value={item.level || ''} onChange={e => handleArrayChange('skills', idx, 'level', e.target.value)} placeholder="Mức độ" style={{ flex: 1, padding: '6px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '13px' }} />
                        <button onClick={() => removeArrayItem('skills', idx)} style={{ background: 'none', border: 'none', color: '#dc3545', cursor: 'pointer' }}>✖</button>
                    </div>
                ))}
                <button onClick={() => addArrayItem('skills', { name: '', level: '' })} style={{ width: '100%', padding: '8px', marginTop: '10px', background: 'white', border: '1px dashed #0d6efd', color: '#0d6efd', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}>+ Thêm kỹ năng</button>
            </div>

            {/* KỸ NĂNG KHÁC */}
            <div style={{ background: '#f8f9fa', padding: '15px', borderRadius: '8px', marginBottom: '20px', border: '1px solid #e9ecef' }}>
                <h4 style={{ marginTop: 0, fontSize: '14px', color: '#333' }}>🌟 Kỹ năng khác (Mềm)</h4>
                {resumeData.otherSkills?.map((item, idx) => (
                    <div key={item.id || idx} style={{ display: 'flex', gap: '10px', marginBottom: '5px', position: 'relative' }}>
                        <input type="text" value={item.name || ''} onChange={e => handleArrayChange('otherSkills', idx, 'name', e.target.value)} placeholder="Tên kỹ năng" style={{ flex: 2, padding: '6px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '13px' }} />
                        <input type="text" value={item.level || ''} onChange={e => handleArrayChange('otherSkills', idx, 'level', e.target.value)} placeholder="Mức độ" style={{ flex: 1, padding: '6px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '13px' }} />
                        <button onClick={() => removeArrayItem('otherSkills', idx)} style={{ background: 'none', border: 'none', color: '#dc3545', cursor: 'pointer' }}>✖</button>
                    </div>
                ))}
                <button onClick={() => addArrayItem('otherSkills', { name: '', level: '' })} style={{ width: '100%', padding: '8px', marginTop: '10px', background: 'white', border: '1px dashed #0d6efd', color: '#0d6efd', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}>+ Thêm kỹ năng mềm</button>
            </div>

            {/* SỞ THÍCH & CHỨNG CHỈ */}
            <div style={{ display: 'flex', gap: '10px', marginBottom: '20px' }}>
                <div style={{ flex: 1, background: '#f8f9fa', padding: '15px', borderRadius: '8px', border: '1px solid #e9ecef' }}>
                    <h4 style={{ marginTop: 0, fontSize: '14px', color: '#333' }}>🏅 Chứng chỉ</h4>
                    {resumeData.certifications?.map((item, idx) => (
                        <div key={item.id || idx} style={{ display: 'flex', gap: '5px', marginBottom: '5px' }}>
                            <input type="text" value={item.year || ''} onChange={e => handleArrayChange('certifications', idx, 'year', e.target.value)} placeholder="Năm" style={{ width: '60px', padding: '6px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '13px' }} />
                            <input type="text" value={item.name || ''} onChange={e => handleArrayChange('certifications', idx, 'name', e.target.value)} placeholder="Tên CC" style={{ flex: 1, padding: '6px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '13px' }} />
                            <button onClick={() => removeArrayItem('certifications', idx)} style={{ background: 'none', border: 'none', color: '#dc3545', cursor: 'pointer' }}>✖</button>
                        </div>
                    ))}
                    <button onClick={() => addArrayItem('certifications', { year: '', name: '' })} style={{ width: '100%', padding: '8px', marginTop: '5px', background: 'white', border: '1px dashed #0d6efd', color: '#0d6efd', borderRadius: '4px', cursor: 'pointer', fontSize: '12px' }}>+ Chứng chỉ</button>
                </div>

                <div style={{ flex: 1, background: '#f8f9fa', padding: '15px', borderRadius: '8px', border: '1px solid #e9ecef' }}>
                    <h4 style={{ marginTop: 0, fontSize: '14px', color: '#333' }}>❤️ Sở thích</h4>
                    {resumeData.hobbies?.map((item, idx) => (
                        <div key={item.id || idx} style={{ display: 'flex', gap: '5px', marginBottom: '5px' }}>
                            <input type="text" value={item.name || ''} onChange={e => handleArrayChange('hobbies', idx, 'name', e.target.value)} placeholder="Sở thích" style={{ flex: 1, padding: '6px', border: '1px solid #ccc', borderRadius: '4px', fontSize: '13px' }} />
                            <button onClick={() => removeArrayItem('hobbies', idx)} style={{ background: 'none', border: 'none', color: '#dc3545', cursor: 'pointer' }}>✖</button>
                        </div>
                    ))}
                    <button onClick={() => addArrayItem('hobbies', { name: '' })} style={{ width: '100%', padding: '8px', marginTop: '5px', background: 'white', border: '1px dashed #0d6efd', color: '#0d6efd', borderRadius: '4px', cursor: 'pointer', fontSize: '12px' }}>+ Sở thích</button>
                </div>
            </div>
            
            {/* THAO TÁC */}
            <div style={{ marginTop: '30px', paddingTop: '20px', borderTop: '1px solid #eee' }}>
                <button style={{ background: '#198754', color: 'white', padding: '12px 20px', border: 'none', borderRadius: '4px', cursor: 'pointer', width: '100%', fontWeight: 'bold', fontSize: '16px' }}>
                    <i className="fas fa-save" style={{ marginRight: '8px' }}></i> LƯU CV VÀO HỆ THỐNG
                </button>
            </div>
        </div>
    );
};

export default LeftForm;
