<template>
<div class="customer-form-container">
    <el-form ref="formRef" :model="formData" :rules="rules" label-width="100px">
      <el-row :gutter="20">
        <el-col :span="isMobile ? 24 : 12">
          <el-form-item label="고객명" prop="name">
            <el-input v-model="formData.name" placeholder="이름을 입력하세요" />
          </el-form-item>
        </el-col>
        <el-col :span="isMobile ? 24 : 12">
          <el-form-item label="연락처" prop="phone">
            <el-input v-model="formData.phone" placeholder="010-0000-0000" />
          </el-form-item>
        </el-col>
      </el-row>

      <el-row :gutter="20">
        <el-col :span="isMobile ? 24 : 12">
          <el-form-item label="이메일" prop="email">
            <el-input v-model="formData.email" placeholder="example@domain.com" />
          </el-form-item>
        </el-col>
        <el-col :span="isMobile ? 24 : 12">
          <el-form-item label="생년월일" prop="birthDate">
            <el-date-picker
              v-model="formData.birthDate"
              type="date"
              placeholder="날짜 선택"
              style="width: 100%;"
              value-format="YYYY-MM-DD"
            />
          </el-form-item>
        </el-col>
      </el-row>

      <el-form-item label="비고" prop="note">
        <el-input
          v-model="formData.note"
          type="textarea"
          :rows="4"
          placeholder="비고를 입력하세요"
        />
      </el-form-item>

      <div v-if="mode === 'edit' && formData.createdAt" class="info-section">
        <el-divider content-position="left">등록 정보</el-divider>
        <el-row :gutter="20">
          <el-col :span="isMobile ? 24 : 12">
            <el-form-item label="등록 업체">
              <el-input :model-value="formData.companyName || '-'" readonly />
            </el-form-item>
          </el-col>
          <el-col :span="isMobile ? 24 : 12">
            <el-form-item label="등록 직원">
              <el-input :model-value="formData.creatorName || '-'" readonly />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="20">
          <el-col :span="isMobile ? 24 : 12">
            <el-form-item label="등록 일시">
              <el-input :model-value="formatDateTime(formData.createdAt)" readonly />
            </el-form-item>
          </el-col>
          <el-col :span="isMobile ? 24 : 12">
            <el-form-item label="수정 일시">
              <el-input :model-value="formatDateTime(formData.updatedAt)" readonly />
            </el-form-item>
          </el-col>
        </el-row>
      </div>

      <div class="form-actions" v-if="showFooter">
        <el-button @click="$emit('cancel')">취소</el-button>
        <el-button type="primary" :loading="loading" @click="submitForm">
          {{ mode === 'create' ? '등록' : '저장' }}
        </el-button>
      </div>
    </el-form>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, watch, onMounted } from 'vue';
import { useMobile } from '@/hooks/useMobile';
import { createCustomer, updateCustomer } from '@/api/customer';
import { ElMessage } from 'element-plus';
import dayjs from 'dayjs';

interface Props {
  mode?: 'create' | 'edit';
  customerData?: any;
  showFooter?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  mode: 'create',
  customerData: () => ({}),
  showFooter: true
});

const emit = defineEmits(['success', 'cancel', 'update:loading']);

const { isMobile } = useMobile();
const formRef = ref();
const loading = ref(false);

const formData = reactive({
  id: null,
  name: '',
  phone: '',
  email: '',
  birthDate: '',
  note: '',
  companyName: '',
  creatorName: '',
  createdAt: '',
  updatedAt: ''
});

const rules = {
  name: [{ required: true, message: '이름은 필수 입력 항목입니다.', trigger: 'blur' }],
  phone: [{ required: true, message: '연락처는 필수 입력 항목입니다.', trigger: 'blur' }],
  email: [
    { type: 'email', message: '올바른 이메일 형식이 아닙니다.', trigger: 'blur' }
  ]
};

const initForm = () => {
  if (props.mode === 'edit' && props.customerData) {
    Object.assign(formData, props.customerData);
  } else {
    Object.assign(formData, {
      id: null,
      name: '',
      phone: '',
      email: '',
      birthDate: '',
      note: '',
      companyName: '',
      creatorName: '',
      createdAt: '',
      updatedAt: ''
    });
  }
};

watch(() => props.customerData, () => {
  initForm();
}, { deep: true });

onMounted(() => {
  initForm();
});

const formatDateTime = (value: string) => {
  if (!value) return '-';
  return dayjs(value).format('YYYY-MM-DD HH:mm:ss');
};

const submitForm = async () => {
  if (!formRef.value) return;

  await formRef.value.validate(async (valid: boolean) => {
    if (valid) {
      loading.value = true;
      emit('update:loading', true);
      try {
        const requestData = {
          name: formData.name,
          phone: formData.phone,
          email: formData.email || null,
          birthDate: formData.birthDate || null,
          note: formData.note || null
        };

        let response;
        if (props.mode === 'create') {
          response = await createCustomer(requestData);
          ElMessage.success('고객이 추가되었습니다');
        } else {
          response = await updateCustomer(formData.id, requestData);
          ElMessage.success('저장되었습니다');
        }
        emit('success', response?.data || formData);
      } catch (error: any) {
        ElMessage.error('저장 실패: ' + (error.response?.data?.message || error.message));
      } finally {
        loading.value = false;
        emit('update:loading', false);
      }
    }
  });
};

defineExpose({
  submitForm,
  resetForm: () => formRef.value?.resetFields()
});
</script>

<style scoped>
.form-actions {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
}
.info-section {
  margin-top: 10px;
}
</style>

