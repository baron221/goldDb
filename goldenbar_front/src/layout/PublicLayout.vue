<template>
<div class="public-layout">

    <header class="public-header" :class="{ 'is-scrolled': isScrolled }">
      <div class="header-content">
        <div class="logo" @click="$router.push('/home')">
          <img src="/ci.png" alt="GOLDB Logo" />
        </div>
        <nav class="nav-links">
          <router-link to="/home" class="nav-item">{{ $t('nav.home') }}</router-link>
          <router-link to="/about" class="nav-item">{{ $t('nav.about') }}</router-link>
          <router-link to="/marketplace" class="nav-item">{{ $t('nav.marketplace') }}</router-link>
          <router-link to="/contact" class="nav-item">{{ $t('nav.contact') }}</router-link>

          <router-link v-if="!isLoggedIn" to="/login" class="nav-item login-btn">{{ $t('nav.login') }}</router-link>
          <div v-else class="auth-btns">
            <router-link to="/dashboard" class="nav-item login-btn">{{ $t('nav.getStarted') }}</router-link>
            <el-tooltip :content="$t('nav.logout')" placement="bottom" effect="light">
              <span class="nav-item logout-icon" @click="handleLogout">
                <el-icon><SwitchButton /></el-icon>
              </span>
            </el-tooltip>
          </div>

          <div class="lang-switch nav-item" @click="toggleLang">
            <span>{{ currentLocale === 'ko' ? 'KO' : 'EN' }}</span>
          </div>
        </nav>

        <div class="mobile-toggle" @click="toggleMobileMenu">
          <el-icon><IconMenu /></el-icon>
        </div>
      </div>
    </header>

    <el-drawer
      v-model="mobileMenuVisible"
      direction="rtl"
      size="80%"
      :with-header="false"
      custom-class="mobile-drawer"
    >
      <div class="mobile-drawer-content">
        <div class="drawer-header">
          <img src="/ci.png" alt="GOLDB Logo" class="drawer-logo" @click="$router.push('/home'); mobileMenuVisible = false" />
          <el-icon class="close-icon" @click="mobileMenuVisible = false"><Close /></el-icon>
        </div>

        <nav class="drawer-nav">
          <router-link to="/home" class="drawer-item" @click="mobileMenuVisible = false">{{ $t('nav.home') }}</router-link>
          <router-link to="/about" class="drawer-item" @click="mobileMenuVisible = false">{{ $t('nav.about') }}</router-link>
          <router-link to="/marketplace" class="drawer-item" @click="mobileMenuVisible = false">{{ $t('nav.marketplace') }}</router-link>
          <router-link to="/contact" class="drawer-item" @click="mobileMenuVisible = false">{{ $t('nav.contact') }}</router-link>

          <div class="drawer-divider"></div>

          <router-link v-if="!isLoggedIn" to="/login" class="drawer-item login-link" @click="mobileMenuVisible = false">{{ $t('nav.login') }}</router-link>
          <template v-else>
            <router-link to="/dashboard" class="drawer-item" @click="mobileMenuVisible = false">{{ $t('nav.getStarted') }}</router-link>
            <div class="drawer-item logout-link" @click="handleLogout(); mobileMenuVisible = false">
              {{ $t('nav.logout') }}
            </div>
          </template>

          <div class="drawer-item lang-link" @click="toggleLang">
            {{ currentLocale === 'ko' ? 'Switch to English' : '한국어로 변경' }}
          </div>
        </nav>
      </div>
    </el-drawer>

    <main class="public-main">
      <router-view />
    </main>

    <footer class="public-footer">
      <div class="footer-content">
        <div class="footer-top">
          <div class="footer-logo">
            <img src="/ci.png" alt="GOLDB Logo" />
          </div>
          <div class="footer-links">
            <div class="link-group">
              <h4>{{ $t('footer.platform') }}</h4>
              <router-link to="/home">{{ $t('nav.home') }}</router-link>
              <router-link to="/about">{{ $t('nav.about') }}</router-link>
              <router-link to="/marketplace">{{ $t('nav.marketplace') }}</router-link>
            </div>
            <div class="link-group">
              <h4>{{ $t('footer.support') }}</h4>
              <router-link to="/contact">{{ $t('nav.contact') }}</router-link>
              <router-link to="/terms">{{ $t('footer.terms') }}</router-link>
              <router-link to="/privacy">{{ $t('footer.privacy') }}</router-link>
            </div>
          </div>
        </div>
        <div class="footer-bottom">
          <p>&copy; 2026 GOLDB v3. {{ $t('footer.rights') }}</p>
          <div class="social-icons">

          </div>
        </div>
      </div>
    </footer>

    <notice-popup
      v-model="noticeVisible"
      :notice="currentNotice"
      :current-index="noticeIndex"
      :total-count="notices.length"
      @prev="showPrevNotice"
      @next="showNextNotice"
    />

    <transition name="fade">
      <div v-if="showBackToTop" class="back-to-top" @click="scrollToTop">
        <el-icon><ArrowUp /></el-icon>
      </div>
    </transition>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, onMounted, onUnmounted, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import userStore from '@/store/modules/user';
import cartStore from '@/store/modules/cart';
import NoticePopup from '@/components/NoticePopup/index.vue';

import { getNotices } from '@/api/notice';
import { SwitchButton, Menu as IconMenu, Close, ArrowUp } from '@element-plus/icons-vue';
import { ElMessage, ElMessageBox } from 'element-plus';

export default defineComponent({
  name: 'PublicLayout',
  components: {
    NoticePopup,
    SwitchButton,
    IconMenu,
    Close,
    ArrowUp
  },
  setup() {
    const { locale } = useI18n();
    const store = userStore();
    const cart = cartStore();
    const isScrolled = ref(false);
    const showBackToTop = ref(false);

    const noticeVisible = ref(false);
    const notices = ref<any[]>([]);
    const currentNotice = ref<any>({});
    const noticeIndex = ref(0);

    const currentLocale = computed(() => locale.value);
    const isLoggedIn = computed(() => !!store.token);
    const cartCount = computed(() => cart.cartCount);

    const mobileMenuVisible = ref(false);
    const toggleMobileMenu = () => {
      mobileMenuVisible.value = !mobileMenuVisible.value;
    };

    const checkNotices = async () => {
      try {

        const params: any = { isActive: true };
        if (!store.token) {
          params.isLoginRequired = false;
        }

        const res = await getNotices(params);
        if (res.code === 20000 && res.data) {
          const now = new Date().getTime();
          notices.value = res.data.filter((n: any) => {

            const hideUntil = localStorage.getItem(`notice_hide_${n.id}`);
            if (hideUntil && parseInt(hideUntil) > now) {
              return false;
            }

            if (n.startDate && new Date(n.startDate).getTime() > now) return false;
            if (n.endDate && new Date(n.endDate).getTime() < now) return false;

            return true;
          });

          if (notices.value.length > 0) {
            showNotice(0);
          }
        }
      } catch (error) {
        console.error('Failed to fetch notices:', error);
      }
    };

    const showNotice = (index: number) => {
      if (index >= 0 && index < notices.value.length) {
        currentNotice.value = notices.value[index];
        noticeIndex.value = index;
        noticeVisible.value = true;
      }
    };

    const showPrevNotice = () => {
      const prevIndex = noticeIndex.value - 1;
      if (prevIndex >= 0) {
        showNotice(prevIndex);
      }
    };

    const showNextNotice = () => {
      const nextIndex = noticeIndex.value + 1;
      if (nextIndex < notices.value.length) {
        showNotice(nextIndex);
      }
    };

    const toggleLang = () => {
      const nextLang = locale.value === 'ko' ? 'en' : 'ko';
      locale.value = nextLang;
      localStorage.setItem('lang', nextLang);
      mobileMenuVisible.value = false;
    };

    const handleLogout = async () => {
      ElMessageBox.confirm('로그아웃 하시겠습니까?', '로그아웃 확인', {
        confirmButtonText: '확인',
        cancelButtonText: '취소',
        type: 'warning'
      }).then(async () => {
        await store.logout();
        location.reload();
        ElMessage.success('로그아웃 되었습니다.');
      }).catch(() => {

      });
    };

    const handleScroll = () => {
      const scrollY = window.scrollY;
      isScrolled.value = scrollY > 50;
      showBackToTop.value = scrollY > 300;
    };

    const scrollToTop = () => {
      window.scrollTo({
        top: 0,
        behavior: 'smooth'
      });
    };

    onMounted(() => {
      window.addEventListener('scroll', handleScroll);
      checkNotices();
      if (store.token) {
        cart.fetchCart();
      }
    });

    onUnmounted(() => {
      window.removeEventListener('scroll', handleScroll);
    });

    return {
      isScrolled,
      currentLocale,
      isLoggedIn,
      cartCount,
      noticeVisible,
      notices,
      currentNotice,
      noticeIndex,
      mobileMenuVisible,
      toggleMobileMenu,
      showBackToTop,
      scrollToTop,
      toggleLang,
      handleLogout,
      showPrevNotice,
      showNextNotice
    };
  }
});
</script>

<style lang="scss" scoped>
@import "./PublicLayoutStyles.scss";
</style>

