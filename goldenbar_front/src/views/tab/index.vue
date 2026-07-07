<template>
<div class="tab-container">
    <el-tag>mounted times ：{{ createdTimes }}</el-tag>
    <el-alert :closable="false" class="tab-alert" title="Tab with keep-alive" type="success" />
    <el-tabs v-model="activeName" style="margin-top: 0.9375rem;" type="border-card">
      <el-tab-pane v-for="item in tabMapOptions" :key="item.key" :label="item.label" :name="item.key">
        <keep-alive>
          <tab-pane v-if="activeName==item.key" :type="item.key" @create="showCreatedTimes" />
        </keep-alive>
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script>
import { defineComponent } from 'vue';
import TabPane from './components/TabPane';

export default defineComponent({
  name: 'Tab',
  components: { TabPane },
  data() {
    return {
      tabMapOptions: [
        { label: 'China', key: 'CN' },
        { label: 'USA', key: 'US' },
        { label: 'Japan', key: 'JP' },
        { label: 'Eurozone', key: 'EU' }
      ],
      activeName: 'CN',
      createdTimes: 0
    };
  },

  created() {

    const tab = this.$route.query.tab;
    if (tab) {
      this.activeName = tab;
    }
  },
  methods: {
    showCreatedTimes() {
      this.createdTimes = this.createdTimes + 1;
    }
  }
});
</script>

<style scoped>
  .tab-container {
    margin: 1.875rem;
  }
  .tab-alert {
    width: 200px;
    display: inline-block;
    vertical-align: middle;
    margin-left: 1.875rem;
  }
  @media (max-width: 768px) {
    .tab-container {
      margin: 1rem;
    }
    .tab-alert {
      width: 100%;
      margin-left: 0;
      margin-top: 0.625rem;
      display: block;
    }
  }
</style>

