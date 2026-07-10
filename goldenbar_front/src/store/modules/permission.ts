import { defineStore } from 'pinia';
import { constantRoutes } from '@/router';
import type { RouteRecordRaw } from 'vue-router';
import { getMenus } from '@/api/user';

const Layout = () => import('@/layout/index.vue');

const modules = import.meta.glob('../../views/**/*.vue');

interface IPermissionState {

  routes: Array<RouteRecordRaw>;

  addRoutes: Array<RouteRecordRaw>;

  permissions: Record<string, any>;

  allMenuPaths: string[];
}

export function filterAsyncRoutes(routes:RouteRecordRaw[], roles: string[]): Array<RouteRecordRaw> {
  const res:Array<RouteRecordRaw> = [];

  routes.forEach(route => {
    const tmp = { ...route };
    if (hasPermission(roles, tmp)) {
      if (tmp.children) {
        tmp.children = filterAsyncRoutes(tmp.children, roles);
      }
      res.push(tmp);
    }
  });

  return res;
}

function hasPermission(roles:string[], route:RouteRecordRaw):boolean {

  if (roles.includes('admin')) return true;

  if (route.meta && route.meta.canSearch) {
    return true;
  }

  return false;
}

export function transformMenus(menus: any[], permissionMap: Record<string, any> = {}, allPaths: string[] = [], parentPath = ''): RouteRecordRaw[] {
  const res: RouteRecordRaw[] = [];

  menus.forEach(menu => {

    let fullPath = menu.path;
    if (parentPath && !menu.path.startsWith('/')) {
      fullPath = parentPath + (parentPath.endsWith('/') ? '' : '/') + menu.path;
    } else if (parentPath && menu.path.startsWith('/')) {
      fullPath = menu.path;
    }
    allPaths.push(fullPath);

    const route: any = {
      path: menu.path,
      name: menu.name,
      meta: {
        id: menu.id,
        isFavorite: !!menu.isFavorite,
        favoriteSortOrder: menu.favoriteSortOrder || 0,
        title: menu.meta?.title,
        icon: menu.meta?.icon,
        noCache: menu.meta?.noCache,
        keepAlive: menu.meta?.keepAlive === true,
        affix: menu.meta?.affix,
        affixSortOrder: menu.meta?.affixSortOrder || 0,
        hidden: menu.meta?.hidden === true,
        alwaysShow: menu.meta?.alwaysShow === true,
        activeMenu: menu.meta?.activeMenu,

        canSearch: menu.meta?.canSearch,
        canCreate: menu.meta?.canCreate,
        canDelete: menu.meta?.canDelete,
        canSave: menu.meta?.canSave,
        canPrint: menu.meta?.canPrint,
        custom1: menu.meta?.custom1,
        custom2: menu.meta?.custom2,
        custom3: menu.meta?.custom3,
        custom4: menu.meta?.custom4,
        custom5: menu.meta?.custom5,
        custom6: menu.meta?.custom6,
        custom7: menu.meta?.custom7,
        custom8: menu.meta?.custom8
      }
    };

    if (menu.redirect) {
      route.redirect = menu.redirect;
    }

    const key = route.name || route.path;
    permissionMap[key] = route.meta;

    if (menu.component === 'Layout') {
      route.component = Layout;
    } else if (menu.component) {
      let componentPath = menu.component.startsWith('/') ? menu.component.substring(1) : menu.component;
      if (componentPath.startsWith('views/')) {
        componentPath = componentPath.substring('views/'.length);
      }
      const fullPathToComp = `../../views/${componentPath}.vue`;
      if (modules[fullPathToComp]) {
        route.component = modules[fullPathToComp];
      } else {
        console.error(`Component not found in src/views: ${fullPathToComp}`);
      }
    }

    if (menu.children && menu.children.length > 0) {
      route.children = transformMenus(menu.children, permissionMap, allPaths, fullPath);
    }

    res.push(route);
  });

  return res;
}

export default defineStore({
  id: 'permission',
  state: ():IPermissionState => ({
    routes: [],
    addRoutes: [],
    permissions: {},
    allMenuPaths: []
  }),
  getters: {},
  actions: {

    setRoutes(routes: RouteRecordRaw[]) {
      this.addRoutes = routes;
      this.routes = routes;
    },

    resetRoutes() {
      this.routes = [];
      this.addRoutes = [];
      this.permissions = {};
      this.allMenuPaths = [];
    },

    async generateRoutes(roles: string[]) {
      try {
        const response = await getMenus();
        const permissionMap = {};
        const allPaths: string[] = [];

        const asyncRoutesFromBackend = transformMenus(response.data, permissionMap, allPaths);
        this.permissions = permissionMap;
        this.allMenuPaths = allPaths;

        let accessedRoutes;
        if (roles.includes('admin')) {
          accessedRoutes = asyncRoutesFromBackend || [];
        } else {
          accessedRoutes = filterAsyncRoutes(asyncRoutesFromBackend, roles);
        }

        accessedRoutes.push({ path: '/:pathMatch(.*)*', redirect: '/404', meta: { hidden: true }});

        this.setRoutes(accessedRoutes);
        return accessedRoutes;
      } catch (error) {
        console.error('Failed to generate routes:', error);
        return [];
      }
    }
  }
});
