<template>
<base-popup
    v-model="visible"
    :title="t('admin.settlement.messages.confirmSettlementTitle')"
    width="600px"
    destroy-on-close
    @close="handleClose"
  >
    <div v-if="order" class="settlement-info-container">
      <el-descriptions :column="1" border>
        <el-descriptions-item label="주문번호">
          <span class="order-no">{{ order.orderNo }}</span>
        </el-descriptions-item>
        <el-descriptions-item label="소매점">
          {{ order.userDisplayName }} ({{ order.companyName || '-' }})
        </el-descriptions-item>
        <el-descriptions-item label="정산방법">
          <el-tag type="info" effect="plain">
            {{ getSettlementMethodLabel(order.settlementMethod) }}
          </el-tag>
        </el-descriptions-item>
        <el-descriptions-item label="정산금액">
          <span class="settlement-amount">
            ₩ {{ formatPrice(order.settlementAmount) }}
          </span>
        </el-descriptions-item>
        <el-descriptions-item label="정산확인요청 메모">
          <div class="remarks-box">
            {{ order.settlementRemarks || '-' }}
          </div>
        </el-descriptions-item>
        <el-descriptions-item label="실수납 금액">
          <div class="collection-input-wrapper">
            <amount-input
              v-model="collectionForm.amount"
              placeholder="수납된 금액을 입력하세요"
              style="width: 200px"
            />
            <span class="ml-2 text-gray">원</span>
          </div>
          <div class="mt-1 text-xs text-orange">
            * 입력된 금액만큼 즉시 입금(DEPOSIT) 처리됩니다.
          </div>
        </el-descriptions-item>
      </el-descriptions>

      <div class="confirm-alert mt-4">
        <el-alert
          :title="t('admin.settlement.messages.confirmSettlement')"
          type="warning"
          :closable="false"
          show-icon
        />
      </div>
    </div>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="visible = false">{{ t('common.cancel') }}</el-button>
        <el-button type="success" :loading="loading" @click="handleConfirm">
          {{ t('admin.settlement.messages.settlementBtn') }}
        </el-button>
      </div>
    </template>
  </base-popup>
</template>

<script setup lang="ts">

import { ref, watch, computed, reactive } from 'vue';
import BasePopup from '@/components/BasePopup/index.vue';
import { useI18n } from 'vue-i18n';
import useCodeStore from '@/store/modules/code';
import AmountInput from '@/components/AmountInput/index.vue';
import { formatPrice } from '@/utils/format';

const props = defineProps<{
  modelValue: boolean;
  order: any;
  loading?: boolean;
}>();

const emit = defineEmits(['update:modelValue', 'confirm']);

const { t } = useI18n();
const codeStore = useCodeStore();
const visible = ref(false);

const collectionForm = reactive({
  amount: 0
});

const codeMap = computed(() => codeStore.codeMap);

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val && props.order) {
    collectionForm.amount = props.order.settlementAmount || 0;
  }
});

watch(() => visible.value, (val) => {
  emit('update:modelValue', val);
});

const handleClose = () => {
  visible.value = false;
};

const handleConfirm = () => {

  const updatedOrder = {
    ...props.order,
    settlementAmount: collectionForm.amount
  };
  emit('confirm', updatedOrder);
};

const getSettlementMethodLabel = (method: string) => {
  if (!method) return '-';
  return codeMap.value[method] || method;
};
</script>

<style scoped>
.settlement-info-container {
  padding: 0.625rem 0;
}

.order-no {
  font-weight: bold;
  color: #409eff;
}

.settlement-amount {
  font-size: 1.125rem;
  font-weight: bold;
  color: #f56c6c;
}

.remarks-box {
  white-space: pre-wrap;
  padding: 0.625rem;
  border-radius: 2px;
  border: 1px solid #ebeef5;
  color: #606266;
  max-height: 200px;
  overflow-y: auto;
}

.mt-4 {
  margin-top: 1.25rem;
}

.ml-2 {
  margin-left: 0.5rem;
}

.mt-1 {
  margin-top: 0.25rem;
}

.text-gray {
  color: #909399;
}

.text-orange {
  color: #e6a23c;
}

.text-xs {
  font-size: 0.95rem;
}

.collection-input-wrapper {
  display: flex;
  align-items: center;
}

.dialog-footer {
  text-align: right;
}
</style>

