<template>
<div class="login-container">

    <el-form ref="findIdForm" :model="findIdForm" :rules="findIdRules" class="login-form" autocomplete="on"
             label-position="left">

      <div class="title-container">
        <h1 class="title">{{ $t('findId.title') }}</h1>
      </div>

      <el-form-item prop="name">
        <span class="svg-container">
          <svg-icon icon-class="user" />
        </span>
        <el-input v-model="findIdForm.name" :placeholder="$t('findId.name')" name="name" type="text" tabindex="1" />
      </el-form-item>

      <el-form-item prop="email">
        <span class="svg-container">
          <svg-icon icon-class="email" />
        </span>
        <el-input v-model="findIdForm.email" :placeholder="$t('findId.email')" name="email" type="text" tabindex="2" />
      </el-form-item>

      <el-button :loading="loading" type="primary" style="width:100%;margin-bottom: 1.875rem;" @click.prevent="handleFindId">
        {{ $t('findId.button') }}</el-button>

      <div v-if="foundUsername" class="tips" style="text-align: center; font-weight: bold; font-size: 1.125rem; margin-bottom: 1.25rem;">
        {{ $t('findId.result') }} <span style="color: #409EFF;">{{ foundUsername }}</span>
      </div>

      <div class="tips">
        <router-link to="/login">{{ $t('findId.back') }}</router-link>
      </div>
    </el-form>
  </div>
</template>

<script lang="ts">
import { defineComponent, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { findId } from '@/api/user';
import { ElMessage } from 'element-plus';

export default defineComponent({
  name: 'FindId',
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
      findIdForm: {
        name: '',
        email: ''
      },
      loading: false,
      foundUsername: ''
    };
  },
  computed: {
    findIdRules(): any {
      return {
        name: [{ required: true, trigger: 'blur', message: this.t('register.rules.name') }],
        email: [{ required: true, trigger: 'blur', message: this.t('register.rules.email') }, { type: 'email', message: this.t('register.rules.emailInvalid') }]
      };
    }
  },
  methods: {
    handleFindId() {
      (this.$refs.findIdForm as any).validate((valid: boolean) => {
        if (valid) {
          this.loading = true;
          this.foundUsername = '';
          findId(this.findIdForm)
            .then(res => {
              if (res.code === 20000) {
                this.foundUsername = res.data;
              } else {
                ElMessage.error(res.message || this.t('findId.notFound'));
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

