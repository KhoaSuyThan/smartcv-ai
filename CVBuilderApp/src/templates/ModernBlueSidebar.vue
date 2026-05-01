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
      <div class="section-block mb-4">
        <h3 class="section-title paginated-item font-bold border-b-2 mb-2 pb-1 uppercase" :style="{ color: templatePrimaryColor, borderColor: templatePrimaryColor }">
          Thông tin cá nhân
        </h3>
        <ul class="w-full list-none p-0 m-0 text-slate-700 space-y-3" :style="{ fontSize: '13px' }">
          
          <li class="flex items-center gap-3 paginated-item">
            <span class="icon-wrap text-blue-500"><svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20"><path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z" /></svg></span>
            <span v-html="!isEmpty(resumeData.general.phone) ? resumeData.general.phone : '0123.456.789'" />
          </li>

          <li class="flex items-center gap-3 paginated-item">
            <span class="icon-wrap text-blue-500"><svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20"><path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z" /><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z" /></svg></span>
            <span class="break-all" v-html="!isEmpty(resumeData.general.email) ? resumeData.general.email : 'email@example.com'" />
          </li>

          <li class="flex items-center gap-3 paginated-item" v-if="!isEmpty(resumeData.general.dob)">
            <span class="icon-wrap text-blue-500"><svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20"><path d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" /></svg></span>
            <span v-html="resumeData.general.dob" />
          </li>

          <li v-if="!isEmpty(resumeData.general.gender)" class="flex items-center gap-3 paginated-item">
            <span class="icon-wrap text-blue-500"><svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20"><path d="M10 2a8 8 0 100 16 8 8 0 000-16zM7 9a3 3 0 116 0 3 3 0 01-6 0z" /></svg></span>
            <span v-html="resumeData.general.gender" />
          </li>

          <li class="flex items-center gap-3 paginated-item">
            <span class="icon-wrap text-blue-500"><svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd" /></svg></span>
            <span v-html="!isEmpty(resumeData.general.address) ? resumeData.general.address : 'TP. Hồ Chí Minh'" />
          </li>

        </ul>
      </div>

      <!-- Sidebar Sections -->
      <div class="w-full flex-1 flex flex-col gap-2">
        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-if="section.isVisible"
            class="section-block group"
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
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7-7" /></svg>
              </button>
            </div>

            <h3 class="section-title paginated-item" :style="{ color: templatePrimaryColor, borderBottomColor: templatePrimaryColor }">
              {{ section.title }}
            </h3>

            <div class="space-y-4 px-1">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative text-slate-800">
                <!-- ĐÃ SỬA: Dùng formatDesc và flex-col để băm nhỏ dòng ở Cột Trái -->
                <div class="html-content font-bold text-[14px] leading-snug flex flex-col" v-html="formatDesc(item.name)" />
                <div v-if="!isEmpty(item.info || item.level)" class="html-content text-[12px] text-slate-500 mt-0.5 flex flex-col"
                     v-html="formatDesc(item.info || item.level)" />
                
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
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-if="section.isVisible"
            class="section-block group"
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
              {{ section.title }}
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
                      {{ section.id === 'education' ? item.school : (item.company || item.name) }}
                    </div>
                    <div v-if="item.year || item.time" class="shrink-0 font-bold text-[13px] text-slate-400 uppercase tracking-wider">
                      {{ item.year || item.time }}
                    </div>
                  </div>
                  <div v-if="item.major || item.role" class="font-semibold text-slate-500 italic text-[14px] mb-2 paginated-item">
                    {{ item.major || item.role }}
                  </div>
                  <div v-if="item.desc" class="html-content text-justify text-[14px] leading-relaxed flex flex-col" v-html="formatDesc(item.desc)" />
                  <div v-else-if="item.gradType" class="text-blue-500 font-semibold text-[13px] paginated-item">{{ item.gradType }}</div>
                </template>

                <!-- Generic -->
                <template v-else>
                  <div class="flex gap-4">
                    <div v-if="item.year || item.time" class="shrink-0 font-bold text-blue-500 text-[14px] paginated-item"
                         v-html="safeHtml(item.year || item.time)" />
                    <div class="flex-1">
                      <div v-if="!isEmpty(item.name)" class="html-content font-bold text-[15px] mb-1 paginated-item"
                           v-html="safeHtml(item.name)" />
                      <div v-if="!isEmpty(item.desc || item.info)"
                           class="html-content text-justify text-[14px] leading-relaxed flex flex-col"
                           v-html="formatDesc(item.desc || item.info)" />
                    </div>
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

const props = defineProps({
  resumeData: { type: Object, required: true }
})

const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

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
    sec.isVisible = defaultVisible.has(sec.id) || hasData
  })

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