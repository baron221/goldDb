<template>
<div class="market-filter-bar">
    <div class="filter-widget search-widget">
      <label class="widget-label">{{ $t('productMarket.labels.searchTitle') }}</label>
      <el-input
        v-model="localFilters.search"
        :placeholder="$t('productMarket.labels.searchPlaceholder')"
        clearable
        class="filter-input"
        @keyup.enter="emit('filter')"
        @clear="emit('filter')"
      >
        <template #prefix>
          <el-icon><Search /></el-icon>
        </template>
      </el-input>
    </div>

    <div class="filter-widget">
      <label class="widget-label">{{ $t('productMarket.labels.categoryFilter') }}</label>
      <common-select
        v-model="localFilters.categoryLarge"
        group-code="PRODUCT_CATEGORY"
        show-all
        placeholder="대분류 전체"
        class="filter-input"
        @change="emit('filter')"
      />
    </div>

    <div class="filter-widget">
      <label class="widget-label">{{ $t('marketplace.filters.purity') }}</label>
      <common-select
        v-model="localFilters.purity"
        group-code="MATERIAL_GRADE"
        show-all
        :placeholder="$t('marketplace.filters.purity')"
        class="filter-input"
        @change="emit('filter')"
      />
    </div>

    <div class="filter-widget weight-widget">
      <label class="widget-label">{{ $t('marketplace.filters.weightRange') }}</label>
      <div class="range-inputs">
        <el-input-number
          v-model="localFilters.minWeight"
          :min="0"
          :precision="2"
          :step="0.1"
          controls-position="right"
          :placeholder="$t('marketplace.filters.min')"
          class="range-input"
          @change="emit('filter')"
        />
        <span class="range-sep">~</span>
        <el-input-number
          v-model="localFilters.maxWeight"
          :min="0"
          :precision="2"
          :step="0.1"
          controls-position="right"
          :placeholder="$t('marketplace.filters.max')"
          class="range-input"
          @change="emit('filter')"
        />
      </div>
    </div>

    <div class="filter-actions">
      <el-button type="primary" class="search-btn" @click="emit('filter')">
        <el-icon><Search /></el-icon> {{ $t('common.search') }}
      </el-button>
      <el-button class="reset-btn" @click="emit('reset')">
        <i class="fas fa-undo"></i> {{ $t('productMarket.labels.resetFilters') }}
      </el-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, watch } from 'vue';
import { Search } from '@element-plus/icons-vue';
import CommonSelect from '@/components/CommonSelect/index.vue';

const props = defineProps<{
  filters: any;
}>();

const emit = defineEmits([
  'filter', 'reset', 'update:filters'
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
.market-filter-bar {
  display: flex;
  align-items: flex-end;
  flex-wrap: wrap;
  gap: 1.25rem;
  padding: 1.25rem 1.625rem;
  border: 1px solid #eae6df;
  border-radius: 2px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.01);
  margin-bottom: 2rem;

  :global(html.dark) & {
    border-color: #2e2e2e;
  }
}

.filter-widget {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  min-width: 180px;

  &.search-widget {
    flex: 1 1 240px;
  }

  &.weight-widget {
    min-width: 220px;
  }
}

.widget-label {
  font-size: 0.8125rem;
  font-weight: 600;
  color: #888;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.filter-input {
  width: 100%;
}

.range-inputs {
  display: flex;
  align-items: center;
  gap: 0.5rem;

  .range-input {
    width: 100%;
  }

  .range-sep {
    color: #999;
  }
}

.filter-actions {
  display: flex;
  gap: 0.75rem;
  margin-left: auto;

  .search-btn {
    font-weight: 700;
    letter-spacing: 0.5px;
  }

  .reset-btn {
    border: 1px dashed #c5a880;
    color: #c5a880;
    background: transparent;

    &:hover {
      background: #c5a880;
      color: white;
      border-style: solid;
    }
  }
}

@media (max-width: 768px) {
  .filter-actions {
    margin-left: 0;
    width: 100%;

    .el-button {
      flex: 1;
    }
  }
}
</style>
