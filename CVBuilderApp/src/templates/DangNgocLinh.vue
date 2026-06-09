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
        <div v-if="contactItems.length > 0" class="section-block relative group -mx-2 px-2 py-1 cursor-pointer hover:bg-black/5 transition-colors" 
             :class="{ 'section-active': selectedSectionId === 'contact' }"
             @click.stop="toggleSection('contact')">
          <div class="flex flex-wrap justify-center items-center gap-x-9 gap-y-1.5 text-[11.5px] text-gray-600 font-medium">
            <div v-for="(ci, ciIdx) in contactItems" :key="ci.key" class="flex items-center gap-1.5 relative contact-item-container group/ci">
              <svg v-if="ci.key !== 'website'" class="w-3.5 h-3.5 text-gray-500 shrink-0" fill="currentColor" viewBox="0 0 24 24" v-html="ci.icon"></svg>
              <svg v-else class="w-3.5 h-3.5 text-gray-500 shrink-0" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" v-html="ci.icon"></svg>
              <span v-html="ci.value"></span>

              <!-- Move Left / Move Right / Delete buttons -->
              <transition name="fade-btns">
                <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print">
                  <button v-if="ciIdx > 0" @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn" title="Di chuyển sang trái" style="padding:2px">
                    <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
                  </button>
                  <button v-if="ciIdx < contactItems.length - 1" @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn" title="Di chuyển sang phải" style="padding:2px">
                    <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
                  </button>
                  <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn-danger" title="Ẩn mục này" style="padding:2px">
                    <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                  </button>
                </div>
              </transition>
            </div>
          </div>
        </div>
      </header>

      <!-- SECTIONS CONTAINER (Khoảng cách giữa các mục giảm xuống gap-[3mm] để thon gọn) -->
      <div class="px-[18mm] pb-[14mm] flex-1 flex flex-col gap-[3mm]">
        <draggable
          v-model="mainSectionsWritable"
          item-key="id"
          group="sections"
          class="flex flex-col cursor-move"
          @end="onDragEnd"
          animation="200"
          ghost-class="opacity-30"
          :delay="100"
          :delayOnTouchOnly="true"
          :fallbackTolerance="5"
          filter=".nav-btn, .delete-btn, .contact-item-btns, .html-content, input"
        >
          <template #item="{ element: section }">
          <div
            v-if="section.isVisible"
            :data-section-id="section.id" class="section-block relative group -mx-[4mm] px-[4mm] py-[1mm] cursor-pointer hover:bg-black/5 transition-colors"
            :class="{ 'section-active': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--active-bg': '#333333' } : {}"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Control Buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print" style="right: 12px; top: 12px;" @click.stop>
                <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên"><svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống"><svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn mục này"><svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg></button>
              </div>
            </transition>

            <!-- Tiêu đề Mục -->
            <div class="paginated-item">
              <div class="flex flex-col mb-2">
                <h3 
                  class="font-extrabold uppercase tracking-[0.08em] text-black" 
                  style="font-size: 15px !important;"
                >
                  <span v-html="section.title"></span>
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
                      <span v-html="item.school"></span>
                    </h4>
                    <span class="font-bold text-[12px] text-[#111111] shrink-0 ml-4">
                      <span v-html="item.year"></span>
                    </span>
                  </div>
                  <div class="text-[12.5px] text-[#333333] font-bold">
                    <span v-html="item.major"></span>
                  </div>
                  <div v-if="item.gradType" class="text-[11.5px] text-[#555555] mt-0.5">
                    <span v-html="item.gradType"></span>
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
                      <span v-html="item.company"></span>
                    </h4>
                    <span class="font-bold text-[12px] text-[#111111] shrink-0 ml-4">
                      <span v-html="item.time || item.year"></span>
                    </span>
                  </div>
                  <div class="text-[12px] text-gray-700 font-bold mb-1 uppercase tracking-wide">
                    <span v-html="item.role || item.position"></span>
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
                    <span v-html="item.name"></span>
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
                      <span v-html="item.name"></span>
                    </h4>
                    <span class="font-bold text-[12px] text-[#111111] shrink-0 ml-4">
                      <span v-html="item.time || item.year"></span>
                    </span>
                  </div>
                  <div v-if="item.role" class="text-[12px] text-[#555555] font-semibold mb-1 italic text-gray-600">
                    <span v-html="item.role"></span>
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
                    <div class="font-bold text-[12.5px] text-black" v-if="item.name || item.title"><span v-html="item.name || item.title"></span></div>
                    <span v-if="item.year || item.time" class="text-[12px] text-gray-500 font-bold ml-2 shrink-0"><span v-html="item.year || item.time"></span></span>
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
        </draggable>
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
import draggable from 'vuedraggable'

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

// ─── CONTACT ITEMS ───
const contactIcons = {
  phone: '<path d="M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z"/>',
  email: '<path d="M20 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z"/>',
  website: '<circle cx="12" cy="12" r="10"></circle><line x1="2" y1="12" x2="22" y2="12"></line><path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"></path>',
  address: '<path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5a2.5 2.5 0 0 1 0-5 2.5 2.5 0 0 1 0 5z"/>',
  birthDate: '<path d="M19 4h-1V2h-2v2H8V2H6v2H5c-1.11 0-1.99.9-1.99 2L3 20a2 2 0 0 0 2 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H5V10h14v10zm0-12H5V6h14v2z"/>',
  facebook: '<path d="M22.675 0H1.325C.593 0 0 .593 0 1.325v21.351C0 23.407.593 24 1.325 24H12.82v-9.294H9.692v-3.622h3.128V8.413c0-3.1 1.893-4.788 4.659-4.788 1.325 0 2.463.099 2.795.143v3.24l-1.918.001c-1.504 0-1.795.715-1.795 1.763v2.313h3.587l-.467 3.622h-3.12V24h6.116c.73 0 1.323-.593 1.323-1.325V1.325C24 .593 23.407 0 22.675 0z"/>'
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

const mainSectionsWritable = ref([])

watch(mainSections, (newVal) => {
  mainSectionsWritable.value = [...newVal]
}, { immediate: true, deep: true })

const onDragEnd = () => {
  mainSectionsWritable.value.forEach(s => {
    const item = props.resumeData.sections.find(x => x.id === s.id)
    if (item) item.column = 'right'
  })

  const newOrderIds = mainSectionsWritable.value.map(s => s.id)
  
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
    right: -4px;
    top: -18px;
    display: flex;
    flex-direction: row;
    gap: 3px;
    z-index: 9999;
}

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
