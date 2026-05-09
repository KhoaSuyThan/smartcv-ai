<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-col relative box-border text-[#333] leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif' }"
    @click.self="selectedSectionId = null"
  >
    <!-- MÀU NỀN TRẮNG TOÀN BỘ CV -->
    <div class="absolute inset-0 bg-white z-0 pointer-events-none"></div>

    <!-- HEADER BLOCK DECORATION (Mảng màu xám xanh lượn sóng kèm chấm bi) -->
    <div class="absolute top-0 left-0 w-full h-[75mm] z-10 overflow-hidden pointer-events-none">
      <!-- SVG Waver with Integrated Dot Pattern -->
      <svg class="absolute inset-0 w-full h-full" viewBox="0 0 1000 200" preserveAspectRatio="none">
        <defs>
          <!-- Định nghĩa Pattern Chấm bi SVG Vector sắc nét -->
          <pattern id="dotPattern" x="0" y="0" width="12" height="12" patternUnits="userSpaceOnUse">
            <circle cx="2" cy="2" r="1.2" fill="#56708b" opacity="0.45" />
          </pattern>
        </defs>
        <!-- Lớp nền màu xám xanh lượn sóng ôm gọn avatar -->
        <path d="M 0 0 L 0 165 C 120 185, 250 200, 350 130 C 410 80, 430 0, 450 0 Z" fill="#b2c2d2" />
        <!-- Lớp chấm bi lượn sóng khớp 100% bên trong mảng -->
        <path d="M 0 0 L 0 165 C 120 185, 250 200, 350 130 C 410 80, 430 0, 450 0 Z" fill="url(#dotPattern)" />
      </svg>
    </div>

    <!-- HEADER CONTENT -->
    <header class="relative z-20 pt-[12mm] px-[12mm] pb-[4mm] flex gap-[10mm] items-center paginated-item shrink-0 w-full">
      <!-- Avatar Section (Left Side) -->
      <div class="w-[75mm] flex justify-center shrink-0">
        <div class="relative w-[50mm] h-[50mm] rounded-full border-[6px] border-white shadow-md overflow-hidden bg-slate-100 flex-shrink-0 z-10">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center bg-[#dee5ed] text-gray-400">
            <!-- Icon default avatar -->
            <svg class="w-20 h-20" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
          </div>
        </div>
      </div>

      <!-- Candidate Name & Job Title (Right Side) -->
      <div class="flex-1 pt-[8mm] pr-[4mm]">
        <!-- Họ và tên (20px, Đậm, Đen/Xám đậm) -->
        <h1 
          class="font-black leading-tight mb-2" 
          style="font-size: 24px !important; text-transform: none !important; color: #1e293b !important;"
          v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'Họ và Tên'"
        ></h1>

        <!-- Vị trí ứng tuyển (16px, Cam đất tươi) -->
        <h2 
          class="font-bold tracking-wide mb-4" 
          style="font-size: 18px !important; text-transform: none !important; color: #c25e1a !important;"
          v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'Vị trí ứng tuyển'"
        ></h2>

        <!-- Tóm tắt/Mục tiêu nghề nghiệp -->
        <div 
          class="section-block p-1 rounded-md"
          :class="{ 'section-active': selectedSectionId === 'summary' }"
          @click.stop="toggleSection('summary')"
        >
          <div 
            class="summary-text leading-relaxed text-justify text-[14px]"
            style="font-weight: bold !important; color: #475569 !important;"
            v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'MỤC TIÊU NGHỀ NGHIỆP')"
          ></div>
        </div>
      </div>
    </header>

    <!-- CONTENT BODY - 2 COLUMNS -->
    <div class="flex-1 flex flex-row relative z-20 px-[12mm] pb-[10mm] pt-[4mm]" @click.self="selectedSectionId = null">
      
      <!-- LEFT COLUMN (Sidebar - Rộng 75mm) -->
      <aside class="w-[75mm] shrink-0 flex flex-col gap-8 pr-[6mm] relative" @click.self="selectedSectionId = null">
        
        <!-- THÔNG TIN CÁ NHÂN (Static block) -->
        <div class="section-block px-1 py-1 rounded-md transition-all">
          <h3 class="font-bold uppercase text-[#1e293b] tracking-wider mb-2 pb-1 border-b-[2px] border-[#b36b2b]" style="font-size: 16px !important;">
            Thông tin cá nhân
          </h3>
          <div class="space-y-4 pt-2 text-[#334155] font-bold" style="font-size: 16px !important;">
            <!-- Số điện thoại -->
            <div class="flex items-center gap-3.5" v-if="!isEmpty(resumeData.general.phone)">
              <div class="w-8 h-8 rounded-full bg-[#b36b2b] flex items-center justify-center shrink-0 shadow-sm">
                <!-- Phone Icon -->
                <svg class="w-4 h-4 text-white" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z"/>
                </svg>
              </div>
              <span class="break-words leading-tight" v-html="resumeData.general.phone"></span>
            </div>

            <!-- Email -->
            <div class="flex items-center gap-3.5" v-if="!isEmpty(resumeData.general.email)">
              <div class="w-8 h-8 rounded-full bg-[#b36b2b] flex items-center justify-center shrink-0 shadow-sm">
                <!-- Mail Icon -->
                <svg class="w-4 h-4 text-white" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M20 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z"/>
                </svg>
              </div>
              <span class="break-all leading-tight" v-html="resumeData.general.email"></span>
            </div>

            <!-- Website -->
            <div class="flex items-center gap-3.5" v-if="!isEmpty(resumeData.general.website)">
              <div class="w-8 h-8 rounded-full bg-[#b36b2b] flex items-center justify-center shrink-0 shadow-sm">
                <!-- Globe Icon -->
                <svg class="w-4 h-4 text-white" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-1 17.93c-3.95-.49-7-3.85-7-7.93 0-.62.08-1.21.21-1.79L9 15v1c0 1.1.9 2 2 2v1.93zm6.9-2.53c-.26-.81-1-1.4-1.9-1.4h-1v-3c0-.55-.45-1-1-1h-6v-2h2c.55 0 1-.45 1-1V7h2c1.1 0 2-.9 2-2v-.41c2.93 1.19 5 4.06 5 7.41 0 2.08-.8 3.97-2.1 5.4z"/>
                </svg>
              </div>
              <span class="break-all leading-tight" v-html="resumeData.general.website"></span>
            </div>

            <!-- Địa chỉ -->
            <div class="flex items-center gap-3.5" v-if="!isEmpty(resumeData.general.address)">
              <div class="w-8 h-8 rounded-full bg-[#b36b2b] flex items-center justify-center shrink-0 shadow-sm">
                <!-- Location Icon -->
                <svg class="w-4 h-4 text-white" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5a2.5 2.5 0 0 1 0-5 2.5 2.5 0 0 1 0 5z"/>
                </svg>
              </div>
              <span class="break-words leading-tight" v-html="resumeData.general.address"></span>
            </div>
          </div>
        </div>

        <!-- DYNAMIC SIDEBAR SECTIONS -->
        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-if="section.isVisible"
            class="section-block relative group px-1 py-1 rounded-md transition-all"
            :class="{ 'section-active': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--active-bg': '#b36b2b' } : {}"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print" style="right: 5px;" @click.stop>
                <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên"><svg class="pointer-events-none" width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống"><svg class="pointer-events-none" width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải"><svg class="pointer-events-none" width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg></button>
              </div>
            </transition>

            <div class="paginated-item">
              <h3 class="font-bold uppercase text-[#1e293b] tracking-wider mb-2 pb-1 border-b-[2px] border-[#b36b2b]" style="font-size: 16px !important;">
                {{ section.title }}
              </h3>
            </div>

            <!-- THÔNG TIN BÊN TRONG CÁC MỤC -->
            <div class="space-y-4 pt-2 text-[#334155] font-bold" style="font-size: 16px !important;" v-if="sectionHasContent(section)">
              
              <!-- HỌC VẤN -->
              <div v-if="section.id.toLowerCase() === 'education'" class="space-y-5">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative leading-relaxed">
                  <div class="font-bold text-[#b36b2b] mb-0.5" style="font-size: 16px !important;">
                    {{ item.school }} <span v-if="item.year" class="font-normal text-gray-500">| {{ item.year }}</span>
                  </div>
                  <div class="font-bold text-gray-800" style="font-size: 16px !important;" v-if="item.major">{{ item.major }}</div>
                  <div class="text-gray-600 font-medium" style="font-size: 16px !important;" v-if="item.gradType || item.gpa">
                    Xếp loại tốt nghiệp: {{ item.gradType }} <span v-if="item.gpa">| GPA: {{ item.gpa }}</span>
                  </div>
                  <div v-if="item.desc" class="html-content leading-relaxed mt-1" style="font-size: 16px !important;" v-html="formatDesc(item.desc)"></div>

                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- KỸ NĂNG -->
              <div v-else-if="section.id.toLowerCase() === 'skills'" class="space-y-3">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container leading-relaxed">
                  <span class="text-[#334155] leading-snug" style="font-size: 16px !important;">{{ item.name }}</span>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- OTHER SIDEBAR ITEMS -->
              <div v-else class="space-y-4">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative leading-relaxed">
                  <div class="paginated-item font-bold text-gray-900 mb-0.5" style="font-size: 16px !important;" v-if="item.name || item.title || item.company || item.school">{{ item.name || item.title || item.company || item.school }}</div>
                  <div class="paginated-item font-medium text-gray-500 italic mb-1" style="font-size: 14px !important;" v-if="item.time || item.year">{{ item.time || item.year }}</div>
                  <div class="html-content leading-relaxed" style="font-size: 16px !important;" v-html="formatDesc(item.desc || item.info)"></div>
                  
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>
            </div>
          </div>
        </template>
      </aside>

      <!-- RIGHT COLUMN (Main Content - rộng còn lại) -->
      <main class="flex-1 flex flex-col gap-8 pl-[6mm] relative" @click.self="selectedSectionId = null">
        <!-- Đường phân tách cột mảnh nhẹ trang nhã -->
        <div class="absolute left-0 top-0 bottom-0 w-px bg-slate-200 pointer-events-none"></div>

        <!-- DYNAMIC MAIN SECTIONS -->
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-if="section.isVisible"
            class="section-block relative group px-1 py-1 rounded-md transition-all"
            :class="{ 'section-active': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--active-bg': '#b36b2b' } : {}"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print" @click.stop>
                <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên"><svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống"><svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái"><svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
              </div>
            </transition>

            <div class="paginated-item">
              <h3 class="font-bold uppercase text-[#1e293b] tracking-wider mb-2 pb-1 border-b-[2px] border-[#b36b2b]" style="font-size: 16px !important;">
                {{ section.title }}
              </h3>
            </div>

            <!-- THÔNG TIN BÊN TRONG CÁC MỤC -->
            <div class="space-y-6 pt-2 text-[#334155] font-bold" style="font-size: 16px !important;" v-if="sectionHasContent(section)">
              <!-- KINH NGHIỆM LÀM VIỆC -->
              <div v-if="section.id.toLowerCase() === 'experience'" class="space-y-6">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative leading-relaxed">
                  <div class="font-bold text-[#b36b2b]" style="font-size: 16px !important;">
                    {{ item.company }} <span v-if="item.time" class="font-normal text-gray-500">| {{ item.time }}</span>
                  </div>
                  <div class="font-bold text-gray-800 mb-2" style="font-size: 16px !important;" v-if="item.role">{{ item.role }}</div>
                  <div class="html-content leading-relaxed list-outside pl-4 text-justify" style="font-size: 16px !important;" v-html="formatDesc(item.desc)"></div>

                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- OTHER MAIN ABSTRACTIONS -->
              <div v-else class="space-y-5">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative leading-relaxed">
                  <div class="paginated-item font-bold text-gray-900" style="font-size: 16px !important;" v-if="item.name || item.title || item.company || item.school">{{ item.name || item.title || item.company || item.school }}</div>
                  <div class="paginated-item font-medium text-gray-500 italic" style="font-size: 14px !important;" v-if="item.time || item.year">{{ item.time || item.year }}</div>
                  <div class="html-content leading-relaxed mt-1" style="font-size: 16px !important;" v-html="formatDesc(item.desc || item.info)"></div>

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
      </main>
    </div>

    <!-- WATERMARK FOOTER -->
    <div class="absolute bottom-[6mm] right-[10mm] text-[12px] font-medium text-gray-400 no-print" style="letter-spacing: 0.05em;">
      @CVBuilder
    </div>

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
  if (section.id === 'summary') return true
  if (!section) return false
  return section.items && section.items.length > 0
}

const formatDesc = (text) => {
  if (!text) return ''
  
  if (!/<[a-z][\s\S]*>/i.test(text)) {
     return text.split('\n')
                .map(l => l.trim())
                .filter(Boolean)
                .map(l => `<div class="paginated-item" style="font-size: 16px !important;">${l}</div>`)
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
        div.className = 'paginated-item min-h-[16px]'
        div.style.fontSize = '16px'
        div.appendChild(node.cloneNode(true))
        container.appendChild(div)
      }
    } else if (node.nodeType === Node.ELEMENT_NODE) {
      if (node.tagName === 'BR') {
        const div = document.createElement('div')
        div.className = 'paginated-item h-[16px]'
        container.appendChild(div)
      } else if (['UL', 'OL'].includes(node.tagName)) {
        Array.from(node.children).forEach(li => {
          li.classList.add('paginated-item')
          li.style.fontSize = '16px'
        })
        container.appendChild(node.cloneNode(true))
      } else {
        node.classList.add('paginated-item')
        node.style.fontSize = '16px'
        container.appendChild(node.cloneNode(true))
      }
    }
  })

  return container.innerHTML
}

const sidebarSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'left' && s.id.toLowerCase() !== 'summary')
)
const mainSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'right' && s.id.toLowerCase() !== 'summary')
)
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
  // Cưỡng chế chỉ kích hoạt các mục có trong ảnh: education, skills, experience, và summary
  const activeLeft = ['education', 'skills']
  const activeRight = ['experience']
  const activeSpecial = ['summary']

  if (props.resumeData && props.resumeData.sections) {
    props.resumeData.sections.forEach(sec => {
      const id = sec.id.toLowerCase()
      if (activeLeft.includes(id)) {
        sec.isVisible = true
        sec.column = 'left'
      } else if (activeRight.includes(id)) {
        sec.isVisible = true
        sec.column = 'right'
      } else if (activeSpecial.includes(id)) {
        sec.isVisible = true
      } else {
        sec.isVisible = false
      }
    })
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
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&display=swap');

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
  font-weight: 700 !important;
}

#cv-printable-area h1 {
  font-weight: 900 !important;
}

#cv-printable-area h2,
#cv-printable-area h3 {
  font-weight: 800 !important;
}

.section-block {
  position: relative;
  border-radius: 6px;
  border: 2px solid transparent;
  cursor: pointer;
  transition: box-shadow 0.18s ease, border-color 0.18s ease;
}

.section-block.section-active {
  transform: none !important;
  border-radius: 6px !important;
  border: 2px solid var(--active-bg, #b36b2b) !important;
  box-shadow: 0 4px 18px rgba(0,0,0,0.1) !important;
  z-index: 10;
}

.nav-btns {
  position: absolute;
  right: 10px;
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
  right: 4px;
  top: 4px;
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
  right: 6px;
  top: 6px;
}

.item-container {
  position: relative;
  min-height: 24px;
  padding-right: 28px;
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
