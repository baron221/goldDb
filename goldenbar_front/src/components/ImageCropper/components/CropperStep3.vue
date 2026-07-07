<template>
<div class="vicp-step3">
    <div class="vicp-upload">
      <span v-show="loading === 1" class="vicp-loading">{{ lang.loading }}</span>
      <div class="vicp-progress-wrap">
        <span v-show="loading === 1" :style="progressStyle" class="vicp-progress" />
      </div>
      <div v-show="hasError" class="vicp-error">
        <i class="vicp-icon2" />
        {{ errorMsg }}
      </div>
      <div v-show="loading === 2" class="vicp-success">
        <i class="vicp-icon3" />
        {{ lang.success }}
      </div>
    </div>
    <div class="vicp-operate">
      <a @click="$emit('setStep', 2)" @mousedown="$emit('ripple', $event)">{{ lang.btn.back }}</a>
      <a @click="$emit('off')" @mousedown="$emit('ripple', $event)">{{ lang.btn.close }}</a>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps({
  loading: {
    type: Number,
    required: true
  },
  lang: {
    type: Object,
    required: true
  },
  progressStyle: {
    type: Object,
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

defineEmits(['setStep', 'off', 'ripple']);
</script>

