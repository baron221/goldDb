<template>
<div class="partner-retailer-container app-container">
    <div class="section-header luxury">
      <h2 class="section-title">협력 물류센터</h2>
      <div class="header-actions">
        <el-button :icon="Refresh" circle @click="fetchData" :loading="loading" />
      </div>
    </div>

    <el-card shadow="never" class="table-card-luxury">
      <div class="filter-bar">
        <el-input
          v-model="searchQuery"
          placeholder="업체명 검색"
          prefix-icon="Search"
          clearable
          class="sharp-input"
          style="width: 260px;"
        />
      </div>

      <base-table
        v-loading="loading"
        :data="filteredCompanies"
        border
        stripe
        size="small"
        class="sharp-table"
        :header-cell-style="{ backgroundColor: '#fbfaf7', color: '#222', fontWeight: '700', textTransform: 'uppercase', fontSize: '11px', letterSpacing: '0.5px' }"
      >
        <el-table-column label="업체 정보" min-width="200" prop="companyName" :excel-formatter="infoFormatter">
          <template #default="{row}">
            <div class="company-info-cell">
              <div class="comp-name">{{ row.companyName }}</div>
              <div class="comp-meta">
                <span class="ceo">{{ row.ceo }}</span>
                <span class="divider">|</span>
                <span class="region">{{ row.region }}</span>
              </div>
            </div>
          </template>
        </el-table-column>

        <el-table-column label="이달 실적" align="center">
          <el-table-column label="건수" prop="monthlyOrderCount" width="90" align="right" />
          <el-table-column label="금액" prop="monthlyOrderAmount" width="140" align="right" :excel-formatter="(row) => `₩${formatPrice(row.monthlyOrderAmount)}`">
            <template #default="{row}">
              <span class="price-text">₩{{ formatPrice(row.monthlyOrderAmount) }}</span>
            </template>
          </el-table-column>
        </el-table-column>

        <el-table-column label="총 실적" align="center">
          <el-table-column label="건수" prop="totalOrderCount" width="100" align="right" />
          <el-table-column label="금액" prop="totalOrderAmount" width="160" align="right" :excel-formatter="(row) => `₩${formatPrice(row.totalOrderAmount)}`">
            <template #default="{row}">
              <span class="total-price-text">₩{{ formatPrice(row.totalOrderAmount) }}</span>
            </template>
          </el-table-column>
        </el-table-column>

        <el-table-column label="진행중 주문" width="110" align="center" prop="pendingOrderCount" :excel-formatter="(row) => `${row.pendingOrderCount}건`">
          <template #default="{row}">
            <el-tag :type="row.pendingOrderCount > 5 ? 'danger' : 'warning'" effect="plain" class="sharp-tag">
              {{ row.pendingOrderCount }}건
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column label="미지급 잔액" width="140" align="right" prop="totalOutstanding" :excel-formatter="(row) => `₩${formatPrice(row.totalOutstanding)}`">
          <template #default="{row}">
            <span class="total-price-text" :class="{ 'text-danger': row.totalOutstanding > 0 }">₩{{ formatPrice(row.totalOutstanding) }}</span>
          </template>
        </el-table-column>
      </base-table>
    </el-card>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { getPartnerLogisticsStats } from '@/api/dashboard';
import { Refresh } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import { formatPrice } from '@/utils/format';
import BaseTable from '@/components/BaseTable/index.vue';

const loading = ref(false);
const companyStats = ref([]);
const searchQuery = ref('');

const fetchData = async () => {
  loading.value = true;
  try {
    const res = await getPartnerLogisticsStats();
    companyStats.value = res.data;
  } catch (error) {
    console.error('Failed to fetch partner logistics stats:', error);
    ElMessage.error('데이터를 불러오는 데 실패했습니다.');
  } finally {
    loading.value = false;
  }
};

const filteredCompanies = computed(() => {
  const query = searchQuery.value.toLowerCase();
  if (!query) return companyStats.value;
  return companyStats.value.filter(c =>
    c.companyName.toLowerCase().includes(query) ||
    c.ceo.toLowerCase().includes(query)
  );
});

const infoFormatter = (row) => `${row.companyName}\n${row.ceo} | ${row.region}`;

onMounted(() => {
  fetchData();
});
</script>

<style lang="scss" scoped>
.partner-retailer-container {
  padding: 1.5625rem;
  background-color: #fcfcfb;
  min-height: calc(100vh - 50px);
}

.section-header.luxury {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5625rem;

  .section-title {
    font-size: 1.25rem;
    font-weight: 600;
    color: #222;
    text-transform: uppercase;
    letter-spacing: 1px;
    position: relative;
    padding-left: 0.9375rem;
    margin: 0;
    &::after {
      content: '';
      position: absolute;
      left: 0;
      top: 2px;
      bottom: 2px;
      width: 4px;
      background-color: #c5a880;
    }
  }
}

.table-card-luxury {
  border-radius: 2px;
  border-color: #eae6df;

  .filter-bar {
    margin-bottom: 1.25rem;
    display: flex;
    align-items: center;
  }
}

.company-info-cell {
  .comp-name {
    font-size: 0.875rem;
    font-weight: 700;
    color: #222;
    margin-bottom: 0.25rem;
  }
  .comp-meta {
    font-size: 0.8875rem;
    color: #999;
    .divider { margin: 0 0.5rem; color: #eee; }
  }
}

.price-text { font-weight: 700; color: #c5a880; font-family: 'S-CoreDream', 'Jost', sans-serif; }
.total-price-text { font-weight: 700; color: #222; font-family: 'S-CoreDream', 'Jost', sans-serif; }
.text-danger { color: #f56c6c !important; }

.sharp-tag {
  border-radius: 0;
  font-weight: 700;
  text-transform: uppercase;
  font-size: 0.825rem;
}

:deep(.el-input__wrapper) {
  border-radius: 0 !important;
  box-shadow: none !important;
  border: 1px solid #dcdfe6;
  &:hover { border-color: #c5a880; }
}
</style>
