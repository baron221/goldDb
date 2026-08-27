<template>
<el-card shadow="never" class="filter-card">
    <el-form :inline="true" :model="localQuery" class="demo-form-inline">
      <el-form-item :label="$t('admin.settlement.filters.company')">
        <company-select v-model="localQuery.companyId" :placeholder="$t('admin.settlement.filters.companyPlaceholder')" style="width: 200px" @change="handleFilter" />
      </el-form-item>
      <el-form-item :label="$t('order.filters.orderNo')">
        <el-input v-model="localQuery.orderNo" :placeholder="$t('order.filters.orderNo')" clearable @keyup.enter="handleFilter" />
      </el-form-item>
      <el-form-item :label="$t('order.list.orderer')">
        <el-input v-model="localQuery.userName" :placeholder="$t('order.filters.searchPlaceholder')" clearable @keyup.enter="handleFilter" />
      </el-form-item>

      <el-form-item :label="$t('admin.settlement.filters.generalCategory')">
        <common-select
          v-model="localQuery.categoryLarge"
          group-code="PRODUCT_CATEGORY"
          :placeholder="$t('productMarket.labels.selectLarge')"
          style="width: 120px;"
          @change="handleFilter"
        />
      </el-form-item>

      <el-form-item :label="$t('admin.settlement.filters.setCategory')">
        <common-select
          v-model="localQuery.setCategoryLarge"
          group-code="PRODUCT_CATEGORY"
          :placeholder="$t('productMarket.labels.selectLarge')"
          style="width: 120px;"
          @change="handleFilter"
        />
      </el-form-item>

      <el-form-item :label="$t('admin.settlement.filters.period')">
        <el-date-picker
          v-model="dateRange"
          type="daterange"
          range-separator="-"
          :start-placeholder="$t('admin.settlement.filters.startDate')"
          :end-placeholder="$t('admin.settlement.filters.endDate')"
          value-format="YYYY-MM-DD"
          @change="handleDateChange"
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
import { ref, watch, reactive } from 'vue';
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

const dateRange = ref<string[]>([localQuery.startDate, localQuery.endDate]);

watch(() => localQuery.startDate, (val) => {
  if (!val) {
    dateRange.value = [];
  } else {
    dateRange.value = [localQuery.startDate, localQuery.endDate];
  }
});

const handleDateChange = (val: string[] | null) => {
  if (val) {
    localQuery.startDate = val[0];
    localQuery.endDate = val[1];
  } else {
    localQuery.startDate = undefined;
    localQuery.endDate = undefined;
  }
  handleFilter();
};

const handleFilter = () => {
  emit('update:query', localQuery);
  emit('filter');
};

const resetQuery = () => {
  dateRange.value = [];
  emit('reset');
};
</script>

