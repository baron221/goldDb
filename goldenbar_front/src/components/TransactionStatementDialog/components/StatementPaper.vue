<template>
<div class="statement-paper">
    <div v-if="snapshot" class="snapshot-badge">
      이 명세서는 {{ formatDateTime(snapshot.timestamp, '{y}-{m}-{d} {h}:{i}') }} 정산 확정 시점에 보관된 데이터입니다.
    </div>

    <div class="top-meta-labels">
      <div class="order-no-top">{{ displayOrder?.orderNo }}</div>
      <div class="print-date-top">{{ formatDateTime(new Date(), '{y}-{m}-{d} {h}:{i}') }}</div>
    </div>
    <div class="header-section">
      <h1 class="main-title">거 래 명 세 표</h1>
      <span class="subtitle">({{ userStore.companyType === 'RTL' ? '공급받는자 보관용' : '공급자 보관용' }})</span>
    </div>

    <div class="top-meta-delivery">
      <div v-if="displayOrder?.deliveryDate" class="meta-item">
        납기일: {{ formatDateOnly(displayOrder.deliveryDate) }}
      </div>
    </div>

    <div class="info-grid-unified">
      <table class="unified-info-table">
        <colgroup>
          <col width="40">
          <col>
          <col width="40">
          <col width="100">
          <col width="40">
          <col>
          <col width="40">
          <col width="100">
        </colgroup>
        <tbody>
          <tr>
            <td colspan="4" class="top-label">공 공급 자</td>
            <td colspan="4" class="top-label">공급받는자</td>
          </tr>
          <tr>
            <td class="cell-label">사업자<br/>번호</td>
            <td colspan="3" class="cell-val">{{ displayOrder?.logisticsCompanyBusinessNo || '123-45-67890' }}</td>
            <td class="cell-label">사업자<br/>번호</td>
            <td colspan="3" class="cell-val">{{ displayOrder?.companyBusinessNo || '-' }}</td>
          </tr>
          <tr>
            <td class="cell-label">상호</td>
            <td colspan="3" class="cell-val">{{ displayOrder?.logisticsCompanyName || '골든바 (GoldB)' }}</td>
            <td class="cell-label">상호</td>
            <td colspan="3" class="cell-val">{{ displayOrder?.companyName }}</td>
          </tr>
          <tr>
            <td class="cell-label">성명</td>
            <td class="cell-val">{{ displayOrder?.logisticsCompanyCEO || '대표자' }}</td>
            <td class="cell-label">연락처</td>
            <td class="cell-val">{{ displayOrder?.logisticsCompanyPhone || '-' }}</td>
            <td class="cell-label">성명</td>
            <td class="cell-val">{{ displayOrder?.companyCEO || '-' }}</td>
            <td class="cell-label">연락처</td>
            <td class="cell-val">{{ displayOrder?.companyPhone || displayOrder?.userPhone || '-' }}</td>
          </tr>
          <tr>
            <td class="cell-label">주소</td>
            <td colspan="3" class="cell-val">{{ displayOrder?.logisticsCompanyAddress || '서울특별시 종로구 종로 1-1' }}</td>
            <td class="cell-label">주소</td>
            <td colspan="3" class="cell-val">{{ displayOrder?.companyAddress || '-' }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <table class="items-table">
      <thead>
        <tr>
          <th width="50%">품명 / 규격</th>
          <th width="37">수량</th>
          <th width="70">중량(g)</th>
          <th width="90">가공비</th>
          <th width="70">비고</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in flattenedItems" :key="item.id">
          <td>
            <div class="product-info-spec">
              <div class="p-name-row">
                <span class="p-name">{{ item.productName || item.productSetTitle }}</span>
                <span class="p-no" v-if="item.productNo">({{ item.productNo }})</span>
              </div>
              <div class="p-spec">
                {{ displayCodeMap[item.purity] || item.purity }}
                <span v-if="item.color && item.color.toUpperCase() !== 'EMPTY'">
                  / {{ displayCodeMap[item.color] || item.color }}
                </span>
              </div>
            </div>
          </td>
          <td align="center">{{ item.quantity }}</td>
          <td align="right">{{ (item.actualWeight || item.requestedWeight || item.approvedWeight || 0).toFixed(2) }}</td>
          <td align="right">{{ formatPrice(((item.retailerConfirmMaterialCost || 0) + (item.retailerConfirmLaborCost || 0)) * (item.quantity || 1)) }}</td>
          <td class="cell-remarks">{{ item.approvedMemo || item.requestedMemo || '' }}</td>
        </tr>
        <tr v-for="i in Math.max(0, 10 - flattenedItems.length)" :key="'empty-' + i">
          <td>&nbsp;</td>
          <td>&nbsp;</td>
          <td>&nbsp;</td>
          <td>&nbsp;</td>
          <td>&nbsp;</td>
        </tr>
      </tbody>
      <tfoot>
        <tr>
          <td colspan="2" class="total-summary-label">합 계</td>
          <td align="right"><strong>{{ totalWeight.toFixed(2) }}</strong></td>
          <td align="right"><strong>{{ formatPrice(totalLaborCost) }}</strong></td>
          <td align="right">{{ formatPrice(totalLaborCost * 0.1) }}</td>
        </tr>
        <tr class="grand-total-row">
          <td colspan="2" class="grand-total-label">청구액</td>
          <td colspan="3" class="grand-total-val">
            <span class="val-text">₩ {{ formatPrice(totalLaborCost * 1.1) }}</span>
          </td>
        </tr>
      </tfoot>
    </table>
  </div>
</template>

<script setup lang="ts">

import { computed } from 'vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import useUserStore from '@/store/modules/user';

const props = defineProps<{
  order: any;
  codeMap: Record<string, string>;
  statement?: any;
}>();

const userStore = useUserStore();

const snapshot = computed(() => {
  if (!props.statement?.snapshotData) return null;
  try {
    return JSON.parse(props.statement.snapshotData);
  } catch (e) {
    console.error('Failed to parse statement snapshot:', e);
    return null;
  }
});

const displayOrder = computed(() => snapshot.value?.order || props.order);
const displayCodeMap = computed(() => snapshot.value?.codeMap || props.codeMap || {});

const formatDateOnly = (dateStr: string) => {
  if (!dateStr) return '';
  return parseTime(new Date(dateStr), '{y}-{m}-{d}');
};

const formatDateTime = (date: Date | string, format: string) => parseTime(date, format);

const flattenedItems = computed(() => {
  const items = displayOrder.value?.orderItems || displayOrder.value?.items;
  if (!items) return [];
  const result: any[] = [];
  items.forEach((item: any) => {
    result.push(item);
    if (item.children && item.children.length > 0) {
      item.children.forEach((child: any) => {
        result.push(child);
      });
    }
  });
  return result;
});

const totalWeight = computed(() => {
  return flattenedItems.value.reduce((sum, item) => sum + (item.actualWeight || item.requestedWeight || item.approvedWeight || 0), 0);
});

const totalLaborCost = computed(() => {
  return flattenedItems.value.reduce((sum, item) => {
    const material = item.retailerConfirmMaterialCost || 0;
    const labor = item.retailerConfirmLaborCost || item.laborCost || 0;
    return sum + (material + labor) * (item.quantity || 1);
  }, 0);
});
</script>

<style lang="scss" src="../StatementStyles.scss" scoped></style>

