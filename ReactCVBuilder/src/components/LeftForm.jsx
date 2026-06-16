import React, { useState, useRef, useEffect } from 'react';

const LeftForm = ({ resumeData, setResumeData }) => {
  const [activeTab, setActiveTab] = useState('basic');
  const [cropModalOpen, setCropModalOpen] = useState(false);
  const [imageToCrop, setImageToCrop] = useState(null);
  const imgRef = useRef(null);
  const cropperRef = useRef(null);

  // Initialize cropper after modal opens and layout stabilizes
  useEffect(() => {
    let timer;
    if (cropModalOpen && imageToCrop) {
      timer = setTimeout(() => {
        if (imgRef.current && window.Cropper) {
          if (cropperRef.current) cropperRef.current.destroy();
          cropperRef.current = new window.Cropper(imgRef.current, {
            aspectRatio: 1,
            viewMode: 1,
            dragMode: 'move',
            autoCropArea: 1,
          });
        }
      }, 100);
    }
    return () => {
      clearTimeout(timer);
      if (cropperRef.current) {
        cropperRef.current.destroy();
        cropperRef.current = null;
      }
    };
  }, [cropModalOpen, imageToCrop]);

  const handleCropApply = () => {
    if (cropperRef.current) {
      const canvas = cropperRef.current.getCroppedCanvas({ width: 400, height: 400 });
      setResumeData({ ...resumeData, avatarUrl: canvas.toDataURL('image/jpeg', 0.9) });
      setCropModalOpen(false);
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setResumeData({ ...resumeData, [name]: value });
  };

  const handleArrayChange = (category, index, field, value) => {
    const newArray = [...(resumeData[category] || [])];
    newArray[index] = { ...newArray[index], [field]: value };
    setResumeData({ ...resumeData, [category]: newArray });
  };

  const addItem = (category, emptyItem) => {
    const currentArray = resumeData[category] || [];
    setResumeData({ ...resumeData, [category]: [...currentArray, emptyItem] });
  };

  const removeItem = (category, index) => {
    const newArray = [...(resumeData[category] || [])];
    newArray.splice(index, 1);
    setResumeData({ ...resumeData, [category]: newArray });
  };

  const handleAvatarUpload = (e) => {
    const file = e.target.files[0];
    const inputElement = e.target;
    if (file) {
      const reader = new FileReader();
      reader.onload = (event) => {
        setImageToCrop(event.target.result);
        setCropModalOpen(true);
        inputElement.value = null; // reset input safely
      };
      reader.readAsDataURL(file);
    }
  };

  const removeAvatar = () => {
    setResumeData({ ...resumeData, avatarUrl: '' });
  };

  // ----- STYLES -----
  const styles = {
    container: {
      fontFamily: '"Inter", "Segoe UI", Roboto, sans-serif',
      display: 'flex',
      flexDirection: 'column',
      height: '100%',
      backgroundColor: '#f8fafc',
    },
    tabsContainer: {
      display: 'flex',
      padding: '8px',
      backgroundColor: '#f1f5f9',
      borderRadius: '12px',
      marginBottom: '20px',
      gap: '4px',
      border: '1px solid #e2e8f0'
    },
    tabBtn: (isActive) => ({
      flex: 1,
      padding: '10px 0',
      border: 'none',
      borderRadius: '8px',
      backgroundColor: isActive ? 'white' : 'transparent',
      color: isActive ? '#2563eb' : '#64748b',
      fontWeight: isActive ? '700' : '500',
      fontSize: '13px',
      cursor: 'pointer',
      boxShadow: isActive ? '0 2px 4px rgba(0,0,0,0.05)' : 'none',
      transition: 'all 0.2s',
      outline: 'none'
    }),
    contentArea: {
      flex: 1,
      overflowY: 'auto',
      paddingRight: '4px'
    },
    section: {
      backgroundColor: 'white',
      padding: '20px',
      borderRadius: '16px',
      marginBottom: '20px',
      border: '1px solid #e2e8f0',
      boxShadow: '0 1px 3px rgba(0,0,0,0.05)'
    },
    sectionTitle: {
      fontWeight: '800',
      color: '#1e293b',
      textTransform: 'uppercase',
      fontSize: '12px',
      letterSpacing: '0.5px',
      marginBottom: '16px',
      display: 'flex',
      alignItems: 'center',
      gap: '8px'
    },
    formGroup: {
      marginBottom: '16px'
    },
    row: {
      display: 'flex',
      gap: '12px',
      marginBottom: '16px'
    },
    col: {
      flex: 1
    },
    label: {
      display: 'block',
      fontWeight: '700',
      marginBottom: '6px',
      fontSize: '12px',
      color: '#475569'
    },
    input: {
      width: '100%',
      padding: '10px 14px',
      border: '1px solid #cbd5e1',
      borderRadius: '8px',
      boxSizing: 'border-box',
      fontSize: '13px',
      color: '#0f172a',
      backgroundColor: '#f8fafc',
      transition: 'all 0.2s',
      outline: 'none',
      ':focus': {
        borderColor: '#3b82f6',
        backgroundColor: 'white',
        boxShadow: '0 0 0 3px rgba(59,130,246,0.1)'
      }
    },
    textarea: {
      minHeight: '80px',
      resize: 'vertical',
      lineHeight: '1.5'
    },
    itemBox: {
      position: 'relative',
      backgroundColor: '#f8fafc',
      padding: '16px',
      borderRadius: '12px',
      marginBottom: '12px',
      border: '1px solid #e2e8f0',
      transition: 'all 0.2s'
    },
    removeBtn: {
      position: 'absolute',
      top: '8px',
      right: '8px',
      color: '#ef4444',
      cursor: 'pointer',
      background: 'white',
      border: '1px solid #fee2e2',
      borderRadius: '50%',
      width: '24px',
      height: '24px',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      fontWeight: 'bold',
      fontSize: '12px',
      boxShadow: '0 1px 2px rgba(0,0,0,0.05)'
    },
    addBtn: {
      width: '100%',
      padding: '10px',
      backgroundColor: 'transparent',
      color: '#3b82f6',
      border: '1px dashed #93c5fd',
      borderRadius: '8px',
      cursor: 'pointer',
      fontWeight: '700',
      fontSize: '12px',
      textTransform: 'uppercase',
      letterSpacing: '0.5px',
      marginTop: '8px',
      transition: 'all 0.2s'
    }
  };

  return (
    <div style={styles.container}>
      
      {/* TABS */}
      <div style={styles.tabsContainer}>
        <button style={styles.tabBtn(activeTab === 'basic')} onClick={() => setActiveTab('basic')}>
          👤 Cá nhân
        </button>
        <button style={styles.tabBtn(activeTab === 'main')} onClick={() => setActiveTab('main')}>
          💼 Nội dung chính
        </button>
        <button style={styles.tabBtn(activeTab === 'skills')} onClick={() => setActiveTab('skills')}>
          🚀 Kỹ năng & Khác
        </button>
      </div>

      <div style={styles.contentArea}>
        
        {/* TAB 1: CÁ NHÂN */}
        {activeTab === 'basic' && (
          <div style={{ animation: 'fadeIn 0.3s' }}>
            <div style={styles.section}>
              <h4 style={styles.sectionTitle}>Thông tin cá nhân</h4>
              
              <div style={{ display: 'flex', alignItems: 'center', gap: '15px', marginBottom: '20px', padding: '15px', backgroundColor: '#eff6ff', borderRadius: '12px', border: '1px dashed #bfdbfe' }}>
                <div style={{ width: '64px', height: '64px', borderRadius: '50%', backgroundColor: 'white', border: '2px solid #dbeafe', display: 'flex', alignItems: 'center', justifyContent: 'center', overflow: 'hidden', flexShrink: 0 }}>
                  {resumeData.avatarUrl ? (
                    <img src={resumeData.avatarUrl} alt="Avatar" style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
                  ) : (
                    <span style={{ fontSize: '24px', color: '#93c5fd' }}>👤</span>
                  )}
                </div>
                <div style={{ flex: 1, display: 'flex', flexDirection: 'column', gap: '8px' }}>
                  <label style={{ ...styles.label, marginBottom: 0, color: '#1d4ed8' }}>ẢNH ĐẠI DIỆN</label>
                  <div style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
                  <label style={{ cursor: 'pointer', backgroundColor: '#2563eb', color: 'white', padding: '6px 14px', borderRadius: '20px', fontSize: '11px', fontWeight: 'bold' }}>
                      Chọn ảnh
                      <input type="file" accept="image/*" onChange={handleAvatarUpload} style={{ display: 'none' }} />
                    </label>
                    {resumeData.avatarUrl && (
                      <button onClick={removeAvatar} style={{ cursor: 'pointer', background: 'transparent', border: '1px solid #fecaca', color: '#ef4444', padding: '5px 10px', borderRadius: '20px', fontSize: '11px', fontWeight: 'bold' }}>Xóa ảnh</button>
                    )}
                  </div>
                </div>
              </div>

              <div style={styles.row}>
                <div style={styles.col}>
                  <label style={styles.label}>Họ và tên</label>
                  <input type="text" name="fullName" value={resumeData.fullName || ''} onChange={handleChange} style={styles.input} placeholder="Họ và tên" />
                </div>
                <div style={styles.col}>
                  <label style={styles.label}>Vị trí ứng tuyển</label>
                  <input type="text" name="jobTitle" value={resumeData.jobTitle || ''} onChange={handleChange} style={styles.input} placeholder="Ví dụ: Backend Developer" />
                </div>
              </div>

              <div style={styles.row}>
                <div style={styles.col}>
                  <label style={styles.label}>Email</label>
                  <input type="text" name="email" value={resumeData.email || ''} onChange={handleChange} style={styles.input} placeholder="email@example.com" />
                </div>
                <div style={styles.col}>
                  <label style={styles.label}>Số điện thoại</label>
                  <input type="text" name="phone" value={resumeData.phone || ''} onChange={handleChange} style={styles.input} placeholder="0123.456.789" />
                </div>
              </div>

              <div style={styles.row}>
                <div style={styles.col}>
                  <label style={styles.label}>Ngày sinh</label>
                  <input type="text" name="birthDate" value={resumeData.birthDate || ''} onChange={handleChange} style={styles.input} placeholder="27/01/1998" />
                </div>
                <div style={styles.col}>
                  <label style={styles.label}>Địa chỉ</label>
                  <input type="text" name="address" value={resumeData.address || ''} onChange={handleChange} style={styles.input} placeholder="Quận 10, TP.HCM" />
                </div>
              </div>

              <div style={styles.formGroup}>
                <label style={styles.label}>Link liên kết (Website, Portfolio, LinkedIn...)</label>
                <input type="text" name="website" value={resumeData.website || ''} onChange={handleChange} style={styles.input} placeholder="https://..." />
              </div>
            </div>

            <div style={styles.section}>
              <h4 style={styles.sectionTitle}>Mục tiêu nghề nghiệp</h4>
              <textarea name="summary" value={resumeData.summary || ''} onChange={handleChange} style={{...styles.input, ...styles.textarea}} placeholder="Tôi là một người đam mê..." />
            </div>
          </div>
        )}

        {/* TAB 2: NỘI DUNG CHÍNH */}
        {activeTab === 'main' && (
          <div style={{ animation: 'fadeIn 0.3s' }}>
            <div style={styles.section}>
              <h4 style={styles.sectionTitle}>Kinh nghiệm làm việc</h4>
              {resumeData.experiences?.map((item, idx) => (
                <div key={idx} style={styles.itemBox}>
                  <button style={styles.removeBtn} onClick={() => removeItem('experiences', idx)}>✕</button>
                  <input type="text" value={item.company || ''} onChange={(e) => handleArrayChange('experiences', idx, 'company', e.target.value)} style={{...styles.input, marginBottom: '8px', fontWeight: 'bold'}} placeholder="Công ty (Larana Studios)" />
                  <div style={styles.row}>
                    <input type="text" value={item.role || ''} onChange={(e) => handleArrayChange('experiences', idx, 'role', e.target.value)} style={{...styles.input, flex: 1}} placeholder="Chức vụ (Marketing Manager)" />
                    <input type="text" value={item.time || ''} onChange={(e) => handleArrayChange('experiences', idx, 'time', e.target.value)} style={{...styles.input, flex: 1}} placeholder="Thời gian (Jun 2019 - Jan 2020)" />
                  </div>
                  <textarea value={item.desc || ''} onChange={(e) => handleArrayChange('experiences', idx, 'desc', e.target.value)} style={{...styles.input, ...styles.textarea}} placeholder="Mô tả công việc (Dùng dấu • để liệt kê)..." />
                </div>
              ))}
              <button style={styles.addBtn} onClick={() => addItem('experiences', { company: '', role: '', time: '', desc: '' })}>+ Thêm kinh nghiệm</button>
            </div>

            <div style={styles.section}>
              <h4 style={styles.sectionTitle}>Học vấn</h4>
              {resumeData.educations?.map((item, idx) => (
                <div key={idx} style={styles.itemBox}>
                  <button style={styles.removeBtn} onClick={() => removeItem('educations', idx)}>✕</button>
                  <input type="text" value={item.school || ''} onChange={(e) => handleArrayChange('educations', idx, 'school', e.target.value)} style={{...styles.input, marginBottom: '8px', fontWeight: 'bold'}} placeholder="Tên trường" />
                  <input type="text" value={item.major || ''} onChange={(e) => handleArrayChange('educations', idx, 'major', e.target.value)} style={{...styles.input, marginBottom: '8px'}} placeholder="Ngành học" />
                  <div style={styles.row}>
                    <input type="text" value={item.year || ''} onChange={(e) => handleArrayChange('educations', idx, 'year', e.target.value)} style={{...styles.input, flex: 1}} placeholder="Thời gian" />
                    <input type="text" value={item.gradType || ''} onChange={(e) => handleArrayChange('educations', idx, 'gradType', e.target.value)} style={{...styles.input, flex: 1}} placeholder="Xếp loại" />
                  </div>
                </div>
              ))}
              <button style={styles.addBtn} onClick={() => addItem('educations', { school: '', major: '', year: '', gradType: '' })}>+ Thêm học vấn</button>
            </div>

            <div style={styles.section}>
              <h4 style={styles.sectionTitle}>Dự án</h4>
              {resumeData.projects?.map((item, idx) => (
                <div key={idx} style={styles.itemBox}>
                  <button style={styles.removeBtn} onClick={() => removeItem('projects', idx)}>✕</button>
                  <input type="text" value={item.name || ''} onChange={(e) => handleArrayChange('projects', idx, 'name', e.target.value)} style={{...styles.input, marginBottom: '8px', fontWeight: 'bold'}} placeholder="Tên dự án" />
                  <div style={styles.row}>
                    <input type="text" value={item.role || ''} onChange={(e) => handleArrayChange('projects', idx, 'role', e.target.value)} style={{...styles.input, flex: 1}} placeholder="Vai trò" />
                    <input type="text" value={item.time || ''} onChange={(e) => handleArrayChange('projects', idx, 'time', e.target.value)} style={{...styles.input, flex: 1}} placeholder="Thời gian" />
                  </div>
                  <textarea value={item.desc || ''} onChange={(e) => handleArrayChange('projects', idx, 'desc', e.target.value)} style={{...styles.input, ...styles.textarea}} placeholder="Mô tả và công nghệ..." />
                </div>
              ))}
              <button style={styles.addBtn} onClick={() => addItem('projects', { name: '', role: '', time: '', desc: '' })}>+ Thêm dự án</button>
            </div>

            <div style={styles.section}>
              <h4 style={styles.sectionTitle}>Hoạt động</h4>
              {resumeData.activities?.map((item, idx) => (
                <div key={idx} style={styles.itemBox}>
                  <button style={styles.removeBtn} onClick={() => removeItem('activities', idx)}>✕</button>
                  <input type="text" value={item.name || ''} onChange={(e) => handleArrayChange('activities', idx, 'name', e.target.value)} style={{...styles.input, marginBottom: '8px', fontWeight: 'bold'}} placeholder="Tên tổ chức/CLB" />
                  <input type="text" value={item.time || ''} onChange={(e) => handleArrayChange('activities', idx, 'time', e.target.value)} style={{...styles.input, marginBottom: '8px'}} placeholder="Thời gian" />
                  <textarea value={item.desc || ''} onChange={(e) => handleArrayChange('activities', idx, 'desc', e.target.value)} style={{...styles.input, ...styles.textarea}} placeholder="Mô tả hoạt động..." />
                </div>
              ))}
              <button style={styles.addBtn} onClick={() => addItem('activities', { name: '', time: '', desc: '' })}>+ Thêm hoạt động</button>
            </div>
          </div>
        )}

        {/* TAB 3: KỸ NĂNG & KHÁC */}
        {activeTab === 'skills' && (
          <div style={{ animation: 'fadeIn 0.3s' }}>
            
            <div style={styles.section}>
              <h4 style={styles.sectionTitle}>Tin học / Kỹ năng cứng</h4>
              {resumeData.skills?.map((item, idx) => (
                <div key={idx} style={styles.itemBox}>
                  <button style={styles.removeBtn} onClick={() => removeItem('skills', idx)}>✕</button>
                  <div style={{ display: 'flex', gap: '8px' }}>
                    <input type="text" value={item.name || ''} onChange={(e) => handleArrayChange('skills', idx, 'name', e.target.value)} style={{...styles.input, flex: 2}} placeholder="Tin học (Excel, Word...)" />
                    <input type="text" value={item.level || ''} onChange={(e) => handleArrayChange('skills', idx, 'level', e.target.value)} style={{...styles.input, flex: 1}} placeholder="Mức độ" />
                  </div>
                </div>
              ))}
              <button style={styles.addBtn} onClick={() => addItem('skills', { name: '', level: '' })}>+ Thêm kỹ năng</button>
            </div>

            <div style={styles.section}>
              <h4 style={styles.sectionTitle}>Kỹ năng mềm / Khác</h4>
              {resumeData.otherSkills?.map((item, idx) => (
                <div key={idx} style={styles.itemBox}>
                  <button style={styles.removeBtn} onClick={() => removeItem('otherSkills', idx)}>✕</button>
                  <div style={{ display: 'flex', gap: '8px' }}>
                    <input type="text" value={item.name || ''} onChange={(e) => handleArrayChange('otherSkills', idx, 'name', e.target.value)} style={{...styles.input, flex: 2}} placeholder="Kỹ năng (Giao tiếp...)" />
                    <input type="text" value={item.level || ''} onChange={(e) => handleArrayChange('otherSkills', idx, 'level', e.target.value)} style={{...styles.input, flex: 1}} placeholder="Mức độ" />
                  </div>
                </div>
              ))}
              <button style={styles.addBtn} onClick={() => addItem('otherSkills', { name: '', level: '' })}>+ Thêm kỹ năng khác</button>
            </div>

            <div style={styles.section}>
              <h4 style={styles.sectionTitle}>Ngoại ngữ</h4>
              {resumeData.languages?.map((item, idx) => (
                <div key={idx} style={styles.itemBox}>
                  <button style={styles.removeBtn} onClick={() => removeItem('languages', idx)}>✕</button>
                  <div style={{ display: 'flex', gap: '8px' }}>
                    <input type="text" value={item.name || ''} onChange={(e) => handleArrayChange('languages', idx, 'name', e.target.value)} style={{...styles.input, flex: 2}} placeholder="Ngoại ngữ (Tiếng Anh...)" />
                    <input type="text" value={item.level || ''} onChange={(e) => handleArrayChange('languages', idx, 'level', e.target.value)} style={{...styles.input, flex: 1}} placeholder="Mức độ" />
                  </div>
                </div>
              ))}
              <button style={styles.addBtn} onClick={() => addItem('languages', { name: '', level: '' })}>+ Thêm ngoại ngữ</button>
            </div>

            <div style={styles.section}>
              <h4 style={styles.sectionTitle}>Chứng chỉ</h4>
              {resumeData.certifications?.map((item, idx) => (
                <div key={idx} style={styles.itemBox}>
                  <button style={styles.removeBtn} onClick={() => removeItem('certifications', idx)}>✕</button>
                  <input type="text" value={item.year || ''} onChange={(e) => handleArrayChange('certifications', idx, 'year', e.target.value)} style={{...styles.input, marginBottom: '8px'}} placeholder="Năm (2019)" />
                  <input type="text" value={item.name || ''} onChange={(e) => handleArrayChange('certifications', idx, 'name', e.target.value)} style={styles.input} placeholder="Tên chứng chỉ (IELTS 7.0)" />
                </div>
              ))}
              <button style={styles.addBtn} onClick={() => addItem('certifications', { year: '', name: '' })}>+ Thêm chứng chỉ</button>
            </div>

            <div style={styles.section}>
              <h4 style={styles.sectionTitle}>Giải thưởng</h4>
              {resumeData.awards?.map((item, idx) => (
                <div key={idx} style={styles.itemBox}>
                  <button style={styles.removeBtn} onClick={() => removeItem('awards', idx)}>✕</button>
                  <input type="text" value={item.name || ''} onChange={(e) => handleArrayChange('awards', idx, 'name', e.target.value)} style={styles.input} placeholder="Tên giải thưởng (Sinh viên 5 tốt...)" />
                </div>
              ))}
              <button style={styles.addBtn} onClick={() => addItem('awards', { name: '' })}>+ Thêm giải thưởng</button>
            </div>

            <div style={styles.section}>
              <h4 style={styles.sectionTitle}>Sở thích</h4>
              {resumeData.hobbies?.map((item, idx) => (
                <div key={idx} style={styles.itemBox}>
                  <button style={styles.removeBtn} onClick={() => removeItem('hobbies', idx)}>✕</button>
                  <input type="text" value={item.name || ''} onChange={(e) => handleArrayChange('hobbies', idx, 'name', e.target.value)} style={styles.input} placeholder="Sở thích (Đọc sách, Nghe nhạc...)" />
                </div>
              ))}
              <button style={styles.addBtn} onClick={() => addItem('hobbies', { name: '' })}>+ Thêm sở thích</button>
            </div>

            <div style={styles.section}>
              <h4 style={styles.sectionTitle}>Người tham chiếu</h4>
              {resumeData.references?.map((item, idx) => (
                <div key={idx} style={styles.itemBox}>
                  <button style={styles.removeBtn} onClick={() => removeItem('references', idx)}>✕</button>
                  <input type="text" value={item.info || ''} onChange={(e) => handleArrayChange('references', idx, 'info', e.target.value)} style={styles.input} placeholder="Họ tên, chức vụ, SĐT người tham chiếu" />
                </div>
              ))}
              <button style={styles.addBtn} onClick={() => addItem('references', { info: '' })}>+ Thêm người tham chiếu</button>
            </div>

          </div>
        )}
      </div>

      <style>{`
        @keyframes fadeIn {
          from { opacity: 0; transform: translateY(5px); }
          to { opacity: 1; transform: translateY(0); }
        }
        input:focus, textarea:focus {
          border-color: #3b82f6 !important;
          background-color: #ffffff !important;
          box-shadow: 0 0 0 3px rgba(59,130,246,0.1) !important;
        }
        button:hover {
          opacity: 0.9;
        }
        ::-webkit-scrollbar {
          width: 6px;
        }
        ::-webkit-scrollbar-track {
          background: transparent;
        }
        ::-webkit-scrollbar-thumb {
          background: #cbd5e1;
          border-radius: 4px;
        }
        ::-webkit-scrollbar-thumb:hover {
          background: #94a3b8;
        }
      `}</style>

      {/* MODAL CẮT ẢNH */}
      {cropModalOpen && (
        <div style={{ position: 'fixed', top: '-25vh', left: '-25vw', width: '150vw', height: '150vh', backgroundColor: 'rgba(0,0,0,0.85)', zIndex: 99999, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          <div style={{ backgroundColor: 'white', borderRadius: '16px', width: '500px', maxWidth: '95%', overflow: 'hidden', display: 'flex', flexDirection: 'column', boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.25)' }}>
            <div style={{ padding: '20px', borderBottom: '1px solid #e2e8f0', fontWeight: 'bold', display: 'flex', justifyContent: 'space-between', backgroundColor: '#f8fafc' }}>
              <span style={{ fontSize: '18px', color: '#0f172a' }}>Cắt ảnh đại diện</span>
              <button onClick={() => setCropModalOpen(false)} style={{ background: 'none', border: 'none', fontSize: '24px', cursor: 'pointer', lineHeight: '1', color: '#64748b' }}>&times;</button>
            </div>
            <div style={{ padding: '0', backgroundColor: '#000', display: 'flex', justifyContent: 'center', alignItems: 'center', height: '400px' }}>
                 <img 
                   ref={imgRef} 
                   src={imageToCrop} 
                   style={{ maxWidth: '100%', maxHeight: '100%', display: 'block' }} 
                   alt="To crop" 
                 />
            </div>
            <div style={{ padding: '20px', display: 'flex', justifyContent: 'flex-end', gap: '12px', backgroundColor: '#f8fafc', borderTop: '1px solid #e2e8f0' }}>
              <button onClick={() => setCropModalOpen(false)} style={{ padding: '10px 20px', borderRadius: '8px', border: '1px solid #cbd5e1', background: 'white', cursor: 'pointer', fontWeight: 'bold', color: '#475569' }}>Hủy bỏ</button>
              <button onClick={handleCropApply} style={{ padding: '10px 24px', borderRadius: '8px', border: 'none', background: '#2563eb', color: 'white', fontWeight: 'bold', cursor: 'pointer', boxShadow: '0 4px 6px -1px rgba(37, 99, 235, 0.2)' }}>Cắt và Áp dụng</button>
            </div>
          </div>
        </div>
      )}

    </div>
  );
};

export default LeftForm;
