<template>
<div class="app-container">
    <el-card shadow="never">
      <template #header>
        <div class="card-header" style="display: flex; justify-content: space-between; align-items: center;">
          <span>책 관리 (카탈로그)</span>
          <div>
            <el-button @click="getList">새로고침</el-button>
            <el-button type="primary" @click="handleCreate">새 책 등록</el-button>
          </div>
        </div>
      </template>

      <div class="filter-container" style="margin-bottom: 1.25rem;">
        <el-form :inline="true" :model="listQuery" class="demo-form-inline">
          <el-form-item label="책제목">
            <el-input v-model="listQuery.title" placeholder="책제목" clearable @keyup.enter="handleFilter" />
          </el-form-item>
          <el-form-item label="보안분류">
            <el-select v-model="listQuery.securityClass" placeholder="전체" clearable style="width: 120px;" @change="handleFilter">
              <el-option label="일반" value="일반" />
              <el-option label="보안" value="보안" />
            </el-select>
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="handleFilter">검색</el-button>
            <el-button @click="resetQuery">초기화</el-button>
          </el-form-item>
        </el-form>
      </div>

      <base-table
        v-loading="listLoading"
        :data="list"
        border
        fit
        highlight-current-row
        v-model:page="listQuery.page"
        v-model:page-size="listQuery.pageSize"
        :total="total"
        @change="getList"
        style="width: 100%;"
      >
        <el-table-column label="책제목" prop="title" min-width="250" header-align="center" />
        <el-table-column
          label="보안분류"
          prop="securityClass"
          width="100"
          align="center"
          header-align="center"
          :excel-formatter="(row: any) => row.securityClass"
        >
          <template #default="{row}">
            <el-tag :type="row.securityClass === '보안' ? 'danger' : 'success'">{{ row.securityClass }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="발행호" prop="issueNo" width="120" align="center" header-align="center" />
        <el-table-column label="전체페이지" prop="totalPages" width="100" align="right" header-align="center" />
        <el-table-column
          label="등록일"
          align="center"
          width="180"
          header-align="center"
          prop="createdAt"
          :excel-formatter="(row: any) => formatDateTime(row.createdAt)"
        >
          <template #default="{row}">
            <span>{{ formatDateTime(row.createdAt) }}</span>
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

    <CatalogDialog
      v-model="dialogFormVisible"
      :id="currentId"
      @success="getList"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue';
import { getCatalogs, deleteCatalog } from '@/api/catalog';
import { ElMessageBox, ElMessage } from 'element-plus';
import { parseTime } from '@/utils';
import CatalogDialog from '@/components/CatalogDialog/index.vue';
import BaseTable from '@/components/BaseTable/index.vue';

const list = ref([]);
const total = ref(0);
const listLoading = ref(true);
const listQuery = reactive({
  page: 1,
  pageSize: 20,
  title: '',
  securityClass: undefined
});

const dialogFormVisible = ref(false);
const currentId = ref<number | null>(null);

const getList = async () => {
  listLoading.value = true;
  try {
    const response = await getCatalogs(listQuery);
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

const resetQuery = () => {
  Object.assign(listQuery, {
    page: 1,
    pageSize: 20,
    title: '',
    securityClass: undefined
  });
  getList();
};

const formatDateTime = (date: string) => {
  if (!date) return '-';
  return parseTime(date, '{y}-{m}-{d}');
};

const handleCreate = () => {
  currentId.value = null;
  dialogFormVisible.value = true;
};

const handleUpdate = (row: any) => {
  currentId.value = row.id;
  dialogFormVisible.value = true;
};

const handleDelete = (row: any) => {
  ElMessageBox.confirm('정말로 이 책 정보를 삭제하시겠습니까?', '경고', {
    confirmButtonText: '삭제',
    cancelButtonText: '취소',
    type: 'warning'
  }).then(async () => {
    try {
      await deleteCatalog(row.id);
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
  padding-bottom: 0.625rem;
}
</style>

