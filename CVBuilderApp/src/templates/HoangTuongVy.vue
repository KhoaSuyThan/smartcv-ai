<template>
  <div
    id="cv-printable-area"
    ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-row relative box-border text-[#333] leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: '\'Inter\', sans-serif' }"
    @click.self="selectedSectionId = null"
  >
    <!-- LEFT COLUMN -->
    <aside class="w-[75mm] z-10 flex flex-col shrink-0 relative bg-[#e5e7eb]">
      
      <!-- HEADER LEFT -->
      <div data-section-id="personal" class="pt-[20mm] px-[8mm] flex flex-col items-center paginated-item relative z-20 cursor-pointer hover:bg-black/5 transition-colors">
        <h1 class="font-bold text-[22px] text-center text-[#222] leading-tight mb-1" v-html="!isEmpty(resumeData.general.fullName) ? resumeData.general.fullName : 'HOÀNG TƯỜNG VY'"></h1>
        <h2 class="font-normal text-[13.5px] text-center text-[#444] mb-6" v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'Content Marketing'"></h2>

        <!-- Avatar -->
        <div class="relative w-[48mm] h-[48mm] rounded-full overflow-hidden mb-8 mx-auto border-4 border-white shadow-md bg-white">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center bg-gray-200 text-gray-500">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
          </div>
        </div>
      </div>

      <!-- SIDEBAR SECTIONS -->
      <div class="w-full flex-1 pb-[20mm] flex flex-col">
        <!-- FIXED CONTACT INFO SECTION (Bản chất nó là thông tin cá nhân) -->
        <div data-section-id="contact" class="section-block relative group contact-block cursor-pointer hover:bg-black/5 transition-colors" style="margin-bottom: 15mm;"
             :class="{ 'contact-active': selectedSectionId === 'contact' }"
             @click.stop="selectedSectionId = selectedSectionId === 'contact' ? null : 'contact'">
          <div class="paginated-item">
            <h3 class="section-title text-white font-bold uppercase tracking-wider text-center py-2 mb-4" style="background-color: #2b2b2b; font-size: 14px !important;">
              THÔNG TIN CÁ NHÂN
            </h3>
          </div>
          
          <div v-if="contactItems.length > 0" class="flex flex-col w-full font-medium text-[#333] space-y-3 px-3" style="font-size: 11.5px !important">
            <div v-for="(ci, ciIdx) in contactItems" :key="ci.key"
                 class="flex items-start gap-3 paginated-item relative contact-item-container">
              <div class="w-5 font-bold text-[#444] shrink-0 flex justify-center pt-[2px]">
                <svg width="12" height="12" fill="currentColor" viewBox="0 0 20 20" v-html="ci.icon"></svg>
              </div>
              <span class="break-all flex-1 pt-[1px]" v-html="ci.value"></span>
              
              <div v-if="selectedSectionId === 'contact'" class="contact-item-btns no-print">
                <button v-if="ciIdx > 0" @click.stop.prevent="moveContactUp(ciIdx)" class="nav-btn" title="Di chuyển lên" style="padding:3px">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                </button>
                <button v-if="ciIdx < contactItems.length - 1" @click.stop.prevent="moveContactDown(ciIdx)" class="nav-btn" title="Di chuyển xuống" style="padding:3px">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                </button>
                <button @click.stop.prevent="removeContactItem(ciIdx)" class="nav-btn nav-btn-danger" title="Ẩn mục này" style="padding:3px">
                  <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </div>

        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-if="section.isVisible"
            :data-section-id="section.id"
            class="section-block relative group cursor-pointer hover:bg-black/5 transition-colors" style="margin-bottom: 15mm;"
            :class="{ 'section-active': selectedSectionId === section.id }"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
                <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên">
                  <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                </button>
                <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống">
                  <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                </button>
                <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'right')" class="nav-btn" title="Sang Phải">
                  <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
                </button>
                <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                  <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </transition>

            <div class="paginated-item">
              <h3 class="section-title text-white font-bold uppercase tracking-wider text-center py-2 mb-4" style="background-color: #2b2b2b; font-size: 14px !important;">
                <span v-html="section.title"></span>
              </h3>
            </div>

            <div class="space-y-4 px-3" v-if="sectionHasContent(section)">
              <!-- Summary -->
              <div v-if="section.id === 'summary'">
                <div class="leading-relaxed text-justify html-content font-medium text-[#444]" style="font-size: 11.5px !important" v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Mục tiêu nghề nghiệp...')"></div>
              </div>

              <!-- Education -->
              <div v-else-if="section.id === 'education'" class="space-y-4 relative">
                <div class="absolute left-[3px] top-1 bottom-4 w-[1px] bg-[#444] z-0 opacity-20"></div>
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative pl-[14px]">
                  <div class="paginated-item relative z-10">
                    <div class="absolute -left-[14px] top-1.5 w-2 h-2 rounded-full bg-[#444]"></div>
                    <div class="flex justify-between items-start gap-2 mb-1">
                      <h4 class="text-[#333] font-bold" style="font-size: 12px !important">
                        <span v-html="item.school"></span>
                      </h4>
                      <span v-if="item.year" class="font-normal text-[#555] shrink-0" style="font-size: 10.5px !important"><span v-html="item.year"></span></span>
                    </div>
                    <div v-if="item.major" class="text-[#444] font-normal mb-1" style="font-size: 11.5px !important"><span v-html="item.major"></span></div>
                    <div v-if="item.gradType" class="text-[#555] font-normal" style="font-size: 11px !important">
                      Xếp loại: <span class="font-bold"><span v-html="item.gradType"></span></span>
                    </div>
                  </div>
                  <div class="text-[#444] leading-relaxed html-content mt-1" style="font-size: 11px !important" v-html="formatDesc(item.desc)"></div>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- Experience / Activities / Projects -->
              <div v-else-if="['experience','activities','project'].includes(section.id)" class="space-y-4 relative">
                <div class="absolute left-[3px] top-1 bottom-4 w-[1px] bg-[#444] z-0 opacity-20"></div>
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative pl-[14px]">
                  <div class="paginated-item relative z-10">
                    <div class="absolute -left-[14px] top-1.5 w-2 h-2 rounded-full bg-[#444]"></div>
                    <div class="flex justify-between items-start gap-2 mb-1">
                      <h4 class="text-[#333] font-bold" style="font-size: 12px !important">
                        <span v-html="section.id === 'experience' ? item.company : (item.name || item.organization)"></span>
                      </h4>
                    </div>
                    <div class="flex justify-between items-start gap-2 mb-1">
                      <span class="font-normal text-[#555] uppercase tracking-wide" style="font-size: 10.5px !important"><span v-html="section.id === 'experience' ? item.role : (item.role || item.position || '')"></span></span>
                      <span v-if="item.time" class="font-normal text-[#555] shrink-0" style="font-size: 10px !important"><span v-html="item.time"></span></span>
                    </div>
                  </div>
                  <div class="leading-relaxed text-[#444] text-justify html-content mt-1" style="font-size: 11px !important" v-html="formatDesc(item.desc)"></div>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- Certifications / Awards -->
              <div v-else-if="['certifications', 'awards'].includes(section.id)" class="space-y-3 relative">
                <div class="absolute left-[3px] top-1 bottom-4 w-[1px] bg-[#444] z-0 opacity-20"></div>
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative pl-[14px]">
                  <div class="paginated-item relative z-10">
                    <div class="absolute -left-[14px] top-1.5 w-2 h-2 rounded-full bg-[#444]"></div>
                    <div v-if="item.year" class="font-bold text-[#333] mb-0.5" style="font-size: 11.5px !important"><span v-html="item.year"></span></div>
                    <div class="font-normal text-[#444] html-content" style="font-size: 11px !important"><span v-html="item.name || item.info"></span></div>
                  </div>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- Skills / IT Skills / Languages -->
              <div v-else-if="['skill', 'lang'].some(k => section.id.toLowerCase().includes(k))" class="space-y-1.5">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="paginated-item text-[#333] item-container font-medium uppercase leading-snug"
                  style="font-size: 11px !important"
                >
                  <div class="flex justify-between items-center w-full">
                    <span class="font-bold text-[#333]"><span v-html="item.name"></span></span>
                    <span v-if="item.level || item.info" class="font-normal text-[#666] normal-case italic" style="font-size: 10px !important"><span v-html="item.level || item.info"></span></span>
                  </div>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: -2px; right: -2px">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- References -->
              <div v-else-if="section.id.toLowerCase().includes('reference')" class="space-y-3">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container leading-relaxed">
                  <div class="font-bold text-[#333] mb-0.5" style="font-size: 11.5px !important"><span v-html="item.name"></span> <span v-if="item.role || item.title" class="font-normal text-[#555]">- <span v-html="item.role || item.title"></span></span></div>
                  <div class="text-[#555] font-normal html-content" style="font-size: 11px !important" v-html="formatDesc(item.contact || item.info || item.desc)"></div>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: -2px; right: -2px">
                      <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- Hobbies -->
              <div v-else-if="section.id === 'hobbies'" class="space-y-1">
                <div class="text-[#333] font-normal leading-relaxed text-justify" style="font-size: 11.5px !important">
                  <span v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container inline-block mr-1 relative">
                    <span v-html="item.name"></span><span v-if="itemIndex < section.items.length - 1"> - </span>
                    <transition name="fade-btns">
                      <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print" style="top: -5px; right: -8px; transform: scale(0.8)">
                        <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                      </button>
                    </transition>
                  </span>
                </div>
              </div>

              <!-- Other sidebar sections -->
              <div v-else class="space-y-3">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[#333] font-normal leading-relaxed item-container relative" style="font-size: 11px !important">
                  <div v-if="item.name || item.title" class="paginated-item font-bold mb-0.5" style="font-size: 11.5px !important; color: #333;">
                    <span v-html="item.name || item.title"></span>
                  </div>
                  <div class="html-content" v-html="formatDesc(item.desc || item.info)"></div>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn no-print">
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
    <main class="flex-1 flex flex-col relative bg-white z-20" @click.self="selectedSectionId = null">
      <div class="px-[10mm] pt-[20mm] pb-[20mm] flex-1 flex flex-col">
        <template v-for="section in mainSections" :key="section.id">
          <div
            v-if="section.isVisible"
            :data-section-id="section.id"
            class="section-block relative group cursor-pointer hover:bg-black/5 transition-colors" style="margin-bottom: 15mm;"
            :class="{ 'section-active': selectedSectionId === section.id }"
            @click.stop="toggleSection(section.id)"
          >
            <!-- Nav Buttons -->
            <transition name="fade-btns">
              <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
                <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                </button>
                <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                </button>
                <button @click.stop.prevent="$emit('moveHorizontal', section.id, 'left')" class="nav-btn" title="Sang Trái">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
                </button>
                <button @click.stop.prevent="section.isVisible = false" class="nav-btn nav-btn-danger" title="Ẩn mục này">
                  <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </transition>

            <div class="paginated-item">
              <h3 class="section-title text-white font-bold uppercase tracking-wider pl-4 py-1.5 mb-4" style="background-color: #2b2b2b; font-size: 15px !important;">
                <span v-html="section.title"></span>
              </h3>
            </div>

            <div class="px-1 space-y-5" v-if="sectionHasContent(section)">
              <!-- Summary -->
              <div v-if="section.id === 'summary'">
                <div class="leading-relaxed text-justify html-content font-medium text-[#444]" style="font-size: 12px !important" v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Mục tiêu nghề nghiệp...')"></div>
              </div>

              <!-- Education -->
              <div v-else-if="section.id === 'education'" class="space-y-4 relative">
                <div class="absolute left-[3px] top-2 bottom-4 w-[1px] bg-[#444] z-0 opacity-20"></div>
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative pl-5">
                  <div class="paginated-item relative z-10">
                    <div class="absolute -left-5 top-1.5 w-2 h-2 rounded-full bg-[#444]"></div>
                    <div class="flex justify-between items-start gap-4 mb-0.5">
                      <h4 class="text-[#222] font-bold" style="font-size: 13px !important">
                        <span v-html="item.school"></span>
                      </h4>
                      <span v-if="item.year" class="font-bold text-[#333] shrink-0" style="font-size: 12px !important"><span v-html="item.year"></span></span>
                    </div>
                    <div v-if="item.major" class="font-normal text-[#555] mb-1" style="font-size: 12px !important"><span v-html="item.major"></span></div>
                    <div v-if="item.gradType" class="text-[#555] font-normal" style="font-size: 11.5px !important">
                      Xếp loại: <span class="font-bold"><span v-html="item.gradType"></span></span>
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

              <!-- Experience / Activities / Projects -->
              <div v-else-if="['experience','activities','project'].includes(section.id)" class="space-y-4 relative">
                <div class="absolute left-[3px] top-2 bottom-4 w-[1px] bg-[#444] z-0 opacity-20"></div>
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative pl-5">
                  <div class="paginated-item relative z-10">
                    <div class="absolute -left-5 top-1.5 w-2 h-2 rounded-full bg-[#444]"></div>
                    <div class="flex justify-between items-start gap-4 mb-0.5">
                      <h4 class="text-[#222] font-bold" style="font-size: 13px !important">
                        <span v-html="section.id === 'experience' ? item.company : (item.name || item.organization)"></span>
                      </h4>
                      <span v-if="item.time" class="font-bold text-[#333] shrink-0" style="font-size: 12px !important"><span v-html="item.time"></span></span>
                    </div>
                    <div class="font-normal text-[#555] uppercase tracking-wide mb-1.5" style="font-size: 11px !important"><span v-html="section.id === 'experience' ? item.role : (item.role || item.position || '')"></span></div>
                  </div>
                  <div class="leading-[1.6] text-[#444] text-justify html-content" style="font-size: 12px !important" v-html="formatDesc(item.desc)"></div>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- Certifications / Awards -->
              <div v-else-if="['certifications', 'awards'].includes(section.id)" class="space-y-4 relative">
                <div class="absolute left-[3px] top-2 bottom-4 w-[1px] bg-[#444] z-0 opacity-20"></div>
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container relative pl-5">
                  <div class="paginated-item relative z-10">
                    <div class="absolute -left-5 top-1.5 w-2 h-2 rounded-full bg-[#444]"></div>
                    <div v-if="item.year" class="font-bold text-[#333] mb-0.5" style="font-size: 12px !important"><span v-html="item.year"></span></div>
                    <div class="font-normal text-[#444] html-content" style="font-size: 12.5px !important"><span v-html="item.name || item.info"></span></div>
                  </div>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- Skills / IT Skills / Languages -->
              <div v-else-if="['skill', 'lang'].some(k => section.id.toLowerCase().includes(k))" class="space-y-1.5">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="paginated-item text-[#333] item-container font-medium uppercase leading-snug"
                  style="font-size: 12px !important"
                >
                  <div class="flex justify-between items-center w-full">
                    <span class="font-bold text-[#333]"><span v-html="item.name"></span></span>
                    <span v-if="item.level || item.info" class="font-normal text-[#666] normal-case italic" style="font-size: 11px !important"><span v-html="item.level || item.info"></span></span>
                  </div>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- References -->
              <div v-else-if="section.id.toLowerCase().includes('reference')" class="space-y-3">
                <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container leading-relaxed">
                  <div class="font-bold text-[#333] mb-0.5" style="font-size: 13px !important"><span v-html="item.name"></span> <span v-if="item.role || item.title" class="font-normal text-[#555]">- <span v-html="item.role || item.title"></span></span></div>
                  <div class="text-[#555] font-normal html-content" style="font-size: 12px !important" v-html="formatDesc(item.contact || item.info || item.desc)"></div>
                  <transition name="fade-btns">
                    <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                      <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </transition>
                </div>
              </div>

              <!-- Hobbies -->
              <div v-else-if="section.id === 'hobbies'" class="space-y-1">
                <div class="text-[#333] font-normal leading-relaxed text-justify" style="font-size: 12px !important">
                  <span v-for="(item, itemIndex) in section.items" :key="item._refId" class="paginated-item item-container inline-block mr-1 relative">
                    <span v-html="item.name"></span><span v-if="itemIndex < section.items.length - 1"> - </span>
                    <transition name="fade-btns">
                      <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-item-btn delete-item-btn--lg no-print">
                        <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                      </button>
                    </transition>
                  </span>
                </div>
              </div>

              <!-- Other main sections -->
              <div v-else class="space-y-4">
                <div
                  v-for="(item, itemIndex) in section.items"
                  :key="item._refId"
                  class="item-container relative text-[#444] leading-relaxed"
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

    <!-- ĐƯỜNG PHÂN TRANG -->
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
}

// ─── CONTACT ITEMS: Danh sách động có thể sắp xếp / ẩn ───
const contactIcons = {
  phone: '<path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z" />',
  email: '<path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z" /><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z" />',
  web: '<path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM4.332 8.027a6.012 6.012 0 011.912-2.706C6.512 5.73 6.974 6 7.5 6A1.5 1.5 0 019 7.5V8a2 2 0 004 0 2 2 0 011.523-1.943A5.977 5.977 0 0116 10c0 .34-.028.675-.083 1H15a2 2 0 00-2 2v2.197A5.973 5.973 0 0110 16v-2a2 2 0 00-2-2 2 2 0 01-2-2 2 2 0 00-1.668-1.973z" clip-rule="evenodd" />',
  address: '<path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd" />',
  birthDate: '<path d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" />'
}

const contactOrder = ref(['phone', 'email', 'web', 'address', 'birthDate'])
const hiddenContacts = ref([])

const getContactValue = (key) => {
  const g = props.resumeData?.general
  if (!g) return ''
  switch (key) {
    case 'phone': return g.phone || ''
    case 'email': return g.email || ''
    case 'web': return g.github || g.linkedin || g.website || ''
    case 'address': return g.address || ''
    case 'birthDate': return g.birthDate || ''
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
    } else if (key === 'web') {
      if (props.resumeData.general.github !== undefined) props.resumeData.general.github = ''
      if (props.resumeData.general.linkedin !== undefined) props.resumeData.general.linkedin = ''
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
  if (section.id === 'summary') return !isEmpty(props.resumeData?.general?.summary)
  return section.items && section.items.length > 0
}

const formatDesc = (text) => {
  if (!text) return ''
  
  if (!/<[a-z][\s\S]*>/i.test(text)) {
     return text.split('\n')
                .map(l => l.trim())
                .filter(Boolean)
                .map(l => `<div class="paginated-item block mb-1">${l}</div>`)
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
        div.className = 'paginated-item block mb-1'
        div.appendChild(node.cloneNode(true))
        container.appendChild(div)
      }
    } else if (node.nodeType === Node.ELEMENT_NODE) {
      if (node.tagName === 'BR') {
        const div = document.createElement('div')
        div.className = 'paginated-item block h-[4px]'
        container.appendChild(div)
      } else if (['UL', 'OL'].includes(node.tagName)) {
        Array.from(node.children).forEach(li => li.classList.add('paginated-item'))
        container.appendChild(node.cloneNode(true))
      } else {
        node.classList.add('paginated-item', 'block', 'mb-1')
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
  
  const bottomSafeZone = 20 * pxPerMm 
  const topMargin = 20 * pxPerMm 

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
    } else if (top > pageH && topInPage < topMargin) {
       el.style.marginTop = `${topMargin - topInPage}px`
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
  // Chỉ bật hiển thị các section đúng theo mẫu
  const isLeft = (id) => ['education', 'skill', 'reference', 'hobbi', 'lang'].some(k => id.toLowerCase().includes(k))
  const isRight = (id) => ['summary', 'experience', 'activit', 'cert', 'award'].some(k => id.toLowerCase().includes(k))
  
  props.resumeData?.sections?.forEach(sec => {
    if (sec.isVisible === undefined) {
      if (isLeft(sec.id)) {
        sec.isVisible = true
        sec.column = 'left'
      } else if (isRight(sec.id)) {
        sec.isVisible = true
        sec.column = 'right'
      } else {
        sec.isVisible = false
      }
    } else {
      if (isLeft(sec.id)) {
        sec.column = 'left'
      } else if (isRight(sec.id)) {
        sec.column = 'right'
      }
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
  overflow-wrap: anywhere;
}

.section-block {
  position: relative;
  border-radius: 6px;
  border: 2px solid transparent;
  cursor: pointer;
  transition: box-shadow 0.18s ease, border-color 0.18s ease;
  padding: 0;
}

.section-block.section-active {
  border: 2px solid #2b2b2b !important;
  box-shadow: 0 4px 18px rgba(0,0,0,0.10);
  z-index: 10;
}

.nav-btns {
  position: absolute;
  right: 6px;
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

.nav-btn-danger {
  background: #ef4444 !important;
  box-shadow: 0 2px 6px rgba(239, 68, 68, 0.4) !important;
}
.nav-btn-danger:hover {
  background: #dc2626 !important;
}

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

.contact-block {
  border-radius: 6px;
  border: 2px solid transparent;
  cursor: pointer;
  transition: border-color 0.18s ease, box-shadow 0.18s ease;
  padding: 4px 6px;
}

.contact-block.contact-active {
  border: 2px solid #2563eb !important;
  box-shadow: 0 4px 18px rgba(0,0,0,0.10);
  z-index: 10;
}

.contact-item-container {
  position: relative;
}

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

:deep(.html-content ul) {
  list-style-type: none !important;
  padding-left: 0.75rem !important;
  margin-top: 0.25rem;
  margin-bottom: 0.25rem;
}
:deep(.html-content ol) {
  list-style-type: decimal !important;
  padding-left: 1.25rem !important;
  margin-top: 0.25rem;
  margin-bottom: 0.25rem;
}
:deep(.html-content li) {
  margin-bottom: 0.25rem;
  position: relative;
}
:deep(.html-content ul li::before) {
  content: "•";
  position: absolute;
  left: -0.75rem;
  color: #555;
}
:deep(.html-content b),
:deep(.html-content strong) { font-weight: 700 !important; }
:deep(.html-content i),
:deep(.html-content em) { font-style: italic !important; }
:deep(.html-content u) { text-decoration: underline !important; }

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
