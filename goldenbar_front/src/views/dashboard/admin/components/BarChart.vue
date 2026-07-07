<template>
<div :class="className" :style="{ height: height, width: width }" />
</template>

<script>
import * as echarts from 'echarts';
import { defineComponent, shallowRef } from 'vue';
import resize from './mixins/resize';

const animationDuration = 3000;

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
      default: '300px'
    }
  },
  data() {
    return {
      chart: null,
      colors: ['#c5a880', '#222222', '#888888']
    };
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

      this.chart.setOption({
        color: this.colors,
        textStyle: {
          fontFamily: 'S-CoreDream, Jost, sans-serif'
        },
        tooltip: {
          trigger: 'axis',
          backgroundColor: 'rgba(255, 255, 255, 0.95)',
          borderColor: '#eae6df',
          borderWidth: 1,
          textStyle: { color: '#222222', fontSize: 12 },
          axisPointer: {
            type: 'shadow'
          },
          padding: [10, 15],
          extraCssText: 'box-shadow: 0 0.25rem 0.95rem rgba(0,0,0,0.08); border-radius: 2px;'
        },
        grid: {
          top: '10%',
          left: '3%',
          right: '3%',
          bottom: '5%',
          containLabel: true
        },
        xAxis: [{
          type: 'category',
          data: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
          axisLine: {
            lineStyle: { color: '#eae6df' }
          },
          axisLabel: { color: '#888888', fontSize: 11 },
          axisTick: {
            show: false
          }
        }],
        yAxis: [{
          type: 'value',
          splitLine: {
            lineStyle: { color: '#f4f2ee' }
          },
          axisLine: { show: false },
          axisLabel: { color: '#888888', fontSize: 11 },
          axisTick: {
            show: false
          }
        }],
        series: [{
          name: 'pageA',
          type: 'bar',
          stack: 'vistors',
          barWidth: '40%',
          data: [79, 52, 200, 334, 390, 330, 220],
          animationDuration
        }, {
          name: 'pageB',
          type: 'bar',
          stack: 'vistors',
          barWidth: '40%',
          data: [80, 52, 200, 334, 390, 330, 220],
          animationDuration
        }, {
          name: 'pageC',
          type: 'bar',
          stack: 'vistors',
          barWidth: '40%',
          data: [30, 52, 200, 334, 390, 330, 220],
          animationDuration
        }]
      });
    }
  }
});
</script>

