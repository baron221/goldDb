<template>
<base-popup
    draggable
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    :title="isEdit ? '다이아몬드 시세 수정' : '새 다이아몬드 시세 등록'"
  >
    <el-form ref="dataForm" :model="temp" label-position="left" label-width="120px" style="width: 400px; margin-left: 3.125rem;">
      <el-form-item label="가격 구분" prop="priceType">
        <el-select v-model="temp.priceType" placeholder="구분 선택" style="width: 100%;">
          <el-option label="시세가" value="MARKET" />
          <el-option label="매입가" value="PURCHASE" />
          <el-option label="판매가" value="SALE" />
        </el-select>
      </el-form-item>
      <el-form-item label="사이즈" prop="size">
        <el-select v-model="temp.size" placeholder="사이즈 선택" style="width: 100%;">
          <el-option v-for="i in 9" :key="i" :label="i + '부'" :value="i + '부'" />
        </el-select>
      </el-form-item>
      <el-form-item label="고시 시간" prop="announcedAt">
        <el-date-picker v-model="temp.announcedAt" type="datetime" placeholder="고시 시간 선택" style="width: 100%;" />
      </el-form-item>
      <el-form-item label="버진(FVVS1)" prop="virginPrice">
        <el-input-number v-model="temp.virginPrice" :min="0" style="width: 100%;" />
      </el-form-item>
      <el-form-item label="우신(GVVS1)" prop="wooshinPrice">
        <el-input-number v-model="temp.wooshinPrice" :min="0" style="width: 100%;" />
      </el-form-item>
      <el-form-item label="현대(GVVS1)" prop="hyundaiPrice">
        <el-input-number v-model="temp.hyundaiPrice" :min="0" style="width: 100%;" />
      </el-form-item>
      <el-form-item label="국제(GVVS1)" prop="gukjePrice">
        <el-input-number v-model="temp.gukjePrice" :min="0" style="width: 100%;" />
      </el-form-item>
      <el-form-item label="국보(GVVS1)" prop="gukboPrice">
        <el-input-number v-model="temp.gukboPrice" :min="0" style="width: 100%;" />
      </el-form-item>
      <el-form-item label="기타(GVVS1)" prop="otherPrice">
        <el-input-number v-model="temp.otherPrice" :min="0" style="width: 100%;" />
      </el-form-item>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="$emit('update:modelValue', false)">
          취소
        </el-button>
        <el-button type="primary" @click="submitForm">
          {{ isEdit ? '저장' : '등록' }}
        </el-button>
      </div>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { reactive, watch, ref } from 'vue';
import { createDiamondPrice, updateDiamondPrice } from '@/api/diamond-price';
import { ElMessage } from 'element-plus';
import BasePopup from '@/components/BasePopup/index.vue';

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  isEdit: {
    type: Boolean,
    default: false
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
  priceType: 'MARKET',
  size: '',
  announcedAt: new Date(),
  virginPrice: 0,
  wooshinPrice: 0,
  hyundaiPrice: 0,
  gukjePrice: 0,
  gukboPrice: 0,
  otherPrice: 0
});

watch(() => props.modelValue, (val) => {
  if (val) {
    if (props.isEdit && props.initialData) {
      Object.assign(temp, props.initialData);
    } else if (Object.keys(props.initialData).length > 0 && !props.initialData.id) {

      Object.assign(temp, props.initialData);
    } else {
      resetTemp();
    }
  }
});

const resetTemp = () => {
  Object.assign(temp, {
    id: undefined,
    priceType: 'MARKET',
    size: '',
    announcedAt: new Date(),
    virginPrice: 0,
    wooshinPrice: 0,
    hyundaiPrice: 0,
    gukjePrice: 0,
    gukboPrice: 0,
    otherPrice: 0
  });
};

const submitForm = async () => {
  try {
    if (props.isEdit && temp.id) {
      await updateDiamondPrice(temp.id, temp);
      ElMessage.success('수정되었습니다.');
    } else {
      await createDiamondPrice(temp);
      ElMessage.success('등록되었습니다.');
    }
    emit('success');
    emit('update:modelValue', false);
  } catch (error) {
    console.error(error);
    ElMessage.error('저장 실패');
  }
};
</script>

