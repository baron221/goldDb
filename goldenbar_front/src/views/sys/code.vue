<template>
<div class="app-container">
    <splitpanes class="default-theme code-split-container">

      <pane size="40" min-size="20">
        <el-card shadow="never" class="code-tree-card">
          <template #header>
            <div class="card-header">
              <div class="left-actions">
                <span>공통 코드</span>
                <el-input
                  v-model="searchQuery"
                  placeholder="검색..."
                  size="small"
                  style="width: 150px; margin-left: 0.625rem;"
                  clearable
                />
              </div>
              <div class="right-actions">
                <el-button v-permission="'create'" type="primary" size="small" @click="handleCreateRoot">그룹 추가</el-button>
                <el-button :icon="Refresh" size="small" @click="getList">새로고침</el-button>
              </div>
            </div>
          </template>

          <base-table
            ref="codeTable"
            :data="filteredCodeList"
            row-key="id"
            border
            :expand-row-keys="expandedRowKeys"
            highlight-current-row
            @row-click="handleRowClick"
            @expand-change="handleExpandChange"
            :tree-props="{ children: 'children' }"
            style="width: 100%"
            height="100%"
          >
            <el-table-column
              label="코드 명칭"
              min-width="180"
              :excel-formatter="(row: any) => (row.parentId ? '' : '[G] ') + row.name + ' (' + row.code + ')'"
            >
              <template #default="scope">
                <el-tag v-if="!scope.row.parentId" size="small" type="info" style="margin-right: 0.3125rem;">G</el-tag>
                <span>{{ scope.row.name }}</span>
                <span style="color: #909399; font-size: 0.95rem; margin-left: 0.3125rem;">({{ scope.row.code }})</span>
              </template>
            </el-table-column>
            <el-table-column align="center" label="작업" width="100">
              <template #default="scope">
                <el-button v-permission="'create'" link class="add-icon-btn" size="small" :icon="Plus" @click.stop="handleCreateSub(scope.row)" />
                <el-button  v-permission="'delete'" link class="delete-icon-btn" :icon="Delete" @click.stop="handleDelete(scope.row)" />

              </template>
            </el-table-column>
          </base-table>
        </el-card>
      </pane>

      <pane size="60" min-size="30">
        <el-card v-if="currentCode" shadow="never" class="detail-card">
          <template #header>
            <div class="card-header">
              <span>상세 설정: <el-tag>{{ currentCode.name || '신규 코드' }}</el-tag></span>
              <el-button v-permission="'save'" type="success" size="small" @click="handleSave">최종 저장</el-button>
            </div>
          </template>

          <el-tabs v-model="activeDetailTab">

            <el-tab-pane label="기본 정보" name="basic">
              <el-form :model="temp" label-width="100px" style="margin-top: 1.25rem;">
                <el-form-item label="상위 코드">
                  <el-select v-model="temp.parentId" placeholder="그룹 코드 (없으면 루트)" style="width: 100%" filterable clearable>
                    <el-option :key="0" label="루트 (그룹 코드)" value="" />
                    <el-option v-for="item in flatCodeList" :key="item.id" :label="item.name" :value="item.id" />
                  </el-select>
                </el-form-item>
                <el-row :gutter="20">
                  <el-col :span="12">
                    <el-form-item label="코드">
                      <el-input v-model="temp.code" placeholder="예: ROLE_USER" :disabled="temp.id !== undefined" />
                    </el-form-item>
                  </el-col>
                  <el-col :span="12">
                    <el-form-item label="코드 명칭">
                      <el-input v-model="temp.name" placeholder="예: 일반 사용자" />
                    </el-form-item>
                  </el-col>
                </el-row>
                <el-form-item label="코드 설명">
                  <el-input v-model="temp.description" type="textarea" :rows="2" />
                </el-form-item>
                <el-row :gutter="20">
                  <el-col :span="12">
                    <el-form-item label="정렬 순서">
                      <el-input-number v-model="temp.sortOrder" :min="0" />
                    </el-form-item>
                  </el-col>
                  <el-col :span="12">
                    <el-form-item label="사용 여부">
                      <el-switch v-model="temp.isActive" />
                    </el-form-item>
                  </el-col>
                </el-row>
              </el-form>
            </el-tab-pane>

            <el-tab-pane label="추가 정보 (Custom)" name="custom">
              <el-form :model="temp" label-width="100px" style="margin-top: 1.25rem;">
                <el-form-item label="Custom 1">
                  <el-input v-model="temp.custom1" />
                </el-form-item>
                <el-form-item label="Custom 2">
                  <el-input v-model="temp.custom2" />
                </el-form-item>
                <el-form-item label="Custom 3">
                  <el-input v-model="temp.custom3" />
                </el-form-item>
                <el-form-item label="Custom 4">
                  <el-input v-model="temp.custom4" />
                </el-form-item>
              </el-form>
            </el-tab-pane>
          </el-tabs>
        </el-card>

        <el-card v-else shadow="never" class="empty-card">
          <div class="empty-state">
            <el-icon :size="50" color="#909399"><InfoFilled /></el-icon>
            <p>목록에서 코드를 선택하거나 새 코드를 추가해 주세요.</p>
          </div>
        </el-card>
      </pane>
    </splitpanes>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, reactive, computed } from 'vue';
import { getCodes, saveCode, deleteCode } from '@/api/code';
import { ElMessage, ElMessageBox } from 'element-plus';
import { InfoFilled, Refresh, Edit, Delete, Plus } from '@element-plus/icons-vue';
import { hasActionPermission } from '@/utils/permission';
import BaseTable from '@/components/BaseTable/index.vue';
import { Splitpanes, Pane } from 'splitpanes';
import 'splitpanes/dist/splitpanes.css';

const codeTable = ref<any>(null);
const codeList = ref([]);
const flatCodeList = ref<any[]>([]);
const expandedRowKeys = ref<any[]>([]);
const isFirstLoad = ref(true);
const listLoading = ref(false);
const currentCode = ref<any>(null);
const searchQuery = ref('');
const activeDetailTab = ref('basic');

const temp = reactive<any>({
  id: undefined,
  parentId: null,
  code: '',
  name: '',
  description: '',
  sortOrder: 0,
  isActive: true,
  custom1: '',
  custom2: '',
  custom3: '',
  custom4: ''
});

const handleExpandChange = (row: any, expanded: boolean) => {
  if (expanded) {
    if (!expandedRowKeys.value.includes(row.id)) {
      expandedRowKeys.value.push(row.id);
    }
  } else {
    const index = expandedRowKeys.value.indexOf(row.id);
    if (index > -1) {
      expandedRowKeys.value.splice(index, 1);
    }
  }
};

const filteredCodeList = computed(() => {
  if (!searchQuery.value) return codeList.value;
  const filter = (list: any[]): any[] => {
    return list.filter(item => {
      const match = item.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                    item.code.toLowerCase().includes(searchQuery.value.toLowerCase());
      if (item.children && item.children.length > 0) {

        const filteredChildren = filter(item.children);
        if (filteredChildren.length > 0) {
          item.children = filteredChildren;
          return true;
        }
      }
      return match;
    });
  };

  return filter(JSON.parse(JSON.stringify(codeList.value)));
});

const getList = async () => {
  listLoading.value = true;
  try {
    const res = await getCodes();
    codeList.value = res.data;

    const flat: any[] = [];

    const traverse = (list: any[]) => {
      list.forEach(item => {
        flat.push({ id: item.id, name: item.name, code: item.code });
        if (item.children) traverse(item.children);
      });
    };
    traverse(res.data);
    flatCodeList.value = flat;

    if (isFirstLoad.value) {
      expandedRowKeys.value = flat.map(item => item.id);
      isFirstLoad.value = false;
    }
  } finally {
    listLoading.value = false;
  }
};

const handleRowClick = (row: any) => {
  currentCode.value = row;
  Object.assign(temp, {
    id: row.id,
    parentId: row.parentId ?? '',
    code: row.code,
    name: row.name,
    description: row.description,
    sortOrder: row.sortOrder,
    isActive: row.isActive,
    custom1: row.custom1,
    custom2: row.custom2,
    custom3: row.custom3,
    custom4: row.custom4
  });
};

const handleCreateRoot = () => {
  currentCode.value = { id: 0, name: '신규 그룹 코드' };
  Object.assign(temp, {
    id: undefined,
    parentId: '',
    code: '',
    name: '',
    description: '',
    sortOrder: codeList.value.length + 1,
    isActive: true,
    custom1: '',
    custom2: '',
    custom3: '',
    custom4: ''
  });
  activeDetailTab.value = 'basic';
};

const handleCreateSub = (row: any) => {
  currentCode.value = { id: 0, name: '신규 상세 코드' };
  Object.assign(temp, {
    id: undefined,
    parentId: row.id,
    code: '',
    name: '',
    description: '',
    sortOrder: (row.children?.length || 0) + 1,
    isActive: true,
    custom1: '',
    custom2: '',
    custom3: '',
    custom4: ''
  });
  activeDetailTab.value = 'basic';
};

const handleSave = async () => {
  if (!hasActionPermission('save')) {
    ElMessage.error('저장 권한이 없습니다');
    return;
  }
  try {
    const payload = { ...temp };
    if (payload.parentId === '') {
      payload.parentId = null;
    }
    await saveCode(payload);
    ElMessage.success('저장되었습니다.');
    getList();
  } catch (error) {
    ElMessage.error('저장에 실패했습니다.');
  }
};

const handleDelete = (row: any) => {
  ElMessageBox.confirm('정말로 해당 코드를 삭제하시겠습니까?', '경고', {
    confirmButtonText: '확인',
    cancelButtonText: '취소',
    type: 'warning'
  }).then(async () => {
    await deleteCode(row.id);
    ElMessage.success('삭제되었습니다.');
    if (currentCode.value?.id === row.id) {
      currentCode.value = null;
    }
    getList();
  });
};

onMounted(() => {
  getList();
});
</script>

<style lang="scss" src="./CodeStyles.scss" scoped></style>

