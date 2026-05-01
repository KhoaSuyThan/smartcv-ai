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
          v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'Nguyễn Văn A'">
        </h1>
        <h2 class="font-medium mb-6 text-[#333]" style="font-size: 13.5px"
          v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'Lập trình viên'">
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
      <div class="px-[8mm] pb-[2mm] paginated-item relative">
        <h3 class="font-bold mb-5" :style="{ fontSize: '16px', color: templatePrimaryColor, fontWeight: '700' }">
          Thông tin cá nhân
        </h3>
        <div class="flex flex-col gap-3.5" style="font-size: 11.5px; color: #333;">
          <!-- Năm sinh -->
          <div class="flex items-start gap-3">
            <div class="w-4 h-4 flex items-center justify-center shrink-0" :style="{ color: templatePrimaryColor }">
              <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                <path d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" />
              </svg>
            </div>
            <span v-html="!isEmpty(resumeData.general.birthDate) ? resumeData.general.birthDate : '1996'"></span>
          </div>
          <!-- Giới tính -->
          <div class="flex items-start gap-3">
            <div class="w-4 h-4 flex items-center justify-center shrink-0" :style="{ color: templatePrimaryColor }">
              <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clip-rule="evenodd" />
              </svg>
            </div>
            <span v-html="!isEmpty(resumeData.general.gender) ? resumeData.general.gender : 'Nam'"></span>
          </div>
          <!-- SĐT -->
          <div class="flex items-start gap-3">
            <div class="w-4 h-4 flex items-center justify-center shrink-0" :style="{ color: templatePrimaryColor }">
              <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                <path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z" />
              </svg>
            </div>
            <span class="break-all" v-html="!isEmpty(resumeData.general.phone) ? resumeData.general.phone : '0123 456 789'"></span>
          </div>
          <!-- Email -->
          <div class="flex items-start gap-3">
            <div class="w-4 h-4 flex items-center justify-center shrink-0" :style="{ color: templatePrimaryColor }">
              <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                <path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z" />
                <path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z" />
              </svg>
            </div>
            <span class="break-all" v-html="!isEmpty(resumeData.general.email) ? resumeData.general.email : 'nguyenvana@gmail.com'"></span>
          </div>
          <!-- Địa chỉ -->
          <div class="flex items-start gap-3">
            <div class="w-4 h-4 flex items-center justify-center shrink-0" :style="{ color: templatePrimaryColor }">
              <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd" />
              </svg>
            </div>
            <span class="break-words" v-html="!isEmpty(resumeData.general.address) ? resumeData.general.address : 'Thanh Xuân, Hà Nội'"></span>
          </div>
        </div>
        <!-- Đường kẻ -->
        <div class="w-full border-b mt-6 opacity-30" :style="{ borderColor: templatePrimaryColor }"></div>
      </div>

      <!-- SIDEBAR SECTIONS -->
      <div class="px-[8mm] flex-1 pb-8 flex flex-col pt-3">
        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block section-block-sidebar relative mb-5"
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
            </div>

            <!-- Tiêu đề section -->
            <h3 class="font-bold mb-4" :style="{ fontSize: '16px', color: templatePrimaryColor, fontWeight: '700' }">
              {{ section.title }}
            </h3>

            <!-- Kỹ năng / Ngôn ngữ / IT -->
            <div v-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'" class="space-y-4">
              <template v-if="section.items && section.items.length > 0">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                  <div class="font-bold text-[#333] leading-snug" style="font-size: 12px;">
                    {{ item.name }}
                    <span v-if="item.level" class="font-normal opacity-80 ml-1">({{ item.level }})</span>
                    <span v-if="item.info" class="font-normal opacity-80 block text-[11px] mt-1">{{ item.info }}</span>
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
               <div class="leading-[1.7] text-justify html-content font-normal text-[#333]" style="font-size: 11.5px;"
                    v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : ''">
               </div>
            </div>

            <!-- Học vấn (sidebar) -->
            <div v-else-if="section.id === 'education'" class="space-y-4">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative" style="font-size: 11.5px; color: #333;">
                <div class="font-bold mb-1">{{ item.school }}</div>
                <div v-if="item.year || item.gradType" class="opacity-65 text-[10.5px] mb-1">
                  <span v-if="item.year">{{ item.year }}</span>
                  <span v-if="item.year && item.gradType"> · </span>
                  <span v-if="item.gradType">{{ item.gradType }}</span>
                </div>
                <div v-if="item.major" class="opacity-80 text-[11px]">Chuyên ngành: {{ item.major }}</div>
                <div v-if="item.desc" class="html-content leading-relaxed mt-1" v-html="formatDesc(item.desc)"></div>
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
                <div class="font-bold">{{ item.name }}</div>
                <div v-if="item.year" class="opacity-65 text-[10.5px]">{{ item.year }}</div>
                <button v-show="selectedSectionId === section.id" @click.stop.prevent="handleRemoveItem(section.id, itemIndex)"
                  class="delete-btn no-print" style="top: -4px; right: -4px;">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Còn lại -->
            <div v-else class="space-y-3">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative" style="font-size: 11.5px; color: #333;">
                <div class="html-content" v-html="formatDesc(item.desc || item.name || item.info)"></div>
                <button v-show="selectedSectionId === section.id" @click.stop.prevent="handleRemoveItem(section.id, itemIndex)"
                  class="delete-btn no-print" style="top: -4px; right: -4px;">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Đường kẻ dưới section -->
            <div class="w-full border-b opacity-30 mt-6" :style="{ borderColor: templatePrimaryColor }"></div>
          </div>
        </template>
      </div>
    </aside>

    <!-- ==================== CỘT PHẢI (MAIN) ==================== -->
    <main class="flex-1 flex flex-col bg-white z-20 relative" @click.self="selectedSectionId = null">
      <div class="px-[10mm] pt-[15mm] pb-[10mm] flex-1 flex flex-col gap-[8mm]">

        <!-- CÁC SECTIONS CHÍNH -->
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block section-block-main paginated-item relative"
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
            </div>

            <!-- Tiêu đề section -->
            <div class="mb-4">
              <h3 class="font-bold" :style="{ fontSize: '18px', color: templatePrimaryColor, fontWeight: '700' }">
                {{ section.title }}
              </h3>
            </div>

            <!-- Kỹ năng / Ngôn ngữ / IT (main) -->
            <div v-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'" class="space-y-4">
              <template v-if="section.items && section.items.length > 0">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                  <div class="font-bold text-[#333] leading-snug" style="font-size: 13.5px;">
                    {{ item.name }}
                    <span v-if="item.level" class="font-normal opacity-80 ml-1">({{ item.level }})</span>
                    <span v-if="item.info" class="font-normal opacity-80 block text-[11.5px] mt-1">{{ item.info }}</span>
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
               <div class="leading-[1.7] text-justify html-content font-normal text-[#333]" style="font-size: 12px;"
                    v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : ''">
               </div>
            </div>

            <!-- Học vấn (main) -->
            <div v-else-if="section.id === 'education'" class="space-y-5">
              <template v-if="section.items && section.items.length > 0">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                  class="item-container relative">
                  <div class="flex justify-between items-baseline gap-4 mb-1">
                    <h4 class="font-bold text-[#333]" style="font-size: 13.5px;">{{ item.school }}</h4>
                    <span class="font-normal text-slate-400 shrink-0 text-[11.5px]">{{ item.year }}</span>
                  </div>
                  <div v-if="item.major" class="text-[#333] font-normal mb-1" style="font-size: 12.5px;">
                    Chuyên ngành: {{ item.major }}
                  </div>
                  <div v-if="item.gradType" class="text-slate-500 italic" style="font-size: 11.5px;">{{ item.gradType }}</div>
                  <div v-if="item.desc" class="html-content leading-relaxed mt-1 text-[#333]" style="font-size: 12px;" v-html="formatDesc(item.desc)"></div>
                  <button v-show="selectedSectionId === section.id" @click.stop.prevent="handleRemoveItem(section.id, itemIndex)"
                    class="delete-btn no-print" style="top: -4px; right: -4px;">
                    <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                  </button>
                </div>
              </template>
            </div>

            <!-- Kinh nghiệm / Dự án / Hoạt động (main) -->
            <div v-else-if="section.id === 'experience' || section.id === 'project' || section.id === 'activities'" class="space-y-6">
              <template v-if="section.items && section.items.length > 0">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                  class="item-container relative">
                  <div class="flex justify-between items-baseline gap-4 mb-1">
                    <h4 class="font-bold text-[#333]" style="font-size: 13.5px;">
                      {{ section.id === 'experience' ? item.role : item.name }}
                    </h4>
                    <span v-if="item.time" class="font-normal text-slate-400 shrink-0 text-[11.5px]">{{ item.time }}</span>
                  </div>
                  <div v-if="item.company || (section.id !== 'experience' && item.role)"
                    class="font-normal text-slate-500 mb-2" style="font-size: 12.5px;">
                    {{ section.id === 'experience' ? item.company : item.role }}
                  </div>
                  <div class="leading-relaxed text-[#333] html-content" style="font-size: 12px;"
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
                  <span class="font-bold text-[#333] shrink-0" style="font-size: 12.5px;">{{ item.year }}</span>
                  <span class="font-normal text-[#333] html-content" style="font-size: 12.5px;" v-html="item.name || item.info"></span>
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
                <div class="html-content" v-html="formatDesc(item.desc || item.info || item.name)"></div>
                <button v-show="selectedSectionId === section.id" @click.stop.prevent="handleRemoveItem(section.id, itemIndex)"
                  class="delete-btn no-print" style="top: -4px; right: -4px;">
                  <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <div class="w-full border-b border-slate-200 mt-6" :style="{ borderColor: templatePrimaryColor, opacity: 0.3 }"></div>
          </div>
        </template>
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

const props = defineProps({
    resumeData: { type: Object, required: true },
    templatePrimaryColor: { type: String, default: '#2cbcd1' },
    templateSecondaryColor: { type: String, default: '#e1f5f8' }
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

// --- HÀM XỬ LÝ DI CHUYỂN SECTION KÈM CẬP NHẬT TRỰC TIẾP ---
const moveSectionUp = (id, currentColumn) => {
    // Vẫn emit để báo cho component cha (nếu có dùng)
    emit('moveUp', id, currentColumn);

    // Thay đổi vị trí trực tiếp trong mảng local để giao diện cập nhật ngay lập tức
    if (!props.resumeData?.sections) return;
    const sections = props.resumeData.sections;
    const colSections = sections.filter(s => s.column === currentColumn);
    const idx = colSections.findIndex(s => s.id === id);

    if (idx > 0) {
        const prevId = colSections[idx - 1].id;
        const realIdxCur = sections.findIndex(s => s.id === id);
        const realIdxPrev = sections.findIndex(s => s.id === prevId);
        
        if (realIdxCur !== -1 && realIdxPrev !== -1) {
            // Cắt phần tử hiện tại và nhét vào vị trí của phần tử phía trên
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
            // Cắt phần tử hiện tại và nhét vào vị trí của phần tử phía dưới
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

// --- PAGINATION ENGINE ---
const A4_W_MM = 210;
const A4_H_MM = 297;
const MARGIN_MM = 15;

let paginateTimer = null;
const requestPagination = () => {
    if (paginateTimer) clearTimeout(paginateTimer);
    paginateTimer = setTimeout(doPagination, 60);
};

const doPagination = async () => {
    if (!cvRoot.value) return;
    
    const richElements = cvRoot.value.querySelectorAll('.item-container, .html-content p, .html-content li, .html-content div');
    richElements.forEach(el => el.classList.add('paginated-item'));

    const allPaginated = cvRoot.value.querySelectorAll('.paginated-item');
    allPaginated.forEach(el => { el.style.marginTop = ''; });

    await nextTick();

    const pxPerMm = cvRoot.value.offsetWidth / A4_W_MM;
    const pageH = A4_H_MM * pxPerMm;
    const marginPx = MARGIN_MM * pxPerMm;
    const safeLine = pageH - marginPx;
    const maxFit = safeLine - marginPx;

    const relTop = (el) => {
        let off = 0, cur = el;
        while (cur && cur !== cvRoot.value) { off += cur.offsetTop; cur = cur.offsetParent; }
        return off;
    };

    const processColumn = (colSelector) => {
        const col = cvRoot.value.querySelector(colSelector);
        if (!col) return;
        const colItems = col.classList.contains('paginated-item') 
            ? [col] 
            : Array.from(col.querySelectorAll('.paginated-item'));
            
        const leafItems = colItems.filter(el => !el.querySelector('.paginated-item'));

        for (let pass = 0; pass < 80; pass++) {
            let stable = true;
            for (const item of leafItems) {
                const top = relTop(item);
                const bottom = top + item.offsetHeight;
                const pageIdx = Math.floor(top / pageH);
                const curSafe = pageIdx * pageH + safeLine;
                
                if (top < curSafe && bottom > curSafe && item.offsetHeight <= maxFit) {
                    const targetTop = (pageIdx + 1) * pageH + marginPx;
                    const extra = targetTop - top;
                    const currentMt = parseFloat(item.style.marginTop || '0');
                    item.style.marginTop = (currentMt + extra) + 'px';
                    stable = false;
                    break;
                }
            }
            if (stable) break;
        }
    };

    processColumn('aside');
    processColumn('main');

    let maxBottom = 0;
    allPaginated.forEach(el => {
        const b = relTop(el) + el.offsetHeight;
        if (b > maxBottom) maxBottom = b;
    });

    const calculatedPageCount = Math.max(1, Math.ceil((maxBottom + marginPx) / pageH));
    if (pageCount.value !== calculatedPageCount) {
        pageCount.value = calculatedPageCount;
    }
};

watch(() => props.resumeData, () => requestPagination(), { deep: true })

onMounted(() => {
    if (props.resumeData && props.resumeData.sections) {
        // Tùy chỉnh hiển thị các ID cho chạy thẳng sang cột phải 
        const FORCED_RIGHT_IDS = ['summary', 'objective', 'education', 'skills']
        const DEFAULT_LEFT_IDS = ['languages', 'it_skills']

        props.resumeData.sections.forEach(sec => {
            const id = sec.id.toLowerCase()
            if (sec.isVisible === undefined) sec.isVisible = true

            // Ép vào cột phải theo yêu cầu
            if (FORCED_RIGHT_IDS.some(k => id.includes(k))) {
                sec.column = 'right'
            } 
            // Nếu chưa có vị trí chỉ định từ Database thì gán mặc định
            else if (!sec.column) {
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

const formatDesc = (text) => {
    if (!text) return '';
    if (/<[a-z][\s\S]*>/i.test(text)) {
        if (text.includes('<li') && (text.includes('<font') || text.includes('style='))) {
            try {
                const tempDiv = document.createElement('div');
                tempDiv.innerHTML = text;
                const lis = tempDiv.querySelectorAll('li');
                lis.forEach(li => {
                    const child = li.firstElementChild;
                    if (child && (child.tagName === 'FONT' || child.tagName === 'SPAN')) {
                        if (child.color) li.style.color = child.color;
                        if (child.style && child.style.color) li.style.color = child.style.color;
                    }
                });
                return tempDiv.innerHTML;
            } catch(e) {
                return text;
            }
        }
        return text;
    }
    return text.split('\n').map(l => l.trim()).filter(l=>l).join('<br/>');
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap');

#cv-printable-area {
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
    overflow-wrap: anywhere;
    font-family: 'Inter', 'Segoe UI', sans-serif;
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

.section-active {
    border: 2px solid var(--border-color, rgba(0,0,0,0.08)) !important;
    border-radius: 6px !important;
    transform: scale(1.015) !important;
    box-shadow: 0 4px 12px rgba(0,0,0,0.06) !important;
    background: rgba(0,0,0,0.02) !important;
    z-index: 10 !important;
}

.section-block-sidebar.section-active {
    --border-color: rgba(44, 188, 209, 0.2);
    background: rgba(255,255,255,0.4) !important;
}

.section-block-main.section-active {
    --border-color: rgba(44, 188, 209, 0.2);
}

/* ==================== NAV BUTTONS ==================== */
.nav-btns {
    position: absolute;
    right: 4px;
    top: -12px;
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

/* ==================== PRINT ==================== */
@media print {
    .no-print { display: none !important; }
    .section-block,
    .section-active {
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
:global(.is-exporting-pdf .section-active) {
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