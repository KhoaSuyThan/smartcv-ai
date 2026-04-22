<template>
  <div id="cv-printable-area" ref="cvRoot" class="bg-white flex w-[210mm] relative box-border text-[13px] leading-relaxed" :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif' }">
    
    <!-- CỘT TRÁI (SIDEBAR) -->
    <aside class="w-[72mm] flex-shrink-0 flex flex-col relative z-20" style="background-color: #f6ede4;" @click.self="selectedSectionId = null">
       
        <!-- HỌ TÊN -->
        <div class="px-[8mm] pt-[12mm] pb-[3mm] paginated-item text-center">
            <h1 class="text-[26px] font-extrabold leading-tight mb-1" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'Nguyễn Huyền Trang'"></h1>
            <h2 class="text-[12px] font-semibold uppercase tracking-wide text-gray-700 mt-1" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'CHUYÊN VIÊN SALES ADMIN'"></h2>
        </div>

        <!-- AVATAR - HÌNH TRÒN -->
        <div class="px-[8mm] pb-[6mm] paginated-item flex justify-center">
            <div class="w-[48mm] h-[48mm] rounded-full overflow-hidden bg-white relative z-10 box-border flex-shrink-0" :style="{ border: `3px solid ${resumeData.theme.primaryColor || '#8A3841'}` }">
                <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
                <div v-else class="w-full h-full flex items-center justify-center text-gray-400">
                    <svg class="w-20 h-20" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
                </div>
            </div>
        </div>

        <!-- CÁC SECTION TRONG SIDEBAR -->
        <div class="flex flex-col flex-1 pb-[8mm]">
            <!-- THÔNG TIN CÁ NHÂN (cố định) -->
            <div class="paginated-item section-block-sidebar py-[3mm] px-[8mm]">
                <div class="sidebar-section-header mb-3">
                    <h3 class="font-bold text-[15px]" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }">Thông tin cá nhân</h3>
                    <div class="w-full h-[1.5px] mt-1" :style="{ backgroundColor: resumeData.theme.primaryColor || '#8A3841' }"></div>
                </div>
                <div class="space-y-2.5 text-[12.5px] font-medium text-gray-800">
                    <div class="flex items-start gap-2.5" v-if="!isEmpty(resumeData.general.birthDate)">
                       <div class="w-[14px] h-[14px] flex-shrink-0 mt-[2px]" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }">
                           <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect><line x1="16" y1="2" x2="16" y2="6"></line><line x1="8" y1="2" x2="8" y2="6"></line><line x1="3" y1="10" x2="21" y2="10"></line></svg>
                       </div>
                       <span class="leading-tight pt-[1px]" v-html="resumeData.general.birthDate"></span>
                    </div>
                    <div class="flex items-start gap-2.5" v-if="!isEmpty(resumeData.general.gender)">
                       <div class="w-[14px] h-[14px] flex-shrink-0 mt-[2px]" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }">
                           <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path><circle cx="12" cy="7" r="4"></circle></svg>
                       </div>
                       <span class="leading-tight pt-[1px]" v-html="resumeData.general.gender"></span>
                    </div>
                    <div class="flex items-start gap-2.5" v-if="!isEmpty(resumeData.general.phone)">
                       <div class="w-[14px] h-[14px] flex-shrink-0 mt-[2px]" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }">
                           <svg viewBox="0 0 24 24" fill="currentColor"><path d="M6.62 10.79a15.091 15.091 0 006.59 6.59l2.2-2.2a1 1 0 011.11-.21c1.12.37 2.33.57 3.58.57a1 1 0 011 1v3.5a1 1 0 01-1 1A16 16 0 013 4a1 1 0 011-1h3.5a1 1 0 011 1c0 1.25.2 2.46.57 3.58a1 1 0 01-.21 1.11l-2.24 2.1z" /></svg>
                       </div>
                       <span class="break-all leading-tight pt-[1px]" v-html="resumeData.general.phone"></span>
                    </div>
                    <div class="flex items-start gap-2.5" v-if="!isEmpty(resumeData.general.email)">
                       <div class="w-[14px] h-[14px] flex-shrink-0 mt-[2px]" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }">
                           <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path><polyline points="22,6 12,13 2,6"></polyline></svg>
                       </div>
                       <span class="break-all leading-tight pt-[1px]" v-html="resumeData.general.email"></span>
                    </div>
                    <div class="flex items-start gap-2.5" v-if="!isEmpty(resumeData.general.website)">
                       <div class="w-[14px] h-[14px] flex-shrink-0 mt-[2px]" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }">
                           <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="2" y1="12" x2="22" y2="12"></line><path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"></path></svg>
                       </div>
                       <span class="break-all leading-tight pt-[1px]" v-html="resumeData.general.website"></span>
                    </div>
                    <div class="flex items-start gap-2.5" v-if="!isEmpty(resumeData.general.address)">
                       <div class="w-[14px] h-[14px] flex-shrink-0 mt-[2px]" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }">
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
                    class="section-block section-block-sidebar py-[3mm] px-[8mm]"
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

                    <!-- Section header với underline -->
                    <div class="sidebar-section-header mb-3 paginated-item">
                        <h3 class="font-bold text-[15px]" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }">
                            {{ section.title }}
                        </h3>
                        <div class="w-full h-[1.5px] mt-1" :style="{ backgroundColor: resumeData.theme.primaryColor || '#8A3841' }"></div>
                    </div>
                    
                    <div class="space-y-4">
                         <!-- Học Vấn trong Sidebar -->
                         <div v-if="section.id === 'education'" class="space-y-4">
                             <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[12.5px] item-container paginated-item flex flex-col gap-1 pr-2 text-gray-800">
                                 <div class="flex justify-between items-baseline gap-1">
                                     <span class="font-bold text-[13px] text-gray-900" v-html="item.school"></span>
                                     <span class="text-[12px] whitespace-nowrap text-gray-700 flex-shrink-0">{{ item.year }}</span>
                                 </div>
                                 <div class="font-normal text-gray-800" v-if="item.major" v-html="item.major"></div>
                                 <div class="font-normal text-gray-700 italic" v-if="item.gradType" v-html="item.gradType"></div>
                                 <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 scale-90">
                                     <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                 </button>
                             </div>
                         </div>

                         <!-- Kỹ năng: Tên bold + mô tả phía dưới (không progress bar, giống ảnh) -->
                         <div v-else-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'" class="space-y-3.5">
                             <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[12.5px] text-gray-800 item-container paginated-item pr-3 leading-snug">
                                 <div class="font-bold text-[13px] text-gray-900 mb-0.5" v-html="item.name"></div>
                                 <div class="text-[12px] font-normal text-gray-700 leading-[1.5]" v-if="item.desc" v-html="formatDesc(item.desc)"></div>
                                 <div class="text-[12px] font-normal text-gray-700" v-else-if="item.level">{{ item.level }}</div>
                                 <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 scale-90">
                                     <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                 </button>
                             </div>
                         </div>
                         
                         <!-- Sở thích -->
                         <div v-else-if="section.id === 'hobbies'" class="space-y-2">
                             <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[12.5px] text-gray-800 font-medium item-container paginated-item pr-3">
                                 <span v-html="item.name"></span>
                                 <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 scale-90">
                                     <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                 </button>
                             </div>
                         </div>
                          
                         <!-- Chứng chỉ, Giải thưởng trong Sidebar -->
                         <div v-else-if="section.id === 'awards' || section.id === 'certifications'" class="space-y-3">
                            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[12.5px] item-container paginated-item flex gap-3 pr-2 text-gray-800">
                                <span class="font-bold text-gray-900 whitespace-nowrap flex-shrink-0">{{ item.year }}</span>
                                <span class="leading-snug font-normal">{{ item.name }}</span>
                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 scale-90">
                                    <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                            </div>
                         </div>

                         <!-- Người tham chiếu -->
                         <div v-else-if="section.id === 'references'" class="space-y-4">
                             <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[12.5px] item-container paginated-item relative text-gray-800">
                                 <div class="sidebar-html-content leading-snug" v-html="formatDesc(item.info)"></div>
                                 <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 scale-90">
                                     <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                 </button>
                             </div>
                         </div>

                         <!-- Default -->
                         <div v-else class="space-y-3">
                             <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[12.5px] text-gray-800 font-medium leading-snug paginated-item item-container pr-3">
                                 <span class="sidebar-html-content" v-html="formatDesc(item.desc || item.name || item.info)"></span>
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
    <main class="flex-1 pt-[12mm] pb-[10mm] px-[8mm] flex flex-col gap-0 relative z-10 bg-white" @click.self="selectedSectionId = null">
        <template v-for="section in mainSections" :key="section.id">
            <div
                v-show="section.isVisible"
                class="section-block section-block-main"
                :class="{ 'section-selected': selectedSectionId === section.id }"
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

                <!-- Tiêu đề section + gạch ngang màu theme -->
                <div class="main-section-header mb-4 paginated-item">
                    <h3 class="font-bold text-[17px]" :style="{ color: resumeData.theme.primaryColor || '#8A3841' }">
                        {{ section.title }}
                    </h3>
                    <div class="w-full h-[1.5px] mt-1" :style="{ backgroundColor: resumeData.theme.primaryColor || '#8A3841', opacity: 0.4 }"></div>
                </div>

                <div class="space-y-5">
                    <!-- Mục tiêu nghề nghiệp -->
                    <div v-if="section.id === 'summary'" class="text-[13px] leading-[1.75] text-gray-800 text-justify html-content paginated-item font-normal" v-html="resumeData.general.summary || 'Chưa có thông tin mục tiêu nghề nghiệp.'"></div>

                    <!-- Kinh nghiệm / Hoạt động / Dự án -->
                    <div v-else-if="section.id === 'experience' || section.id === 'project' || section.id === 'activities'" class="space-y-5">
                        <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container paginated-item">
                            <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30">
                                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                            </button>

                            <!-- Role + Time trên cùng hàng, bold -->
                            <div class="flex justify-between items-baseline gap-2 mb-0.5">
                                <span class="font-bold text-[14px] text-gray-900 leading-tight">{{ section.id === 'experience' ? item.role : (item.role || item.name) }}</span>
                                <span class="text-[13px] font-semibold text-gray-800 whitespace-nowrap flex-shrink-0">{{ item.time }}</span>
                            </div>
                            <!-- Company name, italic gray nhẹ -->
                            <div class="text-[13px] text-gray-500 italic mb-1.5" v-if="section.id === 'experience' ? item.company : (item.company || '')">
                                {{ section.id === 'experience' ? item.company : (item.company || '') }}
                            </div>
                            <!-- Mô tả với bullet points -->
                            <div class="text-[13px] leading-[1.65] text-gray-800 text-justify html-content font-normal" v-html="formatDesc(item.desc)"></div>
                        </div>
                    </div>

                    <!-- Học Vấn (nếu ở Main) -->
                    <div v-else-if="section.id === 'education'" class="space-y-4">
                        <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container paginated-item">
                            <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30">
                                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                            </button>
                            <div class="flex justify-between items-baseline gap-2 mb-0.5">
                                <span class="font-bold text-[14px] text-gray-900">{{ item.school }}</span>
                                <span class="text-[13px] font-semibold text-gray-800 whitespace-nowrap flex-shrink-0">{{ item.year }}</span>
                            </div>
                            <div class="font-medium text-[13px] text-gray-600 mb-1" v-if="item.major">{{ item.major }}</div>
                            <div class="text-[13px] text-gray-700 font-normal">
                                <span v-if="item.gradType">{{ item.gradType }}</span>
                            </div>
                            <div v-if="item.desc" class="text-[13px] leading-[1.65] text-gray-800 text-justify font-normal html-content mt-1" v-html="formatDesc(item.desc)"></div>
                        </div>
                    </div>

                    <!-- Danh hiệu / Giải thưởng (Main) - năm + tên inline -->
                    <div v-else-if="section.id === 'awards'" class="space-y-2.5">
                        <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container paginated-item flex gap-4 items-baseline">
                            <div class="font-bold text-[13px] text-gray-900 w-[42px] flex-shrink-0">{{ item.year }}</div>
                            <div class="flex-1 text-[13px] text-gray-800 font-normal leading-[1.6]" v-html="formatDesc(item.name || item.desc)"></div>
                            <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30">
                                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                            </button>
                        </div>
                    </div>

                    <!-- Chứng chỉ (Main) - năm + tên inline -->
                    <div v-else-if="section.id === 'certifications'" class="space-y-2.5">
                        <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container paginated-item flex gap-4 items-baseline">
                            <div class="font-bold text-[13px] text-gray-900 w-[42px] flex-shrink-0">{{ item.year }}</div>
                            <div class="flex-1 text-[13px] text-gray-800 font-normal leading-[1.6]" v-html="formatDesc(item.name || item.desc)"></div>
                            <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30">
                                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                            </button>
                        </div>
                    </div>

                    <!-- Default Main -->
                    <div v-else class="space-y-4">
                        <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container paginated-item text-[13px] leading-[1.65] text-gray-800 font-normal html-content">
                             <div v-html="formatDesc(item.desc || item.name || item.info)"></div>
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

// --- PAGINATION ENGINE (Optimized & Stable) ---
let paginateTimer = null;
const requestPagination = () => {
    if (paginateTimer) clearTimeout(paginateTimer);
    paginateTimer = setTimeout(doPagination, 50);
};

const doPagination = async () => {
    if (!cvRoot.value) return;
    
    const allPaginated = cvRoot.value.querySelectorAll('.paginated-item');
    allPaginated.forEach(el => { el.style.marginTop = ''; });

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
                const height = item.offsetHeight;
                const bottom = top + height;
                
                const currentPageIndex = Math.floor(top / pageHeightPx);
                const currentSafeBottom = (currentPageIndex * pageHeightPx) + safeBottomPx;
                
                if (bottom > currentSafeBottom && top < (currentPageIndex + 1) * pageHeightPx) {
                    if (height > (pageHeightPx - marginTopPx - marginBottomPx)) {
                        continue;
                    }
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

    processColumn('aside');
    processColumn('main');

    let maxBottom = 0;
    allPaginated.forEach(el => {
        const top = getRelativeTop(el);
        const bottom = top + el.offsetHeight;
        if (bottom > maxBottom) maxBottom = bottom;
    });

    const calculatedPageCount = Math.max(1, Math.ceil((maxBottom + marginBottomPx - 2) / pageHeightPx));
    if (pageCount.value !== calculatedPageCount) {
        pageCount.value = calculatedPageCount;
    }
};

watch(() => props.resumeData, () => requestPagination(), { deep: true });

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

// --- NAVIGATION & INTERACTION ---
const hoveredSectionId = ref(null);
let _hideTimer = null;

const showNav = (id) => {
    if (_hideTimer) { clearTimeout(_hideTimer); _hideTimer = null; }
    hoveredSectionId.value = id;
};

const hideNav = () => {
    _hideTimer = setTimeout(() => {
        hoveredSectionId.value = null;
        _hideTimer = null;
    }, 120);
};

const selectedSectionId = ref(null);

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
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&display=swap');

#cv-printable-area {
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
    overflow-wrap: anywhere;
}

/* -- HTML CONTENT STYLING (Main) -- */
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
:deep(.html-content li) {
    margin-bottom: 0.25rem;
}
:deep(.html-content b), :deep(.html-content strong) { font-weight: 700; color: #111827; }
:deep(.html-content h4) { font-weight: 700; font-size: 14px; margin-bottom: 4px; color: #111827; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic; }
:deep(.html-content u) { text-decoration: underline; }

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
:deep(.sidebar-html-content b), :deep(.sidebar-html-content strong) {
    color: #111827;
    font-weight: 700;
}
:deep(.sidebar-html-content ul) {
    list-style-type: disc !important;
    padding-left: 1rem !important;
    margin-top: 0.15rem;
    margin-bottom: 0.15rem;
}

/* -- ITEM CONTAINERS -- */
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
    pointer-events: none;
}

.item-container:hover .delete-btn {
    opacity: 1;
    pointer-events: auto;
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
    border-color: #f6ede4;
}

main .item-container:hover {
    background-color: rgba(0, 0, 0, 0.01);
}

/* -- SECTION BLOCKS -- */
.section-block {
    position: relative;
    border-radius: 6px;
    border: 2px solid transparent; 
    cursor: pointer;
    transition: border-color 0.2s ease, background 0.2s ease, box-shadow 0.2s ease;
}

/* Main: padding + dải phân cách dưới */
.section-block-main {
    padding: 4px 4px 16px 4px;
    margin-bottom: 14px;
    border-radius: 0;
}

.section-block-main:hover {
    background: rgba(0, 0, 0, 0.01);
}

.section-block-sidebar:hover {
    background: rgba(0, 0, 0, 0.04);
    border-radius: 6px;
}

.section-selected {
    border-color: var(--sel-color, #8A3841) !important;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.06);
    border-radius: 6px !important;
}

.section-selected.section-block-main {
    background: color-mix(in srgb, var(--sel-color, #8A3841) 3%, white) !important;
}

.section-selected.section-block-sidebar {
    background: rgba(0, 0, 0, 0.04) !important;
}

/* -- NAV BUTTONS -- */
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

/* -- PRINT / EXPORT -- */
@media print {
    .no-print {
        display: none !important;
    }
    .section-block, .section-selected {
        cursor: default;
        box-shadow: none !important;
        background: transparent !important;
        border-color: transparent !important;
        border-radius: 0 !important;
    }
    .section-block-main {
        padding: 0 0 14px 0 !important;
        margin-bottom: 14px !important;
    }
    aside .section-block {
        padding-left: 0 !important;
        padding-right: 0 !important;
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
    border-radius: 0 !important;
}
:global(.is-exporting-pdf .section-block-main) {
    padding: 0 0 14px 0 !important;
    margin-bottom: 14px !important;
}
</style>
