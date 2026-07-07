<template>
<div class="set-info-section">

    <div class="meta-info-row">
      <span class="company-name">{{ productSet.companyName || '' }}</span>
      <div class="right-meta">
        <span class="set-badge">SET PRODUCT</span>
        <span class="set-no-label" v-if="productSet.setNo">NO. {{ productSet.setNo }}</span>
      </div>
    </div>

    <div class="set-title-row-detail" style="display: flex; align-items: center; justify-content: space-between; gap: 1rem; width: 100%;">
      <h1 class="set-title" style="margin: 0;">{{ productSet.title }}</h1>
      <el-button
        v-if="isMfgUser"
        type="primary"
        size="small"
        :icon="Edit"
        @click="handleEdit"
      >
        세트제품수정
      </el-button>
    </div>

    <div class="divider-thin"></div>

    <div class="info-grid">
      <div class="info-row">
        <span class="value composition-count">
          <span class="weight-number">{{ productSet.products?.length || 0 }}</span>
          <span class="weight-unit">{{ $t('productMarket.labels.componentsSet') }}</span>
        </span>
      </div>
    </div>

    <div class="price-section">
      <div class="price-value">
        <span class="currency-symbol">₩</span>
        <span class="price-amount">{{ formatPrice((productSet.factoryPrice || 0) + (productSet.laborCost || 0)) }}</span>
      </div>

      <div class="set-price-details-luxury">
        <div class="price-detail-row">
          <span class="label">{{ $t('productDetail.labels.factoryPrice') || '공장도가' }}</span>
          <span class="val">₩{{ formatPrice(productSet.factoryPrice || 0) }}</span>
        </div>
        <div class="price-detail-row">
          <span class="label">{{ $t('productDetail.labels.laborCost') || '수공비' }}</span>
          <span class="val">₩{{ formatPrice(productSet.laborCost || 0) }}</span>
        </div>
      </div>
    </div>

    <div class="divider-thin"></div>

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
        <el-button type="primary" class="btn-secondary btn-jovenca" :disabled="!isRetailUser" @click="isRetailUser && handleCartAll()">{{ $t('productDetail.labels.addToCart') }}</el-button>
        <el-button type="primary" class="btn-primary btn-jovenca" :disabled="!isRetailUser" @click="isRetailUser && handleBuyAll()">{{ $t('productDetail.labels.buyNow') }}</el-button>
      </div>
    </div>

    <div v-if="!isRetailUser" class="non-retail-alert">
      <el-icon class="alert-icon"><InfoFilled /></el-icon>
      <span class="alert-desc">{{ $t('productDetail.messages.retailOnly') }}</span>
    </div>

    <el-collapse
      :model-value="activeCollapseNames"
      @update:model-value="$emit('update:activeCollapseNames', $event)"
      class="jovenca-accordion"
    >
      <el-collapse-item name="description">
        <template #title>
          <span class="accordion-title">{{ $t('productDetail.labels.description') }} (SET DETAILS)</span>
        </template>
        <div class="accordion-body description-text">
          <div v-html="productSet.description || $t('common.noData')"></div>
        </div>
      </el-collapse-item>

      <el-collapse-item name="shipping">
        <template #title>
          <span class="accordion-title">{{ $t('productDetail.labels.shippingInfo') }} (SHIPPING & RETURNS)</span>
        </template>
        <div class="accordion-body shipping-text">
          <p>• <strong>{{ $t('productDetail.shipping.productionPeriod') }}:</strong> {{ $t('productDetail.shipping.productionPeriodDesc') }}</p>
          <p>• <strong>{{ $t('productDetail.shipping.deliveryInfo') }}:</strong> {{ $t('productDetail.shipping.deliveryInfoDesc') }}</p>
          <p>• <strong>{{ $t('productDetail.shipping.returns') }}:</strong> {{ $t('productDetail.shipping.returnsDesc') }}</p>
        </div>
      </el-collapse-item>
    </el-collapse>
  </div>
</template>

<script setup lang="ts">
import { Edit, Star, StarFilled, InfoFilled } from '@element-plus/icons-vue';
import { formatPrice } from '@/utils/format';

defineProps({
  productSet: {
    type: Object,
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
  isMfgUser: {
    type: Boolean,
    required: true
  },
  activeCollapseNames: {
    type: Array as () => string[],
    required: true
  }
});

const emit = defineEmits([
  'update:quantity',
  'update:activeCollapseNames',
  'edit',
  'favorite',
  'cart-all',
  'buy-all'
]);

const handleEdit = () => { emit('edit'); };
const handleFavorite = () => { emit('favorite'); };
const handleCartAll = () => { emit('cart-all'); };
const handleBuyAll = () => { emit('buy-all'); };
</script>

<style lang="scss" src="./ProductSetOptionsExtraStyles.scss" scoped></style>

