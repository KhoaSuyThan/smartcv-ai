<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-row relative box-border text-[#333] leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif' }"
    @click.self="selectedSectionId = null"
  >
    <!-- MÀU NỀN TRẮNG TOÀN CỤC -->
    <div class="absolute inset-0 bg-white z-0 pointer-events-none"></div>

    <!-- LEFT COLUMN (SIDEBAR NỀN NÂU SẪM) -->
    <aside class="w-[78mm] z-10 flex flex-col pt-0 shrink-0 relative text-white overflow-hidden" :style="{ backgroundColor: templatePrimaryColor }">
      <!-- Khối cong trang trí màu nâu nhạt hơn phía sau Avatar -->
      <div class="absolute top-0 left-0 right-0 h-[80mm] overflow-hidden pointer-events-none z-0">
        <div class="absolute top-[-105mm] left-[-30mm] w-[138mm] h-[170mm] rounded-full" :style="{ backgroundColor: templateCircleColor }"></div>
      </div>

      <!-- Avatar Section -->
      <div class="pt-[15mm] px-[8mm] pb-[4mm] flex flex-col paginated-item relative z-10 items-center">
        <div class="relative w-[50mm] h-[50mm] rounded-full mx-auto flex items-center justify-center bg-transparent z-10">
          <!-- Vòng viền kép ngoài cùng -->
          <div class="absolute inset-[-6px] rounded-full border opacity-80" :style="{ borderColor: templateBorderColor }"></div>
          <!-- Vòng viền kép trong cùng bao quanh ảnh -->
          <div class="w-full h-full rounded-full overflow-hidden border-[3px] bg-gray-200" :style="{ borderColor: templateBorderColor }">
            <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
            <div v-else class="w-full h-full flex items-center justify-center text-white/40" :style="{ backgroundColor: templateCircleColor }">
              <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
            </div>
          </div>
        </div>
      </div>

      <!-- Name & Job Title Section -->
      <div class="text-center mt-6 px-[8mm] pb-4 z-10 relative paginated-item">
        <h1 class="font-bold text-white tracking-wide uppercase leading-tight mb-1" style="font-size: 18px !important;" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'TRẦN MẠNH DŨNG'"></h1>
        <h2 class="font-medium tracking-widest uppercase mt-2" style="font-size: 15px !important;" :style="{ color: templateSecondaryColor }" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'CONTENT LEADER'"></h2>
      </div>

      <!-- Sidebar Dynamic Content & Contact -->
      <div class="w-full px-[8mm] flex-1 pb-8 flex flex-col mt-2 z-10 relative">
        <!-- CONTACT INFORMATION SECTION -->
        <div v-if="contactItems.length > 0" class="section-block relative group mb-1 -mx-[4mm] px-[4mm] py-1 cursor-pointer hover:bg-black/5 transition-colors" 
             :class="{ 'section-active': selectedSectionId === 'contact' }"
             :style="selectedSectionId === 'contact' ? { '--active-bg': templatePillColor } : {}"
             @click.stop="toggleSection('contact')">
          <div class="paginated-item">
            <!-- Pill tiêu đề Liên hệ -->
            <div class="relative flex items-center mb-2.5">
              <div class="absolute left-0 right-0 h-[1px] bg-white/15 z-0"></div>
              <div class="relative z-10 text-white text-[11px] font-bold px-4 py-1 rounded-full uppercase tracking-wider" :style="{ backgroundColor: templatePillColor }">
                Liên hệ
              </div>
            </div>

            <!-- Các dòng thông tin liên hệ -->
            <div class="space-y-2.5 px-1 text-[11.5px] font-normal" :style="{ color: templateLightTextColor }">
              <div v-for="(ci, ciIdx) in contactItems" :key="ci.key" class="flex items-center gap-3.5 relative contact-item-container group/ci">
                <div class="w-4 h-4 flex items-center justify-center shrink-0 text-white/80">
                  <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24" v-html="ci.icon"></svg>
                </div>
                <span class="break-all" v-html="ci.value"></span>

                <!-- Move Up / Move Down / Delete buttons -->
                <transition name="fade-btns">
                  <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print">
                    <button v-if="ciIdx > 0" @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn" title="Di chuyển lên" style="padding:2px">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                    </button>
                    <button v-if="ciIdx < contactItems.length - 1" @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn" title="Di chuyển xuống" style="padding:2px">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                    </button>
                    <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn-danger" title="Ẩn mục này" style="padding:2px">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </div>
                </transition>
              </div>
            </div>
          </div>
        </div>

        <!-- DYNAMIC SIDEBAR SECTIONS (Học vấn, Kỹ năng, Sở thích) -->
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
          filter=".nav-btn, .delete-btn, .contact-item-btns, .html-content, input"
        >
          <template #item="{ element: section }">
          <div
            v-if="section.isVisible"
            :data-section-id="section.id" class="section-block relative group mb-1 -mx-[4mm] px-[4mm] py-1 cursor-pointer hover:bg-black/5 transition-colors"
            :class="{ 'section-active': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--active-bg': templatePillColor } : {}"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Control Buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print" style="right: 12px; top: 12px;" @click.stop>
                <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên"><svg class="pointer-events-none" width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống"><svg class="pointer-events-none" width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải"><svg class="pointer-events-none" width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg></button>
                <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn mục này"><svg class="pointer-events-none" width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg></button>
              </div>
            </transition>

            <div class="paginated-item">
              <!-- Pill tiêu đề -->
              <div class="relative flex items-center mb-2.5">
                <div class="absolute left-0 right-0 h-[1px] bg-white/15 z-0"></div>
                <div class="relative z-10 text-white text-[11px] font-bold px-4 py-1 rounded-full uppercase tracking-wider" :style="{ backgroundColor: templatePillColor }">
                  <span v-html="section.title"></span>
                </div>
              </div>
            </div>

            <!-- Content of Section -->
            <div class="space-y-2.5 px-1" v-if="sectionHasContent(section)">
              <!-- 1. MỤC TIÊU NGHỀ NGHIỆP (Nếu chuyển sang trái) -->
              <div v-if="section.id.toLowerCase().includes('summary')" class="item-container relative">
                <div class="text-[12px] leading-[1.7] text-justify font-normal html-content" :style="{ color: templateLightTextColor }" v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Chưa có thông tin mục tiêu nghề nghiệp.')"></div>
              </div>

              <!-- 2. HỌC VẤN (EDUCATION) -->
              <div v-else-if="section.id.toLowerCase().includes('education')" class="space-y-2.5">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="paginated-item item-container leading-relaxed text-[11.5px] text-white/95 relative"
                >
                  <div class="font-bold text-[12px] text-white leading-tight mb-1"><span v-html="item.major || item.degree"></span></div>
                  <div class="font-semibold mb-1" :style="{ color: templateSecondaryColor }"><span v-html="item.year"></span></div>
                  <div class="font-bold text-white mb-1.5"><span v-html="item.school"></span></div>
                  <div v-if="item.desc" class="text-[11px] leading-relaxed html-content" :style="{ color: templateLightTextColor }" v-html="formatDesc(item.desc)"></div>
                  
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: -5px; right: -5px;">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- 3. KINH NGHIỆM LÀM VIỆC (Nếu chuyển sang trái) -->
              <div v-else-if="section.id.toLowerCase().includes('experience')" class="space-y-4">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container text-[11.5px] relative text-white/95">
                  <div class="paginated-item flex justify-between items-start mb-1">
                    <h4 class="font-bold text-[12px] text-white leading-tight flex-1">
                      <span v-html="item.role || item.position"></span>
                    </h4>
                    <span class="font-bold text-[11px] shrink-0 ml-4" :style="{ color: templateSecondaryColor }">
                      <span v-html="item.time || item.year"></span>
                    </span>
                  </div>
                  <div class="paginated-item text-[11.5px] mb-1.5 font-medium" :style="{ color: templateSecondaryColor }">
                    <span v-html="item.company || item.organization"></span>
                  </div>
                  <div class="text-[11px] leading-relaxed html-content" :style="{ color: templateLightTextColor }" v-html="formatDesc(item.desc)"></div>
                  
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: -5px; right: -5px;">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- 4. DANH HIỆU & CHỨNG CHỈ (Nếu chuyển sang trái) -->
              <div v-else-if="section.id.toLowerCase().includes('awards') || section.id.toLowerCase().includes('certification')" class="space-y-3">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="paginated-item item-container flex items-baseline text-[11.5px] text-white/90 relative"
                >
                  <span class="font-bold text-white w-[50px] shrink-0"><span v-html="item.year"></span></span>
                  <span class="flex-1 font-normal html-content leading-relaxed" :style="{ color: templateLightTextColor }" v-html="formatDesc(item.name || item.info)"></span>
                  
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: 2px; right: 0;">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- 5. KỸ NĂNG, SỞ THÍCH & CÁC MỤC KHÁC -->
              <div v-else class="space-y-2">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="paginated-item text-[11.5px] font-medium leading-relaxed item-container pl-1 relative"
                  :style="{ color: templateLightTextColor }"
                >
                  <span v-html="item.name"></span>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: 2px; right: 0;">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>
            </div>
          </div>
        </template>
        </draggable>
      </div>
    </aside>

    <!-- RIGHT COLUMN (MAIN CONTENT NỀN TRẮNG) -->
    <main class="flex-1 flex flex-col relative z-20 min-h-max" @click.self="selectedSectionId = null">
      <!-- Toàn bộ nội dung bên phải cách lề chuẩn -->
      <div class="px-[10mm] pt-[15mm] pb-[15mm] flex-1 flex flex-col gap-[4mm]">
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
            v-if="section.isVisible"
            :data-section-id="section.id" class="section-block relative group -mx-[8mm] -my-[1.5mm] px-[8mm] py-[1.5mm] cursor-pointer hover:bg-black/5 transition-colors"
            :class="{ 'section-active': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--active-bg': templatePillColor } : {}"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Control Buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print" style="right: 20px; top: 12px;" @click.stop>
                <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên"><svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống"><svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái"><svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
                <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn mục này"><svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg></button>
              </div>
            </transition>

            <div class="paginated-item">
              <!-- Pill tiêu đề ở cột phải có đường kẻ ngang mỏng phía sau -->
              <div class="relative flex items-center mb-3">
                <div class="absolute left-0 right-0 h-[1px] bg-[#e2e8f0] z-0"></div>
                <div class="relative z-10 text-white text-[12px] font-bold px-4.5 py-1 rounded-full uppercase tracking-wider shadow-sm" :style="{ backgroundColor: templatePillColor }">
                  <span v-html="section.title"></span>
                </div>
              </div>
            </div>

            <div v-if="sectionHasContent(section)">
              <!-- 1. MỤC TIÊU NGHỀ NGHIỆP (SUMMARY) -->
              <div v-if="section.id.toLowerCase().includes('summary')" class="item-container relative">
                <div class="text-[12.5px] text-[#333333] leading-[1.75] text-justify font-normal html-content" v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Chưa có thông tin mục tiêu nghề nghiệp.')"></div>
              </div>

              <!-- 2. HỌC VẤN (Nếu chuyển sang cột phải) -->
              <div v-else-if="section.id.toLowerCase().includes('education')" class="space-y-4">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container text-[12.5px] relative">
                  <div class="paginated-item flex justify-between items-start mb-1">
                    <h4 class="font-bold text-[13.5px] text-[#111111] leading-tight flex-1">
                      <span v-html="item.major || item.degree"></span>
                    </h4>
                    <span class="font-bold text-[12.5px] text-[#111111] shrink-0 ml-4">
                      <span v-html="item.year"></span>
                    </span>
                  </div>
                  <div class="paginated-item text-[12px] text-[#555555] mb-1.5 font-medium">
                    <span v-html="item.school"></span>
                  </div>
                  <div v-if="item.desc" class="text-[12px] text-[#333333] leading-[1.7] text-justify font-normal html-content" v-html="formatDesc(item.desc)"></div>
                  
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- 3. KINH NGHIỆM LÀM VIỆC (EXPERIENCE) -->
              <div v-else-if="section.id.toLowerCase().includes('experience')" class="space-y-4">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container text-[12.5px] relative">
                  <!-- Dòng 1: Tên vị trí (Trái) và Thời gian (Phải) -->
                  <div class="paginated-item flex justify-between items-start mb-1">
                    <h4 class="font-bold text-[13.5px] text-[#111111] leading-tight flex-1">
                      <span v-html="item.role || item.position"></span>
                    </h4>
                    <span class="font-bold text-[12.5px] text-[#111111] shrink-0 ml-4">
                      <span v-html="item.time || item.year"></span>
                    </span>
                  </div>
                  <!-- Dòng 2: Tên công ty -->
                  <div class="paginated-item text-[12px] text-[#555555] mb-1.5 font-medium">
                    <span v-html="item.company || item.organization"></span>
                  </div>
                  <!-- Dòng 3: Nội dung công việc (Bullet points) -->
                  <div class="text-[12px] text-[#333333] leading-[1.7] text-justify font-normal html-content" v-html="formatDesc(item.desc)"></div>
                  
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- 4. DANH HIỆU & CHỨNG CHỈ (AWARDS & CERTIFICATIONS) -->
              <div v-else-if="section.id.toLowerCase().includes('awards') || section.id.toLowerCase().includes('certification')" class="space-y-4">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="paginated-item item-container flex items-baseline text-[12.5px] relative"
                >
                  <span class="font-bold text-[#111111] w-[60px] shrink-0"><span v-html="item.year"></span></span>
                  <span class="text-[#333333] flex-1 font-normal html-content leading-relaxed" v-html="formatDesc(item.name || item.info)"></span>
                  
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- 5. KỸ NĂNG & SỞ THÍCH HOẶC CÁC MỤC KHÁC (Nếu chuyển sang cột phải) -->
              <div v-else class="space-y-2">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="paginated-item text-[12.5px] text-[#333333] font-medium leading-relaxed item-container pl-1 relative"
                >
                  <span class="inline-block w-1.5 h-1.5 rounded-full mr-2 shrink-0" :style="{ backgroundColor: templatePillColor }"></span>
                  <span v-html="item.name"></span>
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
        </draggable>
      </div>

      <!-- Watermark footer -->
      <div class="absolute bottom-[6mm] right-[10mm] text-[11px] font-medium text-gray-300 no-print tracking-wide">
        @CVBuilder
      </div>
    </main>

    <!-- VIỀN CUỐI TRANG CỐ ĐỊNH -->
    <template v-for="p in pageCount" :key="'footer-border-' + p">
      <div
        class="absolute left-0 w-full flex items-center z-40 pointer-events-none"
        :style="{ top: `calc(${p * 297}mm - 12mm)`, height: '1.5px', paddingLeft: '12mm', paddingRight: '12mm' }"
      >
        <div class="w-full h-full opacity-0"></div>
      </div>
    </template>

    <!-- ĐƯỜNG PHÂN TRANG CHO PDF & TRÌNH DUYỆT -->
    <template v-for="p in (pageCount - 1)" :key="'div-' + p">
      <div
        class="absolute left-0 w-full z-50 flex flex-col items-center justify-center pointer-events-none no-print"
        :style="{ top: `calc(${p * 297}mm - 8px)` }"
      >
        <div class="w-[105%] h-[16px] bg-slate-800/90 shadow-inner overflow-hidden border-y border-black/30 backdrop-blur-sm"></div>
        <span class="absolute text-[9px] uppercase font-bold text-slate-300 tracking-widest bg-slate-700 px-3 py-0.5 rounded border border-slate-600 shadow-md">Ngắt trang {{ p + 1 }}</span>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, ref, onMounted, nextTick, watch, onUnmounted } from 'vue'
import draggable from 'vuedraggable'

const cvRoot = ref(null)
const pageCount = ref(1)
const selectedSectionId = ref(null)

const props = defineProps({
  resumeData: { type: Object, required: true }
})
const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem'])

// ─── COLOR PALETTE THEME SYSTEM ───
const hexToRgb = (hex) => {
  const c = hex.replace('#', '')
  if (c.length === 3) {
    return {
      r: parseInt(c[0] + c[0], 16),
      g: parseInt(c[1] + c[1], 16),
      b: parseInt(c[2] + c[2], 16)
    }
  }
  return {
    r: parseInt(c.substring(0, 2), 16),
    g: parseInt(c.substring(2, 4), 16),
    b: parseInt(c.substring(4, 6), 16)
  }
}

const rgbToHex = (r, g, b) => {
  const toHex = (n) => {
    const h = Math.max(0, Math.min(255, Math.round(n))).toString(16)
    return h.length === 1 ? '0' + h : h
  }
  return `#${toHex(r)}${toHex(g)}${toHex(b)}`
}

const adjustColorBrightness = (hex, percent) => {
  try {
    if (!hex || typeof hex !== 'string' || !hex.startsWith('#')) {
      return hex || ''
    }
    const { r, g, b } = hexToRgb(hex)
    if (isNaN(r) || isNaN(g) || isNaN(b)) return hex
    const newR = percent > 0 ? r + (255 - r) * percent : r * (1 + percent)
    const newG = percent > 0 ? g + (255 - g) * percent : g * (1 + percent)
    const newB = percent > 0 ? b + (255 - b) * percent : b * (1 + percent)
    return rgbToHex(newR, newG, newB)
  } catch (e) {
    return hex
  }
}

const templatePrimaryColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  if (!c || c.toLowerCase() === '#2b5c8f') return '#3d2e2c'
  return c
})

const templateCircleColor = computed(() => {
  return adjustColorBrightness(templatePrimaryColor.value, 0.10)
})

const templatePillColor = computed(() => {
  return adjustColorBrightness(templatePrimaryColor.value, 0.20)
})

const templateBorderColor = computed(() => {
  return adjustColorBrightness(templatePrimaryColor.value, 0.30)
})

const templateSecondaryColor = computed(() => {
  return adjustColorBrightness(templatePrimaryColor.value, 0.70)
})

const templateLightTextColor = computed(() => {
  return adjustColorBrightness(templatePrimaryColor.value, 0.90)
})

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

// ─── CONTACT ITEMS ───
const contactIcons = {
  phone: '<path d="M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z"/>',
  birthDate: '<path d="M19 4h-1V2h-2v2H8V2H6v2H5c-1.11 0-1.99.9-1.99 2L3 20a2 2 0 0 0 2 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H5V10h14v10zm0-12H5V6h14v2z"/>',
  email: '<path d="M20 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z"/>',
  facebook: '<path d="M22.675 0H1.325C.593 0 0 .593 0 1.325v21.351C0 23.407.593 24 1.325 24H12.82v-9.294H9.692v-3.622h3.128V8.413c0-3.1 1.893-4.788 4.659-4.788 1.325 0 2.463.099 2.795.143v3.24l-1.918.001c-1.504 0-1.795.715-1.795 1.763v2.313h3.587l-.467 3.622h-3.12V24h6.116c.73 0 1.323-.593 1.323-1.325V1.325C24 .593 23.407 0 22.675 0z"/>',
  address: '<path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5a2.5 2.5 0 0 1 0-5 2.5 2.5 0 0 1 0 5z"/>'
}

const contactOrder = ref(['phone', 'birthDate', 'email', 'facebook', 'address'])
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
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && !isEmpty(getContactValue(k)))
  if (idx <= 0) return
  const keyA = visible[idx], keyB = visible[idx - 1]
  const idxA = contactOrder.value.indexOf(keyA), idxB = contactOrder.value.indexOf(keyB)
  const arr = [...contactOrder.value]
  ;[arr[idxA], arr[idxB]] = [arr[idxB], arr[idxA]]
  contactOrder.value = arr
}

const moveContactDown = (idx) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && !isEmpty(getContactValue(k)))
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

const sectionHasContent = (section) => {
  if (section.id === 'summary') return true;
  if (!section) return false
  return section.items && section.items.length > 0
}

const formatDesc = (text) => {
  if (!text) return ''
  
  if (!/<[a-z][\s\S]*>/i.test(text)) {
     return text.split('\n')
                .map(l => l.trim())
                .filter(Boolean)
                .map(l => `<div class="paginated-item min-h-[14px]">${l}</div>`)
                .join('')
  }

  const tempDiv = document.createElement('div')
  tempDiv.innerHTML = text
  
  tempDiv.querySelectorAll('li').forEach(li => {
    const child = li.firstElementChild
    if (child && (child.tagName === 'FONT' || child.tagName === 'SPAN')) {
      if (child.color) li.style.color = child.color
      if (child.style && child.style.color) li.style.color = child.style.color
    }
  })

  const container = document.createElement('div')
  Array.from(tempDiv.childNodes).forEach(node => {
    if (node.nodeType === Node.TEXT_NODE) {
      if (node.textContent.trim()) {
        const div = document.createElement('div')
        div.className = 'paginated-item min-h-[14px]'
        div.appendChild(node.cloneNode(true))
        container.appendChild(div)
      }
    } else if (node.nodeType === Node.ELEMENT_NODE) {
      if (node.tagName === 'BR') {
        const div = document.createElement('div')
        div.className = 'paginated-item h-[14px]'
        container.appendChild(div)
      } else if (['UL', 'OL'].includes(node.tagName)) {
        Array.from(node.children).forEach(li => li.classList.add('paginated-item'))
        container.appendChild(node.cloneNode(true))
      } else {
        node.classList.add('paginated-item')
        container.appendChild(node.cloneNode(true))
      }
    }
  })

  return container.innerHTML
}

const sidebarSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'left')
)

const mainSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'right')
)

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

// ─── PAGINATION ENGINE ───
const A4_W_MM   = 210
const A4_H_MM   = 297

let paginateTimer = null
const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer)
  paginateTimer = setTimeout(doPagination, 60)
}

const doPagination = async () => {
  if (!cvRoot.value) return

  const allElements = cvRoot.value.querySelectorAll('.paginated-item')
  allElements.forEach(el => { el.style.marginTop = '0px' })
  await nextTick()

  const pxPerMm = cvRoot.value.offsetWidth / A4_W_MM
  const pageH = A4_H_MM * pxPerMm
  
  const bottomSafeZone = 14 * pxPerMm 
  const topMargin = 8 * pxPerMm 

  const getOffsetTop = (el) => {
    let offset = 0
    let curr = el
    while (curr && curr !== cvRoot.value) {
      offset += curr.offsetTop
      curr = curr.offsetParent
    }
    return offset
  }

  allElements.forEach((el) => {
    if(el.offsetHeight === 0) return
    const top = getOffsetTop(el)
    const topInPage = top % pageH
    const bottomInPage = topInPage + el.offsetHeight
    
    if (bottomInPage > (pageH - bottomSafeZone)) {
       const distToNextPage = pageH - topInPage + topMargin
       el.style.marginTop = `${distToNextPage}px`
    }
  })

  let maxBottom = 0
  allElements.forEach(el => {
    const b = getOffsetTop(el) + el.offsetHeight
    if (b > maxBottom) maxBottom = b
  })
  pageCount.value = Math.max(1, Math.ceil(maxBottom / pageH))
}

watch(() => props.resumeData, requestPagination, { deep: true })

onMounted(() => {
  const LEFT_IDS = ['education', 'skills', 'hobbies'];
  const RIGHT_IDS = ['summary', 'experience', 'awards', 'certifications'];
  const defaultVisible = [...LEFT_IDS, ...RIGHT_IDS];

  if (props.resumeData?.sections) {
    props.resumeData.sections.forEach(sec => {
      if (sec.isVisible === undefined) {
        sec.isVisible = defaultVisible.includes(sec.id);
      }

      if (LEFT_IDS.includes(sec.id)) {
        sec.column = 'left';
      } else if (RIGHT_IDS.includes(sec.id)) {
        sec.column = 'right';
      } else {
        sec.column = 'right';
      }
    });
  }

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
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800;900&display=swap');

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

.section-block {
  position: relative;
  border-radius: 6px;
  border: 2px solid transparent;
  cursor: pointer;
  transition: border-color 0.18s ease, box-shadow 0.18s ease, background-color 0.18s ease;
}

.section-block.section-active {
  border-radius: 6px !important;
  border: 2px solid var(--active-bg, v-bind(templatePillColor)) !important;
  box-shadow: 0 4px 18px rgba(0,0,0,0.12);
  z-index: 30;
}

/* Nav Control Buttons */
.nav-btns {
  position: absolute;
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
  box-shadow: 0 2px 6px rgba(37, 99, 235, 0.4);
  transition: background 0.15s, transform 0.15s;
}
.nav-btn:hover { background: #1d4ed8; transform: scale(1.1); }
.nav-btn:active { transform: scale(0.95); }

.nav-btn-danger {
  background: #ef4444 !important;
  box-shadow: 0 2px 6px rgba(239, 68, 68, 0.4) !important;
}
.nav-btn-danger:hover {
  background: #dc2626 !important;
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

/* Delete Buttons */
.delete-item-btn {
  position: absolute;
  right: 0;
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
  box-shadow: 0 1px 4px rgba(0,0,0,0.2);
  transition: transform 0.15s;
  z-index: 35;
}
.delete-item-btn:hover { transform: scale(1.15); background: #dc2626; }
.delete-item-btn--lg {
  width: 20px;
  height: 20px;
  right: -10px;
  top: -5px;
}

.item-container {
  position: relative;
}

.paginated-item {
  transition: none; 
}

main {
  height: auto !important;
  min-height: 100%;
}

.fade-btns-enter-active,
.fade-btns-leave-active {
  transition: opacity 0.15s ease, transform 0.15s ease;
}
.fade-btns-enter-from,
.fade-btns-leave-to {
  opacity: 0;
  transform: scale(0.85);
}

:deep(.html-content ul) {
  list-style-type: disc !important;
  padding-left: 1rem !important;
  margin-top: 0.35rem;
  margin-bottom: 0.35rem;
}
:deep(.html-content ol) {
  list-style-type: decimal !important;
  padding-left: 1rem !important;
  margin-top: 0.35rem;
  margin-bottom: 0.35rem;
}
:deep(.html-content b),
:deep(.html-content strong) { font-weight: 700 !important; }
:deep(.html-content i),
:deep(.html-content em) { font-style: italic !important; }
:deep(.html-content u) { text-decoration: underline !important; }
:deep(.html-content ul li),
:deep(.html-content ol li) { margin-bottom: 0.25rem; }

@media print {
  .no-print { display: none !important; }
  .section-block,
  .section-block.section-active {
    cursor: default !important;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
    transform: none !important;
    border-radius: 0 !important;
    padding: 0 !important;
    margin: 0 !important;
  }
}

:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .section-block.section-active) {
  cursor: default !important;
  box-shadow: none !important;
  background: transparent !important;
  border-color: transparent !important;
  transform: none !important;
}
</style>
