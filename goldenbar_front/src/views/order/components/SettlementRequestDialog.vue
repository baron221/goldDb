<template>
<base-popup
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    title="정산확인요청"
    width="500px"
  >
    <el-form :model="settleForm" label-position="top">
      <el-form-item label="정산방법" required>
        <common-select
          v-model="settleForm.settlementMethod"
          group-code="SETTLEMENT_METHOD"
          placeholder="정산방법 선택"
          style="width: 100%;"
        />
      </el-form-item>
      <el-form-item label="정산금액" required>
        <amount-input
          v-model="settleForm.settlementAmount"
          placeholder="정산하실 금액을 입력하세요"
          class="settle-amount-large-input"
        />
      </el-form-item>
      <el-form-item label="정산확인요청 메모">
        <el-input
          v-model="settleForm.settlementRemarks"
          type="textarea"
          :rows="6"
          placeholder="물류센터에 전달할 정산 관련 메모를 입력하세요"
        />
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="$emit('update:modelValue', false)">취소</el-button>
        <el-button type="primary" :loading="settleSubmitting" @click="handleSettleSubmit">요청</el-button>
      </span>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import { ElMessage } from 'element-plus';
import BasePopup from '@/components/BasePopup/index.vue';
import CommonSelect from '@/components/CommonSelect/index.vue';
import AmountInput from '@/components/AmountInput/index.vue';

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits(['update:modelValue', 'submit']);

const settleSubmitting = ref(false);
const settleForm = reactive({
  settlementMethod: '',
  settlementAmount: 0,
  settlementRemarks: ''
});

watch(() => props.modelValue, (val) => {
  if (val) {
    settleForm.settlementMethod = '';
    settleForm.settlementAmount = 0;
    settleForm.settlementRemarks = '';
  }
});

const handleSettleSubmit = async () => {
  if (!settleForm.settlementMethod) {
    ElMessage.warning('정산방법을 선택해주세요.');
    return;
  }
  if (!settleForm.settlementAmount || settleForm.settlementAmount <= 0) {
    ElMessage.warning('올바른 정산금액을 입력해주세요.');
    return;
  }

  try {
    settleSubmitting.value = true;

    emit('submit', { ...settleForm });
  } finally {
    settleSubmitting.value = false;
  }
};

defineExpose({
  close: () => emit('update:modelValue', false)
});
</script>

<style scoped>
.settle-amount-large-input :deep(.el-input__inner) {
  font-size: 1.25rem;
  font-weight: bold;
  color: #c5a880;
}
</style>

