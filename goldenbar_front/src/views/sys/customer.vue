<template>
<div class="app-container">
    <div class="split-container" ref="splitContainer">

      <div class="left-pane" :style="{ width: leftWidth + 'px' }">
        <customer-list-pane
          v-model="listQuery"
          :customer-list="customerList"
          :companies="companies"
          :total="total"
          :is-retailer="isRetailer"
          @row-click="handleRowClick"
          @create="handleCreate"
          @delete="handleDelete"
          @refresh="fetchCustomers"
        />
      </div>

      <div class="pane-resizer" @mousedown="startResize"></div>

      <div class="right-pane">
        <customer-detail-pane
          v-if="currentCustomer"
          v-model:active-tab="activeTab"
          :current-customer="currentCustomer"
          :is-new-mode="isNewMode"
          :customer-detail="customerDetail"
          @success="onCustomerSaveSuccess"
        />

        <el-card v-else shadow="never" class="empty-card">
          <div class="empty-state">
            <el-icon :size="50" color="#909399"><InfoFilled /></el-icon>
            <p>선택된 고객이 없습니다. 목록에서 선택하거나 신규 등록을 진행하세요.</p>
          </div>
        </el-card>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">

import { ref, onMounted, reactive, computed } from 'vue';
import {
  getCustomers,
  getCustomer,
  deleteCustomer
} from '@/api/customer';
import { getCompanies } from '@/api/company';
import useUserStore from '@/store/modules/user';
import { ElMessage, ElMessageBox } from 'element-plus';
import { InfoFilled } from '@element-plus/icons-vue';
import CustomerListPane from './components/CustomerListPane.vue';
import CustomerDetailPane from './components/CustomerDetailPane.vue';

const leftWidth = ref(450); 
const splitContainer = ref<HTMLElement | null>(null);
let isResizing = false;

const startResize = () => {
  isResizing = true;
  document.addEventListener('mousemove', handleResize);
  document.addEventListener('mouseup', stopResize);
  document.body.style.cursor = 'col-resize';
  document.body.style.userSelect = 'none';
};

const handleResize = (e: MouseEvent) => {
  if (!isResizing || !splitContainer.value) return;
  const containerRect = splitContainer.value.getBoundingClientRect();
  const newWidth = e.clientX - containerRect.left;

  if (newWidth > 300 && newWidth < containerRect.width - 400) {
    leftWidth.value = newWidth;
  }
};

const stopResize = () => {
  isResizing = false;
  document.removeEventListener('mousemove', handleResize);
  document.removeEventListener('mouseup', stopResize);
  document.body.style.cursor = '';
  document.body.style.userSelect = '';
};

const userStore = useUserStore();
const isRetailer = computed(() => userStore.companyType === 'RTL');
const companies = ref<any[]>([]);
const companiesLoading = ref(false);

const customerList = ref<any[]>([]);
const total = ref(0);

const currentCustomer = ref<any>(null);
const isNewMode = ref(false);

const activeTab = ref('basic');

const listQuery = ref({
  name: '',
  phone: '',
  birthDate: null,
  companyId: null,
  page: 1,
  pageSize: 20
});

const customerDetail = reactive({
  id: null,
  name: '',
  phone: '',
  email: '',
  birthDate: '',
  note: '',
  companyName: '',
  creatorName: '',
  createdAt: '',
  updatedAt: ''
});

const fetchCompanies = async () => {
  if (isRetailer.value) {
    return;
  }
  companiesLoading.value = true;
  try {
    const res = await getCompanies({ category: 'RTL', page: 1, pageSize: 1000 });
    companies.value = res.data.items || [];
  } catch (error) {
    console.error('소매점 목록 로드 실패:', error);
  } finally {
    companiesLoading.value = false;
  }
};

onMounted(() => {
  if (isRetailer.value && userStore.companyId) {
    listQuery.value.companyId = userStore.companyId;
    companies.value = [{
      id: userStore.companyId,
      name: userStore.companyName || '내 소매점'
    }];
  }
  fetchCompanies();
  fetchCustomers();
});

const fetchCustomers = async () => {
  try {
    const res = await getCustomers(listQuery.value);
    customerList.value = res.data.items;
    total.value = res.data.total;
  } catch (error) {
    ElMessage.error('고객 목록 로드 실패');
  }
};

const handleRowClick = async (row: any) => {
  try {
    isNewMode.value = false;
    currentCustomer.value = row;
    const res = await getCustomer(row.id);
    Object.assign(customerDetail, res.data);
    activeTab.value = 'basic';
  } catch (error) {
    ElMessage.error('상세 정보 로드 실패');
  }
};

const handleCreate = () => {
  isNewMode.value = true;
  currentCustomer.value = { id: 0, name: '신규 고객' };
  Object.assign(customerDetail, {
    name: '',
    phone: '',
    email: '',
    birthDate: '',
    note: '',
    companyName: '',
    creatorName: '',
    createdAt: '',
    updatedAt: ''
  });
  activeTab.value = 'basic';
};

const onCustomerSaveSuccess = async (data: any) => {
  fetchCustomers();
  if (isNewMode.value) {
    isNewMode.value = false;
    handleRowClick(data);
  } else {
    const res = await getCustomer(currentCustomer.value.id);
    Object.assign(customerDetail, res.data);
  }
};

const handleDelete = (row: any) => {
  ElMessageBox.confirm('정말로 해당 고객 정보를 삭제하시겠습니까?', '경고', {
    confirmButtonText: '확인',
    cancelButtonText: '취소',
    type: 'warning'
  }).then(async () => {
    try {
      await deleteCustomer(row.id);
      ElMessage.success('삭제되었습니다');
      if (currentCustomer.value?.id === row.id) {
        currentCustomer.value = null;
        isNewMode.value = false;
      }
      fetchCustomers();
    } catch (error) {
      ElMessage.error('삭제 실패');
    }
  });
};
</script>

<style lang="scss" src="./components/CustomerStyles.scss" scoped></style>

