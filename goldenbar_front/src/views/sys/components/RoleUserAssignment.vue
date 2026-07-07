<template>
<el-card shadow="never" class="user-card no-border" :class="{ 'mobile-mode': isMobile }">

    <div v-if="!isMobile" class="detail-content-wrapper user-split-container">
      <el-row :gutter="10" align="top" justify="center">

        <el-col :xs="24" :md="11" class="mb-4 md:mb-0">
          <div class="table-title">할당된 사용자 ({{ usersAssigned.length }})</div>
          <el-input
            v-model="assignedSearchQuery"
            placeholder="이름/아이디 검색"
            size="small"
            style="margin-bottom: 0.625rem;"
            clearable
            :prefix-icon="Search"
          />
          <base-table
            :data="usersAssigned"
            height="300"
            size="small"
            style="height: 300px;"
            @selection-change="handleAssignedSelection"
            @row-dblclick="(row) => toggleUserAssignment(row.id, false)"
          >
            <el-table-column type="selection" width="40" />
            <el-table-column prop="username" label="아이디" width="100" />
            <el-table-column prop="name" label="이름" />
          </base-table>
        </el-col>

        <el-col :xs="24" :md="2" class="action-buttons mb-4 md:mb-0">
          <el-button
            type="primary"
            :icon="ArrowRight"
            circle
            :disabled="selectedAssigned.length === 0"
            @click="handleBatchUnassign"
          />
          <el-button
            type="primary"
            :icon="ArrowLeft"
            circle
            :disabled="selectedUnassigned.length === 0"
            @click="handleBatchAssign"
          />
        </el-col>

        <el-col :xs="24" :md="11">
          <div class="table-title">미할당 사용자 ({{ usersUnassigned.length }})</div>
          <el-input
            v-model="unassignedSearchQuery"
            placeholder="이름/아이디 검색"
            size="small"
            style="margin-bottom: 0.625rem;"
            clearable
            :prefix-icon="Search"
          />
          <base-table
            :data="usersUnassigned"
            height="300"
            size="small"
            style="height: 300px;"
            @selection-change="handleUnassignedSelection"
            @row-dblclick="(row) => toggleUserAssignment(row.id, true)"
          >
            <el-table-column type="selection" width="40" />
            <el-table-column prop="username" label="아이디" width="100" />
            <el-table-column prop="name" label="이름" />
          </base-table>
        </el-col>
      </el-row>
    </div>

    <div v-else class="mobile-user-assignment">
      <div class="table-title">할당된 사용자 ({{ usersAssigned.length }})</div>
      <el-input
        v-model="assignedSearchQuery"
        placeholder="검색"
        size="small"
        clearable
        :prefix-icon="Search"
      />
      <base-table
        :data="usersAssigned"
        height="250"
        size="small"
        style="height: 250px;"
        @row-dblclick="(row) => toggleUserAssignment(row.id, false)"
      >
        <el-table-column prop="username" label="아이디" width="100" />
        <el-table-column prop="name" label="이름" />
        <el-table-column width="60" align="center">
          <template #default="scope">
            <el-button type="danger" :icon="Delete" link @click="toggleUserAssignment(scope.row.id, false)" />
          </template>
        </el-table-column>
      </base-table>

      <div class="table-title" style="margin-top: 1rem;">미할당 사용자 ({{ usersUnassigned.length }})</div>
      <el-input
        v-model="unassignedSearchQuery"
        placeholder="검색"
        size="small"
        clearable
        :prefix-icon="Search"
      />
      <base-table
        :data="usersUnassigned"
        height="250"
        size="small"
        style="height: 250px;"
        @row-dblclick="(row) => toggleUserAssignment(row.id, true)"
      >
        <el-table-column prop="username" label="아이디" width="100" />
        <el-table-column prop="name" label="이름" />
        <el-table-column width="60" align="center">
          <template #default="scope">
            <el-button type="primary" :icon="Plus" link @click="toggleUserAssignment(scope.row.id, true)" />
          </template>
        </el-table-column>
      </base-table>
    </div>
  </el-card>
</template>

<script setup>
import { ref, computed, watch } from 'vue';
import { useMobile } from '@/hooks/useMobile';
import { getRoleUsers, assignUsers } from '@/api/role';
import { ElMessage } from 'element-plus';
import { Search, ArrowRight, ArrowLeft, Delete, Plus } from '@element-plus/icons-vue';
import { hasActionPermission } from '@/utils/permission';
import BaseTable from '@/components/BaseTable/index.vue';

const props = defineProps({
  roleKey: {
    type: String,
    required: true
  },
  roleName: {
    type: String,
    required: true
  }
});

const { isMobile } = useMobile();

const userData = ref([]);
const assignedUsers = ref([]);
const selectedAssigned = ref([]);
const selectedUnassigned = ref([]);
const assignedSearchQuery = ref('');
const unassignedSearchQuery = ref('');

const loadRoleUsers = async () => {
  try {
    const res = await getRoleUsers(props.roleKey);
    userData.value = res.data;
    assignedUsers.value = res.data.filter(u => u.isAssigned).map(u => u.id);
    selectedAssigned.value = [];
    selectedUnassigned.value = [];
    assignedSearchQuery.value = '';
    unassignedSearchQuery.value = '';
  } catch (error) {
    ElMessage.error('사용자 데이터를 불러오는 데 실패했습니다.');
  }
};

const usersAssigned = computed(() => {
  let list = userData.value.filter(user => assignedUsers.value.includes(user.id));
  if (assignedSearchQuery.value) {
    const q = assignedSearchQuery.value.toLowerCase();
    list = list.filter(user =>
      user.username?.toLowerCase().includes(q) ||
      user.name?.toLowerCase().includes(q)
    );
  }
  return list;
});

const usersUnassigned = computed(() => {
  let list = userData.value.filter(user => !assignedUsers.value.includes(user.id));
  if (unassignedSearchQuery.value) {
    const q = unassignedSearchQuery.value.toLowerCase();
    list = list.filter(user =>
      user.username?.toLowerCase().includes(q) ||
      user.name?.toLowerCase().includes(q)
    );
  }
  return list;
});

watch(() => props.roleKey, (newKey) => {
  if (newKey) {
    loadRoleUsers();
  } else {
    userData.value = [];
    assignedUsers.value = [];
  }
}, { immediate: true });

const toggleUserAssignment = async (userId, assign) => {
  if (!hasActionPermission('save')) {
    ElMessage.error('저장 권한이 없습니다');
    return;
  }

  if (assign) {
    if (!assignedUsers.value.includes(userId)) {
      assignedUsers.value.push(userId);
    }
  } else {
    const index = assignedUsers.value.indexOf(userId);
    if (index > -1) {
      assignedUsers.value.splice(index, 1);
    }
  }

  try {
    await assignUsers({
      roleKey: props.roleKey,
      userIds: assignedUsers.value
    });
    ElMessage.success(assign ? '사용자가 할당되었습니다' : '할당이 해제되었습니다');
    selectedAssigned.value = [];
    selectedUnassigned.value = [];
  } catch (error) {
    ElMessage.error('사용자 할당 상태 변경에 실패했습니다');
    loadRoleUsers();
  }
};

const handleAssignedSelection = (val) => {
  selectedAssigned.value = val.map(u => u.id);
};

const handleUnassignedSelection = (val) => {
  selectedUnassigned.value = val.map(u => u.id);
};

const handleBatchAssign = async () => {
  if (selectedUnassigned.value.length === 0) return;
  assignedUsers.value = [...new Set([...assignedUsers.value, ...selectedUnassigned.value])];
  await saveBatchUserAssignment('할당');
  selectedUnassigned.value = [];
};

const handleBatchUnassign = async () => {
  if (selectedAssigned.value.length === 0) return;
  assignedUsers.value = assignedUsers.value.filter(id => !selectedAssigned.value.includes(id));
  await saveBatchUserAssignment('해제');
  selectedAssigned.value = [];
};

const saveBatchUserAssignment = async (actionLabel) => {
  if (!hasActionPermission('save')) {
    ElMessage.error('저장 권한이 없습니다');
    return;
  }
  try {
    await assignUsers({
      roleKey: props.roleKey,
      userIds: assignedUsers.value
    });
    ElMessage.success(`선택한 사용자가 ${actionLabel}되었습니다`);
  } catch (error) {
    ElMessage.error(`${actionLabel} 상태 반영에 실패했습니다`);
    loadRoleUsers();
  }
};

defineExpose({
  loadRoleUsers,
  assignedUsers
});
</script>

<style scoped>
.no-border {
  border: none !important;
}
.user-card {
  margin-bottom: 0;
}
.detail-content-wrapper {
  padding: 0;
}
.user-split-container {
  padding: 0.625rem;
}
.table-title {
  font-weight: bold;
  margin-bottom: 0.625rem;
  color: #606266;
  text-align: center;
}
.action-buttons {
  display: flex;
  flex-direction: row;
  justify-content: center;
  align-items: center;
  gap: 0.625rem;
  align-self: center;
}
.action-buttons .el-button + .el-button {
  margin-left: 0 !important;
}
@media (min-width: 992px) {
  .action-buttons {
    flex-direction: column;
    gap: 0;
  }
  .action-buttons .el-button + .el-button {
    margin-top: 0.625rem;
  }
}
.mobile-user-assignment {
  padding: 0.5rem;
}
</style>

