<template>
<div class="memo-sections-grid-luxury">

    <div v-if="modificationHistory.length > 0 && userStore.companyType !== 'RTL'" class="luxury-history-card modification">
      <h5 class="history-card-title">{{ $t('orderDetail.memos.modificationHistory') }}</h5>
      <div v-for="item in modificationHistory" :key="item.id" class="luxury-history-item">
        <div class="h-header">
          <span class="h-author">{{ item.userDisplayName }} ({{ item.companyName }})</span>
          <span class="h-date">{{ formatTime(item.createdAt) }}</span>
        </div>
        <div class="h-content">{{ extractModificationMemo(item.remarks) }}</div>
      </div>
    </div>

    <div v-if="order.logisticsRemarks && userStore.companyType === 'DCC'" class="luxury-memo-card logistics-memo">
      <div class="memo-header-luxury">
        <span class="author">{{ order.userDisplayName }} ({{ order.companyName || '-' }})</span>
        <span class="date">{{ formatTime(order.createdAt) }}</span>
      </div>
      <div class="memo-body">
        <span class="label">{{ $t('orderDetail.memos.logisticsRemarks') }}:</span>
        <pre>{{ order.logisticsRemarks }}</pre>
      </div>
    </div>

    <div v-if="order.factoryRemarks && (userStore.companyType === 'DCC' || userStore.companyType === 'MFG')" class="luxury-memo-card factory-memo">
      <div class="memo-header-luxury" v-if="factoryRemarksInfo">
        <span class="author">{{ factoryRemarksInfo.userDisplayName }} ({{ factoryRemarksInfo.companyName }})</span>
        <span class="date">{{ formatTime(factoryRemarksInfo.createdAt) }}</span>
      </div>
      <div class="memo-body">
        <span class="label">{{ $t('orderDetail.memos.factoryRemarks') }}:</span>
        <pre>{{ order.factoryRemarks }}</pre>
      </div>
    </div>

    <div v-if="order.workOrderRemarks && userStore.companyType !== 'RTL'" class="luxury-memo-card factory-memo">
      <div class="memo-header-luxury" v-if="workOrderRemarksInfo">
        <span class="author">{{ workOrderRemarksInfo.userDisplayName }} ({{ workOrderRemarksInfo.companyName }})</span>
        <span class="date">{{ formatTime(workOrderRemarksInfo.createdAt) }}</span>
      </div>
      <div class="memo-body">
        <span class="label">{{ $t('admin.inspectionRequest.labels.workOrderMemo') }}:</span>
        <pre>{{ order.workOrderRemarks }}</pre>
      </div>
    </div>

    <div v-if="order.inspectionRemarks && userStore.companyType !== 'RTL'" class="luxury-memo-card inspection-memo">
      <div class="memo-header-luxury" v-if="inspectionRemarksInfo">
        <span class="author">{{ inspectionRemarksInfo.userDisplayName }} ({{ inspectionRemarksInfo.companyName }})</span>
        <span class="date">{{ formatTime(inspectionRemarksInfo.createdAt) }}</span>
      </div>
      <div class="memo-body">
        <span class="label">{{ $t('admin.inspectionRequest.labels.inspectionMessage') }}:</span>
        <pre>{{ order.inspectionRemarks }}</pre>
      </div>
    </div>

    <div v-if="order.closeMemo && userStore.companyType !== 'RTL'" class="luxury-memo-card close-memo">
      <div class="memo-header-luxury" v-if="closeMemoInfo">
        <span class="author">{{ closeMemoInfo.userDisplayName }} ({{ closeMemoInfo.companyName }})</span>
        <span class="date">{{ formatTime(closeMemoInfo.createdAt) }}</span>
      </div>
      <div class="memo-body">
        <span class="label">{{ $t('orderDetail.memos.closeMemo') }}:</span>
        <p>{{ order.closeMemo }}</p>
      </div>
    </div>

    <div v-if="order.settlementMethod || order.settlementRemarks" class="luxury-memo-card settlement-memo">
      <div class="memo-header-luxury" v-if="settleRemarksInfo">
        <span class="author">{{ settleRemarksInfo.userDisplayName }} ({{ settleRemarksInfo.companyName }})</span>
        <span class="date">{{ formatTime(settleRemarksInfo.createdAt) }}</span>
      </div>
      <div class="memo-body">
        <span class="label">{{ $t('order.list.requestSettlement') }}:</span>
        <div class="settlement-info-content">
          <div v-if="order.settlementMethod" class="method-row">
            <span class="sub-label">{{ $t('admin.settlement.table.ratio') }}:</span>
            <span class="method-tag">{{ getCodeName(order.settlementMethod) }}</span>
          </div>
          <div v-if="order.settlementAmount" class="amount-row">
            <span class="sub-label">{{ $t('admin.settlement.table.amount') }}:</span>
            <span class="amount-value">₩{{ formatPrice(order.settlementAmount) }}</span>
          </div>
          <div v-if="order.settlementRemarks" class="remarks-row">
            <span class="sub-label">{{ $t('admin.settlement.table.memo') }}:</span>
            <pre class="memo-text">{{ order.settlementRemarks }}</pre>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';

const props = defineProps({
  order: {
    type: Object,
    required: true
  },
  displayCodeMap: {
    type: Object,
    required: true
  },
  userStore: {
    type: Object,
    required: true
  }
});

const formatTime = (time: string) => {
  return parseTime(time, '{y}-{m}-{d} {h}:{i}');
};

const getCodeName = (code: string) => {
  if (!code) return '';
  return props.displayCodeMap[code] || code;
};

const extractMemo = (remarks: string, tag: string) => {
  if (!remarks) return '';
  const parts = remarks.split(tag);
  if (parts.length > 1) {
    return parts[1].split('[')[0].trim();
  }
  return remarks;
};

const extractModificationMemo = (remarks: string) => {
  return extractMemo(remarks, '[수정의뢰 메모]');
};

const modificationHistory = computed(() => {
  if (!props.order || !props.order.statusHistory) return [];
  return props.order.statusHistory.filter((h: any) => h.remarks && h.remarks.includes('[수정의뢰 메모]'));
});

const factoryRemarksInfo = computed(() => {
  if (!props.order || !props.order.statusHistory) return null;
  let info = props.order.statusHistory.find((h: any) => h.remarks && h.remarks.includes('[전달 메시지]'));
  if (!info) {
    info = props.order.statusHistory.find((h: any) => h.status === 'ORDERED');
  }
  return info;
});

const inspectionRemarksInfo = computed(() => {
  if (!props.order || !props.order.statusHistory) return null;
  return props.order.statusHistory.find((h: any) => h.remarks && h.remarks.includes('[검수 요청 메시지]'));
});

const workOrderRemarksInfo = computed(() => {
  if (!props.order || !props.order.statusHistory) return null;
  return props.order.statusHistory.find((h: any) => h.remarks && h.remarks.includes('[작업서 작성 메시지]'));
});

const closeMemoInfo = computed(() => {
  if (!props.order || !props.order.statusHistory) return null;
  return props.order.statusHistory.find((h: any) => h.remarks && h.remarks.includes('[종결 메모]'));
});

const settleRemarksInfo = computed(() => {
  if (!props.order || !props.order.statusHistory) return null;
  return props.order.statusHistory.find((h: any) => h.status === 'PROCESSING');
});
</script>

<style lang="scss" scoped>
.memo-sections-grid-luxury {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
  gap: 1.25rem;
}

.luxury-history-card, .luxury-memo-card {
  background-color: #ffffff;
  border: 1px solid #eae6df;
  border-radius: 2px;
  padding: 1.25rem;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.02);

  .history-card-title {
    font-size: 0.8875rem;
    font-weight: 800;
    color: #c5a880;
    letter-spacing: 1px;
    margin: 0 0 1.25rem 0;
    padding-bottom: 0.625rem;
    border-bottom: 1px solid #f7f5f0;
  }
}

.luxury-history-item {
  margin-bottom: 0.9375rem;
  padding-bottom: 0.95rem;
  border-bottom: 1px dashed #f7f5f0;
  &:last-child { border-bottom: none; margin-bottom: 0; padding-bottom: 0; }

  .h-header {
    display: flex;
    justify-content: space-between;
    margin-bottom: 0.375rem;
    .h-author { font-size: 0.95rem; font-weight: 700; }
    .h-date { font-size: 0.825rem; color: #bbbbbb; }
  }
  .h-content { font-size: 0.9rem; color: #555555; line-height: 1.6; }
}

.luxury-memo-card {
  border-left: 4px solid #eae6df;

  &.order-memo { border-left-color: #67c23a; }
  &.logistics-memo { border-left-color: #c5a880; }
  &.factory-memo { border-left-color: #409EFF; }
  &.inspection-memo { border-left-color: #E6A23C; }
  &.close-memo { border-left-color: #ff4949; background-color: #fff9f9; }
  &.settlement-memo { border-left-color: #67c23a; }

  .memo-header-luxury {
    display: flex;
    justify-content: space-between;
    font-size: 0.8875rem;
    margin-bottom: 0.95rem;
    color: #999;
    .author { font-weight: 700; color: #666; }
  }

  .memo-body {
    .label { font-size: 0.825rem; font-weight: 800; color: #bbbbbb; letter-spacing: 0.5px; display: block; margin-bottom: 0.5rem; }
    p { margin: 0; font-size: 0.875rem; color: #444444; line-height: 1.6; font-weight: 500; }

    .settlement-info-content {
      display: flex;
      flex-direction: column;
      gap: 0.95rem;

      .sub-label {
        font-size: 0.8875rem;
        font-weight: 700;
        color: #999;
        margin-right: 0.5rem;
        min-width: 60px;
        display: inline-block;
      }

      .method-tag {
        font-size: 0.95rem;
        font-weight: 700;
        color: #67c23a;
        background-color: #f0f9eb;
        padding: 0.125rem 0.5rem;
        border: 1px solid #e1f3d8;
        border-radius: 2px;
      }

      .amount-value {
        font-size: 0.875rem;
        font-weight: 700;
        color: #222;
      }

      .memo-text {
        margin: 0;
        font-size: 0.9rem;
        color: #444;
        line-height: 1.5;
        white-space: pre-wrap;
        font-family: inherit;
        background-color: #fbfaf7;
        padding: 0.625rem;
        border: 1px dashed #efebe5;
      }
    }
  }
}

@media (max-width: 768px) {
  .memo-sections-grid-luxury { grid-template-columns: 1fr; }
}
</style>

