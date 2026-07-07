import { defineStore } from 'pinia';
import type { RouteRecord } from 'vue-router';

interface ITagsViewState {

  visitedViews: Array<RouteRecord>;

  cachedViews: Array<RouteRecord>;

  visitedHistory: Array<string>;
}

export default defineStore({
  id: 'tagsView',
  state: ():ITagsViewState => ({
    visitedViews: [],
    cachedViews: [],
    visitedHistory: []
  }),
  getters: {},
  actions: {

    addView(view) {
      this.addVisitedView(view);
      this.addCachedView(view);
    },

    addVisitedView(view) {
      const allowDuplicate = view.meta && view.meta.allowDuplicate === true;

      if (!allowDuplicate) {
        const existingView = this.visitedViews.find(v => v.path === view.path);
        if (existingView) {

          const oldFullPath = existingView.fullPath;
          existingView.fullPath = view.fullPath;
          existingView.meta = { ...view.meta };

          const historyIdx = this.visitedHistory.indexOf(oldFullPath);
          if (historyIdx > -1) {
            this.visitedHistory.splice(historyIdx, 1);
          }
          if (!this.visitedHistory.includes(view.fullPath)) {
            this.visitedHistory.push(view.fullPath);
          }
          return;
        }
      }

      const idx = this.visitedHistory.indexOf(view.fullPath);
      if (idx > -1) {
        this.visitedHistory.splice(idx, 1);
      }
      this.visitedHistory.push(view.fullPath);

      if (this.visitedViews.some(v => v.fullPath === view.fullPath)) return;
      this.visitedViews.push(
        Object.assign({}, {
          path: view.path,
          fullPath: view.fullPath,
          name: view.name,
          meta: { ...view.meta },
          title: view.meta.title || 'no-name'
        })
      );
    },

    addCachedView(view) {
      const menuId = view.meta?.id || '';
      const cacheName = menuId ? `${menuId}-${view.fullPath}` : view.fullPath;

      if (this.cachedViews.includes(cacheName)) return;

      if (view.meta && view.meta.keepAlive === true) {
        this.cachedViews.push(cacheName);
      }
    },

    delView(view) {
      this.delVisitedView(view);
      this.delCachedView(view);
    },

    delVisitedView(view) {
      for (const [i, v] of this.visitedViews.entries()) {
        if (v.fullPath === view.fullPath) {
          this.visitedViews.splice(i, 1);
          break;
        }
      }
      const idx = this.visitedHistory.indexOf(view.fullPath);
      if (idx > -1) {
        this.visitedHistory.splice(idx, 1);
      }
    },

    delCachedView(view) {
      const menuId = view.meta?.id || '';
      const cacheName = menuId ? `${menuId}-${view.fullPath}` : view.fullPath;
      const index = this.cachedViews.indexOf(cacheName);
      if (index > -1) {
        this.cachedViews.splice(index, 1);
      }
    },

    delOthersViews(view) {
      this.delOthersVisitedViews(view);
      this.delOthersCachedViews(view);
    },

    delOthersVisitedViews(view) {
      this.visitedViews = this.visitedViews.filter(v => {
        return v.meta.affix || v.fullPath === view.fullPath;
      });
      const validPaths = this.visitedViews.map(v => v.fullPath);
      this.visitedHistory = this.visitedHistory.filter(path => validPaths.includes(path));
    },

    delOthersCachedViews(view) {
      const menuId = view.meta?.id || '';
      const cacheName = menuId ? `${menuId}-${view.fullPath}` : view.fullPath;
      const index = this.cachedViews.indexOf(cacheName);
      if (index > -1) {
        this.cachedViews = this.cachedViews.slice(index, index + 1);
      } else {
        this.cachedViews = [];
      }
    },

    delAllViews() {
      this.delAllVisitedViews();
      this.delAllCachedViews();
    },

    delAllVisitedViews() {
      const affixTags = this.visitedViews.filter(tag => tag.meta.affix);
      this.visitedViews = affixTags;
      const validPaths = affixTags.map(v => v.fullPath);
      this.visitedHistory = this.visitedHistory.filter(path => validPaths.includes(path));
    },

    resetVisitedViews() {
      this.visitedViews = [];
      this.cachedViews = [];
      this.visitedHistory = [];
    },

    delAllCachedViews() {
      this.cachedViews = [];
    },

    updateVisitedView(view) {
      for (let v of this.visitedViews) {
        if (v.fullPath === view.fullPath) {
          v = Object.assign(v, view);
          break;
        }
      }
    },

    updateVisitedViewTitle(fullPath: string, title: string) {
      const view = this.visitedViews.find(v => v.fullPath === fullPath);
      if (view) {
        (view as any).title = title;
      }
    },

    setVisitedViews(views) {
      this.visitedViews = views;
      const validPaths = views.map(v => v.fullPath);
      this.visitedHistory = this.visitedHistory.filter(path => validPaths.includes(path));
    }
  }
});
