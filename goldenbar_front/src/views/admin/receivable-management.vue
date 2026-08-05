<template>
<div class="receivable-management-container app-container">
    <el-card shadow="never" class="filter-card">
      <el-form :inline="true" :model="listQuery" class="demo-form-inline">
        <el-form-item label="사용자 검색">
          <el-input v-model="listQuery.search" placeholder="이름 또는 아이디" clearable @keyup.enter="handleFilter" />
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
        row-key="userId"
      >

        <el-table-column type="expand">
          <template #default="{row}">
            <div class="history-detail-expand">
              <div class="expand-header">
                <h4>[{{ row.userDisplayName }}] 상세 거래 내역</h4>
                <el-button size="small" type="primary" plain @click="fetchHistory(row.userId)">새로고침</el-button>
              </div>
              <base-table
                v-loading="historyLoading[row.userId]"
                :data="(historyData[row.userId] || []).filter(r => !r.isCancelled)"
                border
                size="small"
                style="width: 100%; margin-top: 0.625rem;"
              >
                <el-table-column prop="createdAt" label="발생일시" width="160" align="center" :excel-formatter="(row) => formatDate(row.createdAt)">
                  <template #default="item">
                    <span>{{ formatDate(item.row.createdAt) }}</span>
                  </template>
                </el-table-column>
                <el-table-column prop="type" label="구분" width="100" align="center" :excel-formatter="(row) => row.type === 'CHARGE' ? '청구(정산)' : (row.isCancelled ? '입금(취소됨)' : '입금')">
                  <template #default="item">
                    <el-tag :type="item.row.type === 'CHARGE' ? 'danger' : (item.row.isCancelled ? 'info' : 'success')">
                      {{ item.row.type === 'CHARGE' ? '청구(정산)' : '입금' }}
                    </el-tag>
                    <el-tag v-if="item.row.isCancelled" type="info" size="small" style="margin-left: 0.25rem;">취소됨</el-tag>
                  </template>
                </el-table-column>
                <el-table-column prop="orderNo" label="주문번호" width="180" align="center">
                  <template #default="item">
                    <span>{{ item.row.orderNo || '-' }}</span>
                  </template>
                </el-table-column>
                <el-table-column prop="amount" label="금액" width="130" align="right" :excel-formatter="historyAmountFormatter">
                  <template #default="item">
                    <span :style="{ color: item.row.type === 'CHARGE' ? '#f56c6c' : '#67c23a', fontWeight: 'bold' }">
                      {{ item.row.type === 'CHARGE' ? '+' : '-' }} ₩ {{ formatPrice(item.row.amount) }}
                    </span>
                  </template>
                </el-table-column>
                <el-table-column prop="remainingAmount" label="남은 잔액(미수)" width="130" align="right" :excel-formatter="remainingAmountFormatter">
                  <template #default="item">
                    <span v-if="item.row.type === 'CHARGE'" style="color: #909399;">
                      ₩ {{ formatPrice(item.row.remainingAmount) }}
                    </span>
                    <span v-else>-</span>
                  </template>
                </el-table-column>
                <el-table-column prop="memo" label="메모" min-width="200" :excel-formatter="(row) => memoExcelFormatter(row)">
                  <template #default="item">
                    <span>{{ item.row.memo }}</span>
                    <div v-if="item.row.type === 'DEPOSIT' && item.row.appliedCharges && item.row.appliedCharges.length > 0" class="applied-charges-note">
                      <div>적용된 주문:</div>
                      <div v-for="ac in item.row.appliedCharges" :key="ac.chargeId" class="applied-charge-line">
                        <span class="applied-order-link" @click="goToOrder(ac.orderNo)">{{ ac.orderNo || '(주문없음)' }}</span>
                        <span v-if="ac.appliedAmount">₩{{ formatPrice(ac.appliedAmount) }}</span>
                        <span v-if="ac.appliedWeight">{{ ac.appliedWeight.toFixed(2) }}g</span>
                      </div>
                    </div>
                  </template>
                </el-table-column>
                <el-table-column label="작업" width="220" align="center" fixed="right">
                  <template #default="item">
                    <div v-if="item.row.type === 'DEPOSIT'" style="display: flex; gap: 0.375rem; justify-content: center;">
                      <el-button size="small" @click="handlePrintReceipt(item.row, row)">영수증 출력</el-button>
                      <template v-if="!item.row.isCancelled">
                        <el-button size="small" type="warning" @click="openEditDialog(item.row)">수정</el-button>
                        <el-button size="small" type="danger" @click="handleCancelReceivable(item.row, row.userId)">정산취소</el-button>
                      </template>
                    </div>
                  </template>
                </el-table-column>
              </base-table>

              <div v-if="historyTotal[row.userId] > historyQuery.pageSize" class="pagination-container" style="margin-top: 0.625rem;">
                <el-pagination
                  v-model:current-page="historyQuery.page"
                  v-model:page-size="historyQuery.pageSize"
                  :total="historyTotal[row.userId] || 0"
                  layout="total, prev, pager, next"
                  @current-change="() => fetchHistory(row.userId)"
                />
              </div>
            </div>
          </template>
        </el-table-column>

        <el-table-column label="회원 아이디" prop="userName" width="150" align="center" />
        <el-table-column label="이름" prop="userDisplayName" width="150" align="center" />
        <el-table-column label="소속 거래처" prop="companyName" min-width="150" align="center">
          <template #default="{row}">
            <span>{{ row.companyName || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="totalReceivable" label="총 미수금 잔액" width="200" align="right" :excel-formatter="totalReceivableFormatter">
          <template #default="{row}">
            <span style="font-weight: bold; color: #f56c6c; font-size: 1rem;">
              ₩ {{ formatPrice(row.totalReceivable) }}
            </span>
          </template>
        </el-table-column>
        <el-table-column :label="$t('common.action')" width="150" align="center" :fixed="!isMobile ? 'right' : false">
          <template #default="{row}">
            <el-button
              type="success"
              size="small"
              @click="openDepositDialog(row)"
            >
              입금 처리
            </el-button>
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

    <deposit-dialog
      v-model="depositDialogVisible"
      :user="currentUser"
      @saved="onDepositSaved"
    />

    <receivable-edit-dialog
      v-model="editDialogVisible"
      :record="editingRecord"
      @saved="onEditSaved"
    />
  </div>
</template>

<script setup lang="ts">
import { useMobile } from '@/hooks/useMobile';
import { ref, reactive, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { getUserSummaries, getUserSummaryById, getReceivables, cancelReceivable } from '@/api/receivable';
import { ElMessage, ElMessageBox } from 'element-plus';
import type { FormInstance } from 'element-plus';
import { Search, Refresh } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import BaseTable from '@/components/BaseTable/index.vue';
import DepositDialog from './components/DepositDialog.vue';
import ReceivableEditDialog from './components/ReceivableEditDialog.vue';
import useUserStore from '@/store/modules/user';
const { isMobile } = useMobile();
const userStore = useUserStore();
const router = useRouter();

const goToOrder = (orderNo: string) => {
  if (!orderNo) return;
  router.push({ path: '/order/order-tracking', query: { orderNo } });
};

const route = useRoute();

const memoExcelFormatter = (row: any) => {
  let text = row.memo || '';
  if (row.type === 'DEPOSIT' && row.appliedCharges && row.appliedCharges.length > 0) {
    const lines = row.appliedCharges.map((ac: any) => `${ac.orderNo || '(주문없음)'}: ₩${formatPrice(ac.appliedAmount)} ${ac.appliedWeight ? ac.appliedWeight.toFixed(2) + 'g' : ''}`);
    text += `\n적용된 주문:\n${lines.join('\n')}`;
  }
  return text;
};

const listLoading = ref(true);
const submitLoading = ref(false);
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

const depositDialogVisible = ref(false);
const currentUser = ref<any>(null);

const formatDate = (dateStr: string) => {
  if (!dateStr) return '';
  return parseTime(new Date(dateStr), '{y}-{m}-{d} {h}:{i}');
};

const getList = async () => {
  listLoading.value = true;
  try {
    const res = await getUserSummaries(listQuery);
    list.value = res.data.items;
    total.value = res.data.totalCount;
  } catch (error) {
    console.error('Failed to get user summaries:', error);
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

const fetchHistory = async (userId: number) => {
  historyLoading[userId] = true;
  try {
    const res = await getReceivables({
      userId,
      page: historyQuery.page,
      pageSize: historyQuery.pageSize
    });
    historyData[userId] = res.data.items;
    historyTotal[userId] = res.data.totalCount;
  } catch (error) {
    console.error('Failed to fetch history:', error);
  } finally {
    historyLoading[userId] = false;
  }
};

const historyAmountFormatter = (row: any) => {
  const sign = row.type === 'CHARGE' ? '+' : '-';
  return `${sign} ₩${formatPrice(row.amount)}`;
};

const remainingAmountFormatter = (row: any) => {
  if (row.type !== 'CHARGE') return '-';
  return `₩${formatPrice(row.remainingAmount)}`;
};

const totalReceivableFormatter = (row: any) => {
  return `₩${formatPrice(row.totalReceivable)}`;
};

const openDepositDialog = (row: any) => {
  currentUser.value = row;
  depositDialogVisible.value = true;
};

const onDepositSaved = () => {
  getList();
  if (currentUser.value && historyData[currentUser.value.userId]) {
    fetchHistory(currentUser.value.userId);
  }
};

const editDialogVisible = ref(false);
const editingRecord = ref<any>(null);
const editingUserId = ref<number | null>(null);

const openEditDialog = (record: any) => {
  editingRecord.value = record;
  editingUserId.value = record.userId;
  editDialogVisible.value = true;
};

const onEditSaved = () => {
  getList();
  if (editingUserId.value && historyData[editingUserId.value]) {
    fetchHistory(editingUserId.value);
  }
};

const handleCancelReceivable = (record: any, userId: number) => {
  ElMessageBox.confirm('이 입금 내역을 취소하시겠습니까? 관련 미수금이 다시 복구됩니다.', '정산 취소', {
    confirmButtonText: '취소 처리',
    cancelButtonText: '닫기',
    type: 'warning'
  }).then(async () => {
    try {
      await cancelReceivable(record.id);
      ElMessage.success('정산이 취소되었습니다.');
      getList();
      fetchHistory(userId);
    } catch (error) {
      console.error('Failed to cancel receivable:', error);
      ElMessage.error('취소에 실패했습니다.');
    }
  }).catch(() => {});
};

const handlePrintReceipt = (record: any, user: any) => {
  const printWindow = window.open('', '_blank');
  if (!printWindow) return;

  // The deposit record itself has no stored before/after snapshot (it's applied across
  // however many outstanding charges it covers), so this shows the user's current
  // outstanding balance as the "after" figure - accurate as of now, best-effort for
  // older transactions if other settlements happened since.
  const afterAmount = user.totalReceivable || 0;
  const afterWeight = user.totalReceivableWeight || 0;
  const beforeAmount = afterAmount + (record.amount || 0) + (record.discount || 0);
  const beforeWeight = afterWeight + (record.weight || 0);
  const supplierName = userStore.companyName || '-';
  const partnerName = user.companyName || user.userDisplayName || '-';

  const ledgerRows = `
    <tr><td class="label">최근결제</td><td>${user.lastPaymentDate ? formatDate(user.lastPaymentDate) : '-'}</td><td></td><td></td></tr>
    <tr><td class="label">거래 전 미수(A)</td><td>${beforeWeight.toFixed(2)}</td><td>${formatPrice(beforeAmount)}</td><td></td></tr>
    <tr><td class="label">판매(B)</td><td>0.00</td><td>0</td><td></td></tr>
    <tr><td class="label">결제(C)</td><td>${(record.weight || 0).toFixed(2)}</td><td>${formatPrice(record.amount || 0)}</td><td></td></tr>
    <tr><td class="label">할인(D)</td><td>0.00</td><td>${formatPrice(record.discount || 0)}</td><td></td></tr>
    <tr><td class="label"><strong>거래 후 미수(A+B-C-D)</strong></td><td><strong>${afterWeight.toFixed(2)}</strong></td><td><strong>${formatPrice(afterAmount)}</strong></td><td></td></tr>
  `;

  const statementBlock = (copyLabel: string) => `
    <div class="statement-copy">
      <div class="statement-title">[${partnerName}] 거래 명세서(${copyLabel})</div>
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
        <title>거래명세서 - ${partnerName}</title>
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
          ${statementBlock('고객용')}
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

const loadSingleUser = async (userId: number) => {
  listLoading.value = true;
  try {
    const res: any = await getUserSummaryById(userId);
    list.value = res.data ? [res.data] : [];
    total.value = res.data ? 1 : 0;
    if (res.data) {
      fetchHistory(userId);
    }
  } catch (error) {
    console.error('Failed to load user summary:', error);
  } finally {
    listLoading.value = false;
  }
};

onMounted(() => {
  const userId = route.query.userId ? Number(route.query.userId) : null;
  if (userId) {
    loadSingleUser(userId);
  } else {
    getList();
  }
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
</style>

