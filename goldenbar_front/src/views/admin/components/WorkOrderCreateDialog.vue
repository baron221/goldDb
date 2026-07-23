<template>
  <base-popup v-model="visible" :title="$t('admin.inspectionRequest.title')" width="90%" style="max-width: 1100px;" @close="handleClose">
    <el-form :model="completeForm" label-position="top">
      <base-table :data="completeForm.items" border style="width: 100%; margin-bottom: 1.25rem;" :row-class-name="tableRowClassName">
        <el-table-column :label="$t('orderDetail.headers.productInfo')" min-width="250" :excel-formatter="(row) => (row.isSet ? '[SET] ' : '') + (row.productName || row.productSetTitle) + ' (' + row.productNo + ') ' + (row.manufacturerName || '')">
          <template #default="scope">
            <div class="product-info-cell" :style="{ paddingLeft: scope.row.depth * 20 + 'px' }">
              <el-icon v-if="scope.row.isChild" style="margin-right: 0.3125rem; color: #909399;"><BottomLeft /></el-icon>
              <el-image :src="scope.row.photoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 80px; height: 80px;" />
              <div class="product-text">
                <div class="product-name" style="font-size: 0.9rem;">
                  <el-tag v-if="scope.row.isSet" size="small" type="warning" style="margin-right: 0.3125rem;">SET</el-tag>
                  {{ scope.row.productName || scope.row.productSetTitle }}
                </div>
                <div class="product-no" style="font-size: 0.8875rem;">{{ scope.row.productNo }}</div>
                <span style="font-size: 0.95rem; color: #E6A23C;">{{ scope.row.manufacturerName || '-' }}</span>
              </div>

            </div>
          </template>
        </el-table-column>
        <el-table-column :label="$t('orderDetail.headers.qty')" width="70" align="center" prop="quantity" />
        <el-table-column label="AS여부" width="80" align="center" header-align="center" :excel-formatter="(row) => row.isAsOrder ? 'Y' : 'N'">
          <template #default="scope">
            <el-icon v-if="scope.row.isAsOrder" :size="24" color="#F56C6C" style="font-weight: bold;">
              <Check />
            </el-icon>
            <span v-else style="color: #E4E7ED;">-</span>
          </template>
        </el-table-column>
        <el-table-column :label="$t('admin.inspectionRequest.headers.options')" width="110" align="center" :excel-formatter="(row) => [getCodeName(row.purity), row.color && row.color !== 'EMPTY' ? getCodeName(row.color) : ''].filter(Boolean).join(', ')">
          <template #default="scope">
            <div class="option-cell-luxury" style="display: flex; flex-direction: column; gap: 0.25rem; align-items: center; justify-content: center;">
              <el-tag v-if="scope.row.purity" size="small" type="info" effect="plain" style="width: fit-content;">
                {{ getCodeName(scope.row.purity) }}
              </el-tag>
              <el-tag v-if="scope.row.color && scope.row.color !== 'EMPTY'" size="small" type="warning" effect="plain" style="font-size: 0.825rem; width: fit-content;">
                {{ getCodeName(scope.row.color) }}
              </el-tag>
              <span v-if="!scope.row.purity && !scope.row.color">-</span>
            </div>
          </template>
        </el-table-column>

        <el-table-column :label="$t('admin.inspectionRequest.headers.factoryRequest')" width="130" align="center" :excel-formatter="(row) => `의뢰: ${row.requestedWeight || 0}g\n실중량: ${row.actualWeight || 0}g`">
          <template #default="scope">
            <el-tooltip :content="scope.row.requestedMemo || $t('admin.inspectionRequest.labels.noMemo')" placement="top" :disabled="!scope.row.requestedMemo">
              <span style="color: #E6A23C; font-weight: bold;">{{ scope.row.requestedWeight ? scope.row.requestedWeight + 'g' : '-' }}</span>
            </el-tooltip>

            <el-input-number
              v-model="scope.row.actualWeight"
              :precision="2"
              :step="0.1"
              :min="0"
              style="width: 110px"
            />

          </template>
        </el-table-column>

        <el-table-column :label="$t('admin.inspectionRequest.headers.factoryInput')" width="230" align="center" :excel-formatter="(row) => `재료비: ${row.factoryInputMaterialCost || 0}\n수공비: ${row.factoryInputLaborCost || 0}\n메모: ${row.inspectionMemo || ''}`">
          <template #default="scope">
            <div class="factory-input-item">
              <label>재료비:</label>
              <amount-input
                v-model="scope.row.factoryInputMaterialCost"
                :placeholder="$t('admin.inspectionRequest.placeholders.material')"
                style="width: 130px;"
              />
            </div>

            <div class="factory-input-item">
              <label>수공비:</label>
              <amount-input
                v-model="scope.row.factoryInputLaborCost"
                :placeholder="$t('admin.inspectionRequest.placeholders.labor')"
                style="width: 130px;"
              />
            </div>

            <div class="factory-input-item">
              <label>메모:</label>
              <el-input
                v-model="scope.row.inspectionMemo"
                :placeholder="$t('admin.inspectionRequest.placeholders.memo')"
                clearable
                style="width: 130px;"
              />
            </div>
          </template>
        </el-table-column>
      </base-table>

      <el-row :gutter="24" class="memo-action-row">
        <el-col :xs="24" :sm="12" class="mb-4 sm:mb-0">
          <el-form-item :label="$t('admin.inspectionRequest.labels.logisticsRemarks')">
            <div class="memo-view-panel">
              <div class="memo-header-info">
                <span class="info-item"><span class="label-tiny">{{ $t('admin.inspectionRequest.labels.company') }}</span> {{ props.order?.logisticsCompanyName || $t('home.roles.logistics.title') }}</span>
                <span class="info-item date"><span class="label-tiny">{{ $t('admin.inspectionRequest.labels.date') }}</span> {{ parseTime(props.order?.updatedAt || props.order?.createdAt, '{y}-{m}-{d} {h}:{i}') }}</span>
              </div>
              <div class="memo-body-content">
                <i class="fas fa-comment-dots quote-icon"></i>
                <div class="memo-content">
                  {{ completeForm.factoryRemarks || $t('admin.inspectionRequest.labels.noLogisticsMemo') }}
                </div>
              </div>
            </div>
          </el-form-item>
        </el-col>
        <el-col :xs="24" :sm="12">
          <el-form-item :label="$t('admin.inspectionRequest.labels.workOrderMemo')">
            <el-input
              v-model="completeForm.inspectionRemarks"
              type="textarea"
              :rows="4"
              :placeholder="$t('admin.inspectionRequest.placeholders.memo')"
              class="luxury-textarea"
            />
          </el-form-item>
        </el-col>
      </el-row>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button :icon="Printer" @click="handlePrint">{{ $t('admin.inspectionRequest.print') }}</el-button>
        <el-button @click="visible = false">{{ $t('common.cancel') }}</el-button>
        <el-button type="primary" :loading="submitting" @click="handleCompleteSubmit">
          {{ $t('common.ok') }}
        </el-button>
      </span>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { updateOrderStatus } from '@/api/order';
import { ElMessage } from 'element-plus';
import { BottomLeft, Check, Printer } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import BasePopup from '@/components/BasePopup/index.vue';
import AmountInput from '@/components/AmountInput/index.vue';
import BaseTable from '@/components/BaseTable/index.vue';
import { printWorkOrder } from '../utils/workOrderPrint';

const { t } = useI18n();
const props = defineProps({
  modelValue: Boolean,
  order: {
    type: Object,
    default: () => null
  },
  codeMap: {
    type: Object,
    default: () => ({})
  }
});

const emit = defineEmits(['update:modelValue', 'saved']);

const getCodeName = (code: string) => {
  if (!code) return '';
  return (props.codeMap && props.codeMap[code]) || code;
};

const visible = ref(false);
const submitting = ref(false);
const defaultImage = 'https://via.placeholder.com/100x100?text=No+Image';

const completeForm = reactive({
  items: [] as any[],
  factoryRemarks: '',
  inspectionRemarks: ''
});

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val && props.order) {
    initializeForm();
  }
});

watch(visible, (val) => {
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
      purity: item.purity,
      color: item.color,
      factoryPrice: item.factoryPrice,
      laborCost: item.laborCost,
      factoryInputMaterialCost: item.factoryInputMaterialCost || item.retailerConfirmMaterialCost || item.factoryPrice || 0,
      factoryInputLaborCost: item.factoryInputLaborCost || item.retailerConfirmLaborCost || item.laborCost || 0,
      approvedWeight: item.approvedWeight || 0,
      approvedMemo: item.approvedMemo || '',
      requestedWeight: item.requestedWeight || 0,
      requestedMemo: item.requestedMemo || '',
      actualWeight: item.actualWeight || item.requestedWeight || 0,
      inspectionMemo: item.inspectionMemo || '',
      isAsOrder: !!item.isAsOrder,
      isSet: !!item.productSetId,
      manufacturerName: item.manufacturerName,
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
          purity: child.purity,
          color: child.color,
          factoryPrice: child.factoryPrice,
          laborCost: child.laborCost,
          factoryInputMaterialCost: child.factoryInputMaterialCost || child.retailerConfirmMaterialCost || child.factoryPrice || 0,
          factoryInputLaborCost: child.factoryInputLaborCost || child.retailerConfirmLaborCost || child.laborCost || 0,
          approvedWeight: child.approvedWeight || 0,
          approvedMemo: child.approvedMemo || '',
          requestedWeight: child.requestedWeight || 0,
          requestedMemo: child.requestedMemo || '',
          actualWeight: child.actualWeight || child.requestedWeight || 0,
          inspectionMemo: child.inspectionMemo || '',
          isAsOrder: !!child.isAsOrder,
          manufacturerName: child.manufacturerName,
          isChild: true,
          depth: 1
        });
      });
    }
  });

  completeForm.items = items;
  completeForm.factoryRemarks = props.order.factoryRemarks || '';
  completeForm.inspectionRemarks = props.order.workOrderRemarks || '';
};

const handleClose = () => {
  visible.value = false;
  completeForm.items = [];
  completeForm.factoryRemarks = '';
  completeForm.inspectionRemarks = '';
};

const handlePrint = () => {
  printWorkOrder(
    {
      orderNo: props.order?.orderNo,
      logisticsCompanyName: props.order?.logisticsCompanyName,
      manufacturerName: props.order?.manufacturerName,
      factoryRemarks: completeForm.factoryRemarks,
      workOrderRemarks: completeForm.inspectionRemarks
    },
    completeForm.items,
    props.codeMap as Record<string, string>
  );
};

const tableRowClassName = ({ row }: { row: any }) => {
  if (row.isChild) {
    return 'child-row';
  }
  return '';
};

const handleCompleteSubmit = async () => {
  if (!props.order) return;

  submitting.value = true;
  try {
    const data = {
      status: 'WorkOrderCreated',
      workOrderRemarks: completeForm.inspectionRemarks,
      itemWeights: completeForm.items.map(item => ({
        orderItemId: item.orderItemId,
        actualWeight: item.actualWeight,
        inspectionMemo: item.inspectionMemo,
        factoryInputMaterialCost: item.factoryInputMaterialCost,
        factoryInputLaborCost: item.factoryInputLaborCost
      }))
    };

    await updateOrderStatus(props.order.id, data);
    ElMessage.success(t('admin.inspectionRequest.messages.success'));
    visible.value = false;
    emit('saved');
  } catch (error) {
    console.error('Failed to update status:', error);
    ElMessage.error(t('admin.inspectionRequest.messages.error'));
  } finally {
    submitting.value = false;
  }
};
</script>

<style src="./WorkOrderCreateDialogStyles.scss" scoped></style>
