<template>
<el-row :gutter="24" class="panel-group">
    <el-col :xs="24" :sm="12" :lg="4" :xl="4" class="card-panel-col">
      <div class="stat-card minimalist" @click="handleSetLineChartData('total')">
        <div class="stat-label">{{ $t('dashboard.retailer.tabs.total') }}</div>
        <div class="stat-value">
          <count-to :start-val="0" :end-val="stats.totalOrderCount" :duration="2000" />
          <span class="unit">{{ $t('dashboard.common.units.qty') }}</span>
        </div>
      </div>
    </el-col>
    <el-col :xs="24" :sm="12" :lg="5" :xl="5" class="card-panel-col">
      <div class="stat-card minimalist" @click="handleSetLineChartData('production')">
        <div class="stat-label">{{ $t('dashboard.retailer.tabs.production') }}</div>
        <div class="stat-value">
          <count-to :start-val="0" :end-val="stats.inProductionCount" :duration="2200" />
          <span class="unit">{{ $t('dashboard.common.units.qty') }}</span>
        </div>
      </div>
    </el-col>
    <el-col :xs="24" :sm="12" :lg="5" :xl="5" class="card-panel-col">
      <div class="stat-card highlight-gold" @click="handleSetLineChartData('settling')">
        <div class="stat-label">{{ $t('dashboard.retailer.tabs.settling') }}</div>
        <div class="stat-value">
          <count-to :start-val="0" :end-val="stats.settlingCount" :duration="2400" />
          <span class="unit">{{ $t('dashboard.common.units.qty') }}</span>
        </div>
      </div>
    </el-col>
    <el-col :xs="24" :sm="12" :lg="5" :xl="5" class="card-panel-col">
      <div class="stat-card minimalist" @click="handleSetLineChartData('delivery')">
        <div class="stat-label">{{ $t('dashboard.retailer.tabs.delivery') }}</div>
        <div class="stat-value">
          <count-to :start-val="0" :end-val="stats.inDeliveryCount" :duration="2600" />
          <span class="unit">{{ $t('dashboard.common.units.qty') }}</span>
        </div>
      </div>
    </el-col>
    <el-col :xs="24" :sm="12" :lg="5" :xl="5" class="card-panel-col">
      <div class="stat-card minimalist" @click="handleSetLineChartData('received')">
        <div class="stat-label">{{ $t('dashboard.retailer.tabs.received') }}</div>
        <div class="stat-value">
          <count-to :start-val="0" :end-val="stats.receivedCount" :duration="2800" />
          <span class="unit">{{ $t('dashboard.common.units.qty') }}</span>
        </div>
      </div>
    </el-col>
  </el-row>
</template>

<script setup>
import CountTo from '@/components/vue-count-to';

const props = defineProps({
  stats: {
    type: Object,
    default: () => ({
      totalOrderCount: 0,
      inProductionCount: 0,
      settlingCount: 0,
      inDeliveryCount: 0,
      receivedCount: 0
    })
  }
});

const emit = defineEmits(['handleSetLineChartData']);

const handleSetLineChartData = (type) => {
  emit('handleSetLineChartData', type);
};
</script>

<style lang="scss" scoped>
.panel-group {
  margin-top: 1.125rem;

  .card-panel-col {
    margin-bottom: 1.5rem;
  }
}

.stat-card {
  background-color: #ffffff;
  border: 1px solid #eae6df;
  padding: 1.5625rem;
  border-radius: 2px;
  height: 100%;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.02);
  cursor: pointer;

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 12px 30px rgba(197, 168, 128, 0.1);
    border-color: #c5a880;
  }

  .stat-label {
    font-size: 0.8875rem;
    font-weight: 600;
    color: #888888;
    text-transform: uppercase;
    letter-spacing: 1px;
    margin-bottom: 0.95rem;
  }

  .stat-value {
    font-size: 1.5rem;
    font-weight: 700;

    .unit { font-size: 0.875rem; font-weight: 400; margin-left: 0.25rem; color: #888888; }
  }

  &.highlight-gold {
    background-color: #f7f5f0;
    border-left: 3px solid #c5a880;
    .stat-value { color: #c5a880; }
  }
}

@media (max-width: 991px) {
  .card-panel-col {
    margin-bottom: 1.25rem;
  }
}
</style>

