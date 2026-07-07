<template>
<el-dropdown trigger="click" class="more-actions-dropdown" popper-class="more-actions-popper">
    <el-button size="small" :icon="MoreFilled" circle />
    <template #dropdown>
      <el-dropdown-menu>

        <el-dropdown-item
          v-if="props.userCategory === 'MFG' && (props.order.status === 'FactoryRequested' || props.order.status === 'REQUEST_CLOSE_BY_AGREEMENT')"
          @click="$emit('inspection-request', props.order)"
        >
          {{ $t('admin.inspectionRequest.title') }}
        </el-dropdown-item>
        <el-dropdown-item
          v-if="props.userCategory === 'MFG' && props.order.status === 'REQUEST_CLOSE_BY_AGREEMENT'"
          style="color: #f56c6c;"
          @click="$emit('close-by-agreement', props.order)"
        >
          {{ $t('order.list.cancelOrder') }}
        </el-dropdown-item>

        <el-dropdown-item
          v-if="props.userCategory !== 'MFG' && props.order.status === 'ORDERED'"
          @click="$emit('approve', props.order)"
          :disabled="!isActionEnabled(props.order)"
        >
          {{ $t('order.status.LogisticsApproved') }}
        </el-dropdown-item>
        <el-dropdown-item
          v-if="props.userCategory !== 'MFG' && props.order.status === 'LogisticsApproved'"
          @click="$emit('factory-request', props.order)"
          :disabled="!isActionEnabled(props.order)"
        >
          {{ $t('order.status.FactoryRequested') }}
        </el-dropdown-item>
        <el-dropdown-item
          v-if="props.userCategory !== 'MFG' && props.order.status === 'InspectedRequested'"
          @click="$emit('inspection', props.order)"
          :disabled="!isActionEnabled(props.order)"
        >
          {{ $t('order.status.Inspected') }}
        </el-dropdown-item>
        <el-dropdown-item
          v-if="props.userCategory !== 'MFG' && props.order.status === 'Inspected'"
          @click="$emit('settlement-start', props.order)"
          :disabled="!isActionEnabled(props.order)"
        >
          {{ $t('order.tabs.settlement') }}
        </el-dropdown-item>
        <el-dropdown-item
          v-if="props.userCategory !== 'MFG' && props.order.status === 'PROCESSING'"
          @click="$emit('settlement-confirm', props.order)"
          :disabled="!isActionEnabled(props.order)"
        >
          {{ $t('admin.settlement.messages.confirmSettlementTitle') }}
        </el-dropdown-item>

        <el-dropdown-item
          v-if="props.userCategory !== 'MFG' && props.order.status === 'SETTLED'"
          @click="$emit('status-update', props.order.id, 'DELIVERY_READY', $t('order.status.DELIVERY_READY'))"
          :disabled="!isActionEnabled(props.order)"
        >
          {{ $t('order.status.DELIVERY_READY') }}
        </el-dropdown-item>
        <el-dropdown-item
          v-if="props.userCategory !== 'MFG' && (props.order.status === 'SETTLED' || props.order.status === 'DELIVERY_READY')"
          @click="$emit('status-update', props.order.id, 'DELIVERY_IN_TRANSIT', $t('order.status.DELIVERY_IN_TRANSIT'))"
          :disabled="!isActionEnabled(props.order)"
        >
          {{ $t('order.status.DELIVERY_IN_TRANSIT') }}
        </el-dropdown-item>
        <el-dropdown-item
          v-if="props.userCategory !== 'MFG' && (props.order.status === 'SETTLED' || props.order.status === 'DELIVERY_READY' || props.order.status === 'DELIVERY_IN_TRANSIT')"
          @click="$emit('status-update', props.order.id, 'DELIVERED', $t('order.status.DELIVERED'))"
          :disabled="!isActionEnabled(props.order)"
        >
          {{ $t('order.status.DELIVERED') }}
        </el-dropdown-item>

        <el-divider v-if="hasMainAction(props.order.status)" style="margin: 0.25rem 0;" />

        <el-dropdown-item
          v-if="props.order.status === 'ORDERED'"
          :icon="Delete"
          style="color: #f56c6c;"
          @click="$emit('cancel', props.order)"
          :disabled="!isActionEnabled(props.order)"
        >
          {{ $t('order.list.cancelOrder') }}
        </el-dropdown-item>

        <el-dropdown-item
          v-if="props.order.status === 'LogisticsApproved'"
          :icon="Box"
          @click="$emit('stock-exhaustion', props.order)"
          :disabled="!isActionEnabled(props.order)"
        >
          {{ $t('admin.stockExhaustion.title').split(' ')[1] }}
        </el-dropdown-item>

        <el-dropdown-item
          v-if="props.userCategory === 'DCC' && props.order.status === 'InspectedRequested'"
          :icon="Refresh"
          @click="$emit('request-modification', props.order)"
          :disabled="!isActionEnabled(props.order)"
        >
          {{ $t('orderDetail.memos.modificationHistory') }}
        </el-dropdown-item>
        <el-dropdown-item
          v-if="props.userCategory === 'DCC' && props.order.status === 'InspectedRequested'"
          :icon="Delete"
          style="color: #f56c6c;"
          @click="$emit('close-by-agreement', props.order)"
          :disabled="!isActionEnabled(props.order)"
        >
          {{ $t('orderDetail.memos.closeMemo') }}
        </el-dropdown-item>

        <el-dropdown-item
          v-if="isStatementVisibleStatus(props.order.status)"
          @click="$emit('show-statement', props.order)"
        >
          {{ $t('admin.settlement.statement.title') }}
        </el-dropdown-item>

        <el-dropdown-item
          v-if="isStatementVisibleStatus(props.order.status)"
          divided
          style="color: #409EFF;"
          @click="$emit('show-live-statement', props.order)"
        >
          {{ $t('admin.settlement.statement.title') }} (LIVE)
        </el-dropdown-item>
        <el-dropdown-item
          v-if="props.order.hasStatement"
          style="color: #67C23A;"
          @click="$emit('show-statement', props.order)"
        >
          {{ $t('admin.settlement.statement.title') }} (JSON)
        </el-dropdown-item>
        <el-dropdown-item
          v-if="props.order.hasStatement"
          style="color: #E6A23C;"
          @click="handleDownloadStoredPdf(props.order)"
        >
          {{ $t('admin.settlement.statement.title') }} (PDF)
        </el-dropdown-item>

        <el-divider v-if="hasMainAction(props.order.status)" style="margin: 0.25rem 0;" />

        <el-dropdown-item :icon="Clock" @click="$emit('show-history', props.order)">
          {{ $t('orderDetail.memos.showHistory') }}
        </el-dropdown-item>
      </el-dropdown-menu>
    </template>
  </el-dropdown>
</template>

<script setup lang="ts">

import { MoreFilled, Delete, Box, Refresh, Clock } from '@element-plus/icons-vue';
import { isStatementVisibleStatus } from '@/utils/order';
import { getOrderStatementPdf } from '@/api/order';
import { ElMessage } from 'element-plus';
import useUserStore from '@/store/modules/user';
const userStore = useUserStore();

const props = defineProps({
  order: {
    type: Object,
    required: true
  },
  userCategory: {
    type: String,
    default: ''
  }
});

defineEmits([
  'approve',
  'factory-request',
  'inspection',
  'inspection-request',
  'settlement-start',
  'settlement-confirm',
  'cancel',
  'stock-exhaustion',
  'status-update',
  'request-modification',
  'close-by-agreement',
  'show-statement',
  'show-live-statement',
  'show-history'
]);

const isActionEnabled = (order: any) => {
  if (props.userCategory === 'ADMIN') {
    return true;
  }
  if (props.userCategory === 'DCC') {
    return order.logisticsCompanyId === userStore.companyId;
  }
  if (props.userCategory === 'MFG') {
    return true;
  }
  return false;
};

const hasMainAction = (status: string) => {

  if (props.userCategory === 'MFG') {
    return status === 'FactoryRequested' || status === 'REQUEST_CLOSE_BY_AGREEMENT';
  }

  const mainStatuses = [
    'ORDERED',
    'LogisticsApproved',
    'InspectedRequested',
    'Inspected',
    'PROCESSING',
    'SETTLED',
    'DELIVERY_READY',
    'DELIVERY_IN_TRANSIT'
  ];
  return mainStatuses.includes(status);
};

const handleDownloadStoredPdf = async (order: any) => {

  if (!order.hasStatement) {
    ElMessage.error('보관된 명세서가 없습니다.');
    return;
  }

  try {

    const response: any = await getOrderStatementPdf(order.id);
    if (response.data && response.data.pdfBinary) {

      const dataUrl = `data:application/pdf;base64,${response.data.pdfBinary}`;
      const fetchResponse = await fetch(dataUrl);
      const blob = await fetchResponse.blob();

      const link = document.createElement('a');
      link.href = window.URL.createObjectURL(blob);
      link.download = `[저장본]_거래명세서_${order.orderNo}.pdf`;
      link.click();
      ElMessage.success('서버에 저장된 PDF 파일을 다운로드합니다.');
    } else {
      ElMessage.warning('보관된 PDF 데이터가 없습니다.');
    }
  } catch (error) {
    console.error('Failed to download stored PDF:', error);
    ElMessage.error('PDF 다운로드 중 오류가 발생했습니다.');
  }
};
</script>

<style scoped>
.more-actions-dropdown {
  margin-left: 0.25rem;
}
:deep(.el-divider--horizontal) {
  margin: 0.25rem 0;
}

@media (max-width: 768px) {
  :global(.more-actions-popper .el-dropdown-menu__item) {
    padding: 0.75rem 1.25rem !important;
    font-size: 0.95rem !important;
    line-height: 1.5 !important;
  }
}
</style>

