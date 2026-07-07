<template>
<div>

    <base-popup
      :model-value="modelValue"
      @update:model-value="$emit('update:modelValue', $event)"
      title="관리 고객 검색 및 선택"
      class="responsive-dialog-650"
      draggable
      destroy-on-close
    >
      <div class="customer-search-filter" style="margin-bottom: 15px;">
        <el-row :gutter="10">
          <el-col :span="8">
            <el-input
              v-model="customerSearchQuery.name"
              placeholder="고객명 검색"
              clearable
              @keyup.enter="handleCustomerFilter"
            />
          </el-col>
          <el-col :span="8">
            <el-input
              v-model="customerSearchQuery.phone"
              placeholder="연락처 검색"
              clearable
              @keyup.enter="handleCustomerFilter"
            />
          </el-col>
          <el-col :span="8">
            <el-date-picker
              v-model="customerSearchQuery.birthDate"
              type="date"
              placeholder="생년월일 검색"
              style="width: 100%;"
              value-format="YYYY-MM-DD"
              clearable
              @change="handleCustomerFilter"
            />
          </el-col>
        </el-row>
        <div style="display: flex; gap: 10px; margin-top: 10px;">
          <el-button type="primary" style="flex: 2;" :icon="Search" @click="handleCustomerFilter">검색</el-button>
          <el-button type="success" style="flex: 1;" @click="handleAddCustomer">고객 추가</el-button>
        </div>
      </div>

      <base-table
        v-loading="customerListLoading"
        :data="customerList"
        border
        style="width: 100%"
        height="350px"
      >
        <el-table-column prop="name" label="고객명" width="120" />
        <el-table-column prop="phone" label="연락처" width="150" />
        <el-table-column prop="birthDate" label="생년월일" width="120">
          <template #default="scope">
            {{ scope.row.birthDate ? scope.row.birthDate.substring(0, 10) : '-' }}
          </template>
        </el-table-column>
        <el-table-column prop="note" label="비고" show-overflow-tooltip />
        <el-table-column label="선택" width="80" align="center" :fixed="!isMobile ? 'right' : false">
          <template #default="scope">
            <el-button type="success" size="small" @click="handleSelectCustomer(scope.row)">선택</el-button>
          </template>
        </el-table-column>
      </base-table>

      <div class="pagination-container" style="margin-top: 15px; display: flex; justify-content: center;">
        <el-pagination
          v-model:current-page="customerSearchQuery.page"
          v-model:page-size="customerSearchQuery.pageSize"
          :total="customerTotal"
          layout="total, prev, pager, next"
          @current-change="handleCustomerPageChange"
        />
      </div>
    </base-popup>

    <base-popup
      v-model="addCustomerDialogVisible"
      title="신규 고객 등록"
      class="responsive-dialog-600"
      draggable
    >
      <customer-form
        v-if="addCustomerDialogVisible"
        mode="create"
        @success="onAddCustomerSuccess"
        @cancel="addCustomerDialogVisible = false"
      />
    </base-popup>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue';
import { useMobile } from '@/hooks/useMobile';
import { getCustomers } from '@/api/customer';
import type { Customer } from '@/types/customer';
import { ElMessage } from 'element-plus';
import { Search } from '@element-plus/icons-vue';
import BasePopup from '@/components/BasePopup/index.vue';
import BaseTable from '@/components/BaseTable/index.vue';
import CustomerForm from './CustomerForm.vue';

const props = defineProps<{
  modelValue: boolean;
  initialSearch?: string;
}>();

const emit = defineEmits(['update:modelValue', 'select']);

const { isMobile } = useMobile();

const customerSearchQuery = reactive({
  name: '',
  phone: '',
  birthDate: null,
  page: 1,
  pageSize: 10
});

const customerList = ref<Customer[]>([]);
const customerTotal = ref(0);
const customerListLoading = ref(false);
const addCustomerDialogVisible = ref(false);

const fetchCustomersData = async () => {
  customerListLoading.value = true;
  try {
    const res = await getCustomers(customerSearchQuery);
    customerList.value = res.data.items || [];
    customerTotal.value = res.data.total || 0;
  } catch (error) {
    console.error('고객 조회 실패:', error);
    ElMessage.error('고객 조회에 실패했습니다.');
  } finally {
    customerListLoading.value = false;
  }
};

const handleCustomerFilter = () => {
  customerSearchQuery.page = 1;
  fetchCustomersData();
};

const handleCustomerPageChange = (page: number) => {
  customerSearchQuery.page = page;
  fetchCustomersData();
};

const handleSelectCustomer = (customer: Customer) => {
  emit('select', customer);
  emit('update:modelValue', false);
};

const handleAddCustomer = () => {
  addCustomerDialogVisible.value = true;
};

const onAddCustomerSuccess = (customer: Customer) => {
  addCustomerDialogVisible.value = false;
  handleSelectCustomer(customer);
  ElMessage.success('신규 고객이 등록되어 선택되었습니다.');
};

watch(() => props.modelValue, (val) => {
  if (val) {
    if (props.initialSearch) {
      const query = props.initialSearch.trim();
      if (/^[0-9-]+$/.test(query)) {
        customerSearchQuery.name = '';
        customerSearchQuery.phone = query;
      } else {
        customerSearchQuery.name = query;
        customerSearchQuery.phone = '';
      }
    } else {
      customerSearchQuery.name = '';
      customerSearchQuery.phone = '';
    }
    customerSearchQuery.birthDate = null;
    customerSearchQuery.page = 1;
    fetchCustomersData();
  }
});

defineExpose({
  refresh: fetchCustomersData
});
</script>

