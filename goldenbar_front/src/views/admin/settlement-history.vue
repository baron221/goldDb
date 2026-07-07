<template>
<div class="settlement-history-container app-container">

    <el-row :gutter="20" class="summary-cards" v-loading="summaryLoading">
      <el-col :xs="24" :sm="8" class="mb-4 sm:mb-0">
        <el-card shadow="hover" class="summary-card">
          <template #header><div class="card-header"><span>{{ $t('admin.settlement.summary.totalActualWeight') }}</span></div></template>
          <div class="card-value">{{ summaryData.totalActualWeight.toFixed(2) }}g</div>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="8" class="mb-4 sm:mb-0">
        <el-card shadow="hover" class="summary-card gold">
          <template #header><div class="card-header"><span>{{ $t('admin.settlement.summary.totalFineGold') }}</span></div></template>
          <div class="card-value">{{ summaryData.totalFineGold.toFixed(2) }}g</div>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="8">
        <el-card shadow="hover" class="summary-card settle">
          <template #header><div class="card-header"><span>{{ $t('admin.settlement.summary.totalAmount') }}</span></div></template>
          <div class="card-value">₩ {{ formatPrice(summaryData.totalSettlementAmount) }}</div>
        </el-card>
      </el-col>
    </el-row>

    <settlement-history-filter
      :query="listQuery"
      :is-mobile="isMobile"
      @filter="handleFilter"
      @reset="resetQuery"
      style="margin-top: 1.25rem;"
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
              <h4>{{ $t('admin.settlement.table.expandTitle') }}</h4>
              <base-table :data="flattenOrderItems(row.orderItems)" border size="small" style="width: 100%">
                <el-table-column :label="$t('orderDetail.headers.productInfo')" min-width="250" prop="productName" :excel-formatter="productItemInfoFormatter">
                  <template #default="item">
                    <div class="product-info-cell" :style="{ paddingLeft: item.row.depth * 20 + 'px' }">
                      <el-image :src="item.row.photoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 35px; height: 35px;" />
                      <div class="product-text">
                        <div class="product-name">{{ item.row.productName || item.row.productSetTitle }}</div>
                        <div class="product-no">{{ item.row.productNo }}</div>
                        <div class="product-options" v-if="item.row.purity || item.row.color">
                          <el-tag size="small" type="info" effect="plain" v-if="item.row.purity">{{ codeMap[item.row.purity] || item.row.purity }}</el-tag>
                          <el-tag size="small" type="info" effect="plain" v-if="item.row.color && item.row.color !== 'EMPTY'" style="margin-left: 0.3125rem;">{{ codeMap[item.row.color] || item.row.color }}</el-tag>
                        </div>
                      </div>
                    </div>
                  </template>
                </el-table-column>
                <el-table-column :label="$t('orderDetail.headers.actualWeight')" width="90" align="center" prop="actualWeight" :excel-formatter="(row) => row.actualWeight ? row.actualWeight + 'g' : '-'">
                  <template #default="item">
                    <span>{{ item.row.actualWeight ? item.row.actualWeight + 'g' : '-' }}</span>
                  </template>
                </el-table-column>
                <el-table-column :label="$t('productDetail.labels.purity')" width="80" align="center" prop="purity" />
                <el-table-column :label="$t('admin.settlement.table.fineGold')" width="100" align="center" prop="fineGold" :excel-formatter="(row) => calculatePurityWeight(row.actualWeight, row.purity) + 'g'">
                  <template #default="item">
                    <span>{{ calculatePurityWeight(item.row.actualWeight, item.row.purity) }}g</span>
                  </template>
                </el-table-column>
                <el-table-column :label="$t('admin.settlement.table.ratio')" width="80" align="center" prop="settlementRatio" :excel-formatter="(row) => row.settlementRatio + '%'">
                  <template #default="item">
                    <span>{{ item.row.settlementRatio }}%</span>
                  </template>
                </el-table-column>
                <el-table-column :label="$t('admin.settlement.table.amount')" width="120" align="right" prop="settlementAmount" :excel-formatter="(row) => '₩ ' + formatPrice(row.settlementAmount)">
                  <template #default="item">
                    <span style="font-weight: bold; color: #f56c6c;">₩ {{ formatPrice(item.row.settlementAmount) }}</span>
                  </template>
                </el-table-column>
                <el-table-column :label="$t('admin.settlement.table.memo')" min-width="150" prop="settlementMemo" />
              </base-table>
            </div>
          </template>
        </el-table-column>

        <el-table-column :label="$t('admin.settlement.table.settledDate')" width="160" align="center" prop="updatedAt" :excel-formatter="(row) => formatDate(row.updatedAt || row.createdAt)">
          <template #default="{row}">
            <span>{{ formatDate(row.updatedAt || row.createdAt) }}</span>
          </template>
        </el-table-column>
        <el-table-column :label="$t('order.filters.orderNo')" prop="orderNo" width="200" align="center" />
        <el-table-column :label="$t('admin.settlement.filters.company')" width="180" align="center" prop="userDisplayName" :excel-formatter="(row) => `${row.userDisplayName} (${row.userName})`">
          <template #default="{row}">
            <span>{{ row.userDisplayName }} ({{ row.userName }})</span>
          </template>
        </el-table-column>
        <el-table-column :label="$t('admin.settlement.summary.totalAmount')" width="150" align="right" prop="totalSettlementAmount" :excel-formatter="(row) => '₩ ' + formatPrice(calculateOrderTotalSettlement(row))">
          <template #default="{row}">
            <span style="font-weight: bold; color: #f56c6c;">₩ {{ formatPrice(calculateOrderTotalSettlement(row)) }}</span>
          </template>
        </el-table-column>
        <el-table-column :label="$t('admin.settlement.table.receivableLink')" width="150" align="center">
          <template #default="{row}">
            <el-button link size="small" @click="goToReceivable(row.userId)">{{ $t('admin.settlement.table.receivableCheck') }}</el-button>
          </template>
        </el-table-column>
        <el-table-column :label="$t('common.action')" width="180" align="center" :fixed="!isMobile ? 'right' : false">
          <template #default="{row}">
            <el-button type="primary" size="small" :icon="Printer" @click="printStatement(row)">
              {{ $t('common.action') === '작업' ? '거래명세서' : $t('common.action') }}
            </el-button>
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

    <div id="print-area" v-if="printData" style="display: none;">
      <div class="statement-print">
        <h2 style="text-align: center;">{{ $t('admin.settlement.statement.title') }}</h2>
        <div style="display: flex; justify-content: space-between; margin-bottom: 1.25rem;">
          <div>
            <p><strong>{{ $t('order.filters.orderNo') }}:</strong> {{ printData.orderNo }}</p>
            <p><strong>{{ $t('admin.settlement.filters.company') }}:</strong> {{ printData.userDisplayName }} ({{ printData.userName }})</p>
            <p><strong>{{ $t('orderDetail.headers.requestedWeight') }}:</strong> {{ formatDate(printData.createdAt) }}</p>
          </div>
          <div style="text-align: right;">
            <p><strong>{{ $t('admin.settlement.statement.supplier') }}:</strong> {{ $t('admin.settlement.statement.companyName') }}</p>
            <p>(인)</p>
          </div>
        </div>
        <table border="1" style="width: 100%; border-collapse: collapse;">
          <thead>
            <tr>
              <th>{{ $t('productDetail.gemstoneInfo.gemstoneName') }}</th>
              <th>{{ $t('orderDetail.headers.qty') }}</th>
              <th>{{ $t('orderDetail.headers.actualWeight') }}</th>
              <th>{{ $t('productDetail.labels.purity') }}</th>
              <th>{{ $t('orderDetail.settlement.labor') }}</th>
              <th>{{ $t('admin.settlement.table.amount') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in flattenOrderItems(printData.orderItems)" :key="item.id">
              <td>{{ item.productName || item.productSetTitle }}</td>
              <td align="center">{{ item.quantity }}</td>
              <td align="center">{{ item.actualWeight }}g</td>
              <td align="center">{{ item.purity }}</td>
              <td align="right">₩ {{ formatPrice(item.settlementAmount) }}</td>
            </tr>
          </tbody>
          <tfoot>
            <tr>
              <td colspan="4" align="right"><strong>{{ $t('orderDetail.settlement.total') }} {{ $t('orderDetail.headers.price') }}</strong></td>
              <td align="right"><strong>₩ {{ formatPrice(calculateOrderTotalSettlement(printData)) }}</strong></td>
            </tr>
          </tfoot>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useMobile } from '@/hooks/useMobile';
import { ref, reactive, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { getAllOrders, getSettlementSummary } from '@/api/order';
import { Printer } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import useCodeStore from '@/store/modules/code';
import BaseTable from '@/components/BaseTable/index.vue';
import SettlementHistoryFilter from './components/SettlementHistoryFilter.vue';

const { isMobile } = useMobile();
const router = useRouter();

const listLoading = ref(true);
const summaryLoading = ref(true);
const list = ref<any[]>([]);
const total = ref(0);
const codeStore = useCodeStore();
const codeMap = computed(() => codeStore.codeMap);
const defaultImage = 'https://via.placeholder.com/100x100?text=No+Image';

const summaryData = reactive({
  totalActualWeight: 0,
  totalFineGold: 0,
  totalSettlementAmount: 0
});

const end = new Date();
const start = new Date();
start.setTime(start.getTime() - 3600 * 1000 * 24 * 30);
const defaultStartDate = parseTime(start, '{y}-{m}-{d}');
const defaultEndDate = parseTime(end, '{y}-{m}-{d}');

const listQuery = reactive({
  page: 1,
  pageSize: 20,
  status: 'SETTLED',
  orderNo: '',
  userName: '',
  companyId: undefined as number | undefined,
  startDate: defaultStartDate,
  endDate: defaultEndDate,
  categoryLarge: '',
  categoryMedium: '',
  categorySmall: '',
  setCategoryLarge: '',
  setCategoryMedium: '',
  setCategorySmall: ''
});

const printData = ref<any>(null);

const formatDate = (dateStr: string) => {
  if (!dateStr) return '';
  return parseTime(new Date(dateStr), '{y}-{m}-{d} {h}:{i}');
};

const calculatePurityWeight = (weight: number, purity: string) => {
  if (!weight) return '0.00';
  let ratio = 0;
  switch (purity) {
    case '14K': ratio = 0.585; break;
    case '18K': ratio = 0.75; break;
    case '24K': ratio = 1.0; break;
    case 'PT': ratio = 0.95; break;
    default: ratio = 0;
  }
  return (weight * ratio).toFixed(2);
};

const flattenOrderItems = (orderItems: any[]) => {
  const result: any[] = [];
  orderItems.forEach(item => {
    result.push({ ...item, depth: 0 });
    if (item.children && item.children.length > 0) {
      item.children.forEach((child: any) => {
        result.push({ ...child, depth: 1 });
      });
    }
  });
  return result;
};

const calculateOrderTotalSettlement = (order: any) => {
  let total = 0;
  order.orderItems.forEach((item: any) => {
    total += (item.settlementAmount || 0);
    if (item.children) {
      item.children.forEach((c: any) => {
        total += (c.settlementAmount || 0);
      });
    }
  });
  return total;
};

const fetchSummary = async () => {
  summaryLoading.value = true;
  try {
    const res = await getSettlementSummary(listQuery);
    Object.assign(summaryData, res.data);
  } catch (error) {
    console.error('Failed to fetch summary:', error);
  } finally {
    summaryLoading.value = false;
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
    fetchSummary();
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

const goToReceivable = (userId: number) => {
  router.push({
    path: '/admin/receivable',
    query: { userId }
  });
};

const printStatement = (row: any) => {
  printData.value = row;
  setTimeout(() => {
    const printContents = document.getElementById('print-area')?.innerHTML;
    if (!printContents) return;

    const printWindow = window.open('', '_blank');
    if (printWindow) {
      printWindow.document.write('<html><head><title>거래명세서 출력</title>');
      printWindow.document.write('<style>table { border-collapse: collapse; width: 100%; } th, td { border: 1px solid black; padding: 0.5rem; font-size: 0.95rem; } th { background-color: #f2f2f2; }</style>');
      printWindow.document.write('</head><body>');
      printWindow.document.write(printContents);
      printWindow.document.write('</body></html>');
      printWindow.document.close();
      printWindow.print();
    }
  }, 100);
};

const productItemInfoFormatter = (row: any) => {
  const purity = row.purity ? `[${codeMap.value[row.purity] || row.purity}]` : '';
  const color = row.color ? `[${codeMap.value[row.color] || row.color}]` : '';
  return `${row.productName || row.productSetTitle}\n${row.productNo}\n${purity} ${color}`;
};

onMounted(() => {
  getList();
});
</script>

<style lang="scss" scoped>
@import "./SettlementHistoryStyles.scss";
</style>

