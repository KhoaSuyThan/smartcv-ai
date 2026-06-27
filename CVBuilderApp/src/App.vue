<template>
  <div class="w-full bg-slate-50 flex font-sans text-slate-800 cv-builder-container relative" style="height: 100%; overflow: hidden;">
    
    <!-- PANEL GỢI Ý HÀNH ĐỘNG (Đặt ở Root để đảm bảo luôn hiển thị) -->
    <div v-if="showTips && !isPreviewMode" 
         class="absolute left-[715px] top-24 w-72 z-[9999] rounded-2xl shadow-[0_20px_60px_rgba(0,0,0,0.3)] border overflow-hidden transition-all duration-500 completion-tips-panel"
    >
        <div class="bg-slate-900 px-4 py-2 flex items-center justify-between border-b border-white/10">
            <div class="flex items-center gap-2">
                <span class="text-amber-400 text-sm">✨</span>
                <span class="text-[10px] font-black uppercase tracking-widest text-white">Gợi ý hoàn thiện</span>
            </div>
            <button @click.stop="showTips = false" class="w-6 h-6 flex items-center justify-center rounded-full bg-white/10 text-slate-400 hover:text-white transition-all">
                <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"></path></svg>
            </button>
        </div>
        <div class="max-h-[400px] overflow-y-auto custom-scrollbar p-2 completion-tips-body">
            <div v-if="completionTips.length === 0" class="py-10 text-center px-4">
                <div class="text-4xl mb-2">🏆</div>
                <div class="text-[11px] font-black text-emerald-600 uppercase tracking-widest leading-relaxed text-center">Hoàn hảo!</div>
            </div>
            <div v-else class="space-y-1">
                <button 
                    v-for="tip in completionTips" 
                    :key="tip.id"
                    @click.stop="scrollToField(tip.targetId)"
                    class="w-full text-left p-2.5 rounded-xl transition-all flex items-center gap-3 group border completion-tip-item"
                >
                    <div class="shrink-0 flex items-center justify-center w-5 h-5 rounded-lg border transition-colors" :class="tip.isDone ? 'bg-emerald-50 border-emerald-100 text-emerald-600' : 'completion-tip-icon-unread'">
                        <svg v-if="tip.isDone" class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 13l4 4L19 7"></path></svg>
                        <span v-else class="text-[10px] font-black italic">!</span>
                    </div>
                    <div class="text-[12px] font-bold leading-tight transition-colors completion-tip-label">{{ tip.label }}</div>
                </button>
            </div>
        </div>
    </div>
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
      'cv-builder-editor-panel bg-white border-r border-slate-200 shadow-[0_0_20px_rgba(0,0,0,0.05)] z-20 flex flex-col shrink-0 transition-all duration-500 ease-in-out relative',
      isPreviewMode ? 'w-0 opacity-0 border-r-0' : 'w-[600px] opacity-100'
    ]" style="height: 100%;">
      <!-- HEADER -->
      <div class="py-3 border-b border-slate-100 bg-slate-900 text-white shrink-0 relative px-4">
        <div class="absolute top-0 right-0 -mr-8 -mt-8 w-32 h-32 bg-blue-500 rounded-full opacity-20 blur-2xl"></div>
        <div class="flex flex-col gap-3">
            <!-- Trạng thái đồng bộ -->
            <div class="flex items-center justify-between bg-white/5 rounded-lg p-2 backdrop-blur-sm border border-white/5">
                <div class="flex items-center gap-2">
                    <div class="w-2 h-2 rounded-full" :class="isSaving ? 'bg-yellow-400 animate-pulse' : 'bg-emerald-400'"></div>
                    <span class="text-[10px] font-bold uppercase tracking-wider" :class="isSaving ? 'text-yellow-200' : 'text-emerald-100'">{{ isSaving ? 'Đang đồng bộ...' : 'Đã lưu đám mây' }}</span>
                </div>
                <div class="text-[10px] font-medium text-slate-400">{{ lastSavedTime }}</div>
            </div>

            <!-- Thanh tiến trình hoàn thiện CV -->
            <div class="space-y-2 relative group z-[100]">
                <div class="flex items-center justify-between px-0.5">
                    <div class="flex items-center gap-2">
                        <span class="text-[13px] font-black uppercase tracking-widest text-blue-200">Độ hoàn thiện CV</span>
                    </div>
                    <div class="flex items-center gap-2 relative z-[60]">
                        <span class="text-[13px] font-black text-white bg-blue-600 px-3 py-1 rounded-full shadow-lg shadow-blue-500/20">{{ completionPercentage }}%</span>
                        <!-- Nút Toggle Gợi ý mới (Bên phải %) -->
                        <button 
                            @click.stop="showTips = !showTips"
                            class="w-7 h-7 flex items-center justify-center rounded-full bg-blue-500 hover:bg-blue-600 text-white border border-blue-400/30 transition-all shadow-lg cursor-pointer"
                            :class="{ 'rotate-180 bg-slate-800': showTips }"
                            style="pointer-events: auto;"
                            title="Xem gợi ý hoàn thiện"
                        >
                            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M9 5l7 7-7 7"></path></svg>
                            <span v-if="pendingTipsCount > 0 && !showTips" class="absolute -top-1 -right-1 flex h-3 w-3">
                                <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-red-400 opacity-75"></span>
                                <span class="relative inline-flex rounded-full h-3 w-3 bg-red-500 border-2 border-slate-900"></span>
                            </span>
                        </button>
                    </div>
                </div>
                <div class="w-full h-3 bg-white/10 rounded-full overflow-hidden border border-white/5 p-[2px] cursor-pointer" @click="showTips = !showTips">
                    <div 
                        class="h-full rounded-full transition-all duration-1000 ease-out shadow-[0_0_15px_rgba(59,130,246,0.6)]"
                        :class="progressColorClass"
                        :style="{ width: `${completionPercentage}%` }"
                    ></div>
                </div>

                <!-- PANEL GỢI Ý HÀNH ĐỘNG (Bản Note bên phải Thông tin cá nhân) -->
                <!-- Removed from here -->
            </div>
        </div>
      </div>

      <div class="flex-1 min-h-0 flex flex-col relative" style="position: relative;">
          <!-- KHUNG CHỌN MẪU CV LỌT LÒNG LƠ LỬNG BÊN PHẢI (FLOATING POPOVER) -->
          <Teleport to="body">
              <div v-if="showTemplateModal" class="fixed bg-white rounded-[1.5rem] shadow-[0_20px_50px_rgba(0,0,0,0.22)] border border-slate-200/80 flex flex-col overflow-hidden animate-in fade-in zoom-in-95 duration-200" style="position: fixed; left: 608px; top: 175px; width: 480px; height: 660px; z-index: 99999;">
                  <!-- Header -->
                  <div class="px-5 py-3.5 border-b border-slate-100 flex items-center justify-between shrink-0 bg-white text-slate-800">
                      <div class="flex items-center gap-2">
                          <div class="w-7 h-7 bg-blue-500 rounded-lg flex items-center justify-center shadow-lg shadow-blue-500/20">
                              <svg class="w-3.5 h-3.5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z"></path></svg>
                          </div>
                          <div>
                              <h5 class="font-black uppercase tracking-tight text-[11px] text-slate-800">Thư viện mẫu CV</h5>
                          </div>
                      </div>
                      <button @click="showTemplateModal = false" class="w-7 h-7 rounded-full hover:bg-slate-100 flex items-center justify-center text-slate-400 hover:text-slate-600 transition-all">
                          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"></path></svg>
                      </button>
                  </div>

                  <!-- Body -->
                  <div class="flex-1 overflow-y-auto p-4 bg-slate-50 custom-scrollbar">
                      <div class="grid grid-cols-2 gap-3.5">
                          <div 
                              v-for="tpl in availableTemplates" 
                              :key="tpl.id"
                              @click="confirmChangeTemplate(tpl)"
                              class="group relative bg-white border border-slate-200/80 rounded-2xl overflow-hidden shadow-sm hover:shadow-md hover:-translate-y-0.5 transition-all duration-300 flex flex-col"
                              :class="[
                                tpl.componentName === (resumeData.overrideTemplate || initialTemplateName) ? 'ring-2 ring-blue-500 border-blue-500 scale-[0.98]' : '',
                                tpl.isPremium && !isProUser ? 'cursor-not-allowed' : 'cursor-pointer'
                              ]"
                          >
                              <!-- Thumbnail -->
                              <div class="aspect-[3/4] bg-slate-100 overflow-hidden relative">
                                  <img 
                                      :src="tpl.thumbnailUrl || `/images/templates/templatesCV_${tpl.id}.jpg`" 
                                      :alt="tpl.templateName" 
                                      class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" 
                                      :class="{ 'opacity-40 grayscale-[20%]': tpl.isPremium && !isProUser }"
                                  />
                                  
                                  <!-- Badge PRO/Premium -->
                                  <span v-if="tpl.isPremium" class="absolute top-2 right-2 bg-amber-500 text-white text-[7px] font-black px-1.5 py-0.5 rounded shadow-sm z-20">
                                      👑 PRO
                                  </span>
                                  <!-- Badge FREE -->
                                  <span v-else class="absolute top-2 right-2 bg-emerald-500 text-white text-[7px] font-black px-1.5 py-0.5 rounded shadow-sm z-20">
                                      FREE
                                  </span>
                                  
                                  <!-- Icon Tích tròn khi đang dùng -->
                                  <div v-if="tpl.componentName === (resumeData.overrideTemplate || initialTemplateName)" class="absolute bottom-2.5 left-2.5 w-5.5 h-5.5 rounded-full bg-blue-600 text-white flex items-center justify-center shadow-lg border-2 border-white z-20 animate-in zoom-in-50 duration-300">
                                      <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 13l4 4L19 7"></path></svg>
                                  </div>

                                  <!-- Overlay Khóa khi user Free xem mẫu Pro -->
                                  <div v-if="tpl.isPremium && !isProUser" class="absolute inset-0 bg-slate-900/40 backdrop-blur-[0.5px] flex flex-col items-center justify-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity duration-200 z-10">
                                      <span class="w-7 h-7 rounded-full bg-amber-500 text-white flex items-center justify-center shadow-lg">
                                          <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"></path></svg>
                                      </span>
                                      <span class="text-[8px] font-black text-white uppercase tracking-wider bg-slate-950/80 px-1.5 py-0.5 rounded shadow-sm">Bản PRO</span>
                                  </div>
                              </div>
                              
                              <!-- Meta info -->
                              <div class="p-2.5 bg-white flex items-center justify-center border-t border-slate-100 min-h-[34px]">
                                  <h4 class="font-extrabold text-slate-800 text-[10px] text-center line-clamp-1">{{ tpl.templateName }}</h4>
                              </div>
                          </div>
                      </div>
                  </div>
              </div>
          </Teleport>
 
          <!-- THANH ĐỔI MẪU CV (TEMPLATE SWITCHER) -->
          <div class="py-2.5 px-6 border-b border-slate-150 bg-slate-50 flex items-center justify-between shrink-0">
              <div class="flex items-center gap-2">
                  <span class="text-[11px] font-black text-slate-400 uppercase tracking-widest">Mẫu đang dùng:</span>
                  <span class="text-xs font-black text-blue-600 bg-blue-50 px-3 py-1 rounded-lg border border-blue-100 shadow-sm">{{ currentTemplateName }}</span>
              </div>
              <button @click="showTemplateModal = !showTemplateModal" class="flex items-center gap-1.5 bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs px-3 py-1.5 rounded-lg shadow-md hover:shadow-blue-500/20 active:scale-95 transition-all">
                  <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z"></path></svg>
                  Đổi mẫu CV
              </button>
          </div>
          <!-- TABS NAVIGATION CHO EDITOR -->
      <div class="cv-editor-tabs-container flex items-center p-2 bg-slate-100/80 rounded-xl mx-6 mt-4 shadow-inner gap-1 shrink-0 border border-slate-200/60 backdrop-blur-sm">
          <button @click="activeEditorTab = 'basic'" :class="[activeEditorTab === 'basic' ? 'cv-active bg-white text-blue-600 shadow-[0_2px_10px_rgba(37,99,235,0.1)] font-bold ring-1 ring-slate-200' : 'text-slate-500 hover:text-slate-700 hover:bg-slate-200/50', 'cv-editor-tab-btn flex-1 flex items-center justify-center gap-2 py-2.5 rounded-lg text-xs font-medium transition-all relative']">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
              Cá nhân
              <span v-if="basicTipsCount > 0" class="absolute top-1.5 right-2 w-2 h-2 bg-red-500 rounded-full shadow-[0_0_5px_rgba(239,68,68,0.6)]"></span>
          </button>
          <button @click="activeEditorTab = 'main'" :class="[activeEditorTab === 'main' ? 'cv-active bg-white text-blue-600 shadow-[0_2px_10px_rgba(37,99,235,0.1)] font-bold ring-1 ring-slate-200' : 'text-slate-500 hover:text-slate-700 hover:bg-slate-200/50', 'cv-editor-tab-btn flex-1 flex items-center justify-center gap-2 py-2.5 rounded-lg text-xs font-medium transition-all relative']">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M21 13.255A23.931 23.931 0 0112 15c-3.183 0-6.22-.62-9-1.745M16 6V4a2 2 0 00-2-2h-4a2 2 0 00-2 2v2m4 6h.01M5 20h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"></path></svg>
              Nội dung chính
              <span v-if="mainTipsCount > 0" class="absolute top-1.5 right-2 w-2 h-2 bg-red-500 rounded-full shadow-[0_0_5px_rgba(239,68,68,0.6)]"></span>
          </button>
          <button @click="activeEditorTab = 'skills'" :class="[activeEditorTab === 'skills' ? 'cv-active bg-white text-blue-600 shadow-[0_2px_10px_rgba(37,99,235,0.1)] font-bold ring-1 ring-slate-200' : 'text-slate-500 hover:text-slate-700 hover:bg-slate-200/50', 'cv-editor-tab-btn flex-1 flex items-center justify-center gap-2 py-2.5 rounded-lg text-xs font-medium transition-all relative']">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M13 10V3L4 14h7v7l9-11h-7z"></path></svg>
              Kỹ năng & Khác
              <span v-if="skillsTipsCount > 0" class="absolute top-1.5 right-2 w-2 h-2 bg-red-500 rounded-full shadow-[0_0_5px_rgba(239,68,68,0.6)]"></span>
          </button>
      </div>

      <!-- FORMS -->
      <div class="flex-1 overflow-y-auto p-6 space-y-8 scroll-smooth custom-scrollbar bg-slate-50 relative">
        

        
        <!-- THÔNG TIN CHUNG -->
        <div id="field-general" v-show="activeEditorTab === 'basic'" class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm relative animate-in fade-in slide-in-from-bottom-2 duration-300">
          <div class="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-blue-500 to-indigo-500"></div>
          
          <h2 class="text-xs uppercase font-bold text-slate-800 mb-4 tracking-wider flex items-center gap-2">Thông tin Cá nhân</h2>
          
          <!-- Avatar Upload -->
          <div id="field-avatar" class="flex items-center gap-4 mb-6 bg-blue-50/50 p-4 rounded-xl border border-dashed border-blue-200 transition-all">
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
                    <div id="field-fullName" class="flex flex-col gap-1.5 transition-all duration-500">
                        <label class="text-[13px] font-bold text-slate-700 ml-1">Họ và tên</label>
                        <RichTextEditor v-model="resumeData.general.fullName" class="w-full text-sm py-2.5 px-3 border border-slate-200 bg-white rounded-lg focus-within:ring-2 focus-within:ring-blue-500 outline-none transition-all font-medium placeholder-slate-400 shadow-sm" placeholder="Nhập họ tên đầy đủ..." />
                    </div>

                    <div id="field-jobTitle" class="flex flex-col gap-1.5 transition-all duration-500">
                        <label class="text-[13px] font-bold text-slate-700 ml-1">Vị trí ứng tuyển</label>
                        <RichTextEditor v-model="resumeData.general.jobTitle" class="w-full text-sm py-2.5 px-3 border border-slate-200 bg-white rounded-lg focus-within:ring-2 focus-within:ring-blue-500 outline-none transition-all placeholder-slate-400 shadow-sm" placeholder="Ví dụ: Fullstack Developer..." />
                    </div>
                </div>

                <div class="grid grid-cols-2 gap-4">
                    <div id="field-phone" class="flex flex-col gap-1.5 transition-all duration-500">
                        <label class="text-[13px] font-bold text-slate-700 ml-1">Số điện thoại</label>
                        <RichTextEditor v-model="resumeData.general.phone" class="w-full text-sm py-2 px-3 border border-slate-200 bg-white rounded-lg outline-none focus-within:ring-2 focus-within:ring-blue-500 placeholder-slate-400" placeholder="090..." />
                    </div>

                    <div id="field-email" class="flex flex-col gap-1.5 transition-all duration-500">
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
        <div class="animate-in fade-in slide-in-from-bottom-2 duration-300">
          <div v-show="activeEditorTab !== 'basic'" class="flex items-center justify-between mb-4 px-1 mt-6">
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
              <div v-show="isSectionInActiveTab(section.id)" :id="'section-' + section.id" class="bg-white border text-sm border-slate-200 rounded-2xl shadow-sm hover:shadow-md transition-all flex flex-col relative overflow-hidden group focus-within:ring-2 ring-blue-100 mb-4" :class="!section.isVisible ? 'opacity-60 bg-slate-50' : ''">
                
                <!-- Section Header -->
                <div class="flex items-center justify-between p-3.5 border-b border-slate-100 bg-slate-50/50">
                    <div class="flex items-center gap-2 flex-1">
                        <span class="drag-handle cursor-grab text-blue-600 hover:text-blue-700 active:cursor-grabbing p-1.5 bg-white rounded-lg shadow-sm border border-slate-200 transition-colors group-hover:border-blue-200 group-hover:bg-blue-50">
                            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" v-html="getSectionIcon(section.id)"></svg>
                        </span>
                        <RichTextEditor v-model="section.title" class="font-bold text-blue-700 bg-transparent py-1 px-2 rounded-md outline-none focus:ring-2 ring-blue-100 hover:bg-white w-full transition-all uppercase tracking-wide text-sm" />
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
                        <!-- CỤM ĐIỀU KHIỂN AI CỐ ĐỊNH PHÍA TRÊN -->
                        <div class="flex items-center gap-2 bg-slate-50 p-2 rounded-xl border border-slate-200">
                            <input 
                                type="text" 
                                v-model="summaryAIPrompt" 
                                class="flex-1 min-w-0 bg-white border border-slate-200 rounded-lg px-2.5 py-1.5 text-xs outline-none focus:border-blue-400 placeholder-slate-400 text-slate-700 shadow-sm"
                                placeholder="Nhập yêu cầu AI (ví dụ: Viết chuyên nghiệp hơn)..."
                                @keyup.enter="runSummaryAI"
                            />
                            <select 
                                v-model="summaryAIFunc" 
                                class="bg-white border border-slate-200 rounded-lg px-2 py-1.5 text-xs outline-none focus:border-blue-400 text-slate-700 cursor-pointer shadow-sm min-w-[130px]"
                            >
                                <option value="custom">🌟 Prompt tự chọn</option>
                                <option value="optimize">⚡ Tối ưu mục tiêu</option>
                                <option value="english">🌐 Đổi ngôn ngữ</option>
                                <option value="grammar">📝 Sửa chính tả</option>
                            </select>
                            <button 
                                @click="runSummaryAI" 
                                :disabled="isAIProcessing['summary']" 
                                class="cv-ai-btn shrink-0 flex items-center justify-center gap-1.5 bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-600 hover:to-orange-600 text-white text-xs font-bold px-4 py-1.5 rounded-full shadow-sm disabled:opacity-50 transition-all cursor-pointer"
                            >
                                <template v-if="isAIProcessing['summary']">
                                    <svg class="w-3.5 h-3.5 animate-spin text-white" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
                                </template>
                                <span v-else>✨ AI</span>
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
                                        <RichTextEditor v-model="item.company" :class="inputBaseClass" class="font-bold text-slate-800" placeholder="Tên Công ty" />
                                        <div class="grid grid-cols-2 gap-2">
                                            <RichTextEditor v-model="item.role" :class="inputBaseClass" placeholder="Vị trí làm việc" />
                                            <RichTextEditor v-model="item.time" :class="inputBaseClass" class="text-xs" placeholder="Thời gian (VD: 2020 - 2023)" />
                                        </div>
                                        <!-- CỤM ĐIỀU KHIỂN AI CỐ ĐỊNH PHÍA TRÊN -->
                                        <div class="flex items-center gap-2 bg-slate-50 p-2 rounded-xl border border-slate-200 mt-1">
                                            <input 
                                                type="text" 
                                                :value="aiPromptByItem[item._refId] || ''" 
                                                @input="e => aiPromptByItem[item._refId] = e.target.value"
                                                class="flex-1 min-w-0 bg-white border border-slate-200 rounded-lg px-2.5 py-1.5 text-xs outline-none focus:border-blue-400 placeholder-slate-400 text-slate-700 shadow-sm"
                                                placeholder="Nhập yêu cầu AI (ví dụ: Viết chuyên nghiệp hơn)..."
                                                @keyup.enter="runLocalAI(item, 'experience')"
                                            />
                                            <select 
                                                :value="aiFuncByItem[item._refId] || 'custom'" 
                                                @change="e => aiFuncByItem[item._refId] = e.target.value"
                                                class="bg-white border border-slate-200 rounded-lg px-2 py-1.5 text-xs outline-none focus:border-blue-400 text-slate-700 cursor-pointer shadow-sm min-w-[130px]"
                                            >
                                                <option value="custom">🌟 Prompt tự chọn</option>
                                                <option value="optimize">⚡ Tối ưu nội dung</option>
                                                <option value="english">🌐 Đổi ngôn ngữ</option>
                                                <option value="grammar">📝 Sửa chính tả</option>
                                            </select>
                                            <button 
                                                @click="runLocalAI(item, 'experience')" 
                                                :disabled="isAIProcessing[item._refId]" 
                                                class="cv-ai-btn shrink-0 flex items-center justify-center gap-1.5 bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-600 hover:to-orange-600 text-white text-xs font-bold px-4 py-1.5 rounded-full shadow-sm disabled:opacity-50 transition-all cursor-pointer"
                                            >
                                                <template v-if="isAIProcessing[item._refId]">
                                                    <svg class="w-3.5 h-3.5 animate-spin text-white" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
                                                </template>
                                                <span v-else>✨ AI</span>
                                            </button>
                                        </div>
                                        <RichTextEditor v-model="item.desc" :class="inputBaseClass" class="w-full leading-relaxed text-xs border border-transparent !px-2 focus-within:bg-blue-50 focus-within:rounded-md transition-colors" placeholder="Mô tả công việc (Dùng dấu • để liệt kê)" />
                                    </template>

                                    <!-- form học vấn -->
                                    <template v-else-if="section.id === 'education'">
                                        <RichTextEditor v-model="item.school" :class="inputBaseClass" class="font-bold text-slate-800" placeholder="Trường học" />
                                        <RichTextEditor v-model="item.major" :class="inputBaseClass" placeholder="Ngành/Chuyên khoa" />
                                        <div class="grid grid-cols-2 gap-2">
                                            <RichTextEditor v-model="item.year" :class="inputBaseClass" class="text-xs" placeholder="Năm học" />
                                            <RichTextEditor v-model="item.gradType" :class="inputBaseClass" class="text-xs" placeholder="Xếp loại (Giỏi/Khá)" />
                                        </div>
                                    </template>
                                    
                                    <!-- form dự án -->
                                    <template v-else-if="section.id === 'project'">
                                        <RichTextEditor v-model="item.name" :class="inputBaseClass" class="font-bold text-slate-800" placeholder="Tên dự án" />
                                        <div class="grid grid-cols-2 gap-2">
                                            <RichTextEditor v-model="item.role" :class="inputBaseClass" placeholder="Vai trò" />
                                            <RichTextEditor v-model="item.time" :class="inputBaseClass" class="text-xs" placeholder="Thời gian" />
                                        </div>
                                        <!-- CỤM ĐIỀU KHIỂN AI CỐ ĐỊNH PHÍA TRÊN -->
                                        <div class="flex items-center gap-2 bg-slate-50 p-2 rounded-xl border border-slate-200 mt-1">
                                            <input 
                                                type="text" 
                                                :value="aiPromptByItem[item._refId] || ''" 
                                                @input="e => aiPromptByItem[item._refId] = e.target.value"
                                                class="flex-1 min-w-0 bg-white border border-slate-200 rounded-lg px-2.5 py-1.5 text-xs outline-none focus:border-blue-400 placeholder-slate-400 text-slate-700 shadow-sm"
                                                placeholder="Nhập yêu cầu AI (ví dụ: Viết chuyên nghiệp hơn)..."
                                                @keyup.enter="runLocalAI(item, 'project')"
                                            />
                                            <select 
                                                :value="aiFuncByItem[item._refId] || 'custom'" 
                                                @change="e => aiFuncByItem[item._refId] = e.target.value"
                                                class="bg-white border border-slate-200 rounded-lg px-2 py-1.5 text-xs outline-none focus:border-blue-400 text-slate-700 cursor-pointer shadow-sm min-w-[130px]"
                                            >
                                                <option value="custom">🌟 Prompt tự chọn</option>
                                                <option value="optimize">⚡ Tối ưu nội dung</option>
                                                <option value="english">🌐 Đổi ngôn ngữ</option>
                                                <option value="grammar">📝 Sửa chính tả</option>
                                            </select>
                                            <button 
                                                @click="runLocalAI(item, 'project')" 
                                                :disabled="isAIProcessing[item._refId]" 
                                                class="cv-ai-btn shrink-0 flex items-center justify-center gap-1.5 bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-600 hover:to-orange-600 text-white text-xs font-bold px-4 py-1.5 rounded-full shadow-sm disabled:opacity-50 transition-all cursor-pointer"
                                            >
                                                <template v-if="isAIProcessing[item._refId]">
                                                    <svg class="w-3.5 h-3.5 animate-spin text-white" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
                                                </template>
                                                <span v-else>✨ AI</span>
                                            </button>
                                        </div>
                                        <RichTextEditor v-model="item.desc" :class="inputBaseClass" class="w-full leading-relaxed text-xs border border-transparent !px-2 focus-within:bg-blue-50 focus-within:rounded-md transition-colors" placeholder="Công nghệ sử dụng, Kết quả đạt được..." />
                                    </template>

                                    <!-- form kỹ năng chung (name, level) -->
                                    <template v-if="section.id === 'skills' || section.id === 'languages' || section.id === 'it_skills'">
                                        <div class="grid grid-cols-3 gap-2">
                                            <RichTextEditor v-model="item.name" :class="inputBaseClass" class="col-span-2 font-semibold" placeholder="Tên (VD: Lập trình C#) hoặc Ngôn ngữ" />
                                            <RichTextEditor v-model="item.level" :class="inputBaseClass" class="text-xs" placeholder="Mức độ" />
                                        </div>
                                    </template>

                                    <!-- form hoạt động -->
                                    <template v-else-if="section.id === 'activities'">
                                        <RichTextEditor v-model="item.name" :class="inputBaseClass" class="font-bold text-slate-800" placeholder="Tên Hoạt động/Tổ chức" />
                                        <RichTextEditor v-model="item.time" :class="inputBaseClass" class="text-xs" placeholder="Thời gian" />
                                        <!-- CỤM ĐIỀU KHIỂN AI CỐ ĐỊNH PHÍA TRÊN -->
                                        <div class="flex items-center gap-2 bg-slate-50 p-2 rounded-xl border border-slate-200 mt-1">
                                            <input 
                                                type="text" 
                                                :value="aiPromptByItem[item._refId] || ''" 
                                                @input="e => aiPromptByItem[item._refId] = e.target.value"
                                                class="flex-1 min-w-0 bg-white border border-slate-200 rounded-lg px-2.5 py-1.5 text-xs outline-none focus:border-blue-400 placeholder-slate-400 text-slate-700 shadow-sm"
                                                placeholder="Nhập yêu cầu AI (ví dụ: Viết chuyên nghiệp hơn)..."
                                                @keyup.enter="runLocalAI(item, 'activities')"
                                            />
                                            <select 
                                                :value="aiFuncByItem[item._refId] || 'custom'" 
                                                @change="e => aiFuncByItem[item._refId] = e.target.value"
                                                class="bg-white border border-slate-200 rounded-lg px-2 py-1.5 text-xs outline-none focus:border-blue-400 text-slate-700 cursor-pointer shadow-sm min-w-[130px]"
                                            >
                                                <option value="custom">🌟 Prompt tự chọn</option>
                                                <option value="optimize">⚡ Tối ưu nội dung</option>
                                                <option value="english">🌐 Đổi ngôn ngữ</option>
                                                <option value="grammar">📝 Sửa chính tả</option>
                                            </select>
                                            <button 
                                                @click="runLocalAI(item, 'activities')" 
                                                :disabled="isAIProcessing[item._refId]" 
                                                class="cv-ai-btn shrink-0 flex items-center justify-center gap-1.5 bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-600 hover:to-orange-600 text-white text-xs font-bold px-4 py-1.5 rounded-full shadow-sm disabled:opacity-50 transition-all cursor-pointer"
                                            >
                                                <template v-if="isAIProcessing[item._refId]">
                                                    <svg class="w-3.5 h-3.5 animate-spin text-white" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
                                                </template>
                                                <span v-else>✨ AI</span>
                                            </button>
                                        </div>
                                        <RichTextEditor v-model="item.desc" :class="inputBaseClass" class="w-full leading-relaxed text-xs border border-transparent !px-2 focus-within:bg-blue-50 focus-within:rounded-md transition-colors" placeholder="Mô tả chi tiết hoạt động..." />
                                    </template>

                                    <!-- form chứng chỉ/giải thưởng -->
                                    <template v-else-if="section.id === 'certifications' || section.id === 'awards'">
                                        <RichTextEditor v-model="item.name" :class="inputBaseClass" class="font-semibold text-slate-800" placeholder="Tên giải thưởng / Chứng chỉ" />
                                        <RichTextEditor v-model="item.year" :class="inputBaseClass" class="text-xs" placeholder="Năm / Tổ chức cấp" />
                                    </template>

                                    <!-- form sở thích & thông tin thêm -->
                                    <template v-else-if="section.id === 'hobbies' || section.id === 'additional'">
                                        <RichTextEditor v-model="item.name" :class="inputBaseClass" class="font-semibold text-slate-800" :placeholder="section.id === 'additional' ? 'Thông tin thêm (VD: Có xe máy riêng)' : 'Sở thích (VD: Đọc sách)'" />
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
                        <button v-if="section.id === 'skills'" @click="generateAISkills(sectionIndex)" :disabled="isAIProcessing['skills']" class="cv-ai-btn mt-3 shrink-0 flex items-center justify-center gap-1.5 bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-600 hover:to-orange-600 text-white text-xs font-bold px-4 py-1.5 rounded-full shadow-sm disabled:opacity-50 transition-all cursor-pointer focus:outline-none">
                            <svg v-if="isAIProcessing['skills']" class="w-3.5 h-3.5 animate-spin text-white" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
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
  </div>

    <!-- CỘT PHẢI: PREVIEW PANEL THỜI GIAN THỰC -->
    <div class="flex-1 overflow-auto bg-slate-800 relative scroll-smooth pattern-dots" :style="{ height: '100%', '--theme-color': resumeData.theme.primaryColor }">
        
        <!-- RICH TEXT TOOLBAR (Chiết xuất lên Top toàn bộ) -->
        <div class="no-print sticky top-0 left-0 w-full z-40 bg-white/95 backdrop-blur-md border-b border-slate-200 shadow-sm px-6 py-3 flex items-center justify-between transition-all select-none gap-4 cv-builder-toolbar">
            
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
                        <div class="relative overflow-hidden w-4 h-4 rounded-sm border border-slate-200 shadow-sm" :class="resumeData.theme.primaryColor === '#2b5c8f' ? 'bg-white' : ''" :style="resumeData.theme.primaryColor === '#2b5c8f' ? {} : { backgroundColor: resumeData.theme.primaryColor }">
                            <svg v-if="resumeData.theme.primaryColor === '#2b5c8f'" class="absolute inset-0 w-full h-full text-red-500 opacity-70" viewBox="0 0 24 24" fill="none" stroke="currentColor"><line x1="22" y1="2" x2="2" y2="22" stroke-width="2"></line></svg>
                        </div>
                        <svg class="w-3 h-3 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path d="M19 9l-7 7-7-7" stroke-linecap="round" stroke-linejoin="round" stroke-width="2"/></svg>
                    </button>
                    
                    <!-- Dropdown Theme Color -->
                    <div v-if="showThemeMenu" class="absolute top-full left-0 mt-2 p-3 bg-white border border-slate-200 shadow-xl rounded-xl z-50 w-48 animate-in fade-in slide-in-from-top-2 duration-200">
                        <div class="text-[10px] font-bold text-slate-400 uppercase mb-2">Màu chủ đạo CV</div>
                        <div class="grid grid-cols-5 gap-2 mb-3">
                            <button v-for="color in presetColors" :key="color" @click="resumeData.theme.primaryColor = color; showThemeMenu = false" class="relative overflow-hidden w-6 h-6 rounded-md border border-slate-100 shadow-sm hover:scale-110 transition-transform" :class="color === '#2b5c8f' ? 'bg-white' : ''" :style="color === '#2b5c8f' ? {} : { backgroundColor: color }" :title="color === '#2b5c8f' ? 'Mặc định' : color">
                                <svg v-if="color === '#2b5c8f'" class="absolute inset-0 w-full h-full text-red-500 opacity-70" viewBox="0 0 24 24" fill="none" stroke="currentColor"><line x1="22" y1="2" x2="2" y2="22" stroke-width="2"></line></svg>
                            </button>
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
                            <button v-for="color in fontColors" :key="'font-'+color" @mousedown.prevent="execCmd('foreColor', color); showFontMenu = false" class="relative overflow-hidden w-6 h-6 rounded-md border border-slate-100 shadow-sm hover:scale-110 transition-transform" :class="color === 'inherit' ? 'bg-white' : ''" :style="color === 'inherit' ? {} : { backgroundColor: color }" :title="color === 'inherit' ? 'Mặc định' : color">
                                <svg v-if="color === 'inherit'" class="absolute inset-0 w-full h-full text-red-500 opacity-70" viewBox="0 0 24 24" fill="none" stroke="currentColor"><line x1="22" y1="2" x2="2" y2="22" stroke-width="2"></line></svg>
                            </button>
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
                
                <div class="w-px h-6 bg-slate-200 mx-1.5"></div>

                <!-- AI Spell Check -->
                <button @click="scanCVForGrammar" :disabled="isScanningGrammar" class="flex items-center gap-1.5 bg-gradient-to-r from-amber-50 to-orange-50 border border-amber-200 text-amber-700 px-3 py-1.5 rounded-lg hover:from-amber-100 hover:to-orange-100 transition-all shadow-sm group disabled:opacity-50" title="Quét lỗi chính tả & ngữ pháp bằng AI">
                    <template v-if="isScanningGrammar">
                        <svg class="w-3.5 h-3.5 animate-spin" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
                        <span class="text-[10px] font-black uppercase tracking-wider">Đang quét...</span>
                    </template>
                    <template v-else>
                        <svg class="w-3.5 h-3.5 text-amber-500 group-hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"></path></svg>
                        <span class="text-[10px] font-black uppercase tracking-wider">AI Scan</span>
                    </template>
                </button>
            </div>

            <!-- Nút Export (Trên thanh) -->
            <button @click="exportToPDF" :disabled="isExporting" class="shrink-0 bg-blue-600 text-white px-5 py-2.5 rounded-lg font-bold shadow-md shadow-blue-500/40 hover:bg-blue-500 transition-all text-sm flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed">
                <svg v-if="!isExporting" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z"></path></svg>
                <svg v-else class="w-4 h-4 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"></path></svg>
                {{ isExporting ? 'Đang xuất...' : 'Lưu Export PDF' }}
            </button>
        </div>

        <!-- Zoom Bar & AI Tools -->
        <div class="sticky top-[110px] pointer-events-none w-full flex flex-col items-end px-8 z-30 mb-8 mt-6 gap-3">
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

            <!-- Nút Trình tạo Thư xin việc AI (Cover Letter) -->
            <div class="pointer-events-auto">
                <button 
                    type="button" 
                    class="flex items-center gap-1.5 bg-gradient-to-br from-indigo-600 via-blue-600 to-emerald-500 text-white px-2.5 py-1.5 rounded-xl font-black text-[9px] uppercase tracking-wider shadow-lg shadow-blue-500/30 hover:shadow-blue-500/50 hover:scale-105 active:scale-95 transition-all animate-in slide-in-from-right-10 duration-700"
                    data-bs-toggle="modal" 
                    data-bs-target="#coverLetterModal"
                >
                    <span class="flex items-center justify-center w-3.5 h-3.5 bg-white/20 rounded-full">
                        <svg class="w-2.5 h-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z"></path></svg>
                    </span>
                    <span>Cover Letter</span>
                    <span class="bg-amber-400 text-slate-900 text-[8px] font-black px-1.5 py-0.5 rounded-full ml-1 inline-flex items-center gap-0.5 shadow-sm scale-110 shrink-0">
                        <svg class="w-2.5 h-2.5 text-slate-900 shrink-0" viewBox="0 0 24 24" fill="currentColor">
                            <path d="M2 19h20v2H2v-2zm2-3L2 8l5 4 5-7 5 7 5-4-2 8H4z"/>
                        </svg>
                        PRO
                    </span>
                </button>
            </div>

            <!-- Nút Job Matcher (So khớp CV với JD) -->
            <div class="pointer-events-auto">
                <button 
                    type="button" 
                    @click="showJobMatcherModal = true"
                    class="flex items-center gap-1.5 bg-gradient-to-br from-rose-500 via-orange-500 to-amber-400 text-white px-2.5 py-1.5 rounded-xl font-black text-[9px] uppercase tracking-wider shadow-lg shadow-orange-500/30 hover:shadow-orange-500/50 hover:scale-105 active:scale-95 transition-all animate-in slide-in-from-right-10 duration-700"
                >
                    <span class="flex items-center justify-center w-3.5 h-3.5 bg-white/20 rounded-full">
                        <svg class="w-2.5 h-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"></path></svg>
                    </span>
                    <span>Job Matcher</span>
                    <span class="bg-amber-400 text-slate-900 text-[8px] font-black px-1.5 py-0.5 rounded-full ml-1 inline-flex items-center gap-0.5 shadow-sm scale-110 shrink-0">
                        <svg class="w-2.5 h-2.5 text-slate-900 shrink-0" viewBox="0 0 24 24" fill="currentColor">
                            <path d="M2 19h20v2H2v-2zm2-3L2 8l5 4 5-7 5 7 5-4-2 8H4z"/>
                        </svg>
                        PRO
                    </span>
                </button>
            </div>
        </div>

        <!-- Vùng chứa CV: Dùng flex-col items-center và margin động để thanh cuộn khớp với tỉ lệ scale -->
        <div class="cv-preview-wrapper flex flex-col items-center pt-8 pb-32 min-w-max">
            <div class="cv-preview-card transition-transform duration-300 origin-top shadow-2xl bg-white flex-shrink-0" 
                 @click.capture="handlePreviewClick"
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
    <div id="exportPreviewModalContent" class="relative bg-white w-[85vw] h-[96vh] rounded-[1.5rem] shadow-[0_25px_70px_rgba(0,0,0,0.4)] overflow-hidden flex flex-col animate-in zoom-in-95 slide-in-from-bottom-10 duration-500">
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

  <!-- MODAL TRÌNH TẠO THƯ XIN VIỆC AI -->
  <div class="modal fade" id="coverLetterModal" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog modal-xl modal-dialog-centered" style="max-width: 1200px;">
      <div class="modal-content border-0 shadow-2xl rounded-3xl overflow-hidden">
        <div class="modal-header bg-slate-900 text-white py-4 px-6 border-0">
          <div class="flex items-center gap-3">
             <div class="w-10 h-10 bg-blue-600 rounded-2xl flex items-center justify-center shadow-lg shadow-blue-500/30">
                <svg class="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M13 10V3L4 14h7v7l9-11h-7z"></path></svg>
             </div>
             <div>
                <h5 class="modal-title font-black uppercase tracking-tight text-sm">Trình tạo Thư xin việc AI</h5>
                <p class="text-[10px] text-slate-400 font-bold tracking-widest uppercase">Powered by Gemini AI</p>
             </div>
          </div>
          <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
        </div>
        <div class="relative flex-1 flex flex-col min-h-0">
          <!-- Absolute Overlay khi !isProUser -->
          <div v-if="!isProUser" class="absolute inset-0 z-50 bg-slate-900/10 backdrop-blur-[1px] flex items-center justify-center p-6">
              <!-- PRO Banner Ngang Dài -->
              <div class="bg-[#fffbeb] border border-amber-200 rounded-3xl px-8 py-6 max-w-6xl w-full flex flex-col sm:flex-row items-center justify-between gap-6 shadow-[0_20px_50px_rgba(0,0,0,0.15)] transition-all">
                  <div class="flex items-center gap-4 text-left">
                      <span class="text-3xl text-slate-900 flex items-center shrink-0">
                          <svg class="w-8 h-8" viewBox="0 0 24 24" fill="currentColor">
                              <path d="M2 19h20v2H2v-2zm2-3L2 8l5 4 5-7 5 7 5-4-2 8H4z"/>
                          </svg>
                      </span>
                      <p class="text-sm md:text-base font-black text-amber-900 mb-0">Tính năng này chỉ dành cho thành viên PRO. Hãy nâng cấp tài khoản để sử dụng!</p>
                  </div>
                  <div class="flex items-center gap-5 shrink-0">
                      <a href="/Account/Upgrade" class="px-6 py-3.5 bg-amber-500 hover:bg-amber-600 rounded-full font-black text-xs md:text-sm uppercase tracking-wider transition-all flex items-center gap-2 shadow-md" style="color: #0f172a !important; text-decoration: none !important;">
                          <span>💎</span> Nâng cấp lên Pro ngay
                      </a>
                      <button type="button" class="text-slate-400 hover:text-slate-600 transition-colors" data-bs-dismiss="modal">
                          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"></path></svg>
                      </button>
                  </div>
              </div>
          </div>

          <!-- Nội dung body và footer thực tế bị làm mờ và chặn tương tác khi không phải PRO -->
          <div :class="{'filter blur-[1px] pointer-events-none select-none opacity-50 transition-all duration-300': !isProUser}" class="flex-1 flex flex-col min-h-0">
            <div class="modal-body p-6 bg-slate-50 flex-1">
              <div class="grid grid-cols-1 md:grid-cols-12 gap-6">
                <!-- CỘT TRÁI: NHẬP THÔNG TIN (HẸP HƠN - 5 CỘT) -->
                <div class="md:col-span-5 space-y-4 flex flex-col justify-between">
                  <div class="space-y-2">
                      <label class="text-[11px] font-black text-slate-500 uppercase tracking-widest ml-1">Công ty & Vị trí ứng tuyển <span class="text-red-500">*</span></label>
                      <input 
                          type="text" 
                          v-model="targetCompany" 
                          :disabled="!isProUser"
                          class="w-full bg-white border border-slate-200 rounded-2xl px-5 py-3.5 text-sm font-medium focus:ring-4 focus:ring-blue-100 focus:border-blue-500 transition-all shadow-sm outline-none" 
                          placeholder="Ví dụ: FPT Software - Vị trí .NET Developer..."
                      >
                  </div>

                  <div class="space-y-2 flex-1 flex flex-col">
                      <label class="text-[11px] font-black text-slate-500 uppercase tracking-widest ml-1">Mô tả công việc (Tùy chọn - Giúp AI viết sát hơn)</label>
                      <textarea 
                          v-model="coverLetterJD" 
                          :disabled="!isProUser"
                          class="w-full flex-1 bg-white border border-slate-200 rounded-2xl px-5 py-3.5 text-sm focus:ring-4 focus:ring-blue-100 focus:border-blue-500 transition-all shadow-sm outline-none resize-none custom-scrollbar min-h-[300px]" 
                          rows="10"
                          placeholder="Dán nội dung yêu cầu công việc (JD) vào đây..."
                      ></textarea>
                  </div>
                </div>

                <!-- CỘT PHẢI: KẾT QUẢ AI TẠO (RỘNG HƠN - 7 CỘT) -->
                <div class="md:col-span-7 space-y-2 flex flex-col">
                    <div class="flex items-center justify-between px-1">
                        <label class="text-[11px] font-black text-slate-500 uppercase tracking-widest">Nội dung thư gợi ý</label>
                        <div class="flex items-center gap-4">
                            <button v-if="coverLetterResult" @click="downloadCoverLetter" class="text-[10px] font-black text-emerald-600 hover:text-emerald-700 flex items-center gap-1.5 transition-colors uppercase tracking-widest">
                                <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"></path></svg>
                                Tải TXT
                            </button>
                            <button v-if="coverLetterResult" @click="copyCoverLetter" class="text-[10px] font-black text-blue-600 hover:text-blue-700 flex items-center gap-1.5 transition-colors uppercase tracking-widest">
                                <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M8 5H6a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2v-1M8 5a2 2 0 002 2h2a2 2 0 002-2M8 5a2 2 0 002 2h2a2 2 0 002-2M8 5a2 2 0 012-2h2a2 2 0 012 2m0 0h2a2 2 0 012 2v3m2 4H10m0 0l3-3m-3 3l3 3"></path></svg>
                                Sao chép
                            </button>
                        </div>
                    </div>
                    <div class="relative group flex-1">
                        <textarea 
                            v-model="coverLetterResult" 
                            :disabled="!isProUser"
                            class="w-full h-full min-h-[400px] bg-white border border-slate-200 rounded-2xl px-5 py-4 text-sm leading-relaxed focus:ring-4 focus:ring-blue-100 focus:border-blue-500 transition-all shadow-sm outline-none custom-scrollbar resize-none" 
                            placeholder="Nội dung thư xin việc chuyên nghiệp sẽ xuất hiện tại đây..."
                            rows="16"
                        ></textarea>
                        
                        <div v-if="isAIProcessing['cover_letter']" class="absolute inset-0 bg-white/60 backdrop-blur-[2px] rounded-2xl flex flex-col items-center justify-center gap-3">
                            <div class="relative w-12 h-12">
                                <div class="absolute inset-0 border-4 border-blue-100 rounded-full"></div>
                                <div class="absolute inset-0 border-4 border-blue-600 rounded-full border-t-transparent animate-spin"></div>
                            </div>
                            <span class="text-[11px] font-black text-blue-600 uppercase tracking-[0.2em] animate-pulse">AI đang soạn thảo...</span>
                        </div>
                    </div>
                </div>
              </div>
            </div>
            <div class="modal-footer bg-white border-t border-slate-100 p-4 px-6 flex items-center justify-between">
                <p class="text-[10px] text-slate-400 font-medium italic">* AI sẽ dựa vào thông tin CV của bạn để viết thư.</p>
                <div class="flex items-center gap-3">
                    <button type="button" class="px-6 py-2.5 rounded-xl font-bold text-[11px] uppercase tracking-widest text-slate-500 hover:bg-slate-50 transition-all" data-bs-dismiss="modal">Đóng</button>
                    <button 
                      @click="generateAICoverLetter" 
                      :disabled="!isProUser || isAIProcessing['cover_letter']"
                      class="bg-slate-900 text-white px-8 py-2.5 rounded-xl font-black text-[11px] uppercase tracking-[0.2em] shadow-xl shadow-slate-900/20 hover:bg-blue-600 hover:shadow-blue-600/30 transition-all active:scale-95 disabled:opacity-50"
                    >
                        {{ isAIProcessing['cover_letter'] ? 'Đang xử lý...' : '✨ Bắt đầu viết' }}
                    </button>
                </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- MODAL JOB MATCHER (So khớp CV & JD) -->
  <div v-if="showJobMatcherModal" class="fixed inset-0 z-[10000] flex items-center justify-center p-4">
    <!-- Backdrop -->
    <div class="absolute inset-0 bg-slate-900/60 backdrop-blur-md" @click="showJobMatcherModal = false"></div>
    
    <!-- Modal Content -->
    <!-- Modal Content -->
    <div id="jobMatcherModalContent" class="relative bg-white w-[95vw] max-w-[1400px] h-auto max-h-[96vh] rounded-[1.5rem] shadow-[0_25px_70px_rgba(0,0,0,0.4)] overflow-hidden flex flex-col">
        <!-- Modal Header -->
        <div class="px-6 py-4 border-b border-slate-100 flex items-center justify-between shrink-0 bg-gradient-to-r from-slate-900 via-slate-800 to-slate-900 text-white">
            <div class="flex items-center gap-3">
                <div class="w-10 h-10 bg-gradient-to-br from-orange-500 to-rose-500 rounded-2xl flex items-center justify-center shadow-lg shadow-orange-500/30">
                    <svg class="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"></path></svg>
                </div>
                <div>
                    <h5 class="font-black uppercase tracking-tight text-sm">AI Job Matcher</h5>
                    <p class="text-[10px] text-slate-400 font-bold tracking-widest uppercase">So khớp CV với mô tả công việc</p>
                </div>
            </div>
            <button @click="showJobMatcherModal = false" class="w-10 h-10 rounded-full hover:bg-white/10 flex items-center justify-center text-slate-400 hover:text-white transition-all">
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"></path></svg>
            </button>
        </div>
        
        <!-- Modal Body -->
        <div class="relative flex-1 flex flex-col min-h-0">
            <!-- Absolute Overlay khi !isProUser -->
            <div v-if="!isProUser" class="absolute inset-0 z-50 bg-slate-900/10 backdrop-blur-[1px] flex items-center justify-center p-6">
                <!-- PRO Banner Ngang Dài -->
                <div class="bg-[#fffbeb] border border-amber-200 rounded-2xl px-8 py-6 max-w-6xl w-full flex flex-col sm:flex-row items-center justify-between gap-6 shadow-[0_20px_50px_rgba(0,0,0,0.15)] transition-all">
                    <div class="flex items-center gap-4 text-left">
                        <span class="text-3xl text-slate-900 flex items-center shrink-0">
                            <svg class="w-8 h-8" viewBox="0 0 24 24" fill="currentColor">
                                <path d="M2 19h20v2H2v-2zm2-3L2 8l5 4 5-7 5 7 5-4-2 8H4z"/>
                            </svg>
                        </span>
                        <p class="text-sm md:text-base font-black text-amber-900 mb-0">Tính năng này chỉ dành cho thành viên PRO. Hãy nâng cấp tài khoản để sử dụng!</p>
                    </div>
                    <div class="flex items-center gap-5 shrink-0">
                        <a href="/Account/Upgrade" class="px-6 py-3.5 bg-amber-500 hover:bg-amber-600 rounded-full font-black text-xs md:text-sm uppercase tracking-wider transition-all flex items-center gap-2 shadow-md" style="color: #0f172a !important; text-decoration: none !important;">
                            <span>💎</span> Nâng cấp lên Pro ngay
                        </a>
                        <button type="button" class="text-slate-400 hover:text-slate-600 transition-colors" @click="showJobMatcherModal = false">
                            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"></path></svg>
                        </button>
                    </div>
                </div>
            </div>

            <!-- Nội dung body thực tế bị làm mờ và chặn tương tác khi không phải PRO -->
            <div :class="{'filter blur-[1px] pointer-events-none select-none opacity-50 transition-all duration-300': !isProUser}" class="flex-1 flex flex-col min-h-0">
                <div class="flex-1 overflow-y-auto p-6 custom-scrollbar">
                    <div class="flex gap-6" :class="jobMatchResult ? 'flex-row' : 'flex-col items-center'">
                        <!-- Cột trái: Nhập JD -->
                        <div :class="jobMatchResult ? 'w-[35%] shrink-0 flex flex-col' : 'w-full max-w-[1000px] flex flex-col'" class="space-y-4">
                            <div class="space-y-2 flex-1 flex flex-col">
                                <label class="text-[11px] font-black text-slate-500 uppercase tracking-widest ml-1">Nội dung mô tả công việc (JD)</label>
                                <textarea 
                                    v-model="jobDescription" 
                                    :disabled="!isProUser"
                                    class="w-full flex-1 bg-slate-50 border border-slate-200 rounded-2xl px-5 py-4 text-sm leading-relaxed focus:ring-4 focus:ring-orange-100 focus:border-orange-400 transition-all shadow-sm outline-none custom-scrollbar resize-none" 
                                    :class="jobMatchResult ? 'min-h-[500px]' : 'min-h-[370px]'"
                                    placeholder="Ví dụ:
- Vị trí: Backend Developer
- Yêu cầu: Thành thạo C#, ASP.NET Core, SQL Server, Docker...
- Kinh nghiệm: Tối thiểu 2 năm..."
                                ></textarea>
                            </div>
                            
                            <button 
                                @click="analyzeJobMatch" 
                                :disabled="!isProUser || isMatchingJob || !jobDescription.trim()"
                                class="w-full bg-gradient-to-r from-orange-500 to-rose-500 text-white py-3.5 rounded-xl font-black text-[12px] uppercase tracking-[0.15em] shadow-xl shadow-orange-500/20 hover:shadow-orange-500/40 transition-all active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
                            >
                                <template v-if="isMatchingJob">
                                    <svg class="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
                                    <span>AI đang phân tích...</span>
                                </template>
                                <template v-else>
                                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M13 10V3L4 14h7v7l9-11h-7z"></path></svg>
                                    <span>🎯 Phân tích mức độ phù hợp</span>
                                </template>
                            </button>
                        </div>

                        <!-- Cột phải: Kết quả -->
                        <div v-if="jobMatchResult" class="flex-1 space-y-5 min-w-0">
                            <!-- Match Score Circle -->
                            <div class="bg-gradient-to-br from-slate-50 to-white border border-slate-100 rounded-2xl p-6 text-center shadow-sm">
                                <div class="relative w-32 h-32 mx-auto mb-4">
                                    <svg class="w-32 h-32 -rotate-90" viewBox="0 0 120 120">
                                        <circle cx="60" cy="60" r="52" stroke="#e2e8f0" stroke-width="10" fill="none"/>
                                        <circle cx="60" cy="60" r="52" 
                                            :stroke="matchScoreColor" 
                                            stroke-width="10" 
                                            fill="none" 
                                            stroke-linecap="round"
                                            :stroke-dasharray="`${jobMatchResult.matchScore * 3.267} 326.7`"
                                            class="transition-all duration-1000 ease-out"
                                        />
                                    </svg>
                                    <div class="absolute inset-0 flex flex-col items-center justify-center">
                                        <span class="text-3xl font-black" :style="{ color: matchScoreColor }">{{ jobMatchResult.matchScore }}%</span>
                                        <span class="text-[9px] font-bold text-slate-400 uppercase tracking-widest">Match Score</span>
                                    </div>
                                </div>
                                <p class="text-sm font-bold" :style="{ color: matchScoreColor }">{{ matchScoreLabel }}</p>
                            </div>

                            <!-- Kỹ năng khớp -->
                            <div v-if="jobMatchResult.matchedSkills?.length" class="bg-emerald-50/50 border border-emerald-100 rounded-2xl p-4">
                                <h4 class="text-[11px] font-black text-emerald-700 uppercase tracking-widest mb-3 flex items-center gap-2">
                                    <span class="w-5 h-5 bg-emerald-500 rounded-full flex items-center justify-center text-white text-[10px]">✓</span>
                                    Kỹ năng đã khớp ({{ jobMatchResult.matchedSkills.length }})
                                </h4>
                                <div class="flex flex-wrap gap-2">
                                    <span v-for="skill in jobMatchResult.matchedSkills" :key="'matched-'+skill" 
                                        class="px-3 py-1.5 bg-emerald-100 text-emerald-800 rounded-full text-[11px] font-bold border border-emerald-200">✅ {{ skill }}</span>
                                </div>
                            </div>

                            <!-- Kỹ năng thiếu -->
                            <div v-if="jobMatchResult.missingSkills?.length" class="bg-rose-50/50 border border-rose-100 rounded-2xl p-4">
                                <h4 class="text-[11px] font-black text-rose-700 uppercase tracking-widest mb-3 flex items-center gap-2">
                                    <span class="w-5 h-5 bg-rose-500 rounded-full flex items-center justify-center text-white text-[10px]">✗</span>
                                    Kỹ năng còn thiếu ({{ jobMatchResult.missingSkills.length }})
                                </h4>
                                <div class="flex flex-wrap gap-2">
                                    <span v-for="skill in jobMatchResult.missingSkills" :key="'missing-'+skill" 
                                        class="px-3 py-1.5 bg-rose-100 text-rose-800 rounded-full text-[11px] font-bold border border-rose-200">❌ {{ skill }}</span>
                                </div>
                            </div>

                            <!-- Gợi ý cải thiện -->
                            <div v-if="jobMatchResult.suggestions?.length" class="bg-blue-50/50 border border-blue-100 rounded-2xl p-4">
                                <h4 class="text-[11px] font-black text-blue-700 uppercase tracking-widest mb-3 flex items-center gap-2">
                                    <span class="w-5 h-5 bg-blue-500 rounded-full flex items-center justify-center text-white text-[10px]">💡</span>
                                    Gợi ý cải thiện CV
                                </h4>
                                <ul class="space-y-2">
                                    <li v-for="(suggestion, idx) in jobMatchResult.suggestions" :key="'sug-'+idx" 
                                        class="text-sm text-slate-700 leading-relaxed flex items-start gap-2 bg-white p-3 rounded-xl border border-blue-50">
                                        <span class="shrink-0 w-5 h-5 bg-blue-100 text-blue-600 rounded-full flex items-center justify-center text-[10px] font-black mt-0.5">{{ idx + 1 }}</span>
                                        <span>{{ suggestion }}</span>
                                    </li>
                                </ul>
                            </div>

                            <!-- Nhận xét tổng quan -->
                            <div v-if="jobMatchResult.summary" class="bg-amber-50/50 border border-amber-100 rounded-2xl p-4">
                                <h4 class="text-[11px] font-black text-amber-700 uppercase tracking-widest mb-2 flex items-center gap-2">
                                    <span>📋</span> Nhận xét tổng quan
                                </h4>
                                <p class="text-sm text-slate-700 leading-relaxed italic">"{{ jobMatchResult.summary }}"</p>
                            </div>
                        </div>

                        <!-- Placeholder khi chưa có kết quả -->
                        <div v-if="!jobMatchResult && !isMatchingJob" class="w-full flex items-center justify-start gap-3 py-0 mt-[-1rem]">
                            <span class="text-2xl opacity-40">🎯</span>
                            <span class="text-slate-400 text-sm font-medium">Dán nội dung JD vào ô trên và nhấn nút phân tích để xem mức độ phù hợp của CV với công việc.</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>


    </div>
  </div>

</template>

<script setup>
import { ref, reactive, onMounted, onUnmounted, watch, shallowRef, defineAsyncComponent, computed, nextTick } from 'vue'
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
    additional: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />', // Information Circle
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
const lastSavedTime = ref('')
const initialVisibleSectionIds = ref([]) // Lưu danh sách các mục active ban đầu của CV này

// --- ĐỔI MẪU CV ---
// Quản lý trạng thái và chức năng đổi mẫu CV động
const showTemplateModal = ref(false)
const availableTemplates = ref([])
const initialTemplateName = ref('')

// Lấy danh sách các mẫu CV Vue đang active trên hệ thống
const fetchTemplates = async () => {
    try {
        const res = await fetch('/api/cvbuilder/templates');
        const data = await res.json();
        availableTemplates.value = data || [];
    } catch (e) {
        console.error('[CV Builder] Lỗi fetch templates:', e);
    }
}

// Lấy tên hiển thị tiếng Việt của mẫu CV hiện tại
const currentTemplateName = computed(() => {
    const cmp = resumeData.value.overrideTemplate || initialTemplateName.value || 'Template';
    const found = availableTemplates.value.find(t => t.componentName === cmp);
    return found ? found.templateName : cmp;
});

// Tự động bật/tắt (active/deactive) các mục của CV dựa trên nội dung thực tế của người dùng
const optimizeSectionVisibilityOnTemplateChange = () => {
    resumeData.value.sections.forEach(s => {
        if (s.id === 'summary') {
            const hasContent = resumeData.value.general.summary && 
                              resumeData.value.general.summary.replace(/<[^>]*>/g, '').replace(/&nbsp;/g, ' ').trim().length > 0;
            s.isVisible = !!hasContent;
        } else {
            const hasContent = s.items && s.items.length > 0 && s.items.some(item => {
                return Object.entries(item).some(([key, v]) => {
                    if (key === '_refId') return false;
                    if (typeof v !== 'string') return false;
                    const cleanV = v.replace(/<[^>]*>/g, '').replace(/&nbsp;/g, ' ').trim();
                    return cleanV !== '';
                });
            });
            s.isVisible = !!hasContent;
        }
    });
};

// Xác nhận đổi mẫu CV và tối ưu hóa phân mục
const confirmChangeTemplate = (tpl) => {
    if (tpl.isPremium && !isProUser.value) {
        alert("Mẫu CV này chỉ dành cho thành viên PRO. Vui lòng nâng cấp tài khoản để sử dụng!");
        return;
    }
    
    // Ghi đè template và chuyển đổi component render
    resumeData.value.overrideTemplate = tpl.componentName;
    if (templateRegistry[tpl.componentName]) {
        activeTemplate.value = templateRegistry[tpl.componentName];
    }
    
    // Tự động lọc bật các mục có nội dung và tắt các mục thừa
    optimizeSectionVisibilityOnTemplateChange();
    
    // Áp dụng định vị cột mặc định khi chuyển đổi sang các template cấu trúc cột cố định
    if (tpl.componentName === 'VuTungDuong') {
        const rightSections = ['summary', 'experience', 'education', 'activities'];
        const leftSections = ['skills', 'certifications', 'awards', 'hobbies', 'references'];
        resumeData.value.sections.forEach(s => {
            if (rightSections.includes(s.id)) s.column = 'right';
            else if (leftSections.includes(s.id)) s.column = 'left';
        });
        if (!resumeData.value.theme) resumeData.value.theme = {};
        resumeData.value.theme.primaryColor = '#2b5c8f';
    } else if (tpl.componentName === 'NguyenThanhNhatNam') {
        const rightSections = ['education', 'project', 'activities', 'additional'];
        const leftSections = ['skills', 'certifications', 'hobbies', 'summary'];
        resumeData.value.sections.forEach(s => {
            if (rightSections.includes(s.id)) s.column = 'right';
            else if (leftSections.includes(s.id)) s.column = 'left';
        });
    }

    // Cập nhật tính phản ứng của danh sách sections
    sortSectionsByVisibility();
    
    // Đóng Modal chọn mẫu
    showTemplateModal.value = false;
    console.log(`[CV Builder] Đã chuyển đổi thành công sang mẫu: ${tpl.templateName}`);
};



// --- LOGIC TÍNH % HOÀN THIỆN CV (LINH HOẠT THEO TỪNG MẪU CV) ---
const completionPercentage = computed(() => {
    const data = resumeData.value;
    const g = data.general;
    
    // 1. Danh sách các trường thông tin cá nhân cần kiểm tra (7 trường)
    const personalFields = [
        { val: g.fullName, label: 'Họ tên' },
        { val: g.jobTitle, label: 'Vị trí' },
        { val: g.email, label: 'Email' },
        { val: g.phone, label: 'SĐT' },
        { val: g.address, label: 'Địa chỉ' },
        { val: g.avatarUrl, label: 'Ảnh' },
        { val: g.summary, label: 'Mục tiêu' }
    ];

    let totalPoints = personalFields.length;
    let currentPoints = personalFields.filter(f => {
        if (!f.val) return false;
        // Xóa hết tags HTML và thực thể HTML trước khi check nội dung (vì dùng RichTextEditor)
        const val = f.val.replace(/<[^>]*>/g, '').replace(/&nbsp;/g, ' ').trim();
        
        // Không tính nếu là chuỗi rỗng hoặc các placeholder mặc định
        return val !== '' && 
               val !== 'Nhập họ tên đầy đủ...' && 
               val !== 'Vị trí ứng tuyển' &&
               val !== 'Ứng viên năng động, mong chờ cơ hội.';
    }).length;

    // 2. Chỉ tính điểm cho các mục (Sections) đang hiển thị (isVisible)
    // Ưu tiên sử dụng danh sách mục active ban đầu nếu đã được ghi lại
    const sectionsToTrack = data.sections.filter(s => {
        if (initialVisibleSectionIds.value.length > 0) {
            return initialVisibleSectionIds.value.includes(s.id);
        }
        return s.isVisible && s.id !== 'summary'; // summary đã tính ở phần personal
    });

    totalPoints += sectionsToTrack.length;
    
    sectionsToTrack.forEach(s => {
        const hasContent = s.items?.length > 0 && s.items.some(item => {
            // Kiểm tra từng trường trong item, bỏ qua trường ID hệ thống _refId
            return Object.entries(item).some(([key, v]) => {
                if (key === '_refId') return false;
                if (typeof v !== 'string') return false;
                
                // Loại bỏ HTML trước khi kiểm tra
                const cleanV = v.replace(/<[^>]*>/g, '').replace(/&nbsp;/g, ' ').trim();
                return cleanV !== '';
            });
        });
        if (hasContent) currentPoints++;
    });

    if (totalPoints === 0) return 0;
    return Math.round((currentPoints / totalPoints) * 100);
});

const showTips = ref(false);
watch(showTips, (val) => {
    console.log('[CV Builder] showTips changed:', val);
});

const completionTips = computed(() => {
    const data = resumeData.value;
    const g = data.general;
    const tips = [];

    // Helper kiểm tra nội dung
    const isEmpty = (val) => {
        if (!val) return true;
        const clean = val.replace(/<[^>]*>/g, '').replace(/&nbsp;/g, ' ').trim();
        return clean === '' || clean === 'Nhập họ tên đầy đủ...' || clean === 'Vị trí ứng tuyển';
    };

    // 1. Ảnh đại diện
    tips.push({
        id: 'avatar',
        label: 'Tải ảnh chân dung',
        hint: 'Ảnh giúp nhà tuyển dụng tin tưởng bạn hơn 40%.',
        isDone: !!g.avatarUrl,
        targetId: 'field-avatar'
    });

    // 2. Họ tên
    tips.push({
        id: 'fullName',
        label: 'Nhập họ và tên',
        hint: 'Hãy ghi tên thật đầy đủ của bạn.',
        isDone: !isEmpty(g.fullName),
        targetId: 'field-fullName'
    });

    // 3. Vị trí
    tips.push({
        id: 'jobTitle',
        label: 'Vị trí ứng tuyển',
        hint: 'Ghi cụ thể vị trí (VD: Mobile Developer).',
        isDone: !isEmpty(g.jobTitle),
        targetId: 'field-jobTitle'
    });

    // 4. Liên hệ
    tips.push({
        id: 'contact',
        label: 'Email & Số điện thoại',
        hint: 'Để nhà tuyển dụng liên hệ với bạn.',
        isDone: !isEmpty(g.email) && !isEmpty(g.phone),
        targetId: 'field-email'
    });

    // 5. Mục tiêu
    tips.push({
        id: 'summary',
        label: 'Viết mục tiêu nghề nghiệp',
        hint: 'Nêu bật giá trị của bạn trong 2-3 câu.',
        isDone: !isEmpty(g.summary),
        targetId: 'section-summary'
    });

    // 6. Kinh nghiệm
    const expSection = data.sections.find(s => s.id === 'experience');
    tips.push({
        id: 'experience',
        label: 'Kinh nghiệm làm việc',
        hint: 'Thêm ít nhất 1-2 công việc gần nhất.',
        isDone: expSection && expSection.items?.length > 0,
        targetId: 'section-experience'
    });

    // 7. Kỹ năng
    const skillSection = data.sections.find(s => s.id === 'skills');
    tips.push({
        id: 'skills',
        label: 'Kỹ năng chuyên môn',
        hint: 'Liệt kê các công nghệ bạn thành thạo.',
        isDone: skillSection && skillSection.items?.length >= 3,
        targetId: 'section-skills'
    });

    // Trả về danh sách chưa hoàn thành trước, hoàn thành sau
    return tips.sort((a, b) => a.isDone - b.isDone);
});

const pendingTipsCount = computed(() => completionTips.value.filter(t => !t.isDone).length);

// State điều khiển Tabs
const activeEditorTab = ref('basic'); // 'basic', 'main', 'skills'
const basicTipsCount = computed(() => completionTips.value.filter(t => !t.isDone && ['avatar', 'fullName', 'jobTitle', 'contact', 'summary'].includes(t.id)).length);
const mainTipsCount = computed(() => completionTips.value.filter(t => !t.isDone && ['experience', 'education', 'project'].includes(t.id)).length);
const skillsTipsCount = computed(() => completionTips.value.filter(t => !t.isDone && t.id === 'skills').length);

const isSectionInActiveTab = (sectionId) => {
    if (sectionId === 'summary') return activeEditorTab.value === 'basic';
    if (['experience', 'education', 'project', 'activities'].includes(sectionId)) return activeEditorTab.value === 'main';
    return activeEditorTab.value === 'skills';
};

const scrollToField = (targetId) => {
    // Tự động chuyển Tab trước khi scroll
    if (targetId.startsWith('field-') || targetId === 'section-summary') {
        activeEditorTab.value = 'basic';
    } else if (['section-experience', 'section-education', 'section-project', 'section-activities'].includes(targetId)) {
        // Chuyển sang tab Nội dung chính nếu click vào các mục thuộc tab này (bao gồm cả Hoạt động)
        activeEditorTab.value = 'main';
    } else {
        activeEditorTab.value = 'skills';
    }

    // Đợi Vue render DOM của Tab mới rồi mới cuộn
    nextTick(() => {
        const el = document.getElementById(targetId);
        if (el) {
            // Tìm container cuộn (là div có class overflow-y-auto bên trong editor panel)
            const container = el.closest('.overflow-y-auto');
            if (container) {
                const topPos = el.offsetTop;
                container.scrollTo({
                    top: topPos - 100, // Cuộn đến vị trí cách top 100px để không bị sát mép
                    behavior: 'smooth'
                });
            } else {
                el.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
            
            // Hiệu ứng Highlight rõ rệt hơn (Nẩy lên, sáng viền, đổ bóng glow)
            el.classList.remove('bg-white');
            el.classList.add('ring-2', 'ring-blue-500', 'shadow-[0_0_30px_rgba(59,130,246,0.25)]', 'border-blue-500', 'bg-blue-50/80', 'scale-[1.015]', 'z-10');
            setTimeout(() => {
                el.classList.remove('ring-2', 'ring-blue-500', 'shadow-[0_0_30px_rgba(59,130,246,0.25)]', 'border-blue-500', 'bg-blue-50/80', 'scale-[1.015]', 'z-10');
                el.classList.add('bg-white');
            }, 1800);
        }
    });
};

const handlePreviewClick = (event) => {
    // Không can thiệp nếu đang trong trạng thái xem xuất file
    if (isExporting.value || showExportModal.value) return;

    const target = event.target;
    // Tìm phần tử gần nhất có nhãn data-section-id
    const sectionEl = target.closest('[data-section-id]');
    
    if (sectionEl) {
        const sectionId = sectionEl.getAttribute('data-section-id');
        
        // Chuyển hướng click đến đúng id của Form bên trái
        if (sectionId === 'contact' || sectionId === 'avatar' || sectionId === 'personal') {
            scrollToField('field-general');
        } else if (sectionId === 'summary') {
            scrollToField('section-summary');
        } else {
            scrollToField(`section-${sectionId}`);
        }
    }
};

const progressColorClass = computed(() => {
    const p = completionPercentage.value;
    if (p < 30) return 'bg-red-500';
    if (p < 70) return 'bg-amber-500';
    return 'bg-emerald-500';
});

// --- QUẢN LÝ DROPDOWN MÀU SẮC ---
const showThemeMenu = ref(false)
const showFontMenu = ref(false)
const presetColors = ['#2b5c8f', '#000000', '#ffffff', '#dc2626', '#eab308', '#16a34a', '#2563eb', '#6b7280', '#4b5563', '#ef4444']
const fontColors = ['inherit', '#000000', '#ffffff', '#2b5c8f', '#dc2626', '#eab308', '#16a34a', '#2563eb', '#6b7280', '#4b5563']

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
    if (command === 'foreColor' && value === 'inherit') {
        document.execCommand('styleWithCSS', false, true);
        document.execCommand('foreColor', false, 'inherit');
        document.execCommand('styleWithCSS', false, false);
    } else {
        document.execCommand(command, false, value);
    }
    if (typeof updateFormatState === 'function') setTimeout(updateFormatState, 10);
};

const isSaving = ref(false)
const resumeId = window.CURRENT_RESUME_ID || 0
const isProUser = ref(window.IS_PRO_USER === true || window.IS_PRO_USER === 'true')

// Khởi tạo Dữ liệu bám sát Models ResumeViewModel.cs
const resumeData = ref({
  overrideTemplate: '',
  theme: { primaryColor: '#2b5c8f' },
  general: {
    fullName: '', jobTitle: '', email: '', phone: '', address: '', birthDate: '', summary: '', website: '', avatarUrl: ''
  },
  sections: [
    { id: 'summary',        title: 'Mục tiêu Nghề nghiệp',   isVisible: true,  column: 'right', items: [] },
    { id: 'experience',     title: 'Kinh nghiệm Làm việc',   isVisible: true,  column: 'right', items: [] },
    { id: 'education',      title: 'Quá trình Học vấn',     isVisible: true,  column: 'right', items: [] },
    { id: 'skills',         title: 'Kỹ năng Chuyên môn',   isVisible: true,  column: 'left',  items: [] },
    { id: 'it_skills',      title: 'Tin học',              isVisible: false, column: 'left',  items: [] },
    { id: 'languages',      title: 'Ngoại ngữ',            isVisible: false, column: 'left',  items: [] },
    { id: 'activities',     title: 'Hoạt động',             isVisible: false, column: 'right', items: [] },
    { id: 'project',        title: 'Dự án Trọng điểm',     isVisible: false, column: 'right', items: [] },
    { id: 'certifications', title: 'Chứng chỉ / Bằng cấp',  isVisible: false, column: 'left',  items: [] },
    { id: 'awards',         title: 'Giải thưởng',           isVisible: false, column: 'left',  items: [] },
    { id: 'hobbies',        title: 'Sở thích',              isVisible: false, column: 'left',  items: [] },
    { id: 'additional',     title: 'Thông tin thêm',        isVisible: false, column: 'left',  items: [] },
    { id: 'references',     title: 'Người tham chiếu',      isVisible: false, column: 'left',  items: [] },
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
  else if(section.id === 'hobbies' || section.id === 'additional')    Object.assign(newItem, { name: '' });
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

  try {
    document.body.classList.add('is-exporting-pdf');
    // Override trực tiếp trên element gốc đang live (có đủ CSS đang apply)
    // Dùng setProperty với 'important' để chắc chắn thắng mọi CSS kể cả Tailwind !important
    cvEl.style.setProperty('overflow', 'visible', 'important');
    cvEl.style.setProperty('min-height', '0', 'important');

    // Override main, aside và các cột con bên trong template
    cvEl.querySelectorAll('main, aside, .left-sidebar, .right-main, .cv-sidebar, .cv-main-content, .left-column, .right-column').forEach(el => {
      el.style.setProperty('overflow', 'visible', 'important');
      el.style.setProperty('height', '100%', 'important');
      el.style.setProperty('max-height', 'none', 'important');
    });

    // Watermark cho Free user
    let wm = null;
    if (!isProUser.value) {
      wm = document.createElement('div');
      wm.textContent = '@cvbuilder';
      wm.style.cssText = 'position:absolute;bottom:10px;left:14px;font-size:9px;color:rgba(100,116,139,0.55);font-family:Inter,sans-serif;font-weight:500;z-index:10;pointer-events:none;';
      cvEl.appendChild(wm);
    }

    const origTitle = document.title;
    document.title = 'CV_' + (resumeData.value.general.fullName || 'Export');

    // window.print() — Builder.cshtml đã có @media print ẩn UI, hiện CV
    window.print();

    // Restore
    document.body.classList.remove('is-exporting-pdf');
    document.title = origTitle;
    cvEl.style.removeProperty('overflow');
    cvEl.style.removeProperty('min-height');
    cvEl.querySelectorAll('main, aside, .left-sidebar, .right-main, .cv-sidebar, .cv-main-content, .left-column, .right-column').forEach(el => {
      el.style.removeProperty('overflow');
      el.style.removeProperty('height');
      el.style.removeProperty('max-height');
    });
    if (wm) wm.remove();

    try {
      const id = window.CURRENT_RESUME_ID || 0;
      if (id) await fetch('/Resume/LogExport?resumeId=' + id, { method: 'POST' });
    } catch (e) { /* ignore */ }

  } catch (error) {
    console.error('Loi xuat PDF:', error);
    alert('Co loi xay ra. Vui long thu lai!');
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

        // Ghi nhận mẫu CV gốc ban đầu và gói tài khoản của người dùng
        initialTemplateName.value = targetName || 'Template';
        isProUser.value = data.isPro || false;

        if (data && data.jsonContent && data.jsonContent !== "{}") {
            const parsed = JSON.parse(data.jsonContent);
            
            // --- HỖ TRỢ OVERRIDE MẪU CHO TESTING (VUE TEMPLATES) ---
            if (parsed.overrideTemplate) {
                resumeData.value.overrideTemplate = parsed.overrideTemplate; // Lưu lại luôn để AutoSave không làm mất
                if (templateRegistry[parsed.overrideTemplate]) {
                    activeTemplate.value = templateRegistry[parsed.overrideTemplate];
                    console.log(`[Vue Test] Overriding template to: ${parsed.overrideTemplate}`);
                } else {
                    console.warn(`⚠️ Mẫu CV ${parsed.overrideTemplate} chưa có trong bundle JS. Vui lòng chạy lệnh 'npm run build' trong thư mục CVBuilderApp!`);
                }
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

                // --- MIGRATION FOR VUTUNGDUONG TEMPLATE ---
                if (targetName === 'VuTungDuong' || resumeData.value.overrideTemplate === 'VuTungDuong') {
                  const rightSections = ['summary', 'experience', 'education', 'activities'];
                  const leftSections = ['skills', 'certifications', 'awards', 'hobbies', 'references'];
                  const hiddenSections = ['it_skills', 'languages', 'project'];

                  resumeData.value.sections.forEach(s => {
                    if (rightSections.includes(s.id)) {
                      s.column = 'right';
                      if (s.id !== 'activities') s.isVisible = true;
                    } else if (leftSections.includes(s.id)) {
                      s.column = 'left';
                      s.isVisible = true;
                    } else if (hiddenSections.includes(s.id)) {
                      s.isVisible = false;
                    }
                  });

                  // Force màu mặc định (slot đầu tiên) cho VuTungDuong
                  if (!resumeData.value.theme) resumeData.value.theme = {};
                  resumeData.value.theme.primaryColor = '#2b5c8f';
                }

                // --- MIGRATION FOR NGUYENTHANHNHATNAM TEMPLATE ---
                if (targetName === 'NguyenThanhNhatNam' || resumeData.value.overrideTemplate === 'NguyenThanhNhatNam') {
                  const rightSections = ['education', 'project', 'activities', 'additional'];
                  const leftSections = ['skills', 'certifications', 'hobbies', 'summary'];
                  const hiddenSections = ['experience', 'it_skills', 'languages', 'awards', 'references'];

                  resumeData.value.sections.forEach(s => {
                    if (rightSections.includes(s.id)) {
                      if (!s.column) s.column = 'right';
                      s.isVisible = true;
                    } else if (leftSections.includes(s.id)) {
                      if (!s.column) s.column = 'left';
                      s.isVisible = true;
                    } else if (hiddenSections.includes(s.id)) {
                      s.isVisible = false;
                    }
                  });
                }
              }

              // Ghi lại danh sách các mục hiển thị ban đầu của CV này để tính % cố định
              if (initialVisibleSectionIds.value.length === 0) {
                  initialVisibleSectionIds.value = resumeData.value.sections
                      .filter(s => s.isVisible && s.id !== 'summary')
                      .map(s => s.id);
                  console.log('[CV Builder] Khởi tạo bộ khung tính % cho CV:', initialVisibleSectionIds.value);
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
          lastSavedTime.value = new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
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

const isScanningGrammar = ref(false);

const scanCVForGrammar = async () => {
    if (!isProUser.value) {
        alert("Tính năng AI Scan (Quét lỗi chính tả & ngữ pháp bằng AI) chỉ dành cho thành viên PRO. Vui lòng nâng cấp tài khoản để sử dụng!");
        return;
    }
    if (isScanningGrammar.value) return;
    
    // 0. Hàm dọn dẹp highlight cũ (Xóa các thẻ span highlight nhưng giữ lại chữ)
    const clearHighlights = (html) => {
        if (!html) return "";
        return html.replace(/<span class="grammar-error-highlight"[^>]*>(.*?)<\/span>/g, '$1');
    };

    // Dọn dẹp toàn bộ dữ liệu trước khi quét
    resumeData.value.general.fullName = clearHighlights(resumeData.value.general.fullName);
    resumeData.value.general.jobTitle = clearHighlights(resumeData.value.general.jobTitle);
    resumeData.value.general.summary = clearHighlights(resumeData.value.general.summary);
    resumeData.value.sections.forEach(s => {
        if (s.items) {
            s.items.forEach(item => {
                if (item.desc) item.desc = clearHighlights(item.desc);
            });
        }
    });

    // 1. Thu thập toàn bộ nội dung cần quét
    let combinedText = "";
    const mappings = [];
    
    const addSection = (id, text, sectionId = null, itemIdx = null) => {
        if (!text) return;
        const cleanText = text.replace(/<[^>]*>/g, '').replace(/&nbsp;/g, ' ').trim();
        if (cleanText.length < 2) return;
        
        const marker = `[[${id}]]`;
        combinedText += `${marker} ${cleanText} \n`;
        mappings.push({ id, marker, original: text, sectionId, itemIdx });
    };

    addSection('fullName', resumeData.value.general.fullName);
    addSection('jobTitle', resumeData.value.general.jobTitle);
    addSection('summary', resumeData.value.general.summary);
    
    resumeData.value.sections.forEach(section => {
        if (section.isVisible && section.items && ['experience', 'project', 'activities'].includes(section.id)) {
            section.items.forEach((item, idx) => {
                addSection(`${section.id}-${idx}`, item.desc, section.id, idx);
            });
        }
    });

    if (combinedText.length === 0) {
        alert("Không có nội dung nào để quét!");
        return;
    }

    isScanningGrammar.value = true;
    
    try {
        const response = await callAIService('check_grammar', combinedText);
        if (response) {
            // Kiểm tra xem phản hồi có phải là thông báo lỗi của AI không
            if (response.includes("quota") || response.includes("limit") || response.includes("exhausted")) {
                alert("AI đang bị quá tải. Vui lòng đợi khoảng 1 phút rồi thử lại.");
                return;
            }

            try {
                const jsonString = response.replace(/```json/g, '').replace(/```/g, '').trim();
                const errors = JSON.parse(jsonString);
                
                if (Array.isArray(errors) && errors.length > 0) {
                    let totalErrors = 0;

                    mappings.forEach(map => {
                        let highlightedText = map.original;
                        let fieldHasError = false;

                        errors.forEach(err => {
                            try {
                                const escapedError = err.error.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
                                const regex = new RegExp(`(${escapedError})`, 'gi');
                                
                                if (regex.test(highlightedText)) {
                                    highlightedText = highlightedText.replace(regex, `<span class="grammar-error-highlight" title="Click đúp để sửa thành: ${err.fix} (${err.reason})" data-fix="${err.fix}" data-field="${map.id}">$1</span>`);
                                    fieldHasError = true;
                                    totalErrors++;
                                }
                            } catch(e) {}
                        });

                        if (fieldHasError) {
                            if (map.id === 'fullName') resumeData.value.general.fullName = highlightedText;
                            else if (map.id === 'jobTitle') resumeData.value.general.jobTitle = highlightedText;
                            else if (map.id === 'summary') resumeData.value.general.summary = highlightedText;
                            else {
                                const section = resumeData.value.sections.find(s => s.id === map.sectionId);
                                if (section && section.items[map.itemIdx]) {
                                    section.items[map.itemIdx].desc = highlightedText;
                                }
                            }
                        }
                    });

                    if (totalErrors > 0) {
                        alert(`AI đã phát hiện và highlight ${totalErrors} chỗ cần sửa. Hãy di chuột vào vùng gạch đỏ để xem gợi ý!`);
                    } else {
                        alert("Không phát hiện lỗi chính tả đáng kể nào.");
                    }
                } else {
                    alert("Chúc mừng! CV của bạn không có lỗi chính tả.");
                }
            } catch (e) {
                console.error("Lỗi xử lý phản hồi AI:", e, response);
                alert("AI phản hồi không đúng định dạng hoặc nội dung quá phức tạp. Hãy thử lại với đoạn văn ngắn hơn.");
            }
        }
    } catch (err) {
        console.error("Lỗi khi quét AI:", err);
        alert("Hệ thống AI đang bận hoặc gặp lỗi kết nối.");
    } finally {
        isScanningGrammar.value = false;
    }
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

const targetCompany = ref('');
const coverLetterJD = ref('');
const coverLetterResult = ref('');

// --- TRỢ LÝ SOẠN THẢO AI TÙY BIẾN ---
const isAIAssistantExpanded = ref(true);
const aiAssistantPrompt = ref('');
const aiAssistantTarget = ref('');
const isAIAssistantLoading = ref(false);

const quickPrompts = [
  { label: '🌟 Viết chuyên nghiệp hơn', prompt: 'Hãy viết lại phần mô tả này sao cho chuyên nghiệp, thu hút nhà tuyển dụng và sử dụng văn phong công sở lịch sự.' },
  { label: '⚡ Tối ưu chuẩn STAR', prompt: 'Hãy viết lại phần này theo chuẩn STAR (Situation, Task, Action, Result), mô tả chi tiết hành động và kết quả đạt được có kèm số liệu.' },
  { label: '🌐 Đổi ngôn ngữ', prompt: 'Hãy dịch phần này sang tiếng Anh (hoặc ghi rõ ngôn ngữ bạn muốn dịch, VD: "tiếng Nhật").' },
  { label: '📝 Sửa lỗi chính tả & diễn đạt', prompt: 'Hãy sửa lỗi chính tả, lỗi dùng từ và cải thiện diễn đạt cho câu từ trôi chảy hơn.' }
];

const triggerAIAssistant = async () => {
    if (!aiAssistantPrompt.value.trim() || !aiAssistantTarget.value) return;

    let targetField = aiAssistantTarget.value;
    let contextContent = '';
    
    // Xác định nội dung cũ làm ngữ cảnh (context) gửi kèm AI
    if (targetField === 'summary') {
        contextContent = (resumeData.value.general.summary || '').replace(/<[^>]*>/g, '').trim();
    } else {
        const [sectionId, itemId] = targetField.split('|');
        const section = resumeData.value.sections.find(s => s.id === sectionId);
        if (section && section.items) {
            const item = section.items.find(i => i._refId === itemId);
            if (item) {
                contextContent = (item.desc || '').replace(/<[^>]*>/g, '').trim();
            }
        }
    }

    isAIAssistantLoading.value = true;

    try {
        // Gọi API xử lý prompt tùy biến
        const result = await callAIService('custom_prompt', aiAssistantPrompt.value, contextContent);
        if (result) {
            // Cập nhật kết quả ngược lại vào ô thông tin CV tương ứng
            if (targetField === 'summary') {
                resumeData.value.general.summary = result;
                scrollToField('section-summary');
            } else {
                const [sectionId, itemId] = targetField.split('|');
                const section = resumeData.value.sections.find(s => s.id === sectionId);
                if (section && section.items) {
                    const item = section.items.find(i => i._refId === itemId);
                    if (item) {
                        item.desc = result;
                        scrollToField(`section-${sectionId}`);
                    }
                }
            }
        }
    } catch (e) {
        console.error('AI Assistant Error:', e);
        alert('Có lỗi xảy ra khi kết nối với trợ lý AI.');
    } finally {
        isAIAssistantLoading.value = false;
    }
};

// --- TRỢ LÝ AI MINI CHO TỪNG MỤC NHẬP LIỆU ---
const expandedAIs = reactive({});
const summaryAIFunc = ref('custom');
const summaryAIPrompt = ref('');
const aiFuncByItem = reactive({});
const aiPromptByItem = reactive({});

const runSummaryAI = async () => {
    const jobTitle = (resumeData.value.general.jobTitle || '').trim();
    if (!jobTitle) {
        alert("Vui lòng nhập Vị trí ứng tuyển của bạn tại mục Thông tin cá nhân trước khi sử dụng Trợ lý AI!");
        return;
    }

    const func = summaryAIFunc.value;
    let userPrompt = summaryAIPrompt.value.trim();
    let currentSummary = (resumeData.value.general.summary || '').replace(/<[^>]*>/g, '').trim();

    // Chuẩn hóa prompt lười biếng của người dùng
    const lowerPrompt = userPrompt.toLowerCase();
    const lazyKeywords = ['viết', 'viết hộ', 'viết giùm', 'viết mới', 'viết lại', 'viết mục tiêu', 'viết cv', 'tạo mục tiêu', 'sinh mục tiêu', 'summary', 'viet', 'viet ho', 'viet gium', 'viet moi', 'viet lai', 'viet lai muc tieu'];
    const isLazy = lazyKeywords.includes(lowerPrompt) || !userPrompt;

    isAIProcessing.value['summary'] = true;
    
    let result = null;
    try {
        if (func === 'custom') {
            let finalPrompt = userPrompt;
            if (isLazy) {
                finalPrompt = `Viết duy nhất một đoạn văn ngắn gọn (2-3 câu, tối đa 60 từ, TUYỆT ĐỐI KHÔNG sử dụng gạch đầu dòng hay danh sách liệt kê) trình bày mục tiêu nghề nghiệp cá nhân để đưa vào CV cho vị trí: ${jobTitle}. Nội dung phải hướng tới việc đóng góp giá trị cho công ty và phát triển bản thân, viết ở ngôi thứ nhất.`;
            }
            result = await callAIService('custom_prompt_summary', finalPrompt, currentSummary || jobTitle);
        } else if (func === 'optimize') {
            if (userPrompt && !isLazy) {
                // Kết hợp tối ưu hóa và prompt bổ sung
                const combinedPrompt = `Hãy tối ưu hóa mục tiêu nghề nghiệp này dựa trên yêu cầu bổ sung sau: "${userPrompt}"`;
                result = await callAIService('custom_prompt_summary', combinedPrompt, currentSummary || `Tôi đang ứng tuyển vị trí ${jobTitle}`);
            } else {
                result = await callAIService('summary', currentSummary || `Tôi đang ứng tuyển vị trí ${jobTitle}`, jobTitle);
            }
        } else if (func === 'english') {
            result = await callAIService('custom_prompt_lang_summary', userPrompt || '', currentSummary || jobTitle);
        } else if (func === 'grammar') {
            if (userPrompt && !isLazy) {
                // Kết hợp sửa chính tả và prompt bổ sung
                const combinedPrompt = `Hãy sửa lỗi chính tả, ngữ pháp và tinh chỉnh lại mục tiêu nghề nghiệp này dựa trên yêu cầu bổ sung sau: "${userPrompt}"`;
                result = await callAIService('custom_prompt_summary', combinedPrompt, currentSummary);
            } else {
                result = await callAIService('custom_prompt_summary', 'Hãy sửa toàn bộ lỗi chính tả và ngữ pháp trong văn bản này, viết lại thật trơn tru.', currentSummary);
            }
        }

        if (result) {
            resumeData.value.general.summary = result;
            expandedAIs['summary'] = false; // Tự đóng khi thành công
        }
    } catch(e) {
        console.error(e);
    } finally {
        isAIProcessing.value['summary'] = false;
    }
};

const runLocalAI = async (item, sectionId) => {
    const jobTitle = (resumeData.value.general.jobTitle || '').trim();
    if (!jobTitle) {
        alert("Vui lòng nhập Vị trí ứng tuyển của bạn tại mục Thông tin cá nhân trước khi sử dụng Trợ lý AI!");
        return;
    }

    // 1. Validation bắt buộc nhập đầy đủ các trường tương ứng trong giao diện Vue
    if (sectionId === 'experience') {
        const company = (item.company || '').trim();
        const role = (item.role || '').trim();
        const time = (item.time || '').trim();
        if (!company || !role || !time) {
            alert("Vui lòng điền đầy đủ các thông tin: Tên công ty, Vị trí làm việc và Thời gian trước khi sử dụng Trợ lý AI!");
            return;
        }
    } else if (sectionId === 'project') {
        const name = (item.name || '').trim();
        const role = (item.role || '').trim();
        const time = (item.time || '').trim();
        if (!name || !role || !time) {
            alert("Vui lòng điền đầy đủ các thông tin: Tên dự án, Vai trò và Thời gian trước khi sử dụng Trợ lý AI!");
            return;
        }
    } else if (sectionId === 'activities') {
        const name = (item.name || '').trim();
        const time = (item.time || '').trim();
        if (!name || !time) {
            alert("Vui lòng điền đầy đủ các thông tin: Tên hoạt động/tổ chức và Thời gian trước khi sử dụng Trợ lý AI!");
            return;
        }
    }

    const itemId = item._refId;
    const func = aiFuncByItem[itemId] || 'custom';
    const userPrompt = (aiPromptByItem[itemId] || '').trim();
    let currentDesc = (item.desc || '').replace(/<[^>]*>/g, '').trim();

    // Khởi tạo reactive state cho item này nếu chưa có
    if (!aiFuncByItem[itemId]) aiFuncByItem[itemId] = 'custom';

    // Tạo ngữ cảnh đầy đủ sử dụng trường 'time' thay vì 'duration'
    const itemContext = sectionId === 'experience'
        ? `Vị trí: ${item.role}, tại công ty: ${item.company}, thời gian: ${item.time}`
        : (sectionId === 'project'
            ? `Dự án: ${item.name}, vai trò: ${item.role}, thời gian: ${item.time}`
            : `Hoạt động: ${item.name}, thời gian: ${item.time}`);

    // Chuẩn hóa prompt lười biếng
    const lowerPrompt = userPrompt.toLowerCase();
    const lazyKeywords = ['viết', 'viết hộ', 'viết giùm', 'viết mới', 'viết lại', 'viết mô tả', 'summary', 'viet', 'viet ho', 'viet gium', 'viet moi', 'viet lai'];
    const isLazy = lazyKeywords.includes(lowerPrompt) || !userPrompt;

    isAIProcessing.value[itemId] = true;

    let result = null;
    try {
        if (func === 'custom') {
            let finalPrompt = userPrompt;
            if (isLazy) {
                if (sectionId === 'experience') {
                    finalPrompt = `Viết mô tả công việc chi tiết cho vị trí ${item.role} tại công ty ${item.company} trong thời gian ${item.time}`;
                } else if (sectionId === 'project') {
                    finalPrompt = `Viết mô tả dự án chi tiết cho dự án ${item.name} với vai trò ${item.role} trong thời gian ${item.time}`;
                } else {
                    finalPrompt = `Viết mô tả hoạt động chi tiết cho hoạt động ${item.name} trong thời gian ${item.time}`;
                }
            }
            result = await callAIService('custom_prompt', finalPrompt, currentDesc || itemContext);
        } else if (func === 'optimize') {
            if (userPrompt && !isLazy) {
                // Kết hợp tối ưu hóa và prompt bổ sung
                const combinedPrompt = `Hãy tối ưu hóa mô tả công việc này dựa trên yêu cầu bổ sung sau: "${userPrompt}"`;
                result = await callAIService('custom_prompt', combinedPrompt, currentDesc || itemContext);
            } else {
                const aiType = sectionId === 'experience' ? 'optimize' : (sectionId === 'project' ? 'project' : 'activity');
                result = await callAIService(aiType, currentDesc || `Mô tả công việc cho ${itemContext}`, itemContext);
            }
        } else if (func === 'english') {
            result = await callAIService('custom_prompt_lang', userPrompt || '', currentDesc || itemContext);
        } else if (func === 'grammar') {
            if (userPrompt && !isLazy) {
                // Kết hợp sửa chính tả và prompt bổ sung
                const combinedPrompt = `Hãy sửa lỗi chính tả, ngữ pháp và tinh chỉnh lại mô tả công việc này dựa trên yêu cầu bổ sung sau: "${userPrompt}"`;
                result = await callAIService('custom_prompt', combinedPrompt, currentDesc || itemContext);
            } else {
                result = await callAIService('custom_prompt', 'Hãy sửa toàn bộ lỗi chính tả, lỗi dùng từ và cải thiện diễn đạt cho đoạn văn này.', currentDesc || itemContext);
            }
        }

        if (result) {
            item.desc = result;
            expandedAIs[itemId] = false; // Tự đóng khi thành công
        }
    } catch(e) {
        console.error(e);
    } finally {
        isAIProcessing.value[itemId] = false;
    }
};



const generateAICoverLetter = async () => {
    if (!targetCompany.value) {
        alert("Vui lòng nhập Công ty và Vị trí bạn muốn ứng tuyển!");
        return;
    }

    const fullName = resumeData.value.general.fullName || 'Ứng viên';
    const jobTitle = resumeData.value.general.jobTitle || 'Nhân viên';
    const summary = (resumeData.value.general.summary || '').replace(/<[^>]*>/g, '').trim();
    
    // Thu thập kinh nghiệm
    const expSection = resumeData.value.sections.find(s => s.id === 'experience');
    let experiences = "";
    if (expSection && expSection.items) {
        expSection.items.forEach(item => {
            if (item.company || item.role) {
                experiences += `${item.company || ''} (${item.role || ''}); `;
            }
        });
    }

    // Thu thập Kỹ năng
    let skills = "";
    ['skills', 'it_skills', 'languages'].forEach(secId => {
        const sec = resumeData.value.sections.find(s => s.id === secId);
        if (sec && sec.items) {
            sec.items.forEach(item => {
                if (item.name) skills += `${item.name}, `;
            });
        }
    });

    // Thu thập Học vấn
    const eduSection = resumeData.value.sections.find(s => s.id === 'education');
    let educations = "";
    if (eduSection && eduSection.items) {
        eduSection.items.forEach(item => {
            if (item.school || item.major) {
                educations += `${item.school || ''} - ${item.major || ''}; `;
            }
        });
    }

    const fullInfo = `Ứng viên: ${fullName}. Vị trí: ${jobTitle}. Mục tiêu: ${summary}. Kỹ năng: ${skills}. Kinh nghiệm: ${experiences}. Học vấn: ${educations}.`;
    
    // Khớp Context JD và Company
    let targetInfo = targetCompany.value;
    if (coverLetterJD.value.trim()) {
        targetInfo += `\n\nMô tả công việc (JD):\n${coverLetterJD.value.trim()}`;
    }

    isAIProcessing.value['cover_letter'] = true;
    const result = await callAIService('cover_letter', fullInfo, targetInfo);
    if (result) {
        coverLetterResult.value = result;
    }
    isAIProcessing.value['cover_letter'] = false;
};

const copyCoverLetter = () => {
    if (!coverLetterResult.value) return;
    
    navigator.clipboard.writeText(coverLetterResult.value).then(() => {
        alert("Đã sao chép Thư xin việc vào bộ nhớ tạm!");
    }).catch(err => {
        console.error('Không thể sao chép:', err);
        alert("Lỗi khi sao chép. Vui lòng thử lại!");
    });
};

const downloadCoverLetter = () => {
    if (!coverLetterResult.value) return;
    
    const blob = new Blob([coverLetterResult.value], { type: 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    
    // Tạo tên file an toàn
    let companyName = targetCompany.value.split('-')[0].trim().replace(/\s+/g, '_');
    if (!companyName) companyName = "CVBuilder";
    
    link.download = `Cover_Letter_${companyName}.txt`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

// --- TÍNH NĂNG JOB MATCHER ---
const showJobMatcherModal = ref(false);
const jobDescription = ref('');
const jobMatchResult = ref(null);
const isMatchingJob = ref(false);

const matchScoreColor = computed(() => {
    if (!jobMatchResult.value) return '#94a3b8';
    const score = jobMatchResult.value.matchScore;
    if (score >= 80) return '#10b981'; // Emerald
    if (score >= 60) return '#3b82f6'; // Blue
    if (score >= 40) return '#f59e0b'; // Amber
    return '#ef4444'; // Red
});

const matchScoreLabel = computed(() => {
    if (!jobMatchResult.value) return '';
    const score = jobMatchResult.value.matchScore;
    if (score >= 80) return '🏆 Rất phù hợp! Hãy ứng tuyển ngay!';
    if (score >= 60) return '👍 Khá phù hợp. Cải thiện thêm để tăng cơ hội.';
    if (score >= 40) return '⚠️ Tạm được. Cần bổ sung nhiều kỹ năng.';
    return '❌ Chưa phù hợp lắm. Xem gợi ý bên dưới.';
});

const analyzeJobMatch = async () => {
    if (!jobDescription.value.trim()) {
        alert('Vui lòng dán nội dung Job Description!');
        return;
    }

    // Thu thập toàn bộ thông tin CV thành text
    const g = resumeData.value.general;
    let cvText = `Họ tên: ${(g.fullName || '').replace(/<[^>]*>/g, '')}\n`;
    cvText += `Vị trí: ${(g.jobTitle || '').replace(/<[^>]*>/g, '')}\n`;
    cvText += `Mục tiêu: ${(g.summary || '').replace(/<[^>]*>/g, '')}\n\n`;

    resumeData.value.sections.forEach(section => {
        if (!section.isVisible || !section.items) return;
        cvText += `--- ${section.title} ---\n`;
        section.items.forEach(item => {
            if (section.id === 'experience') {
                cvText += `${item.company || ''} | ${item.role || ''} | ${item.time || ''}\n`;
                cvText += `${(item.desc || '').replace(/<[^>]*>/g, '')}\n`;
            } else if (section.id === 'education') {
                cvText += `${item.school || ''} | ${item.major || ''} | ${item.year || ''} | ${item.gradType || ''}\n`;
            } else if (section.id === 'project') {
                cvText += `${item.name || ''} | ${item.role || ''} | ${item.time || ''}\n`;
                cvText += `${(item.desc || '').replace(/<[^>]*>/g, '')}\n`;
            } else if (section.id === 'skills' || section.id === 'it_skills' || section.id === 'languages') {
                cvText += `${item.name || ''} (${item.level || ''})\n`;
            } else if (section.id === 'certifications' || section.id === 'awards') {
                cvText += `${item.name || ''} - ${item.year || ''}\n`;
            } else if (section.id === 'activities') {
                cvText += `${item.name || ''} | ${item.time || ''}\n`;
                cvText += `${(item.desc || '').replace(/<[^>]*>/g, '')}\n`;
            } else if (section.id === 'hobbies') {
                cvText += `${item.name || ''}\n`;
            } else if (section.id === 'references') {
                cvText += `${(item.info || '').replace(/<[^>]*>/g, '')}\n`;
            }
        });
        cvText += '\n';
    });

    isMatchingJob.value = true;
    jobMatchResult.value = null;

    try {
        const response = await callAIService('job_match', cvText, jobDescription.value);
        if (response) {
            try {
                const jsonString = response.replace(/```json/g, '').replace(/```/g, '').trim();
                const parsed = JSON.parse(jsonString);
                jobMatchResult.value = {
                    matchScore: parseInt(parsed.matchScore) || 0,
                    matchedSkills: parsed.matchedSkills || [],
                    missingSkills: parsed.missingSkills || [],
                    suggestions: parsed.suggestions || [],
                    summary: parsed.summary || ''
                };
            } catch (e) {
                console.error('Lỗi parse JSON job match:', e, response);
                alert('AI phản hồi không đúng định dạng. Vui lòng thử lại!');
            }
        }
    } catch (err) {
        console.error('Lỗi job match:', err);
        alert('Hệ thống AI đang bận. Vui lòng thử lại sau!');
    } finally {
        isMatchingJob.value = false;
    }
};

const handleGrammarFix = (e) => {
    const target = e.target.closest('.grammar-error-highlight');
    if (target) {
        const fix = target.getAttribute('data-fix');
        const fieldId = target.getAttribute('data-field');
        
        if (fix && fieldId) {
            // 1. Cập nhật dữ liệu trong resumeData
            if (fieldId === 'fullName') {
                resumeData.value.general.fullName = resumeData.value.general.fullName.replace(target.outerHTML, fix);
            } else if (fieldId === 'jobTitle') {
                resumeData.value.general.jobTitle = resumeData.value.general.jobTitle.replace(target.outerHTML, fix);
            } else if (fieldId === 'summary') {
                resumeData.value.general.summary = resumeData.value.general.summary.replace(target.outerHTML, fix);
            } else {
                // Xử lý các section (experience-0, project-1, ...)
                const [sectionId, itemIdx] = fieldId.split('-');
                const section = resumeData.value.sections.find(s => s.id === sectionId);
                if (section && section.items[itemIdx]) {
                    section.items[itemIdx].desc = section.items[itemIdx].desc.replace(target.outerHTML, fix);
                }
            }
            
            // 2. Nếu đang ở trong editor (ô input/contenteditable), cập nhật UI ngay
            const editor = target.closest('[contenteditable="true"]');
            if (editor) {
                target.outerHTML = fix;
                editor.dispatchEvent(new Event('input', { bubbles: true }));
            }
            
            e.preventDefault();
            e.stopPropagation();
        }
    }
};

onMounted(() => {
    loadData();
    fetchTemplates(); // Lấy danh sách mẫu CV khi tải trang
    document.addEventListener('selectionchange', updateFormatState);
    document.addEventListener('mouseup', updateFormatState);
    document.addEventListener('keyup', updateFormatState);
    document.addEventListener('mousedown', handleOutsideClick);
    document.addEventListener('dblclick', handleGrammarFix);
});

onUnmounted(() => {
    document.removeEventListener('selectionchange', updateFormatState);
    document.removeEventListener('mouseup', updateFormatState);
    document.removeEventListener('keyup', updateFormatState);
    document.removeEventListener('mousedown', handleOutsideClick);
    document.removeEventListener('dblclick', handleGrammarFix);
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
  .cv-builder-container, #app { 
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

    /* Highlight lỗi chính tả chuẩn MS Word */
    .grammar-error-highlight {
        text-decoration: underline wavy #ef4444 !important;
        text-decoration-thickness: 2px !important;
        text-underline-offset: 4px !important;
        background-color: rgba(239, 68, 68, 0.1) !important;
        cursor: pointer;
        display: inline !important;
    }
    
    .grammar-error-highlight:hover {
        background-color: rgba(239, 68, 68, 0.2) !important;
    }

    /* WATERMARK cho tài khoản Free */
    .cv-watermark-free {
        position: absolute;
        bottom: 10px;
        left: 14px;
        font-size: 9px;
        color: rgba(100, 116, 139, 0.55);
        font-family: 'Inter', 'Segoe UI', sans-serif;
        letter-spacing: 0.3px;
        font-weight: 500;
        z-index: 10;
        pointer-events: none;
        user-select: none;
    }

    /* ==========================================
       STYLING CHO PANEL GỢI Ý HOÀN THIỆN
       ========================================== */
    .completion-tips-panel {
        background-color: #ffffff !important;
        border-color: #e2e8f0 !important;
    }

    .completion-tips-body {
        background-color: #ffffff !important;
    }

    .completion-tip-item {
        border-color: transparent !important;
    }

    .completion-tip-item:hover {
        background-color: #f8fafc !important;
        border-color: #e2e8f0 !important;
    }

    .completion-tip-label {
        color: #334155 !important; /* slate-700 */
    }

    .completion-tip-item:hover .completion-tip-label {
        color: #1d4ed8 !important; /* blue-700 */
    }

    /* Icon chưa hoàn thành */
    .completion-tip-icon-unread {
        background-color: #ffffff !important;
        border-color: #e2e8f0 !important;
        color: #94a3b8 !important;
    }

    .completion-tip-item:hover .completion-tip-icon-unread {
        background-color: #eff6ff !important;
        border-color: #bfdbfe !important;
        color: #2563eb !important;
    }

    /* --- ĐỒNG BỘ DARK MODE CHO PANEL GỢI Ý --- */
    [data-bs-theme="dark"] .completion-tips-panel,
    .dark .completion-tips-panel {
        background-color: #0f172a !important; /* Slate-900 */
        border-color: #1e293b !important; /* Slate-800 */
        box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.6) !important;
    }

    [data-bs-theme="dark"] .completion-tips-body,
    .dark .completion-tips-body {
        background-color: #1e293b !important; /* Slate-800 */
    }

    [data-bs-theme="dark"] .completion-tip-item:hover,
    .dark .completion-tip-item:hover {
        background-color: #334155 !important; /* Slate-700 */
        border-color: #475569 !important; /* Slate-600 */
    }

    [data-bs-theme="dark"] .completion-tip-label,
    .dark .completion-tip-label {
        color: #cbd5e1 !important; /* Slate-300 */
    }

    [data-bs-theme="dark"] .completion-tip-item:hover .completion-tip-label,
    .dark .completion-tip-item:hover .completion-tip-label {
        color: #60a5fa !important; /* Blue-400 */
    }

    [data-bs-theme="dark"] .completion-tip-icon-unread,
    .dark .completion-tip-icon-unread {
        background-color: #0f172a !important;
        border-color: #334155 !important;
        color: #94a3b8 !important;
    }

    [data-bs-theme="dark"] .completion-tip-item:hover .completion-tip-icon-unread,
    .dark .completion-tip-item:hover .completion-tip-icon-unread {
        background-color: rgba(59, 130, 246, 0.2) !important;
        border-color: #3b82f6 !important;
        color: #60a5fa !important;
    }

    /* Trạng thái đã hoàn thành (Done) trong dark mode */
    [data-bs-theme="dark"] .bg-emerald-50,
    .dark .bg-emerald-50 {
        background-color: rgba(16, 185, 129, 0.15) !important;
    }
    [data-bs-theme="dark"] .border-emerald-100,
    .dark .border-emerald-100 {
        border-color: rgba(16, 185, 129, 0.3) !important;
    }
    [data-bs-theme="dark"] .text-emerald-600,
    .dark .text-emerald-600 {
        color: #34d399 !important;
    }

    /* ==========================================
       STYLING CHO EDITOR TABS NAVIGATION (DARK MODE & LIGHT MODE)
       ========================================== */
    .cv-editor-tabs-container {
        background-color: rgba(241, 245, 249, 0.8) !important; /* bg-slate-100/80 */
        border-color: rgba(226, 232, 240, 0.6) !important; /* border-slate-200/60 */
    }

    .cv-editor-tab-btn {
        color: #64748b !important; /* text-slate-500 */
        border-radius: 8px !important; /* Bo tròn các tab button */
    }

    .cv-editor-tab-btn:hover {
        color: #1e293b !important; /* text-slate-800 */
        background-color: rgba(226, 232, 240, 0.5) !important; /* hover:bg-slate-200/50 */
        border-radius: 8px !important; /* Bo tròn các tab button khi hover */
    }

    .cv-editor-tab-btn.cv-active {
        background-color: #ffffff !important;
        color: #2563eb !important; /* text-blue-600 */
        font-weight: 700 !important;
        box-shadow: 0 2px 10px rgba(37, 99, 235, 0.1) !important;
        border: 1px solid #e2e8f0 !important; /* ring-slate-200 */
        border-radius: 8px !important; /* Bo tròn tab active */
    }

    /* --- ĐỒNG BỘ DARK MODE --- */
    [data-bs-theme="dark"] .cv-editor-tabs-container,
    .dark .cv-editor-tabs-container {
        background-color: rgba(15, 23, 42, 0.8) !important; /* Slate-900 với opacity */
        border-color: rgba(51, 65, 85, 0.5) !important; /* Slate-700 với opacity */
    }

    [data-bs-theme="dark"] .cv-editor-tab-btn,
    .dark .cv-editor-tab-btn {
        color: #94a3b8 !important; /* text-slate-400 */
        border-radius: 8px !important; /* Bo tròn tab dark mode */
    }

    [data-bs-theme="dark"] .cv-editor-tab-btn:hover,
    .dark .cv-editor-tab-btn:hover {
        color: #f1f5f9 !important; /* text-slate-100 */
        background-color: rgba(51, 65, 85, 0.5) !important; /* Slate-700/50 */
        border-radius: 8px !important; /* Bo tròn tab dark mode khi hover */
    }

    [data-bs-theme="dark"] .cv-editor-tab-btn.cv-active,
    .dark .cv-editor-tab-btn.cv-active {
        background-color: #1e293b !important; /* Slate-800 làm nền tab active */
        color: #60a5fa !important; /* Blue-400 */
        font-weight: 700 !important;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.4) !important;
        border: 1px solid #334155 !important; /* Slate-700 làm viền */
        border-radius: 8px !important; /* Bo tròn tab active dark mode */
    }

    /* --- STYLING NÚT AI BO TRÒN --- */
    .cv-ai-btn {
        border-radius: 9999px !important; /* Bo tròn hoàn toàn */
    }
</style>
