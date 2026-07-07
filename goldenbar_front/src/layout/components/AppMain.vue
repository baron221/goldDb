<template>
<section class="app-main">
    <router-view v-slot="{ Component, route }">
      <transition name="fade-quick">
        <keep-alive :include="cachedViews" :max="20">
          <component :is="wrap(route, Component)" :key="route.fullPath" />
        </keep-alive>
      </transition>
    </router-view>
  </section>
</template>

<script>
import { defineComponent, h } from 'vue';
import store from '@/store';

const wrapperMap = new Map();

export default defineComponent({
  name: 'AppMain',
  computed: {
    cachedViews() {
      return store.tagsView().cachedViews;
    }
  },
  methods: {

    wrap(route, Component) {
      if (!Component) return null;

      const menuId = route.meta?.id || '';
      const wrapperName = menuId ? `${menuId}-${route.fullPath}` : route.fullPath;

      if (wrapperMap.has(wrapperName)) {
        return wrapperMap.get(wrapperName);
      }

      const wrapper = {
        name: wrapperName,
        render() {
          return h(Component);
        }
      };
      wrapperMap.set(wrapperName, wrapper);
      return wrapper;
    }
  }
});
</script>

<style lang="scss" scoped>
.app-main {

  min-height: calc(100vh - 50px);
  width: 100%;
  position: relative;
  overflow: hidden;
}

.fixed-header+.app-main {
  padding-top: 3.125rem;
}

.hasTagsView {
  .app-main {

    min-height: calc(100vh - 86px);
  }

  .fixed-header+.app-main {
    padding-top: 8.5rem;
  }
}

.fade-quick-enter-active,
.fade-quick-leave-active {
  transition: opacity 0.2s ease;
}

.fade-quick-enter-from,
.fade-quick-leave-to {
  opacity: 0;
}
</style>

<style lang="scss">
// fix css style bug in open el-dialog
.el-popup-parent--hidden {
  .fixed-header {
    padding-right: 0.9375rem;
  }
}
</style>

