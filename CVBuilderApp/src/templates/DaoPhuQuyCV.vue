<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="relative box-border bg-white overflow-hidden text-[#333] flex flex-row"
    :style="{
      width: '210mm',
      height: `${Math.max(1, pageCount) * 297}mm`,
      fontFamily: '\'Segoe UI\', Tahoma, Geneva, Verdana, sans-serif'
    }"
    @click="selectedSectionId = null"
  >
    <!-- HOA VĂN BACKGROUND -->
    <div class="absolute top-0 right-0 w-[200px] h-[200px] pointer-events-none opacity-80 z-0">
      <svg width="200" height="200" viewBox="0 0 200 200" fill="none" xmlns="http://www.w3.org/2000/svg">
        <path d="M180 20C160 50 140 30 120 60C100 90 130 110 110 140C90 170 60 150 40 180"
              stroke="#333" stroke-width="1" stroke-linecap="round" stroke-dasharray="2 4"/>
        <path d="M190 30C175 55 160 45 145 70C130 95 150 110 135 135"
              stroke="#333" stroke-width="0.5" opacity="0.4"/>
        <circle cx="180" cy="20" r="2" fill="#333"/>
      </svg>
    </div>
    <div class="absolute bottom-[50px] right-[50px] w-[150px] h-[100px] pointer-events-none opacity-20 z-0">
      <svg width="150" height="100" viewBox="0 0 150 100" fill="none" xmlns="http://www.w3.org/2000/svg">
        <ellipse cx="40"  cy="50" rx="35" ry="45" stroke="#333" stroke-width="0.5" stroke-dasharray="4 2"/>
        <ellipse cx="75"  cy="50" rx="35" ry="45" stroke="#333" stroke-width="0.5" stroke-dasharray="4 2"/>
        <ellipse cx="110" cy="50" rx="35" ry="45" stroke="#333" stroke-width="0.5" stroke-dasharray="4 2"/>
      </svg>
    </div>

    <!-- CỘT TRÁI (SIDEBAR) -->
    <aside
      class="z-10 flex flex-col shrink-0 relative box-border text-white"
      :style="{ width: '35%', backgroundColor: templatePrimaryColor, padding: '40px 25px' }"
      @click.self="selectedSectionId = null"
    >
      <div class="paginated-item relative z-20 w-full flex flex-col items-center mb-[30px]">
        <div
          class="relative rounded-full overflow-hidden mx-auto"
          :style="{ width: '160px', height: '160px', border: '5px solid #ffffff' }"
        >
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center bg-white/10 text-white/50">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
            </svg>
          </div>
        </div>
      </div>

      <div class="w-full mb-[5px]">
        <h3 class="sidebar-section-title paginated-item">LIÊN HỆ VỚI TÔI</h3>
        <ul
          v-if="contactItems.length > 0"
          class="w-full list-none p-0 m-0 flex flex-col gap-[8px] relative contact-block"
          :class="{ 'contact-active': selectedSectionId === 'contact' }"
          @click.stop="selectedSectionId = selectedSectionId === 'contact' ? null : 'contact'"
          :style="{ fontSize: '13px !important', lineHeight: '1.5', paddingLeft: '12px !important' }"
        >
          <li
            v-for="(ci, ciIdx) in contactItems"
            :key="ci.key"
            class="flex items-start paginated-item relative contact-item-container"
          >
            <div class="w-[20px] text-center shrink-0 mr-[5px] mt-[2px]">
              <svg class="w-[12px] h-[12px] inline-block" fill="currentColor" viewBox="0 0 20 20" v-html="ci.icon"></svg>
            </div>
            <span class="break-all" v-html="ci.value"></span>

            <!-- Move Up / Move Down / Delete buttons -->
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
        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block relative w-full mb-[5px]"
            :class="{ 'section-active--sidebar': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <div v-show="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="moveSectionUp(section.id, 'left')" class="nav-btn" title="Lên">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionDown(section.id, 'left')" class="nav-btn" title="Xuống">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionHorizontal(section.id, 'right')" class="nav-btn" title="Sang cột phải">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
              </button>
              <button @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <h3 class="sidebar-section-title paginated-item">{{ section.id === 'summary' ? (section.title || 'MỤC TIÊU NGHỀ NGHIỆP') : section.title }}</h3>

            <div v-if="section.id === 'summary'"
                 class="html-content-sidebar text-justify whitespace-pre-line w-full flex flex-col"
                 :style="{ paddingLeft: '12px !important' }"
                 v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Mục tiêu nghề nghiệp...')">
            </div>

            <div v-else class="w-full flex flex-col gap-[8px]" :style="{ paddingLeft: '12px !important' }">
              <div
                v-for="(item, itemIndex) in section.items"
                :key="item._refId"
                class="item-container relative w-full text-white"
                :style="{ fontSize: '13px !important', lineHeight: '1.5', margin: '0 !important', padding: '0 !important' }"
              >
                <!-- Kinh nghiệm / Học vấn / Hoạt động cột trái -->
                <div v-if="['education','experience','project','activities'].includes(section.id)" class="w-full flex flex-col">
                  <div class="paginated-item w-full flex justify-between items-start gap-2 relative">
                    <span class="font-bold flex-1 break-words leading-tight flex flex-col" v-html="formatDesc(item.school || item.company || item.organization || item.name || item.title)"></span>
                    <span v-if="item.year || item.time || item.date" class="font-bold shrink-0 whitespace-nowrap text-right opacity-90 text-[11px]">{{ item.year || item.time || item.date }}</span>
                  </div>
                  <div v-if="item.major || item.role || item.position" class="italic opacity-90 mt-[2px] text-[12px] paginated-item flex flex-col" v-html="formatDesc(item.major || item.role || item.position)"></div>
                  <div v-if="item.gradType" class="paginated-item font-medium mt-[2px] opacity-90" :style="{ fontSize: '12px !important' }">Xếp loại: {{ item.gradType }}</div>
                  <div v-if="item.desc" class="html-content-sidebar text-justify whitespace-pre-line break-words w-full mt-[4px] flex flex-col" v-html="formatDesc(item.desc)"></div>
                </div>
                
                <!-- Kỹ năng cột trái -->
                <div v-else-if="['skills','languages','it_skills'].includes(section.id)" class="paginated-item flex flex-col">
                  <div class="w-full flex justify-between items-start gap-2">
                    <span class="font-bold flex-1 break-words">{{ item.name }}</span>
                    <span v-if="item.level" class="font-bold shrink-0 whitespace-nowrap text-right opacity-90 text-[11px]">{{ item.level }}</span>
                  </div>
                  <div v-if="item.info" class="w-full break-words mt-[2px] opacity-90">{{ item.info }}</div>
                </div>
                
                <!-- Chứng chỉ / Giải thưởng cột trái -->
                <div v-else class="w-full flex flex-col">
                  <div class="paginated-item w-full flex justify-between items-start gap-2 relative">
                    <span class="font-bold flex-1 break-words leading-tight text-white flex flex-col" v-html="formatDesc(item.name || item.title || item.company || item.organization)"></span>
                    <span v-if="item.year || item.time || item.date" class="font-bold shrink-0 whitespace-nowrap text-right opacity-90 text-[11px]">{{ item.year || item.time || item.date }}</span>
                  </div>
                  <div v-if="item.major || item.role || item.info" class="italic opacity-90 mt-[2px] text-[12px] paginated-item flex flex-col" v-html="formatDesc(item.major || item.role || item.info)"></div>
                  <div v-if="item.desc || item.details" class="html-content-sidebar text-justify whitespace-pre-line break-words w-full mt-[2px] flex flex-col" :style="{ lineHeight: '1.5' }" v-html="formatDesc(item.desc || item.details)"></div>
                </div>

                <button v-show="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>
      </div>
    </aside>

    <!-- CỘT PHẢI (MAIN) -->
    <main
      class="flex-1 flex flex-col relative bg-white z-20 overflow-hidden box-border"
      :style="{ paddingTop: '0', paddingRight: '35px', paddingBottom: '40px', paddingLeft: '0' }"
      @click.self="selectedSectionId = null"
    >
      <header
        class="paginated-item w-full flex flex-col relative"
        :style="{
          borderBottom: `4px solid ${templatePrimaryColor}`,
          paddingBottom: '15px',
          paddingTop: '50px',
          marginBottom: '25px',
          paddingLeft: '35px'
        }"
      >
        <h1
          class="uppercase break-words w-full m-0"
          :style="{ fontSize: '38px !important', fontWeight: '300 !important', color: '#333', lineHeight: '1.15', letterSpacing: '-0.5px' }"
          v-html="formattedFullName"
        ></h1>
        <h2
          class="uppercase break-words w-full m-0 text-[#555]"
          :style="{ fontSize: '15px !important', letterSpacing: '2px', fontWeight: 'normal', marginTop: '10px !important' }"
          v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'VỊ TRÍ ỨNG TUYỂN'"
        ></h2>
      </header>

      <div class="w-full flex flex-col flex-1 m-0 p-0">
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block relative w-full mb-[20px]"
            :class="{ 'section-active--main': selectedSectionId === section.id }"
            :style="{ paddingLeft: '35px', paddingRight: '10px' }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <div v-show="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="moveSectionUp(section.id, 'right')" class="nav-btn" title="Lên">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionDown(section.id, 'right')" class="nav-btn" title="Xuống">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionHorizontal(section.id, 'left')" class="nav-btn" title="Sang cột trái">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
              </button>
              <button @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <h3 class="main-section-title paginated-item" :style="{ color: templatePrimaryColor }">
              <svg v-if="section.id === 'summary'" class="section-icon" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clip-rule="evenodd"/>
              </svg>
              <svg v-else-if="section.id === 'experience'" class="section-icon" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M6 6V5a3 3 0 013-3h2a3 3 0 013 3v1h2a2 2 0 012 2v3.57A22.952 22.952 0 0110 13a22.95 22.95 0 01-8-1.43V8a2 2 0 012-2h2zm2-1a1 1 0 011-1h2a1 1 0 011 1v1H8V5zm1 5a1 1 0 011-1h.01a1 1 0 110 2H10a1 1 0 01-1-1z" clip-rule="evenodd"/>
                <path d="M2 13.692V16a2 2 0 002 2h12a2 2 0 002-2v-2.308A24.974 24.974 0 0110 15c-2.796 0-5.487-.46-8-1.308z"/>
              </svg>
              <svg v-else-if="section.id === 'references'" class="section-icon" fill="currentColor" viewBox="0 0 20 20">
                <path d="M9 6a3 3 0 11-6 0 3 3 0 016 0zM17 6a3 3 0 11-6 0 3 3 0 016 0zM12.93 17c.046-.327.07-.66.07-1a6.97 6.97 0 00-1.5-4.33A5 5 0 0119 16v1h-6.07zM6 11a5 5 0 015 5v1H1v-1a5 5 0 015-5z"/>
              </svg>
              <svg v-else-if="section.id === 'education'" class="section-icon" fill="currentColor" viewBox="0 0 20 20">
                <path d="M10.394 2.08a1 1 0 00-.788 0l-7 3a1 1 0 000 1.84L5.25 8.051a.999.999 0 01.356-.257l4-1.714a1 1 0 11.788 1.838L7.667 9.088l1.94.831a1 1 0 00.787 0l7-3a1 1 0 000-1.838l-7-3zM3.31 9.397L5 10.12v4.102a8.969 8.969 0 00-1.05-.174 1 1 0 01-.89-.89 11.115 11.115 0 01.25-3.762zM9.3 16.573A9.026 9.026 0 007 14.935v-3.957l1.818.78a3 3 0 002.364 0l5.508-2.361a11.026 11.026 0 01.25 3.762 1 1 0 01-.89.89 8.968 8.968 0 00-5.35 2.524 1 1 0 01-1.4 0z"/>
              </svg>
              <svg v-else class="section-icon" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd"/>
              </svg>
              {{ section.id === 'summary' ? (section.title || 'MỤC TIÊU NGHỀ NGHIỆP') : section.title }}
            </h3>

            <!-- SUMMARY -->
            <div v-if="section.id === 'summary'"
                 class="html-content text-justify whitespace-pre-line w-full text-[#444] flex flex-col"
                 :style="{ fontSize: '13.5px !important', lineHeight: '1.65 !important', margin: '0 !important' }"
                 v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Mục tiêu nghề nghiệp...')">
            </div>

            <div v-else class="w-full flex flex-col gap-[14px]">
              <div
                v-for="(item, itemIndex) in section.items"
                :key="item._refId"
                class="item-container relative w-full text-[#333]"
                :style="{ margin: '0 !important', padding: '0 !important' }"
              >
                <!-- Kinh nghiệm / Học vấn / Hoạt động cột phải -->
                <div v-if="['education','experience','project','activities'].includes(section.id)" class="w-full flex flex-col pl-[20px]">
                  <div class="w-full flex justify-between items-start gap-2 mt-[2px] relative">
                    <div class="absolute -left-[20px] top-[6px] w-[6px] h-[6px] rounded-full bg-black"></div>
                    <span class="font-bold break-words flex-1 leading-tight text-[#222] flex flex-col" :style="{ fontSize: '14.5px' }" v-html="formatDesc(item.school || item.company || item.organization || item.name || item.title)"></span>
                    <span v-if="item.year || item.time || item.date" class="paginated-item font-bold shrink-0 whitespace-nowrap text-right" :style="{ color: templatePrimaryColor, fontSize: '13px' }">{{ item.year || item.time || item.date }}</span>
                  </div>
                  <span v-if="item.major || item.role || item.position" class="italic text-[#555] w-full leading-tight mt-[2px] flex flex-col paginated-item" :style="{ fontSize: '13.5px' }" v-html="formatDesc(item.major || item.role || item.position)"></span>
                  <div v-if="item.gradType" class="paginated-item font-medium mt-[2px]" :style="{ color: templatePrimaryColor, fontSize: '13px !important' }">Trạng thái: {{ item.gradType }}</div>
                  <div v-if="item.desc" class="html-content text-justify whitespace-pre-line break-words w-full mt-[5px] text-[#444] flex flex-col" :style="{ fontSize: '13.5px', lineHeight: '1.65' }" v-html="formatDesc(item.desc)"></div>
                </div>

                <!-- Kỹ năng cột phải -->
                <div v-else-if="['skills','languages','it_skills'].includes(section.id)" class="w-full flex flex-col pl-[20px]">
                  <div class="w-full flex justify-between items-start gap-2 mt-[2px] relative">
                    <div class="absolute -left-[20px] top-[6px] w-[6px] h-[6px] rounded-full bg-black"></div>
                    <span class="font-bold break-words flex-1 leading-tight text-[#222] flex flex-col" :style="{ fontSize: '14.5px' }" v-html="formatDesc(item.name)"></span>
                    <span v-if="item.level" class="paginated-item font-bold shrink-0 whitespace-nowrap text-right" :style="{ color: templatePrimaryColor, fontSize: '13px' }">{{ item.level }}</span>
                  </div>
                  <span v-if="item.info" class="italic text-[#555] w-full leading-tight mt-[2px] flex flex-col paginated-item" :style="{ fontSize: '13.5px' }" v-html="formatDesc(item.info)"></span>
                </div>

                <!-- Chứng chỉ / Giải thưởng / Khác cột phải -->
                <div v-else class="w-full flex flex-col pl-[20px]">
                  <div class="w-full flex justify-between items-start gap-2 mt-[2px] relative">
                    <div class="absolute -left-[20px] top-[6px] w-[6px] h-[6px] rounded-full bg-black"></div>
                    <div class="font-bold break-words flex-1 leading-tight text-[#222] flex flex-col" :style="{ fontSize: '14.5px' }" v-html="formatDesc(item.name || item.title || item.company || item.organization)"></div>
                    <div v-if="item.year || item.time || item.date" class="paginated-item font-bold shrink-0 whitespace-nowrap text-right" :style="{ color: templatePrimaryColor, fontSize: '13px' }">{{ item.year || item.time || item.date }}</div>
                  </div>
                  <div v-if="item.info || item.role || item.major" class="paginated-item text-[14px] font-medium text-[#555] mt-[2px] flex flex-col" v-html="formatDesc(item.info || item.role || item.major)"></div>
                  <div v-if="item.desc || item.details" class="html-content text-justify whitespace-pre-line break-words w-full text-[#444] flex flex-col mt-[5px]" :style="{ fontSize: '13.5px', lineHeight: '1.65' }" v-html="formatDesc(item.desc || item.details)"></div>
                </div>

                <button v-show="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>
      </div>
    </main>

    <template v-for="p in (pageCount - 1)" :key="'div-' + p">
      <div
        class="absolute left-0 w-full z-50 flex flex-col items-center justify-center pointer-events-none no-print"
        :style="{ top: `calc(${p * 297}mm - 8px)` }"
      >
        <div class="w-[105%] h-[16px] bg-slate-800/95 shadow-inner overflow-hidden border-y border-black/30 backdrop-blur-sm"></div>
        <span class="absolute text-[9px] uppercase font-bold text-slate-300 tracking-widest bg-slate-700 px-3 py-0.5 rounded border border-slate-600 shadow-md">Ngắt trang {{ p + 1 }}</span>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, ref, onMounted, nextTick, watch, onUnmounted } from 'vue'

const cvRoot            = ref(null)
const pageCount         = ref(1)
const selectedSectionId = ref(null)

const props = defineProps({ resumeData: { type: Object, required: true } })
const emit  = defineEmits(['removeItem'])

const isEmpty = (val) => {
  if (!val) return true
  if (typeof val !== 'string') return false
  return val.replace(/<[^>]*>/g, '').replace(/&nbsp;/g, ' ').trim() === ''
}

// ─── CONTACT ITEMS LOGIC ───
const contactIcons = {
  phone: '<path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z"></path>',
  email: '<path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z"></path><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z"></path>',
  dob: '<path fill-rule="evenodd" d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" clip-rule="evenodd"></path>',
  address: '<path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd"></path>'
}

const contactOrder = ref(['phone', 'email', 'dob', 'address'])
const hiddenContacts = ref([])

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  switch (key) {
    case 'phone': return g.phone || ''
    case 'email': return g.email || ''
    case 'dob': return g.dob || g.birthDate || ''
    case 'address': return g.address || ''
    default: return ''
  }
}

const contactItems = computed(() => {
  return contactOrder.value
    .filter(key => !hiddenContacts.value.includes(key))
    .filter(key => !isEmpty(getContactValue(key)))
    .map(key => ({
      key,
      icon: contactIcons[key],
      value: getContactValue(key)
    }))
})

const moveContactUp = (idx) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && !isEmpty(getContactValue(k)))
  if (idx <= 0) return
  const keyA = visible[idx], keyB = visible[idx - 1]
  const idxA = contactOrder.value.indexOf(keyA), idxB = contactOrder.value.indexOf(keyB)
  const arr = [...contactOrder.value]
  ;[arr[idxA], arr[idxB]] = [arr[idxB], arr[idxA]]
  contactOrder.value = arr
}

const moveContactDown = (idx) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && !isEmpty(getContactValue(k)))
  if (idx >= visible.length - 1) return
  const keyA = visible[idx], keyB = visible[idx + 1]
  const idxA = contactOrder.value.indexOf(keyA), idxB = contactOrder.value.indexOf(keyB)
  const arr = [...contactOrder.value]
  ;[arr[idxA], arr[idxB]] = [arr[idxB], arr[idxA]]
  contactOrder.value = arr
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
    }
    hiddenContacts.value.push(key)
    requestPagination()
  }
}

const formatDesc = (text) => {
  if (!text) return ''
  
  if (!/<[a-z][\s\S]*>/i.test(text)) {
    return text.split('\n')
               .map(l => l.trim())
               .filter(Boolean)
               .map(l => `<span class="paginated-item block w-full">${l}</span>`)
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

  const wrapTextNodes = (element) => {
    Array.from(element.childNodes).forEach(node => {
      if (node.nodeType === Node.TEXT_NODE) {
        if (node.textContent.trim()) {
           const wrapper = document.createElement('span')
           wrapper.className = 'paginated-item block w-full'
           node.replaceWith(wrapper)
           wrapper.appendChild(node)
        }
      } else if (node.nodeType === Node.ELEMENT_NODE) {
        node.classList.remove('paginated-item')
        if (node.tagName === 'BR') {
           node.outerHTML = '<span class="paginated-item block w-full" style="height: 6px;"></span>'
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

const templatePrimaryColor = computed(() => props.resumeData.theme?.primaryColor || '#004C82')

const formattedFullName = computed(() => {
  const name  = props.resumeData.general.fullName || 'HỌ VÀ TÊN ỨNG VIÊN'
  const words = name.trim().split(/\s+/)
  if (words.length < 2) return `<strong style="font-weight:800">${name}</strong>`
  const first = words.slice(0, -2).join(' ')
  const last  = words.slice(-2).join(' ')
  return first
    ? `${first} <strong style="font-weight:800">${last}</strong>`
    : `<strong style="font-weight:800">${last}</strong>`
})

const swapSections = (sections, idxA, idxB) => {
  const temp = sections[idxA]
  sections.splice(idxA, 1, sections[idxB])
  sections.splice(idxB, 1, temp)
}

const moveSectionUp = (sectionId, column) => {
  const col = column === 'left' ? 'left' : 'right'
  const colSections = props.resumeData.sections.filter(s => s.column === col)
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
  const colSections = props.resumeData.sections.filter(s => s.column === col)
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

// ─── THUẬT TOÁN ĐO CHÍNH XÁC (TÍCH HỢP CHIẾN LƯỢC TỪ TỪNG DÒNG LÁ) ─────────────
const A4_WIDTH_MM  = 210
const A4_HEIGHT_MM = 297

let paginateTimer = null
const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 60)
}

const doPagination = async () => {
  if (!cvRoot.value) return

  const activeElements = cvRoot.value.querySelectorAll('.section-selected, .section-block')
  activeElements.forEach(el => el.style.setProperty('transform', 'none', 'important'))

  const allElements = Array.from(cvRoot.value.querySelectorAll('.paginated-item')).filter(el => {
    if (el.offsetHeight === 0) return false;
    let parent = el.parentElement;
    while (parent && parent !== cvRoot.value) {
      if (parent.classList.contains('paginated-item')) return false;
      parent = parent.parentElement;
    }
    return true;
  });

  allElements.forEach(el => {
    el.style.setProperty('margin-top', '0px', 'important')
  })
  
  await nextTick()

  const offsetW = cvRoot.value.offsetWidth;
  const cvRect = cvRoot.value.getBoundingClientRect();
  const scale = offsetW ? cvRect.width / offsetW : 1; 

  const pxPerMm = offsetW / A4_WIDTH_MM;
  const pageH = A4_HEIGHT_MM * pxPerMm;
  const bottomSafeZone = 14 * pxPerMm;
  const topMargin = 28 * pxPerMm;

  let stable = false
  let passes = 0

  while (!stable && passes < 40) {
    stable = true
    passes++
    const currentCvRect = cvRoot.value.getBoundingClientRect()

    for (let i = 0; i < allElements.length; i++) {
      const el = allElements[i]
      if (el.offsetHeight === 0) continue

      const elRect = el.getBoundingClientRect()
      const top = (elRect.top - currentCvRect.top) / scale
      const height = elRect.height / scale
      const bottom = top + height

      const pageIndex = Math.floor(top / pageH)
      const topInPage = top - (pageIndex * pageH)
      const bottomInPage = topInPage + height

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

  activeElements.forEach(el => el.style.removeProperty('transform'))

  const finalCvRect = cvRoot.value.getBoundingClientRect()
  let maxBottom = 0
  allElements.forEach(el => {
    const bottom = (el.getBoundingClientRect().bottom - finalCvRect.top) / scale
    if (bottom > maxBottom) maxBottom = bottom
  })

  pageCount.value = Math.max(1, Math.ceil(maxBottom / pageH))
}

onMounted(() => {
  if (props.resumeData?.sections) {
    const SIDEBAR_IDS = ['education', 'it_skills', 'languages', 'skills']
    const MAIN_IDS    = ['experience', 'references']
    const HIDDEN_IDS  = ['project', 'activities', 'awards', 'hobbies', 'certificates']

    props.resumeData.sections.forEach(sec => {
      if (sec.isVisible === undefined) {
        if (SIDEBAR_IDS.includes(sec.id)) {
          sec.isVisible = true
          sec.column    = 'left'
        } else if (MAIN_IDS.includes(sec.id)) {
          sec.isVisible = true
          sec.column    = 'right'
        } else if (sec.id === 'summary') {
          sec.isVisible = true
          sec.column    = 'right'
        } else if (HIDDEN_IDS.includes(sec.id)) {
          sec.isVisible = false
          if (!sec.column) sec.column = 'right'
        }
      } else {
        if (SIDEBAR_IDS.includes(sec.id)) {
          sec.column    = 'left'
        } else if (MAIN_IDS.includes(sec.id) || sec.id === 'summary') {
          sec.column    = 'right'
        }
      }
    })
  }

  requestPagination()
  window.addEventListener('resize', requestPagination)
  
  if (cvRoot.value) {
    const obs = new MutationObserver(requestPagination)
    obs.observe(cvRoot.value, { childList: true, subtree: true, characterData: true })
    cvRoot._observer = obs
  }
})

onUnmounted(() => {
  window.removeEventListener('resize', requestPagination)
  if (cvRoot._observer) cvRoot._observer.disconnect()
  if (paginateTimer) clearTimeout(paginateTimer)
})

watch(() => props.resumeData, requestPagination, { deep: true })

const sidebarSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'left')
)

const mainSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'right')
)
</script>

<style scoped>
/* ── Base ──────────────────────────────────────────────────────────────── */
#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

/* ── Sidebar section title ─────────────────────────────────────────────── */
.sidebar-section-title { 
  display: block;
  font-size: 16px !important;
  font-weight: 800 !important;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  border-bottom: 2px solid rgba(255, 255, 255, 0.45) !important; 
  padding-bottom: 8px !important;
  padding-left: 12px !important;
  margin: 20px 0 12px 0 !important;
  color: #ffffff !important;
  text-shadow: 0 0 1px rgba(0,0,0,0.1); 
}

/* ── Main section title ────────────────────────────────────────────────── */
.main-section-title {
  display: flex !important;
  align-items: center !important;
  gap: 8px !important;
  font-size: 16px !important;
  font-weight: 800 !important;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  border-bottom: 2px solid currentColor !important;
  padding-bottom: 8px !important;
  margin: 0 0 14px 0 !important;
}

.section-icon {
  width: 18px;
  height: 18px;
  flex-shrink: 0;
  display: inline-block;
}

/* ── Item container ────────────────────────────────────────────────────── */
.item-container { position: relative; }

/* ── Contact block ─────────────────────────────────────────────────────── */
.contact-block {
  cursor: pointer;
  border: 1px solid transparent;
  transition: background 0.2s, border-color 0.2s;
  border-radius: 4px;
  padding: 4px !important; /* Constant padding to prevent shift */
}
.contact-active {
  border: 1px solid rgba(255, 255, 255, 0.4) !important;
  background: rgba(255, 255, 255, 0.15);
}

.contact-item-container {
  position: relative;
  padding: 2px 0;
}

.contact-item-btns {
  position: absolute;
  right: 0;
  top: 50%;
  transform: translateY(-50%);
  display: flex;
  gap: 2px;
  z-index: 50;
}

.nav-btn--xs {
  padding: 2px !important;
  border-radius: 2px !important;
}

/* ── Delete button ─────────────────────────────────────────────────────── */
.delete-btn {
  position: absolute;
  right: -8px;
  top: 0;
  width: 18px;
  height: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #ef4444 !important;
  color: white !important;
  border: none;
  border-radius: 50% !important;
  cursor: pointer;
  box-shadow: 0 2px 6px rgba(0,0,0,0.3);
  z-index: 30;
  transition: transform 0.12s ease;
}
.delete-btn:hover  { transform: scale(1.15) !important; }
.delete-btn:active { transform: scale(0.9)  !important; }

/* ── Section block ─────────────────────────────────────────────────────── */
.section-block {
  position: relative;
  border: 2px solid transparent !important;
  border-radius: 0 !important;
  cursor: pointer;
  transition:
    box-shadow    0.2s ease,
    border-color  0.15s ease,
    border-radius 0.15s ease,
}

.section-active--sidebar {
  border: 2px solid v-bind(templatePrimaryColor) !important;
  border-style: solid !important;
  border-radius: 6px !important;
  /* removed scale */
  box-shadow: 0 6px 20px rgba(0,0,0,0.28), 0 1px 4px rgba(0,0,0,0.1) !important;
  background: rgba(255,255,255,0.08) !important;
  z-index: 10 !important;
}

.section-active--main {
  border: 2px solid v-bind(templatePrimaryColor) !important;
  border-style: solid !important;
  border-radius: 6px !important;
  /* removed scale */
  box-shadow: 0 4px 18px rgba(0,76,130,0.12), 0 1px 4px rgba(0,76,130,0.06) !important;
  background: rgba(0,76,130,0.03) !important;
  z-index: 10 !important;
}

/* ── Nav buttons ───────────────────────────────────────────────────────── */
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
  background: #2563eb !important;
  color: white !important;
  border: none;
  border-radius: 4px !important;
  cursor: pointer;
  box-shadow: 0 2px 6px rgba(37,99,235,0.4);
  transition: background 0.12s, transform 0.1s;
}
.nav-btn:hover  { background: #1d4ed8 !important; }
.nav-btn:active { transform: scale(0.91) !important; }

.nav-btn-danger {
  background: #ef4444 !important;
  box-shadow: 0 2px 6px rgba(239, 68, 68, 0.4) !important;
}
.nav-btn-danger:hover {
  background: #dc2626 !important;
}

/* ── HTML content — main ───────────────────────────────────────────────── */
:deep(.html-content)                                   { margin: 0 !important; padding: 0 !important; }
:deep(.html-content p)                                 { margin: 0 !important; padding: 0 !important; }
:deep(.html-content ul)                                { list-style-type: disc !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content ol)                                { list-style-type: decimal !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content b), :deep(.html-content strong)    { font-weight: bold; }
:deep(.html-content i), :deep(.html-content em)        { font-style: italic; }
:deep(.html-content u)                                 { text-decoration: underline; }
:deep(.html-content ul li), :deep(.html-content ol li) { margin-bottom: 2px !important; }

/* ── HTML content — sidebar ────────────────────────────────────────────── */
:deep(.html-content-sidebar)                                        { margin: 0 !important; padding: 0 !important; color: white !important; }
:deep(.html-content-sidebar p)                                      { margin: 0 !important; padding: 0 !important; }
:deep(.html-content-sidebar ul)                                     { list-style-type: disc !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content-sidebar ol)                                     { list-style-type: decimal !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content-sidebar b), :deep(.html-content-sidebar strong) { font-weight: bold; }
:deep(.html-content-sidebar i), :deep(.html-content-sidebar em)     { font-style: italic; opacity: 0.9; }
:deep(.html-content-sidebar u)                                      { text-decoration: underline; }
:deep(.html-content-sidebar ul li),
:deep(.html-content-sidebar ol li)                                  { margin-bottom: 2px !important; }

/* ── Legacy font size ──────────────────────────────────────────────────── */
:deep(.html-content li:has(> font[size="1"])),
:deep(.html-content-sidebar li:has(> font[size="1"])) { font-size: 10px; }
:deep(.html-content li:has(> font[size="2"])),
:deep(.html-content-sidebar li:has(> font[size="2"])) { font-size: 11px; }
:deep(.html-content li:has(> font[size="3"])),
:deep(.html-content-sidebar li:has(> font[size="3"])) { font-size: 13px; }
:deep(.html-content li:has(> font[size="4"])),
:deep(.html-content-sidebar li:has(> font[size="4"])) { font-size: 15px; }

/* ── Print / PDF ───────────────────────────────────────────────────────── */
@media print {
  .no-print { display: none !important; }
  .section-block,
  .section-active--main,
  .section-active--sidebar {
    cursor: default !important;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
    transform: none !important;
    border-radius: 0 !important;
    padding: 0 !important;
    outline: none !important;
  }
}

:global(.is-exporting-pdf .no-print) { display: none !important; }
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

/* Đóng băng Animation (Chống lỗi đo Pixel) */
.paginated-item {
  transition: none !important;
}
</style>
