<template>
<div class="dashboard-others-container">
    <div class=" clearfix">
      <pan-thumb :image="avatar" style="float: left">
        Your roles:
        <span v-for="item in roles" :key="item" class="pan-info-roles">{{ item }}</span>
      </pan-thumb>
      <div class="info-container">
        <span class="display_name">{{ name }}</span>
        <span style="font-size: 1.25rem;padding-top: 1.25rem;display:inline-block;">{{ $t('dashboard.others.title') }}</span>
      </div>
    </div>

    <el-card style="margin-top: 1.25rem;">
      <div style="padding: 1.25rem; text-align: center;">
        {{ $t('dashboard.others.comingSoon') }}
      </div>
    </el-card>

    <el-row style="margin-top: 1.25rem;">
      <el-col :span="24">
        <gold-price-widget />
      </el-col>
    </el-row>

    <el-row style="margin-top: 1.25rem;">
      <el-col :span="24">
        <diamond-price-widget />
      </el-col>
    </el-row>

  </div>
</template>

<script>
import { mapState } from 'pinia';
import PanThumb from '@/components/PanThumb';
import GoldPriceWidget from '../components/GoldPriceWidget.vue';
import DiamondPriceWidget from '../components/DiamondPriceWidget.vue';
import { defineComponent } from 'vue';
import store from '@/store';

export default defineComponent({
  name: 'DashboardOthers',
  components: { PanThumb, GoldPriceWidget, DiamondPriceWidget },
  computed: {
    ...mapState(store.user, [
      'name',
      'avatar',
      'roles'
    ])
  }
});
</script>

<style lang="scss" scoped>
.dashboard-others-container {
  background: #f0f2f5;
  min-height: 100vh;
  padding: 3.125rem 3.75rem 0rem;

  .pan-info-roles {
    font-size: 0.95rem;
    font-weight: 700;
    color: #333;
    display: block;
  }

  .info-container {
    position: relative;
    margin-left: 11.875rem;
    height: 150px;
    line-height: 200px;

    .display_name {
      font-size: 3rem;
      line-height: 48px;
      color: #212121;
      position: absolute;
      top: 25px;
    }
  }
}
</style>

