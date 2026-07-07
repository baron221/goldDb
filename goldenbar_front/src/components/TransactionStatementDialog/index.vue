<template>
<div>

    <base-popup
      v-model="visible"
      width="850px"
      class="statement-dialog-popup"
      @close="visible = false"
    >
      <template #header>
        <div class="luxury-dialog-header">
          <span class="el-dialog__title">거래명세서 {{ snapshot ? '(보관본)' : '' }}</span>
          <div class="print-actions-header">
            <el-button type="info" size="small" :icon="Printer" @click="handlePrint">인쇄하기</el-button>
            <el-button type="primary" size="small" :icon="Download" :loading="isDownloading" :disabled="isDownloading" @click="handleDownloadPDF">PDF 다운로드</el-button>
          </div>
        </div>
      </template>

      <div
        class="statement-scroll-container"
        v-loading="isDownloading"
        element-loading-text="PDF 생성 중... 잠시만 기다려주세요."
        element-loading-background="rgba(255, 255, 255, 0.7)"
      >
        <statement-paper
          id="statement-pdf-area-visible"
          :order="order"
          :code-map="codeMap"
          :statement="statement"
        />
      </div>
    </base-popup>

    <div
      v-if="!visible && order"
      class="hidden-render-container"
    >
      <statement-paper
        id="statement-pdf-area"
        :order="order"
        :code-map="codeMap"
        :statement="statement"
      />
    </div>
  </div>
</template>

<script setup lang="ts">

import { ref, computed, watch } from 'vue';
import { Printer, Download } from '@element-plus/icons-vue';
import html2canvas from 'html2canvas';
import jsPDF from 'jspdf';
import BasePopup from '@/components/BasePopup/index.vue';
import StatementPaper from './components/StatementPaper.vue';
import { ElMessage } from 'element-plus';
import { getOrderStatementPdf } from '@/api/order';

const props = defineProps<{
  modelValue: boolean;
  order: any;
  codeMap: Record<string, string>;
  statement?: any; 
}>();

const emit = defineEmits(['update:modelValue']);

const visible = ref(false);
const isDownloading = ref(false);

const snapshot = computed(() => {
  if (!props.statement?.snapshotData) return null;
  try {
    return JSON.parse(props.statement.snapshotData);
  } catch (e) {
    console.error('Failed to parse statement snapshot:', e);
    return null;
  }
});

const displayOrder = computed(() => snapshot.value?.order || props.order);

watch(() => props.modelValue, (val) => {
  visible.value = val;
});

watch(() => visible.value, (val) => {
  emit('update:modelValue', val);
});

const handlePrint = () => {
  const printArea = document.getElementById('statement-pdf-area-visible') || document.getElementById('statement-pdf-area');
  if (!printArea) return;

  const printWindow = window.open('', '_blank');
  if (printWindow) {
    const styles = Array.from(document.querySelectorAll('style, link[rel="stylesheet"]'))
      .map(s => s.outerHTML)
      .join('');

    const titleText = (snapshot.value ? '[보관본] ' : '') + '거래명세서 - ' + (displayOrder.value?.orderNo || '');
    printWindow.document.write('<html><head><title>' + titleText + '</title>' + styles + '<style>body { background: white !important; margin: 0; padding: 0; } .statement-paper { box-shadow: none !important; border: none !important; width: 100% !important; margin: 0 !important; padding: 10mm !important; } .items-table { width: 100% !important; border-collapse: collapse !important; } thead { display: table-header-group !important; } tfoot { display: table-footer-group !important; } tr { page-break-inside: avoid !important; } @page { size: A4; margin: 0; }</style></head><body>' + printArea.outerHTML + '<script>window.onload = () => { window.print(); setTimeout(() => window.close(), 500); };<' + '/script></body></html>');
    printWindow.document.close();
  }
};

const handleDownloadPDF = async () => {
  isDownloading.value = true;
  try {
    const orderId = props.order?.id || displayOrder.value?.id;
    const hasStatement = props.order?.hasStatement || !!props.statement;

    if (hasStatement && orderId) {
      const response: any = await getOrderStatementPdf(orderId);
      if (response.data && response.data.pdfBinary) {
        const dataUrl = `data:application/pdf;base64,${response.data.pdfBinary}`;
        const fetchResponse = await fetch(dataUrl);
        const blob = await fetchResponse.blob();

        const link = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        const fileName = (snapshot.value ? '[저장본]_' : '') + '거래명세서_' + (displayOrder.value?.orderNo || 'order') + '.pdf';
        link.download = fileName;
        link.click();

        ElMessage.success('서버에 저장된 PDF 파일을 다운로드합니다.');
        return;
      }
    }

    const element = document.getElementById('statement-pdf-area-visible') || document.getElementById('statement-pdf-area');
    if (!element) return;

    const canvas = await html2canvas(element, {
      scale: 2,
      useCORS: true,
      backgroundColor: '#ffffff'
    });

    const imgData = canvas.toDataURL('image/png');
    const pdf = new jsPDF('p', 'mm', 'a4');
    const imgProps = pdf.getImageProperties(imgData);
    const pdfWidth = pdf.internal.pageSize.getWidth();
    const pdfHeight = (imgProps.height * pdfWidth) / imgProps.width;

    pdf.addImage(imgData, 'PNG', 0, 0, pdfWidth, pdfHeight);
    const fileName = (snapshot.value ? '보관본_' : '') + '거래명세서_' + (displayOrder.value?.orderNo || 'order') + '.pdf';
    pdf.save(fileName);
  } catch (error) {
    console.error('PDF download/generation failed:', error);
    ElMessage.error('PDF 다운로드 중 오류가 발생했습니다.');
  } finally {
    isDownloading.value = false;
  }
};

const generateSnapshotAndPdf = async () => {

  const element = document.getElementById('statement-pdf-area');

  if (!element) return null;

  try {

    const snapshotObj = {
      order: props.order,
      codeMap: props.codeMap,
      timestamp: new Date().toISOString()
    };
    const snapshotJson = JSON.stringify(snapshotObj);

    const canvas = await html2canvas(element, {
      scale: 2,
      useCORS: true,
      backgroundColor: '#ffffff'
    });

    const imgData = canvas.toDataURL('image/png');
    const pdf = new jsPDF('p', 'mm', 'a4');
    const imgProps = pdf.getImageProperties(imgData);
    const pdfWidth = pdf.internal.pageSize.getWidth();
    const pdfHeight = (imgProps.height * pdfWidth) / imgProps.width;

    pdf.addImage(imgData, 'PNG', 0, 0, pdfWidth, pdfHeight);

    const pdfBase64 = pdf.output('datauristring').split(',')[1];

    return {
      snapshot: snapshotJson,
      pdfBase64
    };
  } catch (error) {
    console.error('[DEBUG] Snapshot/PDF generation failed:', error);
    return null;
  }
};

defineExpose({
  generateSnapshotAndPdf
});
</script>

<style lang="scss" src="./StatementStyles.scss" scoped></style>

