<template>
<div class="retailer-management-section" style="margin-top: 1.25rem;">
    <base-table :data="assignedRetailers" border style="width: 100%">
      <el-table-column prop="name" :label="$t('sys.company.labels.name')" />
      <el-table-column prop="ceo" :label="$t('sys.company.labels.ceo')" width="120" />
      <el-table-column
        :label="$t('sys.company.labels.region')"
        width="100"
        align="center"
        prop="region"
        :excel-formatter="(row) => getRegionName(row.region)"
      >
        <template #default="scope">
          {{ getRegionName(scope.row.region) }}
        </template>
      </el-table-column>
    </base-table>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue';
import { getRetailersByCenter } from '@/api/company';
import BaseTable from '@/components/BaseTable/index.vue';

const props = defineProps({
  companyId: {
    type: Number,
    required: true
  },
  regionCodes: {
    type: Array,
    default: () => []
  }
});

const assignedRetailers = ref([]);

watch(() => props.companyId, (newId) => {
  if (newId) {
    fetchRetailers(newId);
  } else {
    assignedRetailers.value = [];
  }
}, { immediate: true });

const fetchRetailers = async (centerId) => {
  try {
    const res = await getRetailersByCenter(centerId);
    assignedRetailers.value = res.data;
  } catch (error) {
    console.error('소속 소매업체 로드 실패:', error);
  }
};

const getRegionName = (code) => {
  const codeObj = props.regionCodes.find(c => c.code === code);
  return codeObj ? codeObj.name : code;
};
</script>

