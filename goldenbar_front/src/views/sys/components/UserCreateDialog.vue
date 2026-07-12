<template>
<base-popup v-model="visible" :title="$t('userManage.createDialogTitle')" width="420px" @close="handleClose">
    <el-form ref="userForm" :model="tempUser" :rules="rules" label-width="110px" style="padding-right: 0.9375rem; margin-top: 0.625rem;">

      <el-form-item :label="$t('userManage.userTypeLabel')" prop="userType">
        <el-radio-group v-model="tempUser.userType">
          <el-radio value="ADMIN">{{ $t('userManage.userTypeAdmin') }}</el-radio>
          <el-radio value="COMPANY">{{ $t('userManage.userTypeCompany') }}</el-radio>
        </el-radio-group>
      </el-form-item>

      <el-form-item :label="$t('userManage.companyLabel')" v-if="tempUser.userType === 'COMPANY'" prop="companyId">
        <company-select v-model="tempUser.companyId" />
      </el-form-item>

      <el-form-item :label="$t('userManage.nameLabel')" prop="name">
        <el-input v-model="tempUser.name" :placeholder="$t('userManage.namePlaceholder')" />
      </el-form-item>

      <el-form-item :label="$t('userManage.idLabel')" prop="username">
        <el-input v-model="tempUser.username" autocomplete="off" :placeholder="$t('userManage.idPlaceholder')" />
      </el-form-item>

      <el-form-item :label="$t('userManage.pwLabel')" prop="password">
        <el-input v-model="tempUser.password" type="password" show-password autocomplete="new-password" :placeholder="$t('userManage.pwPlaceholder')" />
      </el-form-item>

      <el-form-item :label="$t('userManage.pwConfirmLabel')" prop="confirmPassword">
        <el-input v-model="tempUser.confirmPassword" type="password" show-password autocomplete="new-password" :placeholder="$t('userManage.pwConfirmPlaceholder')" />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="visible = false">{{ $t('userManage.cancel') }}</el-button>
      <el-button type="primary" :loading="submitting" @click="confirmCreate">{{ $t('userManage.confirm') }}</el-button>
    </template>
  </base-popup>
</template>

<script setup>
import { ref, reactive, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { createUser } from '@/api/user';
import { getCompanies } from '@/api/company';
import { ElMessage } from 'element-plus';
import CompanySelect from '@/components/CompanySelect/index.vue';
import BasePopup from '@/components/BasePopup/index.vue';

const { t } = useI18n();

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
    callback(new Error(t('userManage.pwConfirmRequired')));
  } else if (value !== tempUser.password) {
    callback(new Error(t('userManage.pwMismatch')));
  } else {
    callback();
  }
};

const validatePassword = (rule, value, callback) => {
  if (value === '') {
    callback(new Error(t('userManage.pwRequired')));
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
    { required: true, message: t('userManage.userTypeRequired'), trigger: 'change' }
  ],
  name: [
    { required: true, message: t('userManage.nameRequired'), trigger: 'blur' }
  ],
  username: [
    { required: true, message: t('userManage.idRequired'), trigger: 'blur' }
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
  if (!category) return t('userManage.categoryDefault');
  const map = {
    'RTL': t('userManage.categoryRetail'),
    'MFG': t('userManage.categoryManufacturer'),
    'DCC': t('userManage.categoryLogistics')
  };
  return map[category] || t('userManage.categoryDefault');
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
        ElMessage.success(t('userManage.createSuccess'));
        visible.value = false;
        emit('created', res.data);
      } catch (error) {
        ElMessage.error(t('userManage.createFail') + ': ' + (error.response?.data?.message || error.message));
      } finally {
        submitting.value = false;
      }
    } else {
      ElMessage.warning(t('userManage.formCheckWarning'));
    }
  });
};
</script>

