<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-row relative box-border text-[#333] leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif' }"
    @click.self="selectedSectionId = null"
  >
    <!-- MÀU NỀN TRẮNG -->
    <div class="absolute inset-0 bg-white z-0 pointer-events-none"></div>

    <!-- LEFT COLUMN -->
    <aside class="w-[75mm] z-10 flex flex-col pt-0 shrink-0 relative bg-[#f9f9f9]">
      
      <!-- Avatar Section -->
      <div class="pt-[14mm] px-[8mm] pb-[4mm] flex flex-col paginated-item relative z-20 items-center bg-[#f9f9f9]">
        <div class="relative w-[50mm] h-[50mm] rounded-full overflow-hidden mx-auto bg-gray-200 shadow-sm border-[2px] border-white z-10">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center bg-gray-200 text-gray-500">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
          </div>
        </div>
      </div>

      <!-- Sidebar Sections -->
      <div class="w-full pl-0 pr-0 flex-1 pb-8 flex flex-col mt-4">
        <!-- CONTACT INFOMATION -->
        <div class="section-block relative group mb-6 px-[6mm]" @click.stop="toggleSection('contact')">
          <div class="paginated-item">
            <div class="bg-[#3b715a] text-white py-[6px] pl-5 pr-4 rounded-r-full font-bold text-[14px] w-[80%] mb-4 -ml-[6mm] uppercase shadow-sm flex items-center tracking-wide">
              LIÊN HỆ
            </div>
            <div class="space-y-3 px-2 text-[11px] font-medium text-gray-700">
              <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.birthDate)">
                <div class="w-4 flex items-center justify-center shrink-0 mt-[1px] text-gray-800">
                   <svg class="w-[14px] h-[14px]" fill="currentColor" viewBox="0 0 24 24"><path d="M19 4h-1V2h-2v2H8V2H6v2H5c-1.11 0-1.99.9-1.99 2L3 20a2 2 0 0 0 2 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H5V10h14v10zm0-12H5V6h14v2z"/></svg>
                </div>
                <span class="break-all leading-tight" v-html="resumeData.general.birthDate"></span>
              </div>
              <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.address)">
                <div class="w-4 flex items-center justify-center shrink-0 mt-[1px] text-gray-800">
                   <svg class="w-[14px] h-[14px]" fill="currentColor" viewBox="0 0 24 24"><path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5a2.5 2.5 0 0 1 0-5 2.5 2.5 0 0 1 0 5z"/></svg>
                </div>
                <span class="break-all leading-tight" v-html="resumeData.general.address"></span>
              </div>
              <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.phone)">
                <div class="w-4 flex items-center justify-center shrink-0 mt-[1px] text-gray-800">
                   <svg class="w-[14px] h-[14px]" fill="currentColor" viewBox="0 0 24 24"><path d="M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z"/></svg>
                </div>
                <span class="break-all leading-tight" v-html="resumeData.general.phone"></span>
              </div>
              <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.facebook)">
                <div class="w-4 flex items-center justify-center shrink-0 mt-[1px] text-gray-800">
                   <svg class="w-[14px] h-[14px]" fill="currentColor" viewBox="0 0 24 24"><path d="M22.675 0H1.325C.593 0 0 .593 0 1.325v21.351C0 23.407.593 24 1.325 24H12.82v-9.294H9.692v-3.622h3.128V8.413c0-3.1 1.893-4.788 4.659-4.788 1.325 0 2.463.099 2.795.143v3.24l-1.918.001c-1.504 0-1.795.715-1.795 1.763v2.313h3.587l-.467 3.622h-3.12V24h6.116c.73 0 1.323-.593 1.323-1.325V1.325C24 .593 23.407 0 22.675 0z"/></svg>
                </div>
                <span class="break-all leading-tight">{{ resumeData.general.facebook }}</span>
              </div>
              <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.github) || !isEmpty(resumeData.general.website) || !isEmpty(resumeData.general.linkedin)">
                <div class="w-4 flex items-center justify-center shrink-0 mt-[1px] text-gray-800">
                   <svg class="w-[14px] h-[14px]" fill="currentColor" viewBox="0 0 24 24"><path d="M12 .297c-6.63 0-12 5.373-12 12 0 5.303 3.438 9.8 8.205 11.385.6.113.82-.258.82-.577 0-.285-.01-1.04-.015-2.04-3.338.724-4.042-1.61-4.042-1.61C4.422 18.07 3.633 17.7 3.633 17.7c-1.087-.744.084-.729.084-.729 1.205.084 1.838 1.236 1.838 1.236 1.07 1.835 2.809 1.305 3.495.998.108-.776.417-1.305.76-1.605-2.665-.3-5.466-1.332-5.466-5.93 0-1.31.465-2.38 1.235-3.22-.135-.303-.54-1.523.105-3.176 0 0 1.005-.322 3.3 1.23.96-.267 1.98-.399 3-.405 1.02.006 2.04.138 3 .405 2.28-1.552 3.285-1.23 3.285-1.23.645 1.653.24 2.873.12 3.176.765.84 1.23 1.91 1.23 3.22 0 4.61-2.805 5.625-5.475 5.92.42.36.81 1.096.81 2.22 0 1.606-.015 2.896-.015 3.286 0 .315.21.69.825.57C20.565 22.092 24 17.592 24 12.297c0-6.627-5.373-12-12-12"/></svg>
                </div>
                <span class="break-all leading-tight">{{
                  !isEmpty(resumeData.general.github)
                    ? resumeData.general.github
                    : (!isEmpty(resumeData.general.linkedin)
                      ? resumeData.general.linkedin
                      : resumeData.general.website)
                }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- OTHER DYNAMIC SIDEBAR SECTIONS -->
        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-if="section.isVisible"
            class="section-block relative group mb-6 px-[6mm]"
            :class="{ 'section-active': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--active-bg': templatePrimaryColor } : {}"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print" style="right: -4px;">
                <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7-7"/></svg></button>
              </div>
            </transition>

            <div class="paginated-item">
              <div class="bg-[#3b715a] text-white py-[6px] pl-5 pr-4 rounded-r-full font-bold text-[14px] w-[80%] mb-4 -ml-[6mm] uppercase shadow-sm flex items-center tracking-wide">
                {{ section.title }}
              </div>
            </div>

            <div class="space-y-4 px-2" v-if="sectionHasContent(section)">
              <!-- KỸ NĂNG -->
              <div v-if="['skills'].includes(section.id)" class="space-y-4">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="paginated-item item-container"
                >
                  <div class="text-[10px] uppercase font-semibold text-gray-700 mb-1.5">{{ item.name }}</div>
                  <div class="w-full h-[5px] bg-gray-200 shadow-inner">
                    <div class="h-full" :style="{ width: getLevelInfo(item.level).percent, backgroundColor: templatePrimaryColor }"></div>
                  </div>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: -5px; right: -5px;">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- CHỨNG CHỈ -->
              <div v-else-if="['awards','certifications'].includes(section.id)" class="space-y-4">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="paginated-item item-container leading-relaxed"
                >
                  <div class="flex items-baseline gap-2 mb-1">
                    <span class="font-bold text-[10.5px] uppercase text-gray-800" style="letter-spacing: 0.02em;">{{ item.name }}</span>
                    <span v-if="item.year" class="text-[10.5px] italic" :style="{ color: '#568e77' }">{{ item.year }}</span>
                  </div>
                  <div class="text-[10.5px] text-gray-700 font-medium leading-[1.6] html-content" v-html="formatDesc(item.info || item.desc)"></div>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: -5px; right: -5px;">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- OTHER FALLBACK SIDEBAR ITEMS -->
              <div v-else class="space-y-4">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="item-container relative"
                >
                  <div class="paginated-item font-bold text-[10.5px] uppercase text-gray-800 mb-1" v-if="item.name || item.title || item.company || item.school">{{ item.name || item.title || item.company || item.school }}</div>
                  <div class="paginated-item font-medium text-[10px] text-gray-500 italic mb-1" v-if="item.time || item.year">{{ item.time || item.year }}</div>
                  <div class="text-[10.5px] text-gray-700 font-medium leading-[1.6] html-content" v-html="formatDesc(item.desc || item.info || item.role)"></div>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: -5px; right: -5px;">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>
            </div>
          </div>
        </template>
      </div>
    </aside>

    <!-- RIGHT COLUMN -->
    <main class="flex-1 flex flex-col relative z-20 min-h-max" @click.self="selectedSectionId = null">

      <!-- HEADER NAME TITLE (Right side) -->
      <header
        class="paginated-item pt-[16mm] px-[10mm] pb-[10mm] flex flex-col relative bg-[#3b715a]"
      >
        <!-- Decor on background -->
        <div class="absolute bottom-0 right-0 w-48 h-full pointer-events-none opacity-[0.03]" style="background-image: repeating-linear-gradient(-45deg, transparent, transparent 10px, #ffffff 10px, #ffffff 20px);"></div>
        
        <h1 class="font-extrabold uppercase tracking-[0.03em] mb-2 leading-tight text-white z-10" style="font-size: 26px !important; text-shadow: 0px 2px 4px rgba(0,0,0,0.15);" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'ĐINH XUÂN THẢO'"></h1>
        <h2 class="font-semibold uppercase tracking-[0.18em] text-white/70 z-10" style="font-size: 13px !important" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'PRODUCT MANAGER'"></h2>
      </header>

      <!-- Main Sections -->
      <div class="px-[10mm] pt-[10mm] pb-[10mm] flex-1 flex flex-col gap-[8mm]">
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-if="section.isVisible"
            class="section-block relative group my-0 py-1"
            :class="{ 'section-active': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--active-bg': templatePrimaryColor } : {}"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
                <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
              </div>
            </transition>

            <div class="paginated-item">
              <h3 class="section-title font-bold mb-5 tracking-wide flex items-center gap-3 text-[18px]" :style="{ color: templatePrimaryColor }">
                <div class="w-[6px] h-[22px]" :style="{ backgroundColor: templatePrimaryColor }"></div>
                {{ section.title }}
              </h3>
            </div>

            <div class="space-y-6" v-if="sectionHasContent(section)">
              
              <!-- SUMMARY -->
              <div v-if="section.id === 'summary'" class="item-container relative">
                <div class="text-[12px] text-gray-700 leading-[1.7] text-justify font-medium html-content" v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Với hơn hai năm kinh nghiệm ở các vị trí Product Manager, Business Analyst... tôi mong muốn tận dụng kỹ năng và kiến thức của mình để đóng góp cho công ty.')"></div>
              </div>

              <!-- EXPERIENCE / PROJECT / ACTIVITIES / EDUCATION -->
              <div v-else class="space-y-6">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                  <div class="paginated-item">
                    <div class="flex justify-between items-baseline mb-1">
                      <h4 class="font-bold text-[13.5px] uppercase flex items-center gap-2 tracking-wide" :style="{ color: templatePrimaryColor }">
                        <span class="text-[9px] mb-0.5">◆</span>
                        <span v-html="section.id === 'experience' ? item.company : (item.school || item.name || '')"></span>
                      </h4>
                      <span v-if="item.time || item.year" class="font-medium text-[11.5px] italic tracking-wide" :style="{ color: '#6c9b83' }">{{ item.time || item.year }}</span>
                    </div>
                    <div v-if="item.role || item.major" class="font-bold text-gray-900 mb-2 text-[12.5px]">{{ item.role || item.major }}</div>
                    <div v-if="item.gradType" class="text-[11.5px] text-gray-600 mb-1.5 font-medium">Tốt nghiệp loại: <span class="font-bold">{{ item.gradType }}</span><span v-if="item.gpa"> | GPA: {{ item.gpa }}</span></div>
                  </div>
                  
                  <div class="text-[11.5px] text-gray-700 leading-[1.65] text-justify font-medium html-content" v-html="formatDesc(item.desc || item.info)"></div>

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
      <div class="absolute bottom-[6mm] right-[10mm] text-[11.5px] font-medium text-gray-400 no-print" style="letter-spacing: 0.05em;">
        @CVBuilder
      </div>
    </main>

    <!-- VIỀN CUỐI TRANG CỐ ĐỊNH -->
    <template v-for="p in pageCount" :key="'footer-border-' + p">
      <div
        class="absolute left-0 w-full flex items-center z-40 pointer-events-none"
        :style="{ top: `calc(${p * 297}mm - 12mm)`, height: '1.5px', paddingLeft: '12mm', paddingRight: '12mm' }"
      >
        <div class="w-full h-full opacity-0"></div>
      </div>
    </template>

    <!-- ĐƯỜNG PHÂN TRANG CHO IN ẤN & XUẤT PDF (TRỰC QUAN TRÊN TRÌNH DUYỆT) -->
    <template v-for="p in (pageCount - 1)" :key="'div-' + p">
      <div
        class="absolute left-0 w-full z-50 flex flex-col items-center justify-center pointer-events-none no-print"
        :style="{ top: `calc(${p * 297}mm - 8px)` }"
      >
        <div class="w-[105%] h-[16px] bg-gray-800/90 shadow-inner overflow-hidden border-y border-black/30 backdrop-blur-sm"></div>
        <span class="absolute text-[9px] uppercase font-bold text-gray-300 tracking-widest bg-gray-700 px-3 py-0.5 rounded border border-gray-600 shadow-md">Ngắt trang {{ p + 1 }}</span>
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
  if (section.id === 'summary') return true;
  if (!section) return false
  return section.items && section.items.length > 0
}

const formatDesc = (text) => {
  if (!text) return ''
  
  if (!/<[a-z][\s\S]*>/i.test(text)) {
     return text.split('\n')
                .map(l => l.trim())
                .filter(Boolean)
                .map(l => `<div class="paginated-item">${l}</div>`)
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

const getLevelInfo = (level) => {
  if (!level) return { text: '', percent: '80%' }
  const l = level.toLowerCase().trim()
  if (l === 'cơ bản') return { text: 'Cơ bản', percent: '25%' }
  if (l === 'trung cấp') return { text: 'Trung cấp', percent: '50%' }
  if (l === 'thành thạo') return { text: 'Thành thạo', percent: '75%' }
  if (l === 'chuyên gia') return { text: 'Chuyên gia', percent: '100%' }
  if (level.includes('%')) return { text: level, percent: level }
  const num = parseInt(level)
  if (!isNaN(num)) return { text: num + '%', percent: num + '%' }
  return { text: level, percent: '80%' }
}

const templatePrimaryColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  if (!c || c.toLowerCase() === '#2b5c8f') return '#3b715a'
  return c
})

const sidebarSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'left' && !['summary', 'it_skills', 'languages', 'education'].includes(s.id))
)
const mainSections = computed(() => {
  const sections = props.resumeData.sections.filter(s => 
    (s.column === 'right' || s.id === 'education') && 
    !['it_skills', 'languages'].includes(s.id)
  )
  return sections
})
const sidebarIds = computed(() => sidebarSections.value.map(s => s.id))
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
  const defaultVisible = ['summary', 'experience', 'skills', 'certifications']
  
  let activeSections = props.resumeData?.sections?.filter(s => s.isVisible).length || 0;
  if(activeSections > 5) {
     props.resumeData.sections.forEach(sec => {
        sec.isVisible = defaultVisible.includes(sec.id);
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
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap');

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

.section-title {
  font-size: 18px !important;
  color: #000000 !important;
  font-weight: bold !important;
}

.section-block {
  position: relative;
  border-radius: 6px;
  border: 2px solid transparent;
  cursor: pointer;
  transition: transform 0.18s ease, box-shadow 0.18s ease, border-color 0.18s ease;
}

.section-block.section-active {
  transform: scale(1.012);
  border-radius: 6px !important;
  border: 2px solid var(--active-bg, #3b715a) !important;
  box-shadow: 0 4px 18px rgba(0,0,0,0.10);
  z-index: 10;
}

.nav-btns {
  position: absolute;
  right: 6px;
  top: 6px;
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
  background: #2563eb;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  box-shadow: 0 2px 6px rgba(37, 99, 235, 0.4);
  transition: background 0.15s, transform 0.15s;
}
.nav-btn:hover { background: #1d4ed8; transform: scale(1.1); }
.nav-btn:active { transform: scale(0.95); }

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
  z-index: 30;
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
  padding-left: 1.25rem !important;
  margin-top: 0.35rem;
  margin-bottom: 0.35rem;
}
:deep(.html-content ol) {
  list-style-type: decimal !important;
  padding-left: 1.25rem !important;
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
