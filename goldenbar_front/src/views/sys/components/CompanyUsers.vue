<template>
<div class="user-management-section" :style="isMobile ? 'padding: 0.5rem;' : 'margin-top: 1.25rem;'">
    <div class="section-header" style="margin-bottom: 1.25rem; display: flex; gap: 0.625rem; flex-wrap: wrap;">
      <el-select v-model="selectedUserId" :placeholder="$t('sys.company.selectUserPlaceholder')" style="flex: 1; min-width: 200px;" filterable>
        <el-option
          v-for="user in availableUsers"
          :key="user.id"
          :label="`${user.name} (${user.username})`"
          :value="user.id"
          :disabled="isUserInCompany(user.id)"
        />
      </el-select>
      <el-button type="primary" :icon="Plus" @click="handleAddUser">{{ $t('sys.company.linkUser') }}</el-button>
      <el-button v-if="!isMobile" type="success" :icon="Plus" @click="handleCreateUser">{{ $t('sys.company.createNewUser') }}</el-button>
    </div>

    <base-table :data="companyUsers" border style="width: 100%" :size="isMobile ? 'small' : 'default'">
      <el-table-column prop="username" :label="$t('login.username')" :width="isMobile ? 'auto' : 150" />
      <el-table-column prop="name" :label="$t('contact.form.name')" :width="isMobile ? 'auto' : 120" />
      <el-table-column
        v-if="!isMobile"
        :label="$t('userManage.headers.lastLogin')"
        align="center"
        width="160"
        prop="lastLoginAt"
        :excel-formatter="(row) => formatDateTime(row.lastLoginAt)"
      >
        <template #default="scope">
          <span>{{ formatDateTime(scope.row.lastLoginAt) }}</span>
        </template>
      </el-table-column>
      <el-table-column align="center" :label="isMobile ? $t('sys.company.unlink') : $t('common.delete')" :width="isMobile ? 50 : 80">
        <template #default="scope">
          <el-button link class="delete-icon-btn" :icon="Delete" :type="isMobile ? 'danger' : 'default'" @click.stop="handleRemoveUser(scope.row)" />
        </template>
      </el-table-column>
    </base-table>

    <user-create-dialog
      v-model="userCreateDialogVisible"
      :default-company-id="companyId"
      default-user-type="COMPANY"
      @created="handleUserCreated"
    />
  </div>
</template>

<script setup>
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { Plus, Delete } from '@element-plus/icons-vue';
import { getCompanyUsers, addUserToCompany, removeUserFromCompany, getAvailableUsers } from '@/api/company';

const { t } = useI18n();
import { ElMessage } from 'element-plus';
import BaseTable from '@/components/BaseTable/index.vue';
import UserCreateDialog from './UserCreateDialog.vue';

const props = defineProps({
  companyId: {
    type: Number,
    required: true
  },
  isMobile: {
    type: Boolean,
    default: false
  }
});

const companyUsers = ref([]);
const availableUsers = ref([]);
const selectedUserId = ref(null);
const userCreateDialogVisible = ref(false);

const fetchAvailableUsers = async () => {
  const res = await getAvailableUsers();
  availableUsers.value = res.data;
};

const fetchCompanyUsers = async (companyId) => {
  try {
    const res = await getCompanyUsers(companyId);
    companyUsers.value = res.data;
  } catch (error) {
    ElMessage.error(t('sys.company.loadUsersFail'));
  }
};

watch(() => props.companyId, (newId) => {
  if (newId) {
    fetchCompanyUsers(newId);
    fetchAvailableUsers();
  } else {
    companyUsers.value = [];
  }
}, { immediate: true });

const isUserInCompany = (userId) => {
  return companyUsers.value.some(u => u.id === userId);
};

const handleAddUser = async () => {
  if (!selectedUserId.value) return;
  try {
    await addUserToCompany(props.companyId, selectedUserId.value);
    ElMessage.success(t('sys.company.linkSuccess'));
    selectedUserId.value = null;
    fetchCompanyUsers(props.companyId);
  } catch (error) {
    ElMessage.error(t('sys.company.linkFail'));
  }
};

const handleRemoveUser = async (user) => {
  try {
    await removeUserFromCompany(props.companyId, user.id);
    ElMessage.success(t('sys.company.unlinkSuccess'));
    fetchCompanyUsers(props.companyId);
  } catch (error) {
    ElMessage.error(t('sys.company.unlinkFail'));
  }
};

const handleCreateUser = () => {
  userCreateDialogVisible.value = true;
};

const handleUserCreated = async (userData) => {
  fetchCompanyUsers(props.companyId);
  fetchAvailableUsers();
};

const formatDateTime = (date) => {
  if (!date) return '-';
  const d = new Date(date);
  if (isNaN(d.getTime())) return '-';
  const yyyy = d.getFullYear();
  const mm = String(d.getMonth() + 1).padStart(2, '0');
  const dd = String(d.getDate()).padStart(2, '0');
  const hh = String(d.getHours()).padStart(2, '0');
  const min = String(d.getMinutes()).padStart(2, '0');
  return `${yyyy}-${mm}-${dd} ${hh}:${min}`;
};
</script>

