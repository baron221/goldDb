<template>
<div class="product-list-wrapper">
    <div v-if="products.length === 0" class="no-products-state">
      <el-icon :size="40" color="#dfdcd6"><InfoFilled /></el-icon>
      <p>현재 페이지에 할당된<br>연결 제품이 없습니다.</p>
    </div>

    <div
      v-for="prod in products"
      :key="prod.id"
      class="market-product-card"
    >
      <div class="prod-details">
        <span class="prod-no">{{ prod.productNo }}</span>
        <h5 class="prod-name">{{ prod.name }}</h5>
      </div>
      <el-button
        type="primary"
        class="add-cart-btn"
        @click="emitAddToCart(prod)"
      >
        <el-icon><Plus /></el-icon> 담기
      </el-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { InfoFilled, Plus } from '@element-plus/icons-vue';

defineProps<{
  products: any[];
}>();

const emit = defineEmits(['add-to-cart']);

const emitAddToCart = (prod: any) => {
  emit('add-to-cart', prod);
};
</script>

