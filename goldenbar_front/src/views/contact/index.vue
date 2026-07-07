<template>
  <div class="contact-page-luxury">

    <header class="luxury-hero-section">
      <div class="hero-content-wrapper">
        <h1 class="hero-title">{{ $t('contact.title') }}</h1>
        <p class="hero-subtitle">{{ $t('contact.subtitle') }}</p>
        <div class="hero-divider"></div>
      </div>
    </header>

    <div class="luxury-contact-content">
      <el-row :gutter="80">

        <el-col :xs="24" :lg="9">
          <div class="info-sidebar-luxury">
            <div class="info-block-luxury">
              <div class="section-label">{{ $t('contact.officeLabel') }}</div>
              <h3 class="info-title">{{ $t('contact.officeTitle') }}</h3>
              <p class="info-text">{{ $t('about.addressValue') }}</p>
              <p class="info-subtext">{{ $t('contact.officeLocation') }}</p>
            </div>

            <div class="info-block-luxury">
              <div class="section-label">{{ $t('contact.channelsLabel') }}</div>
              <h3 class="info-title">{{ $t('contact.contactInfoTitle') }}</h3>
              <div class="contact-methods">
                <div class="method-item">
                  <span class="label">TEL</span>
                  <span class="value">010-3838-8383</span>
                </div>
                <div class="method-item">
                  <span class="label">FAX</span>
                  <span class="value">051-636-7474</span>
                </div>
                <div class="method-item">
                  <span class="label">EMAIL</span>
                  <span class="value"><a href="mailto:dude8@hanmail.net">dude8@hanmail.net</a></span>
                </div>
              </div>
            </div>
          </div>
        </el-col>

        <el-col :xs="24" :lg="15">
          <div class="form-container-luxury">
            <div class="section-label">{{ $t('contact.inquiryFormLabel') }}</div>
            <h2 class="form-title-main">{{ $t('contact.formTitle') }}</h2>

            <el-form ref="contactForm" :model="form" :rules="rules" label-position="top" class="luxury-contact-form">
              <el-row :gutter="30">
                <el-col :xs="24" :sm="12">
                  <el-form-item :label="$t('contact.form.name')" prop="name">
                    <el-input v-model="form.name" :placeholder="$t('contact.form.placeholders.name')" class="luxury-input" />
                  </el-form-item>
                </el-col>
                <el-col :xs="24" :sm="12">
                  <el-form-item :label="$t('contact.form.email')" prop="email">
                    <el-input v-model="form.email" :placeholder="$t('contact.form.placeholders.email')" class="luxury-input" />
                  </el-form-item>
                </el-col>
              </el-row>

              <el-row :gutter="30">
                <el-col :xs="24" :sm="12">
                  <el-form-item :label="$t('contact.form.phone')" prop="phone">
                    <el-input v-model="form.phone" :placeholder="$t('contact.form.placeholders.phone')" class="luxury-input" />
                  </el-form-item>
                </el-col>
                <el-col :xs="24" :sm="12">
                  <el-form-item :label="$t('contact.form.subject')" prop="subject">
                    <el-select v-model="form.subject" :placeholder="$t('contact.form.placeholders.subject')" class="luxury-select" style="width: 100%;">
                      <el-option :label="$t('contact.form.subjects.general')" value="General Inquiry" />
                      <el-option :label="$t('contact.form.subjects.quotation')" value="Quotation" />
                      <el-option :label="$t('contact.form.subjects.partnership')" value="Partnership" />
                      <el-option :label="$t('contact.form.subjects.support')" value="Technical Support" />
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>

              <el-form-item :label="$t('contact.form.message')" prop="message">
                <el-input v-model="form.message" type="textarea" :rows="6" :placeholder="$t('contact.form.placeholders.message')" class="luxury-textarea" />
              </el-form-item>

              <div class="form-action-area">
                <el-button type="primary" :loading="loading" class="luxury-submit-btn" @click="handleSubmit">
                  {{ $t('contact.form.send') }} <i class="fas fa-paper-plane"></i>
                </el-button>
              </div>
            </el-form>
          </div>
        </el-col>
      </el-row>
    </div>

    <section class="luxury-map-section">
      <div class="map-wrapper">
        <div class="map-pattern-overlay"></div>
        <img src="/image/common_map.png" alt="Office Map" class="map-visual" />
        <div class="map-floating-card">
          <div class="section-label">{{ $t('contact.mapLocationLabel') }}</div>
          <h4 class="location-name">{{ $t('contact.mapLocationTitle') }}</h4>
          <p class="location-desc">{{ $t('contact.mapAddress') }}</p>
          <a href="https://naver.me/FfsAAOb3" target="_blank" class="luxury-outline-btn small">
            {{ $t('contact.viewLargerMap') }} <i class="fas fa-external-link-alt"></i>
          </a>
        </div>
      </div>
    </section>
  </div>
</template>

<script lang="ts">
import { defineComponent, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import { useI18n } from 'vue-i18n';
import request from '@/utils/request';

export default defineComponent({
  name: 'Contact',
  setup() {
    const { t } = useI18n();
    const router = useRouter();
    const contactForm = ref();
    const loading = ref(false);

    const form = reactive({
      name: '',
      email: '',
      phone: '',
      subject: '',
      message: ''
    });

    const rules = {
      name: [{ required: true, message: t('contact.form.rules.name'), trigger: 'blur' }],
      email: [
        { required: true, message: t('contact.form.rules.email'), trigger: 'blur' },
        { type: 'email', message: t('contact.form.rules.emailInvalid'), trigger: 'blur' }
      ],
      subject: [{ required: true, message: t('contact.form.rules.subject'), trigger: 'change' }],
      message: [{ required: true, message: t('contact.form.rules.message'), trigger: 'blur' }]
    };

    const handleSubmit = () => {
      contactForm.value.validate((valid: boolean) => {
        if (valid) {
          loading.value = true;
          request({
            url: '/contact',
            method: 'post',
            data: form
          }).then(() => {
            ElMessage.success(t('contact.successMessage'));

            setTimeout(() => {
              router.push('/');
            }, 2000);
          }).finally(() => {
            loading.value = false;
          });
        }
      });
    };

    return {
      form,
      rules,
      loading,
      contactForm,
      handleSubmit
    };
  }
});
</script>

<style lang="scss" src="./ContactStyles.scss" scoped></style>
