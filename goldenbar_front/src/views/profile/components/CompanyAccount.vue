<template>
<div v-loading="loading" class="pane-content">
    <el-form ref="companyForm" :model="form" :rules="rules" label-width="130px" style="margin-top: 1.25rem;">
      <el-divider content-position="left">기본 정보</el-divider>
      <el-row :gutter="20">
        <el-col :span="12">
          <el-form-item label="업체분류">
            <el-input :value="categoryLabel" disabled />
          </el-form-item>
        </el-col>
        <el-col v-if="form.category === 'RTL'" :span="12">
          <el-form-item label="관리 물류센터" prop="logisticsCompanyId">
            <company-select
              v-model="form.logisticsCompanyId"
              category="DCC"
              placeholder="관리 물류센터 선택"
              :disabled="!canEditLogistics"
            />
          </el-form-item>
        </el-col>
      </el-row>
      <el-row :gutter="20">
        <el-col :span="12">
          <el-form-item label="상호명" prop="name">
            <el-input v-model="form.name" />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="대표자명" prop="ceo">
            <el-input v-model="form.ceo" />
          </el-form-item>
        </el-col>
      </el-row>

      <el-row :gutter="20">
        <el-col :span="12">
          <el-form-item label="사업자 등록번호" prop="businessNumber">
            <el-input v-model="form.businessNumber" placeholder="000-00-00000" />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="법인번호">
            <el-input v-model="form.corporateNumber" />
          </el-form-item>
        </el-col>
      </el-row>

      <el-row :gutter="20">
        <el-col :span="12">
          <el-form-item label="업태">
            <el-input v-model="form.businessType" />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="종목">
            <el-input v-model="form.businessCategory" />
          </el-form-item>
        </el-col>
      </el-row>

      <el-row :gutter="20">
        <el-col :span="12">
          <el-form-item label="전화번호">
            <el-input v-model="form.phone" />
          </el-form-item>
        </el-col>
      </el-row>

      <el-divider content-position="left">주소 정보</el-divider>
      <el-form-item label="우편번호">
        <el-input v-model="form.zipCode" style="width: 250px;" readonly>
          <template #append>
            <el-button @click="openPostcode">주소 검색</el-button>
          </template>
        </el-input>
      </el-form-item>
      <el-form-item label="기본 주소">
        <el-input v-model="form.addressBase" readonly />
      </el-form-item>
      <el-form-item label="상세 주소">
        <el-input v-model="form.addressDetail" placeholder="나머지 주소를 입력하세요" />
      </el-form-item>

      <el-divider content-position="left">첨부 파일</el-divider>
      <el-form-item label="사업자 등록증">
        <el-upload
          class="upload-demo"
          action="#"
          :auto-upload="false"
          :on-change="handleUploadChange"
          :show-file-list="false"
        >
          <template #trigger>
            <el-button type="primary">파일 선택</el-button>
          </template>
          <template #tip>
            <div class="el-upload__tip">
              PDF, JPG, PNG 파일 (최대 10MB)
            </div>
          </template>
        </el-upload>
        <div v-if="form.businessLicenseFileUrl" style="margin-top: 0.625rem;">
          <el-link type="primary" :href="getApiBaseUrl() + form.businessLicenseFileUrl" target="_blank">
            <el-icon><Document /></el-icon> 등록증 보기 (새창)
          </el-link>
        </div>
      </el-form-item>

      <div style="text-align: right; margin-top: 1.25rem;">
        <el-button type="success" size="large" @click="submit">업체 정보 저장하기</el-button>
      </div>
    </el-form>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, watch, computed } from 'vue';
import { getCompany, updateCompany } from '@/api/company';
import { uploadFile } from '@/api/file';
import { ElMessage } from 'element-plus';
import { Document } from '@element-plus/icons-vue';
import CompanySelect from '@/components/CompanySelect/index.vue';
import useUserStore from '@/store/modules/user';

const props = defineProps({
  companyId: {
    type: Number,
    required: true
  }
});

const userStore = useUserStore();
const loading = ref(false);
const companyForm = ref(null);

const canEditLogistics = computed(() => {
  const adminRoles = ['admin', 'oprator'];
  return userStore.roles.some(role => adminRoles.includes(role));
});

const form = reactive({
  name: '',
  ceo: '',
  businessNumber: '',
  corporateNumber: '',
  businessType: '',
  businessCategory: '',
  phone: '',
  logisticsCode: '',
  centerNumber: '',
  zipCode: '',
  addressBase: '',
  addressDetail: '',
  businessLicenseFileUrl: '',
  category: '',
  logisticsCompanyId: null
});

const rules = {
  name: [{ required: true, message: '상호명을 입력해 주세요', trigger: 'blur' }],
  ceo: [{ required: true, message: '대표자명을 입력해 주세요', trigger: 'blur' }],
  businessNumber: [{ required: false, message: '사업자 번호를 입력해 주세요', trigger: 'blur' }]
};

const categoryLabel = computed(() => {
  const map: Record<string, string> = {
    'MFG': '제조사',
    'DCC': '물류센터',
    'RTL': '소매점',
    'ADM': '관리자'
  };
  return map[form.category] || form.category;
});

const fetchCompanyData = async () => {
  if (!props.companyId) return;
  loading.value = true;
  try {
    const res = await getCompany(props.companyId);
    Object.assign(form, res.data);
  } catch (error) {
    ElMessage.error('업체 정보를 가져오는 데 실패했습니다.');
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  fetchCompanyData();
});

watch(() => props.companyId, () => {
  fetchCompanyData();
});

const handleUploadChange = async (file: any) => {
  try {
    const formData = new FormData();
    formData.append('file', file.raw);
    formData.append('subDir', 'licenses');

    const res = await uploadFile(formData);
    form.businessLicenseFileUrl = res.data;
    ElMessage.success('파일이 업로드되었습니다. 저장 버튼을 눌러야 반영됩니다.');
  } catch (error) {
    ElMessage.error('업로드 실패');
  }
};

const openPostcode = () => {

  new window.daum.Postcode({
    oncomplete: (data: any) => {
      let fullAddress = data.address;
      let extraAddress = '';

      if (data.addressType === 'R') {
        if (data.bname !== '') {
          extraAddress += data.bname;
        }
        if (data.buildingName !== '') {
          extraAddress += (extraAddress !== '' ? `, ${data.buildingName}` : data.buildingName);
        }
        fullAddress += (extraAddress !== '' ? ` (${extraAddress})` : '');
      }

      form.zipCode = data.zonecode;
      form.addressBase = fullAddress;
    }
  }).open();
};

const getApiBaseUrl = () => {
  return import.meta.env.VITE_APP_BASE_API || '';
};

const submit = async () => {

  companyForm.value?.validate(async (valid: boolean) => {
    if (valid) {
      try {
        await updateCompany(props.companyId, form);
        ElMessage.success('업체 정보가 성공적으로 업데이트되었습니다.');
      } catch (error) {
        ElMessage.error('저장에 실패했습니다.');
      }
    }
  });
};
</script>

<style scoped>
.pane-content {
  padding: 1.25rem;
  min-height: 400px;
}
</style>

