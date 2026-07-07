<template>
<div :class="className" :style="{ height: height, width: width }" />
</template>

<script>
import * as echarts from 'echarts';
import { defineComponent, shallowRef } from 'vue';
import resize from './mixins/resize';

export default defineComponent({
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
    autoResize: {
      type: Boolean,
      default: true
    },
    chartData: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      chart: null,
      colors: ['#c5a880', '#222222']
    };
  },
  watch: {
    chartData: {
      deep: true,
      handler(val) {
        this.setOptions(val);
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
      this.chart = shallowRef(echarts.init(this.$el));
      this.setOptions(this.chartData);
    },

    setOptions({ expectedData, actualData } = {}) {
      this.chart.setOption({
        color: this.colors,
        textStyle: {
          fontFamily: 'S-CoreDream, Jost, sans-serif'
        },
        xAxis: {
          data: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
          boundaryGap: false,
          axisLine: {
            lineStyle: { color: '#eae6df' }
          },
          axisLabel: { color: '#888888', fontSize: 11 },
          axisTick: {
            show: false
          }
        },
        grid: {
          left: '3%',
          right: '4%',
          bottom: '5%',
          top: '15%',
          containLabel: true
        },
        tooltip: {
          trigger: 'axis',
          backgroundColor: 'rgba(255, 255, 255, 0.95)',
          borderColor: '#eae6df',
          borderWidth: 1,
          textStyle: { color: '#222222', fontSize: 12 },
          axisPointer: {
            lineStyle: { color: '#c5a880', width: 1, type: 'dashed' }
          },
          extraCssText: 'box-shadow: 0 4px 12px rgba(0,0,0,0.08); border-radius: 2px;'
        },
        yAxis: {
          splitLine: {
            lineStyle: { color: '#f4f2ee' }
          },
          axisLine: { show: false },
          axisLabel: { color: '#888888', fontSize: 11 },
          axisTick: {
            show: false
          }
        },
        legend: {
          icon: 'rect',
          itemWidth: 12,
          itemHeight: 4,
          itemGap: 24,
          data: ['expected', 'actual'],
          right: 'center',
          top: 0,
          textStyle: { color: '#888888', fontSize: 11, fontWeight: 500, textTransform: 'uppercase' }
        },
        series: [{
          name: 'expected',
          itemStyle: { color: this.colors[0] },
          lineStyle: { color: this.colors[0], width: 3 },
          smooth: true,
          type: 'line',
          data: expectedData,
          animationDuration: 2800,
          animationEasing: 'cubicInOut'
        },
        {
          name: 'actual',
          smooth: true,
          type: 'line',
          itemStyle: { color: this.colors[1], borderColor: '#fff', borderWidth: 2 },
          lineStyle: { color: this.colors[1], width: 3 },
          areaStyle: {
            color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
              { offset: 0, color: this.colors[1] + '33' },
              { offset: 1, color: this.colors[1] + '00' }
            ])
          },
          data: actualData,
          animationDuration: 2800,
          animationEasing: 'quadraticOut'
        }]
      });
    }
  }
});
</script>

