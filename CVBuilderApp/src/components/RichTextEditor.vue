<template>
  <div
    :class="['rich-text-editor outline-none w-full', { 'is-empty': isVisualEmpty }]"
    contenteditable="true"
    @input="onInput"
    @blur="onBlur"
    @paste="onPaste"
    ref="editor"
    :data-placeholder="placeholder"
  ></div>
</template>

<script setup>
import { ref, onMounted, watch, computed } from 'vue';

const props = defineProps({
  modelValue: {
    type: String,
    default: ''
  },
  placeholder: {
    type: String,
    default: ''
  }
});

const isVisualEmpty = computed(() => {
  if (!props.modelValue) return true;
  // Xóa hết tags và kiểm tra xem còn chữ không
  const cleanText = props.modelValue.replace(/<[^>]*>/g, '').replace(/&nbsp;/g, ' ').trim();
  return cleanText === '';
});

const emit = defineEmits(['update:modelValue']);
const editor = ref(null);

onMounted(() => {
  if (editor.value) {
    editor.value.innerHTML = props.modelValue || '';
  }
});

watch(() => props.modelValue, (newVal) => {
  if (editor.value && newVal !== editor.value.innerHTML) {
     editor.value.innerHTML = newVal || '';
  }
});

const onInput = (e) => {
  emit('update:modelValue', e.target.innerHTML);
};

const onBlur = (e) => {
  emit('update:modelValue', e.target.innerHTML);
};

const onPaste = (e) => {
  e.preventDefault();
  // Ngăn chặn các style rác từ web được dán vào, chỉ lấy plain text
  const text = e.clipboardData.getData('text/plain');
  // Chèn text (tự động xử lý xuống dòng với execCommand hoặc text/plain thì nó thay \n thành nội dung text)
  document.execCommand('insertText', false, text);
};
</script>

<style scoped>
.rich-text-editor {
  min-height: 30px;
  cursor: text;
  position: relative;
  color: #1e293b; /* Màu chữ chế độ sáng mặc định */
}

/* HỖ TRỢ CHẾ ĐỘ DARK MODE (BOOTSTRAP [data-bs-theme="dark"]) */
:global([data-bs-theme="dark"]) .rich-text-editor {
  color: #f8fafc !important; /* Chữ trắng sáng (slate-50) nổi bật trên nền tối */
}

/* HỖ TRỢ CHẾ ĐỘ DARK MODE (TAILWIND .dark) */
:global(.dark) .rich-text-editor {
  color: #f8fafc !important;
}

.rich-text-editor.is-empty:before {
  content: attr(data-placeholder);
  color: #94a3b8; /* Tailwind slate-400 */
  pointer-events: none;
  position: absolute;
}

:global([data-bs-theme="dark"]) .rich-text-editor.is-empty:before {
  color: #64748b; /* slate-500 sáng và tương tương dễ đọc ở nền tối */
}

:global(.dark) .rich-text-editor.is-empty:before {
  color: #64748b;
}

/* Tuỳ chọn CSS để các ul, ol trong editor sẽ hiển thị rõ */
.rich-text-editor :deep(ul) {
  list-style-type: disc !important;
  padding-left: 1.5rem !important;
}
.rich-text-editor :deep(ol) {
  list-style-type: decimal !important;
  padding-left: 1.5rem !important;
}
</style>
