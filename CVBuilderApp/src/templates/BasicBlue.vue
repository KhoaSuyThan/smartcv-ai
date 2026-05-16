<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-col relative box-border leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', \'Segoe UI\', sans-serif', color: '#333' }"
    @click.self="selectedSectionId = null"
  >
    <!-- TOP BORDER BAR -->
    <div class="w-full" style="height: 5px; background-color: #1a73e8; flex-shrink: 0;"></div>

    <!-- HEADER -->
    <header class="relative pt-[12mm] px-[12mm] pb-[6mm] flex items-start justify-between paginated-item">
      <!-- Left: Name + Job Title -->
      <div class="flex-1 pr-4">
        <h1 class="font-black uppercase tracking-tight leading-tight mb-2"
          style="font-size: 32px !important; color: #1a73e8;"
          v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'NGUYỄN TÙNG DƯƠNG'">
        </h1>
        <h2 class="font-bold uppercase tracking-wider mb-6"
          style="font-size: 16px !important; color: #1a73e8;"
          v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'NHÂN VIÊN KINH DOANH'">
        </h2>

        <!-- CONTACT GRID -->
        <div v-if="contactItems.length > 0" 
             class="grid grid-cols-3 gap-y-3 gap-x-2 paginated-item contact-block relative"
             :class="{ 'contact-active': selectedSectionId === 'contact' }"
             @click.stop="selectedSectionId = selectedSectionId === 'contact' ? null : 'contact'"
             style="font-size: 11px !important;">
          <div v-for="(ci, ciIdx) in contactItems" :key="ci.key"
               class="flex flex-col pl-3 border-l-[3px] border-[#1a73e8] relative contact-item-container py-1">
            <span class="font-bold text-[#1a73e8] mb-0.5" style="font-size: 10px !important;">{{ ci.label }}</span>
            <span class="font-medium text-slate-700 break-words" v-html="ci.value"></span>
            
            <!-- Controls -->
            <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print flex gap-1" style="right: 2px; top: 2px;">
              <!-- Move Up -->
              <button v-if="ciIdx >= 3" @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn" title="Lên" style="width:16px; height:16px; padding:0;">
                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <!-- Move Down -->
              <button v-if="ciIdx + 3 < contactItems.length" @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn" title="Xuống" style="width:16px; height:16px; padding:0;">
                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <!-- Move Left -->
              <button v-if="ciIdx % 3 !== 0" @click.stop.prevent="moveContactLeft(ciIdx)" class="nav-btn" title="Sang trái" style="width:16px; height:16px; padding:0;">
                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
              </button>
              <!-- Move Right -->
              <button v-if="ciIdx % 3 !== 2 && ciIdx + 1 < contactItems.length" @click.stop.prevent="moveContactRight(ciIdx)" class="nav-btn" title="Sang phải" style="width:16px; height:16px; padding:0;">
                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
              </button>
              <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn-danger" title="Xóa" style="width:16px; height:16px; padding:0;">
                <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Right: Avatar -->
      <div class="flex-shrink-0 pt-2">
        <div class="w-[32mm] h-[32mm] rounded-full overflow-hidden border-[1px] border-slate-200 shadow-sm"
          style="background-color: #f8fafc;">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center text-slate-300">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
            </svg>
          </div>
        </div>
      </div>
    </header>

    <!-- BODY: 2 COLUMNS -->
    <div class="flex px-[12mm] gap-[10mm] flex-1 pb-[10mm]" @click.self="selectedSectionId = null">

      <!-- LEFT COLUMN (MAIN) -->
      <main class="flex-[1.4] flex flex-col gap-4" @click.self="selectedSectionId = null">
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block relative"
            :class="{ 'section-active': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <!-- Nav Buttons -->
            <div v-show="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Lên">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Xuống">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
              </button>
              <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <!-- Section Title -->
            <div class="paginated-item">
              <h3 class="section-title">{{ section.title }}</h3>
            </div>

            <!-- Summary -->
            <div v-if="section.id === 'summary'"
              class="text-justify leading-relaxed html-content pr-2"
              style="font-size: 11.5px !important; color: #333;"
              v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Mô tả mục tiêu nghề nghiệp của bạn...')">
            </div>

            <!-- Education -->
            <div v-else-if="section.id === 'education'" class="space-y-4">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                <div class="paginated-item pr-2">
                  <div class="flex justify-between items-start mb-1">
                    <span class="font-bold text-[#1a73e8]" style="font-size: 13.5px !important;">{{ item.school || 'Tên trường' }}</span>
                    <span class="font-bold px-3 py-1 rounded-full bg-[#f1f5f9] text-[#1a73e8] text-[10px] shrink-0 ml-4 border border-[#e2e8f0]">{{ item.year || '2017-2021' }}</span>
                  </div>
                  <div class="font-bold text-slate-700" style="font-size: 12px !important;">{{ item.major || 'Chuyên ngành' }}</div>
                  <div v-if="item.desc" class="html-content mt-1.5 text-slate-600 leading-relaxed" style="font-size: 11px !important;" v-html="formatDesc(item.desc)"></div>
                </div>
              </div>
            </div>

            <!-- Experience -->
            <div v-else-if="section.id === 'experience'" class="space-y-5">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                <div class="paginated-item pr-2">
                  <div class="flex justify-between items-start mb-1">
                    <span class="font-bold text-[#1a73e8] uppercase" style="font-size: 13.5px !important;">{{ item.company || 'Tên công ty' }}</span>
                    <span class="font-bold px-3 py-1 rounded-full bg-[#f1f5f9] text-[#1a73e8] text-[10px] shrink-0 ml-4 border border-[#e2e8f0]">{{ item.time || '2024-Nay' }}</span>
                  </div>
                  <div class="font-bold text-slate-700 mb-2" style="font-size: 12px !important;">{{ item.role || 'Vị trí' }}</div>
                  <div class="html-content leading-relaxed text-slate-600" style="font-size: 11.5px !important;" v-html="formatDesc(item.desc || '')"></div>
                </div>
              </div>
            </div>

            <!-- Fallback -->
            <div v-else class="space-y-3">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container relative">
                <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                <div class="html-content text-slate-700 pr-2" style="font-size: 11.5px !important;" v-html="formatDesc(item.desc || item.info || item.name)"></div>
              </div>
            </div>
          </div>
        </template>
      </main>

      <!-- RIGHT COLUMN (SIDEBAR) -->
      <aside class="flex-[1] flex flex-col gap-4" @click.self="selectedSectionId = null">
        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block relative"
            :class="{ 'section-active': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <!-- Nav Buttons -->
            <div v-show="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Lên">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Xuống">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
              </button>
              <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <!-- Section Title -->
            <div class="paginated-item">
              <h3 class="section-title">{{ section.title }}</h3>
            </div>

            <!-- Awards / Certifications -->
            <div v-if="['awards','certifications'].includes(section.id)" class="space-y-4">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container relative">
                <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                <div v-if="item.year" class="font-bold mb-0.5" style="font-size: 11.5px !important; color: #333;">{{ item.year }}:</div>
                <div class="font-medium text-slate-700 leading-relaxed" style="font-size: 11px !important;">
                  {{ item.name || item.info }}
                  <span v-if="item.organization" class="text-slate-500 italic"> - {{ item.organization }}</span>
                </div>
              </div>
            </div>

            <!-- Fallback sidebar -->
            <div v-else class="space-y-3">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container relative">
                <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                <div class="html-content text-slate-700" style="font-size: 11px !important;" v-html="formatDesc(item.desc || item.info || item.name)"></div>
              </div>
            </div>
          </div>
        </template>
      </aside>
    </div>

    <!-- PAGE BREAK INDICATORS -->
    <template v-for="p in (pageCount - 1)" :key="'div-' + p">
      <div class="absolute left-0 w-full z-50 flex flex-col items-center justify-center pointer-events-none no-print"
        :style="{ top: `calc(${p * 297}mm - 8px)` }">
        <div class="w-[105%] h-[16px] bg-slate-800/95 shadow-inner border-y border-black/30"></div>
        <span class="absolute text-[9px] uppercase font-bold text-slate-300 tracking-widest bg-slate-700 px-3 py-0.5 rounded border border-slate-600">Ngắt trang {{ p + 1 }}</span>
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

const cvRoot = ref(null)
const pageCount = ref(1)
const selectedSectionId = ref(null)

const getContactLabel = (key) => {
  switch (key) {
    case 'birthDate': return 'Ngày sinh'
    case 'phone': return 'Điện thoại'
    case 'email': return 'Email'
    case 'address': return 'Địa chỉ'
    case 'website': return 'Website'
    default: return ''
  }
}

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  switch (key) {
    case 'birthDate': return isEmpty(g.birthDate) ? '' : g.birthDate
    case 'phone': return isEmpty(g.phone) ? '' : g.phone
    case 'email': return isEmpty(g.email) ? '' : g.email
    case 'address': return isEmpty(g.address) ? '' : g.address
    case 'website': return g.website || g.github || g.linkedin || ''
    default: return ''
  }
}

const contactOrder = ref(['birthDate', 'email', 'phone', 'website', 'address'])
const hiddenContacts = ref([])

const contactItems = computed(() => {
  return contactOrder.value
    .filter(key => !hiddenContacts.value.includes(key))
    .filter(key => !isEmpty(getContactValue(key)))
    .map(key => ({
      key,
      label: getContactLabel(key),
      value: getContactValue(key)
    }))
})

const moveContactUp = (idx) => {
  if (idx < 3) return
  swapContact(idx, idx - 3)
}

const moveContactDown = (idx) => {
  if (idx + 3 >= contactItems.value.length) return
  swapContact(idx, idx + 3)
}

const moveContactLeft = (idx) => {
  if (idx % 3 === 0) return
  swapContact(idx, idx - 1)
}

const moveContactRight = (idx) => {
  if (idx % 3 === 2 || idx + 1 >= contactItems.value.length) return
  swapContact(idx, idx + 1)
}

const swapContact = (idxA, idxB) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && !isEmpty(getContactValue(k)))
  const keyA = visible[idxA], keyB = visible[idxB]
  const realIdxA = contactOrder.value.indexOf(keyA)
  const realIdxB = contactOrder.value.indexOf(keyB)
  const arr = [...contactOrder.value]
  ;[arr[realIdxA], arr[realIdxB]] = [arr[realIdxB], arr[realIdxA]]
  contactOrder.value = arr
}

const removeContactItem = (idx) => {
  const visible = contactItems.value
  if (idx >= 0 && idx < visible.length) {
    const key = visible[idx].key
    hiddenContacts.value.push(key)
    requestPagination()
  }
}

const isEmpty = (val) => {
  if (!val) return true
  if (typeof val !== 'string') return false
  return val.replace(/<[^>]*>/g, '').trim() === ''
}

const sidebarSections = computed(() => props.resumeData.sections.filter(s => s.column === 'right'))
const mainSections = computed(() => props.resumeData.sections.filter(s => s.column === 'left'))
const sidebarIds = computed(() => sidebarSections.value.map(s => s.id))
const mainIds = computed(() => mainSections.value.map(s => s.id))

const formatDesc = (text) => {
  if (!text) return ''
  if (!/(<[a-z][\s\S]*>)/i.test(text)) {
    return text.split('\n').map(l => l.trim()).filter(Boolean)
      .map(l => `<div class="paginated-item">${l}</div>`).join('')
  }
  const tempDiv = document.createElement('div')
  tempDiv.innerHTML = text
  const container = document.createElement('div')
  Array.from(tempDiv.childNodes).forEach(node => {
    if (node.nodeType === Node.TEXT_NODE) {
      if (node.textContent.trim()) {
        const div = document.createElement('div')
        div.className = 'paginated-item'
        div.appendChild(node.cloneNode(true))
        container.appendChild(div)
      }
    } else if (node.nodeType === Node.ELEMENT_NODE) {
      if (node.tagName === 'BR') {
        const div = document.createElement('div')
        div.className = 'paginated-item h-[12px]'
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

  const A4_W_MM = 210
  const A4_H_MM = 297
  const pxPerMm = cvRoot.value.offsetWidth / A4_W_MM
  const pageH = A4_H_MM * pxPerMm
  const bottomSafeZone = 14 * pxPerMm
  const topMargin = 15 * pxPerMm

  const getOffsetTop = (el) => {
    let offset = 0, curr = el
    while (curr && curr !== cvRoot.value) { offset += curr.offsetTop; curr = curr.offsetParent }
    return offset
  }

  allElements.forEach((el) => {
    if (el.offsetHeight === 0) return
    const top = getOffsetTop(el)
    const topInPage = top % pageH
    const bottomInPage = topInPage + el.offsetHeight
    if (bottomInPage > (pageH - bottomSafeZone)) {
      el.style.marginTop = `${pageH - topInPage + topMargin}px`
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
    const LEFT = ['summary', 'education', 'experience']
    const RIGHT = ['awards', 'certifications']
    const ALL_REQUIRED = [...LEFT, ...RIGHT]
    
    props.resumeData.sections.forEach(sec => {
      sec.isVisible = ALL_REQUIRED.includes(sec.id)
      if (LEFT.includes(sec.id)) {
        sec.column = 'left'
      } else if (RIGHT.includes(sec.id)) {
        sec.column = 'right'
      }
    })
  }
  requestPagination()
  window.addEventListener('resize', requestPagination)
})

onUnmounted(() => {
  window.removeEventListener('resize', requestPagination)
  if (paginateTimer) clearTimeout(paginateTimer)
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&display=swap');

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

.section-title {
  font-size: 15px !important;
  font-weight: 800 !important;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: #1a73e8 !important;
  border-bottom: 2px solid #1a73e8;
  padding-bottom: 6px;
  margin-bottom: 10px;
}

.section-block {
  position: relative;
  padding: 6px;
  border-radius: 4px;
  border: 1px solid transparent;
  cursor: pointer;
  transition: all 0.15s ease;
}

.section-active {
  border-color: #93c5fd !important;
  background: rgba(239, 246, 255, 0.5) !important;
}

.contact-block {
  padding: 6px;
  border-radius: 4px;
  border: 1px solid transparent;
  cursor: pointer;
}

.contact-block.contact-active {
  border: 1px solid #93c5fd !important;
  background: rgba(239, 246, 255, 0.5) !important;
}

.contact-item-btns {
  position: absolute;
  z-index: 10;
}

.nav-btns {
  position: absolute;
  right: 6px;
  top: 6px;
  display: flex;
  flex-direction: row;
  gap: 4px;
  z-index: 99;
}

.nav-btn {
  width: 22px;
  height: 22px;
  background: #1a73e8;
  color: white;
  border: none;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}
.nav-btn:hover { background: #1557b0; }
.nav-btn-danger { background: #ef4444 !important; }

.delete-btn {
  position: absolute;
  top: -5px;
  right: -5px;
  background: #ef4444;
  color: white;
  border: none;
  border-radius: 50%;
  width: 18px;
  height: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 30;
}

:deep(.html-content ul) { list-style-type: disc !important; padding-left: 1.2rem !important; }
:deep(.html-content ol) { list-style-type: decimal !important; padding-left: 1.2rem !important; }
:deep(.html-content li) { margin-bottom: 0.4rem; }
:deep(.html-content b), :deep(.html-content strong) { font-weight: 700 !important; }

@media print {
  .no-print { display: none !important; }
  .section-block, .contact-block { border: none !important; background: transparent !important; padding: 0 !important; }
  .nav-btns, .delete-btn { display: none !important; }
}
</style>
