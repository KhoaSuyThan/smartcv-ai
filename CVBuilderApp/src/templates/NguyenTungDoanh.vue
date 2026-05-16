<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="cv-blue-orange-wrapper"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm` }"
    @click.self="selectedSectionId = null"
  >
    <div class="cv-container">
      
      <header class="header-card paginated-item">
        <div class="avatar-container">
          <img v-if="resumeData.general.avatarUrl || resumeData.general.avatar" :src="resumeData.general.avatarUrl || resumeData.general.avatar" class="avatar-img" alt="Avatar"/>
          <div v-else class="avatar-placeholder">
            <svg width="50" height="50" viewBox="0 0 24 24" fill="none" stroke="#a0aec0" stroke-width="1.5">
              <circle cx="12" cy="8" r="4"/><path d="M4 20c0-4 3.6-7 8-7s8 3 8 7"/>
            </svg>
          </div>
        </div>
        <div class="header-info">
          <h1 class="fullname">{{ !isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'NGUYỄN TÙNG DOANH' }}</h1>
          <p class="job-title">{{ !isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'Designer' }}</p>
        </div>
      </header>

      <div class="main-layout">
        
        <aside class="left-column">
          
          <!-- CONTACT INFO (Dynamic) -->
          <div
            v-if="contactItems.length > 0"
            class="custom-card relative contact-block"
            :class="{ 'contact-active': selectedSectionId === 'contact' }"
            @click.stop="toggleSection('contact')"
          >
            <div class="card-badge paginated-item">Thông tin cá nhân</div>
            <div class="card-body">
              <div v-for="(ci, ciIdx) in contactItems" :key="ci.key"
                  class="contact-item paginated-item relative contact-item-container">
                <div class="icon-circle" v-html="ci.icon"></div>
                <span v-html="ci.value"></span>
                
                <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print">
                    <button v-if="ciIdx > 0" @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn" title="Di chuyển lên">
                        <i class="fas fa-chevron-up"></i>
                    </button>
                    <button v-if="ciIdx < contactItems.length - 1" @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn" title="Di chuyển xuống">
                        <i class="fas fa-chevron-down"></i>
                    </button>
                    <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn-danger" title="Xóa mục này">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
              </div>
            </div>
          </div>

          <div class="left-sortable-area" style="display: flex; flex-direction: column;">
            
            <template v-for="section in sidebarSections" :key="section.id">
              <div v-show="section.isVisible" class="custom-card section-block"
                   :style="{ order: getOrder(section.id, leftIds) }"
                   :class="{ 'section-active': selectedSectionId === section.id }"
                   @click.stop="toggleSection(section.id)">
                
                <transition name="fade-btns">
                  <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
                    <button class="nav-btn" @click.stop="moveUp(section.id, leftIds)"><i class="fas fa-chevron-up"></i></button>
                    <button class="nav-btn" @click.stop="moveDown(section.id, leftIds)"><i class="fas fa-chevron-down"></i></button>
                    <button class="nav-btn" @click.stop="moveHorizontal(section.id)"><i class="fas fa-chevron-right"></i></button>
                    <button class="nav-btn nav-btn-danger" @click.stop="section.isVisible = false"><i class="fas fa-times"></i></button>
                  </div>
                </transition>

                <div class="card-badge paginated-item">{{ section.name || section.title || section.id }}</div>
                
                <div class="card-body">
                  <ul class="bullet-list" v-if="section.items?.length">
                    <li v-for="(item, i) in section.items" :key="i" class="item-container paginated-item">
                      <div class="flex flex-col">
                        <span class="font-bold text-[#FF9500]" v-if="item.time || item.year">{{ item.time || item.year }}</span>
                        <span class="font-semibold text-[#2B4C7E]" v-else-if="item.name && item.level">{{ item.name }}</span>
                        <span class="text-[#4A5568]" v-else>{{ item.name || item.title || item }}</span>
                        
                        <span class="text-[#4A5568] text-xs mt-0.5" v-if="item.name && item.level">• {{ item.level }}</span>
                        <span class="text-[#4A5568] text-xs mt-0.5" v-if="(item.time || item.year) && item.name">{{ item.name }}</span>
                      </div>
                      <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop="$emit('removeItem', section.id, i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                    </li>
                  </ul>
                </div>
              </div>
            </template>
          </div>
        </aside>

        <main class="right-column">
          <div class="right-sortable-area" style="display: flex; flex-direction: column;">
            
            <template v-for="section in mainSections.filter(s => s.id === 'summary')" :key="section.id">
              <div v-show="section.isVisible" class="custom-card section-block"
                   :style="{ order: getOrder(section.id, rightIds) }"
                   :class="{ 'section-active': selectedSectionId === section.id }"
                   @click.stop="toggleSection(section.id)">
                
                <transition name="fade-btns">
                  <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
                    <button class="nav-btn" @click.stop="moveHorizontal(section.id)"><i class="fas fa-chevron-left"></i></button>
                    <button class="nav-btn" @click.stop="moveUp(section.id, rightIds)"><i class="fas fa-chevron-up"></i></button>
                    <button class="nav-btn" @click.stop="moveDown(section.id, rightIds)"><i class="fas fa-chevron-down"></i></button>
                    <button class="nav-btn nav-btn-danger" @click.stop="section.isVisible = false"><i class="fas fa-times"></i></button>
                  </div>
                </transition>

                <div class="card-badge paginated-item">{{ section.name || section.title || 'Giới thiệu' }}</div>
                
                <div class="card-body">
                  <div v-if="section.desc || section.description || section.content || section.value || !isEmpty(resumeData.general.summary)" 
                       class="html-content" v-html="formatDesc(section.desc || section.description || section.content || section.value || resumeData.general.summary)"></div>
                </div>
              </div>
            </template>

            <template v-for="section in mainSections.filter(s => ['experience', 'education', 'activities'].includes(s.id))" :key="section.id">
              <div v-show="section.isVisible" class="custom-card section-block"
                   :style="{ order: getOrder(section.id, rightIds) }"
                   :class="{ 'section-active': selectedSectionId === section.id }"
                   @click.stop="toggleSection(section.id)">
                
                <transition name="fade-btns">
                  <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
                    <button class="nav-btn" @click.stop="moveHorizontal(section.id)"><i class="fas fa-chevron-left"></i></button>
                    <button class="nav-btn" @click.stop="moveUp(section.id, rightIds)"><i class="fas fa-chevron-up"></i></button>
                    <button class="nav-btn" @click.stop="moveDown(section.id, rightIds)"><i class="fas fa-chevron-down"></i></button>
                    <button class="nav-btn nav-btn-danger" @click.stop="section.isVisible = false"><i class="fas fa-times"></i></button>
                  </div>
                </transition>

                <div class="card-badge paginated-item">{{ section.name || section.title || section.id.toUpperCase() }}</div>
                
                <div class="card-body">
                  <div v-if="section.desc || section.description || section.content || section.value" class="html-content mb-3" v-html="formatDesc(section.desc || section.description || section.content || section.value)"></div>
                  
                  <div class="timeline-wrapper" v-if="section.items?.length">
                    <div v-for="(item, i) in section.items" :key="i" class="timeline-item item-container">
                      
                      <div class="timeline-icon no-print-bg">
                        <i v-if="section.id === 'education'" class="fas fa-graduation-cap"></i>
                        <i v-else-if="section.id === 'activities'" class="fas fa-users"></i>
                        <i v-else class="fas fa-briefcase"></i>
                      </div>

                      <div class="paginated-item entry-header">
                        <span class="entry-entity">{{ item.company || item.school || item.organization || item.name }}</span>
                        <span class="entry-divider" v-if="(item.company || item.school || item.organization || item.name) && (item.time || item.year || item.date)">|</span>
                        <span class="entry-time">{{ item.time || item.year || item.date }}</span>
                      </div>
                      
                      <div class="paginated-item entry-role" v-if="item.role || item.position || item.major">
                        {{ item.role || item.position || item.major }}
                      </div>

                      <div class="paginated-item entry-role text-[13px]" v-if="item.gradType || item.info">
                        <strong class="text-[#2B4C7E]">Xếp loại:</strong> {{ item.gradType || item.info }}
                      </div>

                      <div class="html-content mt-2" v-if="item.desc || item.description" v-html="formatDesc(item.desc || item.description)"></div>
                      
                      <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop="$emit('removeItem', section.id, i)" class="delete-item-btn no-print" style="top: 0; right: 0;"><i class="fas fa-times"></i></button>
                    </div>
                  </div>
                </div>
              </div>
            </template>

            <template v-for="section in mainSections.filter(s => !['summary', 'experience', 'education', 'activities'].includes(s.id))" :key="section.id">
              <div v-show="section.isVisible" class="custom-card section-block"
                   :style="{ order: getOrder(section.id, rightIds) }"
                   :class="{ 'section-active': selectedSectionId === section.id }"
                   @click.stop="toggleSection(section.id)">
                
                <transition name="fade-btns">
                  <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
                    <button class="nav-btn" @click.stop="moveHorizontal(section.id)"><i class="fas fa-chevron-left"></i></button>
                    <button class="nav-btn" @click.stop="moveUp(section.id, rightIds)"><i class="fas fa-chevron-up"></i></button>
                    <button class="nav-btn" @click.stop="moveDown(section.id, rightIds)"><i class="fas fa-chevron-down"></i></button>
                    <button class="nav-btn nav-btn-danger" @click.stop="section.isVisible = false"><i class="fas fa-times"></i></button>
                  </div>
                </transition>

                <div class="card-badge paginated-item">{{ section.name || section.title || section.id.toUpperCase() }}</div>
                
                <div class="card-body">
                  <div v-if="section.desc || section.description || section.content || section.value" class="html-content mb-3" v-html="formatDesc(section.desc || section.description || section.content || section.value)"></div>
                  
                  <div v-if="section.items?.length">
                    <div v-for="(item, i) in section.items" :key="i" class="item-container relative mb-4 last:mb-0">
                      <template v-if="typeof item === 'object'">
                        <div v-if="item.year || item.time" class="paginated-item entry-time mb-1 inline-block">{{ item.year || item.time }}</div>
                        
                        <div v-if="item.name && /<[a-z][\s\S]*>/i.test(item.name)" class="html-content" v-html="formatDesc(item.name)"></div>
                        <div v-else-if="item.name || item.title" class="paginated-item entry-entity">{{ item.name || item.title }}</div>
                        
                        <div v-if="item.role || item.position || item.level" class="paginated-item entry-role mt-1">{{ item.role || item.position || item.level }}</div>
                        <div v-if="item.info || item.contact" class="html-content mt-1" v-html="formatDesc(item.info || item.contact)"></div>
                        <div v-if="item.desc || item.description" class="html-content mt-1" v-html="formatDesc(item.desc || item.description)"></div>
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

const emit = defineEmits(['moveUp', 'moveDown', 'removeItem', 'moveHorizontal'])

const isEmpty = (v) => !v || v.toString().trim() === ''

// ── DATA MẪU RÚT GỌN ──────────────────────────────────────
const getMockData = (sectionId) => {
  return []
}

// ── QUẢN LÝ ẨN/HIỆN & CHIA CỘT ──────────────────────────────────────
const leftColKeys = computed(() => props.resumeData.leftColKeys || ['skills', 'it_skills', 'languages', 'awards', 'hobbies'])

const sidebarSections = computed(() => (props.resumeData?.sections || []).filter(s => leftColKeys.value.includes(s.id)))
const mainSections = computed(() => (props.resumeData?.sections || []).filter(s => !leftColKeys.value.includes(s.id)))

const getActiveIds = (sourceArray) => sourceArray.filter(s => s.isVisible).map(s => s.id)

const leftIds = computed(() => getActiveIds(sidebarSections.value))
const rightIds = computed(() => getActiveIds(mainSections.value))

const getOrder = (id, activeArray) => activeArray.indexOf(id) + 1

const toggleSection = (id) => { selectedSectionId.value = selectedSectionId.value === id ? null : id }
const moveUp = (id, arr) => emit('moveUp', id, toRaw(arr))
const moveDown = (id, arr) => emit('moveDown', id, toRaw(arr))
const moveHorizontal = (id) => emit('moveHorizontal', id)

// --- CONTACT ITEMS: Danh sách động có thể sắp xếp / ẩn ---
const contactIcons = {
  birthDate: '<i class="fas fa-calendar-alt"></i>',
  phone: '<i class="fas fa-phone-alt"></i>',
  email: '<i class="fas fa-envelope"></i>',
  address: '<i class="fas fa-home"></i>',
  website: '<i class="fas fa-link"></i>'
}

const contactOrder = ref(['birthDate', 'phone', 'email', 'address', 'website'])
const hiddenContacts = ref([])

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  switch (key) {
    case 'birthDate': return isEmpty(g.birthDate) ? '' : g.birthDate
    case 'phone': return isEmpty(g.phone) ? '' : g.phone
    case 'email': return isEmpty(g.email) ? '' : g.email
    case 'address': return isEmpty(g.address) ? '' : g.address
    case 'website': return isEmpty(g.website) ? '' : g.website
    default: return ''
  }
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
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && getContactValue(k) !== '')
  if (idx <= 0) return
  const keyA = visible[idx], keyB = visible[idx - 1]
  const idxA = contactOrder.value.indexOf(keyA), idxB = contactOrder.value.indexOf(keyB)
  const arr = [...contactOrder.value]
  ;[arr[idxA], arr[idxB]] = [arr[idxB], arr[idxA]]
  contactOrder.value = arr
}

const moveContactDown = (idx) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && getContactValue(k) !== '')
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

onMounted(() => {
  if (props.resumeData?.sections) {
    const ACTIVE_SECTIONS = ['summary', 'skills', 'experience', 'awards']
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
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap');

.cv-blue-orange-wrapper { 
  width: 210mm; background: #fff; font-family: 'Inter', sans-serif; 
  position: relative; color: #333; overflow: hidden; box-sizing: border-box; padding: 35px 35px 20px 35px;
}
.cv-container { display: flex; flex-direction: column; width: 100%; min-height: 100%; }

/* ================== HEADER ================== */
.header-card { 
  background: #EAF2FF; border-radius: 20px; padding: 25px 30px; 
  display: flex; align-items: center; gap: 30px; margin-bottom: 30px; 
}
.avatar-container { width: 120px; height: 120px; border-radius: 50%; overflow: hidden; border: 4px solid #fff; box-shadow: 0 4px 10px rgba(0,0,0,0.08); flex-shrink: 0; }
.avatar-img { width: 100%; height: 100%; object-fit: cover; }
.avatar-placeholder { width: 100%; height: 100%; background: #CBD5E1; display: flex; align-items: center; justify-content: center; }

.header-info { flex: 1; }
.fullname { font-size: 32px; font-weight: 800; color: #FF9500; text-transform: uppercase; letter-spacing: 0.5px; margin: 0 0 5px 0; }
.job-title { font-size: 16px; font-weight: 700; color: #2B4C7E; margin: 0; }

/* ================== MAIN LAYOUT (2 COLUMNS) ================== */
.main-layout { display: flex; gap: 25px; align-items: flex-start; }

.left-column { width: 38%; display: flex; flex-direction: column; gap: 25px; }
.right-column { width: 62%; display: flex; flex-direction: column; gap: 25px; }

/* ================== CARDS (KHỐI BO TRÒN) ================== */
.custom-card { 
  background: #EAF2FF; border-radius: 16px; padding: 0 20px 20px 20px; 
  position: relative; margin-top: 15px; 
}
.card-badge { 
  background: #FFF0CC; color: #FF9500; padding: 6px 18px; border-radius: 20px; 
  font-weight: 800; font-size: 13.5px; text-transform: uppercase; 
  display: inline-block; margin-top: -15px; margin-bottom: 12px; 
  box-shadow: 0 2px 5px rgba(0,0,0,0.05); 
}

/* Liên hệ */
.contact-item { display: flex; align-items: center; gap: 12px; font-size: 12.5px; font-weight: 500; color: #4A5568; margin-bottom: 12px; }
.contact-item:last-child { margin-bottom: 0; }
.icon-circle { width: 26px; height: 26px; border-radius: 50%; background: #FF9500; color: white; display: flex; align-items: center; justify-content: center; font-size: 12px; flex-shrink: 0; }

/* List bên trái */
.bullet-list { list-style: none; padding: 0; margin: 0; }
.bullet-list li { position: relative; font-size: 12.5px; color: #4A5568; margin-bottom: 10px; padding-left: 15px; line-height: 1.5; }
.bullet-list li::before { content: '•'; position: absolute; left: 0; top: 0; color: #FF9500; font-weight: 900; font-size: 14px; }

/* ================== TIMELINE CỘT PHẢI ================== */
.timeline-wrapper { border-left: 2px solid #C6D8F5; padding-left: 22px; margin-left: 14px; margin-top: 5px; }
.timeline-item { position: relative; margin-bottom: 25px; }
.timeline-item:last-child { margin-bottom: 0; }

.timeline-icon { 
  position: absolute; left: -37px; top: 0; width: 28px; height: 28px; 
  border-radius: 6px; background: #8FAEE0; color: white; 
  display: flex; align-items: center; justify-content: center; font-size: 12px; 
}

.entry-header { display: flex; flex-wrap: wrap; align-items: center; gap: 6px; margin-bottom: 2px; }
.entry-entity { font-size: 14px; font-weight: 700; color: #2B4C7E; }
.entry-divider { color: #FF9500; font-weight: 800; font-size: 12px; }
.entry-time { font-size: 13px; font-weight: 700; color: #FF9500; }
.entry-role { font-size: 13.5px; font-weight: 700; color: #4A5568; margin-bottom: 4px; }

/* ================== HTML CONTENT ================== */
:deep(.html-content) { font-size: 12.5px; line-height: 1.6; color: #4A5568; text-align: justify; }
:deep(.html-content ul) { list-style-type: none !important; padding-left: 12px !important; margin: 0; }
:deep(.html-content li) { position: relative; margin-bottom: 4px; font-size: 12.5px; line-height: 1.6; }
:deep(.html-content li::before) { content: '•'; position: absolute; left: -12px; color: #4A5568; font-weight: 900; }
:deep(.html-content b), :deep(.html-content strong) { font-weight: 700 !important; color: #2B4C7E; }

/* INTERACTION & BUTTONS */
.section-block { border: 2px solid transparent; transition: 0.2s; cursor: pointer; }
.section-active { border-color: #FF9500 !important; box-shadow: 0 0 10px rgba(255, 149, 0, 0.2); z-index: 20; }
.nav-btns { position: absolute; right: 10px; top: 10px; display: flex; gap: 5px; z-index: 100; }
.nav-btn { background: #FF9500; color: #fff; border: none; width: 24px; height: 24px; border-radius: 4px; cursor: pointer; display: flex; align-items: center; justify-content: center; font-size: 12px; }
.nav-btn:hover { background: #e68600; }
.nav-btn-danger { background: #ff4d4f !important; }
.nav-btn-danger:hover { background: #d9363e !important; }

.contact-block {
  border: 2px solid transparent;
  transition: all 0.2s;
}
.contact-block.contact-active {
  border-color: #FF9500 !important;
  background-color: rgba(255, 149, 0, 0.05);
  box-shadow: 0 4px 18px rgba(0,0,0,0.1);
  z-index: 20;
}
.contact-item-container { position: relative; }
.contact-item-btns {
  position: absolute;
  right: -5px;
  top: 50%;
  transform: translateY(-50%);
  display: flex;
  gap: 3px;
  z-index: 100;
}

.delete-item-btn { position: absolute; right: -10px; top: 0; width: 18px; height: 18px; background: #ff4d4f; color: white; border: none; border-radius: 50%; cursor: pointer; font-size: 10px; display: flex; align-items: center; justify-content: center; z-index: 50; }

.page-break-indicator { position: absolute; left: 0; width: 100%; z-index: 50; display: flex; flex-direction: column; align-items: center; pointer-events: none; }
.page-break-bar { width: 100%; height: 2px; background: rgba(0,0,0,0.1); border-top: 1px dashed rgba(0,0,0,0.2); }
.page-break-label { font-size: 9px; text-transform: uppercase; font-weight: 700; color: #999; background: #fff; padding: 2px 10px; margin-top: -8px; }

@media print { 
  .no-print { display: none !important; } 
  .no-print-bg { print-color-adjust: exact; -webkit-print-color-adjust: exact; }
}
</style>