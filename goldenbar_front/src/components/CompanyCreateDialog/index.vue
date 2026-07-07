<template>
<base-popup
    v-model="visible"
    title="신규 업체 추가"
    width="500px"
    draggable
    append-to-body
    @close="handleClose"
  >
    <el-form ref="createForm" :model="tempCompany" :rules="rules" label-width="120px">
      <el-form-item label="상호명" prop="name">
        <el-input v-model="tempCompany.name" placeholder="상호명을 입력하세요" />
      </el-form-item>
      <el-form-item label="대표자명" prop="ceo">
        <el-input v-model="tempCompany.ceo" placeholder="대표자명을 입력하세요" />
      </el-form-item>
      <el-form-item label="사업자 번호" prop="businessNumber">
        <el-input v-model="tempCompany.businessNumber" placeholder="000-00-00000" />
      </el-form-item>
      <el-form-item label="업체구분" prop="category">
        <common-select
          v-model="tempCompany.category"
          group-code="COMPANY_TYPE"
          placeholder="업체구분 선택"
          :clearable="false"
        />
      </el-form-item>
      <el-form-item label="지역" prop="region">
        <common-select
          v-model="tempCompany.region"
          group-code="REGION"
          placeholder="지역 선택"
        />
      </el-form-item>
      <el-form-item label="직영여부">
        <el-switch v-model="tempCompany.isDirectManagement" active-text="직영" inactive-text="일반" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="visible = false">취소</el-button>
      <el-button type="primary" :loading="loading" @click="confirmCreate">확인</el-button>
    </template>
  </base-popup>
</template>

<script setup>
import { ref, reactive, watch } from 'vue';
import { createCompany } from '@/api/company';
import { ElMessage } from 'element-plus';
import CommonSelect from '@/components/CommonSelect/index.vue';
import BasePopup from '@/components/BasePopup/index.vue';

const props = defineProps({
  modelValue: Boolean
});

const emit = defineEmits(['update:modelValue', 'created']);

const visible = ref(props.modelValue);
const loading = ref(false);
const createForm = ref(null);

const tempCompany = reactive({
  name: '',
  ceo: '',
  businessNumber: '',
  category: '',
  region: '',
  isDirectManagement: false
});

const rules = {
  name: [{ required: true, message: '상호명을 입력해 주세요', trigger: 'blur' }],
  ceo: [{ required: true, message: '대표자명을 입력해 주세요', trigger: 'blur' }],
  businessNumber: [{ required: false, message: '사업자 번호를 입력해 주세요', trigger: 'blur' }],
  category: [{ required: true, message: '업체구분을 선택해 주세요', trigger: 'change' }],
  region: [{ required: true, message: '지역을 선택해 주세요', trigger: 'change' }]
};

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val) {
    resetForm(); 
  }
});

watch(visible, (val) => {
  emit('update:modelValue', val);
});

const resetForm = () => {
  tempCompany.name = '';
  tempCompany.ceo = '';
  tempCompany.businessNumber = '';
  tempCompany.category = '';
  tempCompany.region = '';
  tempCompany.isDirectManagement = false;
  if (createForm.value) {
    createForm.value.clearValidate();
  }
};

const handleClose = () => {
  visible.value = false;
};

const confirmCreate = async () => {
  if (!createForm.value) return;

  createForm.value.validate(async (valid) => {
    if (valid) {
      loading.value = true;
      try {

        const res = await createCompany(tempCompany);
        ElMessage.success('업체가 등록되었습니다');

        emit('created', res.data);
        visible.value = false;
      } catch (error) {
        ElMessage.error('업체 등록 실패: ' + (error.response?.data?.message || error.message));
      } finally {
        loading.value = false;
      }
    }
  });
};
</script>

