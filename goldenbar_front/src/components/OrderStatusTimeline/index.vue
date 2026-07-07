<template>
<div v-if="order" class="timeline-container-luxury" :class="getContainerClass()">
    <el-steps
      :active="getActiveStep(order.status, flow)"
      finish-status="success"
      align-center
      class="luxury-steps"
      :process-status="getProcessStatus(order.status)"
    >
      <el-step v-for="(step, index) in flow" :key="index">
        <template #icon>
          <div class="step-icon-content">
            <i v-if="index === flow.length - 1" :class="getFinalIcon()" class="f-icon"></i>
            <span v-else class="step-num">{{ index + 1 }}</span>
            <span class="step-status-name">{{ getStepName(step) }}</span>
          </div>
        </template>
        <template #title>
          <div class="step-label-wrapper">
            <div v-if="getStepDate(order, step)" class="step-date-indicator">
              <span class="d">{{ getStepDateOnly(order, step) }}</span>
              <span class="t">{{ getStepTimeOnly(order, step) }}</span>
            </div>
          </div>
        </template>
      </el-step>
    </el-steps>

    <div v-if="isCompleted" class="closure-badge">CLOSED</div>
    <div v-if="isTerminated" class="closure-badge terminated">CANCELLED</div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { parseTime } from '@/utils';
import { getOrderStatusFlow } from '@/utils/order';

const props = defineProps({
  order: {
    type: Object,
    required: true
  },
  userType: {
    type: String,
    default: 'retailer'
  }
});

const isTerminated = computed(() => {
  return props.order ? ['Cancelled', 'SETTLED_CANCELLED', 'CLOSED_BY_AGREEMENT'].includes(props.order.status) : false;
});

const isCompleted = computed(() => props.order ? props.order.status === 'Completed' : false);

const flow = computed(() => {
  if (!props.order) return [];
  const rawFlow = getOrderStatusFlow(props.order.status, props.userType);
  const filteredFlow = rawFlow.filter(step => step.name !== '취소됨');

  if (isTerminated.value) {
    const currentIndex = filteredFlow.findIndex(step =>
      step.codes && step.codes.includes(props.order!.status)
    );
    if (currentIndex !== -1) {
      return filteredFlow.slice(0, currentIndex + 1);
    }
  }

  return filteredFlow;
});

const getContainerClass = () => {
  return {
    'is-order-completed': isCompleted.value,
    'is-order-terminated': isTerminated.value
  };
};

const getFinalIcon = () => {
  if (isCompleted.value) return 'fas fa-check-double';
  if (isTerminated.value) return 'fas fa-circle-xmark';
  return 'fas fa-flag-checkered';
};

const getActiveStep = (status: string, flowItems: any[]) => {
  const currentStepIndex = flowItems.findIndex(step => {
    if (step.codes) {
      return step.codes.includes(status);
    }
    return step.code === status;
  });

  if (currentStepIndex !== -1) {
    return currentStepIndex;
  }

  if (props.order.history && props.order.history.length > 0) {
    const lastHistoryStatus = props.order.history[props.order.history.length - 1].status;
    const lastIndex = flowItems.findIndex(step => {
      if (step.codes) return step.codes.includes(lastHistoryStatus);
      return step.code === lastHistoryStatus;
    });
    if (lastIndex !== -1) return lastIndex;
  }

  return 0;
};

const getProcessStatus = (status: string) => {
  if (status === 'Cancelled' || status === 'SETTLED_CANCELLED' || status === 'CLOSED_BY_AGREEMENT') {
    return 'error';
  }
  return 'process';
};

const getStepHistory = (order: any, step: any) => {
  if (!order.history) return null;

  if (step.codes) {
    const historyItems = order.history.filter((h: any) => step.codes.includes(h.status));
    if (historyItems.length === 0) return null;
    return historyItems.sort((a: any, b: any) =>
      new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    )[0];
  }

  return order.history.find((h: any) => h.status === step.code);
};

const getStepName = (step: any) => {
  if (step.codes && step.codes.includes('Cancelled')) {
    const history = getStepHistory(props.order, step);
    if (history) {
      if (history.companyType === 'RTL') return '주문자 취소';
      if (history.companyType === 'LOG') return '물류사 취소';
    }
  }

  return step.name;
};

const getStepDate = (order: any, step: any) => {
  const history = getStepHistory(order, step);
  return history ? history.createdAt : null;
};

const getStepDateOnly = (order: any, step: any) => {
  const date = getStepDate(order, step);
  return date ? parseTime(date, '{m}-{d}') : null;
};

const getStepTimeOnly = (order: any, step: any) => {
  const date = getStepDate(order, step);
  return date ? parseTime(date, '{h}:{i}') : null;
};
</script>

<style lang="scss" scoped>
.timeline-container-luxury {
  padding: 1.875rem 1.25rem 1.25rem;
  background-color: #ffffff;
  border-bottom: 1px solid #f7f5f0;
  position: relative;
  overflow: hidden;

  .luxury-steps {
    :deep(.el-step__icon) {
      width: auto !important;
      height: 32px !important;
      background: transparent !important;
      border: none !important;
    }

    :deep(.el-step__line) {
      top: 15px !important;
      background-color: #f0eeeb !important;
    }

    :deep(.el-step__head.is-success) {
      .el-step__line { background-color: #c5a880 !important; }
    }

    .step-icon-content {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.125rem;
      min-width: 60px;
      background: #fff;
      padding: 0 0.3125rem;
      z-index: 1;

      .step-num, .f-icon {
        font-size: 0.825rem;
        font-weight: 800;
        color: #bbb;
        width: 18px;
        height: 18px;
        border: 1px solid #eee;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        background: #fbfaf7;
      }

      .f-icon { color: #c5a880; border-color: #c5a880; }

      .step-status-name {
        font-size: 0.825rem;
        font-weight: 700;
        color: #999;
        white-space: nowrap;
        letter-spacing: -0.5px;
      }
    }

    :deep(.el-step__head.is-process) {
      .step-num { color: #222; border-color: #222; background: #fff; }
      .step-status-name { color: #222; }
    }

    :deep(.el-step__head.is-success) {
      .step-num { color: #c5a880; border-color: #c5a880; background: #fbfaf7; }
      .step-status-name { color: #c5a880; }
    }

    :deep(.el-step__head.is-error) {
      .step-num { color: #ff4949; border-color: #ff4949; }
      .step-status-name { color: #ff4949; }
    }

    .step-label-wrapper {
      display: flex;
      flex-direction: column;
      align-items: center;
      margin-top: 0.3125rem;
    }

    .step-date-indicator {
      display: flex;
      align-items: center;
      gap: 0.25rem;
      line-height: 1;
      .d { font-size: 0.825rem; font-weight: 700; color: #444; }
      .t { font-size: 0.825rem; color: #bbbbbb; font-family: monospace; }
    }
  }

  .closure-badge {
    position: absolute;
    top: 10px;
    right: 15px;
    background-color: #c5a880;
    color: #ffffff;
    font-size: 0.825rem;
    font-weight: 900;
    padding: 0.125rem 0.5rem;
    border-radius: 0;
    letter-spacing: 1px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    z-index: 10;

    &.terminated {
      background-color: #ff4949;
    }
  }
}

.is-order-completed {
  background-color: #fbfaf7;
  &::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 3px;
    height: 100%;
    background-color: #c5a880;
  }
}

.is-order-terminated {
  background-color: #fff9f9;
  &::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 3px;
    height: 100%;
    background-color: #ff4949;
  }
}

@media (max-width: 991px) {
  .timeline-container-luxury { padding: 1.875rem 0.625rem 0.9375rem; }
  .luxury-steps {
    :deep(.el-step__icon) { transform: scale(0.9); }
  }
}
</style>

