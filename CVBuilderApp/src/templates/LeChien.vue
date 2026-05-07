<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, width: '210mm' }"
    @click.self="selectedSectionId = null"
  >
    <div class="cv-elegant-wrapper">

      <div class="header-area">
        <div class="avatar-box">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="avatar-img" alt="avatar"/>
          <div v-else class="avatar-placeholder">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="#ccc" stroke-width="1.5">
              <circle cx="12" cy="8" r="4"/><path d="M4 20c0-4 3.6-7 8-7s8 3 8 7"/>
            </svg>
          </div>
        </div>
        <div class="header-content">
          <h1 class="fullname">{{ isEmpty(resumeData.general.fullName) ? 'HỌ VÀ TÊN ỨNG VIÊN' : resumeData.general.fullName }}</h1>
          <p class="job-title">{{ isEmpty(resumeData.general.jobTitle) ? 'VỊ TRÍ ỨNG TUYỂN' : resumeData.general.jobTitle }}</p>
          <div class="summary-box">{{ isEmpty(resumeData.general.summary) ? 'Mục tiêu nghề nghiệp...' : resumeData.general.summary }}</div>
        </div>
      </div>

      <div class="middle-grid">
        <div class="grid-col" style="order: 0;">
          <h3 class="section-title">THÔNG TIN CÁ NHÂN</h3>
          <ul class="contact-list">
            <li><i class="fas fa-calendar-alt"></i><span>{{ !isEmpty(resumeData.general.birthDate) ? resumeData.general.birthDate : '27/01/1998' }}</span></li>
            <li><i class="fas fa-envelope"></i><span>{{ !isEmpty(resumeData.general.email) ? resumeData.general.email : 'email@example.com' }}</span></li>
            <li><i class="fas fa-phone-alt"></i><span>{{ !isEmpty(resumeData.general.phone) ? resumeData.general.phone : '0123.456.789' }}</span></li>
            <li><i class="fas fa-globe"></i><span>{{ !isEmpty(resumeData.general.website) ? resumeData.general.website : 'https://github.com/khoa' }}</span></li>
            <li><i class="fas fa-map-marker-alt"></i><span>{{ !isEmpty(resumeData.general.address) ? resumeData.general.address : 'TP. Hồ Chí Minh' }}</span></li>
          </ul>
        </div>

        <div class="grid-col section-block" :style="{ order: getOrder('education', gridIds) }" :class="{ 'section-active': selectedSectionId === 'education' }" @click.stop="toggleSection('education')">
          <transition name="fade-btns">
            <div v-if="selectedSectionId === 'education'" class="nav-btns no-print">
              <button class="nav-btn" @click.stop="moveUp('education',gridIds)"><svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
              <button class="nav-btn" @click.stop="moveDown('education',gridIds)"><svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
            </div>
          </transition>
          <h3 class="section-title">HỌC VẤN</h3>
          <div class="content-text">
            <div v-for="(item, idx) in (educationSection ? educationSection.items : [])" :key="item._refId || idx" class="exp-item item-container">
              <div class="exp-content">
                <div class="paginated-item"><strong>{{ item.school }}</strong></div>
                <div v-if="item.major" class="paginated-item" style="color:#555">{{ item.major }}</div>
                <div v-if="item.gradType || item.info" class="paginated-item" style="font-size: 12.5px; color: #333; margin-top: 2px;">
                  <strong>Tốt nghiệp loại:</strong> {{ item.gradType || item.info }}
                </div>
              </div>
              <div class="exp-year paginated-item">{{ item.year || item.time }}</div>
              <div v-if="item.desc" class="html-content" v-html="formatDesc(item.desc)"></div>
              <transition name="fade-btns"><button v-if="selectedSectionId === 'education'" @click.stop="$emit('removeItem','education',idx)" class="delete-item-btn no-print"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
            </div>
          </div>
        </div>

        <div class="grid-col section-block" :style="{ order: getOrder('certifications', gridIds) }" :class="{ 'section-active': selectedSectionId === 'certifications' }" @click.stop="toggleSection('certifications')">
          <transition name="fade-btns">
            <div v-if="selectedSectionId === 'certifications'" class="nav-btns no-print">
              <button class="nav-btn" @click.stop="moveUp('certifications',gridIds)"><svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
              <button class="nav-btn" @click.stop="moveDown('certifications',gridIds)"><svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
            </div>
          </transition>
          <h3 class="section-title">CHỨNG CHỈ</h3>
          <div class="content-text">
            <div v-for="(item, idx) in (certSection ? certSection.items : [])" :key="item._refId || idx" class="item-container" style="margin-bottom:12px">
              <div class="paginated-item" style="font-weight:700;font-size:13px;color:#111">{{ item.year || item.time }}</div>
              <div class="paginated-item" style="font-size:12.5px;color:#333">{{ item.name || item.info }}</div>
              <transition name="fade-btns"><button v-if="selectedSectionId === 'certifications'" @click.stop="$emit('removeItem','certifications',idx)" class="delete-item-btn no-print"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
            </div>
          </div>
        </div>
      </div>

      <div class="main-sections-wrapper" style="display: flex; flex-direction: column;">
        <div v-if="experienceSection && experienceSection.isVisible" class="main-section section-block" :style="{ order: getOrder('experience', mainIds) }" :class="{ 'section-active': selectedSectionId === 'experience' }" @click.stop="toggleSection('experience')">
          <transition name="fade-btns">
            <div v-if="selectedSectionId === 'experience'" class="nav-btns no-print">
              <button class="nav-btn" @click.stop="moveUp('experience',mainIds)"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
              <button class="nav-btn" @click.stop="moveDown('experience',mainIds)"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
            </div>
          </transition>
          <h3 class="section-title">KINH NGHIỆM LÀM VIỆC</h3>
          <div class="timeline-container">
            <div v-for="(item, idx) in experienceSection.items" :key="item._refId || idx" class="exp-item item-container">
              <div class="exp-year paginated-item">{{ item.time || item.year }}</div>
              
              <div class="exp-content">
                <div class="info-line paginated-item">
                  <strong v-if="item.company || item.name">{{ item.company || item.name }}</strong>
                </div>
                <div v-if="item.role || item.title || item.info" class="info-line paginated-item">
                  <strong>{{ item.role || item.title || item.info }}</strong>
                </div>
                <div v-if="item.desc" class="desc-text html-content" v-html="formatDesc(item.desc)"></div>
              </div>

              <transition name="fade-btns"><button v-if="selectedSectionId === 'experience'" @click.stop="$emit('removeItem','experience',idx)" class="delete-item-btn delete-item-btn--lg no-print"><svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
            </div>
          </div>
        </div>

        <div v-if="projectSection && projectSection.isVisible" class="main-section section-block" :style="{ order: getOrder('project', mainIds) }" :class="{ 'section-active': selectedSectionId === 'project' }" @click.stop="toggleSection('project')">
          <transition name="fade-btns">
            <div v-if="selectedSectionId === 'project'" class="nav-btns no-print">
              <button class="nav-btn" @click.stop="moveUp('project',mainIds)"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
              <button class="nav-btn" @click.stop="moveDown('project',mainIds)"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
            </div>
          </transition>
          <h3 class="section-title">DỰ ÁN NỔI BẬT</h3>
          <div class="timeline-container">
            <div v-for="(item, idx) in projectSection.items" :key="item._refId || idx" class="exp-item item-container">
              <div class="exp-year paginated-item">{{ item.time || item.year }}</div>
              
              <div class="exp-content">
                <div class="info-line paginated-item">
                  <strong v-if="item.name || item.company">{{ item.name || item.company }}</strong>
                </div>
                <div v-if="item.role || item.title || item.info" class="info-line paginated-item">
                  <strong>{{ item.role || item.title || item.info }}</strong>
                </div>
                <div v-if="item.desc" class="desc-text html-content" v-html="formatDesc(item.desc)"></div>
              </div>

              <transition name="fade-btns"><button v-if="selectedSectionId === 'project'" @click.stop="$emit('removeItem','project',idx)" class="delete-item-btn delete-item-btn--lg no-print"><svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
            </div>
          </div>
        </div>
      </div>

      <div class="footer-grid">

        <div class="footer-left" style="display: flex; flex-direction: column;">
          
          <div v-if="skillsSection && skillsSection.isVisible" class="sub-section section-block" :style="{ order: getOrder('skills', footerLeftIds) }" :class="{ 'section-active': selectedSectionId === 'skills' }" @click.stop="toggleSection('skills')">
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'skills'" class="nav-btns no-print">
                <button class="nav-btn" @click.stop="moveUp('skills', footerLeftIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button class="nav-btn" @click.stop="moveDown('skills', footerLeftIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              </div>
            </transition>
            <h3 class="section-title">{{ isEmpty(skillsSection.name) ? 'KỸ NĂNG' : skillsSection.name }}</h3>
            <div class="content-text">
              <ul>
                <li v-for="(item, i) in skillsSection.items" :key="item._refId || i" class="item-container" style="flex-direction: column; align-items: flex-start;">
                  <div class="paginated-item" style="width: 100%;">
                    <strong style="color: #111; font-size: 13px;">{{ item.name }}</strong>
                    <span v-if="item.info || item.level" style="color: #444; font-size: 12.5px;"> — {{ item.info || item.level }}</span>
                  </div>
                  <div v-if="item.desc" class="paginated-item html-content" v-html="formatDesc(item.desc)" style="margin-top: 4px; color: #555;"></div>
                  <transition name="fade-btns"><button v-if="selectedSectionId === 'skills'" @click.stop="$emit('removeItem','skills',i)" class="delete-item-btn no-print" style="top:4px;right:0"><svg width="7" height="7" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
                </li>
              </ul>
            </div>
          </div>

          <div v-if="itSkillsSection && itSkillsSection.isVisible" class="sub-section section-block" :style="{ order: getOrder('it_skills', footerLeftIds) }" :class="{ 'section-active': selectedSectionId === 'it_skills' }" @click.stop="toggleSection('it_skills')">
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'it_skills'" class="nav-btns no-print">
                <button class="nav-btn" @click.stop="moveUp('it_skills', footerLeftIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button class="nav-btn" @click.stop="moveDown('it_skills', footerLeftIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              </div>
            </transition>
            <h3 class="section-title">{{ isEmpty(itSkillsSection.name) ? 'TIN HỌC' : itSkillsSection.name }}</h3>
            <div class="content-text">
              <ul>
                <li v-for="(item, i) in itSkillsSection.items" :key="item._refId || i" class="item-container" style="flex-direction: column; align-items: flex-start;">
                  <div class="paginated-item" style="width: 100%;">
                    <strong style="color: #111; font-size: 13px;">{{ item.name }}</strong>
                    <span v-if="item.info || item.level" style="color: #444; font-size: 12.5px;"> — {{ item.info || item.level }}</span>
                  </div>
                  <div v-if="item.desc" class="paginated-item html-content" v-html="formatDesc(item.desc)" style="margin-top: 4px; color: #555;"></div>
                  <transition name="fade-btns"><button v-if="selectedSectionId === 'it_skills'" @click.stop="$emit('removeItem','it_skills',i)" class="delete-item-btn no-print" style="top:4px;right:0"><svg width="7" height="7" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
                </li>
              </ul>
            </div>
          </div>

          <div v-if="languagesSection && languagesSection.isVisible" class="sub-section section-block" :style="{ order: getOrder('languages', footerLeftIds) }" :class="{ 'section-active': selectedSectionId === 'languages' }" @click.stop="toggleSection('languages')">
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'languages'" class="nav-btns no-print">
                <button class="nav-btn" @click.stop="moveUp('languages', footerLeftIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button class="nav-btn" @click.stop="moveDown('languages', footerLeftIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              </div>
            </transition>
            <h3 class="section-title">{{ isEmpty(languagesSection.name) ? 'NGOẠI NGỮ' : languagesSection.name }}</h3>
            <div class="content-text">
              <ul>
                <li v-for="(item, i) in languagesSection.items" :key="item._refId || i" class="item-container" style="flex-direction: column; align-items: flex-start;">
                  <div class="paginated-item" style="width: 100%;">
                    <strong style="color: #111; font-size: 13px;">{{ item.name }}</strong>
                    <span v-if="item.info || item.level" style="color: #444; font-size: 12.5px;"> — {{ item.info || item.level }}</span>
                  </div>
                  <div v-if="item.desc" class="paginated-item html-content" v-html="formatDesc(item.desc)" style="margin-top: 4px; color: #555;"></div>
                  <transition name="fade-btns"><button v-if="selectedSectionId === 'languages'" @click.stop="$emit('removeItem','languages',i)" class="delete-item-btn no-print" style="top:4px;right:0"><svg width="7" height="7" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
                </li>
              </ul>
            </div>
          </div>

          <div v-if="otherSkillsSection && otherSkillsSection.isVisible" class="sub-section section-block" :style="{ order: getOrder('otherSkills', footerLeftIds) }" :class="{ 'section-active': selectedSectionId === 'otherSkills' }" @click.stop="toggleSection('otherSkills')">
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'otherSkills'" class="nav-btns no-print">
                <button class="nav-btn" @click.stop="moveUp('otherSkills', footerLeftIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button class="nav-btn" @click.stop="moveDown('otherSkills', footerLeftIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              </div>
            </transition>
            <h3 class="section-title">{{ isEmpty(otherSkillsSection.name) ? 'KỸ NĂNG KHÁC' : otherSkillsSection.name }}</h3>
            <div class="content-text">
              <ul>
                <li v-for="(item, i) in otherSkillsSection.items" :key="item._refId || i" class="item-container" style="flex-direction: column; align-items: flex-start;">
                  <div class="paginated-item" style="width: 100%;">
                    <strong style="color: #111; font-size: 13px;">{{ item.name }}</strong>
                    <span v-if="item.info || item.level" style="color: #444; font-size: 12.5px;"> — {{ item.info || item.level }}</span>
                  </div>
                  <div v-if="item.desc" class="paginated-item html-content" v-html="formatDesc(item.desc)" style="margin-top: 4px; color: #555;"></div>
                  <transition name="fade-btns"><button v-if="selectedSectionId === 'otherSkills'" @click.stop="$emit('removeItem','otherSkills',i)" class="delete-item-btn no-print" style="top:4px;right:0"><svg width="7" height="7" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
                </li>
              </ul>
            </div>
          </div>

          <div v-if="hobbiesSection && hobbiesSection.isVisible" class="sub-section section-block" :style="{ order: getOrder('hobbies', footerLeftIds) }" :class="{ 'section-active': selectedSectionId === 'hobbies' }" @click.stop="toggleSection('hobbies')">
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'hobbies'" class="nav-btns no-print">
                <button class="nav-btn" @click.stop="moveUp('hobbies', footerLeftIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button class="nav-btn" @click.stop="moveDown('hobbies', footerLeftIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              </div>
            </transition>
            <h3 class="section-title">{{ isEmpty(hobbiesSection.name) ? 'SỞ THÍCH' : hobbiesSection.name }}</h3>
            <div class="content-text" style="display: flex; flex-wrap: wrap; gap: 8px;">
              <span v-for="(item, i) in hobbiesSection.items" :key="item._refId || i" class="paginated-item item-container" style="display: inline-flex; align-items: center; background: #f1f5f9; padding: 5px 12px; border-radius: 20px; font-size: 12.5px; color: #334155; border: 1px solid #e2e8f0; position: relative;">
                {{ item.name }}
                <transition name="fade-btns"><button v-if="selectedSectionId === 'hobbies'" @click.stop="$emit('removeItem','hobbies',i)" class="delete-item-btn no-print" style="width: 16px; height: 16px; top: -6px; right: -6px;"><svg width="7" height="7" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
              </span>
            </div>
          </div>

        </div><div class="footer-right" style="display: flex; flex-direction: column;">
          <div v-if="awardsSection && awardsSection.isVisible" class="sub-section section-block" :style="{ order: getOrder('awards', footerRightIds) }" :class="{ 'section-active': selectedSectionId === 'awards' }" @click.stop="toggleSection('awards')">
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'awards'" class="nav-btns no-print">
                <button class="nav-btn" @click.stop="moveUp('awards',footerRightIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button class="nav-btn" @click.stop="moveDown('awards',footerRightIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              </div>
            </transition>
            <h3 class="section-title">{{ isEmpty(awardsSection.name) ? 'DANH HIỆU & GIẢI THƯỞNG' : awardsSection.name }}</h3>
            <div class="content-text">
              <ul v-if="awardsSection && awardsSection.items.length">
                <li v-for="(item, i) in awardsSection.items" :key="item._refId || i" class="item-container" style="flex-direction: column; align-items: flex-start;">
                  <div class="paginated-item" style="width: 100%;">
                    <strong v-if="item.time || item.year" style="color: #111; font-size: 13px;">{{ item.time || item.year }}</strong>
                    <span v-if="(item.time || item.year) && (item.name || item.info)"> — </span>
                    <span style="color: #333; font-size: 12.5px;">{{ item.name || item.info }}</span>
                  </div>
                  <div v-if="item.desc" class="paginated-item html-content" v-html="formatDesc(item.desc)" style="margin-top: 4px; color: #555;"></div>
                  <transition name="fade-btns"><button v-if="selectedSectionId === 'awards'" @click.stop="$emit('removeItem','awards',i)" class="delete-item-btn no-print" style="top:4px;right:0"><svg width="7" height="7" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
                </li>
              </ul>
            </div>
          </div>

          <div v-if="activitiesSection && activitiesSection.isVisible" class="sub-section section-block" :style="{ order: getOrder('activities', footerRightIds) }" :class="{ 'section-active': selectedSectionId === 'activities' }" @click.stop="toggleSection('activities')">
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'activities'" class="nav-btns no-print">
                <button class="nav-btn" @click.stop="moveUp('activities',footerRightIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button class="nav-btn" @click.stop="moveDown('activities',footerRightIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              </div>
            </transition>
            <h3 class="section-title">{{ isEmpty(activitiesSection.name) ? 'HOẠT ĐỘNG' : activitiesSection.name }}</h3>
            <div class="timeline-container small-timeline">
              <div v-for="(item, i) in activitiesSection.items" :key="item._refId || i" class="exp-item item-container">
                <div class="exp-year paginated-item">{{ item.time || item.year }}</div>
                
                <div class="exp-content">
                  <div class="info-line paginated-item">{{ item.name || item.company }}</div>
                  <div class="info-line paginated-item">{{ item.role || item.title || item.info }}</div>
                  <div v-if="item.desc" class="desc-text html-content" v-html="formatDesc(item.desc)"></div>
                </div>

                <transition name="fade-btns"><button v-if="selectedSectionId === 'activities'" @click.stop="$emit('removeItem','activities',i)" class="delete-item-btn no-print" style="top:0;right:0"><svg width="7" height="7" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
              </div>
            </div>
          </div>

          <div v-if="referencesSection && referencesSection.isVisible" class="sub-section section-block" :style="{ order: getOrder('references', footerRightIds) }" :class="{ 'section-active': selectedSectionId === 'references' }" @click.stop="toggleSection('references')">
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'references'" class="nav-btns no-print">
                <button class="nav-btn" @click.stop="moveUp('references',footerRightIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button class="nav-btn" @click.stop="moveDown('references',footerRightIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              </div>
            </transition>
            <h3 class="section-title">{{ isEmpty(referencesSection.name) ? 'NGƯỜI GIỚI THIỆU' : referencesSection.name }}</h3>
            <div class="content-text">
              <div v-for="(item, i) in referencesSection.items" :key="item._refId || i" class="item-container" style="position:relative; margin-bottom: 12px;">
                <div class="paginated-item" v-if="item.name || item.title || item.role" style="margin-bottom: 3px; font-size: 13px;">
                  <strong v-if="item.name" style="color: #111;">{{ item.name }}</strong>
                  <span v-if="(item.title || item.role) && item.name"> — </span>
                  <span v-if="item.title || item.role" style="color: #444;">{{ item.title || item.role }}</span>
                </div>
                <div class="html-content" v-if="item.contact || item.info" v-html="formatDesc(item.contact || item.info)"></div>
                <transition name="fade-btns"><button v-if="selectedSectionId === 'references'" @click.stop="$emit('removeItem','references',i)" class="delete-item-btn no-print" style="top:0;right:0"><svg width="7" height="7" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
              </div>
            </div>
          </div>
        </div>
      </div>

    </div>

    <div v-for="p in pageCount" :key="'fl-' + p" class="absolute left-0 w-full pointer-events-none" style="z-index:40;height:1.5px" :style="{ top: `calc(${p * 297}mm - 12mm)` }">
      <div style="width:100%;height:100%;background:linear-gradient(to right,#cbd5e1,#94a3b8,#cbd5e1);opacity:0.35"></div>
    </div>
    <div v-for="p in (pageCount - 1)" :key="'pb-' + p" class="no-print absolute left-0 w-full flex flex-col items-center justify-center pointer-events-none" style="z-index:50" :style="{ top: `calc(${p * 297}mm - 8px)` }">
      <div style="width:105%;height:16px;background:rgba(30,41,59,0.95);border-top:1px solid rgba(0,0,0,0.3);border-bottom:1px solid rgba(0,0,0,0.3)"></div>
      <span style="position:absolute;font-size:9px;text-transform:uppercase;font-weight:700;color:#cbd5e1;letter-spacing:0.1em;background:#334155;padding:2px 12px;border-radius:4px;border:1px solid #475569">
        Ngắt trang {{ p + 1 }}
      </span>
    </div>

  </div>
</template>

<script setup>
import { computed, ref, onMounted, nextTick, watch, onUnmounted, toRaw, unref } from 'vue'

const cvRoot = ref(null)
const pageCount = ref(1)
const selectedSectionId = ref(null)

const props = defineProps({
  resumeData: { type: Object, required: true }
})
const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

const isEmpty = (val) => {
  if (!val) return true
  return String(val).replace(/<[^>]*>/g, '').trim() === ''
}

const toggleSection = (id) => {
  selectedSectionId.value = selectedSectionId.value === id ? null : id
}

const handleOutsideClick = (e) => {
  if (cvRoot.value && !cvRoot.value.contains(e.target)) selectedSectionId.value = null
}

const formatDesc = (text) => {
  if (!text) return ''
  if (!/<[a-z][\s\S]*>/i.test(text)) {
    return text.split('\n').map(l => l.trim()).filter(Boolean)
      .map(l => `<div class="paginated-item">${l}</div>`).join('')
  }
  const tmp = document.createElement('div')
  tmp.innerHTML = text
  const out = document.createElement('div')
  Array.from(tmp.childNodes).forEach(node => {
    if (node.nodeType === Node.TEXT_NODE) {
      if (node.textContent.trim()) {
        const d = document.createElement('div'); d.className = 'paginated-item'; d.appendChild(node.cloneNode(true)); out.appendChild(d)
      }
    } else if (node.nodeType === Node.ELEMENT_NODE) {
      if (node.tagName === 'BR') {
        const d = document.createElement('div'); d.className = 'paginated-item h-[14px]'; out.appendChild(d)
      } else if (['UL','OL'].includes(node.tagName)) {
        Array.from(node.children).forEach(li => li.classList.add('paginated-item'))
        out.appendChild(node.cloneNode(true))
      } else {
        node.classList.add('paginated-item'); out.appendChild(node.cloneNode(true))
      }
    }
  })
  return out.innerHTML
}

const getOrder = (id, arr) => {
  const idx = arr.indexOf(id)
  return idx !== -1 ? idx + 1 : 99
}

const primaryColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  if (!c || ['#ffffff','#fff','#white','white'].includes(c.toLowerCase())) return '#1a568c'
  return c
})

const secondaryColor = computed(() => {
  const c = props.resumeData?.theme?.secondaryColor
  if (!c || ['#ffffff','#fff','#white','white'].includes(c.toLowerCase())) return '#9b1c1c'
  return c
})

const sec = (id) => computed(() => props.resumeData.sections?.find(s => s.id === id))
const educationSection   = sec('education')
const certSection        = sec('certifications')
const experienceSection  = sec('experience')
const projectSection     = sec('project')
const skillsSection      = sec('skills')
const itSkillsSection    = sec('it_skills')
const languagesSection   = sec('languages')
const otherSkillsSection = sec('otherSkills')
const hobbiesSection     = sec('hobbies')
const awardsSection      = sec('awards')
const activitiesSection  = sec('activities')
const referencesSection  = sec('references')

// ĐÃ FIX: Lọc loại bỏ các ID tàng hình để Move Up/Down đổi chỗ chính xác
const getActiveSortedIds = (baseIds) => {
  const order = props.resumeData?.sections?.map(s => s.id) || []
  return baseIds
    .filter(id => order.includes(id)) 
    .sort((a, b) => order.indexOf(a) - order.indexOf(b))
}

const gridIds = computed(() => getActiveSortedIds(['education', 'certifications']))
const mainIds = computed(() => getActiveSortedIds(['experience', 'project']))
const footerLeftIds = computed(() => getActiveSortedIds(['skills', 'it_skills', 'languages', 'otherSkills', 'hobbies']))
const footerRightIds = computed(() => getActiveSortedIds(['awards', 'activities', 'references']))

const A4_W_MM = 210, A4_H_MM = 297
let paginateTimer = null

const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 60)
}

const doPagination = async () => {
  if (!cvRoot.value) return
  const els = cvRoot.value.querySelectorAll('.paginated-item')
  els.forEach(el => { el.style.marginTop = '0px' })
  await nextTick()

  const pxPerMm = cvRoot.value.offsetWidth / A4_W_MM
  const pageH   = A4_H_MM * pxPerMm
  const safe    = 14 * pxPerMm
  const topM    = 8  * pxPerMm

  const getTop = (el) => {
    let o = 0, c = el
    while (c && c !== cvRoot.value) { o += c.offsetTop; c = c.offsetParent }
    return o
  }

  els.forEach(el => {
    if (!el.offsetHeight) return
    const topInPage = getTop(el) % pageH
    const botInPage = topInPage + el.offsetHeight
    if (botInPage > pageH - safe) {
      el.style.marginTop = `${pageH - topInPage + topM}px`
    }
  })

  let maxB = 0
  els.forEach(el => { const b = getTop(el) + el.offsetHeight; if (b > maxB) maxB = b })
  pageCount.value = Math.max(1, Math.ceil(maxB / pageH))
}

const moveUp   = (id, arr) => emit('moveUp',   id, toRaw(unref(arr)))
const moveDown = (id, arr) => emit('moveDown', id, toRaw(unref(arr)))

watch(() => props.resumeData, requestPagination, { deep: true })

onMounted(() => {
  requestPagination()
  window.addEventListener('resize', requestPagination)
  document.addEventListener('keyup', requestPagination)
  document.addEventListener('click', handleOutsideClick)
})
onUnmounted(() => {
  window.removeEventListener('resize', requestPagination)
  document.removeEventListener('keyup', requestPagination)
  document.removeEventListener('click', handleOutsideClick)
  if (paginateTimer) clearTimeout(paginateTimer)
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Be+Vietnam+Pro:wght@400;500;600;700&display=swap');
@import url('https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css');

@media print {
  @page { size: A4; margin: 0; }
  .no-print { display: none !important; }
  .section-block, .section-block.section-active {
    border-color: transparent !important;
    box-shadow: none !important;
  }
}
:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block) { border-color: transparent !important; box-shadow: none !important; }

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
  position: relative;
}

.cv-elegant-wrapper {
  width: 100%; min-height: 297mm; background: #fff; padding: 45px 50px;
  font-family: "Be Vietnam Pro", sans-serif; color: #333; margin: 0 auto;
  box-sizing: border-box; overflow: hidden; 
  border-bottom: 5px solid v-bind(primaryColor);
}
.cv-elegant-wrapper * { box-sizing: border-box; word-wrap: break-word; word-break: break-word; }

.header-area {
  display: flex;
  gap: 28px;
  margin-bottom: 25px;
  align-items: flex-start;
}

/* ĐÃ FIX: Tăng kích thước khung avatar mà không ảnh hưởng mục khác */
.avatar-box {
  flex-shrink: 0;
  width: 180px; /* Tăng từ 110px */
  height: 180px; /* Tăng từ 110px */
  border-radius: 50%;
  overflow: hidden;
  border: 3px solid v-bind(secondaryColor);
  background: #f5f5f5;
  display: flex;
  align-items: center;
  justify-content: center;
}
.avatar-img { width: 100%; height: 100%; object-fit: cover; display: block; }
.avatar-placeholder { width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; background: #f0f0f0; }

.header-content { flex: 1; display: flex; flex-direction: column; justify-content: center; }
.fullname {
  font-size: 28px; color: v-bind(secondaryColor); font-weight: 700; text-transform: uppercase; margin: 0 0 5px 0; letter-spacing: 0.5px;
}
.job-title {
  font-size: 15px; color: #222; font-weight: 500; margin: 0 0 12px 0; border-bottom: 2px solid #222; padding-bottom: 10px; display: inline-block; width: 100%;
}
.summary-box { font-size: 13px; line-height: 1.6; text-align: justify; color: #444; }

.section-title {
  font-size: 14px; font-weight: 700; color: v-bind(primaryColor); border-bottom: 2px solid v-bind(primaryColor);
  padding-bottom: 5px; margin: 0 0 14px 0 !important; text-transform: uppercase; letter-spacing: 0.5px;
}
.main-section { margin-bottom: 28px; }
.main-section:empty { display: none; }

.middle-grid { display: grid; grid-template-columns: 1.3fr 1fr 1fr; gap: 25px; margin-bottom: 28px; }
.contact-list { list-style: none; padding: 0; margin: 0; }
.contact-list li { margin-bottom: 9px; display: flex; gap: 10px; align-items: center; font-size: 12.5px; color: #333; }
.contact-list i { background: v-bind(secondaryColor); color: white !important; width: 22px; height: 22px; border-radius: 4px; display: flex; align-items: center; justify-content: center; font-size: 11px; flex-shrink: 0; }

.grid-col .exp-item { display: flex; flex-direction: column; margin-bottom: 14px; }
.grid-col .exp-year { order: 2; font-size: 12px; color: #555; margin-top: 3px; }
.grid-col .exp-content { order: 1; font-size: 12.5px; line-height: 1.5; color: #444; }
.grid-col .exp-content strong { font-size: 13px; text-transform: uppercase; color: #111; font-weight: 700; }

.timeline-container .exp-item { position: relative; padding-left: 30%; margin-bottom: 22px; min-height: 55px; display: block; }
.timeline-container .exp-item::before { content: ""; position: absolute; left: 30%; top: 6px; width: 2px; height: calc(100% + 18px); background: #ccc; }
.timeline-container .exp-item:last-child::before { display: none; }
.timeline-container .exp-item::after { content: ""; position: absolute; left: calc(30% - 4px); top: 6px; width: 10px; height: 10px; border-radius: 50%; background: v-bind(primaryColor); }
.timeline-container .exp-year { position: absolute; left: 0; top: 3px; width: 28%; font-weight: bold; font-size: 13px; color: #222; }
.timeline-container .exp-content { padding-left: 20px; }
.timeline-container .info-line:first-child { position: absolute; left: 0; top: 22px; width: 28%; font-weight: bold; font-size: 13px; color: #111; }

.timeline-container .info-line:nth-child(2) { font-weight: bold; font-size: 14px; color: #000; margin-bottom: 5px; }

.timeline-container .desc-text { font-size: 12.5px; line-height: 1.6; color: #333; text-align: justify; }

.footer-grid { display: flex; gap: 45px; }
.footer-left { flex: 4; }
.footer-right { flex: 6; }
.sub-section { margin-bottom: 22px; }

.small-timeline .exp-item { padding-left: 18px; margin-bottom: 14px; }
.small-timeline .exp-item::before { left: 0; }
.small-timeline .exp-item::after { left: -4px; width: 10px; height: 10px; top: 6px; }
.small-timeline .exp-year { position: static; width: auto; font-weight: bold; font-size: 12px; margin-bottom: 3px; display: block; }
.small-timeline .exp-content { padding-left: 0; }
.small-timeline .info-line:first-child { position: static; width: auto; font-size: 13.5px; font-weight: bold; text-transform: uppercase; margin-bottom: 2px; }
.small-timeline .info-line:nth-child(2) { font-size: 13px; color: #555; margin-bottom: 5px; font-weight: normal; }

.content-text ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
.content-text li { font-size: 12.5px; color: #333; padding: 5px 0; display: flex; line-height: 1.4; border-bottom: 1px dashed #e0e0e0; margin: 0 !important; }
.content-text li:last-child { border-bottom: none; }
.content-text p { font-size: 13px; font-style: italic; color: #555; margin-bottom: 5px; }

.section-block { border-radius: 5px; border: 2px solid transparent; cursor: pointer; transition: border-color 0.18s ease, box-shadow 0.18s ease; position: relative; }
.section-block.section-active { border: 2px solid v-bind(primaryColor) !important; box-shadow: 0 4px 18px rgba(0,0,0,0.09); z-index: 10; }

.nav-btns { position: absolute; right: 6px; top: 6px; display: flex; flex-direction: row; gap: 5px; z-index: 9999; }
.nav-btn { display: flex; align-items: center; justify-content: center; padding: 4px; background: #2563eb; color: white; border: none; border-radius: 4px; cursor: pointer; box-shadow: 0 2px 6px rgba(37,99,235,0.35); transition: background 0.15s, transform 0.15s; }
.nav-btn:hover { background: #1d4ed8; transform: scale(1.1); }
.nav-btn:active { transform: scale(0.95); }

.delete-item-btn { position: absolute; right: 0; top: 0; width: 18px; height: 18px; display: flex; align-items: center; justify-content: center; background: #ef4444; color: white; border: none; border-radius: 50%; cursor: pointer; box-shadow: 0 1px 4px rgba(0,0,0,0.2); transition: transform 0.15s; z-index: 30; }
.delete-item-btn:hover { transform: scale(1.15); background: #dc2626; }
.delete-item-btn--lg { width: 20px; height: 20px; right: -10px; top: -5px; }

.item-container { position: relative; }

.fade-btns-enter-active, .fade-btns-leave-active { transition: opacity 0.15s, transform 0.15s; }
.fade-btns-enter-from, .fade-btns-leave-to { opacity: 0; transform: scale(0.85); }

:deep(.html-content ul) { list-style-type: disc !important; padding-left: 1.2rem !important; margin: 0.2rem 0; }
:deep(.html-content ol) { list-style-type: decimal !important; padding-left: 1.2rem !important; }
:deep(.html-content li) { margin-bottom: 3px; font-size: 13px; line-height: 1.6; border: none !important; padding: 0 !important; }
:deep(.html-content b), :deep(.html-content strong) { font-weight: 700 !important; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic !important; }
:deep(.html-content u) { text-decoration: underline !important; }
</style>