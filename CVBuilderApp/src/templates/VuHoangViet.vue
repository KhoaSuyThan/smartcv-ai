<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="cv-green-wrapper"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm` }"
    @click.self="selectedSectionId = null"
  >
    <div class="cv-container">
      
      <aside class="left-column">
        
        <div class="profile-section relative">
          <h1 class="fullname paginated-item">{{ !isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'Vũ Hoàng Việt' }}</h1>
          <div class="job-title paginated-item">{{ !isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'Thực tập sinh Kiểm toán' }}</div>
          
          <div class="avatar-container paginated-item">
            <img v-if="resumeData.general.avatarUrl || resumeData.general.avatar" :src="resumeData.general.avatarUrl || resumeData.general.avatar" class="avatar-img" alt="Avatar"/>
            <div v-else class="avatar-placeholder">
              <svg width="60" height="60" viewBox="0 0 24 24" fill="none" stroke="#ccc" stroke-width="1.5">
                <circle cx="12" cy="8" r="4"/><path d="M4 20c0-4 3.6-7 8-7s8 3 8 7"/>
              </svg>
            </div>
          </div>

          <div v-if="contactItems.length > 0" class="contact-list relative contact-block"
               :class="{ 'contact-active': selectedSectionId === 'contact' }"
               @click.stop="selectedSectionId = selectedSectionId === 'contact' ? null : 'contact'">
            <div v-for="(ci, ciIdx) in contactItems" :key="ci.key"
                 class="contact-item paginated-item relative contact-item-container">
              <div class="w-6 h-6 rounded-full flex items-center justify-center shrink-0 mr-2">
                <svg class="w-3 h-3" fill="currentColor" viewBox="0 0 20 20" v-html="ci.icon"></svg>
              </div>
              <span class="break-all" v-html="ci.value"></span>
              <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print">
                <button v-if="ciIdx > 0" @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn" title="Di chuyển lên" style="padding:3px">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                </button>
                <button v-if="ciIdx < contactItems.length - 1" @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn" title="Di chuyển xuống" style="padding:3px">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                </button>
                <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn btn-danger" title="Ẩn mục này" style="padding:3px">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </div>

        <div class="left-sortable-area" style="display: flex; flex-direction: column;">
          
          <template v-for="section in sidebarSections.filter(s => s.id === 'summary')" :key="section.id">
            <div v-show="section.isVisible" :data-section-id="section.id" class="section-block left-block cursor-pointer hover:bg-black/5 transition-colors" :style="{ order: getOrder(section.id, leftIds) }" :class="{ 'section-active': selectedSectionId === section.id }" @click.stop="toggleSection(section.id)">
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print" data-html2canvas-ignore="true">
                  <button class="nav-btn" @click.stop="moveUp(section.id, leftIds)" title="Lên"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop="moveDown(section.id, leftIds)" title="Xuống"><i class="fas fa-chevron-down"></i></button>
                  <button class="nav-btn" @click.stop="moveHorizontal(section.id, 'right')" title="Chuyển sang phải"><i class="fas fa-arrow-right"></i></button>
                  <button class="nav-btn btn-danger" @click.stop="hideSection(section.id)" title="Ẩn mục"><i class="fas fa-times"></i></button>
                </div>
              </transition>
              
              <div class="pill-header paginated-item"><span v-html="section.name || section.title || 'Mục tiêu nghề nghiệp'"></span></div>
              
              <div class="content-area">
                <div v-if="!isEmpty(resumeData.general.summary)" class="html-content" v-html="formatDesc(resumeData.general.summary)"></div>
                <div v-else class="html-content text-placeholder">
                  <div class="paginated-item">Chưa có dữ liệu</div>
                </div>
              </div>
            </div>
          </template>

          <template v-for="section in sidebarSections.filter(s => ['skills', 'it_skills', 'languages'].includes(s.id))" :key="section.id">
            <div v-show="section.isVisible" :data-section-id="section.id" class="section-block left-block cursor-pointer hover:bg-black/5 transition-colors" :style="{ order: getOrder(section.id, leftIds) }" :class="{ 'section-active': selectedSectionId === section.id }" @click.stop="toggleSection(section.id)">
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print" data-html2canvas-ignore="true">
                  <button class="nav-btn" @click.stop="moveUp(section.id, leftIds)" title="Lên"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop="moveDown(section.id, leftIds)" title="Xuống"><i class="fas fa-chevron-down"></i></button>
                  <button class="nav-btn" @click.stop="moveHorizontal(section.id, 'right')" title="Chuyển sang phải"><i class="fas fa-arrow-right"></i></button>
                  <button class="nav-btn btn-danger" @click.stop="hideSection(section.id)" title="Ẩn mục"><i class="fas fa-times"></i></button>
                </div>
              </transition>

              <div class="pill-header paginated-item"><span v-html="section.name || section.title || 'Kỹ năng chuyên môn'"></span></div>
              
              <div class="content-area">
                <ul class="left-list">
                  <li v-for="(item, i) in (section.items?.length ? section.items : [{name: 'Chưa có dữ liệu'}])" :key="i" class="item-container paginated-item">
                    <template v-if="typeof item === 'object'">
                      <span v-html="item.name || ''"></span> <span v-html="item.level ? ' - ' + item.level : ''"></span>
                    </template>
                    <template v-else>{{ item }}</template>
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop="$emit('removeItem', section.id, i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                  </li>
                </ul>
              </div>
            </div>
          </template>

          <template v-for="section in sidebarSections.filter(s => s.id === 'hobbies')" :key="section.id">
            <div v-show="section.isVisible" :data-section-id="section.id" class="section-block left-block cursor-pointer hover:bg-black/5 transition-colors" :style="{ order: getOrder(section.id, leftIds) }" :class="{ 'section-active': selectedSectionId === section.id }" @click.stop="toggleSection(section.id)">
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print" data-html2canvas-ignore="true">
                  <button class="nav-btn" @click.stop="moveUp(section.id, leftIds)" title="Lên"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop="moveDown(section.id, leftIds)" title="Xuống"><i class="fas fa-chevron-down"></i></button>
                  <button class="nav-btn" @click.stop="moveHorizontal(section.id, 'right')" title="Chuyển sang phải"><i class="fas fa-arrow-right"></i></button>
                  <button class="nav-btn btn-danger" @click.stop="hideSection(section.id)" title="Ẩn mục"><i class="fas fa-times"></i></button>
                </div>
              </transition>

              <div class="pill-header paginated-item"><span v-html="section.name || section.title || 'Sở thích'"></span></div>
              
              <div class="content-area">
                <ul class="left-list">
                  <li v-for="(item, i) in (section.items?.length ? section.items : [{name: 'Chưa có dữ liệu'}])" :key="i" class="item-container paginated-item">
                    <span v-html="typeof item === 'object' ? (item.name || item.title || '') : item"></span>
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop="$emit('removeItem', section.id, i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                  </li>
                </ul>
              </div>
            </div>
          </template>

          <template v-for="section in sidebarSections.filter(s => !['summary', 'skills', 'it_skills', 'languages', 'hobbies'].includes(s.id))" :key="section.id">
            <div v-show="section.isVisible" :data-section-id="section.id" class="section-block left-block cursor-pointer hover:bg-black/5 transition-colors" :style="{ order: getOrder(section.id, leftIds) }" :class="{ 'section-active': selectedSectionId === section.id }" @click.stop="toggleSection(section.id)">
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print" data-html2canvas-ignore="true">
                  <button class="nav-btn" @click.stop="moveUp(section.id, leftIds)" title="Lên"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop="moveDown(section.id, leftIds)" title="Xuống"><i class="fas fa-chevron-down"></i></button>
                  <button class="nav-btn" @click.stop="moveHorizontal(section.id, 'right')" title="Chuyển sang phải"><i class="fas fa-arrow-right"></i></button>
                  <button class="nav-btn btn-danger" @click.stop="hideSection(section.id)" title="Ẩn mục"><i class="fas fa-times"></i></button>
                </div>
              </transition>

              <div class="pill-header paginated-item"><span v-html="section.name || section.title || section.id.toUpperCase()"></span></div>
              
              <div class="content-area">
                <div v-if="section.desc || section.description || section.content || section.value" class="html-content mb-2 paginated-item" v-html="formatDesc(section.desc || section.description || section.content || section.value)"></div>
                
                <ul class="left-list">
                  <li v-for="(item, i) in (section.items?.length ? section.items : ((section.desc || section.description || section.content || section.value) ? [] : [{ name: 'Chưa có dữ liệu' }]))" :key="i" class="item-container paginated-item">
                    <template v-if="typeof item === 'object'">
                      <div v-if="item.year || item.time || item.date" class="text-time font-bold"><span v-html="item.year || item.time || item.date"></span></div>
                      <div v-if="item.company || item.school || item.organization || item.name || item.title" class="font-bold"><span v-html="item.company || item.school || item.organization || item.name || item.title"></span></div>
                      <div v-if="item.role || item.position || item.degree || item.major || item.level" class="text-role"><span v-html="item.role || item.position || item.degree || item.major || item.level"></span></div>
                      <div v-if="item.gradType" class="text-role"><strong>Xếp loại:</strong> <span v-html="item.gradType"></span></div>
                      <div v-if="item.info || item.contact" class="html-content" v-html="formatDesc(item.info || item.contact)"></div>
                      <div v-if="item.desc || item.description" class="html-content mt-1" v-html="formatDesc(item.desc || item.description)"></div>
                    </template>
                    <template v-else>
                      <div class="html-content" v-html="formatDesc(item)"></div>
                    </template>
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop="$emit('removeItem', section.id, i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                  </li>
                </ul>
              </div>
            </div>
          </template>

        </div>
      </aside>

      <main class="right-column relative">
        <div class="right-sortable-area timeline-container" style="display: flex; flex-direction: column;">
          
          <div v-show="rightIds.length > 0" class="timeline-line no-print-line"></div>

          <!-- Education -->
          <template v-for="section in mainSections.filter(s => s.id === 'education')" :key="section.id">
            <div v-show="section.isVisible" :data-section-id="section.id" class="section-block right-block cursor-pointer hover:bg-black/5 transition-colors" :style="{ order: getOrder(section.id, rightIds) }" :class="{ 'section-active': selectedSectionId === section.id }" @click.stop="toggleSection(section.id)">
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print" data-html2canvas-ignore="true">
                  <button class="nav-btn" @click.stop="moveUp(section.id, rightIds)" title="Lên"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop="moveDown(section.id, rightIds)" title="Xuống"><i class="fas fa-chevron-down"></i></button>
                  <button class="nav-btn" @click.stop="moveHorizontal(section.id, 'left')" title="Chuyển sang trái"><i class="fas fa-arrow-left"></i></button>
                  <button class="nav-btn btn-danger" @click.stop="hideSection(section.id)" title="Ẩn mục"><i class="fas fa-times"></i></button>
                </div>
              </transition>
              <div class="timeline-dot"></div>
              <div class="right-block-inner">
                <h3 class="right-title paginated-item"><span v-html="section.name || section.title || 'Quá trình Học vấn'"></span></h3>
                <div class="green-card">
                  <div v-for="(item, i) in (section.items?.length ? section.items : [{school: 'Chưa có dữ liệu'}])" :key="i" class="item-container relative mb-4 last:mb-0 paginated-item">
                    <div v-if="item.year || item.time" class="text-time"><span v-html="item.year || item.time"></span></div>
                    <div v-if="item.school || item.name" class="text-entity font-bold"><span v-html="item.school || item.name"></span></div>
                    <div v-if="item.major || item.degree" class="text-role"><span v-html="item.major || item.degree"></span></div>
                    <div v-if="item.gradType || item.info" class="text-role" style="margin-top: 2px;">
                      <strong>Xếp loại:</strong> <span v-html="item.gradType || item.info"></span>
                    </div>
                    <div v-if="item.desc || item.description" class="html-content mt-1" v-html="formatDesc(item.desc || item.description)"></div>
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop="$emit('removeItem', section.id, i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                  </div>
                </div>
              </div>
            </div>
          </template>

          <!-- Experience -->
          <template v-for="section in mainSections.filter(s => s.id === 'experience')" :key="section.id">
            <div v-show="section.isVisible" :data-section-id="section.id" class="section-block right-block cursor-pointer hover:bg-black/5 transition-colors" :style="{ order: getOrder(section.id, rightIds) }" :class="{ 'section-active': selectedSectionId === section.id }" @click.stop="toggleSection(section.id)">
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print" data-html2canvas-ignore="true">
                  <button class="nav-btn" @click.stop="moveUp(section.id, rightIds)" title="Lên"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop="moveDown(section.id, rightIds)" title="Xuống"><i class="fas fa-chevron-down"></i></button>
                  <button class="nav-btn" @click.stop="moveHorizontal(section.id, 'left')" title="Chuyển sang trái"><i class="fas fa-arrow-left"></i></button>
                  <button class="nav-btn btn-danger" @click.stop="hideSection(section.id)" title="Ẩn mục"><i class="fas fa-times"></i></button>
                </div>
              </transition>
              <div class="timeline-dot"></div>
              <div class="right-block-inner">
                <h3 class="right-title paginated-item"><span v-html="section.name || section.title || 'Kinh nghiệm làm việc'"></span></h3>
                <div class="green-card">
                  <div v-for="(item, i) in (section.items?.length ? section.items : [{company: 'Chưa có dữ liệu'}])" :key="i" class="item-container relative mb-4 last:mb-0 paginated-item">
                    <div v-if="item.year || item.time" class="text-time"><span v-html="item.year || item.time"></span></div>
                    <div v-if="item.company || item.name" class="text-entity font-normal"><span v-html="item.company || item.name"></span></div>
                    <div v-if="item.role || item.position" class="text-role font-bold"><span v-html="item.role || item.position"></span></div>
                    <div v-if="item.role || item.position || item.desc || item.description" class="divider-line"></div>
                    <div v-if="item.desc || item.description" class="html-content mt-1" v-html="formatDesc(item.desc || item.description)"></div>
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop="$emit('removeItem', section.id, i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                  </div>
                </div>
              </div>
            </div>
          </template>

          <!-- Activities -->
          <template v-for="section in mainSections.filter(s => s.id === 'activities')" :key="section.id">
            <div v-show="section.isVisible" :data-section-id="section.id" class="section-block right-block cursor-pointer hover:bg-black/5 transition-colors" :style="{ order: getOrder(section.id, rightIds) }" :class="{ 'section-active': selectedSectionId === section.id }" @click.stop="toggleSection(section.id)">
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print" data-html2canvas-ignore="true">
                  <button class="nav-btn" @click.stop="moveUp(section.id, rightIds)" title="Lên"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop="moveDown(section.id, rightIds)" title="Xuống"><i class="fas fa-chevron-down"></i></button>
                  <button class="nav-btn" @click.stop="moveHorizontal(section.id, 'left')" title="Chuyển sang trái"><i class="fas fa-arrow-left"></i></button>
                  <button class="nav-btn btn-danger" @click.stop="hideSection(section.id)" title="Ẩn mục"><i class="fas fa-times"></i></button>
                </div>
              </transition>
              <div class="timeline-dot"></div>
              <div class="right-block-inner">
                <h3 class="right-title paginated-item"><span v-html="section.name || section.title || 'Hoạt động'"></span></h3>
                <div class="green-card">
                  <div v-for="(item, i) in (section.items?.length ? section.items : [{company: 'Chưa có dữ liệu'}])" :key="i" class="item-container relative mb-5 last:mb-0 paginated-item">
                    <div v-if="item.year || item.time || item.date" class="text-time"><span v-html="item.year || item.time || item.date"></span></div>
                    <div v-if="item.company || item.organization || item.name" class="text-entity"><span v-html="item.company || item.organization || item.name"></span></div>
                    <div v-if="item.role" class="text-role font-normal mt-1"><span v-html="item.role"></span></div>
                    <div v-if="item.role || item.desc || item.description" class="divider-line"></div>
                    <div v-if="item.desc || item.description" class="html-content mt-1" v-html="formatDesc(item.desc || item.description)"></div>
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop="$emit('removeItem', section.id, i)" class="delete-item-btn no-print"><i class="fas fa-times"></i></button>
                  </div>
                </div>
              </div>
            </div>
          </template>

          <!-- Catch-all for other right column sections -->
          <template v-for="section in mainSections.filter(s => !['education', 'experience', 'activities'].includes(s.id))" :key="section.id">
            <div v-show="section.isVisible" :data-section-id="section.id" class="section-block right-block cursor-pointer hover:bg-black/5 transition-colors" :style="{ order: getOrder(section.id, rightIds) }" :class="{ 'section-active': selectedSectionId === section.id }" @click.stop="toggleSection(section.id)">
              <transition name="fade-btns">
                <div v-if="selectedSectionId === section.id" class="nav-btns no-print" data-html2canvas-ignore="true">
                  <button class="nav-btn" @click.stop="moveUp(section.id, rightIds)" title="Lên"><i class="fas fa-chevron-up"></i></button>
                  <button class="nav-btn" @click.stop="moveDown(section.id, rightIds)" title="Xuống"><i class="fas fa-chevron-down"></i></button>
                  <button class="nav-btn" @click.stop="moveHorizontal(section.id, 'left')" title="Chuyển sang trái"><i class="fas fa-arrow-left"></i></button>
                  <button class="nav-btn btn-danger" @click.stop="hideSection(section.id)" title="Ẩn mục"><i class="fas fa-times"></i></button>
                </div>
              </transition>
              <div class="timeline-dot"></div>
              <div class="right-block-inner">
                <h3 class="right-title paginated-item"><span v-html="section.name || section.title || section.id.toUpperCase()"></span></h3>
                <div class="green-card">
                  <div v-if="section.desc || section.description || section.content || section.value" class="html-content mb-3 paginated-item" v-html="formatDesc(section.desc || section.description || section.content || section.value)"></div>
                  <div v-for="(item, i) in (section.items?.length ? section.items : ((section.desc || section.description || section.content || section.value) ? [] : [{ name: 'Chưa có dữ liệu' }]))" :key="i" class="item-container relative mb-4 last:mb-0 paginated-item">
                    <div v-if="typeof item === 'object'">
                      <div v-if="item.year || item.time" class="text-time"><span v-html="item.year || item.time"></span></div>
                      <div v-if="item.name && /<[a-z][\s\S]*>/i.test(item.name)" class="html-content" v-html="formatDesc(item.name)"></div>
                      <div v-else-if="item.name || item.title" class="text-entity"><span v-html="item.name || item.title"></span></div>
                      <div v-if="item.role || item.position || item.level" class="text-role font-normal mt-1"><span v-html="item.role || item.position || item.level"></span></div>
                      <div v-if="item.info || item.contact" class="html-content" v-html="formatDesc(item.info || item.contact)"></div>
                      <div v-if="item.desc || item.description" class="html-content mt-1" v-html="formatDesc(item.desc || item.description)"></div>
                    </div>
                    <div v-else class="html-content" v-html="formatDesc(item)"></div>
                    <button v-if="selectedSectionId === section.id && section.items?.length" @click.stop="$emit('removeItem', section.id, i)" class="delete-item-btn no-print" data-html2canvas-ignore="true"><i class="fas fa-times"></i></button>
                  </div>
                </div>
              </div>
            </div>
          </template>

        </div>
      </main>

    </div>

    <!-- ĐƯỜNG PHÂN TRANG -->
    <template v-for="p in (pageCount - 1)" :key="'pb-' + p">
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
import { computed, ref, onMounted, nextTick, watch, onUnmounted, toRaw } from 'vue'

const cvRoot = ref(null)
const pageCount = ref(1)
const selectedSectionId = ref(null)

const props = defineProps({
  resumeData: { type: Object, required: true }
})

const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

const isEmpty = (v) => !v || v.toString().trim() === ''

// ─── CONTACT ITEMS: Danh sách động có thể sắp xếp / ẩn ───
const contactIcons = {
  birthDate: '<path d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" />',
  phone: '<path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z" />',
  email: '<path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z" /><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z" />',
  address: '<path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd" />'
}

const contactOrder = ref(['birthDate', 'phone', 'email', 'address'])
const hiddenContacts = ref([])

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  switch (key) {
    case 'birthDate': return isEmpty(g.birthDate) ? '' : g.birthDate
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

// ── QUẢN LÝ ẨN/HIỆN & CHIA CỘT ──────────────────────────────────────
const leftColKeys = ref(['summary', 'skills', 'it_skills', 'languages', 'hobbies'])

const sidebarSections = computed(() => {
  return (props.resumeData?.sections || []).filter(s => leftColKeys.value.includes(s.id))
})

const mainSections = computed(() => {
  return (props.resumeData?.sections || []).filter(s => !leftColKeys.value.includes(s.id))
})

const moveToLeft = (id) => {
  if (!leftColKeys.value.includes(id)) leftColKeys.value.push(id)
}
const moveToRight = (id) => {
  leftColKeys.value = leftColKeys.value.filter(k => k !== id)
}
const hideSection = (id) => {
  const sec = props.resumeData.sections.find(s => s.id === id)
  if (sec) sec.isVisible = false
  selectedSectionId.value = null
}

const getActiveIds = (sourceArray) => {
  return sourceArray.filter(s => s.isVisible).map(s => s.id)
}

const leftIds = computed(() => getActiveIds(sidebarSections.value))
const rightIds = computed(() => getActiveIds(mainSections.value))

const getOrder = (id, activeArray) => activeArray.indexOf(id) + 1

const toggleSection = (id) => { selectedSectionId.value = selectedSectionId.value === id ? null : id }
const moveUp = (id, arr) => emit('moveUp', id, toRaw(arr))
const moveDown = (id, arr) => emit('moveDown', id, toRaw(arr))
const moveHorizontal = (id, direction) => {
  if (direction === 'right') moveToRight(id)
  else moveToLeft(id)
}

// Kích hoạt mặc định 5 mục giống ảnh
onMounted(() => {
  if (props.resumeData?.sections) {
    const ACTIVE_SECTIONS = ['summary', 'skills', 'experience', 'education', 'activities']
    props.resumeData.sections.forEach(sec => {
      // Ép hiển thị các mục mặc định nếu chưa được set hoặc đang ẩn
      if (ACTIVE_SECTIONS.includes(sec.id)) {
        sec.isVisible = true
      }
    })
  }
  requestPagination()
  window.addEventListener('resize', requestPagination)
})

// ── UTILITIES & HTML FORMAT ──────────────────────────────────────────────
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

// ── PAGINATION LOGIC (FOOLPROOF MARGIN) ─────────────────────────
const A4_W_MM = 210
const A4_H_MM = 297
const MM_TO_PX = 3.779527559 // Chuẩn CSS 1mm = 3.7795px
let paginateTimer = null

const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 100)
}

const doPagination = async () => {
  if (!cvRoot.value) return

  // Khôi phục margin ban đầu
  const allElements = Array.from(cvRoot.value.querySelectorAll('.paginated-item'))
  allElements.forEach(el => {
    el.style.setProperty('margin-top', '0px', 'important')
  })
  
  await nextTick()

  const currentCvRect = cvRoot.value.getBoundingClientRect()
  // Tính tỷ lệ Zoom của trình duyệt/ứng dụng
  const unscaledWidth = 210 * MM_TO_PX
  const scale = (currentCvRect.width || unscaledWidth) / unscaledWidth
  
  const pageH = 297 * MM_TO_PX
  // Vùng đệm 30mm
  const bottomSafeZone = 30 * MM_TO_PX 
  const topPadding = 30 * MM_TO_PX 

  let stable = false
  let passes = 0

  while (!stable && passes < 30) {
    stable = true
    passes++
    
    const cvRectLoop = cvRoot.value.getBoundingClientRect()

    for (let i = 0; i < allElements.length; i++) {
      const el = allElements[i]
      // Bỏ qua các phần tử ẩn hoặc không có nội dung text
      if (el.offsetHeight === 0 || el.textContent.trim() === '') continue

      const elRect = el.getBoundingClientRect()
      // Loại bỏ ảnh hưởng của scale
      const top = (elRect.top - cvRectLoop.top) / scale
      const height = elRect.height / scale
      const bottomInPage = (top % pageH) + height

      // Bỏ qua nếu nội dung quá lớn
      if (height > (pageH - bottomSafeZone - topPadding)) continue

      // Nếu tràn vùng an toàn, đẩy margin
      if (bottomInPage > (pageH - bottomSafeZone)) {
         const topInPage = top % pageH
         const distToNextPage = pageH - topInPage + topPadding
         
         const currentMt = parseFloat(el.style.marginTop || '0')
         // Margin được gán bằng kích thước CSS chuẩn (chưa scale)
         el.style.setProperty('margin-top', `${currentMt + distToNextPage}px`, 'important')
         
         stable = false
         break 
      }
    }
  }

  // Tính toán lại maxBottom để cập nhật số trang
  await nextTick()
  const finalCvRect = cvRoot.value.getBoundingClientRect()
  let maxB = 0
  
  for (let i = 0; i < allElements.length; i++) {
    const el = allElements[i]
    if (el.offsetHeight === 0 || el.textContent.trim() === '') continue
    const elRect = el.getBoundingClientRect()
    const bottom = (elRect.bottom - finalCvRect.top) / scale
    if (bottom > maxB) maxB = bottom
  }

  // Ép trang mới, dùng dung sai âm (-5px) để tránh tạo trang trống do sai số làm tròn
  pageCount.value = Math.max(1, Math.ceil((maxB - 5) / pageH))
}

watch(() => props.resumeData, requestPagination, { deep: true })
onUnmounted(() => {
  window.removeEventListener('resize', requestPagination)
  if (paginateTimer) clearTimeout(paginateTimer)
})
</script>

<style scoped>
@import url('https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css');
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');

.cv-green-wrapper { 
  width: 210mm; background: #fff; font-family: 'Inter', sans-serif; 
  position: relative; color: #333; overflow: hidden; box-sizing: border-box;
}
.cv-container { display: flex; width: 100%; min-height: 100%; }

/* ================== LEFT COLUMN ================== */
.left-column { width: 40%; padding: 40px 30px; background: #fff; }
.fullname { font-size: 32px; font-weight: 700; color: #43936C; line-height: 1.2; margin-bottom: 5px; }
.job-title { font-size: 16px; font-weight: 600; color: #4A5568; margin-bottom: 30px; }

.avatar-container { width: 170px; height: 170px; margin: 0 auto 30px auto; border-radius: 50%; overflow: hidden; border: 4px solid #fff; box-shadow: 0 4px 10px rgba(0,0,0,0.1); }
.avatar-img { width: 100%; height: 100%; object-fit: cover; }
.avatar-placeholder { width: 100%; height: 100%; background: #f0f0f0; display: flex; align-items: center; justify-content: center; }

.contact-list { margin-bottom: 40px; }
.contact-item { display: flex; align-items: center; gap: 12px; font-size: 12px; font-weight: 500; color: #4A5568; padding: 12px 0; border-bottom: 1px solid #E2E8F0; }
.contact-item:last-child { border-bottom: none; }
.contact-item i { width: 16px; text-align: center; font-size: 14px; }

.left-block { margin-bottom: 25px; position: relative; }
.pill-header { background: #43936C; color: #fff; font-size: 15px; font-weight: 700; text-align: center; padding: 8px 0; border-radius: 20px; margin-bottom: 15px; text-transform: uppercase; }
.text-placeholder { font-size: 12.5px; color: #4A5568; text-align: justify; line-height: 1.6; }

.left-list { list-style: none; padding: 0; margin: 0; padding-left: 10px; }
.left-list li { position: relative; font-size: 12.5px; color: #333; font-weight: 600; margin-bottom: 10px; line-height: 1.5; }
.left-list li::before { content: ''; position: absolute; left: -10px; top: 6px; width: 4px; height: 4px; background: #333; border-radius: 50%; }

/* ================== RIGHT COLUMN ================== */
.right-column { width: 60%; padding: 55px 30px 40px 10px !important; } 

.timeline-container { position: relative; padding-left: 30px; height: 100%; }

.timeline-line { 
    position: absolute; 
    left: 4px; 
    top: 12px !important; 
    bottom: 20px; 
    width: 2px; 
    background: #4A5568; 
    z-index: 0; 
}

.right-block { position: relative; margin-bottom: 35px; z-index: 10; }

.timeline-dot {
    position: absolute;
    left: -30px; 
    top: 6px; 
    width: 10px; 
    height: 10px;
    background: #4A5568; 
    border-radius: 50%; 
    z-index: 1;
}

.right-block-inner { padding-left: 35px; }

.right-title { font-size: 18px; font-weight: 700; color: #2D3748; margin: 0 0 15px 0; }

.green-card { background: #E2EFE6; border-radius: 12px; padding: 16px 20px; position: relative; z-index: 10; }

.text-time { font-size: 13px; font-weight: 600; color: #4A5568; margin-bottom: 4px; }
.text-entity { font-size: 14.5px; font-weight: 700; color: #1A202C; margin-bottom: 4px; }
.text-role { font-size: 13px; font-weight: 600; color: #4A5568; }
.divider-line { width: 30px; height: 2px; background: #4A5568; margin: 8px 0; }

/* ================== HTML CONTENT & PAGINATION ================== */
:deep(.html-content) { font-size: 12.5px; line-height: 1.6; color: #333; text-align: justify; }
:deep(.html-content ul) { list-style-type: none !important; padding-left: 12px !important; margin: 0; }
:deep(.html-content li) { position: relative; margin-bottom: 4px; font-size: 12.5px; line-height: 1.6; }
:deep(.html-content li::before) { content: '•'; position: absolute; left: -12px; color: #333; }
:deep(.html-content b), :deep(.html-content strong) { font-weight: 700 !important; }

/* INTERACTION & BUTTONS */
.section-block { border: 2px solid transparent; transition: 0.2s; border-radius: 8px; }
.section-active { border-color: #43936C !important; box-shadow: 0 0 10px rgba(67, 147, 108, 0.2); z-index: 20; }
.nav-btns { 
  position: absolute; right: 6px; top: 6px; display: flex; gap: 4px; z-index: 100; 
  background: #fff; padding: 4px; border-radius: 6px; box-shadow: 0 2px 8px rgba(0,0,0,0.15); border: 1px solid #E2E8F0;
}
.nav-btn { 
  background: #fff; color: #4A5568; border: 1px solid transparent; width: 26px; height: 26px; border-radius: 4px; cursor: pointer; display: flex; align-items: center; justify-content: center; font-size: 12px; transition: 0.2s;
}
.nav-btn:hover { background: #E2EFE6; color: #43936C; }
.btn-danger { color: #E53E3E !important; }
.btn-danger:hover { background: #FFF5F5 !important; color: #C53030 !important; }
.delete-item-btn:hover { transform: scale(1.15); background: #dc2626; }
.delete-item-btn { position: absolute; right: -5px; top: -5px; width: 18px; height: 18px; background: #ff4d4f; color: white; border: none; border-radius: 50%; cursor: pointer; font-size: 10px; display: flex; align-items: center; justify-content: center; z-index: 50; }

.contact-block {
  border-radius: 8px;
  border: 2px solid transparent;
  padding: 4px 6px;
  margin: -4px -6px;
  cursor: pointer;
  transition: border-color 0.18s ease, box-shadow 0.18s ease;
}
.contact-block.contact-active {
  border: 2px solid #43936C !important;
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

.page-break-indicator { 
  position: absolute; left: 0; width: 100%; 
  z-index: 5000; display: flex; align-items: center; justify-content: center; 
  height: 50px; pointer-events: none; 
}
.page-break-mask {
  position: absolute; left: -20px; width: calc(100% + 40px); height: 100%;
  background: #1e293b; /* Màu nền app để tạo khoảng cách vật lý giữa 2 trang */
}
.page-break-label { 
  position: relative; background: #334155; color: #f8fafc; 
  padding: 6px 16px; border-radius: 6px; font-size: 10px; 
  font-weight: 700; text-transform: uppercase; letter-spacing: 0.1em;
  border: 1px solid #475569; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.3);
  z-index: 1;
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
</style>