<template>
<div class="product-market-page app-container">
    <div class="shop-layout-container">

      <div class="shop-main-content">
        <div class="shop-header-toolbar">
          <div class="shop-title-area">
            <h1 class="market-main-title">{{ $t('productMarket.title') }}</h1>
            <p class="market-subtitle">{{ $t('productMarket.subtitle') }}</p>
          </div>

          <div class="elegant-tabs-container">
            <button
              v-for="tab in [{value:'all',label:$t('productMarket.tabs.all')},{value:'product',label:$t('productMarket.tabs.general')},{value:'set',label:$t('productMarket.tabs.set')}]"
              :key="tab.value"
              class="tab-btn"
              :class="{ active: activeTab === tab.value }"
              @click="activeTab = tab.value; handleTabChange()"
            >
              {{ tab.label }}
            </button>
          </div>
        </div>

        <div v-loading="loading" class="grid-loading-container">
          <el-row :gutter="24" class="products-grid">
            <el-col
              v-for="item in displayList"
              :key="item.id + (item.isSet ? '-set' : '-prod')"
              :xs="24" :sm="12" :md="8" :lg="6" :xl="4"
              class="product-grid-item"
            >
              <product-card
                :item="item"
                :is-favorite="isFavorite(item)"
                :is-in-cart="isInCart(item)"
                @click="goToDetail"
                @favorite="toggleFavorite(item)"
                @add-to-cart="handleAddToCart(item)"
              />
            </el-col>
          </el-row>

          <el-empty v-if="!loading && displayList.length === 0" :description="$t('productMarket.labels.noData')" />
        </div>

        <div class="shop-pagination-wrap" v-if="total > 0">
          <el-pagination
            v-model:current-page="filters.page"
            v-model:page-size="filters.pageSize"
            :total="total"
            :page-sizes="[12, 24, 48, 96]"
            layout="prev, pager, next, jumper"
            @size-change="fetchData"
            @current-change="fetchData"
          />
        </div>
      </div>

      <div class="shop-sidebar-content">
        <market-sidebar
          :filters="filters"
          :active-tab="activeTab"
          :large-id="largeId"
          :medium-id="mediumId"
          :set-large-id="setLargeId"
          :set-medium-id="setMediumId"
          @filter="handleFilter"
          @reset="resetFilters"
          @large-change="handleLargeChange"
          @medium-change="handleMediumChange"
          @set-large-change="handleSetLargeChange"
          @set-medium-change="handleSetMediumChange"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useMarket } from './composables/useMarket';
import ProductCard from './components/ProductCard.vue';
import MarketSidebar from './components/MarketSidebar.vue';

const router = useRouter();
const {
  loading,
  displayList,
  total,
  activeTab,
  filters,
  fetchData,
  fetchCart,
  fetchFavorites,
  handleTabChange,
  handleFilter,
  resetFilters,
  handleAddToCart,
  toggleFavorite,
  isFavorite,
  isInCart
} = useMarket();

const largeId = ref<number | null>(null);
const mediumId = ref<number | null>(null);
const setLargeId = ref<number | null>(null);
const setMediumId = ref<number | null>(null);

const handleLargeChange = ({ val, opts }: any) => {
  const selected = opts.find((o: any) => o.code === val);
  largeId.value = selected ? selected.id : null;
  filters.categoryMedium = '';
  filters.categorySmall = '';
  handleFilter();
};

const handleMediumChange = ({ val, opts }: any) => {
  const selected = opts.find((o: any) => o.code === val);
  mediumId.value = selected ? selected.id : null;
  filters.categorySmall = '';
  handleFilter();
};

const handleSetLargeChange = ({ val, opts }: any) => {
  const selected = opts.find((o: any) => o.code === val);
  setLargeId.value = selected ? selected.id : null;
  filters.setCategoryMedium = '';
  filters.setCategorySmall = '';
  handleFilter();
};

const handleSetMediumChange = ({ val, opts }: any) => {
  const selected = opts.find((o: any) => o.code === val);
  setMediumId.value = selected ? selected.id : null;
  filters.setCategorySmall = '';
  handleFilter();
};

const goToDetail = (item: any) => {
  if (item.isSet) {
    router.push(`/prd/product-set-detail/${item.id}`);
  } else {
    router.push(`/prd/product-detail/${item.id}`);
  }
};

onMounted(() => {
  fetchData();
  fetchCart();
  fetchFavorites();
});
</script>

<style lang="scss" scoped>
.product-market-page {
  background-color: #fdfcf9;
  min-height: calc(100vh - 50px);

  :global(html.dark) & {
    background-color: #121212;
  }
}

.shop-layout-container {
  display: flex;
  gap: 2.5rem;
  max-width: 1600px;
  margin: 0 auto;
}

.shop-main-content {
  flex: 1;
  min-width: 0;
}

.shop-header-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  margin-bottom: 2.5rem;
  padding-bottom: 1.25rem;
  border-bottom: 1px solid #eae6df;

  :global(html.dark) & {
    border-bottom-color: #2e2e2e;
  }
}

.market-main-title {
  font-size: 1.75rem;
  font-weight: 700;
  margin: 0 0 0.5rem 0;
  letter-spacing: -0.5px;

  :global(html.dark) & {
    color: #ffffff;
  }
}

.market-subtitle {
  font-size: 0.95rem;
  color: #999999;
  margin: 0;

  :global(html.dark) & {
    color: #aaaaaa;
  }
}

.elegant-tabs-container {
  display: flex;
  gap: 1.5rem;

  .tab-btn {
    background: none;
    border: none;
    font-size: 0.95rem;
    font-weight: 600;
    color: #bbbbbb;
    padding: 0.5rem 0;
    cursor: pointer;
    position: relative;
    transition: all 0.3s;

    :global(html.dark) & {
      color: #777777;
    }

    &::after {
      content: '';
      position: absolute;
      bottom: -1px;
      left: 0;
      width: 0;
      height: 2px;
      background-color: #c5a880;
      transition: width 0.3s;
    }

    &.active {
      &::after { width: 100%; }

      :global(html.dark) & {
        color: #ffffff;
      }
    }

    &:hover:not(.active) {
      color: #888888;

      :global(html.dark) & {
        color: #bbbbbb;
      }
    }
  }
}

.products-grid {
  margin-bottom: 2.5rem;
}

.product-grid-item {
  margin-bottom: 1.875rem;
}

.shop-pagination-wrap {
  display: flex;
  justify-content: center;
  margin: 3rem 0;
}

.shop-sidebar-content {
  width: 290px;
  flex-shrink: 0;
  position: sticky;
  top: 100px;
  align-self: start;
  z-index: 10;
}

@media (max-width: 1200px) {
  .shop-sidebar-content { display: none; }
}
</style>

