<template>
<div class="app-container">

    <splitpanes v-if="!isMobile" class="default-theme company-split-container">

      <pane size="35" min-size="25">
        <company-list-pane
          :company-list="companyList"
          :list-query="listQuery"
          :total="total"
          :is-mobile="false"
          @create="handleCreate"
          @filter="handleFilter"
          @row-click="handleRowClick"
          @delete="handleDelete"
          @change="fetchCompanies"
        />
      </pane>

      <pane size="65" min-size="40">
        <el-card v-if="currentCompany" shadow="never" class="detail-card">
          <template #header>
            <div class="card-header">
              <div class="section-header">
                <h2 v-if="isNewMode" class="section-title">{{ $t('common.create') }}</h2>
                <h2 v-else class="section-title">업체 상세: <el-tag size="small">{{ currentCompany.name }}</el-tag></h2>
              </div>
              <el-button type="success" size="small" @click="handleSave">{{ isNewMode ? $t('common.create') : $t('common.save') }}</el-button>
            </div>
          </template>

          <el-tabs v-model="activeTab">

            <el-tab-pane label="기본 정보" name="basic">
              <company-form
                ref="companyFormRef"
                v-model="companyDetail"
                :is-mobile="false"
              />
            </el-tab-pane>

            <el-tab-pane v-if="!isNewMode" label="소속 사용자" name="users">
              <company-users
                :company-id="currentCompany.id"
                :is-mobile="false"
              />
            </el-tab-pane>

            <el-tab-pane v-if="!isNewMode && companyDetail.category === 'DCC'" label="소속 소매업체" name="retailers">
              <company-retailers
                :company-id="currentCompany.id"
                :region-codes="regionCodes"
              />
            </el-tab-pane>
          </el-tabs>
        </el-card>

        <el-card shadow="never" class="empty-card" v-else>
          <div class="empty-state">
            <el-icon :size="50" color="#909399"><InfoFilled /></el-icon>
            <p>{{ $t('common.noData') }}</p>
          </div>
        </el-card>
      </pane>
    </splitpanes>

    <div v-else class="mobile-layout-container">

      <div v-if="!showMobileDetail" class="mobile-master-view">
        <company-list-pane
          :company-list="companyList"
          :list-query="listQuery"
          :total="total"
          :is-mobile="true"
          @create="handleCreate"
          @filter="handleFilter"
          @row-click="handleRowClick"
          @delete="handleDelete"
          @change="fetchCompanies"
        />
      </div>

      <div v-else class="mobile-detail-view">
        <div class="mobile-detail-header">
          <el-button :icon="ArrowLeft" link @click="showMobileDetail = false">목록으로 돌아가기</el-button>
          <el-button type="success" size="small" @click="handleSave">{{ isNewMode ? $t('common.create') : $t('common.save') }}</el-button>
        </div>
        <div class="mobile-detail-content">
          <el-card shadow="never" class="detail-card">
            <template #header>
              <div class="section-header">
                <h2 v-if="isNewMode" class="section-title">{{ $t('common.create') }}</h2>
                <h2 v-else class="section-title">{{ currentCompany.name }}</h2>
              </div>
            </template>

            <el-tabs v-model="activeTab">
              <el-tab-pane label="기본 정보" name="basic">
                <company-form
                  ref="companyFormMobileRef"
                  v-model="companyDetail"
                  :is-mobile="true"
                />
              </el-tab-pane>

              <el-tab-pane v-if="!isNewMode" label="사용자" name="users">
                <company-users
                  :company-id="currentCompany.id"
                  :is-mobile="true"
                />
              </el-tab-pane>
            </el-tabs>
          </el-card>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useMobile } from '@/hooks/useMobile';
import { ref, onMounted, reactive } from 'vue';
import { useI18n } from 'vue-i18n';
import {
  getCompanies,
  getCompany,
  createCompany,
  updateCompany,
  deleteCompany
} from '@/api/company';
import { ElMessage, ElMessageBox } from 'element-plus';
import { InfoFilled, ArrowLeft } from '@element-plus/icons-vue';
import CompanyForm from './components/CompanyForm.vue';
import CompanyUsers from './components/CompanyUsers.vue';
import CompanyRetailers from './components/CompanyRetailers.vue';
import CompanyListPane from './components/CompanyListPane.vue';
import useCodeStore from '@/store/modules/code';
import { Splitpanes, Pane } from 'splitpanes';
import 'splitpanes/dist/splitpanes.css';

const { isMobile } = useMobile();
const showMobileDetail = ref(false);

const { t } = useI18n();
const codeStore = useCodeStore();
const companyList = ref([]);
const total = ref(0);
const currentCompany = ref(null);
const isNewMode = ref(false);
const companyFormRef = ref(null);
const companyFormMobileRef = ref(null);
const regionCodes = ref([]);

const listQuery = reactive({
  name: '',
  category: null,
  region: null,
  isDirectManagement: null,
  page: 1,
  pageSize: 20
});

const companyDetail = reactive({
  name: '',
  ceo: '',
  region: '',
  businessNumber: '',
  corporateNumber: '',
  businessLicenseFileUrl: '',
  businessType: '',
  businessCategory: '',
  phone: '',
  addressBase: '',
  addressDetail: '',
  zipCode: '',
  logisticsCode: '',
  centerNumber: '',
  isDirectManagement: false,
  logisticsCompanyId: null,
  logisticsCompanyName: '',
  category: 'RTL',
  bankName: '',
  bankAccount: '',
  accountHolder: ''
});

const activeTab = ref('basic');

onMounted(() => {
  fetchRegionCodes();
  fetchCompanies();
});

const fetchRegionCodes = async () => {
  try {
    await codeStore.fetchCodes();
    regionCodes.value = codeStore.getCodesByGroupStore('REGION');
  } catch (error) {
    console.error('Failed to fetch region codes:', error);
  }
};

const fetchCompanies = async () => {
  try {
    const res = await getCompanies(listQuery);
    companyList.value = res.data.items;
    total.value = res.data.total;
  } catch (error) {
    ElMessage.error('목록 로드 실패');
  }
};

const handleFilter = () => {
  listQuery.page = 1;
  fetchCompanies();
};

const handleRowClick = async (row) => {
  try {
    isNewMode.value = false;
    const previousCategory = companyDetail.category;
    currentCompany.value = row;
    const res = await getCompany(row.id);
    Object.assign(companyDetail, res.data);

    if (previousCategory !== res.data.category) {
      activeTab.value = 'basic';
    }

    if (isMobile.value) {
      showMobileDetail.value = true;
    }
  } catch (error) {
    ElMessage.error('상세 정보 로드 실패');
  }
};

const handleCreate = () => {
  isNewMode.value = true;
  currentCompany.value = { id: 0, name: '신규 업체' };
  Object.assign(companyDetail, {
    name: '',
    ceo: '',
    region: '',
    businessNumber: '',
    corporateNumber: '',
    businessLicenseFileUrl: '',
    businessType: '',
    businessCategory: '',
    phone: '',
    addressBase: '',
    addressDetail: '',
    zipCode: '',
    logisticsCode: '',
    centerNumber: '',
    isDirectManagement: false,
    logisticsCompanyId: null,
    logisticsCompanyName: '',
    category: 'RTL',
    bankName: '',
    bankAccount: '',
    accountHolder: ''
  });
  activeTab.value = 'basic';

  if (isMobile.value) {
    showMobileDetail.value = true;
  }
};

const handleSave = async () => {
  const form = isMobile.value ? companyFormMobileRef.value : companyFormRef.value;
  if (form) {
    form.validate(async (valid) => {
      if (valid) {
        try {
          if (isNewMode.value) {
            const res = await createCompany(companyDetail);
            ElMessage.success('업체가 추가되었습니다');
            isNewMode.value = false;
            fetchCompanies();
            if (res.data) {
              handleRowClick(res.data);
            }
          } else {
            await updateCompany(currentCompany.value.id, companyDetail);
            ElMessage.success('저장되었습니다');
            fetchCompanies();
          }
        } catch (error) {
          ElMessage.error('저장 실패: ' + (error.response?.data?.message || error.message));
        }
      }
    });
  }
};

const handleDelete = (row) => {
  ElMessageBox.confirm('정말로 해당 업체를 삭제하시겠습니까?', '경고', {
    confirmButtonText: '확인',
    cancelButtonText: '취소',
    type: 'warning'
  }).then(async () => {
    try {
      await deleteCompany(row.id);
      ElMessage.success('삭제되었습니다');
      if (currentCompany.value?.id === row.id) {
        currentCompany.value = null;
        isNewMode.value = false;
      }
      fetchCompanies();
    } catch (error) {
      ElMessage.error('삭제 실패');
    }
  });
};
</script>

<style scoped lang="scss">
@import "./components/CompanyStyles.scss";
</style>

