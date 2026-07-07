<template>
<div class="app-container">
    <splitpanes class="default-theme menu-split-container" @resized="handleResized">

      <pane size="40" min-size="20">
        <menu-tree-pane
          v-model:search-query="searchQuery"
          v-model:expanded-keys="expandedKeys"
          :filtered-menu-list="filteredMenuList"
          :element-icon-list="elementIconList"
          :element-icon-components="elementIconComponents"
          @create-root="handleCreateRoot"
          @create-sub="handleCreateSub"
          @refresh="getList"
          @node-click="handleNodeClick"
          @node-drop="handleNodeDrop"
          @node-expand="handleNodeExpand"
          @node-collapse="handleNodeCollapse"
          @delete="handleDelete"
        />
      </pane>

      <pane size="60" min-size="30">
        <menu-detail-pane
          :temp="temp"
          :current-menu="currentMenu"
          :flat-menu-list="flatMenuList"
          :element-icon-list="elementIconList"
          :element-icon-components="elementIconComponents"
          :svg-icon-list="svgIconList"
          @create-sibling="handleCreateSibling"
          @create-sub="handleCreateSub"
          @save="handleSave"
        />
      </pane>
    </splitpanes>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, reactive, computed, nextTick } from 'vue';
import { getMenus, saveMenu, deleteMenu, batchUpdateMenus } from '@/api/menu';
import { ElMessage, ElMessageBox } from 'element-plus';
import { hasActionPermission } from '@/utils/permission';
import svgIcons from '@/views/icons/svg-icons';
import elementIcons from '@/views/icons/element-icons';
import { Splitpanes, Pane } from 'splitpanes';
import 'splitpanes/dist/splitpanes.css';
import MenuTreePane from './components/MenuTreePane.vue';
import MenuDetailPane from './components/MenuDetailPane.vue';

const menuList = ref([]);
const flatMenuList = ref<any[]>([]);
const expandedKeys = ref<any[]>([]);
const isFirstLoad = ref(true);
const listLoading = ref(false);
const currentMenu = ref<any>(null);
const searchQuery = ref('');
const isUpdatingTemp = ref(false);

const svgIconList = svgIcons;
const elementIconList = Object.keys(elementIcons);
const elementIconComponents = elementIcons as any;

const handleResized = (event: any) => {

};

const temp = reactive<any>({
  id: undefined,
  parentId: null,
  path: '',
  component: 'Layout',
  name: '',
  redirect: '',
  title: '',
  icon: '',
  hidden: false,
  alwaysShow: false,
  affix: false,
  keepAlive: false,
  allowDuplicate: false,
  sortOrder: 0,
  isMobile: false
});

const filteredMenuList = computed(() => {
  if (!searchQuery.value) return menuList.value;
  const filterText = searchQuery.value.toLowerCase();

  const filterNodes = (nodes: any[]): any[] => {
    return nodes.reduce((acc: any[], node: any) => {
      const titleMatch = (node.meta?.title || '').toLowerCase().includes(filterText);
      const componentMatch = (node.component || '').toLowerCase().includes(filterText);
      const pathMatch = (node.path || '').toLowerCase().includes(filterText);
      const nameMatch = (node.name || '').toLowerCase().includes(filterText);

      const isMatch = titleMatch || componentMatch || pathMatch || nameMatch;
      const childrenMatch = node.children ? filterNodes(node.children) : [];

      if (isMatch || childrenMatch.length > 0) {
        acc.push({ ...node, children: childrenMatch.length > 0 ? childrenMatch : node.children });
      }
      return acc;
    }, []);
  };

  return filterNodes(menuList.value);
});

const handleNodeExpand = (data: any) => {
  if (!expandedKeys.value.includes(data.id)) {
    expandedKeys.value.push(data.id);
  }
};

const handleNodeCollapse = (data: any) => {
  const index = expandedKeys.value.indexOf(data.id);
  if (index > -1) {
    expandedKeys.value.splice(index, 1);
  }
};

const getList = async () => {
  listLoading.value = true;
  try {
    const res = await getMenus(true);
    menuList.value = res.data;

    const flat: any[] = [];

    const traverse = (list: any[]) => {
      list.forEach(item => {
        flat.push({ id: item.id, title: item.meta?.title || item.path });
        if (item.children) traverse(item.children);
      });
    };
    traverse(res.data);
    flatMenuList.value = flat;

    if (isFirstLoad.value) {
      expandedKeys.value = flat.map(item => item.id);
      isFirstLoad.value = false;
    }
  } finally {
    listLoading.value = false;
  }
};

const handleNodeClick = (row: any) => {
  isUpdatingTemp.value = true;
  currentMenu.value = row;
  Object.assign(temp, {
    id: row.id,
    parentId: row.parentId ?? '',
    path: row.component ? row.path : '',
    component: row.component || '',
    name: row.name || '',
    redirect: row.redirect || '',
    title: row.meta?.title || '',
    icon: row.meta?.icon || '',
    hidden: row.meta?.hidden || false,
    alwaysShow: row.meta?.alwaysShow || false,
    affix: row.meta?.affix || false,
    keepAlive: row.meta?.keepAlive || false,
    allowDuplicate: row.meta?.allowDuplicate || false,
    sortOrder: row.sortOrder || 0,
    isMobile: row.isMobile || false
  });
  nextTick(() => {
    isUpdatingTemp.value = false;
  });
};

const handleCreateRoot = () => {
  isUpdatingTemp.value = true;
  currentMenu.value = { id: 0, meta: { title: '새 루트 메뉴' }, path: 'new' };
  Object.assign(temp, {
    id: undefined,
    parentId: '',
    path: '',
    component: 'Layout',
    name: '',
    redirect: '',
    title: '',
    icon: '',
    hidden: false,
    alwaysShow: false,
    affix: false,
    keepAlive: false,
    allowDuplicate: false,
    sortOrder: menuList.value.length + 1,
    isMobile: false
  });
  nextTick(() => {
    isUpdatingTemp.value = false;
  });
};

const handleCreateSibling = (row: any) => {
  isUpdatingTemp.value = true;
  currentMenu.value = { id: 0, meta: { title: '새 형제 메뉴' }, path: 'new' };
  Object.assign(temp, {
    id: undefined,
    parentId: row.parentId ?? '',
    path: '',
    component: row.parentId ? '' : 'Layout',
    name: '',
    redirect: '',
    title: '',
    icon: '',
    hidden: false,
    alwaysShow: false,
    affix: false,
    keepAlive: false,
    allowDuplicate: false,
    sortOrder: row.sortOrder + 1,
    isMobile: false
  });
  nextTick(() => {
    isUpdatingTemp.value = false;
  });
};

const handleCreateSub = (row: any) => {
  isUpdatingTemp.value = true;
  currentMenu.value = { id: 0, meta: { title: '새 하위 메뉴' }, path: 'new' };
  Object.assign(temp, {
    id: undefined,
    parentId: row.id,
    path: '',
    component: '',
    name: '',
    redirect: '',
    title: '',
    icon: '',
    hidden: false,
    alwaysShow: false,
    affix: false,
    keepAlive: false,
    allowDuplicate: false,
    sortOrder: (row.children?.length || 0) + 1,
    isMobile: false
  });
  nextTick(() => {
    isUpdatingTemp.value = false;
  });
};

const handleSave = async (isAuto = false) => {
  if (!hasActionPermission('save')) {
    if (!isAuto) ElMessage.error('저장 권한이 없습니다');
    return;
  }
  try {
    const payload = { ...temp };
    if (payload.parentId === '') {
      payload.parentId = null;
    }
    await saveMenu(payload);
    if (!isAuto) ElMessage.success('저장되었습니다.');
    getList();
  } catch {
    if (!isAuto) ElMessage.error('저장에 실패했습니다.');
  }
};

const handleNodeDrop = async (draggingNode: any, dropNode: any, dropType: string) => {
  if (!hasActionPermission('save')) {
    ElMessage.error('순서 변경 권한이 없습니다');
    getList(); 
    return;
  }

  let parentId = null;
  let siblings = [];

  if (dropType === 'inner') {
    parentId = dropNode.data.id;
    siblings = dropNode.childNodes;
  } else {
    parentId = dropNode.parent.data ? dropNode.parent.data.id : null;
    siblings = dropNode.parent.childNodes;
  }

  const updateItems = siblings.map((node: any, index: number) => ({
    id: node.data.id,
    parentId: parentId,
    sortOrder: index,
    isMobile: node.data.isMobile
  }));

  try {
    await batchUpdateMenus({ items: updateItems });
    ElMessage.success('순서가 변경되었습니다.');
    getList();
  } catch {
    ElMessage.error('순서 변경 실패');
    getList();
  }
};

const handleDelete = (row: any) => {
  ElMessageBox.confirm('정말로 해당 메뉴와 하위 메뉴를 삭제하시겠습니까?', '경고', {
    confirmButtonText: '확인',
    cancelButtonText: '취소',
    type: 'warning'
  }).then(async () => {
    await deleteMenu(row.id);
    ElMessage.success('삭제되었습니다.');
    if (currentMenu.value?.id === row.id) {
      currentMenu.value = null;
    }
    getList();
  });
};

onMounted(() => {
  getList();
});
</script>

<style lang="scss" scoped src="./components/MenuStyles.scss"></style>

