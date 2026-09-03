<template>
<div class="user-filter-container">
    <div class="filter-row">
      <el-select
        v-model="localQuery.companyType"
        clearable
        :placeholder="$t('userManage.companyTypeAll')"
        style="width: 140px;"
        @change="onFilter"
      >
        <el-option :label="$t('userManage.admin')" value="ADMIN" />
        <el-option :label="$t('userManage.manufacturer')" value="MFG" />
        <el-option :label="$t('userManage.logistics')" value="DCC" />
        <el-option :label="$t('userManage.retailer')" value="RTL" />
      </el-select>
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
        {{ $t('userManage.logisticsUnassigned') }}
      </el-checkbox>
      <el-checkbox v-model="localQuery.isPendingApprovalOnly" @change="onFilter">
        {{ $t('userManage.pendingApprovalOnly') }}
      </el-checkbox>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, watch } from 'vue';
import { Search } from '@element-plus/icons-vue';

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

const onFilter = () => {
  emit('update:query', localQuery);
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

