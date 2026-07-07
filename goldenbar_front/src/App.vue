<template>
  <el-config-provider :size="elementSize" :z-index="zIndex" :locale="locale">
    <div :class="'app-wrapper size-' + size">
      <router-view />
    </div>
  </el-config-provider>
</template>

<script>
import { defineComponent } from 'vue';
import ko from 'element-plus/dist/locale/ko.mjs';
import { mapState } from 'pinia';
import store from '@/store';

export default defineComponent({
  data() {
    return {
      locale: ko,
      zIndex: 3000
    };
  },
  computed: {
    ...mapState(store.app, ['size']),
    elementSize() {
      const largeSizes = ['s16', 's18', 's20', 's22', 's24'];
      const smallSizes = ['s10', 's11', 's12', 's13'];
      if (largeSizes.includes(this.size)) return 'large';
      if (smallSizes.includes(this.size)) return 'small';
      return 'default';
    }
  },
  watch: {
    size: {
      handler(newSize, oldSize) {
        if (oldSize) document.documentElement.classList.remove('size-' + oldSize);
        document.documentElement.classList.add('size-' + newSize);
      },
      immediate: true
    }
  }
});
</script>

<style lang="scss">

html,
body,
#app {
  width: 100%;
  height: 100%;
  margin: 0;
  padding: 0;
}

// HTML 레벨에서 전역 변수 정의 (다이얼로그 등 팝업 대응)
html {
  &.size-s10 { --cur-base: 10px; --cur-xs: 8px; --cur-sm: 9px; --cur-lg: 12px; --cur-xl: 14px; }
  &.size-s11 { --cur-base: 11px; --cur-xs: 9px; --cur-sm: 10px; --cur-lg: 13px; --cur-xl: 15px; }
  &.size-s12 { --cur-base: 12px; --cur-xs: 10px; --cur-sm: 11px; --cur-lg: 14px; --cur-xl: 16px; }
  &.size-s13 { --cur-base: 13px; --cur-xs: 11px; --cur-sm: 12px; --cur-lg: 16px; --cur-xl: 18px; }
  &.size-s14 { --cur-base: 14px; --cur-xs: 12px; --cur-sm: 13px; --cur-lg: 18px; --cur-xl: 20px; }
  &.size-s16 { --cur-base: 16px; --cur-xs: 13px; --cur-sm: 14px; --cur-lg: 20px; --cur-xl: 22px; }
  &.size-s18 { --cur-base: 18px; --cur-xs: 14px; --cur-sm: 16px; --cur-lg: 22px; --cur-xl: 24px; }
  &.size-s20 { --cur-base: 20px; --cur-xs: 16px; --cur-sm: 18px; --cur-lg: 24px; --cur-xl: 28px; }
  &.size-s22 { --cur-base: 22px; --cur-xs: 18px; --cur-sm: 20px; --cur-lg: 26px; --cur-xl: 30px; }
  &.size-s24 { --cur-base: 24px; --cur-xs: 20px; --cur-sm: 22px; --cur-lg: 28px; --cur-xl: 32px; }

  // 전역 변수 할당
  --el-font-size-extra-large: var(--cur-xl) !important;
  --el-font-size-large: var(--cur-lg) !important;
  --el-font-size-medium: var(--cur-base) !important;
  --el-font-size-base: var(--cur-base) !important;
  --el-font-size-small: var(--cur-sm) !important;
  --el-font-size-extra-small: var(--cur-xs) !important;

  --el-form-label-font-size: var(--cur-base) !important;
  --el-button-font-size: var(--cur-base) !important;
  --el-input-font-size: var(--cur-base) !important;
  --el-alert-title-font-size: var(--cur-base) !important;
  --el-alert-description-font-size: var(--cur-base) !important;

  --font-size: var(--cur-base) !important;
  font-size: var(--cur-base) !important;
}

.app-wrapper {
  width: 100%;
  height: 100%;
  font-size: var(--cur-base) !important;

  .el-form-item {
    --el-form-label-font-size: var(--cur-base) !important;
    --font-size: var(--cur-base) !important;
  }

  .el-alert {
    .el-alert__description {
      font-size: var(--cur-base) !important;
    }
  }
}

</style>
