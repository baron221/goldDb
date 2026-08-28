<template>
<base-popup v-model="visible" title="정산 처리" width="900px" @close="handleClose">
    <div v-if="summary" style="margin-bottom: 1.25rem; padding: 0.9375rem; border-radius: 2px; background: #fafafa;">
      <div style="display: flex; justify-content: space-between; margin-bottom: 0.5rem;">
        <div><strong>거래처</strong> {{ summary.companyName }}</div>
        <div v-if="summary.lastPaymentDate" style="color: #909399; font-size: 0.875rem;">최근 결제: {{ formatDate(summary.lastPaymentDate) }}</div>
      </div>
      <div style="font-size: 0.875rem; color: #606266;">{{ singleMode ? '단일 주문 정산' : `선택된 주문 ${localItems.length}건` }}</div>
    </div>

    <table v-if="localItems.length > 0" class="ledger-table order-item-table" style="margin-bottom: 1.25rem;">
      <thead>
        <tr>
          <th>주문번호</th>
          <th>주문내용</th>
          <th>중량</th>
          <th>주문일자</th>
          <th>공임비</th>
          <th v-if="!singleMode"></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in localItems" :key="item.payableId">
          <td>{{ item.orderNo }}</td>
          <td class="order-item-info">
            <div v-if="item.items && item.items.length > 0" class="product-info-list">
              <div v-for="(p, idx) in item.items.slice(0, 3)" :key="idx" class="order-item-info-row">
                <el-image :src="p.photoUrl || defaultImage" fit="cover" class="order-item-thumb" />
                <div class="order-item-text">
                  <div class="order-item-name">
                    {{ p.productName }}
                    <span v-if="p.productNo" class="order-item-no">{{ p.productNo }}</span>
                  </div>
                  <div class="order-item-sub">함량: {{ p.purity || '-' }} / 중량: {{ (p.actualWeight || 0).toFixed(3) }}g / 수량: {{ p.quantity }}개</div>
                  <div v-if="p.size && p.size !== 'EMPTY'" class="order-item-sub">표준 사이즈 : {{ p.size }}</div>
                  <div v-if="p.memo" class="order-item-memo">{{ p.memo }}</div>
                </div>
              </div>
              <div v-if="item.items.length > 3" class="product-more">+{{ item.items.length - 3 }}건 더</div>
              <div v-if="item.manufacturerName" class="order-item-sub">주문정보(공장): {{ item.manufacturerName }}</div>
              <div v-if="item.logisticsCompanyName" class="order-item-sub">주문정보(물류): {{ item.logisticsCompanyName }}</div>
            </div>
            <template v-else>
              <el-image :src="item.productPhotoUrl || defaultImage" fit="cover" class="order-item-thumb" />
              <div class="order-item-text">
                <div class="order-item-name">{{ item.productName }}</div>
                <div v-if="item.size && item.size !== 'EMPTY'" class="order-item-sub">표준 사이즈 : {{ item.size }}</div>
                <div v-if="item.manufacturerName" class="order-item-sub">주문정보(공장): {{ item.manufacturerName }}</div>
                <div v-if="item.logisticsCompanyName" class="order-item-sub">주문정보(물류): {{ item.logisticsCompanyName }}</div>
                <div v-if="item.memo" class="order-item-memo">{{ item.memo }}</div>
              </div>
            </template>
          </td>
          <td>{{ item.chargeWeight.toFixed(2) }}g</td>
          <td class="order-date">{{ formatDate(item.orderDate) }}</td>
          <td>
            <el-input-number v-model="item.laborCostOverride" :min="0" :step="1000" size="small" style="width: 110px;" />
          </td>
          <td v-if="!singleMode">
            <el-icon class="remove-icon" @click="removeItem(item)"><Close /></el-icon>
          </td>
        </tr>
      </tbody>
    </table>

    <table v-if="summary && summary.purityBreakdown && summary.purityBreakdown.length > 0" class="ledger-table" style="margin-bottom: 1.25rem;">
      <thead>
        <tr>
          <th>함량</th>
          <th>중량(g)</th>
          <th>공임 및 재료비</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="row in summary.purityBreakdown" :key="row.purity">
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
        <tr v-if="!singleMode">
          <td class="ledger-label">거래 전 미수(A)</td>
          <td class="ledger-readonly">{{ (summary?.totalOutstandingWeight || 0).toFixed(2) }}</td>
          <td class="ledger-readonly">₩ {{ formatPrice(summary?.totalOutstanding || 0) }}</td>
        </tr>
        <tr>
          <td class="ledger-label">{{ singleMode ? '청구 금액' : '판매(B)' }}</td>
          <td class="ledger-readonly">{{ (summary?.saleWeight || 0).toFixed(2) }}</td>
          <td class="ledger-readonly">₩ {{ formatPrice(effectiveSaleAmount) }}</td>
        </tr>
        <tr>
          <td class="ledger-label">결제(C)</td>
          <td>
            <el-input-number v-model="settleForm.weight" :min="0" :max="singleMode ? (summary?.saleWeight || 0) : undefined" :precision="2" :step="0.1" size="small" style="width: 100%;" />
          </td>
          <td>
            <el-input-number v-model="settleForm.amount" :min="0" :max="singleMode ? effectiveSaleAmount : undefined" :step="1000" size="small" style="width: 100%;" />
          </td>
        </tr>
        <tr>
          <td class="ledger-label">할인(D)</td>
          <td>
            <el-input-number v-model="settleForm.discountWeight" :min="0" :precision="2" :step="0.1" size="small" style="width: 100%;" />
          </td>
          <td>
            <el-input-number v-model="settleForm.discount" :min="0" :step="1000" size="small" style="width: 100%;" />
          </td>
        </tr>
        <tr class="ledger-total-row">
          <td class="ledger-label">{{ singleMode ? '결제 후 잔액(B-C-D)' : '거래 후 미수(A+B-C-D)' }}</td>
          <td class="ledger-readonly">{{ afterWeight.toFixed(2) }}</td>
          <td class="ledger-readonly">₩ {{ formatPrice(afterAmount) }}</td>
        </tr>
      </tbody>
    </table>

    <el-form :model="settleForm" label-width="120px" style="margin-top: 1.25rem;">
      <el-form-item label="메모">
        <el-input v-model="settleForm.memo" type="textarea" :rows="3" placeholder="정산 처리 메모" />
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
import { Close } from '@element-plus/icons-vue';
import { getOrderChargeSummary, processPayment } from '@/api/payable';
import BasePopup from '@/components/BasePopup/index.vue';
import { formatPrice } from '@/utils/format';
import { parseTime } from '@/utils';

const defaultImage = '/thumb_no_img.png';

const props = defineProps({
  modelValue: Boolean,
  orderIds: {
    type: Array as () => number[],
    default: () => []
  },
  // Single-order 정산처리 (from a table row button) settles just that one order's own
  // charge - the company-wide running balance (A) isn't relevant there and was confusing
  // users into thinking the whole company's debt was being pulled into a 1-order payment.
  singleMode: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits(['update:modelValue', 'saved']);

const visible = ref(false);
const submitting = ref(false);
const loading = ref(false);
const summary = ref<any>(null);

// Mutable working copy of the selected orders - lets the factory drop a row (X icon)
// out of THIS settlement action without closing the dialog, and gives each row an
// editable 공임비 the factory can adjust before settling (matching the reference
// site's 정산 내용 screen).
const localOrderIds = ref<number[]>([]);
const localItems = ref<any[]>([]);

const effectiveSaleAmount = computed(() =>
  localItems.value.reduce((sum, item) => sum + (item.laborCostOverride || 0), 0)
);

const settleForm = reactive({
  amount: 0,
  weight: 0,
  discount: 0,
  discountWeight: 0,
  memo: ''
});

const afterAmount = computed(() => {
  const a = props.singleMode ? 0 : (summary.value?.totalOutstanding || 0);
  const b = effectiveSaleAmount.value;
  return a + b - (settleForm.amount || 0) - (settleForm.discount || 0);
});

const afterWeight = computed(() => {
  const a = props.singleMode ? 0 : (summary.value?.totalOutstandingWeight || 0);
  const b = summary.value?.saleWeight || 0;
  return a + b - (settleForm.weight || 0) - (settleForm.discountWeight || 0);
});

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-';
  return parseTime(new Date(dateStr), '{y}-{m}-{d} {h}:{i}');
};

const fetchSummary = async () => {
  if (!localOrderIds.value || localOrderIds.value.length === 0) return;
  loading.value = true;
  try {
    const res: any = await getOrderChargeSummary(localOrderIds.value);
    summary.value = res.data;
    localItems.value = (res.data?.items || []).map((item: any) => ({
      ...item,
      laborCostOverride: item.remainingAmount || 0
    }));
  } catch (error) {
    console.error('Failed to fetch order charge summary:', error);
    ElMessage.error('정산 정보를 불러오는 중 오류가 발생했습니다.');
  } finally {
    loading.value = false;
  }
};

const removeItem = (item: any) => {
  if (localItems.value.length <= 1) {
    ElMessage.warning('최소 한 건의 주문은 남아있어야 합니다.');
    return;
  }
  localOrderIds.value = localOrderIds.value.filter((id) => id !== item.orderId);
  fetchSummary();
};

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val) {
    settleForm.amount = 0;
    settleForm.weight = 0;
    settleForm.discount = 0;
    settleForm.discountWeight = 0;
    settleForm.memo = '';
    summary.value = null;
    localItems.value = [];
    localOrderIds.value = [...props.orderIds];
    fetchSummary();
  }
});

watch(visible, (val) => {
  emit('update:modelValue', val);
});

const handleClose = () => {
  visible.value = false;
};

const handleSubmit = () => {
  if (!summary.value) return;

  ElMessageBox.confirm(
    `${summary.value.companyName}에 ₩${formatPrice(settleForm.amount)} 정산 처리하시겠습니까? (주문 ${localItems.value.length}건)`,
    '정산 처리 확인',
    { confirmButtonText: '확인', cancelButtonText: '취소', type: 'warning' }
  ).then(async () => {
    submitting.value = true;
    try {
      await processPayment({
        companyId: summary.value.companyId,
        orderIds: localOrderIds.value,
        amount: settleForm.amount,
        weight: settleForm.weight,
        discount: settleForm.discount,
        discountWeight: settleForm.discountWeight,
        memo: settleForm.memo
      });
      ElMessage.success('정산 처리되었습니다.');
      visible.value = false;
      emit('saved');
    } catch (error) {
      console.error('Failed to process batch payment:', error);
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
.order-item-table td {
  vertical-align: middle;
}
.order-item-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  text-align: left;
}
.order-item-thumb {
  width: 44px;
  height: 44px;
  flex-shrink: 0;
  border-radius: 2px;
  border: 1px solid #eae6df;
}
.order-item-text {
  min-width: 0;
}
.order-item-name {
  font-weight: 600;
  font-size: 0.875rem;
}
.order-item-sub {
  font-size: 0.75rem;
  color: #909399;
}
.order-item-memo {
  font-size: 0.75rem;
  color: #67c23a;
  margin-top: 0.125rem;
}
.product-info-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  text-align: left;
  width: 100%;
}
.order-item-info-row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}
.order-item-no {
  margin-left: 0.375rem;
  font-size: 0.75rem;
  font-weight: normal;
  color: #909399;
}
.product-more {
  font-size: 0.75rem;
  color: #909399;
}
.actual-weight {
  color: #f56c6c;
  font-weight: bold;
}
.order-date {
  font-size: 0.8125rem;
  color: #606266;
}
.remove-icon {
  cursor: pointer;
  color: #f56c6c;
  font-size: 1rem;
}
.remove-icon:hover {
  color: #d73a3a;
}
</style>
