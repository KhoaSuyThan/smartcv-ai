<template>
  <div id="cv-printable-area" ref="cvRoot" class="bg-white flex w-[210mm] relative box-border text-[13px] leading-relaxed overflow-hidden" :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif' }">
    
    <!-- CỘT TRÁI (SIDEBAR) -->
    <aside class="w-[72mm] flex-shrink-0 flex flex-col relative z-20 bg-white" @click.self="selectedSectionId = null">
       <div class="m-[3mm] flex-1 flex flex-col shadow-sm" :style="{ backgroundColor: templateSidebarBgColor }">
            <!-- HỌ TÊN - BOX MÀU -->
            <div class="mx-[3mm] mt-[8mm] mb-6 px-[4mm] py-[6mm] paginated-item text-center rounded-sm shadow-sm" :style="{ backgroundColor: templateAccentColor }">
                <h1 class="font-extrabold leading-tight mb-2 tracking-tight" :style="{ color: templatePrimaryColor, fontSize: '30px !important' }" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'Nguyễn Huyền Trang'"></h1>
                <h2 class="font-bold uppercase tracking-wider text-white" :style="{ fontSize: '22px !important' }" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'CHUYÊN VIÊN SALES ADMIN'"></h2>
            </div>

            <!-- AVATAR - HÌNH TRÒN CÓ VIỀN -->
            <div class="px-[8mm] pb-[10mm] paginated-item flex justify-center mt-2">
                <div class="w-[46mm] h-[46mm] rounded-full overflow-hidden bg-white relative z-10 box-border flex-shrink-0" :style="{ border: '4px solid ' + templatePrimaryColor }">
                    <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="rounded-full w-full h-full object-cover" />
                    <div v-else class="w-full h-full flex items-center justify-center text-gray-400">
                        <svg class="w-20 h-20" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
                    </div>
                </div>
            </div>

            <!-- CÁC SECTION TRONG SIDEBAR -->
            <div class="flex flex-col flex-1 pb-[8mm]">
                <!-- THÔNG TIN CÁ NHÂN (cố định) -->
                <!-- ĐÃ XÓA paginated-item Ở THẺ BỌC -->
                <!-- THÔNG TIN CÁ NHÂN -->
                <div 
                    class="section-block section-block-sidebar py-[4mm] px-[8mm] ml-[4mm] cursor-pointer hover:bg-black/5 transition-colors"
                    :class="{ 'section-active--sidebar': selectedSectionId === 'contact' }"
                    @click.stop="toggleSection('contact')"
                >
                    <!-- Nav Buttons for Contact Block - Removed eye button -->
                    <div class="sidebar-section-header mb-3 paginated-item">
                        <h3 class="font-bold uppercase tracking-wide" :style="{ color: templatePrimaryColor, fontSize: '15px !important' }">Thông tin cá nhân</h3>
                        <div class="w-full h-[1.2px] mt-1" :style="{ backgroundColor: templatePrimaryColor }"></div>
                    </div>
                    <div class="space-y-3 font-medium">
                        <div 
                            v-for="(ci, ciIdx) in contactItems" 
                            :key="ci.key" 
                            class="flex items-start gap-3 paginated-item relative group/item"
                        >
                           <div class="w-[14px] h-[14px] flex-shrink-0 mt-0.5" :style="{ color: templatePrimaryColor }">
                               <svg viewBox="0 0 24 24" fill="ci.fill || 'none'" :stroke="ci.stroke || 'currentColor'" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" v-html="ci.icon"></svg>
                           </div>
                           <span :style="{ fontSize: '12px !important', color: templatePrimaryColor }" class="break-all leading-snug flex-1" v-html="ci.value"></span>

                           <!-- Individual contact item buttons -->
                           <transition name="fade-btns">
                             <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print">
                               <button @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn nav-btn--xs" title="Lên"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                               <button @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn nav-btn--xs" title="Xuống"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                               <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn--xs nav-btn-danger" title="Ẩn"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg></button>
                             </div>
                           </transition>
                        </div>
                    </div>
                </div>

            <!-- DYNAMIC SECTIONS CHO SIDEBAR -->
            <draggable
              v-model="sidebarSectionsWritable"
              item-key="id"
              group="sections"
              class="flex flex-col cursor-move"
              @end="onDragEnd"
              animation="200"
              ghost-class="opacity-30"
              :delay="100"
              :delayOnTouchOnly="true"
              :fallbackTolerance="5"
              filter=".nav-btn, .delete-btn, .contact-item-btns, .html-content, input, .sidebar-html-content"
            >
              <template #item="{ element: section }">
                <div
                    v-show="section.isVisible"
                    :data-section-id="section.id" class="section-block section-block-sidebar py-[4mm] px-[8mm] mb-1 ml-[4mm] cursor-pointer hover:bg-black/5 transition-colors"
                    :class="{ 'section-active--sidebar': selectedSectionId === section.id }"
                    @click.stop="toggleSection(section.id)"
                >
                    <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
                        <button @click.stop.prevent="moveSectionUp(section.id, 'left')" class="nav-btn" title="Lên">
                            <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                        </button>
                        <button @click.stop.prevent="moveSectionDown(section.id, 'left')" class="nav-btn" title="Xuống">
                            <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                        </button>
                        <button @click.stop.prevent="moveSectionHorizontal(section.id, 'right')" class="nav-btn" title="Sang Phải">
                            <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
                        </button>
                        <button @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                            <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                        </button>
                    </div>

                    <div class="sidebar-section-header mb-3 paginated-item">
                        <h3 class="font-bold uppercase tracking-wide" :style="{ color: templatePrimaryColor, fontSize: '15px !important' }">
                            <span v-html="section.title"></span>
                        </h3>
                        <div class="w-full h-[1.2px] mt-1" :style="{ backgroundColor: templatePrimaryColor }"></div>
                    </div>
                    
                    <div class="space-y-4">
                         <div v-if="section.id.toLowerCase().includes('education')" class="space-y-3">
                             <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container flex flex-col gap-0.5 pr-2">
                      <button v-show="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 scale-90">
                                     <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                 </button>
                      <CustomSectionItem v-if="(typeof section !== 'undefined' && section && section.isCustom) || (typeof id !== 'undefined' && typeof sec !== 'undefined' && sec(id)?.value?.isCustom) || (typeof block !== 'undefined' && block && block.isCustom)" :item="item" />
                      <template v-else>
                        <div class="flex justify-between items-start gap-1 paginated-item">
                                     <span class="font-bold leading-snug" :style="{ fontSize: '13px !important', color: templatePrimaryColor }" v-html="item.school"></span>
                                     <span class="font-bold flex-shrink-0 mt-0.5" :style="{ fontSize: '11.5px !important', color: templatePrimaryColor }"><span v-html="item.year"></span></span>
                                 </div>
                                 <div class="font-bold leading-snug paginated-item" v-if="item.major" :style="{ fontSize: '12px !important', color: templatePrimaryColor }" v-html="item.major"></div>
                                 <div class="font-bold leading-snug paginated-item" v-if="item.gradType" :style="{ fontSize: '12px !important', color: templatePrimaryColor }" v-html="item.gradType"></div>
                      </template>
</div>
                         </div>

                         <div v-else-if="section.id.toLowerCase().includes('skill') || section.id.toLowerCase().includes('lang')" class="space-y-3.5">
                             <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-gray-800 item-container pr-3 leading-snug paginated-item">
                      <button v-show="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 scale-90">
                                     <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                 </button>
                      <CustomSectionItem v-if="(typeof section !== 'undefined' && section && section.isCustom) || (typeof id !== 'undefined' && typeof sec !== 'undefined' && sec(id)?.value?.isCustom) || (typeof block !== 'undefined' && block && block.isCustom)" :item="item" />
                      <template v-else>
                        <div class="font-bold text-gray-900 mb-0.5" style="font-size: 13px !important;" v-html="item.name"></div>
                                 <div class="font-normal text-gray-700 leading-[1.6]" v-if="item.desc" style="font-size: 11.5px !important;" v-html="formatDesc(item.desc)"></div>
                                 <div class="font-normal text-gray-700" v-else-if="item.level" style="font-size: 11.5px !important;"><span v-html="item.level"></span></div>
                      </template>
</div>
                         </div>
                         
                        <div v-else class="space-y-3">
                        <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-gray-800 font-medium leading-snug item-container pr-3" :style="{ fontSize: '12px !important', color: templatePrimaryColor }">
                      <button v-show="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30 scale-90">
                                    <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                      <CustomSectionItem v-if="(typeof section !== 'undefined' && section && section.isCustom) || (typeof id !== 'undefined' && typeof sec !== 'undefined' && sec(id)?.value?.isCustom) || (typeof block !== 'undefined' && block && block.isCustom)" :item="item" />
                      <template v-else>
                        <!-- Áp dụng formatDesc để băm nhỏ từng dòng, giúp thuật toán nhận diện và ngắt trang -->
                            <div v-if="item.year || item.time" class="font-bold opacity-70 mb-0.5 flex flex-col" style="font-size: 11px !important;" v-html="formatDesc(item.year || item.time)"></div>
                            
                            <div v-if="item.name || item.title" class="font-bold mb-0.5 flex flex-col" style="font-size: 12px !important;" v-html="formatDesc(item.name || item.title)"></div>
                            
                                <div v-if="item.role || item.position" class="opacity-80 italic mb-0.5 flex flex-col" style="font-size: 11px !important;" v-html="formatDesc(item.role || item.position)"></div>
                            
                            <div v-if="item.desc" class="sidebar-html-content flex flex-col" v-html="formatDesc(item.desc)"></div>
                            <div v-else-if="!item.name && !item.title" class="sidebar-html-content flex flex-col" v-html="formatDesc(item.info || '')"></div>
                      </template>
</div>
                        </div>
                    </div>
                </div>
            </template>
            </draggable>
        </div>
       </div>
    </aside>

    <!-- CỘT PHẢI (MAIN CONTENT) -->
    <main class="flex-1 pt-[15mm] pb-[10mm] px-[10mm] flex flex-col gap-0 relative z-10 bg-white" @click.self="selectedSectionId = null">
        <draggable
          v-model="mainSectionsWritable"
          item-key="id"
          group="sections"
          class="flex flex-col gap-0 cursor-move"
          @end="onDragEnd"
          animation="200"
          ghost-class="opacity-30"
          :delay="100"
          :delayOnTouchOnly="true"
          :fallbackTolerance="5"
          filter=".nav-btn, .delete-btn, .contact-item-btns, .html-content, input"
        >
          <template #item="{ element: section }">
            <div
                v-show="section.isVisible"
                :data-section-id="section.id" class="section-block section-block-main cursor-pointer hover:bg-black/5 transition-colors"
                :class="{ 'section-active--main': selectedSectionId === section.id }"
                :style="[
                    (section.id.toLowerCase().includes('summary') || section.id.toLowerCase().includes('objective')) ? { marginTop: '10mm !important' } : {}
                ]"
                @click.stop="toggleSection(section.id)"
            >
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
                    <button @click.stop.prevent="moveSectionUp(section.id, 'right')" class="nav-btn" title="Lên">
                        <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                    </button>
                    <button @click.stop.prevent="moveSectionDown(section.id, 'right')" class="nav-btn" title="Xuống">
                        <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                    </button>
                    <button @click.stop.prevent="moveSectionHorizontal(section.id, 'left')" class="nav-btn" title="Sang Trái">
                        <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
                    </button>
                    <button @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                        <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                </div>

                <div class="main-section-header mb-4 paginated-item">
                    <h3 class="font-bold uppercase tracking-wide" :style="{ color: templatePrimaryColor, fontSize: '16px !important' }">
                        <span v-html="section.title"></span>
                    </h3>
                    <div class="w-full h-[1.2px] mt-1" :style="{ backgroundColor: templatePrimaryColor, opacity: 0.5 }"></div>
                </div>

                <div class="space-y-5">
                    <div v-if="section.id.toLowerCase().includes('summary')" class="text-gray-800 text-justify html-content font-medium flex flex-col" style="font-size: 12.5px !important;" v-html="formatDesc(resumeData.general.summary || 'Chưa có thông tin mục tiêu nghề nghiệp.')"></div>

                    <div v-else-if="section.id.toLowerCase().includes('experience') || section.id.toLowerCase().includes('project') || section.id.toLowerCase().includes('activit')" class="space-y-5">
                        <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container">
                      <CustomSectionItem v-if="(typeof section !== 'undefined' && section && section.isCustom) || (typeof id !== 'undefined' && typeof sec !== 'undefined' && sec(id)?.value?.isCustom) || (typeof block !== 'undefined' && block && block.isCustom)" :item="item" />
                      <template v-else>
                        <div class="flex justify-between items-baseline gap-2 mb-0.5 paginated-item">
                                <span class="font-bold text-gray-900 leading-tight uppercase" style="font-size: 13.5px !important;">
                                <span v-html="section.id.toLowerCase().includes('experience') ? (item.role || item.company) : (item.name || item.role)"></span>
                                </span>
                                <span class="font-bold text-gray-800 whitespace-nowrap flex-shrink-0" style="font-size: 12.5px !important;"><span v-html="item.time"></span></span>
                            </div>
                            <div class="text-gray-600 font-bold mb-1.5 paginated-item"
                            v-if="section.id.toLowerCase().includes('experience') ? item.company : item.role"
                            style="font-size: 13px !important;">
                                <span v-html="section.id.toLowerCase().includes('experience') ? item.company : item.role"></span>
                            </div>
                            <div class="leading-[1.65] text-gray-800 text-justify html-content font-medium flex flex-col" style="font-size: 12.5px !important;" v-html="formatDesc(item.desc)"></div>
                      </template>
</div>
                    </div>

                    <div v-else-if="section.id.toLowerCase().includes('education')" class="space-y-4">
                        <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container">
                      <button v-show="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30">
                                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                            </button>
                      <CustomSectionItem v-if="(typeof section !== 'undefined' && section && section.isCustom) || (typeof id !== 'undefined' && typeof sec !== 'undefined' && sec(id)?.value?.isCustom) || (typeof block !== 'undefined' && block && block.isCustom)" :item="item" />
                      <template v-else>
                        <div class="flex justify-between items-baseline gap-2 mb-0.5 paginated-item">
                                <span class="font-bold text-gray-900" style="font-size: 14px !important;"><span v-html="item.school"></span></span>
                                <span class="font-bold text-gray-700 whitespace-nowrap flex-shrink-0" style="font-size: 12.5px !important;"><span v-html="item.year"></span></span>
                            </div>
                            <div class="font-bold text-gray-800 mb-1 paginated-item" v-if="item.major" style="font-size: 12.5px !important;"><span v-html="item.major"></span></div>
                            <div class="text-gray-700 font-medium paginated-item" style="font-size: 12.5px !important;">
                                <span v-if="item.gradType"><span v-html="item.gradType"></span></span>
                            </div>
                            <div v-if="item.desc" class="leading-[1.65] text-gray-800 text-justify font-medium html-content mt-1 flex flex-col" style="font-size: 12.5px !important;" v-html="formatDesc(item.desc)"></div>
                      </template>
</div>
                    </div>

                    <div v-else-if="section.id.toLowerCase().includes('award') || section.id.toLowerCase().includes('cert')" class="space-y-3">
                        <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container flex gap-6 items-baseline paginated-item">
                      <button v-show="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30">
                                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                            </button>
                      <CustomSectionItem v-if="(typeof section !== 'undefined' && section && section.isCustom) || (typeof id !== 'undefined' && typeof sec !== 'undefined' && sec(id)?.value?.isCustom) || (typeof block !== 'undefined' && block && block.isCustom)" :item="item" />
                      <template v-else>
                        <div class="font-bold text-gray-900 w-[42px] flex-shrink-0" style="font-size: 12.5px !important;"><span v-html="item.year"></span></div>
                            <div class="flex-1 text-gray-800 font-medium leading-[1.6]" style="display: flex; flex-direction: column; font-size: 12.5px !important;" v-html="formatDesc(item.name || item.desc)"></div>
                      </template>
</div>
                    </div>

                  <div v-else class="space-y-4">
                
                    <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container text-gray-800 font-medium flex flex-col paginated-item" style="font-size: 12.5px !important;">
                      <button v-show="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30">
                            <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                        </button>
                      <CustomSectionItem v-if="(typeof section !== 'undefined' && section && section.isCustom) || (typeof id !== 'undefined' && typeof sec !== 'undefined' && sec(id)?.value?.isCustom) || (typeof block !== 'undefined' && block && block.isCustom)" :item="item" />
                      <template v-else>
                        <div class="flex justify-between items-baseline gap-2 w-full">
                            <!-- Đã bọc hàm formatDesc để băm nhỏ nội dung nếu người dùng xuống dòng -->
                            <span class="font-bold flex flex-col flex-1" v-html="formatDesc(item.name || item.title || '')"></span>
                            <span v-if="item.level || item.info" class="font-normal opacity-80 flex-shrink-0" style="font-size: 11.5px !important;"><span v-html="item.level || item.info"></span></span>
                        </div>
                        <div v-if="item.desc" class="html-content flex flex-col mt-0.5" v-html="formatDesc(item.desc)"></div>
                      </template>
</div>
                </div>
                </div>
            </div>
        </template>
        </draggable>
    </main>

    <!-- ĐƯỜNG PHÂN TRANG -->
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
import draggable from 'vuedraggable'

const props = defineProps({
    resumeData: { type: Object, required: true }
})

const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

const hexToRgb = (hex) => {
  const clean = hex.replace('#', '')
  const num = parseInt(clean, 16)
  return {
    r: (num >> 16) & 255,
    g: (num >> 8) & 255,
    b: num & 255
  }
}

const rgbToHex = (r, g, b) => {
  const clamp = (val) => Math.max(0, Math.min(255, Math.round(val)))
  return '#' + ((1 << 24) + (clamp(r) << 16) + (clamp(g) << 8) + clamp(b)).toString(16).slice(1)
}

const adjustColorBrightness = (hex, percent) => {
  try {
    const { r, g, b } = hexToRgb(hex)
    if (percent < 0) {
      const factor = 1 + percent
      return rgbToHex(r * factor, g * factor, b * factor)
    } else {
      return rgbToHex(
        r + (255 - r) * percent,
        g + (255 - g) * percent,
        b + (255 - b) * percent
      )
    }
  } catch (e) {
    return hex
  }
}

const templatePrimaryColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  if (!c || c.toLowerCase() === '#2b5c8f') return '#6a2a31'
  return c
})

const isCustomColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  return c && c.toLowerCase() !== '#2b5c8f'
})

const templateAccentColor = computed(() => {
  if (isCustomColor.value) return adjustColorBrightness(templatePrimaryColor.value, 0.40)
  return '#c48c8c'
})

const templateSidebarBgColor = computed(() => {
  if (isCustomColor.value) return adjustColorBrightness(templatePrimaryColor.value, 0.88)
  return '#f8e8e8'
})

const templateNavBgColor = computed(() => {
  if (isCustomColor.value) return templatePrimaryColor.value
  return '#8A3841'
})

// ─── CONTACT ITEMS LOGIC ───
const contactIcons = {
  birthDate: { icon: '<rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect><line x1="16" y1="2" x2="16" y2="6"></line><line x1="8" y1="2" x2="8" y2="6"></line><line x1="3" y1="10" x2="21" y2="10"></line>', stroke: 'currentColor', fill: 'none' },
  gender: { icon: '<path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path><circle cx="12" cy="7" r="4"></circle>', stroke: 'currentColor', fill: 'none' },
  phone: { icon: '<path d="M6.62 10.79a15.091 15.091 0 006.59 6.59l2.2-2.2a1 1 0 011.11-.21c1.12.37 2.33.57 3.58.57a1 1 0 011 1v3.5a1 1 0 01-1 1A16 16 0 013 4a1 1 0 011-1h3.5a1 1 0 011 1c0 1.25.2 2.46.57 3.58a1 1 0 01-.21 1.11l-2.24 2.1z" />', fill: 'currentColor', stroke: 'none' },
  email: { icon: '<path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path><polyline points="22,6 12,13 2,6"></polyline>', stroke: 'currentColor', fill: 'none' },
  facebook: { icon: '<path d="M18 2h-3a5 5 0 0 0-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 0 1 1-1h3z"></path>', fill: 'currentColor', stroke: 'none' },
  address: { icon: '<path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path><circle cx="12" cy="10" r="3"></circle>', stroke: 'currentColor', fill: 'none' }
}

const contactOrder = ref(['birthDate', 'gender', 'phone', 'email', 'facebook', 'address'])
const hiddenContacts = ref([])

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  switch (key) {
    case 'birthDate': return g.birthDate
    case 'gender': return g.gender
    case 'phone': return g.phone
    case 'email': return g.email
    case 'facebook': return g.facebook || g.website
    case 'address': return g.address
    default: return ''
  }
}

const contactItems = computed(() => {
  return contactOrder.value
    .filter(key => !hiddenContacts.value.includes(key) && !isEmpty(getContactValue(key)))
    .map(key => ({
      key,
      ...contactIcons[key],
      value: getContactValue(key)
    }))
})

const moveContactUp = (idx) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && !isEmpty(getContactValue(k)))
  if (idx <= 0) return
  const keyA = visible[idx]
  const keyB = visible[idx - 1]
  const idxA = contactOrder.value.indexOf(keyA)
  const idxB = contactOrder.value.indexOf(keyB)
  const arr = [...contactOrder.value]
  ;[arr[idxA], arr[idxB]] = [arr[idxB], arr[idxA]]
  contactOrder.value = arr
  requestPagination()
}

const moveContactDown = (idx) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && !isEmpty(getContactValue(k)))
  if (idx >= visible.length - 1) return
  const keyA = visible[idx]
  const keyB = visible[idx + 1]
  const idxA = contactOrder.value.indexOf(keyA)
  const idxB = contactOrder.value.indexOf(keyB)
  const arr = [...contactOrder.value]
  ;[arr[idxA], arr[idxB]] = [arr[idxB], arr[idxA]]
  contactOrder.value = arr
  requestPagination()
}

const removeContactItem = (idx) => {
  const visible = contactItems.value
  if (idx >= 0 && idx < visible.length) {
    const key = visible[idx].key
    if (props.resumeData.general[key] !== undefined) {
      props.resumeData.general[key] = ''
    } else if (key === 'facebook') {
      props.resumeData.general.facebook = ''
      props.resumeData.general.website = ''
    }
    hiddenContacts.value.push(key)
    requestPagination()
  }
}

const cvRoot = ref(null);
const pageCount = ref(1);
const selectedSectionId = ref(null);

// --- UTILS ---
const isEmpty = (val) => {
    if (!val) return true;
    if (typeof val !== 'string') return false;
    const cleanText = val.replace(/<[^>]*>/g, '').trim();
    return cleanText === '';
};

// --- CHÉM DÒNG TỚI TẬN CÙNG (LEAF-BLOCK PARSING) ---
const formatDesc = (text) => {
    if (!text) return '';
    
    // Nếu không có HTML, biến từng dòng thành một khối SPAN phân trang riêng biệt
    if (!/<[a-z][\s\S]*>/i.test(text)) {
        return text.split('\n')
                   .map(l => l.trim())
                   .filter(Boolean)
                   .map(l => `<span class="paginated-item" style="display: block; width: 100%;">${l}</span>`)
                   .join('');
    }

    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = text;
    
    // Bảo tồn các mã màu của Editor
    const lis = tempDiv.querySelectorAll('li');
    lis.forEach(li => {
        const child = li.firstElementChild;
        if (child && (child.tagName === 'FONT' || child.tagName === 'SPAN')) {
            if (child.color) li.style.color = child.color;
            if (child.style && child.style.color) li.style.color = child.style.color;
        }
    });

    // Thọc sâu vào cấu trúc HTML để chém nhỏ mọi thứ
    const wrapTextNodes = (element) => {
        Array.from(element.childNodes).forEach(node => {
            if (node.nodeType === Node.TEXT_NODE) {
                if (node.textContent.trim()) {
                   // Bọc chữ thô vào thẻ span có class phân trang
                   const wrapper = document.createElement('span');
                   wrapper.className = 'paginated-item';
                   wrapper.style.display = 'block';
                   wrapper.style.width = '100%';
                   node.replaceWith(wrapper);
                   wrapper.appendChild(node);
                }
            } else if (node.nodeType === Node.ELEMENT_NODE) {
                node.classList.remove('paginated-item'); // Xóa ở thẻ cha để tránh double margin

                if (node.tagName === 'BR') {
                   // Ép thẻ BR thành một khối block để nhận margin phân trang
                   node.outerHTML = '<span class="paginated-item" style="display: block; width: 100%; height: 6px;"></span>';
                } else if (node.tagName === 'LI') {
                   // Thẻ LI đã là một khối tốt, giữ nguyên
                   node.classList.add('paginated-item');
                } else {
                   // Đệ quy sâu vào các thẻ P, DIV, SPAN...
                   wrapTextNodes(node);
                }
            }
        });
    };

    wrapTextNodes(tempDiv);
    return tempDiv.innerHTML;
};

const toggleSection = (id) => {
    selectedSectionId.value = selectedSectionId.value === id ? null : id;
    requestPagination();
};

const swapInArray = (arr, i, j) => {
    if (i < 0 || j < 0 || i >= arr.length || j >= arr.length) return;
    const tmp = arr[i]; arr.splice(i, 1, arr[j]); arr.splice(j, 1, tmp);
};

const moveSectionUp = (sectionId, col) => {
    const colSecs  = props.resumeData.sections.filter(s => s.column === col);
    const localIdx = colSecs.findIndex(s => s.id === sectionId);
    if (localIdx <= 0) return;
    const secs = props.resumeData.sections;
    swapInArray(secs, secs.findIndex(s => s.id === colSecs[localIdx].id), secs.findIndex(s => s.id === colSecs[localIdx - 1].id));
    requestPagination();
};

const moveSectionDown = (sectionId, col) => {
    const colSecs  = props.resumeData.sections.filter(s => s.column === col);
    const localIdx = colSecs.findIndex(s => s.id === sectionId);
    if (localIdx < 0 || localIdx >= colSecs.length - 1) return;
    const secs = props.resumeData.sections;
    swapInArray(secs, secs.findIndex(s => s.id === colSecs[localIdx].id), secs.findIndex(s => s.id === colSecs[localIdx + 1].id));
    requestPagination();
};

const moveSectionHorizontal = (sectionId, direction) => {
    const section = props.resumeData.sections.find(s => s.id === sectionId);
    if (!section) return;
    section.column = direction === 'right' ? 'right' : 'left';
    selectedSectionId.value = null;
    requestPagination();
};

// --- PAGINATION ENGINE HOÀN HẢO ---
const A4_W_MM = 210;
const A4_H_MM = 297;

let paginateTimer = null;
const requestPagination = () => {
    if (paginateTimer) clearTimeout(paginateTimer);
    paginateTimer = setTimeout(doPagination, 50);
};

const doPagination = async () => {
    if (!cvRoot.value) return;
    
    // Thuật toán màng lọc: Chỉ lấy LỚP NGOÀI CÙNG (Leaf Nodes) để tránh nhân đôi Margin
    const allPaginated = Array.from(cvRoot.value.querySelectorAll('.paginated-item')).filter(el => {
        if (el.offsetHeight === 0) return false;
        let parent = el.parentElement;
        while (parent && parent !== cvRoot.value) {
            if (parent.classList.contains('paginated-item')) return false;
            parent = parent.parentElement;
        }
        return true;
    });

    allPaginated.forEach(el => { el.style.setProperty('margin-top', '0px', 'important'); });
    await nextTick();

    const cvRect = cvRoot.value.getBoundingClientRect();
    const pxPerMm = cvRect.width / A4_W_MM;
    const pageH = A4_H_MM * pxPerMm;
    
    const bottomSafeZone = 12 * pxPerMm;
    const topMargin = 18 * pxPerMm;
    let stable = false;
    let passes = 0;

    // Vòng lặp rà soát từng dòng văn bản
    while (!stable && passes < 30) {
        stable = true;
        passes++;
        const currentCvRect = cvRoot.value.getBoundingClientRect();

        for (let i = 0; i < allPaginated.length; i++) {
            const el = allPaginated[i];
            const elRect = el.getBoundingClientRect();
            const top = elRect.top - currentCvRect.top;
            const height = elRect.height;
            const bottom = top + height;

            const pageIndex = Math.floor(top / pageH);
            const topInPage = top - (pageIndex * pageH);
            const bottomInPage = topInPage + height;

            // Khối quá to thì bỏ qua (tránh lặp vô tận)
            if (height > (pageH - bottomSafeZone - topMargin)) continue;

            // Đáy đụng vạch đen -> Lập tức ép dòng xuống trang
            if (bottomInPage > (pageH - bottomSafeZone)) {
                const distToNextPage = pageH - topInPage + topMargin;
                const currentMt = parseFloat(el.style.marginTop || '0');
                el.style.setProperty('margin-top', `${currentMt + distToNextPage}px`, 'important');
                stable = false;
                break;
            }
        }
    }

    const finalCvRect = cvRoot.value.getBoundingClientRect();
    let maxBottom = 0;
    allPaginated.forEach(el => {
        const rect = el.getBoundingClientRect();
        const bottom = rect.bottom - finalCvRect.top;
        if (bottom > maxBottom) maxBottom = bottom;
    });

    pageCount.value = Math.max(1, Math.ceil(maxBottom / pageH));
};

watch(() => props.resumeData, () => requestPagination(), { deep: true });

onMounted(() => {
    if (props.resumeData && props.resumeData.sections) {
        const SIDEBAR_IDS = ['education', 'skills', 'reference'];
        const MAIN_IDS = ['summary', 'objective', 'experience', 'award', 'cert', 'activit', 'project'];
        const FORBIDDEN_IDS = ['it_skills', 'technical_skills', 'languages', 'lang'];

        props.resumeData.sections.forEach(sec => {
            const id = sec.id.toLowerCase();
            if (SIDEBAR_IDS.some(activeId => id.includes(activeId)) && !FORBIDDEN_IDS.some(forbiddenId => id.includes(forbiddenId))) {
                if (sec.isVisible === undefined) sec.isVisible = true;
                if (!sec.column) sec.column = 'left';
            } else if (MAIN_IDS.some(activeId => id.includes(activeId))) {
                if (sec.isVisible === undefined) sec.isVisible = true;
                if (!sec.column) sec.column = 'right';
            } else {
                if (sec.isVisible === undefined) sec.isVisible = false;
                if (!sec.column) sec.column = 'right'; 
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

// --- SECTIONS ---
const sidebarSections = computed(() => {
    return (props.resumeData?.sections || []).filter(s => s.column === 'left');
});

const mainSections = computed(() => {
    return (props.resumeData?.sections || []).filter(s => s.column === 'right');
});

const sidebarSectionsWritable = ref([])
const mainSectionsWritable = ref([])

watch(sidebarSections, (newVal) => {
  sidebarSectionsWritable.value = [...newVal]
}, { immediate: true, deep: true })

watch(mainSections, (newVal) => {
  mainSectionsWritable.value = [...newVal]
}, { immediate: true, deep: true })

const onDragEnd = () => {
  sidebarSectionsWritable.value.forEach(s => {
    const item = props.resumeData.sections.find(x => x.id === s.id)
    if (item) item.column = 'left'
  })
  mainSectionsWritable.value.forEach(s => {
    const item = props.resumeData.sections.find(x => x.id === s.id)
    if (item) item.column = 'right'
  })

  const newOrderIds = [
    ...sidebarSectionsWritable.value.map(s => s.id),
    ...mainSectionsWritable.value.map(s => s.id)
  ]
  
  const newSections = []
  props.resumeData.sections.forEach(s => {
    if (!newOrderIds.includes(s.id)) {
      newSections.push(s)
    }
  })
  
  newOrderIds.forEach(id => {
    const item = props.resumeData.sections.find(s => s.id === id)
    if (item) newSections.push(item)
  })

  props.resumeData.sections.splice(0, props.resumeData.sections.length, ...newSections)
  requestPagination()
}

const sidebarIds = computed(() => sidebarSections.value.map(s => s.id));
const mainIds = computed(() => mainSections.value.map(s => s.id));
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&display=swap');

#cv-printable-area {
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
    overflow-wrap: anywhere;
}

/* -- HTML CONTENT STYLING (Main) -- */
:deep(.html-content), :deep(.html-content *) {
    font-size: 12.5px !important;
    line-height: 1.6 !important;
    font-weight: 800 !important;
    color: v-bind(templatePrimaryColor) !important;
}
:deep(.html-content ul) {
    list-style-type: disc !important;
    padding-left: 1.25rem !important;
    margin-top: 0.2rem;
    margin-bottom: 0.2rem;
}
:deep(.html-content li) {
    margin-bottom: 0.25rem;
}
:deep(.html-content b), :deep(.html-content strong) { font-weight: 900 !important; color: #4a1a1f !important; }
:deep(.html-content h4) { font-weight: 900 !important; font-size: 13.5px !important; margin-bottom: 4px; color: #4a1a1f !important; }

/* Sidebar styling overrides */
:deep(.sidebar-html-content), :deep(.sidebar-html-content *) {
    font-size: 12px !important;
    line-height: 1.5 !important;
    font-weight: 800 !important;
    color: v-bind(templatePrimaryColor) !important;
}
:deep(.sidebar-html-content b), :deep(.sidebar-html-content strong) {
    color: #4a1a1f !important;
    font-weight: 900 !important;
}

:deep(span), :deep(div), :deep(p), :deep(h1), :deep(h2), :deep(h3), :deep(h4), :deep(li) {
    font-weight: 800 !important;
}

.text-gray-800, .text-gray-700, .text-gray-600, .text-gray-900 {
    color: v-bind(templatePrimaryColor) !important;
}

h3 {
    color: v-bind(templatePrimaryColor) !important;
    font-weight: 800 !important;
}

/* KHÓA MỌI ANIMATION LÀM SAI LỆCH VỊ TRÍ */
.paginated-item {
    transition: none !important;
}

/* -- ITEM CONTAINERS -- */
.item-container {
    position: relative;
}

.delete-btn {
    position: absolute;
    right: -2px;
    top: -2px;
    width: 22px;
    height: 22px;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    background: #ef4444 !important;
    color: white !important;
    border: none !important;
    box-shadow: 0 2px 6px rgba(0,0,0,0.3);
    z-index: 30;
    transition: transform 0.12s ease;
}

.delete-btn:hover {
    transform: scale(1.15) !important;
}
.delete-btn:active {
    transform: scale(0.9) !important;
}

/* -- SECTION BLOCKS XÓA SCALE -- */
.section-block {
    position: relative;
    border-radius: 0 !important;
    border: 2px solid transparent !important; 
    cursor: pointer;
    transition: box-shadow 0.2s ease, border-color 0.15s ease, background 0.15s ease;
}

.section-block-main {
    padding: 4px 15px 16px 10px;
    margin-right: 35px;
    margin-bottom: 14px;
}

.section-active--main {
    border: 2px solid #ffffff !important;
    border-radius: 6px !important;
    box-shadow: 0 4px 18px rgba(106, 42, 49, 0.15), 0 1px 4px rgba(106, 42, 49, 0.08) !important;
    background: rgba(106, 42, 49, 0.02) !important;
    z-index: 10 !important;
}

.section-block-sidebar {
    padding: 4px 8px;
}

.section-active--sidebar {
    border: 2px solid v-bind(templateSidebarBgColor) !important;
    border-radius: 6px !important;
    box-shadow: 0 6px 20px rgba(0, 0, 0, 0.28), 0 1px 4px rgba(0, 0, 0, 0.1) !important;
    background: rgba(255, 255, 255, 0.06) !important;
    z-index: 10 !important;
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
    background: v-bind(templateNavBgColor) !important;
    color: white !important;
    border: none;
    border-radius: 4px !important;
    cursor: pointer;
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.15);
    transition: background 0.15s ease, transform 0.1s ease;
}

.nav-btn:hover {
    filter: brightness(0.85) !important;
}

.nav-btn:active {
    transform: scale(0.92) !important;
}

.nav-btn--xs {
  padding: 2px !important;
  border-radius: 3px !important;
}

.nav-btn-danger {
  background: #ef4444 !important;
  box-shadow: 0 2px 6px rgba(239, 68, 68, 0.4) !important;
}
.nav-btn-danger:hover {
  background: #dc2626 !important;
}

.contact-item-btns {
  position: absolute;
  right: 0;
  top: 50%;
  transform: translateY(-50%);
  display: flex;
  gap: 3px;
  z-index: 50;
  background: v-bind(templateSidebarBgColor);
  padding-left: 5px;
}

/* XÓA BỎ LỆNH ĐỔI PADDING KHI IN (ĐỂ PDF GIỐNG Y HỆT WEB) */
@media print {
    .no-print {
        display: none !important;
    }
    .section-block, .section-active--main, .section-active--sidebar {
        cursor: default !important;
        box-shadow: none !important;
        background: transparent !important;
        border-color: transparent !important;
        transform: none !important;
        border-radius: 0 !important;
        outline: none !important;
    }
}

:global(.is-exporting-pdf .no-print) {
    display: none !important;
}
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .section-active--main),
:global(.is-exporting-pdf .section-active--sidebar) {
    cursor: default !important;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
    transform: none !important;
    border-radius: 0 !important;
    outline: none !important;
}
</style>