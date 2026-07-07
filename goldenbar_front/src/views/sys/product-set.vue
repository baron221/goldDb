<template>
<div class="app-container">
    <template v-if="!isUnassignedUser">
      <el-card shadow="never" class="list-card">

        <product-set-filter
          :list-query="listQuery"
          @filter="handleFilter"
          @reset="resetQuery"
          @create="handleCreate"
        />

        <base-table
          v-loading="listLoading"
          :data="list"
          :total="total"
          v-model:page="listQuery.page"
          v-model:page-size="listQuery.pageSize"
          style="width: 100%;"
          @row-dblclick="handleDetail"
          @change="getList"
        >
          <el-table-column label="" width="82" header-align="center" class-name="col-preview" :fixed="!isMobile ? 'left' : false" :sortable="false" >
            <template #default="{row}">
              <div v-if="getSetMainPhoto(row)" class="preview-wrap">
                <el-image
                  :src="getSetMainPhoto(row)"
                  fit="cover"
                  :preview-src-list="getSetPhotoList(row)"
                  preview-teleported
                  class="preview-img"
                />
              </div>
              <span v-else style="padding-left: 0.25rem;">-</span>
            </template>
          </el-table-column>
          <el-table-column label="세트 제목" min-width="200" header-align="center" :fixed="!isMobile ? 'left' : false">
            <template #default="{row}">
              <div style="line-height: 1.4;">
                <div style="font-weight: bold; color: #303133;">{{ row.title }}</div>
                <div v-if="getCategoryParts(row).length > 0" style="font-size: 11px; color: #909399; margin-top: 4px; display: flex; align-items: center; gap: 6px;">
                  <template v-for="(part, idx) in getCategoryParts(row)" :key="idx">
                    <span>{{ part }}</span>
                    <i v-if="idx < getCategoryParts(row).length - 1" class="fas fa-chevron-right" style="font-size: 8px; opacity: 0.5;"></i>
                  </template>
                </div>
              </div>
            </template>
          </el-table-column>
          <el-table-column
            label="제조사"
            width="120"
            header-align="center"
            align="center"
            :excel-formatter="(row) => row.companyName || '-'"
          >
            <template #default="{row}">
              <span>{{ row.companyName || '-' }}</span>
            </template>
          </el-table-column>
          <el-table-column
            label="구성품"
            align="left"
            min-width="160"
            header-align="center"
            :excel-formatter="(row) => ((row.basicComponentCount || 0) + (row.products?.length || 0)) + '개'"
          >
            <template #default="{row}">
              <div class="component-text-wrap">
                <product-set-component-popover
                  v-if="row.products && row.products.length > 0"
                  :products="row.products"
                  :basic-component-count="row.basicComponentCount || 0"
                />
                <div v-else-if="row.basicComponentCount > 0" style="font-size: 13px; color: #909399;">
                  기본구성품 {{ row.basicComponentCount }}개
                </div>
                <span v-else>-</span>
              </div>
            </template>
          </el-table-column>
          <el-table-column
            label="가격"
            align="right"
            width="130"
            header-align="center"
            :excel-formatter="(row) => `공장: ${formatPrice(row.factoryPrice)}\n수공: ${formatPrice(row.laborCost)}`"
          >
            <template #default="{row}">
              <div style="font-size: 0.9rem; color: #E6A23C; font-weight: 500;">
                <span style="font-size: 0.75rem; color: #909399; margin-right: 4px;">공장</span>
                {{ formatPrice(row.factoryPrice) }}
              </div>
              <div style="font-size: 0.9rem; color: #67C23A; font-weight: 500; margin-top: 2px;">
                <span style="font-size: 0.75rem; color: #909399; margin-right: 4px;">수공</span>
                {{ formatPrice(row.laborCost) }}
              </div>
            </template>
          </el-table-column>
          <el-table-column
            label="공개여부"
            width="100"
            align="center"
            header-align="center"
            :excel-formatter="(row) => row.isPublic ? '공개' : '비공개'"
          >
            <template #default="{row}">
              <el-tag :type="row.isPublic ? 'success' : 'info'">
                {{ row.isPublic ? '공개' : '비공개' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column
            label="등록일"
            align="center"
            width="120"
            header-align="center"
            :excel-formatter="(row) => formatDateTime(row.createdAt)"
          >
            <template #default="{row}">
              <span>{{ formatDateTime(row.createdAt) }}</span>
            </template>
          </el-table-column>
          <el-table-column label="관리" align="center" width="120" header-align="center" class-name="small-padding fixed-width" :fixed="!isMobile ? 'right' : false">
            <template #default="{row}">
              <el-tooltip content="상세보기" placement="top" :enterable="false">
                <el-button link class="view-icon-btn" :icon="View" @click="handleDetail(row)" />
              </el-tooltip>
              <el-tooltip content="수정" placement="top" :enterable="false">
                <el-button link class="edit-icon-btn" :icon="Edit" @click="handleUpdate(row)" />
              </el-tooltip>
              <el-tooltip content="삭제" placement="top" :enterable="false">
                <el-button link class="delete-icon-btn" :icon="Delete" @click="handleDelete(row)" />
              </el-tooltip>
            </template>
          </el-table-column>
        </base-table>
      </el-card>

      <product-set-dialog
        v-model="dialogFormVisible"
        :id="currentId"
        @success="getList"
      />
    </template>
    <el-card v-else shadow="never" class="empty-card" style="min-height: 500px; display: flex; align-items: center; justify-content: center;">
      <el-empty :description="$t('apiErrors.forbidden')" />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { useMobile } from '@/hooks/useMobile';
import { ref, onMounted, reactive, computed } from 'vue';
import { useRouter } from 'vue-router';
import { getAdminProductSets, deleteProductSet } from '@/api/product-set';
import { ElMessageBox, ElMessage } from 'element-plus';
import { Delete, Edit, View } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import useCodeStore from '@/store/modules/code';
import ProductSetDialog from '@/components/ProductSetDialog/index.vue';
import ProductSetFilter from './components/ProductSetFilter.vue';
import ProductSetComponentPopover from './components/ProductSetComponentPopover.vue';
import useUserStore from '@/store/modules/user';
const { isMobile } = useMobile();

const router = useRouter();
const userStore = useUserStore();
const codeStore = useCodeStore();

const isCompanyUser = computed(() => !userStore.roles.includes('admin'));
const isUnassignedUser = computed(() => isCompanyUser.value && !userStore.companyId);

const list = ref([]);
const total = ref(0);
const listLoading = ref(true);
const allCodesMap = computed(() => codeStore.codeMap);

const listQuery = reactive({
  page: 1,
  pageSize: 20,
  title: '',
  categoryLarge: '',
  categoryMedium: '',
  categorySmall: '',
  companyId: undefined,
  isPublic: undefined
});

const dialogFormVisible = ref(false);
const currentId = ref<number | null>(null);

const getList = async () => {
  listLoading.value = true;
  try {
    await codeStore.fetchCodes();
    const response = await getAdminProductSets(listQuery);
    list.value = response.data.items;
    total.value = response.data.totalCount;
  } catch (error) {
    console.error(error);
  } finally {
    listLoading.value = false;
  }
};

const handleFilter = () => {
  listQuery.page = 1;
  getList();
};

const resetQuery = () => {
  Object.assign(listQuery, {
    page: 1,
    pageSize: 20,
    title: '',
    categoryLarge: '',
    categoryMedium: '',
    categorySmall: '',
    companyId: isCompanyUser.value ? userStore.companyId || undefined : undefined,
    isPublic: undefined
  });
  getList();
};

const getCategoryName = (row: any) => {
  const parts = getCategoryParts(row);
  return parts.join(' > ');
};

const getCategoryParts = (row: any) => {
  const large = allCodesMap.value[row.categoryLarge] || row.categoryLarge;
  const medium = allCodesMap.value[row.categoryMedium] || row.categoryMedium;
  const small = allCodesMap.value[row.categorySmall] || row.categorySmall;

  return [large, medium, small].filter(p => !!p);
};

const getSetMainPhoto = (row: any) => {
  if (row.photos && row.photos.length > 0) {
    return row.photos[0].photoUrl;
  }

  if (row.products && row.products.length > 0) {
    const firstProduct = row.products[0];
    if (firstProduct.photos && firstProduct.photos.length > 0) {
      return firstProduct.photos[0].photoUrl;
    }
  }

  return null;
};

const getSetPhotoList = (row: any) => {
  if (row.photos && row.photos.length > 0) {
    return row.photos.map((p: any) => p.photoUrl);
  }

  if (row.products && row.products.length > 0) {
    const firstProduct = row.products[0];
    if (firstProduct.photos && firstProduct.photos.length > 0) {
      return firstProduct.photos.map((p: any) => p.photoUrl);
    }
  }

  return [];
};

const formatDateTime = (date: string) => {
  if (!date) return '-';
  return parseTime(date, '{y}-{m}-{d}');
};

const handleCreate = () => {
  currentId.value = null;
  dialogFormVisible.value = true;
};

const handleUpdate = async (row: any) => {
  currentId.value = row.id;
  dialogFormVisible.value = true;
};

const handleDelete = (row: any) => {
  ElMessageBox.confirm('정말로 이 세트 정보를 삭제하시겠습니까?', '경고', {
    confirmButtonText: '삭제',
    cancelButtonText: '취소',
    type: 'warning'
  }).then(async () => {
    try {
      await deleteProductSet(row.id);
      ElMessage.success('삭제되었습니다.');
      getList();
    } catch (error) {
      console.error(error);
    }
  });
};

const handleDetail = (row: any) => {
  router.push(`/prd/product-set-detail/${row.id}`);
};

onMounted(() => {
  if (isCompanyUser.value) {
    listQuery.companyId = userStore.companyId || undefined;
  }
  getList();
});
</script>

<style lang="scss" scoped>
@import "./ProductSetStyles.scss";
</style>

