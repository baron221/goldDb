<template>
<div class="filter-container">
    <el-form :inline="true" :model="localQuery" class="demo-form-inline">
      <el-form-item label="세트 제목">
        <el-input
          v-model="localQuery.title"
          placeholder="세트 제목 입력"
          style="width: 200px;"
          @keyup.enter="handleFilter"
        />
      </el-form-item>
      <el-form-item label="대분류">
        <common-select
          v-model="localQuery.categoryLarge"
          group-code="SET_PRODUCT_CATEGORY"
          placeholder="대분류 선택"
          clearable
          @change="handleFilter"
        />
      </el-form-item>
      <el-form-item label="제조사">
        <company-select
          v-model="localQuery.companyId"
          placeholder="제조사 선택"
          :disabled="isCompanyUser"
          clearable
          @change="handleFilter"
        />
      </el-form-item>
      <el-form-item>
        <el-button type="primary" :icon="View" @click="handleFilter">검색</el-button>
        <el-button @click="handleReset">초기화</el-button>
        <el-button type="primary" :icon="Plus" @click="handleCreate">새 세트 등록</el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<script setup lang="ts">
import { reactive, computed, watch } from 'vue';
import { View, Plus } from '@element-plus/icons-vue';
import CommonSelect from '@/components/CommonSelect/index.vue';
import CompanySelect from '@/components/CompanySelect/index.vue';
import useUserStore from '@/store/modules/user';

const props = defineProps<{
  listQuery: {
    title: string;
    categoryLarge: string;
    categoryMedium: string;
    categorySmall: string;
    companyId: number | undefined;
    isPublic: boolean | undefined;
  };
}>();

const emit = defineEmits(['filter', 'reset', 'create', 'update:listQuery']);

const localQuery = reactive({ ...props.listQuery });

watch(() => props.listQuery, (newVal) => {
  Object.assign(localQuery, newVal);
}, { deep: true });

watch(localQuery, (newVal) => {
  emit('update:listQuery', newVal);
}, { deep: true, flush: 'sync' });

const userStore = useUserStore();
const isCompanyUser = computed(() => !userStore.roles.includes('admin'));

const handleFilter = () => {
  emit('filter');
};

const handleReset = () => {
  emit('reset');
};

const handleCreate = () => {
  emit('create');
};
</script>

