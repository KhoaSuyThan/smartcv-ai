<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="cv-green-wrapper"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm` }"
    @click.self="selectedSectionId = null"
  >
    <div class="cv-container">
      
      <aside class="left-column">
        
        <div class="profile-section relative">
          <h1 class="fullname paginated-item">{{ !isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'Vũ Hoàng Việt' }}</h1>
          <div class="job-title paginated-item">{{ !isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'Thực tập sinh Kiểm toán' }}</div>
          
          <div class="avatar-container paginated-item">
            <img v-if="resumeData.general.avatarUrl || resumeData.general.avatar" :src="resumeData.general.avatarUrl || resumeData.general.avatar" class="avatar-img" alt="Avatar"/>
            <div v-else class="avatar-placeholder">
              <svg width="60" height="60" viewBox="0 0 24 24" fill="none" stroke="#ccc" stroke-width="1.5">
                <circle cx="12" cy="8" r="4"/><path d="M4 20c0-4 3.6-7 8-7s8 3 8 7"/>
              </svg>
            </div>
          </div>

          <div class="contact-list">
            <div class="contact-item paginated-item">
              <i class="far fa-calendar"></i> <span>{{ !isEmpty(resumeData.general.birthDate) ? resumeData.general.birthDate : '06/12/2003' }}</span>
            </div>
            <div class="contact-item paginated-item">
              <i class="fas fa-mobile-alt"></i> <span>{{ !isEmpty(resumeData.general.phone) ? resumeData.general.phone : '(034) 612 6612' }}</span>
            </div>
            <div class="contact-item paginated-item">
              <i class="far fa-envelope"></i> <span>{{ !isEmpty(resumeData.general.email) ? resumeData.general.email : 'hotro@topcv.vn' }}</span>
            </div>
            <div class="contact-item paginated-item">
              <i class="far fa-paper-plane"></i> <span>{{ !isEmpty(resumeData.general.address) ? resumeData.general.address : 'Thanh Xuân, Hà Nội' }}</span>
            </div>
          </div>
        </div>

        <div class="left-sortable-area" style="display: flex; flex-direction: column;">
          
          <template v-for="section in sidebarSections.filter(s => s.id === 'summary')" :key="section.id">
            <div v-show="section.isVisible" class="section-block left-block" :style="{ order: getOrder(section.id, leftIds) }" :class="{ 'section-active': selectedSectionId === section.id }" @click.stop="toggleSection(section.id)">
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
                  <button class="nav-btn" @click.stop="moveUp(section.id, leftIds)"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop="moveDown(section.id, leftIds)"><i class="fas fa-chevron-down"></i></button>
                </div>
              </transition>
              
              <div class="pill-header paginated-item">{{ section.name || section.title || 'Mục tiêu nghề nghiệp' }}</div>
              
              <div class="content-area">
                <div v-if="!isEmpty(resumeData.general.summary)" class="html-content" v-html="formatDesc(resumeData.general.summary)"></div>
                <div v-else class="html-content text-placeholder">
                  <div class="paginated-item">Chưa có dữ liệu</div>
                </div>
              </div>
            </div>
          </template>

          <template v-for="section in sidebarSections.filter(s => ['skills', 'it_skills', 'languages'].includes(s.id))" :key="section.id">
            <div v-show="section.isVisible" class="section-block left-block" :style="{ order: getOrder(section.id, leftIds) }" :class="{ 'section-active': selectedSectionId === section.id }" @click.stop="toggleSection(section.id)">
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
                  <button class="nav-btn" @click.stop="moveUp(section.id, leftIds)"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop="moveDown(section.id, leftIds)"><i class="fas fa-chevron-down"></i></button>
                </div>
              </transition>

              <div class="pill-header paginated-item">{{ section.name || section.title || 'Kỹ năng chuyên môn' }}</div>
              
              <div class="content-area">
                <ul class="left-list">
                  <li v-for="(item, i) in (section.items?.length ? section.items : [{name: 'Chưa có dữ liệu'}])" :key="i" class="item-container paginated-item">
                    <template v-if="item.name">{{ item.name }} {{ item.level ? ' - ' + item.level : '' }}</template>
                    <template v-else>{{ item }}</template>
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop="$emit('removeItem', section.id, i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                  </li>
                </ul>
              </div>
            </div>
          </template>

          <template v-for="section in sidebarSections.filter(s => s.id === 'hobbies')" :key="section.id">
            <div v-show="section.isVisible" class="section-block left-block" :style="{ order: getOrder(section.id, leftIds) }" :class="{ 'section-active': selectedSectionId === section.id }" @click.stop="toggleSection(section.id)">
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
                  <button class="nav-btn" @click.stop="moveUp(section.id, leftIds)"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop="moveDown(section.id, leftIds)"><i class="fas fa-chevron-down"></i></button>
                </div>
              </transition>

              <div class="pill-header paginated-item">{{ section.name || section.title || 'Sở thích' }}</div>
              
              <div class="content-area">
                <ul class="left-list">
                  <li v-for="(item, i) in (section.items?.length ? section.items : [{name: 'Chưa có dữ liệu'}])" :key="i" class="item-container paginated-item">
                    {{ item.name || item.title || item }}
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop="$emit('removeItem', section.id, i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                  </li>
                </ul>
              </div>
            </div>
          </template>

        </div>
      </aside>

      <main class="right-column relative">
        <div class="right-sortable-area timeline-container" style="display: flex; flex-direction: column;">
          
          <div v-show="rightIds.length > 0" class="timeline-line no-print-line"></div>

          <template v-for="section in mainSections.filter(s => s.id === 'education')" :key="section.id">
            <div v-show="section.isVisible" class="section-block right-block" :style="{ order: getOrder(section.id, rightIds) }" :class="{ 'section-active': selectedSectionId === section.id }" @click.stop="toggleSection(section.id)">
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print" style="top: -10px;">
                  <button class="nav-btn" @click.stop="moveUp(section.id, rightIds)"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop="moveDown(section.id, rightIds)"><i class="fas fa-chevron-down"></i></button>
                </div>
              </transition>

              <div class="timeline-dot"></div>
              <div class="right-block-inner">
                <h3 class="right-title paginated-item">{{ section.name || section.title || 'Quá trình Học vấn' }}</h3>
                
                <div class="green-card">
                  <div v-for="(item, i) in (section.items?.length ? section.items : [{school: 'Chưa có dữ liệu'}])" :key="i" class="item-container relative mb-4 last:mb-0">
                    <div v-if="item.year || item.time" class="paginated-item text-time">{{ item.year || item.time }}</div>
                    <div v-if="item.school || item.name" class="paginated-item text-entity">{{ item.school || item.name }}</div>
                    <div v-if="item.major || item.degree" class="paginated-item text-role">{{ item.major || item.degree }}</div>
                    
                    <div v-if="item.gradType || item.info" class="paginated-item text-role" style="margin-top: 2px;">
                      <strong>Xếp loại:</strong> {{ item.gradType || item.info }}
                    </div>

                    <div v-if="item.desc || item.description" class="html-content mt-2" v-html="formatDesc(item.desc || item.description)"></div>
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop="$emit('removeItem', section.id, i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                  </div>
                </div>
              </div>
            </div>
          </template>

          <template v-for="section in mainSections.filter(s => s.id === 'experience')" :key="section.id">
            <div v-show="section.isVisible" class="section-block right-block" :style="{ order: getOrder(section.id, rightIds) }" :class="{ 'section-active': selectedSectionId === section.id }" @click.stop="toggleSection(section.id)">
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print" style="top: -10px;">
                  <button class="nav-btn" @click.stop="moveUp(section.id, rightIds)"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop="moveDown(section.id, rightIds)"><i class="fas fa-chevron-down"></i></button>
                </div>
              </transition>

              <div class="timeline-dot"></div>
              <div class="right-block-inner">
                <h3 class="right-title paginated-item">{{ section.name || section.title || 'Kinh nghiệm làm việc' }}</h3>
                
                <div class="green-card">
                  <div v-for="(item, i) in (section.items?.length ? section.items : [{company: 'Chưa có dữ liệu'}])" :key="i" class="item-container relative mb-4 last:mb-0">
                    <div v-if="item.year || item.time" class="paginated-item text-time">{{ item.year || item.time }}</div>
                    <div v-if="item.company || item.name" class="paginated-item text-entity font-normal">{{ item.company || item.name }}</div>
                    <div v-if="item.role || item.position" class="paginated-item text-role font-bold">{{ item.role || item.position }}</div>
                    <div v-if="item.role || item.position || item.desc || item.description" class="divider-line paginated-item"></div>
                    <div v-if="item.desc || item.description" class="html-content mt-2" v-html="formatDesc(item.desc || item.description)"></div>
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop="$emit('removeItem', section.id, i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                  </div>
                </div>
              </div>
            </div>
          </template>

          <template v-for="section in mainSections.filter(s => s.id === 'activities')" :key="section.id">
            <div v-show="section.isVisible" class="section-block right-block" :style="{ order: getOrder(section.id, rightIds) }" :class="{ 'section-active': selectedSectionId === section.id }" @click.stop="toggleSection(section.id)">
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print" style="top: -10px;">
                  <button class="nav-btn" @click.stop="moveUp(section.id, rightIds)"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop="moveDown(section.id, rightIds)"><i class="fas fa-chevron-down"></i></button>
                </div>
              </transition>

              <div class="timeline-dot"></div>
              <div class="right-block-inner">
                <h3 class="right-title paginated-item">{{ section.name || section.title || 'Hoạt động' }}</h3>
                
                <div class="green-card">
                  <div v-for="(item, i) in (section.items?.length ? section.items : [{company: 'Chưa có dữ liệu'}])" :key="i" class="item-container relative mb-5 last:mb-0">
                    <div v-if="item.year || item.time || item.date" class="paginated-item text-time">{{ item.year || item.time || item.date }}</div>
                    <div v-if="item.company || item.organization || item.name" class="paginated-item text-entity">{{ item.company || item.organization || item.name }}</div>
                    <div v-if="item.role" class="paginated-item text-role font-normal mt-1">{{ item.role }}</div>
                    <div v-if="item.role || item.desc || item.description" class="divider-line paginated-item"></div>
                    <div v-if="item.desc || item.description" class="html-content mt-2" v-html="formatDesc(item.desc || item.description)"></div>
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop="$emit('removeItem', section.id, i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                  </div>
                </div>
              </div>
            </div>
          </template>

          <template v-for="section in mainSections.filter(s => !['education', 'experience', 'activities'].includes(s.id))" :key="section.id">
            <div v-show="section.isVisible" class="section-block right-block" :style="{ order: getOrder(section.id, rightIds) }" :class="{ 'section-active': selectedSectionId === section.id }" @click.stop="toggleSection(section.id)">
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print" style="top: -10px;">
                  <button class="nav-btn" @click.stop="moveUp(section.id, rightIds)"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop="moveDown(section.id, rightIds)"><i class="fas fa-chevron-down"></i></button>
                </div>
              </transition>

              <div class="timeline-dot"></div>
              <div class="right-block-inner">
                <h3 class="right-title paginated-item">{{ section.name || section.title || section.id.toUpperCase() }}</h3>
                
                <div class="green-card">
                  <div v-if="section.desc || section.description || section.content || section.value" class="html-content mb-3" v-html="formatDesc(section.desc || section.description || section.content || section.value)"></div>
                  
                  <div v-for="(item, i) in (section.items?.length ? section.items : ((section.desc || section.description || section.content || section.value) ? [] : [{ name: 'Chưa có dữ liệu' }]))" :key="i" class="item-container relative mb-4 last:mb-0">
                    <template v-if="typeof item === 'object'">
                      <div v-if="item.year || item.time" class="paginated-item text-time">{{ item.year || item.time }}</div>
                      <div v-if="item.name && /<[a-z][\s\S]*>/i.test(item.name)" class="html-content" v-html="formatDesc(item.name)"></div>
                      <div v-else-if="item.name || item.title" class="paginated-item text-entity">{{ item.name || item.title }}</div>
                      <div v-if="item.role || item.position || item.level" class="paginated-item text-role font-normal mt-1">{{ item.role || item.position || item.level }}</div>
                      <div v-if="item.info || item.contact" class="html-content" v-html="formatDesc(item.info || item.contact)"></div>
                      <div v-if="item.desc || item.description" class="html-content mt-2" v-html="formatDesc(item.desc || item.description)"></div>
                    </template>
                    <template v-else>
                      <div class="html-content" v-html="formatDesc(item)"></div>
                    </template>
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop="$emit('removeItem', section.id, i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                  </div>
                </div>
              </div>
            </div>
          </template>

        </div>
      </main>

    </div>

    <template v-for="p in (pageCount - 1)" :key="'pb-' + p">
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

// ── QUẢN LÝ ẨN/HIỆN & CHIA CỘT ──────────────────────────────────────
const leftColKeys = ['summary', 'skills', 'it_skills', 'languages', 'hobbies']

const sidebarSections = computed(() => {
  return (props.resumeData?.sections || []).filter(s => leftColKeys.includes(s.id))
})

const mainSections = computed(() => {
  return (props.resumeData?.sections || []).filter(s => !leftColKeys.includes(s.id))
})

const getActiveIds = (sourceArray) => {
  return sourceArray.filter(s => s.isVisible).map(s => s.id)
}

const leftIds = computed(() => getActiveIds(sidebarSections.value))
const rightIds = computed(() => getActiveIds(mainSections.value))

const getOrder = (id, activeArray) => activeArray.indexOf(id) + 1

const toggleSection = (id) => { selectedSectionId.value = selectedSectionId.value === id ? null : id }
const moveUp = (id, arr) => emit('moveUp', id, toRaw(arr))
const moveDown = (id, arr) => emit('moveDown', id, toRaw(arr))

// Kích hoạt mặc định 5 mục giống ảnh
onMounted(() => {
  if (props.resumeData?.sections) {
    const ACTIVE_SECTIONS = ['summary', 'skills', 'experience', 'education', 'activities']
    props.resumeData.sections.forEach(sec => {
      if (sec.isVisible === undefined) {
        sec.isVisible = ACTIVE_SECTIONS.includes(sec.id)
      }
    })
  }
  requestPagination()
  window.addEventListener('resize', requestPagination)
})

// ── UTILITIES & HTML FORMAT ──────────────────────────────────────────────
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

// ── PAGINATION LOGIC (SPACER) ──────────────────────────────────
const A4_H_MM = 297
let paginateTimer = null

const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 100)
}

const doPagination = async () => {
  if (!cvRoot.value) return
  
  // Xóa toàn bộ spacer cũ
  const oldSpacers = cvRoot.value.querySelectorAll('.page-spacer')
  oldSpacers.forEach(el => el.remove())
  
  await nextTick()

  const pxPerMm = cvRoot.value.offsetWidth / 210
  const pageH = A4_H_MM * pxPerMm
  const safeBottom = 15 * pxPerMm

  const getTop = (el) => {
    let offset = 0, curr = el
    while (curr && curr !== cvRoot.value) { offset += curr.offsetTop; curr = curr.offsetParent }
    return offset
  }

  const allEls = cvRoot.value.querySelectorAll('.paginated-item')
  
  for (let i = 0; i < allEls.length; i++) {
    const el = allEls[i]
    if (!el.offsetHeight) continue
    
    const top = getTop(el)
    const bottomInPage = (top % pageH) + el.offsetHeight
    
    // Nếu bị tràn lề dưới, bơm 1 khối tàng hình để ngắt trang
    if (bottomInPage > pageH - safeBottom) {
      const gapToNextPage = pageH - (top % pageH)
      const spacer = document.createElement('div')
      spacer.className = 'page-spacer'
      spacer.style.height = `${gapToNextPage + 10}px`
      spacer.style.width = '100%'
      el.parentNode.insertBefore(spacer, el)
      await nextTick() 
    }
  }

  let maxB = 0
  cvRoot.value.querySelectorAll('.paginated-item').forEach(el => { 
    maxB = Math.max(maxB, getTop(el) + el.offsetHeight) 
  })
  pageCount.value = Math.max(1, Math.ceil(maxB / pageH))
}

watch(() => props.resumeData, requestPagination, { deep: true })
onUnmounted(() => {
  window.removeEventListener('resize', requestPagination)
  if (paginateTimer) clearTimeout(paginateTimer)
})
</script>

<style scoped>
@import url('https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css');
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');

.cv-green-wrapper { 
  width: 210mm; background: #fff; font-family: 'Inter', sans-serif; 
  position: relative; color: #333; overflow: hidden; box-sizing: border-box;
}
.cv-container { display: flex; width: 100%; min-height: 100%; }

/* ================== LEFT COLUMN ================== */
.left-column { width: 40%; padding: 40px 30px; background: #fff; }
.fullname { font-size: 32px; font-weight: 700; color: #43936C; line-height: 1.2; margin-bottom: 5px; }
.job-title { font-size: 16px; font-weight: 600; color: #4A5568; margin-bottom: 30px; }

.avatar-container { width: 170px; height: 170px; margin: 0 auto 30px auto; border-radius: 50%; overflow: hidden; border: 4px solid #fff; box-shadow: 0 4px 10px rgba(0,0,0,0.1); }
.avatar-img { width: 100%; height: 100%; object-fit: cover; }
.avatar-placeholder { width: 100%; height: 100%; background: #f0f0f0; display: flex; align-items: center; justify-content: center; }

.contact-list { margin-bottom: 40px; }
.contact-item { display: flex; align-items: center; gap: 12px; font-size: 12px; font-weight: 500; color: #4A5568; padding: 12px 0; border-bottom: 1px solid #E2E8F0; }
.contact-item:last-child { border-bottom: none; }
.contact-item i { width: 16px; text-align: center; font-size: 14px; }

.left-block { margin-bottom: 25px; position: relative; }
.pill-header { background: #43936C; color: #fff; font-size: 15px; font-weight: 700; text-align: center; padding: 8px 0; border-radius: 20px; margin-bottom: 15px; text-transform: uppercase; }
.text-placeholder { font-size: 12.5px; color: #4A5568; text-align: justify; line-height: 1.6; }

.left-list { list-style: none; padding: 0; margin: 0; padding-left: 10px; }
.left-list li { position: relative; font-size: 12.5px; color: #333; font-weight: 600; margin-bottom: 10px; line-height: 1.5; }
.left-list li::before { content: ''; position: absolute; left: -10px; top: 6px; width: 4px; height: 4px; background: #333; border-radius: 50%; }

/* ================== RIGHT COLUMN ================== */
.right-column { width: 60%; padding: 55px 30px 40px 10px !important; } 

.timeline-container { position: relative; padding-left: 30px; height: 100%; }

.timeline-line { 
    position: absolute; 
    left: 4px; 
    top: 12px !important; 
    bottom: 20px; 
    width: 2px; 
    background: #4A5568; 
    z-index: 0; 
}

.right-block { position: relative; margin-bottom: 35px; z-index: 10; }

.timeline-dot {
    position: absolute;
    left: -30px; 
    top: 6px; 
    width: 10px; 
    height: 10px;
    background: #4A5568; 
    border-radius: 50%; 
    z-index: 1;
}

.right-block-inner { padding-left: 35px; }

.right-title { font-size: 18px; font-weight: 700; color: #2D3748; margin: 0 0 15px 0; }

.green-card { background: #E2EFE6; border-radius: 12px; padding: 16px 20px; position: relative; z-index: 10; }

.text-time { font-size: 13px; font-weight: 600; color: #4A5568; margin-bottom: 4px; }
.text-entity { font-size: 14.5px; font-weight: 700; color: #1A202C; margin-bottom: 4px; }
.text-role { font-size: 13px; font-weight: 600; color: #4A5568; }
.divider-line { width: 30px; height: 2px; background: #4A5568; margin: 8px 0; }

/* ================== HTML CONTENT & PAGINATION ================== */
:deep(.html-content) { font-size: 12.5px; line-height: 1.6; color: #333; text-align: justify; }
:deep(.html-content ul) { list-style-type: none !important; padding-left: 12px !important; margin: 0; }
:deep(.html-content li) { position: relative; margin-bottom: 4px; font-size: 12.5px; line-height: 1.6; }
:deep(.html-content li::before) { content: '•'; position: absolute; left: -12px; color: #333; }
:deep(.html-content b), :deep(.html-content strong) { font-weight: 700 !important; }

/* INTERACTION & BUTTONS */
.section-block { border: 2px solid transparent; transition: 0.2s; border-radius: 8px; }
.section-active { border-color: #43936C !important; box-shadow: 0 0 10px rgba(67, 147, 108, 0.2); z-index: 20; }
.nav-btns { position: absolute; right: 5px; top: -10px; display: flex; gap: 5px; z-index: 100; }
.nav-btn { background: #43936C; color: #fff; border: none; width: 24px; height: 24px; border-radius: 4px; cursor: pointer; display: flex; align-items: center; justify-content: center; font-size: 12px; }
.nav-btn:hover { background: #2f6b4e; }
.delete-item-btn { position: absolute; right: -5px; top: -5px; width: 18px; height: 18px; background: #ff4d4f; color: white; border: none; border-radius: 50%; cursor: pointer; font-size: 10px; display: flex; align-items: center; justify-content: center; z-index: 50; }

.page-break-indicator { position: absolute; left: 0; width: 100%; z-index: 50; display: flex; flex-direction: column; align-items: center; pointer-events: none; }
.page-break-bar { width: 100%; height: 2px; background: rgba(0,0,0,0.1); border-top: 1px dashed rgba(0,0,0,0.2); }
.page-break-label { font-size: 9px; text-transform: uppercase; font-weight: 700; color: #999; background: #fff; padding: 2px 10px; margin-top: -8px; }

@media print { .no-print { display: none !important; } }
</style>