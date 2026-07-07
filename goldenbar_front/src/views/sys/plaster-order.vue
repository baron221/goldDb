<template>
<div class="app-container">
    <el-card shadow="never">
      <template #header>
        <div class="card-header" style="display: flex; justify-content: space-between; align-items: center;">
          <span>석고 관리 (발주 현황)</span>
          <div>
            <el-button @click="getList">새로고침</el-button>
            <el-button type="primary" @click="handleCreate">새 발주 등록</el-button>
          </div>
        </div>
      </template>

      <div class="filter-container" style="margin-bottom: 1.25rem;">
        <el-form :inline="true" :model="listQuery" class="demo-form-inline">
          <el-form-item label="발주사">
            <common-select v-model="listQuery.orderingCompanyId" group-code="COMPANY_TYPE" placeholder="발주사 선택" show-all @change="handleFilter" />
          </el-form-item>
          <el-form-item label="상태">
            <el-select v-model="listQuery.status" placeholder="상태" clearable style="width: 120px;" @change="handleFilter">
              <el-option label="전체" value="전체" />
              <el-option label="발주" value="발주" />
              <el-option label="진행중" value="진행중" />
              <el-option label="완료" value="완료" />
            </el-select>
          </el-form-item>
          <el-form-item label="기간">
            <el-date-picker
              v-model="dateRange"
              type="daterange"
              range-separator="~"
              start-placeholder="시작일"
              end-placeholder="종료일"
              value-format="YYYY-MM-DD"
              style="width: 250px;"
              @change="handleDateChange"
            />
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="handleFilter">검색</el-button>
            <el-button @click="resetQuery">초기화</el-button>
          </el-form-item>
        </el-form>
      </div>

      <div class="summary-container" style="margin-bottom: 0.9375rem; padding: 0.625rem; background-color: #f0f2f5; border-radius: 2px;">
        <span style="font-weight: bold;">총 발주 수량: </span>
        <span style="color: #409EFF; font-size: 1.125rem; font-weight: bold;">{{ totalQuantity }}</span> 개
      </div>

      <base-table
        v-loading="listLoading"
        :data="list"
        :total="total"
        v-model:page="listQuery.page"
        v-model:page-size="listQuery.pageSize"
        @change="getList"
      >
        <el-table-column label="발주사" prop="orderingCompanyName" min-width="150" header-align="center" />
        <el-table-column label="발주 제조사" prop="manufacturerName" min-width="150" header-align="center" />
        <el-table-column label="수량" prop="quantity" width="100" align="right" header-align="center">
          <template #default="{row}">
            <span style="font-weight: bold;">{{ row.quantity.toLocaleString() }}</span>
          </template>
        </el-table-column>
        <el-table-column label="상태" width="100" align="center" header-align="center" :excel-formatter="(row) => row.status">
          <template #default="{row}">
            <el-tag :type="getStatusType(row.status)">{{ row.status }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="발주일자" align="center" width="180" header-align="center" :excel-formatter="(row) => formatDateTime(row.orderDate)">
          <template #default="{row}">
            <span>{{ formatDateTime(row.orderDate) }}</span>
          </template>
        </el-table-column>
        <el-table-column label="상태(취소여부)" width="120" align="center" header-align="center" :excel-formatter="(row) => row.isCancelled ? '취소됨' : '정상'">
          <template #default="{row}">
            <el-tag v-if="row.isCancelled" type="danger">취소됨</el-tag>
            <el-tag v-else type="success">정상</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="관리" align="center" width="150" header-align="center" class-name="small-padding fixed-width">
          <template #default="{row}">
            <el-button type="primary" size="small" @click="handleUpdate(row)">수정</el-button>
            <el-button type="danger" size="small" @click="handleDelete(row)">삭제</el-button>
          </template>
        </el-table-column>
      </base-table>
    </el-card>

    <plaster-order-dialog
      v-model="dialogFormVisible"
      :dialog-status="dialogStatus"
      :initial-data="currentData"
      @success="getList"
    />
  </div>
</template>

<script setup lang="ts">

import { ref, onMounted, reactive, computed } from 'vue';
import { getPlasterOrders, deletePlasterOrder, getPlasterOrder } from '@/api/plaster-order';
import { ElMessageBox, ElMessage } from 'element-plus';
import CommonSelect from '@/components/CommonSelect/index.vue';
import BaseTable from '@/components/BaseTable/index.vue';
import { parseTime } from '@/utils';
import BasePopup from '@/components/BasePopup/index.vue';
import PlasterOrderDialog from './components/PlasterOrderDialog.vue';

const end = new Date();
const start = new Date();
start.setTime(start.getTime() - 3600 * 1000 * 24 * 30);
const defaultStartDate = parseTime(start, '{y}-{m}-{d}');
const defaultEndDate = parseTime(end, '{y}-{m}-{d}');

const list = ref([]);
const total = ref(0);
const listLoading = ref(true);
const dateRange = ref<string[]>([defaultStartDate, defaultEndDate]);
const listQuery = reactive({
  page: 1,
  pageSize: 20,
  orderingCompanyId: undefined,
  status: '전체',
  startDate: defaultStartDate,
  endDate: defaultEndDate
});

const dialogFormVisible = ref(false);
const dialogStatus = ref('create');
const currentData = ref<any>({});

const totalQuantity = computed(() => {
  return list.value.reduce((acc, cur) => acc + (cur.isCancelled ? 0 : cur.quantity), 0);
});

const getList = async () => {
  listLoading.value = true;
  try {
    const response = await getPlasterOrders(listQuery);
    list.value = response.data.items;
    total.value = response.data.totalCount;
  } catch (error) {
    console.error(error);
  } finally {
    listLoading.value = false;
  }
};

const handleFilter = () => {
  listQuery.page = 1;
  getList();
};

const handleDateChange = (val) => {
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
  dateRange.value = [];
  Object.assign(listQuery, {
    page: 1,
    pageSize: 20,
    orderingCompanyId: undefined,
    status: '전체',
    startDate: undefined,
    endDate: undefined
  });
  getList();
};

const getStatusType = (status: string) => {
  switch (status) {
    case '발주': return 'info';
    case '진행중': return 'warning';
    case '완료': return 'success';
    default: return '';
  }
};

const formatDateTime = (date: string) => {
  if (!date) return '-';
  return parseTime(date, '{y}-{m}-{d} {h}:{i}:{s}');
};

const handleCreate = () => {
  currentData.value = {
    id: undefined,
    orderingCompanyId: undefined,
    manufacturerId: undefined,
    quantity: 1,
    status: '발주',
    orderDate: new Date(),
    isCancelled: false,
    remarks: ''
  };
  dialogStatus.value = 'create';
  dialogFormVisible.value = true;
};

const handleUpdate = async (row: any) => {
  try {
    const response = await getPlasterOrder(row.id);
    currentData.value = { ...response.data };
    dialogStatus.value = 'update';
    dialogFormVisible.value = true;
  } catch (error) {
    console.error(error);
  }
};

const handleDelete = (row: any) => {
  ElMessageBox.confirm('정말로 이 발주 내역을 삭제하시겠습니까?', '경고', {
    confirmButtonText: '삭제',
    cancelButtonText: '취소',
    type: 'warning'
  }).then(async () => {
    try {
      await deletePlasterOrder(row.id);
      ElMessage.success('삭제되었습니다.');
      getList();
    } catch (error) {
      console.error(error);
    }
  });
};

onMounted(() => {
  getList();
});
</script>

<style scoped>
.filter-container {
  padding: 1.25rem;
  border-radius: 2px;
}
.summary-container {
  display: flex;
  align-items: center;
}

@media (max-width: 768px) {
  :deep(.responsive-dialog-600) {
    width: 90% !important;
  }
  :deep(.el-form-item) {
    flex-direction: column;
    align-items: flex-start;
  }
  :deep(.el-form-item__label) {
    text-align: left;
    margin-bottom: 4px;
  }
  :deep(.el-form-item__content) {
    width: 100%;
    margin-left: 0 !important;
  }
}
@media (min-width: 768px) {
  :deep(.responsive-dialog-600) {
    width: 600px !important;
  }
}
</style>

