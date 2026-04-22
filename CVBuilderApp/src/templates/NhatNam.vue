<template>
  <div id="cv-printable-area" ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-row relative box-border text-[#333] leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', \'Segoe UI\', sans-serif' }">

    <!-- ==================== CỘT TRÁI (SIDEBAR) ==================== -->
    <aside class="w-[70mm] shrink-0 flex flex-col relative z-10" :style="{ backgroundColor: templateSecondaryColor }">

      <!-- HEADER: Tên + Chức danh + Ảnh -->
      <div class="pt-[14mm] px-[8mm] pb-[6mm] flex flex-col items-center text-center paginated-item">
        <h1 class="font-bold leading-tight mb-1"
          :style="{ fontSize: '20px', color: templatePrimaryColor }"
          v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'Nguyễn Văn A'">
        </h1>
        <h2 class="font-medium opacity-75 mb-5 text-[#333]" style="font-size: 12px"
          v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'Trình dược viên'">
        </h2>

        <!-- Ảnh đại diện tròn -->
        <div class="relative w-[42mm] h-[42mm] rounded-full overflow-hidden border-[3px] bg-gray-100"
          :style="{ borderColor: templatePrimaryColor }">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl"
            class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center text-gray-400">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1"
                d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
          </div>
        </div>
      </div>

      <!-- THÔNG TIN CÁ NHÂN (cố định, không thuộc section) -->
      <div class="px-[8mm] pb-[4mm]">
        <h3 class="font-bold mb-3" :style="{ fontSize: '13px', color: templatePrimaryColor, fontWeight: '700' }">
          Thông tin cá nhân
        </h3>
        <div class="flex flex-col gap-[7px]" style="font-size: 11px; color: #333;">
          <!-- Năm sinh -->
          <div class="flex items-center gap-2" v-if="!isEmpty(resumeData.general.birthDate)">
            <div class="w-4 h-4 flex items-center justify-center shrink-0" :style="{ color: templatePrimaryColor }">
              <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 20 20">
                <path d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" />
              </svg>
            </div>
            <span v-html="resumeData.general.birthDate"></span>
          </div>
          <!-- Giới tính -->
          <div class="flex items-center gap-2" v-if="!isEmpty(resumeData.general.gender)">
            <div class="w-4 h-4 flex items-center justify-center shrink-0" :style="{ color: templatePrimaryColor }">
              <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clip-rule="evenodd" />
              </svg>
            </div>
            <span v-html="resumeData.general.gender"></span>
          </div>
          <!-- SĐT -->
          <div class="flex items-center gap-2" v-if="!isEmpty(resumeData.general.phone)">
            <div class="w-4 h-4 flex items-center justify-center shrink-0" :style="{ color: templatePrimaryColor }">
              <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 20 20">
                <path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z" />
              </svg>
            </div>
            <span class="break-all" v-html="resumeData.general.phone"></span>
          </div>
          <!-- Email -->
          <div class="flex items-center gap-2" v-if="!isEmpty(resumeData.general.email)">
            <div class="w-4 h-4 flex items-center justify-center shrink-0" :style="{ color: templatePrimaryColor }">
              <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 20 20">
                <path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z" />
                <path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z" />
              </svg>
            </div>
            <span class="break-all" v-html="resumeData.general.email"></span>
          </div>
          <!-- Website / Github / Linkedin -->
          <div class="flex items-center gap-2"
            v-if="!isEmpty(resumeData.general.github) || !isEmpty(resumeData.general.website) || !isEmpty(resumeData.general.linkedin)">
            <div class="w-4 h-4 flex items-center justify-center shrink-0" :style="{ color: templatePrimaryColor }">
              <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM4.332 8.027a6.012 6.012 0 011.912-2.706C6.512 5.73 6.974 6 7.5 6A1.5 1.5 0 019 7.5V8a2 2 0 004 0 2 2 0 011.523-1.943A5.977 5.977 0 0116 10c0 .34-.028.675-.083 1H15a2 2 0 00-2 2v2.197A5.973 5.973 0 0110 16v-2a2 2 0 00-2-2 2 2 0 01-2-2 2 2 0 00-1.668-1.973z" clip-rule="evenodd" />
              </svg>
            </div>
            <span class="break-all"
              v-html="!isEmpty(resumeData.general.github) ? resumeData.general.github : (!isEmpty(resumeData.general.linkedin) ? resumeData.general.linkedin : resumeData.general.website)">
            </span>
          </div>
          <!-- Địa chỉ -->
          <div class="flex items-center gap-2" v-if="!isEmpty(resumeData.general.address)">
            <div class="w-4 h-4 flex items-center justify-center shrink-0" :style="{ color: templatePrimaryColor }">
              <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd" />
              </svg>
            </div>
            <span class="break-words" v-html="resumeData.general.address"></span>
          </div>
        </div>
        <!-- Đường kẻ -->
        <div class="w-full border-b mt-4 opacity-25" :style="{ borderColor: templatePrimaryColor }"></div>
      </div>

      <!-- SIDEBAR SECTIONS (Kỹ năng, Học vấn bên trái, v.v.) -->
      <div class="px-[8mm] flex-1 pb-8 flex flex-col">
        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block relative group mb-5"
            :class="{ 'section-selected': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--sel-color': templatePrimaryColor } : {}"
            @mouseenter="showNav(section.id)"
            @mouseleave="hideNav()"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <!-- Nút điều hướng -->
            <div v-show="hoveredSectionId === section.id || selectedSectionId === section.id"
              class="nav-btns no-print"
              @mouseenter="showNav(section.id)"
              @mouseleave="hideNav()">
              <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
              </button>
            </div>

            <!-- Tiêu đề section -->
            <h3 class="font-bold mb-3 paginated-item" :style="{ fontSize: '13px', color: templatePrimaryColor, fontWeight: '700' }">
              {{ section.title }}
            </h3>

            <!-- Kỹ năng / Ngôn ngữ / IT -->
            <div v-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'" class="space-y-2">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative paginated-item">
                <!-- Tên kỹ năng dạng box liền -->
                <div class="font-semibold text-[#333] py-[5px] px-[8px] rounded"
                  :style="{ fontSize: '11px', backgroundColor: templatePrimaryColor + '18', borderLeft: `3px solid ${templatePrimaryColor}` }">
                  {{ item.name }}
                  <span v-if="item.level" class="font-normal opacity-70 ml-1">({{ item.level }})</span>
                  <span v-if="item.info" class="font-normal opacity-70 block text-[10px] mt-0.5">{{ item.info }}</span>
                </div>
                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print" style="top: -4px; right: -4px;">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Học vấn (sidebar) -->
            <div v-else-if="section.id === 'education'" class="space-y-3">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative paginated-item" style="font-size: 11px; color: #333;">
                <div class="font-semibold">{{ item.school }}</div>
                <div v-if="item.year || item.gradType" class="opacity-65 text-[10px]">
                  <span v-if="item.year">{{ item.year }}</span>
                  <span v-if="item.year && item.gradType"> · </span>
                  <span v-if="item.gradType">{{ item.gradType }}</span>
                </div>
                <div v-if="item.major" class="opacity-80 text-[10.5px]">Chuyên ngành: {{ item.major }}</div>
                <div v-if="item.desc" class="html-content leading-relaxed mt-1" v-html="formatDesc(item.desc)"></div>
                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print" style="top: -4px; right: -4px;">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Giải thưởng / Chứng chỉ (sidebar) -->
            <div v-else-if="section.id === 'awards' || section.id === 'certifications'" class="space-y-2">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative paginated-item" style="font-size: 11px; color: #333;">
                <div class="font-semibold">{{ item.name }}</div>
                <div v-if="item.year" class="opacity-65 text-[10px]">{{ item.year }}</div>
                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print" style="top: -4px; right: -4px;">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Còn lại -->
            <div v-else class="space-y-2">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative paginated-item" style="font-size: 11px; color: #333;">
                <div class="html-content" v-html="formatDesc(item.desc || item.name || item.info)"></div>
                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print" style="top: -4px; right: -4px;">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Đường kẻ dưới section -->
            <div class="w-full border-b opacity-20 mt-4 paginated-item" :style="{ borderColor: templatePrimaryColor }"></div>
          </div>
        </template>
      </div>
    </aside>

    <!-- ==================== CỘT PHẢI (MAIN) ==================== -->
    <main class="flex-1 flex flex-col bg-white z-20 relative" @click.self="selectedSectionId = null">
      <div class="px-[10mm] pt-[14mm] pb-[8mm] flex-1 flex flex-col gap-[6mm]">

        <!-- MỤC TIÊU NGHỀ NGHIỆP (summary – cố định đầu cột phải) -->
        <div v-if="summarySection && summarySection.isVisible"
          class="section-block relative group"
          :class="{ 'section-selected': selectedSectionId === summarySection.id }"
          :style="selectedSectionId === summarySection.id ? { '--sel-color': templatePrimaryColor } : {}"
          @mouseenter="showNav(summarySection.id)"
          @mouseleave="hideNav()"
          @click.stop="selectedSectionId = selectedSectionId === summarySection.id ? null : summarySection.id">

          <div v-show="hoveredSectionId === summarySection.id || selectedSectionId === summarySection.id"
            class="nav-btns no-print"
            @mouseenter="showNav(summarySection.id)"
            @mouseleave="hideNav()">
            <button @click.stop.prevent="$emit('moveDown', summarySection.id, mainIds)" class="nav-btn" title="Di chuyển xuống">
              <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
            </button>
          </div>

          <!-- Tiêu đề section có gạch chân màu teal -->
          <div class="flex items-center gap-2 mb-3 paginated-item">
            <h3 class="font-bold" :style="{ fontSize: '15px', color: templatePrimaryColor, fontWeight: '700' }">
              {{ summarySection.title || 'Mục tiêu nghề nghiệp' }}
            </h3>
          </div>
          <div class="leading-relaxed text-justify html-content font-normal text-[#333] paginated-item"
            style="font-size: 11px;"
            v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Nhập mục tiêu nghề nghiệp của bạn tại đây...'">
          </div>
          <div class="w-full border-b border-slate-200 mt-4 paginated-item"></div>
        </div>

        <!-- CÁC SECTIONS CHÍNH -->
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block relative group"
            :class="{ 'section-selected': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--sel-color': templatePrimaryColor } : {}"
            @mouseenter="showNav(section.id)"
            @mouseleave="hideNav()"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id">

            <!-- Nút điều hướng -->
            <div v-show="hoveredSectionId === section.id || selectedSectionId === section.id"
              class="nav-btns no-print"
              @mouseenter="showNav(section.id)"
              @mouseleave="hideNav()">
              <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
              </button>
            </div>

            <!-- Tiêu đề section -->
            <div class="flex items-center gap-2 mb-3 paginated-item">
              <h3 class="font-bold" :style="{ fontSize: '15px', color: templatePrimaryColor, fontWeight: '700' }">
                {{ section.title }}
              </h3>
            </div>

            <!-- Học vấn (main) -->
            <div v-if="section.id === 'education'" class="space-y-4">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative paginated-item">
                <div class="flex justify-between items-baseline gap-4 mb-0.5">
                  <h4 class="font-bold text-[#333]" style="font-size: 12px;">{{ item.school }}</h4>
                  <span v-if="item.year" class="font-normal text-slate-400 italic shrink-0 text-[10.5px]">{{ item.year }}</span>
                </div>
                <div v-if="item.major" class="text-[#333] font-normal mb-0.5" style="font-size: 11px;">
                  Chuyên ngành: {{ item.major }}
                </div>
                <div v-if="item.gradType" class="text-slate-500 italic" style="font-size: 10.5px;">{{ item.gradType }}</div>
                <div v-if="item.desc" class="html-content leading-relaxed mt-1 text-[#333]" style="font-size: 11px;" v-html="formatDesc(item.desc)"></div>
                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print" style="top: -4px; right: -4px;">
                  <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Kinh nghiệm / Dự án / Hoạt động (main) -->
            <div v-else-if="section.id === 'experience' || section.id === 'project' || section.id === 'activities'" class="space-y-5">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative paginated-item">
                <div class="flex justify-between items-baseline gap-4 mb-0.5">
                  <h4 class="font-bold text-[#333]" style="font-size: 12px;">
                    {{ section.id === 'experience' ? item.role : (item.name || '') }}
                  </h4>
                  <span v-if="item.time" class="font-normal text-slate-400 italic shrink-0 text-[10.5px]">{{ item.time }}</span>
                </div>
                <div v-if="item.company || (section.id !== 'experience' && item.role)"
                  class="font-normal text-slate-500 mb-1.5" style="font-size: 11.5px;">
                  {{ section.id === 'experience' ? item.company : item.role }}
                </div>
                <div class="leading-relaxed text-[#333] html-content" style="font-size: 11px;"
                  v-html="formatDesc(item.desc)">
                </div>
                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print" style="top: -4px; right: -4px;">
                  <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Giải thưởng / Chứng chỉ (main) -->
            <div v-else-if="section.id === 'awards' || section.id === 'certifications'" class="space-y-2">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative flex items-start gap-4 paginated-item">
                <span v-if="item.year" class="font-bold text-[#333] shrink-0" style="font-size: 11px;">{{ item.year }}</span>
                <span class="font-normal text-[#333] html-content" style="font-size: 11px;">{{ item.name || item.info }}</span>
                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print" style="top: -4px; right: -4px;">
                  <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Người tham chiếu -->
            <div v-else-if="section.id === 'references'" class="space-y-3">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative paginated-item pl-3 border-l-2 html-content text-[#333] leading-relaxed"
                style="font-size: 11px;"
                :style="{ borderLeftColor: templatePrimaryColor + '60' }"
                v-html="formatDesc(item.info)">
              </div>
            </div>

            <!-- Mặc định -->
            <div v-else class="space-y-3">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative text-[#333] leading-relaxed paginated-item" style="font-size: 11px;">
                <div class="html-content" v-html="formatDesc(item.desc || item.info || item.name)"></div>
                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print" style="top: -4px; right: -4px;">
                  <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <div class="w-full border-b border-slate-200 mt-4 paginated-item"></div>
          </div>
        </template>
      </div>
    </main>

    <!-- ==================== ĐƯỜNG PHÂN TRANG ==================== -->
    <template v-for="p in (pageCount - 1)" :key="'div-'+p">
      <div class="absolute left-0 w-full z-50 flex flex-col items-center justify-center pointer-events-none no-print"
        :style="{ top: `calc(${p * 297}mm - 8px)` }">
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

// ==================== UTILS ====================
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
            } catch(e) { return text }
        }
        return text
    }
    return text.split('\n').map(l => l.trim()).filter(l => l).join('<br/>')
}

// ==================== COLORS ====================
const templatePrimaryColor = computed(() => {
    const c = props.resumeData?.theme?.primaryColor
    if (!c || c.toLowerCase() === '#2b5c8f') return '#2a9d8f'
    return c
})

const templateSecondaryColor = computed(() => '#e5f6f8')

// ==================== SECTIONS ====================
const summarySection = computed(() => props.resumeData.sections.find(s => s.id === 'summary'))
const sidebarSections = computed(() => props.resumeData.sections.filter(s => s.column === 'left' && s.id !== 'summary'))
const mainSections = computed(() => props.resumeData.sections.filter(s => s.column === 'right' && s.id !== 'summary'))
const sidebarIds = computed(() => sidebarSections.value.map(s => s.id))
const mainIds = computed(() => mainSections.value.map(s => s.id))

// ==================== HOVER / SELECT ====================
const hoveredSectionId = ref(null)
const selectedSectionId = ref(null)
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

// ==================== PAGINATION ENGINE ====================
let paginateTimer = null
const requestPagination = () => {
    if (paginateTimer) clearTimeout(paginateTimer)
    paginateTimer = setTimeout(doPagination, 50)
}

const doPagination = async () => {
    if (!cvRoot.value) return

    const items = cvRoot.value.querySelectorAll('.paginated-item')
    items.forEach(el => { el.style.marginTop = '' })

    await nextTick()
    await new Promise(r => setTimeout(r, 50))

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
                const nextPageTop = (currentPageIndex + 1) * pageHeightPx

                const isOverflowing = bottom > currentSafeBottom
                const isCrossingPageBreak = top < nextPageTop && bottom > nextPageTop

                if (isOverflowing || isCrossingPageBreak) {
                    if (height > (pageHeightPx - marginTopPx - marginBottomPx)) continue

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

    processColumn('main')
    processColumn('aside')

    let maxBottom = 0
    items.forEach(el => {
        const top = getRelativeTop(el)
        const bottom = top + el.offsetHeight
        if (bottom > maxBottom) maxBottom = bottom
    })

    const calculatedPageCount = Math.max(1, Math.ceil((maxBottom + marginBottomPx - 2) / pageHeightPx))
    if (pageCount.value !== calculatedPageCount) pageCount.value = calculatedPageCount
}

watch(() => props.resumeData, () => requestPagination(), { deep: true })

onMounted(() => {
    // Chỉ active các mục xuất hiện trong ảnh mẫu, ẩn tất cả còn lại
    const leftSections = ['skills']
    const rightSections = ['summary', 'education', 'experience', 'certifications']
    const activeSections = [...leftSections, ...rightSections]

    if (props.resumeData?.sections) {
        props.resumeData.sections.forEach(sec => {
            if (leftSections.includes(sec.id)) {
                sec.isVisible = true
                sec.column = 'left'
            } else if (rightSections.includes(sec.id)) {
                sec.isVisible = true
                sec.column = 'right'
            } else {
                sec.isVisible = false
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
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap');

#cv-printable-area {
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
    overflow-wrap: anywhere;
    font-family: 'Inter', 'Segoe UI', sans-serif;
}

/* ==================== ITEM CONTAINER ==================== */
.item-container {
    position: relative;
    transition: all 0.2s;
}

/* ==================== DELETE BUTTON ==================== */
.delete-btn {
    position: absolute;
    opacity: 0;
    pointer-events: none;
    background: #ef4444;
    color: white;
    border-radius: 999px;
    width: 18px;
    height: 18px;
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 50;
    transition: all 0.2s;
    box-shadow: 0 2px 4px rgba(0,0,0,0.15);
    cursor: pointer;
    border: none;
}
.item-container:hover .delete-btn {
    opacity: 1;
    pointer-events: auto;
}

/* ==================== HTML CONTENT ==================== */
:deep(.html-content ul) {
    list-style-type: disc !important;
    padding-left: 1.25rem !important;
    margin-top: 0.2rem;
    margin-bottom: 0.2rem;
}
:deep(.html-content ol) {
    list-style-type: decimal !important;
    padding-left: 1.25rem !important;
    margin-top: 0.2rem;
    margin-bottom: 0.2rem;
}
:deep(.html-content b), :deep(.html-content strong) { font-weight: 700; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic; }
:deep(.html-content u) { text-decoration: underline; }
:deep(.html-content ul li), :deep(.html-content ol li) { margin-bottom: 0.2rem; }

/* ==================== SECTION BLOCK ==================== */
.section-block {
    position: relative;
    border-radius: 5px;
    border: 2px solid transparent;
    cursor: pointer;
    transition: all 0.15s ease;
    padding: 4px;
    margin: -4px;
}
.section-block:hover {
    background: rgba(0, 0, 0, 0.015);
}
.section-selected {
    outline: 1.5px solid var(--sel-color, #38b2ac);
    outline-offset: 2px;
    border-radius: 3px;
}

/* ==================== NAV BUTTONS ==================== */
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
    background: #2563eb;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    box-shadow: 0 2px 6px rgba(37, 99, 235, 0.4);
    transition: all 0.15s ease;
}
.nav-btn:hover { background: #1d4ed8; transform: scale(1.1); }
.nav-btn:active { transform: scale(0.95); }

/* ==================== PRINT ==================== */
@media print {
    .no-print { display: none !important; }
    .section-block, .section-selected {
        cursor: default;
        box-shadow: none !important;
        background: transparent !important;
        border-color: transparent !important;
        padding: 0 !important;
        margin: 0 !important;
        border-radius: 0 !important;
    }
}
:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .section-selected) {
    cursor: default !important;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
}
</style>