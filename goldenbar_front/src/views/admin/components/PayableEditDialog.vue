<template>
<base-popup v-model="visible" title="정산 내역 수정" width="480px" @close="handleClose">
    <el-form v-if="record" :model="editForm" label-width="120px">
      <el-form-item label="결제 금액">
        <el-input-number v-model="editForm.amount" :min="0" :step="1000" style="width: 100%;" />
      </el-form-item>
      <el-form-item label="결제 중량(g)">
        <el-input-number v-model="editForm.weight" :min="0" :precision="2" :step="0.1" style="width: 100%;" />
      </el-form-item>
      <el-form-item label="할인 금액">
        <el-input-number v-model="editForm.discount" :min="0" :step="1000" style="width: 100%;" />
      </el-form-item>
      <el-form-item label="메모">
        <el-input v-model="editForm.memo" type="textarea" :rows="3" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="visible = false">취소</el-button>
      <el-button type="primary" :loading="submitting" @click="handleSubmit">저장</el-button>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue';
import { ElMessage } from 'element-plus';
import BasePopup from '@/components/BasePopup/index.vue';
import { updatePayable } from '@/api/payable';

const props = defineProps<{
  modelValue: boolean;
  record: any;
}>();

const emit = defineEmits(['update:modelValue', 'saved']);

const visible = ref(false);
const submitting = ref(false);

const editForm = reactive({
  amount: 0,
  weight: 0,
  discount: 0,
  memo: ''
});

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val && props.record) {
    editForm.amount = props.record.amount || 0;
    editForm.weight = props.record.weight || 0;
    editForm.discount = props.record.discount || 0;
    editForm.memo = props.record.memo || '';
  }
});

watch(visible, (val) => {
  emit('update:modelValue', val);
});

const handleClose = () => {
  visible.value = false;
};

const handleSubmit = async () => {
  if (!props.record) return;

  submitting.value = true;
  try {
    await updatePayable(props.record.id, {
      amount: editForm.amount,
      weight: editForm.weight,
      discount: editForm.discount,
      memo: editForm.memo
    });
    ElMessage.success('수정되었습니다.');
    visible.value = false;
    emit('saved');
  } catch (error) {
    console.error('Failed to update payable:', error);
    ElMessage.error('수정에 실패했습니다.');
  } finally {
    submitting.value = false;
  }
};
</script>
