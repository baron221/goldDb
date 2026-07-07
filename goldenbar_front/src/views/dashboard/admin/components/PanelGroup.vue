<template>
<el-row :gutter="24" class="panel-group">
    <el-col :xs="24" :sm="12" :lg="6" class="card-panel-col">
      <div class="stat-card minimalist" @click="handleSetLineChartData('newVisitis')">
        <div class="stat-label">{{ $t('dashboard.admin.users') }}</div>
        <div class="stat-value">
          <count-to :start-val="0" :end-val="stats.userCount" :duration="2600" />
        </div>
      </div>
    </el-col>
    <el-col :xs="24" :sm="12" :lg="6" class="card-panel-col">
      <div class="stat-card minimalist" @click="handleSetLineChartData('messages')">
        <div class="stat-label">{{ $t('dashboard.admin.roles') }}</div>
        <div class="stat-value">
          <count-to :start-val="0" :end-val="stats.roleCount" :duration="3000" />
        </div>
      </div>
    </el-col>
    <el-col :xs="24" :sm="12" :lg="6" class="card-panel-col">
      <div class="stat-card minimalist" @click="handleSetLineChartData('purchases')">
        <div class="stat-label">{{ $t('dashboard.admin.menus') }}</div>
        <div class="stat-value">
          <count-to :start-val="0" :end-val="stats.menuCount" :duration="3200" />
        </div>
      </div>
    </el-col>
    <el-col :xs="24" :sm="12" :lg="6" class="card-panel-col">
      <div class="stat-card highlight-gold" @click="handleSetLineChartData('shoppings')">
        <div class="stat-label">{{ $t('dashboard.admin.articles') }}</div>
        <div class="stat-value">
          <count-to :start-val="0" :end-val="stats.articleCount" :duration="3600" />
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
      userCount: 0,
      roleCount: 0,
      menuCount: 0,
      articleCount: 0
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

  }

  &.highlight-gold {
    background-color: #f7f5f0;
    border-left: 3px solid #c5a880;
    .stat-value { color: #c5a880; }
  }
}
</style>

