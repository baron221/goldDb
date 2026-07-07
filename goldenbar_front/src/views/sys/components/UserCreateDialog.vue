<template>
<base-popup v-model="visible" title="신규 사용자 추가" width="420px" @close="handleClose">
    <el-form ref="userForm" :model="tempUser" :rules="rules" label-width="110px" style="padding-right: 0.9375rem; margin-top: 0.625rem;">

      <el-form-item label="구분" prop="userType">
        <el-radio-group v-model="tempUser.userType">
          <el-radio value="ADMIN">관리자</el-radio>
          <el-radio value="COMPANY">업체</el-radio>
        </el-radio-group>
      </el-form-item>

      <el-form-item label="업체" v-if="tempUser.userType === 'COMPANY'" prop="companyId">
        <company-select v-model="tempUser.companyId" />
      </el-form-item>

      <el-form-item label="이름" prop="name">
        <el-input v-model="tempUser.name" placeholder="이름을 입력하세요" />
      </el-form-item>

      <el-form-item label="아이디" prop="username">
        <el-input v-model="tempUser.username" autocomplete="off" placeholder="아이디를 입력하세요" />
      </el-form-item>

      <el-form-item label="비밀번호" prop="password">
        <el-input v-model="tempUser.password" type="password" show-password autocomplete="new-password" placeholder="비밀번호를 입력하세요" />
      </el-form-item>

      <el-form-item label="비밀번호 확인" prop="confirmPassword">
        <el-input v-model="tempUser.confirmPassword" type="password" show-password autocomplete="new-password" placeholder="비밀번호를 한번 더 입력하세요" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="visible = false">취소</el-button>
      <el-button type="primary" :loading="submitting" @click="confirmCreate">확인</el-button>
    </template>
  </base-popup>
</template>

<script setup>
import { ref, reactive, watch } from 'vue';
import { createUser } from '@/api/user';
import { getCompanies } from '@/api/company';
import { ElMessage } from 'element-plus';
import CompanySelect from '@/components/CompanySelect/index.vue';
import BasePopup from '@/components/BasePopup/index.vue';

const props = defineProps({
  modelValue: Boolean,
  defaultCompanyId: {
    type: Number,
    default: null
  },
  defaultUserType: {
    type: String,
    default: 'ADMIN'
  }
});

const emit = defineEmits(['update:modelValue', 'created']);

const visible = ref(false);
const userForm = ref(null);
const submitting = ref(false);
const allCompanies = ref([]);

const tempUser = reactive({
  username: '',
  password: '',
  confirmPassword: '',
  name: '',
  userType: 'ADMIN',
  companyId: null
});

const validateConfirmPassword = (rule, value, callback) => {
  if (value === '') {
    callback(new Error('비밀번호 확인을 입력해 주세요'));
  } else if (value !== tempUser.password) {
    callback(new Error('비밀번호가 일치하지 않습니다'));
  } else {
    callback();
  }
};

const validatePassword = (rule, value, callback) => {
  if (value === '') {
    callback(new Error('비밀번호를 입력해 주세요'));
  } else {
    if (tempUser.confirmPassword !== '') {
      if (userForm.value) {
        userForm.value.validateField('confirmPassword');
      }
    }
    callback();
  }
};

const rules = {
  userType: [
    { required: true, message: '구분을 선택해 주세요', trigger: 'change' }
  ],
  name: [
    { required: true, message: '이름을 입력해 주세요', trigger: 'blur' }
  ],
  username: [
    { required: true, message: '아이디를 입력해 주세요', trigger: 'blur' }
  ],
  password: [
    { required: true, validator: validatePassword, trigger: 'blur' }
  ],
  confirmPassword: [
    { required: true, validator: validateConfirmPassword, trigger: 'blur' }
  ]
};

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val) {
    resetForm();
    fetchCompanies();
  }
});

watch(visible, (val) => {
  emit('update:modelValue', val);
});

const resetForm = () => {
  tempUser.username = '';
  tempUser.password = '';
  tempUser.confirmPassword = '';
  tempUser.name = '';
  tempUser.userType = props.defaultUserType;
  tempUser.companyId = props.defaultCompanyId;
  if (userForm.value) {
    userForm.value.resetFields();
  }
};

const getCompanyTypeName = (category) => {
  if (!category) return '업체';
  const map = {
    'RTL': '소매점',
    'MFG': '제조사',
    'DCC': '물류센터'
  };
  return map[category] || '업체';
};

const fetchCompanies = async () => {
  try {
    const res = await getCompanies({ page: 1, pageSize: 1000 });
    allCompanies.value = res.data.items;
  } catch (error) {
    console.error('Failed to fetch companies:', error);
  }
};

const handleClose = () => {
  visible.value = false;
};

const confirmCreate = async () => {
  if (!userForm.value) return;

  userForm.value.validate(async (valid) => {
    if (valid) {
      submitting.value = true;
      try {
        const res = await createUser(tempUser);
        ElMessage.success('사용자가 추가되었습니다');
        visible.value = false;
        emit('created', res.data);
      } catch (error) {
        ElMessage.error('추가 실패: ' + (error.response?.data?.message || error.message));
      } finally {
        submitting.value = false;
      }
    } else {
      ElMessage.warning('필수 입력 항목을 확인해 주세요');
    }
  });
};
</script>

