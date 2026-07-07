<template>
<div v-loading.fullscreen.lock="fullscreenLoading" class="main-article" element-loading-text="Efforts to generate PDF">
    <div class="article__heading">
      <div class="article__heading__title">
        {{ article.title }}
      </div>
    </div>
    <div style="color: #ccc;">
      This article is from Evan You on <a target="_blank" href="https://medium.com/the-vue-point/plans-for-the-next-iteration-of-vue-js-777ffea6fabf">medium</a>
    </div>
    <div ref="content" class="node-article-content" v-html="article.content" />
  </div>
</template>

<script>

import { defineComponent } from 'vue';

export default defineComponent({
  data() {
    return {
      article: '',
      fullscreenLoading: true
    };
  },
  mounted() {
    this.fetchData();
  },
  methods: {

    fetchData() {
      import('./content.js').then(data => {
        const { title } = data.default;
        document.title = title;
        this.article = data.default;
        setTimeout(() => {
          this.fullscreenLoading = false;

          setTimeout(() => {
            window.print();
          }, 100);
        }, 3000);
      });
    }
  }
});
</script>

<style lang="scss">
@mixin clearfix {
  &:before {
    display: table;
    content: '';
    clear: both;
  }

  &:after {
    display: table;
    content: '';
    clear: both;
  }
}

.main-article {
  padding: 1.25rem;
  margin: 0 auto;
  display: block;
  width: 100%;
  max-width: 740px;
  box-sizing: border-box;
  background: #fff;
}

.article__heading {
  position: relative;
  padding: 0 0 1.25rem;
  overflow: hidden;
}

.article__heading__title {
  display: block;
  display: -webkit-box;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  word-wrap: break-word;
  overflow-wrap: break-word;
  font-size: 2rem;
  line-height: 48px;
  font-weight: 600;
  color: #333;
  overflow: hidden;
}

.node-article-content {
  margin: 1.25rem 0 0;
  @include clearfix;
  font-size: 1rem;
  color: #333;
  letter-spacing: 0.5px;
  line-height: 28px;
  margin-bottom: 1.875rem;
  font-family: medium-content-serif-font, Georgia, Cambria, "Times New Roman", Times, serif;

  &> :last-child {
    margin-bottom: 0;
  }

  b,
  strong {
    font-weight: inherit;
    font-weight: bolder;
  }

  img {
    max-width: 100%;
    display: block;
    margin: 0 auto;
  }

  p {
    font-weight: 400;
    font-style: normal;
    font-size: 1.3125rem;
    line-height: 1.58;
    letter-spacing: -.003em;

  }

  ul {
    margin-bottom: 1.875rem;
  }

  li {
    --x-height-multiplier: 0.375;
    --baseline-multiplier: 0.17;

    letter-spacing: .01rem;
    font-weight: 400;
    font-style: normal;
    font-size: 1.3125rem;
    line-height: 1.58;
    letter-spacing: -.003em;
    margin-left: 1.875rem;
    margin-bottom: 0.875rem;
  }

  a {
    text-decoration: none;
    background-repeat: repeat-x;
    background-image: linear-gradient(to right, rgba(0, 0, 0, .84) 100%, rgba(0, 0, 0, 0) 0);
    background-size: 1px 1px;
    background-position: 0 calc(1em + 1px);
    padding: 0 0.375rem;
  }

  code {
    background: rgba(0, 0, 0, .05);
    padding: 0.1875rem 0.25rem;
    margin: 0 0.125rem;
    font-size: 1rem;
    display: inline-block;
  }

  img {
    border: 0;
  }

  img {
    -ms-interpolation-mode: bicubic;
  }

  blockquote {
    --x-height-multiplier: 0.375;
    --baseline-multiplier: 0.17;
    font-family: medium-content-serif-font, Georgia, Cambria, "Times New Roman", Times, serif;
    letter-spacing: .01rem;
    font-weight: 400;
    font-style: italic;
    font-size: 1.3125rem;
    line-height: 1.58;
    letter-spacing: -.003em;
    border-left: 3px solid rgba(0, 0, 0, .84);
    padding-left: 1.25rem;
    margin-left: -1.4375rem;
    padding-bottom: 0.125rem;
  }

  a {
    text-decoration: none;
  }

  h2,
  h3,
  h4 {
    font-size: 2.125rem;
    line-height: 1.15;
    letter-spacing: -.015em;
    margin: 3.3125rem 0 0;
  }

  h4 {
    font-size: 1.625rem;
  }
}
</style>

