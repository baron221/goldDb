<template>
<div :class="classObj" class="app-wrapper">
    <div v-if="device === 'mobile' && sidebar.opened" class="drawer-bg" @click="handleClickOutside" />
    <sidebar class="sidebar-container" />
    <div :class="{ hasTagsView: needTagsView }" class="main-container">
      <div :class="{ 'fixed-header': fixedHeader }">
        <navbar />
        <tags-view v-if="needTagsView" />
      </div>
      <app-main />

    </div>

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

<script>
import { AppMain, Navbar, Settings, Sidebar, TagsView } from './components';
import NoticePopup from '@/components/NoticePopup/index.vue';
import { ArrowUp } from '@element-plus/icons-vue';
import { getNotices } from '@/api/notice';
import ResizeMixin from './mixin/ResizeHandler';
import { mapState } from 'pinia';
import store from '@/store';
import { defineComponent } from 'vue';

export default defineComponent({
  name: 'LayoutIndex',
  components: {
    AppMain,
    Navbar,
    Sidebar,
    TagsView,
    NoticePopup,
    ArrowUp
  },
  mixins: [ResizeMixin],
  data() {
    return {
      noticeVisible: false,
      notices: [],
      currentNotice: {},
      noticeIndex: 0,
      showBackToTop: false
    };
  },
  computed: {
    ...mapState(store.app, ['sidebar', 'device']),
    ...mapState(store.settings, {
      showSettings: 'showSettings',
      needTagsView: 'tagsView',
      fixedHeader: 'fixedHeader'
    }),
    classObj() {
      return {
        hideSidebar: !this.sidebar.opened,
        openSidebar: this.sidebar.opened,
        withoutAnimation: this.sidebar.withoutAnimation,
        mobile: this.device === 'mobile'
      };
    }
  },
  async mounted() {
    await this.checkNotices();
    window.addEventListener('scroll', this.handleScroll);
  },
  beforeUnmount() {
    window.removeEventListener('scroll', this.handleScroll);
  },
  methods: {

    handleClickOutside() {
      store.app().closeSidebar({ withoutAnimation: false });
    },

    async checkNotices() {
      try {
        const res = await getNotices({ isActive: true });
        if (res.code === 20000 && res.data) {
          const now = new Date().getTime();
          this.notices = res.data.filter(n => {

            const hideUntil = localStorage.getItem(`notice_hide_${n.id}`);
            if (hideUntil && parseInt(hideUntil) > now) {
              return false;
            }

            if (n.startDate && new Date(n.startDate).getTime() > now) return false;
            if (n.endDate && new Date(n.endDate).getTime() < now) return false;

            return true;
          });

          if (this.notices.length > 0) {
            this.showNotice(0);
          }
        }
      } catch (error) {
        console.error('Failed to fetch notices:', error);
      }
    },

    showNotice(index) {
      if (index >= 0 && index < this.notices.length) {
        this.currentNotice = this.notices[index];
        this.noticeIndex = index;
        this.noticeVisible = true;
      }
    },

    showPrevNotice() {
      const prevIndex = this.noticeIndex - 1;
      if (prevIndex >= 0) {
        this.showNotice(prevIndex);
      }
    },

    showNextNotice() {
      const nextIndex = this.noticeIndex + 1;
      if (nextIndex < this.notices.length) {
        this.showNotice(nextIndex);
      }
    },
    handleScroll() {
      const scrollY = window.scrollY;
      this.showBackToTop = scrollY > 300;
    },
    scrollToTop() {
      window.scrollTo({
        top: 0,
        behavior: 'smooth'
      });
    }
  }
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixin.scss";

.app-wrapper {
  @include mixin.clearfix;
  position: relative;
  height: 100%;
  width: 100%;
  font-family: 'S-CoreDream', 'Jost', sans-serif;
  background-color: #fbfaf7;

  &.mobile.openSidebar {
    position: fixed;
    top: 0;
  }
}

.back-to-top {
  position: fixed;
  right: 30px;
  bottom: 40px;
  width: 45px;
  height: 45px;
  background-color: rgba(26, 26, 26, 0.9);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  color: #c5a880;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 2000;
  border: 1px solid rgba(197, 168, 128, 0.3);
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);

  &:hover {
    background-color: #c5a880;
    color: #fff;
    transform: translateY(-5px);
    border-color: #c5a880;
  }

  @media (max-width: 768px) {
    right: 20px;
    bottom: 30px;
    width: 40px;
    height: 40px;
  }
}

.fade-enter-active, .fade-leave-active {
  transition: opacity 0.5s, transform 0.5s;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
  transform: translateY(20px);
}

.main-container {
  min-height: 100%;
  transition: margin-left .28s;
  margin-left: var(--side-bar-width);
  position: relative;
  background-color: #fbfaf7;
}

.drawer-bg {
  background: rgba(34, 34, 34, 0.4);
  backdrop-filter: blur(4px);
  width: 100%;
  top: 0;
  height: 100%;
  position: absolute;
  z-index: 999;
}

.fixed-header {
  position: fixed;
  top: 0;
  right: 0;
  z-index: 9;
  width: calc(100% - var(--side-bar-width));
  transition: width 0.28s;
}

.hideSidebar .fixed-header {
  width: calc(100% - 54px)
}

.mobile .fixed-header {
  width: 100%;
}
</style>

