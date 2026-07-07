import { defineStore } from 'pinia';
import defaultSettings from '@/settings';
import { getToken } from '@/utils/auth';
import { updateUserPersonalSettings } from '@/api/user';
import appStore from './app';

const { showSettings, tagsView, fixedHeader, sidebarLogo, secondMenuPopup } = defaultSettings;

export default defineStore({
  id: 'settings',
  state: () => ({

    theme: '#1890ff',

    showSettings: showSettings,

    tagsView: localStorage.getItem('tagsView') !== null ? localStorage.getItem('tagsView') === 'true' : tagsView,

    fixedHeader: localStorage.getItem('fixedHeader') !== null ? localStorage.getItem('fixedHeader') === 'true' : fixedHeader,

    sidebarLogo: localStorage.getItem('sidebarLogo') !== null ? localStorage.getItem('sidebarLogo') === 'true' : sidebarLogo,

    secondMenuPopup: localStorage.getItem('secondMenuPopup') !== null ? localStorage.getItem('secondMenuPopup') === 'true' : secondMenuPopup,

    isDark: document.documentElement.classList.contains('dark') || localStorage.getItem('theme') === 'dark'
  }),
  getters: {},
  actions: {

    changeSetting({ key, value }) {
      if (key in this) {
        this[key] = value;

        if (['tagsView', 'fixedHeader', 'sidebarLogo', 'secondMenuPopup'].includes(key)) {
          localStorage.setItem(key, String(value));

          if (getToken()) {
            updateUserPersonalSettings({
              size: appStore().size,
              tagsView: this.tagsView,
              fixedHeader: this.fixedHeader,
              sidebarLogo: this.sidebarLogo,
              secondMenuPopup: this.secondMenuPopup
            }).catch(err => console.error('Failed to sync settings to DB:', err));
          }
        }
      }
    },

    toggleDark() {
      this.isDark = !this.isDark;
      if (this.isDark) {
        document.documentElement.classList.add('dark');
        localStorage.setItem('theme', 'dark');
      } else {
        document.documentElement.classList.remove('dark');
        localStorage.setItem('theme', 'light');
      }
    }
  }
});
