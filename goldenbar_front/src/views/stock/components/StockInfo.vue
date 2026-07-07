<template>
<div class="stock-info-section">

    <div class="meta-info-row">
      <span class="company-name">{{ stock.companyName || stock.ownerCompanyName || '' }}</span>
      <div class="right-meta">
        <span class="stock-badge">STOCK</span>
        <span class="stock-no-label" v-if="stock.stockNo">{{ $t('stock.labels.no') }} {{ stock.stockNo }}</span>
      </div>
    </div>

    <h1 class="stock-title">{{ displayTitle }}</h1>
    <div class="product-no-label" v-if="stock.productNo">Ref. {{ stock.productNo }}</div>

    <div class="divider-thin"></div>

    <div class="spec-grid">
      <div class="spec-item">
        <span class="spec-label">{{ $t('stock.filter.status') }}</span>
        <span class="spec-value">
          <el-tag :type="getStatusTagType(stock.status)" effect="dark">{{ getStatusName(stock.status) }}</el-tag>
        </span>
      </div>
      <div class="spec-item">
        <span class="spec-label">{{ $t('stockDetail.purity') }}</span>
        <span class="spec-value">{{ codeMap[stock.purity] || stock.purity || '-' }}</span>
      </div>
      <div class="spec-item" v-if="stock.color">
        <span class="spec-label">{{ $t('stockDetail.color') }}</span>
        <span class="spec-value">{{ codeMap[stock.color] || stock.color }}</span>
      </div>
      <div class="spec-item">
        <span class="spec-label">{{ $t('stockDetail.actualWeight') }}</span>
        <span class="spec-value weight-highlight">{{ stock.actualWeight }}<em>g</em></span>
      </div>
      <div class="spec-item">
        <span class="spec-label">{{ $t('notice.labels.date') }} (입고)</span>
        <span class="spec-value">{{ formatDate(stock.createdAt, '{y}-{m}-{d}') }}</span>
      </div>
      <div class="spec-item" v-if="stock.sourceOrderDate">
        <span class="spec-label">{{ $t('stock.labels.orderDate') }}</span>
        <span class="spec-value">{{ formatDate(stock.sourceOrderDate, '{y}-{m}-{d}') }}</span>
      </div>
    </div>

    <div class="divider-thin"></div>

    <div class="cost-info-grid" v-if="stock.retailerConfirmMaterialCost !== null || stock.retailerConfirmLaborCost !== null">
      <div class="cost-item">
        <span class="cost-label">{{ $t('orderDetail.headers.settlement') }} {{ $t('orderDetail.settlement.material') }}</span>
        <span class="cost-value">₩ {{ formatPrice(stock.retailerConfirmMaterialCost) }}</span>
      </div>
      <div class="cost-item">
        <span class="cost-label">{{ $t('orderDetail.headers.settlement') }} {{ $t('orderDetail.settlement.labor') }}</span>
        <span class="cost-value">₩ {{ formatPrice(stock.retailerConfirmLaborCost) }}</span>
      </div>
    </div>

    <div class="divider-thin" v-if="stock.retailerConfirmMaterialCost !== null || stock.retailerConfirmLaborCost !== null"></div>

    <div class="category-row" v-if="categoryPath">
      <span class="cat-label">{{ $t('stock.filter.category') }}</span>
      <span class="cat-value">{{ categoryPath }}</span>
    </div>

    <div class="note-row">
      <span class="note-label">{{ $t('orderDetail.headers.approvedMemo') }}</span>
      <span class="note-value">{{ stock.note || '-' }}</span>
    </div>

    <div class="rent-section" v-if="stock.status === 'RENTED'">
      <div class="rent-row">
        <span class="rent-label">대여자</span>
        <span class="rent-value">{{ stock.renterName }}</span>
      </div>
      <div class="rent-row">
        <span class="rent-label">반납예정일</span>
        <span class="rent-value warning-text">{{ formatDate(stock.returnDueDate, '{y}-{m}-{d}') }}</span>
      </div>
    </div>

    <div class="divider-thin"></div>

    <div class="action-row">
      <el-button class="btn-back btn-jovenca" @click="$emit('back')">
        <el-icon><ArrowLeft /></el-icon> {{ $t('order.filters.to') }} {{ $t('common.action') }}
      </el-button>
      <el-button
        v-if="stock.sourceOrderNo"
        type="primary"
        class="btn-order btn-jovenca"
        @click="$emit('go-to-order', stock.sourceOrderNo)"
      >
        {{ $t('stockDetail.actions.save') }} ({{ stock.sourceOrderNo }})
      </el-button>
    </div>

    <el-collapse
      :model-value="activeCollapseNames"
      @update:model-value="$emit('update:activeCollapseNames', $event)"
      class="jovenca-accordion"
    >

      <el-collapse-item name="order" v-if="stock.sourceOrderId">
        <template #title>
          <span class="accordion-title">{{ $t('stockDetail.originInfo') }} (STOCK IN)</span>
        </template>
        <div class="accordion-body">
          <div class="info-grid-2col">
            <div class="info-block">
              <div class="block-label">{{ $t('stockDetail.logistics') }}</div>
              <div class="block-row"><span class="bl">{{ $t('sys.company.labels.name') }}</span><span class="bv">{{ stock.logisticsCompanyName || '-' }}</span></div>
            </div>
            <div class="info-block">
              <div class="block-label">{{ $t('stockDetail.manufacturer') }}</div>
              <div class="block-row"><span class="bl">{{ $t('sys.company.labels.name') }}</span><span class="bv">{{ stock.companyName || '-' }}</span></div>
            </div>
          </div>
          <div class="remarks-section" v-if="stock.sourceOrder">
            <div class="remark-row" v-if="stock.sourceOrder.factoryRemarks">
              <span class="rk-label">{{ $t('orderDetail.memos.factoryRemarks') }}</span>
              <span class="rk-value">{{ stock.sourceOrder.factoryRemarks }}</span>
            </div>
            <div class="remark-row" v-if="stock.sourceOrder.status === 'Cancelled' && stock.sourceOrder.cancellationReason">
              <span class="rk-label cancelled">{{ $t('order.list.cancellationReason') }}</span>
              <span class="rk-value cancelled">{{ stock.sourceOrder.cancellationReason }}</span>
            </div>
          </div>
        </div>
      </el-collapse-item>

      <el-collapse-item name="history" v-if="stock.orderHistory && stock.orderHistory.length > 0">
        <template #title>
          <span class="accordion-title">{{ $t('stockDetail.history') }} (ORDER HISTORY)</span>
        </template>
        <div class="accordion-body">
          <div class="timeline-list">
            <div
              v-for="(item, index) in stock.orderHistory"
              :key="index"
              class="timeline-item"
              :class="{ 'is-first': index === 0 }"
            >
              <div class="tl-dot"></div>
              <div class="tl-content">
                <div class="tl-header">
                  <el-tag size="small" :type="getOrderStatusTagType(item.status)" class="tl-tag">{{ getOrderStatusName(item.status) }}</el-tag>
                  <span class="tl-date">{{ formatDate(item.createdAt) }}</span>
                  <span class="tl-user" v-if="item.userDisplayName">| {{ item.userDisplayName }} ({{ item.companyName || '시스템' }})</span>
                </div>
                <div class="tl-note" v-if="item.remarks">{{ item.remarks }}</div>
              </div>
            </div>
          </div>
        </div>
      </el-collapse-item>
    </el-collapse>
  </div>
</template>

<script setup lang="ts">
import { ArrowLeft } from '@element-plus/icons-vue';
import { useI18n } from 'vue-i18n';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';

defineProps({
  stock: {
    type: Object,
    required: true
  },
  codeMap: {
    type: Object,
    required: true
  },
  activeCollapseNames: {
    type: Array as () => string[],
    required: true
  },
  displayTitle: {
    type: String,
    required: true
  },
  categoryPath: {
    type: String,
    required: true
  }
});

defineEmits([
  'back',
  'go-to-order',
  'update:activeCollapseNames'
]);

const { t } = useI18n();

const getStatusName = (status: string) => {
  return t(`stock.status.${status}`) || status;
};

const getStatusTagType = (status: string) => {
  switch (status) {
    case 'IN_STOCK': return 'success';
    case 'RENTED': return 'warning';
    case 'SOLD': return 'info';
    case 'DAMAGED':
    case 'LOST':
    case 'DISCARDED': return 'danger';
    default: return '';
  }
};

const getOrderStatusName = (status: string) => {
  return t(`order.status.${status}`) || status;
};

const getOrderStatusTagType = (status: string) => {
  const map: Record<string, string> = {
    'ORDERED': 'info',
    'LogisticsApproved': 'success',
    'FactoryRequested': 'warning',
    'InspectedRequested': 'primary',
    'Inspected': 'success',
    'PENDING': 'warning',
    'Completed': 'info',
    'Cancelled': 'danger'
  };
  return map[status] || 'info';
};

const formatDate = (dateStr: string, format = '{y}-{m}-{d} {h}:{i}') => {
  if (!dateStr) return '-';
  return parseTime(new Date(dateStr), format);
};
</script>

