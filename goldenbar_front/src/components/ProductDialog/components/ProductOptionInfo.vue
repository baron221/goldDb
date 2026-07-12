<template>
<div>
    <el-row :gutter="20">
      <el-col :span="24">
        <el-form-item :label="$t('productDialog.sizes')" prop="sizes">
          <el-select
            v-model="localTemp.sizes"
            multiple
            :placeholder="$t('productDialog.sizesPlaceholder')"
            style="width: 100%"
          >
            <el-option
              v-for="item in productSizes"
              :key="item.code"
              :label="item.name"
              :value="item.code"
            />
          </el-select>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row :gutter="20">
      <el-col :xs="24" :sm="12">
        <el-form-item :label="$t('productDialog.productSize')" prop="productSize">
          <el-input v-model="localTemp.productSize" />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="12">
        <el-form-item :label="$t('productDialog.basicLoss')" prop="basicLoss">
          <el-input-number v-model="localTemp.basicLoss" :precision="2" :step="0.01" style="width: 100%;" />
        </el-form-item>
      </el-col>
    </el-row>

    <el-row v-if="localTemp.purity && localTemp.purity.length > 0 && localTemp.colors && localTemp.colors.length > 0" :gutter="20" style="margin-top: 0.9375rem; margin-bottom: 0.9375rem;">
      <el-col :span="24">
        <el-card shadow="never" style="border-radius: 2px; border: 1px solid #eae6df;">
          <template #header>
            <div style="display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 0.9375rem;">
              <div style="font-weight: 600; font-size: 0.9rem; text-transform: uppercase; letter-spacing: 0.5px;">
                {{ $t('productDialog.combinationTitle') }}
              </div>

              <div style="display: flex; align-items: center; gap: 0.5rem; background-color: #fbfaf7; border: 1px solid #e2ded8; padding: 0.25rem 0.625rem; border-radius: 2px;">
                <span style="font-size: 0.8875rem; font-weight: 600; color: #8a6d3b;">⚖️ {{ $t('productDialog.bulkConverter') }}</span>
                <el-select
                  :model-value="calcBasePurity"
                  @update:model-value="$emit('update:calcBasePurity', $event)"
                  :placeholder="$t('productDialog.basePurity')"
                  size="small"
                  style="width: 100px;"
                >
                  <el-option
                    v-for="purity in localTemp.purity"
                    :key="purity"
                    :label="getCodeName(purity)"
                    :value="purity"
                  />
                </el-select>
                <el-input-number
                  :model-value="calcBaseWeight"
                  @update:model-value="$emit('update:calcBaseWeight', $event)"
                  :precision="2"
                  :step="0.1"
                  :min="0"
                  size="small"
                  :placeholder="$t('productDialog.baseWeight')"
                  style="width: 110px;"
                />
                <span style="font-size: 0.8875rem; color: #606266;">{{ $t('productDialog.gBase') }}</span>
                <el-button type="warning" size="small" plain @click="$emit('apply-bulk-conversion')" style="background-color: #fdf6ec; border-color: #c5a880; color: #8a6d3b; font-weight: 600; padding: 0.25rem 0.5rem; height: 24px;">
                  {{ $t('productDialog.applyConversion') }}
                </el-button>
              </div>
            </div>
          </template>
          <base-table :data="localCombinationGridData" size="small" style="width: 100%;">
            <el-table-column
              :label="$t('productDialog.purity')"
              align="center"
              width="150"
              :excel-formatter="(row: any) => getCodeName(row.purity)"
            >
              <template #default="{ row }">
                <strong>{{ getCodeName(row.purity) }}</strong>
              </template>
            </el-table-column>
            <el-table-column
              :label="$t('productDialog.color')"
              align="center"
              width="180"
              :excel-formatter="(row: any) => getCodeName(row.color)"
            >
              <template #default="{ row }">
                <strong>{{ getCodeName(row.color) }}</strong>
              </template>
            </el-table-column>
            <el-table-column :label="$t('productDialog.weightG')" align="center">
              <template #default="{ row }">
                <el-input-number
                  v-model="row.weight"
                  :precision="2"
                  :step="0.01"
                  :min="0"
                  style="width: 100%;"
                />
              </template>
            </el-table-column>
          </base-table>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import useCodeStore from '@/store/modules/code';
import BaseTable from '@/components/BaseTable/index.vue';

const props = defineProps({
  temp: {
    type: Object,
    required: true
  },
  productSizes: {
    type: Array,
    required: true
  },
  combinationGridData: {
    type: Array,
    required: true
  },
  calcBasePurity: {
    type: String,
    required: true
  },
  calcBaseWeight: {
    type: Number,
    required: true
  }
});

const emit = defineEmits([
  'update:temp',
  'update:combinationGridData',
  'update:calcBasePurity',
  'update:calcBaseWeight',
  'apply-bulk-conversion'
]);

const codeStore = useCodeStore();

const localTemp = ref({ ...props.temp });
const localCombinationGridData = ref([...props.combinationGridData]);

watch(() => props.temp, (val) => {
  localTemp.value = { ...val };
}, { deep: true });

watch(localTemp, (val) => {
  emit('update:temp', val);
}, { deep: true });

watch(() => props.combinationGridData, (val) => {
  localCombinationGridData.value = [...val];
}, { deep: true });

watch(localCombinationGridData, (val) => {
  emit('update:combinationGridData', val);
}, { deep: true });

const getCodeName = (code: string): string => {
  return codeStore.getCodeName(code);
};
</script>

<style scoped>
</style>

