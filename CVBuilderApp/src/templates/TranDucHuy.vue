<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-row relative box-border text-[#333] leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif' }"
    @click.self="selectedSectionId = null"
  >
    <!-- LEFT COLUMN -->
    <aside class="w-[70mm] z-10 flex flex-col pt-0 shrink-0 relative bg-[#676f7a] text-white">
      <!-- Avatar Section -->
      <div class="relative w-[70mm] h-[85mm] bg-[#5e6671] shrink-0 border-b-[4px] border-[#b1b8c6]">
        <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
        <div v-else class="w-full h-full flex items-center justify-center text-white/50 bg-[#676f7a]">
          <svg class="w-20 h-20" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
          </svg>
        </div>
      </div>

      <!-- Left Sidebar Content -->
      <div class="px-[6mm] py-[8mm] flex flex-col gap-6">
        <!-- Contact Info -->
        <div class="space-y-3 text-[12.5px] paginated-item font-medium tracking-wide" v-if="!isEmpty(resumeData.general.phone) || !isEmpty(resumeData.general.email) || !isEmpty(resumeData.general.address) || !isEmpty(resumeData.general.website) || !isEmpty(resumeData.general.linkedin) || !isEmpty(resumeData.general.dob)">
            <div class="flex items-center gap-3" v-if="!isEmpty(resumeData.general.dob)">
                <svg class="w-[14px] h-[14px] shrink-0" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" clip-rule="evenodd"></path></svg>
                <span v-html="resumeData.general.dob" class="break-all"></span>
            </div>
            <div class="flex items-center gap-3" v-if="!isEmpty(resumeData.general.phone)">
                <svg class="w-[14px] h-[14px] shrink-0" fill="currentColor" viewBox="0 0 20 20"><path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z"></path></svg>
                <span v-html="resumeData.general.phone" class="break-all"></span>
            </div>
            <div class="flex items-center gap-3" v-if="!isEmpty(resumeData.general.email)">
                <svg class="w-[14px] h-[14px] shrink-0" fill="currentColor" viewBox="0 0 20 20"><path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z"></path><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z"></path></svg>
                <span v-html="resumeData.general.email" class="break-all"></span>
            </div>
            <div class="flex items-center gap-3" v-if="!isEmpty(resumeData.general.address)">
                <svg class="w-[14px] h-[14px] shrink-0" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd"></path></svg>
                <span v-html="resumeData.general.address" class="break-all"></span>
            </div>
            <div class="flex items-center gap-3" v-if="!isEmpty(resumeData.general.website) || !isEmpty(resumeData.general.linkedin)">
                <svg class="w-[14px] h-[14px] shrink-0" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM4.332 8.027a6.012 6.012 0 011.912-2.706C6.512 5.73 6.974 6 7.5 6A1.5 1.5 0 019 7.5V8a2 2 0 004 0 2 2 0 011.523-1.943A5.977 5.977 0 0116 10c0 .34-.028.675-.083 1H15a2 2 0 00-2 2v2.197A5.973 5.973 0 0110 16v-2a2 2 0 00-2-2 2 2 0 01-2-2 2 2 0 00-1.668-1.973z" clip-rule="evenodd"></path></svg>
                <span v-html="resumeData.general.website || resumeData.general.linkedin" class="break-all"></span>
            </div>
        </div>

        <hr class="border-t-[1px] border-white/60 mb-1" />

        <!-- Sidebar Sections -->
        <template v-for="section in sidebarSections" :key="section.id">
            <div
                v-show="section.isVisible"
                class="section-block"
                :class="{ 'section-selected': selectedSectionId === section.id }"
                :style="selectedSectionId === section.id ? { '--sel-color': '#b1b8c6' } : {}"
                @mouseenter="showNav(section.id)"
                @mouseleave="hideNav()"
                @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
            >
                <div v-show="hoveredSectionId === section.id || selectedSectionId === section.id" class="nav-btns no-print" @mouseenter="showNav(section.id)" @mouseleave="hideNav()">
                    <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                    <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                    <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7-7"/></svg></button>
                </div>

                <!-- SUMMARY SECTION (No title, no bottom border for title) -->
                <div v-if="section.id.toLowerCase().includes('summary')">
                     <div class="text-[12.5px] leading-[1.6] text-justify font-medium html-content text-white/90 paginated-item" v-html="formatDesc(resumeData.general.summary || 'Chưa có thông tin mục tiêu nghề nghiệp.')"></div>
                </div>

                <!-- OTHER SIDEBAR SECTIONS -->
                <div v-else>
                    <h3 class="font-bold pb-2 border-b-[1px] border-white/60 mb-2 tracking-wide text-white paginated-item" style="font-size: 14px;">
                        {{ section.title.charAt(0).toUpperCase() + section.title.slice(1).toLowerCase() }}
                    </h3>
                    
                    <div class="space-y-4">
                        <!-- Skills -->
                        <div v-if="['skill', 'lang', 'it_skill'].some(k => section.id.toLowerCase().includes(k))" class="space-y-3">
                           <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[12.5px] text-white/90 item-container pr-8 min-h-[22px] flex flex-col justify-center">
                               <div class="paginated-item w-full flex flex-col">
                                   <span class="font-bold text-[12.5px]">{{ item.name }}</span> 
                                   <span v-if="item.level || item.info" class="font-normal mt-0.5">{{ item.level || item.info }}</span>
                               </div>
                               <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30">
                                   <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                               </button>
                           </div>
                        </div>

                        <!-- Certifications & Awards -->
                        <div v-else-if="['award', 'cert'].some(k => section.id.toLowerCase().includes(k))" class="space-y-3">
                           <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[12.5px] item-container">
                               <div class="paginated-item" v-if="item.year"><p class="text-[12.5px] text-white/70 mb-0.5">{{ item.year }}</p></div>
                               <div class="paginated-item font-bold text-[12.5px] text-white/90 html-content" v-html="formatDesc(item.name || item.info)"></div>
                               <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30">
                                   <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                               </button>
                           </div>
                        </div>

                        <!-- Universal Fallback for Left Column -->
                        <div v-else class="space-y-3">
                           <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[12.5px] text-white/90 font-medium leading-relaxed item-container">
                                <!-- Main Title -->
                                <div class="paginated-item font-bold text-[13px]" v-if="item.name || item.title || item.company || item.role || item.major || item.school">
                                    {{ section.id.toLowerCase().includes('experience') ? (item.company || item.organization || item.name || item.role) : (item.name || item.title || item.role || item.major || item.school) }}
                                </div>
                                
                                <!-- Sub Title -->
                                <div class="paginated-item text-[12.5px] font-bold opacity-90 mt-0.5" v-if="item.company || item.organization || item.role || item.major">
                                    {{ section.id.toLowerCase().includes('experience') ? (item.role || item.position) : (item.company || item.organization || item.role || item.major) }}
                                </div>

                                <!-- Meta & Time -->
                                <div class="paginated-item text-[12.5px] text-white/70 mt-0.5" v-if="item.time || item.year || item.level">{{ item.time || item.year || item.level }}</div>

                                <!-- GPA & GradType -->
                                <div class="paginated-item text-[12px] font-medium text-[#cbd5e1] mt-0.5" v-if="item.gradType || item.gpa">
                                    <span v-if="item.gradType">Loại: {{ item.gradType }}</span>
                                    <span v-if="item.gradType && item.gpa"> | </span>
                                    <span v-if="item.gpa">GPA: {{ item.gpa }}</span>
                                </div>

                                <!-- Description -->
                                <div class="html-content mt-1" v-if="item.desc || item.info" v-html="formatDesc(item.desc || item.info)"></div>

                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30">
                                    <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                           </div>
                        </div>
                    </div>
                </div>
            </div>
        </template>
      </div>
    </aside>

    <!-- RIGHT COLUMN -->
    <main class="flex-1 bg-white flex flex-col relative z-10 w-[140mm]">
        <!-- HEADER -->
        <header class="bg-[#b1b8c6] h-[85mm] pt-[15mm] px-[12mm] flex flex-col justify-center shrink-0 paginated-item relative z-20 items-start box-border">
            <h1 class="text-[26px] font-black uppercase text-[#1e232b] tracking-tight mb-2 leading-[1.1]" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'HỌ VÀ TÊN'"></h1>
            <h2 class="text-[13.5px] font-medium text-[#2f3540]" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'Vị trí ứng tuyển'"></h2>
        </header>

        <!-- RIGHT SECTIONS -->
        <div class="px-[10mm] py-[6mm] flex flex-col gap-6">
            <template v-for="section in mainSections" :key="section.id">
                <div
                    v-show="section.isVisible"
                    class="section-block"
                    :class="{ 'section-selected': selectedSectionId === section.id }"
                    :style="selectedSectionId === section.id ? { '--sel-color': '#b1b8c6' } : {}"
                    @mouseenter="showNav(section.id)"
                    @mouseleave="hideNav()"
                    @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
                >
                    <div v-show="hoveredSectionId === section.id || selectedSectionId === section.id" class="nav-btns no-print" @mouseenter="showNav(section.id)" @mouseleave="hideNav()">
                        <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                        <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                        <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái"><svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
                    </div>

                    <div class="flex items-stretch mb-2 paginated-item w-[110mm]">
                        <div class="font-bold text-white bg-[#676f7a] px-4 tracking-wider whitespace-nowrap flex items-center justify-center py-1.5" style="font-size: 15px;">
                            {{ section.title.charAt(0).toUpperCase() + section.title.slice(1).toLowerCase() }}
                        </div>
                        <div class="flex-1 bg-[#b1b8c6]"></div>
                    </div>

                    <div class="space-y-6">
                        <!-- Summary -->
                        <div v-if="section.id.toLowerCase().includes('summary')" class="text-[13px] leading-[1.6] text-slate-800 text-justify font-medium html-content" v-html="formatDesc(resumeData.general.summary || 'Chưa có thông tin mục tiêu nghề nghiệp.')">
                        </div>

                        <!-- Education -->
                        <div v-else-if="section.id.toLowerCase().includes('education')" class="space-y-6">
                            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container p-2 -m-2 rounded-lg">
                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 scale-125">
                                    <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                                <div class="paginated-item mb-2">
                                    <div class="font-bold text-[14px] text-slate-900 mb-0.5">{{ item.school }}</div>
                                    <div class="text-[13px] font-medium text-slate-400 mb-1">{{ item.major }}</div>
                                    <div class="text-[12.5px] text-slate-800">{{ item.year }}</div>
                                    <div class="text-[12.5px] text-slate-700 mt-0.5" v-if="item.gradType || item.gpa">
                                        <span v-if="item.gradType">Loại: <strong>{{ item.gradType }}</strong></span>
                                        <span v-if="item.gradType && item.gpa"> | </span>
                                        <span v-if="item.gpa">GPA: <strong>{{ item.gpa }}</strong></span>
                                    </div>
                                </div>
                                <div v-if="item.desc" class="text-[13px] leading-[1.6] text-slate-800 text-justify font-medium html-content" v-html="formatDesc(item.desc)"></div>
                            </div>
                        </div>

                        <!-- Experience / Project / Activities -->
                        <div v-else-if="['experience', 'project', 'activit'].some(k => section.id.toLowerCase().includes(k))" class="space-y-6">
                            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container p-2 -m-2 rounded-lg">
                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 scale-125">
                                    <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                                <div class="paginated-item mb-2">
                                    <div class="font-bold text-[14px] text-slate-900 mb-0.5">
                                        {{ section.id.toLowerCase().includes('experience') ? (item.company || item.organization || item.name) : (item.name || item.title) }}
                                    </div>
                                    <div class="text-[13px] font-medium text-slate-500 mb-1">
                                        {{ section.id.toLowerCase().includes('experience') ? (item.role || item.position) : (item.role || item.company) }}
                                    </div>
                                    <div class="text-[12.5px] text-slate-800 font-bold">{{ item.time || item.year }}</div>
                                </div>
                                <div v-if="item.desc" class="text-[13px] leading-[1.6] text-slate-800 text-justify font-medium html-content" v-html="formatDesc(item.desc)"></div>
                            </div>
                        </div>

                        <!-- Universal Fallback for Right Column -->
                        <div v-else class="space-y-6">
                            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container p-2 -m-2 rounded-lg">
                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 scale-125">
                                    <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                                
                                <div class="paginated-item mb-2">
                                    <div style="display: flex; justify-content: space-between; align-items: baseline; gap: 8px;">
                                        <div class="font-bold text-[14px] text-slate-900 mb-0.5">{{ item.name || item.title || item.role || item.position || item.major || item.school }}</div>
                                        <div class="text-[12.5px] text-slate-800 font-bold whitespace-nowrap" v-if="item.time || item.year">{{ item.time || item.year }}</div>
                                    </div>
                                    
                                    <div class="text-[13px] font-medium text-slate-400 mb-1" v-if="item.company || item.organization || (item.school && item.major)">
                                        {{ item.company || item.organization || (item.school ? item.school : '') }}
                                    </div>
                                    <div class="text-[13px] font-medium text-slate-400 mb-1" v-else-if="(item.name || item.title) && item.role">{{ item.role }}</div>
                                    
                                    <div class="text-[13px] font-bold text-slate-600 mb-1" v-if="item.level">{{ item.level }}</div>
                                    
                                    <div class="text-[12.5px] text-slate-700 mt-0.5" v-if="item.gradType || item.gpa">
                                        <span v-if="item.gradType">Loại: <strong>{{ item.gradType }}</strong></span>
                                        <span v-if="item.gradType && item.gpa"> | </span>
                                        <span v-if="item.gpa">GPA: <strong>{{ item.gpa }}</strong></span>
                                    </div>
                                </div>
                                
                                <div v-if="item.desc" class="text-[13px] leading-[1.6] text-slate-800 text-justify font-medium html-content" v-html="formatDesc(item.desc)"></div>
                                <div v-else-if="item.info" class="text-[13px] leading-[1.6] text-slate-800 text-justify font-medium html-content" v-html="formatDesc(item.info)"></div>
                            </div>
                        </div>
                    </div>
                </div>
            </template>
        </div>
    </main>

    <!-- ĐƯỜNG PHÂN TRANG -->
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

const props = defineProps({
    resumeData: { type: Object, required: true }
})

const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

const formatDesc = (text) => {
    if (!text) return '';
    
    if (!/<[a-z][\s\S]*>/i.test(text)) {
        return text.split('\n')
                   .map(l => l.trim())
                   .filter(Boolean)
                   .map(l => `<div class="paginated-item w-full block">${l}</div>`)
                   .join('');
    }

    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = text;
    
    tempDiv.querySelectorAll('li').forEach(li => {
        const child = li.firstElementChild;
        if (child && (child.tagName === 'FONT' || child.tagName === 'SPAN')) {
            if (child.color) li.style.color = child.color;
            if (child.style && child.style.color) li.style.color = child.style.color;
        }
    });

    const blockTags = ['P', 'DIV', 'LI', 'H1', 'H2', 'H3', 'H4', 'H5', 'H6'];
    
    tempDiv.querySelectorAll('*').forEach(el => {
        if (blockTags.includes(el.tagName.toUpperCase())) {
            const hasBlockChild = Array.from(el.children).some(child => blockTags.includes(child.tagName.toUpperCase()));
            if (!hasBlockChild) {
                el.classList.add('paginated-item');
            }
        }
    });

    Array.from(tempDiv.childNodes).forEach(node => {
        if (node.nodeType === Node.TEXT_NODE && node.textContent.trim()) {
           const wrapper = document.createElement('div');
           wrapper.className = 'paginated-item w-full block';
           node.replaceWith(wrapper);
           wrapper.appendChild(node);
        }
    });

    return tempDiv.innerHTML;
};

// --- PAGINATION ENGINE ---
let paginateTimer = null;
const requestPagination = () => {
    if (paginateTimer) clearTimeout(paginateTimer);
    paginateTimer = setTimeout(doPagination, 60);
};

const doPagination = async () => {
    if (!cvRoot.value) return;

    // 1. Màng lọc thông minh: Ngăn chặn lỗi lồng margin, chỉ lấy các block ngoài cùng
    const allElements = Array.from(cvRoot.value.querySelectorAll('.paginated-item')).filter(el => {
        if (el.offsetHeight === 0) return false;
        let parent = el.parentElement;
        while (parent && parent !== cvRoot.value) {
            if (parent.classList.contains('paginated-item')) return false;
            parent = parent.parentElement;
        }
        return true;
    });

    // 2. Đóng băng các hiệu ứng Transform/Hover để đo đạc chính xác 100%
    const activeElements = cvRoot.value.querySelectorAll('.section-selected, .section-block');
    activeElements.forEach(el => el.style.setProperty('transform', 'none', 'important'));

    // Reset lại toàn bộ margin cũ
    allElements.forEach(el => {
        el.style.setProperty('margin-top', '0px', 'important');
    });
    
    // Xóa margin trên các container để tránh xung đột
    const allContainers = Array.from(cvRoot.value.querySelectorAll('.item-container'));
    allContainers.forEach(el => el.style.setProperty('margin-top', '0px', 'important'));

    await nextTick();

    // 3. THUẬT TOÁN KHỬ SCALE: Đo lường tỷ lệ thu phóng hiện tại của trình duyệt/editor
    const offsetW = cvRoot.value.offsetWidth;
    const cvRect = cvRoot.value.getBoundingClientRect();
    const scale = offsetW ? cvRect.width / offsetW : 1; 

    const A4_W_MM = 210;
    const A4_H_MM = 297;
    const pxPerMm = offsetW / A4_W_MM; // Dùng chiều rộng gốc chưa bị scale
    const pageH = A4_H_MM * pxPerMm;
    
    // Căn lề chuẩn cho mẫu CV này: Đáy 15mm, Đỉnh trang mới 15mm
    const bottomSafeZone = 15 * pxPerMm;
    const topMargin = 15 * pxPerMm;

    let stable = false;
    let passes = 0;

    // 4. Vòng lặp quét từ trên xuống dưới, đụng lề là đẩy
    while (!stable && passes < 40) {
        stable = true;
        passes++;
        
        const currentCvRect = cvRoot.value.getBoundingClientRect();

        for (let i = 0; i < allElements.length; i++) {
            const el = allElements[i];
            if (el.offsetHeight === 0) continue;

            const elRect = el.getBoundingClientRect();
            
            // Chia cho scale để đưa tọa độ về lại kích thước chuẩn 100%
            const top = (elRect.top - currentCvRect.top) / scale;
            const height = elRect.height / scale;
            
            const pageIndex = Math.floor(top / pageH);
            const topInPage = top - (pageIndex * pageH);
            const bottomInPage = topInPage + height;

            // Bỏ qua nếu có khối nội dung nào đó dài hơn cả 1 trang giấy
            if (height > (pageH - bottomSafeZone - topMargin)) continue;

            // Chạm vùng an toàn đáy -> Lập tức bẻ sang trang mới
            if (bottomInPage > (pageH - bottomSafeZone)) {
                const distToNextPage = pageH - topInPage + topMargin;
                const currentMt = parseFloat(el.style.marginTop || '0');
                
                // Gắn margin pixel chuẩn, trình duyệt sẽ tự scale lại cho khớp giao diện
                el.style.setProperty('margin-top', `${currentMt + distToNextPage}px`, 'important');
                stable = false; // Reset vòng lặp để đo lại khối bên dưới
                break;
            }
        }
    }

    // 5. Trả lại hiệu ứng hover cho giao diện
    activeElements.forEach(el => el.style.removeProperty('transform'));

    // 6. Chốt số trang thực tế cần xuất PDF
    const finalCvRect = cvRoot.value.getBoundingClientRect();
    let maxBottom = 0;
    allElements.forEach(el => {
        const bottom = (el.getBoundingClientRect().bottom - finalCvRect.top) / scale;
        if (bottom > maxBottom) maxBottom = bottom;
    });

    pageCount.value = Math.max(1, Math.ceil(maxBottom / pageH));
};

watch(() => props.resumeData, () => {
    requestPagination();
}, { deep: true });

onMounted(() => {
    if (props.resumeData?.sections) {
        // Chỉ active đúng các mục có trong ảnh thiết kế
        const ACTIVE_SECTIONS = ['summary', 'skills', 'certifications', 'hobbies', 'education', 'activities', 'project'];
        
        const RIGHT_IDS = ['education', 'experience', 'project', 'activities'];
        const LEFT_IDS  = ['summary', 'skills', 'it_skills', 'languages', 'certifications', 'awards', 'hobbies'];

        props.resumeData.sections.forEach(sec => {
            const isDefaultVisible = ACTIVE_SECTIONS.some(k => sec.id.toLowerCase().includes(k));
            if (sec.isVisible === undefined) sec.isVisible = isDefaultVisible;

            if (!sec.column) {
                if (LEFT_IDS.some(k => sec.id.toLowerCase().includes(k))) {
                    sec.column = 'left';
                } else {
                    sec.column = 'right';
                }
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
    top: 2px;
    right: 2px;
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

.item-container:hover {
    background-color: rgba(0, 0, 0, 0.03);
}

aside .item-container:hover {
    background-color: rgba(255, 255, 255, 0.05);
}
</style>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&display=swap');

#cv-printable-area {
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
    overflow-wrap: anywhere;
}

.paginated-item {
    transition: none !important;
}

:deep(.html-content p) { margin-bottom: 0.25rem !important; }
:deep(.html-content ul) {
    list-style-type: disc !important;
    padding-left: 1.5rem !important;
    margin-top: 0.25rem;
    margin-bottom: 0.25rem;
}
:deep(.html-content ol) {
    list-style-type: decimal !important;
    padding-left: 1.5rem !important;
    margin-top: 0.25rem;
    margin-bottom: 0.25rem;
}
:deep(.html-content b), :deep(.html-content strong) { font-weight: bold; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic; }
:deep(.html-content u) { text-decoration: underline; }
:deep(.html-content ul li), :deep(.html-content ol li) { margin-bottom: 0.25rem; }

:deep(.html-content li:has(> font[size="1"])) { font-size: 10px; }
:deep(.html-content li:has(> font[size="2"])) { font-size: 13px; }
:deep(.html-content li:has(> font[size="3"])) { font-size: 16px; }
:deep(.html-content li:has(> font[size="4"])) { font-size: 18px; }

.section-block {
    position: relative;
    border-radius: 8px;
    padding: 12px 14px;
    margin: -12px -14px;
    border: 2px solid transparent; 
    cursor: pointer;
    transition: border-color 0.2s ease, background 0.2s ease, box-shadow 0.2s ease;
}

.section-block:hover {
    background: rgba(0, 0, 0, 0.02);
}

aside .section-block:hover {
    background: rgba(255, 255, 255, 0.05);
}

.section-selected {
    border-color: var(--sel-color, #2563eb);
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.06);
    background: color-mix(in srgb, var(--sel-color, #2563eb) 8%, white) !important;
}

aside .section-selected {
    border-color: #b1b8c6;
    background: rgba(255, 255, 255, 0.1) !important;
}

.nav-btns {
    position: absolute;
    right: 8px;
    top: 8px;
    display: flex;
    flex-direction: row;
    gap: 6px;
    z-index: 9999;
}

.nav-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 5px;
    background: #475569;
    color: white;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
    transition: background 0.15s ease, transform 0.1s ease;
}

.nav-btn:hover { background: #334155; transform: scale(1.1); }
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
