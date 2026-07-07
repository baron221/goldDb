<template>
<base-popup
    v-model="visible"
    title="사진 관리"
    width="800px"
    draggable
    append-to-body
    class="photo-manager-dialog"
  >
    <div class="dialog-desc" style="margin-bottom: 0.9375rem; color: #606266; font-size: 0.9rem;">
      * 등록된 사진 중 하나를 반드시 <strong>대표사진</strong>으로 설정해야 합니다. 첫 번째 사진은 자동으로 대표사진으로 지정됩니다.
    </div>

    <div class="photo-list" v-loading="loading">
      <div v-for="(photo, index) in localPhotos" :key="index" class="photo-item">
        <div v-if="photo.isMain" class="main-badge">대표</div>
        <div class="image-upload-wrapper">
          <image-upload
            v-model:model-value="photo.attachmentId"
            v-model:initial-url="photo.photoUrl"
            :sub-dir="subDir"
            compact
            @change="(val) => handlePhotoChange(index, val)"
          />
        </div>
        <div class="photo-actions">
          <el-button
            :type="photo.isMain ? 'success' : 'info'"
            size="small"
            plain
            class="main-btn"
            @click="setMainPhoto(index)"
          >{{ photo.isMain ? '대표사진' : '대표설정' }}</el-button>
          <el-button
            type="danger"
            size="small"
            plain
            :icon="Delete"
            class="delete-btn"
            @click="removePhoto(index)"
          />
        </div>
      </div>

      <el-upload
        ref="bulkUploadRef"
        multiple
        :auto-upload="false"
        :on-change="handleBulkUpload"
        :show-file-list="false"
        class="bulk-photo-uploader"
        accept="image/*"
      >
        <div class="photo-add-card">
          <div class="add-content">
            <el-icon :size="28" class="add-icon"><Plus /></el-icon>
            <div class="add-text">사진 추가 (다중선택 가능)</div>
          </div>
        </div>
      </el-upload>
    </div>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="visible = false">취소</el-button>
        <el-button type="primary" @click="handleConfirm">확인</el-button>
      </div>
    </template>
  </base-popup>
</template>

<script setup lang="ts">

import { ref, watch } from 'vue';
import BasePopup from '@/components/BasePopup/index.vue';
import { Delete, Plus, UploadFilled } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import ImageUpload from '@/components/ImageUpload/index.vue';
import { uploadImage } from '@/api/file';

const props = withDefaults(defineProps<{
  modelValue: boolean;
  photos: any[];
  subDir?: string;
}>(), {
  subDir: 'common'
});

const emit = defineEmits(['update:modelValue', 'update:photos']);

const visible = ref(false);
const localPhotos = ref<any[]>([]);
const loading = ref(false);
const bulkUploadRef = ref<any>(null);
let uploadTimer: any = null;

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val) {

    localPhotos.value = (props.photos || []).map((p: any) => ({
      ...p,
      isMain: !!p.isMain
    }));
  }
});

watch(() => visible.value, (val) => {
  emit('update:modelValue', val);
});

const handleBulkUpload = (file: any, fileList: any[]) => {

  if (uploadTimer) clearTimeout(uploadTimer);

  uploadTimer = setTimeout(async () => {
    const rawFiles = fileList.map(f => f.raw).filter(f => !!f);
    if (rawFiles.length === 0) return;

    loading.value = true;
    try {
      const uploadPromises = rawFiles.map(async (rawFile) => {
        const formData = new FormData();
        formData.append('file', rawFile);
        formData.append('subDir', props.subDir);

        const res = await uploadImage(formData);
        return {
          id: undefined,
          photoUrl: res.data.filePath,
          attachmentId: res.data.id,
          sortOrder: localPhotos.value.length,
          isMain: false
        };
      });

      const newPhotos = await Promise.all(uploadPromises);

      if (localPhotos.value.length === 0 && newPhotos.length > 0) {
        newPhotos[0].isMain = true;
      }

      localPhotos.value.push(...newPhotos);
      ElMessage.success(`${newPhotos.length}개의 사진이 추가되었습니다.`);

      if (bulkUploadRef.value) {
        bulkUploadRef.value.clearFiles();
      }
    } catch (error) {
      console.error('Bulk upload error:', error);
      ElMessage.error('일부 사진 업로드에 실패했습니다.');
    } finally {
      loading.value = false;
      uploadTimer = null;
    }
  }, 300); 
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

const removePhoto = (index: number) => {
  const wasMain = localPhotos.value[index].isMain;
  localPhotos.value.splice(index, 1);

  if (wasMain && localPhotos.value.length > 0) {
    localPhotos.value[0].isMain = true;
  }
};

const setMainPhoto = (index: number) => {
  localPhotos.value.forEach((p: any, i: number) => {
    p.isMain = i === index;
  });
};

const handlePhotoChange = (index: number, attachment: any) => {
  if (attachment) {
    localPhotos.value[index].attachmentId = attachment.id;
    localPhotos.value[index].photoUrl = attachment.filePath;
  }
};

const handleConfirm = () => {

  const hasPhotos = localPhotos.value.length > 0;
  if (hasPhotos) {
    const hasUploadedPhoto = localPhotos.value.some(p => p.attachmentId || p.photoUrl);
    if (!hasUploadedPhoto) {
      ElMessage.warning('최소 한 개 이상의 이미지를 업로드해 주세요.');
      return;
    }

    const hasMain = localPhotos.value.some(p => p.isMain);
    if (!hasMain) {

      localPhotos.value[0].isMain = true;
    }
  }

  const finalizedPhotos = localPhotos.value.map((p, idx) => ({
    ...p,
    sortOrder: idx
  }));

  emit('update:photos', finalizedPhotos);
  visible.value = false;
};
</script>

<style scoped>
.photo-list {
  display: flex;
  flex-wrap: wrap;
  gap: 1.25rem;
  min-height: 180px;
  align-content: flex-start;
  padding: 0.625rem 0;
}

.photo-item {
  width: 180px;
  position: relative;
  border: 1px solid #e4e7ed;
  border-radius: 2px;
  padding: 0.95rem;
  background: #ffffff;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  transition: all 0.3s ease;
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.photo-item:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.08);
  border-color: #409eff;
}

.image-upload-wrapper {
  width: 100%;
  aspect-ratio: 1 / 1;
  overflow: hidden;
  border-radius: 2px;
  background-color: #fafafa;
}

.main-badge {
  position: absolute;
  top: 18px;
  left: 18px;
  background: #67c23a;
  color: white;
  padding: 0.125rem 0.5rem;
  font-size: 0.825rem;
  font-weight: bold;
  border-radius: 2px;
  z-index: 10;
  box-shadow: 0 2px 4px rgba(103, 194, 58, 0.3);
}

.photo-actions {
  margin-top: 0.95rem;
  display: flex;
  justify-content: space-between;
  gap: 0.375rem;
}

.main-btn {
  flex: 1;
  font-weight: 500;
}

.delete-btn {
  padding: 0.5rem;
}

.bulk-photo-uploader {
  display: block;
}

.photo-add-card {
  width: 180px;
  height: 180px;
  border: 1.5px dashed #dcdfe6;
  border-radius: 2px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  background: #fafafa;
  transition: all 0.3s ease;
  box-sizing: border-box;
}

.photo-add-card:hover {
  border-color: #409eff;
  background: #f0f7ff;
}

.photo-add-card:hover .add-icon {
  color: #409eff;
  transform: scale(1.15);
}

.photo-add-card:hover .add-text {
  color: #409eff;
}

.add-content {
  text-align: center;
}

.add-icon {
  color: #909399;
  transition: all 0.3s ease;
}

.add-text {
  font-size: 0.9rem;
  margin-top: 0.375rem;
  color: #606266;
  font-weight: 500;
  transition: all 0.3s ease;
}
</style>

