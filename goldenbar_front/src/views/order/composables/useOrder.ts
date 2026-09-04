import { ref, reactive } from 'vue';
import { getMyOrders, updateOrderStatus } from '@/api/order';
import { ElMessage } from 'element-plus';
import { parseTime } from '@/utils';

export function useOrder() {
  const orders = ref<any[]>([]);
  const totalCount = ref(0);
  const loading = ref(false);

  const end = new Date();
  const start = new Date();
  start.setTime(start.getTime() - 3600 * 1000 * 24 * 30);
  const defaultStartDate = parseTime(start, '{y}-{m}-{d}');
  const defaultEndDate = parseTime(end, '{y}-{m}-{d}');

  const listQuery = reactive({
    page: 1,
    pageSize: 10,
    status: '',
    startDate: defaultStartDate as string | undefined,
    endDate: defaultEndDate as string | undefined,
    excludeCancelled: true,
    excludeCompleted: true,
    excludeSettled: true,
    orderNo: '',
    categoryLarge: '',
    categoryMedium: '',
    categorySmall: '',
    setCategoryLarge: '',
    setCategoryMedium: '',
    setCategorySmall: '',
    sortBy: 'CreatedAt',
    isDescending: true
  });

  const getList = async () => {
    loading.value = true;
    try {
      const { data } = await getMyOrders(listQuery);
      orders.value = data.items;
      totalCount.value = data.totalCount;
    } catch (error) {
      console.error('주문 목록 조회 실패:', error);
      ElMessage.error('주문 목록을 불러오는데 실패했습니다.');
    } finally {
      loading.value = false;
    }
  };

  const handleFilter = () => {
    listQuery.page = 1;
    getList();
  };

  const handleStatusChange = (status: string) => {
    listQuery.status = status;
    handleFilter();
  };

  const updateStatus = async (orderId: number, statusData: any) => {
    try {
      await updateOrderStatus(orderId, statusData);
      ElMessage.success('상태가 변경되었습니다.');
      getList();
      return true;
    } catch (error: any) {
      console.error('상태 변경 실패:', error);
      ElMessage.error(error.message || '상태 변경에 실패했습니다.');
      return false;
    }
  };

  return {
    orders,
    totalCount,
    loading,
    listQuery,
    getList,
    handleFilter,
    handleStatusChange,
    updateStatus
  };
}
