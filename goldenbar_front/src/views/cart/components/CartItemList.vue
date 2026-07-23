<template>
<div class="inventory-table-card">
    <base-table
      ref="cartTableRef"
      v-loading="loading"
      :data="cartItems"
      style="width: 100%"
      class="custom-cart-table d-none-mobile"
      @selection-change="handleSelectionChange"
    >
      <el-table-column type="selection" width="55" align="center" />

      <el-table-column
        :label="$t('cart.itemList.headers.productInfo')"
        min-width="400"
        :excel-formatter="(row) => {
          let text = row.productSetId ? `[SET] ${row.productSetTitle}` : `${row.productNo} / ${row.productName}`;
          if (row.companyName) text += `\n제조사: ${row.companyName}`;
          const cat = [row.categoryLarge, row.categoryMedium, row.categorySmall]
            .map(c => codeMap[c] || c)
            .filter(c => c && c !== 'EMPTY')
            .join(' > ');
          if (cat) text += `\n분류: ${cat}`;
          if (!row.productSetId) {
            const specs = [codeMap[row.purity], codeMap[row.color], codeMap[row.size] || row.size]
              .filter(s => s && s !== 'EMPTY')
              .join(', ');
            if (specs) text += `\n스펙: ${specs}`;
            text += `\n중량: ${row.weight}g`;
            if (row.memo) text += `\n메모: ${row.memo}`;
          }
          return text;
        }"
      >
        <template #default="{row}">
          <div class="cart-product-info">
            <el-image :src="getThumbnailUrl(row.photoUrl) || defaultImage" fit="cover" class="product-img" />
            <div class="product-text">
              <template v-if="row.productSetId">
                <div class="product-no-row">
                  <div class="set-badge">SET</div>
                  <div v-if="row.companyName" class="manufacturer-badge-mini">{{ row.companyName }}</div>
                </div>
                <div class="product-name" @click="goToSetDetail(row.productSetId)">{{ row.productSetTitle }}</div>
                <div class="product-category" v-if="row.categoryLarge" style="font-size: 0.8875rem; color: #999; margin-top: 0.125rem;">
                  {{ codeMap[row.categoryLarge] || row.categoryLarge }}
                  <template v-if="row.categoryMedium"> > {{ codeMap[row.categoryMedium] || row.categoryMedium }}</template>
                  <template v-if="row.categorySmall"> > {{ codeMap[row.categorySmall] || row.categorySmall }}</template>
                </div>
              </template>
              <template v-else>
                <div class="product-no-row">
                  <div class="product-no">{{ row.productNo }}</div>
                  <div v-if="row.companyName" class="manufacturer-badge-mini">{{ row.companyName }}</div>
                </div>
                <div class="product-name" @click="goToProductDetail(row.productId)">{{ row.productName }}</div>
                <div class="product-category" v-if="row.categoryLarge" style="font-size: 0.8875rem; color: #999; margin-top: 0.125rem;">
                  {{ codeMap[row.categoryLarge] || row.categoryLarge }}
                  <template v-if="row.categoryMedium"> > {{ codeMap[row.categoryMedium] || row.categoryMedium }}</template>
                  <template v-if="row.categorySmall"> > {{ codeMap[row.categorySmall] || row.categorySmall }}</template>
                </div>
                <div class="product-spec">
                  <span v-if="row.purity && row.purity !== 'EMPTY' && (codeMap[row.purity] || row.purity) !== 'EMPTY'" class="spec-tag">{{ codeMap[row.purity] || row.purity }}</span>
                  <span v-if="row.color && row.color !== 'EMPTY' && (codeMap[row.color] || row.color) !== 'EMPTY'" class="spec-tag">{{ codeMap[row.color] || row.color }}</span>
                  <span v-if="row.size && row.size !== 'EMPTY'" class="spec-tag">{{ codeMap[row.size] || row.size }}</span>
                  <span class="spec-val">{{ row.weight }}g</span>
                </div>
                <div v-if="row.memo" class="cart-item-memo">📝 {{ row.memo }}</div>
              </template>
            </div>
          </div>
        </template>
      </el-table-column>

      <el-table-column
        :label="$t('cart.itemList.headers.basePriceLabor')"
        width="240"
        align="right"
        header-align="center"
        :excel-formatter="(row) => {
          const base = row.customFactoryPrice || row.factoryPrice || 0;
          const labor = row.customLaborCost || row.laborCost || 0;
          return `기본: ₩${formatPrice(base)}\n수공비: ₩${formatPrice(labor)}\n합계: ₩${formatPrice(base + labor)}`;
        }"
      >
        <template #default="{row}">
          <div v-if="userStore.companyType === 'RTL'" class="price-static-view">
            <div class="price-line">{{ $t('cart.itemList.labels.base') }} ₩ {{ formatPrice(row.customFactoryPrice || row.factoryPrice) }}</div>
            <div class="price-line">{{ $t('cart.itemList.labels.labor') }} ₩ {{ formatPrice(row.customLaborCost || row.laborCost) }}</div>
            <div class="price-total-line">
              {{ $t('cart.itemList.labels.total') }} ₩ {{ formatPrice((row.customFactoryPrice || row.factoryPrice || 0) + (row.customLaborCost || row.laborCost || 0)) }}
            </div>
          </div>
          <div v-else class="price-edit-cell">
            <div class="price-input-row">
              <span class="mini-label">{{ $t('cart.itemList.labels.base') }}</span>
              <el-input-number
                v-model="row.customFactoryPrice"
                :min="0"
                size="small"
                controls-position="right"
                class="custom-num-input"
                @change="() => handlePriceChange(row)"
              />
            </div>
            <div class="price-input-row">
              <span class="mini-label">{{ $t('cart.itemList.labels.labor') }}</span>
              <el-input-number
                v-model="row.customLaborCost"
                :min="0"
                size="small"
                controls-position="right"
                class="custom-num-input"
                @change="() => handlePriceChange(row)"
              />
            </div>
            <div class="price-total-summary">
              {{ $t('cart.itemList.labels.total') }} ₩ {{ formatPrice((row.customFactoryPrice || 0) + (row.customLaborCost || 0)) }}
            </div>
          </div>
        </template>
      </el-table-column>

      <el-table-column :label="$t('cart.itemList.headers.qty')" width="140" align="center">
        <template #default="{row}">
          <el-input-number
            v-model="row.quantity"
            :min="1"
            size="small"
            controls-position="right"
            style="width: 100px;"
            @change="(val) => handleQuantityChange(row, val)"
          />
        </template>
      </el-table-column>

      <el-table-column
        :label="$t('cart.itemList.headers.totalAmount')"
        width="160"
        header-align="center"
        align="right"
        :excel-formatter="(row) => `₩${formatPrice(row.price * row.quantity)}`"
      >
        <template #default="{row}">
          <span class="row-total-price">₩ {{ formatPrice(row.price * row.quantity) }}</span>
        </template>
      </el-table-column>

      <el-table-column :label="$t('cart.itemList.headers.del')" width="80" align="center">
        <template #default="{row}">
          <el-button link class="delete-icon-btn" :icon="Delete" @click="handleRemove(row)" />
        </template>
      </el-table-column>
    </base-table>

    <div class="mobile-cart-list d-none-desktop">
      <div v-for="row in cartItems" :key="row.id" class="mobile-cart-card">
        <div class="mobile-card-header">
          <el-checkbox
            :model-value="selectedItems.some(i => i.id === row.id)"
            @change="(val) => handleMobileCheckboxChange(row, val)"
          ><span class="mobile-checkbox-label">선택</span></el-checkbox>
          <el-button link class="delete-icon-btn" :icon="Delete" @click="handleRemove(row)" />
        </div>

        <div class="mobile-card-body cart-product-info">
          <el-image :src="getThumbnailUrl(row.photoUrl) || defaultImage" fit="cover" class="product-img" />
          <div class="product-text">
            <template v-if="row.productSetId">
              <div class="product-no-row">
                <div class="set-badge">SET</div>
                <div v-if="row.companyName" class="manufacturer-badge-mini">{{ row.companyName }}</div>
              </div>
              <div class="product-name" @click="goToSetDetail(row.productSetId)">{{ row.productSetTitle }}</div>
              <div class="product-category" v-if="row.categoryLarge" style="font-size: 0.8875rem; color: #999; margin-top: 0.125rem;">
                {{ codeMap[row.categoryLarge] || row.categoryLarge }}
                <template v-if="row.categoryMedium"> > {{ codeMap[row.categoryMedium] || row.categoryMedium }}</template>
                <template v-if="row.categorySmall"> > {{ codeMap[row.categorySmall] || row.categorySmall }}</template>
              </div>
            </template>
            <template v-else>
              <div class="product-no-row">
                <div class="product-no">{{ row.productNo }}</div>
                <div v-if="row.companyName" class="manufacturer-badge-mini">{{ row.companyName }}</div>
              </div>
              <div class="product-name" @click="goToProductDetail(row.productId)">{{ row.productName }}</div>
              <div class="product-category" v-if="row.categoryLarge" style="font-size: 0.8875rem; color: #999; margin-top: 0.125rem;">
                {{ codeMap[row.categoryLarge] || row.categoryLarge }}
                <template v-if="row.categoryMedium"> > {{ codeMap[row.categoryMedium] || row.categoryMedium }}</template>
                <template v-if="row.categorySmall"> > {{ codeMap[row.categorySmall] || row.categorySmall }}</template>
              </div>
              <div class="product-spec">
                <span v-if="row.purity && row.purity !== 'EMPTY' && (codeMap[row.purity] || row.purity) !== 'EMPTY'" class="spec-tag">{{ codeMap[row.purity] || row.purity }}</span>
                <span v-if="row.color && row.color !== 'EMPTY' && (codeMap[row.color] || row.color) !== 'EMPTY'" class="spec-tag">{{ codeMap[row.color] || row.color }}</span>
                <span v-if="row.size && row.size !== 'EMPTY'" class="spec-tag">{{ codeMap[row.size] || row.size }}</span>
                <span class="spec-val">{{ row.weight }}g</span>
              </div>
              <div v-if="row.memo" class="cart-item-memo">📝 {{ row.memo }}</div>
            </template>
          </div>
        </div>

        <div class="mobile-card-prices">
          <div v-if="userStore.companyType === 'RTL'" class="price-static-view">
            <div class="price-line">{{ $t('cart.itemList.labels.base') }} ₩ {{ formatPrice(row.customFactoryPrice || row.factoryPrice) }}</div>
            <div class="price-line">{{ $t('cart.itemList.labels.labor') }} ₩ {{ formatPrice(row.customLaborCost || row.laborCost) }}</div>
            <div class="price-total-line">
              {{ $t('cart.itemList.labels.total') }} ₩ {{ formatPrice((row.customFactoryPrice || row.factoryPrice || 0) + (row.customLaborCost || row.laborCost || 0)) }}
            </div>
          </div>
          <div v-else class="price-edit-cell">
            <div class="price-input-row">
              <span class="mini-label">{{ $t('cart.itemList.labels.base') }}</span>
              <el-input-number v-model="row.customFactoryPrice" :min="0" size="small" controls-position="right" class="custom-num-input" @change="() => handlePriceChange(row)" />
            </div>
            <div class="price-input-row">
              <span class="mini-label">{{ $t('cart.itemList.labels.labor') }}</span>
              <el-input-number v-model="row.customLaborCost" :min="0" size="small" controls-position="right" class="custom-num-input" @change="() => handlePriceChange(row)" />
            </div>
            <div class="price-total-summary">
              {{ $t('cart.itemList.labels.total') }} ₩ {{ formatPrice((row.customFactoryPrice || 0) + (row.customLaborCost || 0)) }}
            </div>
          </div>
        </div>

        <div class="mobile-qty-total">
          <div class="qty-control" style="display: flex; align-items: center;">
            <span style="font-size: 0.8875rem; color: #888; margin-right: 0.5rem;">수량: </span>
            <el-input-number
              v-model="row.quantity"
              :min="1"
              size="small"
              controls-position="right"
              style="width: 90px;"
              @change="(val) => handleQuantityChange(row, val)"
            />
          </div>
          <div class="row-total-price">합계: ₩ {{ formatPrice(row.price * row.quantity) }}</div>
        </div>
      </div>
    </div>

    <el-empty v-if="!loading && cartItems.length === 0" :description="$t('cart.itemList.empty.description')" class="empty-cart-view">
      <el-button type="primary" @click="goToShopping" class="primary-gold-btn">{{ $t('cart.itemList.empty.button') }}</el-button>
    </el-empty>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { updateCartPrice, removeFromCart, updateCartQuantity } from '@/api/cart';
import { ElMessage, ElMessageBox } from 'element-plus';
import { Delete } from '@element-plus/icons-vue';
import { getThumbnailUrl } from '@/utils';
import { formatPrice } from '@/utils/format';
import BaseTable from '@/components/BaseTable/index.vue';

const props = defineProps({
  cartItems: {
    type: Array as () => any[],
    required: true
  },
  loading: {
    type: Boolean,
    default: false
  },
  userStore: {
    type: Object,
    required: true
  },
  codeMap: {
    type: Object,
    required: true
  }
});

const emit = defineEmits([
  'selection-change',
  'go-to-product-detail',
  'go-to-set-detail',
  'go-to-shopping',
  'refresh'
]);

const { t } = useI18n();
const cartTableRef = ref();
const selectedItems = ref<any[]>([]);
const defaultImage = 'https://via.placeholder.com/100x100?text=No+Image';

const handleSelectionChange = (val: any[]) => {
  selectedItems.value = val;
  emit('selection-change', val);
};

const handleMobileCheckboxChange = (row: any, checked: boolean) => {
  if (cartTableRef.value) {
    cartTableRef.value.toggleRowSelection(row, checked);
  }
};

const handleQuantityChange = async (row: any, val: number) => {
  if (!val) return;
  try {
    await updateCartQuantity(row.id, val);
    emit('refresh');
  } catch (error) {
    ElMessage.error(t('cart.messages.quantityError'));
  }
};

const handlePriceChange = async (row: any) => {
  try {
    await updateCartPrice(row.id, row.customFactoryPrice, row.customLaborCost);
    row.price = (row.customFactoryPrice || 0) + (row.customLaborCost || 0);
  } catch (error) {
    ElMessage.error(t('cart.messages.priceError'));
  }
};

const handleRemove = (row: any) => {
  ElMessageBox.confirm(t('cart.messages.confirmRemove'), t('cart.messages.confirmRemoveTitle')).then(async () => {
    await removeFromCart(row.id);
    ElMessage.success(t('cart.messages.removeSuccess'));
    emit('refresh');
  });
};

const goToProductDetail = (id: number) => {
  emit('go-to-product-detail', id);
};

const goToSetDetail = (id: number) => {
  emit('go-to-set-detail', id);
};

const goToShopping = () => {
  emit('go-to-shopping');
};

const toggleRowSelection = (row: any, select: boolean) => {
  if (cartTableRef.value) {
    cartTableRef.value.toggleRowSelection(row, select);
  }
};

defineExpose({
  toggleRowSelection
});
</script>

<style lang="scss" scoped>
@import "./CartItemListStyles.scss";
</style>

