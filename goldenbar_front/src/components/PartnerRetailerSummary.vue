<template>
<el-row :gutter="20" class="summary-row" v-loading="loading">

    <el-col :span="8">
      <div class="summary-group-box total">
        <div class="group-title">{{ $t('admin.partner.summary.total') }}</div>
        <div class="metrics-grid">
          <div class="m-item">
            <span class="m-label">{{ $t('admin.partner.summary.retailerCount') }}</span>
            <span class="m-value">{{ stats.total.count }}<small>{{ $t('admin.partner.summary.unitPlaces') }}</small></span>
          </div>
          <div class="m-item">
            <span class="m-label">{{ $t('admin.partner.summary.totalStock') }}</span>
            <span class="m-value">{{ formatNumber(stats.total.stockCount) }}<small>pcs</small></span>
          </div>
          <div class="m-item">
            <span class="m-label">{{ $t('admin.partner.summary.monthlyPerformance') }}</span>
            <span class="m-value">₩{{ formatPrice(stats.total.monthlyAmount) }}</span>
          </div>
          <div class="m-item">
            <span class="m-label">{{ $t('admin.partner.summary.pendingCount') }}</span>
            <span class="m-value danger">{{ formatNumber(stats.total.pendingCount) }}<small>{{ $t('admin.partner.table.pendingUnit') }}</small></span>
          </div>
        </div>
      </div>
    </el-col>

    <el-col :span="8">
      <div class="summary-group-box direct">
        <div class="group-title">{{ $t('admin.partner.summary.direct') }}</div>
        <div class="metrics-grid">
          <div class="m-item">
            <span class="m-label">{{ $t('admin.partner.summary.retailerCount') }}</span>
            <span class="m-value">{{ stats.direct.count }}<small>{{ $t('admin.partner.summary.unitPlaces') }}</small></span>
          </div>
          <div class="m-item">
            <span class="m-label">{{ $t('admin.partner.summary.totalStock') }}</span>
            <span class="m-value">{{ formatNumber(stats.direct.stockCount) }}<small>pcs</small></span>
          </div>
          <div class="m-item">
            <span class="m-label">{{ $t('admin.partner.summary.monthlyPerformance') }}</span>
            <span class="m-value gold">₩{{ formatPrice(stats.direct.monthlyAmount) }}</span>
          </div>
          <div class="m-item">
            <span class="m-label">{{ $t('admin.partner.summary.pendingCount') }}</span>
            <span class="m-value danger">{{ formatNumber(stats.direct.pendingCount) }}<small>{{ $t('admin.partner.table.pendingUnit') }}</small></span>
          </div>
        </div>
      </div>
    </el-col>

    <el-col :span="8">
      <div class="summary-group-box partner">
        <div class="group-title">{{ $t('admin.partner.summary.partner') }}</div>
        <div class="metrics-grid">
          <div class="m-item">
            <span class="m-label">{{ $t('admin.partner.summary.retailerCount') }}</span>
            <span class="m-value">{{ stats.partner.count }}<small>{{ $t('admin.partner.summary.unitPlaces') }}</small></span>
          </div>
          <div class="m-item">
            <span class="m-label">{{ $t('admin.partner.summary.totalStock') }}</span>
            <span class="m-value">{{ formatNumber(stats.partner.stockCount) }}<small>pcs</small></span>
          </div>
          <div class="m-item">
            <span class="m-label">{{ $t('admin.partner.summary.monthlyPerformance') }}</span>
            <span class="m-value blue">₩{{ formatPrice(stats.partner.monthlyAmount) }}</span>
          </div>
          <div class="m-item">
            <span class="m-label">{{ $t('admin.partner.summary.pendingCount') }}</span>
            <span class="m-value danger">{{ formatNumber(stats.partner.pendingCount) }}<small>{{ $t('admin.partner.table.pendingUnit') }}</small></span>
          </div>
        </div>
      </div>
    </el-col>
  </el-row>
</template>

<script setup>
import { ref, onMounted, computed, defineExpose } from 'vue';
import { getPartnerRetailerStats } from '@/api/dashboard';
import { formatPrice } from '@/utils/format';
import { ElMessage } from 'element-plus';

const loading = ref(false);
const retailerStats = ref([]);

const formatNumber = (n) => (n || 0).toLocaleString();

const fetchData = async () => {
  loading.value = true;
  try {
    const res = await getPartnerRetailerStats();
    retailerStats.value = res.data;
  } catch (error) {
    console.error('Failed to fetch partner stats:', error);
    ElMessage.error('데이터를 불러오는 데 실패했습니다.');
  } finally {
    loading.value = false;
  }
};

const directRetailersList = computed(() => retailerStats.value.filter(r => r.isDirectManagement));
const partnerRetailersList = computed(() => retailerStats.value.filter(r => !r.isDirectManagement));

const stats = computed(() => {
  const calc = (list) => ({
    count: list.length,
    stockCount: list.reduce((sum, r) => sum + r.totalStockCount, 0),
    stockWeight: list.reduce((sum, r) => sum + r.totalStockWeight, 0),
    monthlyAmount: list.reduce((sum, r) => sum + r.monthlyOrderAmount, 0),
    totalAmount: list.reduce((sum, r) => sum + r.totalOrderAmount, 0),
    pendingCount: list.reduce((sum, r) => sum + r.pendingOrderCount, 0)
  });

  return {
    total: calc(retailerStats.value),
    direct: calc(directRetailersList.value),
    partner: calc(partnerRetailersList.value)
  };
});

onMounted(() => {
  fetchData();
});

defineExpose({
  fetchData
});
</script>

<style lang="scss" scoped>
.summary-row {
  margin-bottom: 1.5625rem;
}

.summary-group-box {
  border: 1px solid #eae6df;
  border-radius: 2px;
  padding: 1.25rem;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.02);
  height: 100%;

  .group-title {
    font-size: 0.8875rem;
    font-weight: 800;
    color: #222;
    text-transform: uppercase;
    letter-spacing: 1.5px;
    margin-bottom: 0.9375rem;
    padding-bottom: 0.625rem;
    border-bottom: 1px solid #f7f5f0;
  }

  .metrics-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 0.9375rem;

    .m-item {
      display: flex;
      flex-direction: column;

      .m-label {
        font-size: 0.825rem;
        font-weight: 600;
        color: #bbb;
        text-transform: uppercase;
        margin-bottom: 0.125rem;
      }

      .m-value {
        font-size: 0.9375rem;
        font-weight: 700;
        color: #444;
        font-family: 'S-CoreDream','Jost', sans-serif;

        small {
          font-size: 0.825rem;
          font-weight: 400;
          margin-left: 0.125rem;
          color: #999;
        }
        &.gold {
          color: #c5a880;
        }
        &.blue {
          color: #409eff;
        }
        &.danger {
          color: #f56c6c;
        }
      }
    }
  }

  &.total {
    border-top: 3px solid #606266;
  }
  &.direct {
    border-top: 3px solid #c5a880;
  }
  &.partner {
    border-top: 3px solid #409eff;
  }
}
</style>

