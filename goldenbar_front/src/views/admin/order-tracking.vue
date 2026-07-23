<template>
  <div class="app-container order-tracking-container">
    <el-row :gutter="20">

      <el-col :span="14">
        <el-card shadow="never" class="list-card">
          <template #header>
            <div class="card-header">
              <span style="font-weight: bold; font-size: 1rem;">주문 검색</span>
            </div>
          </template>

          <el-form :inline="true" :model="listQuery" class="filter-form">
            <el-form-item label="주문기간">
              <el-date-picker
                v-model="dateRange"
                type="daterange"
                range-separator="-"
                start-placeholder="시작일"
                end-placeholder="종료일"
                value-format="YYYY-MM-DD"
                @change="handleDateChange"
                style="width: 250px"
              />
            </el-form-item>
            <el-form-item label="주문번호">
              <el-input v-model="listQuery.orderNo" placeholder="주문번호 입력" clearable @keyup.enter="handleFilter" style="width: 150px" />
            </el-form-item>

            <el-form-item label="일반분류">
              <common-select
                v-model="listQuery.categoryLarge"
                group-code="PRODUCT_CATEGORY"
                placeholder="대분류"
                style="width: 120px; margin-right: 0.3125rem;"
                @change="(val, options) => handleLargeChange(val, options)"
              />
              <common-select
                v-model="listQuery.categoryMedium"
                :parent-id="largeId"
                placeholder="중분류"
                style="width: 120px; margin-right: 0.3125rem;"
                @change="(val, options) => handleMediumChange(val, options)"
              />
              <common-select
                v-model="listQuery.categorySmall"
                :parent-id="mediumId"
                placeholder="소분류"
                style="width: 120px;"
                @change="handleFilter"
              />
            </el-form-item>

            <el-form-item label="세트분류">
              <common-select
                v-model="listQuery.setCategoryLarge"
                group-code="PRODUCT_CATEGORY"
                placeholder="대분류"
                style="width: 120px; margin-right: 0.3125rem;"
                @change="(val, options) => handleSetLargeChange(val, options)"
              />
              <common-select
                v-model="listQuery.setCategoryMedium"
                :parent-id="setLargeId"
                placeholder="중분류"
                style="width: 120px; margin-right: 0.3125rem;"
                @change="(val, options) => handleSetMediumChange(val, options)"
              />
              <common-select
                v-model="listQuery.setCategorySmall"
                :parent-id="setMediumId"
                placeholder="소분류"
                style="width: 120px;"
                @change="handleFilter"
              />
            </el-form-item>

            <el-form-item>
              <el-button type="primary" :icon="Search" @click="handleFilter">검색</el-button>
              <el-button :icon="Refresh" @click="resetQuery">초기화</el-button>
            </el-form-item>
          </el-form>

          <base-table
            v-loading="listLoading"
            :data="list"
            :total="total"
            v-model:page="listQuery.page"
            v-model:page-size="listQuery.pageSize"
            style="width: 100%; cursor: pointer;"
            @row-click="handleRowClick"
            @change="getList"
          >
            <el-table-column label="주문일시" width="160" align="center" :excel-formatter="(row) => formatDate(row.createdAt)">
              <template #default="{row}">
                <span>{{ formatDate(row.createdAt) }}</span>
              </template>
            </el-table-column>
            <el-table-column label="주문번호" prop="orderNo" min-width="150" align="center" />
            <el-table-column label="상품 정보" min-width="230" :excel-formatter="(row) => itemsSummaryText(row)">
              <template #default="{row}">
                <div v-if="row.orderItems && row.orderItems.length" class="order-item-cell">
                  <el-image :src="row.orderItems[0].photoUrl" fit="cover" class="order-item-thumb">
                    <template #error>
                      <div class="order-item-thumb-fallback">
                        <el-icon><Picture /></el-icon>
                      </div>
                    </template>
                  </el-image>
                  <div class="order-item-text">
                    <div class="order-item-name">
                      {{ firstItemName(row) }}
                      <el-tag v-if="row.orderItems.length > 1" size="small" type="info" effect="plain" class="more-tag">
                        +{{ row.orderItems.length - 1 }}
                      </el-tag>
                    </div>
                    <div class="order-item-no">{{ row.orderItems[0].productNo || '-' }}</div>
                    <div v-if="row.orderItems[0].size && row.orderItems[0].size !== 'EMPTY'" class="order-item-size">{{ $t('productDetail.labels.productSize') }}: {{ codeMap[row.orderItems[0].size] || row.orderItems[0].size }}</div>
                    <el-tooltip v-if="row.orderItems[0].memo" :content="row.orderItems[0].memo" placement="top">
                      <div class="order-item-memo">📝 {{ row.orderItems[0].memo }}</div>
                    </el-tooltip>
                    <div v-if="showManufacturerInfo && row.manufacturerName" class="order-item-party">주문정보(생산): {{ row.manufacturerName }}</div>
                    <div v-if="showMarketInfo && row.companyName" class="order-item-party">주문정보(소매점): {{ row.companyName }}</div>
                  </div>
                </div>
                <span v-else>-</span>
              </template>
            </el-table-column>
            <el-table-column label="주문자" width="120" align="center" :excel-formatter="(row) => row.userDisplayName || row.userName">
              <template #default="{row}">
                <span>{{ row.userDisplayName || row.userName }}</span>
              </template>
            </el-table-column>
            <el-table-column v-if="showLogisticsInfo" label="물류업체" width="120" align="center" :excel-formatter="(row) => row.logisticsCompanyName || '-'">
              <template #default="{row}">
                <span>{{ row.logisticsCompanyName || '-' }}</span>
              </template>
            </el-table-column>
            <el-table-column label="담당자" width="100" align="center" :excel-formatter="(row) => row.handledByUserName || '-'">
              <template #default="{row}">
                <span>{{ row.handledByUserName || '-' }}</span>
              </template>
            </el-table-column>
            <el-table-column label="현재 상태" width="120" align="center" :excel-formatter="(row) => statusLabel(row.status)">
              <template #default="{row}">
                <el-tag :type="getStatusType(row.status)">{{ statusLabel(row.status) }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column label="조회" width="80" align="center">
              <template #default>
                <el-button type="info" size="small" :icon="ArrowRight" circle />
              </template>
            </el-table-column>
          </base-table>
        </el-card>
      </el-col>

      <el-col :span="10">
        <el-card shadow="never" class="timeline-card">
          <template #header>
            <div class="card-header">
              <span style="font-weight: bold; font-size: 1rem;">주문 진행 이력</span>
              <span v-if="selectedOrder" style="margin-left: 0.625rem; color: #409eff; font-size: 0.875rem;">
                ({{ selectedOrder.orderNo }})
              </span>
            </div>
          </template>

          <div v-if="!selectedOrder" class="empty-state">
            <el-empty description="왼쪽 목록에서 주문을 선택해주세요." />
          </div>
          <div v-else v-loading="historyLoading" class="timeline-wrapper">
            <div v-if="selectedOrder.handledByUserName" class="handler-badge">
              <span class="handler-label">담당자</span>
              <span class="handler-name">{{ selectedOrder.handledByUserName }}</span>
            </div>
            <el-timeline>
              <el-timeline-item
                v-for="(activity, index) in orderHistory"
                :key="index"
                :timestamp="formatDate(activity.createdAt)"
                :type="index === 0 ? 'primary' : 'info'"
                placement="top"
              >
                <el-card shadow="hover">
                  <h4 style="margin-top: 0; margin-bottom: 0.5rem; color: #303133;">{{ statusLabel(activity.status) }}</h4>
                  <div class="timeline-details">
                    <p>
                      <strong>처리자:</strong>
                      <span v-if="activity.userDisplayName">{{ activity.userDisplayName }}</span>
                      <span v-else>시스템</span>
                    </p>
                    <p v-if="activity.remarks" class="remarks">
                      <strong>메모:</strong> {{ activity.remarks }}
                    </p>
                  </div>
                </el-card>
              </el-timeline-item>
            </el-timeline>
            <el-empty v-if="orderHistory.length === 0 && !historyLoading" description="이력 정보가 없습니다." />
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue';
import { getAllOrders, getOrderHistory } from '@/api/order';
import { ElMessage } from 'element-plus';
import { Search, Refresh, ArrowRight, Picture } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import CommonSelect from '@/components/CommonSelect/index.vue';
import BaseTable from '@/components/BaseTable/index.vue';
import useCodeStore from '@/store/modules/code';
import useUserStore from '@/store/modules/user';

const listLoading = ref(false);
const list = ref<any[]>([]);
const total = ref(0);
const codeStore = useCodeStore();
const userStore = useUserStore();

const isAdmin = computed(() => userStore.roles.includes('admin'));
const showManufacturerInfo = computed(() => isAdmin.value || userStore.companyType === 'DCC');
const showMarketInfo = computed(() => isAdmin.value || userStore.companyType === 'DCC');
const showLogisticsInfo = computed(() => isAdmin.value || userStore.companyType === 'MFG' || userStore.companyType === 'RTL');

const end = new Date();
const start = new Date();
start.setTime(start.getTime() - 3600 * 1000 * 24 * 30);
const defaultStartDate = parseTime(start, '{y}-{m}-{d}');
const defaultEndDate = parseTime(end, '{y}-{m}-{d}');

const dateRange = ref<string[]>([defaultStartDate, defaultEndDate]);
const orderStatusCodes = ref<any[]>([]);

const listQuery = reactive({
  page: 1, pageSize: 20, status: '', orderNo: '',
  startDate: defaultStartDate, endDate: defaultEndDate,
  categoryLarge: '', categoryMedium: '', categorySmall: '',
  setCategoryLarge: '', setCategoryMedium: '', setCategorySmall: ''
});

const largeId = ref<number | null>(null);
const mediumId = ref<number | null>(null);
const setLargeId = ref<number | null>(null);
const setMediumId = ref<number | null>(null);

const handleLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  largeId.value = selected ? selected.id : null;
  listQuery.categoryMedium = '';
  listQuery.categorySmall = '';
  mediumId.value = null;
  handleFilter();
};

const handleMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  mediumId.value = selected ? selected.id : null;
  listQuery.categorySmall = '';
  handleFilter();
};

const handleSetLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  setLargeId.value = selected ? selected.id : null;
  listQuery.setCategoryMedium = '';
  listQuery.setCategorySmall = '';
  setMediumId.value = null;
  handleFilter();
};

const handleSetMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  setMediumId.value = selected ? selected.id : null;
  listQuery.setCategorySmall = '';
  handleFilter();
};

const selectedOrder = ref<any>(null);
const historyLoading = ref(false);
const orderHistory = ref<any[]>([]);

const codeMap = computed(() => codeStore.codeMap);

const statusLabel = (status: string) => codeStore.getCodeName(status);

const getStatusType = (status: string) => {
  const types: Record<string, string> = {
    'ORDERED': 'info', 'FactoryRequested': 'warning', 'LogisticsApproved': '',
    'Inspected': 'success', 'Completed': 'info', 'Cancelled': 'danger'
  };
  return types[status] || 'info';
};

const formatDate = (date: string) => parseTime(date, '{y}-{m}-{d} {h}:{i}');

const firstItemName = (row: any) => {
  const item = row.orderItems?.[0];
  if (!item) return '-';
  return item.productName || item.productSetTitle || '-';
};

const itemsSummaryText = (row: any) => {
  const items = row.orderItems || [];
  if (!items.length) return '-';
  const name = firstItemName(row);
  const no = items[0].productNo ? ` (${items[0].productNo})` : '';
  const more = items.length > 1 ? ` 외 ${items.length - 1}건` : '';
  return `${name}${no}${more}`;
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
  listLoading.value = true;
  try {
    const res = await getAllOrders(listQuery);
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
  selectedOrder.value = null;
  orderHistory.value = [];
};

const handleDateChange = (val: string[] | null) => {
  if (val) { listQuery.startDate = val[0]; listQuery.endDate = val[1]; } else { listQuery.startDate = undefined; listQuery.endDate = undefined; }
  handleFilter();
};

const resetQuery = () => {
  listQuery.status = ''; listQuery.orderNo = '';
  listQuery.startDate = defaultStartDate; listQuery.endDate = defaultEndDate;
  dateRange.value = [defaultStartDate, defaultEndDate];
  handleFilter();
};

const handleRowClick = async (row: any) => {
  selectedOrder.value = row;
  historyLoading.value = true;
  orderHistory.value = [];
  try {
    const res = await getOrderHistory(row.id);
    orderHistory.value = res.data || [];
  } catch (error) {
    console.error('Failed to fetch order history:', error);
    ElMessage.error('이력 정보를 불러오는데 실패했습니다.');
  } finally {
    historyLoading.value = false;
  }
};

onMounted(() => { fetchOrderStatuses(); getList(); });
</script>

<style lang="scss" src="./OrderTrackingStyles.scss" scoped></style>
