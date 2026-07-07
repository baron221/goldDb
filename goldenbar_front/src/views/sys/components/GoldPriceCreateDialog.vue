<template>
<base-popup v-model="visible" title="새 시세 등록" @close="handleClose">
    <el-form ref="dataForm" :model="temp" label-position="left" label-width="100px" style="width: 400px; margin-left: 3.125rem;">
      <el-form-item label="고시 시간" prop="announcedAt">
        <el-date-picker v-model="temp.announcedAt" type="datetime" placeholder="고시 시간을 선택하세요" style="width: 100%;" />
      </el-form-item>
      <el-form-item label="순금" prop="pureGold">
        <el-input-number v-model="temp.pureGold" :min="0" :step="1000" style="width: 100%;" />
      </el-form-item>
      <el-form-item label="18K" prop="gold18K">
        <el-input-number v-model="temp.gold18K" :min="0" :step="1000" style="width: 100%;" />
      </el-form-item>
      <el-form-item label="14K" prop="gold14K">
        <el-input-number v-model="temp.gold14K" :min="0" :step="1000" style="width: 100%;" />
      </el-form-item>
      <el-form-item label="백금" prop="platinum">
        <el-input-number v-model="temp.platinum" :min="0" :step="1000" style="width: 100%;" />
      </el-form-item>
      <el-form-item label="실버" prop="silver">
        <el-input-number v-model="temp.silver" :min="0" :step="1000" style="width: 100%;" />
      </el-form-item>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="visible = false">
          취소
        </el-button>
        <el-button type="primary" :loading="submitting" @click="createData">
          등록
        </el-button>
      </div>
    </template>
  </base-popup>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { createGoldPrice, getLatestGoldPrice } from '@/api/gold-price';
import { ElMessage } from 'element-plus';
import BasePopup from '@/components/BasePopup/index.vue';

const props = defineProps({
  modelValue: Boolean
});

const emit = defineEmits(['update:modelValue', 'saved']);

const visible = ref(false);
const submitting = ref(false);

const temp = ref({
  announcedAt: new Date(),
  pureGold: 0,
  gold18K: 0,
  gold14K: 0,
  platinum: 0,
  silver: 0
});

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val) {
    initializeLatest();
  }
});

watch(visible, (val) => {
  emit('update:modelValue', val);
});

const initializeLatest = async () => {
  try {
    const response = await getLatestGoldPrice();
    if (response.data) {
      temp.value = {
        announcedAt: new Date(),
        pureGold: response.data.pureGold,
        gold18K: response.data.gold18K,
        gold14K: response.data.gold14K,
        platinum: response.data.platinum,
        silver: response.data.silver
      };
    } else {
      resetTemp();
    }
  } catch (error) {
    resetTemp();
  }
};

const resetTemp = () => {
  temp.value = {
    announcedAt: new Date(),
    pureGold: 0,
    gold18K: 0,
    gold14K: 0,
    platinum: 0,
    silver: 0
  };
};

const handleClose = () => {
  visible.value = false;
  resetTemp();
};

const createData = async () => {
  submitting.value = true;
  try {
    await createGoldPrice(temp.value);
    ElMessage.success('시세가 성공적으로 등록되었습니다.');
    visible.value = false;
    emit('saved');
  } catch (error) {
    console.error(error);
    ElMessage.error('시세 등록에 실패했습니다.');
  } finally {
    submitting.value = false;
  }
};
</script>

<style scoped>
</style>

