<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="cv-pastel-wrapper"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm` }"
    @click.self="selectedSectionId = null"
  >
    <div v-if="contactItems.length > 0" 
         class="pastel-block contact-block paginated-item section-block"
         :class="{ 'section-active': selectedSectionId === 'contact' }"
         @click.stop="toggleSection('contact')">
      <div v-for="(ci, ciIdx) in contactItems" :key="ci.key" class="contact-item item-container group/ci">
        <i :class="ci.icon"></i>
        <span v-html="ci.value"></span>

        <!-- Move Left / Move Right / Delete buttons -->
        <transition name="fade-btns">
          <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print">
            <button v-if="ciIdx > 0" @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn" title="Di chuyển sang trái" style="padding:2px; width: 16px; height: 16px;">
              <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
            </button>
            <button v-if="ciIdx < contactItems.length - 1" @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn" title="Di chuyển sang phải" style="padding:2px; width: 16px; height: 16px;">
              <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
            </button>
            <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn-danger" title="Ẩn mục này" style="padding:2px; width: 16px; height: 16px;">
              <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
            </button>
          </div>
        </transition>
      </div>
    </div>

    <div class="pastel-block profile-block paginated-item">
      <div class="profile-left">
        <h1 class="fullname">{{ !isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'HỌ VÀ TÊN ỨNG VIÊN' }}</h1>
        <div class="job-title-wrapper">
          <span class="job-title">{{ !isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'VỊ TRÍ ỨNG TUYỂN' }}</span>
          <span class="title-line-accent"></span>
        </div>
        <div class="summary-text" v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Mục tiêu nghề nghiệp...'"></div>
      </div>
      <div class="profile-right">
        <div class="avatar-accent"></div>
        <img
          v-if="resumeData.general.avatarUrl || resumeData.general.avatar"
          :src="resumeData.general.avatarUrl || resumeData.general.avatar"
          class="avatar-img"
          alt="Avatar"
        />
        <div v-else class="avatar-placeholder">
          <svg width="60" height="60" viewBox="0 0 24 24" fill="none" stroke="#ccc" stroke-width="1.5">
            <circle cx="12" cy="8" r="4"/><path d="M4 20c0-4 3.6-7 8-7s8 3 8 7"/>
          </svg>
        </div>
      </div>
    </div>

    <draggable
      v-model="blockRepIdsWritable"
      item-key="id"
      group="sections"
      class="sections-container"
      style="display: flex; flex-direction: column;"
      @end="onDragEnd"
      animation="200"
      ghost-class="opacity-30"
      :delay="100"
      :delayOnTouchOnly="true"
      :fallbackTolerance="5"
      filter=".nav-btn, .delete-btn, .contact-item-btns, .html-content, input"
    >
      <template #item="{ element: block }">
        <template v-if="block.id === getRepId('edu_cert') && (educationSection?.isVisible || certSection?.isVisible)">
          <div class="pastel-block section-block cursor-pointer hover:bg-black/5 transition-colors" 
               :class="{ 'section-active': selectedSectionId === 'edu_cert' }"
               @click.stop="toggleSection('edu_cert')" data-section-id="edu_cert">
            
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'edu_cert'" class="nav-btns no-print">
                <button class="nav-btn" @click.stop="$emit('moveUp', getRepId('edu_cert'), blockRepIds)"><i class="fas fa-chevron-up"></i></button>
                <button class="nav-btn" @click.stop="$emit('moveDown', getRepId('edu_cert'), blockRepIds)"><i class="fas fa-chevron-down"></i></button>
                <button class="nav-btn nav-btn-danger" @click.stop="hideGroup('edu_cert')"><i class="fas fa-times"></i></button>
              </div>
            </transition>
    
            <div class="section-group" v-if="educationSection?.isVisible">
              <div class="section-heading paginated-item">
                <h3 class="section-title">{{ educationSection.name || educationSection.title || 'EDUCATION' }}</h3>
                <div class="double-line"><div class="line-blue"></div><div class="line-red"></div></div>
              </div>
              <div class="content-area">
                <div v-for="(edu, i) in (educationSection?.items || [])" :key="i" class="edu-item item-container">
                  <div class="edu-header paginated-item">
                    <div class="exp-content">
                      <strong>{{ edu.school }}</strong>
                      <div>{{ edu.major || edu.degree }}</div>
                      <div v-if="edu.gradType || edu.info" style="font-size: 12px; color: #444; margin-top: 2px;"><strong>Tốt nghiệp loại:</strong> {{ edu.gradType || edu.info }}</div>
                    </div>
                    <div class="exp-year">{{ edu.year || edu.time }}</div>
                  </div>
                  <div v-if="edu.desc" class="html-content" v-html="formatDesc(edu.desc)"></div>
                  <button v-if="selectedSectionId === 'edu_cert' && educationSection?.items?.length" @click.stop="$emit('removeItem','education',i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                </div>
              </div>
            </div>
    
            <div class="section-group" v-if="certSection?.isVisible" :style="{ marginTop: educationSection?.isVisible ? '25px' : '0' }">
              <div class="section-heading paginated-item">
                <h3 class="section-title">{{ certSection.name || certSection.title || 'CERTIFICATIONS' }}</h3>
                <div class="double-line"><div class="line-blue"></div><div class="line-red"></div></div>
              </div>
              <div class="content-area">
                <div v-for="(cert, i) in (certSection?.items || [])" :key="i" class="cert-entry item-container paginated-item">
                  <div class="cert-year-div">{{ cert.year || cert.time }}</div>
                  <div class="cert-name-div">{{ cert.name || cert.title || cert.info }}</div>
                  <div v-if="cert.desc" class="html-content" v-html="formatDesc(cert.desc)"></div>
                  <button v-if="selectedSectionId === 'edu_cert' && certSection?.items?.length" @click.stop="$emit('removeItem','certifications',i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                </div>
              </div>
            </div>
          </div>
        </template>
  
        <template v-else-if="block.id === getRepId('work') && (projectSection?.isVisible || experienceSection?.isVisible)">
          <div class="pastel-block section-block cursor-pointer hover:bg-black/5 transition-colors" 
               :class="{ 'section-active': selectedSectionId === 'work' }"
               @click.stop="toggleSection('work')" data-section-id="work">
            
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'work'" class="nav-btns no-print">
                <button class="nav-btn" @click.stop="$emit('moveUp', getRepId('work'), blockRepIds)"><i class="fas fa-chevron-up"></i></button>
                <button class="nav-btn" @click.stop="$emit('moveDown', getRepId('work'), blockRepIds)"><i class="fas fa-chevron-down"></i></button>
                <button class="nav-btn nav-btn-danger" @click.stop="hideGroup('work')"><i class="fas fa-times"></i></button>
              </div>
            </transition>
    
            <div v-if="projectSection?.isVisible">
              <div class="section-heading paginated-item">
                <h3 class="section-title">{{ projectSection.name || projectSection.title || 'PROJECTS' }}</h3>
                <div class="double-line"><div class="line-blue"></div><div class="line-red"></div></div>
              </div>
              <div class="timeline-area">
                <div v-for="(proj, i) in (projectSection?.items || [])" :key="i" class="timeline-item item-container">
                  <div class="exp-header-wrap paginated-item">
                    <div class="exp-year">{{ proj.year || proj.time }}</div>
                    <div class="exp-content-wrap">
                      <div class="info-name">{{ proj.name || proj.title || proj.company }}</div>
                      <div class="info-role">{{ proj.role || proj.info }}</div>
                    </div>
                  </div>
                  <div v-if="proj.desc || proj.description" class="desc-text html-content" v-html="formatDesc(proj.desc || proj.description)"></div>
                  <button v-if="selectedSectionId === 'work' && projectSection?.items?.length" @click.stop="$emit('removeItem','projects',i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                </div>
              </div>
            </div>
    
            <div v-if="experienceSection?.isVisible" :style="{ marginTop: projectSection?.isVisible ? '25px' : '0' }">
              <div class="section-heading paginated-item">
                <h3 class="section-title">{{ experienceSection.name || experienceSection.title || 'EXPERIENCE' }}</h3>
                <div class="double-line"><div class="line-blue"></div><div class="line-red"></div></div>
              </div>
              <div class="timeline-area">
                <div v-for="(exp, i) in (experienceSection?.items || [])" :key="i" class="timeline-item item-container">
                  <div class="exp-header-wrap paginated-item">
                    <div class="exp-year">{{ exp.year || exp.time }}</div>
                    <div class="exp-content-wrap">
                      <div class="info-name">{{ exp.company || exp.name }}</div>
                      <div class="info-role">{{ exp.role || exp.position || exp.title }}</div>
                    </div>
                  </div>
                  <div v-if="exp.desc || exp.description" class="desc-text html-content" v-html="formatDesc(exp.desc || exp.description)"></div>
                  <button v-if="selectedSectionId === 'work' && experienceSection?.items?.length" @click.stop="$emit('removeItem','experience',i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                </div>
              </div>
            </div>
          </div>
        </template>
  
        <template v-else-if="block.id === getRepId('activities') && activitiesSection?.isVisible">
          <div class="pastel-block section-block cursor-pointer hover:bg-black/5 transition-colors" 
               :class="{ 'section-active': selectedSectionId === 'activities' }"
               @click.stop="toggleSection('activities')" data-section-id="activities">
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'activities'" class="nav-btns no-print">
                <button class="nav-btn" @click.stop="$emit('moveUp', getRepId('activities'), blockRepIds)"><i class="fas fa-chevron-up"></i></button>
                <button class="nav-btn" @click.stop="$emit('moveDown', getRepId('activities'), blockRepIds)"><i class="fas fa-chevron-down"></i></button>
                <button class="nav-btn nav-btn-danger" @click.stop="hideGroup('activities')"><i class="fas fa-times"></i></button>
              </div>
            </transition>
            <div class="section-heading paginated-item">
              <h3 class="section-title">{{ activitiesSection.name || activitiesSection.title || 'ACTIVITIES' }}</h3>
              <div class="double-line"><div class="line-blue"></div><div class="line-red"></div></div>
            </div>
            <div class="act-area">
              <div v-for="(act, i) in (activitiesSection?.items || [])" :key="i" class="timeline-item item-container">
                <div class="exp-header-wrap paginated-item">
                  <div class="date-badge">{{ act.date || act.time || act.year }}</div>
                  <div class="exp-content-wrap">
                    <div class="company-name">{{ act.organization || act.name || act.company }}</div>
                  </div>
                </div>
                <div class="exp-desc html-content" v-html="formatDesc(act.desc || act.description)"></div>
                <button v-if="selectedSectionId === 'activities' && activitiesSection?.items?.length" @click.stop="$emit('removeItem','activities',i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
              </div>
            </div>
          </div>
        </template>
  
        <template v-else-if="block.id === getRepId('skills') && skillsSection?.isVisible">
          <div class="pastel-block section-block cursor-pointer hover:bg-black/5 transition-colors" 
               :class="{ 'section-active': selectedSectionId === 'skills' }"
               @click.stop="toggleSection('skills')" data-section-id="skills">
               
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'skills'" class="nav-btns no-print">
                <button class="nav-btn" @click.stop="$emit('moveUp', getRepId('skills'), blockRepIds)"><i class="fas fa-chevron-up"></i></button>
                <button class="nav-btn" @click.stop="$emit('moveDown', getRepId('skills'), blockRepIds)"><i class="fas fa-chevron-down"></i></button>
                <button class="nav-btn nav-btn-danger" @click.stop="hideGroup('skills')"><i class="fas fa-times"></i></button>
              </div>
            </transition>
            <div class="section-heading paginated-item">
              <h3 class="section-title">{{ skillsSection.name || skillsSection.title || 'SKILLS' }}</h3>
              <div class="double-line"><div class="line-blue"></div><div class="line-red"></div></div>
            </div>
            <div class="content-area">
              <ul class="skill-ul">
                <li v-for="(skill, i) in (skillsSection?.items || [])" :key="i" class="item-container paginated-item">
                  <template v-if="skill.name"><strong>{{ skill.name }}</strong>{{ skill.level ? ': ' + skill.level : '' }}{{ skill.info ? ': ' + skill.info : '' }}</template>
                  <template v-else>{{ skill }}</template>
                  <button v-if="selectedSectionId === 'skills' && skillsSection?.items?.length" @click.stop="$emit('removeItem','skills',i)" class="delete-item-btn no-print" style="right: 0; top: 2px;"><i class="fas fa-times"></i></button>
                </li>
              </ul>
            </div>
          </div>
        </template>
  
        <template v-else-if="block.id === getRepId('footer') && (hobbiesSection?.isVisible || awardsSection?.isVisible)">
          <div class="pastel-block bottom-split section-block cursor-pointer hover:bg-black/5 transition-colors"
               :class="{ 'section-active': selectedSectionId === 'footer' }"
               @click.stop="toggleSection('footer')" data-section-id="footer">
            
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'footer'" class="nav-btns no-print">
                <button class="nav-btn" @click.stop="$emit('moveUp', getRepId('footer'), blockRepIds)"><i class="fas fa-chevron-up"></i></button>
                <button class="nav-btn" @click.stop="$emit('moveDown', getRepId('footer'), blockRepIds)"><i class="fas fa-chevron-down"></i></button>
                <button class="nav-btn nav-btn-danger" @click.stop="hideGroup('footer')"><i class="fas fa-times"></i></button>
              </div>
            </transition>
            
            <div class="bottom-left" v-if="hobbiesSection?.isVisible">
              <div class="section-heading paginated-item">
                <h3 class="section-title">{{ hobbiesSection.name || hobbiesSection.title || 'INTERESTS' }}</h3>
                <div class="double-line"><div class="line-blue"></div><div class="line-red"></div></div>
              </div>
              <div class="content-area">
                <ul class="hobbies-ul">
                  <li v-for="(hobby, i) in (hobbiesSection?.items || [])" :key="i" class="item-container paginated-item">
                    {{ hobby.name || hobby.title || hobby }}
                    <button v-if="selectedSectionId === 'footer' && hobbiesSection?.items?.length" @click.stop="$emit('removeItem','hobbies',i)" class="delete-item-btn no-print" style="right: -10px; top: -5px;"><i class="fas fa-times"></i></button>
                  </li>
                </ul>
              </div>
            </div>
    
            <div class="bottom-right" v-if="awardsSection?.isVisible">
              <div class="awards-group">
                <div class="section-heading paginated-item">
                  <h3 class="section-title">{{ awardsSection.name || awardsSection.title || 'ADDITIONAL INFO' }}</h3>
                  <div class="double-line"><div class="line-blue"></div><div class="line-red"></div></div>
                </div>
                <div class="content-area">
                  <ul class="awards-ul">
                    <li v-for="(award, i) in (awardsSection?.items || [])" :key="i" class="item-container paginated-item">
                      <span v-if="award.year || award.time"><strong>{{ award.year || award.time }}</strong> — </span>
                      {{ award.name || award.title || award }}
                      <button v-if="selectedSectionId === 'footer' && awardsSection?.items?.length" @click.stop="$emit('removeItem','awards',i)" class="delete-item-btn no-print" style="right: 0; top: 0;"><i class="fas fa-times"></i></button>
                    </li>
                  </ul>
                </div>
              </div>
            </div>
          </div>
        </template>
  
        <template v-else-if="unmappedSections.find(s => s.id === block.id && s.isVisible)">
          <div class="pastel-block section-block cursor-pointer hover:bg-black/5 transition-colors"
               :class="{ 'section-active': selectedSectionId === block.id }"
               @click.stop="toggleSection(block.id)" :data-section-id="block.id">
    
            <transition name="fade-btns">
              <div v-if="selectedSectionId === block.id" class="nav-btns no-print">
                <button class="nav-btn" @click.stop="$emit('moveUp', block.id, blockRepIds)"><i class="fas fa-chevron-up"></i></button>
                <button class="nav-btn" @click.stop="$emit('moveDown', block.id, blockRepIds)"><i class="fas fa-chevron-down"></i></button>
                <button class="nav-btn nav-btn-danger" @click.stop="hideGroup(block.id)"><i class="fas fa-times"></i></button>
              </div>
            </transition>
    
            <div class="section-heading paginated-item">
              <h3 class="section-title"><span v-html="unmappedSections.find(s => s.id === block.id).name || unmappedSections.find(s => s.id === block.id).title || block.id.toUpperCase()"></span></h3>
              <div class="double-line"><div class="line-blue"></div><div class="line-red"></div></div>
            </div>
            
            <div class="content-area">
              <div v-if="unmappedSections.find(s => s.id === block.id).desc || unmappedSections.find(s => s.id === block.id).description" class="html-content paginated-item" v-html="formatDesc(unmappedSections.find(s => s.id === block.id).desc || unmappedSections.find(s => s.id === block.id).description)" style="margin-bottom: 12px;"></div>
              
              <div v-for="(item, i) in (unmappedSections.find(s => s.id === block.id).items?.length ? unmappedSections.find(s => s.id === block.id).items : ((unmappedSections.find(s => s.id === block.id).desc || unmappedSections.find(s => s.id === block.id).description) ? [] : [{ name: 'Chưa có dữ liệu' }]))" 
                  :key="i" class="item-container" style="margin-bottom: 12px; position: relative;">
                
                <template v-if="typeof item === 'object'">
                  <div v-if="item.name && /<[a-z][\s\S]*>/i.test(item.name)" class="html-content" v-html="formatDesc(item.name)"></div>
                  <div v-else-if="item.name || item.title || item.level || item.year || item.time" class="paginated-item" style="margin-bottom: 4px;">
                    <strong v-if="item.name || item.title"><span v-html="item.name || item.title"></span></strong>
                    <span v-if="item.level"> — <span v-html="item.level"></span></span>
                    <span v-if="item.year || item.time"> (<span v-html="item.year || item.time"></span>)</span>
                  </div>
                  <div v-if="item.info || item.contact" class="html-content" v-html="formatDesc(item.info || item.contact)"></div>
                  <div v-if="item.desc || item.description" class="html-content" v-html="formatDesc(item.desc || item.description)"></div>
                </template>
                <template v-else>
                  <div class="html-content" v-html="formatDesc(item)"></div>
                </template>
                <button v-if="selectedSectionId === block.id && unmappedSections.find(s => s.id === block.id).items?.length"
                        @click.stop="$emit('removeItem', block.id, i)"
                        class="delete-item-btn no-print" style="top: -5px; right: -5px;">
                  <i class="fas fa-times"></i>
                </button>
              </div>
            </div>
          </div>
        </template>
      </template>
    </draggable>

    <template v-for="p in (pageCount - 1)" :key="'div-' + p">
      <div class="page-break-indicator no-print" :style="{ top: `calc(${p * 297}mm - 8px)` }">
        <div class="page-break-bar"></div>
        <span class="page-break-label">Ngắt trang {{ p + 1 }}</span>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, ref, onMounted, nextTick, watch, onUnmounted, toRaw } from 'vue'
import draggable from 'vuedraggable'

const cvRoot = ref(null)
const pageCount = ref(1)
const selectedSectionId = ref(null)

const props = defineProps({
  resumeData: { type: Object, required: true }
})

const emit = defineEmits(['moveUp', 'moveDown', 'removeItem'])

const hexToRgb = (hex) => {
  const clean = hex.replace('#', '')
  const num = parseInt(clean, 16)
  return {
    r: (num >> 16) & 255,
    g: (num >> 8) & 255,
    b: num & 255
  }
}

const rgbToHex = (r, g, b) => {
  const clamp = (val) => Math.max(0, Math.min(255, Math.round(val)))
  return '#' + ((1 << 24) + (clamp(r) << 16) + (clamp(g) << 8) + clamp(b)).toString(16).slice(1)
}

const adjustColorBrightness = (hex, percent) => {
  try {
    const { r, g, b } = hexToRgb(hex)
    if (percent < 0) {
      const factor = 1 + percent
      return rgbToHex(r * factor, g * factor, b * factor)
    } else {
      return rgbToHex(
        r + (255 - r) * percent,
        g + (255 - g) * percent,
        b + (255 - b) * percent
      )
    }
  } catch (e) {
    return hex
  }
}

const templatePrimaryColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  if (!c || c.toLowerCase() === '#2b5c8f') return '#0056b3'
  return c
})

const isCustomColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  return c && c.toLowerCase() !== '#2b5c8f'
})

const templateAccentColor = computed(() => {
  if (isCustomColor.value) return templatePrimaryColor.value
  return '#D6624B'
})

const templateBlockBgColor = computed(() => {
  if (!isCustomColor.value) return '#EFECE9'
  return adjustColorBrightness(templatePrimaryColor.value, 0.92)
})

const templateNameColor = computed(() => {
  if (isCustomColor.value) return templatePrimaryColor.value
  return '#4A2B28'
})

const isEmpty = (v) => !v || v.toString().trim() === ''

const getSection = (id) => props.resumeData?.sections?.find(s => s.id === id) ?? null

const educationSection   = computed(() => getSection('education'))
const certSection        = computed(() => getSection('certifications') ?? getSection('certificates'))
const projectSection     = computed(() => getSection('projects') ?? getSection('project'))
const experienceSection  = computed(() => getSection('experience'))
const activitiesSection  = computed(() => getSection('activities'))
const skillsSection      = computed(() => getSection('skills'))
const hobbiesSection     = computed(() => getSection('hobbies') ?? getSection('interests'))
const awardsSection      = computed(() => getSection('awards') ?? getSection('additional'))

const MAPPED_IDS = new Set([
  'summary', 'objective',
  'education', 'certifications', 'certificates',
  'projects', 'project', 'experience',
  'activities', 'skills',
  'hobbies', 'interests', 'awards', 'additional'
])

const unmappedSections = computed(() =>
  (props.resumeData?.sections ?? []).filter(s => !MAPPED_IDS.has(s.id))
)

const getRepId = (type) => {
  if (type === 'edu_cert') return educationSection.value?.id || certSection.value?.id
  if (type === 'work') return experienceSection.value?.id || projectSection.value?.id
  if (type === 'activities') return activitiesSection.value?.id
  if (type === 'skills') return skillsSection.value?.id
  if (type === 'footer') return hobbiesSection.value?.id || awardsSection.value?.id
  return type
}

const blockRepIds = computed(() => {
  const reps = []
  if (educationSection.value?.isVisible || certSection.value?.isVisible) {
    reps.push(educationSection.value?.id || certSection.value?.id)
  }
  if (projectSection.value?.isVisible || experienceSection.value?.isVisible) {
    reps.push(experienceSection.value?.id || projectSection.value?.id)
  }
  if (activitiesSection.value?.isVisible) reps.push(activitiesSection.value.id)
  if (skillsSection.value?.isVisible) reps.push(skillsSection.value.id)
  if (hobbiesSection.value?.isVisible || awardsSection.value?.isVisible) {
    reps.push(hobbiesSection.value?.id || awardsSection.value?.id)
  }
  unmappedSections.value.forEach(s => { if (s.isVisible) reps.push(s.id) })

  const sourceOrder = props.resumeData?.sections?.map(s => s.id) || []
  return reps.sort((a, b) => sourceOrder.indexOf(a) - sourceOrder.indexOf(b))
})

const blockRepIdsWritable = ref([])

watch(blockRepIds, (newVal) => {
  blockRepIdsWritable.value = newVal.map(id => ({ id }))
}, { immediate: true, deep: true })

const onDragEnd = () => {
  const newOrderIds = blockRepIdsWritable.value.map(b => b.id)
  
  // Create a mapping from mapped block IDs back to the original section IDs
  const expandMap = {
    [getRepId('edu_cert')]: [educationSection.value?.id, certSection.value?.id].filter(Boolean),
    [getRepId('work')]: [projectSection.value?.id, experienceSection.value?.id].filter(Boolean),
    [getRepId('activities')]: [activitiesSection.value?.id].filter(Boolean),
    [getRepId('skills')]: [skillsSection.value?.id].filter(Boolean),
    [getRepId('footer')]: [hobbiesSection.value?.id, awardsSection.value?.id].filter(Boolean),
  }
  
  const expandedOrderIds = []
  newOrderIds.forEach(id => {
    if (expandMap[id]) {
      expandedOrderIds.push(...expandMap[id])
    } else {
      expandedOrderIds.push(id)
    }
  })

  const newSections = []
  props.resumeData.sections.forEach(s => {
    if (!expandedOrderIds.includes(s.id)) {
      newSections.push(s)
    }
  })
  
  expandedOrderIds.forEach(id => {
    const item = props.resumeData.sections.find(s => s.id === id)
    if (item) newSections.push(item)
  })

  props.resumeData.sections.splice(0, props.resumeData.sections.length, ...newSections)
  requestPagination()
}

const toggleSection = (id) => {
  selectedSectionId.value = selectedSectionId.value === id ? null : id
}

// ─── CONTACT ITEMS ───
const contactIcons = {
  phone: 'fas fa-phone-alt',
  email: 'fas fa-envelope',
  website: 'fas fa-globe',
  address: 'fas fa-map-marker-alt',
  birthDate: 'fas fa-calendar-alt',
  facebook: 'fab fa-facebook-f'
}

const contactOrder = ref(['phone', 'email', 'website', 'address', 'birthDate', 'facebook'])
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

const hideGroup = (groupId) => {
    if (groupId === 'edu_cert') {
        hideSection('education'); hideSection('certifications'); hideSection('certificates')
    } else if (groupId === 'work') {
        hideSection('experience'); hideSection('projects'); hideSection('project')
    } else if (groupId === 'footer') {
        hideSection('hobbies'); hideSection('interests'); hideSection('awards'); hideSection('additional')
    } else {
        hideSection(groupId)
    }
}

// ── Utilities (ĐÃ NÂNG CẤP CHIA NHỎ TỪNG DÒNG HTML) ─────────
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

// ── Pagination Logic ──────────────────────────────────────────────
const A4_H_MM = 297
let paginateTimer = null

const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 80)
}

const doPagination = async () => {
  if (!cvRoot.value) return
  const allEls = cvRoot.value.querySelectorAll('.paginated-item')
  allEls.forEach(el => { el.style.marginTop = '' })
  await nextTick()

  const pxPerMm = cvRoot.value.offsetWidth / 210
  const pageH = A4_H_MM * pxPerMm
  const safeBottom = 15 * pxPerMm

  const getTop = (el) => {
    let offset = 0, curr = el
    while (curr && curr !== cvRoot.value) { offset += curr.offsetTop; curr = curr.offsetParent }
    return offset
  }

  allEls.forEach(el => {
    if (!el.offsetHeight) return
    const top = getTop(el)
    const bottomInPage = (top % pageH) + el.offsetHeight
    if (bottomInPage > pageH - safeBottom) {
      el.style.marginTop = `${pageH - (top % pageH) + 10}px`
    }
  })

  let maxB = 0
  allEls.forEach(el => { maxB = Math.max(maxB, getTop(el) + el.offsetHeight) })
  pageCount.value = Math.max(1, Math.ceil(maxB / pageH))
}

watch(() => props.resumeData, requestPagination, { deep: true })
onMounted(() => { requestPagination(); window.addEventListener('resize', requestPagination) })
onUnmounted(() => {
  window.removeEventListener('resize', requestPagination)
  if (paginateTimer) clearTimeout(paginateTimer)
})
</script>

<style scoped>
@import url('https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css');

.cv-pastel-wrapper {
  width: 210mm; background: #fff; padding: 25px 35px;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  box-sizing: border-box; position: relative; color: #333; overflow: hidden;
}

.pastel-block { background: v-bind(templateBlockBgColor); padding: 20px 25px; border-radius: 4px; margin-bottom: 18px; position: relative; }

.section-title { font-size: 15px; font-weight: 700; color: v-bind(templatePrimaryColor); margin: 0 !important; letter-spacing: 0.5px; text-transform: uppercase; }
.double-line { height: 4px; margin: 6px 0 15px 0; display: flex; flex-direction: column; gap: 1px; }
.line-blue { height: 1.5px; background: v-bind(templatePrimaryColor); width: 100%; }
.line-red  { height: 1.5px; background: v-bind(templateAccentColor); width: 100%; }

.contact-block { display: flex; justify-content: space-around; padding: 12px; }
.contact-item { display: flex; align-items: center; gap: 8px; font-size: 11px; font-weight: 600; position: relative; padding-right: 15px; }
.contact-item i { color: v-bind(templateAccentColor); font-size: 13px; }

.profile-block { display: flex; gap: 30px; align-items: flex-start; }
.profile-left { flex: 1; }
.fullname { font-size: 28px; font-weight: 700; color: v-bind(templateNameColor); margin: 0 0 8px 0; text-transform: uppercase; }
.job-title-wrapper { display: flex; align-items: center; gap: 12px; margin-bottom: 10px; }
.job-title { font-size: 15px; font-weight: 700; color: #111; text-transform: uppercase; }
.title-line-accent { height: 2px; background: v-bind(templateAccentColor); width: 80px; }
.summary-text { font-size: 12.5px; line-height: 1.5; color: #444; }

.profile-right { position: relative; width: 130px; height: 150px; flex-shrink: 0; padding: 8px 0 0 8px; }
.avatar-accent { position: absolute; top: 0; left: 0; width: 60px; height: 60px; background: v-bind(templateAccentColor); z-index: 1; border-radius: 2px; }
.avatar-img, .avatar-placeholder { position: relative; z-index: 2; width: 100%; height: 100%; object-fit: cover; border-radius: 2px; box-shadow: 2px 2px 8px rgba(0,0,0,0.1); background: #fff; }
.avatar-placeholder { display: flex; align-items: center; justify-content: center; }

/* ── KIẾN TRÚC MỚI KHÔNG DÙNG GRID ĐỂ TRÁNH LỖI PHÂN TRANG ── */
.edu-item { margin-bottom: 12px; position: relative; }
.edu-header { display: flex; justify-content: space-between; align-items: flex-start; }
.exp-year { font-weight: 700; font-size: 12.5px; color: #555; text-align: right; min-width: 80px; margin-top: 2px; }
.exp-content { font-size: 12.5px; flex: 1; padding-right: 15px; }
.exp-content strong { display: block; margin-bottom: 2px; color: #000; font-size: 13px; text-transform: uppercase; }

.cert-entry { margin-bottom: 12px; position: relative; }
.cert-year-div { font-size: 12.5px; font-weight: 700; color: #333; margin-bottom: 3px; }
.cert-name-div { font-size: 13px; color: #111; }

.timeline-item { border-left: 1.5px solid #ccc; margin-left: 5px; position: relative; margin-bottom: 20px; padding-bottom: 5px; }
.timeline-item:last-child { margin-bottom: 0; }
.exp-header-wrap { display: flex; gap: 15px; padding-left: 15px; position: relative; }
.exp-header-wrap::after { content: ''; position: absolute; left: -5.5px; top: 5px; width: 10px; height: 10px; border-radius: 50%; background: v-bind(templateAccentColor); }
.date-badge { width: 85px; flex-shrink: 0; font-weight: 700; font-size: 12.5px; color: #444; margin-top: 2px; text-align: left; }
.exp-content-wrap { flex: 1; display: flex; flex-direction: column; gap: 4px; padding-right: 15px; }

.info-name, .company-name { font-size: 13px; font-weight: 700; color: #000; text-transform: uppercase; }
.info-role { font-size: 13px; font-weight: 700; color: #111; }
.desc-text, .exp-desc { padding-left: 115px; margin-top: 6px; padding-right: 15px; }

/* ────────────────────────────────────────────────────────── */

.skill-ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
.skill-ul li { font-size: 13px; line-height: 1.6; margin-bottom: 8px; color: #111; padding-left: 10px; position: relative; }
.skill-ul li::before { content: '•'; position: absolute; left: 0; color: v-bind(templatePrimaryColor); }

.bottom-split { display: grid; grid-template-columns: 1fr 1fr; gap: 30px; }
.hobbies-ul, .awards-ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
.hobbies-ul li, .awards-ul li { font-size: 12.5px; color: #222; margin-bottom: 6px; position: relative; display: inline-block; margin-right: 15px; }

.section-block { border: 2px solid transparent; cursor: pointer; transition: 0.2s; }
.section-active { border-color: v-bind(templatePrimaryColor) !important; box-shadow: 0 0 10px rgba(0,86,179,0.1); z-index: 10; }
.nav-btns { position: absolute; top: 10px; right: 10px; display: flex; gap: 5px; z-index: 100; }
.nav-btn { background: v-bind(templatePrimaryColor); color: #fff; border: none; width: 24px; height: 24px; border-radius: 3px; cursor: pointer; font-size: 12px; display: flex; align-items: center; justify-content: center; transition: background 0.15s, filter 0.15s; }
.nav-btn:hover { filter: brightness(0.85); }

.nav-btn-danger {
  background: #ff4d4f !important;
  box-shadow: 0 2px 6px rgba(255, 77, 79, 0.4) !important;
}
.nav-btn-danger:hover {
  background: #ff7875 !important;
}

.contact-item-btns {
    position: absolute;
    right: -2px;
    top: -14px;
    display: flex;
    flex-direction: row;
    gap: 3px;
    z-index: 9999;
}
.delete-item-btn { position: absolute; right: -8px; top: -8px; width: 18px; height: 18px; background: #ff4d4f; color: white; border: none; border-radius: 50%; cursor: pointer; font-size: 12px; display: flex; align-items: center; justify-content: center; z-index: 50; }
.delete-item-btn:hover { background: #cc0000; }

.item-container { position: relative; }
.fade-btns-enter-active, .fade-btns-leave-active { transition: opacity 0.15s, transform 0.15s; }
.fade-btns-enter-from, .fade-btns-leave-to { opacity: 0; transform: scale(0.85); }

/* XỬ LÝ HTML CONTENT */
:deep(.html-content) { font-size: 12.5px; line-height: 1.6; color: #222; text-align: justify; }
:deep(.html-content ul) { list-style-type: disc !important; padding-left: 1.2rem !important; margin: 0.2rem 0; }
:deep(.html-content ol) { list-style-type: decimal !important; padding-left: 1.2rem !important; }
:deep(.html-content li) { margin-bottom: 3px; font-size: 12.5px; line-height: 1.6; border: none !important; padding: 0 !important; }
:deep(.html-content li::before) { content: none; }
:deep(.html-content b), :deep(.html-content strong) { font-weight: 700 !important; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic !important; }
:deep(.html-content u) { text-decoration: underline !important; }

.page-break-indicator { position: absolute; left: 0; width: 100%; z-index: 50; display: flex; flex-direction: column; align-items: center; justify-content: center; pointer-events: none; }
.page-break-bar { width: 105%; height: 2px; background: rgba(0,0,0,0.1); border-top: 1px dashed rgba(0,0,0,0.2); }
.page-break-label { font-size: 9px; text-transform: uppercase; font-weight: 700; color: #999; background: #fff; padding: 2px 10px; margin-top: -8px; }

@media print { .no-print { display: none !important; } }
</style>