<template>
<div class="app-container documentation-container">
    <a class="document-btn" target="_blank"
       href="https://vue3-element-admin-site.midfar.com/zh/guide/essentials/router-and-nav.html">도움말 문서</a>
    <a class="document-btn" target="_blank"
       href="https://github.com/midfar/vue3-element-admin">Github 저장소</a>
    <dropdown-menu class="document-btn" :items="articleList" title="시리즈 게시글" />

  </div>
</template>

<script>
import DropdownMenu from '@/components/Share/DropdownMenu';
import { defineComponent } from 'vue';

export default defineComponent({
  name: 'Documentation',
  components: { DropdownMenu },
  data() {
    return {
      articleList: [
        { title: '기초편', href: 'https://juejin.im/post/59097cd7a22b9d0065fb61d2' },
        { title: '로그인 권한편', href: 'https://juejin.im/post/591aa14f570c35006961acac' },
        { title: '실전편', href: 'https://juejin.im/post/593121aa0ce4630057f70d35' },
        { title: 'vue-admin-template 편', href: 'https://juejin.im/post/595b4d776fb9a06bbe7dba56' },
        { title: 'v4.0 편', href: 'https://juejin.im/post/5c92ff94f265da6128275a85' },
        { title: '컴포넌트 자체 캡슐화', href: 'https://segmentfault.com/a/1190000009090836' },
        { title: '아이콘을 우아하게 사용하는 법', href: 'https://juejin.im/post/59bb864b5188257e7a427c09' }

      ]
    };
  },
  created() {

  }
});
</script>

<style lang="scss" scoped>
.documentation-container {
  margin: 3.125rem;
  display: flex;
  flex-wrap: wrap;
  justify-content: space-evenly;

  @media (max-width: 768px) {
    margin: 1.25rem;
  }

  .document-btn {
    flex-shrink: 0;
    display: block;
    cursor: pointer;
    background: black;
    color: white;
    height: 60px;
    width: 200px;
    margin-bottom: 1rem;
    line-height: 60px;
    font-size: 1.25rem;
    text-align: center;
  }
}
</style>

