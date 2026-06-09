<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="flex flex-row relative box-border bg-white overflow-hidden text-[#333]"
    :style="{
      width: '210mm',
      height: `${Math.max(1, pageCount) * 297}mm`,
      fontFamily: '\'Inter\', sans-serif',
      lineHeight: '1.5'
    }"
    @click.self="selectedSectionId = null"
  >
    <!-- SIDEBAR -->
    <aside
      class="z-10 flex flex-col shrink-0 relative box-border"
      :style="{ width: '32%', backgroundColor: sidebarBgColor, padding: '30px 20px' }"
    >
      <!-- Avatar -->
      <div class="paginated-item relative z-20 w-full flex flex-col items-center mb-8">
        <div
          class="relative rounded-full overflow-hidden mx-auto bg-white"
          :style="{ width: '150px', height: '150px', border: '5px solid white', boxShadow: '0 4px 15px rgba(0,0,0,0.1)' }"
        >
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center bg-gray-100 text-gray-400">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
          </div>
        </div>
      </div>

      <!-- Thông tin cá nhân -->
      <div 
        class="section-block mb-4 ml-[4mm] cursor-pointer hover:bg-black/5 transition-colors"
        :class="{ 'section-active': selectedSectionId === 'contact' }"
        :style="getSectionStyle('contact', 'left')"
        @click.stop="toggleSection('contact')"
      >
        <!-- Nav Buttons for Contact Block - Removed eye button -->
        <h3 class="section-title paginated-item font-bold border-b-2 mb-2 pb-1 uppercase" :style="{ color: templatePrimaryColor, borderColor: templatePrimaryColor }">
          Thông tin cá nhân
        </h3>
        <ul class="w-full list-none p-0 m-0 text-slate-700 space-y-3" :style="{ fontSize: '13px' }">
          <li 
            v-for="(ci, ciIdx) in contactItems" 
            :key="ci.key" 
            class="flex items-start gap-3 paginated-item relative group/item"
          >
            <span class="icon-wrap text-blue-500 mt-0.5"><svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20" v-html="ci.icon"></svg></span>
            <span class="break-all flex-1" v-html="ci.value"></span>

            <!-- Individual contact item buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print">
                <button @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn nav-btn--xs" title="Lên"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn nav-btn--xs" title="Xuống"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn--xs nav-btn-danger" title="Ẩn"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg></button>
              </div>
            </transition>
          </li>
        </ul>
      </div>

      <!-- Sidebar Sections -->
      <div class="w-full flex-1 flex flex-col gap-2">
        <draggable
          v-model="sidebarSectionsWritable"
          item-key="id"
          group="sections"
          class="flex flex-col gap-2 cursor-move"
          @end="onDragEnd"
          animation="200"
          ghost-class="opacity-30"
          :delay="100"
          :delayOnTouchOnly="true"
          :fallbackTolerance="5"
          filter=".nav-btn, .delete-btn, .contact-item-btns, .html-content, input, .action-btn"
        >
          <template #item="{ element: section }">
          <div
            v-if="section.isVisible"
            :data-section-id="section.id" class="section-block group ml-[4mm] cursor-pointer hover:bg-black/5 transition-colors"
            :class="{ 'section-active': selectedSectionId === section.id }"
            :style="getSectionStyle(section.id, 'left')"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Action Buttons -->
            <div v-if="selectedSectionId === section.id" class="action-btns no-print">
              <button @click.stop="$emit('moveUp', section.id, sidebarIds)" class="action-btn" title="Lên">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7" /></svg>
              </button>
              <button @click.stop="$emit('moveDown', section.id, sidebarIds)" class="action-btn" title="Xuống">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7" /></svg>
              </button>
              <button @click.stop="$emit('moveHorizontal', section.id, 'right')" class="action-btn" title="Sang phải">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7" /></svg>
              </button>
              <button @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <h3 class="section-title paginated-item" :style="{ color: templatePrimaryColor, borderBottomColor: templatePrimaryColor }">
              <span v-html="section.title"></span>
            </h3>

            <div class="space-y-4 px-1">
  <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative text-slate-800">
    
    <!-- SỬA LỖI VÀ ĐỊNH DẠNG HỌC VẤN / KINH NGHIỆM GIỐNG ẢNH MẪU SỐ 3 -->
    <template v-if="['education','experience','project','activities'].includes(section.id)">
      <!-- Ngành học / Vị trí in đậm -->
      <div class="html-content font-bold text-[14px] leading-snug flex flex-col text-slate-800 paginated-item" v-html="formatDesc(item.major || item.role || item.position || item.name)"></div>
      
      <!-- Thời gian - Xếp loại in nghiêng màu nhạt -->
      <div class="html-content text-[13px] text-slate-500 italic mt-0.5 flex items-center flex-wrap paginated-item">
        <span v-if="item.year || item.time || item.date" v-html="formatDesc(item.year || item.time || item.date)"></span>
        <span v-if="(item.year || item.time || item.date) && item.gradType" class="mx-1">•</span>
        <span v-if="item.gradType" v-html="formatDesc('Loại ' + item.gradType)"></span>
      </div>
      
      <!-- Tên trường / Công ty ở dưới cùng -->
      <div class="html-content text-[13px] text-slate-600 mt-0.5 flex flex-col paginated-item" v-if="item.school || item.company || item.organization" v-html="formatDesc((section.id === 'education' ? 'Tên trường học ' : '') + (item.school || item.company || item.organization))"></div>
      
      <!-- Mô tả thêm -->
      <div v-if="item.desc" class="html-content text-justify text-[13px] leading-relaxed flex flex-col mt-1 text-slate-500 paginated-item" v-html="formatDesc(item.desc)"></div>
    </template>
    
    <!-- MẶC ĐỊNH CHO KỸ NĂNG / CHỨNG CHỈ CÒN LẠI -->
    <template v-else>
      <div class="flex justify-between items-start gap-2 paginated-item">
        <div class="html-content font-bold text-[14px] leading-snug flex-1 flex flex-col" v-html="formatDesc(item.name || item.title)" />
        <span v-if="item.level || item.info" class="font-bold text-[12px] text-slate-500 shrink-0"><span v-html="item.level || item.info"></span></span>
      </div>
      <div v-if="item.year || item.time" class="html-content text-[12px] text-slate-500 mt-0.5 flex flex-col paginated-item" v-html="formatDesc(item.year || item.time)" />
      <div v-if="item.desc" class="html-content text-[12px] text-slate-500 mt-0.5 flex flex-col paginated-item text-justify" v-html="formatDesc(item.desc)" />
    </template>
    
    <button
      v-if="selectedSectionId === section.id"
      @click.stop="$emit('removeItem', section.id, itemIndex)"
      class="delete-btn no-print"
      title="Xóa"
    >
      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12" /></svg>
    </button>
  </div>
</div>
          </div>
        </template>
        </draggable>
      </div>
    </aside>

    <!-- MAIN CONTENT -->
    <main
      class="flex-1 flex flex-col relative bg-white z-20 overflow-hidden box-border"
      :style="{ padding: '40px 60px 40px 45px' }"
      @click.self="selectedSectionId = null"
    >
      <header class="cv-header w-full flex flex-col mb-4 paginated-item" style="padding-top: 22px; padding-left: 20px;">
        <h1
          class="uppercase font-black tracking-tight"
          :style="{ color: templatePrimaryColor, fontSize: '42px', lineHeight: '1.1', fontWeight: '900' }"
          v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'HỌ VÀ TÊN'"
        />
        <h2
          class="uppercase font-semibold text-slate-500 mt-2 tracking-widest"
          :style="{ fontSize: '18px', fontWeight: '600' }"
          v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'VỊ TRÍ ỨNG TUYỂN'"
        />
      </header>

      <!-- Main Sections -->
      <div class="w-full flex flex-col gap-2">
        <draggable
          v-model="mainSectionsWritable"
          item-key="id"
          group="sections"
          class="flex flex-col gap-2 cursor-move"
          @end="onDragEnd"
          animation="200"
          ghost-class="opacity-30"
          :delay="100"
          :delayOnTouchOnly="true"
          :fallbackTolerance="5"
          filter=".nav-btn, .delete-btn, .contact-item-btns, .html-content, input, .action-btn"
        >
          <template #item="{ element: section }">
          <div
            v-if="section.isVisible"
            :data-section-id="section.id" class="section-block group cursor-pointer hover:bg-black/5 transition-colors"
            :class="{ 'section-active': selectedSectionId === section.id }"
            :style="getSectionStyle(section.id, 'right')"
            @click.stop="toggleSection(section.id)"
          >
            <div v-if="selectedSectionId === section.id" class="action-btns no-print">
              <button @click.stop="$emit('moveUp', section.id, mainIds)" class="action-btn" title="Lên">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7" /></svg>
              </button>
              <button @click.stop="$emit('moveDown', section.id, mainIds)" class="action-btn" title="Xuống">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7" /></svg>
              </button>
              <button @click.stop="$emit('moveHorizontal', section.id, 'left')" class="action-btn" title="Sang trái">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7" /></svg>
              </button>
              <button @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <h3
              class="section-title paginated-item"
              :style="{
                color: templatePrimaryColor,
                borderBottomColor: templatePrimaryColor,
                fontSize: '18px !important',
                width: 'calc(100% - 30px) !important'
              }"
            >
              <span v-html="section.title"></span>
            </h3>

            <!-- Summary -->
            <div
              v-if="section.id === 'summary'"
              class="html-content text-justify text-slate-700 leading-relaxed flex flex-col"
              :style="{ fontSize: '14px' }"
              v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Tôi là một ứng viên năng động...')"
            />

            <!-- Other sections -->
            <div v-else class="space-y-8">
              <div
                v-for="(item, itemIndex) in section.items"
                :key="item._refId"
                class="item-container relative w-full text-slate-700"
              >
                <!-- Education / Experience / Project / Activities -->
                <template v-if="['education','experience','project','activities'].includes(section.id)">
                  <div class="flex justify-between items-start gap-4 mb-1 paginated-item">
                    <div class="font-bold text-[16px] text-slate-900 leading-tight">
                      <span v-html="section.id === 'education' ? item.school : (item.company || item.name)"></span>
                    </div>
                    <div v-if="item.year || item.time" class="shrink-0 font-bold text-[13px] text-slate-400 uppercase tracking-wider">
                      <span v-html="item.year || item.time"></span>
                    </div>
                  </div>
                  <div v-if="item.major || item.role" class="font-semibold text-slate-500 italic text-[14px] mb-2 paginated-item">
                    <span v-html="item.major || item.role"></span>
                  </div>
                  <div v-if="item.desc" class="html-content text-justify text-[14px] leading-relaxed flex flex-col" v-html="formatDesc(item.desc)" />
                  <div v-else-if="item.gradType" class="text-blue-500 font-semibold text-[13px] paginated-item"><span v-html="item.gradType"></span></div>
                </template>

                <!-- Generic -->
                <template v-else>
                <div class="flex-1">
                  <div class="flex justify-between items-start gap-2">
                    <div v-if="!isEmpty(item.name || item.title)" 
                        class="html-content font-bold text-[15px] mb-1 paginated-item flex-1"
                        v-html="formatDesc(item.name || item.title)" />
                    <span v-if="item.level || item.info" class="font-bold text-[13px] text-slate-500 shrink-0"><span v-html="item.level || item.info"></span></span>
                  </div>
                  <div v-if="!isEmpty(item.role || item.info || item.position)"
                      class="html-content text-[13px] text-slate-500 italic mb-1 paginated-item"
                      v-html="formatDesc(item.role || item.info || item.position)" />
                  <div v-if="item.year || item.time" 
                      class="html-content text-[13px] text-blue-500 font-bold mb-1 paginated-item"
                      v-html="formatDesc(item.year || item.time)" />
                  <div v-if="!isEmpty(item.desc)"
                      class="html-content text-justify text-[14px] leading-relaxed flex flex-col paginated-item"
                      v-html="formatDesc(item.desc)" />
                </div>
              </template>

                <button
                  v-if="selectedSectionId === section.id"
                  @click.stop="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn delete-btn--main no-print"
                  title="Xóa"
                >
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12" /></svg>
                </button>
              </div>
            </div>
          </div>
        </template>
        </draggable>
      </div>
    </main>

    <!-- Page break overlays -->
    <template v-for="p in (pageCount - 1)" :key="'div-' + p">
      <div
        class="absolute left-0 w-full z-50 flex flex-col items-center justify-center pointer-events-none no-print"
        :style="{ top: `calc(${p * 297}mm - 8px)` }"
      >
        <div class="w-[105%] h-[16px] bg-slate-800/95 shadow-inner border-y border-black/30 backdrop-blur-sm" />
        <span class="absolute text-[9px] uppercase font-bold text-slate-300 tracking-widest bg-slate-700 px-3 py-0.5 rounded border border-slate-600 shadow-md">
          Ngắt trang {{ p + 1 }}
        </span>
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

// ─── CONTACT ITEMS LOGIC ───
const contactIcons = {
  phone: '<path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z" />',
  email: '<path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z" /><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z" />',
  dob: '<path d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" />',
  gender: '<path d="M10 2a8 8 0 100 16 8 8 0 000-16zM7 9a3 3 0 116 0 3 3 0 01-6 0z" />',
  address: '<path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd" />'
}

const contactOrder = ref(['phone', 'email', 'dob', 'gender', 'address'])
const hiddenContacts = ref([])

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  switch (key) {
    case 'phone': return g.phone
    case 'email': return g.email
    case 'dob': return g.dob || g.birthDate
    case 'gender': return g.gender
    case 'address': return g.address
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
    }
    hiddenContacts.value.push(key)
    requestPagination()
  }
}

const cvRoot = ref(null)
const pageCount = ref(1)
const selectedSectionId = ref(null)

const templatePrimaryColor = computed(() => props.resumeData?.theme?.primaryColor || '#2d7fb2')
const sidebarBgColor = '#e0f2f7'

const toggleSection = (id) => {
  selectedSectionId.value = selectedSectionId.value === id ? null : id
  requestPagination()
}

// Bỏ Scale để không làm sai lệch vị trí
const getSectionStyle = (sectionId, column) => {
  const isActive = selectedSectionId.value === sectionId
  const bgColor = column === 'left' ? sidebarBgColor : '#ffffff'

  if (isActive) {
    return {
      borderColor: `${bgColor} !important`,
      borderStyle: 'solid !important',
      borderWidth: '2px !important',
      borderRadius: '6px !important',
      boxShadow: `0 4px 16px rgba(0,0,0,0.10), 0 0 0 2px ${templatePrimaryColor.value}22`,
      transition: 'box-shadow 0.18s ease',
      zIndex: 10,
      position: 'relative',
    }
  }
  return {
    borderColor: 'transparent !important',
    borderStyle: 'solid !important',
    borderWidth: '2px !important',
    borderRadius: '6px !important',
    transition: 'box-shadow 0.18s ease',
    position: 'relative',
  }
}

const sidebarSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'left' && s.id !== 'summary')
)
const mainSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'right')
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

const sidebarIds = computed(() => sidebarSections.value.map(s => s.id))
const mainIds = computed(() => mainSections.value.map(s => s.id))

const isEmpty = (val) => {
  if (val === null || val === undefined) return true
  if (typeof val === 'object') return false
  const str = String(val)
  return str.replace(/<[^>]*>/g, '').trim() === ''
}

const safeHtml = (val) => {
  if (val === null || val === undefined) return ''
  return String(val)
}

// BỘ LỌC THÔNG MINH: BĂM NHỎ TỪNG DÒNG
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

  // Cứu mã màu của List do người dùng chọn trong Editor
  tempDiv.querySelectorAll('li').forEach(li => {
    const child = li.firstElementChild
    if (child && (child.tagName === 'FONT' || child.tagName === 'SPAN')) {
      if (child.color) li.style.color = child.color
      if (child.style?.color) li.style.color = child.style.color
    }
  })

  // Đệ quy bọc các thẻ text bằng SPAN block để không làm hỏng thẻ P/DIV
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

// ─── THUẬT TOÁN PHÂN TRANG VÒNG LẶP ĐỘNG ──────────────────────────────
const A4_WIDTH_MM  = 210
const A4_HEIGHT_MM = 297

let paginateTimer = null
const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 60)
}

const doPagination = async () => {
  if (!cvRoot.value) return

  // Tạm vô hiệu hóa hiệu ứng Scale để thuật toán đo chính xác
  const activeElements = cvRoot.value.querySelectorAll('.section-active, .section-active--main, .section-active--sidebar')
  activeElements.forEach(el => el.style.setProperty('transform', 'none', 'important'))

  // BỘ LỌC: Chỉ lấy ĐÚNG LỚP NGOÀI CÙNG
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

  const cvRect = cvRoot.value.getBoundingClientRect()
  const pxPerMm = cvRect.width / A4_WIDTH_MM
  const pageH = A4_HEIGHT_MM * pxPerMm
  
  const bottomSafeZone = 14 * pxPerMm
  const topMargin = 16 * pxPerMm

  let stable = false
  let passes = 0

  while (!stable && passes < 30) {
    stable = true
    passes++
    
    const currentCvRect = cvRoot.value.getBoundingClientRect()

    for (let i = 0; i < allElements.length; i++) {
      const el = allElements[i]
      if (el.offsetHeight === 0) continue

      const elRect = el.getBoundingClientRect()
      const top = elRect.top - currentCvRect.top
      const height = elRect.height
      const bottom = top + height

      const pageIndex = Math.floor(top / pageH)
      const topInPage = top - (pageIndex * pageH)
      const bottomInPage = topInPage + height

      // Chặn nếu có thẻ khổng lồ lọt vào
      if (height > (pageH - bottomSafeZone - topMargin)) continue

      if (bottomInPage > (pageH - bottomSafeZone)) {
         const distToNextPage = pageH - topInPage + topMargin
         const currentMt = parseFloat(el.style.marginTop || '0')
         // Ép dòng HTML nhảy trang!
         el.style.setProperty('margin-top', `${currentMt + distToNextPage}px`, 'important')
         stable = false
         break
      }
    }
  }

  // Khôi phục lại hiệu ứng
  activeElements.forEach(el => el.style.removeProperty('transform'))

  const finalCvRect = cvRoot.value.getBoundingClientRect()
  let maxBottom = 0
  allElements.forEach(el => {
    const rect = el.getBoundingClientRect()
    const bottom = rect.bottom - finalCvRect.top
    if (bottom > maxBottom) maxBottom = bottom
  })

  pageCount.value = Math.max(1, Math.ceil(maxBottom / pageH))
}

watch(() => props.resumeData, requestPagination, { deep: true })

onMounted(() => {
  const defaultVisible = new Set(['summary', 'education', 'experience', 'skills', 'certifications'])
  props.resumeData?.sections?.forEach(sec => {
    const hasData = sec.items?.length > 0
    if (sec.isVisible === undefined) {
      sec.isVisible = defaultVisible.has(sec.id) || hasData
    }
  })
  const refSection = props.resumeData?.sections?.find(s => s.id === 'references')
  if (refSection) refSection.column = 'right'

  requestPagination()
  window.addEventListener('resize', requestPagination)
  document.addEventListener('keyup', requestPagination)
})

onUnmounted(() => {
  window.removeEventListener('resize', requestPagination)
  document.removeEventListener('keyup', requestPagination)
  if (paginateTimer) clearTimeout(paginateTimer)
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800;900&display=swap');

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

.cv-header {
  flex-shrink: 0 !important;
}

/* Đóng băng hiệu ứng Transition để đo độ chuẩn */
.paginated-item {
  transition: none !important;
}

.section-block {
  cursor: pointer !important;
  padding: 10px 15px !important;
  border-radius: 6px !important;
  border: 2px solid transparent !important;
  transition: box-shadow 0.18s ease, border-color 0.18s ease !important;
  position: relative !important;
}

.section-block:hover {
  background: rgba(0, 0, 0, 0.025) !important;
}

.section-title {
  display: block !important;
  text-transform: uppercase !important;
  font-weight: 700 !important;
  font-size: 16px !important;
  margin-bottom: 1rem !important;
  border-bottom-width: 2px !important;
  border-bottom-style: solid !important;
  padding-bottom: 5px !important;
}

.icon-wrap {
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.action-btns {
  position: absolute !important;
  right: 10px !important;
  top: 10px !important;
  display: flex !important;
  gap: 4px !important;
  z-index: 100 !important;
}

.action-btn {
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  padding: 4px !important;
  background: v-bind(templatePrimaryColor) !important;
  color: white !important;
  border-radius: 4px !important;
  cursor: pointer !important;
  border: none !important;
  transition: opacity 0.15s !important;
}

.action-btn:hover {
  opacity: 0.85 !important;
}

.nav-btn {
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  padding: 4px !important;
  background: v-bind(templatePrimaryColor) !important;
  color: white !important;
  border-radius: 4px !important;
  cursor: pointer !important;
  border: none !important;
  transition: opacity 0.15s !important;
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
  background: v-bind(sidebarBgColor);
  padding-left: 5px;
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

.delete-btn {
  position: absolute !important;
  top: 0 !important;
  right: -8px !important;
  width: 18px !important;
  height: 18px !important;
  background: #ef4444 !important;
  color: white !important;
  border-radius: 9999px !important;
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  cursor: pointer !important;
  border: none !important;
  box-shadow: 0 2px 6px rgba(0,0,0,0.18) !important;
  transition: opacity 0.15s !important;
  z-index: 30 !important;
}

.delete-btn--main {
  right: -20px !important;
  width: 20px !important;
  height: 20px !important;
}

.delete-btn:hover { opacity: 0.85 !important; }

:deep(.html-content)                                   { margin: 0 !important; padding: 0 !important; }
:deep(.html-content p)                                 { margin-bottom: 2px !important; padding: 0 !important; }
:deep(.html-content ul)                                { list-style-type: disc !important; padding-left: 1.5rem !important; margin-bottom: 2px !important; }
:deep(.html-content ol)                                { list-style-type: decimal !important; padding-left: 1.5rem !important; margin-bottom: 2px !important; }
:deep(.html-content b), :deep(.html-content strong)    { font-weight: bold !important; }
:deep(.html-content i), :deep(.html-content em)        { font-style: italic !important; }
:deep(.html-content u)                                 { text-decoration: underline !important; }
:deep(.html-content li)                                { margin-bottom: 2px !important; }

:deep(.html-content li:has(> font[size="1"])) { font-size: 10px !important; }
:deep(.html-content li:has(> font[size="2"])) { font-size: 13px !important; }
:deep(.html-content li:has(> font[size="3"])) { font-size: 16px !important; }

/* BẢO VỆ KÍCH THƯỚC BẢN IN PDF (XÓA SẠCH PADDING 0) */
@media print {
  .no-print { display: none !important; }
  .section-block {
    border: none !important;
    background: transparent !important;
    transform: none !important;
    box-shadow: none !important;
  }
}

:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block) {
    border: none !important;
    background: transparent !important;
    transform: none !important;
    box-shadow: none !important;
}
</style>