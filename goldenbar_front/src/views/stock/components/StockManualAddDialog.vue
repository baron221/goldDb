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
        <el-input v-model="form.productName" placeholder="제품명을 입력하세요" />
      </el-form-item>

      <el-form-item label="제품번호">
        <el-input v-model="form.productNo" placeholder="제품번호를 입력하세요 (선택 사항)" />
      </el-form-item>

      <el-form-item label="생산공장">
        <el-input v-model="form.factoryName" placeholder="생산공장을 입력하세요 (선택 사항)" />
      </el-form-item>

      <el-form-item label="함량" required>
        <el-input v-model="form.purity" placeholder="함량을 입력하세요 (예: 14K)" />
      </el-form-item>

      <el-form-item label="컬러">
        <el-input v-model="form.color" placeholder="컬러 (선택 사항)" />
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
import { createStock, updateStockPhotos } from '@/api/stock';
import useUserStore from '@/store/modules/user';
import BasePopup from '@/components/BasePopup/index.vue';
import AmountInput from '@/components/AmountInput/index.vue';
import ImageUpload from '@/components/ImageUpload/index.vue';

const props = defineProps<{
  modelValue: boolean;
}>();

const emit = defineEmits(['update:modelValue', 'saved']);

const userStore = useUserStore();
const submitting = ref(false);
const imageAttachmentId = ref<number | null>(null);

// 제품/함량/컬러/생산공장 - a plain-text entry, not a catalog lookup. This dialog only ever
// exists to record stock that isn't (or doesn't need to be) tied to a real catalog product,
// so there's no product/company search here - what's typed is exactly what gets saved.
const form = reactive({
  productName: '',
  productNo: '',
  factoryName: '',
  purity: '',
  color: '',
  size: '',
  actualWeight: 0,
  retailerConfirmMaterialCost: 0,
  retailerConfirmLaborCost: 0,
  note: ''
});

const resetForm = () => {
  form.productName = '';
  form.productNo = '';
  form.factoryName = userStore.companyName || '';
  form.purity = '';
  form.color = '';
  form.size = '';
  form.actualWeight = 0;
  form.retailerConfirmMaterialCost = 0;
  form.retailerConfirmLaborCost = 0;
  form.note = '';
  imageAttachmentId.value = null;
};

watch(() => props.modelValue, (val) => {
  if (val) resetForm();
});

const handleSubmit = async () => {
  if (!form.productName.trim()) {
    ElMessage.warning('제품명을 입력해주세요.');
    return;
  }
  if (!form.purity.trim()) {
    ElMessage.warning('함량을 입력해주세요.');
    return;
  }
  if (!form.actualWeight || form.actualWeight <= 0) {
    ElMessage.warning('실중량을 입력해주세요.');
    return;
  }

  submitting.value = true;
  try {
    // 함량은 자유 텍스트라 "14K" 대신 그냥 "14"만 입력하기 쉽다 - 그 상태로 저장되면
    // 14K/18K/순금 합계 집계에서 조용히 빠져버리므로, 순수 캐럿 숫자면 여기서 K를 보충한다.
    const trimmedPurity = form.purity.trim();
    const normalizedPurity = /^(14|18|24)$/.test(trimmedPurity) ? `${trimmedPurity}K` : trimmedPurity;

    const res: any = await createStock({
      productName: form.productName,
      productNo: form.productNo || undefined,
      factoryName: form.factoryName || undefined,
      purity: normalizedPurity,
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
