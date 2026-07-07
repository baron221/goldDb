<template>
<base-popup
    v-model="visible"
    title="소매점 정산 시작"
    width="900px"
    @close="handleClose"
  >
    <div v-if="order" class="dialog-body">
      <div class="order-summary-header mb-4">
        <span class="order-no">주문번호: <strong>{{ order.orderNo }}</strong></span>
        <span class="divider" style="margin: 0 0.625rem; color: #dcdfe6;">|</span>
        <span class="client-name">거래처: {{ order.userDisplayName }} ({{ order.companyName || '-' }})</span>
      </div>

      <el-alert
        title="소매점 확인용 비용 입력 안내"
        type="info"
        description="정산 대기 상태로 변경하기 전, 각 제품별 소매점 확인용 재료비와 수공비를 입력해 주세요."
        show-icon
        :closable="false"
        style="margin-bottom: 1.25rem;"
      />

      <base-table :data="settlementItems" border style="width: 100%;">

        <el-table-column prop="photoUrl" width="80" align="center">
          <template #default="{ row }">
            <el-image :src="row.photoUrl || defaultImage" fit="cover" style="width: 100%; min-height: 80px; " />
          </template>
        </el-table-column>

        <el-table-column
          label="상품 정보"
          prop="productName"
          min-width="180"
          :excel-formatter="(row) => {
            const options = [
              row.purity && row.purity.toUpperCase() !== 'EMPTY' ? `함량: ${codeMap[row.purity] || row.purity}` : '',
              row.color && row.color.toUpperCase() !== 'EMPTY' ? `컬러: ${codeMap[row.color] || row.color}` : ''
            ].filter(Boolean).join(' | ');
            return `${row.productName || row.productSetTitle} (${row.productNo})${options ? '\n' + options : ''}\n수량: ${row.quantity}개`;
          }"
        >
          <template #default="{ row }">
            <div class="product-info-cell" style="display: flex; align-items: center; gap: 0.625rem;">
              <div class="product-text">
                <div class="product-name" style="font-weight: bold; font-size: 0.95rem; line-height: 1.2;">
                  {{ row.productName || row.productSetTitle }}
                  <span class="product-no" style="color: #909399; font-size: 0.85rem; font-weight: normal; margin-left: 0.375rem;">({{ row.productNo }})</span>
                </div>
                <div v-if="(row.purity && row.purity.toUpperCase() !== 'EMPTY') || (row.color && row.color.toUpperCase() !== 'EMPTY')" class="product-spec" style="color: #606266; font-size: 0.85rem; margin-top: 0.125rem;">
                  <span v-if="row.purity && row.purity.toUpperCase() !== 'EMPTY'">함량: {{ codeMap[row.purity] || row.purity }}</span>
                  <span v-if="row.purity && row.purity.toUpperCase() !== 'EMPTY' && row.color && row.color.toUpperCase() !== 'EMPTY'" style="margin: 0 4px; color: #dcdfe6;">|</span>
                  <span v-if="row.color && row.color.toUpperCase() !== 'EMPTY'">컬러: {{ codeMap[row.color] || row.color }}</span>
                </div>
                <div class="product-qty" style="color: #606266; margin-top: 0.125rem;">수량: {{ row.quantity }}개</div>
              </div>
            </div>
          </template>
        </el-table-column>

        <el-table-column
          label="공장 /소매 재료비"
          prop="retailerConfirmMaterialCost"
          width="120"
          header-align="center"
          align="right"
          :excel-formatter="(row) => `공장: ${formatPrice(row.factoryInputMaterialCost)}\n소매(확정): ${formatPrice(row.retailerConfirmMaterialCost)}`"
        >
          <template #default="{ row }">
            <span style="font-size: 0.825rem; color: #909399; margin-right: 0.125rem; font-weight: normal;">₩</span><span style="font-size: 0.9rem; color: #606266; font-weight: bold;">{{ formatPrice(row.factoryInputMaterialCost) }}</span>

            <amount-input
              v-model="row.retailerConfirmMaterialCost"
              placeholder="재료비 입력"
            />
          </template>
        </el-table-column>
        <el-table-column
          label="공장 /소매 수공비"
          prop="retailerConfirmLaborCost"
          width="120"
          header-align="center"
          align="right"
          :excel-formatter="(row) => `공장: ${formatPrice(row.factoryInputLaborCost)}\n소매(확정): ${formatPrice(row.retailerConfirmLaborCost)}`"
        >
          <template #default="{ row }">
            <span style="font-size: 0.825rem; color: #909399; margin-right: 0.125rem; font-weight: normal;">₩</span>
            <span style="font-size: 0.9rem; color: #606266; font-weight: bold;">{{ formatPrice(row.factoryInputLaborCost) }}</span>

            <amount-input
              v-model="row.retailerConfirmLaborCost"
              placeholder="수공비 입력"
            />
          </template>
        </el-table-column>
        <el-table-column
          label="수공비 처리비율 (%)"
          prop="laborCostRatio"
          width="120"
          align="center"
          :excel-formatter="(row) => `${row.laborCostRatio}%`"
        >
          <template #default="{ row }">
            <div style="color: #909399; margin-bottom: 0.125rem;">+ (수공비 * 비율%)</div>
            <el-input-number
              v-model="row.laborCostRatio"
              :min="0"
              :max="500"
              :step="1"
              controls-position="right"
              style="width: 90px;"
              @change="handleRatioChange(row)"
            />
          </template>
        </el-table-column>
      </base-table>

      <div class="settlement-memo-section" style="margin-top: 1.25rem;">
        <div style="font-weight: bold; font-size: 0.9rem; margin-bottom: 0.5rem; color: #606266;">정산 시작 메모</div>
        <el-input
          v-model="settlementMemo"
          type="textarea"
          :rows="3"
          placeholder="정산 시작 시 참고할 메모를 입력해 주세요 (선택 사항)"
          maxlength="500"
          show-word-limit
        />
      </div>
    </div>
    <template #footer>
      <div class="dialog-footer" style="text-align: right;">
        <el-button @click="visible = false">취소</el-button>
        <el-button type="primary" :loading="submitLoading" @click="handleSubmit">정산 시작 (정산대기)</el-button>
      </div>
    </template>
  </base-popup>
</template>

<script setup lang="ts">

import { ref, watch, computed } from 'vue';
import { ElMessage } from 'element-plus';
import { updateOrderStatus } from '@/api/order';
import AmountInput from '@/components/AmountInput/index.vue';
import { formatPrice } from '@/utils/format';
import useCodeStore from '@/store/modules/code';
import BaseTable from '@/components/BaseTable/index.vue';
import BasePopup from '@/components/BasePopup/index.vue';

const props = defineProps<{
  modelValue: boolean;
  order: any;
}>();

const emit = defineEmits(['update:modelValue', 'completed']);

const visible = ref(false);
const submitLoading = ref(false);
const settlementItems = ref<any[]>([]);
const settlementMemo = ref('');
const defaultImage = 'https://via.placeholder.com/100x100?text=No+Image';

const codeStore = useCodeStore();
const codeMap = computed(() => codeStore.codeMap);

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val && props.order) {
    initSettlementItems();
    settlementMemo.value = '';
  }
});

watch(() => visible.value, (val) => {
  emit('update:modelValue', val);
});

const initSettlementItems = async () => {
  await codeStore.fetchCodes();
  settlementItems.value = (props.order.orderItems || [])
    .filter((oi: any) => !oi.parentId)
    .map((oi: any) => {
      const factoryMaterial = oi.factoryInputMaterialCost || 0;
      const factoryLabor = oi.factoryInputLaborCost || 0;
      const settlementRatio = oi.settlementRatio || 70;

      const retailerMaterial = oi.retailerConfirmMaterialCost !== null && oi.retailerConfirmMaterialCost !== undefined && oi.retailerConfirmMaterialCost > 0
        ? oi.retailerConfirmMaterialCost
        : factoryMaterial;

      const retailerLabor = oi.retailerConfirmLaborCost !== null && oi.retailerConfirmLaborCost !== undefined && oi.retailerConfirmLaborCost > 0
        ? oi.retailerConfirmLaborCost
        : Math.round(factoryLabor * 2 * (settlementRatio / 100));

      return {
        id: oi.id,
        productName: oi.productName,
        productSetTitle: oi.productSetTitle,
        productNo: oi.productNo,
        photoUrl: oi.photoUrl,
        quantity: oi.quantity,
        purity: oi.purity,
        color: oi.color,
        factoryInputMaterialCost: factoryMaterial,
        factoryInputLaborCost: factoryLabor,
        laborCostRatio: settlementRatio,
        retailerConfirmMaterialCost: retailerMaterial,
        retailerConfirmLaborCost: factoryLabor + Math.round(factoryLabor * (settlementRatio / 100)) 
      };
    });
};

const handleRatioChange = (row: any) => {
  const factoryLabor = row.factoryInputLaborCost || 0;
  const ratio = row.laborCostRatio || 0;
  row.retailerConfirmLaborCost = factoryLabor + Math.round(factoryLabor * (ratio / 100));
};

const handleClose = () => {
  visible.value = false;
};

const handleSubmit = async () => {
  if (!props.order) return;
  submitLoading.value = true;
  try {
    const itemWeights = settlementItems.value.map(item => ({
      orderItemId: item.id,
      retailerConfirmMaterialCost: item.retailerConfirmMaterialCost,
      retailerConfirmLaborCost: item.retailerConfirmLaborCost,
      settlementRatio: item.laborCostRatio
    }));

    await updateOrderStatus(props.order.id, {
      status: 'PENDING',
      settlementStartMemo: settlementMemo.value,
      itemWeights
    });

    ElMessage.success('정산 시작 처리가 완료되었습니다. 주문이 정산대기 상태로 변경되었습니다.');
    visible.value = false;
    emit('completed');
  } catch (error) {
    console.error('Failed to start retailer settlement:', error);
    ElMessage.error('정산 시작 처리에 실패했습니다.');
  } finally {
    submitLoading.value = false;
  }
};
</script>

<style scoped>
.order-summary-header {
  font-size: 0.875rem;
  padding: 0.625rem 0.9375rem;
  border-radius: 2px;
  border-left: 4px solid #409eff;
}

.mb-4 {
  margin-bottom: 1rem;
}

.dialog-body {
  padding: 0 0.3125rem;
}
</style>

