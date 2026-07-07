<template>
  <div>

    <base-popup
      draggable
      v-model="visible"
      :title="dialogStatus === 'create' ? '새 책 등록' : '책 정보 수정'"
      width="700px"
    >
      <el-form ref="dataForm" :model="temp" label-position="left" label-width="120px" style="padding: 0 1.25rem;">
        <el-tabs v-model="activeTab">
          <el-tab-pane label="기본 정보" name="basic">
            <el-form-item label="책제목" prop="title" required>
              <el-input v-model="temp.title" />
            </el-form-item>
            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label="보안분류" prop="securityClass">
                  <el-select v-model="temp.securityClass" style="width: 100%;">
                    <el-option label="일반" value="일반" />
                    <el-option label="보안" value="보안" />
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="발행호" prop="issueNo">
                  <el-input v-model="temp.issueNo" />
                </el-form-item>
              </el-col>
            </el-row>
            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label="전체페이지수" prop="totalPages">
                  <el-input-number v-model="temp.totalPages" :min="1" style="width: 100%;" />
                </el-form-item>
              </el-col>
            </el-row>
            <el-form-item label="책자사진" prop="photoUrl">
              <image-upload
                v-model:initial-url="temp.photoUrl"
                sub-dir="catalogs"
              />
            </el-form-item>
          </el-tab-pane>

          <el-tab-pane label="페이지 상세 정보" name="pages">
            <catalog-page-list
              :temp="temp"
              :active-tab="activeTab"
              @open-bulk="openBulkPageEditor"
              @add-page="openPageEditor(null)"
              @edit-page="({ page, index }) => openPageEditor(page, index)"
              @remove-page="removePage"
              @update-pages-order="handleUpdatePagesOrder"
            />
          </el-tab-pane>
        </el-tabs>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="visible = false">취소</el-button>
          <el-button type="primary" @click="dialogStatus === 'create' ? createData() : updateData()">저장</el-button>
        </div>
      </template>
    </base-popup>

    <catalog-page-editor
      v-model="pageEditorVisible"
      :page-temp="pageTemp"
      :page-edit-index="pageEditIndex"
      v-model:page-large-id="pageLargeId"
      v-model:page-medium-id="pageMediumId"
      @open-product-selector="openProductSelector"
      @remove-product="removeProduct"
      @save="savePageData"
      @save-continue="saveAndContinuePageData"
    />

    <catalog-bulk-editor
      v-model="bulkEditorVisible"
      :bulk-loading="bulkLoading"
      v-model:bulk-file-list="bulkFileList"
      v-model:bulk-large-id="bulkLargeId"
      v-model:bulk-medium-id="bulkMediumId"
      :bulk-temp="bulkTemp"
      @save="saveBulkPageData"
    />

    <catalog-product-selector
      v-model="productSelectorVisible"
      :product-loading="productLoading"
      :product-list="productList"
      :product-total="productTotal"
      :product-query="productQuery"
      @get-product-list="getProductList"
      @add-selected-products="addSelectedProducts"
    />
  </div>
</template>

<script setup lang="ts">

import { ref, watch } from 'vue';
import BasePopup from '@/components/BasePopup/index.vue';
import { createCatalog, updateCatalog, getCatalog } from '@/api/catalog';
import { ElMessage } from 'element-plus';
import ImageUpload from '@/components/ImageUpload/index.vue';
import CatalogPageList from './components/CatalogPageList.vue';
import CatalogPageEditor from './components/CatalogPageEditor.vue';
import CatalogBulkEditor from './components/CatalogBulkEditor.vue';
import CatalogProductSelector from './components/CatalogProductSelector.vue';
import { useCatalogPageEditor } from './composables/useCatalogPageEditor';

const props = defineProps<{ modelValue: boolean; id?: number | null }>();
const emit = defineEmits(['update:modelValue', 'success']);

const visible = ref(false);
const activeTab = ref('basic');
const dialogStatus = ref('create');

const temp = ref<any>({
  id: undefined, title: '', securityClass: '일반', issueNo: '', photoUrl: '', totalPages: 1, pages: []
});

const {
  productSelectorVisible, productLoading, productList, productTotal, productQuery,
  openProductSelector, addSelectedProducts, removeProduct, getProductList,
  bulkEditorVisible, bulkLoading, bulkFileList, bulkLargeId, bulkMediumId, bulkTemp,
  openBulkPageEditor, saveBulkPageData,
  pageEditorVisible, pageEditIndex, pageLargeId, pageMediumId, pageTemp,
  openPageEditor, savePageData, saveAndContinuePageData, removePage, handleUpdatePagesOrder
} = useCatalogPageEditor(temp);

watch(() => props.modelValue, async (val) => {
  visible.value = val;
  if (val) {
    activeTab.value = 'basic';
    resetTemp();
    if (props.id) {
      dialogStatus.value = 'update';
      await loadCatalogData(props.id);
    } else {
      dialogStatus.value = 'create';
    }
  }
});

watch(() => visible.value, (val) => { emit('update:modelValue', val); });

watch(() => temp.value.pages, (newPages) => {
  if (newPages && newPages.length > 0) {
    newPages.sort((a: any, b: any) => a.pageNumber - b.pageNumber);
    const maxPage = Math.max(...newPages.map((p: any) => p.pageNumber));
    if (maxPage > 0) temp.value.totalPages = maxPage;
  }
}, { deep: true });

const resetTemp = () => {
  temp.value = { id: undefined, title: '', securityClass: '일반', issueNo: '', photoUrl: '', totalPages: 1, pages: [] };
};

const loadCatalogData = async (id: number) => {
  try {
    const response = await getCatalog(id);
    temp.value = { ...response.data, pages: response.data.pages || [] };
  } catch (error) {
    console.error(error);
    ElMessage.error('책 정보를 불러오는데 실패했습니다.');
  }
};

const createData = async () => {
  try {
    await createCatalog({ ...temp.value, pages: temp.value.pages || [] });
    visible.value = false;
    ElMessage.success('책 정보가 등록되었습니다.');
    emit('success');
  } catch (error) { console.error(error); }
};

const updateData = async () => {
  try {
    await updateCatalog(temp.value.id, { ...temp.value, pages: temp.value.pages || [] });
    visible.value = false;
    ElMessage.success('책 정보가 수정되었습니다.');
    emit('success');
  } catch (error) { console.error(error); }
};
</script>

<style scoped src="./components/CatalogDialogStyles.scss"></style>
