<template>
<base-popup
    draggable
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    :title="dialogType === 'edit' ? '권한 수정' : '새 권한 추가'"
    width="500px"
  >
    <el-form :model="role" label-width="100px" label-position="left">
      <el-form-item label="권한 키">
        <el-input v-model="role.key" placeholder="권한 키 (예: admin)" :disabled="dialogType === 'edit'" />
      </el-form-item>
      <el-form-item label="권한 명칭">
        <el-input v-model="role.name" placeholder="권한 명칭" />
      </el-form-item>
      <el-form-item label="설명">
        <el-input v-model="role.description" :autosize="{ minRows: 2, maxRows: 4 }" type="textarea" placeholder="권한 설명" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="$emit('update:modelValue', false)">취소</el-button>
      <el-button type="primary" @click="submitForm">확인</el-button>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { reactive, watch } from 'vue';
import { addRole, updateRole } from '@/api/role';
import { ElMessage } from 'element-plus';
import BasePopup from '@/components/BasePopup/index.vue';

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  dialogType: {
    type: String,
    default: 'new' 
  },
  initialData: {
    type: Object,
    default: () => ({})
  }
});

const emit = defineEmits(['update:modelValue', 'success']);

const role = reactive({
  key: '',
  name: '',
  description: ''
});

watch(() => props.modelValue, (val) => {
  if (val) {
    if (props.dialogType === 'edit' && props.initialData) {
      Object.assign(role, props.initialData);
    } else {
      resetRole();
    }
  }
});

const resetRole = () => {
  Object.assign(role, {
    key: '',
    name: '',
    description: ''
  });
};

const submitForm = async () => {
  try {
    if (props.dialogType === 'edit') {
      await updateRole(role.key, role);
      ElMessage.success('권한이 수정되었습니다.');
    } else {
      await addRole(role);
      ElMessage.success('새 권한이 추가되었습니다.');
    }
    emit('success');
    emit('update:modelValue', false);
  } catch (error) {
    console.error(error);
    ElMessage.error('권한 저장에 실패했습니다.');
  }
};
</script>

