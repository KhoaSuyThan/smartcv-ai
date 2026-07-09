<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="flex flex-row relative box-border overflow-hidden"
    :style="{
      width: '210mm',
      height: `${Math.max(1, pageCount) * 297}mm`,
      fontFamily: '\'Segoe UI\', Roboto, Helvetica, Arial, sans-serif',
      backgroundColor: `${templatePageBgColor} !important`
    }"
  >
    <main
      class="flex-[6] flex flex-col relative z-20 overflow-hidden box-border pt-[40px] pb-[50px]"
      :style="{ backgroundColor: `${templatePageBgColor} !important` }"
      @click.self="selectedSectionId = null"
    >
      <header
        class="paginated-item w-full flex flex-col relative"
        :style="{
          paddingBottom: '65px',
          marginBottom: '0',
          marginLeft: '20px !important',
          paddingTop: '75px !important',
          marginRight: '20px !important',
          width: 'calc(100% - 40px)'
        }"
      >
        <h1
          class="uppercase break-words w-full m-0"
          :style="{
            fontSize: '56px !important',
            fontWeight: '800 !important',
            color: templatePrimaryColor,
            lineHeight: '1.1',
            marginTop: '0 !important',
            paddingLeft: '20px !important'
          }"
          v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'HỌ VÀ TÊN'"
        ></h1>
        <p
          class="uppercase break-words w-full m-0"
          :style="{
            fontSize: '22px !important',
            letterSpacing: '2px',
            color: templateAccentColor,
            marginTop: '10px !important',
            fontWeight: '600 !important',
            paddingLeft: '20px !important'
          }"
          v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'VỊ TRÍ ỨNG TUYỂN'"
        ></p>
      </header>

      <div
        class="paginated-item w-full"
        :style="{ height: '10px', backgroundColor: templatePrimaryColor, marginBottom: '40px', width: '100%' }"
      ></div>

      <div class="w-full flex flex-col flex-1 m-0 p-0">
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
            :style="{ marginLeft: '20px !important', marginRight: '20px !important', width: 'calc(100% - 40px)' }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <div v-show="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang phải (Cột phụ)">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
              </button>
              <button @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <h1
              class="paginated-item uppercase w-full block"
              :style="{
                fontSize: '17px !important',
                color: templateAccentColor,
                paddingBottom: '8px !important',
                margin: '20px 0 15px 0 !important',
                paddingLeft: '20px !important',
                fontWeight: '800 !important',
                letterSpacing: '1px'
              }"
            ><span v-html="section.title || (section.id === 'summary' ? 'MỤC TIÊU NGHỀ NGHIỆP' : section.id === 'contact' ? 'THÔNG TIN LIÊN HỆ' : '')"></span></h1>

            <div
              class="w-full flex flex-col"
              :style="{
                gap: '20px',
                borderLeft: ['summary', 'contact'].includes(section.id) ? 'none' : '1px solid ' + templateAccentColor,
                paddingLeft: ['summary', 'contact'].includes(section.id) ? '0' : '0 !important',
                marginLeft: ['summary', 'contact'].includes(section.id) ? '20px !important' : '20px !important',
                width: ['summary', 'contact'].includes(section.id) ? 'calc(100% - 20px)' : 'calc(100% - 20px)'
              }"
            >
              <template v-if="section.id === 'summary'">
                <div class="html-content text-justify whitespace-pre-line w-full text-[#4a3728] flex flex-col" :style="{ fontSize: '14px', lineHeight: '1.7', paddingLeft: '20px' }" v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Tôi là một ứng viên năng động, mong muốn...')"></div>
              </template>

              <template v-else-if="section.id === 'contact'">
                <ul
                  v-if="contactItems.length > 0"
                  class="w-full list-none p-0 m-0 flex flex-col gap-[12px] relative contact-block"
                  :class="{ 'contact-active': selectedSectionId === 'contact' }"
                  @click.stop="selectedSectionId = selectedSectionId === 'contact' ? null : 'contact'"
                  :style="{ fontSize: '14px', lineHeight: '1.7', color: '#4a3728', paddingLeft: '20px' }"
                >
                  <li
                    v-for="(ci, ciIdx) in contactItems"
                    :key="ci.key"
                    class="paginated-item flex items-start break-words w-full relative contact-item-container"
                  >
                    <strong class="font-bold mr-1 shrink-0" :style="{ color: templateAccentColor }">{{ ci.label }}:</strong>
                    <span v-html="ci.value"></span>

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
              </template>

              <template v-else>
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full text-[#4a3728]" style="margin: 0 !important; padding: 0 0 0 20px !important;">
                      <button v-show="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button>
                      <CustomSectionItem v-if="(typeof section !== 'undefined' && section && section.isCustom) || (typeof id !== 'undefined' && typeof sec !== 'undefined' && sec(id)?.value?.isCustom) || (typeof block !== 'undefined' && block && block.isCustom)" :item="item" />
                      <template v-else>
                        <div class="absolute left-[0px] top-[6px] w-[6px] h-[6px] rounded-full" :style="{ backgroundColor: templatePrimaryColor }"></div>

                  <!-- KINH NGHIỆM / HỌC VẤN -->
                  <div v-if="['education','experience','project','activities'].includes(section.id)" class="w-full flex flex-col">
                    <div class="w-full flex justify-between items-baseline gap-2 relative" :style="{ margin: '0 !important' }">
                      <span class="font-bold break-words flex-1 leading-tight text-[#4a3728] flex flex-col" :style="{ margin: '0 !important', fontSize: '15px' }" v-html="formatDesc(section.id === 'education' ? item.school : (item.company || item.name))"></span>
                      <span v-if="item.year || item.time" class="paginated-item font-bold shrink-0 whitespace-nowrap text-right" :style="{ color: templateAccentColor, fontSize: '13px', margin: '0 !important' }"><span v-html="item.year || item.time"></span></span>
                    </div>
                    <div v-if="item.major || item.role" class="italic text-[#5d4e46] mt-[2px] mb-[5px] w-full flex flex-col" :style="{ fontSize: '14px !important' }" v-html="formatDesc(item.major || item.role)"></div>
                    <div v-if="item.gradType" class="paginated-item font-medium mt-[2px]" :style="{ color: templateAccentColor, fontSize: '13px !important', margin: '0 !important' }">Trạng thái: <span v-html="item.gradType"></span></div>
                    <div v-if="item.desc" class="html-content text-justify whitespace-pre-line break-words w-full flex flex-col" :style="{ margin: '0 !important', padding: '0 !important', fontSize: '14px', lineHeight: '1.7' }" v-html="formatDesc(item.desc)"></div>
                  </div>
                  <!-- KỸ NĂNG / TIN HỌC / NGOẠI NGỮ -->
<div v-else-if="['skills','languages','it_skills'].includes(section.id)" class="w-full flex flex-col">
  <div class="paginated-item w-full flex justify-between items-baseline gap-2 relative" :style="{ margin: '0 !important' }">
    <div class="absolute -left-[20px] top-[6px] w-[6px] h-[6px] rounded-full" :style="{ backgroundColor: templatePrimaryColor }"></div>
    <span class="font-bold break-words flex-1 leading-tight text-[#4a3728] flex flex-col" :style="{ fontSize: '15px' }" v-html="formatDesc(item.name)"></span>
    <!-- ĐÂY LÀ CHỖ HIỂN THỊ MỨC ĐỘ -->
    <span v-if="item.level" class="font-bold shrink-0 whitespace-nowrap text-right" :style="{ color: templateAccentColor, fontSize: '13px', margin: '0 !important' }"><span v-html="item.level"></span></span>
  </div>
  <div v-if="item.info" class="italic text-[#5d4e46] mt-[2px] mb-[5px] w-full flex flex-col" :style="{ fontSize: '14px !important' }" v-html="formatDesc(item.info)"></div>
</div>
                  
                  <!-- NGƯỜI THAM CHIẾU / CHỨNG CHỈ -->
                  <div v-else class="w-full flex flex-col">
                    <div class="w-full flex justify-between items-baseline gap-2 relative" :style="{ margin: '0 !important' }">
                      <span class="font-bold break-words flex-1 leading-tight text-[#4a3728] flex flex-col" :style="{ fontSize: '15px' }" v-html="formatDesc(item.name || item.title)"></span>
                      <span v-if="item.year || item.time" class="paginated-item font-bold shrink-0 whitespace-nowrap text-right" :style="{ color: templateAccentColor, fontSize: '13px', margin: '0 !important' }"><span v-html="item.year || item.time"></span></span>
                    </div>
                    <div v-if="item.info" class="italic text-[#5d4e46] mt-[2px] mb-[5px] w-full flex flex-col" :style="{ fontSize: '14px !important' }" v-html="formatDesc(item.info)"></div>
                    <div v-if="item.desc || item.details" class="html-content text-justify whitespace-pre-line break-words w-full flex flex-col" :style="{ margin: '0 !important', padding: '0 !important', fontSize: '14px', lineHeight: '1.7' }" v-html="formatDesc(item.desc || item.details)"></div>
                  </div>
                      </template>
</div>
              </template>
            </div>
          </div>
        </template>
        </draggable>
      </div>
    </main>

    <aside
      class="z-10 flex flex-col shrink-0 relative box-border text-white"
      :style="{
        width: '38%',
        backgroundColor: templateSidebarBgColor,
        padding: '40px 30px',
        margin: '20px 0 !important',
        height: 'calc(100% - 40px) !important',
        alignSelf: 'flex-start'
      }"
    >
      <div class="paginated-item relative z-20 w-full flex flex-col items-center mb-[40px]">
        <div class="relative rounded-full overflow-hidden mx-auto" :style="{ width: '180px', height: '180px', border: '8px solid rgba(255,255,255,0.1)' }">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="rounded-full w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center bg-white/10 text-white/50">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/></svg>
          </div>
        </div>
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
            :data-section-id="section.id" class="section-block relative w-full mb-[35px] cursor-pointer hover:bg-black/5 transition-colors"
            :class="{ 'section-active--sidebar': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <div v-show="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
              <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang trái (Cột chính)"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
              <button @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <h1 class="paginated-item uppercase w-full block" :style="{ fontSize: '16px !important', fontWeight: '700 !important', paddingBottom: '5px !important', margin: '0 0 15px 0 !important', color: '#fdf5e6' }">
              <span v-html="section.title || (section.id === 'summary' ? 'MỤC TIÊU NGHỀ NGHIỆP' : section.id === 'contact' ? 'THÔNG TIN LIÊN HỆ' : '')"></span>
            </h1>

            <div class="w-full flex flex-col gap-[15px]">
              <template v-if="section.id === 'summary'">
                <div class="html-content-sidebar text-justify whitespace-pre-line w-full text-[#e8e8e8] flex flex-col" :style="{ fontSize: '13px !important', lineHeight: '1.6 !important', margin: '0 !important' }" v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Tôi là một ứng viên năng động, mong muốn...')"></div>
              </template>

              <template v-else-if="section.id === 'contact'">
                <ul
                  v-if="contactItems.length > 0"
                  class="w-full list-none p-0 m-0 flex flex-col gap-[12px] relative contact-block"
                  :class="{ 'contact-active-sidebar': selectedSectionId === 'contact' }"
                  @click.stop="selectedSectionId = selectedSectionId === 'contact' ? null : 'contact'"
                  :style="{ fontSize: '13px !important', lineHeight: '1.6', color: '#e8e8e8' }"
                >
                  <li
                    v-for="(ci, ciIdx) in contactItems"
                    :key="ci.key"
                    class="paginated-item flex items-start break-words w-full relative contact-item-container"
                  >
                    <strong class="font-bold mr-1 shrink-0 text-[#fdf5e6]">{{ ci.label }}:</strong>
                    <span v-html="ci.value"></span>

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
              </template>

              <template v-else>
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full text-[#e8e8e8]" :style="{ fontSize: '13px !important', margin: '0 !important', padding: '0 0 0 15px !important' }">
                      <button v-show="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button>
                      <CustomSectionItem v-if="(typeof section !== 'undefined' && section && section.isCustom) || (typeof id !== 'undefined' && typeof sec !== 'undefined' && sec(id)?.value?.isCustom) || (typeof block !== 'undefined' && block && block.isCustom)" :item="item" />
                      <template v-else>
                        <div v-if="['skills','languages','it_skills'].includes(section.id)" class="paginated-item w-full flex flex-col skill-group-extra relative">
                    <div class="absolute -left-[15px] top-[6px] w-[4px] h-[4px] rounded-full bg-white"></div>
                    <div class="extra-label w-full uppercase" :style="{ fontSize: '12px !important', fontWeight: 'bold !important', color: '#fdf5e6', marginBottom: '5px' }"><span v-html="item.name"></span></div>
                    <div v-if="item.info || item.level" class="w-full break-words whitespace-pre-line" :style="{ fontSize: '12px !important', color: '#ddd', lineHeight: '1.4', paddingLeft: '5px' }"><span v-html="item.info || item.level"></span></div>
                  </div>
                  
                  <div v-else class="w-full flex flex-col">
                    <div class="absolute left-[0px] top-[6px] w-[4px] h-[4px] rounded-full bg-white"></div>

                    <div class="w-full flex justify-between items-baseline gap-2 relative" :style="{ margin: '0 !important' }">
                      <span class="font-bold w-full break-words leading-tight text-[#fdf5e6] flex flex-col" v-html="formatDesc(section.id === 'education' ? item.school : (item.company || item.name))"></span>
                      <span v-if="item.year || item.time" class="paginated-item font-bold shrink-0 whitespace-nowrap text-right opacity-90 text-[11px]"><span v-html="item.year || item.time"></span></span>
                    </div>
                    <div v-if="item.major || item.role" class="italic opacity-90 mt-[2px] text-[12px] flex flex-col" v-html="formatDesc(item.major || item.role)"></div>
                    <div v-if="item.gradType" class="paginated-item font-medium mt-[2px] opacity-90" :style="{ fontSize: '12px !important' }">Xếp loại: <span v-html="item.gradType"></span></div>
                    <div v-if="item.desc || item.info" class="html-content-sidebar text-justify whitespace-pre-line break-words w-full mt-[5px] flex flex-col" :style="{ lineHeight: '1.6' }" v-html="formatDesc(item.desc || item.info)"></div>
                  </div>
                      </template>
</div>
              </template>
            </div>
          </div>
        </template>
        </draggable>
      </div>
    </aside>

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
import draggable from 'vuedraggable'

const cvRoot = ref(null)
const pageCount = ref(1)
const selectedSectionId = ref(null)

const props = defineProps({ resumeData: { type: Object, required: true } })
const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

const isEmpty = (val) => {
  if (!val) return true
  if (typeof val !== 'string') return false
  return val.replace(/<[^>]*>/g, '').trim() === ''
}

// ─── CONTACT ITEMS LOGIC ───
const contactLabels = {
  phone: 'Di động',
  email: 'Email',
  dob: 'Ngày sinh',
  address: 'Địa chỉ',
  website: 'Website',
  linkedin: 'Linkedin',
  github: 'Github'
}

const contactOrder = ref(['phone', 'email', 'dob', 'address', 'website', 'linkedin', 'github'])
const hiddenContacts = ref([])

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  switch (key) {
    case 'phone': return g.phone || ''
    case 'email': return g.email || ''
    case 'dob': return g.dob || g.birthDate || ''
    case 'address': return g.address || ''
    case 'website': return g.website || ''
    case 'linkedin': return g.linkedin || ''
    case 'github': return g.github || ''
    default: return ''
  }
}

const contactItems = computed(() => {
  return contactOrder.value
    .filter(key => !hiddenContacts.value.includes(key))
    .filter(key => !isEmpty(getContactValue(key)))
    .map(key => ({
      key,
      label: contactLabels[key],
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

// BỘ LỌC CHUẨN: Tự động băm nhỏ lá và tạo block riêng rẽ
const formatDesc = (text) => {
  if (!text) return ''
  if (/<[a-z][\s\S]*>/i.test(text)) {
    if (text.includes('<li') && (text.includes('<font') || text.includes('style='))) {
      try {
        const div = document.createElement('div')
        div.innerHTML = text
        div.querySelectorAll('li').forEach(li => {
          const child = li.firstElementChild
          if (child && (child.tagName === 'FONT' || child.tagName === 'SPAN')) {
            if (child.color) li.style.color = child.color
            if (child.style?.color) li.style.color = child.style.color
          }
        })
        text = div.innerHTML
      } catch { /* ignore */ }
    }
  } else {
    text = text.split('\n').map(l => l.trim()).filter(Boolean).join('<br/>')
  }

  const tempDiv = document.createElement('div')
  tempDiv.innerHTML = text

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

// ─── THUẬT TOÁN ĐO CHÍNH XÁC (PHIÊN BẢN BĂM NHỎ TỪNG DÒNG LÁ) ─────────────
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

  // Chỉ lấy những phần tử lá mang thẻ paginated-item (như cv 1-12)
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
  const topMargin = 16 * pxPerMm;

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

      // Chặn nếu có thẻ khổng lồ lọt vào
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
  if (props.resumeData && props.resumeData.sections) {
    const secs = props.resumeData.sections

    let summaryAdded = false
    if (!secs.some(s => s.id === 'summary')) {
      secs.push({ id: 'summary', isVisible: true, column: 'right', title: 'MỤC TIÊU NGHỀ NGHIỆP' })
      summaryAdded = true
    }

    let contactAdded = false
    if (!secs.some(s => s.id === 'contact')) {
      secs.push({ id: 'contact', isVisible: true, column: 'right', title: 'THÔNG TIN LIÊN HỆ' })
      contactAdded = true
    }

    secs.forEach(sec => {
      if (['experience', 'education', 'certificates', 'hobbies', 'references'].includes(sec.id)) {
        if(sec.isVisible === undefined) sec.isVisible = true
        if(!sec.column) sec.column = 'left'
      }
      else if (['awards', 'skills', 'summary', 'contact'].includes(sec.id)) {
        if(sec.isVisible === undefined) sec.isVisible = true
        if(!sec.column) sec.column = 'right'
      }
      else if (['project', 'activities', 'languages', 'it_skills'].includes(sec.id)) {
        if(sec.isVisible === undefined) sec.isVisible = false
        if(!sec.column) sec.column = 'left'
      }
    })

    if (summaryAdded) {
      const sIdx = secs.findIndex(s => s.id === 'summary')
      const [sumItem] = secs.splice(sIdx, 1)
      secs.unshift(sumItem)
    }

    if (contactAdded) {
      const cIdx = secs.findIndex(s => s.id === 'contact')
      const [contactItem] = secs.splice(cIdx, 1)
      
      let rightCount = 0
      let insertAt = secs.length
      for (let i = 0; i < secs.length; i++) {
        if (secs[i].column === 'right') {
          rightCount++
          if (rightCount === 2) {
            insertAt = i + 1
            break
          }
        }
      }
      secs.splice(insertAt, 0, contactItem)
    }
  }

  requestPagination()
  window.addEventListener('resize', requestPagination)
  document.addEventListener('keyup', requestPagination)
})

onUnmounted(() => {
  window.removeEventListener('resize', requestPagination)
  if (cvRoot._observer) cvRoot._observer.disconnect()
  if (paginateTimer) clearTimeout(paginateTimer)
})

watch(() => props.resumeData, requestPagination, { deep: true })

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
  if (!c || c.toLowerCase() === '#2b5c8f') return '#5d4e46'
  return c
})

const isCustomColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  return c && c.toLowerCase() !== '#2b5c8f'
})

const templateAccentColor = computed(() => {
  if (isCustomColor.value) return templatePrimaryColor.value
  return '#8b7355'
})

const templateSidebarBgColor = computed(() => {
  if (isCustomColor.value) return templatePrimaryColor.value
  return '#5d4e46'
})

const templatePageBgColor = computed(() => {
  if (isCustomColor.value) return adjustColorBrightness(templatePrimaryColor.value, 0.94)
  return '#f3ebdd'
})

const mainSections = computed(() => (props.resumeData?.sections || []).filter(s => s.column === 'left'))
const sidebarSections = computed(() => (props.resumeData?.sections || []).filter(s => s.column === 'right'))

const mainSectionsWritable = ref([])
const sidebarSectionsWritable = ref([])

watch(mainSections, (newVal) => {
  mainSectionsWritable.value = [...newVal]
}, { immediate: true, deep: true })

watch(sidebarSections, (newVal) => {
  sidebarSectionsWritable.value = [...newVal]
}, { immediate: true, deep: true })

const onDragEnd = () => {
  mainSectionsWritable.value.forEach(s => {
    const item = props.resumeData.sections.find(x => x.id === s.id)
    if (item) item.column = 'left'
  })
  sidebarSectionsWritable.value.forEach(s => {
    const item = props.resumeData.sections.find(x => x.id === s.id)
    if (item) item.column = 'right'
  })

  const newOrderIds = [
    ...mainSectionsWritable.value.map(s => s.id),
    ...sidebarSectionsWritable.value.map(s => s.id)
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

const mainIds = computed(() => mainSections.value.map(s => s.id))
const sidebarIds = computed(() => sidebarSections.value.map(s => s.id))
</script>

<style scoped>
#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

.item-container { position: relative; }

/* ── Contact block ─────────────────────────────────────────────────────── */
.contact-block {
  cursor: pointer;
  border: 1px solid transparent;
  transition: background 0.2s, border-color 0.2s;
  border-radius: 4px;
  padding: 8px !important; /* Constant padding to prevent shift */
}
.contact-active {
  border: 1px solid rgba(139, 115, 85, 0.4) !important;
  background: rgba(139, 115, 85, 0.1);
}
.contact-active-sidebar {
  border: 1px solid rgba(255, 255, 255, 0.3) !important;
  background: rgba(255, 255, 255, 0.1);
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

.delete-btn {
  position: absolute;
  right: 20px;
  top: 5px;
  width: 18px;
  height: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #ef4444;
  color: white;
  border: none;
  border-radius: 50%;
  cursor: pointer;
  box-shadow: 0 2px 6px rgba(0,0,0,0.25);
  z-index: 30;
  transition: transform 0.12s ease;
}
.delete-btn:hover { transform: scale(1.15) !important; }
.delete-btn:active { transform: scale(0.9) !important; }

.section-block {
  position: relative;
  border: 2px solid transparent !important;
  border-radius: 0 !important;
  cursor: pointer;
  transition:
    box-shadow 0.2s ease,
    border-color 0.15s ease,
    border-radius 0.15s ease,
    background 0.15s ease;
}

.section-active--main {
  border: 2px solid v-bind(templatePageBgColor) !important;
  border-style: solid !important;
  border-radius: 6px !important;
  /* removed scale */
  box-shadow: 0 6px 24px rgba(93,78,70,0.18), 0 1px 4px rgba(93,78,70,0.08) !important;
  background: rgba(255,255,255,0.28) !important;
  z-index: 10 !important;
}

.section-active--sidebar {
  border: 2px solid v-bind(templatePrimaryColor) !important;
  border-style: solid !important;
  border-radius: 6px !important;
  /* removed scale */
  box-shadow: 0 6px 24px rgba(0,0,0,0.3), 0 1px 4px rgba(0,0,0,0.12) !important;
  background: rgba(255,255,255,0.06) !important;
  z-index: 10 !important;
}

.nav-btns {
  position: absolute;
  right: 10px;
  top: 10px;
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
  background: v-bind(templatePrimaryColor);
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.15);
  transition: background 0.12s, transform 0.1s, filter 0.12s;
}
.nav-btn:hover { filter: brightness(0.85); }
.nav-btn:active { transform: scale(0.92) !important; }

.nav-btn-danger {
  background: #ef4444 !important;
  box-shadow: 0 2px 6px rgba(239, 68, 68, 0.4) !important;
}
.nav-btn-danger:hover {
  background: #dc2626 !important;
}

/* CSS hỗ trợ băm lá mượt mà */
:deep(.html-content) { margin: 0 !important; padding: 0 !important; }
:deep(.html-content p) { margin: 0 !important; padding: 0 !important; }
:deep(.html-content ul) { list-style-type: disc !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content ol) { list-style-type: decimal !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content b), :deep(.html-content strong) { font-weight: bold; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic; }
:deep(.html-content u) { text-decoration: underline; }
:deep(.html-content ul li), :deep(.html-content ol li) { margin-bottom: 2px !important; }

:deep(.html-content-sidebar) { margin: 0 !important; padding: 0 !important; color: #e8e8e8 !important; }
:deep(.html-content-sidebar p) { margin: 0 !important; padding: 0 !important; }
:deep(.html-content-sidebar ul) { list-style-type: disc !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content-sidebar ol) { list-style-type: decimal !important; padding-left: 1.25rem !important; margin: 0 !important; }
:deep(.html-content-sidebar b), :deep(.html-content-sidebar strong) { font-weight: bold; color: #fdf5e6 !important; }
:deep(.html-content-sidebar i), :deep(.html-content-sidebar em) { font-style: italic; opacity: 0.9; }
:deep(.html-content-sidebar u) { text-decoration: underline; }
:deep(.html-content-sidebar ul li),
:deep(.html-content-sidebar ol li) { margin-bottom: 2px !important; }

:deep(.html-content li:has(> font[size="1"])),
:deep(.html-content-sidebar li:has(> font[size="1"])) { font-size: 10px; }
:deep(.html-content li:has(> font[size="2"])),
:deep(.html-content-sidebar li:has(> font[size="2"])) { font-size: 11px; }
:deep(.html-content li:has(> font[size="3"])),
:deep(.html-content-sidebar li:has(> font[size="3"])) { font-size: 13px; }
:deep(.html-content li:has(> font[size="4"])),
:deep(.html-content-sidebar li:has(> font[size="4"])) { font-size: 15px; }

/* Cố định phần Animation để không bị lỗi đo kích thước */
.paginated-item {
  transition: none !important;
}

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
</style>
