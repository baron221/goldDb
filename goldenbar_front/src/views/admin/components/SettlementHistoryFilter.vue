<template>
  <el-card shadow="never" class="filter-card">
    <el-form :inline="!isMobile" :label-position="isMobile ? 'top' : 'right'" :model="localQuery" class="demo-form-inline">
      <el-form-item :label="companyLabel || $t('admin.settlement.filters.company')">
        <company-select
          v-model="localQuery.companyId"
          :placeholder="$t('admin.settlement.filters.companyPlaceholder')"
          :category="companyCategory"
          :only-partners="!companyCategory"
          style="width: 200px"
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
      <el-form-item label="상품명">
        <el-input v-model="localQuery.productName" placeholder="상품명" clearable @keyup.enter="handleFilter" />
      </el-form-item>



      <el-form-item>
        <el-button type="primary" :icon="Search" @click="handleFilter">{{ $t('common.search') }}</el-button>
        <el-button :icon="Refresh" @click="resetQuery">{{ $t('common.reset') }}</el-button>
        <el-button v-if="showCombinedPrint" type="success" :icon="Printer" @click="$emit('print-combined')">일괄 거래명세서</el-button>
      </el-form-item>
    </el-form>
  </el-card>
</template>

<script setup lang="ts">
import { ref, watch, reactive, computed } from 'vue';
import { Search, Refresh, Printer } from '@element-plus/icons-vue';
import CompanySelect from '@/components/CompanySelect/index.vue';
import CommonSelect from '@/components/CommonSelect/index.vue';

const props = withDefaults(defineProps<{
  query: any;
  isMobile: boolean;
  showCombinedPrint?: boolean;
  companyCategory?: string;
  companyLabel?: string;
}>(), {
  showCombinedPrint: true
});

const companyCategory = computed(() => props.companyCategory || '');
const companyLabel = computed(() => props.companyLabel || '');

const showCombinedPrint = computed(() => props.showCombinedPrint !== false);

const emit = defineEmits<{
  (_e: 'filter'): void;
  (_e: 'reset'): void;
  (_e: 'print-combined'): void;
  (_e: 'update:query', value: any): void;
}>();

const localQuery = reactive({ ...props.query });

watch(() => props.query, (newVal) => {
  Object.assign(localQuery, newVal);
}, { deep: true });

watch(localQuery, (newVal) => {
  emit('update:query', newVal);
}, { deep: true, flush: 'sync' });

const largeId = ref<number | null>(null);
const mediumId = ref<number | null>(null);
const setLargeId = ref<number | null>(null);
const setMediumId = ref<number | null>(null);

const dateRange = ref<string[]>([localQuery.startDate, localQuery.endDate]);

watch(() => localQuery.startDate, (val) => {
  if (!val) {
    dateRange.value = [];
  } else {
    dateRange.value = [localQuery.startDate, localQuery.endDate];
  }
});

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

const handleDateChange = (val: string[] | null) => {
  if (val && val.length === 2) {
    localQuery.startDate = val[0];
    localQuery.endDate = val[1];
  } else {
    localQuery.startDate = undefined;
    localQuery.endDate = undefined;
  }
  handleFilter();
};

const handleFilter = () => {
  emit('filter');
};

const resetQuery = () => {
  emit('reset');
};
</script>

<style scoped lang="scss">
.filter-card {
  .demo-form-inline {
    .el-form-item {
      margin-right: 16px;
      margin-bottom: 12px;
    }
  }
}
</style>
