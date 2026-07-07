<template>
<div class="login-container">

    <el-form ref="registerForm" :model="registerForm" :rules="registerRules" class="login-form" autocomplete="off"
             label-position="left">

      <div class="title-container">
        <h1 class="title">{{ $t('register.title') }}</h1>
      </div>

      <el-form-item prop="username">
        <span class="svg-container">
          <svg-icon icon-class="user" />
        </span>
        <el-input v-model="registerForm.username" :placeholder="$t('register.username')" name="username" type="text" tabindex="1" autocomplete="off" />
      </el-form-item>

      <el-form-item prop="password">
        <span class="svg-container">
          <svg-icon icon-class="password" />
        </span>
        <el-input v-model="registerForm.password" type="password" :placeholder="$t('register.password')" name="password" tabindex="2" autocomplete="new-password" />
      </el-form-item>

      <el-form-item prop="confirmPassword">
        <span class="svg-container">
          <svg-icon icon-class="password" />
        </span>
        <el-input v-model="registerForm.confirmPassword" type="password" :placeholder="$t('register.confirmPassword')" name="confirmPassword" tabindex="3" autocomplete="new-password" />
      </el-form-item>

      <el-form-item prop="name">
        <span class="svg-container">
          <svg-icon icon-class="user" />
        </span>
        <el-input v-model="registerForm.name" :placeholder="$t('register.name')" name="name" type="text" tabindex="4" />
      </el-form-item>

      <el-form-item prop="email">
        <span class="svg-container">
          <svg-icon icon-class="email" />
        </span>
        <el-input v-model="registerForm.email" :placeholder="$t('register.email')" name="email" type="text" tabindex="5" />
      </el-form-item>

      <el-form-item prop="phone">
        <span class="svg-container">
          <svg-icon icon-class="tab" />
        </span>
        <el-input v-model="registerForm.phone" :placeholder="$t('register.phone')" name="phone" type="text" tabindex="6" />
      </el-form-item>

      <el-form-item prop="introduction">
        <el-input
          v-model="registerForm.introduction"
          type="textarea"
          :autosize="{ minRows: 2, maxRows: 4}"
          :placeholder="$t('register.introduction')"
          name="introduction"
          tabindex="7"
        />
      </el-form-item>

      <el-button :loading="loading" type="primary" style="width:100%;margin-bottom: 1.875rem;" @click.prevent="handleRegister">
        {{ $t('register.button') }}</el-button>

      <div class="tips">
        <router-link to="/login">{{ $t('register.back') }}</router-link>
      </div>
    </el-form>
  </div>
</template>

<script lang="ts">
import { defineComponent, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { register } from '@/api/user';
import { ElMessage } from 'element-plus';

export default defineComponent({
  name: 'Register',
  setup() {
    const { locale, t } = useI18n();
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
      registerForm: {
        username: '',
        password: '',
        confirmPassword: '',
        name: '',
        email: '',
        phone: '',
        introduction: '',
        userType: 'COMPANY'
      },
      loading: false
    };
  },
  computed: {
    registerRules(): any {

      const validateConfirmPassword = (rule: any, value: any, callback: any) => {
        if (value !== this.registerForm.password) {
          callback(new Error(this.t('register.rules.passwordMismatch')));
        } else {
          callback();
        }
      };
      const validateNoSpace = (rule: any, value: any, callback: any) => {
        if (value && /\s/.test(value)) {
          callback(new Error(this.t('register.rules.noSpace')));
        } else {
          callback();
        }
      };
      return {
        username: [
          { required: true, trigger: 'blur', message: this.t('register.rules.username') },
          { validator: validateNoSpace, trigger: 'blur' }
        ],
        password: [
          { required: true, trigger: 'blur', message: this.t('register.rules.password') },
          { min: 6, message: this.t('register.rules.passwordMin') },
          { validator: validateNoSpace, trigger: 'blur' }
        ],
        confirmPassword: [
          { required: true, trigger: 'blur', message: this.t('register.rules.confirmPassword') },
          { validator: validateConfirmPassword, trigger: 'blur' }
        ],
        name: [{ required: true, trigger: 'blur', message: this.t('register.rules.name') }],
        email: [
          { required: true, trigger: 'blur', message: this.t('register.rules.email') },
          { type: 'email', message: this.t('register.rules.emailInvalid') },
          { validator: validateNoSpace, trigger: 'blur' }
        ],
        phone: [{ required: true, trigger: 'blur', message: this.t('register.rules.phone') }]
      };
    }
  },
  methods: {
    handleRegister() {
      (this.$refs.registerForm as any).validate((valid: boolean) => {
        if (valid) {
          this.loading = true;
          const { confirmPassword, ...submitData } = this.registerForm;
          register(submitData)
            .then(() => {
              ElMessage.success(this.t('register.success'));
              this.$router.push('/login');
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

