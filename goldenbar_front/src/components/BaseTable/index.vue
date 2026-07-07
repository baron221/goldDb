<template>
  <div class="base-table-container">
    <el-table
      ref="tableRef"
      v-loading="loading"
      border
      fit
      highlight-current-row
      v-bind="$attrs"
      @row-contextmenu="handleRowContextMenu"
    >

      <template v-for="(_, name) in nonDefaultSlots" :key="name" #[name]="data">
        <slot :name="name" v-bind="data || {}" />
      </template>

      <RenderColumns />
    </el-table>

    <div class="pagination-container-luxury" v-if="showFooter && (total !== undefined || displayCount > 0)">
      <div class="pagination-side-spacer" v-if="total !== undefined"></div>

      <div class="pagination-wrapper" v-if="total !== undefined">
        <el-pagination
          :current-page="page"
          :page-size="pageSize"
          :total="total"
          :page-sizes="pageSizes"
          :layout="effectiveLayout"
          :small="isMobile"
          @update:current-page="handlePageChange"
          @update:page-size="handlePageSizeChange"
        />
      </div>
      <div class="pagination-main-spacer" v-if="total === undefined"></div>

      <div class="row-count-info" :class="{ 'no-pagination': total === undefined }">
        <span class="count-item">
          <span class="count-value">{{ displayCount }}</span>
          / <span class="count-value">{{ total !== undefined ? total : displayCount }}</span>
        </span>
      </div>
    </div>

    <div
      v-if="contextMenuVisible"
      class="custom-context-menu"
      :style="{ top: contextMenuPosition.y + 'px', left: contextMenuPosition.x + 'px' }"
    >
      <div class="menu-item" @click="handleExportRowExcel">
        <el-icon><Download /></el-icon>
        <span>엑셀 내보내기 (데이터 전체)</span>
      </div>
      <div class="menu-item" @click="handleExportDomExcel">
        <el-icon><Download /></el-icon>
        <span>엑셀 내보내기 (화면 그대로)</span>
      </div>

      <slot name="contextmenu" :row="contextMenuRow" :close="closeContextMenu" />
    </div>
  </div>
</template>

<script setup lang="ts">

import { ref, useSlots, useAttrs, computed, cloneVNode, defineComponent } from 'vue';
import { useMobile } from '@/hooks/useMobile';
import { ElTableColumn } from 'element-plus';
import { Download } from '@element-plus/icons-vue';
import { useTableExcel } from './hooks/useTableExcel';

const props = defineProps({
  loading: {
    type: Boolean,
    default: false
  },
  total: {
    type: Number,
    default: undefined
  },
  page: {
    type: Number,
    default: 1
  },
  pageSize: {
    type: Number,
    default: 20
  },
  pageSizes: {
    type: Array as () => number[],
    default: () => [10, 20, 50, 100]
  },
  layout: {
    type: String,
    default: 'total, sizes, prev, pager, next, jumper'
  },
  showFooter: {
    type: Boolean,
    default: true
  }
});

const { isMobile } = useMobile();

const effectiveLayout = computed(() =>
  isMobile.value ? 'total, prev, pager, next' : props.layout
);

const emit = defineEmits(['update:page', 'update:pageSize', 'change', 'row-contextmenu']);

const tableRef = ref();
const slots = useSlots();
const attrs = useAttrs();

const displayCount = computed(() => {
  return (attrs.data as any[])?.length || 0;
});

const contextMenuVisible = ref(false);
const contextMenuPosition = ref({ x: 0, y: 0 });
const contextMenuRow = ref<any>(null);

const { exportDomExcel, exportExcel } = useTableExcel(tableRef, slots, attrs);

const handleRowContextMenu = (row: any, column: any, event: MouseEvent) => {
  event.preventDefault();
  contextMenuRow.value = row;
  contextMenuPosition.value = {
    x: event.clientX,
    y: event.clientY
  };
  contextMenuVisible.value = true;
  document.addEventListener('click', closeContextMenu);
};

const closeContextMenu = () => {
  contextMenuVisible.value = false;
  document.removeEventListener('click', closeContextMenu);
};

const handleExportRowExcel = () => {
  exportExcel();
  closeContextMenu();
};

const handleExportDomExcel = () => {
  exportDomExcel();
  closeContextMenu();
};

const nonDefaultSlots = computed(() => {
  const allSlots = { ...slots };
  delete allSlots.default;
  return allSlots;
});

const modifyVNode = (vnode: any): any => {
  if (!vnode) return vnode;

  if (vnode.type === Symbol.for('v-fgt') || vnode.type?.toString() === 'Symbol(v-fgt)' || vnode.type === 'Fragment') {
    if (Array.isArray(vnode.children)) {
      return cloneVNode(vnode, {
        children: vnode.children.map((child: any) => modifyVNode(child))
      });
    }
  }

  const type = vnode.type;

  const isTableColumn =
    type === ElTableColumn ||
    type?.name === 'ElTableColumn' ||
    type?.__name === 'ElTableColumn' ||
    type?.name === 'el-table-column' ||
    (typeof type === 'object' && type !== null && (type.name === 'ElTableColumn' || type.__name === 'ElTableColumn'));

  if (isTableColumn) {
    const props = vnode.props || {};
    const label = props.label || '';
    const newProps: any = {};

    const trimmedLabel = label.trim();
    const isNonSortableColumn = !trimmedLabel || ['관리', '액션', '작업', '선택', '사진', '대표사진', '더보기'].includes(trimmedLabel);

    if (props.sortable === undefined) {
      newProps.sortable = !isNonSortableColumn;
    }

    if (props['header-align'] === undefined && props.headerAlign === undefined) {
      newProps['header-align'] = 'center';
    }

    if (Object.keys(newProps).length > 0) {
      return cloneVNode(vnode, newProps);
    }
  }

  return vnode;
};

const RenderColumns = defineComponent({
  setup() {
    return () => {

      const defaultSlot = slots.default ? slots.default() : [];
      return defaultSlot.map(vnode => modifyVNode(vnode));
    };
  }
});

const handlePageChange = (val: number) => {
  emit('update:page', val);
  emit('change'); 
};

const handlePageSizeChange = (val: number) => {
  emit('update:pageSize', val);
  emit('update:page', 1); 
  emit('change');
};

defineExpose({
  tableRef,
  exportExcel,
  exportDomExcel,
  clearSelection: () => tableRef.value?.clearSelection(),
  getSelectionRows: () => tableRef.value?.getSelectionRows(),
  toggleRowSelection: (row: any, selected?: boolean) => tableRef.value?.toggleRowSelection(row, selected),
  toggleAllSelection: () => tableRef.value?.toggleAllSelection(),
  toggleRowExpansion: (row: any, expanded?: boolean) => tableRef.value?.toggleRowExpansion(row, expanded),
  setCurrentRow: (row: any) => tableRef.value?.setCurrentRow(row),
  clearSort: () => tableRef.value?.clearSort(),
  clearFilter: (columnKeys?: string[]) => tableRef.value?.clearFilter(columnKeys),
  doLayout: () => tableRef.value?.doLayout(),
  sort: (prop: string, order: string) => tableRef.value?.sort(prop, order)
});
</script>

<style scoped lang="scss">
@import "./BaseTableStyles.scss";
</style>
