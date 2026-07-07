<template>
<el-input
    v-model="displayValue"
    :placeholder="placeholder"
    :disabled="disabled"
    class="amount-input"
    @focus="handleFocus"
    @blur="handleBlur"
  />
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';

const props = defineProps({
  modelValue: {
    type: Number,
    default: null
  },
  placeholder: {
    type: String,
    default: '금액을 입력하세요'
  },
  disabled: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits(['update:modelValue', 'focus', 'blur']);

const isFocused = ref(false);

const displayValue = computed({
  get() {
    const val = props.modelValue;
    if (isFocused.value) {

      return val === 0 || val === undefined || val === null ? '' : `${val}`;
    } else {

      return val === undefined || val === null ? '0' : `${val}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    }
  },
  set(val) {

    const cleanVal = val.replace(/[^\d]/g, '');
    const num = cleanVal ? Number(cleanVal) : 0;

    emit('update:modelValue', num);
  }
});

const handleFocus = (event: FocusEvent) => {
  isFocused.value = true;
  emit('focus', event);
};

const handleBlur = (event: FocusEvent) => {
  isFocused.value = false;
  emit('blur', event);
};
</script>

<style scoped>
.amount-input :deep(.el-input__inner) {
  text-align: right; 
}
</style>

