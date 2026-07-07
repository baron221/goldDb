<template>
  <el-select
    v-model="internalValue"
    :placeholder="placeholder"
    :clearable="clearable"
    :disabled="disabled"
    :multiple="multiple"
    @change="handleChange"
    style="width: 100%"
  >
    <el-option
      v-if="showAll"
      label="전체"
      value=""
    />
    <el-option
      v-for="item in localOptions"
      :key="item.code"
      :label="item.name"
      :value="item.code"
    />
  </el-select>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import useCodeStore from '@/store/modules/code';

const codeStore = useCodeStore();

const props = defineProps({
  modelValue: {
    type: [String, Number, Array],
    default: null
  },
  groupCode: {
    type: String,
    default: ''
  },
  parentId: {
    type: Number,
    default: null
  },
  options: {
    type: Array,
    default: null
  },
  placeholder: {
    type: String,
    default: '선택해 주세요'
  },
  clearable: {
    type: Boolean,
    default: true
  },
  disabled: {
    type: Boolean,
    default: false
  },
  multiple: {
    type: Boolean,
    default: false
  },
  showAll: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits(['update:modelValue', 'change', 'options-loaded']);

const internalValue = ref(props.modelValue);
const localOptions = ref([]);

const fetchOptions = async () => {
  if (props.options) {
    localOptions.value = props.options;
    emit('options-loaded', localOptions.value);
    return;
  }

  await codeStore.fetchCodes();

  if (props.parentId) {
    localOptions.value = codeStore.getCodesByParentIdStore(props.parentId);
  } else if (props.groupCode) {
    localOptions.value = codeStore.getCodesByGroupStore(props.groupCode);
  } else {
    localOptions.value = [];
    return;
  }
  emit('options-loaded', localOptions.value);
};

onMounted(() => {

  fetchOptions();
});

watch(() => props.modelValue, (newVal) => {

  internalValue.value = newVal;
});

watch(() => props.groupCode, () => {

  fetchOptions();
});

watch(() => props.parentId, (newVal) => {
  if (props.options) return; 

  if (newVal) {
    fetchOptions();
  } else {

    localOptions.value = [];
    internalValue.value = null;
  }
});

watch(() => props.options, (newVal) => {

  if (newVal) {
    localOptions.value = newVal;
    emit('options-loaded', localOptions.value);
  }
}, { immediate: true });

const handleChange = (val) => {

  emit('update:modelValue', val);
  emit('change', val, localOptions.value);
};
</script>
