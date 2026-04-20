<template>
  <div id="cv-printable-area" ref="cvRoot" class="bg-[white] flex w-[210mm] relative box-border text-[13px] leading-relaxed" :style="{ minHeight: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif' }">
    
    <!-- CỘT TRÁI (SIDEBAR) -->
    <aside class="w-[75mm] flex-shrink-0 bg-[#f6efe9] text-gray-800 flex flex-col relative z-20 pb-[10mm]" @click.self="selectedSectionId = null">
       
        <!-- HỌ TÊN VÀ VỊ TRÍ -->
        <div class="px-[8mm] pt-[12mm] pb-[4mm] paginated-item text-center">
            <h1 class="text-[28px] font-bold uppercase leading-tight mb-2" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'HỌ VÀ TÊN'"></h1>
            <h2 class="text-[13px] font-semibold uppercase tracking-wide text-gray-800" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'CHUYÊN VIÊN SALES ADMIN'"></h2>
        </div>

        <!-- AVATAR - HÌNH TRÒN CÓ VIỀN -->
        <div class="px-[8mm] pt-[1mm] pb-[6mm] paginated-item flex justify-center">
            <div class="w-[50mm] h-[50mm] rounded-full overflow-hidden bg-white border-[2.5px] border-white/10 relative z-10 box-border flex-shrink-0" :style="{ borderColor: resumeData.theme.primaryColor || '#8A3841' }">
                <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
                <div v-else class="w-full h-full flex items-center justify-center text-gray-400">
                    <svg class="w-20 h-20" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
                </div>
            </div>
        </div>

        <!-- CÁC SECTION TRONG SIDEBAR -->
        <div class="px-[8mm] flex flex-col gap-5 flex-1 mt-2">
            <!-- THÔNG TIN CÁ NHÂN -->
            <div class="paginated-item w-full section-block-sidebar py-[2mm] px-[2mm] -mx-[2mm]">
                <div class="mb-4">
                    <h3 class="font-bold text-[16px] whitespace-nowrap" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }">Thông tin cá nhân</h3>
                </div>
                <div class="space-y-3.5 text-[12.5px] font-medium text-gray-900">
                    <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.birthDate)">
                       <div class="w-[15px] h-[15px] flex-shrink-0 mt-[2px]" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }">
                           <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect><line x1="16" y1="2" x2="16" y2="6"></line><line x1="8" y1="2" x2="8" y2="6"></line><line x1="3" y1="10" x2="21" y2="10"></line></svg>
                       </div>
                       <span class="leading-tight pt-[1px]" v-html="resumeData.general.birthDate"></span>
                    </div>
                    <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.gender)">
                       <div class="w-[15px] h-[15px] flex-shrink-0 mt-[2px]" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }">
                           <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path><circle cx="12" cy="7" r="4"></circle></svg>
                       </div>
                       <span class="leading-tight pt-[1px]" v-html="resumeData.general.gender"></span>
                    </div>
                    <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.phone)">
                       <div class="w-[15px] h-[15px] flex-shrink-0 mt-[2px]" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }">
                           <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"></path></svg>
                       </div>
                       <span class="break-all leading-tight pt-[1px]" v-html="resumeData.general.phone"></span>
                    </div>
                    <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.email)">
                       <div class="w-[15px] h-[15px] flex-shrink-0 mt-[2px]" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }">
                           <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path><polyline points="22,6 12,13 2,6"></polyline></svg>
                       </div>
                       <span class="break-all leading-tight pt-[1px]" v-html="resumeData.general.email"></span>
                    </div>
                    <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.website)">
                       <div class="w-[15px] h-[15px] flex-shrink-0 mt-[2px]" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }">
                           <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="2" y1="12" x2="22" y2="12"></line><path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"></path></svg>
                       </div>
                       <span class="break-all leading-tight pt-[1px]" v-html="resumeData.general.website"></span>
                    </div>
                    <div class="flex items-start gap-3" v-if="!isEmpty(resumeData.general.address)">
                       <div class="w-[15px] h-[15px] flex-shrink-0 mt-[2px]" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }">
                           <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path><circle cx="12" cy="10" r="3"></circle></svg>
                       </div>
                       <span class="leading-tight pt-[1px]" v-html="resumeData.general.address"></span>
                    </div>
                </div>
            </div>

            <!-- DYNAMIC SECTIONS CHO SIDEBAR -->
            <template v-for="section in sidebarSections" :key="section.id">
                <div
                    v-show="section.isVisible"
                    class="section-block section-block-sidebar py-[2mm] px-[2mm] -mx-[2mm]"
                    :class="{ 'section-selected': selectedSectionId === section.id }"
                    :style="selectedSectionId === section.id ? { '--sel-color': resumeData.theme.primaryColor || '#8A3841' } : {}"
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

                    <div class="mb-4 paginated-item">
                        <h3 class="font-bold text-[16px] whitespace-nowrap" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }">
                            {{ section.title }}
                        </h3>
                    </div>
                    
                    <div class="space-y-4">
                         <!-- Học Vấn trong Sidebar (đặc biệt cho mẫu thiết kế này) -->
                         <div v-if="section.id === 'education'" class="space-y-4">
                             <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[12px] item-container paginated-item flex flex-col gap-1 pr-2 text-gray-900">
                                 <div class="flex justify-between items-baseline">
                                     <span class="font-bold text-[13px]" v-html="item.school"></span>
                                     <span class="text-[12px] whitespace-nowrap">{{ item.year }}</span>
                                 </div>
                                 <div class="font-normal" v-if="item.major" v-html="item.major"></div>
                                 <div class="font-normal" v-if="item.gradType" v-html="item.gradType"></div>
                                 <!-- Delete btn -->
                                 <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 scale-90">
                                     <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                 </button>
                             </div>
                         </div>

                         <!-- Kỹ năng, Sở thích -->
                         <div v-else-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills' || section.id === 'hobbies'" class="space-y-4">
                             <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[12px] text-gray-900 font-medium item-container paginated-item pr-4 leading-snug">
                                 <div class="font-bold text-[13px] mb-1 text-black" v-html="item.name"></div>
                                 <div class="text-[12px] text-justify font-normal" v-if="item.desc" v-html="formatDesc(item.desc)"></div>
                                 <div class="text-[12px] text-justify font-normal" v-else-if="item.level" v-html="item.level"></div>
                                 <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 scale-90">
                                     <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                 </button>
                             </div>
                         </div>
                         
                         <!-- Chứng chỉ, Giải thưởng -->
                         <div v-else-if="section.id === 'awards' || section.id === 'certifications'" class="space-y-3.5">
                            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[12px] item-container paginated-item flex flex-col gap-1 pr-2 text-gray-900">
                                <span class="font-bold whitespace-nowrap">{{ item.year }}</span>
                                <span class="leading-snug font-normal">{{ item.name }}</span>
                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 scale-90">
                                    <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                            </div>
                         </div>

                         <!-- Người tham chiếu -->
                         <div v-else-if="section.id === 'references'" class="space-y-4">
                             <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[12px] item-container paginated-item relative text-gray-900">
                                 <div class="sidebar-html-content leading-snug" v-html="formatDesc(item.info)"></div>
                                 <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 scale-90">
                                     <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                 </button>
                             </div>
                         </div>

                         <!-- Default -->
                         <div v-else class="space-y-3">
                             <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[12px] text-gray-900 whitespace-pre-line leading-relaxed paginated-item item-container">
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
    <main class="flex-1 pt-[14mm] pb-[10mm] px-[8mm] gap-0 flex flex-col relative z-10 box-border bg-white" @click.self="selectedSectionId = null">
        <template v-for="(section, idx) in mainSections" :key="section.id">
            <div
                v-show="section.isVisible"
                class="section-block section-block-main px-[4mm] mt-[1mm]"
                :class="{ 'section-selected bg-blue-50/50': selectedSectionId === section.id }"
                :style="selectedSectionId === section.id ? { '--sel-color': resumeData.theme.primaryColor || '#8A3841' } : {}"
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

                <div class="mb-5 paginated-item">
                    <h3 class="font-bold text-[17px] whitespace-nowrap" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }">
                        {{ section.title }}
                    </h3>
                </div>

                <div class="space-y-6">
                    <!-- Mục tiêu nghề nghiệp -->
                    <div v-if="section.id === 'summary'" class="text-[13px] leading-[1.7] text-gray-900 text-justify html-content paginated-item pr-1 font-normal" v-html="resumeData.general.summary || 'Chưa có thông tin mục tiêu nghề nghiệp.'"></div>

                    <!-- Kinh nghiệm / Hoạt động / Dự án -->
                    <div v-else-if="section.id === 'experience' || section.id === 'project' || section.id === 'activities'" class="space-y-5">
                        <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container paginated-item rounded-lg p-2 -mx-2 hover:bg-gray-50 border border-transparent hover:border-gray-100 transition-colors">
                            <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30">
                                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                            </button>

                            <div class="flex flex-col mb-1.5">
                                <div class="flex justify-between items-baseline gap-1 md:gap-0">
                                    <span class="font-bold text-[14px] text-black">{{ section.id === 'experience' ? item.role : (item.role || item.name) }}</span>
                                    <span class="text-[13px] text-black whitespace-nowrap text-right ml-2">{{ item.time }}</span>
                                </div>
                                <div class="font-medium text-[13px] text-gray-500 mt-0.5 italicx" v-if="(section.id === 'experience' ? item.company : (item.company || ''))">{{ section.id === 'experience' ? item.company : (item.company || '') }}</div>
                            </div>
                            <div class="text-[12.5px] leading-[1.65] text-gray-900 text-justify html-content font-normal ml-0" v-html="formatDesc(item.desc)"></div>
                        </div>
                    </div>

                    <!-- Học Vấn trong Main (Nếu User kéo thả sang) -->
                    <div v-else-if="section.id === 'education'" class="space-y-5">
                        <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container paginated-item rounded-lg p-2 -mx-2 hover:bg-gray-50 border border-transparent hover:border-gray-100 transition-colors">
                            <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30">
                                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                            </button>

                            <div class="flex flex-col mb-1.5">
                                <div class="flex justify-between items-baseline gap-1 md:gap-0">
                                    <span class="font-bold text-[14px] text-black">{{ item.school }}</span>
                                    <span class="text-[13px] font-medium text-black whitespace-nowrap">{{ item.year }}</span>
                                </div>
                                <div class="font-[600] text-[13px] text-gray-500 mt-0.5" v-if="item.major">{{ item.major }}</div>
                            </div>
                            <div class="text-[13px] text-gray-900 ml-0 font-normal">
                                <span v-if="item.gradType">Tốt nghiệp loại: <strong>{{ item.gradType }}</strong></span>
                                <span v-if="item.gradType && item.gpa"> | </span>
                                <span v-if="item.gpa">GPA: <strong>{{ item.gpa }}</strong></span>
                            </div>
                            <div v-if="item.desc" class="text-[13px] leading-[1.65] text-gray-900 text-justify font-normal html-content mt-1.5 ml-0" v-html="formatDesc(item.desc)"></div>
                        </div>
                    </div>
                    
                    <!-- Chứng Chỉ, Giải thưởng trong Main -->
                    <div v-else-if="section.id === 'awards' || section.id === 'certifications'" class="space-y-2 mt-4">
                        <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container paginated-item flex gap-4 p-2 -mx-2 hover:bg-gray-50 transition-colors rounded-lg">
                             <div class="font-bold text-[13px] text-black w-[50px] flex-shrink-0 mt-[1px]">{{ item.year }}</div>
                             <div class="flex-1 text-[13px] text-gray-900 leading-[1.6] font-normal" v-html="formatDesc(item.name || item.desc)"></div>
                             <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30">
                                 <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                             </button>
                        </div>
                    </div>

                    <!-- Default Main -->
                    <div v-else class="space-y-4">
                        <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container paginated-item text-[13px] leading-[1.65] text-gray-900 font-normal html-content rounded-lg p-2 -mx-2 hover:bg-gray-50 border border-transparent hover:border-gray-100 transition-colors">
                             <div class="ml-0" v-html="formatDesc(item.desc || item.name || item.info)"></div>
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
    top: 5px;
    right: 5px;
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
    background-color: rgba(0, 0, 0, 0.04);
    border-radius: 6px;
    padding-left: 4px;
    margin-left: -4px;
}
aside .delete-btn {
    top: -2px;
    right: -2px;
    border-color: #f4ebe8;
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
:deep(.html-content b), :deep(.html-content strong) { font-weight: 700; color: #111827; }
:deep(.html-content h4) { font-weight: 700; font-size: 14.5px; margin-bottom: 4px; color: #111827; }
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
    color: #111827;
    font-weight: 700;
}

/* SECTION BLOCKS */
.section-block {
    position: relative;
    border-radius: 6px;
    border: 2px solid transparent; 
    cursor: pointer;
    transition: border-color 0.2s ease, background 0.2s ease, box-shadow 0.2s ease;
}

.section-block-main {
    border-bottom: 1.5px solid #eaeaea;
    margin-bottom: 16px;
    padding-bottom: 18px;
    border-radius: 0;
}

.section-block-main:last-of-type {
    border-bottom: 1.5px solid transparent;
}

.section-block-main:hover {
    background: rgba(0, 0, 0, 0.01);
}

.section-block-sidebar:hover {
    background: rgba(0, 0, 0, 0.04);
}

.section-selected {
    border-color: var(--sel-color, #8A3841) !important;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.section-selected.section-block-main {
    background: color-mix(in srgb, var(--sel-color, #8A3841) 3%, white) !important;
    border-radius: 6px;
    border-bottom-color: transparent;
}

.section-selected.section-block-sidebar {
    background: rgba(0, 0, 0, 0.06);
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
    background: #8A3841;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    box-shadow: 0 2px 6px rgba(0,0,0, 0.2);
    transition: background 0.15s ease, transform 0.1s ease;
}

.nav-btn:hover {
    background: #6a2a31;
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
        margin-left: 0 !important;
        margin-right: 0 !important;
        border-radius: 0 !important;
    }
    .section-block-main {
        border-bottom: 1.5px solid #eaeaea !important;
    }
    .section-block-main:last-of-type {
        border-bottom: 1.5px solid transparent !important;
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
:global(.is-exporting-pdf .section-block-main) {
    border-bottom: 1.5px solid #eaeaea !important;
}
</style>
