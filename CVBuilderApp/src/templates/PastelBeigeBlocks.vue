<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="cv-pastel-wrapper"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm` }"
    @click.self="selectedSectionId = null"
  >
    <div class="pastel-block contact-block paginated-item">
      <div class="contact-item">
        <i class="fas fa-phone-alt"></i>
        <span>{{ !isEmpty(resumeData.general.phone) ? resumeData.general.phone : '0123.456.789' }}</span>
      </div>
      <div class="contact-item">
        <i class="fas fa-envelope"></i>
        <span>{{ !isEmpty(resumeData.general.email) ? resumeData.general.email : 'email@example.com' }}</span>
      </div>
      <div class="contact-item">
        <i class="fas fa-globe"></i>
        <span>{{ !isEmpty(resumeData.general.website) ? resumeData.general.website : 'https://github.com/khoa' }}</span>
      </div>
      <div class="contact-item">
        <i class="fas fa-map-marker-alt"></i>
        <span>{{ !isEmpty(resumeData.general.address) ? resumeData.general.address : 'TP. Hồ Chí Minh' }}</span>
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

    <div class="sections-container" style="display: flex; flex-direction: column;">
      
      <div class="pastel-block section-block" 
           v-if="educationSection?.isVisible || certSection?.isVisible"
           :style="{ order: getOrder('edu_cert', allIds) }"
           :class="{ 'section-active': selectedSectionId === 'edu_cert' }"
           @click.stop="toggleSection('edu_cert')">
        
        <transition name="fade-btns">
          <div v-if="selectedSectionId === 'edu_cert'" class="nav-btns no-print">
            <button class="nav-btn" @click.stop="moveUp('edu_cert', allIds)"><i class="fas fa-chevron-up"></i></button>
            <button class="nav-btn" @click.stop="moveDown('edu_cert', allIds)"><i class="fas fa-chevron-down"></i></button>
          </div>
        </transition>

        <div class="section-group" v-if="educationSection?.isVisible">
          <div class="section-heading paginated-item">
            <h3 class="section-title">{{ educationSection.name || educationSection.title || 'EDUCATION' }}</h3>
            <div class="double-line"><div class="line-blue"></div><div class="line-red"></div></div>
          </div>
          <div class="content-area">
            <div v-for="(edu, i) in (educationSection?.items?.length ? educationSection.items : [{school: 'Tên trường học', major: 'Chuyên ngành học', year: '2015 - 2019'}])" :key="i" class="edu-item item-container">
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
            <div v-for="(cert, i) in (certSection?.items?.length ? certSection.items : [{year: '2020', name: 'Tên chứng chỉ'}])" :key="i" class="cert-entry item-container paginated-item">
              <div class="cert-year-div">{{ cert.year || cert.time }}</div>
              <div class="cert-name-div">{{ cert.name || cert.title || cert.info }}</div>
              <div v-if="cert.desc" class="html-content" v-html="formatDesc(cert.desc)"></div>
              <button v-if="selectedSectionId === 'edu_cert' && certSection?.items?.length" @click.stop="$emit('removeItem','certifications',i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
            </div>
          </div>
        </div>
      </div>

      <div class="pastel-block section-block" 
           v-if="projectSection?.isVisible || experienceSection?.isVisible"
           :style="{ order: getOrder('work', allIds) }"
           :class="{ 'section-active': selectedSectionId === 'work' }"
           @click.stop="toggleSection('work')">
        
        <transition name="fade-btns">
          <div v-if="selectedSectionId === 'work'" class="nav-btns no-print">
            <button class="nav-btn" @click.stop="moveUp('work', allIds)"><i class="fas fa-chevron-up"></i></button>
            <button class="nav-btn" @click.stop="moveDown('work', allIds)"><i class="fas fa-chevron-down"></i></button>
          </div>
        </transition>

        <div v-if="projectSection?.isVisible">
          <div class="section-heading paginated-item">
            <h3 class="section-title">{{ projectSection.name || projectSection.title || 'PROJECTS' }}</h3>
            <div class="double-line"><div class="line-blue"></div><div class="line-red"></div></div>
          </div>
          <div class="timeline-area">
            <div v-for="(proj, i) in (projectSection?.items?.length ? projectSection.items : [{year: '2022', name: 'Tên dự án', role: 'Vai trò của bạn', desc: 'Mô tả dự án...'}])" :key="i" class="timeline-item item-container">
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
            <div v-for="(exp, i) in (experienceSection?.items?.length ? experienceSection.items : [{year: '2021 - Hiện tại', company: 'Tên công ty', role: 'Vị trí công việc', desc: 'Mô tả công việc...'}])" :key="i" class="timeline-item item-container">
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

      <div class="pastel-block section-block" 
           v-if="activitiesSection?.isVisible"
           :style="{ order: getOrder('activities', allIds) }"
           :class="{ 'section-active': selectedSectionId === 'activities' }"
           @click.stop="toggleSection('activities')">
        <transition name="fade-btns">
          <div v-if="selectedSectionId === 'activities'" class="nav-btns no-print">
            <button class="nav-btn" @click.stop="moveUp('activities', allIds)"><i class="fas fa-chevron-up"></i></button>
            <button class="nav-btn" @click.stop="moveDown('activities', allIds)"><i class="fas fa-chevron-down"></i></button>
          </div>
        </transition>
        <div class="section-heading paginated-item">
          <h3 class="section-title">{{ activitiesSection.name || activitiesSection.title || 'ACTIVITIES' }}</h3>
          <div class="double-line"><div class="line-blue"></div><div class="line-red"></div></div>
        </div>
        <div class="act-area">
          <div v-for="(act, i) in (activitiesSection?.items?.length ? activitiesSection.items : [{date: '2020', organization: 'Tổ chức/Hoạt động', desc: 'Mô tả ngắn gọn về hoạt động...'}])" :key="i" class="timeline-item item-container">
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

      <div class="pastel-block section-block" 
           v-if="skillsSection?.isVisible"
           :style="{ order: getOrder('skills', allIds) }"
           :class="{ 'section-active': selectedSectionId === 'skills' }"
           @click.stop="toggleSection('skills')">
           
        <transition name="fade-btns">
          <div v-if="selectedSectionId === 'skills'" class="nav-btns no-print">
            <button class="nav-btn" @click.stop="moveUp('skills', allIds)"><i class="fas fa-chevron-up"></i></button>
            <button class="nav-btn" @click.stop="moveDown('skills', allIds)"><i class="fas fa-chevron-down"></i></button>
          </div>
        </transition>
        <div class="section-heading paginated-item">
          <h3 class="section-title">{{ skillsSection.name || skillsSection.title || 'SKILLS' }}</h3>
          <div class="double-line"><div class="line-blue"></div><div class="line-red"></div></div>
        </div>
        <div class="content-area">
          <ul class="skill-ul">
            <li v-for="(skill, i) in (skillsSection?.items?.length ? skillsSection.items : [{name: 'Kỹ năng 1'}, {name: 'Kỹ năng 2'}])" :key="i" class="item-container paginated-item">
              <template v-if="skill.name"><strong>{{ skill.name }}</strong>{{ skill.level ? ': ' + skill.level : '' }}{{ skill.info ? ': ' + skill.info : '' }}</template>
              <template v-else>{{ skill }}</template>
              <button v-if="selectedSectionId === 'skills' && skillsSection?.items?.length" @click.stop="$emit('removeItem','skills',i)" class="delete-item-btn no-print" style="right: 0; top: 2px;"><i class="fas fa-times"></i></button>
            </li>
          </ul>
        </div>
      </div>

      <div class="pastel-block bottom-split section-block"
           v-if="hobbiesSection?.isVisible || awardsSection?.isVisible"
           :style="{ order: getOrder('footer', allIds) }"
           :class="{ 'section-active': selectedSectionId === 'footer' }"
           @click.stop="toggleSection('footer')">
        
        <transition name="fade-btns">
          <div v-if="selectedSectionId === 'footer'" class="nav-btns no-print">
            <button class="nav-btn" @click.stop="moveUp('footer', allIds)"><i class="fas fa-chevron-up"></i></button>
            <button class="nav-btn" @click.stop="moveDown('footer', allIds)"><i class="fas fa-chevron-down"></i></button>
          </div>
        </transition>
        
        <div class="bottom-left" v-if="hobbiesSection?.isVisible">
          <div class="section-heading paginated-item">
            <h3 class="section-title">{{ hobbiesSection.name || hobbiesSection.title || 'INTERESTS' }}</h3>
            <div class="double-line"><div class="line-blue"></div><div class="line-red"></div></div>
          </div>
          <div class="content-area">
            <ul class="hobbies-ul">
              <li v-for="(hobby, i) in (hobbiesSection?.items?.length ? hobbiesSection.items : [{name: 'Sở thích 1'}])" :key="i" class="item-container paginated-item">
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
                <li v-for="(award, i) in (awardsSection?.items?.length ? awardsSection.items : [{name: 'Thông tin bổ sung'}])" :key="i" class="item-container paginated-item">
                  <span v-if="award.year || award.time"><strong>{{ award.year || award.time }}</strong> — </span>
                  {{ award.name || award.title || award }}
                  <button v-if="selectedSectionId === 'footer' && awardsSection?.items?.length" @click.stop="$emit('removeItem','awards',i)" class="delete-item-btn no-print" style="right: 0; top: 0;"><i class="fas fa-times"></i></button>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>

      <template v-for="section in unmappedSections" :key="section.id">
        <div class="pastel-block section-block"
             v-if="section.isVisible"
             :style="{ order: getOrder(section.id, allIds) }"
             :class="{ 'section-active': selectedSectionId === section.id }"
             @click.stop="toggleSection(section.id)">

          <transition name="fade-btns">
            <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
              <button class="nav-btn" @click.stop="moveUp(section.id, allIds)"><i class="fas fa-chevron-up"></i></button>
              <button class="nav-btn" @click.stop="moveDown(section.id, allIds)"><i class="fas fa-chevron-down"></i></button>
            </div>
          </transition>

          <div class="section-heading paginated-item">
            <h3 class="section-title">{{ section.name || section.title || section.id.toUpperCase() }}</h3>
            <div class="double-line"><div class="line-blue"></div><div class="line-red"></div></div>
          </div>
          
          <div class="content-area">
            <div v-if="section.desc || section.description" class="html-content paginated-item" v-html="formatDesc(section.desc || section.description)" style="margin-bottom: 12px;"></div>
            
            <div v-for="(item, i) in (section.items?.length ? section.items : ((section.desc || section.description) ? [] : [{ name: 'Chưa có dữ liệu' }]))" 
                :key="i" class="item-container" style="margin-bottom: 12px; position: relative;">
              
              <template v-if="typeof item === 'object'">
                <div v-if="item.name && /<[a-z][\s\S]*>/i.test(item.name)" class="html-content" v-html="formatDesc(item.name)"></div>
                <div v-else-if="item.name || item.title || item.level || item.year || item.time" class="paginated-item" style="margin-bottom: 4px;">
                  <strong v-if="item.name || item.title">{{ item.name || item.title }}</strong>
                  <span v-if="item.level"> — {{ item.level }}</span>
                  <span v-if="item.year || item.time"> ({{ item.year || item.time }})</span>
                </div>
                <div v-if="item.info || item.contact" class="html-content" v-html="formatDesc(item.info || item.contact)"></div>
                <div v-if="item.desc || item.description" class="html-content" v-html="formatDesc(item.desc || item.description)"></div>
              </template>
              <template v-else>
                <div class="html-content" v-html="formatDesc(item)"></div>
              </template>
              <button v-if="selectedSectionId === section.id && section.items?.length"
                      @click.stop="$emit('removeItem', section.id, i)"
                      class="delete-item-btn no-print" style="top: -5px; right: -5px;">
                <i class="fas fa-times"></i>
              </button>
            </div>
          </div>
        </div>
      </template>

    </div>

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

const cvRoot = ref(null)
const pageCount = ref(1)
const selectedSectionId = ref(null)

const props = defineProps({
  resumeData: { type: Object, required: true }
})

const emit = defineEmits(['moveUp', 'moveDown', 'removeItem'])

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

const allIds = computed(() => {
  const activeIds = []
  if (educationSection.value?.isVisible || certSection.value?.isVisible) activeIds.push('edu_cert')
  if (projectSection.value?.isVisible || experienceSection.value?.isVisible) activeIds.push('work')
  if (activitiesSection.value?.isVisible) activeIds.push('activities')
  if (skillsSection.value?.isVisible) activeIds.push('skills')
  if (hobbiesSection.value?.isVisible || awardsSection.value?.isVisible) activeIds.push('footer')

  unmappedSections.value.forEach(s => {
    if (s.isVisible) activeIds.push(s.id)
  })

  const sourceOrder = props.resumeData?.sections?.map(s => s.id) || []
  return activeIds.sort((a, b) => {
    const mapToSource = (gid) => {
      if (gid === 'edu_cert') return sourceOrder.includes('education') ? 'education' : 'certifications'
      if (gid === 'work') return sourceOrder.includes('experience') ? 'experience' : 'projects'
      if (gid === 'footer') return sourceOrder.includes('hobbies') ? 'hobbies' : 'awards'
      return gid
    }
    const idxA = sourceOrder.indexOf(mapToSource(a))
    const idxB = sourceOrder.indexOf(mapToSource(b))
    return (idxA > -1 ? idxA : 99) - (idxB > -1 ? idxB : 99)
  })
})

const getOrder = (id, arr) => arr.indexOf(id) + 1

const toggleSection = (id) => {
  selectedSectionId.value = selectedSectionId.value === id ? null : id
}

const moveUp = (id, arr) => emit('moveUp', id, toRaw(arr))
const moveDown = (id, arr) => emit('moveDown', id, toRaw(arr))

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

.pastel-block { background: #EFECE9; padding: 20px 25px; border-radius: 4px; margin-bottom: 18px; position: relative; }

.section-title { font-size: 15px; font-weight: 700; color: #0056b3; margin: 0 !important; letter-spacing: 0.5px; text-transform: uppercase; }
.double-line { height: 4px; margin: 6px 0 15px 0; display: flex; flex-direction: column; gap: 1px; }
.line-blue { height: 1.5px; background: #0056b3; width: 100%; }
.line-red  { height: 1.5px; background: #D6624B; width: 100%; }

.contact-block { display: flex; justify-content: space-around; padding: 12px; }
.contact-item { display: flex; align-items: center; gap: 8px; font-size: 11px; font-weight: 600; }
.contact-item i { color: #D6624B; font-size: 13px; }

.profile-block { display: flex; gap: 30px; align-items: flex-start; }
.profile-left { flex: 1; }
.fullname { font-size: 28px; font-weight: 700; color: #4A2B28; margin: 0 0 8px 0; text-transform: uppercase; }
.job-title-wrapper { display: flex; align-items: center; gap: 12px; margin-bottom: 10px; }
.job-title { font-size: 15px; font-weight: 700; color: #111; text-transform: uppercase; }
.title-line-accent { height: 2px; background: #D6624B; width: 80px; }
.summary-text { font-size: 12.5px; line-height: 1.5; color: #444; }

.profile-right { position: relative; width: 130px; height: 150px; flex-shrink: 0; padding: 8px 0 0 8px; }
.avatar-accent { position: absolute; top: 0; left: 0; width: 60px; height: 60px; background: #D6624B; z-index: 1; border-radius: 2px; }
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
.exp-header-wrap::after { content: ''; position: absolute; left: -5.5px; top: 5px; width: 10px; height: 10px; border-radius: 50%; background: #D6624B; }
.date-badge { width: 85px; flex-shrink: 0; font-weight: 700; font-size: 12.5px; color: #444; margin-top: 2px; text-align: left; }
.exp-content-wrap { flex: 1; display: flex; flex-direction: column; gap: 4px; padding-right: 15px; }

.info-name, .company-name { font-size: 13px; font-weight: 700; color: #000; text-transform: uppercase; }
.info-role { font-size: 13px; font-weight: 700; color: #111; }
.desc-text, .exp-desc { padding-left: 115px; margin-top: 6px; padding-right: 15px; }

/* ────────────────────────────────────────────────────────── */

.skill-ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
.skill-ul li { font-size: 13px; line-height: 1.6; margin-bottom: 8px; color: #111; padding-left: 10px; position: relative; }
.skill-ul li::before { content: '•'; position: absolute; left: 0; color: #0056b3; }

.bottom-split { display: grid; grid-template-columns: 1fr 1fr; gap: 30px; }
.hobbies-ul, .awards-ul { list-style: none !important; padding: 0 !important; margin: 0 !important; }
.hobbies-ul li, .awards-ul li { font-size: 12.5px; color: #222; margin-bottom: 6px; position: relative; display: inline-block; margin-right: 15px; }

.section-block { border: 2px solid transparent; cursor: pointer; transition: 0.2s; }
.section-active { border-color: #0056b3 !important; box-shadow: 0 0 10px rgba(0,86,179,0.1); z-index: 10; }
.nav-btns { position: absolute; top: -14px; right: 10px; display: flex; gap: 5px; z-index: 100; }
.nav-btn { background: #0056b3; color: #fff; border: none; width: 24px; height: 24px; border-radius: 3px; cursor: pointer; font-size: 12px; display: flex; align-items: center; justify-content: center; }
.nav-btn:hover { background: #003d82; }
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