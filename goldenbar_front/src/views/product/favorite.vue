<template>
<div class="favorite-page app-container">

    <el-row :gutter="30" class="shop-layout-row">

      <el-col :xs="24" :sm="24" :md="16" :lg="18" class="shop-main-content">
        <div class="shop-header-toolbar">
          <div class="shop-title-area">
            <h1 class="market-main-title">{{ $t('favorite.title') }}</h1>
            <p class="market-subtitle">{{ $t('favorite.subtitle') }}</p>
          </div>

          <div class="toolbar-actions">

            <div class="elegant-tabs-container">
              <button
                v-for="tab in [{value:'all',label:$t('favorite.tabs.all')},{value:'product',label:$t('favorite.tabs.general')},{value:'set',label:$t('favorite.tabs.set')}]"
                :key="tab.value"
                class="tab-btn"
                :class="{ active: activeTab === tab.value }"
                @click="activeTab = tab.value"
              >
                {{ tab.label }}
              </button>
            </div>

            <el-button link :icon="Refresh" @click="fetchFavorites" :class="{'is-loading': loading}" class="refresh-btn">
              {{ $t('favorite.actions.refresh') }}
            </el-button>
          </div>
        </div>

        <div v-loading="loading" class="grid-loading-container">
          <el-row :gutter="24" class="products-grid">
            <el-col
              v-for="item in filteredList"
              :key="item.id"
              :xs="24" :sm="12" :md="12" :lg="8" :xl="6"
              class="product-grid-item"
            >

              <div class="jovenca-product-card" @click="goToDetail(item)">
                <div class="product-image-box">
                  <el-image
                    v-if="getPhotoUrl(item)"
                    :src="getPhotoUrl(item)"
                    fit="cover"
                    class="product-img"
                  />
                  <div v-else class="no-image-box">
                    <span>{{ $t('favorite.labels.noImage') }}</span>
                  </div>

                  <div v-if="item.productSetId" class="set-label-badge">{{ $t('favorite.labels.setProduct') }}</div>

                  <div class="card-hover-actions">
                    <el-button
                      type="danger"
                      :icon="Delete"
                      circle
                      class="quick-action-button delete"
                      @click.stop="handleRemove(item.id)"
                    />
                    <el-button
                      type="warning"
                      :icon="ShoppingCart"
                      circle
                      class="quick-action-button cart"
                      @click.stop="handleAddToCart(item)"
                    />
                    <el-button
                      type="primary"
                      :icon="Search"
                      circle
                      class="quick-action-button search"
                      @click.stop="goToDetail(item)"
                    />
                  </div>

                  <div v-if="item.productSet && item.productSet.products && item.productSet.products.length > 0" class="set-compositions-tray">
                    <div v-for="(subProd, idx) in item.productSet.products.slice(0, 3)" :key="idx" class="tray-photo">
                      <el-image
                        v-if="subProd.photos && subProd.photos.length > 0"
                        :src="subProd.photos.find((p: any) => p.isMain)?.photoUrl || subProd.photos[0].photoUrl"
                        fit="cover"
                      />
                      <div v-else class="sub-placeholder">N/A</div>
                    </div>
                    <div v-if="item.productSet.products.length > 3" class="tray-more-count">
                      +{{ item.productSet.products.length - 3 }}
                    </div>
                  </div>
                </div>

                <div class="product-details-box">
                  <div class="product-title-row">
                    <h3 class="product-name-title" :title="getName(item)">{{ getName(item) }}</h3>
                    <span class="category-meta" :title="getCategory(item)">{{ getCategory(item) }}</span>
                  </div>
                  <div class="product-specifications">
                    <template v-if="item.product">
                      <span class="purity-badge">{{ item.product.purity }}</span>
                      <span class="spec-dot">•</span>
                      <span class="weight-badge">{{ item.product.weight }}g</span>
                    </template>
                    <template v-else-if="item.productSet">
                      <span class="compos-badge">{{ item.productSet.products?.length || 0 }} {{ $t('favorite.labels.componentsSet') }}</span>
                    </template>
                  </div>
                </div>
              </div>
            </el-col>
          </el-row>

          <el-empty v-if="!loading && filteredList.length === 0" :description="$t('favorite.labels.noData')" />
        </div>
      </el-col>

      <favorite-sidebar
        v-model:search-query="searchQuery"
        :favorite-list="favoriteList"
      />
    </el-row>
  </div>
</template>

<script setup lang="ts">

import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import store from '@/store';
import { getMyFavorites, removeFavorite } from '@/api/favorite';
import { addToCart } from '@/api/cart';
import { ElMessage, ElMessageBox } from 'element-plus';
import { Search, ShoppingCart, Delete, Refresh } from '@element-plus/icons-vue';
import useCodeStore from '@/store/modules/code';
import FavoriteSidebar from './components/FavoriteSidebar.vue';

const router = useRouter();
const { t } = useI18n();
const loading = ref(false);
const favoriteList = ref<any[]>([]);
const activeTab = ref('all');
const searchQuery = ref('');
const codeStore = useCodeStore();
const codeMap = computed(() => codeStore.codeMap);

const fetchFavorites = async () => {
  loading.value = true;
  try {
    const [res] = await Promise.all([
      getMyFavorites(),
      codeStore.fetchCodes()
    ]);

    favoriteList.value = res.data;
  } catch (error) {
    console.error('Failed to fetch favorites:', error);
  } finally {
    loading.value = false;
  }
};

const handleAddToCart = async (item: any) => {
  try {
    const data: any = {
      quantity: 1
    };

    if (item.productId) {
      data.productId = item.productId;

      if (item.product) {
        if (item.product.purity && item.product.purity !== 'EMPTY') {
          data.purity = item.product.purity.split(',')[0].trim();
        }
        if (item.product.colors) {
          data.color = item.product.colors.split(',')[0].trim();
        }
      }
    } else if (item.productSetId) {
      data.productSetId = item.productSetId;
    }

    await addToCart(data);
    await store.cart().fetchCart();
    ElMessage.success(t('productMarket.messages.cartAdded'));
  } catch (error) {
    console.error('Failed to add to cart:', error);
    ElMessage.error(t('productMarket.messages.cartError'));
  }
};

const filteredList = computed(() => {
  let list = favoriteList.value;

  if (activeTab.value === 'product') {
    list = list.filter(item => item.productId);
  } else if (activeTab.value === 'set') {
    list = list.filter(item => item.productSetId);
  }

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase();
    list = list.filter(item => {
      const name = getName(item).toLowerCase();
      return name.includes(query);
    });
  }

  return list;
});

const getPhotoUrl = (item: any) => {
  if (item.product) {
    const mainPhoto = item.product.photos?.find((p: any) => p.isMain) || item.product.photos?.[0];
    return mainPhoto ? mainPhoto.photoUrl : null;
  }
  if (item.productSet) {
    const mainPhoto = item.productSet.photos?.find((p: any) => p.isMain) || item.productSet.photos?.[0];
    return mainPhoto ? mainPhoto.photoUrl : null;
  }
  return null;
};

const getName = (item: any) => {
  return item.product?.name || item.productSet?.title || 'Unknown';
};

const getCategory = (item: any) => {
  const p = item.product || item.productSet;
  if (!p) return 'UNKNOWN';

  const parts = [
    codeMap.value[p.categoryLarge] || p.categoryLarge,
    codeMap.value[p.categoryMedium] || p.categoryMedium,
    codeMap.value[p.categorySmall] || p.categorySmall
  ].filter(v => !!v);

  return parts.length > 0 ? parts.join(' > ') : (item.productSetId ? 'SET' : 'GENERAL');
};

const handleRemove = async (id: number) => {
  try {
    await ElMessageBox.confirm(t('favorite.messages.confirmRemove'), t('favorite.messages.confirmTitle'), {
      confirmButtonText: t('cart.messages.orderBtn'),
      cancelButtonText: t('cart.messages.cancelBtn'),
      type: 'warning'
    });

    await removeFavorite(id);
    ElMessage.success(t('favorite.messages.removeSuccess'));
    fetchFavorites();
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Failed to remove favorite:', error);
      ElMessage.error(t('favorite.messages.removeError'));
    }
  }
};

const goToDetail = (item: any) => {
  if (item.productSetId) {
    router.push(`/prd/product-set-detail/${item.productSetId}`);
  } else if (item.productId) {
    router.push(`/prd/product-detail/${item.productId}`);
  }
};

onMounted(() => {
  fetchFavorites();
});
</script>

<script lang="ts">
export default {
  name: 'FavoriteList'
};
</script>
<style lang="scss" scoped src="./components/FavoriteStyles.scss"></style>

