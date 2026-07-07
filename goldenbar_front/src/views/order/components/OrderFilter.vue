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
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => handleLargeChange(val, options)"
        />
        <common-select
          v-if="largeId !== null"
          v-model="localQuery.categoryMedium"
          :parent-id="largeId"
          placeholder="중분류"
          class="luxury-input"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => handleMediumChange(val, options)"
        />
        <common-select
          v-if="largeId !== null && mediumId !== null"
          v-model="localQuery.categorySmall"
          :parent-id="mediumId"
          placeholder="소분류"
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
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => handleSetLargeChange(val, options)"
        />
        <common-select
          v-if="setLargeId !== null"
          v-model="localQuery.setCategoryMedium"
          :parent-id="setLargeId"
          placeholder="중분류"
          class="luxury-input"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => handleSetMediumChange(val, options)"
        />
        <common-select
          v-if="setLargeId !== null && setMediumId !== null"
          v-model="localQuery.setCategorySmall"
          :parent-id="setMediumId"
          placeholder="소분류"
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
import { ref, reactive, watch } from 'vue';
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
}, { deep: true });

const largeId = ref<number | null>(null);
const mediumId = ref<number | null>(null);
const setLargeId = ref<number | null>(null);
const setMediumId = ref<number | null>(null);

const onFilter = () => {
  emit('filter');
};

const handleLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  largeId.value = selected ? selected.id : null;
  localQuery.categoryMedium = '';
  localQuery.categorySmall = '';
  onFilter();
};

const handleMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  mediumId.value = selected ? selected.id : null;
  localQuery.categorySmall = '';
  onFilter();
};

const handleSetLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  setLargeId.value = selected ? selected.id : null;
  localQuery.setCategoryMedium = '';
  localQuery.setCategorySmall = '';
  onFilter();
};

const handleSetMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  setMediumId.value = selected ? selected.id : null;
  localQuery.setCategorySmall = '';
  onFilter();
};
</script>

