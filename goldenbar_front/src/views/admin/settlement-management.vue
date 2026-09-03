<template>
<div class="settlement-management-container app-container">
    <el-card v-if="isDcc" shadow="never" class="filter-card">
      <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.9375rem;">
        <div style="font-size: 1.1rem; font-weight: bold;">정산 대상 내역</div>
        <el-button type="warning" plain :icon="Plus" @click="manualOrderDialogVisible = true">주문 수기 등록</el-button>
      </div>
      <el-form :inline="true" :model="orderHistoryQuery" class="demo-form-inline">
        <el-form-item label="소매점">
          <company-select v-model="orderHistoryQuery.companyId" category="RTL" placeholder="전체" style="width: 180px;" @change="handleOrderHistoryFilter" />
          <el-button style="margin-left: 0.375rem;" @click="handleAllCompanies">전체소매점</el-button>
        </el-form-item>
        <el-form-item label="비고">
          <el-input v-model="orderHistoryQuery.remarks" placeholder="비고" clearable @keyup.enter="handleOrderHistoryFilter" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :icon="Search" @click="handleOrderHistoryFilter">조회</el-button>
        </el-form-item>
      </el-form>

      <div v-if="selectedOrderRows.length > 0" class="batch-settle-bar">
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
        row-key="receivableId"
        style="width: 100%; margin-top: 1.25rem;"
        @change="fetchOrderHistory"
        @selection-change="handleOrderHistorySelectionChange"
      >
        <el-table-column type="selection" width="45" :selectable="isOrderRowSelectable" />
        <el-table-column label="주문번호" width="200" align="center" prop="orderNo" />
        <el-table-column label="제품정보" min-width="280">
          <template #default="{row}">
            <div v-if="(row.items || []).length > 0" class="product-info-list">
              <div v-for="(item, idx) in row.items.slice(0, 3)" :key="idx" class="product-info-cell">
                <el-image :src="item.photoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 56px; height: 56px;" />
                <div class="product-text">
                  <div class="product-name">
                    {{ item.productName || '-' }}
                    <span v-if="item.productNo" class="product-no-code">{{ item.productNo }}</span>
                  </div>
                  <div class="product-spec">
                    함량: {{ codeMap[item.purity] || item.purity || '-' }} / 중량: {{ item.actualWeight ? item.actualWeight + 'g' : '-' }} / 수량: {{ item.quantity }}개
                    <template v-if="item.color && item.color !== 'EMPTY'"> / 색상: {{ codeMap[item.color] || item.color }}</template>
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
        <el-table-column label="비고" min-width="160">
          <template #default="{row}">
            <span v-if="row.remarks" style="color: #606266;">{{ row.remarks }}</span>
            <span v-else style="color: #c0c4cc;">-</span>
          </template>
        </el-table-column>
        <el-table-column label="소매점" width="160" align="center">
          <template #default="{row}">
            {{ row.retailerCompanyName || row.userDisplayName }}
          </template>
        </el-table-column>
        <el-table-column label="청구일시" width="160" align="center">
          <template #default="{row}">{{ formatDate(row.orderDate) }}</template>
        </el-table-column>
        <el-table-column label="청구" width="150" align="right">
          <template #default="{row}">
            <span style="color: #f56c6c; font-weight: bold;">₩ {{ formatPrice(row.chargeAmount) }}</span>
            <span v-if="row.chargeWeight > 0" style="color: #909399; font-size: 0.8125rem;"> ({{ row.chargeWeight.toFixed(2) }}g)</span>
          </template>
        </el-table-column>
      </base-table>
    </el-card>

    <receivable-batch-settle-dialog
      v-model="batchSettleDialogVisible"
      :order-ids="selectedOrderRows.map((r) => r.orderId)"
      @saved="onBatchSettleSaved"
    />

    <order-manual-register-dialog
      v-model="manualOrderDialogVisible"
      @saved="onManualOrderSaved"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue';
import { getReceivableOrderHistory } from '@/api/receivable';
import { Search, Plus } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import BaseTable from '@/components/BaseTable/index.vue';
import CompanySelect from '@/components/CompanySelect/index.vue';
import useCodeStore from '@/store/modules/code';
import useUserStore from '@/store/modules/user';
import OrderManualRegisterDialog from './components/OrderManualRegisterDialog.vue';
import ReceivableBatchSettleDialog from './components/ReceivableBatchSettleDialog.vue';

const userStore = useUserStore();
const codeStore = useCodeStore();
const codeMap = computed(() => codeStore.codeMap);
const defaultImage = '/thumb_no_img.png';

const isDcc = computed(() => userStore.companyType === 'DCC' || userStore.roles.includes('admin'));

// Charge-state-based worklist (mirrors MFG's payable-management.vue 정산 내역 table exactly):
// lists every completely untouched Receivable charge regardless of the underlying order's
// delivery/logistics status, so a charge can never become invisible just because its order's
// status moved on without ever being paid. The moment any payment lands on one it drops out
// of here and into 미수금 관리 instead (GetReceivableOrderHistoryAsync enforces this
// server-side, and never filters by order status at all).
const orderHistoryLoading = ref(false);
const orderHistoryList = ref<any[]>([]);
const orderHistoryTotal = ref(0);
const orderHistoryQuery = reactive({
  page: 1,
  pageSize: 20,
  companyId: undefined as number | undefined,
  remarks: ''
});

const handleAllCompanies = () => {
  orderHistoryQuery.companyId = undefined;
  handleOrderHistoryFilter();
};

const orderHistoryTableRef = ref<any>(null);
const selectedOrderRows = ref<any[]>([]);

const selectedTotalAmount = computed(() => selectedOrderRows.value.reduce((sum, r) => sum + (r.remainingAmount || 0), 0));
const selectedTotalWeight = computed(() => selectedOrderRows.value.reduce((sum, r) => sum + (r.remainingWeight || 0), 0));

const handleOrderHistorySelectionChange = (rows: any[]) => {
  selectedOrderRows.value = rows;
};

// A batch settlement is scoped to a single retailer, so once one row is picked, only
// rows from that same retailer remain selectable - prevents silently mixing different
// retailers' charges into one deposit.
const isOrderRowSelectable = (row: any) => {
  if (row.remainingAmount <= 0 && row.remainingWeight <= 0) return false;
  if (selectedOrderRows.value.length === 0) return true;
  return row.userId === selectedOrderRows.value[0].userId;
};

const batchSettleDialogVisible = ref(false);
const manualOrderDialogVisible = ref(false);

const openBatchSettleDialog = () => {
  if (selectedOrderRows.value.length === 0) return;
  batchSettleDialogVisible.value = true;
};

const onBatchSettleSaved = () => {
  selectedOrderRows.value = [];
  orderHistoryTableRef.value?.clearSelection?.();
  fetchOrderHistory();
};

// A manually-registered order still has to pass through logistics approval/inspection
// before it becomes a settleable charge, so it won't show up in this worklist yet -
// nothing here needs to refresh.
const onManualOrderSaved = () => {};

const formatDate = (dateStr: string) => {
  if (!dateStr) return '';
  return parseTime(new Date(dateStr), '{y}-{m}-{d} {h}:{i}');
};

const fetchOrderHistory = async () => {
  orderHistoryLoading.value = true;
  try {
    const res: any = await getReceivableOrderHistory(orderHistoryQuery);
    orderHistoryList.value = res.data.items;
    orderHistoryTotal.value = res.data.totalCount;
  } catch (error) {
    console.error('Failed to fetch receivable order history:', error);
  } finally {
    orderHistoryLoading.value = false;
  }
};

const handleOrderHistoryFilter = () => {
  orderHistoryQuery.page = 1;
  fetchOrderHistory();
};

onMounted(() => {
  codeStore.fetchCodes();
  if (isDcc.value) {
    fetchOrderHistory();
  }
});
</script>

<style lang="scss" scoped>
@import "./SettlementManagementStyles.scss";
</style>
