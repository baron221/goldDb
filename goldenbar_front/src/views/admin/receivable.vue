<template>
<div class="receivable-container app-container">
    <el-row :gutter="20">

      <el-col :xs="24" :sm="10">
        <el-card class="box-card" shadow="never">
          <template #header>
            <div class="card-header">
              <span>소매점별 미수금 현황</span>
            </div>
          </template>

          <div class="filter-container">
            <el-input
              v-model="summaryQuery.search"
              placeholder="소매점명 또는 실명 검색"
              class="filter-item search-input"
              clearable
              @keyup.enter="handleSummaryFilter"
            >
              <template #append>
                <el-button :icon="Search" @click="handleSummaryFilter" />
              </template>
            </el-input>
          </div>

          <base-table
            v-loading="summaryLoading"
            :data="summaryList"
            :total="summaryTotal"
            v-model:page="summaryQuery.page"
            v-model:page-size="summaryQuery.pageSize"
            style="width: 100%; cursor: pointer;"
            @row-click="handleRowClick"
            @change="getSummaryList"
          >
            <el-table-column label="소매점 (실명)" min-width="120" :excel-formatter="(row) => (row.companyName || row.userName) + (row.userDisplayName ? ' (' + row.userDisplayName + ')' : '')">
              <template #default="{row}">
                {{ row.companyName || row.userName }}
                <div v-if="row.userDisplayName" style="color: #909399; font-size: 0.8875rem;">({{ row.userDisplayName }})</div>
              </template>
            </el-table-column>
            <el-table-column label="총 청구액" align="right" width="100" :excel-formatter="(row) => formatPrice(row.totalCharge)">
              <template #default="{row}">
                <span style="color: #909399;">
                  {{ formatPrice(row.totalCharge) }}
                </span>
              </template>
            </el-table-column>
            <el-table-column label="총 수납액" align="right" width="100" :excel-formatter="(row) => formatPrice(row.totalDeposit)">
              <template #default="{row}">
                <span style="color: #67c23a;">
                  {{ formatPrice(row.totalDeposit) }}
                </span>
              </template>
            </el-table-column>
            <el-table-column label="미납금" align="right" width="100" :excel-formatter="(row) => formatPrice(row.totalReceivable)">
              <template #default="{row}">
                <span class="amount-text" :class="{'has-debt': row.totalReceivable > 0}">
                  {{ formatPrice(row.totalReceivable) }}
                </span>
              </template>
            </el-table-column>
            <el-table-column label="관리" align="center" width="70">
              <template #default="{row}">
                <el-button type="primary" size="small" plain @click.stop="openDepositDialog(row)">수납</el-button>
              </template>
            </el-table-column>
          </base-table>
        </el-card>
      </el-col>

      <el-col :xs="24" :sm="14">
        <el-card class="box-card" shadow="never">
          <template #header>
            <div class="card-header flex-between">
              <span>상세 내역 <span v-if="selectedUser" class="text-primary">[{{ selectedUser.companyName || selectedUser.userName }}]</span></span>
              <el-button v-if="selectedUser" type="success" size="small" @click="openDepositDialog(selectedUser)">
                수납 등록
              </el-button>
            </div>
          </template>

          <div v-if="!selectedUser" class="empty-state">
            <el-empty description="좌측 목록에서 소매점을 선택해주세요." />
          </div>
          <div v-else>
            <div class="filter-container mb-3">
              <el-radio-group v-model="detailQuery.type" @change="handleDetailFilter">
                <el-radio-button value="">전체</el-radio-button>
                <el-radio-button value="CHARGE">청구(미수)</el-radio-button>
                <el-radio-button value="DEPOSIT">수납(입금)</el-radio-button>
              </el-radio-group>
            </div>

            <base-table
              v-loading="detailLoading"
              :data="detailList"
              :total="detailTotal"
              v-model:page="detailQuery.page"
              v-model:page-size="detailQuery.pageSize"
              style="width: 100%;"
              @change="getDetailList"
            >
              <el-table-column label="일시" width="160" align="center" :excel-formatter="(row) => formatDate(row.createdAt)">
                <template #default="{row}">
                  <span>{{ formatDate(row.createdAt) }}</span>
                </template>
              </el-table-column>
              <el-table-column label="구분" width="100" align="center" :excel-formatter="(row) => row.type === 'CHARGE' ? '청구' : '수납'">
                <template #default="{row}">
                  <el-tag :type="row.type === 'CHARGE' ? 'danger' : 'success'">
                    {{ row.type === 'CHARGE' ? '청구' : '수납' }}
                  </el-tag>
                </template>
              </el-table-column>
              <el-table-column label="금액" align="right" width="120" :excel-formatter="(row) => (row.type === 'CHARGE' ? '+' : '-') + formatPrice(row.amount)">
                <template #default="{row}">
                  <span :class="row.type === 'CHARGE' ? 'text-danger' : 'text-success'">
                    {{ row.type === 'CHARGE' ? '+' : '-' }}{{ formatPrice(row.amount) }}
                  </span>
                </template>
              </el-table-column>
              <el-table-column label="잔여 청구액" align="right" width="110" :excel-formatter="(row) => row.type === 'CHARGE' ? formatPrice(row.remainingAmount) : '-'">
                <template #default="{row}">
                  <span v-if="row.type === 'CHARGE'" class="text-gray">{{ formatPrice(row.remainingAmount) }}</span>
                  <span v-else>-</span>
                </template>
              </el-table-column>
              <el-table-column label="수납방법" width="100" align="center" :excel-formatter="(row) => row.settlementMethod ? codeStore.getCodeName(row.settlementMethod) : '-'">
                <template #default="{row}">
                  {{ row.settlementMethod ? codeStore.getCodeName(row.settlementMethod) : '-' }}
                </template>
              </el-table-column>
              <el-table-column label="관련 주문/메모" min-width="200" :excel-formatter="(row) => (row.orderNo ? row.orderNo + '\n' : '') + row.memo">
                <template #default="{row}">
                  <div v-if="row.orderNo" class="order-link">{{ row.orderNo }}</div>
                  <div class="memo-text">{{ row.memo }}</div>
                </template>
              </el-table-column>
            </base-table>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <receivable-deposit-dialog
      v-model="depositDialogVisible"
      :user="selectedUserForDeposit"
      @success="handleDepositSuccess"
    />
  </div>
</template>

<script setup lang="ts">

import { ref, reactive, onMounted, computed } from 'vue';
import { Search } from '@element-plus/icons-vue';
import { getUserSummaries, getReceivables } from '@/api/receivable';
import { formatPrice } from '@/utils/format';
import BaseTable from '@/components/BaseTable/index.vue';
import ReceivableDepositDialog from './components/ReceivableDepositDialog.vue';
import useCodeStore from '@/store/modules/code';
import dayjs from 'dayjs';

const codeStore = useCodeStore();
const codeMap = computed(() => codeStore.codeMap);

const summaryLoading = ref(false);
const summaryList = ref<any[]>([]);
const summaryTotal = ref(0);
const summaryQuery = reactive({
  page: 1,
  pageSize: 20,
  search: ''
});

const detailLoading = ref(false);
const detailList = ref<any[]>([]);
const detailTotal = ref(0);
const selectedUser = ref<any>(null);
const detailQuery = reactive({
  page: 1,
  pageSize: 20,
  userId: undefined as number | undefined,
  type: ''
});

const depositDialogVisible = ref(false);
const selectedUserForDeposit = ref<any>(null);

onMounted(async () => {
  await codeStore.fetchCodes();

  getSummaryList();
});

const getSummaryList = async () => {
  try {
    summaryLoading.value = true;
    const res: any = await getUserSummaries(summaryQuery);
    summaryList.value = res.data.items;
    summaryTotal.value = res.data.totalCount;
  } catch (error) {
    console.error(error);
  } finally {
    summaryLoading.value = false;
  }
};

const handleSummaryFilter = () => {
  summaryQuery.page = 1;
  getSummaryList();
};

const handleRowClick = (row: any) => {
  selectedUser.value = row;
  detailQuery.userId = row.userId;
  detailQuery.page = 1;
  detailQuery.type = '';
  getDetailList();
};

const getDetailList = async () => {
  if (!detailQuery.userId) return;
  try {
    detailLoading.value = true;
    const res: any = await getReceivables(detailQuery);
    detailList.value = res.data.items;
    detailTotal.value = res.data.totalCount;
  } catch (error) {
    console.error(error);
  } finally {
    detailLoading.value = false;
  }
};

const handleDetailFilter = () => {
  detailQuery.page = 1;
  getDetailList();
};

const openDepositDialog = (user: any) => {
  selectedUserForDeposit.value = user;
  depositDialogVisible.value = true;
};

const handleDepositSuccess = () => {
  getSummaryList();
  if (selectedUser.value && selectedUser.value.userId === selectedUserForDeposit.value?.userId) {
    getDetailList();
  }
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return '';
  return dayjs(dateStr).format('YYYY-MM-DD HH:mm');
};
</script>

<style lang="scss" src="./ReceivableStyles.scss" scoped></style>

