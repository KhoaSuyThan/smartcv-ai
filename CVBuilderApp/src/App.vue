<template>
  <div class="w-full bg-slate-50 flex font-sans text-slate-800 cv-builder-container relative" style="height: 100%; overflow: hidden;">
    <!-- CỘT TRÁI: EDITOR PANEL (STICKY) -->
    <div class="cv-builder-editor-panel w-[700px] bg-white border-r border-slate-200 shadow-[0_0_20px_rgba(0,0,0,0.05)] z-20 flex flex-col shrink-0 overflow-hidden" style="height: 100%;">
      <!-- HEADER -->
      <div class="p-6 border-b border-slate-100 bg-slate-900 text-white shrink-0 relative overflow-hidden">
        <div class="absolute top-0 right-0 -mr-8 -mt-8 w-32 h-32 bg-blue-500 rounded-full opacity-20 blur-2xl"></div>
        <h1 class="text-xl font-black uppercase tracking-widest text-transparent bg-clip-text bg-gradient-to-r from-blue-400 to-emerald-400">CV Builder Pro <span class="text-[10px] bg-red-500 text-white px-1 rounded ml-2">V10</span></h1>
        <p class="text-slate-400 text-xs mt-1.5 font-medium border-l-2 border-red-500 pl-2">Đồ án Cơ sở Công nghệ phần mềm(Cập nhật 4:54 PM)</p>
        <div class="mt-4 flex flex-col gap-2">
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
          <div class="flex items-center gap-4 mb-6 bg-slate-50 p-3 rounded-xl border border-dashed border-slate-200">
            <div class="relative w-16 h-16 rounded-full bg-slate-200 overflow-hidden border-2 border-white shadow-sm shrink-0">
                <img v-if="resumeData.general.avatarUrl" :src="resumeData.general.avatarUrl" class="w-full h-full object-cover" />
                <div v-else class="w-full h-full flex items-center justify-center text-slate-400">
                    <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
                </div>
            </div>
            <div class="flex-1">
                <label class="block text-[10px] font-bold text-slate-500 uppercase mb-1">Ảnh đại diện</label>
                <input type="file" @change="onAvatarChange" accept="image/*" class="block w-full text-[11px] text-slate-500 file:mr-2 file:py-1 file:px-2 file:rounded-full file:border-0 file:text-[11px] file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100 cursor-pointer" />
            </div>
          </div>

          <div class="flex items-center justify-between bg-slate-50/50 p-3 rounded-lg border border-slate-100 mb-4">
            <label class="text-xs font-semibold text-slate-600">Đổi màu Chủ đạo CV</label>
            <div class="relative w-8 h-8 rounded-full border border-slate-200 shadow-sm overflow-hidden cursor-pointer hover:ring-2 hover:ring-blue-400 transition-all">
                <input type="color" v-model="resumeData.theme.primaryColor" class="w-[200%] h-[200%] -top-2 -left-2 absolute cursor-pointer border-0 p-0" />
            </div>
          </div>
          
          <div class="space-y-4">
            <div class="grid grid-cols-1 gap-3">
                <input v-model="resumeData.general.fullName" type="text" class="w-full text-sm py-2.5 px-3 border border-slate-200 bg-white rounded-lg focus:ring-2 focus:ring-blue-500 outline-none transition-all font-medium placeholder-slate-400 shadow-sm" placeholder="Họ và Tên" />
                <input v-model="resumeData.general.jobTitle" type="text" class="w-full text-sm py-2.5 px-3 border border-slate-200 bg-white rounded-lg focus:ring-2 focus:ring-blue-500 outline-none transition-all placeholder-slate-400 shadow-sm" placeholder="Vị trí ứng tuyển" />
            </div>
             <div class="grid grid-cols-2 gap-3">
                <input v-model="resumeData.general.phone" type="text" class="w-full text-sm py-2 px-3 border border-slate-200 bg-white rounded-lg outline-none focus:ring-2 focus:ring-blue-500 placeholder-slate-400" placeholder="Số điện thoại" />
                <input v-model="resumeData.general.email" type="email" class="w-full text-sm py-2 px-3 border border-slate-200 bg-white rounded-lg outline-none focus:ring-2 focus:ring-blue-500 placeholder-slate-400" placeholder="Email" />
                <input v-model="resumeData.general.birthDate" type="text" class="w-full text-sm py-2 px-3 border border-slate-200 bg-white rounded-lg outline-none focus:ring-2 focus:ring-blue-500 placeholder-slate-400" placeholder="Ngày sinh (Ví dụ: 01/01/2000)" />
                <input v-model="resumeData.general.gender" type="text" class="w-full text-sm py-2 px-3 border border-slate-200 bg-white rounded-lg outline-none focus:ring-2 focus:ring-blue-500 placeholder-slate-400" placeholder="Giới tính (Nam/Nữ)" />
                <input v-model="resumeData.general.address" type="text" class="w-full text-sm py-2 px-3 border border-slate-200 bg-white rounded-lg outline-none focus:ring-2 focus:ring-blue-500 placeholder-slate-400 col-span-2" placeholder="Địa chỉ hiện tại" />
                <input v-model="resumeData.general.website" type="text" class="w-full text-sm py-2 px-3 border border-slate-200 bg-white rounded-lg outline-none focus:ring-2 focus:ring-blue-500 placeholder-slate-400 col-span-2" placeholder="Website / Portfolio / LinkedIn" />
            </div>
            <textarea v-model="resumeData.general.summary" rows="3" class="w-full text-sm py-2.5 px-3 border border-slate-200 bg-white rounded-lg focus:ring-2 focus:ring-blue-500 outline-none transition-all resize-none placeholder-slate-400 shadow-sm leading-relaxed" placeholder="Mục tiêu nghề nghiệp hoặc Tóm tắt bản thân..."></textarea>
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
                        <span class="drag-handle cursor-grab text-slate-400 hover:text-blue-600 active:cursor-grabbing p-1 bg-white rounded shadow-sm border border-slate-200">
                            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 8h16M4 16h16"></path></svg>
                        </span>
                        <input v-model="section.title" class="font-bold text-slate-800 bg-transparent py-1 px-2 rounded-md outline-none focus:ring-2 ring-blue-100 hover:bg-white w-full transition-all uppercase tracking-wide text-xs" />
                    </div>
                    <div class="flex items-center gap-2 ml-2 pl-3 border-l border-slate-200">
                        <!-- Toggle -->
                        <label class="relative inline-flex items-center cursor-pointer" title="Ẩn/Hiện thẻ này">
                            <input type="checkbox" v-model="section.isVisible" class="sr-only peer">
                            <div class="w-8 h-4 bg-slate-300 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-[16px] peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-3 after:w-3 after:transition-all peer-checked:bg-blue-500"></div>
                        </label>
                    </div>
                </div>
                
                    <!-- Section Items (Forms Type) -->
                <div v-show="section.isVisible" class="p-4 bg-white">
                    <!-- Textarea đặc biệt cho Mục tiêu nghề nghiệp -->
                    <div v-if="section.id === 'summary'">
                        <textarea v-model="resumeData.general.summary" rows="4" class="w-full text-xs py-2.5 px-3 border border-slate-200 bg-slate-50 rounded-xl focus:ring-2 focus:ring-blue-500 outline-none transition-all resize-none placeholder-slate-400 shadow-sm leading-relaxed" placeholder="Mô tả mục tiêu nghề nghiệp của bạn..."></textarea>
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
                                        <textarea v-model="item.desc" rows="2" :class="inputBaseClass" class="leading-relaxed resize-none text-xs" placeholder="Mô tả công việc (Dùng dấu • để liệt kê)"></textarea>
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
                                        <textarea v-model="item.desc" rows="2" :class="inputBaseClass" class="leading-relaxed resize-none text-xs" placeholder="Công nghệ sử dụng, Kết quả đạt được..."></textarea>
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
                                        <textarea v-model="item.desc" rows="2" :class="inputBaseClass" class="leading-relaxed resize-none text-xs" placeholder="Mô tả chi tiết hoạt động..."></textarea>
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
                                        <textarea v-model="item.info" rows="2" :class="inputBaseClass" class="leading-relaxed resize-none text-xs" placeholder="Họ tên, Chức vụ, Số điện thoại người tham chiếu"></textarea>
                                    </template>
                                </div>
                            </div>
                        </template>
                    </draggable>
                   
                    <button v-if="section.id !== 'summary'" @click="addItem(sectionIndex)" class="mt-3 w-full border border-dashed border-slate-300 hover:border-blue-500 text-slate-500 hover:text-blue-600 bg-slate-50/50 hover:bg-blue-50 transition-colors rounded-xl py-2 text-xs font-bold uppercase tracking-wider flex items-center justify-center gap-1.5 focus:outline-none">
                        <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path></svg> 
                        Thêm Dòng
                    </button>
                </div>
              </div>
            </template>
          </draggable>
        </div>
      </div>
    </div>

    <!-- CỘT PHẢI: PREVIEW PANEL THỜI GIAN THỰC -->
    <div class="flex-1 overflow-auto bg-slate-800 relative scroll-smooth pattern-dots" :style="{ height: '100%', '--theme-color': resumeData.theme.primaryColor }">
        
        <!-- Toolbar Zoom & PDF -->
        <div class="sticky top-4 pr-8 flex justify-end items-center gap-3 z-30 mb-4">
            <!-- Bộ điều khiển Zoom -->
            <div class="flex items-center bg-white/90 backdrop-blur-md border border-slate-200 rounded-full px-3 py-1.5 shadow-xl gap-2 mr-2">
                <button @click="previewScale = Math.max(0.8, previewScale - 0.1)" class="w-8 h-8 flex items-center justify-center rounded-full hover:bg-slate-100 text-slate-600 transition-colors" title="Thu nhỏ (Min 80%)">
                    <svg width="18" height="18" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M20 12H4"/></svg>
                </button>
                <span class="text-xs font-bold text-slate-700 w-12 text-center">{{ Math.round(previewScale * 100) }}%</span>
                <button @click="previewScale = Math.min(1.5, previewScale + 0.1)" class="w-8 h-8 flex items-center justify-center rounded-full hover:bg-slate-100 text-slate-600 transition-colors" title="Phóng to (Max 150%)">
                    <svg width="18" height="18" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M12 4v16m8-8H4"/></svg>
                </button>
            </div>

            <button @click="exportToPDF" class="bg-blue-600 text-white px-6 py-3 rounded-full font-bold shadow-2xl shadow-blue-500/50 hover:bg-blue-500 transform hover:-translate-y-1 transition-all text-sm flex items-center gap-2 ring-4 ring-white/10 group">
                <svg class="w-5 h-5 group-hover:animate-bounce" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z"></path></svg>
                Lưu Export PDF
            </button>
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
</template>

<script setup>
import { ref, onMounted, watch, shallowRef, defineAsyncComponent } from 'vue'
import draggable from 'vuedraggable'

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
    4: 'Template4' // Mẫu Premium chính
}

const activeTemplate = shallowRef(null) // Sẽ được set khi load data
const previewScale = ref(1.0); // Mặc định ban đầu là 100%


const isSaving = ref(false)
const resumeId = window.CURRENT_RESUME_ID || 0

// Khởi tạo Dữ liệu bám sát Models ResumeViewModel.cs
const resumeData = ref({
  theme: { primaryColor: '#2b5c8f' },
  general: {
    fullName: '', jobTitle: '', email: '', phone: '', address: '', birthDate: '', summary: '', website: '', avatarUrl: ''
  },
  sections: [
    { id: 'summary', title: 'Mục tiêu Nghề nghiệp', isVisible: true, items: [] },
    { id: 'experience', title: 'Kinh nghiệm Làm việc', isVisible: true, items: [] },
    { id: 'education', title: 'Quá trình Học vấn', isVisible: true, items: [] },
    { id: 'skills', title: 'Kỹ năng Chuyên môn', isVisible: true, items: [] },
    { id: 'it_skills', title: 'Tin học', isVisible: true, items: [] },
    { id: 'languages', title: 'Ngoại ngữ', isVisible: true, items: [] },
    { id: 'activities', title: 'Hoạt động', isVisible: false, items: [] },
    { id: 'project', title: 'Dự án Trọng điểm', isVisible: false, items: [] },
    { id: 'certifications', title: 'Chứng chỉ / Bằng cấp', isVisible: false, items: [] },
    { id: 'awards', title: 'Giải thưởng', isVisible: false, items: [] },
    { id: 'hobbies', title: 'Sở thích', isVisible: false, items: [] },
    { id: 'references', title: 'Người tham chiếu', isVisible: false, items: [] },
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

const exportToPDF = () => {
  // Lấy nội dung CV gốc
  const cvEl = document.getElementById('cv-printable-area');
  if (!cvEl) { window.print(); return; }

  // Sao chép toàn bộ các thẻ link/style từ head để đồng bộ font và CSS Tailwind
  let headHtml = '';
  const headNodes = document.head.querySelectorAll('link[rel="stylesheet"], style, link[href*="fonts.googleapis.com"]');
  headNodes.forEach(node => {
      headHtml += node.outerHTML;
  });

  // Thêm các rule in ấn tối thiểu để dọn dẹp GUI
  const printStyles = `
    <style>
      @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&display=swap');
      * { -webkit-print-color-adjust: exact !important; print-color-adjust: exact !important; box-sizing: border-box; }
      html, body { 
          margin: 0 !important; 
          padding: 0 !important; 
          background: white !important;
          width: 210mm;
          height: 297mm;
          font-family: 'Inter', sans-serif !important;
      }
      #cv-printable-area { 
          box-shadow: none !important; 
          width: 210mm !important;
          margin: 0 !important;
          transform: none !important;
          border: none !important;
          display: flex !important;
          flex-direction: column !important;
      }
      /* Ép layout 2 cột cho Iframe */
      #cv-printable-area .flex { display: flex !important; }
      #cv-printable-area aside { 
          width: 68mm !important; 
          min-width: 68mm !important; 
          max-width: 68mm !important;
          display: flex !important; 
          flex-direction: column !important; 
          flex-shrink: 0 !important;
      }
      #cv-printable-area main { 
          flex: 1 !important; 
          display: flex !important; 
          flex-direction: column !important; 
          min-width: 0 !important;
      }
      .no-print, .nav-btns, .delete-btn { display: none !important; }
      @page { size: A4 portrait; margin: 0; }
    </style>
  `;

  const iframe = document.createElement('iframe');
  iframe.style.cssText = 'position:fixed;top:-10000px;left:-10000px;width:210mm;height:297mm;border:none;visibility:hidden;';
  document.body.appendChild(iframe);

  const doc = iframe.contentDocument || iframe.contentWindow.document;
  doc.open();
  doc.write(`<!DOCTYPE html><html><head><meta charset="utf-8">${headHtml}${printStyles}</head><body>${cvEl.outerHTML}</body></html>`);
  doc.close();

  let printed = false;
  const doPrint = () => {
    if (printed) return;
    printed = true;
    try {
      iframe.contentWindow.focus();
      iframe.contentWindow.print();
    } catch(e) { window.print(); }
    setTimeout(() => {
      if (document.body.contains(iframe)) document.body.removeChild(iframe);
    }, 2000);
  };

  iframe.onload = () => setTimeout(doPrint, 1000);
  // Fallback sau 3s nếu onload chậm
  setTimeout(doPrint, 3000);
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
        if (data && data.templateId) {
            // 1. Thử tìm trong bảng ánh xạ Mapping
            targetName = templateMapping[data.templateId];
            
            // 2. Nếu không có trong mapping, thử tìm theo quy tắc Template<ID>.vue (VD: Template3.vue)
            if (!targetName) targetName = `Template${data.templateId}`;
        }

        // 3. Lấy component từ registry hoặc dùng fallback mẫu Premium (Template4)
        if (targetName && templateRegistry[targetName]) {
            activeTemplate.value = templateRegistry[targetName];
        } else {
            // Luôn đảm bảo có mẫu hiển thị (Fallback Template4)
            activeTemplate.value = templateRegistry['Template4'] || templateRegistry[Object.keys(templateRegistry)[0]];
            if (data && data.templateId) {
                console.warn(`Không tìm thấy mẫu CV ID: ${data.templateId}, đang sử dụng mẫu mặc định.`);
            }
        }

        if (data && data.jsonContent && data.jsonContent !== "{}") {
            const parsed = JSON.parse(data.jsonContent);
            
            // --- HỖ TRỢ OVERRIDE MẪU CHO TESTING (VUE TEMPLATES) ---
            if (parsed.overrideTemplate && templateRegistry[parsed.overrideTemplate]) {
                activeTemplate.value = templateRegistry[parsed.overrideTemplate];
                console.log(`[Vue Test] Overriding template to: ${parsed.overrideTemplate}`);
            }

            resumeData.value.theme = parsed.theme || resumeData.value.theme;
            resumeData.value.general = parsed.general || resumeData.value.general;
            
            if (parsed.sections) {
                // MIGRATION: Đảm bảo các section id mới luôn tồn tại
                const existingIds = parsed.sections.map(s => s.id);
                const missingSections = resumeData.value.sections.filter(s => !existingIds.includes(s.id));
                resumeData.value.sections = [...parsed.sections, ...missingSections];
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

onMounted(loadData);
</script>

<style>
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
