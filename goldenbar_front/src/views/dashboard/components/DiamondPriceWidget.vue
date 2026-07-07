<template>
<div v-loading="loading" class="diamond-price-widget-container">
    <div class="widget-header">
      <div class="left-controls">
        <el-radio-group v-model="activePriceType" class="price-type-selector" size="small" @change="fetchPrices">
          <el-radio-button value="MARKET">{{ $t('dashboard.admin.labels.marketPrice') }}</el-radio-button>
          <el-radio-button value="PURCHASE">{{ $t('dashboard.admin.labels.purchasePrice') }}</el-radio-button>
          <el-radio-button value="SALE">{{ $t('dashboard.admin.labels.salePrice') }}</el-radio-button>
        </el-radio-group>
      </div>
      <div v-if="latestAnnouncedAt" class="announced-at">
        <el-icon><Clock /></el-icon> {{ $t('dashboard.admin.labels.announcedAt') }}: {{ formatDateTime(latestAnnouncedAt) }}
      </div>
    </div>

    <div class="diamond-table-wrapper">
      <base-table :data="prices" size="default" border style="width: 100%" class="custom-diamond-table">
        <el-table-column prop="size" :label="$t('dashboard.admin.labels.diamondSize')" width="100" align="center" :fixed="!isMobile ? 'left' : false" />
        <el-table-column prop="virginPrice" :label="$t('dashboard.admin.labels.virgin')" align="right" min-width="120" :excel-formatter="row => row.virginPrice?.toLocaleString()">
          <template #default="{row}">
            <count-to :start-val="0" :end-val="row.virginPrice" :duration="2000" separator="," class="price-val" />
          </template>
        </el-table-column>
        <el-table-column prop="wooshinPrice" :label="$t('dashboard.admin.labels.wooshin')" align="right" min-width="120" :excel-formatter="row => row.wooshinPrice?.toLocaleString()">
          <template #default="{row}">
            <count-to :start-val="0" :end-val="row.wooshinPrice" :duration="2000" separator="," class="price-val" />
          </template>
        </el-table-column>
        <el-table-column prop="hyundaiPrice" :label="$t('dashboard.admin.labels.hyundai')" align="right" min-width="120" :excel-formatter="row => row.hyundaiPrice?.toLocaleString()">
          <template #default="{row}">
            <count-to :start-val="0" :end-val="row.hyundaiPrice" :duration="2000" separator="," class="price-val" />
          </template>
        </el-table-column>
        <el-table-column prop="gukjePrice" :label="$t('dashboard.admin.labels.gukje')" align="right" min-width="120" :excel-formatter="row => row.gukjePrice?.toLocaleString()">
          <template #default="{row}">
            <count-to :start-val="0" :end-val="row.gukjePrice" :duration="2000" separator="," class="price-val" />
          </template>
        </el-table-column>
        <el-table-column prop="gukboPrice" :label="$t('dashboard.admin.labels.gukbo')" align="right" min-width="120" :excel-formatter="row => row.gukboPrice?.toLocaleString()">
          <template #default="{row}">
            <count-to :start-val="0" :end-val="row.gukboPrice" :duration="2000" separator="," class="price-val" />
          </template>
        </el-table-column>
        <el-table-column prop="otherPrice" :label="$t('dashboard.admin.labels.others')" align="right" min-width="120" :excel-formatter="row => row.otherPrice?.toLocaleString()">
          <template #default="{row}">
            <count-to :start-val="0" :end-val="row.otherPrice" :duration="2000" separator="," class="price-val" />
          </template>
        </el-table-column>
      </base-table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useMobile } from '@/hooks/useMobile';
import { ref, onMounted, computed } from 'vue';
import { getDiamondPrices } from '@/api/diamond-price';
import { parseTime } from '@/utils';
import countTo from '@/components/vue-count-to';
import { Clock } from '@element-plus/icons-vue';
import BaseTable from '@/components/BaseTable/index.vue';
const { isMobile } = useMobile();

const loading = ref(false);
const prices = ref([]);
const activePriceType = ref('MARKET');

const latestAnnouncedAt = computed(() => {
  if (prices.value.length === 0) return '';
  return prices.value[0].announcedAt;
});

const fetchPrices = async () => {
  loading.value = true;
  try {
    const response = await getDiamondPrices({ priceType: activePriceType.value });
    if (response.data) {
      prices.value = response.data;
    }
  } catch (error) {
    console.error('Failed to fetch diamond prices:', error);
  } finally {
    loading.value = false;
  }
};

const formatDateTime = (date: string) => {
  if (!date) return '';
  return parseTime(date, '{y}-{m}-{d} {h}:{i}');
};

onMounted(() => {
  fetchPrices();
});
</script>

<style lang="scss" scoped>
.diamond-price-widget-container {
  border: 1px solid #eae6df;
  border-radius: 2px;
  padding: 1.5rem;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.02);

  .widget-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1.25rem;

    .price-type-selector {
      :deep(.el-radio-button) {
        .el-radio-button__inner {
          border: 1px solid #eae6df !important;
          background-color: transparent;
          color: #888888;
          font-weight: 600;
          padding: 0.5rem 1.25rem;
          font-size: 0.95rem;
          text-transform: uppercase;
          letter-spacing: 1px;
          box-shadow: none !important;
        }

        &.is-active {
          .el-radio-button__inner {
            background-color: #222222 !important;
            border-color: #222222 !important;
            color: #ffffff !important;
          }
        }
      }
    }

    .announced-at {
      font-size: 0.9rem;
      color: #999;
      display: flex;
      align-items: center;
      gap: 0.3125rem;
    }

    @media (max-width: 768px) {
      flex-direction: column;
      align-items: stretch;
      gap: 0.75rem;

      .left-controls {
        width: 100%;
      }

      .price-type-selector {
        display: flex !important;
        width: 100% !important;

        :deep(.el-radio-button) {
          flex: 1 !important;
          display: inline-flex !important;

          .el-radio-button__inner {
            width: 100% !important;
            padding: 0.5rem 0 !important;
            text-align: center;
          }
        }
      }

      .announced-at {
        justify-content: flex-end;
      }
    }
  }

  .diamond-table-wrapper {
    .custom-diamond-table {
      border-color: #eae6df;

      :deep(.el-table__header-wrapper) {
        th {
          background-color: #fbfaf7;

          font-weight: 600;
          text-transform: uppercase;
          letter-spacing: 1px;
          font-size: 0.8875rem;
          height: 45px;
        }
      }

      :deep(.el-table__row) {
        td {
          padding: 0.95rem 0;
        }
      }
    }
  }

  .price-val {
    font-weight: 700;
    font-size: 1rem;

  }
}

:global(html.dark) {
  .diamond-price-widget-container {
    background-color: #1a1a1a;
    border-color: #333333;

    .price-type-selector {
      :deep(.el-radio-button) {
        .el-radio-button__inner {
          border-color: #333333 !important;
          color: #888888;
        }
        &.is-active {
          .el-radio-button__inner {
            background-color: #ffffff !important;

          }
        }
      }
    }

    .diamond-table-wrapper {
      .custom-diamond-table {
        background-color: #1a1a1a;
        color: #eeeeee;
        :deep(.el-table__header-wrapper) th {
          background-color: #222222;
          color: #ffffff;
        }
        :deep(.el-table__cell) {
          background-color: #1a1a1a;
          border-bottom-color: #333333;
        }
      }
    }

    .price-val {
      color: #ffffff;
    }
  }
}
</style>

