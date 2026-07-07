<template>
<div class="order-detail-luxury">
    <order-detail-header
      :title="title"
      :order="order"
      @show-statement="handleShowStatement"
    />

    <order-detail-table
      :order="order"
      :display-code-map="displayCodeMap"
      :is-mobile="isMobile"
      :is-post-pending="isPostPending"
      :user-store="userStore"
      @go-to-detail="goToDetail"
    />

    <order-detail-memos
      :order="order"
      :display-code-map="displayCodeMap"
      :user-store="userStore"
    />

    <transaction-statement-dialog
      v-model="statementDialogVisible"
      :order="order"
      :code-map="codeMap"
      :statement="currentStatement"
    />
  </div>
</template>

<script setup lang="ts">

import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import useUserStore from '@/store/modules/user';
import useCodeStore from '@/store/modules/code';
import { isPostPendingStatus } from '@/utils/order';
import TransactionStatementDialog from '@/components/TransactionStatementDialog/index.vue';
import { getOrderStatement } from '@/api/order';
import { useMobile } from '@/hooks/useMobile';

import OrderDetailHeader from './components/OrderDetailHeader.vue';
import OrderDetailTable from './components/OrderDetailTable.vue';
import OrderDetailMemos from './components/OrderDetailMemos.vue';

const props = defineProps({
  title: {
    type: String,
    default: '의뢰 상세 내역'
  },
  order: {
    type: Object,
    required: true
  },
  codeMap: {
    type: Object,
    default: null
  }
});

const router = useRouter();
const userStore = useUserStore();
const codeStore = useCodeStore();

const statementDialogVisible = ref(false);
const currentStatement = ref<any>(null);

const displayCodeMap = computed(() => props.codeMap || codeStore.codeMap);
const { isMobile } = useMobile();

const isPostPending = computed(() => {
  return props.order ? isPostPendingStatus(props.order.status) : false;
});

const goToDetail = (row: any) => {
  if (row.productSetId) {
    router.push(`/prd/product-set-detail/${row.productSetId}`);
  } else if (row.productId) {
    router.push(`/prd/product-detail/${row.productId}`);
  }
};

const handleShowStatement = async () => {
  currentStatement.value = null;
  if (props.order?.hasStatement) {
    try {
      const response: any = await getOrderStatement(props.order.id);
      if (response.data) {
        currentStatement.value = response.data;
      }
    } catch (error) {
      console.error('Failed to fetch stored statement:', error);
    }
  }
  statementDialogVisible.value = true;
};

onMounted(() => {
  codeStore.fetchCodes();
});
</script>

<style lang="scss" scoped>
@import url('https://fonts.googleapis.com/css2?family=Jost:wght@300;400;500;600;700&display=swap');

.order-detail-luxury {
  font-family: 'S-CoreDream', 'Jost', sans-serif;
  background-color: #ffffff;
  padding: 1.875rem;
  border-radius: 2px;
  border: 1px solid #eae6df;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.04);
}

@media (max-width: 768px) {
  .order-detail-luxury { padding: 0.9375rem; }
}
</style>

