<template>
<div class="action-buttons-container">

    <template v-if="userCategory === 'MFG'">
      <el-button
        v-if="row.status === 'FactoryRequested'"
        type="warning"
        size="small"
        @click="$emit('work-order-create', row)"
      >
        {{ $t('orderDetail.memos.workOrderMemo').split(' ')[0] }}
      </el-button>
      <el-button
        v-else-if="row.status === 'WorkOrderCreated'"
        type="primary"
        size="small"
        @click="$emit('inspection-request', row)"
      >
        {{ $t('admin.inspectionRequest.title') }}
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
        v-if="row.status === 'ORDERED'"
        type="primary"
        size="small"
        @click="$emit('approve', row)"
        :disabled="!isActionEnabled(row)"
      >
        {{ $t('order.status.LogisticsApproved') }}
      </el-button>
      <el-button
        v-else-if="row.status === 'LogisticsApproved'"
        type="warning"
        size="small"
        @click="$emit('factory-request', row)"
        :disabled="!isActionEnabled(row)"
      >
        {{ $t('order.status.FactoryRequested') }}
      </el-button>
      <el-button
        v-else-if="row.status === 'InspectedRequested'"
        type="success"
        size="small"
        @click="$emit('inspection', row)"
        :disabled="!isActionEnabled(row)"
      >
        {{ $t('order.status.Inspected') }}
      </el-button>
      <el-button
        v-else-if="row.status === 'Inspected'"
        type="primary"
        size="small"
        @click="$emit('settlement-start', row)"
        :disabled="!isActionEnabled(row)"
      >
        {{ $t('order.tabs.settlement') }}
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
        v-else-if="row.status === 'SETTLED'"
        type="primary"
        size="small"
        @click="$emit('status-update', row.id, 'DELIVERY_READY', $t('order.status.DELIVERY_READY'))"
        :disabled="!isActionEnabled(row)"
      >
        {{ $t('order.status.DELIVERY_READY') }}
      </el-button>
      <el-button
        v-else-if="row.status === 'DELIVERY_READY'"
        type="warning"
        size="small"
        @click="$emit('status-update', row.id, 'DELIVERY_IN_TRANSIT', $t('order.status.DELIVERY_IN_TRANSIT'))"
        :disabled="!isActionEnabled(row)"
      >
        {{ $t('order.status.DELIVERY_IN_TRANSIT') }}
      </el-button>
      <el-button
        v-else-if="row.status === 'DELIVERY_IN_TRANSIT'"
        type="success"
        size="small"
        @click="$emit('status-update', row.id, 'DELIVERED', $t('order.status.DELIVERED'))"
        :disabled="!isActionEnabled(row)"
      >
        {{ $t('order.status.DELIVERED') }}
      </el-button>
    </template>
  </div>
</template>

<script setup lang="ts">

import useUserStore from '@/store/modules/user';

const props = defineProps<{
  row: any;
  userCategory: string;
}>();

defineEmits([
  'approve',
  'factory-request',
  'inspection',
  'inspection-request',
  'settlement-start',
  'settlement-confirm',
  'status-update',
  'work-order-create',
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

