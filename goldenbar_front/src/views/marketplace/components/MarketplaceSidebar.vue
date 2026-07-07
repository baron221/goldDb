<template>
<aside class="shop-sidebar-content">
    <div class="sidebar-widgets-wrapper">

      <div class="sidebar-widget actions-widget-top">
        <div class="search-actions-row-modern">
          <el-button
            type="primary"
            class="luxury-search-btn-full"
            @click="emitFilter"
          >
            <el-icon><Search /></el-icon>
            조회
          </el-button>
          <el-button
            class="luxury-reset-btn-full"
            @click="emitReset"
          >
            필터 초기화
          </el-button>
        </div>
      </div>

      <div class="sidebar-widget search-widget">
        <h4 class="widget-title">{{ $t('productMarket.labels.searchTitle') }}</h4>
        <div class="search-input-box">
          <el-input
            v-model="localFilters.search"
            :placeholder="$t('productMarket.labels.searchPlaceholder')"
            clearable
            class="luxury-search-input-sidebar"
            @keyup.enter="emitFilter"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
        </div>

        <div class="sidebar-widget" style="margin-top: 15px; margin-bottom: 5px;">
          <div class="select-filters-stack" style="padding: 0;">
            <div class="select-filter-item">
              <label>{{ $t('marketplace.filters.manufacturer') }}</label>
              <el-select
                v-model="localFilters.companyId"
                :placeholder="$t('marketplace.filters.manufacturer')"
                clearable
                filterable
                class="luxury-select-full"
                @change="emitFilter"
              >
                <el-option
                  v-for="company in manufacturers"
                  :key="company.id"
                  :label="company.name"
                  :value="company.id"
                />
              </el-select>
            </div>
          </div>
        </div>

        <div class="sidebar-widget" style="margin-top: 15px; margin-bottom: 5px;">
          <div class="select-filters-stack" style="padding: 0;">
            <div class="select-filter-item">
              <label>{{ $t('marketplace.filters.purity') }}</label>
              <common-select
                v-model="localFilters.product.purity"
                group-code="MATERIAL_GRADE"
                :placeholder="$t('marketplace.filters.purity')"
                class="luxury-select-full"
                @change="emitFilter"
              />
            </div>
          </div>
        </div>

        <div class="sidebar-widget" style="margin-top: 15px; margin-bottom: 5px;">
          <div class="select-filters-stack" style="padding: 0;">
            <div class="select-filter-item">
              <label>{{ $t('marketplace.filters.weightRange') }}</label>
              <div class="range-inputs">
                <el-input-number
                  v-model="localFilters.product.minWeight"
                  :min="0"
                  :precision="2"
                  :step="0.1"
                  controls-position="right"
                  :placeholder="$t('marketplace.filters.min')"
                  class="luxury-input-number"
                  @change="emitFilter"
                />
                <span class="range-sep">~</span>
                <el-input-number
                  v-model="localFilters.product.maxWeight"
                  :min="0"
                  :precision="2"
                  :step="0.1"
                  controls-position="right"
                  :placeholder="$t('marketplace.filters.max')"
                  class="luxury-input-number"
                  @change="emitFilter"
                />
              </div>
            </div>
          </div>
        </div>

        <div class="sidebar-widget" style="margin-top: 15px; margin-bottom: 5px;">
          <div class="select-filters-stack" style="padding: 0;">
            <div class="select-filter-item">
              <label>{{ $t('marketplace.filters.sizeRange') }}</label>
              <div class="range-inputs">
                <el-input-number
                  v-model="localFilters.product.minSize"
                  :min="1"
                  :max="30"
                  :step="1"
                  controls-position="right"
                  :placeholder="$t('marketplace.filters.min')"
                  class="luxury-input-number"
                  @change="emitFilter"
                />
                <span class="range-sep">~</span>
                <el-input-number
                  v-model="localFilters.product.maxSize"
                  :min="1"
                  :max="30"
                  :step="1"
                  controls-position="right"
                  :placeholder="$t('marketplace.filters.max')"
                  class="luxury-input-number"
                  @change="emitFilter"
                />
              </div>
            </div>
          </div>
        </div>
      </div>

      <div v-if="activeTab === 'all' || activeTab === 'product'" class="sidebar-widget categories-widget">
        <h4 class="widget-title">{{ $t('productMarket.labels.generalCategories') }}</h4>
        <div class="select-filters-stack">
          <div class="select-filter-item">
            <label>{{ $t('productMarket.labels.largeCategory') }}</label>
            <common-select
              v-model="localFilters.product.categoryLarge"
              group-code="PRODUCT_CATEGORY"
              :placeholder="$t('productMarket.labels.selectLarge')"
              class="luxury-select-full"
              @change="(val, options) => handleProdLargeChange(val, options)"
            />
          </div>
          <div class="select-filter-item" v-if="localFilters.product.categoryLarge">
            <label>{{ $t('productMarket.labels.mediumCategory') }}</label>
            <common-select
              v-model="localFilters.product.categoryMedium"
              :parent-id="prodLargeId"
              :placeholder="$t('productMarket.labels.selectMedium')"
              class="luxury-select-full"
              @change="(val, options) => handleProdMediumChange(val, options)"
            />
          </div>
          <div class="select-filter-item" v-if="localFilters.product.categoryMedium">
            <label>{{ $t('productMarket.labels.smallCategory') }}</label>
            <common-select
              v-model="localFilters.product.categorySmall"
              :parent-id="prodMediumId"
              :placeholder="$t('productMarket.labels.selectSmall')"
              class="luxury-select-full"
              @change="emitFilter"
            />
          </div>
        </div>
      </div>

      <div v-if="activeTab === 'all' || activeTab === 'set'" class="sidebar-widget categories-widget set-categories">
        <h4 class="widget-title">{{ $t('productMarket.labels.setCategories') }}</h4>
        <div class="select-filters-stack">
          <div class="select-filter-item">
            <label>{{ $t('productMarket.labels.largeCategory') }}</label>
            <common-select
              v-model="localFilters.set.categoryLarge"
              group-code="PRODUCT_CATEGORY"
              :placeholder="$t('productMarket.labels.selectLarge')"
              class="luxury-select-full"
              @change="(val, options) => handleSetLargeChange(val, options)"
            />
          </div>
          <div class="select-filter-item" v-if="localFilters.set.categoryLarge">
            <label>{{ $t('productMarket.labels.mediumCategory') }}</label>
            <common-select
              v-model="localFilters.set.categoryMedium"
              :parent-id="setLargeId"
              :placeholder="$t('productMarket.labels.selectMedium')"
              class="luxury-select-full"
              @change="(val, options) => handleSetMediumChange(val, options)"
            />
          </div>
          <div class="select-filter-item" v-if="localFilters.set.categoryMedium">
            <label>{{ $t('productMarket.labels.smallCategory') }}</label>
            <common-select
              v-model="localFilters.set.categorySmall"
              :parent-id="setMediumId"
              :placeholder="$t('productMarket.labels.selectSmall')"
              class="luxury-select-full"
              @change="emitFilter"
            />
          </div>
        </div>
      </div>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue';
import { Search } from '@element-plus/icons-vue';
import CommonSelect from '@/components/CommonSelect/index.vue';

const props = defineProps<{
  filters: any;
  manufacturers: any[];
  activeTab: string;
}>();

const emit = defineEmits(['filter', 'reset', 'update:filters']);

const localFilters = reactive({ ...props.filters });

watch(() => props.filters, (newVal) => {
  Object.assign(localFilters, newVal);
}, { deep: true });

watch(localFilters, (newVal) => {
  emit('update:filters', newVal);
}, { deep: true });

const prodLargeId = ref<number | null>(null);
const prodMediumId = ref<number | null>(null);
const setLargeId = ref<number | null>(null);
const setMediumId = ref<number | null>(null);

const emitFilter = () => {
  emit('filter');
};

const handleProdLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  prodLargeId.value = selected ? selected.id : null;
  localFilters.product.categoryMedium = '';
  localFilters.product.categorySmall = '';
  prodMediumId.value = null;
  emitFilter();
};

const handleProdMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  prodMediumId.value = selected ? selected.id : null;
  localFilters.product.categorySmall = '';
  emitFilter();
};

const handleSetLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  setLargeId.value = selected ? selected.id : null;
  localFilters.set.categoryMedium = '';
  localFilters.set.categorySmall = '';
  setMediumId.value = null;
  emitFilter();
};

const handleSetMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  setMediumId.value = selected ? selected.id : null;
  localFilters.set.categorySmall = '';
  emitFilter();
};

const emitReset = () => {
  prodLargeId.value = null;
  prodMediumId.value = null;
  setLargeId.value = null;
  setMediumId.value = null;
  emit('reset');
};
</script>

