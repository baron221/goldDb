<template>
<div v-show="value" class="vue-image-crop-upload">
    <div class="vicp-wrap">
      <div class="vicp-close" @click="off">
        <i class="vicp-icon4" />
      </div>

      <cropper-step1
        v-if="step == 1"
        :loading="loading"
        :lang="lang"
        :is-supported="isSupported"
        :has-error="hasError"
        :error-msg="errorMsg"
        @file-selected="onFileSelected"
        @off="off"
        @ripple="ripple"
      />

      <cropper-step2
        v-if="step == 2"
        :source-img-url="sourceImgUrl"
        :source-img-style="sourceImgStyle"
        :source-img-shade-style="sourceImgShadeStyle"
        :scale="scale"
        :no-rotate="noRotate"
        :no-square="noSquare"
        :no-circle="noCircle"
        :create-img-url="createImgUrl"
        :preview-style="previewStyle"
        :lang="lang"
        @imgStartMove="imgStartMove"
        @imgMove="imgMove"
        @createImg="createImg"
        @zoomChange="zoomChange"
        @startZoomSub="startZoomSub"
        @endZoomSub="endZoomSub"
        @startZoomAdd="startZoomAdd"
        @endZoomAdd="endZoomAdd"
        @startRotateLeft="startRotateLeft"
        @startRotateRight="startRotateRight"
        @endRotate="endRotate"
        @setStep="setStep"
        @prepareUpload="prepareUpload"
        @ripple="ripple"
      />

      <cropper-step3
        v-if="step == 3"
        :loading="loading"
        :lang="lang"
        :progress-style="progressStyle"
        :has-error="hasError"
        :error-msg="errorMsg"
        @setStep="setStep"
        @off="off"
        @ripple="ripple"
      />

      <canvas v-show="false" ref="canvasRef" :width="width" :height="height" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import CropperStep1 from './components/CropperStep1.vue';
import CropperStep2 from './components/CropperStep2.vue';
import CropperStep3 from './components/CropperStep3.vue';
import effectRipple from './utils/effectRipple.js';
import { useImageCropper } from './hooks/useImageCropper';

const props = defineProps({
  field: {
    type: String,
    default: 'avatar'
  },
  ki: {
    type: Number,
    default: 0
  },
  value: {
    type: Boolean,
    default: true
  },
  url: {
    type: String,
    default: ''
  },
  params: {
    type: Object,
    default: null
  },
  headers: {
    type: Object,
    default: null
  },
  width: {
    type: Number,
    default: 200
  },
  height: {
    type: Number,
    default: 200
  },
  noRotate: {
    type: Boolean,
    default: true
  },
  noCircle: {
    type: Boolean,
    default: false
  },
  noSquare: {
    type: Boolean,
    default: false
  },
  maxSize: {
    type: Number,
    default: 10240
  },
  langType: {
    type: String,
    default: 'zh'
  },
  langExt: {
    type: Object,
    default: null
  },
  imgFormatRaw: {
    type: String,
    default: 'png'
  },
  withCredentials: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits([
  'input',
  'close',
  'crop-success',
  'crop-upload-success',
  'crop-upload-fail'
]);

const canvasRef = ref<HTMLCanvasElement | null>(null);

const ripple = (e: Event) => {
  effectRipple(e);
};

const {
  lang,
  isSupported,
  step,
  loading,
  hasError,
  errorMsg,
  sourceImgUrl,
  createImgUrl,
  scale,
  progressStyle,
  sourceImgStyle,
  sourceImgShadeStyle,
  previewStyle,
  off,
  setStep,
  onFileSelected,
  imgStartMove,
  imgMove,
  createImg,
  zoomChange,
  startZoomSub,
  endZoomSub,
  startZoomAdd,
  endZoomAdd,
  startRotateLeft,
  startRotateRight,
  endRotate,
  prepareUpload
} = useImageCropper(props, emit, canvasRef);
</script>

<style lang="scss" src="./styles.scss"></style>

