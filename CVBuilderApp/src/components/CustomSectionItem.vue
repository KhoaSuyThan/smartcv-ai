<template>
  <div class="custom-section-item paginated-item w-full">
    <!-- Title & Time concatenated by a hyphen -->
    <div :class="titleClass" v-if="!isEmpty(item.name) || !isEmpty(item.time)">
      <span v-if="!isEmpty(item.name)" v-html="item.name"></span>
      <span v-if="!isEmpty(item.name) && !isEmpty(item.time)"> - </span>
      <span v-if="!isEmpty(item.time)" :class="timeClass" v-html="item.time"></span>
    </div>
    <!-- Role / Position -->
    <div :class="roleClass" v-if="!isEmpty(item.role)">
      <span v-html="item.role"></span>
    </div>
    <!-- Description -->
    <div :class="descClass" v-if="!isEmpty(item.desc)" v-html="formatDesc(item.desc)"></div>
  </div>
</template>

<script setup>
const props = defineProps({
  item: { type: Object, required: true },
  titleClass: { type: String, default: '' },
  timeClass: { type: String, default: '' },
  roleClass: { type: String, default: '' },
  descClass: { type: String, default: 'html-content text-justify' }
})

const isEmpty = (val) => {
  if (!val) return true;
  if (typeof val !== 'string') return false;
  return val.replace(/<[^>]*>/g, '').trim() === '';
}

const formatDesc = (text) => {
  if (!text) return ''
  // If plain text (no HTML tags), format into paginated divs
  if (!/<[a-z][\s\S]*>/i.test(text)) {
    return text.split('\n')
               .map(l => l.trim())
               .filter(Boolean)
               .map(l => `<div class="paginated-item">${l}</div>`)
               .join('')
  }
  return text;
}
</script>

<style scoped>
.custom-section-item {
  position: relative;
}
</style>
