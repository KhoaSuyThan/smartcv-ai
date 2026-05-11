<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-row relative box-border text-[#2F2926] leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Montserrat\', \'Inter\', sans-serif' }"
    @click.self="selectedSectionId = null"
  >
    <!-- BACKGROUND DECORATIONS (TRANG TRÍ NỀN CỘT TRÁI) -->
    <div class="absolute top-0 left-0 w-[72mm] h-[80mm] bg-[#EDE4DC] z-0 pointer-events-none"></div>
    <div class="absolute bottom-0 left-0 w-[72mm] h-[80mm] bg-[#EDE4DC] z-0 pointer-events-none"></div>

    <!-- LEFT COLUMN (CỘT TRÁI) -->
    <aside class="w-[72mm] z-10 flex flex-col pt-[14mm] pb-[12mm] shrink-0 relative bg-transparent">
      <!-- WHITE CARD CONTAINER (Khung chữ nhật màu trắng viền đen bao quanh toàn bộ avt và nội dung cột trái) -->
      <div class="mx-auto w-[62mm] bg-white border border-[#2F2926] px-[20px] pt-[20px] pb-[20px] flex flex-col gap-6 relative z-10 flex-1">

        
        <!-- Avatar Section (Đặt bên trong khung chữ nhật trắng viền đen) -->
        <div class="pb-[2mm] flex flex-col paginated-item items-center">
          <div class="relative w-[48mm] h-[48mm] rounded-full overflow-hidden mx-auto bg-[#EAE5DF] shadow-sm border-[3px] border-white">
            <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
            <div v-else class="w-full h-full flex items-center justify-center bg-gray-200 text-gray-400">
              <svg class="w-20 h-20" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
            </div>
          </div>
        </div>

        <!-- LIÊN LẠC SECTION -->
        <div class="section-block relative group" :class="{ 'section-active': selectedSectionId === 'contact' }" @click.stop="toggleSection('contact')">
          <div class="paginated-item">
            <h3 class="font-bold uppercase tracking-[0.15em] text-[#2F2926] mb-4" style="font-size: 20px !important;">
              LIÊN LẠC
            </h3>
            <div class="space-y-3 text-[11px] font-medium text-[#2F2926] pl-0.5">
              <!-- Email (Chỉ hiển thị dòng nếu có dữ liệu) -->
              <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.email)">
                <div class="shrink-0 mt-[2px] text-[#2F2926]">
                  <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                  </svg>
                </div>
                <span class="break-all leading-tight" v-html="resumeData.general.email"></span>
              </div>
              <!-- Phone -->
              <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.phone)">
                <div class="shrink-0 mt-[2px] text-[#2F2926]">
                  <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.94.725l.548 2.2a1 1 0 01-.321.988l-1.305.98a10.582 10.582 0 004.872 4.872l.98-1.305a1 1 0 01.988-.321l2.2.548a1 1 0 01.725.94V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
                  </svg>
                </div>
                <span class="break-all leading-tight" v-html="resumeData.general.phone"></span>
              </div>
              <!-- Address -->
              <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.address)">
                <div class="shrink-0 mt-[1px] text-[#2F2926]">
                  <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                </div>
                <span class="break-all leading-tight" v-html="resumeData.general.address"></span>
              </div>
              <!-- Website -->
              <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.website)">
                <div class="shrink-0 mt-[1px] text-[#2F2926]">
                  <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
                  </svg>
                </div>
                <span class="break-all leading-tight" v-html="resumeData.general.website"></span>
              </div>
            </div>
          </div>
        </div>

        <!-- DYNAMIC SIDEBAR SECTIONS (KỸ NĂNG, REFERENCE...) -->
        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-if="section.isVisible"
            class="section-block relative group"
            :class="{ 'section-active': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--active-bg': '#2F2926' } : {}"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Buttons (Thanh điều khiển section) -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print" @click.stop>
                <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên">
                  <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                </button>
                <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống">
                  <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                </button>
                <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải">
                  <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
                </button>
              </div>
            </transition>

            <!-- Tiêu đề đề mục (Cỡ chữ 20px) -->
            <div class="paginated-item">
              <h3 class="font-bold uppercase tracking-[0.15em] text-[#2F2926] mb-4" style="font-size: 20px !important;">
                {{ section.title }}
              </h3>
            </div>

            <!-- Content of Section -->
            <div class="space-y-3" v-if="sectionHasContent(section)">
              <!-- KỸ NĂNG (skills) -->
              <div v-if="section.id.toLowerCase().includes('skill')" class="space-y-3">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="paginated-item text-[11px] font-medium text-[#2F2926] leading-relaxed item-container flex items-start gap-2 relative group/item"
                >
                  <span class="mt-[5px] w-1.5 h-1.5 bg-[#2F2926] rounded-full shrink-0"></span>
                  <span class="flex-1 text-justify">
                    <span class="font-semibold">{{ item.name }}</span>
                    <span v-if="item.info" class="font-normal"> ({{ item.info }})</span>
                    <span v-else-if="item.level" class="font-normal"> ({{ item.level }})</span>
                  </span>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- REFERENCE (references) -->
              <div v-else-if="section.id.toLowerCase().includes('reference')" class="space-y-4">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="paginated-item item-container leading-relaxed text-[11px] font-medium text-[#2F2926] relative group/item"
                >
                  <div class="html-content text-justify font-medium" v-html="formatDesc(item.info || item.desc || item.name)"></div>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- FALLBACK SIDEBAR ITEMS -->
              <div v-else class="space-y-3">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="paginated-item item-container leading-relaxed text-[11px] font-medium text-[#2F2926] relative group/item"
                >
                  <div class="font-bold text-[11.5px] mb-0.5" v-if="item.name || item.title">{{ item.name || item.title }}</div>
                  <div class="html-content" v-html="formatDesc(item.desc || item.info)"></div>
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
      </div>
    </aside>

    <!-- RIGHT COLUMN (CỘT PHẢI) -->
    <main class="flex-1 flex flex-col relative bg-white z-20 min-h-max" @click.self="selectedSectionId = null">
      <!-- HEADER: TÊN + VỊ TRÍ TRÊN CỘT PHẢI (Luôn hiển thị với dữ liệu thực tế hoặc mặc định của mẫu) -->
      <header class="paginated-item pt-[20mm] px-[12mm] pb-[10mm] flex flex-col min-h-min">
        <h1 class="font-black uppercase tracking-wide leading-[0.95] mb-2 text-[#2F2926]" style="font-size: 38px !important; font-family: 'Montserrat', sans-serif;">
          <span v-html="nameLines.firstLine"></span><br>
          <span v-html="nameLines.secondLine"></span>
        </h1>
        <h2 class="font-bold uppercase tracking-[0.2em] text-[#5C524A] mt-2 mb-4" style="font-size: 13.5px !important">
          <span v-html="resumeData.general.jobTitle || 'NHÂN VIÊN BÁN HÀNG'"></span>
        </h2>
      </header>

      <!-- CONTENT SECTIONS (SUMMARY, KINH NGHIỆM, HỌC VẤN) -->
      <div class="px-[12mm] pb-[12mm] flex-1 flex flex-col gap-[7mm]">
        <!-- SUMMARY / GIỚI THIỆU BẢN THÂN -->
        <div class="section-block relative group py-1" v-if="!isEmpty(resumeData.general.summary)" :class="{ 'section-active': selectedSectionId === 'summary' }" @click.stop="toggleSection('summary')">
          <div class="paginated-item leading-[1.8] text-[#4A433F] text-justify text-[11.5px] font-medium html-content" v-html="formatDesc(resumeData.general.summary)"></div>
        </div>

        <!-- DYNAMIC MAIN SECTIONS -->
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-if="section.isVisible"
            class="section-block relative group py-1"
            :class="{ 'section-active': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--active-bg': '#2F2926' } : {}"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Buttons (Thanh điều khiển) -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print" @click.stop>
                <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                </button>
                <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                </button>
                <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
                </button>
              </div>
            </transition>

            <!-- Tiêu đề đề mục (Cỡ chữ 20px) -->
            <div class="paginated-item">
              <h3 class="font-bold uppercase tracking-[0.15em] text-[#2F2926] mb-5" style="font-size: 20px !important;">
                {{ section.title }}
              </h3>
            </div>

            <!-- Content -->
            <div class="space-y-6" v-if="sectionHasContent(section)">
              <!-- KINH NGHIỆM (experience) -->
              <div v-if="section.id.toLowerCase().includes('experience')" class="space-y-6">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative group/item">
                  <div class="paginated-item">
                    <div class="flex justify-between items-baseline mb-2">
                      <h4 class="text-[#2F2926] font-bold text-[12px] uppercase">
                        <span class="font-normal normal-case" v-if="item.role && item.role.toLowerCase().startsWith('thực tập')">Thực tập </span>
                        <span class="font-bold" v-html="formatCleanRole(item.role)"></span>
                      </h4>
                      <span v-if="item.time" class="font-medium text-[#4A433F] text-[11.5px] italic shrink-0 ml-4">{{ item.time }}</span>
                    </div>
                  </div>
                  
                  <!-- Mô tả công việc chi tiết -->
                  <div class="text-[#4A433F] leading-[1.8] text-justify text-[11.5px] font-medium html-content text-list-style" v-html="formatDesc(item.desc)"></div>

                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- HỌC VẤN (education) -->
              <div v-else-if="section.id.toLowerCase().includes('education')" class="space-y-5">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative group/item">
                  <div class="paginated-item">
                    <div class="text-[#4A433F] leading-[1.8] text-justify text-[11.5px] font-medium">
                      <span class="font-bold text-[#2F2926]" v-if="item.major">Chuyên ngành: {{ item.major }}</span>
                      <span v-if="item.school"> - {{ item.school }}</span>
                      <span class="font-bold" v-if="item.gpa || item.gradType"> 
                        (GPA: {{ item.gpa || item.gradType }})
                      </span>
                    </div>
                    <div v-if="item.desc" class="text-[#4A433F] leading-[1.8] text-justify text-[11.5px] font-medium html-content mt-1" v-html="formatDesc(item.desc)"></div>
                  </div>

                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- OTHER MAIN SECTIONS -->
              <div v-else class="space-y-4">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative group/item">
                  <div class="paginated-item flex justify-between items-baseline mb-1">
                    <div class="font-bold text-[12px] text-[#2F2926] html-content" v-if="item.name || item.title" v-html="formatDesc(item.name || item.title)"></div>
                    <p v-if="item.year || item.time" class="text-[11px] text-gray-500 italic ml-2 whitespace-nowrap">{{ item.year || item.time }}</p>
                  </div>
                  <div class="text-[11.5px] text-[#4A433F] html-content" v-html="formatDesc(item.desc || item.info)"></div>

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
    </main>

    <!-- VIỀN CUỐI TRANG CỐ ĐỊNH -->
    <template v-for="p in pageCount" :key="'footer-border-' + p">
      <div
        class="absolute left-0 w-full flex items-center z-40 pointer-events-none"
        :style="{ top: `calc(${p * 297}mm - 12mm)`, height: '1.5px', paddingLeft: '12mm', paddingRight: '12mm' }"
      >
        <div class="w-full h-full opacity-30 bg-gradient-to-r from-slate-400 via-slate-500 to-slate-400"></div>
      </div>
    </template>

    <!-- ĐƯỜNG PHÂN TRANG CHO TRÌNH DUYỆT -->
    <template v-for="p in (pageCount - 1)" :key="'div-' + p">
      <div
        class="absolute left-0 w-full z-50 flex flex-col items-center justify-center pointer-events-none no-print"
        :style="{ top: `calc(${p * 297}mm - 8px)` }"
      >
        <div class="w-[105%] h-[16px] bg-[#2F2926]/95 shadow-inner overflow-hidden border-y border-black/30 backdrop-blur-sm"></div>
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
  if (!section) return false
  return section.items && section.items.length > 0
}

// Hàm format và tạo thẻ .paginated-item cho Rich Text
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

// Xử lý hiển thị vị trí (Role) loại bỏ chữ 'Thực tập' ở đầu nếu có vì đã render riêng
const formatCleanRole = (role) => {
  if (!role) return ''
  const clean = role.replace(/<[^>]*>/g, '').trim()
  if (clean.toLowerCase().startsWith('thực tập')) {
    return clean.substring(8).trim().toUpperCase()
  }
  return clean.toUpperCase()
}

// Tách tên thành 2 dòng (Họ & Tên riêng) cho giống ảnh thiết kế
const nameLines = computed(() => {
  const name = props.resumeData?.general?.fullName
  if (isEmpty(name)) {
    return { firstLine: 'SAMIRA', secondLine: 'NGUYEN' }
  }
  const cleanName = name.replace(/<[^>]*>/g, '').trim()
  const words = cleanName.split(' ')
  if (words.length <= 1) {
    return { firstLine: cleanName, secondLine: '' }
  }
  // Lấy từ cuối cùng làm dòng 2, các từ trước làm dòng 1
  const secondLine = words[words.length - 1]
  const firstLine = words.slice(0, words.length - 1).join(' ')
  return { firstLine, secondLine }
})

// Chia các sections động về đúng cột và ẩn/hiện đúng mục có trong ảnh
const sidebarSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'left' && s.id !== 'summary')
)
const mainSections = computed(() => {
  const sections = props.resumeData.sections.filter(s => 
    (s.column === 'right' || s.id === 'education') && s.id !== 'summary'
  )
  // Sắp xếp: education luôn nằm dưới experience
  const expIndex = sections.findIndex(s => s.id === 'experience')
  const eduIndex = sections.findIndex(s => s.id === 'education')
  if (expIndex !== -1 && eduIndex !== -1 && eduIndex < expIndex) {
    const [edu] = sections.splice(eduIndex, 1)
    const newExpIndex = sections.findIndex(s => s.id === 'experience')
    sections.splice(newExpIndex + 1, 0, edu)
  }
  return sections
})

const sidebarIds = computed(() => sidebarSections.value.map(s => s.id))
const mainIds = computed(() => mainSections.value.map(s => s.id))

// ─── PAGINATION ENGINE (BỘ PHÂN TRANG TỰ ĐỘNG) ───
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
  // Chỉ bật hiển thị các section có trong ảnh: skills, references, experience, education
  const allowedIds = ['skills', 'references', 'experience', 'education', 'summary']
  
  if (props.resumeData?.sections) {
    props.resumeData.sections.forEach(sec => {
      sec.isVisible = allowedIds.includes(sec.id.toLowerCase())
      
      // Gán cột chuẩn theo thiết kế
      if (['skills', 'references'].includes(sec.id.toLowerCase())) {
        sec.column = 'left'
      } else if (['experience', 'education'].includes(sec.id.toLowerCase())) {
        sec.column = 'right'
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
@import url('https://fonts.googleapis.com/css2?family=Montserrat:wght@400;500;600;700;800;900&family=Inter:wght@400;500;600;700&display=swap');

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

.section-block {
  position: relative;
  border-radius: 4px;
  border: 1.5px solid transparent;
  cursor: pointer;
  transition: box-shadow 0.18s ease, border-color 0.18s ease;
  padding: 15px 20px; /* Padding giúp viền cách nội dung trên 15px và trái/phải đúng 20px cực kỳ cân đối */
}

.section-block.section-active {
  border-radius: 4px !important;
  border: 1.5px solid var(--active-bg, #2F2926) !important;
  box-shadow: 0 4px 12px rgba(0,0,0,0.08);
  z-index: 10;
}

.nav-btns {
  position: absolute;
  right: 6px;
  top: 6px;
  display: flex;
  flex-direction: row;
  gap: 4px;
  z-index: 9999;
}

.nav-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 3px;
  background: #2563eb;
  color: white;
  border: none;
  border-radius: 3px;
  cursor: pointer;
  box-shadow: 0 1px 4px rgba(37, 99, 235, 0.4);
  transition: background 0.15s, transform 0.15s;
}
.nav-btn:hover { background: #1d4ed8; transform: scale(1.1); }
.nav-btn:active { transform: scale(0.95); }

.delete-item-btn {
  position: absolute;
  right: -12px; /* Nằm cân đối chính giữa khoảng đệm 20px bên phải */
  top: 6px;    /* Đứng thẳng hàng với dòng đầu tiên của mục con */
  width: 16px;
  height: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #ef4444;
  color: white;
  border: none;
  border-radius: 50%;
  cursor: pointer;
  box-shadow: 0 1px 3px rgba(0,0,0,0.15);
  transition: transform 0.15s, opacity 0.15s;
  z-index: 30;
  opacity: 0;
}
.item-container:hover .delete-item-btn {
  opacity: 1;
}
/* KHI CLICK VÀO SECTION (ACTIVE), HIỆN NÚT X LUÔN KHÔNG CẦN PHẢI DI CHUỘT VÀO NỘI DUNG */
.section-block.section-active .delete-item-btn {
  opacity: 1 !important;
}
.delete-item-btn:hover { transform: scale(1.15); background: #dc2626; }
.delete-item-btn--lg {
  width: 18px;
  height: 18px;
  right: -12px; /* Nằm cân đối chính giữa khoảng đệm 20px bên phải */
  top: 6px;    /* Đứng thẳng hàng với dòng đầu tiên của mục con */
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
  padding-left: 1.15rem !important;
  margin-top: 0.25rem;
  margin-bottom: 0.25rem;
}
:deep(.html-content ol) {
  list-style-type: decimal !important;
  padding-left: 1.15rem !important;
  margin-top: 0.25rem;
  margin-bottom: 0.25rem;
}
:deep(.html-content b),
:deep(.html-content strong) { font-weight: 700 !important; }
:deep(.html-content i),
:deep(.html-content em) { font-style: italic !important; }
:deep(.html-content u) { text-decoration: underline !important; }
:deep(.html-content ul li),
:deep(.html-content ol li) { margin-bottom: 0.2rem; }

/* Custom Bullet points for placeholders and experiences list */
.text-list-style :deep(ul) {
  list-style-type: none !important;
  padding-left: 0 !important;
}
.text-list-style :deep(ul li) {
  position: relative;
  padding-left: 1.2rem;
  margin-bottom: 0.4rem;
}
.text-list-style :deep(ul li::before) {
  content: "•";
  position: absolute;
  left: 0.2rem;
  top: 0;
  color: #2F2926;
  font-size: 1.15rem;
  line-height: 1;
}

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
