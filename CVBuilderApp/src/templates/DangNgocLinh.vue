<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-col relative box-border text-[#333] leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif' }"
    @click.self="selectedSectionId = null"
  >
    <!-- MÀU NỀN TRẮNG TOÀN CỤC -->
    <div class="absolute inset-0 bg-white z-0 pointer-events-none"></div>

    <!-- MAIN VERTICAL FLOW -->
    <main class="flex-1 flex flex-col relative z-20 min-h-max" @click.self="selectedSectionId = null">
      
      <!-- HEADER BLOCK (Căn giữa hoàn toàn - Khoảng cách tinh tế) -->
      <header class="text-center pt-[14mm] px-[18mm] pb-[4mm] flex flex-col paginated-item relative z-10">
        <h1 
          class="font-black text-[#111111] tracking-wide uppercase leading-tight mb-1.5" 
          style="font-size: 25px !important;" 
          v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'HỌ VÀ TÊN'"
        ></h1>
        <h2 
          class="font-semibold text-gray-500 tracking-[0.15em] uppercase text-[12px] mb-3.5" 
          v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'Vị trí ứng tuyển'"
        ></h2>
        
        <!-- Hàng ngang liên hệ -->
        <div class="flex flex-wrap justify-center items-center gap-x-5 gap-y-1.5 text-[11.5px] text-gray-600 font-medium">
          <!-- Điện thoại -->
          <div class="flex items-center gap-1.5" v-if="!isEmpty(resumeData.general.phone)">
            <svg class="w-3.5 h-3.5 text-gray-500 shrink-0" fill="currentColor" viewBox="0 0 24 24">
              <path d="M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z"/>
            </svg>
            <span v-html="resumeData.general.phone"></span>
          </div>
          <!-- Email -->
          <div class="flex items-center gap-1.5" v-if="!isEmpty(resumeData.general.email)">
            <svg class="w-3.5 h-3.5 text-gray-500 shrink-0" fill="currentColor" viewBox="0 0 24 24">
              <path d="M20 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z"/>
            </svg>
            <span v-html="resumeData.general.email"></span>
          </div>
          <!-- Website -->
          <div class="flex items-center gap-1.5" v-if="!isEmpty(resumeData.general.website)">
            <svg class="w-3.5 h-3.5 text-gray-500 shrink-0" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
              <circle cx="12" cy="12" r="10"></circle>
              <line x1="2" y1="12" x2="22" y2="12"></line>
              <path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"></path>
            </svg>
            <span v-html="resumeData.general.website"></span>
          </div>
          <!-- Địa chỉ -->
          <div class="flex items-center gap-1.5" v-if="!isEmpty(resumeData.general.address)">
            <svg class="w-3.5 h-3.5 text-gray-500 shrink-0" fill="currentColor" viewBox="0 0 24 24">
              <path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5a2.5 2.5 0 0 1 0-5 2.5 2.5 0 0 1 0 5z"/>
            </svg>
            <span v-html="resumeData.general.address"></span>
          </div>
        </div>
      </header>

      <!-- SECTIONS CONTAINER (Khoảng cách giữa các mục giảm xuống gap-[3mm] để thon gọn) -->
      <div class="px-[18mm] pb-[14mm] flex-1 flex flex-col gap-[3mm]">
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-if="section.isVisible"
            class="section-block relative group -mx-[4mm] px-[4mm] py-[1mm]"
            :class="{ 'section-active': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--active-bg': '#333333' } : {}"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Control Buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print" style="right: 12px; top: 12px;" @click.stop>
                <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên"><svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống"><svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              </div>
            </transition>

            <!-- Tiêu đề Mục -->
            <div class="paginated-item">
              <div class="flex flex-col mb-2">
                <h3 
                  class="font-extrabold uppercase tracking-[0.08em] text-black" 
                  style="font-size: 15px !important;"
                >
                  {{ section.title }}
                </h3>
                <div class="h-[1.5px] bg-black w-full mt-1"></div>
              </div>
            </div>

            <div v-if="sectionHasContent(section)">
              <!-- 1. MỤC TIÊU NGHỀ NGHIỆP (SUMMARY) -->
              <div v-if="section.id === 'summary'" class="item-container relative">
                <div class="text-[12px] text-[#333333] leading-[1.7] text-justify font-normal html-content" v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Chưa có thông tin mục tiêu nghề nghiệp.')"></div>
              </div>

              <!-- 2. QUÁ TRÌNH HỌC VẤN (EDUCATION) -->
              <div v-else-if="section.id === 'education'" class="space-y-2.5">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container text-[12px] relative">
                  <div class="flex justify-between items-baseline mb-0.5">
                    <h4 class="font-bold text-[13.5px] text-[#111111] leading-tight flex-1">
                      {{ item.school }}
                    </h4>
                    <span class="font-bold text-[12px] text-[#111111] shrink-0 ml-4">
                      {{ item.year }}
                    </span>
                  </div>
                  <div class="text-[12.5px] text-[#333333] font-bold">
                    {{ item.major }}
                  </div>
                  <div v-if="item.gradType" class="text-[11.5px] text-[#555555] mt-0.5">
                    {{ item.gradType }}
                  </div>
                  <div v-if="item.desc" class="text-[11.5px] text-[#555555] leading-[1.6] mt-1 html-content" v-html="formatDesc(item.desc)"></div>
                  
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- 3. KINH NGHIỆM LÀM VIỆC (EXPERIENCE) -->
              <div v-else-if="section.id === 'experience'" class="space-y-3">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container text-[12px] relative mb-2.5 last:mb-0">
                  <div class="flex justify-between items-baseline mb-0.5">
                    <h4 class="font-bold text-[13.5px] text-[#111111] leading-tight flex-1">
                      {{ item.company }}
                    </h4>
                    <span class="font-bold text-[12px] text-[#111111] shrink-0 ml-4">
                      {{ item.time || item.year }}
                    </span>
                  </div>
                  <div class="text-[12px] text-gray-700 font-bold mb-1 uppercase tracking-wide">
                    {{ item.role || item.position }}
                  </div>
                  <div class="text-[11.5px] text-[#333333] leading-[1.65] text-justify font-normal html-content" v-html="formatDesc(item.desc)"></div>
                  
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- 4. KỸ NĂNG (SKILLS) - DẠNG BẢNG ĐẶC THÙ -->
              <div v-else-if="section.id === 'skills'" class="flex flex-col border-t border-gray-300">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item flex border-b border-gray-300 py-2.5 items-start relative group/skill">
                  <div class="w-[30%] shrink-0 font-bold text-[13px] text-black pr-4">
                    {{ item.name }}
                  </div>
                  <div class="w-[70%] text-[12px] text-[#333333] leading-relaxed pr-8 html-content" v-html="formatDesc(item.level || item.info)"></div>
                  
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- 5. HOẠT ĐỘNG (ACTIVITIES) -->
              <div v-else-if="section.id === 'activities'" class="space-y-3">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container text-[12px] relative mb-2.5 last:mb-0">
                  <div class="flex justify-between items-baseline mb-0.5">
                    <h4 class="font-bold text-[13.5px] text-[#111111] leading-tight flex-1">
                      {{ item.name }}
                    </h4>
                    <span class="font-bold text-[12px] text-[#111111] shrink-0 ml-4">
                      {{ item.time || item.year }}
                    </span>
                  </div>
                  <div v-if="item.role" class="text-[12px] text-[#555555] font-semibold mb-1 italic text-gray-600">
                    {{ item.role }}
                  </div>
                  <div v-if="item.desc" class="text-[11.5px] text-[#333333] leading-[1.65] text-justify font-normal html-content" v-html="formatDesc(item.desc)"></div>
                  
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- CÁC MỤC KHÁC NẾU NGƯỜI DÙNG KÍCH HOẠT THÊM -->
              <div v-else class="space-y-2.5">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item text-[12px] text-[#333333] leading-relaxed item-container relative">
                  <div class="flex justify-between items-baseline mb-0.5">
                    <div class="font-bold text-[12.5px] text-black" v-if="item.name || item.title">{{ item.name || item.title }}</div>
                    <span v-if="item.year || item.time" class="text-[12px] text-gray-500 font-bold ml-2 shrink-0">{{ item.year || item.time }}</span>
                  </div>
                  <div v-if="item.desc" class="text-[11.5px] text-slate-700 html-content leading-relaxed" v-html="formatDesc(item.desc)"></div>
                  <div v-else-if="item.info" class="text-[11.5px] text-slate-700 html-content leading-relaxed" v-html="formatDesc(item.info)"></div>
                  
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

            </div>
          </div>
        </template>
      </div>

      <!-- Watermark footer -->
      <div class="absolute bottom-[6mm] right-[18mm] text-[11px] font-medium text-gray-300 no-print tracking-wide">
        @CVBuilder
      </div>
    </main>

    <!-- ĐƯỜNG PHÂN TRANG CHO PDF & TRÌNH DUYỆT -->
    <template v-for="p in (pageCount - 1)" :key="'div-' + p">
      <div
        class="absolute left-0 w-full z-50 flex flex-col items-center justify-center pointer-events-none no-print"
        :style="{ top: `calc(${p * 297}mm - 8px)` }"
      >
        <div class="w-[105%] h-[16px] bg-slate-800/90 shadow-inner overflow-hidden border-y border-black/30 backdrop-blur-sm"></div>
        <span class="absolute text-[9px] uppercase font-bold text-slate-300 tracking-widest bg-slate-700 px-3 py-0.5 rounded border border-slate-600 shadow-md">Ngắt trang {{ p + 1 }}</span>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, ref, onMounted, nextTick, watch, onUnmounted } from 'vue'

const cvRoot = ref(null)
const pageCount = ref(1)
const selectedSectionId = ref(null)

const props = defineProps({
  resumeData: { type: Object, required: true }
})
const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

const toggleSection = (id) => {
  selectedSectionId.value = selectedSectionId.value === id ? null : id
}

const handleOutsideClick = (e) => {
  if (cvRoot.value && !cvRoot.value.contains(e.target)) {
    selectedSectionId.value = null
  }
}

const isEmpty = (val) => {
  if (!val) return true
  if (typeof val !== 'string') return false
  return val.replace(/<[^>]*>/g, '').trim() === ''
}

const sectionHasContent = (section) => {
  if (section.id === 'summary') return true
  return !section || (section.items && section.items.length > 0)
}

const formatDesc = (text) => {
  if (!text) return ''
  
  if (!/<[a-z][\s\S]*>/i.test(text)) {
     return text.split('\n')
                .map(l => l.trim())
                .filter(Boolean)
                .map(l => `<div class="paginated-item min-h-[14px]">${l}</div>`)
                .join('')
  }

  const tempDiv = document.createElement('div')
  tempDiv.innerHTML = text
  
  tempDiv.querySelectorAll('li').forEach(li => {
    const child = li.firstElementChild
    if (child && (child.tagName === 'FONT' || child.tagName === 'SPAN')) {
      if (child.color) li.style.color = child.color
      if (child.style && child.style.color) li.style.color = child.style.color
    }
  })

  const container = document.createElement('div')
  Array.from(tempDiv.childNodes).forEach(node => {
    if (node.nodeType === Node.TEXT_NODE) {
      if (node.textContent.trim()) {
        const div = document.createElement('div')
        div.className = 'paginated-item min-h-[14px]'
        div.appendChild(node.cloneNode(true))
        container.appendChild(div)
      }
    } else if (node.nodeType === Node.ELEMENT_NODE) {
      if (node.tagName === 'BR') {
        const div = document.createElement('div')
        div.className = 'paginated-item h-[14px]'
        container.appendChild(div)
      } else if (['UL', 'OL'].includes(node.tagName)) {
        Array.from(node.children).forEach(li => li.classList.add('paginated-item'))
        container.appendChild(node.cloneNode(true))
      } else {
        node.classList.add('paginated-item')
        container.appendChild(node.cloneNode(true))
      }
    }
  })

  return container.innerHTML
}

// Cả 5 mục đề xuất của mẫu Đặng Ngọc Linh đều dồn về một cột chính (cột right)
const mainSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'right')
)

const mainIds = computed(() => mainSections.value.map(s => s.id))

// ─── PAGINATION ENGINE ───
const A4_W_MM   = 210
const A4_H_MM   = 297

let paginateTimer = null
const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 60)
}

const doPagination = async () => {
  if (!cvRoot.value) return

  const allElements = cvRoot.value.querySelectorAll('.paginated-item')
  allElements.forEach(el => { el.style.marginTop = '0px' })
  await nextTick()

  const pxPerMm = cvRoot.value.offsetWidth / A4_W_MM
  const pageH = A4_H_MM * pxPerMm
  
  const bottomSafeZone = 14 * pxPerMm 
  const topMargin = 8 * pxPerMm 

  const getOffsetTop = (el) => {
    let offset = 0
    let curr = el
    while (curr && curr !== cvRoot.value) {
      offset += curr.offsetTop
      curr = curr.offsetParent
    }
    return offset
  }

  allElements.forEach((el) => {
    if(el.offsetHeight === 0) return
    const top = getOffsetTop(el)
    const topInPage = top % pageH
    const bottomInPage = topInPage + el.offsetHeight
    
    if (bottomInPage > (pageH - bottomSafeZone)) {
       const distToNextPage = pageH - topInPage + topMargin
       el.style.marginTop = `${distToNextPage}px`
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

onMounted(() => {
  const activeDefault = ['summary', 'education', 'experience', 'skills', 'activities']
  
  if (props.resumeData?.sections) {
    props.resumeData.sections.forEach(sec => {
      if (sec.isVisible === undefined) {
        // Chỉ hiển thị các mục xuất hiện trong thiết kế ảnh
        sec.isVisible = activeDefault.includes(sec.id);
        // Quy chuẩn toàn bộ về cột chính cho giao diện 1 cột phẳng
        sec.column = 'right';
      } else {
        sec.column = 'right';
      }
    });
  }

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
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800;900&display=swap');

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

.section-block {
  position: relative;
  border-radius: 6px;
  border: 2px solid transparent;
  cursor: pointer;
  transition: border-color 0.18s ease, box-shadow 0.18s ease, background-color 0.18s ease;
}

.section-block.section-active {
  border-radius: 6px !important;
  border: 2px solid var(--active-bg, #333333) !important;
  box-shadow: 0 4px 18px rgba(0,0,0,0.08);
  z-index: 30;
}

/* Nav Control Buttons */
.nav-btns {
  position: absolute;
  display: flex;
  flex-direction: row;
  gap: 5px;
  z-index: 9999;
}

.nav-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 4px;
  background: #333333;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  box-shadow: 0 2px 6px rgba(0,0,0,0.25);
  transition: background 0.15s, transform 0.15s;
}
.nav-btn:hover { background: #111111; transform: scale(1.1); }
.nav-btn:active { transform: scale(0.95); }

/* Delete Buttons */
.delete-item-btn {
  position: absolute;
  right: 0;
  top: 0;
  width: 18px;
  height: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #ef4444;
  color: white;
  border: none;
  border-radius: 50%;
  cursor: pointer;
  box-shadow: 0 1px 4px rgba(0,0,0,0.2);
  transition: transform 0.15s;
  z-index: 35;
}
.delete-item-btn:hover { transform: scale(1.15); background: #dc2626; }
.delete-item-btn--lg {
  width: 20px;
  height: 20px;
  right: -10px;
  top: -5px;
}

.item-container {
  position: relative;
}

.paginated-item {
  transition: none; 
}

main {
  height: auto !important;
  min-height: 100%;
}

.fade-btns-enter-active,
.fade-btns-leave-active {
  transition: opacity 0.15s ease, transform 0.15s ease;
}
.fade-btns-enter-from,
.fade-btns-leave-to {
  opacity: 0;
  transform: scale(0.85);
}

:deep(.html-content ul) {
  list-style-type: disc !important;
  padding-left: 1.2rem !important;
  margin-top: 0.35rem;
  margin-bottom: 0.35rem;
}
:deep(.html-content ol) {
  list-style-type: decimal !important;
  padding-left: 1.2rem !important;
  margin-top: 0.35rem;
  margin-bottom: 0.35rem;
}
:deep(.html-content b),
:deep(.html-content strong) { font-weight: 700 !important; }
:deep(.html-content i),
:deep(.html-content em) { font-style: italic !important; }
:deep(.html-content u) { text-decoration: underline !important; }
:deep(.html-content ul li),
:deep(.html-content ol li) { margin-bottom: 0.25rem; }

@media print {
  .no-print { display: none !important; }
  .section-block,
  .section-block.section-active {
    cursor: default !important;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
    transform: none !important;
    border-radius: 0 !important;
    padding: 0 !important;
    margin: 0 !important;
  }
}

:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .section-block.section-active) {
  cursor: default !important;
  box-shadow: none !important;
  background: transparent !important;
  border-color: transparent !important;
  transform: none !important;
}
</style>
