<template>
<el-card shadow="never" class="filter-card">
    <el-form :inline="true" :model="localQuery" class="demo-form-inline">
      <el-form-item :label="$t('order.filters.orderNo')">
        <el-input v-model="localQuery.orderNo" :placeholder="$t('order.filters.orderNo')" clearable @keyup.enter="emitFilter" />
      </el-form-item>
      <el-form-item :label="$t('order.filters.searchTitle')">
        <el-input v-model="localQuery.userName" :placeholder="$t('order.filters.searchPlaceholder')" clearable @keyup.enter="emitFilter" />
      </el-form-item>
      <el-form-item :label="$t('stock.filter.status')">
        <el-select v-model="localQuery.status" :placeholder="$t('stock.filter.status')" clearable style="width: 150px" @change="emitFilter">
          <el-option :label="$t('order.tabs.all')" value="" />
          <el-option v-for="item in orderStatusCodes" :key="item.code" :label="statusLabel(item.code)" :value="item.code" />
        </el-select>
      </el-form-item>
      <el-form-item :label="$t('order.filters.dateRange')">
        <el-date-picker
          v-model="dateRange"
          type="daterange"
          range-separator="-"
          :start-placeholder="$t('order.filters.to')"
          :end-placeholder="$t('order.filters.to')"
          value-format="YYYY-MM-DD"
          @change="handleDateChange"
        />
      </el-form-item>

      <el-form-item :label="$t('productMarket.labels.generalCategories')">
        <common-select
          v-model="localQuery.categoryLarge"
          group-code="PRODUCT_CATEGORY"
          :placeholder="$t('productMarket.labels.selectLarge')"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => handleLargeChange(val, options)"
        />
        <common-select
          v-model="localQuery.categoryMedium"
          :parent-id="largeId"
          :placeholder="$t('productMarket.labels.selectMedium')"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => handleMediumChange(val, options)"
        />
        <common-select
          v-model="localQuery.categorySmall"
          :parent-id="mediumId"
          :placeholder="$t('productMarket.labels.selectSmall')"
          style="width: 120px;"
          @change="emitFilter"
        />
      </el-form-item>

      <el-form-item :label="$t('productMarket.labels.setCategories')">
        <common-select
          v-model="localQuery.setCategoryLarge"
          group-code="PRODUCT_CATEGORY"
          :placeholder="$t('productMarket.labels.selectLarge')"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => handleSetLargeChange(val, options)"
        />
        <common-select
          v-model="localQuery.setCategoryMedium"
          :parent-id="setLargeId"
          :placeholder="$t('productMarket.labels.selectMedium')"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => handleSetMediumChange(val, options)"
        />
        <common-select
          v-model="localQuery.setCategorySmall"
          :parent-id="setMediumId"
          :placeholder="$t('productMarket.labels.selectSmall')"
          style="width: 120px;"
          @change="emitFilter"
        />
      </el-form-item>

      <el-form-item>
        <el-button type="primary" :icon="Search" @click="emitFilter">{{ $t('common.search') }}</el-button>
        <el-button :icon="Refresh" @click="emitReset">{{ $t('common.reset') }}</el-button>
        <el-button v-if="isAdmin" type="success" size="small" :icon="Printer" @click="emitPrintAll">{{ $t('stock.filter.bulkPrint') }}</el-button>
      </el-form-item>
    </el-form>
  </el-card>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, reactive, watch } from 'vue';
import { Search, Refresh, Printer } from '@element-plus/icons-vue';
import CommonSelect from '@/components/CommonSelect/index.vue';
import useCodeStore from '@/store/modules/code';
import useUserStore from '@/store/modules/user';
import { parseTime } from '@/utils';

const props = defineProps<{
  query: any;
}>();

const emit = defineEmits(['filter', 'reset', 'print-all', 'update:query']);

const localQuery = reactive({ ...props.query });

watch(() => props.query, (newVal) => {
  Object.assign(localQuery, newVal);
}, { deep: true });

watch(localQuery, (newVal) => {
  emit('update:query', newVal);
}, { deep: true });

const codeStore = useCodeStore();
const userStore = useUserStore();

const end = new Date();
const start = new Date();
start.setTime(start.getTime() - 3600 * 1000 * 24 * 30);
const defaultStartDate = parseTime(start, '{y}-{m}-{d}');
const defaultEndDate = parseTime(end, '{y}-{m}-{d}');

const dateRange = ref<string[]>([localQuery.startDate || defaultStartDate, localQuery.endDate || defaultEndDate]);
const orderStatusCodes = ref<any[]>([]);

const largeId = ref<number | null>(null);
const mediumId = ref<number | null>(null);
const setLargeId = ref<number | null>(null);
const setMediumId = ref<number | null>(null);

const isAdmin = computed(() => userStore.roles.includes('admin'));

const statusLabel = (status: string) => {
  return codeStore.getCodeName(status);
};

const emitFilter = () => {
  emit('filter');
};

const handleDateChange = (val: string[] | null) => {
  if (val) {
    localQuery.startDate = val[0];
    localQuery.endDate = val[1];
  } else {
    localQuery.startDate = undefined;
    localQuery.endDate = undefined;
  }
  emitFilter();
};

const handleLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  largeId.value = selected ? selected.id : null;
  localQuery.categoryMedium = '';
  localQuery.categorySmall = '';
  mediumId.value = null;
  emitFilter();
};

const handleMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  mediumId.value = selected ? selected.id : null;
  localQuery.categorySmall = '';
  emitFilter();
};

const handleSetLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  setLargeId.value = selected ? selected.id : null;
  localQuery.setCategoryMedium = '';
  localQuery.setCategorySmall = '';
  setMediumId.value = null;
  emitFilter();
};

const handleSetMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  setMediumId.value = selected ? selected.id : null;
  localQuery.setCategorySmall = '';
  emitFilter();
};

const emitReset = () => {
  dateRange.value = [defaultStartDate, defaultEndDate];
  localQuery.startDate = defaultStartDate;
  localQuery.endDate = defaultEndDate;
  localQuery.status = '';
  localQuery.orderNo = '';
  localQuery.userName = '';
  localQuery.categoryLarge = '';
  localQuery.categoryMedium = '';
  localQuery.categorySmall = '';
  localQuery.setCategoryLarge = '';
  localQuery.setCategoryMedium = '';
  localQuery.setCategorySmall = '';

  largeId.value = null;
  mediumId.value = null;
  setLargeId.value = null;
  setMediumId.value = null;

  emit('reset');
};

const emitPrintAll = () => {
  emit('print-all');
};

const fetchOrderStatuses = async () => {
  try {
    await codeStore.fetchCodes();
    orderStatusCodes.value = codeStore.getCodesByGroupStore('ORDERED_STATUS');
  } catch (error) {
    console.error('Failed to fetch order status codes:', error);
  }
};

onMounted(() => {
  fetchOrderStatuses();
});
</script>

