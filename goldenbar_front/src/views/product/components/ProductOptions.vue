<template>
<div class="product-info-section">

    <div class="meta-info-row">
      <span class="company-name">{{ product.companyName || 'GOLDEN BAR' }}</span>
      <div class="right-meta">
        <div class="category-path mini">
          <span v-if="categoryNames.large" class="category-tag">{{ categoryNames.large }}</span>
          <span v-if="categoryNames.medium" class="category-separator">/</span>
          <span v-if="categoryNames.medium" class="category-tag">{{ categoryNames.medium }}</span>
          <span v-if="categoryNames.small" class="category-separator">/</span>
          <span v-if="categoryNames.small" class="category-tag">{{ categoryNames.small }}</span>
        </div>
        <span class="product-no">NO. {{ product.productNo }}</span>
      </div>
    </div>

    <div class="product-title-row-detail" style="display: flex; align-items: center; justify-content: space-between; gap: 1rem; width: 100%;">
      <h1 class="product-title" style="margin: 0;">{{ product.name }}</h1>
      <el-button
        v-if="isMfgUser"
        type="primary"
        size="small"
        :icon="Edit"
        @click="handleEdit"
      >
        제품 수정
      </el-button>
    </div>

    <div class="divider-thin"></div>

    <div class="option-section" v-if="product.id">
      <div class="option-row" v-if="purityOptions.length > 1 || (purityOptions.length === 1 && purityOptions[0] !== 'EMPTY')">
        <span class="option-label">{{ $t('productDetail.labels.purity') }}</span>
        <el-radio-group
          :model-value="selectedPurity"
          @update:model-value="$emit('update:selectedPurity', $event)"
          class="jovenca-pills"
          :disabled="!isRetailUser"
        >
          <el-radio-button
            v-for="code in purityOptions"
            :key="code"
            :value="code"
          >
            {{ codeMap[code] || code }}
          </el-radio-button>
        </el-radio-group>
      </div>
      <div class="option-row" v-if="colorOptions.length > 1 || (colorOptions.length === 1 && colorOptions[0] !== 'EMPTY')">
        <span class="option-label">{{ $t('productDetail.labels.color') }}</span>
        <el-radio-group
          :model-value="selectedColor"
          @update:model-value="$emit('update:selectedColor', $event)"
          class="jovenca-pills"
          :disabled="!isRetailUser"
        >
          <el-radio-button
            v-for="code in colorOptions"
            :key="code"
            :value="code"
          >
            {{ codeMap[code] || code }}
          </el-radio-button>
        </el-radio-group>
      </div>
    </div>

    <div class="divider-thin"></div>

    <div class="info-grid">
      <div class="info-row">
        <span class="value weight-display" :class="{ 'highlight-gold': hasOptionWeight }">
          <span class="weight-number">{{ activeWeight }}</span><span class="weight-unit">g</span>
          <span v-if="hasOptionWeight" class="option-weight-badge">{{ $t('orderDetail.headers.orderWeight') }}</span>
        </span>
      </div>
    </div>

    <div class="price-section">
      <div class="price-value">
        <span class="currency-symbol">₩</span><span class="price-amount">{{ formatPrice(totalPrice) }}</span>
      </div>
    </div>

    <div class="action-section" :class="{ 'is-disabled': !isRetailUser }">
      <div class="quantity-input-row">
        <span class="qty-label">{{ $t('productDetail.labels.qty') }}</span>
        <el-input-number
          :model-value="quantity"
          @update:model-value="$emit('update:quantity', $event)"
          :min="1"
          class="jovenca-qty"
          :disabled="!isRetailUser"
        />
      </div>
      <div class="button-group">
        <el-button
          :type="isFavorite ? 'warning' : 'info'"
          class="btn-favorite"
          :class="{ 'is-active': isFavorite }"
          circle
          :disabled="!isRetailUser"
          @click="isRetailUser && handleFavorite()"
          :title="$t('favorite.title')"
        >
          <el-icon v-if="isFavorite"><StarFilled /></el-icon>
          <el-icon v-else><Star /></el-icon>
        </el-button>
        <el-button type="primary" class="btn-secondary btn-jovenca" :disabled="!isRetailUser" @click="isRetailUser && handleCart()">{{ $t('productDetail.labels.addToCart') }}</el-button>
        <el-button type="primary" class="btn-primary btn-jovenca" :disabled="!isRetailUser" @click="isRetailUser && handleBuy()">{{ $t('productDetail.labels.buyNow') }}</el-button>
      </div>
    </div>

    <div v-if="isRetailButNoLogistics" class="non-retail-alert">
      <el-icon class="alert-icon"><InfoFilled /></el-icon>
      <span class="alert-desc">관리 물류업체가 지정되지 않았습니다. 주문 및 장바구니 이용을 위해 관리자에게 물류업체 지정을 요청해 주세요.</span>
    </div>
    <div v-else-if="!isRetailUser" class="non-retail-alert">
      <el-icon class="alert-icon"><InfoFilled /></el-icon>
      <span class="alert-desc">{{ $t('productDetail.messages.retailOnly') }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Edit, Star, StarFilled, InfoFilled } from '@element-plus/icons-vue';
import { formatPrice } from '@/utils/format';

defineProps({
  product: {
    type: Object,
    required: true
  },
  selectedPurity: {
    type: String,
    required: true
  },
  selectedColor: {
    type: String,
    required: true
  },
  activeWeight: {
    type: Number,
    required: true
  },
  hasOptionWeight: {
    type: Boolean,
    required: true
  },
  totalPrice: {
    type: Number,
    required: true
  },
  quantity: {
    type: Number,
    required: true
  },
  isFavorite: {
    type: Boolean,
    required: true
  },
  isRetailUser: {
    type: Boolean,
    required: true
  },
  isRetailButNoLogistics: {
    type: Boolean,
    required: true
  },
  isMfgUser: {
    type: Boolean,
    required: true
  },
  purityOptions: {
    type: Array as () => string[],
    required: true
  },
  colorOptions: {
    type: Array as () => string[],
    required: true
  },
  categoryNames: {
    type: Object,
    required: true
  },
  codeMap: {
    type: Object,
    required: true
  }
});

const emit = defineEmits([
  'update:selectedPurity',
  'update:selectedColor',
  'update:quantity',
  'edit',
  'favorite',
  'cart',
  'buy'
]);

const handleEdit = () => {
  emit('edit');
};

const handleFavorite = () => {
  emit('favorite');
};

const handleCart = () => {
  emit('cart');
};

const handleBuy = () => {
  emit('buy');
};
</script>

<style lang="scss" scoped>
@import "./ProductOptionsStyles.scss";
</style>

