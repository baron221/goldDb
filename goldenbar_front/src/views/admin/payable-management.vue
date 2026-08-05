<template>
<div class="payable-management-container app-container">
    <el-card shadow="never" class="filter-card">
      <div style="font-size: 1.1rem; font-weight: bold; margin-bottom: 0.9375rem;">
        {{ isLogistics ? '정산처리 내역 (공장에 지급할 금액)' : '정산받은 내역 (물류로부터 받을 금액)' }}
      </div>
      <el-form :inline="true" :model="listQuery" class="demo-form-inline">
        <el-form-item label="거래처 검색">
          <el-input v-model="listQuery.search" placeholder="업체명" clearable @keyup.enter="handleFilter" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :icon="Search" @click="handleFilter">검색</el-button>
          <el-button :icon="Refresh" @click="resetQuery">초기화</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card shadow="never" style="margin-top: 1.25rem;">
      <base-table
        v-loading="listLoading"
        :data="list"
        border
        fit
        highlight-current-row
        style="width: 100%"
        row-key="companyId"
      >
        <el-table-column type="expand">
          <template #default="{row}">
            <div class="history-detail-expand">
              <div class="expand-header" style="display: flex; justify-content: space-between; align-items: center;">
                <h4>[{{ row.companyName }}] 상세 거래 내역</h4>
                <div style="display: flex; gap: 0.5rem;">
                  <el-button size="small" type="success" @click="openPaymentDialog(row)">정산 처리</el-button>
                  <el-button size="small" type="primary" plain @click="fetchHistory(row.companyId)">새로고침</el-button>
                </div>
              </div>
              <base-table
                v-loading="historyLoading[row.companyId]"
                :data="(historyData[row.companyId] || []).filter(r => !r.isCancelled)"
                border
                size="small"
                style="width: 100%; margin-top: 0.625rem;"
              >
                <el-table-column prop="createdAt" label="발생일시" width="160" align="center" :excel-formatter="(row) => formatDate(row.createdAt)">
                  <template #default="item">
                    <span>{{ formatDate(item.row.createdAt) }}</span>
                  </template>
                </el-table-column>
                <el-table-column prop="type" label="구분" width="120" align="center">
                  <template #default="item">
                    <el-tag :type="item.row.type === 'CHARGE' ? 'danger' : (item.row.isCancelled ? 'info' : 'success')">
                      {{ item.row.type === 'CHARGE' ? '청구' : '정산' }}
                    </el-tag>
                    <el-tag v-if="item.row.isCancelled" type="info" size="small" style="margin-left: 0.25rem;">취소됨</el-tag>
                  </template>
                </el-table-column>
                <el-table-column prop="orderNo" label="주문번호" width="180" align="center">
                  <template #default="item">
                    <el-button v-if="item.row.orderNo" type="primary" link @click="goToOrder(item.row.orderNo)">
                      {{ item.row.orderNo }}
                    </el-button>
                    <span v-else>-</span>
                  </template>
                </el-table-column>
                <el-table-column prop="amount" label="금액" width="130" align="right">
                  <template #default="item">
                    <span :style="{ color: item.row.type === 'CHARGE' ? '#f56c6c' : '#67c23a', fontWeight: 'bold' }">
                      {{ item.row.type === 'CHARGE' ? '+' : '-' }} ₩ {{ formatPrice(item.row.amount) }}
                    </span>
                  </template>
                </el-table-column>
                <el-table-column prop="remainingAmount" label="남은 미지급" width="130" align="right">
                  <template #default="item">
                    <span v-if="item.row.type === 'CHARGE'" :style="{ color: item.row.remainingAmount > 0 ? '#f56c6c' : '#67c23a', fontWeight: 'bold' }">
                      ₩ {{ formatPrice(item.row.remainingAmount) }}
                    </span>
                    <span v-else>-</span>
                  </template>
                </el-table-column>
                <el-table-column prop="memo" label="메모" min-width="200">
                  <template #default="item">
                    <span>{{ item.row.memo }}</span>
                  </template>
                </el-table-column>
                <el-table-column label="작업" width="260" align="center" fixed="right">
                  <template #default="item">
                    <div v-if="item.row.type === 'PAYMENT'" style="display: flex; gap: 0.375rem; justify-content: center;">
                      <el-button size="small" @click="handlePrintReceipt(item.row, row)">영수증 출력</el-button>
                      <template v-if="!item.row.isCancelled">
                        <el-button size="small" type="warning" @click="openEditDialog(item.row)">수정</el-button>
                        <el-button size="small" type="danger" @click="handleCancelPayable(item.row, row.companyId)">정산취소</el-button>
                      </template>
                    </div>
                    <div v-else-if="item.row.type === 'CHARGE'" style="display: flex; gap: 0.375rem; justify-content: center; align-items: center;">
                      <el-button v-if="item.row.remainingAmount > 0 || item.row.remainingWeight > 0" size="small" type="success" @click="openOrderPaymentDialog(item.row, row)">주문 정산</el-button>
                      <template v-else>
                        <el-tag type="success" size="small">정산완료</el-tag>
                        <el-button size="small" type="success" plain @click="openOrderPaymentDialog(item.row, row)">추가 정산</el-button>
                      </template>
                    </div>
                  </template>
                </el-table-column>
              </base-table>

              <div v-if="historyTotal[row.companyId] > historyQuery.pageSize" class="pagination-container" style="margin-top: 0.625rem;">
                <el-pagination
                  v-model:current-page="historyQuery.page"
                  v-model:page-size="historyQuery.pageSize"
                  :total="historyTotal[row.companyId] || 0"
                  layout="total, prev, pager, next"
                  @current-change="() => fetchHistory(row.companyId)"
                />
              </div>
            </div>
          </template>
        </el-table-column>

        <el-table-column label="거래처" prop="companyName" min-width="200" align="center" />
        <el-table-column prop="totalOutstanding" label="총 미지급 잔액" width="200" align="right">
          <template #default="{row}">
            <span style="font-weight: bold; color: #f56c6c; font-size: 1rem;">
              ₩ {{ formatPrice(row.totalOutstanding) }}
            </span>
          </template>
        </el-table-column>
        <el-table-column v-if="isLogistics" :label="$t('common.action')" width="150" align="center" :fixed="!isMobile ? 'right' : false">
          <template #default="{row}">
            <el-button type="success" size="small" @click="openPaymentDialog(row)">정산 처리</el-button>
          </template>
        </el-table-column>
      </base-table>

      <div class="pagination-container">
        <el-pagination
          v-model:current-page="listQuery.page"
          v-model:page-size="listQuery.pageSize"
          :total="total"
          :page-sizes="[10, 20, 30, 50]"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="getList"
          @current-change="getList"
        />
      </div>
    </el-card>

    <el-card shadow="never" class="filter-card" style="margin-top: 1.25rem;">
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

      <base-table
        v-loading="orderHistoryLoading"
        :data="orderHistoryList"
        :total="orderHistoryTotal"
        v-model:page="orderHistoryQuery.page"
        v-model:page-size="orderHistoryQuery.pageSize"
        border
        style="width: 100%; margin-top: 1.25rem;"
        @change="fetchOrderHistory"
      >
        <el-table-column label="주문번호" width="200" align="center">
          <template #default="{row}">
            <span class="order-link" @click="goToOrder(row.orderNo)">{{ row.orderNo }}</span>
          </template>
        </el-table-column>
        <el-table-column label="제품정보" min-width="220">
          <template #default="{row}">
            <div v-if="row.productName" class="product-info-cell">
              <el-image :src="row.productPhotoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 40px; height: 40px;" />
              <div class="product-text">
                <div class="product-name">
                  {{ row.productName }}
                  <el-tag v-if="row.productItemCount > 1" size="small" type="info" effect="plain" style="margin-left: 0.3125rem;">+{{ row.productItemCount - 1 }}</el-tag>
                </div>
              </div>
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
      </base-table>
    </el-card>

    <payment-dialog
      v-model="paymentDialogVisible"
      :company="currentCompany"
      :order-id="selectedOrderId"
      :order-no="selectedOrderNo"
      @saved="onPaymentSaved"
    />

    <payable-edit-dialog
      v-model="editDialogVisible"
      :record="editingRecord"
      @saved="onEditSaved"
    />
  </div>
</template>

<script setup lang="ts">
import { useMobile } from '@/hooks/useMobile';
import { ref, reactive, computed, watch, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { getCompanySummaries, getPayables, cancelPayable, getPayableOrderHistory, getPayableOrderHistorySummary } from '@/api/payable';
import { ElMessage, ElMessageBox } from 'element-plus';
import { Search, Refresh } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import BaseTable from '@/components/BaseTable/index.vue';
import PaymentDialog from './components/PaymentDialog.vue';
import PayableEditDialog from './components/PayableEditDialog.vue';
import useUserStore from '@/store/modules/user';

const { isMobile } = useMobile();
const userStore = useUserStore();
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

const historyData = reactive<Record<number, any[]>>({});
const historyLoading = reactive<Record<number, boolean>>({});
const historyTotal = reactive<Record<number, number>>({});
const historyQuery = reactive({
  page: 1,
  pageSize: 10
});

const paymentDialogVisible = ref(false);
const currentCompany = ref<any>(null);
const selectedOrderId = ref<number | undefined>(undefined);
const selectedOrderNo = ref<string | undefined>(undefined);

const openOrderPaymentDialog = (chargeRecord: any, companyRow: any) => {
  currentCompany.value = {
    ...companyRow,
    totalOutstanding: chargeRecord.remainingAmount,
    totalOutstandingWeight: chargeRecord.remainingWeight
  };
  selectedOrderId.value = chargeRecord.orderId;
  selectedOrderNo.value = chargeRecord.orderNo;
  paymentDialogVisible.value = true;
};

const openPaymentDialog = (row: any) => {
  currentCompany.value = row;
  selectedOrderId.value = undefined;
  selectedOrderNo.value = undefined;
  paymentDialogVisible.value = true;
};

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

const handleFilter = () => {
  listQuery.page = 1;
  getList();
};

const resetQuery = () => {
  listQuery.search = '';
  handleFilter();
};

const fetchHistory = async (companyId: number) => {
  historyLoading[companyId] = true;
  try {
    const res = await getPayables({
      companyId,
      page: historyQuery.page,
      pageSize: historyQuery.pageSize
    });
    historyData[companyId] = res.data.items;
    historyTotal[companyId] = res.data.totalCount;
  } catch (error) {
    console.error('Failed to fetch history:', error);
  } finally {
    historyLoading[companyId] = false;
  }
};


const onPaymentSaved = () => {
  getList();
  if (currentCompany.value && historyData[currentCompany.value.companyId]) {
    fetchHistory(currentCompany.value.companyId);
  }
};

const editDialogVisible = ref(false);
const editingRecord = ref<any>(null);
const editingCompanyId = ref<number | null>(null);

const openEditDialog = (record: any) => {
  editingRecord.value = record;
  editingCompanyId.value = record.manufacturerCompanyId;
  editDialogVisible.value = true;
};

const onEditSaved = () => {
  getList();
  if (editingCompanyId.value && historyData[editingCompanyId.value]) {
    fetchHistory(editingCompanyId.value);
  }
};

const handleCancelPayable = (record: any, companyId: number) => {
  ElMessageBox.confirm('이 정산 내역을 취소하시겠습니까? 관련 미지급액이 다시 복구됩니다.', '정산 취소', {
    confirmButtonText: '취소 처리',
    cancelButtonText: '닫기',
    type: 'warning'
  }).then(async () => {
    try {
      await cancelPayable(record.id);
      ElMessage.success('정산이 취소되었습니다.');
      getList();
      fetchHistory(companyId);
    } catch (error) {
      console.error('Failed to cancel payable:', error);
      ElMessage.error('취소에 실패했습니다.');
    }
  }).catch(() => {});
};

const handlePrintReceipt = (record: any, company: any) => {
  const printWindow = window.open('', '_blank');
  if (!printWindow) return;

  // The payable record itself has no stored before/after snapshot (it's applied across
  // however many outstanding charges it covers), so this shows the company's current
  // outstanding balance as the "after" figure - accurate as of now.
  const afterAmount = company.totalOutstanding || 0;
  const afterWeight = company.totalOutstandingWeight || 0;
  const beforeAmount = afterAmount + (record.amount || 0) + (record.discount || 0);
  const beforeWeight = afterWeight + (record.weight || 0);

  // 공급자 is always the manufacturer regardless of which side is viewing.
  const supplierName = isLogistics.value ? company.companyName : userStore.companyName || '-';
  const payerName = isLogistics.value ? userStore.companyName || '-' : company.companyName;

  const ledgerRows = `
    <tr><td class="label">최근결제</td><td>${company.lastPaymentDate ? formatDate(company.lastPaymentDate) : '-'}</td><td></td><td></td></tr>
    <tr><td class="label">거래 전 미지급(A)</td><td>${beforeWeight.toFixed(2)}</td><td>${formatPrice(beforeAmount)}</td><td></td></tr>
    <tr><td class="label">청구(B)</td><td>0.00</td><td>0</td><td></td></tr>
    <tr><td class="label">결제(C)</td><td>${(record.weight || 0).toFixed(2)}</td><td>${formatPrice(record.amount || 0)}</td><td></td></tr>
    <tr><td class="label">할인(D)</td><td>0.00</td><td>${formatPrice(record.discount || 0)}</td><td></td></tr>
    <tr><td class="label"><strong>거래 후 미지급(A+B-C-D)</strong></td><td><strong>${afterWeight.toFixed(2)}</strong></td><td><strong>${formatPrice(afterAmount)}</strong></td><td></td></tr>
  `;

  const statementBlock = (copyLabel: string) => `
    <div class="statement-copy">
      <div class="statement-title">[${payerName}] 정산 명세서(${copyLabel})</div>
      <div class="statement-meta">
        <span>공급자: ${supplierName}</span>
        <span>일자: ${formatDate(record.createdAt)}</span>
        <span>거래No: ${record.id}</span>
      </div>
      <table>
        <thead><tr><th></th><th>순금(g)</th><th>공임 및 현금</th><th>금액 합계</th></tr></thead>
        <tbody>${ledgerRows}</tbody>
      </table>
    </div>
  `;

  const html = `
    <html>
      <head>
        <title>정산 명세서 - ${payerName}</title>
        <style>
          body { font-family: 'Malgun Gothic', sans-serif; padding: 10mm; }
          .statements-row { display: flex; gap: 10mm; }
          .statement-copy { flex: 1; min-width: 0; }
          .statement-title { font-weight: bold; font-size: 1rem; margin-bottom: 8px; }
          .statement-meta { display: flex; justify-content: space-between; font-size: 0.85rem; color: #333; margin-bottom: 8px; }
          table { width: 100%; border-collapse: collapse; }
          th, td { border: 1px solid #333; padding: 6px; text-align: center; font-size: 0.85rem; }
          th { background: #f5f5f5; }
          td.label { text-align: left; background: #fafafa; font-weight: 600; }
          .footer-note { margin-top: 16px; text-align: center; font-size: 0.85rem; color: #333; }
        </style>
      </head>
      <body>
        <div class="statements-row">
          ${statementBlock('공급자용')}
          ${statementBlock('보관용')}
        </div>
        <p class="footer-note">상기 대여 및 영수(미수는 대여로 함)합니다. (VAT 별도)</p>
        <p style="margin-top: 10px; font-size: 0.85rem;">메모: ${record.memo || '-'}</p>
        <script>window.onload = () => { window.print(); setTimeout(() => window.close(), 500); };<\/script>
      </body>
    </html>
  `;

  printWindow.document.write(html);
  printWindow.document.close();
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

const handlePrintChargeReceipt = (charge: any, company: any) => {
  const printWindow = window.open('', '_blank');
  if (!printWindow) return;

  const counterpartyLabel = isLogistics.value ? '공장' : '물류';
  const counterpartyName = company.companyName;

  const html = `
    <html>
      <head>
        <title>정산 영수증 - ${charge.orderNo || charge.id}</title>
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
            <tr><td class="label">주문번호</td><td>${charge.orderNo || '-'}</td></tr>
            <tr><td class="label">${counterpartyLabel}</td><td>${counterpartyName || '-'}</td></tr>
            <tr><td class="label">발생일시</td><td>${formatDate(charge.createdAt)}</td></tr>
            <tr><td class="label">청구 금액</td><td>₩ ${formatPrice(charge.amount)} (${(charge.weight || 0).toFixed(2)}g)</td></tr>
            <tr><td class="label">남은 미지급</td><td>₩ ${formatPrice(charge.remainingAmount)} (${(charge.remainingWeight || 0).toFixed(2)}g)</td></tr>
            <tr><td class="label">메모</td><td>${charge.memo || '-'}</td></tr>
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
  getList();
  fetchOrderHistory();
  fetchOrderHistorySummary();
});
</script>

<style lang="scss" scoped>
.filter-card {
  margin-bottom: 1.25rem;
}
.history-detail-expand {
  padding: 0.9375rem 1.875rem;
  background-color: #fafafa;
  border-radius: 2px;

  .expand-header {
    display: flex;
    justify-content: space-between;
    align-items: center;

    h4 {
      margin: 0;
      color: #333;
    }
  }
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
</style>
