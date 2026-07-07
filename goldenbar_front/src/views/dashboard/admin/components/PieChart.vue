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
      default: '300px'
    }
  },
  data() {
    return {
      chart: null,
      colors: ['#c5a880', '#222222', '#888888', '#a88f6c', '#444444']
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
          trigger: 'item',
          backgroundColor: 'rgba(255, 255, 255, 0.95)',
          borderColor: '#eae6df',
          borderWidth: 1,
          textStyle: { color: '#222222', fontSize: 12 },
          padding: [10, 15],
          extraCssText: 'box-shadow: 0 0.25rem 0.95rem rgba(0,0,0,0.08); border-radius: 2px;',
          formatter: '{a} <br/>{b} : {c} ({d}%)'
        },
        legend: {
          icon: 'circle',
          itemWidth: 10,
          itemGap: 20,
          left: 'center',
          bottom: '0',
          data: ['Industries', 'Technology', 'Forex', 'Gold', 'Forecasts'],
          textStyle: { color: '#888888', fontSize: 11, fontWeight: 500 }
        },
        series: [
          {
            name: 'WEEKLY WRITE ARTICLES',
            type: 'pie',
            roseType: 'radius',
            radius: [15, 95],
            center: ['50%', '42%'],
            data: [
              { value: 320, name: 'Industries' },
              { value: 240, name: 'Technology' },
              { value: 149, name: 'Forex' },
              { value: 100, name: 'Gold' },
              { value: 59, name: 'Forecasts' }
            ],
            label: {
              color: '#888888',
              fontSize: 10,
              fontWeight: 500
            },
            itemStyle: {
              borderRadius: 4,
              borderColor: '#fff',
              borderWidth: 2
            },
            animationEasing: 'cubicInOut',
            animationDuration: 2600
          }
        ]
      });
    }
  }
});
</script>

