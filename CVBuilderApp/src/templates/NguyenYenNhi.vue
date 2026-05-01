<template>
  <div id="cv-printable-area" ref="cvRoot" class="bg-white shadow-2xl w-[210mm] flex flex-col relative box-border text-[#333] leading-relaxed overflow-hidden" :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', \'Segoe UI\', sans-serif' }">
    
    <!-- NỀN TRANG TRÍ (Background Shapes) -->
    <div class="absolute top-[-50mm] right-[-50mm] w-[150mm] h-[150mm] rounded-full bg-pink-100/50 blur-[80px] z-0"></div>
    <div class="absolute top-[100mm] left-[-30mm] w-[100mm] h-[100mm] rounded-full bg-red-50/40 blur-[60px] z-0"></div>
    
    <!-- HEADER SECTION -->
    <header class="relative z-10 pt-[12mm] px-[15mm] pb-[4mm] flex items-start paginated-item">
        <div class="flex-grow mr-[10mm]">
            <!-- Typography Tên cách điệu như mẫu -->
            <div class="mb-2">
                <h1 class="text-[55px] font-black tracking-[0.05em] leading-[1.1] text-slate-900" 
                    style="font-family: 'Playfair Display', serif;">
                    {{ cleanedFullName }}
                </h1>
            </div>
            <h2 class="text-[12px] font-extrabold text-slate-700 uppercase tracking-[0.05em] mt-1.5" 
                v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'CHUYÊN VIÊN THIẾT KẾ ĐỒ HỌA'">
            </h2>
        </div>

        <!-- Avatar Section with enhanced decorations -->
        <div class="relative flex-shrink-0 mt-[-5mm] mr-4">
            <!-- Gradient glow -->
            <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[64mm] h-[64mm] rounded-full bg-gradient-to-br from-pink-400/30 to-red-300/20 blur-xl z-0"></div>
            <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[54mm] h-[54mm] rounded-full bg-gradient-to-br from-pink-500 to-red-400 opacity-90 z-0"></div>
            
            <!-- Circular Image -->
            <div class="w-[50mm] h-[50mm] rounded-full border-[6px] border-white shadow-2xl overflow-hidden bg-slate-100 relative z-10">
                <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
                <div v-else class="w-full h-full flex items-center justify-center text-slate-300">
                    <svg class="w-24 h-24" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
                </div>
            </div>
            
            <!-- Decorative SVG path lines -->
            <svg class="absolute -top-4 -right-4 w-[75mm] h-[75mm] z-20 pointer-events-none" viewBox="0 0 100 100">
                <path d="M75,15 Q95,45 70,65 T35,85" fill="none" stroke="#222" stroke-width="0.3" class="opacity-40" />
                <path d="M85,25 Q100,55 75,75 T45,95" fill="none" stroke="#222" stroke-width="0.2" class="opacity-30" />
                <!-- Biểu tượng ngôi sao lấp lánh -->
                <path d="M20,15 L22,21 L28,23 L22,25 L20,31 L18,25 L12,23 L18,21 Z" fill="currentColor" class="text-slate-900 opacity-80" />
            </svg>
        </div>
    </header>

    <!-- TRANG TRÍ DỌC (Side accent) -->
    <div class="absolute top-0 right-0 w-[4mm] h-[100%] bg-gradient-to-b from-pink-500 to-red-400 z-0 opacity-10 no-print"></div>

    <!-- CONTENT BODY (Dựa trên Sidebar/Main Split) -->
    <div class="flex px-[15mm] py-[2mm] gap-[10mm] relative z-10 mb-2" @click.self="selectedSectionId = null">
        
        <!-- CỘT TRÁI (SIDEBAR) -->
        <aside class="flex-[1.1] min-w-[75mm] flex flex-col gap-5">
            <template v-for="section in sidebarSections" :key="section.id">
                <div v-show="section.isVisible" 
                     class="section-block relative"
                     :class="{ 'section-selected': selectedSectionId === section.id }"
                     @mouseenter="showNav(section.id)"
                     @mouseleave="hideNav()"
                     @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id">
                    
                    <!-- Nút điều hướng -->
                    <div v-show="hoveredSectionId === section.id || selectedSectionId === section.id" 
                         class="nav-btns no-print">
                        <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên">
                            <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                        </button>
                        <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống">
                            <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                        </button>
                        <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải">
                            <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
                        </button>
                    </div>

                    <h3 class="font-black uppercase mb-3 tracking-wider flex items-center gap-2 text-slate-900 paginated-item" :style="{ fontSize: '20px !important', fontWeight: 'bold !important' }">
                        {{ section.title }}
                    </h3>

                    <div class="space-y-3">
                        <!-- Summary đặc biệt -->
                        <div v-if="section.id === 'summary'" 
                             class="text-[12.5px] leading-[1.6] text-slate-700 text-justify font-medium html-content paginated-item" 
                             v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Mô tả mục tiêu nghề nghiệp của bạn...'">
                        </div>

                        <!-- Học vấn -->
                        <div v-else-if="section.id === 'education'" class="space-y-4">
                            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="relative pl-6 paginated-item item-container">
                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print" style="top: 0; right: 0;">
                                    <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                                <div class="absolute left-0 top-1 text-pink-500">
                                    <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24"><path d="M12,2L14.5,9H21L15.5,13.5L18,20.5L12,16L6,20.5L8.5,13.5L3,9H9.5L12,2Z" /></svg>
                                </div>
                                <div class="text-[12px] font-black text-pink-500 mb-0.5 tracking-wide">{{ item.year || '2024 - 2028' }}</div>
                                <div class="text-[14px] font-black text-slate-900 leading-tight mb-0.5">{{ item.major || 'Chuyên ngành' }}</div>
                                <div class="text-[12px] font-bold text-slate-500 italic">{{ item.school || 'Tên trường học' }}</div>
                            </div>
                        </div>

                        <!-- Kỹ năng với Progress Bar phong cách Canva và nhãn mức độ -->
                        <div v-else-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'" class="space-y-2.5">
                            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container relative">
                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print" style="top: -5px; right: -5px;">
                                    <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                                
                                <div class="flex justify-between items-end mb-1">
                                    <span class="text-[12px] font-bold text-slate-800 leading-tight">{{ item.name }}</span>
                                    <span class="text-[10px] font-bold text-pink-500 whitespace-nowrap ml-2">
                                        {{ getLevelInfo(item.level).text }}
                                    </span>
                                </div>

                                <div class="w-full h-[5px] bg-slate-100 rounded-full overflow-hidden relative shadow-inner">
                                    <div class="absolute h-full left-0 top-0 rounded-full bg-gradient-to-r from-red-500 via-pink-400 to-red-400 shadow-sm transition-all duration-500" 
                                         :style="{ width: getLevelInfo(item.level).percent }"></div>
                                </div>
                            </div>
                        </div>

                        <!-- Các mục khác mặc định cho Sidebar -->
                        <div v-else class="space-y-2.5">
                            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container pl-6 relative">
                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print" style="top: -2px; right: -2px;">
                                    <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                                <div class="absolute left-0 top-1 text-pink-500">
                                    <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M12,2L14.5,9H21L15.5,13.5L18,20.5L12,16L6,20.5L8.5,13.5L3,9H9.5L12,2Z" /></svg>
                                </div>
                                <div class="text-[12.5px] font-bold text-slate-800" v-html="formatDesc(item.name || item.desc || item.info)"></div>
                            </div>
                        </div>
                    </div>
                </div>
            </template>
        </aside>

        <!-- CỘT PHẢI (MAIN) -->
        <main class="flex-1 min-w-[100mm] flex flex-col gap-5">
            <!-- LIÊN HỆ GỐC (Cố định ở đầu cột phải) -->
            <div class="section-block p-3 border-2 border-slate-50 rounded-2xl bg-white/50 backdrop-blur-sm paginated-item"
                 @click.stop="selectedSectionId = null">
                <h3 class="font-black uppercase mb-3 tracking-wider text-slate-900 flex items-center gap-3" :style="{ fontSize: '20px !important', fontWeight: 'bold !important' }">
                    LIÊN HỆ
                </h3>
                <div class="space-y-2.5 text-[12.5px] font-bold text-slate-700">
                    <div class="flex items-center gap-4 group" v-if="!isEmpty(resumeData.general.phone)">
                        <div class="w-7 h-7 rounded-full bg-pink-50 flex items-center justify-center text-pink-500 group-hover:bg-pink-500 group-hover:text-white transition-colors duration-300">
                            <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24"><path d="M6.62,10.79C8.06,13.62 10.38,15.94 13.21,17.38L15.41,15.18C15.69,14.9 16.08,14.82 16.43,14.93C17.55,15.3 18.75,15.5 20,15.5A1,1 0 0,1 21,16.5V20A1,1 0 0,1 20,21A17,17 0 0,1 3,4A1,1 0 0,1 4,3H7.5A1,1 0 0,1 8.5,4C8.5,5.25 8.7,6.45 9.07,7.57C9.18,7.92 9.1,8.31 8.82,8.59L6.62,10.79Z" /></svg>
                        </div>
                        <span v-html="resumeData.general.phone"></span>
                    </div>
                    <div class="flex items-center gap-4 group" v-if="!isEmpty(resumeData.general.email)">
                        <div class="w-7 h-7 rounded-full bg-pink-50 flex items-center justify-center text-pink-500 group-hover:bg-pink-500 group-hover:text-white transition-colors duration-300">
                            <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24"><path d="M20,4H4C2.89,4 2,4.89 2,6V18A2,2 0 0,0 4,20H20A2,2 0 0,0 22,18V6C22,4.89 21.1,4 20,4M20,8L12,13L4,8V6L12,11L20,6V8Z" /></svg>
                        </div>
                        <span v-html="resumeData.general.email"></span>
                    </div>
                    <div class="flex items-center gap-4 group" v-if="!isEmpty(resumeData.general.address)">
                        <div class="w-7 h-7 rounded-full bg-pink-50 flex items-center justify-center text-pink-500 group-hover:bg-pink-500 group-hover:text-white transition-colors duration-300">
                            <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24"><path d="M12,2C8.13,2 5,5.13 5,9C5,14.25 12,22 12,22C12,22 19,14.25 19,9C19,5.13 15.87,2 12,2M12,11.5A2.5,2.5 0 0,1 9.5,9A2.5,2.5 0 0,1 12,6.5A2.5,2.5 0 0,1 14.5,9A2.5,2.5 0 0,1 12,11.5Z" /></svg>
                        </div>
                        <span v-html="resumeData.general.address"></span>
                    </div>
                </div>
            </div>

            <!-- CÁC SECTION MAIN NHƯ KINH NGHIỆM -->
            <template v-for="section in mainSections" :key="section.id">
                <div v-show="section.isVisible" 
                     class="section-block relative"
                     :class="{ 'section-selected': selectedSectionId === section.id }"
                     @mouseenter="showNav(section.id)"
                     @mouseleave="hideNav()"
                     @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id">
                    
                    <!-- Nút điều hướng main column -->
                    <div v-show="hoveredSectionId === section.id || selectedSectionId === section.id" 
                         class="nav-btns no-print">
                        <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên">
                            <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                        </button>
                        <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống">
                            <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                        </button>
                        <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái">
                            <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
                        </button>
                    </div>

                    <h3 class="font-black uppercase mb-4 tracking-wider text-slate-900 border-b border-pink-50 pb-2 paginated-item" :style="{ fontSize: '20px !important', fontWeight: 'bold !important' }">
                        {{ section.title }}
                    </h3>

                    <div class="space-y-6 relative">
                        <!-- Timeline effect line (optional stylized) -->
                        <div class="absolute left-[7px] top-2 bottom-6 w-[2px] bg-pink-50 z-0"></div>

                        <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="relative pl-8 paginated-item item-container z-10">
                            <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print" style="left: -2px; top: -2px;">
                                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
                            </button>
                            
                            <!-- Star icon marker -->
                            <div class="absolute left-0 top-1.5 text-pink-500 bg-white shadow-sm ring-4 ring-white rounded-full">
                                 <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M12,2L14.5,9H21L15.5,13.5L18,20.5L12,16L6,20.5L8.5,13.5L3,9H9.5L12,2Z" /></svg>
                            </div>

                            <div class="text-[12.5px] font-black text-pink-500 mb-1 tracking-wide flex justify-between">
                                <span>{{ item.time || '2028 - Hiên tại' }}</span>
                                <span class="text-slate-500 uppercase font-black text-[10px] opacity-60 tracking-widest">{{ section.id === 'experience' ? (item.company || 'CÔNG TY') : (item.name || 'DỰ ÁN') }}</span>
                            </div>
                            
                            <h4 class="text-[14.5px] font-black text-slate-900 leading-tight mb-1.5">
                                {{ section.id === 'experience' ? item.role : (item.role || 'Vị trí') }}
                                <span v-if="section.id === 'experience' && item.company" class="text-slate-400 font-bold text-[13px] ml-1 opacity-80">| {{ item.company }}</span>
                            </h4>
                            
                            <div class="text-[12.5px] leading-[1.6] text-slate-600 text-justify font-medium html-content" 
                                 v-html="formatDesc(item.desc || 'Mô tả chi tiết công việc và thành tựu đã đạt được...')">
                            </div>
                        </div>
                    </div>
                </div>
            </template>
        </main>
    </div>

    <!-- STARS ONLY -->
    <div class="absolute bottom-[4mm] right-[10mm] flex items-center gap-1 text-pink-500 opacity-60 z-20 bg-transparent">
        <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M12,2L14.5,9H21L15.5,13.5L18,20.5L12,16L6,20.5L8.5,13.5L3,9H9.5L12,2Z" /></svg>
        <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M12,2L14.5,9H21L15.5,13.5L18,20.5L12,16L6,20.5L8.5,13.5L3,9H9.5L12,2Z" /></svg>
        <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M12,2L14.5,9H21L15.5,13.5L18,20.5L12,16L6,20.5L8.5,13.5L3,9H9.5L12,2Z" /></svg>
    </div>

    <!-- VIỀN DƯỚI CỐ ĐỊNH Ở TỪNG TRANG -->
    <template v-for="p in pageCount" :key="'footer-border-'+p">
        <div class="absolute left-0 w-full flex items-center z-40 pointer-events-none" 
             :style="{ top: `calc(${p * 297}mm - 12mm)`, height: '1.5px', paddingLeft: '15mm', paddingRight: '15mm' }">
            <div class="w-full h-full opacity-30 bg-gradient-to-r from-pink-400 via-red-400 to-pink-400"></div>
        </div>
    </template>

    <!-- DIVIDER PAGES Indicator (Chỉ dành cho xem trên Web) -->
    <template v-for="p in (pageCount - 1)" :key="'div-'+p">
        <div class="absolute left-[-2.5%] w-[105%] z-50 flex flex-col items-center justify-center pointer-events-none no-print" 
             :style="{ top: `calc(${p * 297}mm - 12px)` }">
            <div class="w-full h-[24px] bg-slate-900 shadow-[inset_0_2px_10px_rgba(0,0,0,0.5),0_5px_15px_rgba(0,0,0,0.2)] border-y border-white/10 flex items-center justify-center overflow-hidden">
                <div class="w-full h-[1px] bg-gradient-to-r from-transparent via-pink-500/50 to-transparent"></div>
            </div>
            <span class="absolute text-[9px] uppercase font-black text-white tracking-[0.3em] bg-slate-800 px-4 py-1 rounded-full border border-pink-500/30 shadow-2xl backdrop-blur-md">
                Ngắt trang {{ p + 1 }}
            </span>
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

// --- UTILS ---
const isEmpty = (val) => {
    if (!val) return true;
    if (typeof val !== 'string') return false;
    const cleanText = val.replace(/<[^>]*>/g, '').trim();
    return cleanText === '';
};

const cleanedFullName = computed(() => {
    // Lọc sạch HTML và thực thể &nbsp;
    const raw = props.resumeData.general.fullName || '';
    const clean = raw.replace(/<[^>]*>/g, '')      // Loại bỏ thẻ HTML
                     .replace(/&nbsp;/g, ' ')      // Chuyển &nbsp; thành dấu cách
                     .replace(/\u00a0/g, ' ')      // Chuyển ký tự non-breaking space thành dấu cách
                     .trim();
    return clean || 'Nguyễn Yến Nhi';
});

const firstName = computed(() => {
    const clean = cleanedFullName.value;
    return clean.split(/\s+/)[0] || clean;
});

const lastName = computed(() => {
    const clean = cleanedFullName.value;
    const parts = clean.split(/\s+/);
    if (parts.length <= 1) return '';
    return parts.slice(1).join(' ');
});

// --- SECTION MANAGEMENT ---
const hoveredSectionId = ref(null);
const selectedSectionId = ref(null);
let _hideTimer = null;

const showNav = (id) => {
    if (_hideTimer) { clearTimeout(_hideTimer); _hideTimer = null; }
    hoveredSectionId.value = id;
};

const hideNav = () => {
    _hideTimer = setTimeout(() => {
        hoveredSectionId.value = null;
        _hideTimer = null;
    }, 150);
};


// --- PAGINATION ENGINE (Optimized & Stable) ---
let paginateTimer = null;
const requestPagination = () => {
    if (paginateTimer) clearTimeout(paginateTimer);
    // Độ trễ ngắn hơn để cập nhật tức thì khi người dùng gõ phím hoặc thêm mục mới
    paginateTimer = setTimeout(doPagination, 50);
};

const doPagination = async () => {
    if (!cvRoot.value) return;
    
    // 1. Reset tất cả margin cũ
    const allPaginated = cvRoot.value.querySelectorAll('.paginated-item');
    allPaginated.forEach(el => { el.style.marginTop = ''; });

    await nextTick(); // Chờ Vue render xong dữ liệu mới

    const A4_WIDTH_MM = 210;
    const A4_HEIGHT_MM = 297;
    const MARGIN_BOTTOM_MM = 18; // Tăng lề dưới để không chạm vào viền cố định (12mm)
    const MARGIN_TOP_MM = 15;    // Lề đầu trang mới (15mm)
    
    const rootWidthPx = cvRoot.value.offsetWidth;
    const pxPerMm = rootWidthPx / A4_WIDTH_MM;
    const pageHeightPx = A4_HEIGHT_MM * pxPerMm;
    const marginBottomPx = MARGIN_BOTTOM_MM * pxPerMm;
    const marginTopPx = MARGIN_TOP_MM * pxPerMm;
    const safeBottomPx = pageHeightPx - marginBottomPx;

    // Tính khoảng cách chính xác từ top Document (thẻ CV) so với các thành phần bên trong
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
                
                // Lấy tọa độ thực tại thời điểm này
                const top = getRelativeTop(item);
                const height = item.offsetHeight;
                const bottom = top + height;
                
                const currentPageIndex = Math.floor(top / pageHeightPx);
                const currentSafeBottom = (currentPageIndex * pageHeightPx) + safeBottomPx;

                // Nếu dính lề hoặc tràn trang, PUSH nó qua trang mới!
                if (bottom > currentSafeBottom && top < (currentPageIndex + 1) * pageHeightPx) {
                    
                    // XỬ LÝ QUAN TRỌNG: Nếu bản thân phần tử có chiều cao LỚN HƠN 1 trang (ví dụ đoạn text rất dài)
                    // Hủy bỏ việc đẩy nó qua trang tiếp, thay vì push nó gây lặp vô hạn thì buộc để nó tràn (hoặc browser tự ngắt text)
                    if (height > (pageHeightPx - marginTopPx - marginBottomPx)) {
                        continue;
                    }
                    
                    const targetTop = (currentPageIndex + 1) * pageHeightPx + marginTopPx;
                    const pushAmount = targetTop - top;
                    const currentMt = parseFloat(item.style.marginTop || '0');
                    item.style.marginTop = (currentMt + pushAmount) + 'px';
                    
                    isStable = false; // Phải lặp lại để cập nhật top mới cho phần tử dưới.
                    break;
                }
            }
        }
    };

    processColumn('header');
    processColumn('aside');
    processColumn('main');
    processColumn('footer');

    // Tính toán lại số trang dựa trên phần tử đáy cùng
    let maxBottom = 0;
    allPaginated.forEach(el => {
        const top = getRelativeTop(el);
        const bottom = top + el.offsetHeight;
        if (bottom > maxBottom) maxBottom = bottom;
    });

    // Trừ đi một khoảng an toàn nhỏ (2px) để tránh sai số thập phân khiến tạo thêm 1 trang trống
    const calculatedPageCount = Math.max(1, Math.ceil((maxBottom + marginBottomPx - 2) / pageHeightPx));
    if (pageCount.value !== calculatedPageCount) {
        pageCount.value = calculatedPageCount;
    }
};

const sidebarSections = computed(() => {
    return props.resumeData.sections.filter(s => s.column === 'left');
});

const mainSections = computed(() => {
    return props.resumeData.sections.filter(s => s.column === 'right');
});

const sidebarIds = computed(() => sidebarSections.value.map(s => s.id));
const mainIds = computed(() => mainSections.value.map(s => s.id));

watch(() => props.resumeData, () => requestPagination(), { deep: true });
onMounted(() => {
    // Khởi tạo trạng thái hiển thị mặc định cho mẫu này theo đúng Ảnh 2
    if (props.resumeData?.sections) {
        const SIDEBAR_IDS = ['summary','education', 'skills'];
        const MAIN_IDS = ['experience', 'project'];

        props.resumeData.sections.forEach(sec => {
            // Chỉ active đúng các mục có trong ảnh 2 (6 mục tính cả Thông tin liên hệ)
            const isTarget = [...SIDEBAR_IDS, ...MAIN_IDS].includes(sec.id);
            sec.isVisible = isTarget;

            // Gán cột theo thiết kế
            if (SIDEBAR_IDS.includes(sec.id)) {
                sec.column = 'left';
            } else {
                sec.column = 'right';
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

const formatDesc = (text) => {
    if (!text) return '';
    if (/<[a-z][\s\S]*>/i.test(text)) return text;
    return text.split('\n').map(l => l.trim()).filter(l=>l).join('<br/>');
}

const getLevelInfo = (level) => {
    if (!level) return { text: '', percent: '80%' };
    const l = level.toLowerCase().trim();
    if (l === 'cơ bản') return { text: 'Cơ bản', percent: '25%' };
    if (l === 'trung cấp') return { text: 'Trung cấp', percent: '50%' };
    if (l === 'thành thạo') return { text: 'Thành thạo', percent: '75%' };
    if (l === 'chuyên gia') return { text: 'Chuyên gia', percent: '100%' };
    
    // Hỗ trợ nhập số trực tiếp (ví dụ: 90 hoặc 90%)
    if (level.includes('%')) return { text: level, percent: level };
    const num = parseInt(level);
    if (!isNaN(num)) return { text: num + '%', percent: num + '%' };
    
    // Mặc định nếu không khớp
    return { text: level, percent: '80%' };
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Playfair+Display:wght@900&family=Inter:wght@400;500;600;700;800;900&display=swap');

#cv-printable-area {
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
    overflow-wrap: anywhere;
}

/* --- SECTION BLOCKS --- */
.section-block {
    padding: 12px;
    border: 2px solid transparent;
    border-radius: 16px;
    cursor: pointer;
    transition: all 0.25s ease;
}
.section-block:hover { background: rgba(0,0,0,0.01); }
.section-selected {
    border-color: #f472b6; 
    background: rgba(244, 114, 182, 0.04) !important;
    box-shadow: 0 4px 15px rgba(244, 114, 182, 0.1);
}

/* --- NAV BUTTONS --- */
.nav-btns {
    position: absolute;
    right: 8px;
    top: 8px;
    display: flex;
    gap: 5px;
    z-index: 100;
}
.nav-btn {
    width: 26px;
    height: 26px;
    background: #1e293b;
    color: white;
    border: none;
    border-radius: 6px;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    box-shadow: 0 2px 5px rgba(0,0,0,0.2);
}
.nav-btn:hover { background: #f472b6; transform: scale(1.1); }

/* --- DELETE BUTTONS --- */
.item-container {
    transition: all 0.2s;
}
.delete-btn {
    position: absolute;
    opacity: 0;
    pointer-events: none;
    background: #ef4444;
    color: white;
    border-radius: 999px;
    width: 20px;
    height: 20px;
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 50;
    transition: all 0.2s;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}
.item-container:hover .delete-btn {
    opacity: 1;
    pointer-events: auto;
    transform: scale(1.1);
}

/* --- RICH TEXT CONTENT --- */
:deep(.html-content ul) {
    list-style-type: none !important;
    padding-left: 0.5rem !important;
}
:deep(.html-content li) { 
    margin-bottom: 0.25rem; 
    position: relative;
    padding-left: 1.2rem;
}
:deep(.html-content li::before) {
    content: "✦";
    position: absolute;
    left: 0;
    color: #f472b6;
    font-size: 14px;
}
:deep(.html-content b), :deep(.html-content strong) { font-weight: 800; }

@media print {
    .no-print { display: none !important; }
    .section-block { padding: 0 !important; border: none !important; margin-bottom: 2rem !important; }
}
</style>
