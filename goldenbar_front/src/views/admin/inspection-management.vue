<template>
<div class="inspection-management-container app-container">

    <inspection-management-filter
      :query="listQuery"
      @filter="handleFilter"
      @reset="resetQuery"
    />

    <el-card shadow="never" style="margin-top: 1.25rem;">
      <base-table
        ref="multipleTable"
        v-loading="listLoading"
        :data="list"
        border
        fit
        highlight-current-row
        style="width: 100%"
        row-key="id"
        :expand-row-keys="expandedRowKeys"
        @expand-change="handleExpandChange"
      >
        <el-table-column type="expand">
          <template #default="{row}">
            <div class="order-detail-expand">
              <h4>{{ $t('admin.inspectionRequest.title') }} {{ $t('admin.settlement.table.expandTitle') }}</h4>
              <base-table :data="row.orderItems" border style="width: 100%">
                <el-table-column prop="productName" :label="$t('admin.inspectionRequest.headers.productInfo')" min-width="250" :excel-formatter="innerProductInfoFormatter">
                  <template #default="item">
                    <div class="product-info-cell">
                      <el-image :src="item.row.photoUrl || defaultImage" fit="cover" class="product-thumb" />
                      <div class="product-text">
                        <div v-if="item.row.productSetId" class="set-tag">SET</div>
                        <div class="product-name">{{ item.row.productName || item.row.productSetTitle }}</div>
                        <div class="product-no">{{ item.row.productNo }}</div>
                        <div class="product-options" v-if="item.row.purity || item.row.color">
                          <el-tag size="small" type="info" effect="plain" v-if="item.row.purity">{{ codeMap[item.row.purity] || item.row.purity }}</el-tag>
                          <el-tag size="small" type="info" effect="plain" v-if="item.row.color" style="margin-left: 0.3125rem;">{{ codeMap[item.row.color] || item.row.color }}</el-tag>
                        </div>
                      </div>
                    </div>
                  </template>
                </el-table-column>
                <el-table-column :label="$t('admin.inspectionRequest.headers.qty')" width="80" align="center" prop="quantity" />
                <el-table-column prop="actualWeight" :label="$t('admin.inspectionRequest.headers.factoryRequest')" width="120" align="center" :excel-formatter="(row) => row.actualWeight ? row.actualWeight + 'g' : '-'">
                  <template #default="item">
                    <span style="font-weight: bold;">{{ item.row.actualWeight ? item.row.actualWeight + 'g' : '-' }}</span>
                  </template>
                </el-table-column>
                <el-table-column :label="$t('stockDetail.purity')" width="100" align="center" prop="purity" />
                <el-table-column prop="isAsOrder" :label="$t('admin.orderApproval.headers.asOrder')" width="80" align="center" :excel-formatter="(row) => row.isAsOrder ? 'AS' : '-'">
                  <template #default="item">
                    <el-tag v-if="item.row.isAsOrder" type="danger" size="small">AS</el-tag>
                    <span v-else>-</span>
                  </template>
                </el-table-column>
                <el-table-column :label="$t('admin.inspectionRequest.placeholders.memo')" min-width="150" prop="inspectionMemo" />
              </base-table>
              <div v-if="row.factoryRemarks" class="factory-remarks">
                <p><strong>{{ $t('admin.orderApproval.additionalMessage') }}:</strong> {{ row.factoryRemarks }}</p>
              </div>
            </div>
          </template>
        </el-table-column>

        <el-table-column :label="$t('order.filters.orderNo')" prop="orderNo" width="220" align="center" />
        <el-table-column prop="userDisplayName" :label="$t('order.list.orderer')" width="180" align="center" :excel-formatter="userFormatter">
          <template #default="{row}">
            <span>{{ row.userDisplayName }} ({{ row.userName }})</span>
          </template>
        </el-table-column>
        <el-table-column prop="totalAmount" :label="$t('order.list.totalAmount')" width="150" align="right" :excel-formatter="amountFormatter">
          <template #default="{row}">
            <span style="font-size: 0.8875rem; color: #909399; margin-right: 0.1875rem; font-weight: normal;">₩</span><span style="font-weight: bold;">{{ formatPrice(getOrderTotalAmount(row)) }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" :label="$t('order.filters.dateRange')" width="180" align="center" :excel-formatter="(row) => formatDate(row.createdAt)">
          <template #default="{row}">
            <span>{{ formatDate(row.createdAt) }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="status" :label="$t('stock.filter.status')" width="100" align="center" :fixed="!isMobile ? 'right' : false" :excel-formatter="(row) => statusLabel(row.status)">
          <template #default="{row}">
            <el-tag :type="getStatusType(row.status)" size="small">{{ statusLabel(row.status) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column :label="$t('common.action')" width="180" align="center" :fixed="!isMobile ? 'right' : false">
          <template #default="{row}">
            <div class="action-buttons">
              <el-button
                v-if="row.status === 'InspectedRequested'"
                type="primary"
                size="small"
                @click="openInspectionDialog(row)"
              >
                {{ $t('common.ok') }}
              </el-button>
            </div>
          </template>
        </el-table-column>
      </base-table>

      <div class="pagination-container">
        <el-pagination
          v-model:current-page="listQuery.page"
          v-model:page-size="listQuery.pageSize"
          :total="total"
          :page-sizes="[10, 20, 30, 50]"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="getList"
          @current-change="getList"
        />
      </div>
    </el-card>

    <inspection-dialog
      v-model="inspectionDialogVisible"
      :order="currentOrder"
      :code-map="codeMap"
      @completed="getList"
    />
  </div>
</template>

<script setup lang="ts">
import { useMobile } from '@/hooks/useMobile';
import { ref, reactive, onMounted, nextTick, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { getAllOrders } from '@/api/order';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import BaseTable from '@/components/BaseTable/index.vue';
import { isPostPendingStatus } from '@/utils/order';
import useCodeStore from '@/store/modules/code';
import InspectionDialog from '@/components/InspectionDialog/index.vue';
import InspectionManagementFilter from './components/InspectionManagementFilter.vue';

const { isMobile } = useMobile();
const { t } = useI18n();

const listLoading = ref(true);
const list = ref<any[]>([]);
const total = ref(0);
const codeStore = useCodeStore();
const codeMap = computed(() => codeStore.codeMap);
const orderStatusCodes = ref<any[]>([]);
const defaultImage = 'https://via.placeholder.com/100x100?text=No+Image';
const expandedRowKeys = ref<number[]>([]);
const scrollPosition = ref(0);

const end = new Date();
const start = new Date();
start.setTime(start.getTime() - 3600 * 1000 * 24 * 30);
const defaultStartDate = parseTime(start, '{y}-{m}-{d}');
const defaultEndDate = parseTime(end, '{y}-{m}-{d}');

const listQuery = reactive({
  page: 1,
  pageSize: 20,
  status: 'Inspection_Group',
  orderNo: '',
  userName: '',
  companyId: undefined as number | undefined,
  categoryLarge: '',
  categoryMedium: '',
  categorySmall: '',
  setCategoryLarge: '',
  setCategoryMedium: '',
  setCategorySmall: '',
  startDate: defaultStartDate,
  endDate: defaultEndDate
});

const inspectionDialogVisible = ref(false);
const currentOrder = ref<any>(null);

const getOrderTotalAmount = (order: any) => {
  const isPostPending = isPostPendingStatus(order.status);

  if (isPostPending && order.orderItems && order.orderItems.length > 0) {
    const topLevelItems = order.orderItems.filter((item: any) => !item.parentId);
    return topLevelItems.reduce((acc: number, item: any) => {
      const material = item.retailerConfirmMaterialCost || 0;
      const labor = item.retailerConfirmLaborCost || 0;
      return acc + (material + labor) * item.quantity;
    }, 0);
  }
  return order.totalAmount;
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return '';
  return parseTime(new Date(dateStr), '{y}-{m}-{d} {h}:{i}');
};

const statusLabel = (status: string) => {
  const codeObj = orderStatusCodes.value.find(c => c.code === status);
  return codeObj ? codeObj.name : status;
};

const getStatusType = (status: string) => {
  const map: Record<string, string> = {
    'ORDERED': 'info',
    'LogisticsApproved': 'success',
    'FactoryRequested': 'warning',
    'InspectedRequested': 'primary',
    'Inspected': 'success',
    'Completed': 'info',
    'Cancelled': 'danger'
  };
  return map[status] || 'info';
};

const fetchOrderStatuses = async () => {
  try {
    await codeStore.fetchCodes();
    orderStatusCodes.value = codeStore.getCodesByGroupStore('ORDERED_STATUS');
  } catch (error) {
    console.error('Failed to fetch order status codes:', error);
  }
};

const getList = async () => {
  const container = document.querySelector('.app-main');
  if (container) {
    scrollPosition.value = container.scrollTop;
  }

  listLoading.value = true;
  try {
    const [res] = await Promise.all([
      getAllOrders(listQuery),
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

const handleFilter = () => {
  listQuery.page = 1;
  getList();
};

const resetQuery = () => {
  listQuery.orderNo = '';
  listQuery.userName = '';
  listQuery.companyId = undefined;
  listQuery.startDate = undefined;
  listQuery.endDate = undefined;
  listQuery.categoryLarge = '';
  listQuery.categoryMedium = '';
  listQuery.categorySmall = '';
  listQuery.setCategoryLarge = '';
  listQuery.setCategoryMedium = '';
  listQuery.setCategorySmall = '';
  handleFilter();
};

const handleExpandChange = (row: any, expandedRows: any[]) => {
  expandedRowKeys.value = expandedRows.map(r => r.id);
};

const openInspectionDialog = (row: any) => {
  currentOrder.value = row;
  inspectionDialogVisible.value = true;
};

const userFormatter = (row: any) => {
  return `${row.userDisplayName} (${row.userName})`;
};

const amountFormatter = (row: any) => {
  return `₩${formatPrice(getOrderTotalAmount(row))}`;
};

const innerProductInfoFormatter = (row: any) => {
  const setTag = row.productSetId ? '[SET] ' : '';
  let text = `${setTag}${row.productName || row.productSetTitle || ''}`;
  if (row.productNo) text += ` (${row.productNo})`;

  const options = [];
  if (row.purity) options.push(codeMap.value[row.purity] || row.purity);
  if (row.color) options.push(codeMap.value[row.color] || row.color);

  if (options.length > 0) {
    text += `\n옵션: ${options.join(', ')}`;
  }
  return text;
};

onMounted(() => {
  fetchOrderStatuses();
  getList();
});
</script>

<style lang="scss" scoped>
@import "./InspectionManagementStyles.scss";
</style>

