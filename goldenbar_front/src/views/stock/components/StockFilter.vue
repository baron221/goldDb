<template>
<div class="filter-container">
    <el-form :inline="true" :model="modelValue">
      <el-form-item label="업체" v-if="isAdmin">
        <company-select
          :model-value="modelValue.companyId"
          @update:model-value="val => updateField('companyId', val)"
          placeholder="업체 선택"
          clearable
          style="width: 200px;"
          @change="handleFilter"
        />
      </el-form-item>
      <el-form-item label="주문번호">
        <el-input
          :model-value="modelValue.orderNo"
          @update:model-value="val => updateField('orderNo', val)"
          placeholder="주문번호"
          clearable
          @keyup.enter="handleFilter"
          style="width: 150px;"
        />
      </el-form-item>
      <el-form-item label="재고번호">
        <el-input
          :model-value="modelValue.stockNo"
          @update:model-value="val => updateField('stockNo', val)"
          placeholder="재고번호"
          clearable
          @keyup.enter="handleFilter"
          style="width: 150px;"
        />
      </el-form-item>
      <el-form-item label="상품명">
        <el-input
          :model-value="modelValue.productName"
          @update:model-value="val => updateField('productName', val)"
          placeholder="상품명"
          clearable
          @keyup.enter="handleFilter"
          style="width: 150px;"
        />
      </el-form-item>
      <el-form-item label="중량 범위">
        <el-input-number
          :model-value="modelValue.minWeight"
          @update:model-value="val => updateField('minWeight', val)"
          placeholder="최소"
          :precision="2"
          :controls="false"
          style="width: 80px;"
          @change="handleFilter"
        />
        <span style="margin: 0 0.3125rem;">~</span>
        <el-input-number
          :model-value="modelValue.maxWeight"
          @update:model-value="val => updateField('maxWeight', val)"
          placeholder="최대"
          :precision="2"
          :controls="false"
          style="width: 80px;"
          @change="handleFilter"
        />
      </el-form-item>
      <el-form-item label="일반분류">
        <common-select
          :model-value="modelValue.categoryLarge"
          @update:model-value="val => updateField('categoryLarge', val)"
          group-code="PRODUCT_CATEGORY"
          placeholder="대분류"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => $emit('large-change', val, options)"
        />
        <common-select
          :model-value="modelValue.categoryMedium"
          @update:model-value="val => updateField('categoryMedium', val)"
          :parent-id="largeId"
          placeholder="중분류"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => $emit('medium-change', val, options)"
        />
        <common-select
          :model-value="modelValue.categorySmall"
          @update:model-value="val => updateField('categorySmall', val)"
          :parent-id="mediumId"
          placeholder="소분류"
          style="width: 120px;"
          @change="handleFilter"
        />
      </el-form-item>

      <el-form-item label="세트분류">
        <common-select
          :model-value="modelValue.setCategoryLarge"
          @update:model-value="val => updateField('setCategoryLarge', val)"
          group-code="PRODUCT_CATEGORY"
          placeholder="대분류"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => $emit('set-large-change', val, options)"
        />
        <common-select
          :model-value="modelValue.setCategoryMedium"
          @update:model-value="val => updateField('setCategoryMedium', val)"
          :parent-id="setLargeId"
          placeholder="중분류"
          style="width: 120px; margin-right: 0.3125rem;"
          @change="(val, options) => $emit('set-medium-change', val, options)"
        />
        <common-select
          :model-value="modelValue.setCategorySmall"
          @update:model-value="val => updateField('setCategorySmall', val)"
          :parent-id="setMediumId"
          placeholder="소분류"
          style="width: 120px;"
          @change="handleFilter"
        />
      </el-form-item>

      <el-form-item label="상태">
        <el-select
          :model-value="modelValue.status"
          @update:model-value="val => updateField('status', val)"
          placeholder="상태 선택"
          clearable
          style="width: 140px;"
          @change="handleFilter"
        >
          <el-option label="가용 재고(전체)" value="ACTIVE" />
          <el-option label="정상재고" value="IN_STOCK" />
          <el-option label="진열중" value="ON_DISPLAY" />
          <el-option label="대여중" value="RENTED" />
          <el-option label="예약" value="RESERVED" />
          <el-option label="보류" value="HOLD" />
          <el-option label="판매" value="SOLD" />
          <el-option label="분실" value="LOST" />
          <el-option label="기타" value="ETC" />
        </el-select>
      </el-form-item>

      <el-form-item>
        <el-button type="primary" @click="handleFilter">조회</el-button>
        <el-button
          type="warning"
          :disabled="selectedGroupsCount === 0"
          @click="$emit('bulk-print')"
        >선택 라벨 일괄인쇄 (인쇄2)</el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<script setup lang="ts">

import CompanySelect from '@/components/CompanySelect/index.vue';
import CommonSelect from '@/components/CommonSelect/index.vue';

const props = defineProps({

  modelValue: {
    type: Object,
    required: true
  },

  isAdmin: {
    type: Boolean,
    default: false
  },

  largeId: {
    type: [Number, null] as any,
    default: null
  },

  mediumId: {
    type: [Number, null] as any,
    default: null
  },

  setLargeId: {
    type: [Number, null] as any,
    default: null
  },

  setMediumId: {
    type: [Number, null] as any,
    default: null
  },

  selectedGroupsCount: {
    type: Number,
    default: 0
  }
});

const emit = defineEmits([
  'update:modelValue',
  'filter',
  'large-change',
  'medium-change',
  'set-large-change',
  'set-medium-change',
  'bulk-print'
]);

const updateField = (field: string, value: any) => {
  emit('update:modelValue', {
    ...props.modelValue,
    [field]: value
  });
};

const handleFilter = () => {
  emit('filter');
};
</script>

<style lang="scss" scoped>
.filter-container {
  padding: 0.9375rem 1.25rem;
  border: 1px solid #f0eeeb;
  border-radius: 0;

  :deep(.el-form-item__label) {
    font-size: 0.8875rem;
    font-weight: 700;
    color: #888;
    letter-spacing: 0.5px;
  }
}

:deep(.el-input__wrapper), :deep(.el-input-number__wrapper), :deep(.el-select__wrapper) {
  border-radius: 0 !important;
  box-shadow: none !important;
  border: 1px solid #dcdfe6;
  &:hover { border-color: #c5a880; }
}
</style>

