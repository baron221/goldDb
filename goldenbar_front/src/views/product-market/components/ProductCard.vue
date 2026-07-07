<template>
<div class="jovenca-product-card" @click="emit('click', item)">
    <div class="product-image-box">
      <el-image
        v-if="item.photoUrl || (item.photos && item.photos.photoUrl)"
        :src="getThumbnailUrl(item.photoUrl || item.photos.photoUrl, 'medium') || (item.photoUrl || item.photos.photoUrl)"
        fit="cover"
        class="product-img"
      />
      <el-image
        v-else-if="item.isSet && item.subProductPhotos && item.subProductPhotos.length > 0"
        :src="getThumbnailUrl(item.subProductPhotos[0], 'medium') || item.subProductPhotos[0]"
        fit="cover"
        class="product-img"
      />
      <div v-else class="no-image-box">
        <span>{{ $t('productMarket.labels.noImage') }}</span>
      </div>

      <div v-if="item.isSet" class="set-label-badge">{{ $t('productMarket.labels.setProduct') }}</div>

      <div class="card-hover-actions">
        <el-button
          :type="isFavorite ? 'warning' : 'default'"
          :icon="isFavorite ? StarFilled : Star"
          circle
          class="quick-action-button favorite"
          :class="{ 'is-active': isFavorite }"
          @click.stop="emit('favorite')"
        />
        <el-button
          :type="isInCart ? 'primary' : 'default'"
          :icon="ShoppingCart"
          circle
          class="quick-action-button cart"
          :class="{ 'is-active': isInCart }"
          @click.stop="emit('add-to-cart')"
        />
      </div>

      <div v-if="item.isSet && item.subProductPhotos && item.subProductPhotos.length > 0" class="set-compositions-tray">
        <div v-for="(subPhoto, idx) in item.subProductPhotos.slice(0, 3)" :key="idx" class="tray-photo">
          <el-image :src="getThumbnailUrl(subPhoto) || subPhoto" fit="cover" />
        </div>
        <div v-if="item.subProductPhotos.length > 3" class="tray-more-count">
          +{{ item.subProductPhotos.length - 3 }}
        </div>
      </div>
    </div>

    <div class="product-details-box">
      <div class="product-title-row">
        <h3 class="product-name-title" :title="item.name || item.title">{{ item.name || item.title }}</h3>
        <div class="manufacturer-badge" v-if="item.companyName">
          {{ item.companyName }}
        </div>
        <div class="category-meta-wrap" :title="getCategoryName()">
          <template v-for="(part, i) in getCategoryParts()" :key="i">
            <span class="category-part">{{ part }}</span>
            <i v-if="i < getCategoryParts().length - 1" class="fas fa-chevron-right category-sep"></i>
          </template>
        </div>
      </div>

      <div class="product-attributes-footer">
        <div class="attr-badges">
          <template v-if="item.purity">
            <el-tag
              v-for="p in item.purity.split(',')"
              :key="p"
              size="small"
              effect="plain"
              class="attr-tag"
            >
              {{ p.trim() }}
            </el-tag>
          </template>
        </div>
        <div class="product-id-label">{{ item.productNo || item.setNo || item.no }}</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Star, StarFilled, ShoppingCart } from '@element-plus/icons-vue';
import { getThumbnailUrl } from '@/utils';
import useCodeStore from '@/store/modules/code';

const props = defineProps<{
  item: any;
  isFavorite: boolean;
  isInCart: boolean;
}>();

const emit = defineEmits(['click', 'favorite', 'add-to-cart']);

const codeStore = useCodeStore();

const getCategoryName = () => {
  const parts = [];
  if (props.item.categoryLarge) parts.push(codeStore.getName(props.item.categoryLarge));
  if (props.item.categoryMedium) parts.push(codeStore.getName(props.item.categoryMedium));
  return parts.join(' > ');
};

const getCategoryParts = () => {
  const parts = [];
  if (props.item.categoryLarge) parts.push(codeStore.getName(props.item.categoryLarge));
  if (props.item.categoryMedium) parts.push(codeStore.getName(props.item.categoryMedium));
  return parts;
};
</script>

<style lang="scss" scoped>
.jovenca-product-card {
   border: 1px solid #f2efeb; position: relative; cursor: pointer; transition: all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1); height: 100%; display: flex; flex-direction: column; overflow: hidden;
  &:hover { transform: translateY(-5px); border-color: #c5a880; box-shadow: 0 10px 30px rgba(197, 168, 128, 0.12);
    .card-hover-actions { opacity: 1; transform: translate(-50%, -50%) scale(1); }
    .product-img { transform: scale(1.08); }
  }
}
.product-image-box { position: relative; width: 100%; padding-top: 100%; overflow: hidden; background-color: #faf9f6; }
.product-img { position: absolute; top: 0; left: 0; width: 100%; height: 100%; transition: transform 0.8s cubic-bezier(0.165, 0.84, 0.44, 1); }
.no-image-box { position: absolute; top: 0; left: 0; width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; color: #bbbbbb; font-size: 0.85rem; font-style: italic; }
.set-label-badge { position: absolute; top: 12px; left: 12px; background: rgba(34, 34, 34, 0.85); color: #ffffff; padding: 3px 8px; font-size: 0.7rem; font-weight: 600; letter-spacing: 0.5px; text-transform: uppercase; z-index: 5; }
.card-hover-actions { position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%) scale(0.9); display: flex; gap: 0.75rem; opacity: 0; transition: all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1); z-index: 10; }
.quick-action-button { width: 42px; height: 42px; border: none; background: #ffffff; box-shadow: 0 4px 15px rgba(0,0,0,0.1);
  &:hover { transform: scale(1.1); color: #c5a880; }
  &.is-active { background: #c5a880; color: #ffffff; }
}
.set-compositions-tray { position: absolute; bottom: 10px; left: 10px; right: 10px; display: flex; gap: 4px; align-items: center; background: rgba(255,255,255,0.7); backdrop-filter: blur(4px); padding: 4px; border-radius: 2px; }
.tray-photo { width: 24px; height: 24px; border-radius: 1px; overflow: hidden; border: 1px solid #ffffff; }
.tray-more-count { font-size: 0.65rem; color: #666; font-weight: 600; margin-left: 2px; }

.product-details-box { padding: 1.25rem; flex: 1; display: flex; flex-direction: column; justify-content: space-between; border-top: 1px solid #f9f8f6; }
.product-name-title { font-size: 0.95rem; font-weight: 600; margin: 0 0 0.5rem 0; line-height: 1.4; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.manufacturer-badge { display: inline-block; font-size: 0.7rem; color: #c5a880; font-weight: 600; margin-bottom: 0.5rem; text-transform: uppercase; letter-spacing: 0.5px; }
.category-meta-wrap { display: flex; align-items: center; gap: 4px; font-size: 0.75rem; color: #999999; margin-bottom: 0.75rem; overflow: hidden; }
.category-sep { font-size: 0.6rem; color: #dddddd; }

.product-attributes-footer { display: flex; justify-content: space-between; align-items: flex-end; margin-top: auto; }
.attr-badges { display: flex; flex-wrap: wrap; gap: 4px; }
.attr-tag { border-radius: 0; font-size: 0.65rem; height: 20px; line-height: 18px; padding: 0 5px; color: #888; border-color: #eeeeee; }
.product-id-label { font-size: 0.75rem; color: #cccccc; font-family: 'S-CoreDream', 'Jost', sans-serif; font-weight: 400; }

:global(html.dark) {
  .jovenca-product-card {
    background-color: #1a1a1a;
    border-color: #2e2e2e;

    &:hover {
      box-shadow: 0 10px 30px rgba(0, 0, 0, 0.4);
    }
  }

  .product-image-box {
    background-color: #252525;
  }

  .product-details-box {
    border-top-color: #2a2a2a;
  }

  .product-name-title {
    color: #e5e7eb;
  }

  .quick-action-button {
    background: #2b2b2b;
    color: #e5e7eb;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.3);

    &.is-active {
      background: #c5a880;
      color: #ffffff;
    }
  }

  .set-compositions-tray {
    background: rgba(30, 30, 30, 0.7);

    .tray-photo {
      border-color: #2b2b2b;
    }

    .tray-more-count {
      color: #aaa;
    }
  }

  .attr-tag {
    color: #aaa;
    border-color: #333333;
    background-color: #252525;
  }
}
</style>

