<template>
<div class="product-set-detail-container app-container">
    <el-card v-loading="loading" shadow="never" class="premium-detail-card">
      <div v-if="productSet.id">
        <el-row :gutter="50" class="product-main-row">

          <el-col :span="11" :xs="24" :sm="11">
            <product-set-gallery
              :product-set="productSet"
              :default-image="defaultImage"
            />
          </el-col>

          <el-col :span="13" :xs="24" :sm="13">
            <product-set-options
              v-model:quantity="quantity"
              v-model:active-collapse-names="activeCollapseNames"
              :product-set="productSet"
              :is-favorite="isFavorite"
              :is-retail-user="isRetailUser"
              :is-mfg-user="isMfgUser"
              @edit="handleEdit"
              @favorite="handleFavorite"
              @cart-all="handleCartAll"
              @buy-all="handleBuyAll"
            />
          </el-col>
        </el-row>

        <product-set-included
          :products="productSet.products"
          @go-to-product-detail="goToProductDetail"
        />
      </div>
      <el-empty v-else :description="$t('productDetail.messages.loadError')" />

      <ProductSetDialog
        v-model="dialogFormVisible"
        :id="currentProductSetId"
        @success="getDetail"
      />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, nextTick } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { getProductSet } from '@/api/product-set';
import store from '@/store';
import { addToCart } from '@/api/cart';
import { addFavorite, removeFavoriteByProduct, getMyFavorites } from '@/api/favorite';
import { ElMessage, ElNotification } from 'element-plus';
import useUserStore from '@/store/modules/user';
import ProductSetDialog from '@/components/ProductSetDialog/index.vue';
import ProductSetGallery from './components/ProductSetGallery.vue';
import ProductSetOptions from './components/ProductSetOptions.vue';
import ProductSetIncluded from './components/ProductSetIncluded.vue';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const userStore = useUserStore();

const loading = ref(true);

const productSet = ref<any>({});
const quantity = ref(1);
const isFavorite = ref(false);
const activeCollapseNames = ref(['description']);
const defaultImage = 'https://via.placeholder.com/600x600?text=No+Set+Image';

const isRetailUser = computed(() => userStore.companyType === 'RTL');
const isMfgUser = computed(() => userStore.companyType === 'MFG');

const dialogFormVisible = ref(false);
const currentProductSetId = ref<number | null>(null);

const handleEdit = () => {
  currentProductSetId.value = productSet.value.id;
  dialogFormVisible.value = true;
};

const checkFavorite = async () => {
  try {
    const res = await getMyFavorites();
    const favorites = res.data || [];
    isFavorite.value = favorites.some((f: any) => f.productSetId === productSet.value.id);
  } catch (error) {
    console.error('Failed to check favorite status:', error);
  }
};

const getDetail = async () => {
  const id = route.params.id as string;
  if (!id) return;

  loading.value = true;
  try {
    const res = await getProductSet(parseInt(id));
    productSet.value = res.data;

    await checkFavorite();

    if (productSet.value.title) {
      await nextTick();
      const title = `${productSet.value.title}|${productSet.value.productNo || ''}`;
      store.tagsView().updateVisitedViewTitle(route.fullPath, title);
    }
  } catch (error) {
    console.error(error);
    ElMessage.error('세트 정보를 불러오는데 실패했습니다.');
  } finally {
    loading.value = false;
  }
};

const goToProductDetail = (id: number) => {
  router.push(`/prd/product-detail/${id}`);
};

const handleBuyAll = async () => {
  try {
    await addToCart({
      productSetId: productSet.value.id,
      quantity: quantity.value
    });
    await store.cart().fetchCart();
    router.push('/my/cart');
  } catch (error) {
    console.error(error);
    ElMessage.error('장바구니 담기에 실패했습니다.');
  }
};

const handleCartAll = async () => {
  try {
    await addToCart({
      productSetId: productSet.value.id,
      quantity: quantity.value
    });
    await store.cart().fetchCart();

    let seconds = 3;
    const notification = ElNotification({
      title: '장바구니 담기 성공',
      dangerouslyUseHTMLString: true,
      message: `<div id="cart-notif-set-msg">장바구니에 담겼습니다. <br> <strong>${seconds}</strong>초 후 사라집니다. <br><br> <span style="color: #409eff; font-weight: bold; text-decoration: underline; cursor: pointer;">[장바구니로 이동]</span></div>`,
      duration: 3000,
      onClick: () => {
        router.push('/my/cart');
        notification.close();
      }
    });

    const timer = setInterval(() => {
      seconds--;
      const el = document.querySelector('#cart-notif-set-msg');
      if (el) {
        el.innerHTML = `장바구니에 담겼습니다. <br> <strong>${seconds}</strong>초 후 사라집니다. <br><br> <span style="color: #409eff; font-weight: bold; text-decoration: underline; cursor: pointer;">[장바구니로 이동]</span>`;
      }
      if (seconds <= 0) clearInterval(timer);
    }, 1000);
  } catch (error) {
    console.error(error);
    ElMessage.error('장바구니 담기에 실패했습니다.');
  }
};

const handleFavorite = async () => {
  try {
    if (isFavorite.value) {
      await removeFavoriteByProduct({ productSetId: productSet.value.id });
      isFavorite.value = false;
      ElMessage.success('즐겨찾기에서 삭제되었습니다.');
    } else {
      await addFavorite({ productSetId: productSet.value.id });
      isFavorite.value = true;
      ElMessage.success('즐겨찾기에 추가되었습니다.');
    }
  } catch (error) {
    console.error('Failed to toggle favorite:', error);
    ElMessage.error('즐겨찾기 처리 중 오류가 발생했습니다.');
  }
};

onMounted(() => {
  getDetail();
});
</script>

<style lang="scss" scoped>
.product-set-detail-container {
  max-width: 1280px;
  margin: 0 auto;
  padding: 0.625rem 0;

  .premium-detail-card {
    border: none;
    background-color: transparent;

    :deep(.el-card__body) {
      padding: 0;
    }
  }

  .product-main-row {
    display: flex;
    flex-wrap: wrap;
  }
}

:deep(.el-image-viewer__wrapper) {
  .el-image-viewer__mask {
    background-color: rgba(10, 10, 10, 0.96) !important;
  }

  .el-image-viewer__btn {
    background-color: rgba(255, 255, 255, 0.04) !important;
    border: 1px solid rgba(255, 255, 255, 0.08) !important;
    color: #ffffff !important;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    transition: all 0.3s ease;

    &:hover {
      background-color: #c5a880 !important;
      border-color: #c5a880 !important;
      color: #1a1a1a !important;
    }
  }

  .el-image-viewer__close {
    top: 40px;
    right: 40px;
    width: 48px;
    height: 48px;
    font-size: 1.25rem;
  }

  .el-image-viewer__prev,
  .el-image-viewer__next {
    width: 50px;
    height: 50px;
    font-size: 1.375rem;
    border-radius: 50%;
  }

  .el-image-viewer__actions {
    background-color: rgba(26, 26, 26, 0.9) !important;
    border: 1px solid rgba(197, 168, 128, 0.2) !important;
    border-radius: 30px !important;
    padding: 0.5rem 1.5rem !important;
    bottom: 40px;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);

    .el-image-viewer__actions__inner {
      color: #ffffff;
      font-size: 1rem;
      gap: 1.125rem;

      i, .el-icon {
        cursor: pointer;
        transition: color 0.3s ease;

        &:hover {
          color: #c5a880 !important;
        }
      }
    }
  }
}
</style>

