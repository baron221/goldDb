<template>
<base-popup
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    :title="qrStockNo ? `QR 코드 - ${qrStockNo}` : 'QR 코드'"
    width="650px"
    align-center
    destroy-on-close
  >
    <div class="qr-dialog-container">
      <div class="qr-dialog-body">
        <div class="qr-code-wrapper" ref="qrWrapperRef">
          <qrcode-vue
            :value="qrUrl"
            :size="240"
            level="H"
            render-as="canvas"
          />
        </div>
        <p class="qr-stock-no">{{ qrStockNo }}</p>
        <a class="qr-url-text clickable" :href="qrUrl" target="_blank">{{ qrUrl }}</a>
      </div>
      <div class="qr-info-side">
        <div class="info-row">
          <span class="label">제품번호</span>
          <span class="value">{{ qrInfo?.productNo || '-' }}</span>
        </div>
        <div class="info-row">
          <span class="label">제품명</span>
          <span class="value">{{ qrInfo?.productName || '-' }}</span>
        </div>
        <div class="info-row">
          <span class="label">카테고리</span>
          <span class="value">
            {{ codeMap[qrInfo?.categoryLarge] || qrInfo?.categoryLarge }}
            <template v-if="qrInfo?.categoryMedium"> > {{ codeMap[qrInfo?.categoryMedium] || qrInfo?.categoryMedium }}</template>
            <template v-if="qrInfo?.categorySmall"> > {{ codeMap[qrInfo?.categorySmall] || qrInfo?.categorySmall }}</template>
          </span>
        </div>
        <div class="info-row">
          <span class="label">제품생산일</span>
          <span class="value">{{ qrInfo?.productionDate ? formatDate(qrInfo.productionDate).split(' ')[0] : '-' }}</span>
        </div>
        <div class="info-row">
          <span class="label">주문일시</span>
          <span class="value">{{ qrInfo?.sourceOrderDate ? formatDate(qrInfo.sourceOrderDate) : '-' }}</span>
        </div>
        <div class="info-row">
          <span class="label">제조사</span>
          <span class="value">{{ qrInfo?.companyName || '-' }}</span>
        </div>
        <div class="info-row">
          <span class="label">물류사</span>
          <span class="value">{{ qrInfo?.logisticsCompanyName || '-' }}</span>
        </div>
      </div>
    </div>
    <template #footer>
      <el-button @click="$emit('print-qr')">인쇄</el-button>
      <el-button type="warning" @click="$emit('print-barcode')">인쇄2</el-button>
      <el-button type="info" @click="$emit('print-qr3')">인쇄3</el-button>
      <el-button type="success" @click="$emit('print-pdf')">PDF 다운로드</el-button>
      <el-button type="primary" @click="$emit('update:modelValue', false)">닫기</el-button>
    </template>
  </base-popup>
</template>

<script setup lang="ts">

import { ref, onMounted } from 'vue';
import QrcodeVue from 'qrcode.vue';
import BasePopup from '@/components/BasePopup/index.vue';

defineProps({

  modelValue: {
    type: Boolean,
    required: true
  },

  qrUrl: {
    type: String,
    required: true
  },

  qrStockNo: {
    type: String,
    required: true
  },

  qrInfo: {
    type: Object,
    default: null
  },

  codeMap: {
    type: Object,
    default: () => ({})
  },

  formatDate: {
    type: Function,
    required: true
  }
});

defineEmits([
  'update:modelValue',

  'print-qr',

  'print-barcode',

  'print-qr3',

  'print-pdf'
]);

const qrWrapperRef = ref<HTMLElement | null>(null);

defineExpose({

  qrWrapperRef
});
</script>

<style lang="scss" scoped>

.qr-dialog-container {
  display: flex;
  gap: 1.875rem;
  align-items: center;
  padding: 0.625rem 0.625rem;
}

.qr-dialog-body {
  flex: 0 0 300px;
  display: flex;
  flex-direction: column;
  align-items: center;

  .qr-code-wrapper {
    border: 1px solid #eae6df;
    padding: 1rem;
    background: #fff;
    display: inline-flex;
    border-radius: 0;
    box-shadow: 0 2px 8px rgba(0,0,0,0.06);
  }

  .qr-stock-no {
    margin: 0.875rem 0 0.25rem;
    font-size: 0.9375rem;
    font-weight: 700;
    letter-spacing: 2px;
    color: #1a1a1a;
    font-family: 'S-CoreDream', 'Jost', monospace;
    text-transform: uppercase;
  }

  .qr-url-text {
    margin: 0;
    font-size: 0.825rem;
    color: #aaa;
    word-break: break-all;
    text-align: center;
    max-width: 260px;
    line-height: 1.5;
    text-decoration: none;

    &.clickable {
      cursor: pointer;
      color: #c5a880;
      transition: color 0.2s;

      &:hover {
        text-decoration: underline;
        color: #b0946d;
      }
    }
  }
}

.qr-info-side {
  flex: 1;
  background-color: #faf9f6;
  border-radius: 2px;
  padding: 1.25rem;
  border: 1px solid #eae6df;

  .info-row {
    display: flex;
    justify-content: space-between;
    padding: 0.625rem 0;
    border-bottom: 1px dashed #eae6df;

    &:last-child {
      border-bottom: none;
    }

    .label {
      font-size: 0.9rem;
      color: #888;
      font-weight: 500;
    }

    .value {
      font-size: 0.9rem;
      color: #222;
      font-weight: 600;
      font-family: 'S-CoreDream', 'Jost', sans-serif;
    }
  }
}
</style>

