<template>
<div :class="className" :style="{ height: height, width: width }" />
</template>

<script>
import * as echarts from 'echarts';
import { defineComponent, shallowRef } from 'vue';
import macaronsTheme from '@/styles/echarts/theme/macarons';
import resize from '../admin/components/mixins/resize';
import { getCompanyStats } from '@/api/dashboard';

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
    }
  },
  data() {
    return {
      chart: null,
      stats: {
        employeeCount: 0,
        orderCount: 0
      }
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
        const res = await getCompanyStats();
        this.stats = res.data;
        this.initChart();
      } catch (error) {
        console.error('Failed to fetch company stats:', error);
      }
    },

    initChart() {
      this.chart = shallowRef(echarts.init(this.$el, macaronsTheme));

      this.chart.setOption({
        title: {
          text: '소속 업체 현황',
          left: 'center'
        },
        tooltip: {
          trigger: 'axis',
          axisPointer: {
            type: 'shadow'
          }
        },
        grid: {
          top: 80,
          left: '3%',
          right: '4%',
          bottom: '3%',
          containLabel: true
        },
        xAxis: [{
          type: 'category',
          data: ['직원 수', '주문 건수'],
          axisTick: {
            alignWithLabel: true
          }
        }],
        yAxis: [{
          type: 'value'
        }],
        series: [{
          name: '현황',
          type: 'bar',
          barWidth: '40%',
          data: [
            {
              value: this.stats.employeeCount,
              itemStyle: { color: '#409EFF' }
            },
            {
              value: this.stats.orderCount,
              itemStyle: { color: '#67C23A' }
            }
          ],
          label: {
            show: true,
            position: 'top'
          }
        }]
      });
    }
  }
});
</script>

