<template>
<base-popup
    draggable
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    title="대량 페이지 추가"
    width="650px"
    append-to-body
  >
    <div style="padding: 0 1.25rem;" v-loading="bulkLoading">
      <el-alert
        title="여러 장의 사진을 선택하면 현재 마지막 페이지 번호 이후로 자동 등록됩니다."
        type="info"
        show-icon
        :closable="false"
        style="margin-bottom: 1rem;"
      />

      <el-form label-position="top">
        <el-form-item label="사진 업로드 (여러 개 선택 가능)">
          <el-upload
            class="bulk-uploader"
            drag
            action="#"
            multiple
            :auto-upload="false"
            :on-change="handleBulkUploadChange"
            :file-list="bulkFileList"
          >
            <el-icon class="el-icon--upload"><upload-filled /></el-icon>
            <div class="el-upload__text">
              파일을 드래그하거나 <em>클릭하여 업로드</em>하세요.
            </div>
          </el-upload>
        </el-form-item>

        <el-divider content-position="left">공통 속성 설정</el-divider>

        <el-form-item label="공통 설명">
          <el-input v-model="localBulkTemp.description" placeholder="선택한 모든 페이지에 적용될 설명" />
        </el-form-item>

        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="공통 제조사">
              <company-select v-model="localBulkTemp.companyId" category="MFG" style="width: 100%;" @change="handleBulkCompanyChange" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="공통 제품 분류">
              <el-row :gutter="5">
                <el-col :span="12">
                  <common-select
                    v-model="localBulkTemp.categoryLarge"
                    group-code="PRODUCT_CATEGORY"
                    placeholder="대분류"
                    @change="handleBulkLargeChange"
                    @options-loaded="onBulkLargeOptionsLoaded"
                  />
                </el-col>
                <el-col :span="12">
                  <common-select
                    v-model="localBulkTemp.categoryMedium"
                    :parent-id="bulkLargeId"
                    placeholder="중분류"
                    @change="handleBulkMediumChange"
                    @options-loaded="onBulkMediumOptionsLoaded"
                  />
                </el-col>
              </el-row>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </div>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="$emit('update:modelValue', false)" :disabled="bulkLoading">취소</el-button>
        <el-button type="primary" @click="$emit('save')" :loading="bulkLoading">일괄 업로드 및 추가</el-button>
      </div>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { UploadFilled } from '@element-plus/icons-vue';
import BasePopup from '@/components/BasePopup/index.vue';
import CompanySelect from '@/components/CompanySelect/index.vue';
import CommonSelect from '@/components/CommonSelect/index.vue';

const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true
  },
  bulkLoading: {
    type: Boolean,
    required: true
  },
  bulkFileList: {
    type: Array,
    required: true
  },
  bulkLargeId: {
    type: [Number, null] as any,
    required: true
  },
  bulkMediumId: {
    type: [Number, null] as any,
    required: true
  },
  bulkTemp: {
    type: Object,
    required: true
  }
});

const emit = defineEmits([
  'update:modelValue',
  'update:bulkFileList',
  'update:bulkLargeId',
  'update:bulkMediumId',
  'update:bulkTemp',
  'save'
]);

const localBulkTemp = ref({ ...props.bulkTemp });

watch(() => props.bulkTemp, (val) => {
  localBulkTemp.value = { ...val };
}, { deep: true });

watch(localBulkTemp, (val) => {
  emit('update:bulkTemp', val);
}, { deep: true });

const handleBulkUploadChange = (file: any, fileList: any[]) => {
  emit('update:bulkFileList', fileList);
};

const onBulkLargeOptionsLoaded = (options: any[]) => {
  if (localBulkTemp.value.categoryLarge) {
    const found = options.find((o: any) => o.code === localBulkTemp.value.categoryLarge);
    emit('update:bulkLargeId', found ? found.id : null);
  }
};

const onBulkMediumOptionsLoaded = (options: any[]) => {
  if (localBulkTemp.value.categoryMedium) {
    const found = options.find((o: any) => o.code === localBulkTemp.value.categoryMedium);
    emit('update:bulkMediumId', found ? found.id : null);
  }
};

const handleBulkLargeChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  emit('update:bulkLargeId', selected ? selected.id : null);
  localBulkTemp.value.categoryMedium = '';
  localBulkTemp.value.categorySmall = '';
  emit('update:bulkMediumId', null);
};

const handleBulkMediumChange = (val: string, options: any) => {
  const selected = options.find((o: any) => o.code === val);
  emit('update:bulkMediumId', selected ? selected.id : null);
  localBulkTemp.value.categorySmall = '';
};

const handleBulkCompanyChange = (val: number | null, option: any) => {
  localBulkTemp.value.companyName = option ? option.name : '';
};
</script>

<style scoped>
.bulk-uploader {
  width: 100%;
}
:deep(.bulk-uploader .el-upload-dragger) {
  padding: 2rem 1.25rem;
}
</style>

