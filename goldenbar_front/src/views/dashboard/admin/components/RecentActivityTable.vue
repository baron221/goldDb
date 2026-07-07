<template>
<el-row :gutter="20" class="recent-activity">
    <el-col :xs="24" :lg="16" style="">
      <div class="table-box">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.admin.recentActivity.recentOrders') }}</h2>
        </div>
        <base-table :data="recentOrders" stripe style="width: 100%" size="small">
          <el-table-column :label="$t('dashboard.admin.recentActivity.orderNo')" prop="orderNo" width="150" align="center" />
          <el-table-column :label="$t('dashboard.admin.recentActivity.retailer')" prop="companyName" min-width="150" />
          <el-table-column :label="$t('dashboard.admin.recentActivity.amount')" width="130" align="right" prop="totalAmount" :excel-formatter="(row) => '₩' + formatPrice(row.totalAmount)">
            <template #default="{row}">
              <span class="amount">₩{{ formatPrice(row.totalAmount) }}</span>
            </template>
          </el-table-column>
          <el-table-column :label="$t('dashboard.admin.recentActivity.status')" width="120" align="center" prop="status">
            <template #default="{row}">
              <el-tag :type="getStatusTag(row.status)" size="small">{{ $t('order.status.' + row.status) }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column :label="$t('dashboard.admin.recentActivity.time')" width="150" align="center" prop="createdAt" :excel-formatter="(row) => formatDate(row.createdAt)">
            <template #default="{row}">
              <span class="time">{{ formatDate(row.createdAt) }}</span>
            </template>
          </el-table-column>
        </base-table>
      </div>
    </el-col>
    <el-col :xs="24" :lg="8">
      <div class="table-box">
        <div class="section-header">
          <h2 class="section-title">{{ $t('dashboard.admin.recentActivity.userActivity') }}</h2>
        </div>
        <base-table :data="recentLogins" stripe style="width: 100%" size="small">
          <el-table-column :label="$t('dashboard.admin.recentActivity.user')" :excel-formatter="(row) => row.name + ' (' + row.username + ')'">
            <template #default="{row}">
              <div class="user-info-container">
                <span class="name">{{ row.name }}</span>
                <span class="username">({{ row.username }})</span>
              </div>
            </template>
          </el-table-column>
          <el-table-column :label="$t('dashboard.admin.recentActivity.lastLogin')" align="right" :excel-formatter="(row) => formatDate(row.lastLoginAt) + ' (' + row.lastLoginIp + ')'">
            <template #default="{row}">
              <div class="login-info-container">
                <span class="time">{{ formatDate(row.lastLoginAt) }}</span>
                <span class="ip">[{{ row.lastLoginIp }}]</span>
              </div>
            </template>
          </el-table-column>
        </base-table>
      </div>
    </el-col>
  </el-row>
</template>

<script setup lang="ts">
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import BaseTable from '@/components/BaseTable/index.vue';

defineProps({
  recentOrders: { type: Array, required: true },
  recentLogins: { type: Array, required: true }
});

const formatDate = (date: string) => {
  return parseTime(date, '{y}-{m}-{d} {h}:{i}');
};

const getStatusTag = (status: string) => {
  const map: Record<string, string> = {
    'ORDERED': 'info',
    'Completed': 'success',
    'Cancelled': 'danger'
  };
  return map[status] || 'primary';
};
</script>

<style lang="scss" scoped>
.recent-activity {
  margin-bottom: 1.5625rem;
}

.table-box {
  border: 1px solid #eae6df;
  padding: 1.5625rem;
  border-radius: 2px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.02);
  height: 100%;
}

.section-header {
  margin-bottom: 1.25rem;
  .section-title {
    font-size: 0.9rem;
    font-weight: 600;

    text-transform: uppercase;
    letter-spacing: 1px;
    position: relative;
    padding-left: 0.9375rem;
    line-height: 1.2;
    &::after {
      content: '';
      position: absolute;
      left: 0;
      top: 0;
      bottom: 0;
      width: 3px;
      background-color: #c5a880;
    }
  }
}

.amount { font-weight: 600;  }
.time { color: #888; font-size: 0.8875rem; }

.user-info-container, .login-info-container {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.login-info-container {
  justify-content: flex-end;
}

.name { font-weight: 600; font-size: 0.95rem;  }
.username { font-size: 0.75rem; color: #bbb; font-family: monospace; }
.ip { font-size: 0.75rem; color: #bbb; font-family: monospace; }

@media (max-width: 768px) {
  .user-info-container, .login-info-container {
    flex-direction: column;
    align-items: flex-start;
    gap: 0;
  }

  .login-info-container {
    align-items: flex-end;
  }

  .username, .ip {
    font-size: 0.825rem;
  }
}

:deep(.el-table) {
  th.el-table__cell {
    background-color: #fbfaf7 !important;
    color: #888 !important;
    font-size: 0.825rem !important;
    text-transform: uppercase;
    letter-spacing: 1px;
  }
}
</style>

