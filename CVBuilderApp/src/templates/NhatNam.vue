<template>
  <div id="cv-printable-area" ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-row relative box-border text-[#333] leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', \'Segoe UI\', sans-serif' }">

    <!-- ==================== CỘT TRÁI (SIDEBAR) ==================== -->
    <aside class="w-[72mm] shrink-0 flex flex-col relative z-10" :style="{ backgroundColor: templateSecondaryColor }">

      <!-- HEADER: Tên + Chức danh + Ảnh -->
      <div class="pt-[16mm] px-[8mm] pb-[6mm] flex flex-col items-center text-center paginated-item">
        <h1 class="font-bold leading-tight mb-2"
          :style="{ fontSize: '26px', color: templatePrimaryColor }"
          v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : ''">
        </h1>
        <h2 class="font-medium mb-6 text-[#333]" style="font-size: 13.5px"
          v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : ''">
        </h2>

        <!-- Ảnh đại diện tròn -->
        <div class="relative w-[50mm] h-[50mm] rounded-full overflow-hidden border-[3px] bg-white"
          :style="{ borderColor: templatePrimaryColor }">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl"
            class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center text-gray-400">
            <svg class="w-20 h-20" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1"
                d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
          </div>
        </div>
      </div>

      <!-- THÔNG TIN CÁ NHÂN -->
      <div v-if="contactItems.length > 0" class="px-[8mm] pb-[2mm] relative">
        <h3 class="font-bold mb-5 paginated-item" :style="{ fontSize: '16px', color: templatePrimaryColor, fontWeight: '700' }">
          Thông tin cá nhân
        </h3>
        <div class="flex flex-col gap-3.5 contact-block" style="font-size: 11.5px; color: #333;"
          :class="{ 'contact-active': selectedSectionId === 'contact' }"
          @click.stop="selectedSectionId = selectedSectionId === 'contact' ? null : 'contact'">
          <div v-for="(ci, ciIdx) in contactItems" :key="ci.key"
            class="flex items-start gap-3 paginated-item relative contact-item-container">
            <div class="w-4 h-4 flex items-center justify-center shrink-0" :style="{ color: templatePrimaryColor }">
              <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20" v-html="ci.icon"></svg>
            </div>
            <span class="break-all" v-html="ci.value"></span>
            <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print">
              <button v-if="ciIdx > 0" @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn" title="Di chuyển lên" style="padding:3px">
                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button v-if="ciIdx < contactItems.length - 1" @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn" title="Di chuyển xuống" style="padding:3px">
                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn-danger" title="Ẩn mục này" style="padding:3px">
                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>
          </div>
        </div>
        <!-- Đường kẻ -->
        <div class="w-full border-b mt-6 opacity-30" :style="{ borderColor: templatePrimaryColor }"></div>
      </div>

      <!-- SIDEBAR SECTIONS -->
      <draggable
        v-model="sidebarSectionsWritable"
        item-key="id"
        group="sections"
        class="px-[8mm] flex-1 pb-8 flex flex-col pt-3 cursor-move"
        @end="onDragEnd"
        animation="200"
        ghost-class="opacity-30"
        :delay="100"
        :delayOnTouchOnly="true"
      >
        <template #item="{ element: section }">
          <div
            v-show="section.isVisible"
            :data-section-id="section.id" class="section-block section-block-sidebar relative mb-3 cursor-pointer hover:bg-black/5 transition-colors"
            :class="{ 'section-active': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <!-- Nút điều hướng -->
            <div v-show="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="moveSectionUp(section.id, 'left')" class="nav-btn" title="Di chuyển lên">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionDown(section.id, 'left')" class="nav-btn" title="Di chuyển xuống">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionHorizontal(section.id, 'right')" class="nav-btn" title="Sang Phải">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
              </button>
              <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <!-- Tiêu đề section -->
            <h3 class="font-bold mb-4 paginated-item" :style="{ fontSize: '16px', color: templatePrimaryColor, fontWeight: '700' }">
              <span v-html="section.title"></span>
            </h3>

            <!-- Kỹ năng / Ngôn ngữ / IT -->
            <div v-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'" class="space-y-4">
              <template v-if="section.items && section.items.length > 0">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                  <div class="font-bold text-[#333] leading-snug paginated-item" style="font-size: 12px;">
                    <span v-html="item.name"></span>
                    <span v-if="item.level" class="font-normal opacity-80 ml-1">(<span v-html="item.level"></span>)</span>
                    <span v-if="item.info" class="font-normal opacity-80 block text-[11px] mt-1"><span v-html="item.info"></span></span>
                  </div>
                  <button v-show="selectedSectionId === section.id" @click.stop.prevent="handleRemoveItem(section.id, itemIndex)"
                    class="delete-btn no-print" style="top: -4px; right: -4px;">
                    <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                  </button>
                </div>
              </template>
            </div>

            <!-- Mục tiêu nghề nghiệp (nếu ở cột trái) -->
            <div v-else-if="section.id === 'summary' || section.id === 'objective'" class="space-y-3">
               <div class="leading-[1.7] text-justify html-content font-normal text-[#333] flex flex-col" style="font-size: 11.5px;"
                    v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : '')">
               </div>
            </div>

            <!-- Học vấn (sidebar) -->
            <div v-else-if="section.id === 'education'" class="space-y-4">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative" style="font-size: 11.5px; color: #333;">
                <div class="font-bold mb-1 paginated-item"><span v-html="item.school"></span></div>
                <div v-if="item.year || item.gradType" class="opacity-65 text-[10.5px] mb-1 paginated-item">
                  <span v-if="item.year"><span v-html="item.year"></span></span>
                  <span v-if="item.year && item.gradType"> · </span>
                  <span v-if="item.gradType"><span v-html="item.gradType"></span></span>
                </div>
                <div v-if="item.major" class="opacity-80 text-[11px] paginated-item">Chuyên ngành: <span v-html="item.major"></span></div>
                <div v-if="item.desc" class="html-content leading-relaxed mt-1 flex flex-col" v-html="formatDesc(item.desc)"></div>
                <button v-show="selectedSectionId === section.id" @click.stop.prevent="handleRemoveItem(section.id, itemIndex)"
                  class="delete-btn no-print" style="top: -4px; right: -4px;">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Giải thưởng / Chứng chỉ (sidebar) -->
            <div v-else-if="section.id === 'awards' || section.id === 'certifications'" class="space-y-3">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative" style="font-size: 11.5px; color: #333;">
                <div class="font-bold paginated-item"><span v-html="item.name"></span></div>
                <div v-if="item.year" class="opacity-65 text-[10.5px] paginated-item"><span v-html="item.year"></span></div>
                <button v-show="selectedSectionId === section.id" @click.stop.prevent="handleRemoveItem(section.id, itemIndex)"
                  class="delete-btn no-print" style="top: -4px; right: -4px;">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Còn lại -->
            <div v-else class="space-y-4">
            <div v-for="(item, itemIndex) in section.items" :key="item._refId"
              class="item-container relative" style="font-size: 11.5px; color: #333;">
              <div v-if="item.name || item.company || item.organization"
                class="font-bold mb-0.5">
                <span v-html="item.name || item.company || item.organization"></span>
              </div>
              <div v-if="item.role || item.position || item.major"
                class="opacity-75 italic mb-0.5" style="font-size: 11px;">
                <span v-html="item.role || item.position || item.major"></span>
              </div>
              <div v-if="item.year || item.time || item.date"
                class="opacity-65 mb-1" style="font-size: 10.5px;">
                <span v-html="item.year || item.time || item.date"></span>
              </div>
              <div v-if="item.desc" class="html-content" v-html="formatDesc(item.desc)"></div>
                <button v-show="selectedSectionId === section.id" @click.stop.prevent="handleRemoveItem(section.id, itemIndex)"
                  class="delete-btn no-print" style="top: -4px; right: -4px;">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Đường kẻ dưới section -->
            <div class="w-full border-b opacity-30 mt-3" :style="{ borderColor: templatePrimaryColor }"></div>
          </div>
        </template>
      </draggable>
    </aside>

    <!-- ==================== CỘT PHẢI (MAIN) ==================== -->
    <main class="flex-1 flex flex-col bg-white z-20 relative" @click.self="selectedSectionId = null">
      <div class="px-[10mm] pt-[15mm] pb-[10mm] flex-1 flex flex-col gap-[4mm]">

        <!-- CÁC SECTIONS CHÍNH -->
        <draggable
          v-model="mainSectionsWritable"
          item-key="id"
          group="sections"
          class="flex flex-col cursor-move gap-[4mm]"
          @end="onDragEnd"
          animation="200"
          ghost-class="opacity-30"
          :delay="100"
          :delayOnTouchOnly="true"
        >
          <template #item="{ element: section }">
          <div
            v-show="section.isVisible"
            :data-section-id="section.id" class="section-block section-block-main relative cursor-pointer hover:bg-black/5 transition-colors"
            :class="{ 'section-active': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id">

            <!-- Nút điều hướng -->
            <div v-show="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="moveSectionUp(section.id, 'right')" class="nav-btn" title="Di chuyển lên">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionDown(section.id, 'right')" class="nav-btn" title="Di chuyển xuống">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionHorizontal(section.id, 'left')" class="nav-btn" title="Sang Trái">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
              </button>
              <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <!-- Tiêu đề section -->
            <div class="mb-2 paginated-item">
              <h3 class="font-bold" :style="{ fontSize: '18px', color: templatePrimaryColor, fontWeight: '700' }">
                <span v-html="section.title"></span>
              </h3>
            </div>

            <!-- Kỹ năng / Ngôn ngữ / IT (main) -->
            <div v-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'" class="space-y-4">
              <template v-if="section.items && section.items.length > 0">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                  <div class="font-bold text-[#333] leading-snug paginated-item" style="font-size: 13.5px;">
                    <span v-html="item.name"></span>
                    <span v-if="item.level" class="font-normal opacity-80 ml-1">(<span v-html="item.level"></span>)</span>
                    <span v-if="item.info" class="font-normal opacity-80 block text-[11.5px] mt-1"><span v-html="item.info"></span></span>
                  </div>
                  <button v-show="selectedSectionId === section.id" @click.stop.prevent="handleRemoveItem(section.id, itemIndex)"
                    class="delete-btn no-print" style="top: -4px; right: -4px;">
                    <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                  </button>
                </div>
              </template>
            </div>

            <!-- Mục tiêu nghề nghiệp (main) -->
            <div v-else-if="section.id === 'summary' || section.id === 'objective'" class="space-y-3">
               <div class="leading-[1.7] text-justify html-content font-normal text-[#333] flex flex-col" style="font-size: 12px;"
                    v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : '')">
               </div>
            </div>

            <!-- Học vấn (main) -->
            <div v-else-if="section.id === 'education'" class="space-y-3.5">
              <template v-if="section.items && section.items.length > 0">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                  class="item-container relative">
                  <div class="flex justify-between items-baseline gap-4 mb-1 paginated-item">
                    <h4 class="font-bold text-[#333]" style="font-size: 13.5px;"><span v-html="item.school"></span></h4>
                    <span class="font-normal text-slate-400 shrink-0 text-[11.5px]"><span v-html="item.year"></span></span>
                  </div>
                  <div v-if="item.major" class="text-[#333] font-normal mb-1 paginated-item" style="font-size: 12.5px;">
                    Chuyên ngành: <span v-html="item.major"></span>
                  </div>
                  <div v-if="item.gradType" class="text-slate-500 italic paginated-item" style="font-size: 11.5px;"><span v-html="item.gradType"></span></div>
                  <div v-if="item.desc" class="html-content leading-relaxed mt-1 text-[#333] flex flex-col" style="font-size: 12px;" v-html="formatDesc(item.desc)"></div>
                  <button v-show="selectedSectionId === section.id" @click.stop.prevent="handleRemoveItem(section.id, itemIndex)"
                    class="delete-btn no-print" style="top: -4px; right: -4px;">
                    <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                  </button>
                </div>
              </template>
            </div>

            <!-- Kinh nghiệm / Dự án / Hoạt động (main) -->
            <div v-else-if="section.id === 'experience' || section.id === 'project' || section.id === 'activities'" class="space-y-4">
              <template v-if="section.items && section.items.length > 0">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                  class="item-container relative">
                  <div class="flex justify-between items-baseline gap-4 mb-1 paginated-item">
                    <h4 class="font-bold text-[#333]" style="font-size: 13.5px;">
                      <span v-html="section.id === 'experience' ? item.role : item.name"></span>
                    </h4>
                    <span v-if="item.time" class="font-normal text-slate-400 shrink-0 text-[11.5px]"><span v-html="item.time"></span></span>
                  </div>
                  <div v-if="item.company || (section.id !== 'experience' && item.role)"
                    class="font-normal text-slate-500 mb-2 paginated-item" style="font-size: 12.5px;">
                    <span v-html="section.id === 'experience' ? item.company : item.role"></span>
                  </div>
                  <div class="leading-relaxed text-[#333] html-content flex flex-col" style="font-size: 12px;"
                    v-html="formatDesc(item.desc)">
                  </div>
                  <button v-show="selectedSectionId === section.id" @click.stop.prevent="handleRemoveItem(section.id, itemIndex)"
                    class="delete-btn no-print" style="top: -4px; right: -4px;">
                    <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                  </button>
                </div>
              </template>
            </div>

            <!-- Giải thưởng / Chứng chỉ (main) -->
            <div v-else-if="section.id === 'awards' || section.id === 'certifications'" class="space-y-3">
              <template v-if="section.items && section.items.length > 0">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                  class="item-container relative flex items-start gap-5">
                  <div class="font-bold text-[#333] shrink-0 paginated-item" style="font-size: 12.5px;"><span v-html="item.year"></span></div>
                  <div class="font-normal text-[#333] html-content flex flex-col w-full" style="font-size: 12.5px;" v-html="formatDesc(item.name || item.info)"></div>
                  <button v-show="selectedSectionId === section.id" @click.stop.prevent="handleRemoveItem(section.id, itemIndex)"
                    class="delete-btn no-print" style="top: -4px; right: -4px;">
                    <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                  </button>
                </div>
              </template>
            </div>

            <!-- Mặc định -->
            <div v-else class="space-y-4">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative text-[#333] leading-relaxed" style="font-size: 12px;">
                <div class="html-content flex flex-col" v-html="formatDesc(item.desc || item.info || item.name)"></div>
                <button v-show="selectedSectionId === section.id" @click.stop.prevent="handleRemoveItem(section.id, itemIndex)"
                  class="delete-btn no-print" style="top: -4px; right: -4px;">
                  <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <div class="w-full border-b border-slate-200 mt-3" :style="{ borderColor: templatePrimaryColor, opacity: 0.3 }"></div>
          </div>
        </template>
      </draggable>
      </div>
    </main>

    <!-- ==================== ĐƯỜNG PHÂN TRANG ==================== -->
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
  if (!c || c.toLowerCase() === '#2b5c8f') return '#2cbcd1'
  return c
})

const templateSecondaryColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  if (!c || c.toLowerCase() === '#2b5c8f') return '#e1f5f8'
  return adjustBrightness(c, 0.9)
})

const activeBorderColor = computed(() => {
  const { r, g, b } = hexToRgb(templatePrimaryColor.value)
  return `rgba(${r}, ${g}, ${b}, 0.2)`
})

const activeContactBorderColor = computed(() => {
  const { r, g, b } = hexToRgb(templatePrimaryColor.value)
  return `rgba(${r}, ${g}, ${b}, 0.3)`
})

const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

const cvRoot = ref(null)
const pageCount = ref(1)
const selectedSectionId = ref(null)

const isEmpty = (val) => {
    if (!val) return true
    if (typeof val !== 'string') return false
    const cleanText = val.replace(/<[^>]*>/g, '').trim()
    return cleanText === ''
}

// ─── CONTACT ITEMS: Danh sách động có thể sắp xếp / ẩn ───
const contactIcons = {
  birthDate: '<path d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" />',
  gender: '<path fill-rule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clip-rule="evenodd" />',
  phone: '<path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z" />',
  email: '<path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z" /><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z" />',
  address: '<path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd" />'
}

const contactOrder = ref(['birthDate', 'gender', 'phone', 'email', 'address'])
const hiddenContacts = ref([])

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  switch (key) {
    case 'birthDate': return g.birthDate || ''
    case 'gender': return g.gender || ''
    case 'phone': return g.phone || ''
    case 'email': return g.email || ''
    case 'address': return g.address || ''
    default: return ''
  }
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
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k))
  if (idx <= 0) return
  const keyA = visible[idx], keyB = visible[idx - 1]
  const idxA = contactOrder.value.indexOf(keyA), idxB = contactOrder.value.indexOf(keyB)
  const arr = [...contactOrder.value]
  ;[arr[idxA], arr[idxB]] = [arr[idxB], arr[idxA]]
  contactOrder.value = arr
}

const moveContactDown = (idx) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k))
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

// BỘ LỌC THÔNG MINH: BĂM NHỎ TỪNG DÒNG ĐỂ PHÂN TRANG
const formatDesc = (text) => {
    if (!text) return ''

    // Nếu không chứa thẻ HTML nào -> tự động cắt dòng bằng span
    if (!/<[a-z][\s\S]*>/i.test(text)) {
        return text.split('\n')
                   .map(l => l.trim())
                   .filter(Boolean)
                   .map(l => `<span class="paginated-item block w-full">${l}</span>`)
                   .join('')
    }

    const tempDiv = document.createElement('div')
    tempDiv.innerHTML = text

    // Cứu mã màu của người dùng chọn trong Editor
    tempDiv.querySelectorAll('li').forEach(li => {
        const child = li.firstElementChild
        if (child && (child.tagName === 'FONT' || child.tagName === 'SPAN')) {
            if (child.color) li.style.color = child.color
            if (child.style?.color) li.style.color = child.style.color
        }
    })

    // Đệ quy bọc các text nodes bằng class paginated-item để chống đứt gãy
    const wrapTextNodes = (element) => {
        Array.from(element.childNodes).forEach(node => {
            if (node.nodeType === Node.TEXT_NODE) {
                if (node.textContent.trim()) {
                   const wrapper = document.createElement('span')
                   wrapper.className = 'paginated-item block w-full'
                   node.replaceWith(wrapper)
                   wrapper.appendChild(node)
                }
            } else if (node.nodeType === Node.ELEMENT_NODE) {
                node.classList.remove('paginated-item')
                if (node.tagName === 'BR') {
                   node.outerHTML = '<span class="paginated-item block w-full" style="height: 6px;"></span>'
                } else if (node.tagName === 'LI') {
                   node.classList.add('paginated-item')
                } else {
                   wrapTextNodes(node)
                }
            }
        })
    }

    wrapTextNodes(tempDiv)
    return tempDiv.innerHTML
}

// --- HÀM XỬ LÝ DI CHUYỂN SECTION ---
const moveSectionUp = (id, currentColumn) => {
    emit('moveUp', id, currentColumn);
    if (!props.resumeData?.sections) return;
    const sections = props.resumeData.sections;
    const colSections = sections.filter(s => s.column === currentColumn);
    const idx = colSections.findIndex(s => s.id === id);

    if (idx > 0) {
        const prevId = colSections[idx - 1].id;
        const realIdxCur = sections.findIndex(s => s.id === id);
        const realIdxPrev = sections.findIndex(s => s.id === prevId);
        
        if (realIdxCur !== -1 && realIdxPrev !== -1) {
            const temp = sections.splice(realIdxCur, 1)[0];
            sections.splice(realIdxPrev, 0, temp);
        }
    }
}

const moveSectionDown = (id, currentColumn) => {
    emit('moveDown', id, currentColumn);
    if (!props.resumeData?.sections) return;
    const sections = props.resumeData.sections;
    const colSections = sections.filter(s => s.column === currentColumn);
    const idx = colSections.findIndex(s => s.id === id);

    if (idx !== -1 && idx < colSections.length - 1) {
        const nextId = colSections[idx + 1].id;
        const realIdxCur = sections.findIndex(s => s.id === id);
        const realIdxNext = sections.findIndex(s => s.id === nextId);

        if (realIdxCur !== -1 && realIdxNext !== -1) {
            const temp = sections.splice(realIdxCur, 1)[0];
            sections.splice(realIdxNext, 0, temp);
        }
    }
}

const moveSectionHorizontal = (id, targetColumn) => {
    emit('moveHorizontal', id, targetColumn);
    if (!props.resumeData?.sections) return;
    const sec = props.resumeData.sections.find(s => s.id === id);
    if (sec) {
        sec.column = targetColumn;
    }
}

const handleRemoveItem = (sectionId, itemIndex) => {
    emit('removeItem', sectionId, itemIndex);
    if (!props.resumeData?.sections) return;
    const section = props.resumeData.sections.find(s => s.id === sectionId);
    if (section && section.items && section.items.length > itemIndex) {
        section.items.splice(itemIndex, 1);
    }
}

// --- THUẬT TOÁN PHÂN TRANG (TÍNH THEO SCALE KHÁNG ZOOM) ---
const A4_W_MM = 210;
const A4_H_MM = 297;

let paginateTimer = null;
const requestPagination = () => {
    if (paginateTimer) clearTimeout(paginateTimer);
    paginateTimer = setTimeout(doPagination, 60);
};

const doPagination = async () => {
    if (!cvRoot.value) return;

    // Vô hiệu hóa hiệu ứng Scale để lấy toạ độ thực
    const activeElements = cvRoot.value.querySelectorAll('.section-active, .section-block-main, .section-block-sidebar');
    activeElements.forEach(el => el.style.setProperty('transform', 'none', 'important'));

    // Lọc lấy các node lá ngoài cùng để đo
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
        el.style.setProperty('margin-top', '0px', 'important');
    });

    await nextTick();

    const offsetW = cvRoot.value.offsetWidth;
    const cvRect = cvRoot.value.getBoundingClientRect();
    const scale = offsetW ? cvRect.width / offsetW : 1; 

    const pxPerMm = offsetW / A4_W_MM;
    const pageH = A4_H_MM * pxPerMm;
    
    const bottomSafeZone = 14 * pxPerMm;
    const topMargin = 22 * pxPerMm; // Chống dính sát vạch phân trang

    let stable = false;
    let passes = 0;

    while (!stable && passes < 40) {
        stable = true;
        passes++;
        
        const currentCvRect = cvRoot.value.getBoundingClientRect();

        for (let i = 0; i < allElements.length; i++) {
            const el = allElements[i];
            if (el.offsetHeight === 0) continue;

            const elRect = el.getBoundingClientRect();
            const top = (elRect.top - currentCvRect.top) / scale;
            const height = elRect.height / scale;
            const bottom = top + height;

            const pageIndex = Math.floor(top / pageH);
            const topInPage = top - (pageIndex * pageH);
            const bottomInPage = topInPage + height;

            // Bỏ qua nếu khối quá lớn
            if (height > (pageH - bottomSafeZone - topMargin)) continue;

            if (bottomInPage > (pageH - bottomSafeZone)) {
                const distToNextPage = pageH - topInPage + topMargin;
                const currentMt = parseFloat(el.style.marginTop || '0');
                el.style.setProperty('margin-top', `${currentMt + distToNextPage}px`, 'important');
                stable = false;
                break;
            }
        }
    }

    // Mở lại hiệu ứng Scale
    activeElements.forEach(el => el.style.removeProperty('transform'));

    const finalCvRect = cvRoot.value.getBoundingClientRect();
    let maxBottom = 0;
    allElements.forEach(el => {
        const bottom = (el.getBoundingClientRect().bottom - finalCvRect.top) / scale;
        if (bottom > maxBottom) maxBottom = bottom;
    });

    pageCount.value = Math.max(1, Math.ceil(maxBottom / pageH));
};

watch(() => props.resumeData, () => requestPagination(), { deep: true })

onMounted(() => {
    if (props.resumeData && props.resumeData.sections) {
        const FORCED_RIGHT_IDS = ['summary', 'objective', 'education', 'skills']
        const DEFAULT_LEFT_IDS = ['languages', 'it_skills']

        props.resumeData.sections.forEach(sec => {
            const id = sec.id.toLowerCase()
            if (sec.isVisible === undefined) sec.isVisible = true

            if (FORCED_RIGHT_IDS.some(k => id.includes(k))) {
                sec.column = 'right'
            } else if (!sec.column) {
                if (DEFAULT_LEFT_IDS.some(k => id.includes(k))) {
                    sec.column = 'left'
                } else {
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

const sidebarSections = computed(() => {
    return (props.resumeData?.sections || []).filter(s => s.column === 'left')
})

const mainSections = computed(() => {
    return (props.resumeData?.sections || []).filter(s => s.column === 'right')
})

const sidebarSectionsWritable = ref([])
const mainSectionsWritable = ref([])

watch(sidebarSections, (newVal) => {
  sidebarSectionsWritable.value = [...newVal]
}, { immediate: true, deep: true })

watch(mainSections, (newVal) => {
  mainSectionsWritable.value = [...newVal]
}, { immediate: true, deep: true })

const onDragEnd = () => {
  sidebarSectionsWritable.value.forEach(s => {
    const item = props.resumeData.sections.find(x => x.id === s.id)
    if (item) item.column = 'left'
  })
  mainSectionsWritable.value.forEach(s => {
    const item = props.resumeData.sections.find(x => x.id === s.id)
    if (item) item.column = 'right'
  })

  const newOrderIds = [
    ...sidebarSectionsWritable.value.map(s => s.id),
    ...mainSectionsWritable.value.map(s => s.id)
  ]
  
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
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap');

#cv-printable-area {
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
    overflow-wrap: anywhere;
    font-family: 'Inter', 'Segoe UI', sans-serif;
}

/* Đóng băng hiệu ứng Transition để đo độ chuẩn */
.paginated-item {
  transition: none !important;
}

/* ==================== ITEM CONTAINER ==================== */
.item-container {
    position: relative;
    transition: all 0.2s;
}

/* ==================== DELETE BUTTON ==================== */
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
    z-index: 50;
    transition: all 0.2s;
    box-shadow: 0 2px 4px rgba(0,0,0,0.15);
    cursor: pointer;
    border: none;
}
.delete-btn:hover { transform: scale(1.15); }
.delete-btn:active { transform: scale(0.9); }

/* ==================== HTML CONTENT ==================== */
:deep(.html-content p) { margin-bottom: 0.25rem; }
:deep(.html-content ul) {
    list-style-type: disc !important;
    padding-left: 1.25rem !important;
    margin-top: 0.25rem;
    margin-bottom: 0.25rem;
}
:deep(.html-content ol) {
    list-style-type: decimal !important;
    padding-left: 1.25rem !important;
    margin-top: 0.25rem;
    margin-bottom: 0.25rem;
}
:deep(.html-content b), :deep(.html-content strong) { font-weight: 700; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic; }
:deep(.html-content u) { text-decoration: underline; }
:deep(.html-content ul li), :deep(.html-content ol li) { margin-bottom: 0.25rem; }

/* ==================== SECTION BLOCK ==================== */
.section-block {
    position: relative;
    border-radius: 6px;
    border: 2px solid transparent;
    cursor: pointer;
    transition: all 0.15s ease;
    padding: 6px;
    margin: -6px;
}

.contact-block {
    border-radius: 6px;
    border: 2px solid transparent;
    cursor: pointer;
    transition: border-color 0.18s ease, box-shadow 0.18s ease;
    padding: 4px 2px;
}
.contact-block.contact-active {
    border: 2px solid v-bind(activeContactBorderColor) !important;
    box-shadow: 0 4px 12px rgba(0,0,0,0.06) !important;
    background: rgba(255,255,255,0.4) !important;
    border-radius: 6px !important;
    z-index: 10;
}
.contact-item-container { position: relative; }
.contact-item-btns {
    position: absolute;
    right: -4px;
    top: 50%;
    transform: translateY(-50%);
    display: flex;
    flex-direction: row;
    gap: 3px;
    z-index: 9999;
}

.section-active {
    border: 2px solid var(--border-color, rgba(0,0,0,0.08)) !important;
    border-radius: 6px !important;
    /* removed scale */
    box-shadow: 0 4px 12px rgba(0,0,0,0.06) !important;
    background: rgba(0,0,0,0.02) !important;
    z-index: 10 !important;
}

.section-block-sidebar.section-active {
    --border-color: v-bind(activeBorderColor);
    background: rgba(255,255,255,0.4) !important;
}

.section-block-main.section-active {
    --border-color: v-bind(activeBorderColor);
}

/* ==================== NAV BUTTONS ==================== */
.nav-btns {
    position: absolute;
    right: 10px;
    top: 10px;
    display: flex;
    flex-direction: row;
    gap: 4px;
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
    transition: all 0.15s ease;
}
.nav-btn:hover { background: #1d4ed8; transform: scale(1.1); }
.nav-btn:active { transform: scale(0.95); }

.nav-btn-danger {
    background: #ef4444 !important;
    box-shadow: 0 2px 6px rgba(239, 68, 68, 0.4) !important;
}
.nav-btn-danger:hover {
    background: #dc2626 !important;
}

/* ==================== PRINT ==================== */
@media print {
    .no-print { display: none !important; }
    .section-block,
    .section-active,
    .contact-block,
    .contact-block.contact-active {
        cursor: default !important;
        box-shadow: none !important;
        background: transparent !important;
        border-color: transparent !important;
        transform: none !important;
        border-radius: 0 !important;
        padding: 0 !important;
        margin: 0 !important;
        outline: none !important;
    }
}
:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .section-active),
:global(.is-exporting-pdf .contact-block),
:global(.is-exporting-pdf .contact-block.contact-active) {
    cursor: default !important;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
    transform: none !important;
    border-radius: 0 !important;
    padding: 0 !important;
    margin: 0 !important;
    outline: none !important;
}
</style>
