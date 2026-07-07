<template>
<base-popup draggable v-model="visible" title="직영점 재고 소진 처리" width="400px" @close="handleClosed">
    <el-form :model="deleteForm" label-width="100px">
      <el-form-item label="변경할 상태" required>
        <common-select
          v-model="deleteForm.reason"
          group-code="INVENTORY_STATUS"
          placeholder="상태 선택"
          style="width: 100%;"
        />
      </el-form-item>
      <el-form-item v-if="deleteForm.reason === '판매' || deleteForm.reason === 'SOLD'" label="연결 주문">
        <el-select
          v-model="deleteForm.exhaustionOrderId"
          placeholder="물류승인 주문 선택 (선택 사항)"
          filterable
          remote
          clearable
          :remote-method="fetchApprovedOrders"
          :loading="ordersLoading"
          style="width: 100%;"
        >
          <el-option
            v-for="item in approvedOrders"
            :key="item.id"
            :label="`${item.orderNo} (${item.userName}) - ${getOrderSummary(item)}`"
            :value="item.id"
          >
            <div style="display: flex; flex-direction: column; line-height: 1.2; padding: 0.3125rem 0;">
              <span style="font-weight: bold;">{{ item.orderNo }} ({{ item.userName }})</span>
              <span style="font-size: 0.95rem; color: #8492a6; margin-top: 0.125rem;">{{ getOrderSummary(item) }}</span>
            </div>
          </el-option>
        </el-select>
        <div style="font-size: 0.8875rem; color: #999; margin-top: 0.3125rem;">
          * 물류승인 상태인 주문만 선택 가능합니다. (미선택 시 주문 연결 없이 소진 처리됨)
        </div>
      </el-form-item>
      <el-form-item label="메모">
        <el-input v-model="deleteForm.memo" type="textarea" placeholder="상세 사유를 입력하세요" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="visible = false">취소</el-button>
      <el-button type="primary" :loading="submitLoading" @click="handleSubmit">확정</el-button>
    </template>
  </base-popup>
</template>

<script setup lang="ts">

import { ref, reactive, watch } from 'vue';
import BasePopup from '@/components/BasePopup/index.vue';
import { ElMessage } from 'element-plus';
import { getAllOrders } from '@/api/order';
import { deleteStocksBulk } from '@/api/stock';
import useUserStore from '@/store/modules/user';
import CommonSelect from '@/components/CommonSelect/index.vue';

const props = defineProps<{
  modelValue: boolean;
  stockIds: number[];
}>();

const emit = defineEmits(['update:modelValue', 'completed']);

const userStore = useUserStore();
const visible = ref(false);
const submitLoading = ref(false);
const ordersLoading = ref(false);
const approvedOrders = ref<any[]>([]);

const deleteForm = reactive({
  reason: 'SOLD',
  memo: '',
  exhaustionOrderId: undefined as number | undefined
});

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val) {
    initForm();
    fetchApprovedOrders();
  }
});

watch(() => visible.value, (val) => {
  emit('update:modelValue', val);
});

const initForm = () => {
  deleteForm.reason = 'SOLD'; 
  deleteForm.memo = '';
  deleteForm.exhaustionOrderId = undefined;
};

const handleClosed = () => {
  initForm();
};

const fetchApprovedOrders = async (queryText?: string) => {
  ordersLoading.value = true;
  try {
    const res = await getAllOrders({
      page: 1,
      pageSize: 50,
      status: 'LogisticsApproved',
      searchText: queryText,
      logisticsCompanyId: userStore.companyId
    });
    approvedOrders.value = res.data.items;
  } catch (error) {
    console.error('Failed to fetch approved orders:', error);
  } finally {
    ordersLoading.value = false;
  }
};

const getOrderSummary = (order: any) => {
  if (!order || !order.orderItems || order.orderItems.length === 0) return '상품 없음';

  const products = order.orderItems.map((item: any) => {
    if (item.productSetTitle) {
      return item.productSetTitle;
    }
    return item.productName || '알 수 없는 상품';
  });

  const uniqueProducts = [...new Set(products)];
  if (uniqueProducts.length > 2) {
    return `${uniqueProducts[0]}, ${uniqueProducts[1]} 외 ${uniqueProducts.length - 2}건`;
  }
  return uniqueProducts.join(', ');
};

const handleSubmit = async () => {
  if (!deleteForm.reason) {
    ElMessage.warning('변경할 상태를 선택해주세요.');
    return;
  }

  if (props.stockIds.length === 0) {
    ElMessage.warning('대상을 선택해주세요.');
    return;
  }

  submitLoading.value = true;
  try {
    await deleteStocksBulk({
      ids: props.stockIds,
      reason: deleteForm.reason,
      memo: deleteForm.memo,
      exhaustionOrderId: deleteForm.exhaustionOrderId
    });
    ElMessage.success('재고 소진 처리가 완료되었습니다.');
    visible.value = false;
    emit('completed');
  } catch (error) {
    console.error('Failed to exhaust stock:', error);
  } finally {
    submitLoading.value = false;
  }
};
</script>

