<template>
  <div id="cv-printable-area" ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-col relative box-border text-[#333] leading-relaxed overflow-hidden"
    :style="{ minHeight: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', \'Segoe UI\', sans-serif' }"
    @click.self="selectedSectionId = null">

    <!-- HEADER: Name & Job Title -->
    <header class="pt-[15mm] px-[12mm] pb-[45mm] flex justify-between items-start paginated-item relative z-20">
      <div class="flex-1">
        <h1 class="text-[34px] font-black uppercase text-[#5ba4b5] tracking-tight leading-none mb-2" 
          v-html="resumeData.general.fullName || 'HỌ VÀ TÊN'"></h1>
        <h2 class="text-[18px] font-bold text-slate-400 uppercase tracking-widest" 
          v-html="resumeData.general.jobTitle || 'VỊ TRÍ ỨNG TUYỂN'"></h2>
      </div>
      
      <!-- Decoration and Avatar -->
      <div class="relative flex-shrink-0 mr-4 z-30">
        <div class="absolute -top-10 right-0 w-[25mm] h-[25mm] bg-[#5ba4b5] opacity-80 z-0"></div>
        <div class="w-[48mm] h-[58mm] rounded-lg border-[6px] border-white shadow-lg overflow-hidden bg-slate-100 relative z-10">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center text-slate-300">
            <svg class="w-20 h-20" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path>
            </svg>
          </div>
        </div>
      </div>
    </header>

    <!-- SUMMARY BLOCK (TEAL) WRAPPER -->
    <div class="relative z-10" style="margin-top: -65mm;">
      <!-- SUMMARY BLOCK (TEAL) -->
      <div class="mx-0 bg-[#5ba4b5] text-white py-[8mm] px-[12mm] relative" 
           @click.stop="selectedSectionId = 'summary'">
        <h3 class="text-[15px] font-bold uppercase mb-3 tracking-wider paginated-item">Mục tiêu nghề nghiệp</h3>
        <div class="text-[15px] leading-relaxed text-justify font-medium mb-8 opacity-95 html-content"
          v-html="formatDesc(resumeData.general.summary || 'Mô tả mục tiêu nghề nghiệp...')">
        </div>

        <!-- Contact Info Grid (3 Columns) -->
        <div class="grid grid-cols-3 gap-y-4 text-[13px] font-medium paginated-item">
          <div class="flex items-center gap-3" v-if="!isEmpty(resumeData.general.gender)">
            <svg class="w-5 h-5 opacity-80" fill="currentColor" viewBox="0 0 24 24"><path d="M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4M12,6A6,6 0 0,0 6,12A6,6 0 0,0 12,18A6,6 0 0,0 18,12A6,6 0 0,0 12,6M12,8A4,4 0 0,1 16,12A4,4 0 0,1 12,16A4,4 0 0,1 8,12A4,4 0 0,1 12,8Z" /></svg>
            <span v-html="resumeData.general.gender"></span>
          </div>
          <div class="flex items-center gap-3" v-if="!isEmpty(resumeData.general.phone)">
            <svg class="w-5 h-5 opacity-80" fill="currentColor" viewBox="0 0 24 24"><path d="M6.62,10.79C8.06,13.62 10.38,15.94 13.21,17.38L15.41,15.18C15.69,14.9 16.08,14.82 16.43,14.93C17.55,15.3 18.75,15.5 20,15.5A1,1 0 0,1 21,16.5V20A1,1 0 0,1 20,21A17,17 0 0,1 3,4A1,1 0 0,1 4,3H7.5A1,1 0 0,1 8.5,4C8.5,5.25 8.7,6.45 9.07,7.57C9.18,7.92 9.1,8.31 8.82,8.59L6.62,10.79Z" /></svg>
            <span v-html="resumeData.general.phone"></span>
          </div>
          <div class="flex items-center gap-3" v-if="!isEmpty(resumeData.general.website)">
            <svg class="w-5 h-5 opacity-80" fill="currentColor" viewBox="0 0 24 24"><path d="M12 2C6.47 2 2 6.47 2 12s4.47 10 10 10 10-4.47 10-10S17.53 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8zM12 7c-2.76 0-5 2.24-5 5s2.24 5 5 5 5-2.24 5-5-2.24-5-5-5zm0 8c-1.66 0-3-1.34-3-3s1.34-3 3-3 3 1.34 3 3-1.34 3-3 3z" /></svg>
            <span v-html="resumeData.general.website"></span>
          </div>
          <div class="flex items-center gap-3" v-if="!isEmpty(resumeData.general.birthDate)">
            <svg class="w-5 h-5 opacity-80" fill="currentColor" viewBox="0 0 24 24"><path d="M19,4H18V2H16V4H8V2H6V4H5A2,2 0 0,0 3,6V20A2,2 0 0,0 5,22H19A2,2 0 0,0 21,20V6A2,2 0 0,0 19,4M19,20H5V10H19V20M19,8H5V6H19V8M7,12H12V17H7V12Z" /></svg>
            <span v-html="resumeData.general.birthDate"></span>
          </div>
          <div class="flex items-center gap-3" v-if="!isEmpty(resumeData.general.email)">
            <svg class="w-5 h-5 opacity-80" fill="currentColor" viewBox="0 0 24 24"><path d="M20,4H4C2.89,4 2,4.89 2,6V18A2,2 0 0,0 4,20H20A2,2 0 0,0 22,18V6C22,4.89 21.1,4 20,4M20,8L12,13L4,8V6L12,11L20,6V8Z" /></svg>
            <span class="break-all" v-html="resumeData.general.email"></span>
          </div>
          <div class="flex items-center gap-3" v-if="!isEmpty(resumeData.general.address)">
            <svg class="w-5 h-5 opacity-80" fill="currentColor" viewBox="0 0 24 24"><path d="M12,11.5A2.5,2.5 0 0,1 9.5,9A2.5,2.5 0 0,1 12,6.5A2.5,2.5 0 0,1 14.5,9A2.5,2.5 0 0,1 12,11.5M12,2C8.13,2 5,5.13 5,9C5,14.25 12,22 12,22C12,22 19,14.25 19,9C19,5.13 15.87,2 12,2Z" /></svg>
            <span v-html="resumeData.general.address"></span>
          </div>
        </div>
      </div>
    </div>

    <!-- BODY: 2 COLUMNS -->
    <div class="flex px-[12mm] py-[8mm] gap-[10mm] relative z-10" @click.self="selectedSectionId = null">
      
      <!-- LEFT COLUMN: Education & Experience -->
      <div class="flex-[1.5] flex flex-col gap-8">
        <template v-for="section in leftSections" :key="section.id">
          <div v-show="section.isVisible" class="section-block relative"
            :class="{ 'section-active': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id">
            
            <div v-show="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="moveSectionUp(section.id, 'left')" class="nav-btn">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionDown(section.id, 'left')" class="nav-btn">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionHorizontal(section.id, 'right')" class="nav-btn">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M9 5l7 7-7 7"/></svg>
              </button>
            </div>

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

            <!-- Education -->
            <div v-if="section.id === 'education'" class="space-y-6">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                <button v-show="selectedSectionId === section.id" @click.stop.prevent="handleRemoveItem(section.id, itemIndex)" class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                <div class="paginated-item">
                  <div class="flex justify-between items-start mb-1">
                    <h4 class="text-[10px] font-bold text-slate-800 flex items-center gap-2">
                      <span class="text-[10px]">▶</span> {{ item.school }}
                    </h4>
                    <span class="text-[14px] font-bold text-slate-800 shrink-0">{{ item.year }}</span>
                  </div>
                  <div class="pl-5">
                    <div class="text-[14.5px] font-bold italic text-slate-600">{{ item.major }}</div>
                    <div v-if="item.gradType" class="text-[14px] text-slate-500 font-medium">Học lực: {{ item.gradType }}</div>
                  </div>
                </div>
                <div v-if="item.desc" class="text-[10px] leading-relaxed text-slate-600 text-justify html-content mt-2 pl-5" v-html="formatDesc(item.desc)"></div>
              </div>
            </div>

            <!-- Experience / Project / Activities -->
            <div v-else-if="['experience', 'project', 'activities'].includes(section.id)" class="space-y-8">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                <button v-show="selectedSectionId === section.id" @click.stop.prevent="handleRemoveItem(section.id, itemIndex)" class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                <div class="paginated-item">
                  <div class="flex justify-between items-start mb-1">
                    <h4 class="text-[10px] font-bold text-slate-800 flex items-center gap-2 uppercase">
                      <span class="text-[10px]">▶</span> {{ section.id === 'experience' ? item.company : (item.name || item.title) }}
                    </h4>
                    <span v-if="item.time || item.year" class="text-[14px] font-bold text-slate-800 shrink-0">{{ item.time || item.year }}</span>
                    <span v-else-if="item.level || item.info" class="text-[14px] font-bold italic text-slate-500 shrink-0">{{ item.level || item.info }}</span>
                  </div>
                  <div v-if="section.id === 'experience' || item.role || item.company" class="pl-5 text-[14.5px] font-bold italic text-slate-600 mb-2">
                    {{ section.id === 'experience' ? item.role : (item.role || item.company) }}
                  </div>
                </div>
                <div class="text-[15px] leading-relaxed text-slate-600 text-justify html-content pl-5" v-html="formatDesc(item.desc || item.info || item.content)"></div>
              </div>
            </div>

            <!-- Other Generic Sections (Skills, Languages, etc.) -->
            <div v-else class="space-y-4">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                <button v-show="selectedSectionId === section.id" @click.stop.prevent="handleRemoveItem(section.id, itemIndex)" class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                <div class="paginated-item">
                  <div class="flex items-start gap-2">
                    <span class="text-[10px] mt-[4px] shrink-0">▶</span>
                    <div class="flex-1 min-w-0">
                      <!-- Main Line: prioritized name/title, fallback to desc/info if nothing else -->
                      <div class="flex justify-between items-start gap-2">
                        <div style="font-size: 15px !important;" class="font-bold text-slate-800 break-words flex-1">
                          <span v-html="item.name || item.title || (!item.desc ? item.info : '') || (!item.name && !item.title && !item.info ? item.desc : '') || 'Chưa có nội dung'"></span>
                        </div>
                        <span v-if="item.level || (item.info && (item.name || item.title))" class="text-[14px] font-bold italic text-slate-500 shrink-0">
                          {{ item.level || item.info }}
                        </span>
                        <span v-else-if="item.year || item.time" class="text-[14px] font-bold text-slate-800 shrink-0">{{ item.year || item.time }}</span>
                      </div>
                      <!-- Sub content: only if different from the main line and exists -->
                      <div v-if="item.desc && item.desc !== item.name && item.desc !== item.title" 
                           style="font-size: 15px !important;"
                           class="leading-relaxed text-slate-600 text-justify html-content mt-1" 
                           v-html="formatDesc(item.desc)">
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </template>
      </div>

      <!-- RIGHT COLUMN: Awards & Certs -->
      <div class="flex-1 flex flex-col gap-8">
        <template v-for="section in rightSections" :key="section.id">
          <div v-show="section.isVisible" class="section-block relative"
            :class="{ 'section-active': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id">
            
            <div v-show="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="moveSectionUp(section.id, 'right')" class="nav-btn">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionDown(section.id, 'right')" class="nav-btn">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionHorizontal(section.id, 'left')" class="nav-btn">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M15 19l-7-7 7-7"/></svg>
              </button>
            </div>

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

            <div class="space-y-6">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                <button v-show="selectedSectionId === section.id" @click.stop.prevent="handleRemoveItem(section.id, itemIndex)" class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                
                <div class="paginated-item">
                  <div class="flex items-start gap-2">
                    <span class="text-[10px] mt-[4px] shrink-0">▶</span>
                    <div class="flex-1 min-w-0">
                      <!-- Title/Name Line -->
                      <div class="flex justify-between items-start gap-2 mb-0.5">
                        <h4 style="font-size: 15px !important;" class="font-bold text-slate-800 flex-1 break-words">
                          {{ section.id === 'experience' ? item.company : 
                             section.id === 'education' ? item.school : 
                             (item.name || item.title || (!item.desc ? item.info : '')) }}
                        </h4>
                        <span v-if="item.year || item.time" style="font-size: 14px !important;" class="font-bold text-slate-500 shrink-0">{{ item.year || item.time }}</span>
                        <span v-else-if="item.level" style="font-size: 14px !important;" class="font-bold italic text-slate-500 shrink-0">{{ item.level }}</span>
                        <span v-else-if="item.info && (item.name || item.title)" style="font-size: 14px !important;" class="font-bold italic text-slate-500 shrink-0">{{ item.info }}</span>
                      </div>

                      <!-- Subtitle Line (Role/Major) -->
                      <div v-if="section.id === 'experience' || section.id === 'education' || item.role || item.position || item.major" 
                           class="text-[14.5px] font-bold italic text-slate-600 mb-0.5">
                        {{ section.id === 'experience' ? item.role : 
                           section.id === 'education' ? item.major : 
                           (item.role || item.position || item.major) }}
                      </div>

                      <!-- Education Grade -->
                      <div v-if="section.id === 'education' && item.gradType" 
                           class="text-[14px] text-slate-500 font-medium mb-1">
                        Học lực: {{ item.gradType }}
                      </div>

                      <!-- Description -->
                      <div v-if="(item.desc || item.info || item.content) && (item.desc !== item.name && item.desc !== item.title)"
                           style="font-size: 15px !important;"
                           class="leading-relaxed text-slate-600 text-justify html-content" 
                           v-html="formatDesc(item.desc || item.info || item.content)"></div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </template>
      </div>
    </div>

    <!-- PAGINATION INDICATORS -->
    <template v-for="p in (pageCount - 1)" :key="'div-'+p">
      <div class="absolute left-0 w-full z-50 flex flex-col items-center justify-center pointer-events-none no-print" 
        :style="{ top: `calc(${p * 297}mm - 12px)` }">
        <div class="w-[105%] h-[24px] bg-slate-900/90 shadow-xl border-y border-white/10 flex items-center justify-center">
          <div class="w-full h-[1px] bg-gradient-to-r from-transparent via-[#5ba4b5] to-transparent"></div>
        </div>
        <span class="absolute text-[9px] uppercase font-black text-white tracking-[0.3em] bg-[#5ba4b5] px-4 py-1 rounded-full border border-white/20 shadow-2xl">Ngắt trang {{ p + 1 }}</span>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, ref, onMounted, nextTick, watch, onUnmounted } from 'vue'

const props = defineProps({
  resumeData: { type: Object, required: true }
})
const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

const cvRoot = ref(null)
const pageCount = ref(1)
const selectedSectionId = ref(null)

const isEmpty = (val) => {
  if (!val) return true
  if (typeof val !== 'string') return false
  return val.replace(/<[^>]*>/g, '').trim() === ''
}

const leftSections = computed(() => props.resumeData.sections.filter(s => s.column === 'left' && s.id !== 'summary'))
const rightSections = computed(() => props.resumeData.sections.filter(s => s.column === 'right' && s.id !== 'summary'))

const formatDesc = (text) => {
  if (!text) return ''
  if (!/<[a-z][\s\S]*>/i.test(text)) {
    return text.split('\n').map(l => l.trim()).filter(Boolean)
      .map(l => `<div class="paginated-item">${l}</div>`).join('')
  }
  const tempDiv = document.createElement('div')
  tempDiv.innerHTML = text
  const blockTags = ['P', 'DIV', 'LI', 'H1', 'H2', 'H3', 'H4', 'H5', 'H6']
  tempDiv.querySelectorAll('*').forEach(el => {
    if (blockTags.includes(el.tagName.toUpperCase())) {
      const hasBlockChild = Array.from(el.children).some(child => blockTags.includes(child.tagName.toUpperCase()))
      if (!hasBlockChild) el.classList.add('paginated-item')
    }
  })
  Array.from(tempDiv.childNodes).forEach(node => {
    if (node.nodeType === Node.TEXT_NODE && node.textContent.trim()) {
      const wrapper = document.createElement('div')
      wrapper.className = 'paginated-item'
      node.replaceWith(wrapper)
      wrapper.appendChild(node)
    }
  })
  return tempDiv.innerHTML
}

// --- SECTION MOVEMENT FUNCTIONS ---
const moveSectionUp = (id, currentColumn) => {
  if (!props.resumeData?.sections) return
  const sections = props.resumeData.sections
  const colSections = sections.filter(s => s.column === currentColumn)
  const idx = colSections.findIndex(s => s.id === id)

  if (idx > 0) {
    const prevId = colSections[idx - 1].id
    const realIdxCur = sections.findIndex(s => s.id === id)
    const realIdxPrev = sections.findIndex(s => s.id === prevId)
    if (realIdxCur !== -1 && realIdxPrev !== -1) {
      const temp = sections.splice(realIdxCur, 1)[0]
      sections.splice(realIdxPrev, 0, temp)
      emit('moveUp', id, currentColumn)
    }
  }
}

const moveSectionDown = (id, currentColumn) => {
  if (!props.resumeData?.sections) return
  const sections = props.resumeData.sections
  const colSections = sections.filter(s => s.column === currentColumn)
  const idx = colSections.findIndex(s => s.id === id)

  if (idx !== -1 && idx < colSections.length - 1) {
    const nextId = colSections[idx + 1].id
    const realIdxCur = sections.findIndex(s => s.id === id)
    const realIdxNext = sections.findIndex(s => s.id === nextId)
    if (realIdxCur !== -1 && realIdxNext !== -1) {
      const temp = sections.splice(realIdxCur, 1)[0]
      sections.splice(realIdxNext, 0, temp)
      emit('moveDown', id, currentColumn)
    }
  }
}

const moveSectionHorizontal = (id, targetColumn) => {
  emit('moveHorizontal', id, targetColumn)
  if (!props.resumeData?.sections) return
  const sec = props.resumeData.sections.find(s => s.id === id)
  if (sec) {
    sec.column = targetColumn
  }
}

const handleRemoveItem = (sectionId, itemIndex) => {
  emit('removeItem', sectionId, itemIndex)
  if (!props.resumeData?.sections) return
  const section = props.resumeData.sections.find(s => s.id === sectionId)
  if (section && section.items && section.items.length > itemIndex) {
    section.items.splice(itemIndex, 1)
  }
}

let paginateTimer = null
const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 60)
}

const doPagination = async () => {
  if (!cvRoot.value) return
  const allElements = Array.from(cvRoot.value.querySelectorAll('.paginated-item')).filter(el => {
    if (el.offsetHeight === 0) return false
    let parent = el.parentElement
    while (parent && parent !== cvRoot.value) {
      if (parent.classList.contains('paginated-item')) return false
      parent = parent.parentElement
    }
    return true
  })
  allElements.forEach(el => { el.style.marginTop = '0px' })
  await nextTick()

  const offsetW = cvRoot.value.offsetWidth
  const cvRect = cvRoot.value.getBoundingClientRect()
  const scale = offsetW ? cvRect.width / offsetW : 1
  const A4_H_MM = 297
  const pxPerMm = offsetW / 210
  const pageH = A4_H_MM * pxPerMm
  const bottomSafeZone = 15 * pxPerMm
  const topMargin = 15 * pxPerMm

  let stable = false
  let passes = 0
  while (!stable && passes < 40) {
    stable = true; passes++
    const currentCvRect = cvRoot.value.getBoundingClientRect()
    for (let i = 0; i < allElements.length; i++) {
      const el = allElements[i]
      const elRect = el.getBoundingClientRect()
      const top = (elRect.top - currentCvRect.top) / scale
      const height = elRect.height / scale
      const pageIndex = Math.floor(top / pageH)
      const topInPage = top - (pageIndex * pageH)
      const bottomInPage = topInPage + height
      if (height > (pageH - bottomSafeZone - topMargin)) continue
      if (bottomInPage > (pageH - bottomSafeZone)) {
        const distToNextPage = pageH - topInPage + topMargin
        const currentMt = parseFloat(el.style.marginTop || '0')
        el.style.marginTop = `${currentMt + distToNextPage}px`
        stable = false; break
      }
    }
  }

  const finalCvRect = cvRoot.value.getBoundingClientRect()
  let maxBottom = 0
  allElements.forEach(el => {
    const bottom = (el.getBoundingClientRect().bottom - finalCvRect.top) / scale
    if (bottom > maxBottom) maxBottom = bottom
  })
  pageCount.value = Math.max(1, Math.ceil(maxBottom / pageH))
}

watch(() => props.resumeData, requestPagination, { deep: true })

onMounted(() => {
  if (props.resumeData?.sections) {
    const LEFT = ['education', 'experience']
    const RIGHT = ['awards', 'certifications', 'skills']
    props.resumeData.sections.forEach(sec => {
      if (sec.isVisible === undefined) {
        sec.isVisible = [...LEFT, ...RIGHT].includes(sec.id) || sec.id === 'summary'
        sec.column = LEFT.includes(sec.id) ? 'left' : 'right'
      } else {
        if (LEFT.includes(sec.id)) {
          sec.column = 'left'
        } else if (RIGHT.includes(sec.id)) {
          sec.column = 'right'
        }
      }
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
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&display=swap');

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

.section-block {
  padding: 8px;
  border: 2px solid transparent;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.section-active {
  border-color: #5ba4b5 !important;
  background: rgba(91, 164, 181, 0.05) !important;
  z-index: 100 !important;
}

.nav-btns {
  position: absolute;
  right: 4px;
  top: 4px;
  display: flex;
  gap: 4px;
  z-index: 9999;
}

.nav-btn {
  padding: 2px 4px;
  background: #5ba4b5;
  color: white;
  border-radius: 4px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.item-container {
  transition: all 0.2s;
}

.delete-btn {
  position: absolute;
  background: #ef4444;
  color: white;
  border-radius: 999px;
  width: 18px;
  height: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  top: -5px;
  right: -5px;
  opacity: 0;
  transition: all 0.2s;
  z-index: 40;
}

.item-container:hover .delete-btn {
  opacity: 1;
}

.paginated-item { transition: none !important; }

:deep(.html-content ul) { list-style-type: decimal; padding-left: 1.25rem; }
:deep(.html-content li) { margin-bottom: 0.25rem; }
:deep(.html-content b), :deep(.html-content strong) { font-weight: 800; }

@media print {
  .no-print { display: none !important; }
  .section-block {
    border-color: transparent !important;
    background: transparent !important;
    padding: 0 !important;
    border-radius: 0 !important;
  }
}
</style>
