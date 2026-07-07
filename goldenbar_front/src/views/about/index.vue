<template>
<div class="about-page-luxury">

    <header class="luxury-hero-section">
      <div class="hero-content-wrapper">
        <h1 class="hero-title">{{ $t('about.heroTitle') }}</h1>
        <p class="hero-subtitle">{{ $t('about.heroSubtitle') }}</p>
        <div class="hero-divider"></div>
      </div>
    </header>

    <div class="luxury-about-content">

      <section class="content-section">
        <div class="section-label">OUR MISSION</div>
        <div class="mission-text-block">
          <p class="large-body-text">
            {{ $t('about.missionDesc') }}
          </p>
        </div>
      </section>

      <section class="vision-display-section">
        <div class="vision-box">
          <div class="vision-overlay">
            <h2 class="vision-title">{{ $t('about.visionText') }}</h2>
            <div class="gold-accent-line"></div>
          </div>
          <div class="vision-bg-pattern"></div>
        </div>
      </section>

      <section class="values-grid-section">
        <div class="section-label">CORE VALUES</div>
        <el-row :gutter="60" class="values-row">
          <el-col :xs="24" :md="12" class="value-col">
            <div class="luxury-value-item">
              <div class="item-header">
                <i class="fas fa-eye item-icon"></i>
                <h3 class="item-title">{{ $t('about.values.transparencyTitle') }}</h3>
              </div>
              <p class="item-desc">{{ $t('about.values.transparencyDesc') }}</p>
            </div>
          </el-col>
          <el-col :xs="24" :md="12" class="value-col">
            <div class="luxury-value-item">
              <div class="item-header">
                <i class="fas fa-lightbulb item-icon"></i>
                <h3 class="item-title">{{ $t('about.values.innovationTitle') }}</h3>
              </div>
              <p class="item-desc">{{ $t('about.values.innovationDesc') }}</p>
            </div>
          </el-col>
        </el-row>
      </section>

      <section class="team-intro-section">
        <div class="section-label">THE TEAM</div>
        <div class="team-content">
          <h2 class="team-title-main">{{ $t('about.teamTitle') }}</h2>
          <p class="team-desc-text">{{ $t('about.teamDesc') }}</p>
        </div>
      </section>

      <section class="office-directions-section">
        <el-row :gutter="40">
          <el-col :lg="12" :md="24">
            <div class="luxury-info-card office-card">
              <div class="section-label">HEAD OFFICE</div>
              <div class="info-list">
                <div class="info-item">
                  <span class="label">{{ $t('about.addressLabel') }}</span>
                  <span class="value">{{ $t('about.addressValue') }}</span>
                </div>
                <div class="info-item">
                  <span class="label">TEL</span>
                  <span class="value">010-3838-8383</span>
                </div>
                <div class="info-item">
                  <span class="label">FAX</span>
                  <span class="value">051-636-7474</span>
                </div>
                <div class="info-item">
                  <span class="label">EMAIL</span>
                  <span class="value">dude8@hanmail.net</span>
                </div>
              </div>
            </div>
          </el-col>
          <el-col :lg="12" :md="24">
            <div class="luxury-info-card directions-card">
              <div class="section-label">DIRECTIONS</div>
              <div class="directions-list">
                <div class="dir-block">
                  <h4 class="dir-title"><i class="fas fa-subway"></i> {{ $t('about.directions.subway.title') }}</h4>
                  <p class="dir-text">{{ $t('about.directions.subway.desc') }}</p>
                </div>
                <div class="dir-block">
                  <h4 class="dir-title"><i class="fas fa-bus"></i> {{ $t('about.directions.bus.title') }}</h4>
                  <p class="dir-text">{{ $t('about.directions.bus.desc') }}</p>
                </div>
                <div class="dir-block">
                  <h4 class="dir-title"><i class="fas fa-shuttle-van"></i> {{ $t('about.directions.villageBus.title') }}</h4>
                  <p class="dir-text">{{ $t('about.directions.villageBus.desc') }}</p>
                </div>
              </div>
            </div>
          </el-col>
        </el-row>
      </section>

      <section class="guide-cta-section">
        <div class="cta-inner">
          <h2 class="cta-title">{{ $t('about.siteGuideTitle') }}</h2>
          <p class="cta-desc">{{ $t('about.siteGuideDesc') }}</p>
          <el-button class="luxury-outline-btn" @click="handleViewGuide">
            {{ $t('about.viewGuide') }} <i class="fas fa-arrow-right"></i>
          </el-button>
        </div>
      </section>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';

export default defineComponent({
  name: 'About',
  methods: {

    handleViewGuide() {

    }
  }
});
</script>

<style lang="scss" scoped>
@import url('https://fonts.googleapis.com/css2?family=Jost:wght@300;400;500;600;700&display=swap');

.about-page-luxury {
  font-family: 'S-CoreDream', 'Jost', sans-serif;
  background-color: #ffffff;

  min-height: 100vh;
  overflow-x: hidden;
}

.luxury-hero-section {
  background-color: #fbfaf7;
  padding: 12.5rem 1.25rem 7.5rem;
  text-align: center;
  border-bottom: 1px solid #eae6df;

  .hero-content-wrapper {
    max-width: 900px;
    margin: 0 auto;
  }

  .hero-title {
    font-size: 4rem;
    font-weight: 700;

    text-transform: uppercase;
    letter-spacing: 6px;
    margin-bottom: 1.5625rem;
    line-height: 1.1;
  }

  .hero-subtitle {
    font-size: 1.125rem;
    color: #888888;
    font-weight: 300;
    text-transform: uppercase;
    letter-spacing: 3px;
    margin-bottom: 2.5rem;
  }

  .hero-divider {
    width: 80px;
    height: 2px;
    background-color: #c5a880;
    margin: 0 auto;
  }
}

.luxury-about-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 6.25rem 2.5rem 9.375rem;
}

.section-label {
  font-size: 0.8875rem;
  font-weight: 800;
  color: #c5a880;
  letter-spacing: 3px;
  text-transform: uppercase;
  margin-bottom: 1.875rem;
}

.content-section {
  margin-bottom: 7.5rem;
  .large-body-text {
    font-size: 2rem;
    line-height: 1.5;
    font-weight: 400;

    max-width: 1000px;
  }
}

.vision-display-section {
  margin-bottom: 7.5rem;
  .vision-box {
    position: relative;
    height: 450px;
    background-color: #222222;
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;
    border-radius: 2px;

    .vision-overlay {
      position: relative;
      z-index: 10;
      text-align: center;
      .vision-title {
        color: #ffffff;
        font-size: 2.25rem;
        font-weight: 700;
        letter-spacing: 2px;
        text-transform: uppercase;
        margin-bottom: 1.25rem;
      }
      .gold-accent-line {
        width: 100px;
        height: 3px;
        background-color: #c5a880;
        margin: 0 auto;
      }
    }

    .vision-bg-pattern {
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background-image: radial-gradient(circle at 2px 2px, rgba(197, 168, 128, 0.1) 1px, transparent 0);
      background-size: 40px 40px;
      opacity: 0.3;
    }
  }
}

.values-grid-section {
  margin-bottom: 7.5rem;
  .luxury-value-item {
    .item-header {
      display: flex;
      align-items: center;
      gap: 0.9375rem;
      margin-bottom: 1.25rem;
      .item-icon { font-size: 1.5rem; color: #c5a880; }
      .item-title { font-size: 1.125rem; font-weight: 700; margin: 0; letter-spacing: 1px; }
    }
    .item-desc {
      font-size: 1rem;
      line-height: 1.7;
      color: #666666;
    }
  }
}

.team-intro-section {
  margin-bottom: 7.5rem;
  .team-content {
    .team-title-main { font-size: 2.625rem; font-weight: 700; margin-bottom: 1.5625rem; }
    .team-desc-text { font-size: 1.25rem; line-height: 1.6; color: #666666; font-weight: 400; }
  }
}

.luxury-info-card {
  background-color: #fbfaf7;
  border: 1px solid #eae6df;
  border-radius: 2px;
  padding: 2.5rem;
  height: 100%;
  margin-bottom: 2.5rem;

  .info-list {
    display: flex;
    flex-direction: column;
    gap: 1.25rem;
    .info-item {
      display: flex;
      flex-direction: column;
      gap: 0.3125rem;
      .label { font-size: 0.825rem; font-weight: 800; color: #bbbbbb; letter-spacing: 1px; }
      .value { font-size: 1rem; font-weight: 600; }
    }
  }

  .directions-list {
    display: flex;
    flex-direction: column;
    gap: 1.875rem;
    .dir-block {
      .dir-title { font-size: 0.875rem; font-weight: 700; margin-bottom: 0.625rem; display: flex; align-items: center; gap: 0.625rem;
        i { color: #c5a880; font-size: 1rem; }
      }
      .dir-text { font-size: 0.875rem; color: #666666; line-height: 1.6; }
    }
  }
}

.guide-cta-section {
  background-color: #222222;
  color: #ffffff;
  padding: 6.25rem 2.5rem;
  text-align: center;
  border-radius: 2px;

  .cta-title { font-size: 2.25rem; font-weight: 700; margin-bottom: 1.25rem; letter-spacing: 2px; text-transform: uppercase; }
  .cta-desc { font-size: 1.125rem; color: #bbbbbb; margin-bottom: 2.5rem; font-weight: 300; max-width: 700px; margin-left: auto; margin-right: auto; }

  .luxury-outline-btn {
    background-color: transparent;
    border: 2px solid #c5a880;
    color: #c5a880;
    padding: 0.9375rem 2.5rem;
    font-weight: 700;
    letter-spacing: 2px;
    text-transform: uppercase;
    font-size: 0.9rem;
    transition: all 0.3s;
    &:hover { background-color: #c5a880; color: #ffffff; }
    i { margin-left: 0.625rem; }
  }
}

@media (max-width: 991px) {
  .luxury-hero-section {
    padding: 8.75rem 1.25rem 5rem;
    .hero-title { font-size: 2.25rem; word-break: keep-all; }
    .hero-subtitle { font-size: 1rem; }
  }
  .luxury-about-content { padding: 3.75rem 1.25rem 6.25rem; }
  .content-section .large-body-text { font-size: 1.25rem; }
  .vision-display-section .vision-box { height: 300px; .vision-overlay .vision-title { font-size: 1.25rem; } }
  .team-intro-section .team-title-main { font-size: 1.75rem; }
  .office-directions-section .luxury-info-card { margin-bottom: 2rem; }
}

@media (max-width: 480px) {
  .luxury-hero-section .hero-title { font-size: 1.75rem; }
}
</style>

