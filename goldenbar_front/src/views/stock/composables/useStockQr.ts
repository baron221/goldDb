import { ref, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';
import { ElMessage } from 'element-plus';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';
import { formatPrice } from '@/utils/format';
import { printBarcode, printQr3, printQr } from '../utils/stockPrint';

export function useStockQr(codeMap: any) {
  const { t } = useI18n();

  const qrDialogVisible = ref(false);
  const qrUrl = ref('');
  const qrStockNo = ref('');
  const qrInfo = ref<any>(null);
  const qrInfoDialogRef = ref<any>(null);
  const pdfLabelRef = ref<HTMLElement | null>(null);

  const formatDate = (dateStr: string) => {
    if (!dateStr) return '-';
    const d = new Date(dateStr);
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`;
  };

  const handleQrPdf = async () => {
    if (!pdfLabelRef.value) return;
    try {
      const canvas = await html2canvas(pdfLabelRef.value, {
        scale: 3,
        useCORS: true,
        backgroundColor: '#ffffff'
      });
      const imgData = canvas.toDataURL('image/png');
      const JsPDF = jsPDF as any;
      const pdf = new JsPDF({ orientation: 'landscape', unit: 'mm', format: [50, 30] });
      pdf.addImage(imgData, 'PNG', 0, 0, 50, 30);
      pdf.save(`label_${qrStockNo.value}.pdf`);
    } catch (error) {
      console.error(t('apiErrors.unknown'), error);
      ElMessage.error(t('apiErrors.unknown'));
    }
  };

  const handleBarcodePrint = () => {
    printBarcode(qrInfo.value, codeMap.value, t('stock.labels.manufacturer'));
  };

  const handleQrPrint3 = () => {
    const canvas = qrInfoDialogRef.value?.qrWrapperRef?.querySelector('canvas');
    if (!canvas) return;
    const dataUrl = canvas.toDataURL('image/png');
    const labels = {
      productName: t('dashboard.factory.table.productName'),
      spec: t('productMarket.labels.mediumCategory'),
      weight: t('dashboard.factory.table.weight'),
      date: t('dashboard.factory.table.requestedAt'),
      mfg: t('stock.labels.manufacturer')
    };
    const productionDateFormatted = qrInfo.value?.productionDate ? formatDate(qrInfo.value.productionDate).split(' ')[0] : '-';
    printQr3(qrInfo.value, codeMap.value, dataUrl, labels, productionDateFormatted);
  };

  const handleQrPrint = () => {
    const canvas = qrInfoDialogRef.value?.qrWrapperRef?.querySelector('canvas');
    if (!canvas) return;
    const dataUrl = canvas.toDataURL('image/png');
    const labels = {
      orderNo: t('order.filters.orderNo'),
      productName: t('dashboard.factory.table.productName'),
      category: t('marketplace.filters.category'),
      date1: t('dashboard.factory.table.requestedAt'),
      date2: t('dashboard.factory.table.requestedAt'),
      mfg: t('stock.labels.manufacturer'),
      logistics: t('dashboard.logistics.title')
    };
    const dates = {
      productionDate: qrInfo.value?.productionDate ? formatDate(qrInfo.value.productionDate).split(' ')[0] : '-',
      orderDate: qrInfo.value?.sourceOrderDate ? formatDate(qrInfo.value.sourceOrderDate) : '-'
    };
    printQr(qrInfo.value, codeMap.value, dataUrl, labels, dates);
  };

  const openQrDialog = (row: any) => {
    const baseUrl = `${window.location.protocol}//${window.location.host}${window.location.pathname}`;
    qrUrl.value = `${baseUrl}#/stock/stock_detail/${row.id}`;
    qrStockNo.value = row.stockNo;
    qrInfo.value = row;
    qrDialogVisible.value = true;
  };

  return {
    qrDialogVisible,
    qrUrl,
    qrStockNo,
    qrInfo,
    qrInfoDialogRef,
    pdfLabelRef,
    formatDate,
    handleQrPdf,
    handleBarcodePrint,
    handleQrPrint3,
    handleQrPrint,
    openQrDialog
  };
}
