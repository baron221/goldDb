<template>
<el-card shadow="never" class="list-card">
    <template #header>
      <div class="card-header">
        <div class="section-header">
          <h2 class="section-title">{{ $t('sys.company.title') }}</h2>
        </div>
        <el-button type="primary" size="small" :icon="Plus" @click="handleCreate">{{ $t('common.create') }}</el-button>
      </div>
    </template>

    <div class="filter-container" style="margin-bottom: 0.9375rem;">
      <el-input
        v-model="localQuery.name"
        :placeholder="$t('sys.company.labels.name')"
        style="width: 100%; margin-bottom: 0.625rem;"
        clearable
        @keyup.enter="handleFilter"
      />
      <div v-if="isMobile" style="display: flex; gap: 0.5rem;">
        <common-select v-model="localQuery.category" group-code="COMPANY_TYPE" :placeholder="$t('sys.company.labels.type')" show-all style="flex: 1;" @change="handleFilter" />
        <common-select v-model="localQuery.region" group-code="REGION" :placeholder="$t('sys.company.labels.region')" show-all style="flex: 1;" @change="handleFilter" />
      </div>
      <template v-else>
        <el-row :gutter="10">
          <el-col :xs="24" :sm="12">
            <common-select
              v-model="localQuery.category"
              group-code="COMPANY_TYPE"
              :placeholder="$t('sys.company.labels.type')"
              show-all
              @change="handleFilter"
            />
          </el-col>
          <el-col :xs="24" :sm="12">
            <common-select
              v-model="localQuery.region"
              group-code="REGION"
              :placeholder="$t('sys.company.labels.region')"
              show-all
              @change="handleFilter"
            />
          </el-col>
        </el-row>
        <el-row :gutter="10" style="margin-top: 0.625rem;">
          <el-col :xs="24" :sm="12">
            <el-checkbox v-model="localQuery.isDirectManagement" :label="$t('sys.company.directOnlyFilter')" @change="handleFilter" />
          </el-col>
        </el-row>
      </template>
      <el-button type="primary" style="width: 100%; margin-top: 0.625rem;" @click="handleFilter">{{ $t('common.search') }}</el-button>
    </div>

    <base-table
      :data="companyList"
      v-model:page="localQuery.page"
      v-model:page-size="localQuery.pageSize"
      :total="total"
      style="width: 100%"
      border
      highlight-current-row
      @row-click="handleRowClick"
      @change="handleChange"
    >
      <el-table-column prop="name" :label="$t('sys.company.labels.name')" :sortable="!isMobile" />
      <el-table-column v-if="!isMobile" prop="ceo" :label="$t('sys.company.labels.ceo')" width="80" />
      <el-table-column
        prop="category"
        :label="$t('sys.company.labels.type')"
        :width="isMobile ? 70 : 60"
        align="center"
        :excel-formatter="(row) => getCategoryName(row.category)"
      >
        <template #default="scope">
          <el-tag :type="getCategoryType(scope.row.category)" size="small">
            {{ getCategoryName(scope.row.category) }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column
        v-if="!isMobile"
        :label="$t('sys.company.labels.isDirect')"
        width="50"
        align="center"
        prop="isDirectManagement"
        :excel-formatter="(row) => row.isDirectManagement ? $t('sys.company.labels.isDirect') : ''"
      >
        <template #default="scope">
          <el-tag v-if="scope.row.isDirectManagement" type="danger" size="small">{{ $t('sys.company.labels.isDirect') }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column
        v-if="!isMobile"
        prop="logisticsCompanyName"
        :label="$t('admin.orderApproval.logisticsCompany')"
        min-width="100"
        :excel-formatter="(row) => row.category === 'RTL' ? (row.logisticsCompanyName || '-') : ''"
      >
        <template #default="scope">
          <span v-if="scope.row.category === 'RTL'" style="font-size: 0.95rem; color: #666;">
            {{ scope.row.logisticsCompanyName || '-' }}
          </span>
        </template>
      </el-table-column>
      <el-table-column v-if="!isMobile" align="center" :label="$t('common.action')" width="60" :fixed="!isMobile ? 'right' : false">
        <template #default="scope">
          <el-button link class="delete-icon-btn" :icon="Delete" @click.stop="handleDelete(scope.row)" />
        </template>
      </el-table-column>
    </base-table>
  </el-card>
</template>

<script setup lang="ts">

import { ref, onMounted, reactive, watch } from 'vue';
import { Plus, Delete } from '@element-plus/icons-vue';
import BaseTable from '@/components/BaseTable/index.vue';
import CommonSelect from '@/components/CommonSelect/index.vue';
import useCodeStore from '@/store/modules/code';

const props = defineProps<{
  companyList: any[];
  listQuery: any;
  total: number;
  isMobile: boolean;
}>();

const emit = defineEmits(['row-click', 'delete', 'create', 'change', 'filter', 'update:listQuery']);

const codeStore = useCodeStore();
const companyTypeCodes = ref<any[]>([]);

const localQuery = reactive({ ...props.listQuery });

watch(() => props.listQuery, (newVal) => {
  Object.assign(localQuery, newVal);
}, { deep: true });

watch(localQuery, (newVal) => {
  emit('update:listQuery', { ...newVal });
}, { deep: true });

onMounted(async () => {
  try {
    await codeStore.fetchCodes();
    companyTypeCodes.value = codeStore.getCodesByGroupStore('COMPANY_TYPE');
  } catch (error) {
    console.error('Failed to fetch company types:', error);
  }
});

const getCategoryName = (code: string) => {
  const codeObj = companyTypeCodes.value.find(c => c.code === code);
  return codeObj ? codeObj.name : code;
};

const getCategoryType = (code: string) => {
  switch (code) {
    case 'MFG': return 'success';
    case 'DCC': return 'warning';
    case 'RTL': return 'info';
    default: return 'info';
  }
};

const handleCreate = () => {
  emit('create');
};

const handleFilter = () => {
  emit('filter');
};

const handleRowClick = (row: any) => {
  emit('row-click', row);
};

const handleDelete = (row: any) => {
  emit('delete', row);
};

const handleChange = () => {
  emit('change');
};
</script>

