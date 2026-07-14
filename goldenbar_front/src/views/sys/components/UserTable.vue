<template>
<base-table
    :data="data"
    style="width: 100%"
    height="100%"
    border
    highlight-current-row
    :default-sort="{ prop: 'createdAt', order: 'descending' }"
    @row-click="onRowClick"
  >
    <el-table-column prop="avatar" width="40" align="center">
      <template #default="scope">
        <el-avatar v-if="scope.row.avatar" :size="22" :src="scope.row.avatar" />
      </template>
    </el-table-column>
    <el-table-column prop="username" :label="$t('userManage.username')" min-width="90" show-overflow-tooltip sortable />
    <el-table-column prop="name" :label="$t('userManage.name')" min-width="80" show-overflow-tooltip sortable />
    <el-table-column
      prop="userType"
      :label="$t('userManage.type')"
      min-width="95"
      align="center"
    >
      <template #default="scope">
        <div style="display: flex; flex-direction: row; gap: 0.25rem; align-items: center; justify-content: center;">
          <el-tag :type="getTagType(scope.row)" size="small" class="tiny-tag">
            {{ scope.row.userType === 'ADMIN' ? $t('userManage.admin') : getCompanyTypeName(scope.row.companyCategory) }}
          </el-tag>
          <el-tag v-if="scope.row.isDirectManagement" type="success" size="small" effect="dark" class="tiny-tag direct-badge">{{ $t('userManage.directBadge') }}</el-tag>
        </div>
      </template>
    </el-table-column>
    <el-table-column
      prop="companyName"
      :label="$t('userManage.company')"
      min-width="150"
      show-overflow-tooltip
    >
      <template #default="scope">
        <div v-if="scope.row.companyName" style="color: #606266; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
          <div style="display: flex; align-items: center; gap: 0.375rem; white-space: nowrap;">
            <span style="overflow: hidden; text-overflow: ellipsis;">{{ scope.row.companyName }}</span>
            <el-tag v-if="scope.row.companyCategory === 'RTL' && scope.row.logisticsCompanyName" type="info" size="small" class="tiny-tag" effect="plain" style="flex-shrink: 0;">
              {{ scope.row.logisticsCompanyName }}
            </el-tag>
          </div>
        </div>
        <span v-else>-</span>
      </template>
    </el-table-column>
    <el-table-column
      prop="roles"
      :label="$t('userManage.roles')"
      min-width="100"
      show-overflow-tooltip
    >
      <template #default="scope">
        <div class="role-tags">
          <el-tag
            v-for="role in scope.row.roles"
            :key="role"
            size="small"
            type="info"
            effect="plain"
            class="tiny-tag"
          >
            {{ role }}
          </el-tag>
        </div>
      </template>
    </el-table-column>
    <el-table-column
      prop="isApproved"
      :label="$t('userManage.approveStatus')"
      min-width="130"
      align="center"
    >
      <template #default="scope">
        <div v-if="scope.row.isApproved" style="display: flex; gap: 5px; justify-content: center; align-items: center;">
          <el-tag type="success" size="small" class="tiny-tag">{{ $t('userManage.approved') }}</el-tag>
          <el-button
            type="danger"
            size="small"
            plain
            @click.stop="onUnapproveClick(scope.row)"
          >
            승인 취소
          </el-button>
        </div>
        <el-button
          v-else
          type="success"
          size="small"
          @click.stop="onApproveClick(scope.row)"
        >
          {{ $t('userManage.approve') }}
        </el-button>
      </template>
    </el-table-column>
  </base-table>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import BaseTable from '@/components/BaseTable/index.vue';

const props = defineProps<{
  data: any[];
}>();

const emit = defineEmits(['row-click', 'approve', 'unapprove']);

const { t } = useI18n();

const onRowClick = (row: any) => {
  emit('row-click', row);
};

const onApproveClick = (row: any) => {
  emit('approve', row);
};

const onUnapproveClick = (row: any) => {
  emit('unapprove', row);
};

const getTagType = (row: any) => {
  if (row.userType === 'ADMIN') return 'danger';
  switch (row.companyCategory) {
    case 'MFG': return 'warning';
    case 'DCC': return 'success';
    case 'RTL': return 'primary';
    default: return 'info';
  }
};

const getCompanyTypeName = (category: string) => {
  switch (category) {
    case 'MFG': return t('userManage.manufacturer');
    case 'DCC': return t('userManage.logistics');
    case 'RTL': return t('userManage.retailer');
    default: return t('userManage.none');
  }
};
</script>

<style scoped>
.tiny-tag {
  height: 1.5em !important;
  line-height: 1.4em !important;
  padding: 0 0.6em !important;
  font-size: 0.8rem !important;
  border-radius: 0 !important;
}
.role-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 2px;
}
</style>

