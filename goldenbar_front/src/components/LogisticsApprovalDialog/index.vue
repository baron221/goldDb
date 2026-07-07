<template>
  <base-popup
    v-model="visible"
    title="물류 승인 상세 정보"
    width="90%"
    style="max-width: 1100px;"
    @close="handleClosed"
  >
    <el-form :model="approvalForm" label-position="top">
      <base-table :data="approvalForm.items" style="width: 100%; margin-bottom: 1.25rem;" :row-class-name="tableRowClassName">
        <el-table-column label="사진" prop="photoUrl" width="60" align="center" header-align="center" class-name="photo-column" :fixed="!isMobile ? 'left' : false">
          <template #default="scope">
            <el-image :src="scope.row.photoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 60px; height: 60px; display: block;" />
          </template>
        </el-table-column>
        <el-table-column
          label="상품 정보"
          min-width="250"
          header-align="center"
          :fixed="!isMobile ? 'left' : false"
          :excel-formatter="(row) => {
            const name = row.productName || row.productSetTitle || '';
            const no = row.productNo || '';
            const manufacturer = row.manufacturerName ? `[${row.manufacturerName}]` : '';
            const purity = row.purity && row.purity.toUpperCase() !== 'EMPTY' ? codeMap[row.purity] || row.purity : '';
            const color = row.color && row.color.toUpperCase() !== 'EMPTY' ? codeMap[row.color] || row.color : '';
            const options = [purity, color].filter(Boolean).join(', ');
            return `${row.isSet ? '[SET] ' : ''}${name}\n${no} ${manufacturer}${options ? '\n' + options : ''}`;
          }"
        >
          <template #default="scope">
            <div class="product-info-cell" :style="{ paddingLeft: scope.row.depth * 20 + 'px' }">
              <el-icon v-if="scope.row.isChild" style="margin-right: 0.3125rem; color: #909399;"><BottomLeft /></el-icon>
              <div class="product-text">
                <div class="product-name" style="font-size: 0.9rem;">
                  <el-tag v-if="scope.row.isSet" size="small" type="warning" style="margin-right: 0.3125rem;">SET</el-tag>
                  {{ scope.row.productName || scope.row.productSetTitle }}
                </div>
                <div class="product-no-row" style="display: flex; align-items: center; gap: 0.375rem; margin-top: 0.125rem; margin-bottom: 0.125rem;">
                  <span class="product-no" style="font-size: 0.8875rem;">{{ scope.row.productNo }}</span>
                  <el-tag v-if="scope.row.manufacturerName" size="small" type="warning" effect="plain" style="font-size: 0.825rem !important; height: 16px !important; line-height: 14px !important; padding: 0 0.25rem !important;">
                    {{ scope.row.manufacturerName }}
                  </el-tag>
                </div>
                <div class="product-options" v-if="(scope.row.purity && scope.row.purity.toUpperCase() !== 'EMPTY') || (scope.row.color && scope.row.color.toUpperCase() !== 'EMPTY')" style="margin-top: 0.3125rem;">
                  <el-tag size="small" type="info" effect="plain" v-if="scope.row.purity && scope.row.purity.toUpperCase() !== 'EMPTY'" style="font-size: 0.825rem !important; height: 16px !important; line-height: 14px !important; padding: 0 0.25rem !important;">{{ codeMap[scope.row.purity] || scope.row.purity }}</el-tag>
                  <el-tag size="small" type="info" effect="plain" v-if="scope.row.color && scope.row.color.toUpperCase() !== 'EMPTY'" :style="{ marginLeft: (scope.row.purity && scope.row.purity.toUpperCase() !== 'EMPTY') ? '5px' : '0' }" style="font-size: 0.825rem !important; height: 16px !important; line-height: 14px !important; padding: 0 0.25rem !important;">{{ codeMap[scope.row.color] || scope.row.color }}</el-tag>
                </div>
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="수량" width="70" align="center" prop="quantity" header-align="center" />
        <el-table-column label="AS" width="80" align="center" header-align="center" :excel-formatter="(row) => row.isAsOrder ? 'Y' : 'N'">
          <template #default="scope">
            <el-checkbox v-model="scope.row.isAsOrder" />
          </template>
        </el-table-column>

        <el-table-column label="주문 중량" prop="orderWeight" width="100" align="center" header-align="center" :excel-formatter="(row) => row.orderWeight ? `${row.orderWeight}g` : '-'">
          <template #default="scope">
            <span v-if="scope.row.orderWeight">{{ scope.row.orderWeight }}g</span>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column
          label="비용"
          width="220"
          align="center"
          header-align="center"
          :excel-formatter="(row) => `재료비: ${row.retailerConfirmMaterialCost || 0}\n수공비: ${row.retailerConfirmLaborCost || 0}`"
        >
          <template #default="scope">
            <div class="cost-row">
              <span class="cost-label">재료비:</span>
              <amount-input
                v-model="scope.row.retailerConfirmMaterialCost"
                placeholder="재료비"
                style="width: 120px;"
              />
            </div>
            <div class="cost-row" style="margin-top: 0.3125rem;">
              <span class="cost-label">수공비:</span>
              <amount-input
                v-model="scope.row.retailerConfirmLaborCost"
                placeholder="수공비"
                style="width: 120px;"
              />
            </div>
          </template>
        </el-table-column>
        <el-table-column
          label="승인중량/메모"
          min-width="200"
          header-align="center"
          :excel-formatter="(row) => `승인중량: ${row.approvedWeight || 0}g\n메모: ${row.approvedMemo || ''}`"
        >
          <template #default="scope">
            <el-input-number v-model="scope.row.approvedWeight" :precision="2" :step="0.1" style="width: 120px" /><br />
            <el-input v-model="scope.row.approvedMemo" placeholder="승인 메모" />
          </template>
        </el-table-column>
      </base-table>

      <el-row :gutter="24" class="memo-action-row">
        <el-col :span="12">
          <el-form-item label="소매점 요청사항">
            <div class="memo-view-panel">
              <div class="memo-header-info">
                <span class="info-item"><span class="label-tiny">업체:</span> {{ approvalForm.retailerCompanyName }}</span>
                <span class="info-item"><span class="label-tiny">작성자:</span> {{ approvalForm.retailerName }}</span>
                <span class="info-item date"><span class="label-tiny">일시:</span> {{ parseTime(approvalForm.orderDate, '{y}-{m}-{d} {h}:{i}') }}</span>
              </div>
              <div class="memo-body-content">
                <i class="fas fa-comment-dots quote-icon"></i>
                <div class="memo-content">
                  {{ approvalForm.logisticsRemarks || '소매점에서 전달한 메모가 없습니다' }}
                </div>
              </div>
            </div>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="제조사 전달사항">
            <el-input
              v-model="approvalForm.factoryRemarks"
              type="textarea"
              :rows="4"
              placeholder="제조사에 전달할 공통 메모를 입력하세요"
              class="luxury-textarea"
            />
          </el-form-item>
        </el-col>
      </el-row>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="visible = false">취소</el-button>
        <el-button type="primary" :loading="submitLoading" @click="handleSubmit">승인 완료</el-button>
      </div>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { useMobile } from '@/hooks/useMobile';
import { ref, reactive, watch } from 'vue';
import { BottomLeft } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import { updateOrderStatus } from '@/api/order';
import { parseTime } from '@/utils';
import BasePopup from '@/components/BasePopup/index.vue';
import AmountInput from '@/components/AmountInput/index.vue';
import BaseTable from '@/components/BaseTable/index.vue';
const { isMobile } = useMobile();

const props = defineProps<{
  modelValue: boolean;
  order: any;
  codeMap: Record<string, string>;
}>();

const emit = defineEmits(['update:modelValue', 'completed']);

const visible = ref(false);
const submitLoading = ref(false);
const defaultImage = 'https://via.placeholder.com/100x100?text=No+Image';

const approvalForm = reactive({
  items: [] as any[],
  factoryRemarks: '',
  orderMemo: '',
  logisticsRemarks: '',
  retailerName: '',
  retailerCompanyName: '',
  orderDate: ''
});

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val && props.order) {
    initForm();
  }
});

watch(() => visible.value, (val) => {
  emit('update:modelValue', val);
});

const initForm = () => {
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
      orderWeight: item.orderWeight,

      approvedWeight: item.approvedWeight || item.orderWeight || item.weight || 0,
      approvedMemo: item.approvedMemo || '',
      purity: item.purity,
      color: item.color,
      isAsOrder: !!item.isAsOrder,
      isSet: !!item.productSetId,
      manufacturerName: item.manufacturerName,

      retailerConfirmMaterialCost: item.retailerConfirmMaterialCost || item.factoryPrice || 0,
      retailerConfirmLaborCost: item.retailerConfirmLaborCost || item.laborCost || 0,
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
          orderWeight: child.orderWeight,
          approvedWeight: child.approvedWeight || child.weight || 0,
          approvedMemo: child.approvedMemo || '',
          purity: child.purity,
          color: child.color,
          isAsOrder: !!child.isAsOrder,
          manufacturerName: child.manufacturerName,
          retailerConfirmMaterialCost: child.retailerConfirmMaterialCost || child.factoryPrice || 0,
          retailerConfirmLaborCost: child.retailerConfirmLaborCost || child.laborCost || 0,
          isChild: true,
          depth: 1
        });
      });
    }
  });

  approvalForm.items = items;

  approvalForm.orderMemo = props.order.orderMemo || '';
  approvalForm.factoryRemarks = props.order.factoryRemarks || '';
  approvalForm.logisticsRemarks = props.order.logisticsRemarks || '';
  approvalForm.retailerName = props.order.userDisplayName || '';
  approvalForm.retailerCompanyName = props.order.companyName || '';
  approvalForm.orderDate = props.order.createdAt || '';
};

const handleClosed = () => {
  approvalForm.items = [];
  approvalForm.factoryRemarks = '';
  approvalForm.orderMemo = '';
  approvalForm.logisticsRemarks = '';
  approvalForm.retailerName = '';
  approvalForm.retailerCompanyName = '';
  approvalForm.orderDate = '';
};

const handleSubmit = async () => {
  if (!props.order) return;

  submitLoading.value = true;
  try {
    const data = {
      status: 'LogisticsApproved', 
      factoryRemarks: approvalForm.factoryRemarks,

      itemWeights: approvalForm.items.map(item => ({
        orderItemId: item.orderItemId,
        approvedWeight: item.approvedWeight,
        approvedMemo: item.approvedMemo,
        isAsOrder: !!item.isAsOrder,
        retailerConfirmMaterialCost: item.retailerConfirmMaterialCost,
        retailerConfirmLaborCost: item.retailerConfirmLaborCost
      }))
    };

    await updateOrderStatus(props.order.id, data);
    ElMessage.success('물류 승인이 완료되었습니다.');
    visible.value = false;
    emit('completed'); 
  } catch (error) {
    console.error('Failed to update status:', error);
  } finally {
    submitLoading.value = false;
  }
};

const tableRowClassName = ({ row }: { row: any }) => {
  if (row.isChild) {
    return 'child-row';
  }
  return '';
};
</script>

<style src="./LogisticsApprovalDialogStyles.scss" scoped></style>
