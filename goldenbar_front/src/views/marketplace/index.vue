<template>
<div class="marketplace-page-luxury">

    <header class="luxury-hero-header">
      <div class="hero-content-wrapper">
        <h1 class="hero-title">{{ $t('marketplace.title') }}</h1>
        <p class="hero-subtitle">{{ $t('marketplace.subtitle') }}</p>
        <div class="hero-divider"></div>
      </div>
    </header>

    <div class="luxury-marketplace-content">
      <div class="marketplace-layout-container">

        <marketplace-sidebar
          :filters="filters"
          :manufacturers="manufacturers"
          :active-tab="activeTab"
          @filter="handleFilter"
          @reset="resetQuery"
        />

        <main class="shop-main-content">

          <section class="filter-section-luxury">
            <div class="filter-controls-grid">
              <div class="filter-left">
                <el-radio-group v-model="activeTab" size="default" class="luxury-tab-group" @change="handleTabChange">
                  <el-radio-button value="all">{{ $t('marketplace.tabs.all') }}</el-radio-button>
                  <el-radio-button value="product">{{ $t('marketplace.tabs.product') }}</el-radio-button>
                  <el-radio-button value="set">{{ $t('marketplace.tabs.set') }}</el-radio-button>
                </el-radio-group>
              </div>
              <div class="filter-right">
                <div class="results-count" v-if="!loading">
                  <span>{{ total }}</span> {{ $t('productMarket.labels.resultsFound') }}
                </div>
              </div>
            </div>
          </section>

          <div v-loading="loading" class="collections-grid-wrapper">
            <el-row :gutter="24" class="luxury-products-grid">
              <el-col :xs="24" :sm="12" :md="12" :lg="8" v-for="item in displayList" :key="item.id + (item.isSet ? '-set' : '-prod')">
                <div class="luxury-product-card" @click="goToDetail(item)">

                  <div class="card-image-box">
                    <div class="hover-overlay">
                      <span class="view-detail-text">{{ $t('marketplace.viewDetails') }}</span>
                    </div>
                    <el-image
                      v-if="item.displayPhoto"
                      :src="item.displayPhoto"
                      fit="cover"
                      class="main-product-image"
                    />
                    <div v-else class="image-placeholder">
                      <span class="placeholder-text">{{ $t('marketplace.labels.placeholder') }}</span>
                    </div>
                    <div v-if="item.isSet" class="luxury-set-badge">{{ $t('marketplace.labels.set') }}</div>
                  </div>

                  <div class="card-info-box">
                    <div class="category-label">{{ getCategoryName(item) }}</div>
                    <h3 class="product-name">{{ item.name || item.title }}</h3>

                    <div class="product-specs">
                      <template v-if="!item.isSet">
                        <span class="spec-purity">{{ item.purity }}</span>
                        <span class="spec-weight">{{ item.weight }}g</span>
                      </template>
                      <template v-else>
                        <span class="spec-count">{{ item.productCount }} {{ $t('marketplace.labels.piecesIncluded') }}</span>
                      </template>
                    </div>
                  </div>
                </div>
              </el-col>
            </el-row>

            <el-empty v-if="!loading && displayList.length === 0" :description="$t('productMarket.labels.noData')" class="luxury-empty-state" />
          </div>

          <div class="pagination-container-luxury" v-if="total > 0">
            <el-pagination
              v-model:current-page="filters.page"
              v-model:page-size="filters.pageSize"
              layout="prev, pager, next"
              :total="total"
              class="luxury-pagination"
              @current-change="fetchData"
            />
          </div>
        </main>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { getProducts } from '@/api/product';
import { getProductSets } from '@/api/product-set';
import { getCompanies } from '@/api/company';
import { getThumbnailUrl } from '@/utils';
import useCodeStore from '@/store/modules/code';
import MarketplaceSidebar from './components/MarketplaceSidebar.vue';

const router = useRouter();
const codeStore = useCodeStore();
const loading = ref(false);
const activeTab = ref('all');
const productList = ref<any[]>([]);
const setList = ref<any[]>([]);
const total = ref(0);
const manufacturers = ref<any[]>([]);

const filters = reactive({
  search: '',
  companyId: null as number | null,
  page: 1,
  pageSize: 12,
  product: {
    categoryLarge: '',
    categoryMedium: '',
    categorySmall: '',
    purity: '',
    minWeight: null as number | null,
    maxWeight: null as number | null,
    minSize: null as number | null,
    maxSize: null as number | null
  },
  set: {
    categoryLarge: '',
    categoryMedium: '',
    categorySmall: ''
  }
});

const getCategoryName = (item: any) => {
  const parts = [];
  if (item.categoryLarge) parts.push(codeStore.getName(item.categoryLarge));
  if (item.categoryMedium) parts.push(codeStore.getName(item.categoryMedium));
  if (item.categorySmall) parts.push(codeStore.getName(item.categorySmall));

  const path = parts.filter(Boolean).join(' > ');
  return path || (item.isSet ? 'SET' : 'COLLECTION');
};

const displayList = computed(() => {
  let combined: any[] = [];

  if (activeTab.value === 'all' || activeTab.value === 'product') {
    combined = [...combined, ...productList.value.map(p => {
      const mainPhoto = p.photos?.find((ph: any) => ph.isMain) || p.photos?.[0];
      const photoUrl = mainPhoto?.photoUrl || p.photoUrl || null;
      return {
        ...p,
        isSet: false,
        displayPhoto: photoUrl || null
      };
    })];
  }

  if (activeTab.value === 'all' || activeTab.value === 'set') {
    combined = [...combined, ...setList.value.map(s => {
      const mainPhoto = s.photos?.find((ph: any) => ph.isMain) || s.photos?.[0];
      const photoUrl = mainPhoto?.photoUrl || s.photoUrl || null;
      return {
        ...s,
        isSet: true,
        name: s.title,
        displayPhoto: photoUrl || null,
        productCount: s.products ? s.products.length : 0
      };
    })];
  }

  return combined;
});

const fetchManufacturers = async () => {
  try {
    const res = await getCompanies({ category: 'MFG' });
    manufacturers.value = res.data.items || res.data;
  } catch (error) {
    console.error('Failed to fetch manufacturers:', error);
  }
};

const handleFilter = () => {
  filters.page = 1;
  fetchData();
};

const resetQuery = () => {
  filters.search = '';
  filters.companyId = null;
  filters.product = {
    categoryLarge: '',
    categoryMedium: '',
    categorySmall: '',
    purity: '',
    minWeight: null,
    maxWeight: null,
    minSize: null,
    maxSize: null
  };
  filters.set = {
    categoryLarge: '',
    categoryMedium: '',
    categorySmall: ''
  };
  handleFilter();
};

const fetchData = async () => {
  loading.value = true;
  try {
    if (activeTab.value === 'all' || activeTab.value === 'product') {
      const res = await getProducts({
        name: filters.search,
        companyId: filters.companyId || undefined,
        categoryLarge: filters.product.categoryLarge,
        categoryMedium: filters.product.categoryMedium,
        categorySmall: filters.product.categorySmall,
        purity: filters.product.purity || undefined,
        minWeight: filters.product.minWeight || undefined,
        maxWeight: filters.product.maxWeight || undefined,
        minSize: filters.product.minSize || undefined,
        maxSize: filters.product.maxSize || undefined,
        page: filters.page,
        pageSize: filters.pageSize,
        isPublic: true
      });
      productList.value = res.data.items;
      if (activeTab.value === 'product') total.value = res.data.totalCount;
    }

    if (activeTab.value === 'all' || activeTab.value === 'set') {
      const res = await getProductSets({
        title: filters.search,
        companyId: filters.companyId || undefined,
        categoryLarge: filters.set.categoryLarge,
        categoryMedium: filters.set.categoryMedium,
        categorySmall: filters.set.categorySmall,
        page: filters.page,
        pageSize: filters.pageSize,
        isPublic: true
      });
      setList.value = res.data.items;
      if (activeTab.value === 'set') total.value = res.data.totalCount;
    }

    if (activeTab.value === 'all') {
      total.value = productList.value.length + setList.value.length;
    }
  } catch (error) {
    console.error('Failed to fetch marketplace data:', error);
  } finally {
    loading.value = false;
  }
};

const handleTabChange = () => {
  filters.page = 1;
  fetchData();
};

const goToDetail = (item: any) => {
  if (item.isSet) {
    router.push(`/prd/product-set-detail/${item.id}`);
  } else {
    router.push(`/prd/product-detail/${item.id}`);
  }
};

onMounted(() => {
  fetchManufacturers();
  fetchData();
});
</script>

<style lang="scss" src="./components/MarketplaceStyles.scss"></style>

