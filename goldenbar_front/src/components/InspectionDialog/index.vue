<template>
  <base-popup draggable
              v-model="visible"
              width="90%"
              style="max-width: 1600px;"
              @close="handleClosed"
  >
    <template #header>
      <div class="luxury-dialog-header">
        <span class="el-dialog__title">물류 검수 확인 및 완료</span>
        <div v-if="order?.deliveryDate" class="delivery-date-badge">
          <el-icon><Calendar /></el-icon>
          <span class="label">납기일:</span>
          <span class="val">{{ order.deliveryDate.substring(0, 10) }}</span>
        </div>
      </div>
    </template>
    <el-form :model="inspectionForm" label-position="top">
      <base-table :data="inspectionForm.items" border style="width: 100%; margin-bottom: 1.25rem;" :row-class-name="tableRowClassName">
        <el-table-column label="사진" prop="photoUrl" :fixed="!isMobile ? 'left' : false" width="80" align="center" header-align="center" class-name="photo-column">
          <template #default="scope">
            <el-image :src="scope.row.photoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 100%; min-height: 80px; display: block;" />
          </template>
        </el-table-column>
        <el-table-column
          label="상품 정보"
          prop="productName"
          min-width="200"
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
                <div class="product-options" v-if="(scope.row.purity && scope.row.purity.toUpperCase() !== 'EMPTY') || (scope.row.color && scope.row.color.toUpperCase() !== 'EMPTY')" style="margin-top: 0.3125rem;">
                  <el-tag size="small" type="info" effect="plain" v-if="scope.row.purity && scope.row.purity.toUpperCase() !== 'EMPTY'" style="height: 16px !important; line-height: 14px !important; padding: 0 0.25rem !important;">{{ codeMap[scope.row.purity] || scope.row.purity }}</el-tag>
                  <el-tag size="small" type="info" effect="plain" v-if="scope.row.color && scope.row.color.toUpperCase() !== 'EMPTY'" :style="{ marginLeft: (scope.row.purity && scope.row.purity.toUpperCase() !== 'EMPTY') ? '5px' : '0' }" style="font-size: 0.825rem !important; height: 16px !important; line-height: 14px !important; padding: 0 0.25rem !important;">{{ codeMap[scope.row.color] || scope.row.color }}</el-tag>
                </div>
              </div>
            </div>
          </template>
        </el-table-column>

        <el-table-column
          label="수량/AS"
          prop="quantity"
          width="60"
          align="center"
          header-align="center"
          :excel-formatter="(row) => `${row.quantity}${row.isAsOrder ? ' (AS)' : ''}`"
        >
          <template #default="scope">
            <el-tag >{{ scope.row.quantity }}</el-tag>
            <el-tag v-if="scope.row.isAsOrder" type="danger" size="small">AS</el-tag>
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
            <span>{{ scope.row.approvedWeight ? scope.row.approvedWeight + 'g' : '-' }}</span>
            <br />
            <span style="font-size: 0.8875rem;">{{ scope.row.approvedMemo || '-' }}</span>
          </template>
        </el-table-column>

        <el-table-column
          label="공장 의뢰"
          prop="requestedWeight"
          align="center"
          header-align="center"
          width="180"
          :excel-formatter="(row) => `${row.requestedWeight ? row.requestedWeight + 'g' : '-'}${row.requestedMemo ? '\n메모: ' + row.requestedMemo : ''}`"
        >
          <template #default="scope">
            <span>{{ scope.row.requestedWeight ? scope.row.requestedWeight + 'g' : '-' }}</span>
            <br />
            <span style="font-size: 0.8875rem;">{{ scope.row.requestedMemo || '-' }}</span>
          </template>
        </el-table-column>

        <el-table-column label="공장 검수요청" align="center" header-align="center">
          <el-table-column
            label="실중량"
            prop="actualWeight"
            width="80"
            align="center"
            header-align="center"
            :excel-formatter="(row) => row.actualWeight ? row.actualWeight + 'g' : '-'"
          >
            <template #default="scope">
              <span style="font-weight: bold; color: #409EFF;">{{ scope.row.actualWeight ? scope.row.actualWeight + 'g' : '-' }}</span>
            </template>
          </el-table-column>
          <el-table-column
            label="공장입력 실비"
            prop="factoryInputMaterialCost"
            align="right"
            header-align="center"
            width="180"
            :excel-formatter="(row) => {
              const costs = [
                row.factoryInputMaterialCost ? '재료비: ' + formatPrice(row.factoryInputMaterialCost) : '',
                row.factoryInputLaborCost ? '수공비: ' + formatPrice(row.factoryInputLaborCost) : ''
              ].filter(Boolean).join('\n');
              return (costs || '-') + (row.inspectionMemo ? '\n메모: ' + row.inspectionMemo : '');
            }"
          >
            <template #default="scope">
              <div style="font-size: 0.8875rem; color: #606266;">
                <div v-if="scope.row.factoryInputMaterialCost">재료비: {{ formatPrice(scope.row.factoryInputMaterialCost || 0) }}</div>
                <div v-if="scope.row.factoryInputLaborCost">수공비: {{ formatPrice(scope.row.factoryInputLaborCost || 0) }}</div>
                <div v-if="!scope.row.factoryInputMaterialCost && !scope.row.factoryInputLaborCost">-</div>
              </div>
              <span style="font-size: 0.8875rem;">{{ scope.row.inspectionMemo || '-' }}</span>
            </template>
          </el-table-column>
        </el-table-column>

        <el-table-column
          label="최종 검수 (물류)"
          align="center"
          :fixed="!isMobile ? 'right' : false"
          width="250"
          class-name="final-inspection-col"
          header-align="center"
          :excel-formatter="(row) => {
            return `제작일: ${row.productionDate || '-'}\n중량: ${row.confirmedWeight || 0}g\n메모: ${row.logisticsMemo || '-'}`;
          }"
        >

          <template #default="scope">
            <div class="inspection-item">
              <label>제작일:</label>
              <el-date-picker
                v-model="scope.row.productionDate"
                type="date"
                placeholder="날짜 선택"
                value-format="YYYY-MM-DD"
                style="width: 170px"
              />
            </div>

            <div class="inspection-item">
              <label>검수중량:</label>
              <el-input-number
                v-model="scope.row.confirmedWeight"
                :precision="2"
                :step="0.1"
                :min="0"
                style="width: 170px"
              />
            </div>

            <div class="inspection-item">
              <label>검수메모:</label>
              <el-input
                v-model="scope.row.logisticsMemo"
                placeholder="검수 결과 입력"
                clearable
                style="width: 170px"
              />
            </div>
          </template>
        </el-table-column>
      </base-table>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="visible = false">취소</el-button>
        <el-button type="primary" :loading="submitLoading" @click="handleInspectionSubmit">
          검수 완료 확정
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
const defaultImage = 'https://via.placeholder.com/100x100?text=No+Image';

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val && props.order) {
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
      retailerConfirmMaterialCost: item.retailerConfirmMaterialCost,
      retailerConfirmLaborCost: item.retailerConfirmLaborCost,
      factoryInputMaterialCost: item.factoryInputMaterialCost,
      factoryInputLaborCost: item.factoryInputLaborCost,
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

const handleInspectionSubmit = async () => {
  if (!props.order) return;

  submitLoading.value = true;
  try {
    const data = {
      status: 'Inspected', 

      itemWeights: inspectionForm.items.map(item => ({
        orderItemId: item.orderItemId,
        confirmedWeight: item.confirmedWeight,
        logisticsMemo: item.logisticsMemo,
        productionDate: item.productionDate
      }))
    };

    await updateOrderStatus(props.order.id, data);
    ElMessage.success('검수 확인 및 완료 처리가 되었습니다.');
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
