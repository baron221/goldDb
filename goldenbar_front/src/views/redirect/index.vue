<script>
import { defineComponent, h } from 'vue';

export default defineComponent({
  created() {

    const { params, query } = this.$route;
    const { path } = params;
    this.$router.replace({ path: '/' + path, query });
  },
  render() {
    return h('div'); 
  }
});
</script>
