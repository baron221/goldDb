<template>
<div class="partner-retailer-container app-container">
    <div class="section-header luxury">
      <h2 class="section-title">{{ $t('admin.partner.title') }}</h2>
      <div class="header-actions">
        <el-button :icon="Refresh" circle @click="fetchData" :loading="loading" />
      </div>
    </div>

    <partner-retailer-summary ref="summaryRef" />

    <el-card shadow="never" class="table-card-luxury">
      <div class="filter-bar">
        <el-radio-group v-model="managementTypeFilter" size="small" class="sharp-radio-group" @change="handleTypeChange">
          <el-radio-button value="ALL">{{ $t('admin.partner.filters.typeAll') }}</el-radio-button>
          <el-radio-button value="DIRECT">{{ $t('admin.partner.filters.typeDirect') }}</el-radio-button>
          <el-radio-button value="PARTNER">{{ $t('admin.partner.filters.typePartner') }}</el-radio-button>
        </el-radio-group>

        <el-input
          v-model="searchQuery"
          :placeholder="$t('admin.partner.filters.searchPlaceholder')"
          prefix-icon="Search"
          clearable
          class="sharp-input"
          style="width: 260px; margin-left: 1.25rem;"
        />
      </div>

      <base-table
        v-loading="loading"
        :data="filteredRetailers"
        border
        stripe
        size="small"
        class="sharp-table"
        :header-cell-style="{ backgroundColor: '#fbfaf7', color: '#222', fontWeight: '700', textTransform: 'uppercase', fontSize: '11px', letterSpacing: '0.5px' }"
      >
        <el-table-column :label="$t('admin.partner.table.type')" width="100" align="center" prop="isDirectManagement" :excel-formatter="typeFormatter">
          <template #default="{row}">
            <el-tag v-if="row.isDirectManagement" type="warning" effect="dark" size="small" class="sharp-tag">{{ $t('admin.partner.table.direct') }}</el-tag>
            <el-tag v-else type="info" effect="plain" size="small" class="sharp-tag">{{ $t('admin.partner.table.partner') }}</el-tag>
          </template>
        </el-table-column>

        <el-table-column :label="$t('admin.partner.table.info')" min-width="200" prop="companyName" :excel-formatter="infoFormatter">
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

        <el-table-column :label="$t('admin.partner.table.stock')" align="center">
          <el-table-column :label="$t('admin.partner.table.stockQty')" prop="totalStockCount" width="100" align="right" :excel-formatter="(row) => `${formatNumber(row.totalStockCount)} 개`">
            <template #default="{row}">
              <span class="num-bold">{{ formatNumber(row.totalStockCount) }}</span><span class="unit-inline">개</span>
            </template>
          </el-table-column>
          <el-table-column :label="$t('admin.partner.table.stockWeight')" prop="totalStockWeight" width="120" align="right" :excel-formatter="(row) => `${row.totalStockWeight.toFixed(2)} g`">
            <template #default="{row}">
              <span class="num-bold">{{ row.totalStockWeight.toFixed(2) }}</span><span class="unit-inline">g</span>
            </template>
          </el-table-column>
        </el-table-column>

        <el-table-column :label="$t('admin.partner.table.monthlySales')" align="center">
          <el-table-column :label="$t('admin.partner.table.count')" prop="monthlyOrderCount" width="90" align="right" />
          <el-table-column :label="$t('admin.partner.table.amount')" prop="monthlyOrderAmount" width="140" align="right" :excel-formatter="(row) => `₩${formatPrice(row.monthlyOrderAmount)}` ">
            <template #default="{row}">
              <span class="price-text">₩{{ formatPrice(row.monthlyOrderAmount) }}</span>
            </template>
          </el-table-column>
        </el-table-column>

        <el-table-column :label="$t('admin.partner.table.totalPerformance')" align="center">
          <el-table-column :label="$t('admin.partner.table.totalCount')" prop="totalOrderCount" width="100" align="right" />
          <el-table-column :label="$t('admin.partner.table.totalAmount')" prop="totalOrderAmount" width="160" align="right" :excel-formatter="(row) => `₩${formatPrice(row.totalOrderAmount)}` ">
            <template #default="{row}">
              <span class="total-price-text">₩{{ formatPrice(row.totalOrderAmount) }}</span>
            </template>
          </el-table-column>
        </el-table-column>

        <el-table-column :label="$t('admin.partner.table.pendingOrder')" width="110" align="center" prop="pendingOrderCount" :excel-formatter="(row) => `${row.pendingOrderCount}건` ">
          <template #default="{row}">
            <el-tag :type="row.pendingOrderCount > 5 ? 'danger' : 'warning'" effect="plain" class="sharp-tag">
              {{ row.pendingOrderCount }}{{ $t('admin.partner.table.pendingUnit') }}
            </el-tag>
          </template>
        </el-table-column>

      </base-table>
    </el-card>
  </div>
</template>

<script setup>
import { useMobile } from '@/hooks/useMobile';
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { getPartnerRetailerStats } from '@/api/dashboard';
import { Refresh } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import { formatPrice } from '@/utils/format';
import BaseTable from '@/components/BaseTable/index.vue';
import PartnerRetailerSummary from '@/components/PartnerRetailerSummary.vue';
const { isMobile } = useMobile();

const router = useRouter();
const loading = ref(false);
const retailerStats = ref([]);
const searchQuery = ref('');
const managementTypeFilter = ref('ALL');
const summaryRef = ref(null);

const fetchData = async () => {
  loading.value = true;
  try {
    const res = await getPartnerRetailerStats();
    retailerStats.value = res.data;
  } catch (error) {
    console.error('Failed to fetch partner stats:', error);
    ElMessage.error('데이터를 불러오는 데 실패했습니다.');
  } finally {
    loading.value = false;
  }

  if (summaryRef.value) {
    summaryRef.value.fetchData();
  }
};

const filteredRetailers = computed(() => {
  let list = retailerStats.value;

  if (managementTypeFilter.value === 'DIRECT') {
    list = list.filter(r => r.isDirectManagement);
  } else if (managementTypeFilter.value === 'PARTNER') {
    list = list.filter(r => !r.isDirectManagement);
  }

  const query = searchQuery.value.toLowerCase();
  if (query) {
    list = list.filter(r =>
      r.companyName.toLowerCase().includes(query) ||
      r.ceo.toLowerCase().includes(query)
    );
  }

  return list;
});

const directRetailersList = computed(() => retailerStats.value.filter(r => r.isDirectManagement));
const partnerRetailersList = computed(() => retailerStats.value.filter(r => !r.isDirectManagement));

const categorizedStats = computed(() => {

  const calc = (list) => ({
    count: list.length,
    stockCount: list.reduce((sum, r) => sum + r.totalStockCount, 0),
    stockWeight: list.reduce((sum, r) => sum + r.totalStockWeight, 0),
    monthlyAmount: list.reduce((sum, r) => sum + r.monthlyOrderAmount, 0),
    totalAmount: list.reduce((sum, r) => sum + r.totalOrderAmount, 0),
    pendingCount: list.reduce((sum, r) => sum + r.pendingOrderCount, 0)
  });

  return {
    total: calc(retailerStats.value),
    direct: calc(directRetailersList.value),
    partner: calc(partnerRetailersList.value)
  };
});

const formatNumber = (n) => (n || 0).toLocaleString();

const handleTypeChange = () => {

};

const typeFormatter = (row) => row.isDirectManagement ? '직영' : '협력사';
const infoFormatter = (row) => `${row.companyName}\n${row.ceo} | ${row.region}`;

const viewRetailerDetails = (row) => {

  router.push({
    path: '/admin/logistics-approval',
    query: {
      companyId: row.companyId,
      logisticsCompanyId: row.logisticsCompanyId
    }
  });
};

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

.summary-row {
  margin-bottom: 1.5625rem;
}

.summary-group-box {
  border: 1px solid #eae6df;
  border-radius: 2px;
  padding: 1.25rem;
  box-shadow: 0 4px 15px rgba(0,0,0,0.02);
  height: 100%;

  .group-title {
    font-size: 0.8875rem;
    font-weight: 800;
    color: #222;
    text-transform: uppercase;
    letter-spacing: 1.5px;
    margin-bottom: 0.9375rem;
    padding-bottom: 0.625rem;
    border-bottom: 1px solid #f7f5f0;
  }

  .metrics-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 0.9375rem;

    .m-item {
      display: flex;
      flex-direction: column;

      .m-label {
        font-size: 0.825rem;
        font-weight: 600;
        color: #bbb;
        text-transform: uppercase;
        margin-bottom: 0.125rem;
      }

      .m-value {
        font-size: 0.9375rem;
        font-weight: 700;
        color: #444;
        font-family: 'S-CoreDream','Jost', sans-serif;

        small { font-size: 0.825rem; font-weight: 400; margin-left: 0.125rem; color: #999; }
        &.gold { color: #c5a880; }
        &.blue { color: #409eff; }
        &.danger { color: #f56c6c; }
      }
    }
  }

  &.total { border-top: 3px solid #606266; }
  &.direct { border-top: 3px solid #c5a880; background-color: #fbfaf7; }
  &.partner { border-top: 3px solid #409eff; }
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

.sharp-radio-group {
  :deep(.el-radio-button__inner) {
    border-radius: 0 !important;
    font-size: 0.8875rem;
    font-weight: 700;
    padding: 0.5rem 1.25rem;
    text-transform: uppercase;
    letter-spacing: 0.5px;
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

.num-bold { font-weight: 700; color: #444; font-family: 'S-CoreDream', 'Jost', sans-serif; }
.unit-inline { font-size: 0.8875rem; color: #999; margin-left: 0.125rem; }
.price-text { font-weight: 700; color: #c5a880; font-family: 'S-CoreDream', 'Jost', sans-serif; }
.total-price-text { font-weight: 700; color: #222; font-family: 'S-CoreDream', 'Jost', sans-serif; }

.sharp-tag {
  border-radius: 0;
  font-weight: 700;
  text-transform: uppercase;
  font-size: 0.825rem;
}

.sharp-btn-link {
  font-weight: 700;
  font-size: 0.95rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

:deep(.el-input__wrapper) {
  border-radius: 0 !important;
  box-shadow: none !important;
  border: 1px solid #dcdfe6;
  &:hover { border-color: #c5a880; }
}
</style>

