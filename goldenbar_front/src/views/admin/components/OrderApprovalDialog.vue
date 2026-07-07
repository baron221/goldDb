<template>
<base-popup v-model="visible" :title="$t('admin.orderApproval.title')" width="90%" style="max-width: 1000px;" @close="handleClose">
    <el-form :model="approvalForm" label-position="top">
      <el-form-item :label="$t('admin.orderApproval.logisticsCompany')" required>
        <company-select v-model="approvalForm.logisticsCompanyId" category="DCC" :placeholder="$t('admin.orderApproval.placeholder')" style="width: 300px" />
      </el-form-item>
      <base-table :data="approvalForm.items" border style="width: 100%; margin-bottom: 1.25rem;" :row-class-name="tableRowClassName">
        <el-table-column
          :label="$t('admin.orderApproval.headers.productInfo')"
          min-width="250"
          :excel-formatter="(row: any) => (row.productName || row.productSetTitle) + ' (' + row.productNo + ')'"
        >
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
        <el-table-column :label="$t('admin.orderApproval.headers.qty')" width="70" align="center" prop="quantity" />
        <el-table-column
          :label="$t('admin.orderApproval.headers.baseWeight')"
          width="100"
          align="center"
          :excel-formatter="(row: any) => row.weight ? row.weight + 'g' : '-'"
        >
          <template #default="scope">
            <span style="color: #909399;">{{ scope.row.weight ? scope.row.weight + 'g' : '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column
          :label="$t('admin.orderApproval.headers.approvedWeight')"
          width="150"
          align="center"
          prop="approvedWeight"
          :excel-formatter="(row: any) => row.approvedWeight ? row.approvedWeight + 'g' : '-'"
        >
          <template #default="scope">
            <el-input-number
              v-model="scope.row.approvedWeight"
              :precision="2"
              :step="0.1"
              :min="0"
              style="width: 120px"
            />
          </template>
        </el-table-column>
        <el-table-column :label="$t('admin.orderApproval.headers.approvedMemo')" min-width="250" prop="approvedMemo">
          <template #default="scope">
            <el-input v-model="scope.row.approvedMemo" :placeholder="$t('admin.orderApproval.headers.approvedMemo')" clearable />
          </template>
        </el-table-column>
        <el-table-column
          :label="$t('admin.orderApproval.headers.asOrder')"
          width="80"
          align="center"
          prop="isAsOrder"
          :excel-formatter="(row: any) => row.isAsOrder ? 'Y' : 'N'"
        >
          <template #default="scope">
            <el-checkbox v-model="scope.row.isAsOrder" />
          </template>
        </el-table-column>
      </base-table>

      <el-form-item :label="$t('admin.orderApproval.additionalMessage')">
        <el-input
          v-model="approvalForm.factoryRemarks"
          type="textarea"
          :rows="3"
          :placeholder="$t('admin.orderApproval.msgPlaceholder')"
        />
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="visible = false">{{ $t('common.cancel') }}</el-button>
        <el-button type="primary" :loading="submitting" @click="handleApprovalSubmit">
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
import CompanySelect from '@/components/CompanySelect/index.vue';
import BasePopup from '@/components/BasePopup/index.vue';
import BaseTable from '@/components/BaseTable/index.vue';

const { t } = useI18n();

const props = defineProps({
  modelValue: Boolean,
  order: {
    type: Object,
    default: () => null
  }
});

const emit = defineEmits(['update:modelValue', 'saved']);

const visible = ref(false);
const submitting = ref(false);
const defaultImage = 'https://via.placeholder.com/100x100?text=No+Image';

const approvalForm = reactive({
  items: [] as any[],
  factoryRemarks: '',
  logisticsCompanyId: null as number | null
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
  approvalForm.logisticsCompanyId = props.order.logisticsCompanyId || null;
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
      approvedWeight: item.approvedWeight || item.weight || 0,
      approvedMemo: item.approvedMemo || '',
      isAsOrder: !!item.isAsOrder,
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
          approvedWeight: child.approvedWeight || child.weight || 0,
          approvedMemo: child.approvedMemo || '',
          isAsOrder: !!child.isAsOrder,
          isChild: true,
          depth: 1
        });
      });
    }
  });

  approvalForm.items = items;
  approvalForm.factoryRemarks = props.order.factoryRemarks || '';
};

const handleClose = () => {
  visible.value = false;
  approvalForm.items = [];
  approvalForm.factoryRemarks = '';
  approvalForm.logisticsCompanyId = null;
};

const tableRowClassName = ({ row }: { row: any }) => {
  if (row.isChild) {
    return 'child-row';
  }
  return '';
};

const handleApprovalSubmit = async () => {
  if (!props.order) return;

  if (!approvalForm.logisticsCompanyId) {
    ElMessage.error(t('admin.orderApproval.messages.selectCompany'));
    return;
  }

  submitting.value = true;
  try {
    const data = {
      status: 'LogisticsApproved',
      factoryRemarks: approvalForm.factoryRemarks,
      logisticsCompanyId: approvalForm.logisticsCompanyId,
      itemWeights: approvalForm.items.map(item => ({
        orderItemId: item.orderItemId,
        approvedWeight: item.approvedWeight,
        approvedMemo: item.approvedMemo,
        isAsOrder: !!item.isAsOrder
      }))
    };

    await updateOrderStatus(props.order.id, data);
    ElMessage.success(t('admin.orderApproval.messages.success'));
    visible.value = false;
    emit('saved');
  } catch (error) {
    console.error('Failed to update status:', error);
    ElMessage.error(t('admin.orderApproval.messages.error'));
  } finally {
    submitting.value = false;
  }
};
</script>

<style scoped>
.product-info-cell {
  display: flex;
  align-items: center;
  gap: 0.9375rem;
}
.product-thumb {
  width: 60px;
  height: 60px;
  border-radius: 2px;
  border: 1px solid #ebeef5;
}
.product-text {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}
.product-name {
  font-weight: bold;
  color: #303133;
}
.product-no {
  font-size: 0.95rem;
  color: #909399;
}
:deep(.child-row) {
  background-color: #fafafa;
}
</style>

