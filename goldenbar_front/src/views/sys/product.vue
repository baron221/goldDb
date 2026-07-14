<template>
<div class="app-container">
    <template v-if="!isUnassignedUser">
      <el-card shadow="never">

        <product-search-filter
          :query="listQuery"
          :is-company-user="isCompanyUser"
          @filter="handleFilter"
          @reset="resetQuery"
          @create="handleCreate"
        />

        <base-table
          :loading="listLoading"
          :data="list"
          :total="total"
          v-model:page="listQuery.page"
          v-model:page-size="listQuery.pageSize"
          @row-dblclick="handleDetail"
          @change="getList"
        >
          <el-table-column :label="$t('orderDetail.headers.photo')" prop="photoUrl" width="80" align="center" header-align="center" class-name="photo-column" :sortable="false">
            <template #default="{row}">
              <el-image
                v-if="getProductPhotoSmall(row)"
                :src="getProductPhotoSmall(row)"
                fit="contain"
                style="width: 80px; height: 80px; display: block; vertical-align: middle; background-color: #fff;"
                :preview-src-list="row.photos ? row.photos.map((p: any) => p.photoUrl) : []"
                preview-teleported
              />
              <div v-else style="width: 80px; height: 80px; display: flex; align-items: center; justify-content: center; background-color: #f5f7fa; color: #c0c4cc;">
                <el-icon :size="20"><Picture /></el-icon>
              </div>
            </template>
          </el-table-column>
          <el-table-column
            :label="$t('sys.product.headers.productInfo')"
            prop="productNo"
            align="left"
            header-align="center"
            :excel-formatter="(row) => `[${row.productNo || '-'}] ${row.name || '-'} (${row.companyName || '-'})`"
          >
            <template #default="{row}">
              <span>{{ row.productNo || '-' }}</span>
              <br />
              <span>{{ row.name || '-' }}</span>
              <br />
              <span>{{ row.companyName || '-' }}</span>
            </template>
          </el-table-column>

          <el-table-column
            :label="$t('sys.product.headers.purityColor')"
            min-width="180"
            align="left"
            header-align="center"
            :excel-formatter="formatPurityColorExcel"
          >
            <template #default="{row}">
              <div v-if="row.optionWeights && row.optionWeights.length > 0" class="option-weights-container">
                <div v-for="ow in row.optionWeights" :key="ow.id" class="option-weight-row">
                  <el-tag size="small" type="warning" effect="plain" class="mini-tag">{{ allCodesMap[ow.purity] || ow.purity }}</el-tag>
                  <el-tag v-if="ow.color != 'EMPTY'" size="small" type="info" effect="plain" class="mini-tag">{{ getColorName(ow.color) }}</el-tag>
                  <span class="weight-text">{{ ow.weight.toFixed(2) }}g</span>
                </div>
              </div>
              <div v-else class="center-tags">
                <div v-if="row.purity && row.purity != 'EMPTY'">
                  <el-tag
                    v-for="pCode in row.purity.split(',')"
                    :key="pCode"
                    size="small"
                    type="warning"
                    style="margin-right: 0.125rem; margin-bottom: 0.125rem"
                  >
                    {{ allCodesMap[pCode] || pCode }}
                  </el-tag>
                </div>

                <div v-if="row.colors && row.colors != 'EMPTY' ">
                  <el-tag
                    v-for="colorCode in row.colors.split(',')"
                    :key="colorCode"
                    size="small"
                    style="margin-right: 0.125rem; margin-bottom: 0.125rem"
                  >
                    {{ getColorName(colorCode) }}
                  </el-tag>
                </div>
              </div>
            </template>
          </el-table-column>
          <el-table-column :label="$t('sys.product.headers.baseWeight')" prop="weight" width="100" align="right" header-align="center" />
          <el-table-column
            :label="$t('productMarket.labels.largeCategory')"
            width="100"
            align="center"
            header-align="center"
            :excel-formatter="(row) => allCodesMap[row.categoryLarge] || row.categoryLarge || '-'"
          >
            <template #default="{row}">
              <span>{{ allCodesMap[row.categoryLarge] || row.categoryLarge || '-' }}</span>
            </template>
          </el-table-column>
          <el-table-column
            :label="$t('productMarket.labels.mediumCategory')"
            width="100"
            align="center"
            header-align="center"
            :excel-formatter="(row) => allCodesMap[row.categoryMedium] || row.categoryMedium || '-'"
          >
            <template #default="{row}">
              <span>{{ allCodesMap[row.categoryMedium] || row.categoryMedium || '-' }}</span>
            </template>
          </el-table-column>
          <el-table-column
            :label="$t('productMarket.labels.smallCategory')"
            width="100"
            align="center"
            header-align="center"
            :excel-formatter="(row) => allCodesMap[row.categorySmall] || row.categorySmall || '-'"
          >
            <template #default="{row}">
              <span>{{ allCodesMap[row.categorySmall] || row.categorySmall || '-' }}</span>
            </template>
          </el-table-column>
          <el-table-column
            :label="$t('sys.product.headers.factoryPrice')"
            align="right"
            width="120"
            header-align="center"
            :excel-formatter="(row) => `공장가: ${formatPrice(row.factoryPrice)}, 수공비: ${formatPrice(row.laborCost)}`"
          >
            <template #default="{row}">
              <span>{{ formatPrice(row.factoryPrice) }}</span>
              <br />
              <span>{{ formatPrice(row.laborCost) }}</span>
            </template>
          </el-table-column>
          <el-table-column
            :label="$t('sys.product.headers.public')"
            width="100"
            align="center"
            header-align="center"
            :excel-formatter="(row) => row.isPublic ? $t('sys.product.headers.public') : '비공개'"
          >
            <template #default="{row}">
              <el-tag v-if="row.isPublic" type="success">{{ $t('sys.product.headers.public') }}</el-tag>
              <span v-else style="color: #909399;">비공개</span>
            </template>
          </el-table-column>
          <el-table-column :label="$t('common.action')" align="center" width="150" header-align="center" class-name="small-padding fixed-width" :fixed="!isMobile ? 'right' : false" :sortable="false">
            <template #default="{row}">
              <template v-if="!isCompanyUser || isMfgUser">
                <el-tooltip :content="$t('stock.table.action')" placement="top" :enterable="false">
                  <el-button link class="view-icon-btn" :icon="View" @click="handleDetail(row)" />
                </el-tooltip>
                <el-tooltip :content="$t('common.edit')" placement="top" :enterable="false">
                  <el-button link class="edit-icon-btn" :icon="Edit" @click="handleUpdate(row)" />
                </el-tooltip>
                <el-tooltip :content="$t('common.delete')" placement="top" :enterable="false">
                  <el-button link class="delete-icon-btn" :icon="Delete" @click="handleDelete(row)" />
                </el-tooltip>
              </template>
              <template v-else>
                <el-button type="primary" size="small" plain @click="handleDetail(row)">
                  {{ $t('stock.table.action') }}
                </el-button>
              </template>
            </template>
          </el-table-column>
        </base-table>

      </el-card>

      <ProductDialog
        v-model="dialogFormVisible"
        :product-id="currentProductId"
        @saved="getList"
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
import { useI18n } from 'vue-i18n';
import useUserStore from '@/store/modules/user';
import { getAdminProducts, deleteProduct } from '@/api/product';
import { ElMessage, ElMessageBox } from 'element-plus';
import { Edit, Delete, View, Picture } from '@element-plus/icons-vue';
import ProductDialog from '@/components/ProductDialog/index.vue';
import BaseTable from '@/components/BaseTable/index.vue';
import { getThumbnailUrl } from '@/utils';
import { formatPrice } from '@/utils/format';
import useCodeStore from '@/store/modules/code';
import ProductSearchFilter from './components/ProductSearchFilter.vue';

const { isMobile } = useMobile();
const { t } = useI18n();
const router = useRouter();
const userStore = useUserStore();
const codeStore = useCodeStore();

const isCompanyUser = computed(() => !userStore.roles.includes('admin'));
const isMfgUser = computed(() => userStore.companyType === 'MFG');
const isUnassignedUser = computed(() => isCompanyUser.value && !userStore.companyId);

const list = ref([]);
const total = ref(0);
const listLoading = ref(true);
const productColors = ref<any[]>([]);
const allCodesMap = computed(() => codeStore.codeMap);

onMounted(() => {
  if (isCompanyUser.value) {
    listQuery.companyId = userStore.companyId || undefined;
  }
  getList();
  codeStore.fetchCodes().then(() => {
    productColors.value = codeStore.getCodesByGroupStore('PRODUCT_COLOR');
  });
});

const listQuery = reactive({
  page: 1,
  pageSize: 20,
  name: '',
  productNo: '',
  minWeight: undefined,
  maxWeight: undefined,
  categoryLarge: '',
  categoryMedium: '',
  categorySmall: '',
  companyId: undefined,
  isPublic: undefined
});

const dialogFormVisible = ref(false);
const currentProductId = ref<number | null>(null);

const getList = async () => {
  listLoading.value = true;
  try {
    const response = await getAdminProducts(listQuery);
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
    name: '',
    productNo: '',
    minWeight: undefined,
    maxWeight: undefined,
    categoryLarge: '',
    categoryMedium: '',
    categorySmall: '',
    companyId: isCompanyUser.value ? userStore.companyId || undefined : undefined,
    isPublic: undefined
  });
  getList();
};

const getColorName = (code: string) => {
  const color = productColors.value.find(c => c.code === code);
  return color ? color.name : (allCodesMap.value[code] || code);
};

const formatPurityColorExcel = (row: any) => {
  if (row.optionWeights && row.optionWeights.length > 0) {
    return row.optionWeights.map((ow: any) =>
      `${allCodesMap.value[ow.purity] || ow.purity} / ${getColorName(ow.color)} (${ow.weight.toFixed(2)}g)`
    ).join(', ');
  } else {
    const purities = row.purity ? row.purity.split(',').map((p: string) => allCodesMap.value[p.trim()] || p.trim()).join('/') : '';
    const colors = row.colors ? row.colors.split(',').map((c: string) => getColorName(c.trim())).join('/') : '';
    return [purities, colors].filter(Boolean).join(' - ');
  }
};

const getProductPhotoSmall = (row: any) => {
  if (!row.photos || row.photos.length === 0) return '';
  const mainPhoto = row.photos.find((p: any) => p.isMain) || row.photos[0];
  if (!mainPhoto || !mainPhoto.photoUrl) return '';

  return getThumbnailUrl(mainPhoto.photoUrl, 'small');
};

const handleCreate = () => {
  currentProductId.value = null;
  dialogFormVisible.value = true;
};

const handleUpdate = (row: any) => {
  currentProductId.value = row.id;
  dialogFormVisible.value = true;
};

const handleDetail = (row: any) => {
  router.push(`/prd/product-detail/${row.id}`);
};

const handleDelete = (row: any) => {
  ElMessageBox.confirm(t('favorite.messages.confirmRemove'), t('favorite.messages.confirmTitle'), {
    confirmButtonText: t('common.delete'),
    cancelButtonText: t('common.cancel'),
    type: 'warning'
  }).then(async () => {
    try {
      await deleteProduct(row.id);
      ElMessage.success(t('favorite.messages.removeSuccess'));
      getList();
    } catch (error) {
      console.error(error);
    }
  });
};
</script>

<style lang="scss" scoped>
@import "./SystemProductStyles.scss";
</style>

