<template>
<div class="app-container">
    <el-card shadow="never">
      <template #header>
        <div class="card-header" style="display: flex; justify-content: space-between; align-items: center;">
          <span>일반 시세 정보 관리</span>
          <div>
            <el-button @click="getList">새로고침</el-button>
            <el-button type="primary" @click="handleCreate">새 시세 등록</el-button>
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
        <el-table-column
          label="순금"
          align="right"
          header-align="center"
          :excel-formatter="(row) => formatPrice(row.pureGold) + '원'"
        >
          <template #default="{row}">
            <el-input-number
              v-if="row.id === selectedId"
              v-model="row.pureGold"
              :min="0"
              :step="1000"
              size="small"
              controls-position="right"
              class="inline-edit-input"
              @change="handleUpdate(row)"
            />
            <span v-else>{{ formatPrice(row.pureGold) }}원</span>
          </template>
        </el-table-column>
        <el-table-column
          label="18K"
          align="right"
          header-align="center"
          :excel-formatter="(row) => formatPrice(row.gold18K) + '원'"
        >
          <template #default="{row}">
            <el-input-number
              v-if="row.id === selectedId"
              v-model="row.gold18K"
              :min="0"
              :step="1000"
              size="small"
              controls-position="right"
              class="inline-edit-input"
              @change="handleUpdate(row)"
            />
            <span v-else>{{ formatPrice(row.gold18K) }}원</span>
          </template>
        </el-table-column>
        <el-table-column
          label="14K"
          align="right"
          header-align="center"
          :excel-formatter="(row) => formatPrice(row.gold14K) + '원'"
        >
          <template #default="{row}">
            <el-input-number
              v-if="row.id === selectedId"
              v-model="row.gold14K"
              :min="0"
              :step="1000"
              size="small"
              controls-position="right"
              class="inline-edit-input"
              @change="handleUpdate(row)"
            />
            <span v-else>{{ formatPrice(row.gold14K) }}원</span>
          </template>
        </el-table-column>
        <el-table-column
          label="백금"
          align="right"
          header-align="center"
          :excel-formatter="(row) => formatPrice(row.platinum) + '원'"
        >
          <template #default="{row}">
            <el-input-number
              v-if="row.id === selectedId"
              v-model="row.platinum"
              :min="0"
              :step="1000"
              size="small"
              controls-position="right"
              class="inline-edit-input"
              @change="handleUpdate(row)"
            />
            <span v-else>{{ formatPrice(row.platinum) }}원</span>
          </template>
        </el-table-column>
        <el-table-column
          label="실버"
          align="right"
          header-align="center"
          :excel-formatter="(row) => formatPrice(row.silver) + '원'"
        >
          <template #default="{row}">
            <el-input-number
              v-if="row.id === selectedId"
              v-model="row.silver"
              :min="0"
              :step="1000"
              size="small"
              controls-position="right"
              class="inline-edit-input"
              @change="handleUpdate(row)"
            />
            <span v-else>{{ formatPrice(row.silver) }}원</span>
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

    <GoldPriceCreateDialog
      v-model="dialogFormVisible"
      @saved="getList"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { getGoldPrices, updateGoldPrice, deleteGoldPrice } from '@/api/gold-price';
import { ElMessageBox, ElMessage } from 'element-plus';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import { CopyDocument, Delete } from '@element-plus/icons-vue';
import GoldPriceCreateDialog from './components/GoldPriceCreateDialog.vue';

const list = ref([]);
const listLoading = ref(true);
const selectedId = ref<number | null>(null);
const dialogFormVisible = ref(false);

const getList = async () => {
  listLoading.value = true;
  try {
    const response = await getGoldPrices();
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

const formatDateTime = (date: string) => {
  if (!date) return '-';
  return parseTime(date, '{y}-{m}-{d} {h}:{i}:{s}');
};

const handleCreate = () => {
  dialogFormVisible.value = true;
};

const handleUpdate = async (row: any) => {
  try {
    await updateGoldPrice(row.id, {
      announcedAt: row.announcedAt,
      pureGold: row.pureGold,
      gold18K: row.gold18K,
      gold14K: row.gold14K,
      platinum: row.platinum,
      silver: row.silver
    });
    ElMessage.success('수정되었습니다.');
  } catch (error) {
    console.error(error);
  }
};

const handleCopy = async (row: any) => {
  try {
    const { createGoldPrice } = await import('@/api/gold-price');
    await createGoldPrice({
      announcedAt: new Date(),
      pureGold: row.pureGold,
      gold18K: row.gold18K,
      gold14K: row.gold14K,
      platinum: row.platinum,
      silver: row.silver
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
      await deleteGoldPrice(row.id);
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

<style scoped>
.card-header {
  font-weight: bold;
}
</style>

