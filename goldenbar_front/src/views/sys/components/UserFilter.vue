<template>
<div class="user-filter-container">
    <div class="filter-row">
      <common-select
        v-model="localQuery.companyType"
        group-code="COMPANY_TYPE"
        show-all
        :placeholder="$t('userManage.companyTypeAll')"
        style="width: 140px;"
        @change="onFilter"
        @options-loaded="onCompanyTypesLoaded"
      />
      <el-input
        v-model="localQuery.searchText"
        :placeholder="$t('userManage.searchPlaceholder')"
        clearable
        style="flex: 1;"
        @keyup.enter="onFilter"
        @clear="onFilter"
      >
        <template #prefix>
          <el-icon><Search /></el-icon>
        </template>
      </el-input>
      <el-button type="primary" :icon="Search" @click="onFilter">{{ $t('common.search') }}</el-button>
    </div>
    <div class="filter-options-row">
      <el-checkbox v-model="localQuery.isUnassignedOnly" @change="onFilter">
        {{ $t('userManage.unassignedOnly') }}
      </el-checkbox>
      <el-checkbox v-model="localQuery.isLogisticsUnassigned" @change="onFilter">
        물류 미지정 (소매)
      </el-checkbox>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, watch } from 'vue';
import { Search } from '@element-plus/icons-vue';
import CommonSelect from '@/components/CommonSelect/index.vue';

const props = defineProps<{
  query: any;
}>();

const emit = defineEmits(['filter', 'update:query']);

const localQuery = reactive({ ...props.query });

watch(() => props.query, (newVal) => {
  Object.assign(localQuery, newVal);
}, { deep: true });

watch(localQuery, (newVal) => {
  emit('update:query', newVal);
}, { deep: true });

const onCompanyTypesLoaded = (options: any[]) => {

  if (options && options.length > 0 && !localQuery.companyType) {
    localQuery.companyType = options[0].code;
    onFilter();
  }
};

const onFilter = () => {
  emit('filter');
};
</script>

<style scoped>
.user-filter-container {
  display: flex;
  flex-direction: column;
  gap: 0.625rem;
}
.filter-row {
  display: flex;
  gap: 0.625rem;
}
.filter-options-row {
  display: flex;
  gap: 1rem;
  align-items: center;
}
</style>

