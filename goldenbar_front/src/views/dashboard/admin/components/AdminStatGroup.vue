<template>
<el-row :gutter="20" class="admin-stat-group">
    <el-col :xs="24" :sm="12" :lg="3" class="stat-col">
      <div class="stat-card" @click="handleCardClick('total')">
        <div class="stat-content">
          <div class="stat-label">{{ $t('dashboard.admin.users') }}</div>
          <div class="stat-value">{{ data.totalUsers }}</div>
        </div>
      </div>
    </el-col>
    <el-col :xs="24" :sm="12" :lg="3" class="stat-col">
      <div class="stat-card" @click="handleCardClick('companies')">
        <div class="stat-content">
          <div class="stat-label">{{ $t('dashboard.admin.companies') }}</div>
          <div class="stat-value">{{ data.totalCompanies }}</div>
        </div>
      </div>
    </el-col>
    <el-col :xs="24" :sm="12" :lg="3" class="stat-col">
      <div class="stat-card" @click="handleCardClick('products')">
        <div class="stat-content">
          <div class="stat-label">{{ $t('dashboard.admin.products') }}</div>
          <div class="stat-value">{{ data.totalProducts }}</div>
        </div>
      </div>
    </el-col>
    <el-col :xs="24" :sm="12" :lg="3" class="stat-col">
      <div class="stat-card highlight-gold" @click="handleCardClick('revenue')">
        <div class="stat-content">
          <div class="stat-label">{{ $t('dashboard.admin.revenue') }}</div>
          <div class="stat-value">₩{{ formatPrice(data.totalRevenue) }}</div>
        </div>
      </div>
    </el-col>
    <el-col :xs="24" :sm="12" :lg="3" class="stat-col">
      <div class="stat-card" @click="handleCardClick('orders')">
        <div class="stat-content">
          <div class="stat-label">{{ $t('dashboard.admin.orders') }}</div>
          <div class="stat-value">{{ data.totalOrders }}</div>
        </div>
      </div>
    </el-col>
    <el-col :xs="24" :sm="12" :lg="3" class="stat-col">
      <div class="stat-card highlight-blue" @click="handleCardClick('pending')">
        <div class="stat-content">
          <div class="stat-label">{{ $t('dashboard.admin.pendingApproval') }}</div>
          <div class="stat-value">{{ data.pendingApprovalCount }}</div>
        </div>
      </div>
    </el-col>

    <el-col :xs="24" :sm="12" :lg="3" class="stat-col">
      <div class="stat-card highlight-warning" @click="handleCardClick('unassigned_company')">
        <div class="stat-content">
          <div class="stat-label">{{ $t('dashboard.admin.unassignedCompanyUsers') }}</div>
          <div class="stat-value">{{ data.unassignedCompanyUserCount }}</div>
        </div>
      </div>
    </el-col>
    <el-col :xs="24" :sm="12" :lg="3" class="stat-col">
      <div class="stat-card highlight-danger" @click="handleCardClick('unassigned_logistics')">
        <div class="stat-content">
          <div class="stat-label">{{ $t('dashboard.admin.unassignedLogisticsRetailers') }}</div>
          <div class="stat-value">{{ data.unassignedLogisticsRetailerCount }}</div>
        </div>
      </div>
    </el-col>
  </el-row>
</template>

<script setup lang="ts">
import { formatPrice } from '@/utils/format';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';

const { t } = useI18n();
const router = useRouter();

defineProps({
  data: {
    type: Object,
    required: true
  }
});

const handleCardClick = (type: string) => {
  switch (type) {
    case 'unassigned_company':
      router.push({ path: '/sys/user', query: { isUnassignedOnly: 'true' }});
      break;
    case 'unassigned_logistics':
      router.push({ path: '/sys/user', query: { isLogisticsUnassigned: 'true' }});
      break;
    case 'total':
      router.push({ path: '/sys/user' });
      break;
    case 'companies':
      router.push({ path: '/sys/company' });
      break;
    case 'products':
      router.push({ path: '/product/index' });
      break;
    case 'orders':
      router.push({ path: '/order/index' });
      break;
    case 'pending':
      router.push({ path: '/admin/order-approval' });
      break;
  }
};
</script>

<style lang="scss" scoped>
.admin-stat-group {
  margin-bottom: 1.5625rem;
}

.stat-card {
  background-color: #ffffff;
  border: 1px solid #eae6df;
  padding: 0.9375rem 1.25rem;
  border-radius: 2px;
  height: 100%;
  display: flex;
  align-items: center;
  gap: 0.9375rem;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.02);
  transition: all 0.3s ease;

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 20px rgba(0, 0, 0, 0.05);
    border-color: #c5a880;
  }

  .stat-content {
    flex: 1;
  }

  .stat-label {
    font-size: 0.825rem;
    font-weight: 800;
    color: #bbbbbb;
    letter-spacing: 1.5px;
    text-transform: uppercase;
    margin-bottom: 0.25rem;
  }

  .stat-value {
    font-size: 1.125rem;
    font-weight: 700;

    font-family: 'S-CoreDream','Jost', sans-serif;
  }

  &.highlight-gold {
    border-left: 3px solid #c5a880;
    background-color: #fbfaf7;
    .stat-value { color: #c5a880; }
  }

  &.highlight-blue {
    border-left: 3px solid #409eff;
    .stat-value { color: #409eff; }
  }

  &.highlight-warning {
    border-left: 3px solid #e6a23c;
    .stat-value { color: #e6a23c; }
  }

  &.highlight-danger {
    border-left: 3px solid #f56c6c;
    .stat-value { color: #f56c6c; }
  }
}

@media (max-width: 1200px) {
  .stat-col {
    margin-bottom: 1.25rem;
  }
}
</style>

