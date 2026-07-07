<template>
<div class="retailer-settlement-container app-container">
    <div class="header-section">
      <h2 class="page-title">{{ $t('retailerSettlement.historyTitle') }}</h2>
      <p class="page-subtitle">{{ $t('retailerSettlement.historySubtitle') }}</p>
    </div>

    <el-row :gutter="24" class="panel-group" v-loading="summaryLoading">
      <el-col :xs="24" :sm="8" class="card-panel-col">
        <div class="stat-card minimalist charge-card">
          <div class="stat-label">{{ $t('retailerSettlement.totalBillAmount') }}</div>
          <div class="stat-value">
            <span class="currency">₩</span>
            <span class="amount-val">{{ formatPrice(mySummary.totalCharge) }}</span>
          </div>
        </div>
      </el-col>
      <el-col :xs="24" :sm="8" class="card-panel-col">
        <div class="stat-card minimalist deposit-card">
          <div class="stat-label">{{ $t('retailerSettlement.totalAmount') }} (수납)</div>
          <div class="stat-value">
            <span class="currency">₩</span>
            <span class="amount-val">{{ formatPrice(mySummary.totalDeposit) }}</span>
          </div>
        </div>
      </el-col>
      <el-col :xs="24" :sm="8" class="card-panel-col">
        <div class="stat-card highlight-gold unpaid-card">
          <div class="stat-label">{{ $t('retailerSettlement.totalUnpaid') }}</div>
          <div class="stat-value">
            <span class="currency">₩</span>
            <span class="amount-val">{{ formatPrice(mySummary.totalReceivable) }}</span>
          </div>
        </div>
      </el-col>
    </el-row>

    <el-card shadow="never" class="filter-card">
      <el-form :inline="true" :model="detailQuery" class="demo-form-inline">
        <el-form-item :label="$t('order.filters.orderNo')">
          <el-input v-model="detailQuery.orderNo" placeholder="주문번호 입력" clearable @keyup.enter="handleDetailFilter" />
        </el-form-item>
        <el-form-item label="제품번호">
          <el-input v-model="detailQuery.productNo" placeholder="제품번호 입력" clearable @keyup.enter="handleDetailFilter" />
        </el-form-item>
        <el-form-item :label="$t('retailerSettlement.productName')">
          <el-input v-model="detailQuery.productName" placeholder="제품명 입력" clearable @keyup.enter="handleDetailFilter" />
        </el-form-item>
        <el-form-item :label="$t('stock.labels.manufacturer')">
          <el-select v-model="detailQuery.manufacturerId" placeholder="제조사 선택" clearable @change="handleDetailFilter" style="width: 150px;">
            <el-option v-for="item in manufacturers" :key="item.id" :label="item.name" :value="item.id" />
          </el-select>
        </el-form-item>

        <el-form-item :label="$t('admin.settlement.filters.generalCategory')">
          <common-select
            v-model="detailQuery.categoryLarge"
            group-code="PRODUCT_CATEGORY"
            placeholder="대분류"
            style="width: 120px; margin-right: 0.3125rem;"
            @change="(val, options) => handleLargeChange(val, options)"
          />
          <common-select
            v-model="detailQuery.categoryMedium"
            :parent-id="largeId"
            placeholder="중분류"
            style="width: 120px; margin-right: 0.3125rem;"
            @change="(val, options) => handleMediumChange(val, options)"
          />
          <common-select
            v-model="detailQuery.categorySmall"
            :parent-id="mediumId"
            placeholder="소분류"
            style="width: 120px;"
            @change="handleDetailFilter"
          />
        </el-form-item>

        <el-form-item :label="$t('admin.settlement.filters.period')">
          <el-date-picker
            v-model="dateRange"
            type="daterange"
            range-separator="-"
            start-placeholder="시작일"
            end-placeholder="종료일"
            value-format="YYYY-MM-DD"
            @change="handleDateChange"
            style="width: 240px;"
          />
        </el-form-item>

        <el-form-item>
          <el-button type="primary" :icon="Search" @click="handleDetailFilter">{{ $t('common.search') }}</el-button>
          <el-button :icon="Refresh" @click="resetQuery">{{ $t('common.reset') }}</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card shadow="never" class="table-card">
      <div class="filter-container mb-3">
        <el-radio-group v-model="detailQuery.type" @change="handleDetailFilter">
          <el-radio-button value="">{{ $t('retailerSettlement.all') }}</el-radio-button>
          <el-radio-button value="CHARGE">{{ $t('retailerSettlement.charge') }}</el-radio-button>
          <el-radio-button value="DEPOSIT">{{ $t('retailerSettlement.deposit') }}</el-radio-button>
        </el-radio-group>
      </div>

      <base-table
        v-loading="detailLoading"
        :data="detailList"
        :total="detailTotal"
        v-model:page="detailQuery.page"
        v-model:page-size="detailQuery.pageSize"
        @change="getDetailList"
      >
        <el-table-column
          :label="$t('retailerSettlement.settledDate')"
          width="180"
          align="center"
          :excel-formatter="(row: any) => formatDate(row.createdAt)"
        >
          <template #default="{row}">
            <span>{{ formatDate(row.createdAt) }}</span>
          </template>
        </el-table-column>

        <el-table-column
          :label="$t('retailerSettlement.type')"
          width="120"
          align="center"
          :excel-formatter="(row: any) => row.type === 'CHARGE' ? $t('retailerSettlement.charge') : $t('retailerSettlement.deposit')"
        >
          <template #default="{row}">
            <el-tag :type="row.type === 'CHARGE' ? 'danger' : 'success'" effect="light" class="type-tag">
              {{ row.type === 'CHARGE' ? $t('retailerSettlement.charge') : $t('retailerSettlement.deposit') }}
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column
          :label="$t('retailerSettlement.amount')"
          align="right"
          width="160"
          :excel-formatter="(row: any) => (row.type === 'CHARGE' ? '+' : '-') + formatPrice(row.amount)"
        >
          <template #default="{row}">
            <span :class="row.type === 'CHARGE' ? 'text-danger' : 'text-success'" class="amount-text">
              {{ row.type === 'CHARGE' ? '+' : '-' }}{{ formatPrice(row.amount) }}
            </span>
          </template>
        </el-table-column>

        <el-table-column
          :label="$t('retailerSettlement.remainingAmount')"
          align="right"
          width="160"
          :excel-formatter="(row: any) => row.type === 'CHARGE' ? formatPrice(row.remainingAmount) : '-'"
        >
          <template #default="{row}">
            <span v-if="row.type === 'CHARGE'" class="amount-text text-gray">{{ formatPrice(row.remainingAmount) }}</span>
            <span v-else>-</span>
          </template>
        </el-table-column>

        <el-table-column
          :label="$t('retailerSettlement.method')"
          width="140"
          align="center"
          :excel-formatter="(row: any) => row.settlementMethod ? codeStore.getCodeName(row.settlementMethod) : '-'"
        >
          <template #default="{row}">
            {{ row.settlementMethod ? codeStore.getCodeName(row.settlementMethod) : '-' }}
          </template>
        </el-table-column>

        <el-table-column
          :label="$t('retailerSettlement.orderAndMemo')"
          min-width="250"
          :excel-formatter="(row: any) => (row.orderNo ? '[주문] ' + row.orderNo : '') + (row.memo ? '\n' + row.memo : '')"
        >
          <template #default="{row}">
            <div v-if="row.orderNo" class="order-link-wrapper" @click="handleOrderClick(row.orderNo)">
              <el-tag size="small" type="info" effect="plain" class="mr-1">주문</el-tag>
              <span class="order-no">{{ row.orderNo }}</span>
            </div>
            <div class="memo-text">{{ row.memo || '-' }}</div>
          </template>
        </el-table-column>
      </base-table>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { getReceivables, getUserSummaries } from '@/api/receivable';
import { getCompanies } from '@/api/company';
import { formatPrice } from '@/utils/format';
import useCodeStore from '@/store/modules/code';
import useUserStore from '@/store/modules/user';
import { Document, CircleCheck, Warning, Search, Refresh } from '@element-plus/icons-vue';
import CommonSelect from '@/components/CommonSelect/index.vue';
import BaseTable from '@/components/BaseTable/index.vue';
import dayjs from 'dayjs';

const { t } = useI18n();
const router = useRouter();
const codeStore = useCodeStore();
const userStore = useUserStore();

const summaryLoading = ref(false);
const detailLoading = ref(false);
const detailList = ref<any[]>([]);
const detailTotal = ref(0);
const manufacturers = ref<any[]>([]);
const largeId = ref<number | null>(null);
const mediumId = ref<number | null>(null);
const dateRange = ref<string[]>([]);

const mySummary = reactive({
  totalCharge: 0,
  totalDeposit: 0,
  totalReceivable: 0
});

const detailQuery = reactive({
  page: 1,
  pageSize: 20,
  userId: undefined as number | undefined,
  type: '',
  orderNo: '',
  productNo: '',
  productName: '',
  manufacturerId: undefined as number | undefined,
  categoryLarge: '',
  categoryMedium: '',
  categorySmall: '',
  startDate: undefined as string | undefined,
  endDate: undefined as string | undefined
});

onMounted(async () => {
  await codeStore.fetchCodes();
  detailQuery.userId = userStore.userId ? Number(userStore.userId) : undefined;
  getMySummary();
  fetchManufacturers();
  getDetailList();
});

const fetchManufacturers = async () => {
  try {
    const res = await getCompanies({ category: 'MFG', pageSize: 100 });
    if (res.data && res.data.items) {
      manufacturers.value = res.data.items;
    }
  } catch (error) {
    console.error('Failed to fetch manufacturers:', error);
  }
};

const handleLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  largeId.value = selected ? selected.id : null;
  detailQuery.categoryMedium = '';
  detailQuery.categorySmall = '';
  mediumId.value = null;
  handleDetailFilter();
};

const handleMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  mediumId.value = selected ? selected.id : null;
  detailQuery.categorySmall = '';
  handleDetailFilter();
};

const handleDateChange = (val: string[] | null) => {
  if (val) {
    detailQuery.startDate = val[0];
    detailQuery.endDate = val[1];
  } else {
    detailQuery.startDate = undefined;
    detailQuery.endDate = undefined;
  }
  handleDetailFilter();
};

const resetQuery = () => {
  detailQuery.orderNo = '';
  detailQuery.productNo = '';
  detailQuery.productName = '';
  detailQuery.manufacturerId = undefined;
  detailQuery.categoryLarge = '';
  detailQuery.categoryMedium = '';
  detailQuery.categorySmall = '';
  detailQuery.startDate = undefined;
  detailQuery.endDate = undefined;
  largeId.value = null;
  mediumId.value = null;
  dateRange.value = [];
  handleDetailFilter();
};

const getMySummary = async () => {
  if (!userStore.username) return;
  try {
    summaryLoading.value = true;
    const res: any = await getUserSummaries({
      page: 1,
      pageSize: 1,
      search: userStore.username
    });
    if (res.data && res.data.items && res.data.items.length > 0) {
      const item = res.data.items.find((i: any) => i.userName === userStore.username) || res.data.items[0];
      mySummary.totalCharge = item.totalCharge;
      mySummary.totalDeposit = item.totalDeposit;
      mySummary.totalReceivable = item.totalReceivable;
    }
  } catch (error) {
    console.error('Failed to get my receivable summary:', error);
  } finally {
    summaryLoading.value = false;
  }
};

const getDetailList = async () => {
  if (!detailQuery.userId && userStore.userId) {
    detailQuery.userId = Number(userStore.userId);
  }
  try {
    detailLoading.value = true;
    const res: any = await getReceivables(detailQuery);
    detailList.value = res.data.items;
    detailTotal.value = res.data.totalCount;
  } catch (error) {
    console.error('Failed to get receivable details:', error);
  } finally {
    detailLoading.value = false;
  }
};

const handleDetailFilter = () => {
  detailQuery.page = 1;
  getDetailList();
};

const handleSizeChange = (val: number) => {
  detailQuery.pageSize = val;
  detailQuery.page = 1;
  getDetailList();
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return '';
  return dayjs(dateStr).format('YYYY-MM-DD HH:mm');
};

const handleOrderClick = (orderNo: string) => {
  router.push({
    path: '/my/order',
    query: { orderNo }
  });
};
</script>

<style lang="scss" scoped>
@import "./StockSettlementStyles.scss";
</style>

