<template>
  <div id="cv-printable-area" ref="cvRoot" class="flex flex-row relative box-border overflow-hidden" :style="{ width: '210mm', height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Segoe UI\', Roboto, Helvetica, Arial, sans-serif', backgroundColor: '#f3ebdd !important' }">
    
    <main class="flex-[6] flex flex-col relative z-20 overflow-hidden box-border pt-[40px] pb-[50px]" :style="{ backgroundColor: '#f3ebdd !important' }" @click.self="selectedSectionId = null">
      
      <header class="paginated-item w-full flex flex-col relative" :style="{ paddingBottom: '65px', marginBottom: '0', marginLeft: '20px !important', paddingTop: '75px !important', marginRight: '20px !important', width: 'calc(100% - 40px)' }">
        
        <h1 class="uppercase break-words w-full m-0" :style="{ fontSize: '56px !important', fontWeight: '800 !important', color: '#5d4e46', lineHeight: '1.1', marginTop: '0 !important', paddingLeft: '20px !important' }" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'HỌ VÀ TÊN'"></h1>
        
        <p class="uppercase break-words w-full m-0" :style="{ fontSize: '22px !important', letterSpacing: '2px', color: '#8b7355', marginTop: '10px !important', fontWeight: '600 !important', paddingLeft: '20px !important' }" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'VỊ TRÍ ỨNG TUYỂN'"></p>
      </header>

      <!-- Thanh ngang 10px -->
      <div class="paginated-item w-full" :style="{ height: '10px', backgroundColor: '#5d4e46', marginBottom: '40px', width: '100%' }"></div>

      <div class="w-full flex flex-col flex-1 m-0 p-0">
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block relative paginated-item group w-full mb-[20px]"
            :class="{ 'section-selected': selectedSectionId === section.id }"
            :style="{ '--sel-color': '#8b7355', marginLeft: '20px !important', marginRight: '20px !important', width: 'calc(100% - 40px)' }"
            @mouseenter="showNav(section.id)"
            @mouseleave="hideNav()"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <div v-show="hoveredSectionId === section.id || selectedSectionId === section.id" class="nav-btns no-print" @mouseenter="showNav(section.id)" @mouseleave="hideNav()">
              <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
              <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải (Cột phụ)"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg></button>
            </div>

            <h1 class="uppercase w-full block" :style="{ fontSize: '17px !important', color: '#8b7355', paddingBottom: '8px !important', margin: '20px 0 15px 0 !important', paddingLeft: '20px !important', fontWeight: '800 !important', letterSpacing: '1px' }">
              {{ section.title }}
            </h1>

            <div class="w-full flex flex-col gap-[20px]" :style="{ borderLeft: '1px solid #d4c5b4', paddingLeft: '15px !important', marginLeft: '20px !important', width: 'calc(100% - 20px)' }">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full text-[#4a3728]" :style="{ margin: '0 !important', padding: '0 !important' }">
                
                <div v-if="section.id === 'education' || section.id === 'experience' || section.id === 'project' || section.id === 'activities'" class="w-full">
                  <div class="w-full flex flex-col">
                    <div class="w-full flex justify-between items-baseline gap-2" :style="{ margin: '0 !important' }">
                        <span class="font-bold break-words flex-1 leading-tight text-[#4a3728]" :style="{ margin: '0 !important', fontSize: '15px' }" v-html="section.id === 'education' ? item.school : (item.company || item.name)"></span>
                        <span class="font-bold shrink-0 whitespace-nowrap text-right text-[#8b7355]" v-if="item.year || item.time" :style="{ fontSize: '13px', margin: '0 !important' }">{{ item.year || item.time }}</span>
                    </div>
                    <div class="italic text-[#5d4e46] mt-[2px] mb-[5px] w-full" :style="{ fontSize: '14px !important' }" v-if="item.major || item.role">{{ item.major || item.role }}</div>
                  </div>
                  
                  <div v-if="item.desc" class="html-content text-justify whitespace-pre-line break-words w-full" :style="{ margin: '0 !important', padding: '0 !important', fontSize: '14px', lineHeight: '1.7' }" v-html="formatDesc(item.desc)"></div>
                  <div v-else-if="item.gradType" class="font-medium mt-[2px]" :style="{ color: '#8b7355', fontSize: '13px !important', margin: '0 !important' }">Trạng thái: {{ item.gradType }}</div>
                </div>

                <div v-else class="w-full">
                  <div v-if="item.year || item.time" class="font-bold mb-[5px] text-[#4a3728]" :style="{ fontSize: '14px', margin: '0 !important' }">{{ item.year || item.time }}</div>
                  <div class="html-content text-justify whitespace-pre-line break-words w-full" :style="{ margin: '0 !important', padding: '0 !important', fontSize: '14px', lineHeight: '1.7' }" v-html="formatDesc(item.desc || item.name || item.info)"></div>
                </div>

                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 absolute right-[20px] top-[5px] w-[18px] h-[18px]">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>
      </div>
    </main>

    <aside class="z-10 flex flex-col shrink-0 relative box-border text-white" :style="{ width: '38%', backgroundColor: '#5d4e46', padding: '40px 30px', margin: '20px 0 !important', height: 'calc(100% - 40px) !important', alignSelf: 'flex-start' }">
      
      <div class="paginated-item relative z-20 w-full flex flex-col items-center mb-[40px]">
        <div class="relative rounded-full overflow-hidden mx-auto" :style="{ width: '180px', height: '180px', border: '8px solid rgba(255,255,255,0.1)' }">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center bg-white/10 text-white/50">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
          </div>
        </div>
      </div>

      <div v-if="summarySection && summarySection.isVisible" class="paginated-item relative group w-full mb-[35px]" :class="{ 'section-selected': selectedSectionId === summarySection.id }" @click.stop="selectedSectionId = selectedSectionId === summarySection.id ? null : summarySection.id">
        <h1 class="uppercase block" :style="{ fontSize: '16px !important', fontWeight: '700 !important', paddingBottom: '5px !important', margin: '0 0 15px 0 !important', color: '#fdf5e6' }">
          {{ summarySection.title || 'TÓM TẮT CHUYÊN MÔN' }}
        </h1>
        <div class="html-content-sidebar text-justify whitespace-pre-line w-full text-[#e8e8e8]" :style="{ fontSize: '13px !important', lineHeight: '1.6 !important', margin: '0 !important' }" v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Tôi là một ứng viên năng động, mong muốn...' "></div>
      </div>

      <div class="w-full flex-1 flex flex-col m-0 p-0">
        <!-- Render Awards Section if in sidebar -->
        <template v-for="section in sidebarSections.filter(s => s.id === 'awards')" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block relative paginated-item group w-full mb-[35px]"
            :class="{ 'section-selected': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--sel-color': 'white' } : {}"
            @mouseenter="showNav(section.id)"
            @mouseleave="hideNav()"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <div v-show="hoveredSectionId === section.id || selectedSectionId === section.id" class="nav-btns no-print" @mouseenter="showNav(section.id)" @mouseleave="hideNav()">
              <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
              <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái (Cột chính)"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
            </div>

            <h1 class="uppercase w-full block" :style="{ fontSize: '16px !important', fontWeight: '700 !important', paddingBottom: '5px !important', margin: '0 0 15px 0 !important', color: '#fdf5e6' }">
              {{ section.title }}
            </h1>
            
            <div class="w-full flex flex-col gap-[15px]">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full text-[#e8e8e8]" :style="{ fontSize: '13px !important', margin: '0 !important', padding: '0 !important' }">
                <div class="w-full flex flex-col">
                  <span class="font-bold w-full break-words leading-tight text-[#fdf5e6]" v-html="section.id === 'education' ? item.school : (item.company || item.name)"></span>
                  <div class="italic opacity-90 mt-[2px] text-[12px]" v-if="item.major || item.role">{{ item.major || item.role }}</div>
                  <div class="font-bold opacity-90 text-[11px] mt-[2px]" v-if="item.year || item.time">{{ item.year || item.time }}</div>
                  <div v-if="item.desc" class="html-content-sidebar text-justify whitespace-pre-line break-words w-full mt-[5px]" :style="{ lineHeight: '1.6' }" v-html="formatDesc(item.desc)"></div>
                </div>
                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 opacity-0 group-hover:opacity-100 transition-opacity absolute right-[20px] top-[5px] w-[18px] h-[18px]">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>

        <!-- Render Contact Info -->
        <div class="paginated-item w-full mb-[35px]">
          <h1 class="uppercase block" :style="{ fontSize: '16px !important', fontWeight: '700 !important', paddingBottom: '5px !important', margin: '0 0 15px 0 !important', color: '#fdf5e6' }">
            THÔNG TIN LIÊN HỆ
          </h1>
          <ul class="w-full list-none p-0 m-0 flex flex-col gap-[12px]" :style="{ fontSize: '13px !important', lineHeight: '1.6', color: '#e8e8e8' }">
            <li v-if="!isEmpty(resumeData.general.phone)" class="flex items-start relative break-words w-full">
              <strong class="font-bold mr-1 shrink-0 text-[#fdf5e6]">Di động:</strong> 
              <span v-html="resumeData.general.phone"></span>
            </li>
            <li v-if="!isEmpty(resumeData.general.email)" class="flex items-start relative break-words w-full">
              <strong class="font-bold mr-1 shrink-0 text-[#fdf5e6]">Email:</strong> 
              <span v-html="resumeData.general.email"></span>
            </li>
            <li v-if="!isEmpty(resumeData.general.website) || !isEmpty(resumeData.general.linkedin) || !isEmpty(resumeData.general.github)" class="flex items-start relative break-words w-full">
              <strong class="font-bold mr-1 shrink-0 text-[#fdf5e6]">Trang web:</strong> 
              <span v-html="!isEmpty(resumeData.general.website) ? resumeData.general.website : (!isEmpty(resumeData.general.linkedin) ? resumeData.general.linkedin : resumeData.general.github)"></span>
            </li>
            <li v-if="!isEmpty(resumeData.general.address)" class="flex items-start relative break-words w-full">
              <strong class="font-bold mr-1 shrink-0 text-[#fdf5e6]">Địa chỉ:</strong> 
              <span v-html="resumeData.general.address"></span>
            </li>
          </ul>
        </div>

        <!-- Render remaining sidebar sections (including Skills) -->
        <template v-for="section in sidebarSections.filter(s => s.id !== 'awards')" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block relative paginated-item group w-full mb-[35px]"
            :class="{ 'section-selected': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--sel-color': 'white' } : {}"
            @mouseenter="showNav(section.id)"
            @mouseleave="hideNav()"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <div v-show="hoveredSectionId === section.id || selectedSectionId === section.id" class="nav-btns no-print" @mouseenter="showNav(section.id)" @mouseleave="hideNav()">
              <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
              <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái (Cột chính)"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
            </div>

            <h1 class="uppercase w-full block" :style="{ fontSize: '16px !important', fontWeight: '700 !important', paddingBottom: '5px !important', margin: '0 0 15px 0 !important', color: '#fdf5e6' }">
              {{ section.title }}
            </h1>
            
            <div class="w-full flex flex-col gap-[15px]">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full text-[#e8e8e8]" :style="{ fontSize: '13px !important', margin: '0 !important', padding: '0 !important' }">
                
                <div v-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'" class="flex flex-col skill-group-extra mt-[5px]">
                  <div class="extra-label w-full uppercase" :style="{ fontSize: '12px !important', fontWeight: 'bold !important', color: '#fdf5e6', marginBottom: '5px' }">
                     🎨 {{ item.name }}
                  </div>
                  <div v-if="item.info || item.level" class="extra-content w-full break-words whitespace-pre-line" :style="{ fontSize: '12px !important', color: '#ddd', lineHeight: '1.4', paddingLeft: '5px' }">{{ item.info || item.level }}</div>
                </div>

                <div v-else class="w-full flex flex-col">
                  <span class="font-bold w-full break-words leading-tight text-[#fdf5e6]" v-html="section.id === 'education' ? item.school : (item.company || item.name)"></span>
                  <div class="italic opacity-90 mt-[2px] text-[12px]" v-if="item.major || item.role">{{ item.major || item.role }}</div>
                  <div class="font-bold opacity-90 text-[11px] mt-[2px]" v-if="item.year || item.time">{{ item.year || item.time }}</div>
                  <div v-if="item.desc" class="html-content-sidebar text-justify whitespace-pre-line break-words w-full mt-[5px]" :style="{ lineHeight: '1.6' }" v-html="formatDesc(item.desc)"></div>
                </div>

                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 opacity-0 group-hover:opacity-100 transition-opacity absolute right-[20px] top-[5px] w-[18px] h-[18px]">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>
      </div>
    </aside>
    
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
        const colItems = col.classList.contains('paginated-item') 
            ? [col] 
            : col.querySelectorAll('.paginated-item');

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
    
    if (pageCount.value !== calculatedPageCount) {
        pageCount.value = calculatedPageCount;
    }
};

watch(() => props.resumeData, () => {
    requestPagination();
}, { deep: true });

onMounted(() => {
    const activeMain = ['experience', 'education'];
    const activeSidebar = ['awards', 'skills'];
    const hideSections = ['project', 'activities', 'languages', 'it_skills'];
    
    if (props.resumeData && props.resumeData.sections) {
        props.resumeData.sections.forEach(sec => {
            if (activeMain.includes(sec.id)) {
                sec.isVisible = true;
                sec.column = 'left';
            } else if (activeSidebar.includes(sec.id)) {
                sec.isVisible = true;
                sec.column = 'right';
            } else if (hideSections.includes(sec.id)) {
                sec.isVisible = false;
            }
        });
        
        const summary = props.resumeData.sections.find(s => s.id === 'summary');
        if (summary) {
            summary.isVisible = true;
            summary.column = 'right';
        }
    }

    requestPagination();
    window.addEventListener('resize', requestPagination);
    document.addEventListener('keyup', requestPagination);
});

onUnmounted(() => {
    window.removeEventListener('resize', requestPagination);
    document.removeEventListener('keyup', requestPagination);
    if (paginateTimer) clearTimeout(paginateTimer);
});

const isEmpty = (val) => {
    if (!val) return true;
    if (typeof val !== 'string') return false;
    const cleanText = val.replace(/<[^>]*>/g, '').trim();
    return cleanText === '';
};

const props = defineProps({
    resumeData: { type: Object, required: true }
})

const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

const hoveredSectionId = ref(null)
let _hideTimer = null

const showNav = (id) => {
    if (_hideTimer) { clearTimeout(_hideTimer); _hideTimer = null; }
    hoveredSectionId.value = id;
}

const hideNav = () => {
    _hideTimer = setTimeout(() => {
        hoveredSectionId.value = null;
        _hideTimer = null;
    }, 120);
}

const selectedSectionId = ref(null)

const summarySection = computed(() => props.resumeData.sections.find(s => s.id === 'summary'));

// Lưu ý: Đảo ngược logic cột. Trong mẫu này:
// Cột trái là Main Column (Học vấn, Kinh nghiệm) -> s.column === 'left' (hoặc map với right nếu data gốc ngược)
// Cột phải là Sidebar Column (Avatar, Skill, Contact) -> s.column === 'right'
// Để đảm bảo data render đúng nếu User đã nhập dữ liệu, ta cần map linh hoạt:
const mainSections = computed(() => {
    const order = ['experience', 'education'];
    return props.resumeData.sections
        .filter(s => s.column === 'left' && s.id !== 'summary')
        .sort((a, b) => {
            const indexA = order.indexOf(a.id);
            const indexB = order.indexOf(b.id);
            if (indexA === -1 && indexB === -1) return 0;
            if (indexA === -1) return 1;
            if (indexB === -1) return -1;
            return indexA - indexB;
        });
});

const sidebarSections = computed(() => {
    // Sắp xếp các mục sidebar theo thứ tự: awards -> skills
    const order = ['awards', 'skills'];
    return props.resumeData.sections
        .filter(s => s.column === 'right' && s.id !== 'summary')
        .sort((a, b) => {
            const indexA = order.indexOf(a.id);
            const indexB = order.indexOf(b.id);
            if (indexA === -1 && indexB === -1) return 0;
            if (indexA === -1) return 1;
            if (indexB === -1) return -1;
            return indexA - indexB;
        });
});

const mainIds = computed(() => mainSections.value.map(s => s.id));
const sidebarIds = computed(() => sidebarSections.value.map(s => s.id));

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
}
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

/* HTML CONTENT CHO CỘT MAIN (Chữ nâu) */
:deep(.html-content) { margin: 0 !important; padding: 0 !important; }
:deep(.html-content p) { margin: 0 !important; padding: 0 !important; }
:deep(.html-content ul) { list-style-type: disc !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content ol) { list-style-type: decimal !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content b), :deep(.html-content strong) { font-weight: bold; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic; }
:deep(.html-content u) { text-decoration: underline; }
:deep(.html-content ul li), :deep(.html-content ol li) { margin-bottom: 2px !important; }

/* HTML CONTENT CHO CỘT SIDEBAR (Chữ trắng/sáng) */
:deep(.html-content-sidebar) { margin: 0 !important; padding: 0 !important; color: #e8e8e8 !important; }
:deep(.html-content-sidebar p) { margin: 0 !important; padding: 0 !important; }
:deep(.html-content-sidebar ul) { list-style-type: disc !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content-sidebar ol) { list-style-type: decimal !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content-sidebar b), :deep(.html-content-sidebar strong) { font-weight: bold; color: #fdf5e6 !important;}
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
    border: 2px solid transparent; 
    cursor: pointer;
    transition: all 0.15s ease;
}

.section-block:hover {
    background: rgba(0, 0, 0, 0.03);
}

.section-selected {
    border-color: var(--sel-color, #8b7355) !important;
    border-style: solid !important;
}

/* === NAV BUTTONS === */
.nav-btns {
    position: absolute;
    right: 10px;
    top: 10px;
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

.nav-btn:hover { background: #1d4ed8; }
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