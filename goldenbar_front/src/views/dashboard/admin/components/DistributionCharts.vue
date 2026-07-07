<template>
<el-row :gutter="20" class="distribution-charts">
    <el-col :xs="24" :lg="12" style="">
      <div class="chart-box">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.admin.analytics.orderStatusDistribution') }}</h2>
        </div>
        <div ref="orderStatusChartRef" class="chart-canvas" style="height: 300px;"></div>
      </div>
    </el-col>
    <el-col :xs="24" :lg="12">
      <div class="chart-box">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.admin.analytics.stockStatusDistribution') }}</h2>
        </div>
        <div ref="stockStatusChartRef" class="chart-canvas" style="height: 300px;"></div>
      </div>
    </el-col>
  </el-row>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, onUnmounted } from 'vue';
import * as echarts from 'echarts';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps({
  orderDistribution: {
    type: Array,
    required: true
  },
  stockDistribution: {
    type: Array,
    required: true
  }
});

const orderStatusChartRef = ref<HTMLElement | null>(null);
const stockStatusChartRef = ref<HTMLElement | null>(null);
let orderChart: echarts.ECharts | null = null;
let stockChart: echarts.ECharts | null = null;

const initCharts = () => {
  if (orderStatusChartRef.value) {
    orderChart = echarts.init(orderStatusChartRef.value);
    updatePieChart(orderChart, props.orderDistribution, t('order.title'));
  }
  if (stockStatusChartRef.value) {
    stockChart = echarts.init(stockStatusChartRef.value);
    updatePieChart(stockChart, props.stockDistribution, t('stock.title'));
  }
  window.addEventListener('resize', handleResize);
};

const updatePieChart = (chartInstance: echarts.ECharts | null, data: any[], name: string) => {
  if (!chartInstance) return;

  const chartData = data.map((item: any) => ({
    value: item.count,
    name: t(`order.status.${item.status}`) !== `order.status.${item.status}` ? t(`order.status.${item.status}`) : (t(`stock.status.${item.status}`) !== `stock.status.${item.status}` ? t(`stock.status.${item.status}`) : item.status)
  }));

  const option = {
    tooltip: {
      trigger: 'item',
      formatter: '{a} <br/>{b}: {c} ({d}%)'
    },
    legend: {
      orient: 'vertical',
      left: 'left',
      top: 'center',
      textStyle: { fontSize: 10 }
    },
    series: [
      {
        name: name,
        type: 'pie',
        radius: ['50%', '80%'],
        center: ['65%', '50%'],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 0,
          borderColor: '#fff',
          borderWidth: 2
        },
        label: { show: false },
        emphasis: {
          label: {
            show: true,
            fontSize: '12',
            fontWeight: 'bold'
          }
        },
        data: chartData
      }
    ],
    color: ['#c5a880', '#222222', '#606266', '#a8824f', '#dcdfe6', '#909399', '#409eff', '#67c23a']
  };

  chartInstance.setOption(option);
};

const handleResize = () => {
  orderChart?.resize();
  stockChart?.resize();
};

watch(() => props.orderDistribution, () => updatePieChart(orderChart, props.orderDistribution, t('order.title')), { deep: true });
watch(() => props.stockDistribution, () => updatePieChart(stockChart, props.stockDistribution, t('stock.title')), { deep: true });

onMounted(() => {
  initCharts();
});

onUnmounted(() => {
  window.removeEventListener('resize', handleResize);
  orderChart?.dispose();
  stockChart?.dispose();
});
</script>

<style lang="scss" scoped>
.distribution-charts {
  margin-bottom: 1.5625rem;
}

.chart-box {
  border: 1px solid #eae6df;
  padding: 1.5625rem;
  border-radius: 2px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.02);
  height: 100%;
}

.section-header {
  margin-bottom: 1.25rem;
  .section-title {
    font-size: 0.9rem;
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
</style>

