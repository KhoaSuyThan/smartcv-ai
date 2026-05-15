<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-col relative box-border text-[#4A352F] leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif' }"
    @click.self="selectedSectionId = null"
  >
    <!-- BACKGROUND CHUNG -->
    <div class="absolute inset-0 bg-[#FCF8F6] z-0 pointer-events-none"></div>

    <!-- MAIN FLOW -->
    <main class="flex-1 flex flex-col relative z-20 min-h-max" @click.self="selectedSectionId = null">
      
      <!-- 1. HEADER THÔNG TIN LIÊN HỆ NẰM NGANG (Dynamic) -->
      <header 
        class="section-block paginated-item !mt-[20px] !mx-[20px] bg-[#E9DCD6] px-[10mm] py-[3.5mm] flex justify-between items-center text-[11px] font-medium text-[#4A352F]/90 shrink-0 border border-[#DCD0C9] rounded-[4px] relative"
        :class="{ 'section-active': selectedSectionId === 'contact' }"
        @click.stop="toggleSection('contact')"
      >
        <div class="flex flex-wrap justify-between items-center w-full gap-4">
          <div 
            v-for="(ci, ciIdx) in contactItems" 
            :key="ci.key" 
            class="flex items-center gap-2 relative group/item"
          >
            <div class="w-[18px] h-[18px] flex items-center justify-center text-[#D03B29]" v-html="ci.icon"></div>
            <span class="break-all" v-html="ci.value"></span>

            <!-- Individual contact item buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print">
                <button @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn nav-btn--xs" title="Sang trái"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg></button>
                <button @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn nav-btn--xs" title="Sang phải"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg></button>
                <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn--xs nav-btn-danger" title="Ẩn"><svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg></button>
              </div>
            </transition>
          </div>
        </div>
      </header>

      <!-- 2. KHỐI TÊN, SUMMARY VÀ AVATAR -->
      <section class="paginated-item px-[10mm] pt-[6mm] pb-[4mm] flex gap-[6mm] items-stretch shrink-0">
        <!-- Khung Tên & Summary (Trái) -->
        <div class="flex-1 bg-[#F5ECE8] p-[6mm] rounded-[4px] border border-[#E9DDD7] flex flex-col justify-center">
          <h1 class="!text-[20px] font-extrabold uppercase tracking-tight text-[#4A352F] leading-none mb-2" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'HỌ VÀ TÊN'"></h1>
          
          <!-- Vị trí ứng tuyển kèm đường kẻ ngang -->
          <div class="flex items-center gap-4 mb-4">
            <h2 class="!text-[11px] font-extrabold text-[#4A352F] uppercase tracking-wider shrink-0" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'IT INTERNSHIP'"></h2>
            <div class="h-[2px] bg-[#D03B29] flex-1 max-w-[120px]"></div>
          </div>

          <!-- Đoạn Summary (Mục tiêu nghề nghiệp) -->
          <div 
            class="section-block border border-transparent rounded cursor-pointer relative group !p-1.5"
            :class="{ 'section-active': selectedSectionId === 'summary' }"
            :style="selectedSectionId === 'summary' ? { '--active-bg': '#4A352F' } : {}"
            @click.stop="toggleSection('summary')"
          >
            <div class="text-[11.5px] leading-[1.65] text-[#4A352F]/90 text-justify font-medium" v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Mục tiêu nghề nghiệp của bạn...'"></div>
          </div>
        </div>

        <!-- Khung Avatar (Phải) -->
        <div class="w-[44mm] flex-shrink-0 flex items-center justify-center">
          <div class="w-[44mm] h-[44mm] bg-gray-100 overflow-hidden relative shadow-sm border border-[#E9DDD7] avatar-clip">
            <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
            <div v-else class="w-full h-full flex items-center justify-center text-[#4A352F]/40">
              <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
            </div>
          </div>
        </div>
      </section>

      <!-- 3. DANH SÁCH CÁC SECTION BLOCK (EDUCATION, CERTIFICATIONS, PROJECTS, ACTIVITIES, SKILLS) -->
      <div class="px-[10mm] pb-[2mm] flex flex-col gap-[4mm]">
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-if="section.isVisible"
            class="section-block bg-[#F5ECE8] p-[5mm] rounded-[4px] border border-[#E9DDD7] relative group"
            :class="{ 'section-active': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--active-bg': '#4A352F' } : {}"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nút điều khiển Nav -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print" style="right: 12px; top: 12px;" @click.stop>
                <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên">
                  <svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                </button>
                <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống">
                  <svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                </button>
                <button @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                  <svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </transition>

            <!-- Tiêu đề Mục & Đường kẻ đỏ cam -->
            <div class="paginated-item">
              <div class="flex flex-col mb-3">
                <h3 class="section-title">
                  {{ section.title }}
                </h3>
                <div class="h-[1.2px] bg-[#D03B29] w-full mt-1"></div>
              </div>
            </div>

            <!-- Nội dung chi tiết từng loại section -->
            <div v-if="sectionHasContent(section)">
              
              <!-- A. EDUCATION -->
              <div v-if="section.id === 'education'" class="space-y-3">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container text-[11.5px] relative">
                  <div class="flex justify-between items-baseline mb-1">
                    <h4 class="font-bold text-[13px] text-[#4A352F] leading-tight flex-1">
                      {{ item.school }}
                    </h4>
                    <span class="font-extrabold text-[12px] text-[#4A352F]/70 shrink-0 ml-4">
                      {{ item.year }}
                    </span>
                  </div>
                  <div class="text-[12px] text-[#4A352F] font-bold italic mb-1.5">
                    {{ item.major }}
                  </div>
                  <!-- Web application, GPA và các thông tin mô tả khác -->
                  <div class="text-[11.5px] text-[#4A352F]/90 leading-[1.6] text-justify html-content" v-html="formatDesc(item.desc)"></div>
                  
                  <div v-if="item.gradType" class="text-[11.5px] text-[#4A352F]/95 mt-1">
                    GPA/Xếp loại: <span class="font-bold">{{ item.gradType }}</span>
                  </div>

                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- B. CERTIFICATIONS -->
              <div v-else-if="section.id === 'certifications' || section.id === 'cert'" class="space-y-3">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container text-[11.5px] relative">
                  <div class="flex justify-between items-baseline mb-0.5">
                    <h4 class="font-bold text-[12.5px] text-[#4A352F] leading-tight flex-1">
                      {{ item.name }}
                    </h4>
                    <span class="font-extrabold text-[11.5px] text-[#4A352F]/70 shrink-0 ml-4">
                      {{ item.year }}
                    </span>
                  </div>
                  <div class="text-[11.5px] text-[#4A352F]/90 leading-[1.6] html-content" v-html="formatDesc(item.info || item.desc)"></div>

                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- C. PROJECTS -->
              <div v-else-if="section.id === 'project' || section.id === 'projects'" class="relative pl-[20px]">
                <!-- Đường kẻ trục đứng của timeline -->
                <div class="absolute left-[3px] top-[4px] bottom-[4px] w-[1px] bg-[#DCD0C9]"></div>

                <div class="space-y-4">
                  <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container text-[11.5px] relative">
                    <!-- Chấm tròn mốc đỏ cam -->
                    <div class="absolute left-[-21px] top-[4px] w-[7px] h-[7px] rounded-full bg-[#D03B29] z-10 border border-[#FCF8F6]"></div>

                    <!-- Hàng đầu tiên: Thời gian (trái) và Vị trí (phải) -->
                    <div class="flex justify-between items-start mb-1 gap-4">
                      <div class="font-extrabold text-[11.5px] text-[#4A352F]/75 w-[110px] shrink-0">
                        {{ item.time || item.year }}
                      </div>
                      <div class="font-bold text-[13px] text-[#4A352F] text-right flex-1 leading-tight">
                        {{ item.role || 'Full-stack Web Developer' }}
                      </div>
                    </div>

                    <!-- Tên Dự án -->
                    <h5 class="font-extrabold text-[12px] text-[#4A352F] uppercase mb-1.5">
                      {{ item.name || item.title }}
                    </h5>

                    <!-- Mô tả và Trách nhiệm -->
                    <div class="text-[11.5px] text-[#4A352F]/90 leading-[1.65] text-justify html-content" v-html="formatDesc(item.desc)"></div>

                    <transition name="fade-btns">
                      <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print" style="right: -8px; top: -2px;">
                        <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                      </button>
                    </transition>
                  </div>
                </div>
              </div>

              <!-- D. ACTIVITIES -->
              <div v-else-if="section.id === 'activities'" class="relative pl-[20px]">
                <!-- Đường kẻ trục đứng của timeline -->
                <div class="absolute left-[3px] top-[4px] bottom-[4px] w-[1px] bg-[#DCD0C9]"></div>

                <div class="space-y-4">
                  <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container text-[11.5px] relative">
                    <!-- Chấm tròn mốc đỏ cam -->
                    <div class="absolute left-[-21px] top-[4px] w-[7px] h-[7px] rounded-full bg-[#D03B29] z-10 border border-[#FCF8F6]"></div>

                    <!-- Hàng đầu tiên: Thời gian (trái) và Chức danh (phải) -->
                    <div class="flex justify-between items-start mb-1 gap-4">
                      <div class="font-extrabold text-[11.5px] text-[#4A352F]/75 w-[110px] shrink-0">
                        {{ item.time || item.year }}
                      </div>
                      <div class="font-bold text-[13px] text-[#4A352F] text-right flex-1 leading-tight">
                        {{ item.role || 'Deputy Secretary' }}
                      </div>
                    </div>

                    <!-- Tên Đơn vị/Tổ chức -->
                    <h5 class="font-bold text-[12px] text-[#4A352F] mb-1.5">
                      {{ item.name || item.company }}
                    </h5>

                    <!-- Mô tả hoạt động -->
                    <div class="text-[11.5px] text-[#4A352F]/90 leading-[1.65] text-justify html-content" v-html="formatDesc(item.desc)"></div>

                    <transition name="fade-btns">
                      <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print" style="right: -8px; top: -2px;">
                        <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                      </button>
                    </transition>
                  </div>
                </div>
              </div>

              <!-- E. SKILLS -->
              <div v-else-if="section.id === 'skills'" class="space-y-3.5">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container text-[11.5px] relative">
                  <div class="font-bold text-[12.5px] text-[#4A352F] mb-1">
                    {{ item.name }}
                  </div>
                  <div class="text-[11.5px] text-[#4A352F]/90 leading-[1.6] html-content" v-html="formatDesc(item.level || item.info)"></div>

                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- CÁC MỤC KHÁC (FALLBACK) -->
              <div v-else class="space-y-3">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container text-[11.5px] relative">
                  <div class="flex justify-between items-baseline mb-0.5">
                    <div class="font-bold text-[12.5px] text-[#4A352F]" v-if="item.name || item.title">{{ item.name || item.title }}</div>
                    <span v-if="item.year || item.time" class="text-[11px] text-gray-500 font-bold ml-2 shrink-0">{{ item.year || item.time }}</span>
                  </div>
                  <div v-if="item.desc" class="text-[11.5px] text-[#4A352F]/90 html-content leading-relaxed" v-html="formatDesc(item.desc)"></div>
                  <div v-else-if="item.info" class="text-[11.5px] text-[#4A352F]/90 html-content leading-relaxed" v-html="formatDesc(item.info)"></div>

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

      <!-- 4. HÀNG DƯỚI CÙNG (INTERESTS & ADDITIONAL INFORMATION - CHIA ĐÔI SONG SONG) -->
      <section class="paginated-item px-[10mm] pb-[8mm] grid grid-cols-2 gap-[5mm] shrink-0">
        <!-- Cột: Interests (Hobbies) -->
        <div
          v-if="hobbiesSection && hobbiesSection.isVisible"
          class="section-block bg-[#F5ECE8] p-[5mm] rounded-[4px] border border-[#E9DDD7] relative group"
          :class="{ 'section-active': selectedSectionId === hobbiesSection.id }"
          :style="[
            selectedSectionId === hobbiesSection.id ? { '--active-bg': '#4A352F' } : {},
            { order: hobbiesSection.column === 'left' ? 1 : 2 }
          ]"
          @click.stop="toggleSection(hobbiesSection.id)"
        >
          <!-- Nav Control -->
          <transition name="fade-btns">
            <div v-if="selectedSectionId === hobbiesSection.id" class="nav-btns no-print" style="right: 12px; top: 12px;" @click.stop>
              <button @click.stop.prevent="moveHorizontal(hobbiesSection.id)" class="nav-btn" :title="hobbiesSection.column === 'left' ? 'Sang Phải' : 'Sang Trái'">
                <svg class="pointer-events-none" width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path v-if="hobbiesSection.column === 'left'" stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/>
                  <path v-else stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/>
                </svg>
              </button>
            </div>
          </transition>

          <div class="paginated-item">
            <div class="flex flex-col mb-2.5">
              <h3 class="section-title">
                Sở thích
              </h3>
              <div class="h-[1.2px] bg-[#D03B29] w-full mt-1"></div>
            </div>
          </div>

          <div class="text-[11.5px] text-[#4A352F]/90 leading-[1.6] text-justify font-medium">
            <div v-for="(item, itemIndex) in hobbiesSection.items" :key="item._refId" class="inline item-container relative">
              <span>{{ item.name }}</span><span v-if="itemIndex < hobbiesSection.items.length - 1">, </span>
              <transition name="fade-btns">
                <button v-if="selectedSectionId === hobbiesSection.id" @click.stop.prevent="$emit('removeItem', hobbiesSection.id, itemIndex)" class="delete-item-btn no-print" style="top: -6px; right: -6px; transform: scale(0.8)">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </transition>
            </div>
            <div v-if="hobbiesSection.items && hobbiesSection.items.length === 0" class="text-[#4A352F]/40 italic">Chưa có sở thích.</div>
          </div>
        </div>

        <!-- Cột: Additional Information (Languages / Awards) -->
        <div
          v-if="additionalSection && additionalSection.isVisible"
          class="section-block bg-[#F5ECE8] p-[5mm] rounded-[4px] border border-[#E9DDD7] relative group"
          :class="{ 'section-active': selectedSectionId === additionalSection.id }"
          :style="[
            selectedSectionId === additionalSection.id ? { '--active-bg': '#4A352F' } : {},
            { order: additionalSection.column === 'left' ? 1 : 2 }
          ]"
          @click.stop="toggleSection(additionalSection.id)"
        >
          <!-- Nav Control -->
          <transition name="fade-btns">
            <div v-if="selectedSectionId === additionalSection.id" class="nav-btns no-print" style="right: 12px; top: 12px;" @click.stop>
              <button @click.stop.prevent="moveHorizontal(additionalSection.id)" class="nav-btn" :title="additionalSection.column === 'left' ? 'Sang Phải' : 'Sang Trái'">
                <svg class="pointer-events-none" width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path v-if="additionalSection.column === 'left'" stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/>
                  <path v-else stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/>
                </svg>
              </button>
            </div>
          </transition>
          <div class="paginated-item">
            <div class="flex flex-col mb-2.5">
              <h3 class="section-title">
                Thông tin thêm
              </h3>
              <div class="h-[1.2px] bg-[#D03B29] w-full mt-1"></div>
            </div>
          </div>

          <div class="text-[11.5px] text-[#4A352F]/90 leading-[1.65] text-justify font-medium space-y-2">
            <div v-for="(item, itemIndex) in additionalSection.items" :key="item._refId" class="item-container relative">
              <div class="flex items-start gap-1">
                <span class="text-[#D03B29] shrink-0 mt-[1.5px] font-bold">•</span>
                <div class="flex-1">
                  <span v-if="item.name">{{ item.name }}</span>
                  <span v-else class="html-content" v-html="formatDesc(item.info || item.desc)"></span>
                </div>
              </div>
              <transition name="fade-btns">
                <button v-if="selectedSectionId === additionalSection.id" @click.stop.prevent="$emit('removeItem', additionalSection.id, itemIndex)" class="delete-item-btn no-print" style="top: -4px; right: -4px;">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </transition>
            </div>
            <div v-if="additionalSection.items && additionalSection.items.length === 0" class="text-[#4A352F]/40 italic">Chưa có thông tin bổ sung.</div>
          </div>
        </div>
      </section>

      <!-- Watermark chân trang -->
      <div class="absolute bottom-[4mm] right-[10mm] text-[10px] font-medium text-[#4A352F]/30 no-print tracking-wide">
        @CVBuilder
      </div>
    </main>

    <!-- ĐƯỜNG PHÂN TRANG HOÀN HẢO -->
    <template v-for="p in (pageCount - 1)" :key="'div-' + p">
      <div
        class="absolute left-0 w-full z-50 flex flex-col items-center justify-center pointer-events-none no-print"
        :style="{ top: `calc(${p * 297}mm - 8px)` }"
      >
        <div class="w-[105%] h-[16px] bg-slate-800/95 shadow-inner overflow-hidden border-y border-black/30 backdrop-blur-sm"></div>
        <span class="absolute text-[9px] uppercase font-bold text-slate-300 tracking-widest bg-slate-700 px-3 py-0.5 rounded border border-slate-600 shadow-md">Ngắt trang {{ p + 1 }}</span>
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
  requestPagination()
}

// ─── CONTACT ITEMS LOGIC ───
const contactIcons = {
  phone: '<svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z"/></svg>',
  email: '<svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M20 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z"/></svg>',
  address: '<svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z"/></svg>',
  github: '<svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M12 .297c-6.63 0-12 5.373-12 12 0 5.303 3.438 9.8 8.205 11.385.6.113.82-.258.82-.577 0-.285-.01-1.04-.015-2.04-3.338.724-4.042-1.61-4.042-1.61C4.422 18.07 3.633 17.7 3.633 17.7c-1.087-.744.084-.729.084-.729 1.205.084 1.838 1.236 1.838 1.236 1.07 1.835 2.809 1.305 3.495.998.108-.776.417-1.305.76-1.605-2.665-.3-5.466-1.332-5.466-5.93 0-1.31.465-2.38 1.235-3.22-.135-.303-.54-1.523.105-3.176 0 0 1.005-.322 3.3 1.23.96-.267 1.98-.399 3-.405 1.02.006 2.04.138 3 .405 2.28-1.552 3.285-1.23 3.285-1.23.645 1.653.24 2.873.12 3.176.765.84 1.23 1.91 1.23 3.22 0 4.61-2.805 5.625-5.475 5.92.42.36.81 1.096.81 2.22 0 1.606-.015 2.896-.015 3.286 0 .315.21.69.825.57C20.565 22.092 24 17.592 24 12.297c0-6.627-5.373-12-12-12"/></svg>'
}

const contactOrder = ref(['phone', 'email', 'github', 'address'])
const hiddenContacts = ref([])

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  switch (key) {
    case 'phone': return g.phone
    case 'email': return g.email
    case 'address': return g.address
    case 'github': return g.github || g.website
    default: return ''
  }
}

const contactItems = computed(() => {
  return contactOrder.value
    .filter(key => !hiddenContacts.value.includes(key) && !isEmpty(getContactValue(key)))
    .map(key => ({
      key,
      icon: contactIcons[key],
      value: getContactValue(key)
    }))
})

const moveContactUp = (idx) => {
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && !isEmpty(getContactValue(k)))
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
  const visible = contactOrder.value.filter(k => !hiddenContacts.value.includes(k) && !isEmpty(getContactValue(k)))
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
    } else if (key === 'github') {
       if (props.resumeData.general.github !== undefined) props.resumeData.general.github = ''
       if (props.resumeData.general.website !== undefined) props.resumeData.general.website = ''
    }
    hiddenContacts.value.push(key)
    requestPagination()
  }
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

// Lọc các section hiển thị ở thân chính
const mainSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'right' && !['hobbies', 'languages', 'awards', 'additional', 'summary'].includes(s.id))
)
const mainIds = computed(() => mainSections.value.map(s => s.id))

// Cấu trúc section chân trang (Sở thích & Thông tin thêm)
const hobbiesSection = computed(() =>
  props.resumeData.sections.find(s => s.id === 'hobbies')
)
const additionalSection = computed(() =>
  props.resumeData.sections.find(s => s.id === 'additional')
)

const moveHorizontal = (sectionId) => {
  const current = props.resumeData.sections.find(s => s.id === sectionId)
  if (!current) return

  const targetDir = current.column === 'left' ? 'right' : 'left'
  current.column = targetDir

  const otherId = sectionId === 'hobbies' ? 'additional' : 'hobbies'
  const other = props.resumeData.sections.find(s => s.id === otherId)
  if (other) {
    other.column = current.column === 'left' ? 'right' : 'left'
  }

  requestPagination()
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

  // 1. Reset marginTop về 0 cho TẤT CẢ các phần tử .paginated-item trong DOM trước khi tính toán
  const rawElements = cvRoot.value.querySelectorAll('.paginated-item')
  rawElements.forEach(el => { el.style.marginTop = '0px' })
  await nextTick()

  // 2. Màng lọc thông minh: Chỉ lấy các block .paginated-item ngoài cùng để tính toán phân trang, ngăn chặn lỗi lồng margin-top
  const allElements = Array.from(rawElements).filter(el => {
    if (el.offsetHeight === 0) return false
    let parent = el.parentElement
    while (parent && parent !== cvRoot.value) {
      if (parent.classList.contains('paginated-item')) return false
      parent = parent.parentElement
    }
    return true
  })

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
  // Chỉ kích hoạt các mục có trong ảnh thiết kế: Education, Certifications, Project, Activities, Skills, Hobbies (Interests), Languages (Additional Info)
  const activeDefault = ['summary', 'education', 'certifications', 'project', 'activities', 'skills', 'hobbies', 'languages', 'awards', 'additional']
  
  if (props.resumeData?.sections) {
    props.resumeData.sections.forEach(sec => {
      // 1. Chỉ active những mục có trong ảnh nếu chưa có giá trị
      if (sec.isVisible === undefined) {
        sec.isVisible = activeDefault.includes(sec.id);
      }
      
      // 2. Quy chuẩn vị trí cột để tương thích (chỉ set nếu chưa có column)
      if (!sec.column) {
        if (sec.id === 'hobbies' || sec.id === 'languages' || sec.id === 'awards') {
          sec.column = 'left';
        } else if (sec.id === 'additional') {
          sec.column = 'right';
        } else {
          sec.column = 'right';
        }
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

.section-title {
  font-size: 13.5px !important;
  font-weight: 800 !important;
  letter-spacing: 0.05em !important;
  color: #4A352F !important;
  margin: 0 !important;
  text-transform: uppercase !important;
  line-height: 1.2 !important;
}

/* Hiệu ứng vát góc xéo đỉnh trái tinh tế cho Avatar */
.avatar-clip {
  clip-path: polygon(18% 0%, 100% 0%, 100% 100%, 0% 100%, 0% 18%);
}

.section-block {
  position: relative;
  border-radius: 4px;
  border: 1.5px solid transparent;
  cursor: pointer;
  transition: border-color 0.18s ease, box-shadow 0.18s ease, background-color 0.18s ease;
}

.section-block.section-active {
  border-radius: 4px !important;
  border: 1.5px solid var(--active-bg, #4A352F) !important;
  box-shadow: 0 4px 18px rgba(74, 53, 47, 0.08);
  z-index: 30;
}

/* Nút điều hướng block */
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
  background: #4A352F;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  box-shadow: 0 2px 6px rgba(0,0,0,0.2);
  transition: background 0.15s, transform 0.15s;
}
.nav-btn:hover { background: #32231F; transform: scale(1.1); }
.nav-btn:active { transform: scale(0.95); }
.nav-btn-danger { background: #ef4444 !important; }
.nav-btn-danger:hover { background: #dc2626 !important; }

.nav-btn--xs {
  padding: 2px !important;
  border-radius: 3px !important;
}

.contact-item-btns {
  position: absolute;
  right: 0;
  top: 50%;
  transform: translateY(-50%);
  display: flex;
  gap: 3px;
  z-index: 50;
  background: #E9DCD6;
  padding-left: 5px;
}

/* Nút xóa item */
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
  padding-left: 1.25rem !important;
  margin-top: 0.25rem;
  margin-bottom: 0.25rem;
}
:deep(.html-content ol) {
  list-style-type: decimal !important;
  padding-left: 1.25rem !important;
  margin-top: 0.25rem;
  margin-bottom: 0.25rem;
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
