<template>
<el-breadcrumb class="app-breadcrumb" separator="/">
    <transition-group name="breadcrumb">
      <el-breadcrumb-item v-for="(item, index) in levelList" :key="item.path">
        <span v-if="item.redirect === 'noRedirect' || index == levelList.length - 1" class="no-redirect">{{
          item.meta.title
        }}</span>
        <a v-else @click.prevent="handleLink(item)">{{ item.meta.title }}</a>
      </el-breadcrumb-item>
    </transition-group>
  </el-breadcrumb>
</template>

<script>
import { defineComponent } from 'vue';
import { compile } from 'path-to-regexp';

export default defineComponent({
  name: 'Breadcrumb',
  data() {
    return {
      levelList: null
    };
  },
  watch: {
    $route(route) {

      if (route.path.startsWith('/redirect/')) {
        return;
      }
      this.getBreadcrumb();
    }
  },
  created() {
    this.getBreadcrumb();
  },
  methods: {
    getBreadcrumb() {

      const matched = this.$route.matched.filter(item => item.meta && item.meta.title);

      this.levelList = matched.filter(item => item.meta && item.meta.title && item.meta.breadcrumb !== false);
    },
    isDashboard(route) {
      const name = route && route.name;
      if (!name) {
        return false;
      }
      return name.trim().toLocaleLowerCase() === 'Dashboard'.toLocaleLowerCase();
    },
    pathCompile(path) {

      const { params } = this.$route;
      var toPath = compile(path);
      return toPath(params);
    },
    handleLink(item) {
      const { redirect, path } = item;
      if (redirect) {
        this.$router.push(redirect);
        return;
      }
      this.$router.push(this.pathCompile(path));
    }
  }
});
</script>

<style lang="scss" scoped>
.app-breadcrumb.el-breadcrumb {
  display: inline-block;
  font-size: 0.875rem;
  line-height: 50px;
  margin-left: 0.5rem;

  .no-redirect {
    color: #97a8be;
    cursor: text;
  }

  .el-breadcrumb__item {
    :deep(.el-breadcrumb__inner a) {
      font-weight: 400 !important;
    }
  }
}
</style>

