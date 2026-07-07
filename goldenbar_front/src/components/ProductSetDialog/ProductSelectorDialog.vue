<template>
<base-popup
    draggable
    v-model="visible"
    title="세트에 포함할 제품 선택"
    width="1050px"
    append-to-body
    @close="handleClose"
  >
    <div class="filter-container">
      <el-input
        v-model="productQuery.name"
        placeholder="제품명 검색"
        style="width: 200px;"
        @keyup.enter="getProductList"
      />
      <el-button type="primary" style="margin-left: 0.625rem;" @click="getProductList">검색</el-button>
    </div>
    <base-table
      v-loading="productLoading"
      :data="productList"
      style="width: 100%; margin-top: 0.625rem;"
      :total="productTotal"
      v-model:page="productQuery.page"
      v-model:page-size="productQuery.pageSize"
      :page-sizes="[5, 10, 20, 50]"
      @change="getProductList"
      @selection-change="handleProductSelectionChange"
    >
      <el-table-column type="selection" width="55" />
      <el-table-column label="대표사진" width="80" align="center">
        <template #default="{ row }">
          <el-image
            v-if="row.photos && row.photos.length > 0"
            :src="row.photos.find((p: any) => p.isMain)?.photoUrl || row.photos[0].photoUrl"
            fit="cover"
            style="width: 52px; height: 52px; border-radius: 3px; border: 1px solid #eee;"
            :preview-src-list="[row.photos.find((p: any) => p.isMain)?.photoUrl || row.photos[0].photoUrl]"
            preview-teleported
          />
          <div v-else style="width: 52px; height: 52px; background: #f5f5f5; border-radius: 3px; display: flex; align-items: center; justify-content: center; font-size: 0.825rem; color: #bbb; border: 1px solid #eee;">NO IMG</div>
        </template>
      </el-table-column>
      <el-table-column prop="productNo" label="제품번호" width="110" />
      <el-table-column prop="name" label="제품명" min-width="140" />
      <el-table-column label="제조사" width="110" prop="companyName" />
      <el-table-column
        label="함량"
        width="130"
        :excel-formatter="(row: any) => row.purity ? row.purity.split(',').map((c: string) => allCodesMap[c.trim()] || c.trim()).join(', ') : '-'"
      >
        <template #default="{ row }">
          <template v-if="row.purity">
            <el-tag
              v-for="code in row.purity.split(',')"
              :key="code"
              size="small"
              type="warning"
              style="margin: 0.0625rem 0.125rem; font-size: 0.825rem;"
            >
              {{ allCodesMap[code.trim()] || code.trim() }}
            </el-tag>
          </template>
          <span v-else style="color: #ccc; font-size: 0.95rem;">-</span>
        </template>
      </el-table-column>
      <el-table-column
        label="컬러"
        width="130"
        :excel-formatter="(row: any) => row.colors ? row.colors.split(',').map((c: string) => allCodesMap[c.trim()] || c.trim()).join(', ') : '-'"
      >
        <template #default="{ row }">
          <template v-if="row.colors">
            <el-tag
              v-for="code in row.colors.split(',')"
              :key="code"
              size="small"
              style="margin: 0.0625rem 0.125rem; font-size: 0.825rem;"
            >
              {{ allCodesMap[code.trim()] || code.trim() }}
            </el-tag>
          </template>
          <span v-else style="color: #ccc; font-size: 0.95rem;">-</span>
        </template>
      </el-table-column>
    </base-table>
    <template #footer>
      <el-button @click="handleClose">취소</el-button>
      <el-button type="primary" @click="addSelectedProducts">선택 추가</el-button>
    </template>
  </base-popup>
</template>

<script setup lang="ts">

import { ref, reactive, watch, computed } from 'vue';
import { getAdminProducts } from '@/api/product';
import useCodeStore from '@/store/modules/code';
import useUserStore from '@/store/modules/user';
import BasePopup from '@/components/BasePopup/index.vue';
import BaseTable from '@/components/BaseTable/index.vue';

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits(['update:modelValue', 'select']);

const userStore = useUserStore();
const codeStore = useCodeStore();
const isCompanyUser = computed(() => !userStore.roles.includes('admin'));

const visible = ref(false);
const productLoading = ref(false);
const productList = ref([]);
const productTotal = ref(0);

const productQuery = reactive({
  page: 1,
  pageSize: 5,
  name: '',
  companyId: undefined as number | undefined
});

const selectedProducts = ref<any[]>([]);

const allCodesMap = computed(() => codeStore.codeMap);

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val) {
    productQuery.page = 1;
    productQuery.name = '';
    productQuery.companyId = isCompanyUser.value ? userStore.companyId || undefined : undefined;
    getProductList();
  }
});

watch(() => visible.value, (val) => {
  emit('update:modelValue', val);
});

const handleClose = () => {
  visible.value = false;
};

const getProductList = async () => {
  productLoading.value = true;
  try {
    const response = await getAdminProducts(productQuery);
    productList.value = response.data.items;
    productTotal.value = response.data.totalCount;
  } catch (error) {
    console.error(error);
  } finally {
    productLoading.value = false;
  }
};

const handleProductSelectionChange = (val: any) => {
  selectedProducts.value = val;
};

const addSelectedProducts = () => {
  emit('select', selectedProducts.value);
  visible.value = false;
};
</script>

<style scoped>
.filter-container {
  padding-bottom: 0.625rem;
}
</style>

