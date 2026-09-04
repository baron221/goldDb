<template>
<div class="receivable-management-container app-container">

    <el-card v-if="isMfg" shadow="never" class="filter-card">
      <div style="font-size: 1.1rem; font-weight: bold; margin-bottom: 0.9375rem;">미수금 관리</div>
      <el-form :inline="true" :model="overdueQuery" class="demo-form-inline">
        <el-form-item label="거래처">
          <company-select v-model="overdueQuery.logisticsCompanyId" category="DCC" placeholder="전체" style="width: 200px" @change="handleOverdueFilter" />
        </el-form-item>
        <el-form-item label="조회기간">
          <el-date-picker
            v-model="overdueDateRange"
            type="daterange"
            range-separator="~"
            start-placeholder="시작일"
            end-placeholder="종료일"
            value-format="YYYY-MM-DD"
            style="width: 260px"
            @change="handleOverdueDateChange"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :icon="Search" @click="handleOverdueFilter">검색</el-button>
          <el-button :icon="Refresh" @click="resetOverdueFilter">초기화</el-button>
        </el-form-item>
      </el-form>

      <base-table v-loading="overdueLoading" :data="overdueList" border row-key="logisticsCompanyId" style="width: 100%; margin-top: 1.25rem;">
        <el-table-column label="거래처" width="160" align="center" prop="companyName" />
        <el-table-column label="마지막거래일자" width="150" align="center">
          <template #default="{row}">{{ formatDate(row.lastTransactionDate || row.saleDate) }}</template>
        </el-table-column>
        <el-table-column label="순금/수공" min-width="170" align="right">
          <template #default="{row}">
            <div style="color: #f56c6c; font-weight: bold;">순금: {{ row.outstandingWeight.toFixed(2) }}g</div>
            <div style="color: #f56c6c;">수공: ₩{{ formatPrice(row.outstandingAmount) }}</div>
          </template>
        </el-table-column>
        <el-table-column label="연체일수" width="100" align="center">
          <template #default="{row}">{{ row.overdueDays }}일</template>
        </el-table-column>
        <el-table-column label="작업" width="110" align="center">
          <template #default="{row}">
            <el-button type="primary" size="small" @click="openOverdueSettleDialog(row)">입금처리</el-button>
          </template>
        </el-table-column>
      </base-table>

      <batch-settle-dialog
        v-model="overdueBatchSettleDialogVisible"
        :order-ids="overdueOrderIds"
        :single-mode="false"
        :hide-products="true"
        @saved="onOverdueSettleSaved"
      />
    </el-card>

    <el-card v-if="isDcc" shadow="never" class="filter-card">
      <div style="font-size: 1.1rem; font-weight: bold; margin-bottom: 0.9375rem;">미수금 관리</div>
      <el-form :inline="true" :model="receivableOverdueQuery" class="demo-form-inline">
        <el-form-item label="소매점">
          <el-select v-model="receivableOverdueQuery.userId" placeholder="전체" clearable filterable style="width: 200px" @change="handleReceivableOverdueFilter">
            <el-option v-for="c in list" :key="c.userId" :label="c.companyName || c.userDisplayName" :value="c.userId" />
          </el-select>
        </el-form-item>
        <el-form-item label="조회기간">
          <el-date-picker
            v-model="receivableOverdueDateRange"
            type="daterange"
            range-separator="~"
            start-placeholder="시작일"
            end-placeholder="종료일"
            value-format="YYYY-MM-DD"
            style="width: 260px"
            @change="handleReceivableOverdueDateChange"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :icon="Search" @click="handleReceivableOverdueFilter">검색</el-button>
          <el-button :icon="Refresh" @click="resetReceivableOverdueFilter">초기화</el-button>
        </el-form-item>
      </el-form>

      <base-table v-loading="receivableOverdueLoading" :data="receivableOverdueList" border row-key="userId" style="width: 100%; margin-top: 1.25rem;">
        <el-table-column label="거래처" width="160" align="center">
          <template #default="{row}">{{ row.companyName || row.userDisplayName }}</template>
        </el-table-column>
        <el-table-column label="마지막거래일자" width="150" align="center">
          <template #default="{row}">{{ formatDate(row.lastTransactionDate || row.saleDate) }}</template>
        </el-table-column>
        <el-table-column label="순금/수공" min-width="170" align="right">
          <template #default="{row}">
            <div style="color: #f56c6c; font-weight: bold;">순금: {{ row.outstandingWeight.toFixed(2) }}g</div>
            <div style="color: #f56c6c;">수공: ₩{{ formatPrice(row.outstandingAmount) }}</div>
          </template>
        </el-table-column>
        <el-table-column label="연체일수" width="100" align="center">
          <template #default="{row}">{{ row.overdueDays }}일</template>
        </el-table-column>
        <el-table-column label="작업" width="110" align="center">
          <template #default="{row}">
            <el-button type="primary" size="small" @click="openReceivableOverdueSettleDialog(row)">입금처리</el-button>
          </template>
        </el-table-column>
      </base-table>

      <receivable-batch-settle-dialog
        v-model="receivableOverdueBatchSettleDialogVisible"
        :order-ids="receivableOverdueOrderIds"
        :single-mode="false"
        :hide-products="true"
        @saved="onReceivableOverdueSettleSaved"
      />
    </el-card>

    <el-card v-if="!isMfg && (!isDcc || isAdmin)" shadow="never" class="filter-card">
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

    <el-card v-if="!isMfg && (!isDcc || isAdmin)" shadow="never" style="margin-top: 1.25rem;">
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
                <el-table-column prop="orderNo" label="주문내용" min-width="220" align="left" :excel-formatter="(row) => `${row.orderNo || '-'}${row.productName ? ' / ' + row.productName : ''}`">
                  <template #default="item">
                    <div v-if="item.row.orderNo" style="display: flex; align-items: center; gap: 0.5rem;">
                      <el-image :src="item.row.productPhotoUrl || defaultImage" fit="cover" style="width: 44px; height: 44px; border-radius: 2px; border: 1px solid #ebeef5; flex-shrink: 0;" />
                      <div style="min-width: 0; text-align: left;">
                        <div style="font-weight: 600; font-size: 0.875rem;">{{ item.row.productName || '-' }}</div>
                        <div class="applied-order-link" style="font-size: 0.8125rem;" @click="goToOrder(item.row.orderNo)">{{ item.row.orderNo }}</div>
                      </div>
                    </div>
                    <span v-else>-</span>
                  </template>
                </el-table-column>
                <el-table-column prop="amount" label="금액" width="130" align="right" :excel-formatter="historyAmountFormatter">
                  <template #default="item">
                    <span :style="{ color: item.row.type === 'CHARGE' ? '#f56c6c' : '#67c23a', fontWeight: 'bold' }">
                      {{ item.row.type === 'CHARGE' ? '+' : '-' }} ₩ {{ formatPrice(item.row.amount) }}
                    </span>
                  </template>
                </el-table-column>
                <el-table-column v-if="!isMobile" label="결제 금액" width="130" align="right" :excel-formatter="(row) => row.type === 'CHARGE' ? formatPrice(row.amount - row.remainingAmount) : '-'">
                  <template #default="item">
                    <span v-if="item.row.type === 'CHARGE'" style="color: #67c23a;">
                      ₩ {{ formatPrice(item.row.amount - item.row.remainingAmount) }}
                    </span>
                    <span v-else>-</span>
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
                      <!-- This deposit was recorded by the DCC that collected it - the retailer
                           it was collected from (this whole card is also shown to them, see
                           isDcc's own comment above) can view and print it, but never cancel or
                           otherwise change it. Only an admin gets the override here. -->
                      <template v-if="!item.row.isCancelled && !item.row.sourcePaymentId && isAdmin">
                        <el-button size="small" type="danger" @click="handleCancelReceivable(item.row, row.userId)">정산취소</el-button>
                      </template>
                      <span v-else-if="!item.row.isCancelled && item.row.sourcePaymentId" style="font-size: 0.75rem; color: #909399;">정산처리 연동</span>
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

  </div>
</template>

<script setup lang="ts">
import { useMobile } from '@/hooks/useMobile';
import { ref, reactive, computed, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { getUserSummaries, getUserSummaryById, getReceivables, cancelReceivable, getLedgerBefore, getReceivableOverdueSummary } from '@/api/receivable';
import { getPayableOverdueSummary } from '@/api/payable';
import { ElMessage, ElMessageBox } from 'element-plus';
import type { FormInstance } from 'element-plus';
import { Search, Refresh } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import BaseTable from '@/components/BaseTable/index.vue';
import DepositDialog from './components/DepositDialog.vue';
import BatchSettleDialog from './components/BatchSettleDialog.vue';
import ReceivableBatchSettleDialog from './components/ReceivableBatchSettleDialog.vue';
import CompanySelect from '@/components/CompanySelect/index.vue';
import useUserStore from '@/store/modules/user';
const { isMobile } = useMobile();
const userStore = useUserStore();
const router = useRouter();
const defaultImage = '/thumb_no_img.png';

const isDcc = computed(() => userStore.companyType === 'DCC' || userStore.roles.includes('admin'));

// isDcc folds admin into the DCC-aggregate branch above, but admin also needs the detailed
// per-user table below (정산취소/거래명세서 출력/general 입금 처리) - a plain DCC viewer never
// gets that table (they use the aggregate one instead), but admin shouldn't lose it just
// because they also satisfy isDcc. See the two `!isMfg && (!isDcc || isAdmin)` guards below.
const isAdmin = computed(() => userStore.roles.includes('admin'));

// MFG's own 미수금 관리 - tracks partially-paid Payable charges (DCC owes this factory) by
// DCC partner. This page is otherwise entirely Receivable-focused (DCC<->retailer), but the
// sidebar's single "미수금 관리" tab is shared, so an MFG viewer gets this Payable-side view
// instead of the Receivable content above, which would be empty/meaningless for them anyway.
const isMfg = computed(() => userStore.companyType === 'MFG');

const overdueLoading = ref(false);
const overdueList = ref<any[]>([]);
const overdueQuery = reactive({
  logisticsCompanyId: undefined as number | undefined,
  startDate: undefined as string | undefined,
  endDate: undefined as string | undefined
});
const overdueDateRange = ref<[string, string] | null>(null);

const fetchOverdueSummary = async () => {
  overdueLoading.value = true;
  try {
    const res: any = await getPayableOverdueSummary({ ...overdueQuery });
    overdueList.value = res.data || [];
  } catch (error) {
    console.error('Failed to fetch payable overdue summary:', error);
  } finally {
    overdueLoading.value = false;
  }
};

const handleOverdueFilter = () => {
  fetchOverdueSummary();
};

const handleOverdueDateChange = (val: [string, string] | null) => {
  overdueQuery.startDate = val ? val[0] : undefined;
  overdueQuery.endDate = val ? val[1] : undefined;
  fetchOverdueSummary();
};

const resetOverdueFilter = () => {
  overdueQuery.logisticsCompanyId = undefined;
  overdueQuery.startDate = undefined;
  overdueQuery.endDate = undefined;
  overdueDateRange.value = null;
  fetchOverdueSummary();
};

const overdueBatchSettleDialogVisible = ref(false);
const overdueOrderIds = ref<number[]>([]);

const openOverdueSettleDialog = (row: any) => {
  overdueOrderIds.value = row.orderIds;
  overdueBatchSettleDialogVisible.value = true;
};

const onOverdueSettleSaved = () => {
  fetchOverdueSummary();
};

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

const handlePrintReceipt = async (record: any, user: any) => {
  const printWindow = window.open('', '_blank');
  if (!printWindow) return;

  // company.totalReceivable is today's running balance, so it's only a correct "before"
  // figure for the single most recent deposit. For anything older, fetch the true
  // point-in-time balance from the backend instead of approximating from today's total.
  let beforeAmount = (user.totalReceivable || 0) + (record.amount || 0) + (record.discount || 0);
  let beforeWeight = (user.totalReceivableWeight || 0) + (record.weight || 0);
  let newChargeAmount = 0;
  let newChargeWeight = 0;
  if (record.id) {
    try {
      const res: any = await getLedgerBefore(record.id);
      beforeAmount = res.data.beforeAmount;
      beforeWeight = res.data.beforeWeight;
      newChargeAmount = res.data.newChargeAmount || 0;
      newChargeWeight = res.data.newChargeWeight || 0;
    } catch (error) {
      console.error('Failed to fetch accurate ledger balance, falling back to approximation:', error);
    }
  }
  const afterAmount = beforeAmount + newChargeAmount - (record.amount || 0) - (record.discount || 0);
  const afterWeight = beforeWeight + newChargeWeight - (record.weight || 0);
  const supplierName = userStore.companyName || '-';
  const partnerName = user.companyName || user.userDisplayName || '-';

  const ledgerRows = `
    <tr><td class="label">최근결제</td><td>${user.lastPaymentDate ? formatDate(user.lastPaymentDate) : '-'}</td><td></td><td></td></tr>
    <tr><td class="label">거래 전 미수(A)</td><td>${beforeWeight.toFixed(2)}</td><td>${formatPrice(beforeAmount)}</td><td></td></tr>
    <tr><td class="label">판매(B)</td><td>${newChargeWeight.toFixed(2)}</td><td>${formatPrice(newChargeAmount)}</td><td></td></tr>
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

// ---- DCC's own 미수금 관리 - retailers with at least one partially-paid Receivable charge
// still outstanding. Mirrors the MFG-side payable-overdue tracker exactly (one row per
// counterparty, not per order), just grouped by retailer instead of by DCC partner. ----

const receivableOverdueLoading = ref(false);
const receivableOverdueList = ref<any[]>([]);
const receivableOverdueQuery = reactive({
  userId: undefined as number | undefined,
  startDate: undefined as string | undefined,
  endDate: undefined as string | undefined
});
const receivableOverdueDateRange = ref<[string, string] | null>(null);

const fetchReceivableOverdueSummary = async () => {
  receivableOverdueLoading.value = true;
  try {
    const res: any = await getReceivableOverdueSummary({ ...receivableOverdueQuery });
    receivableOverdueList.value = res.data || [];
  } catch (error) {
    console.error('Failed to fetch receivable overdue summary:', error);
  } finally {
    receivableOverdueLoading.value = false;
  }
};

const handleReceivableOverdueFilter = () => {
  fetchReceivableOverdueSummary();
};

const handleReceivableOverdueDateChange = (val: [string, string] | null) => {
  receivableOverdueQuery.startDate = val ? val[0] : undefined;
  receivableOverdueQuery.endDate = val ? val[1] : undefined;
  fetchReceivableOverdueSummary();
};

const resetReceivableOverdueFilter = () => {
  receivableOverdueQuery.userId = undefined;
  receivableOverdueQuery.startDate = undefined;
  receivableOverdueQuery.endDate = undefined;
  receivableOverdueDateRange.value = null;
  fetchReceivableOverdueSummary();
};

const receivableOverdueBatchSettleDialogVisible = ref(false);
const receivableOverdueOrderIds = ref<number[]>([]);

const openReceivableOverdueSettleDialog = (row: any) => {
  receivableOverdueOrderIds.value = row.orderIds;
  receivableOverdueBatchSettleDialogVisible.value = true;
};

const onReceivableOverdueSettleSaved = () => {
  fetchReceivableOverdueSummary();
};

onMounted(() => {
  if (isMfg.value) {
    fetchOverdueSummary();
    return;
  }

  if (isDcc.value) {
    getList();
    fetchReceivableOverdueSummary();
    return;
  }

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

