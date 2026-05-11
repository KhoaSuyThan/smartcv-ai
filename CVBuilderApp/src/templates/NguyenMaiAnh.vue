<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="cv-browser-wrapper"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm` }"
    @click.self="selectedSectionId = null"
  >
    <div class="browser-header paginated-item">
      <div class="browser-tabs">
        <div class="window-controls">
          <span class="dot red"></span>
          <span class="dot yellow"></span>
          <span class="dot green"></span>
        </div>
        <div class="tab active-tab">
          Curriculum Vitae <i class="fas fa-times close-tab"></i>
        </div>
        <div class="new-tab"><i class="fas fa-plus"></i></div>
      </div>
      <div class="browser-url-bar">
        <div class="nav-arrows">
          <i class="fas fa-chevron-left"></i>
          <i class="fas fa-chevron-right text-gray-300"></i>
          <i class="fas fa-redo-alt"></i>
        </div>
        <div class="url-box">
          <i class="fas fa-briefcase url-icon"></i>
          <span class="url-text">Kỹ sư phần mềm IT</span>
        </div>
        <div class="browser-actions">
          <i class="fas fa-microphone"></i>
          <i class="fas fa-ellipsis-v"></i>
        </div>
      </div>
    </div>

    <div class="cv-container">
      
      <header class="custom-card profile-card paginated-item">
        <div class="avatar-container">
          <img v-if="resumeData.general.avatarUrl || resumeData.general.avatar" :src="resumeData.general.avatarUrl || resumeData.general.avatar" class="avatar-img" alt="Avatar"/>
          <div v-else class="avatar-placeholder">
            <i class="fas fa-user text-4xl text-[#CBD5E1]"></i>
          </div>
        </div>
        <div class="header-info">
          
          <h1 class="fullname" v-if="!isEmpty(resumeData.general.fullName)" v-html="resumeData.general.fullName"></h1>
          <h1 class="fullname" v-else>NGUYỄN MAI ANH</h1>
          
          <p class="job-title" v-if="!isEmpty(resumeData.general.jobTitle)" v-html="resumeData.general.jobTitle"></p>
          <p class="job-title" v-else>Kỹ sư phần mềm IT</p>
          
          <div class="contact-grid">
            <div class="contact-item">
              <i class="fas fa-calendar-alt"></i>
              <span v-if="!isEmpty(resumeData.general.birthDate)" v-html="resumeData.general.birthDate"></span>
              <span v-else>18/12/1997</span>
            </div>
            <div class="contact-item">
              <i class="fas fa-phone-alt"></i>
              <span v-if="!isEmpty(resumeData.general.phone)" v-html="resumeData.general.phone"></span>
              <span v-else>(024) 6680 5588</span>
            </div>
            <div class="contact-item">
              <i class="fas fa-envelope"></i>
              <span v-if="!isEmpty(resumeData.general.email)" v-html="resumeData.general.email"></span>
              <span v-else>hotro@topcv.vn</span>
            </div>
            <div class="contact-item">
              <i class="fas fa-map-marker-alt"></i>
              <span v-if="!isEmpty(resumeData.general.address)" v-html="resumeData.general.address"></span>
              <span v-else>Quận A, Hà Nội</span>
            </div>
          </div>
        </div>
      </header>

      <template v-for="section in (resumeData.sections || []).filter(s => s.id === 'summary')" :key="section.id">
        <div v-show="section.isVisible" class="custom-card section-block mb-6"
             :class="{ 'section-active': selectedSectionId === section.id }"
             @click.stop="toggleSection(section.id)">
          
          <transition name="fade-btns">
            <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
              <button class="nav-btn" @click.stop.prevent="$emit('moveUp', section.id, [])"><i class="fas fa-chevron-up"></i></button>
              <button class="nav-btn" @click.stop.prevent="$emit('moveDown', section.id, [])"><i class="fas fa-chevron-down"></i></button>
            </div>
          </transition>

          <div class="card-body">
            <div v-if="!(isEmpty(section.desc) && isEmpty(section.description) && isEmpty(section.content) && isEmpty(section.value) && isEmpty(resumeData.general.summary))" 
                 class="html-content text-justify text-gray-500" v-html="formatDesc(section.desc || section.description || section.content || section.value || resumeData.general.summary)"></div>
            <div v-else class="html-content text-gray-400 italic paginated-item">
              Hãy nói 1 chút về mục tiêu nghề nghiệp của bạn...
            </div>
          </div>
        </div>
      </template>

      <div class="main-layout">
        
        <aside class="left-column">
          <template v-for="section in sidebarSections" :key="section.id">
            <div v-show="section.isVisible" class="custom-card section-block"
                 :style="{ order: getOrder(section.id, leftIds) }"
                 :class="{ 'section-active': selectedSectionId === section.id }"
                 @click.stop="toggleSection(section.id)">
              
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
                  <button class="nav-btn" @click.stop.prevent="$emit('moveUp', section.id, leftIds)"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop.prevent="$emit('moveDown', section.id, leftIds)"><i class="fas fa-chevron-down"></i></button>
                  <button class="nav-btn" @click.stop.prevent="moveHorizontal(section.id)"><i class="fas fa-exchange-alt"></i></button>
                </div>
              </transition>

              <div class="card-header paginated-item">
                <div class="card-title-group">
                  <i v-if="section.id === 'education'" class="fas fa-graduation-cap card-icon"></i>
                  <i v-else-if="section.id === 'experience'" class="fas fa-briefcase card-icon"></i>
                  <i v-else-if="section.id === 'skills' || section.id === 'it_skills' || section.id === 'languages'" class="fas fa-pen card-icon"></i>
                  <i v-else-if="section.id === 'activities'" class="fas fa-users card-icon"></i>
                  <i v-else-if="section.id === 'project' || section.id === 'projects'" class="fas fa-project-diagram card-icon"></i>
                  <i v-else-if="section.id === 'awards'" class="fas fa-trophy card-icon"></i>
                  <i v-else-if="section.id === 'certifications' || section.id === 'certificates'" class="fas fa-certificate card-icon"></i>
                  <i v-else-if="section.id === 'references'" class="fas fa-user-check card-icon"></i>
                  <i v-else class="fas fa-layer-group card-icon"></i>
                  <h3 class="card-title">{{ section.name || section.title || (section.id === 'education' ? 'Học vấn' : section.id === 'experience' ? 'Kinh nghiệm làm việc' : section.id) }}</h3>
                </div>
                <i class="fas fa-ellipsis-v text-blue-500 text-lg"></i>
              </div>
              
              <div class="card-body">
                <div v-if="section.desc || section.description || section.content || section.value" class="html-content mb-3" v-html="formatDesc(section.desc || section.description || section.content || section.value)"></div>
                
                <ul class="bullet-list" v-if="['skills', 'it_skills', 'languages', 'hobbies'].includes(section.id)">
                  <li v-for="(item, i) in (section.items?.length ? section.items : getMockData(section.id))" :key="i" class="item-container" :class="{'no-bullet': !item.level && item.name && /<[a-z][\s\S]*>/i.test(item.name)}">
                    <template v-if="typeof item === 'object'">
                      <div class="flex flex-col paginated-item">
                        <span v-if="item.name && /<[a-z][\s\S]*>/i.test(item.name)" v-html="item.name"></span>
                        <span class="text-gray-600" v-else>{{ item.name || item.title || item.info }}</span>
                        <span class="text-gray-500 text-xs mt-0.5" v-if="item.level">{{ item.level }}</span>
                      </div>
                    </template>
                    <template v-else>
                      <div class="html-content" v-html="formatDesc(item)"></div>
                    </template>
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop.prevent="$emit('removeItem', section.id, i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                  </li>
                </ul>

                <div class="detailed-list" v-else>
                  <div v-for="(item, i) in (section.items?.length ? section.items : getMockData(section.id))" :key="i" class="item-container relative mb-5 last:mb-0">
                    <template v-if="typeof item === 'object'">
                      <div class="paginated-item">
                        <div class="flex justify-between items-start mb-1" v-if="item.company || item.school || item.organization || item.name || item.title || item.time || item.year || item.date">
                          <span class="entry-entity">
                            <template v-if="item.name && /<[a-z][\s\S]*>/i.test(item.name)"><span v-html="item.name"></span></template>
                            <template v-else>{{ item.company || item.school || item.organization || item.name || item.title }}</template>
                          </span>
                          <span class="entry-time badge-time" v-if="item.time || item.year || item.date">{{ item.time || item.year || item.date }}</span>
                        </div>
                        <div class="entry-role" v-if="item.role || item.position || item.major">
                          {{ item.role || item.position || item.major }}
                        </div>
                      </div>

                      <div class="text-[13px] text-gray-500 mt-1" v-if="item.gradType || item.info || item.contact">
                        <div v-if="item.gradType" class="text-gray-400 paginated-item">{{ item.gradType }}</div>
                        <div v-if="item.info || item.contact" class="html-content" v-html="formatDesc(item.info || item.contact)"></div>
                      </div>

                      <div class="html-content mt-2" v-if="item.desc || item.description" v-html="formatDesc(item.desc || item.description)"></div>
                    </template>
                    <template v-else>
                      <div class="html-content" v-html="formatDesc(item)"></div>
                    </template>
                    
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop.prevent="$emit('removeItem', section.id, i)" class="delete-item-btn no-print" style="top: 0; right: -10px;"><i class="fas fa-times"></i></button>
                  </div>
                </div>
              </div>
            </div>
          </template>
        </aside>

        <main class="right-column">
          <template v-for="section in mainSections" :key="section.id">
            <div v-show="section.isVisible" class="custom-card section-block"
                 :style="{ order: getOrder(section.id, rightIds) }"
                 :class="{ 'section-active': selectedSectionId === section.id }"
                 @click.stop="toggleSection(section.id)">
              
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
                  <button class="nav-btn" @click.stop.prevent="moveHorizontal(section.id)"><i class="fas fa-exchange-alt"></i></button>
                  <button class="nav-btn" @click.stop.prevent="$emit('moveUp', section.id, rightIds)"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop.prevent="$emit('moveDown', section.id, rightIds)"><i class="fas fa-chevron-down"></i></button>
                </div>
              </transition>

              <div class="card-header paginated-item">
                <div class="card-title-group">
                  <i v-if="section.id === 'education'" class="fas fa-graduation-cap card-icon"></i>
                  <i v-else-if="section.id === 'experience'" class="fas fa-briefcase card-icon"></i>
                  <i v-else-if="section.id === 'skills' || section.id === 'it_skills' || section.id === 'languages'" class="fas fa-pen card-icon"></i>
                  <i v-else-if="section.id === 'activities'" class="fas fa-users card-icon"></i>
                  <i v-else-if="section.id === 'project' || section.id === 'projects'" class="fas fa-project-diagram card-icon"></i>
                  <i v-else-if="section.id === 'awards'" class="fas fa-trophy card-icon"></i>
                  <i v-else-if="section.id === 'certifications' || section.id === 'certificates'" class="fas fa-certificate card-icon"></i>
                  <i v-else-if="section.id === 'references'" class="fas fa-user-check card-icon"></i>
                  <i v-else class="fas fa-layer-group card-icon"></i>
                  <h3 class="card-title">{{ section.name || section.title || (section.id === 'skills' ? 'Các kỹ năng' : section.id.toUpperCase()) }}</h3>
                </div>
                <i class="fas fa-ellipsis-v text-blue-500 text-lg"></i>
              </div>
              
              <div class="card-body">
                <div v-if="section.desc || section.description || section.content || section.value" class="html-content mb-3" v-html="formatDesc(section.desc || section.description || section.content || section.value)"></div>
                
                <ul class="bullet-list" v-if="['skills', 'it_skills', 'languages', 'hobbies'].includes(section.id)">
                  <li v-for="(item, i) in (section.items?.length ? section.items : getMockData(section.id))" :key="i" class="item-container" :class="{'no-bullet': !item.level && item.name && /<[a-z][\s\S]*>/i.test(item.name)}">
                    <template v-if="typeof item === 'object'">
                      <div class="flex flex-col paginated-item">
                        <span v-if="item.name && /<[a-z][\s\S]*>/i.test(item.name)" v-html="item.name"></span>
                        <span class="text-gray-600" v-else>{{ item.name || item.title || item.info }}</span>
                        <span class="text-gray-500 text-xs mt-0.5" v-if="item.level">{{ item.level }}</span>
                      </div>
                    </template>
                    <template v-else>
                      <div class="html-content" v-html="formatDesc(item)"></div>
                    </template>
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop.prevent="$emit('removeItem', section.id, i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                  </li>
                </ul>

                <div class="detailed-list" v-else>
                  <div v-for="(item, i) in (section.items?.length ? section.items : getMockData(section.id))" :key="i" class="item-container relative mb-5 last:mb-0">
                    <template v-if="typeof item === 'object'">
                      <div class="paginated-item">
                        <div class="flex justify-between items-start mb-1" v-if="item.company || item.school || item.organization || item.name || item.title || item.time || item.year || item.date">
                          <span class="entry-entity">
                            <template v-if="item.name && /<[a-z][\s\S]*>/i.test(item.name)"><span v-html="item.name"></span></template>
                            <template v-else>{{ item.company || item.school || item.organization || item.name || item.title }}</template>
                          </span>
                          <span class="entry-time badge-time" v-if="item.time || item.year || item.date">{{ item.time || item.year || item.date }}</span>
                        </div>
                        <div class="entry-role" v-if="item.role || item.position || item.major">
                          {{ item.role || item.position || item.major }}
                        </div>
                      </div>

                      <div class="text-[13px] text-gray-500 mt-1" v-if="item.gradType || item.info || item.contact">
                        <div v-if="item.gradType" class="text-gray-400 paginated-item">{{ item.gradType }}</div>
                        <div v-if="item.info || item.contact" class="html-content" v-html="formatDesc(item.info || item.contact)"></div>
                      </div>

                      <div class="html-content mt-2" v-if="item.desc || item.description" v-html="formatDesc(item.desc || item.description)"></div>
                    </template>
                    <template v-else>
                      <div class="html-content" v-html="formatDesc(item)"></div>
                    </template>
                    
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop.prevent="$emit('removeItem', section.id, i)" class="delete-item-btn no-print" style="top: 0; right: -10px;"><i class="fas fa-times"></i></button>
                  </div>
                </div>
              </div>
            </div>
          </template>
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

const isEmpty = (val) => {
  if (!val) return true;
  if (typeof val !== 'string') return false;
  const cleanStr = val.replace(/<\/?[^>]+(>|$)/g, "").replace(/&[#a-z0-9]+;/ig, "").trim().toLowerCase();
  return cleanStr === '' || cleanStr === 'br';
}

const getMockData = (sectionId) => {
  return [{ name: 'Chưa có dữ liệu' }]
}

// ── QUẢN LÝ ẨN/HIỆN & CHIA CỘT ──────────────────────────────────────
const localLeftKeys = ref(['education', 'experience', 'activities', 'project'])

watch(() => props.resumeData?.leftColKeys, (newVal) => {
  if (newVal && Array.isArray(newVal)) {
    localLeftKeys.value = newVal
  }
}, { immediate: true })

const moveHorizontal = (id) => {
  if (localLeftKeys.value.includes(id)) {
    localLeftKeys.value = localLeftKeys.value.filter(k => k !== id)
  } else {
    localLeftKeys.value.push(id)
  }
  emit('moveHorizontal', id, toRaw(localLeftKeys.value))
  
  nextTick(() => {
    requestPagination()
  })
}

const sidebarSections = computed(() => (props.resumeData?.sections || []).filter(s => localLeftKeys.value.includes(s.id) && s.id !== 'summary'))
const mainSections = computed(() => (props.resumeData?.sections || []).filter(s => !localLeftKeys.value.includes(s.id) && s.id !== 'summary'))

const getActiveIds = (sourceArray) => sourceArray.filter(s => s.isVisible).map(s => s.id)
const leftIds = computed(() => getActiveIds(sidebarSections.value))
const rightIds = computed(() => getActiveIds(mainSections.value))

const getOrder = (id, activeArray) => activeArray.indexOf(id) + 1

const toggleSection = (id) => { selectedSectionId.value = selectedSectionId.value === id ? null : id }

onMounted(() => {
  if (props.resumeData?.sections) {
    const ACTIVE_SECTIONS = ['summary', 'education', 'experience', 'skills', 'project', 'projects']
    props.resumeData.sections.forEach(sec => {
      if (ACTIVE_SECTIONS.includes(sec.id) && !sec.isVisible) {
        sec.isVisible = true
      }
    })
  }
  requestPagination()
  window.addEventListener('resize', requestPagination)
})

// ── UTILITIES & HTML FORMAT ──────────────────────────────────────────────
const formatDesc = (text) => {
  if (isEmpty(text)) return '';
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
        // UL/OL sinh ra các LI được gắn paginated-item => Mỗi thẻ LI sẽ rớt dòng độc lập
        Array.from(node.children).forEach(li => li.classList.add('paginated-item'))
        out.appendChild(node.cloneNode(true))
      } else {
        node.classList.add('paginated-item'); out.appendChild(node.cloneNode(true))
      }
    }
  })
  return out.innerHTML
}

// ── PAGINATION LOGIC ──────────────────────────────────
const A4_W_MM = 210
const A4_H_MM = 297

let paginateTimer = null

const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 100)
}

const doPagination = async () => {
  if (!cvRoot.value) return

  const allElements = cvRoot.value.querySelectorAll('.paginated-item, .custom-card')
  allElements.forEach(el => { el.style.marginTop = '0px' })
  await nextTick()

  const pxPerMm = cvRoot.value.offsetWidth / A4_W_MM
  const pageH = A4_H_MM * pxPerMm
  const bottomSafeZone = 14 * pxPerMm 
  const topMargin = 10 * pxPerMm 

  const getOffsetTop = (el) => {
    let offset = 0
    let curr = el
    while (curr && curr !== cvRoot.value) {
      offset += curr.offsetTop
      curr = curr.offsetParent
    }
    return offset
  }

  const paginatedItems = cvRoot.value.querySelectorAll('.paginated-item')

  paginatedItems.forEach((el) => {
    if(el.offsetHeight === 0) return
    const top = getOffsetTop(el)
    const topInPage = top % pageH
    const bottomInPage = topInPage + el.offsetHeight
    
    if (bottomInPage > (pageH - bottomSafeZone)) {
       let targetEl = el
       const parentCard = el.closest('.custom-card')

       // CHỈ kéo cả Thẻ (Card) xuống dòng NẾU như cái bị đẩy là cái Tiêu Đề của Thẻ đó (để tiêu đề không bị bơ vơ)
       // HOẶC toàn bộ nội dung cái thẻ đó siêu ngắn (nhỏ hơn 30% trang).
       // CÒN LẠI: Cắt ngang dòng nào, rớt dòng đó, nền thẻ sẽ tự động chải dài!
       if (parentCard && (el.classList.contains('card-header') || parentCard.offsetHeight < (pageH * 0.3))) {
           targetEl = parentCard
       }

       if (!targetEl.style.marginTop || targetEl.style.marginTop === '0px') {
           const targetTop = getOffsetTop(targetEl)
           const targetTopInPage = targetTop % pageH
           const distToNextPage = pageH - targetTopInPage + topMargin
           targetEl.style.marginTop = `${distToNextPage}px`
       }
    }
  })

  let maxBottom = 0
  allElements.forEach(el => {
    const b = getOffsetTop(el) + el.offsetHeight
    if (b > maxBottom) maxBottom = b
  })
  pageCount.value = Math.max(1, Math.ceil(maxBottom / pageH))
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

.cv-browser-wrapper { 
  width: 210mm; font-family: 'Inter', sans-serif; 
  position: relative; color: #333; overflow: hidden; box-sizing: border-box;
  background-color: #F8FAFC;
  background-image: 
    linear-gradient(#E2E8F0 1px, transparent 1px),
    linear-gradient(90deg, #E2E8F0 1px, transparent 1px);
  background-size: 25px 25px;
}

/* ================== BROWSER UI ================== */
.browser-header {
  background: #fff; border-bottom: 1px solid #E2E8F0;
  box-shadow: 0 2px 4px rgba(0,0,0,0.02);
  margin-bottom: 30px;
}
.browser-tabs {
  display: flex; align-items: flex-end; padding: 10px 15px 0 15px;
  background: #F1F5F9; border-bottom: 1px solid #E2E8F0;
}
.window-controls { display: flex; gap: 6px; align-items: center; padding-bottom: 10px; margin-right: 20px; }
.window-controls .dot { width: 10px; height: 10px; border-radius: 50%; }
.dot.red { background: #FF5F56; }
.dot.yellow { background: #FFBD2E; }
.dot.green { background: #27C93F; }

.tab {
  background: #fff; padding: 8px 20px; border-radius: 8px 8px 0 0;
  font-size: 12px; font-weight: 600; color: #1E40AF;
  display: flex; align-items: center; gap: 15px;
  border-top: 1px solid #E2E8F0; border-left: 1px solid #E2E8F0; border-right: 1px solid #E2E8F0;
}
.close-tab { color: #94A3B8; font-size: 10px; cursor: pointer; }
.new-tab { padding: 8px 15px; color: #64748B; font-size: 14px; cursor: pointer; margin-bottom: 5px; }

.browser-url-bar {
  display: flex; align-items: center; padding: 8px 20px; gap: 15px;
}
.nav-arrows { display: flex; gap: 15px; color: #1E40AF; font-size: 14px; }
.url-box {
  flex: 1; background: #F1F5F9; border-radius: 20px; padding: 6px 15px;
  display: flex; align-items: center; gap: 10px;
}
.url-icon { color: #1E40AF; font-size: 13px; }
.url-text { font-size: 13px; color: #475569; font-weight: 500; }
.browser-actions { display: flex; gap: 15px; color: #1E40AF; font-size: 14px; }

/* ================== CV CONTENT ================== */
.cv-container { padding: 0 35px 35px 35px; }

.custom-card { 
  background: #fff; border-radius: 20px; padding: 25px 30px; 
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.03); 
}

/* HEADER PROFILE */
.profile-card {
  display: flex; align-items: center; gap: 35px; margin-bottom: 25px;
}
.avatar-container { 
  width: 130px; height: 130px; border-radius: 50%; overflow: hidden; 
  border: 4px solid #F1F5F9; flex-shrink: 0; 
}
.avatar-img { width: 100%; height: 100%; object-fit: cover; }
.avatar-placeholder { width: 100%; height: 100%; background: #F8FAFC; display: flex; align-items: center; justify-content: center; }

.header-info { flex: 1; }
.fullname { font-size: 32px; font-weight: 800; color: #1D4ED8; text-transform: uppercase; margin: 0 0 5px 0; letter-spacing: 0.5px; }
.job-title { font-size: 16px; font-weight: 600; color: #3B82F6; margin: 0 0 20px 0; }

.contact-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.contact-item { display: flex; align-items: center; gap: 10px; font-size: 12.5px; color: #64748B; font-weight: 500; }
.contact-item i { width: 14px; text-align: center; font-size: 13px; color: #94A3B8; }

/* 2 COLUMNS LAYOUT */
.main-layout { display: flex; gap: 25px; align-items: flex-start; }
.left-column { width: 55%; display: flex; flex-direction: column; gap: 25px; }
.right-column { width: 45%; display: flex; flex-direction: column; gap: 25px; }

/* CARD HEADER & BODY */
.card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.card-title-group { display: flex; align-items: center; gap: 12px; }
.card-icon { font-size: 18px; color: #3B82F6; }
.card-title { font-size: 18px; font-weight: 800; color: #1D4ED8; margin: 0; }

.entry-entity { font-size: 15px; font-weight: 700; color: #1D4ED8; }
.entry-role { font-size: 13px; font-weight: 600; color: #475569; margin-bottom: 4px; }
.badge-time { background: #DBEAFE; color: #2563EB; padding: 3px 12px; border-radius: 20px; font-size: 12px; font-weight: 700; }

/* List Bullet */
.bullet-list { list-style: none; padding: 0; margin: 0; }
.bullet-list li { position: relative; font-size: 12.5px; margin-bottom: 12px; padding-left: 15px; line-height: 1.6; }
.bullet-list li::before { content: '•'; position: absolute; left: 0; top: 0; color: #94A3B8; font-weight: 900; font-size: 14px; }
.bullet-list li.no-bullet { padding-left: 0; }
.bullet-list li.no-bullet::before { display: none; }

/* ================== HTML CONTENT ================== */
.paginated-item { transition: none; }

:deep(.html-content) { font-size: 12.5px; line-height: 1.6; color: #64748B; text-align: justify; }
:deep(.html-content ul) { list-style-type: none !important; padding-left: 12px !important; margin: 0; }
:deep(.html-content li) { position: relative; margin-bottom: 4px; font-size: 12.5px; line-height: 1.6; }
:deep(.html-content li::before) { content: '•'; position: absolute; left: -12px; color: #94A3B8; font-weight: 900; }
:deep(.html-content b), :deep(.html-content strong) { font-weight: 700 !important; color: #334155; }

/* INTERACTION & BUTTONS */
.section-block { position: relative; border: 2px solid transparent; transition: 0.2s; cursor: pointer; border-radius: 20px; }
.section-active { border-color: #3B82F6 !important; box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.1); z-index: 20; }

.fade-btns-enter-active, .fade-btns-leave-active { transition: opacity 0.2s ease, transform 0.2s ease; }
.fade-btns-enter-from, .fade-btns-leave-to { opacity: 0; transform: scale(0.9); }
.nav-btns { position: absolute; right: 20px; top: 18px; display: flex; gap: 6px; z-index: 100; }
.nav-btn { 
  background: #3B82F6; color: #fff; border: none; width: 28px; height: 28px; 
  border-radius: 6px; cursor: pointer; display: flex; align-items: center; 
  justify-content: center; font-size: 13px; box-shadow: 0 2px 4px rgba(59, 130, 246, 0.3);
}
.nav-btn:hover { background: #2563EB; }

.delete-item-btn { position: absolute; right: -5px; top: 0; width: 18px; height: 18px; background: #EF4444; color: white; border: none; border-radius: 50%; cursor: pointer; font-size: 10px; display: flex; align-items: center; justify-content: center; z-index: 50; }

.page-break-indicator { position: absolute; left: 0; width: 100%; z-index: 50; display: flex; flex-direction: column; align-items: center; pointer-events: none; }
.page-break-bar { width: 100%; height: 2px; background: rgba(0,0,0,0.1); border-top: 1px dashed rgba(0,0,0,0.2); }
.page-break-label { font-size: 9px; text-transform: uppercase; font-weight: 700; color: #999; background: #fff; padding: 2px 10px; margin-top: -8px; }

@media print { 
  .no-print { display: none !important; } 
  .no-print-bg { print-color-adjust: exact; -webkit-print-color-adjust: exact; }
  .section-block,
  .section-block.section-active {
    cursor: default !important;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.03) !important;
    border-color: transparent !important;
  }
}
</style>