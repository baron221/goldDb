<template>
<div class="drawer-container">
    <div>
      <h3 class="drawer-title">시스템 설정</h3>

      <div class="drawer-item">
        <span>Tags-View 사용</span>
        <el-switch v-model="tagsView" class="drawer-switch" />
      </div>

      <div class="drawer-item">
        <span>Header 고정</span>
        <el-switch v-model="fixedHeader" class="drawer-switch" />
      </div>

      <div class="drawer-item">
        <span>사이드바 로고</span>
        <el-switch v-model="sidebarLogo" class="drawer-switch" />
      </div>

      <div class="drawer-item">
        <span>사이드바 하위 메뉴 팝업 표시</span>
        <el-switch v-model="secondMenuPopup" class="drawer-switch" />
      </div>

    </div>
  </div>
</template>

<script>

import { defineComponent } from 'vue';
import store from '@/store';

export default defineComponent({
  components: {

  },
  data() {
    return {};
  },
  computed: {
    fixedHeader: {
      get() {
        return store.settings().fixedHeader;
      },
      set(val) {
        store.settings().changeSetting({
          key: 'fixedHeader',
          value: val
        });
      }
    },
    tagsView: {
      get() {
        return store.settings().tagsView;
      },
      set(val) {
        store.settings().changeSetting({
          key: 'tagsView',
          value: val
        });
      }
    },
    sidebarLogo: {
      get() {
        return store.settings().sidebarLogo;
      },
      set(val) {
        store.settings().changeSetting({
          key: 'sidebarLogo',
          value: val
        });
      }
    },
    secondMenuPopup: {
      get() {
        return store.settings().secondMenuPopup;
      },
      set(val) {
        store.settings().changeSetting({
          key: 'secondMenuPopup',
          value: val
        });
      }
    }
  },
  methods: {

    themeChange(val) {
      store.settings().changeSetting({
        key: 'theme',
        value: val
      });
    }
  }
});
</script>

<style lang="scss" scoped>
.drawer-container {
  padding: 1.5rem;
  font-size: 0.875rem;
  line-height: 1.5;
  word-wrap: break-word;

  .drawer-title {
    margin-bottom: 0.95rem;
    color: rgba(0, 0, 0, .85);
    font-size: 0.875rem;
    line-height: 22px;
  }

  .drawer-item {
    display: flex;
    align-items: center;
    justify-content: space-between;
    color: rgba(0, 0, 0, .65);
    font-size: 0.875rem;
    padding: 0.95rem 0;
  }

  .drawer-switch {
    float: right
  }
}
</style>

