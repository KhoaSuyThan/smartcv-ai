<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-col relative box-border leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', \'Segoe UI\', sans-serif', color: '#333' }"
    @click.self="selectedSectionId = null"
  >
    <!-- TOP BORDER BAR -->
    <div class="w-full deco-element" :style="{ height: '5px', backgroundColor: templatePrimaryColor, flexShrink: 0 }"></div>

    <!-- HEADER -->
    <header class="relative pt-[12mm] px-[12mm] pb-[6mm] flex items-start justify-between paginated-item">
      <!-- Left: Name + Job Title -->
      <div class="flex-1 pr-4">
        <h1 class="font-black uppercase tracking-tight leading-tight mb-2"
          :style="{ fontSize: '32px !important', color: templatePrimaryColor }"
          v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'NGUYỄN TÙNG DƯƠNG'">
        </h1>
        <h2 class="font-bold uppercase tracking-wider mb-6"
          :style="{ fontSize: '16px !important', color: templatePrimaryColor }"
          v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'NHÂN VIÊN KINH DOANH'">
        </h2>

        <!-- CONTACT GRID -->
        <div v-if="contactItems.length > 0" 
             class="grid grid-cols-3 gap-y-3 gap-x-2 paginated-item contact-block relative"
             :class="{ 'contact-active': selectedSectionId === 'contact' }"
             @click.stop="selectedSectionId = selectedSectionId === 'contact' ? null : 'contact'"
             style="font-size: 11px !important;">
          <div v-for="(ci, ciIdx) in contactItems" :key="ci.key"
               class="flex flex-col pl-3 border-l-[3px] relative contact-item-container py-1"
               :style="{ borderLeftColor: templatePrimaryColor }">
            <span class="font-bold mb-0.5" :style="{ color: templatePrimaryColor, fontSize: '10px !important' }">{{ ci.label }}</span>
            <span class="font-medium text-slate-700 break-words" v-html="ci.value"></span>
            
            <!-- Controls -->
            <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print flex gap-1" style="right: 2px; top: 2px;">
              <!-- Move Up -->
              <button v-if="ciIdx >= 3" @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn" title="Lên" style="width:16px; height:16px; padding:0;">
                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <!-- Move Down -->
              <button v-if="ciIdx + 3 < contactItems.length" @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn" title="Xuống" style="width:16px; height:16px; padding:0;">
                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <!-- Move Left -->
              <button v-if="ciIdx % 3 !== 0" @click.stop.prevent="moveContactLeft(ciIdx)" class="nav-btn" title="Sang trái" style="width:16px; height:16px; padding:0;">
                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
              </button>
              <!-- Move Right -->
              <button v-if="ciIdx % 3 !== 2 && ciIdx + 1 < contactItems.length" @click.stop.prevent="moveContactRight(ciIdx)" class="nav-btn" title="Sang phải" style="width:16px; height:16px; padding:0;">
                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
              </button>
              <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn-danger" title="Xóa" style="width:16px; height:16px; padding:0;">
                <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Right: Avatar -->
      <div class="flex-shrink-0 pt-2">
        <div class="w-[32mm] h-[32mm] rounded-full overflow-hidden border-[1px] border-slate-200 shadow-sm"
          style="background-color: #f8fafc;">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="rounded-full w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center text-slate-300">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
            </svg>
          </div>
        </div>
      </div>
    </header>

    <!-- BODY: 2 COLUMNS -->
    <div class="flex px-[12mm] gap-[10mm] flex-1 pb-[10mm]" @click.self="selectedSectionId = null">

      <!-- LEFT COLUMN (MAIN) -->
      <main class="flex-[1.4] flex flex-col gap-4" @click.self="selectedSectionId = null">
        <draggable
          v-model="mainSectionsWritable"
          item-key="id"
          group="sections"
          class="flex flex-col gap-4 cursor-move"
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
            v-show="section.isVisible"
            :data-section-id="section.id" class="section-block relative cursor-pointer hover:bg-black/5 transition-colors"
            :class="{ 'section-active': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <!-- Nav Buttons -->
            <div v-show="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Lên">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Xuống">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
              </button>
              <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <!-- Section Title -->
            <div class="paginated-item">
              <h3 class="section-title"><span v-html="section.title"></span></h3>
            </div>

            <!-- Summary -->
            <div v-if="section.id === 'summary'"
              class="text-justify leading-relaxed html-content pr-2"
              style="font-size: 11.5px !important; color: #333;"
              v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Mô tả mục tiêu nghề nghiệp của bạn...')">
            </div>

            <!-- Education -->
            <div v-else-if="section.id === 'education'" class="space-y-4">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                <div class="paginated-item pr-2">
                  <div class="flex justify-between items-start mb-1">
                    <span class="font-bold" :style="{ color: templatePrimaryColor, fontSize: '13.5px !important' }"><span v-html="item.school || 'Tên trường'"></span></span>
                    <span class="font-bold px-3 py-1 rounded-full text-[10px] shrink-0 ml-4 border border-[#e2e8f0]" :style="{ backgroundColor: templateSecondaryColor, color: templatePrimaryColor }"><span v-html="item.year || '2017-2021'"></span></span>
                  </div>
                  <div class="font-bold text-slate-700" style="font-size: 12px !important;"><span v-html="item.major || 'Chuyên ngành'"></span></div>
                  <div class="flex gap-2 text-slate-500 mt-1" style="font-size: 11px !important;" v-if="item.gradType || item.gpa">
                    <span v-if="item.gradType">Xếp loại: <strong><span v-html="item.gradType"></span></strong></span>
                    <span v-if="item.gradType && item.gpa"> | </span>
                    <span v-if="item.gpa">GPA: <strong><span v-html="item.gpa"></span></strong></span>
                  </div>
                  <div v-if="item.desc" class="html-content mt-1.5 text-slate-600 leading-relaxed" style="font-size: 11px !important;" v-html="formatDesc(item.desc)"></div>
                </div>
              </div>
            </div>

            <!-- Experience, Projects, Activities -->
            <div v-else-if="['experience', 'projects', 'project', 'activities', 'activity'].some(x => section.id.toLowerCase().includes(x))" class="space-y-5">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                <div class="paginated-item pr-2">
                  <div class="flex justify-between items-start mb-1">
                    <span class="font-bold uppercase" :style="{ color: templatePrimaryColor, fontSize: '13.5px !important' }">
                      <span v-html="item.company || item.organization || item.name || item.title || 'Tên đơn vị'"></span>
                    </span>
                    <span class="font-bold px-3 py-1 rounded-full text-[10px] shrink-0 ml-4 border border-[#e2e8f0]" :style="{ backgroundColor: templateSecondaryColor, color: templatePrimaryColor }">
                      <span v-html="item.time || item.year || 'Thời gian'"></span>
                    </span>
                  </div>
                  <div class="font-bold text-slate-700 mb-2" style="font-size: 12px !important;">
                    <span v-html="item.role || item.position || ''"></span>
                    <span v-if="(item.company || item.organization) && (item.name || item.title) && (item.name !== item.company) && (item.title !== item.organization)" class="text-slate-500 italic font-normal ml-2" style="font-size: 11px !important;">
                      - Dự án: <span v-html="item.name || item.title"></span>
                    </span>
                  </div>
                  <div class="html-content leading-relaxed text-slate-600" style="font-size: 11.5px !important;" v-html="formatDesc(item.desc || item.info || '')"></div>
                </div>
              </div>
            </div>

            <!-- Skills, IT Skills, Languages -->
            <div v-else-if="['skills', 'it_skills', 'languages'].some(x => section.id.toLowerCase().includes(x))" class="space-y-3">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                <div class="paginated-item pr-2">
                  <div class="font-bold text-slate-700 flex justify-between items-baseline" style="font-size: 12.5px !important;">
                    <span v-html="item.name || 'Tên kỹ năng'"></span>
                    <span v-if="item.level || item.info" class="font-semibold text-slate-500" style="font-size: 11.5px !important;">
                      <span v-html="item.level || item.info"></span>
                    </span>
                  </div>
                  <div v-if="item.desc" class="html-content mt-1 text-slate-600 leading-relaxed" style="font-size: 11px !important;" v-html="formatDesc(item.desc)"></div>
                </div>
              </div>
            </div>

            <!-- Fallback -->
            <div v-else class="space-y-3">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container relative">
                <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                <div class="html-content text-slate-700 pr-2" style="font-size: 11.5px !important;" v-html="formatDesc(item.desc || item.info || item.name)"></div>
              </div>
            </div>
          </div>
          </template>
        </draggable>
      </main>

      <!-- RIGHT COLUMN (SIDEBAR) -->
      <aside class="flex-[1] flex flex-col gap-4" @click.self="selectedSectionId = null">
        <draggable
          v-model="sidebarSectionsWritable"
          item-key="id"
          group="sections"
          class="flex flex-col gap-4 cursor-move"
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
            v-show="section.isVisible"
            :data-section-id="section.id" class="section-block relative cursor-pointer hover:bg-black/5 transition-colors"
            :class="{ 'section-active': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <!-- Nav Buttons -->
            <div v-show="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Lên">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Xuống">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
              </button>
              <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <!-- Section Title -->
            <div class="paginated-item">
              <h3 class="section-title"><span v-html="section.title"></span></h3>
            </div>

            <!-- Awards / Certifications -->
            <div v-if="['awards','certifications'].includes(section.id)" class="space-y-4">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container relative">
                <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                <div v-if="item.year" class="font-bold mb-0.5" style="font-size: 11.5px !important; color: #333;"><span v-html="item.year"></span>:</div>
                <div class="font-medium text-slate-700 leading-relaxed" style="font-size: 11px !important;">
                  <span v-html="item.name || item.info"></span>
                  <span v-if="item.organization" class="text-slate-500 italic"> - <span v-html="item.organization"></span></span>
                </div>
              </div>
            </div>

            <!-- Skills, IT Skills, Languages -->
            <div v-else-if="['skills', 'it_skills', 'languages'].some(x => section.id.toLowerCase().includes(x))" class="space-y-3">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                <div class="paginated-item pr-2">
                  <div class="font-bold text-slate-700 flex justify-between items-baseline" style="font-size: 11.5px !important;">
                    <span v-html="item.name || 'Tên kỹ năng'"></span>
                    <span v-if="item.level || item.info" class="font-semibold text-slate-500" style="font-size: 10.5px !important;">
                      <span v-html="item.level || item.info"></span>
                    </span>
                  </div>
                  <div v-if="item.desc" class="html-content mt-1 text-slate-600 leading-relaxed" style="font-size: 10.5px !important;" v-html="formatDesc(item.desc)"></div>
                </div>
              </div>
            </div>

            <!-- Education, Experience, Projects, Activities (if dragged to sidebar) -->
            <div v-else-if="['education', 'experience', 'projects', 'project', 'activities', 'activity'].some(x => section.id.toLowerCase().includes(x))" class="space-y-4">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                <div class="paginated-item pr-2">
                  <!-- Time/Year -->
                  <div v-if="item.time || item.year" class="font-bold mb-0.5" :style="{ color: templatePrimaryColor, fontSize: '11px !important' }">
                    <span v-html="item.time || item.year"></span>
                  </div>
                  <!-- Title / Major / Project / Activity Name -->
                  <div class="font-bold text-slate-800" style="font-size: 11.5px !important;">
                    <span v-html="item.major || item.role || item.position || item.name || item.title || ''"></span>
                  </div>
                  <!-- School / Company / Organization -->
                  <div v-if="item.school || item.company || item.organization" class="font-medium text-slate-600" style="font-size: 11px !important;">
                    <span v-html="item.school || item.company || item.organization"></span>
                  </div>
                  <!-- Education GradType / GPA -->
                  <div class="flex gap-2 text-slate-500 mt-0.5" style="font-size: 10.5px !important;" v-if="item.gradType || item.gpa">
                    <span v-if="item.gradType">Loại: <span v-html="item.gradType"></span></span>
                    <span v-if="item.gradType && item.gpa"> | </span>
                    <span v-if="item.gpa">GPA: <span v-html="item.gpa"></span></span>
                  </div>
                  <!-- Description -->
                  <div v-if="item.desc || item.info" class="html-content mt-1 text-slate-500 leading-relaxed" style="font-size: 10.5px !important;" v-html="formatDesc(item.desc || item.info)"></div>
                </div>
              </div>
            </div>

            <!-- Fallback sidebar -->
            <div v-else class="space-y-3">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container relative">
                <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                <div class="html-content text-slate-700" style="font-size: 11px !important;" v-html="formatDesc(item.desc || item.info || item.name)"></div>
              </div>
            </div>
          </div>
          </template>
        </draggable>
      </aside>
    </div>

    <!-- PAGE BREAK INDICATORS -->
    <template v-for="p in (pageCount - 1)" :key="'div-' + p">
      <div class="absolute left-0 w-full z-50 flex flex-col items-center justify-center pointer-events-none no-print"
        :style="{ top: `calc(${p * 297}mm - 8px)` }">
        <div class="w-[105%] h-[16px] bg-slate-800/95 shadow-inner border-y border-black/30"></div>
        <span class="absolute text-[9px] uppercase font-bold text-slate-300 tracking-widest bg-slate-700 px-3 py-0.5 rounded border border-slate-600">Ngắt trang {{ p + 1 }}</span>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, ref, onMounted, nextTick, watch, onUnmounted } from 'vue'
import draggable from 'vuedraggable'

const props = defineProps({
  resumeData: { type: Object, required: true }
})

// --- COLOR CUSTOMIZATION ---
const hexToRgb = (hex) => {
  hex = hex.replace(/^#/, '')
  if (hex.length === 3) {
    hex = hex.split('').map(c => c + c).join('')
  }
  const num = parseInt(hex, 16)
  return {
    r: (num >> 16) & 255,
    g: (num >> 8) & 255,
    b: num & 255
  }
}

const rgbToHex = (r, g, b) => {
  return '#' + [r, g, b].map(x => {
    const hex = Math.max(0, Math.min(255, Math.round(x))).toString(16)
    return hex.length === 1 ? '0' + hex : hex
  }).join('')
}

const adjustBrightness = (hex, percent) => {
  try {
    const { r, g, b } = hexToRgb(hex)
    if (percent < 0) {
      const factor = 1 + percent
      return rgbToHex(r * factor, g * factor, b * factor)
    } else {
      return rgbToHex(
        r + (255 - r) * percent,
        g + (255 - g) * percent,
        b + (255 - b) * percent
      )
    }
  } catch (e) {
    return hex
  }
}

const templatePrimaryColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  if (!c || c.toLowerCase() === '#2b5c8f') return '#1a73e8'
  return c
})

const templateSecondaryColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  if (!c || c.toLowerCase() === '#2b5c8f') return '#f1f5f9'
  return adjustBrightness(c, 0.95)
})

const hoverPrimaryColor = computed(() => {
  return adjustBrightness(templatePrimaryColor.value, -0.2)
})

const activeBorderColor = computed(() => {
  const { r, g, b } = hexToRgb(templatePrimaryColor.value)
  return `rgba(${r}, ${g}, ${b}, 0.4)`
})

const activeBgColor = computed(() => {
  const { r, g, b } = hexToRgb(templatePrimaryColor.value)
  return `rgba(${r}, ${g}, ${b}, 0.05)`
})
const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

const cvRoot = ref(null)
const pageCount = ref(1)
const selectedSectionId = ref(null)

const getContactLabel = (key) => {
  switch (key) {
    case 'birthDate': return 'Ngày sinh'
    case 'phone': return 'Điện thoại'
    case 'email': return 'Email'
    case 'address': return 'Địa chỉ'
    case 'website': return 'Website'
    default: return ''
  }
}

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  switch (key) {
    case 'birthDate': return isEmpty(g.birthDate) ? '' : g.birthDate
    case 'phone': return isEmpty(g.phone) ? '' : g.phone
    case 'email': return isEmpty(g.email) ? '' : g.email
    case 'address': return isEmpty(g.address) ? '' : g.address
    case 'website': return g.website || g.github || g.linkedin || ''
    default: return ''
  }
}

const contactOrder = ref(['birthDate', 'email', 'phone', 'website', 'address'])
const hiddenContacts = ref([])

const contactItems = computed(() => {
  return contactOrder.value
    .filter(key => !hiddenContacts.value.includes(key))
    .filter(key => !isEmpty(getContactValue(key)))
    .map(key => ({
      key,
      label: getContactLabel(key),
      value: getContactValue(key)
    }))
})

const moveContactUp = (idx) => {
  if (idx < 3) return
  swapContact(idx, idx - 3)
}

const moveContactDown = (idx) => {
  if (idx + 3 >= contactItems.value.length) return
  swapContact(idx, idx + 3)
}

const moveContactLeft = (idx) => {
  if (idx % 3 === 0) return
  swapContact(idx, idx - 1)
}

const moveContactRight = (idx) => {
  if (idx % 3 === 2 || idx + 1 >= contactItems.value.length) return
  swapContact(idx, idx + 1)
}

const swapContact = (idxA, idxB) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && !isEmpty(getContactValue(k)))
  const keyA = visible[idxA], keyB = visible[idxB]
  const realIdxA = contactOrder.value.indexOf(keyA)
  const realIdxB = contactOrder.value.indexOf(keyB)
  const arr = [...contactOrder.value]
  ;[arr[realIdxA], arr[realIdxB]] = [arr[realIdxB], arr[realIdxA]]
  contactOrder.value = arr
}

const removeContactItem = (idx) => {
  const visible = contactItems.value
  if (idx >= 0 && idx < visible.length) {
    const key = visible[idx].key
    hiddenContacts.value.push(key)
    requestPagination()
  }
}

const isEmpty = (val) => {
  if (!val) return true
  if (typeof val !== 'string') return false
  return val.replace(/<[^>]*>/g, '').trim() === ''
}

const sidebarSections = computed(() => props.resumeData.sections.filter(s => s.column === 'right'))
const mainSections = computed(() => props.resumeData.sections.filter(s => s.column === 'left'))

const sidebarSectionsWritable = ref([])
const mainSectionsWritable = ref([])

watch(sidebarSections, (newVal) => {
  sidebarSectionsWritable.value = [...newVal]
}, { immediate: true, deep: true })

watch(mainSections, (newVal) => {
  mainSectionsWritable.value = [...newVal]
}, { immediate: true, deep: true })

const onDragEnd = () => {
  mainSectionsWritable.value.forEach(s => {
    const item = props.resumeData.sections.find(x => x.id === s.id)
    if (item) item.column = 'left'
  })
  sidebarSectionsWritable.value.forEach(s => {
    const item = props.resumeData.sections.find(x => x.id === s.id)
    if (item) item.column = 'right'
  })

  const leftIds = mainSectionsWritable.value.map(s => s.id)
  const rightIds = sidebarSectionsWritable.value.map(s => s.id)
  
  const newSections = []
  props.resumeData.sections.forEach(s => {
    if (!leftIds.includes(s.id) && !rightIds.includes(s.id)) {
      newSections.push(s)
    }
  })
  
  leftIds.forEach(id => {
    const item = props.resumeData.sections.find(s => s.id === id)
    if (item) newSections.push(item)
  })
  
  rightIds.forEach(id => {
    const item = props.resumeData.sections.find(s => s.id === id)
    if (item) newSections.push(item)
  })

  props.resumeData.sections.splice(0, props.resumeData.sections.length, ...newSections)
  requestPagination()
}

const sidebarIds = computed(() => sidebarSections.value.map(s => s.id))
const mainIds = computed(() => mainSections.value.map(s => s.id))

const formatDesc = (text) => {
  if (!text) return ''
  
  if (!/<[a-z][\s\S]*>/i.test(text)) {
    return text.split('\n')
               .map(l => l.trim())
               .filter(Boolean)
               .map(l => `<div class="paginated-item w-full block">${l}</div>`)
               .join('')
  }

  const tempDiv = document.createElement('div')
  tempDiv.innerHTML = text

  // Cứu mã màu của người dùng set trong Editor
  tempDiv.querySelectorAll('li').forEach(li => {
    const child = li.firstElementChild
    if (child && (child.tagName === 'FONT' || child.tagName === 'SPAN')) {
      if (child.color) li.style.color = child.color
      if (child.style?.color) li.style.color = child.style.color
    }
  })

  // Định nghĩa các thẻ bao khối (Block)
  const blockTags = ['P', 'DIV', 'LI', 'H1', 'H2', 'H3', 'H4', 'H5', 'H6']
  
  // Quét toàn bộ HTML, CHỈ gắn class 'paginated-item' cho các Block KHÔNG chứa Block con (Leaf-Blocks)
  tempDiv.querySelectorAll('*').forEach(el => {
    if (blockTags.includes(el.tagName.toUpperCase())) {
      const hasBlockChild = Array.from(el.children).some(child => blockTags.includes(child.tagName.toUpperCase()))
      if (!hasBlockChild) {
        el.classList.add('paginated-item')
      }
    }
  })

  // Tự động bọc các dòng Text đứng bơ vơ bên ngoài
  Array.from(tempDiv.childNodes).forEach(node => {
    if (node.nodeType === Node.TEXT_NODE && node.textContent.trim()) {
       const wrapper = document.createElement('div')
       wrapper.className = 'paginated-item w-full block'
       node.replaceWith(wrapper)
       wrapper.appendChild(node)
    }
  })

  return tempDiv.innerHTML
}

let paginateTimer = null
const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 60)
}

const A4_W_MM = 210
const A4_H_MM = 297

const doPagination = async () => {
  if (!cvRoot.value) return

  // BỘ LỌC THÔNG MINH: Ngăn chặn lỗi lồng margin, chỉ tính các lớp con ngoài cùng
  const allElements = Array.from(cvRoot.value.querySelectorAll('.paginated-item')).filter(el => {
    if (el.offsetHeight === 0) return false;
    let parent = el.parentElement;
    while (parent && parent !== cvRoot.value) {
      if (parent.classList.contains('paginated-item')) return false;
      parent = parent.parentElement;
    }
    return true;
  });

  allElements.forEach(el => {
    el.style.setProperty('margin-top', '0px', 'important')
  })
  await nextTick()

  const cvRect = cvRoot.value.getBoundingClientRect()
  const pxPerMm = cvRect.width / A4_W_MM
  const pageH = A4_H_MM * pxPerMm
  
  // Thiết lập vùng an toàn 14mm cách đáy
  const bottomSafeZone = 14 * pxPerMm
  const topMargin = 15 * pxPerMm

  let stable = false
  let passes = 0

  while (!stable && passes < 30) {
    stable = true
    passes++
    
    // Tự động tính toán lại chiều cao mỗi khi có dòng nhảy trang
    const currentCvRect = cvRoot.value.getBoundingClientRect()

    for (let i = 0; i < allElements.length; i++) {
      const el = allElements[i]
      if (el.offsetHeight === 0) continue

      const elRect = el.getBoundingClientRect()
      const top = elRect.top - currentCvRect.top
      const height = elRect.height
      const bottom = top + height

      const pageIndex = Math.floor(top / pageH)
      const topInPage = top - (pageIndex * pageH)
      const bottomInPage = topInPage + height

      // Bảo vệ vòng lặp nếu có khối bất thường
      if (height > (pageH - bottomSafeZone - topMargin)) continue

      // Nếu dòng chữ đụng vào vạch đen -> Đẩy xuống!
      if (bottomInPage > (pageH - bottomSafeZone)) {
         const distToNextPage = pageH - topInPage + topMargin
         const currentMt = parseFloat(el.style.marginTop || '0')
         el.style.setProperty('margin-top', `${currentMt + distToNextPage}px`, 'important')
         stable = false
         break
      }
    }
  }

  // Chốt độ dài thực tế của CV
  const finalCvRect = cvRoot.value.getBoundingClientRect()
  let maxBottom = 0
  allElements.forEach(el => {
    const rect = el.getBoundingClientRect()
    const bottom = rect.bottom - finalCvRect.top
    if (bottom > maxBottom) maxBottom = bottom
  })

  pageCount.value = Math.max(1, Math.ceil(maxBottom / pageH))
}

watch(() => props.resumeData, requestPagination, { deep: true })

onMounted(() => {
  if (props.resumeData?.sections) {
    const LEFT = ['summary', 'education', 'experience']
    const RIGHT = ['awards', 'certifications']
    const ALL_REQUIRED = [...LEFT, ...RIGHT]
    
    props.resumeData.sections.forEach(sec => {
      sec.isVisible = ALL_REQUIRED.includes(sec.id)
      if (LEFT.includes(sec.id)) {
        sec.column = 'left'
      } else if (RIGHT.includes(sec.id)) {
        sec.column = 'right'
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

.section-title {
  font-size: 15px !important;
  font-weight: 800 !important;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: v-bind(templatePrimaryColor) !important;
  border-bottom: 2px solid v-bind(templatePrimaryColor);
  padding-bottom: 6px;
  margin-bottom: 10px;
}

.section-block {
  position: relative;
  padding: 6px;
  border-radius: 4px;
  border: 1px solid transparent;
  cursor: pointer;
  transition: all 0.15s ease;
}

.section-active {
  border-color: v-bind(activeBorderColor) !important;
  background: v-bind(activeBgColor) !important;
}

.contact-block {
  padding: 6px;
  border-radius: 4px;
  border: 1px solid transparent;
  cursor: pointer;
}

.contact-block.contact-active {
  border: 1px solid v-bind(activeBorderColor) !important;
  background: v-bind(activeBgColor) !important;
}

.contact-item-btns {
  position: absolute;
  z-index: 10;
}

.nav-btns {
  position: absolute;
  right: 6px;
  top: 6px;
  display: flex;
  flex-direction: row;
  gap: 4px;
  z-index: 99;
}

.nav-btn {
  width: 22px;
  height: 22px;
  background: v-bind(templatePrimaryColor);
  color: white;
  border: none;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}
.nav-btn:hover { background: v-bind(hoverPrimaryColor); }
.nav-btn-danger { background: #ef4444 !important; }

.delete-btn {
  position: absolute;
  top: -5px;
  right: -5px;
  background: #ef4444;
  color: white;
  border: none;
  border-radius: 50%;
  width: 18px;
  height: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 30;
}

.paginated-item {
  transition: none !important; 
}

/* NGĂN CHẶN SỤP LỀ CSS - GIỮ PHÉP ĐO JAVASCRIPT CHUẨN XÁC */
:deep(.html-content)                                   { margin: 0 !important; padding: 0 !important; }
:deep(.html-content p)                                 { margin-bottom: 4px !important; padding: 0 !important; }
:deep(.html-content ul)                                { list-style-type: disc !important; padding-left: 1.2rem !important; margin: 0.1rem 0 !important; }
:deep(.html-content ol)                                { list-style-type: decimal !important; padding-left: 1.2rem !important; margin: 0.1rem 0 !important; }
:deep(.html-content li)                                { margin-bottom: 0.15rem !important; }
:deep(.html-content b), :deep(.html-content strong)    { font-weight: 700 !important; }
:deep(.html-content i), :deep(.html-content em)        { font-style: italic !important; }
:deep(.html-content u)                                 { text-decoration: underline !important; }

:deep(.html-content li:has(> font[size="1"])) { font-size: 10px !important; }
:deep(.html-content li:has(> font[size="2"])) { font-size: 13px !important; }
:deep(.html-content li:has(> font[size="3"])) { font-size: 16px !important; }

@media print {
  .no-print { display: none !important; }
  .section-block, .contact-block {
    cursor: default !important;
    box-shadow: none !important;
    background: transparent !important;
    border: none !important;
    outline: none !important;
    border-radius: 0 !important;
    padding: 6px !important;
  }
  .nav-btns, .delete-btn { display: none !important; }
}

:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .contact-block),
:global(.is-exporting-pdf .section-active),
:global(.is-exporting-pdf .contact-active) {
  cursor: default !important;
  box-shadow: none !important;
  background: transparent !important;
  border: none !important;
  outline: none !important;
  border-radius: 0 !important;
  padding: 6px !important;
}
</style>
