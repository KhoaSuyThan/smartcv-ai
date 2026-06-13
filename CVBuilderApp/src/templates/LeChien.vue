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
          <h1 class="fullname" v-html="isEmpty(resumeData.general.fullName) ? 'HỌ VÀ TÊN ỨNG VIÊN' : resumeData.general.fullName"></h1>
          <p class="job-title" v-html="isEmpty(resumeData.general.jobTitle) ? 'VỊ TRÍ ỨNG TUYỂN' : resumeData.general.jobTitle"></p>
          <div class="summary-box">{{ isEmpty(resumeData.general.summary) ? 'Mục tiêu nghề nghiệp...' : resumeData.general.summary }}</div>
        </div>
      </div>

      <draggable
        v-model="gridIdsWritable"
        item-key="id"
        group="grid-sections"
        class="middle-grid"
        @end="onDragEnd"
        animation="200"
        ghost-class="opacity-30"
        :delay="100"
        :delayOnTouchOnly="true"
        :fallbackTolerance="5"
        filter=".nav-btn, .delete-btn, .contact-item-btns, .html-content, input"
      >
        <template #header>
          <div class="grid-col section-block cursor-pointer hover:bg-black/5 transition-colors" 
               v-if="contactItems.length > 0"
               :class="{ 'section-active': selectedSectionId === 'contact' }"
               @click.stop="toggleSection('contact')">
            <h3 class="section-title">THÔNG TIN CÁ NHÂN</h3>
            <ul class="contact-list">
              <li v-for="(ci, ciIdx) in contactItems" :key="ci.key" class="item-container group/ci">
                <i :class="ci.icon"></i>
                <span v-html="ci.value"></span>
                
                <!-- Move Up / Move Down / Delete buttons -->
                <transition name="fade-btns">
                  <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print">
                    <button v-if="ciIdx > 0" @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn" title="Di chuyển lên" style="padding:2px">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                    </button>
                    <button v-if="ciIdx < contactItems.length - 1" @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn" title="Di chuyển xuống" style="padding:2px">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                    </button>
                    <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn-danger" title="Ẩn mục này" style="padding:2px">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </div>
                </transition>
              </li>
            </ul>
          </div>
        </template>
        <template #item="{ element: sId }">
          <div class="grid-col section-block cursor-pointer hover:bg-black/5 transition-colors" 
               v-if="sId === 'education' && educationSection?.isVisible"
               :class="{ 'section-active': selectedSectionId === 'education' }" @click.stop="toggleSection('education')" data-section-id="education">
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'education'" class="nav-btns no-print">
                <button class="nav-btn" @click.stop.prevent="$emit('moveUp', educationSection.id, gridIds)"><svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
                <button class="nav-btn" @click.stop.prevent="$emit('moveDown', educationSection.id, gridIds)"><svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg></button>
                <button class="nav-btn nav-btn-danger" @click.stop="hideSection('education')"><svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button>
              </div>
            </transition>
            <h3 class="section-title">HỌC VẤN</h3>
            <div class="content-text">
              <div v-for="(item, idx) in (educationSection ? educationSection.items : [])" :key="item._refId || idx" class="exp-item item-container">
                <div class="exp-content">
                  <div class="paginated-item"><strong><span v-html="item.school"></span></strong></div>
                  <div v-if="item.major" class="paginated-item" style="color:#555"><span v-html="item.major"></span></div>
                  <div v-if="item.gradType || item.info" class="paginated-item" style="font-size: 12.5px; color: #333; margin-top: 2px;">
                    <strong>Tốt nghiệp loại:</strong> <span v-html="item.gradType || item.info"></span>
                  </div>
                </div>
                <div class="exp-year paginated-item"><span v-html="item.year || item.time"></span></div>
                <div v-if="item.desc" class="html-content" v-html="formatDesc(item.desc)"></div>
                <transition name="fade-btns"><button v-if="selectedSectionId === 'education'" @click.stop="$emit('removeItem','education',idx)" class="delete-item-btn no-print"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
              </div>
            </div>
          </div>
  
          <div class="grid-col section-block cursor-pointer hover:bg-black/5 transition-colors" 
               v-else-if="sId === 'certifications' && certSection?.isVisible"
               :class="{ 'section-active': selectedSectionId === 'certifications' }" @click.stop="toggleSection('certifications')" data-section-id="certifications">
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'certifications'" class="nav-btns no-print">
                <button class="nav-btn" @click.stop.prevent="$emit('moveUp', certSection.id, gridIds)"><svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
                <button class="nav-btn" @click.stop.prevent="$emit('moveDown', certSection.id, gridIds)"><svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg></button>
                <button class="nav-btn nav-btn-danger" @click.stop="hideSection('certifications')"><svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button>
              </div>
            </transition>
            <h3 class="section-title">CHỨNG CHỈ</h3>
            <div class="content-text">
              <div v-for="(item, idx) in (certSection ? certSection.items : [])" :key="item._refId || idx" class="item-container" style="margin-bottom:12px">
                <div class="paginated-item" style="font-weight:700;font-size:13px;color:#111"><span v-html="item.year || item.time"></span></div>
                <div class="paginated-item" style="font-size:12.5px;color:#333"><span v-html="item.name || item.info"></span></div>
                <transition name="fade-btns"><button v-if="selectedSectionId === 'certifications'" @click.stop="$emit('removeItem','certifications',idx)" class="delete-item-btn no-print"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
              </div>
            </div>
          </div>
        </template>
      </draggable>

      <draggable
        v-model="verticalIdsWritable"
        item-key="id"
        group="vertical-sections"
        class="vertical-container"
        @end="onDragEnd"
        animation="200"
        ghost-class="opacity-30"
        :delay="100"
        :delayOnTouchOnly="true"
        :fallbackTolerance="5"
        filter=".nav-btn, .delete-btn, .contact-item-btns, .html-content, input"
      >
        <template #item="{ element: id }">
          <!-- Experience -->
          <div v-if="id === 'experience' && experienceSection?.isVisible" 
               class="main-section section-block span-full cursor-pointer hover:bg-black/5 transition-colors" 
               :class="{ 'section-active': selectedSectionId === 'experience' }" 
               @click.stop="toggleSection('experience')" data-section-id="experience">
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'experience'" class="nav-btns no-print">
                <button class="nav-btn" @click.stop.prevent="$emit('moveUp', experienceSection.id, verticalIds)"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button class="nav-btn" @click.stop.prevent="$emit('moveDown', experienceSection.id, verticalIds)"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                <button class="nav-btn nav-btn-danger" @click.stop="hideSection('experience')"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button>
              </div>
            </transition>
            <h3 class="section-title">KINH NGHIỆM LÀM VIỆC</h3>
            <div class="timeline-container">
              <div v-for="(item, idx) in experienceSection.items" :key="item._refId || idx" class="exp-item item-container">
                <div class="exp-year paginated-item"><span v-html="item.time || item.year"></span></div>
                <div class="exp-content">
                  <div class="info-line paginated-item"><strong v-if="item.company || item.name"><span v-html="item.company || item.name"></span></strong></div>
                  <div v-if="item.role || item.title || item.info" class="info-line paginated-item"><strong><span v-html="item.role || item.title || item.info"></span></strong></div>
                  <div v-if="item.desc" class="desc-text html-content" v-html="formatDesc(item.desc)"></div>
                </div>
                <transition name="fade-btns"><button v-if="selectedSectionId === 'experience'" @click.stop="$emit('removeItem','experience',idx)" class="delete-item-btn delete-item-btn--lg no-print"><svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
              </div>
            </div>
          </div>
  
          <!-- Project -->
          <div v-else-if="id === 'project' && projectSection?.isVisible" 
               class="main-section section-block span-full cursor-pointer hover:bg-black/5 transition-colors" 
               :class="{ 'section-active': selectedSectionId === 'project' }" 
               @click.stop="toggleSection('project')" data-section-id="project">
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'project'" class="nav-btns no-print">
                <button class="nav-btn" @click.stop.prevent="$emit('moveUp', projectSection.id, verticalIds)"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button class="nav-btn" @click.stop.prevent="$emit('moveDown', projectSection.id, verticalIds)"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                <button class="nav-btn nav-btn-danger" @click.stop="hideSection('project')"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button>
              </div>
            </transition>
            <h3 class="section-title">DỰ ÁN NỔI BẬT</h3>
            <div class="timeline-container">
              <div v-for="(item, idx) in projectSection.items" :key="item._refId || idx" class="exp-item item-container">
                <div class="exp-year paginated-item"><span v-html="item.time || item.year"></span></div>
                <div class="exp-content">
                  <div class="info-line paginated-item"><strong v-if="item.name || item.company"><span v-html="item.name || item.company"></span></strong></div>
                  <div v-if="item.role || item.title || item.info" class="info-line paginated-item"><strong><span v-html="item.role || item.title || item.info"></span></strong></div>
                  <div v-if="item.desc" class="desc-text html-content" v-html="formatDesc(item.desc)"></div>
                </div>
                <transition name="fade-btns"><button v-if="selectedSectionId === 'project'" @click.stop="$emit('removeItem','project',idx)" class="delete-item-btn delete-item-btn--lg no-print"><svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
              </div>
            </div>
          </div>
  
          <!-- Footer Left Area Sections -->
          <div v-else-if="['skills', 'it_skills', 'languages', 'otherSkills', 'hobbies'].includes(id) && sec(id).value?.isVisible" 
               class="sub-section section-block col-left cursor-pointer hover:bg-black/5 transition-colors" 
               :class="{ 'section-active': selectedSectionId === id }" 
               @click.stop="toggleSection(id)" :data-section-id="id">
            <transition name="fade-btns">
              <div v-if="selectedSectionId === id" class="nav-btns no-print">
                <button class="nav-btn" @click.stop.prevent="$emit('moveUp', id, verticalIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button class="nav-btn" @click.stop.prevent="$emit('moveDown', id, verticalIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                <button class="nav-btn nav-btn-danger" @click.stop="hideSection(id)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button>
              </div>
            </transition>
            <h3 class="section-title">{{ getSectionDisplayTitle(id) }}</h3>
            <div class="content-text">
              <ul v-if="id !== 'hobbies'">
                <li v-for="(item, i) in sec(id).value.items" :key="item._refId || i" class="item-container" style="flex-direction: column; align-items: flex-start;">
                  <div class="paginated-item" style="width: 100%;">
                    <strong style="color: #111; font-size: 13px;"><span v-html="item.name"></span></strong>
                    <span v-if="item.info || item.level" style="color: #444; font-size: 12.5px;"> — <span v-html="item.info || item.level"></span></span>
                  </div>
                  <div v-if="item.desc" class="paginated-item html-content" v-html="formatDesc(item.desc)" style="margin-top: 4px; color: #555;"></div>
                  <transition name="fade-btns"><button v-if="selectedSectionId === id" @click.stop="$emit('removeItem',id,i)" class="delete-item-btn no-print" style="top:4px;right:0"><svg width="7" height="7" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
                </li>
              </ul>
              <div v-else style="display: flex; flex-wrap: wrap; gap: 8px;">
                <span v-for="(item, i) in sec(id).value.items" :key="item._refId || i" class="paginated-item item-container" style="display: inline-flex; align-items: center; background: #f1f5f9; padding: 5px 12px; border-radius: 20px; font-size: 12.5px; color: #334155; border: 1px solid #e2e8f0; position: relative;">
                  <span v-html="item.name"></span>
                  <transition name="fade-btns"><button v-if="selectedSectionId === id" @click.stop="$emit('removeItem',id,i)" class="delete-item-btn no-print" style="width: 16px; height: 16px; top: -6px; right: -6px;"><svg width="7" height="7" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
                </span>
              </div>
            </div>
          </div>
  
          <!-- Footer Right Area Sections -->
          <div v-else-if="['awards', 'activities', 'references'].includes(id) && sec(id).value?.isVisible" 
               class="sub-section section-block col-right cursor-pointer hover:bg-black/5 transition-colors" 
               :class="{ 'section-active': selectedSectionId === id }" 
               @click.stop="toggleSection(id)" :data-section-id="id">
            <transition name="fade-btns">
              <div v-if="selectedSectionId === id" class="nav-btns no-print">
                <button class="nav-btn" @click.stop.prevent="$emit('moveUp', id, verticalIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button class="nav-btn" @click.stop.prevent="$emit('moveDown', id, verticalIds)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                <button class="nav-btn nav-btn-danger" @click.stop="hideSection(id)"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button>
              </div>
            </transition>
            <h3 class="section-title">{{ getSectionDisplayTitle(id) }}</h3>
            <div class="content-text">
              <ul v-if="id === 'awards'">
                <li v-for="(item, i) in sec(id).value.items" :key="item._refId || i" class="item-container" style="flex-direction: column; align-items: flex-start;">
                  <div class="paginated-item" style="width: 100%;">
                    <strong v-if="item.time || item.year" style="color: #111; font-size: 13px;"><span v-html="item.time || item.year"></span></strong>
                    <span v-if="(item.time || item.year) && (item.name || item.info)"> — </span>
                    <span style="color: #333; font-size: 12.5px;"><span v-html="item.name || item.info"></span></span>
                  </div>
                  <div v-if="item.desc" class="paginated-item html-content" v-html="formatDesc(item.desc)" style="margin-top: 4px; color: #555;"></div>
                  <transition name="fade-btns"><button v-if="selectedSectionId === id" @click.stop="$emit('removeItem',id,i)" class="delete-item-btn no-print" style="top:4px;right:0"><svg width="7" height="7" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
                </li>
              </ul>
              <div v-else-if="id === 'activities'" class="timeline-container small-timeline">
                <div v-for="(item, i) in sec(id).value.items" :key="item._refId || i" class="exp-item item-container">
                  <div class="exp-year paginated-item"><span v-html="item.time || item.year"></span></div>
                  <div class="exp-content">
                    <div class="info-line paginated-item"><span v-html="item.name || item.company"></span></div>
                    <div class="info-line paginated-item"><span v-html="item.role || item.title || item.info"></span></div>
                    <div v-if="item.desc" class="desc-text html-content" v-html="formatDesc(item.desc)"></div>
                  </div>
                  <transition name="fade-btns"><button v-if="selectedSectionId === id" @click.stop="$emit('removeItem',id,i)" class="delete-item-btn no-print" style="top:0;right:0"><svg width="7" height="7" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
                </div>
              </div>
              <div v-else-if="id === 'references'">
                <div v-for="(item, i) in sec(id).value.items" :key="item._refId || i" class="item-container" style="position:relative; margin-bottom: 12px;">
                  <div class="paginated-item" v-if="item.name || item.title || item.role" style="margin-bottom: 3px; font-size: 13px;">
                    <strong v-if="item.name" style="color: #111;"><span v-html="item.name"></span></strong>
                    <span v-if="(item.title || item.role) && item.name"> — </span>
                    <span v-if="item.title || item.role" style="color: #444;"><span v-html="item.title || item.role"></span></span>
                  </div>
                  <div class="html-content" v-if="item.contact || item.info" v-html="formatDesc(item.contact || item.info)"></div>
                  <transition name="fade-btns"><button v-if="selectedSectionId === id" @click.stop="$emit('removeItem',id,i)" class="delete-item-btn no-print" style="top:0;right:0"><svg width="7" height="7" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button></transition>
                </div>
              </div>
            </div>
          </div>
        </template>
      </draggable>

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
import draggable from 'vuedraggable'

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

// ─── CONTACT ITEMS ───
const contactIcons = {
  birthDate: 'fas fa-calendar-alt',
  email: 'fas fa-envelope',
  phone: 'fas fa-phone-alt',
  website: 'fas fa-globe',
  address: 'fas fa-map-marker-alt',
  facebook: 'fab fa-facebook-f'
}

const contactOrder = ref(['birthDate', 'email', 'phone', 'website', 'address', 'facebook'])
const hiddenContacts = ref([])

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  return g[key] || ''
}

const contactItems = computed(() => {
  return contactOrder.value
    .filter(key => !hiddenContacts.value.includes(key))
    .filter(key => !isEmpty(getContactValue(key)))
    .map(key => ({
      key,
      icon: contactIcons[key],
      value: getContactValue(key)
    }))
})

const moveContactUp = (idx) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && !isEmpty(getContactValue(k)))
  if (idx <= 0) return
  const keyA = visible[idx], keyB = visible[idx - 1]
  const idxA = contactOrder.value.indexOf(keyA), idxB = contactOrder.value.indexOf(keyB)
  const arr = [...contactOrder.value]
  ;[arr[idxA], arr[idxB]] = [arr[idxB], arr[idxA]]
  contactOrder.value = arr
}

const moveContactDown = (idx) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && !isEmpty(getContactValue(k)))
  if (idx >= visible.length - 1) return
  const keyA = visible[idx], keyB = visible[idx + 1]
  const idxA = contactOrder.value.indexOf(keyA), idxB = contactOrder.value.indexOf(keyB)
  const arr = [...contactOrder.value]
  ;[arr[idxA], arr[idxB]] = [arr[idxB], arr[idxA]]
  contactOrder.value = arr
}

const removeContactItem = (idx) => {
  const visible = contactItems.value
  if (idx >= 0 && idx < visible.length) {
    const key = visible[idx].key
    if (props.resumeData.general[key] !== undefined) {
      props.resumeData.general[key] = ''
    }
    hiddenContacts.value.push(key)
    requestPagination()
  }
}

const hideSection = (id) => {
  const s = props.resumeData.sections?.find(s => s.id === id)
  if (s) s.isVisible = false
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

const getSectionDisplayTitle = (id) => {
  const s = sec(id).value
  if (s && !isEmpty(s.name)) return s.name
  
  const defaults = {
    skills: 'Kỹ năng',
    it_skills: 'TIN HỌC',
    languages: 'NGOẠI NGỮ',
    otherSkills: 'KỸ NĂNG KHÁC',
    hobbies: 'SỞ THÍCH',
    awards: 'DANH HIỆU & GIẢI THƯỞNG',
    activities: 'HOẠT ĐỘNG',
    references: 'NGƯỜI GIỚI THIỆU'
  }
  return defaults[id] || id.toUpperCase()
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

const verticalIds = computed(() => {
  const allVisible = (props.resumeData?.sections || []).filter(s => s.isVisible).map(s => s.id)
  return allVisible.filter(id => id !== 'education' && id !== 'certifications')
})

const gridIdsWritable = ref([])
const verticalIdsWritable = ref([])

watch(gridIds, (newVal) => {
  gridIdsWritable.value = [...newVal]
}, { immediate: true, deep: true })

watch(verticalIds, (newVal) => {
  verticalIdsWritable.value = [...newVal]
}, { immediate: true, deep: true })

const onDragEnd = () => {
  const newOrderIds = [
    ...gridIdsWritable.value,
    ...verticalIdsWritable.value
  ]
  
  const newSections = []
  props.resumeData.sections.forEach(s => {
    if (!newOrderIds.includes(s.id)) {
      newSections.push(s)
    }
  })
  
  newOrderIds.forEach(id => {
    const item = props.resumeData.sections.find(s => s.id === id)
    if (item) newSections.push(item)
  })

  props.resumeData.sections.splice(0, props.resumeData.sections.length, ...newSections)
  requestPagination()
}

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

.vertical-container { display: grid; grid-template-columns: 4fr 6fr; gap: 28px; margin-bottom: 28px; }
.span-full { grid-column: 1 / -1; }
.col-left { grid-column: 1; }
.col-right { grid-column: 2; }

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

.section-block { 
  border-radius: 5px; 
  border: 2px solid transparent; 
  cursor: pointer; 
  transition: border-color 0.18s ease, box-shadow 0.18s ease; 
  position: relative;
  padding: 6px;
  margin: -6px;
}
.section-block.section-active { border: 2px solid v-bind(primaryColor) !important; box-shadow: 0 4px 18px rgba(0,0,0,0.09); z-index: 10; }

.nav-btns { position: absolute; right: 6px; top: 6px; display: flex; flex-direction: row; gap: 5px; z-index: 9999; }
.nav-btn { display: flex; align-items: center; justify-content: center; padding: 4px; background: #2563eb; color: white; border: none; border-radius: 4px; cursor: pointer; box-shadow: 0 2px 6px rgba(37,99,235,0.35); transition: background 0.15s, transform 0.15s; }
.nav-btn:hover { background: #1d4ed8; transform: scale(1.1); }
.nav-btn:active { transform: scale(0.95); }

.nav-btn-danger {
  background: #ef4444 !important;
  box-shadow: 0 2px 6px rgba(239, 68, 68, 0.4) !important;
}
.nav-btn-danger:hover {
  background: #dc2626 !important;
}

.contact-item-container { position: relative; }
.contact-item-btns {
    position: absolute;
    right: 4px;
    top: 50%;
    transform: translateY(-50%);
    display: flex;
    flex-direction: row;
    gap: 3px;
    z-index: 9999;
}

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