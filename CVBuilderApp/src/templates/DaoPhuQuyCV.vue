<template>
  <div id="cv-printable-area" ref="cvRoot" class="flex flex-row relative box-border bg-white overflow-hidden" :style="{ width: '210mm', height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Segoe UI\', Tahoma, Geneva, Verdana, sans-serif' }">
    
    <aside class="z-10 flex flex-col shrink-0 relative box-border text-white" :style="{ width: '35%', backgroundColor: templatePrimaryColor, padding: '40px 25px' }">
      
      <div class="paginated-item relative z-20 w-full flex flex-col items-center mb-[30px]">
        <div class="relative rounded-full overflow-hidden mx-auto" :style="{ width: '160px', height: '160px', border: '5px solid #ffffff' }">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center bg-white/10 text-white/50">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
          </div>
        </div>
      </div>

      <div class="paginated-item w-full mb-[15px]">
        <h3 class="uppercase block" :style="{ fontSize: '18px !important', fontWeight: 'bold !important', borderBottom: '2px solid rgba(255,255,255,0.5)', paddingBottom: '8px !important', margin: '0 0 15px 0 !important', paddingLeft: '12px !important' }">
          LIÊN HỆ
        </h3>
        <ul class="w-full list-none p-0 m-0 flex flex-col gap-[8px]" :style="{ fontSize: '13px !important', lineHeight: '1.5', paddingLeft: '12px !important' }">
          <li v-if="!isEmpty(resumeData.general.phone)" class="flex items-start relative">
            <div class="w-[20px] text-center shrink-0 mr-[5px] mt-[2px]">
              <svg class="w-[12px] h-[12px] inline-block" fill="currentColor" viewBox="0 0 20 20"><path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z"></path></svg>
            </div>
            <span class="break-all" v-html="resumeData.general.phone"></span>
          </li>
          
          <li v-if="!isEmpty(resumeData.general.email)" class="flex items-start relative">
            <div class="w-[20px] text-center shrink-0 mr-[5px] mt-[2px]">
              <svg class="w-[12px] h-[12px] inline-block" fill="currentColor" viewBox="0 0 20 20"><path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z"></path><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z"></path></svg>
            </div>
            <span class="break-all" v-html="resumeData.general.email"></span>
          </li>

          <li v-if="!isEmpty(resumeData.general.dob)" class="flex items-start relative">
            <div class="w-[20px] text-center shrink-0 mr-[5px] mt-[2px]">
              <svg class="w-[12px] h-[12px] inline-block" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" clip-rule="evenodd"></path></svg>
            </div>
            <span class="break-all" v-html="resumeData.general.dob"></span>
          </li>

          <li v-if="!isEmpty(resumeData.general.address)" class="flex items-start relative">
            <div class="w-[20px] text-center shrink-0 mr-[5px] mt-[2px]">
              <svg class="w-[12px] h-[12px] inline-block" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd"></path></svg>
            </div>
            <span class="break-all" v-html="resumeData.general.address"></span>
          </li>
        </ul>
      </div>

      <div class="w-full flex-1 flex flex-col m-0 p-0">
        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block relative paginated-item group w-full mb-[10px]"
            :class="{ 'section-selected': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--sel-color': 'white' } : {}"
            @mouseenter="showNav(section.id)"
            @mouseleave="hideNav()"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <div v-show="hoveredSectionId === section.id || selectedSectionId === section.id" class="nav-btns no-print" @mouseenter="showNav(section.id)" @mouseleave="hideNav()">
              <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
              <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7-7"/></svg></button>
            </div>

            <h3 class="uppercase w-full block" :style="{ fontSize: '18px !important', fontWeight: 'bold !important', borderBottom: '2px solid rgba(255,255,255,0.5)', paddingBottom: '8px !important', margin: '25px 0 15px 0 !important', paddingLeft: '12px !important' }">
              {{ section.title }}
            </h3>
            
            <div class="w-full flex flex-col gap-[8px]" :style="{ paddingLeft: '12px !important' }">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full text-white" :style="{ fontSize: '13px !important', lineHeight: '1.5', margin: '0 !important', padding: '0 !important' }">
                <div v-if="section.id === 'education' || section.id === 'experience' || section.id === 'project'" class="w-full flex flex-col">
                  <span class="font-bold w-full break-words leading-tight" v-html="section.id === 'education' ? item.school : (item.company || item.name)"></span>
                  <div class="italic opacity-90 mt-[2px]" v-if="item.major || item.role">{{ item.major || item.role }}</div>
                  <div class="font-bold opacity-90 text-[11px] mt-[2px]" v-if="item.year || item.time">{{ item.year || item.time }}</div>
                  <div v-if="item.desc" class="html-content-sidebar text-justify whitespace-pre-line break-words w-full mt-[4px]" v-html="formatDesc(item.desc)"></div>
                </div>
                <div v-else-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'" class="flex flex-col">
                  <div class="font-bold w-full break-words">{{ item.name }}</div>
                  <div v-if="item.info || item.level" class="w-full break-words mt-[2px] opacity-90">{{ item.info || item.level }}</div>
                </div>
                <div v-else class="html-content-sidebar break-words whitespace-pre-line text-justify w-full" v-html="formatDesc(item.desc || item.name || item.info)"></div>
              </div>
            </div>
          </div>
        </template>
      </div>
    </aside>

    <main class="flex-1 flex flex-col relative bg-[#ffffff] z-20 overflow-hidden box-border pt-[30px] pr-[80px] pb-[40px]" @click.self="selectedSectionId = null">
      
      <header class="paginated-item w-full flex flex-col relative" :style="{ borderBottom: `4px solid ${templatePrimaryColor}`, paddingBottom: '15px', marginBottom: '30px', marginTop: '50px !important', marginLeft: '20px !important', marginRight: '40px !important', width: 'calc(100% - 60px)' }">
        <h1 class="uppercase break-words w-full m-0" :style="{ fontSize: '42px !important', fontWeight: '300 !important', color: '#333', lineHeight: '1.2', marginTop: '50px !important', paddingLeft: '20px !important', letterSpacing: '-1px' }" v-html="formattedFullName"></h1>
        
        <h2 class="uppercase break-words w-full m-0 text-[#555]" :style="{ fontSize: '18px !important', letterSpacing: '2px', fontWeight: 'normal', paddingLeft: '20px !important', marginTop: '12px !important' }" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'VỊ TRÍ ỨNG TUYỂN'"></h2>
      </header>

      <div v-if="summarySection && summarySection.isVisible" class="paginated-item relative group w-full mb-[25px]" :class="{ 'section-selected': selectedSectionId === summarySection.id }" @click.stop="selectedSectionId = selectedSectionId === summarySection.id ? null : summarySection.id" :style="{ marginLeft: '20px !important', marginRight: '40px !important', width: 'calc(100% - 60px)' }">
        
        <h3 class="uppercase w-full flex items-center" :style="{ fontSize: '19px !important', color: '#333', borderBottom: '2px solid #333', paddingBottom: '10px !important', margin: '0 0 15px 0 !important', paddingLeft: '20px !important', fontWeight: 'bold !important', gap: '10px' }">
          {{ summarySection.title || 'Mục tiêu nghề nghiệp' }}
        </h3>
        
        <div class="html-content text-justify whitespace-pre-line w-full text-[#333]" :style="{ fontSize: '14px !important', lineHeight: '1.6 !important', margin: '0 !important', paddingLeft: '20px !important' }" v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Tôi là một ứng viên năng động, mong muốn...' "></div>
      </div>

      <div class="w-full flex flex-col flex-1 m-0 p-0">
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block relative paginated-item group w-full mb-[25px]"
            :class="{ 'section-selected': selectedSectionId === section.id }"
            :style="[
              { marginLeft: '20px !important', marginRight: '40px !important', width: 'calc(100% - 60px)' },
              selectedSectionId === section.id ? { '--sel-color': templatePrimaryColor } : {}
            ]"
            @mouseenter="showNav(section.id)"
            @mouseleave="hideNav()"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <div v-show="hoveredSectionId === section.id || selectedSectionId === section.id" class="nav-btns no-print" @mouseenter="showNav(section.id)" @mouseleave="hideNav()">
              <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
              <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
            </div>

            <h3 class="uppercase w-full flex items-center" :style="{ fontSize: '19px !important', color: '#333', borderBottom: '2px solid #333', paddingBottom: '10px !important', margin: '20px 0 15px 0 !important', paddingLeft: '20px !important', fontWeight: 'bold !important', gap: '10px' }">
              {{ section.title }}
            </h3>

            <div class="w-full flex flex-col gap-[15px]" :style="{ paddingLeft: '20px !important' }">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full text-[#333]" :style="{ margin: '0 !important', padding: '0 !important' }">
                
                <div v-if="section.id === 'education' || section.id === 'experience' || section.id === 'project' || section.id === 'activities'" class="w-full">
                  <div class="w-full flex flex-col">
                    <span class="font-bold w-full break-words text-[#333] leading-tight" :style="{ margin: '0 !important', fontSize: '14.5px' }" v-html="section.id === 'education' ? item.school : (item.company || item.name)"></span>
                    
                    <div class="w-full flex justify-between items-baseline gap-2 text-[#555] mt-[2px]" :style="{ margin: '0 !important' }">
                        <span class="italic break-words flex-1 leading-tight" :style="{ margin: '0 !important', fontSize: '13.5px' }" v-if="item.major || item.role">{{ item.major || item.role }}</span>
                        <span class="font-bold shrink-0 whitespace-nowrap text-right" v-if="item.year || item.time" :style="{ color: templatePrimaryColor, fontSize: '13px', margin: '0 !important' }">{{ item.year || item.time }}</span>
                    </div>
                  </div>
                  
                  <div v-if="item.desc" class="html-content text-justify whitespace-pre-line break-words w-full mt-[5px]" :style="{ margin: '0 !important', padding: '0 !important', fontSize: '14px', lineHeight: '1.6' }" v-html="formatDesc(item.desc)"></div>
                  <div v-else-if="item.gradType" class="font-medium mt-[2px]" :style="{ color: templatePrimaryColor, fontSize: '13px !important', margin: '0 !important' }">Trạng thái: {{ item.gradType }}</div>
                </div>

                <div v-else class="w-full">
                  <div v-if="item.year || item.time" class="font-bold mb-[2px]" :style="{ color: templatePrimaryColor, fontSize: '13.5px', margin: '0 !important' }">{{ item.year || item.time }}</div>
                  <div class="html-content text-justify whitespace-pre-line break-words w-full" :style="{ margin: '0 !important', padding: '0 !important', fontSize: '14px', lineHeight: '1.6' }" v-html="formatDesc(item.desc || item.name || item.info)"></div>
                </div>

                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 absolute right-[5px] top-0 w-[18px] h-[18px]">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>
      </div>
    </main>
    
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

const cvRoot = ref(null);
const pageCount = ref(1);

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
    const MARGIN_TOP_MM = 20;    
    const rootWidthPx = cvRoot.value.offsetWidth;
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
        const colItems = col.classList.contains('paginated-item') ? [col] : col.querySelectorAll('.paginated-item');
        let isStable = false;
        let attempts = 0;
        while (!isStable && attempts < 50) {
            isStable = true;
            attempts++;
            for (let i = 0; i < colItems.length; i++) {
                const item = colItems[i];
                if (!item) continue;
                const top = getRelativeTop(item);
                const bottom = top + item.offsetHeight;
                const currentPageIndex = Math.floor(top / pageHeightPx);
                const currentSafeBottom = (currentPageIndex * pageHeightPx) + safeBottomPx;
                if (bottom > currentSafeBottom && top < (currentPageIndex + 1) * pageHeightPx) {
                    const targetTop = (currentPageIndex + 1) * pageHeightPx + marginTopPx;
                    const pushAmount = targetTop - top;
                    const currentMt = parseFloat(item.style.marginTop || '0');
                    item.style.marginTop = (currentMt + pushAmount) + 'px';
                    isStable = false; 
                    break;
                }
            }
        }
    };

    processColumn('main');
    processColumn('aside');
    let maxBottom = 0;
    items.forEach(el => {
        const top = getRelativeTop(el);
        const bottom = top + el.offsetHeight;
        if (bottom > maxBottom) maxBottom = bottom;
    });
    const calculatedPageCount = Math.max(1, Math.ceil((maxBottom + marginBottomPx - 2) / pageHeightPx));
    if (pageCount.value !== calculatedPageCount) pageCount.value = calculatedPageCount;
};

watch(() => props.resumeData, () => requestPagination(), { deep: true });
onMounted(() => {
    // Chỉ kích hoạt các mục có trong ảnh mẫu
    const sidebarIdsArr = ['education', 'it_skills', 'languages'];
    const mainIdsArr = ['summary', 'experience', 'skills'];
    
    if (props.resumeData && props.resumeData.sections) {
        props.resumeData.sections.forEach(sec => {
            if (sidebarIdsArr.includes(sec.id)) {
                sec.isVisible = true;
                sec.column = 'left';
            } else if (mainIdsArr.includes(sec.id)) {
                sec.isVisible = true;
                sec.column = 'right';
            } else {
                sec.isVisible = false;
            }
        });
    }

    requestPagination();
    window.addEventListener('resize', requestPagination);
    document.addEventListener('keyup', requestPagination);
});
onUnmounted(() => {
    window.removeEventListener('resize', requestPagination);
    document.removeEventListener('keyup', requestPagination);
});

const isEmpty = (val) => {
    if (!val) return true;
    if (typeof val !== 'string') return false;
    const cleanText = val.replace(/<[^>]*>/g, '').replace(/&nbsp;/g, ' ').trim();
    return cleanText === '';
};
const props = defineProps({ resumeData: { type: Object, required: true } });
const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem']);
const hoveredSectionId = ref(null);
const showNav = (id) => hoveredSectionId.value = id;
const hideNav = () => hoveredSectionId.value = null;
const selectedSectionId = ref(null);
const templatePrimaryColor = computed(() => props.resumeData.theme?.primaryColor || '#004C82');
const summarySection = computed(() => props.resumeData.sections.find(s => s.id === 'summary'));
const sidebarSections = computed(() => props.resumeData.sections.filter(s => s.column === 'left' && s.id !== 'summary'));
const mainSections = computed(() => props.resumeData.sections.filter(s => s.column === 'right' && s.id !== 'summary'));
const sidebarIds = computed(() => sidebarSections.value.map(s => s.id));
const mainIds = computed(() => mainSections.value.map(s => s.id));
const formatDesc = (text) => text?.split('\n').join('<br/>') || '';

const formattedFullName = computed(() => {
    const name = props.resumeData.general.fullName || 'HỌ VÀ TÊN';
    const words = name.trim().split(/\s+/);
    if (words.length < 2) return name;
    
    // Nếu có 2 từ trở lên, in đậm 2 từ cuối
    const lastTwo = words.slice(-2).join(' ');
    const firstPart = words.slice(0, -2).join(' ');
    return `${firstPart} <strong style="font-weight: 800">${lastTwo}</strong>`.trim();
});

const hasContactInfo = computed(() => {
    return !isEmpty(props.resumeData.general.phone) || 
           !isEmpty(props.resumeData.general.email) || 
           !isEmpty(props.resumeData.general.dob) || 
           !isEmpty(props.resumeData.general.address);
});
</script>

<style scoped>
#cv-printable-area {
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
    overflow-wrap: anywhere;
}

.item-container {
    position: relative;
    transition: all 0.2s;
}

.delete-btn {
    opacity: 0;
    transition: all 0.2s;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
}

.item-container:hover .delete-btn {
    opacity: 1;
}

.section-selected .delete-btn {
    opacity: 1 !important;
}

/* HTML CONTENT CHO CỘT MAIN (Chữ đen/xám) */
:deep(.html-content) { margin: 0 !important; padding: 0 !important; }
:deep(.html-content p) { margin: 0 !important; padding: 0 !important; }
:deep(.html-content ul) { list-style-type: disc !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content ol) { list-style-type: decimal !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content b), :deep(.html-content strong) { font-weight: bold; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic; }
:deep(.html-content u) { text-decoration: underline; }
:deep(.html-content ul li), :deep(.html-content ol li) { margin-bottom: 2px !important; }

/* HTML CONTENT CHO CỘT SIDEBAR (Chữ trắng) */
:deep(.html-content-sidebar) { margin: 0 !important; padding: 0 !important; color: white !important; }
:deep(.html-content-sidebar p) { margin: 0 !important; padding: 0 !important; }
:deep(.html-content-sidebar ul) { list-style-type: disc !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content-sidebar ol) { list-style-type: decimal !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content-sidebar b), :deep(.html-content-sidebar strong) { font-weight: bold; }
:deep(.html-content-sidebar i), :deep(.html-content-sidebar em) { font-style: italic; opacity: 0.9;}
:deep(.html-content-sidebar u) { text-decoration: underline; }
:deep(.html-content-sidebar ul li), :deep(.html-content-sidebar ol li) { margin-bottom: 2px !important; }

/* CHUNG SIZE FONT EDITOR */
:deep(.html-content li:has(> font[size="1"])), :deep(.html-content-sidebar li:has(> font[size="1"])) { font-size: 10px; }
:deep(.html-content li:has(> font[size="2"])), :deep(.html-content-sidebar li:has(> font[size="2"])) { font-size: 11px; }
:deep(.html-content li:has(> font[size="3"])), :deep(.html-content-sidebar li:has(> font[size="3"])) { font-size: 13px; }
:deep(.html-content li:has(> font[size="4"])), :deep(.html-content-sidebar li:has(> font[size="4"])) { font-size: 15px; }


/* === SECTION BLOCKS === */
.section-block {
    position: relative;
    border: 2px solid transparent !important; 
    cursor: pointer;
    transition: all 0.15s ease;
    border-radius: 8px !important;
}

.section-block:hover {
    background: rgba(0, 0, 0, 0.02);
}

.section-selected {
    border-color: var(--sel-color, #004C82) !important;
    outline: none !important;
}

/* === NAV BUTTONS === */
.nav-btns {
    position: absolute;
    right: 5px;
    top: 5px;
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
    transition: all 0.15s ease;
}

.nav-btn:hover { background: #1d4ed8; transform: scale(1.1); }
.nav-btn:active { transform: scale(0.95); }

@media print {
    .no-print { display: none !important; }
    .section-block, .section-selected {
        cursor: default;
        box-shadow: none !important;
        background: transparent !important;
        border-color: transparent !important;
        padding: 0 !important;
        margin: 0 !important;
        outline: none !important;
    }
}
:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .section-selected) {
    cursor: default !important;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
    outline: none !important;
}
</style>