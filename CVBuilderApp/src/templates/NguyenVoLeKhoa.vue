<template>
  <div id="cv-printable-area" ref="cvRoot" class="bg-white shadow-2xl w-[210mm] flex flex-row relative box-border text-[#333] leading-relaxed overflow-hidden" :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif !important' }">
    
    <!-- LEFT COLUMN -->
    <aside class="w-[68mm] z-10 flex flex-col pt-0 shrink-0 relative bg-white">
        
        <!-- Vùng Header Trái: Avatar + Liên hệ (Có màu nền) -->
        <div class="pt-[15mm] px-[8mm] flex flex-col paginated-item relative z-20 items-center" :style="{ backgroundColor: templateSecondaryColor }">
            <!-- Avatar -->
            <div class="relative w-[45mm] h-[45mm] rounded-full overflow-hidden mb-6 mx-auto bg-transparent border-[1.5px] border-slate-300">
                <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
                <div v-else class="w-full h-full flex items-center justify-center bg-gray-200 text-gray-500">
                    <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
                </div>
            </div>

            <!-- Contact -->
            <div class="flex flex-col w-full font-medium text-[#333]" :style="{ fontSize: '11px !important' }">
                <!-- Phone -->
                <div class="w-full border-t border-slate-400/80 py-3" v-if="!isEmpty(resumeData.general.phone)">
                    <div class="flex items-center gap-3.5 relative">
                        <div class="w-7 h-7 rounded-full border-[1.5px] border-slate-600 flex items-center justify-center shrink-0">
                            <svg class="w-3.5 h-3.5 text-slate-800" fill="currentColor" viewBox="0 0 20 20"><path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z"></path></svg>
                        </div>
                        <span class="break-all" v-html="resumeData.general.phone"></span>
                    </div>
                </div>
                
                <!-- Email -->
                <div class="w-full border-t border-slate-400/80 py-3" v-if="!isEmpty(resumeData.general.email)">
                    <div class="flex items-center gap-3.5 relative">
                        <div class="w-7 h-7 rounded-full border-[1.5px] border-slate-600 flex items-center justify-center shrink-0">
                            <svg class="w-3.5 h-3.5 text-slate-800" fill="currentColor" viewBox="0 0 20 20"><path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z"></path><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z"></path></svg>
                        </div>
                        <span class="break-all" v-html="resumeData.general.email"></span>
                    </div>
                </div>

                <!-- GitHub / Website -->
                <div class="w-full border-t border-slate-400/80 py-3" v-if="!isEmpty(resumeData.general.github) || !isEmpty(resumeData.general.website) || !isEmpty(resumeData.general.linkedin)">
                    <div class="flex items-center gap-3.5 relative">
                        <div class="w-7 h-7 rounded-full border-[1.5px] border-slate-600 flex items-center justify-center shrink-0">
                            <!-- Icon Globe -->
                            <svg class="w-3.5 h-3.5 text-slate-800" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM4.332 8.027a6.012 6.012 0 011.912-2.706C6.512 5.73 6.974 6 7.5 6A1.5 1.5 0 019 7.5V8a2 2 0 004 0 2 2 0 011.523-1.943A5.977 5.977 0 0116 10c0 .34-.028.675-.083 1H15a2 2 0 00-2 2v2.197A5.973 5.973 0 0110 16v-2a2 2 0 00-2-2 2 2 0 01-2-2 2 2 0 00-1.668-1.973z" clip-rule="evenodd"></path></svg>
                        </div>
                        <span class="break-all" v-html="!isEmpty(resumeData.general.github) ? resumeData.general.github : (!isEmpty(resumeData.general.linkedin) ? resumeData.general.linkedin : resumeData.general.website)"></span>
                    </div>
                </div>

                <!-- Location -->
                <div class="w-full border-t border-slate-400/80 py-3" v-if="!isEmpty(resumeData.general.address)">
                    <div class="flex items-center gap-3.5 relative">
                        <div class="w-7 h-7 rounded-full border-[1.5px] border-slate-600 flex items-center justify-center shrink-0">
                            <svg class="w-3.5 h-3.5 text-slate-800" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd"></path></svg>
                        </div>
                        <span class="break-all" v-html="resumeData.general.address"></span>
                    </div>
                </div>
                
            </div>
        </div>

        <!-- Các Sections của Left Column -->
        <div class="w-full pl-[8mm] pr-0 flex-1 pb-8 flex flex-col mt-4">
            <template v-for="section in sidebarSections" :key="section.id">
                <div
                    v-show="section.isVisible"
                    class="section-block relative paginated-item group mb-6"
                    :class="{ 'section-selected': selectedSectionId === section.id }"
                    :style="selectedSectionId === section.id ? { '--sel-color': templatePrimaryColor } : {}"
                    @mouseenter="showNav(section.id)"
                    @mouseleave="hideNav()"
                    @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
                >
                    <!-- Nav Btns -->
                    <div
                        v-show="hoveredSectionId === section.id || selectedSectionId === section.id"
                        class="nav-btns no-print"
                        @mouseenter="showNav(section.id)"
                        @mouseleave="hideNav()"
                    >
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

                    <h3 class="font-bold uppercase mb-1 tracking-wider flex items-center gap-2" :style="{ fontSize: '16px !important', color: '#1a4731 !important', fontWeight: 'bold !important' }">
                        {{ section.title }}
                    </h3>
                    <div class="w-full border-b-[1.5px] border-[#333]/30 mb-4"></div>
                    
                    <div class="space-y-4">
                        <!-- Danh sách Kỹ năng -->
                        <div v-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'" class="space-y-3">
                           <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[#333] item-container font-medium leading-relaxed flex items-start gap-2" :style="{ fontSize: '11px !important' }">
                               <span class="mt-[0.5px] text-slate-500 shrink-0">
                                   <!-- Icon giống hoa thị 4 điểm -->
                                   <svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><path d="M12 2l2 7 7 2-7 2-2 7-2-7-7-2 7-2z" fill="currentColor"/></svg>
                               </span>
                               <span class="flex-1 text-justify"><span class="text-[#333]" :style="{ fontWeight: 'bold !important' }">{{ item.name }}:</span> <span v-if="item.info" class="font-normal">{{ item.info }}</span><span v-else-if="item.level" class="font-normal">({{ item.level }})</span></span>
                               
                               <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 opacity-0 group-hover:opacity-100 transition-opacity absolute right-0 top-0">
                                   <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                               </button>
                           </div>
                           <!-- Đường line mờ phân cách section đã được chuyển lên dưới tiêu đề -->
                        </div>

                        <!-- Chứng chỉ / Giải thưởng (Date rồi đến nội dung) -->
                        <div v-else-if="section.id === 'awards' || section.id === 'certifications'" class="space-y-5">
                           <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container leading-relaxed text-[#333]" :style="{ fontSize: '11px !important' }">
                               <p v-if="item.year" class="font-bold text-[#333] mb-0.5">{{ item.year }}</p>
                               <p class="font-normal html-content">{{ item.name || item.info }}</p>

                               <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 opacity-0 group-hover:opacity-100 transition-opacity absolute right-0 top-0">
                                   <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                               </button>
                           </div>
                        </div>

                        <!-- Sở thích (Hobby) -->
                        <div v-else-if="section.id === 'hobbies'" class="space-y-1">
                           <div class="flex flex-wrap gap-1 text-[#333] font-normal leading-relaxed" :style="{ fontSize: '11px !important' }">
                               <template v-for="(item, itemIndex) in section.items" :key="item._refId">
                                    <span class="item-container relative">
                                        {{ item.name }}<span v-if="itemIndex < section.items.length - 1">, </span>
                                        <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 opacity-0 group-hover:opacity-100 transition-opacity absolute right-[-5px] top-[-5px] scale-75">
                                           <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                        </button>
                                    </span>
                               </template>
                           </div>
                        </div>

                        <!-- Các mục khác của Sidebar -->
                        <div v-else class="space-y-3">
                           <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[#333] font-normal leading-relaxed item-container relative" :style="{ fontSize: '11px !important' }">
                               <span class="html-content" v-html="formatDesc(item.desc || item.name || item.info)"></span>
                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 opacity-0 group-hover:opacity-100 transition-opacity absolute right-0 top-0">
                                   <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                               </button>
                           </div>
                        </div>
                    </div>
                </div>
            </template>
        </div>
    </aside>

    <!-- RIGHT COLUMN -->
    <main class="flex-1 flex flex-col relative bg-white z-20 min-h-max" @click.self="selectedSectionId = null">
        
        <!-- Header Phải: Tên + Nghề nghiệp + Summary -->
        <header class="paginated-item pt-[12mm] px-[12mm] pb-[8mm] flex flex-col min-h-min" :style="{ backgroundColor: templatePrimaryColor, color: 'white' }">
            <h1 class="font-bold uppercase tracking-wide mb-1 leading-tight" :style="{ fontSize: '26px !important' }" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'NGUYỄN VÕ LÊ KHOA'"></h1>
            
            <div class="border-b border-white pb-3 w-fit pr-12 mb-4">
                <h2 class="font-medium uppercase tracking-[0.08em] opacity-90" :style="{ fontSize: '13.5px !important' }" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'VỊ TRÍ ỨNG TUYỂN'"></h2>
            </div>

            <div v-if="summarySection && summarySection.isVisible" class="leading-relaxed text-justify html-content font-medium opacity-90 mt-1" :style="{ fontSize: '11px !important' }" v-html="resumeData.general.summary || 'Tôi là một sinh viên CNTT năng động...'"></div>
        </header>

        <!-- Các Sections của Right Column -->
        <div class="px-[12mm] pt-[8mm] pb-[8mm] flex-1 flex flex-col gap-[7mm]">
            <template v-for="section in mainSections" :key="section.id">
                <div
                    v-show="section.isVisible"
                    class="section-block relative paginated-item group my-0 py-1"
                    :class="{ 'section-selected': selectedSectionId === section.id }"
                    :style="selectedSectionId === section.id ? { '--sel-color': templatePrimaryColor } : {}"
                    @mouseenter="showNav(section.id)"
                    @mouseleave="hideNav()"
                    @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
                >
                    <!-- Nav Btns -->
                    <div
                        v-show="hoveredSectionId === section.id || selectedSectionId === section.id"
                        class="nav-btns no-print"
                        @mouseenter="showNav(section.id)"
                        @mouseleave="hideNav()"
                    >
                        <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên">
                            <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                        </button>
                        <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống">
                            <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                        </button>
                        <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái">
                            <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
                        </button>
                    </div>

                    <h3 class="font-bold uppercase mb-1 tracking-wide flex items-center gap-2" :style="{ fontSize: '16px !important', color: '#1a4731 !important', fontWeight: 'bold !important' }">
                        {{ section.title }}
                    </h3>
                    <div class="w-full border-b-[1.5px] border-[#333]/30 mb-4"></div>

                    <div class="space-y-6">
                        <!-- Học Vấn -->
                        <div v-if="section.id === 'education'" class="space-y-6">
                            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                                <div class="flex justify-between items-start gap-4 mb-1">
                                    <h4 class="text-[#333] align-middle mt-[2px]" :style="{ fontSize: '11.5px !important', fontWeight: 'bold !important' }"><span v-html="item.school"></span></h4>
                                    <span v-if="item.year" class="font-bold text-white px-[8px] py-[2px] rounded-full shrink-0 leading-none" :style="{ backgroundColor: templatePrimaryColor, fontSize: '9px !important' }">
                                        {{ item.year }}
                                    </span>
                                </div>
                                <div class="text-[#333] font-normal html-content mb-1" v-if="item.major" :style="{ fontSize: '11px !important' }">{{ item.major }}</div>
                                <div v-if="item.desc" class="text-[#333] html-content leading-relaxed" :style="{ fontSize: '11px !important' }" v-html="formatDesc(item.desc)"></div>
                                <div v-else-if="item.gradType" class="text-[#333] font-bold mt-1" :style="{ fontSize: '11px !important' }">Trạng thái: <span class="font-normal">{{ item.gradType }}</span></div>

                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 absolute right-[-10px] top-[-5px]">
                                    <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                            </div>
                        </div>

                        <!-- Kinh nghiệm / Dự án / Hoạt động -->
                        <div v-else-if="section.id === 'experience' || section.id === 'project' || section.id === 'activities'" class="space-y-6">
                            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                                <div class="flex justify-between items-start gap-4 mb-1.5">
                                    <h4 class="text-[#333] uppercase leading-snug mt-[1px]" :style="{ fontSize: '11.5px !important', fontWeight: 'bold !important' }"><span v-html="section.id === 'experience' ? item.company : (item.name || '')"></span></h4>
                                    <span v-if="item.time" class="font-bold text-white px-[8px] py-[2px] rounded-full shrink-0 leading-none" :style="{ backgroundColor: templatePrimaryColor, fontSize: '9px !important' }">
                                        {{ item.time }}
                                    </span>
                                </div>
                                <div class="font-normal text-[#333] mb-1.5" v-if="item.role" :style="{ fontSize: '11px !important' }">{{ section.id === 'experience' ? item.role : (item.role || '') }}</div>
                                <div class="leading-relaxed text-[#333] text-justify html-content" :style="{ fontSize: '11px !important' }" v-html="formatDesc(item.desc)"></div>

                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 absolute right-[-10px] top-[-5px]">
                                    <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                            </div>
                        </div>

                        <!-- Các mục khác của MainColumn -->
                        <div v-else class="space-y-4">
                            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative text-[#333] leading-relaxed" :style="{ fontSize: '11px !important' }">
                                <div class="html-content" v-html="formatDesc(item.desc || item.info || item.name)"></div>
                                
                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 absolute right-[-10px] top-[-5px]">
                                    <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </template>
        </div>
    </main>
    
    <!-- VIỀN DƯỚI CỐ ĐỊNH Ở TỪNG TRANG -->
    <template v-for="p in pageCount" :key="'footer-border-'+p">
        <div class="absolute left-0 w-full flex items-center z-40 pointer-events-none" 
             :style="{ top: `calc(${p * 297}mm - 12mm)`, height: '1.5px', paddingLeft: '12mm', paddingRight: '12mm' }">
            <div class="w-full h-full opacity-30 bg-gradient-to-r from-slate-400 via-slate-500 to-slate-400"></div>
        </div>
    </template>

    <!-- ĐƯỜNG PHÂN TRANG ẢO Trên Preview (Chỉ xem trên web) -->
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

// --- CHỨC NĂNG PHÂN TRANG (PAGINATION ENGINE) ---
let paginateTimer = null;
const requestPagination = () => {
    if (paginateTimer) clearTimeout(paginateTimer);
    paginateTimer = setTimeout(doPagination, 50); // Giảm độ trễ để phản hồi nhanh hơn
};

const doPagination = async () => {
    if (!cvRoot.value) return;
    
    const items = cvRoot.value.querySelectorAll('.paginated-item');
    items.forEach(el => { el.style.marginTop = ''; });

    await nextTick();

    const A4_WIDTH_MM = 210;
    const A4_HEIGHT_MM = 297;
    const MARGIN_BOTTOM_MM = 18; // Tăng lề dưới để không chạm vào viền cố định (12mm)
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
    // Tự động bật các trường mặc định theo thiết kế của template này
    const templateSpecificSections = ['education', 'activities', 'project', 'skills', 'certifications', 'awards', 'hobbies'];
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

// Computed colors
const templatePrimaryColor = computed(() => {
    // Nếu là màu xanh biển mặc định của hệ thống, ép nó về Xanh rêu (Olive Green) để giữ thiết kế đặc trưng
    if (!props.resumeData.theme.primaryColor || props.resumeData.theme.primaryColor.toLowerCase() === '#2b5c8f') {
        return '#556050';
    }
    return props.resumeData.theme.primaryColor;
});

const templateSecondaryColor = computed(() => {
    // Màu kem/cát sáng đặc trưng cho cột trái
    return '#e8e4db';
});

// Tách 'summary' ra khỏi các column sections vì nó đã được render cố định ở Header phải
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
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap');

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
    width: 20px;
    height: 20px;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
}

.item-container:hover .delete-btn {
    opacity: 1;
}

/* === HTML CONTENT SUPPORT === */
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

/* Dynamic list marker scaling for Rich Text Editor */
:deep(.html-content li:has(> font[size="1"])) { font-size: 10px; }
:deep(.html-content li:has(> font[size="2"])) { font-size: 12px; }
:deep(.html-content li:has(> font[size="3"])) { font-size: 14px; }
:deep(.html-content li:has(> font[size="4"])) { font-size: 16px; }

/* === SECTION BLOCKS === */
.section-block {
    position: relative;
    border-radius: 6px;
    border: 2px solid transparent; 
    cursor: pointer;
    transition: all 0.15s ease;
}

.section-block:hover {
    background: rgba(0, 0, 0, 0.02);
}

.section-selected {
    /* Viền đơn cách chữ 2mm */
    outline: 1.5px solid var(--sel-color, #1a4731);
    outline-offset: 2mm;
    border-radius: 2px;
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

/* Ẩn khi in */
@media print {
    .no-print { display: none !important; }
    .section-block, .section-selected {
        cursor: default;
        box-shadow: none !important;
        background: transparent !important;
        border-color: transparent !important;
        padding: 0 !important;
        margin: 0 !important;
        border-radius: 0 !important;
    }
}
:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .section-selected) {
    cursor: default !important;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
}
</style>
