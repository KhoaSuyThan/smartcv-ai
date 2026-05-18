<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-col relative box-border text-[#333] leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif' }"
    @click.self="selectedSectionId = null"
  >
    <!-- Absolute Full-Width Green Ribbon -->
    <div class="absolute top-[4mm] left-0 w-full bg-[#8c9a85] h-[36mm] z-20 flex items-center px-[8mm] overflow-hidden">
      <div class="w-[72mm] flex flex-col">
        <h1 class="font-bold text-white leading-tight mb-0.5" style="font-size: 24px !important;" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'Trương Mỹ Linh'"></h1>
        <h2 class="font-normal text-white tracking-wider" style="font-size: 18px !important;" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'Giám đốc Nhân sự (CHRO)'"></h2>
      </div>
    </div>

    <!-- Absolute Avatar -->
    <!-- Same top as ribbon, same height => centered -->
    <div class="absolute right-[12mm] top-[4mm] w-[36mm] h-[36mm] rounded-full overflow-hidden border-[3px] border-white z-30 shadow-md bg-gray-200">
        <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
        <div v-else class="w-full h-full flex items-center justify-center text-gray-500">
           <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
           </svg>
        </div>
    </div>

    <!-- MAIN BODY -->
    <div class="flex-1 flex w-full relative z-0 bg-white">
      <!-- LEFT COLUMN -->
      <aside class="w-[72mm] shrink-0 bg-[#2d3e47] flex flex-col relative z-10 min-h-full">
        <!-- Spacer for top area + ribbon -->
        <div class="w-full h-[45mm] shrink-0"></div>
        
        <!-- CONTENT -->
        <div class="px-[8mm] pt-[5mm] pb-[20mm] flex flex-col flex-1">
          <!-- CONTACT INFO (Dynamic) -->
          <div
            v-if="contactItems.length > 0"
            class="flex flex-col w-full text-white space-y-4 mb-6 relative contact-block"
            :class="{ 'contact-active': selectedSectionId === 'contact' }"
            style="font-size: 11px !important"
            @click.stop="toggleSection('contact')"
          >
            <div v-for="(ci, ciIdx) in contactItems" :key="ci.key"
                class="flex items-center gap-3 relative contact-item-container">
               <svg class="w-3.5 h-3.5 text-white shrink-0" fill="currentColor" viewBox="0 0 20 20" v-html="ci.icon"></svg>
               <span class="break-all" v-html="ci.value"></span>
               
               <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print">
                   <button v-if="ciIdx > 0" @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn" title="Di chuyển lên" style="padding:3px">
                       <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                   </button>
                   <button v-if="ciIdx < contactItems.length - 1" @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn" title="Di chuyển xuống" style="padding:3px">
                       <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                   </button>
                   <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn-danger" title="Xóa mục này" style="padding:3px">
                       <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                   </button>
               </div>
            </div>
            <!-- Border after contact -->
            <div class="w-full border-b border-white/40 mt-3 mb-2"></div>
          </div>

        <!-- Sidebar Sections -->
        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-if="section.isVisible"
            :data-section-id="section.id" class="section-block relative group cursor-pointer hover:bg-black/5 transition-colors" style="margin-bottom: 8mm;"
            :class="{ 'section-active': selectedSectionId === section.id }"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print" data-html2canvas-ignore="true">
                <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                </button>
                <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                </button>
                <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
                </button>
                <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn phần này">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </transition>

            <!-- Title -->
            <div class="paginated-item relative mb-4">
              <h3 class="section-title text-white font-bold uppercase tracking-wide pb-1.5 border-b border-white" style="font-size: 14px !important;">
                {{ section.title }}
              </h3>
            </div>

            <!-- Content Container -->
            <div class="flex flex-col w-full text-white space-y-4">
              
              <!-- Education -->
              <div v-if="section.id === 'education'" class="space-y-4">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                  <div class="paginated-item font-bold text-white mb-0.5" style="font-size: 12px !important">
                    <span v-html="item.school"></span>
                    <span v-if="item.year" class="font-normal text-white"> ({{ item.year }})</span>
                  </div>
                  <div v-if="item.major" class="paginated-item font-normal text-white mb-0.5" style="font-size: 11px !important">{{ item.major }}</div>
                  <div v-if="item.gradType" class="paginated-item text-white font-normal italic" style="font-size: 11px !important">
                    Tốt nghiệp loại <span class="font-medium">{{ item.gradType }}</span>
                  </div>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: 50%; right: -8px; transform: translateY(-50%)">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- Skills / Languages / IT Skills -->
              <div v-else-if="['skill', 'lang', 'it_skill'].some(k => section.id.toLowerCase().includes(k))" class="space-y-4">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="item-container relative"
                >
                  <div class="paginated-item font-bold text-white mb-1 leading-snug" style="font-size: 11.5px !important">{{ item.name }}</div>
                  <div v-if="item.desc || item.info || item.level" class="text-white/90 font-normal leading-relaxed text-justify html-content" style="font-size: 10px !important" v-html="formatDesc(item.desc || item.info || item.level)"></div>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: 50%; right: -8px; transform: translateY(-50%)">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- Generic fallback for left column -->
              <div v-else class="space-y-3">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container leading-relaxed">
                  <div class="paginated-item font-bold text-white mb-0.5" style="font-size: 12px !important">
                    <span v-html="item.company || item.name || item.organization"></span>
                    <span v-if="item.role || item.title || item.major" class="font-normal text-white/90"> - {{ item.role || item.title || item.major }}</span>
                    <span v-if="item.time || item.year" class="font-normal text-white/80 block mt-0.5" style="font-size: 10px !important">{{ item.time || item.year }}</span>
                  </div>
                  <div class="text-white/90 font-normal html-content" style="font-size: 10px !important" v-html="formatDesc(item.contact || item.info || item.desc)"></div>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: 50%; right: -8px; transform: translateY(-50%)">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>
            </div>
          </div>
        </template>
        </div>
      </aside>

      <!-- RIGHT COLUMN -->
      <main class="flex-1 flex flex-col relative bg-white z-10 min-h-full">
        <!-- Spacer for top area + ribbon -->
        <div class="w-full h-[45mm] shrink-0"></div>

        <div class="px-[12mm] pt-[5mm] pb-[20mm] flex-1 flex flex-col">
          <template v-for="section in mainSections" :key="section.id">
            <div
              v-if="section.isVisible"
              :data-section-id="section.id" class="section-block relative group cursor-pointer hover:bg-black/5 transition-colors" style="margin-bottom: 12mm;"
              :class="{ 'section-active': selectedSectionId === section.id }"
              @click.stop="toggleSection(section.id)"
            >
              <!-- Nav Buttons -->
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print" data-html2canvas-ignore="true">
                  <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên">
                    <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                  </button>
                  <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống">
                    <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                  </button>
                  <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái">
                    <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
                  </button>
                  <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn phần này">
                    <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                  </button>
                </div>
              </transition>

              <!-- Title -->
              <div class="paginated-item relative mb-4">
                <div class="flex items-center">
                  <h3 class="section-title text-[#222] font-bold uppercase tracking-wide whitespace-nowrap pr-3" style="font-size: 15px !important;">
                    {{ section.title }}
                  </h3>
                  <div class="flex-1 border-b border-[#8c9a85]"></div>
                </div>
              </div>

              <!-- Content Container -->
              <div class="flex flex-col w-full text-[#333]">
                
                <!-- Summary -->
                <div v-if="section.id === 'summary'">
                  <div class="leading-[1.7] text-justify html-content font-medium text-[#444]" style="font-size: 12px !important" v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Mục tiêu nghề nghiệp...')"></div>
                </div>

                <!-- Experience / Activities / Projects -->
                <div v-else-if="['experience','activities','project'].includes(section.id)" class="space-y-5">
                  <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                    <div class="paginated-item relative z-10">
                      <div class="flex justify-between items-start gap-4 mb-0.5">
                        <h4 class="text-[#222] font-bold" style="font-size: 13px !important">
                          <span v-html="section.id === 'experience' ? item.company : (item.name || item.organization)"></span>
                          <span v-if="item.major" class="font-bold text-[#222]"> – {{ item.major }}</span>
                        </h4>
                        <span v-if="item.time" class="font-normal text-[#444] shrink-0" style="font-size: 12px !important">{{ item.time }}</span>
                      </div>
                      <div class="font-normal text-[#222] mb-1.5" style="font-size: 12px !important">
                        {{ section.id === 'experience' ? item.role : (item.role || item.position || '') }}
                      </div>
                    </div>
                    <div class="leading-[1.7] text-[#444] text-justify html-content" style="font-size: 12px !important" v-html="formatDesc(item.desc)"></div>
                    <transition name="fade-btns">
                      <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                        <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                      </button>
                    </transition>
                  </div>
                </div>

                <!-- Education (Right column fallback) -->
                <div v-else-if="section.id === 'education'" class="space-y-4">
                  <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                    <div class="paginated-item relative z-10">
                      <div class="flex justify-between items-start gap-4 mb-0.5">
                        <h4 class="text-[#222] font-bold" style="font-size: 13px !important">
                          <span v-html="item.school"></span>
                        </h4>
                        <span v-if="item.year" class="font-bold text-[#333] shrink-0" style="font-size: 12px !important">{{ item.year }}</span>
                      </div>
                      <div v-if="item.major" class="font-normal text-[#555] mb-1" style="font-size: 12px !important">{{ item.major }}</div>
                      <div v-if="item.gradType" class="text-[#555] font-normal" style="font-size: 11.5px !important">
                        Xếp loại: <span class="font-bold">{{ item.gradType }}</span>
                      </div>
                    </div>
                    <div class="leading-[1.6] text-[#444] html-content mt-1" style="font-size: 12px !important" v-html="formatDesc(item.desc)"></div>
                    <transition name="fade-btns">
                      <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                        <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                      </button>
                    </transition>
                  </div>
                </div>

                <!-- Certifications / Awards -->
                <div v-else-if="['certifications', 'awards'].includes(section.id)" class="space-y-4">
                  <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                    <div class="paginated-item relative z-10">
                      <div class="flex justify-between items-start gap-4 mb-0.5">
                         <div class="font-bold text-[#222]" style="font-size: 13px !important">{{ item.name || item.info }}</div>
                         <div v-if="item.year" class="font-bold text-[#333] shrink-0" style="font-size: 12px !important">{{ item.year }}</div>
                      </div>
                    </div>
                    <transition name="fade-btns">
                      <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                        <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                      </button>
                    </transition>
                  </div>
                </div>

                <!-- Skills / Languages / IT Skills (Right Column) -->
                <div v-else-if="['skill', 'lang', 'it_skill'].some(k => section.id.toLowerCase().includes(k))" class="space-y-4">
                  <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative">
                    <div class="paginated-item relative z-10">
                      <div class="flex justify-between items-baseline gap-4 mb-0.5 border-b border-gray-100 pb-1">
                        <div class="font-bold text-[#222]" style="font-size: 13px !important">{{ item.name }}</div>
                        <div v-if="item.level || item.info" class="font-medium text-[#666] shrink-0" style="font-size: 11px !important">{{ item.level || item.info }}</div>
                      </div>
                      <div v-if="item.desc" class="leading-[1.6] text-[#555] html-content mt-1" style="font-size: 11.5px !important" v-html="formatDesc(item.desc)"></div>
                    </div>
                    <transition name="fade-btns">
                      <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                        <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                      </button>
                    </transition>
                  </div>
                </div>

                <!-- Other main sections -->
                <div v-else class="space-y-4">
                  <div
                    v-for="(item, itemIndex) in section.items"
                    :key="item._refId"
                    class="item-container relative text-[#444] leading-[1.7]"
                    style="font-size: 12px !important"
                  >
                    <div class="html-content" v-html="formatDesc(item.desc || item.info || item.name)"></div>
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
        </div>
      </main>
    </div>

    <!-- ĐƯỜNG PHÂN TRANG -->
    <template v-for="p in (pageCount - 1)" :key="'div-' + p">
      <div 
        class="page-break-indicator no-print" 
        data-html2canvas-ignore="true" 
        :style="{ top: `calc(${p * 297}mm - 25px)` }"
      >
        <div class="page-break-mask"></div>
        <span class="page-break-label">Ngắt trang {{ p + 1 }}</span>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, ref, onMounted, nextTick, watch, onUnmounted } from 'vue'

const cvRoot = ref(null)
const pageCount = ref(1)
const selectedSectionId = ref(null)

const props = defineProps({
  resumeData: { type: Object, required: true }
})
const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

const toggleSection = (id) => {
  selectedSectionId.value = selectedSectionId.value === id ? null : id
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
  if (section.id === 'summary') return !isEmpty(props.resumeData?.general?.summary)
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
  
  const wrapTextNodes = (element) => {
    Array.from(element.childNodes).forEach(node => {
      if (node.nodeType === Node.TEXT_NODE) {
        if (node.textContent.trim()) {
           const wrapper = document.createElement('div')
           wrapper.className = 'paginated-item inline-block w-full'
           node.replaceWith(wrapper)
           wrapper.appendChild(node)
        }
      } else if (node.nodeType === Node.ELEMENT_NODE) {
        if (node.tagName === 'BR') {
           node.outerHTML = '<div class="paginated-item h-[10px] w-full"></div>'
        } else if (['P', 'DIV', 'LI'].includes(node.tagName)) {
           node.classList.add('paginated-item')
           wrapTextNodes(node)
        } else if (['UL', 'OL'].includes(node.tagName)) {
           node.classList.add('paginated-item')
           wrapTextNodes(node)
        }
      }
    })
  }

  wrapTextNodes(tempDiv)
  return tempDiv.innerHTML
}

const sidebarSections = computed(() => {
  return props.resumeData.sections?.filter(s => s.column === 'left' && s.isVisible) || []
})

const mainSections = computed(() => {
  return props.resumeData.sections?.filter(s => s.column === 'right' && s.isVisible) || []
})

const sidebarIds = computed(() => sidebarSections.value.map(s => s.id))
const mainIds = computed(() => mainSections.value.map(s => s.id))

// --- CONTACT ITEMS: Danh sách động có thể sắp xếp / ẩn ---
const contactIcons = {
  dob: '<path fill-rule="evenodd" d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" clip-rule="evenodd"></path>',
  phone: '<path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z" />',
  email: '<path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z" /><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z" />',
  address: '<path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd" />'
}

const contactOrder = ref(['dob', 'phone', 'email', 'address'])
const hiddenContacts = ref([])

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  switch (key) {
    case 'dob': return isEmpty(g.dob) ? '' : g.dob
    case 'phone': return isEmpty(g.phone) ? '' : g.phone
    case 'email': return isEmpty(g.email) ? '' : g.email
    case 'address': return isEmpty(g.address) ? '' : g.address
    default: return ''
  }
}

const contactItems = computed(() => {
  return contactOrder.value
    .filter(key => !hiddenContacts.value.includes(key))
    .filter(key => !isEmpty(getContactValue(key)))
    .map(key => ({
      key,
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
    }
    hiddenContacts.value.push(key)
    requestPagination()
  }
}

const A4_W_MM = 210
const A4_H_MM = 297
let paginateTimer = null

const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 100)
}

const doPagination = async () => {
  if (!cvRoot.value) return

  const allElements = Array.from(cvRoot.value.querySelectorAll('.paginated-item'))
  allElements.forEach(el => {
    el.style.setProperty('margin-top', '0px', 'important')
  })
  await nextTick()

  const cvRect = cvRoot.value.getBoundingClientRect()
  const pxPerMm = cvRect.width / A4_W_MM
  const pageH = A4_H_MM * pxPerMm
  
  const bottomSafeZone = 20 * pxPerMm 
  const topMargin = 20 * pxPerMm 

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
  const isLeft = (id) => ['education', 'skill', 'reference', 'hobbi', 'lang'].some(k => id.toLowerCase().includes(k))
  const isRight = (id) => ['summary', 'experience', 'activit', 'cert', 'award', 'project'].some(k => id.toLowerCase().includes(k))
  const defaultVisible = ['summary', 'skill', 'education', 'experience']
  
  props.resumeData?.sections?.forEach(sec => {
    if (isLeft(sec.id)) {
      sec.column = 'left'
    } else if (isRight(sec.id)) {
      sec.column = 'right'
    }
    
    if (defaultVisible.some(k => sec.id.toLowerCase().includes(k))) {
      sec.isVisible = true
    } else {
      sec.isVisible = false
    }
  })

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
}

:deep(.html-content ul) {
  list-style-type: disc !important;
  padding-left: 1rem !important;
}
:deep(.html-content ol) {
  list-style-type: decimal !important;
  padding-left: 1rem !important;
}
:deep(.html-content p) {
  margin-bottom: 0.25rem !important;
}

@media print {
  .no-print { display: none !important; }
}

:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .section-active),
:global(.is-exporting-pdf .page-break-indicator) {
  cursor: default !important;
  box-shadow: none !important;
  background: transparent !important;
  border-color: transparent !important;
  transform: none !important;
}

.page-break-indicator { 
  position: absolute; left: 0; width: 100%; 
  z-index: 5000; display: flex; align-items: center; justify-content: center; 
  height: 50px; pointer-events: none; 
}
.page-break-mask {
  position: absolute; left: -20px; width: calc(100% + 40px); height: 100%;
  background: #1e293b; 
}
.page-break-label { 
  position: relative; background: #334155; color: #f8fafc; 
  padding: 6px 16px; border-radius: 6px; font-size: 10px; 
  font-weight: 700; text-transform: uppercase; letter-spacing: 0.1em;
  border: 1px solid #475569; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.3);
  z-index: 1;
}

.section-block {
  border: 1.5px solid transparent;
  border-radius: 4px;
  padding: 6px;
  margin: -6px;
  cursor: pointer;
  transition: all 0.2s;
}

.section-active {
  border-color: #2b2b2b;
  background-color: rgba(0,0,0,0.02);
}

.nav-btns {
  position: absolute;
  right: 6px;
  top: 6px;
  display: flex;
  gap: 4px;
  z-index: 50;
  background: #f4f5f7;
  padding: 4px;
  border-radius: 4px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.15);
  border: 1px solid #e2e8f0;
}

.nav-btn {
  width: 22px;
  height: 22px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #222b36;
  color: white;
  border: none;
  cursor: pointer;
  transition: all 0.2s;
}
.nav-btn:hover {
  background: #1a202c;
  transform: translateY(-1px);
}
.nav-btn-danger { background: #ef4444 !important; }
.nav-btn-danger:hover { background: #dc2626 !important; }
.nav-btn.nav-btn-danger {
  background: #ef4444;
}
.nav-btn.nav-btn-danger:hover {
  background: #dc2626;
}

.delete-item-btn {
  position: absolute;
  width: 18px;
  height: 18px;
  background: #ef4444;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 30;
  box-shadow: 0 2px 4px rgba(0,0,0,0.2);
  transition: all 0.2s;
}
.delete-item-btn:hover {
  background: #dc2626;
  transform: scale(1.1);
}
.delete-item-btn--lg {
  width: 22px;
  height: 22px;
  right: -10px;
  top: 50%;
  transform: translateY(-50%);
}
.delete-item-btn--lg:hover {
  transform: translateY(-50%) scale(1.1);
}

.contact-block {
  border-radius: 8px;
  border: 1.5px solid transparent;
  padding: 4px 6px;
  margin: -4px -6px;
  cursor: pointer;
  transition: all 0.2s;
}
.contact-block.contact-active {
  border: 1.5px solid white !important;
  background-color: rgba(255,255,255,0.05);
  box-shadow: 0 4px 18px rgba(0,0,0,0.10);
  z-index: 10;
}
.contact-item-container { position: relative; }
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

.fade-btns-enter-active,
.fade-btns-leave-active {
  transition: opacity 0.2s, transform 0.2s;
}
.fade-btns-enter-from,
.fade-btns-leave-to {
  opacity: 0;
  transform: translateY(5px);
}
</style>
