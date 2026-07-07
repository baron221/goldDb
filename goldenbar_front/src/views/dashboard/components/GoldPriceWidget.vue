<template>
<div v-loading="loading" class="gold-price-widget-container">
    <div v-if="priceData.announcedAt" class="announced-at-wrapper">
      <el-icon><Clock /></el-icon> {{ $t('dashboard.admin.labels.announcedAt') }}: {{ formatDateTime(priceData.announcedAt) }}
    </div>

    <el-row :gutter="24" class="price-summary-grid">
      <el-col :xs="24" :sm="12" :md="4">
        <div class="stat-card highlight-gold">
          <div class="stat-label">{{ $t('dashboard.admin.labels.pureGold24K') }}</div>
          <div class="stat-value">
            <count-to :start-val="0" :end-val="priceData.pureGold" :duration="2000" separator="," />
            <span class="unit">{{ $t('common.currency') }}</span>
          </div>
        </div>
      </el-col>
      <el-col :xs="24" :sm="12" :md="4">
        <div class="stat-card minimalist">
          <div class="stat-label">{{ $t('dashboard.admin.labels.gold18K') }}</div>
          <div class="stat-value">
            <count-to :start-val="0" :end-val="priceData.gold18K" :duration="2000" separator="," />
            <span class="unit">{{ $t('common.currency') }}</span>
          </div>
        </div>
      </el-col>
      <el-col :xs="24" :sm="12" :md="4">
        <div class="stat-card minimalist">
          <div class="stat-label">{{ $t('dashboard.admin.labels.gold14K') }}</div>
          <div class="stat-value">
            <count-to :start-val="0" :end-val="priceData.gold14K" :duration="2000" separator="," />
            <span class="unit">{{ $t('common.currency') }}</span>
          </div>
        </div>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6" :lg="6" :xl="6" class="md-mt-24">
        <div class="stat-card minimalist">
          <div class="stat-label">{{ $t('dashboard.admin.labels.platinum') }}</div>
          <div class="stat-value">
            <count-to :start-val="0" :end-val="priceData.platinum" :duration="2000" separator="," />
            <span class="unit">{{ $t('common.currency') }}</span>
          </div>
        </div>
      </el-col>
      <el-col :xs="24" :sm="12" :md="6" :lg="6" :xl="6" class="md-mt-24">
        <div class="stat-card minimalist">
          <div class="stat-label">{{ $t('dashboard.admin.labels.silver') }}</div>
          <div class="stat-value">
            <count-to :start-val="0" :end-val="priceData.silver" :duration="2000" separator="," />
            <span class="unit">{{ $t('common.currency') }}</span>
          </div>
        </div>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { getLatestGoldPrice } from '@/api/gold-price';
import { parseTime } from '@/utils';
import countTo from '@/components/vue-count-to';
import { Clock } from '@element-plus/icons-vue';

const loading = ref(false);
const priceData = ref({
  announcedAt: '',
  pureGold: 0,
  gold18K: 0,
  gold14K: 0,
  platinum: 0,
  silver: 0
});

const fetchLatestPrice = async () => {
  loading.value = true;
  try {
    const response = await getLatestGoldPrice();
    if (response.data) {
      priceData.value = response.data;
    }
  } catch (error) {
    console.error('Failed to fetch latest gold price:', error);
  } finally {
    loading.value = false;
  }
};

const formatDateTime = (date: string) => {
  if (!date) return '';
  return parseTime(date, '{y}-{m}-{d} {h}:{i}:{s}');
};

onMounted(() => {
  fetchLatestPrice();
});
</script>

<style lang="scss" scoped>
.gold-price-widget-container {
  .announced-at-wrapper {
    font-size: 0.9rem;
    color: #999;
    margin-bottom: 0.9375rem;
    display: flex;
    align-items: center;
    gap: 0.3125rem;
  }
}

.stat-card {
  background-color: #ffffff;
  border: 1px solid #eae6df;
  padding: 1.5625rem;
  border-radius: 2px;
  height: 100%;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.02);

  &:hover {
    transform: translateY(-5px);
    box-shadow: 0 12px 30px rgba(197, 168, 128, 0.1);
    border-color: #c5a880;
  }

  .stat-label {
    font-size: 0.95rem;
    color: #888888;
    text-transform: uppercase;
    letter-spacing: 1px;
    margin-bottom: 0.95rem;
  }

  .stat-value {
    font-size: 1.5rem;
    font-weight: 700;

    .unit { font-size: 0.875rem; font-weight: 400; margin-left: 0.25rem; color: #888888; }
  }

  &.highlight-gold {
    background-color: #f7f5f0;
    border-left: 4px solid #c5a880;
    .stat-value { color: #c5a880; }
  }
}

.price-summary-grid {
  .el-col {
    margin-bottom: 1.5rem;
  }
}

@media (max-width: 991px) {
  .md-mt-24 {
    margin-top: 0;
  }
}
</style>

