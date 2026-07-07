<template>
<div class="trend-chart-container">
    <div class="section-header">
      <h2 class="section-title">{{ title || $t('dashboard.admin.performance.transactionTrends') }}</h2>
      <div class="chart-actions">
        <el-radio-group v-model="viewMode" size="small">
          <el-radio-button value="daily">{{ $t('dashboard.admin.performance.daily') }}</el-radio-button>
          <el-radio-button value="monthly">{{ $t('dashboard.admin.performance.monthly') }}</el-radio-button>
        </el-radio-group>
      </div>
    </div>
    <div ref="chartRef" class="chart-canvas" style="height: 350px;"></div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed, onUnmounted } from 'vue';
import * as echarts from 'echarts';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps({
  dailyData: {
    type: Array,
    required: true
  },
  monthlyData: {
    type: Array,
    required: true
  },
  title: {
    type: String,
    default: ''
  }
});

const chartRef = ref<HTMLElement | null>(null);
let chart: echarts.ECharts | null = null;
const viewMode = ref('daily');

const currentData = computed(() => {
  return viewMode.value === 'daily' ? props.dailyData : props.monthlyData;
});

const initChart = () => {
  if (!chartRef.value) return;

  chart = echarts.init(chartRef.value);
  updateChart();

  window.addEventListener('resize', handleResize);
};

const updateChart = () => {
  if (!chart) return;

  const labels = currentData.value.map((item: any) => item.label);
  const counts = currentData.value.map((item: any) => item.orderCount);
  const amounts = currentData.value.map((item: any) => item.totalAmount);

  const option = {
    tooltip: {
      trigger: 'axis',
      axisPointer: { type: 'cross' }
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '3%',
      containLabel: true
    },
    legend: {
      data: [t('dashboard.admin.performance.orders'), t('dashboard.admin.performance.revenue')],
      bottom: 0
    },
    xAxis: [
      {
        type: 'category',
        data: labels,
        axisPointer: { type: 'shadow' },
        axisLabel: { color: '#888', fontSize: 10 }
      }
    ],
    yAxis: [
      {
        type: 'value',
        name: t('dashboard.admin.performance.qty'),
        min: 0,
        axisLabel: { formatter: '{value}' }
      },
      {
        type: 'value',
        name: t('dashboard.admin.performance.revenue'),
        axisLabel: {
          formatter: (value: number) => {
            if (value >= 1000000) return (value / 1000000).toFixed(1) + 'M';
            if (value >= 1000) return (value / 1000).toFixed(0) + 'K';
            return value;
          }
        }
      }
    ],
    series: [
      {
        name: t('dashboard.admin.performance.orders'),
        type: 'bar',
        data: counts,
        itemStyle: { color: '#c5a880' },
        barWidth: '40%'
      },
      {
        name: t('dashboard.admin.performance.revenue'),
        type: 'line',
        yAxisIndex: 1,
        data: amounts,
        symbolSize: 8,
        itemStyle: { color: '#222' },
        lineStyle: { width: 3 }
      }
    ]
  };

  chart.setOption(option);
};

const handleResize = () => {
  chart?.resize();
};

watch(currentData, () => {
  updateChart();
});

onMounted(() => {
  initChart();
});

onUnmounted(() => {
  window.removeEventListener('resize', handleResize);
  chart?.dispose();
});
</script>

<style lang="scss" scoped>
.trend-chart-container {
  border: 1px solid #eae6df;
  padding: 1.5625rem;
  border-radius: 2px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.02);
  height: 100%;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5625rem;

  .section-title {
    font-size: 0.9375rem;
    font-weight: 600;

    text-transform: uppercase;
    letter-spacing: 1px;
    position: relative;
    padding-left: 0.9375rem;
    line-height: 1.2;
    margin: 0;
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

:deep(.el-radio-button__inner) {
  border-radius: 0 !important;
  font-size: 0.825rem;
  font-weight: 700;
  letter-spacing: 1px;
  padding: 0.5rem 0.9375rem;
}
</style>

