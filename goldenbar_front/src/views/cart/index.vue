<template>
<div class="cart-page-luxury">

    <div class="dashboard-header-banner">
      <div class="header-content-wrapper">
        <div class="page-title-area">
          <h1 class="display-name">{{ $t('cart.title') }}</h1>
          <p class="dashboard-title">{{ $t('cart.subtitle') }}</p>
        </div>
        <div class="header-actions">
          <el-button link :icon="Delete" @click="handleClearCart" :disabled="cartItems.length === 0" class="clear-btn">{{ $t('cart.headerActions.emptyCart') }}</el-button>
        </div>
      </div>
    </div>

    <div class="dashboard-body-content">
      <div class="cart-layout-container">

        <div class="cart-main-content">
          <section class="section-container">
            <div class="section-header">
              <h2 class="section-title">{{ $t('cart.itemList.title') }}</h2>
            </div>

            <cart-item-list
              ref="itemListRef"
              :cart-items="cartItems"
              :loading="loading"
              :user-store="userStore"
              :code-map="codeMap"
              @selection-change="handleSelectionChange"
              @go-to-product-detail="goToProductDetail"
              @go-to-set-detail="goToSetDetail"
              @go-to-shopping="goToShopping"
              @refresh="getCartList"
            />
          </section>
        </div>

        <div v-if="cartItems.length > 0" class="cart-sidebar-content">
          <section class="section-container">
            <div class="section-header">
              <h2 class="section-title">{{ $t('cart.orderSummary.title') }}</h2>
            </div>

            <cart-order-summary
              v-model:main-customer-search="mainCustomerSearch"
              v-model:order-memo="orderMemo"
              v-model:request-memo="requestMemo"
              :selected-items="selectedItems"
              :total-price="totalPrice"
              :selected-customer-id="selectedCustomerId"
              :is-mobile="false"
              @open-customer-dialog="openCustomerDialog"
              @clear-customer="handleClearCustomer"
              @checkout="handleCheckout"
              @go-to-shopping="goToShopping"
            />
          </section>
        </div>
      </div>
    </div>
  </div>

  <div v-if="cartItems.length > 0" class="mobile-floating-checkout d-none-desktop">
    <div class="floating-totals">
      <div class="floating-label">총 <span class="highlight-text">{{ selectedItems.length }}</span>건 결제금액</div>
      <div class="floating-value">₩ {{ formatPrice(totalPrice) }}</div>
    </div>
    <el-button
      type="primary"
      class="checkout-btn"
      :disabled="selectedItems.length === 0"
      @click="handleCheckout"
    >
      {{ $t('cart.orderSummary.actions.placeOrder') }}
    </el-button>
  </div>

  <customer-select-dialog
    v-model="customerDialogVisible"
    :initial-search="mainCustomerSearch"
    @select="handleSelectCustomer"
  />
</template>

<script setup lang="ts">

import { useMobile } from '@/hooks/useMobile';
import { ref, onMounted, computed, nextTick } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import store from '@/store';
import { clearCart } from '@/api/cart';
import { createOrder } from '@/api/order';
import type { Customer } from '@/types/customer';
import { formatPrice } from '@/utils/format';
import { ElMessage, ElMessageBox } from 'element-plus';
import { Delete } from '@element-plus/icons-vue';
import useUserStore from '@/store/modules/user';
import useCodeStore from '@/store/modules/code';
import CustomerSelectDialog from '@/views/sys/components/CustomerSelectDialog.vue';
import CartItemList from './components/CartItemList.vue';
import CartOrderSummary from './components/CartOrderSummary.vue';

const { isMobile } = useMobile();
const router = useRouter();
const { t } = useI18n();
const userStore = useUserStore();
const codeStore = useCodeStore();

const loading = ref(true);
const itemListRef = ref();
const cartItems = computed(() => store.cart()?.items || []);
const selectedItems = ref<any[]>([]);
const codeMap = computed(() => codeStore.codeMap);

const orderMemo = ref('');
const requestMemo = ref('');
const selectedCustomerId = ref<number | null>(null);
const mainCustomerSearch = ref('');
const customerDialogVisible = ref(false);

const openCustomerDialog = (searchVal?: string) => {
  if (typeof searchVal === 'string') {
    mainCustomerSearch.value = searchVal;
  }
  customerDialogVisible.value = true;
};

const handleSelectCustomer = (customer: Customer) => {
  selectedCustomerId.value = customer.id;
  mainCustomerSearch.value = customer.name;
  customerDialogVisible.value = false;
};

const handleClearCustomer = () => {
  selectedCustomerId.value = null;
  mainCustomerSearch.value = '';
};

const totalPrice = computed(() => {
  return selectedItems.value.reduce((sum, item) => sum + (item.price * item.quantity), 0);
});

const getCartList = async () => {
  loading.value = true;
  try {
    await Promise.all([
      store.cart().fetchCart(),
      codeStore.fetchCodes()
    ]);

    if (cartItems.value && cartItems.value.length > 0) {
      cartItems.value.forEach(row => {
        if (row.customFactoryPrice === null || row.customFactoryPrice === undefined) {
          row.customFactoryPrice = row.factoryPrice;
        }
        if (row.customLaborCost === null || row.customLaborCost === undefined) {
          row.customLaborCost = row.laborCost;
        }
      });
    }

    await nextTick();
    if (itemListRef.value && cartItems.value.length > 0) {
      const buyNowProductId = router.currentRoute.value.query.buyNowProductId;

      cartItems.value.forEach(row => {
        if (buyNowProductId) {
          const isTarget = row.productId === parseInt(buyNowProductId as string);
          itemListRef.value.toggleRowSelection(row, isTarget);
        } else {
          itemListRef.value.toggleRowSelection(row, true);
        }
      });
    }
  } catch (error) {
    console.error(error);
  } finally {
    loading.value = false;
  }
};

const handleSelectionChange = (val: any[]) => {
  selectedItems.value = val;
};

const handleClearCart = () => {
  ElMessageBox.confirm(t('cart.messages.confirmClear'), t('cart.messages.confirmClearTitle'), { type: 'warning' }).then(async () => {
    await clearCart();
    ElMessage.success(t('cart.messages.clearSuccess'));
    getCartList();
  });
};

const goToProductDetail = (id: number) => {
  router.push(`/prd/product-detail/${id}`);
};

const goToSetDetail = (id: number) => {
  router.push(`/prd/product-set-detail/${id}`);
};

const goToShopping = () => {
  router.push('/prd/pro_market');
};

const handleCheckout = () => {
  if (selectedItems.value.length === 0) {
    ElMessage.warning(t('cart.messages.selectItems'));
    return;
  }

  ElMessageBox.confirm(t('cart.messages.confirmOrder', { count: selectedItems.value.length }), t('cart.messages.confirmOrderTitle'), {
    confirmButtonText: t('cart.messages.orderBtn'),
    cancelButtonText: t('cart.messages.cancelBtn'),
    type: 'success'
  }).then(async () => {
    try {
      const cartItemIds = selectedItems.value.map(item => item.id);
      await createOrder({
        cartItemIds,
        orderMemo: orderMemo.value,
        logisticsRemarks: requestMemo.value,
        customerId: selectedCustomerId.value || undefined
      });
      await store.cart().fetchCart();
      ElMessage.success(t('cart.messages.orderSuccess') + ' (제조사별로 주문이 분할 생성되었습니다.)');
      router.push('/my/order');
    } catch (error) {
      console.error('Order failed:', error);
      ElMessage.error(t('cart.messages.orderError'));
    }
  });
};

onMounted(() => {
  getCartList();
});
</script>

<style lang="scss" scoped>
@import url('https://fonts.googleapis.com/css2?family=Jost:wght@300;400;500;600;700&display=swap');

.cart-page-luxury {
  font-family: 'S-CoreDream', 'Jost', sans-serif;
  background-color: #fbfaf7;
  min-height: 100vh;
  padding-bottom: 5rem;
}

.dashboard-header-banner {
  background-color: #ffffff;
  border-bottom: 1px solid #eae6df;
  padding: 1.875rem 0;
  margin-bottom: 1.875rem;

  .header-content-wrapper {
    margin: 0 auto;
    padding: 0 1.875rem;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .display-name {
    font-size: 1.5rem;
    font-weight: 600;

    margin: 0 0 0.25rem 0;
    text-transform: uppercase;
    letter-spacing: 1px;
    position: relative;
    padding-left: 0.9375rem;
    &::after {
      content: '';
      position: absolute;
      left: 0;
      top: 0;
      bottom: 0;
      width: 3px;
      background-color: #c5a880;
    }
  }
  .dashboard-title {
    font-size: 0.9rem;
    color: #888888;
    margin: 0;
    font-weight: 300;
    text-transform: uppercase;
    letter-spacing: 2px;
    padding-left: 0.9375rem;
  }
}

.clear-btn {
  font-size: 0.8875rem;
  font-weight: 700;
  letter-spacing: 1px;
  color: #ff4949;
  &:hover { color: #d73a3a; }
  &:disabled { color: #cbcbcb; }
}

.dashboard-body-content {
  margin: 0 auto;
  padding: 0 1.875rem;
}

.cart-layout-container {
  display: flex;
  gap: 1.875rem;
  align-items: flex-start;
}

.cart-main-content {
  flex: 1;
  min-width: 0;
}

.cart-sidebar-content {
  width: 400px;
  flex-shrink: 0;
  position: sticky;
  top: 100px;
  z-index: 10;
}

.section-container {
  margin-bottom: 1.875rem;
}

.section-header {
  margin-bottom: 0.9375rem;
  .section-title {
    font-size: 0.9375rem;
    font-weight: 600;

    text-transform: uppercase;
    letter-spacing: 1px;
    position: relative;
    padding-left: 0.9375rem;
    line-height: 1.2;
    &::after {
      content: '';
      position: absolute;
      left: 0;
      top: 0;
      bottom: 0;
      width: 3px;
      background-color: #c5a880;
    }
  }
}

.mobile-floating-checkout {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  background-color: #ffffff;
  padding: 1rem 1.25rem;
  box-shadow: 0 -4px 20px rgba(0, 0, 0, 0.08);
  z-index: 1000;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
  border-top: 1px solid #eae6df;

  .floating-label { font-size: 0.8rem; color: #888; margin-bottom: 0.2rem; }
  .highlight-text { color: #c5a880; font-weight: 700; }
  .floating-value { font-size: 1.2rem; font-weight: 700; color: #222; }
  .checkout-btn { height: 45px; padding: 0 1.5rem; background-color: #222222 !important; border-color: #222222 !important; font-size: 0.95rem; font-weight: 700; border-radius: 0; }
}

@media (min-width: 992px) {
  .d-none-desktop { display: none !important; }
}

@media (max-width: 991px) {
  .dashboard-header-banner { padding: 1.875rem 0; }
  .dashboard-body-content { padding: 0 1.25rem; }
  .cart-layout-container {
    flex-direction: column;
    align-items: stretch;
  }
  .cart-main-content { width: 100%; }
  .cart-sidebar-content { width: 100%; position: relative; top: auto; }
  .cart-page-luxury { padding-bottom: 7rem; }
}
</style>

