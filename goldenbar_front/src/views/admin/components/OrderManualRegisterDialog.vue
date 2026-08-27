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
        <el-select
          v-model="form.productId"
          placeholder="제품명 또는 제품번호로 검색"
          filterable
          remote
          clearable
          :remote-method="searchProducts"
          :loading="productsLoading"
          style="width: 100%;"
          @change="handleProductChange"
        >
          <el-option
            v-for="p in productOptions"
            :key="p.id"
            :label="`[${p.productNo || '-'}] ${p.name}`"
            :value="p.id"
          />
        </el-select>
      </el-form-item>

      <el-form-item label="생산공장">
        <el-input :model-value="selectedProduct?.companyName || '-'" disabled />
      </el-form-item>

      <div style="display: flex; gap: 0.75rem;">
        <el-form-item label="함량" required style="flex: 1;">
          <el-select v-model="form.purity" placeholder="함량 선택" style="width: 100%;">
            <el-option v-for="code in purityOptions" :key="code" :label="codeStore.codeMap[code] || code" :value="code" />
          </el-select>
        </el-form-item>
        <el-form-item label="컬러" style="flex: 1;">
          <el-select v-model="form.color" placeholder="컬러 선택" clearable style="width: 100%;">
            <el-option v-for="code in colorOptions" :key="code" :label="codeStore.codeMap[code] || code" :value="code" />
          </el-select>
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
import { reactive, ref, watch, computed } from 'vue';
import { ElMessage } from 'element-plus';
import { getProducts } from '@/api/product';
import { getRetailersByCenter, getCompanyUsers } from '@/api/company';
import { createOrder } from '@/api/order';
import useUserStore from '@/store/modules/user';
import useCodeStore from '@/store/modules/code';
import BasePopup from '@/components/BasePopup/index.vue';

const props = defineProps<{
  modelValue: boolean;
}>();

const emit = defineEmits(['update:modelValue', 'saved']);

const userStore = useUserStore();
const codeStore = useCodeStore();

const submitting = ref(false);
const productsLoading = ref(false);
const productOptions = ref<any[]>([]);
const retailersList = ref<any[]>([]);
const employeeList = ref<any[]>([]);

const form = reactive({
  productId: undefined as number | undefined,
  purity: '',
  color: '',
  size: '',
  quantity: 1,
  targetCompanyId: null as number | null,
  handledByUserId: null as number | null,
  memo: ''
});

const selectedProduct = computed(() => productOptions.value.find((p) => p.id === form.productId));
const purityOptions = computed(() => selectedProduct.value?.purity ? selectedProduct.value.purity.split(',') : []);
const colorOptions = computed(() => selectedProduct.value?.colors ? selectedProduct.value.colors.split(',') : []);

const resetForm = () => {
  form.productId = undefined;
  form.purity = '';
  form.color = '';
  form.size = '';
  form.quantity = 1;
  form.targetCompanyId = null;
  form.handledByUserId = null;
  form.memo = '';
  productOptions.value = [];
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

const searchProducts = async (query: string) => {
  if (!query) {
    productOptions.value = [];
    return;
  }
  productsLoading.value = true;
  try {
    const res: any = await getProducts({ name: query, page: 1, pageSize: 20 });
    productOptions.value = res.data.items || res.data || [];
  } catch (error) {
    console.error('Failed to search products:', error);
  } finally {
    productsLoading.value = false;
  }
};

const handleProductChange = () => {
  form.purity = purityOptions.value.length > 0 ? purityOptions.value[0] : '';
  form.color = colorOptions.value.length > 0 ? colorOptions.value[0] : '';
};

const handleSubmit = async () => {
  if (!form.productId) {
    ElMessage.warning('제품을 선택해주세요.');
    return;
  }
  if (!form.purity) {
    ElMessage.warning('함량을 선택해주세요.');
    return;
  }

  submitting.value = true;
  try {
    await createOrder({
      directProductId: form.productId,
      directQuantity: form.quantity,
      directPurity: form.purity,
      directColor: form.color || undefined,
      directSize: form.size || undefined,
      directMemo: form.memo || undefined,
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
