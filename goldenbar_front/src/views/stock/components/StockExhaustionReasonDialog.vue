<template>
<base-popup v-model="visible" title="재고 상태 변경 (소진 처리)" width="400px" @close="handleClose">
    <div v-if="targetStocks.length > 0" class="exhaustion-target-info" style="margin-bottom: 1.25rem; padding: 0.75rem; background-color: #fcfbf9; border-radius: 2px; border: 1px solid #eadecc;">
      <div style="font-weight: bold; font-size: 0.9rem; margin-bottom: 0.5rem; color: #7f6e56;">소진 대상 제품</div>
      <div v-for="stock in targetStocks" :key="stock.id" style="font-size: 0.85rem; color: #606266; margin-bottom: 0.25rem;">
        • {{ stock.productName }} ({{ stock.stockNo }})
        <span v-if="stock.customerName" style="color: #c5a880; font-weight: bold; margin-left: 0.3125rem;">
          [구매고객: {{ stock.customerName }}]
        </span>
      </div>
    </div>
    <el-form :model="deleteForm" label-width="100px">
      <el-form-item label="변경할 상태" required>
        <el-select v-model="deleteForm.reason" placeholder="상태 선택" style="width: 100%;">
          <el-option label="판매" value="SOLD" />
          <el-option label="분실" value="LOST" />
          <el-option label="기타" value="ETC" />
        </el-select>
      </el-form-item>
      <el-form-item label="소진 날짜" required>
        <el-date-picker
          v-model="deleteForm.exhaustionDate"
          type="date"
          placeholder="소진 날짜 선택"
          style="width: 100%;"
          value-format="YYYY-MM-DD"
        />
      </el-form-item>
      <el-form-item label="메모">
        <el-input v-model="deleteForm.memo" type="textarea" placeholder="상세 사유를 입력하세요" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="visible = false">취소</el-button>
      <el-button type="primary" :loading="submitting" @click="confirmDelete">확정</el-button>
    </template>
  </base-popup>
</template>

<script setup lang="ts">

import { ref, reactive, watch } from 'vue';
import { deleteStocksBulk, getStockDetail } from '@/api/stock';
import { ElMessage } from 'element-plus';
import BasePopup from '@/components/BasePopup/index.vue';

const props = defineProps({

  modelValue: Boolean,

  ids: {
    type: Array as () => number[],
    default: () => []
  }
});

const emit = defineEmits([
  'update:modelValue',

  'saved'
]);

const visible = ref(false);

const submitting = ref(false);

const targetStocks = ref<any[]>([]);

const deleteForm = reactive({

  reason: 'SOLD',

  memo: '',

  exhaustionDate: ''
});

watch(() => props.modelValue, async (val) => {
  visible.value = val;
  if (val) {

    deleteForm.reason = 'SOLD';
    deleteForm.memo = '';

    const today = new Date();
    const yyyy = today.getFullYear();
    const mm = String(today.getMonth() + 1).padStart(2, '0');
    const dd = String(today.getDate()).padStart(2, '0');
    deleteForm.exhaustionDate = `${yyyy}-${mm}-${dd}`;

    targetStocks.value = [];
    if (props.ids && props.ids.length > 0) {
      try {
        const promises = props.ids.map(id => getStockDetail(id));
        const responses = await Promise.all(promises);
        targetStocks.value = responses.map(res => res.data);

        if (targetStocks.value.length > 0) {
          const firstStock = targetStocks.value[0];
          if (firstStock.isExhausted) {
            deleteForm.reason = firstStock.status || 'SOLD';
            deleteForm.memo = firstStock.note || '';
            if (firstStock.exhaustionDate) {
              deleteForm.exhaustionDate = firstStock.exhaustionDate.substring(0, 10);
            }
          }
        }
      } catch (err) {
        console.error('Failed to load stock details:', err);
      }
    }
  }
});

watch(visible, (val) => {
  emit('update:modelValue', val);
});

const handleClose = () => {
  visible.value = false;
};

const confirmDelete = async () => {
  if (!deleteForm.reason) {
    ElMessage.warning('변경할 상태를 선택해주세요.');
    return;
  }
  if (!deleteForm.exhaustionDate) {
    ElMessage.warning('소진 날짜를 선택해주세요.');
    return;
  }
  if (!props.ids || props.ids.length === 0) {
    ElMessage.warning('대상을 선택해주세요.');
    return;
  }

  submitting.value = true;
  try {
    await deleteStocksBulk({
      ids: props.ids,
      reason: deleteForm.reason,
      memo: deleteForm.memo,
      exhaustionDate: deleteForm.exhaustionDate
    });
    ElMessage.success('처리되었습니다.');
    visible.value = false;
    emit('saved');
  } catch (error) {
    console.error(error);
    ElMessage.error('처리에 실패했습니다.');
  } finally {
    submitting.value = false;
  }
};
</script>

