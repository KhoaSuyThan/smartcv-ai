<template>
  <div id="cv-printable-area" ref="cvRoot" class="flex flex-row relative box-border bg-white overflow-hidden" :style="{ width: '210mm', height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Segoe UI\', Arial, sans-serif' }">
    
    <aside class="z-10 flex flex-col shrink-0 relative box-border text-white" :style="{ width: '35%', backgroundColor: templatePrimaryColor, padding: '40px 25px', minHeight: `${Math.max(1, pageCount) * 297}mm` }">
      
      <div class="paginated-item relative z-20 w-full flex flex-col items-center mb-[40px]">
        <div class="relative rounded-full overflow-hidden mx-auto" :style="{ width: '160px', height: '160px', border: '6px solid rgba(255,255,255,0.1)' }">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center bg-white/10 text-white/50">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
          </div>
        </div>
      </div>

      <div class="paginated-item w-full mb-[15px]">
        <h3 class="uppercase block" :style="{ fontSize: '16px !important', fontWeight: 'bold !important', paddingBottom: '8px !important', margin: '0 0 15px 0 !important', letterSpacing: '1px' }">
          LIÊN HỆ VỚI TÔI
        </h3>
        <ul class="w-full list-none p-0 m-0 flex flex-col gap-[12px]" :style="{ fontSize: '13px !important', lineHeight: '1.4' }">
          
          <li class="flex items-start relative gap-[10px]">
            <div class="w-[15px] text-center shrink-0 mt-[3px]">
              <svg class="w-[12px] h-[12px] inline-block" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd"></path></svg>
            </div>
            <span class="break-words" :style="{ opacity: isEmpty(resumeData.general.address) ? '0.45' : '1' }" v-html="!isEmpty(resumeData.general.address) ? resumeData.general.address : 'TP. Hồ Chí Minh'"></span>
          </li>

          <li class="flex items-start relative gap-[10px]">
            <div class="w-[15px] text-center shrink-0 mt-[3px]">
              <svg class="w-[12px] h-[12px] inline-block" fill="currentColor" viewBox="0 0 20 20"><path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z"></path><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z"></path></svg>
            </div>
            <span class="break-words" :style="{ opacity: isEmpty(resumeData.general.email) ? '0.45' : '1' }" v-html="!isEmpty(resumeData.general.email) ? resumeData.general.email : 'email@example.com'"></span>
          </li>

          <li class="flex items-start relative gap-[10px]">
            <div class="w-[15px] text-center shrink-0 mt-[3px]">
              <svg class="w-[12px] h-[12px] inline-block" fill="currentColor" viewBox="0 0 20 20"><path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z"></path></svg>
            </div>
            <span class="break-words" :style="{ opacity: isEmpty(resumeData.general.phone) ? '0.45' : '1' }" v-html="!isEmpty(resumeData.general.phone) ? resumeData.general.phone : '0123.456.789'"></span>
          </li>

          <li v-if="!isEmpty(resumeData.general.dob)" class="flex items-start relative gap-[10px]">
            <div class="w-[15px] text-center shrink-0 mt-[3px]">
              <svg class="w-[12px] h-[12px] inline-block" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" clip-rule="evenodd"></path></svg>
            </div>
            <span class="break-words" v-html="resumeData.general.dob"></span>
          </li>
          
          <li v-if="!isEmpty(resumeData.general.github) || !isEmpty(resumeData.general.website) || !isEmpty(resumeData.general.linkedin)" class="flex items-start relative gap-[10px]">
            <div class="w-[15px] text-center shrink-0 mt-[3px]">
              <svg class="w-[12px] h-[12px] inline-block" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM4.332 8.027a6.012 6.012 0 011.912-2.706C6.512 5.73 6.974 6 7.5 6A1.5 1.5 0 019 7.5V8a2 2 0 004 0 2 2 0 011.523-1.943A5.977 5.977 0 0116 10c0 .34-.028.675-.083 1H15a2 2 0 00-2 2v2.197A5.973 5.973 0 0110 16v-2a2 2 0 00-2-2 2 2 0 01-2-2 2 2 0 00-1.668-1.973z" clip-rule="evenodd"></path></svg>
            </div>
            <span class="break-words" v-html="!isEmpty(resumeData.general.github) ? resumeData.general.github : (!isEmpty(resumeData.general.linkedin) ? resumeData.general.linkedin : resumeData.general.website)"></span>
          </li>
        </ul>
      </div>

      <div class="w-full flex-1 flex flex-col m-0 p-0">
        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-if="section.isVisible"
            class="section-block relative paginated-item group w-full mb-[15px]"
            :class="{ 'section-selected-sidebar': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { transform: 'scale(1.02)', transformOrigin: 'left center', zIndex: 10 } : {}"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Buttons: chỉ hiện khi CLICK (active), không hiện khi hover -->
            <div v-if="selectedSectionId === section.id" class="nav-btns no-print" @click.stop>
              <div class="nav-btns-row">
                <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7-7"/></svg></button>
              </div>
              <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn phần này"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg></button>
            </div>

            <h3 class="uppercase w-full block" :style="{ fontSize: '16px !important', fontWeight: 'bold !important', paddingBottom: '8px !important', margin: '30px 0 15px 0 !important', letterSpacing: '1px' }">
              {{ section.title }}
            </h3>
            
            <div class="w-full flex flex-col gap-[15px]">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full text-white" :style="{ fontSize: '13px !important', margin: '0 !important', padding: '0 !important' }">
                
                <div v-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'" class="flex flex-col">
                  <div class="w-full uppercase" :style="{ fontSize: '12px !important', fontWeight: 'bold !important', color: '#a5b8d4', marginBottom: '5px' }">{{ item.name }}</div>
                  <div v-if="item.info || item.level" class="w-full break-words whitespace-pre-line" :style="{ margin: '0 !important' }">{{ item.info || item.level }}</div>
                </div>

                <div v-else class="w-full flex flex-col">
                  <span class="font-bold w-full break-words leading-tight" v-html="section.id === 'education' ? item.school : (item.company || item.name)"></span>
                  <div class="italic opacity-90 mt-[2px] text-[12px]" v-if="item.major || item.role">{{ item.major || item.role }}</div>
                  <div class="font-bold opacity-90 text-[11px] mt-[2px]" v-if="item.year || item.time">{{ item.year || item.time }}</div>
                  <div v-if="item.desc" class="html-content-sidebar text-justify whitespace-pre-line break-words w-full mt-[5px]" :style="{ lineHeight: '1.6' }" v-html="formatDesc(item.desc)"></div>
                </div>

                <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>
      </div>
    </aside>

    <main class="flex-1 flex flex-col relative bg-[#ffffff] z-20 overflow-hidden box-border pt-[30px] pb-[50px]" @click.self="selectedSectionId = null">
      
      <header class="paginated-item w-full flex flex-col relative" :style="{ paddingBottom: '15px', marginBottom: '50px', marginLeft: '20px !important', marginRight: '20px !important', width: 'calc(100% - 40px)' }">
        <h1 class="uppercase break-words w-full m-0" :style="{ fontSize: '48px !important', fontWeight: '800 !important', color: templatePrimaryColor, lineHeight: '1.2', marginTop: '60px !important', paddingLeft: '20px !important' }" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'HỌ VÀ TÊN'"></h1>
        <div class="h-[2px] w-[60px] ml-[20px] my-[15px] bg-[#eee]"></div>
        <h2 class="uppercase break-words w-full m-0" :style="{ fontSize: '20px !important', letterSpacing: '3px', fontWeight: 'bold', color: templateAccentColor, marginTop: '0 !important', paddingLeft: '20px !important' }" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'VỊ TRÍ ỨNG TUYỂN'"></h2>
      </header>

      <div
        v-if="summarySection && summarySection.isVisible"
        class="section-block paginated-item relative group w-full mb-[20px]"
        :class="{ 'section-selected-main': selectedSectionId === summarySection.id }"
        :style="[
          { marginLeft: '20px !important', marginRight: '20px !important', width: 'calc(100% - 40px)' },
          selectedSectionId === summarySection.id ? { transform: 'scale(1.015)', transformOrigin: 'left center', zIndex: 10 } : {}
        ]"
        @click.stop="toggleSection(summarySection.id)"
      >
        <!-- Nav Buttons: chỉ hiện khi CLICK -->
        <div v-if="selectedSectionId === summarySection.id" class="nav-btns no-print" @click.stop>
          <div class="nav-btns-row">
            <button @click.stop.prevent="$emit('moveUp', summarySection.id, mainIds)" class="nav-btn" title="Di chuyển lên"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
            <button @click.stop.prevent="$emit('moveDown', summarySection.id, mainIds)" class="nav-btn" title="Di chuyển xuống"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
            <button @click.stop.prevent="$emit('moveHorizontal', summarySection.id, 'left')" class="nav-btn" title="Sang Trái"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
          </div>
          <button @click.stop.prevent="summarySection.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn phần này"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg></button>
        </div>

        <h3 class="uppercase w-full block" :style="{ fontSize: '16px !important', color: templateAccentColor, borderBottom: '2px solid #c8d8e8', paddingBottom: '8px !important', margin: '0 0 15px 0 !important', paddingLeft: '20px !important', fontWeight: 'bold !important', letterSpacing: '2px' }">
          {{ summarySection.title || 'MỤC TIÊU NGHỀ NGHIỆP' }}
        </h3>
        
        <div class="w-full" :style="{ borderLeft: '2px solid #c8d8e8', paddingLeft: '15px !important', marginLeft: '20px !important', width: 'calc(100% - 20px)' }">
          <div class="html-content text-justify whitespace-pre-line w-full text-[#444]" :style="{ fontSize: '14px !important', lineHeight: '1.8 !important', margin: '0 !important' }" v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Mục tiêu nghề nghiệp...'"></div>
        </div>
      </div>

      <div class="w-full flex flex-col flex-1 m-0 p-0">
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-if="section.isVisible"
            class="section-block relative paginated-item group w-full mb-[20px]"
            :class="{ 'section-selected-main': selectedSectionId === section.id }"
            :style="[
              { marginLeft: '20px !important', marginRight: '20px !important', width: 'calc(100% - 40px)' },
              selectedSectionId === section.id ? { transform: 'scale(1.015)', transformOrigin: 'left center', zIndex: 10 } : {}
            ]"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Buttons: chỉ hiện khi CLICK -->
            <div v-if="selectedSectionId === section.id" class="nav-btns no-print" @click.stop>
              <div class="nav-btns-row">
                <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
              </div>
              <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn phần này"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg></button>
            </div>

            <h3 class="uppercase w-full block" :style="{ fontSize: '16px !important', color: templateAccentColor, borderBottom: '2px solid #c8d8e8', paddingBottom: '8px !important', margin: '20px 0 15px 0 !important', paddingLeft: '20px !important', fontWeight: 'bold !important', letterSpacing: '2px' }">
              {{ section.title }}
            </h3>

            <div class="w-full flex flex-col gap-[20px]" :style="{ borderLeft: '2px solid #c8d8e8', paddingLeft: '15px !important', marginLeft: '20px !important', width: 'calc(100% - 20px)' }">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full text-[#444]" :style="{ margin: '0 !important', padding: '0 !important' }">
                
                <div v-if="section.id === 'education' || section.id === 'experience' || section.id === 'project' || section.id === 'activities'" class="w-full">
                  <div class="w-full flex flex-col">
                    <div class="w-full flex justify-between items-baseline gap-2" :style="{ margin: '0 !important' }">
                      <span class="font-bold break-words flex-1 leading-tight text-[#333]" :style="{ margin: '0 !important', fontSize: '15px' }" v-html="section.id === 'education' ? item.school : (item.company || item.name)"></span>
                      <span class="font-bold shrink-0 whitespace-nowrap text-right text-[#666]" v-if="item.year || item.time" :style="{ fontSize: '13px', margin: '0 !important' }">{{ item.year || item.time }}</span>
                    </div>
                    <div class="italic text-[#555] mt-[2px] mb-[5px] w-full" :style="{ fontSize: '14px !important' }" v-if="item.major || item.role">{{ item.major || item.role }}</div>
                  </div>
                  <div v-if="item.desc" class="html-content text-justify whitespace-pre-line break-words w-full" :style="{ margin: '0 !important', padding: '0 !important', fontSize: '14px', lineHeight: '1.8' }" v-html="formatDesc(item.desc)"></div>
                  <div v-else-if="item.gradType" class="font-medium mt-[2px]" :style="{ color: templateAccentColor, fontSize: '13px !important', margin: '0 !important' }">Trạng thái: {{ item.gradType }}</div>
                </div>

                <div v-else class="w-full">
                  <div v-if="item.year || item.time" class="font-bold mb-[5px] text-[#333]" :style="{ fontSize: '14px', margin: '0 !important' }">{{ item.year || item.time }}</div>
                  <div class="html-content text-justify whitespace-pre-line break-words w-full" :style="{ margin: '0 !important', padding: '0 !important', fontSize: '14px', lineHeight: '1.8' }" v-html="formatDesc(item.desc || item.name || item.info)"></div>
                </div>

                <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 w-[18px] h-[18px]">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>
      </div>
    </main>
    
    <!-- Page break indicators -->
    <template v-for="p in (pageCount - 1)" :key="'div-'+p">
      <div class="absolute left-0 w-full z-50 flex flex-col items-center justify-center pointer-events-none no-print" 
           :style="{ top: `calc(${p * 297}mm - 8px)` }">
        <div class="w-[105%] h-[16px] bg-slate-800/95 shadow-inner overflow-hidden border-y border-black/30 backdrop-blur-sm"></div>
        <span class="absolute text-[9px] uppercase font-bold text-slate-300 tracking-widest bg-slate-700 px-3 py-0.5 rounded border border-slate-600 shadow-md">Ngắt trang {{ p + 1 }}</span>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, ref, onMounted, nextTick, watch, onUnmounted } from 'vue'

// ─── Props & Emits ───────────────────────────────────────────────────────────
const props = defineProps({
  resumeData: { type: Object, required: true }
})
const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

// ─── Refs ────────────────────────────────────────────────────────────────────
const cvRoot = ref(null)
const pageCount = ref(1)
const selectedSectionId = ref(null)

// ─── Colors ──────────────────────────────────────────────────────────────────
const templatePrimaryColor = computed(() => {
  if (!props.resumeData.theme.primaryColor || props.resumeData.theme.primaryColor.toLowerCase() === '#0d6efd') {
    return '#32507d'
  }
  return props.resumeData.theme.primaryColor
})

const templateAccentColor = computed(() => '#5fb4c4')

// ─── Section Helpers ─────────────────────────────────────────────────────────
const isEmpty = (val) => {
  if (!val) return true
  if (typeof val !== 'string') return false
  return val.replace(/<[^>]*>/g, '').trim() === ''
}

const sectionHasData = (_section) => {
  // Luôn hiển thị section nếu isVisible = true (kể cả khi items rỗng)
  return true
}

const toggleSection = (id) => {
  selectedSectionId.value = selectedSectionId.value === id ? null : id
}

const summarySection = computed(() => props.resumeData.sections.find(s => s.id === 'summary'))
const sidebarSections = computed(() => props.resumeData.sections.filter(s => s.column === 'left' && s.id !== 'summary'))
const mainSections = computed(() => props.resumeData.sections.filter(s => s.column === 'right' && s.id !== 'summary'))
const sidebarIds = computed(() => sidebarSections.value.map(s => s.id))
const mainIds = computed(() => mainSections.value.map(s => s.id))

// ─── Format ───────────────────────────────────────────────────────────────────
const formatDesc = (text) => {
  if (!text) return ''
  if (/<[a-z][\s\S]*>/i.test(text)) {
    if (text.includes('<li') && (text.includes('<font') || text.includes('style='))) {
      try {
        const tempDiv = document.createElement('div')
        tempDiv.innerHTML = text
        tempDiv.querySelectorAll('li').forEach(li => {
          const child = li.firstElementChild
          if (child && (child.tagName === 'FONT' || child.tagName === 'SPAN')) {
            if (child.color) li.style.color = child.color
            if (child.style?.color) li.style.color = child.style.color
          }
        })
        return tempDiv.innerHTML
      } catch (e) { return text }
    }
    return text
  }
  return text.split('\n').map(l => l.trim()).filter(l => l).join('<br/>')
}

// ─── Pagination ───────────────────────────────────────────────────────────────
const A4_WIDTH_MM = 210
const A4_HEIGHT_MM = 297
const PAGE_MARGIN_TOP_MM = 20
const PAGE_MARGIN_BOTTOM_MM = 25  // khoảng trắng cố định cuối trang

let paginateTimer = null
const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 80)
}

const doPagination = async () => {
  if (!cvRoot.value) return

  // Bước 1: Reset toàn bộ margin đã inject trước đó
  cvRoot.value.querySelectorAll('.paginated-item').forEach(el => {
    el.style.marginTop = ''
  })
  await nextTick()

  const rootWidthPx = cvRoot.value.offsetWidth
  const pxPerMm = rootWidthPx / A4_WIDTH_MM
  const pageHeightPx = A4_HEIGHT_MM * pxPerMm
  const marginTopPx = PAGE_MARGIN_TOP_MM * pxPerMm
  const marginBottomPx = PAGE_MARGIN_BOTTOM_MM * pxPerMm

  // Giới hạn an toàn: nội dung không được vượt qua điểm này
  // safeBottom = pageHeight - marginBottom (tính từ đầu trang hiện tại)
  const getSafeBottom = (pageIndex) => (pageIndex + 1) * pageHeightPx - marginBottomPx

  // Lấy vị trí top của element tương đối với cvRoot
  const getTopRelative = (el) => {
    let offset = 0
    let cur = el
    while (cur && cur !== cvRoot.value) {
      offset += cur.offsetTop
      cur = cur.offsetParent
    }
    return offset
  }

  // Xử lý một cột: đẩy TOÀN BỘ block xuống trang mới nếu vượt safe zone
  const processColumn = async (colEl) => {
    if (!colEl) return

    // Lặp nhiều lần đến khi ổn định (vì đẩy 1 item có thể làm item sau bị lệch)
    for (let pass = 0; pass < 20; pass++) {
      let changed = false
      const items = colEl.querySelectorAll('.paginated-item')

      for (let i = 0; i < items.length; i++) {
        const item = items[i]
        const top = getTopRelative(item)
        const height = item.offsetHeight
        const bottom = top + height

        const pageIndex = Math.floor(top / pageHeightPx)
        const safeBottom = getSafeBottom(pageIndex)
        const nextPageTop = (pageIndex + 1) * pageHeightPx + marginTopPx

        // Nếu bottom của block vượt qua safe zone → đẩy TOÀN BỘ block xuống trang mới
        if (bottom > safeBottom) {
          const push = nextPageTop - top
          if (push > 0) {
            const currentMt = parseFloat(item.style.marginTop || '0')
            item.style.marginTop = (currentMt + push) + 'px'
            changed = true
            await nextTick()
            break // restart pass vì layout đã thay đổi
          }
        }
      }

      if (!changed) break // ổn định rồi, thoát
    }
  }

  const mainEl = cvRoot.value.querySelector('main')
  const asideEl = cvRoot.value.querySelector('aside')

  await processColumn(mainEl)
  await processColumn(asideEl)

  // Bước 3: Tính số trang thực tế — không tạo trang trống
  let maxBottom = 0
  cvRoot.value.querySelectorAll('.paginated-item').forEach(el => {
    const top = getTopRelative(el)
    const bottom = top + el.offsetHeight
    if (bottom > maxBottom) maxBottom = bottom
  })

  const needed = Math.max(1, Math.ceil((maxBottom + marginBottomPx) / pageHeightPx))
  if (pageCount.value !== needed) {
    pageCount.value = needed
  }
}

// ─── Lifecycle ────────────────────────────────────────────────────────────────
watch(() => props.resumeData, requestPagination, { deep: true })

onMounted(() => {
  // Hiển thị các section mặc định theo ảnh mẫu (kể cả khi chưa có data)
  // Sidebar: skills (gồm it_skills, languages, other_skills), awards
  // Main: summary, experience, education
  const defaultVisible = [
    'summary', 'experience', 'education',            // cột phải (main)
    'skills', 'it_skills', 'languages', 'awards'     // cột trái (sidebar)
  ]
  if (props.resumeData?.sections) {
    props.resumeData.sections.forEach(sec => {
      sec.isVisible = defaultVisible.includes(sec.id)
    })
  }

  requestPagination()
  window.addEventListener('resize', requestPagination)
  document.addEventListener('keyup', requestPagination)
})

onUnmounted(() => {
  window.removeEventListener('resize', requestPagination)
  document.removeEventListener('keyup', requestPagination)
  if (paginateTimer) clearTimeout(paginateTimer)
})
</script>

<style scoped>
/* ─── Base ──────────────────────────────────────────────────────────────────── */
#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

/* ─── Item containers ────────────────────────────────────────────────────────── */
.item-container {
  position: relative;
  transition: all 0.2s;
}

.delete-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  position: absolute;
  right: 20px;
  top: 0;
}

/* ─── Section blocks ─────────────────────────────────────────────────────────── */
.section-block {
  position: relative;
  border: 2px solid transparent;
  cursor: pointer;
  border-radius: 6px !important;
  /* Không có hiệu ứng khi hover — chỉ pointer cursor */
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

/* Không hover effect — chỉ cursor */
.section-block:hover {
  background: transparent !important;
}

/* ─── ACTIVE STATE: Main (cột phải - nền trắng) ──────────────────────────────── */
.section-selected-main {
  /* Border màu trùng nền section (trắng → dùng accent để visible nhưng tinh tế) */
  border: 2px solid v-bind('templateAccentColor') !important;
  border-radius: 6px !important;
  /* Zoom nhẹ để nổi bật */
  transform: scale(1.015) !important;
  transform-origin: left center !important;
  box-shadow: 0 4px 16px rgba(95, 180, 196, 0.2) !important;
  background: rgba(95, 180, 196, 0.03) !important;
  z-index: 10 !important;
}

/* ─── ACTIVE STATE: Sidebar (cột trái - nền navy) ────────────────────────────── */
.section-selected-sidebar {
  /* Border màu trùng nền sidebar (rgba white để "chìm" nhưng vẫn định hình) */
  border: 2px solid rgba(255, 255, 255, 0.35) !important;
  border-radius: 6px !important;
  transform: scale(1.02) !important;
  transform-origin: left center !important;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.25) !important;
  background: rgba(255, 255, 255, 0.05) !important;
  z-index: 10 !important;
}

/* ─── Nav buttons ────────────────────────────────────────────────────────────── */
.nav-btns {
  position: absolute;
  right: 10px;
  top: 10px;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 8px;
  z-index: 9999;
}

.nav-btns-row {
  display: flex;
  flex-direction: row;
  gap: 8px;
}

.nav-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 4px;
  background: #2563eb !important;
  color: white !important;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  box-shadow: 0 2px 6px rgba(37, 99, 235, 0.4);
  transition: all 0.15s ease;
}

.nav-btn:hover {
  background: #1d4ed8 !important;
  transform: scale(1.1);
}

.nav-btn:active {
  transform: scale(0.95);
}

.nav-btn-danger {
  background: #ef4444 !important;
}

.nav-btn-danger:hover {
  background: #dc2626 !important;
}

/* ─── HTML Content (main column) ─────────────────────────────────────────────── */
:deep(.html-content) { margin: 0 !important; padding: 0 !important; }
:deep(.html-content p) { margin: 0 !important; padding: 0 !important; }
:deep(.html-content ul) { list-style-type: disc !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content ol) { list-style-type: decimal !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content b), :deep(.html-content strong) { font-weight: bold; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic; }
:deep(.html-content u) { text-decoration: underline; }
:deep(.html-content ul li), :deep(.html-content ol li) { margin-bottom: 2px !important; }

/* ─── HTML Content (sidebar column) ─────────────────────────────────────────── */
:deep(.html-content-sidebar) { margin: 0 !important; padding: 0 !important; color: white !important; }
:deep(.html-content-sidebar p) { margin: 0 !important; padding: 0 !important; }
:deep(.html-content-sidebar ul) { list-style-type: disc !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content-sidebar ol) { list-style-type: decimal !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content-sidebar b), :deep(.html-content-sidebar strong) { font-weight: bold; }
:deep(.html-content-sidebar i), :deep(.html-content-sidebar em) { font-style: italic; opacity: 0.9; }
:deep(.html-content-sidebar u) { text-decoration: underline; }
:deep(.html-content-sidebar ul li), :deep(.html-content-sidebar ol li) { margin-bottom: 2px !important; }

/* ─── Font sizes from editor ─────────────────────────────────────────────────── */
:deep(.html-content li:has(> font[size="1"])), :deep(.html-content-sidebar li:has(> font[size="1"])) { font-size: 10px; }
:deep(.html-content li:has(> font[size="2"])), :deep(.html-content-sidebar li:has(> font[size="2"])) { font-size: 11px; }
:deep(.html-content li:has(> font[size="3"])), :deep(.html-content-sidebar li:has(> font[size="3"])) { font-size: 13px; }
:deep(.html-content li:has(> font[size="4"])), :deep(.html-content-sidebar li:has(> font[size="4"])) { font-size: 15px; }

/* ─── Print ───────────────────────────────────────────────────────────────────── */
@media print {
  .no-print { display: none !important; }
  .section-block,
  .section-selected-main,
  .section-selected-sidebar {
    cursor: default !important;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
    transform: none !important;
    outline: none !important;
  }
}

:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .section-selected-main),
:global(.is-exporting-pdf .section-selected-sidebar) {
  cursor: default !important;
  box-shadow: none !important;
  background: transparent !important;
  border-color: transparent !important;
  transform: none !important;
  outline: none !important;
}
</style>