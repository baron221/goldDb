<template>
<el-collapse
    :model-value="activeCollapseNames"
    @update:model-value="$emit('update:activeCollapseNames', $event)"
    class="jovenca-accordion"
  >

    <el-collapse-item name="description">
      <template #title>
        <span class="accordion-title">{{ $t('productDetail.labels.description') }}</span>
      </template>
      <div class="accordion-body description-text">
        <div class="product-spec-notes" v-if="product.productSize || product.specialNote" style="margin-bottom: 1rem; padding-bottom: 0.95rem; border-bottom: 1px dashed #eaeaea;">
          <p v-if="product.productSize" style="margin: 0 0 0.375rem 0; font-size: 0.9rem; color: #444;"><strong style="color: #1a1a1a;">• {{ $t('productDetail.labels.productSize') }}:</strong> {{ product.productSize }}</p>
          <p v-if="product.specialNote" style="margin: 0; font-size: 0.9rem; color: #444;"><strong style="color: #1a1a1a;">• {{ $t('productDetail.labels.specialNote') }}:</strong> {{ product.specialNote }}</p>
        </div>
        <div v-html="product.description || $t('common.noData')"></div>
      </div>
    </el-collapse-item>

    <el-collapse-item name="gemstone" v-if="stoneData && stoneData.length > 0">
      <template #title>
        <span class="accordion-title">{{ $t('productDetail.labels.gemstoneInfo') }}</span>
      </template>
      <div class="accordion-body stone-luxury-content">
        <div v-for="(stone, idx) in stoneData" :key="idx" class="stone-spec-group">
          <div class="stone-header">
            <span class="stone-type">{{ stone.type }}</span>
            <span class="stone-name">{{ stone.name }}</span>
          </div>
          <div class="stone-grid">
            <div class="stone-info-block">
              <span class="info-label">{{ $t('productDetail.labels.gemstoneShape') }}</span>
              <span class="info-value">{{ stone.shape }}</span>
            </div>
            <div class="stone-info-block">
              <span class="info-label">{{ $t('productDetail.labels.gemstoneSize') }}</span>
              <span class="info-value">{{ stone.size }}</span>
            </div>
            <div class="stone-info-block">
              <span class="info-label">{{ $t('productDetail.labels.qty') }}</span>
              <span class="info-value">{{ stone.count }}P</span>
            </div>
          </div>
          <div v-if="idx < stoneData.length - 1" class="stone-inner-divider"></div>
        </div>
      </div>
    </el-collapse-item>

    <el-collapse-item name="shipping">
      <template #title>
        <span class="accordion-title">{{ $t('productDetail.labels.shippingInfo') }}</span>
      </template>
      <div class="accordion-body shipping-luxury-content">
        <div class="shipping-info-item">
          <div class="item-icon-box"><el-icon><Calendar /></el-icon></div>
          <div class="item-text-box">
            <span class="item-label">{{ $t('productDetail.shipping.productionPeriod') }}</span>
            <p class="item-desc">{{ $t('productDetail.shipping.productionPeriodDesc') }}</p>
          </div>
        </div>
        <div class="shipping-info-item">
          <div class="item-icon-box"><el-icon><Van /></el-icon></div>
          <div class="item-text-box">
            <span class="item-label">{{ $t('productDetail.shipping.deliveryInfo') }}</span>
            <p class="item-desc">{{ $t('productDetail.shipping.deliveryInfoDesc') }}</p>
          </div>
        </div>
        <div class="shipping-info-item">
          <div class="item-icon-box"><el-icon><RefreshRight /></el-icon></div>
          <div class="item-text-box">
            <span class="item-label">{{ $t('productDetail.shipping.returns') }}</span>
            <p class="item-desc">{{ $t('productDetail.shipping.returnsDesc') }}</p>
          </div>
        </div>
        <div class="luxury-divider-gold"></div>
        <div class="shipping-footer-note">
          <el-icon><InfoFilled /></el-icon>
          <span>모든 제품은 전문 검수원의 엄격한 품질 확인 후 발송됩니다.</span>
        </div>
      </div>
    </el-collapse-item>
  </el-collapse>
</template>

<script setup lang="ts">
import { Calendar, Van, RefreshRight, InfoFilled } from '@element-plus/icons-vue';

defineProps({
  product: {
    type: Object,
    required: true
  },
  stoneData: {
    type: Array as () => any[],
    required: true
  },
  activeCollapseNames: {
    type: Array as () => string[],
    required: true
  }
});

defineEmits(['update:activeCollapseNames']);
</script>

<style lang="scss" scoped>
.jovenca-accordion {
  border-top: 1px solid #1a1a1a;
  border-bottom: none;
  --el-collapse-header-bg-color: transparent;
  --el-collapse-header-text-color: #1a1a1a;
  --el-collapse-content-bg-color: transparent;

  :global(html.dark) & {
    border-top-color: var(--el-border-color-darker, #363636);
    --el-collapse-header-text-color: var(--el-text-color-primary, #ffffff);
  }

  :deep(.el-collapse-item__header) {
    border-bottom: 1px solid #eaeaea;
    padding: 1.375rem 0.25rem;
    height: auto;
    line-height: 1;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);

    :global(html.dark) & {
      border-bottom-color: var(--el-border-color-dark, #363636);
    }

    &:hover {
      color: #c5a880;
      .accordion-title { color: #c5a880; }
    }

    &.is-active {
      border-bottom-color: #eaeaea;
      padding-bottom: 1rem;

      :global(html.dark) & {
        border-bottom-color: var(--el-border-color-dark, #363636);
      }
      .accordion-title {
        &::before {
          background-color: #c5a880;
          transform: rotate(45deg) scale(1.1);
          opacity: 1;
          margin-right: 12px;
        }
      }
    }

    .el-collapse-item__arrow {
      color: #c5a880;
      font-weight: bold;
    }
  }

  :deep(.el-collapse-item__wrap) {
    border-bottom: none;
  }

  :deep(.el-collapse-item__content) {
    padding: 1.25rem 0.5rem 2.5rem 0.5rem;
    color: #555;

    :global(html.dark) & {
      color: var(--el-text-color-regular, #eaeaea);
    }
  }

  .accordion-title {
    font-family: 'S-CoreDream','Jost', sans-serif;
    font-size: 0.95rem;
    font-weight: 800;
    letter-spacing: 2.5px;
    text-transform: uppercase;
    transition: all 0.4s ease;
    display: flex;
    align-items: center;

    &::before {
      content: '';
      display: inline-block;
      width: 6px;
      height: 6px;
      background-color: #bbbbbb;
      margin-right: 8px;
      transform: rotate(45deg) scale(0.8);
      opacity: 0.4;
      transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
    }
  }

  .accordion-body {
    &.description-text {
      font-size: 0.9rem;
      line-height: 1.9;
      letter-spacing: -0.2px;
    }
  }
}

.stone-luxury-content {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  padding: 0.5rem;

  .stone-spec-group {
    display: flex;
    flex-direction: column;
    gap: 1.125rem;

    .stone-header {
      display: flex;
      align-items: center;
      gap: 0.75rem;

      .stone-type {
        background-color: #1a1a1a;
        color: #ffffff;
        font-size: 0.75rem;
        font-weight: 700;
        padding: 0.2rem 0.5rem;
        letter-spacing: 0.5px;
        border-radius: 1px;

        :global(html.dark) & {
          background-color: var(--el-fill-color-darker, #303030);
          color: var(--el-text-color-primary, #ffffff);
          border: 1px solid var(--el-border-color-darker, #4c4c4c);
        }
      }

      .stone-name {
        font-size: 1.05rem;
        font-weight: 700;
        color: #222;
        font-family: 'S-CoreDream','Jost', sans-serif;

        :global(html.dark) & {
          color: var(--el-text-color-primary, #ffffff);
        }
      }
    }

    .stone-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 1rem;
      padding: 1.125rem;
      border-radius: 2px;

      .stone-info-block {
        display: flex;
        flex-direction: column;
        gap: 0.375rem;

        .info-label {
          font-size: 0.75rem;
          color: #999;
          text-transform: uppercase;
          letter-spacing: 1px;
          font-weight: 600;
        }

        .info-value {
          font-size: 0.9rem;
          color: #333;
          font-weight: 600;
          font-family: 'S-CoreDream','Jost', sans-serif;

          :global(html.dark) & {
            color: var(--el-text-color-regular, #eaeaea);
          }
        }
      }
    }

    .stone-inner-divider {
      height: 1px;
      background-color: #eaeaea;
      margin-top: 0.5rem;

      :global(html.dark) & {
        background-color: var(--el-border-color-dark, #363636);
      }
    }
  }
}

.shipping-luxury-content {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  padding: 1.25rem 0.5rem;

  .shipping-info-item {
    display: flex;
    gap: 1.125rem;
    align-items: flex-start;

    .item-icon-box {
      width: 38px;
      height: 38px;
      background-color: #fbfaf7;
      border: 1px solid #f2efeb;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      color: #c5a880;
      font-size: 1.125rem;
      box-shadow: 0 2px 6px rgba(197, 168, 128, 0.08);

      :global(html.dark) & {
        background-color: var(--el-fill-color-darker, #202020);
        border-color: var(--el-border-color-darker, #363636);
        box-shadow: none;
      }
    }

    .item-text-box {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;

      .item-label {
        font-size: 0.8875rem;
        font-weight: 700;

        letter-spacing: 0.5px;
        text-transform: uppercase;

        :global(html.dark) & {
          color: var(--el-text-color-primary, #ffffff);
        }
      }

      .item-desc {
        font-size: 0.875rem;
        color: #777777;
        line-height: 1.6;
        margin: 0;

        :global(html.dark) & {
          color: var(--el-text-color-secondary, #a8abb2);
        }
      }
    }
  }

  .luxury-divider-gold {
    height: 1px;
    background-color: #eaeaea;
    margin: 0.5rem 0;

    :global(html.dark) & {
      background-color: var(--el-border-color-dark, #363636);
    }
  }

  .shipping-footer-note {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    justify-content: center;
    font-size: 0.8125rem;
    color: #999999;
    font-style: italic;

    :global(html.dark) & {
      color: var(--el-text-color-placeholder, #808080);
    }

    .el-icon {
      color: #c5a880;
      font-size: 0.95rem;
    }
  }
}
</style>

