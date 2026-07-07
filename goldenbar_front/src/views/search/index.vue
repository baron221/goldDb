<template>
<div class="search-results-container app-container">
    <div class="search-header">
      <div class="header-main">
        <h2 class="search-title">
          <el-icon><Search /></el-icon>
          {{ $t('search.title') }}
        </h2>
        <div class="header-search-bar">
          <el-input
            v-model="queryInput"
            :placeholder="$t('search.placeholder')"
            clearable
            @keyup.enter="performSearch"
            class="inner-search-input"
          >
            <template #append>
              <el-button @click="performSearch">
                <el-icon><Search /></el-icon>
              </el-button>
            </template>
          </el-input>
        </div>
      </div>
      <p class="search-query" v-if="query" v-html="$t('search.resultFor', { query: `<strong>${query}</strong>` })"></p>
    </div>

    <el-tabs v-model="activeTab" class="search-tabs">

      <el-tab-pane :label="$t('search.tabs.products', { count: results.products.length })" name="products">
        <div v-if="results.products.length > 0" class="results-grid">
          <el-card v-for="item in results.products" :key="item.id" class="result-card" @click="goToDetail('product', item.id)">
            <div class="card-content">
              <el-image :src="item.photoUrl || defaultImage" fit="cover" class="result-image" />
              <div class="info">
                <div class="name">{{ item.name }}</div>
                <div class="no">{{ item.productNo }}</div>
              </div>
            </div>
          </el-card>
        </div>
        <el-empty v-else :description="$t('common.noData')" />
      </el-tab-pane>

      <el-tab-pane :label="$t('search.tabs.productSets', { count: results.productSets.length })" name="productSets">
        <div v-if="results.productSets.length > 0" class="results-grid">
          <el-card v-for="item in results.productSets" :key="item.id" class="result-card" @click="goToDetail('productSet', item.id)">
            <div class="card-content">
              <el-image :src="item.photoUrl || defaultImage" fit="cover" class="result-image" />
              <div class="info">
                <div class="name">{{ item.title }}</div>
                <div class="no">{{ item.setNo }}</div>
              </div>
            </div>
          </el-card>
        </div>
        <el-empty v-else :description="$t('common.noData')" />
      </el-tab-pane>

      <el-tab-pane :label="$t('search.tabs.stocks', { count: results.stocks.length })" name="stocks">
        <div v-if="results.stocks.length > 0" class="results-grid">
          <el-card v-for="item in results.stocks" :key="item.id" class="result-card" @click="goToDetail('stock', item.id)">
            <div class="card-content">
              <el-image :src="item.photoUrl || defaultImage" fit="cover" class="result-image" />
              <div class="info">
                <div class="name">{{ item.stockNo }}</div>
                <div class="desc">{{ item.productName }}</div>
                <el-tag size="small" :type="getStatusType(item.status)">{{ getStatusName(item.status) }}</el-tag>
              </div>
            </div>
          </el-card>
        </div>
        <el-empty v-else :description="$t('common.noData')" />
      </el-tab-pane>

      <el-tab-pane :label="$t('search.tabs.orders', { count: results.orders.length })" name="orders">
        <base-table v-if="results.orders.length > 0" :data="results.orders" style="width: 100%" @row-click="(row) => goToDetail('order', row.orderNo)">
          <el-table-column prop="orderNo" :label="$t('order.filters.orderNo')" width="220" />
          <el-table-column prop="status" :label="$t('factory.table.status')" :excel-formatter="(row) => getOrderStatusName(row.status)">
            <template #default="{row}">
              <el-tag size="small" :type="getOrderStatusTagType(row.status)">{{ getOrderStatusName(row.status) }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="userName" :label="$t('order.list.orderer')" />
          <el-table-column prop="companyName" :label="$t('profile.labels.company')" />
          <el-table-column prop="createdAt" :label="$t('notice.labels.date')" width="180" :excel-formatter="(row) => formatDate(row.createdAt)">
            <template #default="{row}">
              {{ formatDate(row.createdAt) }}
            </template>
          </el-table-column>
        </base-table>
        <el-empty v-else :description="$t('common.noData')" />
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { Search } from '@element-plus/icons-vue';
import { integratedSearch } from '@/api/search';
import { parseTime } from '@/utils';
import { getStatusLabel, getStatusTagType } from '@/utils/order';
import BaseTable from '@/components/BaseTable/index.vue';
import useUserStore from '@/store/modules/user';
import useCodeStore from '@/store/modules/code';

const route = useRoute();
const router = useRouter();
const userStore = useUserStore();
const codeStore = useCodeStore();
const query = ref('');
const queryInput = ref('');
const activeTab = ref('products');
const loading = ref(false);
const defaultImage = '/thumb_no_img.png';
const codeMap = computed(() => codeStore.codeMap);

const results = ref({
  products: [] as any[],
  productSets: [] as any[],
  stocks: [] as any[],
  orders: [] as any[]
});

const performSearch = () => {
  if (!queryInput.value.trim()) return;
  router.push({ path: '/search', query: { q: queryInput.value.trim() }});
};

const handleSearch = async () => {
  if (!query.value) return;
  loading.value = true;
  try {
    const res = await integratedSearch(query.value);
    results.value = res.data;

    if (results.value.products.length > 0) activeTab.value = 'products';
    else if (results.value.productSets.length > 0) activeTab.value = 'productSets';
    else if (results.value.stocks.length > 0) activeTab.value = 'stocks';
    else if (results.value.orders.length > 0) activeTab.value = 'orders';
  } catch (error) {
    console.error(error);
  } finally {
    loading.value = false;
  }
};

const goToDetail = (type: string, id: any) => {
  switch (type) {
    case 'product':
      router.push(`/prd/product-detail/${id}`);
      break;
    case 'productSet':
      router.push(`/prd/product-set-detail/${id}`);
      break;
    case 'stock':
      router.push(`/stock/stock_detail/${id}`);
      break;
    case 'order':
      router.push({ path: '/my/order', query: { orderNo: id }});
      break;
  }
};

const getStatusName = (status: string) => {
  return codeMap.value[status] || status;
};

const getOrderStatusName = (status: string) => {
  return getStatusLabel(status, userStore.companyType);
};

const getOrderStatusTagType = (status: string) => {
  return getStatusTagType(status, userStore.companyType);
};

const getStatusType = (status: string) => {
  const map: any = {
    'IN_STOCK': 'success',
    'SOLD': 'info',
    'RENTED': 'warning'
  };
  return map[status] || '';
};

const formatDate = (date: string) => {
  return parseTime(new Date(date), '{y}-{m}-{d} {h}:{i}');
};

watch(() => route.query.q, (newQ) => {
  query.value = (newQ as string) || '';
  queryInput.value = query.value;
  if (query.value) {
    handleSearch();
  }
}, { immediate: true });

onMounted(async () => {
  codeStore.fetchCodes();
  if (route.query.q) {
    query.value = route.query.q as string;
    queryInput.value = query.value;
    handleSearch();
  }
});
</script>

<style lang="scss" scoped>
.search-results-container {
  max-width: 1200px;
  margin: 0 auto;

  .search-header {
    margin-bottom: 1.875rem;
    padding-bottom: 1.25rem;
    border-bottom: 1px solid #eee;

    .header-main {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 0.9375rem;

      @media (max-width: 768px) {
        flex-direction: column;
        align-items: flex-start;
        gap: 0.9375rem;
      }
    }

    .search-title {
      font-size: 1.5rem;
      font-weight: 700;
      margin: 0;
      display: flex;
      align-items: center;
      gap: 0.625rem;
      color: #1a1a1a;

      .el-icon { color: #c5a880; }
    }

    .header-search-bar {
      width: 400px;

      @media (max-width: 768px) {
        width: 100%;
      }

      .inner-search-input {
        :deep(.el-input__wrapper) {
          border-radius: 2px 0 0 8px;
          box-shadow: 0 0 0 1px #dcdfe6 inset !important;

          &.is-focus {
            box-shadow: 0 0 0 1px #c5a880 inset !important;
          }
        }

        :deep(.el-input-group__append) {
          background-color: #c5a880;
          border-color: #c5a880;
          color: #fff;
          border-radius: 0 8px 8px 0;

          .el-button {
            margin: 0;
            padding: 0 1.25rem;
            color: #fff;
            &:hover {
              opacity: 0.8;
            }
          }
        }
      }
    }

    .search-query {
      font-size: 0.875rem;
      color: #666;
      margin: 0;
      strong { color: #c5a880; }
    }
  }

  .results-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
    gap: 1.25rem;
    padding: 0.625rem 0;
  }

  .result-card {
    cursor: pointer;
    transition: transform 0.2s, box-shadow 0.2s;
    border-radius: 2px;
    overflow: hidden;

    &:hover {
      transform: translateY(-5px);
      box-shadow: 0 10px 20px rgba(0,0,0,0.1);
      border-color: #c5a880;
    }

    :deep(.el-card__body) { padding: 0; }

    .card-content {
      .result-image {
        width: 100%;
        height: 180px;
        background: #f8f8f8;
      }

      .info {
        padding: 0.9375rem;

        .name {
          font-weight: 700;
          font-size: 0.875rem;
          color: #333;
          margin-bottom: 0.25rem;
          white-space: nowrap;
          overflow: hidden;
          text-overflow: ellipsis;
        }

        .no {
          font-size: 0.95rem;
          color: #999;
          font-family: monospace;
          margin-bottom: 0.5rem;
        }

        .desc {
          font-size: 0.95rem;
          color: #666;
          margin-bottom: 0.5rem;
        }
      }
    }
  }

  .search-tabs {
    :deep(.el-tabs__item) {
      font-size: 0.9375rem;
      font-weight: 600;

      &.is-active {
        color: #c5a880;
      }
    }
    :deep(.el-tabs__active-bar) {
      background-color: #c5a880;
    }
  }
}
</style>

