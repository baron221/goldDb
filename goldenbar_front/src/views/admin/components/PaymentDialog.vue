<template>
<base-popup v-model="visible" title="정산 처리" width="560px" @close="handleClose">
    <div v-if="company" style="margin-bottom: 1.25rem; padding: 0.9375rem; border-radius: 2px; background: #fafafa;">
      <div style="display: flex; justify-content: space-between; margin-bottom: 0.5rem;">
        <div><strong>거래처</strong> {{ company.companyName }}</div>
        <div v-if="company.lastPaymentDate" style="color: #909399; font-size: 0.875rem;">최근 결제: {{ formatDate(company.lastPaymentDate) }}</div>
      </div>
      <div v-if="orderNo" style="margin-bottom: 0.5rem;"><strong>관련 주문</strong> <span style="color: #409eff;">{{ orderNo }}</span></div>
      <div style="font-size: 1rem;"><strong>미지급 잔액</strong> <span style="color: #f56c6c; font-weight: bold;">₩ {{ formatPrice(company.totalOutstanding) }}</span></div>
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
          <td class="ledger-readonly">{{ (company?.totalOutstandingWeight || 0).toFixed(2) }}</td>
          <td class="ledger-readonly">₩ {{ formatPrice(company?.totalOutstanding || 0) }}</td>
        </tr>
        <tr>
          <td class="ledger-label">청구(B)</td>
          <td class="ledger-readonly">0.00</td>
          <td class="ledger-readonly">₩ 0</td>
        </tr>
        <tr>
          <td class="ledger-label">결제(C)</td>
          <td>
            <el-input-number v-model="paymentForm.weight" :min="0" :precision="2" :step="0.1" size="small" style="width: 100%;" />
          </td>
          <td>
            <el-input-number v-model="paymentForm.amount" :min="0" :step="1000" size="small" style="width: 100%;" />
          </td>
        </tr>
        <tr>
          <td class="ledger-label">할인(D)</td>
          <td>
            <el-input-number v-model="paymentForm.discountWeight" :min="0" :precision="2" :step="0.1" size="small" style="width: 100%;" />
          </td>
          <td>
            <el-input-number v-model="paymentForm.discount" :min="0" :step="1000" size="small" style="width: 100%;" />
          </td>
        </tr>
        <tr class="ledger-total-row">
          <td class="ledger-label">거래 후 미지급(A+B-C-D)</td>
          <td class="ledger-readonly">{{ afterWeight.toFixed(2) }}</td>
          <td class="ledger-readonly">₩ {{ formatPrice(afterAmount) }}</td>
        </tr>
      </tbody>
    </table>

    <el-form :model="paymentForm" label-width="120px" style="margin-top: 1.25rem;">
      <el-form-item label="메모">
        <el-input v-model="paymentForm.memo" type="textarea" :rows="3" placeholder="정산 처리 메모" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="visible = false">취소</el-button>
      <el-button type="primary" :loading="submitting" @click="handleSubmit">정산 처리</el-button>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import { processPayment } from '@/api/payable';
import BasePopup from '@/components/BasePopup/index.vue';
import { formatPrice } from '@/utils/format';
import { parseTime } from '@/utils';

const props = defineProps({
  modelValue: Boolean,
  company: {
    type: Object,
    default: () => null
  },
  orderId: {
    type: Number,
    default: null
  },
  orderNo: {
    type: String,
    default: null
  }
});

const emit = defineEmits(['update:modelValue', 'saved']);

const visible = ref(false);
const submitting = ref(false);

const paymentForm = reactive({
  amount: 0,
  weight: 0,
  discount: 0,
  discountWeight: 0,
  memo: ''
});

const afterAmount = computed(() => {
  const a = props.company?.totalOutstanding || 0;
  return a - (paymentForm.amount || 0) - (paymentForm.discount || 0);
});

const afterWeight = computed(() => {
  const a = props.company?.totalOutstandingWeight || 0;
  return a - (paymentForm.weight || 0) - (paymentForm.discountWeight || 0);
});

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-';
  return parseTime(new Date(dateStr), '{y}-{m}-{d} {h}:{i}');
};

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val) {
    paymentForm.amount = 0;
    paymentForm.weight = 0;
    paymentForm.discount = 0;
    paymentForm.discountWeight = 0;
    paymentForm.memo = '';
  }
});

watch(visible, (val) => {
  emit('update:modelValue', val);
});

const handleClose = () => {
  visible.value = false;
};

const handleSubmit = () => {
  if (paymentForm.amount <= 0 && paymentForm.weight <= 0 && paymentForm.discount <= 0 && paymentForm.discountWeight <= 0) {
    ElMessage.error('결제액, 할인액 또는 중량 중 하나는 0보다 커야 합니다.');
    return;
  }

  ElMessageBox.confirm(
    `${props.company.companyName}에 ₩${formatPrice(paymentForm.amount)} 정산 처리하시겠습니까?`,
    '정산 처리 확인',
    { confirmButtonText: '확인', cancelButtonText: '취소', type: 'warning' }
  ).then(async () => {
    submitting.value = true;
    try {
      await processPayment({
        companyId: props.company.companyId,
        orderId: props.orderId || undefined,
        amount: paymentForm.amount,
        weight: paymentForm.weight,
        discount: paymentForm.discount,
        discountWeight: paymentForm.discountWeight,
        memo: paymentForm.memo
      });
      ElMessage.success('정산 처리되었습니다.');
      visible.value = false;
      emit('saved');
    } catch (error) {
      console.error('Failed to process payment:', error);
      ElMessage.error('정산 처리에 실패했습니다.');
    } finally {
      submitting.value = false;
    }
  }).catch(() => {});
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
