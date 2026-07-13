<template>
  <div class="app-container">
    <div class="split-container" ref="splitContainer">
      
      <!-- Left Pane: Employee List -->
      <div class="left-pane" :style="{ width: leftWidth + 'px' }">
        <el-card shadow="never" class="left-list-card">
          <template #header>
            <div class="card-header">
              <div class="section-header">
                <h2 class="section-title">{{ $t('userManage.tabs.employees') }}</h2>
              </div>
              <div class="header-actions">
                <el-button type="info" size="small" :icon="Link" @click="handleOpenLinkDialog">
                  {{ $t('sys.company.linkUser') }}
                </el-button>
                <el-button type="primary" size="small" :icon="Plus" @click="handleCreate">
                  {{ $t('sys.company.createNewUser') }}
                </el-button>
                <el-button type="success" size="small" :loading="downloadLoading" @click="handleDownload">
                  Excel 내보내기
                </el-button>
              </div>
            </div>
          </template>

          <div class="filter-container" style="margin-bottom: 15px;">
            <el-row :gutter="10">
              <el-col :span="12" v-if="isAdmin">
                <company-select
                  v-model="selectedCompanyId"
                  placeholder="업체 선택"
                  style="width: 100%;"
                  @change="handleCompanyChange"
                />
              </el-col>
              <el-col :span="isAdmin ? 12 : 24">
                <el-input
                  v-model="searchText"
                  :placeholder="$t('userManage.namePlaceholder') + ' / ID'"
                  clearable
                  :prefix-icon="Search"
                />
              </el-col>
            </el-row>
          </div>

          <base-table
            v-loading="loading"
            :data="filteredEmployees"
            style="width: 100%"
            border
            highlight-current-row
            @row-click="handleRowClick"
          >
            <el-table-column prop="name" :label="$t('contact.form.name')" sortable width="120" />
            <el-table-column prop="username" :label="$t('login.username')" min-width="120" />
            <el-table-column
              v-if="!isMobile"
              :label="$t('userManage.headers.lastLogin')"
              align="center"
              width="150"
            >
              <template #default="scope">
                {{ formatDateTime(scope.row.lastLoginAt) }}
              </template>
            </el-table-column>
            <el-table-column align="center" :label="$t('sys.company.unlink')" width="80" :fixed="!isMobile ? 'right' : false">
              <template #default="scope">
                <el-button
                  link
                  class="delete-icon-btn"
                  :icon="Delete"
                  @click.stop="handleUnlink(scope.row)"
                />
              </template>
            </el-table-column>
          </base-table>
        </el-card>
      </div>

      <!-- Pane Resizer -->
      <div class="pane-resizer" @mousedown="startResize"></div>

      <!-- Right Pane: Employee Detail -->
      <div class="right-pane">
        <el-card v-if="currentEmployee" shadow="never" class="right-detail-card">
          <div class="card-header" style="margin-bottom: 15px; display: flex; justify-content: space-between; align-items: center;">
            <div class="section-header">
              <h2 class="section-title">
                직원 상세: <el-tag size="small">{{ currentEmployee.name }}</el-tag>
              </h2>
            </div>
            <el-button
              v-if="activeTab === 'basic'"
              type="success"
              size="small"
              :icon="Check"
              @click="handleSave"
              :loading="saving"
            >
              {{ $t('common.save') }}
            </el-button>
          </div>

          <el-tabs v-model="activeTab" style="margin-top: -10px;">
            <!-- Tab: Basic Info -->
            <el-tab-pane :label="$t('userManage.tabs.basic')" name="basic">
              <div class="tab-section" style="padding-top: 10px;">
                <el-form ref="formRef" :model="userDetail" label-position="top">
                  <el-row :gutter="20">
                    <el-col :span="6">
                      <div class="avatar-edit-container" style="position: relative; display: inline-block;">
                        <image-upload
                          v-model="userDetail.avatarId"
                          v-model:initial-url="userDetail.avatar"
                          :aspect-ratio="1"
                          :max-size="2"
                          @change="handleAvatarChange"
                        />
                      </div>
                    </el-col>
                    <el-col :span="18">
                      <el-form-item :label="$t('userManage.username')">
                        <el-input v-model="userDetail.username" readonly disabled />
                      </el-form-item>
                      <el-form-item :label="$t('userManage.name')" required>
                        <el-input v-model="userDetail.name" :placeholder="$t('userManage.namePlaceholder')" />
                      </el-form-item>
                    </el-col>
                  </el-row>

                  <el-row :gutter="20">
                    <el-col :span="12">
                      <el-form-item :label="$t('userManage.ssn')">
                        <el-input v-model="userDetail.ssn" placeholder="000000-0000000" />
                      </el-form-item>
                    </el-col>
                    <el-col :span="12">
                      <el-form-item label="SMS 수신 동의">
                        <el-radio-group v-model="userDetail.smsAllowed">
                          <el-radio :value="true">{{ $t('userManage.receptionYes') }}</el-radio>
                          <el-radio :value="false">{{ $t('userManage.receptionNo') }}</el-radio>
                        </el-radio-group>
                      </el-form-item>
                    </el-col>
                  </el-row>

                  <el-form-item :label="$t('userManage.address')">
                    <div style="display: flex; gap: 8px; margin-bottom: 8px;">
                      <el-input v-model="userDetail.zipCode" style="width: 120px;" :placeholder="$t('userManage.zipCode')" readonly />
                      <el-button type="info" plain @click="openPostcode">{{ $t('userManage.searchPostcode') }}</el-button>
                    </div>
                    <el-input v-model="userDetail.addressBase" :placeholder="$t('userManage.addressBase')" readonly style="margin-bottom: 8px;" />
                    <el-input v-model="userDetail.addressDetail" :placeholder="$t('userManage.addressDetailPlaceholder')" />
                  </el-form-item>

                  <el-form-item :label="$t('userManage.introduction')">
                    <el-input v-model="userDetail.introduction" type="textarea" :rows="3" />
                  </el-form-item>

                  <el-divider />

                  <!-- Emails Section -->
                  <div class="section-title" style="margin-bottom: 10px; display: flex; justify-content: space-between; align-items: center;">
                    <span style="font-weight: 600;">{{ $t('userManage.emails') }}</span>
                    <el-button type="primary" link :icon="Plus" @click="addEmail">{{ $t('common.add') }}</el-button>
                  </div>
                  <div v-for="(email, index) in userDetail.emails" :key="'email-'+index" style="display: flex; gap: 10px; align-items: center; margin-bottom: 8px;">
                    <el-input v-model="email.email" placeholder="example@domain.com" style="flex: 1;" />
                    <el-checkbox v-model="email.isPrimary" @change="setPrimaryEmail(index)">{{ $t('userManage.primary') }}</el-checkbox>
                    <el-button type="danger" link :icon="Delete" @click="removeEmail(index)" />
                  </div>

                  <el-divider />

                  <!-- Phones Section -->
                  <div class="section-title" style="margin-bottom: 10px; display: flex; justify-content: space-between; align-items: center;">
                    <span style="font-weight: 600;">{{ $t('userManage.phones') }}</span>
                    <el-button type="primary" link :icon="Plus" @click="addPhone">{{ $t('common.add') }}</el-button>
                  </div>
                  <div v-for="(phone, index) in userDetail.phones" :key="'phone-'+index" style="display: flex; gap: 10px; align-items: center; margin-bottom: 8px;">
                    <el-input v-model="phone.phoneNumber" placeholder="010-0000-0000" style="flex: 1;" />
                    <el-checkbox v-model="phone.isPrimary" @change="setPrimaryPhone(index)">{{ $t('userManage.primary') }}</el-checkbox>
                    <el-button type="danger" link :icon="Delete" @click="removePhone(index)" />
                  </div>

                  <el-divider />

                  <!-- Gallery/Photos Section -->
                  <div class="section-title" style="margin-bottom: 10px; display: flex; justify-content: space-between; align-items: center;">
                    <span style="font-weight: 600;">{{ $t('userManage.userPhotos') }}</span>
                    <image-upload :model-value="null" multiple @change="handleNewGalleryPhoto">
                      <template #trigger>
                        <el-button type="primary" size="small" plain :icon="Plus">사진 추가</el-button>
                      </template>
                    </image-upload>
                  </div>
                  <div v-if="userDetail.photos && userDetail.photos.length > 0" style="margin-top: 10px;">
                    <el-carousel :interval="4000" type="card" height="200px" indicator-position="outside">
                      <el-carousel-item v-for="(photo, index) in userDetail.photos" :key="index">
                        <div class="carousel-photo-container" style="position: relative; height: 100%; text-align: center; background: #f5f5f5;">
                          <el-image
                            :src="photo.photoUrl"
                            fit="contain"
                            style="height: 100%; width: 100%;"
                            :preview-src-list="userDetail.photos.map(p => p.photoUrl)"
                            :initial-index="index"
                            preview-teleported
                          />
                          <div style="position: absolute; bottom: 5px; left: 5px; right: 5px; display: flex; justify-content: space-between; background: rgba(255,255,255,0.8); padding: 2px 5px; border-radius: 2px;">
                            <el-checkbox v-model="photo.isMain" @change="setMainPhoto(index)" size="small">대표</el-checkbox>
                            <div>
                              <el-icon style="cursor: pointer; margin-right: 5px;" @click="movePhoto(index, 'up')"><ArrowUpBold /></el-icon>
                              <el-icon style="cursor: pointer; margin-right: 5px;" @click="movePhoto(index, 'down')"><ArrowDownBold /></el-icon>
                              <el-icon style="cursor: pointer; color: #f56c6c;" @click="removeGalleryPhoto(index)"><Delete /></el-icon>
                            </div>
                          </div>
                        </div>
                      </el-carousel-item>
                    </el-carousel>
                  </div>
                </el-form>
              </div>
            </el-tab-pane>

            <!-- Tab: Order History -->
            <el-tab-pane label="주문 내역" name="orders">
              <div class="order-list-section" style="margin-top: 15px;">
                <base-table :data="employeeOrders" v-loading="ordersLoading" border style="width: 100%">
                  <el-table-column type="expand">
                    <template #default="props">
                      <div class="expand-table-wrapper" style="padding: 10px 20px; background-color: #faf9f6;">
                        <h4 style="margin: 0 0 10px 0; color: #222; font-size: 14px; border-left: 3px solid #c5a880; padding-left: 8px;">
                          주문 품목 리스트
                        </h4>
                        <base-table :data="props.row.orderItems" border size="small" style="width: 100%">
                          <el-table-column prop="productNo" label="제품번호" width="120" />
                          <el-table-column prop="productName" label="제품명" min-width="150" show-overflow-tooltip />
                          <el-table-column prop="purity" label="함량" width="90" align="center" />
                          <el-table-column prop="orderWeight" label="중량" width="90" align="right">
                            <template #default="scope">
                              {{ scope.row.orderWeight ? scope.row.orderWeight.toFixed(2) + 'g' : '-' }}
                            </template>
                          </el-table-column>
                          <el-table-column prop="color" label="컬러" width="95" align="center" />
                          <el-table-column prop="size" label="사이즈" width="90" align="center">
                            <template #default="scope">
                              {{ scope.row.size || '-' }}
                            </template>
                          </el-table-column>
                          <el-table-column prop="quantity" label="수량" width="70" align="center" />
                          <el-table-column prop="price" label="금액" width="120" align="right">
                            <template #default="scope">
                              ₩ {{ formatPrice(scope.row.price) }}
                            </template>
                          </el-table-column>
                        </base-table>
                      </div>
                    </template>
                  </el-table-column>

                  <el-table-column prop="orderNo" label="주문번호" width="180" sortable />
                  <el-table-column prop="createdAt" label="주문일시" width="160">
                    <template #default="scope">
                      {{ formatDateTime(scope.row.createdAt) }}
                    </template>
                  </el-table-column>
                  <el-table-column prop="status" label="상태" width="120" align="center">
                    <template #default="scope">
                      <el-tag :type="getOrderStatusTag(scope.row.status)" size="small">
                        {{ getOrderStatusName(scope.row.status) }}
                      </el-tag>
                    </template>
                  </el-table-column>
                  <el-table-column prop="totalAmount" label="총 주문금액" align="right">
                    <template #default="scope">
                      ₩ {{ formatPrice(scope.row.totalAmount) }}
                    </template>
                  </el-table-column>
                </base-table>
              </div>
            </el-tab-pane>
          </el-tabs>
        </el-card>
        
        <el-card v-else shadow="never" class="empty-card">
          <div class="empty-state">
            <el-icon :size="50" color="#909399"><InfoFilled /></el-icon>
            <p>선택된 직원이 없습니다. 목록에서 선택하거나 신규 등록을 진행하세요.</p>
          </div>
        </el-card>
      </div>

    </div>

    <!-- Dialogs -->
    <user-create-dialog
      v-model="createDialogVisible"
      :default-company-id="selectedCompanyId"
      default-user-type="COMPANY"
      @created="handleCreated"
    />

    <!-- Link User Dialog -->
    <el-dialog v-model="linkDialogVisible" title="직원 연결" width="400px">
      <el-form label-position="top">
        <el-form-item label="연결할 사용자 선택">
          <el-select v-model="selectedLinkUserId" placeholder="사용자를 선택하세요" filterable style="width: 100%;">
            <el-option
              v-for="user in availableUsers"
              :key="user.id"
              :label="`${user.name} (${user.username})`"
              :value="user.id"
            />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="linkDialogVisible = false">{{ $t('userManage.cancel') }}</el-button>
        <el-button type="primary" @click="confirmLinkUser" :loading="linking">{{ $t('sys.company.linkUser') }}</el-button>
      </template>
    </el-dialog>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue';
import { useMobile } from '@/hooks/useMobile';
import { useI18n } from 'vue-i18n';
import { ElMessage, ElMessageBox } from 'element-plus';
import { Plus, Delete, Search, Check, Link, ArrowUpBold, ArrowDownBold, InfoFilled } from '@element-plus/icons-vue';
import dayjs from 'dayjs';
import useUserStore from '@/store/modules/user';
import BaseTable from '@/components/BaseTable/index.vue';
import ImageUpload from '@/components/ImageUpload/index.vue';
import CompanySelect from '@/components/CompanySelect/index.vue';
import UserCreateDialog from './components/UserCreateDialog.vue';
import { getUserDetail, updateUser } from '@/api/user';
import { getCompanyUsers, addUserToCompany, removeUserFromCompany, getAvailableUsers } from '@/api/company';
import { getAllOrders } from '@/api/order';

const { isMobile } = useMobile();
const { t } = useI18n();
const userStore = useUserStore();

const leftWidth = ref(450);
const splitContainer = ref<HTMLElement | null>(null);
let isResizing = false;

// Resizer logic
const startResize = () => {
  isResizing = true;
  document.addEventListener('mousemove', handleResize);
  document.addEventListener('mouseup', stopResize);
  document.body.style.cursor = 'col-resize';
  document.body.style.userSelect = 'none';
};

const handleResize = (e: MouseEvent) => {
  if (!isResizing || !splitContainer.value) return;
  const containerRect = splitContainer.value.getBoundingClientRect();
  const newWidth = e.clientX - containerRect.left;

  if (newWidth > 300 && newWidth < containerRect.width - 400) {
    leftWidth.value = newWidth;
  }
};

const stopResize = () => {
  isResizing = false;
  document.removeEventListener('mousemove', handleResize);
  document.removeEventListener('mouseup', stopResize);
  document.body.style.cursor = '';
  document.body.style.userSelect = '';
};

// State Variables
const isAdmin = computed(() => userStore.roles.includes('admin'));
const selectedCompanyId = ref<number | null>(null);
const employeeList = ref<any[]>([]);
const filteredEmployees = computed(() => {
  let list = employeeList.value || [];
  if (searchText.value) {
    const s = searchText.value.toLowerCase();
    list = list.filter(e => e.name.toLowerCase().includes(s) || e.username.toLowerCase().includes(s));
  }
  return list;
});

const searchText = ref('');
const loading = ref(false);
const downloadLoading = ref(false);
const saving = ref(false);

const currentEmployee = ref<any>(null);
const activeTab = ref('basic');

// Detail State
const userDetail = reactive<any>({
  id: undefined,
  username: '',
  name: '',
  ssn: '',
  zipCode: '',
  addressBase: '',
  addressDetail: '',
  avatar: '',
  avatarId: null,
  introduction: '',
  smsAllowed: false,
  emails: [],
  phones: [],
  photos: []
});

// Dialogs State
const createDialogVisible = ref(false);
const linkDialogVisible = ref(false);
const availableUsers = ref<any[]>([]);
const selectedLinkUserId = ref<number | null>(null);
const linking = ref(false);

// Orders State
const employeeOrders = ref<any[]>([]);
const ordersLoading = ref(false);

// Fetch Data
const fetchEmployees = async () => {
  if (!selectedCompanyId.value) return;
  loading.value = true;
  try {
    const res = await getCompanyUsers(selectedCompanyId.value);
    employeeList.value = res.data || [];
  } catch (error) {
    ElMessage.error(t('sys.company.loadUsersFail'));
  } finally {
    loading.value = false;
  }
};

const handleCompanyChange = () => {
  currentEmployee.value = null;
  fetchEmployees();
};

onMounted(() => {
  selectedCompanyId.value = userStore.companyId;
  fetchEmployees();
});

// Row Click
const handleRowClick = async (row: any) => {
  try {
    currentEmployee.value = row;
    const res = await getUserDetail(row.id);
    if (res.data) {
      Object.assign(userDetail, res.data);
      if (!userDetail.emails) userDetail.emails = [];
      if (!userDetail.phones) userDetail.phones = [];
      if (!userDetail.photos) userDetail.photos = [];
      activeTab.value = 'basic';
      fetchEmployeeOrders(row.id);
    }
  } catch (error) {
    ElMessage.error(t('userManage.loadFail'));
  }
};

// Save
const handleSave = async () => {
  if (!userDetail.id) return;
  saving.value = true;
  try {
    await updateUser(userDetail.id, userDetail);
    ElMessage.success(t('common.save'));
    fetchEmployees();
  } catch (error: any) {
    ElMessage.error(t('common.save') + ': ' + (error.response?.data?.message || error.message));
  } finally {
    saving.value = false;
  }
};

// Link Dialog
const handleOpenLinkDialog = async () => {
  linkDialogVisible.value = true;
  selectedLinkUserId.value = null;
  try {
    const res = await getAvailableUsers();
    availableUsers.value = res.data || [];
  } catch (error) {
    ElMessage.error('사용자 목록 로드 실패');
  }
};

const confirmLinkUser = async () => {
  if (!selectedLinkUserId.value || !selectedCompanyId.value) return;
  linking.value = true;
  try {
    await addUserToCompany(selectedCompanyId.value, selectedLinkUserId.value);
    ElMessage.success(t('sys.company.linkSuccess'));
    linkDialogVisible.value = false;
    fetchEmployees();
  } catch (error) {
    ElMessage.error(t('sys.company.linkFail'));
  } finally {
    linking.value = false;
  }
};

// Create User
const handleCreate = () => {
  if (!selectedCompanyId.value) {
    ElMessage.warning('먼저 업체를 선택해 주세요.');
    return;
  }
  createDialogVisible.value = true;
};

const handleCreated = (newUser: any) => {
  fetchEmployees();
  handleRowClick(newUser);
};

// Unlink User
const handleUnlink = (row: any) => {
  ElMessageBox.confirm('해당 직원을 업체에서 제외(연결 해제)하시겠습니까?', '경고', {
    confirmButtonText: '확인',
    cancelButtonText: '취소',
    type: 'warning'
  }).then(async () => {
    if (!selectedCompanyId.value) return;
    try {
      await removeUserFromCompany(selectedCompanyId.value, row.id);
      ElMessage.success(t('sys.company.unlinkSuccess'));
      if (currentEmployee.value?.id === row.id) {
        currentEmployee.value = null;
      }
      fetchEmployees();
    } catch (error) {
      ElMessage.error(t('sys.company.unlinkFail'));
    }
  });
};

// Emails & Phones methods
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

// Avatar & Gallery methods
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

const movePhoto = (index: number, dir: 'up' | 'down') => {
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

// Daum postcode search
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

// Order History methods
const fetchEmployeeOrders = async (userId: number) => {
  if (!selectedCompanyId.value) return;
  ordersLoading.value = true;
  try {
    const res = await getAllOrders({
      companyId: selectedCompanyId.value,
      userName: userDetail.username,
      page: 1,
      pageSize: 100
    });
    employeeOrders.value = res.data.items || [];
  } catch (error) {
    console.error('직원 주문 내역 로드 실패:', error);
    ElMessage.error('주문 내역 로드 실패');
  } finally {
    ordersLoading.value = false;
  }
};

// Formatter Helpers
const formatDateTime = (value: string | null | undefined) => {
  if (!value) return '-';
  return dayjs(value).format('YYYY-MM-DD HH:mm');
};

const formatPrice = (value: number) => {
  if (value === null || value === undefined) return '0';
  return value.toLocaleString();
};

const getOrderStatusName = (status: string) => {
  switch (status) {
    case 'ORDERED': return '주문접수';
    case 'FactoryRequested': return '공장의뢰';
    case 'LogisticsApproved': return '물류승인';
    case 'Inspected': return '검수완료';
    case 'SETTLED': return '정산완료';
    case 'CANCELLED': return '주문취소';
    default: return status;
  }
};

const getOrderStatusTag = (status: string) => {
  switch (status) {
    case 'ORDERED': return 'info';
    case 'FactoryRequested': return 'warning';
    case 'LogisticsApproved': return 'primary';
    case 'Inspected': return 'success';
    case 'SETTLED': return 'success';
    case 'CANCELLED': return 'danger';
    default: return 'info';
  }
};

// Excel Download
const handleDownload = () => {
  if (filteredEmployees.value.length === 0) {
    ElMessage.warning('다운로드할 직원 데이터가 없습니다.');
    return;
  }

  downloadLoading.value = true;
  import('@/vendor/Export2Excel').then(excel => {
    const tHeader = ['이름', '아이디', '최근 접속일시', '등록일시'];
    const filterVal = ['name', 'username', 'lastLoginAt', 'createdAt'];
    
    const data = filteredEmployees.value.map(v => filterVal.map(j => {
      if (j === 'lastLoginAt' || j === 'createdAt') {
        return formatDateTime(v[j]);
      } else {
        return v[j];
      }
    }));

    excel.export_json_to_excel({
      header: tHeader,
      data,
      filename: '직원목록_' + dayjs().format('YYYYMMDD'),
      autoWidth: true,
      bookType: 'xlsx'
    });

    downloadLoading.value = false;
  }).catch(err => {
    console.error(err);
    ElMessage.error('엑셀 변환 중 오류가 발생했습니다.');
    downloadLoading.value = false;
  });
};
</script>

<style lang="scss" src="./components/CustomerStyles.scss" scoped></style>
