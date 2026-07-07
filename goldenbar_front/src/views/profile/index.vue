<template>
<div class="app-container">
    <div v-if="loading" class="loading-container">
      <el-skeleton :rows="10" animated />
    </div>
    <div v-else-if="user && user.id">
      <el-row :gutter="20">
        <el-col :span="6" :xs="24">
          <user-card :user="user" @avatar-change="handleAvatarChange" />
        </el-col>

        <el-col :span="18" :xs="24">
          <el-card>
            <el-tabs v-model="activeTab">
              <el-tab-pane :label="$t('profile.tabs.basic')" name="basic">
                <account :user-data="user" mode="basic" @success="getUser" />
              </el-tab-pane>
              <el-tab-pane :label="$t('profile.tabs.contact')" name="contact">
                <account :user-data="user" mode="contact" @success="getUser" />
              </el-tab-pane>
              <el-tab-pane :label="$t('profile.tabs.gallery')" name="gallery">
                <account :user-data="user" mode="gallery" @success="getUser" />
              </el-tab-pane>
              <el-tab-pane v-if="user.userType === 'COMPANY' && user.companyId" :label="$t('profile.tabs.company')" name="company">
                <company-account :company-id="user.companyId" />
              </el-tab-pane>
            </el-tabs>
          </el-card>
        </el-col>
      </el-row>
    </div>
    <div v-else class="empty-container">
      <el-empty :description="$t('profile.messages.loadError')">
        <el-button type="primary" @click="getUser">{{ $t('profile.actions.retry') }}</el-button>
      </el-empty>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import UserCard from './components/UserCard.vue';
import Account from './components/Account.vue';
import CompanyAccount from './components/CompanyAccount.vue';
import useUserStore from '@/store/modules/user';
import { getUserDetail, updateUser } from '@/api/user';
import { ElMessage } from 'element-plus';

const { t } = useI18n();
const userStore = useUserStore();
const user = ref<any>({});
const activeTab = ref('basic');
const loading = ref(true);

const handleAvatarChange = async (attachment: any) => {
  if (attachment && user.value.id) {
    try {

      const updateData = { ...user.value };
      updateData.avatarId = attachment.id;
      updateData.avatar = attachment.filePath;

      await updateUser(user.value.id, updateData);

      user.value = {
        ...user.value,
        avatarId: attachment.id,
        avatar: attachment.filePath
      };

      if (userStore.userId === String(user.value.id)) {
        userStore.avatar = attachment.filePath;
      }

      ElMessage.success(t('profile.messages.avatarSuccess'));
    } catch (error) {
      ElMessage.error(t('profile.messages.avatarError'));
    }
  }
};

const getUser = async () => {
  loading.value = true;
  try {
    if (userStore.userId) {
      const res = await getUserDetail(Number(userStore.userId));
      user.value = res.data;
    } else {
      ElMessage.warning(t('profile.messages.sessionError'));
    }
  } catch (error) {
    console.error('Failed to fetch user profile:', error);
    ElMessage.error(t('profile.messages.fetchError'));
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  getUser();
});
</script>

<style scoped>
.loading-container, .empty-container {
  padding: 2.5rem;
  text-align: center;
}
</style>

