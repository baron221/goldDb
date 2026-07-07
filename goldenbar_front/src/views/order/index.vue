<template>
<div class="order-page-luxury">
    <div class="dashboard-body-content">

      <section class="section-container">
        <order-filter :query="listQuery" @update:query="(val) => Object.assign(listQuery, val)" @filter="handleFilter" />
      </section>

      <section class="section-container" style="margin-top: 2rem;">
        <div class="status-tabs-luxury">
          <el-tabs v-model="listQuery.status" @tab-change="handleStatusChange">
            <el-tab-pane label="전체 주문" name="" />
            <el-tab-pane label="주문접수" name="ORDERED" />
            <el-tab-pane label="물류승인" name="LogisticsApproved" />
            <el-tab-pane label="공장의뢰" name="FactoryRequested" />
            <el-tab-pane label="작업진행" name="WorkOrderCreated" />
            <el-tab-pane label="검수요청" name="InspectedRequested" />
            <el-tab-pane label="검수완료" name="Inspected" />
            <el-tab-pane label="정산대기" name="PENDING" />
            <el-tab-pane label="정산중" name="PROCESSING" />
            <el-tab-pane label="배송준비" name="DELIVERY_READY" />
            <el-tab-pane label="배송중" name="DELIVERY_IN_TRANSIT" />
            <el-tab-pane label="수령완료" name="Completed" />
          </el-tabs>
        </div>
      </section>

      <section class="section-container" style="margin-top: 1.5rem;" v-loading="loading">
        <order-list
          :orders="orders"
          :search-term="listQuery.orderNo"
          :expanded-order-ids="expandedOrderIds"
          :is-mobile="isMobile"
          @settle="openSettleDialog"
          @statement="openStatementDialog"
          @cancel="handleCancelOrder"
          @toggle-expand="toggleOrderExpand"
        />

        <div class="pagination-wrap-luxury">
          <el-pagination
            v-model:current-page="listQuery.page"
            v-model:page-size="listQuery.pageSize"
            :total="totalCount"
            :page-sizes="[10, 20, 50]"
            layout="total, sizes, prev, pager, next"
            class="luxury-pagination"
            @current-change="getList"
            @size-change="handleFilter"
          />
        </div>
      </section>
    </div>

    <transaction-statement-dialog
      v-model="statementDialogVisible"
      :order="selectedOrderForStatement"
      :code-map="codeMap"
      :statement="currentStatementData"
    />

    <settlement-request-dialog
      v-model="settleDialogVisible"
      @submit="handleSettleSubmit"
    />
  </div>
</template>

<script setup lang="ts">
import { onMounted, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { ElMessageBox } from 'element-plus';
import { useMobile } from '@/hooks/useMobile';
import { useOrder } from './composables/useOrder';
import OrderFilter from './components/OrderFilter.vue';
import OrderList from './components/OrderList.vue';
import SettlementRequestDialog from './components/SettlementRequestDialog.vue';
import TransactionStatementDialog from '@/components/TransactionStatementDialog/index.vue';
import useCodeStore from '@/store/modules/code';

const { isMobile } = useMobile();
const codeStore = useCodeStore();
const codeMap = computed(() => codeStore.codeMap);
const { t } = useI18n();
const {
  orders,
  totalCount,
  loading,
  listQuery,
  expandedOrderIds,
  settleDialogVisible,
  currentSettleOrder,
  statementDialogVisible,
  selectedOrderForStatement,
  currentStatementData,
  getList,
  handleFilter,
  handleStatusChange,
  toggleOrderExpand,
  updateStatus,
  openSettleDialog,
  openStatementDialog
} = useOrder();

const handleSettleSubmit = async (formData: any) => {
  if (!currentSettleOrder.value) return;
  const success = await updateStatus(currentSettleOrder.value.id, {
    status: '정산확인요청',
    ...formData
  });
  if (success) {
    settleDialogVisible.value = false;
  }
};

const handleCancelOrder = (order: any) => {
  ElMessageBox.prompt(t('order.list.cancellationReason'), t('order.list.cancelOrder'), {
    confirmButtonText: t('common.ok'),
    cancelButtonText: t('common.cancel'),
    inputPlaceholder: t('order.list.cancellationReason'),
    inputValidator: (value) => {
      if (!value || value.trim().length === 0) {
        return t('order.list.cancellationReason');
      }
      return true;
    }
  }).then(async ({ value }) => {
    await updateStatus(order.id, {
      status: 'Cancelled',
      cancellationReason: value
    });
  }).catch(() => {});
};

onMounted(() => {
  getList();
  codeStore.fetchCodes();
});
</script>

<style lang="scss" scoped>
.order-page-luxury {
  background-color: #fdfcf9;
  min-height: 100vh;
  padding-bottom: 5rem;
}

.dashboard-body-content {
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 2rem;
}

.section-container {
  animation: fadeInUp 0.6s ease backwards;
}

@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.filter-card-luxury {
  background: white;
  border: 1px solid #eae6df;
  padding: 1.5rem 2rem;
  box-shadow: 0 4px 20px rgba(0,0,0,0.03);
}

.status-tabs-luxury {
  :deep(.el-tabs__header) { margin-bottom: 0; border-bottom: 1px solid #eae6df; }
  :deep(.el-tabs__nav-wrap::after) { display: none; }
  :deep(.el-tabs__active-bar) { background-color: #c5a880; height: 3px; }
  :deep(.el-tabs__item) {
    font-size: 1rem;
    font-weight: 500;
    color: #999;
    padding: 0 1.5rem;
    height: 50px;
    line-height: 50px;
    &.is-active { color: #222; font-weight: 700; }
    &:hover { color: #c5a880; }
  }
}

.pagination-wrap-luxury {
  padding: 2.5rem 0;
  display: flex;
  justify-content: center;
}

@media (max-width: 991px) {
  .dashboard-body-content { padding: 0 1rem; }
}

:global(html.dark) {
  .order-page-luxury {
    background-color: #121212;
  }

  .filter-card-luxury {
    background-color: #1a1a1a;
    border-color: #333333;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.2);
  }

  .status-tabs-luxury {
    :deep(.el-tabs__header) {
      border-bottom-color: #333333;
    }
    :deep(.el-tabs__item) {
      color: #888888;
      &.is-active {
        color: #ffffff;
      }
      &:hover {
        color: #c5a880;
      }
    }
  }

  :deep(.luxury-pagination) {
    background-color: transparent !important;

    .btn-prev, .btn-next, .el-pager li {
      background-color: #1a1a1a !important;
      border-color: #333333 !important;
      color: #888888 !important;

      &:hover {
        color: #c5a880 !important;
        border-color: #c5a880 !important;
      }
    }

    .el-pager li.is-active {
      background-color: #c5a880 !important;
      border-color: #c5a880 !important;
      color: #ffffff !important;
    }

    .el-pagination__total,
    .el-pagination__jump,
    .el-pagination__classifier {
      color: #888888 !important;
    }
  }
}
</style>

