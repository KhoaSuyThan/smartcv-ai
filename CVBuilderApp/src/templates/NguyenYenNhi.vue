<template>
  <div id="cv-printable-area" ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-col relative box-border text-[#333] leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', \'Segoe UI\', sans-serif' }"
    @click.self="selectedSectionId = null">

    <!-- NỀN TRANG TRÍ -->
    <div class="absolute top-[-50mm] right-[-50mm] w-[150mm] h-[150mm] rounded-full bg-pink-100/50 blur-[80px] z-0 no-print"></div>
    <div class="absolute top-[100mm] left-[-30mm] w-[100mm] h-[100mm] rounded-full bg-red-50/40 blur-[60px] z-0 no-print"></div>

    <!-- HEADER -->
    <header class="relative z-10 pt-[12mm] px-[15mm] pb-[4mm] flex items-start paginated-item">
      <div class="flex-grow mr-[10mm]">
        <div class="mb-2">
          <h1 class="text-[55px] font-black tracking-[0.05em] leading-[1.1] text-slate-900"
            style="font-family: 'Playfair Display', serif;">
            {{ cleanedFullName }}
          </h1>
        </div>
        <h2 class="text-[12px] font-extrabold text-slate-700 uppercase tracking-[0.05em] mt-1.5"
          v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'CHUYÊN VIÊN THIẾT KẾ ĐỒ HỌA'">
        </h2>
      </div>
      <div class="relative flex-shrink-0 mt-[-5mm] mr-4">
        <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[64mm] h-[64mm] rounded-full bg-gradient-to-br from-pink-400/30 to-red-300/20 blur-xl z-0"></div>
        <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[54mm] h-[54mm] rounded-full bg-gradient-to-br from-pink-500 to-red-400 opacity-90 z-0"></div>
        <div class="w-[50mm] h-[50mm] rounded-full border-[6px] border-white shadow-2xl overflow-hidden bg-slate-100 relative z-10">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center text-slate-300">
            <svg class="w-24 h-24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path>
            </svg>
          </div>
        </div>
        <svg class="absolute -top-4 -right-4 w-[75mm] h-[75mm] z-20 pointer-events-none" viewBox="0 0 100 100">
          <path d="M75,15 Q95,45 70,65 T35,85" fill="none" stroke="#222" stroke-width="0.3" class="opacity-40" />
          <path d="M85,25 Q100,55 75,75 T45,95" fill="none" stroke="#222" stroke-width="0.2" class="opacity-30" />
          <path d="M20,15 L22,21 L28,23 L22,25 L20,31 L18,25 L12,23 L18,21 Z" fill="currentColor" class="text-slate-900 opacity-80" />
        </svg>
      </div>
    </header>

    <div class="absolute top-0 right-0 w-[4mm] h-[100%] bg-gradient-to-b from-pink-500 to-red-400 z-0 opacity-10 no-print"></div>

    <!-- CONTENT BODY -->
    <div class="flex px-[15mm] py-[2mm] gap-[10mm] relative z-10 mb-2" @click.self="selectedSectionId = null">

      <!-- CỘT TRÁI (SIDEBAR) -->
      <aside class="flex-[1.1] min-w-[75mm] flex flex-col gap-5" @click.self="selectedSectionId = null">
        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block relative paginated-item"
            :class="{ 'section-active': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <div v-show="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
              <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg></button>
              <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg></button>
            </div>

            <h3 class="font-black uppercase mb-3 tracking-wider text-slate-900" style="font-size: 20px !important; font-weight: bold !important;">{{ section.title }}</h3>

            <div class="space-y-3">
              <div v-if="section.id === 'summary'"
                class="text-[12.5px] leading-[1.6] text-slate-700 text-justify font-medium html-content paginated-item"
                v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Mô tả mục tiêu nghề nghiệp của bạn...'">
              </div>

              <div v-else-if="section.id === 'education'" class="space-y-4">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="relative pl-6 paginated-item item-container">
                  <button v-show="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print" style="top: 0; right: 0;"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg></button>
                  <div class="absolute left-0 top-1 text-pink-500"><svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24"><path d="M12,2L14.5,9H21L15.5,13.5L18,20.5L12,16L6,20.5L8.5,13.5L3,9H9.5L12,2Z" /></svg></div>
                  <div class="text-[12px] font-black text-pink-500 mb-0.5 tracking-wide">{{ item.year || '2024 - 2028' }}</div>
                  <div class="text-[14px] font-black text-slate-900 leading-tight mb-0.5">{{ item.major || 'Chuyên ngành' }}</div>
                  <div class="text-[12px] font-bold text-slate-500 italic">{{ item.school || 'Tên trường học' }}</div>
                </div>
              </div>

              <div v-else-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'" class="space-y-2.5">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container relative">
                  <button v-show="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print" style="top: -5px; right: -5px;"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg></button>
                  <div class="flex justify-between items-end mb-1">
                    <span class="text-[12px] font-bold text-slate-800 leading-tight">{{ item.name }}</span>
                    <span class="text-[10px] font-bold text-pink-500 whitespace-nowrap ml-2">{{ getLevelInfo(item.level).text }}</span>
                  </div>
                  <div class="w-full h-[5px] bg-slate-100 rounded-full overflow-hidden relative shadow-inner">
                    <div class="absolute h-full left-0 top-0 rounded-full bg-gradient-to-r from-red-500 via-pink-400 to-red-400 shadow-sm" :style="{ width: getLevelInfo(item.level).percent }"></div>
                  </div>
                </div>
              </div>

              <div v-else class="space-y-2.5">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container pl-6 relative">
                  <button v-show="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print" style="top: -2px; right: -2px;"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg></button>
                  <div class="absolute left-0 top-1 text-pink-500"><svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M12,2L14.5,9H21L15.5,13.5L18,20.5L12,16L6,20.5L8.5,13.5L3,9H9.5L12,2Z" /></svg></div>
                  <div class="text-[12.5px] font-bold text-slate-800" v-html="formatDesc(item.name || item.desc || item.info)"></div>
                </div>
              </div>
            </div>
          </div>
        </template>
      </aside>

      <!-- CỘT PHẢI (MAIN) -->
      <main class="flex-1 min-w-[100mm] flex flex-col gap-5" @click.self="selectedSectionId = null">

        <!-- LIÊN HỆ CỐ ĐỊNH -->
        <div class="contact-block p-3 border-2 border-slate-50 rounded-2xl bg-white/50 backdrop-blur-sm paginated-item">
          <h3 class="font-black uppercase mb-3 tracking-wider text-slate-900" style="font-size: 20px !important; font-weight: bold !important;">LIÊN HỆ</h3>
          <div class="space-y-2.5 text-[12.5px] font-bold text-slate-700">
            <div class="flex items-center gap-4" v-if="!isEmpty(resumeData.general.phone)">
              <div class="w-7 h-7 rounded-full bg-pink-50 flex items-center justify-center text-pink-500 flex-shrink-0"><svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24"><path d="M6.62,10.79C8.06,13.62 10.38,15.94 13.21,17.38L15.41,15.18C15.69,14.9 16.08,14.82 16.43,14.93C17.55,15.3 18.75,15.5 20,15.5A1,1 0 0,1 21,16.5V20A1,1 0 0,1 20,21A17,17 0 0,1 3,4A1,1 0 0,1 4,3H7.5A1,1 0 0,1 8.5,4C8.5,5.25 8.7,6.45 9.07,7.57C9.18,7.92 9.1,8.31 8.82,8.59L6.62,10.79Z" /></svg></div>
              <span v-html="resumeData.general.phone"></span>
            </div>
            <div class="flex items-center gap-4" v-if="!isEmpty(resumeData.general.email)">
              <div class="w-7 h-7 rounded-full bg-pink-50 flex items-center justify-center text-pink-500 flex-shrink-0"><svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24"><path d="M20,4H4C2.89,4 2,4.89 2,6V18A2,2 0 0,0 4,20H20A2,2 0 0,0 22,18V6C22,4.89 21.1,4 20,4M20,8L12,13L4,8V6L12,11L20,6V8Z" /></svg></div>
              <span v-html="resumeData.general.email"></span>
            </div>
            <div class="flex items-center gap-4" v-if="!isEmpty(resumeData.general.address)">
              <div class="w-7 h-7 rounded-full bg-pink-50 flex items-center justify-center text-pink-500 flex-shrink-0"><svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24"><path d="M12,2C8.13,2 5,5.13 5,9C5,14.25 12,22 12,22C12,22 19,14.25 19,9C19,5.13 15.87,2 12,2M12,11.5A2.5,2.5 0 0,1 9.5,9A2.5,2.5 0 0,1 12,6.5A2.5,2.5 0 0,1 14.5,9A2.5,2.5 0 0,1 12,11.5Z" /></svg></div>
              <span v-html="resumeData.general.address"></span>
            </div>
          </div>
        </div>

        <!-- CÁC SECTION MAIN -->
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block relative paginated-item"
            :class="{ 'section-active': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <div v-show="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
              <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
              <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg></button>
            </div>

            <h3 class="font-black uppercase mb-4 tracking-wider text-slate-900 border-b border-pink-50 pb-2 paginated-item" style="font-size: 20px !important; font-weight: bold !important;">{{ section.title }}</h3>

            <div v-if="section.id === 'summary'"
              class="text-[12.5px] leading-[1.6] text-slate-700 text-justify font-medium html-content paginated-item"
              v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Mô tả mục tiêu nghề nghiệp của bạn...'">
            </div>

            <div v-else-if="section.id === 'experience' || section.id === 'project' || section.id === 'activities'" class="space-y-6 relative">
              <div class="absolute left-[7px] top-2 bottom-6 w-[2px] bg-pink-50 z-0"></div>
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="relative pl-8 paginated-item item-container z-10">
                <button v-show="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print" style="left: -2px; top: -2px;"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg></button>
                <div class="absolute left-0 top-1.5 text-pink-500 bg-white shadow-sm ring-4 ring-white rounded-full"><svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M12,2L14.5,9H21L15.5,13.5L18,20.5L12,16L6,20.5L8.5,13.5L3,9H9.5L12,2Z" /></svg></div>
                <div class="text-[12.5px] font-black text-pink-500 mb-1 tracking-wide flex justify-between">
                  <span>{{ item.time || item.year || '2024 - Hiện tại' }}</span>
                  <span class="text-slate-500 uppercase font-black text-[10px] opacity-60 tracking-widest">{{ section.id === 'experience' ? (item.company || 'CÔNG TY') : (item.name || 'DỰ ÁN') }}</span>
                </div>
                <h4 class="text-[14.5px] font-black text-slate-900 leading-tight mb-1.5">
                  {{ section.id === 'experience' ? item.role : (item.role || item.name || 'Vị trí') }}
                  <span v-if="section.id === 'experience' && item.company" class="text-slate-400 font-bold text-[13px] ml-1 opacity-80">| {{ item.company }}</span>
                </h4>
                <div class="text-[12.5px] leading-[1.6] text-slate-600 text-justify font-medium html-content" v-html="formatDesc(item.desc || '')"></div>
              </div>
            </div>

            <div v-else-if="section.id === 'education'" class="space-y-4">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="relative pl-6 paginated-item item-container">
                <button v-show="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print" style="top: 0; right: 0;"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg></button>
                <div class="absolute left-0 top-1 text-pink-500"><svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24"><path d="M12,2L14.5,9H21L15.5,13.5L18,20.5L12,16L6,20.5L8.5,13.5L3,9H9.5L12,2Z" /></svg></div>
                <div class="text-[12px] font-black text-pink-500 mb-0.5 tracking-wide">{{ item.year || '2024 - 2028' }}</div>
                <div class="text-[14px] font-black text-slate-900 leading-tight mb-0.5">{{ item.major || 'Chuyên ngành' }}</div>
                <div class="text-[12px] font-bold text-slate-500 italic">{{ item.school || 'Tên trường học' }}</div>
              </div>
            </div>

            <div v-else class="space-y-3" style="padding-left: 4px;">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="relative pl-6 paginated-item item-container">
                <button v-show="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print" style="top: -2px; right: -2px;"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg></button>
                <div class="absolute left-0 top-1 text-pink-500"><svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M12,2L14.5,9H21L15.5,13.5L18,20.5L12,16L6,20.5L8.5,13.5L3,9H9.5L12,2Z" /></svg></div>
                <div class="html-content font-medium text-justify text-[12.5px] text-slate-800" v-html="formatDesc(item.desc || item.info || item.name)"></div>
              </div>
            </div>
          </div>
        </template>
      </main>
    </div>

    <div class="absolute bottom-[4mm] right-[10mm] flex items-center gap-1 text-pink-500 opacity-60 z-20 no-print">
      <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M12,2L14.5,9H21L15.5,13.5L18,20.5L12,16L6,20.5L8.5,13.5L3,9H9.5L12,2Z" /></svg>
      <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M12,2L14.5,9H21L15.5,13.5L18,20.5L12,16L6,20.5L8.5,13.5L3,9H9.5L12,2Z" /></svg>
      <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M12,2L14.5,9H21L15.5,13.5L18,20.5L12,16L6,20.5L8.5,13.5L3,9H9.5L12,2Z" /></svg>
    </div>

    <template v-for="p in pageCount" :key="'footer-border-'+p">
      <div class="absolute left-0 w-full flex items-center z-40 pointer-events-none"
        :style="{ top: `calc(${p * 297}mm - 12mm)`, height: '1.5px', paddingLeft: '15mm', paddingRight: '15mm' }">
        <div class="w-full h-full opacity-30 bg-gradient-to-r from-pink-400 via-red-400 to-pink-400"></div>
      </div>
    </template>

    <template v-for="p in (pageCount - 1)" :key="'div-'+p">
      <div class="absolute left-[-2.5%] w-[105%] z-50 flex flex-col items-center justify-center pointer-events-none no-print"
        :style="{ top: `calc(${p * 297}mm - 12px)` }">
        <div class="w-full h-[24px] bg-slate-900 shadow-[inset_0_2px_10px_rgba(0,0,0,0.5)] border-y border-white/10 flex items-center justify-center overflow-hidden">
          <div class="w-full h-[1px] bg-gradient-to-r from-transparent via-pink-500/50 to-transparent"></div>
        </div>
        <span class="absolute text-[9px] uppercase font-black text-white tracking-[0.3em] bg-slate-800 px-4 py-1 rounded-full border border-pink-500/30 shadow-2xl">Ngắt trang {{ p + 1 }}</span>
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

const cleanedFullName = computed(() => {
  const raw = props.resumeData.general.fullName || ''
  return raw.replace(/<[^>]*>/g, '').replace(/&nbsp;/g, ' ').replace(/\u00a0/g, ' ').trim() || 'Nguyễn Yến Nhi'
})

const sidebarSections = computed(() => props.resumeData.sections.filter(s => s.column === 'left'))
const mainSections = computed(() => props.resumeData.sections.filter(s => s.column === 'right'))
const sidebarIds = computed(() => sidebarSections.value.map(s => s.id))
const mainIds = computed(() => mainSections.value.map(s => s.id))

const formatDesc = (text) => {
  if (!text) return ''
  if (/<[a-z][\s\S]*>/i.test(text)) return text
  return text.split('\n').map(l => l.trim()).filter(l => l).join('<br/>')
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

// ── Pagination ──────────────────────────────────────────────────────────────
let paginateTimer = null
const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 50)
}

const doPagination = async () => {
  if (!cvRoot.value) return
  cvRoot.value.querySelectorAll('.paginated-item').forEach(el => { el.style.marginTop = '' })
  await nextTick()

  const A4_W = 210, A4_H = 297, MB = 18, MT = 15
  const pxMm = cvRoot.value.offsetWidth / A4_W
  const pageH = A4_H * pxMm
  const mbPx = MB * pxMm
  const mtPx = MT * pxMm

  const relTop = (el) => {
    let off = 0, cur = el
    while (cur && cur !== cvRoot.value) { off += cur.offsetTop; cur = cur.offsetParent }
    return off
  }

  const processColumn = (sel) => {
    const col = cvRoot.value.querySelector(sel)
    if (!col) return
    const items = col.querySelectorAll('.paginated-item')
    let stable = false, attempts = 0
    while (!stable && attempts < 50) {
      stable = true; attempts++
      for (const item of items) {
        const top = relTop(item), h = item.offsetHeight, bottom = top + h
        const pageIdx = Math.floor(top / pageH)
        const safeBottom = (pageIdx + 1) * pageH - mbPx
        if (bottom > safeBottom && top < (pageIdx + 1) * pageH) {
          if (h > pageH - mtPx - mbPx) continue
          const delta = (pageIdx + 1) * pageH + mtPx - top
          item.style.marginTop = (parseFloat(item.style.marginTop || '0') + delta) + 'px'
          stable = false; break
        }
      }
    }
  }

  processColumn('header')
  processColumn('aside')
  processColumn('main')

  let maxBottom = 0
  cvRoot.value.querySelectorAll('.paginated-item').forEach(el => {
    const b = relTop(el) + el.offsetHeight
    if (b > maxBottom) maxBottom = b
  })
  const calc = Math.max(1, Math.ceil((maxBottom + mbPx - 2) / pageH))
  if (pageCount.value !== calc) pageCount.value = calc
}

watch(() => props.resumeData, requestPagination, { deep: true })

onMounted(() => {
  if (props.resumeData?.sections) {
    const SIDEBAR = ['summary', 'education', 'skills']
    const MAIN = ['experience', 'project']
    props.resumeData.sections.forEach(sec => {
      sec.isVisible = [...SIDEBAR, ...MAIN].includes(sec.id)
      sec.column = SIDEBAR.includes(sec.id) ? 'left' : 'right'
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
@import url('https://fonts.googleapis.com/css2?family=Playfair+Display:wght@900&family=Inter:wght@400;500;600;700;800;900&display=swap');

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

/* ── Section block mặc định ── */
.section-block {
  padding: 12px;
  /* border transparent để giữ chỗ, tránh layout shift khi active */
  border-width: 2px !important;
  border-style: solid !important;
  border-color: transparent !important;
  border-radius: 16px !important;
  cursor: pointer;
  /* transform phải nằm trong transition để zoom hoạt động */
  transition:
    transform 0.18s ease,
    border-color 0.15s ease,
    border-radius 0.15s ease,
    box-shadow 0.18s ease !important;
  transform-origin: center center;
}

/* Hover: hoàn toàn không có thay đổi visual */
.section-block:hover {
  background: transparent !important;
  box-shadow: none !important;
}

/* ── Active: đây là phần quan trọng được fix ── */
.section-active {
  /* 1. Zoom nhẹ — scale nằm trong transform, không bị ghi đè */
  transform: scale(1.013) !important;
  transform-origin: center center !important;

  /* 2. Bo góc 6px đúng yêu cầu */
  border-radius: 6px !important;

  /* 3. Viền solid, màu trùng background của section (trắng = #ffffff)
        Dùng màu nhẹ hơn một chút (#f8f8f8 → hơi tối hơn trắng) để tạo cảm giác "viền chìm"
        Vì background của section-block là white/transparent, ta dùng màu xám rất nhạt
        để border vẫn thấy nhưng không lấn át nội dung */
  border-style: solid !important;
  border-width: 2px !important;
  border-color: #f9a8d4 !important; /* pink-300 — gần với nền trắng/hồng nhạt của template */

  /* 4. Shadow nhẹ để khung nổi bật */
  box-shadow: 0 6px 20px rgba(244, 114, 182, 0.20) !important;

  /* 5. Background hơi tinted để phân biệt */
  background: rgba(253, 242, 248, 0.6) !important; /* pink-50/60 */

  z-index: 10 !important;
  position: relative !important;
}

/* ── Nav Buttons ── */
.nav-btns {
  position: absolute;
  right: 8px;
  top: 8px;
  display: flex;
  flex-direction: row;
  gap: 5px;
  z-index: 9999;
}

.nav-btn {
  width: 26px;
  height: 26px;
  background: #1e293b !important;
  color: white !important;
  border: none;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.2);
  transition: background 0.12s ease, transform 0.1s ease;
}
.nav-btn:hover { background: #f472b6 !important; transform: scale(1.1) !important; }
.nav-btn:active { transform: scale(0.95) !important; }
.nav-btn-danger { background: #ef4444 !important; }
.nav-btn-danger:hover { background: #dc2626 !important; }

/* ── Delete Buttons ── */
.item-container { position: relative; }

.delete-btn {
  position: absolute;
  background: #ef4444 !important;
  color: white;
  border-radius: 999px;
  border: none;
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 50;
  cursor: pointer;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.15);
  transition: transform 0.12s ease;
}
.delete-btn:hover { transform: scale(1.15) !important; }
.delete-btn:active { transform: scale(0.9) !important; }

/* ── Contact block (không active được, chỉ display) ── */
.contact-block {
  border-width: 2px;
  border-style: solid;
  border-color: #f1f5f9; /* slate-100 */
}

/* ── Rich text ── */
:deep(.html-content ul) { list-style-type: none !important; padding-left: 0.5rem !important; }
:deep(.html-content li) { margin-bottom: 0.25rem; position: relative; padding-left: 1.2rem; }
:deep(.html-content li::before) { content: "✦"; position: absolute; left: 0; color: #f472b6; font-size: 14px; }
:deep(.html-content b), :deep(.html-content strong) { font-weight: 800 !important; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic !important; }
:deep(.html-content u) { text-decoration: underline !important; }

/* ── Print ── */
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