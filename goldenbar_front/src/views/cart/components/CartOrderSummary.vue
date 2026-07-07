<template>
<div class="checkout-summary-card">
    <div class="memo-form-wrapper">
      <el-form label-position="top">
        <el-form-item label="관리 고객 선택">
          <div style="display: flex; gap: 8px; width: 100%;">
            <el-input
              :model-value="mainCustomerSearch"
              @update:model-value="$emit('update:mainCustomerSearch', $event)"
              placeholder="고객명 또는 연락처 입력 후 엔터"
              style="flex: 1;"
              @keyup.enter="openCustomerDialog"
            >
              <template #suffix v-if="selectedCustomerId">
                <el-button link :icon="Close" @click="handleClearCustomer" style="margin-right: -5px;" />
              </template>
            </el-input>
            <el-button type="primary" class="primary-gold-btn" @click="openCustomerDialog">고객 찾기</el-button>
          </div>
        </el-form-item>
        <el-form-item :label="$t('cart.orderSummary.labels.orderMemo')">
          <el-input
            :model-value="orderMemo"
            @update:model-value="$emit('update:orderMemo', $event)"
            type="textarea"
            :rows="3"
            :placeholder="$t('cart.orderSummary.placeholders.orderMemo')"
            class="luxury-textarea"
          />
        </el-form-item>
        <el-form-item :label="$t('cart.orderSummary.labels.logisticsRemarks')">
          <el-input
            :model-value="requestMemo"
            @update:model-value="$emit('update:requestMemo', $event)"
            type="textarea"
            :rows="3"
            :placeholder="$t('cart.orderSummary.placeholders.logisticsRemarks')"
            class="luxury-textarea"
          />
        </el-form-item>
      </el-form>
    </div>

    <div class="totals-panel">
      <div class="total-row">
        <span class="label">{{ $t('cart.orderSummary.totals.itemsSelected') }}</span>
        <span class="value">{{ selectedItems.length }} {{ $t('cart.orderSummary.totals.itemsUnit') }}</span>
      </div>
      <div class="total-row">
        <span class="label">{{ $t('cart.orderSummary.totals.subtotal') }}</span>
        <span class="value">₩ {{ formatPrice(totalPrice) }}</span>
      </div>
      <div class="grand-total-divider"></div>
      <div class="total-row highlight">
        <span class="label">{{ $t('cart.orderSummary.totals.estimatedTotal') }}</span>
        <span class="value">₩ {{ formatPrice(totalPrice) }}</span>
      </div>

      <div class="checkout-actions d-none-mobile">
        <el-button @click="goToShopping" class="secondary-btn">{{ $t('cart.orderSummary.actions.continueShopping') }}</el-button>
        <el-button
          type="primary"
          class="checkout-btn"
          :disabled="selectedItems.length === 0"
          @click="handleCheckout"
        >
          {{ $t('cart.orderSummary.actions.placeOrder') }}
        </el-button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Close } from '@element-plus/icons-vue';
import { formatPrice } from '@/utils/format';

defineProps({
  selectedItems: {
    type: Array,
    required: true
  },
  totalPrice: {
    type: Number,
    required: true
  },
  mainCustomerSearch: {
    type: String,
    required: true
  },
  selectedCustomerId: {
    type: [Number, null] as any,
    required: true
  },
  orderMemo: {
    type: String,
    required: true
  },
  requestMemo: {
    type: String,
    required: true
  },
  isMobile: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits([
  'update:mainCustomerSearch',
  'update:orderMemo',
  'update:requestMemo',
  'open-customer-dialog',
  'clear-customer',
  'checkout',
  'go-to-shopping'
]);

const openCustomerDialog = () => {
  emit('open-customer-dialog');
};

const handleClearCustomer = () => {
  emit('clear-customer');
};

const handleCheckout = () => {
  emit('checkout');
};

const goToShopping = () => {
  emit('go-to-shopping');
};
</script>

<style lang="scss" scoped>
.checkout-summary-card {
  background-color: #ffffff;
  border: 1px solid #eae6df;
  border-radius: 2px;
  padding: 1.875rem;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.02);
}

.memo-form-wrapper {
  margin-bottom: 1.5625rem;
  .luxury-textarea {
    :deep(.el-textarea__inner) {
      border-color: #eae6df;
      font-family: 'S-CoreDream', sans-serif;
      padding: 0.95rem;
      border-radius: 0;
      &:focus { border-color: #c5a880; }
    }
  }
  :deep(.el-form-item__label) {
    font-size: 0.8875rem;
    font-weight: 700;
    letter-spacing: 1px;
    color: #888888;
  }
}

.totals-panel {
  background-color: #fbfaf7;
  padding: 1.5625rem;
  border-radius: 2px;
  border: 1px solid #f0eeeb;

  .total-row {
    display: flex;
    justify-content: space-between;
    margin-bottom: 0.95rem;
    .label { font-size: 0.9rem; color: #888888; }
    .value { font-size: 0.875rem; font-weight: 600; }

    &.highlight {
      margin-top: 0.95rem;
      .label { font-size: 0.9375rem; font-weight: 700; }
      .value { font-size: 1.5rem; font-weight: 700; color: #c5a880; }
    }
  }

  .grand-total-divider {
    height: 1px;
    background-color: #eae6df;
    margin: 0.9375rem 0;
  }

  .checkout-actions {
    display: flex;
    flex-direction: column;
    gap: 0.625rem;
    margin-top: 1.5625rem;

    .checkout-btn {
      height: 50px;
      background-color: #222222 !important;
      border-color: #222222 !important;
      font-size: 0.875rem;
      font-weight: 700;
      letter-spacing: 2px;
      border-radius: 0;
      &:hover { background-color: #c5a880 !important; border-color: #c5a880 !important; }
    }

    .secondary-btn {
      height: 40px;
      background-color: transparent;
      border: 1px solid #eae6df;
      color: #888888;
      font-size: 0.95rem;
      font-weight: 600;
      letter-spacing: 1px;
      border-radius: 0;
      &:hover { border-color: #c5a880; color: #c5a880; }
    }
  }
}

@media (max-width: 991px) {
  .d-none-mobile { display: none !important; }
  .totals-panel { margin-top: 1.875rem; }
}
</style>

