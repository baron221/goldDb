<template>
  <div class="company-search-container">
    <el-input
      v-model="selectedName"
      :placeholder="placeholder"
      readonly
      :disabled="disabled"
      :clearable="clearable"
      class="company-input"
      @clear="handleClear"
    >
      <template #append>
        <el-button :icon="Search" @click="openDialog" :disabled="disabled" />
      </template>
    </el-input>

    <base-popup
      v-model="dialogVisible"
      title="업체 검색"
      width="800px"
      append-to-body
      draggable
    >
      <div class="filter-container" style="margin-bottom: 1.25rem;">
        <el-row :gutter="10">
          <el-col :span="8">
            <el-input
              v-model="listQuery.name"
              placeholder="상호명 검색"
              clearable
              @keyup.enter="handleFilter"
            />
          </el-col>
          <el-col :span="6">
            <common-select
              v-model="listQuery.category"
              group-code="COMPANY_TYPE"
              placeholder="업체구분 전체"
              show-all
              @change="handleFilter"
            />
          </el-col>
          <el-col :span="6">
            <common-select
              v-model="listQuery.region"
              group-code="REGION"
              placeholder="지역 전체"
              show-all
              @change="handleFilter"
            />
          </el-col>
          <el-col :span="4">
            <el-button type="primary" :icon="Search" @click="handleFilter" style="width: 100%;">검색</el-button>
          </el-col>
        </el-row>
      </div>

      <base-table
        v-loading="loading"
        :data="companyList"
        style="width: 100%"
        height="400px"
        highlight-current-row
        :total="total"
        v-model:page="listQuery.page"
        v-model:page-size="listQuery.pageSize"
        @change="fetchCompanies"
        @row-dblclick="handleSelect"
      >
        <el-table-column prop="name" label="상호명" min-width="150" />
        <el-table-column prop="ceo" label="대표자" width="100" />
        <el-table-column
          prop="category"
          label="구분"
          width="100"
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
          label="지역"
          width="100"
          align="center"
          :excel-formatter="(row) => getRegionName(row.region)"
        >
          <template #default="scope">
            {{ getRegionName(scope.row.region) }}
          </template>
        </el-table-column>
        <el-table-column
          label="직영"
          width="60"
          align="center"
          :excel-formatter="(row) => row.isDirectManagement ? '직영' : ''"
        >
          <template #default="scope">
            <el-tag v-if="scope.row.isDirectManagement" type="danger" size="small">직영</el-tag>
          </template>
        </el-table-column>
        <el-table-column align="center" label="선택" width="80" :fixed="!isMobile ? 'right' : false">
          <template #default="scope">
            <el-button type="primary" size="small" @click="handleSelect(scope.row)">선택</el-button>
          </template>
        </el-table-column>
      </base-table>
    </base-popup>
  </div>
</template>

<script setup>
import { useMobile } from '@/hooks/useMobile';
import { ref, reactive, watch, onMounted } from 'vue';
import { getCompanies, getCompany } from '@/api/company';
import useCodeStore from '@/store/modules/code';
import { Search } from '@element-plus/icons-vue';
import BaseTable from '@/components/BaseTable/index.vue';
import BasePopup from '@/components/BasePopup/index.vue';
const { isMobile } = useMobile();

const props = defineProps({
  modelValue: [Number, String],
  placeholder: {
    type: String,
    default: '업체를 선택해 주세요'
  },
  disabled: {
    type: Boolean,
    default: false
  },
  clearable: {
    type: Boolean,
    default: true
  },
  initialName: {
    type: String,
    default: ''
  },
  category: {
    type: String,
    default: ''
  }
});

const emit = defineEmits(['update:modelValue', 'change']);

const codeStore = useCodeStore();
const selectedName = ref(props.initialName);
const dialogVisible = ref(false);
const loading = ref(false);
const companyList = ref([]);
const total = ref(0);
const companyTypeCodes = ref([]);
const regionCodes = ref([]);

const listQuery = reactive({
  name: '',
  category: props.category || null,
  region: null,
  page: 1,
  pageSize: 10
});

watch(() => props.modelValue, (newId) => {
  if (!newId) {
    selectedName.value = '';
  } else if (newId !== internalId.value) {
    fetchCompanyName(newId);
  }
});

const internalId = ref(props.modelValue);

onMounted(() => {
  if (props.modelValue && !props.initialName) {
    fetchCompanyName(props.modelValue);
  }
  fetchCodes();
});

const fetchCodes = async () => {
  try {
    await codeStore.fetchCodes();
    companyTypeCodes.value = codeStore.getCodesByGroupStore('COMPANY_TYPE');
    regionCodes.value = codeStore.getCodesByGroupStore('REGION');
  } catch (error) {
    console.error('Failed to fetch codes:', error);
  }
};

const getCategoryName = (code) => {
  const codeObj = companyTypeCodes.value.find(c => c.code === code);
  return codeObj ? codeObj.name : code;
};

const getRegionName = (code) => {
  const codeObj = regionCodes.value.find(c => c.code === code);
  return codeObj ? codeObj.name : code;
};

const getCategoryType = (code) => {
  switch (code) {
    case 'MFG': return 'success';
    case 'DCC': return 'warning';
    case 'RTL': return 'info';
    default: return 'info';
  }
};

const fetchCompanyName = async (id) => {
  try {
    const res = await getCompany(id);
    if (res.data) {
      selectedName.value = res.data.name;
      internalId.value = id;
    }
  } catch (error) {
    console.error('Failed to fetch company name:', error);
  }
};

const openDialog = () => {
  dialogVisible.value = true;
  fetchCompanies();
};

const fetchCompanies = async () => {
  loading.value = true;
  try {
    const res = await getCompanies(listQuery);
    companyList.value = res.data.items;
    total.value = res.data.total;
  } catch (error) {
    console.error('Failed to fetch companies:', error);
  } finally {
    loading.value = false;
  }
};

const handleFilter = () => {
  listQuery.page = 1;
  fetchCompanies();
};

const handleClear = () => {
  selectedName.value = '';
  internalId.value = null;
  emit('update:modelValue', null);
  emit('change', null, null);
};

const handleSelect = (row) => {
  selectedName.value = row.name;
  internalId.value = row.id;
  emit('update:modelValue', row.id);
  emit('change', row.id, row);
  dialogVisible.value = false;
};
</script>

<style scoped>
.company-search-container {
  width: 100%;
}
.company-input :deep(.el-input__wrapper) {
  cursor: pointer;
}
.company-input :deep(.el-input__inner) {
  cursor: pointer;
}
</style>
