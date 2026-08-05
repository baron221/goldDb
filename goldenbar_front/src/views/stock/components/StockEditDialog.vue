<template>
<base-popup
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    title="재고 정보 수정"
    width="450px"
    append-to-body
  >
    <el-form :model="editForm" label-position="top">
      <el-form-item label="사이즈">
        <el-input v-model="editForm.size" placeholder="사이즈 입력" />
      </el-form-item>
      <el-form-item label="실중량(g)">
        <el-input-number v-model="editForm.actualWeight" :precision="2" :step="0.1" :min="0" style="width: 100%;" />
      </el-form-item>
      <el-form-item label="소매 재료비">
        <amount-input v-model="editForm.retailerConfirmMaterialCost" placeholder="재료비 입력" />
      </el-form-item>
      <el-form-item label="소매 수공비">
        <amount-input v-model="editForm.retailerConfirmLaborCost" placeholder="수공비 입력" />
      </el-form-item>
      <el-form-item label="메모">
        <el-input v-model="editForm.note" type="textarea" :rows="3" placeholder="메모 입력" />
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="$emit('update:modelValue', false)">취소</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSave">저장</el-button>
      </span>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import { ElMessage } from 'element-plus';
import { updateStock } from '@/api/stock';
import BasePopup from '@/components/BasePopup/index.vue';
import AmountInput from '@/components/AmountInput/index.vue';

const props = defineProps<{
  modelValue: boolean;
  stock: any;
}>();

const emit = defineEmits(['update:modelValue', 'saved']);

const submitting = ref(false);
const editForm = reactive({
  size: '',
  actualWeight: 0,
  retailerConfirmMaterialCost: 0,
  retailerConfirmLaborCost: 0,
  note: ''
});

watch(() => props.modelValue, (val) => {
  if (val && props.stock) {
    editForm.size = props.stock.size && props.stock.size !== 'EMPTY' ? props.stock.size : '';
    editForm.actualWeight = props.stock.actualWeight || 0;
    editForm.retailerConfirmMaterialCost = props.stock.retailerConfirmMaterialCost || 0;
    editForm.retailerConfirmLaborCost = props.stock.retailerConfirmLaborCost || 0;
    editForm.note = props.stock.note || '';
  }
});

const handleSave = async () => {
  if (!props.stock) return;
  submitting.value = true;
  try {
    await updateStock(props.stock.id, {
      status: props.stock.status,
      renterName: props.stock.renterName,
      rentDate: props.stock.rentDate,
      returnDueDate: props.stock.returnDueDate,
      size: editForm.size,
      actualWeight: editForm.actualWeight,
      retailerConfirmMaterialCost: editForm.retailerConfirmMaterialCost,
      retailerConfirmLaborCost: editForm.retailerConfirmLaborCost,
      note: editForm.note
    });
    ElMessage.success('재고 정보가 수정되었습니다.');
    emit('update:modelValue', false);
    emit('saved');
  } catch (error) {
    console.error('Failed to update stock:', error);
    ElMessage.error('재고 수정에 실패했습니다.');
  } finally {
    submitting.value = false;
  }
};
</script>
