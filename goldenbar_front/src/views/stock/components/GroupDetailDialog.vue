<template>
  <base-popup
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    title="제품별 재고 상세 내역"
    width="1100px"
    destroy-on-close
    append-to-body
    class="premium-dialog"
  >
    <div v-if="selectedGroup" class="dialog-content">

      <div class="product-dashboard">
        <div class="dashboard-photo">
          <el-image
            v-if="(selectedGroup.attachments && selectedGroup.attachments.length > 0) || selectedGroup.productPhotoUrl"
            :src="(selectedGroup.attachments && selectedGroup.attachments.find(a => a.isMain)) ? selectedGroup.attachments.find(a => a.isMain).filePath : (selectedGroup.attachments && selectedGroup.attachments.length > 0 ? selectedGroup.attachments[0].filePath : selectedGroup.productPhotoUrl)"
            fit="contain"
            class="dashboard-img"
            :preview-src-list="[(selectedGroup.attachments && selectedGroup.attachments.length > 0) ? selectedGroup.attachments[0].filePath : selectedGroup.productPhotoUrl]"
            preview-teleported
          />
          <div v-else class="no-image-placeholder">NO IMAGE</div>
        </div>
        <div class="dashboard-details">
          <div class="prod-no">{{ selectedGroup.productNo }}</div>
          <div class="prod-name">{{ selectedGroup.productName }}</div>
          <div class="prod-meta">
            <span class="meta-label">카테고리:</span>
            <span class="meta-val">
              {{ codeMap[selectedGroup.categoryLarge] || selectedGroup.categoryLarge }}
              <template v-if="selectedGroup.categoryMedium"> > {{ codeMap[selectedGroup.categoryMedium] || selectedGroup.categoryMedium }}</template>
              <template v-if="selectedGroup.categorySmall"> > {{ codeMap[selectedGroup.categorySmall] || selectedGroup.categorySmall }}</template>
            </span>
            <span class="meta-divider">|</span>
            <span class="meta-label">함량:</span>
            <span class="meta-val">{{ codeMap[selectedGroup.purity] || selectedGroup.purity }}</span>
            <span v-if="selectedGroup.color" class="meta-divider">|</span>
            <span v-if="selectedGroup.color" class="meta-label">컬러:</span>
            <span v-if="selectedGroup.color" class="meta-val">{{ codeMap[selectedGroup.color] || selectedGroup.color }}</span>
          </div>
          <div class="dashboard-stats">
            <div class="stat-box">
              <div class="stat-label">총 보유 수량</div>
              <div class="stat-value">{{ selectedGroup.quantity }}<span>개</span></div>
            </div>
            <div class="stat-box">
              <div class="stat-label">총 실중량</div>
              <div class="stat-value">{{ selectedGroup.totalWeight.toFixed(2) }}<span>g</span></div>
            </div>
          </div>
        </div>
      </div>

      <base-table
        :data="selectedGroup.items"
        border
        fit
        highlight-current-row
        style="width: 100%; margin-top: 1.25rem;"
        :header-cell-style="{ textAlign: 'center' }"
      >
        <el-table-column
          label="재고/주문 번호"
          prop="stockNo"
          width="200"
          align="center"
          header-align="center"
          :excel-formatter="(row) => `${row.stockNo}${row.sourceOrderNo ? '\n(' + row.sourceOrderNo + ')' : ''}`"
        >
          <template #default="{row}">
            <span
              style="cursor: pointer; color: #c5a880; font-weight: bold; text-decoration: underline; font-family: 'S-CoreDream', 'Jost', sans-serif; white-space: nowrap;"
              @click="$emit('navigate-stock', row.id)"
            >
              {{ row.stockNo }}
            </span>
            <br />
            <span
              v-if="row.sourceOrderNo"
              style="cursor: pointer; color: #c5a880; font-weight: bold; text-decoration: underline; font-family: 'S-CoreDream', 'Jost', sans-serif; white-space: nowrap;"
              @click="$emit('navigate-order', row.sourceOrderNo)"
            >
              {{ row.sourceOrderNo }}
            </span>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column
          label="상태"
          prop="status"
          width="130"
          align="center"
          header-align="center"
          :excel-formatter="(row) => {
            let text = getStatusName(row.status);
            if (row.exhaustionDate) text += `\n소진: ${formatDate(row.exhaustionDate).split(' ')[0]}`;
            if (row.updatedAt) text += `\n(${formatDate(row.updatedAt)})`;
            return text;
          }"
        >
          <template #default="{row}">
            <el-tag :type="getStatusTagType(row.status)">{{ getStatusName(row.status) }}</el-tag>
            <div v-if="row.exhaustionDate" style="font-size: 0.8rem; color: #f56c6c; margin-top: 0.25rem; font-weight: bold;">
              소진: {{ formatDate(row.exhaustionDate).split(' ')[0] }}
            </div>
            <div v-if="row.updatedAt" style="font-size: 0.8rem; color: #909399; margin-top: 0.25rem;">
              {{ formatDate(row.updatedAt) }}
            </div>
          </template>
        </el-table-column>
        <el-table-column
          label="함량/실중량"
          width="100"
          align="right"
          header-align="center"
          :excel-formatter="(row) => `${codeMap[row.purity] || row.purity}\n${row.actualWeight.toFixed(2)}g`"
        >
          <template #default="{row}">
            <span style="font-size: 0.8875rem; color: #909399;">{{ codeMap[row.purity] || row.purity }}</span>
            <br />
            <span>{{ row.actualWeight.toFixed(2) }}g</span>
          </template>
        </el-table-column>
        <el-table-column
          label="제작/수령 일시"
          width="150"
          align="center"
          header-align="center"
          :excel-formatter="(row) => `제작: ${row.productionDate ? formatDate(row.productionDate).split(' ')[0] : '-'}\n수령: ${formatDate(row.createdAt)}`"
        >
          <template #default="{row}">
            <span style="font-size: 0.95rem; color: #606266;">{{ row.productionDate ? formatDate(row.productionDate).split(' ' )[0] : '-' }}</span>
            <br />
            <span style="font-size: 0.95rem; color: #606266;">{{ formatDate(row.createdAt) }}</span>
          </template>
        </el-table-column>
        <el-table-column
          label="소매 재료비/수공비"
          width="130"
          align="right"
          header-align="center"
          :excel-formatter="(row) => `재료비: ${row.retailerConfirmMaterialCost !== undefined ? formatPrice(row.retailerConfirmMaterialCost) : '-'}\n수공비: ${row.retailerConfirmLaborCost !== undefined ? formatPrice(row.retailerConfirmLaborCost) : '-'}`"
        >
          <template #default="{row}">
            <template v-if="row.retailerConfirmMaterialCost !== undefined && row.retailerConfirmMaterialCost !== null">
              <span style="font-size: 0.8875rem; color: #909399; margin-right: 0.125rem;">₩</span>
              <span style="font-weight: 600;">{{ formatPrice(row.retailerConfirmMaterialCost) }}</span>
            </template>
            <span v-else style="color: #999; font-size: 0.95rem;">-</span>

            <br />

            <template v-if="row.retailerConfirmLaborCost !== undefined && row.retailerConfirmLaborCost !== null">
              <span style="font-size: 0.8875rem; color: #909399; margin-right: 0.125rem;">₩</span>
              <span style="font-weight: 600;">{{ formatPrice(row.retailerConfirmLaborCost) }}</span>
            </template>
            <span v-else style="color: #999; font-size: 0.95rem;">-</span>

          </template>
        </el-table-column>
        <el-table-column
          label="제조사/물류업체"
          prop="companyName"
          width="120"
          header-align="center"
          :excel-formatter="(row) => `${row.companyName || '-'}\n${row.logisticsCompanyName || '-'}`"
        >
          <template #default="{row}">
            <span >{{ row.companyName  }}</span>
            <br />
            <span >{{ row.logisticsCompanyName  }}</span>
          </template>

        </el-table-column>

        <el-table-column label="메모" prop="memo" min-width="120" header-align="center" show-overflow-tooltip />
        <el-table-column label="액션" width="140" align="center" :fixed="!isMobile ? 'right' : false" header-align="center">
          <template #default="{row}">
            <el-tooltip content="바코드 출력" placement="top">
              <el-button size="small" :icon="Printer" circle @click="$emit('print-barcode', row)" />
            </el-tooltip>
            <el-tooltip content="사진 추가" placement="top">
              <el-button size="small" :icon="Picture" circle @click="$emit('open-photo', row)" />
            </el-tooltip>
            <el-tooltip content="재고소진" placement="top">
              <el-button size="small" :icon="Delete" type="danger" plain circle @click="$emit('delete', row)" />
            </el-tooltip>
          </template>
        </el-table-column>
      </base-table>
    </div>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="$emit('update:modelValue', false)" style="font-family: 'S-CoreDream', 'Jost', sans-serif;">닫기</el-button>
      </span>
    </template>
  </base-popup>
</template>

<script setup lang="ts">

import { useMobile } from '@/hooks/useMobile';
import { Delete, Printer, Picture } from '@element-plus/icons-vue';
import BaseTable from '@/components/BaseTable/index.vue';
import BasePopup from '@/components/BasePopup/index.vue';
const { isMobile } = useMobile();

defineProps({

  modelValue: {
    type: Boolean,
    required: true
  },

  selectedGroup: {
    type: Object,
    default: null
  },

  codeMap: {
    type: Object,
    default: () => ({})
  },

  formatDate: {
    type: Function,
    required: true
  },

  formatPrice: {
    type: Function,
    required: true
  },

  getStatusName: {
    type: Function,
    required: true
  },

  getStatusTagType: {
    type: Function,
    required: true
  }
});

defineEmits([
  'update:modelValue',

  'navigate-stock',

  'navigate-order',

  'print-barcode',

  'open-photo',

  'delete'
]);
</script>

<style lang="scss" src="./GroupDetailDialogStyles.scss" scoped></style>
