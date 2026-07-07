<template>
<div class="dashboard-logistics-page">

    <div class="dashboard-header-banner">
      <div class="header-content-wrapper">
        <div class="user-profile-area">
          <pan-thumb :image="avatar" width="100px" height="100px" class="jovenca-thumb">
            <div class="thumb-content">
              <span class="thumb-label">{{ $t('dashboard.admin.labels.roles') }}</span>
              <span v-for="item in roles" :key="item" class="role-tag">{{ item }}</span>
            </div>
          </pan-thumb>
          <div class="welcome-text">
            <h1 class="display-name">{{ name }}</h1>
            <p class="dashboard-title">{{ $t('dashboard.logistics.title') }}</p>
          </div>
        </div>
        <div class="header-actions">
          <el-button link :icon="Refresh" @click="refreshData" class="refresh-btn">{{ $t('dashboard.common.refreshData') }}</el-button>
        </div>
      </div>
    </div>

    <div class="dashboard-body-content">

      <section class="section-container">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.logistics.inventorySummary') }}</h2>
        </div>

        <el-row :gutter="24" class="summary-grid">
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card minimalist">
              <div class="stat-label">{{ $t('dashboard.logistics.total14K') }}</div>
              <div class="stat-value">{{ summary.total14KWeight.toFixed(2) }}<span class="unit">{{ $t('dashboard.common.units.g') }}</span></div>
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card minimalist">
              <div class="stat-label">{{ $t('dashboard.logistics.total18K') }}</div>
              <div class="stat-value">{{ summary.total18KWeight.toFixed(2) }}<span class="unit">{{ $t('dashboard.common.units.g') }}</span></div>
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card minimalist">
              <div class="stat-label">{{ $t('dashboard.logistics.pureGold') }}</div>
              <div class="stat-value">{{ summary.totalPureGoldWeight.toFixed(2) }}<span class="unit">{{ $t('dashboard.common.units.g') }}</span></div>
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card highlight-gold">
              <div class="stat-label">{{ $t('dashboard.logistics.pureGoldReserve') }}</div>
              <div class="stat-value">{{ summary.totalCalculatedPureGoldWeight.toFixed(2) }}<span class="unit">{{ $t('dashboard.common.units.g') }}</span></div>
              <div class="formula-tooltip">
                ({{ summary.total14KWeight.toFixed(2) }}g × 0.6435) + ({{ summary.total18KWeight.toFixed(2) }}g × 0.825) + {{ summary.totalPureGoldWeight.toFixed(2) }}g
              </div>
            </div>
          </el-col>
        </el-row>
      </section>

      <section class="section-container">
        <div class="section-header">
          <h2 class="section-title">협력 소매점 현황</h2>
        </div>
        <partner-retailer-summary ref="partnerSummaryRef" />
      </section>

      <section class="section-container">
        <div class="section-header">
          <h2 class="section-title">소매점 미수금 현황</h2>
        </div>

        <el-row :gutter="24" class="summary-grid" style="margin-bottom: 1.25rem;">
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card minimalist">
              <div class="stat-label">총 미수금액수</div>
              <div class="stat-value" style="color: #f56c6c;">{{ formatPrice(receivableSummary.totalUnpaid) }}<span class="unit">원</span></div>
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card minimalist">
              <div class="stat-label">당월 발생 미수금</div>
              <div class="stat-value">{{ formatPrice(receivableSummary.currentMonthUnpaid) }}<span class="unit">원</span></div>
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card minimalist">
              <div class="stat-label">당월 청구액</div>
              <div class="stat-value">{{ formatPrice(receivableSummary.currentMonthCharge) }}<span class="unit">원</span></div>
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card minimalist">
              <div class="stat-label">당월 수납액</div>
              <div class="stat-value" style="color: #67c23a;">{{ formatPrice(receivableSummary.currentMonthDeposit) }}<span class="unit">원</span></div>
            </div>
          </el-col>
        </el-row>

        <el-card shadow="never">
          <base-table :data="receivableSummary.retailerSummaries" border style="width: 100%">
            <el-table-column prop="companyName" label="소매점명" />
            <el-table-column prop="totalUnpaid" label="총 미수금" align="right" :excel-formatter="row => formatPrice(row.totalUnpaid)">
              <template #default="{row}">{{ formatPrice(row.totalUnpaid) }}</template>
            </el-table-column>
            <el-table-column prop="currentMonthCharge" label="당월 청구" align="right" :excel-formatter="row => formatPrice(row.currentMonthCharge)">
              <template #default="{row}">{{ formatPrice(row.currentMonthCharge) }}</template>
            </el-table-column>
            <el-table-column prop="currentMonthDeposit" label="당월 수납" align="right" :excel-formatter="row => formatPrice(row.currentMonthDeposit)">
              <template #default="{row}">{{ formatPrice(row.currentMonthDeposit) }}</template>
            </el-table-column>
          </base-table>
        </el-card>
      </section>

      <section class="section-container">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.common.marketGoldPrice') }}</h2>
        </div>
        <gold-price-widget />
      </section>

      <section class="section-container">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.common.marketDiamondPrice') }}</h2>
        </div>
        <diamond-price-widget />
      </section>

      <section class="section-container">
        <el-row :gutter="24">
          <el-col v-for="(val, key) in statusKeys" :key="key" :span="6" :xs="12">
            <div class="status-indicator-card">
              <div class="indicator-header">{{ $t('dashboard.logistics.status.' + key) }}</div>
              <div class="indicator-count">{{ statusStats[key] }}</div>
              <div class="indicator-footer">
                <el-link type="primary" underline="never">{{ $t('dashboard.logistics.viewDetails') }} <el-icon><ArrowRight /></el-icon></el-link>
              </div>
            </div>
          </el-col>
        </el-row>
      </section>

      <section class="section-container">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.common.orderTrends') }}</h2>
        </div>
        <div class="chart-container-card">
          <div class="chart-body">
            <company-order-line-chart />
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted, computed } from 'vue';
import { mapState } from 'pinia';
import PanThumb from '@/components/PanThumb/index.vue';
import GoldPriceWidget from '../components/GoldPriceWidget.vue';
import DiamondPriceWidget from '../components/DiamondPriceWidget.vue';
import CompanyOrderLineChart from '../components/CompanyOrderLineChart.vue';
import BaseTable from '@/components/BaseTable/index.vue';
import PartnerRetailerSummary from '@/components/PartnerRetailerSummary.vue';
import { Refresh, ArrowRight } from '@element-plus/icons-vue';
import store from '@/store';
import { fetchStockSummary } from '@/api/stock';
import { getLogisticsReceivableSummary } from '@/api/receivable';
import useUserStore from '@/store/modules/user';
import { formatPrice } from '@/utils/format';

const userStore = useUserStore();
const name = computed(() => userStore.name);
const avatar = computed(() => userStore.avatar);
const roles = computed(() => userStore.roles);
const companyId = computed(() => userStore.companyId);

const loading = ref(false);
const partnerSummaryRef = ref<any>(null);
const summary = reactive({
  total14KWeight: 0,
  total18KWeight: 0,
  totalPureGoldWeight: 0,
  totalCalculatedPureGoldWeight: 0
});

const receivableSummary = reactive({
  totalUnpaid: 0,
  currentMonthCharge: 0,
  currentMonthDeposit: 0,
  currentMonthUnpaid: 0
});

const statusKeys = ['pendingReceipt', 'inInspection', 'pendingRelease', 'inDelivery'];
const statusStats = reactive({
  pendingReceipt: 0,
  inInspection: 0,
  pendingRelease: 0,
  inDelivery: 0
});

const getDirectStockSummary = async () => {
  loading.value = true;
  try {
    const listQuery = {
      logisticsCompanyId: companyId.value || undefined,
      isDirectManagement: true
    };
    const res = await fetchStockSummary(listQuery);
    Object.assign(summary, res.data);
  } catch (error) {
    console.error('Failed to fetch stock summary', error);
  } finally {
    loading.value = false;
  }
};

const getReceivableSummaryData = async () => {
  try {
    const res: any = await getLogisticsReceivableSummary();
    if (res.data) {
      Object.assign(receivableSummary, res.data);

      if (receivableSummary.retailerSummaries) {
        receivableSummary.retailerSummaries.sort((a: any, b: any) => {
          if (b.totalUnpaid !== a.totalUnpaid) return b.totalUnpaid - a.totalUnpaid;
          if (b.currentMonthCharge !== a.currentMonthCharge) return b.currentMonthCharge - a.currentMonthCharge;
          return b.currentMonthDeposit - a.currentMonthDeposit;
        });
      }
    }
  } catch (error) {
    console.error('Failed to fetch receivable summary', error);
  }
};

let pollingInterval: any = null;

const refreshData = () => {
  getDirectStockSummary();
  getReceivableSummaryData();
  if (partnerSummaryRef.value) {
    partnerSummaryRef.value.fetchData();
  }
};

onMounted(() => {
  refreshData();

  pollingInterval = setInterval(refreshData, 120000);
});

onUnmounted(() => {
  if (pollingInterval) {
    clearInterval(pollingInterval);
  }
});
</script>

<style lang="scss" src="./DashboardLogisticsStyles.scss" scoped></style>

