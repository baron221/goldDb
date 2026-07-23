<template>
<div class="app-container">

    <el-card shadow="never" style="margin-top: 1.25rem;">
      <direct-stock-filter
        v-model="listQuery"
        :direct-retailers="directRetailers"
        @filter="handleFilter"
      />

      <div style="display: flex; justify-content: flex-end; margin-bottom: 0.9375rem;">
        <el-button type="success" :icon="Download" @click="handleExportExcel">엑셀 다운로드</el-button>
      </div>

      <base-table
        ref="tableRef"
        v-loading="listLoading"
        :data="list"
        border
        fit
        highlight-current-row
        style="width: 100%;"
        :header-cell-style="{ textAlign: 'center' }"
        @selection-change="handleSelectionChange"
      >
        <el-table-column type="selection" width="55" align="center" :fixed="!isMobile ? 'left' : false" />
        <el-table-column label="소매점" prop="ownerCompanyName" width="150" header-align="center" sortable />
        <el-table-column
          label="상품내용"
          prop="productName"
          min-width="250"
          header-align="center"
          sortable
          :excel-formatter="(row) => `[${row.productNo}] ${row.productName} (${codeMap[row.categoryLarge] || row.categoryLarge}${row.categoryMedium ? ' > ' + (codeMap[row.categoryMedium] || row.categoryMedium) : ''}${row.categorySmall ? ' > ' + (codeMap[row.categorySmall] || row.categorySmall) : ''})`"
        >
          <template #default="{row}">
            <div class="product-info" @click="handleDetail(row)" style="cursor: pointer;">
              <div class="product-no">{{ row.productNo }}</div>
              <div class="product-name">{{ row.productName }}</div>
              <div class="category">
                {{ codeMap[row.categoryLarge] || row.categoryLarge }}
                <template v-if="row.categoryMedium"> > {{ codeMap[row.categoryMedium] || row.categoryMedium }}</template>
                <template v-if="row.categorySmall"> > {{ codeMap[row.categorySmall] || row.categorySmall }}</template>
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="사진" prop="productPhotoUrl" width="80" align="center" header-align="center">
          <template #default="{row}">
            <el-image
              v-if="(row.attachments && row.attachments.length > 0) || row.productPhotoUrl"
              :src="(row.attachments && row.attachments.find(a => a.isMain)) ? row.attachments.find(a => a.isMain).filePath : (row.attachments && row.attachments.length > 0 ? row.attachments[0].filePath : row.productPhotoUrl)"
              style="width: 50px; height: 50px; border-radius: 2px;"
              :preview-src-list="[(row.attachments && row.attachments.length > 0) ? row.attachments[0].filePath : row.productPhotoUrl]"
            />
            <div v-else style="width: 50px; height: 50px; background: #f5f5f5; border-radius: 2px; display: flex; align-items: center; justify-content: center; font-size: 0.825rem; color: #999;">NO IMG</div>
          </template>
        </el-table-column>
        <el-table-column
          label="상태"
          prop="status"
          width="100"
          align="center"
          header-align="center"
          sortable
          :excel-formatter="(row) => row.isExhausted ? '소진됨' : (codeMap[row.status] || row.status)"
        >
          <template #default="{row}">
            <el-tag :type="row.isExhausted ? 'danger' : 'success'" size="small">
              {{ row.isExhausted ? '소진됨' : (codeMap[row.status] || row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="제조사" prop="companyName" width="120" header-align="center" sortable />
        <el-table-column label="재고번호" prop="stockNo" width="150" align="center" header-align="center" sortable />
        <el-table-column
          label="함량/컬러"
          prop="purity"
          width="120"
          align="center"
          header-align="center"
          sortable
          :excel-formatter="(row) => `${codeMap[row.purity] || row.purity}${row.color ? ' / ' + (codeMap[row.color] || row.color) : ''}`"
        >
          <template #default="{row}">
            <div>{{ codeMap[row.purity] || row.purity }}</div>
            <div v-if="row.color" style="font-size: 0.8875rem; color: #999;">{{ codeMap[row.color] || row.color }}</div>
          </template>
        </el-table-column>
        <el-table-column
          label="실중량"
          prop="actualWeight"
          width="100"
          align="right"
          header-align="center"
          sortable
          :excel-formatter="(row) => `${row.actualWeight.toFixed(2)}g`"
        >
          <template #default="{row}">
            <span>{{ row.actualWeight.toFixed(2) }}g</span>
          </template>
        </el-table-column>
        <el-table-column
          label="수량"
          prop="quantity"
          width="70"
          align="center"
          header-align="center"
          sortable
          :excel-formatter="(row) => `${row.quantity || 1}`"
        >
          <template #default="{row}">
            <span>{{ row.quantity || 1 }}</span>
          </template>
        </el-table-column>
        <el-table-column
          label="수령일자"
          prop="createdAt"
          width="140"
          align="center"
          header-align="center"
          sortable
          :excel-formatter="(row) => formatDate(row.createdAt)"
        >
          <template #default="{row}">
            <span style="font-size: 0.95rem; color: #606266;">{{ formatDate(row.createdAt) }}</span>
          </template>
        </el-table-column>
        <el-table-column label="액션" width="80" align="center" :fixed="!isMobile ? 'right' : false" header-align="center">
          <template #default="{row}">
            <el-dropdown trigger="click">
              <el-button size="small" :icon="MoreFilled" circle />
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item :icon="MoreFilled" @click="handleDetail(row)">상세보기</el-dropdown-item>
                  <el-dropdown-item :icon="Delete" style="color: #f56c6c;" @click="handleDelete(row)">재고소진</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </template>
        </el-table-column>
      </base-table>

      <div class="pagination-container" style="margin-top: 1.25rem;">
        <el-pagination
          v-model:current-page="listQuery.page"
          v-model:page-size="listQuery.pageSize"
          :total="total"
          :page-sizes="[20, 50, 100, 200]"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="getList"
          @current-change="getList"
        />
      </div>

      <div class="action-footer" style="margin-top: 1.25rem; display: flex; gap: 0.625rem; flex-wrap: wrap;">
        <el-button type="danger" @click="handleBulkDelete">선택상품 재고소진</el-button>
      </div>
    </el-card>

    <stock-exhaustion-reason-dialog
      v-model="deleteDialogVisible"
      :ids="itemToDelete ? [itemToDelete.id] : selectedItems.map(i => i.id)"
      @saved="getList"
    />
  </div>
</template>

<script setup lang="ts">

import { useMobile } from '@/hooks/useMobile';
import { ref, reactive, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { fetchStocks, fetchStockSummary } from '@/api/stock';
import { getRetailersByCenter } from '@/api/company';
import { ElMessage } from 'element-plus';
import { Delete, MoreFilled, Download } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import useUserStore from '@/store/modules/user';
import useCodeStore from '@/store/modules/code';
import BaseTable from '@/components/BaseTable/index.vue';
import StockExhaustionReasonDialog from './components/StockExhaustionReasonDialog.vue';
import DirectStockFilter from './components/DirectStockFilter.vue';
const { isMobile } = useMobile();

const userStore = useUserStore();
const router = useRouter();
const codeStore = useCodeStore();

const tableRef = ref<any>(null);

const handleExportExcel = () => {
  if (tableRef.value) {
    tableRef.value.exportExcel('직영재고_현황_목록');
  }
};

const list = ref<any[]>([]);
const total = ref(0);
const listLoading = ref(true);
const selectedItems = ref<any[]>([]);
const codeMap = computed(() => codeStore.codeMap);
const directRetailers = ref<any[]>([]);

const summary = reactive({
  total14KWeight: 0,
  total18KWeight: 0,
  totalPureGoldWeight: 0,
  totalCalculatedPureGoldWeight: 0
});

const listQuery = ref({
  page: 1,
  pageSize: 50,
  categoryLarge: undefined as string | undefined,
  categoryMedium: undefined as string | undefined,
  categorySmall: undefined as string | undefined,
  setCategoryLarge: undefined as string | undefined,
  setCategoryMedium: undefined as string | undefined,
  setCategorySmall: undefined as string | undefined,
  orderNo: '',
  stockNo: '',
  productName: '',
  status: '',
  minWeight: undefined as number | undefined,
  maxWeight: undefined as number | undefined,
  companyId: undefined as number | undefined,
  logisticsCompanyId: userStore.companyId || undefined,
  isDirectManagement: true,
  isExhausted: false
});

const getList = async () => {
  listLoading.value = true;
  try {
    const [res] = await Promise.all([
      fetchStocks(listQuery.value),
      codeStore.fetchCodes()
    ]);

    list.value = res.data.items;
    total.value = res.data.totalCount;

    const sumRes = await fetchStockSummary(listQuery.value);
    Object.assign(summary, sumRes.data);
  } catch (error) {
    console.error(error);
  } finally {
    listLoading.value = false;
  }
};

const fetchDirectRetailers = async () => {
  if (!userStore.companyId) return;
  try {
    const res = await getRetailersByCenter(userStore.companyId);
    directRetailers.value = res.data.filter((r: any) => r.isDirectManagement);
  } catch (error) {
    console.error('Failed to fetch direct retailers:', error);
  }
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-';
  return parseTime(new Date(dateStr), '{y}-{m}-{d} {h}:{i}');
};

const handleFilter = () => {
  listQuery.value.page = 1;
  getList();
};

const handleDetail = (row: any) => {
  router.push(`/stock/stock_detail/${row.id}`);
};

const handleSelectionChange = (val: any[]) => {
  selectedItems.value = val;
};

const itemToDelete = ref<any>(null);
const deleteDialogVisible = ref(false);

const handleDelete = (row: any) => {
  itemToDelete.value = row;
  deleteDialogVisible.value = true;
};

const handleBulkDelete = () => {
  if (selectedItems.value.length === 0) {
    ElMessage.warning('대상을 선택해주세요.');
    return;
  }
  itemToDelete.value = null;
  deleteDialogVisible.value = true;
};

onMounted(() => {
  fetchDirectRetailers();
  getList();
});
</script>

<style lang="scss" src="./DirectStockStyles.scss" scoped></style>

<script lang="ts">
export default {
  name: 'DirectStockManagement'
};
</script>

