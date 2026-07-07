<template>
<div :class="{ 'has-logo': showLogo }">
    <logo v-if="showLogo" :collapse="isCollapse" />
    <div class="sidebar-action-box" :class="{'is-collapse': isCollapse}">
      <div v-if="!isCollapse" class="search-input-wrapper">
        <el-input
          v-model="filterText"
          placeholder="메뉴 검색"
          :prefix-icon="Search"
          size="small"
          class="el-input-primary-no-border"
          clearable
        />
      </div>
      <el-tooltip content="메뉴 새로고침" placement="right">
        <div class="refresh-btn" @click="handleRefreshMenu">
          <el-icon :class="{ 'is-loading': refreshing }"><Refresh /></el-icon>
        </div>
      </el-tooltip>
    </div>
    <div v-if="!isCollapse" class="sidebar-tabs">
      <div class="tab-item" :class="{active: activeTab === 'fav'}" @click="activeTab = 'fav'">
        즐겨찾기
      </div>
      <div class="tab-item" :class="{active: activeTab === 'all'}" @click="activeTab = 'all'">
        전체메뉴
      </div>
    </div>
    <el-scrollbar wrap-class="scrollbar-wrapper">
      <el-menu class="left-menu" :default-active="activeMenu" :collapse="isCollapse"
               background-color="transparent" text-color="var(--menu-text)" :unique-opened="false"
               active-text-color="var(--menu-active-text)" :collapse-transition="false" mode="vertical"
               :default-openeds="openedMenus" :key="filterText">

        <template v-if="filterText">
          <sidebar-item v-for="route in filteredRoutes" :key="route.path" :item="route" :base-path="route.path" :is-top-route="true" />
        </template>

        <template v-else>

          <template v-if="!isCollapse && activeTab === 'fav'">
            <sidebar-item v-for="route in favoriteRoutes" :key="'fav-' + route.path" :item="route" :base-path="route.path" :is-top-route="true" />
            <div v-if="favoriteRoutes.length === 0" class="no-favorites">즐겨찾는 메뉴가 없습니다.</div>
          </template>

          <template v-else>
            <sidebar-item v-for="route in filteredRoutes" :key="route.path" :item="route" :base-path="route.path" :is-top-route="true" />
          </template>
        </template>
      </el-menu>
    </el-scrollbar>
  </div>
</template>

<script>
import { defineComponent, markRaw } from 'vue';
import { mapState } from 'pinia';
import path from 'path-browserify';
import Logo from './Logo';
import SidebarItem from './SidebarItem';
import { Refresh, Search } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import store from '@/store';

export default defineComponent({
  name: 'Sidebar',
  components: {
    SidebarItem,
    Logo,
    Refresh
  },
  data() {
    return {
      refreshing: false,
      filterText: '',
      activeTab: 'all',
      Search: markRaw(Search)
    };
  },
  computed: {
    ...mapState(store.app, ['sidebar']),
    ...mapState(store.permission, {
      permission_routes: 'routes'
    }),
    ...mapState(store.settings, {
      secondMenuPopup: 'secondMenuPopup'
    }),
    activeMenu() {
      const route = this.$route;
      const { meta, path } = route;

      if (meta.activeMenu) {
        return meta.activeMenu;
      }
      return path;
    },
    showLogo() {
      return store.settings().sidebarLogo;
    },
    isCollapse() {
      if (this.secondMenuPopup) {
        return true;
      }
      return !this.sidebar.opened;
    },
    favoriteRoutes() {
      const favorites = [];

      const collect = (routes, basePath = '') => {
        routes.forEach(route => {

          let fullPath = route.path;
          if (basePath && !route.path.startsWith('/')) {
            fullPath = path.resolve(basePath, route.path);
          } else if (!route.path.startsWith('/')) {
            fullPath = '/' + route.path;
          }

          if (route.meta && route.meta.isFavorite) {
            favorites.push({
              ...route,
              path: fullPath, 
              children: []
            });
          }
          if (route.children) {
            collect(route.children, fullPath);
          }
        });
      };
      collect(this.permission_routes);

      return favorites.sort((a, b) => (a.meta.favoriteSortOrder || 0) - (b.meta.favoriteSortOrder || 0));
    },
    openedMenus() {
      if (!this.filterText) {
        return [];
      }

      const openedPaths = new Set();

      const collect = (routes, basePath) => {
        routes.forEach(route => {
          if (route.children && route.children.length > 0) {

            const fullPath = path.resolve(basePath, route.path);
            openedPaths.add(fullPath);

            collect(route.children, fullPath);
          }
        });
      };

      this.filteredRoutes.forEach(route => {
        if (route.children && route.children.length > 0) {

          const fullPath = path.resolve(route.path, route.path);
          openedPaths.add(fullPath);
          collect(route.children, fullPath);
        }
      });

      return Array.from(openedPaths);
    },
    filteredRoutes() {
      if (!this.filterText) {
        return this.permission_routes;
      }
      const query = this.filterText.toLowerCase();

      const filter = (routes, basePath = '') => {
        const res = [];
        routes.forEach(route => {

          if (route.meta && route.meta.hidden) return;

          const tmp = { ...route };
          const title = (tmp.meta && tmp.meta.title) || '';

          let fullPath = tmp.path;
          if (basePath && !tmp.path.startsWith('/')) {
            fullPath = path.resolve(basePath, tmp.path);
          } else if (!tmp.path.startsWith('/')) {
            fullPath = '/' + tmp.path;
          }

          const matches = title.toLowerCase().includes(query) ||
                          tmp.path.toLowerCase().includes(query) ||
                          fullPath.toLowerCase().includes(query);

          let childrenMatches = false;
          if (tmp.children) {
            const filteredChildren = filter(tmp.children, fullPath);
            if (filteredChildren.length > 0) {
              tmp.children = filteredChildren;
              childrenMatches = true;
            } else if (!matches) {

              tmp.children = [];
            }
          }

          if (matches || childrenMatches) {

            if (tmp.children && tmp.children.length > 0) {
              tmp.meta = { ...tmp.meta, alwaysShow: true };
            }
            res.push(tmp);
          }
        });
        return res;
      };

      return filter(this.permission_routes);
    }
  },
  methods: {

    async handleRefreshMenu() {
      if (this.refreshing) return;
      this.refreshing = true;
      try {
        const roles = store.user().roles;
        await store.permission().generateRoutes(roles);
        ElMessage.success('메뉴가 갱신되었습니다.');
      } catch (error) {
        console.error(error);
        ElMessage.error('메뉴 갱신 중 오류가 발생했습니다.');
      } finally {
        this.refreshing = false;
      }
    }
  }
});
</script>

<style lang="scss" scoped>
.sidebar-action-box {
  height: 56px;
  display: flex;
  align-items: center;
  padding: 0 0.9375rem;
  background-color: var(--menu-bg);
  border-bottom: 1px solid rgba(255, 255, 255, 0.03);
  gap: 0.625rem;

  &.is-collapse {
    justify-content: center;
    padding: 0;
  }

  .search-input-wrapper {
    flex: 1;
    :deep(.el-input__wrapper) {
      background-color: rgba(255, 255, 255, 0.03) !important;
      box-shadow: none !important;
      border: 1px solid rgba(255, 255, 255, 0.06);
      border-radius: 2px;
      &:hover, &.is-focus { border-color: var(--menu-active-text); }
    }
    :deep(.el-input__inner) {
      color: #ffffff;
      font-size: 0.8875rem;
      font-family: 'S-CoreDream', sans-serif;
      letter-spacing: 0.5px;
      &::placeholder { color: #666; }
    }
  }

  .refresh-btn {
    color: #666666;
    cursor: pointer;
    transition: all 0.3s;
    &:hover { color: var(--menu-active-text); transform: rotate(180deg); }
  }
}

.sidebar-tabs {
  display: flex;
  height: 48px;
  background-color: var(--menu-bg);
  border-bottom: 1px solid rgba(255, 255, 255, 0.03);
  padding: 0 0.625rem;
  gap: 0.3125rem;

  .tab-item {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.9575rem;
    font-weight: 600;
    color: #666666;
    cursor: pointer;
    position: relative;
    transition: all 0.3s;
    font-family: 'S-CoreDream', sans-serif;
    text-transform: uppercase;
    letter-spacing: 1px;
    margin: 0.5rem 0;
    border-radius: 2px;

    &:hover {
      color: #ffffff;
      background-color: rgba(255, 255, 255, 0.02);
    }

    &.active {
      color: var(--menu-active-text);
      background-color: rgba(197, 168, 128, 0.08);
      font-weight: 700;

      &::after {
        content: '';
        position: absolute;
        bottom: -8px;
        left: 20%;
        width: 60%;
        height: 2px;
        background-color: var(--menu-active-text);
        box-shadow: 0 0 8px rgba(197, 168, 128, 0.5);
      }
    }
  }
}

.left-menu {
  border: none;
  font-family: 'S-CoreDream', 'Jost', sans-serif;
}

.no-favorites {
  padding: 2.5rem 1.25rem;
  font-size: 0.8875rem;
  color: #666666;
  text-align: center;
  font-family: 'S-CoreDream', sans-serif;
  text-transform: uppercase;
  letter-spacing: 1px;
}
</style>

