import { ref, reactive, computed } from 'vue';
import { integratedSearch } from '@/api/search';
import { addToCart, getCart } from '@/api/cart';
import { addFavorite, removeFavorite, getMyFavorites } from '@/api/favorite';
import { ElMessage } from 'element-plus';
import { useI18n } from 'vue-i18n';
import useCodeStore from '@/store/modules/code';

export function useMarket() {
  const { t } = useI18n();
  const codeStore = useCodeStore();

  const loading = ref(false);
  const displayList = ref<any[]>([]);
  const total = ref(0);
  const activeTab = ref('all');

  const filters = reactive({
    page: 1,
    pageSize: 24,
    search: '',
    categoryLarge: '',
    categoryMedium: '',
    categorySmall: '',
    companyId: null,
    sortBy: 'latest'
  });

  const cartItems = ref<any[]>([]);
  const favorites = ref<any[]>([]);

  const fetchData = async () => {
    loading.value = true;
    try {

      const res = await (integratedSearch as any)({
        ...filters,
        type: activeTab.value === 'all' ? undefined : activeTab.value
      });
      displayList.value = res.data.items || [];
      total.value = res.data.total || 0;
    } catch (error) {
      console.error('마켓 데이터 로드 실패:', error);
      ElMessage.error(t('productMarket.messages.loadFail'));
    } finally {
      loading.value = false;
    }
  };

  const fetchCart = async () => {
    try {
      const res = await getCart();
      cartItems.value = res.data.items || [];
    } catch (error) {
      console.error('장바구니 로드 실패:', error);
    }
  };

  const fetchFavorites = async () => {
    try {
      const res = await getMyFavorites();
      favorites.value = res.data || [];
    } catch (error) {
      console.error('관심상품 로드 실패:', error);
    }
  };

  const handleTabChange = () => {
    filters.page = 1;
    fetchData();
  };

  const handleFilter = () => {
    filters.page = 1;
    fetchData();
  };

  const resetFilters = () => {
    filters.search = '';
    filters.categoryLarge = '';
    filters.categoryMedium = '';
    filters.categorySmall = '';
    filters.companyId = null;
    handleFilter();
  };

  const handleAddToCart = async (item: any) => {
    try {
      await addToCart({
        productId: item.isSet ? null : item.id,
        productSetId: item.isSet ? item.id : null,
        quantity: 1
      });
      ElMessage.success(t('productMarket.messages.cartAddSuccess'));
      fetchCart();
    } catch (error) {
      console.error('장바구니 추가 실패:', error);
      ElMessage.error(t('productMarket.messages.cartAddFail'));
    }
  };

  const toggleFavorite = async (item: any) => {
    const favorite = favorites.value.find(f =>
      (item.isSet && f.productSetId === item.id) || (!item.isSet && f.productId === item.id)
    );

    try {
      if (favorite) {
        await removeFavorite(favorite.id);
        ElMessage.success(t('productMarket.messages.favoriteRemove'));
      } else {
        await addFavorite({
          productId: item.isSet ? null : item.id,
          productSetId: item.isSet ? item.id : null
        });
        ElMessage.success(t('productMarket.messages.favoriteAdd'));
      }
      fetchFavorites();
    } catch (error) {
      console.error('관심상품 처리 실패:', error);
    }
  };

  const isFavorite = (item: any) => {
    return favorites.value.some(f =>
      (item.isSet && f.productSetId === item.id) || (!item.isSet && f.productId === item.id)
    );
  };

  const isInCart = (item: any) => {
    return cartItems.value.some(c =>
      (item.isSet && c.productSetId === item.id) || (!item.isSet && c.productId === item.id)
    );
  };

  return {
    loading,
    displayList,
    total,
    activeTab,
    filters,
    fetchData,
    fetchCart,
    fetchFavorites,
    favorites,
    handleTabChange,
    handleFilter,
    resetFilters,
    handleAddToCart,
    toggleFavorite,
    isFavorite,
    isInCart
  };
}
