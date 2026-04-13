<template>
  <div id="cv-printable-area" class="bg-white shadow-2xl w-[210mm] min-h-[297mm] h-max flex flex-col relative box-border text-[#333] leading-relaxed" style="font-family: 'Inter', 'Segoe UI', sans-serif;">
    
    <!-- TRANG TRÍ GÓC TRÁI TRÊN (Khối dọc ôm Avatar và thanh xanh) -->
    <div class="absolute top-0 left-0 w-[60mm] h-[60mm] z-0" :style="{ backgroundColor: (resumeData.theme.primaryColor || '#2d7fb2') + '15' }"></div>
    <div class="absolute top-0 left-0 w-[12mm] h-[80mm] z-0" :style="{ backgroundColor: resumeData.theme.primaryColor || '#2d7fb2' }"></div>
    <div class="absolute top-0 left-0 w-[60mm] h-[4mm] z-0" :style="{ backgroundColor: resumeData.theme.primaryColor || '#2d7fb2' }"></div>

    <!-- THANH TRANG TRÍ DỌC BÊN PHẢI (Mẫu mới) -->
    <div class="absolute bottom-0 right-0 w-[7mm] h-[150mm] z-0" :style="{ backgroundColor: resumeData.theme.primaryColor || '#2d7fb2' }"></div>

    <!-- HEADER BLOCK -->
    <header class="relative z-10 pt-[15mm] px-[15mm] pb-[8mm] flex gap-12 items-center">
        <!-- Avatar Section -->
        <div class="relative ml-4">
            <div class="w-[52mm] h-[52mm] rounded-full border-[8px] border-white shadow-2xl overflow-hidden bg-slate-100 flex-shrink-0 relative z-10 ring-1 ring-slate-100">
                <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
                <div v-else class="w-full h-full flex items-center justify-center text-slate-300">
                    <svg class="w-24 h-24" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
                </div>
            </div>
            <!-- Vòng trang trí mảnh bên ngoài -->
            <div class="absolute -inset-3 rounded-full border-2 border-slate-50 z-0"></div>
        </div>

        <!-- Name & Info Section -->
        <div class="flex-1">
            <h1 class="text-[42px] font-black uppercase tracking-tight mb-0.5 leading-none text-slate-900">
                {{ resumeData.general.fullName || 'HỌ VÀ TÊN' }}
            </h1>
            <h2 class="text-[14px] font-extrabold text-slate-600 uppercase tracking-[0.25em] mb-7">
                {{ resumeData.general.jobTitle || 'Vị trí ứng tuyển' }}
            </h2>

            <!-- Contact List với Icon tròn như ảnh -->
            <div class="space-y-3 text-[13.5px] font-bold text-slate-700">
                <div class="flex items-center gap-4" v-if="resumeData.general.birthDate">
                   <div class="w-6 h-6 rounded-full flex items-center justify-center text-white shrink-0" :style="{ backgroundColor: resumeData.theme.primaryColor || '#2d7fb2' }">
                       <svg class="w-3 h-3" fill="currentColor" viewBox="0 0 20 20"><path d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z"></path></svg>
                   </div>
                   <span>{{ resumeData.general.birthDate }}</span>
                </div>
                <div class="flex items-center gap-4" v-if="resumeData.general.phone">
                   <div class="w-6 h-6 rounded-full flex items-center justify-center text-white shrink-0" :style="{ backgroundColor: resumeData.theme.primaryColor || '#2d7fb2' }">
                       <svg class="w-3 h-3" fill="currentColor" viewBox="0 0 20 20"><path d="M2 3a1 1 0 011-1h2.153a1 1 0 01.986.836l.74 4.435a1 1 0 01-.54 1.06l-1.548.773a11.037 11.037 0 006.105 6.105l.774-1.548a1 1 0 011.059-.54l4.435.74a1 1 0 01.836.986V17a1 1 0 01-1 1h-2C7.82 18 2 12.18 2 5V3z"></path></svg>
                   </div>
                   <span>{{ resumeData.general.phone }}</span>
                </div>
                <div class="flex items-center gap-4" v-if="resumeData.general.email">
                   <div class="w-6 h-6 rounded-full flex items-center justify-center text-white shrink-0" :style="{ backgroundColor: resumeData.theme.primaryColor || '#2d7fb2' }">
                       <svg class="w-3 h-3" fill="currentColor" viewBox="0 0 20 20"><path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z"></path><path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z"></path></svg>
                   </div>
                   <span>{{ resumeData.general.email }}</span>
                </div>
                <div class="flex items-center gap-4" v-if="resumeData.general.address">
                   <div class="w-6 h-6 rounded-full flex items-center justify-center text-white shrink-0" :style="{ backgroundColor: resumeData.theme.primaryColor || '#2d7fb2' }">
                       <svg class="w-3 h-3" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd"></path></svg>
                   </div>
                   <span>{{ resumeData.general.address }}</span>
                </div>
            </div>
        </div>
    </header>

    <!-- CONTENT BODY - 2 COLUMNS -->
    <div class="flex flex-1 px-[15mm] py-[2mm] gap-[10mm] relative z-10 mb-8" @click.self="selectedSectionId = null">
        
        <!-- CỘT TRÁI (SIDEBAR) -->
        <aside class="w-[68mm] flex flex-col gap-9 pr-4 pl-6">
            <template v-for="section in sidebarSections" :key="section.id">
                <div
                    v-show="section.isVisible"
                    class="section-block"
                    :class="{ 'section-selected': selectedSectionId === section.id }"
                    :style="selectedSectionId === section.id ? { '--sel-color': resumeData.theme.primaryColor || '#2d7fb2' } : {}"
                    @mouseenter="showNav(section.id)"
                    @mouseleave="hideNav()"
                    @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
                >
                    <!-- Nút điều hướng -->
                    <div
                        v-show="hoveredSectionId === section.id || selectedSectionId === section.id"
                        class="nav-btns no-print"
                        @mouseenter="showNav(section.id)"
                        @mouseleave="hideNav()"
                    >
                        <button @click.stop.prevent="$emit('moveUp', section.id, sidebarIds)" class="nav-btn" title="Di chuyển lên">
                            <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                        </button>
                        <button @click.stop.prevent="$emit('moveDown', section.id, sidebarIds)" class="nav-btn" title="Di chuyển xuống">
                            <svg width="12" height="12" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                        </button>
                    </div>

                    <h3 class="text-[17px] font-black uppercase mb-4 tracking-tight" :style="{ color: resumeData.theme.primaryColor || '#2d7fb2' }">
                        {{ section.title }}
                    </h3>
                    
                    <div class="space-y-3">
                        <!-- Danh sách Kỹ năng/Ngôn ngữ (Dạng gạch đầu dòng) -->
                        <div v-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'" class="space-y-2">
                           <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[13.5px] text-slate-800 font-medium item-container pr-8 min-h-[22px] flex items-center">
                               - {{ item.name }} <span v-if="item.level" class="text-slate-500 font-normal ml-1">({{ item.level }})</span>
                               
                               <!-- Nút xóa item -->
                               <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30">
                                   <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                               </button>
                           </div>
                        </div>

                        <!-- Giải thưởng/Chứng chỉ -->
                        <div v-else-if="section.id === 'awards' || section.id === 'certifications'" class="space-y-3">
                           <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[13.5px] item-container">
                               <p class="font-bold text-slate-800">- {{ item.name }}</p>
                               <p v-if="item.year" class="text-[12px] text-slate-500 italic ml-3">{{ item.year }}</p>

                               <!-- Nút xóa item -->
                               <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30">
                                   <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                               </button>
                           </div>
                        </div>

                        <!-- Sở thích -->
                        <div v-else-if="section.id === 'hobbies'" class="space-y-1.5">
                            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="text-[13.5px] text-slate-800 font-medium item-container">
                                - {{ item.name }}
                                <!-- Nút xóa item -->
                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-sm z-30">
                                    <svg width="8" height="8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                            </div>
                        </div>

                        <!-- Người tham chiếu -->
                        <div v-else-if="section.id === 'references'" class="space-y-4">
                            <div v-for="item in section.items" :key="item._refId" class="text-[13.5px] text-slate-800 leading-relaxed font-bold whitespace-pre-line border-l-2 pl-3" :style="{ borderLeftColor: (resumeData.theme.primaryColor || '#2d7fb2') + '40' }">
                                {{ item.info }}
                            </div>
                        </div>

                         <!-- Các mục mặc định cho Sidebar -->
                        <div v-else class="space-y-2">
                           <div v-for="item in section.items" :key="item._refId" class="text-[13.5px] text-slate-800 whitespace-pre-line font-medium leading-relaxed">
                               - {{ item.desc || item.name || item.info }}
                           </div>
                        </div>
                    </div>
                </div>
            </template>
        </aside>

        <!-- CỘT PHẢI (MAIN CONTENT) -->
        <main class="flex-1 flex flex-col gap-10">
            <template v-for="section in mainSections" :key="section.id">
                <div
                    v-show="section.isVisible"
                    class="section-block"
                    :class="{ 'section-selected': selectedSectionId === section.id }"
                    :style="selectedSectionId === section.id ? { '--sel-color': resumeData.theme.primaryColor || '#2d7fb2' } : {}"
                    @mouseenter="showNav(section.id)"
                    @mouseleave="hideNav()"
                    @click.stop="selectedSectionId = selectedSectionId === section.id ? null : section.id"
                >
                    <!-- Nút điều hướng main column -->
                    <div
                        v-show="hoveredSectionId === section.id || selectedSectionId === section.id"
                        class="nav-btns nav-btns--left no-print"
                        @mouseenter="showNav(section.id)"
                        @mouseleave="hideNav()"
                    >
                        <button @click.stop.prevent="$emit('moveUp', section.id, mainIds)" class="nav-btn" title="Di chuyển lên">
                            <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 15l7-7 7 7"/></svg>
                        </button>
                        <button @click.stop.prevent="$emit('moveDown', section.id, mainIds)" class="nav-btn" title="Di chuyển xuống">
                            <svg width="14" height="14" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19 9l-7 7-7-7"/></svg>
                        </button>
                    </div>

                    <h3 class="text-[18px] font-black uppercase mb-4 tracking-tight" :style="{ color: resumeData.theme.primaryColor || '#2d7fb2' }">
                        {{ section.title }}
                    </h3>

                    <div class="space-y-8">
                        <!-- Trường hợp đặc biệt: Mục tiêu nghề nghiệp -->
                        <div v-if="section.id === 'summary'" class="text-[13.5px] leading-[1.8] text-slate-800 text-justify font-medium">
                            {{ resumeData.general.summary || 'Chưa có thông tin mục tiêu nghề nghiệp.' }}
                        </div>

                        <!-- Kinh nghiệm / Dự án / Hoạt động -->
                        <div v-else-if="section.id === 'experience' || section.id === 'project' || section.id === 'activities'" class="space-y-8">
                            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container p-2 -m-2 rounded-lg">
                                <!-- Nút xóa item -->
                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 scale-125">
                                    <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>

                                <h4 class="font-bold text-[15.5px] text-slate-900 leading-tight mb-0.5">{{ section.id === 'experience' ? item.company : (item.name || 'Tên hoạt động') }}</h4>
                                <div class="flex justify-between items-baseline mb-1.5">
                                    <span class="text-[13.5px] font-bold text-slate-600 italic">{{ section.id === 'experience' ? item.role : (item.role || 'Thành viên') }}</span>
                                    <span class="text-[12.5px] font-extrabold text-slate-500 uppercase">{{ item.time }}</span>
                                </div>
                                <div class="text-[13.5px] leading-[1.75] text-slate-700 text-justify whitespace-pre-line font-medium" v-html="formatDesc(item.desc)"></div>
                            </div>
                        </div>

                        <!-- Học Vấn -->
                        <div v-else-if="section.id === 'education'" class="space-y-7">
                            <div v-for="(item, itemIndex) in section.items" :key="item._refId" class="item-container p-2 -m-2 rounded-lg">
                                <!-- Nút xóa item -->
                                <button @click.stop.prevent="$emit('removeItem', section.id, itemIndex)" class="delete-btn bg-red-500 text-white rounded-full no-print shadow-md z-30 scale-125">
                                    <svg width="10" height="10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>

                                <div class="font-bold text-[15.5px] text-slate-900 mb-0.5">{{ item.school }}</div>
                                <div class="flex justify-between items-baseline mb-1">
                                    <span class="text-[13px] font-extrabold text-slate-500 uppercase italic">Thời gian: {{ item.year }}</span>
                                </div>
                                <div class="text-[13.5px] font-bold text-slate-700">Chuyên ngành: {{ item.major }}</div>
                                <div v-if="item.gradType" class="text-[13px] text-slate-600">Xếp loại tốt nghiệp: <span class="font-bold underline">{{ item.gradType }}</span></div>
                            </div>
                        </div>
                    </div>
                </div>
            </template>
        </main>
    </div>

  </div>
</template>

<script setup>
import { computed, ref } from 'vue'

const props = defineProps({
    resumeData: { type: Object, required: true }
})

const emit = defineEmits(['moveUp', 'moveDown'])

// Hover tracking với delayed-hide pattern (tránh race condition giữa mouseleave và mouseenter)
const hoveredSectionId = ref(null)
let _hideTimer = null

const showNav = (id) => {
    if (_hideTimer) { clearTimeout(_hideTimer); _hideTimer = null; }
    hoveredSectionId.value = id;
}

const hideNav = () => {
    _hideTimer = setTimeout(() => {
        hoveredSectionId.value = null;
        _hideTimer = null;
    }, 120);
}

// Selected section (highlight khi click)
const selectedSectionId = ref(null)

// Các ID phần mục được chỉ định hiển thị ở Sidebar hoặc Main
const sidebarIds = ['skills', 'it_skills', 'languages', 'awards', 'certifications', 'hobbies', 'references', 'additional'];
const mainIds = ['summary', 'education', 'experience', 'activities', 'project'];

const sidebarSections = computed(() => {
    return props.resumeData.sections.filter(s => sidebarIds.includes(s.id));
});

const mainSections = computed(() => {
    return props.resumeData.sections.filter(s => mainIds.includes(s.id));
});

const formatDesc = (text) => {
    if (!text) return '';
    return text.split('\n').map(l => l.trim()).filter(l=>l).join('<br/>');
}
</script>

<style scoped>
.item-container {
    position: relative;
    transition: all 0.2s;
}

.delete-btn {
    opacity: 0;
    transition: all 0.2s;
    position: absolute;
    top: 2px;
    right: 2px;
    width: 22px;
    height: 22px;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    border: 2px solid white;
}

.item-container:hover .delete-btn {
    opacity: 1;
}

.item-container:hover {
    background-color: rgba(239, 68, 68, 0.05); /* Phớt đỏ nhẹ để biết có thể xóa */
}
</style>


<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&display=swap');

#cv-printable-area {
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
}

/* === SECTION BLOCKS === */
.section-block {
    position: relative;
    border-radius: 6px;
    padding: 6px 8px;
    margin: -6px -8px;
    cursor: pointer;
    transition: box-shadow 0.15s ease, background 0.15s ease;
}

/* Hover: viền nhẹ */
.section-block:hover {
    background: rgba(0, 0, 0, 0.015);
}

/* Selected: border đậm màu theme + shadow nổi bật */
.section-selected {
    box-shadow:
        0 0 0 2px var(--sel-color, #2563eb),
        0 4px 16px rgba(0, 0, 0, 0.08);
    background: color-mix(in srgb, var(--sel-color, #2563eb) 5%, white) !important;
}

/* === NAV BUTTONS (MOVE UP/DOWN) === */
.nav-btns {
    position: absolute;
    right: 4px;
    top: 4px; /* Đưa vào bên trong góc trên bên phải */
    display: flex;
    flex-direction: row;
    gap: 4px;
    z-index: 9999;
}

.nav-btns--left {
    right: auto;
    left: 4px;
}

.nav-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 5px;
    background: #2563eb;
    color: white;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    box-shadow: 0 2px 8px rgba(37, 99, 235, 0.4);
    transition: background 0.15s ease, transform 0.1s ease;
}

.nav-btn:hover {
    background: #1d4ed8;
    transform: scale(1.1);
}

.nav-btn:active {
    transform: scale(0.95);
}

/* Ẩn khi in */
@media print {
    .no-print {
        display: none !important;
    }
    .section-block {
        cursor: default;
        box-shadow: none !important;
        background: transparent !important;
        padding: 0 !important;
        margin: 0 !important;
        border-radius: 0 !important;
    }
}
</style>
