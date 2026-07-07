<template>
<el-row :gutter="20" class="product-performance">
    <el-col :xs="24" :lg="14" style="">
      <div class="table-box">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.admin.performance.topSellingProducts') }}</h2>
        </div>
        <base-table :data="topProducts" stripe style="width: 100%" size="small">
          <el-table-column
            :label="$t('dashboard.admin.performance.product')"
            min-width="200"
            :excel-formatter="(row) => row.productName + ' (' + row.productNo + ')'"
          >
            <template #default="{row}">
              <div class="prod-info">
                <div class="prod-name">{{ row.productName }}</div>
                <div class="prod-no">{{ row.productNo }}</div>
              </div>
            </template>
          </el-table-column>
          <el-table-column prop="quantity" :label="$t('dashboard.admin.performance.qty')" width="80" align="center" />
          <el-table-column
            :label="$t('dashboard.admin.performance.revenue')"
            width="150"
            align="right"
            :excel-formatter="(row) => '₩' + formatPrice(row.totalAmount)"
          >
            <template #default="{row}">
              <span class="revenue">₩{{ formatPrice(row.totalAmount) }}</span>
            </template>
          </el-table-column>
        </base-table>
      </div>
    </el-col>
    <el-col :xs="24" :lg="10">
      <div class="table-box">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.admin.performance.categoryPerformance') }}</h2>
        </div>
        <base-table :data="categoryPerformance" stripe style="width: 100%" size="small">
          <el-table-column prop="categoryName" :label="$t('dashboard.admin.performance.category')" />
          <el-table-column prop="orderCount" :label="$t('dashboard.admin.performance.orders')" width="80" align="center" />
          <el-table-column
            :label="$t('dashboard.admin.performance.revenue')"
            width="130"
            align="right"
            :excel-formatter="(row) => '₩' + formatPrice(row.totalAmount)"
          >
            <template #default="{row}">
              <span class="revenue-mini">₩{{ formatPrice(row.totalAmount) }}</span>
            </template>
          </el-table-column>
        </base-table>
      </div>
    </el-col>
  </el-row>
</template>

<script setup lang="ts">
import { formatPrice } from '@/utils/format';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

defineProps({
  topProducts: { type: Array, required: true },
  categoryPerformance: { type: Array, required: true }
});
</script>

<style lang="scss" scoped>
.product-performance {
  margin-bottom: 1.5625rem;
}

.table-box {
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

.prod-info {
  .prod-name { font-weight: 600; color: #222; }
  .prod-no { font-size: 0.825rem; color: #bbbbbb; font-family: monospace; }
}

.revenue { font-weight: 700; color: #c5a880; }
.revenue-mini { font-weight: 600; color: #666; font-size: 0.8875rem; }

:deep(.el-table) {
  th.el-table__cell {
    background-color: #fbfaf7 !important;
    color: #888 !important;
    text-transform: uppercase;
    letter-spacing: 1px;
  }
}
</style>

