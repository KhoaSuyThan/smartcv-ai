<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-col relative box-border text-[#333] leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif' }"
    @click.self="selectedSectionId = null"
  >
    <!-- MÀU NỀN TRẮNG -->
    <div class="absolute inset-0 bg-white z-0 pointer-events-none"></div>

    <!-- HEADER BLOCK -->
    <header class="paginated-item w-full h-[70mm] flex relative overflow-hidden shrink-0 z-10 bg-white">
      <!-- Cột trái của Header: Màu vàng mù tạt bao quanh Avatar tròn -->
      <div class="w-[75mm] h-full bg-[#D49A17] relative flex items-center justify-center rounded-br-[40px] z-20 shrink-0">
        <!-- 3 chấm nhỏ trang trí dốc đứng ở góc trái trên -->
        <div class="absolute top-5 left-5 flex flex-col gap-1.5 opacity-45 z-30">
          <div class="w-1.5 h-1.5 rounded-full bg-white"></div>
          <div class="w-1.5 h-1.5 rounded-full bg-white"></div>
          <div class="w-1.5 h-1.5 rounded-full bg-white"></div>
        </div>

        <!-- Avatar hình tròn viền trắng nổi bật -->
        <div class="relative w-[48mm] h-[48mm] rounded-full overflow-hidden bg-white shadow-lg border-4 border-white flex items-center justify-center z-30">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center bg-gray-200 text-gray-400">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
          </div>
        </div>
      </div>

      <!-- Cột phải của Header: Màu đỏ sẫm chứa Họ tên & Vị trí ứng tuyển -->
      <div class="flex-1 h-full bg-[#901A1E] text-white flex flex-col justify-center pl-[12mm] pr-[15mm] relative z-10">
        <!-- Họa tiết vòng tròn đồng tâm mờ mỏng ở góc trên bên phải -->
        <div class="absolute -top-12 -right-12 w-[65mm] h-[65mm] text-white/10 pointer-events-none z-0">
          <svg class="w-full h-full" viewBox="0 0 100 100">
            <circle cx="50" cy="50" r="45" fill="none" stroke="currentColor" stroke-width="1.2" />
            <circle cx="50" cy="50" r="35" fill="none" stroke="currentColor" stroke-width="1.2" />
            <circle cx="50" cy="50" r="25" fill="none" stroke="currentColor" stroke-width="1.2" />
            <circle cx="50" cy="50" r="15" fill="none" stroke="currentColor" stroke-width="1.2" />
          </svg>
        </div>

        <!-- Họ và tên (20px, in hoa, màu trắng, đậm) -->
        <h1 
          class="font-black uppercase tracking-wider mb-2 leading-tight text-white z-10" 
          style="font-size: 20px !important; text-shadow: 0px 2px 4px rgba(0,0,0,0.15);" 
          v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'Họ và Tên'"
        ></h1>

        <!-- Vị trí ứng tuyển (16px, màu trắng) -->
        <h2 
          class="font-bold uppercase tracking-wide text-white/90 z-10" 
          style="font-size: 16px !important" 
          v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'Vị trí ứng tuyển'"
        ></h2>

        <!-- Dải cong lượn lượn sóng màu trắng dưới đáy cột phải -->
        <div class="absolute bottom-0 left-0 right-0 h-[25mm] pointer-events-none overflow-hidden z-20">
          <svg class="w-full h-full text-white fill-current" viewBox="0 0 1000 100" preserveAspectRatio="none">
            <path d="M0,40 Q150,110 500,60 T1000,20 L1000,100 L0,100 Z" />
          </svg>
        </div>
      </div>
    </header>

    <!-- CONTENT BODY - 2 COLUMNS -->
    <div class="flex-1 flex flex-row relative z-10" @click.self="selectedSectionId = null">
      
      <!-- LEFT COLUMN (Sidebar - rộng 75mm) -->
      <aside class="w-[75mm] shrink-0 bg-white flex flex-col pt-6 px-[8mm] pb-8 gap-6 relative" @click.self="selectedSectionId = null">
        <!-- THÔNG TIN LIÊN HỆ (Dynamic) -->
        <div
          v-if="contactItems.length > 0"
          class="section-block px-3 py-2.5 rounded-lg border border-transparent transition-all relative contact-block cursor-pointer hover:bg-black/5 transition-colors"
          :class="{ 'contact-active': selectedSectionId === 'contact' }"
          @click.stop="toggleSection('contact')"
        >
          <h3 class="font-black uppercase tracking-wider mb-4 border-b border-gray-100 pb-1.5" style="font-size: 16px !important; color: #901A1E !important; font-weight: 900 !important;">
            THÔNG TIN LIÊN HỆ
          </h3>
          <div class="space-y-3.5 text-gray-700 font-medium">
            <div v-for="(ci, ciIdx) in contactItems" :key="ci.key"
                class="flex items-center gap-3 relative contact-item-container">
              <div v-html="ci.icon" class="w-5 h-5 text-[#901A1E] shrink-0 flex items-center justify-center"></div>
              <span class="break-words leading-tight" style="font-size: 16px !important;">
                <strong class="font-black text-gray-900">{{ ci.label }}:</strong> <span v-html="ci.value"></span>
              </span>

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
          </div>
        </div>

        <!-- DYNAMIC SIDEBAR SECTIONS -->
        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-if="section.isVisible"
            :data-section-id="section.id" class="section-block relative group px-3 py-2.5 rounded-lg border border-transparent transition-all cursor-pointer hover:bg-black/5 transition-colors"
            :class="{ 'section-active': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--active-bg': '#901A1E' } : {}"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print" style="right: 10px;" @click.stop>
                <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên"><svg class="pointer-events-none" width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống"><svg class="pointer-events-none" width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải"><svg class="pointer-events-none" width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg></button>
                <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn mục này"><svg class="pointer-events-none" width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg></button>
              </div>
            </transition>

            <div class="paginated-item">
              <h3 class="font-black uppercase tracking-wider mb-4 border-b border-gray-100 pb-1.5" style="font-size: 16px !important; color: #901A1E !important; font-weight: 900 !important;">
                {{ section.title }}
              </h3>
            </div>

            <div class="space-y-3.5" v-if="sectionHasContent(section)">
              <!-- MỤC TIÊU NGHỀ NGHIỆP -->
              <div v-if="section.id.toLowerCase() === 'summary'" class="item-container relative text-gray-700 leading-relaxed text-justify">
                <div 
                  class="html-content text-[16px]" 
                  style="font-size: 16px !important;"
                  v-html="formatDesc(resumeData.general.summary)"
                ></div>
              </div>

              <!-- KỸ NĂNG -->
              <div v-else-if="section.id.toLowerCase() === 'skills'" class="space-y-3">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="paginated-item item-container flex items-start gap-2.5 text-gray-700"
                >
                  <svg class="w-4.5 h-4.5 text-[#333] shrink-0 mt-[3px]" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="3">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
                  </svg>
                  <span class="leading-tight" style="font-size: 16px !important;">{{ item.name }}</span>

                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: -5px; right: -5px;">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- GIẢI THƯỜNG -->
              <div v-else-if="section.id.toLowerCase() === 'awards'" class="space-y-3">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="paginated-item item-container text-gray-700 relative"
                >
                  <div class="font-bold text-gray-900 leading-snug" style="font-size: 16px !important;">{{ item.name }}</div>
                  <div class="text-sm text-gray-500 italic mt-0.5" v-if="item.year">{{ item.year }}</div>
                  
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: -5px; right: -5px;">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- HỌC VẤN -->
              <div v-else-if="section.id.toLowerCase() === 'education'" class="space-y-4">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative text-gray-700 leading-relaxed">
                  <div class="font-bold text-gray-900" style="font-size: 16px !important;" v-if="item.school">
                    <strong class="font-black text-gray-900">Trường học:</strong> {{ item.school }}
                  </div>
                  <div v-if="item.major">
                    <strong class="font-black text-gray-900">Chuyên ngành:</strong> {{ item.major }}
                  </div>
                  <div class="text-gray-500 font-medium" style="font-size: 16px !important;" v-if="item.year">
                    <strong class="font-black text-gray-900">Năm học:</strong> {{ item.year }}
                  </div>
                  <div v-if="item.gradType">
                    <strong class="font-black text-gray-900">Xếp loại:</strong> {{ item.gradType }}
                  </div>
                  <div v-if="item.desc" class="html-content text-[16px] leading-relaxed mt-1" style="font-size: 16px !important;" v-html="formatDesc(item.desc)"></div>

                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: -5px; right: -5px;">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- KINH NGHIỆM LÀM VIỆC -->
              <div v-else-if="section.id.toLowerCase() === 'experience'" class="space-y-4">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative text-gray-700 leading-relaxed">
                  <div v-if="item.role">
                    <strong class="font-black text-gray-900">Vị trí:</strong> {{ item.role }}
                  </div>
                  <div class="font-bold text-[#901A1E] italic" style="font-size: 16px !important;" v-if="item.company">
                    <strong class="font-black text-gray-900">Công ty:</strong> {{ item.company }}
                  </div>
                  <div class="text-gray-500 font-medium" style="font-size: 16px !important;" v-if="item.time">
                    <strong class="font-black text-gray-900">Thời gian:</strong> {{ item.time }}
                  </div>
                  <div class="font-bold mt-1 text-gray-900" style="font-size: 16px !important;" v-if="item.desc">
                    <strong class="font-black text-gray-900">Mô tả:</strong>
                  </div>
                  <div class="html-content text-[16px] leading-relaxed mt-1" style="font-size: 16px !important;" v-html="formatDesc(item.desc)"></div>

                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: -5px; right: -5px;">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- OTHER SIDEBAR ITEMS -->
              <div v-else class="space-y-3">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="item-container relative text-gray-700"
                >
                  <div class="paginated-item font-bold text-gray-900 mb-1" style="font-size: 16px !important;" v-if="item.name || item.title">{{ item.name || item.title }}</div>
                  <div class="paginated-item font-medium text-gray-400 italic mb-1" style="font-size: 14px !important;" v-if="item.time || item.year">{{ item.time || item.year }}</div>
                  <div class="text-[16px] leading-[1.65] html-content" style="font-size: 16px !important;" v-html="formatDesc(item.desc || item.info)"></div>
                  
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: -5px; right: -5px;">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>
            </div>
          </div>
        </template>
      </aside>

      <!-- RIGHT COLUMN (Main Content - rộng 135mm) -->
      <main class="flex-1 bg-white flex flex-col pt-6 px-[10mm] pb-8 gap-6 relative" @click.self="selectedSectionId = null">
        <!-- Đường chia dọc mờ giữa 2 cột -->
        <div class="absolute left-0 top-0 bottom-0 w-px bg-slate-200"></div>

        <!-- DYNAMIC MAIN SECTIONS -->
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-if="section.isVisible"
            :data-section-id="section.id" class="section-block relative group px-4 py-2.5 rounded-lg border border-transparent transition-all cursor-pointer hover:bg-black/5 transition-colors"
            :class="{ 'section-active': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--active-bg': '#901A1E' } : {}"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print" @click.stop>
                <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên"><svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg></button>
                <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống"><svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg></button>
                <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái"><svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
                <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn mục này"><svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg></button>
              </div>
            </transition>

            <div class="paginated-item">
              <h3 class="font-black uppercase tracking-wider mb-4 border-b border-gray-100 pb-1.5" style="font-size: 16px !important; color: #901A1E !important; font-weight: 900 !important;">
                {{ section.title }}
              </h3>
            </div>

            <div class="space-y-6" v-if="sectionHasContent(section)">
              <!-- MỤC TIÊU NGHỀ NGHIỆP -->
              <div v-if="section.id.toLowerCase() === 'summary'" class="item-container relative text-gray-700 leading-relaxed text-justify">
                <div 
                  class="html-content text-[16px]" 
                  style="font-size: 16px !important;"
                  v-html="formatDesc(resumeData.general.summary)"
                ></div>
              </div>

              <!-- KỸ NĂNG -->
              <div v-else-if="section.id.toLowerCase() === 'skills'" class="space-y-3">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="paginated-item item-container flex items-start gap-2.5 text-gray-700"
                >
                  <svg class="w-4.5 h-4.5 text-[#333] shrink-0 mt-[3px]" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="3">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
                  </svg>
                  <span class="leading-tight" style="font-size: 16px !important;">{{ item.name }}</span>

                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- GIẢI THƯỜNG -->
              <div v-else-if="section.id.toLowerCase() === 'awards'" class="space-y-3">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="paginated-item item-container text-gray-700 relative"
                >
                  <div class="font-bold text-gray-900 leading-snug" style="font-size: 16px !important;">{{ item.name }}</div>
                  <div class="text-sm text-gray-500 italic mt-0.5" v-if="item.year">{{ item.year }}</div>
                  
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- HỌC VẤN -->
              <div v-else-if="section.id.toLowerCase() === 'education'" class="space-y-5">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative text-gray-700 leading-relaxed">
                  <div class="font-bold text-gray-900" style="font-size: 16px !important;" v-if="item.school">
                    <strong class="font-black text-gray-900">Trường học:</strong> {{ item.school }}
                  </div>
                  <div v-if="item.major">
                    <strong class="font-black text-gray-900">Chuyên ngành:</strong> {{ item.major }}
                  </div>
                  <div class="text-gray-500 font-medium" style="font-size: 16px !important;" v-if="item.year">
                    <strong class="font-black text-gray-900">Năm học:</strong> {{ item.year }}
                  </div>
                  <div v-if="item.gradType">
                    <strong class="font-black text-gray-900">Xếp loại:</strong> {{ item.gradType }}
                  </div>
                  <div v-if="item.desc" class="html-content text-[16px] leading-relaxed mt-1" style="font-size: 16px !important;" v-html="formatDesc(item.desc)"></div>

                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- KINH NGHIỆM LÀM VIỆC -->
              <div v-else-if="section.id.toLowerCase() === 'experience'" class="space-y-6">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative text-gray-700 leading-relaxed">
                  <div v-if="item.role">
                    <strong class="font-black text-gray-900">Vị trí:</strong> {{ item.role }}
                  </div>
                  <div class="font-bold text-[#901A1E] italic" style="font-size: 16px !important;" v-if="item.company">
                    <strong class="font-black text-gray-900">Công ty:</strong> {{ item.company }}
                  </div>
                  <div class="text-gray-500 font-medium" style="font-size: 16px !important;" v-if="item.time">
                    <strong class="font-black text-gray-900">Thời gian:</strong> {{ item.time }}
                  </div>
                  <div class="font-bold mt-1 text-gray-900" style="font-size: 16px !important;" v-if="item.desc">
                    <strong class="font-black text-gray-900">Mô tả:</strong>
                  </div>
                  <div class="html-content text-[16px] leading-relaxed mt-1" style="font-size: 16px !important;" v-html="formatDesc(item.desc)"></div>

                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- OTHER MAIN ABSTRACTIONS -->
              <div v-else class="space-y-5">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative text-gray-700 leading-relaxed">
                  <div class="paginated-item font-bold text-gray-900" style="font-size: 16px !important;" v-if="item.name || item.title || item.company || item.school">{{ item.name || item.title || item.company || item.school }}</div>
                  <div class="paginated-item font-medium text-gray-500 italic" style="font-size: 14px !important;" v-if="item.time || item.year">{{ item.time || item.year }}</div>
                  <div class="html-content text-[16px] leading-relaxed mt-1" style="font-size: 16px !important;" v-html="formatDesc(item.desc || item.info)"></div>

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
      </main>
    </div>

    <!-- WATERMARK FOOTER -->
    <div class="absolute bottom-[6mm] right-[10mm] text-[12px] font-medium text-gray-400 no-print" style="letter-spacing: 0.05em;">
      @CVBuilder
    </div>

    <!-- VIỀN CUỐI TRANG CỐ ĐỊNH -->
    <template v-for="p in pageCount" :key="'footer-border-' + p">
      <div
        class="absolute left-0 w-full flex items-center z-40 pointer-events-none"
        :style="{ top: `calc(${p * 297}mm - 12mm)`, height: '1.5px', paddingLeft: '12mm', paddingRight: '12mm' }"
      >
        <div class="w-full h-full opacity-0"></div>
      </div>
    </template>

    <!-- ĐƯỜNG PHÂN TRANG CHO IN ẤN & XUẤT PDF (TRỰC QUAN TRÊN TRÌNH DUYỆT) -->
    <template v-for="p in (pageCount - 1)" :key="'div-' + p">
      <div
        class="absolute left-0 w-full z-50 flex flex-col items-center justify-center pointer-events-none no-print"
        :style="{ top: `calc(${p * 297}mm - 8px)` }"
      >
        <div class="w-[105%] h-[16px] bg-gray-800/90 shadow-inner overflow-hidden border-y border-black/30 backdrop-blur-sm"></div>
        <span class="absolute text-[9px] uppercase font-bold text-gray-300 tracking-widest bg-gray-700 px-3 py-0.5 rounded border border-gray-600 shadow-md">Ngắt trang {{ p + 1 }}</span>
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
  if (section.id === 'summary') return true
  if (!section) return false
  return section.items && section.items.length > 0
}

const formatDesc = (text) => {
  if (!text) return ''
  
  if (!/<[a-z][\s\S]*>/i.test(text)) {
     return text.split('\n')
                .map(l => l.trim())
                .filter(Boolean)
                .map(l => `<div class="paginated-item" style="font-size: 16px !important;">${l}</div>`)
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
        div.className = 'paginated-item min-h-[16px]'
        div.style.fontSize = '16px'
        div.appendChild(node.cloneNode(true))
        container.appendChild(div)
      }
    } else if (node.nodeType === Node.ELEMENT_NODE) {
      if (node.tagName === 'BR') {
        const div = document.createElement('div')
        div.className = 'paginated-item h-[16px]'
        container.appendChild(div)
      } else if (['UL', 'OL'].includes(node.tagName)) {
        Array.from(node.children).forEach(li => {
          li.classList.add('paginated-item')
          li.style.fontSize = '16px'
        })
        container.appendChild(node.cloneNode(true))
      } else {
        node.classList.add('paginated-item')
        node.style.fontSize = '16px'
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
const sidebarIds = computed(() => sidebarSections.value.map(s => s.id))
const mainIds = computed(() => mainSections.value.map(s => s.id))

// --- CONTACT ITEMS: Danh sách động có thể sắp xếp / ẩn ---
const contactIcons = {
  birthDate: { label: 'Ngày sinh', icon: '<svg fill="currentColor" viewBox="0 0 24 24"><path d="M19 4h-1V2h-2v2H8V2H6v2H5c-1.11 0-1.99.9-1.99 2L3 20a2 2 0 0 0 2 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H5V10h14v10zm0-12H5V6h14v2z"/></svg>' },
  gender: { label: 'Giới tính', icon: '<svg fill="currentColor" viewBox="0 0 24 24"><path d="M12 2c1.1 0 2 .9 2 2s-.9 2-2 2-2-.9-2-2 .9-2 2-2zm9 7h-6v13h-2v-6h-2v6H9V9H3V7h18v2z"/></svg>' },
  phone: { label: 'SĐT', icon: '<svg fill="currentColor" viewBox="0 0 24 24"><path d="M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z"/></svg>' },
  email: { label: 'Email', icon: '<svg fill="currentColor" viewBox="0 0 24 24"><path d="M20 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z"/></svg>' },
  address: { label: 'Địa chỉ', icon: '<svg fill="currentColor" viewBox="0 0 24 24"><path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5a2.5 2.5 0 0 1 0-5 2.5 2.5 0 0 1 0 5z"/></svg>' }
}

const contactOrder = ref(['birthDate', 'gender', 'phone', 'email', 'address'])
const hiddenContacts = ref([])

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  switch (key) {
    case 'birthDate': return isEmpty(g.birthDate) ? '' : g.birthDate
    case 'gender': return isEmpty(g.gender) ? '' : g.gender
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
      label: contactIcons[key].label,
      icon: contactIcons[key].icon,
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
  // Cưỡng chế chỉ active 5 mục có trong ảnh, ẩn các mục khác và phân bố đúng cột
  const activeLeft = ['summary', 'skills', 'awards']
  const activeRight = ['education', 'experience']

  if (props.resumeData && props.resumeData.sections) {
    props.resumeData.sections.forEach(sec => {
      const id = sec.id.toLowerCase()
      if (sec.isVisible === undefined) {
        if (activeLeft.includes(id)) {
          sec.isVisible = true
          sec.column = 'left'
        } else if (activeRight.includes(id)) {
          sec.isVisible = true
          sec.column = 'right'
        } else {
          sec.isVisible = false
        }
      } else {
        if (activeLeft.includes(id)) {
          sec.column = 'left'
        } else if (activeRight.includes(id)) {
          sec.column = 'right'
        }
      }
    })
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
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap');

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

.section-block {
  position: relative;
  border-radius: 0px !important;
  border: 2px solid transparent !important;
  background: transparent !important;
  box-shadow: none !important;
  cursor: pointer;
  transition: box-shadow 0.18s ease, border-color 0.18s ease;
}

.section-block.section-active {
  transform: none !important;
  border-radius: 6px !important;
  border: 2px solid var(--active-bg, #901A1E) !important;
  box-shadow: 0 4px 18px rgba(0,0,0,0.1) !important;
  z-index: 10;
}

.contact-block {
  border: 2px solid transparent !important;
  cursor: pointer;
  transition: all 0.2s;
}
.contact-block.contact-active {
  border: 2px solid #901A1E !important;
  border-radius: 8px !important;
  background-color: rgba(144, 26, 30, 0.05) !important;
  box-shadow: 0 4px 18px rgba(0,0,0,0.1) !important;
  z-index: 10;
}
.contact-item-container { position: relative; }
.contact-item-btns {
  position: absolute;
  right: -5px;
  top: 50%;
  transform: translateY(-50%);
  display: flex;
  gap: 3px;
  z-index: 100;
}

.nav-btns {
  position: absolute;
  right: 10px;
  top: 6px;
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
.nav-btn-danger { background: #ef4444 !important; }
.nav-btn-danger:hover { background: #dc2626 !important; }

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
  z-index: 30;
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
  padding-left: 1.25rem !important;
  margin-top: 0.35rem;
  margin-bottom: 0.35rem;
}
:deep(.html-content ol) {
  list-style-type: decimal !important;
  padding-left: 1.25rem !important;
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
