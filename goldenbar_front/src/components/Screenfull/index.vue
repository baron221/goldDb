<template>
<div>
    <svg-icon class-name="screenfull-svg" :icon-class="isFullscreen ? 'exit-fullscreen' : 'fullscreen'" @click="click" />
  </div>
</template>

<script>
import { defineComponent } from 'vue';
import screenfull, { bindF11, unbindF11 } from '@/utils/screenfull';

export default defineComponent({
  name: 'Screenfull',
  data() {
    return {
      isFullscreen: false
    };
  },
  mounted() {
    this.init();
    bindF11();
  },
  beforeUnmount() {
    this.destroy();
    unbindF11();
  },
  methods: {
    click() {

      if (!screenfull.isEnabled) {
        ElMessage({
          message: 'you browser can not work',
          type: 'warning'
        });
        return false;
      }
      screenfull.toggle();
    },
    handleChange() {

      this.isFullscreen = screenfull.isFullscreen;
    },
    init() {
      if (screenfull.isEnabled) {
        screenfull.on('change', this.handleChange);
      }
    },
    destroy() {
      if (screenfull.isEnabled) {
        screenfull.off('change', this.handleChange);
      }
    }
  }
});
</script>

<style scoped>
.screenfull-svg {
  cursor: pointer;
  fill: #5a5e66;
  width: 0.7em;
  height: 0.7em;
}
</style>

