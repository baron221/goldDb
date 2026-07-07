<template>
<div class="app-container">
    <el-card shadow="never">
      <template #header>
        <div class="card-header">
          <span>고객 문의 관리</span>
          <el-button :icon="Refresh" circle @click="getList" />
        </div>
      </template>

      <div class="filter-container" style="margin-bottom: 1.25rem;">
        <el-form :inline="true" :model="listQuery">
          <el-form-item label="처리 상태">
            <el-radio-group v-model="listQuery.isProcessed" @change="handleFilter">
              <el-radio-button :value="undefined">전체</el-radio-button>
              <el-radio-button :value="true">완료</el-radio-button>
              <el-radio-button :value="false">미완료</el-radio-button>
            </el-radio-group>
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="handleFilter">검색</el-button>
          </el-form-item>
        </el-form>
      </div>

      <base-table
        v-loading="listLoading"
        :data="list"
        :total="total"
        v-model:page="listQuery.page"
        v-model:page-size="listQuery.pageSize"
        border
        fit
        highlight-current-row
        style="width: 100%;"
        @change="getList"
      >
        <el-table-column label="ID" prop="id" width="80" align="center" />
        <el-table-column label="이름" prop="name" width="120" align="center" />
        <el-table-column label="이메일" prop="email" width="200" />
        <el-table-column label="연락처" prop="phone" width="150" align="center" />
        <el-table-column label="주제" prop="subject" width="150" />
        <el-table-column label="문의 내용" prop="message" min-width="300" show-overflow-tooltip />
        <el-table-column
          label="등록일"
          prop="createdAt"
          align="center"
          width="180"
          :excel-formatter="(row) => formatDateTime(row.createdAt)"
        >
          <template #default="{row}">
            <span>{{ formatDateTime(row.createdAt) }}</span>
          </template>
        </el-table-column>
        <el-table-column
          label="상태"
          prop="isProcessed"
          width="100"
          align="center"
          :excel-formatter="(row) => row.isProcessed ? '처리완료' : '미처리'"
        >
          <template #default="{row}">
            <el-tag :type="row.isProcessed ? 'success' : 'danger'">
              {{ row.isProcessed ? '처리완료' : '미처리' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="관리" align="center" width="120" :fixed="!isMobile ? 'right' : false">
          <template #default="{row}">
            <el-tooltip content="보기/처리" placement="top">
              <el-button type="primary" :icon="View" circle size="small" @click="handleView(row)" />
            </el-tooltip>
            <el-tooltip content="삭제" placement="top">
              <el-button type="danger" :icon="Delete" circle size="small" @click="handleDelete(row)" />
            </el-tooltip>
          </template>
        </el-table-column>
      </base-table>
    </el-card>

    <ContactDetailDialog
      v-model="dialogVisible"
      :contact="temp"
      @saved="getList"
    />
  </div>
</template>

<script setup lang="ts">
import { useMobile } from '@/hooks/useMobile';
import { ref, onMounted, reactive } from 'vue';
import { getContacts, deleteContact } from '@/api/contact-admin';
import { ElMessage, ElMessageBox } from 'element-plus';
import { Refresh, View, Delete } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import ContactDetailDialog from './components/ContactDetailDialog.vue';
import BaseTable from '@/components/BaseTable/index.vue';
const { isMobile } = useMobile();

const list = ref([]);
const total = ref(0);
const listLoading = ref(true);
const dialogVisible = ref(false);
const listQuery = reactive({
  page: 1,
  pageSize: 20,
  isProcessed: undefined
});

const temp = ref<any>({});

const getList = async () => {
  listLoading.value = true;
  try {
    const res = await getContacts(listQuery);
    list.value = res.data.items;
    total.value = res.data.totalCount;
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

const formatDateTime = (date: string) => {
  if (!date) return '-';
  return parseTime(date, '{y}-{m}-{d} {h}:{i}:{s}');
};

const handleView = (row: any) => {
  temp.value = { ...row };
  dialogVisible.value = true;
};

const handleDelete = (row: any) => {
  ElMessageBox.confirm('정말로 삭제하시겠습니까?', '경고', {
    confirmButtonText: '삭제',
    cancelButtonText: '취소',
    type: 'warning'
  }).then(async () => {
    try {
      await deleteContact(row.id);
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
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>

