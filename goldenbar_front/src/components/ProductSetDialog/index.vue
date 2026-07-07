<template>
<div>

    <base-popup
      draggable
      v-model="visible"
      :title="dialogStatus === 'create' ? '새 세트 등록' : '세트 정보 수정'"
      width="800px"
      @close="handleClose"
    >
      <el-form ref="dataForm" :model="temp" label-position="left" label-width="120px" style="padding: 0 1.25rem;">
        <el-tabs v-model="activeTab">
          <el-tab-pane label="기본 정보" name="basic">
            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label="세트 제목" prop="title" required>
                  <el-input v-model="temp.title" placeholder="세트 이름을 입력하세요" />
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="세트 번호" prop="setNo">
                  <el-input v-model="temp.setNo" placeholder="세트 번호를 입력하세요" />
                </el-form-item>
              </el-col>
            </el-row>
            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label="제조사" prop="companyId">
                  <company-select v-model="temp.companyId" placeholder="제조사 선택" :disabled="isCompanyUser" style="width: 100%;" />
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="기본구성품수" prop="basicComponentCount">
                  <el-input-number v-model="temp.basicComponentCount" :min="0" style="width: 100%;" />
                </el-form-item>
              </el-col>
            </el-row>
            <el-form-item label="제품 분류" required>
              <div style="display: flex; align-items: center; gap: 8px; width: 100%;">
                <common-select
                  v-model="temp.categoryLarge"
                  group-code="SET_PRODUCT_CATEGORY"
                  placeholder="대분류"
                  style="flex: 1;"
                  @change="(val, options) => handleLargeCategoryChange(val, options)"
                  @options-loaded="onLargeOptionsLoaded"
                />

                <template v-if="temp.categoryLarge">
                  <i class="fas fa-chevron-right" style="color: #c0c4cc; font-size: 0.8rem;"></i>
                  <common-select
                    v-model="temp.categoryMedium"
                    :parent-id="largeCategoryId"
                    placeholder="중분류"
                    style="flex: 1;"
                    @change="(val, options) => handleMediumCategoryChange(val, options)"
                    @options-loaded="onMediumOptionsLoaded"
                  />
                </template>

                <template v-if="temp.categoryMedium">
                  <i class="fas fa-chevron-right" style="color: #c0c4cc; font-size: 0.8rem;"></i>
                  <common-select
                    v-model="temp.categorySmall"
                    :parent-id="mediumCategoryId"
                    placeholder="소분류"
                    style="flex: 1;"
                  />
                </template>
              </div>
            </el-form-item>
            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label="공장도가격" prop="factoryPrice">
                  <amount-input v-model="temp.factoryPrice" placeholder="공장도가를 입력하세요" />
                  <div v-if="totalProductsFactoryPrice > 0" class="price-hint">
                    구성제품 합계: {{ formatPrice(totalProductsFactoryPrice) }}
                  </div>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="수공비" prop="laborCost">
                  <amount-input v-model="temp.laborCost" placeholder="수공비를 입력하세요" />
                  <div v-if="totalProductsLaborCost > 0" class="price-hint">
                    구성제품 합계: {{ formatPrice(totalProductsLaborCost) }}
                  </div>
                </el-form-item>
              </el-col>
            </el-row>
            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label="총구성품수">
                  <el-tag type="info" size="large" style="width: 100%; text-align: center;">
                    {{ (temp.basicComponentCount || 0) + (temp.connectedProducts?.length || 0) }} 개
                  </el-tag>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="공개여부" prop="isPublic">
                  <el-switch v-model="temp.isPublic" active-text="공개" inactive-text="비공개" />
                </el-form-item>
              </el-col>
            </el-row>
            <el-form-item label="세트 설명" prop="description">
              <el-input v-model="temp.description" type="textarea" :rows="3" placeholder="세트 설명을 입력하세요" />
            </el-form-item>
          </el-tab-pane>

          <el-tab-pane label="사진 관리" name="photos">
            <product-set-photo-manager :photos="temp.photos" />
          </el-tab-pane>

          <el-tab-pane label="구성 제품 연결" name="products">
            <product-set-connected-table
              :connected-products="temp.connectedProducts"
              @products-added="onProductsAdded"
            />
          </el-tab-pane>
        </el-tabs>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="visible = false">취소</el-button>
          <el-button type="primary" @click="handleSubmit">저장</el-button>
        </div>
      </template>
    </base-popup>
  </div>
</template>

<script setup lang="ts">

import { ref, watch, onMounted, computed } from 'vue';
import { getProductSet, createProductSet, updateProductSet } from '@/api/product-set';
import useCodeStore from '@/store/modules/code';
import { ElMessage } from 'element-plus';
import { formatPrice } from '@/utils/format';
import CommonSelect from '@/components/CommonSelect/index.vue';
import CompanySelect from '@/components/CompanySelect/index.vue';
import AmountInput from '@/components/AmountInput/index.vue';
import BasePopup from '@/components/BasePopup/index.vue';
import ProductSetPhotoManager from './ProductSetPhotoManager.vue';
import ProductSetConnectedTable from './ProductSetConnectedTable.vue';
import useUserStore from '@/store/modules/user';

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  id: {
    type: Number,
    default: null
  }
});

const emit = defineEmits(['update:modelValue', 'success']);

const userStore = useUserStore();
const isCompanyUser = computed(() => !userStore.roles.includes('admin'));

const codeStore = useCodeStore();
const visible = ref(false);
const dialogStatus = ref('create');
const activeTab = ref('basic');
const largeCategoryId = ref<number | null>(null);
const mediumCategoryId = ref<number | null>(null);
const allCodesFullMap = ref<any>({});

const temp = ref<any>({
  id: undefined,
  title: '',
  setNo: '',
  description: '',
  factoryPrice: 0,
  laborCost: 0,
  categoryLarge: '',
  categoryMedium: '',
  categorySmall: '',
  photos: [],
  connectedProducts: [],
  isPublic: false
});

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val) {
    if (props.id) {
      dialogStatus.value = 'update';
      fetchData(props.id);
    } else {
      dialogStatus.value = 'create';
      resetTemp();
    }
  }
});

watch(() => visible.value, (val) => {
  emit('update:modelValue', val);
});

const resetTemp = () => {
  largeCategoryId.value = null;
  mediumCategoryId.value = null;
  activeTab.value = 'basic';
  temp.value = {
    id: undefined,
    title: '',
    setNo: '',
    description: '',
    companyId: isCompanyUser.value ? userStore.companyId || undefined : undefined,
    basicComponentCount: 0,
    factoryPrice: 0,
    laborCost: 0,
    categoryLarge: '',
    categoryMedium: '',
    categorySmall: '',
    photos: [],
    connectedProducts: [],
    isPublic: false
  };
};

const fetchData = async (id: number) => {
  try {
    const response = await getProductSet(id);
    const data = response.data;

    if (data.categoryLarge) {
      const large = allCodesFullMap.value[data.categoryLarge];
      largeCategoryId.value = large ? large.id : null;
    } else {
      largeCategoryId.value = null;
    }

    if (data.categoryMedium) {
      const medium = allCodesFullMap.value[data.categoryMedium];
      mediumCategoryId.value = medium ? medium.id : null;
    } else {
      mediumCategoryId.value = null;
    }

    temp.value = {
      ...data,
      photos: data.photos || [],
      connectedProducts: data.products || []
    };
    activeTab.value = 'basic';
  } catch (error) {
    console.error(error);
  }
};

const fetchCodes = async () => {
  try {
    await codeStore.fetchCodes();
    const codes = codeStore.codes;
    const fullMap: any = {};
    const flatten = (list: any[]) => {
      list.forEach(item => {
        fullMap[item.code] = item;
        if (item.children) flatten(item.children);
      });
    };
    flatten(codes);
    allCodesFullMap.value = fullMap;
  } catch (error) {
    console.error('Failed to fetch codes:', error);
  }
};

const handleLargeCategoryChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  largeCategoryId.value = selected ? selected.id : null;
  temp.value.categoryMedium = '';
  temp.value.categorySmall = '';
  mediumCategoryId.value = null;
};

const handleMediumCategoryChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  mediumCategoryId.value = selected ? selected.id : null;
  temp.value.categorySmall = '';
};

const onLargeOptionsLoaded = (options: any[]) => {
  if (temp.value.categoryLarge) {
    const selected = options.find(o => o.code === temp.value.categoryLarge);
    largeCategoryId.value = selected ? selected.id : null;
  }
};

const onMediumOptionsLoaded = (options: any[]) => {
  if (temp.value.categoryMedium) {
    const selected = options.find(o => o.code === temp.value.categoryMedium);
    mediumCategoryId.value = selected ? selected.id : null;
  }
};

const handleSubmit = async () => {
  try {
    const payload = {
      ...temp.value,
      productIds: temp.value.connectedProducts.map((p: any) => p.id),
      photos: temp.value.photos.map((p: any, idx: number) => ({
        ...p,
        sortOrder: idx
      }))
    };

    if (dialogStatus.value === 'create') {
      await createProductSet(payload);
      ElMessage.success('세트 제품이 등록되었습니다.');
    } else {
      await updateProductSet(temp.value.id, payload);
      ElMessage.success('세트 정보가 수정되었습니다.');
    }
    visible.value = false;
    emit('success');
  } catch (error) {
    console.error(error);
  }
};

const handleClose = () => {
  visible.value = false;
};

const totalProductsFactoryPrice = computed(() => {
  return temp.value.connectedProducts.reduce((sum: number, p: any) => sum + (p.factoryPrice || 0), 0);
});

const totalProductsLaborCost = computed(() => {
  return temp.value.connectedProducts.reduce((sum: number, p: any) => sum + (p.laborCost || 0), 0);
});

const onProductsAdded = () => {
  if (temp.value.factoryPrice === 0 && totalProductsFactoryPrice.value > 0) {
    temp.value.factoryPrice = totalProductsFactoryPrice.value;
  }
  if (temp.value.laborCost === 0 && totalProductsLaborCost.value > 0) {
    temp.value.laborCost = totalProductsLaborCost.value;
  }
};

onMounted(() => {
  fetchCodes();
});
</script>

<style scoped>
.price-hint {
  font-size: 0.85rem;
  color: #909399;
  margin-top: 2px;
  line-height: 1.2;
}
</style>

