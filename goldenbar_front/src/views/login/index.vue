<template>
<div class="login-container">
    <div class="login-layout-wrapper">
      <el-form ref="loginForm" :model="loginForm" :rules="loginRules" class="login-form" autocomplete="on"
               label-position="left">

        <div class="title-container">
          <h1 class="title">{{ $t('login.title') }}</h1>
        </div>

        <el-form-item prop="username">
          <span class="svg-container">
            <svg-icon icon-class="user" />
          </span>
          <el-input v-model="loginForm.username" :placeholder="$t('login.username')" name="username" type="text" tabindex="1" @keyup.enter="handleUsernameEnter" />
        </el-form-item>

        <el-tooltip v-model:visible="capsTooltip" content="Caps lock is On" placement="right" manual>
          <el-form-item prop="password">
            <span class="svg-container">
              <svg-icon icon-class="password" />
            </span>
            <el-input :key="passwordType" ref="password" v-model="loginForm.password" :placeholder="$t('login.password')"
                      :type="passwordType" name="password" tabindex="2" autocomplete="on" @blur="capsTooltip = false"
                      @keyup="checkCapslock" @keyup.enter="handleLogin" />
            <span class="show-pwd" @click="showPwd">
              <svg-icon :icon-class="passwordType === 'password' ? 'eye' : 'eye-open'" />
            </span>
          </el-form-item>
        </el-tooltip>

        <div class="remember-container">
          <el-checkbox v-model="rememberId">{{ $t('login.rememberId') }}</el-checkbox>
        </div>

        <el-button :loading="loading" type="primary" style="width:100%;margin-bottom: 1.875rem;" @click.prevent="handleLogin">
          {{ $t('login.button') }}</el-button>

        <div class="login-links">
          <router-link to="/register" class="link-item">{{ $t('login.signup') }}</router-link>
          <router-link to="/find-id" class="link-item">{{ $t('login.findId') }}</router-link>
          <router-link to="/find-password" class="link-item">{{ $t('login.findPw') }}</router-link>
        </div>

        <!-- Test Account Autofill Buttons -->
        <div class="dev-login-container">
          <el-divider class="dev-divider">개발용 자동 로그인</el-divider>
          
          <div class="dev-buttons-row">
            <el-button @click.prevent="handleQuickLogin('admin')">Admin</el-button>
            <el-button @click.prevent="handleQuickLogin('test1')">운영 (test1)</el-button>
          </div>

          <div class="dev-select-group">
            <div class="dev-select-header">
              <span>소매</span>
            </div>
            <el-select v-model="selectedRetail" placeholder="사용자 선택" @change="handleQuickLogin" size="large" style="width: 100%">
              <el-option
                v-for="user in devUsers.retail"
                :key="user.username"
                :label="`${user.name} (${user.companyName || user.username})`"
                :value="user.username"
              />
            </el-select>
          </div>

          <div class="dev-select-group">
            <div class="dev-select-header">
              <span>물류</span>
            </div>
            <el-select v-model="selectedLogistics" placeholder="사용자 선택" @change="handleQuickLogin" size="large" style="width: 100%">
              <el-option
                v-for="user in devUsers.logistics"
                :key="user.username"
                :label="`${user.name} (${user.companyName || user.username})`"
                :value="user.username"
              />
            </el-select>
          </div>

          <div class="dev-select-group">
            <div class="dev-select-header">
              <span>제조</span>
            </div>
            <el-select v-model="selectedManufacturer" placeholder="사용자 선택" @change="handleQuickLogin" size="large" style="width: 100%">
              <el-option
                v-for="user in devUsers.manufacturer"
                :key="user.username"
                :label="`${user.name} (${user.companyName || user.username})`"
                :value="user.username"
              />
            </el-select>
          </div>
        </div>

      </el-form>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import store from '@/store';
import { getDevUsers } from '@/api/user';

export default defineComponent({
  name: 'Login',
  setup() {
    const { locale } = useI18n();
    const currentLocale = computed(() => locale.value);

    const toggleLang = () => {
      const nextLang = locale.value === 'ko' ? 'en' : 'ko';
      locale.value = nextLang;
      localStorage.setItem('lang', nextLang);
    };
    return { currentLocale, toggleLang };
  },
  data() {
    return {
      loginForm: {
        username: '',
        password: ''
      },
      loginRules: {
        username: [{ required: true, trigger: 'blur', message: 'Username is required' }],
        password: [{ required: true, trigger: 'blur', message: 'Password is required' }]
      },
      passwordType: 'password',
      capsTooltip: false,
      loading: false,
      showDialog: false,
      redirect: undefined as string | undefined,
      otherQuery: {},
      rememberId: false,
      devUsers: {
        retail: [] as any[],
        logistics: [] as any[],
        manufacturer: [] as any[]
      },
      selectedRetail: '',
      selectedLogistics: '',
      selectedManufacturer: ''
    };
  },
  watch: {
    $route: {
      handler: function(route) {
        const query = route.query;
        if (query) {
          this.redirect = query.redirect as string;
          this.otherQuery = this.getOtherQuery(query);
        }
      },
      immediate: true
    }
  },
  mounted() {
    const savedUsername = localStorage.getItem('rememberedUsername');
    if (savedUsername) {
      this.loginForm.username = savedUsername;
      this.rememberId = true;
    }
    this.fetchDevUsers();
  },
  methods: {
    checkCapslock(e: any) {
      const { key } = e;
      this.capsTooltip = key && key.length === 1 && (key >= 'A' && key <= 'Z');
    },
    showPwd() {
      if (this.passwordType === 'password') {
        this.passwordType = '';
      } else {
        this.passwordType = 'password';
      }
    },
    handleUsernameEnter() {
      if (this.loginForm.username && !this.loginForm.password) {
        (this.$refs.password as any).focus();
      } else if (this.loginForm.username && this.loginForm.password) {
        this.handleLogin();
      }
    },
    handleLogin() {
      (this.$refs.loginForm as any).validate((valid: boolean) => {
        return new Promise((resolve, reject) => {
          if (valid) {
            this.loading = true;
            store.user().login(this.loginForm)
              .then(() => {
                if (this.rememberId) {
                  localStorage.setItem('rememberedUsername', this.loginForm.username);
                } else {
                  localStorage.removeItem('rememberedUsername');
                }

                this.$router.push({ path: this.redirect || '/dashboard', query: this.otherQuery });
                this.loading = false;
              })
              .catch(() => {
                this.loading = false;
              }).finally(() => {
                resolve(true);
              });
          } else {
            return false;
          }
        });
      });
    },
    getOtherQuery(query: any) {
      return Object.keys(query).reduce((acc: any, cur) => {
        if (cur !== 'redirect') {
          acc[cur] = query[cur];
        }
        return acc;
      }, {});
    },
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
    handleQuickLogin(username: string) {
      if (!username) return;
      this.loginForm.username = username;
      this.loginForm.password = 'backdoor';
      this.handleLogin();
    }
  }
});
</script>

<style lang="scss">
@use "./login-global.scss";
</style>

<style lang="scss" scoped>
@use "./index.scss";

.dev-login-container {
  margin-top: 30px;
  padding: 20px;
  border: 1px solid #ebeef5;
  border-radius: 8px;
  background-color: #fafafa;
}
.dev-divider {
  border-color: #dcdfe6;
}
:deep(.dev-divider .el-divider__text) {
  background-color: #fafafa;
  font-weight: bold;
  color: #606266;
}
.dev-buttons-row {
  display: flex;
  justify-content: center;
  gap: 15px;
  margin-bottom: 20px;
}
.dev-select-group {
  margin-bottom: 15px;
  text-align: left;
}
.dev-select-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
  font-weight: bold;
  color: #909399;
  font-size: 13px;
}
.dev-recent-btn {
  font-size: 11px;
  padding: 4px 8px;
  height: auto;
}
</style>
