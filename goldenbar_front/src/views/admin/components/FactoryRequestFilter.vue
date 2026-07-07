<template>
<el-card shadow="never" class="filter-card">
    <el-form :inline="true" :model="localQuery" class="demo-form-inline">
      <el-form-item :label="$t('home.roles.factory.title')">
        <company-select
          v-model="localQuery.factoryCompanyId"
          category="MFG"
          :placeholder="$t('home.roles.factory.title')"
          style="width: 200px"
          :disabled="!isAdmin"
          @change="handleFilter"
        />
      </el-form-item>
      <el-form-item :label="$t('home.roles.logistics.title')">
        <company-select
          v-model="localQuery.logisticsCompanyId"
          category="DCC"
          :placeholder="$t('home.roles.logistics.title')"
          style="width: 200px"
          @change="handleFilter"
        />
      </el-form-item>
      <el-form-item :label="$t('home.roles.store.title')">
        <company-select
          v-model="localQuery.companyId"
          category="RTL"
          :placeholder="$t('home.roles.store.title')"
          style="width: 200px"
          @change="handleFilter"
        />
      </el-form-item>

      <el-form-item :label="$t('productMarket.labels.generalCategories')">
        <common-select
          v-model="localQuery.categoryLarge"
          group-code="PRODUCT_CATEGORY"
          :placeholder="$t('productMarket.labels.largeCategory')"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="handleLargeChange"
        />
        <common-select
          v-if="localQuery.categoryLarge"
          v-model="localQuery.categoryMedium"
          :parent-id="largeId"
          :placeholder="$t('productMarket.labels.mediumCategory')"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="handleMediumChange"
        />
        <common-select
          v-if="localQuery.categoryMedium"
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
          @change="handleSetLargeChange"
        />
        <common-select
          v-if="localQuery.setCategoryLarge"
          v-model="localQuery.setCategoryMedium"
          :parent-id="setLargeId"
          :placeholder="$t('productMarket.labels.mediumCategory')"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="handleSetMediumChange"
        />
        <common-select
          v-if="localQuery.setCategoryMedium"
          v-model="localQuery.setCategorySmall"
          :parent-id="setMediumId"
          :placeholder="$t('productMarket.labels.smallCategory')"
          style="width: 120px;"
          @change="handleFilter"
        />
      </el-form-item>

      <el-form-item :label="$t('order.filters.orderNo')">
        <el-input
          v-model="localQuery.orderNo"
          :placeholder="$t('order.filters.orderNo')"
          clearable
          @keyup.enter="handleFilter"
        />
      </el-form-item>
      <el-form-item :label="$t('register.name')">
        <el-input
          v-model="localQuery.userName"
          :placeholder="$t('order.filters.searchPlaceholderAll')"
          clearable
          @keyup.enter="handleFilter"
        />
      </el-form-item>
      <el-form-item :label="$t('dashboard.factory.table.status')">
        <el-select
          v-model="localQuery.status"
          :placeholder="$t('order.tabs.all')"
          clearable
          @change="handleFilter"
          style="width: 140px;"
        >
          <el-option
            v-for="item in orderStatusCodes"
            :key="item.code"
            :label="item.name"
            :value="item.code"
          />
        </el-select>
      </el-form-item>
      <el-form-item :label="$t('order.filters.dateRange')">
        <div class="date-range-inputs">
          <el-date-picker
            v-model="startDate"
            type="date"
            placeholder="시작일"
            value-format="YYYY-MM-DD"
            @change="handleDateChange"
            style="width: 130px;"
          />
          <span class="range-separator">-</span>
          <el-date-picker
            v-model="endDate"
            type="date"
            placeholder="종료일"
            value-format="YYYY-MM-DD"
            @change="handleDateChange"
            style="width: 130px;"
          />
        </div>
      </el-form-item>
      <el-form-item>
        <el-checkbox v-model="localQuery.excludeCancelled" @change="handleFilter">{{ $t('order.filters.hideCancelled') }}</el-checkbox>
        <el-checkbox v-model="localQuery.excludeCompleted" @change="handleFilter">{{ $t('order.filters.hideCompleted') }}</el-checkbox>
        <el-checkbox v-model="localQuery.isAsOnly" @change="handleFilter" style="margin-left: 0.75rem;">
          <span style="color: #F56C6C; font-weight: 600;">AS의뢰 건만</span>
        </el-checkbox>
      </el-form-item>
      <el-form-item>
        <el-button type="primary" :icon="Search" @click="handleFilter">{{ $t('common.search') }}</el-button>
        <el-button :icon="Refresh" @click="handleReset">{{ $t('common.reset') }}</el-button>
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
  listQuery: any;
  dateRange: string[];
  orderStatusCodes: any[];
  isAdmin: boolean;
}>();

const emit = defineEmits(['filter', 'reset', 'date-change', 'update:listQuery']);

const localQuery = reactive({ ...props.listQuery });

watch(() => props.listQuery, (newVal) => {
  Object.assign(localQuery, newVal);
}, { deep: true });

watch(localQuery, (newVal) => {
  emit('update:listQuery', newVal);
}, { deep: true });

const startDate = ref(props.dateRange?.[0] || '');
const endDate = ref(props.dateRange?.[1] || '');

watch(() => props.dateRange, (newVal) => {
  startDate.value = newVal?.[0] || '';
  endDate.value = newVal?.[1] || '';
}, { deep: true });

const largeId = ref<number | null>(null);
const mediumId = ref<number | null>(null);
const setLargeId = ref<number | null>(null);
const setMediumId = ref<number | null>(null);

const handleLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  largeId.value = selected ? selected.id : null;
  localQuery.categoryMedium = undefined;
  localQuery.categorySmall = undefined;
  mediumId.value = null;
  handleFilter();
};

const handleMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  mediumId.value = selected ? selected.id : null;
  localQuery.categorySmall = undefined;
  handleFilter();
};

const handleSetLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  setLargeId.value = selected ? selected.id : null;
  localQuery.setCategoryMedium = undefined;
  localQuery.setCategorySmall = undefined;
  setMediumId.value = null;
  handleFilter();
};

const handleSetMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  setMediumId.value = selected ? selected.id : null;
  localQuery.setCategorySmall = undefined;
  handleFilter();
};

const handleFilter = () => {
  emit('filter');
};

const handleReset = () => {
  largeId.value = null;
  mediumId.value = null;
  setLargeId.value = null;
  setMediumId.value = null;
  emit('reset');
};

const handleDateChange = () => {
  emit('date-change', [startDate.value, endDate.value]);
};
</script>

<style scoped>
.date-range-inputs {
  display: flex;
  align-items: center;
  gap: 8px;
}
.range-separator {
  color: #909399;
}
</style>

