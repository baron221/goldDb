<template>
<div class="factory-request-container app-container">
    <factory-request-filter
      :list-query="listQuery"
      :date-range="dateRange"
      :order-status-codes="orderStatusCodes"
      :is-admin="isAdmin"
      @filter="handleFilter"
      @reset="resetQuery"
      @date-change="handleDateChange"
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
      @inspection-request="openCompleteDialog"
      @work-order-create="openWorkOrderDialog"
      @close-by-agreement="handleCloseByAgreement"
      @show-history="openHistoryDialog"
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

    <order-status-history-dialog
      v-model="historyDialogVisible"
      :order-id="selectedOrderId"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, nextTick, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute } from 'vue-router';
import { getAllOrders, updateOrderStatus } from '@/api/order';
import { getCompanies } from '@/api/company';
import useCodeStore from '@/store/modules/code';
import { ElMessage, ElMessageBox } from 'element-plus';
import { parseTime } from '@/utils';
import store from '@/store';
import OrderStatusHistoryDialog from '@/components/OrderStatusHistoryDialog/index.vue';
import InspectionRequestDialog from './components/InspectionRequestDialog.vue';
import WorkOrderCreateDialog from './components/WorkOrderCreateDialog.vue';
import FactoryRequestFilter from './components/FactoryRequestFilter.vue';

const { t } = useI18n();
const codeStore = useCodeStore();
const listLoading = ref(true);
const list = ref<any[]>([]);
const total = ref(0);
const codeMap = ref<Record<string, string>>({});
const companies = ref<any[]>([]);

const historyDialogVisible = ref(false);
const selectedOrderId = ref<number | null>(null);

const openHistoryDialog = (row: any) => {
  selectedOrderId.value = row.id;
  historyDialogVisible.value = true;
};

const completeDialogVisible = ref(false);
const workOrderDialogVisible = ref(false);
const currentOrder = ref<any>(null);

const end = new Date();
const start = new Date();
start.setTime(start.getTime() - 3600 * 1000 * 24 * 30);
const defaultStartDate = parseTime(start, '{y}-{m}-{d}');
const defaultEndDate = parseTime(end, '{y}-{m}-{d}');

const dateRange = ref<string[]>([defaultStartDate, defaultEndDate]);
const orderStatusCodes = ref<any[]>([]);
const defaultImage = 'https://via.placeholder.com/100x100?text=No+Image';
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
  factoryCompanyId: undefined as number | undefined,
  categoryLarge: undefined as string | undefined,
  categoryMedium: undefined as string | undefined,
  categorySmall: undefined as string | undefined,
  setCategoryLarge: undefined as string | undefined,
  setCategoryMedium: undefined as string | undefined,
  setCategorySmall: undefined as string | undefined,
  startDate: defaultStartDate,
  endDate: defaultEndDate,
  excludeCancelled: true,
  excludeCompleted: true,
  isAsOnly: false
});

const fetchOrderStatuses = async () => {
  try {
    await codeStore.fetchCodes();
    const res = codeStore.getCodesByGroupStore('ORDERED_STATUS');

    const allowedStatuses = [
      'FactoryRequested',
      'WorkOrderCreated',
      'InspectedRequested',
      'REQUEST_CLOSE_BY_AGREEMENT',
      'CLOSED_BY_AGREEMENT',
      'Inspected'
    ];

    const filteredCodes = res.filter((c: any) => allowedStatuses.includes(c.code));

    filteredCodes.push({ code: 'Post_Inspected_Group', name: t('order.tabs.completed') });

    orderStatusCodes.value = filteredCodes;
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

    if (!query.status) {
      query.status = 'Factory_Group';
    }

    const [res] = await Promise.all([
      getAllOrders(query),
      codeStore.fetchCodes()
    ]);

    let fetchedList = res.data.items;
    total.value = res.data.totalCount;

    if (listQuery.excludeCancelled) {
      fetchedList = fetchedList.filter((order: any) => order.status !== 'Cancelled');
    }

    if (listQuery.excludeCompleted) {
      const postInspectedStatuses = [
        'PENDING',
        'PROCESSING',
        'SETTLED',
        'SETTLED_CANCELLED',
        'DELIVERY_READY',
        'DELIVERY_IN_TRANSIT',
        'DELIVERED',
        'Completed'
      ];
      fetchedList = fetchedList.filter((order: any) => !postInspectedStatuses.includes(order.status));
    }
    list.value = fetchedList;

    const codes = codeStore.codes;
    if (codes) {
      codeMap.value = { ...codeStore.codeMap };
    }

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

const handleDateChange = (val: string[] | null) => {
  if (val) {
    listQuery.startDate = val[0];
    listQuery.endDate = val[1];
  } else {
    listQuery.startDate = undefined;
    listQuery.endDate = undefined;
  }
  handleFilter();
};

const resetQuery = () => {
  listQuery.orderNo = '';
  listQuery.userName = '';
  listQuery.companyId = undefined;
  listQuery.logisticsCompanyId = undefined;
  listQuery.factoryCompanyId = isAdmin.value ? undefined : store.user().companyId;
  listQuery.categoryLarge = undefined;
  listQuery.categoryMedium = undefined;
  listQuery.categorySmall = undefined;
  listQuery.status = undefined;
  listQuery.startDate = defaultStartDate;
  listQuery.endDate = defaultEndDate;
  listQuery.excludeCancelled = false;
  listQuery.excludeCompleted = false;
  listQuery.isAsOnly = false;
  dateRange.value = [defaultStartDate, defaultEndDate];
  handleFilter();
};

const openCompleteDialog = (row: any) => {
  currentOrder.value = row;
  completeDialogVisible.value = true;
};

const openWorkOrderDialog = (row: any) => {
  currentOrder.value = row;
  workOrderDialogVisible.value = true;
};

const handleCloseByAgreement = (order: any) => {
  ElMessageBox.confirm(`${t('common.confirmStatusUpdate')} "${t('orderDetail.memos.closeMemo')}"?`, t('common.ok'), {
    confirmButtonText: t('common.ok'),
    cancelButtonText: t('common.cancel'),
    type: 'warning'
  }).then(async () => {
    try {
      await updateOrderStatus(order.id, {
        status: 'CLOSED_BY_AGREEMENT'
      });
      ElMessage.success(t('admin.deposit.messages.success'));
      getList();
    } catch (error) {
      ElMessage.error(t('admin.deposit.messages.error'));
    }
  }).catch(() => {});
};

onMounted(() => {
  const route = useRoute();
  if (route.query.status) {
    listQuery.status = route.query.status as string;
  }

  if (!isAdmin.value) {
    listQuery.factoryCompanyId = store.user().companyId;
  }
  fetchCompanies();
  fetchOrderStatuses();
  getList();
});
</script>

<style lang="scss" scoped>
@import "./FactoryRequestStyles.scss";
</style>

