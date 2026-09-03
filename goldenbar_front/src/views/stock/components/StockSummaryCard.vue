<template>
<el-card shadow="never" class="summary-card">
    <el-row :gutter="20">
      <el-col :xs="24" :sm="8" :md="2" class="mb-4 md:mb-0">
        <div class="summary-item">
          <div class="label">14K 총 실중량</div>
          <div class="value">{{ summary.total14KWeight.toFixed(2) }}g</div>
        </div>
      </el-col>
      <el-col :xs="24" :sm="8" :md="2" class="mb-4 md:mb-0">
        <div class="summary-item">
          <div class="label">18K 총 실중량</div>
          <div class="value">{{ summary.total18KWeight.toFixed(2) }}g</div>
        </div>
      </el-col>
      <el-col :xs="24" :sm="8" :md="2" class="mb-4 md:mb-0">
        <div class="summary-item">
          <div class="label">순금 총 실중량</div>
          <div class="value">{{ summary.totalPureGoldWeight.toFixed(2) }}g</div>
        </div>
      </el-col>
      <el-col :xs="24" :sm="8" :md="2" class="mb-4 md:mb-0">
        <div class="summary-item">
          <div class="label">PT 총 실중량</div>
          <div class="value">{{ summary.totalPtWeight.toFixed(2) }}g</div>
        </div>
      </el-col>
      <el-col :xs="24" :sm="8" :md="2" class="mb-4 md:mb-0">
        <div class="summary-item">
          <div class="label">순은 총 실중량</div>
          <div class="value">{{ (summary.totalSilverWeight || 0).toFixed(2) }}g</div>
        </div>
      </el-col>
      <el-col :xs="24" :sm="24" :md="14">
        <div class="summary-item highlight">
          <div class="label">총 순금 보유량</div>
          <div class="value formula">
            ({{ summary.total14KWeight.toFixed(2) }}g × 0.6435) + ({{ summary.total18KWeight.toFixed(2) }}g × 0.825) + {{ summary.totalPureGoldWeight.toFixed(2) }}g + ({{ summary.totalPtWeight.toFixed(2) }}g × 0.95) = {{ summary.totalCalculatedPureGoldWeight.toFixed(2) }}g
          </div>
        </div>
      </el-col>
    </el-row>
  </el-card>
</template>

<script setup lang="ts">

import type { PropType } from 'vue';

defineProps({

  summary: {
    type: Object as PropType<{
      total14KWeight: number;
      total18KWeight: number;
      totalPureGoldWeight: number;
      totalPtWeight: number;
      totalCalculatedPureGoldWeight: number;
      totalSilverWeight?: number;
    }>,
    required: true
  }
});
</script>

<style lang="scss" scoped>
.summary-card {
  border-radius: 2px;
  border: 1px solid #eae6df;
  background-color: #ffffff;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.02);

  .summary-item {
    text-align: center;
    padding: 0.625rem 0;

    .label {
      font-size: 0.8875rem;
      font-weight: 700;
      color: #888888;
      text-transform: uppercase;
      letter-spacing: 1px;
      margin-bottom: 0.5rem;
    }

    .value {
      font-size: 1.25rem;
      font-weight: 700;

      font-family: 'S-CoreDream', 'Jost', sans-serif;
    }

    &.highlight {
      border-left: 3px solid #c5a880;
      .value { color: #c5a880; }
      .formula { font-size: 0.9rem; font-weight: 500; }
    }
  }
}
</style>

