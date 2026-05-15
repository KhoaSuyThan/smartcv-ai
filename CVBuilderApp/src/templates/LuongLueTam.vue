<template>
  <div id="cv-printable-area" ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-row relative box-border text-[#333] leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: `'Inter', sans-serif` }">

    <!-- ===== HEADER OVERLAY ===== -->
    <div class="absolute top-0 left-0 w-full z-30 pointer-events-none flex" style="height: 75mm;">
      <div class="flex flex-col justify-end pb-[6mm] pl-[10mm] pr-[6mm] pointer-events-auto shrink-0"
        style="width: 125mm; background-color: #9db078;">
        <h1 class="uppercase font-bold text-white/80 leading-none tracking-[0.15em]" style="font-size: 38px !important;">
          {{ splitName.last }}
        </h1>
        <h1 class="font-cursive text-white leading-none tracking-wide"
          style="font-size: 58px; text-shadow: 1px 2px 4px rgba(0,0,0,0.12);">
          {{ splitName.first }}
        </h1>
        <div class="flex items-center gap-3 mt-3">
          <span class="font-bold uppercase tracking-[0.18em] text-[#334155]" style="font-size: 12px !important;"
            v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : ''">
          </span>
          <div class="flex-1 h-[2px] bg-[#334155]/50 max-w-[100px]"></div>
        </div>
      </div>
      <div class="flex-1 flex items-start justify-end pr-[8mm] pointer-events-auto" style="padding-top: 4mm;">
        <div class="bg-gray-200 border-[3px] border-white shadow-md overflow-hidden relative z-30"
          style="width: 46mm; height: 58mm;">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center text-gray-400">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1"
                d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
            </svg>
          </div>
        </div>
      </div>
    </div>

    <!-- ===== SIDEBAR TRÁI ===== -->
    <aside class="shrink-0 z-10 flex flex-col relative" style="width: 85mm;" :style="{ backgroundColor: templatePrimaryColor }">
      <div class="absolute top-[78mm] bottom-[10mm] z-10"
        style="left: 8mm; width: 1.5px; background: rgba(255,255,255,0.55);"></div>
      <div style="height: 78mm; flex-shrink: 0;"></div>

      <div class="flex flex-col flex-1 relative z-20" style="padding: 0 5mm 8mm 20mm;">

        <!-- Liên hệ (cố định) -->
        <div
          class="section-block mb-3 relative"
          :class="{ 'section-active--sidebar': selectedSectionId === 'contact' }"
          @click.stop="selectedSectionId = selectedSectionId === 'contact' ? null : 'contact'"
        >
          <div class="paginated-item relative z-20">
            <div class="absolute z-20 rounded-full bg-white shadow-[0_0_0_3px_var(--dot-color)]"
              :style="{ '--dot-color': templatePrimaryColor, left: '-12mm', top: '10px', width: '10px', height: '10px', transform: 'translateX(-50%)' }">
            </div>
            <h3 class="font-bold uppercase px-3 py-1.5 inline-block mb-4 shadow-[3px_3px_0px_rgba(0,0,0,0.15)] tracking-wide break-words whitespace-normal"
              style="background: white; font-size: 13px !important; color: #465568;">
              Liên hệ
            </h3>
          </div>

          <div class="flex flex-col gap-3 paginated-item" style="font-size: 11px !important; color: rgba(255,255,255,0.9);">
            <div
              v-for="(ci, ciIdx) in contactItems"
              :key="ci.key"
              class="flex items-center gap-2 relative pr-12 min-h-[20px]"
            >
              <svg class="w-3.5 h-3.5 shrink-0 opacity-80 mt-[1px]" fill="currentColor" viewBox="0 0 20 20" v-html="ci.icon"></svg>
              <span class="font-semibold text-white tracking-wide break-all" v-html="ci.value"></span>

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

        <!-- Sidebar sections -->
        <template v-for="section in sidebarSections" :key="section.id">
          <!-- ĐÃ GỠ BỎ paginated-item Ở ĐÂY ĐỂ TRÁNH KÉO CẢ CỤC -->
          <div
            v-show="section.isVisible"
            class="section-block mb-3 relative"
            :class="{ 'section-active--sidebar': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="moveSectionUp(section.id, 'left')" class="nav-btn" title="Di chuyển lên">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionDown(section.id, 'left')" class="nav-btn" title="Di chuyển xuống">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionHorizontal(section.id, 'right')" class="nav-btn" title="Sang Phải">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"/></svg>
              </button>
              <button @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn--delete" title="Ẩn mục này">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <!-- Tiêu đề có paginated-item -->
            <div class="paginated-item relative z-20">
              <div class="absolute z-20 rounded-full bg-white"
                :style="{ boxShadow: `0 0 0 3px ${templatePrimaryColor}`, left: '-12mm', top: '10px', width: '10px', height: '10px', transform: 'translateX(-50%)' }">
              </div>
              <h3 class="font-bold uppercase px-3 py-1.5 inline-block mb-4 shadow-[3px_3px_0px_rgba(0,0,0,0.15)] tracking-wide break-words whitespace-normal"
                style="background: white; font-size: 13px !important; color: #465568;">
                {{ section.title }}
              </h3>
            </div>

            <!-- Summary -->
            <div v-if="section.id === 'summary'"
              class="leading-relaxed text-justify html-content font-medium"
              style="font-size: 11px !important; color: rgba(255,255,255,0.9);"
              v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : '')">
            </div>

            <!-- NÂNG CẤP: Timeline xịn xò cho Sidebar -->
            <div v-else-if="['education','experience','project','activities'].includes(section.id)" class="space-y-4">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative flex flex-col" style="color: white;">
                
                <!-- Gói Tiêu đề & Thời gian vào 1 cụm paginated-item -->
                <div class="paginated-item relative">
                  <div class="absolute rounded-full bg-white" style="left: -20px; top: 5px; width: 6px; height: 6px;"></div>
                  <h4 class="font-bold leading-snug mb-1" style="font-size: 11.5px !important;">
                    <span v-html="section.id === 'education' ? item.school : (section.id === 'experience' ? item.company : item.name)"></span>
                  </h4>
                  <div v-if="item.year || item.time"
                    class="inline-block text-[#334155] font-bold tracking-wide shadow-[1px_1px_0px_rgba(0,0,0,0.1)] mb-1"
                    style="font-size: 10px !important; padding: 2px 6px; background-color: #f1f5f9; width: fit-content;">
                    {{ item.year || item.time }}
                  </div>
                  <h4 v-if="item.major || item.role" class="font-semibold mb-1 opacity-90" style="font-size: 11.5px !important;">
                    {{ item.major || item.role }}
                  </h4>
                </div>
                
                <!-- Nội dung mô tả sẽ được cắt nhỏ -->
                <div v-if="item.desc" class="text-white/80 html-content leading-relaxed text-justify font-medium"
                  style="font-size: 11px !important;"
                  v-html="formatDesc(item.desc)">
                </div>

                <div v-if="item.gradType" class="paginated-item font-semibold mt-1"
                  :style="{ fontSize: '11px !important', color: 'rgba(255,255,255,0.8)' }">
                  Xếp loại: {{ item.gradType }}
                </div>

                <button v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print absolute" style="right: 0; top: -4px;">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Skills / Languages / IT -->
            <div v-else-if="['skills','languages','it_skills'].includes(section.id)" class="space-y-3">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="flex flex-col item-container relative paginated-item"
                style="font-size: 11px !important; color: white;">
                <div class="flex items-center gap-2 mb-1">
                  <div class="rounded-full bg-white shrink-0" style="width: 6px; height: 6px;"></div>
                  <span class="font-bold tracking-wide">{{ item.name }}</span>
                </div>
                <div class="w-full rounded-full overflow-hidden relative" style="height: 5px; background: rgba(255,255,255,0.2);">
                  <div class="absolute left-0 top-0 bottom-0 rounded-full"
                    :style="{ width: getLevelPercent(item.level), backgroundColor: templateSecondaryColor }"></div>
                </div>
                <button v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print absolute" style="right: 0; top: 0;">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Các section khác (Fallback) -->
            <div v-else class="space-y-2">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="font-medium leading-relaxed item-container relative"
                style="font-size: 11px !important; color: rgba(255,255,255,0.9);">
                <div class="flex items-start gap-2">
                  <div class="rounded-full bg-white shrink-0" style="width: 6px; height: 6px; margin-top: 6px;"></div>
                  <div class="flex flex-col">
                    <div v-if="item.year || item.time"
                      class="inline-block text-[#334155] font-bold tracking-wide shadow-[1px_1px_0px_rgba(0,0,0,0.1)] mb-1"
                      style="font-size: 10px !important; padding: 2px 6px; background-color: #f1f5f9; width: fit-content;">
                      {{ item.year || item.time }}
                    </div>
                    <div class="font-bold mb-0.5" v-if="item.name || item.info" style="font-size: 11px !important;">
                      {{ item.name || item.info }}
                    </div>
                    <div class="html-content opacity-80" v-if="item.desc" v-html="formatDesc(item.desc)"></div>
                  </div>
                </div>
                <button v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print absolute" style="right: 0; top: 0;">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>
      </div>
    </aside>

    <!-- ===== MAIN CONTENT PHẢI ===== -->
    <main class="flex-1 flex flex-col relative bg-white z-20" @click.self="selectedSectionId = null">
      <div style="height: 78mm; flex-shrink: 0; border-left: 1.5px solid #e2e8f0; margin-left: 12mm;"></div>

      <div class="flex-1 flex flex-col pb-[12mm] relative gap-2" style="padding-left: 10mm; padding-right: 12mm;"
        @click.self="selectedSectionId = null">
        <template v-for="section in mainSections" :key="section.id">
          <!-- ĐÃ GỠ BỎ paginated-item Ở ĐÂY -->
          <div
            v-show="section.isVisible"
            class="section-block mb-1"
            :class="{ 'section-active--main': selectedSectionId === section.id }"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <div v-if="selectedSectionId === section.id" class="nav-btns no-print">
              <button @click.stop.prevent="moveSectionUp(section.id, 'right')" class="nav-btn" title="Di chuyển lên">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionDown(section.id, 'right')" class="nav-btn" title="Di chuyển xuống">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
              </button>
              <button @click.stop.prevent="moveSectionHorizontal(section.id, 'left')" class="nav-btn" title="Sang Trái">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"/></svg>
              </button>
              <button @click.stop.prevent="section.isVisible = false; selectedSectionId = null; requestPagination()" class="nav-btn nav-btn--delete" title="Ẩn mục này">
                <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
              </button>
            </div>

            <!-- Tiêu đề section (Paginated) -->
            <div class="flex items-center gap-3 mb-3 paginated-item" style="margin-left: -22px;">
              <div class="rounded-full flex items-center justify-center shrink-0"
                :style="{ width: '34px', height: '34px', border: `4px solid ${templateSecondaryColor}`, backgroundColor: '#dce4cd' }">
                <div class="rounded-full" :style="{ width: '10px', height: '10px', backgroundColor: templatePrimaryColor }"></div>
              </div>
              <h3 class="font-bold uppercase text-white tracking-widest shadow-[3px_3px_0px_rgba(0,0,0,0.15)]"
                style="font-size: 14px !important; padding: 6px 18px;"
                :style="{ backgroundColor: templateSecondaryColor }">
                {{ section.title }}
              </h3>
            </div>

            <div v-if="section.id === 'summary'"
              class="text-[12.5px] leading-[1.6] text-slate-700 text-justify font-medium html-content"
              v-html="formatDesc(!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Mô tả mục tiêu nghề nghiệp của bạn...')">
            </div>

            <!-- Experience / Project / Activities -->
            <div v-else-if="['experience','project','activities'].includes(section.id)" class="space-y-3">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative flex items-stretch">
                <!-- Cột trái: Tách thành paginated-item riêng -->
                <div class="pr-3 relative shrink-0 paginated-item" style="width: 36%;">
                  <div class="absolute rounded-full bg-slate-400" style="left: -14px; top: 7px; width: 7px; height: 7px;"></div>
                  <h4 class="font-bold leading-snug mb-1" style="font-size: 11.5px !important; color: #222;">
                    <span v-html="section.id === 'experience' ? item.company : (item.name || item.title)"></span>
                  </h4>
                  <div v-if="item.year || item.time"
                    class="inline-block text-white font-bold tracking-wide shadow-[2px_2px_0px_rgba(0,0,0,0.15)]"
                    style="font-size: 10px !important; padding: 2px 8px; margin-top: 3px;"
                    :style="{ backgroundColor: templateSecondaryColor }">
                    {{ item.year || item.time }}
                  </div>
                </div>
                
                <div class="pb-2 relative" style="width: 64%; border-left: 2px solid #cbd5e1; padding-left: 12px;">
                  <h4 v-if="item.major || item.role" class="font-bold mb-1 paginated-item" style="font-size: 11.5px !important; color: #333;">
                    {{ item.major || item.role }}
                  </h4>
                  <div v-if="item.desc" class="text-slate-700 html-content leading-relaxed text-justify font-medium"
                    style="font-size: 11px !important;"
                    v-html="formatDesc(item.desc)">
                  </div>
                  <div v-if="item.gradType" class="font-semibold mt-1 paginated-item"
                    :style="{ fontSize: '11px !important', color: templateSecondaryColor }">
                    Xếp loại: {{ item.gradType }}
                  </div>
                </div>
                
                <button v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print absolute" style="right: -8px; top: -4px;">
                  <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Education -->
            <div v-else-if="section.id === 'education'" class="space-y-2.5">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="relative pl-6 item-container">
                <button v-if="selectedSectionId === section.id" @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn no-print" style="top: 0; right: 0;"><svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg></button>
                <div class="paginated-item relative">
                  <div class="absolute -left-6 top-1 text-pink-500"><svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24"><path d="M12,2L14.5,9H21L15.5,13.5L18,20.5L12,16L6,20.5L8.5,13.5L3,9H9.5L12,2Z" /></svg></div>
                  <div class="text-[12px] font-black text-pink-500 mb-0.5 tracking-wide">{{ item.year || '2024 - 2028' }}</div>
                  <div class="text-[14px] font-black text-slate-900 leading-tight mb-0.5">{{ item.major || 'Chuyên ngành' }}</div>
                  <div class="text-[12px] font-bold text-slate-500 italic">{{ item.school || 'Tên trường học' }}</div>
                </div>
                <div class="text-[12.5px] leading-[1.6] text-slate-600 html-content" v-html="formatDesc(item.desc)"></div>
                <div v-if="!item.desc && item.gradType" class="paginated-item text-[12.5px] font-bold mt-1">Trạng thái: <span class="font-normal">{{ item.gradType }}</span></div>
              </div>
            </div>

            <!-- Awards / Certifications -->
            <div v-else-if="['awards','certifications'].includes(section.id)" class="space-y-2">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative flex items-stretch paginated-item">
                <div class="pr-3 relative shrink-0" style="width: 36%;">
                  <div class="absolute rounded-full bg-slate-400" style="left: -14px; top: 7px; width: 7px; height: 7px;"></div>
                  <div v-if="item.year"
                    class="inline-block text-white font-bold tracking-wide shadow-[2px_2px_0px_rgba(0,0,0,0.15)]"
                    style="font-size: 10px !important; padding: 2px 8px;"
                    :style="{ backgroundColor: templateSecondaryColor }">
                    {{ item.year }}
                  </div>
                </div>
                <div class="pb-2 relative" style="width: 64%; border-left: 2px solid #cbd5e1; padding-left: 12px;">
                  <span class="font-bold" style="font-size: 11.5px !important; color: #333;">{{ item.name || item.info }}</span>
                </div>
                <button v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print absolute" style="right: -8px; top: -4px;">
                  <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Các mục khác (Mặc định) -->
            <div v-else class="space-y-2" style="padding-left: 4px;">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative text-[#333] leading-relaxed"
                style="font-size: 11px !important;">
                <div class="absolute rounded-full bg-slate-400" style="left: -18px; top: 7px; width: 6px; height: 6px;"></div>
                <div class="html-content font-medium text-justify" v-html="formatDesc(item.desc || item.info || item.name)"></div>
                <button v-if="selectedSectionId === section.id"
                  @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print absolute" style="right: -8px; top: -4px;">
                  <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>
      </div>
    </main>

    <!-- Page break markers -->
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

const cvRoot            = ref(null)
const pageCount         = ref(1)
const selectedSectionId = ref(null)

const props = defineProps({ resumeData: { type: Object, required: true } })
const emit  = defineEmits(['removeItem', 'moveUp', 'moveDown', 'moveHorizontal'])

const isEmpty = (val) => {
  if (!val) return true
  if (typeof val !== 'string') return false
  return val.replace(/<[^>]*>/g, '').trim() === ''
}

// Hàm chia dòng xịn xò
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

const templatePrimaryColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor
  return (!c || c.toLowerCase() === '#2b5c8f') ? '#465568' : c
})
const templateSecondaryColor = '#9db078'

const splitName = computed(() => {
  const raw   = props.resumeData.general.fullName || ''
  const clean = raw.replace(/<[^>]*>/g, '').replace(/&nbsp;|\u00a0/g, ' ').trim()
  const full  = clean || ''
  const parts = full.trim().split(' ')
  return parts.length > 1
    ? { last: parts[0], first: parts.slice(1).join(' ') }
    : { last: full, first: '' }
})

// ─── CONTACT ITEMS: Danh sách động có thể sắp xếp / ẩn ───
const contactIcons = {
  phone: '<path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z" />',
  email: '<path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z" /><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z" />',
  web: '<path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM4.332 8.027a6.012 6.012 0 011.912-2.706C6.512 5.73 6.974 6 7.5 6A1.5 1.5 0 019 7.5V8a2 2 0 004 0 2 2 0 011.523-1.943A5.977 5.977 0 0116 10c0 .34-.028.675-.083 1H15a2 2 0 00-2 2v2.197A5.973 5.973 0 0110 16v-2a2 2 0 00-2-2 2 2 0 01-2-2 2 2 0 00-1.668-1.973z" clip-rule="evenodd" />',
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

const moveSectionUp = (sectionId, col) => {
  emit('moveUp', sectionId, col === 'left' ? sidebarIds.value : mainIds.value)
  requestPagination()
}

const moveSectionDown = (sectionId, col) => {
  emit('moveDown', sectionId, col === 'left' ? sidebarIds.value : mainIds.value)
  requestPagination()
}

const moveSectionHorizontal = (sectionId, direction) => {
  emit('moveHorizontal', sectionId, direction)
  selectedSectionId.value = null
  requestPagination()
}

// ── Pagination Engine Bảo Mật Tuyệt Đối ──
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

  const pxPerMm   = cvRoot.value.offsetWidth / A4_W_MM
  const pageH     = A4_H_MM * pxPerMm
  const bottomSafeZone = 14 * pxPerMm
  const topMargin = 8 * pxPerMm

  const getOffsetTop = (el) => {
    let offset = 0, cur = el
    while (cur && cur !== cvRoot.value) { offset += cur.offsetTop; cur = cur.offsetParent }
    return offset
  }

  allElements.forEach((el) => {
    if(el.offsetHeight === 0) return
    const top = getOffsetTop(el)
    const topInPage = top % pageH
    const bottomInPage = topInPage + el.offsetHeight

    // Nếu element dài vượt quá cả 1 trang thì bỏ qua để không lỗi infinite loop
    if (el.offsetHeight > (pageH - bottomSafeZone - topMargin)) return

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

  const needed = Math.max(1, Math.ceil(maxBottom / pageH))
  if (pageCount.value !== needed) pageCount.value = needed
}

const hasData = (sec) => {
  if (sec.id === 'summary') return !isEmpty(props.resumeData.general.summary || '')
  return Array.isArray(sec.items) && sec.items.length > 0
}

const DEFAULT_VISIBLE_SIDEBAR = ['summary', 'skills']
const DEFAULT_VISIBLE_MAIN    = ['experience', 'education']
const SIDEBAR_IDS = ['summary', 'skills', 'languages', 'it_skills', 'certifications', 'awards', 'references', 'hobbies']
const MAIN_IDS    = ['education', 'experience', 'project', 'activities']

const handleOutsideClick = (e) => {
  if (!e.target.closest('.section-block')) {
    selectedSectionId.value = null
  }
}

onMounted(() => {
  if (props.resumeData?.sections) {
    props.resumeData.sections.forEach(sec => {
      if (sec.id === 'contact') return
      if (SIDEBAR_IDS.includes(sec.id)) sec.column = 'left'
      else if (MAIN_IDS.includes(sec.id)) sec.column = 'right'
      else if (!sec.column) sec.column = 'right'

      if (sec.isVisible === undefined) {
        if (DEFAULT_VISIBLE_SIDEBAR.includes(sec.id)) sec.isVisible = true
        else if (DEFAULT_VISIBLE_MAIN.includes(sec.id)) sec.isVisible = true
        else sec.isVisible = false
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

watch(() => props.resumeData, requestPagination, { deep: true })

const sidebarSections = computed(() => props.resumeData.sections.filter(s => s.column === 'left' && s.id !== 'contact'))
const mainSections = computed(() => props.resumeData.sections.filter(s => s.column === 'right'))
const sidebarIds = computed(() => sidebarSections.value.map(s => s.id))
const mainIds = computed(() => mainSections.value.map(s => s.id))

const getLevelPercent = (level) => {
  if (!level) return '75%'
  const l = String(level).toLowerCase().trim()
  if (l === 'cơ bản')    return '25%'
  if (l === 'trung cấp') return '50%'
  if (l === 'thành thạo') return '75%'
  if (l === 'chuyên gia') return '100%'
  if (l.includes('%'))   return l
  const num = parseInt(l)
  return isNaN(num) ? '75%' : num + '%'
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=Dancing+Script:wght@700&display=swap');

.font-cursive { font-family: 'Dancing Script', cursive !important; }

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

.item-container { position: relative; }

.delete-btn {
  width: 20px !important;
  height: 20px !important;
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  background: #ef4444 !important;
  color: white !important;
  border: none !important;
  border-radius: 50% !important;
  cursor: pointer !important;
  box-shadow: 0 2px 6px rgba(0,0,0,0.3) !important;
  z-index: 30 !important;
  transition: transform 0.12s ease !important;
}
.delete-btn:hover  { transform: scale(1.15) !important; }
.delete-btn:active { transform: scale(0.9)  !important; }

.section-block {
  position: relative !important;
  border: 2px solid transparent !important;
  border-radius: 0 !important;
  cursor: pointer !important;
  padding: 6px 10px !important;
  margin-top: -6px !important;
  margin-left: -10px !important;
  margin-right: -10px !important;
  transition:
    transform     0.2s cubic-bezier(0.34, 1.56, 0.64, 1),
    box-shadow    0.2s ease,
    border-color  0.15s ease,
    border-radius 0.15s ease,
    background    0.15s ease !important;
}

.section-block:hover { background: transparent !important; }

.section-active--sidebar {
  border: 2px solid rgba(255,255,255,0.5) !important;
  border-radius: 6px !important;
  /* removed scale */
  box-shadow: 0 6px 20px rgba(0,0,0,0.2), 0 1px 4px rgba(0,0,0,0.1) !important;
  background: rgba(255,255,255,0.07) !important;
  z-index: 10 !important;
}

.section-active--main {
  border: 2px solid #c8d4b8 !important;
  border-radius: 6px !important;
  /* removed scale */
  box-shadow: 0 4px 18px rgba(70,85,104,0.10), 0 1px 4px rgba(70,85,104,0.06) !important;
  background: rgba(220,228,205,0.10) !important;
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
  background: #2563eb !important;
  color: white !important;
  border: none !important;
  border-radius: 4px !important;
  cursor: pointer !important;
  box-shadow: 0 2px 6px rgba(37,99,235,0.4) !important;
  transition: background 0.12s, transform 0.1s !important;
}
.nav-btn:hover  { background: #1d4ed8 !important; }
.nav-btn:active { transform: scale(0.91) !important; }

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
  box-shadow: 0 2px 6px rgba(239,68,68,0.4) !important;
}
.nav-btn--delete:hover {
  background: #dc2626 !important;
}

.paginated-item { transition: none; }

:deep(.html-content)                                   { margin: 0 !important; padding: 0 !important; }
:deep(.html-content p)                                 { margin: 0 !important; padding: 0 !important; }
:deep(.html-content ul)                                { list-style-type: disc !important; padding-left: 1rem !important; margin: 0.2rem 0 !important; }
:deep(.html-content ol)                                { list-style-type: decimal !important; padding-left: 1rem !important; margin: 0.2rem 0 !important; }
:deep(.html-content b), :deep(.html-content strong)    { font-weight: 700 !important; }
:deep(.html-content i), :deep(.html-content em)        { font-style: italic !important; }
:deep(.html-content u)                                 { text-decoration: underline !important; }
:deep(.html-content ul li), :deep(.html-content ol li) { margin-bottom: 0.2rem !important; }

@media print {
  .no-print { display: none !important; }
  .section-block,
  .section-active--sidebar,
  .section-active--main {
    cursor: default !important;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
    transform: none !important;
    border-radius: 0 !important;
    outline: none !important;
  }
}

:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .section-active--sidebar),
:global(.is-exporting-pdf .section-active--main) {
  cursor: default !important;
  box-shadow: none !important;
  background: transparent !important;
  border-color: transparent !important;
  transform: none !important;
  border-radius: 0 !important;
  outline: none !important;
}
</style>
