<template>
<base-popup v-model="visible" title="적용 내역 수정" width="480px" @close="handleClose">
    <div v-if="record" style="margin-bottom: 1.25rem; padding: 0.9375rem; border-radius: 2px; background: #fafafa;">
      <div style="display: flex; justify-content: space-between;">
        <div><strong>거래처</strong> {{ companyName || '-' }}</div>
        <div style="color: #909399; font-size: 0.875rem;">주문번호: {{ record.orderNo }}</div>
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
          <td class="ledger-label">적용(C)</td>
          <td>
            <el-input-number v-model="editForm.appliedWeight" :min="0" :max="beforeWeight" :precision="2" :step="0.1" size="small" style="width: 100%;" />
          </td>
          <td>
            <el-input-number v-model="editForm.appliedAmount" :min="0" :max="beforeAmount" :step="1000" size="small" style="width: 100%;" />
          </td>
        </tr>
        <tr class="ledger-total-row">
          <td class="ledger-label">거래 후 미지급(A-C)</td>
          <td class="ledger-readonly">{{ afterWeight.toFixed(2) }}</td>
          <td class="ledger-readonly">₩ {{ formatPrice(afterAmount) }}</td>
        </tr>
      </tbody>
    </table>

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
import { updatePaymentApplication } from '@/api/payable';
import { formatPrice } from '@/utils/format';

const props = defineProps<{
  modelValue: boolean;
  record: any;
  companyName?: string;
}>();

const emit = defineEmits(['update:modelValue', 'saved']);

const visible = ref(false);
const submitting = ref(false);

const editForm = reactive({
  appliedAmount: 0,
  appliedWeight: 0
});

// chargeRemainingAmount already excludes this application's own current contribution
// (it's what's left AFTER this payment applied to the charge), so adding it back gives
// the balance as it was before this specific application - same pattern as the
// whole-payment ledger, just scoped to this one charge instead of the whole company.
const beforeAmount = computed(() => (props.record?.chargeRemainingAmount || 0) + (props.record?.appliedAmount || 0));
const beforeWeight = computed(() => (props.record?.chargeRemainingWeight || 0) + (props.record?.appliedWeight || 0));

const afterAmount = computed(() => beforeAmount.value - (editForm.appliedAmount || 0));
const afterWeight = computed(() => beforeWeight.value - (editForm.appliedWeight || 0));

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val && props.record) {
    editForm.appliedAmount = props.record.appliedAmount || 0;
    editForm.appliedWeight = props.record.appliedWeight || 0;
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
    await updatePaymentApplication(props.record.id, {
      appliedAmount: editForm.appliedAmount,
      appliedWeight: editForm.appliedWeight
    });
    ElMessage.success('수정되었습니다.');
    visible.value = false;
    emit('saved');
  } catch (error) {
    console.error('Failed to update payment application:', error);
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
