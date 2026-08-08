<template>
<div class="payable-management-container app-container">
    <el-card shadow="never" class="filter-card">
      <div style="font-size: 1.1rem; font-weight: bold; margin-bottom: 0.9375rem;">정산 내역</div>
      <el-form :inline="true" :model="orderHistoryQuery" class="demo-form-inline">
        <el-form-item label="거래처">
          <el-select v-model="orderHistoryQuery.companyId" placeholder="전체" clearable style="width: 180px;">
            <el-option v-for="c in list" :key="c.companyId" :label="c.companyName" :value="c.companyId" />
          </el-select>
        </el-form-item>
        <el-form-item label="검색기간">
          <el-date-picker
            v-model="orderHistoryDateRange"
            type="daterange"
            range-separator="~"
            start-placeholder="시작일"
            end-placeholder="종료일"
            value-format="YYYY-MM-DD"
            style="width: 260px;"
          />
        </el-form-item>
        <el-form-item label="상품명">
          <el-input v-model="orderHistoryQuery.productName" placeholder="상품명" clearable @keyup.enter="handleOrderHistoryFilter" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :icon="Search" @click="handleOrderHistoryFilter">조회</el-button>
        </el-form-item>
      </el-form>

      <table class="order-history-summary-table" v-loading="orderHistorySummaryLoading">
        <thead>
          <tr>
            <th></th>
            <th>순금(g)</th>
            <th>공임 및 현금</th>
            <th>금액합계</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td class="summary-label">총 판매</td>
            <td>{{ (orderHistorySummary.totalChargeWeight || 0).toFixed(2) }}</td>
            <td>₩ {{ formatPrice(orderHistorySummary.totalChargeAmount) }}</td>
            <td>0</td>
          </tr>
          <tr>
            <td class="summary-label">총 결제</td>
            <td>{{ (orderHistorySummary.totalPaidWeight || 0).toFixed(2) }}</td>
            <td>₩ {{ formatPrice(orderHistorySummary.totalPaidAmount) }}</td>
            <td>0</td>
          </tr>
        </tbody>
      </table>

      <div v-if="!isLogistics && selectedOrderRows.length > 0" class="batch-settle-bar">
        <span>{{ selectedOrderRows.length }}건 선택됨 · 청구 ₩{{ formatPrice(selectedTotalAmount) }} ({{ selectedTotalWeight.toFixed(2) }}g)</span>
        <el-button type="primary" @click="openBatchSettleDialog">정산</el-button>
      </div>

      <base-table
        ref="orderHistoryTableRef"
        v-loading="orderHistoryLoading"
        :data="orderHistoryList"
        :total="orderHistoryTotal"
        v-model:page="orderHistoryQuery.page"
        v-model:page-size="orderHistoryQuery.pageSize"
        border
        row-key="payableId"
        style="width: 100%; margin-top: 1.25rem;"
        @change="fetchOrderHistory"
        @selection-change="handleOrderHistorySelectionChange"
      >
        <el-table-column v-if="!isLogistics" type="selection" width="45" :selectable="isOrderRowSelectable" />
        <el-table-column label="주문번호" width="200" align="center">
          <template #default="{row}">
            <span class="order-link" @click="goToOrder(row.orderNo)">{{ row.orderNo }}</span>
          </template>
        </el-table-column>
        <el-table-column label="제품정보" min-width="280">
          <template #default="{row}">
            <div v-if="row.items && row.items.length > 0" class="product-info-list">
              <div v-for="(item, idx) in row.items.slice(0, 3)" :key="idx" class="product-info-cell">
                <el-image :src="item.photoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 36px; height: 36px;" />
                <div class="product-text">
                  <div class="product-name">
                    {{ item.productName || '-' }}
                    <span v-if="item.productNo" class="product-no-code">{{ item.productNo }}</span>
                  </div>
                  <div class="product-spec">
                    함량: {{ item.purity || '-' }} / 수량: {{ item.quantity }}개
                    <template v-if="item.color && item.color !== 'EMPTY'"> / 색상: {{ codeStore.codeMap[item.color] || item.color }}</template>
                    <template v-if="item.size && item.size !== 'EMPTY'"> / 사이즈: {{ item.size }}</template>
                  </div>
                  <div v-if="item.memo" class="product-memo">메모: {{ item.memo }}</div>
                </div>
              </div>
              <div v-if="row.items.length > 3" class="product-more">+{{ row.items.length - 3 }}건 더</div>
            </div>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column :label="isLogistics ? '공장' : '물류'" width="160" align="center">
          <template #default="{row}">
            {{ isLogistics ? row.manufacturerCompanyName : row.logisticsCompanyName }}
          </template>
        </el-table-column>
        <el-table-column label="주문일시" width="160" align="center">
          <template #default="{row}">
            {{ formatDate(row.orderDate) }}
          </template>
        </el-table-column>
        <el-table-column label="청구" width="130" align="right">
          <template #default="{row}">
            <span style="color: #f56c6c; font-weight: bold;">₩ {{ formatPrice(row.chargeAmount) }}</span>
          </template>
        </el-table-column>
        <el-table-column label="결제" width="130" align="right">
          <template #default="{row}">
            <span style="color: #67c23a; font-weight: bold;">₩ {{ formatPrice(row.paidAmount) }}</span>
          </template>
        </el-table-column>
        <el-table-column label="영수증 출력" width="120" align="center">
          <template #default="{row}">
            <el-button size="small" @click="handlePrintOrderReceipt(row)">출력</el-button>
          </template>
        </el-table-column>
        <el-table-column v-if="!isLogistics" label="정산처리" width="110" align="center">
          <template #default="{row}">
            <el-button v-if="row.remainingAmount > 0 || row.remainingWeight > 0" type="primary" size="small" @click="openSingleSettleDialog(row)">정산처리</el-button>
          </template>
        </el-table-column>
      </base-table>
    </el-card>

    <batch-settle-dialog
      v-model="batchSettleDialogVisible"
      :order-ids="dialogOrderIds"
      :single-mode="!!singleSettleRow"
      @saved="onBatchSettleSaved"
    />
  </div>
</template>

<script setup lang="ts">
import { useMobile } from '@/hooks/useMobile';
import { ref, reactive, computed, watch, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { getCompanySummaries, getPayableOrderHistory, getPayableOrderHistorySummary } from '@/api/payable';
import { ElMessage } from 'element-plus';
import { Search, Refresh } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import BaseTable from '@/components/BaseTable/index.vue';
import BatchSettleDialog from './components/BatchSettleDialog.vue';
import useUserStore from '@/store/modules/user';
import useCodeStore from '@/store/modules/code';

const { isMobile } = useMobile();
const userStore = useUserStore();
const codeStore = useCodeStore();
const router = useRouter();
const defaultImage = '/thumb_no_img.png';

const isLogistics = computed(() => userStore.companyType === 'DCC' || userStore.roles.includes('admin'));

const listLoading = ref(true);
const list = ref<any[]>([]);
const total = ref(0);

const listQuery = reactive({
  page: 1,
  pageSize: 20,
  search: ''
});

const orderHistoryLoading = ref(false);
const orderHistorySummaryLoading = ref(false);
const orderHistoryList = ref<any[]>([]);
const orderHistoryTotal = ref(0);
const orderHistoryDateRange = ref<[string, string] | null>(null);
const orderHistorySummary = reactive({
  totalChargeAmount: 0,
  totalChargeWeight: 0,
  totalPaidAmount: 0,
  totalPaidWeight: 0
});
const orderHistoryQuery = reactive({
  page: 1,
  pageSize: 20,
  companyId: undefined as number | undefined,
  startDate: undefined as string | undefined,
  endDate: undefined as string | undefined,
  productName: ''
});

watch(orderHistoryDateRange, (val) => {
  orderHistoryQuery.startDate = val ? val[0] : undefined;
  orderHistoryQuery.endDate = val ? val[1] : undefined;
});

const orderHistoryTableRef = ref<any>(null);
const selectedOrderRows = ref<any[]>([]);

const selectedTotalAmount = computed(() => selectedOrderRows.value.reduce((sum, r) => sum + (r.remainingAmount || 0), 0));
const selectedTotalWeight = computed(() => selectedOrderRows.value.reduce((sum, r) => sum + (r.remainingWeight || 0), 0));

const handleOrderHistorySelectionChange = (rows: any[]) => {
  selectedOrderRows.value = rows;
};

// A batch settlement is scoped to a single counterparty, so once one row is picked,
// only rows from that same company remain selectable - prevents silently mixing
// orders from different DCC partners into one ledger.
const isOrderRowSelectable = (row: any) => {
  if (row.remainingAmount <= 0 && row.remainingWeight <= 0) return false;
  if (selectedOrderRows.value.length === 0) return true;
  return row.logisticsCompanyId === selectedOrderRows.value[0].logisticsCompanyId;
};

const batchSettleDialogVisible = ref(false);
const singleSettleRow = ref<any>(null);

// Two entry points share one dialog: the checkbox+bar flow (multiple orders) and the
// per-row 정산처리 button (single order). singleSettleRow, when set, takes priority so
// the two flows never bleed into each other.
const dialogOrderIds = computed(() => {
  if (singleSettleRow.value) return [singleSettleRow.value.orderId];
  return selectedOrderRows.value.map((r) => r.orderId);
});

watch(batchSettleDialogVisible, (val) => {
  if (!val) singleSettleRow.value = null;
});

const openBatchSettleDialog = () => {
  if (selectedOrderRows.value.length === 0) return;
  singleSettleRow.value = null;
  batchSettleDialogVisible.value = true;
};

const openSingleSettleDialog = (row: any) => {
  singleSettleRow.value = row;
  batchSettleDialogVisible.value = true;
};

const onBatchSettleSaved = () => {
  selectedOrderRows.value = [];
  singleSettleRow.value = null;
  orderHistoryTableRef.value?.clearSelection?.();
  getList();
  fetchOrderHistory();
  fetchOrderHistorySummary();
};

const goToOrder = (orderNo: string) => {
  router.push({ path: '/order/order-tracking', query: { orderNo } });
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return '';
  return parseTime(new Date(dateStr), '{y}-{m}-{d} {h}:{i}');
};

const getList = async () => {
  listLoading.value = true;
  try {
    const res = await getCompanySummaries(listQuery);
    list.value = res.data.items;
    total.value = res.data.totalCount;
  } catch (error) {
    console.error('Failed to get company summaries:', error);
  } finally {
    listLoading.value = false;
  }
};

const fetchOrderHistory = async () => {
  orderHistoryLoading.value = true;
  try {
    const res: any = await getPayableOrderHistory(orderHistoryQuery);
    orderHistoryList.value = res.data.items;
    orderHistoryTotal.value = res.data.totalCount;
  } catch (error) {
    console.error('Failed to fetch payable order history:', error);
  } finally {
    orderHistoryLoading.value = false;
  }
};

const fetchOrderHistorySummary = async () => {
  orderHistorySummaryLoading.value = true;
  try {
    const res: any = await getPayableOrderHistorySummary(orderHistoryQuery);
    Object.assign(orderHistorySummary, res.data);
  } catch (error) {
    console.error('Failed to fetch payable order history summary:', error);
  } finally {
    orderHistorySummaryLoading.value = false;
  }
};

const handleOrderHistoryFilter = () => {
  orderHistoryQuery.page = 1;
  fetchOrderHistory();
  fetchOrderHistorySummary();
};

const handlePrintOrderReceipt = (row: any) => {
  const printWindow = window.open('', '_blank');
  if (!printWindow) return;

  const counterpartyLabel = isLogistics.value ? '공장' : '물류';
  const counterpartyName = isLogistics.value ? row.manufacturerCompanyName : row.logisticsCompanyName;

  const html = `
    <html>
      <head>
        <title>정산 영수증 - ${row.orderNo}</title>
        <style>
          body { font-family: 'Malgun Gothic', sans-serif; padding: 10mm; }
          .statement-title { font-weight: bold; font-size: 1.1rem; margin-bottom: 12px; text-align: center; }
          table { width: 100%; border-collapse: collapse; }
          th, td { border: 1px solid #333; padding: 8px; text-align: center; font-size: 0.9rem; }
          th { background: #f5f5f5; }
          td.label { text-align: left; background: #fafafa; font-weight: 600; }
        </style>
      </head>
      <body>
        <div class="statement-title">정산 영수증</div>
        <table>
          <tbody>
            <tr><td class="label">주문번호</td><td>${row.orderNo || '-'}</td></tr>
            <tr><td class="label">${counterpartyLabel}</td><td>${counterpartyName || '-'}</td></tr>
            <tr><td class="label">제품정보</td><td>${row.productName || '-'}${row.productItemCount > 1 ? ` 외 ${row.productItemCount - 1}건` : ''}</td></tr>
            <tr><td class="label">주문일시</td><td>${formatDate(row.orderDate)}</td></tr>
            <tr><td class="label">청구 금액</td><td>₩ ${formatPrice(row.chargeAmount)} (${(row.chargeWeight || 0).toFixed(2)}g)</td></tr>
            <tr><td class="label">결제 금액</td><td>₩ ${formatPrice(row.paidAmount)} (${(row.paidWeight || 0).toFixed(2)}g)</td></tr>
            <tr><td class="label">남은 미지급</td><td>₩ ${formatPrice(row.remainingAmount)} (${(row.remainingWeight || 0).toFixed(2)}g)</td></tr>
          </tbody>
        </table>
        <script>window.onload = () => { window.print(); setTimeout(() => window.close(), 500); };<\/script>
      </body>
    </html>
  `;

  printWindow.document.write(html);
  printWindow.document.close();
};

onMounted(() => {
  codeStore.fetchCodes();
  getList();
  fetchOrderHistory();
  fetchOrderHistorySummary();
});
</script>

<style lang="scss" scoped>
.filter-card {
  margin-bottom: 1.25rem;
}
.pagination-container {
  margin-top: 1.25rem;
  display: flex;
  justify-content: center;
}
.order-history-summary-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}
.order-history-summary-table th,
.order-history-summary-table td {
  border: 1px solid #ebeef5;
  padding: 0.5rem;
  text-align: center;
}
.order-history-summary-table th {
  background: #f5f7fa;
  font-weight: 600;
}
.summary-label {
  text-align: left;
  font-weight: 600;
  background: #fafafa;
  white-space: nowrap;
}
.order-link {
  color: #409eff;
  cursor: pointer;
}
.order-link:hover {
  text-decoration: underline;
}
.product-info-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding: 0.375rem 0;
}
.product-info-cell {
  display: flex;
  align-items: center;
  gap: 0.625rem;
}
.product-thumb {
  border-radius: 2px;
  border: 1px solid #ebeef5;
  flex-shrink: 0;
}
.product-text {
  min-width: 0;
}
.product-name {
  font-weight: 600;
}
.product-no-code {
  color: #409eff;
  font-size: 0.75rem;
  margin-left: 0.25rem;
}
.product-memo {
  font-size: 0.75rem;
  color: #e6a23c;
  margin-top: 0.125rem;
}
.product-spec {
  font-size: 0.75rem;
  color: #909399;
  margin-top: 0.125rem;
}
.product-more {
  font-size: 0.75rem;
  color: #409eff;
}
.batch-settle-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 1.25rem;
  padding: 0.75rem 1rem;
  background: #ecf5ff;
  border: 1px solid #d9ecff;
  border-radius: 2px;
  font-weight: 600;
  color: #303133;
}
</style>
