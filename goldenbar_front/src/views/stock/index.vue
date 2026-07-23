<template>
<div class="app-container">
    <stock-summary-card :summary="summary" />

    <el-card shadow="never" style="margin-top: 1.25rem;">
      <div style="display: flex; justify-content: flex-end; margin-bottom: 0.9375rem;">
        <el-button type="success" :icon="Download" @click="handleExportExcel">엑셀 다운로드</el-button>
      </div>

      <stock-filter
        v-model="listQuery"
        :is-admin="isAdmin"
        :large-id="largeId"
        :medium-id="mediumId"
        :set-large-id="setLargeId"
        :set-medium-id="setMediumId"
        :selected-groups-count="selectedGroups.length"
        @filter="handleFilter"
        @large-change="handleLargeChange"
        @medium-change="handleMediumChange"
        @set-large-change="handleSetLargeChange"
        @set-medium-change="handleSetMediumChange"
        @bulk-print="handleBulkBarcodePrint"
      />

      <stock-table
        ref="stockTableRef"
        :data="groupedList"
        :loading="listLoading"
        :code-map="codeMap"
        :format-price="formatPrice"
        @selection-change="handleSelectionChange"
        @open-details="openGroupDetails"
      />

      <div class="pagination-container" style="margin-top: 1.25rem;">
        <el-pagination
          v-model:current-page="listQuery.page"
          v-model:page-size="listQuery.pageSize"
          :total="total"
          :page-sizes="[20, 50, 100, 200]"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="getList"
          @current-change="getList"
        />
      </div>

    </el-card>

    <group-detail-dialog
      v-model="groupDetailDialogVisible"
      :selected-group="selectedGroup"
      :code-map="codeMap"
      :format-date="formatDate"
      :format-price="formatPrice"
      :get-status-name="getStatusName"
      :get-status-tag-type="getStatusTagType"
      @navigate-stock="navigateToStock"
      @navigate-order="navigateToOrder"
      @print-barcode="handlePrintBarcode"
      @open-photo="openPhotoDialog"
      @delete="handleDelete"
    />

    <stock-exhaustion-reason-dialog
      v-model="deleteDialogVisible"
      :ids="deleteStockIds"
      @saved="getList"
    />

    <stock-photo-dialog
      v-model="photoDialogVisible"
      :stock-id="currentStockId"
      :attachments="currentAttachments"
      @uploaded="getList"
    />

    <qr-info-dialog
      v-model="qrDialogVisible"
      :qr-url="qrUrl"
      :qr-stock-no="qrStockNo"
      :qr-info="qrInfo"
      :code-map="codeMap"
      :format-date="formatDate"
      ref="qrInfoDialogRef"
      @print-qr="handleQrPrint"
      @print-barcode="handleBarcodePrint"
      @print-qr3="handleQrPrint3"
      @print-pdf="handleQrPdf"
    />

    <div v-if="qrInfo" style="position: absolute; left: -9999px; top: -9999px;">
      <div id="pdf-label-template" ref="pdfLabelRef" style="width: 750px; height: 450px; background: white; display: flex; align-items: center; padding: 0.9375rem; box-sizing: border-box;">
        <div style="flex: 0 0 200px; height: 200px; display: flex; align-items: center; justify-content: center; padding-left: 0.625rem;">
          <qrcode-vue
            :value="qrUrl"
            :size="170"
            level="H"
            render-as="canvas"
          />
        </div>
        <div style="flex: 1; padding-left: 0.625rem; display: flex; flex-direction: column; justify-content: center; font-family: sans-serif; white-space: nowrap; overflow: hidden;">
          <div style="margin-bottom: 0.9375rem; display: flex; align-items: center;"><span style="font-weight: bold; font-size: 1.625rem; letter-spacing: 1px;">{{ qrInfo.stockNo }}</span></div>
          <div style="margin-bottom: 0.9375rem; display: flex; align-items: center;"><span style="font-weight: normal; width: 60px; flex-shrink: 0; font-size: 1.125rem;">{{ $t('stock.labels.product') }}</span> <span style="font-weight: bold; font-size: 1.125rem; overflow: hidden; text-overflow: ellipsis;">{{ qrInfo.productNo }} / {{ qrInfo.productName }}</span></div>
          <div style="margin-bottom: 0.9375rem; display: flex; align-items: center;"><span style="font-weight: normal; width: 60px; flex-shrink: 0; font-size: 1.125rem;">{{ $t('stock.labels.production') }}</span> <span style="font-weight: bold; font-size: 1.125rem;">{{ qrInfo.productionDate ? formatDate(qrInfo.productionDate).split(' ')[0] : '-' }}</span></div>
          <div style="margin-bottom: 0.9375rem; display: flex; align-items: center;"><span style="font-weight: normal; width: 60px; flex-shrink: 0; font-size: 1.125rem;">{{ $t('stock.labels.manufacturer') }}</span> <span style="font-weight: bold; font-size: 1.125rem;">{{ qrInfo.companyName || '-' }}</span></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">

import { ref, reactive, onMounted, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import QrcodeVue from 'qrcode.vue';
import {
  fetchStocks, fetchStockSummary
} from '@/api/stock';
import { ElMessage } from 'element-plus';
import { Download } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import useUserStore from '@/store/modules/user';
import useCodeStore from '@/store/modules/code';
import StockSummaryCard from './components/StockSummaryCard.vue';
import StockFilter from './components/StockFilter.vue';
import StockTable from './components/StockTable.vue';
import StockPhotoDialog from '@/components/StockPhotoDialog/index.vue';
import StockExhaustionReasonDialog from './components/StockExhaustionReasonDialog.vue';
import GroupDetailDialog from './components/GroupDetailDialog.vue';
import QrInfoDialog from './components/QrInfoDialog.vue';
import { printBulkBarcode } from './utils/stockPrint';
import { useStockQr } from './composables/useStockQr';

const { t } = useI18n();
const userStore = useUserStore();
const router = useRouter();
const route = useRoute();
const codeStore = useCodeStore();
const isAdmin = computed(() => userStore.roles.includes('admin'));
const stockTableRef = ref<any>(null);

const handleExportExcel = () => {
  if (stockTableRef.value) {
    stockTableRef.value.exportExcel('재고_현황_목록');
  }
};

const list = ref<any[]>([]);
const total = ref(0);
const listLoading = ref(true);
const codeMap = computed(() => codeStore.codeMap);
const summary = reactive({
  total14KWeight: 0,
  total18KWeight: 0,
  totalPureGoldWeight: 0,
  totalCalculatedPureGoldWeight: 0
});

const selectedGroups = ref<any[]>([]);
const handleSelectionChange = (val: any[]) => { selectedGroups.value = val; };
const handleBulkBarcodePrint = () => { printBulkBarcode(selectedGroups.value, codeMap.value); };

const photoDialogVisible = ref(false);
const currentStockId = ref(0);
const currentAttachments = ref([]);
const openPhotoDialog = (row: any) => {
  currentStockId.value = row.id;
  currentAttachments.value = row.attachments || [];
  photoDialogVisible.value = true;
};

const {
  qrDialogVisible, qrUrl, qrStockNo, qrInfo, qrInfoDialogRef, pdfLabelRef,
  formatDate, handleQrPdf, handleBarcodePrint, handleQrPrint3, handleQrPrint, openQrDialog
} = useStockQr(codeMap);

const handlePrintBarcode = (row: any) => openQrDialog(row);

const groupDetailDialogVisible = ref(false);
const selectedGroupKey = ref<string | null>(null);

const groupedList = computed(() => {
  const groups: Record<string, any> = {};
  list.value.forEach(item => {
    const key = `${item.productNo}_${item.purity}_${item.color || ''}`;
    if (!groups[key]) {
      const materialCost = item.retailerConfirmMaterialCost || 0;
      const laborCost = item.retailerConfirmLaborCost || 0;
      groups[key] = {
        key,
        productNo: item.productNo,
        productName: item.productName,
        categoryLarge: item.categoryLarge,
        categoryMedium: item.categoryMedium,
        categorySmall: item.categorySmall,
        purity: item.purity,
        color: item.color,
        productPhotoUrl: item.productPhotoUrl,
        attachments: item.attachments || [],
        quantity: 0,
        totalWeight: 0,
        retailerConfirmMaterialCost: materialCost,
        retailerConfirmLaborCost: laborCost,
        materialLaborCostText: `재료비: ₩${materialCost.toLocaleString()} / 수공비: ₩${laborCost.toLocaleString()}`,
        items: []
      };
    }
    groups[key].quantity += item.quantity || 1;
    groups[key].totalWeight += (item.actualWeight || 0) * (item.quantity || 1);
    groups[key].items.push(item);
  });
  return Object.values(groups);
});

const selectedGroup = computed(() => {
  if (!selectedGroupKey.value) return null;
  return groupedList.value.find(g => g.key === selectedGroupKey.value) || null;
});

const openGroupDetails = (group: any) => {
  selectedGroupKey.value = group.key;
  groupDetailDialogVisible.value = true;
};

const navigateToOrder = (orderNo: string) => {
  groupDetailDialogVisible.value = false;
  router.push({ path: '/my/order', query: { orderNo }});
};

const navigateToStock = (stockId: number) => {
  groupDetailDialogVisible.value = false;
  router.push(`/stock/stock_detail/${stockId}`);
};

const listQuery = ref({
  page: 1,
  pageSize: 50,
  orderNo: '',
  stockNo: '',
  productName: '',
  status: '',
  categoryLarge: '',
  categoryMedium: '',
  categorySmall: '',
  setCategoryLarge: '',
  setCategoryMedium: '',
  setCategorySmall: '',
  minWeight: undefined as number | undefined,
  maxWeight: undefined as number | undefined,
  companyId: undefined as number | undefined,
  exhaustionDateStart: '',
  exhaustionDateEnd: '',
  createdAtStart: '',
  createdAtEnd: ''
});

const largeId = ref<number | null>(null);
const mediumId = ref<number | null>(null);
const setLargeId = ref<number | null>(null);
const setMediumId = ref<number | null>(null);

const handleLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  largeId.value = selected ? selected.id : null;
  listQuery.value.categoryMedium = '';
  listQuery.value.categorySmall = '';
  mediumId.value = null;
  handleFilter();
};

const handleMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  mediumId.value = selected ? selected.id : null;
  listQuery.value.categorySmall = '';
  handleFilter();
};

const handleSetLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  setLargeId.value = selected ? selected.id : null;
  listQuery.value.setCategoryMedium = '';
  listQuery.value.setCategorySmall = '';
  setMediumId.value = null;
  handleFilter();
};

const handleSetMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  setMediumId.value = selected ? selected.id : null;
  listQuery.value.setCategorySmall = '';
  handleFilter();
};

const getList = async () => {
  listLoading.value = true;
  try {
    if (!isAdmin.value) {
      listQuery.value.companyId = userStore.companyId || undefined;
    }

    const [res] = await Promise.all([
      fetchStocks(listQuery.value),
      codeStore.fetchCodes()
    ]);

    list.value = res.data.items.map((item: any) => ({ ...item, isDirty: false }));
    total.value = res.data.totalCount;

    const sumRes = await fetchStockSummary(listQuery.value);
    Object.assign(summary, sumRes.data);
  } catch (error) {
    console.error(error);
  } finally {
    listLoading.value = false;
  }
};

const getStatusName = (status: string) => {
  return t(`stock.status.${status}`);
};

const getStatusTagType = (status: string) => {
  switch (status) {
    case 'IN_STOCK': return 'success';
    case 'RENTED': return 'warning';
    case 'SOLD': return 'info';
    case 'DAMAGED':
    case 'LOST':
    case 'DISCARDED': return 'danger';
    default: return '';
  }
};

const handleFilter = () => {
  listQuery.value.page = 1;
  getList();
};

const deleteDialogVisible = ref(false);
const deleteStockIds = ref<number[]>([]);

const handleDelete = (row: any) => {
  deleteStockIds.value = [row.id];
  deleteDialogVisible.value = true;
};

onMounted(() => {
  if (route.query.status) {
    listQuery.value.status = route.query.status as string;
  }
  if (route.query.exhaustionDateStart) {
    listQuery.value.exhaustionDateStart = route.query.exhaustionDateStart as string;
  }
  if (route.query.exhaustionDateEnd) {
    listQuery.value.exhaustionDateEnd = route.query.exhaustionDateEnd as string;
  }
  if (route.query.createdAtStart) {
    listQuery.value.createdAtStart = route.query.createdAtStart as string;
  }
  if (route.query.createdAtEnd) {
    listQuery.value.createdAtEnd = route.query.createdAtEnd as string;
  }

  getList();
});
</script>

<script lang="ts">
export default {
  name: 'StockManagement'
};
</script>

<style lang="scss" src="./StockStyles.scss" scoped></style>

