<template>
<div id="tags-view-container" class="tags-view-container">
    <scroll-pane ref="scrollPane" class="tags-view-wrapper" @scroll="handleScroll">
      <router-link v-for="tag in visitedViews" :key="tag.fullPath"
                   :to="{ path: tag.path, query: tag.query, fullPath: tag.fullPath }" custom
                   v-slot="{ navigate, isActive, isExactActive }" ref="tag">
        <span :class="['tags-view-item', isActive && 'router-link-active', isExactActive && 'router-link-exact-active']"
              @click="navigate"
              @click.middle="!isAffix(tag) ? closeSelectedTag(tag) : ''"
              @contextmenu.prevent="openMenu(tag, $event)">
          <span v-if="isAffix(tag)" class="affix-dot" />
          <template v-if="tag.title && tag.title.includes('|')">
            {{ tag.title.split('|')[0] }}
            <span class="tag-subtitle">({{ tag.title.split('|')[1] }})</span>
          </template>
          <template v-else>
            {{ tag.title }}
          </template>
          <icon-close v-if="!isAffix(tag)" class="el-icon-close" @click.prevent.stop="closeSelectedTag(tag)" />
        </span>
      </router-link>
    </scroll-pane>
    <ul v-show="visible" :style="{ left: left + 'px', top: top + 'px' }" class="contextmenu">
      <li @click="refreshSelectedTag(selectedTag)">새로고침</li>
      <li v-if="getMenuId(selectedTag)" @click="toggleFavorite(selectedTag)">
        {{ isFavoriteTag(selectedTag) ? '즐겨찾기 해제' : '즐겨찾기 추가' }}
      </li>
      <li v-if="getMenuId(selectedTag)" @click="toggleAffix(selectedTag)">
        {{ isAffix(selectedTag) ? '탭 고정 해제' : '탭 고정' }}
      </li>
      <li @click="closeSelectedTag(selectedTag)">닫기</li>
      <li @click="closeOthersTags">다른 탭 닫기</li>
      <li @click="closeAllTags(selectedTag)">전체 닫기</li>
    </ul>
  </div>
</template>

<script>
import { defineComponent } from 'vue';
import ScrollPane from './ScrollPane';
import path from 'path-browserify';
import store from '@/store';
import { Close as IconClose } from '@element-plus/icons-vue';
import Sortable from 'sortablejs';
import { toggleMenuFavorite } from '@/api/menu-favorite';
import { updateUserMenuSetting, updateUserMenuSettingSort } from '@/api/user';
import { ElMessage } from 'element-plus';

function filterAffixTags(routes, basePath = '/') {
  let tags = [];
  routes.forEach(route => {
    if (route.meta && route.meta.affix) {
      const tagPath = path.resolve(basePath, route.path);
      tags.push({
        fullPath: tagPath,
        path: tagPath,
        name: route.name,
        meta: { ...route.meta }
      });
    }
    if (route.children) {
      const tempTags = filterAffixTags(route.children, route.path);
      if (tempTags.length >= 1) {
        tags = [...tags, ...tempTags];
      }
    }
  });
  return tags;
}

export default defineComponent({
  name: 'TagsView',
  components: { ScrollPane, IconClose },
  data() {
    return {
      visible: false,
      top: 0,
      left: 0,
      selectedTag: {},
      affixTags: [],
      sortable: null
    };
  },
  computed: {
    visitedViews() {
      return store.tagsView().visitedViews;
    },
    routes() {
      return store.permission().routes;
    },
    roles() {
      return store.user().roles;
    }
  },
  watch: {
    $route() {
      this.addTags();
      this.moveToCurrentTag();
    },
    visible(value) {
      if (value) {
        document.body.addEventListener('click', this.closeMenu);
      } else {
        document.body.removeEventListener('click', this.closeMenu);
      }
    },
    routes() {
      this.initTags();
    }
  },
  mounted() {
    this.initTags();
    this.addTags();
    this.initSortable();
  },
  methods: {
    getMenuId(view) {
      if (view.meta && view.meta.id) return view.meta.id;
      const key = view.name || view.path;
      const perm = store.permission().permissions[key];
      return perm?.id;
    },
    isFavoriteTag(view) {
      if (view.meta && view.meta.isFavorite !== undefined) return view.meta.isFavorite;
      const key = view.name || view.path;
      const perm = store.permission().permissions[key];
      return !!perm?.isFavorite;
    },
    async toggleFavorite(view) {
      const menuId = this.getMenuId(view);
      if (!menuId) return;
      try {
        const res = await toggleMenuFavorite(menuId);
        if (res.code === 20000) {
          const isFavorite = res.data;
          if (view.meta) view.meta.isFavorite = isFavorite;

          ElMessage.success(isFavorite ? '즐겨찾기에 추가되었습니다.' : '즐겨찾기에서 제거되었습니다.');
          await store.permission().generateRoutes(store.user().roles);
        }
      } catch (error) {
        console.error(error);
      }
    },
    initSortable() {
      const el = this.$refs.scrollPane.$el.querySelector('.el-scrollbar__view');
      this.sortable = Sortable.create(el, {
        ghostClass: 'sortable-ghost',
        setData: function(dataTransfer) {
          dataTransfer.setData('Text', '');
        },
        onEnd: async evt => {
          const views = [...store.tagsView().visitedViews];
          const targetRow = views.splice(evt.oldIndex, 1)[0];
          views.splice(evt.newIndex, 0, targetRow);

          store.tagsView().setVisitedViews(views);

          const pinnedItems = views
            .filter(v => v.meta && v.meta.affix && v.meta.id)
            .map((v, index) => ({
              menuId: v.meta.id,
              sortOrder: index + 1
            }));

          if (pinnedItems.length > 0) {
            try {
              await updateUserMenuSettingSort({ items: pinnedItems });
            } catch (error) {
              console.error('Failed to save tag order:', error);
            }
          }
        }
      });
    },
    isCurrentRoute(route) {
      return route.fullPath === this.$route.fullPath;
    },
    isAffix(tag) {
      return tag.meta && tag.meta.affix;
    },
    async toggleAffix(view) {
      const menuId = this.getMenuId(view);
      if (!menuId) return;
      try {
        const newAffix = !this.isAffix(view);
        const res = await updateUserMenuSetting({
          menuId: menuId,
          affix: newAffix
        });
        if (res.code === 20000 || res.success) {
          if (view.meta) view.meta.affix = newAffix;
          ElMessage.success(newAffix ? '탭이 고정되었습니다.' : '탭 고정이 해제되었습니다.');
        }
      } catch (error) {
        console.error(error);
        ElMessage.error('설정 저장에 실패했습니다.');
      }
    },
    initTags() {
      const affixTags = this.affixTags = filterAffixTags(this.routes);
      affixTags.sort((a, b) => (a.meta.affixSortOrder || 0) - (b.meta.affixSortOrder || 0));

      for (const tag of affixTags) {
        if (tag.name || (tag.meta && tag.meta.title)) {
          store.tagsView().addVisitedView(tag);
        }
      }
    },
    addTags() {
      const { name, meta } = this.$route;
      if (name || (meta && meta.title)) {
        store.tagsView().addView(this.$route);
      }
      return false;
    },
    moveToCurrentTag() {
      const tags = this.$refs.tag;
      this.$nextTick(() => {
        if (!tags) return;
        for (const tag of tags) {
          if (tag.to.fullPath === this.$route.fullPath) {
            this.$refs.scrollPane.moveToTarget(tag);
            break;
          }
        }
      });
    },
    refreshSelectedTag(view) {
      store.tagsView().delCachedView(view);
      const { fullPath } = view;
      this.$nextTick(() => {
        this.$router.replace({
          path: '/redirect' + fullPath
        });
      });
    },
    closeSelectedTag(view) {
      store.tagsView().delView(view);
      const visitedViews = store.tagsView().visitedViews;
      if (this.isCurrentRoute(view)) {
        this.toLastView(visitedViews, view);
      }
    },
    closeOthersTags() {
      this.$router.push(this.selectedTag);
      store.tagsView().delOthersViews(this.selectedTag);
      this.moveToCurrentTag();
    },
    closeAllTags(view) {
      store.tagsView().delAllViews();
      const visitedViews = store.tagsView().visitedViews;
      if (this.affixTags.some(tag => tag.path === view.path)) {
        return;
      }
      this.toLastView(visitedViews, view);
    },
    toLastView(visitedViews, view) {
      const visitedHistory = store.tagsView().visitedHistory;
      const prevFullPath = visitedHistory[visitedHistory.length - 1];
      const prevView = visitedViews.find(v => v.fullPath === prevFullPath);

      if (prevView) {
        this.$router.push(prevView.fullPath);
      } else {
        const latestView = visitedViews.slice(-1)[0];
        if (latestView) {
          this.$router.push(latestView.fullPath);
        } else {
          if (view.name === 'Dashboard') {
            this.$router.replace({ path: '/redirect' + view.fullPath });
          } else {
            this.$router.push('/');
          }
        }
      }
    },
    openMenu(tag, e) {
      const menuMinWidth = 105;
      const offsetLeft = this.$el.getBoundingClientRect().left;
      const offsetWidth = this.$el.offsetWidth;
      const maxLeft = offsetWidth - menuMinWidth;
      const left = e.clientX - offsetLeft + 15;

      if (left > maxLeft) {
        this.left = maxLeft;
      } else {
        this.left = left;
      }

      this.top = e.clientY;
      this.visible = true;
      this.selectedTag = tag;
    },
    closeMenu() {
      this.visible = false;
    },
    handleScroll() {
      this.closeMenu();
    }
  }
});
</script>

<style lang="scss" scoped>
@import "./TagsViewStyles.scss";
</style>

<style lang="scss">
@import "./TagsViewGlobal.scss";
</style>

