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
                <el-table-column :label="$t('admin.settlement.table.amount')" width="120" align="right" prop="settlementAmount" :excel-formatter="(row) => '₩ ' + formatPrice(row.settlementAmount || row.totalPrice || row.price || (((row.retailerConfirmMaterialCost || row.factoryInputMaterialCost || 0) + (row.retailerConfirmLaborCost || row.factoryInputLaborCost || 0)) * (row.quantity || 1)))">
                  <template #default="item">
                    <span style="font-weight: bold; color: #f56c6c;">₩ {{ formatPrice(item.row.settlementAmount || item.row.totalPrice || item.row.price || (((item.row.retailerConfirmMaterialCost || item.row.factoryInputMaterialCost || 0) + (item.row.retailerConfirmLaborCost || item.row.factoryInputLaborCost || 0)) * (item.row.quantity || 1))) }}</span>
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
        <el-table-column :label="$t('orderDetail.headers.productInfo')" min-width="220" :excel-formatter="productSummaryFormatter">
          <template #default="{row}">
            <div v-if="primaryOrderItem(row)" class="product-info-cell">
              <el-image :src="primaryOrderItem(row).photoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 40px; height: 40px;" />
              <div class="product-text">
                <div class="product-name">
                  {{ primaryOrderItem(row).productName || primaryOrderItem(row).productSetTitle }}
                  <el-tag v-if="orderItemCount(row) > 1" size="small" type="info" effect="plain" style="margin-left: 0.3125rem;">+{{ orderItemCount(row) - 1 }}</el-tag>
                </div>
                <div class="product-no">{{ primaryOrderItem(row).productNo }}</div>
              </div>
            </div>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column v-if="!isMfg" :label="$t('admin.settlement.filters.company')" width="180" align="center" prop="userDisplayName" :excel-formatter="(row) => `${row.userDisplayName} (${row.userName})`">
          <template #default="{row}">
            <span>{{ row.userDisplayName }} ({{ row.userName }})</span>
          </template>
        </el-table-column>
        <el-table-column v-else label="물류" width="180" align="center" prop="logisticsCompanyName" :excel-formatter="(row) => row.logisticsCompanyName || '-'">
          <template #default="{row}">
            <span>{{ row.logisticsCompanyName || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column :label="$t('admin.settlement.summary.totalAmount')" width="150" align="right" prop="totalSettlementAmount" :excel-formatter="(row) => '₩ ' + formatPrice(calculateOrderTotalSettlement(row))">
          <template #default="{row}">
            <span style="font-weight: bold; color: #f56c6c;">₩ {{ formatPrice(calculateOrderTotalSettlement(row)) }}</span>
          </template>
        </el-table-column>
        <el-table-column v-if="!isMfg" :label="$t('admin.settlement.table.receivableLink')" width="150" align="center">
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
      <div class="receipt-double-container">
        <template v-for="copyType in ['고객용', '보관용']" :key="copyType">
          <div class="receipt-copy-block">
            <!-- Header Table -->
            <table class="receipt-header-table">
              <tr>
                <td class="header-title-cell">
                  <span class="company-tag">[{{ isMfg ? (printData.companyName || '제조사') : (printData.userDisplayName || printData.userName || printData.companyName || '거래처') }}]</span>
                  <span class="main-title">임가공 거래 명세서</span>
                  <span class="copy-badge" :class="{ 'blue-badge': copyType === '고객용' }">({{ copyType }})</span>
                </td>
                <td class="supplier-info-cell" rowspan="2">
                  <div>공급자: {{ isMfg ? (printData.companyName || '제조사') : (printData.logisticsCompanyName || '골든바') }}</div>
                  <div>전 화: {{ printData.logisticsCompanyPhone || printData.userPhone || '051-633-1116' }}</div>
                  <div>팩 스: {{ printData.logisticsCompanyFax || '-' }}</div>
                </td>
              </tr>
              <tr>
                <td class="header-meta-cell">
                  <span>일자: {{ formatDate(printData.createdAt) }}</span>
                  <span class="meta-separator">|</span>
                  <span>거래No: {{ printData.id || printData.orderNo }}</span>
                </td>
              </tr>
            </table>

            <!-- Order Items Table -->
            <table class="receipt-items-table">
              <thead>
                <tr>
                  <th width="12%">No</th>
                  <th width="42%">주문내용</th>
                  <th width="12%">함량</th>
                  <th width="13%">실중량</th>
                  <th width="10%">주문수량</th>
                  <th width="11%">공임비</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in paddedOrderItems(printData.orderItems, 6)" :key="item.id">
                  <template v-if="!item.isDummy">
                    <td align="center">{{ item.productNo || item.id }}</td>
                    <td>
                      <div class="item-info-flex">
                        <img v-if="item.photoUrl" :src="item.photoUrl" class="item-img" />
                        <span class="item-name">{{ item.productName || item.productSetTitle }}</span>
                      </div>
                    </td>
                    <td align="center">{{ codeMap[item.purity] || item.purity || '-' }}</td>
                    <td align="right">{{ (item.actualWeight || item.requestedWeight || 0).toFixed(3) }}g</td>
                    <td align="center">{{ item.quantity }}개</td>
                    <td align="right">{{ formatPrice(isMfg ? ((item.factoryInputMaterialCost || 0) + (item.factoryInputLaborCost || 0)) * (item.quantity || 1) : (item.settlementAmount || item.totalPrice || 0)) }}</td>
                  </template>
                  <template v-else>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                  </template>
                </tr>
              </tbody>
            </table>

            <!-- Settlement Balance Table -->
            <table class="receipt-balance-table">
              <thead>
                <tr>
                  <th width="30%"></th>
                  <th width="22%">순금(g)</th>
                  <th width="24%">공임 및 현금</th>
                  <th width="24%">금액 합계</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td class="row-label">최근 결제</td>
                  <td></td>
                  <td></td>
                  <td align="center" style="font-size: 9px;">{{ formatDate(printData.createdAt) }}</td>
                </tr>
                <tr>
                  <td class="row-label">거래 전 미수(A)</td>
                  <td align="right">{{ (printData.beforeWeight || 0).toFixed(2) }}</td>
                  <td align="right">{{ formatPrice(printData.beforeAmount || 0) }}</td>
                  <td></td>
                </tr>
                <tr>
                  <td class="row-label">판매(B)</td>
                  <td align="right">{{ (calculateOrderTotalPureWeight(printData) || 0).toFixed(2) }}</td>
                  <td align="right">{{ formatPrice(calculateOrderTotalSettlement(printData) || 0) }}</td>
                  <td></td>
                </tr>
                <tr>
                  <td class="row-label">결제(C)</td>
                  <td align="right">{{ (printData.paidWeight || 0).toFixed(2) }}</td>
                  <td align="right">{{ formatPrice(printData.paidAmount || 0) }}</td>
                  <td></td>
                </tr>
                <tr>
                  <td class="row-label">할인(D)</td>
                  <td align="right">0.00</td>
                  <td align="right">{{ formatPrice(printData.discountAmount || 0) }}</td>
                  <td></td>
                </tr>
                <tr class="after-balance-row">
                  <td class="row-label"><strong>거래 후 미수<br/>(A+B-C-D)</strong></td>
                  <td align="right"><strong>{{ ((printData.beforeWeight || 0) + (calculateOrderTotalPureWeight(printData) || 0) - (printData.paidWeight || 0)).toFixed(2) }}</strong></td>
                  <td align="right"><strong>{{ formatPrice((printData.beforeAmount || 0) + (calculateOrderTotalSettlement(printData) || 0) - (printData.paidAmount || 0) - (printData.discountAmount || 0)) }}</strong></td>
                  <td></td>
                </tr>
              </tbody>
            </table>
          </div>
        </template>
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
import useUserStore from '@/store/modules/user';
import BaseTable from '@/components/BaseTable/index.vue';
import SettlementHistoryFilter from './components/SettlementHistoryFilter.vue';

const { isMobile } = useMobile();
const router = useRouter();
const userStore = useUserStore();
const isMfg = computed(() => userStore.companyType === 'MFG');

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
    case '14K': ratio = 0.6435; break;
    case '18K': ratio = 0.825; break;
    case '24K': ratio = 1.0; break;
    case 'PT': ratio = 0.95; break;
    default: ratio = 0;
  }
  return (weight * ratio).toFixed(2);
};

const primaryOrderItem = (row: any) => {
  return row.orderItems && row.orderItems.length > 0 ? row.orderItems[0] : null;
};

const orderItemCount = (row: any) => {
  return row.orderItems ? row.orderItems.length : 0;
};

const productSummaryFormatter = (row: any) => {
  const item = primaryOrderItem(row);
  if (!item) return '-';
  const name = item.productName || item.productSetTitle || '';
  const extra = orderItemCount(row) > 1 ? ` 외 ${orderItemCount(row) - 1}건` : '';
  return `${name}${extra}`;
};

const flattenOrderItems = (orderItems: any[]) => {
  const result: any[] = [];
  if (!orderItems) return result;
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

const paddedOrderItems = (orderItems: any[], minCount: number = 6) => {
  const flattened = flattenOrderItems(orderItems || []);
  const result = [...flattened];
  while (result.length < minCount) {
    result.push({ isDummy: true, id: `dummy-${result.length}` });
  }
  return result;
};

const calculateOrderTotalSettlement = (order: any) => {
  if (!order) return 0;
  if (isMfg.value) {
    let mfgTotal = 0;
    if (order.orderItems) {
      order.orderItems.forEach((item: any) => {
        mfgTotal += ((item.factoryInputMaterialCost || 0) + (item.factoryInputLaborCost || 0)) * (item.quantity || 1);
        if (item.children) {
          item.children.forEach((c: any) => {
            mfgTotal += ((c.factoryInputMaterialCost || 0) + (c.factoryInputLaborCost || 0)) * (c.quantity || 1);
          });
        }
      });
    }
    return mfgTotal;
  }

  if (order.settlementAmount && order.settlementAmount > 0) {
    return order.settlementAmount;
  }

  let total = 0;
  if (order.orderItems) {
    order.orderItems.forEach((item: any) => {
      total += (item.settlementAmount || item.totalPrice || item.price || 0);
      if (item.children) {
        item.children.forEach((c: any) => {
          total += (c.settlementAmount || c.totalPrice || c.price || 0);
        });
      }
    });
  }
  return total || order.totalAmount || 0;
};

const calculateOrderTotalPureWeight = (order: any) => {
  if (!order || !order.orderItems) return 0;
  let total = 0;
  order.orderItems.forEach((item: any) => {
    const weight = item.actualWeight || item.requestedWeight || item.confirmedWeight || 0;
    let ratio = 0;
    switch (item.purity) {
      case '14K': ratio = 0.6435; break;
      case '18K': ratio = 0.825; break;
      case '24K': ratio = 1.0; break;
      case 'PT': ratio = 0.95; break;
      default: ratio = 0;
    }
    total += weight * ratio * (item.quantity || 1);
    if (item.children) {
      item.children.forEach((c: any) => {
        const cWeight = c.actualWeight || c.requestedWeight || c.confirmedWeight || 0;
        let cRatio = 0;
        switch (c.purity) {
          case '14K': cRatio = 0.6435; break;
          case '18K': cRatio = 0.825; break;
          case '24K': cRatio = 1.0; break;
          case 'PT': cRatio = 0.95; break;
          default: cRatio = 0;
        }
        total += cWeight * cRatio * (c.quantity || 1);
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
    path: '/admin/receivable-management',
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
      printWindow.document.write(`
        <html>
        <head>
          <title>거래명세서 - ${row.orderNo || ''}</title>
          <style>
            @page { size: A4 landscape; margin: 8mm; }
            body { font-family: 'Malgun Gothic', 'Noto Sans KR', sans-serif; color: #111; margin: 0; padding: 5px; background: #fff; }
            .receipt-double-container { display: flex; gap: 6mm; width: 100%; box-sizing: border-box; }
            .receipt-copy-block { flex: 1; min-width: 0; border: 1.5px solid #222; padding: 6px; background: #fff; box-sizing: border-box; }

            .receipt-header-table { width: 100%; border-collapse: collapse; margin-bottom: 6px; border: 1px solid #333; }
            .receipt-header-table td { padding: 4px 6px; border: 1px solid #333; vertical-align: top; }
            .header-title-cell { font-size: 12px; font-weight: bold; }
            .company-tag { color: #333; margin-right: 4px; }
            .main-title { font-size: 13px; font-weight: bold; letter-spacing: 0.5px; }
            .copy-badge { font-weight: bold; margin-left: 4px; color: #222; }
            .copy-badge.blue-badge { color: #1d4ed8; }
            .header-meta-cell { font-size: 10px; color: #444; background: #fafafa; }
            .meta-separator { margin: 0 4px; color: #aaa; }
            .supplier-info-cell { font-size: 10px; text-align: left; width: 35%; background: #fff; line-height: 1.3; }

            .receipt-items-table { width: 100%; border-collapse: collapse; margin-bottom: 8px; border: 1px solid #333; font-size: 10px; }
            .receipt-items-table th { background-color: #f2f2f2; border: 1px solid #333; padding: 4px 2px; font-weight: bold; text-align: center; color: #222; }
            .receipt-items-table td { border: 1px solid #333; padding: 3px 5px; vertical-align: middle; height: 25px; box-sizing: border-box; }
            .item-info-flex { display: flex; align-items: center; gap: 4px; }
            .item-img { width: 22px; height: 22px; object-fit: cover; border-radius: 2px; border: 1px solid #ccc; }
            .item-name { font-weight: 600; }

            .receipt-balance-table { width: 100%; border-collapse: collapse; border: 1px solid #333; font-size: 10px; }
            .receipt-balance-table th { background-color: #f2f2f2; border: 1px solid #333; padding: 4px; font-weight: bold; text-align: center; color: #222; }
            .receipt-balance-table td { border: 1px solid #333; padding: 3px 5px; }
            .receipt-balance-table .row-label { background-color: #fafafa; font-weight: 600; text-align: left; }
            .receipt-balance-table .after-balance-row td { background-color: #f8f9fa; font-weight: bold; }
          </style>
        </head>
        <body>
      `);
      printWindow.document.write(printContents);
      printWindow.document.write('<script>window.onload = () => { window.print(); setTimeout(() => window.close(), 500); };<\/script>');
      printWindow.document.write('</body></html>');
      printWindow.document.close();
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

