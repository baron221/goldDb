<template>
<div class="app-container">
    <el-card shadow="never">
      <template #header>
        <div class="card-header">
          <span>시스템 개인화 설정</span>
        </div>
      </template>

      <el-tabs v-model="activeTab" class="settings-tabs">
        <el-tab-pane label="개인화 설정" name="personal">
          <el-form :label-position="isMobile ? 'top' : 'left'" :label-width="isMobile ? '100%' : '250px'" style="max-width: 800px; margin-top: 1.25rem;">
            <el-form-item label="글자 크기 설정">
              <div class="setting-item slider-container">
                <el-slider
                  v-model="internalSliderValue"
                  :min="0"
                  :max="9"
                  :format-tooltip="formatTooltip"
                  :marks="activeMarks"
                  @change="handleSizeChange"
                />
                <span class="setting-hint slider-hint">시스템 전체의 글자 및 컴포넌트 크기를 조절합니다.</span>
              </div>
            </el-form-item>

            <el-form-item label="Tags-View 사용">
              <div class="setting-item">
                <el-switch v-model="tagsView" />
                <span class="setting-hint">상단에 열려있는 페이지를 탭 형태로 표시하여 빠른 이동을 지원합니다.</span>
              </div>
            </el-form-item>

            <el-form-item label="Header 고정">
              <div class="setting-item">
                <el-switch v-model="fixedHeader" />
                <span class="setting-hint">스크롤 시에도 상단 헤더(검색, 프로필 등)를 화면 상단에 고정합니다.</span>
              </div>
            </el-form-item>

            <el-form-item label="사이드바 로고 보기">
              <div class="setting-item">
                <el-switch v-model="sidebarLogo" />
                <span class="setting-hint">왼쪽 메뉴 사이드바 상단에 시스템 로고와 타이틀을 표시합니다.</span>
              </div>
            </el-form-item>

            <el-form-item label="사이드바 하위 메뉴 팝업 표시">
              <div class="setting-item">
                <el-switch v-model="secondMenuPopup" />
                <span class="setting-hint">사이드바가 접힌 상태이거나 특정 모드에서 하위 메뉴를 팝업 형태로 표시합니다.</span>
              </div>
            </el-form-item>
          </el-form>
        </el-tab-pane>

        <el-tab-pane label="고정 메뉴 관리" name="fixed">
          <div class="tab-content-wrapper">
            <h3>고정 메뉴 순서 및 관리</h3>
            <div class="setting-item fixed-menus-container">
              <div ref="fixedListRef" class="fixed-menus-list vertical">
                <div
                  v-for="menu in fixedMenus"
                  :key="menu.id"
                  class="fixed-menu-item"
                  :data-id="menu.id"
                >
                  <el-icon class="drag-handle"><Rank /></el-icon>
                  <span class="menu-title">{{ menu.title }}</span>
                  <el-icon class="remove-btn" @click="handleUnfix(menu)"><Close /></el-icon>
                </div>
                <div v-if="fixedMenus.length === 0" class="no-fixed-menus">
                  고정된 메뉴가 없습니다.
                </div>
              </div>
              <div class="setting-hint" style="margin-top: 0.9375rem; margin-left: 0;">
                상단 탭에 항상 고정되어 표시되는 메뉴들입니다. 드래그하여 순서를 변경하거나 'X'를 눌러 해제할 수 있습니다.
              </div>
            </div>
          </div>
        </el-tab-pane>

        <el-tab-pane label="즐겨찾기 관리" name="favorite">
          <div class="tab-content-wrapper">
            <h3>즐겨찾기 메뉴 순서 및 관리</h3>
            <div class="setting-item favorite-menus-container">
              <div ref="favoriteListRef" class="favorite-menus-list">
                <div
                  v-for="menu in favoriteMenus"
                  :key="menu.menuId"
                  class="favorite-menu-item"
                  :data-id="menu.menuId"
                >
                  <el-icon class="drag-handle"><Rank /></el-icon>
                  <span class="menu-title">{{ menu.title }}</span>
                  <el-icon class="remove-btn" @click="handleUnfavorite(menu)"><Close /></el-icon>
                </div>
                <div v-if="favoriteMenus.length === 0" class="no-favorite-menus">
                  즐겨찾기한 메뉴가 없습니다.
                </div>
              </div>
              <div class="setting-hint" style="margin-top: 0.9375rem; margin-left: 0;">
                사이드바 상단에 표시되는 즐겨찾기 메뉴입니다. 드래그하여 순서를 변경하거나 'X'를 눌러 해제할 수 있습니다.
              </div>
            </div>
          </div>
        </el-tab-pane>
      </el-tabs>

      <div class="footer-tip">
        <el-alert
          title="설정 안내"
          type="info"
          description="이 설정은 브라우저 세션 동안 유지되며, 로컬 저장소에 저장되어 다음 접속 시에도 유지됩니다."
          show-icon
          :closable="false"
        />
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import store from '@/store';
import { ElMessage } from 'element-plus';
import { updateUserMenuSetting, updateUserMenuSettingSort } from '@/api/user';
import { getMenuFavoritesDetail, updateMenuFavoriteSort, toggleMenuFavorite } from '@/api/menu-favorite';
import { Rank, Close } from '@element-plus/icons-vue';
import Sortable from 'sortablejs';

const findAffixTags = (routes: any[]) => {
  let tags: any[] = [];
  routes.forEach(route => {
    if (route.meta && route.meta.affix) {
      tags.push({
        id: route.meta.id,
        title: route.meta.title,
        affixSortOrder: route.meta.affixSortOrder || 0
      });
    }
    if (route.children) {
      tags = [...tags, ...findAffixTags(route.children)];
    }
  });
  return tags;
};

const settingsStore = store.settings();
const appStore = store.app();
const route = useRoute();
const router = useRouter();

const activeTab = ref('personal');
const isMobile = computed(() => appStore.device === 'mobile');
const fixedMenus = ref<any[]>([]);
const favoriteMenus = ref<any[]>([]);
const favoriteListRef = ref<HTMLElement | null>(null);
const fixedListRef = ref<HTMLElement | null>(null);

const getFixedMenus = () => {
  const tags = findAffixTags(store.permission().routes);
  fixedMenus.value = tags.sort((a, b) => a.affixSortOrder - b.affixSortOrder);
};

const getFavoriteMenus = async () => {
  try {
    const res = await getMenuFavoritesDetail();
    favoriteMenus.value = res.data || [];
  } catch (error) {
    console.error(error);
  }
};

const handleUnfavorite = async (menu: any) => {
  try {
    const res = await toggleMenuFavorite(menu.menuId);
    if (res.code === 20000) {
      ElMessage.success(`'${menu.title}' 즐겨찾기가 해제되었습니다.`);
      await store.permission().generateRoutes(store.user().roles);
      getFavoriteMenus();
    }
  } catch (error) {
    console.error(error);
  }
};

const initFavoriteSortable = () => {
  if (!favoriteListRef.value) return;
  Sortable.create(favoriteListRef.value, {
    handle: '.drag-handle',
    animation: 150,
    onEnd: async (evt) => {
      const { oldIndex, newIndex } = evt;
      if (oldIndex === newIndex) return;

      const items = [...favoriteMenus.value];
      const movedItem = items.splice(oldIndex!, 1)[0];
      items.splice(newIndex!, 0, movedItem);

      favoriteMenus.value = items;

      const sortData = items.map((item, index) => ({
        menuId: item.menuId,
        sortOrder: index + 1
      }));

      try {
        await updateMenuFavoriteSort({ items: sortData });
        await store.permission().generateRoutes(store.user().roles);
      } catch (error) {
        console.error(error);
        ElMessage.error('순서 저장에 실패했습니다.');
      }
    }
  });
};

const handleUnfix = async (menu: any) => {
  try {
    const res = await updateUserMenuSetting({
      menuId: menu.id,
      affix: false
    });
    if (res.code === 20000 || res.success) {
      ElMessage.success(`'${menu.title}' 고정이 해제되었습니다.`);
      await store.permission().generateRoutes(store.user().roles);
      getFixedMenus();
    }
  } catch (error) {
    console.error(error);
  }
};

onMounted(() => {
  getFixedMenus();
  getFavoriteMenus();
  initFavoriteSortable();
  initFixedSortable();
});

const initFixedSortable = () => {
  if (!fixedListRef.value) return;
  Sortable.create(fixedListRef.value, {
    handle: '.drag-handle',
    animation: 150,
    onEnd: async (evt) => {
      const { oldIndex, newIndex } = evt;
      if (oldIndex === newIndex) return;

      const items = [...fixedMenus.value];
      const movedItem = items.splice(oldIndex!, 1)[0];
      items.splice(newIndex!, 0, movedItem);

      fixedMenus.value = items;

      const sortData = items.map((item, index) => ({
        menuId: item.id,
        sortOrder: index + 1
      }));

      try {
        await updateUserMenuSettingSort({ items: sortData });
        await store.permission().generateRoutes(store.user().roles);
        getFixedMenus();
      } catch (error) {
        console.error(error);
        ElMessage.error('순서 저장에 실패했습니다.');
      }
    }
  });
};

const sizeOptions = ['s10', 's11', 's12', 's13', 's14', 's16', 's18', 's20', 's22', 's24'];
const sizeLabels = [
  '매우 작게 (10px)', '아주 작게 (11px)', '약간 작게 (12px)', '작게 (13px)', '기본 (14px)', '약간 크게 (16px)',
  '크게 (18px)', '매우 크게 (20px)', '거대하게 (22px)', '울트라 (24px)'
];

const allMarks = {
  0: '10px',
  1: '11px',
  2: '12px',
  3: '13px',
  4: '14px',
  5: '16px',
  6: '18px',
  7: '20px',
  8: '22px',
  9: '24px'
};

const internalSliderValue = ref(sizeOptions.indexOf(appStore.size));

watch(() => appStore.size, (newSize) => {
  const index = sizeOptions.indexOf(newSize);
  if (index !== -1 && index !== internalSliderValue.value) {
    internalSliderValue.value = index;
  }
});

const activeMarks = computed(() => {
  const index = internalSliderValue.value;
  return {
    [index]: allMarks[index as keyof typeof allMarks]
  };
});

const formatTooltip = (val: number) => {
  return sizeLabels[val];
};

const handleSizeChange = (val: number) => {
  const size = sizeOptions[val];
  appStore.setSize(size);
  refreshView();
  ElMessage({
    message: `글자 크기가 '${sizeLabels[val]}'로 변경되었습니다.`,
    type: 'success'
  });
};

const refreshView = () => {
  store.tagsView().delAllCachedViews();
  const { fullPath } = route;
  router.replace({
    path: '/redirect' + fullPath
  });
};

const tagsView = computed({
  get: () => settingsStore.tagsView,
  set: (val) => settingsStore.changeSetting({ key: 'tagsView', value: val })
});

const fixedHeader = computed({
  get: () => settingsStore.fixedHeader,
  set: (val) => settingsStore.changeSetting({ key: 'fixedHeader', value: val })
});

const sidebarLogo = computed({
  get: () => settingsStore.sidebarLogo,
  set: (val) => settingsStore.changeSetting({ key: 'sidebarLogo', value: val })
});

const secondMenuPopup = computed({
  get: () => settingsStore.secondMenuPopup,
  set: (val) => settingsStore.changeSetting({ key: 'secondMenuPopup', value: val })
});
</script>

<style scoped>
@import "./SystemSettingsStyles.scss";
</style>

