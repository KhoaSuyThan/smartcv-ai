<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="flex flex-row relative box-border overflow-hidden"
    :style="{
      width: '210mm',
      height: `${Math.max(1, pageCount) * 297}mm`,
      fontFamily: '\'Segoe UI\', Roboto, Helvetica, Arial, sans-serif',
      backgroundColor: '#f3ebdd !important'
    }"
  >
    <main
      class="flex-[6] flex flex-col relative z-20 overflow-hidden box-border pt-[40px] pb-[50px]"
      :style="{ backgroundColor: '#f3ebdd !important' }"
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
            color: '#5d4e46',
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
            color: '#8b7355',
            marginTop: '10px !important',
            fontWeight: '600 !important',
            paddingLeft: '20px !important'
          }"
          v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'VỊ TRÍ ỨNG TUYỂN'"
        ></p>
      </header>

      <div
        class="paginated-item w-full"
        :style="{ height: '10px', backgroundColor: '#5d4e46', marginBottom: '40px', width: '100%' }"
      ></div>

      <div class="w-full flex flex-col flex-1 m-0 p-0">
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block relative paginated-item w-full mb-[20px]"
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
            </div>

            <h1
              class="uppercase w-full block"
              :style="{
                fontSize: '17px !important',
                color: '#8b7355',
                paddingBottom: '8px !important',
                margin: '20px 0 15px 0 !important',
                paddingLeft: '20px !important',
                fontWeight: '800 !important',
                letterSpacing: '1px'
              }"
            >{{ section.title || (section.id === 'summary' ? 'MỤC TIÊU NGHỀ NGHIỆP' : section.id === 'contact' ? 'THÔNG TIN LIÊN HỆ' : '') }}</h1>

            <div
              class="w-full flex flex-col"
              :style="{
                gap: '20px',
                borderLeft: ['summary', 'contact'].includes(section.id) ? 'none' : '1px solid #d4c5b4',
                paddingLeft: ['summary', 'contact'].includes(section.id) ? '0' : '15px !important',
                marginLeft: ['summary', 'contact'].includes(section.id) ? '20px !important' : '20px !important',
                width: ['summary', 'contact'].includes(section.id) ? 'calc(100% - 20px)' : 'calc(100% - 20px)'
              }"
            >
              <template v-if="section.id === 'summary'">
                <div class="html-content text-justify whitespace-pre-line w-full text-[#4a3728]" :style="{ fontSize: '14px', lineHeight: '1.7', paddingLeft: '20px' }" v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Tôi là một ứng viên năng động, mong muốn...'"></div>
              </template>

              <template v-else-if="section.id === 'contact'">
                <ul class="w-full list-none p-0 m-0 flex flex-col gap-[12px]" :style="{ fontSize: '14px', lineHeight: '1.7', color: '#4a3728', paddingLeft: '20px' }">
                  <li v-if="!isEmpty(resumeData.general.phone)" class="flex items-start break-words w-full"><strong class="font-bold mr-1 shrink-0 text-[#8b7355]">Di động:</strong><span v-html="resumeData.general.phone"></span></li>
                  <li v-if="!isEmpty(resumeData.general.email)" class="flex items-start break-words w-full"><strong class="font-bold mr-1 shrink-0 text-[#8b7355]">Email:</strong><span v-html="resumeData.general.email"></span></li>
                  <li v-if="!isEmpty(resumeData.general.website) || !isEmpty(resumeData.general.linkedin) || !isEmpty(resumeData.general.github)" class="flex items-start break-words w-full"><strong class="font-bold mr-1 shrink-0 text-[#8b7355]">Trang web:</strong><span v-html="!isEmpty(resumeData.general.website) ? resumeData.general.website : (!isEmpty(resumeData.general.linkedin) ? resumeData.general.linkedin : resumeData.general.github)"></span></li>
                  <li v-if="!isEmpty(resumeData.general.address)" class="flex items-start break-words w-full"><strong class="font-bold mr-1 shrink-0 text-[#8b7355]">Địa chỉ:</strong><span v-html="resumeData.general.address"></span></li>
                </ul>
              </template>

              <template v-else>
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full text-[#4a3728]" style="margin: 0 !important; padding: 0 !important;">
                  <div v-if="['education','experience','project','activities'].includes(section.id)" class="w-full">
                    <div class="w-full flex flex-col">
                      <div class="w-full flex justify-between items-baseline gap-2" :style="{ margin: '0 !important' }">
                        <span class="font-bold break-words flex-1 leading-tight text-[#4a3728]" :style="{ margin: '0 !important', fontSize: '15px' }" v-html="section.id === 'education' ? item.school : (item.company || item.name)"></span>
                        <span v-if="item.year || item.time" class="font-bold shrink-0 whitespace-nowrap text-right text-[#8b7355]" :style="{ fontSize: '13px', margin: '0 !important' }">{{ item.year || item.time }}</span>
                      </div>
                      <div v-if="item.major || item.role" class="italic text-[#5d4e46] mt-[2px] mb-[5px] w-full" :style="{ fontSize: '14px !important' }">{{ item.major || item.role }}</div>
                    </div>
                    <div v-if="item.desc" class="html-content text-justify whitespace-pre-line break-words w-full" :style="{ margin: '0 !important', padding: '0 !important', fontSize: '14px', lineHeight: '1.7' }" v-html="formatDesc(item.desc)"></div>
                    <div v-else-if="item.gradType" class="font-medium mt-[2px]" :style="{ color: '#8b7355', fontSize: '13px !important', margin: '0 !important' }">Trạng thái: {{ item.gradType }}</div>
                  </div>
                  <div v-else class="w-full">
                    <div v-if="item.year || item.time" class="font-bold mb-[5px] text-[#4a3728]" :style="{ fontSize: '14px', margin: '0 !important' }">{{ item.year || item.time }}</div>
                    <div class="html-content text-justify whitespace-pre-line break-words w-full" :style="{ margin: '0 !important', padding: '0 !important', fontSize: '14px', lineHeight: '1.7' }" v-html="formatDesc(item.desc || item.name || item.info)"></div>
                  </div>
                  <button v-show="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button>
                </div>
              </template>
            </div>
          </div>
        </template>
      </div>
    </main>

    <aside
      class="z-10 flex flex-col shrink-0 relative box-border text-white"
      :style="{
        width: '38%',
        backgroundColor: '#5d4e46',
        padding: '40px 30px',
        margin: '20px 0 !important',
        height: 'calc(100% - 40px) !important',
        alignSelf: 'flex-start'
      }"
    >
      <div class="paginated-item relative z-20 w-full flex flex-col items-center mb-[40px]">
        <div class="relative rounded-full overflow-hidden mx-auto" :style="{ width: '180px', height: '180px', border: '8px solid rgba(255,255,255,0.1)' }">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center bg-white/10 text-white/50">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/></svg>
          </div>
        </div>
      </div>

      <div class="w-full flex-1 flex flex-col m-0 p-0">
        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block relative paginated-item w-full mb-[35px]"
            :class="{ 'section-active--sidebar': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <div v-show="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
              <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang trái (Cột chính)"><svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
            </div>

            <h1 class="uppercase w-full block" :style="{ fontSize: '16px !important', fontWeight: '700 !important', paddingBottom: '5px !important', margin: '0 0 15px 0 !important', color: '#fdf5e6' }">
              {{ section.title || (section.id === 'summary' ? 'MỤC TIÊU NGHỀ NGHIỆP' : section.id === 'contact' ? 'THÔNG TIN LIÊN HỆ' : '') }}
            </h1>

            <div class="w-full flex flex-col gap-[15px]">
              <template v-if="section.id === 'summary'">
                <div class="html-content-sidebar text-justify whitespace-pre-line w-full text-[#e8e8e8]" :style="{ fontSize: '13px !important', lineHeight: '1.6 !important', margin: '0 !important' }" v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Tôi là một ứng viên năng động, mong muốn...'"></div>
              </template>

              <template v-else-if="section.id === 'contact'">
                <ul class="w-full list-none p-0 m-0 flex flex-col gap-[12px]" :style="{ fontSize: '13px !important', lineHeight: '1.6', color: '#e8e8e8' }">
                  <li v-if="!isEmpty(resumeData.general.phone)" class="flex items-start break-words w-full"><strong class="font-bold mr-1 shrink-0 text-[#fdf5e6]">Di động:</strong><span v-html="resumeData.general.phone"></span></li>
                  <li v-if="!isEmpty(resumeData.general.email)" class="flex items-start break-words w-full"><strong class="font-bold mr-1 shrink-0 text-[#fdf5e6]">Email:</strong><span v-html="resumeData.general.email"></span></li>
                  <li v-if="!isEmpty(resumeData.general.website) || !isEmpty(resumeData.general.linkedin) || !isEmpty(resumeData.general.github)" class="flex items-start break-words w-full"><strong class="font-bold mr-1 shrink-0 text-[#fdf5e6]">Trang web:</strong><span v-html="!isEmpty(resumeData.general.website) ? resumeData.general.website : (!isEmpty(resumeData.general.linkedin) ? resumeData.general.linkedin : resumeData.general.github)"></span></li>
                  <li v-if="!isEmpty(resumeData.general.address)" class="flex items-start break-words w-full"><strong class="font-bold mr-1 shrink-0 text-[#fdf5e6]">Địa chỉ:</strong><span v-html="resumeData.general.address"></span></li>
                </ul>
              </template>

              <template v-else>
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative w-full text-[#e8e8e8]" :style="{ fontSize: '13px !important', margin: '0 !important', padding: '0 !important' }">
                  <div v-if="['skills','languages','it_skills'].includes(section.id)" class="flex flex-col skill-group-extra mt-[5px]">
                    <div class="extra-label w-full uppercase" :style="{ fontSize: '12px !important', fontWeight: 'bold !important', color: '#fdf5e6', marginBottom: '5px' }">{{ item.name }}</div>
                    <div v-if="item.info || item.level" class="w-full break-words whitespace-pre-line" :style="{ fontSize: '12px !important', color: '#ddd', lineHeight: '1.4', paddingLeft: '5px' }">{{ item.info || item.level }}</div>
                  </div>
                  <div v-else class="w-full flex flex-col">
                    <span class="font-bold w-full break-words leading-tight text-[#fdf5e6]" v-html="section.id === 'education' ? item.school : (item.company || item.name)"></span>
                    <div v-if="item.major || item.role" class="italic opacity-90 mt-[2px] text-[12px]">{{ item.major || item.role }}</div>
                    <div v-if="item.year || item.time" class="font-bold opacity-90 text-[11px] mt-[2px]">{{ item.year || item.time }}</div>
                    <div v-if="item.desc" class="html-content-sidebar text-justify whitespace-pre-line break-words w-full mt-[5px]" :style="{ lineHeight: '1.6' }" v-html="formatDesc(item.desc)"></div>
                  </div>
                  <button v-show="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button>
                </div>
              </template>
            </div>
          </div>
        </template>
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
        return div.innerHTML
      } catch { return text }
    }
    return text
  }
  return text.split('\n').map(l => l.trim()).filter(Boolean).join('<br/>')
}

const A4_W_MM = 210
const A4_H_MM = 297
const MARGIN_MM = 20

let paginateTimer = null
const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 60)
}

const doPagination = async () => {
  if (!cvRoot.value) return

  cvRoot.value.querySelectorAll('.paginated-item').forEach(el => { el.style.marginTop = '' })
  await nextTick()

  const pxPerMm = cvRoot.value.offsetWidth / A4_W_MM
  const pageH = A4_H_MM * pxPerMm
  const marginPx = MARGIN_MM * pxPerMm
  const safeLine = pageH - marginPx

  const relTop = (el) => {
    let off = 0, cur = el
    while (cur && cur !== cvRoot.value) { off += cur.offsetTop; cur = cur.offsetParent }
    return off
  }

  const processColumn = (selector) => {
    const col = cvRoot.value.querySelector(selector)
    if (!col) return
    const colItems = col.classList.contains('paginated-item')
      ? [col]
      : Array.from(col.querySelectorAll('.paginated-item'))

    const maxUsable = safeLine - marginPx

    for (let pass = 0; pass < 80; pass++) {
      let stable = true
      for (const item of colItems) {
        const top = relTop(item)
        const bottom = top + item.offsetHeight
        const pageIdx = Math.floor(top / pageH)
        const curSafe = pageIdx * pageH + safeLine

        if (top < curSafe && bottom > curSafe && item.offsetHeight <= maxUsable) {
          const targetTop = (pageIdx + 1) * pageH + marginPx
          const extra = targetTop - top
          const cur = parseFloat(item.style.marginTop || '0')
          item.style.marginTop = (cur + extra) + 'px'
          stable = false
          break
        }
      }
      if (stable) break
    }
  }

  processColumn('main')
  processColumn('aside')

  let maxBottom = 0
  cvRoot.value.querySelectorAll('.paginated-item').forEach(el => {
    const b = relTop(el) + el.offsetHeight
    if (b > maxBottom) maxBottom = b
  })

  const needed = Math.max(1, Math.ceil((maxBottom + marginPx) / pageH))
  if (pageCount.value !== needed) pageCount.value = needed
}

onMounted(() => {
  if (props.resumeData && props.resumeData.sections) {
    const secs = props.resumeData.sections

    // 1. Khởi tạo/Thêm thẳng các mục cấu trúc quan trọng vào mảng của Cha nếu bị thiếu
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

    // 2. Thiết lập cấu hình mặc định (cột trái/phải, ẩn/hiện)
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

    // Ép mục Summary luôn nằm ở đầu (vị trí số 1) nếu nó mới được tạo
    if (summaryAdded) {
      const sIdx = secs.findIndex(s => s.id === 'summary')
      const [sumItem] = secs.splice(sIdx, 1)
      secs.unshift(sumItem)
    }

    // 3. Ép mục Contact nằm ở vị trí số 3 bên cột phải nếu nó mới được tạo
    if (contactAdded) {
      const cIdx = secs.findIndex(s => s.id === 'contact')
      const [contactItem] = secs.splice(cIdx, 1)
      
      let rightCount = 0
      let insertAt = secs.length
      for (let i = 0; i < secs.length; i++) {
        if (secs[i].column === 'right') {
          rightCount++
          // Vị trí sau 2 phần tử cột phải đầu tiên (tức là đứng thứ 3 cột phải)
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
  document.removeEventListener('keyup', requestPagination)
  if (paginateTimer) clearTimeout(paginateTimer)
})

watch(() => props.resumeData, requestPagination, { deep: true })

// Tính toán trực tiếp dựa trên props để đồng bộ chuẩn với Component Cha (Nút di chuyển sẽ hoạt động hoàn hảo)
const mainSections = computed(() => (props.resumeData?.sections || []).filter(s => s.column === 'left'))
const sidebarSections = computed(() => (props.resumeData?.sections || []).filter(s => s.column === 'right'))

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
    transform 0.2s cubic-bezier(0.34, 1.56, 0.64, 1),
    box-shadow 0.2s ease,
    border-color 0.15s ease,
    border-radius 0.15s ease,
    background 0.15s ease;
}

.section-active--main {
  border: 2px solid #f3ebdd !important;
  border-style: solid !important;
  border-radius: 6px !important;
  transform: scale(1.013) !important;
  box-shadow: 0 6px 24px rgba(93,78,70,0.18), 0 1px 4px rgba(93,78,70,0.08) !important;
  background: rgba(255,255,255,0.28) !important;
  z-index: 10 !important;
}

.section-active--sidebar {
  border: 2px solid #5d4e46 !important;
  border-style: solid !important;
  border-radius: 6px !important;
  transform: scale(1.013) !important;
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
  background: #2563eb;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  box-shadow: 0 2px 6px rgba(37,99,235,0.4);
  transition: background 0.12s, transform 0.1s;
}
.nav-btn:hover { background: #1d4ed8 !important; }
.nav-btn:active { transform: scale(0.92) !important; }

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