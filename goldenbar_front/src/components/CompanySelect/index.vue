<template>
<el-select
    v-model="internalValue"
    :placeholder="placeholder"
    :clearable="clearable"
    :filterable="filterable"
    :disabled="disabled"
    style="width: 100%"
    @change="handleChange"
    @visible-change="handleVisibleChange"
  >
    <el-option
      v-for="item in filteredCompanies"
      :key="item.id"
      :label="getLabel(item)"
      :value="item.id"
    >
      <div style="display: flex; justify-content: space-between; align-items: center;">
        <span>{{ item.name }}</span>
        <el-tag v-if="item.isDirectManagement" type="warning" size="small" effect="dark" style="border-radius: 0; font-size: 0.825rem; height: 18px; line-height: 18px;">직영</el-tag>
      </div>
    </el-option>
  </el-select>
</template>

<script setup>
import { ref, watch, onMounted, computed } from 'vue';
import { getCompanies } from '@/api/company';

const props = defineProps({
  modelValue: [Number, String],
  placeholder: {
    type: String,
    default: '업체 선택'
  },
  clearable: {
    type: Boolean,
    default: true
  },
  filterable: {
    type: Boolean,
    default: true
  },
  disabled: {
    type: Boolean,
    default: false
  },
  category: {
    type: String,
    default: ''
  },
  logisticsCompanyId: {
    type: [Number, String],
    default: null
  }
});

const emit = defineEmits(['update:modelValue', 'change']);

const internalValue = ref(props.modelValue);
const companies = ref([]);

const filteredCompanies = computed(() => {
  if (!props.logisticsCompanyId) return companies.value;
  return companies.value.filter(c => c.logisticsCompanyId === props.logisticsCompanyId);
});

watch(() => props.modelValue, (val) => {
  internalValue.value = val;
});

watch(() => props.logisticsCompanyId, () => {

  if (internalValue.value && !filteredCompanies.value.find(c => c.id === internalValue.value)) {
    emit('update:modelValue', undefined);
    internalValue.value = undefined;
  }
});

onMounted(() => {
  fetchCompanies();
});

const fetchCompanies = async () => {
  try {
    const params = { page: 1, pageSize: 1000 };
    if (props.category) {
      params.category = props.category;
    }
    const res = await getCompanies(params);
    companies.value = res.data.items || res.data;
  } catch (error) {
    console.error('Failed to fetch companies for select:', error);
  }
};

const getCompanyTypeName = (category) => {
  if (!category) return '업체';
  const map = {
    'RTL': '소매점',
    'MFG': '제조사',
    'DCC': '물류센터'
  };
  return map[category] || '업체';
};

const getLabel = (item) => {
  let label = item.name;
  if (item.isDirectManagement) label += ' [직영]';
  return label;
};

const handleChange = (val) => {
  emit('update:modelValue', val);
  const selected = companies.value.find(c => c.id === val);
  emit('change', val, selected);
};

const handleVisibleChange = (visible) => {
  if (visible) {
    fetchCompanies();
  }
};
</script>

