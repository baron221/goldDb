import { ref, reactive } from 'vue';
import { getUsers, getUserDetail, deleteUser, createUser, updateUser } from '@/api/user';
import { getRoles } from '@/api/role';
import { getCompanies } from '@/api/company';
import { ElMessage, ElMessageBox } from 'element-plus';
import { useI18n } from 'vue-i18n';

export function useUser() {
  const { t } = useI18n();
  const userList = ref<any[]>([]);
  const currentUser = ref<any>(null);
  const loading = ref(false);
  const totalCount = ref(0);

  const listQuery = reactive({
    companyType: '',
    searchText: '',
    isUnassignedOnly: false,
    isLogisticsUnassigned: false,
    page: 1,
    pageSize: 20
  });

  const allRoles = ref<any[]>([]);
  const allCompanies = ref<any[]>([]);

  const fetchUsers = async () => {
    loading.value = true;
    try {

      const res = await getUsers(listQuery);
      userList.value = res.data || [];
      totalCount.value = res.data?.length || 0;
    } catch (error) {
      console.error('사용자 목록 조회 실패:', error);
      ElMessage.error(t('userManage.loadFail'));
    } finally {
      loading.value = false;
    }
  };

  const handleFilter = () => {
    listQuery.page = 1;
    fetchUsers();
  };

  const fetchUserDetail = async (id: number) => {
    try {
      const res = await getUserDetail(id);
      return res.data;
    } catch (error) {
      console.error('사용자 상세 조회 실패:', error);
      ElMessage.error(t('userManage.loadFail'));
      return null;
    }
  };

  const removeUser = async (id: number) => {
    try {
      await ElMessageBox.confirm(t('userManage.confirmDelete'), t('common.delete'), {
        confirmButtonText: t('common.ok'),
        cancelButtonText: t('common.cancel'),
        type: 'warning'
      });
      await deleteUser(id);
      ElMessage.success(t('common.delete'));
      if (currentUser.value?.id === id) {
        currentUser.value = null;
      }
      fetchUsers();
      return true;
    } catch (error) {
      if (error !== 'cancel') {
        console.error('사용자 삭제 실패:', error);
      }
      return false;
    }
  };

  const fetchInitialData = async () => {
    try {
      const [rolesRes, companiesRes] = await Promise.all([
        getRoles(),
        getCompanies({ pageSize: 1000 })
      ]);
      allRoles.value = rolesRes.data || [];
      allCompanies.value = companiesRes.data.items || [];
    } catch (error) {
      console.error('초기 데이터 로드 실패:', error);
    }
  };

  return {
    userList,
    currentUser,
    loading,
    totalCount,
    listQuery,
    allRoles,
    allCompanies,
    fetchUsers,
    handleFilter,
    fetchUserDetail,
    removeUser,
    fetchInitialData
  };
}
