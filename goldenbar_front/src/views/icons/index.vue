<template>
<div class="icons-container">
    <aside>
      <a href="https://vue3-element-admin-site.midfar.com/guide/advanced/icon.html" target="_blank">Add and use
      </a>
    </aside>
    <el-tabs type="border-card">
      <el-tab-pane label="Icons">
        <div class="grid">
          <div v-for="item of svgIcons" :key="item" @click="handleClipboard(generateIconCode(item), $event)">
            <el-tooltip placement="top">
              <template #content>
                {{ generateIconCode(item) }}
              </template>
              <div class="icon-item">
                <svg-icon :icon-class="item" class-name="disabled" />
                <span>{{ item }}</span>
              </div>
            </el-tooltip>
          </div>
        </div>
      </el-tab-pane>
      <el-tab-pane label="Element-UI Icons">
        <div class="grid">
          <div v-for="item of elementIcons" :key="item" @click="handleClipboard(generateElementIconCode(item), $event)">
            <el-tooltip placement="top">
              <template #content>
                {{ generateElementIconCode(item) }}
              </template>
              <div class="icon-item">
                <el-icon>

                  <component :is="item" class="svg-icon disabled"></component>
                </el-icon>
                <span>{{ item }}</span>
              </div>
            </el-tooltip>
          </div>
        </div>
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script>

import clipboard from '@/utils/clipboard';
import svgIcons from './svg-icons';
import ElementPlusIconsVue from './element-icons';
import { defineComponent } from 'vue';

export default defineComponent({
  name: 'Icons',
  components: {
    ...ElementPlusIconsVue
  },
  data() {
    return {
      svgIcons,
      elementIcons: Object.keys(ElementPlusIconsVue)
    };
  },
  methods: {

    generateIconCode(symbol) {

      return `<svg-icon icon-class="${symbol}" />`;
    },

    generateElementIconCode(symbol) {

      return `<el-icon><${symbol} /></el-icon>`;
    },

    handleClipboard(text, event) {

      clipboard(text, event);
    }
  }
});
</script>

<style lang="scss" scoped>
.icons-container {
  margin: 0.625rem 1.25rem 0;
  overflow: hidden;

  .grid {
    position: relative;
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
  }

  .icon-item {
    margin: 1.25rem auto;
    height: 85px;
    text-align: center;
    width: 100%;
    max-width: 100px;
    font-size: 1.5rem;
    color: #24292e;
    cursor: pointer;
  }

  span {
    display: block;
    font-size: 0.95rem;
    margin-top: 0.625rem;
  }

  .disabled {
    pointer-events: none;
  }
}
</style>

