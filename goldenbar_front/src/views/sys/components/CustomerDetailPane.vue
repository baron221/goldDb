<template>
<el-card shadow="never" class="right-detail-card">
    <div class="card-header" style="margin-bottom: 15px; display: flex; justify-content: space-between; align-items: center;">
      <div class="section-header">
        <h2 v-if="isNewMode" class="section-title">신규 고객 등록</h2>
        <h2 v-else class="section-title">고객 상세: <el-tag size="small">{{ currentCustomer?.name }}</el-tag></h2>
      </div>
      <el-button
        v-if="localActiveTab === 'basic'"
        type="success"
        size="small"
        :icon="Check"
        @click="handleSave"
      >
        {{ isNewMode ? '등록' : '저장' }}
      </el-button>
    </div>

    <el-tabs v-model="localActiveTab" style="margin-top: -10px;">

      <el-tab-pane label="기본 정보" name="basic">
        <customer-form
          ref="formRef"
          :mode="isNewMode ? 'create' : 'edit'"
          :customer-data="customerDetail"
          :show-footer="false"
          @success="data => $emit('success', data)"
        />
      </el-tab-pane>

      <el-tab-pane v-if="!isNewMode" label="주문 내역" name="orders">
        <div class="order-list-section" style="margin-top: 15px;">
          <base-table :data="customerOrders" v-loading="ordersLoading" border style="width: 100%">

            <el-table-column type="expand">
              <template #default="props">
                <div class="expand-table-wrapper" style="padding: 10px 20px; background-color: #faf9f6;">
                  <h4 style="margin: 0 0 10px 0; color: #222; font-size: 14px; border-left: 3px solid #c5a880; padding-left: 8px;">
                    주문 품목 리스트
                  </h4>
                  <base-table :data="props.row.orderItems" border size="small" style="width: 100%">
                    <el-table-column prop="productNo" label="제품번호" width="120" />
                    <el-table-column prop="productName" label="제품명" min-width="150" show-overflow-tooltip />
                    <el-table-column prop="purity" label="함량" width="90" align="center" />
                    <el-table-column prop="orderWeight" label="중량" width="90" align="right" :excel-formatter="scope => scope.orderWeight ? scope.orderWeight.toFixed(2) + 'g' : '-'">
                      <template #default="scope">
                        {{ scope.row.orderWeight ? scope.row.orderWeight.toFixed(2) + 'g' : '-' }}
                      </template>
                    </el-table-column>
                    <el-table-column prop="color" label="컬러" width="95" align="center" />
                    <el-table-column prop="size" label="사이즈" width="90" align="center">
                      <template #default="scope">
                        {{ scope.row.size || '-' }}
                      </template>
                    </el-table-column>
                    <el-table-column prop="quantity" label="수량" width="70" align="center" />
                    <el-table-column prop="price" label="금액" width="120" align="right" :excel-formatter="scope => formatPrice(scope.price)">
                      <template #default="scope">
                        ₩ {{ formatPrice(scope.row.price) }}
                      </template>
                    </el-table-column>
                  </base-table>

                  <div class="customer-note-section" style="margin-top: 15px; padding-top: 15px; border-top: 1px dashed #e0dcd5;">
                    <h4 style="margin: 0 0 10px 0; color: #222; font-size: 14px; border-left: 3px solid #c5a880; padding-left: 8px;">
                      메모 &amp; 사진
                    </h4>
                    <div style="display: flex; gap: 15px; align-items: flex-start; flex-wrap: wrap;">
                      <div style="width: 200px; flex-shrink: 0;">
                        <image-upload
                          :initial-url="props.row.customerNotePhotoUrl"
                          @update:initial-url="(val: string | null) => { props.row.customerNotePhotoUrl = val; }"
                          sub-dir="orders"
                        />
                      </div>
                      <div style="flex: 1; min-width: 240px;">
                        <el-input
                          v-model="props.row.customerNoteMemo"
                          type="textarea"
                          :rows="3"
                          maxlength="500"
                          show-word-limit
                          placeholder="메모를 입력하세요 (선택 사항)"
                        />
                        <div style="margin-top: 8px; text-align: right;">
                          <el-button
                            type="primary"
                            size="small"
                            :loading="savingNoteId === props.row.id"
                            @click="handleSaveNote(props.row)"
                          >
                            저장
                          </el-button>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </template>
            </el-table-column>

            <el-table-column prop="orderNo" label="주문번호" width="180" sortable />
            <el-table-column prop="createdAt" label="주문일시" width="160" :excel-formatter="scope => formatDateTime(scope.createdAt)">
              <template #default="scope">
                {{ formatDateTime(scope.row.createdAt) }}
              </template>
            </el-table-column>
            <el-table-column prop="orderMemo" label="메모" min-width="150" show-overflow-tooltip>
              <template #default="scope">
                {{ scope.row.orderMemo || '-' }}
                <el-icon
                  v-if="scope.row.customerNoteMemo || scope.row.customerNotePhotoUrl"
                  title="메모/사진 있음"
                  style="color: #c5a880; vertical-align: -2px; margin-left: 4px;"
                >
                  <Picture />
                </el-icon>
              </template>
            </el-table-column>
            <el-table-column prop="totalAmount" label="총 주문금액" align="right" :excel-formatter="scope => formatPrice(scope.totalAmount)">
              <template #default="scope">
                ₩ {{ formatPrice(scope.row.totalAmount) }}
              </template>
            </el-table-column>
          </base-table>
        </div>
      </el-tab-pane>
    </el-tabs>
  </el-card>
</template>

<script setup lang="ts">

import { ref, watch } from 'vue';
import { getMyOrders, updateOrderCustomerNote } from '@/api/order';
import { ElMessage } from 'element-plus';
import { Check, Picture } from '@element-plus/icons-vue';
import dayjs from 'dayjs';
import BaseTable from '@/components/BaseTable/index.vue';
import ImageUpload from '@/components/ImageUpload/index.vue';
import CustomerForm from './CustomerForm.vue';

const props = defineProps<{
  currentCustomer: any;
  isNewMode: boolean;
  customerDetail: any;
  activeTab: string;
}>();

const emit = defineEmits(['success', 'update:activeTab']);

const localActiveTab = ref('basic');
const customerOrders = ref([]);
const ordersLoading = ref(false);
const formRef = ref<any>(null);

watch(localActiveTab, (newVal) => {
  emit('update:activeTab', newVal);
});

watch(() => props.activeTab, (newVal) => {
  localActiveTab.value = newVal;
});

const fetchCustomerOrders = async (customerId: number) => {
  ordersLoading.value = true;
  try {
    const res = await getMyOrders({ customerId, page: 1, pageSize: 100 });
    customerOrders.value = res.data.items || [];
  } catch (error) {
    console.error('고객 주문 내역 로드 실패:', error);
    ElMessage.error('주문 내역 로드 실패');
  } finally {
    ordersLoading.value = false;
  }
};

// immediate:true fires synchronously while this component is still being set up, so
// fetchCustomerOrders must already be defined above this point (a const isn't hoisted the
// way a function declaration would be - referencing it before its own line throws a
// "Cannot access before initialization" TDZ error).
watch(() => props.currentCustomer, (newVal) => {
  if (newVal && newVal.id && !props.isNewMode) {
    fetchCustomerOrders(newVal.id);
  } else {
    customerOrders.value = [];
  }
}, { immediate: true });

const savingNoteId = ref<number | null>(null);

const handleSaveNote = async (row: any) => {
  savingNoteId.value = row.id;
  try {
    await updateOrderCustomerNote(row.id, {
      customerNoteMemo: row.customerNoteMemo || undefined,
      customerNotePhotoUrl: row.customerNotePhotoUrl || undefined
    });
    ElMessage.success('저장되었습니다.');
  } catch (error) {
    console.error('Failed to save customer note:', error);
    ElMessage.error('저장에 실패했습니다.');
  } finally {
    savingNoteId.value = null;
  }
};

const handleSave = () => {
  if (formRef.value) {
    formRef.value.submitForm();
  }
};

const formatDateTime = (value: string) => {
  if (!value) return '-';
  return dayjs(value).format('YYYY-MM-DD HH:mm:ss');
};

const formatPrice = (value: number) => {
  if (value === null || value === undefined) return '0';
  return value.toLocaleString();
};

</script>

