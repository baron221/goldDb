<template>
<div class="filter-container">
    <el-form :inline="!isMobile" :label-position="isMobile ? 'top' : 'right'" :model="localQuery">
      <el-form-item label="직영 소매점">
        <el-select
          v-model="localQuery.companyId"
          placeholder="소매점 선택"
          clearable
          style="width: 200px;"
          @change="handleFilter"
        >
          <el-option
            v-for="item in directRetailers"
            :key="item.id"
            :label="item.name"
            :value="item.id"
          />
        </el-select>
      </el-form-item>

      <el-form-item label="일반분류">
        <div class="flex flex-wrap gap-2">
          <common-select
            v-model="localQuery.categoryLarge"
            group-code="PRODUCT_CATEGORY"
            placeholder="대분류"
            style="width: 120px;"
            @change="(val, options) => handleLargeChange(val, options)"
          />
          <common-select
            v-model="localQuery.categoryMedium"
            :parent-id="largeId"
            placeholder="중분류"
            style="width: 120px;"
            @change="(val, options) => handleMediumChange(val, options)"
          />
          <common-select
            v-model="localQuery.categorySmall"
            :parent-id="mediumId"
            placeholder="소분류"
            style="width: 120px;"
            @change="handleFilter"
          />
        </div>
      </el-form-item>

      <el-form-item label="세트분류">
        <div class="flex flex-wrap gap-2">
          <common-select
            v-model="localQuery.setCategoryLarge"
            group-code="PRODUCT_CATEGORY"
            placeholder="대분류"
            style="width: 120px;"
            @change="(val, options) => handleSetLargeChange(val, options)"
          />
          <common-select
            v-model="localQuery.setCategoryMedium"
            :parent-id="setLargeId"
            placeholder="중분류"
            style="width: 120px;"
            @change="(val, options) => handleSetMediumChange(val, options)"
          />
          <common-select
            v-model="localQuery.setCategorySmall"
            :parent-id="setMediumId"
            placeholder="소분류"
            style="width: 120px;"
            @change="handleFilter"
          />
        </div>
      </el-form-item>

      <el-form-item label="주문번호">
        <el-input v-model="localQuery.orderNo" placeholder="주문번호" clearable @keyup.enter="handleFilter" style="width: 150px;" />
      </el-form-item>
      <el-form-item label="재고번호">
        <el-input v-model="localQuery.stockNo" placeholder="재고번호" clearable @keyup.enter="handleFilter" style="width: 150px;" />
      </el-form-item>
      <el-form-item label="상품명">
        <el-input v-model="localQuery.productName" placeholder="상품명" clearable @keyup.enter="handleFilter" style="width: 150px;" />
      </el-form-item>
      <el-form-item label="중량 범위">
        <div class="flex items-center gap-2">
          <el-input-number v-model="localQuery.minWeight" placeholder="최소" :precision="2" style="width: 100px;" />
          <span>~</span>
          <el-input-number v-model="localQuery.maxWeight" placeholder="최대" :precision="2" style="width: 100px;" />
        </div>
      </el-form-item>
      <el-form-item label="상태">
        <common-select
          v-model="localQuery.status"
          group-code="INVENTORY_STATUS"
          placeholder="상태 선택"
          show-all
          style="width: 140px;"
          @change="handleFilter"
        />
      </el-form-item>

      <el-form-item label="소진 여부">
        <el-switch
          v-model="localQuery.isExhausted"
          active-text="소진됨"
          inactive-text="보유중"
          :active-value="true"
          :inactive-value="false"
          @change="handleFilter"
        />
      </el-form-item>

      <el-form-item>
        <el-button type="primary" @click="handleFilter">조회</el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useMobile } from '@/hooks/useMobile';
import CommonSelect from '@/components/CommonSelect/index.vue';

const props = defineProps<{
  modelValue: any;
  directRetailers: any[];
}>();

const emit = defineEmits<{(e: 'update:modelValue', value: any): void;
  (e: 'filter'): void;
}>();

const { isMobile } = useMobile();

const localQuery = ref({ ...props.modelValue });

watch(() => props.modelValue, (newVal) => {
  localQuery.value = { ...newVal };
}, { deep: true });

watch(localQuery, (newVal) => {
  emit('update:modelValue', newVal);
}, { deep: true });

const largeId = ref<number | null>(null);
const mediumId = ref<number | null>(null);
const setLargeId = ref<number | null>(null);
const setMediumId = ref<number | null>(null);

const handleLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  largeId.value = selected ? selected.id : null;
  localQuery.value.categoryMedium = undefined;
  localQuery.value.categorySmall = undefined;
  mediumId.value = null;
  handleFilter();
};

const handleMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  mediumId.value = selected ? selected.id : null;
  localQuery.value.categorySmall = undefined;
  handleFilter();
};

const handleSetLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  setLargeId.value = selected ? selected.id : null;
  localQuery.value.setCategoryMedium = undefined;
  localQuery.value.setCategorySmall = undefined;
  setMediumId.value = null;
  handleFilter();
};

const handleSetMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  setMediumId.value = selected ? selected.id : null;
  localQuery.value.setCategorySmall = undefined;
  handleFilter();
};

const handleFilter = () => {
  emit('filter');
};
</script>

