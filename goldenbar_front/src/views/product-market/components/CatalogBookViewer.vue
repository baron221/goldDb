<template>
<div class="viewer-main-container">
    <div class="viewer-header-bar">
      <el-button class="back-list-btn" @click="closeViewer">
        <el-icon><ArrowLeft /></el-icon>
        <span v-if="!isMobile"> 책자 목록으로</span>
        <span v-else> 목록</span>
      </el-button>

      <div class="active-catalog-title">
        <h3>{{ selectedCatalog.title }}</h3>
        <span class="active-issue">{{ selectedCatalog.issueNo }}</span>
      </div>

      <div class="header-right-controls">

        <div v-if="!isMobile" class="view-mode-controls">
          <el-button-group>
            <el-button
              :type="userViewMode === 'auto' ? 'primary' : 'default'"
              size="default"
              @click="userViewMode = 'auto'"
              title="화면 크기에 따라 자동 설정"
            >
              자동
            </el-button>
            <el-button
              :type="userViewMode === 'single' ? 'primary' : 'default'"
              size="default"
              @click="userViewMode = 'single'"
              title="1페이지씩 보기"
            >
              1쪽 보기
            </el-button>
            <el-button
              :type="userViewMode === 'double' ? 'primary' : 'default'"
              size="default"
              @click="userViewMode = 'double'"
              title="2페이지씩 보기"
            >
              2쪽 보기
            </el-button>
          </el-button-group>
        </div>

        <el-select
          :model-value="activeCatalogId"
          placeholder="다른 책자 선택"
          class="catalog-switcher"
          @change="changeCatalog"
        >
          <el-option
            v-for="cat in catalogList"
            :key="cat.id"
            :label="cat.title"
            :value="cat.id"
          />
        </el-select>
      </div>
    </div>

    <div class="viewer-workspace">

      <div
        class="book-flip-container"
        @touchstart="handleTouchStart"
        @touchend="handleTouchEnd"
      >
        <el-button
          circle
          class="nav-btn prev-btn"
          :disabled="currentPageIndex === 0"
          @click="prevPage"
        >
          <el-icon><ArrowLeft /></el-icon>
        </el-button>

        <div v-if="viewMode === 'double'" class="pages-spread">

          <div class="page-sheet left-sheet" :class="{ 'cover-page': currentPageIndex === 0 }">
            <div class="page-wrapper-inner" v-if="leftPage" v-loading="isLeftLoading">
              <img
                :src="getApiBaseUrl() + leftPage.photoUrl"
                class="page-img"
                alt="left-page"
                @load="handleLeftImageLoad"
                @error="handleLeftImageLoad"
              />
              <div class="page-number-tag">{{ leftPage.pageNumber }}p</div>
            </div>
            <div v-else class="empty-sheet-placeholder">
              <p>표지 뒷면</p>
            </div>
          </div>

          <div class="page-sheet right-sheet">
            <div class="page-wrapper-inner" v-if="rightPage" v-loading="isRightLoading">
              <img
                :src="getApiBaseUrl() + rightPage.photoUrl"
                class="page-img"
                alt="right-page"
                @load="handleRightImageLoad"
                @error="handleRightImageLoad"
              />
              <div class="page-number-tag">{{ rightPage.pageNumber }}p</div>
            </div>
            <div v-else class="empty-sheet-placeholder">
              <p>마지막 페이지</p>
            </div>
          </div>
        </div>

        <div v-else class="page-single">
          <div class="page-sheet single-sheet">
            <div class="page-wrapper-inner" v-if="singlePage" v-loading="isSingleLoading">
              <img
                :src="getApiBaseUrl() + singlePage.photoUrl"
                class="page-img"
                alt="single-page"
                @load="handleSingleImageLoad"
                @error="handleSingleImageLoad"
              />
              <div class="page-number-tag">{{ singlePage.pageNumber }}p</div>
            </div>
            <div v-else class="empty-sheet-placeholder">
              <p>페이지가 없습니다.</p>
            </div>
          </div>
        </div>

        <el-button
          circle
          class="nav-btn next-btn"
          :disabled="currentPageIndex >= maxPageIndex"
          @click="nextPage"
        >
          <el-icon><ArrowRight /></el-icon>
        </el-button>
      </div>

      <div v-if="!isMobile" class="connected-products-panel">
        <h4 class="panel-title">
          <el-icon><ShoppingCart /></el-icon> 현재 페이지 상품 목록
        </h4>
        <catalog-product-list
          :products="currentProducts"
          @add-to-cart="addProductToCart"
        />
      </div>
    </div>

    <div class="viewer-footer-nav">
      <div class="page-indicator">
        <span>{{ currentPageIndicatorText }}</span>
      </div>
      <div class="slider-wrapper">
        <el-slider
          v-model="currentPageIndex"
          :max="maxPageIndex"
          :min="0"
          :show-tooltip="false"
          @input="handleSliderInput"
        />
      </div>

      <el-button
        v-if="isMobile"
        type="primary"
        class="mobile-products-toggle-btn"
        @click="showMobileProducts = true"
      >
        <el-icon><ShoppingCart /></el-icon> 상품 ({{ currentProducts.length }})
      </el-button>
    </div>

    <el-drawer
      v-if="isMobile"
      v-model="showMobileProducts"
      title="현재 페이지 상품 목록"
      direction="bottom"
      size="60%"
      class="mobile-products-drawer"
    >
      <catalog-product-list
        :products="currentProducts"
        @add-to-cart="addProductToCart"
      />
    </el-drawer>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { getProduct } from '@/api/product';
import { addToCart } from '@/api/cart';
import store from '@/store';
import { ElMessage } from 'element-plus';
import { ArrowLeft, ArrowRight, ShoppingCart } from '@element-plus/icons-vue';
import CatalogProductList from './CatalogProductList.vue';
import { useCatalogViewer } from './composables/useCatalogViewer';

const props = defineProps({
  selectedCatalog: { type: Object, required: true },
  catalogList: { type: Array as () => any[], required: true },
  activeCatalogId: { type: Number, required: true }
});

const emit = defineEmits(['close', 'select-catalog']);

const showMobileProducts = ref(false);

const selectedCatalogRef = computed(() => props.selectedCatalog);
const activeCatalogIdRef = computed(() => props.activeCatalogId);

const {
  currentPageIndex, userViewMode, viewMode,
  isLeftLoading, isRightLoading, isSingleLoading,
  handleLeftImageLoad, handleRightImageLoad, handleSingleImageLoad,
  handleTouchStart, handleTouchEnd,
  maxPageIndex, singlePage, leftPage, rightPage,
  currentPageIndicatorText, currentProducts,
  prevPage, nextPage, handleSliderInput
} = useCatalogViewer(selectedCatalogRef, activeCatalogIdRef);

const closeViewer = () => emit('close');
const changeCatalog = (id: number) => {
  emit('select-catalog', id);
  currentPageIndex.value = 0;
};

const addProductToCart = async (prod: any) => {
  try {
    const productRes = await getProduct(prod.id);
    const productDetail = productRes.data;
    const data: any = { productId: prod.id, quantity: 1 };
    if (productDetail.purity && productDetail.purity !== 'EMPTY') {
      data.purity = productDetail.purity.split(',')[0].trim();
    }
    if (productDetail.colors) {
      data.color = productDetail.colors.split(',')[0].trim();
    }
    await addToCart(data);
    await store.cart().fetchCart();
    ElMessage.success(`${prod.name} 상품이 장바구니에 추가되었습니다.`);
  } catch (error) {
    ElMessage.error('장바구니 담기에 실패했습니다.');
  }
};

const getApiBaseUrl = () => import.meta.env.VITE_APP_BASE_API || '';
</script>

<style lang="scss" scoped src="./CatalogBookViewerStyles.scss"></style>

