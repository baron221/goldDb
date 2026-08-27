<template>
<div class="order-list-compact">
    <base-table :data="orders" style="width: 100%" row-key="id">
      <el-table-column label="주문 상품" min-width="440">
        <template #default="{row}">
          <order-goods-cell :order-items="row.orderItems" :order-id="row.id" :order-status="row.status" />
        </template>
      </el-table-column>

      <el-table-column
        prop="orderNo"
        label="주문번호 / 일시"
        width="200"
        align="center"
        :excel-formatter="(row) => `${row.orderNo}\n${formatDate(row.createdAt)}`"
      >
        <template #default="{row}">
          <div class="order-no-info">
            <span class="order-no">{{ row.orderNo }}</span>
            <div class="order-time-display">{{ formatDate(row.createdAt) }}</div>
          </div>
        </template>
      </el-table-column>

      <el-table-column
        prop="logisticsCompanyName"
        label="물류업체"
        width="130"
        align="center"
        :excel-formatter="(row) => row.logisticsCompanyName || '-'"
      >
        <template #default="{row}">
          <span>{{ row.logisticsCompanyName || '-' }}</span>
        </template>
      </el-table-column>

      <el-table-column prop="deliveryDate" label="납기일" width="100" align="center">
        <template #default="{row}">
          <span v-if="row.deliveryDate" style="color: #409EFF; font-weight: bold;">{{ row.deliveryDate.substring(0, 10) }}</span>
          <span v-else>-</span>
        </template>
      </el-table-column>

      <el-table-column
        label="상태"
        width="120"
        align="center"
        :excel-formatter="(row) => getStatusLabel(row.status)"
      >
        <template #default="{row}">
          <el-tag :type="getStatusType(row.status)" size="small">{{ getStatusLabel(row.status) }}</el-tag>
        </template>
      </el-table-column>

      <el-table-column label="고객" width="150" align="center">
        <template #default="{row}">
          <div class="action-cell">
            <span class="customer-name">{{ row.customerName || '-' }}</span>
            <el-button
              v-if="row.status === 'ORDERED' || row.status === '접수대기'"
              type="danger"
              size="small"
              @click="emit('cancel', row)"
            >
              주문취소
            </el-button>
          </div>
        </template>
      </el-table-column>
    </base-table>
</div>
</template>

<script setup lang="ts">
import { parseTime } from '@/utils';
import BaseTable from '@/components/BaseTable/index.vue';
import OrderGoodsCell from '@/components/OrderTableList/components/OrderGoodsCell.vue';

const props = defineProps<{
  orders: any[];
  isMobile: boolean;
  searchTerm?: string;
}>();

const emit = defineEmits(['cancel']);

const formatDate = (dateStr: string) => {
  if (!dateStr) return '';
  return parseTime(new Date(dateStr), '{y}-{m}-{d} {h}:{i}');
};

const STATUS_LABEL: Record<string, string> = {
  'ORDERED': '접수대기',
  'Cancelled': '주문취소',
  'LogisticsApproved': '제품준비중',
  'FactoryApproved': '제품준비중',
  'FactoryRejected': '공장거절',
  'FactoryRequested': '제품준비중',
  'WorkOrderCreated': '제품준비중',
  'InspectedRequested': '제품준비중',
  'REQUEST_CLOSE_BY_AGREEMENT': '제품준비중',
  'CLOSED_BY_AGREEMENT': '제품준비중',
  'Inspected': '제품준비중',
  'PENDING': '정산대기',
  'PROCESSING': '정산중',
  'SETTLED_CANCELLED': '정산취소',
  'SETTLED': '출고준비중',
  'DELIVERY_READY': '출고대기',
  'DELIVERY_IN_TRANSIT': '이송중',
  'DELIVERED': '수령대기',
  'Completed': '수령완료'
};

const getStatusLabel = (status: string): string => STATUS_LABEL[status] ?? status;

const getStatusType = (status: string) => {
  const map: Record<string, string> = {
    'ORDERED': 'info',
    'Cancelled': 'info',
    'LogisticsApproved': 'warning',
    'FactoryApproved': 'warning',
    'FactoryRejected': 'danger',
    'FactoryRequested': 'warning',
    'WorkOrderCreated': 'warning',
    'InspectedRequested': 'warning',
    'REQUEST_CLOSE_BY_AGREEMENT': 'warning',
    'CLOSED_BY_AGREEMENT': 'warning',
    'Inspected': 'warning',
    'PENDING': 'danger',
    'PROCESSING': 'warning',
    'SETTLED_CANCELLED': 'info',
    'SETTLED': 'success',
    'DELIVERY_READY': 'primary',
    'DELIVERY_IN_TRANSIT': 'warning',
    'DELIVERED': 'success',
    'Completed': 'success'
  };
  return map[status] ?? 'info';
};
</script>

<style lang="scss" scoped>
.order-list-compact {
  width: 100%;

  .order-no-info {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.25rem;

    .order-no {
      font-weight: 700;
      font-size: 0.9rem;
      color: #2c2c2e;

      :global(html.dark) & {
        color: #eaeaea;
      }
    }

    .order-time-display {
      font-size: 0.8rem;
      color: #909399;
    }
  }

  .action-cell {
    display: flex;
    flex-direction: column;
    gap: 0.375rem;
    align-items: stretch;

    .el-button {
      width: 100%;
      margin-left: 0 !important;
    }

    .customer-name {
      font-size: 0.875rem;
      font-weight: 600;
      color: #2c2c2e;

      :global(html.dark) & {
        color: #eaeaea;
      }
    }
  }
}
</style>
