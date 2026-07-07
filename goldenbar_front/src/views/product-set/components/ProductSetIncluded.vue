<template>
<div class="included-products-section" v-if="products && products.length > 0">
    <div class="section-header">
      <span class="section-label">INCLUDED PRODUCTS</span>
      <h2 class="section-title">{{ $t('orderDetail.componentItems') }}</h2>
      <div class="section-divider"></div>
    </div>

    <el-row :gutter="24" class="products-grid">
      <el-col
        v-for="item in products"
        :key="item.id"
        :xs="24" :sm="12" :md="8" :lg="6"
        class="product-grid-item"
      >
        <div class="jovenca-product-card" @click="goToProductDetail(item.id)">
          <div class="product-image-box">
            <el-image
              v-if="item.photos && item.photos.length > 0"
              :src="item.photos[0].photoUrl"
              fit="cover"
              class="product-img"
            />
            <div v-else class="no-image-box">
              <span>{{ $t('productMarket.labels.noImage') }}</span>
            </div>

            <div class="card-hover-overlay">
              <span class="overlay-hint">{{ $t('productMarket.viewDetails') }}</span>
            </div>
          </div>

          <div class="product-card-info">
            <div class="card-product-no">{{ item.productNo }}</div>
            <div class="card-product-name">{{ item.name }}</div>
            <div class="card-product-spec" v-if="item.weight">
              <span class="spec-weight">{{ item.weight }}</span><span class="spec-unit">g</span>
            </div>
          </div>
        </div>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
defineProps({
  products: {
    type: Array as () => any[],
    required: true
  }
});

const emit = defineEmits(['go-to-product-detail']);

const goToProductDetail = (id: number) => {
  emit('go-to-product-detail', id);
};
</script>

<style lang="scss" scoped>
.included-products-section {
  margin-top: 3.75rem;
  padding-top: 2.5rem;
  border-top: 1px solid #eaeaea;

  .section-header {
    text-align: center;
    margin-bottom: 2.5rem;

    .section-label {
      display: block;
      font-size: 0.8875rem;
      font-weight: 700;
      color: #c5a880;
      letter-spacing: 3px;
      text-transform: uppercase;
      margin-bottom: 0.5rem;
    }

    .section-title {
      font-size: 1.5rem;
      font-weight: 600;
      color: #1a1a1a;
      margin: 0 0 1rem 0;
      letter-spacing: -0.3px;
    }

    .section-divider {
      width: 40px;
      height: 1px;
      background-color: #c5a880;
      margin: 0 auto;
    }
  }

  .products-grid {
    margin: 0 !important;
  }

  .product-grid-item {
    margin-bottom: 2rem;
  }

  .jovenca-product-card {
    cursor: pointer;
    position: relative;

    .product-image-box {
      position: relative;
      width: 100%;
      padding-bottom: 120%;
      overflow: hidden;
      background-color: #f8f8f8;

      .product-img {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        transition: transform 0.6s ease;

        :deep(img) {
          object-fit: cover;
          width: 100%;
          height: 100%;
        }
      }

      .no-image-box {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        display: flex;
        align-items: center;
        justify-content: center;
        color: #ccc;
        font-size: 0.95rem;
        letter-spacing: 1px;
        background-color: #f5f5f5;
      }

      .card-hover-overlay {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background-color: rgba(26, 26, 26, 0.45);
        display: flex;
        align-items: center;
        justify-content: center;
        opacity: 0;
        transition: opacity 0.3s ease;

        .overlay-hint {
          font-size: 0.95rem;
          font-weight: 700;
          color: #ffffff;
          letter-spacing: 2px;
          text-transform: uppercase;
          border: 1px solid rgba(255, 255, 255, 0.6);
          padding: 0.5rem 1.125rem;
        }
      }
    }

    &:hover {
      .product-image-box {
        .product-img {
          transform: scale(1.05);
        }
        .card-hover-overlay {
          opacity: 1;
        }
      }
      .product-card-info {
        .card-product-name {
          color: #c5a880;
        }
      }
    }

    .product-card-info {
      padding: 0.875rem 0.25rem 0;

      .card-product-no {
        font-size: 0.825rem;
        font-weight: 600;
        color: #c5a880;
        letter-spacing: 1.5px;
        text-transform: uppercase;
        margin-bottom: 0.3125rem;
        font-family: monospace;
      }

      .card-product-name {
        font-size: 0.875rem;
        font-weight: 500;
        color: #1a1a1a;
        line-height: 1.4;
        margin-bottom: 0.375rem;
        transition: color 0.3s ease;
        overflow: hidden;
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
        text-overflow: ellipsis;
      }

      .card-product-spec {
        display: inline-flex;
        align-items: baseline;
        gap: 0.125rem;

        .spec-weight {
          font-size: 0.9375rem;
          font-weight: 700;
          color: #333;
        }

        .spec-unit {
          font-size: 0.825rem;
          font-weight: 500;
          color: #aaa;
        }
      }
    }
  }
}
</style>

