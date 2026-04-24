<template>
  <div id="cv-printable-area" ref="cvRoot" class="relative box-border bg-white overflow-hidden text-[#333]" :style="{ width: '210mm', height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Segoe UI\', Tahoma, Geneva, Verdana, sans-serif' }">
    
    <!-- Họa tiết trang trí góc trên phải (Curly line) -->
    <div class="absolute top-0 right-0 w-[200px] h-[200px] pointer-events-none no-print opacity-80">
      <svg width="200" height="200" viewBox="0 0 200 200" fill="none" xmlns="http://www.w3.org/2000/svg">
        <path d="M180 20C160 50 140 30 120 60C100 90 130 110 110 140C90 170 60 150 40 180" stroke="#333" stroke-width="1" stroke-linecap="round" stroke-dasharray="2 4"/>
        <path d="M190 30C175 55 160 45 145 70C130 95 150 110 135 135" stroke="#333" stroke-width="0.5" opacity="0.4"/>
        <circle cx="180" cy="20" r="2" fill="#333"/>
      </svg>
    </div>

    <!-- Họa tiết trang trí góc dưới phải (Circles) -->
    <div class="absolute bottom-[50px] right-[50px] w-[150px] h-[100px] pointer-events-none no-print opacity-20">
      <svg width="150" height="100" viewBox="0 0 150 100" fill="none" xmlns="http://www.w3.org/2000/svg">
        <ellipse cx="40" cy="50" rx="35" ry="45" stroke="#333" stroke-width="0.5" stroke-dasharray="4 2"/>
        <ellipse cx="75" cy="50" rx="35" ry="45" stroke="#333" stroke-width="0.5" stroke-dasharray="4 2"/>
        <ellipse cx="110" cy="50" rx="35" ry="45" stroke="#333" stroke-width="0.5" stroke-dasharray="4 2"/>
      </svg>
    </div>

    <div class="w-full h-full box-border flex flex-col relative z-10" :style="{ padding: '40px 50px' }">
      
      <!-- HEADER 3 CỘT -->
      <header class="paginated-item w-full flex justify-between items-start mb-[40px] gap-[20px]">
        
        <!-- Cột 1: Tên và Thông tin cá nhân -->
        <div class="flex-[1.4] flex flex-col">
          <div class="w-fit mb-[25px]">
            <h1 class="m-0 leading-tight break-words" :style="{ fontSize: '64px !important', color: '#4a5568', fontFamily: 'Georgia, serif', fontWeight: '400' }" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'Neil Tran'"></h1>
            <div class="h-[1px] w-full bg-[#cbd5e0] mt-[5px]"></div>
          </div>
          
          <div class="flex flex-col gap-[8px] text-[#333]" :style="{ fontSize: '14px !important' }">
            <div v-if="!isEmpty(resumeData.general.dob)" class="flex gap-[5px]"><span class="font-bold shrink-0">Ngày sinh:</span> <span v-html="resumeData.general.dob"></span></div>
            <div v-if="!isEmpty(resumeData.general.address)" class="flex gap-[5px]"><span class="font-bold shrink-0">Địa chỉ:</span> <span v-html="resumeData.general.address"></span></div>
            <div v-if="!isEmpty(resumeData.general.email)" class="flex gap-[5px]"><span class="font-bold shrink-0">Email:</span> <span v-html="resumeData.general.email"></span></div>
            <div v-if="!isEmpty(resumeData.general.phone)" class="flex gap-[5px]"><span class="font-bold shrink-0">Số điện thoại:</span> <span v-html="resumeData.general.phone"></span></div>
          </div>
        </div>

        <!-- Cột 2: Ảnh đại diện tròn -->
        <div class="flex-1 flex justify-center pt-[20px]">
          <div class="relative rounded-full overflow-hidden bg-[#f7fafc] border-[12px] border-white shadow-sm shrink-0" :style="{ width: '190px', height: '190px' }">
            <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
            <div v-else class="w-full h-full flex items-center justify-center bg-gray-100 text-gray-400">
              <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
            </div>
          </div>
        </div>

        <!-- Cột 3: Vị trí ứng tuyển -->
        <div class="flex-1 text-right flex flex-col items-end pt-[50px]">
          <div class="w-fit">
            <h2 class="m-0 break-words leading-tight" :style="{ fontSize: '32px !important', color: '#333', fontWeight: '400' }" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'Chuyên Viên Kế Toán'"></h2>
            <div class="h-[1px] w-full bg-[#333] mt-[10px]"></div>
          </div>
        </div>

      </header>

      <!-- CÁC SECTION CHÍNH -->
      <div class="cv-body-content w-full flex flex-col m-0 p-0">
        <template v-for="section in allVisibleSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block relative paginated-item group w-full mb-[30px]"
            :class="{ 'section-selected': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--sel-color': templatePrimaryColor } : {}"
            @mouseenter="showNav(section.id)"
            @mouseleave="hideNav()"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <!-- Navigation Buttons -->
            <div v-show="hoveredSectionId === section.id || selectedSectionId === section.id" class="nav-btns no-print" @mouseenter="showNav(section.id)" @mouseleave="hideNav()">
              <button @click.stop.prevent="$emit('moveUp', section.id, section.column === 'left' ? sidebarIds : mainIds)" class="nav-btn" title="Di chuyển lên"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
              <button @click.stop.prevent="$emit('moveDown', section.id, section.column === 'left' ? sidebarIds : mainIds)" class="nav-btn" title="Di chuyển xuống"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, section.column === 'left' ? 'right' : 'left')" class="nav-btn" title="Đổi Cột (Dữ liệu)"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4"/></svg></button>
            </div>

            <h3 class="uppercase w-full block break-words border-b-0" :style="{ fontSize: '28px !important', color: '#222', marginBottom: '15px !important', fontWeight: 'bold !important', letterSpacing: '1px' }">
              {{ section.title }}
            </h3>

            <!-- LAYOUT LINH HOẠT CHO KINH NGHIỆM LÀM VIỆC -->
            <div v-if="section.id === 'experience'" 
                 :class="['grid gap-x-[60px] gap-y-[25px] w-full items-start', section.items.length >= 2 ? 'grid-cols-2' : 'grid-cols-1']">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full text-[#333] break-words pl-[20px]">
                <div class="absolute left-0 top-[8px] w-[6px] h-[6px] rounded-full bg-black"></div>
                
                <div class="italic text-[#333] block mb-[5px]" :style="{ fontSize: '18px !important' }" v-if="item.year || item.time">{{ item.year || item.time }}</div>
                <div class="text-[16px] leading-tight mb-[3px] w-full">Công ty: {{ item.company || item.name }}</div>
                <div class="text-[16px] w-full mb-[3px]">Vị trí: {{ item.role || 'Nhân viên' }}</div>
                
                <div class="html-content text-justify whitespace-pre-line w-full text-[#333]" :style="{ fontSize: '15px !important', lineHeight: '1.5' }" v-if="item.desc">
                    <span class="block">Mô tả:</span>
                    <div v-html="formatDesc(item.desc)"></div>
                </div>
                
                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 absolute right-[-5px] top-0 w-[18px] h-[18px]">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- LAYOUT CHO KỸ NĂNG (Hỗ trợ nhiều loại kỹ năng) -->
            <div v-else-if="section.id === 'skills' || section.id === 'it_skills' || section.id === 'languages'" class="grid grid-cols-2 gap-x-[80px] gap-y-[25px] w-full">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full flex flex-col pl-[20px]">
                <div class="absolute left-0 top-[10px] w-[6px] h-[6px] rounded-full bg-black"></div>
                <div class="flex justify-between items-end mb-[10px]">
                    <span class="italic text-[#333] block leading-tight" :style="{ fontSize: '18px !important' }">{{ item.name }}</span>
                    <span class="text-[14px] font-bold text-[#8da9c4] whitespace-nowrap ml-2">
                        {{ getLevelInfo(item.level).text }}
                    </span>
                </div>
                
                <div class="w-full bg-[#cbd5e0] rounded-full overflow-hidden" :style="{ height: '10px' }">
                    <div class="h-full rounded-full transition-all duration-500" :style="{ width: getLevelInfo(item.level).percent, backgroundColor: '#8da9c4' }"></div>
                </div>

                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 absolute right-[-15px] top-0 w-[18px] h-[18px]">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- LAYOUT CHO HỌC VẤN -->
            <div v-else-if="section.id === 'education'" class="flex flex-col gap-[20px] w-full">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full text-[#333] pl-[20px]">
                <div class="absolute left-0 top-[10px] w-[6px] h-[6px] rounded-full bg-black"></div>
                <div class="italic text-[#333] mb-[5px]" :style="{ fontSize: '18px !important' }" v-if="item.year || item.time">{{ item.year || item.time }}</div>
                <div class="text-[16.5px] font-normal mb-[3px]" v-html="item.school || item.name"></div>
                <div class="text-[15.5px]" v-if="item.major || item.role">{{ item.major || item.role }}</div>
                <div v-if="item.desc" class="html-content text-[15px] mt-[5px]" v-html="formatDesc(item.desc)"></div>
                
                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 absolute right-[-15px] top-0 w-[18px] h-[18px]">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- CÁC MỤC KHÁC -->
            <div v-else class="flex flex-col gap-[15px] w-full">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full text-[#333] pl-[20px]">
                <div class="absolute left-0 top-[10px] w-[6px] h-[6px] rounded-full bg-black"></div>
                <div class="font-bold text-[16px] mb-[2px]" v-html="item.name || item.school || item.company"></div>
                <div class="html-content text-justify whitespace-pre-line w-full text-[#333]" :style="{ fontSize: '15px !important', lineHeight: '1.6' }" v-html="formatDesc(item.desc || item.info)"></div>
                
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

// --- CHỨC NĂNG PHÂN TRANG ---
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
    // Chỉ kích hoạt các section có trong ảnh mẫu
    const activeSections = ['experience', 'education', 'skills'];
    if (props.resumeData && props.resumeData.sections) {
        props.resumeData.sections.forEach(sec => {
            sec.isVisible = activeSections.includes(sec.id);
            // Mẫu này ưu tiên hiển thị 1 cột chính (không sidebar)
            sec.column = 'right'; 
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
    return props.resumeData.theme.primaryColor || '#4a5568';
});

const allVisibleSections = computed(() => {
    return props.resumeData.sections.filter(s => s.isVisible);
});

const sidebarIds = computed(() => props.resumeData.sections.filter(s => s.column === 'left').map(s => s.id));
const mainIds = computed(() => props.resumeData.sections.filter(s => s.column === 'right').map(s => s.id));

const formatDesc = (text) => {
    if (!text) return '';
    if (/<[a-z][\s\S]*>/i.test(text)) return text;
    return text.split('\n').map(l => l.trim()).filter(l=>l).join('<br/>');
}

const getLevelInfo = (level) => {
    if (!level) return { text: '', percent: '75%' };
    const l = level.toLowerCase().trim();
    if (l === 'cơ bản') return { text: 'Cơ bản', percent: '25%' };
    if (l === 'trung cấp') return { text: 'Trung cấp', percent: '50%' };
    if (l === 'thành thạo') return { text: 'Thành thạo', percent: '75%' };
    if (l === 'chuyên gia') return { text: 'Chuyên gia', percent: '100%' };
    
    if (level.includes('%')) return { text: level, percent: level };
    const num = parseInt(level);
    if (!isNaN(num)) return { text: num + '%', percent: num + '%' };
    
    return { text: level, percent: '75%' };
};
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

/* === HTML CONTENT SUPPORT === */
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

/* === SECTION BLOCKS === */
.section-block {
    position: relative;
    border: 1px solid transparent; 
    cursor: pointer;
    transition: all 0.15s ease;
}

.section-block:hover {
    background: rgba(0, 0, 0, 0.01);
}

.section-selected {
    outline: 1.5px solid var(--sel-color, #4a5568);
    outline-offset: 2mm;
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
}

.nav-btn:hover { background: #1d4ed8; transform: scale(1.1); }

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
</style>