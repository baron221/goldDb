<template>
<div class="app-container">

    <div v-if="!isMobile" class="split-container" ref="splitContainer">

      <div class="left-pane" :style="{ width: leftWidth + 'px' }">
        <el-card shadow="never" class="left-list-card">
          <template #header>
            <div class="card-header">
              <span>{{ $t('userManage.listTitle') }}</span>
              <div class="header-actions">
                <el-button v-permission="'create'" type="primary" size="small" @click="handleCreate">{{ $t('userManage.addUser') }}</el-button>
                <el-button :icon="Refresh" size="small" circle @click="fetchUsers" />
              </div>
            </div>
            <user-filter :query="listQuery" @update:query="(val) => Object.assign(listQuery, val)" @filter="handleFilter" style="margin-top: 15px;" />
          </template>

          <user-table :data="userList" @row-click="handleRowClick" @approve="handleApproveRow" @unapprove="handleUnapproveRow" v-loading="loading" />
        </el-card>
      </div>

      <div class="pane-resizer" @mousedown="startResizing"></div>

      <div class="right-pane">
        <user-detail-pane
          :user="userDetail"
          :roles="allRoles"
          @update:user="(val) => Object.assign(userDetail, val)"
          @save="handleSave"
          @approve="handleApprove"
          @unapprove="handleUnapprove"
          @delete="() => removeUser(userDetail.id)"
          @create-company="companyCreateVisible = true"
          @open-postcode="openPostcode"
          @add-email="addEmail"
          @remove-email="removeEmail"
          @set-primary-email="setPrimaryEmail"
          @add-phone="addPhone"
          @remove-phone="removePhone"
          @set-primary-phone="setPrimaryPhone"
          @remove-photo="removeGalleryPhoto"
          @move-photo="handleMovePhoto"
          @set-main-photo="setMainPhoto"
          @avatar-change="handleAvatarChange"
          @new-photo="handleNewGalleryPhoto"
        />
      </div>
    </div>

    <div v-else class="mobile-container">
      <el-card shadow="never" class="mobile-list-card" v-if="!showMobileDetail">
        <template #header>
          <div class="card-header">
            <span>{{ $t('userManage.listTitle') }}</span>
            <el-button v-permission="'create'" type="primary" size="small" @click="handleCreate">{{ $t('userManage.addUser') }}</el-button>
          </div>
          <user-filter :query="listQuery" @update:query="(val) => Object.assign(listQuery, val)" @filter="handleFilter" style="margin-top: 15px;" />
        </template>
        <user-table :data="userList" @row-click="handleRowClick" @approve="handleApproveRow" @unapprove="handleUnapproveRow" v-loading="loading" />
      </el-card>

      <div v-else class="mobile-detail-wrapper">
        <div class="mobile-detail-header">
          <el-button :icon="ArrowLeft" circle @click="showMobileDetail = false" />
          <span class="header-title">{{ userDetail.name || $t('userManage.detailFallback') }}</span>
        </div>
        <user-detail-pane
          :user="userDetail"
          :roles="allRoles"
          @update:user="(val) => Object.assign(userDetail, val)"
          @save="handleSave"
          @approve="handleApprove"
          @unapprove="handleUnapprove"
          @delete="() => removeUser(userDetail.id)"
          @create-company="companyCreateVisible = true"
          @open-postcode="openPostcode"
          @add-email="addEmail"
          @remove-email="removeEmail"
          @set-primary-email="setPrimaryEmail"
          @add-phone="addPhone"
          @remove-phone="removePhone"
          @set-primary-phone="setPrimaryPhone"
          @remove-photo="removeGalleryPhoto"
          @move-photo="handleMovePhoto"
          @set-main-photo="setMainPhoto"
          @avatar-change="handleAvatarChange"
          @new-photo="handleNewGalleryPhoto"
        />
      </div>
    </div>

    <user-create-dialog v-model="createDialogVisible" @created="handleUserCreated" />
    <company-create-dialog v-model="companyCreateVisible" @created="handleCompanyCreated" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive, watch, nextTick } from 'vue';
import { useMobile } from '@/hooks/useMobile';
import { useI18n } from 'vue-i18n';
import { ElMessage, ElMessageBox } from 'element-plus';
import { Refresh, ArrowLeft } from '@element-plus/icons-vue';
import { createUser, updateUser, approveUser, unapproveUser } from '@/api/user';

import UserCreateDialog from './components/UserCreateDialog.vue';
import CompanyCreateDialog from '@/components/CompanyCreateDialog/index.vue';
import UserFilter from './components/UserFilter.vue';
import UserTable from './components/UserTable.vue';
import UserDetailPane from './components/UserDetailPane.vue';
import { useUser } from './composables/useUser';

const { isMobile } = useMobile();
const { t } = useI18n();
const {
  userList,
  loading,
  listQuery,
  allRoles,
  allCompanies,
  fetchUsers,
  handleFilter,
  fetchUserDetail,
  approveUserRow,
  removeUser,
  fetchInitialData
} = useUser();

const showMobileDetail = ref(false);
const createDialogVisible = ref(false);
const companyCreateVisible = ref(false);

const userDetail = reactive<any>({
  id: undefined,
  username: '',
  password: '',
  name: '',
  ssn: '',
  zipCode: '',
  addressBase: '',
  addressDetail: '',
  avatar: '',
  avatarId: null,
  companyId: null,
  emails: [],
  phones: [],
  photos: [],
  roles: []
});

const leftWidth = ref(450);
const splitContainer = ref<HTMLElement | null>(null);
let isResizing = false;

const startResizing = () => {
  isResizing = true;
  document.addEventListener('mousemove', handleMouseMove);
  document.addEventListener('mouseup', stopResizing);
  document.body.style.cursor = 'col-resize';
};

const handleMouseMove = (e: MouseEvent) => {
  if (!isResizing || !splitContainer.value) return;
  const containerRect = splitContainer.value.getBoundingClientRect();
  const newWidth = e.clientX - containerRect.left;
  if (newWidth > 300 && newWidth < containerRect.width - 400) {
    leftWidth.value = newWidth;
  }
};

const stopResizing = () => {
  isResizing = false;
  document.removeEventListener('mousemove', handleMouseMove);
  document.removeEventListener('mouseup', stopResizing);
  document.body.style.cursor = '';
};

const handleRowClick = async (row: any) => {
  const data = await fetchUserDetail(row.id);
  if (data) {
    Object.assign(userDetail, data);

    if (data.userRoles && data.userRoles.length > 0) {
      userDetail.roles = data.userRoles.map((ur: any) => ur.role?.key).filter(Boolean);
    } else if (data.roles) {
      userDetail.roles = data.roles;
    } else {
      userDetail.roles = [];
    }
    if (isMobile.value) showMobileDetail.value = true;
  }
};

const handleCreate = () => {
  createDialogVisible.value = true;
};

const handleUserCreated = (user: any) => {
  fetchUsers();
  handleRowClick(user);
};

const handleCompanyCreated = () => {
  fetchInitialData();
};

const handleSave = async () => {
  try {
    const payload = { ...userDetail };
    await updateUser(userDetail.id, payload);
    ElMessage.success(t('common.save'));
    fetchUsers();
  } catch (error: any) {
    ElMessage.error(t('common.save') + ': ' + (error.response?.data?.message || error.message));
  }
};

const handleApprove = async () => {
  if (!userDetail.id) return;
  try {
    await approveUser(userDetail.id);
    ElMessage.success(t('userManage.approveSuccess'));
    userDetail.isApproved = true;
    fetchUsers();
  } catch (error: any) {
    ElMessage.error(t('userManage.approveFail') + ': ' + (error.response?.data?.message || error.message));
  }
};

const handleApproveRow = async (row: any) => {
  try {
    await ElMessageBox.confirm(`'${row.name}' (${row.username}) ${t('userManage.approveConfirmMessage')}`, t('userManage.approveConfirmTitle'), {
      confirmButtonText: t('userManage.approve'),
      cancelButtonText: t('common.cancel'),
      type: 'warning'
    });
  } catch {
    return;
  }

  const success = await approveUserRow(row.id);
  if (success && userDetail.id === row.id) {
    userDetail.isApproved = true;
  }
};

const handleUnapprove = async () => {
  if (!userDetail.id) return;
  try {
    await unapproveUser(userDetail.id);
    ElMessage.success('승인이 취소되었습니다.');
    userDetail.isApproved = false;
    fetchUsers();
  } catch (error: any) {
    ElMessage.error('승인 취소 실패: ' + (error.response?.data?.message || error.message));
  }
};

const handleUnapproveRow = async (row: any) => {
  try {
    await ElMessageBox.confirm(`'${row.name}' (${row.username}) 사용자의 승인을 취소하시겠습니까?`, '승인 취소 확인', {
      confirmButtonText: '승인 취소',
      cancelButtonText: t('common.cancel'),
      type: 'warning'
    });
  } catch {
    return;
  }

  try {
    await unapproveUser(row.id);
    ElMessage.success('승인이 취소되었습니다.');
    if (userDetail.id === row.id) {
      userDetail.isApproved = false;
    }
    fetchUsers();
  } catch (error: any) {
    ElMessage.error('승인 취소 실패: ' + (error.response?.data?.message || error.message));
  }
};

const addEmail = () => {
  userDetail.emails.push({ email: '', isPrimary: userDetail.emails.length === 0 });
};
const removeEmail = (index: number) => {
  userDetail.emails.splice(index, 1);
};
const setPrimaryEmail = (index: number) => {
  userDetail.emails.forEach((e: any, i: number) => { e.isPrimary = i === index; });
};

const addPhone = () => {
  userDetail.phones.push({ phoneNumber: '', isPrimary: userDetail.phones.length === 0 });
};
const removePhone = (index: number) => {
  userDetail.phones.splice(index, 1);
};
const setPrimaryPhone = (index: number) => {
  userDetail.phones.forEach((p: any, i: number) => { p.isPrimary = i === index; });
};

const handleAvatarChange = (attachment: any) => {
  if (attachment) {
    userDetail.avatar = attachment.filePath;
    userDetail.avatarId = attachment.id;
  } else {
    userDetail.avatar = '';
    userDetail.avatarId = null;
  }
};

const handleNewGalleryPhoto = (attachment: any) => {
  if (attachment) {
    userDetail.photos.push({
      photoUrl: attachment.filePath,
      attachmentId: attachment.id,
      sortOrder: userDetail.photos.length,
      isMain: false
    });
  }
};

const removeGalleryPhoto = (index: number) => {
  userDetail.photos.splice(index, 1);
};

const handleMovePhoto = ({ index, dir }: { index: number, dir: 'up' | 'down' }) => {
  if (dir === 'up' && index > 0) {
    const temp = userDetail.photos[index];
    userDetail.photos[index] = userDetail.photos[index - 1];
    userDetail.photos[index - 1] = temp;
  } else if (dir === 'down' && index < userDetail.photos.length - 1) {
    const temp = userDetail.photos[index];
    userDetail.photos[index] = userDetail.photos[index + 1];
    userDetail.photos[index + 1] = temp;
  }
};

const setMainPhoto = (index: number) => {
  userDetail.photos.forEach((p: any, i: number) => { p.isMain = i === index; });
};

const openPostcode = () => {
  const Postcode = (window as any).daum.Postcode;
  new Postcode({
    oncomplete: (data: any) => {
      let fullAddress = data.address;
      let extraAddress = '';
      if (data.addressType === 'R') {
        if (data.bname !== '') extraAddress += data.bname;
        if (data.buildingName !== '') extraAddress += (extraAddress !== '' ? `, ${data.buildingName}` : data.buildingName);
        fullAddress += (extraAddress !== '' ? ` (${extraAddress})` : '');
      }
      userDetail.zipCode = data.zonecode;
      userDetail.addressBase = fullAddress;
    }
  }).open();
};

onMounted(() => {
  fetchInitialData();
  fetchUsers();
});
</script>

<style lang="scss" scoped>
.app-container {
  padding: 0.9375rem 1.25rem 1.25rem;
  height: calc(100vh - 50px - 20px);
  box-sizing: border-box;
  overflow: hidden;
}

.split-container {
  display: flex;
  height: 100%;
  width: 100%;
  overflow: hidden;
}

.left-pane {
  height: 100%;
  min-width: 300px;
  display: flex;
  flex-direction: column;
}

.pane-resizer {
  width: 10px;
  height: 100%;
  cursor: col-resize;
  background-color: transparent;
  position: relative;
  flex-shrink: 0;
  transition: background-color 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
  &:hover, &:active { background-color: #f0f2f5; }
  &::after { content: ""; width: 2px; height: 30px; background-color: #dcdfe6; border-radius: 1px; }
}

.right-pane {
  flex: 1;
  height: 100%;
  min-width: 400px;
  overflow: hidden;
  padding-left: 0.625rem;
}

.left-list-card {
  height: 100%;
  display: flex;
  flex-direction: column;
  :deep(.el-card__body) {
    flex: 1;
    overflow: hidden;
    display: flex;
    flex-direction: column;
    padding: 0.625rem;
  }
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  span {
    font-size: 0.9375rem;
    font-weight: 600;

    text-transform: uppercase;
    letter-spacing: 1px;
    position: relative;
    padding-left: 0.9375rem;
    line-height: 1.2;
    &::after { content: ''; position: absolute; left: 0; top: 0; bottom: 0; width: 3px; background-color: #c5a880; }
  }
}

.header-actions { display: flex; gap: 0.3125rem; }

.mobile-container {
  height: 100%;
  overflow-y: auto;
}

.mobile-detail-header {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: white;
  border-bottom: 1px solid #eae6df;
  .header-title { font-weight: bold; font-size: 1.1rem; }
}

:deep(.el-card) { border-radius: 2px; border-color: #eae6df; }
:deep(.el-button) { border-radius: 0; }
</style>

