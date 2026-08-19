<template>
<el-card shadow="never" class="filter-card">
    <el-form :inline="true" :model="localQuery" class="demo-form-inline">
      <el-form-item v-if="isMfg" label="물류">
        <el-select v-model="localQuery.logisticsCompanyId" placeholder="전체" clearable filterable style="width: 200px" @change="handleFilter">
          <el-option v-for="c in linkedCompanies" :key="c.companyId" :label="c.companyName" :value="c.companyId" />
        </el-select>
      </el-form-item>
      <el-form-item v-else-if="isDcc" label="소매">
        <company-select v-model="localQuery.companyId" category="RTL" placeholder="전체" style="width: 200px" @change="handleFilter" />
      </el-form-item>
      <el-form-item v-else :label="$t('sys.company.labels.name')">
        <company-select v-model="localQuery.companyId" :placeholder="$t('marketplace.filters.search')" style="width: 200px" @change="handleFilter" />
      </el-form-item>
      <el-form-item label="조회기간">
        <el-date-picker
          v-model="dateRange"
          type="daterange"
          range-separator="~"
          start-placeholder="시작일"
          end-placeholder="종료일"
          value-format="YYYY-MM-DD"
          style="width: 260px"
          @change="handleDateChange"
        />
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

      <el-form-item>
        <el-button type="primary" :icon="Search" @click="handleFilter">{{ $t('common.search') }}</el-button>
        <el-button :icon="Refresh" @click="resetQuery">{{ $t('common.reset') }}</el-button>
      </el-form-item>
    </el-form>
  </el-card>
</template>

<script setup lang="ts">
import { ref, reactive, watch, computed, onMounted } from 'vue';
import { Search, Refresh } from '@element-plus/icons-vue';
import CompanySelect from '@/components/CompanySelect/index.vue';
import { getCompanySummaries } from '@/api/payable';
import useUserStore from '@/store/modules/user';

const props = defineProps<{
  query: any;
}>();

const userStore = useUserStore();
const isMfg = computed(() => userStore.companyType === 'MFG');
const isDcc = computed(() => userStore.companyType === 'DCC');

// Only companies this manufacturer/logistics center has an actual settlement
// relationship with (via existing Payable charges), instead of every company
// in the system regardless of category or relevance.
const linkedCompanies = ref<any[]>([]);

const fetchLinkedCompanies = async () => {
  if (!isMfg.value && !isDcc.value) return;
  try {
    const res: any = await getCompanySummaries({ page: 1, pageSize: 1000 });
    linkedCompanies.value = res.data.items || [];
  } catch (error) {
    console.error('Failed to fetch linked companies:', error);
  }
};

onMounted(() => {
  fetchLinkedCompanies();
});

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
}, { deep: true, flush: 'sync' });

const dateRange = ref<string[] | null>(localQuery.startDate ? [localQuery.startDate, localQuery.endDate] : null);

watch(() => props.query, () => {
  dateRange.value = localQuery.startDate ? [localQuery.startDate, localQuery.endDate] : null;
});

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
  dateRange.value = null;
  emit('reset');
};
</script>

