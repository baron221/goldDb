<template>
<div class="dashboard-admin-page">

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
            <p class="dashboard-title">{{ $t('dashboard.admin.title') }}</p>
          </div>
        </div>
        <div class="header-actions">
          <el-button link :icon="Refresh" @click="fetchDashboardData" class="refresh-btn">{{ $t('dashboard.common.refreshData') }}</el-button>
        </div>
      </div>
    </div>

    <div class="dashboard-body-content">

      <admin-stat-group :data="adminStats" v-loading="loading" />

      <section class="section-container">
        <transaction-trend-chart
          :daily-data="adminStats.dailyTrends"
          :monthly-data="adminStats.monthlyTrends"
          v-loading="loading"
        />
      </section>

      <section class="section-container">
        <distribution-charts
          :order-distribution="adminStats.orderStatusDistribution"
          :stock-distribution="adminStats.stockStatusDistribution"
          v-loading="loading"
        />
      </section>

      <section class="section-container">
        <company-analytics
          :top-retailers="adminStats.topRetailers"
          :top-manufacturers="adminStats.topManufacturers"
          v-loading="loading"
        />
      </section>

      <section class="section-container">
        <product-performance-table
          :top-products="adminStats.topSellingProducts"
          :category-performance="adminStats.categoryPerformance"
          v-loading="loading"
        />
      </section>

      <section class="section-container">
        <recent-activity-table
          :recent-orders="adminStats.recentOrders"
          :recent-logins="adminStats.recentLogins"
          v-loading="loading"
        />
      </section>

      <el-row :gutter="24">
        <el-col :xs="24" :lg="24">
          <section class="section-container">
            <div class="section-header">
              <h2 class="section-title">{{ $t('dashboard.admin.labels.goldPrice') }}</h2>
            </div>
            <gold-price-widget />
          </section>
        </el-col>
        <el-col :xs="24" :lg="24">
          <section class="section-container">
            <div class="section-header">
              <h2 class="section-title">{{ $t('dashboard.admin.labels.diamondPrice') }}</h2>
            </div>
            <diamond-price-widget />
          </section>
        </el-col>
      </el-row>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, reactive, computed } from 'vue';
import AdminStatGroup from './components/AdminStatGroup.vue';
import TransactionTrendChart from './components/TransactionTrendChart.vue';
import DistributionCharts from './components/DistributionCharts.vue';
import CompanyAnalytics from './components/CompanyAnalytics.vue';
import ProductPerformanceTable from './components/ProductPerformanceTable.vue';
import RecentActivityTable from './components/RecentActivityTable.vue';
import GoldPriceWidget from '../components/GoldPriceWidget.vue';
import DiamondPriceWidget from '../components/DiamondPriceWidget.vue';
import PanThumb from '@/components/PanThumb/index.vue';
import { Refresh } from '@element-plus/icons-vue';
import { getAdminStats } from '@/api/dashboard';
import useUserStore from '@/store/modules/user';

const userStore = useUserStore();
const name = computed(() => userStore.name);
const avatar = computed(() => userStore.avatar);
const roles = computed(() => userStore.roles);

const loading = ref(true);
const adminStats = reactive({
  totalUsers: 0,
  totalCompanies: 0,
  totalProducts: 0,
  totalOrders: 0,
  totalRevenue: 0,
  pendingApprovalCount: 0,
  unassignedCompanyUserCount: 0,
  unassignedLogisticsRetailerCount: 0,
  dailyTrends: [],
  monthlyTrends: [],
  topSellingProducts: [],
  categoryPerformance: [],
  topRetailers: [],
  topManufacturers: [],
  orderStatusDistribution: [],
  stockStatusDistribution: [],
  recentOrders: [],
  recentLogins: []
});

const fetchDashboardData = async () => {
  loading.value = true;
  try {
    const res = await getAdminStats();
    Object.assign(adminStats, res.data);
  } catch (error) {
    console.error('Failed to fetch admin dashboard stats');
  } finally {
    loading.value = false;
  }
};

let pollingInterval: any = null;

onMounted(() => {
  fetchDashboardData();

  pollingInterval = setInterval(fetchDashboardData, 120000);
});

onUnmounted(() => {
  if (pollingInterval) {
    clearInterval(pollingInterval);
  }
});
</script>

<style lang="scss" scoped>
@import url('https://fonts.googleapis.com/css2?family=Jost:wght@300;400;500;600;700&display=swap');

.dashboard-admin-page {
  font-family: 'S-CoreDream', 'Jost', sans-serif;
  min-height: 100vh;
  padding-bottom: 3.75rem;
}

.dashboard-header-banner {
  background-color: #ffffff;
  border-bottom: 1px solid #eae6df;
  padding: 2.5rem 0;
  margin-bottom: 2.5rem;

  .header-content-wrapper {
    // max-width: 1400px;
    margin: 0 auto;
    padding: 0 1.875rem;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
}

.user-profile-area {
  display: flex;
  align-items: center;
  gap: 1.875rem;

  .jovenca-thumb {
    width: 100px !important;
    height: 100px !important;
    border: 2px solid #c5a880;
    box-shadow: 0 8px 20px rgba(197, 168, 128, 0.15);
  }

  .welcome-text {
    .display-name {
      font-size: 2rem;
      font-weight: 600;

      margin: 0 0 0.25rem 0;
      text-transform: uppercase;
      letter-spacing: 1px;
    }
    .dashboard-title {
      font-size: 0.875rem;
      color: #888888;
      margin: 0;
      font-weight: 300;
      text-transform: uppercase;
      letter-spacing: 2px;
    }
  }
}

.role-tag {
  font-size: 0.825rem;
  font-weight: 700;
  color: #c5a880;
  display: block;
  text-transform: uppercase;
}

.refresh-btn {
  font-size: 0.8875rem;
  font-weight: 700;
  letter-spacing: 1px;
  color: #c5a880;
  &:hover { }
}

.dashboard-body-content {
  // max-width: 1400px;
  margin: 0 auto;
  padding: 0 1.875rem;
}

.section-container {
  margin-bottom: 2.5rem;
}

.section-header {
  margin-bottom: 0.9375rem;
  .section-title {
    font-size: 0.9375rem;
    font-weight: 600;

    text-transform: uppercase;
    letter-spacing: 1px;
    position: relative;
    padding-left: 0.9375rem;
    line-height: 1.2;
    &::after {
      content: '';
      position: absolute;
      left: 0;
      top: 0;
      bottom: 0;
      width: 3px;
      background-color: #c5a880;
    }
  }
}

.analytics-card-luxury, .luxury-chart-box, .luxury-table-card, .luxury-list-card, .luxury-box-card {
  background-color: #ffffff;
  border: 1px solid #eae6df;
  padding: 1.5rem;
  border-radius: 2px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.02);
}

.analytics-card-luxury {
  padding-top: 2.5rem;
}

@media (max-width: 991px) {
  .dashboard-header-banner {
    padding: 1.875rem 0;
  }
  .user-profile-area {
    gap: 1.25rem;
  }
  .welcome-text .display-name {
    font-size: 1.5rem;
  }
}
</style>

