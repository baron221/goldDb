<template>
<div class="image-upload-container" :class="{ 'is-compact': compact }" @paste="handlePaste">
    <el-upload
      class="image-uploader"
      :action="uploadUrl"
      :headers="headers"
      :show-file-list="false"
      :on-success="handleSuccess"
      :before-upload="beforeUpload"
      drag
      :data="{ subDir: subDir }"
      name="file"
    >
      <div v-if="displayUrl" class="image-preview">
        <img :src="displayUrl" class="uploaded-image" />
        <div class="image-actions" @click.stop>
          <el-button type="danger" circle @click="handleRemove">
            <el-icon><Delete /></el-icon>
          </el-button>
        </div>
      </div>
      <template v-else>
        <div v-if="compact" class="compact-upload-content">
          <el-icon><UploadFilled /></el-icon>
          <span class="compact-text">이미지 추가 (클릭/드래그/붙여넣기)</span>
        </div>
        <template v-else>
          <el-icon class="el-icon--upload"><UploadFilled /></el-icon>
          <div class="el-upload__text">
            드래그하거나 <em>클릭하여 업로드</em>
            <div class="paste-hint">(클립보드 이미지를 붙여넣을 수 있습니다)</div>
          </div>
        </template>
      </template>
    </el-upload>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import { getToken } from '@/utils/auth';
import { ElMessage } from 'element-plus';
import { UploadFilled, Delete } from '@element-plus/icons-vue';
import axios from 'axios';

interface Attachment {
  id: number;
  fileName: string;
  originalName: string;
  extension: string;
  filePath: string;
  subDirectory: string;
}

const props = defineProps<{
  modelValue?: number | null; 
  initialUrl?: string | null; 
  subDir?: string;
  size?: '800' | 'medium' | 'small' | 'thumb' | 'avatar' | 'original';
  clearAfterUpload?: boolean;
  compact?: boolean;
}>();

const emit = defineEmits(['update:modelValue', 'update:initialUrl', 'change']);

const subDir = props.subDir || 'general';
const uploadUrl = `${import.meta.env.VITE_BASE_API}/file/upload-image`;
const headers = {
  Authorization: `Bearer ${getToken()}`
};

const internalUrl = ref<string | null>(props.initialUrl || null);

watch(() => props.initialUrl, (newVal) => {
  internalUrl.value = newVal || null;
});

const displayUrl = computed(() => {
  if (props.clearAfterUpload) return null;
  if (internalUrl.value) {
    if (props.size && props.size !== 'original') {

      if (internalUrl.value.startsWith('/uploads/')) {
        const parts = internalUrl.value.split('/');

        if (parts.length >= 4) {
          const fileName = parts.pop();
          return [...parts, props.size, fileName].join('/');
        }
      }
    }
    return internalUrl.value;
  }
  return null;
});

const beforeUpload = (file: File) => {
  const isJPGorPNG = file.type === 'image/jpeg' || file.type === 'image/png' || file.type === 'image/gif';
  if (!isJPGorPNG) {
    ElMessage.error('이미지 파일(JPG, PNG, GIF)만 업로드 가능합니다.');
    return false;
  }
  const isLt2M = file.size / 1024 / 1024 < 5;
  if (!isLt2M) {
    ElMessage.error('이미지 크기는 5MB를 초과할 수 없습니다.');
    return false;
  }
  return true;
};

const handleSuccess = (response: any) => {
  if (response.code === 20000) {
    const attachment = response.data as Attachment;
    if (!props.clearAfterUpload) {
      emit('update:modelValue', attachment.id);
      emit('update:initialUrl', attachment.filePath);
      internalUrl.value = attachment.filePath;
    }
    emit('change', attachment);
    ElMessage.success('이미지가 업로드되었습니다.');
  } else {
    ElMessage.error(response.message || '업로드 실패');
  }
};

const handleRemove = () => {
  emit('update:modelValue', null);
  emit('update:initialUrl', null);
  internalUrl.value = null;
  emit('change', null);
};

const handlePaste = async (event: ClipboardEvent) => {
  const items = event.clipboardData?.items;
  if (!items) return;

  for (let i = 0; i < items.length; i++) {
    if (items[i].type.indexOf('image') !== -1) {
      const file = items[i].getAsFile();
      if (file) {
        uploadFile(file);
      }
    }
  }
};

const uploadFile = async (file: File) => {
  if (!beforeUpload(file)) return;

  const formData = new FormData();
  formData.append('file', file);
  formData.append('subDir', subDir);

  try {
    const response = await axios.post(uploadUrl, formData, {
      headers: {
        ...headers,
        'Content-Type': 'multipart/form-data'
      }
    });
    handleSuccess(response.data);
  } catch (error: any) {
    ElMessage.error('업로드 중 오류가 발생했습니다.');
  }
};
</script>

<style scoped lang="scss">
.image-upload-container {
  width: 100%;

  :deep(.el-upload-dragger) {
    padding: 1.25rem;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 180px;
    transition: all 0.3s;
  }

  &.is-compact :deep(.el-upload-dragger) {
    min-height: 60px;
    padding: 0.625rem;
    flex-direction: row;
    gap: 0.625rem;
  }
}

.compact-upload-content {
  display: flex;
  align-items: center;
  gap: 0.625rem;
  color: #606266;

  .el-icon {
    font-size: 1.25rem;
  }
}

.compact-text {
  font-size: 0.875rem;
}

.image-preview {
  position: relative;
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;

  .uploaded-image {
    max-width: 100%;
    max-height: 150px;
    object-fit: contain;
  }

  .image-actions {
    position: absolute;
    top: 5px;
    right: 5px;
    display: none;
  }

  &:hover .image-actions {
    display: block;
  }
}

.paste-hint {
  font-size: 0.95rem;
  color: #909399;
  margin-top: 0.3125rem;
}

.avatar {
    width: 100px;
    height: 100px;
    border-radius: 50%;
    object-fit: cover;
}
</style>

