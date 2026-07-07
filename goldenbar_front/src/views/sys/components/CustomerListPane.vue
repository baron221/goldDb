<template>
<el-card shadow="never" class="left-list-card">
    <template #header>
      <div class="card-header">
        <div class="section-header">
          <h2 class="section-title">고객 관리</h2>
        </div>
        <div class="header-actions">
          <el-button type="success" size="small" :loading="downloadLoading" @click="handleDownload">Excel 내보내기</el-button>
          <el-button type="primary" size="small" :icon="Plus" @click="$emit('create')">신규 등록</el-button>
        </div>
      </div>
    </template>

    <div class="filter-container" style="margin-bottom: 15px;">
      <el-row :gutter="10">
        <el-col :span="6">
          <el-select
            v-model="listQuery.companyId"
            placeholder="소매점 선택"
            clearable
            :disabled="isRetailer"
            style="width: 100%;"
            @change="handleFilter"
          >
            <el-option
              v-for="item in companies"
              :key="item.id"
              :label="item.name"
              :value="item.id"
            />
          </el-select>
        </el-col>
        <el-col :span="6">
          <el-input
            v-model="listQuery.name"
            placeholder="고객명 검색"
            clearable
            @keyup.enter="handleFilter"
          />
        </el-col>
        <el-col :span="6">
          <el-input
            v-model="listQuery.phone"
            placeholder="연락처 검색"
            clearable
            @keyup.enter="handleFilter"
          />
        </el-col>
        <el-col :span="6">
          <el-date-picker
            v-model="listQuery.birthDate"
            type="date"
            placeholder="생년월일 검색"
            style="width: 100%;"
            value-format="YYYY-MM-DD"
            clearable
            @change="handleFilter"
          />
        </el-col>
      </el-row>
      <el-button type="primary" style="width: 100%; margin-top: 10px;" :icon="Search" @click="handleFilter">검색</el-button>
    </div>

    <base-table
      ref="customerTableRef"
      :data="customerList"
      style="width: 100%"
      border
      highlight-current-row
      @row-click="row => $emit('row-click', row)"
    >
      <template #contextmenu="{ row, close }">
        <div class="menu-item" @click="handleEditRow(row, close)">
          <el-icon><Edit /></el-icon>
          <span>고객 정보 수정</span>
        </div>
        <div class="menu-item" style="color: #f56c6c;" @click="handleDeleteRow(row, close)">
          <el-icon><Delete /></el-icon>
          <span>고객 정보 삭제</span>
        </div>
      </template>

      <el-table-column prop="name" label="고객명" sortable width="100" />
      <el-table-column
        prop="birthDate"
        label="생년월일"
        width="105"
        align="center"
        :excel-formatter="(row) => formatDate(row.birthDate)"
      >
        <template #default="scope">
          {{ formatDate(scope.row.birthDate) }}
        </template>
      </el-table-column>
      <el-table-column prop="phone" label="연락처" width="130" />
      <el-table-column prop="email" label="이메일" min-width="120" show-overflow-tooltip />
      <el-table-column prop="companyName" label="등록업체" min-width="120" show-overflow-tooltip />
      <el-table-column prop="creatorName" label="등록자" width="90" align="center" />
      <el-table-column align="center" label="작업" width="60" :fixed="!isMobile ? 'right' : false">
        <template #default="scope">
          <el-button link class="delete-icon-btn" :icon="Delete" @click.stop="$emit('delete', scope.row)" />
        </template>
      </el-table-column>
    </base-table>

    <div class="pagination-container" style="margin-top: 20px; display: flex; justify-content: center;">
      <el-pagination
        v-model:current-page="listQuery.page"
        v-model:page-size="listQuery.pageSize"
        :total="total"
        layout="total, prev, pager, next"
        @current-change="handlePageChange"
      />
    </div>
  </el-card>
</template>

<script setup lang="ts">

import { ref, reactive, watch } from 'vue';
import { useMobile } from '@/hooks/useMobile';
import { getCustomers } from '@/api/customer';
import { ElMessage } from 'element-plus';
import { Delete, Plus, Search, Edit } from '@element-plus/icons-vue';
import dayjs from 'dayjs';
import BaseTable from '@/components/BaseTable/index.vue';

const props = defineProps<{
  customerList: any[];
  companies: any[];
  total: number;
  isRetailer: boolean;
  modelValue: any; 
}>();

const emit = defineEmits(['row-click', 'create', 'edit', 'delete', 'update:modelValue', 'refresh']);

const { isMobile } = useMobile();
const customerTableRef = ref(null);
const downloadLoading = ref(false);

const listQuery = reactive({
  name: '',
  phone: '',
  birthDate: null,
  companyId: null,
  page: 1,
  pageSize: 20
});

watch(listQuery, (newVal) => {
  emit('update:modelValue', newVal);
}, { deep: true });

watch(() => props.modelValue, (newVal) => {
  if (newVal) {
    Object.assign(listQuery, newVal);
  }
}, { immediate: true, deep: true });

const handleFilter = () => {
  listQuery.page = 1;
  emit('refresh');
};

const handlePageChange = (page: number) => {
  listQuery.page = page;
  emit('refresh');
};

const handleEditRow = (row: any, close: () => void) => {
  emit('row-click', row);
  close();
};

const handleDeleteRow = (row: any, close: () => void) => {
  emit('delete', row);
  close();
};

const formatDate = (value: string) => {
  if (!value) return '-';
  return dayjs(value).format('YYYY-MM-DD');
};

const formatDateTime = (value: string) => {
  if (!value) return '-';
  return dayjs(value).format('YYYY-MM-DD HH:mm:ss');
};

const formatJson = (filterVal: string[], jsonData: any[]) => {
  return jsonData.map(v => filterVal.map(j => {
    if (j === 'birthDate') {
      return formatDate(v[j]);
    } else if (j === 'createdAt') {
      return formatDateTime(v[j]);
    } else {
      return v[j];
    }
  }));
};

const handleDownload = () => {
  if (props.customerList.length === 0) {
    ElMessage.warning('다운로드할 고객 데이터가 없습니다.');
    return;
  }

  downloadLoading.value = true;
  const allParams = { ...listQuery, page: 1, pageSize: 10000 };

  getCustomers(allParams).then(response => {
    const list = response.data.items || [];

    import('@/vendor/Export2Excel').then(excel => {
      const tHeader = ['이름', '생년월일', '연락처', '이메일', '비고', '등록일시'];
      const filterVal = ['name', 'birthDate', 'phone', 'email', 'note', 'createdAt'];
      const data = formatJson(filterVal, list);

      excel.export_json_to_excel({
        header: tHeader,
        data,
        filename: '고객목록_' + dayjs().format('YYYYMMDD'),
        autoWidth: true,
        bookType: 'xlsx'
      });

      downloadLoading.value = false;
    }).catch(err => {
      console.error(err);
      ElMessage.error('엑셀 변환 중 오류가 발생했습니다.');
      downloadLoading.value = false;
    });
  }).catch(err => {
    console.error(err);
    ElMessage.error('데이터 조회에 실패했습니다.');
    downloadLoading.value = false;
  });
};
</script>

