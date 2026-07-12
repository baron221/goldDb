<template>
<div>
    <el-divider content-position="left">{{ $t('productDialog.centerStoneTitle') }}</el-divider>
    <el-row :gutter="20">
      <el-col :xs="24" :sm="12">
        <el-form-item :label="$t('productDialog.centerStone')" prop="centerStone">
          <common-select v-model="localTemp.centerStone" group-code="CENTER_STONE" />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="12">
        <el-form-item :label="$t('productDialog.shape')" prop="centerStoneShape">
          <common-select v-model="localTemp.centerStoneShape" group-code="CENTER_STONE_SHAPE" />
        </el-form-item>
      </el-col>
    </el-row>
    <el-row :gutter="20">
      <el-col :xs="24" :sm="12">
        <el-form-item :label="$t('productDialog.size')" prop="centerStoneSize">
          <el-input v-model="localTemp.centerStoneSize" />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="12">
        <el-form-item :label="$t('productDialog.count')" prop="centerStoneCount">
          <el-input-number v-model="localTemp.centerStoneCount" :min="0" style="width: 100%;" />
        </el-form-item>
      </el-col>
    </el-row>

    <el-divider content-position="left">{{ $t('productDialog.sideStoneTitle') }}</el-divider>
    <el-row :gutter="20">
      <el-col :xs="24" :sm="12">
        <el-form-item :label="$t('productDialog.sideStone')" prop="sideStone">
          <common-select v-model="localTemp.sideStone" group-code="SUPPORT_STONE" />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="12">
        <el-form-item :label="$t('productDialog.shape')" prop="sideStoneShape">
          <common-select v-model="localTemp.sideStoneShape" group-code="SUPPORT_STONE_SHAPE" />
        </el-form-item>
      </el-col>
    </el-row>
    <el-row :gutter="20">
      <el-col :xs="24" :sm="12">
        <el-form-item :label="$t('productDialog.size')" prop="sideStoneSize">
          <el-input v-model="localTemp.sideStoneSize" />
        </el-form-item>
      </el-col>
      <el-col :xs="24" :sm="12">
        <el-form-item :label="$t('productDialog.count')" prop="sideStoneCount">
          <el-input-number v-model="localTemp.sideStoneCount" :min="0" style="width: 100%;" />
        </el-form-item>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import CommonSelect from '@/components/CommonSelect/index.vue';

const props = defineProps({
  temp: {
    type: Object,
    required: true
  }
});

const emit = defineEmits(['update:temp']);

const localTemp = ref({ ...props.temp });

watch(() => props.temp, (val) => {
  localTemp.value = { ...val };
}, { deep: true });

watch(localTemp, (val) => {
  emit('update:temp', val);
}, { deep: true });
</script>

<style scoped>
</style>

