<template>
<el-card style="margin-bottom: 1.25rem;">
    <template #header>
      <div class="clearfix">
        <span>프로필 요약</span>
      </div>
    </template>

    <div class="user-profile">
      <div class="box-center">
        <div class="avatar-edit-container">
          <image-upload
            :model-value="user.avatarId"
            :initial-url="user.avatar"
            sub-dir="users"
            size="avatar"
            @change="handleAvatarChange"
          />
          <div class="edit-overlay">
            <el-icon><Camera /></el-icon>
            <span>변경</span>
          </div>
        </div>
      </div>
      <div class="box-center">
        <div class="user-name text-center">
          <span style="font-weight: bold; color: #409EFF;">{{ user.username }}</span>
          <span v-if="user.name" style="margin-left: 0.3125rem; color: #606266; font-size: 0.9rem;">({{ user.name }})</span>
        </div>
        <div class="user-role text-center text-muted">{{ user.roles?.join(' | ') }}</div>
        <div v-if="user.companyName" class="user-company text-center text-muted">
          <el-icon><OfficeBuilding /></el-icon> {{ user.companyName }}
        </div>
      </div>
    </div>

    <div class="user-bio">
      <div class="user-education user-bio-section">
        <div class="user-bio-section-header">
          <el-icon><Location /></el-icon><span>주소 정보</span>
        </div>
        <div class="user-bio-section-body">
          <div class="text-muted">
            {{ user.addressBase }} {{ user.addressDetail }}
          </div>
        </div>
      </div>

      <div class="user-skills user-bio-section">
        <div class="user-bio-section-header">
          <el-icon><Phone /></el-icon><span>연락처</span>
        </div>
        <div class="user-bio-section-body">
          <div v-if="primaryPhone" class="text-muted">
            대표번호: {{ primaryPhone.phoneNumber }}
          </div>
          <div v-if="primaryEmail" class="text-muted" style="margin-top: 0.3125rem;">
            이메일: {{ primaryEmail.email }}
          </div>
        </div>
      </div>

      <div class="user-bio-section">
        <div class="user-bio-section-header">
          <el-icon><ChatLineRound /></el-icon><span>자기 소개</span>
        </div>
        <div class="user-bio-section-body">
          <div class="text-muted">
            {{ user.introduction || '등록된 소개가 없습니다.' }}
          </div>
        </div>
      </div>
    </div>
  </el-card>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import ImageUpload from '@/components/ImageUpload/index.vue';
import { Location, Phone, ChatLineRound, Camera, OfficeBuilding } from '@element-plus/icons-vue';

const props = defineProps({
  user: {
    type: Object,
    required: true
  }
});

const emit = defineEmits(['avatar-change']);

const primaryPhone = computed(() => props.user.phones?.find((p: any) => p.isPrimary));
const primaryEmail = computed(() => props.user.emails?.find((e: any) => e.isPrimary));

const handleAvatarChange = (attachment: any) => {
  emit('avatar-change', attachment);
};
</script>

<style lang="scss" scoped>
.box-center {
  margin: 0 auto;
  display: table;
}

.avatar-edit-container {
  position: relative;
  width: 100px;
  height: 100px;
  border-radius: 50%;
  overflow: hidden;
  cursor: pointer;
  border: 2px solid #ebeef5;

  &:hover .edit-overlay {
    opacity: 1;
  }

  :deep(.image-upload-container) {
    width: 100%;
    height: 100%;

    .el-upload-dragger {
      padding: 0;
      min-height: 100px;
      height: 100px;
      border: none;
      background: transparent;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .uploaded-image {
      width: 100px;
      height: 100px;
      border-radius: 50%;
      object-fit: cover;
    }
  }
}

.edit-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.4);
  color: white;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  font-size: 0.95rem;
  opacity: 0;
  transition: opacity 0.3s;
  pointer-events: none;

  .el-icon {
    font-size: 1.25rem;
    margin-bottom: 0.25rem;
  }
}

.text-muted {
  color: #777;
  font-size: 0.9rem;
}

.user-profile {
  .user-name {
    font-weight: bold;
    margin-top: 0.9375rem;
  }

  .box-center {
    padding-top: 0.625rem;
  }

  .user-role {
    padding-top: 0.625rem;
    font-weight: 400;
    font-size: 0.875rem;
  }

  .user-company {
    padding-top: 0.3125rem;
    font-size: 0.9rem;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.25rem;
  }
}

.user-bio {
  margin-top: 1.25rem;
  color: #606266;

  span {
    padding-left: 0.25rem;
  }

  .user-bio-section {
    font-size: 0.875rem;
    padding: 0.9375rem 0;

    .user-bio-section-header {
      border-bottom: 1px solid #dfe6ec;
      padding-bottom: 0.625rem;
      margin-bottom: 0.625rem;
      font-weight: bold;
      display: flex;
      align-items: center;
    }
  }
}
</style>

