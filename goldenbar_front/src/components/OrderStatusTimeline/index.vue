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
  return props.order ? ['Cancelled', 'SETTLED_CANCELLED', 'CLOSED_BY_AGREEMENT', 'FactoryRejected'].includes(props.order.status) : false;
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
  if (status === 'Cancelled' || status === 'SETTLED_CANCELLED' || status === 'CLOSED_BY_AGREEMENT' || status === 'FactoryRejected') {
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
@import "./OrderStatusTimelineStyles.scss";
</style>
