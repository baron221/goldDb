<template>
  <el-form :model="form" label-width="120px" style="margin-top: 1.25rem;">

    <div v-if="mode === 'basic'" class="pane-content">
      <el-row :gutter="20">
        <el-col :span="12">
          <el-form-item label="이름">
            <el-input v-model="form.name" />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="주민번호">
            <el-input v-model="form.ssn" placeholder="000000-0000000" />
          </el-form-item>
        </el-col>
      </el-row>
      <el-form-item label="우편번호">
        <el-input v-model="form.zipCode" style="width: 250px;" readonly>
          <template #append>
            <el-button @click="openPostcode">주소 검색</el-button>
          </template>
        </el-input>
      </el-form-item>
      <el-form-item label="기본 주소">
        <el-input v-model="form.addressBase" readonly />
      </el-form-item>
      <el-form-item label="상세 주소">
        <el-input v-model="form.addressDetail" placeholder="상세 주소를 입력하세요" />
      </el-form-item>
      <el-form-item label="자기 소개">
        <el-input v-model="form.introduction" type="textarea" :rows="3" />
      </el-form-item>
      <el-form-item label="수신 동의">
        <el-checkbox v-model="form.smsAllowed">SMS</el-checkbox>
        <el-checkbox v-model="form.kakaoAllowed">카카오톡</el-checkbox>
        <el-checkbox v-model="form.emailAllowed">이메일</el-checkbox>
      </el-form-item>
    </div>

    <div v-if="mode === 'contact'" class="pane-content">
      <div class="section-title">
        <span>이메일 주소</span>
        <el-button type="primary" size="small" :icon="Plus" circle @click="addEmail" />
      </div>
      <div v-for="(item, index) in form.emails" :key="'e'+index" class="dynamic-item">
        <el-input v-model="item.email" placeholder="email@example.com" style="flex: 1;" />
        <el-radio v-model="primaryEmailIndex" :value="index" @change="setPrimaryEmail(index)">대표</el-radio>
        <el-button type="danger" :icon="Delete" circle size="small" @click="removeEmail(index)" />
      </div>

      <el-divider />

      <div class="section-title">
        <span>휴대폰 번호</span>
        <el-button type="primary" size="small" :icon="Plus" circle @click="addPhone" />
      </div>
      <div v-for="(item, index) in form.phones" :key="'p'+index" class="dynamic-item">
        <el-input v-model="item.phoneNumber" placeholder="010-0000-0000" style="flex: 1;" />
        <el-radio v-model="primaryPhoneIndex" :value="index" @change="setPrimaryPhone(index)">대표</el-radio>
        <el-button type="danger" :icon="Delete" circle size="small" @click="removePhone(index)" />
      </div>
    </div>

    <div v-if="mode === 'gallery'" class="pane-content">
      <el-divider>갤러리 사진</el-divider>

      <div v-if="form.photos.length > 0" class="gallery-carousel-wrapper">
        <el-carousel :interval="4000" type="card" height="300px" indicator-position="outside">
          <el-carousel-item v-for="(item, index) in form.photos" :key="index">
            <div class="carousel-photo-container">
              <img :src="getPhotoDisplayUrl(item)" class="carousel-img" />

              <div class="order-badge">#{{ item.sortOrder + 1 }}</div>

              <el-button
                type="danger"
                :icon="Delete"
                circle
                class="floating-delete-btn"
                @click="removePhoto(index)"
              />

              <div class="floating-sort-controls">
                <el-icon class="sort-icon-btn" @click="item.sortOrder++"><ArrowUpBold /></el-icon>
                <el-icon class="sort-icon-btn" @click="item.sortOrder > 0 ? item.sortOrder-- : null"><ArrowDownBold /></el-icon>
              </div>
            </div>
          </el-carousel-item>
        </el-carousel>
      </div>
      <el-empty v-else description="등록된 갤러리 사진이 없습니다." :image-size="100" />

      <div class="photo-adder-wrapper">
        <image-upload
          sub-dir="users/gallery"
          size="medium"
          clear-after-upload
          compact
          @change="handleNewGalleryPhoto"
        />
      </div>
    </div>

    <div style="text-align: right; margin-top: 1.25rem; padding-right: 1.25rem;">
      <el-button type="success" size="large" @click="submit">정보 저장하기</el-button>
    </div>
  </el-form>
</template>

<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import { updateUser } from '@/api/user';
import { ElMessage } from 'element-plus';
import { Plus, Delete, ArrowUpBold, ArrowDownBold } from '@element-plus/icons-vue';
import ImageUpload from '@/components/ImageUpload/index.vue';

const props = defineProps({
  userData: {
    type: Object,
    required: true
  },
  mode: {
    type: String,
    default: 'basic' 
  }
});

const emit = defineEmits(['success']);

const form = reactive({
  id: undefined,
  name: '',
  ssn: '',
  zipCode: '',
  addressBase: '',
  addressDetail: '',
  avatar: '',
  avatarId: null as number | null,
  introduction: '',
  smsAllowed: false,
  kakaoAllowed: false,
  emailAllowed: false,
  emails: [] as any[],
  phones: [] as any[],
  photos: [] as any[],
  roles: [] as string[]
});

const primaryEmailIndex = ref(-1);
const primaryPhoneIndex = ref(-1);

watch(() => props.userData, (newVal) => {
  if (newVal && newVal.id) {
    Object.assign(form, JSON.parse(JSON.stringify(newVal)));
    primaryEmailIndex.value = form.emails.findIndex(e => e.isPrimary);
    primaryPhoneIndex.value = form.phones.findIndex(p => p.isPrimary);
  }
}, { immediate: true });

const addEmail = () => form.emails.push({ email: '', isPrimary: false });

const removeEmail = (index: number) => {
  form.emails.splice(index, 1);
  if (primaryEmailIndex.value === index) primaryEmailIndex.value = -1;
};

const setPrimaryEmail = (index: number) => {
  primaryEmailIndex.value = index;
};

const addPhone = () => form.phones.push({ phoneNumber: '', isPrimary: false });

const removePhone = (index: number) => {
  form.phones.splice(index, 1);
  if (primaryPhoneIndex.value === index) primaryPhoneIndex.value = -1;
};

const setPrimaryPhone = (index: number) => {
  primaryPhoneIndex.value = index;
};

const addPhoto = () => form.photos.push({ photoUrl: '', attachmentId: null, sortOrder: form.photos.length });

const removePhoto = (index: number) => form.photos.splice(index, 1);

const openPostcode = () => {
  new (window as any).daum.Postcode({
    oncomplete: (data: any) => {
      let fullAddress = data.address;
      let extraAddress = '';

      if (data.addressType === 'R') {
        if (data.bname !== '') {
          extraAddress += data.bname;
        }
        if (data.buildingName !== '') {
          extraAddress += (extraAddress !== '' ? `, ${data.buildingName}` : data.buildingName);
        }
        fullAddress += (extraAddress !== '' ? ` (${extraAddress})` : '');
      }

      form.zipCode = data.zonecode;
      form.addressBase = fullAddress;

      document.querySelector('input[placeholder="상세 주소를 입력하세요"]')?.focus();
    }
  }).open();
};

const handleNewGalleryPhoto = (attachment: any) => {
  if (attachment) {
    form.photos.push({
      photoUrl: attachment.filePath,
      attachmentId: attachment.id,
      sortOrder: form.photos.length
    });
  }
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

const submit = async () => {
  try {

    form.emails.forEach((e, idx) => { e.isPrimary = idx === primaryEmailIndex.value; });
    form.phones.forEach((p, idx) => { p.isPrimary = idx === primaryPhoneIndex.value; });

    await updateUser(form.id, form);
    ElMessage.success('프로필 정보가 성공적으로 업데이트되었습니다.');
    emit('success');
  } catch (error) {
    ElMessage.error('저장에 실패했습니다.');
  }
};
</script>

<style src="./AccountStyles.scss" scoped></style>
