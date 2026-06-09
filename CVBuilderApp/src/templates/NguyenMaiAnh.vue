<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="cv-browser-wrapper"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm` }"
    @click.self="closeAllSections"
  >
    <div class="browser-header paginated-item">
      <div class="browser-tabs">
        <div class="window-controls">
          <span class="dot red"></span>
          <span class="dot yellow"></span>
          <span class="dot green"></span>
        </div>
        <div class="tab active-tab">
          Curriculum Vitae <i class="fas fa-times close-tab"></i>
        </div>
        <div class="new-tab"><i class="fas fa-plus"></i></div>
      </div>
      <div class="browser-url-bar">
        <div class="nav-arrows">
          <i class="fas fa-chevron-left"></i>
          <i class="fas fa-chevron-right text-gray-300"></i>
          <i class="fas fa-redo-alt"></i>
        </div>
        <div class="url-box">
          <i class="fas fa-briefcase url-icon"></i>
          <span class="url-text">Kỹ sư phần mềm IT</span>
        </div>
        <div class="browser-actions">
          <i class="fas fa-microphone"></i>
          <i class="fas fa-ellipsis-v"></i>
        </div>
      </div>
    </div>

    <div class="cv-container">
      
      <header class="custom-card profile-card paginated-item">
        <div class="avatar-container">
          <img v-if="resumeData.general.avatarUrl || resumeData.general.avatar" :src="resumeData.general.avatarUrl || resumeData.general.avatar" class="avatar-img" alt="Avatar"/>
          <div v-else class="avatar-placeholder">
            <i class="fas fa-user text-4xl text-[#CBD5E1]"></i>
          </div>
        </div>
        <div class="header-info">
          
          <h1 class="fullname" v-if="!isEmpty(resumeData.general.fullName)" v-html="stripTags(resumeData.general.fullName)"></h1>
          <h1 class="fullname" v-else>NGUYỄN MAI ANH</h1>
          
          <p class="job-title" v-if="!isEmpty(resumeData.general.jobTitle)" v-html="stripTags(resumeData.general.jobTitle)"></p>
          <p class="job-title" v-else>Kỹ sư phần mềm IT</p>
          
          <div v-if="contactItems.length > 0" class="contact-grid relative contact-block"
               :class="{ 'contact-active': selectedSectionId === 'contact' }"
               @click.stop="selectedSectionId = selectedSectionId === 'contact' ? null : 'contact'">
            <div v-for="(ci, ciIdx) in contactItems" :key="ci.key"
                 class="contact-item relative contact-item-container">
              <i v-html="ci.icon"></i>
              <span v-html="ci.value"></span>
              
              <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print">
                <!-- Move Up -->
                <button v-if="ciIdx >= 2" @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn" title="Di chuyển lên" style="padding:2px">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                </button>
                <!-- Move Down -->
                <button v-if="ciIdx + 2 < contactItems.length" @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn" title="Di chuyển xuống" style="padding:2px">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                </button>
                <!-- Move Left -->
                <button v-if="ciIdx % 2 === 1" @click.stop.prevent="moveContactLeft(ciIdx)" class="nav-btn" title="Di chuyển sang trái" style="padding:2px">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
                </button>
                <!-- Move Right -->
                <button v-if="ciIdx % 2 === 0 && ciIdx + 1 < contactItems.length" @click.stop.prevent="moveContactRight(ciIdx)" class="nav-btn" title="Di chuyển sang phải" style="padding:2px">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
                </button>
                
                <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn-danger" title="Ẩn mục này" style="padding:2px">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </div>
      </header>

      <template v-for="section in (resumeData.sections || []).filter(s => s.id === 'summary')" :key="section.id">
        <div v-show="section.isVisible" class="custom-card section-block mb-6 cursor-pointer hover:bg-black/5 transition-colors"
             :class="{ 'section-active': selectedSectionId === section.id }"
             @click.stop="toggleSection(section.id)" :data-section-id="section.id">
          
          <transition name="fade-btns">
            <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
              <button class="nav-btn" @click.stop.prevent="$emit('moveUp', section.id, [])"><i class="fas fa-chevron-up"></i></button>
              <button class="nav-btn" @click.stop.prevent="$emit('moveDown', section.id, [])"><i class="fas fa-chevron-down"></i></button>
              <button class="nav-btn nav-btn-danger close-active-btn" @click.stop.prevent="section.isVisible = false; selectedSectionId = null" title="Ẩn mục này"><i class="fas fa-times"></i></button>
            </div>
          </transition>

          <div class="card-body">
            <div v-if="!(isEmpty(section.desc) && isEmpty(section.description) && isEmpty(section.content) && isEmpty(section.value) && isEmpty(resumeData.general.summary))" 
                 class="html-content text-justify text-gray-500" v-html="formatDesc(section.desc || section.description || section.content || section.value || resumeData.general.summary)"></div>
            <div v-else class="html-content text-gray-400 italic paginated-item">
              Hãy nói 1 chút về mục tiêu nghề nghiệp của bạn...
            </div>
          </div>
        </div>
      </template>

      <div class="main-layout">
        
        <aside class="left-column">
          <draggable
            v-model="sidebarSectionsWritable"
            item-key="id"
            group="sections"
            class="flex flex-col gap-[25px] w-full cursor-move"
            @end="onDragEnd"
            animation="200"
            ghost-class="opacity-30"
            :delay="100"
            :delayOnTouchOnly="true"
            :fallbackTolerance="5"
            filter=".nav-btn, .delete-item-btn, .contact-item-btns, .html-content, input"
          >
            <template #item="{ element: section }">
            <div v-show="section.isVisible" class="custom-card section-block cursor-pointer hover:bg-black/5 transition-colors"
                 :class="{ 'section-active': selectedSectionId === section.id }"
                 @click.stop="toggleSection(section.id)" :data-section-id="section.id">
              
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
                  <button class="nav-btn" @click.stop.prevent="$emit('moveUp', section.id, leftIds)"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop.prevent="$emit('moveDown', section.id, leftIds)"><i class="fas fa-chevron-down"></i></button>
                  <button class="nav-btn" @click.stop.prevent="moveHorizontal(section.id)"><i class="fas fa-exchange-alt"></i></button>
                  <button class="nav-btn nav-btn-danger close-active-btn" @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" title="Ẩn mục này"><i class="fas fa-times"></i></button>
                </div>
              </transition>

              <div class="card-header paginated-item">
                <div class="card-title-group">
                  <i v-if="section.id === 'education'" class="fas fa-graduation-cap card-icon"></i>
                  <i v-else-if="section.id === 'experience'" class="fas fa-briefcase card-icon"></i>
                  <i v-else-if="section.id === 'skills' || section.id === 'it_skills' || section.id === 'languages'" class="fas fa-pen card-icon"></i>
                  <i v-else-if="section.id === 'activities'" class="fas fa-users card-icon"></i>
                  <i v-else-if="section.id === 'project' || section.id === 'projects'" class="fas fa-project-diagram card-icon"></i>
                  <i v-else-if="section.id === 'awards'" class="fas fa-trophy card-icon"></i>
                  <i v-else-if="section.id === 'certifications' || section.id === 'certificates'" class="fas fa-certificate card-icon"></i>
                  <i v-else-if="section.id === 'references'" class="fas fa-user-check card-icon"></i>
                  <i v-else class="fas fa-layer-group card-icon"></i>
                  <h3 class="card-title"><span v-html="section.name || section.title || (section.id === 'education' ? 'Học vấn' : section.id === 'experience' ? 'Kinh nghiệm làm việc' : section.id)"></span></h3>
                </div>
                <i class="fas fa-ellipsis-v text-blue-500 text-lg"></i>
              </div>
              
              <div class="card-body">
                <div v-if="section.desc || section.description || section.content || section.value" class="html-content mb-3" v-html="formatDesc(section.desc || section.description || section.content || section.value)"></div>
                
                <ul class="bullet-list" v-if="['skills', 'it_skills', 'languages', 'hobbies'].includes(section.id)">
                  <li v-for="(item, i) in (section.items?.length ? section.items : getMockData(section.id))" :key="i" class="item-container" :class="{'no-bullet': !item.level && item.name && /<[a-z][\s\S]*>/i.test(item.name)}">
                    <template v-if="typeof item === 'object'">
                      <div class="flex flex-col paginated-item">
                        <span v-if="item.name && /<[a-z][\s\S]*>/i.test(item.name)" v-html="item.name"></span>
                        <span class="text-gray-600" v-else><span v-html="item.name || item.title || item.info"></span></span>
                        <span class="text-gray-500 text-xs mt-0.5" v-if="item.level"><span v-html="item.level"></span></span>
                      </div>
                    </template>
                    <template v-else>
                      <div class="html-content" v-html="formatDesc(item)"></div>
                    </template>
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop.prevent="$emit('removeItem', section.id, i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                  </li>
                </ul>

                <div class="detailed-list" v-else>
                  <div v-for="(item, i) in (section.items?.length ? section.items : getMockData(section.id))" :key="i" class="item-container relative mb-5 last:mb-0">
                    <template v-if="typeof item === 'object'">
                      <div class="paginated-item">
                        <div class="flex justify-between items-start mb-1" v-if="item.company || item.school || item.organization || item.name || item.title || item.time || item.year || item.date">
                          <span class="entry-entity">
                            <template v-if="item.name && /<[a-z][\s\S]*>/i.test(item.name)"><span v-html="item.name"></span></template>
                            <template v-else><span v-html="item.company || item.school || item.organization || item.name || item.title"></span></template>
                          </span>
                          <span class="entry-time badge-time" v-if="item.time || item.year || item.date"><span v-html="item.time || item.year || item.date"></span></span>
                        </div>
                        <div class="entry-role" v-if="item.role || item.position || item.major">
                          <span v-html="item.role || item.position || item.major"></span>
                        </div>
                      </div>

                      <div class="text-[13px] text-gray-500 mt-1 paginated-item" v-if="item.gradType || item.info || item.contact">
                        <div v-if="item.gradType" class="text-gray-400"><span v-html="item.gradType"></span></div>
                        <div v-if="item.info || item.contact" class="html-content" v-html="formatDesc(item.info || item.contact)"></div>
                      </div>

                      <div class="html-content mt-2" v-if="item.desc || item.description" v-html="formatDesc(item.desc || item.description)"></div>
                    </template>
                    <template v-else>
                      <div class="html-content" v-html="formatDesc(item)"></div>
                    </template>
                    
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop.prevent="$emit('removeItem', section.id, i)" class="delete-item-btn no-print" style="top: 0; right: -10px;"><i class="fas fa-times"></i></button>
                  </div>
                </div>
              </div>
            </div>
            </template>
          </draggable>
        </aside>

        <main class="right-column">
          <draggable
            v-model="mainSectionsWritable"
            item-key="id"
            group="sections"
            class="flex flex-col gap-[25px] w-full cursor-move"
            @end="onDragEnd"
            animation="200"
            ghost-class="opacity-30"
            :delay="100"
            :delayOnTouchOnly="true"
            :fallbackTolerance="5"
            filter=".nav-btn, .delete-item-btn, .contact-item-btns, .html-content, input"
          >
            <template #item="{ element: section }">
            <div v-show="section.isVisible" class="custom-card section-block cursor-pointer hover:bg-black/5 transition-colors"
                 :class="{ 'section-active': selectedSectionId === section.id }"
                 @click.stop="toggleSection(section.id)" :data-section-id="section.id">
              
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
                  <button class="nav-btn" @click.stop.prevent="moveHorizontal(section.id)"><i class="fas fa-exchange-alt"></i></button>
                  <button class="nav-btn" @click.stop.prevent="$emit('moveUp', section.id, rightIds)"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop.prevent="$emit('moveDown', section.id, rightIds)"><i class="fas fa-chevron-down"></i></button>
                  <button class="nav-btn nav-btn-danger close-active-btn" @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" title="Ẩn mục này"><i class="fas fa-times"></i></button>
                </div>
              </transition>

              <div class="card-header paginated-item">
                <div class="card-title-group">
                  <i v-if="section.id === 'education'" class="fas fa-graduation-cap card-icon"></i>
                  <i v-else-if="section.id === 'experience'" class="fas fa-briefcase card-icon"></i>
                  <i v-else-if="section.id === 'skills' || section.id === 'it_skills' || section.id === 'languages'" class="fas fa-pen card-icon"></i>
                  <i v-else-if="section.id === 'activities'" class="fas fa-users card-icon"></i>
                  <i v-else-if="section.id === 'project' || section.id === 'projects'" class="fas fa-project-diagram card-icon"></i>
                  <i v-else-if="section.id === 'awards'" class="fas fa-trophy card-icon"></i>
                  <i v-else-if="section.id === 'certifications' || section.id === 'certificates'" class="fas fa-certificate card-icon"></i>
                  <i v-else-if="section.id === 'references'" class="fas fa-user-check card-icon"></i>
                  <i v-else class="fas fa-layer-group card-icon"></i>
                  <h3 class="card-title"><span v-html="section.name || section.title || (section.id === 'skills' ? 'Các kỹ năng' : section.id.toUpperCase())"></span></h3>
                </div>
                <i class="fas fa-ellipsis-v text-blue-500 text-lg"></i>
              </div>
              
              <div class="card-body">
                <div v-if="section.desc || section.description || section.content || section.value" class="html-content mb-3" v-html="formatDesc(section.desc || section.description || section.content || section.value)"></div>
                
                <ul class="bullet-list" v-if="['skills', 'it_skills', 'languages', 'hobbies'].includes(section.id)">
                  <li v-for="(item, i) in (section.items?.length ? section.items : getMockData(section.id))" :key="i" class="item-container" :class="{'no-bullet': !item.level && item.name && /<[a-z][\s\S]*>/i.test(item.name)}">
                    <template v-if="typeof item === 'object'">
                      <div class="flex flex-col paginated-item">
                        <span v-if="item.name && /<[a-z][\s\S]*>/i.test(item.name)" v-html="item.name"></span>
                        <span class="text-gray-600" v-else><span v-html="item.name || item.title || item.info"></span></span>
                        <span class="text-gray-500 text-xs mt-0.5" v-if="item.level"><span v-html="item.level"></span></span>
                      </div>
                    </template>
                    <template v-else>
                      <div class="html-content" v-html="formatDesc(item)"></div>
                    </template>
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop.prevent="$emit('removeItem', section.id, i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                  </li>
                </ul>

                <div class="detailed-list" v-else>
                  <div v-for="(item, i) in (section.items?.length ? section.items : getMockData(section.id))" :key="i" class="item-container relative mb-5 last:mb-0">
                    <template v-if="typeof item === 'object'">
                      <div class="paginated-item">
                        <div class="flex justify-between items-start mb-1" v-if="item.company || item.school || item.organization || item.name || item.title || item.time || item.year || item.date">
                          <span class="entry-entity">
                            <template v-if="item.name && /<[a-z][\s\S]*>/i.test(item.name)"><span v-html="item.name"></span></template>
                            <template v-else><span v-html="item.company || item.school || item.organization || item.name || item.title"></span></template>
                          </span>
                          <span class="entry-time badge-time" v-if="item.time || item.year || item.date"><span v-html="item.time || item.year || item.date"></span></span>
                        </div>
                        <div class="entry-role" v-if="item.role || item.position || item.major">
                          <span v-html="item.role || item.position || item.major"></span>
                        </div>
                      </div>

                      <div class="text-[13px] text-gray-500 mt-1 paginated-item" v-if="item.gradType || item.info || item.contact">
                        <div v-if="item.gradType" class="text-gray-400"><span v-html="item.gradType"></span></div>
                        <div v-if="item.info || item.contact" class="html-content" v-html="formatDesc(item.info || item.contact)"></div>
                      </div>

                      <div class="html-content mt-2" v-if="item.desc || item.description" v-html="formatDesc(item.desc || item.description)"></div>
                    </template>
                    <template v-else>
                      <div class="html-content" v-html="formatDesc(item)"></div>
                    </template>
                    
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop.prevent="$emit('removeItem', section.id, i)" class="delete-item-btn no-print" style="top: 0; right: -10px;"><i class="fas fa-times"></i></button>
                  </div>
                </div>
              </div>
            </div>
            </template>
          </draggable>
        </main>
      </div>

    </div>

    <template v-for="p in (pageCount - 1)" :key="'pb-' + p">
      <div class="page-break-indicator no-print" :style="{ top: `calc(${p * 297}mm - 8px)` }">
        <div class="page-break-bar"></div>
        <span class="page-break-label">Ngắt trang {{ p + 1 }}</span>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, ref, onMounted, nextTick, watch, onUnmounted, toRaw } from 'vue'
import draggable from 'vuedraggable'

const cvRoot = ref(null)
const pageCount = ref(1)
const selectedSectionId = ref(null)

// ─── CONTACT ITEMS: Danh sách động có thể sắp xếp / ẩn ───
const contactIcons = {
  birthDate: '<i class="fas fa-calendar-alt"></i>',
  phone: '<i class="fas fa-phone-alt"></i>',
  email: '<i class="fas fa-envelope"></i>',
  address: '<i class="fas fa-map-marker-alt"></i>'
}

const contactOrder = ref(['birthDate', 'phone', 'email', 'address'])
const hiddenContacts = ref([])

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  return g[key] || ''
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
  if (idx < 2) return
  swapContact(idx, idx - 2)
}

const moveContactDown = (idx) => {
  if (idx + 2 >= contactItems.value.length) return
  swapContact(idx, idx + 2)
}

const moveContactLeft = (idx) => {
  if (idx % 2 === 0) return
  swapContact(idx, idx - 1)
}

const moveContactRight = (idx) => {
  if (idx % 2 !== 0 || idx + 1 >= contactItems.value.length) return
  swapContact(idx, idx + 1)
}

const swapContact = (idxA, idxB) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && getContactValue(k) !== '')
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
    if (props.resumeData.general[key] !== undefined) {
      props.resumeData.general[key] = ''
    }
    hiddenContacts.value.push(key)
    requestPagination()
  }
}

const props = defineProps({
  resumeData: { type: Object, required: true }
})

const emit = defineEmits(['moveUp', 'moveDown', 'removeItem', 'moveHorizontal'])

const isEmpty = (val) => {
  if (!val) return true;
  if (typeof val !== 'string') return false;
  const clean = val
    .replace(/<\/?[^>]+(>|$)/g, "")
    .replace(/&[#a-z0-9]+;/ig, "")
    .replace(/\s+/g, "")
    .toLowerCase();
  return clean === '' || clean === 'br';
}

const stripTags = (val) => {
  if (!val || typeof val !== 'string') return val;
  return val
    .replace(/<\/?[^>]+(>|$)/g, '')
    .replace(/&[#a-z0-9]+;/ig, '')
    .trim();
}

const getMockData = (sectionId) => {
  return [{ name: 'Chưa có dữ liệu' }]
}

// ── QUẢN LÝ ẨN/HIỆN & CHIA CỘT ──────────────────────────────────────
const localLeftKeys = ref(['education', 'experience', 'activities', 'project'])

watch(() => props.resumeData?.leftColKeys, (newVal) => {
  if (newVal && Array.isArray(newVal)) {
    localLeftKeys.value = newVal
  }
}, { immediate: true })

const moveHorizontal = (id) => {
  if (localLeftKeys.value.includes(id)) {
    localLeftKeys.value = localLeftKeys.value.filter(k => k !== id)
  } else {
    localLeftKeys.value.push(id)
  }
  emit('moveHorizontal', id, toRaw(localLeftKeys.value))
  
  nextTick(() => {
    requestPagination()
  })
}

// ĐÃ FIX: Thuật toán tự động đảo "Học vấn" lên trên "Kinh nghiệm" nếu cả 2 cùng chung cột
const sidebarSections = computed(() => {
  const arr = (props.resumeData?.sections || []).filter(s => localLeftKeys.value.includes(s.id) && s.id !== 'summary')
  const eduIdx = arr.findIndex(s => s.id === 'education')
  const expIdx = arr.findIndex(s => s.id === 'experience')
  if (eduIdx !== -1 && expIdx !== -1 && eduIdx > expIdx) {
    const edu = arr.splice(eduIdx, 1)[0]
    arr.splice(expIdx, 0, edu)
  }
  return arr
})

const mainSections = computed(() => {
  const arr = (props.resumeData?.sections || []).filter(s => !localLeftKeys.value.includes(s.id) && s.id !== 'summary')
  const eduIdx = arr.findIndex(s => s.id === 'education')
  const expIdx = arr.findIndex(s => s.id === 'experience')
  if (eduIdx !== -1 && expIdx !== -1 && eduIdx > expIdx) {
    const edu = arr.splice(eduIdx, 1)[0]
    arr.splice(expIdx, 0, edu)
  }
  return arr
})

const getActiveIds = (sourceArray) => sourceArray.filter(s => s.isVisible).map(s => s.id)
const leftIds = computed(() => getActiveIds(sidebarSections.value))
const rightIds = computed(() => getActiveIds(mainSections.value))

const sidebarSectionsWritable = ref([])
const mainSectionsWritable = ref([])

watch(sidebarSections, (newVal) => {
  sidebarSectionsWritable.value = [...newVal]
}, { immediate: true, deep: true })

watch(mainSections, (newVal) => {
  mainSectionsWritable.value = [...newVal]
}, { immediate: true, deep: true })

const onDragEnd = () => {
  const newOrder = []
  const newLeftKeys = []
  
  const summary = props.resumeData.sections.find(x => x.id === 'summary')
  if (summary) newOrder.push(summary)

  sidebarSectionsWritable.value.forEach(s => {
    const orig = props.resumeData.sections.find(x => x.id === s.id)
    if (orig) {
      newLeftKeys.push(s.id)
      newOrder.push(orig)
    }
  })
  
  mainSectionsWritable.value.forEach(s => {
    const orig = props.resumeData.sections.find(x => x.id === s.id)
    if (orig) {
      newOrder.push(orig)
    }
  })
  
  props.resumeData.sections.forEach(s => {
    if (!newOrder.find(x => x.id === s.id)) {
      newOrder.push(s)
    }
  })
  
  localLeftKeys.value = newLeftKeys
  emit('moveHorizontal', null, toRaw(localLeftKeys.value))
  
  props.resumeData.sections.splice(0, props.resumeData.sections.length, ...newOrder)
  requestPagination()
}

const getOrder = (id, activeArray) => activeArray.indexOf(id) + 1

const toggleSection = (id) => { selectedSectionId.value = selectedSectionId.value === id ? null : id }

const closeAllSections = () => {
  selectedSectionId.value = null;
}

onMounted(() => {
  if (props.resumeData?.sections) {
    // ĐÃ FIX: Bật sẵn Mục tiêu nghề nghiệp (summary), Học vấn, Kinh nghiệm, Kỹ năng
    const ACTIVE_SECTIONS = ['summary', 'education', 'experience', 'skills']
    props.resumeData.sections.forEach(sec => {
      sec.isVisible = ACTIVE_SECTIONS.includes(sec.id)
    })
  }
  requestPagination()
  window.addEventListener('resize', requestPagination)
})

// ── UTILITIES & HTML FORMAT ──────────────────────────────────────────────
const formatDesc = (text) => {
  if (isEmpty(text)) return '';
  if (!/<[a-z][\s\S]*>/i.test(text)) {
    return text.split('\n').map(l => l.trim()).filter(Boolean)
      .map(l => `<div class="paginated-item">${l}</div>`).join('')
  }
  const tmp = document.createElement('div')
  tmp.innerHTML = text
  const out = document.createElement('div')
  Array.from(tmp.childNodes).forEach(node => {
    if (node.nodeType === Node.TEXT_NODE) {
      if (node.textContent.trim()) {
        const d = document.createElement('div'); d.className = 'paginated-item'; d.appendChild(node.cloneNode(true)); out.appendChild(d)
      }
    } else if (node.nodeType === Node.ELEMENT_NODE) {
      if (node.tagName === 'BR') {
        const d = document.createElement('div'); d.className = 'paginated-item h-[14px]'; out.appendChild(d)
      } else if (['UL','OL'].includes(node.tagName)) {
        Array.from(node.children).forEach(li => li.classList.add('paginated-item'))
        out.appendChild(node.cloneNode(true))
      } else {
        node.classList.add('paginated-item'); out.appendChild(node.cloneNode(true))
      }
    }
  })
  return out.innerHTML
}

// ── PAGINATION LOGIC ──────────────────────────────────
const A4_W_MM = 210
const A4_H_MM = 297

let paginateTimer = null

const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 100)
}

const doPagination = async () => {
  if (!cvRoot.value) return

  const allElements = cvRoot.value.querySelectorAll('.paginated-item')
  allElements.forEach(el => { el.style.marginTop = '0px' })
  await nextTick()

  const pxPerMm = cvRoot.value.offsetWidth / A4_W_MM
  const pageH = A4_H_MM * pxPerMm
  const bottomSafeZone = 20 * pxPerMm 
  const topMargin = 25 * pxPerMm 

  const getOffsetTop = (el) => {
    let offset = 0
    let curr = el
    while (curr && curr !== cvRoot.value) {
      offset += curr.offsetTop
      curr = curr.offsetParent
    }
    return offset
  }

  const paginatedItems = cvRoot.value.querySelectorAll('.paginated-item')

  paginatedItems.forEach((el) => {
    if(el.offsetHeight === 0) return
    const top = getOffsetTop(el)
    const topInPage = top % pageH
    const bottomInPage = topInPage + el.offsetHeight
    
    if (bottomInPage > (pageH - bottomSafeZone)) {
       if (!el.style.marginTop || el.style.marginTop === '0px') {
           const distToNextPage = pageH - topInPage + topMargin
           el.style.marginTop = `${distToNextPage}px`
       }
    }
  })

  let maxBottom = 0
  const blocks = cvRoot.value.querySelectorAll('.section-block, .paginated-item')
  blocks.forEach(el => {
    const b = getOffsetTop(el) + el.offsetHeight
    if (b > maxBottom) maxBottom = b
  })
  pageCount.value = Math.max(1, Math.ceil(maxBottom / pageH))
}

watch(() => props.resumeData, requestPagination, { deep: true })
onUnmounted(() => {
  window.removeEventListener('resize', requestPagination)
  if (paginateTimer) clearTimeout(paginateTimer)
})
</script>

<style scoped>
@import url('https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css');
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap');

.cv-browser-wrapper { 
  width: 210mm; font-family: 'Inter', sans-serif; 
  position: relative; color: #333; overflow: hidden; box-sizing: border-box;
  background-color: #F8FAFC;
  background-image: 
    linear-gradient(#E2E8F0 1px, transparent 1px),
    linear-gradient(90deg, #E2E8F0 1px, transparent 1px);
  background-size: 25px 25px;
}

/* ================== BROWSER UI ================== */
.browser-header {
  background: #fff; border-bottom: 1px solid #E2E8F0;
  box-shadow: 0 2px 4px rgba(0,0,0,0.02);
  margin-bottom: 30px;
}
.browser-tabs {
  display: flex; align-items: flex-end; padding: 10px 15px 0 15px;
  background: #F1F5F9; border-bottom: 1px solid #E2E8F0;
}
.window-controls { display: flex; gap: 6px; align-items: center; padding-bottom: 10px; margin-right: 20px; }
.window-controls .dot { width: 10px; height: 10px; border-radius: 50%; }
.dot.red { background: #FF5F56; }
.dot.yellow { background: #FFBD2E; }
.dot.green { background: #27C93F; }

.tab {
  background: #fff; padding: 8px 20px; border-radius: 8px 8px 0 0;
  font-size: 12px; font-weight: 600; color: #1E40AF;
  display: flex; align-items: center; gap: 15px;
  border-top: 1px solid #E2E8F0; border-left: 1px solid #E2E8F0; border-right: 1px solid #E2E8F0;
}
.close-tab { color: #94A3B8; font-size: 10px; cursor: pointer; }
.new-tab { padding: 8px 15px; color: #64748B; font-size: 14px; cursor: pointer; margin-bottom: 5px; }

.browser-url-bar {
  display: flex; align-items: center; padding: 8px 20px; gap: 15px;
}
.nav-arrows { display: flex; gap: 15px; color: #1E40AF; font-size: 14px; }
.url-box {
  flex: 1; background: #F1F5F9; border-radius: 20px; padding: 6px 15px;
  display: flex; align-items: center; gap: 10px;
}
.url-icon { color: #1E40AF; font-size: 13px; }
.url-text { font-size: 13px; color: #475569; font-weight: 500; }
.browser-actions { display: flex; gap: 15px; color: #1E40AF; font-size: 14px; }

/* ================== CV CONTENT ================== */
.cv-container { padding: 0 35px 35px 35px; }

.custom-card { 
  background: #fff; border-radius: 20px; padding: 25px 30px; 
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.03); 
}

/* HEADER PROFILE */
.profile-card {
  display: flex; align-items: center; gap: 35px; margin-bottom: 25px;
}
.avatar-container { 
  width: 130px; height: 130px; border-radius: 50%; overflow: hidden; 
  border: 4px solid #F1F5F9; flex-shrink: 0; 
}
.avatar-img { width: 100%; height: 100%; object-fit: cover; }
.avatar-placeholder { width: 100%; height: 100%; background: #F8FAFC; display: flex; align-items: center; justify-content: center; }

.header-info { flex: 1; }
.fullname { font-size: 32px; font-weight: 800; color: #1D4ED8; text-transform: uppercase; margin: 0 0 5px 0; letter-spacing: 0.5px; }
.job-title { font-size: 16px; font-weight: 600; color: #3B82F6; margin: 0 0 20px 0; }

.contact-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.contact-item { display: flex; align-items: center; gap: 10px; font-size: 12.5px; color: #64748B; font-weight: 500; }
.contact-item i { width: 14px; text-align: center; font-size: 13px; color: #94A3B8; }

/* 2 COLUMNS LAYOUT */
.main-layout { display: flex; gap: 25px; align-items: flex-start; position: relative; z-index: 10; margin-top: 15px; }
.left-column { width: 55%; display: flex; flex-direction: column; gap: 25px; }
.right-column { width: 45%; display: flex; flex-direction: column; gap: 25px; }

/* CARD HEADER & BODY */
.card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.card-title-group { display: flex; align-items: center; gap: 12px; }
.card-icon { font-size: 18px; color: #3B82F6; }
.card-title { font-size: 18px; font-weight: 800; color: #1D4ED8; margin: 0; }

.entry-entity { font-size: 15px; font-weight: 700; color: #1D4ED8; }
.entry-role { font-size: 13px; font-weight: 600; color: #475569; margin-bottom: 4px; }
.badge-time { background: #DBEAFE; color: #2563EB; padding: 3px 12px; border-radius: 20px; font-size: 12px; font-weight: 700; }

/* List Bullet */
.bullet-list { list-style: none; padding: 0; margin: 0; }
.bullet-list li { position: relative; font-size: 12.5px; margin-bottom: 12px; padding-left: 15px; line-height: 1.6; }
.bullet-list li::before { content: '•'; position: absolute; left: 0; top: 0; color: #94A3B8; font-weight: 900; font-size: 14px; }
.bullet-list li.no-bullet { padding-left: 0; }
.bullet-list li.no-bullet::before { display: none; }

/* ================== HTML CONTENT ================== */
.paginated-item { 
  transition: none; 
  page-break-inside: avoid !important;
  break-inside: avoid !important;
}

:deep(.html-content) { font-size: 12.5px; line-height: 1.6; color: #64748B; text-align: justify; }
:deep(.html-content ul) { list-style-type: none !important; padding-left: 12px !important; margin: 0; }
:deep(.html-content li) { position: relative; margin-bottom: 4px; font-size: 12.5px; line-height: 1.6; }
:deep(.html-content li::before) { content: '•'; position: absolute; left: -12px; color: #94A3B8; font-weight: 900; }
:deep(.html-content b), :deep(.html-content strong) { font-weight: 700 !important; color: #334155; }

/* INTERACTION & BUTTONS */
.section-block { position: relative; border: 2px solid transparent; transition: 0.2s; cursor: pointer; border-radius: 20px; }
.section-active { border-color: #3B82F6 !important; box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.1); z-index: 20; }

.fade-btns-enter-active, .fade-btns-leave-active { transition: opacity 0.2s ease, transform 0.2s ease; }
.fade-btns-enter-from, .fade-btns-leave-to { opacity: 0; transform: scale(0.9); }
.nav-btns { position: absolute; right: 20px; top: 18px; display: flex; gap: 6px; z-index: 100; }
.nav-btn { 
  background: #3B82F6; color: #fff; border: none; width: 28px; height: 28px; 
  border-radius: 6px; cursor: pointer; display: flex; align-items: center; 
  justify-content: center; font-size: 13px; box-shadow: 0 2px 4px rgba(59, 130, 246, 0.3);
}
.nav-btn:hover { background: #2563EB; }

/* NÚT TẮT ACTIVE MÀU ĐỎ */
.nav-btn.close-active-btn, .nav-btn-danger {
  background: #EF4444 !important; 
}
.nav-btn.close-active-btn:hover, .nav-btn-danger:hover {
  background: #DC2626 !important;
}

.contact-block {
    border-radius: 12px;
    border: 2px solid transparent;
    padding: 6px 10px;
    margin: -6px -10px;
    cursor: pointer;
    transition: border-color 0.18s ease, box-shadow 0.18s ease;
}
.contact-block.contact-active {
    border: 2px solid #3B82F6 !important;
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

.delete-item-btn { position: absolute; right: -5px; top: 0; width: 18px; height: 18px; background: #EF4444; color: white; border: none; border-radius: 50%; cursor: pointer; font-size: 10px; display: flex; align-items: center; justify-content: center; z-index: 50; }

.page-break-indicator { position: absolute; left: 0; width: 100%; z-index: 50; display: flex; flex-direction: column; align-items: center; pointer-events: none; }
.page-break-bar { width: 100%; height: 2px; background: rgba(0,0,0,0.1); border-top: 1px dashed rgba(0,0,0,0.2); }
.page-break-label { font-size: 9px; text-transform: uppercase; font-weight: 700; color: #999; background: #fff; padding: 2px 10px; margin-top: -8px; }

@media print { 
  @page { margin: 0; size: A4; }
  
  .no-print { display: none !important; } 
  .no-print-bg { print-color-adjust: exact; -webkit-print-color-adjust: exact; }
  .section-block,
  .section-block.section-active {
    cursor: default !important;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.03) !important;
    border-color: transparent !important;
  }
}
</style>