<template>
<div v-if="orderItems && orderItems.length > 0" class="order-goods-info">
    <div class="goods-info-top">
      <div class="goods-visual-group">
        <div class="goods-thumb-strip">
          <div
            v-for="(item, idx) in orderItems.slice(0, 7)"
            :key="item.id || idx"
            class="goods-thumb-item"
          >
            <el-tooltip placement="top">
              <template #content>
                <div style="line-height: 1.6;">
                  <div style="font-weight: bold; margin-bottom: 0.25rem; border-bottom: 1px solid #666; padding-bottom: 0.125rem;">
                    {{ item.productName || item.productSetTitle || '' }}
                  </div>
                  <div>함량: {{ item.purity || '-' }}</div>
                  <div>컬러: {{ item.color || '-' }}</div>
                  <div>수량: {{ item.quantity || 0 }}개</div>
                </div>
              </template>
              <el-image
                :src="item.photoUrl || '/thumb_no_img.png'"
                fit="cover"
                class="goods-thumb"
                lazy
              >
                <template #error>
                  <div class="goods-thumb-fallback"><i class="fas fa-gem"></i></div>
                </template>
              </el-image>
            </el-tooltip>
          </div>
          <span v-if="orderItems.length > 8" class="goods-thumb-more">+{{ orderItems.length - 8 }}</span>
        </div>
      </div>
      <div class="goods-summary-sub">
        제품:{{ orderItems.length }}종 <br />
        개수:{{ getOrderTotalQuantity(orderItems) }}개
      </div>
    </div>
  </div>
  <span v-else>-</span>
</template>

<script setup lang="ts">

defineProps<{
  orderItems: any[];
}>();

const getOrderTotalQuantity = (items: any[]) => {
  if (!items) return 0;
  return items.reduce((sum, item) => sum + (item.quantity || 0), 0);
};
</script>

<style lang="scss" src="../OrderTableListStyles.scss" scoped></style>

