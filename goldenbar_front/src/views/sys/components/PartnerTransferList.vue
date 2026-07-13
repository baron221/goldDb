<template>
<el-row :gutter="10" class="transfer-list" v-loading="loading">
    <el-col :span="11">
      <div class="table-title">{{ $t('sys.company.assigned') }}</div>
      <base-table
        :data="assigned"
        height="260"
        size="small"
        @selection-change="(val) => assignedSelection = val"
      >
        <el-table-column type="selection" width="40" />
        <el-table-column prop="name" :label="$t('sys.company.labels.name')" />
        <el-table-column
          :label="$t('sys.company.labels.region')"
          width="80"
          align="center"
        >
          <template #default="scope">
            {{ getRegionName(scope.row.region) }}
          </template>
        </el-table-column>
      </base-table>
    </el-col>

    <el-col :span="2" class="action-buttons">
      <el-button
        type="primary"
        :icon="ArrowRight"
        circle
        :disabled="unassignedSelection.length === 0"
        @click="onMoveToAssigned"
      />
      <el-button
        type="primary"
        :icon="ArrowLeft"
        circle
        style="margin-left: 0; margin-top: 0.625rem;"
        :disabled="assignedSelection.length === 0"
        @click="onMoveToUnassigned"
      />
    </el-col>

    <el-col :span="11">
      <div class="table-title">{{ $t('sys.company.unassigned') }}</div>
      <base-table
        :data="unassigned"
        height="260"
        size="small"
        @selection-change="(val) => unassignedSelection = val"
      >
        <el-table-column type="selection" width="40" />
        <el-table-column prop="name" :label="$t('sys.company.labels.name')" />
        <el-table-column
          :label="$t('sys.company.labels.region')"
          width="80"
          align="center"
        >
          <template #default="scope">
            {{ getRegionName(scope.row.region) }}
          </template>
        </el-table-column>
      </base-table>
    </el-col>
  </el-row>
</template>

<script setup>
import { ref } from 'vue';
import { ArrowRight, ArrowLeft } from '@element-plus/icons-vue';
import BaseTable from '@/components/BaseTable/index.vue';
import useCodeStore from '@/store/modules/code';

defineProps({
  assigned: {
    type: Array,
    default: () => []
  },
  unassigned: {
    type: Array,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits(['move-to-assigned', 'move-to-unassigned']);

const codeStore = useCodeStore();
const assignedSelection = ref([]);
const unassignedSelection = ref([]);

const getRegionName = (code) => {
  const codeObj = codeStore.getCodesByGroupStore('REGION').find(c => c.code === code);
  return codeObj ? codeObj.name : code;
};

const onMoveToAssigned = () => {
  emit('move-to-assigned', unassignedSelection.value.map(s => s.id));
};

const onMoveToUnassigned = () => {
  emit('move-to-unassigned', assignedSelection.value.map(s => s.id));
};
</script>

<style scoped>
.transfer-list {
  padding: 0.3rem 0;
}
.table-title {
  font-weight: 600;
  margin-bottom: 0.5rem;
  color: #606266;
  text-align: center;
  font-size: 0.85rem;
}
.action-buttons {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
}
</style>
