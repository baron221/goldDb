<template>
<el-card v-if="user" shadow="never" class="right-detail-card">
    <template #header>
      <div class="card-header">
        <span>{{ $t('userManage.detailTitle') }} <el-tag v-if="localUser && !localUser.isApproved" type="warning" size="small" style="margin-left: 10px;">Pending Approval</el-tag></span>
        <div class="header-actions">
          <el-button v-if="localUser && !localUser.isApproved" v-permission="'save'" type="success" size="small" @click="emit('approve')">
            승인 (Approve)
          </el-button>
          <el-button v-permission="'save'" type="primary" size="small" :icon="Check" @click="emit('save')">
            {{ $t('common.save') }}
          </el-button>
          <el-button v-permission="'delete'" type="danger" size="small" :icon="Delete" @click="emit('delete')">
            {{ $t('common.delete') }}
          </el-button>
        </div>
      </div>
    </template>

    <div class="detail-container">
      <el-tabs v-model="activeTab" class="user-detail-tabs">

        <el-tab-pane :label="$t('userManage.tabs.basic')" name="basic">
          <div class="tab-section">
            <el-form ref="formRef" :model="localUser" label-position="top">
              <el-row :gutter="20">
                <el-col :span="8">
                  <div class="avatar-edit-container">
                    <image-upload
                      v-model="localUser.avatarId"
                      v-model:initial-url="localUser.avatar"
                      :aspect-ratio="1"
                      :max-size="2"
                      @change="onAvatarChange"
                    />
                    <div class="edit-overlay">
                      <el-icon><Camera /></el-icon>
                      <span>{{ $t('common.edit') }}</span>
                    </div>
                  </div>
                </el-col>
                <el-col :span="16">
                  <el-form-item :label="$t('userManage.username')">
                    <el-input v-model="localUser.username" readonly disabled />
                  </el-form-item>
                  <el-form-item :label="$t('userManage.name')" required>
                    <el-input v-model="localUser.name" :placeholder="$t('userManage.namePlaceholder')" />
                  </el-form-item>
                </el-col>
              </el-row>

              <el-row :gutter="20">
                <el-col :span="12">
                  <el-form-item :label="$t('userManage.ssn')">
                    <el-input v-model="localUser.ssn" placeholder="000000-0000000" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item :label="$t('userManage.company')">
                    <div style="display: flex; gap: 5px;">
                      <company-select v-model="localUser.companyId" style="flex: 1;" />
                      <el-button :icon="Plus" @click="emit('create-company')" />
                    </div>
                  </el-form-item>
                </el-col>
              </el-row>

              <el-form-item :label="$t('userManage.address')">
                <div style="display: flex; gap: 8px; margin-bottom: 8px;">
                  <el-input v-model="localUser.zipCode" style="width: 120px;" :placeholder="$t('userManage.zipCode')" readonly />
                  <el-button type="info" plain @click="emit('open-postcode')">{{ $t('userManage.searchPostcode') }}</el-button>
                </div>
                <el-input v-model="localUser.addressBase" :placeholder="$t('userManage.addressBase')" readonly style="margin-bottom: 8px;" />
                <el-input v-model="localUser.addressDetail" :placeholder="$t('userManage.addressDetailPlaceholder')" />
              </el-form-item>
            </el-form>
          </div>
        </el-tab-pane>

        <el-tab-pane :label="$t('userManage.tabs.contacts')" name="contacts">
          <div class="tab-section">
            <div class="section-title">
              <span>{{ $t('userManage.emails') }}</span>
              <el-button type="primary" link :icon="Plus" @click="emit('add-email')">{{ $t('common.add') }}</el-button>
            </div>
            <div v-for="(email, index) in localUser.emails" :key="'email-'+index" class="dynamic-item">
              <el-input v-model="email.email" placeholder="example@domain.com" style="flex: 1;" />
              <el-checkbox v-model="email.isPrimary" @change="emit('set-primary-email', index)">{{ $t('userManage.primary') }}</el-checkbox>
              <el-button type="danger" link :icon="Delete" @click="emit('remove-email', index)" />
            </div>

            <el-divider />

            <div class="section-title">
              <span>{{ $t('userManage.phones') }}</span>
              <el-button type="primary" link :icon="Plus" @click="emit('add-phone')">{{ $t('common.add') }}</el-button>
            </div>
            <div v-for="(phone, index) in localUser.phones" :key="'phone-'+index" class="dynamic-item">
              <el-input v-model="phone.phoneNumber" placeholder="010-0000-0000" style="flex: 1;" />
              <el-checkbox v-model="phone.isPrimary" @change="emit('set-primary-phone', index)">{{ $t('userManage.primary') }}</el-checkbox>
              <el-button type="danger" link :icon="Delete" @click="emit('remove-phone', index)" />
            </div>
          </div>
        </el-tab-pane>

        <el-tab-pane :label="$t('userManage.tabs.gallery')" name="gallery">
          <div class="tab-section">
            <div class="section-title">
              <span>{{ $t('userManage.userPhotos') }}</span>
              <div class="photo-adder-wrapper">
                <image-upload
                  :model-value="null"
                  multiple
                  @change="onNewPhoto"
                >
                  <template #trigger>
                    <el-button type="primary" size="small" plain :icon="Plus">{{ $t('userManage.addPhoto') }}</el-button>
                  </template>
                </image-upload>
              </div>
            </div>

            <div class="gallery-carousel-wrapper" v-if="localUser.photos && localUser.photos.length > 0">
              <el-carousel :interval="4000" type="card" height="300px" indicator-position="outside">
                <el-carousel-item v-for="(photo, index) in localUser.photos" :key="index">
                  <div class="carousel-photo-container">
                    <el-image
                      :src="getPhotoDisplayUrl(photo)"
                      fit="contain"
                      class="carousel-img"
                      :preview-src-list="localUser.photos.map(p => p.photoUrl)"
                      :initial-index="index"
                      preview-teleported
                    />
                    <div v-if="photo.isMain" class="order-badge">{{ $t('userManage.representative') }}</div>
                    <el-button
                      type="danger"
                      size="small"
                      circle
                      :icon="Delete"
                      class="floating-delete-btn"
                      @click="emit('remove-photo', index)"
                    />
                    <div class="floating-sort-controls">
                      <el-icon class="sort-icon-btn" @click="emit('move-photo', {index, dir: 'up'})"><ArrowUpBold /></el-icon>
                      <el-icon class="sort-icon-btn" @click="emit('move-photo', {index, dir: 'down'})"><ArrowDownBold /></el-icon>
                      <el-button
                        size="small"
                        type="success"
                        plain
                        style="margin-top: 5px; padding: 4px;"
                        @click="emit('set-main-photo', index)"
                      >
                        {{ $t('userManage.setMainPhoto') }}
                      </el-button>
                    </div>
                  </div>
                </el-carousel-item>
              </el-carousel>
            </div>
            <el-empty v-else :description="$t('userManage.noGallery')" />
          </div>
        </el-tab-pane>

        <el-tab-pane :label="$t('userManage.tabs.roles')" name="roles">
          <div class="tab-section">
            <div class="section-title">{{ $t('userManage.assignedRoles') }}</div>
            <el-checkbox-group v-model="localUser.roles" class="role-checkbox-group">
              <el-checkbox
                v-for="role in roles"
                :key="role.key"
                :value="role.key"
                border
                style="margin-bottom: 10px;"
              >
                {{ role.name }}
              </el-checkbox>
            </el-checkbox-group>
          </div>
        </el-tab-pane>
      </el-tabs>
    </div>
  </el-card>

  <el-card v-else shadow="never" class="empty-card">
    <div class="empty-state">
      <el-icon :size="48"><InfoFilled /></el-icon>
      <p>{{ $t('userManage.selectUserHint') }}</p>
    </div>
  </el-card>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue';
import { Check, Delete, Plus, InfoFilled, Camera, ArrowUpBold, ArrowDownBold } from '@element-plus/icons-vue';
import ImageUpload from '@/components/ImageUpload/index.vue';
import CompanySelect from '@/components/CompanySelect/index.vue';

const props = defineProps<{
  user: any;
  roles: any[];
}>();

const emit = defineEmits([
  'save', 'delete', 'create-company', 'open-postcode',
  'add-email', 'remove-email', 'set-primary-email',
  'add-phone', 'remove-phone', 'set-primary-phone',
  'remove-photo', 'move-photo', 'set-main-photo',
  'avatar-change', 'new-photo', 'update:user', 'approve'
]);

const activeTab = ref('basic');

const localUser = reactive({ ...props.user });

watch(() => props.user, (newVal) => {
  Object.assign(localUser, newVal);
}, { deep: true });

watch(localUser, (newVal) => {
  emit('update:user', { ...newVal });
}, { deep: true });

const onAvatarChange = (attachment: any) => {
  emit('avatar-change', attachment);
};

const onNewPhoto = (attachment: any) => {
  emit('new-photo', attachment);
};

const getPhotoDisplayUrl = (item: any) => {
  const url = item.photoUrl;
  if (!url) return '';
  if (item.attachmentId && url.startsWith('/uploads/')) {
    const parts = url.split('/');
    if (parts.length >= 4) {
      const fileName = parts.pop();
      return [...parts, 'medium', fileName].join('/');
    }
  }
  return url;
};
</script>

<style lang="scss" scoped>

.right-detail-card {
  height: 100%;
  display: flex;
  flex-direction: column;
}
:deep(.right-detail-card .el-card__body) {
  flex: 1;
  overflow-y: auto;
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
    &::after {
      content: '';
      position: absolute; left: 0; top: 0; bottom: 0; width: 3px; background-color: #c5a880;
    }
  }
}
.header-actions { display: flex; gap: 0.3125rem; }
.empty-card { height: 100%; display: flex; align-items: center; justify-content: center; }
.empty-state { text-align: center; color: #909399; }
.tab-section { padding: 0.625rem 0; }
.section-title { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.9375rem; font-weight: bold; }
.dynamic-item { display: flex; align-items: center; gap: 0.9375rem; margin-bottom: 0.625rem; }
.role-checkbox-group { display: flex; flex-wrap: wrap; gap: 0.625rem; margin-top: 0.9375rem; }
.avatar-edit-container {
  position: relative; width: 100px; height: 100px; border-radius: 50%; overflow: hidden; cursor: pointer; border: 2px solid #ebeef5;
  &:hover .edit-overlay { opacity: 1; }
  :deep(.image-upload-container) {
    width: 100%; height: 100%;
    .el-upload-dragger { padding: 0; min-height: 100px; height: 100px; border: none; background: transparent; display: flex; align-items: center; justify-content: center; }
    .uploaded-image { width: 100px; height: 100px; border-radius: 50%; object-fit: cover; }
  }
}
.edit-overlay {
  position: absolute; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0, 0, 0, 0.4); color: white; display: flex; flex-direction: column; align-items: center; justify-content: center; font-size: 0.95rem; opacity: 0; transition: opacity 0.3s; pointer-events: none;
  .el-icon { font-size: 1.25rem; margin-bottom: 0.25rem; }
}
.carousel-photo-container {
  position: relative; width: 100%; height: 100%; background-color: #000; display: flex; align-items: center; justify-content: center; overflow: hidden; border-radius: 2px; border: 1px solid #000;
}
.carousel-img { height: 100%; object-fit: cover; }
.order-badge { position: absolute; top: 10px; left: 10px; background: rgba(0, 0, 0, 0.4); color: white; padding: 0.125rem 0.5rem; border-radius: 2px; font-size: 0.8875rem; font-weight: bold; backdrop-filter: blur(2px); z-index: 11; }
.floating-delete-btn { position: absolute; top: 8px; right: 8px; opacity: 0; transform: scale(0.8); transition: opacity 0.3s, transform 0.2s; z-index: 10; }
.floating-sort-controls { position: absolute; bottom: 10px; left: 10px; display: flex; flex-direction: column; gap: 0.25rem; opacity: 0.2; transition: opacity 0.3s; z-index: 10; }
.carousel-photo-container:hover .floating-delete-btn { opacity: 1; transform: scale(0.85); }
.carousel-photo-container:hover .floating-sort-controls { opacity: 1; }
.sort-icon-btn { background: rgba(255, 255, 255, 0.8); color: #606266; padding: 0.25rem; border-radius: 2px; cursor: pointer; font-size: 0.875rem; }
.sort-icon-btn:hover { background: #c5a880; color: white; }
</style>

