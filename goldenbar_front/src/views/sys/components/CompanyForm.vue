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
        <el-form-item :label="$t('sys.company.labels.corporateNumber')">
          <el-input v-model="localForm.corporateNumber" />
        </el-form-item>
      </el-col>
    </el-row>

    <el-row :gutter="20">
      <el-col :xs="24" :sm="12">
        <el-form-item :label="$t('sys.company.labels.businessType')">
          <el-input v-model="localForm.businessType" />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="12">
        <el-form-item :label="$t('sys.company.labels.businessCategory')">
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
        <el-form-item :label="$t('sys.company.labels.logisticsCode')">
          <el-input v-model="localForm.logisticsCode" />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="5">
        <el-form-item :label="$t('sys.company.labels.centerNumber')">
          <el-input v-model="localForm.centerNumber" />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="6" v-if="localForm.category === 'RTL'">
        <el-form-item :label="$t('sys.company.labels.isDirectManagement')">
          <el-switch v-model="localForm.isDirectManagement" :active-text="$t('sys.company.labels.isDirect')" :inactive-text="$t('sys.company.labels.general')" />
        </el-form-item>
      </el-col>
    </el-row>

    <el-divider content-position="left">{{ $t('sys.company.labels.addressInfo') }}</el-divider>
    <el-form-item :label="$t('sys.company.labels.zipCode')">
      <el-input v-model="localForm.zipCode" style="width: 250px;" readonly>
        <template #append>
          <el-button @click="openPostcode">{{ $t('sys.company.labels.addressSearch') }}</el-button>
        </template>
      </el-input>
    </el-form-item>
    <el-form-item :label="$t('sys.company.labels.addressBase')">
      <el-input v-model="localForm.addressBase" readonly />
    </el-form-item>
    <el-form-item :label="$t('sys.company.labels.addressDetail')">
      <el-input v-model="localForm.addressDetail" :placeholder="$t('sys.company.labels.addressDetailPlaceholder')" />
    </el-form-item>

    <el-divider content-position="left">{{ $t('sys.company.labels.accountInfo') }}</el-divider>
    <el-row :gutter="20">
      <el-col :xs="24" :sm="24" :md="8" class="mb-4 md:mb-0">
        <el-form-item :label="$t('sys.company.labels.bankName')">
          <el-input v-model="localForm.bankName" :placeholder="$t('sys.company.labels.bankNamePlaceholder')" />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="24" :md="8" class="mb-4 md:mb-0">
        <el-form-item :label="$t('sys.company.labels.bankAccount')">
          <el-input v-model="localForm.bankAccount" :placeholder="$t('sys.company.labels.bankAccountPlaceholder')" />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="24" :md="8" class="mb-4 md:mb-0">
        <el-form-item :label="$t('sys.company.labels.accountHolder')">
          <el-input v-model="localForm.accountHolder" :placeholder="$t('sys.company.labels.accountHolderPlaceholder')" />
        </el-form-item>
      </el-col>
    </el-row>

    <el-divider content-position="left">{{ $t('sys.company.labels.attachments') }}</el-divider>
    <el-form-item :label="$t('sys.company.labels.businessLicense')">
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
            {{ $t('sys.company.labels.fileHint') }}
          </div>
        </template>
      </el-upload>
      <div v-if="localForm.businessLicenseFileUrl" style="margin-top: 0.625rem;">
        <el-link type="primary" :href="getApiBaseUrl() + localForm.businessLicenseFileUrl" target="_blank">
          <el-icon><Document /></el-icon> {{ $t('sys.company.labels.viewLicense') }}
        </el-link>
      </div>
    </el-form-item>
  </el-form>
</template>

<script setup>
import { ref } from 'vue';
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

const { t } = useI18n();
const formRef = ref(null);

// Bind directly to the parent's reactive object instead of keeping a
// separate local copy synced through a watch/emit loop. That loop could
// leave the form showing a previously-selected company's data; sharing
// the same reactive object keeps the form in lockstep with the parent.
const localForm = props.modelValue;

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

      document.querySelector(`input[placeholder="${t('sys.company.labels.addressDetailPlaceholder')}"]`)?.focus();
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
    ElMessage.success(t('sys.company.uploadSuccess'));
  } catch {
    ElMessage.error(t('sys.company.uploadFail'));
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

