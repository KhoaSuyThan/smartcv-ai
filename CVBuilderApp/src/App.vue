<template>
  <div class="w-full bg-slate-50 flex font-sans text-slate-800 cv-builder-container relative" style="height: 100%; overflow: hidden;">
    <!-- NÚT ẨN HIỆN SIDEBAR (GRIP HANDLE) -->
    <div 
        class="absolute top-7 z-40 flex items-center transition-all duration-500 ease-in-out" 
        :style="{ left: isPreviewMode ? '0' : '700px' }"
    >
        <button 
           @click="isPreviewMode = !isPreviewMode"
           class="w-6 h-10 bg-slate-900 border border-slate-700 border-l-0 shadow-xl rounded-r-lg flex items-center justify-center text-slate-400 hover:text-white hover:bg-slate-800 transition-colors cursor-pointer group"
           title="Ẩn/Hiện cột soạn thảo"
        >
            <svg v-if="!isPreviewMode" class="w-4 h-4 group-hover:-translate-x-0.5 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15 19l-7-7 7-7"></path></svg>
            <svg v-else class="w-4 h-4 group-hover:translate-x-0.5 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"></path></svg>
        </button>
    </div>

    <!-- CỘT TRÁI: EDITOR PANEL (STICKY) -->
    <div :class="[
      'cv-builder-editor-panel bg-white border-r border-slate-200 shadow-[0_0_20px_rgba(0,0,0,0.05)] z-20 flex flex-col shrink-0 overflow-hidden transition-all duration-500 ease-in-out',
      isPreviewMode ? 'w-0 opacity-0 border-r-0' : 'w-[700px] opacity-100'
    ]" style="height: 100%;">
      <!-- HEADER -->
      <div class="py-2 border-b border-slate-100 bg-slate-900 text-white shrink-0 relative overflow-hidden">
        <div class="absolute top-0 right-0 -mr-8 -mt-8 w-32 h-32 bg-blue-500 rounded-full opacity-20 blur-2xl"></div>
        <div class="flex flex-col gap-2">
            <div class="flex items-center justify-between bg-white/10 rounded-lg p-2 backdrop-blur-sm border border-white/5">
                <div class="flex items-center gap-2">
                    <div class="w-2.5 h-2.5 rounded-full" :class="isSaving ? 'bg-yellow-400 animate-pulse' : 'bg-emerald-400'"></div>
                    <span class="text-xs font-semibold tracking-wide" :class="isSaving ? 'text-yellow-100' : 'text-emerald-50'">{{ isSaving ? 'Đang bộ đồng dữ liệu...' : 'Đã đồng bộ máy chủ' }}</span>
                </div>
            </div>
        </div>
      </div>

      <!-- FORMS -->
      <div class="flex-1 overflow-y-auto p-6 space-y-8 scroll-smooth custom-scrollbar bg-slate-50">
        
        <!-- THÔNG TIN CHUNG -->
        <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm relative overflow-hidden">
          <div class="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-blue-500 to-indigo-500"></div>
          <h2 class="text-xs uppercase font-bold text-slate-800 mb-4 tracking-wider flex items-center gap-2">Thông tin Cá nhân</h2>
          
          <!-- Avatar Upload -->
          <div class="flex items-center gap-4 mb-6 bg-blue-50/50 p-4 rounded-xl border border-dashed border-blue-200 transition-all">
            <div class="relative w-16 h-16 rounded-full bg-white overflow-hidden border-2 border-blue-100 shadow-sm shrink-0 flex items-center justify-center">
                <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
                <div v-else class="w-full h-full flex items-center justify-center text-blue-300">
                    <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path>
                    </svg>
                </div>
            </div>

            <div class="flex-1 flex flex-col gap-1.5">
                <label class="block text-[11px] font-bold text-blue-700 uppercase tracking-wider ml-1">Ảnh đại diện</label>
                
                <div class="flex items-center gap-2">
                    <input type="file" @change="onAvatarChange" accept="image/*" 
                        class="block w-full text-[11px] text-slate-500 file:mr-3 file:py-1.5 file:px-4 file:rounded-full file:border-0 file:text-[11px] file:font-bold file:bg-blue-600 file:text-white hover:file:bg-blue-700 cursor-pointer transition-all shadow-sm file:shadow-blue-200" />
                    
                    <button v-if="resumeData.general.avatarUrl" 
                            @click="removeAvatar" 
                            type="button"
                            class="p-2 text-red-500 hover:bg-red-50 rounded-full transition-all border border-transparent hover:border-red-100 shrink-0" 
                            title="Xóa ảnh và quay về mặc định">
                        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                        </svg>
                    </button>
                </div>
            </div>
        </div>

          <!-- Đã di chuyển trình chọn màu lên Toolbar -->

            <div class="space-y-4">
                <div class="grid grid-cols-1 gap-4">
                    <div class="flex flex-col gap-1.5">
                        <label class="text-[13px] font-bold text-slate-700 ml-1">Họ và tên</label>
                        <RichTextEditor v-model="resumeData.general.fullName" class="w-full text-sm py-2.5 px-3 border border-slate-200 bg-white rounded-lg focus-within:ring-2 focus-within:ring-blue-500 outline-none transition-all font-medium placeholder-slate-400 shadow-sm" placeholder="Nhập họ tên đầy đủ..." />
                    </div>

                    <div class="flex flex-col gap-1.5">
                        <label class="text-[13px] font-bold text-slate-700 ml-1">Vị trí ứng tuyển</label>
                        <RichTextEditor v-model="resumeData.general.jobTitle" class="w-full text-sm py-2.5 px-3 border border-slate-200 bg-white rounded-lg focus-within:ring-2 focus-within:ring-blue-500 outline-none transition-all placeholder-slate-400 shadow-sm" placeholder="Ví dụ: Fullstack Developer..." />
                    </div>
                </div>

                <div class="grid grid-cols-2 gap-4">
                    <div class="flex flex-col gap-1.5">
                        <label class="text-[13px] font-bold text-slate-700 ml-1">Số điện thoại</label>
                        <RichTextEditor v-model="resumeData.general.phone" class="w-full text-sm py-2 px-3 border border-slate-200 bg-white rounded-lg outline-none focus-within:ring-2 focus-within:ring-blue-500 placeholder-slate-400" placeholder="090..." />
                    </div>

                    <div class="flex flex-col gap-1.5">
                        <label class="text-[13px] font-bold text-slate-700 ml-1">Email</label>
                        <RichTextEditor v-model="resumeData.general.email" class="w-full text-sm py-2 px-3 border border-slate-200 bg-white rounded-lg outline-none focus-within:ring-2 focus-within:ring-blue-500 placeholder-slate-400" placeholder="example@gmail.com" />
                    </div>

                    <div class="flex flex-col gap-1.5">
                        <label class="text-[13px] font-bold text-slate-700 ml-1">Ngày sinh</label>
                        <RichTextEditor v-model="resumeData.general.birthDate" class="w-full text-sm py-2 px-3 border border-slate-200 bg-white rounded-lg outline-none focus-within:ring-2 focus-within:ring-blue-500 placeholder-slate-400" placeholder="01/01/2000" />
                    </div>

                    <div class="flex flex-col gap-1.5">
                        <label class="text-[13px] font-bold text-slate-700 ml-1">Giới tính</label>
                        <RichTextEditor v-model="resumeData.general.gender" class="w-full text-sm py-2 px-3 border border-slate-200 bg-white rounded-lg outline-none focus-within:ring-2 focus-within:ring-blue-500 placeholder-slate-400" placeholder="Nam / Nữ" />
                    </div>

                    <div class="flex flex-col gap-1.5 col-span-2">
                        <label class="text-[13px] font-bold text-slate-700 ml-1">Địa chỉ hiện tại</label>
                        <RichTextEditor v-model="resumeData.general.address" class="w-full text-sm py-2 px-3 border border-slate-200 bg-white rounded-lg outline-none focus-within:ring-2 focus-within:ring-blue-500 placeholder-slate-400" placeholder="Quận 1, TP. Hồ Chí Minh" />
                    </div>

                    <div class="flex flex-col gap-1.5 col-span-2">
                        <label class="text-[13px] font-bold text-slate-700 ml-1">Website / LinkedIn / Portfolio</label>
                        <RichTextEditor v-model="resumeData.general.website" class="w-full text-sm py-2 px-3 border border-slate-200 bg-white rounded-lg outline-none focus-within:ring-2 focus-within:ring-blue-500 placeholder-slate-400" placeholder="https://..." />
                    </div>
                </div>
            </div>
        </div>


        <!-- CÁC MỤC ĐỘNG -->
        <div>
          <div class="flex items-center justify-between mb-4 px-1">
              <h2 class="text-xs uppercase font-extrabold text-slate-800 tracking-wider">Quản lý Bố cục</h2>
              <span class="text-[10px] bg-indigo-100 text-indigo-700 px-2 py-0.5 rounded-full font-bold">DRAG & DROP</span>
          </div>
          
          <draggable
            v-model="resumeData.sections"
            item-key="id"
            handle=".drag-handle"
            class="space-y-4"
            ghost-class="opacity-30 scale-95"
            animation="300"
          >
            <template #item="{ element: section, index: sectionIndex }">
              <div class="bg-white border text-sm border-slate-200 rounded-2xl shadow-sm hover:shadow-md transition-all flex flex-col relative overflow-hidden group focus-within:ring-2 ring-blue-100" :class="!section.isVisible ? 'opacity-60 bg-slate-50' : ''">
                
                <!-- Section Header -->
                <div class="flex items-center justify-between p-3.5 border-b border-slate-100 bg-slate-50/50">
                    <div class="flex items-center gap-2 flex-1">
                        <span class="drag-handle cursor-grab text-blue-600 hover:text-blue-700 active:cursor-grabbing p-1.5 bg-white rounded-lg shadow-sm border border-slate-200 transition-colors group-hover:border-blue-200 group-hover:bg-blue-50">
                            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" v-html="getSectionIcon(section.id)"></svg>
                        </span>
                        <input v-model="section.title" class="font-bold text-blue-700 bg-transparent py-1 px-2 rounded-md outline-none focus:ring-2 ring-blue-100 hover:bg-white w-full transition-all uppercase tracking-wide text-sm" />
                    </div>
                    <div class="flex items-center gap-2 ml-2 pl-3 border-l border-slate-200">
                        <!-- Toggle -->
                        <label class="relative inline-flex items-center cursor-pointer" title="Ẩn/Hiện thẻ này">
                            <input type="checkbox" 
                                v-model="section.isVisible" 
                                @change="sortSectionsByVisibility" 
                                class="sr-only peer">
                            <div class="w-8 h-4 bg-slate-300 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-[16px] peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-3 after:w-3 after:transition-all peer-checked:bg-blue-500"></div>
                        </label>
                    </div>
                </div>
                
                    <!-- Section Items (Forms Type) -->
                <div v-show="section.isVisible" class="p-4 bg-white">
                    <!-- Textarea đặc biệt cho Mục tiêu nghề nghiệp -->
                    <div v-if="section.id === 'summary'" class="space-y-2">
                        <div class="flex justify-end">
                            <button @click="generateAISummary" :disabled="isAIProcessing['summary']" class="text-[9px] flex items-center gap-1 bg-amber-100 text-amber-800 px-2 py-0.5 rounded-full hover:bg-amber-200 transition-all font-bold uppercase shadow-sm border border-amber-200 disabled:opacity-50">
                                <template v-if="isAIProcessing['summary']">
                                    <svg class="w-2.5 h-2.5 animate-spin" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
                                </template>
                                <template v-else>
                                    <span>✨ AI</span>
                                </template>
                            </button>
                        </div>
                        <RichTextEditor v-model="resumeData.general.summary" class="w-full text-xs py-2.5 px-3 border border-slate-200 bg-slate-50 rounded-xl focus-within:ring-2 focus-within:ring-blue-400 focus-within:bg-blue-50/60 outline-none transition-all placeholder-slate-400 shadow-sm leading-relaxed" placeholder="Mô tả mục tiêu nghề nghiệp của bạn..." />
                    </div>

                    <draggable v-else-if="section.items" v-model="section.items" item-key="_refId" handle=".sub-drag" animation="200" class="space-y-3">
                        <template #item="{ element: item, index: itemIndex }">
                             <div class="p-3 border border-slate-100 rounded-xl bg-slate-50/50 relative item-card hover:border-blue-200 transition-colors">
                                <!-- Delete Item -->
                                <button @click="removeItem(sectionIndex, itemIndex)" class="delete-btn absolute top-1 right-1 w-6 h-6 bg-red-500 text-white rounded-full flex items-center justify-center shadow-lg z-20">
                                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"></path></svg>
                                </button>
                                <!-- Drag Handle Sub -->
                                <div class="sub-drag cursor-grab absolute left-0 top-0 w-6 h-full flex flex-col justify-center items-center text-slate-300 hover:text-blue-500 opacity-0 group-hover/item:opacity-100">
                                    <div class="w-1 h-1 bg-current rounded-full mb-0.5"></div><div class="w-1 h-1 bg-current rounded-full mb-0.5"></div><div class="w-1 h-1 bg-current rounded-full mb-0.5"></div>
                                </div>

                                <div class="pl-4 space-y-2">
                                    <!-- form kinh nghiệm -->
                                    <template v-if="section.id === 'experience'">
                                        <input v-model="item.company" :class="inputBaseClass" class="font-bold text-slate-800" placeholder="Tên Công ty" />
                                        <div class="grid grid-cols-2 gap-2">
                                            <input v-model="item.role" :class="inputBaseClass" placeholder="Vị trí làm việc" />
                                            <input v-model="item.time" :class="inputBaseClass" class="text-xs" placeholder="Thời gian (VD: 2020 - 2023)" />
                                        </div>
                                        <div class="flex items-start gap-2">
                                            <RichTextEditor v-model="item.desc" :class="inputBaseClass" class="flex-1 leading-relaxed text-xs border border-transparent !px-2 focus-within:bg-blue-50 focus-within:rounded-md transition-colors" placeholder="Mô tả công việc (Dùng dấu • để liệt kê)" />
                                            <button @click="improveAIDesc(item, 'experience')" :disabled="isAIProcessing[item._refId]" class="shrink-0 mt-1 text-[9px] flex items-center gap-1 bg-amber-100 text-amber-800 px-2 py-0.5 rounded-full hover:bg-amber-200 transition-all font-bold shadow-sm border border-amber-200 disabled:opacity-50">
                                                <template v-if="isAIProcessing[item._refId]">
                                                    <svg class="w-2.5 h-2.5 animate-spin" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
                                                </template>
                                                <span v-else>✨ AI</span>
                                            </button>
                                        </div>
                                    </template>

                                    <!-- form học vấn -->
                                    <template v-else-if="section.id === 'education'">
                                        <input v-model="item.school" :class="inputBaseClass" class="font-bold text-slate-800" placeholder="Trường học" />
                                        <input v-model="item.major" :class="inputBaseClass" placeholder="Ngành/Chuyên khoa" />
                                        <div class="grid grid-cols-2 gap-2">
                                            <input v-model="item.year" :class="inputBaseClass" class="text-xs" placeholder="Năm học" />
                                            <input v-model="item.gradType" :class="inputBaseClass" class="text-xs" placeholder="Xếp loại (Giỏi/Khá)" />
                                        </div>
                                    </template>
                                    
                                    <!-- form dự án -->
                                    <template v-else-if="section.id === 'project'">
                                        <input v-model="item.name" :class="inputBaseClass" class="font-bold text-slate-800" placeholder="Tên dự án" />
                                        <div class="grid grid-cols-2 gap-2">
                                            <input v-model="item.role" :class="inputBaseClass" placeholder="Vai trò" />
                                            <input v-model="item.time" :class="inputBaseClass" class="text-xs" placeholder="Thời gian" />
                                        </div>
                                        <div class="flex items-start gap-2">
                                            <RichTextEditor v-model="item.desc" :class="inputBaseClass" class="flex-1 leading-relaxed text-xs border border-transparent !px-2 focus-within:bg-blue-50 focus-within:rounded-md transition-colors" placeholder="Công nghệ sử dụng, Kết quả đạt được..." />
                                            <button @click="improveAIDesc(item, 'project')" :disabled="isAIProcessing[item._refId]" class="shrink-0 mt-1 text-[9px] flex items-center gap-1 bg-amber-100 text-amber-800 px-2 py-0.5 rounded-full hover:bg-amber-200 transition-all font-bold shadow-sm border border-amber-200 disabled:opacity-50">
                                                <template v-if="isAIProcessing[item._refId]">
                                                    <svg class="w-2.5 h-2.5 animate-spin" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
                                                </template>
                                                <span v-else>✨ AI</span>
                                            </button>
                                        </div>
                                    </template>

                                    <!-- form kỹ năng chung (name, level) -->
                                    <template v-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'">
                                        <div class="grid grid-cols-3 gap-2">
                                            <input v-model="item.name" :class="inputBaseClass" class="col-span-2 font-semibold" placeholder="Tên (VD: Lập trình C#) hoặc Ngôn ngữ" />
                                            <input v-model="item.level" :class="inputBaseClass" class="text-xs" placeholder="Mức độ" />
                                        </div>
                                    </template>

                                    <!-- form hoạt động -->
                                    <template v-else-if="section.id === 'activities'">
                                        <input v-model="item.name" :class="inputBaseClass" class="font-bold text-slate-800" placeholder="Tên Hoạt động/Tổ chức" />
                                        <input v-model="item.time" :class="inputBaseClass" class="text-xs" placeholder="Thời gian" />
                                        <div class="flex items-start gap-2">
                                            <RichTextEditor v-model="item.desc" :class="inputBaseClass" class="flex-1 leading-relaxed text-xs border border-transparent !px-2 focus-within:bg-blue-50 focus-within:rounded-md transition-colors" placeholder="Mô tả chi tiết hoạt động..." />
                                            <button @click="improveAIDesc(item, 'activities')" :disabled="isAIProcessing[item._refId]" class="shrink-0 mt-1 text-[9px] flex items-center gap-1 bg-amber-100 text-amber-800 px-2 py-0.5 rounded-full hover:bg-amber-200 transition-all font-bold shadow-sm border border-amber-200 disabled:opacity-50">
                                                <template v-if="isAIProcessing[item._refId]">
                                                    <svg class="w-2.5 h-2.5 animate-spin" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
                                                </template>
                                                <span v-else>✨ AI</span>
                                            </button>
                                        </div>
                                    </template>

                                    <!-- form chứng chỉ/giải thưởng -->
                                    <template v-else-if="section.id === 'certifications' || section.id === 'awards'">
                                        <input v-model="item.name" :class="inputBaseClass" class="font-semibold text-slate-800" placeholder="Tên giải thưởng / Chứng chỉ" />
                                        <input v-model="item.year" :class="inputBaseClass" class="text-xs" placeholder="Năm / Tổ chức cấp" />
                                    </template>

                                    <!-- form sở thích -->
                                    <template v-else-if="section.id === 'hobbies'">
                                        <input v-model="item.name" :class="inputBaseClass" class="font-semibold text-slate-800" placeholder="Sở thích (VD: Đọc sách)" />
                                    </template>

                                    <!-- form tham chiếu -->
                                    <template v-else-if="section.id === 'references'">
                                        <RichTextEditor v-model="item.info" :class="inputBaseClass" class="leading-relaxed text-xs border border-transparent !px-2 focus-within:bg-blue-50 focus-within:rounded-md transition-colors" placeholder="Họ tên, Chức vụ, Số điện thoại người tham chiếu" />
                                    </template>
                                </div>
                            </div>
                        </template>
                    </draggable>
                   
                    <div v-if="section.id !== 'summary'" class="flex gap-2">
                        <button @click="addItem(sectionIndex)" class="mt-3 flex-1 border border-dashed border-slate-300 hover:border-blue-500 text-slate-500 hover:text-blue-600 bg-slate-50/50 hover:bg-blue-50 transition-colors rounded-xl py-2 text-xs font-bold uppercase tracking-wider flex items-center justify-center gap-1.5 focus:outline-none">
                            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path></svg> 
                            Thêm Dòng
                        </button>
                        <button v-if="section.id === 'skills'" @click="generateAISkills(sectionIndex)" :disabled="isAIProcessing['skills']" class="mt-3 border border-amber-200 text-amber-800 bg-amber-100 hover:bg-amber-200 transition-colors rounded-full px-3 py-1 text-[9px] font-black uppercase flex items-center justify-center gap-1 focus:outline-none disabled:opacity-50 shadow-sm">
                            <svg v-if="isAIProcessing['skills']" class="w-2.5 h-2.5 animate-spin" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
                            <span v-else>✨ AI</span>
                        </button>
                    </div>
                </div>
              </div>
            </template>
          </draggable>
        </div>
      </div>
    </div>

    <!-- CỘT PHẢI: PREVIEW PANEL THỜI GIAN THỰC -->
    <div class="flex-1 overflow-auto bg-slate-800 relative scroll-smooth pattern-dots" :style="{ height: '100%', '--theme-color': resumeData.theme.primaryColor }">
        
        <!-- RICH TEXT TOOLBAR (Chiết xuất lên Top toàn bộ) -->
        <div class="no-print sticky top-0 left-0 w-full z-40 bg-white/95 backdrop-blur-md border-b border-slate-200 shadow-sm px-6 py-3 flex items-center justify-between transition-all select-none gap-4">
            
            <!-- Nhóm công cụ Rich Text -->
            <div class="flex flex-wrap items-center gap-1">
                <!-- Dropdown Font Family -->
                <select @change="execCmd('fontName', $event.target.value)" class="text-[11px] border border-slate-200 rounded px-2 py-1.5 bg-white outline-none hover:border-blue-400 text-slate-700 font-medium w-28 cursor-pointer shadow-sm transition-all focus:ring-2 focus:ring-blue-100 hover:bg-slate-50">
                    <option value="">Kiểu chữ</option>
                    <option value="Arial, sans-serif">Arial</option>
                    <option value="'Times New Roman', serif">Times New</option>
                    <option value="Inter, sans-serif">Inter</option>
                    <option value="Roboto, sans-serif">Roboto</option>
                    <option value="Tahoma, sans-serif">Tahoma</option>
                    <option value="Verdana, sans-serif">Verdana</option>
                    <option value="'Courier New', monospace">Courier</option>
                </select>

                <!-- Dropdown Font Size (đổi sang số) -->
                <select @change="execCmd('fontSize', $event.target.value)" class="text-[11px] border border-slate-200 rounded px-2 py-1.5 bg-white outline-none hover:border-blue-400 text-slate-700 font-medium w-28 cursor-pointer shadow-sm transition-all focus:ring-2 focus:ring-blue-100 hover:bg-slate-50">
                    <option value="">Cỡ chữ</option>
                    <option value="1">10</option>
                    <option value="2">12</option>
                    <option value="3">14</option>
                    <option value="4">16</option>
                    <option value="5">18</option>
                    <option value="6">24</option>
                    <option value="7">36</option>
                </select>
                
                <div class="w-px h-6 bg-slate-200 mx-1.5"></div>

                <!-- Đổi màu Chủ đạo CV (Mẫu Gọn mới với Dropdown) -->
                <div class="relative color-picker-dropdown">
                    <button @click="toggleThemeMenu" class="flex items-center gap-1.5 bg-slate-50/80 px-2.5 py-1.5 rounded-lg border border-slate-200 hover:border-blue-400 transition-all shadow-sm">
                        <span class="text-[10px] font-bold text-slate-500 uppercase">Màu nền</span>
                        <div class="w-4 h-4 rounded-sm border border-slate-200 shadow-sm" :style="{ backgroundColor: resumeData.theme.primaryColor }"></div>
                        <svg class="w-3 h-3 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path d="M19 9l-7 7-7-7" stroke-linecap="round" stroke-linejoin="round" stroke-width="2"/></svg>
                    </button>
                    
                    <!-- Dropdown Theme Color -->
                    <div v-if="showThemeMenu" class="absolute top-full left-0 mt-2 p-3 bg-white border border-slate-200 shadow-xl rounded-xl z-50 w-48 animate-in fade-in slide-in-from-top-2 duration-200">
                        <div class="text-[10px] font-bold text-slate-400 uppercase mb-2">Màu chủ đạo CV</div>
                        <div class="grid grid-cols-5 gap-2 mb-3">
                            <button v-for="color in presetColors" :key="color" @click="resumeData.theme.primaryColor = color; showThemeMenu = false" class="w-6 h-6 rounded-md border border-slate-100 shadow-sm hover:scale-110 transition-transform" :style="{ backgroundColor: color }" :title="color"></button>
                        </div>
                        <div class="pt-2 border-t border-slate-100">
                             <label class="flex items-center gap-2 cursor-pointer hover:bg-slate-50 p-1 rounded transition-colors">
                                <div class="relative w-5 h-5 rounded overflow-hidden border border-slate-200 bg-gradient-to-br from-red-500 via-green-500 to-blue-500">
                                    <input type="color" v-model="resumeData.theme.primaryColor" class="absolute inset-0 opacity-0 cursor-pointer" @change="showThemeMenu = false" />
                                </div>
                                <span class="text-[11px] font-medium text-slate-600">Màu tùy chỉnh...</span>
                             </label>
                        </div>
                    </div>
                </div>
                
                <div class="w-px h-6 bg-slate-200 mx-1"></div>

                <!-- Đổi màu Chữ (Mẫu Gọn mới với Dropdown) -->
                <div class="relative color-picker-dropdown">
                    <button @click="toggleFontMenu" class="flex items-center gap-1.5 bg-slate-50/80 px-2.5 py-1.5 rounded-lg border border-slate-200 hover:border-blue-400 transition-all shadow-sm">
                        <span class="text-[10px] font-bold text-slate-500 uppercase">Màu chữ</span>
                        <div class="w-4 h-4 rounded-sm border border-slate-200 shadow-sm flex items-center justify-center bg-white">
                            <span class="text-[10px] font-bold" style="color: #444">A</span>
                        </div>
                        <svg class="w-3 h-3 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path d="M19 9l-7 7-7-7" stroke-linecap="round" stroke-linejoin="round" stroke-width="2"/></svg>
                    </button>

                    <!-- Dropdown Font Color -->
                    <div v-if="showFontMenu" class="absolute top-full left-0 mt-2 p-3 bg-white border border-slate-200 shadow-xl rounded-xl z-50 w-48 animate-in fade-in slide-in-from-top-2 duration-200">
                        <div class="text-[10px] font-bold text-slate-400 uppercase mb-2">Màu văn bản</div>
                        <div class="grid grid-cols-5 gap-2 mb-3">
                            <button v-for="color in presetColors" :key="'font-'+color" @mousedown.prevent="execCmd('foreColor', color); showFontMenu = false" class="w-6 h-6 rounded-md border border-slate-100 shadow-sm hover:scale-110 transition-transform" :style="{ backgroundColor: color }" :title="color"></button>
                        </div>
                        <div class="pt-2 border-t border-slate-100">
                             <label class="flex items-center gap-2 cursor-pointer hover:bg-slate-50 p-1 rounded transition-colors">
                                <div class="relative w-5 h-5 rounded overflow-hidden border border-slate-200 bg-gradient-to-br from-red-500 via-green-500 to-blue-500">
                                    <input type="color" @input="execCmd('foreColor', $event.target.value)" class="absolute inset-0 opacity-0 cursor-pointer" @change="showFontMenu = false" />
                                </div>
                                <span class="text-[11px] font-medium text-slate-600">Màu tùy chỉnh...</span>
                             </label>
                        </div>
                    </div>
                </div>


                <div class="w-px h-6 bg-slate-200 mx-1.5"></div>

                <!-- Bold Italic Underline -->
                <button @mousedown.prevent="execCmd('bold')" :class="{'bg-blue-100 text-blue-700 shadow-inner scale-95 ring-1 ring-blue-300': activeFormats.bold}" class="w-7 h-7 flex items-center justify-center rounded hover:bg-blue-100 hover:text-blue-700 hover:shadow-inner active:bg-blue-200 active:scale-90 transition-all text-slate-700 font-bold outline-none" title="In đậm (Bold)">B</button>
                <button @mousedown.prevent="execCmd('italic')" :class="{'bg-blue-100 text-blue-700 shadow-inner scale-95 ring-1 ring-blue-300': activeFormats.italic}" class="w-7 h-7 flex items-center justify-center rounded hover:bg-blue-100 hover:text-blue-700 hover:shadow-inner active:bg-blue-200 active:scale-90 transition-all text-slate-700 font-serif italic outline-none" title="In nghiêng (Italic)">I</button>
                <button @mousedown.prevent="execCmd('underline')" :class="{'bg-blue-100 text-blue-700 shadow-inner scale-95 ring-1 ring-blue-300': activeFormats.underline}" class="w-7 h-7 flex items-center justify-center rounded hover:bg-blue-100 hover:text-blue-700 hover:shadow-inner active:bg-blue-200 active:scale-90 transition-all text-slate-700 underline outline-none" title="Gạch chân (Underline)">U</button>
                <div class="w-px h-6 bg-slate-200 mx-1.5"></div>

                <!-- Alignment -->
                <button @mousedown.prevent="execCmd('justifyLeft')" :class="{'bg-blue-100 text-blue-700 shadow-inner scale-95 ring-1 ring-blue-300': activeFormats.justifyLeft}" class="w-7 h-7 flex items-center justify-center rounded hover:bg-blue-100 hover:text-blue-700 hover:shadow-inner active:bg-blue-200 active:scale-90 transition-all text-slate-700 outline-none" title="Căn trái">
                   <svg width="15" height="15" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h10M4 18h16" /></svg>
                </button>
                <button @mousedown.prevent="execCmd('justifyCenter')" :class="{'bg-blue-100 text-blue-700 shadow-inner scale-95 ring-1 ring-blue-300': activeFormats.justifyCenter}" class="w-7 h-7 flex items-center justify-center rounded hover:bg-blue-100 hover:text-blue-700 hover:shadow-inner active:bg-blue-200 active:scale-90 transition-all text-slate-700 outline-none" title="Căn giữa">
                   <svg width="15" height="15" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M7 12h10M4 18h16" /></svg>
                </button>
                <button @mousedown.prevent="execCmd('justifyRight')" :class="{'bg-blue-100 text-blue-700 shadow-inner scale-95 ring-1 ring-blue-300': activeFormats.justifyRight}" class="w-7 h-7 flex items-center justify-center rounded hover:bg-blue-100 hover:text-blue-700 hover:shadow-inner active:bg-blue-200 active:scale-90 transition-all text-slate-700 outline-none" title="Căn phải">
                   <svg width="15" height="15" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M10 12h10M4 18h16" /></svg>
                </button>
                <button @mousedown.prevent="execCmd('justifyFull')" :class="{'bg-blue-100 text-blue-700 shadow-inner scale-95 ring-1 ring-blue-300': activeFormats.justifyFull}" class="w-7 h-7 flex items-center justify-center rounded hover:bg-blue-100 hover:text-blue-700 hover:shadow-inner active:bg-blue-200 active:scale-90 transition-all text-slate-700 outline-none" title="Căn đều">
                   <svg width="15" height="15" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" /></svg>
                </button>
                <div class="w-px h-6 bg-slate-200 mx-1.5"></div>

                <!-- Lists -->
                <button @mousedown.prevent="execCmd('insertUnorderedList')" :class="{'bg-blue-100 text-blue-700 shadow-inner scale-95 ring-1 ring-blue-300': activeFormats.insertUnorderedList}" class="w-7 h-7 flex items-center justify-center rounded hover:bg-blue-100 hover:text-blue-700 hover:shadow-inner active:bg-blue-200 active:scale-90 transition-all text-slate-700 font-bold outline-none" title="Danh sách Bullet">&bull;&equiv;</button>
                <button @mousedown.prevent="execCmd('insertOrderedList')" :class="{'bg-blue-100 text-blue-700 shadow-inner scale-95 ring-1 ring-blue-300': activeFormats.insertOrderedList}" class="w-7 h-7 flex items-center justify-center rounded hover:bg-blue-100 hover:text-blue-700 hover:shadow-inner active:bg-blue-200 active:scale-90 transition-all text-slate-700 font-bold text-[11px] outline-none" title="Danh sách số">1.&equiv;</button>
                <div class="w-px h-6 bg-slate-200 mx-1.5"></div>

                <!-- Clear -->
                <button @mousedown.prevent="execCmd('removeFormat')" class="w-7 h-7 flex items-center justify-center rounded hover:bg-red-100 hover:shadow-inner hover:text-red-700 active:bg-red-200 active:scale-90 transition-all text-slate-700 font-bold text-[14px] outline-none" title="Xoá định dạng (Reset)">T&times;</button>
            </div>

            <!-- Nút Export (Trên thanh) -->
            <button @click="exportToPDF" :disabled="isExporting" class="shrink-0 bg-blue-600 text-white px-5 py-2.5 rounded-lg font-bold shadow-md shadow-blue-500/40 hover:bg-blue-500 transition-all text-sm flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed">
                <svg v-if="!isExporting" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z"></path></svg>
                <svg v-else class="w-4 h-4 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"></path></svg>
                {{ isExporting ? 'Đang xuất...' : 'Lưu Export PDF' }}
            </button>
        </div>

        <!-- Zoom Bar bên dưới Toolbar -->
        <div class="sticky top-[80px] pointer-events-none w-full flex justify-end px-8 z-30 mb-8 mt-6">
            <!-- Bộ điều khiển Zoom -->
            <div class="pointer-events-auto flex items-center bg-white/90 backdrop-blur-md border border-slate-200 rounded-full px-3 py-1.5 shadow-xl gap-2">
                <button @click="previewScale = Math.max(0.8, previewScale - 0.1)" class="w-8 h-8 flex items-center justify-center rounded-full hover:bg-slate-100 text-slate-600 transition-colors" title="Thu nhỏ (Min 80%)">
                    <svg width="18" height="18" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M20 12H4"/></svg>
                </button>
                <span class="text-xs font-bold text-slate-700 w-12 text-center">{{ Math.round(previewScale * 100) }}%</span>
                <button @click="previewScale = Math.min(1.5, previewScale + 0.1)" class="w-8 h-8 flex items-center justify-center rounded-full hover:bg-slate-100 text-slate-600 transition-colors" title="Phóng to (Max 150%)">
                    <svg width="18" height="18" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M12 4v16m8-8H4"/></svg>
                </button>
            </div>
        </div>

        <!-- Vùng chứa CV: Dùng flex-col items-center và margin động để thanh cuộn khớp với tỉ lệ scale -->
        <div class="flex flex-col items-center pt-8 pb-32 min-w-max">
            <div class="cv-preview-card transition-transform duration-300 origin-top shadow-2xl bg-white flex-shrink-0" 
                 :style="{ 
                     transform: `scale(${previewScale})`, 
                     width: '210mm',
                     minHeight: '297mm',
                     marginBottom: `${(previewScale - 1) * 297}mm`,
                     marginLeft: `${previewScale > 1 ? (previewScale - 1) * 210 / 2 : 0}mm`,
                     marginRight: `${previewScale > 1 ? (previewScale - 1) * 210 / 2 : 0}mm`
                 }">
                <component 
                  v-if="activeTemplate"
                  :is="activeTemplate" 
                  :resumeData="resumeData" 
                  @moveUp="moveSectionUp" 
                  @moveDown="moveSectionDown" 
                  @moveHorizontal="moveSectionHorizontal"
                  @removeItem="removeItemFromPreview"
                />
            </div>
        </div>
    </div>
  </div>

  <!-- MODAL CẮT ẢNH (Dùng Bootstrap Modal có sẵn trong layout) -->
  <div class="modal fade" id="vueCropperModal" data-bs-backdrop="static" tabindex="-1" aria-hidden="true" style="z-index: 9999;">
    <div class="modal-dialog modal-lg modal-dialog-centered">
      <div class="modal-content border-0 shadow-lg rounded-4 overflow-hidden">
        <div class="modal-header border-bottom-0 bg-slate-900 text-white py-3 px-4">
          <h5 class="modal-title fw-bold">Căn chỉnh ảnh đại diện</h5>
          <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
        </div>
        <div class="modal-body p-0 bg-slate-50">
          <div class="d-flex justify-content-center align-items-center" style="height: 450px; background: #000;">
            <img id="imageToCropVue" src="" style="display: block; max-width: 100%;">
          </div>
          <div class="bg-white p-3 border-top d-flex justify-content-center gap-3">
            <button type="button" class="w-10 h-10 flex items-center justify-center rounded-full border border-slate-200 hover:bg-slate-50" @click="rotateLeft" title="Xoay trái">
               <svg width="20" height="20" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"/></svg>
            </button>
            <button type="button" class="w-10 h-10 flex items-center justify-center rounded-full border border-slate-200 hover:bg-slate-50" @click="zoomIn" title="Phóng to">
               <svg width="20" height="20" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/></svg>
            </button>
            <button type="button" class="w-10 h-10 flex items-center justify-center rounded-full border border-slate-200 hover:bg-slate-50" @click="zoomOut" title="Thu nhỏ">
               <svg width="20" height="20" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 12H4"/></svg>
            </button>
            <button type="button" class="w-10 h-10 flex items-center justify-center rounded-full border border-slate-200 hover:bg-slate-50" @click="rotateRight" title="Xoay phải">
               <svg width="20" height="20" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14 5l7 7m0 0l-7 7m7-7H3"/></svg>
            </button>
          </div>
        </div>
        <div class="modal-footer border-top-0 bg-white p-3 px-4">
          <button type="button" class="px-6 py-2 rounded-full font-bold text-slate-500 hover:bg-slate-100 transition-all" data-bs-dismiss="modal">Hủy bỏ</button>
          <button type="button" class="px-8 py-2 bg-blue-600 text-white rounded-full font-bold shadow-lg shadow-blue-200 hover:bg-blue-700 transition-all" @click="applyCrop">Cắt và Áp dụng</button>
        </div>
      </div>
    </div>
  </div>

  <!-- MODAL XEM TRƯỚC XUẤT FILE (Export Preview) -->
  <div v-if="showExportModal" class="fixed inset-0 z-[10000] flex items-center justify-center p-4">
    <!-- Backdrop với Blur hiện đại -->
    <div class="absolute inset-0 bg-slate-900/60 backdrop-blur-md animate-in fade-in duration-500" @click="showExportModal = false"></div>
    
    <!-- Modal Content -->
    <div class="relative bg-white w-[85vw] h-[96vh] rounded-[1.5rem] shadow-[0_25px_70px_rgba(0,0,0,0.4)] overflow-hidden flex flex-col animate-in zoom-in-95 slide-in-from-bottom-10 duration-500">
        <!-- Modal Header - Thu gọn chiều cao -->
        <div class="px-6 py-2 border-b border-slate-100 flex items-center justify-between shrink-0 bg-white/90 backdrop-blur-md sticky top-0 z-10 transition-all">
            <div class="flex items-center gap-3">
                <span class="w-1.5 h-6 bg-blue-600 rounded-full"></span>
                <h3 class="text-sm font-black text-slate-800 uppercase tracking-tight">Kiểm tra bản in ({{ exportPagesCount }} trang)</h3>
            </div>

            <!-- Bộ điều khiển Zoom gọn hơn -->
            <div class="flex items-center bg-slate-100 rounded-full px-4 py-1 gap-4 border border-slate-200">
                <button @click="modalPreviewScale = Math.max(0.4, modalPreviewScale - 0.1)" class="w-8 h-8 flex items-center justify-center rounded-full bg-white shadow-sm hover:bg-blue-50 hover:text-blue-600 transition-all font-bold text-lg border border-slate-100">-</button>
                <span class="text-[11px] font-black text-slate-700 min-w-[35px] text-center">{{ Math.round(modalPreviewScale * 100) }}%</span>
                <button @click="modalPreviewScale = Math.min(1.5, modalPreviewScale + 0.1)" class="w-8 h-8 flex items-center justify-center rounded-full bg-white shadow-sm hover:bg-blue-50 hover:text-blue-600 transition-all font-bold text-lg border border-slate-100">+</button>
            </div>

            <button @click="showExportModal = false" class="w-10 h-10 rounded-full hover:bg-slate-100 flex items-center justify-center text-slate-400 hover:text-red-500 transition-all">
                <svg class="w-7 h-7" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"></path></svg>
            </button>
        </div>

        <!-- Vùng cuộn bản xem trước - Chỉnh padding p-4 để tăng diện tích hiển thị -->
        <div class="flex-1 overflow-y-auto p-4 bg-slate-900/10 pattern-dots custom-scrollbar">
            <!-- Wrapper để duy trì chiều cao thực tế khi Scale -->
            <div :style="{ 
                    height: `calc(${exportPagesCount * 297}mm * ${modalPreviewScale} + ${exportPagesCount * 3}rem)`,
                    minWidth: 'fit-content'
                 }" 
                 class="flex flex-col items-center mx-auto transition-all duration-300">
                
                <!-- Khối trang thực phẩm - Scale tập trung ở đây -->
                <div :style="{ 
                        transform: `scale(${modalPreviewScale})`, 
                        transformOrigin: 'top center' 
                     }" 
                     class="flex flex-col items-center">
                    
                    <div v-for="pageIdx in exportPagesCount" :key="pageIdx" 
                         class="relative bg-white shadow-[0_30px_60px_rgba(0,0,0,0.18)] ring-1 ring-slate-300 mb-12 last:mb-0 flex-shrink-0 overflow-hidden" 
                         style="width: 210mm; height: 297mm;">
                        
                        <div :style="{ transform: `translateY(-${(pageIdx - 1) * 297}mm)` }">
                            <img :src="exportPreviewUrl" class="w-full h-auto block" alt="CV Preview Page" />
                        </div>

                        <!-- Label trang kín đáo -->
                        <div class="absolute bottom-6 right-8 bg-slate-900 text-white/90 px-4 py-1.5 rounded-lg text-[11px] font-black uppercase tracking-widest shadow-xl">
                            P.{{ pageIdx }}
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal Footer - Làm nhỏ các nút -->
        <div class="p-4 border-t border-slate-100 bg-white/90 backdrop-blur-md flex items-center justify-center gap-4 shrink-0 shadow-[0_-10px_30px_rgba(0,0,0,0.02)]">
            <button @click="showExportModal = false" class="group px-6 py-2 rounded-xl font-bold text-[11px] uppercase tracking-widest text-slate-500 hover:bg-slate-50 hover:text-slate-800 transition-all flex items-center gap-2">
                <svg class="w-3.5 h-3.5 group-hover:-translate-x-1 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M10 19l-7-7m0 0l7-7m-7 7h18"></path></svg>
                Quay lại
            </button>
            <button @click="confirmDownloadPDF" :disabled="isExporting" class="group px-8 py-2.5 bg-slate-900 text-white rounded-xl font-bold text-[11px] uppercase tracking-[0.1em] shadow-xl shadow-slate-900/20 hover:bg-blue-600 hover:shadow-blue-600/20 transition-all flex items-center gap-2.5 active:scale-95 disabled:opacity-50">
                <svg v-if="!isExporting" class="w-4 h-4 group-hover:-translate-y-0.5 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"></path></svg>
                <svg v-else class="w-4 h-4 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"></path></svg>
                {{ isExporting ? 'Đang tạo...' : 'Tải xuống PDF' }}
            </button>
        </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch, shallowRef, defineAsyncComponent } from 'vue'
import draggable from 'vuedraggable'
import RichTextEditor from './components/RichTextEditor.vue'
import { toJpeg } from 'html-to-image'
import { jsPDF } from 'jspdf'

const sortSectionsByVisibility = () => {
    // Chúng ta không dùng hàm .sort() mặc định vì nó có thể làm xáo trộn 
    // thứ tự ưu tiên mà người dùng đã dày công sắp xếp trước đó.
    const visibleSections = resumeData.value.sections.filter(s => s.isVisible);
    const hiddenSections = resumeData.value.sections.filter(s => !s.isVisible);
    
    // Gộp lại: mục đang bật lên trước, mục đang tắt xuống sau
    resumeData.value.sections = [...visibleSections, ...hiddenSections];
    
    console.log('[CV Builder] Đã ưu tiên đẩy các mục đang kích hoạt lên trên.');
};

const removeAvatar = () => {
    resumeData.value.general.avatarUrl = '';
    const fileInput = document.querySelector('input[accept="image/*"]');
    if (fileInput) {
        fileInput.value = '';
    }

    console.log('Đã xóa ảnh đại diện.');
};

const getSectionIcon = (id) => {
  const icons = {
    summary: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />', // User
    experience: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 13.255A23.931 23.931 0 0112 15c-3.183 0-6.22-.62-9-1.745M16 6V4a2 2 0 00-2-2h-4a2 2 0 00-2 2v2m4 6h.01M5 20h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />', // Briefcase
    education: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 14l9-5-9-5-9 5 9 5zm0 0l6.16-3.422a12.083 12.083 0 01.665 6.479A11.952 11.952 0 0012 20.055a11.952 11.952 0 00-6.824-2.998 12.078 12.078 0 01.665-6.479L12 14zm-4 6v-7.5l4-2.222" />', // Academic
    skills: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />', // Bolt
    it_skills: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />', // Desktop
    languages: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5h12M9 3v2m1.048 9.5A18.022 18.022 0 016.412 9m6.088 9h7M11 21l5-10 5 10M12.751 5C11.783 10.77 8.07 15.61 3 18.129" />', // Language
    activities: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />', // Users
    project: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />', // Folder
    certifications: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4M7.835 4.697a3.42 3.42 0 001.946-.806 3.42 3.42 0 014.438 0 3.42 3.42 0 001.946.806 3.42 3.42 0 013.138 3.138 3.42 3.42 0 00.806 1.946 3.42 3.42 0 010 4.438 3.42 3.42 0 00-.806 1.946 3.42 3.42 0 01-3.138 3.138 3.42 3.42 0 00-1.946.806 3.42 3.42 0 01-4.438 0 3.42 3.42 0 00-1.946-.806 3.42 3.42 0 01-3.138-3.138 3.42 3.42 0 00-.806-1.946 3.42 3.42 0 010-4.438 3.42 3.42 0 00.806-1.946 3.42 3.42 0 013.138-3.138z" />', // Badge
    awards: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />', // Sparkles
    hobbies: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />', // Heart
    references: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />', // Chat
  };
  return icons[id] || '<path d="M4 8h16M4 16h16" />'; // Default là dấu =
};

// --- CƠ CHẾ DYNAMIC TEMPLATE LOADER ---
// Tự động quét toàn bộ file .vue trong thư mục templates
const allTemplates = import.meta.glob('./templates/*.vue')
const templateRegistry = {}

// Đăng ký vào bộ nhớ đệm
for (const path in allTemplates) {
    const fileName = path.split('/').pop().replace('.vue', '')
    templateRegistry[fileName] = defineAsyncComponent(allTemplates[path])
}

// Bảng ánh xạ ID database -> Tên file
const templateMapping = {
    4: 'Template' // Map tới Template.vue
}

const activeTemplate = shallowRef(null) // Sẽ được set khi load data
const previewScale = ref(1.0); // Mặc định ban đầu là 100%
const isPreviewMode = ref(false)
const isExporting = ref(false)
const showExportModal = ref(false)
const exportPreviewUrl = ref('')
const exportPagesCount = ref(1)
const modalPreviewScale = ref(0.85) // Tăng tỉ lệ mặc định để xem to hơn

// --- QUẢN LÝ DROPDOWN MÀU SẮC ---
const showThemeMenu = ref(false)
const showFontMenu = ref(false)
const presetColors = ['#000000', '#ffffff', '#2b5c8f', '#dc2626', '#eab308', '#16a34a', '#2563eb', '#6b7280', '#4b5563', '#ef4444']

const toggleThemeMenu = () => {
    showThemeMenu.value = !showThemeMenu.value
    if (showThemeMenu.value) showFontMenu.value = false
}

const toggleFontMenu = () => {
    showFontMenu.value = !showFontMenu.value
    if (showFontMenu.value) showThemeMenu.value = false
}

const handleOutsideClick = (e) => {
    if (!e.target.closest('.color-picker-dropdown')) {
        showThemeMenu.value = false
        showFontMenu.value = false
    }
}

const execCmd = (command, value = null) => {
    document.execCommand(command, false, value);
    if (typeof updateFormatState === 'function') setTimeout(updateFormatState, 10);
};

const isSaving = ref(false)
const resumeId = window.CURRENT_RESUME_ID || 0

// Khởi tạo Dữ liệu bám sát Models ResumeViewModel.cs
const resumeData = ref({
  overrideTemplate: '',
  theme: { primaryColor: '#2b5c8f' },
  general: {
    fullName: '', jobTitle: '', email: '', phone: '', address: '', birthDate: '', summary: '', website: '', avatarUrl: ''
  },
  sections: [
    { id: 'summary', title: 'Mục tiêu Nghề nghiệp', isVisible: true, column: 'left', items: [] },
    { id: 'experience', title: 'Kinh nghiệm Làm việc', isVisible: true, column: 'right', items: [] },
    { id: 'education', title: 'Quá trình Học vấn', isVisible: true, column: 'left', items: [] },
    { id: 'skills', title: 'Kỹ năng Chuyên môn', isVisible: true, column: 'left', items: [] },
    { id: 'it_skills', title: 'Tin học', isVisible: true, column: 'left', items: [] },
    { id: 'languages', title: 'Ngoại ngữ', isVisible: true, column: 'left', items: [] },
    { id: 'activities', title: 'Hoạt động', isVisible: false, column: 'right', items: [] },
    { id: 'project', title: 'Dự án Trọng điểm', isVisible: false, column: 'right', items: [] },
    { id: 'certifications', title: 'Chứng chỉ / Bằng cấp', isVisible: false, column: 'left', items: [] },
    { id: 'awards', title: 'Giải thưởng', isVisible: false, column: 'left', items: [] },
    { id: 'hobbies', title: 'Sở thích', isVisible: false, column: 'left', items: [] },
    { id: 'references', title: 'Người tham chiếu', isVisible: false, column: 'left', items: [] },
  ]
})

// Utilities Func
const formatDesc = (text) => {
    if (!text) return '';
    // Format các đầu mục có gạch ngang hoặc chấm tròn sang thẻ bullet
    return text.split('\n').map(l => l.trim()).filter(l=>l).join('<br/>');
}

const generateId = () => Math.random().toString(36).substr(2, 9);
const addItem = (sectionIndex) => {
  const section = resumeData.value.sections[sectionIndex];
  if (!section.items) section.items = [];
  
  // Gen factory dựa theo type
  let newItem = { _refId: generateId() };
  if(section.id === 'experience')      Object.assign(newItem, { company: '', role: '', time: '', desc: '' });
  else if(section.id === 'education')  Object.assign(newItem, { school: '', major: '', year: '', gradType: '' });
  else if(section.id === 'project')    Object.assign(newItem, { name: '', role: '', time: '', desc: '' });
  else if(section.id === 'skills' || section.id==='languages' || section.id==='it_skills') Object.assign(newItem, { name: '', level: '' });
  else if(section.id === 'activities') Object.assign(newItem, { name: '', time: '', desc: '' });
  else if(section.id === 'hobbies')    Object.assign(newItem, { name: '' });
  else if(section.id === 'references') Object.assign(newItem, { info: '' });
  else Object.assign(newItem, { name: '', year: '' });

  section.items.push(newItem);
}

const removeItem = (sIdx, iIdx) => resumeData.value.sections[sIdx].items.splice(iIdx, 1);
const removeItemFromPreview = (sectionId, itemIndex) => {
    const sIdx = resumeData.value.sections.findIndex(s => s.id === sectionId);
    if (sIdx !== -1) {
        resumeData.value.sections[sIdx].items.splice(itemIndex, 1);
    }
}

const exportToPDF = async () => {
  const cvEl = document.getElementById('cv-printable-area') || document.querySelector('.cv-preview-card');
  if (!cvEl) { window.print(); return; }

  isExporting.value = true;
  
  // Lưu lại viewport state và scale cũ
  const originalScale = previewScale.value;
  // Đưa scale về đúng 100% để canvas chụp chính xác tỷ lệ và độ phân giải
  previewScale.value = 1.0;
  
  // Xóa bỏ trạng thái active/hover box tạm thời bằng cách thêm class is-exporting-pdf
  cvEl.classList.add('is-exporting-pdf');
  
  // Đợi Vue render DOM xong (do thay đổi scale và xóa trạng thái)
  await new Promise(resolve => setTimeout(resolve, 500));

  try {
      // Sử dụng html-to-image giúp xử lý các CSS hiện đại (như oklch của Tailwind v4) mà không bị lỗi
      const dataUrl = await toJpeg(cvEl, {
          quality: 1.0,
          pixelRatio: 2.5, // Giảm nhẹ xuống 2.5 để tăng tốc độ preview (vẫn rất sắc nét)
          backgroundColor: '#ffffff'
      });

      exportPreviewUrl.value = dataUrl;
      
      // Tính số trang để hiển thị preview tách trang
      const pdfWidth = 210; 
      const pageHeight = 297; 
      const totalPdfHeight = (cvEl.offsetHeight * pdfWidth) / cvEl.offsetWidth; 
      exportPagesCount.value = Math.max(1, Math.ceil(totalPdfHeight / pageHeight));
      
      showExportModal.value = true;
  } catch (error) {
      console.error('Lỗi khi chuẩn bị bản xem trước: ', error);
      alert('Có lỗi xảy ra khi chuẩn bị bản xem trước. Vui lòng thử lại!');
  } finally {
      // Trả lại scale cũ và loại bỏ class ẩn viền
      cvEl.classList.remove('is-exporting-pdf');
      previewScale.value = originalScale;
      isExporting.value = false;
  }
}

const confirmDownloadPDF = async () => {
  if (!exportPreviewUrl.value) return;
  
  isExporting.value = true;
  try {
      const cvEl = document.getElementById('cv-printable-area') || document.querySelector('.cv-preview-card');
      const pdf = new jsPDF('p', 'mm', 'a4');
      const pdfWidth = 210; 
      const pageHeight = 297; 
      
      const totalPdfHeight = (cvEl.offsetHeight * pdfWidth) / cvEl.offsetWidth; 
      const pages = Math.max(1, Math.ceil(totalPdfHeight / pageHeight));

      for (let i = 0; i < pages; i++) {
          if (i > 0) pdf.addPage();
          pdf.addImage(exportPreviewUrl.value, 'JPEG', 0, -(i * pageHeight), pdfWidth, totalPdfHeight);
      }
      
      pdf.save(`CV_${resumeData.value.general.fullName || 'Export'}.pdf`);
      showExportModal.value = false;
  } catch (error) {
      console.error('Lỗi khi xuất PDF: ', error);
      alert('Có lỗi xảy ra khi tải xuống PDF!');
  } finally {
      isExporting.value = false;
  }
}

let cropperInstance = null;
const onAvatarChange = (event) => {
    const file = event.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = (e) => {
            const img = document.getElementById('imageToCropVue');
            img.src = e.target.result;
            
            // Mở Modal (Sử dụng Bootstrap có sẵn)
            const modalEl = document.getElementById('vueCropperModal');
            const modal = new window.bootstrap.Modal(modalEl);
            modal.show();

            // Khởi tạo Cropper khi modal hiện xong
            modalEl.addEventListener('shown.bs.modal', () => {
                if (cropperInstance) cropperInstance.destroy();
                cropperInstance = new window.Cropper(img, {
                    aspectRatio: 1,
                    viewMode: 1,
                    dragMode: 'move',
                    autoCropArea: 1,
                });
            }, { once: true });
        };
        reader.readAsDataURL(file);
    }
}

const applyCrop = () => {
    if (!cropperInstance) return;
    const canvas = cropperInstance.getCroppedCanvas({ width: 400, height: 400 });
    resumeData.value.general.avatarUrl = canvas.toDataURL('image/jpeg', 0.9);
    
    // Đóng Modal
    const modalEl = document.getElementById('vueCropperModal');
    const modal = window.bootstrap.Modal.getInstance(modalEl);
    modal.hide();
}

const rotateLeft = () => cropperInstance?.rotate(-45);
const rotateRight = () => cropperInstance?.rotate(45);
const zoomIn = () => cropperInstance?.zoom(0.1);
const zoomOut = () => cropperInstance?.zoom(-0.1);

const moveSectionUp = (sectionId, columnIds) => {
  console.log('[CV Builder] moveUp:', sectionId, 'column:', columnIds);
  const list = [...resumeData.value.sections];
  const idx = list.findIndex(s => s.id === sectionId);
  if (idx === -1) { console.warn('Section not found:', sectionId); return; }

  // Chỉ cần tìm phần mục phía trước thuộc cùng một cột (bỏ điều kiện isVisible)
  let targetIdx = -1;
  for (let i = idx - 1; i >= 0; i--) {
    if (columnIds.includes(list[i].id)) {
      targetIdx = i;
      break;
    }
  }

  console.log('[CV Builder] moveUp: idx=', idx, 'targetIdx=', targetIdx);
  if (targetIdx !== -1) {
    const item = list.splice(idx, 1)[0];
    list.splice(targetIdx, 0, item);
    resumeData.value.sections = list;
    console.log('[CV Builder] moveUp: Done! New order:', list.map(s => s.id));
  }
}

const moveSectionDown = (sectionId, columnIds) => {
  console.log('[CV Builder] moveDown:', sectionId, 'column:', columnIds);
  const list = [...resumeData.value.sections];
  const idx = list.findIndex(s => s.id === sectionId);
  if (idx === -1) { console.warn('Section not found:', sectionId); return; }

  // Chỉ cần tìm phần mục phía sau thuộc cùng một cột (bỏ điều kiện isVisible)
  let targetIdx = -1;
  for (let i = idx + 1; i < list.length; i++) {
    if (columnIds.includes(list[i].id)) {
      targetIdx = i;
      break;
    }
  }

  console.log('[CV Builder] moveDown: idx=', idx, 'targetIdx=', targetIdx);
  if (targetIdx !== -1) {
    const item = list.splice(idx, 1)[0];
    list.splice(targetIdx, 0, item);
    resumeData.value.sections = list;
    console.log('[CV Builder] moveDown: Done! New order:', list.map(s => s.id));
  }
}

const moveSectionHorizontal = (sectionId, direction) => {
    console.log('[CV Builder] moveHorizontal:', sectionId, 'to:', direction);
    const section = resumeData.value.sections.find(s => s.id === sectionId);
    if (section) {
        section.column = direction;
        // Đưa xuống cuối cùng của cột mới bằng cách di chuyển vị trí trong mảng
        const list = [...resumeData.value.sections];
        const idx = list.findIndex(s => s.id === sectionId);
        const item = list.splice(idx, 1)[0];
        list.push(item); // Đẩy xuống cuối mảng sections
        resumeData.value.sections = list;
    }
}

// Logic API (Đồng bộ khứ hồi với C# Backend)
const inputBaseClass = "w-full bg-transparent border-b border-transparent focus:border-blue-400 py-1 outline-none transition-colors hover:bg-white px-1 -ml-1 rounded-sm focus:bg-white";

const loadData = async () => {
    if (resumeId === 0) return;
    try {
        const res = await fetch(`/api/cvbuilder/data/${resumeId}`);
        const data = await res.json();
        
        // Đổi Mẫu dựa trên TemplateID từ API
        // --- CHỌN MẪU CV TỰ ĐỘNG ---
        let targetName = null;
        if (data) {
            // 1. Ưu tiên: Sử dụng ComponentName trả về trực tiếp từ API (Chính xác nhất dành cho mẫu Vue)
            if (data.componentName) {
                targetName = data.componentName;
            }
            // 2. Dự phòng 1: Nhận diện mẫu dựa trên tên từ database (Dành cho bản ghi cũ)
            else if (data.templateName) {
                if (data.templateName.includes("Nguyễn Yên Nhi")) targetName = "NguyenYenNhi";
            }

            // 3. Dự phòng 2: Nếu không tìm thấy, thử tìm trong bảng ánh xạ Mapping ID cũ
            if (!targetName && data.templateId) {
                targetName = templateMapping[data.templateId];
            }
            
            // 4. Cuối cùng: Thử tìm theo quy tắc đặt tên mặc định Template<ID>.vue
            if (!targetName && data.templateId) {
                targetName = `Template${data.templateId}`;
            }
        }

        // 3. Lấy component từ registry
        if (targetName && templateRegistry[targetName]) {
            activeTemplate.value = templateRegistry[targetName];
        } else {
            // Luôn đảm bảo có mẫu hiển thị
            activeTemplate.value = templateRegistry['Template'] || templateRegistry[Object.keys(templateRegistry)[0]];
            if (data && data.templateId) {
                console.warn(`Không tìm thấy mẫu CV ID: ${data.templateId}, đang sử dụng mẫu:`, activeTemplate.value);
            }
        }

        if (data && data.jsonContent && data.jsonContent !== "{}") {
            const parsed = JSON.parse(data.jsonContent);
            
            // --- HỖ TRỢ OVERRIDE MẪU CHO TESTING (VUE TEMPLATES) ---
            if (parsed.overrideTemplate && templateRegistry[parsed.overrideTemplate]) {
                activeTemplate.value = templateRegistry[parsed.overrideTemplate];
                resumeData.value.overrideTemplate = parsed.overrideTemplate; // Lưu lại để AutoSave không làm mất
                console.log(`[Vue Test] Overriding template to: ${parsed.overrideTemplate}`);
            }

            resumeData.value.theme = parsed.theme || resumeData.value.theme;
            resumeData.value.general = parsed.general || resumeData.value.general;
            
            if (parsed.sections) {
                // MIGRATION: Đảm bảo các section id mới luôn tồn tại và có thuộc tính column
                const defaultSections = resumeData.value.sections;
                
                resumeData.value.sections = parsed.sections.map(s => {
                    const def = defaultSections.find(ds => ds.id === s.id);
                    return {
                        ...s,
                        column: s.column || (def ? def.column : 'left')
                    };
                });

                // Thêm các section hoàn toàn mới (chưa có trong JSON cũ)
                const existingIds = parsed.sections.map(s => s.id);
                const missingSections = defaultSections.filter(s => !existingIds.includes(s.id));
                resumeData.value.sections = [...resumeData.value.sections, ...missingSections];
            }
        } else {
            // Đổ Data mẫu trải nghiệm nếu file trắng
            resumeData.value.general = { fullName: 'Trần Văn Demo', jobTitle: 'Fullstack Developer', email: 'mail@demo.com', phone: '090-000-000', address: 'Quận 1, TP HCM', summary: 'Ứng viên năng động, mong chờ cơ hội.' };
            resumeData.value.sections[0].items.push({ _refId: '1', company: 'FPT Software', role: 'Dev', time: '2020-2023', desc: '• Xây app Vuejs\n• Build API .NET Core\n• Unit Test' });
            resumeData.value.sections[1].items.push({ _refId: '2', school: 'ĐH Bách Khoa', major: 'Khoa học Máy tính', year: '2019', gradType: 'Giỏi' });
            resumeData.value.sections[2].items.push({ _refId: '3', name: 'C# / .NET', level: 'Chuyên gia' });
        }
    } catch (err) { console.error("Lỗi:", err); }
}

let saveTimeout = null;
watch(resumeData, () => {
   if (saveTimeout) clearTimeout(saveTimeout);
   saveTimeout = setTimeout(async () => {
       if (resumeId === 0) return;
        isSaving.value = true;
        try {
          await fetch(`/api/cvbuilder/save/${resumeId}`, {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify({ jsonContent: JSON.stringify(resumeData.value) })
          });
        } catch(e) {} finally { isSaving.value = false; }
   }, 2000); // Đợi 2s không gõ mới lưu DB bảo vệ C# Database
}, { deep: true });

const activeFormats = ref({
    bold: false,
    italic: false,
    underline: false,
    justifyLeft: false,
    justifyCenter: false,
    justifyRight: false,
    justifyFull: false,
    insertUnorderedList: false,
    insertOrderedList: false,
});

const updateFormatState = () => {
    try {
        activeFormats.value.bold = document.queryCommandState('bold');
        activeFormats.value.italic = document.queryCommandState('italic');
        activeFormats.value.underline = document.queryCommandState('underline');
        activeFormats.value.justifyLeft = document.queryCommandState('justifyLeft');
        activeFormats.value.justifyCenter = document.queryCommandState('justifyCenter');
        activeFormats.value.justifyRight = document.queryCommandState('justifyRight');
        activeFormats.value.justifyFull = document.queryCommandState('justifyFull');
        activeFormats.value.insertUnorderedList = document.queryCommandState('insertUnorderedList');
        activeFormats.value.insertOrderedList = document.queryCommandState('insertOrderedList');
    } catch(e) {}
};

// --- TÍNH NĂNG AI (GEMINI) ---
const isAIProcessing = ref({});

const callAIService = async (type, content, context = '') => {
    try {
        const response = await fetch('/AI/ProcessText', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: new URLSearchParams({ type, content, context })
        });
        const result = await response.json();
        if (result.success) return result.data;
        else {
            alert(result.data || 'Lỗi xử lý AI');
            return null;
        }
    } catch (e) {
        console.error('AI Error:', e);
        alert('Không thể kết nối với máy chủ AI.');
        return null;
    }
};

const generateAISummary = async () => {
    const jobTitle = resumeData.value.general.jobTitle; 
    let currentSummary = (resumeData.value.general.summary || '').replace(/<[^>]*>/g, '').trim();
    
    // Nếu cả tóm tắt và vị trí ứng tuyển đều trống thì báo lỗi
    if (!currentSummary && !jobTitle) {
        alert("Vui lòng nhập Vị trí ứng tuyển hoặc một vài ý chính để AI có thể viết mục tiêu nghề nghiệp!");
        return;
    }

    // Nếu chưa nhập tóm tắt, dùng Job Title làm hạt giống
    if (!currentSummary) {
        currentSummary = `Tôi đang ứng tuyển vị trí ${jobTitle}`;
    }

    isAIProcessing.value['summary'] = true;
    const result = await callAIService('summary', currentSummary, jobTitle || 'Nhân viên');
    if (result) resumeData.value.general.summary = result;
    isAIProcessing.value['summary'] = false;
};

const improveAIDesc = async (item, sectionId) => {
    const jobTitle = resumeData.value.general.jobTitle || 'Nhân viên';
    let content = (item.desc || '').replace(/<[^>]*>/g, '').trim();
    
    // Nếu mô tả trống, lấy dữ liệu từ các ô phía trên trong cùng 1 mục để gợi ý cho AI
    if (!content) {
        if (sectionId === 'experience') {
            content = item.company ? `Làm việc tại ${item.company}` : '';
        } else if (sectionId === 'project') {
            content = item.name ? `Dự án ${item.name}` : '';
        } else if (sectionId === 'activities') {
            content = item.name ? `Hoạt động tại ${item.name}` : '';
        }
    }

    // Kiểm tra nếu thực sự không có gì để AI dựa vào
    if (!content) {
        alert("Vui lòng nhập một vài ý chính để AI có thể hỗ trợ!");
        return;
    }

    const context = sectionId === 'experience' ? (item.role || jobTitle) : (sectionId === 'project' ? (item.role || jobTitle) : jobTitle);
    const aiType = sectionId === 'experience' ? 'optimize' : (sectionId === 'project' ? 'project' : 'activity');
    
    isAIProcessing.value[item._refId] = true;
    const result = await callAIService(aiType, content, context);
    if (result) item.desc = result;
    isAIProcessing.value[item._refId] = false;
};

const generateAISkills = async (sectionIndex) => {
  const section = resumeData.value.sections[sectionIndex];
  const jobTitle = (resumeData.value.general.jobTitle || '').replace(/<[^>]*>/g, '').trim();

  if (!jobTitle) {
      alert("Vui lòng nhập Vị trí ứng tuyển để AI có thể gợi ý kỹ năng phù hợp!");
      return;
  }
  
  isAIProcessing.value['skills'] = true;
  const result = await callAIService('suggest_skills', jobTitle, jobTitle);
  if (result) {
      if (!section.items) section.items = [];
      const skillNames = result.split(',').map(s => s.trim()).filter(s => s);
      skillNames.forEach(name => {
          section.items.push({ _refId: generateId(), name: name, level: 'Thành thạo' });
      });
  }
  isAIProcessing.value['skills'] = false;
};

onMounted(() => {
    loadData();
    document.addEventListener('selectionchange', updateFormatState);
    document.addEventListener('mouseup', updateFormatState);
    document.addEventListener('keyup', updateFormatState);
    document.addEventListener('mousedown', handleOutsideClick);
});

onUnmounted(() => {
    document.removeEventListener('selectionchange', updateFormatState);
    document.removeEventListener('mouseup', updateFormatState);
    document.removeEventListener('keyup', updateFormatState);
    document.removeEventListener('mousedown', handleOutsideClick);
});
</script>

<style>

.cv-builder-editor-panel .overflow-y-auto {
    padding-bottom: 100px !important; /* Tạo khoảng trống để không bị sát rìa */
}

.pattern-dots {
    background-image: radial-gradient(#cbd5e1 1px, transparent 1px);
    background-size: 24px 24px;
}

/* Media print giữ lại làm fallback nếu window.print() được gọi trực tiếp */
@media print {
  .hide-on-print { display: none !important; }
  body, html { 
    background: white !important; 
    margin: 0 !important; 
    padding: 0 !important; 
    overflow: visible !important; 
    height: auto !important; 
    width: auto !important;
  }
  .cv-builder-editor-panel, .sticky, .no-print, .nav-btns, button { display: none !important; }
  .pattern-dots, .bg-slate-800, .bg-slate-50 { background: none !important; background-image: none !important; }
  .cv-builder-container, #app, .flex-1 { 
    display: block !important; 
    height: auto !important; 
    overflow: visible !important; 
    background: white !important;
    padding: 0 !important;
    margin: 0 !important;
  }
  .cv-preview-card { 
    transform: none !important; 
    margin: 0 !important; 
    box-shadow: none !important; 
    width: 210mm !important;
    min-height: 297mm !important;
  }
  #cv-printable-area { 
    border: none !important; 
    box-shadow: none !important; 
    margin: 0 !important; 
    width: 210mm !important;
    background: white !important;
  }
  @page { size: A4 portrait; margin: 0; }
}

/* Style cho nút xóa trong Sidebar */
.item-card .delete-btn {
    opacity: 0;
    transform: scale(0.8);
    transition: all 0.2s ease-in-out;
}
.item-card:hover .delete-btn {
    opacity: 1;
    transform: scale(1);
}
.delete-btn:hover {
    background-color: #ef4444;
    box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1);
}
</style>
