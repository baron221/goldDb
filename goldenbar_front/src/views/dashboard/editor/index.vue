<template>
<div class="dashboard-retailer-page">

    <div class="dashboard-header-banner">
      <div class="header-content-wrapper">
        <div class="user-profile-area">
          <pan-thumb :image="avatar" class="jovenca-thumb">
            <span v-for="item in roles" :key="item" class="role-tag">{{ item }}</span>
          </pan-thumb>
          <div class="welcome-text">
            <h1 class="display-name">{{ name }}</h1>
            <p class="dashboard-title">{{ $t('dashboard.retailer.title') }}</p>
          </div>
        </div>
        <div class="header-actions">
          <el-button link :icon="Refresh" @click="fetchStats" class="refresh-btn">{{ $t('dashboard.common.refreshData') }}</el-button>
        </div>
      </div>
    </div>

    <div class="dashboard-body-content">

      <section class="section-container">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.retailer.summary') }}</h2>
        </div>

        <el-row :gutter="24" class="summary-grid">
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card minimalist" @click="navigateToStockReport('dailySales')">
              <div class="stat-label">{{ $t('dashboard.retailer.dailySales') }}</div>
              <div class="stat-value">{{ stats.dailySalesCount || 0 }}<span class="unit">{{ $t('dashboard.common.units.qty') }}</span></div>
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card minimalist" @click="navigateToStockReport('dailyPurchase')">
              <div class="stat-label">{{ $t('dashboard.retailer.dailyPurchase') }}</div>
              <div class="stat-value">{{ stats.dailyPurchaseCount || 0 }}<span class="unit">{{ $t('dashboard.common.units.qty') }}</span></div>
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card minimalist" @click="navigateToStockReport('monthlySales')">
              <div class="stat-label">{{ $t('dashboard.retailer.monthlySales') }}</div>
              <div class="stat-value">{{ stats.monthlySalesCount || 0 }}<span class="unit">{{ $t('dashboard.common.units.qty') }}</span></div>
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card highlight-gold" @click="navigateToStockReport('monthlyPurchase')">
              <div class="stat-label">{{ $t('dashboard.retailer.monthlyPurchase') }}</div>
              <div class="stat-value">{{ stats.monthlyPurchaseCount || 0 }}<span class="unit">{{ $t('dashboard.common.units.qty') }}</span></div>
            </div>
          </el-col>
        </el-row>
      </section>

      <section class="section-container">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.retailer.workflow') }}</h2>
        </div>
        <retail-panel-group v-if="logisticsCompanyId" :stats="stats" @handleSetLineChartData="handleSetLineChartData" />
        <div v-else class="no-logistics-alert">
          <el-alert
            title="관리 물류업체가 지정되지 않았습니다."
            type="warning"
            description="현재 귀하의 업체에 할당된 관리 물류업체가 없습니다. 정확한 정산 및 배송 정보 확인을 위해 관리자에게 물류업체 지정을 요청해 주세요."
            show-icon
            :closable="false"
          />
        </div>
      </section>

      <section class="section-container">
        <div class="section-header">
          <h2 class="section-title">자사 미수금 현황</h2>
        </div>

        <el-row :gutter="24" class="summary-grid">
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card minimalist">
              <div class="stat-label">총 미수금액</div>
              <div class="stat-value" style="color: #f56c6c;">{{ formatPrice(stats.totalUnpaid) }}<span class="unit">원</span></div>
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card minimalist">
              <div class="stat-label">당월 발생 미수금</div>
              <div class="stat-value">{{ formatPrice(stats.currentMonthUnpaid) }}<span class="unit">원</span></div>
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card minimalist">
              <div class="stat-label">당월 청구액</div>
              <div class="stat-value">{{ formatPrice(stats.currentMonthCharge) }}<span class="unit">원</span></div>
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <div class="stat-card minimalist">
              <div class="stat-label">당월 수납액</div>
              <div class="stat-value" style="color: #67c23a;">{{ formatPrice(stats.currentMonthDeposit) }}<span class="unit">원</span></div>
            </div>
          </el-col>
        </el-row>
      </section>

      <section class="section-container">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.retailer.inventory') }}</h2>
        </div>
        <div class="inventory-table-card">
          <base-table :data="stats.inventoryByCategory || []" style="width: 100%" border class="custom-inventory-table">
            <el-table-column prop="categoryName" :label="$t('dashboard.retailer.table.category')" align="center" />
            <el-table-column prop="count" :label="$t('dashboard.retailer.table.stockQty')" align="center" :excel-formatter="row => `${row.count} ${$t('dashboard.common.units.ea')}`">
              <template #default="{row}">
                <span class="inventory-count">{{ row.count }} <span class="unit">{{ $t('dashboard.common.units.ea') }}</span></span>
              </template>
            </el-table-column>
          </base-table>
        </div>
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
import PanThumb from '@/components/PanThumb';
import GoldPriceWidget from '../components/GoldPriceWidget.vue';
import DiamondPriceWidget from '../components/DiamondPriceWidget.vue';
import CompanyOrderLineChart from '../components/CompanyOrderLineChart.vue';
import RetailPanelGroup from './components/RetailPanelGroup.vue';
import BaseTable from '@/components/BaseTable/index.vue';
import { Refresh } from '@element-plus/icons-vue';
import { defineComponent } from 'vue';
import { useRouter } from 'vue-router';
import store from '@/store';
import { getRetailStats } from '@/api/dashboard';

export default defineComponent({
  name: 'DashboardEditor',
  components: { PanThumb, GoldPriceWidget, DiamondPriceWidget, CompanyOrderLineChart, RetailPanelGroup, BaseTable },
  setup() {
    const router = useRouter();
    return { Refresh, router };
  },
  data() {
    return {
      stats: {
        totalOrderCount: 0,
        inProductionCount: 0,
        settlingCount: 0,
        inDeliveryCount: 0,
        receivedCount: 0,
        dailySalesCount: 0,
        dailyPurchaseCount: 0,
        monthlySalesCount: 0,
        monthlyPurchaseCount: 0,
        inventoryByCategory: [],
        totalUnpaid: 0,
        currentMonthCharge: 0,
        currentMonthDeposit: 0,
        currentMonthUnpaid: 0
      },
      pollingInterval: null
    };
  },
  computed: {
    ...mapState(store.user, [
      'name',
      'avatar',
      'roles',
      'logisticsCompanyId'
    ])
  },
  created() {
    this.fetchStats();
  },
  mounted() {

    this.pollingInterval = setInterval(() => {
      this.fetchStats();
    }, 120000);
  },
  beforeUnmount() {
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
    }
  },
  methods: {
    formatPrice(price) {
      return (Math.floor(price || 0)).toLocaleString();
    },

    async fetchStats() {
      try {
        const res = await getRetailStats();
        this.stats = Object.assign(this.stats, res.data);
      } catch (error) {
        console.error('Failed to fetch retail stats:', error);
      }
    },

    handleSetLineChartData(type) {
      let status = '';
      let excludeCompleted = true;

      switch (type) {
        case 'total':
          status = '';
          excludeCompleted = false; 
          break;
        case 'production':

          status = 'LogisticsApproved,FactoryRequested,InspectedRequested,Inspected';
          break;
        case 'settling':

          status = 'PENDING,PROCESSING,SETTLED';
          break;
        case 'delivery':

          status = 'DELIVERY_READY,DELIVERY_IN_TRANSIT,DELIVERED';
          break;
        case 'received':

          status = 'Completed';
          excludeCompleted = false; 
          break;
      }

      this.router.push({
        path: '/my/order',
        query: {
          status: status,
          excludeCompleted: excludeCompleted ? 'true' : 'false'
        }
      });
    },
    navigateToStockReport(type) {
      const today = new Date();
      const format = (d) => {
        const yyyy = d.getFullYear();
        const mm = String(d.getMonth() + 1).padStart(2, '0');
        const dd = String(d.getDate()).padStart(2, '0');
        return `${yyyy}-${mm}-${dd}T00:00:00`;
      };

      const startOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);

      let query = {};
      switch (type) {
        case 'dailySales':
          query = {
            status: 'SOLD',
            exhaustionDateStart: format(today),
            exhaustionDateEnd: `${format(today).substring(0, 10)}T23:59:59`
          };
          break;
        case 'dailyPurchase':
          query = {
            createdAtStart: format(today),
            createdAtEnd: `${format(today).substring(0, 10)}T23:59:59`
          };
          break;
        case 'monthlySales':
          query = {
            status: 'SOLD',
            exhaustionDateStart: format(startOfMonth),
            exhaustionDateEnd: `${format(today).substring(0, 10)}T23:59:59`
          };
          break;
        case 'monthlyPurchase':
          query = {
            createdAtStart: format(startOfMonth),
            createdAtEnd: `${format(today).substring(0, 10)}T23:59:59`
          };
          break;
      }

      this.router.push({
        path: '/stock/index',
        query
      });
    }
  }
});
</script>

<style lang="scss" scoped>
@import "./EditorDashboardStyles.scss";
</style>

