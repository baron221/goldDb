<template>
<div class="gallery-container">
    <div class="main-image">
      <el-image
        :src="activePhoto || defaultImage"
        fit="contain"
        :preview-src-list="allPhotos"
        preview-teleported
        class="image-viewer"
      />
      <div class="gallery-expand-trigger">
        <el-icon><FullScreen /></el-icon>
      </div>
    </div>
    <div v-if="product.photos && product.photos.length > 1" class="thumbnail-list">
      <div
        v-for="(photo, index) in product.photos"
        :key="index"
        class="thumbnail-item"
        :class="{ active: activePhoto === photo.photoUrl }"
        @click="activePhoto = photo.photoUrl"
      >
        <el-image :src="photo.photoUrl" fit="cover" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { FullScreen } from '@element-plus/icons-vue';

const props = defineProps({
  product: {
    type: Object,
    required: true
  },
  defaultImage: {
    type: String,
    default: 'https://via.placeholder.com/600x600?text=No+Image'
  }
});

const activePhoto = ref('');

const allPhotos = computed(() => {
  return props.product.photos?.map((p: any) => p.photoUrl) || [];
});

watch(() => props.product.photos, (newPhotos) => {
  if (newPhotos && newPhotos.length > 0) {
    activePhoto.value = newPhotos[0].photoUrl;
  } else {
    activePhoto.value = '';
  }
}, { immediate: true });
</script>

<style lang="scss" scoped>
.gallery-container {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  align-items: stretch;

  .main-image {
    position: relative;
    width: 100%;
    height: 520px;
    border: 1px solid #f0f0f0;
    border-radius: 2px;
    overflow: hidden;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: #fafafa;
    transition: all 0.3s ease;
    cursor: pointer;

    &:hover {
      border-color: #c5a880;

      .gallery-expand-trigger {
        opacity: 1;
        transform: translate(-50%, -50%) scale(1);
      }
    }

    .image-viewer {
      width: 100%;
      height: 100%;
      transition: transform 0.5s ease;

      &:hover {
        transform: scale(1.03);
      }
    }

    .gallery-expand-trigger {
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%) scale(0.8);
      width: 54px;
      height: 54px;
      background-color: rgba(26, 26, 26, 0.85);
      border: 1px solid rgba(197, 168, 128, 0.4);
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      opacity: 0;
      pointer-events: none;
      transition: all 0.3s cubic-bezier(0.25, 0.46, 0.45, 0.94);
      box-shadow: 0 4px 15px rgba(0, 0, 0, 0.35);

      .el-icon {
        color: #c5a880;
        font-size: 1.375rem;
      }
    }
  }

  .thumbnail-list {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 1rem;
    width: 100%;
    margin-top: 1rem;

    .thumbnail-item {
      width: 100%;
      height: 240px;
      border: 1px solid #e5e5e5;
      border-radius: 2px;
      overflow: hidden;
      cursor: pointer;
      transition: all 0.3s ease;
      flex-shrink: 0;

      &:hover {
        border-color: #c5a880;
      }

      .el-image {
        width: 100%;
        height: 100%;
        object-fit: cover;
        transition: transform 0.3s ease;

        &:hover {
          transform: scale(1.05);
        }
      }
    }
  }
}
</style>

