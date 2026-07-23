<template>
  <div class="product-detail-container app-container">
    <el-card v-loading="loading" shadow="never" class="premium-detail-card">
      <div v-if="product.id">
        <el-row :gutter="50" class="product-main-row">

          <el-col :span="11" :xs="24" :sm="11">
            <product-gallery
              :product="product"
              :default-image="defaultImage"
            />
          </el-col>

          <el-col :span="13" :xs="24" :sm="13">
            <product-options
              v-model:selected-purity="selectedPurity"
              v-model:selected-color="selectedColor"
              v-model:quantity="quantity"
              v-model:selected-retailer-id="selectedRetailerId"
              v-model:selected-handler-id="selectedHandlerId"
              v-model:memo="orderMemo"
              v-model:order-size="orderSize"
              :employee-list="employeeList"
              :product="product"
              :active-weight="activeWeight"
              :has-option-weight="hasOptionWeight"
              :total-price="totalPrice"
              :is-favorite="isFavorite"
              :is-retail-user="isRetailUser"
              :is-retail-but-no-logistics="isRetailButNoLogistics"
              :is-mfg-user="isMfgUser"
              :is-logistics-user="isLogisticsUser"
              :retailers-list="retailersList"
              :purity-options="purityOptions"
              :color-options="colorOptions"
              :category-names="categoryNames"
              :code-map="codeMap"
              @edit="handleEdit"
              @favorite="handleFavorite"
              @cart="handleCart"
              @buy="handleBuy"
              @register-stock="handleRegisterStock"
            />

            <product-specs
              v-model:active-collapse-names="activeCollapseNames"
              :product="product"
              :stone-data="stoneData"
            />
          </el-col>
        </el-row>
      </div>
      <el-empty v-else description="제품 정보를 찾을 수 없습니다." />

      <ProductDialog
        v-model="dialogFormVisible"
        :product-id="currentProductId"
        @saved="getDetail"
      />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, reactive, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import store from '@/store';
import useUserStore from '@/store/modules/user';
import { getProduct } from '@/api/product';
import { addToCart } from '@/api/cart';
import { createStock } from '@/api/stock';
import { addFavorite, removeFavoriteByProduct, getMyFavorites } from '@/api/favorite';
import { getRetailersByCenter, getCompanyUsers } from '@/api/company';
import { createOrder } from '@/api/order';
import { ElMessage, ElNotification } from 'element-plus';
import useCodeStore from '@/store/modules/code';
import ProductDialog from '@/components/ProductDialog/index.vue';
import ProductGallery from './components/ProductGallery.vue';
import ProductOptions from './components/ProductOptions.vue';
import ProductSpecs from './components/ProductSpecs.vue';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();

const loading = ref(true);
const userStore = useUserStore();
const codeStore = useCodeStore();
const product = ref<any>({});
const quantity = ref(1);
const isFavorite = ref(false);
const activeCollapseNames = ref(['description']);
const defaultImage = 'https://via.placeholder.com/600x600?text=No+Image';

const isRetailUser = computed(() => {
  return userStore.companyType === 'RTL' && !!userStore.logisticsCompanyId;
});

const isRetailButNoLogistics = computed(() => {
  return userStore.companyType === 'RTL' && !userStore.logisticsCompanyId;
});

const isMfgUser = computed(() => {
  return userStore.companyType === 'MFG';
});

const isLogisticsUser = computed(() => {
  return userStore.companyType === 'LOG' || userStore.companyType === 'DCC';
});

const retailersList = ref<any[]>([]);
const selectedRetailerId = ref<number | null>(null);
const employeeList = ref<any[]>([]);
const selectedHandlerId = ref<number | null>(null);

const fetchRetailers = async () => {
  if (isLogisticsUser.value && userStore.companyId) {
    try {
      const res = await getRetailersByCenter(userStore.companyId);
      retailersList.value = res.data || [];
      selectedRetailerId.value = null;
    } catch (error) {
      console.error('Failed to fetch retailers:', error);
    }
    try {
      const empRes = await getCompanyUsers(userStore.companyId);
      employeeList.value = empRes.data || [];
    } catch (error) {
      console.error('Failed to fetch employees:', error);
    }
  }
};

const dialogFormVisible = ref(false);
const currentProductId = ref<number | null>(null);

const handleEdit = () => {
  currentProductId.value = product.value.id;
  dialogFormVisible.value = true;
};

const selectedPurity = ref('');
const selectedColor = ref('');
const orderMemo = ref('');
const orderSize = ref('');

const activeWeight = computed(() => {
  if (!product.value || !selectedPurity.value || !selectedColor.value) {
    return product.value?.weight || 0;
  }

  if (product.value.optionWeights && product.value.optionWeights.length > 0) {
    const matched = product.value.optionWeights.find(
      (ow: any) => ow.purity === selectedPurity.value && ow.color === selectedColor.value
    );
    if (matched) {
      return matched.weight;
    }
  }

  return product.value.weight || 0;
});

const hasOptionWeight = computed(() => {
  if (!product.value || !selectedPurity.value || !selectedColor.value) return false;
  if (!product.value.optionWeights || product.value.optionWeights.length === 0) return false;
  return product.value.optionWeights.some(
    (ow: any) => ow.purity === selectedPurity.value && ow.color === selectedColor.value
  );
});

const totalPrice = computed(() => {
  const factoryPrice = product.value?.factoryPrice || 0;
  const laborCost = product.value?.laborCost || 0;
  return (factoryPrice + laborCost) * quantity.value;
});

const purityOptions = computed(() => {
  return product.value.purity ? product.value.purity.split(',') : [];
});

const colorOptions = computed(() => {
  return product.value.colors ? product.value.colors.split(',') : [];
});

const categoryNames = reactive({
  large: '',
  medium: '',
  small: '',
  purity: ''
});

const codeMap = computed(() => codeStore.codeMap);

const stoneData = computed(() => {
  const data: any[] = [];

  if (product.value.centerStone || product.value.centerStoneSize || product.value.centerStoneCount) {
    data.push({
      type: '중앙석',
      name: codeMap.value[product.value.centerStone] || product.value.centerStone || '-',
      shape: codeMap.value[product.value.centerStoneShape] || product.value.centerStoneShape || '-',
      size: product.value.centerStoneSize || '-',
      count: product.value.centerStoneCount || 0
    });
  }

  if (product.value.sideStone || product.value.sideStoneSize || product.value.sideStoneCount) {
    data.push({
      type: '보조석',
      name: codeMap.value[product.value.sideStone] || product.value.sideStone || '-',
      shape: codeMap.value[product.value.sideStoneShape] || product.value.sideStoneShape || '-',
      size: product.value.sideStoneSize || '-',
      count: product.value.sideStoneCount || 0
    });
  }

  return data;
});

const checkFavorite = async () => {
  try {
    const res = await getMyFavorites();
    const favorites = res.data || [];
    isFavorite.value = favorites.some((f: any) => f.productId === product.value.id);
  } catch (error) {
    console.error('Failed to check favorite status:', error);
  }
};

const getDetail = async () => {
  const id = route.params.id as string;
  if (!id) return;

  loading.value = true;
  try {
    const [res] = await Promise.all([
      getProduct(parseInt(id)),
      codeStore.fetchCodes()
    ]);

    product.value = res.data;

    categoryNames.large = codeMap.value[product.value.categoryLarge] || product.value.categoryLarge;
    categoryNames.medium = codeMap.value[product.value.categoryMedium] || product.value.categoryMedium;
    categoryNames.small = codeMap.value[product.value.categorySmall] || product.value.categorySmall;
    categoryNames.purity = codeMap.value[product.value.purity] || product.value.purity;

    if (purityOptions.value.length > 0) {
      selectedPurity.value = purityOptions.value[0];
    }
    if (colorOptions.value.length > 0) {
      selectedColor.value = colorOptions.value[0];
    }

    await checkFavorite();

    if (stoneData.value && stoneData.value.length > 0) {
      if (!activeCollapseNames.value.includes('gemstone')) {
        activeCollapseNames.value.push('gemstone');
      }
    }

    if (product.value.name) {
      await nextTick();
      const title = `${product.value.name}|${product.value.productNo || ''}`;
      store.tagsView().updateVisitedViewTitle(route.fullPath, title);
    }
  } catch (error) {
    console.error(error);
    ElMessage.error(t('productDetail.messages.loadError'));
  } finally {
    loading.value = false;
  }
};

const handleCart = async () => {
  if (!selectedPurity.value || !selectedColor.value) {
    ElMessage.warning(t('productDetail.messages.selectOptions'));
    return;
  }

  try {
    await addToCart({
      productId: product.value.id,
      quantity: quantity.value,
      purity: selectedPurity.value,
      color: selectedColor.value,
      size: orderSize.value || product.value.sizes || undefined,
      memo: orderMemo.value || undefined,
      targetCompanyId: isLogisticsUser.value ? selectedRetailerId.value : undefined
    });

    if (store.cart && typeof store.cart === 'function') {
      store.cart().fetchCart();
    }

    if (isLogisticsUser.value) {
      const selectedRet = retailersList.value.find(r => r.id === selectedRetailerId.value);
      ElMessage.success(`${selectedRet?.name || '소매점'}의 장바구니에 상품을 담았습니다.`);
      return;
    }

    let seconds = 3;
    const notification = ElNotification({
      title: t('productDetail.messages.cartSuccess'),
      dangerouslyUseHTMLString: true,
      message: `<div id="cart-notif-msg">${t('productDetail.messages.cartSuccessDesc')} <br> <strong>${seconds}</strong>초 후 사라집니다. <br><br> <span style="color: #409eff; font-weight: bold; text-decoration: underline; cursor: pointer;">[장바구니로 이동]</span></div>`,
      duration: 3000,
      onClick: () => {
        router.push('/my/cart');
        notification.close();
      }
    });

    const timer = setInterval(() => {
      seconds--;
      const el = document.querySelector('#cart-notif-msg');
      if (el) {
        el.innerHTML = `${t('productDetail.messages.cartSuccessDesc')} <br> <strong>${seconds}</strong>초 후 사라집니다. <br><br> <span style="color: #409eff; font-weight: bold; text-decoration: underline; cursor: pointer;">[장바구니로 이동]</span>`;
      }
      if (seconds <= 0) clearInterval(timer);
    }, 1000);
  } catch (error) {
    console.error(error);
    ElMessage.error(t('marketplace.messages.cartError'));
  }
};

const handleFavorite = async () => {
  try {
    if (isFavorite.value) {
      await removeFavoriteByProduct({ productId: product.value.id });
      isFavorite.value = false;
      ElMessage.success(t('productDetail.messages.favoriteRemoved'));
    } else {
      await addFavorite({ productId: product.value.id });
      isFavorite.value = true;
      ElMessage.success(t('productDetail.messages.favoriteAdded'));
    }
  } catch (error) {
    console.error('Failed to toggle favorite:', error);
    ElMessage.error(t('productDetail.messages.favoriteError'));
  }
};

const handleBuy = async () => {
  if (!selectedPurity.value || !selectedColor.value) {
    ElMessage.warning(t('productDetail.messages.selectOptions'));
    return;
  }

  try {
    if (isLogisticsUser.value) {
      await createOrder({
        directProductId: product.value.id,
        directQuantity: quantity.value,
        directPurity: selectedPurity.value,
        directColor: selectedColor.value,
        directSize: orderSize.value || product.value.sizes || undefined,
        directMemo: orderMemo.value || undefined,
        targetCompanyId: selectedRetailerId.value,
        handledByUserId: selectedHandlerId.value,
        orderMemo: '물류사 대리 전화주문'
      });
      ElMessage.success('대리 주문이 성공적으로 등록되었습니다.');
      router.push('/order/order-tracking');
      return;
    }

    await addToCart({
      productId: product.value.id,
      quantity: quantity.value,
      purity: selectedPurity.value,
      color: selectedColor.value,
      size: orderSize.value || product.value.sizes || undefined,
      memo: orderMemo.value || undefined
    });
    router.push({ path: '/my/cart', query: { buyNowProductId: product.value.id }});
  } catch (error) {
    console.error(error);
    ElMessage.error(t('marketplace.messages.cartError'));
  }
};

const handleRegisterStock = async () => {
  if (!selectedPurity.value || !selectedColor.value) {
    ElMessage.warning(t('productDetail.messages.selectOptions'));
    return;
  }

  try {
    await createStock({
      productId: product.value.id,
      purity: selectedPurity.value,
      color: selectedColor.value,
      size: orderSize.value || product.value.sizes || undefined,
      quantity: quantity.value,
      actualWeight: activeWeight.value,
      note: orderMemo.value || undefined
    });
    ElMessage.success('재고에 등록되었습니다.');
  } catch (error) {
    console.error(error);
    ElMessage.error('재고 등록에 실패했습니다.');
  }
};

onMounted(() => {
  getDetail();
  fetchRetailers();
});
</script>

<style lang="scss" src="./ProductDetailStyles.scss" scoped></style>
