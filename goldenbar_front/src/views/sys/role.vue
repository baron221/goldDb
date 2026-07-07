<template>
<div class="app-container">

    <splitpanes v-if="!isMobile" class="default-theme role-split-container">

      <pane size="25" min-size="20">
        <el-card shadow="never" class="list-card">
          <template #header>
            <div class="card-header">
              <span>권한 목록</span>
              <el-button v-permission="'create'" type="primary" size="small" @click="handleAddRole">추가</el-button>
            </div>
          </template>

          <base-table
            :data="rolesList"
            style="width: 100%"
            highlight-current-row
            @row-click="handleRowClick"
            height="100%"
          >
            <el-table-column prop="name" label="권한 명칭" show-overflow-tooltip />
            <el-table-column align="center" label="관리" width="100">
              <template #default="scope">
                <el-button  link :icon="Edit" @click.stop="handleEditRole(scope.row)" />
                <el-button v-permission="'delete'" link class="delete-icon-btn" :icon="Delete" @click.stop="handleDelete(scope.row)" />
              </template>
            </el-table-column>
          </base-table>
        </el-card>
      </pane>

      <pane size="75" min-size="40">
        <div class="detail-wrapper-pane">
          <template v-if="currentRole">
            <el-collapse v-model="activeNames">

              <el-collapse-item name="users">
                <template #title>
                  <div class="collapse-title">
                    <span>사용자 할당: <el-tag size="small">{{ currentRole.name }}</el-tag></span>
                  </div>
                </template>
                <role-user-assignment
                  :role-key="currentRole.key"
                  :role-name="currentRole.name"
                />
              </el-collapse-item>

              <el-collapse-item name="menus">
                <template #title>
                  <div class="collapse-title"><span>메뉴 설정</span></div>
                </template>
                <role-menu-settings
                  :role-key="currentRole.key"
                />
              </el-collapse-item>
            </el-collapse>
          </template>
          <el-card v-else shadow="never" class="empty-card">
            <div class="empty-state">
              <el-icon :size="50" color="#909399"><InfoFilled /></el-icon>
              <p>권한을 선택해 주세요.</p>
            </div>
          </el-card>
        </div>
      </pane>
    </splitpanes>

    <div v-else class="mobile-layout-container">

      <div v-if="!showMobileDetail" class="mobile-master-view">
        <el-card shadow="never" class="list-card">
          <template #header>
            <div class="card-header">
              <span>권한 목록</span>
              <el-button v-permission="'create'" type="primary" size="small" @click="handleAddRole">추가</el-button>
            </div>
          </template>
          <base-table :data="rolesList" style="width: 100%" highlight-current-row @row-click="handleRowClick">
            <el-table-column prop="name" label="권한 명칭" show-overflow-tooltip />
            <el-table-column align="center" label="관리" width="100">
              <template #default="scope">
                <el-button type="primary" size="small" circle :icon="Edit" @click.stop="handleEditRole(scope.row)" />
                <el-button v-permission="'delete'" link class="delete-icon-btn" :icon="Delete" @click.stop="handleDelete(scope.row)" />
              </template>
            </el-table-column>
          </base-table>
        </el-card>
      </div>

      <div v-else class="mobile-detail-view">
        <div class="mobile-detail-header">
          <el-button :icon="ArrowLeft" link @click="showMobileDetail = false">목록으로 돌아가기</el-button>
        </div>
        <div class="mobile-detail-content">
          <template v-if="currentRole">
            <el-collapse v-model="activeNames">

              <el-collapse-item name="users">
                <template #title>
                  <div class="collapse-title">
                    <span>사용자 할당: <el-tag size="small">{{ currentRole.name }}</el-tag></span>
                  </div>
                </template>
                <role-user-assignment
                  :role-key="currentRole.key"
                  :role-name="currentRole.name"
                />
              </el-collapse-item>

              <el-collapse-item name="menus">
                <template #title>
                  <div class="collapse-title"><span>메뉴 설정</span></div>
                </template>
                <role-menu-settings
                  :role-key="currentRole.key"
                />
              </el-collapse-item>
            </el-collapse>
          </template>
        </div>
      </div>
    </div>

    <role-dialog
      v-model="dialogVisible"
      :dialog-type="dialogType"
      :initial-data="role"
      @success="getRolesData"
    />
  </div>
</template>

<script setup>
import { useMobile } from '@/hooks/useMobile';
import { ref, onMounted } from 'vue';
import { getRoles, deleteRole } from '@/api/role';
import { ElMessage, ElMessageBox, ElCard, ElButton, ElTableColumn, ElIcon, ElCollapse, ElCollapseItem, ElTag } from 'element-plus';
import { InfoFilled, Edit, Delete, ArrowLeft } from '@element-plus/icons-vue';
import BaseTable from '@/components/BaseTable/index.vue';
import RoleDialog from './components/RoleDialog.vue';
import RoleUserAssignment from './components/RoleUserAssignment.vue';
import RoleMenuSettings from './components/RoleMenuSettings.vue';
import { Splitpanes, Pane } from 'splitpanes';
import 'splitpanes/dist/splitpanes.css';

const { isMobile } = useMobile();
const showMobileDetail = ref(false);

const rolesList = ref([]);
const role = ref({});
const dialogVisible = ref(false);
const dialogType = ref('new');

const currentRole = ref(null);
const activeNames = ref(['users']);

onMounted(() => {
  getRolesData();
});

const getRolesData = async () => {
  const res = await getRoles();
  rolesList.value = res.data;
};

const handleRowClick = (row) => {
  currentRole.value = row;
  if (isMobile.value) {
    showMobileDetail.value = true;
  }
};

const handleAddRole = () => {
  role.value = {
    key: '',
    name: '',
    description: ''
  };
  dialogType.value = 'new';
  dialogVisible.value = true;
};

const handleEditRole = (row) => {
  dialogType.value = 'edit';
  role.value = { ...row };
  dialogVisible.value = true;
};

const handleDelete = (row) => {
  ElMessageBox.confirm('해당 권한을 삭제하시겠습니까?', '경고', {
    confirmButtonText: '확인',
    cancelButtonText: '취소',
    type: 'warning'
  }).then(async () => {
    await deleteRole(row.id);
    ElMessage.success('성공적으로 삭제되었습니다');
    if (currentRole.value?.id === row.id) {
      currentRole.value = null;
    }
    getRolesData();
  });
};
</script>

<style scoped>
.app-container {
  padding: 1.25rem;
  height: calc(100vh - 84px);
  display: flex;
  flex-direction: column;
}

.role-split-container {
  height: 100%;
  background-color: transparent;

  :deep(.splitpanes__splitter) {
    background-color: #f0f2f5;
    box-sizing: border-box;
    position: relative;
    width: 10px;
    border-left: 1px solid #dcdfe6;
    border-right: 1px solid #dcdfe6;
    transition: background-color 0.3s;

    &:hover {
      background-color: var(--el-color-primary-light-7);
    }

    &:before {
      content: '';
      position: absolute;
      left: 50%;
      top: 50%;
      transform: translate(-50%, -50%);
      width: 2px;
      height: 30px;
      background-color: #c0c4cc;
      border-radius: 1px;
    }
  }
}

.detail-wrapper-pane {
  height: 100%;
  overflow-y: auto;
  padding-left: 0.625rem;
}

.mobile-layout-container {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.mobile-master-view, .mobile-detail-view {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.mobile-detail-header {
  padding: 0.5rem 1rem;
  background-color: #fbfaf7;
  border-bottom: 1px solid #eae6df;
  margin-bottom: 0.5rem;
}

.mobile-detail-content {
  flex: 1;
  overflow-y: auto;
  padding: 0.5rem;
}

.list-card {
  height: 100%;
  display: flex;
  flex-direction: column;
  :deep(.el-card__body) {
    flex: 1;
    padding: 0;
    min-height: 0;
  }
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.collapse-title {
  font-weight: bold;
  font-size: 0.875rem;
  padding-left: 1rem;
}
.empty-card {
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 400px;
}
.empty-state {
  text-align: center;
  color: #909399;
}
.empty-state p {
  margin-top: 0.9375rem;
}

:deep(.el-button.is-link) {
  border: none !important;
  outline: none !important;
  box-shadow: none !important;

  .el-icon {
    font-size: 15px !important;
  }

  &:focus,
  &:focus-visible,
  &:active {
    outline: none !important;
    box-shadow: none !important;
    border: none !important;
    background: transparent !important;
  }
}
</style>

