<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-col relative box-border text-[#333] leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif' }"
    @click.self="selectedSectionId = null"
  >
    <!-- FULL WIDTH HEADER (Orange Banner) -->
    <header
      class="paginated-item relative w-full h-[45mm] flex items-center shrink-0 z-10"
      :style="{ backgroundColor: templatePrimaryColor, color: 'white' }"
    >
      <!-- Name & Job Title Container starting at 72mm -->
      <div class="flex-1 pl-[72mm] pr-[12mm] flex flex-col justify-center">
        <h1 class="font-extrabold uppercase tracking-wide mb-1 leading-tight !text-[28px]" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'HỌ VÀ TÊN'"></h1>
        <h2 class="font-medium uppercase tracking-[0.15em] text-white/90 !text-[16px] mt-1" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'VỊ TRÍ ỨNG TUYỂN'"></h2>
      </div>
    </header>

    <!-- TWO COLUMN BODY -->
    <div class="flex flex-row flex-1 w-full relative min-h-max z-20" @click.self="selectedSectionId = null">
      <!-- LEFT COLUMN (SIDEBAR) -->
      <aside class="w-[72mm] shrink-0 bg-[#fafafa] flex flex-col pt-0 pb-[10mm] px-[6mm] items-center gap-[2mm] z-30 relative">
        
        <!-- Capsule Container (Avatar + Personal Info in a capsule/ellipse shape) -->
        <div 
          class="contact-capsule relative mt-[-22.5mm] mb-[6mm] w-[60mm] bg-white rounded-[30mm] shadow-xl border border-slate-100 flex flex-col items-center pt-[6mm] pb-[10mm] px-[4mm] gap-[3mm] z-30 section-block"
          :class="{ 'section-active': selectedSectionId === 'contact' }"
          @click.stop="toggleSection('contact')"
        >
          <!-- Avatar Circular -->
          <div class="relative w-[45mm] h-[45mm] rounded-full overflow-hidden border-[4px] border-[#fafafa] shadow-inner bg-slate-100 shrink-0">
            <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="rounded-full w-full h-full object-cover" />
            <div v-else class="w-full h-full flex items-center justify-center bg-gray-100 text-gray-400">
              <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
            </div>
          </div>

          <!-- Contact Items (Dynamic) -->
          <div class="w-full flex flex-col gap-[3mm] text-center" style="margin:0;padding:0;">
            <div 
              v-for="(ci, ciIdx) in contactItems" 
              :key="ci.key" 
              class="relative group/item"
              style="margin:0;padding:0;"
            >
              <p class="text-[10px] text-gray-400 font-medium tracking-wide uppercase" style="margin:0;padding:0;">{{ ci.label }}</p>
              <p class="text-[13px] font-bold text-gray-800 mt-0.5 break-all px-2" style="margin:0;padding:0;" v-html="ci.value"></p>

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

        <!-- Sidebar Dynamic Sections (Awards & References) -->
        <div class="w-full flex flex-col gap-6">
        <draggable
          v-model="sidebarSectionsWritable"
          item-key="id"
          group="sections"
          class="w-full flex flex-col gap-6 cursor-move"
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
              v-if="section.isVisible"
              :data-section-id="section.id" class="section-block relative w-full -mx-[6mm] px-[6mm] py-[3mm] cursor-pointer hover:bg-black/5 transition-colors"
              :class="{ 'section-active': selectedSectionId === section.id }"
              :style="selectedSectionId === section.id ? { '--active-bg': templatePrimaryColor } : {}"
              @click.stop="toggleSection(section.id)"
            >
              <!-- Navigation buttons -->
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print" @click.stop>
                  <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên">
                    <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                  </button>
                  <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống">
                    <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                  </button>
                  <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang phải">
                    <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
                  </button>
                  <button @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                    <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                  </button>
                </div>
              </transition>

              <!-- Section Title Pill -->
              <div class="paginated-item">
                <h3 
                  class="section-title text-white py-[5px] pl-[16px] pr-4 rounded-r-full font-bold uppercase tracking-wide mb-3 shrink-0 shadow-sm -ml-[6mm] w-[110%]"
                  :style="{ backgroundColor: templatePrimaryColor, fontSize: '15px' }"
                >
                  <span v-html="section.title"></span>
                </h3>
              </div>

              <!-- Section Content -->
              <div class="space-y-4 pl-[20px] pr-1" v-if="sectionHasContent(section)">
                <!-- Awards (Danh hiệu và giải thưởng) -->
                <div v-if="section.id.toLowerCase().includes('award') || section.id.toLowerCase().includes('cert')" class="space-y-4">
                  <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container leading-relaxed relative text-slate-700 text-[12.5px]">
                    <p v-if="item.year" class="font-bold text-slate-900 mb-0.5"><span v-html="item.year"></span>:</p>
                    <div class="font-normal html-content" v-html="formatDesc(item.name || item.info || item.desc)"></div>
                    <transition name="fade-btns">
                      <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print">
                        <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                      </button>
                    </transition>
                  </div>
                </div>

                <!-- References (Người tham chiếu) -->
                <div v-else-if="section.id.toLowerCase().includes('reference')" class="space-y-4">
                  <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container leading-relaxed relative text-slate-700 text-[12px] font-medium">
                    <div class="html-content" v-html="formatDesc(item.info || item.desc || item.name)"></div>
                    <transition name="fade-btns">
                      <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print">
                        <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                      </button>
                    </transition>
                  </div>
                </div>

                <!-- Fallback rendering for any other sidebar section -->
                <div v-else class="space-y-3">
                  <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container relative text-slate-700 text-[12.5px] leading-relaxed">
                    <div class="font-bold text-slate-900 mb-0.5" v-if="item.name || item.title || item.company || item.school"><span v-html="item.name || item.title || item.company || item.school"></span></div>
                    <div class="html-content" v-html="formatDesc(item.desc || item.info)"></div>
                    <transition name="fade-btns">
                      <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print">
                        <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                      </button>
                    </transition>
                  </div>
                </div>
              </div>
            </div>
          </template>
        </draggable>
        </div>
      </aside>

      <!-- RIGHT COLUMN (MAIN CONTENT) -->
      <main class="flex-1 flex flex-col relative bg-white z-20 min-h-max" @click.self="selectedSectionId = null">

      <!-- Main Column Dynamic Sections (Summary, Education, Experience) -->
      <div class="px-[12mm] pt-[10mm] pb-[10mm] flex-1 flex flex-col gap-[8mm]">
        <draggable
          v-model="mainSectionsWritable"
          item-key="id"
          group="sections"
          class="flex-1 flex flex-col gap-[8mm] cursor-move"
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
              v-if="section.isVisible"
            :data-section-id="section.id" class="section-block relative group my-0 py-1 cursor-pointer hover:bg-black/5 transition-colors"
            :class="{ 'section-active': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--active-bg': templatePrimaryColor } : {}"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Navigation buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print" @click.stop>
                <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                </button>
                <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                </button>
                <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang trái">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
                </button>
                <button @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </transition>

            <!-- Section Title Pill -->
            <div class="paginated-item">
              <h3 
                class="section-title text-white py-[5px] px-[16px] rounded-[10px] font-bold uppercase tracking-wide mb-3 shadow-sm w-fit"
                :style="{ backgroundColor: templatePrimaryColor, fontSize: '15px' }"
              >
                <span v-html="section.title"></span>
              </h3>
            </div>

            <!-- Content items -->
            <div class="space-y-6" v-if="sectionHasContent(section) || section.id === 'summary'">
              <!-- Summary / Objective (Mục tiêu nghề nghiệp) -->
              <div v-if="section.id.toLowerCase().includes('summary')" class="item-container relative">
                <div class="text-[12.5px] leading-[1.75] text-slate-700 text-justify font-medium html-content" v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Chưa có thông tin mục tiêu nghề nghiệp.')"></div>
              </div>

              <!-- Education (Học vấn) -->
              <div v-else-if="section.id.toLowerCase().includes('education')" class="space-y-5">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative pl-0">
                  <div class="paginated-item">
                    <div class="flex flex-col mb-1.5">
                      <h4 class="text-slate-800 font-bold text-[13.5px] leading-snug">
                        <span v-html="item.school"></span><span v-if="item.major">, <span v-html="item.major"></span></span>
                      </h4>
                      <span v-if="item.year" class="font-bold text-slate-400 text-[11.5px] uppercase tracking-wider mt-0.5"><span v-html="item.year"></span></span>
                    </div>
                    <div v-if="item.gradType" class="text-slate-600 font-semibold text-[12px] mb-1">Tốt nghiệp loại: <span v-html="item.gradType"></span></div>
                  </div>
                  
                  <div class="leading-relaxed text-slate-600 text-justify html-content text-[12.5px]" v-html="formatDesc(item.desc)"></div>
                  
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- Experience / Project / Activities (Kinh nghiệm làm việc) -->
              <div v-else-if="['experience','project','activities'].some(k => section.id.toLowerCase().includes(k))" class="space-y-5">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative pl-0">
                  <div class="paginated-item">
                    <div class="flex flex-col mb-1.5">
                      <h4 class="text-slate-800 font-bold text-[13.5px] leading-snug">
                        <span v-html="section.id.toLowerCase().includes('experience') ? item.company : (item.name || '')"></span><span v-if="item.role">, <span v-html="item.role"></span></span>
                      </h4>
                      <span v-if="item.time" class="font-bold text-slate-400 text-[11.5px] uppercase tracking-wider mt-0.5"><span v-html="item.time"></span></span>
                    </div>
                  </div>
                  
                  <div class="leading-relaxed text-slate-600 text-justify html-content text-[12.5px]" v-html="formatDesc(item.desc)"></div>

                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- Fallback rendering for any other right column section -->
              <div v-else class="space-y-4">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative leading-relaxed text-slate-700 text-[12.5px]">
                  <div class="paginated-item font-bold text-slate-900 mb-0.5" v-if="item.name || item.title || item.company || item.school"><span v-html="item.name || item.title || item.company || item.school"></span></div>
                  <div class="paginated-item font-semibold text-slate-400 text-[11px] mb-1" v-if="item.time || item.year"><span v-html="item.time || item.year"></span></div>
                  <div class="html-content text-justify" v-html="formatDesc(item.desc || item.info || item.role)"></div>
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
        </draggable>
      </div>
    </main>
  </div>

  <!-- FIXED PAGE BORDER DECORATORS -->
    <template v-for="p in pageCount" :key="'footer-border-' + p">
      <div
        class="absolute left-0 w-full flex items-center z-40 pointer-events-none"
        :style="{ top: `calc(${p * 297}mm - 12mm)`, height: '1.5px', paddingLeft: '12mm', paddingRight: '12mm' }"
      >
        <div class="w-full h-full opacity-10 bg-gradient-to-r from-slate-400 via-slate-500 to-slate-400"></div>
      </div>
    </template>

    <!-- PRINT PAGES GUIDELINES (VISUAL IN BROWSER EDITOR) -->
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
import { computed, ref, onMounted, nextTick, watch, onUnmounted, toRaw } from 'vue'
import draggable from 'vuedraggable'

const cvRoot = ref(null)
const pageCount = ref(1)
const selectedSectionId = ref(null)

const props = defineProps({
  resumeData: { type: Object, required: true }
})
const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

const toggleSection = (id) => {
  selectedSectionId.value = selectedSectionId.value === id ? null : id
  requestPagination()
}

// ─── CONTACT ITEMS LOGIC ───
const contactLabels = {
  gender: 'Giới tính',
  birthDate: 'Ngày sinh',
  email: 'Email',
  phone: 'Điện thoại',
  address: 'Địa chỉ'
}

const contactOrder = ref(['gender', 'birthDate', 'email', 'phone', 'address'])
const hiddenContacts = ref([])

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  return g[key] || ''
}

const contactItems = computed(() => {
  return contactOrder.value
    .filter(key => !hiddenContacts.value.includes(key) && !isEmpty(getContactValue(key)))
    .map(key => ({
      key,
      label: contactLabels[key],
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
    }
    hiddenContacts.value.push(key)
    requestPagination()
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
  if (!c || c.toLowerCase() === '#2b5c8f') return '#f26522' // Vibrant Orange matches the image perfectly
  return c
})

const sidebarSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'left' && s.isVisible)
)

const mainSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'right' && s.isVisible)
)

const sidebarSectionsWritable = ref([])
const mainSectionsWritable = ref([])

watch(sidebarSections, (newVal) => {
  sidebarSectionsWritable.value = [...newVal]
}, { immediate: true, deep: true })

watch(mainSections, (newVal) => {
  mainSectionsWritable.value = [...newVal]
}, { immediate: true, deep: true })

const onDragEnd = () => {
  // Update columns based on which list they are in
  sidebarSectionsWritable.value.forEach(s => {
    const item = props.resumeData.sections.find(x => x.id === s.id)
    if (item) item.column = 'left'
  })
  mainSectionsWritable.value.forEach(s => {
    const item = props.resumeData.sections.find(x => x.id === s.id)
    if (item) item.column = 'right'
  })

  // Reorder sections array
  const leftIds = sidebarSectionsWritable.value.map(s => s.id)
  const rightIds = mainSectionsWritable.value.map(s => s.id)
  
  const newSections = []
  props.resumeData.sections.forEach(s => {
    if (!leftIds.includes(s.id) && !rightIds.includes(s.id)) {
      newSections.push(s)
    }
  })
  
  leftIds.forEach(id => {
    const item = props.resumeData.sections.find(s => s.id === id)
    if (item) newSections.push(item)
  })
  
  rightIds.forEach(id => {
    const item = props.resumeData.sections.find(s => s.id === id)
    if (item) newSections.push(item)
  })

  props.resumeData.sections.splice(0, props.resumeData.sections.length, ...newSections)
  requestPagination()
}

const sidebarIds = computed(() => sidebarSections.value.map(s => s.id))
const mainIds = computed(() => mainSections.value.map(s => s.id))

// ─── PAGINATION ENGINE: AUTO FLUID SPLITTING ───
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
  
  const bottomSafeZone = 14 * pxPerMm 
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
    
    if (bottomInPage > (pageH - bottomSafeZone)) {
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
  if (props.resumeData?.sections) {
    const LEFT_IDS  = ['awards', 'references', 'skills', 'it_skills', 'languages', 'certifications', 'hobbies', 'additional', 'reference'];
    const RIGHT_IDS = ['summary', 'education', 'experience', 'project', 'activities'];

    // Only activate sections in the image: Awards, References, Summary, Education, Experience
    const ACTIVE_IDS = ['summary', 'education', 'experience', 'awards', 'references'];

    props.resumeData.sections.forEach(sec => {
      // 1. Map to correct column if not set
      if (!sec.column) {
        if (LEFT_IDS.some(k => sec.id.toLowerCase().includes(k))) {
          sec.column = 'left';
        } else {
          sec.column = 'right';
        }
      }
      // 2. Set default visibility: only active those in ACTIVE_IDS if not set
      if (sec.isVisible === undefined) {
        const isActive = ACTIVE_IDS.some(k => sec.id.toLowerCase().includes(k));
        sec.isVisible = isActive;
      }
    });
  }

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

.section-block {
  position: relative;
  border-radius: 8px;
  border: 2px solid transparent;
  cursor: pointer;
  transition: border-color 0.18s ease, box-shadow 0.18s ease;
}

.contact-capsule {
  position: relative !important;
  border-radius: 30mm !important;
}

@media print {
  .contact-capsule {
    position: relative !important;
    border-radius: 30mm !important;
    padding-top: 6mm !important;
    padding-bottom: 10mm !important;
    padding-left: 4mm !important;
    padding-right: 4mm !important;
  }
}

.section-block:hover {
  background-color: rgba(0, 0, 0, 0.01);
}

.section-block.section-active {
  border-radius: 8px !important;
  border: 2px solid var(--active-bg, #f26522) !important;
  box-shadow: 0 4px 18px rgba(0, 0, 0, 0.08);
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
.nav-btn-danger { background: #ef4444 !important; }
.nav-btn-danger:hover { background: #dc2626 !important; }

.nav-btn--xs {
  padding: 2px !important;
  border-radius: 3px !important;
}

.contact-item-btns {
  position: absolute;
  right: 0;
  top: 50%;
  transform: translateY(-50%);
  display: flex;
  gap: 3px;
  z-index: 50;
  background: white;
  padding-left: 5px;
}

.delete-item-btn {
  position: absolute;
  right: -10px;
  top: -2px;
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
  right: -12px;
  top: -4px;
}

.item-container {
  position: relative;
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
  .section-block.section-active {
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
:global(.is-exporting-pdf .section-block.section-active) {
  cursor: default !important;
  box-shadow: none !important;
  background: transparent !important;
  border-color: transparent !important;
  transform: none !important;
}
</style>
