<template>
<div class="logistics-approval-container app-container">
    <logistics-approval-filter
      :list-query="listQuery"
      @update:list-query="(val) => Object.assign(listQuery, val)"
      :order-status-codes="orderStatusCodes"
      :is-admin="isAdmin"
      v-model:large-id="largeId"
      v-model:medium-id="mediumId"
      v-model:set-large-id="setLargeId"
      v-model:set-medium-id="setMediumId"
      @filter="handleFilter"
      @reset="resetQuery"
    />

    <order-table-list
      :loading="listLoading"
      :data="list"
      :total="total"
      v-model:page="listQuery.page"
      v-model:page-size="listQuery.pageSize"
      v-model:expanded-row-keys="expandedRowKeys"
      :code-map="codeMap"
      :order-status-codes="orderStatusCodes"
      :user-category="store.user().companyType"
      @expand-change="handleExpandChange"
      @refresh="getList"
      @order-no-click="handleOrderNoClick"
      @approve="openApprovalDialog"
      @factory-request="openRequestDialog"
      @inspection="openInspectionDialog"
      @settlement-start="openRetailerSettlementDialog"
      @settlement-confirm="handleConfirmSettlement"
      @cancel="handleCancelOrder"
      @stock-exhaustion="handleStockExhaustion"
      @status-update="handleStatusUpdate"
      @request-modification="handleRequestModification"
      @close-by-agreement="handleCloseByAgreement"
      @show-statement="handleShowStatement"
      @show-live-statement="handleShowLiveDataStatement"
      @show-history="openHistoryDialog"
      @inspection-request="openCompleteDialog"
      @work-order-create="openWorkOrderDialog"
    />

    <factory-request-dialog
      v-model="requestDialogVisible"
      :order="currentOrder"
      :code-map="codeMap"
      @completed="getList"
    />

    <logistics-approval-dialog
      v-model="approvalDialogVisible"
      :order="currentOrder"
      :code-map="codeMap"
      @completed="getList"
    />

    <order-status-history-dialog
      v-model="historyDialogVisible"
      :order-id="selectedOrderId"
    />

    <InspectionRequestDialog
      v-model="completeDialogVisible"
      :order="currentOrder"
      :code-map="codeMap"
      @saved="getList"
    />

    <WorkOrderCreateDialog
      v-model="workOrderDialogVisible"
      :order="currentOrder"
      :code-map="codeMap"
      @saved="getList"
    />

    <inspection-dialog
      v-model="inspectionDialogVisible"
      :order="currentOrder"
      :code-map="codeMap"
      @completed="getList"
    />

    <order-stock-exhaustion-dialog
      v-model="exhaustionDialogVisible"
      :order="currentOrder"
      @completed="getList"
    />

    <retailer-settlement-dialog
      v-model="retailerSettlementDialogVisible"
      :order="currentOrder"
      @completed="getList"
    />

    <transaction-statement-dialog
      ref="statementRef"
      v-model="statementDialogVisible"
      :order="currentOrder"
      :code-map="codeMap"
      :statement="currentStatement"
    />

    <settlement-confirm-dialog
      v-model="settlementConfirmDialogVisible"
      :order="currentOrder"
      :loading="listLoading"
      @confirm="onSettlementConfirmed"
    />
  </div>
</template>

<script setup lang="ts">

import { ref, reactive, onMounted, nextTick, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute } from 'vue-router';
import { getAllOrders, updateOrderStatus, saveOrderStatement, getOrderStatement } from '@/api/order';
import { getCompanies } from '@/api/company';
import useCodeStore from '@/store/modules/code';
import { ElMessage } from 'element-plus';
import { parseTime } from '@/utils';
import store from '@/store';
import OrderStatusHistoryDialog from '@/components/OrderStatusHistoryDialog/index.vue';
import InspectionDialog from '@/components/InspectionDialog/index.vue';
import LogisticsApprovalDialog from '@/components/LogisticsApprovalDialog/index.vue';
import FactoryRequestDialog from '@/components/FactoryRequestDialog/index.vue';
import RetailerSettlementDialog from '@/components/RetailerSettlementDialog/index.vue';
import OrderStockExhaustionDialog from './components/OrderStockExhaustionDialog.vue';
import TransactionStatementDialog from '@/components/TransactionStatementDialog/index.vue';
import OrderTableList from '@/components/OrderTableList/index.vue';
import InspectionRequestDialog from './components/InspectionRequestDialog.vue';
import WorkOrderCreateDialog from './components/WorkOrderCreateDialog.vue';
import SettlementConfirmDialog from '@/components/SettlementConfirmDialog/index.vue';
import { useLogisticsApprovalActions } from './composables/useLogisticsApprovalActions';
import LogisticsApprovalFilter from './components/LogisticsApprovalFilter.vue';

const { t } = useI18n();
const route = useRoute();
const codeStore = useCodeStore();
const listLoading = ref(true);
const list = ref<any[]>([]);
const total = ref(0);
const companies = ref<any[]>([]);
const codeMap = computed(() => codeStore.codeMap);

const currentOrder = ref<any>(null);
const historyDialogVisible = ref(false); const requestDialogVisible = ref(false); const exhaustionDialogVisible = ref(false); const approvalDialogVisible = ref(false); const inspectionDialogVisible = ref(false); const completeDialogVisible = ref(false); const workOrderDialogVisible = ref(false);
const selectedOrderId = ref<number | null>(null);
const statementDialogVisible = ref(false); const statementRef = ref<any>(null); const currentStatement = ref<any>(null);
const retailerSettlementDialogVisible = ref(false); const settlementConfirmDialogVisible = ref(false);

const openRetailerSettlementDialog = (row: any) => { currentOrder.value = row; retailerSettlementDialogVisible.value = true; };
const openHistoryDialog = (row: any) => { selectedOrderId.value = row.id; historyDialogVisible.value = true; };
const handleStockExhaustion = (row: any) => { currentOrder.value = row; exhaustionDialogVisible.value = true; };
const openRequestDialog = (row: any) => { currentOrder.value = row; requestDialogVisible.value = true; };
const openCompleteDialog = (row: any) => { currentOrder.value = row; completeDialogVisible.value = true; };
const openWorkOrderDialog = (row: any) => { currentOrder.value = row; workOrderDialogVisible.value = true; };

const end = new Date();
const start = new Date();
start.setTime(start.getTime() - 3600 * 1000 * 24 * 30);
const defaultStartDate = parseTime(start, '{y}-{m}-{d}');
const defaultEndDate = parseTime(end, '{y}-{m}-{d}');

const orderStatusCodes = ref<any[]>([]);
const expandedRowKeys = ref<number[]>([]);
const scrollPosition = ref(0);

const isAdmin = computed(() => {
  return store.user().roles.includes('admin');
});

const listQuery = reactive({
  page: 1,
  pageSize: 20,
  status: undefined as string | undefined,
  orderNo: '',
  userName: '',
  companyId: undefined as number | undefined,
  logisticsCompanyId: undefined as number | undefined,
  startDate: defaultStartDate,
  endDate: defaultEndDate,
  excludeCancelled: true,
  excludeCompleted: true,
  isAsOnly: false,
  categoryLarge: '',
  categoryMedium: '',
  categorySmall: '',
  setCategoryLarge: '',
  setCategoryMedium: '',
  setCategorySmall: ''
});

const largeId = ref<number | null>(null);
const mediumId = ref<number | null>(null);
const setLargeId = ref<number | null>(null);
const setMediumId = ref<number | null>(null);

const fetchOrderStatuses = async () => {
  try {
    await codeStore.fetchCodes();
    orderStatusCodes.value = codeStore.getCodesByGroupStore('ORDERED_STATUS');
  } catch (error) {
    console.error('Failed to fetch order status codes:', error);
  }
};

const fetchCompanies = async () => {
  try {
    const res = await getCompanies({ page: 1, pageSize: 1000 });
    companies.value = res.data.items || res.data;
  } catch (error) {
    console.error('Failed to fetch companies:', error);
  }
};

const getList = async () => {
  const container = document.querySelector('.app-main');
  if (container) {
    scrollPosition.value = container.scrollTop;
  }

  listLoading.value = true;
  try {
    const query = { ...listQuery } as any;

    const [res] = await Promise.all([
      getAllOrders(query),
      codeStore.fetchCodes()
    ]);
    list.value = res.data.items;
    total.value = res.data.totalCount;

    await nextTick();
    if (container) {
      container.scrollTop = scrollPosition.value;
    }
  } catch (error) {
    console.error('Failed to get orders:', error);
  } finally {
    listLoading.value = false;
  }
};

const { handleStatusUpdate, handleCancelOrder, handleRequestModification, handleCloseByAgreement } = useLogisticsApprovalActions(t, getList);

const handleExpandChange = (row: any, expandedRows: any[]) => {
  expandedRowKeys.value = expandedRows.map(r => r.id);
};

const handleFilter = () => {
  listQuery.page = 1;
  getList();
};

const handleOrderNoClick = (orderNo: string) => {
  listQuery.orderNo = orderNo;
  handleFilter();
};

const resetQuery = () => {
  listQuery.status = undefined;
  listQuery.orderNo = '';
  listQuery.userName = '';
  listQuery.companyId = undefined;
  listQuery.logisticsCompanyId = isAdmin.value ? undefined : store.user().companyId;
  listQuery.startDate = defaultStartDate;
  listQuery.endDate = defaultEndDate;
  listQuery.excludeCancelled = false;
  listQuery.excludeCompleted = false;
  listQuery.isAsOnly = false;
  listQuery.categoryLarge = '';
  listQuery.categoryMedium = '';
  listQuery.categorySmall = '';
  listQuery.setCategoryLarge = '';
  listQuery.setCategoryMedium = '';
  listQuery.setCategorySmall = '';
  largeId.value = null;
  mediumId.value = null;
  setLargeId.value = null;
  setMediumId.value = null;
  handleFilter();
};

const handleConfirmSettlement = (order: any) => {
  currentOrder.value = order;
  settlementConfirmDialogVisible.value = true;
};

const onSettlementConfirmed = async (order: any) => {
  try {
    listLoading.value = true;
    currentOrder.value = order;
    await nextTick();

    const statementData = await statementRef.value?.generateSnapshotAndPdf();

    if (statementData) {
      await saveOrderStatement(order.id, {
        snapshotData: statementData.snapshot,
        pdfBase64: statementData.pdfBase64
      });
    }

    await updateOrderStatus(order.id, {
      status: 'SETTLED',
      settlementAmount: order.settlementAmount
    });

    ElMessage.success(t('admin.settlement.messages.settlementSuccess'));
    settlementConfirmDialogVisible.value = false;
    getList();
  } catch (error) {
    console.error('정산 처리 중 오류:', error);
    ElMessage.error(t('admin.settlement.messages.settlementError'));
  } finally {
    listLoading.value = false;
  }
};

const openApprovalDialog = (row: any) => {
  currentOrder.value = row;
  approvalDialogVisible.value = true;
};

const handleShowStatement = async (order: any) => {
  currentOrder.value = order;
  currentStatement.value = null;

  if (order.hasStatement) {
    try {
      const response: any = await getOrderStatement(order.id);
      if (response.data) {
        currentStatement.value = response.data;
      }
    } catch (error) {
      console.error('Failed to fetch stored statement:', error);
    }
  }

  statementDialogVisible.value = true;
};

const handleShowLiveDataStatement = (order: any) => {
  currentOrder.value = order;
  currentStatement.value = null;
  statementDialogVisible.value = true;
};

const openInspectionDialog = (row: any) => {
  currentOrder.value = row;
  inspectionDialogVisible.value = true;
};

onMounted(() => {
  if (!isAdmin.value) {
    listQuery.logisticsCompanyId = store.user().companyId;
  }

  if (route.query.logisticsCompanyId) {
    listQuery.logisticsCompanyId = Number(route.query.logisticsCompanyId);
  }
  if (route.query.companyId) {
    listQuery.companyId = Number(route.query.companyId);
  }

  fetchCompanies();
  fetchOrderStatuses();
  getList();
});
</script>

<style scoped src="./components/LogisticsApprovalStyles.scss"></style>

