<template>
<base-popup v-model="visible" :title="$t('home.roles.factory.title') + ' ' + $t('common.create')" width="90%" style="max-width: 1000px;" @close="handleClose">
    <el-form :model="factoryForm" label-position="top">
      <base-table :data="factoryForm.items" style="width: 100%; margin-bottom: 1.25rem;" :row-class-name="tableRowClassName">
        <el-table-column :label="$t('admin.inspectionRequest.headers.productInfo')" min-width="250" :excel-formatter="(row) => (row.isSet ? '[SET] ' : '') + (row.productName || row.productSetTitle) + ' (' + row.productNo + ')'">
          <template #default="scope">
            <div class="product-info-cell" :style="{ paddingLeft: scope.row.depth * 20 + 'px' }">
              <el-icon v-if="scope.row.isChild" style="margin-right: 0.3125rem; color: #909399;"><BottomLeft /></el-icon>
              <el-image :src="scope.row.photoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 40px; height: 40px; margin-right: 0.625rem;" />
              <div class="product-text">
                <div class="product-name" style="font-size: 0.9rem;">
                  <el-tag v-if="scope.row.isSet" size="small" type="warning" style="margin-right: 0.3125rem;">SET</el-tag>
                  {{ scope.row.productName || scope.row.productSetTitle }}
                </div>
                <div class="product-no" style="font-size: 0.8875rem;">{{ scope.row.productNo }}</div>
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column :label="$t('admin.inspectionRequest.headers.qty')" width="70" align="center" prop="quantity" />
        <el-table-column :label="$t('admin.orderApproval.headers.approvedWeight')" width="100" align="center" :excel-formatter="(row) => row.approvedWeight ? row.approvedWeight + 'g' : '-'">
          <template #default="scope">
            <span style="color: #67C23A;">{{ scope.row.approvedWeight ? scope.row.approvedWeight + 'g' : '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column :label="$t('admin.orderApproval.headers.approvedMemo')" width="150" align="center" :excel-formatter="(row) => row.approvedMemo || '-'">
          <template #default="scope">
            <span style="font-size: 0.95rem; color: #606266;">{{ scope.row.approvedMemo || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column :label="$t('admin.inspectionRequest.headers.factoryRequest')" width="180" align="center" :excel-formatter="(row) => row.requestedWeight + ' / ' + (row.purity || '')">
          <template #default="scope">
            <div style="display: flex; flex-direction: column; gap: 0.3125rem;">
              <el-input-number
                v-model="scope.row.requestedWeight"
                :precision="2"
                :step="0.1"
                :min="0"
                style="width: 130px"
              />
              <el-select v-model="scope.row.purity" size="small" style="width: 130px" :placeholder="$t('stockDetail.purity')">
                <el-option label="14K" value="14K" />
                <el-option label="18K" value="18K" />
                <el-option label="24K" value="24K" />
                <el-option label="PT950" value="PT950" />
                <el-option label="PT900" value="PT900" />
              </el-select>
            </div>
          </template>
        </el-table-column>
        <el-table-column :label="$t('admin.inspectionRequest.placeholders.memo')" min-width="200" prop="inspectionMemo">
          <template #default="scope">
            <el-input v-model="scope.row.inspectionMemo" :placeholder="$t('admin.inspectionRequest.placeholders.memo')" clearable />
          </template>
        </el-table-column>
      </base-table>

      <el-form-item :label="$t('admin.orderApproval.additionalMessage')">
        <el-input
          v-model="factoryForm.factoryRemarks"
          type="textarea"
          :rows="3"
          :placeholder="$t('admin.orderApproval.msgPlaceholder')"
        />
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="visible = false">{{ $t('common.cancel') }}</el-button>
        <el-button type="primary" :loading="submitting" @click="handleFactorySubmit">
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

const visible = ref(false);
const submitting = ref(false);
const defaultImage = 'https://via.placeholder.com/100x100?text=No+Image';

const factoryForm = reactive({
  items: [] as any[],
  factoryRemarks: ''
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

    if (item.exhaustedStockId) return;

    items.push({
      orderItemId: item.id,
      productName: item.productName,
      productSetTitle: item.productSetTitle,
      productNo: item.productNo,
      photoUrl: item.photoUrl,
      quantity: item.quantity,
      approvedWeight: item.approvedWeight || 0,
      approvedMemo: item.approvedMemo || '',
      actualWeight: item.actualWeight || item.approvedWeight || 0,
      requestedWeight: item.requestedWeight || 0,
      purity: item.purity || (item.productSetId ? '' : '14K'),
      isSet: !!item.productSetId,
      requestedMemo: item.requestedMemo || '',
      inspectionMemo: item.inspectionMemo || '', 
      depth: 0
    });

    if (item.children && item.children.length > 0) {
      item.children.forEach((child: any) => {

        if (child.exhaustedStockId) return;

        items.push({
          orderItemId: child.id,
          productName: child.productName,
          productSetTitle: child.productSetTitle,
          productNo: child.productNo,
          photoUrl: child.photoUrl,
          quantity: child.quantity,
          approvedWeight: child.approvedWeight || 0,
          approvedMemo: child.approvedMemo || '',
          actualWeight: child.actualWeight || child.approvedWeight || 0,
          requestedWeight: child.requestedWeight || 0,
          purity: child.purity || '14K',
          isChild: true,
          requestedMemo: child.requestedMemo || '',
          inspectionMemo: child.inspectionMemo || '',
          depth: 1
        });
      });
    }
  });

  factoryForm.items = items;
  factoryForm.factoryRemarks = props.order.factoryRemarks || '';
};

const handleClose = () => {
  visible.value = false;
  factoryForm.items = [];
  factoryForm.factoryRemarks = '';
};

const tableRowClassName = ({ row }: { row: any }) => {
  if (row.isChild) {
    return 'child-row';
  }
  return '';
};

const handleFactorySubmit = async () => {
  if (!props.order) return;

  submitting.value = true;
  try {
    const data = {
      status: 'FactoryRequested',
      factoryRemarks: factoryForm.factoryRemarks,
      itemWeights: factoryForm.items.map(item => ({
        orderItemId: item.orderItemId,

        requestedWeight: item.requestedWeight,
        purity: item.purity,
        requestedMemo: item.inspectionMemo 
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

<style scoped>
.product-info-cell {
  display: flex;
  align-items: center;
}
.product-thumb {
  border-radius: 2px;
}
.product-text {
  display: flex;
  flex-direction: column;
}
.product-name {
  font-weight: bold;
}
.product-no {
  color: #909399;
}
:deep(.child-row) {
  background-color: #fafafa;
}
</style>

