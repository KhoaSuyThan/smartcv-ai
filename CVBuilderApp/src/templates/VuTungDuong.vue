<template>
  <div id="cv-printable-area" ref="cvRoot" class="bg-white flex w-[210mm] relative box-border text-[13px] leading-relaxed" :style="{ minHeight: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif' }">
    
    <!-- CỘT TRÁI (SIDEBAR) -->
    <aside class="w-[78mm] flex-shrink-0 bg-[#3a3e43] text-gray-200 flex flex-col relative z-20 pb-[10mm]" @click.self="selectedSectionId = null">
       
        <!-- AVATAR - HÌNH CHỮ NHẬT DỌC -->
        <div class="px-[8mm] pt-[10mm] pb-[2mm] paginated-item flex justify-center">
            <div class="w-[62mm] h-[82mm] rounded-md overflow-hidden bg-[#4a4e55] border-[2px] border-white/10 shadow-lg relative z-10 box-border flex-shrink-0">
                <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
                <div v-else class="w-full h-full flex items-center justify-center text-gray-400">
                    <svg class="w-20 h-20" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
                </div>
            </div>
        </div>

        <!-- TÊN & CHỨC DANH -->
        <div class="px-[8mm] pt-[2mm] pb-[4mm] paginated-item">
            <h1 class="text-[25px] font-extrabold text-center uppercase leading-tight mb-1" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'HỌ VÀ TÊN'"></h1>
            <h2 class="text-[14px] font-bold text-center uppercase tracking-wider" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'Vị trí ứng tuyển'"></h2>
        </div>

        <!-- CÁC SECTION TRONG SIDEBAR -->
        <div class="px-[8mm] flex flex-col gap-5 flex-1 mt-4">
            <!-- THÔNG TIN CÁ NHÂN -->
            <div class="paginated-item w-full section-block-sidebar py-[2mm] px-[2mm] -mx-[2mm]">
                <div class="flex items-center gap-3 mb-4">
                    <h3 class="font-bold text-[14px] uppercase whitespace-nowrap" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">Thông tin cá nhân</h3>
                    <div class="flex-1 h-[1.5px]" :style="{ backgroundColor: resumeData.theme.primaryColor || '#dfa234' }"></div>
                </div>
                <div class="space-y-4 text-[12px] font-medium text-white">
                    <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.phone)">
                       <div class="w-[16px] h-[16px] flex-shrink-0 mt-[2px]" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">
                           <svg viewBox="0 0 24 24" fill="currentColor"><path d="M6.62 10.79a15.091 15.091 0 006.59 6.59l2.2-2.2a1 1 0 011.11-.21c1.12.37 2.33.57 3.58.57a1 1 0 011 1v3.5a1 1 0 01-1 1A16 16 0 013 4a1 1 0 011-1h3.5a1 1 0 011 1c0 1.25.2 2.46.57 3.58a1 1 0 01-.21 1.11l-2.24 2.1z" /></svg>
                       </div>
                       <span class="break-all leading-tight pt-[1px]" v-html="resumeData.general.phone"></span>
                    </div>
                    <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.email)">
                       <div class="w-[16px] h-[16px] flex-shrink-0 mt-[2px]" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">
                           <svg viewBox="0 0 24 24" fill="currentColor"><path d="M22 6c0-1.1-.9-2-2-2H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6zm-2 0l-8 5-8-5h16zm0 12H4V8l8 5 8-5v10z" /></svg>
                       </div>
                       <span class="break-all leading-tight pt-[1px]" v-html="resumeData.general.email"></span>
                    </div>
                    <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.website)">
                       <div class="w-[16px] h-[16px] flex-shrink-0 mt-[2px]" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">
                           <svg viewBox="0 0 24 24" fill="currentColor"><path d="M3.9 12c0-1.71 1.39-3.1 3.1-3.1h4V7H7c-2.76 0-5 2.24-5 5s2.24 5 5 5h4v-1.9H7c-1.71 0-3.1-1.39-3.1-3.1zM8 13h8v-2H8v2zm9-6h-4v1.9h4c1.71 0 3.1 1.39 3.1 3.1s-1.39 3.1-3.1 3.1h-4V17h4c2.76 0 5-2.24 5-5s-2.24-5-5-5z" /></svg>
                       </div>
                       <span class="break-all leading-tight pt-[1px]" v-html="resumeData.general.website"></span>
                    </div>
                    <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.address)">
                       <div class="w-[16px] h-[16px] flex-shrink-0 mt-[2px]" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">
                           <svg viewBox="0 0 24 24" fill="currentColor"><path d="M12 2C8.13 2 5 5.13 5 9c0 4.17 4.42 9.92 6.24 12.11a1 1 0 001.52 0C14.58 18.92 19 13.17 19 9c0-3.87-3.13-7-7-7zm0 9.5a2.5 2.5 0 010-5 2.5 2.5 0 010 5z" /></svg>
                       </div>
                       <span class="leading-tight pt-[1px]" v-html="resumeData.general.address"></span>
                    </div>
                    <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.birthDate)">
                       <div class="w-[16px] h-[16px] flex-shrink-0 mt-[2px]" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">
                           <svg viewBox="0 0 24 24" fill="currentColor"><path d="M19 4h-1V2h-2v2H8V2H6v2H5c-1.11 0-1.99.9-1.99 2L3 20a2 2 0 002 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H5V10h14v10zM5 8V6h14v2H5z"/></svg>
                       </div>
                       <span class="leading-tight pt-[1px]" v-html="resumeData.general.birthDate"></span>
                    </div>
                </div>
            </div>

            <!-- DYNAMIC SECTIONS CHO SIDEBAR -->
            <template v-for="section in sidebarSections" :key="section.id">
                <div
                    v-show="section.isVisible"
                    class="section-block section-block-sidebar py-[2mm] px-[2mm] -mx-[2mm]"
                    :class="{ 'section-selected bg-[#4a4e55]': selectedSectionId === section.id }"
                    :style="selectedSectionId === section.id ? { '--sel-color': resumeData.theme.primaryColor || '#dfa234' } : {}"
                    @mouseenter="showNav(section.id)"
                    @mouseleave="hideNav()"
                    @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
                >
                    <!-- Nav buttons -->
                    <div
                        v-show="hoveredSectionId === section.id || selectedSectionId === section.id"
                        class="nav-btns no-print"
                        @mouseenter="showNav(section.id)"
                        @mouseleave="hideNav()"
                    >
                        <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Lên">
                            <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                        </button>
                        <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Xuống">
                            <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                        </button>
                        <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải">
                            <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
                        </button>
                    </div>

                    <div class="flex items-center gap-3 mb-3.5 paginated-item">
                        <h3 class="font-bold text-[14px] uppercase whitespace-nowrap" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">
                            {{ section.title }}
                        </h3>
                        <div class="flex-1 h-[1.5px]" :style="{ backgroundColor: resumeData.theme.primaryColor || '#dfa234' }"></div>
                    </div>
                    
                    <div class="space-y-4">
                         <!-- Kỹ năng, Sở thích -->
                         <div v-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills' || section.id === 'hobbies'" class="space-y-3">
                             <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[12px] text-white font-medium item-container paginated-item pr-4 leading-snug">
                                 <span v-html="item.name"></span>
                                 <span v-if="item.level" class="text-gray-300 font-normal ml-1">({{ item.level }})</span>
                                 <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 scale-90">
                                     <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                 </button>
                             </div>
                         </div>
                         
                         <!-- Chứng chỉ, Giải thưởng -->
                         <div v-else-if="section.id === 'awards' || section.id === 'certifications'" class="space-y-3.5">
                            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[12px] item-container paginated-item flex flex-col gap-0.5 pr-2">
                                <p v-if="item.year" class="font-bold text-white whitespace-nowrap">{{ item.year }}</p>
                                <p class="text-white leading-snug font-medium">{{ item.name }}</p>
                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 scale-90">
                                    <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                            </div>
                         </div>

                         <!-- Người tham chiếu -->
                         <div v-else-if="section.id === 'references'" class="space-y-4">
                             <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[12px] item-container paginated-item relative">
                                 <div class="sidebar-html-content text-white leading-snug" v-html="formatDesc(item.info)"></div>
                                 <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 scale-90">
                                     <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                 </button>
                             </div>
                         </div>

                         <!-- Default -->
                         <div v-else class="space-y-3">
                             <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[12px] text-white whitespace-pre-line leading-relaxed paginated-item item-container">
                                 {{ item.desc || item.name || item.info }}
                                 <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 scale-90">
                                     <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                 </button>
                             </div>
                         </div>
                    </div>
                </div>
            </template>
        </div>
    </aside>

    <!-- CỘT PHẢI (MAIN CONTENT) -->
    <main class="flex-1 py-[10mm] px-[10mm] gap-4 flex flex-col relative z-10" @click.self="selectedSectionId = null">
        <template v-for="section in mainSections" :key="section.id">
            <div
                v-show="section.isVisible"
                class="section-block section-block-main px-[4mm] py-[3mm]"
                :class="{ 'section-selected bg-blue-50/50': selectedSectionId === section.id }"
                :style="selectedSectionId === section.id ? { '--sel-color': resumeData.theme.primaryColor || '#dfa234' } : {}"
                @mouseenter="showNav(section.id)"
                @mouseleave="hideNav()"
                @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
            >
                <div
                    v-show="hoveredSectionId === section.id || selectedSectionId === section.id"
                    class="nav-btns no-print"
                    @mouseenter="showNav(section.id)"
                    @mouseleave="hideNav()"
                >
                    <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Lên">
                        <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                    </button>
                    <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Xuống">
                        <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                    </button>
                    <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái">
                        <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
                    </button>
                </div>

                <div class="flex items-center gap-3 mb-6 paginated-item">
                    <h3 class="font-black text-[16px] uppercase whitespace-nowrap" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">
                        {{ section.title }}
                    </h3>
                    <div class="flex-1 h-[2px]" :style="{ backgroundColor: resumeData.theme.primaryColor || '#dfa234' }"></div>
                </div>

                <div class="space-y-6">
                    <!-- Mục tiêu nghề nghiệp -->
                    <div v-if="section.id === 'summary'" class="text-[12px] leading-[1.7] text-gray-900 text-justify html-content paginated-item pl-1 pr-1 font-medium" v-html="resumeData.general.summary || 'Chưa có thông tin mục tiêu nghề nghiệp.'">
                    </div>

                    <!-- Kinh nghiệm / Hoạt động / Dự án -->
                    <div v-else-if="section.id === 'experience' || section.id === 'project' || section.id === 'activities'" class="space-y-6">
                        <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container paginated-item rounded-lg p-2 -mx-2 hover:bg-gray-50 border border-transparent hover:border-gray-100 transition-colors">
                            <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30">
                                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                            </button>

                            <div class="flex flex-col mb-2.5">
                                <div class="flex justify-between items-baseline md:flex-row flex-col gap-1 md:gap-0">
                                    <span class="font-bold text-[14px] text-black">{{ section.id === 'experience' ? item.role : (item.role || item.name) }}</span>
                                    <span class="text-[12px] font-bold text-black whitespace-nowrap">{{ item.time }}</span>
                                </div>
                                <div class="font-[700] text-[13px] text-black mt-0.5">{{ section.id === 'experience' ? item.company : (item.company || '') }}</div>
                            </div>
                            <div class="text-[12px] leading-[1.65] text-gray-900 text-justify html-content font-medium ml-1" v-html="formatDesc(item.desc)"></div>
                        </div>
                    </div>

                    <!-- Học Vấn -->
                    <div v-else-if="section.id === 'education'" class="space-y-6">
                        <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container paginated-item rounded-lg p-2 -mx-2 hover:bg-gray-50 border border-transparent hover:border-gray-100 transition-colors">
                            <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30">
                                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                            </button>

                            <div class="flex flex-col mb-2">
                                <div class="flex justify-between items-baseline md:flex-row flex-col gap-1 md:gap-0">
                                    <span class="font-bold text-[14px] text-black">{{ item.major || item.school }}</span>
                                    <span class="text-[12px] font-bold text-black whitespace-nowrap">{{ item.year }}</span>
                                </div>
                                <div class="font-[700] text-[13px] text-black mt-0.5">{{ item.school }}</div>
                            </div>
                            <div class="text-[12px] text-gray-900 ml-1 font-medium">
                                <span v-if="item.gradType">Tốt nghiệp loại: <strong>{{ item.gradType }}</strong></span>
                                <span v-if="item.gradType && item.gpa"> | </span>
                                <span v-if="item.gpa">GPA: <strong>{{ item.gpa }}</strong></span>
                            </div>
                            <div v-if="item.desc" class="text-[12px] leading-[1.65] text-gray-900 text-justify font-medium html-content mt-2 ml-1" v-html="formatDesc(item.desc)"></div>
                        </div>
                    </div>

                    <!-- Default Main -->
                    <div v-else class="space-y-5">
                        <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container paginated-item text-[12px] leading-[1.65] text-gray-900 font-medium html-content rounded-lg p-2 -mx-2 hover:bg-gray-50 border border-transparent hover:border-gray-100 transition-colors">
                             <div class="ml-1" v-html="formatDesc(item.desc || item.name || item.info)"></div>
                             <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30">
                                 <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                             </button>
                        </div>
                    </div>
                </div>
            </div>
        </template>
    </main>

    <!-- ĐƯỜNG PHÂN TRANG ẢO Trên Preview -->
    <template v-for="p in (pageCount - 1)" :key="'div-'+p">
        <div class="absolute left-0 w-full z-[100] flex flex-col items-center justify-center pointer-events-none no-print" 
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

// --- CHỨC NĂNG PHÂN TRANG (PAGINATION ENGINE) ---
let paginateTimer = null;
const requestPagination = () => {
    if (paginateTimer) clearTimeout(paginateTimer);
    paginateTimer = setTimeout(doPagination, 300);
};

const doPagination = async () => {
    if (!cvRoot.value) return;
    
    // 1. Reset nội dung lề đã định hình trước đó
    const items = cvRoot.value.querySelectorAll('.paginated-item');
    items.forEach(el => { el.style.marginTop = ''; });

    await nextTick();

    const A4_WIDTH_MM = 210;
    const A4_HEIGHT_MM = 297;
    const MARGIN_BOTTOM_MM = 15; // Lề dưới an toàn (15mm)
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
                
                const top = getRelativeTop(item);
                const bottom = top + item.offsetHeight;
                
                const currentPageIndex = Math.floor(top / pageHeightPx);
                const currentSafeBottom = (currentPageIndex * pageHeightPx) + safeBottomPx;
                
                // Nếu dính lề hoặc tràn trang, PUSH nó qua trang mới!
                if (bottom > currentSafeBottom && top < (currentPageIndex + 1) * pageHeightPx) {
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

    // Cập nhật lại số lượng giấy cần Render dựa trên phần tử nằm xa nhất thay vì offsetHeight (bị kẹt bởi minHeight)
    let maxBottom = 0;
    items.forEach(el => {
        const top = getRelativeTop(el);
        const bottom = top + el.offsetHeight;
        if (bottom > maxBottom) maxBottom = bottom;
    });

    // Tính số trang dựa trên điểm đáy cuối cùng + lề dưới an toàn
    const calculatedPageCount = Math.max(1, Math.ceil((maxBottom + marginBottomPx - 2) / pageHeightPx));
    
    // Chỉ cập nhật nếu có sự thay đổi để tránh re-render thừa
    if (pageCount.value !== calculatedPageCount) {
        pageCount.value = calculatedPageCount;
    }
};

watch(() => props.resumeData, () => {
    requestPagination();
}, { deep: true });

onMounted(() => {
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

const sidebarIds = computed(() => sidebarSections.value.map(s => s.id));
const mainIds = computed(() => mainSections.value.map(s => s.id));

const sidebarSections = computed(() => {
    return props.resumeData.sections.filter(s => s.column === 'left');
});

const mainSections = computed(() => {
    return props.resumeData.sections.filter(s => s.column === 'right');
});

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
.item-container {
    position: relative;
    transition: all 0.2s;
}

.delete-btn {
    opacity: 0;
    transition: all 0.2s;
    position: absolute;
    top: 0px;
    right: 0px;
    width: 22px;
    height: 22px;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    border: 2px solid white;
}

.item-container:hover .delete-btn {
    opacity: 1;
}

aside .item-container:hover {
    background-color: rgba(255, 255, 255, 0.05);
    border-radius: 6px;
    padding-left: 4px;
    margin-left: -4px;
}
aside .delete-btn {
    top: -2px;
    right: -2px;
    border-color: #3e4348;
}

main .item-container:hover {
    background-color: rgba(0, 0, 0, 0.02);
}
</style>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&display=swap');

#cv-printable-area {
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
    overflow-wrap: anywhere;
}

/* HTML CONTENT SUPPORT */
:deep(.html-content ul) {
    list-style-type: disc !important;
    padding-left: 1.25rem !important;
    margin-top: 0.2rem;
    margin-bottom: 0.2rem;
}
:deep(.html-content ol) {
    list-style-type: decimal !important;
    padding-left: 1.25rem !important;
    margin-top: 0.2rem;
    margin-bottom: 0.2rem;
}
:deep(.html-content b), :deep(.html-content strong) { font-weight: 700; }
:deep(.html-content h4) { font-weight: 700; font-size: 14.5px; margin-bottom: 4px; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic; }
:deep(.html-content u) { text-decoration: underline; }
:deep(.html-content ul li), :deep(.html-content ol li) { margin-bottom: 0.25rem; }

/* Dynamic list marker scaling */
:deep(.html-content li:has(> font[size="1"])) { font-size: 10px; }
:deep(.html-content li:has(> font[size="2"])) { font-size: 13px; }
:deep(.html-content li:has(> font[size="3"])) { font-size: 16px; }

/* Sidebar styling overrides */
:deep(.sidebar-html-content p),
:deep(.sidebar-html-content span),
:deep(.sidebar-html-content li),
:deep(.sidebar-html-content div) {
    color: inherit;
}

:deep(.sidebar-html-content b), :deep(.sidebar-html-content strong), :deep(.sidebar-html-content h4) {
    color: white;
    font-weight: 700;
}

/* SECTION BLOCKS */
.section-block {
    position: relative;
    border-radius: 8px;
    border: 2px solid transparent; 
    cursor: pointer;
    transition: border-color 0.2s ease, background 0.2s ease, box-shadow 0.2s ease;
}

.section-block-main:hover {
    background: rgba(0, 0, 0, 0.02);
}

.section-block-sidebar:hover {
    background: rgba(255, 255, 255, 0.04);
}

.section-selected {
    border-color: var(--sel-color, #dfa234);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.section-selected.section-block-main {
    background: color-mix(in srgb, var(--sel-color, #dfa234) 4%, white) !important;
}

/* NAV BUTTONS */
.nav-btns {
    position: absolute;
    right: 4px;
    top: 4px;
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
    background: #dfa234;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    box-shadow: 0 2px 6px rgba(0,0,0, 0.2);
    transition: background 0.15s ease, transform 0.1s ease;
}

.nav-btn:hover {
    background: #c78d2b;
    transform: scale(1.1);
}

.nav-btn:active {
    transform: scale(0.95);
}

@media print {
    .no-print {
        display: none !important;
    }
    .section-block, .section-selected {
        cursor: default;
        box-shadow: none !important;
        background: transparent !important;
        border-color: transparent !important;
        padding-left: 0 !important;
        padding-right: 0 !important;
        padding-top: 0 !important;
        padding-bottom: 0 !important;
        margin: 0 !important;
        border-radius: 0 !important;
    }
}

:global(.is-exporting-pdf .no-print) {
    display: none !important;
}
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .section-selected) {
    cursor: default !important;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
}
</style>
