<template>
<div>
    <div style="margin-bottom: 0.625rem;">
      <el-button type="primary" size="small" @click="openProductSelector">제품 추가</el-button>
    </div>
    <base-table :data="localProducts" style="width: 100%" :show-footer="false">
      <el-table-column label="사진" width="80" align="center">
        <template #default="{row}">
          <el-image
            v-if="getProductMainPhoto(row)"
            :src="getProductMainPhoto(row)"
            fit="cover"
            style="width: 60px; height: 60px; border-radius: 4px; display: block; margin: 0 auto;"
            :preview-src-list="[getProductMainPhoto(row)]"
            preview-teleported
          />
          <span v-else>-</span>
        </template>
      </el-table-column>
      <el-table-column label="제품정보" min-width="180">
        <template #default="{ row }">
          <div style="font-weight: bold; color: #409EFF; font-size: 0.9rem;">{{ row.productNo }}</div>
          <div style="font-size: 0.85rem; color: #606266; margin-top: 2px;">{{ row.name }}</div>
        </template>
      </el-table-column>

      <el-table-column label="속성" width="180">
        <template #default="{ row }">
          <div style="margin-bottom: 4px;">
            <template v-if="row.purity">
              <el-tag
                v-for="pCode in row.purity.split(',')"
                :key="pCode"
                size="small"
                type="warning"
                style="margin: 0 2px 2px 0;"
              >
                {{ allCodesMap[pCode.trim()] || pCode.trim() }}
              </el-tag>
            </template>
            <template v-if="row.colors && row.colors.toUpperCase() !== 'EMPTY'">
              <el-tag
                v-for="code in row.colors.split(',').filter(c => c.trim().toUpperCase() !== 'EMPTY')"
                :key="code"
                size="small"
                style="margin: 0 2px 2px 0;"
              >
                {{ allCodesMap[code.trim()] || code.trim() }}
              </el-tag>
            </template>
          </div>
          <div style="font-size: 0.85rem; color: #909399;">
            <i class="fas fa-weight-hanging" style="font-size: 0.75rem; margin-right: 4px;"></i>
            {{ row.weight ? row.weight + 'g' : '-' }}
          </div>
        </template>
      </el-table-column>

      <el-table-column label="가격" width="130" align="right">
        <template #default="{ row }">
          <div style="font-size: 0.9rem; color: #E6A23C; font-weight: 500;">
            <span style="font-size: 0.75rem; color: #909399; margin-right: 4px;">공장</span>
            {{ formatPrice(row.factoryPrice) }}
          </div>
          <div style="font-size: 0.9rem; color: #67C23A; font-weight: 500; margin-top: 2px;">
            <span style="font-size: 0.75rem; color: #909399; margin-right: 4px;">수공</span>
            {{ formatPrice(row.laborCost) }}
          </div>
        </template>
      </el-table-column>

      <el-table-column label="관리" width="60" align="center">
        <template #default="scope">
          <el-button link class="delete-icon-btn" :icon="Delete" @click="removeProduct(scope.$index)" />
        </template>
      </el-table-column>
    </base-table>

    <product-selector-dialog
      v-model="productSelectorVisible"
      @select="addSelectedProducts"
    />
  </div>
</template>

<script setup lang="ts">

import { ref, computed, watch } from 'vue';
import { Delete } from '@element-plus/icons-vue';
import { formatPrice } from '@/utils/format';
import useCodeStore from '@/store/modules/code';
import BaseTable from '@/components/BaseTable/index.vue';
import ProductSelectorDialog from './ProductSelectorDialog.vue';

const props = defineProps<{
  connectedProducts: any[];
}>();

const emit = defineEmits(['update:connectedProducts', 'products-added']);

const localProducts = ref([...props.connectedProducts]);

watch(() => props.connectedProducts, (val) => {
  localProducts.value = [...val];
}, { deep: true });

watch(localProducts, (val) => {
  emit('update:connectedProducts', val);
}, { deep: true });

const codeStore = useCodeStore();
const allCodesMap = computed(() => codeStore.codeMap);

const productSelectorVisible = ref(false);

const getProductMainPhoto = (product: any) => {
  if (product.photos && product.photos.length > 0) {
    const mainPhoto = product.photos.find((p: any) => p.isMain) || product.photos[0];
    return mainPhoto.photoUrl;
  }
  return null;
};

const openProductSelector = () => {
  productSelectorVisible.value = true;
};

const removeProduct = (index: number) => {
  localProducts.value.splice(index, 1);
};

const addSelectedProducts = (productsList: any[]) => {
  productsList.forEach((p: any) => {
    if (!localProducts.value.find((cp: any) => cp.id === p.id)) {
      localProducts.value.push({
        id: p.id,
        name: p.name,
        productNo: p.productNo,
        purity: p.purity,
        colors: p.colors,
        weight: p.weight,
        factoryPrice: p.factoryPrice,
        laborCost: p.laborCost,
        photos: p.photos || []
      });
    }
  });
  emit('products-added');
};
</script>

