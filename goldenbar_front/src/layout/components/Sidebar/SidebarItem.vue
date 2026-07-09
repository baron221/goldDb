<template>
<div v-if="!isItemHidden" class="root-sidebar-item">
    <template
      v-if="hasOneShowingChild(item.children, item) && (!onlyOneChild.children || onlyOneChild.noShowingChildren) && !(item.meta && item.meta.alwaysShow)">
      <app-link class="link" :to="resolvePath(onlyOneChild.path)">
        <el-menu-item class=" left-menu-item" v-if="onlyOneChild.meta" :index="resolvePath(onlyOneChild.path)"
                      :class="{ 'submenu-title-noDropdown': !isNest }">

          <template v-if="get2MetaIconPath(onlyOneChild, item)">
            <template v-if="typeof get2MetaIconPath(onlyOneChild, item) === 'string'">
              <el-icon v-if="elementIconList.includes(get2MetaIconPath(onlyOneChild, item))" class="sub-el-icon">
                <component :is="get2MetaIconPath(onlyOneChild, item)" />
              </el-icon>
              <svg-icon v-else :icon-class="get2MetaIconPath(onlyOneChild, item)" />
              <span v-if="secondMenuPopup && isTopRoute" class="text text-one text-one-added">{{ generateTitle(onlyOneChild.meta.title) }}</span>
            </template>
            <component v-else :is="get2MetaIconPath(onlyOneChild, item)" class="svg-icon el-svg-icon" />
          </template>
          <template #title>
            <span class="text text-one">{{ generateTitle(onlyOneChild.meta.title) }}</span>
          </template>
        </el-menu-item>
      </app-link>
    </template>

    <el-sub-menu class="left-sub-menu" v-else ref="subMenu" :index="resolvePath(item.path)" teleported>
      <template v-if="item.meta" #title>

        <template v-if="getMetaIconPath(item)">
          <template v-if="typeof getMetaIconPath(item) === 'string'">
            <el-icon v-if="elementIconList.includes(getMetaIconPath(item))" class="sub-el-icon">
              <component :is="getMetaIconPath(item)" />
            </el-icon>
            <svg-icon v-else :icon-class="getMetaIconPath(item)" />
          </template>
          <component v-else :is="getMetaIconPath(item)" class="svg-icon el-svg-icon" />
        </template>

        <span class="text text-two">{{ generateTitle(item.meta.title) }}</span>
      </template>
      <sidebar-item v-for="child in item.children" :key="child.path" :is-nest="true" :item="child"
                    :base-path="resolvePath(child.path)" class="nest-menu" />
    </el-sub-menu>
  </div>
</template>

<script>
import { defineComponent } from 'vue';
import path from 'path-browserify';
import { isExternal } from '@/utils/validate';

import AppLink from './Link';
import FixiOSBug from './FixiOSBug';
import { mapState } from 'pinia';
import store from '@/store';
import elementIcons from '@/views/icons/element-icons';

export default defineComponent({
  name: 'SidebarItem',
  components: {

    AppLink,
    ...elementIcons
  },
  mixins: [FixiOSBug],
  props: {

    item: {
      type: Object,
      required: true
    },
    isNest: {
      type: Boolean,
      default: false
    },
    basePath: {
      type: String,
      default: ''
    },
    isTopRoute: { 
      type: Boolean,
      default: false
    }
  },
  data() {

    this.onlyOneChild = null;
    return {
      elementIconList: Object.keys(elementIcons)
    };
  },
  computed: {
    ...mapState(store.settings, {
      secondMenuPopup: 'secondMenuPopup'
    }),
    isItemHidden() {
      if (this.item.meta && this.item.meta.hidden) {
        return true;
      }
      if (this.item.children && this.item.children.length > 0) {
        const hasVisibleDescendant = (node) => {
          if (node.meta && node.meta.hidden) {
            return false;
          }
          if (node.children && node.children.length > 0) {
            return node.children.some(child => hasVisibleDescendant(child));
          }
          return true;
        };
        const hasVisible = this.item.children.some(child => hasVisibleDescendant(child));
        if (!hasVisible) {
          return true;
        }
      }
      return false;
    }
  },
  methods: {

    getMetaIconPath(item) {
      return item.meta && item.meta.icon;
    },

    get2MetaIconPath(onlyOneChild, item) {
      return onlyOneChild.meta.icon || (item.meta && item.meta.icon);
    },

    hasOneShowingChild(children = [], parent) {
      const showingChildren = children.filter(item => {
        if (item.meta && item.meta.hidden) {
          return false;
        } else {

          this.onlyOneChild = item;
          return true;
        }
      });

      if (showingChildren.length === 1) {
        return true;
      }

      if (showingChildren.length === 0) {
        this.onlyOneChild = { ...parent, path: '', noShowingChildren: true };
        return true;
      }

      return false;
    },

    resolvePath(routePath) {
      if (isExternal(routePath)) {
        return routePath;
      }
      if (isExternal(this.basePath)) {
        return this.basePath;
      }
      return path.resolve(this.basePath, routePath);
    },
    generateTitle(title) {
      if (!title) return '';
      const key = 'route.' + title;
      if (this.$te && this.$te(key)) {
        return this.$t(key);
      }
      return title;
    }
  }
});
</script>

<style lang="scss" scoped>
.link :deep(.el-menu-tooltip__trigger) {
  position: relative;
  padding: 0;
}

.el-svg-icon {
  width: 1em;
  height: 1em;
  vertical-align: -0.15em;
  fill: currentColor;
  overflow: hidden;
  transition: transform 0.3s ease;
}

.sub-el-icon {
  color: currentColor;
  width: 1em !important;
  height: 1em !important;
  font-size: 0.875rem !important;
  margin-right: 0.95rem;
  transition: transform 0.3s ease;
}

.text {
  font-family: 'S-CoreDream', 'Jost', sans-serif;
  font-size: 0.95rem;
  letter-spacing: 0.5px;
  transition: all 0.3s;
}

.text-one {
  font-weight: 500;
}

.text-two {
  font-weight: 600;
  text-transform: uppercase;
  font-size: 0.95rem;
  letter-spacing: 1px;
}

.el-menu-item:hover, .el-sub-menu__title:hover {
  .el-svg-icon, .sub-el-icon {
    transform: translateX(3px);
  }
}

.el-menu-item.is-active {
  .text {
    font-weight: 700;
  }
}
</style>

