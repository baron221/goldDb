import { defineStore } from 'pinia';
import Cookies from 'js-cookie';
import { getToken } from '@/utils/auth';
import { updateUserPersonalSettings } from '@/api/user';
import settingsStore from './settings';

interface IAppState {

  sidebar: {
    opened: boolean;
    withoutAnimation: boolean;
  };

  device: 'desktop' | 'mobile';

  size: 's11' | 's12' | 's14' | 's16' | 's18' | 's20' | 's22' | 's24';
}

export default defineStore({
  id: 'app',
  state: ():IAppState => ({
    sidebar: {
      opened: Cookies.get('sidebarStatus') ? !!+Cookies.get('sidebarStatus') : true,
      withoutAnimation: false
    },
    device: 'desktop',
    size: (localStorage.getItem('size') as any) || 's12'
  }),
  getters: {},
  actions: {

    toggleSidebar() {
      this.sidebar.opened = !this.sidebar.opened;
      this.sidebar.withoutAnimation = false;
      if (this.sidebar.opened) {
        Cookies.set('sidebarStatus', 1);
      } else {
        Cookies.set('sidebarStatus', 0);
      }
    },

    closeSidebar({ withoutAnimation }) {
      Cookies.set('sidebarStatus', 0);
      this.sidebar.opened = false;
      this.sidebar.withoutAnimation = withoutAnimation;
    },

    toggleDevice(device) {
      this.device = device;
    },

    setSize(size) {
      this.size = size;
      localStorage.setItem('size', size);

      if (getToken()) {
        const settings = settingsStore();
        updateUserPersonalSettings({
          size: size,
          tagsView: settings.tagsView,
          fixedHeader: settings.fixedHeader,
          sidebarLogo: settings.sidebarLogo,
          secondMenuPopup: settings.secondMenuPopup
        }).catch(err => console.error('Failed to sync size to DB:', err));
      }
    }
  }
});
