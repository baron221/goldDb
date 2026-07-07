<template>
<base-popup
    v-model="visible"
    :title="$t('admin.stockExhaustion.title')"
    width="80%"
    style="max-width: 1000px;"
    draggable
    append-to-body
    @close="handleClose"
  >
    <div v-loading="loading">
      <base-table :data="orderItems" style="width: 100%">
        <el-table-column :label="$t('admin.stockExhaustion.headers.productInfo')" min-width="200" :excel-formatter="(row) => (row.productName || row.productSetTitle) + ' (' + row.productNo + ')'">
          <template #default="scope">
            <div class="product-info">
              <el-image :src="scope.row.photoUrl || defaultImage" class="product-thumb" fit="cover" />
              <div class="product-detail">
                <div class="product-name">{{ scope.row.productName || scope.row.productSetTitle }}</div>
                <div class="product-no">{{ scope.row.productNo }}</div>
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column :label="$t('admin.stockExhaustion.headers.purity')" width="80" align="center" prop="purity" />
        <el-table-column :label="$t('admin.stockExhaustion.headers.qty')" width="70" align="center" prop="quantity" />
        <el-table-column :label="$t('admin.stockExhaustion.headers.status')" width="120" align="center" :excel-formatter="(row) => row.isStockExhausted ? $t('admin.stockExhaustion.labels.completed') : $t('admin.stockExhaustion.labels.pending')">
          <template #default="scope">
            <el-tag v-if="scope.row.isStockExhausted" type="success">{{ $t('admin.stockExhaustion.labels.completed') }}</el-tag>
            <el-tag v-else type="info">{{ $t('admin.stockExhaustion.labels.pending') }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column :label="$t('admin.stockExhaustion.headers.linkedStock')" min-width="150" :excel-formatter="(row) => row.isStockExhausted ? row.stockNo : ''">
          <template #default="scope">
            <div v-if="scope.row.isStockExhausted" class="stock-info">
              <el-tag type="warning" size="small">{{ scope.row.stockNo }}</el-tag>
            </div>
            <div v-else-if="scope.row.productId">
              <el-button type="primary" size="small" @click="handleShowStock(scope.row)">{{ $t('admin.stockExhaustion.actions.findStock') }}</el-button>
            </div>
            <div v-else>-</div>
          </template>
        </el-table-column>
      </base-table>

      <base-popup v-model="stockSelectorVisible" :title="$t('admin.stockExhaustion.selector.title')" width="700px" append-to-body>
        <div v-if="currentItem" class="current-product-info" style="margin-bottom: 1.25rem; display: flex; align-items: center; gap: 0.9375rem; padding: 0.625rem; background-color: #f8f9fa; border-radius: 2px;">
          <el-image :src="currentItem.photoUrl || defaultImage" style="width: 50px; height: 50px; border-radius: 2px;" fit="cover" />
          <div>
            <div style="font-weight: bold;">{{ currentItem.productName || currentItem.productSetTitle }}</div>
            <div style="font-size: 0.95rem; color: #909399;">{{ currentItem.productNo }}</div>
          </div>
        </div>
        <div v-loading="stockLoading">
          <base-table :data="matchingStocks" style="width: 100%" @row-click="selectStock">
            <el-table-column prop="stockNo" :label="$t('admin.stockExhaustion.selector.headers.stockNo')" width="130" />
            <el-table-column prop="productNo" :label="$t('admin.stockExhaustion.selector.headers.productNo')" width="120" />
            <el-table-column prop="productName" :label="$t('admin.stockExhaustion.selector.headers.productName')" min-width="150" show-overflow-tooltip />
            <el-table-column :label="$t('admin.stockExhaustion.selector.headers.qty')" width="60" align="center" :excel-formatter="() => 1">
              <template #default>1</template>
            </el-table-column>
            <el-table-column prop="ownerCompanyName" :label="$t('admin.stockExhaustion.selector.headers.owner')" width="120" />
            <el-table-column :label="$t('admin.stockExhaustion.selector.headers.weight')" width="80" align="right" :excel-formatter="(row) => row.actualWeight + 'g'">
              <template #default="scope">{{ scope.row.actualWeight }}g</template>
            </el-table-column>
            <el-table-column prop="purity" :label="$t('admin.stockExhaustion.selector.headers.purity')" width="80" align="center" />
            <el-table-column prop="color" :label="$t('admin.stockExhaustion.selector.headers.color')" width="80" align="center" />
            <el-table-column :label="$t('admin.stockExhaustion.selector.headers.receiptDate')" width="100" align="center" :excel-formatter="(row) => formatDate(row.createdAt)">
              <template #default="scope">{{ formatDate(scope.row.createdAt) }}</template>
            </el-table-column>
            <el-table-column :label="$t('admin.stockExhaustion.selector.headers.select')" width="60" align="center">
              <template #default="scope">
                <el-button type="success" size="small" circle :icon="Check" @click="selectStock(scope.row)" />
              </template>
            </el-table-column>
          </base-table>
          <div v-if="matchingStocks.length === 0" style="padding: 1.875rem; text-align: center; color: #909399;">
            {{ $t('admin.stockExhaustion.selector.noData') }}
          </div>
        </div>
      </base-popup>
    </div>
    <template #footer>
      <el-button @click="visible = false">{{ $t('notice.close') }}</el-button>
    </template>
  </base-popup>
</template>

<script setup lang="ts">

import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { fetchStocks, exhaustStockItem } from '@/api/stock';
import { ElMessage, ElMessageBox } from 'element-plus';
import { Check } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import BaseTable from '@/components/BaseTable/index.vue';
import BasePopup from '@/components/BasePopup/index.vue';
import store from '@/store';

const { t } = useI18n();

const props = defineProps({
  modelValue: Boolean,
  order: {
    type: Object,
    default: () => null
  }
});

const emit = defineEmits(['update:modelValue', 'completed']);

const userStore = store.user();
const visible = ref(false);
const loading = ref(false);
const stockLoading = ref(false);
const orderItems = ref<any[]>([]);
const defaultImage = 'https://via.placeholder.com/100x100?text=No+Image';

const stockSelectorVisible = ref(false);
const currentItem = ref<any>(null);
const matchingStocks = ref<any[]>([]);

watch(() => props.modelValue, (val) => {

  visible.value = val;
  if (val && props.order) {

    initialize();
  }
}, { immediate: true });

watch(visible, (val) => {

  emit('update:modelValue', val);
});

const initialize = () => {

  if (!props.order) {
    console.warn('[OrderStockExhaustionDialog] no order provided');
    return;
  }
  if (!props.order.orderItems) {
    console.warn('[OrderStockExhaustionDialog] order has no orderItems');
    return;
  }

  const items: any[] = [];
  props.order.orderItems.forEach((item: any) => {
    if (item.children && item.children.length > 0) {

      item.children.forEach((child: any) => {
        items.push({ ...child, parentId: item.id });
      });
    } else {

      items.push(item);
    }
  });
  orderItems.value = items;

};

const handleClose = () => {
  visible.value = false;
  orderItems.value = [];
};

const handleShowStock = async (item: any) => {
  currentItem.value = item;
  stockSelectorVisible.value = true;
  stockLoading.value = true;
  try {
    const res = await fetchStocks({
      productId: item.productId,
      logisticsCompanyId: userStore.companyId,
      isDirectManagement: true,
      isExhausted: false,
      status: 'ACTIVE',
      pageSize: 100
    });
    matchingStocks.value = res.data.items;
  } catch (error) {
    console.error(error);
  } finally {
    stockLoading.value = false;
  }
};

const selectStock = (stock: any) => {
  ElMessageBox.confirm(t('admin.stockExhaustion.messages.confirm', { stockNo: stock.stockNo }), t('admin.stockExhaustion.messages.confirmTitle'), {
    confirmButtonText: t('common.ok'),
    cancelButtonText: t('common.cancel'),
    type: 'warning'
  }).then(async () => {
    try {
      await exhaustStockItem({
        orderItemId: currentItem.value.id,
        stockId: stock.id,
        memo: t('admin.stockExhaustion.messages.memo', { orderNo: props.order.orderNo })
      });
      ElMessage.success(t('admin.stockExhaustion.messages.success'));
      stockSelectorVisible.value = false;

      const idx = orderItems.value.findIndex(i => i.id === currentItem.value.id);
      if (idx !== -1) {
        orderItems.value[idx].isStockExhausted = true;
        orderItems.value[idx].stockNo = stock.stockNo;
      }

      emit('completed');
    } catch (error) {
      console.error(error);
    }
  });
};

const formatDate = (date: string) => {
  if (!date) return '-';
  return parseTime(new Date(date), '{y}-{m}-{d}');
};
</script>

<style scoped>
.product-info {
  display: flex;
  align-items: center;
  gap: 0.625rem;
}
.product-thumb {
  width: 40px;
  height: 40px;
  border-radius: 2px;
}
.product-detail {
  display: flex;
  flex-direction: column;
}
.product-name {
  font-weight: bold;
  font-size: 0.9rem;
}
.product-no {
  font-size: 0.8875rem;
  color: #909399;
}
</style>

