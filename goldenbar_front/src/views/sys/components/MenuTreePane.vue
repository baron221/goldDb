<template>
<el-card shadow="never" class="menu-tree-card">
    <template #header>
      <div class="card-header">
        <div class="left-actions">
          <span>메뉴 구조</span>
          <el-input
            :model-value="searchQuery"
            placeholder="검색..."
            size="small"
            style="width: 150px; margin-left: 0.625rem;"
            clearable
            @input="val => $emit('update:searchQuery', val)"
          />
        </div>
        <div class="right-actions">
          <el-button v-permission="'create'" type="primary" size="small" @click="$emit('create-root')">루트 추가</el-button>
          <el-tooltip content="전체 펼치기" placement="top">
            <el-button size="small" circle @click="expandAll">
              <i class="fas fa-angles-down"></i>
            </el-button>
          </el-tooltip>
          <el-tooltip content="전체 접기" placement="top">
            <el-button size="small" circle @click="collapseAll">
              <i class="fas fa-angles-up"></i>
            </el-button>
          </el-tooltip>
          <el-tooltip content="새로고침" placement="top">
            <el-button size="small" circle @click="$emit('refresh')">
              <i class="fas fa-rotate"></i>
            </el-button>
          </el-tooltip>
        </div>
      </div>
    </template>

    <div class="tip-box mb-10">
      <i class="fas fa-info-circle mr-5"></i>
      <span>드래그하여 순서/계층 변경 가능</span>
    </div>

    <div class="tree-container" :style="{ height: 'calc(100vh - 300px)' }">
      <el-tree
        ref="menuTreeRef"
        :data="filteredMenuList"
        node-key="id"
        draggable
        highlight-current
        :expand-on-click-node="false"
        :default-expanded-keys="expandedKeys"
        :props="{ children: 'children', label: (d) => d.meta?.title }"
        class="modern-menu-tree"
        @node-click="node => $emit('node-click', node)"
        @node-drop="(draggingNode, dropNode, dropType) => $emit('node-drop', draggingNode, dropNode, dropType)"
        @node-expand="node => $emit('node-expand', node)"
        @node-collapse="node => $emit('node-collapse', node)"
      >
        <template #default="{ data }">
          <div class="custom-tree-node">
            <div class="node-label">
              <div class="icon-wrapper">
                <template v-if="data.meta?.icon">
                  <el-icon v-if="elementIconList.includes(data.meta.icon)" class="tree-node-icon">
                    <component :is="elementIconComponents[data.meta.icon]" />
                  </el-icon>
                  <svg-icon v-else :icon-class="data.meta.icon" class="tree-node-icon" />
                </template>
                <el-icon v-else class="tree-node-icon default-icon"><Menu /></el-icon>
              </div>
              <span class="title-text">{{ data.meta?.title || '(제목 없음)' }}</span>
              <span v-if="data.component" class="component-path">
                {{ formatComponentPath(data.component) }}
              </span>
            </div>
            <div class="node-actions">
              <el-tooltip content="하위 메뉴 추가" placement="top">
                <el-button v-permission="'create'" type="primary" link @click.stop="$emit('create-sub', data)">
                  <el-icon><Plus /></el-icon>
                </el-button>
              </el-tooltip>
              <el-tooltip content="메뉴 삭제" placement="top">
                <el-button v-permission="'delete'" type="danger" link @click.stop="$emit('delete', data)">
                  <el-icon><Delete /></el-icon>
                </el-button>
              </el-tooltip>
            </div>
          </div>
        </template>
      </el-tree>
    </div>
  </el-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { Menu, Plus, Delete } from '@element-plus/icons-vue';

defineProps<{
  searchQuery: string;
  filteredMenuList: any[];
  expandedKeys: any[];
  elementIconList: string[];
  elementIconComponents: any;
}>();

const emit = defineEmits([
  'update:searchQuery',
  'update:expandedKeys',
  'create-root',
  'create-sub',
  'refresh',
  'node-click',
  'node-drop',
  'node-expand',
  'node-collapse',
  'delete'
]);

const menuTreeRef = ref<any>(null);

const formatComponentPath = (component: string) => {
  if (!component) return '';
  return component.replace(/^(@\/views\/|src\/views\/|views\/)/, '');
};

const expandAll = () => {
  if (!menuTreeRef.value) return;
  const nodes = menuTreeRef.value.store._getAllNodes();
  const keys: any[] = [];
  nodes.forEach((node: any) => {
    node.expanded = true;
    keys.push(node.data.id);
  });
  emit('update:expandedKeys', keys);
};

const collapseAll = () => {
  if (!menuTreeRef.value) return;
  const nodes = menuTreeRef.value.store._getAllNodes();
  nodes.forEach((node: any) => {
    node.expanded = false;
  });
  emit('update:expandedKeys', []);
};
</script>

<style lang="scss" scoped>
.menu-tree-card {
  height: 100%;
  display: flex;
  flex-direction: column;
  border: none;
  background-color: transparent;

  :deep(.el-card__header) {
    padding: 1rem 1.25rem;
    border-bottom: 1px solid #f0f0f0;
  }

  :deep(.el-card__body) {
    flex: 1;
    overflow: hidden;
    padding: 1.25rem;
    display: flex;
    flex-direction: column;
  }
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;

  .left-actions {
    display: flex;
    align-items: center;
    span {
      font-size: 1rem;
      font-weight: 600;
      color: #333;
    }
  }

  .right-actions {
    display: flex;
    gap: 0.5rem;
  }
}

.tip-box {
  background-color: #fdf6ec;
  border-left: 4px solid #e6a23c;
  padding: 0.75rem 1rem;
  font-size: 0.875rem;
  color: #8a6d3b;
  display: flex;
  align-items: center;
  margin-bottom: 1.25rem;
  border-radius: 4px;

  i {
    font-size: 1.1rem;
    color: #e6a23c;
    margin-right: 8px;
  }
}

.tree-container {
  overflow-y: auto;
  padding-right: 0.5rem;

  &::-webkit-scrollbar {
    width: 6px;
  }
  &::-webkit-scrollbar-thumb {
    background: #e0e0e0;
    border-radius: 3px;
  }
}

.modern-menu-tree {
  background: transparent;

  :deep(.el-tree-node__content) {
    height: 48px;
    border-radius: 6px;
    margin-bottom: 2px;
    transition: all 0.2s ease;

    &:hover {
      background-color: #f5f7fa;
      .node-actions {
        opacity: 1;
      }
    }
  }

  :deep(.el-tree-node.is-current > .el-tree-node__content) {
    background-color: #ecf5ff;
    color: #409eff;
    font-weight: 600;

    .icon-wrapper {
      background-color: #409eff;
      color: #fff;
    }

    .component-path {
      color: #409eff;
      opacity: 0.8;
    }
  }
}

.custom-tree-node {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-size: 0.9375rem;
  padding-right: 8px;
  min-width: 0;

  .node-label {
    display: flex;
    align-items: center;
    gap: 10px;
    min-width: 0;
    flex: 1;

    .icon-wrapper {
      width: 32px;
      height: 32px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 6px;
      background-color: #f0f2f5;
      color: #606266;
      transition: all 0.2s ease;
      flex-shrink: 0;

      .tree-node-icon {
        font-size: 1rem;
      }

      .default-icon {
        opacity: 0.5;
      }
    }

    .title-text {
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .component-path {
      font-size: 0.8125rem;
      color: #909399;
      background-color: #f4f4f5;
      padding: 1px 6px;
      border-radius: 4px;
      margin-left: 8px;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      max-width: 200px;
    }
  }

  .node-actions {
    display: flex;
    gap: 4px;
    opacity: 0;
    transition: opacity 0.2s ease;
    flex-shrink: 0;

    .el-button {
      padding: 4px;
      height: auto;

      .el-icon {
        font-size: 1.1rem;
      }
    }
  }
}
</style>

