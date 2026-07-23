<template>
<div class="expand-table-wrapper-luxury">
    <base-table :data="order?.orderItems?.filter(i => !i.parentId) || order?.orderItems || []" style="width: 100%" row-key="id" class="luxury-nested-table">
      <el-table-column :label="$t('orderDetail.headers.photo')" width="130" align="center" header-align="center" class-name="photo-column" :fixed="isMobile ? false : 'left'" :sortable="false">
        <template #default="item">
          <div class="photo-wrapper-luxury">
            <el-image :src="getPhotoUrl(item.row) || defaultImage" fit="cover" class="product-thumb-luxury">
              <template #error>
                <el-image :src="getPhotoUrl(item.row) ? errorImage : defaultImage" fit="cover" class="product-thumb-luxury" />
              </template>
            </el-image>
            <span v-if="item.row.productSetId" class="set-badge-photo">SET</span>
          </div>
        </template>
      </el-table-column>

      <el-table-column prop="productName" :label="$t('orderDetail.headers.productInfo')" min-width="320" header-align="center" :fixed="isMobile ? false : 'left'" :excel-formatter="productInfoFormatter">
        <template #default="item">
          <div class="product-info-container-luxury" style="display: flex; gap: 1rem; align-items: flex-start; justify-content: space-between; width: 100%;">

            <div class="product-text-luxury" style="flex: 1.2; min-width: 0;">
              <div class="product-meta-row">
                <span v-if="showManufacturerInfo && item.row.manufacturerName" class="manufacturer-name-tag">주문정보(생산): {{ item.row.manufacturerName }}</span>
                <span class="product-category-luxury" v-if="item.row.categoryLarge" style="color: #999;">
                  <span style="margin: 0 0.25rem; color: #dcdfe6;">|</span>
                  {{ displayCodeMap[item.row.categoryLarge] || item.row.categoryLarge }}
                  <template v-if="item.row.categoryMedium"> &gt; {{ displayCodeMap[item.row.categoryMedium] || item.row.categoryMedium }}</template>
                  <template v-if="item.row.categorySmall"> &gt; {{ displayCodeMap[item.row.categorySmall] || item.row.categorySmall }}</template>
                </span>
              </div>
              <div v-if="showMarketInfo && order.companyName" class="product-meta-row">
                <span class="manufacturer-name-tag">주문정보(소매점): {{ order.companyName }}</span>
              </div>

              <div class="product-name-luxury clickable" @click="$emit('go-to-detail', item.row)">{{ item.row.productName || item.row.productSetTitle }}
                <span class="product-no-luxury">{{ item.row.productNo }}</span>
              </div>

              <div class="product-options-luxury" v-if="(item.row.purity && item.row.purity.toUpperCase() !== 'EMPTY') || (item.row.color && item.row.color.toUpperCase() !== 'EMPTY')">
                <span class="option-tag-luxury" v-if="item.row.purity && item.row.purity.toUpperCase() !== 'EMPTY'">{{ getCodeName(item.row.purity) }}</span>
                <span class="option-tag-luxury" v-if="item.row.color && item.row.color.toUpperCase() !== 'EMPTY'">{{ getCodeName(item.row.color) }}</span>
              </div>
            </div>

            <div v-if="item.row.productSetId && item.row.children && item.row.children.length > 0" class="set-children-list-luxury" style="flex: 1; min-width: 140px; padding: 0.4rem 0.6rem; background: rgba(0, 0, 0, 0.02); border-radius: 4px; border-left: 3px solid #b8975a;">
              <div style="font-size: 11px; font-weight: 600; color: #b8975a; margin-bottom: 0.25rem;">[구성품 목록]</div>
              <div v-for="c in item.row.children" :key="c.id" style="font-size: 11px; color: #666; display: flex; justify-content: space-between; margin-bottom: 0.125rem; gap: 0.5rem;">
                <span style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">• {{ c.productName || c.productSetTitle }} <span style="color: #999; font-size: 10px;">{{ c.productNo }}</span></span>
                <span style="font-weight: 600; color: #333; white-space: nowrap;">{{ c.quantity }}개</span>
              </div>
            </div>
          </div>
        </template>
      </el-table-column>

      <el-table-column :label="$t('orderDetail.headers.qty')" width="90" align="center" prop="quantity" header-align="center" :fixed="isMobile ? false : 'left'" :excel-formatter="qtyFormatter">
        <template #default="item">
          <span class="qty-badge-large">{{ item.row.quantity }}</span>
          <div v-if="item.row.productSetId && item.row.children && item.row.children.length > 0" class="set-children-qty-count" style="font-size: 10px; color: #888; margin-top: 2px; line-height: 1.1;">
            (구성품 {{ item.row.children.length }}개)
          </div>
          <br v-if="!item.row.productSetId || item.row.isAsOrder" />
          <span v-if="item.row.isAsOrder" class="as-active-icon">AS</span>
          <el-icon v-if="item.row.isAsOrder" class="as-active-icon"><Check /></el-icon>
          <span v-else-if="!item.row.productSetId" class="empty-dash">-</span>
        </template>
      </el-table-column>

      <el-table-column v-if="userStore.companyType !== 'MFFG' && userStore.companyType !== 'MFG'" prop="approvedWeight" label="물류승인" width="180" align="center" header-align="center" :excel-formatter="(row) => weightFormatter(row, 'approvedWeight', 'approvedMemo')">
        <template #default="item">
          <span v-if="item.row.approvedWeight" class="weight-display-luxury">
            <span class="val">{{ item.row.approvedWeight }}</span><span class="unit">g</span>
          </span>
          <span v-else class="empty-dash">-</span>
          <br />
          <span class="memo-text-luxury">{{ item.row.approvedMemo || '-' }}</span>
        </template>
      </el-table-column>

      <el-table-column v-if="userStore.companyType !== 'RTL'" prop="requestedWeight" label="공장 의뢰" width="180" align="center" header-align="center" :excel-formatter="(row) => weightFormatter(row, 'requestedWeight', 'requestedMemo')">
        <template #default="item">
          <span v-if="item.row.requestedWeight" class="weight-display-luxury requested">
            <span class="val">{{ item.row.requestedWeight }}</span><span class="unit">g</span>
          </span>
          <span v-else class="empty-dash">-</span><br />
          <span class="memo-text-luxury">{{ item.row.requestedMemo || '-' }}</span>
        </template>
      </el-table-column>

      <el-table-column prop="actualWeight" label="검수" width="180" align="center" header-align="center" :excel-formatter="(row) => weightFormatter(row, 'actualWeight', 'inspectionMemo')">
        <template #default="item">
          <span v-if="item.row.actualWeight" class="weight-display-luxury actual">
            <span class="val">{{ item.row.actualWeight }}</span><span class="unit">g</span>
          </span>
          <span v-else class="empty-dash">-</span><br />
          <span class="memo-text-luxury">{{ item.row.inspectionMemo || '-' }}</span>
        </template>
      </el-table-column>

      <el-table-column v-if="userStore.companyType !== 'RTL'" prop="factoryInputMaterialCost" label="공장입력비용" width="130" align="right" header-align="center" :excel-formatter="factoryInputFormatter">
        <template #default="item">
          <span v-if="item.row.factoryInputMaterialCost" class="cost-text-luxury">
            <span class="curr">₩</span>{{ formatPrice(item.row.factoryInputMaterialCost) }}
          </span>
          <span v-else class="empty-dash">-</span>
          <br />
          <span v-if="item.row.factoryInputLaborCost" class="cost-text-luxury">
            <span class="curr">₩</span>{{ formatPrice(item.row.factoryInputLaborCost) }}
          </span>
          <span v-else class="empty-dash">-</span>
        </template>
      </el-table-column>

      <el-table-column v-if="userStore.companyType !== 'MFG'" prop="price" :label="$t('orderDetail.headers.unitPrice')" width="130" align="right" header-align="center" :fixed="isMobile ? false : 'right'" :excel-formatter="unitPriceFormatter">
        <template #default="{ row }">
          <div class="settlement-luxury-box unit-price-box">
            <div class="settle-line">{{ $t('orderDetail.settlement.material') }}: <span class="v">
              <template v-if="isPostPending">
                <template v-if="getMaterialCostPending(row)">₩{{ formatPrice(getMaterialCostPending(row)) }}</template>
                <template v-else>-</template>
              </template>
              <template v-else>
                <template v-if="getMaterialCostNormal(row)">₩{{ formatPrice(getMaterialCostNormal(row)) }}</template>
                <template v-else>-</template>
              </template>
            </span></div>
            <div class="settle-line">{{ $t('orderDetail.settlement.labor') }}: <span class="v">
              <template v-if="isPostPending">
                <template v-if="getLaborCostPending(row)">₩{{ formatPrice(getLaborCostPending(row)) }}</template>
                <template v-else>-</template>
              </template>
              <template v-else>
                <template v-if="getLaborCostNormal(row)">₩{{ formatPrice(getLaborCostNormal(row)) }}</template>
                <template v-else>-</template>
              </template>
            </span></div>
            <div class="settle-total-luxury unit-total">
              <span class="label">{{ $t('orderDetail.settlement.total') }}:</span>
              <span class="amount"><span class="curr">₩</span>{{ formatPrice(getItemUnitPrice(row)) }}</span>
            </div>
          </div>
        </template>
      </el-table-column>

      <el-table-column prop="price" :label="isStatementVisible ? $t('orderDetail.headers.settlementConfirmed') : $t('orderDetail.headers.settlementExpected')" width="150" align="right" header-align="center" :fixed="isMobile ? false : 'right'" class-name="price-column" :excel-formatter="settlementTotalFormatter">
        <template #default="{ row }">
          <div class="settlement-luxury-box">
            <div class="settle-line">{{ $t('orderDetail.settlement.material') }}: <span class="v">
              <template v-if="isPostPending">
                <template v-if="getMaterialCostPending(row)">₩{{ formatPrice(getMaterialCostPending(row) * row.quantity) }}</template>
                <template v-else>-</template>
              </template>
              <template v-else>
                <template v-if="getMaterialCostNormal(row)">₩{{ formatPrice(getMaterialCostNormal(row) * row.quantity) }}</template>
                <template v-else>-</template>
              </template>
            </span></div>
            <div class="settle-line">{{ $t('orderDetail.settlement.labor') }}: <span class="v">
              <template v-if="isPostPending">
                <template v-if="getLaborCostPending(row)">₩{{ formatPrice(getLaborCostPending(row) * row.quantity) }}</template>
                <template v-else>-</template>
              </template>
              <template v-else>
                <template v-if="getLaborCostNormal(row)">₩{{ formatPrice(getLaborCostNormal(row) * row.quantity) }}</template>
                <template v-else>-</template>
              </template>
            </span></div>
            <div class="settle-total-luxury">
              <span class="label">{{ $t('orderDetail.settlement.total') }}:</span>
              <span class="amount"><span class="curr">₩</span>{{ formatPrice(getItemTotal(row)) }}</span>
            </div>
          </div>
        </template>
      </el-table-column>
    </base-table>
  </div>
</template>

<script setup lang="ts">
import { Check } from '@element-plus/icons-vue';
import { formatPrice } from '@/utils/format';
import BaseTable from '@/components/BaseTable/index.vue';
import { isStatementVisibleStatus } from '@/utils/order';
import { computed } from 'vue';

const props = defineProps({
  order: {
    type: Object,
    required: true
  },
  displayCodeMap: {
    type: Object,
    required: true
  },
  isMobile: {
    type: Boolean,
    default: false
  },
  isPostPending: {
    type: Boolean,
    default: false
  },
  userStore: {
    type: Object,
    required: true
  }
});

defineEmits(['go-to-detail']);

const defaultImage = '/thumb_no_img.png';
const errorImage = '/nope-not-here.avif';

const getPhotoUrl = (row: any) => {
  if (row.photoUrl) return row.photoUrl;
  if (row.productSetId && row.children && row.children.length > 0) {
    const childWithPhoto = row.children.find((c: any) => c.photoUrl);
    if (childWithPhoto) return childWithPhoto.photoUrl;
  }
  return '';
};

const isStatementVisible = computed(() => {
  return props.order ? isStatementVisibleStatus(props.order.status) : false;
});

const isAdmin = computed(() => (props.userStore.roles || []).includes('admin'));
const showManufacturerInfo = computed(() => isAdmin.value || props.userStore.companyType === 'DCC');
const showMarketInfo = computed(() => isAdmin.value || props.userStore.companyType === 'DCC');

const getCodeName = (code: string) => {
  if (!code) return '';
  return props.displayCodeMap[code] || code;
};

const getMaterialCostPending = (row: any) => {
  if (row.productSetId && row.children && row.children.length > 0) {
    return row.children.reduce((sum: number, c: any) => sum + (c.retailerConfirmMaterialCost || 0), 0);
  }
  return row.retailerConfirmMaterialCost || 0;
};

const getMaterialCostNormal = (row: any) => {
  if (row.productSetId && row.children && row.children.length > 0) {
    return row.children.reduce((sum: number, c: any) => sum + (c.factoryPrice || 0), 0);
  }
  return row.factoryPrice || 0;
};

const getLaborCostPending = (row: any) => {
  if (row.productSetId && row.children && row.children.length > 0) {
    return row.children.reduce((sum: number, c: any) => sum + (c.retailerConfirmLaborCost || 0), 0);
  }
  return row.retailerConfirmLaborCost || 0;
};

const getLaborCostNormal = (row: any) => {
  if (row.productSetId && row.children && row.children.length > 0) {
    return row.children.reduce((sum: number, c: any) => sum + (c.laborCost || 0), 0);
  }
  return row.laborCost || 0;
};

const getItemTotal = (row: any) => {
  return getItemUnitPrice(row) * row.quantity;
};

const getItemUnitPrice = (row: any) => {
  if (row.productSetId && row.children && row.children.length > 0) {
    if (props.isPostPending) {
      return row.children.reduce((sum: number, c: any) => sum + (c.retailerConfirmMaterialCost || 0) + (c.retailerConfirmLaborCost || 0), 0);
    } else {
      return row.children.reduce((sum: number, c: any) => sum + (c.price || 0), 0);
    }
  }
  if (props.isPostPending) {
    const material = row.retailerConfirmMaterialCost || 0;
    const labor = row.retailerConfirmLaborCost || 0;
    return material + labor;
  } else {
    return row.price || 0;
  }
};

const productInfoFormatter = (row: any) => {
  const manufacturer = row.manufacturerName ? `[${row.manufacturerName}] ` : '';
  const categories = [row.categoryLarge, row.categoryMedium, row.categorySmall]
    .filter(Boolean)
    .map(c => props.displayCodeMap[c] || c)
    .join(' > ');
  const options = [row.purity, row.color]
    .filter(c => c && c.toUpperCase() !== 'EMPTY')
    .map(c => getCodeName(c))
    .join(', ');

  let text = `${manufacturer}${row.productName || row.productSetTitle || ''}`;
  if (row.productNo) text += `\n(번호: ${row.productNo})`;
  if (categories) text += `\n(분류: ${categories})`;
  if (options) text += `\n(옵션: ${options})`;

  if (row.productSetId && row.children && row.children.length > 0) {
    const childrenText = row.children.map((c: any) => `  - ${c.productName || c.productSetTitle || ''} (${c.productNo || ''}): ${c.quantity}개`).join('\n');
    text += `\n[구성품 목록]\n${childrenText}`;
  }
  return text;
};

const qtyFormatter = (row: any) => {
  let text = `${row.quantity}${row.isAsOrder ? '\n(AS 의뢰)' : ''}`;
  if (row.productSetId && row.children && row.children.length > 0) {
    text += `\n(구성품 ${row.children.length}개)`;
  }
  return text;
};

const weightFormatter = (row: any, weightField: string, memoField: string) => {
  const weight = row[weightField];
  const memo = row[memoField];
  if (!weight && !memo) return '-';
  return `${weight || 0}g${memo ? `\n(${memo})` : ''}`;
};

const unitPriceFormatter = (row: any) => {
  const isPending = props.isPostPending;
  const material = isPending ? getMaterialCostPending(row) : getMaterialCostNormal(row);
  const labor = isPending ? getLaborCostPending(row) : getLaborCostNormal(row);
  const total = getItemUnitPrice(row);
  return `재료비: ₩${formatPrice(material)}\n수공비: ₩${formatPrice(labor)}\n합계: ₩${formatPrice(total)}`;
};

const settlementTotalFormatter = (row: any) => {
  const isPending = props.isPostPending;
  const material = isPending ? getMaterialCostPending(row) : getMaterialCostNormal(row);
  const labor = isPending ? getLaborCostPending(row) : getLaborCostNormal(row);
  const total = getItemTotal(row);
  return `재료비: ₩${formatPrice(material * row.quantity)}\n수공비: ₩${formatPrice(labor * row.quantity)}\n합계: ₩${formatPrice(total)}`;
};

const factoryInputFormatter = (row: any) => {
  const m = row.factoryInputMaterialCost;
  const l = row.factoryInputLaborCost;
  if (!m && !l) return '-';
  return `재료: ₩${formatPrice(m || 0)}\n수공: ₩${formatPrice(l || 0)}`;
};
</script>

<style lang="scss" src="./OrderDetailTableStyles.scss" scoped></style>

