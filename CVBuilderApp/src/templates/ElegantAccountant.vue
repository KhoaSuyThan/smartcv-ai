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
    <div class="absolute top-0 right-0 w-[200px] h-[200px] pointer-events-none no-print opacity-80 z-0">
      <svg width="200" height="200" viewBox="0 0 200 200" fill="none" xmlns="http://www.w3.org/2000/svg">
        <path d="M180 20C160 50 140 30 120 60C100 90 130 110 110 140C90 170 60 150 40 180"
              stroke="#333" stroke-width="1" stroke-linecap="round" stroke-dasharray="2 4"/>
        <path d="M190 30C175 55 160 45 145 70C130 95 150 110 135 135"
              stroke="#333" stroke-width="0.5" opacity="0.4"/>
        <circle cx="180" cy="20" r="2" fill="#333"/>
      </svg>
    </div>
    <div class="absolute bottom-[50px] right-[50px] w-[150px] h-[100px] pointer-events-none no-print opacity-20 z-0">
      <svg width="150" height="100" viewBox="0 0 150 100" fill="none" xmlns="http://www.w3.org/2000/svg">
        <ellipse cx="40"  cy="50" rx="35" ry="45" stroke="#333" stroke-width="0.5" stroke-dasharray="4 2"/>
        <ellipse cx="75"  cy="50" rx="35" ry="45" stroke="#333" stroke-width="0.5" stroke-dasharray="4 2"/>
        <ellipse cx="110" cy="50" rx="35" ry="45" stroke="#333" stroke-width="0.5" stroke-dasharray="4 2"/>
      </svg>
    </div>

    <div
      class="w-full h-full box-border flex flex-col relative z-10"
      :style="{ padding: '20mm 20mm 20mm 20mm' }"
    >

      <header class="paginated-item w-full flex justify-between items-start mb-[40px] gap-[20px]">

        <div class="flex-[1.4] flex flex-col">
          <div class="w-fit mb-[25px]">
            <h1
              class="m-0 leading-tight break-words"
              :style="{ fontSize: '64px', color: '#4a5568', fontFamily: 'Georgia, serif', fontWeight: '400' }"
            >{{ isEmpty(resumeData.general.fullName) ? 'Họ Và Tên Ứng Viên' : resumeData.general.fullName }}</h1>
            <div class="h-[1px] w-full bg-[#cbd5e0] mt-[5px]"></div>
          </div>

          <div class="flex flex-col gap-[8px] text-[#333]" :style="{ fontSize: '14px' }">
            <div class="flex gap-[5px]">
              <span class="font-bold shrink-0">Ngày sinh:</span>
              <span>27/01/1998</span>
            </div>
            <div class="flex gap-[5px]">
              <span class="font-bold shrink-0">Địa chỉ:</span>
              <span>TP. Hồ Chí Minh</span>
            </div>
            <div class="flex gap-[5px]">
              <span class="font-bold shrink-0">Email:</span>
              <span>email@example.com</span>
            </div>
            <div class="flex gap-[5px]">
              <span class="font-bold shrink-0">Số điện thoại:</span>
              <span>0123.456.789</span>
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

      <div class="w-full flex flex-col m-0 p-0">
        <template v-for="section in allVisibleSections" :key="section.id">
          <div
            class="section-block relative w-full mb-[30px]"
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
            </div>

            <h3
              class="paginated-item uppercase w-full block break-words"
              :style="{ fontSize: '28px', color: '#222', marginBottom: '15px', fontWeight: 'bold', letterSpacing: '1px' }"
            >{{ section.title }}</h3>

            <div
              v-if="section.id === 'experience'"
              :class="['grid gap-x-[60px] gap-y-[25px] w-full items-start',
                       (section.items || []).length >= 2 ? 'grid-cols-2' : 'grid-cols-1']"
            >
              <div v-if="!section.items || section.items.length === 0"
                   class="text-[#aaa] italic text-[14px] pl-[20px]">Chưa có dữ liệu</div>
              <div
                v-for="(item, itemIndex) in section.items"
                :key="item._refId"
                class="item-container relative w-full text-[#333] break-words pl-[20px]"
              >
                <div class="absolute left-0 top-[8px] w-[6px] h-[6px] rounded-full bg-black"></div>
                <div v-if="item.year || item.time"
                     class="paginated-item italic text-[#333] block mb-[5px]"
                     :style="{ fontSize: '18px' }">{{ item.year || item.time }}</div>
                <div class="paginated-item text-[16px] leading-tight mb-[3px] w-full">Công ty: {{ item.company || item.name }}</div>
                <div class="paginated-item text-[16px] w-full mb-[3px]">Vị trí: {{ item.role || 'Nhân viên' }}</div>
                <div v-if="item.desc" class="html-content text-justify w-full text-[#333]"
                     :style="{ fontSize: '15px', lineHeight: '1.5' }">
                  <span class="paginated-item block">Mô tả:</span>
                  <template v-if="!isHtml(item.desc)">
                    <div v-for="(line, idx) in item.desc.split('\n')" :key="idx"
                         class="paginated-item w-full" style="min-height:1.5em">{{ line || '\u00A0' }}</div>
                  </template>
                  <div v-else class="paginated-item" v-html="sanitize(item.desc)"></div>
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

            <div
              v-else-if="['skills','it_skills','languages'].includes(section.id)"
              class="grid grid-cols-2 gap-x-[80px] gap-y-[25px] w-full"
            >
              <div v-if="!section.items || section.items.length === 0"
                   class="text-[#aaa] italic text-[14px] pl-[20px] col-span-2">Chưa có dữ liệu</div>
              <div
                v-for="(item, itemIndex) in section.items"
                :key="item._refId"
                class="paginated-item item-container relative w-full flex flex-col pl-[20px]"
              >
                <div class="absolute left-0 top-[10px] w-[6px] h-[6px] rounded-full bg-black"></div>
                <div class="flex justify-between items-end mb-[10px]">
                  <span class="italic text-[#333] block leading-tight" :style="{ fontSize: '18px' }">{{ item.name }}</span>
                  <span class="text-[14px] font-bold text-[#8da9c4] whitespace-nowrap ml-2">{{ getLevelInfo(item.level).text }}</span>
                </div>
                <div class="w-full bg-[#cbd5e0] rounded-full overflow-hidden" :style="{ height: '10px' }">
                  <div class="h-full rounded-full transition-all duration-500"
                       :style="{ width: getLevelInfo(item.level).percent, backgroundColor: '#8da9c4' }"></div>
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

            <div v-else-if="section.id === 'education'" class="flex flex-col gap-[20px] w-full">
              <div v-if="!section.items || section.items.length === 0"
                   class="text-[#aaa] italic text-[14px] pl-[20px]">Chưa có dữ liệu</div>
              <div
                v-for="(item, itemIndex) in section.items"
                :key="item._refId"
                class="item-container relative w-full text-[#333] pl-[20px]"
              >
                <div class="absolute left-0 top-[10px] w-[6px] h-[6px] rounded-full bg-black"></div>
                <div v-if="item.year || item.time"
                     class="paginated-item italic text-[#333] mb-[5px]"
                     :style="{ fontSize: '18px' }">{{ item.year || item.time }}</div>
                <div class="paginated-item text-[16.5px] font-normal mb-[3px]">{{ item.school || item.name }}</div>
                <div v-if="item.major || item.role" class="paginated-item text-[15.5px]">{{ item.major || item.role }}</div>
                <div v-if="item.desc" class="html-content text-[15px] mt-[5px]">
                  <template v-if="!isHtml(item.desc)">
                    <div v-for="(line, idx) in item.desc.split('\n')" :key="idx"
                         class="paginated-item w-full" style="min-height:1.5em">{{ line || '\u00A0' }}</div>
                  </template>
                  <div v-else class="paginated-item" v-html="sanitize(item.desc)"></div>
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

            <div v-else class="flex flex-col gap-[15px] w-full">
              <div v-if="!section.items || section.items.length === 0"
                   class="text-[#aaa] italic text-[14px] pl-[20px]">Chưa có dữ liệu</div>
              <div
                v-for="(item, itemIndex) in section.items"
                :key="item._refId"
                class="item-container relative w-full text-[#333] pl-[20px]"
              >
                <div class="absolute left-0 top-[10px] w-[6px] h-[6px] rounded-full bg-black"></div>
                <div class="paginated-item font-bold text-[16px] mb-[2px]">
                  {{ item.name || item.school || item.company || item.title }}
                </div>
                <div class="html-content text-justify w-full text-[#333]" :style="{ fontSize: '15px', lineHeight: '1.6' }">
                  <template v-if="!isHtml(item.desc || item.info || item.details)">
                    <div v-for="(line, idx) in (item.desc || item.info || item.details || '').split('\n')" :key="idx"
                         class="paginated-item w-full" style="min-height:1.5em">{{ line || '\u00A0' }}</div>
                  </template>
                  <div v-else class="paginated-item" v-html="sanitize(item.desc || item.info || item.details)"></div>
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

          </div>
        </template>
      </div>
    </div>

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
// ✅ defineProps / defineEmits luôn đứng đầu tiên
const props = defineProps({
  resumeData: { type: Object, required: true }
})
const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

import { computed, ref, onMounted, onUnmounted, nextTick, watch } from 'vue'

// ─── State ────────────────────────────────────────────────────────────────
const cvRoot            = ref(null)
const pageCount         = ref(1)
const selectedSectionId = ref(null)

// ─── Helpers ──────────────────────────────────────────────────────────────

const isEmpty = (val) => {
  if (!val) return true
  if (typeof val !== 'string') return false
  return val.replace(/<[^>]*>/g, '').trim() === ''
}

// Helper DRY: kiểm tra HTML một lần duy nhất
const isHtml = (text) => !!text && /<[a-z][\s\S]*>/i.test(text)

// Sanitize cơ bản — nên thay bằng DOMPurify nếu project đã cài
const sanitize = (html) => {
  if (!html) return ''
  return html
    .replace(/<script[\s\S]*?<\/script>/gi, '')
    .replace(/on\w+="[^"]*"/gi, '')
    .replace(/on\w+='[^']*'/gi, '')
    .replace(/javascript:/gi, '')
}

const formatDesc = (text) => {
  if (!text) return ''
  if (isHtml(text)) return sanitize(text)
  return text.split('\n').map(l => l.trim()).filter(Boolean).join('<br/>')
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

// ─── Computed ─────────────────────────────────────────────────────────────

const templatePrimaryColor = computed(() =>
  props.resumeData?.theme?.primaryColor || '#4a5568'
)

// Section mặc định trong thiết kế gốc — luôn hiện dù chưa có data
const DEFAULT_SECTION_IDS = ['experience', 'education', 'skills']

// Hiển thị nếu: (a) isVisible + có items, HOẶC (b) là section mặc định
const allVisibleSections = computed(() =>
  (props.resumeData?.sections || []).filter(s =>
    (s.isVisible && s.items?.length > 0) || DEFAULT_SECTION_IDS.includes(s.id)
  )
)

const sidebarIds = computed(() =>
  (props.resumeData?.sections || []).filter(s => s.column === 'left').map(s => s.id)
)
const mainIds = computed(() =>
  (props.resumeData?.sections || []).filter(s => s.column === 'right').map(s => s.id)
)

// ─── Pagination ───────────────────────────────────────────────────────────
// Chiến lược 2 tầng:
//   Tầng 1 — section-block: KHÔNG bao giờ bị cắt. Nếu section bị cắt ngang
//             trang thì đẩy NGUYÊN KHỐI xuống đầu trang kế.
//   Tầng 2 — paginated-item bên trong section có nội dung rất dài
//             (lớn hơn 1 trang): mới được phép bị cắt (content-splitting).
//             Trường hợp này cực hiếm trong thực tế CV.

const A4_W_MM   = 210
const A4_H_MM   = 297
const MARGIN_MM = 20   // lề đồng đều 4 phía

let paginateTimer = null

const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 60)
}

const doPagination = async () => {
  if (!cvRoot.value) return

  // 1. Reset toàn bộ margin đã áp
  cvRoot.value.querySelectorAll('.section-block, .paginated-item').forEach(el => {
    el.style.marginTop = ''
  })
  await nextTick()

  const pxPerMm  = cvRoot.value.offsetWidth / A4_W_MM
  const pageH    = A4_H_MM   * pxPerMm
  const marginPx = MARGIN_MM * pxPerMm
  const safeLine = pageH - marginPx          // vùng an toàn tính từ đỉnh mỗi trang
  const maxFit   = pageH - marginPx * 2      // chiều cao tối đa fit trong 1 trang

  // Tính vị trí tương đối so với cvRoot (tránh getBoundingClientRect layout thrash)
  const relTop = (el) => {
    let off = 0, cur = el
    while (cur && cur !== cvRoot.value) {
      off += cur.offsetTop
      cur  = cur.offsetParent
    }
    return off
  }

  // ── TẦNG 1: Đẩy nguyên section-block ────────────────────────────────────
  // Lấy các section-block theo thứ tự DOM (đảm bảo xử lý tuần tự từ trên xuống)
  const sectionBlocks = Array.from(cvRoot.value.querySelectorAll('.section-block'))

  for (let pass = 0; pass < 60; pass++) {
    let stable = true

    for (const block of sectionBlocks) {
      const top     = relTop(block)
      const bottom  = top + block.offsetHeight
      const pageIdx = Math.floor(top / pageH)
      const curSafe = pageIdx * pageH + safeLine

      // Section lớn hơn cả trang: bỏ qua (xử lý ở tầng 2)
      if (block.offsetHeight > maxFit) continue

      // Section bị cắt ngang ranh giới trang → đẩy NGUYÊN KHỐI xuống
      if (top < curSafe && bottom > curSafe) {
        const pushTo = (pageIdx + 1) * pageH + marginPx
        const extra  = pushTo - top
        block.style.marginTop = (parseFloat(block.style.marginTop || '0') + extra) + 'px'
        stable = false
        break  // layout thay đổi → phải tính lại từ đầu
      }
    }

    if (stable) break
  }

  // ── TẦNG 2: Xử lý paginated-item bên trong section quá dài ──────────────
  // Chỉ áp dụng khi section lớn hơn maxFit (nội dung cực kỳ dài)
  const longSections = sectionBlocks.filter(b => b.offsetHeight > maxFit)
  if (longSections.length > 0) {
    const innerItems = Array.from(cvRoot.value.querySelectorAll('.paginated-item'))
    const skipped    = new Set()

    for (let pass = 0; pass < 40; pass++) {
      let stable = true

      for (const item of innerItems) {
        if (skipped.has(item)) continue

        const top     = relTop(item)
        const bottom  = top + item.offsetHeight
        const pageIdx = Math.floor(top / pageH)
        const curSafe = pageIdx * pageH + safeLine

        // Item lớn hơn cả trang: cho phép bị cắt (không thể làm gì tốt hơn)
        if (item.offsetHeight > maxFit) {
          skipped.add(item)
          continue
        }

        if (top < curSafe && bottom > curSafe) {
          const pushTo = (pageIdx + 1) * pageH + marginPx
          const extra  = pushTo - top
          item.style.marginTop = (parseFloat(item.style.marginTop || '0') + extra) + 'px'
          stable = false
          break
        }
      }

      if (stable) break
    }
  }

  // ── Tính số trang thực tế cần thiết ────────────────────────────────────
  let maxBottom = 0
  sectionBlocks.forEach(el => {
    const b = relTop(el) + el.offsetHeight
    if (b > maxBottom) maxBottom = b
  })
  // Đảm bảo cả header cũng được tính
  const headerEl = cvRoot.value.querySelector('header')
  if (headerEl) {
    const b = relTop(headerEl) + headerEl.offsetHeight
    if (b > maxBottom) maxBottom = b
  }

  // Xóa trang thừa: chỉ giữ đủ trang thực sự có nội dung
  const needed = Math.max(1, Math.ceil((maxBottom + marginPx) / pageH))
  if (pageCount.value !== needed) pageCount.value = needed
}

// ─── Lifecycle ────────────────────────────────────────────────────────────

watch(
  () => [props.resumeData?.sections, props.resumeData?.general],
  () => requestPagination(),
  { deep: true }
)

onMounted(() => {
  requestPagination()
  window.addEventListener('resize', requestPagination)

  // MutationObserver thay cho document keyup — nhẹ hơn, chính xác hơn
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
/* ── Base ───────────────────────────────────────────────── */
#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

.item-container {
  position: relative;
  transition: background 0.15s;
}

/* ── Delete button — hiện bằng v-if, không cần CSS ẩn/hiện ── */
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

/* ── Section block ──────────────────────────────────────── */
/*
  Mặc định: border trong suốt (giữ layout không nhảy khi active).
  Hover: KHÔNG hiển thị nút nào — chỉ cursor thay đổi.
  Click (active): viền solid màu primaryColor, bo 6px, scale nhẹ.
  Màu viền = màu nền section → hiệu ứng "viền chìm" như yêu cầu.
*/
.section-block {
  position: relative;
  /* border trong suốt để layout không bị shift khi active thêm viền */
  border: 2px solid transparent;
  border-radius: 6px;
  cursor: pointer;
  padding: 10px;
  transition:
    border-color 0.18s ease,
    border-radius 0.18s ease,
    transform     0.2s cubic-bezier(0.34, 1.56, 0.64, 1),
    box-shadow    0.2s ease,
    background    0.15s ease;
  background-color: transparent;
}

/* Hover: chỉ cursor — KHÔNG hiện nút nào */
.section-block:hover {
  cursor: pointer;
}

/* Active / selected */
.section-selected {
  /* border-color được set inline bằng templatePrimaryColor */
  border-style: solid !important;
  border-radius: 6px !important;
  transform: scale(1.012) !important;
  background-color: rgba(0, 0, 0, 0.012) !important;
  z-index: 20;
  box-shadow: 0 4px 18px rgba(0, 0, 0, 0.07) !important;
}

/* ── Nav buttons ────────────────────────────────────────── */
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
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 5px;
  /* Dùng var(--sel-color) để đồng màu với primaryColor của section */
  background: var(--sel-color, #2563eb);
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.22);
  transition: filter 0.12s, transform 0.1s;
}
.nav-btn:hover  { filter: brightness(0.88); }
.nav-btn:active { transform: scale(0.92); }

/* ── HTML content ───────────────────────────────────────── */
:deep(.html-content) { margin: 0 !important; padding: 0 !important; }
:deep(.html-content p) { margin: 0 !important; padding: 0 !important; }
:deep(.html-content ul) { list-style-type: disc !important; padding-left: 1.5rem !important; margin: 0 !important; }
:deep(.html-content ol) { list-style-type: decimal !important; padding-left: 1.5rem !important; margin: 0 !important; }
:deep(.html-content li) { margin-bottom: 2px !important; }
:deep(.html-content b),
:deep(.html-content strong) { font-weight: bold; }
:deep(.html-content i),
:deep(.html-content em) { font-style: italic; }
:deep(.html-content u) { text-decoration: underline; }

/* ── Print / PDF ────────────────────────────────────────── */
@media print {
  .no-print { display: none !important; }

  .section-block,
  .section-selected {
    cursor: default !important;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
    border-radius: 0 !important;
    padding: 0 !important;
    margin-bottom: 25px !important;
    transform: none !important;
  }

  .paginated-item {
    page-break-inside: avoid;
    break-inside: avoid;
  }
}

/* Export PDF class (nếu dùng class .is-exporting-pdf thay print media) */
:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .section-selected) {
  box-shadow: none !important;
  background: transparent !important;
  border-color: transparent !important;
  border-radius: 0 !important;
  transform: none !important;
}
</style>