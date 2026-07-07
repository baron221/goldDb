<template>
<div class="photo-list" style="display: flex; flex-wrap: wrap; gap: 1.25rem; margin-top: 1.25rem;">
    <div v-for="(photo, index) in localPhotos" :key="index" class="photo-item" style="width: 180px; position: relative;">
      <div v-if="photo.isMain" class="main-badge">대표</div>
      <image-upload
        v-model:model-value="photo.attachmentId"
        v-model:initial-url="photo.photoUrl"
        sub-dir="product-sets"
        compact
        @change="(val) => handlePhotoChange(index, val)"
      />
      <div style="margin-top: 0.625rem; display: flex; justify-content: space-between; gap: 0.3125rem;">
        <el-button
          :type="photo.isMain ? 'success' : 'info'"
          size="small"
          plain
          style="flex: 1;"
          @click="setMainPhoto(index)"
        >{{ photo.isMain ? '대표사진' : '대표설정' }}</el-button>
        <el-button
          type="danger"
          size="small"
          plain
          :icon="Delete"
          @click="removePhoto(index)"
        />
      </div>
    </div>
    <div
      class="photo-add-card"
      style="width: 180px; height: 100px; border: 1px dashed #ccc; border-radius: 2px; display: flex; align-items: center; justify-content: center; cursor: pointer;"
      @click="addPhoto"
    >
      <div style="text-align: center; color: #909399;">
        <el-icon :size="24"><Plus /></el-icon>
        <div style="font-size: 0.95rem; margin-top: 0.25rem;">사진 추가</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { Delete, Plus } from '@element-plus/icons-vue';
import ImageUpload from '@/components/ImageUpload/index.vue';

const props = defineProps<{
  photos: any[];
}>();

const emit = defineEmits(['update:photos']);

const localPhotos = ref([...props.photos]);

watch(() => props.photos, (val) => {
  localPhotos.value = [...val];
}, { deep: true });

watch(localPhotos, (val) => {
  emit('update:photos', val);
}, { deep: true });

const handlePhotoChange = (index: number, attachment: any) => {
  if (attachment) {
    localPhotos.value[index].attachmentId = attachment.id;
    localPhotos.value[index].photoUrl = attachment.filePath;
  }
};

const setMainPhoto = (index: number) => {
  localPhotos.value.forEach((p: any, i: number) => {
    p.isMain = i === index;
  });
};

const removePhoto = (index: number) => {
  const wasMain = localPhotos.value[index].isMain;
  localPhotos.value.splice(index, 1);
  if (wasMain && localPhotos.value.length > 0) {
    localPhotos.value[0].isMain = true;
  }
};

const addPhoto = () => {
  localPhotos.value.push({
    id: undefined,
    photoUrl: '',
    attachmentId: null,
    sortOrder: localPhotos.value.length,
    isMain: localPhotos.value.length === 0
  });
};
</script>

<style scoped>
.main-badge {
  position: absolute;
  top: 10px;
  left: 10px;
  background: #67c23a;
  color: white;
  padding: 0.125rem 0.5rem;
  font-size: 0.825rem;
  font-weight: bold;
  border-radius: 2px;
  z-index: 10;
}
</style>

