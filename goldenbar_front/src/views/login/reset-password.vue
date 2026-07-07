<template>
<div class="login-container">

    <router-link to="/home" class="login-home-link">
      GOLDB<span>v3</span>
    </router-link>

    <div class="login-lang-switch">
      <span :class="{ active: currentLocale === 'ko' }" @click="changeLang('ko')">KO</span>
      <span class="divider">/</span>
      <span :class="{ active: currentLocale === 'en' }" @click="changeLang('en')">EN</span>
    </div>

    <el-form ref="resetForm" :model="resetForm" :rules="resetRules" class="login-form" autocomplete="on"
             label-position="left">

      <div class="title-container">
        <h1 class="title">{{ $t('resetPw.title') }}</h1>
      </div>

      <el-form-item prop="newPassword">
        <span class="svg-container">
          <svg-icon icon-class="password" />
        </span>
        <el-input v-model="resetForm.newPassword" type="password" :placeholder="$t('resetPw.newPassword')" name="newPassword" tabindex="1" />
      </el-form-item>

      <el-form-item prop="confirmPassword">
        <span class="svg-container">
          <svg-icon icon-class="password" />
        </span>
        <el-input v-model="resetForm.confirmPassword" type="password" :placeholder="$t('resetPw.confirmPassword')" name="confirmPassword" tabindex="2" />
      </el-form-item>

      <el-button :loading="loading" type="primary" style="width:100%;margin-bottom: 1.875rem;" @click.prevent="handleReset">
        {{ $t('resetPw.button') }}</el-button>

      <div class="tips">
        <router-link to="/login">{{ $t('resetPw.back') }}</router-link>
      </div>
    </el-form>
  </div>
</template>

<script lang="ts">
import { defineComponent, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { resetPassword } from '@/api/user';
import { ElMessage } from 'element-plus';

export default defineComponent({
  name: 'ResetPassword',
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
      resetForm: {
        token: '',
        newPassword: '',
        confirmPassword: ''
      },
      loading: false
    };
  },
  computed: {
    resetRules(): any {

      const validateConfirmPassword = (rule: any, value: any, callback: any) => {
        if (value !== this.resetForm.newPassword) {
          callback(new Error(this.t('resetPw.mismatch')));
        } else {
          callback();
        }
      };
      return {
        newPassword: [
          { required: true, trigger: 'blur', message: this.t('register.rules.password') },
          { min: 6, message: this.t('register.rules.passwordMin') }
        ],
        confirmPassword: [
          { required: true, trigger: 'blur', message: this.t('resetPw.confirmPassword') },
          { validator: validateConfirmPassword, trigger: 'blur' }
        ]
      };
    }
  },
  created() {
    this.resetForm.token = this.$route.query.token as string || '';
    if (!this.resetForm.token) {
      ElMessage.error(this.t('resetPw.invalidToken'));
      this.$router.push('/login');
    }
  },
  methods: {
    handleReset() {
      (this.$refs.resetForm as any).validate((valid: boolean) => {
        if (valid) {
          this.loading = true;
          resetPassword({
            token: this.resetForm.token,
            newPassword: this.resetForm.newPassword
          })
            .then(res => {
              if (res.code === 20000) {
                ElMessage.success(this.t('resetPw.success'));
                this.$router.push('/login');
              } else {
                ElMessage.error(res.message || this.t('resetPw.error'));
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

