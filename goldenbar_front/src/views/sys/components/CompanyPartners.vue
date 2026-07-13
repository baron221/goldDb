<template>
<div class="company-partners">
    <template v-if="category === 'DCC'">
      <div class="partner-group">
        <div class="group-title">{{ $t('sys.company.linkedManufacturers') }}</div>
        <partner-transfer-list
          :assigned="assignedManufacturers"
          :unassigned="unassignedManufacturers"
          :loading="mfgLoading"
          @move-to-assigned="moveManufacturersToAssigned"
          @move-to-unassigned="moveManufacturersToUnassigned"
        />
      </div>
      <div class="partner-group">
        <div class="group-title">{{ $t('sys.company.linkedRetailers') }}</div>
        <partner-transfer-list
          :assigned="assignedRetailers"
          :unassigned="unassignedRetailers"
          :loading="rtlLoading"
          @move-to-assigned="moveRetailersToAssigned"
          @move-to-unassigned="moveRetailersToUnassigned"
        />
      </div>
    </template>

    <template v-else-if="category === 'MFG'">
      <div class="partner-group">
        <div class="group-title">{{ $t('sys.company.linkedLogisticsCenters') }}</div>
        <partner-transfer-list
          :assigned="assignedLogistics"
          :unassigned="unassignedLogistics"
          :loading="logisticsLoading"
          @move-to-assigned="moveLogisticsToAssigned"
          @move-to-unassigned="moveLogisticsToUnassigned"
        />
      </div>
    </template>

    <template v-else-if="category === 'RTL'">
      <div class="partner-group">
        <div class="group-title">{{ $t('sys.company.linkedLogisticsCenter') }}</div>
        <el-empty v-if="!retailerLogisticsName" :description="$t('sys.company.noLogisticsAssigned')" />
        <div v-else class="single-partner-card">
          <el-icon><OfficeBuilding /></el-icon>
          <span>{{ retailerLogisticsName }}</span>
        </div>
        <p class="hint">{{ $t('sys.company.retailerMappingHint') }}</p>
      </div>
    </template>

    <el-empty v-else :description="$t('common.noData')" />
  </div>
</template>

<script setup>
import { ref, watch } from 'vue';
import { OfficeBuilding } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import { useI18n } from 'vue-i18n';
import {
  getManufacturersByCenter, getUnassignedManufacturers, assignManufacturersToCenter,
  getRetailersByCenter, getUnassignedRetailers, assignRetailersToCenter,
  getLogisticsCentersByManufacturer, getUnassignedLogisticsCentersForManufacturer, assignLogisticsCentersToManufacturer,
  getCompany
} from '@/api/company';
import PartnerTransferList from './PartnerTransferList.vue';

const props = defineProps({
  companyId: {
    type: Number,
    default: null
  },
  category: {
    type: String,
    default: null
  }
});

const { t } = useI18n();

const assignedManufacturers = ref([]);
const unassignedManufacturers = ref([]);
const mfgLoading = ref(false);

const assignedRetailers = ref([]);
const unassignedRetailers = ref([]);
const rtlLoading = ref(false);

const assignedLogistics = ref([]);
const unassignedLogistics = ref([]);
const logisticsLoading = ref(false);

const retailerLogisticsName = ref('');

const fetchDccData = async () => {
  mfgLoading.value = true;
  rtlLoading.value = true;
  try {
    const [mfgAssignedRes, mfgUnassignedRes] = await Promise.all([
      getManufacturersByCenter(props.companyId),
      getUnassignedManufacturers(props.companyId)
    ]);
    assignedManufacturers.value = mfgAssignedRes.data;
    unassignedManufacturers.value = mfgUnassignedRes.data;

    const [rtlAssignedRes, rtlUnassignedRes] = await Promise.all([
      getRetailersByCenter(props.companyId),
      getUnassignedRetailers()
    ]);
    assignedRetailers.value = rtlAssignedRes.data;
    unassignedRetailers.value = rtlUnassignedRes.data;
  } catch (error) {
    ElMessage.error(t('sys.company.loadPartnersFail'));
  } finally {
    mfgLoading.value = false;
    rtlLoading.value = false;
  }
};

const fetchMfgData = async () => {
  logisticsLoading.value = true;
  try {
    const [assignedRes, unassignedRes] = await Promise.all([
      getLogisticsCentersByManufacturer(props.companyId),
      getUnassignedLogisticsCentersForManufacturer(props.companyId)
    ]);
    assignedLogistics.value = assignedRes.data;
    unassignedLogistics.value = unassignedRes.data;
  } catch (error) {
    ElMessage.error(t('sys.company.loadPartnersFail'));
  } finally {
    logisticsLoading.value = false;
  }
};

const fetchRtlData = async () => {
  try {
    const res = await getCompany(props.companyId);
    retailerLogisticsName.value = res.data.logisticsCompanyName || '';
  } catch (error) {
    ElMessage.error(t('sys.company.loadPartnersFail'));
  }
};

const fetchData = () => {
  if (!props.companyId || !props.category) return;
  if (props.category === 'DCC') fetchDccData();
  else if (props.category === 'MFG') fetchMfgData();
  else if (props.category === 'RTL') fetchRtlData();
};

watch(() => [props.companyId, props.category], fetchData, { immediate: true });

const moveManufacturersToAssigned = async (ids) => {
  const newIds = [...assignedManufacturers.value.map(m => m.id), ...ids];
  await assignManufacturersToCenter(props.companyId, newIds);
  ElMessage.success(t('sys.company.mappingUpdated'));
  fetchDccData();
};

const moveManufacturersToUnassigned = async (ids) => {
  const newIds = assignedManufacturers.value.map(m => m.id).filter(id => !ids.includes(id));
  await assignManufacturersToCenter(props.companyId, newIds);
  ElMessage.success(t('sys.company.mappingUpdated'));
  fetchDccData();
};

const moveRetailersToAssigned = async (ids) => {
  const newIds = [...assignedRetailers.value.map(r => r.id), ...ids];
  await assignRetailersToCenter(props.companyId, newIds);
  ElMessage.success(t('sys.company.mappingUpdated'));
  fetchDccData();
};

const moveRetailersToUnassigned = async (ids) => {
  const newIds = assignedRetailers.value.map(r => r.id).filter(id => !ids.includes(id));
  await assignRetailersToCenter(props.companyId, newIds);
  ElMessage.success(t('sys.company.mappingUpdated'));
  fetchDccData();
};

const moveLogisticsToAssigned = async (ids) => {
  const newIds = [...assignedLogistics.value.map(l => l.id), ...ids];
  await assignLogisticsCentersToManufacturer(props.companyId, newIds);
  ElMessage.success(t('sys.company.mappingUpdated'));
  fetchMfgData();
};

const moveLogisticsToUnassigned = async (ids) => {
  const newIds = assignedLogistics.value.map(l => l.id).filter(id => !ids.includes(id));
  await assignLogisticsCentersToManufacturer(props.companyId, newIds);
  ElMessage.success(t('sys.company.mappingUpdated'));
  fetchMfgData();
};
</script>

<style scoped>
.company-partners {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}
.partner-group .group-title {
  font-weight: bold;
  margin-bottom: 0.625rem;
  color: #606266;
}
.single-partner-card {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1rem;
  border: 1px solid #e4e7ed;
  border-radius: 2px;
  background: #f9f9f9;
  width: fit-content;
}
.hint {
  margin-top: 0.5rem;
  font-size: 0.8rem;
  color: #909399;
}
</style>
