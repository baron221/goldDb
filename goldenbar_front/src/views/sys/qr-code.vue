<template>
<div class="app-container">
    <el-card shadow="never">
      <template #header>
        <div class="card-header">
          <span>QR 코드 생성 및 인쇄</span>
        </div>
      </template>

      <el-row :gutter="40">

        <el-col :xs="24" :sm="12" class="mb-4 sm:mb-0">
          <el-form label-position="top">
            <el-form-item label="대상 URL 또는 텍스트">
              <el-input
                v-model="qrValue"
                type="textarea"
                :rows="4"
                placeholder="https://example.com"
                @input="generateQr"
              />
            </el-form-item>

            <el-row :gutter="20">
              <el-col :xs="24" :sm="12">
                <el-form-item label="크기 (px)">
                  <el-input-number v-model="qrSize" :min="100" :max="1000" :step="50" style="width: 100%;" />
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12">
                <el-form-item label="오류 정정 수준">
                  <el-select v-model="qrLevel" style="width: 100%;">
                    <el-option label="Low (7%)" value="L" />
                    <el-option label="Medium (15%)" value="M" />
                    <el-option label="Quartile (25%)" value="Q" />
                    <el-option label="High (30%)" value="H" />
                  </el-select>
                </el-form-item>
              </el-col>
            </el-row>

            <el-row :gutter="20">
              <el-col :xs="24" :sm="12">
                <el-form-item label="전경색">
                  <el-color-picker v-model="qrForeground" style="width: 100%;" />
                </el-form-item>
              </el-col>
              <el-col :xs="24" :sm="12">
                <el-form-item label="배경색">
                  <el-color-picker v-model="qrBackground" style="width: 100%;" />
                </el-form-item>
              </el-col>
            </el-row>

            <div style="margin-top: 1.25rem;">
              <el-button type="primary" :icon="Download" @click="downloadQr">이미지 저장</el-button>
              <el-button type="success" :icon="Printer" @click="printQr">QR 코드 인쇄</el-button>
              <el-button @click="resetSettings">초기화</el-button>
            </div>
          </el-form>
        </el-col>

        <el-col :xs="24" :sm="12" class="preview-container">
          <div class="preview-box" id="qr-preview">
            <div class="qr-wrapper" ref="qrWrapper">
              <qrcode-vue
                :value="qrValue"
                :size="qrSize"
                :level="qrLevel"
                :foreground="qrForeground"
                :background="qrBackground"
                render-as="canvas"
              />
            </div>
            <div class="qr-info" v-if="qrValue">
              <p class="url-text">{{ qrValue }}</p>
            </div>
          </div>
          <div class="preview-label">미리보기</div>
        </el-col>
      </el-row>
    </el-card>

    <div id="print-area" style="display: none;">
      <div style="text-align: center; padding: 3.125rem;">
        <div id="print-qr-content"></div>
        <div style="margin-top: 1.25rem; font-size: 0.875rem; color: #333;">{{ qrValue }}</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import QrcodeVue from 'qrcode.vue';
import { Download, Printer } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';

const qrValue = ref('https://goldb.jsini.co.kr');
const qrSize = ref(300);
const qrLevel = ref('M');
const qrForeground = ref('#000000');
const qrBackground = ref('#ffffff');
const qrWrapper = ref(null);

const resetSettings = () => {
  qrValue.value = 'https://goldb.jsini.co.kr';
  qrSize.value = 300;
  qrLevel.value = 'M';
  qrForeground.value = '#000000';
  qrBackground.value = '#ffffff';
};

const downloadQr = () => {
  const canvas = document.querySelector('#qr-preview canvas') as HTMLCanvasElement;
  if (!canvas) return;

  const url = canvas.toDataURL('image/png');
  const a = document.createElement('a');
  a.download = 'qrcode.png';
  a.href = url;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  ElMessage.success('QR 코드가 이미지로 저장되었습니다.');
};

const printQr = () => {
  const canvas = document.querySelector('#qr-preview canvas') as HTMLCanvasElement;
  if (!canvas) return;

  const dataUrl = canvas.toDataURL('image/png');
  const windowContent = `
    <!DOCTYPE html>
    <html>
      <head>
        <title>QR Code Print</title>
        <style>
          body { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100vh; margin: 0; }
          img { max-width: 100%; }
          .footer { margin-top: 1.25rem; font-size: 1rem; font-family: sans-serif; word-break: break-all; text-align: center; max-width: 80%; }
        </style>
      </head>
      <body>
        <img src="${dataUrl}" />
        <div class="footer">${qrValue.value}</div>
        <script>
          window.onload = () => {
            window.print();
            window.onafterprint = () => window.close();
          };
        <\/script>
      </body>
    </html>
  `;

  const printWindow = window.open('', '_blank', 'width=600,height=600');
  if (printWindow) {
    printWindow.document.open();
    printWindow.document.write(windowContent);
    printWindow.document.close();
  } else {
    ElMessage.error('팝업 차단을 해제해주세요.');
  }
};

const generateQr = () => {

};
</script>

<style scoped>
.preview-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background-color: #f5f7fa;
  border-radius: 2px;
  min-height: 450px;
}

.preview-box {
  background: white;
  padding: 1.875rem;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  border-radius: 2px;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.qr-wrapper {
  line-height: 0;
}

.qr-info {
  margin-top: 1.25rem;
  width: 100%;
  text-align: center;
}

.url-text {
  font-size: 0.9rem;
  color: #666;
  word-break: break-all;
  margin: 0;
  max-width: 300px;
}

.preview-label {
  margin-top: 0.9375rem;
  font-size: 0.875rem;
  color: #909399;
  font-weight: bold;
}
</style>

