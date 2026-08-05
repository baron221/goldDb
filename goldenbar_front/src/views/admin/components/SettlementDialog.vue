<template>
<base-popup v-model="visible" :title="$t('admin.settlementDialog.title')" width="95%" style="max-width: 1600px;" @close="handleClose">
    <div v-if="order" v-loading="receivableSummaryLoading" style="margin-bottom: 1.25rem;">
      <table class="retailer-summary-table">
        <thead>
          <tr>
            <th></th>
            <th>순금(g)</th>
            <th>공임 및 현금</th>
            <th>금액합계</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td class="summary-label">총 판매</td>
            <td>{{ (receivableSummary?.totalChargeWeight || 0).toFixed(2) }}</td>
            <td>₩ {{ formatPrice(receivableSummary?.totalCharge || 0) }}</td>
            <td>0</td>
          </tr>
          <tr>
            <td class="summary-label">총 결제</td>
            <td>{{ (receivableSummary?.totalDepositWeight || 0).toFixed(2) }}</td>
            <td>₩ {{ formatPrice(receivableSummary?.totalDeposit || 0) }}</td>
            <td>0</td>
          </tr>
          <tr class="summary-total-row">
            <td class="summary-label">미수금</td>
            <td>{{ (receivableSummary?.totalReceivableWeight || 0).toFixed(2) }}</td>
            <td>₩ {{ formatPrice(receivableSummary?.totalReceivable || 0) }}</td>
            <td>0</td>
          </tr>
        </tbody>
      </table>
    </div>

    <el-form :model="settlementForm" label-position="top">
      <base-table :data="settlementForm.items" border style="width: 100%; margin-bottom: 1.25rem;" :row-class-name="tableRowClassName">
        <el-table-column :label="$t('admin.settlementDialog.headers.productInfo')" min-width="200" :fixed="!isMobile ? 'left' : false" prop="productName" :excel-formatter="productInfoFormatter">
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
        <el-table-column :label="$t('admin.settlementDialog.headers.qty')" width="60" align="center" prop="quantity" />
        <el-table-column :label="$t('admin.inspectionRequest.headers.factoryRequest').split(' / ')[0]" width="80" align="center" prop="weight" :excel-formatter="(row) => row.weight ? row.weight + 'g' : '-'">
          <template #default="scope">
            <span style="color: #909399;">{{ scope.row.weight ? scope.row.weight + 'g' : '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column :label="$t('admin.settlementDialog.headers.actualWeight')" width="110" align="center" prop="confirmedWeight" :excel-formatter="(row) => row.confirmedWeight ? row.confirmedWeight + 'g' : '-'">
          <template #default="scope">
            <span style="font-weight: bold; color: #67c23a;">{{ scope.row.confirmedWeight ? scope.row.confirmedWeight + 'g' : '-' }}</span>
          </template>
        </el-table-column>

        <el-table-column :label="$t('admin.settlement.table.ratio') + '(%)'" width="130" align="center" prop="settlementRatio" :excel-formatter="(row) => row.settlementRatio + '%'">
          <template #default="scope">
            <el-input-number
              v-model="scope.row.settlementRatio"
              :precision="0"
              :step="5"
              :min="0"
              :max="100"
              style="width: 100px"
              @change="updateSettlementAmount(scope.row)"
            />
          </template>
        </el-table-column>
        <el-table-column :label="$t('admin.settlement.table.amount')" width="150" align="center" prop="settlementAmount" :excel-formatter="(row) => '₩ ' + formatPrice(row.settlementAmount)">
          <template #default="scope">
            <el-input-number
              v-model="scope.row.settlementAmount"
              :step="1000"
              :min="0"
              style="width: 120px"
            />
          </template>
        </el-table-column>
        <el-table-column :label="$t('admin.settlement.table.memo')" min-width="200" prop="settlementMemo">
          <template #default="scope">
            <el-input v-model="scope.row.settlementMemo" :placeholder="$t('admin.settlement.table.memo')" clearable />
          </template>
        </el-table-column>
      </base-table>

      <div style="text-align: right; margin-bottom: 1.25rem; font-size: 1rem;">
        <strong>{{ $t('cart.orderSummary.totals.estimatedTotal') }} <span style="color: #f56c6c; font-size: 1.25rem;">₩ {{ formatPrice(totalSettlementAmount) }}</span></strong>
      </div>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="visible = false">{{ $t('common.cancel') }}</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSettlementSubmit">
          {{ $t('admin.settlement.table.receivableLink') }}
        </el-button>
      </span>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { useMobile } from '@/hooks/useMobile';
import { ref, reactive, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { updateOrderStatus } from '@/api/order';
import { getUserSummaryById } from '@/api/receivable';
import { ElMessage } from 'element-plus';
import { BottomLeft } from '@element-plus/icons-vue';
import BasePopup from '@/components/BasePopup/index.vue';
import BaseTable from '@/components/BaseTable/index.vue';
import { formatPrice } from '@/utils/format';
const { isMobile } = useMobile();

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

const settlementForm = reactive({
  items: [] as any[]
});

const receivableSummary = ref<any>(null);
const receivableSummaryLoading = ref(false);

const loadReceivableSummary = async () => {
  if (!props.order?.userId) {
    receivableSummary.value = null;
    return;
  }
  receivableSummaryLoading.value = true;
  try {
    const res: any = await getUserSummaryById(props.order.userId);
    receivableSummary.value = res.data;
  } catch (error) {
    console.error('Failed to load receivable summary:', error);
    receivableSummary.value = null;
  } finally {
    receivableSummaryLoading.value = false;
  }
};

const totalSettlementAmount = computed(() => {
  return settlementForm.items.reduce((sum, item) => sum + (item.settlementAmount || 0), 0);
});

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val && props.order) {
    initializeForm();
    loadReceivableSummary();
  }
});

watch(visible, (val) => {
  emit('update:modelValue', val);
});

const initializeForm = () => {
  const items: any[] = [];
  props.order.orderItems.forEach((item: any) => {
    const defaultRatio = item.settlementRatio || 70;
    const amount = item.settlementAmount || 0;

    items.push({
      orderItemId: item.id,
      productName: item.productName,
      productSetTitle: item.productSetTitle,
      productNo: item.productNo,
      photoUrl: item.photoUrl,
      quantity: item.quantity,
      weight: item.weight,
      confirmedWeight: item.confirmedWeight || item.actualWeight || 0,
      settlementRatio: defaultRatio,
      settlementAmount: amount,
      settlementMemo: item.settlementMemo || '',
      isSet: !!item.productSetId,
      depth: 0
    });

    if (item.children && item.children.length > 0) {
      item.children.forEach((child: any) => {
        const childRatio = child.settlementRatio || 70;
        const childAmount = child.settlementAmount || 0;

        items.push({
          orderItemId: child.id,
          productName: child.productName,
          productSetTitle: child.productSetTitle,
          productNo: child.productNo,
          photoUrl: child.photoUrl,
          quantity: child.quantity,
          weight: child.weight,
          confirmedWeight: child.confirmedWeight || child.actualWeight || 0,
          settlementRatio: childRatio,
          settlementAmount: childAmount,
          settlementMemo: child.settlementMemo || '',
          isChild: true,
          depth: 1
        });
      });
    }
  });

  settlementForm.items = items;
};

const handleClose = () => {
  visible.value = false;
  settlementForm.items = [];
  receivableSummary.value = null;
};

const productInfoFormatter = (row: any) => {
  const setTag = row.isSet ? '[SET] ' : '';
  return `${setTag}${row.productName || row.productSetTitle}\n${row.productNo}`;
};

const updateSettlementAmount = (row: any) => {

};

const tableRowClassName = ({ row }: { row: any }) => {
  if (row.isChild) {
    return 'child-row';
  }
  return '';
};

const handleSettlementSubmit = async () => {
  if (!props.order) return;

  submitting.value = true;
  try {
    const data = {
      status: 'SETTLED',
      itemWeights: settlementForm.items.map(item => ({
        orderItemId: item.orderItemId,
        settlementRatio: item.settlementRatio,
        settlementAmount: item.settlementAmount,
        settlementMemo: item.settlementMemo
      }))
    };

    await updateOrderStatus(props.order.id, data);
    ElMessage.success(t('admin.deposit.messages.success'));
    visible.value = false;
    emit('saved');
  } catch (error) {
    console.error('Failed to update status:', error);
    ElMessage.error(t('admin.deposit.messages.error'));
  } finally {
    submitting.value = false;
  }
};
</script>

<style scoped>
.retailer-summary-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}
.retailer-summary-table th,
.retailer-summary-table td {
  border: 1px solid #ebeef5;
  padding: 0.5rem;
  text-align: center;
}
.retailer-summary-table th {
  background: #f5f7fa;
  font-weight: 600;
}
.summary-label {
  text-align: left;
  font-weight: 600;
  background: #fafafa;
  white-space: nowrap;
}
.summary-total-row {
  font-weight: bold;
}
.summary-total-row td:not(.summary-label) {
  color: #f56c6c;
}
.product-info-cell {
  display: flex;
  align-items: center;
  gap: 0.9375rem;
}
.product-thumb {
  border-radius: 2px;
  border: 1px solid #ebeef5;
}
.product-text {
  display: flex;
  flex-direction: column;
  gap: 0.125rem;
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

