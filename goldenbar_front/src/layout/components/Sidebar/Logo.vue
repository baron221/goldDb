<template>
<div class="sidebar-logo-container" :class="{ 'collapse': collapse }">
    <transition name="sidebarLogoFade">
      <router-link v-if="collapse" key="collapse" class="sidebar-logo-link" to="/">
        <img v-if="logo" :src="logo" class="sidebar-logo" :title="title" />
      </router-link>
      <router-link v-else key="expand" class="sidebar-logo-link" to="/">
        <img v-if="logo" :src="logo" class="sidebar-logo">
      </router-link>
    </transition>
  </div>
</template>

<script>
import { defineComponent } from 'vue';
import { mapState } from 'pinia';
import store from '@/store';

export default defineComponent({
  name: 'SidebarLogo',
  props: {
    collapse: {
      type: Boolean,
      required: true
    }
  },
  data() {
    return {
      title: 'Goldb'
    };
  },
  computed: {
    ...mapState(store.settings, ['isDark']),
    logo() {
      return this.isDark ? '/ci_dark.png' : '/ci_dark.png';
    }
  }
});
</script>

<style lang="scss" scoped>
.sidebarLogoFade-enter-active {
  transition: opacity 1.5s;
}

.sidebarLogoFade-enter,
.sidebarLogoFade-leave-to {
  opacity: 0;
}

.sidebar-logo-container {
  position: relative;
  width: 100%;
  height: 47px;
  line-height: 47px;

  text-align: center;
  overflow: hidden;

  & .sidebar-logo-link {
    height: 100%;
    width: 100%;

    & .sidebar-logo {

      height: 32px;
      vertical-align: middle;
      margin: auto;
      margin-top: 0.375rem;
    }

    & .sidebar-title {
      display: inline-block;
      margin: 0;
      color: #fff;
      font-weight: 600;
      line-height: 50px;
      font-size: 0.875rem;
      font-family: 'S-CoreDream', 'Jost', sans-serif;
      vertical-align: middle;
    }
  }

  &.collapse {
    .sidebar-logo {
      margin-right: 0rem;
    }
  }
}
</style>

