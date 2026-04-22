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
      <!-- AVATAR - hình chữ nhật dọc, không bo góc, full width sidebar -->
      <div class="paginated-item" style="padding: 10mm 8mm 5mm 8mm;">
        <div
          style="width: 62mm; height: 80mm; background-color: #4a4e55; position: relative; overflow: hidden; flex-shrink: 0;"
        >
          <img
            v-if="resumeData.general.avatarUrl"
            :src="resumeData.general.avatarUrl"
            style="width: 100%; height: 100%; object-fit: cover; display: block;"
          />
          <div
            v-else
            style="width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; color: #9ca3af;"
          >
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

      <!-- THÔNG TIN CÁ NHÂN (cố định, luôn hiển thị nếu có data) -->
      <div class="paginated-item sidebar-section" style="padding: 4mm 8mm;">
        <div class="sidebar-section-header" :style="{ '--primary': resumeData.theme.primaryColor || '#dfa234' }">
          <h3 class="sidebar-section-title" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">Thông tin cá nhân</h3>
          <div class="sidebar-divider" :style="{ backgroundColor: resumeData.theme.primaryColor || '#dfa234' }"></div>
        </div>
        <div style="display: flex; flex-direction: column; gap: 9px; margin-top: 10px;">
          <div v-if="!isEmpty(resumeData.general.phone)" class="contact-row">
            <span class="contact-icon" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">
              <svg viewBox="0 0 24 24" fill="currentColor" style="width:13px;height:13px;">
                <path d="M6.62 10.79a15.091 15.091 0 006.59 6.59l2.2-2.2a1 1 0 011.11-.21c1.12.37 2.33.57 3.58.57a1 1 0 011 1v3.5a1 1 0 01-1 1A16 16 0 013 4a1 1 0 011-1h3.5a1 1 0 011 1c0 1.25.2 2.46.57 3.58a1 1 0 01-.21 1.11l-2.24 2.1z"/>
              </svg>
            </span>
            <span class="contact-text" v-html="resumeData.general.phone"></span>
          </div>
          <div v-if="!isEmpty(resumeData.general.email)" class="contact-row">
            <span class="contact-icon" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">
              <svg viewBox="0 0 24 24" fill="currentColor" style="width:13px;height:13px;">
                <path d="M22 6c0-1.1-.9-2-2-2H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6zm-2 0l-8 5-8-5h16zm0 12H4V8l8 5 8-5v10z"/>
              </svg>
            </span>
            <span class="contact-text" v-html="resumeData.general.email"></span>
          </div>
          <div v-if="!isEmpty(resumeData.general.website)" class="contact-row">
            <span class="contact-icon" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">
              <svg viewBox="0 0 24 24" fill="currentColor" style="width:13px;height:13px;">
                <path d="M3.9 12c0-1.71 1.39-3.1 3.1-3.1h4V7H7c-2.76 0-5 2.24-5 5s2.24 5 5 5h4v-1.9H7c-1.71 0-3.1-1.39-3.1-3.1zM8 13h8v-2H8v2zm9-6h-4v1.9h4c1.71 0 3.1 1.39 3.1 3.1s-1.39 3.1-3.1 3.1h-4V17h4c2.76 0 5-2.24 5-5s-2.24-5-5-5z"/>
              </svg>
            </span>
            <span class="contact-text" v-html="resumeData.general.website"></span>
          </div>
          <div v-if="!isEmpty(resumeData.general.address)" class="contact-row">
            <span class="contact-icon" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">
              <svg viewBox="0 0 24 24" fill="currentColor" style="width:13px;height:13px;">
                <path d="M12 2C8.13 2 5 5.13 5 9c0 4.17 4.42 9.92 6.24 12.11a1 1 0 001.52 0C14.58 18.92 19 13.17 19 9c0-3.87-3.13-7-7-7zm0 9.5a2.5 2.5 0 010-5 2.5 2.5 0 010 5z"/>
              </svg>
            </span>
            <span class="contact-text" v-html="resumeData.general.address"></span>
          </div>
        </div>
      </div>

      <!-- CÁC SECTION DYNAMIC CỦA SIDEBAR -->
      <template v-for="section in sidebarSections" :key="section.id">
        <div
          v-show="section.isVisible"
          class="section-block section-block-sidebar paginated-item"
          style="padding: 4mm 8mm;"
          :class="{ 'section-selected': selectedSectionId === section.id }"
          :style="selectedSectionId === section.id ? { '--sel-color': resumeData.theme.primaryColor || '#dfa234' } : {}"
          @mouseenter="showNav(section.id)"
          @mouseleave="hideNav()"
          @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
        >
          <!-- Nav buttons -->
          <div
            v-show="hoveredSectionId === section.id || selectedSectionId === section.id"
            class="nav-btns no-print"
            @mouseenter="showNav(section.id)"
            @mouseleave="hideNav()"
          >
            <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Lên">
              <svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/>
              </svg>
            </button>
            <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Xuống">
              <svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/>
              </svg>
            </button>
            <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải">
              <svg width="11" height="11" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/>
              </svg>
            </button>
          </div>

          <!-- Tiêu đề section -->
          <div class="sidebar-section-header" style="margin-bottom: 10px;">
            <h3 class="sidebar-section-title" :style="{ color: resumeData.theme.primaryColor || '#dfa234' }">
              {{ section.title }}
            </h3>
            <div class="sidebar-divider" :style="{ backgroundColor: resumeData.theme.primaryColor || '#dfa234' }"></div>
          </div>

          <!-- Nội dung section -->
          <div>
            <!-- Kỹ năng, Ngôn ngữ, IT Skills -->
            <div
              v-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'"
              style="display: flex; flex-direction: column; gap: 6px;"
            >
              <div
                v-for="(item, itemIndex) in section.items"
                :key="item._refId"
                class="item-container paginated-item"
                style="position: relative; color: #ffffff; font-size: 12.5px; font-weight: 700; line-height: 1.45; padding-right: 20px;"
              >
                <span style="font-weight: 800;" v-html="item.name"></span>
                <span v-if="item.level" style="color: #cbd5e1; font-weight: 400; margin-left: 4px;">- {{ item.level }}</span>
                <button
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print"
                >
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                  </svg>
                </button>
              </div>
            </div>

            <!-- Giải thưởng / Danh hiệu & Chứng chỉ: năm (màu) - tên (trắng) -->
            <div
              v-else-if="section.id === 'awards' || section.id === 'certifications'"
              style="display: flex; flex-direction: column; gap: 8px;"
            >
              <div
                v-for="(item, itemIndex) in section.items"
                :key="item._refId"
                class="item-container paginated-item"
                style="position: relative; font-size: 12.5px; line-height: 1.45; padding-right: 20px;"
              >
                <span
                  style="font-weight: 700;"
                  :style="{ color: resumeData.theme.primaryColor || '#dfa234' }"
                >{{ item.year }}</span>
                <span v-if="item.year" style="color: #ffffff; font-weight: 400;"> - </span>
                <span style="color: #ffffff; font-weight: 700;">{{ item.name }}</span>
                <button
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print"
                >
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                  </svg>
                </button>
              </div>
            </div>

            <!-- Người giới thiệu -->
            <div
              v-else-if="section.id === 'references'"
              style="display: flex; flex-direction: column; gap: 12px;"
            >
              <div
                v-for="(item, itemIndex) in section.items"
                :key="item._refId"
                class="item-container paginated-item"
                style="position: relative; color: #ffffff; font-size: 12.5px; line-height: 1.65; padding-right: 20px;"
              >
                <div class="sidebar-html-content" v-html="formatDesc(item.info)"></div>
                <button
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print"
                >
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                  </svg>
                </button>
              </div>
            </div>

            <!-- Sở thích -->
            <div
              v-else-if="section.id === 'hobbies'"
              style="display: flex; flex-direction: column; gap: 6px;"
            >
              <div
                v-for="(item, itemIndex) in section.items"
                :key="item._refId"
                class="item-container paginated-item"
                style="position: relative; color: #ffffff; font-size: 12.5px; font-weight: 700; line-height: 1.45; padding-right: 20px;"
              >
                <span v-html="item.name"></span>
                <button
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print"
                >
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                  </svg>
                </button>
              </div>
            </div>

            <!-- Default sidebar (Thông tin thêm, v.v.) -->
            <div
              v-else
              style="display: flex; flex-direction: column; gap: 7px;"
            >
              <div
                v-for="(item, itemIndex) in section.items"
                :key="item._refId"
                class="item-container paginated-item"
                style="position: relative; color: #fff; font-size: 12.5px; font-weight: 600; line-height: 1.5; padding-right: 20px;"
              >
                <span class="sidebar-html-content" v-html="formatDesc(item.desc || item.name || item.info)"></span>
                <button
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print"
                >
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

    <!-- ===================== CỘT PHẢI (MAIN CONTENT) ===================== -->
    <main
      class="flex-1 flex flex-col relative z-10"
      style="padding: 8mm 8mm 8mm 8mm; overflow: hidden;"
      @click.self="selectedSectionId = null"
    >
      <template v-for="section in mainSections" :key="section.id">
        <div
          v-show="section.isVisible"
          class="section-block section-block-main"
          style="padding: 4mm 4mm 3.5mm 4mm; margin-bottom: 1px;"
          :class="{ 'section-selected': selectedSectionId === section.id }"
          :style="selectedSectionId === section.id ? { '--sel-color': resumeData.theme.primaryColor || '#dfa234' } : {}"
          @mouseenter="showNav(section.id)"
          @mouseleave="hideNav()"
          @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
        >
          <!-- Nav buttons -->
          <div
            v-show="hoveredSectionId === section.id || selectedSectionId === section.id"
            class="nav-btns no-print"
            @mouseenter="showNav(section.id)"
            @mouseleave="hideNav()"
          >
            <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Lên">
              <svg width="13" height="13" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/>
              </svg>
            </button>
            <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Xuống">
              <svg width="13" height="13" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/>
              </svg>
            </button>
            <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái">
              <svg width="13" height="13" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/>
              </svg>
            </button>
          </div>

          <!-- Tiêu đề section main -->
          <div class="main-section-header paginated-item" style="margin-bottom: 10px;">
            <h3
              class="main-section-title"
              :style="{ color: resumeData.theme.primaryColor || '#dfa234' }"
            >{{ section.title }}</h3>
            <div class="main-divider" :style="{ backgroundColor: resumeData.theme.primaryColor || '#dfa234' }"></div>
          </div>

          <!-- Mục tiêu nghề nghiệp -->
          <div
            v-if="section.id === 'summary'"
            class="paginated-item html-content"
            style="font-size: 13.5px; line-height: 1.7; color: #1a1a1a; text-align: justify; font-weight: 500;"
            v-html="resumeData.general.summary || 'Chưa có thông tin.'"
          ></div>

          <!-- Kinh nghiệm làm việc / Dự án -->
          <div
            v-else-if="section.id === 'experience' || section.id === 'project'"
            style="display: flex; flex-direction: column; gap: 16px;"
          >
            <div
              v-for="(item, itemIndex) in section.items"
              :key="item._refId"
              class="item-container paginated-item"
              style="position: relative;"
            >
              <button
                @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                class="delete-btn no-print"
                style="top: 0; right: 0;"
              >
                <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                </svg>
              </button>
              <!-- Dòng 1: Role + Time -->
              <div style="display: flex; justify-content: space-between; align-items: baseline; margin-bottom: 2px;">
                <span style="font-weight: 800; font-size: 14px; color: #111; line-height: 1.3;">
                  {{ section.id === 'experience' ? item.role : (item.role || item.name) }}
                </span>
                <span style="font-size: 12.5px; font-weight: 700; color: #555; white-space: nowrap; padding-left: 6px;">{{ item.time }}</span>
              </div>
              <!-- Dòng 2: Company -->
              <div style="font-size: 13px; font-weight: 700; color: #444; margin-bottom: 6px;">
                {{ section.id === 'experience' ? item.company : (item.company || '') }}
              </div>
              <!-- Mô tả -->
              <div
                class="html-content"
                style="font-size: 13px; line-height: 1.6; color: #222; text-align: justify;"
                v-html="formatDesc(item.desc)"
              ></div>
            </div>
          </div>

          <!-- Hoạt động -->
          <div
            v-else-if="section.id === 'activities'"
            style="display: flex; flex-direction: column; gap: 16px;"
          >
            <div
              v-for="(item, itemIndex) in section.items"
              :key="item._refId"
              class="item-container paginated-item"
              style="position: relative;"
            >
              <button
                @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                class="delete-btn no-print"
                style="top: 0; right: 0;"
              >
                <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                </svg>
              </button>
              <!-- Dòng 1: Tên hoạt động/Vai trò + Time -->
              <div style="display: flex; justify-content: space-between; align-items: baseline; margin-bottom: 2px;">
                <span style="font-weight: 800; font-size: 14px; color: #111; line-height: 1.3;">
                  {{ item.role || item.name }}
                </span>
                <span style="font-size: 12.5px; font-weight: 700; color: #555; white-space: nowrap; padding-left: 6px;">{{ item.time }}</span>
              </div>
              <!-- Dòng 2: Tên tổ chức/CLB (nếu có) -->
              <div v-if="item.company || item.club || item.organization" style="font-size: 13px; font-weight: 700; color: #444; margin-bottom: 6px;">
                {{ item.company || item.club || item.organization }}
              </div>
              <!-- Mô tả -->
              <div
                class="html-content"
                style="font-size: 13px; line-height: 1.6; color: #222; text-align: justify;"
                v-html="formatDesc(item.desc)"
              ></div>
            </div>
          </div>

          <!-- Học Vấn -->
          <div
            v-else-if="section.id === 'education'"
            style="display: flex; flex-direction: column; gap: 14px;"
          >
            <div
              v-for="(item, itemIndex) in section.items"
              :key="item._refId"
              class="item-container paginated-item"
              style="position: relative;"
            >
              <button
                @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                class="delete-btn no-print"
                style="top: 0; right: 0;"
              >
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
              <div
                v-if="item.desc"
                class="html-content"
                style="font-size: 13px; line-height: 1.6; color: #222; margin-top: 4px;"
                v-html="formatDesc(item.desc)"
              ></div>
            </div>
          </div>

          <!-- Default Main -->
          <div
            v-else
            style="display: flex; flex-direction: column; gap: 12px;"
          >
            <div
              v-for="(item, itemIndex) in section.items"
              :key="item._refId"
              class="item-container paginated-item html-content"
              style="position: relative; font-size: 13px; line-height: 1.6; color: #222;"
            >
              <div v-html="formatDesc(item.desc || item.name || item.info)"></div>
              <button
                @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                class="delete-btn no-print"
                style="top: 0; right: 0;"
              >
                <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/>
                </svg>
              </button>
            </div>
          </div>
        </div>
      </template>
    </main>

    <!-- DIVIDER PAGES (Chỉ dành cho xem trên Web) -->
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

const props = defineProps({
  resumeData: { type: Object, required: true }
})

const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

const cvRoot = ref(null)
const pageCount = ref(1)

// --- UTILS ---
const isEmpty = (val) => {
  if (!val) return true
  if (typeof val !== 'string') return false
  return val.replace(/<[^>]*>/g, '').trim() === ''
}

// --- PAGINATION ENGINE (Optimized & Stable từ NguyenYenNhi.vue) ---
let paginateTimer = null
const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 50)
}

const doPagination = async () => {
  if (!cvRoot.value) return

  const allPaginated = cvRoot.value.querySelectorAll('.paginated-item')
  allPaginated.forEach(el => { el.style.marginTop = '' })

  await nextTick()

  const A4_WIDTH_MM = 210
  const A4_HEIGHT_MM = 297
  const MARGIN_BOTTOM_MM = 15
  const MARGIN_TOP_MM = 15

  const rootWidthPx = cvRoot.value.offsetWidth
  const pxPerMm = rootWidthPx / A4_WIDTH_MM
  const pageHeightPx = A4_HEIGHT_MM * pxPerMm
  const marginBottomPx = MARGIN_BOTTOM_MM * pxPerMm
  const marginTopPx = MARGIN_TOP_MM * pxPerMm
  const safeBottomPx = pageHeightPx - marginBottomPx

  const getRelativeTop = (el) => {
    let offset = 0
    let currentEl = el
    while (currentEl && currentEl !== cvRoot.value) {
      offset += currentEl.offsetTop
      currentEl = currentEl.offsetParent
    }
    return offset
  }

  const processColumn = (colSelector) => {
    const col = cvRoot.value.querySelector(colSelector)
    if (!col) return
    const colItems = col.classList.contains('paginated-item')
      ? [col]
      : col.querySelectorAll('.paginated-item')

    let isStable = false
    let attempts = 0

    while (!isStable && attempts < 50) {
      isStable = true
      attempts++

      for (let i = 0; i < colItems.length; i++) {
        const item = colItems[i]
        if (!item) continue

        const top = getRelativeTop(item)
        const height = item.offsetHeight
        const bottom = top + height

        const currentPageIndex = Math.floor(top / pageHeightPx)
        const currentSafeBottom = (currentPageIndex * pageHeightPx) + safeBottomPx

        if (bottom > currentSafeBottom && top < (currentPageIndex + 1) * pageHeightPx) {
          if (height > (pageHeightPx - marginTopPx - marginBottomPx)) {
            continue
          }
          const targetTop = (currentPageIndex + 1) * pageHeightPx + marginTopPx
          const pushAmount = targetTop - top
          const currentMt = parseFloat(item.style.marginTop || '0')
          item.style.marginTop = (currentMt + pushAmount) + 'px'

          isStable = false
          break
        }
      }
    }
  }

  processColumn('aside')
  processColumn('main')

  let maxBottom = 0
  allPaginated.forEach(el => {
    const top = getRelativeTop(el)
    const bottom = top + el.offsetHeight
    if (bottom > maxBottom) maxBottom = bottom
  })

  const calculatedPageCount = Math.max(1, Math.ceil((maxBottom + marginBottomPx - 2) / pageHeightPx))
  if (pageCount.value !== calculatedPageCount) {
    pageCount.value = calculatedPageCount
  }
}

watch(() => props.resumeData, () => requestPagination(), { deep: true })

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

// --- NAVIGATION & INTERACTION ---
const hoveredSectionId = ref(null)
let _hideTimer = null

const showNav = (id) => {
  if (_hideTimer) { clearTimeout(_hideTimer); _hideTimer = null }
  hoveredSectionId.value = id
}

const hideNav = () => {
  _hideTimer = setTimeout(() => {
    hoveredSectionId.value = null
    _hideTimer = null
  }, 120)
}

const selectedSectionId = ref(null)

const sidebarIds = computed(() => sidebarSections.value.map(s => s.id))
const mainIds = computed(() => mainSections.value.map(s => s.id))

const sidebarSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'left')
)

const mainSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'right')
)

const formatDesc = (text) => {
  if (!text) return ''
  if (/<[a-z][\s\S]*>/i.test(text)) {
    if (text.includes('<li') && (text.includes('<font') || text.includes('style='))) {
      try {
        const tempDiv = document.createElement('div')
        tempDiv.innerHTML = text
        const lis = tempDiv.querySelectorAll('li')
        lis.forEach(li => {
          const child = li.firstElementChild
          if (child && (child.tagName === 'FONT' || child.tagName === 'SPAN')) {
            if (child.color) li.style.color = child.color
            if (child.style && child.style.color) li.style.color = child.style.color
          }
        })
        return tempDiv.innerHTML
      } catch (e) {
        return text
      }
    }
    return text
  }
  return text.split('\n').map(l => l.trim()).filter(l => l).join('<br/>')
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&display=swap');

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

/* ===== SIDEBAR STYLES ===== */
.sidebar-section-header {
  display: flex;
  align-items: center;
  gap: 10px;
}

.sidebar-section-title {
  font-size: 14px;
  font-weight: 800;
  text-transform: uppercase;
  white-space: nowrap;
  letter-spacing: 0.03em;
}

.sidebar-divider {
  flex: 1;
  height: 1px;
  opacity: 0.8;
}

.contact-row {
  display: flex;
  align-items: flex-start;
  gap: 10px;
}

.contact-icon {
  flex-shrink: 0;
  margin-top: 2px;
  display: flex;
  align-items: center;
}

.contact-text {
  font-size: 12.5px;
  font-weight: 600;
  color: #fff;
  line-height: 1.4;
  word-break: break-all;
}

/* ===== MAIN SECTION HEADER ===== */
.main-section-header {
  display: flex;
  align-items: center;
  gap: 10px;
}

.main-section-title {
  font-size: 15px;
  font-weight: 900;
  text-transform: uppercase;
  white-space: nowrap;
  letter-spacing: 0.03em;
}

.main-divider {
  flex: 1;
  height: 1.5px;
}

/* ===== HTML CONTENT ===== */
:deep(.html-content ul) {
  list-style-type: disc !important;
  padding-left: 1.2rem !important;
  margin-top: 0.1rem;
  margin-bottom: 0.1rem;
}
:deep(.html-content ol) {
  list-style-type: decimal !important;
  padding-left: 1.2rem !important;
  margin-top: 0.1rem;
  margin-bottom: 0.1rem;
}
:deep(.html-content li) {
  margin-bottom: 0.15rem;
}
:deep(.html-content p) {
  margin-bottom: 0.15rem;
}
:deep(.html-content b), :deep(.html-content strong) { font-weight: 700; color: #111; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic; }
:deep(.html-content u) { text-decoration: underline; }

:deep(.html-content li:has(> font[size="1"])) { font-size: 10px; }
:deep(.html-content li:has(> font[size="2"])) { font-size: 13px; }
:deep(.html-content li:has(> font[size="3"])) { font-size: 16px; }

/* SIDEBAR HTML OVERRIDES */
:deep(.sidebar-html-content p),
:deep(.sidebar-html-content span),
:deep(.sidebar-html-content div) {
  color: inherit;
  margin-bottom: 0.1rem;
}
:deep(.sidebar-html-content b),
:deep(.sidebar-html-content strong) {
  color: white;
  font-weight: 700;
}
:deep(.sidebar-html-content ul) {
  list-style-type: disc !important;
  padding-left: 1.1rem !important;
  margin-top: 0.1rem;
  margin-bottom: 0.1rem;
}
:deep(.sidebar-html-content li) {
  margin-bottom: 0.1rem;
}

/* ===== ITEM CONTAINERS ===== */
.item-container {
  position: relative;
  transition: all 0.2s;
}

aside .item-container:hover {
  background-color: rgba(255, 255, 255, 0.05);
  border-radius: 5px;
}

/* ===== DELETE BUTTON ===== */
.delete-btn {
  opacity: 0;
  transition: all 0.2s;
  position: absolute;
  top: 2px;
  right: 2px;
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  background: #ef4444;
  color: white;
  border: 2px solid white;
  border-radius: 999px;
  pointer-events: none;
  z-index: 30;
}

.item-container:hover .delete-btn {
  opacity: 1;
  pointer-events: auto;
}

aside .delete-btn {
  border-color: #3a3e43;
}

/* ===== SECTION BLOCKS ===== */
.section-block {
  position: relative;
  border-radius: 6px;
  border: 2px solid transparent;
  cursor: pointer;
  transition: border-color 0.2s ease, background 0.2s ease;
}

.section-block-main:hover {
  background: rgba(0, 0, 0, 0.015);
}

.section-block-sidebar:hover {
  background: rgba(255, 255, 255, 0.03);
}

.section-selected {
  border-color: var(--sel-color, #dfa234);
}

.section-selected.section-block-main {
  background: color-mix(in srgb, var(--sel-color, #dfa234) 5%, white) !important;
}

.section-selected.section-block-sidebar {
  background: rgba(255, 255, 255, 0.06) !important;
}

/* ===== NAV BUTTONS ===== */
.nav-btns {
  position: absolute;
  right: 4px;
  top: 4px;
  display: flex;
  flex-direction: row;
  gap: 4px;
  z-index: 9999;
}

.nav-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 4px;
  background: #dfa234;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  box-shadow: 0 2px 5px rgba(0,0,0,0.25);
  transition: background 0.15s ease, transform 0.1s ease;
}

.nav-btn:hover {
  background: #c78d2b;
  transform: scale(1.1);
}

.nav-btn:active {
  transform: scale(0.95);
}

/* ===== PRINT / PDF EXPORT ===== */
@media print {
  .no-print {
    display: none !important;
  }
  .section-block,
  .section-selected {
    cursor: default;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
    padding: 0 !important;
    margin: 0 !important;
    border-radius: 0 !important;
    margin-bottom: 12px !important;
  }
  aside .section-block {
    margin-bottom: 16px !important;
    padding-left: 8mm !important;
    padding-right: 8mm !important;
  }
  main .section-block {
    padding-left: 4mm !important;
    padding-right: 4mm !important;
  }
}

:global(.is-exporting-pdf .no-print) {
  display: none !important;
}
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .section-selected) {
  cursor: default !important;
  box-shadow: none !important;
  background: transparent !important;
  border-color: transparent !important;
  padding: 0 !important;
  margin: 0 !important;
  border-radius: 0 !important;
  margin-bottom: 12px !important;
}
:global(.is-exporting-pdf aside .section-block) {
  padding-left: 8mm !important;
  padding-right: 8mm !important;
}
:global(.is-exporting-pdf main .section-block) {
  padding-left: 4mm !important;
  padding-right: 4mm !important;
}
</style>
