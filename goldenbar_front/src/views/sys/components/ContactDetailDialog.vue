<template>
<base-popup v-model="visible" title="문의 상세 내역" width="600px" @close="handleClose">
    <el-descriptions :column="1" border>
      <el-descriptions-item label="작성자">{{ temp.name }}</el-descriptions-item>
      <el-descriptions-item label="이메일">{{ temp.email }}</el-descriptions-item>
      <el-descriptions-item label="연락처">{{ temp.phone || '-' }}</el-descriptions-item>
      <el-descriptions-item label="주제">{{ temp.subject }}</el-descriptions-item>
      <el-descriptions-item label="문의 내용">
        <div style="white-space: pre-wrap; line-height: 1.6;">{{ temp.message }}</div>
      </el-descriptions-item>
      <el-descriptions-item label="등록일시">{{ formatDateTime(temp.createdAt) }}</el-descriptions-item>
    </el-descriptions>

    <div style="margin-top: 1.25rem;">
      <el-form label-position="top">
        <el-form-item label="관리자 메모">
          <el-input v-model="temp.adminMemo" type="textarea" :rows="3" placeholder="처리 내용이나 메모를 입력하세요" />
        </el-form-item>
        <el-form-item label="처리 상태">
          <el-checkbox v-model="temp.isProcessed">처리 완료로 표시</el-checkbox>
        </el-form-item>
      </el-form>
    </div>

    <template #footer>
      <el-button @click="visible = false">닫기</el-button>
      <el-button type="primary" :loading="submitting" @click="updateStatus">저장</el-button>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { processContact } from '@/api/contact-admin';
import { ElMessage } from 'element-plus';
import { parseTime } from '@/utils';
import BasePopup from '@/components/BasePopup/index.vue';

const props = defineProps({
  modelValue: Boolean,
  contact: {
    type: Object,
    default: () => null
  }
});

const emit = defineEmits(['update:modelValue', 'saved']);

const visible = ref(false);
const submitting = ref(false);
const temp = ref<any>({});

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val && props.contact) {
    temp.value = { ...props.contact };
  }
});

watch(visible, (val) => {
  emit('update:modelValue', val);
});

const formatDateTime = (date: string) => {
  if (!date) return '-';
  return parseTime(date, '{y}-{m}-{d} {h}:{i}:{s}');
};

const handleClose = () => {
  visible.value = false;
  temp.value = {};
};

const updateStatus = async () => {
  if (!temp.value.id) return;

  submitting.value = true;
  try {
    await processContact(temp.value.id, {
      isProcessed: temp.value.isProcessed,
      adminMemo: temp.value.adminMemo
    });
    ElMessage.success('상태가 업데이트되었습니다.');
    visible.value = false;
    emit('saved');
  } catch (error) {
    console.error(error);
    ElMessage.error('업데이트에 실패했습니다.');
  } finally {
    submitting.value = false;
  }
};
</script>

<style scoped>
</style>

