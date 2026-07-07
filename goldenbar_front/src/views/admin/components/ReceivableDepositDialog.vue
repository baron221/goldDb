<template>
<base-popup
    :model-value="modelValue"
    @update:model-value="val => $emit('update:modelValue', val)"
    title="수납 등록"
    width="400px"
  >
    <el-form ref="depositFormRef" :model="depositForm" :rules="depositRules" label-width="80px">
      <el-form-item label="소매점">
        <span>{{ user?.companyName || user?.userName }}</span>
      </el-form-item>
      <el-form-item label="현재 미납금">
        <span style="font-weight: bold; color: #f56c6c;">{{ formatPrice(user?.totalReceivable || 0) }} 원</span>
      </el-form-item>
      <el-form-item label="대상 주문" prop="orderId">
        <el-select
          v-model="depositForm.orderId"
          placeholder="수납 대상 주문 선택"
          v-loading="unpaidOrdersLoading"
          style="width: 100%;"
          @change="handleOrderChange"
        >
          <el-option
            v-for="item in unpaidOrders"
            :key="item.orderId"
            :label="`${item.orderNo} (미납: ${formatPrice(item.remainingAmount)}원)`"
            :value="item.orderId"
          />
        </el-select>
      </el-form-item>
      <el-form-item label="수납방법" prop="settlementMethod">
        <common-select
          v-model="depositForm.settlementMethod"
          group-code="SETTLEMENT_METHOD"
          placeholder="수납방법 선택"
        />
      </el-form-item>
      <el-form-item label="수납금액" prop="amount">
        <amount-input
          v-model="depositForm.amount"
          placeholder="수납 금액 입력"
        />
      </el-form-item>
      <el-form-item label="예상 미납금">
        <span style="font-weight: bold; color: #409eff;">{{ formatPrice(remainingBalance) }} 원</span>
      </el-form-item>
      <el-form-item label="메모">
        <el-input v-model="depositForm.memo" type="textarea" :rows="3" placeholder="수납 관련 메모 입력" />
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="$emit('update:modelValue', false)">취소</el-button>
        <el-button type="primary" :loading="depositSubmitting" @click="submitDeposit">수납 완료</el-button>
      </span>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { ref, reactive, watch, computed } from 'vue';
import { ElMessage } from 'element-plus';
import { getReceivables, processDeposit } from '@/api/receivable';
import { formatPrice } from '@/utils/format';
import AmountInput from '@/components/AmountInput/index.vue';
import CommonSelect from '@/components/CommonSelect/index.vue';
import BasePopup from '@/components/BasePopup/index.vue';

const props = defineProps<{
  modelValue: boolean;
  user: any;
}>();

const emit = defineEmits<{(e: 'update:modelValue', val: boolean): void;
  (e: 'success'): void;
}>();

const depositSubmitting = ref(false);
const depositFormRef = ref<any>(null);
const unpaidOrders = ref<any[]>([]);
const unpaidOrdersLoading = ref(false);

const depositForm = reactive({
  amount: 0,
  settlementMethod: 'CASH',
  memo: '',
  orderId: undefined as number | undefined
});

const depositRules = {
  amount: [
    { required: true, message: '수납 금액을 입력해주세요.', trigger: 'blur' },
    { type: 'number', min: 1, message: '금액은 0보다 커야 합니다.', trigger: 'blur' }
  ],
  settlementMethod: [
    { required: true, message: '수납 방법을 선택해주세요.', trigger: 'change' }
  ],
  orderId: [
    { required: true, message: '수납 대상 주문을 선택해주세요.', trigger: 'change' }
  ]
};

const remainingBalance = computed(() => {
  const currentTotal = props.user?.totalReceivable || 0;
  return Math.max(0, currentTotal - (depositForm.amount || 0));
});

watch(() => props.modelValue, async (newVal) => {
  if (newVal && props.user) {
    depositForm.amount = 0;
    depositForm.settlementMethod = 'CASH';
    depositForm.memo = '';
    depositForm.orderId = undefined;
    if (depositFormRef.value) {
      depositFormRef.value.clearValidate();
    }
    await fetchUnpaidOrders();
  }
});

const fetchUnpaidOrders = async () => {
  if (!props.user) return;
  try {
    unpaidOrdersLoading.value = true;
    unpaidOrders.value = [];
    const res: any = await getReceivables({
      userId: props.user.userId,
      type: 'CHARGE',
      page: 1,
      pageSize: 100
    });
    if (res.data && res.data.items) {
      const ordersMap = new Map<number, any>();
      res.data.items.forEach((item: any) => {
        if (item.orderId && item.remainingAmount > 0) {
          if (!ordersMap.has(item.orderId)) {
            ordersMap.set(item.orderId, {
              orderId: item.orderId,
              orderNo: item.orderNo,
              remainingAmount: item.remainingAmount
            });
          } else {
            const existing = ordersMap.get(item.orderId);
            existing.remainingAmount += item.remainingAmount;
          }
        }
      });
      unpaidOrders.value = Array.from(ordersMap.values());
    }
  } catch (error) {
    console.error('Failed to fetch unpaid orders:', error);
  } finally {
    unpaidOrdersLoading.value = false;
  }
};

const handleOrderChange = (orderId: number) => {
  const selected = unpaidOrders.value.find(o => o.orderId === orderId);
  if (selected) {
    depositForm.amount = selected.remainingAmount;
  }
};

const submitDeposit = () => {
  depositFormRef.value.validate(async (valid: boolean) => {
    if (valid) {
      try {
        depositSubmitting.value = true;
        await processDeposit({
          userId: props.user.userId,
          orderId: depositForm.orderId,
          amount: depositForm.amount,
          memo: depositForm.memo,
          settlementMethod: depositForm.settlementMethod
        });
        ElMessage.success('수납 처리가 완료되었습니다.');
        emit('update:modelValue', false);
        emit('success');
      } catch (error) {
        console.error(error);
        ElMessage.error('수납 처리 중 오류가 발생했습니다.');
      } finally {
        depositSubmitting.value = false;
      }
    }
  });
};
</script>

