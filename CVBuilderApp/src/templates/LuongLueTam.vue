<template>
  <div id="cv-printable-area" ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-row relative box-border text-[#333] leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: `'Inter', sans-serif` }">

    <!-- ===== HEADER OVERLAY ===== -->
    <div class="absolute top-0 left-0 w-full z-30 pointer-events-none flex" style="height: 75mm;">
      <div class="flex flex-col justify-end pb-[6mm] pl-[10mm] pr-[6mm] pointer-events-auto shrink-0"
        style="width: 125mm; background-color: #9db078;">
        <h1 class="uppercase font-bold text-white/80 leading-none tracking-[0.15em]" style="font-size: 38px !important;">
          {{ splitName.last }}
        </h1>
        <h1 class="font-cursive text-white leading-none tracking-wide"
          style="font-size: 58px; text-shadow: 1px 2px 4px rgba(0,0,0,0.12);">
          {{ splitName.first }}
        </h1>
        <div class="flex items-center gap-3 mt-3">
          <span class="font-bold uppercase tracking-[0.18em] text-[#334155]" style="font-size: 12px !important;"
            v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'Vị trí ứng tuyển'">
          </span>
          <div class="flex-1 h-[2px] bg-[#334155]/50 max-w-[100px]"></div>
        </div>
      </div>
      <div class="flex-1 flex items-start justify-end pr-[8mm] pointer-events-auto" style="padding-top: 4mm;">
        <div class="bg-gray-200 border-[3px] border-white shadow-md overflow-hidden relative z-30"
          style="width: 46mm; height: 58mm;">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center text-gray-400">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1"
                d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
            </svg>
          </div>
        </div>
      </div>
    </div>

    <!-- ===== SIDEBAR TRÁI ===== -->
    <aside class="shrink-0 z-10 flex flex-col relative" style="width: 85mm;" :style="{ backgroundColor: templatePrimaryColor }">
      <div class="absolute top-[78mm] bottom-[10mm] z-10"
        style="left: 8mm; width: 1.5px; background: rgba(255,255,255,0.55);"></div>
      <div style="height: 78mm; flex-shrink: 0;"></div>

      <div class="flex flex-col flex-1 relative z-20" style="padding: 0 5mm 8mm 20mm;">

        <!-- Liên hệ (cố định) -->
        <div class="section-block mb-7 relative">
          <div class="paginated-item relative z-20">
            <div class="absolute z-20 rounded-full bg-white shadow-[0_0_0_3px_var(--dot-color)]"
              :style="{ '--dot-color': templatePrimaryColor, left: '-12mm', top: '10px', width: '10px', height: '10px', transform: 'translateX(-50%)' }">
            </div>
            <h3 class="font-bold uppercase px-3 py-1 inline-block mb-4 shadow-[3px_3px_0px_rgba(0,0,0,0.15)] tracking-wide"
              style="background: white; font-size: 13px !important; color: #465568;">
              Liên hệ
            </h3>
          </div>
          <div class="flex flex-col gap-3 paginated-item" style="font-size: 11px !important; color: rgba(255,255,255,0.9);">
            <div class="flex items-center gap-2" v-if="!isEmpty(resumeData.general.phone)">
              <svg class="w-3.5 h-3.5 shrink-0 opacity-80" fill="currentColor" viewBox="0 0 20 20"><path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z"/></svg>
              <span class="font-semibold text-white tracking-wide break-all" v-html="resumeData.general.phone"></span>
            </div>
            <div class="flex items-center gap-2" v-if="!isEmpty(resumeData.general.email)">
              <svg class="w-3.5 h-3.5 shrink-0 opacity-80" fill="currentColor" viewBox="0 0 20 20"><path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z"/><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z"/></svg>
              <span class="font-semibold text-white tracking-wide break-all" v-html="resumeData.general.email"></span>
            </div>
            <div class="flex items-center gap-2" v-if="!isEmpty(resumeData.general.website) || !isEmpty(resumeData.general.github) || !isEmpty(resumeData.general.linkedin)">
              <svg class="w-3.5 h-3.5 shrink-0 opacity-80" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM4.332 8.027a6.012 6.012 0 011.912-2.706C6.512 5.73 6.974 6 7.5 6A1.5 1.5 0 019 7.5V8a2 2 0 004 0 2 2 0 011.523-1.943A5.977 5.977 0 0116 10c0 .34-.028.675-.083 1H15a2 2 0 00-2 2v2.197A5.973 5.973 0 0110 16v-2a2 2 0 00-2-2 2 2 0 01-2-2 2 2 0 00-1.668-1.973z" clip-rule="evenodd"/></svg>
              <span class="font-semibold text-white tracking-wide break-all"
                v-html="!isEmpty(resumeData.general.website) ? resumeData.general.website : (!isEmpty(resumeData.general.github) ? resumeData.general.github : resumeData.general.linkedin)">
              </span>
            </div>
            <div class="flex items-start gap-2" v-if="!isEmpty(resumeData.general.address)">
              <svg class="w-3.5 h-3.5 shrink-0 opacity-80 mt-[1px]" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd"/></svg>
              <span class="font-semibold text-white tracking-wide break-words" v-html="resumeData.general.address"></span>
            </div>
          </div>
        </div>

        <!-- Sidebar sections -->
        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block paginated-item mb-7 relative"
            :class="{ 'section-active--sidebar': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <!-- ✅ v-if: chỉ render DOM khi click, không bao giờ hiện trước -->
            <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="moveSectionUp(section.id, 'left')" class="nav-btn" title="Di chuyển lên">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionDown(section.id, 'left')" class="nav-btn" title="Di chuyển xuống">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionHorizontal(section.id, 'right')" class="nav-btn" title="Sang Phải">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
              </button>
            </div>

            <div class="paginated-item relative z-20">
              <div class="absolute z-20 rounded-full bg-white"
                :style="{ boxShadow: `0 0 0 3px ${templatePrimaryColor}`, left: '-12mm', top: '10px', width: '10px', height: '10px', transform: 'translateX(-50%)' }">
              </div>
              <h3 class="font-bold uppercase px-3 py-1.5 inline-block mb-4 shadow-[3px_3px_0px_rgba(0,0,0,0.15)] tracking-wide break-words whitespace-normal"
                style="background: white; font-size: 13px !important; color: #465568;">
                {{ section.title }}
              </h3>
            </div>

            <!-- Summary -->
            <div v-if="section.id === 'summary'"
              class="leading-relaxed text-justify html-content font-medium paginated-item"
              style="font-size: 11px !important; color: rgba(255,255,255,0.9);"
              v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Trình bày ngắn gọn từ 2-3 câu về số năm kinh nghiệm...'">
            </div>

            <!-- Skills / Languages / IT -->
            <div v-else-if="['skills','languages','it_skills'].includes(section.id)" class="space-y-3">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="flex flex-col item-container relative"
                style="font-size: 11px !important; color: white;">
                <div class="flex items-center gap-2 mb-1">
                  <div class="rounded-full bg-white shrink-0" style="width: 6px; height: 6px;"></div>
                  <span class="font-bold tracking-wide">{{ item.name }}</span>
                </div>
                <div class="w-full rounded-full overflow-hidden relative" style="height: 5px; background: rgba(255,255,255,0.2);">
                  <div class="absolute left-0 top-0 bottom-0 rounded-full"
                    :style="{ width: getLevelPercent(item.level), backgroundColor: templateSecondaryColor }"></div>
                </div>
                <!-- ✅ v-if: nút xóa chỉ render khi active -->
                <button v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print absolute" style="right: 0; top: 0;">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Các section khác -->
            <div v-else class="space-y-2">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="font-medium leading-relaxed item-container relative"
                style="font-size: 11px !important; color: rgba(255,255,255,0.9);">
                <div class="flex items-start gap-2">
                  <div class="rounded-full bg-white shrink-0" style="width: 6px; height: 6px; margin-top: 6px;"></div>
                  <span class="html-content" v-html="formatDesc(item.desc || item.name || item.info)"></span>
                </div>
                <button v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print absolute" style="right: 0; top: 0;">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>
      </div>
    </aside>

    <!-- ===== MAIN CONTENT PHẢI ===== -->
    <main class="flex-1 flex flex-col relative bg-white z-20" @click.self="selectedSectionId = null">
      <div style="height: 78mm; flex-shrink: 0; border-left: 1.5px solid #e2e8f0; margin-left: 12mm;"></div>

      <div class="flex-1 flex flex-col pb-[12mm] relative gap-2" style="padding-left: 10mm; padding-right: 12mm;"
        @click.self="selectedSectionId = null">
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block paginated-item mb-6"
            :class="{ 'section-active--main': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <!-- ✅ v-if -->
            <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="moveSectionUp(section.id, 'right')" class="nav-btn" title="Di chuyển lên">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionDown(section.id, 'right')" class="nav-btn" title="Di chuyển xuống">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionHorizontal(section.id, 'left')" class="nav-btn" title="Sang Trái">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
              </button>
            </div>

            <!-- Tiêu đề section -->
            <div class="flex items-center gap-3 mb-5 paginated-item" style="margin-left: -22px;">
              <div class="rounded-full flex items-center justify-center shrink-0"
                :style="{ width: '34px', height: '34px', border: `4px solid ${templateSecondaryColor}`, backgroundColor: '#dce4cd' }">
                <div class="rounded-full" :style="{ width: '10px', height: '10px', backgroundColor: templatePrimaryColor }"></div>
              </div>
              <h3 class="font-bold uppercase text-white tracking-widest shadow-[3px_3px_0px_rgba(0,0,0,0.15)]"
                style="font-size: 14px !important; padding: 6px 18px;"
                :style="{ backgroundColor: templateSecondaryColor }">
                {{ section.title }}
              </h3>
            </div>

            <!-- Education / Experience / Project / Activities -->
            <div v-if="['education','experience','project','activities'].includes(section.id)" class="space-y-5">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative flex items-stretch">
                <div class="pr-3 relative shrink-0" style="width: 36%;">
                  <div class="absolute rounded-full bg-slate-400" style="left: -14px; top: 7px; width: 7px; height: 7px;"></div>
                  <h4 class="font-bold leading-snug mb-1" style="font-size: 11.5px !important; color: #222;">
                    <span v-html="section.id === 'education' ? item.school : (section.id === 'experience' ? item.company : item.name)"></span>
                  </h4>
                  <div v-if="item.year || item.time"
                    class="inline-block text-white font-bold tracking-wide shadow-[2px_2px_0px_rgba(0,0,0,0.15)]"
                    style="font-size: 10px !important; padding: 2px 8px; margin-top: 3px;"
                    :style="{ backgroundColor: templateSecondaryColor }">
                    {{ item.year || item.time }}
                  </div>
                </div>
                <div class="pb-2 relative" style="width: 64%; border-left: 2px solid #cbd5e1; padding-left: 12px;">
                  <h4 v-if="item.major || item.role" class="font-bold mb-1" style="font-size: 11.5px !important; color: #333;">
                    {{ item.major || item.role }}
                  </h4>
                  <div v-if="item.desc" class="text-slate-700 html-content leading-relaxed text-justify font-medium"
                    style="font-size: 11px !important;"
                    v-html="formatDesc(item.desc)">
                  </div>
                  <div v-if="item.gradType" class="font-semibold mt-1"
                    :style="{ fontSize: '11px !important', color: templateSecondaryColor }">
                    Xếp loại: {{ item.gradType }}
                  </div>
                </div>
                <button v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print absolute" style="right: -8px; top: -4px;">
                  <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Awards / Certifications -->
            <div v-else-if="['awards','certifications'].includes(section.id)" class="space-y-4">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative flex items-stretch">
                <div class="pr-3 relative shrink-0" style="width: 36%;">
                  <div class="absolute rounded-full bg-slate-400" style="left: -14px; top: 7px; width: 7px; height: 7px;"></div>
                  <div v-if="item.year"
                    class="inline-block text-white font-bold tracking-wide shadow-[2px_2px_0px_rgba(0,0,0,0.15)]"
                    style="font-size: 10px !important; padding: 2px 8px;"
                    :style="{ backgroundColor: templateSecondaryColor }">
                    {{ item.year }}
                  </div>
                </div>
                <div class="pb-2 relative" style="width: 64%; border-left: 2px solid #cbd5e1; padding-left: 12px;">
                  <span class="font-bold" style="font-size: 11.5px !important; color: #333;">{{ item.name || item.info }}</span>
                </div>
                <button v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print absolute" style="right: -8px; top: -4px;">
                  <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Mặc định -->
            <div v-else class="space-y-3" style="padding-left: 4px;">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative text-[#333] leading-relaxed"
                style="font-size: 11px !important;">
                <div class="absolute rounded-full bg-slate-400" style="left: -18px; top: 7px; width: 6px; height: 6px;"></div>
                <div class="html-content font-medium text-justify" v-html="formatDesc(item.desc || item.info || item.name)"></div>
                <button v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print absolute" style="right: -8px; top: -4px;">
                  <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>
      </div>
    </main>

    <!-- Page break markers -->
    <template v-for="p in (pageCount - 1)" :key="'div-'+p">
      <div class="absolute left-0 w-full z-50 flex flex-col items-center justify-center pointer-events-none no-print"
        :style="{ top: `calc(${p * 297}mm - 8px)` }">
        <div class="w-[105%] h-[16px] bg-slate-800/95 shadow-inner overflow-hidden border-y border-black/30 backdrop-blur-sm"></div>
        <span class="absolute text-[9px] uppercase font-bold text-slate-300 tracking-widest bg-slate-700 px-3 py-0.5 rounded border border-slate-600 shadow-md">
          Ngắt trang {{ p + 1 }}
        </span>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, ref, onMounted, nextTick, watch, onUnmounted } from 'vue'

// ─── Refs ──────────────────────────────────────────────────────────────────
const cvRoot            = ref(null)
const pageCount         = ref(1)
const selectedSectionId = ref(null)

// ─── Props / Emits ─────────────────────────────────────────────────────────
const props = defineProps({ resumeData: { type: Object, required: true } })
const emit  = defineEmits(['removeItem'])

// ─── Helpers ───────────────────────────────────────────────────────────────
const isEmpty = (val) => {
  if (!val) return true
  if (typeof val !== 'string') return false
  return val.replace(/<[^>]*>/g, '').trim() === ''
}

const formatDesc = (text) => {
  if (!text) return ''
  // Nếu đã là HTML → render trực tiếp
  if (/<[a-z][\s\S]*>/i.test(text)) {
    if (text.includes('<li') && (text.includes('<font') || text.includes('style='))) {
      try {
        const div = document.createElement('div')
        div.innerHTML = text
        div.querySelectorAll('li').forEach(li => {
          const child = li.firstElementChild
          if (child && (child.tagName === 'FONT' || child.tagName === 'SPAN')) {
            if (child.color)        li.style.color = child.color
            if (child.style?.color) li.style.color = child.style.color
          }
        })
        return div.innerHTML
      } catch { return text }
    }
    return text
  }
  // Thuần text → escape rồi convert newline → <br>
  return text
    .split('\n')
    .map(l => l.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').trim())
    .filter(Boolean)
    .join('<br/>')
}

// ─── Theme colors ──────────────────────────────────────────────────────────
const templatePrimaryColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  return (!c || c.toLowerCase() === '#2b5c8f') ? '#465568' : c
})
const templateSecondaryColor = '#9db078'

// ─── Split name ────────────────────────────────────────────────────────────
const splitName = computed(() => {
  const raw   = props.resumeData.general.fullName || ''
  const clean = raw.replace(/<[^>]*>/g, '').replace(/&nbsp;|\u00a0/g, ' ').trim()
  const full  = clean || 'HỌ Và Tên'
  const parts = full.trim().split(' ')
  return parts.length > 1
    ? { last: parts[0], first: parts.slice(1).join(' ') }
    : { last: full, first: '' }
})

// ─── Section move helpers (splice = reactive-safe) ─────────────────────────
const swapInArray = (arr, i, j) => {
  if (i < 0 || j < 0 || i >= arr.length || j >= arr.length) return
  const tmp = arr[i]; arr.splice(i, 1, arr[j]); arr.splice(j, 1, tmp)
}

const moveSectionUp = (sectionId, col) => {
  const colSecs  = props.resumeData.sections.filter(s => s.column === col && s.id !== 'contact')
  const localIdx = colSecs.findIndex(s => s.id === sectionId)
  if (localIdx <= 0) return
  const secs = props.resumeData.sections
  swapInArray(secs, secs.findIndex(s => s.id === colSecs[localIdx].id), secs.findIndex(s => s.id === colSecs[localIdx - 1].id))
  requestPagination()
}

const moveSectionDown = (sectionId, col) => {
  const colSecs  = props.resumeData.sections.filter(s => s.column === col && s.id !== 'contact')
  const localIdx = colSecs.findIndex(s => s.id === sectionId)
  if (localIdx < 0 || localIdx >= colSecs.length - 1) return
  const secs = props.resumeData.sections
  swapInArray(secs, secs.findIndex(s => s.id === colSecs[localIdx].id), secs.findIndex(s => s.id === colSecs[localIdx + 1].id))
  requestPagination()
}

const moveSectionHorizontal = (sectionId, direction) => {
  const section = props.resumeData.sections.find(s => s.id === sectionId)
  if (!section) return
  section.column = direction === 'right' ? 'right' : 'left'
  selectedSectionId.value = null
  requestPagination()
}

// ─── Pagination ────────────────────────────────────────────────────────────
const A4_W_MM   = 210
const A4_H_MM   = 297
const MARGIN_MM = 18  // margin đồng nhất top/bottom

let paginateTimer = null
const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 60)
}

const doPagination = async () => {
  if (!cvRoot.value) return

  cvRoot.value.querySelectorAll('.paginated-item').forEach(el => { el.style.marginTop = '' })
  await nextTick()

  const pxPerMm   = cvRoot.value.offsetWidth / A4_W_MM
  const pageH     = A4_H_MM * pxPerMm
  const marginPx  = MARGIN_MM * pxPerMm
  const safeLine  = pageH - marginPx
  const maxUsable = safeLine - marginPx  // item lớn hơn này → bỏ qua (cho cắt tự nhiên)

  const relTop = (el) => {
    let off = 0, cur = el
    while (cur && cur !== cvRoot.value) { off += cur.offsetTop; cur = cur.offsetParent }
    return off
  }

  const processColumn = (selector) => {
    const col = cvRoot.value.querySelector(selector)
    if (!col) return
    // Chỉ lấy paginated-item trực tiếp bên trong col (không lấy nested)
    // → pagination kéo cả cụm section, không kéo từng item con
    const allItems = Array.from(col.querySelectorAll('.paginated-item'))
    const colItems = allItems.filter(el => {
      if (el.offsetHeight === 0) return false
      // Loại bỏ nếu có ancestor cũng là paginated-item trong cùng col
      let parent = el.parentElement
      while (parent && parent !== col) {
        if (parent.classList.contains('paginated-item')) return false
        parent = parent.parentElement
      }
      return true
    })

    for (let pass = 0; pass < 80; pass++) {
      let stable = true
      for (const item of colItems) {
        const top     = relTop(item)
        const bottom  = top + item.offsetHeight
        const pageIdx = Math.floor(top / pageH)
        const curSafe = pageIdx * pageH + safeLine

        if (top < curSafe && bottom > curSafe && item.offsetHeight <= maxUsable) {
          const extra = ((pageIdx + 1) * pageH + marginPx) - top
          item.style.marginTop = (parseFloat(item.style.marginTop || '0') + extra) + 'px'
          stable = false
          break
        }
      }
      if (stable) break
    }
  }

  processColumn('aside')
  processColumn('main')

  let maxBottom = 0
  cvRoot.value.querySelectorAll('.paginated-item').forEach(el => {
    if (el.offsetHeight === 0) return
    const b = relTop(el) + el.offsetHeight
    if (b > maxBottom) maxBottom = b
  })

  const needed = Math.max(1, Math.ceil((maxBottom + marginPx) / pageH))
  if (pageCount.value !== needed) pageCount.value = needed
}

// ─── Visibility init ───────────────────────────────────────────────────────
const hasData = (sec) => {
  if (sec.id === 'summary') return !isEmpty(props.resumeData.general.summary || '')
  return Array.isArray(sec.items) && sec.items.length > 0
}

// Đúng 5 mục hiển thị mặc định theo ảnh mẫu:
// Sidebar: summary (Mục tiêu), skills (Kỹ năng)
// Main: experience (Kinh nghiệm), education (Học vấn)
// + Liên hệ cố định (không qua sections)
const DEFAULT_VISIBLE_SIDEBAR = ['summary', 'skills']
const DEFAULT_VISIBLE_MAIN    = ['experience', 'education']

// Các section còn lại vẫn giữ trong data nhưng ẩn mặc định
const SIDEBAR_IDS = ['summary', 'skills', 'languages', 'it_skills', 'certifications', 'awards', 'references', 'hobbies']
const MAIN_IDS    = ['education', 'experience', 'project', 'activities']

onMounted(() => {
  if (props.resumeData?.sections) {
    props.resumeData.sections.forEach(sec => {
      if (sec.id === 'contact') return

      // Gán cột
      if (SIDEBAR_IDS.includes(sec.id)) {
        sec.column = 'left'
      } else if (MAIN_IDS.includes(sec.id)) {
        sec.column = 'right'
      } else if (!sec.column) {
        sec.column = 'right'
      }

      // Chỉ hiện đúng 5 mục theo ảnh mẫu Image 2
      if (DEFAULT_VISIBLE_SIDEBAR.includes(sec.id)) {
        sec.isVisible = true
      } else if (DEFAULT_VISIBLE_MAIN.includes(sec.id)) {
        sec.isVisible = true
      } else {
        sec.isVisible = false
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

watch(() => props.resumeData, requestPagination, { deep: true })

// ─── Computed sections ─────────────────────────────────────────────────────
const sidebarSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'left' && s.id !== 'contact')
)
const mainSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'right')
)

// ─── Progress bar ──────────────────────────────────────────────────────────
const getLevelPercent = (level) => {
  if (!level) return '75%'
  const l = String(level).toLowerCase().trim()
  if (l === 'cơ bản')    return '25%'
  if (l === 'trung cấp') return '50%'
  if (l === 'thành thạo') return '75%'
  if (l === 'chuyên gia') return '100%'
  if (l.includes('%'))   return l
  const num = parseInt(l)
  return isNaN(num) ? '75%' : num + '%'
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=Dancing+Script:wght@700&display=swap');

.font-cursive { font-family: 'Dancing Script', cursive !important; }

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

/* ── Item container ──────────────────────────────────────────────────────── */
.item-container { position: relative; }

/* ── Delete button ───────────────────────────────────────────────────────── */
.delete-btn {
  width: 20px !important;
  height: 20px !important;
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  background: #ef4444 !important;
  color: white !important;
  border: none !important;
  border-radius: 50% !important;
  cursor: pointer !important;
  box-shadow: 0 2px 6px rgba(0,0,0,0.3) !important;
  z-index: 30 !important;
  transition: transform 0.12s ease !important;
}
.delete-btn:hover  { transform: scale(1.15) !important; }
.delete-btn:active { transform: scale(0.9)  !important; }

/* ── Section block base — KHÔNG có hover effect ──────────────────────────── */
.section-block {
  position: relative !important;
  border: 2px solid transparent !important;
  border-radius: 0 !important;
  cursor: pointer !important;
  transition:
    transform     0.2s cubic-bezier(0.34, 1.56, 0.64, 1),
    box-shadow    0.2s ease,
    border-color  0.15s ease,
    border-radius 0.15s ease,
    background    0.15s ease !important;
}

/* Hover: hoàn toàn không có hiệu ứng gì */
.section-block:hover { background: transparent !important; }

/* ── Active sidebar: viền rgba(255,255,255,0.5) → chìm trên nền tối ─────── */
.section-active--sidebar {
  border: 2px solid rgba(255,255,255,0.5) !important;
  border-radius: 6px !important;
  transform: scale(1.013) !important;
  box-shadow: 0 6px 20px rgba(0,0,0,0.2), 0 1px 4px rgba(0,0,0,0.1) !important;
  background: rgba(255,255,255,0.07) !important;
  z-index: 10 !important;
}

/* ── Active main: viền #dce4cd (màu nền dot) → chìm trên nền trắng ──────── */
.section-active--main {
  border: 2px solid #c8d4b8 !important;
  border-radius: 6px !important;
  transform: scale(1.013) !important;
  box-shadow: 0 4px 18px rgba(70,85,104,0.10), 0 1px 4px rgba(70,85,104,0.06) !important;
  background: rgba(220,228,205,0.10) !important;
  z-index: 10 !important;
}

/* ── Nav buttons ─────────────────────────────────────────────────────────── */
.nav-btns {
  position: absolute !important;
  right: 4px !important;
  top: 4px !important;
  display: flex !important;
  flex-direction: row !important;
  gap: 4px !important;
  z-index: 9999 !important;
}

.nav-btn {
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  padding: 4px !important;
  background: #2563eb !important;
  color: white !important;
  border: none !important;
  border-radius: 4px !important;
  cursor: pointer !important;
  box-shadow: 0 2px 6px rgba(37,99,235,0.4) !important;
  transition: background 0.12s, transform 0.1s !important;
}
.nav-btn:hover  { background: #1d4ed8 !important; }
.nav-btn:active { transform: scale(0.91) !important; }

/* ── HTML content ────────────────────────────────────────────────────────── */
:deep(.html-content)                                   { margin: 0 !important; padding: 0 !important; }
:deep(.html-content p)                                 { margin: 0 !important; padding: 0 !important; }
:deep(.html-content ul)                                { list-style-type: disc !important; padding-left: 1rem !important; margin: 0.2rem 0 !important; }
:deep(.html-content ol)                                { list-style-type: decimal !important; padding-left: 1rem !important; margin: 0.2rem 0 !important; }
:deep(.html-content b), :deep(.html-content strong)    { font-weight: 700 !important; }
:deep(.html-content i), :deep(.html-content em)        { font-style: italic !important; }
:deep(.html-content u)                                 { text-decoration: underline !important; }
:deep(.html-content ul li), :deep(.html-content ol li) { margin-bottom: 0.2rem !important; }

/* ── Print / PDF ─────────────────────────────────────────────────────────── */
@media print {
  .no-print { display: none !important; }
  .section-block,
  .section-active--sidebar,
  .section-active--main {
    cursor: default !important;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
    transform: none !important;
    border-radius: 0 !important;
    outline: none !important;
  }
}

:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .section-active--sidebar),
:global(.is-exporting-pdf .section-active--main) {
  cursor: default !important;
  box-shadow: none !important;
  background: transparent !important;
  border-color: transparent !important;
  transform: none !important;
  border-radius: 0 !important;
  outline: none !important;
}
</style>