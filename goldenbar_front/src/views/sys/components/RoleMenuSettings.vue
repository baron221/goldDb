<template>
<el-card shadow="never" class="menu-card no-border" :class="{ 'mobile-mode': isMobile }">

    <template #header v-if="!isMobile">
      <div class="card-header mini-header">
        <span>메뉴 권한 상세 설정</span>
        <div class="header-actions" style="display: flex; gap: 0.375rem; align-items: center;">
          <el-button type="default" circle size="small" :icon="Expand" @click.stop="expandAll" title="전체 확장" />
          <el-button type="default" circle size="small" :icon="Fold" @click.stop="collapseAll" title="전체 축소" />
          <el-button type="primary" circle size="small" :icon="Select" @click.stop="handleManualSave" title="전체 저장" />
        </div>
      </div>
    </template>

    <div v-if="isMobile" style="display: flex; justify-content: flex-end; gap: 0.5rem; margin-bottom: 0.5rem; padding: 0.5rem;">
      <el-button type="primary" size="small" :icon="Select" @click="handleManualSave">저장</el-button>
    </div>

    <div class="detail-content-wrapper" :style="!isMobile ? '' : 'padding: 0.5rem;'">
      <base-table
        ref="menuTable"
        :key="menuTableKey"
        :data="menuData"
        row-key="id"
        highlight-current-row
        :expand-row-keys="expandedRowKeys"
        @expand-change="handleExpandChange"
        @row-click="handleMenuRowClick"
        :tree-props="{ children: 'children' }"
        :style="!isMobile ? 'width: 100%; height: calc(100vh - 350px);' : 'width: 100%; height: 350px;'"
        size="small"
        :height="!isMobile ? 'calc(100vh - 350px)' : 350"
      >

        <el-table-column prop="title" label="메뉴명" :min-width="!isMobile ? 180 : 150" :fixed="(!isMobile) ? 'left' : false">
          <template #header v-if="!isMobile">
            <div style="display: flex; align-items: center; gap: 0.3125rem;">
              <el-checkbox @change="toggleMasterAll($event)" />
              <span>메뉴명</span>
            </div>
          </template>
        </el-table-column>

        <el-table-column width="55" align="center" label="전체" prop="isAssigned">
          <template #header v-if="!isMobile">
            <el-checkbox @change="toggleAllRows('isAssigned', $event)" />
            <div class="header-label">전체</div>
          </template>
          <template #default="scope">
            <el-checkbox
              v-if="editingMenuId === scope.row.id || isMobile"
              v-model="scope.row.isAssigned"
              @change="handleMenuPermissionChange(scope.row)"
            />
            <el-icon v-else-if="scope.row.isAssigned" color="#67C23A"><Select /></el-icon>
            <span v-else>-</span>
          </template>
        </el-table-column>

        <template v-for="col in standardPermissions" :key="col.prop">
          <el-table-column
            v-if="!col.desktopOnly || !isMobile"
            :width="!isMobile ? 50 : 45"
            align="center"
            :label="col.label"
            :prop="col.prop"
          >
            <template #header v-if="!isMobile">
              <el-checkbox @change="toggleAllRows(col.prop, $event)" />
              <div class="header-label">{{ col.label }}</div>
            </template>
            <template #default="scope">
              <el-checkbox
                v-if="editingMenuId === scope.row.id || isMobile"
                v-model="scope.row[col.prop]"
                :disabled="!scope.row.isAssigned"
                @change="handleMenuPermissionChange(scope.row)"
              />
              <el-icon v-else-if="scope.row[col.prop]" :color="col.color"><Check /></el-icon>
              <span v-else>-</span>
            </template>
          </el-table-column>
        </template>

        <template v-if="!isMobile">
          <el-table-column v-for="i in 8" :key="i" width="45" align="center" :label="'C'+i" :prop="'custom'+i">
            <template #header>
              <el-checkbox @change="toggleAllRows(`custom${i}`, $event)" />
              <div class="header-label">C{{ i }}</div>
            </template>
            <template #default="scope">
              <el-checkbox
                v-if="editingMenuId === scope.row.id"
                v-model="scope.row[`custom${i}`]"
                :disabled="!scope.row.isAssigned"
                @change="handleMenuPermissionChange(scope.row)"
              />
              <el-icon v-else-if="scope.row[`custom${i}`]" color="#E6A23C"><Check /></el-icon>
              <span v-else>-</span>
            </template>
          </el-table-column>
        </template>
      </base-table>
    </div>
  </el-card>
</template>

<script setup>
import { ref, watch } from 'vue';
import { useMobile } from '@/hooks/useMobile';
import { getRoleMenus, assignMenus } from '@/api/role';
import { ElMessage, ElIcon, ElCheckbox, ElButton } from 'element-plus';
import { Expand, Fold, Select, Check } from '@element-plus/icons-vue';
import { hasActionPermission } from '@/utils/permission';
import BaseTable from '@/components/BaseTable/index.vue';

const props = defineProps({
  roleKey: {
    type: String,
    required: true
  }
});

const { isMobile } = useMobile();

const standardPermissions = [
  { prop: 'canSearch', label: '조회', color: '#409EFF' },
  { prop: 'canCreate', label: '추가', color: '#409EFF' },
  { prop: 'canDelete', label: '삭제', color: '#409EFF' },
  { prop: 'canSave', label: '저장', color: '#409EFF', desktopOnly: true },
  { prop: 'canPrint', label: '출력', color: '#409EFF', desktopOnly: true }
];

const menuData = ref([]);
const expandedRowKeys = ref([]);
const menuTable = ref(null);
const menuTableKey = ref(0);
const editingMenuId = ref(null);

let saveTimer = null;

const loadRoleMenus = async () => {
  try {
    const res = await getRoleMenus(props.roleKey);
    menuData.value = buildMenuTree(res.data);

    expandedRowKeys.value = res.data.map(m => String(m.id));
  } catch (error) {
    ElMessage.error('메뉴 데이터를 불러오는 데 실패했습니다.');
  }
};

watch(() => props.roleKey, (newKey) => {
  if (newKey) {
    editingMenuId.value = null;
    loadRoleMenus();
  } else {
    menuData.value = [];
  }
}, { immediate: true });

const buildMenuTree = (menus) => {
  const map = {};
  const roots = [];
  menus.forEach(m => {
    map[m.id] = { ...m, children: [] };
  });
  menus.forEach(m => {
    if (m.parentId) {
      if (map[m.parentId]) {
        map[m.parentId].children.push(map[m.id]);
      }
    } else {
      roots.push(map[m.id]);
    }
  });

  Object.values(map).forEach(node => {
    if (node.children && node.children.length > 0) {
      node.children.sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0));
    }
  });

  roots.sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0));
  return roots;
};

const handleExpandChange = (row, expanded) => {
  const idStr = String(row.id);
  if (expanded) {
    if (!expandedRowKeys.value.includes(idStr)) {
      expandedRowKeys.value.push(idStr);
    }
  } else {
    expandedRowKeys.value = expandedRowKeys.value.filter(id => id !== idStr);
  }
};

const handleMenuRowClick = (row) => {
  editingMenuId.value = row.id;
};

const expandAll = () => {
  if (!menuData.value.length) return;

  const ids = [];
  const traverse = (list) => {
    list.forEach(item => {
      if (item.children && item.children.length > 0) {
        ids.push(String(item.id));
        traverse(item.children);
      }
    });
  };
  traverse(menuData.value);

  expandedRowKeys.value = ids;
  menuTableKey.value++;
};

const collapseAll = () => {
  expandedRowKeys.value = [];
  menuTableKey.value++;
};

const toggleMasterAll = (value) => {
  const fields = ['isAssigned', 'canSearch', 'canCreate', 'canDelete', 'canSave', 'canPrint'];
  for (let i = 1; i <= 8; i++) fields.push(`custom${i}`);

  const traverse = (list) => {
    list.forEach(item => {
      fields.forEach(field => {
        item[field] = value;
      });
      if (item.children) traverse(item.children);
    });
  };
  traverse(menuData.value);
  handleMenuPermissionChange({});
};

const toggleAllRows = (field, value) => {
  const traverse = (list) => {
    list.forEach(item => {
      item[field] = value;

      if (field === 'isAssigned') {
        if (value) {
          item.canSearch = true;
          item.canCreate = true;
          item.canDelete = true;
          item.canSave = true;
          item.canPrint = true;
        } else {
          item.canSearch = false;
          item.canCreate = false;
          item.canDelete = false;
          item.canSave = false;
          item.canPrint = false;
          for (let i = 1; i <= 8; i++) item[`custom${i}`] = false;
        }
      } else if (value) {
        item.isAssigned = true;
      }

      if (item.children) traverse(item.children);
    });
  };
  traverse(menuData.value);
  handleMenuPermissionChange({});
};

const flattenMenuPermissions = (list) => {
  const flattened = [];
  const traverse = (items) => {
    items.forEach(item => {
      if (item.isAssigned) {
        flattened.push({
          menuId: item.id,
          canSearch: !!item.canSearch, canCreate: !!item.canCreate, canDelete: !!item.canDelete,
          canSave: !!item.canSave, canPrint: !!item.canPrint,
          custom1: !!item.custom1, custom2: !!item.custom2, custom3: !!item.custom3,
          custom4: !!item.custom4, custom5: !!item.custom5, custom6: !!item.custom6,
          custom7: !!item.custom7, custom8: !!item.custom8
        });
      }
      if (item.children) traverse(item.children);
    });
  };
  traverse(list);
  return flattened;
};

const handleManualSave = async () => {
  if (!props.roleKey) return;
  try {
    await assignMenus({ roleKey: props.roleKey, permissions: flattenMenuPermissions(menuData.value) });
    ElMessage.success('메뉴 권한이 성공적으로 저장되었습니다');
  } catch (error) {
    ElMessage.error('메뉴 권한 저장에 실패했습니다');
  }
};

const handleMenuPermissionChange = (row) => {
  if (!hasActionPermission('save')) {
    ElMessage.error('저장 권한이 없습니다');
    loadRoleMenus(); 
    return;
  }

  if (row.id && !row.isAssigned) {
    row.canSearch = false;
    row.canCreate = false;
    row.canDelete = false;
    row.canSave = false;
    row.canPrint = false;
    for (let i = 1; i <= 8; i++) row[`custom${i}`] = false;
  } else if (row.id) {
    if (!row.canSearch && !row.canCreate && !row.canDelete && !row.canSave && !row.canPrint) {
      row.canSearch = true;
      row.canCreate = true;
      row.canDelete = true;
      row.canSave = true;
      row.canPrint = true;
    }
  }

  if (saveTimer) {
    clearTimeout(saveTimer);
  }

  saveTimer = setTimeout(async () => {
    try {
      await assignMenus({ roleKey: props.roleKey, permissions: flattenMenuPermissions(menuData.value) });
      ElMessage.success('권한 설정이 반영되었습니다');
    } catch (error) {
      ElMessage.error('권한 설정 반영에 실패했습니다');
      loadRoleMenus();
    }
  }, 500);
};

defineExpose({
  loadRoleMenus,
  handleManualSave
});
</script>

<style scoped src="./RoleMenuSettingsStyles.scss"></style>

