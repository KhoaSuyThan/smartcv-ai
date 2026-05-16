<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-col relative box-border leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', \'Segoe UI\', sans-serif', color: '#333' }"
    @click.self="selectedSectionId = null"
  >
    <!-- TOP BORDER BAR -->
    <div class="w-full" style="height: 5px; background-color: #1a56db; flex-shrink: 0;"></div>

    <!-- HEADER -->
    <header class="relative pt-[10mm] px-[12mm] pb-[4mm] flex items-start justify-between paginated-item">
      <!-- Left: Name + Job Title -->
      <div class="flex-1 pr-4">
        <h1 class="font-black uppercase tracking-wide leading-tight mb-1"
          style="font-size: 26px !important; color: #1a56db;"
          v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'NGUYỄN TÙNG DƯƠNG'">
        </h1>
        <h2 class="font-semibold uppercase tracking-widest"
          style="font-size: 11px !important; color: #555;"
          v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'NHÂN VIÊN KINH DOANH'">
        </h2>
      </div>
      <!-- Right: Avatar -->
      <div class="flex-shrink-0">
        <div class="w-[28mm] h-[28mm] rounded-full overflow-hidden border-[3px] bg-slate-100"
          style="border-color: #1a56db;">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center text-slate-300">
            <svg class="w-12 h-12" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
            </svg>
          </div>
        </div>
      </div>
    </header>

    <!-- INFO BAR -->
    <div v-if="contactItems.length > 0" class="mx-[12mm] mb-[5mm] border border-slate-200 rounded-sm paginated-item contact-block relative"
         :class="{ 'contact-active': selectedSectionId === 'contact' }"
         @click.stop="selectedSectionId = selectedSectionId === 'contact' ? null : 'contact'"
         style="font-size: 10.5px !important;">
      <div class="flex flex-wrap">
        <div v-for="(ci, ciIdx) in contactItems" :key="ci.key"
             class="flex flex-col px-4 py-2 border-r border-slate-200 relative contact-item-container"
             style="border-left: 3px solid #1a56db;">
          <span class="font-bold text-slate-500 uppercase mb-0.5" style="font-size: 9px !important;">{{ ci.label }}</span>
          <span class="font-medium text-slate-700" v-html="ci.value"></span>
          
          <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print" style="right: 2px; top: 2px; transform: none;">
            <button v-if="ciIdx > 0" @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn" title="Lên" style="padding:2px; width:18px; height:18px;">
              <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
            </button>
            <button v-if="ciIdx < contactItems.length - 1" @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn" title="Xuống" style="padding:2px; width:18px; height:18px;">
              <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
            </button>
            <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn-danger" title="Xóa" style="padding:2px; width:18px; height:18px;">
              <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- BODY: 2 COLUMNS -->
    <div class="flex px-[12mm] gap-[8mm] flex-1" @click.self="selectedSectionId = null">

      <!-- CỘT TRÁI (MAIN - rộng hơn) -->
      <main class="flex-[1.6] flex flex-col gap-4" @click.self="selectedSectionId = null">
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
              class="text-justify leading-relaxed html-content"
              style="font-size: 11px !important; color: #444;"
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
                <div class="paginated-item">
                  <div class="flex justify-between items-start mb-0.5">
                    <span class="font-bold text-slate-800" style="font-size: 12px !important;">{{ item.school || 'Tên trường' }}</span>
                    <span class="font-bold px-2 py-0.5 rounded text-white text-[9px] shrink-0 ml-2" style="background-color: #1a56db;">{{ item.year || '2017-2021' }}</span>
                  </div>
                  <div class="font-semibold" style="font-size: 11px !important; color: #1a56db;">{{ item.major || 'Chuyên ngành' }}</div>
                  <div v-if="item.gradType" class="text-slate-500" style="font-size: 10.5px !important;">{{ item.gradType }}</div>
                  <div v-if="item.desc" class="html-content mt-1 text-slate-600" style="font-size: 10.5px !important;" v-html="formatDesc(item.desc)"></div>
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
                <div class="paginated-item">
                  <div class="flex justify-between items-start mb-0.5">
                    <span class="font-bold text-slate-800 uppercase" style="font-size: 12px !important;">{{ item.company || 'Tên công ty' }}</span>
                    <span class="font-bold px-2 py-0.5 rounded text-white text-[9px] shrink-0 ml-2" style="background-color: #1a56db;">{{ item.time || '2024-Nay' }}</span>
                  </div>
                  <div class="font-semibold mb-1" style="font-size: 11px !important; color: #1a56db;">{{ item.role || 'Vị trí' }}</div>
                </div>
                <div class="html-content leading-relaxed text-slate-600" style="font-size: 11px !important;" v-html="formatDesc(item.desc || '')"></div>
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
                <div class="html-content text-slate-700" style="font-size: 11px !important;" v-html="formatDesc(item.desc || item.info || item.name)"></div>
              </div>
            </div>
          </div>
        </template>
      </main>

      <!-- CỘT PHẢI (SIDEBAR - hẹp hơn) -->
      <aside class="flex-[0.9] flex flex-col gap-4" @click.self="selectedSectionId = null">
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
            <div v-if="['awards','certifications'].includes(section.id)" class="space-y-3">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container relative">
                <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                <div v-if="item.year" class="font-bold" style="font-size: 10px !important; color: #1a56db;">{{ item.year }}</div>
                <div class="font-medium text-slate-800" style="font-size: 11px !important;">{{ item.name || item.info }}</div>
                <div v-if="item.organization" class="text-slate-500 italic" style="font-size: 10px !important;">{{ item.organization }}</div>
              </div>
            </div>

            <!-- Fallback sidebar -->
            <div v-else class="space-y-2">
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

    <!-- VIỀN DƯỚI TRANG -->
    <template v-for="p in pageCount" :key="'footer-' + p">
      <div class="absolute left-0 w-full pointer-events-none z-40"
        :style="{ top: `calc(${p * 297}mm - 10mm)`, height: '1px' }">
        <div class="w-full h-full opacity-20" style="background: #1a56db;"></div>
      </div>
    </template>

    <!-- ĐƯỜNG PHÂN TRANG -->
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

// ─── CONTACT ITEMS: Danh sách động có thể sắp xếp / ẩn ───
const contactIcons = {
  birthDate: '<path d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" />',
  phone: '<path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z" />',
  email: '<path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z" /><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z" />',
  address: '<path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd" />',
  website: '<path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM4.332 8.027a6.012 6.012 0 011.912-2.706C6.512 5.73 6.974 6 7.5 6A1.5 1.5 0 019 7.5V8a2 2 0 004 0 2 2 0 011.523-1.943A5.977 5.977 0 0116 10c0 .34-.028.675-.083 1H15a2 2 0 00-2 2v2.197A5.973 5.973 0 0110 16v-2a2 2 0 00-2-2 2 2 0 01-2-2 2 2 0 00-1.668-1.973z" clip-rule="evenodd" />'
}

const contactOrder = ref(['birthDate', 'email', 'phone', 'website', 'address'])
const hiddenContacts = ref([])

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

const contactItems = computed(() => {
  return contactOrder.value
    .filter(key => !hiddenContacts.value.includes(key))
    .filter(key => !isEmpty(getContactValue(key)))
    .map(key => ({
      key,
      label: getContactLabel(key),
      icon: contactIcons[key],
      value: getContactValue(key)
    }))
})

const moveContactUp = (idx) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && getContactValue(k) !== '')
  if (idx <= 0) return
  const keyA = visible[idx], keyB = visible[idx - 1]
  const idxA = contactOrder.value.indexOf(keyA), idxB = contactOrder.value.indexOf(keyB)
  const arr = [...contactOrder.value]
  ;[arr[idxA], arr[idxB]] = [arr[idxB], arr[idxA]]
  contactOrder.value = arr
}

const moveContactDown = (idx) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && getContactValue(k) !== '')
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
    } else if (key === 'website') {
       if (props.resumeData.general.github !== undefined) props.resumeData.general.github = ''
       if (props.resumeData.general.linkedin !== undefined) props.resumeData.general.linkedin = ''
       if (props.resumeData.general.website !== undefined) props.resumeData.general.website = ''
    }
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
  tempDiv.querySelectorAll('li').forEach(li => {
    const child = li.firstElementChild
    if (child && (child.tagName === 'FONT' || child.tagName === 'SPAN')) {
      if (child.style && child.style.color) li.style.color = child.style.color
    }
  })
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
    props.resumeData.sections.forEach(sec => {
      if (sec.isVisible === undefined) {
        sec.isVisible = [...LEFT, ...RIGHT].includes(sec.id)
        sec.column = RIGHT.includes(sec.id) ? 'right' : 'left'
      } else {
        if (LEFT.includes(sec.id)) {
          sec.column = 'left'
        } else if (RIGHT.includes(sec.id)) {
          sec.column = 'right'
        }
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
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&display=swap');

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

.section-title {
  font-size: 13px !important;
  font-weight: 800 !important;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: #1a56db !important;
  border-bottom: 1.5px solid #1a56db;
  padding-bottom: 4px;
  margin-bottom: 10px;
}

.section-block {
  position: relative;
  padding: 10px;
  border-radius: 6px;
  border: 2px solid transparent;
  cursor: pointer;
  transition: border-color 0.15s ease;
}

.section-active {
  border-color: #93c5fd !important;
  background: rgba(219, 234, 254, 0.3) !important;
  border-radius: 6px !important;
}

.contact-block {
  border-radius: 6px;
  border: 2px solid transparent;
  cursor: pointer;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

.contact-block.contact-active {
  border: 2px solid #1a56db !important;
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

.paginated-item { transition: none; }

.nav-btns {
  position: absolute;
  right: 6px;
  top: 6px;
  display: flex;
  flex-direction: row;
  gap: 4px;
  z-index: 9999;
}

.nav-btn {
  width: 24px;
  height: 24px;
  background: #1d4ed8;
  color: white;
  border: none;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: background 0.1s;
}
.nav-btn:hover { background: #1e40af; }
.nav-btn-danger { background: #ef4444 !important; }
.nav-btn-danger:hover { background: #dc2626 !important; }

.item-container { position: relative; }

.delete-btn {
  position: absolute;
  top: 0;
  right: 0;
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
.delete-btn:hover { background: #dc2626; transform: scale(1.1); }

:deep(.html-content ul) { list-style-type: disc !important; padding-left: 1.2rem !important; margin-top: 0.2rem; }
:deep(.html-content ol) { list-style-type: decimal !important; padding-left: 1.2rem !important; margin-top: 0.2rem; }
:deep(.html-content li) { margin-bottom: 0.2rem; }
:deep(.html-content b), :deep(.html-content strong) { font-weight: 700 !important; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic !important; }
:deep(.html-content u) { text-decoration: underline !important; }

@media print {
  .no-print { display: none !important; }
  .section-block, .section-block.section-active {
    cursor: default !important;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
    padding: 0 !important;
    border-radius: 0 !important;
  }
}

:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .section-block.section-active) {
  cursor: default !important;
  box-shadow: none !important;
  background: transparent !important;
  border-color: transparent !important;
}
</style>
