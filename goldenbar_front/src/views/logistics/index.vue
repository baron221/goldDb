<template>
<div class="logistics-list-page app-container">

    <div class="page-header">
      <div class="header-text">
        <h1 class="page-title">물류업체 현황</h1>
        <p class="page-subtitle">등록된 물류(DCC) 업체의 목록과 상세 연락처를 확인합니다.</p>
      </div>
      <div class="header-badge">
        <span class="total-badge">총 {{ total }}개 업체</span>
      </div>
    </div>

    <el-card shadow="never" class="search-card">
      <el-form :inline="true" :model="listQuery" class="search-form">

        <el-form-item label="업체명">
          <el-input
            v-model="listQuery.name"
            placeholder="업체명 검색"
            clearable
            style="width: 200px;"
            @keyup.enter="handleFilter"
            @clear="handleFilter"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
        </el-form-item>

        <el-form-item label="대표자명">
          <el-input
            v-model="listQuery.ceo"
            placeholder="대표자명 검색"
            clearable
            style="width: 160px;"
            @keyup.enter="handleFilter"
            @clear="handleFilter"
          >
            <template #prefix>
              <el-icon><User /></el-icon>
            </template>
          </el-input>
        </el-form-item>

        <el-form-item label="연락처">
          <el-input
            v-model="listQuery.phone"
            placeholder="연락처 검색"
            clearable
            style="width: 160px;"
            @keyup.enter="handleFilter"
            @clear="handleFilter"
          >
            <template #prefix>
              <el-icon><Phone /></el-icon>
            </template>
          </el-input>
        </el-form-item>

        <el-form-item label="지역">
          <common-select
            v-model="listQuery.region"
            group-code="REGION"
            placeholder="전체 지역"
            show-all
            style="width: 140px;"
            @change="handleFilter"
          />
        </el-form-item>

        <el-form-item>
          <el-button type="primary" :icon="SearchIcon" @click="handleFilter">검색</el-button>
          <el-button :icon="Refresh" @click="resetQuery">초기화</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <div v-loading="loading" class="card-grid-wrapper">

      <div v-if="!loading && companyList.length === 0" class="empty-state">
        <el-icon :size="64" color="#c5a880"><OfficeBuilding /></el-icon>
        <p>검색 조건에 해당하는 물류업체가 없습니다.</p>
        <el-button @click="resetQuery">검색 초기화</el-button>
      </div>

      <div v-else class="company-card-grid">
        <div
          v-for="company in companyList"
          :key="company.id"
          class="company-card"
          @click="openDetail(company)"
        >

          <div class="card-top-row">
            <el-tag type="warning" size="small" class="dcc-tag">물류(DCC)</el-tag>
            <el-tag v-if="company.isDirectManagement" type="danger" size="small">직영</el-tag>
          </div>

          <h3 class="company-name">{{ company.name }}</h3>

          <ul class="info-list">
            <li class="info-item">
              <el-icon class="info-icon"><User /></el-icon>
              <span class="info-label">대표자</span>
              <span class="info-value">{{ company.ceo || '-' }}</span>
            </li>
            <li class="info-item">
              <el-icon class="info-icon"><Phone /></el-icon>
              <span class="info-label">연락처</span>
              <span class="info-value">{{ company.phone || '-' }}</span>
            </li>
            <li class="info-item">
              <el-icon class="info-icon"><Location /></el-icon>
              <span class="info-label">지역</span>
              <span class="info-value">{{ getRegionName(company.region) || '-' }}</span>
            </li>
            <li v-if="company.addressBase" class="info-item address-item">
              <el-icon class="info-icon"><MapLocation /></el-icon>
              <span class="info-label">주소</span>
              <span class="info-value address-text">
                {{ company.addressBase }}
                <span v-if="company.addressDetail"> {{ company.addressDetail }}</span>
              </span>
            </li>
          </ul>

          <div class="card-footer">
            <span class="retailer-count-badge">
              <el-icon><Connection /></el-icon>
              소속 소매점 {{ company.retailerCount ?? '-' }}개
            </span>
            <span class="view-detail-hint">상세보기 →</span>
          </div>
        </div>
      </div>

      <div v-if="total > 0" class="pagination-container">
        <el-pagination
          v-model:current-page="listQuery.page"
          v-model:page-size="listQuery.pageSize"
          :total="total"
          :page-sizes="[12, 24, 48]"
          layout="total, sizes, prev, pager, next, jumper"
          background
          @current-change="fetchList"
          @size-change="handleSizeChange"
        />
      </div>
    </div>

    <base-popup
      v-model="detailVisible"
      :title="detailCompany?.name"
      width="580px"
      class="logistics-detail-dialog"
    >
      <div v-if="detailCompany" class="detail-body">
        <div class="detail-section">
          <h4 class="detail-section-title">기본 정보</h4>
          <el-descriptions :column="2" border size="small">
            <el-descriptions-item label="업체명">{{ detailCompany.name }}</el-descriptions-item>
            <el-descriptions-item label="대표자">{{ detailCompany.ceo || '-' }}</el-descriptions-item>
            <el-descriptions-item label="연락처">{{ detailCompany.phone || '-' }}</el-descriptions-item>
            <el-descriptions-item label="사업자등록번호">{{ detailCompany.businessNumber || '-' }}</el-descriptions-item>
            <el-descriptions-item label="지역" :span="2">{{ getRegionName(detailCompany.region) || '-' }}</el-descriptions-item>
            <el-descriptions-item label="주소" :span="2">
              {{ detailCompany.addressBase || '-' }}
              <span v-if="detailCompany.addressDetail"> {{ detailCompany.addressDetail }}</span>
            </el-descriptions-item>
          </el-descriptions>
        </div>

        <div v-if="detailCompany.bankName || detailCompany.bankAccount" class="detail-section">
          <h4 class="detail-section-title">정산 정보</h4>
          <el-descriptions :column="2" border size="small">
            <el-descriptions-item label="은행명">{{ detailCompany.bankName || '-' }}</el-descriptions-item>
            <el-descriptions-item label="예금주">{{ detailCompany.accountHolder || '-' }}</el-descriptions-item>
            <el-descriptions-item label="계좌번호" :span="2">{{ detailCompany.bankAccount || '-' }}</el-descriptions-item>
          </el-descriptions>
        </div>

        <div v-if="detailCompany.centerNumber || detailCompany.logisticsCode" class="detail-section">
          <h4 class="detail-section-title">물류 정보</h4>
          <el-descriptions :column="2" border size="small">
            <el-descriptions-item label="센터 번호">{{ detailCompany.centerNumber || '-' }}</el-descriptions-item>
            <el-descriptions-item label="물류 코드">{{ detailCompany.logisticsCode || '-' }}</el-descriptions-item>
          </el-descriptions>
        </div>
      </div>

      <template #footer>
        <el-button @click="detailVisible = false">닫기</el-button>
      </template>
    </base-popup>
  </div>
</template>

<script setup lang="ts">

import { ref, reactive, onMounted } from 'vue';
import { getCompanies, getCompany } from '@/api/company';
import { ElMessage } from 'element-plus';
import {
  Search,
  Refresh,
  User,
  Phone,
  Location,
  MapLocation,
  OfficeBuilding,
  Connection
} from '@element-plus/icons-vue';
import CommonSelect from '@/components/CommonSelect/index.vue';
import BasePopup from '@/components/BasePopup/index.vue';
import useCodeStore from '@/store/modules/code';

const SearchIcon = Search;

const codeStore = useCodeStore();
const loading = ref(false);
const companyList = ref<any[]>([]);
const total = ref(0);
const regionCodes = ref<any[]>([]);

const detailVisible = ref(false);
const detailCompany = ref<any>(null);

const listQuery = reactive({
  name: '',
  ceo: '',
  phone: '',
  region: null as string | null,
  category: 'DCC', 
  page: 1,
  pageSize: 12
});

onMounted(() => {
  fetchRegionCodes();
  fetchList();
});

const fetchRegionCodes = async () => {
  try {
    await codeStore.fetchCodes();
    regionCodes.value = codeStore.getCodesByGroupStore('REGION');
  } catch (e) {
    console.error('지역 코드 조회 실패:', e);
  }
};

const getRegionName = (code: string) => {
  const found = regionCodes.value.find((c: any) => c.code === code);
  return found ? found.name : code;
};

const fetchList = async () => {
  loading.value = true;
  try {
    const res = await getCompanies(listQuery);
    companyList.value = res.data.items;
    total.value = res.data.total ?? res.data.totalCount ?? 0;
  } catch (e) {
    ElMessage.error('물류업체 목록 조회에 실패했습니다.');
  } finally {
    loading.value = false;
  }
};

const handleFilter = () => {
  listQuery.page = 1;
  fetchList();
};

const handleSizeChange = () => {
  listQuery.page = 1;
  fetchList();
};

const resetQuery = () => {
  listQuery.name = '';
  listQuery.ceo = '';
  listQuery.phone = '';
  listQuery.region = null;
  listQuery.page = 1;
  fetchList();
};

const openDetail = async (company: any) => {
  try {
    const res = await getCompany(company.id);
    detailCompany.value = res.data;
    detailVisible.value = true;
  } catch (e) {
    ElMessage.error('업체 상세 정보 조회에 실패했습니다.');
  }
};
</script>

<style lang="scss" scoped>
@import "./LogisticsStyles.scss";
</style>

