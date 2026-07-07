<template>
<div class="app-container">
    <div class="filter-container flex flex-wrap gap-2 items-center">
      <el-input v-model="listQuery.title" :placeholder="$t('notice.labels.title')" style="width: 200px;" class="filter-item" @keyup.enter="handleFilter" />
      <el-select v-model="listQuery.isLoginRequired" :placeholder="$t('notice.labels.visibility')" clearable style="width: 150px" class="filter-item" @change="handleFilter">
        <el-option :label="$t('notice.labels.all')" :value="undefined" />
        <el-option :label="$t('notice.labels.system')" :value="true" />
        <el-option :label="$t('notice.labels.public')" :value="false" />
      </el-select>
      <el-button class="filter-item" type="primary" :icon="Search" @click="handleFilter">
        {{ $t('common.search') }}
      </el-button>
      <el-button class="filter-item" type="primary" :icon="Plus" @click="handleCreate">
        {{ $t('notice.actions.add') }}
      </el-button>
    </div>

    <base-table
      v-loading="listLoading"
      :data="list"
      highlight-current-row
      style="width: 100%;"
    >
      <el-table-column label="ID" prop="id" align="center" width="80" />
      <el-table-column :label="$t('notice.labels.title')" prop="title" min-width="150px" :excel-formatter="row => row.title + (row.isLoginRequired ? ` (${$t('notice.labels.loginRequired')})` : '')">
        <template #default="{row}">
          <span class="link-type" @click="handleUpdate(row)">{{ row.title }}</span>
          <el-tag v-if="row.isLoginRequired" type="warning" size="small" style="margin-left: 0.625rem;">{{ $t('notice.labels.loginRequired') }}</el-tag>
          <el-tag v-else type="success" size="small" style="margin-left: 0.625rem;">{{ $t('notice.labels.publicOpen') }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column :label="$t('notice.labels.viewCount')" prop="viewCount" align="center" width="100" />
      <el-table-column :label="$t('notice.labels.status')" prop="isActive" class-name="status-col" width="100" :excel-formatter="row => row.isActive ? $t('notice.labels.active') : $t('notice.labels.inactive')">
        <template #default="{row}">
          <el-tag :type="row.isActive ? 'success' : 'info'">
            {{ row.isActive ? $t('notice.labels.active') : $t('notice.labels.inactive') }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column :label="$t('notice.labels.period')" align="center" width="300" :excel-formatter="row => (row.startDate || row.endDate) ? `${row.startDate ? formatDate(row.startDate) : $t('notice.labels.noLimit')} ~ ${row.endDate ? formatDate(row.endDate) : $t('notice.labels.noLimit')}` : $t('notice.labels.always')">
        <template #default="{row}">
          <span v-if="row.startDate || row.endDate">
            {{ row.startDate ? formatDate(row.startDate) : $t('notice.labels.noLimit') }} ~
            {{ row.endDate ? formatDate(row.endDate) : $t('notice.labels.noLimit') }}
          </span>
          <span v-else>{{ $t('notice.labels.always') }}</span>
        </template>
      </el-table-column>
      <el-table-column :label="$t('notice.labels.date')" prop="createdAt" align="center" width="180" :excel-formatter="row => formatDate(row.createdAt)">
        <template #default="{row}">
          <span>{{ formatDate(row.createdAt) }}</span>
        </template>
      </el-table-column>
      <el-table-column :label="$t('common.action')" align="center" width="280" class-name="small-padding fixed-width">
        <template #default="{row}">
          <el-button type="info" size="small" @click="handlePreview(row)">
            {{ $t('notice.actions.preview') }}
          </el-button>
          <el-button type="primary" size="small" @click="handleUpdate(row)">
            {{ $t('common.edit') }}
          </el-button>
          <el-button size="small" type="danger" @click="handleDelete(row)">
            {{ $t('common.delete') }}
          </el-button>
        </template>
      </el-table-column>
    </base-table>

    <notice-edit-dialog
      v-model="dialogFormVisible"
      :dialog-status="dialogStatus"
      :notice-data="selectedNotice"
      @saved="getList"
    />

    <notice-popup
      v-model="previewVisible"
      :notice="previewNotice"
      is-preview
    />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { getNotices, deleteNotice } from '@/api/notice';
import { ElMessage, ElMessageBox } from 'element-plus';
import { Search, Plus } from '@element-plus/icons-vue';
import NoticePopup from '@/components/NoticePopup/index.vue';
import NoticeEditDialog from './components/NoticeEditDialog.vue';
import BaseTable from '@/components/BaseTable/index.vue';

const { t } = useI18n();
const list = ref([]);
const listLoading = ref(true);
const listQuery = reactive({
  title: '',
  isLoginRequired: undefined
});

const dialogFormVisible = ref(false);
const dialogStatus = ref('');
const selectedNotice = ref<any>(null);

const previewVisible = ref(false);
const previewNotice = ref({});

const getList = async () => {
  listLoading.value = true;
  try {
    const res = await getNotices(listQuery);
    list.value = res.data; 
  } catch (error) {
    console.error(error);
  } finally {
    listLoading.value = false;
  }
};

const handleFilter = () => {
  getList();
};

const handleCreate = () => {
  selectedNotice.value = null;
  dialogStatus.value = 'create';
  dialogFormVisible.value = true;
};

const handleUpdate = (row: any) => {
  selectedNotice.value = { ...row };
  dialogStatus.value = 'update';
  dialogFormVisible.value = true;
};

const handlePreview = (row: any) => {
  previewNotice.value = row;
  previewVisible.value = true;
};

const handleDelete = (row: any) => {
  ElMessageBox.confirm(t('notice.messages.confirmDelete'), t('favorite.messages.confirmTitle'), {
    confirmButtonText: t('common.ok'),
    cancelButtonText: t('common.cancel'),
    type: 'warning'
  }).then(async () => {
    await deleteNotice(row.id);
    ElMessage.success(t('notice.messages.deleteSuccess'));
    getList();
  });
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  return date.toLocaleString();
};

onMounted(() => {
  getList();
});
</script>

<style scoped>
.filter-container {
  padding-bottom: 0.625rem;
}
.filter-item {
  display: inline-block;
  vertical-align: middle;
  margin-bottom: 0.625rem;
}
</style>

