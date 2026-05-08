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
    <div class="mx-[12mm] mb-[5mm] border border-slate-200 rounded-sm paginated-item" style="font-size: 10.5px !important;">
      <div class="flex flex-wrap">
        <!-- Ngày sinh -->
        <div v-if="!isEmpty(resumeData.general.birthDate)" class="flex flex-col px-4 py-2 border-r border-slate-200" style="border-left: 3px solid #1a56db;">
          <span class="font-bold text-slate-500 uppercase mb-0.5" style="font-size: 9px !important;">Ngày sinh</span>
          <span class="font-medium text-slate-700" v-html="resumeData.general.birthDate"></span>
        </div>
        <!-- Email -->
        <div v-if="!isEmpty(resumeData.general.email)" class="flex flex-col px-4 py-2 border-r border-slate-200" style="border-left: 3px solid #1a56db;">
          <span class="font-bold text-slate-500 uppercase mb-0.5" style="font-size: 9px !important;">Email</span>
          <span class="font-medium text-slate-700" v-html="resumeData.general.email"></span>
        </div>
        <!-- Điện thoại -->
        <div v-if="!isEmpty(resumeData.general.phone)" class="flex flex-col px-4 py-2 border-r border-slate-200" style="border-left: 3px solid #1a56db;">
          <span class="font-bold text-slate-500 uppercase mb-0.5" style="font-size: 9px !important;">Điện thoại</span>
          <span class="font-medium text-slate-700" v-html="resumeData.general.phone"></span>
        </div>
        <!-- Website -->
        <div v-if="!isEmpty(resumeData.general.website)" class="flex flex-col px-4 py-2 border-r border-slate-200" style="border-left: 3px solid #1a56db;">
          <span class="font-bold text-slate-500 uppercase mb-0.5" style="font-size: 9px !important;">Website</span>
          <span class="font-medium text-slate-700">{{ resumeData.general.website }}</span>
        </div>
        <!-- Địa chỉ -->
        <div v-if="!isEmpty(resumeData.general.address)" class="flex flex-col px-4 py-2" style="border-left: 3px solid #1a56db;">
          <span class="font-bold text-slate-500 uppercase mb-0.5" style="font-size: 9px !important;">Địa chỉ</span>
          <span class="font-medium text-slate-700" v-html="resumeData.general.address"></span>
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
              <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn">
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
              <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn">
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
      sec.isVisible = [...LEFT, ...RIGHT].includes(sec.id)
      sec.column = RIGHT.includes(sec.id) ? 'right' : 'left'
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
