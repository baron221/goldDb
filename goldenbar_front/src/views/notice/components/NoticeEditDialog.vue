<template>
<base-popup v-model="visible" :title="dialogStatus === 'create' ? '공지 추가' : '공지 수정'" width="60%" @close="handleClose">
    <el-form ref="formRef" :model="temp" :label-position="isMobile ? 'top' : 'left'" label-width="120px" :style="!isMobile ? { marginLeft: '3.125rem' } : {}">
      <el-form-item label="제목" prop="title" required>
        <el-input v-model="temp.title" />
      </el-form-item>
      <el-form-item label="노출 설정">
        <el-checkbox v-model="temp.isLoginRequired">로그인 시에만 노출</el-checkbox>
      </el-form-item>
      <el-form-item label="상태">
        <el-switch v-model="temp.isActive" active-text="활성" inactive-text="비활성" />
      </el-form-item>
      <el-form-item label="노출 시작일">
        <el-date-picker v-model="temp.startDate" type="datetime" placeholder="시작일 선택 (미설정 시 즉시)" style="width: 100%" />
      </el-form-item>
      <el-form-item label="노출 종료일">
        <el-date-picker v-model="temp.endDate" type="datetime" placeholder="종료일 선택 (미설정 시 무제한)" style="width: 100%" />
      </el-form-item>
      <el-form-item label="내용">
        <tinymce v-model="temp.content" :height="400" />
      </el-form-item>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="visible = false">취소</el-button>
        <el-button type="primary" :loading="submitting" @click="confirmSave">확인</el-button>
      </div>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue';
import { useMobile } from '@/hooks/useMobile';
import { createNotice, updateNotice } from '@/api/notice';
import { ElMessage } from 'element-plus';
import Tinymce from '@/components/Tinymce/index.vue';
import BasePopup from '@/components/BasePopup/index.vue';

const { isMobile } = useMobile();

const props = defineProps({
  modelValue: Boolean,
  dialogStatus: {
    type: String,
    default: 'create'
  },
  noticeData: {
    type: Object,
    default: () => null
  }
});

const emit = defineEmits(['update:modelValue', 'saved']);

const visible = ref(false);
const submitting = ref(false);
const formRef = ref();

const temp = reactive({
  id: undefined as number | undefined,
  title: '',
  content: '',
  isLoginRequired: false,
  isActive: true,
  startDate: null as any,
  endDate: null as any
});

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val) {
    resetTemp();
    if (props.dialogStatus === 'update' && props.noticeData) {
      Object.assign(temp, props.noticeData);
    }
  }
});

watch(visible, (val) => {
  emit('update:modelValue', val);
});

const resetTemp = () => {
  temp.id = undefined;
  temp.title = '';
  temp.content = '';
  temp.isLoginRequired = false;
  temp.isActive = true;
  temp.startDate = null;
  temp.endDate = null;
};

const handleClose = () => {
  visible.value = false;
};

const confirmSave = async () => {
  if (!temp.title) {
    ElMessage.warning('제목을 입력해주세요');
    return;
  }

  submitting.value = true;
  try {
    if (props.dialogStatus === 'create') {
      await createNotice(temp);
      ElMessage.success('성공적으로 등록되었습니다.');
    } else {
      await updateNotice(temp);
      ElMessage.success('성공적으로 수정되었습니다.');
    }
    visible.value = false;
    emit('saved');
  } catch (error) {
    console.error(error);
    ElMessage.error(props.dialogStatus === 'create' ? '등록 실패' : '수정 실패');
  } finally {
    submitting.value = false;
  }
};
</script>

