<template>
<div class="vicp-step2">
    <div class="vicp-crop">
      <div v-show="true" class="vicp-crop-left">
        <div class="vicp-img-container">
          <img
            ref="img"
            :src="sourceImgUrl"
            :style="sourceImgStyle"
            class="vicp-img"
            draggable="false"
            @drag="preventDefault"
            @dragstart="preventDefault"
            @dragend="preventDefault"
            @dragleave="preventDefault"
            @dragover="preventDefault"
            @dragenter="preventDefault"
            @drop="preventDefault"
            @touchstart="$emit('imgStartMove', $event)"
            @touchmove="$emit('imgMove', $event)"
            @touchend="$emit('createImg')"
            @touchcancel="$emit('createImg')"
            @mousedown="$emit('imgStartMove', $event)"
            @mousemove="$emit('imgMove', $event)"
            @mouseup="$emit('createImg')"
            @mouseout="$emit('createImg')"
          >
          <div :style="sourceImgShadeStyle" class="vicp-img-shade vicp-img-shade-1" />
          <div :style="sourceImgShadeStyle" class="vicp-img-shade vicp-img-shade-2" />
        </div>

        <div class="vicp-range">
          <input
            :value="scale.range"
            type="range"
            step="1"
            min="0"
            max="100"
            @input="$emit('zoomChange', $event)"
          >
          <i
            class="vicp-icon5"
            @mousedown="$emit('startZoomSub')"
            @mouseout="$emit('endZoomSub')"
            @mouseup="$emit('endZoomSub')"
          />
          <i
            class="vicp-icon6"
            @mousedown="$emit('startZoomAdd')"
            @mouseout="$emit('endZoomAdd')"
            @mouseup="$emit('endZoomAdd')"
          />
        </div>

        <div v-if="!noRotate" class="vicp-rotate">
          <i @mousedown="$emit('startRotateLeft')" @mouseout="$emit('endRotate')" @mouseup="$emit('endRotate')">↺</i>
          <i @mousedown="$emit('startRotateRight')" @mouseout="$emit('endRotate')" @mouseup="$emit('endRotate')">↻</i>
        </div>
      </div>
      <div v-show="true" class="vicp-crop-right">
        <div class="vicp-preview">
          <div v-if="!noSquare" class="vicp-preview-item">
            <img :src="createImgUrl" :style="previewStyle">
            <span>{{ lang.preview }}</span>
          </div>
          <div v-if="!noCircle" class="vicp-preview-item vicp-preview-item-circle">
            <img :src="createImgUrl" :style="previewStyle">
            <span>{{ lang.preview }}</span>
          </div>
        </div>
      </div>
    </div>
    <div class="vicp-operate">
      <a @click="$emit('setStep', 1)" @mousedown="$emit('ripple', $event)">{{ lang.btn.back }}</a>
      <a class="vicp-operate-btn" @click="$emit('prepareUpload')" @mousedown="$emit('ripple', $event)">{{ lang.btn.save }}</a>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps({
  sourceImgUrl: {
    type: String,
    required: true
  },
  sourceImgStyle: {
    type: Object,
    required: true
  },
  sourceImgShadeStyle: {
    type: Object,
    required: true
  },
  scale: {
    type: Object,
    required: true
  },
  noRotate: {
    type: Boolean,
    default: true
  },
  noSquare: {
    type: Boolean,
    default: false
  },
  noCircle: {
    type: Boolean,
    default: false
  },
  createImgUrl: {
    type: String,
    required: true
  },
  previewStyle: {
    type: Object,
    required: true
  },
  lang: {
    type: Object,
    required: true
  }
});

defineEmits([
  'imgStartMove',
  'imgMove',
  'createImg',
  'zoomChange',
  'startZoomSub',
  'endZoomSub',
  'startZoomAdd',
  'endZoomAdd',
  'startRotateLeft',
  'startRotateRight',
  'endRotate',
  'setStep',
  'prepareUpload',
  'ripple'
]);

const preventDefault = (e: Event) => {
  e.preventDefault();
  return false;
};
</script>

