<template>
  <div id="cv-printable-area" ref="cvRoot" class="flex flex-row relative box-border bg-white overflow-hidden text-[#333]" :style="{ width: '210mm', height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif', lineHeight: '1.5' }">
    
    <!-- SIDEBAR -->
    <aside class="z-10 flex flex-col shrink-0 relative box-border" :style="{ width: '32%', backgroundColor: '#e0f2f7', padding: '30px 20px' }">
      
      <!-- Avatar Section -->
      <div class="paginated-item relative z-20 w-full flex flex-col items-center mb-8">
        <div class="relative rounded-full overflow-hidden mx-auto bg-white" :style="{ width: '150px', height: '150px', border: '5px solid white', boxShadow: '0 4px 15px rgba(0,0,0,0.1)' }">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center bg-gray-100 text-gray-400">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
          </div>
        </div>
      </div>

      <!-- Thông tin cá nhân -->
      <div class="section-block paginated-item group mb-4">
        <h3 class="uppercase block font-bold mb-4" :style="{ fontSize: '16px !important', color: templatePrimaryColor, borderBottom: `2px solid ${templatePrimaryColor}`, paddingBottom: '5px !important', fontWeight: 'bold !important' }">
          Thông tin cá nhân
        </h3>
        <ul class="w-full list-none p-0 m-0 text-slate-700 space-y-3" :style="{ fontSize: '13px' }">
          <li v-if="!isEmpty(resumeData.general.dob)" class="flex items-center gap-3">
            <div class="w-5 h-5 flex items-center justify-center text-blue-500">
              <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20"><path d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z"></path></svg>
            </div>
            <span v-html="resumeData.general.dob"></span>
          </li>
          <li v-if="!isEmpty(resumeData.general.gender)" class="flex items-center gap-3">
            <div class="w-5 h-5 flex items-center justify-center text-blue-500">
              <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20"><path d="M10 2a8 8 0 100 16 8 8 0 000-16zM7 9a3 3 0 116 0 3 3 0 01-6 0zm1.707 5.707l1.293-1.293 1.293 1.293A1 1 0 0110 16.414l-1.293-1.707z"></path></svg>
            </div>
            <span v-html="resumeData.general.gender"></span>
          </li>
          <li v-if="!isEmpty(resumeData.general.phone)" class="flex items-center gap-3">
            <div class="w-5 h-5 flex items-center justify-center text-blue-500">
              <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20"><path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z"></path></svg>
            </div>
            <span v-html="resumeData.general.phone"></span>
          </li>
          <li v-if="!isEmpty(resumeData.general.email)" class="flex items-center gap-3">
            <div class="w-5 h-5 flex items-center justify-center text-blue-500">
              <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20"><path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z"></path><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z"></path></svg>
            </div>
            <span class="break-all" v-html="resumeData.general.email"></span>
          </li>
          <li v-if="!isEmpty(resumeData.general.address)" class="flex items-center gap-3">
            <div class="w-5 h-5 flex items-center justify-center text-blue-500">
              <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd"></path></svg>
            </div>
            <span v-html="resumeData.general.address"></span>
          </li>
        </ul>
      </div>

      <!-- Sidebar Sections (Skills) -->
      <div class="w-full flex-1 flex flex-col gap-2">
        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block relative paginated-item group w-full"
            :class="{ 'section-selected': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--sel-color': templatePrimaryColor } : {}"
            @mouseenter="showNav(section.id)"
            @mouseleave="hideNav()"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <div v-show="hoveredSectionId === section.id || selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg></button>
              <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg></button>
            </div>

            <h3 class="uppercase block font-bold mb-4" :style="{ fontSize: '16px !important', color: templatePrimaryColor, borderBottom: `2px solid ${templatePrimaryColor}`, paddingBottom: '5px !important', fontWeight: 'bold !important' }">
              {{ section.title }}
            </h3>
            
            <div class="space-y-4 px-1">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative text-slate-800">
                <div class="font-bold text-[14px] leading-snug" :style="{ fontWeight: 'bold !important' }">{{ item.name }}</div>
                <div v-if="item.info || item.level" class="text-[12px] text-slate-500 mt-0.5">{{ item.info || item.level }}</div>

                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 opacity-0 group-hover:opacity-100 absolute -right-2 top-0">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>
      </div>
    </aside>

    <!-- MAIN CONTENT -->
    <main class="flex-1 flex flex-col relative bg-white z-20 overflow-hidden box-border" :style="{ padding: '40px 60px 40px 45px !important' }" @click.self="selectedSectionId = null">
      
      <!-- Header -->
      <header class="paginated-item w-full flex flex-col mb-4">
        <h1 class="uppercase font-black tracking-tight" :style="{ color: templatePrimaryColor, fontSize: '42px !important', lineHeight: '1.1 !important', fontWeight: '900 !important' }" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'HỌ VÀ TÊN'"></h1>
        <h2 class="uppercase font-semibold text-slate-500 mt-2 tracking-widest" :style="{ fontSize: '18px !important', fontWeight: '600 !important' }" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'VỊ TRÍ ỨNG TUYỂN'"></h2>
      </header>

      <!-- Main Sections -->
      <div class="w-full flex flex-col gap-2">
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block relative paginated-item group w-full"
            :class="{ 'section-selected': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--sel-color': templatePrimaryColor } : {}"
            @mouseenter="showNav(section.id)"
            @mouseleave="hideNav()"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <div v-show="hoveredSectionId === section.id || selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
              <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
            </div>

            <h3 class="uppercase font-bold mb-5" :style="{ fontSize: '18px !important', color: templatePrimaryColor, borderBottom: `2px solid ${templatePrimaryColor}`, paddingBottom: '5px !important', width: 'calc(100% - 30px) !important', fontWeight: 'bold !important' }">
              {{ section.title }}
            </h3>

            <!-- Summary Special Case inside loop -->
            <div v-if="section.id === 'summary'" class="html-content text-justify text-slate-700 leading-relaxed" :style="{ fontSize: '14px' }" v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Tôi là một ứng viên năng động...'">
            </div>

            <div v-else class="space-y-8">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full text-slate-700">
                
                <!-- Education & Experience Style -->
                <div v-if="section.id === 'education' || section.id === 'experience' || section.id === 'project' || section.id === 'activities'">
                  <div class="flex justify-between items-start gap-4 mb-1">
                    <div class="font-bold text-[16px] text-slate-900 leading-tight" :style="{ fontWeight: 'bold !important' }">
                      {{ section.id === 'education' ? item.school : (item.company || item.name) }}
                    </div>
                    <div v-if="item.year || item.time" class="shrink-0 font-bold text-[13px] text-slate-400 uppercase tracking-wider">
                      {{ item.year || item.time }}
                    </div>
                  </div>
                  
                  <div v-if="item.major || item.role" class="font-semibold text-slate-500 italic text-[14px] mb-2">
                    {{ item.major || item.role }}
                  </div>

                  <div v-if="item.desc" class="html-content text-justify text-[14px] leading-relaxed" v-html="formatDesc(item.desc)"></div>
                  <div v-else-if="item.gradType" class="text-blue-500 font-semibold text-[13px]">{{ item.gradType }}</div>
                </div>

                <!-- Generic Style -->
                <div v-else>
                  <div class="flex gap-4">
                    <div v-if="item.year || item.time" class="shrink-0 font-bold text-blue-500 text-[14px]">
                      {{ item.year || item.time }}
                    </div>
                    <div class="flex-1">
                      <div class="font-bold text-[15px] mb-1" v-if="item.name">{{ item.name }}</div>
                      <div class="html-content text-justify text-[14px] leading-relaxed" v-html="formatDesc(item.desc || item.info)"></div>
                    </div>
                  </div>
                </div>

                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 absolute -right-6 top-0 w-5 h-5">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>
      </div>
    </main>

    <!-- Pagination Overlays -->
    <template v-for="p in (pageCount - 1)" :key="'div-'+p">
      <div class="absolute left-0 w-full z-50 flex flex-col items-center justify-center pointer-events-none no-print" 
           :style="{ top: `calc(${p * 297}mm - 8px)` }">
        <div class="w-[105%] h-[16px] bg-slate-800/95 shadow-inner border-y border-black/30 backdrop-blur-sm"></div>
        <span class="absolute text-[9px] uppercase font-bold text-slate-300 tracking-widest bg-slate-700 px-3 py-0.5 rounded border border-slate-600 shadow-md">Ngắt trang {{ p + 1 }}</span>
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

const cvRoot = ref(null);
const pageCount = ref(1);

// --- CHỨC NĂNG PHÂN TRANG (PAGINATION ENGINE) ---
let paginateTimer = null;
const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer);
  paginateTimer = setTimeout(doPagination, 50);
};

const doPagination = async () => {
  if (!cvRoot.value) return;
  
  const items = cvRoot.value.querySelectorAll('.paginated-item');
  items.forEach(el => { el.style.marginTop = ''; });

  await nextTick();

  const A4_WIDTH_MM = 210;
  const A4_HEIGHT_MM = 297;
  const MARGIN_BOTTOM_MM = 20; 
  const MARGIN_TOP_MM = 15;    
  
  const rootWidthPx = cvRoot.value.offsetWidth;
  if (!rootWidthPx) return;
  
  const pxPerMm = rootWidthPx / A4_WIDTH_MM;
  const pageHeightPx = A4_HEIGHT_MM * pxPerMm;
  const marginBottomPx = MARGIN_BOTTOM_MM * pxPerMm;
  const marginTopPx = MARGIN_TOP_MM * pxPerMm;
  const safeBottomPx = pageHeightPx - marginBottomPx;

  const getRelativeTop = (el) => {
    let offset = 0;
    let currentEl = el;
    while (currentEl && currentEl !== cvRoot.value) {
      offset += currentEl.offsetTop;
      currentEl = currentEl.offsetParent;
    }
    return offset;
  };

  const processColumn = (colSelector) => {
    const col = cvRoot.value.querySelector(colSelector);
    if (!col) return;
    const colItems = col.querySelectorAll('.paginated-item');

    let isStable = false;
    let attempts = 0;
    
    while (!isStable && attempts < 50) {
      isStable = true;
      attempts++;
      
      for (let i = 0; i < colItems.length; i++) {
        const item = colItems[i];
        const top = getRelativeTop(item);
        const bottom = top + item.offsetHeight;
        
        const currentPageIndex = Math.floor(top / pageHeightPx);
        const currentSafeBottom = (currentPageIndex * pageHeightPx) + safeBottomPx;
        
        if (bottom > currentSafeBottom && top < (currentPageIndex + 1) * pageHeightPx) {
          const targetTop = (currentPageIndex + 1) * pageHeightPx + marginTopPx;
          const currentMargin = parseFloat(item.style.marginTop || '0');
          const topWithoutMargin = top - currentMargin;
          item.style.marginTop = (targetTop - topWithoutMargin) + 'px';
          isStable = false; 
          break;
        }
      }
    }
  };

  processColumn('header');
  processColumn('aside');
  processColumn('main');

  let maxBottom = 0;
  items.forEach(el => {
    const top = getRelativeTop(el);
    const bottom = top + el.offsetHeight;
    if (bottom > maxBottom) maxBottom = bottom;
  });

  const calculatedPageCount = Math.max(1, Math.ceil((maxBottom + marginBottomPx - 2) / pageHeightPx));
  if (pageCount.value !== calculatedPageCount) {
    pageCount.value = calculatedPageCount;
    // Chạy lại một lần nữa sau khi Vue cập nhật DOM để đảm bảo vị trí chuẩn xác
    nextTick(() => requestPagination());
  }
};

watch(() => props.resumeData, () => requestPagination(), { deep: true });

let observer = null;
onMounted(() => {
  // Chỉ kích hoạt các mục có trong ảnh mẫu
  const activeSectionIds = ['summary', 'education', 'experience', 'skills', 'certifications'];
  if (props.resumeData && props.resumeData.sections) {
    props.resumeData.sections.forEach(sec => {
      sec.isVisible = activeSectionIds.includes(sec.id);
    });
  }

  requestPagination();
  window.addEventListener('resize', requestPagination);
  document.addEventListener('keyup', requestPagination);

  // Theo dõi sự thay đổi của DOM (như khi xóa item) để cập nhật trang ngay lập tức
  observer = new MutationObserver(() => requestPagination());
  if (cvRoot.value) {
    observer.observe(cvRoot.value, { childList: true, subtree: true, characterData: true });
  }
});

onUnmounted(() => {
  window.removeEventListener('resize', requestPagination);
  document.removeEventListener('keyup', requestPagination);
  if (paginateTimer) clearTimeout(paginateTimer);
  if (observer) observer.disconnect();
});

const isEmpty = (val) => {
  if (!val) return true;
  if (typeof val !== 'string') return false;
  return val.replace(/<[^>]*>/g, '').trim() === '';
};

const hoveredSectionId = ref(null)
let _hideTimer = null

const showNav = (id) => {
  if (_hideTimer) { clearTimeout(_hideTimer); _hideTimer = null; }
  hoveredSectionId.value = id;
}

const hideNav = () => {
  _hideTimer = setTimeout(() => { hoveredSectionId.value = null; _hideTimer = null; }, 120);
}

const selectedSectionId = ref(null)

const templatePrimaryColor = computed(() => props.resumeData.theme.primaryColor || '#2d7fb2');

const summarySection = computed(() => props.resumeData.sections.find(s => s.id === 'summary'));
const sidebarSections = computed(() => props.resumeData.sections.filter(s => s.column === 'left' && s.id !== 'summary'));
const mainSections = computed(() => props.resumeData.sections.filter(s => s.column === 'right'));

const sidebarIds = computed(() => sidebarSections.value.map(s => s.id));
const mainIds = computed(() => mainSections.value.map(s => s.id));

const formatDesc = (text) => {
  if (!text) return '';
  if (/<[a-z][\s\S]*>/i.test(text)) return text;
  return text.split('\n').map(l => l.trim()).filter(l=>l).join('<br/>');
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800;900&display=swap');

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

.section-block {
  border: 1.5px solid transparent !important; 
  cursor: pointer !important;
  transition: all 0.2s !important;
  padding: 10px 15px !important;
  border-radius: 8px !important;
}

.section-block:hover { background: rgba(0, 0, 0, 0.02); }

.section-selected {
  border: 1.5px solid var(--sel-color, #2d7fb2) !important;
  background: rgba(45, 127, 178, 0.05) !important;
  box-shadow: 0 0 0 2px rgba(45, 127, 178, 0.1) !important;
}

.nav-btns {
  position: absolute;
  right: 10px;
  top: 10px;
  display: flex;
  gap: 4px;
  z-index: 99;
}

.nav-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 4px;
  background: #2d7fb2;
  color: white;
  border-radius: 4px;
  cursor: pointer;
}

.delete-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: opacity 0.2s;
}

:deep(.html-content ul) {
  list-style-type: disc !important;
  padding-left: 1.5rem !important;
}

:deep(.html-content ol) {
  list-style-type: decimal !important;
  padding-left: 1.5rem !important;
}

:deep(.html-content b), :deep(.html-content strong) { font-weight: bold; }

@media print {
  .no-print { display: none !important; }
  .section-block { border: none !important; padding: 0 !important; background: transparent !important; }
}
</style>