<template>
<div class="filter-card-luxury">
    <el-form :inline="true" class="luxury-inline-form">
      <el-form-item :label="$t('order.filters.searchTitle')">
        <el-input
          v-model="localQuery.orderNo"
          :placeholder="$t('order.filters.searchPlaceholderAll')"
          clearable
          class="luxury-input"
            style="width: 200px;"
          @keyup.enter="onFilter"
        />
      </el-form-item>
      <el-form-item :label="$t('order.filters.dateRange')">
        <div class="date-picker-group">
          <el-date-picker
            v-model="localQuery.startDate"
            type="date"
            placeholder="Start Date"
            value-format="YYYY-MM-DD"
            class="luxury-date-picker-single"
            style="width: 120px;"
            @change="onFilter"
          />
          <span class="date-separator">~</span>
          <el-date-picker
            v-model="localQuery.endDate"
            type="date"
            placeholder="End Date"
            value-format="YYYY-MM-DD"
            class="luxury-date-picker-single"
            style="width: 120px;"
            @change="onFilter"
          />
        </div>
      </el-form-item>

      <el-form-item label="일반분류">
        <common-select
          v-model="localQuery.categoryLarge"
          group-code="PRODUCT_CATEGORY"
          placeholder="대분류"
          class="luxury-input"
          style="width: 120px;"
          @change="onFilter"
        />
      </el-form-item>

      <el-form-item label="세트분류">
        <common-select
          v-model="localQuery.setCategoryLarge"
          group-code="PRODUCT_CATEGORY"
          placeholder="대분류"
          class="luxury-input"
          style="width: 120px;"
          @change="onFilter"
        />
      </el-form-item>

      <el-form-item>
        <el-button type="primary" @click="onFilter" class="search-btn">{{ $t('order.filters.searchBtn') }}</el-button>
      </el-form-item>

      <div class="filter-checkbox-group" v-if="!localQuery.status || localQuery.status === ''">
        <el-checkbox v-model="localQuery.excludeCancelled" @change="onFilter">{{ $t('order.filters.hideCancelled') }}</el-checkbox>
        <el-checkbox v-model="localQuery.excludeCompleted" @change="onFilter">{{ $t('order.filters.hideCompleted') }}</el-checkbox>
      </div>
    </el-form>
  </div>
</template>

<script setup lang="ts">
import { reactive, watch } from 'vue';
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
}, { deep: true, flush: 'sync' });

const onFilter = () => {
  emit('filter');
};
</script>

