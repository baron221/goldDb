<template>
<div class="action-buttons-container">

    <template v-if="userCategory === 'MFG'">
      <el-button
        v-if="row.status === 'LogisticsApproved' || row.status === 'FactoryRequested'"
        type="success"
        size="small"
        @click="$emit('factory-approve', row)"
      >
        발주 확인
      </el-button>
      <el-button
        v-else-if="row.status === 'FactoryApproved' || row.status === 'WorkOrderCreated'"
        type="primary"
        size="small"
        @click="$emit('inspection-request', row)"
      >
        {{ $t('admin.inspectionRequest.productDispatch') }}
      </el-button>
      <el-button
        v-else-if="row.status === 'REQUEST_CLOSE_BY_AGREEMENT'"
        type="danger"
        size="small"
        @click="$emit('close-by-agreement', row)"
      >
        {{ $t('order.list.cancelOrder') }}
      </el-button>
    </template>

    <template v-else>
      <el-button
        v-if="row.status === 'ORDERED' || row.status === 'LogisticsApproved' || row.status === 'FactoryRejected'"
        type="primary"
        size="small"
        @click="$emit('factory-request', row)"
        :disabled="!isActionEnabled(row)"
      >
        {{ row.status === 'FactoryRejected' ? '재의뢰' : $t('order.status.FactoryRequested') }}
      </el-button>
      <el-tooltip
        v-else-if="row.status === 'InspectedRequested' && row.isSettlementProcessed === false"
        content="제조사 정산처리가 완료되면 물류도착 처리를 할 수 있습니다"
        placement="top"
      >
        <el-tag type="warning" size="small" effect="plain">정산처리 대기중</el-tag>
      </el-tooltip>
      <el-button
        v-else-if="row.status === 'InspectedRequested' || row.status === 'Inspected'"
        type="success"
        size="small"
        @click="$emit('inspection', row)"
        :disabled="!isActionEnabled(row)"
      >
        {{ getStatusLabel('Inspected', userCategory) }}
      </el-button>
      <el-button
        v-else-if="row.status === 'PROCESSING'"
        type="success"
        size="small"
        @click="$emit('settlement-confirm', row)"
        :disabled="!isActionEnabled(row)"
      >
        {{ $t('admin.settlement.messages.settlementBtn') }}
      </el-button>
      <el-button
        v-else-if="['SETTLED', 'DELIVERY_READY', 'DELIVERY_IN_TRANSIT', 'DELIVERED'].includes(row.status)"
        type="success"
        size="small"
        @click="$emit('status-update', row.id, 'Completed', $t('order.status.Completed'))"
        :disabled="!isActionEnabled(row)"
      >
        {{ getStatusLabel('Completed', userCategory) }}
      </el-button>
      <el-tag
        v-else-if="row.status === 'Completed'"
        type="success"
        size="small"
        effect="plain"
      >
        수령완료
      </el-tag>
    </template>
  </div>
</template>

<script setup lang="ts">

import useUserStore from '@/store/modules/user';
import { getStatusLabel } from '@/utils/order';

const props = defineProps<{
  row: any;
  userCategory: string;
}>();

defineEmits([
  'factory-request',
  'factory-approve',
  'inspection',
  'inspection-request',
  'settlement-confirm',
  'status-update',
  'close-by-agreement'
]);

const userStore = useUserStore();

const isActionEnabled = (row: any) => {
  if (props.userCategory === 'ADMIN') {
    return true;
  }
  if (props.userCategory === 'DCC') {
    return row.logisticsCompanyId === userStore.companyId;
  }
  if (props.userCategory === 'MFG') {
    return true;
  }
  return false;
};
</script>

<style scoped>
.action-buttons-container {
  display: flex;
  flex-direction: column;
  align-items: stretch;
  gap: 0.375rem;
}
.action-buttons-container .el-button {
  width: 100%;
  margin-left: 0 !important;
}
</style>

