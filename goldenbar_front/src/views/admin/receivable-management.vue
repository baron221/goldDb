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
                :data="historyData[row.userId] || []"
                border
                size="small"
                style="width: 100%; margin-top: 0.625rem;"
              >
                <el-table-column prop="createdAt" label="발생일시" width="160" align="center" :excel-formatter="(row) => formatDate(row.createdAt)">
                  <template #default="item">
                    <span>{{ formatDate(item.row.createdAt) }}</span>
                  </template>
                </el-table-column>
                <el-table-column prop="type" label="구분" width="100" align="center" :excel-formatter="(row) => row.type === 'CHARGE' ? '청구(정산)' : '입금'">
                  <template #default="item">
                    <el-tag :type="item.row.type === 'CHARGE' ? 'danger' : 'success'">
                      {{ item.row.type === 'CHARGE' ? '청구(정산)' : '입금' }}
                    </el-tag>
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
                <el-table-column prop="memo" label="메모" min-width="250">
                  <template #default="item">
                    <span>{{ item.row.memo }}</span>
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
import { ref, reactive, onMounted } from 'vue';
import { getUserSummaries, getReceivables } from '@/api/receivable';
import { ElMessage, ElMessageBox } from 'element-plus';
import type { FormInstance } from 'element-plus';
import { Search, Refresh } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import BaseTable from '@/components/BaseTable/index.vue';
import DepositDialog from './components/DepositDialog.vue';
const { isMobile } = useMobile();

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

onMounted(() => {
  getList();
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

