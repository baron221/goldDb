<template>
<div class="order-management-container app-container">
    <order-search-filter
      :query="listQuery"
      @filter="handleFilter"
      @reset="resetQuery"
      @print-all="handlePrintAll"
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
              <h4>{{ $t('orderDetail.itemSpecifications') }}</h4>
              <base-table :data="row.orderItems" border style="width: 100%" :row-class-name="getInnerRowClass">
                <el-table-column type="expand">
                  <template #default="item">
                    <div v-if="item.row.children && item.row.children.length > 0" class="sub-items-container">
                      <div class="sub-items-title">{{ $t('orderDetail.componentItems') }}</div>
                      <base-table :data="item.row.children" style="width: 100%" :show-header="false" size="small">
                        <el-table-column width="48" />
                        <el-table-column prop="productName" :label="$t('orderDetail.headers.productInfo')" min-width="250" :excel-formatter="innerProductInfoFormatter">
                          <template #default="subItem">
                            <div class="product-info-cell sub-item">
                              <el-image :src="getThumbnailUrl(subItem.row.photoUrl) || defaultImage" fit="cover" class="product-thumb small" />
                              <div class="product-text">
                                <div class="product-name">{{ subItem.row.productName }}</div>
                                <div class="product-no">{{ subItem.row.productNo }}</div>
                              </div>
                            </div>
                          </template>
                        </el-table-column>
                        <el-table-column prop="price" width="150" align="right" :excel-formatter="priceFormatter">
                          <template #default="subItem">
                            <span v-if="subItem.row.price > 0">₩ {{ formatPrice(subItem.row.price) }}</span>
                          </template>
                        </el-table-column>
                        <el-table-column width="100" align="center" prop="quantity" />
                        <el-table-column prop="itemTotal" width="150" align="right" :excel-formatter="itemTotalFormatter">
                          <template #default="subItem">
                            <span v-if="subItem.row.price > 0" class="item-total">₩ {{ formatPrice(subItem.row.price * subItem.row.quantity) }}</span>
                          </template>
                        </el-table-column>
                      </base-table>
                    </div>
                  </template>
                </el-table-column>
                <el-table-column prop="productName" :label="$t('orderDetail.headers.productInfo')" min-width="300" :excel-formatter="innerProductInfoFormatter">
                  <template #default="item">
                    <div class="product-info-cell">
                      <el-image :src="item.row.photoUrl || defaultImage" fit="cover" class="product-thumb" />
                      <div class="product-text">
                        <div v-if="item.row.productSetId" class="set-tag">SET</div>
                        <div class="product-name">{{ item.row.productName || item.row.productSetTitle }}</div>
                        <div class="product-no">{{ item.row.productNo }}</div>
                        <div class="product-options" v-if="item.row.purity || item.row.color || item.row.size">
                          <el-tag size="small" type="info" effect="plain" v-if="item.row.purity">{{ codeMap[item.row.purity] || item.row.purity }}</el-tag>
                          <el-tag size="small" type="info" effect="plain" v-if="item.row.color" style="margin-left: 0.3125rem;">{{ codeMap[item.row.color] || item.row.color }}</el-tag>
                          <el-tag size="small" type="info" effect="plain" v-if="item.row.size && item.row.size !== 'EMPTY'" style="margin-left: 0.3125rem;">{{ codeMap[item.row.size] || item.row.size }}</el-tag>
                        </div>
                        <div v-if="item.row.memo" style="font-size: 0.8rem; color: #8a6d3b; margin-top: 0.125rem;">📝 {{ item.row.memo }}</div>
                      </div>
                    </div>
                  </template>
                </el-table-column>
                <el-table-column prop="price" :label="$t('orderDetail.headers.price')" width="150" align="right" :excel-formatter="priceFormatter">
                  <template #default="item">
                    <span>₩ {{ formatPrice(item.row.price) }}</span>
                  </template>
                </el-table-column>
                <el-table-column :label="$t('orderDetail.headers.qty')" width="100" align="center" prop="quantity" />
                <el-table-column prop="itemTotal" :label="$t('orderDetail.settlement.total')" width="150" align="right" :excel-formatter="itemTotalFormatter">
                  <template #default="item">
                    <span class="item-total">₩ {{ formatPrice(item.row.price * item.row.quantity) }}</span>
                  </template>
                </el-table-column>
              </base-table>
            </div>
          </template>
        </el-table-column>

        <el-table-column :label="$t('order.filters.orderNo')" prop="orderNo" width="200" align="center" />
        <el-table-column :label="$t('sys.company.labels.name')" prop="companyName" width="150" align="center" />
        <el-table-column prop="userDisplayName" :label="$t('order.filters.searchTitle')" width="150" align="center" :excel-formatter="userFormatter">
          <template #default="{row}">
            <span>{{ row.userDisplayName }} ({{ row.userName }})</span>
          </template>
        </el-table-column>
        <el-table-column prop="logisticsCompanyName" :label="$t('admin.orderApproval.logisticsCompany')" width="150" align="center" :excel-formatter="(row) => row.logisticsCompanyName || '-'">
          <template #default="{row}">
            <span>{{ row.logisticsCompanyName || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="manufacturerName" :label="$t('stock.labels.manufacturer')" width="150" align="center" :excel-formatter="(row) => row.manufacturerName || '-'">
          <template #default="{row}">
            <el-tag v-if="row.manufacturerName" type="warning" size="small" effect="plain">{{ row.manufacturerName }}</el-tag>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column prop="totalAmount" :label="$t('order.list.totalAmount')" width="150" align="right" :excel-formatter="amountFormatter">
          <template #default="{row}">
            <span style="font-size: 0.8875rem; color: #909399; margin-right: 0.1875rem; font-weight: normal;">₩</span><span style="font-weight: bold;">{{ formatPrice(getOrderTotalAmount(row)) }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" :label="$t('dashboard.factory.table.requestedAt')" width="180" align="center" :excel-formatter="(row) => formatDate(row.createdAt)">
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
            <div class="action-buttons-container">
              <el-button
                v-if="row.status === 'ORDERED'"
                type="primary"
                size="small"
                @click="handleStatusUpdate(row, 'LogisticsApproved')"
              >
                {{ $t('order.status.LogisticsApproved') }}
              </el-button>
              <el-button
                v-else-if="row.status === 'LogisticsApproved'"
                type="warning"
                size="small"
                @click="handleStatusUpdate(row, 'FactoryRequested')"
              >
                {{ $t('order.status.FactoryRequested') }}
              </el-button>
              <el-button
                v-else-if="row.status === 'InspectedRequested'"
                type="success"
                size="small"
                @click="handleStatusUpdate(row, 'Inspected')"
              >
                {{ $t('order.status.Inspected') }}
              </el-button>
              <el-button
                v-else-if="row.status === 'Inspected'"
                type="info"
                size="small"
                @click="handleStatusUpdate(row, 'Completed')"
              >
                {{ $t('order.status.Completed') }}
              </el-button>

              <el-dropdown trigger="click" class="more-actions-dropdown">
                <el-button size="small" :icon="MoreFilled" circle />
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item
                      v-if="row.status === 'ORDERED'"
                      style="color: #F56C6C;"
                      @click="handleStatusUpdate(row, 'Cancelled')"
                    >
                      {{ $t('order.list.cancelOrder') }}
                    </el-dropdown-item>
                    <el-dropdown-item
                      v-if="row.status === 'Cancelled' || row.status === 'SETTLED_CANCELLED'"
                      style="color: #F56C6C;"
                      @click="handleDeleteOrder(row)"
                    >
                      {{ $t('common.delete') }}
                    </el-dropdown-item>
                    <el-dropdown-item :icon="Clock" @click="openHistoryDialog(row)">
                      {{ $t('orderDetail.memos.showHistory') }}
                    </el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
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

    <OrderApprovalDialog
      v-model="approvalDialogVisible"
      :order="currentOrder"
      @saved="getList"
    />

    <OrderFactoryRequestDialog
      v-model="factoryDialogVisible"
      :order="currentOrder"
      @saved="getList"
    />

    <order-status-history-dialog
      v-model="historyDialogVisible"
      :order-id="selectedOrderId"
    />
  </div>
</template>

<script setup lang="ts">
import { useMobile } from '@/hooks/useMobile';
import { ref, reactive, onMounted, nextTick, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { getAllOrders, updateOrderStatus, deleteOrder } from '@/api/order';
import { ElMessage, ElMessageBox } from 'element-plus';
import { Clock, MoreFilled } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import { getOrderTotalAmount } from '@/utils/order';
import BaseTable from '@/components/BaseTable/index.vue';
import OrderStatusHistoryDialog from '@/components/OrderStatusHistoryDialog/index.vue';
import OrderApprovalDialog from './components/OrderApprovalDialog.vue';
import OrderFactoryRequestDialog from './components/OrderFactoryRequestDialog.vue';
import OrderSearchFilter from './components/OrderSearchFilter.vue';
import useCodeStore from '@/store/modules/code';
import useUserStore from '@/store/modules/user';
import { useOrderFormatters } from './composables/useOrderFormatters';
import { getThumbnailUrl } from '@/utils';

const { isMobile } = useMobile();
const { t } = useI18n();

const listLoading = ref(true);
const list = ref<any[]>([]);
const total = ref(0);
const codeStore = useCodeStore();
const codeMap = computed(() => codeStore.codeMap);
const defaultImage = 'https://via.placeholder.com/100x100?text=No+Image';

const userStore = useUserStore();
const isAdmin = computed(() => userStore.roles.includes('admin'));

const historyDialogVisible = ref(false);
const selectedOrderId = ref<number | null>(null);
const openHistoryDialog = (row: any) => {
  selectedOrderId.value = row.id;
  historyDialogVisible.value = true;
};

const factoryDialogVisible = ref(false);
const approvalDialogVisible = ref(false);
const currentOrder = ref<any>(null);

const end = new Date();
const start = new Date();
start.setTime(start.getTime() - 3600 * 1000 * 24 * 30);
const defaultStartDate = parseTime(start, '{y}-{m}-{d}');
const defaultEndDate = parseTime(end, '{y}-{m}-{d}');

const multipleTable = ref<any>(null);
const expandedRowKeys = ref<number[]>([]);
const scrollPosition = ref(0);

const listQuery = reactive({
  page: 1, pageSize: 20, status: '', orderNo: '', userName: '',
  startDate: defaultStartDate, endDate: defaultEndDate,
  categoryLarge: '', categoryMedium: '', categorySmall: '',
  setCategoryLarge: '', setCategoryMedium: '', setCategorySmall: ''
});

const statusLabel = (status: string) => codeStore.getCodeName(status);
const getStatusType = (status: string) => {
  const types: Record<string, string> = {
    'ORDERED': 'info', 'FactoryRequested': 'warning', 'LogisticsApproved': '',
    'Inspected': 'success', 'Completed': 'info', 'Cancelled': 'danger'
  };
  return types[status] || 'info';
};

const getList = async () => {
  const container = document.querySelector('.app-main');
  if (container) scrollPosition.value = (container as HTMLElement).scrollTop;
  listLoading.value = true;
  try {
    const [res] = await Promise.all([getAllOrders(listQuery), codeStore.fetchCodes()]);
    list.value = res.data.items;
    total.value = res.data.totalCount;
    expandedRowKeys.value = list.value.map((r: any) => r.id);
    await nextTick();
    if (container) (container as HTMLElement).scrollTop = scrollPosition.value;
  } catch (error) {
    console.error('Failed to get orders:', error);
  } finally {
    listLoading.value = false;
  }
};

const handleExpandChange = (row: any, expandedRows: any[]) => {
  expandedRowKeys.value = expandedRows.map(r => r.id);
};

const getInnerRowClass = ({ row }: { row: any }) => {
  return (!row.children || row.children.length === 0) ? 'hide-expand' : '';
};

const handleFilter = () => { listQuery.page = 1; getList(); };

const resetQuery = () => {
  listQuery.status = ''; listQuery.orderNo = ''; listQuery.userName = '';
  listQuery.startDate = defaultStartDate; listQuery.endDate = defaultEndDate;
  listQuery.categoryLarge = ''; listQuery.categoryMedium = ''; listQuery.categorySmall = '';
  listQuery.setCategoryLarge = ''; listQuery.setCategoryMedium = ''; listQuery.setCategorySmall = '';
  handleFilter();
};

const openApprovalDialog = (row: any) => { currentOrder.value = row; approvalDialogVisible.value = true; };
const openFactoryDialog = (row: any) => { currentOrder.value = row; factoryDialogVisible.value = true; };

const handleStatusUpdate = (row: any, status: string) => {
  if (status === 'LogisticsApproved') { openApprovalDialog(row); return; }
  if (status === 'FactoryRequested') { openFactoryDialog(row); return; }
  ElMessageBox.confirm(t('cart.messages.confirmOrder', { count: 1 }), t('cart.messages.confirmOrderTitle'), {
    confirmButtonText: t('common.ok'), cancelButtonText: t('common.cancel'), type: 'warning'
  }).then(async () => {
    try {
      await updateOrderStatus(row.id, { status });
      ElMessage.success(t('cart.messages.orderSuccess'));
      getList();
    } catch (error) { console.error('Failed to update status:', error); }
  });
};

const handleDeleteOrder = (row: any) => {
  ElMessageBox.confirm(t('favorite.messages.confirmRemove'), t('favorite.messages.confirmTitle'), {
    confirmButtonText: t('common.delete'), cancelButtonText: t('common.cancel'), type: 'error'
  }).then(async () => {
    try {
      await deleteOrder(row.id);
      ElMessage.success(t('favorite.messages.removeSuccess'));
      getList();
    } catch (error) {
      console.error('Failed to delete order:', error);
      ElMessage.error(t('favorite.messages.removeError'));
    }
  });
};

const handlePrintAll = () => ElMessage.warning('인쇄 기능이 준비 중입니다.');

const { formatDate, userFormatter, amountFormatter, innerProductInfoFormatter, priceFormatter, itemTotalFormatter } = useOrderFormatters(codeMap);

onMounted(() => { codeStore.fetchCodes(); getList(); });
</script>

<style lang="scss" scoped src="./components/OrderManagementStyles.scss"></style>

