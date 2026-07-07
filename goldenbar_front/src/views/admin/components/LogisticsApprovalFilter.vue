<template>
<el-card shadow="never" class="filter-card">
    <el-form :inline="true" :model="localListQuery" class="demo-form-inline">
      <el-form-item :label="$t('home.roles.logistics.title')">
        <company-select
          v-model="localListQuery.logisticsCompanyId"
          category="DCC"
          :placeholder="$t('admin.orderApproval.placeholder')"
          style="width: 200px"
          :disabled="!isAdmin"
          @change="$emit('filter')"
        />
      </el-form-item>
      <el-form-item :label="$t('sys.company.labels.name')">
        <company-select
          v-model="localListQuery.companyId"
          :placeholder="$t('marketplace.filters.search')"
          style="width: 200px"
          :logistics-company-id="localListQuery.logisticsCompanyId"
          @change="$emit('filter')"
        />
      </el-form-item>
      <el-form-item :label="$t('order.filters.orderNo')">
        <el-input v-model="localListQuery.orderNo" :placeholder="$t('order.filters.orderNo')" clearable @keyup.enter="$emit('filter')" />
      </el-form-item>
      <el-form-item :label="$t('register.name')">
        <el-input v-model="localListQuery.userName" :placeholder="$t('order.filters.searchPlaceholderAll')" clearable @keyup.enter="$emit('filter')" />
      </el-form-item>
      <el-form-item :label="$t('dashboard.factory.table.status')">
        <el-select v-model="localListQuery.status" :placeholder="$t('contact.form.placeholders.subject')" clearable @change="$emit('filter')">
          <el-option v-for="item in orderStatusCodes" :key="item.code" :label="item.name" :value="item.code" />
        </el-select>
      </el-form-item>
      <el-form-item :label="$t('order.filters.dateRange')">
        <div class="date-picker-group">
          <el-date-picker
            v-model="localListQuery.startDate"
            type="date"
            :placeholder="$t('retailerSettlement.startDate')"
            value-format="YYYY-MM-DD"
            style="width: 140px"
            @change="$emit('filter')"
          />
          <span class="date-separator">-</span>
          <el-date-picker
            v-model="localListQuery.endDate"
            type="date"
            :placeholder="$t('retailerSettlement.endDate')"
            value-format="YYYY-MM-DD"
            style="width: 140px"
            @change="$emit('filter')"
          />
        </div>
      </el-form-item>

      <el-form-item :label="$t('productMarket.labels.generalCategories')">
        <common-select
          v-model="localListQuery.categoryLarge"
          group-code="PRODUCT_CATEGORY"
          :placeholder="$t('productMarket.labels.largeCategory')"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => handleLargeChange(val, options)"
        />
        <common-select
          v-if="localListQuery.categoryLarge"
          v-model="localListQuery.categoryMedium"
          :parent-id="largeId"
          :placeholder="$t('productMarket.labels.mediumCategory')"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => handleMediumChange(val, options)"
        />
        <common-select
          v-if="localListQuery.categoryMedium"
          v-model="localListQuery.categorySmall"
          :parent-id="mediumId"
          :placeholder="$t('productMarket.labels.smallCategory')"
          style="width: 120px;"
          @change="$emit('filter')"
        />
      </el-form-item>

      <el-form-item :label="$t('productMarket.labels.setCategories')">
        <common-select
          v-model="localListQuery.setCategoryLarge"
          group-code="PRODUCT_CATEGORY"
          :placeholder="$t('productMarket.labels.largeCategory')"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => handleSetLargeChange(val, options)"
        />
        <common-select
          v-if="localListQuery.setCategoryLarge"
          v-model="localListQuery.setCategoryMedium"
          :parent-id="setLargeId"
          :placeholder="$t('productMarket.labels.mediumCategory')"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => handleSetMediumChange(val, options)"
        />
        <common-select
          v-if="localListQuery.setCategoryMedium"
          v-model="localListQuery.setCategorySmall"
          :parent-id="setMediumId"
          :placeholder="$t('productMarket.labels.smallCategory')"
          style="width: 120px;"
          @change="$emit('filter')"
        />
      </el-form-item>

      <el-form-item class="checkbox-filter-item">
        <el-checkbox v-model="localListQuery.excludeCancelled" @change="$emit('filter')">{{ $t('order.filters.hideCancelled') }}</el-checkbox>
        <el-checkbox v-model="localListQuery.excludeCompleted" @change="$emit('filter')">{{ $t('order.filters.hideCompleted') }}</el-checkbox>
        <el-checkbox v-model="localListQuery.isAsOnly" @change="$emit('filter')" class="as-only-checkbox">
          <span class="as-label">AS주문건만</span>
        </el-checkbox>
      </el-form-item>
      <el-form-item>
        <el-button type="primary" :icon="Search" @click="$emit('filter')">{{ $t('common.search') }}</el-button>
        <el-button :icon="Refresh" @click="$emit('reset')">{{ $t('common.reset') }}</el-button>
      </el-form-item>
    </el-form>
  </el-card>
</template>

<script setup lang="ts">
import { reactive, watch } from 'vue';
import { Search, Refresh } from '@element-plus/icons-vue';
import CompanySelect from '@/components/CompanySelect/index.vue';
import CommonSelect from '@/components/CommonSelect/index.vue';

const props = defineProps({
  listQuery: {
    type: Object,
    required: true
  },
  orderStatusCodes: {
    type: Array,
    required: true
  },
  isAdmin: {
    type: Boolean,
    required: true
  },
  largeId: {
    type: [Number, null] as any,
    required: true
  },
  mediumId: {
    type: [Number, null] as any,
    required: true
  },
  setLargeId: {
    type: [Number, null] as any,
    required: true
  },
  setMediumId: {
    type: [Number, null] as any,
    required: true
  }
});

const emit = defineEmits([
  'filter',
  'reset',
  'update:largeId',
  'update:mediumId',
  'update:setLargeId',
  'update:setMediumId',
  'update:listQuery'
]);

const localListQuery = reactive({ ...props.listQuery });

watch(() => props.listQuery, (newVal) => {
  Object.assign(localListQuery, newVal);
}, { deep: true });

watch(localListQuery, (newVal) => {
  emit('update:listQuery', newVal);
}, { deep: true });

const handleLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  emit('update:largeId', selected ? selected.id : null);
  localListQuery.categoryMedium = '';
  localListQuery.categorySmall = '';
  emit('update:mediumId', null);
  emit('filter');
};

const handleMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  emit('update:mediumId', selected ? selected.id : null);
  localListQuery.categorySmall = '';
  emit('filter');
};

const handleSetLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  emit('update:setLargeId', selected ? selected.id : null);
  localListQuery.setCategoryMedium = '';
  localListQuery.setCategorySmall = '';
  emit('update:setMediumId', null);
  emit('filter');
};

const handleSetMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  emit('update:setMediumId', selected ? selected.id : null);
  localListQuery.setCategorySmall = '';
  emit('filter');
};
</script>

