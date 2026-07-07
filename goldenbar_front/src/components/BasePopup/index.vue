<template>
  <el-dialog
    v-model="visible"
    :title="title"
    :width="width"
    :fullscreen="fullscreen"
    :top="top"
    :modal="modal"
    :close-on-click-modal="closeOnClickModal"
    :close-on-press-escape="closeOnPressEscape"
    :show-close="showClose"
    :before-close="beforeClose"
    :draggable="true"
    :append-to-body="appendToBody"
    destroy-on-close
    @close="handleClose"
    class="base-popup-dialog"
  >

    <template #header v-if="$slots.header">
      <slot name="header" />
    </template>

    <slot />

    <template #footer v-if="$slots.footer">
      <slot name="footer" />
    </template>
  </el-dialog>
</template>

<script>

const openPopups = [];
</script>

<script setup>
import { computed, watch, onMounted, onUnmounted } from 'vue';

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  title: {
    type: String,
    default: ''
  },
  width: {
    type: String,
    default: '50%'
  },
  fullscreen: {
    type: Boolean,
    default: false
  },
  top: {
    type: String,
    default: '15vh'
  },
  modal: {
    type: Boolean,
    default: true
  },
  closeOnClickModal: {
    type: Boolean,
    default: false
  },
  closeOnPressEscape: {
    type: Boolean,
    default: true
  },
  showClose: {
    type: Boolean,
    default: true
  },
  beforeClose: {
    type: Function,
    default: null
  },
  appendToBody: {
    type: Boolean,
    default: true
  }
});

const emit = defineEmits(['update:modelValue', 'close']);

const visible = computed({
  get() {
    return props.modelValue;
  },
  set(val) {
    emit('update:modelValue', val);
  }
});

const popupId = `base-popup-${Math.random().toString(36).substr(2, 9)}`;
let poppedBySelf = false;

const onKeyDown = (event) => {
  if (event.key === 'Escape' && props.closeOnPressEscape && visible.value) {
    if (openPopups[openPopups.length - 1] === popupId) {
      visible.value = false;
      event.stopPropagation();
    }
  }
};

const onPopState = (event) => {
  if (visible.value) {
    const newStateId = event.state?.popupId;
    const myIndex = openPopups.indexOf(popupId);
    const newTopIndex = openPopups.indexOf(newStateId);

    if (myIndex > -1 && myIndex > newTopIndex) {
      poppedBySelf = true;
      visible.value = false;
    }
  }
};

watch(visible, (newVal) => {
  if (newVal) {
    if (!openPopups.includes(popupId)) {
      openPopups.push(popupId);
    }
    window.history.pushState({ popupId }, '');
    window.addEventListener('popstate', onPopState);
    window.addEventListener('keydown', onKeyDown, true);
  } else {
    const index = openPopups.indexOf(popupId);
    if (index > -1) {
      openPopups.splice(index, 1);
    }
    window.removeEventListener('popstate', onPopState);
    window.removeEventListener('keydown', onKeyDown, true);

    if (!poppedBySelf) {
      if (window.history.state && window.history.state.popupId === popupId) {
        window.history.back();
      }
    }
    poppedBySelf = false;
  }
});

onMounted(() => {
  if (visible.value) {
    if (!openPopups.includes(popupId)) {
      openPopups.push(popupId);
    }
    window.history.pushState({ popupId }, '');
    window.addEventListener('popstate', onPopState);
    window.addEventListener('keydown', onKeyDown, true);
  }
});

onUnmounted(() => {
  const index = openPopups.indexOf(popupId);
  if (index > -1) {
    openPopups.splice(index, 1);
  }
  window.removeEventListener('popstate', onPopState);
  window.removeEventListener('keydown', onKeyDown, true);

  if (visible.value && !poppedBySelf) {
    if (window.history.state && window.history.state.popupId === popupId) {
      window.history.back();
    }
  }
});

const handleClose = () => {
  emit('close');
};
</script>

<style lang="scss" src="./BasePopupStyles.scss" scoped></style>
