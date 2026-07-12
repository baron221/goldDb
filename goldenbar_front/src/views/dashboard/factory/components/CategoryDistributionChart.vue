<template>
<div :class="className" :style="{ height: height, width: width }" />
</template>

<script>
import * as echarts from 'echarts';
import { defineComponent, shallowRef, watch } from 'vue';
import macaronsTheme from '@/styles/echarts/theme/macarons';
import resize from '../../admin/components/mixins/resize';

export default defineComponent({
  name: 'CategoryDistributionChart',
  mixins: [resize],
  props: {
    className: {
      type: String,
      default: 'chart'
    },
    width: {
      type: String,
      default: '100%'
    },
    height: {
      type: String,
      default: '350px'
    },
    title: {
      type: String,
      default: '등록 현황'
    },
    chartData: {
      type: Array,
      required: true,
      default: () => []
    },
    dates: {
      type: Array,
      required: true,
      default: () => []
    }
  },
  setup() {
    return {
      chart: shallowRef(null)
    };
  },
  watch: {
    chartData: {
      deep: true,
      handler() {
        this.setOptions();
      }
    },
    dates: {
      deep: true,
      handler() {
        this.setOptions();
      }
    }
  },
  mounted() {
    this.$nextTick(() => {
      this.initChart();
    });
  },
  beforeUnmount() {
    if (!this.chart) {
      return;
    }
    this.chart.dispose();
    this.chart = null;
  },
  methods: {
    initChart() {
      this.chart = echarts.init(this.$el, macaronsTheme);
      this.setOptions();
    },
    setOptions() {
      if (!this.chart || !this.chartData || !this.dates) return;

      const categorySeries = this.chartData.map(item => ({
        name: item.categoryName,
        type: 'line',
        smooth: true,
        symbol: 'circle',
        symbolSize: 6,
        lineStyle: {
          width: 2
        },
        data: item.dailyCounts,
        animationDuration: 2800,
        animationEasing: 'cubicInOut'
      }));

      const totalDailyCounts = this.dates.map((_, index) => {
        return this.chartData.reduce((sum, item) => sum + (item.dailyCounts[index] || 0), 0);
      });

      const totalSeries = {
        name: this.$t('dashboard.factory.totalSum'),
        type: 'line',
        smooth: true,
        symbol: 'diamond',
        symbolSize: 8,
        itemStyle: {
          color: '#222222'
        },
        lineStyle: {
          width: 3,
          type: 'dashed'
        },
        data: totalDailyCounts,
        animationDuration: 2800,
        animationEasing: 'cubicInOut'
      };

      const allSeries = [totalSeries, ...categorySeries];

      this.chart.setOption({
        tooltip: {
          trigger: 'axis',
          axisPointer: {
            type: 'cross',
            label: {
              backgroundColor: '#6a7985'
            }
          },
          formatter: (params) => {
            let res = `<div style="font-weight: bold; margin-bottom: 5px;">${params[0].name} ${this.title}</div>`;
            params.forEach(item => {
              res += `<div style="display: flex; justify-content: space-between; gap: 20px;">
                        <span>${item.marker} ${item.seriesName}</span>
                        <span style="font-weight: bold;">${item.value} ${this.$t('dashboard.common.units.qty')}</span>
                      </div>`;
            });
            return res;
          }
        },
        legend: {
          data: allSeries.map(item => item.name),
          top: '0',
          itemGap: 15
        },
        grid: {
          left: '3%',
          right: '5%',
          bottom: '5%',
          top: '15%',
          containLabel: true
        },
        xAxis: {
          type: 'category',
          name: this.$t('dashboard.factory.dateAxis'),
          boundaryGap: false,
          data: this.dates,
          axisTick: {
            show: false
          },
          axisLabel: {
            color: '#888888'
          }
        },
        yAxis: {
          type: 'value',
          name: this.$t('dashboard.factory.quantityAxis'),
          minInterval: 1, 
          axisTick: {
            show: false
          },
          axisLabel: {
            color: '#888888',
            formatter: '{value}' 
          },
          splitLine: {
            lineStyle: {
              type: 'dashed',
              color: '#f0f0f0'
            }
          }
        },
        series: allSeries
      });
    }
  }
});
</script>

