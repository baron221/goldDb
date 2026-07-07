<template>
<el-card shadow="never" class="filter-card">
    <el-form :inline="true" :model="localQuery" class="demo-form-inline">
      <el-form-item :label="$t('sys.company.labels.name')">
        <company-select v-model="localQuery.companyId" :placeholder="$t('marketplace.filters.search')" style="width: 200px" @change="handleFilter" />
      </el-form-item>
      <el-form-item :label="$t('order.filters.title')">
        <el-select v-model="localQuery.status" :placeholder="$t('order.filters.searchPlaceholder')" clearable @change="handleFilter">
          <el-option :label="$t('order.status.PENDING')" value="PENDING" />
          <el-option :label="$t('order.status.PROCESSING')" value="PROCESSING" />
          <el-option :label="$t('order.status.SETTLED')" value="SETTLED" />
        </el-select>
      </el-form-item>
      <el-form-item :label="$t('order.filters.orderNo')">
        <el-input v-model="localQuery.orderNo" :placeholder="$t('order.filters.orderNo')" clearable @keyup.enter="handleFilter" />
      </el-form-item>

      <el-form-item :label="$t('productMarket.labels.generalCategories')">
        <common-select
          v-model="localQuery.categoryLarge"
          group-code="PRODUCT_CATEGORY"
          :placeholder="$t('productMarket.labels.largeCategory')"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => handleLargeChange(val, options)"
        />
        <common-select
          v-model="localQuery.categoryMedium"
          :parent-id="largeId"
          :placeholder="$t('productMarket.labels.mediumCategory')"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => handleMediumChange(val, options)"
        />
        <common-select
          v-model="localQuery.categorySmall"
          :parent-id="mediumId"
          :placeholder="$t('productMarket.labels.smallCategory')"
          style="width: 120px;"
          @change="handleFilter"
        />
      </el-form-item>

      <el-form-item :label="$t('productMarket.labels.setCategories')">
        <common-select
          v-model="localQuery.setCategoryLarge"
          group-code="PRODUCT_CATEGORY"
          :placeholder="$t('productMarket.labels.largeCategory')"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => handleSetLargeChange(val, options)"
        />
        <common-select
          v-model="localQuery.setCategoryMedium"
          :parent-id="setLargeId"
          :placeholder="$t('productMarket.labels.mediumCategory')"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => handleSetMediumChange(val, options)"
        />
        <common-select
          v-model="localQuery.setCategorySmall"
          :parent-id="setMediumId"
          :placeholder="$t('productMarket.labels.smallCategory')"
          style="width: 120px;"
          @change="handleFilter"
        />
      </el-form-item>

      <el-form-item>
        <el-button type="primary" :icon="Search" @click="handleFilter">{{ $t('common.search') }}</el-button>
        <el-button :icon="Refresh" @click="resetQuery">{{ $t('common.reset') }}</el-button>
      </el-form-item>
    </el-form>
  </el-card>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue';
import { Search, Refresh } from '@element-plus/icons-vue';
import CompanySelect from '@/components/CompanySelect/index.vue';
import CommonSelect from '@/components/CommonSelect/index.vue';

const props = defineProps<{
  query: any;
}>();

const emit = defineEmits<{(_e: 'filter'): void;
  (_e: 'reset'): void;
  (_e: 'update:query', value: any): void;
}>();

const localQuery = reactive({ ...props.query });

watch(() => props.query, (newVal) => {
  Object.assign(localQuery, newVal);
}, { deep: true });

watch(localQuery, (newVal) => {
  emit('update:query', newVal);
}, { deep: true });

const largeId = ref<number | null>(null);
const mediumId = ref<number | null>(null);
const setLargeId = ref<number | null>(null);
const setMediumId = ref<number | null>(null);

const handleLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  largeId.value = selected ? selected.id : null;
  localQuery.categoryMedium = '';
  localQuery.categorySmall = '';
  mediumId.value = null;
  handleFilter();
};

const handleMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  mediumId.value = selected ? selected.id : null;
  localQuery.categorySmall = '';
  handleFilter();
};

const handleSetLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  setLargeId.value = selected ? selected.id : null;
  localQuery.setCategoryMedium = '';
  localQuery.setCategorySmall = '';
  setMediumId.value = null;
  handleFilter();
};

const handleSetMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  setMediumId.value = selected ? selected.id : null;
  localQuery.setCategorySmall = '';
  handleFilter();
};

const handleFilter = () => {
  emit('filter');
};

const resetQuery = () => {
  largeId.value = null;
  mediumId.value = null;
  setLargeId.value = null;
  setMediumId.value = null;
  emit('reset');
};
</script>

