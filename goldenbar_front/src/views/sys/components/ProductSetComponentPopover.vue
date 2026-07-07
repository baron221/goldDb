<template>
<el-popover
    v-if="products && products.length > 0"
    placement="right"
    :width="500"
    trigger="click"
    teleported
  >
    <template #reference>
      <div class="component-summary-text" style="cursor: pointer; color: #606266; font-size: 13px;">
        <span style="font-weight: bold; color: #c5a880;">{{ products[0].name }}</span>
        <span v-if="(basicComponentCount || 0) + products.length > 1" style="margin-left: 4px; color: #909399;">
          외 {{ (basicComponentCount || 0) + products.length - 1 }}개
        </span>
      </div>
    </template>
    <div class="component-detail-popover">
      <div class="popover-header">
        <i class="fas fa-layer-group"></i> 구성 제품 상세 정보
      </div>
      <div class="popover-body">
        <div v-for="p in products" :key="p.id" class="popover-item">
          <div class="item-thumb">
            <el-image :src="getProductMainPhoto(p)" fit="cover" class="thumb-img">
              <template #error>
                <div class="thumb-error"><i class="fas fa-image"></i></div>
              </template>
            </el-image>
          </div>
          <div class="item-content">
            <div class="item-main-info">
              <div class="product-info-group">
                <span class="product-no" style="cursor: pointer;" @click="handleProductDetail(p.id)">[{{ p.productNo || '-' }}]</span>
                <span class="product-name" style="cursor: pointer;" @click="handleProductDetail(p.id)">{{ p.name || '-' }}</span>
              </div>
              <div class="product-meta-group">
                <span class="info-tag purity" v-if="p.purity">
                  {{ allCodesMap[p.purity] || p.purity }}
                </span>
                <span class="info-tag color" v-if="p.colors && p.colors !== 'EMPTY'">
                  {{ getColorsDisplay(p.colors) }}
                </span>
                <span class="info-weight">
                  <i class="fas fa-weight-hanging"></i> {{ (p.weight || 0).toFixed(2) }}g
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div v-if="basicComponentCount > 0" class="popover-footer">
        기타 기본 구성품 {{ basicComponentCount }}개가 포함되어 있습니다.
      </div>
    </div>
  </el-popover>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import useCodeStore from '@/store/modules/code';

const props = defineProps<{
  products: any[];
  basicComponentCount: number;
}>();

const router = useRouter();
const codeStore = useCodeStore();
const allCodesMap = computed(() => codeStore.codeMap);

const getProductMainPhoto = (product: any) => {
  if (product.photos && product.photos.length > 0) {
    const mainPhoto = product.photos.find((p: any) => p.isMain) || product.photos[0];
    return mainPhoto.photoUrl;
  }
  return null;
};

const getColorName = (code: string) => {
  const codes = codeStore.getCodesByGroupStore('PRODUCT_COLOR');
  const found = codes.find((c: any) => c.code === code);
  return found ? found.name : code;
};

const getColorsDisplay = (colors: string) => {
  if (!colors || colors === 'EMPTY') return '-';
  return colors.split(',').map(c => getColorName(c.trim())).join(', ');
};

const handleProductDetail = (id: number) => {
  if (!id) return;
  router.push({ path: `/prd/product-detail/${id}` });
};
</script>

