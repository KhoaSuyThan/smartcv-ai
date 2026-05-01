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
      style="width: 78mm; background-color: #3a3e43; color: #ffffff;"
      @click.self="selectedSectionId = null"
    >
      <!-- AVATAR -->
      <div class="paginated-item" style="padding: 10mm 8mm 5mm 8mm;">
        <div style="width: 62mm; height: 80mm; background-color: #4a4e55; position: relative; overflow: hidden; flex-shrink: 0;">
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
          :style="{ color: resumeData.theme.primaryColor || '#dfa234' }"
          v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'VŨ TÙNG DƯƠNG'"
        ></h1>
        <h2
          style="font-size: 13.5px; font-weight: 700; line-height: 1.3;"
          :style="{ color: resumeData.theme.primaryColor || '#dfa234' }"
          v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'Senior Digital Marketing'"
        ></h2>
      </div>

      <!-- THÔNG TIN CÁ NHÂN (cố định) -->
      <div class="paginated-item sidebar-section" style="padding: 4mm 8mm;">
        <div class="sidebar-section-header" style="margin-bottom: 10px;">
          <h3 class="sidebar-section-title" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">Thông tin cá nhân</h3>
          <div class="sidebar-divider" :style="{ backgroundColor: resumeData.theme.primaryColor || '#dfa234' }"></div>
        </div>
        <div style="display: flex; flex-direction: column; gap: 9px; margin-top: 10px;">
          <div class="contact-row">
            <span class="contact-icon" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">
              <svg viewBox="0 0 24 24" fill="currentColor" style="width:13px;height:13px;">
                <path d="M6.62 10.79a15.091 15.091 0 006.59 6.59l2.2-2.2a1 1 0 011.11-.21c1.12.37 2.33.57 3.58.57a1 1 0 011 1v3.5a1 1 0 01-1 1A16 16 0 013 4a1 1 0 011-1h3.5a1 1 0 011 1c0 1.25.2 2.46.57 3.58a1 1 0 01-.21 1.11l-2.24 2.1z"/>
              </svg>
            </span>
            <span class="contact-text" v-html="resumeData.general.phone || '0123.456.789'"></span>
          </div>
          <div class="contact-row">
            <span class="contact-icon" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">
              <svg viewBox="0 0 24 24" fill="currentColor" style="width:13px;height:13px;">
                <path d="M22 6c0-1.1-.9-2-2-2H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6zm-2 0l-8 5-8-5h16zm0 12H4V8l8 5 8-5v10z"/>
              </svg>
            </span>
            <span class="contact-text" v-html="resumeData.general.email || 'email@example.com'"></span>
          </div>
          <div v-if="!isEmpty(resumeData.general.website)" class="contact-row">
            <span class="contact-icon" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">
              <svg viewBox="0 0 24 24" fill="currentColor" style="width:13px;height:13px;">
                <path d="M3.9 12c0-1.71 1.39-3.1 3.1-3.1h4V7H7c-2.76 0-5 2.24-5 5s2.24 5 5 5h4v-1.9H7c-1.71 0-3.1-1.39-3.1-3.1zM8 13h8v-2H8v2zm9-6h-4v1.9h4c1.71 0 3.1 1.39 3.1 3.1s-1.39 3.1-3.1 3.1h-4V17h4c2.76 0 5-2.24 5-5s-2.24-5-5-5z"/>
              </svg>
            </span>
            <span class="contact-text" v-html="resumeData.general.website"></span>
          </div>
          <div class="contact-row">
            <span class="contact-icon" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">
              <svg viewBox="0 0 24 24" fill="currentColor" style="width:13px;height:13px;">
                <path d="M12 2C8.13 2 5 5.13 5 9c0 4.17 4.42 9.92 6.24 12.11a1 1 0 001.52 0C14.58 18.92 19 13.17 19 9c0-3.87-3.13-7-7-7zm0 9.5a2.5 2.5 0 010-5 2.5 2.5 0 010 5z"/>
              </svg>
            </span>
            <span class="contact-text" v-html="resumeData.general.address || 'TP. Hồ Chí Minh'"></span>
          </div>
        </div>
      </div>

      <!-- CÁC SECTION DYNAMIC SIDEBAR -->
      <template v-for="section in sidebarSections" :key="section.id">
        <div
          v-show="section.isVisible"
          class="section-block section-block-sidebar paginated-item"
          style="padding: 4mm 8mm;"
          :class="{ 'section-active--sidebar': selectedSectionId === section.id }"
          @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
        >
          <!-- ✅ v-if: chỉ render khi click -->
          <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
            <button @click.stop.prevent="moveSectionUp(section.id, 'left')" class="nav-btn" title="Lên">
              <svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/>
              </svg>
            </button>
            <button @click.stop.prevent="moveSectionDown(section.id, 'left')" class="nav-btn" title="Xuống">
              <svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/>
              </svg>
            </button>
            <button @click.stop.prevent="moveSectionHorizontal(section.id, 'right')" class="nav-btn" title="Sang Phải">
              <svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/>
              </svg>
            </button>
          </div>

          <div class="sidebar-section-header" style="margin-bottom: 10px;">
            <h3 class="sidebar-section-title" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">
              {{ section.title }}
            </h3>
            <div class="sidebar-divider" :style="{ backgroundColor: resumeData.theme.primaryColor || '#dfa234' }"></div>
          </div>

          <div>
            <!-- Skills / Languages / IT -->
            <div v-if="['skills','languages','it_skills'].includes(section.id)"
              style="display: flex; flex-direction: column; gap: 6px;">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container"
                style="position: relative; color: #ffffff; font-size: 12.5px; font-weight: 700; line-height: 1.45; padding-right: 20px;">
                <span style="font-weight: 800;" v-html="item.name"></span>
                <span v-if="item.level" style="color: #cbd5e1; font-weight: 400; margin-left: 4px;">- {{ item.level }}</span>
                <button v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                  </svg>
                </button>
              </div>
            </div>

            <!-- Awards / Certifications -->
            <div v-else-if="['awards','certifications'].includes(section.id)"
              style="display: flex; flex-direction: column; gap: 8px;">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container"
                style="position: relative; font-size: 12.5px; line-height: 1.45; padding-right: 20px;">
                <span style="font-weight: 700;" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">{{ item.year }}</span>
                <span v-if="item.year" style="color: #ffffff; font-weight: 400;"> - </span>
                <span style="color: #ffffff; font-weight: 700;">{{ item.name }}</span>
                <button v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                  </svg>
                </button>
              </div>
            </div>

            <!-- References -->
            <div v-else-if="section.id === 'references'"
              style="display: flex; flex-direction: column; gap: 12px;">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container"
                style="position: relative; color: #ffffff; font-size: 12.5px; line-height: 1.65; padding-right: 20px;">
                <div class="sidebar-html-content" v-html="formatDesc(item.info)"></div>
                <button v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                  </svg>
                </button>
              </div>
            </div>

            <!-- Hobbies -->
            <div v-else-if="section.id === 'hobbies'"
              style="display: flex; flex-direction: column; gap: 6px;">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container"
                style="position: relative; color: #ffffff; font-size: 12.5px; font-weight: 700; line-height: 1.45; padding-right: 20px;">
                <span v-html="item.name"></span>
                <button v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                  </svg>
                </button>
              </div>
            </div>

            <!-- Default sidebar -->
            <div v-else style="display: flex; flex-direction: column; gap: 7px;">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container"
                style="position: relative; color: #fff; font-size: 12.5px; font-weight: 600; line-height: 1.5; padding-right: 20px;">
                <span class="sidebar-html-content" v-html="formatDesc(item.desc || item.name || item.info)"></span>
                <button v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                  </svg>
                </button>
              </div>
            </div>
          </div>
        </div>
      </template>
    </aside>

    <!-- ===================== CỘT PHẢI (MAIN) ===================== -->
    <main
  class="flex-1 flex flex-col relative z-10"
  style="padding: 14mm 8mm 8mm 8mm; overflow: hidden;"
  @click.self="selectedSectionId = null"
    >
  <!-- Spacer đẩy nội dung xuống -->
  <div style="height: 5mm;" class="no-print"></div>
      <template v-for="section in mainSections" :key="section.id">
        <div
          v-show="section.isVisible"
          class="section-block section-block-main paginated-item"
          style="padding: 4mm 4mm 3.5mm 4mm; margin-bottom: 1px;"
          :class="{ 'section-active--main': selectedSectionId === section.id }"
          @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
        >
          <!-- ✅ v-if: chỉ render khi click -->
          <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
            <button @click.stop.prevent="moveSectionUp(section.id, 'right')" class="nav-btn" title="Lên">
              <svg width="13" height="13" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/>
              </svg>
            </button>
            <button @click.stop.prevent="moveSectionDown(section.id, 'right')" class="nav-btn" title="Xuống">
              <svg width="13" height="13" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/>
              </svg>
            </button>
            <button @click.stop.prevent="moveSectionHorizontal(section.id, 'left')" class="nav-btn" title="Sang Trái">
              <svg width="13" height="13" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/>
              </svg>
            </button>
          </div>

          <!-- Tiêu đề section main -->
          <div class="main-section-header" style="margin-bottom: 10px;">
            <h3 class="main-section-title" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">
              {{ section.title }}
            </h3>
            <div class="main-divider" :style="{ backgroundColor: resumeData.theme.primaryColor || '#dfa234' }"></div>
          </div>

          <!-- Summary -->
          <div v-if="section.id === 'summary'"
            class="html-content"
            style="font-size: 13.5px; line-height: 1.7; color: #1a1a1a; text-align: justify; font-weight: 500;"
            v-html="resumeData.general.summary || 'Chưa có thông tin.'">
          </div>

          <!-- Experience / Project -->
          <div v-else-if="['experience','project'].includes(section.id)"
            style="display: flex; flex-direction: column; gap: 16px;">
            <div v-for="(item, itemIndex) in section.items" :key="item._refId"
              class="item-container" style="position: relative;">
              <button v-if="selectedSectionId === section.id"
                @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                class="delete-btn no-print" style="top: 0; right: 0;">
                <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                </svg>
              </button>
              <div style="display: flex; justify-content: space-between; align-items: baseline; margin-bottom: 2px;">
                <span style="font-weight: 800; font-size: 14px; color: #111; line-height: 1.3;">
                  {{ section.id === 'experience' ? item.role : (item.role || item.name) }}
                </span>
                <span style="font-size: 12.5px; font-weight: 700; color: #555; white-space: nowrap; padding-left: 6px;">{{ item.time }}</span>
              </div>
              <div style="font-size: 13px; font-weight: 700; color: #444; margin-bottom: 6px;">
                {{ section.id === 'experience' ? item.company : (item.company || '') }}
              </div>
              <div class="html-content"
                style="font-size: 13px; line-height: 1.6; color: #222; text-align: justify;"
                v-html="formatDesc(item.desc)">
              </div>
            </div>
          </div>

          <!-- Activities -->
          <div v-else-if="section.id === 'activities'"
            style="display: flex; flex-direction: column; gap: 16px;">
            <div v-for="(item, itemIndex) in section.items" :key="item._refId"
              class="item-container" style="position: relative;">
              <button v-if="selectedSectionId === section.id"
                @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                class="delete-btn no-print" style="top: 0; right: 0;">
                <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                </svg>
              </button>
              <div style="display: flex; justify-content: space-between; align-items: baseline; margin-bottom: 2px;">
                <span style="font-weight: 800; font-size: 14px; color: #111; line-height: 1.3;">
                  {{ item.role || item.name }}
                </span>
                <span style="font-size: 12.5px; font-weight: 700; color: #555; white-space: nowrap; padding-left: 6px;">{{ item.time }}</span>
              </div>
              <div v-if="item.company || item.club || item.organization"
                style="font-size: 13px; font-weight: 700; color: #444; margin-bottom: 6px;">
                {{ item.company || item.club || item.organization }}
              </div>
              <div class="html-content"
                style="font-size: 13px; line-height: 1.6; color: #222; text-align: justify;"
                v-html="formatDesc(item.desc)">
              </div>
            </div>
          </div>

          <!-- Education -->
          <div v-else-if="section.id === 'education'"
            style="display: flex; flex-direction: column; gap: 14px;">
            <div v-for="(item, itemIndex) in section.items" :key="item._refId"
              class="item-container" style="position: relative;">
              <button v-if="selectedSectionId === section.id"
                @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                class="delete-btn no-print" style="top: 0; right: 0;">
                <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                </svg>
              </button>
              <div style="display: flex; justify-content: space-between; align-items: baseline; margin-bottom: 2px;">
                <span style="font-weight: 800; font-size: 14px; color: #111;">{{ item.major || item.school }}</span>
                <span style="font-size: 12.5px; font-weight: 700; color: #555; white-space: nowrap; padding-left: 6px;">{{ item.year }}</span>
              </div>
              <div style="font-size: 13px; font-weight: 700; color: #444; margin-bottom: 3px;">{{ item.school }}</div>
              <div style="font-size: 13px; color: #555;">
                <span v-if="item.gradType">Tốt nghiệp loại: <strong>{{ item.gradType }}</strong></span>
                <span v-if="item.gradType && item.gpa"> | </span>
                <span v-if="item.gpa">GPA: <strong>{{ item.gpa }}</strong></span>
              </div>
              <div v-if="item.desc" class="html-content"
                style="font-size: 13px; line-height: 1.6; color: #222; margin-top: 4px;"
                v-html="formatDesc(item.desc)">
              </div>
            </div>
          </div>

          <!-- Default main -->
          <div v-else style="display: flex; flex-direction: column; gap: 12px;">
            <div v-for="(item, itemIndex) in section.items" :key="item._refId"
              class="item-container html-content"
              style="position: relative; font-size: 13px; line-height: 1.6; color: #222;">
              <div v-html="formatDesc(item.desc || item.name || item.info)"></div>
              <button v-if="selectedSectionId === section.id"
                @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                class="delete-btn no-print" style="top: 0; right: 0;">
                <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                </svg>
              </button>
            </div>
          </div>
        </div>
      </template>
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

const props = defineProps({ resumeData: { type: Object, required: true } })
const emit  = defineEmits(['removeItem'])

const cvRoot            = ref(null)
const pageCount         = ref(1)
const selectedSectionId = ref(null)

// ─── Helpers ───────────────────────────────────────────────────────────────
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
            if (child.color)        li.style.color = child.color
            if (child.style?.color) li.style.color = child.style.color
          }
        })
        return div.innerHTML
      } catch { return text }
    }
    return text
  }
  return text
    .split('\n')
    .map(l => l.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').trim())
    .filter(Boolean)
    .join('<br/>')
}

// ─── Section move (splice = reactive-safe) ─────────────────────────────────
const swapInArray = (arr, i, j) => {
  if (i < 0 || j < 0 || i >= arr.length || j >= arr.length) return
  const tmp = arr[i]; arr.splice(i, 1, arr[j]); arr.splice(j, 1, tmp)
}

const moveSectionUp = (sectionId, col) => {
  const colSecs  = props.resumeData.sections.filter(s => s.column === col)
  const localIdx = colSecs.findIndex(s => s.id === sectionId)
  if (localIdx <= 0) return
  const secs = props.resumeData.sections
  swapInArray(secs,
    secs.findIndex(s => s.id === colSecs[localIdx].id),
    secs.findIndex(s => s.id === colSecs[localIdx - 1].id)
  )
  requestPagination()
}

const moveSectionDown = (sectionId, col) => {
  const colSecs  = props.resumeData.sections.filter(s => s.column === col)
  const localIdx = colSecs.findIndex(s => s.id === sectionId)
  if (localIdx < 0 || localIdx >= colSecs.length - 1) return
  const secs = props.resumeData.sections
  swapInArray(secs,
    secs.findIndex(s => s.id === colSecs[localIdx].id),
    secs.findIndex(s => s.id === colSecs[localIdx + 1].id)
  )
  requestPagination()
}

const moveSectionHorizontal = (sectionId, direction) => {
  const section = props.resumeData.sections.find(s => s.id === sectionId)
  if (!section) return
  section.column = direction === 'right' ? 'right' : 'left'
  selectedSectionId.value = null
  requestPagination()
}

// ─── Pagination ────────────────────────────────────────────────────────────
const A4_W_MM   = 210
const A4_H_MM   = 297
const MARGIN_MM = 15

let paginateTimer = null
const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 60)
}

const doPagination = async () => {
  if (!cvRoot.value) return

  cvRoot.value.querySelectorAll('.paginated-item').forEach(el => { el.style.marginTop = '' })
  await nextTick()

  const pxPerMm   = cvRoot.value.offsetWidth / A4_W_MM
  const pageH     = A4_H_MM * pxPerMm
  const marginPx  = MARGIN_MM * pxPerMm
  const safeLine  = pageH - marginPx
  const maxUsable = safeLine - marginPx

  const relTop = (el) => {
    let off = 0, cur = el
    while (cur && cur !== cvRoot.value) { off += cur.offsetTop; cur = cur.offsetParent }
    return off
  }

  const processColumn = (selector) => {
    const col = cvRoot.value.querySelector(selector)
    if (!col) return

    const allItems = Array.from(col.querySelectorAll('.paginated-item'))
    const colItems = allItems.filter(el => {
      if (el.offsetHeight === 0) return false
      let parent = el.parentElement
      while (parent && parent !== col) {
        if (parent.classList.contains('paginated-item')) return false
        parent = parent.parentElement
      }
      return true
    })

    for (let pass = 0; pass < 80; pass++) {
      let stable = true
      for (const item of colItems) {
        const top     = relTop(item)
        const bottom  = top + item.offsetHeight
        const pageIdx = Math.floor(top / pageH)
        const curSafe = pageIdx * pageH + safeLine

        if (top < curSafe && bottom > curSafe && item.offsetHeight <= maxUsable) {
          const extra = ((pageIdx + 1) * pageH + marginPx) - top
          item.style.marginTop = (parseFloat(item.style.marginTop || '0') + extra) + 'px'
          stable = false
          break
        }
      }
      if (stable) break
    }
  }

  processColumn('aside')
  processColumn('main')

  let maxBottom = 0
  cvRoot.value.querySelectorAll('.paginated-item').forEach(el => {
    if (el.offsetHeight === 0) return
    const b = relTop(el) + el.offsetHeight
    if (b > maxBottom) maxBottom = b
  })

  const needed = Math.max(1, Math.ceil((maxBottom + marginPx) / pageH))
  if (pageCount.value !== needed) pageCount.value = needed
}

// ─── Visibility init ───────────────────────────────────────────────────────
// Danh sách các section luôn hiển thị mặc định (kể cả khi chưa có items)
const DEFAULT_VISIBLE_IDS = new Set([
  'job_target', 'summary', 'education', 'experience', // cột phải
  'skills', 'it_skills', 'languages', 'awards'        // cột trái
])

const hasData = (sec) => {
  if (sec.id === 'summary') return !isEmpty(props.resumeData.general?.summary || '')
  return Array.isArray(sec.items) && sec.items.length > 0
}

onMounted(() => {
  if (props.resumeData?.sections) {
    props.resumeData.sections.forEach(sec => {
      // Hiển thị mặc định nếu thuộc danh sách ưu tiên HOẶC đã có dữ liệu thực
      sec.isVisible = DEFAULT_VISIBLE_IDS.has(sec.id) || hasData(sec)
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

// ─── Computed ──────────────────────────────────────────────────────────────
const sidebarSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'left')
)
const mainSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'right')
)
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

.section-block {
  position: relative !important;
  border: 2px solid transparent !important;
  border-radius: 0 !important;
  cursor: pointer !important;
  transition:
    transform     0.2s cubic-bezier(0.34, 1.56, 0.64, 1),
    box-shadow    0.2s ease,
    border-color  0.15s ease,
    border-radius 0.15s ease,
    background    0.15s ease !important;
}

.section-block:hover { background: transparent !important; }

.section-active--sidebar {
  border: 2px solid rgba(255,255,255,0.25) !important;
  border-radius: 6px !important;
  transform: scale(1.013) !important;
  box-shadow: 0 6px 20px rgba(0,0,0,0.25), 0 1px 4px rgba(0,0,0,0.1) !important;
  background: rgba(255,255,255,0.05) !important;
  z-index: 10 !important;
}

.section-active--main {
  border: 2px solid rgba(223,162,52,0.35) !important;
  border-radius: 6px !important;
  transform: scale(1.013) !important;
  box-shadow: 0 4px 18px rgba(223,162,52,0.12), 0 1px 4px rgba(223,162,52,0.06) !important;
  background: rgba(223,162,52,0.04) !important;
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

:deep(.html-content)                                   { margin: 0 !important; padding: 0 !important; }
:deep(.html-content p)                                 { margin-bottom: 0.15rem !important; }
:deep(.html-content ul)                                { list-style-type: disc !important; padding-left: 1.2rem !important; margin: 0.1rem 0 !important; }
:deep(.html-content ol)                                { list-style-type: decimal !important; padding-left: 1.2rem !important; margin: 0.1rem 0 !important; }
:deep(.html-content li)                                { margin-bottom: 0.15rem !important; }
:deep(.html-content b), :deep(.html-content strong)    { font-weight: 700 !important; color: #111 !important; }
:deep(.html-content i), :deep(.html-content em)        { font-style: italic !important; }
:deep(.html-content u)                                 { text-decoration: underline !important; }

:deep(.html-content li:has(> font[size="1"])) { font-size: 10px !important; }
:deep(.html-content li:has(> font[size="2"])) { font-size: 13px !important; }
:deep(.html-content li:has(> font[size="3"])) { font-size: 16px !important; }

:deep(.sidebar-html-content p),
:deep(.sidebar-html-content span),
:deep(.sidebar-html-content div)  { color: inherit !important; margin-bottom: 0.1rem !important; }
:deep(.sidebar-html-content b),
:deep(.sidebar-html-content strong) { color: white !important; font-weight: 700 !important; }
:deep(.sidebar-html-content ul)   { list-style-type: disc !important; padding-left: 1.1rem !important; margin: 0.1rem 0 !important; }
:deep(.sidebar-html-content li)   { margin-bottom: 0.1rem !important; }

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