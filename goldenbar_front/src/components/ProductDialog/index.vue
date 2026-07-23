<template>
<base-popup
    v-model="visible"
    :title="dialogStatus === 'create' ? $t('productDialog.createTitle') : $t('productDialog.editTitle')"
    draggable
    class="product-dialog responsive-dialog-800"
  >
    <el-form ref="dataForm" :rules="formRules" :model="temp" label-position="left" label-width="120px" class="dialog-form">
      <el-tabs v-model="activeTab" class="fixed-height-tabs">

        <el-tab-pane :label="$t('productDialog.tabBasic')" name="basic">
          <product-basic-info
            :temp="temp"
            :product-colors="productColors"
            :is-company-user="isCompanyUser"
            :medium-category-options="mediumCategoryOptions"
            :small-category-options="smallCategoryOptions"
            :current-view-photo-url="currentViewPhotoUrl"
            :sorted-photos="sortedPhotos"
            v-model:active-photo-url="activePhotoUrl"
            @update:temp="(val) => Object.assign(temp, val)"
            @change-large-category="handleLargeCategoryChange"
            @change-medium-category="handleMediumCategoryChange"
            @loaded-large-options="onLargeOptionsLoaded"
            @loaded-medium-options="onMediumOptionsLoaded"
            @manage-photo="photoManagerVisible = true"
          />
        </el-tab-pane>

        <el-tab-pane :label="$t('productDialog.tabOptions')" name="options">
          <product-option-info
            :temp="temp"
            :combination-grid-data="combinationGridData"
            v-model:calc-base-purity="calcBasePurity"
            v-model:calc-base-weight="calcBaseWeight"
            @update:temp="(val) => Object.assign(temp, val)"
            @apply-bulk-conversion="applyBulkWeightConversion"
          />
        </el-tab-pane>

        <el-tab-pane :label="$t('productDialog.tabStones')" name="stones">
          <product-stone-info :temp="temp" @update:temp="(val) => Object.assign(temp, val)" />
        </el-tab-pane>
      </el-tabs>
    </el-form>

    <PhotoManagerDialog
      v-model="photoManagerVisible"
      v-model:photos="temp.photos"
      sub-dir="products"
    />
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="visible = false">{{ $t('productDialog.cancel') }}</el-button>
        <template v-if="dialogStatus === 'create'">
          <el-button type="success" @click="createData(true)">{{ $t('productDialog.saveAndContinue') }}</el-button>
          <el-button type="primary" @click="createData(false)">{{ $t('productDialog.save') }}</el-button>
        </template>
        <template v-else>
          <el-button type="primary" @click="updateData()">{{ $t('productDialog.save') }}</el-button>
        </template>
      </div>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import BasePopup from '@/components/BasePopup/index.vue';
import PhotoManagerDialog from '@/components/PhotoManagerDialog/index.vue';

import ProductBasicInfo from './components/ProductBasicInfo.vue';
import ProductOptionInfo from './components/ProductOptionInfo.vue';
import ProductStoneInfo from './components/ProductStoneInfo.vue';
import { useProductDialog } from './hooks/useProductDialog';

const props = defineProps<{
  modelValue: boolean;
  productId?: number | null;
}>();

const emit = defineEmits(['update:modelValue', 'saved']);

const dataForm = ref<any>(null);

const {
  visible,
  activeTab,
  productColors,
  temp,
  activePhotoUrl,
  currentViewPhotoUrl,
  sortedPhotos,
  mediumCategoryOptions,
  smallCategoryOptions,
  photoManagerVisible,
  isCompanyUser,
  combinationGridData,
  calcBasePurity,
  calcBaseWeight,
  handleLargeCategoryChange,
  handleMediumCategoryChange,
  onLargeOptionsLoaded,
  onMediumOptionsLoaded,
  applyBulkWeightConversion,
  createData,
  updateData
} = useProductDialog(props, emit, dataForm);

const dialogStatus = computed(() => props.productId ? 'update' : 'create');
const { t } = useI18n();

const formRules = {
  name: [{ required: true, message: t('productDialog.nameRequired'), trigger: 'blur' }],
  categoryLarge: [{ required: true, message: t('productDialog.categoryLargeRequired'), trigger: 'change' }],
  purity: [{ type: 'array', required: true, message: t('productDialog.purityRequired'), trigger: 'change' }],
  colors: [{ type: 'array', required: true, message: t('productDialog.colorsRequired'), trigger: 'change' }],
  weight: [{ required: true, message: t('productDialog.weightRequired'), trigger: 'blur' }],
  laborCost: [{ required: true, message: t('productDialog.laborCostRequired'), trigger: 'blur' }]
};
</script>

<style scoped>

.product-dialog :deep(.el-dialog__body) {
  padding: 0.625rem 1.25rem;
}

.dialog-form {
  height: 550px;
  display: flex;
  flex-direction: column;
}

.fixed-height-tabs {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.fixed-height-tabs :deep(.el-tabs__content) {
  flex: 1;
  overflow-y: auto;
  padding: 0.625rem 0.3125rem;
}

.fixed-height-tabs :deep(.el-tab-pane) {
  height: 100%;
}

:deep(.responsive-dialog-800) {
  width: 800px !important;
}

@media (max-width: 991px) {
  :deep(.responsive-dialog-800) {
    width: 92% !important;
  }
  :deep(.el-form-item) {
    flex-direction: column;
    align-items: flex-start;
  }
  :deep(.el-form-item__label) {
    text-align: left;
    margin-bottom: 4px;
  }
  :deep(.el-form-item__content) {
    width: 100%;
    margin-left: 0 !important;
  }
}
</style>

