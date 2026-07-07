import store from '@/store';
import { defineComponent } from 'vue';

export default defineComponent({
  computed: {
    device() {
      return store.app().device;
    }
  },
  mounted() {

    this.fixBugIniOS();
  },
  methods: {

    fixBugIniOS() {
      const $subMenu = this.$refs.subMenu;
      if ($subMenu) {
        const handleMouseleave = $subMenu.handleMouseleave;
        $subMenu.handleMouseleave = (e) => {
          if (this.device === 'mobile') {
            return;
          }
          handleMouseleave(e);
        };
      }
    }
  }
});
