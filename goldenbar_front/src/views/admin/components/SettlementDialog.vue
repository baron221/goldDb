<template>
<base-popup v-model="visible" title="정산 처리" width="620px" @close="handleClose">
    <div v-if="summary" style="margin-bottom: 1.25rem; padding: 0.9375rem; border-radius: 2px; background: #fafafa;">
      <div style="display: flex; justify-content: space-between; margin-bottom: 0.5rem;">
        <div><strong>거래처</strong> {{ summary.companyName }} ({{ summary.userDisplayName }})</div>
        <div v-if="summary.lastPaymentDate" style="color: #909399; font-size: 0.875rem;">최근 결제: {{ formatDate(summary.lastPaymentDate) }}</div>
      </div>
      <div style="font-size: 0.875rem; color: #606266;">선택된 주문 {{ orders.length }}건</div>
    </div>

    <table v-if="purityBreakdown.length > 0" class="ledger-table" style="margin-bottom: 1.25rem;">
      <thead>
        <tr>
          <th>함량</th>
          <th>중량(g)</th>
          <th>공임 및 재료비</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="row in purityBreakdown" :key="row.purity">
          <td class="ledger-label">{{ row.purity }}</td>
          <td class="ledger-readonly">{{ row.weight.toFixed(2) }}</td>
          <td class="ledger-readonly">₩ {{ formatPrice(row.amount) }}</td>
        </tr>
      </tbody>
    </table>

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
          <td class="ledger-readonly">{{ (summary?.totalReceivableWeight || 0).toFixed(2) }}</td>
          <td class="ledger-readonly">₩ {{ formatPrice(summary?.totalReceivable || 0) }}</td>
        </tr>
        <tr>
          <td class="ledger-label">판매(B)</td>
          <td class="ledger-readonly">{{ saleWeight.toFixed(2) }}</td>
          <td class="ledger-readonly">₩ {{ formatPrice(saleAmount) }}</td>
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
      <el-form-item label="메모">
        <el-input v-model="depositForm.memo" type="textarea" :rows="3" placeholder="정산 처리 메모" />
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="visible = false">취소</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSettlementSubmit">정산 처리</el-button>
      </span>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import { updateOrderStatus } from '@/api/order';
import { getUserSummaryById, processDeposit } from '@/api/receivable';
import BasePopup from '@/components/BasePopup/index.vue';
import { formatPrice } from '@/utils/format';
import { parseTime } from '@/utils';

const props = defineProps({
  modelValue: Boolean,
  orders: {
    type: Array as () => any[],
    default: () => []
  }
});

const emit = defineEmits(['update:modelValue', 'saved']);

const visible = ref(false);
const submitting = ref(false);
const summary = ref<any>(null);

const depositForm = reactive({
  amount: 0,
  weight: 0,
  discount: 0,
  memo: ''
});

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-';
  return parseTime(new Date(dateStr), '{y}-{m}-{d} {h}:{i}');
};

// No per-item ratio negotiation UI anymore - settlement is the item's full declared
// cost, same formula (and same result) as the order list's own 총 주문 금액 column
// (getOrderTotalAmount in settlement-management.vue), so the two always agree.
const calcBaseCost = (item: any) => {
  const material = item.retailerConfirmMaterialCost || item.factoryInputMaterialCost || 0;
  const labor = item.retailerConfirmLaborCost || item.factoryInputLaborCost || 0;
  return material + labor;
};

const calcSettlementAmount = (baseCost: number, quantity: number) => {
  return Math.round(baseCost * (quantity || 1));
};

// 순금(g) columns show fine-gold-equivalent weight, not raw alloy weight - a 18K item's
// actual gram figure is only 82.5% pure gold, so it's converted to what it's worth in
// 24K terms before summing, matching how 거래 전 미수(A) is already tracked.
const purityRatio = (purity: string) => {
  switch (purity) {
    case '14K': return 0.6435;
    case '18K': return 0.825;
    case '24K': return 1.0;
    case 'PT': return 0.95;
    default: return 0;
  }
};

const flattenItems = () => {
  const flat: any[] = [];
  props.orders.forEach((order: any) => {
    (order.orderItems || []).forEach((item: any) => {
      flat.push(item);
      (item.children || []).forEach((child: any) => flat.push(child));
    });
  });
  return flat;
};

const saleAmount = computed(() => {
  return flattenItems().reduce((sum, item) => sum + calcSettlementAmount(calcBaseCost(item), item.quantity), 0);
});

const saleWeight = computed(() => {
  return flattenItems().reduce((sum, item) => {
    const rawWeight = (item.confirmedWeight || item.actualWeight || item.weight || 0) * (item.quantity || 1);
    return sum + rawWeight * purityRatio(item.purity);
  }, 0);
});

const purityBreakdown = computed(() => {
  const groups: Record<string, { purity: string; weight: number; amount: number }> = {};
  flattenItems().forEach((item: any) => {
    const key = item.purity || '기타';
    if (!groups[key]) groups[key] = { purity: key, weight: 0, amount: 0 };
    const rawWeight = (item.confirmedWeight || item.actualWeight || item.weight || 0) * (item.quantity || 1);
    groups[key].weight += rawWeight * purityRatio(item.purity);
    groups[key].amount += calcSettlementAmount(calcBaseCost(item), item.quantity);
  });
  return Object.values(groups).filter((g) => g.weight > 0 || g.amount > 0).sort((a, b) => b.weight - a.weight);
});

const afterAmount = computed(() => {
  const a = summary.value?.totalReceivable || 0;
  return a + saleAmount.value - (depositForm.amount || 0) - (depositForm.discount || 0);
});

const afterWeight = computed(() => {
  const a = summary.value?.totalReceivableWeight || 0;
  return a + saleWeight.value - (depositForm.weight || 0);
});

const fetchSummary = async () => {
  const userId = props.orders?.[0]?.userId;
  if (!userId) {
    summary.value = null;
    return;
  }
  try {
    const res: any = await getUserSummaryById(userId);
    summary.value = res.data;
  } catch (error) {
    console.error('Failed to fetch user receivable summary:', error);
    summary.value = null;
  }
};

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val) {
    depositForm.amount = 0;
    depositForm.weight = 0;
    depositForm.discount = 0;
    depositForm.memo = '';
    summary.value = null;
    fetchSummary();
  }
});

watch(visible, (val) => {
  emit('update:modelValue', val);
});

const handleClose = () => {
  visible.value = false;
};

const handleSettlementSubmit = () => {
  if (!props.orders || props.orders.length === 0) return;
  const userId = props.orders[0].userId;

  ElMessageBox.confirm(
    `${summary.value?.companyName || ''}에 ${props.orders.length}건 정산 처리하시겠습니까?`,
    '정산 처리 확인',
    { confirmButtonText: '확인', cancelButtonText: '취소', type: 'warning' }
  ).then(async () => {
    submitting.value = true;
    try {
      await Promise.all(props.orders.map((order: any) => {
        const orderItems: any[] = [];
        (order.orderItems || []).forEach((item: any) => {
          orderItems.push(item);
          (item.children || []).forEach((child: any) => orderItems.push(child));
        });
        const itemWeights = orderItems.map((item: any) => ({
          orderItemId: item.id,
          settlementRatio: 100,
          settlementAmount: calcSettlementAmount(calcBaseCost(item), item.quantity)
        }));
        return updateOrderStatus(order.id, { status: 'SETTLED', itemWeights });
      }));

      if ((depositForm.amount || 0) > 0 || (depositForm.weight || 0) > 0 || (depositForm.discount || 0) > 0) {
        await processDeposit({
          userId,
          amount: depositForm.amount,
          weight: depositForm.weight,
          discount: depositForm.discount,
          memo: depositForm.memo
        });
      }

      ElMessage.success('정산 처리되었습니다.');
      visible.value = false;
      emit('saved');
    } catch (error) {
      console.error('Failed to process settlement:', error);
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
