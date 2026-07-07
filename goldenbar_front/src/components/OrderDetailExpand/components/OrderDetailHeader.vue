<template>
<div class="expand-header-luxury">
    <div class="title-with-actions">
      <h4 class="expand-title-text">{{ title }}</h4>
      <el-button
        v-if="isStatementVisible"
        type="info"
        size="small"
        class="statement-btn-header"
        plain
        @click="$emit('show-statement')"
      >{{ $t('admin.settlement.statement.title') }}</el-button>
    </div>

    <div v-if="order.status === 'Cancelled' || order.status === 'SETTLED_CANCELLED'" class="cancellation-header-info">
      <div v-if="cancelHistory" class="cancel-meta-row">
        <span class="cancel-badge" :class="cancelHistory.companyType === 'RTL' ? 'retailer' : 'logistics'">
          {{ cancelHistory.companyType === 'RTL' ? '주문자 취소' : '물류사 취소' }}
        </span>
        <span class="cancel-meta">
          <span class="label-mini">취소자:</span> <strong>{{ cancelHistory.userDisplayName }}</strong> ({{ cancelHistory.companyName }})
        </span>
        <span class="cancel-date">
          <span class="label-mini">취소일시:</span> {{ formatTime(cancelHistory.createdAt) }}
        </span>
        <span class="cancel-reason">
          <span class="label-mini">취소메모:</span> <span class="reason-val">{{ extractCancellationReason(cancelHistory.remarks) || order.cancellationReason || '사유 없음' }}</span>
        </span>
      </div>
      <div v-else class="cancel-meta-row">
        <span class="cancel-badge">주문 취소</span>
        <span class="cancel-meta">정보 없음</span>
        <span class="cancel-reason">
          <span class="label-mini">취소메모:</span> <span class="reason-val">{{ order.cancellationReason || '사유 없음' }}</span>
        </span>
      </div>
    </div>

    <div v-else-if="order.deliveryDate" class="delivery-date-header">
      <el-icon><Calendar /></el-icon>
      <span class="label">납기일:</span>
      <span class="val">{{ order.deliveryDate.substring(0, 10) }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { Calendar } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import { isStatementVisibleStatus } from '@/utils/order';

const props = defineProps({
  title: {
    type: String,
    required: true
  },
  order: {
    type: Object,
    required: true
  }
});

defineEmits(['show-statement']);

const isStatementVisible = computed(() => {
  return props.order ? isStatementVisibleStatus(props.order.status) : false;
});

const cancelHistory = computed(() => {
  if (!props.order) return null;
  const history = props.order.statusHistory || props.order.history;
  if (!history) return null;
  return history.find((h: any) => h.status === 'Cancelled' || h.status === 'SETTLED_CANCELLED');
});

const formatTime = (time: string) => {
  return parseTime(time, '{y}-{m}-{d} {h}:{i}');
};

const extractMemo = (remarks: string, tag: string) => {
  if (!remarks) return '';
  const parts = remarks.split(tag);
  if (parts.length > 1) {
    return parts[1].split('[')[0].trim();
  }
  return remarks;
};

const extractCancellationReason = (remarks: string) => {
  return extractMemo(remarks, '[취소 사유]');
};
</script>

<style lang="scss" scoped>
.expand-header-luxury {
  margin-bottom: 1.5625rem;
  border-bottom: 1px solid #f7f5f0;
  padding-bottom: 0.9375rem;
  display: flex;
  justify-content: space-between;
  align-items: center;

  .title-with-actions {
    display: flex;
    align-items: center;
    gap: 0.95rem;
  }

  .expand-title-text {
    font-size: 0.875rem;
    font-weight: 700;

    text-transform: uppercase;
    letter-spacing: 2px;
    margin: 0;
    position: relative;
    padding-left: 0.9375rem;
    &::before {
      content: '';
      position: absolute;
      left: 0;
      top: 50%;
      transform: translateY(-50%);
      width: 4px;
      height: 14px;
      background-color: #c5a880;
    }
  }

  .cancellation-header-info {
    display: flex;
    align-items: center;
    gap: 0.95rem;
    background-color: #fffafa;
    padding: 0.375rem 1rem;
    border-radius: 4px;
    border: 1px solid #ffebeb;

    .cancel-meta-row {
      display: flex;
      align-items: center;
      gap: 0.95rem;
      flex-wrap: wrap;
      font-size: 0.95rem;
    }

    .cancel-badge {
      font-size: 0.825rem;
      font-weight: 800;
      padding: 0.125rem 0.375rem;
      border-radius: 2px;
      color: #fff;
      &.retailer { background-color: #ff4949; }
      &.logistics { background-color: #222222; }
    }

    .cancel-meta {
      color: #444;
      strong { color: #222; }
    }

    .cancel-date {
      color: #666;
    }

    .cancel-reason {
      color: #d73a3a;
      font-weight: 500;

      .reason-val {
        font-weight: 600;
      }
    }

    .label-mini {
      font-size: 0.825rem;
      color: #bbbbbb;
      font-weight: 700;
      margin-right: 0.25rem;
      text-transform: uppercase;
    }
  }

  .delivery-date-header {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    background-color: #fcfaf7;
    padding: 0.375rem 0.95rem;
    border-radius: 4px;
    border: 1px solid #efebe5;

    .el-icon {
      color: #c5a880;
      font-size: 1rem;
    }

    .label {
      font-size: 0.95rem;
      color: #888888;
      font-weight: 500;
    }

    .val {
      font-size: 0.9rem;
      color: #409EFF;
      font-weight: 700;
      letter-spacing: 0.5px;
    }
  }

  @media (max-width: 768px) {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.625rem;

    .title-with-actions {
      width: 100%;
      justify-content: space-between;

      .expand-title-text {
        font-size: 0.8125rem;
      }

      .statement-btn-header {
        margin-left: auto;
      }
    }

    .cancellation-header-info {
      width: 100%;
      border-radius: 2px;

      .cancel-meta-row {
        gap: 0.5rem;
        font-size: 0.8125rem;
      }
    }

    .delivery-date-header {
      width: 100%;
      border-radius: 2px;
      padding: 0.5rem 0.75rem;
      justify-content: flex-start;
    }
  }
}
</style>

