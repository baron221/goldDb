<template>
<div :class="{ 'show': show }" class="header-search">
    <svg-icon class-name="search-icon" icon-class="search" @click.stop="click" />

    <div class="search-container-luxury">
      <el-select
        ref="headerSearchSelect"
        v-model="search"
        :remote-method="querySearch"
        filterable
        default-first-option
        remote
        :placeholder="$t('globalSearch.placeholder')"
        class="header-search-select"
        @change="change"
        @keyup.enter="handleEnter"
      >
        <template #prefix>
          <el-icon class="inner-search-icon"><Search /></el-icon>
        </template>
        <el-option v-if="searchQuery" :value="{ path: '/search', query: { q: searchQuery }, isGlobal: true }"
                   :label="`'${searchQuery}' ${$t('globalSearch.integratedSearch')}`" class="global-search-option" />
        <el-option v-for="optItem in options" :key="optItem.item.path" :value="optItem.item"
                   :label="optItem.item.title.join(' > ')" />
      </el-select>

    </div>
  </div>
</template>

<script>

import { defineComponent } from 'vue';
import Fuse from 'fuse.js';
import path from 'path-browserify';
import store from '@/store';
import { Search } from '@element-plus/icons-vue';

export default defineComponent({
  name: 'HeaderSearch',
  components: { Search },
  data() {
    return {
      search: '',
      searchQuery: '',
      options: [],
      searchPool: [],
      show: false,
      fuse: undefined
    };
  },
  computed: {
    routes() {
      return store.permission().routes;
    }
  },
  watch: {
    routes() {
      this.searchPool = this.generateRoutes(this.routes);
    },
    searchPool(list) {
      this.initFuse(list);
    },
    show(value) {
      if (value) {
        document.body.addEventListener('click', this.close);
      } else {
        document.body.removeEventListener('click', this.close);
      }
    }
  },
  mounted() {
    this.searchPool = this.generateRoutes(this.routes);
  },
  methods: {
    click() {
      this.show = !this.show;
      if (this.show) {
        if (this.$refs.headerSearchSelect) {
          this.$refs.headerSearchSelect.focus();
        }
      }
    },
    close() {
      if (this.$refs.headerSearchSelect) {
        this.$refs.headerSearchSelect.blur();
      }
      this.options = [];
      this.show = false;
    },
    change(val) {
      if (val.isGlobal) {
        this.$router.push({ path: val.path, query: val.query });
      } else {
        this.$router.push(val.path);
      }
      this.search = '';
      this.searchQuery = '';
      this.options = [];
      this.$nextTick(() => {
        this.show = false;
      });
    },
    handleEnter() {
      if (this.searchQuery) {
        this.$router.push({ path: '/search', query: { q: this.searchQuery }});
        this.search = '';
        this.searchQuery = '';
        this.options = [];
        this.show = false;
      }
    },
    initFuse(list) {
      this.fuse = new Fuse(list, {
        shouldSort: true,
        threshold: 0.4,
        location: 0,
        distance: 100,
        maxPatternLength: 32,
        minMatchCharLength: 1,
        keys: [{
          name: 'title',
          weight: 0.7
        }, {
          name: 'path',
          weight: 0.3
        }]
      });
    },

    generateRoutes(routes, basePath = '/', prefixTitle = []) {
      let res = [];

      for (const router of routes) {

        if (router.meta && router.meta.hidden) { continue; }

        const data = {
          path: path.resolve(basePath, router.path),
          title: [...prefixTitle]
        };

        if (router.meta && router.meta.title) {
          data.title = [...data.title, router.meta.title];

          if (router.redirect !== 'noRedirect') {

            res.push(data);
          }
        }

        if (router.children) {
          const tempRoutes = this.generateRoutes(router.children, data.path, data.title);
          if (tempRoutes.length >= 1) {
            res = [...res, ...tempRoutes];
          }
        }
      }
      return res;
    },
    querySearch(query) {
      this.searchQuery = query;
      if (query !== '') {
        this.options = this.fuse.search(query);
      } else {
        this.options = [];
      }
    }
  }
});
</script>

<style lang="scss" scoped>
.header-search {
  font-size: 0 !important;
  display: flex;
  align-items: center;

  .search-icon {
    cursor: pointer;
    font-size: 1.125rem;
    vertical-align: middle;
    color: #888888;
    transition: color 0.3s;
    &:hover { color: #c5a880; }
  }

  .search-container-luxury {
    display: flex;
    align-items: center;
    gap: 0.625rem;
    transition: all 0.3s;
    width: 0;
    overflow: hidden;
    opacity: 0;
  }

  .header-search-select {
    font-size: 0.875rem;
    flex: 1;
    background: transparent;
    border-radius: 0;
    display: inline-block;
    vertical-align: middle;
    font-family: 'S-CoreDream', 'Jost', sans-serif;

    :deep(.el-input) {
      .el-input__wrapper {
        padding: 0 0.625rem;
        background-color: #fcfcfb !important;
        border: 1px solid #eae6df !important;
        box-shadow: none !important;
        border-radius: 2px;
        height: 38px;
        transition: all 0.3s ease;

        &:hover, &.is-focus {
          border-color: #c5a880 !important;
        }

        .el-input__inner {
          border-radius: 0;

          font-weight: 500;
          &::placeholder { color: #bbbbbb; }
        }

        .el-input__prefix {
          color: #aaaaaa;
          margin-right: 0.3125rem;
        }
      }
    }
  }

  .search-submit-btn-luxury {
    background-color: #222222;
    border: none;
    color: #ffffff;
    width: 38px;
    height: 38px;
    border-radius: 2px;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: all 0.3s ease;
    flex-shrink: 0;

    &:hover {
      background-color: #c5a880;
      transform: translateY(-1px);
    }

    .el-icon {
      font-size: 1rem;
    }
  }

  &.show {
    .search-container-luxury {
      width: 280px;
      margin-left: 0.9375rem;
      opacity: 1;
    }
    .search-icon {
      color: #c5a880;
    }
  }
}

.global-search-option {
  color: #c5a880 !important;
  font-weight: 700;
  border-bottom: 1px dashed #f0eeeb;
  font-size: 0.9rem;
  background-color: #fbfaf7;
  letter-spacing: 0.5px;
}
</style>

