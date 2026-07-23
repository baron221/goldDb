<template>
<div v-if="orderItems && orderItems.length > 0" class="order-goods-info">
    <div class="goods-info-top">
      <div class="goods-visual-group">
        <div class="goods-thumb-strip">
          <div
            v-for="(item, idx) in orderItems.slice(0, 3)"
            :key="item.id || idx"
            class="goods-item-row"
          >
            <el-image
              :src="item.photoUrl || '/thumb_no_img.png'"
              fit="cover"
              class="goods-thumb"
              lazy
            >
              <template #error>
                <div class="goods-thumb-fallback"><i class="fas fa-gem"></i></div>
              </template>
            </el-image>
            <div class="goods-item-text">
              <div class="goods-item-name">{{ item.productName || item.productSetTitle || '-' }}</div>
              <div class="goods-item-spec">함량: {{ item.purity || '-' }} / 컬러: {{ item.color || '-' }} / 수량: {{ item.quantity || 0 }}개</div>
            </div>
            <div class="goods-item-memo-box" @click.stop>
              <template v-if="editingId === item.id">
                <el-input
                  v-model="editingValue"
                  type="textarea"
                  :rows="2"
                  size="small"
                  placeholder="주문 메모"
                />
                <div class="memo-actions">
                  <el-button size="small" type="primary" link @click="saveMemo(item)">저장</el-button>
                  <el-button size="small" link @click="cancelEdit()">취소</el-button>
                </div>
              </template>
              <template v-else>
                <span class="memo-text">{{ item.memo || '메모 없음' }}</span>
                <el-icon class="memo-edit-icon" @click="startEdit(item)"><Edit /></el-icon>
              </template>
            </div>
          </div>
          <span v-if="orderItems.length > 3" class="goods-thumb-more">+{{ orderItems.length - 3 }}건</span>
        </div>
      </div>
      <div class="goods-summary-sub">
        제품:{{ orderItems.length }}종 <br />
        개수:{{ getOrderTotalQuantity(orderItems) }}개
      </div>
    </div>
  </div>
  <span v-else>-</span>
</template>

<script setup lang="ts">

import { ref } from 'vue';
import { Edit } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import { updateOrderStatus } from '@/api/order';

const props = defineProps<{
  orderItems: any[];
  orderId?: number;
  orderStatus?: string;
}>();

const getOrderTotalQuantity = (items: any[]) => {
  if (!items) return 0;
  return items.reduce((sum, item) => sum + (item.quantity || 0), 0);
};

const editingId = ref<number | null>(null);
const editingValue = ref('');

const startEdit = (item: any) => {
  editingId.value = item.id;
  editingValue.value = item.memo || '';
};

const cancelEdit = () => {
  editingId.value = null;
  editingValue.value = '';
};

const saveMemo = async (item: any) => {
  if (!props.orderId) return;
  try {
    await updateOrderStatus(props.orderId, {
      status: props.orderStatus,
      itemWeights: [{ orderItemId: item.id, memo: editingValue.value }]
    });
    item.memo = editingValue.value;
    editingId.value = null;
    ElMessage.success('메모가 저장되었습니다.');
  } catch (error) {
    console.error(error);
    ElMessage.error('메모 저장에 실패했습니다.');
  }
};
</script>

<style lang="scss" src="../OrderTableListStyles.scss" scoped></style>

