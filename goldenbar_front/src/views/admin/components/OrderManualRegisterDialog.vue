<template>
<base-popup
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    title="주문 수기 등록"
    width="560px"
    append-to-body
  >
    <el-form :model="form" label-position="top">
      <el-form-item label="제품 선택" required>
        <el-input v-model="form.productName" placeholder="제품명을 입력하세요" />
      </el-form-item>

      <el-form-item label="제조사" required>
        <company-select v-model="form.manufacturerCompanyId" category="MFG" placeholder="제조사를 선택하세요" style="width: 100%;" />
      </el-form-item>

      <div style="display: flex; gap: 0.75rem;">
        <el-form-item label="함량" required style="flex: 1;">
          <el-input v-model="form.purity" placeholder="함량을 입력하세요 (예: 14K)" />
        </el-form-item>
        <el-form-item label="컬러" style="flex: 1;">
          <el-input v-model="form.color" placeholder="컬러 (선택 사항)" />
        </el-form-item>
      </div>

      <div style="display: flex; gap: 0.75rem;">
        <el-form-item label="사이즈" style="flex: 1;">
          <el-input v-model="form.size" placeholder="사이즈 (선택 사항)" />
        </el-form-item>
        <el-form-item label="수량" style="flex: 1;">
          <el-input-number v-model="form.quantity" :min="1" style="width: 100%;" />
        </el-form-item>
      </div>

      <div style="display: flex; gap: 0.75rem;">
        <el-form-item label="재료비" style="flex: 1;">
          <el-input-number v-model="form.materialCost" :min="0" :step="1000" style="width: 100%;" />
        </el-form-item>
        <el-form-item label="수공비" style="flex: 1;">
          <el-input-number v-model="form.laborCost" :min="0" :step="1000" style="width: 100%;" />
        </el-form-item>
        <el-form-item label="중량(g)" required style="flex: 1;">
          <el-input-number v-model="form.weight" :min="0" :precision="2" :step="0.1" style="width: 100%;" />
        </el-form-item>
      </div>

      <el-form-item label="주문 소매점 (물류/자체)">
        <el-select v-model="form.targetCompanyId" placeholder="소매점을 선택하거나 자체 주문 선택 (선택 사항)" filterable clearable style="width: 100%;">
          <el-option label="물류사 자체 재고용 주문 (본인 구매)" :value="null" />
          <el-option v-for="retailer in retailersList" :key="retailer.id" :label="retailer.name" :value="retailer.id" />
        </el-select>
      </el-form-item>

      <el-form-item label="담당자">
        <el-select v-model="form.handledByUserId" placeholder="주문을 처리하는 직원을 선택하세요 (선택 사항)" filterable clearable style="width: 100%;">
          <el-option v-for="emp in employeeList" :key="emp.id" :label="`${emp.name} (${emp.username})`" :value="emp.id" />
        </el-select>
      </el-form-item>

      <el-form-item label="메모">
        <el-input v-model="form.memo" type="textarea" :rows="2" placeholder="메모 (선택 사항)" />
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="$emit('update:modelValue', false)">취소</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSubmit">등록</el-button>
      </span>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import { ElMessage } from 'element-plus';
import { getRetailersByCenter, getCompanyUsers } from '@/api/company';
import { createOrder } from '@/api/order';
import useUserStore from '@/store/modules/user';
import BasePopup from '@/components/BasePopup/index.vue';
import CompanySelect from '@/components/CompanySelect/index.vue';

const props = defineProps<{
  modelValue: boolean;
}>();

const emit = defineEmits(['update:modelValue', 'saved']);

const userStore = useUserStore();

const submitting = ref(false);
const retailersList = ref<any[]>([]);
const employeeList = ref<any[]>([]);

// 제품/함량/컬러 - a plain-text entry, not a catalog lookup. This whole dialog only ever
// exists to record something that isn't (or doesn't need to be) a real catalog product, so
// there's no product search here - what's typed is exactly what gets saved.
const form = reactive({
  productName: '',
  manufacturerCompanyId: null as number | null,
  purity: '',
  color: '',
  size: '',
  quantity: 1,
  materialCost: 0,
  laborCost: 0,
  weight: 0,
  targetCompanyId: null as number | null,
  handledByUserId: null as number | null,
  memo: ''
});

const resetForm = () => {
  form.productName = '';
  form.manufacturerCompanyId = null;
  form.purity = '';
  form.color = '';
  form.size = '';
  form.quantity = 1;
  form.materialCost = 0;
  form.laborCost = 0;
  form.weight = 0;
  form.targetCompanyId = null;
  form.handledByUserId = null;
  form.memo = '';
};

const fetchRetailersAndEmployees = async () => {
  if (!userStore.companyId) return;
  try {
    const res: any = await getRetailersByCenter(userStore.companyId);
    retailersList.value = res.data || [];
  } catch (error) {
    console.error('Failed to fetch retailers:', error);
  }
  try {
    const empRes: any = await getCompanyUsers(userStore.companyId);
    employeeList.value = empRes.data || [];
  } catch (error) {
    console.error('Failed to fetch employees:', error);
  }
};

watch(() => props.modelValue, (val) => {
  if (val) {
    resetForm();
    fetchRetailersAndEmployees();
  }
});

const handleSubmit = async () => {
  if (!form.productName.trim()) {
    ElMessage.warning('제품명을 입력해주세요.');
    return;
  }
  if (!form.manufacturerCompanyId) {
    ElMessage.warning('제조사를 선택해주세요.');
    return;
  }
  if (!form.purity.trim()) {
    ElMessage.warning('함량을 입력해주세요.');
    return;
  }
  if (!form.weight || form.weight <= 0) {
    ElMessage.warning('중량을 입력해주세요.');
    return;
  }

  submitting.value = true;
  try {
    await createOrder({
      directProductName: form.productName,
      directManufacturerCompanyId: form.manufacturerCompanyId,
      directWeight: form.weight,
      directQuantity: form.quantity,
      directPurity: form.purity,
      directColor: form.color || undefined,
      directSize: form.size || undefined,
      directMemo: form.memo || undefined,
      directFactoryPrice: form.materialCost,
      directLaborCost: form.laborCost,
      targetCompanyId: form.targetCompanyId,
      handledByUserId: form.handledByUserId,
      orderMemo: '정산처리 수기 주문 등록'
    });
    ElMessage.success('주문이 등록되었습니다. 물류승인 화면에서 이어서 처리해주세요.');
    emit('update:modelValue', false);
    emit('saved');
  } catch (error) {
    console.error('Failed to register manual order:', error);
    ElMessage.error('주문 등록에 실패했습니다.');
  } finally {
    submitting.value = false;
  }
};
</script>
