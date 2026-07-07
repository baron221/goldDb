<template>
<el-col :xs="24" :sm="24" :md="8" :lg="6" class="shop-sidebar-content">
    <div class="sidebar-widgets-wrapper">

      <div class="sidebar-widget search-widget">
        <h4 class="widget-title">{{ $t('favorite.labels.searchTitle') }}</h4>
        <div class="search-input-box">
          <el-input
            :model-value="searchQuery"
            :placeholder="$t('favorite.labels.searchPlaceholder')"
            clearable
            @input="emitSearchQuery"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
        </div>
      </div>

      <div class="sidebar-widget categories-widget">
        <h4 class="widget-title">{{ $t('favorite.labels.summary') }}</h4>
        <div class="summary-list">
          <div class="summary-item">
            <span class="label">{{ $t('favorite.labels.totalSaved') }}</span>
            <span class="value">{{ favoriteList.length }}</span>
          </div>
          <div class="summary-item">
            <span class="label">{{ $t('favorite.labels.generalProducts') }}</span>
            <span class="value">{{ favoriteList.filter(f => f.productId).length }}</span>
          </div>
          <div class="summary-item">
            <span class="label">{{ $t('favorite.labels.setProducts') }}</span>
            <span class="value">{{ favoriteList.filter(f => f.productSetId).length }}</span>
          </div>
        </div>
        <div class="sidebar-actions">
          <el-button class="market-link-btn" link @click="goBackToMarket">
            {{ $t('favorite.actions.backToMarket') }} <el-icon><ArrowRight /></el-icon>
          </el-button>
        </div>
      </div>

    </div>
  </el-col>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';
import { Search, ArrowRight } from '@element-plus/icons-vue';

defineProps<{
  searchQuery: string;
  favoriteList: any[];
}>();

const emit = defineEmits(['update:searchQuery']);

const router = useRouter();

const emitSearchQuery = (val: string) => {
  emit('update:searchQuery', val);
};

const goBackToMarket = () => {
  router.push('/prd/product-market');
};
</script>

