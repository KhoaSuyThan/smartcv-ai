<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="bg-white flex w-[210mm] relative box-border overflow-hidden"
    :style="{
      height: `${Math.max(1, pageCount) * 297}mm`,
      fontFamily: '\'Inter\', sans-serif',
      fontSize: '13px',
      lineHeight: '1.55'
    }"
    @click.self="selectedSectionId = null"
  >

    <!-- ===================== CỘT TRÁI (SIDEBAR) ===================== -->
    <aside
      class="flex-shrink-0 flex flex-col relative z-20"
      style="width: 78mm; color: #ffffff;"
      :style="{ backgroundColor: templatePrimaryColor }"
      @click.self="selectedSectionId = null"
    >
      <!-- AVATAR -->
      <div class="paginated-item" style="padding: 10mm 8mm 5mm 8mm;">
        <div style="width: 48mm; height: 62mm; margin: 0 auto; background-color: #4a4e55; position: relative; overflow: hidden; flex-shrink: 0; border-radius: 4px; box-shadow: 0 4px 12px rgba(0,0,0,0.15);">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl"
            style="width: 100%; height: 100%; object-fit: cover; display: block;" />
          <div v-else style="width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; color: #9ca3af;">
            <svg style="width: 80px; height: 80px;" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1"
                d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
          </div>
        </div>
      </div>

      <!-- TÊN & CHỨC DANH -->
      <div class="paginated-item" style="padding: 0 8mm 6mm 8mm;">
        <h1
          style="font-size: 26px; font-weight: 900; text-transform: uppercase; line-height: 1.15; margin-bottom: 4px; word-break: break-word;"
          :style="{ color: isCustomTheme ? '#ffffff' : templateAccentColor }"
          v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : ''"
        ></h1>
        <h2
          style="font-size: 13.5px; font-weight: 700; line-height: 1.3;"
          :style="{ color: isCustomTheme ? 'rgba(255, 255, 255, 0.9)' : templateAccentColor }"
          v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : ''"
        ></h2>
      </div>

      <!-- THÔNG TIN CÁ NHÂN (cố định) -->
      <div
        class="section-block cursor-pointer hover:bg-black/5 transition-colors"
        style="padding: 4mm 8mm;"
        :class="{ 'section-active--sidebar': selectedSectionId === 'contact' }"
        @click.stop="toggleSection('contact')"
      >
        <div class="sidebar-section-header paginated-item" style="margin-bottom: 10px;">
          <h3 class="sidebar-section-title" :style="{ color: isCustomTheme ? '#ffffff' : templateAccentColor }">Thông tin cá nhân</h3>
          <div class="sidebar-divider" :style="{ backgroundColor: isCustomTheme ? 'rgba(255, 255, 255, 0.4)' : templateAccentColor }"></div>
        </div>
        <div style="display: flex; flex-direction: column; gap: 9px; margin-top: 10px;">
          <div
            v-for="(ci, ciIdx) in contactItems"
            :key="ci.key"
            class="contact-row paginated-item relative pr-12 min-h-[20px]"
          >
            <span class="contact-icon" :style="{ color: isCustomTheme ? '#ffffff' : templateAccentColor }">
              <svg viewBox="0 0 20 20" fill="currentColor" style="width:13px;height:13px;" v-html="ci.icon"></svg>
            </span>
            <span class="contact-text" v-html="ci.value"></span>

            <!-- Move Up / Move Down / Delete buttons -->
            <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print">
              <button v-if="ciIdx > 0" @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn" title="Di chuyển lên" style="padding:3px">
                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button v-if="ciIdx < contactItems.length - 1" @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn" title="Di chuyển xuống" style="padding:3px">
                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn--delete" title="Ẩn mục này" style="padding:3px">
                <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- CÁC SECTION DYNAMIC SIDEBAR -->
      <draggable
        v-model="sidebarSectionsWritable"
        item-key="id"
        group="sections"
        class="flex flex-col cursor-move"
        @end="onDragEnd"
        animation="200"
        ghost-class="opacity-30"
        :delay="100"
        :delayOnTouchOnly="true"
        :fallbackTolerance="5"
        filter=".nav-btn, .delete-btn, .contact-item-btns, .html-content, input, .sidebar-html-content"
      >
        <template #item="{ element: section }">
        <div
          v-show="section.isVisible"
          :data-section-id="section.id" class="section-block section-block-sidebar cursor-pointer hover:bg-black/5 transition-colors"
          style="padding: 4mm 8mm;"
          :class="{ 'section-active--sidebar': selectedSectionId === section.id }"
          @click.stop="toggleSection(section.id)"
        >
          <div v-if="selectedSectionId === section.id" class="nav-btns no-print" @click.stop>
            <button @click.stop.prevent="moveSectionUp(section.id, 'left')" class="nav-btn" title="Lên">
              <svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
            </button>
            <button @click.stop.prevent="moveSectionDown(section.id, 'left')" class="nav-btn" title="Xuống">
              <svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
            </button>
            <button @click.stop.prevent="moveSectionHorizontal(section.id, 'right')" class="nav-btn" title="Sang Phải">
              <svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
            </button>
            <button @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn--delete" title="Ẩn mục này">
              <svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
            </button>
          </div>

          <div class="sidebar-section-header paginated-item" style="margin-bottom: 10px;">
            <h3 class="sidebar-section-title" :style="{ color: isCustomTheme ? '#ffffff' : templateAccentColor }">
              <span v-html="section.title"></span>
            </h3>
            <div class="sidebar-divider" :style="{ backgroundColor: isCustomTheme ? 'rgba(255, 255, 255, 0.4)' : templateAccentColor }"></div>
          </div>

          <div>
            <div v-if="['skills','languages','it_skills'].includes(section.id)"
              style="display: flex; flex-direction: column; gap: 6px;">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container paginated-item"
                style="position: relative; color: #ffffff; font-size: 12.5px; font-weight: 700; line-height: 1.45; padding-right: 20px;">
                <span style="font-weight: 800;" v-html="item.name"></span>
                <span v-if="item.level" style="color: #cbd5e1; font-weight: 400; margin-left: 4px;">- <span v-html="item.level"></span></span>
                <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <div v-else-if="['awards','certifications'].includes(section.id)"
              style="display: flex; flex-direction: column; gap: 8px;">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container paginated-item"
                style="position: relative; font-size: 12.5px; line-height: 1.45; padding-right: 20px;">
                <span style="font-weight: 700;" :style="{ color: isCustomTheme ? '#ffffff' : templateAccentColor }"><span v-html="item.year"></span></span>
                <span v-if="item.year" style="color: #ffffff; font-weight: 400;"> - </span>
                <span style="color: #ffffff; font-weight: 700;"><span v-html="item.name"></span></span>
                <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <div v-else style="display: flex; flex-direction: column; gap: 12px;">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container"
                style="position: relative; color: #fff; line-height: 1.5; padding-right: 20px;">
                
                <!-- Thời gian in màu nhấn -->
                <div v-if="item.time || item.year" class="paginated-item" :style="{ color: isCustomTheme ? '#ffffff' : templateAccentColor, fontWeight: '800', fontSize: '12px', marginBottom: '2px' }">
                  <span v-html="item.time || item.year"></span>
                </div>
                
                <!-- Tiêu đề chính (Vai trò / Tên dự án / Tên hoạt động / Ngành học) -->
                <div v-if="item.name || item.title || item.major || item.role" 
                  class="paginated-item" 
                  style="font-weight: 800; font-size: 13px; margin-bottom: 2px;">
                  <span v-html="item.name || item.title || item.major || item.role"></span>
                </div>

                <div v-if="(item.name || item.title || item.major) && item.role" 
                  class="paginated-item" 
                  style="font-weight: 600; font-size: 12.5px; opacity: 0.85; margin-bottom: 2px;">
                  <span v-html="item.role"></span>
                </div>
                
                <!-- Tiêu đề phụ (Công ty / Tổ chức / Trường học) in mờ hơn một chút -->
                <div v-if="item.company || item.school || item.organization" class="paginated-item" style="font-weight: 600; font-size: 12.5px; opacity: 0.85; margin-bottom: 3px;">
                  <span v-html="item.company || item.school || item.organization"></span>
                </div>
                
                <!-- ĐÃ BỔ SUNG: Xếp loại & GPA (Dành riêng cho Học vấn) -->
                <div v-if="item.gradType || item.gpa" class="paginated-item" style="font-weight: 600; font-size: 12px; color: #cbd5e1; margin-bottom: 4px;">
                  <span v-if="item.gradType">Loại: <span v-html="item.gradType"></span></span>
                  <span v-if="item.gradType && item.gpa"> | </span>
                  <span v-if="item.gpa">GPA: <span v-html="item.gpa"></span></span>
                </div>
                
                <!-- Mô tả chi tiết (Được băm nhỏ bởi thuật toán) -->
                <div v-if="item.desc || item.info" class="sidebar-html-content" style="font-weight: 500; font-size: 12.5px;" v-html="formatDesc(item.desc || item.info)"></div>

                <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print" style="top: 0px; right: 0px;">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </div>
      </template>
      </draggable>
    </aside>

    <!-- ===================== CỘT PHẢI (MAIN) ===================== -->
    <main
      class="flex-1 flex flex-col relative z-10"
      style="padding: 14mm 8mm 8mm 8mm; overflow: hidden;"
      @click.self="selectedSectionId = null"
    >
      <div style="height: 5mm;"></div>

      <draggable
        v-model="mainSectionsWritable"
        item-key="id"
        group="sections"
        class="flex flex-col cursor-move"
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
          :data-section-id="section.id" class="section-block section-block-main cursor-pointer hover:bg-black/5 transition-colors"
          style="padding: 4mm 4mm 3.5mm 4mm; margin-bottom: 1px;"
          :class="{ 'section-active--main': selectedSectionId === section.id }"
          @click.stop="toggleSection(section.id)"
        >
          <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
            <button @click.stop.prevent="moveSectionUp(section.id, 'right')" class="nav-btn" title="Lên"><svg width="13" height="13" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
            <button @click.stop.prevent="moveSectionDown(section.id, 'right')" class="nav-btn" title="Xuống"><svg width="13" height="13" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
            <button @click.stop.prevent="moveSectionHorizontal(section.id, 'left')" class="nav-btn" title="Sang Trái"><svg width="13" height="13" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
            <button @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn--delete" title="Ẩn mục này"><svg width="13" height="13" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg></button>
          </div>

          <div class="main-section-header paginated-item" style="margin-bottom: 10px;">
            <h3 class="main-section-title" :style="{ color: templateAccentColor }">
              <span v-html="section.title"></span>
            </h3>
            <div class="main-divider" :style="{ backgroundColor: templateAccentColor }"></div>
          </div>

          <!-- SUMMARY -->
          <div v-if="section.id === 'summary'"
            class="html-content"
            style="font-size: 13.5px; line-height: 1.7; color: #1a1a1a; text-align: justify; font-weight: 500;"
            v-html="formatDesc(resumeData.general.summary || 'Chưa có thông tin.')">
          </div>

          <!-- KINH NGHIỆM LÀM VIỆC & DỰ ÁN TRỌNG ĐIỂM & HOẠT ĐỘNG (Main) -->
          <div v-else-if="section.id.toLowerCase().includes('experience') || section.id.toLowerCase().includes('project') || section.id.toLowerCase().includes('activit')"
            style="display: flex; flex-direction: column; gap: 16px;">
            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container" style="position: relative;">
              <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print" style="top: 0; right: 0;"><svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button>
              
                        <div class="paginated-item" style="display: flex; justify-content: space-between; align-items: baseline; margin-bottom: 2px;">
              <span style="font-weight: 800; font-size: 14px; color: #111; line-height: 1.3; padding-right: 15px;">
                <!-- Experience: lấy role/position | Project: lấy name/title -->
                {{ section.id.toLowerCase().includes('experience') 
                    ? (item.role || item.position) 
                    : (item.name || item.title) }}
              </span>
              <span style="font-size: 12.5px; font-weight: 700; color: #555; white-space: nowrap; flex-shrink: 0;">
                <span v-html="item.time"></span>
              </span>
            </div>

            <div class="paginated-item" 
              v-if="section.id.toLowerCase().includes('experience') 
                    ? (item.company || item.organization) 
                    : item.role"   
              style="font-size: 13px; font-weight: 700; color: #444; margin-bottom: 6px;">
              {{ section.id.toLowerCase().includes('experience') 
                  ? (item.company || item.organization) 
                  : item.role }}   
            </div>
              
              <div class="html-content" style="font-size: 13px; line-height: 1.6; color: #222; text-align: justify;" v-html="formatDesc(item.desc)"></div>
            </div>
          </div>

          <!-- HỌC VẤN -->
          <div v-else-if="section.id.toLowerCase().includes('education')"
            style="display: flex; flex-direction: column; gap: 14px;">
            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container" style="position: relative;">
              <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print" style="top: 0; right: 0;"><svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg></button>
              
              <div class="paginated-item" style="display: flex; justify-content: space-between; align-items: baseline; margin-bottom: 2px;">
                <span style="font-weight: 800; font-size: 14px; color: #111;"><span v-html="item.major || item.school"></span></span>
                <span style="font-size: 12.5px; font-weight: 700; color: #555; white-space: nowrap; padding-left: 6px;"><span v-html="item.year"></span></span>
              </div>
              <div class="paginated-item" style="font-size: 13px; font-weight: 700; color: #444; margin-bottom: 3px;"><span v-html="item.school"></span></div>
              <div class="paginated-item" style="font-size: 13px; color: #555;" v-if="item.gradType || item.gpa">
                <span v-if="item.gradType">Tốt nghiệp loại: <strong><span v-html="item.gradType"></span></strong></span>
                <span v-if="item.gradType && item.gpa"> | </span>
                <span v-if="item.gpa">GPA: <strong><span v-html="item.gpa"></span></strong></span>
              </div>
              <div v-if="item.desc" class="html-content" style="font-size: 13px; line-height: 1.6; color: #222; margin-top: 4px;" v-html="formatDesc(item.desc)"></div>
            </div>
          </div>

          <!-- CÁC MỤC MẶC ĐỊNH KHÁC -->
          <div v-else style="display: flex; flex-direction: column; gap: 12px;">
            <div v-for="(item, itemIndex) in section.items" :key="item._refId"
              class="item-container"
              style="position: relative; font-size: 13px; line-height: 1.6; color: #222;">
              
              <div class="paginated-item" style="display: flex; justify-content: space-between; align-items: baseline;">
                <div style="font-weight: 800; font-size: 14px; color: #111;" class="html-content flex-1" v-html="formatDesc(item.name || item.title || '')"></div>
                
                <span v-if="['skills', 'languages', 'it_skills'].includes(section.id.toLowerCase()) && (item.level || item.info)" style="font-size: 13px; font-weight: 700; color: #555; margin-left: 12px; white-space: nowrap;">
                  <span v-html="item.level || item.info"></span>
                </span>
                
                <span v-else-if="item.year" style="font-size: 13px; font-weight: 700; color: #555; margin-left: 12px; white-space: nowrap;">
                  <span v-html="item.year"></span>
                </span>
              </div>
              
              <div v-if="item.desc" class="html-content" style="margin-top: 3px;" v-html="formatDesc(item.desc)"></div>
              <div v-else-if="!['skills', 'languages', 'it_skills'].includes(section.id.toLowerCase()) && item.info" class="html-content" style="margin-top: 3px;" v-html="formatDesc(item.info)"></div>

              <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print" style="top: -4px; right: -4px;">
                <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>
          </div>
        </div>
      </template>
      </draggable>
    </main>

    <!-- PAGE DIVIDERS -->
    <template v-for="p in (pageCount - 1)" :key="'div-' + p">
      <div
        class="absolute left-0 w-full z-[100] flex flex-col items-center justify-center pointer-events-none no-print"
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
import { computed, ref, onMounted, nextTick, watch, onUnmounted } from 'vue'
import draggable from 'vuedraggable'

const props = defineProps({ resumeData: { type: Object, required: true } })
const emit  = defineEmits(['removeItem', 'moveUp', 'moveDown', 'moveHorizontal'])

const cvRoot            = ref(null)
const pageCount         = ref(1)
const selectedSectionId = ref(null)

const isEmpty = (val) => {
  if (!val) return true
  if (typeof val !== 'string') return false
  return val.replace(/<[^>]*>/g, '').trim() === ''
}

// ─── THUẬT TOÁN LEAF-BLOCK (CHÉM NHỎ ĐẾN TẬN CÙNG VĂN BẢN) ─────────────────────────
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

  // Cứu mã màu của người dùng set trong Editor
  tempDiv.querySelectorAll('li').forEach(li => {
    const child = li.firstElementChild
    if (child && (child.tagName === 'FONT' || child.tagName === 'SPAN')) {
      if (child.color) li.style.color = child.color
      if (child.style?.color) li.style.color = child.style.color
    }
  })

  // Định nghĩa các thẻ bao khối (Block)
  const blockTags = ['P', 'DIV', 'LI', 'H1', 'H2', 'H3', 'H4', 'H5', 'H6']
  
  // Quét toàn bộ HTML, CHỈ gắn class 'paginated-item' cho các Block KHÔNG chứa Block con (Leaf-Blocks)
  tempDiv.querySelectorAll('*').forEach(el => {
    if (blockTags.includes(el.tagName.toUpperCase())) {
      const hasBlockChild = Array.from(el.children).some(child => blockTags.includes(child.tagName.toUpperCase()))
      if (!hasBlockChild) {
        el.classList.add('paginated-item')
      }
    }
  })

  // Tự động bọc các dòng Text đứng bơ vơ bên ngoài
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

const toggleSection = (id) => {
  selectedSectionId.value = selectedSectionId.value === id ? null : id
  requestPagination() 
}

const swapInArray = (arr, i, j) => {
  if (i < 0 || j < 0 || i >= arr.length || j >= arr.length) return
  const tmp = arr[i]; arr.splice(i, 1, arr[j]); arr.splice(j, 1, tmp)
}

const moveSectionUp = (sectionId, col) => {
  const colSecs  = props.resumeData.sections.filter(s => s.column === col)
  const localIdx = colSecs.findIndex(s => s.id === sectionId)
  if (localIdx <= 0) return
  const secs = props.resumeData.sections
  swapInArray(secs, secs.findIndex(s => s.id === colSecs[localIdx].id), secs.findIndex(s => s.id === colSecs[localIdx - 1].id))
  requestPagination()
}

const moveSectionDown = (sectionId, col) => {
  const colSecs  = props.resumeData.sections.filter(s => s.column === col)
  const localIdx = colSecs.findIndex(s => s.id === sectionId)
  if (localIdx < 0 || localIdx >= colSecs.length - 1) return
  const secs = props.resumeData.sections
  swapInArray(secs, secs.findIndex(s => s.id === colSecs[localIdx].id), secs.findIndex(s => s.id === colSecs[localIdx + 1].id))
  requestPagination()
}

const moveSectionHorizontal = (sectionId, direction) => {
  const section = props.resumeData.sections.find(s => s.id === sectionId)
  if (!section) return
  section.column = direction === 'right' ? 'right' : 'left'
  selectedSectionId.value = null
  requestPagination()
}

// ─── CONTACT ITEMS: Danh sách động có thể sắp xếp / ẩn ───
const contactIcons = {
  phone: '<path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z" />',
  email: '<path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z" /><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z" />',
  web: '<path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM4.332 8.027a6.012 6.012 0 011.912-2.706C6.512 5.73 6.974 6 7.5 6A1.5 1.5 0 019 7.5V8a2 2 0 004 0 2 2 0 011.523-1.943A5.977 5.977 0 0116 10c0 .34-.028.675-.083 1H15a2 2 0 00-2 2v2.197A5.973 5.973 0 0110 16v-2a2 2 0 00-2-2 2 2 0 00-2-2 2 2 0 01-2-2 2 2 0 00-1.668-1.973z" clip-rule="evenodd" />',
  address: '<path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd" />'
}

const contactOrder = ref(['phone', 'email', 'web', 'address'])
const hiddenContacts = ref([])

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  switch (key) {
    case 'phone': return g.phone || ''
    case 'email': return g.email || ''
    case 'web':
      return g.github || g.linkedin || g.website || ''
    case 'address': return g.address || ''
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
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k))
  if (idx <= 0) return
  const keyA = visible[idx]
  const keyB = visible[idx - 1]
  const idxA = contactOrder.value.indexOf(keyA)
  const idxB = contactOrder.value.indexOf(keyB)
  const arr = [...contactOrder.value]
  ;[arr[idxA], arr[idxB]] = [arr[idxB], arr[idxA]]
  contactOrder.value = arr
  requestPagination()
}

const moveContactDown = (idx) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k))
  if (idx >= visible.length - 1) return
  const keyA = visible[idx]
  const keyB = visible[idx + 1]
  const idxA = contactOrder.value.indexOf(keyA)
  const idxB = contactOrder.value.indexOf(keyB)
  const arr = [...contactOrder.value]
  ;[arr[idxA], arr[idxB]] = [arr[idxB], arr[idxA]]
  contactOrder.value = arr
  requestPagination()
}

const removeContactItem = (idx) => {
  const visible = contactItems.value
  if (idx >= 0 && idx < visible.length) {
    const key = visible[idx].key
    if (props.resumeData.general[key] !== undefined) {
      props.resumeData.general[key] = ''
    } else if (key === 'web') {
      if (props.resumeData.general.github !== undefined) props.resumeData.general.github = ''
      if (props.resumeData.general.linkedin !== undefined) props.resumeData.general.linkedin = ''
      if (props.resumeData.general.website !== undefined) props.resumeData.general.website = ''
    }
    hiddenContacts.value.push(key)
    requestPagination()
  }
}

// ─── THUẬT TOÁN PHÂN TRANG VÒNG LẶP ĐỘNG ─────────────────────────────────────
const A4_W_MM   = 210
const A4_H_MM   = 297

let paginateTimer = null
const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 60)
}

const doPagination = async () => {
  if (!cvRoot.value) return

  // BỘ LỌC THÔNG MINH: Ngăn chặn lỗi lồng margin, chỉ tính các lớp con ngoài cùng
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
  
  // Thiết lập vùng an toàn 14mm cách đáy
  const bottomSafeZone = 14 * pxPerMm
  const topMargin = 22 * pxPerMm;

  let stable = false
  let passes = 0

  while (!stable && passes < 30) {
    stable = true
    passes++
    
    // Tự động tính toán lại chiều cao mỗi khi có dòng nhảy trang
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

      // Bảo vệ vòng lặp nếu có khối bất thường
      if (height > (pageH - bottomSafeZone - topMargin)) continue

      // Nếu dòng chữ đụng vào vạch đen -> Đẩy xuống!
      if (bottomInPage > (pageH - bottomSafeZone)) {
         const distToNextPage = pageH - topInPage + topMargin
         const currentMt = parseFloat(el.style.marginTop || '0')
         el.style.setProperty('margin-top', `${currentMt + distToNextPage}px`, 'important')
         stable = false
         break
      }
    }
  }

  // Chốt độ dài thực tế của CV
  const finalCvRect = cvRoot.value.getBoundingClientRect()
  let maxBottom = 0
  allElements.forEach(el => {
    const rect = el.getBoundingClientRect()
    const bottom = rect.bottom - finalCvRect.top
    if (bottom > maxBottom) maxBottom = bottom
  })

  pageCount.value = Math.max(1, Math.ceil(maxBottom / pageH))
}

const DEFAULT_VISIBLE_IDS = new Set([
  'job_target', 'summary', 'education', 'experience', 
  'skills', 'it_skills', 'languages', 'awards'        
])

const hasData = (sec) => {
  if (sec.id === 'summary') return !isEmpty(props.resumeData.general?.summary || '')
  return Array.isArray(sec.items) && sec.items.length > 0
}

onMounted(() => {
  if (props.resumeData?.sections) {
    props.resumeData.sections.forEach(sec => {
      if (sec.isVisible === undefined) {
        sec.isVisible = DEFAULT_VISIBLE_IDS.has(sec.id) || hasData(sec)
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

watch(() => props.resumeData, requestPagination, { deep: true })

const sidebarSections = computed(() => props.resumeData.sections.filter(s => s.column === 'left'))
const mainSections = computed(() => props.resumeData.sections.filter(s => s.column === 'right'))

const sidebarSectionsWritable = ref([])
const mainSectionsWritable = ref([])

watch(sidebarSections, (newVal) => {
  sidebarSectionsWritable.value = [...newVal]
}, { immediate: true, deep: true })

watch(mainSections, (newVal) => {
  mainSectionsWritable.value = [...newVal]
}, { immediate: true, deep: true })

const onDragEnd = () => {
  sidebarSectionsWritable.value.forEach(s => {
    const item = props.resumeData.sections.find(x => x.id === s.id)
    if (item) item.column = 'left'
  })
  mainSectionsWritable.value.forEach(s => {
    const item = props.resumeData.sections.find(x => x.id === s.id)
    if (item) item.column = 'right'
  })

  const newOrderIds = [
    ...sidebarSectionsWritable.value.map(s => s.id),
    ...mainSectionsWritable.value.map(s => s.id)
  ]
  
  const newSections = []
  props.resumeData.sections.forEach(s => {
    if (!newOrderIds.includes(s.id)) {
      newSections.push(s)
    }
  })
  
  newOrderIds.forEach(id => {
    const item = props.resumeData.sections.find(s => s.id === id)
    if (item) newSections.push(item)
  })

  props.resumeData.sections.splice(0, props.resumeData.sections.length, ...newSections)
  requestPagination()
}
const sidebarIds = computed(() => sidebarSections.value.map(s => s.id))
const mainIds = computed(() => mainSections.value.map(s => s.id))

const hexToRgb = (hex) => {
  hex = hex.replace(/^#/, '')
  if (hex.length === 3) {
    hex = hex.split('').map(c => c + c).join('')
  }
  const num = parseInt(hex, 16)
  return {
    r: (num >> 16) & 255,
    g: (num >> 8) & 255,
    b: num & 255
  }
}

const isCustomTheme = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  return c && c.toLowerCase() !== '#2b5c8f'
})

const templatePrimaryColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  if (!c || c.toLowerCase() === '#2b5c8f') return '#3a3e43'
  return c
})

const templateAccentColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  if (!c || c.toLowerCase() === '#2b5c8f') return '#dfa234'
  return c
})

const activeBorderColorMain = computed(() => {
  const { r, g, b } = hexToRgb(templateAccentColor.value)
  return `rgba(${r}, ${g}, ${b}, 0.35)`
})
const activeBgColorMain = computed(() => {
  const { r, g, b } = hexToRgb(templateAccentColor.value)
  return `rgba(${r}, ${g}, ${b}, 0.04)`
})
const activeShadowColorMain = computed(() => {
  const { r, g, b } = hexToRgb(templateAccentColor.value)
  return `rgba(${r}, ${g}, ${b}, 0.12)`
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&display=swap');

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

.sidebar-section-header { display: flex; align-items: center; gap: 10px; }
.sidebar-section-title  { font-size: 14px; font-weight: 800; text-transform: uppercase; white-space: nowrap; letter-spacing: 0.03em; }
.sidebar-divider        { flex: 1; height: 1px; opacity: 0.8; }
.contact-row            { display: flex; align-items: flex-start; gap: 10px; }
.contact-icon           { flex-shrink: 0; margin-top: 2px; display: flex; align-items: center; }
.contact-text           { font-size: 12.5px; font-weight: 600; color: #fff; line-height: 1.4; word-break: break-all; }

.main-section-header { display: flex; align-items: center; gap: 10px; }
.main-section-title  { font-size: 15px; font-weight: 900; text-transform: uppercase; white-space: nowrap; letter-spacing: 0.03em; }
.main-divider        { flex: 1; height: 1.5px; }

.item-container { position: relative; }

.delete-btn {
  position: absolute !important;
  width: 20px !important;
  height: 20px !important;
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  background: #ef4444 !important;
  color: white !important;
  border: 2px solid white !important;
  border-radius: 50% !important;
  cursor: pointer !important;
  box-shadow: 0 2px 6px rgba(0,0,0,0.3) !important;
  z-index: 30 !important;
  transition: transform 0.12s ease !important;
}
.delete-btn:hover  { transform: scale(1.15) !important; }
.delete-btn:active { transform: scale(0.9)  !important; }

aside .delete-btn { border-color: #3a3e43 !important; }

/* CSS FIX: Xóa sạch Zoom Scale để đo Pixel chuẩn 100% */
.paginated-item {
  transition: none !important; 
}

.section-block {
  position: relative !important;
  border: 2px solid transparent !important;
  border-radius: 0 !important;
  cursor: pointer !important;
  transition: box-shadow 0.2s ease, background 0.2s ease, border-color 0.2s ease;
}

.section-block:hover { background: transparent !important; }

.section-active--sidebar {
  border: 2px solid rgba(255,255,255,0.25) !important;
  border-radius: 6px !important;
  box-shadow: 0 6px 20px rgba(0,0,0,0.25), 0 1px 4px rgba(0,0,0,0.1) !important;
  background: rgba(255,255,255,0.05) !important;
  z-index: 10 !important;
}

.section-active--main {
  border: 2px solid v-bind(activeBorderColorMain) !important;
  border-radius: 6px !important;
  box-shadow: 0 4px 18px v-bind(activeShadowColorMain), 0 1px 4px rgba(0,0,0,0.06) !important;
  background: v-bind(activeBgColorMain) !important;
  z-index: 10 !important;
}

.nav-btns {
  position: absolute !important;
  right: 4px !important;
  top: 4px !important;
  display: flex !important;
  flex-direction: row !important;
  gap: 4px !important;
  z-index: 9999 !important;
}

.nav-btn {
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  padding: 4px !important;
  background: #dfa234 !important;
  color: white !important;
  border: none !important;
  border-radius: 4px !important;
  cursor: pointer !important;
  box-shadow: 0 2px 5px rgba(0,0,0,0.25) !important;
  transition: background 0.12s, transform 0.1s !important;
}
.nav-btn:hover  { background: #c78d2b !important; }
.nav-btn:active { transform: scale(0.95) !important; }

.contact-item-btns {
  position: absolute !important;
  right: -4px !important;
  top: 50% !important;
  transform: translateY(-50%) !important;
  display: flex !important;
  flex-direction: row !important;
  gap: 3px !important;
  z-index: 9999 !important;
}

.nav-btn--delete {
  background: #ef4444 !important;
  box-shadow: 0 2px 5px rgba(239,68,68,0.4) !important;
}
.nav-btn--delete:hover {
  background: #dc2626 !important;
}

/* NGĂN CHẶN SỤP LỀ CSS - GIỮ PHÉP ĐO JAVASCRIPT CHUẨN XÁC */
:deep(.html-content)                                   { margin: 0 !important; padding: 0 !important; }
:deep(.html-content p)                                 { margin-bottom: 4px !important; padding: 0 !important; }
:deep(.html-content ul)                                { list-style-type: disc !important; padding-left: 1.2rem !important; margin: 0.1rem 0 !important; }
:deep(.html-content ol)                                { list-style-type: decimal !important; padding-left: 1.2rem !important; margin: 0.1rem 0 !important; }
:deep(.html-content li)                                { margin-bottom: 0.15rem !important; }
:deep(.html-content b), :deep(.html-content strong)    { font-weight: 700 !important; color: #111 !important; }
:deep(.html-content i), :deep(.html-content em)        { font-style: italic !important; }
:deep(.html-content u)                                 { text-decoration: underline !important; }

:deep(.html-content li:has(> font[size="1"])) { font-size: 10px !important; }
:deep(.html-content li:has(> font[size="2"])) { font-size: 13px !important; }
:deep(.html-content li:has(> font[size="3"])) { font-size: 16px !important; }

/* BẢO TỒN CHỮ TRẮNG Ở CỘT TRÁI ĐÈ LÊN MÀU CỦA EDITOR */
:deep(.sidebar-html-content p),
:deep(.sidebar-html-content span),
:deep(.sidebar-html-content div)  { color: white !important; margin-bottom: 0.1rem !important; }
:deep(.sidebar-html-content b),
:deep(.sidebar-html-content strong) { color: white !important; font-weight: 700 !important; }
:deep(.sidebar-html-content ul)   { color: white !important; list-style-type: disc !important; padding-left: 1.1rem !important; margin: 0.1rem 0 !important; }
:deep(.sidebar-html-content li)   { color: white !important; margin-bottom: 0.1rem !important; }

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
    outline: none !important;
    margin-bottom: 12px !important;
  }
  aside .section-block  { padding-left: 8mm !important; padding-right: 8mm !important; }
  main  .section-block  { padding-left: 4mm !important; padding-right: 4mm !important; }
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
  margin-bottom: 12px !important;
}
:global(.is-exporting-pdf aside .section-block) { padding-left: 8mm !important; padding-right: 8mm !important; }
:global(.is-exporting-pdf main  .section-block) { padding-left: 4mm !important; padding-right: 4mm !important; }
</style>