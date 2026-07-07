<template>
<base-popup
    draggable
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    :title="pageEditIndex === null ? '페이지 정보 추가' : '페이지 정보 수정'"
    width="600px"
    append-to-body
  >
    <el-form ref="pageForm" :model="localPageTemp" label-position="left" label-width="120px" style="padding: 0 1.25rem;">
      <el-form-item label="페이지 번호" required>
        <el-input-number v-model="localPageTemp.pageNumber" :min="1" style="width: 100%;" />
      </el-form-item>
      <el-form-item label="페이지 사진">
        <image-upload
          v-model:initial-url="localPageTemp.photoUrl"
          sub-dir="catalogs"
        />
      </el-form-item>
      <el-form-item label="설명">
        <el-input v-model="localPageTemp.description" type="textarea" :rows="3" placeholder="페이지 설명을 입력하세요" />
      </el-form-item>
      <el-form-item label="제조사">
        <company-select v-model="localPageTemp.companyId" category="MFG" style="width: 100%;" @change="handlePageCompanyChange" />
      </el-form-item>
      <el-form-item label="제품 분류">
        <el-row :gutter="10" style="width: 100%;">
          <el-col :span="8">
            <common-select
              v-model="localPageTemp.categoryLarge"
              group-code="PRODUCT_CATEGORY"
              placeholder="대분류"
              @change="handlePageLargeChange"
              @options-loaded="onPageLargeOptionsLoaded"
            />
          </el-col>
          <el-col :span="8">
            <common-select
              v-model="localPageTemp.categoryMedium"
              :parent-id="pageLargeId"
              placeholder="중분류"
              @change="handlePageMediumChange"
              @options-loaded="onPageMediumOptionsLoaded"
            />
          </el-col>
          <el-col :span="8">
            <common-select
              v-model="localPageTemp.categorySmall"
              :parent-id="pageMediumId"
              placeholder="소분류"
            />
          </el-col>
        </el-row>
      </el-form-item>
      <el-form-item label="연결 제품">
        <div style="margin-bottom: 0.625rem;">
          <el-button type="primary" size="small" @click="$emit('open-product-selector')">제품 추가</el-button>
        </div>
        <base-table :data="localPageTemp.connectedProducts" style="width: 100%">
          <el-table-column prop="productNo" label="제품번호" width="120" />
          <el-table-column prop="name" label="제품명" />
          <el-table-column label="관리" width="80" align="center">
            <template #default="scope">
              <el-button link class="delete-icon-btn" :icon="Delete" @click="$emit('remove-product', scope.$index)" />
            </template>
          </el-table-column>
        </base-table>
      </el-form-item>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="$emit('update:modelValue', false)">취소</el-button>
        <el-button type="primary" @click="$emit('save')">확인</el-button>
        <el-button type="success" @click="$emit('save-continue')">저장 후 계속 추가</el-button>
      </div>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { Delete } from '@element-plus/icons-vue';
import BasePopup from '@/components/BasePopup/index.vue';
import ImageUpload from '@/components/ImageUpload/index.vue';
import CompanySelect from '@/components/CompanySelect/index.vue';
import CommonSelect from '@/components/CommonSelect/index.vue';
import BaseTable from '@/components/BaseTable/index.vue';

const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true
  },
  pageTemp: {
    type: Object,
    required: true
  },
  pageEditIndex: {
    type: [Number, null] as any,
    required: true
  },
  pageLargeId: {
    type: [Number, null] as any,
    required: true
  },
  pageMediumId: {
    type: [Number, null] as any,
    required: true
  }
});

const emit = defineEmits([
  'update:modelValue',
  'update:pageLargeId',
  'update:pageMediumId',
  'update:pageTemp',
  'open-product-selector',
  'remove-product',
  'save',
  'save-continue'
]);

const localPageTemp = ref({ ...props.pageTemp });

watch(() => props.pageTemp, (val) => {
  localPageTemp.value = { ...val };
}, { deep: true });

watch(localPageTemp, (val) => {
  emit('update:pageTemp', val);
}, { deep: true });

const onPageLargeOptionsLoaded = (options: any[]) => {
  if (localPageTemp.value.categoryLarge) {
    const found = options.find((o: any) => o.code === localPageTemp.value.categoryLarge);
    emit('update:pageLargeId', found ? found.id : null);
  }
};

const onPageMediumOptionsLoaded = (options: any[]) => {
  if (localPageTemp.value.categoryMedium) {
    const found = options.find((o: any) => o.code === localPageTemp.value.categoryMedium);
    emit('update:pageMediumId', found ? found.id : null);
  }
};

const handlePageLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  emit('update:pageLargeId', selected ? selected.id : null);
  localPageTemp.value.categoryMedium = '';
  localPageTemp.value.categorySmall = '';
  emit('update:pageMediumId', null);
};

const handlePageMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  emit('update:pageMediumId', selected ? selected.id : null);
  localPageTemp.value.categorySmall = '';
};

const handlePageCompanyChange = (val: number | null, option: any) => {
  localPageTemp.value.companyName = option ? option.name : '';
};
</script>

