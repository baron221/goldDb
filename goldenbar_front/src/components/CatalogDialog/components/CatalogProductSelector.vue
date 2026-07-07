<template>
<base-popup
    draggable
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    title="연결할 제품 선택"
    width="800px"
  >
    <div class="filter-container">
      <el-input
        v-model="localProductQuery.name"
        placeholder="제품명 검색"
        style="width: 200px;"
        @keyup.enter="$emit('get-product-list')"
      />
      <el-button type="primary" style="margin-left: 0.625rem;" @click="$emit('get-product-list')">검색</el-button>
    </div>
    <base-table
      v-loading="productLoading"
      :data="productList"
      style="width: 100%; margin-top: 0.625rem;"
      :total="productTotal"
      v-model:page="localProductQuery.page"
      v-model:page-size="localProductQuery.pageSize"
      @change="$emit('get-product-list')"
      @selection-change="handleProductSelectionChange"
    >
      <el-table-column type="selection" width="55" />
      <el-table-column prop="productNo" label="제품번호" width="120" />
      <el-table-column prop="name" label="제품명" />
    </base-table>
    <template #footer>
      <el-button @click="$emit('update:modelValue', false)">취소</el-button>
      <el-button type="primary" @click="confirmAdd">선택 추가</el-button>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import BasePopup from '@/components/BasePopup/index.vue';
import BaseTable from '@/components/BaseTable/index.vue';

const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true
  },
  productLoading: {
    type: Boolean,
    required: true
  },
  productList: {
    type: Array,
    required: true
  },
  productTotal: {
    type: Number,
    required: true
  },
  productQuery: {
    type: Object,
    required: true
  }
});

const emit = defineEmits([
  'update:modelValue',
  'update:productQuery',
  'get-product-list',
  'add-selected-products'
]);

const localProductQuery = ref({ ...props.productQuery });

watch(() => props.productQuery, (val) => {
  localProductQuery.value = { ...val };
}, { deep: true });

watch(localProductQuery, (val) => {
  emit('update:productQuery', val);
}, { deep: true });

const selectedProducts = ref<any[]>([]);

const handleProductSelectionChange = (val: any[]) => {
  selectedProducts.value = val;
};

const confirmAdd = () => {
  emit('add-selected-products', selectedProducts.value);
};
</script>

<style scoped>
.filter-container {
  padding-bottom: 0.625rem;
}
</style>

