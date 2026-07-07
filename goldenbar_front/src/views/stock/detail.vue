<template>
<div class="stock-detail-container app-container">
    <el-card v-loading="loading" shadow="never" class="premium-detail-card">
      <div v-if="stock.id">
        <el-row :gutter="50" class="stock-main-row">

          <el-col :span="10" :xs="24" :sm="10">
            <stock-gallery
              v-model:active-photo="activePhoto"
              :default-image="defaultImage"
              :all-photos="allPhotos"
              :wide-photos="widePhotos"
              :stock="stock"
            />
          </el-col>

          <el-col :span="14" :xs="24" :sm="14">
            <stock-info
              :stock="stock"
              :code-map="codeMap"
              v-model:active-collapse-names="activeCollapseNames"
              :display-title="displayTitle"
              :category-path="categoryPath"
              @back="goBack"
              @go-to-order="goToOrder"
            />
          </el-col>
        </el-row>
      </div>
      <el-empty v-else-if="!loading" :description="$t('profile.messages.loadError')" />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, nextTick } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import store from '@/store';
import { getStockDetail } from '@/api/stock';
import { ElMessage } from 'element-plus';
import useCodeStore from '@/store/modules/code';

import StockGallery from './components/StockGallery.vue';
import StockInfo from './components/StockInfo.vue';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const loading = ref(true);
const codeStore = useCodeStore();
const stock = ref<any>({});
const activePhoto = ref('');
const activeCollapseNames = ref(['order']);
const defaultImage = 'https://via.placeholder.com/600x600?text=No+Image';
const codeMap = computed(() => codeStore.codeMap);
const widePhotos = ref<Set<string>>(new Set());

const allPhotos = computed(() => {
  const photos: string[] = [];

  if (stock.value.product?.photos) {
    stock.value.product.photos.forEach((p: any) => {
      if (p.photoUrl) photos.push(p.photoUrl);
    });
  }

  if (stock.value.productSet?.photos) {
    stock.value.productSet.photos.forEach((p: any) => {
      if (p.photoUrl) photos.push(p.photoUrl);
    });
  }

  if (stock.value.attachments) {
    stock.value.attachments.forEach((a: any) => {
      if (a.filePath) photos.push(a.filePath);
    });
  }

  const uniquePhotos = Array.from(new Set(photos));

  if (stock.value.productPhotoUrl && !uniquePhotos.includes(stock.value.productPhotoUrl)) {
    uniquePhotos.unshift(stock.value.productPhotoUrl);
  }

  return uniquePhotos.length > 0 ? uniquePhotos : [defaultImage];
});

const checkImageRatios = () => {
  widePhotos.value.clear();
  allPhotos.value.forEach(url => {
    const img = new Image();
    img.onload = () => {
      if (img.width / img.height >= 1.5) {
        widePhotos.value.add(url);
      }
    };
    img.src = url;
  });
};

const displayTitle = computed(() => {
  return stock.value.productName || stock.value.productSetTitle || '-';
});

const categoryPath = computed(() => {
  const parts = [
    codeMap.value[stock.value.categoryLarge] || stock.value.categoryLarge,
    codeMap.value[stock.value.categoryMedium] || stock.value.categoryMedium,
    codeMap.value[stock.value.categorySmall] || stock.value.categorySmall
  ].filter(Boolean);
  return parts.join(' › ');
});

const getDetail = async () => {
  const id = route.params.id as string;
  if (!id) return;

  loading.value = true;
  try {
    const [res] = await Promise.all([
      getStockDetail(parseInt(id)),
      codeStore.fetchCodes()
    ]);
    stock.value = res.data;

    if (stock.value.product?.photos?.length > 0) {
      activePhoto.value = stock.value.product.photos[0].photoUrl;
    } else if (stock.value.productPhotoUrl) {
      activePhoto.value = stock.value.productPhotoUrl;
    }

    if (stock.value.stockNo) {
      await nextTick();
      const title = `${stock.value.productName || stock.value.productSetTitle || ''}|${stock.value.stockNo}`;
      store.tagsView().updateVisitedViewTitle(route.fullPath, title);
    }
  } catch (error) {
    console.error(error);
    ElMessage.error(t('stockDetail.messages.fetchError') || 'Error fetching stock details.');
  } finally {
    loading.value = false;
    checkImageRatios();
  }
};

const goBack = () => router.back();

const goToOrder = (orderNo: string) => {
  router.push({ path: '/my/order', query: { orderNo }});
};

onMounted(() => {
  getDetail();
});
</script>

<style scoped lang="scss" src="./components/StockDetailStyles.scss"></style>

