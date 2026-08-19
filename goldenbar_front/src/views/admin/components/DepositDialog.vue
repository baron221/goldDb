<template>
<base-popup v-model="visible" :title="$t('admin.deposit.title')" width="560px" @close="handleClose">
    <div v-if="user" style="margin-bottom: 1.25rem; padding: 0.9375rem; border-radius: 2px; background: #fafafa;">
      <div style="display: flex; justify-content: space-between; margin-bottom: 0.5rem;">
        <div><strong>{{ $t('admin.deposit.member') }}</strong> {{ user.userDisplayName }} ({{ user.userName }})</div>
        <div v-if="user.lastPaymentDate" style="color: #909399; font-size: 0.875rem;">최근 결제: {{ formatDate(user.lastPaymentDate) }}</div>
      </div>
      <div style="font-size: 1rem;"><strong>{{ $t('admin.deposit.totalReceivable') }}</strong> <span style="color: #f56c6c; font-weight: bold;">₩ {{ formatPrice(user.totalReceivable) }}</span></div>
    </div>

    <div v-loading="purityLoading" style="margin-bottom: 1.25rem;">
      <table class="purity-summary-table">
        <thead>
          <tr>
            <th>14K 합계(g)</th>
            <th>18K 합계(g)</th>
            <th>순금 합계(g)</th>
            <th>PT 합계(g)</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>{{ purity.purity14k.toFixed(2) }}</td>
            <td>{{ purity.purity18k.toFixed(2) }}</td>
            <td>{{ purity.pureGold.toFixed(2) }}</td>
            <td>{{ purity.purityPt.toFixed(2) }}</td>
          </tr>
        </tbody>
      </table>
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
          <td class="ledger-label">거래 전 미수(A)</td>
          <td class="ledger-readonly">{{ (user?.totalReceivableWeight || 0).toFixed(2) }}</td>
          <td class="ledger-readonly">₩ {{ formatPrice(user?.totalReceivable || 0) }}</td>
        </tr>
        <tr>
          <td class="ledger-label">판매(B)</td>
          <td class="ledger-readonly">0.00</td>
          <td class="ledger-readonly">₩ 0</td>
        </tr>
        <tr>
          <td class="ledger-label">결제(C)</td>
          <td>
            <el-input-number v-model="depositForm.weight" :min="0" :precision="2" :step="0.1" size="small" style="width: 100%;" />
          </td>
          <td>
            <el-input-number v-model="depositForm.amount" :min="0" :step="1000" size="small" style="width: 100%;" />
          </td>
        </tr>
        <tr>
          <td class="ledger-label">할인(D)</td>
          <td class="ledger-readonly">-</td>
          <td>
            <el-input-number v-model="depositForm.discount" :min="0" :step="1000" size="small" style="width: 100%;" />
          </td>
        </tr>
        <tr class="ledger-total-row">
          <td class="ledger-label">거래 후 미수(A+B-C-D)</td>
          <td class="ledger-readonly">{{ afterWeight.toFixed(2) }}</td>
          <td class="ledger-readonly">₩ {{ formatPrice(afterAmount) }}</td>
        </tr>
      </tbody>
    </table>

    <el-form :model="depositForm" label-width="120px" style="margin-top: 1.25rem;">
      <el-form-item :label="$t('admin.deposit.memoLabel')">
        <el-input
          v-model="depositForm.memo"
          type="textarea"
          :rows="3"
          :placeholder="$t('admin.deposit.placeholder')"
        />
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="visible = false">{{ $t('common.cancel') }}</el-button>
        <el-button type="primary" :loading="submitting" @click="handleDepositSubmit">
          {{ $t('admin.deposit.submitBtn') }}
        </el-button>
      </span>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { processDeposit, getPuritySummary } from '@/api/receivable';
import { ElMessage, ElMessageBox } from 'element-plus';
import BasePopup from '@/components/BasePopup/index.vue';
import { formatPrice } from '@/utils/format';
import { parseTime } from '@/utils';

const { t } = useI18n();

const props = defineProps({
  modelValue: Boolean,
  user: {
    type: Object,
    default: () => null
  }
});

const emit = defineEmits(['update:modelValue', 'saved']);

const visible = ref(false);
const submitting = ref(false);
const purityLoading = ref(false);

const purity = reactive({
  purity14k: 0,
  purity18k: 0,
  pureGold: 0,
  purityPt: 0
});

const depositForm = reactive({
  amount: 0,
  weight: 0,
  discount: 0,
  memo: ''
});

const afterAmount = computed(() => {
  const a = props.user?.totalReceivable || 0;
  return a - (depositForm.amount || 0) - (depositForm.discount || 0);
});

const afterWeight = computed(() => {
  const a = props.user?.totalReceivableWeight || 0;
  return a - (depositForm.weight || 0);
});

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-';
  return parseTime(new Date(dateStr), '{y}-{m}-{d} {h}:{i}');
};

watch(() => props.modelValue, async (val) => {
  visible.value = val;
  if (val && props.user) {
    depositForm.amount = 0;
    depositForm.weight = 0;
    depositForm.discount = 0;
    depositForm.memo = '';

    purityLoading.value = true;
    try {
      const res: any = await getPuritySummary(props.user.userId);
      purity.purity14k = res.data.purity14k || 0;
      purity.purity18k = res.data.purity18k || 0;
      purity.pureGold = res.data.pureGold || 0;
      purity.purityPt = res.data.purityPt || 0;
    } catch (error) {
      console.error('Failed to load purity summary:', error);
    } finally {
      purityLoading.value = false;
    }
  }
});

watch(visible, (val) => {
  emit('update:modelValue', val);
});

const handleClose = () => {
  visible.value = false;
};

const handleDepositSubmit = () => {
  if (depositForm.amount <= 0 && depositForm.weight <= 0 && depositForm.discount <= 0) {
    ElMessage.error(t('admin.deposit.messages.amountZero'));
    return;
  }

  ElMessageBox.confirm(
    t('admin.deposit.messages.confirm', { name: props.user.userDisplayName, amount: formatPrice(depositForm.amount) }),
    t('admin.deposit.messages.confirmTitle'),
    {
      confirmButtonText: t('common.ok'),
      cancelButtonText: t('common.cancel'),
      type: 'warning'
    }
  ).then(async () => {
    submitting.value = true;
    try {
      await processDeposit({
        userId: props.user.userId,
        amount: depositForm.amount,
        weight: depositForm.weight,
        discount: depositForm.discount,
        memo: depositForm.memo
      });
      ElMessage.success(t('admin.deposit.messages.success'));
      visible.value = false;
      emit('saved');
    } catch (error) {
      console.error('Failed to process deposit:', error);
      ElMessage.error(t('admin.deposit.messages.error'));
    } finally {
      submitting.value = false;
    }
  }).catch(() => {});
};
</script>

<style scoped>
.purity-summary-table, .ledger-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}
.purity-summary-table th, .purity-summary-table td,
.ledger-table th, .ledger-table td {
  border: 1px solid #ebeef5;
  padding: 0.5rem;
  text-align: center;
}
.purity-summary-table th, .ledger-table th {
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
