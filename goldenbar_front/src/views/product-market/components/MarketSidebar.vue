<template>
<div class="sidebar-widgets-wrapper">

    <div class="sidebar-widget search-widget">
      <h4 class="widget-title">{{ $t('productMarket.labels.searchTitle') }}</h4>
      <div class="search-input-box">
        <el-input
          v-model="localFilters.search"
          :placeholder="$t('productMarket.labels.searchPlaceholder')"
          clearable
          @keyup.enter="emit('filter')"
          @clear="emit('filter')"
        >
          <template #prefix>
            <el-icon><Search /></el-icon>
          </template>
        </el-input>
      </div>
    </div>

    <div class="sidebar-widget filter-widget" v-if="activeTab !== 'set'">
      <h4 class="widget-title">{{ $t('productMarket.labels.categoryFilter') }}</h4>
      <div class="category-selection-stack">
        <common-select
          v-model="localFilters.categoryLarge"
          group-code="PRODUCT_CATEGORY"
          show-all
          placeholder="대분류 전체"
          class="sidebar-select"
          @change="(val, opts) => emit('large-change', {val, opts})"
        />
        <common-select
          v-model="localFilters.categoryMedium"
          :parent-id="largeId"
          placeholder="중분류 전체"
          class="sidebar-select"
          @change="(val, opts) => emit('medium-change', {val, opts})"
        />
        <common-select
          v-model="localFilters.categorySmall"
          :parent-id="mediumId"
          placeholder="소분류 전체"
          class="sidebar-select"
          @change="emit('filter')"
        />
      </div>
    </div>

    <div class="sidebar-widget filter-widget set-categories" v-if="activeTab !== 'product'">
      <h4 class="widget-title">{{ $t('productMarket.labels.setCategoryFilter') }}</h4>
      <div class="category-selection-stack">
        <common-select
          v-model="localFilters.setCategoryLarge"
          group-code="PRODUCT_CATEGORY"
          show-all
          placeholder="세트 대분류"
          class="sidebar-select"
          @change="(val, opts) => emit('set-large-change', {val, opts})"
        />
        <common-select
          v-model="localFilters.setCategoryMedium"
          :parent-id="setLargeId"
          placeholder="세트 중분류"
          class="sidebar-select"
          @change="(val, opts) => emit('set-medium-change', {val, opts})"
        />
        <common-select
          v-model="localFilters.setCategorySmall"
          :parent-id="setMediumId"
          placeholder="세트 소분류"
          class="sidebar-select"
          @change="emit('filter')"
        />
      </div>
    </div>

    <div class="sidebar-widget company-widget">
      <h4 class="widget-title">{{ $t('productMarket.labels.manufacturerFilter') }}</h4>
      <company-select
        v-model="localFilters.companyId"
        placeholder="제조사 전체"
        class="sidebar-select"
        @change="emit('filter')"
      />
    </div>

    <div class="sidebar-actions-wrap">
      <el-button type="primary" class="search-btn" @click="emit('filter')">
        <el-icon><Search /></el-icon> {{ $t('common.search') }}
      </el-button>
      <el-button class="reset-all-btn" @click="emit('reset')">
        <i class="fas fa-undo"></i> {{ $t('productMarket.labels.resetFilters') }}
      </el-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, watch } from 'vue';
import { Search } from '@element-plus/icons-vue';
import CommonSelect from '@/components/CommonSelect/index.vue';
import CompanySelect from '@/components/CompanySelect/index.vue';

const props = defineProps<{
  filters: any;
  activeTab: string;
  largeId: number | null;
  mediumId: number | null;
  setLargeId: number | null;
  setMediumId: number | null;
}>();

const emit = defineEmits([
  'filter', 'reset', 'large-change', 'medium-change', 'set-large-change', 'set-medium-change', 'update:filters'
]);

const localFilters = reactive({ ...props.filters });

watch(() => props.filters, (newVal) => {
  Object.assign(localFilters, newVal);
}, { deep: true });

watch(localFilters, (newVal) => {
  emit('update:filters', newVal);
}, { deep: true });
</script>

<style lang="scss" scoped>
.sidebar-widgets-wrapper { display: flex; flex-direction: column; gap: 1.875rem; }
.sidebar-widget {
  border: 1px solid #eae6df; border-radius: 2px; padding: 1.625rem; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.01);
  &.set-categories { border-top: 3px solid #c5a880; }
  .widget-title {
    font-size: 0.875rem; font-weight: 600; margin: 0 0 1.25rem 0; padding-bottom: 0.95rem; padding-left: 0.9375rem; border-bottom: 1px solid #eae6df; text-transform: uppercase; letter-spacing: 0.5px; position: relative;
    &::after { content: ''; position: absolute; bottom: 0; left: 0; top: 0; width: 3px; background-color: #c5a880; }
  }
}
.category-selection-stack { display: flex; flex-direction: column; gap: 0.75rem; }
.sidebar-select { width: 100% !important; }
.sidebar-actions-wrap {
  display: flex;
  flex-direction: column;
  gap: 0.625rem;
  margin-top: 0.625rem;
}
.search-btn {
  width: 100%;
  height: 48px;
  font-weight: 700;
  letter-spacing: 1px;
  text-transform: uppercase;
}
.reset-all-btn {
  width: 100%; height: 42px; border: 1px dashed #c5a880; color: #c5a880; background: #fdfcf9; border-radius: 2px; font-weight: 600; letter-spacing: 0.5px; transition: all 0.3s;
  margin-left: 0 !important;
  &:hover { background: #c5a880; color: white; border-style: solid; }
}
</style>

