<template>
<div class="catalog-viewer-page app-container">

    <catalog-selection
      v-if="!selectedCatalog"
      :catalog-list="catalogList"
      :loading="loading"
      @select="selectCatalog"
    />

    <catalog-book-viewer
      v-else
      :selected-catalog="selectedCatalog"
      :catalog-list="catalogList"
      :active-catalog-id="activeCatalogId"
      @close="closeViewer"
      @select-catalog="selectCatalog"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { getCatalogs, getCatalog } from '@/api/catalog';
import { ElMessage } from 'element-plus';
import CatalogSelection from './components/CatalogSelection.vue';
import CatalogBookViewer from './components/CatalogBookViewer.vue';

const loading = ref(false);
const catalogList = ref<any[]>([]);
const selectedCatalog = ref<any>(null);
const activeCatalogId = ref<number | null>(null);

onMounted(() => {
  fetchCatalogs();
});

const fetchCatalogs = async () => {
  loading.value = true;
  try {
    const res = await getCatalogs({ page: 1, pageSize: 100 });
    catalogList.value = res.data.items;
  } catch (error) {
    console.error('Failed to load catalogs:', error);
  } finally {
    loading.value = false;
  }
};

const selectCatalog = async (id: number) => {
  loading.value = true;
  try {
    const res = await getCatalog(id);
    selectedCatalog.value = res.data;
    activeCatalogId.value = id;
  } catch (error) {
    console.error('Failed to load catalog detail:', error);
    ElMessage.error('책자 정보를 불러오는데 실패했습니다.');
  } finally {
    loading.value = false;
  }
};

const closeViewer = () => {
  selectedCatalog.value = null;
  activeCatalogId.value = null;
};
</script>

<style lang="scss" scoped>
.catalog-viewer-page {
  min-height: calc(100vh - 50px);
}
</style>

