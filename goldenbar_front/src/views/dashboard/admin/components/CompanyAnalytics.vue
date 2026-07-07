<template>
<el-row :gutter="20" class="company-analytics">
    <el-col :xs="24" :lg="12" style="">
      <div class="chart-box">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.admin.analytics.topRetailers') }}</h2>
        </div>
        <div ref="retailerChartRef" class="chart-canvas" style="height: 350px;"></div>
      </div>
    </el-col>
    <el-col :xs="24" :lg="12">
      <div class="chart-box">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.admin.analytics.topManufacturers') }}</h2>
        </div>
        <div ref="manufacturerChartRef" class="chart-canvas" style="height: 350px;"></div>
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
  topRetailers: {
    type: Array,
    required: true
  },
  topManufacturers: {
    type: Array,
    required: true
  }
});

const retailerChartRef = ref<HTMLElement | null>(null);
const manufacturerChartRef = ref<HTMLElement | null>(null);
let retailerChart: echarts.ECharts | null = null;
let manufacturerChart: echarts.ECharts | null = null;

const initCharts = () => {
  if (retailerChartRef.value) {
    retailerChart = echarts.init(retailerChartRef.value);
    updateBarChart(retailerChart, props.topRetailers, t('dashboard.admin.analytics.revenue'), '#c5a880');
  }
  if (manufacturerChartRef.value) {
    manufacturerChart = echarts.init(manufacturerChartRef.value);
    updateBarChart(manufacturerChart, props.topManufacturers, t('dashboard.admin.analytics.revenue'), '#222');
  }
  window.addEventListener('resize', handleResize);
};

const updateBarChart = (chartInstance: echarts.ECharts | null, data: any[], seriesName: string, color: string) => {
  if (!chartInstance) return;

  const labels = data.map((item: any) => item.companyName).reverse();
  const values = data.map((item: any) => item.totalAmount).reverse();

  const option = {
    tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' }},
    grid: { left: '3%', right: '8%', bottom: '3%', containLabel: true },
    xAxis: {
      type: 'value',
      axisLabel: {
        formatter: (value: number) => {
          if (value >= 1000000) return (value / 1000000).toFixed(0) + 'M';
          return value;
        }
      }
    },
    yAxis: { type: 'category', data: labels, axisLabel: { fontSize: 10 }},
    series: [
      {
        name: seriesName,
        type: 'bar',
        data: values,
        itemStyle: { color: color },
        label: { show: true, position: 'right', formatter: (params: any) => '₩' + (params.value / 1000000).toFixed(1) + 'M', fontSize: 10 }
      }
    ]
  };

  chartInstance.setOption(option);
};

const handleResize = () => {
  retailerChart?.resize();
  manufacturerChart?.resize();
};

watch(() => props.topRetailers, () => updateBarChart(retailerChart, props.topRetailers, t('dashboard.admin.analytics.revenue'), '#c5a880'), { deep: true });
watch(() => props.topManufacturers, () => updateBarChart(manufacturerChart, props.topManufacturers, t('dashboard.admin.analytics.revenue'), '#222'), { deep: true });

onMounted(() => {
  initCharts();
});

onUnmounted(() => {
  window.removeEventListener('resize', handleResize);
  retailerChart?.dispose();
  manufacturerChart?.dispose();
});
</script>

<style lang="scss" scoped>
.company-analytics {
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

