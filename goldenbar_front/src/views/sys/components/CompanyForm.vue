<template>
<el-form ref="formRef" :model="localForm" :rules="rules" :label-width="isMobile ? '100px' : '130px'" :label-position="isMobile ? 'top' : 'right'" style="margin-top: 1rem;">
    <el-row :gutter="20">
      <el-col :xs="24" :sm="12">
        <el-form-item :label="$t('sys.company.labels.name')" prop="name">
          <el-input v-model="localForm.name" />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="12">
        <el-form-item :label="$t('sys.company.labels.ceo')" prop="ceo">
          <el-input v-model="localForm.ceo" />
        </el-form-item>
      </el-col>
    </el-row>

    <el-row :gutter="20">
      <el-col :xs="24" :sm="12">
        <el-form-item :label="$t('sys.company.labels.type')" prop="category">
          <common-select
            v-model="localForm.category"
            group-code="COMPANY_TYPE"
            :placeholder="$t('sys.company.labels.type')"
            :clearable="false"
          />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="12">
        <el-form-item :label="$t('sys.company.labels.region')" prop="region">
          <common-select
            v-model="localForm.region"
            group-code="REGION"
            :placeholder="$t('sys.company.labels.region')"
          />
        </el-form-item>
      </el-col>
    </el-row>

    <el-row v-if="localForm.category === 'RTL'" :gutter="20">
      <el-col :xs="24" :sm="12">
        <el-form-item :label="$t('admin.orderApproval.logisticsCompany')">
          <el-input :model-value="localForm.logisticsCompanyName || $t('common.noData')" readonly>
            <template #prefix>
              <el-icon><Connection /></el-icon>
            </template>
          </el-input>
        </el-form-item>
      </el-col>
    </el-row>

    <el-row :gutter="20">
      <el-col :xs="24" :sm="12">
        <el-form-item :label="$t('sys.company.labels.businessNo')" prop="businessNumber">
          <el-input v-model="localForm.businessNumber" placeholder="000-00-00000" />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="12">
        <el-form-item label="법인번호">
          <el-input v-model="localForm.corporateNumber" />
        </el-form-item>
      </el-col>
    </el-row>

    <el-row :gutter="20">
      <el-col :xs="24" :sm="12">
        <el-form-item label="업태">
          <el-input v-model="localForm.businessType" />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="12">
        <el-form-item label="종목">
          <el-input v-model="localForm.businessCategory" />
        </el-form-item>
      </el-col>
    </el-row>

    <el-row :gutter="20">
      <el-col :xs="24" :sm="24" :md="8" class="mb-4 md:mb-0">
        <el-form-item :label="$t('sys.company.labels.phone')">
          <el-input v-model="localForm.phone" />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="5">
        <el-form-item label="물류코드">
          <el-input v-model="localForm.logisticsCode" />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="5">
        <el-form-item label="센터번호">
          <el-input v-model="localForm.centerNumber" />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="6" v-if="localForm.category === 'RTL'">
        <el-form-item label="직영여부">
          <el-switch v-model="localForm.isDirectManagement" active-text="직영" inactive-text="일반" />
        </el-form-item>
      </el-col>
    </el-row>

    <el-divider content-position="left">주소 정보</el-divider>
    <el-form-item label="우편번호">
      <el-input v-model="localForm.zipCode" style="width: 250px;" readonly>
        <template #append>
          <el-button @click="openPostcode">주소 검색</el-button>
        </template>
      </el-input>
    </el-form-item>
    <el-form-item label="기본 주소">
      <el-input v-model="localForm.addressBase" readonly />
    </el-form-item>
    <el-form-item label="상세 주소">
      <el-input v-model="localForm.addressDetail" placeholder="나머지 주소를 입력하세요" />
    </el-form-item>

    <el-divider content-position="left">계좌 정보</el-divider>
    <el-row :gutter="20">
      <el-col :xs="24" :sm="24" :md="8" class="mb-4 md:mb-0">
        <el-form-item label="은행명">
          <el-input v-model="localForm.bankName" placeholder="은행명을 입력하세요" />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="24" :md="8" class="mb-4 md:mb-0">
        <el-form-item label="계좌번호">
          <el-input v-model="localForm.bankAccount" placeholder="계좌번호를 입력하세요" />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="24" :md="8" class="mb-4 md:mb-0">
        <el-form-item label="예금주명">
          <el-input v-model="localForm.accountHolder" placeholder="예금주명을 입력하세요" />
        </el-form-item>
      </el-col>
    </el-row>

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
          <el-button type="primary">{{ $t('contact.marketplace.categories.all') }}</el-button>
        </template>
        <template #tip>
          <div class="el-upload__tip">
            PDF, JPG, PNG 파일 (최대 10MB)
          </div>
        </template>
      </el-upload>
      <div v-if="localForm.businessLicenseFileUrl" style="margin-top: 0.625rem;">
        <el-link type="primary" :href="getApiBaseUrl() + localForm.businessLicenseFileUrl" target="_blank">
          <el-icon><Document /></el-icon> 등록증 보기 (새창)
        </el-link>
      </div>
    </el-form-item>
  </el-form>
</template>

<script setup>
import { ref, reactive, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { Connection, Document } from '@element-plus/icons-vue';
import { uploadFile } from '@/api/file';
import { ElMessage } from 'element-plus';
import CommonSelect from '@/components/CommonSelect/index.vue'; 

const props = defineProps({
  modelValue: {
    type: Object,
    required: true
  },
  isMobile: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits(['update:modelValue']);
const { t } = useI18n();
const formRef = ref(null);

const localForm = reactive({ ...props.modelValue });

watch(() => props.modelValue, (newVal) => {
  Object.assign(localForm, newVal);
}, { deep: true });

watch(localForm, (newVal) => {
  emit('update:modelValue', { ...newVal });
}, { deep: true });

const rules = {
  name: [{ required: true, message: t('register.rules.name'), trigger: 'blur' }],
  ceo: [{ required: true, message: t('register.rules.name'), trigger: 'blur' }],
  businessNumber: [{ required: false, message: t('sys.company.labels.businessNo'), trigger: 'blur' }],
  category: [{ required: true, message: t('sys.company.labels.type'), trigger: 'change' }],
  region: [{ required: true, message: t('sys.company.labels.region'), trigger: 'change' }]
};

const openPostcode = () => {
  new window.daum.Postcode({
    oncomplete: (data) => {
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

      localForm.zipCode = data.zonecode;
      localForm.addressBase = fullAddress;

      document.querySelector('input[placeholder="나머지 주소를 입력하세요"]')?.focus();
    }
  }).open();
};

const handleUploadChange = async (file) => {
  try {
    const formData = new FormData();
    formData.append('file', file.raw);
    formData.append('subDir', 'licenses');

    const res = await uploadFile(formData);
    localForm.businessLicenseFileUrl = res.data;
    ElMessage.success('파일이 업로드되었습니다. 최종 저장을 눌러주세요.');
  } catch {
    ElMessage.error('업로드 실패');
  }
};

const getApiBaseUrl = () => {
  return import.meta.env.VITE_APP_BASE_API || '';
};

const validate = (callback) => {
  if (formRef.value) {
    formRef.value.validate(callback);
  }
};

defineExpose({
  validate
});
</script>

