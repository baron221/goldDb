<template>
<div class="settlement-management-container app-container">

    <settlement-management-filter
      :query="listQuery"
      @filter="handleFilter"
      @reset="resetQuery"
    />

    <el-card shadow="never" style="margin-top: 1.25rem;">
      <base-table
        v-loading="listLoading"
        :data="list"
        border
        fit
        highlight-current-row
        style="width: 100%"
        row-key="id"
      >
        <el-table-column type="expand">
          <template #default="{row}">
            <div class="order-detail-expand">
              <h4>{{ $t('orderDetail.itemSpecifications') }}</h4>
              <base-table :data="row.orderItems" border style="width: 100%">
                <el-table-column prop="productName" :label="$t('orderDetail.headers.productInfo')" min-width="250" :excel-formatter="innerProductInfoFormatter">
                  <template #default="item">
                    <div class="product-info-cell">
                      <el-image :src="item.row.photoUrl || defaultImage" fit="cover" class="product-thumb" />
                      <div class="product-text">
                        <div v-if="item.row.productSetId" class="set-tag">SET</div>
                        <div class="product-name">{{ item.row.productName || item.row.productSetTitle }}</div>
                        <div class="product-no">{{ item.row.productNo }}</div>
                      </div>
                    </div>
                  </template>
                </el-table-column>
                <el-table-column :label="$t('orderDetail.headers.qty')" width="70" align="center" prop="quantity" />
                <el-table-column prop="weight" :label="$t('orderDetail.headers.orderWeight')" width="90" align="center" :excel-formatter="(row) => row.weight ? row.weight + 'g' : '-'">
                  <template #default="item">
                    <span>{{ item.row.weight ? item.row.weight + 'g' : '-' }}</span>
                  </template>
                </el-table-column>
                <el-table-column prop="confirmedWeight" :label="$t('orderDetail.headers.actualWeight')" width="110" align="center" :excel-formatter="(row) => row.confirmedWeight ? row.confirmedWeight + 'g' : '-'">
                  <template #default="item">
                    <span style="font-weight: bold; color: #67c23a;">{{ item.row.confirmedWeight ? item.row.confirmedWeight + 'g' : '-' }}</span>
                  </template>
                </el-table-column>
                <el-table-column prop="purity" :label="$t('sys.product.headers.purityColor')" width="120" align="center" :excel-formatter="purityColorFormatter">
                  <template #default="item">
                    <div style="margin-bottom: 0.125rem;">{{ codeMap[item.row.purity] || item.row.purity }}</div>
                    <div v-if="item.row.color" style="font-size: 0.8875rem; color: #666; margin-bottom: 0.125rem;">{{ codeMap[item.row.color] || item.row.color }}</div>
                    <el-tag v-if="item.row.isAsOrder" type="danger" size="small">AS</el-tag>
                  </template>
                </el-table-column>
                <el-table-column prop="inspectionMemo" :label="$t('orderDetail.memos.orderMemo')" min-width="200" :excel-formatter="memoFormatter">
                  <template #default="item">
                    <div style="font-size: 0.8875rem; color: #909399;">{{ $t('home.roles.factory.title') }}: {{ item.row.inspectionMemo || '-' }}</div>
                    <div style="font-size: 0.8875rem; color: #409EFF;">{{ $t('home.roles.logistics.title') }}: {{ item.row.logisticsMemo || '-' }}</div>
                  </template>
                </el-table-column>
              </base-table>
            </div>
          </template>
        </el-table-column>

        <el-table-column :label="$t('order.filters.orderNo')" prop="orderNo" width="200" align="center" />
        <el-table-column prop="userDisplayName" :label="$t('sys.company.labels.name')" width="180" align="center" :excel-formatter="userFormatter">
          <template #default="{row}">
            <span>{{ row.userDisplayName }} ({{ row.userName }})</span>
          </template>
        </el-table-column>
        <el-table-column prop="totalAmount" :label="$t('order.list.totalAmount')" width="140" align="right" :excel-formatter="amountFormatter">
          <template #default="{row}">
            <span style="font-size: 0.8875rem; color: #909399; margin-right: 0.1875rem; font-weight: normal;">₩</span><span style="font-weight: bold;">{{ formatPrice(getOrderTotalAmount(row)) }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="status" :label="$t('dashboard.factory.table.status')" width="130" align="center" :excel-formatter="(row) => statusLabel(row.status)">
          <template #default="{row}">
            <el-tag :type="getStatusTag(row.status)">{{ statusLabel(row.status) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="updatedAt" :label="$t('dashboard.factory.table.requestedAt')" width="160" align="center" :excel-formatter="(row) => formatDate(row.updatedAt || row.createdAt)">
          <template #default="{row}">
            <span>{{ formatDate(row.updatedAt || row.createdAt) }}</span>
          </template>
        </el-table-column>
        <el-table-column :label="$t('common.action')" min-width="250" align="center" :fixed="!isMobile ? 'right' : false">
          <template #default="{row}">
            <div class="action-buttons">
              <el-button
                v-if="row.status === 'PENDING'"
                type="primary"
                size="small"
                @click="handleStatusUpdate(row, 'PROCESSING')"
              >
                {{ $t('order.status.PROCESSING') }}
              </el-button>
              <el-button
                v-if="row.status === 'PROCESSING'"
                type="success"
                size="small"
                @click="openSettlementDialog(row)"
              >
                {{ $t('order.list.requestSettlement') }}
              </el-button>
              <el-button
                v-if="row.status === 'SETTLED'"
                type="info"
                size="small"
                disabled
              >
                {{ $t('order.status.SETTLED') }}
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

    <settlement-dialog
      v-model="settlementDialogVisible"
      :order="currentOrder"
      @saved="getList"
    />
  </div>
</template>

<script setup lang="ts">
import { useMobile } from '@/hooks/useMobile';
import { ref, reactive, computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { getAllOrders, updateOrderStatus } from '@/api/order';
import { ElMessage, ElMessageBox } from 'element-plus';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import BaseTable from '@/components/BaseTable/index.vue';
import { isPostPendingStatus } from '@/utils/order';
import useCodeStore from '@/store/modules/code';
import SettlementDialog from './components/SettlementDialog.vue';
import SettlementManagementFilter from './components/SettlementManagementFilter.vue';

const { isMobile } = useMobile();
const { t } = useI18n();

const listLoading = ref(true);
const list = ref<any[]>([]);
const total = ref(0);
const codeStore = useCodeStore();
const codeMap = computed(() => codeStore.codeMap);
const defaultImage = 'https://via.placeholder.com/100x100?text=No+Image';

const settlementDialogVisible = ref(false);
const currentOrder = ref<any>(null);

const listQuery = reactive({
  page: 1,
  pageSize: 20,
  status: 'PENDING', 
  orderNo: '',
  userName: '',
  companyId: undefined as number | undefined,
  categoryLarge: '',
  categoryMedium: '',
  categorySmall: '',
  setCategoryLarge: '',
  setCategoryMedium: '',
  setCategorySmall: ''
});

const formatDate = (dateStr: string) => {
  if (!dateStr) return '';
  return parseTime(new Date(dateStr), '{y}-{m}-{d} {h}:{i}');
};

const statusLabel = (status: string) => {
  const name = codeStore.getCodeName(status);
  if (name !== status) return name;
  return t(`order.status.${status}`);
};

const getStatusTag = (status: string) => {
  switch (status) {
    case 'Inspected': return 'success';
    case 'PENDING': return 'warning';
    case 'PROCESSING': return 'primary';
    case 'SETTLED': return 'success';
    default: return 'info';
  }
};

const getList = async () => {
  listLoading.value = true;
  try {
    const [res] = await Promise.all([
      getAllOrders(listQuery),
      codeStore.fetchCodes()
    ]);
    list.value = res.data.items;
    total.value = res.data.totalCount;
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
  listQuery.status = 'PENDING';
  listQuery.categoryLarge = '';
  listQuery.categoryMedium = '';
  listQuery.categorySmall = '';
  listQuery.setCategoryLarge = '';
  listQuery.setCategoryMedium = '';
  listQuery.setCategorySmall = '';
  handleFilter();
};

const openSettlementDialog = (row: any) => {
  currentOrder.value = row;
  settlementDialogVisible.value = true;
};

const handleStatusUpdate = (row: any, status: string) => {
  const label = status === 'PROCESSING' ? t('order.status.PROCESSING') : t('order.status.SETTLED');
  ElMessageBox.confirm(`${label} ${t('admin.deposit.messages.confirmTitle')}?`, t('common.ok'), {
    confirmButtonText: t('common.ok'),
    cancelButtonText: t('common.cancel'),
    type: 'warning'
  }).then(async () => {
    try {
      await updateOrderStatus(row.id, { status });
      ElMessage.success(`${label} ${t('admin.deposit.messages.success')}`);
      getList();
    } catch (error) {
      console.error('Failed to update status:', error);
    }
  });
};

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
  return text;
};

const purityColorFormatter = (row: any) => {
  const p = codeMap.value[row.purity] || row.purity || '';
  const c = codeMap.value[row.color] || row.color || '';
  const as = row.isAsOrder ? '\n(AS 의뢰)' : '';
  return `${p}${c ? ' / ' + c : ''}${as}`;
};

const memoFormatter = (row: any) => {
  return `공장: ${row.inspectionMemo || '-'}\n물류: ${row.logisticsMemo || '-'}`;
};

onMounted(() => {
  getList();
});
</script>

<style lang="scss" scoped>
@import "./SettlementManagementStyles.scss";
</style>

