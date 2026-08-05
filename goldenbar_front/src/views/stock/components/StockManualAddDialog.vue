<template>
<base-popup
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    title="재고 수기등록"
    width="500px"
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
        <el-input :model-value="companyName" disabled />
      </el-form-item>

      <el-form-item label="함량" required>
        <common-select
          v-model="form.purity"
          group-code="MATERIAL_GRADE"
          placeholder="함량 선택"
          @change="handlePurityChange"
        />
      </el-form-item>

      <el-form-item label="컬러">
        <common-select
          v-model="form.color"
          group-code="PRODUCT_COLOR"
          placeholder="컬러 선택 (선택 사항)"
        />
      </el-form-item>

      <el-form-item label="사이즈">
        <el-input v-model="form.size" placeholder="사이즈 (선택 사항)" />
      </el-form-item>

      <el-form-item label="실중량(g)" required>
        <el-input-number v-model="form.actualWeight" :precision="2" :step="0.1" :min="0" style="width: 100%;" />
      </el-form-item>

      <el-form-item label="재료비">
        <amount-input v-model="form.retailerConfirmMaterialCost" placeholder="재료비 (선택 사항)" />
      </el-form-item>

      <el-form-item label="수공비">
        <amount-input v-model="form.retailerConfirmLaborCost" placeholder="수공비 (선택 사항)" />
      </el-form-item>

      <el-form-item label="메모">
        <el-input v-model="form.note" type="textarea" :rows="2" placeholder="메모 (선택 사항)" />
      </el-form-item>

      <el-form-item label="이미지">
        <image-upload v-model:model-value="imageAttachmentId" sub-dir="stocks" />
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="$emit('update:modelValue', false)">취소</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSubmit">추가</el-button>
      </span>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import { ElMessage } from 'element-plus';
import { getProducts } from '@/api/product';
import { createStock, updateStockPhotos } from '@/api/stock';
import useUserStore from '@/store/modules/user';
import BasePopup from '@/components/BasePopup/index.vue';
import CommonSelect from '@/components/CommonSelect/index.vue';
import AmountInput from '@/components/AmountInput/index.vue';
import ImageUpload from '@/components/ImageUpload/index.vue';

const props = defineProps<{
  modelValue: boolean;
}>();

const emit = defineEmits(['update:modelValue', 'saved']);

const userStore = useUserStore();
const submitting = ref(false);
const productsLoading = ref(false);
const productOptions = ref<any[]>([]);
const imageAttachmentId = ref<number | null>(null);
const companyName = ref('');

const form = reactive({
  productId: undefined as number | undefined,
  purity: '',
  color: '',
  size: '',
  actualWeight: 0,
  retailerConfirmMaterialCost: 0,
  retailerConfirmLaborCost: 0,
  note: ''
});

const resetForm = () => {
  form.productId = undefined;
  form.purity = '';
  form.color = '';
  form.size = '';
  form.actualWeight = 0;
  form.retailerConfirmMaterialCost = 0;
  form.retailerConfirmLaborCost = 0;
  form.note = '';
  imageAttachmentId.value = null;
  productOptions.value = [];
  companyName.value = userStore.companyName || '';
};

watch(() => props.modelValue, (val) => {
  if (val) resetForm();
});

const searchProducts = async (query: string) => {
  if (!query) {
    productOptions.value = [];
    return;
  }
  productsLoading.value = true;
  try {
    const isAdmin = userStore.roles.includes('admin');
    const res: any = await getProducts({
      name: query,
      companyId: isAdmin ? undefined : userStore.companyId,
      page: 1,
      pageSize: 20
    });
    productOptions.value = res.data.items || res.data || [];
  } catch (error) {
    console.error('Failed to search products:', error);
  } finally {
    productsLoading.value = false;
  }
};

const handleProductChange = (productId: number) => {
  const product = productOptions.value.find((p) => p.id === productId);
  if (!product) return;
  if (!form.purity && product.purity) {
    form.purity = product.purity.split(',')[0];
  }
  applyProductWeight(product);
};

const handlePurityChange = () => {
  const product = productOptions.value.find((p) => p.id === form.productId);
  if (product) applyProductWeight(product);
};

const applyProductWeight = (product: any) => {
  if (!form.purity) return;
  const match = (product.optionWeights || []).find((ow: any) => ow.purity === form.purity && (!form.color || ow.color === form.color));
  if (match) {
    form.actualWeight = match.weight;
  } else if (product.weight) {
    form.actualWeight = product.weight;
  }
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
  if (!form.actualWeight || form.actualWeight <= 0) {
    ElMessage.warning('실중량을 입력해주세요.');
    return;
  }

  submitting.value = true;
  try {
    const res: any = await createStock({
      productId: form.productId,
      purity: form.purity,
      color: form.color || undefined,
      size: form.size || undefined,
      actualWeight: form.actualWeight,
      retailerConfirmMaterialCost: form.retailerConfirmMaterialCost || undefined,
      retailerConfirmLaborCost: form.retailerConfirmLaborCost || undefined,
      note: form.note || undefined
    });

    const newStockId = res.data?.id;
    if (newStockId && imageAttachmentId.value) {
      await updateStockPhotos(newStockId, [{ attachmentId: imageAttachmentId.value }], imageAttachmentId.value);
    }

    ElMessage.success('재고가 등록되었습니다.');
    emit('update:modelValue', false);
    emit('saved');
  } catch (error) {
    console.error('Failed to create stock:', error);
    ElMessage.error('재고 등록에 실패했습니다.');
  } finally {
    submitting.value = false;
  }
};
</script>
