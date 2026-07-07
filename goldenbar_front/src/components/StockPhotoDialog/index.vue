<template>
<base-popup draggable v-model="visible" title="재고 사진 관리" width="600px">
    <div style="display: flex; flex-wrap: wrap; gap: 1.25rem;">
      <div v-for="(photo, index) in stockPhotos" :key="index" style="width: 150px; text-align: center;">
        <image-upload
          v-model:model-value="photo.attachmentId"
          v-model:initial-url="photo.filePath"
          sub-dir="stocks"
          @change="(val) => handlePhotoChange(index, val)"
        />
        <div style="margin-top: 0.3125rem;">
          <el-button
            :type="photo.isMain ? 'success' : 'info'"
            size="small"
            @click="setMain(index)"
          >
            {{ photo.isMain ? '대표' : '대표설정' }}
          </el-button>
          <el-button type="danger" size="small" @click="removePhoto(index)">삭제</el-button>
        </div>
      </div>
      <el-button style="width: 150px; height: 150px;" @click="addPhoto">
        <el-icon><Plus /></el-icon>
      </el-button>
    </div>
    <template #footer>
      <el-button @click="visible = false">닫기</el-button>
      <el-button type="primary" :loading="submitLoading" @click="savePhotos">저장</el-button>
    </template>
  </base-popup>
</template>

<script setup lang="ts">

import { ref, watch, computed } from 'vue';
import BasePopup from '@/components/BasePopup/index.vue';
import { Plus } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import ImageUpload from '@/components/ImageUpload/index.vue';
import { updateStockPhotos } from '@/api/stock';

const props = defineProps(['modelValue', 'stockId', 'attachments']);
const emit = defineEmits(['update:modelValue', 'uploaded']);

const visible = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
});

const submitLoading = ref(false);
const stockPhotos = ref<any[]>([]);

watch(() => props.modelValue, (val) => {
  if (val) {
    stockPhotos.value = props.attachments.map((a: any) => ({
      id: a.id,
      attachmentId: a.id,
      filePath: a.filePath,
      isMain: a.isMain || false
    }));
  }
});

const addPhoto = () => {
  stockPhotos.value.push({ attachmentId: null, filePath: '', isMain: false });
};

const removePhoto = (index: number) => {
  stockPhotos.value.splice(index, 1);
};

const setMain = (index: number) => {
  stockPhotos.value.forEach((p, i) => p.isMain = (i === index));
};

const handlePhotoChange = (index: number, val: any) => {
  stockPhotos.value[index].attachmentId = (typeof val === 'object' && val !== null) ? val.id : val;
};

const savePhotos = async () => {
  submitLoading.value = true;
  try {
    const attachments = stockPhotos.value
      .filter(p => p.attachmentId)
      .map(p => {
        const id = (typeof p.attachmentId === 'object' && p.attachmentId !== null)
          ? p.attachmentId.id
          : p.attachmentId;
        return { attachmentId: id };
      });

    const mainPhoto = stockPhotos.value.find(p => p.isMain);
    const mainAttachmentId = mainPhoto
      ? (typeof mainPhoto.attachmentId === 'object' && mainPhoto.attachmentId !== null
        ? mainPhoto.attachmentId.id
        : mainPhoto.attachmentId)
      : null;

    await updateStockPhotos(props.stockId, attachments, mainAttachmentId);

    ElMessage.success('사진 정보가 저장되었습니다.');
    visible.value = false;
    emit('uploaded');
  } catch (error) {
    console.error(error);
    ElMessage.error('저장에 실패했습니다.');
  } finally {
    submitLoading.value = false;
  }
};
</script>

