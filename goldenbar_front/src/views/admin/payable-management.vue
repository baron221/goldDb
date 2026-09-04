<template>
<div class="payable-management-container app-container">
    <el-card v-if="!isLogistics" shadow="never" class="filter-card">
      <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.9375rem;">
        <div style="font-size: 1.1rem; font-weight: bold;">정산 내역</div>
        <el-button type="warning" plain :icon="Plus" @click="manualOrderDialogVisible = true">주문 수기 등록</el-button>
      </div>
      <el-form :inline="true" :model="orderHistoryQuery" class="demo-form-inline">
        <el-form-item label="업체">
          <el-select v-model="orderHistoryQuery.companyId" placeholder="전체" clearable style="width: 180px;">
            <el-option v-for="c in list" :key="c.companyId" :label="c.companyName" :value="c.companyId" />
          </el-select>
          <el-button style="margin-left: 0.375rem;" @click="handleAllCompanies">전체업체</el-button>
        </el-form-item>
        <el-form-item label="비고">
          <el-input v-model="orderHistoryQuery.remarks" placeholder="비고" clearable @keyup.enter="handleOrderHistoryFilter" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :icon="Search" @click="handleOrderHistoryFilter">조회</el-button>
        </el-form-item>
      </el-form>

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
        <el-table-column label="주문번호" width="200" align="center" prop="orderNo" />
        <el-table-column label="제품정보" min-width="280">
          <template #default="{row}">
            <div v-if="row.items && row.items.length > 0" class="product-info-list">
              <div v-for="(item, idx) in row.items.slice(0, 3)" :key="idx" class="product-info-cell">
                <el-image :src="item.photoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 56px; height: 56px;" />
                <div class="product-text">
                  <div class="product-name">
                    {{ item.productName || '-' }}
                    <span v-if="item.productNo" class="product-no-code">{{ item.productNo }}</span>
                  </div>
                  <div class="product-spec">
                    함량: {{ item.purity || '-' }} / 중량: {{ item.actualWeight ? item.actualWeight + 'g' : '-' }} / 수량: {{ item.quantity }}개
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
        <el-table-column label="비고" min-width="160">
          <template #default="{row}">
            <span v-if="row.remarks" style="color: #606266;">{{ row.remarks }}</span>
            <span v-else style="color: #c0c4cc;">-</span>
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
        <el-table-column label="청구" width="150" align="right">
          <template #default="{row}">
            <span style="color: #f56c6c; font-weight: bold;">₩ {{ formatPrice(row.chargeAmount) }}</span>
            <span v-if="row.chargeWeight > 0" style="color: #909399; font-size: 0.8125rem;"> ({{ row.chargeWeight.toFixed(2) }}g)</span>
          </template>
        </el-table-column>
      </base-table>
    </el-card>

    <template v-if="isLogistics">
      <div style="display: flex; justify-content: space-between; align-items: center; margin: 1.25rem 0 0.9375rem;">
        <div style="font-size: 1.1rem; font-weight: bold;">정산받은 내역</div>
        <el-button type="warning" plain :icon="Plus" @click="manualOrderDialogVisible = true">주문 수기 등록</el-button>
      </div>
      <settlement-history-filter
        :query="payableQuery"
        :is-mobile="isMobile"
        company-category="MFG"
        company-label="제조사"
        @update:query="Object.assign(payableQuery, $event)"
        @filter="handlePayableFilter"
        @reset="resetPayableQuery"
        @print-combined="printCombinedPayableStatement"
      />

      <table class="order-history-summary-table">
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
            <td>{{ (payableSummary.totalChargeWeight || 0).toFixed(2) }}</td>
            <td>₩ {{ formatPrice(payableSummary.totalChargeAmount) }}</td>
            <td>0</td>
          </tr>
          <tr>
            <td class="summary-label">총 결제</td>
            <td>{{ (payableSummary.totalPaidWeight || 0).toFixed(2) }}</td>
            <td>₩ {{ formatPrice(payableSummary.totalPaidAmount) }}</td>
            <td>0</td>
          </tr>
          <tr>
            <td class="summary-label">미수금</td>
            <td>{{ payableOutstandingWeight.toFixed(2) }}</td>
            <td>₩ {{ formatPrice(payableOutstandingAmount) }}</td>
            <td>0</td>
          </tr>
        </tbody>
      </table>

      <div style="margin: 1.25rem 0;">
        <el-radio-group v-model="payableViewMode" @change="handleViewModeChange">
          <el-radio-button value="transaction">거래별 보기</el-radio-button>
          <el-radio-button value="order">주문별 보기</el-radio-button>
        </el-radio-group>
      </div>

      <el-card v-if="payableViewMode === 'order'" shadow="never">
        <base-table
          v-loading="completedOrderLoading"
          :data="completedOrderList"
          :total="completedOrderTotal"
          v-model:page="payableQuery.page"
          v-model:page-size="payableQuery.pageSize"
          border
          row-key="payableId"
          style="width: 100%;"
          @change="fetchCompletedOrders"
          @expand-change="handleCompletedOrderExpandChange"
        >
          <el-table-column type="expand">
            <template #default="{row}">
              <div class="order-detail-expand" v-loading="chargeApplicationsLoading[row.payableId]">
                <h4>결제 내역 (이 주문이 정산된 거래번호 목록)</h4>
                <base-table :data="chargeApplicationsData[row.payableId] || []" border size="small" style="width: 100%" row-key="paymentId">
                  <el-table-column label="거래번호" width="120" align="center" prop="paymentId" />
                  <el-table-column label="결제일시" width="160" align="center">
                    <template #default="{row: app}">
                      {{ formatDate(app.paymentCreatedAt) }}
                    </template>
                  </el-table-column>
                  <el-table-column label="적용 금액" width="140" align="right">
                    <template #default="{row: app}">
                      <span style="color: #67c23a; font-weight: bold;">₩ {{ formatPrice(app.appliedAmount) }}</span>
                    </template>
                  </el-table-column>
                  <el-table-column label="적용 중량" width="120" align="right">
                    <template #default="{row: app}">
                      {{ (app.appliedWeight || 0).toFixed(2) }}g
                    </template>
                  </el-table-column>
                  <el-table-column label="상태" width="100" align="center">
                    <template #default="{row: app}">
                      <el-tag v-if="app.isCancelled" type="info" size="small">취소됨</el-tag>
                      <el-tag v-else type="success" size="small">완료</el-tag>
                    </template>
                  </el-table-column>
                  <el-table-column label="메모" min-width="140" prop="memo" />
                </base-table>
              </div>
            </template>
          </el-table-column>
          <el-table-column label="주문번호" width="200" align="center" prop="orderNo" />
          <el-table-column label="제품정보" min-width="280">
            <template #default="{row}">
              <div v-if="row.items && row.items.length > 0" class="product-info-list">
                <div v-for="(item, idx) in row.items.slice(0, 3)" :key="idx" class="product-info-cell">
                  <el-image :src="item.photoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 56px; height: 56px;" />
                  <div class="product-text">
                    <div class="product-name">
                      {{ item.productName || '-' }}
                      <span v-if="item.productNo" class="product-no-code">{{ item.productNo }}</span>
                    </div>
                    <div class="product-spec">함량: {{ item.purity || '-' }} / 수량: {{ item.quantity }}개</div>
                  </div>
                </div>
                <div v-if="row.items.length > 3" class="product-more">+{{ row.items.length - 3 }}건 더</div>
              </div>
              <span v-else>-</span>
            </template>
          </el-table-column>
          <el-table-column label="발행처" width="160" align="center">
            <template #default="{row}">
              {{ row.manufacturerCompanyName }}
            </template>
          </el-table-column>
          <el-table-column label="주문일시" width="160" align="center">
            <template #default="{row}">
              {{ formatDate(row.orderDate) }}
            </template>
          </el-table-column>
          <el-table-column label="정산 금액" width="150" align="right">
            <template #default="{row}">
              <span style="font-weight: bold; color: #67c23a;">₩ {{ formatPrice(row.chargeAmount) }}</span>
              <span v-if="row.chargeWeight > 0" style="color: #909399; font-size: 0.8125rem;"> ({{ row.chargeWeight.toFixed(2) }}g)</span>
            </template>
          </el-table-column>
          <el-table-column label="작업" width="120" align="center">
            <template #default="{row}">
              <el-button size="small" @click="handleIssueOrderStatement(row)">거래명세서</el-button>
            </template>
          </el-table-column>
        </base-table>
      </el-card>

      <el-card v-else shadow="never">
        <base-table
          v-loading="payableLoading"
          :data="payableList"
          :total="payableTotal"
          v-model:page="payableQuery.page"
          v-model:page-size="payableQuery.pageSize"
          border
          row-key="id"
          style="width: 100%;"
          @change="fetchPayableList"
        >
          <el-table-column label="" width="70" align="center">
            <template #default="{row}">
              <el-button size="small" text type="primary" @click="openLedgerDetail(row)">상세</el-button>
            </template>
          </el-table-column>
          <el-table-column label="거래번호" width="100" align="center" prop="id" />
          <el-table-column label="발행처" width="180" align="center">
            <template #default="{row}">
              {{ row.manufacturerCompanyName }}
            </template>
          </el-table-column>
          <el-table-column label="거래일자" width="160" align="center">
            <template #default="{row}">
              {{ formatDate(row.createdAt) }}
            </template>
          </el-table-column>
          <el-table-column label="거래 주문 수량" width="130" align="center">
            <template #default="{row}">
              {{ row.orderCount }}건
            </template>
          </el-table-column>
          <el-table-column label="정산 금액" width="150" align="right">
            <template #default="{row}">
              <span style="font-weight: bold; color: #67c23a;">₩ {{ formatPrice(row.amount) }}</span>
              <span v-if="row.weight > 0" style="color: #909399; font-size: 0.8125rem;"> ({{ row.weight.toFixed(2) }}g)</span>
            </template>
          </el-table-column>
          <el-table-column label="상태" width="100" align="center">
            <template #default="{row}">
              <el-tag v-if="row.isCancelled" type="info" size="small">취소됨</el-tag>
              <el-tag v-else-if="(row.outstandingChargeAmount || 0) > 0 || (row.outstandingChargeWeight || 0) > 0" type="warning" size="small">부분 정산</el-tag>
              <el-tag v-else type="success" size="small">완료</el-tag>
            </template>
          </el-table-column>
          <el-table-column label="미수 잔액" width="150" align="right">
            <template #default="{row}">
              <span v-if="!row.isCancelled && ((row.outstandingChargeAmount || 0) > 0 || (row.outstandingChargeWeight || 0) > 0)" style="color: #f56c6c; font-weight: bold;">
                ₩ {{ formatPrice(row.outstandingChargeAmount) }}
                <span v-if="row.outstandingChargeWeight > 0" style="color: #909399; font-size: 0.8125rem;"> ({{ row.outstandingChargeWeight.toFixed(2) }}g)</span>
              </span>
              <span v-else style="color: #c0c4cc;">-</span>
            </template>
          </el-table-column>
          <el-table-column label="메모" min-width="140" prop="memo" />
          <el-table-column label="작업" width="120" align="center" :fixed="!isMobile ? 'right' : false">
            <template #default="{row}">
              <el-button size="small" @click="handleIssueStatement(row)">거래명세서</el-button>
            </template>
          </el-table-column>
        </base-table>
      </el-card>
    </template>

    <base-popup v-model="ledgerDetailVisible" title="정산 상세" width="900px">
      <div v-if="ledgerDetailRow" class="order-detail-expand" v-loading="paymentApplicationsLoading[ledgerDetailRow.id]">
        <el-alert v-if="ledgerDetailRow.isCancelled" type="info" :closable="false" show-icon>
          이 정산은 취소되어 적용된 제품 내역이 없습니다. 취소 시 연결된 청구 금액이 원래대로 복구되었습니다.
        </el-alert>
        <template v-else>
        <h4>적용 제품 내역</h4>
        <base-table :data="paymentApplicationsData[ledgerDetailRow.id] || []" border size="small" style="width: 100%" row-key="chargeId">
          <el-table-column label="주문번호" width="180" align="center">
            <template #default="{row: item}">
              <span class="order-link" @click="goToOrder(item.orderNo)">{{ item.orderNo }}</span>
            </template>
          </el-table-column>
          <el-table-column label="제품정보" min-width="280">
            <template #default="{row: item}">
              <div v-if="item.items && item.items.length > 0" class="product-info-list">
                <div v-for="(p, idx) in item.items.slice(0, 3)" :key="idx" class="product-info-cell">
                  <el-image :src="p.photoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 56px; height: 56px;" />
                  <div class="product-text">
                    <div class="product-name">
                      {{ p.productName || '-' }}
                      <span v-if="p.productNo" class="product-no-code">{{ p.productNo }}</span>
                    </div>
                    <div class="product-spec">
                      함량: {{ p.purity || '-' }} / 수량: {{ p.quantity }}개
                      <template v-if="p.color && p.color !== 'EMPTY'"> / 색상: {{ codeStore.codeMap[p.color] || p.color }}</template>
                      <template v-if="p.size && p.size !== 'EMPTY'"> / 사이즈: {{ p.size }}</template>
                    </div>
                    <div v-if="p.memo" class="product-memo">메모: {{ p.memo }}</div>
                  </div>
                </div>
                <div v-if="item.items.length > 3" class="product-more">+{{ item.items.length - 3 }}건 더</div>
              </div>
              <span v-else>-</span>
            </template>
          </el-table-column>
          <el-table-column label="적용 금액" width="140" align="right">
            <template #default="{row: item}">
              <span style="color: #67c23a; font-weight: bold;">₩ {{ formatPrice(item.appliedAmount) }}</span>
            </template>
          </el-table-column>
          <el-table-column label="적용 중량" width="120" align="right">
            <template #default="{row: item}">
              {{ getEffectiveAppliedWeight(item).toFixed(2) }}g
            </template>
          </el-table-column>
          <el-table-column label="작업" width="120" align="center">
            <template #default="{row: item}">
              <el-button size="small" @click="handleIssueItemStatement(item, ledgerDetailRow)">거래명세서</el-button>
            </template>
          </el-table-column>
        </base-table>

        <table v-if="paymentApplicationsData[ledgerDetailRow.id] && paymentApplicationsData[ledgerDetailRow.id].length > 0" class="purity-summary-table expand-ledger">
          <thead>
            <tr>
              <th>14K 합계(g)</th>
              <th>18K 합계(g)</th>
              <th>순금 합계(g)</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>{{ getPurityBreakdownForRow(ledgerDetailRow).p14.toFixed(2) }}</td>
              <td>{{ getPurityBreakdownForRow(ledgerDetailRow).p18.toFixed(2) }}</td>
              <td>{{ getPurityBreakdownForRow(ledgerDetailRow).pure.toFixed(2) }}</td>
            </tr>
          </tbody>
        </table>

        <table class="ledger-table expand-ledger">
          <thead>
            <tr>
              <th></th>
              <th>순금(g)</th>
              <th>공임 및 현금</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td class="ledger-label">거래 전 미지급(A)</td>
              <td class="ledger-readonly">{{ ledgerDetailBase.beforeWeight.toFixed(2) }}</td>
              <td class="ledger-readonly">₩ {{ formatPrice(ledgerDetailBase.beforeAmount) }}</td>
            </tr>
            <tr>
              <td class="ledger-label">청구(B)</td>
              <td class="ledger-readonly">{{ (ledgerDetailBase.newChargeWeight || 0).toFixed(2) }}</td>
              <td class="ledger-readonly">₩ {{ formatPrice(ledgerDetailBase.newChargeAmount || 0) }}</td>
            </tr>
            <tr>
              <td class="ledger-label">결제(C)</td>
              <td class="ledger-readonly">{{ (ledgerDetailRow.weight || 0).toFixed(2) }}</td>
              <td class="ledger-readonly">₩ {{ formatPrice(ledgerDetailRow.amount) }}</td>
            </tr>
            <tr>
              <td class="ledger-label">할인(D)</td>
              <td class="ledger-readonly">{{ (ledgerDetailRow.discountWeight || 0).toFixed(2) }}</td>
              <td class="ledger-readonly">₩ {{ formatPrice(ledgerDetailRow.discount) }}</td>
            </tr>
            <tr class="ledger-total-row">
              <td class="ledger-label">거래 후 미지급(A+B-C-D)</td>
              <td class="ledger-readonly">{{ (ledgerDetailBase.beforeWeight + (ledgerDetailBase.newChargeWeight || 0) - (ledgerDetailRow.weight || 0) - (ledgerDetailRow.discountWeight || 0)).toFixed(2) }}</td>
              <td class="ledger-readonly">₩ {{ formatPrice(ledgerDetailBase.beforeAmount + (ledgerDetailBase.newChargeAmount || 0) - (ledgerDetailRow.amount || 0) - (ledgerDetailRow.discount || 0)) }}</td>
            </tr>
          </tbody>
        </table>
        </template>
      </div>
    </base-popup>

    <batch-settle-dialog
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
import { useMobile } from '@/hooks/useMobile';
import { ref, reactive, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { getCompanySummaries, getPayableOrderHistory, getPayables, getPaymentApplications, getCompletedPayableOrders, getChargeApplications, getLedgerBefore, getPayableOrderHistorySummary } from '@/api/payable';
import { ElMessage } from 'element-plus';
import { Search, Refresh, Plus } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import BaseTable from '@/components/BaseTable/index.vue';
import BasePopup from '@/components/BasePopup/index.vue';
import BatchSettleDialog from './components/BatchSettleDialog.vue';
import SettlementHistoryFilter from './components/SettlementHistoryFilter.vue';
import OrderManualRegisterDialog from './components/OrderManualRegisterDialog.vue';
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

// A batch settlement is scoped to a single counterparty, so once one row is picked,
// only rows from that same company remain selectable - prevents silently mixing
// orders from different DCC partners into one ledger.
const isOrderRowSelectable = (row: any) => {
  if (row.remainingAmount <= 0 && row.remainingWeight <= 0) return false;
  if (selectedOrderRows.value.length === 0) return true;
  return row.logisticsCompanyId === selectedOrderRows.value[0].logisticsCompanyId;
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
  getList();
  fetchOrderHistory();
};

// A manually-registered order starts at ORDERED, same as any normal order - it only
// generates a Payable CHARGE (and shows up in this page's own lists) once it's driven
// through 물류승인/검수 to PENDING or later, so there's nothing on this page to refresh yet.
const onManualOrderSaved = () => {};

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

const handleOrderHistoryFilter = () => {
  orderHistoryQuery.page = 1;
  fetchOrderHistory();
};

// ---- DCC's own 정산받은 내역 (payment-transaction history with MFG) - ported from
// settlement-history.vue's payable branch, but view/print/cancel only, no editing:
// DCC can see and cancel what they've paid, but corrections go through MFG's side. ----

const payableList = ref<any[]>([]);
const payableTotal = ref(0);
const payableLoading = ref(false);
const payableQuery = reactive({
  page: 1,
  pageSize: 20,
  type: 'PAYMENT',
  companyId: undefined as number | undefined,
  productName: '',
  startDate: undefined as string | undefined,
  endDate: undefined as string | undefined
});

const companySummaries = ref<any[]>([]);

const fetchCompanySummariesFull = async () => {
  try {
    const res: any = await getCompanySummaries({ page: 1, pageSize: 1000 });
    companySummaries.value = res.data.items || [];
  } catch (error) {
    console.error('Failed to fetch company summaries:', error);
  }
};

const getCompanyForPaymentRow = (row: any) => {
  const found = companySummaries.value.find((c: any) => c.companyId === row.manufacturerCompanyId);
  if (found) return found;
  return {
    companyName: row.manufacturerCompanyName,
    totalOutstanding: 0,
    totalOutstandingWeight: 0,
    lastPaymentDate: null
  };
};

const fetchPayableList = async () => {
  payableLoading.value = true;
  try {
    const res: any = await getPayables({ ...payableQuery });
    payableList.value = res.data.items;
    payableTotal.value = res.data.totalCount;
  } catch (error) {
    console.error('Failed to fetch payable list:', error);
  } finally {
    payableLoading.value = false;
  }
};

// Totals for the currently filtered 거래처/기간 - mirrors settlement-history.vue's identical
// summary table (MFG's own 정산 완료 내역), so DCC sees the same 총 판매/총 결제/미수금
// breakdown for their own 정산받은 내역.
const payableSummary = reactive({
  totalChargeAmount: 0,
  totalChargeWeight: 0,
  totalPaidAmount: 0,
  totalPaidWeight: 0
});
const payableOutstandingAmount = computed(() => Math.max(0, (payableSummary.totalChargeAmount || 0) - (payableSummary.totalPaidAmount || 0)));
const payableOutstandingWeight = computed(() => Math.max(0, (payableSummary.totalChargeWeight || 0) - (payableSummary.totalPaidWeight || 0)));

const fetchPayableSummary = async () => {
  try {
    const res: any = await getPayableOrderHistorySummary({
      page: 1,
      pageSize: 1,
      companyId: payableQuery.companyId,
      startDate: payableQuery.startDate,
      endDate: payableQuery.endDate
    });
    Object.assign(payableSummary, res.data);
  } catch (error) {
    console.error('Failed to fetch payable order history summary:', error);
  }
};

const handlePayableFilter = () => {
  payableQuery.page = 1;
  if (payableViewMode.value === 'order') {
    fetchCompletedOrders();
  } else {
    fetchPayableList();
  }
  fetchPayableSummary();
};

const resetPayableQuery = () => {
  payableQuery.companyId = undefined;
  payableQuery.productName = '';
  payableQuery.startDate = undefined;
  payableQuery.endDate = undefined;
  handlePayableFilter();
};

const paymentApplicationsData = reactive<Record<number, any[]>>({});
const paymentApplicationsLoading = reactive<Record<number, boolean>>({});

const fetchPaymentApplications = async (paymentId: number) => {
  paymentApplicationsLoading[paymentId] = true;
  try {
    const res: any = await getPaymentApplications(paymentId);
    paymentApplicationsData[paymentId] = res.data;
  } catch (error) {
    console.error('Failed to fetch payment applications:', error);
  } finally {
    paymentApplicationsLoading[paymentId] = false;
  }
};

const ledgerDetailVisible = ref(false);
const ledgerDetailRow = ref<any>(null);
// getLedgerForRow approximates "before" from TODAY's company balance, which is only
// correct for the single most recent transaction - any earlier row silently bakes in
// every payment that happened after it too. This holds the real point-in-time balance
// (same getAccurateLedger/getLedgerBefore lookup the 거래명세서 flow already uses) so
// A/B here are accurate regardless of how many newer transactions exist for this row.
const ledgerDetailAccurate = ref<any>(null);

const ledgerDetailBase = computed(() => {
  if (ledgerDetailAccurate.value) return ledgerDetailAccurate.value;
  if (!ledgerDetailRow.value) return { beforeAmount: 0, beforeWeight: 0, newChargeAmount: 0, newChargeWeight: 0 };
  return { ...getLedgerForRow(ledgerDetailRow.value), newChargeAmount: 0, newChargeWeight: 0 };
});

const openLedgerDetail = async (row: any) => {
  ledgerDetailRow.value = row;
  ledgerDetailVisible.value = true;
  ledgerDetailAccurate.value = null;
  if (!row.isCancelled && !paymentApplicationsData[row.id]) {
    fetchPaymentApplications(row.id);
  }
  if (!row.isCancelled) {
    ledgerDetailAccurate.value = await getAccurateLedger(row.id, row);
  }
};

// 주문별 보기 - the same fully-paid charges as 거래별 보기, but grouped one row per order
// instead of one row per 거래번호, so a single order paid across several transactions
// doesn't read as several unrelated order rows.
const payableViewMode = ref<'transaction' | 'order'>('transaction');
const completedOrderList = ref<any[]>([]);
const completedOrderTotal = ref(0);
const completedOrderLoading = ref(false);

const fetchCompletedOrders = async () => {
  completedOrderLoading.value = true;
  try {
    const res: any = await getCompletedPayableOrders({ ...payableQuery });
    completedOrderList.value = res.data.items;
    completedOrderTotal.value = res.data.totalCount;
  } catch (error) {
    console.error('Failed to fetch completed payable orders:', error);
  } finally {
    completedOrderLoading.value = false;
  }
};

const handleViewModeChange = (mode: 'transaction' | 'order') => {
  if (mode === 'order' && completedOrderList.value.length === 0) {
    fetchCompletedOrders();
  }
};

const chargeApplicationsData = reactive<Record<number, any[]>>({});
const chargeApplicationsLoading = reactive<Record<number, boolean>>({});

const fetchChargeApplications = async (chargeId: number) => {
  chargeApplicationsLoading[chargeId] = true;
  try {
    const res: any = await getChargeApplications(chargeId);
    chargeApplicationsData[chargeId] = res.data;
  } catch (error) {
    console.error('Failed to fetch charge applications:', error);
  } finally {
    chargeApplicationsLoading[chargeId] = false;
  }
};

const handleCompletedOrderExpandChange = (row: any, expandedRows: any[]) => {
  if (expandedRows.includes(row) && !chargeApplicationsData[row.payableId]) {
    fetchChargeApplications(row.payableId);
  }
};

// p14/p18 are raw weight per purity (informational); pure is the true fine-gold total
// across ALL purities (14K/18K converted via their fine-gold ratio, 24K/PT as-is).
const getPurityBreakdownForRow = (row: any) => {
  const allItems = (paymentApplicationsData[row.id] || []).flatMap((app: any) => app.items || []);
  let p14 = 0;
  let p18 = 0;
  let pure = 0;
  allItems.forEach((item: any) => {
    const weight = (item.actualWeight || 0) * (item.quantity || 1);
    const purity = (item.purity || '').toUpperCase();
    if (purity.includes('14K')) p14 += weight;
    else if (purity.includes('18K')) p18 += weight;

    let ratio = 0;
    switch (purity) {
      case '14K': ratio = 0.6435; break;
      case '18K': ratio = 0.825; break;
      case '24K': ratio = 1.0; break;
      case 'PT': ratio = 0.95; break;
      default: ratio = purity.includes('PURE') ? 1.0 : 0;
    }
    pure += weight * ratio;
  });
  return { p14, p18, pure };
};

// Amount and weight settle a charge together (either side fully paid clears both), so a
// literal 0 here can mean "this application's own weight pool never had to touch this
// charge, because the amount side already cleared it" - not "no weight was owed". When
// the charge is now fully settled but this application recorded 0 weight, show the
// charge's own weight instead so it doesn't read as unpaid.
const getEffectiveAppliedWeight = (item: any) => {
  if (item.appliedWeight > 0) return item.appliedWeight;
  if ((item.chargeRemainingWeight || 0) <= 0 && (item.chargeRemainingAmount || 0) <= 0) {
    return item.chargeWeight || 0;
  }
  return item.appliedWeight || 0;
};

const getLedgerForRow = (record: any) => {
  const company = getCompanyForPaymentRow(record);
  const afterAmount = company.totalOutstanding || 0;
  const afterWeight = company.totalOutstandingWeight || 0;
  const beforeAmount = afterAmount + (record.amount || 0) + (record.discount || 0);
  const beforeWeight = afterWeight + (record.weight || 0) + (record.discountWeight || 0);
  return { company, afterAmount, afterWeight, beforeAmount, beforeWeight };
};

// getLedgerForRow's beforeAmount/beforeWeight only line up with reality for the single most
// recent payment - company.totalOutstanding is *today's* balance, so for any older record it
// silently bakes in every charge/payment that happened afterward too. This fetches the true
// point-in-time balance from the backend (which walks the full chronological ledger) and
// derives afterAmount/afterWeight from it, for statements where historical accuracy matters.
const getAccurateLedger = async (anchorId: number | undefined, record: any) => {
  const fallback = { ...getLedgerForRow(record), newChargeAmount: 0, newChargeWeight: 0 };
  if (!anchorId) return fallback;
  try {
    const res: any = await getLedgerBefore(anchorId);
    const beforeAmount = res.data.beforeAmount ?? fallback.beforeAmount;
    const beforeWeight = res.data.beforeWeight ?? fallback.beforeWeight;
    const newChargeAmount = res.data.newChargeAmount || 0;
    const newChargeWeight = res.data.newChargeWeight || 0;
    return {
      ...fallback,
      beforeAmount,
      beforeWeight,
      newChargeAmount,
      newChargeWeight,
      afterAmount: beforeAmount + newChargeAmount - (record.amount || 0) - (record.discount || 0),
      afterWeight: beforeWeight + newChargeWeight - (record.weight || 0) - (record.discountWeight || 0)
    };
  } catch (error) {
    console.error('Failed to fetch accurate ledger balance, falling back to approximation:', error);
    return fallback;
  }
};

// Shared "임가공 거래 명세서" print template - same shape as settlement-history.vue's, so
// every printed statement in the app looks like one document type.
const paddedStatementItems = (items: any[], minCount = 6) => {
  const result = [...items];
  while (result.length < minCount) {
    result.push({ isDummy: true });
  }
  return result;
};

const buildStatementItemRows = (items: any[]) => {
  return paddedStatementItems(items || [], 6).map((item: any) => {
    if (item.isDummy) {
      return `<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>`;
    }
    const cost = ((item.materialCost || 0) + (item.laborCost || 0)) * (item.quantity || 1);
    return `
      <tr>
        <td align="center">${item.productNo || '-'}</td>
        <td><div class="item-info-flex">${item.photoUrl ? `<img src="${item.photoUrl}" class="item-img" />` : ''}<span class="item-name">${item.productName || '-'}</span></div></td>
        <td align="center">${codeStore.codeMap[item.purity] || item.purity || '-'}</td>
        <td align="right">${(item.actualWeight || 0).toFixed(3)}g</td>
        <td align="center">${item.quantity || 1}개</td>
        <td align="right">${formatPrice(cost)}</td>
      </tr>
    `;
  }).join('');
};

// 14K/18K raw totals plus the true fine-gold (24K-converted) total across every purity -
// same computation as getPurityBreakdownForRow, applied to a statement's own item list.
const buildStatementPurityRows = (items: any[]) => {
  let p14 = 0;
  let p18 = 0;
  let pure = 0;
  (items || []).forEach((item: any) => {
    const weight = (item.actualWeight || 0) * (item.quantity || 1);
    const purity = (item.purity || '').toUpperCase();
    if (purity.includes('14K')) p14 += weight;
    else if (purity.includes('18K')) p18 += weight;

    let ratio = 0;
    switch (purity) {
      case '14K': ratio = 0.6435; break;
      case '18K': ratio = 0.825; break;
      case '24K': ratio = 1.0; break;
      case 'PT': ratio = 0.95; break;
      default: ratio = purity.includes('PURE') ? 1.0 : 0;
    }
    pure += weight * ratio;
  });
  return `<tr><td align="right">${p14.toFixed(2)}g</td><td align="right">${p18.toFixed(2)}g</td><td align="right">${pure.toFixed(2)}g</td></tr>`;
};

const buildStatementBalanceRows = (ledger: any, record: any) => {
  const { company, beforeAmount, beforeWeight, afterAmount, afterWeight, newChargeAmount, newChargeWeight } = ledger;
  return `
    <tr><td class="row-label">최근 결제</td><td></td><td></td><td align="center" style="font-size: 9px;">${company.lastPaymentDate ? formatDate(company.lastPaymentDate) : '-'}</td></tr>
    <tr><td class="row-label">거래 전 미지급(A)</td><td align="right">${beforeWeight.toFixed(2)}</td><td align="right">${formatPrice(beforeAmount)}</td><td></td></tr>
    <tr><td class="row-label">판매(B)</td><td align="right">${(newChargeWeight || 0).toFixed(2)}</td><td align="right">${formatPrice(newChargeAmount || 0)}</td><td></td></tr>
    <tr><td class="row-label">결제(C)</td><td align="right">${(record.weight || 0).toFixed(2)}</td><td align="right">${formatPrice(record.amount || 0)}</td><td></td></tr>
    <tr><td class="row-label">할인(D)</td><td align="right">${(record.discountWeight || 0).toFixed(2)}</td><td align="right">${formatPrice(record.discount || 0)}</td><td></td></tr>
    <tr class="after-balance-row"><td class="row-label"><strong>거래 후 미지급<br/>(A+B-C-D)</strong></td><td align="right"><strong>${afterWeight.toFixed(2)}</strong></td><td align="right"><strong>${formatPrice(afterAmount)}</strong></td><td></td></tr>
  `;
};

const buildStatementCopy = (copyLabel: string, opts: { companyTag: string; supplierName: string; date: string; transactionNo: string | number; itemRows: string; purityRows: string; balanceRows: string }) => `
  <div class="receipt-copy-block">
    <table class="receipt-header-table">
      <tr>
        <td class="header-title-cell">
          <span class="company-tag">[${opts.companyTag}]</span>
          <span class="main-title">임가공 거래 명세서</span>
          <span class="copy-badge ${copyLabel === '고객용' ? 'blue-badge' : ''}">(${copyLabel})</span>
        </td>
        <td class="supplier-info-cell" rowspan="2">
          <div>공급자: ${opts.supplierName}</div>
          <div>전 화: 051-633-1116</div>
          <div>팩 스: -</div>
        </td>
      </tr>
      <tr>
        <td class="header-meta-cell">
          <span>일자: ${opts.date}</span>
          <span class="meta-separator">|</span>
          <span>거래No: ${opts.transactionNo}</span>
        </td>
      </tr>
    </table>
    <table class="receipt-items-table">
      <thead><tr><th width="12%">No</th><th width="42%">주문내용</th><th width="12%">함량</th><th width="13%">실중량</th><th width="10%">주문수량</th><th width="11%">공임비</th></tr></thead>
      <tbody>${opts.itemRows}</tbody>
    </table>
    <table class="receipt-purity-table">
      <thead><tr><th width="33%">14K 합계(g)</th><th width="33%">18K 합계(g)</th><th width="34%">순금(24K) 합계(g)</th></tr></thead>
      <tbody>${opts.purityRows}</tbody>
    </table>
    <table class="receipt-balance-table">
      <thead><tr><th width="30%"></th><th width="22%">순금(g)</th><th width="24%">공임 및 현금</th><th width="24%">금액 합계</th></tr></thead>
      <tbody>${opts.balanceRows}</tbody>
    </table>
  </div>
`;

const statementPrintStyles = `
  @page { size: A4 landscape; margin: 8mm; }
  body { font-family: 'Malgun Gothic', 'Noto Sans KR', sans-serif; color: #111; margin: 0; padding: 5px; background: #fff; }
  .receipt-double-container { display: flex; gap: 6mm; width: 100%; box-sizing: border-box; }
  .receipt-copy-block { flex: 1; min-width: 0; border: 1.5px solid #222; padding: 6px; background: #fff; box-sizing: border-box; }
  .receipt-header-table { width: 100%; border-collapse: collapse; margin-bottom: 6px; border: 1px solid #333; }
  .receipt-header-table td { padding: 4px 6px; border: 1px solid #333; vertical-align: top; }
  .header-title-cell { font-size: 12px; font-weight: bold; }
  .company-tag { color: #333; margin-right: 4px; }
  .main-title { font-size: 13px; font-weight: bold; letter-spacing: 0.5px; }
  .copy-badge { font-weight: bold; margin-left: 4px; color: #222; }
  .copy-badge.blue-badge { color: #1d4ed8; }
  .header-meta-cell { font-size: 10px; color: #444; background: #fafafa; }
  .meta-separator { margin: 0 4px; color: #aaa; }
  .supplier-info-cell { font-size: 10px; text-align: left; width: 35%; background: #fff; line-height: 1.3; }
  .receipt-items-table { width: 100%; border-collapse: collapse; margin-bottom: 8px; border: 1px solid #333; font-size: 10px; }
  .receipt-items-table th { background-color: #f2f2f2; border: 1px solid #333; padding: 4px 2px; font-weight: bold; text-align: center; color: #222; }
  .receipt-items-table td { border: 1px solid #333; padding: 3px 5px; vertical-align: middle; height: 25px; box-sizing: border-box; }
  .item-info-flex { display: flex; align-items: center; gap: 4px; }
  .item-img { width: 22px; height: 22px; object-fit: cover; border-radius: 2px; border: 1px solid #ccc; }
  .item-name { font-weight: 600; }
  .receipt-purity-table { width: 100%; border-collapse: collapse; margin-bottom: 8px; border: 1px solid #333; font-size: 10px; }
  .receipt-purity-table th { background-color: #f2f2f2; border: 1px solid #333; padding: 4px; font-weight: bold; text-align: center; color: #222; }
  .receipt-purity-table td { border: 1px solid #333; padding: 3px 5px; }
  .receipt-balance-table { width: 100%; border-collapse: collapse; border: 1px solid #333; font-size: 10px; }
  .receipt-balance-table th { background-color: #f2f2f2; border: 1px solid #333; padding: 4px; font-weight: bold; text-align: center; color: #222; }
  .receipt-balance-table td { border: 1px solid #333; padding: 3px 5px; }
  .receipt-balance-table .row-label { background-color: #fafafa; font-weight: 600; text-align: left; }
  .receipt-balance-table .after-balance-row td { background-color: #f8f9fa; font-weight: bold; }
`;

const openStatementPrintWindow = (title: string, bodyHtml: string) => {
  const printWindow = window.open('', '_blank');
  if (!printWindow) return;
  printWindow.document.write(`
    <html>
      <head><title>${title}</title><style>${statementPrintStyles}</style></head>
      <body>
        <div class="receipt-double-container">${bodyHtml}</div>
        <script>window.onload = () => { window.print(); setTimeout(() => window.close(), 500); };<\/script>
      </body>
    </html>
  `);
  printWindow.document.close();
};

const handleIssueStatement = async (record: any) => {
  if (!paymentApplicationsData[record.id]) {
    await fetchPaymentApplications(record.id);
  }
  const allItems = (paymentApplicationsData[record.id] || []).flatMap((app: any) => app.items || []);
  const ledger = await getAccurateLedger(record.id, record);
  const supplierName = ledger.company.companyName || '-';
  const payerName = userStore.companyName || '-';

  const itemRows = buildStatementItemRows(allItems);
  const purityRows = buildStatementPurityRows(allItems);
  const balanceRows = buildStatementBalanceRows(ledger, record);

  const bodyHtml = ['고객용', '보관용'].map((label) => buildStatementCopy(label, {
    companyTag: payerName,
    supplierName,
    date: formatDate(record.createdAt),
    transactionNo: record.id,
    itemRows,
    purityRows,
    balanceRows
  })).join('');

  openStatementPrintWindow(`정산 명세서 - ${payerName}`, bodyHtml);
};

const handleIssueItemStatement = async (item: any, record: any) => {
  // 결제(C)/할인(D) must reflect only what THIS order's application received, not the full
  // payment's totals - a single payment can cover multiple orders, and this receipt is
  // scoped to just one of them. The ledger-before lookup still anchors on the payment's own
  // chronological position (record.id), since that's when the money actually moved.
  const itemRecord = {
    amount: item.appliedAmount || 0,
    weight: getEffectiveAppliedWeight(item),
    discount: 0,
    discountWeight: 0,
    createdAt: record.createdAt
  };
  const ledger = await getAccurateLedger(record.id, itemRecord);
  const supplierName = ledger.company.companyName || '-';
  const payerName = userStore.companyName || '-';

  const itemRows = buildStatementItemRows(item.items || []);
  const purityRows = buildStatementPurityRows(item.items || []);
  const balanceRows = buildStatementBalanceRows(ledger, itemRecord);

  const bodyHtml = ['고객용', '보관용'].map((label) => buildStatementCopy(label, {
    companyTag: payerName,
    supplierName,
    date: formatDate(record.createdAt),
    transactionNo: item.chargeId,
    itemRows,
    purityRows,
    balanceRows
  })).join('');

  openStatementPrintWindow(`정산 영수증 - ${item.orderNo}`, bodyHtml);
};

// 주문별 보기 row's own 거래명세서 - the row already carries its full item list and its
// paid amount/weight (== charge amount/weight, since only fully-settled orders show up
// here), so it reuses the same ledger/statement builders without a separate lookup.
const handleIssueOrderStatement = async (row: any) => {
  const pseudoRecord = { ...row, amount: row.paidAmount, weight: row.paidWeight, discount: 0, discountWeight: 0 };
  const ledger = await getAccurateLedger(row.payableId, pseudoRecord);
  const supplierName = ledger.company.companyName || '-';
  const payerName = userStore.companyName || '-';

  const itemRows = buildStatementItemRows(row.items || []);
  const purityRows = buildStatementPurityRows(row.items || []);
  const balanceRows = buildStatementBalanceRows(ledger, pseudoRecord);

  const bodyHtml = ['고객용', '보관용'].map((label) => buildStatementCopy(label, {
    companyTag: payerName,
    supplierName,
    date: formatDate(row.orderDate),
    transactionNo: row.payableId,
    itemRows,
    purityRows,
    balanceRows
  })).join('');

  openStatementPrintWindow(`정산 명세서 - ${row.orderNo}`, bodyHtml);
};

const printCombinedPayableStatement = async () => {
  if (!payableList.value || payableList.value.length === 0) {
    ElMessage.warning('출력할 거래 내역이 없습니다.');
    return;
  }

  // payableList is whatever page is currently on screen (20 by default) - a "combined"
  // statement has to cover every record matching the active filter/date range, not just
  // the visible page, so re-fetch the full unpaginated set instead of reusing payableList.
  let combinedList = payableList.value;
  try {
    const fullRes: any = await getPayables({ ...payableQuery, page: 1, pageSize: 10000 });
    combinedList = fullRes.data.items || payableList.value;
  } catch (error) {
    console.error('Failed to fetch full payable list for combined statement, falling back to current page:', error);
  }

  await Promise.all(combinedList.map(async (p: any) => {
    if (!paymentApplicationsData[p.id]) {
      await fetchPaymentApplications(p.id);
    }
  }));

  const company = getCompanyForPaymentRow(combinedList[0]);

  let totalAmount = 0;
  let totalWeight = 0;
  let totalDiscount = 0;
  let totalDiscountWeight = 0;
  const allItems: any[] = [];

  combinedList.forEach((p: any) => {
    totalAmount += p.amount || 0;
    totalWeight += p.weight || 0;
    totalDiscount += p.discount || 0;
    totalDiscountWeight += p.discountWeight || 0;
    (paymentApplicationsData[p.id] || []).forEach((app: any) => {
      allItems.push(...(app.items || []));
    });
  });

  const pseudoRecord = { weight: totalWeight, amount: totalAmount, discountWeight: totalDiscountWeight, discount: totalDiscount };

  // "Before" for a combined statement means before the *earliest* payment in the batch -
  // every payment after that is already folded into the combined C/D totals above.
  const earliestRecord = [...combinedList].sort((a: any, b: any) => {
    const dateDiff = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
    return dateDiff !== 0 ? dateDiff : a.id - b.id;
  })[0];

  let beforeAmount = (company.totalOutstanding || 0) + totalAmount + totalDiscount;
  let beforeWeight = (company.totalOutstandingWeight || 0) + totalWeight + totalDiscountWeight;
  let newChargeAmount = 0;
  let newChargeWeight = 0;
  if (earliestRecord?.id) {
    try {
      const res: any = await getLedgerBefore(earliestRecord.id);
      beforeAmount = res.data.beforeAmount;
      beforeWeight = res.data.beforeWeight;
      newChargeAmount = res.data.newChargeAmount || 0;
      newChargeWeight = res.data.newChargeWeight || 0;
    } catch (error) {
      console.error('Failed to fetch accurate ledger balance, falling back to approximation:', error);
    }
  }
  const ledger = {
    company,
    beforeAmount,
    beforeWeight,
    newChargeAmount,
    newChargeWeight,
    afterAmount: beforeAmount + newChargeAmount - totalAmount - totalDiscount,
    afterWeight: beforeWeight + newChargeWeight - totalWeight - totalDiscountWeight
  };

  const supplierName = company.companyName || '-';
  const payerName = userStore.companyName || '-';

  const dateTitle = payableQuery.startDate && payableQuery.endDate
    ? (payableQuery.startDate === payableQuery.endDate ? payableQuery.startDate : `${payableQuery.startDate} ~ ${payableQuery.endDate}`)
    : '전체 기간';

  const itemRows = buildStatementItemRows(allItems);
  const purityRows = buildStatementPurityRows(allItems);
  const balanceRows = buildStatementBalanceRows(ledger, pseudoRecord);

  const bodyHtml = ['고객용', '보관용'].map((label) => buildStatementCopy(label, {
    companyTag: payerName,
    supplierName,
    date: dateTitle,
    transactionNo: `${combinedList.length}건`,
    itemRows,
    purityRows,
    balanceRows
  })).join('');

  openStatementPrintWindow(`일괄 정산 명세서 - ${payerName}`, bodyHtml);
};

onMounted(() => {
  codeStore.fetchCodes();
  if (isLogistics.value) {
    fetchCompanySummariesFull();
    fetchPayableList();
    fetchPayableSummary();
  } else {
    getList();
    fetchOrderHistory();
  }
});
</script>

<style lang="scss" scoped>
.filter-card {
  margin-bottom: 1.25rem;
}
.order-history-summary-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
  margin-top: 1.25rem;
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
.pagination-container {
  margin-top: 1.25rem;
  display: flex;
  justify-content: center;
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
.order-detail-expand {
  padding: 1.25rem;
}
.order-detail-expand h4 {
  margin-top: 0;
  margin-bottom: 0.9375rem;
}
.ledger-table,
.purity-summary-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}
.ledger-table th, .ledger-table td,
.purity-summary-table th, .purity-summary-table td {
  border: 1px solid #ebeef5;
  padding: 0.5rem;
  text-align: center;
}
.ledger-table th,
.purity-summary-table th {
  background: #f5f7fa;
  font-weight: 600;
}
.ledger-label {
  text-align: left;
  font-weight: 600;
  background: #fafafa;
  white-space: nowrap;
}
.ledger-readonly {
  color: #606266;
}
.ledger-total-row {
  font-weight: bold;
}
.ledger-total-row .ledger-readonly {
  color: #f56c6c;
}
.expand-ledger {
  margin-top: 1.25rem;
}
</style>
