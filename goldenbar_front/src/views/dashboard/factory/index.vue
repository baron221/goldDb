<template>
<div class="dashboard-factory-page">

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
            <p class="dashboard-title">{{ $t('dashboard.factory.title') }}</p>
          </div>
        </div>
        <div class="header-actions">
          <el-button link :icon="Refresh" @click="fetchData" class="refresh-btn">{{ $t('dashboard.common.refreshData') }}</el-button>
        </div>
      </div>
    </div>

    <div class="dashboard-body-content">

      <section class="section-container">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.factory.productionSummary') }}</h2>
        </div>

        <el-row :gutter="24" class="summary-grid">
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card highlight-gold clickable" @click="$router.push({ path: '/order/factory-request', query: { status: 'FactoryRequested' }})">
              <div class="stat-label">{{ $t('dashboard.factory.newRequests') }}</div>
              <div class="stat-value">{{ stats.newRequestCount || 0 }}<span class="unit">{{ $t('dashboard.common.units.qty') }}</span></div>
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card minimalist clickable" @click="$router.push({ path: '/order/factory-request', query: { status: 'WorkOrderCreated' }})">
              <div class="stat-label">{{ $t('dashboard.factory.inProduction') }}</div>
              <div class="stat-value">{{ stats.inProductionCount || 0 }}<span class="unit">{{ $t('dashboard.common.units.qty') }}</span></div>
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card minimalist clickable" @click="$router.push({ path: '/order/factory-request', query: { status: 'InspectedRequested' }})">
              <div class="stat-label">{{ $t('dashboard.factory.completedTasks') }}</div>
              <div class="stat-value">{{ stats.completedCount || 0 }}<span class="unit">{{ $t('dashboard.common.units.qty') }}</span></div>
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card minimalist clickable" @click="$router.push({ path: '/order/factory-request', query: { status: 'Post_Inspected_Group' }})">
              <div class="stat-label">{{ $t('dashboard.factory.shippedToday') }}</div>
              <div class="stat-value">{{ stats.shippedCount || 0 }}<span class="unit">{{ $t('dashboard.common.units.qty') }}</span></div>
            </div>
          </el-col>
        </el-row>
      </section>

      <section class="section-container">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.factory.productSummary') || '제품 현황' }}</h2>
        </div>

        <el-row :gutter="24" class="summary-grid">
          <el-col :xs="24" :sm="12" :md="8">
            <div class="stat-card highlight-blue clickable" @click="$router.push({ path: '/product/list' })">
              <div class="stat-label">{{ $t('dashboard.factory.regularProducts') || '일반 제품' }}</div>
              <div class="stat-value">{{ stats.regularProductCount || 0 }}<span class="unit">{{ $t('dashboard.common.units.items') }}</span></div>
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="8">
            <div class="stat-card highlight-blue clickable" @click="$router.push({ path: '/product/set-list' })">
              <div class="stat-label">{{ $t('dashboard.factory.setProducts') || '세트 제품' }}</div>
              <div class="stat-value">{{ stats.setProductCount || 0 }}<span class="unit">{{ $t('dashboard.common.units.items') }}</span></div>
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="8">
            <div class="stat-card highlight-blue">
              <div class="stat-label">{{ $t('dashboard.factory.totalProducts') || '총 제품 수' }}</div>
              <div class="stat-value">{{ (stats.regularProductCount || 0) + (stats.setProductCount || 0) }}<span class="unit">{{ $t('dashboard.common.units.items') }}</span></div>
            </div>
          </el-col>
        </el-row>
      </section>

      <section class="section-container">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.factory.categoryDistribution') || '분류별 제품 현황' }}</h2>
        </div>
        <div class="inventory-table-card">
          <category-distribution-chart :chart-data="stats.productTrendByCategory" :dates="stats.recentDates" title="제품등록 현황" />
        </div>
      </section>

      <section class="section-container">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.factory.orderCategoryDistribution') || '분류별 주문 현황' }}</h2>
        </div>
        <div class="inventory-table-card">
          <category-distribution-chart :chart-data="stats.orderTrendByCategory" :dates="stats.recentDates" title="주문 현황" />
        </div>
      </section>

      <section class="section-container">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.factory.recentUrgent') }}</h2>
        </div>
        <div class="inventory-table-card">
          <base-table :data="stats.recentItems" style="width: 100%" border class="custom-factory-table">
            <el-table-column prop="orderNo" :label="$t('dashboard.factory.table.orderNo')" width="160" />
            <el-table-column prop="productName" :label="$t('dashboard.factory.table.productName')" min-width="200" />
            <el-table-column prop="quantity" :label="$t('dashboard.factory.table.qty')" width="80" align="center" />
            <el-table-column prop="requestedWeight" :label="$t('dashboard.factory.table.weight')" width="120" align="right" :excel-formatter="row => `${row.requestedWeight} ${$t('dashboard.common.units.g')}`">
              <template #default="scope">
                <span class="weight-text">{{ scope.row.requestedWeight }} <span class="unit">{{ $t('dashboard.common.units.g') }}</span></span>
              </template>
            </el-table-column>
            <el-table-column prop="status" :label="$t('dashboard.factory.table.status')" width="140" align="center" :excel-formatter="row => row.status === 'FactoryRequested' ? $t('dashboard.factory.newRequests') : $t('order.status.InspectedRequested')">
              <template #default="scope">
                <div :class="['status-badge', scope.row.status === 'FactoryRequested' ? 'urgent' : 'process']">
                  {{ scope.row.status === 'FactoryRequested' ? $t('dashboard.factory.newRequests') : $t('order.status.InspectedRequested') }}
                </div>
              </template>
            </el-table-column>
            <el-table-column prop="requestedAt" :label="$t('dashboard.factory.table.requestedAt')" width="180" align="center" :excel-formatter="row => formatDate(row.requestedAt)">
              <template #default="scope">
                <span class="date-text">{{ formatDate(scope.row.requestedAt) }}</span>
              </template>
            </el-table-column>
          </base-table>
          <div v-if="!stats.recentItems || stats.recentItems.length === 0" class="empty-data-hint">
            {{ $t('dashboard.factory.noData') }}
          </div>
        </div>
      </section>

      <section class="section-container">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.factory.insights') }}</h2>
        </div>
        <el-row :gutter="24">
          <el-col :span="12" :xs="24">
            <div class="insight-card">
              <div class="insight-label">{{ $t('dashboard.factory.weeklyRequests') }}</div>
              <div class="insight-value">{{ totalWeeklyRequests }} <span class="unit">{{ $t('dashboard.common.units.items') }}</span></div>
            </div>
          </el-col>
          <el-col :span="12" :xs="24">
            <div class="insight-card">
              <div class="insight-label">{{ $t('dashboard.factory.weeklyCompletions') }}</div>
              <div class="insight-value">{{ totalWeeklyCompletions }} <span class="unit">{{ $t('dashboard.common.units.items') }}</span></div>
            </div>
          </el-col>
        </el-row>
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

<script>
import { mapState } from 'pinia';
import PanThumb from '@/components/PanThumb/index.vue';
import GoldPriceWidget from '../components/GoldPriceWidget.vue';
import DiamondPriceWidget from '../components/DiamondPriceWidget.vue';
import CompanyOrderLineChart from '../components/CompanyOrderLineChart.vue';
import CategoryDistributionChart from './components/CategoryDistributionChart.vue';
import BaseTable from '@/components/BaseTable/index.vue';
import { Refresh } from '@element-plus/icons-vue';
import { defineComponent } from 'vue';
import store from '@/store';
import { getFactoryStats } from '@/api/dashboard';
import { parseTime } from '@/utils';

export default defineComponent({
  name: 'DashboardFactory',
  components: { PanThumb, GoldPriceWidget, DiamondPriceWidget, CompanyOrderLineChart, CategoryDistributionChart, BaseTable },
  setup() {
    return { Refresh };
  },
  data() {
    return {
      stats: {
        newRequestCount: 0,
        inProductionCount: 0,
        completedCount: 0,
        shippedCount: 0,
        regularProductCount: 0,
        setProductCount: 0,
        productByCategory: [],
        productTrendByCategory: [],
        orderTrendByCategory: [],
        recentItems: [],
        dailyRequestCounts: [],
        dailyCompletedCounts: []
      },
      pollingInterval: null
    };
  },
  computed: {
    ...mapState(store.user, [
      'name',
      'avatar',
      'roles'
    ]),

    totalWeeklyRequests() {
      return this.stats.dailyRequestCounts ? this.stats.dailyRequestCounts.reduce((a, b) => a + b, 0) : 0;
    },

    totalWeeklyCompletions() {
      return this.stats.dailyCompletedCounts ? this.stats.dailyCompletedCounts.reduce((a, b) => a + b, 0) : 0;
    }
  },
  created() {
    this.fetchData();
  },
  mounted() {

    this.pollingInterval = setInterval(() => {
      this.fetchData();
    }, 120000);
  },
  beforeUnmount() {
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
    }
  },
  methods: {

    async fetchData() {
      try {
        const res = await getFactoryStats();
        if (res.data) {
          this.stats = res.data;
        }
      } catch (error) {
        console.error('Failed to fetch factory stats:', error);
      }
    },

    formatDate(dateStr) {
      if (!dateStr) return '';
      return parseTime(new Date(dateStr), '{m}-{d} {h}:{i}');
    }
  }
});
</script>

<style lang="scss" scoped>
@import "./FactoryDashboardStyles.scss";
</style>

