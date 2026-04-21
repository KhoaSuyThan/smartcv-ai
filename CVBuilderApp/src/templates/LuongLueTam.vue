<template>
  <div id="cv-printable-area" ref="cvRoot"
    class="bg-white shadow-2xl w-[210mm] flex flex-row relative box-border text-[#333] leading-relaxed overflow-hidden"
    :style="{ height: `${Math.max(1, pageCount) * 297}mm`, fontFamily: `'Inter', sans-serif` }">

    <!-- ===== HEADER OVERLAY (nằm trên cả 2 cột) ===== -->
    <div class="absolute top-0 left-0 w-full z-30 pointer-events-none flex" style="height: 75mm;">

      <!-- Khối tên (nền xanh lá) -->
      <div class="flex flex-col justify-end pb-[6mm] pl-[10mm] pr-[6mm] pointer-events-auto shrink-0"
        style="width: 125mm; background-color: #9db078;">
        <!-- Họ (chữ lớn, in hoa, trắng/xám nhạt) -->
        <h1 class="uppercase font-bold text-white/80 leading-none tracking-[0.15em]"
          style="font-size: 38px !important;">
          {{ splitName.last }}
        </h1>
        <!-- Tên (chữ cursive, lớn hơn, trắng) -->
        <h1 class="font-cursive text-white leading-none tracking-wide"
          style="font-size: 58px; text-shadow: 1px 2px 4px rgba(0,0,0,0.12);">
          {{ splitName.first }}
        </h1>
        <!-- Vị trí ứng tuyển -->
        <div class="flex items-center gap-3 mt-3">
          <span class="font-bold uppercase tracking-[0.18em] text-[#334155]"
            style="font-size: 12px !important;"
            v-html="!isEmpty(resumeData.general.jobTitle) ? resumeData.general.jobTitle : 'Vị trí ứng tuyển'">
          </span>
          <div class="flex-1 h-[2px] bg-[#334155]/50 max-w-[100px]"></div>
        </div>
      </div>

      <!-- Khoảng trống giữa (nền đậm sidebar) không có gì -->
      <div class="flex-1 flex items-start justify-end pr-[8mm] pointer-events-auto" style="padding-top: 4mm;">
        <!-- Avatar -->
        <div class="bg-gray-200 border-[3px] border-white shadow-md overflow-hidden relative z-30"
          style="width: 46mm; height: 58mm;">
          <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl"
            class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center text-gray-400">
            <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1"
                d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path>
            </svg>
          </div>
        </div>
      </div>
    </div>

    <!-- ===== SIDEBAR TRÁI ===== -->
    <aside class="shrink-0 z-10 flex flex-col relative" style="width: 85mm;" :style="{ backgroundColor: templatePrimaryColor }">
      <!-- Đường dọc trắng bên trái nội dung sidebar -->
      <div class="absolute top-[78mm] bottom-[10mm] z-10"
        style="left: 8mm; width: 1.5px; background: rgba(255,255,255,0.55);"></div>

      <!-- Spacer cho vùng header -->
      <div style="height: 78mm; flex-shrink: 0;"></div>

      <!-- Nội dung sidebar -->
      <div class="flex flex-col flex-1 relative z-20" style="padding: 0 5mm 8mm 20mm;">

        <!-- ===== MỤC: LIÊN HỆ (cố định, không move) ===== -->
        <div class="section-block group mb-7 relative">
          <div class="paginated-item relative z-20">
            <!-- Dot marker -->
            <div class="absolute z-20 rounded-full bg-white shadow-[0_0_0_3px_var(--dot-color)]"
              :style="{ '--dot-color': templatePrimaryColor, left: '-12mm', top: '10px', width: '10px', height: '10px', transform: 'translateX(-50%)' }">
            </div>
            <!-- Tiêu đề -->
            <h3 class="font-bold uppercase px-3 py-1 inline-block mb-4 shadow-[3px_3px_0px_rgba(0,0,0,0.15)] tracking-wide"
              style="background: white; font-size: 13px !important; color: #465568;">
              Liên hệ
            </h3>
          </div>

          <div class="flex flex-col gap-3 paginated-item" style="font-size: 11px !important; color: rgba(255,255,255,0.9);">
            <!-- Điện thoại -->
            <div class="flex items-center gap-2" v-if="!isEmpty(resumeData.general.phone)">
              <div style="opacity: 0.8; flex-shrink: 0;">
                <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 20 20">
                  <path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z"></path>
                </svg>
              </div>
              <span class="font-semibold text-white tracking-wide break-all" v-html="resumeData.general.phone"></span>
            </div>
            <!-- Email -->
            <div class="flex items-center gap-2" v-if="!isEmpty(resumeData.general.email)">
              <div style="opacity: 0.8; flex-shrink: 0;">
                <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 20 20">
                  <path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z"></path>
                  <path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z"></path>
                </svg>
              </div>
              <span class="font-semibold text-white tracking-wide break-all" v-html="resumeData.general.email"></span>
            </div>
            <!-- Website / Github / LinkedIn -->
            <div class="flex items-center gap-2" v-if="!isEmpty(resumeData.general.website) || !isEmpty(resumeData.general.github) || !isEmpty(resumeData.general.linkedin)">
              <div style="opacity: 0.8; flex-shrink: 0;">
                <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM4.332 8.027a6.012 6.012 0 011.912-2.706C6.512 5.73 6.974 6 7.5 6A1.5 1.5 0 019 7.5V8a2 2 0 004 0 2 2 0 011.523-1.943A5.977 5.977 0 0116 10c0 .34-.028.675-.083 1H15a2 2 0 00-2 2v2.197A5.973 5.973 0 0110 16v-2a2 2 0 00-2-2 2 2 0 01-2-2 2 2 0 00-1.668-1.973z" clip-rule="evenodd"></path>
                </svg>
              </div>
              <span class="font-semibold text-white tracking-wide break-all"
                v-html="!isEmpty(resumeData.general.website) ? resumeData.general.website : (!isEmpty(resumeData.general.github) ? resumeData.general.github : resumeData.general.linkedin)">
              </span>
            </div>
            <!-- Địa chỉ -->
            <div class="flex items-start gap-2" v-if="!isEmpty(resumeData.general.address)">
              <div style="opacity: 0.8; flex-shrink: 0; margin-top: 1px;">
                <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd"></path>
                </svg>
              </div>
              <span class="font-semibold text-white tracking-wide break-words" v-html="resumeData.general.address"></span>
            </div>
          </div>
        </div>

        <!-- ===== CÁC SECTION SIDEBAR KHÁC (Mục tiêu, Kỹ năng,...) ===== -->
        <template v-for="section in sidebarSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block group mb-7 relative"
            :class="{ 'section-selected': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--sel-color': 'rgba(255,255,255,0.7)' } : {}"
            @mouseenter="showNav(section.id)"
            @mouseleave="hideNav()"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <!-- Nav buttons -->
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
            <div class="paginated-item relative z-20">
              <div class="absolute z-20 rounded-full bg-white"
                :style="{ boxShadow: `0 0 0 3px ${templatePrimaryColor}`, left: '-12mm', top: '10px', width: '10px', height: '10px', transform: 'translateX(-50%)' }">
              </div>
              <h3 class="font-bold uppercase px-3 py-1.5 inline-block mb-4 shadow-[3px_3px_0px_rgba(0,0,0,0.15)] tracking-wide break-words whitespace-normal"
                style="background: white; font-size: 13px !important; color: #465568;">
                {{ section.title }}
              </h3>
            </div>

            <!-- Nội dung: Mục tiêu (summary) -->
            <div v-if="section.id === 'summary'"
              class="leading-relaxed text-justify html-content font-medium paginated-item"
              style="font-size: 11px !important; color: rgba(255,255,255,0.9);"
              v-html="!isEmpty(resumeData.general.summary) ? resumeData.general.summary : 'Trình bày ngắn gọn từ 2-3 câu về số năm kinh nghiệm và công việc từng thực hiện bên cạnh những thành tựu có liên quan...'">
            </div>

            <!-- Nội dung: Kỹ năng / Ngôn ngữ / IT (có progress bar) -->
            <div v-else-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'"
              class="space-y-3">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="flex flex-col paginated-item item-container relative"
                style="font-size: 11px !important; color: white;">
                <div class="flex items-center gap-2 mb-1">
                  <div class="rounded-full bg-white shrink-0" style="width: 6px; height: 6px;"></div>
                  <span class="font-bold tracking-wide">{{ item.name }}</span>
                </div>
                <!-- Progress bar -->
                <div class="w-full rounded-full overflow-hidden relative" style="height: 5px; background: rgba(255,255,255,0.2);">
                  <div class="absolute left-0 top-0 bottom-0 rounded-full"
                    :style="{ width: getLevelPercent(item.level), backgroundColor: templateSecondaryColor }"></div>
                </div>
                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print bg-red-500 text-white rounded-full shadow-sm z-30 opacity-0 group-hover:opacity-100 transition-opacity absolute"
                  style="right: 0; top: 0;">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Nội dung: Các section khác -->
            <div v-else class="space-y-2">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="font-medium leading-relaxed item-container relative paginated-item"
                style="font-size: 11px !important; color: rgba(255,255,255,0.9);">
                <div class="flex items-start gap-2">
                  <div class="rounded-full bg-white shrink-0" style="width: 6px; height: 6px; margin-top: 6px;"></div>
                  <span class="html-content" style="color: rgba(255,255,255,0.9);" v-html="formatDesc(item.desc || item.name || item.info)"></span>
                </div>
                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print bg-red-500 text-white rounded-full shadow-sm z-30 opacity-0 group-hover:opacity-100 transition-opacity absolute"
                  style="right: 0; top: 0;">
                  <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>
          </div>
        </template>

      </div>
    </aside>

    <!-- ===== MAIN CONTENT PHẢI ===== -->
    <main class="flex-1 flex flex-col relative bg-white z-20" style="min-height: max-content;"
      @click.self="selectedSectionId = null">

      <!-- Spacer header -->
      <div style="height: 78mm; flex-shrink: 0; border-left: 1.5px solid #e2e8f0; margin-left: 12mm;"></div>

      <!-- Nội dung main -->
      <div class="flex-1 flex flex-col pb-[12mm] relative gap-2" style="padding-left: 10mm; padding-right: 12mm;">

        <template v-for="section in mainSections" :key="section.id">
          <div
            v-show="section.isVisible"
            class="section-block group mb-6"
            :class="{ 'section-selected': selectedSectionId === section.id }"
            :style="selectedSectionId === section.id ? { '--sel-color': templateSecondaryColor } : {}"
            @mouseenter="showNav(section.id)"
            @mouseleave="hideNav()"
            @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
          >
            <!-- Nav buttons -->
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

            <!-- Tiêu đề section (dot tròn lớn + label màu) -->
            <div class="flex items-center gap-3 mb-5 paginated-item" style="margin-left: -22px;">
              <!-- Dot tròn lớn có viền -->
              <div class="rounded-full flex items-center justify-center shrink-0"
                :style="{ width: '34px', height: '34px', border: `4px solid ${templateSecondaryColor}`, backgroundColor: '#dce4cd' }">
                <div class="rounded-full" :style="{ width: '10px', height: '10px', backgroundColor: templatePrimaryColor }"></div>
              </div>
              <!-- Label màu xanh lá -->
              <h3 class="font-bold uppercase text-white tracking-widest shadow-[3px_3px_0px_rgba(0,0,0,0.15)]"
                style="font-size: 14px !important; padding: 6px 18px;"
                :style="{ backgroundColor: templateSecondaryColor }">
                {{ section.title }}
              </h3>
            </div>

            <!-- Items: Học vấn / Kinh nghiệm / Dự án / Hoạt động -->
            <div v-if="section.id === 'education' || section.id === 'experience' || section.id === 'project' || section.id === 'activities'"
              class="space-y-5">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative flex items-stretch paginated-item">

                <!-- Cột trái: tên trường/công ty + năm -->
                <div class="pr-3 relative shrink-0" style="width: 36%;">
                  <div class="absolute rounded-full bg-slate-400" style="left: -14px; top: 7px; width: 7px; height: 7px;"></div>
                  <h4 class="font-bold leading-snug mb-1" style="font-size: 11.5px !important; color: #222;">
                    <span v-html="section.id === 'education' ? item.school : (section.id === 'experience' ? item.company : item.name)"></span>
                  </h4>
                  <div v-if="item.year || item.time"
                    class="inline-block text-white font-bold tracking-wide shadow-[2px_2px_0px_rgba(0,0,0,0.15)]"
                    style="font-size: 10px !important; padding: 2px 8px; margin-top: 3px;"
                    :style="{ backgroundColor: templateSecondaryColor }">
                    {{ item.year || item.time }}
                  </div>
                </div>

                <!-- Cột phải: chuyên ngành/chức vụ + mô tả -->
                <div class="pb-2 relative" style="width: 64%; border-left: 2px solid #cbd5e1; padding-left: 12px;">
                  <h4 class="font-bold mb-1" style="font-size: 11.5px !important; color: #333;"
                    v-if="item.major || item.role">
                    {{ item.major || item.role }}
                  </h4>
                  <div class="text-slate-700 html-content leading-relaxed text-justify font-medium"
                    style="font-size: 11px !important;"
                    v-html="formatDesc(item.desc)">
                  </div>
                </div>

                <!-- Nút xóa item -->
                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print bg-red-500 text-white rounded-full shadow-md z-30 absolute"
                  style="right: -8px; top: -4px;">
                  <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Items: Giải thưởng / Chứng chỉ -->
            <div v-else-if="section.id === 'awards' || section.id === 'certifications'"
              class="space-y-4">
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
                  <span class="font-bold html-content" style="font-size: 11.5px !important; color: #333;">{{ item.name || item.info }}</span>
                </div>
                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print bg-red-500 text-white rounded-full shadow-md z-30 absolute"
                  style="right: -8px; top: -4px;">
                  <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

            <!-- Items mặc định -->
            <div v-else class="space-y-3" style="padding-left: 4px;">
              <div v-for="(item, itemIndex) in section.items" :key="item._refId"
                class="item-container relative text-[#333] leading-relaxed paginated-item"
                style="font-size: 11px !important;">
                <div class="absolute rounded-full bg-slate-400" style="left: -18px; top: 7px; width: 6px; height: 6px;"></div>
                <div class="html-content font-medium text-justify"
                  v-html="formatDesc(item.desc || item.info || item.name)">
                </div>
                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)"
                  class="delete-btn no-print bg-red-500 text-white rounded-full shadow-md z-30 absolute"
                  style="right: -8px; top: -4px;">
                  <svg width="9" height="9" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
              </div>
            </div>

          </div>
        </template>

      </div>
    </main>


    <!-- ===== ĐƯỜNG NGẮT TRANG (chỉ hiển thị trên web) ===== -->
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

const cvRoot = ref(null);
const pageCount = ref(1);

// ===== PHÂN TRANG =====
let paginateTimer = null;
const requestPagination = () => {
  if (paginateTimer) clearTimeout(paginateTimer);
  paginateTimer = setTimeout(doPagination, 50);
};

const doPagination = async () => {
  if (!cvRoot.value) return;

  const items = cvRoot.value.querySelectorAll('.paginated-item');
  items.forEach(el => { el.style.marginTop = ''; });

  await nextTick();
  await new Promise(r => setTimeout(r, 50));

  const A4_WIDTH_MM = 210;
  const A4_HEIGHT_MM = 297;
  const MARGIN_BOTTOM_MM = 18; // Lề dưới đủ để content dừng trước viền đáy (12mm) + khoảng trống
  const MARGIN_TOP_MM = 15;

  const rootWidthPx = cvRoot.value.offsetWidth;
  const pxPerMm = rootWidthPx / A4_WIDTH_MM;
  const pageHeightPx = A4_HEIGHT_MM * pxPerMm;
  const marginBottomPx = MARGIN_BOTTOM_MM * pxPerMm;
  const marginTopPx = MARGIN_TOP_MM * pxPerMm;
  const safeBottomPx = pageHeightPx - marginBottomPx;

  const getRelativeTop = (el) => {
    let offset = 0;
    let currentEl = el;
    while (currentEl && currentEl !== cvRoot.value) {
      offset += currentEl.offsetTop;
      currentEl = currentEl.offsetParent;
    }
    return offset;
  };

  const processColumn = (colSelector) => {
    const col = cvRoot.value.querySelector(colSelector);
    if (!col) return;
    const colItems = col.classList.contains('paginated-item')
      ? [col]
      : col.querySelectorAll('.paginated-item');

    let isStable = false;
    let attempts = 0;
    while (!isStable && attempts < 100) {
      isStable = true;
      attempts++;
      for (let i = 0; i < colItems.length; i++) {
        const item = colItems[i];
        // Bỏ qua phần tử ẩn hoặc đã xóa
        if (!item || item.offsetHeight === 0) continue;
        const top = getRelativeTop(item);
        const height = item.offsetHeight;
        const bottom = top + height;
        const currentPageIndex = Math.floor(top / pageHeightPx);
        const currentSafeBottom = (currentPageIndex * pageHeightPx) + safeBottomPx;
        if (bottom > currentSafeBottom && top < (currentPageIndex + 1) * pageHeightPx) {
          if (height > (pageHeightPx - marginTopPx - marginBottomPx)) continue;
          const targetTop = (currentPageIndex + 1) * pageHeightPx + marginTopPx;
          const pushAmount = targetTop - top;
          const currentMt = parseFloat(item.style.marginTop || '0');
          item.style.marginTop = (currentMt + pushAmount) + 'px';
          isStable = false;
          break;
        }
      }
    }
  };

  processColumn('main');
  processColumn('aside');

  // Tính maxBottom, bỏ qua các phần tử ẩn (offsetHeight=0)
  let maxBottom = 0;
  items.forEach(el => {
    if (el.offsetHeight === 0) return;
    const top = getRelativeTop(el);
    const bottom = top + el.offsetHeight;
    if (bottom > maxBottom) maxBottom = bottom;
  });

  const calculatedPageCount = Math.max(1, Math.ceil((maxBottom + marginBottomPx - 2) / pageHeightPx));
  if (pageCount.value !== calculatedPageCount) {
    pageCount.value = calculatedPageCount;
  }
};

watch(() => props.resumeData, () => { requestPagination(); }, { deep: true });

onMounted(() => {
  // ===== ÉP ĐÚNG CÁC SECTION THEO ẢNH MẪU =====
  // Chỉ active: summary, skills (sidebar trái) + education, experience (main phải)
  // Tất cả section còn lại -> isVisible = false
  const SIDEBAR_ACTIVE = ['summary', 'skills'];
  const MAIN_ACTIVE = ['education', 'experience'];

  if (props.resumeData && props.resumeData.sections) {
    props.resumeData.sections.forEach(sec => {
      if (SIDEBAR_ACTIVE.includes(sec.id)) {
        sec.isVisible = true;
        sec.column = 'left';
      } else if (MAIN_ACTIVE.includes(sec.id)) {
        sec.isVisible = true;
        sec.column = 'right';
      } else if (sec.id !== 'contact') {
        // Ẩn tất cả section không có trong mẫu (trừ contact luôn cố định)
        sec.isVisible = false;
      }
    });
  }
  // =============================================

  requestPagination();
  window.addEventListener('resize', requestPagination);
  document.addEventListener('keyup', requestPagination);
});

onUnmounted(() => {
  window.removeEventListener('resize', requestPagination);
  document.removeEventListener('keyup', requestPagination);
  if (paginateTimer) clearTimeout(paginateTimer);
});
// ===== END PHÂN TRANG =====

const isEmpty = (val) => {
  if (!val) return true;
  if (typeof val !== 'string') return false;
  return val.replace(/<[^>]*>/g, '').trim() === '';
};

const props = defineProps({
  resumeData: { type: Object, required: true }
});

const emit = defineEmits(['moveUp', 'moveDown', 'moveHorizontal', 'removeItem']);

// Hover/Select tracking
const hoveredSectionId = ref(null);
const selectedSectionId = ref(null);
let _hideTimer = null;

const showNav = (id) => {
  if (_hideTimer) { clearTimeout(_hideTimer); _hideTimer = null; }
  hoveredSectionId.value = id;
};

const hideNav = () => {
  _hideTimer = setTimeout(() => {
    hoveredSectionId.value = null;
    _hideTimer = null;
  }, 120);
};

// Tách họ / tên từ họ và tên đầy đủ
const splitName = computed(() => {
  const raw = props.resumeData.general.fullName || '';
  const clean = raw.replace(/<[^>]*>/g, '').replace(/&nbsp;/g, ' ').replace(/\u00a0/g, ' ').trim();
  const full = clean || 'LƯƠNG Tuệ Lâm';
  const parts = full.trim().split(' ');
  if (parts.length > 1) {
    return { last: parts[0], first: parts.slice(1).join(' ') };
  }
  return { last: full, first: '' };
});

// Màu theme
const templatePrimaryColor = computed(() => {
  const c = props.resumeData?.theme?.primaryColor;
  if (!c || c.toLowerCase() === '#2b5c8f') return '#465568';
  return c;
});

const templateSecondaryColor = '#9db078';

// Sections
const sidebarSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'left' && s.id !== 'contact')
);
const mainSections = computed(() =>
  props.resumeData.sections.filter(s => s.column === 'right')
);
const sidebarIds = computed(() => sidebarSections.value.map(s => s.id));
const mainIds = computed(() => mainSections.value.map(s => s.id));

// Progress bar level
const getLevelPercent = (level) => {
  if (!level) return '75%';
  const l = String(level).toLowerCase().trim();
  if (l === 'cơ bản') return '25%';
  if (l === 'trung cấp') return '50%';
  if (l === 'thành thạo') return '75%';
  if (l === 'chuyên gia') return '100%';
  if (l.includes('%')) return l;
  const num = parseInt(l);
  if (!isNaN(num)) return num + '%';
  return '75%';
};

// Format rich text
const formatDesc = (text) => {
  if (!text) return '';
  if (/<[a-z][\s\S]*>/i.test(text)) {
    if (text.includes('<li') && (text.includes('<font') || text.includes('style='))) {
      try {
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = text;
        tempDiv.querySelectorAll('li').forEach(li => {
          const child = li.firstElementChild;
          if (child && (child.tagName === 'FONT' || child.tagName === 'SPAN')) {
            if (child.color) li.style.color = child.color;
            if (child.style && child.style.color) li.style.color = child.style.color;
          }
        });
        return tempDiv.innerHTML;
      } catch (e) { return text; }
    }
    return text;
  }
  return text.split('\n').map(l => l.trim()).filter(l => l).join('<br/>');
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=Dancing+Script:wght@700&display=swap');

.font-cursive {
  font-family: 'Dancing Script', cursive !important;
}

#cv-printable-area {
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  overflow-wrap: anywhere;
}

/* ===== ITEM CONTAINER + DELETE BTN ===== */
.item-container {
  position: relative;
  transition: all 0.2s;
}

.delete-btn {
  opacity: 0;
  transition: all 0.2s;
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}

.item-container:hover .delete-btn {
  opacity: 1;
}

/* ===== HTML CONTENT ===== */
:deep(.html-content ul) {
  list-style-type: disc !important;
  padding-left: 1rem !important;
  margin-top: 0.2rem;
  margin-bottom: 0.2rem;
}
:deep(.html-content ol) {
  list-style-type: decimal !important;
  padding-left: 1rem !important;
  margin-top: 0.2rem;
  margin-bottom: 0.2rem;
}
:deep(.html-content b), :deep(.html-content strong) { font-weight: 700; }
:deep(.html-content i), :deep(.html-content em) { font-style: italic; }
:deep(.html-content u) { text-decoration: underline; }
:deep(.html-content ul li), :deep(.html-content ol li) { margin-bottom: 0.2rem; }

/* ===== SECTION BLOCKS ===== */
.section-block {
  position: relative;
  border: 2px solid transparent;
  border-radius: 2px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.section-block:hover {
  background: rgba(0, 0, 0, 0.02);
}

.section-selected {
  outline: 1.5px solid var(--sel-color, #9db078);
  outline-offset: 3px;
  border-radius: 2px;
}

/* ===== NAV BUTTONS ===== */
.nav-btns {
  position: absolute;
  right: 5px;
  top: -14px;
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

/* ===== PRINT ===== */
@media print {
  .no-print { display: none !important; }
  .section-block, .section-selected {
    cursor: default;
    box-shadow: none !important;
    background: transparent !important;
    border-color: transparent !important;
    outline: none !important;
    padding: 0 !important;
  }
}

:global(.is-exporting-pdf .no-print) { display: none !important; }
:global(.is-exporting-pdf .section-block),
:global(.is-exporting-pdf .section-selected) {
  cursor: default !important;
  box-shadow: none !important;
  background: transparent !important;
  border-color: transparent !important;
  outline: none !important;
}
</style>