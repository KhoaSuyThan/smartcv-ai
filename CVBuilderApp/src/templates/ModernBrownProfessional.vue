<template>
  <div id="cv-printable-area" ref="cvRoot" class="flex flex-row relative box-border bg-white overflow-hidden text-[#333]" :style="{ width: '210mm', height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Segoe UI\', Tahoma, Geneva, Verdana, sans-serif', lineHeight: '1.3' }">
    
    <aside class="z-10 flex flex-col shrink-0 relative box-border" :style="{ width: '35%', backgroundColor: templateSecondaryColor, padding: '30px 20px', gap: '20px' }">
      
      <!-- ĐÃ SỬA: Đổi mb-[-5px] thành mb-[10px] để nhích avatar lên, không bị đè vạch -->
      <div class="paginated-item relative z-20 w-full flex flex-col items-center mb-[10px]">
        <div class="relative rounded-full overflow-hidden mx-auto bg-white" :style="{ width: '150px', height: '150px', border: '5px solid ' + avatarBorderColor, boxShadow: '0 4px 10px rgba(0,0,0,0.05)' }">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="rounded-full w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center bg-gray-100 text-gray-400">
            <svg class="w-12 h-12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
          </div>
        </div>
      </div>

      <div 
        class="section-block mb-[20px] ml-[4mm] cursor-pointer hover:bg-black/5 transition-colors"
        :class="{ 'section-active--sidebar': selectedSectionId === 'contact' }"
        @click.stop="toggleSection('contact')"
        :style="{ borderTop: '1px solid ' + activeBorderColorSidebar, borderBottom: '1px solid ' + activeBorderColorSidebar, padding: '12px 0' }"
      >
        <!-- Nav Buttons for Contact Block - Removed eye button -->
        <ul class="w-full list-none p-0 m-0 text-[#333] flex flex-col gap-[10px]" :style="{ fontSize: '12px !important' }">
          <li 
            v-for="(ci, ciIdx) in contactItems" 
            :key="ci.key" 
            class="flex items-center gap-[10px] relative group/item"
          >
            <div class="w-[24px] h-[24px] bg-white rounded-full flex items-center justify-center shrink-0 shadow-sm" :style="{ color: templatePrimaryColor }">
              <svg class="w-[11px] h-[11px] inline-block" fill="currentColor" viewBox="0 0 20 20" v-html="ci.icon"></svg>
            </div>
            <span class="break-all leading-tight flex-1" v-html="ci.value"></span>

            <!-- Individual contact item buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print">
                <button v-if="ciIdx > 0" @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn nav-btn--xs" title="Lên"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button v-if="ciIdx < contactItems.length - 1" @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn nav-btn--xs" title="Xuống"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn--xs nav-btn-danger" title="Xóa"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button>
              </div>
            </transition>
          </li>
        </ul>
      </div>

      <div class="w-full flex-1 flex flex-col m-0 p-0">
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
          filter=".nav-btn, .delete-btn, .contact-item-btns, .html-content, input"
        >
          <template #item="{ element: section }">
          <div
            v-show="section.isVisible"
            :data-section-id="section.id" class="section-block relative w-full mb-[20px] cursor-pointer hover:bg-black/5 transition-colors"
            :class="{ 'section-active--sidebar': selectedSectionId === section.id }"
            @click.stop="toggleSection(section.id)"
          >
            <div v-show="selectedSectionId === section.id" class="nav-btns no-print" style="right: 5px;">
              <button @click.stop.prevent="moveSectionUp(section.id, 'left')" class="nav-btn" title="Di chuyển lên">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionDown(section.id, 'left')" class="nav-btn" title="Di chuyển xuống">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionHorizontal(section.id, 'right')" class="nav-btn" title="Sang Phải">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M9 5l7 7-7 7"/></svg>
              </button>
              <button @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <h3 class="w-full block paginated-item" :style="{ fontSize: '16px !important', color: '#333', borderBottom: '1px solid ' + activeBorderColorSidebar, paddingBottom: '5px !important', margin: '0 0 10px 0 !important', fontWeight: 'bold !important' }">
              <span v-html="section.title"></span>
            </h3>

            <div class="w-full flex flex-col gap-[10px]">
              <div
                v-for="(item, itemIndex) in section.items"
                :key="item._refId"
                class="item-container relative w-full text-[#444]"
                :style="{ fontSize: '12px !important', lineHeight: '1.5', margin: '0 !important', padding: '0 !important' }"
              > 
                <div v-if="section.id === 'skills'" class="flex items-start gap-[8px] paginated-item w-full">
                  <svg class="mt-[4px] shrink-0 opacity-70" :style="{ color: templatePrimaryColor }" width="10" height="10" viewBox="0 0 24 24" fill="currentColor"><path d="M12 2L14.5 9.5L22 12L14.5 14.5L12 22L9.5 14.5L2 12L9.5 9.5L12 2Z"/></svg>
                  <div class="w-full flex justify-between items-start gap-2">
                  <span class="font-medium text-[#333] break-words"><span v-html="item.name"></span></span>
                  <span v-if="item.level || item.info" class="font-bold shrink-0 whitespace-nowrap text-right opacity-90 text-[11px]"><span v-html="item.level || item.info"></span></span>
                </div>
              </div>
                
                <div v-else-if="section.id === 'education' || section.id === 'experience'" class="w-full">
                  <div class="w-full flex justify-between items-start mb-[2px] paginated-item">
                    <span class="font-bold text-[#333] leading-tight flex-1" v-html="section.id === 'education' ? item.school : (item.company || item.name)"></span>
                    <span v-if="item.year || item.time" class="shrink-0 text-[10px] text-white px-2 py-0.5 rounded-full ml-2 font-bold" :style="{ backgroundColor: badgeBgColor }"><span v-html="item.year || item.time"></span></span>
                  </div>
                  <div v-if="item.major || item.role" class="font-bold text-[#444] mb-[2px] paginated-item"><span v-html="item.major || item.role"></span></div>
                  <div v-if="item.desc" class="html-content text-justify whitespace-pre-line text-[#555] text-[11px] flex flex-col" v-html="formatDesc(item.desc)"></div>
                </div>
                
                <div v-else-if="section.id === 'languages' || section.id === 'it_skills'" class="flex flex-col paginated-item">
                  <div class="font-bold text-[#333] w-full break-words" :style="{ margin: '0 !important' }"><span v-html="item.name"></span></div>
                  <div v-if="item.info || item.level" class="w-full break-words mt-[2px]" :style="{ margin: '0 !important' }"><span v-html="item.info || item.level"></span></div>
                </div>
                
                <div v-else class="w-full flex flex-col">
  <div class="paginated-item w-full flex justify-between items-start gap-2 relative">
    <span class="font-bold flex-1 break-words leading-tight text-[#333] flex flex-col" v-html="formatDesc(item.name || item.title || item.company || item.organization)"></span>
    <!-- Hiển thị Năm với thiết kế bo tròn đồng bộ -->
    <span v-if="item.year || item.time || item.date" class="shrink-0 text-[10px] text-white px-2 py-0.5 rounded-full ml-2 font-bold" :style="{ backgroundColor: badgeBgColor }"><span v-html="item.year || item.time || item.date"></span></span>
  </div>
  <div v-if="item.major || item.role || item.info" class="italic opacity-90 mt-[2px] text-[12px] paginated-item flex flex-col text-[#444]" v-html="formatDesc(item.major || item.role || item.info)"></div>
  <div v-if="item.desc || item.details" class="html-content text-justify whitespace-pre-line break-words w-full mt-[2px] flex flex-col text-[#555]" :style="{ lineHeight: '1.5' }" v-html="formatDesc(item.desc || item.details)"></div>
</div>

                <!-- Nút xóa: chỉ hiện khi section đang active -->
                <button
                 v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print"
                >
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>
        </draggable>
      </div>
    </aside>

    <main class="flex-1 flex flex-col relative bg-white z-20 overflow-hidden box-border" @click.self="selectedSectionId = null">

      <header class="paginated-item w-full flex flex-col relative" :style="{ backgroundColor: templatePrimaryColor, color: 'white', padding: '40px 35px' }">
        <h1 class="text-white break-words w-full" :style="{ margin: '0 !important', padding: '0 !important', fontSize: '36px !important', textTransform: 'capitalize', fontWeight: '800 !important', letterSpacing: '1px', lineHeight: '1.1' }" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'Họ Và Tên'"></h1>
        <h2 class="text-white break-words w-full uppercase" :style="{ margin: '15px 0 15px 0 !important', padding: '0 0 10px 0 !important', fontSize: '20px !important', fontWeight: 'bold !important', borderBottom: '1px solid rgba(255,255,255,0.3)' }" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'VỊ TRÍ ỨNG TUYỂN'"></h2>
        <div
          v-if="summarySection && summarySection.isVisible"
          class="relative w-full"
          :class="{ 'section-active--header': selectedSectionId === summarySection.id }"
          @click.stop="selectedSectionId = selectedSectionId === summarySection.id ? null : summarySection.id"
        >
          <div class="html-content text-justify whitespace-pre-line w-full text-white flex flex-col" :style="{ fontSize: '12.5px !important', lineHeight: '1.6 !important', opacity: '0.9', margin: '0 !important', padding: '0 !important' }" v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Tôi là một ứng viên năng động, có tinh thần trách nhiệm cao và mong muốn đóng góp giá trị cho công ty...')"></div>
          <!-- Nav Buttons for Summary - Added X button -->
          <div v-show="selectedSectionId === summarySection.id" class="nav-btns no-print" style="top: -20px; right: 0;">
            <button @click.stop.prevent="summarySection.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn-danger" title="Ẩn mục này">
              <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
            </button>
          </div>
        </div>
      </header>

      <div class="w-full flex flex-col flex-1 box-border m-0" :style="{ padding: '30px 35px' }">
        <draggable
          v-model="mainSectionsWritable"
          item-key="id"
          group="sections"
          class="flex flex-col cursor-move"
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
            :data-section-id="section.id" class="section-block relative w-full mb-[20px] cursor-pointer hover:bg-black/5 transition-colors"
            :class="{ 'section-active--main': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <!-- Nav: chỉ hiện khi click -->
            <div v-show="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="moveSectionUp(section.id, 'right')" class="nav-btn" title="Di chuyển lên">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionDown(section.id, 'right')" class="nav-btn" title="Di chuyển xuống">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionHorizontal(section.id, 'left')" class="nav-btn" title="Sang Trái">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M15 19l-7-7 7-7"/></svg>
              </button>
              <button @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <h3 class="uppercase w-full block paginated-item" :style="{ fontSize: '18px !important', color: '#333', borderBottom: `2px solid ${templatePrimaryColor}`, paddingBottom: '5px !important', margin: '0 0 15px 0 !important', fontWeight: 'bold !important' }">
              <span v-html="section.title"></span>
            </h3>

            <div class="w-full flex flex-col gap-[15px]">
              <div
                v-for="(item, itemIndex) in section.items"
                :key="item._refId"
                class="item-container relative w-full text-[#333]"
                :style="{ margin: '0 !important', padding: '0 !important' }"
              >
                <div v-if="['education','experience','project','activities'].includes(section.id)" class="w-full" :style="{ padding: '0 !important' }">
                  <div class="w-full flex justify-between items-start mb-[5px] paginated-item">
                    <div class="flex-1 pr-3">
                      <span class="font-bold text-[#333] leading-tight block" :style="{ fontSize: '14px', margin: '0 !important' }" v-html="section.id === 'education' ? item.school : (item.company || item.name)"></span>
                      <span class="italic text-[#555] block mt-[2px] leading-tight" :style="{ fontSize: '13px', margin: '0 !important' }" v-if="item.major || item.role"><span v-html="item.major || item.role"></span></span>
                    </div>
                    <span v-if="item.year || item.time" class="shrink-0 text-center whitespace-nowrap" :style="{ backgroundColor: badgeBgColor, color: 'white', padding: '3px 12px', borderRadius: '12px', fontSize: '11px', fontWeight: 'bold' }"><span v-html="item.year || item.time"></span></span>
                  </div>
                  <div v-if="item.desc" class="html-content text-justify whitespace-pre-line break-words w-full text-[#444] flex flex-col" :style="{ fontSize: '12.5px', lineHeight: '1.5', margin: '5px 0 0 0 !important', padding: '0 !important' }" v-html="formatDesc(item.desc)"></div>
                  <div v-else-if="item.gradType" class="font-medium mt-[5px] text-[#444] paginated-item" :style="{ fontSize: '12.5px !important', margin: '0 !important' }">Trạng thái: <span v-html="item.gradType"></span></div>
                </div>
                
                <div v-else class="w-full" :style="{ padding: '0 !important' }">
              <div class="paginated-item w-full flex justify-between items-start gap-2">
                <span class="font-bold text-[#333] leading-tight flex-1" v-html="formatDesc(item.name || item.title || item.company || item.organization)"></span>
                <!-- ĐÃ BỔ SUNG item.level VÀO ĐÂY ĐỂ HIỂN THỊ MỨC ĐỘ GÓC PHẢI -->
                <span v-if="item.year || item.time || item.date || item.level" class="font-bold shrink-0 text-[12px]" :style="{ color: templatePrimaryColor }"><span v-html="item.year || item.time || item.date || item.level"></span></span>
              </div>
               <div v-if="item.info || item.role || item.major" class="paginated-item text-[14px] font-medium text-[#555] mt-[2px] flex flex-col" v-html="formatDesc(item.info || item.role || item.major)"></div>
               <div v-if="item.desc || item.details" class="html-content text-justify whitespace-pre-line break-words w-full text-[#444] flex flex-col mt-[2px]" :style="{ fontSize: '12.5px', lineHeight: '1.5', margin: '0 !important', padding: '0 !important' }" v-html="formatDesc(item.desc || item.details)"></div>
              </div>

                <!-- Nút xóa: chỉ hiện khi section đang active -->
                <button
                  v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print"
                >
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>
        </draggable>
      </div>
    </main>

    <!-- Footer border per page -->
    <template v-for="p in pageCount" :key="'footer-border-'+p">
      <div class="absolute left-0 w-full flex items-center z-40 pointer-events-none"
           :style="{ top: `calc(${p * 297}mm - 12mm)`, height: '1.5px', paddingLeft: '20px', paddingRight: '20px' }">
        <div class="w-full h-full opacity-20" :style="{ backgroundImage: 'linear-gradient(to right, transparent, ' + templatePrimaryColor + ', transparent)' }"></div>
      </div>
    </template>

    <!-- Page break markers -->
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
import draggable from 'vuedraggable'

// ─── Refs ──────────────────────────────────────────────────────────────────
const cvRoot            = ref(null)
const pageCount         = ref(1)
const selectedSectionId = ref(null)

// ─── Props / Emits ─────────────────────────────────────────────────────────
const props = defineProps({ resumeData: { type: Object, required: true } })
const emit  = defineEmits(['removeItem', 'moveUp', 'moveDown', 'moveHorizontal'])

// ─── CONTACT ITEMS LOGIC ───
const contactIcons = {
  phone: '<path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z"></path>',
  email: '<path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z"></path><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z"></path>',
  dob: '<path fill-rule="evenodd" d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" clip-rule="evenodd"></path>',
  address: '<path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd"></path>',
  website: '<path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM4.332 8.027a6.012 6.012 0 011.912-2.706C6.512 5.73 6.974 6 7.5 6A1.5 1.5 0 019 7.5V8a2 2 0 004 0 2 2 0 011.523-1.943A5.977 5.977 0 0116 10c0 .34-.028.675-.083 1H15a2 2 0 00-2 2v2.197A5.973 5.973 0 0110 16v-2a2 2 0 00-2-2 2 2 0 01-2-2 2 2 0 00-1.668-1.973z" clip-rule="evenodd"></path>'
}

const contactOrder = ref(['phone', 'email', 'dob', 'address', 'website'])
const hiddenContacts = ref([])

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  switch (key) {
    case 'phone': return g.phone
    case 'email': return g.email
    case 'dob': return g.dob || g.birthDate
    case 'address': return g.address
    case 'website': return g.website || g.github || g.linkedin
    default: return ''
  }
}

const contactItems = computed(() => {
  return contactOrder.value
    .filter(key => !hiddenContacts.value.includes(key) && !isEmpty(getContactValue(key)))
    .map(key => ({
      key,
      icon: contactIcons[key],
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
    } else if (key === 'dob') {
       if (props.resumeData.general.dob !== undefined) props.resumeData.general.dob = ''
       if (props.resumeData.general.birthDate !== undefined) props.resumeData.general.birthDate = ''
    } else if (key === 'website') {
       props.resumeData.general.website = ''
       props.resumeData.general.github = ''
       props.resumeData.general.linkedin = ''
    }
    hiddenContacts.value.push(key)
    requestPagination()
  }
}

const toggleSection = (id) => {
  selectedSectionId.value = selectedSectionId.value === id ? null : id
  requestPagination()
}

// ─── Helpers ───────────────────────────────────────────────────────────────
const isEmpty = (val) => {
  if (!val) return true
  if (typeof val !== 'string') return false
  return val.replace(/<[^>]*>/g, '').trim() === ''
}

// THUẬT TOÁN "BĂM LÁ" XUYÊN SÂU TRÌNH SOẠN THẢO HTML
const formatDesc = (text) => {
  if (!text) return ''
  
  if (!/<[a-z][\s\S]*>/i.test(text)) {
    return text.split('\n')
               .map(l => l.trim())
               .filter(Boolean)
               .map(l => `<span class="paginated-item" style="display: block; width: 100%;">${l}</span>`)
               .join('')
  }
  
  const tempDiv = document.createElement('div')
  tempDiv.innerHTML = text
  
  tempDiv.querySelectorAll('li').forEach(li => {
    const child = li.firstElementChild
    if (child && (child.tagName === 'FONT' || child.tagName === 'SPAN')) {
      if (child.color) li.style.color = child.color
      if (child.style?.color) li.style.color = child.style.color
    }
  })

  // Đệ quy bọc các text nodes bằng thẻ span để thuật toán JS tóm gọn từng dòng
  const wrapTextNodes = (element) => {
    Array.from(element.childNodes).forEach(node => {
      if (node.nodeType === Node.TEXT_NODE) {
        if (node.textContent.trim()) {
           const wrapper = document.createElement('span')
           wrapper.className = 'paginated-item'
           wrapper.style.display = 'block'
           wrapper.style.width = '100%'
           node.replaceWith(wrapper)
           wrapper.appendChild(node)
        }
      } else if (node.nodeType === Node.ELEMENT_NODE) {
        node.classList.remove('paginated-item')
        if (node.tagName === 'BR') {
           node.outerHTML = '<span class="paginated-item" style="display: block; width: 100%; height: 6px;"></span>'
        } else if (node.tagName === 'LI') {
           node.classList.add('paginated-item')
        } else {
           wrapTextNodes(node)
        }
      }
    })
  }

  wrapTextNodes(tempDiv)
  return tempDiv.innerHTML
}

// ─── Theme colors ──────────────────────────────────────────────────────────
const hexToRgb = (hex) => {
  hex = hex.replace(/^#/, '')
  if (hex.length === 3) {
    hex = hex.split('').map(c => c + c).join('')
  }
  const num = parseInt(hex, 16)
  return {
    r: (num >> 16) & 255,
    g: (num >> 8) & 255,
    b: num & 255
  }
}

const rgbToHex = (r, g, b) => {
  return '#' + [r, g, b].map(x => {
    const hex = Math.max(0, Math.min(255, Math.round(x))).toString(16)
    return hex.length === 1 ? '0' + hex : hex
  }).join('')
}

const adjustBrightness = (hex, percent) => {
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
  if (!c || c.toLowerCase() === '#0d6efd' || c.toLowerCase() === '#2b5c8f') return '#634c46'
  return c
})

const templateSecondaryColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  if (!c || c.toLowerCase() === '#0d6efd' || c.toLowerCase() === '#2b5c8f') return '#e5ddd5'
  return adjustBrightness(c, 0.88)
})

const badgeBgColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  if (!c || c.toLowerCase() === '#0d6efd' || c.toLowerCase() === '#2b5c8f') return '#9b8a7e'
  return adjustBrightness(c, 0.35)
})

const avatarBorderColor = computed(() => adjustBrightness(templateSecondaryColor.value, -0.07))
const activeBorderColorSidebar = computed(() => adjustBrightness(templateSecondaryColor.value, -0.12))
const activeBorderColorMain = computed(() => adjustBrightness(templateSecondaryColor.value, 0.05))

const primaryRgb = computed(() => {
  try {
    const { r, g, b } = hexToRgb(templatePrimaryColor.value)
    return `${r},${g},${b}`
  } catch (e) {
    return '99,76,70'
  }
})

const secondaryRgb = computed(() => {
  try {
    const { r, g, b } = hexToRgb(templateSecondaryColor.value)
    return `${r},${g},${b}`
  } catch (e) {
    return '229,221,213'
  }
})

// ─── Section move helpers (splice = reactive-safe) ─────────────────────────
const swapSections = (sections, idxA, idxB) => {
  const a = sections[idxA]
  const b = sections[idxB]
  sections.splice(idxA, 1, b)
  sections.splice(idxB, 1, a)
}

const moveSectionUp = (sectionId, column) => {
  const col = column === 'left' ? 'left' : 'right'
  const colSections = props.resumeData.sections.filter(s => s.column === col && s.id !== 'summary')
  const idx = colSections.findIndex(s => s.id === sectionId)
  if (idx <= 0) return
  const sections = props.resumeData.sections
  const idxA = sections.findIndex(s => s.id === colSections[idx].id)
  const idxB = sections.findIndex(s => s.id === colSections[idx - 1].id)
  if (idxA < 0 || idxB < 0) return
  swapSections(sections, idxA, idxB)
  requestPagination()
}

const moveSectionDown = (sectionId, column) => {
  const col = column === 'left' ? 'left' : 'right'
  const colSections = props.resumeData.sections.filter(s => s.column === col && s.id !== 'summary')
  const idx = colSections.findIndex(s => s.id === sectionId)
  if (idx < 0 || idx >= colSections.length - 1) return
  const sections = props.resumeData.sections
  const idxA = sections.findIndex(s => s.id === colSections[idx].id)
  const idxB = sections.findIndex(s => s.id === colSections[idx + 1].id)
  if (idxA < 0 || idxB < 0) return
  swapSections(sections, idxA, idxB)
  requestPagination()
}

const moveSectionHorizontal = (sectionId, direction) => {
  const section = props.resumeData.sections.find(s => s.id === sectionId)
  if (!section) return
  section.column = direction === 'right' ? 'right' : 'left'
  requestPagination()
}

// ─── THUẬT TOÁN PHÂN TRANG (TÍNH TOÁN THEO KÍCH THƯỚC VẬT LÝ, KHÁNG ZOOM) ─
const A4_W_MM   = 210
const A4_H_MM   = 297

let paginateTimer = null
const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 50)
}

const doPagination = async () => {
  if (!cvRoot.value) return

  // Tạm vô hiệu hóa hiệu ứng Scale CSS để tránh sai lệch tọa độ khi người dùng chọn khối
  const activeElements = cvRoot.value.querySelectorAll('.section-active--main, .section-active--sidebar, .section-active--header')
  activeElements.forEach(el => el.style.setProperty('transform', 'none', 'important'))

  // BỘ LỌC CHỈ CHỌN LỚP NGOÀI CÙNG
  const allElements = Array.from(cvRoot.value.querySelectorAll('.paginated-item')).filter(el => {
    if (el.offsetHeight === 0) return false;
    let parent = el.parentElement;
    while (parent && parent !== cvRoot.value) {
      if (parent.classList.contains('paginated-item')) return false;
      parent = parent.parentElement;
    }
    return true;
  });

  allElements.forEach(el => { el.style.setProperty('margin-top', '0px', 'important') })
  await nextTick()

  // THUẬT TOÁN KHÁNG ZOOM: TÍNH THEO OFFSET_WIDTH THAY VÌ GET_BOUNDING_CLIENT_RECT
  const offsetW = cvRoot.value.offsetWidth;
  const cvRect = cvRoot.value.getBoundingClientRect();
  const scale = offsetW ? cvRect.width / offsetW : 1; 

  const pxPerMm  = offsetW / A4_W_MM
  const pageH    = A4_H_MM * pxPerMm
  const bottomSafeZone = 14 * pxPerMm
  // Tăng lề trên để nhảy trang không bị sát vạch
  const topMargin = 16 * pxPerMm

  let stable = false
  let passes = 0

  while (!stable && passes < 30) {
    stable = true
    passes++
    const currentCvRect = cvRoot.value.getBoundingClientRect()

    for (let i = 0; i < allElements.length; i++) {
      const el = allElements[i]
      const elRect = el.getBoundingClientRect()
      
      // Khử tỷ lệ thu phóng do trình duyệt Zoom
      const top = (elRect.top - currentCvRect.top) / scale
      const height = elRect.height / scale
      const bottom = top + height

      const pageIndex = Math.floor(top / pageH)
      const topInPage = top - (pageIndex * pageH)
      const bottomInPage = topInPage + height

      // Chặn nếu có khối vô tình quá lớn
      if (height > (pageH - bottomSafeZone - topMargin)) continue

      if (bottomInPage > (pageH - bottomSafeZone)) {
         const distToNextPage = pageH - topInPage + topMargin
         const currentMt = parseFloat(el.style.marginTop || '0')
         el.style.setProperty('margin-top', `${currentMt + distToNextPage}px`, 'important')
         stable = false
         break
      }
    }
  }

  // Khôi phục lại hiệu ứng thu phóng
  activeElements.forEach(el => el.style.removeProperty('transform'))

  const finalCvRect = cvRoot.value.getBoundingClientRect()
  let maxBottom = 0
  allElements.forEach(el => {
    const bottom = (el.getBoundingClientRect().bottom - finalCvRect.top) / scale
    if (bottom > maxBottom) maxBottom = bottom
  })

  pageCount.value = Math.max(1, Math.ceil(maxBottom / pageH))
}

// ─── Visibility init ───────────────────────────────────────────────────────
const DESIGN_SIDEBAR = ['skills', 'certifications', 'awards', 'education', 'references', 'languages', 'it_skills']
const DESIGN_MAIN    = ['experience', 'activities', 'project']

onMounted(() => {
  if (props.resumeData?.sections) {
    props.resumeData.sections.forEach(sec => {
      if (DESIGN_SIDEBAR.includes(sec.id)) {
        sec.column = 'left'
      } else if (DESIGN_MAIN.includes(sec.id)) {
        sec.column = 'right'
      } else if (sec.id === 'summary') {
        sec.column = 'right'
      } else if (!sec.column) {
        sec.column = 'right'
      }
      const inDesign = [...DESIGN_SIDEBAR, ...DESIGN_MAIN, 'summary'].includes(sec.id)
      if (sec.isVisible === undefined) {
        sec.isVisible = inDesign
      }
    })
  }

  requestPagination()
  window.addEventListener('resize', requestPagination)
  document.addEventListener('keyup', requestPagination)
})

onUnmounted(() => {
  window.removeEventListener('resize', requestPagination)
  document.removeEventListener('keyup', requestPagination)
  if (paginateTimer) clearTimeout(paginateTimer)
})

watch(() => props.resumeData, requestPagination, { deep: true })

// ─── Computed sections ─────────────────────────────────────────────────────
const summarySection  = computed(() => props.resumeData.sections.find(s => s.id === 'summary'))
const sidebarSections = computed(() => props.resumeData.sections.filter(s => s.column === 'left'  && s.id !== 'summary'))
const mainSections    = computed(() => props.resumeData.sections.filter(s => s.column === 'right' && s.id !== 'summary'))

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

const sidebarIds      = computed(() => sidebarSections.value.map(s => s.id))
const mainIds         = computed(() => mainSections.value.map(s => s.id))
</script>

<style scoped>
/* ── Base ──────────────────────────────────────────────────────────────── */
#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

/* ── Item container ────────────────────────────────────────────────────── */
.item-container {
  position: relative;
}

/* ── Đóng băng Animation (Chống lỗi đo Pixel) ──── */
.paginated-item {
  transition: none !important;
}

/* ── Delete button ──── */
.delete-btn {
  position: absolute !important;
  right: -6px !important;
  top: 0 !important;
  width: 18px !important;
  height: 18px !important;
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  background: #ef4444 !important;
  color: white !important;
  border: none !important;
  border-radius: 50% !important;
  cursor: pointer !important;
  box-shadow: 0 2px 6px rgba(0,0,0,0.25) !important;
  z-index: 30 !important;
  transition: transform 0.12s ease !important;
}
.delete-btn:hover  { transform: scale(1.15) !important; }
.delete-btn:active { transform: scale(0.9)  !important; }

/* ── Section block base ────────────────────────────────────────────────── */
.section-block {
  position: relative;
  border: 2px solid transparent !important;
  border-radius: 0 !important;
  cursor: pointer !important;
  transition:
    box-shadow    0.2s ease,
    border-color  0.15s ease,
    border-radius 0.15s ease,
    background    0.15s ease;
}

.section-block:hover {
  background: transparent !important;
}

.section-active--sidebar {
  border: 2px solid v-bind(activeBorderColorSidebar) !important;
  border-style: solid !important;
  border-radius: 6px !important;
  /* removed scale */
  box-shadow: 0 6px 20px rgba(0,0,0,0.12), 0 1px 4px rgba(0,0,0,0.06) !important;
  background: rgba(255,255,255,0.35) !important;
  z-index: 10 !important;
}

.section-active--main {
  border: 2px solid v-bind(activeBorderColorMain) !important;
  border-style: solid !important;
  border-radius: 6px !important;
  /* removed scale */
  box-shadow: 0 4px 18px rgba(v-bind(primaryRgb),0.10), 0 1px 4px rgba(v-bind(primaryRgb),0.06) !important;
  background: rgba(v-bind(secondaryRgb),0.10) !important;
  z-index: 10 !important;
}

.section-active--header {
  border-radius: 6px !important;
  outline: 2px solid rgba(255,255,255,0.4) !important;
  outline-offset: 4px !important;
}

/* ── Nav buttons ── */
.nav-btns {
  position: absolute !important;
  right: 4px !important;
  top: 4px !important;
  display: none !important; 
  flex-direction: row !important;
  gap: 4px !important;
  z-index: 9999 !important;
}

.section-active--sidebar .nav-btns,
.section-active--main .nav-btns {
  display: flex !important;
}

.nav-btn {
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  padding: 4px !important;
  background: v-bind(templatePrimaryColor) !important;
  color: white !important;
  border: none !important;
  border-radius: 4px !important;
  cursor: pointer !important;
  box-shadow: 0 2px 6px rgba(0,0,0,0.15) !important;
  transition: background 0.12s, transform 0.1s !important;
}
.nav-btn:hover  { filter: brightness(1.2) !important; }
.nav-btn:active { transform: scale(0.91) !important; }

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
  background: transparent;
  padding-left: 5px;
}

.fade-btns-enter-active,
.fade-btns-leave-active {
  transition: none !important;
}
.fade-btns-enter-from,
.fade-btns-leave-to {
  opacity: 0;
  transform: scale(0.85);
}

/* ── HTML content ──────────────────────────────────────────────────────── */
:deep(.html-content)                                   { margin: 0 !important; padding: 0 !important; }
:deep(.html-content p)                                 { margin: 0 !important; padding: 0 !important; }
:deep(.html-content ul)                                { list-style-type: disc !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content ol)                                { list-style-type: decimal !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content b), :deep(.html-content strong)    { font-weight: bold !important; }
:deep(.html-content i), :deep(.html-content em)        { font-style: italic !important; }
:deep(.html-content u)                                 { text-decoration: underline !important; }
:deep(.html-content ul li), :deep(.html-content ol li) { margin-bottom: 2px !important; }

:deep(.html-content li:has(> font[size="1"])) { font-size: 10px !important; }
:deep(.html-content li:has(> font[size="2"])) { font-size: 11px !important; }
:deep(.html-content li:has(> font[size="3"])) { font-size: 12.5px !important; }
:deep(.html-content li:has(> font[size="4"])) { font-size: 14px !important; }

/* BẢO VỆ GIAO DIỆN BẢN IN, XÓA MỌI GHI ĐÈ SAI LỆCH VỊ TRÍ PADDING */
@media print {
  .no-print { display: none !important; }
  .section-block,
  .section-active--main,
  .section-active--sidebar,
  .section-active--header {
    cursor: default !important;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
    transform: none !important;
    border-radius: 0 !important;
    outline: none !important;
  }
}

:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .section-active--main),
:global(.is-exporting-pdf .section-active--sidebar),
:global(.is-exporting-pdf .section-active--header) {
  cursor: default !important;
  box-shadow: none !important;
  background: transparent !important;
  border-color: transparent !important;
  transform: none !important;
  border-radius: 0 !important;
  outline: none !important;
}
</style>
