<template>
<base-popup
    v-model="visible"
    title="공장 의뢰"
    width="90%"
    style="max-width: 1100px;"
    @close="handleClosed"
  >
    <el-form :model="requestForm" label-position="top">
      <base-table :data="requestForm.items" border style="width: 100%; margin-bottom: 1.25rem;" :row-class-name="tableRowClassName">
        <el-table-column label="사진" width="60" align="center" header-align="center" class-name="photo-column">
          <template #default="scope">
            <el-image :src="scope.row.photoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 100%; min-height: 80px; display: block;" />
          </template>
        </el-table-column>
        <el-table-column label="상품 정보" min-width="250" header-align="center" :excel-formatter="scope => `${scope.manufacturerName || '-'} (${scope.productNo || '-'}) ${scope.productName || scope.productSetTitle}`">
          <template #default="scope">
            <div class="product-info-cell" :style="{ paddingLeft: scope.row.depth * 20 + 'px' }">
              <el-icon v-if="scope.row.isChild" style="margin-right: 0.3125rem; color: #909399;"><BottomLeft /></el-icon>
              <div class="product-text">

                <div style="display: flex; align-items: center; gap: 6px; flex-wrap: wrap;">
                  <span style="font-size: 0.95rem; color: #E6A23C; font-weight: bold;">{{ scope.row.manufacturerName || '-' }}</span>
                  <span style="font-size: 0.85rem; color: #909399;">({{ scope.row.productNo || '-' }})</span>
                </div>

                <div style="font-size: 0.9rem; display: flex; align-items: center; gap: 5px; flex-wrap: wrap; margin-top: 2px;">
                  <el-tag v-if="scope.row.isSet" size="small" type="warning" style="margin-right: 0.125rem;">SET</el-tag>
                  <span class="product-name" style="font-weight: bold;">{{ scope.row.productName || scope.row.productSetTitle }}</span>
                  <el-tag size="small" type="info" effect="plain" v-if="scope.row.purity && scope.row.purity.toUpperCase() !== 'EMPTY'" style="font-size: 0.825rem !important; height: 16px !important; line-height: 14px !important; padding: 0 0.25rem !important;">{{ codeMap[scope.row.purity] || scope.row.purity }}</el-tag>
                  <el-tag size="small" type="info" effect="plain" v-if="scope.row.color && scope.row.color.toUpperCase() !== 'EMPTY'" style="font-size: 0.825rem !important; height: 16px !important; line-height: 14px !important; padding: 0 0.25rem !important;">{{ codeMap[scope.row.color] || scope.row.color }}</el-tag>
                </div>

                <div v-if="scope.row.categoryLarge" style="font-size: 0.75rem; color: #909399; margin-top: 4px;">
                  <span>{{ codeMap[scope.row.categoryLarge] || scope.row.categoryLarge }}</span>
                  <span v-if="scope.row.categoryMedium"> &gt; {{ codeMap[scope.row.categoryMedium] || scope.row.categoryMedium }}</span>
                  <span v-if="scope.row.categorySmall"> &gt; {{ codeMap[scope.row.categorySmall] || scope.row.categorySmall }}</span>
                </div>
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="수량" width="70" align="center" prop="quantity" header-align="center" />

        <el-table-column label="AS여부" width="80" align="center" header-align="center" :excel-formatter="scope => scope.isAsOrder ? 'Y' : 'N'">
          <template #default="scope">
            <el-icon v-if="scope.row.isAsOrder" :size="24" color="#F56C6C" style="font-weight: bold;">
              <Check />
            </el-icon>
            <span v-else style="color: #E4E7ED;">-</span>
          </template>
        </el-table-column>

        <el-table-column label="승인 중량" width="90" align="center" header-align="center" :excel-formatter="scope => scope.approvedWeight ? scope.approvedWeight + 'g' : '-'">
          <template #default="scope">
            <span style="font-weight: bold; color: #409EFF;">{{ scope.row.approvedWeight ? scope.row.approvedWeight + 'g' : '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="공장 의뢰"  align="center" header-align="center" width="200" :excel-formatter="scope => `중량: ${scope.requestedWeight || 0}g, 메모: ${scope.requestedMemo || '-'}`">
          <template #default="scope">
            <div class="request-item">
              <label>중량:</label>
              <el-input-number
                v-model="scope.row.requestedWeight"
                :precision="2"
                :step="0.1"
                :min="0"
                style="width: 120px"
              />
            </div>
            <div class="request-item">
              <label>메모:</label>
              <el-input v-model="scope.row.requestedMemo" placeholder="공장 의뢰 메모" clearable style="width: 120px" />
            </div>
          </template>
        </el-table-column>

      </base-table>

      <el-row :gutter="20">

        <el-col :span="12">
        </el-col>

        <el-col :span="12">
          <el-form-item label="납기일 희망일">
            <el-date-picker
              v-model="requestForm.deliveryDate"
              type="date"
              placeholder="납기일을 선택하세요"
              style="width: 100%"
              format="YYYY-MM-DD"
              value-format="YYYY-MM-DD"
            />
          </el-form-item>
          <el-form-item label="공장 전달 추가 메시지">
            <el-input
              v-model="requestForm.factoryRemarks"
              type="textarea"
              :rows="3"
              placeholder="공장에 전달할 전체 메시지를 입력하세요"
            />
          </el-form-item>
        </el-col>
      </el-row>

    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="visible = false">취소</el-button>
        <el-button type="primary" :loading="submitLoading" @click="handleSubmit">
          의뢰 완료
        </el-button>
      </span>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue';
import { BottomLeft, Check } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import { updateOrderStatus } from '@/api/order';
import BasePopup from '@/components/BasePopup/index.vue';
import BaseTable from '@/components/BaseTable/index.vue';

const props = defineProps<{
  modelValue: boolean;
  order: any;
  codeMap: Record<string, string>;
}>();

const emit = defineEmits(['update:modelValue', 'completed']);

const visible = ref(false);
const submitLoading = ref(false);
const defaultImage = 'https://via.placeholder.com/100x100?text=No+Image';

const requestForm = reactive({
  items: [] as any[],
  factoryRemarks: '',
  deliveryDate: ''
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
      approvedWeight: item.approvedWeight || 0,
      approvedMemo: item.approvedMemo || '',

      requestedWeight: item.requestedWeight || item.approvedWeight || 0,
      purity: item.purity || (item.productSetId ? '' : '14K'),
      color: item.color,
      isAsOrder: !!item.isAsOrder,
      requestedMemo: item.requestedMemo || '',
      isSet: !!item.productSetId,
      manufacturerName: item.manufacturerName,
      categoryLarge: item.categoryLarge,
      categoryMedium: item.categoryMedium,
      categorySmall: item.categorySmall,
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
          approvedWeight: child.approvedWeight || 0,
          approvedMemo: child.approvedMemo || '',
          requestedWeight: child.requestedWeight || child.approvedWeight || 0,
          purity: child.purity || '14K',
          color: child.color,
          isAsOrder: !!child.isAsOrder,
          requestedMemo: child.requestedMemo || '',
          manufacturerName: child.manufacturerName,
          categoryLarge: child.categoryLarge,
          categoryMedium: child.categoryMedium,
          categorySmall: child.categorySmall,
          isChild: true, 
          depth: 1 
        });
      });
    }
  });

  requestForm.items = items;
  requestForm.factoryRemarks = props.order.factoryRemarks || '';

  if (props.order.deliveryDate) {
    requestForm.deliveryDate = props.order.deliveryDate.substring(0, 10);
  } else {
    const date = new Date();
    date.setDate(date.getDate() + 5);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    requestForm.deliveryDate = `${year}-${month}-${day}`;
  }
};

const handleClosed = () => {
  requestForm.items = [];
  requestForm.factoryRemarks = '';
  requestForm.deliveryDate = '';
};

const handleSubmit = async () => {
  if (!props.order) return;

  submitLoading.value = true;
  try {
    const data = {
      status: 'FactoryRequested', 
      factoryRemarks: requestForm.factoryRemarks,
      deliveryDate: requestForm.deliveryDate,

      itemWeights: requestForm.items.map(item => ({
        orderItemId: item.orderItemId,
        requestedWeight: item.requestedWeight,
        purity: item.purity,
        requestedMemo: item.requestedMemo
      }))
    };

    await updateOrderStatus(props.order.id, data);
    ElMessage.success('공장 의뢰 처리가 완료되었습니다.');
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

<style scoped>
.request-item {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.375rem;
  margin-bottom: 0.375rem;
}
.request-item:last-child {
  margin-bottom: 0;
}
.request-item label {
  width: 35px;
  text-align: right;
  font-size: 0.95rem;
  color: #606266;
  flex-shrink: 0;
}
.product-info-cell {
  display: flex;
  align-items: center;
}
.product-thumb {
  width: 60px;
  height: 60px;
  border-radius: 0;
  border: none;
  display: block;
}
:deep(.photo-column) {
  padding: 0 !important;
}
:deep(.photo-column .cell) {
  padding: 0 !important;
  line-height: 0 !important;
  display: block !important;
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
.manufacturer-name {
  font-size: 0.8875rem;
  color: #e6a23c;
  margin-bottom: 0.125rem;
}
.product-no {
  font-size: 0.95rem;
  color: #909399;
}
:deep(.child-row) {
  background-color: #fafafa;
}
</style>

