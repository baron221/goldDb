<template>
<div class="navbar">
    <hamburger id="hamburger-container" :is-active="sidebar.opened" class="hamburger-container"
               @toggleClick="toggleSidebar" />

    <breadcrumb id="breadcrumb-container" class="breadcrumb-container" />

    <div class="top-menu-wrapper">
      <router-link to="/home" class="top-menu-item">Home</router-link>
    </div>

    <div class="right-menu">
      <template v-if="device !== 'mobile'">
        <search id="header-search" class="right-menu-item" />

        <el-tooltip v-if="isRetail" :content="$t('nav.cart')" effect="dark" placement="bottom">
          <div class="right-menu-item hover-effect" @click="$router.push('/my/cart')">
            <el-badge :value="cartCount" :hidden="cartCount === 0" class="cart-badge"  style="margin-top: 0.375rem;" >
              <el-icon><ShoppingCart /></el-icon>
            </el-badge>
          </div>
        </el-tooltip>

        <el-tooltip v-if="isRetail" :content="$t('nav.orders')" effect="dark" placement="bottom">
          <div class="right-menu-item hover-effect" @click="$router.push('/my/order')">
            <el-icon><Tickets /></el-icon>
          </div>
        </el-tooltip>

        <div class="right-menu-item hover-effect" @click="toggleTheme">
          <el-icon>
            <Sunny v-if="isDark" />
            <Moon v-else />
          </el-icon>
        </div>

        <error-log class="errLog-container right-menu-item hover-effect" />

        <screenfull id="screenfull" class="right-menu-item hover-effect" />

        <div class="right-menu-item hover-effect lang-switch-item" @click="toggleLang">
          <span>{{ currentLocale === 'ko' ? 'KO' : 'EN' }}</span>
        </div>

        <div v-if="companyName" class="right-menu-item company-info-display">
          <span class="c-name">{{ companyName }}</span>
          <span class="c-type">{{ getCompanyTypeName(companyType) }}</span>
        </div>
      </template>

      <el-dropdown class="avatar-container right-menu-item hover-effect" trigger="click" popper-class="user-dropdown-popper">
        <div class="avatar-wrapper" tabindex="0">
          <user-avatar :name="name" :company-name="companyName" :avatar="avatar" :size="30" />
        </div>
        <template #dropdown>
          <el-dropdown-menu class="user-dropdown">

            <el-dropdown-item class="user-dropdown-header-item">
              <div class="user-dropdown-header-modern">
                <div class="header-user-info">
                  <span class="user-name">{{ name }}</span>
                  <span v-if="companyName" class="user-company">{{ companyName }}</span>
                </div>
                <div class="user-dropdown-stats-grid">
                  <div class="stat-item">
                    <span class="stat-label">{{ $t('nav.id') }}</span>
                    <span class="stat-value">{{ username }}</span>
                  </div>
                  <div class="stat-item">
                    <span class="stat-label">{{ $t('nav.ip') }}</span>
                    <span class="stat-value">{{ lastLoginIp || '-' }}</span>
                  </div>
                  <div class="stat-item">
                    <span class="stat-label">{{ $t('nav.recentLogin') }}</span>
                    <span class="stat-value">{{ formatDate(lastLoginAt) }}</span>
                  </div>
                </div>
              </div>
            </el-dropdown-item>

            <el-divider class="dropdown-divider-luxury" />

            <router-link to="/profile/index">
              <el-dropdown-item>
                <el-icon><User /></el-icon>{{ $t('nav.profile') }}
              </el-dropdown-item>
            </router-link>
            <router-link to="/">
              <el-dropdown-item>
                <el-icon><HomeFilled /></el-icon>{{ $t('nav.home') }}
              </el-dropdown-item>
            </router-link>

            <a target="_blank" href="https://goldbdoc.jsini.co.kr/">
              <el-dropdown-item>
                <el-icon><QuestionFilled /></el-icon>{{ $t('nav.help') }}
              </el-dropdown-item>
            </a>

            <el-dropdown-item @click="toggleLang">
              <el-icon><Refresh /></el-icon>
              {{ currentLocale === 'ko' ? 'Switch to English' : '한국어로 변경' }}
            </el-dropdown-item>

            <el-divider class="dropdown-divider-luxury" />

            <el-dropdown-item class="dev-switch-section-item">
              <div class="dev-switch-section-modern">
                <div class="dev-section-title">
                  <el-icon><Refresh /></el-icon>
                  {{ $t('nav.accountSwitch') }} (DEV)
                </div>

                <div class="dev-switch-vertical-list">

                  <div class="switch-group">
                    <span class="group-label">Admin</span>
                    <el-button size="small" class="quick-login-btn" @click="handleQuickLogin('sysadm')">SYS</el-button>
                    <el-button size="small" class="quick-login-btn" @click="handleQuickLogin('admin')">ADM</el-button>
                  </div>

                  <div v-if="devUsers.retail.length > 0" class="switch-group">
                    <span class="group-label">Retail</span>
                    <el-select
                      v-model="selectedRetail"
                      size="small"
                      placeholder="소매점"
                      class="dev-quick-select"
                      filterable
                      @change="handleQuickLogin"
                    >
                      <el-option
                        v-for="user in devUsers.retail"
                        :key="user.username"
                        :label="`${user.name} (${user.companyName || user.username})`"
                        :value="user.username"
                      />
                    </el-select>
                  </div>

                  <div v-if="devUsers.logistics.length > 0" class="switch-group">
                    <span class="group-label">Logis</span>
                    <el-select
                      v-model="selectedLogistics"
                      size="small"
                      placeholder="물류사"
                      class="dev-quick-select"
                      filterable
                      @change="handleQuickLogin"
                    >
                      <el-option
                        v-for="user in devUsers.logistics"
                        :key="user.username"
                        :label="`${user.name} (${user.companyName || user.username})`"
                        :value="user.username"
                      />
                    </el-select>
                  </div>

                  <div v-if="devUsers.manufacturer.length > 0" class="switch-group">
                    <span class="group-label">Mfg</span>
                    <el-select
                      v-model="selectedManufacturer"
                      size="small"
                      placeholder="제조사"
                      class="dev-quick-select"
                      filterable
                      @change="handleQuickLogin"
                    >
                      <el-option
                        v-for="user in devUsers.manufacturer"
                        :key="user.username"
                        :label="`${user.name} (${user.companyName || user.username})`"
                        :value="user.username"
                      />
                    </el-select>
                  </div>

                </div>
              </div>
            </el-dropdown-item>

            <el-divider class="dropdown-divider-luxury" />

            <el-dropdown-item divided @click="logout">
              <el-icon color="#F56C6C"><SwitchButton /></el-icon>
              <span style="color: #F56C6C; font-weight: 600;">{{ $t('nav.logout') }}</span>
            </el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
    </div>
  </div>
</template>

<script>
import { mapState } from 'pinia';
import { useI18n } from 'vue-i18n';
import { defineComponent, markRaw, computed } from 'vue';
import store from '@/store';
import { parseTime } from '@/utils';
import Breadcrumb from '@/components/Breadcrumb';
import Hamburger from '@/components/Hamburger';
import ErrorLog from '@/components/ErrorLog';
import Screenfull from '@/components/Screenfull';
import Search from '@/components/HeaderSearch';
import UserAvatar from '@/components/UserAvatar';
import { ElMessage, ElMessageBox } from 'element-plus';
import { Moon, Sunny, ShoppingCart, Tickets, User, QuestionFilled, SwitchButton, Refresh, HomeFilled } from '@element-plus/icons-vue';

import { getDevUsers } from '@/api/user';

export default defineComponent({
  name: 'Navbar',
  components: {
    Breadcrumb,
    Hamburger,
    ErrorLog,
    Screenfull,
    Search,
    UserAvatar,
    Moon,
    Sunny,
    ShoppingCart,
    Tickets,
    User,
    QuestionFilled,
    SwitchButton,
    Refresh,
    HomeFilled
  },
  setup() {
    const { t, locale } = useI18n();
    const currentLocale = computed(() => locale.value);

    const toggleLang = () => {
      const nextLang = locale.value === 'ko' ? 'en' : 'ko';
      locale.value = nextLang;
      localStorage.setItem('lang', nextLang);
    };
    return { t, currentLocale, toggleLang };
  },
  data() {
    return {
      lastRetailUser: null,
      lastLogisticsUser: null,
      lastManufacturerUser: null,
      devUsers: {
        retail: [],
        logistics: [],
        manufacturer: []
      },
      selectedRetail: '',
      selectedLogistics: '',
      selectedManufacturer: ''
    };
  },
  computed: {
    ...mapState(store.app, [
      'sidebar',
      'device'
    ]),
    ...mapState(store.user, [
      'avatar',
      'name',
      'companyName',
      'username',
      'lastLoginIp',
      'lastLoginAt',
      'loginCount',
      'companyType'
    ]),
    ...mapState(store.settings, [
      'isDark'
    ]),
    cartCount() {
      return store.cart().cartCount;
    },
    isRetail() {
      return this.companyType === 'RTL';
    }
  },
  mounted() {
    if (this.isDark) {
      document.documentElement.classList.add('dark');
    }
    store.cart().fetchCart();
    this.fetchDevUsers();

    const rStr = localStorage.getItem('lastRetailUser');
    if (rStr) {
      try { this.lastRetailUser = JSON.parse(rStr); } catch (e) { console.error(e); }
    }
    const lStr = localStorage.getItem('lastLogisticsUser');
    if (lStr) {
      try { this.lastLogisticsUser = JSON.parse(lStr); } catch (e) { console.error(e); }
    }
    const mStr = localStorage.getItem('lastManufacturerUser');
    if (mStr) {
      try { this.lastManufacturerUser = JSON.parse(mStr); } catch (e) { console.error(e); }
    }
  },
  methods: {

    async fetchDevUsers() {
      try {
        const response = await getDevUsers();
        if (response && response.data) {
          this.devUsers.retail = response.data.retail || [];
          this.devUsers.logistics = response.data.logistics || [];
          this.devUsers.manufacturer = response.data.manufacturer || [];
        }
      } catch (error) {
        console.error('Failed to fetch dev users:', error);
      }
    },

    async handleQuickLogin(username) {
      if (!username) return;
      try {
        await store.user().login({ username, password: 'backdoor' });

        window.location.reload();
      } catch (error) {
        console.error('Quick login failed:', error);
        ElMessage.error(this.t('login.error'));
      }
    },

    getCompanyTypeName(type) {
      const map = {
        'RTL': this.t('home.roles.store.title').split(' ')[0],
        'DCC': this.t('home.roles.logistics.title').split(' ')[0],
        'MFG': this.t('home.roles.factory.title').split(' ')[0],
        'ADM': this.t('home.roles.admin.title').split(' ')[0]
      };
      return map[type] || type;
    },

    formatDate(date) {
      if (!date) return this.t('nav.noRecord');
      return parseTime(date, '{y}-{m}-{d} {h}:{i}:{s}');
    },

    toggleTheme() {
      store.settings().toggleDark();
    },

    toggleSidebar() {
      store.app().toggleSidebar();
    },

    async logout() {
      ElMessageBox.confirm(this.t('nav.logout') + '?', this.t('nav.logout'), {
        confirmButtonText: this.t('common.ok'),
        cancelButtonText: this.t('common.cancel'),
        type: 'warning'
      }).then(async () => {
        await store.user().logout();
        this.$router.push(`/login?redirect=${this.$route.fullPath}`);
        ElMessage.success(this.t('nav.logout'));
      }).catch(() => {

      });
    }
  }
});
</script>

<style lang="scss" src="./NavbarStyles.scss"></style>

