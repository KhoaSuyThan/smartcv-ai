<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-row relative box-border text-[#333] leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif' }"
    @click.self="selectedSectionId = null"
  >
    <!-- LEFT COLUMN -->
    <aside class="w-[68mm] z-10 flex flex-col pt-0 shrink-0 relative bg-white">

      <!-- Header Trái: Avatar + Liên hệ -->
      <div
        class="pt-[15mm] px-[8mm] flex flex-col paginated-item relative z-20 items-center"
        :style="{ backgroundColor: templateSecondaryColor }"
      >
        <!-- Avatar -->
        <div class="relative w-[45mm] h-[45mm] rounded-full overflow-hidden mb-6 mx-auto bg-transparent border-[1.5px] border-slate-300">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center bg-gray-200 text-gray-500">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
          </div>
        </div>

        <!-- Contact Info (dynamic, reorderable) -->
        <div
          v-if="contactItems.length > 0"
          class="flex flex-col w-full font-medium text-[#333] relative contact-block"
          style="font-size: 11px !important"
          :class="{ 'contact-active': selectedSectionId === 'contact' }"
          @click.stop="toggleSection('contact')"
        >
          <div
            v-for="(ci, ciIdx) in contactItems"
            :key="ci.key"
            class="w-full py-2 relative contact-item-container"
            :class="{ 'pb-6': ciIdx === contactItems.length - 1 }"
          >
            <div class="flex items-center gap-3.5">
              <div class="w-7 h-7 rounded-full border-[1.5px] border-slate-600 flex items-center justify-center shrink-0">
                <svg class="w-3.5 h-3.5 text-slate-800" fill="currentColor" viewBox="0 0 20 20" v-html="ci.icon"></svg>
              </div>
              <span class="break-all" v-html="ci.value"></span>
            </div>

            <!-- Move Up / Move Down / Delete buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print">
                <button v-if="ciIdx > 0" @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn" title="Di chuyển lên" style="padding:3px">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                </button>
                <button v-if="ciIdx < contactItems.length - 1" @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn" title="Di chuyển xuống" style="padding:3px">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                </button>
                <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn-danger" title="Ẩn mục này" style="padding:3px">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </transition>
          </div>
        </div>
      </div>

      <!-- Sidebar Sections -->
      <div class="w-full pl-[8mm] pr-0 flex-1 pb-8 flex flex-col mt-4">
        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-if="section.isVisible"
            class="section-block relative group mb-1"
            :class="{ 'section-active': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--active-bg': templateSecondaryColor } : {}"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
                <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên">
                  <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                </button>
                <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống">
                  <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                </button>
                <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải">
                  <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
                </button>
                <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                  <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </transition>

            <div class="paginated-item">
              <h3 class="section-title font-bold uppercase mb-1 tracking-wider flex items-center gap-2">
                {{ section.title }}
              </h3>
              <div class="w-full border-b-[1.5px] border-[#333]/30 mb-2"></div>
            </div>

            <div class="space-y-4" v-if="sectionHasContent(section)">
              <!-- Skills / Languages / IT Skills -->
              <div v-if="['skills'].includes(section.id)" class="space-y-2">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="paginated-item text-[#333] item-container font-medium leading-relaxed flex items-start gap-2"
                  style="font-size: 11px !important"
                >
                  <span class="mt-[0.5px] text-slate-500 shrink-0">
                    <svg width="10" height="10" viewBox="0 0 24 24" fill="currentColor" stroke="none"><path d="M12 2l2 7 7 2-7 2-2 7-2-7-7-2 7-2z"/></svg>
                  </span>
                  <span class="flex-1 text-justify">
                    <span class="text-[#333] font-bold">{{ item.name }}:</span>
                    <span v-if="item.info" class="font-normal"> {{ item.info }}</span>
                    <span v-else-if="item.level" class="font-normal"> ({{ item.level }})</span>
                  </span>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- Awards / Certifications -->
              <div v-else-if="['awards','certifications'].includes(section.id)" class="space-y-3">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="paginated-item item-container leading-relaxed text-[#333] relative"
                  style="font-size: 11px !important"
                >
                  <p v-if="item.year" class="font-bold text-[#333] mb-0.5">{{ item.year }}</p>
                  <p class="font-normal html-content">{{ item.name || item.info }}</p>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- Hobbies -->
              <div v-else-if="section.id === 'hobbies'" class="space-y-1">
                <div class="flex flex-wrap gap-1 text-[#333] font-normal leading-relaxed" style="font-size: 11px !important">
                  <span
                    v-for="(item, itemIndex) in section.items"
                    :key="item._refId"
                    class="paginated-item item-container relative"
                  >
                    {{ item.name }}<span v-if="itemIndex < section.items.length - 1">, </span>
                    <transition name="fade-btns">
                      <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: -5px; right: -5px; transform: scale(0.8)">
                        <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                      </button>
                    </transition>
                  </span>
                </div>
              </div>

              <!-- Other sidebar sections -->
              <div v-else class="space-y-3">
              <div
                v-for="(item, itemIndex) in section.items"
                :key="item._refId"
                class="text-[#333] font-normal leading-relaxed item-container relative"
                style="font-size: 11px !important"
              >

                <div v-if="item.time || item.year" 
                  class="paginated-item font-bold mb-0.5"
                  :style="{ color: templatePrimaryColor, fontSize: '10px !important' }">
                  {{ item.time || item.year }}
                </div>

                <div v-if="item.company || item.school || item.name || item.title"
                  class="paginated-item font-bold mb-0.5"
                  style="font-size: 11.5px !important; color: #333;">
                  {{ item.company || item.school || item.name || item.title }}
                </div>

                <!-- ✅ Ngành học: hiện riêng bên dưới tên trường -->
                <div v-if="item.major"
                  class="paginated-item font-normal mb-0.5"
                  style="font-size: 11px !important; color: #555;">
                  {{ item.major }}
                </div>

                <!-- Vai trò: chỉ hiện khi có tên ở trên (giữ nguyên logic cũ nhưng thêm school) -->
                <div v-if="(item.company || item.school || item.name || item.title) && item.role"
                  class="paginated-item font-normal mb-1"
                  style="font-size: 11px !important; color: #555;">
                  {{ item.role }}
                </div>

                <!-- ✅ Xếp loại / GPA: thêm mới -->
                <div v-if="item.gradType || item.gpa"
                  class="paginated-item font-normal mb-1"
                  style="font-size: 11px !important; color: #555;">
                  <span v-if="item.gradType">Tốt nghiệp loại: <strong>{{ item.gradType }}</strong></span>
                  <span v-if="item.gradType && item.gpa"> | </span>
                  <span v-if="item.gpa">GPA: <strong>{{ item.gpa }}</strong></span>
                </div>

                <!-- Mô tả (giữ nguyên) -->
                <div class="html-content" v-html="formatDesc(item.desc || item.info)"></div>

                <transition name="fade-btns">
                  <button v-if="selectedSectionId === section.id" 
                    @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" 
                    class="delete-item-btn no-print">
                    <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                    </svg>
                  </button>
                </transition>
              </div>
            </div>
            </div>
          </div>
        </template>
      </div>
    </aside>

    <!-- RIGHT COLUMN -->
    <main class="flex-1 flex flex-col relative bg-white z-20 min-h-max" @click.self="selectedSectionId = null">

      <!-- Header: Tên + Nghề nghiệp + Summary -->
      <header
        class="paginated-item pt-[12mm] px-[12mm] pb-[8mm] flex flex-col min-h-min"
        :style="{ backgroundColor: templatePrimaryColor, color: 'white' }"
      >
        <h1 class="font-bold uppercase tracking-wide mb-1 leading-tight" style="font-size: 26px !important" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'HỌ VÀ TÊN ỨNG VIÊN'"></h1>
        <div class="border-b border-white pb-3 w-fit pr-12 mb-4">
          <h2 class="font-medium uppercase tracking-[0.08em] opacity-90" style="font-size: 13.5px !important" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'VỊ TRÍ ỨNG TUYỂN'"></h2>
        </div>
        <div class="leading-relaxed text-justify html-content font-medium opacity-90 mt-1" style="font-size: 11px !important" v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Mục tiêu nghề nghiệp...'"></div>
      </header>

      <!-- Main Sections -->
      <div class="px-[12mm] pt-[8mm] pb-[8mm] flex-1 flex flex-col gap-[3.5mm]">
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-if="section.isVisible"
            class="section-block relative group my-0"
            :class="{ 'section-active': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--active-bg': templatePrimaryColor } : {}"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
                <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                </button>
                <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                </button>
                <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
                </button>
                <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </transition>

            <div class="paginated-item">
              <h3 class="section-title font-bold uppercase mb-1 tracking-wide flex items-center gap-2">
                {{ section.title }}
              </h3>
              <div class="w-full border-b-[1.5px] border-[#333]/30 mb-2"></div>
            </div>

            <div class="space-y-3" v-if="sectionHasContent(section)">
              <!-- Education -->
              <div v-if="section.id === 'education'" class="space-y-3">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                  <div class="paginated-item">
                    <div class="flex justify-between items-start gap-4 mb-1">
                      <h4 class="text-[#333] align-middle mt-[2px] font-bold" style="font-size: 11.5px !important">
                        <span v-html="item.school"></span>
                      </h4>
                      <span v-if="item.year" class="font-bold text-white px-[8px] py-[2px] rounded-full shrink-0 leading-none" :style="{ backgroundColor: templatePrimaryColor, fontSize: '9px !important' }">{{ item.year }}</span>
                    </div>
                    <div v-if="item.major" class="text-[#333] font-normal html-content mb-1" style="font-size: 11px !important">{{ item.major }}</div>
                  </div>
                  
                  <div class="text-[#333] leading-relaxed html-content" style="font-size: 11px !important" v-html="formatDesc(item.desc)"></div>
                  
                  <div v-if="!item.desc && item.gradType" class="paginated-item text-[#333] font-bold mt-1" style="font-size: 11px !important">
                    Tốt nghiệp loại: <span class="font-normal">{{ item.gradType }}</span>
                  </div>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- Experience / Project / Activities -->
              <div v-else-if="['experience','project','activities'].includes(section.id)" class="space-y-3">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                  <div class="paginated-item">
                    <div class="flex justify-between items-start gap-4 mb-1.5">
                      <h4 class="text-[#333] uppercase leading-snug mt-[1px] font-bold" style="font-size: 11.5px !important">
                        <span v-html="section.id === 'experience' ? item.company : (item.name || '')"></span>
                      </h4>
                      <span v-if="item.time" class="font-bold text-white px-[8px] py-[2px] rounded-full shrink-0 leading-none" :style="{ backgroundColor: templatePrimaryColor, fontSize: '9px !important' }">{{ item.time }}</span>
                    </div>
                    <div v-if="item.role" class="font-normal text-[#333] mb-1.5" style="font-size: 11px !important">{{ item.role }}</div>
                  </div>
                  
                  <div class="leading-relaxed text-[#333] text-justify html-content" style="font-size: 11px !important" v-html="formatDesc(item.desc)"></div>

                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- Other main sections -->
              <div v-else class="space-y-4">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="item-container relative text-[#333] leading-relaxed"
                  style="font-size: 11px !important"
                >
                <div v-if="['awards','certifications'].includes(section.id)" class="paginated-item">
                  <p v-if="item.year" class="font-bold text-[#333] mb-0.5">{{ item.year }}</p>
                  <p class="font-normal">{{ item.name || item.info }}</p>
                </div>

                <div v-else-if="['skills','languages','it_skills'].includes(section.id)"
                  class="paginated-item flex items-start gap-2">
                  <span class="mt-[0.5px] text-slate-500 shrink-0">
                    <svg width="10" height="10" viewBox="0 0 24 24" fill="currentColor" stroke="none">
                      <path d="M12 2l2 7 7 2-7 2-2 7-2-7-7-2 7-2z"/>
                    </svg>
                  </span>
                  <span class="flex-1">
                    <span class="font-bold">{{ item.name }}:</span>
                    <span v-if="item.info" class="font-normal"> {{ item.info }}</span>
                    <span v-else-if="item.level" class="font-normal"> ({{ item.level }})</span>
                  </span>
                </div>
                
                  <div v-else class="html-content" v-html="formatDesc(item.desc || item.info || item.name)"></div>

                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>
            </div>
          </div>
        </template>
      </div>
    </main>

    <!-- VIỀN CUỐI TRANG CỐ ĐỊNH -->
    <template v-for="p in pageCount" :key="'footer-border-' + p">
      <div
        class="absolute left-0 w-full flex items-center z-40 pointer-events-none"
        :style="{ top: `calc(${p * 297}mm - 12mm)`, height: '1.5px', paddingLeft: '12mm', paddingRight: '12mm' }"
      >
        <div class="w-full h-full opacity-30 bg-gradient-to-r from-slate-400 via-slate-500 to-slate-400"></div>
      </div>
    </template>

    <!-- ĐƯỜNG PHÂN TRANG -->
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
import { computed, ref, reactive, onMounted, nextTick, watch, onUnmounted } from 'vue'

const cvRoot = ref(null)
const pageCount = ref(1)
const selectedSectionId = ref(null)

const props = defineProps({
  resumeData: { type: Object, required: true }
})
const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

const toggleSection = (id) => {
  selectedSectionId.value = selectedSectionId.value === id ? null : id
}

// ─── CONTACT ITEMS: Danh sách động có thể sắp xếp / ẩn ───
const contactIcons = {
  phone: '<path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z" />',
  email: '<path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z" /><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z" />',
  web: '<path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM4.332 8.027a6.012 6.012 0 011.912-2.706C6.512 5.73 6.974 6 7.5 6A1.5 1.5 0 019 7.5V8a2 2 0 004 0 2 2 0 011.523-1.943A5.977 5.977 0 0116 10c0 .34-.028.675-.083 1H15a2 2 0 00-2 2v2.197A5.973 5.973 0 0110 16v-2a2 2 0 00-2-2 2 2 0 01-2-2 2 2 0 00-1.668-1.973z" clip-rule="evenodd" />',
  address: '<path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd" />'
}

const contactOrder = ref(['phone', 'email', 'web', 'address'])
const hiddenContacts = ref([])

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  switch (key) {
    case 'phone': return isEmpty(g.phone) ? '0123.456.789' : g.phone
    case 'email': return isEmpty(g.email) ? 'email@example.com' : g.email
    case 'web':
      if (!isEmpty(g.github)) return g.github
      if (!isEmpty(g.linkedin)) return g.linkedin
      if (!isEmpty(g.website)) return g.website
      return 'https://github.com/khoa'
    case 'address': return isEmpty(g.address) ? 'TP. Hồ Chí Minh' : g.address
    default: return ''
  }
}

const contactItems = computed(() => {
  return contactOrder.value
    .filter(key => !hiddenContacts.value.includes(key))
    .map(key => ({
      key,
      icon: contactIcons[key],
      value: getContactValue(key)
    }))
})

const moveContactUp = (idx) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k))
  if (idx <= 0) return
  const keyA = visible[idx]
  const keyB = visible[idx - 1]
  const idxA = contactOrder.value.indexOf(keyA)
  const idxB = contactOrder.value.indexOf(keyB)
  const arr = [...contactOrder.value]
  ;[arr[idxA], arr[idxB]] = [arr[idxB], arr[idxA]]
  contactOrder.value = arr
}

const moveContactDown = (idx) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k))
  if (idx >= visible.length - 1) return
  const keyA = visible[idx]
  const keyB = visible[idx + 1]
  const idxA = contactOrder.value.indexOf(keyA)
  const idxB = contactOrder.value.indexOf(keyB)
  const arr = [...contactOrder.value]
  ;[arr[idxA], arr[idxB]] = [arr[idxB], arr[idxA]]
  contactOrder.value = arr
}

const removeContactItem = (idx) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k))
  if (idx >= 0 && idx < visible.length) {
    hiddenContacts.value.push(visible[idx])
  }
}

const handleOutsideClick = (e) => {
  if (cvRoot.value && !cvRoot.value.contains(e.target)) {
    selectedSectionId.value = null
  }
}

const isEmpty = (val) => {
  if (!val) return true
  if (typeof val !== 'string') return false
  return val.replace(/<[^>]*>/g, '').trim() === ''
}

const sectionHasContent = (section) => {
  if (!section) return false
  return section.items && section.items.length > 0
}

const formatDesc = (text) => {
  if (!text) return ''
  
  if (!/<[a-z][\s\S]*>/i.test(text)) {
     return text.split('\n')
                .map(l => l.trim())
                .filter(Boolean)
                .map(l => `<div class="paginated-item">${l}</div>`)
                .join('')
  }

  const tempDiv = document.createElement('div')
  tempDiv.innerHTML = text
  
  tempDiv.querySelectorAll('li').forEach(li => {
    const child = li.firstElementChild
    if (child && (child.tagName === 'FONT' || child.tagName === 'SPAN')) {
      if (child.color) li.style.color = child.color
      if (child.style && child.style.color) li.style.color = child.style.color
    }
  })

  const container = document.createElement('div')
  Array.from(tempDiv.childNodes).forEach(node => {
    if (node.nodeType === Node.TEXT_NODE) {
      if (node.textContent.trim()) {
        const div = document.createElement('div')
        div.className = 'paginated-item min-h-[14px]'
        div.appendChild(node.cloneNode(true))
        container.appendChild(div)
      }
    } else if (node.nodeType === Node.ELEMENT_NODE) {
      if (node.tagName === 'BR') {
        const div = document.createElement('div')
        div.className = 'paginated-item h-[14px]'
        container.appendChild(div)
      } else if (['UL', 'OL'].includes(node.tagName)) {
        Array.from(node.children).forEach(li => li.classList.add('paginated-item'))
        container.appendChild(node.cloneNode(true))
      } else {
        node.classList.add('paginated-item')
        container.appendChild(node.cloneNode(true))
      }
    }
  })

  return container.innerHTML
}

const templatePrimaryColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  if (!c || c.toLowerCase() === '#2b5c8f') return '#556050'
  return c
})
const templateSecondaryColor = computed(() => '#e8e4db')

const sidebarSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'left' && !['summary', 'it_skills', 'languages', 'education'].includes(s.id))
)
const mainSections = computed(() => {
  const sections = props.resumeData.sections.filter(s => 
    (s.column === 'right' || s.id === 'education') && 
    !['summary', 'it_skills', 'languages'].includes(s.id)
  )
  
  // Sắp xếp: education nằm ngay dưới experience
  const expIndex = sections.findIndex(s => s.id === 'experience')
  const eduIndex = sections.findIndex(s => s.id === 'education')
  if (expIndex !== -1 && eduIndex !== -1) {
    const [edu] = sections.splice(eduIndex, 1)
    const newExpIndex = sections.findIndex(s => s.id === 'experience')
    sections.splice(newExpIndex + 1, 0, edu)
  }
  return sections
})
const sidebarIds = computed(() => sidebarSections.value.map(s => s.id))
const mainIds = computed(() => mainSections.value.map(s => s.id))

// ─── PAGINATION ENGINE: ĐÃ CĂN CHỈNH KHOẢNG TRẮNG BẰNG ĐƯỜNG KẺ MỜ ───
const A4_W_MM   = 210
const A4_H_MM   = 297

let paginateTimer = null
const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 60)
}

const doPagination = async () => {
  if (!cvRoot.value) return

  const allElements = cvRoot.value.querySelectorAll('.paginated-item')
  allElements.forEach(el => { el.style.marginTop = '0px' })
  await nextTick()

  const pxPerMm = cvRoot.value.offsetWidth / A4_W_MM
  const pageH = A4_H_MM * pxPerMm
  
  // Vùng an toàn đáy trang: 14mm. Đường kẻ mờ của bạn đang ở mức 12mm.
  // -> Chữ sẽ cách mép dưới 14mm (dừng lại vừa vặn ngay trên đường kẻ mờ 2mm).
  const bottomSafeZone = 14 * pxPerMm 
  // Lề trên trang mới: 8mm.
  const topMargin = 8 * pxPerMm 

  const getOffsetTop = (el) => {
    let offset = 0
    let curr = el
    while (curr && curr !== cvRoot.value) {
      offset += curr.offsetTop
      curr = curr.offsetParent
    }
    return offset
  }

  allElements.forEach((el) => {
    if(el.offsetHeight === 0) return
    const top = getOffsetTop(el)
    const topInPage = top % pageH
    const bottomInPage = topInPage + el.offsetHeight
    
    // Nếu ĐÁY CỦA DÒNG đè vào vùng an toàn 14mm cuối trang
    if (bottomInPage > (pageH - bottomSafeZone)) {
       // Đẩy qua vạch đen sang trang mới + lề trên 8mm
       const distToNextPage = pageH - topInPage + topMargin
       el.style.marginTop = `${distToNextPage}px`
    }
  })

  let maxBottom = 0
  allElements.forEach(el => {
    const b = getOffsetTop(el) + el.offsetHeight
    if (b > maxBottom) maxBottom = b
  })
  pageCount.value = Math.max(1, Math.ceil(maxBottom / pageH))
}

watch(() => props.resumeData, requestPagination, { deep: true })

onMounted(() => {
  const defaultVisible = ['education', 'experience', 'activities', 'project', 'skills', 'certifications', 'awards', 'hobbies']
  props.resumeData?.sections?.forEach(sec => {
    if (defaultVisible.includes(sec.id) && !sec.isVisible) sec.isVisible = true
  })

  requestPagination()
  window.addEventListener('resize', requestPagination)
  document.addEventListener('keyup', requestPagination)
  document.addEventListener('click', handleOutsideClick)
})

onUnmounted(() => {
  window.removeEventListener('resize', requestPagination)
  document.removeEventListener('keyup', requestPagination)
  document.removeEventListener('click', handleOutsideClick)
  if (paginateTimer) clearTimeout(paginateTimer)
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap');

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

.section-title {
  font-size: 16px !important;
  color: #000000 !important;
  font-weight: bold !important;
}

.section-block {
  position: relative;
  border-radius: 6px;
  border: 2px solid transparent;
  cursor: pointer;
  transition: transform 0.18s ease, box-shadow 0.18s ease, border-color 0.18s ease;
  padding: 10px;
}

.section-block.section-active {
  /* removed scale */
  border-radius: 6px !important;
  border: 2px solid var(--active-bg, #556050) !important;
  box-shadow: 0 4px 18px rgba(0,0,0,0.10);
  z-index: 10;
}

.nav-btns {
  position: absolute;
  right: 6px;
  top: 6px;
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
  transition: background 0.15s, transform 0.15s;
}
.nav-btn:hover { background: #1d4ed8; transform: scale(1.1); }
.nav-btn:active { transform: scale(0.95); }

.nav-btn-danger {
  background: #ef4444 !important;
  box-shadow: 0 2px 6px rgba(239, 68, 68, 0.4) !important;
}
.nav-btn-danger:hover {
  background: #dc2626 !important;
}

.delete-item-btn {
  position: absolute;
  right: 0;
  top: 0;
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
  box-shadow: 0 1px 4px rgba(0,0,0,0.2);
  transition: transform 0.15s;
  z-index: 30;
}
.delete-item-btn:hover { transform: scale(1.15); background: #dc2626; }
.delete-item-btn--lg {
  width: 20px;
  height: 20px;
  right: -10px;
  top: -5px;
}

.item-container {
  position: relative;
}

.contact-block {
  border-radius: 6px;
  border: 2px solid transparent;
  cursor: pointer;
  transition: border-color 0.18s ease, box-shadow 0.18s ease;
  padding: 4px 6px;
}

.contact-block.contact-active {
  border: 2px solid #2563eb !important;
  box-shadow: 0 4px 18px rgba(0,0,0,0.10);
  z-index: 10;
}

.contact-item-container {
  position: relative;
}

.contact-item-btns {
  position: absolute;
  right: -4px;
  top: 50%;
  transform: translateY(-50%);
  display: flex;
  flex-direction: row;
  gap: 3px;
  z-index: 9999;
}

.paginated-item {
  transition: none; 
}

main {
  height: auto !important;
  min-height: 100%;
}

.fade-btns-enter-active,
.fade-btns-leave-active {
  transition: opacity 0.15s ease, transform 0.15s ease;
}
.fade-btns-enter-from,
.fade-btns-leave-to {
  opacity: 0;
  transform: scale(0.85);
}

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
:deep(.html-content b),
:deep(.html-content strong) { font-weight: 700 !important; }
:deep(.html-content i),
:deep(.html-content em) { font-style: italic !important; }
:deep(.html-content u) { text-decoration: underline !important; }
:deep(.html-content ul li),
:deep(.html-content ol li) { margin-bottom: 0.25rem; }

@media print {
  .no-print { display: none !important; }
  .section-block,
  .section-block.section-active,
  .contact-block,
  .contact-block.contact-active {
    cursor: default !important;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
    transform: none !important;
    border-radius: 0 !important;
    padding: 0 !important;
    margin: 0 !important;
  }
}

:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .section-block.section-active),
:global(.is-exporting-pdf .contact-block),
:global(.is-exporting-pdf .contact-block.contact-active) {
  cursor: default !important;
  box-shadow: none !important;
  background: transparent !important;
  border-color: transparent !important;
  transform: none !important;
  padding: 0 !important;
  margin: 0 !important;
}
</style>
