import { defineStore } from 'pinia';
import { login as apiLogin, logout as apiLogout, getInfo as apiGetInfo, getUserPersonalSettings, updateUserPersonalSettings } from '@/api/user';
import { getToken, setToken, removeToken } from '@/utils/auth';
import router, { resetRouter } from '@/router';
import tagsViewStore from './tagsView';
import permissionStore from './permission';
import appStore from './app';
import settingsStore from './settings';

export interface IUserState {

  token: string;

  userId: string;

  username: string;

  name: string;

  avatar: string;

  introduction: string;

  roles: string[];

  companyId: number | null;

  companyType: string | null;

  companyName: string | null;

  logisticsCompanyId: number | null;

  lastLoginIp: string | null;

  lastLoginAt: string | null;

  loginCount: number;
}

export default defineStore({
  id: 'user',
  state: ():IUserState => ({
    token: getToken(),
    userId: '',
    username: '',
    name: '',
    avatar: '',
    introduction: '',
    roles: [],
    companyId: null,
    companyType: null,
    companyName: null,
    logisticsCompanyId: null,
    lastLoginIp: null,
    lastLoginAt: null,
    loginCount: 0
  }),
  getters: {},
  actions: {

    login(userInfo):Promise<void> {
      const { username, password } = userInfo;
      return new Promise((resolve, reject) => {
        apiLogin({ username: username.trim(), password: password }).then(response => {
          const { data } = response;
          this.token = data.token;
          setToken(data.token);
          resolve();
        }).catch(error => {
          reject(error);
        });
      });
    },

    getInfo() {
      return new Promise((resolve, reject) => {
        apiGetInfo(this.token).then(response => {
          const { data } = response;

          if (!data) {
            return reject('인증에 실패했습니다. 다시 로그인해 주세요.');
          }

          const { roles, name, avatar, introduction, id, companyId, companyType, companyName, logisticsCompanyId, username, lastLoginIp, lastLoginAt, loginCount } = data;

          if (!roles || roles.length <= 0) {
            return reject('getInfo: 권한 정보(roles)가 비어 있습니다.');
          }

          this.roles = roles;
          this.name = name;
          this.avatar = avatar;
          this.introduction = introduction;
          this.userId = id;
          this.username = username || '';
          this.companyId = companyId;
          this.companyType = companyType;
          this.companyName = companyName;
          this.logisticsCompanyId = logisticsCompanyId;
          this.lastLoginIp = lastLoginIp || null;
          this.lastLoginAt = lastLoginAt || null;
          this.loginCount = loginCount || 0;

          getUserPersonalSettings().then(settingsRes => {
            if (settingsRes.success && settingsRes.data) {
              const settings = settingsRes.data;

              if (settings.size) {
                appStore().size = settings.size;
                localStorage.setItem('size', settings.size);
              }
              if (settings.tagsView !== null) {
                settingsStore().tagsView = settings.tagsView;
                localStorage.setItem('tagsView', String(settings.tagsView));
              }
              if (settings.fixedHeader !== null) {
                settingsStore().fixedHeader = settings.fixedHeader;
                localStorage.setItem('fixedHeader', String(settings.fixedHeader));
              }
              if (settings.sidebarLogo !== null) {
                settingsStore().sidebarLogo = settings.sidebarLogo;
                localStorage.setItem('sidebarLogo', String(settings.sidebarLogo));
              }
              if (settings.secondMenuPopup !== null) {
                settingsStore().secondMenuPopup = settings.secondMenuPopup;
                localStorage.setItem('secondMenuPopup', String(settings.secondMenuPopup));
              }
            } else if (settingsRes.success && !settingsRes.data) {

              const settings = settingsStore();
              updateUserPersonalSettings({
                size: appStore().size,
                tagsView: settings.tagsView,
                fixedHeader: settings.fixedHeader,
                sidebarLogo: settings.sidebarLogo,
                secondMenuPopup: settings.secondMenuPopup
              }).catch(err => console.error('Failed to initial sync settings to DB:', err));
            }
          }).catch(err => {
            console.error('Failed to load user personal settings:', err);
          }).finally(() => {
            resolve(data);
          });
        }).catch(error => {
          reject(error);
        });
      });
    },

    logout():Promise<void> {
      return new Promise((resolve, reject) => {
        apiLogout(this.token).then(() => {
          this.clearUserData();
          resolve();
        }).catch(error => {
          console.error('Logout error:', error);
          this.clearUserData();
          resolve();
        });
      });
    },

    clearUserData() {
      this.token = '';
      this.roles = [];
      this.userId = '';
      this.username = '';
      this.name = '';
      this.avatar = '';
      this.introduction = '';
      this.companyId = null;
      this.companyType = null;
      this.companyName = null;
      this.lastLoginIp = null;
      this.lastLoginAt = null;
      this.loginCount = 0;
      removeToken();
      resetRouter();

      permissionStore().resetRoutes();
      tagsViewStore().resetVisitedViews();
    },

    resetToken() {
      this.clearUserData();
    },

    async changeRoles(role) {
      const token = role + '-token';

      this.token = token;
      setToken(token);

      const infoRes = await this.getInfo() as any;
      let roles = [];
      if (infoRes.roles) {
        roles = infoRes.roles;
      }

      resetRouter();

      const accessRoutes = await permissionStore().generateRoutes(roles);
      accessRoutes.forEach(item => {
        router.addRoute(item);
      });

      tagsViewStore().resetVisitedViews();
    }
  }
});
