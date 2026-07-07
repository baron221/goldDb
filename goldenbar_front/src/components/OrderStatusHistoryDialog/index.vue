<template>
<base-popup
    draggable
    v-model="visible"
    title="주문 상태 변경 이력"
    width="600px"
    @close="handleClosed"
  >
    <div v-loading="loading" class="history-timeline-container">
      <el-timeline>
        <el-timeline-item
          v-for="(activity, index) in historyData"
          :key="index"
          :timestamp="formatDate(activity.createdAt)"
          :type="index === 0 ? 'primary' : 'info'"
          placement="top"
        >
          <el-card shadow="never" class="history-card">
            <div class="history-header">
              <h4 class="status-title">{{ getStatusLabel(activity.status) }}</h4>
              <span class="user-info">
                <el-icon><User /></el-icon>
                {{ activity.userDisplayName || '시스템' }}
                <span v-if="activity.companyName" style="color: #909399; font-size: 0.95rem; margin-left: 0.25rem;">
                  ({{ activity.companyName }})
                </span>
              </span>
            </div>
            <p v-if="activity.remarks" class="remarks-text">
              <el-icon><InfoFilled /></el-icon>
              {{ formatRemarks(activity.remarks) }}
            </p>
          </el-card>
        </el-timeline-item>
      </el-timeline>
      <el-empty v-if="!loading && historyData.length === 0" description="이력 정보가 없습니다." />
    </div>
  </base-popup>
</template>

<script setup lang="ts">

import { ref, watch, onMounted } from 'vue';
import BasePopup from '@/components/BasePopup/index.vue';
import { getOrderHistory } from '@/api/order';
import useCodeStore from '@/store/modules/code';
import { parseTime } from '@/utils';
import { User, InfoFilled } from '@element-plus/icons-vue';

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  orderId: {
    type: [Number, String],
    default: null
  }
});

const emit = defineEmits(['update:modelValue', 'closed']);

const codeStore = useCodeStore();
const visible = ref(false);
const loading = ref(false);
const historyData = ref<any[]>([]);
const orderStatusCodes = ref<any[]>([]);

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val && props.orderId) {
    fetchHistory();
  }
});

watch(visible, (val) => {
  emit('update:modelValue', val);
});

const fetchHistory = async () => {
  if (!props.orderId) return;

  loading.value = true;
  try {
    const res = await getOrderHistory(Number(props.orderId));
    historyData.value = res.data || [];
  } catch (error) {
    console.error('Failed to fetch order history:', error);
  } finally {
    loading.value = false;
  }
};

const fetchStatusCodes = async () => {
  try {
    await codeStore.fetchCodes();
    orderStatusCodes.value = codeStore.getCodesByGroupStore('ORDERED_STATUS');
  } catch (error) {
    console.error('Failed to fetch status codes:', error);
  }
};

const getStatusLabel = (status: string) => {
  return codeStore.getCodeName(status);
};

const formatRemarks = (remarks: string) => {
  if (!remarks) return '';

  let formatted = remarks;

  orderStatusCodes.value.forEach(codeObj => {
    const regex = new RegExp(codeObj.code, 'g');
    formatted = formatted.replace(regex, codeObj.name);
  });

  return formatted;
};

const formatDate = (date: string) => {
  if (!date) return '';
  return parseTime(date, '{y}-{m}-{d} {h}:{i}');
};

const handleClosed = () => {
  emit('closed');
  historyData.value = [];
};

onMounted(() => {
  fetchStatusCodes();
});
</script>

<style lang="scss" scoped>
.history-timeline-container {
  padding: 0.625rem;
  max-height: 500px;
  overflow-y: auto;
}

.history-card {
  margin-bottom: 0.3125rem;
  border: 1px solid #f0f0f0;

  :deep(.el-card__body) {
    padding: 0.95rem 0.9375rem;
  }
}

.history-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;

  .status-title {
    margin: 0;
    font-size: 0.9375rem;
    font-weight: 600;
    color: #303133;
  }

  .user-info {
    font-size: 0.95rem;
    color: #909399;
    display: flex;
    align-items: center;
    gap: 0.25rem;
  }
}

.remarks-text {
  margin: 0;
  color: #606266;
  font-size: 0.9rem;
  background-color: #f8f9fb;
  padding: 0.5rem 0.625rem;
  border-radius: 2px;
  display: flex;
  align-items: flex-start;
  gap: 0.375rem;
  line-height: 1.4;
  white-space: pre-wrap;

  .el-icon {
    margin-top: 0.125rem;
    color: #409EFF;
  }
}
</style>

