import { ref, reactive } from 'vue';
import { getProducts } from '@/api/product';
import { uploadImage } from '@/api/file';
import { ElMessage } from 'element-plus';

export function useCatalogPageEditor(temp: any) {

  const productSelectorVisible = ref(false);
  const productLoading = ref(false);
  const productList = ref([]);
  const productTotal = ref(0);
  const productQuery = reactive({ page: 1, pageSize: 10, name: '' });

  const bulkEditorVisible = ref(false);
  const bulkLoading = ref(false);
  const bulkFileList = ref<any[]>([]);
  const bulkLargeId = ref<number | null>(null);
  const bulkMediumId = ref<number | null>(null);
  const bulkTemp = ref<any>({
    description: '', companyId: null, companyName: '',
    categoryLarge: '', categoryMedium: '', categorySmall: ''
  });

  const pageEditorVisible = ref(false);
  const pageEditIndex = ref<number | null>(null);
  const pageLargeId = ref<number | null>(null);
  const pageMediumId = ref<number | null>(null);
  const pageTemp = ref<any>({
    id: undefined, pageNumber: 1, photoUrl: '', description: '',
    companyId: null, companyName: '', categoryLarge: '', categoryMedium: '',
    categorySmall: '', connectedProducts: []
  });

  const getProductList = async () => {
    productLoading.value = true;
    try {
      const response = await getProducts(productQuery);
      productList.value = response.data.items;
      productTotal.value = response.data.totalCount;
    } catch (error) {
      console.error(error);
    } finally {
      productLoading.value = false;
    }
  };

  const openProductSelector = () => {
    productQuery.page = 1;
    productQuery.name = '';
    getProductList();
    productSelectorVisible.value = true;
  };

  const addSelectedProducts = (selected: any[]) => {
    selected.forEach((p: any) => {
      if (!pageTemp.value.connectedProducts) pageTemp.value.connectedProducts = [];
      if (!pageTemp.value.connectedProducts.find((cp: any) => cp.id === p.id)) {
        pageTemp.value.connectedProducts.push({ id: p.id, name: p.name, productNo: p.productNo });
      }
    });
    productSelectorVisible.value = false;
  };

  const removeProduct = (index: number) => {
    pageTemp.value.connectedProducts.splice(index, 1);
  };

  const openBulkPageEditor = () => {
    bulkFileList.value = [];
    bulkLargeId.value = null;
    bulkMediumId.value = null;
    bulkTemp.value = {
      description: '', companyId: null, companyName: '',
      categoryLarge: '', categoryMedium: '', categorySmall: ''
    };
    bulkEditorVisible.value = true;
  };

  const saveBulkPageData = async () => {
    if (bulkFileList.value.length === 0) {
      ElMessage.warning('업로드할 사진을 선택해주세요.');
      return;
    }
    bulkLoading.value = true;
    try {
      const startPageNumber = (temp.value.pages?.length > 0)
        ? Math.max(...temp.value.pages.map((p: any) => p.pageNumber)) + 1
        : 1;
      const uploadPromises = bulkFileList.value.map(async (fileItem, index) => {
        const formData = new FormData();
        formData.append('file', fileItem.raw);
        formData.append('subDir', 'catalogs');
        const res = await uploadImage(formData);
        return { ...bulkTemp.value, pageNumber: startPageNumber + index, photoUrl: res.data.filePath, connectedProducts: [] };
      });
      const newPages = await Promise.all(uploadPromises);
      if (!temp.value.pages) temp.value.pages = [];
      temp.value.pages.push(...newPages);
      temp.value.pages.sort((a: any, b: any) => a.pageNumber - b.pageNumber);
      ElMessage.success(`${newPages.length}개의 페이지가 추가되었습니다.`);
      bulkEditorVisible.value = false;
    } catch (error) {
      console.error('Bulk upload error:', error);
      ElMessage.error('대량 업로드 중 오류가 발생했습니다.');
    } finally {
      bulkLoading.value = false;
    }
  };

  const openPageEditor = (row: any | null, index: number | null = null) => {
    pageLargeId.value = null;
    pageMediumId.value = null;
    pageEditIndex.value = index;
    if (row) {
      pageTemp.value = { ...row, connectedProducts: row.products || row.connectedProducts || [] };
    } else {
      pageTemp.value = {
        id: undefined, pageNumber: (temp.value.pages?.length || 0) + 1,
        photoUrl: '', description: '', companyId: null, companyName: '',
        categoryLarge: '', categoryMedium: '', categorySmall: '', connectedProducts: []
      };
    }
    pageEditorVisible.value = true;
  };

  const applyPageTemp = () => {
    if (pageTemp.value.connectedProducts) {
      pageTemp.value.productIds = pageTemp.value.connectedProducts.map((p: any) => p.id);
    } else {
      pageTemp.value.productIds = [];
    }
    if (!temp.value.pages) temp.value.pages = [];
    if (pageEditIndex.value === null) {
      temp.value.pages.push({ ...pageTemp.value });
    } else {
      temp.value.pages[pageEditIndex.value] = { ...pageTemp.value };
    }
    temp.value.pages.sort((a: any, b: any) => a.pageNumber - b.pageNumber);
  };

  const savePageData = () => {
    if (!pageTemp.value.pageNumber) { ElMessage.warning('페이지 번호를 입력해주세요.'); return; }
    applyPageTemp();
    pageEditorVisible.value = false;
  };

  const saveAndContinuePageData = () => {
    if (!pageTemp.value.pageNumber) { ElMessage.warning('페이지 번호를 입력해주세요.'); return; }
    applyPageTemp();
    ElMessage.success(`${pageTemp.value.pageNumber}페이지가 저장되었습니다.`);
    const nextNumber = pageTemp.value.pageNumber + 1;
    pageTemp.value = {
      id: undefined, pageNumber: nextNumber, photoUrl: '', description: '',
      companyId: null, companyName: '', categoryLarge: '', categoryMedium: '',
      categorySmall: '', connectedProducts: []
    };
    pageLargeId.value = null;
    pageMediumId.value = null;
    pageEditIndex.value = null;
  };

  const removePage = (index: number) => { temp.value.pages.splice(index, 1); };

  const handleUpdatePagesOrder = ({ oldIndex, newIndex }: { oldIndex: number, newIndex: number }) => {
    const target = temp.value.pages.splice(oldIndex, 1)[0];
    temp.value.pages.splice(newIndex, 0, target);
    temp.value.pages.forEach((page: any, idx: number) => { page.pageNumber = idx + 1; });
  };

  return {

    productSelectorVisible, productLoading, productList, productTotal, productQuery,
    openProductSelector, addSelectedProducts, removeProduct, getProductList,

    bulkEditorVisible, bulkLoading, bulkFileList, bulkLargeId, bulkMediumId, bulkTemp,
    openBulkPageEditor, saveBulkPageData,

    pageEditorVisible, pageEditIndex, pageLargeId, pageMediumId, pageTemp,
    openPageEditor, savePageData, saveAndContinuePageData, removePage, handleUpdatePagesOrder
  };
}
