<template>
<base-table
    ref="tableRef"
    v-loading="loading"
    :data="data"
    border
    fit
    highlight-current-row
    style="width: 100%;"
    :header-cell-style="{ textAlign: 'center' }"
    @selection-change="$emit('selection-change', $event)"
  >
    <el-table-column type="selection" width="45" align="center" :fixed="!isMobile ? 'left' : false" />
    <el-table-column label="사진" prop="productPhotoUrl" width="80" align="center" header-align="center" class-name="photo-column" :fixed="!isMobile ? 'left' : false">
      <template #default="{row}">
        <div class="photo-wrapper" style="width: 80px; height: 80px; display: flex; align-items: center; justify-content: center; background-color: #faf9f6; overflow: hidden;">
          <el-image
            v-if="(row.attachments && row.attachments.length > 0) || row.productPhotoUrl"
            :src="getThumbnailUrl((row.attachments && row.attachments.find(a => a.isMain)) ? row.attachments.find(a => a.isMain).filePath : (row.attachments && row.attachments.length > 0 ? row.attachments[0].filePath : row.productPhotoUrl))"
            fit="contain"
            style="width: 80px; height: 80px; display: block;"
            :preview-src-list="[(row.attachments && row.attachments.length > 0) ? row.attachments[0].filePath : row.productPhotoUrl]"
            preview-teleported
          />
          <div v-else style="width: 100%; height: 100%; min-height: 80px; display: flex; align-items: center; justify-content: center; font-size: 0.8875rem; color: #909399; font-family: 'S-CoreDream', 'Jost', sans-serif;">NO IMAGE</div>
        </div>
      </template>
    </el-table-column>
    <el-table-column
      label="상품내용"
      prop="productName"
      min-width="250"
      header-align="center"
      :excel-formatter="(row) => `[${row.productNo}] ${row.productName} (${codeMap[row.categoryLarge] || row.categoryLarge}${row.categoryMedium ? ' > ' + (codeMap[row.categoryMedium] || row.categoryMedium) : ''}${row.categorySmall ? ' > ' + (codeMap[row.categorySmall] || row.categorySmall) : ''})`"
    >
      <template #default="{row}">
        <div class="product-info" style="cursor: default;">
          <div class="product-no">{{ row.productNo }}</div>
          <div class="product-name">{{ row.productName }}</div>
          <div class="category">
            {{ codeMap[row.categoryLarge] || row.categoryLarge }}
            <template v-if="row.categoryMedium"> > {{ codeMap[row.categoryMedium] || row.categoryMedium }}</template>
            <template v-if="row.categorySmall"> > {{ codeMap[row.categorySmall] || row.categorySmall }}</template>
          </div>
        </div>
      </template>
    </el-table-column>
    <el-table-column
      label="함량 컬러/실중량"
      prop="totalWeight"
      width="180"
      align="center"
      header-align="center"
      :excel-formatter="(row) => `${codeMap[row.purity] || row.purity} / ${row.color !=='EMPTY' ? (codeMap[row.color] || row.color || '-') : '-'} (${row.totalWeight.toFixed(2)}g)`"
    >
      <template #default="{row}">
        <span>{{ codeMap[row.purity] || row.purity }}</span> / <span v-if="row.color !=='EMPTY'">{{ codeMap[row.color] || row.color || '-' }}</span>
        <br />
        <span>{{ row.totalWeight.toFixed(2) }}g</span>
      </template>
    </el-table-column>
    <el-table-column
      label="소매확인용재료비/수공비"
      prop="materialLaborCostText"
      width="200"
      align="right"
      header-align="center"
      :excel-formatter="(row) => {
        const materialCost = row.items?.[0]?.retailerConfirmMaterialCost;
        const laborCost = row.items?.[0]?.retailerConfirmLaborCost;
        return `재료비: ${materialCost !== undefined && materialCost !== null ? formatPrice(materialCost) : '-'}, 수공비: ${laborCost !== undefined && laborCost !== null ? formatPrice(laborCost) : '-'}`;
      }"
    >
      <template #default="{row}">
        <template v-if="row.items && row.items.length > 0 && row.items[0].retailerConfirmMaterialCost !== undefined && row.items[0].retailerConfirmMaterialCost !== null">
          <span style="font-size: 0.8875rem; color: #909399; margin-right: 0.125rem;">₩</span>
          <span style="font-weight: 600;">{{ formatPrice(row.items[0].retailerConfirmMaterialCost) }}</span>
        </template>
        <span v-else style="color: #999; font-size: 0.95rem;">-</span>

        <br />

        <template v-if="row.items && row.items.length > 0 && row.items[0].retailerConfirmLaborCost !== undefined && row.items[0].retailerConfirmLaborCost !== null">
          <span style="font-size: 0.8875rem; color: #909399; margin-right: 0.125rem;">₩</span>
          <span style="font-weight: 600;">{{ formatPrice(row.items[0].retailerConfirmLaborCost) }}</span>
        </template>
        <span v-else style="color: #999; font-size: 0.95rem;">-</span>

      </template>
    </el-table-column>
    <el-table-column
      label="개수"
      prop="quantity"
      width="100"
      align="center"
      header-align="center"
      :excel-formatter="(row) => `${row.quantity}개`"
    >
      <template #default="{row}">
        <span style="font-weight: 700; color: #c5a880; font-family: 'S-CoreDream', 'Jost', sans-serif;">{{ row.quantity }}개</span>
      </template>
    </el-table-column>
    <el-table-column label="액션" width="150" align="center" :fixed="!isMobile ? 'right' : false" header-align="center">
      <template #default="{row}">
        <el-button
          type="primary"
          size="small"
          style="background-color: #222; border-color: #222; color: #fff; font-family: 'S-CoreDream', 'Jost', sans-serif; font-weight: 500;"
          @click="$emit('open-details', row)"
        >제품별상세보기</el-button>
      </template>
    </el-table-column>
  </base-table>
</template>

<script setup lang="ts">

import { useMobile } from '@/hooks/useMobile';
import { ref } from 'vue';
import BaseTable from '@/components/BaseTable/index.vue';
import { getThumbnailUrl, parseTime } from '@/utils';
const { isMobile } = useMobile();

defineProps({

  data: {
    type: Array,
    required: true
  },

  loading: {
    type: Boolean,
    default: false
  },

  codeMap: {
    type: Object,
    default: () => ({})
  },

  formatPrice: {
    type: Function,
    required: true
  }
});

defineEmits([

  'selection-change',

  'open-details'
]);

const getStatusTagType = (status: string) => {
  switch (status) {
    case '보유중':
      return 'success';
    case '대여중':
      return 'warning';
    case '판매':
      return 'info';
    case '출고':
      return 'primary';
    case '반품':
      return 'danger';
    default:
      return 'info';
  }
};

const formatDateTime = (date: string) => {
  if (!date) return '-';
  return parseTime(date, '{y}-{m}-{d} {h}:{i}');
};

const tableRef = ref<any>(null);

defineExpose({
  exportExcel: (filename?: string) => tableRef.value?.exportExcel(filename)
});
</script>

<style lang="scss" scoped>
:deep(.el-table) {
  th.el-table__cell {
    padding: 0.375rem 0 !important;
    background-color: #faf9f6 !important;
    color: #222222 !important;
    font-weight: 600 !important;
    font-size: 0.9rem !important;
  }
}

:deep(.photo-column) {
  padding: 0 !important;
  .cell {
    padding: 0 !important;
    width: 80px !important;
    display: flex;
    align-items: center;
    justify-content: center;
  }
}

.product-info {
  .product-no {
    font-size: 0.8875rem;
    color: #bbbbbb;
    font-family: 'S-CoreDream', 'Jost', sans-serif;
  }
  .product-name {
    font-size: 0.875rem;
    font-weight: 600;
    color: #222;
  }
  .category {
    font-size: 0.8875rem;
    color: #888;
  }
}
</style>

