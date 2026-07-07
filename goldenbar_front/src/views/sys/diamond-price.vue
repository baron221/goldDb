<template>
<div class="app-container">
    <el-card shadow="never">
      <template #header>
        <div class="card-header diamond-header">
          <span class="header-title">다이아몬드 시세 정보 관리</span>
          <div class="header-actions">
            <el-radio-group v-model="activePriceType" class="mobile-radio-group" @change="handleFilter">
              <el-radio-button value="MARKET">시세가</el-radio-button>
              <el-radio-button value="PURCHASE">매입가</el-radio-button>
              <el-radio-button value="SALE">판매가</el-radio-button>
            </el-radio-group>
            <div class="action-btns">
              <el-button @click="getList">새로고침</el-button>
              <el-button type="primary" @click="handleCreate">새 시세 등록</el-button>
            </div>
          </div>
        </div>
      </template>

      <base-table
        v-loading="listLoading"
        :data="list"
        highlight-current-row
        style="width: 100%;"
        @row-click="handleRowClick"
      >
        <el-table-column label="사이즈" align="center" header-align="center" width="100" prop="size">
          <template #default="{row}">
            <el-select v-if="row.id === selectedId" v-model="row.size" size="small" @change="handleUpdate(row)">
              <el-option v-for="i in 9" :key="i" :label="i + '부'" :value="i + '부'" />
            </el-select>
            <span v-else>{{ row.size }}</span>
          </template>
        </el-table-column>
        <el-table-column
          label="버진(FVVS1)"
          align="right"
          header-align="center"
          :excel-formatter="(row) => formatPrice(row.virginPrice)"
        >
          <template #default="{row}">
            <el-input-number v-if="row.id === selectedId" v-model="row.virginPrice" :min="0" size="small" controls-position="right" style="width: 100%;" @change="handleUpdate(row)" />
            <span v-else>{{ formatPrice(row.virginPrice) }}</span>
          </template>
        </el-table-column>
        <el-table-column
          label="우신(GVVS1)"
          align="right"
          header-align="center"
          :excel-formatter="(row) => formatPrice(row.wooshinPrice)"
        >
          <template #default="{row}">
            <el-input-number v-if="row.id === selectedId" v-model="row.wooshinPrice" :min="0" size="small" controls-position="right" style="width: 100%;" @change="handleUpdate(row)" />
            <span v-else>{{ formatPrice(row.wooshinPrice) }}</span>
          </template>
        </el-table-column>
        <el-table-column
          label="현대(GVVS1)"
          align="right"
          header-align="center"
          :excel-formatter="(row) => formatPrice(row.hyundaiPrice)"
        >
          <template #default="{row}">
            <el-input-number v-if="row.id === selectedId" v-model="row.hyundaiPrice" :min="0" size="small" controls-position="right" style="width: 100%;" @change="handleUpdate(row)" />
            <span v-else>{{ formatPrice(row.hyundaiPrice) }}</span>
          </template>
        </el-table-column>
        <el-table-column
          label="국제(GVVS1)"
          align="right"
          header-align="center"
          :excel-formatter="(row) => formatPrice(row.gukjePrice)"
        >
          <template #default="{row}">
            <el-input-number v-if="row.id === selectedId" v-model="row.gukjePrice" :min="0" size="small" controls-position="right" style="width: 100%;" @change="handleUpdate(row)" />
            <span v-else>{{ formatPrice(row.gukjePrice) }}</span>
          </template>
        </el-table-column>
        <el-table-column
          label="국보(GVVS1)"
          align="right"
          header-align="center"
          :excel-formatter="(row) => formatPrice(row.gukboPrice)"
        >
          <template #default="{row}">
            <el-input-number v-if="row.id === selectedId" v-model="row.gukboPrice" :min="0" size="small" controls-position="right" style="width: 100%;" @change="handleUpdate(row)" />
            <span v-else>{{ formatPrice(row.gukboPrice) }}</span>
          </template>
        </el-table-column>
        <el-table-column
          label="기타(GVVS1)"
          align="right"
          header-align="center"
          :excel-formatter="(row) => formatPrice(row.otherPrice)"
        >
          <template #default="{row}">
            <el-input-number v-if="row.id === selectedId" v-model="row.otherPrice" :min="0" size="small" controls-position="right" style="width: 100%;" @change="handleUpdate(row)" />
            <span v-else>{{ formatPrice(row.otherPrice) }}</span>
          </template>
        </el-table-column>
        <el-table-column
          label="고시 시간"
          align="center"
          header-align="center"
          width="220"
          :excel-formatter="(row) => formatDateTime(row.announcedAt)"
        >
          <template #default="{row}">
            <el-date-picker
              v-if="row.id === selectedId"
              v-model="row.announcedAt"
              type="datetime"
              placeholder="선택"
              size="small"
              style="width: 100%;"
              @change="handleUpdate(row)"
            />
            <span v-else>{{ formatDateTime(row.announcedAt) }}</span>
          </template>
        </el-table-column>
        <el-table-column label="관리" align="center" header-align="center" width="120" class-name="small-padding fixed-width">
          <template #default="{row}">
            <el-tooltip content="복사하여 새로 등록" placement="top">
              <el-button link class="copy-icon-btn" size="small" :icon="CopyDocument" @click.stop="handleCopy(row)" />
            </el-tooltip>
            <el-tooltip content="삭제" placement="top">
              <el-button link class="delete-icon-btn" size="small" :icon="Delete" @click.stop="handleDelete(row)" />
            </el-tooltip>
          </template>
        </el-table-column>
      </base-table>
    </el-card>

    <diamond-price-dialog
      v-model="dialogFormVisible"
      :is-edit="isEdit"
      :initial-data="currentData"
      @success="getList"
    />
  </div>
</template>

<script setup lang="ts">

import { ref, onMounted } from 'vue';
import {
  getDiamondPrices,
  deleteDiamondPrice,
  updateDiamondPrice,
  createDiamondPrice
} from '@/api/diamond-price';
import { ElMessageBox, ElMessage } from 'element-plus';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import { CopyDocument, Delete } from '@element-plus/icons-vue';
import DiamondPriceDialog from './components/DiamondPriceDialog.vue';

const list = ref([]);
const listLoading = ref(true);
const activePriceType = ref('MARKET');
const dialogFormVisible = ref(false);
const isEdit = ref(false);
const currentData = ref<any>({});
const selectedId = ref<number | null>(null);

const getList = async () => {
  listLoading.value = true;
  try {
    const response = await getDiamondPrices({ priceType: activePriceType.value });
    list.value = response.data;
  } catch (error) {
    console.error(error);
  } finally {
    listLoading.value = false;
  }
};

const handleRowClick = (row: any) => {
  selectedId.value = row.id;
};

const handleFilter = () => {
  getList();
};

const formatDateTime = (date: string) => {
  if (!date) return '-';
  return parseTime(date, '{y}-{m}-{d} {h}:{i}:{s}');
};

const handleCreate = () => {
  isEdit.value = false;
  currentData.value = {
    priceType: activePriceType.value,
    size: '1부',
    announcedAt: new Date(),
    virginPrice: 0,
    wooshinPrice: 0,
    hyundaiPrice: 0,
    gukjePrice: 0,
    gukboPrice: 0,
    otherPrice: 0
  };
  dialogFormVisible.value = true;
};

const handleUpdate = async (row: any) => {
  try {
    await updateDiamondPrice(row.id, {
      priceType: row.priceType,
      size: row.size,
      virginPrice: row.virginPrice,
      wooshinPrice: row.wooshinPrice,
      hyundaiPrice: row.hyundaiPrice,
      gukjePrice: row.gukjePrice,
      gukboPrice: row.gukboPrice,
      otherPrice: row.otherPrice,
      announcedAt: row.announcedAt
    });
    ElMessage.success('수정되었습니다.');
  } catch (error) {
    console.error(error);
  }
};

const handleCopy = async (row: any) => {
  try {
    await createDiamondPrice({
      priceType: row.priceType,
      size: row.size,
      virginPrice: row.virginPrice,
      wooshinPrice: row.wooshinPrice,
      hyundaiPrice: row.hyundaiPrice,
      gukjePrice: row.gukjePrice,
      gukboPrice: row.gukboPrice,
      otherPrice: row.otherPrice,
      announcedAt: new Date()
    });
    ElMessage.success('복사되었습니다.');
    getList();
  } catch (error) {
    console.error(error);
  }
};

const handleDelete = (row: any) => {
  ElMessageBox.confirm('정말로 이 시세 정보를 삭제하시겠습니까?', '경고', {
    confirmButtonText: '삭제',
    cancelButtonText: '취소',
    type: 'warning'
  }).then(async () => {
    try {
      await deleteDiamondPrice(row.id);
      ElMessage.success('삭제되었습니다.');
      getList();
    } catch (error) {
      console.error(error);
    }
  });
};

onMounted(() => {
  getList();
});
</script>

<style scoped lang="scss">
.card-header {
  font-weight: bold;
}
.header-actions {
  display: flex;
  align-items: center;
  gap: 10px;
}
.action-btns {
  display: flex;
  align-items: center;
  gap: 10px;
}

@media (max-width: 768px) {
  .diamond-header {
    flex-direction: column;
    align-items: stretch !important;
  }
  .header-title {
    margin-bottom: 10px;
    text-align: center;
  }
  .header-actions {
    flex-direction: column;
    align-items: stretch;
  }
  .mobile-radio-group {
    display: flex;
    width: 100%;
    :deep(.el-radio-button) {
      flex: 1;
      .el-radio-button__inner {
        width: 100%;
        padding: 8px 0;
      }
    }
  }
  .action-btns {
    display: flex;
    width: 100%;
    .el-button {
      flex: 1;
      margin: 0 !important;
    }
  }
}
</style>

