<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-col relative box-border leading-relaxed overflow-hidden py-[15mm] px-[15mm]"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif', color: '#1e293b' }"
    @click.self="selectedSectionId = null"
  >
    <!-- HEADER -->
    <header class="flex items-center gap-6 pb-5 mb-3 border-b border-slate-100 paginated-item" @click.stop="selectedSectionId = 'general'">
      <!-- Left side: Avatar -->
      <div class="shrink-0 relative">
        <img :src="!isEmpty(resumeData.general.avatarUrl) ? resumeData.general.avatarUrl : '/images/default-avatar.png'" alt="Avatar" class="w-[110px] height-[110px] w-28 h-28 rounded-full object-cover border border-slate-200 shadow-sm" />
      </div>

      <!-- Right side: Info -->
      <div class="flex-1 min-w-0">
        <!-- Full Name -->
        <h1 class="font-extrabold tracking-tight leading-none mb-1.5"
          style="font-size: 28px !important; color: #000000;"
          v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'HỌ VÀ TÊN CỦA BẠN'">
        </h1>

        <!-- Job Title -->
        <div class="inline-block pb-1 mb-2.5 border-b border-slate-300 pr-8">
          <div class="italic font-semibold text-slate-500 leading-tight"
            style="font-size: 13.5px !important;"
            v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'Vị trí ứng tuyển'">
          </div>
        </div>

        <!-- Contact Information (Grid 3 Columns) -->
        <div v-if="contactItems.length > 0" 
             class="grid grid-cols-3 gap-x-4 gap-y-2 text-slate-600 contact-block relative"
             :class="{ 'contact-active': selectedSectionId === 'contact' }"
             @click.stop="selectedSectionId = selectedSectionId === 'contact' ? null : 'contact'"
             style="font-size: 12.5px !important;">
          <div v-for="(ci, ciIdx) in contactItems" :key="ci.key"
               class="flex items-center relative contact-item-container py-0.5"
               :style="selectedSectionId === 'contact' ? 'padding-right: 65px;' : ''">
            <!-- Icon Mapping -->
            <span class="mr-1.5 shrink-0 text-slate-500" v-html="getContactIcon(ci.key)"></span>
            <span class="font-normal truncate text-slate-700" v-html="ci.value"></span>
            
            <!-- Controls -->
            <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print flex gap-1 bg-white border border-slate-200 shadow rounded px-1" style="position: absolute; right: 2px; top: 50%; transform: translateY(-50%); z-index: 50;">
              <button v-if="ciIdx > 0" @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn" title="Lên/Trái" style="width:18px; height:18px; padding:0;">
                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
              </button>
              <button v-if="ciIdx < contactItems.length - 1" @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn" title="Xuống/Phải" style="width:18px; height:18px; padding:0;">
                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
              </button>
              <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn-danger" title="Ẩn" style="width:18px; height:18px; padding:0;">
                <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>
          </div>
        </div>
      </div>
    </header>

    <!-- DRAGGABLE SECTIONS IN A SINGLE COLUMN -->
    <div class="flex-1 flex flex-col gap-2.5 mt-1.5" @click.self="selectedSectionId = null">
      <draggable
        v-model="sectionsWritable"
        item-key="id"
        class="flex flex-col gap-2.5 cursor-move"
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
            v-show="section.isVisible"
            :data-section-id="section.id" class="section-block relative cursor-pointer hover:bg-black/[0.02] transition-colors"
            :class="{ 'section-active': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <!-- Nav Controls -->
            <div v-show="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="$emit('moveUp', section.id, sectionIds)" class="nav-btn" title="Lên">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="$emit('moveDown', section.id, sectionIds)" class="nav-btn" title="Xuống">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <!-- Section Title (Banner Style) -->
            <div class="paginated-item w-full">
              <div class="section-banner">
                <h3 class="font-bold text-center uppercase tracking-wider text-[#1e3a8a] m-0 py-1"
                  style="font-size: 13.5px !important; line-height: 1.2 !important; margin: 0 !important;"
                  v-html="section.title"></h3>
              </div>
            </div>

            <!-- Summary Section -->
            <div v-if="section.id === 'summary'"
              class="text-justify leading-relaxed html-content pr-1 text-[13px] text-slate-700 mt-1 px-1"
              v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Tóm tắt mục tiêu nghề nghiệp...')">
            </div>

            <!-- Education Section -->
            <div v-else-if="section.id === 'education'" class="space-y-2 mt-1 px-1">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                      <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                      <CustomSectionItem v-if="(typeof section !== 'undefined' && section && section.isCustom) || (typeof id !== 'undefined' && typeof sec !== 'undefined' && sec(id)?.value?.isCustom) || (typeof block !== 'undefined' && block && block.isCustom)" :item="item" />
                      <template v-else>
                        <div class="paginated-item pr-1 flex gap-4">
                  <!-- Left: Year -->
                  <div class="w-[110px] shrink-0 text-slate-500 font-semibold text-[13px] pt-0.5">
                    <span v-html="item.year || 'Thời gian'"></span>
                  </div>
                  <!-- Right: Details -->
                  <div class="flex-1">
                    <div class="text-[13.5px] leading-snug">
                      <strong class="text-slate-800" v-html="item.school || 'Tên trường học'"></strong>
                      <span class="mx-2 text-slate-300">|</span>
                      <span class="font-semibold text-slate-700" v-html="item.major || 'Chuyên ngành học'"></span>
                    </div>
                    <div v-if="item.gradType" class="text-[12.5px] italic text-slate-500 mt-0.5" v-html="'Xếp loại: ' + item.gradType"></div>
                    <div v-if="item.desc" class="html-content mt-1.5 text-[13px] text-slate-600 text-justify" v-html="formatDesc(item.desc)"></div>
                  </div>
                </div>
                      </template>
</div>
            </div>

            <!-- Experience, Projects, Activities Section -->
            <div v-else-if="['experience', 'projects', 'project', 'activities', 'activity'].some(x => section.id.toLowerCase().includes(x))" class="space-y-2 mt-1 px-1">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                      <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                      <CustomSectionItem v-if="(typeof section !== 'undefined' && section && section.isCustom) || (typeof id !== 'undefined' && typeof sec !== 'undefined' && sec(id)?.value?.isCustom) || (typeof block !== 'undefined' && block && block.isCustom)" :item="item" />
                      <template v-else>
                        <div class="paginated-item pr-1 flex gap-4">
                  <!-- Left: Time -->
                  <div class="w-[110px] shrink-0 text-slate-500 font-semibold text-[13px] pt-0.5">
                    <span v-html="item.time || item.year || 'Thời gian'"></span>
                  </div>
                  <!-- Right: Details -->
                  <div class="flex-1">
                    <div class="text-[13.5px] leading-snug">
                      <strong class="text-slate-800" v-html="item.company || item.organization || item.name || item.title || 'Tên tổ chức'"></strong>
                      <span class="mx-2 text-slate-300">|</span>
                      <span class="font-semibold text-slate-700" v-html="item.role || item.position || 'Vai trò'"></span>
                    </div>
                    <div v-if="item.desc || item.info" class="html-content mt-1.5 text-[13px] text-slate-600 text-justify" v-html="formatDesc(item.desc || item.info || '')"></div>
                  </div>
                </div>
                      </template>
</div>
            </div>

            <!-- Skills Section (Split in 2 columns list) -->
            <div v-else-if="section.id === 'skills'" class="mt-1 px-1">
              <div class="grid grid-cols-2 gap-x-8 gap-y-1 item-container relative">
                <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, 0)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item pr-1 text-[13px] flex items-start gap-1.5">
                      <CustomSectionItem v-if="(typeof section !== 'undefined' && section && section.isCustom) || (typeof id !== 'undefined' && typeof sec !== 'undefined' && sec(id)?.value?.isCustom) || (typeof block !== 'undefined' && block && block.isCustom)" :item="item" />
                      <template v-else>
                        <span class="text-slate-400 mt-1.5 shrink-0">•</span>
                  <div class="text-slate-700 py-0.5 leading-snug">
                    <strong v-html="item.name || 'Kỹ năng'"></strong>
                    <span v-if="item.level || item.info"> - <span v-html="item.level || item.info"></span></span>
                    <span v-if="item.desc" class="text-slate-500 text-[12.5px] block mt-0.5" v-html="item.desc"></span>
                  </div>
                      </template>
</div>
              </div>
            </div>

            <!-- Languages & Certifications Section -->
            <div v-else-if="['languages', 'certifications', 'awards'].some(x => section.id.toLowerCase().includes(x))" class="space-y-1.5 mt-1 px-1">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                      <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                      <CustomSectionItem v-if="(typeof section !== 'undefined' && section && section.isCustom) || (typeof id !== 'undefined' && typeof sec !== 'undefined' && sec(id)?.value?.isCustom) || (typeof block !== 'undefined' && block && block.isCustom)" :item="item" />
                      <template v-else>
                        <div class="paginated-item pr-1 flex gap-4 text-[13px]">
                  <!-- Left: Year/Level -->
                  <div class="w-[110px] shrink-0 text-slate-500 font-semibold pt-0.5">
                    <span v-html="item.year || item.level || 'Thông tin'"></span>
                  </div>
                  <!-- Right: Name -->
                  <div class="flex-1">
                    <strong class="text-slate-800" v-html="item.name || item.info || ''"></strong>
                    <span v-if="item.organization" class="text-slate-500 italic"> (<span v-html="item.organization"></span>)</span>
                    <div v-if="item.desc" class="html-content mt-1 text-slate-600" v-html="formatDesc(item.desc)"></div>
                  </div>
                </div>
                      </template>
</div>
            </div>

            <!-- Fallback Section -->
            <div v-else class="space-y-1.5 mt-1 px-1">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container relative text-[13px]">
                      <button v-show="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
                      <CustomSectionItem v-if="(typeof section !== 'undefined' && section && section.isCustom) || (typeof id !== 'undefined' && typeof sec !== 'undefined' && sec(id)?.value?.isCustom) || (typeof block !== 'undefined' && block && block.isCustom)" :item="item" />
                      <template v-else>
                        <div class="flex gap-4">
                  <div class="w-[110px] shrink-0 text-slate-500 font-semibold" v-html="item.time || item.year || ''"></div>
                  <div class="flex-1">
                    <div v-if="item.name || item.title" class="font-bold text-slate-800 mb-0.5"><span v-html="item.name || item.title"></span></div>
                    <div class="html-content text-justify text-slate-600 leading-relaxed" v-html="formatDesc(item.desc || item.info || '')"></div>
                  </div>
                </div>
                      </template>
</div>
            </div>
          </div>
        </template>
      </draggable>
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
import draggable from 'vuedraggable'

const props = defineProps({
  resumeData: { type: Object, required: true }
})

const emit = defineEmits(['moveUp', 'moveDown', 'removeItem'])

const cvRoot = ref(null)
const pageCount = ref(1)
const selectedSectionId = ref(null)

const isEmpty = (val) => {
  if (!val) return true
  const str = String(val)
  const stripped = str.replace(/<[^>]*>/g, '').replace(/&nbsp;/g, '').trim()
  return stripped === '' || stripped === 'undefined' || stripped === 'null'
}

// ─── CONTACT ITEMS ───
const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  switch (key) {
    case 'birthDate': return isEmpty(g.birthDate) ? '' : g.birthDate
    case 'phone': return isEmpty(g.phone) ? '' : g.phone
    case 'email': return isEmpty(g.email) ? '' : g.email
    case 'address': return isEmpty(g.address) ? '' : g.address
    case 'gender': return isEmpty(g.gender) ? '' : g.gender
    case 'website': return g.website || g.github || g.linkedin || ''
    default: return ''
  }
}

const getContactIcon = (key) => {
  switch (key) {
    case 'birthDate': return `<svg class="w-3.5 h-3.5 inline" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/></svg>`
    case 'phone': return `<svg class="w-3.5 h-3.5 inline" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.94.725l.548 2.2a1 1 0 01-.321.988l-1.305.98a10.582 10.582 0 004.872 4.872l.98-1.305a1 1 0 01.988-.321l2.2.548a1 1 0 01.725.94V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z"/></svg>`
    case 'email': return `<svg class="w-3.5 h-3.5 inline" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/></svg>`
    case 'address': return `<svg class="w-3.5 h-3.5 inline" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"/><path stroke-linecap="round" stroke-linejoin="round" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"/></svg>`
    case 'gender': return `<svg class="w-3.5 h-3.5 inline" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/></svg>`
    case 'website': return `<svg class="w-3.5 h-3.5 inline" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1"/></svg>`
    default: return ''
  }
}

const contactOrder = ref(['birthDate', 'phone', 'email', 'gender', 'address', 'website'])
const hiddenContacts = ref([])

watch(() => props.resumeData.general, (newVal) => {
  if (!newVal) return
  hiddenContacts.value = hiddenContacts.value.filter(key => {
    if (key === 'website') {
      const hasValue = !isEmpty(newVal.website) || !isEmpty(newVal.github) || !isEmpty(newVal.linkedin)
      return !hasValue
    }
    return isEmpty(newVal[key])
  })
}, { deep: true })

const contactItems = computed(() => {
  return contactOrder.value
    .filter(key => !hiddenContacts.value.includes(key))
    .filter(key => !isEmpty(getContactValue(key)))
    .map(key => ({
      key,
      value: getContactValue(key)
    }))
})

const moveContactUp = (idx) => {
  if (idx <= 0) return
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && !isEmpty(getContactValue(k)))
  const keyA = visible[idx], keyB = visible[idx - 1]
  const idxA = contactOrder.value.indexOf(keyA)
  const idxB = contactOrder.value.indexOf(keyB)
  const arr = [...contactOrder.value]
  ;[arr[idxA], arr[idxB]] = [arr[idxB], arr[idxA]]
  contactOrder.value = arr
}

const moveContactDown = (idx) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && !isEmpty(getContactValue(k)))
  if (idx >= visible.length - 1) return
  const keyA = visible[idx], keyB = visible[idx + 1]
  const idxA = contactOrder.value.indexOf(keyA)
  const idxB = contactOrder.value.indexOf(keyB)
  const arr = [...contactOrder.value]
  ;[arr[idxA], arr[idxB]] = [arr[idxB], arr[idxA]]
  contactOrder.value = arr
}

const removeContactItem = (idx) => {
  const visible = contactItems.value
  if (idx >= 0 && idx < visible.length) {
    const key = visible[idx].key
    if (props.resumeData && props.resumeData.general) {
      if (key === 'website') {
        if (props.resumeData.general.website !== undefined) props.resumeData.general.website = ''
        if (props.resumeData.general.github !== undefined) props.resumeData.general.github = ''
        if (props.resumeData.general.linkedin !== undefined) props.resumeData.general.linkedin = ''
      } else {
        props.resumeData.general[key] = ''
      }
    }
    hiddenContacts.value.push(key)
    requestPagination()
  }
}

// ─── SECTIONS WRITABLE ───
const sectionsWritable = ref([])

watch(() => props.resumeData.sections, (newVal) => {
  sectionsWritable.value = [...newVal]
}, { immediate: true, deep: true })

const sectionIds = computed(() => props.resumeData.sections.map(s => s.id))

const onDragEnd = () => {
  props.resumeData.sections.splice(0, props.resumeData.sections.length, ...sectionsWritable.value)
  requestPagination()
}

// ─── UTILITIES & FORMATTING ───
const formatDesc = (text) => {
  if (!text) return ''
  
  if (!/<[a-z][\s\S]*>/i.test(text)) {
    return text.split('\n')
               .map(l => l.trim())
               .filter(Boolean)
               .map(l => `<div class="paginated-item w-full block">${l}</div>`)
               .join('')
  }

  const tempDiv = document.createElement('div')
  tempDiv.innerHTML = text

  // Preserve colors
  tempDiv.querySelectorAll('li').forEach(li => {
    const child = li.firstElementChild
    if (child && (child.tagName === 'FONT' || child.tagName === 'SPAN')) {
      if (child.color) li.style.color = child.color
      if (child.style?.color) li.style.color = child.style.color
    }
  })

  const blockTags = ['P', 'DIV', 'LI', 'H1', 'H2', 'H3', 'H4', 'H5', 'H6']
  tempDiv.querySelectorAll('*').forEach(el => {
    if (blockTags.includes(el.tagName.toUpperCase())) {
      const hasBlockChild = Array.from(el.children).some(child => blockTags.includes(child.tagName.toUpperCase()))
      if (!hasBlockChild) {
        el.classList.add('paginated-item')
      }
    }
  })

  Array.from(tempDiv.childNodes).forEach(node => {
    if (node.nodeType === Node.TEXT_NODE && node.textContent.trim()) {
       const wrapper = document.createElement('div')
       wrapper.className = 'paginated-item w-full block'
       node.replaceWith(wrapper)
       wrapper.appendChild(node)
    }
  })

  return tempDiv.innerHTML
}

// ─── PAGINATION LOGIC ───
let paginateTimer = null
const A4_W_MM = 210
const A4_H_MM = 297

const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 60)
}

const doPagination = async () => {
  if (!cvRoot.value) return

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
  const pxPerMm = cvRect.width / A4_W_MM
  const pageH = A4_H_MM * pxPerMm
  
  const bottomSafeZone = 14 * pxPerMm
  const topMargin = 14 * pxPerMm

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

.section-banner {
  background-color: #f4ebe6;
  border-radius: 2px;
  width: 100%;
  margin-bottom: 2px;
  padding: 3px 0;
}

.section-block {
  position: relative;
  padding: 4px 6px;
  border-radius: 4px;
  border: 1px solid transparent;
  cursor: pointer;
  transition: all 0.15s ease;
}

.section-active {
  border-color: rgba(30, 58, 138, 0.15) !important;
  background: rgba(30, 58, 138, 0.01) !important;
}

.contact-block {
  padding: 4px;
  border-radius: 4px;
  border: 1px solid transparent;
  cursor: pointer;
}

.contact-block.contact-active {
  border: 1px solid rgba(0, 0, 0, 0.1) !important;
  background: rgba(0, 0, 0, 0.01) !important;
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
  background: #333333;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 1px 3px rgba(0,0,0,0.2);
}

.nav-btn:hover {
  background: #111111;
}

.nav-btn-danger {
  background: #ef4444 !important;
}

.nav-btn-danger:hover {
  background: #dc2626 !important;
}

.item-container {
  position: relative;
  transition: all 0.2s;
  padding: 2px 4px;
}

.item-container:hover {
  background-color: rgba(0, 0, 0, 0.01);
}

.delete-btn {
  opacity: 0;
  transition: all 0.2s;
  position: absolute;
  top: 2px;
  right: 2px;
  width: 18px;
  height: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  background: #ef4444;
  color: white;
  border: 1px solid white;
  border-radius: 50%;
}

.item-container:hover .delete-btn {
  opacity: 1;
}

:deep(.html-content p) { margin-bottom: 0.15rem !important; }
:deep(.html-content ul) {
  list-style-type: disc !important;
  padding-left: 1.25rem !important;
  margin-top: 0.15rem;
  margin-bottom: 0.15rem;
}
:deep(.html-content ol) {
  list-style-type: decimal !important;
  padding-left: 1.25rem !important;
  margin-top: 0.15rem;
  margin-bottom: 0.15rem;
}
:deep(.html-content b), :deep(.html-content strong) { font-weight: bold; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic; }
:deep(.html-content u) { text-decoration: underline; }
:deep(.html-content ul li), :deep(.html-content ol li) { margin-bottom: 0.1rem; }

@media print {
  .no-print { display: none !important; }
  .section-block, .section-active,
  .contact-block, .contact-block.contact-active,
  .item-container {
    cursor: default;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
  }
}
</style>
