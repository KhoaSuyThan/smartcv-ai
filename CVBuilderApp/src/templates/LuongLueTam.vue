<template>
  <div id="cv-printable-area" ref="cvRoot" class="bg-white shadow-2xl w-[210mm] flex flex-row relative box-border text-[#333] leading-relaxed overflow-hidden" :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif !important' }">
    
    <div class="absolute top-[12mm] left-0 w-full h-[65mm] z-30 pointer-events-none flex">
        <div class="w-[125mm] h-[55mm] pl-[12mm] py-[8mm] shadow-lg flex flex-col justify-center pointer-events-auto relative z-20" :style="{ backgroundColor: templateSecondaryColor }">
            <h1 class="text-white/90 uppercase tracking-[0.2em] font-bold leading-none mb-0" :style="{ fontSize: '42px !important' }">{{ splitName.last }}</h1>
            <h1 class="text-white font-cursive leading-none mb-3 tracking-wide" :style="{ fontSize: '64px !important', textShadow: '2px 2px 4px rgba(0,0,0,0.1)' }">{{ splitName.first }}</h1>
            
            <div class="flex items-center gap-3">
                <h2 class="font-bold tracking-[0.15em] uppercase text-[#334155]" :style="{ fontSize: '13px !important' }" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'VỊ TRÍ ỨNG TUYỂN'"></h2>
                <div class="flex-1 h-[2px] bg-[#334155]/60 max-w-[120px]"></div>
            </div>
        </div>

        <div class="flex-1 h-full pr-[15mm] flex items-start justify-end pointer-events-auto">
            <div class="w-[45mm] h-[60mm] bg-gray-200 border-[4px] border-white shadow-md overflow-hidden relative z-30">
                <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
                <div v-else class="w-full h-full flex items-center justify-center text-gray-400">
                    <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
                </div>
            </div>
            <div class="absolute right-[65mm] top-[40mm] w-[1.5px] h-[35mm] bg-slate-300"></div>
        </div>
    </div>

    <aside class="w-[85mm] z-10 flex flex-col shrink-0 relative" :style="{ backgroundColor: templatePrimaryColor }">
        <div class="absolute left-[8mm] top-[75mm] bottom-[15mm] w-[1.5px] bg-white/70 z-10"></div>
        
        <div class="h-[80mm] shrink-0 w-full"></div>

        <div class="pl-[20mm] pr-[5mm] flex-1 pb-8 flex flex-col relative z-20">
            
            <div class="section-block group mb-8 relative">
                <div class="paginated-item relative z-20">
                    <div class="absolute left-[-12mm] top-[10px] w-2.5 h-2.5 rounded-full bg-white z-20 shadow-[0_0_0_3px_var(--primary-color)] transform -translate-x-1/2" :style="{ '--primary-color': templatePrimaryColor }"></div>
                    
                    <h3 class="font-bold uppercase bg-white px-3.5 py-1.5 inline-block max-w-full mb-5 shadow-[3px_3px_0px_rgba(0,0,0,0.15)] text-[13px] tracking-wide break-words whitespace-normal" :style="{ color: templatePrimaryColor }">
                        Liên hệ
                    </h3>
                </div>
                
                <div class="flex flex-col gap-4 font-normal text-white/90 paginated-item" :style="{ fontSize: '11px !important' }">
                    <div class="flex items-center gap-2 relative" v-if="!isEmpty(resumeData.general.phone)">
                        <div class="opacity-80 shrink-0">
                            <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 20 20"><path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z"></path></svg>
                        </div>
                        <span class="break-all font-semibold text-white tracking-wide leading-tight" v-html="resumeData.general.phone"></span>
                    </div>
                    
                    <div class="flex items-center gap-2 relative" v-if="!isEmpty(resumeData.general.email)">
                        <div class="opacity-80 shrink-0">
                            <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 20 20"><path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z"></path><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z"></path></svg>
                        </div>
                        <span class="break-all font-semibold text-white tracking-wide leading-tight" v-html="resumeData.general.email"></span>
                    </div>

                    <div class="flex items-center gap-2 relative" v-if="!isEmpty(resumeData.general.github) || !isEmpty(resumeData.general.website) || !isEmpty(resumeData.general.linkedin)">
                        <div class="opacity-80 shrink-0">
                            <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM4.332 8.027a6.012 6.012 0 011.912-2.706C6.512 5.73 6.974 6 7.5 6A1.5 1.5 0 019 7.5V8a2 2 0 004 0 2 2 0 011.523-1.943A5.977 5.977 0 0116 10c0 .34-.028.675-.083 1H15a2 2 0 00-2 2v2.197A5.973 5.973 0 0110 16v-2a2 2 0 00-2-2 2 2 0 01-2-2 2 2 0 00-1.668-1.973z" clip-rule="evenodd"></path></svg>
                        </div>
                        <span class="break-all font-semibold text-white tracking-wide leading-tight" v-html="!isEmpty(resumeData.general.github) ? resumeData.general.github : (!isEmpty(resumeData.general.linkedin) ? resumeData.general.linkedin : resumeData.general.website)"></span>
                    </div>

                    <div class="flex items-center gap-2 relative" v-if="!isEmpty(resumeData.general.address)">
                        <div class="opacity-80 shrink-0">
                            <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd"></path></svg>
                        </div>
                        <span class="break-all font-semibold text-white tracking-wide leading-tight" v-html="resumeData.general.address"></span>
                    </div>
                </div>
            </div>

            <div v-if="summarySection && summarySection.isVisible" class="section-block group mb-8 relative" :class="{ 'section-selected': selectedSectionId === summarySection.id }" :style="selectedSectionId === summarySection.id ? { '--sel-color': 'white' } : {}" @mouseenter="showNav(summarySection.id)" @mouseleave="hideNav()" @click.stop="selectedSectionId = selectedSectionId === summarySection.id ? null : summarySection.id">
                <div v-show="hoveredSectionId === summarySection.id || selectedSectionId === summarySection.id" class="nav-btns no-print" @mouseenter="showNav(summarySection.id)" @mouseleave="hideNav()">
                     <button @click.stop.prevent="$emit('moveDown', summarySection.id, mainIds)" class="nav-btn" title="Di chuyển xuống"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                </div>
                
                <div class="paginated-item relative z-20">
                    <div class="absolute left-[-12mm] top-[10px] w-2.5 h-2.5 rounded-full bg-white z-20 shadow-[0_0_0_3px_var(--primary-color)] transform -translate-x-1/2" :style="{ '--primary-color': templatePrimaryColor }"></div>
                    <h3 class="font-bold uppercase bg-white px-3.5 py-1.5 inline-block max-w-full mb-5 shadow-[3px_3px_0px_rgba(0,0,0,0.15)] text-[13px] tracking-wide break-words whitespace-normal" :style="{ color: templatePrimaryColor }">{{ summarySection.title || 'Mục tiêu' }}</h3>
                </div>
                
                <div class="leading-relaxed text-justify html-content font-medium text-white/90 paginated-item" :style="{ fontSize: '11px !important' }" v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Trình bày ngắn gọn về số năm kinh nghiệm và công việc...' "></div>
            </div>

            <template v-for="section in sidebarSections" :key="section.id">
                <div
                    v-show="section.isVisible"
                    class="section-block group mb-8 relative"
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

                    <div class="paginated-item relative z-20">
                        <div class="absolute left-[-12mm] top-[10px] w-2.5 h-2.5 rounded-full bg-white z-20 shadow-[0_0_0_3px_var(--primary-color)] transform -translate-x-1/2" :style="{ '--primary-color': templatePrimaryColor }"></div>
                        <h3 class="font-bold uppercase bg-white px-3.5 py-1.5 inline-block max-w-full mb-5 shadow-[3px_3px_0px_rgba(0,0,0,0.15)] text-[13px] tracking-wide break-words whitespace-normal" :style="{ color: templatePrimaryColor }">
                            {{ section.title }}
                        </h3>
                    </div>
                    
                    <div class="space-y-4">
                        <div v-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'" class="space-y-4">
                           <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-white item-container leading-relaxed flex flex-col paginated-item" :style="{ fontSize: '11px !important' }">
                               <div class="flex items-center gap-2 mb-1.5">
                                   <div class="w-1.5 h-1.5 rounded-full bg-white shrink-0"></div>
                                   <span class="font-bold tracking-wide">{{ item.name }}</span>
                               </div>
                               <div class="w-full h-[5px] bg-white/20 rounded-full overflow-hidden relative">
                                   <div class="absolute left-0 top-0 bottom-0" :style="{ width: item.level ? item.level + '%' : '80%', backgroundColor: templateSecondaryColor }"></div>
                               </div>
                               
                               <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 opacity-0 group-hover:opacity-100 transition-opacity absolute right-0 top-0">
                                   <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                               </button>
                           </div>
                        </div>

                        <div v-else class="space-y-3.5">
                           <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-white font-medium leading-relaxed item-container relative paginated-item" :style="{ fontSize: '11px !important' }">
                               <div class="flex items-start gap-2">
                                   <div class="w-1.5 h-1.5 rounded-full bg-white shrink-0 mt-[6px]"></div>
                                   <span class="html-content text-white/90" v-html="formatDesc(item.desc || item.name || item.info)"></span>
                               </div>
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

    <main class="flex-1 flex flex-col relative bg-white z-20 min-h-max" @click.self="selectedSectionId = null">
        
        <div class="h-[80mm] shrink-0 w-full border-l-[1.5px] border-slate-200 ml-5"></div>

        <div class="pl-[12mm] pr-[15mm] flex-1 flex flex-col pb-[12mm] relative gap-2">
            
            <template v-for="section in mainSections" :key="section.id">
                <div
                    v-show="section.isVisible"
                    class="section-block group my-0 py-2 mb-8"
                    :class="{ 'section-selected': selectedSectionId === section.id }"
                    :style="selectedSectionId === section.id ? { '--sel-color': templateSecondaryColor } : {}"
                    @mouseenter="showNav(section.id)"
                    @mouseleave="hideNav()"
                    @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
                >
                    <div v-show="hoveredSectionId === section.id || selectedSectionId === section.id" class="nav-btns no-print" @mouseenter="showNav(section.id)" @mouseleave="hideNav()">
                        <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                        <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                        <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
                    </div>

                    <div class="flex items-center gap-4 mb-6 paginated-item relative left-[-26px]">
                        <div class="w-9 h-9 rounded-full bg-[#dbe2cd] border-[4px] flex items-center justify-center shrink-0 shadow-sm" :style="{ borderColor: templateSecondaryColor }">
                            <div class="w-3 h-3 rounded-full" :style="{ backgroundColor: templatePrimaryColor }"></div>
                        </div>
                        <h3 class="font-bold uppercase text-white px-5 py-2 shadow-[3px_3px_0px_rgba(0,0,0,0.15)] text-[15px] tracking-wider" :style="{ backgroundColor: templateSecondaryColor }">
                            {{ section.title }}
                        </h3>
                    </div>

                    <div class="space-y-6">
                        <div v-if="section.id === 'education' || section.id === 'experience' || section.id === 'project' || section.id === 'activities'" class="space-y-6">
                            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative flex items-stretch paginated-item">
                                
                                <div class="w-[35%] pr-4 relative shrink-0">
                                    <div class="absolute left-[-16px] top-[6px] w-1.5 h-1.5 rounded-full bg-slate-400"></div>
                                    <h4 class="text-[#222] font-bold leading-snug mb-1.5" :style="{ fontSize: '11.5px !important' }">
                                        <span v-html="section.id === 'education' ? item.school : (section.id === 'experience' ? item.company : item.name)"></span>
                                    </h4>
                                    <div v-if="item.year || item.time" class="inline-block text-white font-bold px-2 py-0.5 shadow-[2px_2px_0px_rgba(0,0,0,0.15)] mt-1 tracking-wide" :style="{ backgroundColor: templateSecondaryColor, fontSize: '10px !important' }">
                                        {{ item.year || item.time }}
                                    </div>
                                </div>

                                <div class="w-[65%] border-l-2 border-slate-300 pl-4 pb-2 relative">
                                    <h4 class="text-[#333] font-bold mb-1.5" :style="{ fontSize: '11.5px !important' }" v-if="item.major || item.role">
                                        {{ item.major || item.role }}
                                    </h4>
                                    <div class="text-slate-700 html-content leading-relaxed text-justify font-medium" :style="{ fontSize: '11px !important' }" v-html="formatDesc(item.desc)"></div>
                                </div>

                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 absolute right-[-10px] top-[-5px]">
                                    <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                            </div>
                        </div>
                        
                        <div v-else-if="section.id === 'awards' || section.id === 'certifications'" class="space-y-4">
                            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative flex items-stretch paginated-item">
                                <div class="w-[35%] pr-4 relative shrink-0">
                                    <div class="absolute left-[-16px] top-[6px] w-1.5 h-1.5 rounded-full bg-slate-400"></div>
                                    <div v-if="item.year" class="inline-block text-white font-bold px-2 py-0.5 shadow-[2px_2px_0px_rgba(0,0,0,0.15)] tracking-wide" :style="{ backgroundColor: templateSecondaryColor, fontSize: '10px !important' }">
                                        {{ item.year }}
                                    </div>
                                </div>
                                <div class="w-[65%] border-l-2 border-slate-300 pl-4 pb-2 relative">
                                    <span class="font-bold text-[#333] html-content" :style="{ fontSize: '11.5px !important' }">{{ item.name || item.info }}</span>
                                </div>

                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 absolute right-[-10px] top-[-5px]">
                                    <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                            </div>
                        </div>

                        <div v-else class="space-y-4 pl-[4px]">
                            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative text-[#333] leading-relaxed paginated-item" :style="{ fontSize: '11px !important' }">
                                <div class="absolute left-[-20px] top-[6px] w-1.5 h-1.5 rounded-full bg-slate-400"></div>
                                <div class="html-content font-medium text-justify" v-html="formatDesc(item.desc || item.info || item.name)"></div>
                                
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

// --- CHỨC NĂNG PHÂN TRANG ĐÃ FIX LỖI LỒNG NHAU ---
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
    await new Promise(r => setTimeout(r, 50));

    const A4_WIDTH_MM = 210;
    const A4_HEIGHT_MM = 297;
    const MARGIN_BOTTOM_MM = 15; 
    const MARGIN_TOP_MM = 15;    
    
    const rootRect = cvRoot.value.getBoundingClientRect();
    const rootWidthPx = rootRect.width;
    const pxPerMm = rootWidthPx / A4_WIDTH_MM;
    const pageHeightPx = A4_HEIGHT_MM * pxPerMm;
    const marginBottomPx = MARGIN_BOTTOM_MM * pxPerMm;
    const marginTopPx = MARGIN_TOP_MM * pxPerMm;
    const safeBottomPx = pageHeightPx - marginBottomPx;

    const getRelativeTop = (el) => {
        const elRect = el.getBoundingClientRect();
        const currentRootRect = cvRoot.value.getBoundingClientRect();
        return elRect.top - currentRootRect.top;
    };

    const processColumn = (colSelector) => {
        const col = cvRoot.value.querySelector(colSelector);
        if (!col) return;
        const colItems = col.querySelectorAll('.paginated-item');

        let isStable = false;
        let attempts = 0;
        
        while (!isStable && attempts < 100) {
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

    const calculatedPageCount = Math.max(1, Math.ceil((maxBottom + marginBottomPx) / pageHeightPx));
    
    if (pageCount.value !== calculatedPageCount) {
        pageCount.value = calculatedPageCount;
    }
};

watch(() => props.resumeData, () => {
    requestPagination();
}, { deep: true });

onMounted(() => {
    const templateSpecificSections = ['education', 'experience', 'skills', 'certifications'];
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
// --- END ---

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

const splitName = computed(() => {
    let full = props.resumeData.general.fullName || 'LƯƠNG Tuệ Lâm';
    let parts = full.trim().split(' ');
    if (parts.length > 1) {
        let last = parts[0]; 
        let first = parts.slice(1).join(' '); 
        return { first, last };
    }
    return { first: full, last: '' };
});

const templatePrimaryColor = computed(() => {
    if (!props.resumeData.theme.primaryColor || props.resumeData.theme.primaryColor.toLowerCase() === '#2b5c8f') {
        return '#465568'; 
    }
    return props.resumeData.theme.primaryColor;
});

const templateSecondaryColor = computed(() => {
    return '#9db078';
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
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=Dancing+Script:wght@700&display=swap');

.font-cursive {
    font-family: 'Dancing Script', cursive !important;
}

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

:deep(.html-content ul) {
    list-style-type: disc !important;
    padding-left: 1rem !important;
    margin-top: 0.25rem;
    margin-bottom: 0.25rem;
}
:deep(.html-content ol) {
    list-style-type: decimal !important;
    padding-left: 1rem !important;
    margin-top: 0.25rem;
    margin-bottom: 0.25rem;
}
:deep(.html-content b), :deep(.html-content strong) { font-weight: 700; color: #222; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic; }
:deep(.html-content u) { text-decoration: underline; }
:deep(.html-content ul li), :deep(.html-content ol li) { margin-bottom: 0.25rem; }

.section-block {
    position: relative;
    border-radius: 2px;
    border: 2px solid transparent; 
    cursor: pointer;
    transition: all 0.15s ease;
}

.section-block:hover {
    background: rgba(0, 0, 0, 0.02);
}

.section-selected {
    outline: 1.5px solid var(--sel-color, #9db078);
    outline-offset: 2mm;
    border-radius: 2px;
}

.nav-btns {
    position: absolute;
    right: 5px;
    top: -15px;
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