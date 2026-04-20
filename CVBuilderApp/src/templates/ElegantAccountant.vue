<template>
  <div id="cv-printable-area" ref="cvRoot" class="relative box-border bg-white overflow-hidden text-[#333]" :style="{ width: '210mm', height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Segoe UI\', Tahoma, Geneva, Verdana, sans-serif' }">
    
<div class="decor-top-right absolute top-[10px] right-[20px] w-[100px] h-[100px] opacity-50 pointer-events-none no-print"></div>

    <div class="w-full h-full box-border flex flex-col relative z-10" :style="{ padding: '50px' }">
      
      <header class="paginated-item w-full flex justify-between items-start mb-[25px]">
        
        <div class="flex-[1.5] border-t border-[#ddd] pt-[15px]">
          <h1 class="m-0 leading-none break-words uppercase" :style="{ fontSize: '55px !important', color: templatePrimaryColor, fontFamily: 'Georgia, serif', fontWeight: 'normal' }" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'HỌ VÀ TÊN'"></h1>
          
          <div class="mt-[20px] text-[#333]" :style="{ fontSize: '14.5px !important', lineHeight: '1.8' }">
            <p v-if="!isEmpty(resumeData.general.dob)" class="m-0 p-0"><strong class="font-bold">Ngày sinh:</strong> <span v-html="resumeData.general.dob"></span></p>
            <p v-if="!isEmpty(resumeData.general.address)" class="m-0 p-0"><strong class="font-bold">Địa chỉ:</strong> <span v-html="resumeData.general.address"></span></p>
            <p v-if="!isEmpty(resumeData.general.email)" class="m-0 p-0"><strong class="font-bold">Email:</strong> <span v-html="resumeData.general.email"></span></p>
            <p v-if="!isEmpty(resumeData.general.phone)" class="m-0 p-0"><strong class="font-bold">Số điện thoại:</strong> <span v-html="resumeData.general.phone"></span></p>
            <p v-if="!isEmpty(resumeData.general.website) || !isEmpty(resumeData.general.github) || !isEmpty(resumeData.general.linkedin)" class="m-0 p-0">
                <strong class="font-bold">Liên kết:</strong> 
                <span v-html="!isEmpty(resumeData.general.github) ? resumeData.general.github : (!isEmpty(resumeData.general.linkedin) ? resumeData.general.linkedin : resumeData.general.website)"></span>
            </p>
          </div>
        </div>

        <div class="flex-1 flex justify-center mt-[-10px]">
          <div class="relative rounded-full overflow-hidden bg-[#eee] shrink-0" :style="{ width: '180px', height: '180px' }">
            <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
            <div v-else class="w-full h-full flex items-center justify-center bg-gray-100 text-gray-400">
              <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
            </div>
          </div>
        </div>

        <div class="flex-1 text-right border-t border-[#333] pt-[15px]">
          <h2 class="m-0 uppercase break-words" :style="{ fontSize: '28px !important', color: '#333', lineHeight: '1.1', fontWeight: 'bold' }" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'VỊ TRÍ ỨNG TUYỂN'"></h2>
        </div>

      </header>

      <div v-if="summarySection && summarySection.isVisible" class="paginated-item relative group w-full" :class="{ 'section-selected': selectedSectionId === summarySection.id }" @click.stop="selectedSectionId = selectedSectionId === summarySection.id ? null : summarySection.id">
        <h3 class="uppercase block" :style="{ fontSize: '24px !important', color: '#222', margin: '35px 0 15px 0 !important', fontWeight: 'bold !important' }">
          {{ summarySection.title || 'Mục tiêu nghề nghiệp' }}
        </h3>
        <div class="html-content text-justify whitespace-pre-line w-full text-[#333]" :style="{ fontSize: '15.5px !important', lineHeight: '1.7 !important', margin: '0 !important', padding: '0 !important' }" v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Tôi là một ứng viên năng động, có tư duy logic tốt và khả năng thích nghi cao...'"></div>
      </div>

      <div class="cv-body-content w-full flex flex-col m-0 p-0">
        <template v-for="section in allVisibleSections" :key="section.id">
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
              <button @click.stop.prevent="$emit('moveUp', section.id, section.column === 'left' ? sidebarIds : mainIds)" class="nav-btn" title="Di chuyển lên"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
              <button @click.stop.prevent="$emit('moveDown', section.id, section.column === 'left' ? sidebarIds : mainIds)" class="nav-btn" title="Di chuyển xuống"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, section.column === 'left' ? 'right' : 'left')" class="nav-btn" title="Đổi Cột (Dữ liệu)"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4"/></svg></button>
            </div>

            <h3 class="uppercase w-full block break-words" :style="{ fontSize: '24px !important', color: '#222', margin: '35px 0 15px 0 !important', fontWeight: 'bold !important' }">
              {{ section.title }}
            </h3>

            <div v-if="section.id === 'experience' || section.id === 'project' || section.id === 'activities'" class="grid grid-cols-2 gap-x-[50px] gap-y-[25px] w-full items-start">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full text-[#333] break-words">
                
                <div class="font-bold text-[#333] block" :style="{ fontSize: '17px !important', marginBottom: '5px' }" v-if="item.year || item.time">{{ item.year || item.time }}</div>
                <div class="font-bold text-[#333] text-[15.5px] leading-tight mb-[2px] w-full" v-html="item.company || item.name || item.school"></div>
                <div class="italic text-[#555] text-[14.5px] w-full mb-[5px]" v-if="item.role">{{ item.role }}</div>
                
                <div class="html-content text-justify whitespace-pre-line w-full text-[#333]" :style="{ fontSize: '15px !important', lineHeight: '1.6 !important' }" v-if="item.desc" v-html="formatDesc(item.desc)"></div>
                
                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 absolute right-[-15px] top-0 w-[18px] h-[18px]">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <div v-else-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'" class="grid grid-cols-2 gap-x-[60px] gap-y-[20px] w-full">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full flex flex-col">
                <span class="italic text-[#333] block" :style="{ fontSize: '15.5px !important', marginBottom: '8px' }">{{ item.name }}</span>
                
                <div class="w-full bg-[#e0e6ed] rounded-[5px] overflow-hidden" :style="{ height: '10px' }" v-if="item.level && !isNaN(parseInt(item.level))">
                    <div class="h-full rounded-[5px] transition-all" :style="{ width: parseInt(item.level) + '%', backgroundColor: templatePrimaryColor }"></div>
                </div>
                <div v-else-if="item.level || item.info" class="text-[14.5px] text-[#555]">{{ item.level || item.info }}</div>

                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 absolute right-[-15px] top-0 w-[18px] h-[18px]">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <div v-else class="flex flex-col gap-[20px] w-full">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full text-[#333]">
                
                <div class="w-full flex flex-col mb-[5px]">
                  <div class="font-bold text-[#333] leading-tight" :style="{ fontSize: '16.5px !important' }">
                    <span v-html="item.school || item.name || item.company"></span>
                    <span class="font-normal text-[#666] ml-[5px]" v-if="item.year || item.time">({{ item.year || item.time }})</span>
                  </div>
                  <div class="italic text-[#555]" :style="{ fontSize: '15.5px !important', marginTop: '3px' }" v-if="item.major || item.role">{{ item.major || item.role }}</div>
                </div>
                
                <div class="html-content text-justify whitespace-pre-line w-full text-[#333]" :style="{ fontSize: '15.5px !important', lineHeight: '1.7 !important' }" v-html="formatDesc(item.desc || item.info)"></div>
                
                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 absolute right-[-15px] top-0 w-[18px] h-[18px]">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

          </div>
        </template>
      </div>

    </div>
    
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

// --- CHỨC NĂNG PHÂN TRANG (PAGINATION ENGINE CHO 1 CỘT) ---
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

    let isStable = false;
    let attempts = 0;
    
    while (!isStable && attempts < 50) {
        isStable = true;
        attempts++;
        
        for (let i = 0; i < items.length; i++) {
            const item = items[i];
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
    // Tự động bật các trường mặc định theo thiết kế
    const templateSpecificSections = ['experience', 'education', 'skills'];
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
// --- END PAGINATION ENGINE ---

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

// Màu chủ đạo đặc trưng của CV: Xanh xám (#556b8d)
const templatePrimaryColor = computed(() => {
    if (!props.resumeData.theme.primaryColor || props.resumeData.theme.primaryColor.toLowerCase() === '#0d6efd') {
        return '#FFFFFF'; 
    }
    return props.resumeData.theme.primaryColor;
});

// Tách Summary ra để hiển thị riêng trên Top
const summarySection = computed(() => props.resumeData.sections.find(s => s.id === 'summary'));

// Gom tất cả các section lại để hiển thị dạng 1 cột từ trên xuống
const sidebarSections = computed(() => props.resumeData.sections.filter(s => s.column === 'left' && s.id !== 'summary'));
const mainSections = computed(() => props.resumeData.sections.filter(s => s.column === 'right' && s.id !== 'summary'));

const allVisibleSections = computed(() => {
    return [...sidebarSections.value, ...mainSections.value].filter(s => s.isVisible);
});

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

/* === HTML CONTENT SUPPORT - Đảm bảo margin 0 === */
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
    padding-left: 1.5rem !important;
    margin: 0 !important;
}
:deep(.html-content ol) {
    list-style-type: decimal !important;
    padding-left: 1.5rem !important;
    margin: 0 !important;
}
:deep(.html-content b), :deep(.html-content strong) { font-weight: bold; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic; }
:deep(.html-content u) { text-decoration: underline; }
:deep(.html-content ul li), :deep(.html-content ol li) { margin-bottom: 2px !important; }

:deep(.html-content li:has(> font[size="1"])) { font-size: 11px; }
:deep(.html-content li:has(> font[size="2"])) { font-size: 13px; }
:deep(.html-content li:has(> font[size="3"])) { font-size: 15px; }
:deep(.html-content li:has(> font[size="4"])) { font-size: 17px; }

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
    outline: 1.5px solid var(--sel-color, #556b8d);
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