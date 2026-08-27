<template>
  <base-popup v-model="visible" :title="$t('admin.inspectionRequest.productDispatch')" width="90%" style="max-width: 1100px;" @close="handleClose">
    <el-form :model="completeForm" label-position="top">
      <base-table :data="completeForm.items" border style="width: 100%; margin-bottom: 1.25rem;" :row-class-name="tableRowClassName">
        <el-table-column
          :label="$t('admin.inspectionRequest.headers.productInfo')"
          min-width="200"
          :excel-formatter="(row: any) => (row.productName || row.productSetTitle) + ' (' + row.productNo + ')'"
        >
          <template #default="scope">
            <div class="product-info-cell" :style="{ paddingLeft: scope.row.depth * 20 + 'px' }">
              <el-icon v-if="scope.row.isChild" style="margin-right: 0.3125rem; color: #909399;"><BottomLeft /></el-icon>
              <el-image :src="scope.row.photoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 40px; height: 40px;" />
              <div class="product-text">
                <div class="product-name" style="font-size: 0.9rem;">
                  <el-tag v-if="scope.row.isSet" size="small" type="warning" style="margin-right: 0.3125rem;">SET</el-tag>
                  {{ scope.row.productName || scope.row.productSetTitle }}
                </div>
                <div class="product-no" style="font-size: 0.8875rem;">{{ scope.row.productNo }}</div>
                <span style="font-size: 0.95rem; color: #E6A23C;">물류: {{ props.order?.logisticsCompanyName || '-' }}</span>
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column :label="$t('admin.inspectionRequest.headers.qty')" width="70" align="center" prop="quantity" />
        <el-table-column
          :label="$t('admin.inspectionRequest.headers.options')"
          width="110"
          align="center"
          :excel-formatter="(row: any) => [getCodeName(row.purity), row.color && row.color !== 'EMPTY' ? getCodeName(row.color) : '', row.size && row.size !== 'EMPTY' ? row.size : ''].filter(Boolean).join(', ')"
        >
          <template #default="scope">
            <div class="option-cell-luxury" style="display: flex; flex-direction: column; gap: 0.25rem; align-items: center; justify-content: center;">
              <el-tag v-if="scope.row.purity" size="small" type="info" effect="plain" style="font-size: 0.825rem; width: fit-content;">
                {{ getCodeName(scope.row.purity) }}
              </el-tag>
              <el-tag v-if="scope.row.color && scope.row.color !== 'EMPTY'" size="small" type="warning" effect="plain" style="font-size: 0.825rem; width: fit-content;">
                {{ getCodeName(scope.row.color) }}
              </el-tag>
              <el-tag v-if="scope.row.size && scope.row.size !== 'EMPTY'" size="small" type="success" effect="plain" style="font-size: 0.825rem; width: fit-content;">
                {{ $t('admin.inspectionRequest.headers.size') }}: {{ scope.row.size }}
              </el-tag>
              <span v-if="!scope.row.purity && !scope.row.color && !scope.row.size">-</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column
          :label="$t('admin.inspectionRequest.headers.logisticsApproval')"
          width="100"
          align="center"
          :excel-formatter="(row: any) => row.approvedWeight ? row.approvedWeight + 'g' : '-'"
        >
          <template #default="scope">
            <el-tooltip :content="scope.row.approvedMemo || $t('admin.inspectionRequest.labels.noMemo')" placement="top" :disabled="!scope.row.approvedMemo">
              <span style="color: #67C23A;">{{ scope.row.approvedWeight ? scope.row.approvedWeight + 'g' : '-' }}</span>
            </el-tooltip>
          </template>
        </el-table-column>
        <el-table-column
          :label="$t('admin.inspectionRequest.headers.factoryRequest')"
          width="150"
          align="center"
          :excel-formatter="(row: any) => row.requestedWeight ? row.requestedWeight + 'g' : '-'"
        >
          <template #default="scope">
            <el-tooltip :content="scope.row.requestedMemo || $t('admin.inspectionRequest.labels.noMemo')" placement="top" :disabled="!scope.row.requestedMemo">
              <span style="color: #E6A23C; font-weight: bold;">{{ scope.row.requestedWeight ? scope.row.requestedWeight + 'g' : '-' }}</span>
            </el-tooltip>

            <br />
            <el-input-number
              v-model="scope.row.rawWeight"
              :precision="2"
              :step="0.1"
              :min="0"
              style="width: 110px"
              @change="recalcWeight(scope.row)"
            />
            <div v-if="scope.row.basicLoss > 0" class="loss-checkbox-row">
              <el-checkbox v-model="scope.row.applyLoss" @change="recalcWeight(scope.row)">
                {{ $t('admin.inspectionRequest.labels.applyLoss') }} ({{ scope.row.basicLoss }}g)
              </el-checkbox>
              <div v-if="scope.row.applyLoss" class="loss-result">
                → {{ scope.row.actualWeight.toFixed(2) }}g
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column
          :label="$t('admin.inspectionRequest.headers.factoryInput')"
          width="200"
          align="center"
          :excel-formatter="(row: any) => row.inspectionMemo"
        >
          <template #default="scope">
            <amount-input
              v-model="scope.row.factoryInputMaterialCost"
              :placeholder="$t('admin.inspectionRequest.placeholders.material')"
            />

            <amount-input
              v-model="scope.row.factoryInputLaborCost"
              :placeholder="$t('admin.inspectionRequest.placeholders.labor')"
            />

            <el-input
              v-model="scope.row.inspectionMemo"
              :placeholder="$t('admin.inspectionRequest.placeholders.memo')"
              clearable
            />
          </template>
        </el-table-column>
      </base-table>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
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
import { BottomLeft } from '@element-plus/icons-vue';
import BasePopup from '@/components/BasePopup/index.vue';
import BaseTable from '@/components/BaseTable/index.vue';
import AmountInput from '@/components/AmountInput/index.vue';

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
const defaultImage = '/thumb_no_img.png';

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
      size: item.size,
      factoryPrice: item.factoryPrice,
      laborCost: item.laborCost,
      factoryInputMaterialCost: item.factoryInputMaterialCost || item.retailerConfirmMaterialCost || item.factoryPrice || 0,
      factoryInputLaborCost: item.factoryInputLaborCost || item.retailerConfirmLaborCost || item.laborCost || 0,
      approvedWeight: item.approvedWeight || 0,
      approvedMemo: item.approvedMemo || '',
      requestedWeight: item.requestedWeight || 0,
      requestedMemo: item.requestedMemo || '',
      rawWeight: item.actualWeight || item.requestedWeight || 0,
      actualWeight: item.actualWeight || item.requestedWeight || 0,
      basicLoss: item.basicLoss || 0,
      applyLoss: false,
      inspectionMemo: item.inspectionMemo || '',
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
          size: child.size,
          factoryPrice: child.factoryPrice,
          laborCost: child.laborCost,
          factoryInputMaterialCost: child.factoryInputMaterialCost || child.retailerConfirmMaterialCost || child.factoryPrice || 0,
          factoryInputLaborCost: child.factoryInputLaborCost || child.retailerConfirmLaborCost || child.laborCost || 0,
          approvedWeight: child.approvedWeight || 0,
          approvedMemo: child.approvedMemo || '',
          requestedWeight: child.requestedWeight || 0,
          requestedMemo: child.requestedMemo || '',
          rawWeight: child.actualWeight || child.requestedWeight || 0,
          actualWeight: child.actualWeight || child.requestedWeight || 0,
          basicLoss: child.basicLoss || 0,
          applyLoss: false,
          inspectionMemo: child.inspectionMemo || '',
          manufacturerName: child.manufacturerName,
          isChild: true,
          depth: 1
        });
      });
    }
  });

  completeForm.items = items;
  completeForm.factoryRemarks = props.order.factoryRemarks || '';
  completeForm.inspectionRemarks = props.order.inspectionRemarks || '';
};

const handleClose = () => {
  visible.value = false;
  completeForm.items = [];
  completeForm.factoryRemarks = '';
  completeForm.inspectionRemarks = '';
};

// 실중량 is whatever the factory measures on the scale (rawWeight). When the product
// has a registered 기본감량(basicLoss) - a fixed gram amount (casting/stone loss) the
// factory declared at product registration - the checkbox lets them subtract it right
// here instead of doing the math by hand; unchecked, the typed number is used as-is.
const recalcWeight = (row: any) => {
  const raw = row.rawWeight || 0;
  const loss = row.basicLoss || 0;
  row.actualWeight = row.applyLoss && loss > 0
    ? Math.max(Math.round((raw - loss) * 100) / 100, 0)
    : raw;
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
      status: 'InspectedRequested',
      inspectionRemarks: completeForm.inspectionRemarks,
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

<style src="./InspectionRequestDialogStyles.scss" scoped></style>
