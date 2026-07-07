<template>
<div class="login-container">

    <el-form ref="findPwForm" :model="findPwForm" :rules="findPwRules" class="login-form" autocomplete="on"
             label-position="left">

      <div class="title-container">
        <h1 class="title">{{ $t('findPw.title') }}</h1>
      </div>

      <el-form-item prop="username">
        <span class="svg-container">
          <svg-icon icon-class="user" />
        </span>
        <el-input v-model="findPwForm.username" :placeholder="$t('findPw.username')" name="username" type="text" tabindex="1" />
      </el-form-item>

      <el-form-item prop="email">
        <span class="svg-container">
          <svg-icon icon-class="email" />
        </span>
        <el-input v-model="findPwForm.email" :placeholder="$t('findPw.email')" name="email" type="text" tabindex="2" />
      </el-form-item>

      <el-button :loading="loading" type="primary" style="width:100%;margin-bottom: 1.875rem;" @click.prevent="handleRequestReset">
        {{ $t('findPw.button') }}</el-button>

      <div v-if="resetToken" class="tips" style="background: rgba(0,0,0,0.05); padding: 0.625rem; border-radius: 2px; margin-bottom: 1.25rem;">
        <p style="color: #666; font-size: 0.875rem; margin-bottom: 0.625rem;">[Development Mode] Reset Link:</p>
        <router-link :to="`/reset-password?token=${resetToken}`" style="color: #409EFF; word-break: break-all;">
          {{ fullResetLink }}
        </router-link>
      </div>

      <div class="tips">
        <router-link to="/login">{{ $t('findPw.back') }}</router-link>
      </div>
    </el-form>
  </div>
</template>

<script lang="ts">
import { defineComponent, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { requestResetPassword } from '@/api/user';
import { ElMessage } from 'element-plus';

export default defineComponent({
  name: 'FindPassword',
  setup() {
    const { locale, t } = useI18n();
    const currentLocale = computed(() => locale.value);

    const changeLang = (lang: string) => {
      locale.value = lang;
      localStorage.setItem('lang', lang);
    };
    return { t, currentLocale, changeLang };
  },
  data() {
    return {
      findPwForm: {
        username: '',
        email: ''
      },
      loading: false,
      resetToken: ''
    };
  },
  computed: {
    findPwRules(): any {
      return {
        username: [{ required: true, trigger: 'blur', message: this.t('register.rules.username') }],
        email: [{ required: true, trigger: 'blur', message: this.t('register.rules.email') }, { type: 'email', message: this.t('register.rules.emailInvalid') }]
      };
    },
    fullResetLink(): string {
      if (!this.resetToken) return '';
      return window.location.origin + '#/reset-password?token=' + this.resetToken;
    }
  },
  methods: {
    handleRequestReset() {
      (this.$refs.findPwForm as any).validate((valid: boolean) => {
        if (valid) {
          this.loading = true;
          this.resetToken = '';
          requestResetPassword(this.findPwForm)
            .then(res => {
              if (res.code === 20000) {
                this.resetToken = res.data;
                ElMessage.success(this.t('findPw.linkGenerated'));
              } else {
                ElMessage.error(res.message || this.t('findPw.notFound'));
              }
              this.loading = false;
            })
            .catch(() => {
              this.loading = false;
            });
        }
      });
    }
  }
});
</script>

<style lang="scss">
@use "./login-global.scss";
</style>

<style lang="scss" scoped>
@use "./index.scss";
</style>

