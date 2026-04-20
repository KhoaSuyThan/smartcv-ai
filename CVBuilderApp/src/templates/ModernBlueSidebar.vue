<template>
  <div id="cv-printable-area" ref="cvRoot" class="flex flex-row relative box-border bg-white overflow-hidden text-[#333]" :style="{ width: '210mm', height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Segoe UI\', Tahoma, Geneva, Verdana, sans-serif', lineHeight: '1.3' }">
    
    <aside class="z-10 flex flex-col shrink-0 relative box-border border-r border-[#eee]" :style="{ width: '32%', backgroundColor: templateSecondaryColor, padding: '15px' }">
      
      <div class="paginated-item relative z-20 w-full flex flex-col items-center">
        <div class="relative rounded-full overflow-hidden mx-auto bg-white" :style="{ width: '120px', height: '120px', border: '3px solid white', marginBottom: '8px', boxShadow: '0 4px 10px rgba(0,0,0,0.1)' }">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center bg-gray-100 text-gray-400">
            <svg class="w-12 h-12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
          </div>
        </div>

        <ul class="w-full list-none p-0 m-0 text-[#333] flex flex-col" :style="{ fontSize: '11.5px !important', marginBottom: '10px' }">
          
          <li v-if="!isEmpty(resumeData.general.phone)" class="flex items-center relative" :style="{ marginBottom: '3px' }">
            <div class="w-[18px] text-center shrink-0 mr-[6px]" :style="{ color: templatePrimaryColor }">
              <svg class="w-[12px] h-[12px] inline-block" fill="currentColor" viewBox="0 0 20 20"><path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z"></path></svg>
            </div>
            <span class="break-all" v-html="resumeData.general.phone"></span>
          </li>
          
          <li v-if="!isEmpty(resumeData.general.email)" class="flex items-center relative" :style="{ marginBottom: '3px' }">
            <div class="w-[18px] text-center shrink-0 mr-[6px]" :style="{ color: templatePrimaryColor }">
              <svg class="w-[12px] h-[12px] inline-block" fill="currentColor" viewBox="0 0 20 20"><path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z"></path><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z"></path></svg>
            </div>
            <span class="break-all" v-html="resumeData.general.email"></span>
          </li>

          <li v-if="!isEmpty(resumeData.general.dob)" class="flex items-center relative" :style="{ marginBottom: '3px' }">
            <div class="w-[18px] text-center shrink-0 mr-[6px]" :style="{ color: templatePrimaryColor }">
              <svg class="w-[12px] h-[12px] inline-block" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" clip-rule="evenodd"></path></svg>
            </div>
            <span class="break-all" v-html="resumeData.general.dob"></span>
          </li>

          <li v-if="!isEmpty(resumeData.general.address)" class="flex items-center relative" :style="{ marginBottom: '3px' }">
            <div class="w-[18px] text-center shrink-0 mr-[6px]" :style="{ color: templatePrimaryColor }">
              <svg class="w-[12px] h-[12px] inline-block" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd"></path></svg>
            </div>
            <span class="break-all" v-html="resumeData.general.address"></span>
          </li>
          
          <li v-if="!isEmpty(resumeData.general.github) || !isEmpty(resumeData.general.website) || !isEmpty(resumeData.general.linkedin)" class="flex items-center relative" :style="{ marginBottom: '3px' }">
            <div class="w-[18px] text-center shrink-0 mr-[6px]" :style="{ color: templatePrimaryColor }">
              <svg class="w-[12px] h-[12px] inline-block" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM4.332 8.027a6.012 6.012 0 011.912-2.706C6.512 5.73 6.974 6 7.5 6A1.5 1.5 0 019 7.5V8a2 2 0 004 0 2 2 0 011.523-1.943A5.977 5.977 0 0116 10c0 .34-.028.675-.083 1H15a2 2 0 00-2 2v2.197A5.973 5.973 0 0110 16v-2a2 2 0 00-2-2 2 2 0 01-2-2 2 2 0 01-2-2 2 2 0 00-1.668-1.973z" clip-rule="evenodd"></path></svg>
            </div>
            <span class="break-all" v-html="!isEmpty(resumeData.general.github) ? resumeData.general.github : (!isEmpty(resumeData.general.linkedin) ? resumeData.general.linkedin : resumeData.general.website)"></span>
          </li>
        </ul>
      </div>

      <div class="w-full flex-1 flex flex-col m-0 p-0">
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
            <div v-show="hoveredSectionId === section.id || selectedSectionId === section.id" class="nav-btns no-print" @mouseenter="showNav(section.id)" @mouseleave="hideNav()">
              <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
              <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7-7"/></svg></button>
            </div>

            <h3 class="uppercase block" :style="{ fontSize: '14px !important', color: templatePrimaryColor, borderBottom: `1px solid ${templatePrimaryColor}`, paddingBottom: '3px !important', margin: '3px 0 2px 0 !important', marginLeft: '12px !important', marginRight: '12px !important', width: 'calc(100% - 24px)', fontWeight: 'bold !important' }">
              {{ section.title }}
            </h3>
            
            <div class="w-full">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative text-[#333] w-full" :style="{ fontSize: '12.5px !important', margin: '0 !important', padding: '0 0 0 12px !important' }">
                
                <div v-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'" class="flex flex-col">
                  <div class="font-bold w-full break-words" :style="{ margin: '0 !important' }">{{ item.name }}</div>
                  <div v-if="item.info || item.level" class="font-normal w-full break-words" :style="{ margin: '12px 0 0 0 !important' }">{{ item.info || item.level }}</div>
                </div>

                <div v-else class="html-content break-words whitespace-pre-line text-justify w-full" :style="{ margin: '0 !important' }" v-html="formatDesc(item.desc || item.name || item.info || item.school || item.company)"></div>

                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 opacity-0 group-hover:opacity-100 transition-opacity absolute right-0 top-0">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>
      </div>
    </aside>

    <main class="flex-1 flex flex-col relative bg-white z-20 overflow-hidden box-border" :style="{ padding: '15px 35px' }" @click.self="selectedSectionId = null">
      
      <header class="paginated-item w-full flex flex-col relative">
        <h1 class="uppercase text-[#333] break-words w-full" :style="{ margin: '8px 0 0 0 !important', padding: '0 12px 0 12px !important', fontSize: '42px !important', fontWeight: '800 !important', lineHeight: '1 !important' }" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'HỌ VÀ TÊN ỨNG VIÊN'"></h1>
        <h2 class="uppercase text-[#666] break-words w-full" :style="{ margin: '0 0 3px 0 !important', padding: '0 12px 0 12px !important', fontSize: '18px !important', fontWeight: 'normal !important', letterSpacing: '1px' }" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'VỊ TRÍ ỨNG TUYỂN'"></h2>
      </header>

      <div v-if="summarySection && summarySection.isVisible" class="paginated-item relative group w-full" :class="{ 'section-selected': selectedSectionId === summarySection.id }" @click.stop="selectedSectionId = selectedSectionId === summarySection.id ? null : summarySection.id">
        <h3 class="uppercase block" :style="{ fontSize: '14px !important', color: templatePrimaryColor, borderBottom: `1px solid ${templatePrimaryColor}`, paddingBottom: '3px !important', margin: '3px 0 2px 0 !important', marginLeft: '12px !important', marginRight: '12px !important', width: 'calc(100% - 24px)', fontWeight: 'bold !important' }">
          {{ summarySection.title || 'Mục tiêu nghề nghiệp' }}
        </h3>
        <div class="html-content text-justify whitespace-pre-line w-full text-[#333]" :style="{ fontSize: '12.5px !important', margin: '2px 0 !important', padding: '0 12px 0 12px !important' }" v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Tôi là một ứng viên năng động...'"></div>
      </div>

      <div class="w-full flex flex-col flex-1 m-0 p-0">
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
            <div v-show="hoveredSectionId === section.id || selectedSectionId === section.id" class="nav-btns no-print" @mouseenter="showNav(section.id)" @mouseleave="hideNav()">
              <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
              <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
            </div>

            <h3 class="uppercase block" :style="{ fontSize: '14px !important', color: templatePrimaryColor, borderBottom: `1px solid ${templatePrimaryColor}`, paddingBottom: '3px !important', margin: '3px 0 2px 0 !important', marginLeft: '12px !important', marginRight: '12px !important', width: 'calc(100% - 24px)', fontWeight: 'bold !important' }">
              {{ section.title }}
            </h3>

            <div class="w-full">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full text-[#333]" :style="{ fontSize: '12.5px !important', margin: '1px 0 !important', padding: '0 12px 0 12px !important' }">
                
                <div v-if="section.id === 'education' || section.id === 'experience' || section.id === 'project' || section.id === 'activities'" class="w-full">
                  <div class="w-full flex flex-col">
                    <span class="font-bold w-full break-words text-[#333] leading-tight" :style="{ margin: '0 !important' }" v-html="section.id === 'education' ? item.school : (item.company || item.name)"></span>
                    
                    <div class="w-full flex justify-between items-baseline gap-2 text-[#555]" :style="{ margin: '0 !important' }">
                        <span class="italic break-words flex-1 leading-tight" :style="{ margin: '0 !important' }" v-if="item.major || item.role">{{ item.major || item.role }}</span>
                        <span class="font-medium shrink-0 whitespace-nowrap text-right" v-if="item.year || item.time" :style="{ color: templatePrimaryColor, fontSize: '11.5px', margin: '0 !important' }">{{ item.year || item.time }}</span>
                    </div>
                  </div>
                  
                  <div v-if="item.desc" class="html-content text-justify whitespace-pre-line break-words w-full" :style="{ margin: '0 !important', paddingRight: '0 !important' }" v-html="formatDesc(item.desc)"></div>
                  <div v-else-if="item.gradType" class="font-medium" :style="{ color: templatePrimaryColor, fontSize: '11px !important', margin: '0 !important' }">{{ item.gradType }}</div>
                </div>

                <div v-else class="w-full">
                  <div v-if="item.year || item.time" class="font-bold" :style="{ color: templatePrimaryColor, fontSize: '11.5px', margin: '0 !important' }">{{ item.year || item.time }}</div>
                  <div class="html-content text-justify whitespace-pre-line break-words w-full" :style="{ margin: '0 !important', paddingRight: '0 !important' }" v-html="formatDesc(item.desc || item.name || item.info)"></div>
                </div>

                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 absolute right-[-15px] top-0 w-[16px] h-[16px]">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>
      </div>
    </main>
    
    <template v-for="p in pageCount" :key="'footer-border-'+p">
        <div class="absolute left-0 w-full flex items-center z-40 pointer-events-none" 
             :style="{ top: `calc(${p * 297}mm - 12mm)`, height: '1.5px', paddingLeft: '15px', paddingRight: '15px' }">
            <div class="w-full h-full opacity-20 bg-gradient-to-r from-transparent via-[#0d6efd] to-transparent" :style="{ backgroundImage: `linear-gradient(to right, transparent, ${templatePrimaryColor}, transparent)` }"></div>
        </div>
    </template>

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
    const MARGIN_BOTTOM_MM = 15; 
    const MARGIN_TOP_MM = 15;    
    
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
    const templateSpecificSections = ['education', 'experience', 'skills', 'languages', 'awards', 'references'];
    if (props.resumeData && props.resumeData.sections) {
        props.resumeData.sections.forEach(sec => {
            if (templateSpecificSections.includes(sec.id) && !sec.isVisible) {
                sec.isVisible = true;
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

const templatePrimaryColor = computed(() => {
    if (!props.resumeData.theme.primaryColor) {
        return '#0d6efd'; 
    }
    return props.resumeData.theme.primaryColor;
});

const templateSecondaryColor = computed(() => {
    return '#f7f9fc';
});

const summarySection = computed(() => props.resumeData.sections.find(s => s.id === 'summary'));
const sidebarSections = computed(() => props.resumeData.sections.filter(s => s.column === 'left' && s.id !== 'summary'));
const mainSections = computed(() => props.resumeData.sections.filter(s => s.column === 'right' && s.id !== 'summary'));

const sidebarIds = computed(() => sidebarSections.value.map(s => s.id));
const mainIds = computed(() => mainSections.value.map(s => s.id));

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

/* KHÔNG CÒN margin HAY padding NÀO Ở ĐÂY ĐỂ ĐẢM BẢO THẲNG TẮP LỀ TRÁI */
:deep(.html-content) {
    margin: 0 !important;
    padding: 0 !important;
}
:deep(.html-content p) {
    margin: 0 !important;
    padding: 0 !important;
}
:deep(.html-content ul) {
    list-style-type: disc !important;
    padding-left: 1.25rem !important; /* Ngoại trừ dấu chấm của list */
    margin: 0 !important;
}
:deep(.html-content ol) {
    list-style-type: decimal !important;
    padding-left: 1.25rem !important;
    margin: 0 !important;
}
:deep(.html-content b), :deep(.html-content strong) { font-weight: bold; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic; }
:deep(.html-content u) { text-decoration: underline; }
:deep(.html-content ul li), :deep(.html-content ol li) { margin-bottom: 0px !important; }

:deep(.html-content li:has(> font[size="1"])) { font-size: 10px; }
:deep(.html-content li:has(> font[size="2"])) { font-size: 11px; }
:deep(.html-content li:has(> font[size="3"])) { font-size: 12.5px; }
:deep(.html-content li:has(> font[size="4"])) { font-size: 14px; }

/* === SECTION BLOCKS === */
.section-block {
    position: relative;
    border: 2px solid transparent; 
    cursor: pointer;
    transition: all 0.15s ease;
}

.section-block:hover {
    background: rgba(0, 0, 0, 0.02);
}

.section-selected {
    outline: 1.5px solid var(--sel-color, #0d6efd);
    outline-offset: 1mm;
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