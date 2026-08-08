<template>
<base-popup v-model="visible" title="정산 수정" width="620px" @close="handleClose">
    <div v-if="record" style="margin-bottom: 1.25rem; padding: 0.9375rem; border-radius: 2px; background: #fafafa;">
      <div style="display: flex; justify-content: space-between; margin-bottom: 0.5rem;">
        <div><strong>거래처</strong> {{ company?.companyName || '-' }}</div>
        <div style="color: #909399; font-size: 0.875rem;">거래No: {{ record.id }}</div>
      </div>
    </div>

    <table class="ledger-table">
      <thead>
        <tr>
          <th></th>
          <th>순금(g)</th>
          <th>공임 및 현금</th>
        </tr>
      </thead>
      <tbody>
        <tr>
          <td class="ledger-label">거래 전 미지급(A)</td>
          <td class="ledger-readonly">{{ beforeWeight.toFixed(2) }}</td>
          <td class="ledger-readonly">₩ {{ formatPrice(beforeAmount) }}</td>
        </tr>
        <tr>
          <td class="ledger-label">결제(C)</td>
          <td>
            <el-input-number v-model="editForm.weight" :min="0" :precision="2" :step="0.1" size="small" style="width: 100%;" />
          </td>
          <td>
            <el-input-number v-model="editForm.amount" :min="0" :step="1000" size="small" style="width: 100%;" />
          </td>
        </tr>
        <tr>
          <td class="ledger-label">할인(D)</td>
          <td>
            <el-input-number v-model="editForm.discountWeight" :min="0" :precision="2" :step="0.1" size="small" style="width: 100%;" />
          </td>
          <td>
            <el-input-number v-model="editForm.discount" :min="0" :step="1000" size="small" style="width: 100%;" />
          </td>
        </tr>
        <tr class="ledger-total-row">
          <td class="ledger-label">거래 후 미지급(A-C-D)</td>
          <td class="ledger-readonly">{{ afterWeight.toFixed(2) }}</td>
          <td class="ledger-readonly">₩ {{ formatPrice(afterAmount) }}</td>
        </tr>
      </tbody>
    </table>

    <el-form :model="editForm" label-width="120px" style="margin-top: 1.25rem;">
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
import { ref, reactive, computed, watch } from 'vue';
import { ElMessage } from 'element-plus';
import BasePopup from '@/components/BasePopup/index.vue';
import { updatePayable } from '@/api/payable';
import { formatPrice } from '@/utils/format';

const props = defineProps<{
  modelValue: boolean;
  record: any;
  company?: any;
}>();

const emit = defineEmits(['update:modelValue', 'saved']);

const visible = ref(false);
const submitting = ref(false);

const editForm = reactive({
  amount: 0,
  weight: 0,
  discount: 0,
  discountWeight: 0,
  memo: ''
});

// company.totalOutstanding already reflects this payment's effect (it's the CURRENT
// balance), so adding this record's own amount/discount back gives the balance as it
// was before this payment was ever applied - same math as the 거래명세서 statement.
const beforeAmount = computed(() => {
  const after = props.company?.totalOutstanding || 0;
  return after + (props.record?.amount || 0) + (props.record?.discount || 0);
});

const beforeWeight = computed(() => {
  const after = props.company?.totalOutstandingWeight || 0;
  return after + (props.record?.weight || 0) + (props.record?.discountWeight || 0);
});

const afterAmount = computed(() => beforeAmount.value - (editForm.amount || 0) - (editForm.discount || 0));
const afterWeight = computed(() => beforeWeight.value - (editForm.weight || 0) - (editForm.discountWeight || 0));

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val && props.record) {
    editForm.amount = props.record.amount || 0;
    editForm.weight = props.record.weight || 0;
    editForm.discount = props.record.discount || 0;
    editForm.discountWeight = props.record.discountWeight || 0;
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
      discountWeight: editForm.discountWeight,
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

<style scoped>
.ledger-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}
.ledger-table th, .ledger-table td {
  border: 1px solid #ebeef5;
  padding: 0.5rem;
  text-align: center;
}
.ledger-table th {
  background: #f5f7fa;
  font-weight: 600;
}
.ledger-label {
  text-align: left;
  font-weight: 600;
  background: #fafafa;
  white-space: nowrap;
}
.ledger-readonly {
  color: #606266;
}
.ledger-total-row {
  font-weight: bold;
}
.ledger-total-row .ledger-readonly {
  color: #f56c6c;
}
</style>
