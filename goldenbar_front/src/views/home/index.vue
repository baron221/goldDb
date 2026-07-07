<template>
<div class="home-page-luxury">

    <section class="hero-section">
      <div class="hero-background-pattern"></div>
      <div class="hero-content">
        <h1 class="hero-title">{{ $t('home.heroTitle') }}</h1>
        <p class="hero-subtitle">{{ $t('home.heroSubtitle') }}</p>
        <p class="hero-description">
          {{ $t('home.heroDesc') }}
        </p>
        <div class="hero-action-btns">
          <el-button type="primary" class="luxury-btn primary" @click="$router.push('/login')">
            {{ $t('home.getStarted') }} <i class="fas fa-chevron-right"></i>
          </el-button>
          <el-button class="luxury-btn outline" @click="$router.push('/contact')">
            {{ $t('home.contactUs') }}
          </el-button>
        </div>
      </div>
      <div class="hero-scroll-indicator">
        <div class="mouse"></div>
      </div>
    </section>

    <section class="roles-section-luxury">
      <div class="section-header-luxury">
        <div class="section-label">PLATFORM ECOSYSTEM</div>
        <h2 class="section-title-main">{{ $t('home.rolesTitle') }}</h2>
        <div class="gold-accent-line"></div>
      </div>

      <div class="roles-grid-wrapper">
        <el-row :gutter="40">
          <el-col :xs="24" :sm="12" :md="6" v-for="role in roles" :key="role.titleKey">
            <div class="role-card-luxury">
              <div class="role-icon-box">
                <i :class="role.faIcon"></i>
              </div>
              <h3 class="role-name">{{ $t(role.titleKey) }}</h3>
              <p class="role-desc">{{ $t(role.descKey) }}</p>
              <div class="card-footer-line"></div>
            </div>
          </el-col>
        </el-row>
      </div>
    </section>

    <section class="ai-feature-section">
      <div class="feature-content-container">
        <el-row align="middle" :gutter="80">
          <el-col :xs="24" :lg="12">
            <div class="ai-visual-wrapper">
              <div class="visual-box">
                <div class="scan-line"></div>
                <span class="visual-label">AI IMAGE ENGINE v3</span>
                <div class="visual-placeholder">
                  <i class="fas fa-microchip"></i>
                </div>
              </div>
              <div class="visual-deco-dots"></div>
            </div>
          </el-col>
          <el-col :xs="24" :lg="12">
            <div class="feature-text-luxury">
              <div class="section-label">ADVANCED TECHNOLOGY</div>
              <h2 class="feature-title">{{ $t('home.aiSearchTitle') }}</h2>
              <p class="feature-desc">
                {{ $t('home.aiSearchDesc') }}
              </p>
              <el-button link class="luxury-link-btn" @click="$router.push('/marketplace')">
                {{ $t('home.learnMore') }} <i class="fas fa-arrow-right"></i>
              </el-button>
            </div>
          </el-col>
        </el-row>
      </div>
    </section>

    <section class="stats-section-luxury">
      <div class="stats-background"></div>
      <div class="stats-content-wrapper">
        <el-row :gutter="20">
          <el-col :span="8" :xs="24" v-for="stat in stats" :key="stat.labelKey">
            <div class="stat-item-luxury">
              <div class="stat-val">{{ stat.value }}</div>
              <div class="stat-label-luxury">{{ $t(stat.labelKey) }}</div>
            </div>
          </el-col>
        </el-row>
      </div>
    </section>

    <section class="home-bottom-cta">
      <div class="cta-card">
        <h2 class="cta-title">Ready to Transform Your Business?</h2>
        <p class="cta-subtitle">Join the premier B2B jewelry network today.</p>
        <el-button class="luxury-btn primary gold" @click="$router.push('/login')">JOIN THE NETWORK</el-button>
      </div>
    </section>
  </div>
</template>

<script lang="ts">

import { defineComponent } from 'vue';

export default defineComponent({
  name: 'Home',
  setup() {
    const roles = [
      { titleKey: 'home.roles.admin.title', faIcon: 'fas fa-shield-alt', descKey: 'home.roles.admin.desc' },
      { titleKey: 'home.roles.logistics.title', faIcon: 'fas fa-truck-loading', descKey: 'home.roles.logistics.desc' },
      { titleKey: 'home.roles.factory.title', faIcon: 'fas fa-industry', descKey: 'home.roles.factory.desc' },
      { titleKey: 'home.roles.store.title', faIcon: 'fas fa-store-alt', descKey: 'home.roles.store.desc' }
    ];

    const stats = [
      { labelKey: 'home.stats.partners', value: '500+' },
      { labelKey: 'home.stats.orders', value: '1,200+' },
      { labelKey: 'home.stats.products', value: '50,000+' }
    ];

    return {
      roles,
      stats
    };
  }
});
</script>

<style lang="scss" src="./HomeStyles.scss" scoped></style>

