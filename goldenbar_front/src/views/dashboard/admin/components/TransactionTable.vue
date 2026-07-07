<template>
<base-table :data="list" style="width: 100%" class="luxury-admin-table">
    <el-table-column
      :label="$t('dashboard.admin.recentActivity.orderNo')"
      min-width="200"
      :excel-formatter="(row) => row.order_no ? row.order_no.substring(0, 30) : ''"
    >
      <template #default="scope">
        <span class="order-no-text">{{ displayOrderNo(scope) }}</span>
      </template>
    </el-table-column>
    <el-table-column
      :label="$t('dashboard.admin.recentActivity.amount')"
      width="160"
      align="right"
      :excel-formatter="(row) => '₩' + (Math.floor(row.price || 0)).toLocaleString()"
    >
      <template #default="scope">
        <span class="price-text"><span class="curr">₩</span>{{ displayPrice(scope) }}</span>
      </template>
    </el-table-column>
    <el-table-column
      :label="$t('dashboard.admin.recentActivity.status')"
      width="120"
      align="center"
      :excel-formatter="(row) => row.status ? row.status.toUpperCase() : ''"
    >
      <template #default="{ row }">
        <div :class="['status-pill-luxury', row.status]">
          {{ row.status ? row.status.toUpperCase() : '' }}
        </div>
      </template>
    </el-table-column>
  </base-table>
</template>

<script>
import { defineComponent } from 'vue';
import { transactionList } from '@/api/remote-search';

export default defineComponent({
  data() {
    return {
      list: null
    };
  },
  created() {
    this.fetchData();
  },
  methods: {

    displayOrderNo(scope) {
      const s = scope.row.order_no;
      return s ? s.substring(0, 30) : '';
    },

    displayPrice(scope) {
      return (Math.floor(scope.row.price || 0)).toLocaleString();
    },

    fetchData() {
      transactionList().then(response => {
        this.list = response.data.items.slice(0, 8);
      });
    }
  }
});
</script>

<style lang="scss" scoped>
.luxury-admin-table {
  border-color: #eae6df;

  :deep(.el-table__header-wrapper) {
    th {
      background-color: #fbfaf7;

      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 1px;
      font-size: 0.8875rem;
      height: 45px;
    }
  }

  .order-no-text {
    font-family: monospace;
    font-size: 0.9rem;
    color: #666;
  }

  .price-text {
    font-weight: 700;

    .curr { font-size: 0.825rem; color: #bbbbbb; margin-right: 0.25rem; font-weight: 400; }
  }
}

.status-pill-luxury {
  display: inline-block;
  padding: 0.25rem 0.95rem;
  border-radius: 20px;
  font-size: 0.825rem;
  font-weight: 700;
  letter-spacing: 0.5px;

  &.success { background-color: #f0f9eb; color: #67c23a; border: 1px solid #e1f3d8; }
  &.pending { background-color: #fdf6ec; color: #e6a23c; border: 1px solid #faecd8; }
}
</style>

