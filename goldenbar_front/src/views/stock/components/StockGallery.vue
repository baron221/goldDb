<template>
<div class="gallery-container">
    <div class="main-image">
      <el-image
        :src="activePhoto || defaultImage"
        fit="cover"
        :preview-src-list="allPhotos"
        preview-teleported
        class="image-viewer"
        style="width: 100%; height: auto;"
      />
      <div class="gallery-expand-trigger">
        <el-icon><FullScreen /></el-icon>
      </div>

      <div class="status-badge-overlay">
        <span class="status-chip" :class="`status-${stock.status}`">{{ getStatusName(stock.status) }}</span>
      </div>
    </div>
    <div v-if="allPhotos.length > 1" class="thumbnail-list">
      <div
        v-for="(photo, index) in allPhotos"
        :key="index"
        class="thumbnail-item"
        :class="{
          active: activePhoto === photo,
          'span-2': widePhotos.has(photo)
        }"
        @click="$emit('update:activePhoto', photo)"
      >
        <el-image :src="photo" fit="cover" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { FullScreen } from '@element-plus/icons-vue';
import { useI18n } from 'vue-i18n';

defineProps({
  activePhoto: {
    type: String,
    required: true
  },
  defaultImage: {
    type: String,
    required: true
  },
  allPhotos: {
    type: Array as () => string[],
    required: true
  },
  widePhotos: {
    type: Object as () => Set<string>,
    required: true
  },
  stock: {
    type: Object,
    required: true
  }
});

defineEmits(['update:activePhoto']);

const { t } = useI18n();

const getStatusName = (status: string) => {
  return t(`stock.status.${status}`) || status;
};
</script>

