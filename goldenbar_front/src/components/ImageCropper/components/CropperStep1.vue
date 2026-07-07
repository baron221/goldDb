<template>
<div class="vicp-step1">
    <div
      class="vicp-drop-area"
      @dragleave="preventDefault"
      @dragover="preventDefault"
      @dragenter="preventDefault"
      @click="handleClick"
      @drop="handleChange"
    >
      <i v-show="loading != 1" class="vicp-icon1">
        <i class="vicp-icon1-arrow" />
        <i class="vicp-icon1-body" />
        <i class="vicp-icon1-bottom" />
      </i>
      <span v-show="loading !== 1" class="vicp-hint">{{ lang.hint }}</span>
      <span v-show="!isSupported" class="vicp-no-supported-hint">{{ lang.noSupported }}</span>
      <input v-show="false" ref="fileinput" type="file" @change="handleChange">
    </div>
    <div v-show="hasError" class="vicp-error">
      <i class="vicp-icon2" />
      {{ errorMsg }}
    </div>
    <div class="vicp-operate">
      <a @click="$emit('off')" @mousedown="$emit('ripple', $event)">{{ lang.btn.off }}</a>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';

defineProps({
  loading: {
    type: Number,
    required: true
  },
  lang: {
    type: Object,
    required: true
  },
  isSupported: {
    type: Boolean,
    required: true
  },
  hasError: {
    type: Boolean,
    required: true
  },
  errorMsg: {
    type: String,
    required: true
  }
});

const emit = defineEmits(['file-selected', 'off', 'ripple']);

const fileinput = ref<HTMLInputElement | null>(null);

const preventDefault = (e: Event) => {
  e.preventDefault();
  return false;
};

const handleClick = (e: Event) => {
  if (fileinput.value && e.target !== fileinput.value) {
    e.preventDefault();
    fileinput.value.click();
  }
};

const handleChange = (e: any) => {
  e.preventDefault();
  const files = e.target.files || e.dataTransfer.files;
  if (files && files.length > 0) {
    emit('file-selected', files[0]);
  }
};
</script>

