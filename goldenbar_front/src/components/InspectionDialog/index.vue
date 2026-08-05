<template>
  <base-popup draggable
              v-model="visible"
              width="90%"
              style="max-width: 1600px;"
              @close="handleClosed"
  >
    <template #header>
      <div class="luxury-dialog-header">
        <span class="el-dialog__title">물류 검수 확인 및 정산 시작</span>
        <div v-if="order?.deliveryDate" class="delivery-date-badge">
          <el-icon><Calendar /></el-icon>
          <span class="label">납기일:</span>
          <span class="val">{{ order.deliveryDate.substring(0, 10) }}</span>
        </div>
      </div>
    </template>
    <el-form :model="inspectionForm" label-position="top">
      <base-table :data="inspectionForm.items" border style="width: 100%; margin-bottom: 1.25rem;" :row-class-name="tableRowClassName">
        <el-table-column
          label="상품 정보"
          prop="productName"
          min-width="240"
          :fixed="!isMobile ? 'left' : false"
          header-align="center"
          :excel-formatter="(row) => {
            const setTag = row.isSet ? '[SET] ' : '';
            const manufacturer = row.manufacturerName ? `[${row.manufacturerName}] ` : '';
            const options = [
              row.purity && row.purity.toUpperCase() !== 'EMPTY' ? (codeMap[row.purity] || row.purity) : '',
              row.color && row.color.toUpperCase() !== 'EMPTY' ? (codeMap[row.color] || row.color) : ''
            ].filter(Boolean).join(' / ');
            return `${manufacturer}${setTag}${row.productName || row.productSetTitle} (${row.productNo})${options ? '\n옵션: ' + options : ''}`;
          }"
        >
          <template #default="scope">
            <div class="product-info-cell" :style="{ paddingLeft: scope.row.depth * 20 + 'px' }">
              <el-icon v-if="scope.row.isChild" style="margin-right: 0.3125rem; color: #909399;"><BottomLeft /></el-icon>
              <el-image :src="scope.row.photoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 40px; height: 40px; flex-shrink: 0;" />
              <div class="product-text">
                <div class="product-no-row" style="display: flex; align-items: center; gap: 0.375rem; margin-top: 0.125rem; margin-bottom: 0.125rem;">
                  <el-tag v-if="scope.row.manufacturerName" size="small" type="warning" effect="plain" style=" height: 16px !important; line-height: 14px !important; padding: 0 0.25rem !important;">
                    {{ scope.row.manufacturerName }}
                  </el-tag>

                </div>
                <div class="product-name" style="font-size: 0.9rem;">
                  <el-tag v-if="scope.row.isSet" size="small" type="warning" style="margin-right: 0.3125rem;">SET</el-tag>
                  {{ scope.row.productName || scope.row.productSetTitle }} ( <span class="product-no" style="font-size: 0.8875rem;">{{ scope.row.productNo }}</span> )
                </div>
              </div>
            </div>
          </template>
        </el-table-column>

        <el-table-column
          label="수량"
          prop="quantity"
          width="60"
          align="center"
          header-align="center"
        >
          <template #default="scope">
            <el-tag>{{ scope.row.quantity }}</el-tag>
          </template>
        </el-table-column>

        <el-table-column
          label="옵션"
          width="110"
          align="center"
          header-align="center"
          :excel-formatter="(row) => [row.purity && row.purity.toUpperCase() !== 'EMPTY' ? (codeMap[row.purity] || row.purity) : '', row.color && row.color.toUpperCase() !== 'EMPTY' ? (codeMap[row.color] || row.color) : '', row.size && row.size.toUpperCase() !== 'EMPTY' ? row.size : ''].filter(Boolean).join(', ')"
        >
          <template #default="scope">
            <div style="display: flex; flex-direction: column; gap: 0.25rem; align-items: center; justify-content: center;">
              <el-tag v-if="scope.row.purity && scope.row.purity.toUpperCase() !== 'EMPTY'" size="small" type="info" effect="plain" style="font-size: 0.825rem; width: fit-content;">{{ codeMap[scope.row.purity] || scope.row.purity }}</el-tag>
              <el-tag v-if="scope.row.color && scope.row.color.toUpperCase() !== 'EMPTY'" size="small" type="warning" effect="plain" style="font-size: 0.825rem; width: fit-content;">{{ codeMap[scope.row.color] || scope.row.color }}</el-tag>
              <el-tag v-if="scope.row.size && scope.row.size.toUpperCase() !== 'EMPTY'" size="small" type="success" effect="plain" style="font-size: 0.825rem; width: fit-content;">{{ scope.row.size }}</el-tag>
              <span v-if="(!scope.row.purity || scope.row.purity.toUpperCase() === 'EMPTY') && (!scope.row.color || scope.row.color.toUpperCase() === 'EMPTY') && (!scope.row.size || scope.row.size.toUpperCase() === 'EMPTY')">-</span>
            </div>
          </template>
        </el-table-column>

        <el-table-column
          label="물류 승인"
          prop="approvedWeight"
          align="center"
          header-align="center"
          width="180"
          :excel-formatter="(row) => `${row.approvedWeight ? row.approvedWeight + 'g' : '-'}${row.approvedMemo ? '\n메모: ' + row.approvedMemo : ''}`"
        >
          <template #default="scope">
            <el-tooltip :content="scope.row.approvedMemo || '메모 없음'" placement="top" :disabled="!scope.row.approvedMemo">
              <span style="color: #67C23A; font-weight: bold;">{{ scope.row.approvedWeight ? scope.row.approvedWeight + 'g' : '-' }}</span>
            </el-tooltip>
          </template>
        </el-table-column>

        <el-table-column
          label="공장 의뢰 / 검수중량(g)"
          width="180"
          align="center"
          header-align="center"
          :excel-formatter="(row) => `의뢰: ${row.requestedWeight || 0}g / 실측: ${row.actualWeight || 0}g\n검수중량: ${row.confirmedWeight || 0}g`"
        >
          <template #default="scope">
            <el-tooltip :content="scope.row.requestedMemo || '메모 없음'" placement="top" :disabled="!scope.row.requestedMemo">
              <span style="color: #E6A23C; font-weight: bold;">{{ scope.row.requestedWeight ? scope.row.requestedWeight + 'g' : '-' }}</span>
            </el-tooltip>
            <el-tooltip :content="scope.row.inspectionMemo || '메모 없음'" placement="top" :disabled="!scope.row.inspectionMemo">
              <span style="color: #409EFF; font-size: 0.8125rem; margin-left: 0.375rem;">(실측 {{ scope.row.actualWeight ? scope.row.actualWeight + 'g' : '-' }})</span>
            </el-tooltip>
            <br />
            <el-input-number
              v-model="scope.row.confirmedWeight"
              :precision="2"
              :step="0.1"
              :min="0"
              style="width: 110px; margin-top: 0.25rem;"
            />
          </template>
        </el-table-column>

        <el-table-column
          :label="$t('admin.inspectionRequest.headers.retailerConfirm')"
          align="center"
          header-align="center"
          width="200"
          class-name="final-inspection-col"
          :excel-formatter="(row) => `재료비: ${row.retailerConfirmMaterialCost || 0}\n수공비: ${row.retailerConfirmLaborCost || 0}\n제작일: ${row.productionDate || '-'}\n검수메모: ${row.logisticsMemo || '-'}`"
        >
          <template #default="scope">
            <div v-if="scope.row.factoryInputMaterialCost || scope.row.factoryInputLaborCost" style="font-size: 0.8125rem; color: #909399; margin-bottom: 0.25rem;">
              공장입력: {{ formatPrice(scope.row.factoryInputMaterialCost || 0) }} / {{ formatPrice(scope.row.factoryInputLaborCost || 0) }}
            </div>
            <amount-input v-model="scope.row.retailerConfirmMaterialCost" placeholder="재료비" style="width: 170px" />
            <amount-input v-model="scope.row.retailerConfirmLaborCost" placeholder="수공비" style="width: 170px; margin-top: 0.25rem;" />
            <el-date-picker
              v-model="scope.row.productionDate"
              type="date"
              placeholder="제작일"
              value-format="YYYY-MM-DD"
              style="width: 170px; margin-top: 0.25rem;"
            />
            <el-input
              v-model="scope.row.logisticsMemo"
              placeholder="검수메모"
              clearable
              style="width: 170px; margin-top: 0.25rem;"
            />
          </template>
        </el-table-column>

        <el-table-column
          label="수공비 처리비율 (%)"
          width="120"
          align="center"
          header-align="center"
          :excel-formatter="(row) => `${row.settlementRatio}%`"
        >
          <template #default="scope">
            <div style="color: #909399; font-size: 0.8125rem; margin-bottom: 0.125rem;">+ (수공비 * 비율%)</div>
            <el-input-number
              v-model="scope.row.settlementRatio"
              :min="0"
              :max="500"
              :step="1"
              controls-position="right"
              style="width: 90px;"
              @change="handleRatioChange(scope.row)"
            />
          </template>
        </el-table-column>
      </base-table>

      <div class="settlement-memo-section" style="margin-top: 1.25rem;">
        <div style="font-weight: bold; font-size: 0.9rem; margin-bottom: 0.5rem; color: #606266;">정산 시작 메모</div>
        <el-input
          v-model="settlementStartMemo"
          type="textarea"
          :rows="3"
          placeholder="정산 시작 시 참고할 메모를 입력해 주세요 (선택 사항)"
          maxlength="500"
          show-word-limit
        />
      </div>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="visible = false">취소</el-button>
        <el-button type="primary" :loading="submitLoading" @click="handleInspectionSubmit">
          검수 완료 및 정산 시작
        </el-button>
      </span>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { useMobile } from '@/hooks/useMobile';
import { ref, reactive, watch } from 'vue';
import { updateOrderStatus } from '@/api/order';
import { ElMessage } from 'element-plus';
import { BottomLeft, Calendar } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import BasePopup from '@/components/BasePopup/index.vue';
import BaseTable from '@/components/BaseTable/index.vue';
import AmountInput from '@/components/AmountInput/index.vue';
const { isMobile } = useMobile();

const props = defineProps({
  modelValue: Boolean,
  order: {
    type: Object,
    default: null
  },
  codeMap: {
    type: Object,
    default: () => ({})
  }
});

const emit = defineEmits(['update:modelValue', 'completed']);

const visible = ref(props.modelValue);
const submitLoading = ref(false);

const inspectionForm = reactive({
  items: [] as any[]
});
const settlementStartMemo = ref('');
const defaultImage = 'https://via.placeholder.com/100x100?text=No+Image';

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val && props.order) {
    settlementStartMemo.value = '';
    initializeForm();
  }
});

watch(() => visible.value, (val) => {
  emit('update:modelValue', val);
});

const initializeForm = () => {
  const items: any[] = [];

  props.order.orderItems.forEach((item: any) => {

    items.push({
      orderItemId: item.id,
      productName: item.productName,
      productSetTitle: item.productSetTitle,
      productNo: item.productNo,
      photoUrl: item.photoUrl,
      quantity: item.quantity,
      weight: item.weight,
      factoryPrice: item.factoryPrice,
      laborCost: item.laborCost,
      settlementRatio: item.settlementRatio || 70,
      retailerConfirmMaterialCost: item.retailerConfirmMaterialCost > 0 ? item.retailerConfirmMaterialCost : (item.factoryInputMaterialCost || 0),
      retailerConfirmLaborCost: (item.factoryInputLaborCost || 0) + Math.round((item.factoryInputLaborCost || 0) * ((item.settlementRatio || 70) / 100)),
      factoryInputMaterialCost: item.factoryInputMaterialCost,
      factoryInputLaborCost: item.factoryInputLaborCost,
      size: item.size,
      approvedWeight: item.approvedWeight,
      approvedMemo: item.approvedMemo,
      requestedWeight: item.requestedWeight,
      requestedMemo: item.requestedMemo,
      actualWeight: item.actualWeight,
      inspectionMemo: item.inspectionMemo,

      confirmedWeight: item.confirmedWeight || item.actualWeight || item.requestedWeight || item.approvedWeight || 0,
      logisticsMemo: item.logisticsMemo || '',

      productionDate: item.productionDate || parseTime(new Date(), '{y}-{m}-{d}'),
      purity: item.purity,
      color: item.color,
      manufacturerName: item.manufacturerName,
      isAsOrder: item.isAsOrder,
      isSet: !!item.productSetId,
      depth: 0
    });

    if (item.children && item.children.length > 0) {
      item.children.forEach((child: any) => {
        items.push({
          orderItemId: child.id,
          productName: child.productName,
          productSetTitle: child.productSetTitle,
          productNo: child.productNo,
          photoUrl: child.photoUrl,
          quantity: child.quantity,
          weight: child.weight,
          factoryPrice: child.factoryPrice,
          laborCost: child.laborCost,
          settlementRatio: child.settlementRatio || 70,
          retailerConfirmMaterialCost: child.retailerConfirmMaterialCost > 0 ? child.retailerConfirmMaterialCost : (child.factoryInputMaterialCost || 0),
          retailerConfirmLaborCost: (child.factoryInputLaborCost || 0) + Math.round((child.factoryInputLaborCost || 0) * ((child.settlementRatio || 70) / 100)),
          factoryInputMaterialCost: child.factoryInputMaterialCost,
          factoryInputLaborCost: child.factoryInputLaborCost,
          approvedWeight: child.approvedWeight,
          approvedMemo: child.approvedMemo,
          requestedWeight: child.requestedWeight,
          requestedMemo: child.requestedMemo,
          actualWeight: child.actualWeight,
          inspectionMemo: child.inspectionMemo,
          confirmedWeight: child.confirmedWeight || child.actualWeight || child.requestedWeight || child.approvedWeight || 0,
          logisticsMemo: child.logisticsMemo || '',
          productionDate: child.productionDate || parseTime(new Date(), '{y}-{m}-{d}'),
          purity: child.purity,
          color: child.color,
          size: child.size,
          manufacturerName: child.manufacturerName,
          isAsOrder: child.isAsOrder,
          isChild: true,
          depth: 1
        });
      });
    }
  });

  inspectionForm.items = items;
};

const handleRatioChange = (row: any) => {
  const factoryLabor = row.factoryInputLaborCost || 0;
  const ratio = row.settlementRatio || 0;
  row.retailerConfirmLaborCost = factoryLabor + Math.round(factoryLabor * (ratio / 100));
};

const handleInspectionSubmit = async () => {
  if (!props.order) return;

  submitLoading.value = true;
  try {
    const data = {
      status: 'PENDING',
      settlementStartMemo: settlementStartMemo.value,

      itemWeights: inspectionForm.items.map(item => ({
        orderItemId: item.orderItemId,
        confirmedWeight: item.confirmedWeight,
        logisticsMemo: item.logisticsMemo,
        productionDate: item.productionDate,
        retailerConfirmMaterialCost: item.retailerConfirmMaterialCost,
        retailerConfirmLaborCost: item.retailerConfirmLaborCost,
        settlementRatio: item.settlementRatio
      }))
    };

    await updateOrderStatus(props.order.id, data);
    ElMessage.success('검수 완료 및 정산 시작 처리가 되었습니다. 주문이 정산대기 상태로 변경되었습니다.');
    visible.value = false;
    emit('completed');
  } catch (error) {
    console.error('Failed to update status:', error);
  } finally {
    submitLoading.value = false;
  }
};

const handleClosed = () => {
  inspectionForm.items = [];
};

const tableRowClassName = ({ row }: { row: any }) => {
  if (row.isChild) {
    return 'child-row';
  }
  return '';
};
</script>

<style lang="scss" src="./InspectionDialogStyles.scss" scoped></style>
