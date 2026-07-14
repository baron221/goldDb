<template>
<el-row :gutter="30">

    <el-col :xs="24" :sm="6" class="mb-4 sm:mb-0">
      <el-form-item label-width="0" style="margin-bottom: 0;">
        <div class="main-photo-card" @click="$emit('manage-photo')">
          <el-image
            v-if="currentViewPhotoUrl"
            :src="currentViewPhotoUrl"
            fit="cover"
            class="main-photo-img"
          />
          <div v-else class="main-photo-placeholder">
            <el-icon :size="32"><Picture /></el-icon>
            <span style="font-size: 0.95rem;">{{ $t('productDialog.addPhoto') }}</span>
          </div>
          <div class="main-photo-overlay">
            <el-icon :size="20"><Edit /></el-icon>
            <span style="margin-top: 0.3125rem; font-size: 0.95rem;">{{ $t('productDialog.managePhoto') }}</span>
          </div>
        </div>

        <div v-if="localTemp.photos && localTemp.photos.length > 0" class="photo-thumbnail-container">
          <div
            v-for="(photo, index) in sortedPhotos"
            :key="photo.id || index"
            class="photo-thumbnail-item"
            :class="{ active: currentViewPhotoUrl === photo.photoUrl, 'is-main': photo.isMain || index === 0 }"
            @click.stop="$emit('update:activePhotoUrl', photo.photoUrl)"
          >
            <el-image :src="photo.photoUrl" fit="cover" class="thumbnail-img" />
            <span v-if="photo.isMain || index === 0" class="main-badge">{{ $t('productDialog.main') }}</span>
          </div>
        </div>
      </el-form-item>
    </el-col>

    <el-col :xs="24" :sm="18">
      <el-row :gutter="20">
        <el-col :xs="24" :sm="12">
          <el-form-item :label="$t('productDialog.name')" prop="name" required>
            <el-input v-model="localTemp.name" />
          </el-form-item>
        </el-col>
        <el-col :xs="24" :sm="12">
          <el-form-item :label="$t('productDialog.productNo')" prop="productNo">
            <el-input v-model="localTemp.productNo" />
          </el-form-item>
        </el-col>
      </el-row>
      <el-row :gutter="20">
        <el-col :xs="24" :sm="12">
          <el-form-item :label="$t('productDialog.purity')" prop="purity" required>
            <common-select v-model="localTemp.purity" group-code="MATERIAL_GRADE" :placeholder="$t('productDialog.purityPlaceholder')" multiple />
          </el-form-item>
        </el-col>
        <el-col :xs="24" :sm="12">
          <el-form-item :label="$t('productDialog.weight')" required>
            <div style="display: flex; flex-direction: column; width: 100%; gap: 10px;">
              <div v-for="(pCode, index) in localTemp.purity" :key="pCode" style="display: flex; align-items: center; gap: 10px;">
                <span style="min-width: 50px; font-weight: bold; color: #606266;">{{ getCodeName(pCode) }}</span>
                <el-input-number 
                  v-model="localTemp.purityWeightsMap[pCode]" 
                  @change="(val) => handlePurityWeightChange(pCode, val, index)"
                  :precision="2" :step="0.01" :min="0" style="flex: 1;" 
                />
              </div>
              <div v-if="!localTemp.purity || localTemp.purity.length === 0" style="display: flex; align-items: center; gap: 10px; width: 100%">
                <el-input-number v-model="localTemp.weight" :precision="2" :step="0.01" style="flex: 1;" disabled :placeholder="$t('productDialog.purityPlaceholder')" />
              </div>
            </div>
          </el-form-item>
        </el-col>
      </el-row>

      <el-row :gutter="20">
        <el-col :span="12">
          <el-form-item :label="$t('productDialog.colors')" prop="colors" required>
            <el-select
              v-model="localTemp.colors"
              multiple
              :placeholder="$t('productDialog.colorsPlaceholder')"
              style="width: 100%"
            >
              <el-option
                v-for="item in productColors"
                :key="item.code"
                :label="item.name"
                :value="item.code"
              />
            </el-select>
          </el-form-item>
        </el-col>

        <el-col :span="12">
          <el-form-item :label="$t('productDialog.manufacturer')" prop="companyId">
            <company-select v-model="localTemp.companyId" category="MFG" :disabled="isCompanyUser" style="width: 100%;" />
          </el-form-item>
        </el-col>
      </el-row>

      <el-row :gutter="20">
        <el-col :xs="24" :sm="8">
          <el-form-item :label="$t('productDialog.categoryLarge')" prop="categoryLarge" required label-width="60px">
            <common-select
              v-model="localTemp.categoryLarge"
              group-code="PRODUCT_CATEGORY"
              :placeholder="$t('productDialog.categoryLargePlaceholder')"
              @change="$emit('change-large-category', $event)"
              @options-loaded="$emit('loaded-large-options', $event)"
            />
          </el-form-item>
        </el-col>
        <el-col :xs="24" :sm="8">
          <el-form-item :label="$t('productDialog.categoryMedium')" prop="categoryMedium" label-width="60px">
            <common-select
              v-model="localTemp.categoryMedium"
              :options="mediumCategoryOptions"
              :placeholder="$t('productDialog.categoryMediumPlaceholder')"
              @change="$emit('change-medium-category', $event)"
              @options-loaded="$emit('loaded-medium-options', $event)"
            />
          </el-form-item>
        </el-col>
        <el-col :xs="24" :sm="8">
          <el-form-item :label="$t('productDialog.categorySmall')" prop="categorySmall" label-width="60px">
            <common-select
              v-model="localTemp.categorySmall"
              :options="smallCategoryOptions"
              :placeholder="$t('productDialog.categorySmallPlaceholder')"
            />
          </el-form-item>
        </el-col>
      </el-row>

      <el-row :gutter="20">
        <el-col :xs="24" :sm="12">
          <el-form-item :label="$t('productDialog.factoryPrice')" prop="factoryPrice">
            <amount-input
              v-model="localTemp.factoryPrice"
              :placeholder="$t('productDialog.factoryPricePlaceholder')"
            />
          </el-form-item>
        </el-col>
      </el-row>
      <el-row :gutter="20">
        <el-col :xs="24" :sm="12">
          <el-form-item :label="$t('productDialog.isPublic')" prop="isPublic">
            <el-switch v-model="localTemp.isPublic" :active-text="$t('productDialog.public')" :inactive-text="$t('productDialog.private')" />
          </el-form-item>
        </el-col>
        <el-col :xs="24" :sm="12">
          <el-form-item :label="$t('productDialog.laborCost')" prop="laborCost" required>
            <amount-input
              v-model="localTemp.laborCost"
              :placeholder="$t('productDialog.laborCostPlaceholder')"
            />
          </el-form-item>
        </el-col>
      </el-row>
      <el-row :gutter="20">
        <el-col :span="24">
          <el-form-item :label="$t('productDialog.designNotice')" prop="designNotice">
            <el-input v-model="localTemp.designNotice" type="textarea" :rows="2" :placeholder="$t('productDialog.designNoticePlaceholder')" />
          </el-form-item>
        </el-col>
      </el-row>
    </el-col>
  </el-row>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { Picture, Edit } from '@element-plus/icons-vue';
import CommonSelect from '@/components/CommonSelect/index.vue';
import CompanySelect from '@/components/CompanySelect/index.vue';
import AmountInput from '@/components/AmountInput/index.vue';
import useCodeStore from '@/store/modules/code';

const codeStore = useCodeStore();

const getCodeName = (code: string) => {
  return codeStore.getCodeName(code) || code;
};

const handlePurityWeightChange = (pCode: string, val: number, index: number) => {
  if (index === 0) {
    localTemp.value.weight = val;
  }
};

const props = defineProps({
  temp: {
    type: Object,
    required: true
  },
  productColors: {
    type: Array,
    required: true
  },
  isCompanyUser: {
    type: Boolean,
    required: true
  },
  mediumCategoryOptions: {
    type: Array,
    required: true
  },
  smallCategoryOptions: {
    type: Array,
    required: true
  },
  currentViewPhotoUrl: {
    type: String,
    required: true
  },
  sortedPhotos: {
    type: Array,
    required: true
  }
});

const emit = defineEmits([
  'update:temp',
  'update:activePhotoUrl',
  'change-large-category',
  'change-medium-category',
  'loaded-large-options',
  'loaded-medium-options',
  'manage-photo'
]);

const localTemp = ref({ ...props.temp });

watch(() => props.temp, (val) => {
  localTemp.value = { ...val };
}, { deep: true });

watch(localTemp, (val) => {
  emit('update:temp', val);
}, { deep: true });
</script>

<style scoped>

.main-photo-card {
  width: 100%;
  aspect-ratio: 1 / 1;
  border: 1px solid #dcdfe6;
  border-radius: 2px;
  position: relative;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.3s ease;
  background-color: #f5f7fa;
  box-sizing: border-box;
}

.main-photo-card:hover {
  border-color: #409eff;
  box-shadow: 0 4px 12px rgba(64, 158, 255, 0.15);
}

.main-photo-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.main-photo-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #909399;
  gap: 0.5rem;
}

.main-photo-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  color: #ffffff;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: opacity 0.3s ease;
  z-index: 2;
}

.main-photo-card:hover .main-photo-overlay {
  opacity: 1;
}

.photo-thumbnail-container {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-top: 0.625rem;
  max-height: 120px;
  overflow-y: auto;
  padding: 0.125rem;
}

.photo-thumbnail-item {
  width: 48px;
  height: 48px;
  border: 1px solid #dcdfe6;
  border-radius: 2px;
  position: relative;
  cursor: pointer;
  overflow: hidden;
  transition: all 0.2s ease;
  box-sizing: border-box;
}

.photo-thumbnail-item:hover {
  border-color: #409eff;
  transform: translateY(-2px);
}

.photo-thumbnail-item.active {
  border-color: #409eff;
  box-shadow: 0 0 0 2px rgba(64, 158, 255, 0.2);
}

.thumbnail-img {
  width: 100%;
  height: 100%;
}

.main-badge {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  background-color: rgba(64, 158, 255, 0.95);
  color: #ffffff;
  font-size: 0.825rem;
  text-align: center;
  padding: 0.0625rem 0;
  font-weight: bold;
  transform: scale(0.95);
}
</style>

