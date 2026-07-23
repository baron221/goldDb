<template>
<el-card shadow="never" class="table-card">
    <base-table
      ref="multipleTable"
      :loading="loading"
      :data="data"
      :total="total"
      :page="page"
      :page-size="pageSize"
      :default-sort="{ prop: 'createdAt', order: 'descending' }"
      @update:page="(val) => $emit('update:page', val)"
      @update:page-size="(val) => $emit('update:page-size', val)"
      style="width: 100%"
      :height="isMobile ? undefined : '100%'"
      row-key="id"
      :expand-row-keys="expandable ? expandedRowKeys : []"
      @expand-change="(row, expandedRows) => $emit('expand-change', row, expandedRows)"
      @change="() => $emit('refresh')"
    >
      <el-table-column v-if="expandable" type="expand">
        <template #header>
          <el-popover ref="popoverRef" placement="bottom-start" trigger="click" width="120">
            <template #reference>
              <i class="fas fa-list-ul" style="color: #909399; font-size: 0.875rem; cursor: pointer;"></i>
            </template>
            <div class="expand-controls" style="background: transparent; padding: 0; justify-content: center;">
              <el-tooltip content="모두 확장" placement="top">
                <span class="ctrl-icon expand-all" @click="expandAll">
                  <i class="fas fa-expand"></i>
                </span>
              </el-tooltip>
              <el-tooltip content="모두 축소" placement="top">
                <span class="ctrl-icon collapse-all" @click="collapseAll">
                  <i class="fas fa-compress"></i>
                </span>
              </el-tooltip>
              <span class="ctrl-divider"></span>
              <el-tooltip content="필요한 행만 확장" placement="top">
                <span class="ctrl-icon expand-action" @click="expandActionableOnly">
                  <i class="fas fa-wand-magic-sparkles"></i>
                </span>
              </el-tooltip>
            </div>
          </el-popover>
        </template>
        <template #default="{row}">
          <order-detail-expand :title="expandTitle" :order="row" :code-map="codeMap" />
        </template>
      </el-table-column>

      <el-table-column
        label="주문 상품"
        min-width="300"
        header-align="center"
        :excel-formatter="(row) => {
          if (!row.orderItems || row.orderItems.length === 0) return '-';
          return row.orderItems.map(item => {
            const name = item.productName || item.productSetTitle || '';
            const qty = item.quantity || 0;
            return `${name} (${qty}개)`;
          }).join('\n');
        }"
      >
        <template #default="{row}">
          <order-goods-cell :order-items="row.orderItems" :order-id="row.id" :order-status="row.status" />
        </template>
      </el-table-column>

      <el-table-column
        prop="orderNo"
        label="주문번호 / 일시"
        width="220"
        align="center"
        :excel-formatter="(row) => `${row.orderNo}\n${formatDate(row.createdAt)}`"
      >
        <template #default="{row}">
          <div class="order-no-info">
            <el-link type="primary" underline="never" style="font-weight: bold;" @click="$emit('order-no-click', row.orderNo)">
              {{ row.orderNo }}
            </el-link>
            <div v-if="row.createdAt" class="order-time-display" style="margin-top: 0.25rem; font-size: 0.8875rem;">
              <span class="date" style="color: #606266;">{{ formatDate(row.createdAt).split(' ')[0] }}</span>
              <span class="time" style="color: #909399; margin-left: 0.3125rem;">{{ formatDate(row.createdAt).split(' ')[1] }}</span>
            </div>
          </div>
        </template>
      </el-table-column>

      <el-table-column
        prop="manufacturerName"
        label="제조사 / 소매점"
        width="180"
        align="center"
        :excel-formatter="(row) => `${row.manufacturerName || '-'}\n${row.companyName || '-'}`"
      >
        <template #default="{row}">
          <div class="company-info">
            <div class="manufacturer-name">
              <el-tag v-if="row.manufacturerName" type="warning" size="small" effect="plain" style="font-size: 0.825rem;">
                {{ row.manufacturerName }}
              </el-tag>
              <span v-else style="color: #c0c4cc; font-size: 0.825rem;">-</span>
            </div>
            <div class="company-name" style="font-weight: bold; color: #303133; font-size: 0.9rem; margin-top: 0.25rem;">
              {{ row.companyName || '-' }}
            </div>
          </div>
        </template>
      </el-table-column>

      <el-table-column prop="deliveryDate" label="납기일" width="100" align="center">
        <template #default="{row}">
          <span v-if="row.deliveryDate" style="color: #409EFF; font-weight: bold;">
            {{ row.deliveryDate.substring(0, 10) }}
          </span>
          <span v-else>-</span>
        </template>
      </el-table-column>

      <el-table-column
        prop="totalAmount"
        label="주문의 총금액"
        width="130"
        align="right"
        :fixed="!isMobile ? 'right' : false"
        :excel-formatter="(row) => `${statusLabel(row.status)} / ₩${formatPrice(getOrderTotalAmount(row, props.userCategory))}`"
      >
        <template #default="{row}">
          <el-tag :type="getStatusType(row.status)" size="small">{{ statusLabel(row.status) }}</el-tag><br />
          <span style="font-size: 0.8875rem; color: #909399; margin-right: 0.1875rem; font-weight: normal;">₩</span><span style="font-weight: bold; color: #f56c6c; font-size: 1.2rem;">{{ formatPrice(getOrderTotalAmount(row, props.userCategory)) }}</span>
        </template>
      </el-table-column>

      <el-table-column label="액션" width="120" align="center" :fixed="!isMobile ? 'right' : false">
        <template #default="{row}">
          <order-action-buttons
            :row="row"
            :user-category="props.userCategory"
            @approve="(r) => $emit('approve', r)"
            @factory-request="(r) => $emit('factory-request', r)"
            @inspection="(r) => $emit('inspection', r)"
            @inspection-request="(r) => $emit('inspection-request', r)"
            @settlement-start="(r) => $emit('settlement-start', r)"
            @settlement-confirm="(r) => $emit('settlement-confirm', r)"
            @status-update="(id, status, label) => $emit('status-update', id, status, label)"
            @work-order-create="(r) => $emit('work-order-create', r)"
            @close-by-agreement="(r) => $emit('close-by-agreement', r)"
          />
        </template>
      </el-table-column>

      <el-table-column label="더보기" width="80" align="center" :fixed="!isMobile ? 'right' : false">
        <template #default="{row}">
          <order-action-more
            :order="row"
            :user-category="props.userCategory"
            @approve="(r) => $emit('approve', r)"
            @factory-request="(r) => $emit('factory-request', r)"
            @inspection="(r) => $emit('inspection', r)"
            @inspection-request="(r) => $emit('inspection-request', r)"
            @settlement-start="(r) => $emit('settlement-start', r)"
            @settlement-confirm="(r) => $emit('settlement-confirm', r)"
            @cancel="(r) => $emit('cancel', r)"
            @stock-exhaustion="(r) => $emit('stock-exhaustion', r)"
            @status-update="(id, status, label) => $emit('status-update', id, status, label)"
            @request-modification="(r) => $emit('request-modification', r)"
            @close-by-agreement="(r) => $emit('close-by-agreement', r)"
            @show-statement="(r) => $emit('show-statement', r)"
            @show-live-statement="(r) => $emit('show-live-statement', r)"
            @show-history="(r) => $emit('show-history', r)"
            @work-order-create="(r) => $emit('work-order-create', r)"
          />
        </template>
      </el-table-column>
    </base-table>
  </el-card>
</template>

<script setup lang="ts">

import { useMobile } from '@/hooks/useMobile';
import BaseTable from '@/components/BaseTable/index.vue';
import OrderDetailExpand from '@/components/OrderDetailExpand/index.vue';
import OrderActionMore from '@/components/OrderActionMore/index.vue';
import OrderGoodsCell from './components/OrderGoodsCell.vue';
import OrderActionButtons from './components/OrderActionButtons.vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import { getStatusLabel, getStatusTagType, getOrderTotalAmount } from '@/utils/order';
import { ref } from 'vue';
const { isMobile } = useMobile();

const props = defineProps({
  loading: { type: Boolean, default: false },
  data: { type: Array, default: () => [] },
  total: { type: Number, default: 0 },
  page: { type: Number, default: 1 },
  pageSize: { type: Number, default: 20 },
  expandedRowKeys: { type: Array, default: () => [] },
  expandable: { type: Boolean, default: true },
  codeMap: { type: Object, default: () => ({}) },
  expandTitle: { type: String, default: '승인 상세 내역' },
  orderStatusCodes: { type: Array, default: () => [] },
  userCategory: { type: String, default: '' }
});

const emit = defineEmits([
  'update:page',
  'update:pageSize',
  'update:expandedRowKeys',
  'expand-change',
  'refresh',
  'order-no-click',
  'approve',
  'factory-request',
  'inspection',
  'inspection-request',
  'work-order-create',
  'settlement-start',
  'settlement-confirm',
  'status-update',
  'cancel',
  'stock-exhaustion',
  'request-modification',
  'close-by-agreement',
  'show-statement',
  'show-live-statement',
  'show-history'
]);

const formatDate = (dateStr: string) => {
  if (!dateStr) return '';
  return parseTime(new Date(dateStr), '{y}-{m}-{d} {h}:{i}');
};

const statusLabel = (status: string) => {
  return getStatusLabel(status, props.userCategory);
};

const getStatusType = (status: string) => {
  return getStatusTagType(status, props.userCategory);
};

const isActionable = (row: any) => {
  if (props.userCategory === 'MFG') {
    return ['FactoryRequested', 'WorkOrderCreated', 'REQUEST_CLOSE_BY_AGREEMENT'].includes(row.status);
  } else {
    return [
      'ORDERED',
      'LogisticsApproved',
      'InspectedRequested',
      'Inspected',
      'PROCESSING',
      'SETTLED',
      'DELIVERY_READY',
      'DELIVERY_IN_TRANSIT'
    ].includes(row.status);
  }
};

const popoverRef = ref();

const expandAll = () => {
  const allIds = (props.data as any[]).map((row: any) => row.id);
  emit('update:expandedRowKeys', allIds);
  popoverRef.value?.hide();
};

const collapseAll = () => {
  emit('update:expandedRowKeys', []);
  popoverRef.value?.hide();
};

const expandActionableOnly = () => {
  const actionableIds = (props.data as any[])
    .filter((row: any) => isActionable(row))
    .map((row: any) => row.id);
  emit('update:expandedRowKeys', actionableIds);
  popoverRef.value?.hide();
};
</script>
<style lang="scss" src="./OrderTableListStyles.scss" scoped></style>

