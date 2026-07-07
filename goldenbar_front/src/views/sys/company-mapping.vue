<template>
<div class="app-container">
    <el-row :gutter="20">

      <el-col :span="6">
        <el-card shadow="never">
          <template #header>
            <div class="card-header">
              <span>물류센터 목록</span>
            </div>
          </template>

          <base-table
            v-loading="centerLoading"
            :data="centerList"
            style="width: 100%"
            highlight-current-row
            @row-click="handleCenterClick"
          >
            <el-table-column prop="name" label="물류센터명" />
            <el-table-column prop="retailerCount" label="업체수" width="70" align="center">
              <template #default="scope">
                <el-tag size="small" type="info">{{ scope.row.retailerCount }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column
              label="지역"
              width="80"
              align="center"
              :excel-formatter="(row) => getRegionName(row.region)"
            >
              <template #default="scope">
                {{ getRegionName(scope.row.region) }}
              </template>
            </el-table-column>
          </base-table>

        </el-card>
      </el-col>

      <el-col :span="18">
        <el-card v-if="selectedCenter" shadow="never">
          <template #header>
            <div class="card-header">
              <span>
                매핑 관리: <el-tag type="warning">{{ selectedCenter.name }}</el-tag>
              </span>
              <el-button size="small" @click="fetchMappingData(selectedCenter.id)">새로고침</el-button>
            </div>
          </template>

          <div class="mapping-container">
            <el-row :gutter="10" style="width: 100%;">

              <el-col :span="11">
                <div class="table-title">현재 센터 소매업체</div>
                <el-input
                  v-model="assignedFilter"
                  placeholder="업체명 검색"
                  size="small"
                  style="margin-bottom: 0.625rem;"
                  clearable
                />
                <base-table
                  v-loading="mappingLoading"
                  :data="filteredAssigned"
                  height="500"
                  size="small"
                  @selection-change="handleAssignedSelection"
                >
                  <el-table-column type="selection" width="40" />
                  <el-table-column prop="name" label="업체명" />
                  <el-table-column
                    label="지역"
                    width="80"
                    align="center"
                    :excel-formatter="(row) => getRegionName(row.region)"
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
                  :disabled="assignedSelection.length === 0 || mappingLoading"
                  @click="moveToUnassigned"
                />
                <el-button
                  type="primary"
                  :icon="ArrowLeft"
                  circle
                  style="margin-left: 0; margin-top: 0.625rem;"
                  :disabled="unassignedSelection.length === 0 || mappingLoading"
                  @click="moveToAssigned"
                />
              </el-col>

              <el-col :span="11">
                <div class="table-title">미할당 소매업체</div>
                <div style="display: flex; gap: 0.3125rem; margin-bottom: 0.625rem;">
                  <el-input
                    v-model="unassignedFilter"
                    placeholder="업체명 검색"
                    size="small"
                    style="flex: 2;"
                    clearable
                  />
                  <el-select
                    v-model="unassignedRegionFilter"
                    placeholder="지역 전체"
                    size="small"
                    style="flex: 1;"
                    clearable
                  >
                    <el-option
                      v-for="item in regionCodes"
                      :key="item.code"
                      :label="item.name"
                      :value="item.code"
                    />
                  </el-select>
                </div>
                <base-table
                  v-loading="mappingLoading"
                  :data="filteredUnassigned"
                  height="500"
                  size="small"
                  @selection-change="handleUnassignedSelection"
                >
                  <el-table-column type="selection" width="40" />
                  <el-table-column prop="name" label="업체명" />
                  <el-table-column
                    label="지역"
                    width="80"
                    align="center"
                    :excel-formatter="(row) => getRegionName(row.region)"
                  >
                    <template #default="scope">
                      {{ getRegionName(scope.row.region) }}
                    </template>
                  </el-table-column>
                </base-table>
              </el-col>
            </el-row>
          </div>

          <div style="margin-top: 1.25rem;">
            <el-alert
              title="도움말"
              type="success"
              description="테이블 간 업체 이동 시 즉시 서버에 저장됩니다. (왼쪽: 소속 업체, 오른쪽: 미소속 업체)"
              show-icon
              :closable="false"
            />
          </div>
        </el-card>

        <el-card v-else shadow="never" class="empty-card">
          <div class="empty-state">
            <el-icon :size="50" color="#909399"><InfoFilled /></el-icon>
            <p>좌측 목록에서 물류센터를 선택하여 매핑을 관리해 주세요.</p>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { getCompanies, getRetailersByCenter, getUnassignedRetailers, assignRetailersToCenter } from '@/api/company';
import { ElMessage } from 'element-plus';
import { InfoFilled, ArrowRight, ArrowLeft } from '@element-plus/icons-vue';
import useCodeStore from '@/store/modules/code';

const codeStore = useCodeStore();
const centerList = ref([]);
const centerLoading = ref(false);
const mappingLoading = ref(false);
const selectedCenter = ref(null);

const assignedRetailers = ref([]);
const unassignedRetailers = ref([]);
const regionCodes = ref([]);

const unassignedFilter = ref('');
const unassignedRegionFilter = ref(null);
const assignedFilter = ref('');

const unassignedSelection = ref([]);
const assignedSelection = ref([]);

onMounted(() => {
  fetchRegionCodes();
  fetchCenters();
});

const fetchRegionCodes = async () => {
  try {
    await codeStore.fetchCodes();
    regionCodes.value = codeStore.getCodesByGroupStore('REGION');
  } catch (error) {
    console.error('Failed to fetch region codes:', error);
  }
};

const getRegionName = (code) => {
  const codeObj = regionCodes.value.find(c => c.code === code);
  return codeObj ? codeObj.name : code;
};

const fetchCenters = async () => {
  centerLoading.value = true;
  try {
    const res = await getCompanies({ category: 'DCC', page: 1, pageSize: 1000 });
    centerList.value = res.data.items;
  } catch (error) {
    ElMessage.error('물류센터 목록 로드 실패');
  } finally {
    centerLoading.value = false;
  }
};

const handleCenterClick = async (row) => {
  selectedCenter.value = row;
  fetchMappingData(row.id);
};

const fetchMappingData = async (centerId) => {
  mappingLoading.value = true;
  try {
    const [assignedRes, unassignedRes] = await Promise.all([
      getRetailersByCenter(centerId),
      getUnassignedRetailers()
    ]);

    assignedRetailers.value = assignedRes.data;
    unassignedRetailers.value = unassignedRes.data;
  } catch (error) {
    ElMessage.error('매핑 정보 로드 실패');
  } finally {
    mappingLoading.value = false;
  }
};

const filteredUnassigned = computed(() => {
  return unassignedRetailers.value.filter(r => {
    const matchName = !unassignedFilter.value || r.name.toLowerCase().includes(unassignedFilter.value.toLowerCase());
    const matchRegion = !unassignedRegionFilter.value || r.region === unassignedRegionFilter.value;
    return matchName && matchRegion;
  });
});

const filteredAssigned = computed(() => {
  return assignedRetailers.value.filter(r =>
    !assignedFilter.value || r.name.toLowerCase().includes(assignedFilter.value.toLowerCase())
  );
});

const handleUnassignedSelection = (val) => {
  unassignedSelection.value = val;
};

const handleAssignedSelection = (val) => {
  assignedSelection.value = val;
};

const moveToAssigned = async () => {
  if (!selectedCenter.value) return;

  const moveIds = unassignedSelection.value.map(s => s.id);
  const newAssignedIds = [...assignedRetailers.value.map(r => r.id), ...moveIds];

  mappingLoading.value = true;
  try {
    await assignRetailersToCenter(selectedCenter.value.id, newAssignedIds);
    ElMessage.success('매핑이 추가되었습니다');
    fetchMappingData(selectedCenter.value.id);
    fetchCenters(); 
  } catch (error) {
    ElMessage.error('저장 실패');
  } finally {
    mappingLoading.value = false;
  }
};

const moveToUnassigned = async () => {
  if (!selectedCenter.value) return;

  const removeIds = assignedSelection.value.map(s => s.id);
  const newAssignedIds = assignedRetailers.value
    .map(r => r.id)
    .filter(id => !removeIds.includes(id));

  mappingLoading.value = true;
  try {
    await assignRetailersToCenter(selectedCenter.value.id, newAssignedIds);
    ElMessage.success('매핑이 해제되었습니다');
    fetchMappingData(selectedCenter.value.id);
    fetchCenters(); 
  } catch (error) {
    ElMessage.error('저장 실패');
  } finally {
    mappingLoading.value = false;
  }
};
</script>

<style scoped>
.app-container {
  padding: 1.25rem;
}
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.empty-card {
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 500px;
}
.empty-state {
  text-align: center;
  color: #909399;
}
.mapping-container {
  padding: 0.625rem 0;
}
.table-title {
  font-weight: bold;
  margin-bottom: 0.625rem;
  color: #606266;
  text-align: center;
}
.action-buttons {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
}
</style>

