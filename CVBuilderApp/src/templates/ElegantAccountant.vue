<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="relative box-border bg-white overflow-hidden text-[#333]"
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

    <!-- NỘI DUNG CHÍNH -->
    <div
      class="w-full h-full box-border flex flex-col relative z-10"
      :style="{ padding: '20mm 20mm 20mm 20mm' }"
    >
      <!-- HEADER -->
      <header class="paginated-item w-full flex justify-between items-start mb-[40px] gap-[20px]">
        <div class="flex-[1.4] flex flex-col">
          <div class="w-fit mb-[25px]">
            <h1
              class="m-0 leading-tight break-words"
              :style="{ fontSize: '64px', color: '#4a5568', fontFamily: 'Georgia, serif', fontWeight: '400' }"
            >{{ isEmpty(resumeData.general.fullName) ? 'Họ Và Tên Ứng Viên' : resumeData.general.fullName }}</h1>
            <div class="h-[1px] w-full bg-[#cbd5e0] mt-[5px]"></div>
          </div>

          <div 
            class="section-block flex flex-col gap-[8px] text-[#333] relative cursor-pointer hover:bg-black/5 transition-colors" 
            :class="{ 'section-selected': selectedSectionId === 'contact' }"
            :style="selectedSectionId === 'contact' ? { '--sel-color': templatePrimaryColor, borderColor: templatePrimaryColor, fontSize: '14px' } : { fontSize: '14px' }"
            @click.stop="toggleSection('contact')"
          >
            <div 
              v-for="(ci, ciIdx) in contactItems" 
              :key="ci.key" 
              class="flex gap-[5px] relative group/item paginated-item"
            >
              <span class="font-bold shrink-0">{{ ci.label }}:</span>
              <span class="flex-1" v-html="ci.value"></span>

              <!-- Individual contact item buttons -->
              <transition name="fade-btns">
                <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print">
                  <button v-if="ciIdx > 0" @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn nav-btn--xs" title="Lên"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                  <button v-if="ciIdx < contactItems.length - 1" @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn nav-btn--xs" title="Xuống"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                  <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn--xs nav-btn-danger" title="Xóa"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button>
                </div>
              </transition>
            </div>
          </div>
        </div>

        <div class="flex-1 flex justify-center pt-[20px]">
          <div
            class="relative rounded-full overflow-hidden bg-[#f7fafc] border-[12px] border-white shadow-sm shrink-0"
            :style="{ width: '190px', height: '190px' }"
          >
            <img
              v-if="resumeData.general.avatarUrl"
              :src="resumeData.general.avatarUrl"
              class="w-full h-full object-cover"
              alt="Avatar"
            />
            <div v-else class="w-full h-full flex items-center justify-center bg-gray-100 text-gray-400">
              <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1"
                      d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
              </svg>
            </div>
          </div>
        </div>

        <div class="flex-1 text-right flex flex-col items-end pt-[50px]">
          <div class="w-fit">
            <h2
              class="m-0 break-words leading-tight"
              :style="{ fontSize: '32px', color: '#333', fontWeight: '400' }"
            >{{ isEmpty(resumeData.general.jobTitle) ? 'Vị Trí Ứng Tuyển' : resumeData.general.jobTitle }}</h2>
            <div class="h-[1px] w-full bg-[#333] mt-[10px]"></div>
          </div>
        </div>
      </header>

      <!-- BODY SECTIONS -->
      <div class="w-full flex flex-col m-0 p-0">
        <draggable
          v-model="allVisibleSectionsWritable"
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
          <!-- Đã xóa nhãn paginated-item ở lớp bọc ngoài cùng -->
          <div
            class="section-block relative w-full mb-[30px] cursor-pointer hover:bg-black/5 transition-colors"
            :class="{ 'section-selected': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id
              ? { '--sel-color': templatePrimaryColor, borderColor: templatePrimaryColor }
              : {}"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >

            <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
              <button
                @click.stop.prevent="$emit('moveUp', section.id, section.column === 'left' ? sidebarIds : mainIds)"
                class="nav-btn" title="Di chuyển lên"
              >
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/>
                </svg>
              </button>
              <button
                @click.stop.prevent="$emit('moveDown', section.id, section.column === 'left' ? sidebarIds : mainIds)"
                class="nav-btn" title="Di chuyển xuống"
              >
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/>
                </svg>
              </button>
              <button
                @click.stop.prevent="$emit('moveHorizontal', section.id, section.column === 'left' ? 'right' : 'left')"
                class="nav-btn" title="Đổi cột"
              >
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5"
                        d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4"/>
                </svg>
              </button>
              <button @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <!-- TIÊU ĐỀ LÀ 1 DÒNG TỰ ĐỘNG PHÂN TRANG -->
            <h3
              class="paginated-item uppercase w-full block break-words"
              :style="{ fontSize: '28px', color: '#222', marginBottom: '15px', fontWeight: 'bold', letterSpacing: '1px' }"
            ><span v-html="section.title"></span></h3>

            <!-- SUMMARY -->
            <div v-if="section.id === 'summary'">
              <!-- v-html để hàm formatDesc băm nhỏ thành nhiều thẻ span -->
              <div class="html-content text-justify w-full text-[#333] flex flex-col" :style="{ fontSize: '15px', lineHeight: '1.5' }" v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Tôi là một ứng viên năng động, có tinh thần trách nhiệm cao...')"></div>
            </div>

            <!-- KINH NGHIỆM / HỌC VẤN / DỰ ÁN / HOẠT ĐỘNG -->
            <div
              v-else-if="['education', 'experience', 'project', 'activities'].includes(section.id)"
              class="flex flex-wrap w-full items-start"
              style="gap: 25px 60px;"
            >
              <div v-if="!section.items || section.items.length === 0"
                   class="text-[#aaa] italic text-[14px] pl-[20px] w-full">Chưa có dữ liệu</div>
              
              <!-- Đã xóa nhãn paginated-item ở container, để JS đi sâu vào chém các dòng -->
              <div
                v-for="(item, itemIndex) in section.items"
                :key="item._refId"
                class="item-container relative text-[#333] break-words pl-[20px]"
                :style="(section.items || []).length >= 2 ? 'width: calc(50% - 30px)' : 'width: 100%'"
              >
                <!-- DÒNG 1: Tiêu đề + Chấm tròn dính chặt nhau -->
                <div class="w-full flex justify-between items-start mb-[3px] paginated-item relative">
                  <div class="absolute -left-[20px] top-[8px] w-[6px] h-[6px] rounded-full bg-black"></div>
                  <div class="text-[16px] leading-tight font-bold flex-1 pr-3 flex flex-col" v-html="formatDesc(item.company || item.school || item.name || item.organization)"></div>
                  <div v-if="item.time || item.year" class="italic text-[16px] text-[#4a5568] shrink-0"><span v-html="item.time || item.year"></span></div>
                </div>
                
                <div v-if="item.role || item.major" class="paginated-item text-[16px] italic mb-[3px] w-full flex flex-col" v-html="formatDesc(item.role || item.major)"></div>
                <div v-if="item.gradType" class="paginated-item text-[14.5px] font-medium w-full mb-[3px] text-blue-600">Xếp loại: <span v-html="item.gradType"></span></div>
                
                <!-- BĂM MÔ TẢ THÀNH NHIỀU DÒNG -->
                <div v-if="item.desc" class="html-content text-justify w-full text-[#333] flex flex-col mt-1"
                     :style="{ fontSize: '15px', lineHeight: '1.5' }" v-html="formatDesc(item.desc)">
                </div>
                
                <button
                  v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print" title="Xóa"
                >
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                  </svg>
                </button>
              </div>
            </div>

            <!-- KỸ NĂNG (Gộp khối vì nó ngắn) -->
            <div
              v-else-if="['skills','it_skills','languages'].includes(section.id)"
              class="flex flex-wrap w-full" style="gap: 25px 80px;"
            >
              <div v-if="!section.items || section.items.length === 0"
                   class="text-[#aaa] italic text-[14px] pl-[20px] w-full">Chưa có dữ liệu</div>
              <div
                v-for="(item, itemIndex) in section.items"
                :key="item._refId"
                class="item-container relative flex flex-col pl-[20px]"
                style="width: calc(50% - 40px)"
              >
                <!-- DÒNG DUY NHẤT: Tiêu đề + Chấm tròn + Thanh ngang -->
                <div class="paginated-item w-full relative">
                  <div class="absolute -left-[20px] top-[10px] w-[6px] h-[6px] rounded-full bg-black"></div>
                  <div class="flex justify-between items-end mb-[10px]">
                    <span class="italic text-[#333] block leading-tight flex flex-col" :style="{ fontSize: '18px' }" v-html="formatDesc(item.name)"></span>
                    <span class="text-[14px] font-bold text-[#8da9c4] whitespace-nowrap ml-2"><span v-html="getLevelInfo(item.level).text"></span></span>
                  </div>
                  <div class="w-full bg-[#cbd5e0] rounded-full overflow-hidden" :style="{ height: '10px' }">
                    <div class="h-full rounded-full transition-all duration-500"
                         :style="{ width: getLevelInfo(item.level).percent, backgroundColor: '#8da9c4' }"></div>
                  </div>
                </div>
                
                <button
                  v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print" title="Xóa"
                >
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                  </svg>
                </button>
              </div>
            </div>

            <!-- CHỨNG CHỈ / GIẢI THƯỞNG / KHÁC (NGƯỜI THAM CHIẾU) -->
            <div v-else class="flex flex-col gap-[15px] w-full">
              <div v-if="!section.items || section.items.length === 0"
                   class="text-[#aaa] italic text-[14px] pl-[20px]">Chưa có dữ liệu</div>
              
              <div
                v-for="(item, itemIndex) in section.items"
                :key="item._refId"
                class="item-container relative w-full text-[#333] pl-[20px]"
              >
                <!-- DÒNG 1: Tiêu đề + Chấm tròn -->
                <div class="w-full flex justify-between items-start paginated-item mb-[2px] relative">
                  <div class="absolute -left-[20px] top-[8px] w-[6px] h-[6px] rounded-full bg-black"></div>
                  <div class="font-bold text-[16px] flex-1 flex flex-col" v-html="formatDesc(item.name || item.title)">
                  </div>
                  <div v-if="item.year || item.time" class="italic text-[14px] ml-4 shrink-0 text-[#666]">
                    <span v-html="item.year || item.time"></span>
                  </div>
                </div>

                <div v-if="item.info" class="text-[15px] font-medium text-[#555] mb-[2px] flex flex-col" v-html="formatDesc(item.info)">
                </div>

                <!-- BĂM MÔ TẢ THÀNH NHIỀU DÒNG ĐỂ TRÁNH TRÀN CHỮ -->
                <div v-if="item.desc || item.details" class="html-content text-justify w-full text-[#333] flex flex-col mt-1" :style="{ fontSize: '15px', lineHeight: '1.6' }" v-html="formatDesc(item.desc || item.details)"></div>
                
                <button
                  v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print" title="Xóa"
                >
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                  </svg>
                </button>
              </div>
            </div>

          </div>
        </template>
        </draggable>
      </div>
    </div>

    <!-- Page break markers -->
    <template v-for="p in (pageCount - 1)" :key="'div-' + p">
      <div
        class="absolute left-0 w-full z-50 flex flex-col items-center justify-center pointer-events-none no-print"
        :style="{ top: `calc(${p * 297}mm - 8px)` }"
      >
        <div class="w-[105%] h-[16px] bg-slate-800/95 shadow-inner overflow-hidden border-y border-black/30 backdrop-blur-sm"></div>
        <span class="absolute text-[9px] uppercase font-bold text-slate-300 tracking-widest bg-slate-700 px-3 py-0.5 rounded border border-slate-600 shadow-md">
          Ngắt trang {{ p + 1 }}
        </span>
      </div>
    </template>
  </div>
</template>

<script setup>
const props = defineProps({
  resumeData: { type: Object, required: true }
})
const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

import { computed, ref, onMounted, onUnmounted, nextTick, watch } from 'vue'
import draggable from 'vuedraggable'

const cvRoot            = ref(null)
const pageCount         = ref(1)
const selectedSectionId = ref(null)

// ─── CONTACT ITEMS LOGIC ───
const contactLabels = {
  dob: 'Ngày sinh',
  address: 'Địa chỉ',
  email: 'Email',
  phone: 'Số điện thoại'
}

const contactOrder = ref(['dob', 'address', 'email', 'phone'])
const hiddenContacts = ref([])

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  switch (key) {
    case 'phone': return g.phone
    case 'email': return g.email
    case 'dob': return g.dob || g.birthDate
    case 'address': return g.address
    default: return ''
  }
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
    } else if (key === 'dob') {
       if (props.resumeData.general.dob !== undefined) props.resumeData.general.dob = ''
       if (props.resumeData.general.birthDate !== undefined) props.resumeData.general.birthDate = ''
    }
    hiddenContacts.value.push(key)
    requestPagination()
  }
}

const toggleSection = (id) => {
  selectedSectionId.value = selectedSectionId.value === id ? null : id
  requestPagination()
}

const isEmpty = (val) => {
  if (!val) return true
  if (typeof val !== 'string') return false
  return val.replace(/<[^>]*>/g, '').trim() === ''
}

// BỘ LỌC CHUẨN: Băm nhỏ dòng thành các thẻ span để JS tóm được
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

const getLevelInfo = (level) => {
  if (!level || typeof level !== 'string') return { text: '', percent: '75%' }
  const l = level.toLowerCase().trim()
  const map = {
    'cơ bản':    { text: 'Cơ bản',    percent: '25%'  },
    'trung cấp': { text: 'Trung cấp', percent: '50%'  },
    'thành thạo':{ text: 'Thành thạo',percent: '75%'  },
    'chuyên gia':{ text: 'Chuyên gia',percent: '100%' },
  }
  if (map[l]) return map[l]
  const numMatch = level.match(/^(\d{1,3})%?$/)
  if (numMatch) {
    const n = Math.min(100, Math.max(0, parseInt(numMatch[1])))
    return { text: n + '%', percent: n + '%' }
  }
  return { text: '', percent: '75%' }
}

const templatePrimaryColor = computed(() => props.resumeData?.theme?.primaryColor || '#4a5568')

const DEFAULT_SECTION_IDS = ['summary', 'experience', 'education', 'skills']

const allVisibleSections = computed(() =>
  (props.resumeData?.sections || []).filter(s =>
    (s.isVisible && s.items?.length > 0) || DEFAULT_SECTION_IDS.includes(s.id) || (s.id === 'summary' && !isEmpty(props.resumeData.general.summary))
  )
)

const allVisibleSectionsWritable = ref([])

watch(allVisibleSections, (newVal) => {
  allVisibleSectionsWritable.value = [...newVal]
}, { immediate: true, deep: true })

const onDragEnd = () => {
  const newOrderIds = allVisibleSectionsWritable.value.map(s => s.id)
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

const sidebarIds = computed(() => allVisibleSections.value.filter(s => s.column === 'left').map(s => s.id))
const mainIds = computed(() => allVisibleSections.value.filter(s => s.column === 'right').map(s => s.id))

const swapSections = (sections, idxA, idxB) => {
  const a = sections[idxA]; const b = sections[idxB]
  sections.splice(idxA, 1, b); sections.splice(idxB, 1, a)
}

const moveSectionUp = (sectionId, column) => {
  const colSections = props.resumeData.sections.filter(s => s.column === column && s.id !== 'summary')
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
  const colSections = props.resumeData.sections.filter(s => s.column === column && s.id !== 'summary')
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

// ─── THUẬT TOÁN ĐO CHÍNH XÁC (KHÁNG ZOOM, DÙNG TRỰC TIẾP CHO TỪNG DÒNG LÁ) ──
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

  // Lọc chỉ lấy các lá ngoài cùng của paginated-item (như những CV trước)
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

  while (!stable && passes < 30) {
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

watch(() => [props.resumeData?.sections, props.resumeData?.general], () => requestPagination(), { deep: true })

onMounted(() => {
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
</script>

<style scoped>
#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

.item-container {
  position: relative;
  transition: background 0.15s;
}

.paginated-item {
  transition: none !important;
}

.delete-btn {
  position: absolute;
  right: -5px;
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
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.25);
  z-index: 30;
  transition: transform 0.12s, background 0.12s;
}
.delete-btn:hover  { background: #dc2626; transform: scale(1.12); }
.delete-btn:active { transform: scale(0.92); }

.section-block {
  position: relative;
  border: 2px solid transparent;
  border-radius: 6px;
  cursor: pointer;
  padding: 10px;
  transition: border-color 0.18s ease, border-radius 0.18s ease, box-shadow 0.2s ease, background 0.15s ease;
  background-color: transparent;
}

.section-block:hover { cursor: pointer; }
.section-selected {
  border-style: solid !important;
  border-radius: 6px !important;
  /* removed scale */
  background-color: rgba(0, 0, 0, 0.012) !important;
  z-index: 20;
  box-shadow: 0 4px 18px rgba(0, 0, 0, 0.07) !important;
}

.nav-btns {
  position: absolute;
  right: 8px;
  top: 8px;
  display: flex;
  flex-direction: row;
  gap: 5px;
  z-index: 9999;
}
.nav-btn {
  display: flex; align-items: center; justify-content: center; padding: 5px;
  background: var(--sel-color, #2563eb); color: white; border: none; border-radius: 4px;
  cursor: pointer; box-shadow: 0 2px 6px rgba(0, 0, 0, 0.22); transition: filter 0.12s, transform 0.1s;
}
.nav-btn:hover  { filter: brightness(0.88); }
.nav-btn:active { transform: scale(0.92); }

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
  background: white;
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

:deep(.html-content) { margin: 0 !important; padding: 0 !important; }
:deep(.html-content p) { margin: 0 !important; padding: 0 !important; }
:deep(.html-content ul) { list-style-type: disc !important; padding-left: 1.5rem !important; margin: 0 !important; }
:deep(.html-content ol) { list-style-type: decimal !important; padding-left: 1.5rem !important; margin: 0 !important; }
:deep(.html-content li) { margin-bottom: 2px !important; }
:deep(.html-content b), :deep(.html-content strong) { font-weight: bold; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic; }
:deep(.html-content u) { text-decoration: underline; }

@media print {
  .no-print { display: none !important; }
  .section-block, .section-selected {
    cursor: default !important; box-shadow: none !important; background: transparent !important;
    border-color: transparent !important; border-radius: 0 !important; padding: 0 !important;
    margin-bottom: 25px !important; transform: none !important;
  }
  .paginated-item { page-break-inside: avoid; break-inside: avoid; }
}
:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block), :global(.is-exporting-pdf .section-selected) {
  box-shadow: none !important; background: transparent !important; border-color: transparent !important;
  border-radius: 0 !important; transform: none !important;
}
</style>
