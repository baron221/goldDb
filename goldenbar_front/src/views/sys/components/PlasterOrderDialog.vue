<template>
<base-popup
    draggable
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    :title="dialogStatus === 'create' ? '새 석고 발주 등록' : '발주 정보 수정'"
    class="responsive-dialog-600"
  >
    <el-form ref="dataForm" :model="temp" label-position="left" label-width="120px" style="padding: 0 1.25rem;">
      <el-form-item label="발주사" prop="orderingCompanyId">
        <common-select v-model="temp.orderingCompanyId" group-code="COMPANY_TYPE" placeholder="발주사 선택" />
      </el-form-item>
      <el-form-item label="발주 제조사" prop="manufacturerId">
        <common-select v-model="temp.manufacturerId" group-code="COMPANY_TYPE" placeholder="제조사 선택" />
      </el-form-item>
      <el-form-item label="수량" prop="quantity">
        <el-input-number v-model="temp.quantity" :min="1" style="width: 100%;" />
      </el-form-item>
      <el-form-item label="상태" prop="status">
        <el-select v-model="temp.status" style="width: 100%;">
          <el-option label="발주" value="발주" />
          <el-option label="진행중" value="진행중" />
          <el-option label="완료" value="완료" />
        </el-select>
      </el-form-item>
      <el-form-item label="발주일자" prop="orderDate">
        <el-date-picker v-model="temp.orderDate" type="datetime" style="width: 100%;" />
      </el-form-item>
      <el-form-item label="취소 여부" prop="isCancelled">
        <el-switch v-model="temp.isCancelled" active-text="취소됨" inactive-text="정상" />
      </el-form-item>
      <el-form-item label="비고" prop="remarks">
        <el-input v-model="temp.remarks" type="textarea" :rows="3" />
      </el-form-item>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="$emit('update:modelValue', false)">취소</el-button>
        <el-button type="primary" @click="submitForm">저장</el-button>
      </div>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { reactive, watch, ref } from 'vue';
import { createPlasterOrder, updatePlasterOrder } from '@/api/plaster-order';
import { ElMessage } from 'element-plus';
import BasePopup from '@/components/BasePopup/index.vue';
import CommonSelect from '@/components/CommonSelect/index.vue';

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  dialogStatus: {
    type: String,
    default: 'create' 
  },
  initialData: {
    type: Object,
    default: () => ({})
  }
});

const emit = defineEmits(['update:modelValue', 'success']);

const dataForm = ref();
const temp = reactive({
  id: undefined,
  orderingCompanyId: undefined as number | undefined,
  manufacturerId: undefined as number | undefined,
  quantity: 1,
  status: '발주',
  orderDate: new Date(),
  isCancelled: false,
  remarks: ''
});

watch(() => props.modelValue, (val) => {
  if (val) {
    if (props.dialogStatus === 'update' && props.initialData) {
      Object.assign(temp, props.initialData);
    } else {
      resetTemp();
    }
  }
});

const resetTemp = () => {
  Object.assign(temp, {
    id: undefined,
    orderingCompanyId: undefined,
    manufacturerId: undefined,
    quantity: 1,
    status: '발주',
    orderDate: new Date(),
    isCancelled: false,
    remarks: ''
  });
};

const submitForm = async () => {
  try {
    if (props.dialogStatus === 'create') {
      await createPlasterOrder(temp);
      ElMessage.success('등록되었습니다.');
    } else {
      await updatePlasterOrder(temp.id!, temp);
      ElMessage.success('수정되었습니다.');
    }
    emit('success');
    emit('update:modelValue', false);
  } catch (error) {
    console.error(error);
    ElMessage.error('저장 실패');
  }
};
</script>

