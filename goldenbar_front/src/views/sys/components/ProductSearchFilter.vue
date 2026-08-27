<template>
<div class="filter-container" style="margin-bottom: 1.25rem;">
    <el-form :inline="true" :model="localQuery" class="demo-form-inline">
      <el-form-item :label="$t('dashboard.factory.table.productName')">
        <el-input v-model="localQuery.name" :placeholder="$t('productMarket.labels.searchPlaceholder')" clearable @keyup.enter="handleFilter" />
      </el-form-item>
      <el-form-item :label="$t('dashboard.factory.table.orderNo')">
        <el-input v-model="localQuery.productNo" :placeholder="$t('dashboard.factory.table.orderNo')" clearable @keyup.enter="handleFilter" />
      </el-form-item>
      <el-form-item :label="$t('dashboard.factory.table.weight')">
        <el-input-number v-model="localQuery.minWeight" :precision="2" :step="0.1" :placeholder="$t('stock.filter.minWeight')" style="width: 120px;" />
        <span style="margin: 0 0.3125rem;">~</span>
        <el-input-number v-model="localQuery.maxWeight" :precision="2" :step="0.1" :placeholder="$t('stock.filter.maxWeight')" style="width: 120px;" />
      </el-form-item>
      <el-form-item :label="$t('marketplace.filters.category')">
        <common-select
          v-model="localQuery.categoryLarge"
          group-code="PRODUCT_CATEGORY"
          :placeholder="$t('productMarket.labels.selectLarge')"
          style="width: 120px;"
          @change="handleFilter"
        />
      </el-form-item>
      <el-form-item :label="$t('stock.labels.manufacturer')">
        <company-select v-model="localQuery.companyId" category="MFG" :disabled="isCompanyUser" />
      </el-form-item>
      <el-form-item :label="$t('productDetail.labels.purity')">
        <el-select v-model="localQuery.purity" placeholder="전체" clearable style="width: 120px;" @change="handleFilter">
          <el-option label="14K" value="14K" />
          <el-option label="18K" value="18K" />
          <el-option label="24K" value="24K" />
          <el-option label="PT" value="PT" />
        </el-select>
      </el-form-item>
      <el-form-item :label="$t('sys.product.headers.public')">
        <el-select v-model="localQuery.isPublic" :placeholder="$t('order.tabs.all')" clearable style="width: 120px;">
          <el-option :label="$t('sys.product.headers.public')" :value="true" />
          <el-option label="비공개" :value="false" />
        </el-select>
      </el-form-item>
      <el-form-item>
        <el-button type="primary" @click="handleFilter">{{ $t('common.search') }}</el-button>
        <el-button @click="resetQuery">{{ $t('common.reset') }}</el-button>
        <el-button v-if="!isCompanyUser || userStore.companyType === 'MFG'" size="small" type="primary" :icon="Plus" @click="handleCreate">{{ $t('common.create') }}</el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<script setup lang="ts">
import { reactive, watch } from 'vue';
import { Plus } from '@element-plus/icons-vue';
import CommonSelect from '@/components/CommonSelect/index.vue';
import CompanySelect from '@/components/CompanySelect/index.vue';
import useUserStore from '@/store/modules/user';

const userStore = useUserStore();

const props = defineProps<{
  query: any;
  isCompanyUser: boolean;
}>();

const emit = defineEmits<{(_e: 'filter'): void;
  (_e: 'reset'): void;
  (_e: 'create'): void;
  (_e: 'update:query', value: any): void;
}>();

const localQuery = reactive({ ...props.query });

watch(() => props.query, (newVal) => {
  Object.assign(localQuery, newVal);
}, { deep: true });

watch(localQuery, (newVal) => {
  emit('update:query', newVal);
}, { deep: true, flush: 'sync' });

const handleFilter = () => {
  emit('filter');
};

const resetQuery = () => {
  emit('reset');
};

const handleCreate = () => {
  emit('create');
};
</script>

