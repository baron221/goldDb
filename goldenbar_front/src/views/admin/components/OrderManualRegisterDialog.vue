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
          placeholder="제품명 또는 제품번호로 검색 - 목록에 없으면 그대로 입력 후 선택하세요"
          filterable
          remote
          clearable
          allow-create
          default-first-option
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
        <div v-if="isCustomProduct" style="color: #e6a23c; font-size: 0.8125rem; margin-top: 0.25rem;">
          카탈로그에 없는 제품명입니다 - 제조사/함량/중량을 직접 입력해주세요.
        </div>
      </el-form-item>

      <el-form-item v-if="isCustomProduct" label="제조사" required>
        <company-select v-model="form.manufacturerCompanyId" category="MFG" placeholder="제조사를 선택하세요" style="width: 100%;" />
      </el-form-item>
      <el-form-item v-else label="생산공장">
        <el-input :model-value="selectedProduct?.companyName || '-'" disabled />
      </el-form-item>

      <div style="display: flex; gap: 0.75rem;">
        <el-form-item label="함량" required style="flex: 1;">
          <el-select v-model="form.purity" placeholder="함량 선택" style="width: 100%;">
            <el-option v-for="opt in purityOptions" :key="opt.code" :label="opt.name" :value="opt.code" />
          </el-select>
        </el-form-item>
        <el-form-item label="컬러" style="flex: 1;">
          <el-select v-model="form.color" placeholder="컬러 선택" clearable style="width: 100%;">
            <el-option v-for="opt in colorOptions" :key="opt.code" :label="opt.name" :value="opt.code" />
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

      <div style="display: flex; gap: 0.75rem;">
        <el-form-item label="재료비" style="flex: 1;">
          <el-input-number v-model="form.materialCost" :min="0" :step="1000" style="width: 100%;" />
        </el-form-item>
        <el-form-item label="수공비" style="flex: 1;">
          <el-input-number v-model="form.laborCost" :min="0" :step="1000" style="width: 100%;" />
        </el-form-item>
        <el-form-item v-if="isCustomProduct" label="중량(g)" required style="flex: 1;">
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
import { reactive, ref, watch, computed } from 'vue';
import { ElMessage } from 'element-plus';
import { getProducts } from '@/api/product';
import { getRetailersByCenter, getCompanyUsers } from '@/api/company';
import { createOrder } from '@/api/order';
import useUserStore from '@/store/modules/user';
import useCodeStore from '@/store/modules/code';
import BasePopup from '@/components/BasePopup/index.vue';
import CompanySelect from '@/components/CompanySelect/index.vue';

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
  // el-select's allow-create hands back the typed text itself (a string) when no
  // catalog product matches - that's how a free-text/custom product entry is detected.
  productId: undefined as number | string | undefined,
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

const selectedProduct = computed(() => productOptions.value.find((p) => p.id === form.productId));
const isCustomProduct = computed(() => typeof form.productId === 'string');

// Normalized to {code, name} either way - from the selected product's own purity/colors
// when a real product is picked, or from the MATERIAL_GRADE/PRODUCT_COLOR code groups
// (same source ProductBasicInfo.vue uses when defining a product from scratch) when the
// product name was typed in free-text and there's nothing to derive options from.
const purityOptions = computed(() => {
  if (isCustomProduct.value) return codeStore.getCodesByGroupStore('MATERIAL_GRADE');
  const codes = selectedProduct.value?.purity ? selectedProduct.value.purity.split(',') : [];
  return codes.map((code: string) => ({ code, name: codeStore.codeMap[code] || code }));
});
const colorOptions = computed(() => {
  if (isCustomProduct.value) return codeStore.getCodesByGroupStore('PRODUCT_COLOR');
  const codes = selectedProduct.value?.colors ? selectedProduct.value.colors.split(',') : [];
  return codes.map((code: string) => ({ code, name: codeStore.codeMap[code] || code }));
});

const resetForm = () => {
  form.productId = undefined;
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
  form.purity = purityOptions.value.length > 0 ? purityOptions.value[0].code : '';
  form.color = colorOptions.value.length > 0 ? colorOptions.value[0].code : '';
  // Prefill from the product's own catalog price - still freely editable per order below.
  // A custom (free-text) entry has no catalog price to prefill from, so this naturally
  // resets to 0, prompting the admin to enter 재료비/수공비/중량 by hand.
  form.materialCost = selectedProduct.value?.factoryPrice || 0;
  form.laborCost = selectedProduct.value?.laborCost || 0;
  form.weight = 0;
  if (!isCustomProduct.value) form.manufacturerCompanyId = null;
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
  if (isCustomProduct.value) {
    if (!form.manufacturerCompanyId) {
      ElMessage.warning('제조사를 선택해주세요.');
      return;
    }
    if (!form.weight || form.weight <= 0) {
      ElMessage.warning('중량을 입력해주세요.');
      return;
    }
  }

  submitting.value = true;
  try {
    await createOrder({
      directProductId: isCustomProduct.value ? undefined : form.productId,
      directProductName: isCustomProduct.value ? form.productId : undefined,
      directManufacturerCompanyId: isCustomProduct.value ? form.manufacturerCompanyId : undefined,
      directWeight: isCustomProduct.value ? form.weight : undefined,
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
