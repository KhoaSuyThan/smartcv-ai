import React, { useState, useRef, useEffect } from 'react';

const LeftForm = ({ resumeData, setResumeData }) => {
  const [activeTab, setActiveTab] = useState('basic');
  const [cropModalOpen, setCropModalOpen] = useState(false);
  const [imageToCrop, setImageToCrop] = useState(null);
  const imgRef = useRef(null);
  const cropperRef = useRef(null);
  const [isAIProcessing, setIsAIProcessing] = useState({});

  const callAIService = async (type, content, context = '') => {
    try {
      const response = await fetch('/AI/ProcessText', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({ type, content, context })
      });
      const result = await response.json();
      if (result.success) return result.data;
      else {
        alert(result.data || 'Lỗi xử lý AI');
        return null;
      }
    } catch (e) {
      console.error('AI Error:', e);
      alert('Không thể kết nối với máy chủ AI.');
      return null;
    }
  };

  const generateAISummary = async () => {
    const jobTitle = resumeData.jobTitle || '';
    let currentSummary = (resumeData.summary || '').replace(/<[^>]*>/g, '').trim();
    
    if (!currentSummary && !jobTitle) {
      alert("Vui lòng nhập Vị trí ứng tuyển hoặc một vài ý chính để AI có thể hỗ trợ!");
      return;
    }

    if (!currentSummary) {
      currentSummary = `Tôi đang ứng tuyển vị trí ${jobTitle}`;
    }

    setIsAIProcessing(prev => ({ ...prev, summary: true }));
    const result = await callAIService('summary', currentSummary, jobTitle || 'Nhân viên');
    if (result) {
      setResumeData(prev => ({ ...prev, summary: result }));
    }
    setIsAIProcessing(prev => ({ ...prev, summary: false }));
  };

  const generateAISkills = async () => {
    const jobTitle = (resumeData.jobTitle || '').trim();
    if (!jobTitle) {
      alert("Vui lòng nhập Vị trí ứng tuyển để AI gợi ý kỹ năng phù hợp!");
      return;
    }

    setIsAIProcessing(prev => ({ ...prev, skills: true }));
    const result = await callAIService('suggest_skills', jobTitle, jobTitle);
    if (result) {
      const skillNames = result.split(',').map(s => s.trim()).filter(s => s);
      const currentSkills = [...(resumeData.skills || [])];
      skillNames.forEach(name => {
        if (!currentSkills.some(s => s.name?.toLowerCase() === name.toLowerCase())) {
          currentSkills.push({ name: name, level: 'Thành thạo' });
        }
      });
      setResumeData(prev => ({ ...prev, skills: currentSkills }));
    }
    setIsAIProcessing(prev => ({ ...prev, skills: false }));
  };

  const improveAIDesc = async (category, idx) => {
    const item = resumeData[category]?.[idx];
    if (!item) return;
    const jobTitle = resumeData.jobTitle || 'Nhân viên';
    let content = (item.desc || '').replace(/<[^>]*>/g, '').trim();

    if (!content) {
      if (category === 'experiences') {
        content = item.company ? `Làm việc tại ${item.company}` : '';
      } else if (category === 'projects') {
        content = item.name ? `Dự án ${item.name}` : '';
      } else if (category === 'activities') {
        content = item.name ? `Hoạt động tại ${item.name}` : '';
      }
    }

    if (!content) {
      alert("Vui lòng nhập một vài ý chính mô tả để AI có thể hỗ trợ!");
      return;
    }

    const context = category === 'experiences' ? (item.role || jobTitle) : (category === 'projects' ? (item.role || jobTitle) : jobTitle);
    const aiType = category === 'experiences' ? 'optimize' : (category === 'projects' ? 'project' : 'activity');

    const key = `${category}_${idx}`;
    setIsAIProcessing(prev => ({ ...prev, [key]: true }));
    const result = await callAIService(aiType, content, context);
    if (result) {
      handleArrayChange(category, idx, 'desc', result);
    }
    setIsAIProcessing(prev => ({ ...prev, [key]: false }));
  };

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

  const renderSectionHeader = (title, key, hasAi = false, aiAction = null, aiProcessingKey = '') => {
    const isVisible = resumeData.visibleSections?.[key] !== false;
    
    return (
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
        <h4 style={{ ...styles.sectionTitle, marginBottom: 0 }}>{title}</h4>
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
          {hasAi && (
            <button
              onClick={aiAction}
              disabled={isAIProcessing[aiProcessingKey]}
              style={styles.aiButton}
            >
              {isAIProcessing[aiProcessingKey] ? '🤖 Đang xử lý...' : '🤖 Gợi ý AI'}
            </button>
          )}
          
          <div 
            onClick={() => {
              setResumeData(prev => ({
                ...prev,
                visibleSections: {
                  ...(prev.visibleSections || {}),
                  [key]: !isVisible
                }
              }));
            }}
            style={{ display: 'inline-flex', alignItems: 'center', cursor: 'pointer', gap: '6px', userSelect: 'none' }} 
            title="Bật/Tắt mục này trên CV"
          >
            <span style={{ fontSize: '10px', fontWeight: 'bold', color: isVisible ? '#3b82f6' : '#64748b' }}>
              {isVisible ? 'HIỆN' : 'ẨN'}
            </span>
            <div style={{
              width: '32px',
              height: '18px',
              backgroundColor: isVisible ? '#3b82f6' : '#cbd5e1',
              borderRadius: '9px',
              position: 'relative',
              transition: 'background-color 0.2s'
            }}>
              <div style={{
                width: '12px',
                height: '12px',
                backgroundColor: '#fff',
                borderRadius: '50%',
                position: 'absolute',
                top: '3px',
                left: isVisible ? '17px' : '3px',
                transition: 'left 0.2s'
              }} />
            </div>
          </div>
        </div>
      </div>
    );
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
    },
    aiButton: {
      padding: '4px 10px',
      backgroundColor: '#eff6ff',
      color: '#2563eb',
      border: '1px solid #bfdbfe',
      borderRadius: '6px',
      fontSize: '11px',
      fontWeight: 'bold',
      cursor: 'pointer',
      display: 'inline-flex',
      alignItems: 'center',
      gap: '4px',
      transition: 'all 0.2s',
      outline: 'none'
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
              {renderSectionHeader('Mục tiêu nghề nghiệp', 'summary', true, generateAISummary, 'summary')}
              <div style={{ opacity: resumeData.visibleSections?.summary !== false ? 1 : 0.5, transition: 'opacity 0.2s' }}>
                <textarea name="summary" data-field="summary" value={resumeData.summary || ''} onChange={handleChange} style={{...styles.input, ...styles.textarea}} placeholder="Tôi là một người đam mê..." />
              </div>
            </div>
          </div>
        )}

        {/* TAB 2: NỘI DUNG CHÍNH */}
        {activeTab === 'main' && (
          <div style={{ animation: 'fadeIn 0.3s' }}>
            <div style={styles.section}>
              {renderSectionHeader('Kinh nghiệm làm việc', 'experiences')}
              <div style={{ opacity: resumeData.visibleSections?.experiences !== false ? 1 : 0.5, transition: 'opacity 0.2s' }}>
                {resumeData.experiences?.map((item, idx) => (
                  <div key={idx} style={styles.itemBox}>
                    <button style={styles.removeBtn} onClick={() => removeItem('experiences', idx)}>✕</button>
                    <input type="text" value={item.company || ''} onChange={(e) => handleArrayChange('experiences', idx, 'company', e.target.value)} style={{...styles.input, marginBottom: '8px', fontWeight: 'bold'}} placeholder="Công ty (Larana Studios)" />
                    <div style={styles.row}>
                      <input type="text" value={item.role || ''} onChange={(e) => handleArrayChange('experiences', idx, 'role', e.target.value)} style={{...styles.input, flex: 1}} placeholder="Chức vụ (Marketing Manager)" />
                      <input type="text" value={item.time || ''} onChange={(e) => handleArrayChange('experiences', idx, 'time', e.target.value)} style={{...styles.input, flex: 1}} placeholder="Thời gian (Jun 2019 - Jan 2020)" />
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '4px', marginTop: '8px' }}>
                      <label style={{ ...styles.label, marginBottom: 0 }}>Mô tả công việc</label>
                      <button
                        onClick={() => improveAIDesc('experiences', idx)}
                        disabled={isAIProcessing[`experiences_${idx}`]}
                        style={styles.aiButton}
                      >
                        {isAIProcessing[`experiences_${idx}`] ? '🤖 Đang tối ưu...' : '🤖 AI Tối ưu'}
                      </button>
                    </div>
                    <textarea data-field="experiences" data-index={idx} data-subfield="desc" value={item.desc || ''} onChange={(e) => handleArrayChange('experiences', idx, 'desc', e.target.value)} style={{...styles.input, ...styles.textarea}} placeholder="Mô tả công việc (Dùng dấu • để liệt kê)..." />
                  </div>
                ))}
                <button style={styles.addBtn} onClick={() => addItem('experiences', { company: '', role: '', time: '', desc: '' })}>+ Thêm kinh nghiệm</button>
              </div>
            </div>

            <div style={styles.section}>
              {renderSectionHeader('Học vấn', 'educations')}
              <div style={{ opacity: resumeData.visibleSections?.educations !== false ? 1 : 0.5, transition: 'opacity 0.2s' }}>
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
            </div>

            <div style={styles.section}>
              {renderSectionHeader('Dự án', 'projects')}
              <div style={{ opacity: resumeData.visibleSections?.projects !== false ? 1 : 0.5, transition: 'opacity 0.2s' }}>
                {resumeData.projects?.map((item, idx) => (
                  <div key={idx} style={styles.itemBox}>
                    <button style={styles.removeBtn} onClick={() => removeItem('projects', idx)}>✕</button>
                    <input type="text" value={item.name || ''} onChange={(e) => handleArrayChange('projects', idx, 'name', e.target.value)} style={{...styles.input, marginBottom: '8px', fontWeight: 'bold'}} placeholder="Tên dự án" />
                    <div style={styles.row}>
                      <input type="text" value={item.role || ''} onChange={(e) => handleArrayChange('projects', idx, 'role', e.target.value)} style={{...styles.input, flex: 1}} placeholder="Vai trò" />
                      <input type="text" value={item.time || ''} onChange={(e) => handleArrayChange('projects', idx, 'time', e.target.value)} style={{...styles.input, flex: 1}} placeholder="Thời gian" />
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '4px', marginTop: '8px' }}>
                      <label style={{ ...styles.label, marginBottom: 0 }}>Mô tả dự án</label>
                      <button
                        onClick={() => improveAIDesc('projects', idx)}
                        disabled={isAIProcessing[`projects_${idx}`]}
                        style={styles.aiButton}
                      >
                        {isAIProcessing[`projects_${idx}`] ? '🤖 Đang tối ưu...' : '🤖 AI Tối ưu'}
                      </button>
                    </div>
                    <textarea data-field="projects" data-index={idx} data-subfield="desc" value={item.desc || ''} onChange={(e) => handleArrayChange('projects', idx, 'desc', e.target.value)} style={{...styles.input, ...styles.textarea}} placeholder="Mô tả và công nghệ..." />
                  </div>
                ))}
                <button style={styles.addBtn} onClick={() => addItem('projects', { name: '', role: '', time: '', desc: '' })}>+ Thêm dự án</button>
              </div>
            </div>

            <div style={styles.section}>
              {renderSectionHeader('Hoạt động', 'activities')}
              <div style={{ opacity: resumeData.visibleSections?.activities !== false ? 1 : 0.5, transition: 'opacity 0.2s' }}>
                {resumeData.activities?.map((item, idx) => (
                  <div key={idx} style={styles.itemBox}>
                    <button style={styles.removeBtn} onClick={() => removeItem('activities', idx)}>✕</button>
                    <input type="text" value={item.name || ''} onChange={(e) => handleArrayChange('activities', idx, 'name', e.target.value)} style={{...styles.input, marginBottom: '8px', fontWeight: 'bold'}} placeholder="Tên tổ chức/CLB" />
                    <input type="text" value={item.time || ''} onChange={(e) => handleArrayChange('activities', idx, 'time', e.target.value)} style={{...styles.input, marginBottom: '8px'}} placeholder="Thời gian" />
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '4px', marginTop: '8px' }}>
                      <label style={{ ...styles.label, marginBottom: 0 }}>Mô tả hoạt động</label>
                      <button
                        onClick={() => improveAIDesc('activities', idx)}
                        disabled={isAIProcessing[`activities_${idx}`]}
                        style={styles.aiButton}
                      >
                        {isAIProcessing[`activities_${idx}`] ? '🤖 Đang tối ưu...' : '🤖 AI Tối ưu'}
                      </button>
                    </div>
                    <textarea data-field="activities" data-index={idx} data-subfield="desc" value={item.desc || ''} onChange={(e) => handleArrayChange('activities', idx, 'desc', e.target.value)} style={{...styles.input, ...styles.textarea}} placeholder="Mô tả hoạt động..." />
                  </div>
                ))}
                <button style={styles.addBtn} onClick={() => addItem('activities', { name: '', time: '', desc: '' })}>+ Thêm hoạt động</button>
              </div>
            </div>
          </div>
        )}

        {/* TAB 3: KỸ NĂNG & KHÁC */}
        {activeTab === 'skills' && (
          <div style={{ animation: 'fadeIn 0.3s' }}>
            
            <div style={styles.section}>
              {renderSectionHeader('Tin học / Kỹ năng cứng', 'skills', true, generateAISkills, 'skills')}
              <div style={{ opacity: resumeData.visibleSections?.skills !== false ? 1 : 0.5, transition: 'opacity 0.2s' }}>
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
            </div>

            <div style={styles.section}>
              {renderSectionHeader('Kỹ năng mềm / Khác', 'otherSkills')}
              <div style={{ opacity: resumeData.visibleSections?.otherSkills !== false ? 1 : 0.5, transition: 'opacity 0.2s' }}>
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
            </div>

            <div style={styles.section}>
              {renderSectionHeader('Ngoại ngữ', 'languages')}
              <div style={{ opacity: resumeData.visibleSections?.languages !== false ? 1 : 0.5, transition: 'opacity 0.2s' }}>
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
            </div>

            <div style={styles.section}>
              {renderSectionHeader('Chứng chỉ', 'certifications')}
              <div style={{ opacity: resumeData.visibleSections?.certifications !== false ? 1 : 0.5, transition: 'opacity 0.2s' }}>
                {resumeData.certifications?.map((item, idx) => (
                  <div key={idx} style={styles.itemBox}>
                    <button style={styles.removeBtn} onClick={() => removeItem('certifications', idx)}>✕</button>
                    <input type="text" value={item.year || ''} onChange={(e) => handleArrayChange('certifications', idx, 'year', e.target.value)} style={{...styles.input, marginBottom: '8px'}} placeholder="Năm (2019)" />
                    <input type="text" value={item.name || ''} onChange={(e) => handleArrayChange('certifications', idx, 'name', e.target.value)} style={styles.input} placeholder="Tên chứng chỉ (IELTS 7.0)" />
                  </div>
                ))}
                <button style={styles.addBtn} onClick={() => addItem('certifications', { year: '', name: '' })}>+ Thêm chứng chỉ</button>
              </div>
            </div>

            <div style={styles.section}>
              {renderSectionHeader('Giải thưởng', 'awards')}
              <div style={{ opacity: resumeData.visibleSections?.awards !== false ? 1 : 0.5, transition: 'opacity 0.2s' }}>
                {resumeData.awards?.map((item, idx) => (
                  <div key={idx} style={styles.itemBox}>
                    <button style={styles.removeBtn} onClick={() => removeItem('awards', idx)}>✕</button>
                    <input type="text" value={item.name || ''} onChange={(e) => handleArrayChange('awards', idx, 'name', e.target.value)} style={styles.input} placeholder="Tên giải thưởng (Sinh viên 5 tốt...)" />
                  </div>
                ))}
                <button style={styles.addBtn} onClick={() => addItem('awards', { name: '' })}>+ Thêm giải thưởng</button>
              </div>
            </div>

            <div style={styles.section}>
              {renderSectionHeader('Sở thích', 'hobbies')}
              <div style={{ opacity: resumeData.visibleSections?.hobbies !== false ? 1 : 0.5, transition: 'opacity 0.2s' }}>
                {resumeData.hobbies?.map((item, idx) => (
                  <div key={idx} style={styles.itemBox}>
                    <button style={styles.removeBtn} onClick={() => removeItem('hobbies', idx)}>✕</button>
                    <input type="text" value={item.name || ''} onChange={(e) => handleArrayChange('hobbies', idx, 'name', e.target.value)} style={styles.input} placeholder="Sở thích (Đọc sách, Nghe nhạc...)" />
                  </div>
                ))}
                <button style={styles.addBtn} onClick={() => addItem('hobbies', { name: '' })}>+ Thêm sở thích</button>
              </div>
            </div>

            <div style={styles.section}>
              {renderSectionHeader('Người tham chiếu', 'references')}
              <div style={{ opacity: resumeData.visibleSections?.references !== false ? 1 : 0.5, transition: 'opacity 0.2s' }}>
                {resumeData.references?.map((item, idx) => (
                  <div key={idx} style={styles.itemBox}>
                    <button style={styles.removeBtn} onClick={() => removeItem('references', idx)}>✕</button>
                    <input type="text" value={item.info || ''} onChange={(e) => handleArrayChange('references', idx, 'info', e.target.value)} style={styles.input} placeholder="Họ tên, chức vụ, SĐT người tham chiếu" />
                  </div>
                ))}
                <button style={styles.addBtn} onClick={() => addItem('references', { info: '' })}>+ Thêm người tham chiếu</button>
              </div>
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
