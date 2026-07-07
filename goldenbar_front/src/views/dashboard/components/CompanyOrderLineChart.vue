<template>
<div :class="className" :style="{ height: height, width: width }" />
</template>

<script>
import * as echarts from 'echarts';
import { defineComponent, shallowRef } from 'vue';
import resize from '../admin/components/mixins/resize';
import { getDailyCompanyStats } from '@/api/dashboard';

export default defineComponent({
  name: 'CompanyOrderLineChart',
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
      default: '400px'
    }
  },
  data() {
    return {
      chart: null,
      chartData: {
        dates: [],
        series: []
      },

      colors: ['#c5a880', '#222222', '#888888', '#a88f6c', '#444444', '#e0d8cc']
    };
  },
  mounted() {
    this.fetchData();
  },
  beforeUnmount() {
    if (!this.chart) {
      return;
    }
    this.chart.dispose();
    this.chart = null;
  },
  methods: {

    async fetchData() {
      try {
        const res = await getDailyCompanyStats();
        this.chartData = res.data;
        this.initChart();
      } catch (error) {
        console.error('Failed to fetch daily company stats:', error);
      }
    },

    initChart() {

      this.chart = shallowRef(echarts.init(this.$el));

      const series = this.chartData.series.map((item, index) => ({
        name: item.employeeName,
        type: 'line',
        smooth: true,
        symbol: 'circle',
        symbolSize: 8,
        showSymbol: false,
        lineStyle: {
          width: 3,
          color: this.colors[index % this.colors.length]
        },
        itemStyle: {
          color: this.colors[index % this.colors.length],
          borderColor: '#fff',
          borderWidth: 2
        },
        areaStyle: {
          color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
            {
              offset: 0,
              color: this.colors[index % this.colors.length] + '44' 
            },
            {
              offset: 1,
              color: this.colors[index % this.colors.length] + '00' 
            }
          ])
        },
        data: item.orderCounts,
        animationDuration: 2000,
        animationEasing: 'quadraticOut'
      }));

      this.chart.setOption({
        color: this.colors,
        backgroundColor: 'transparent',
        textStyle: {
          fontFamily: 'S-CoreDream, Jost, sans-serif'
        },
        tooltip: {
          trigger: 'axis',
          backgroundColor: 'rgba(255, 255, 255, 0.95)',
          borderColor: '#eae6df',
          borderWidth: 1,
          textStyle: {
            color: '#222222',
            fontSize: 12
          },
          axisPointer: {
            lineStyle: {
              color: '#c5a880',
              width: 1,
              type: 'dashed'
            }
          },
          padding: [12, 16],
          extraCssText: 'box-shadow: 0 0.25rem 0.95rem rgba(0,0,0,0.08); border-radius: 2px;'
        },
        grid: {
          top: 60,
          left: '3%',
          right: '4%',
          bottom: '5%',
          containLabel: true
        },
        legend: {
          icon: 'rect',
          itemWidth: 12,
          itemHeight: 4,
          itemGap: 24,
          data: this.chartData.series.map(s => s.employeeName),
          right: 'center',
          top: 0,
          textStyle: {
            color: '#888888',
            fontSize: 12,
            fontWeight: 500,
            textTransform: 'uppercase',
            letterSpacing: '1px'
          }
        },
        xAxis: {
          type: 'category',
          boundaryGap: false,
          data: this.chartData.dates,
          axisLine: {
            lineStyle: {
              color: '#eae6df'
            }
          },
          axisLabel: {
            color: '#888888',
            fontSize: 11,
            margin: 15
          },
          axisTick: {
            show: false
          }
        },
        yAxis: {
          type: 'value',
          minInterval: 1,
          splitLine: {
            lineStyle: {
              color: '#f4f2ee',
              type: 'solid'
            }
          },
          axisLine: {
            show: false
          },
          axisLabel: {
            color: '#888888',
            fontSize: 11
          },
          axisTick: {
            show: false
          }
        },
        series: series
      });
    }
  }
});
</script>

