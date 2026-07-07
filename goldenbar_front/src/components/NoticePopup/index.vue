<template>
<base-popup
    draggable
    v-model="visible"
    :title="currentNotice.title || $t('legal.termsTitle')"
    width="50%"
    append-to-body
    :close-on-click-modal="false"
    :close-on-press-escape="true"
    :show-close="true"
    class="notice-dialog"
  >
    <div v-if="!isPreview" class="dont-show-today-outside">
      <el-checkbox v-model="dontShowToday" @change="handleDontShowChange">{{ $t('notice.dontShow') }}</el-checkbox>
    </div>
    <div class="notice-content" v-html="currentNotice.content"></div>
    <template #footer>
      <div class="dialog-footer">
        <div>
          <div v-if="!isPreview" class="dont-show-today-mobile">
            <el-checkbox v-model="dontShowToday" @change="handleDontShowChange">{{ $t('notice.dontShow') }}</el-checkbox>
          </div>
        </div>
        <div class="nav-actions">
          <template v-if="totalCount > 1">
            <el-button
              circle
              :disabled="currentIndex <= 0"
              @click="handlePrev"
            >
              <el-icon><ArrowLeft /></el-icon>
            </el-button>
            <span class="page-info">{{ currentIndex + 1 }} / {{ totalCount }}</span>
            <el-button
              circle
              :disabled="currentIndex >= totalCount - 1"
              @click="handleNext"
            >
              <el-icon><ArrowRight /></el-icon>
            </el-button>
          </template>
          <el-button
            class="mobile-close-btn"
            type="primary"
            plain
            @click="visible = false"
          >
            {{ $t('notice.close') }}
          </el-button>
        </div>
      </div>
    </template>
  </base-popup>
</template>

<script setup lang="ts">

import { ref, watch } from 'vue';
import BasePopup from '@/components/BasePopup/index.vue';
import { ArrowLeft, ArrowRight } from '@element-plus/icons-vue';

const props = defineProps({
  modelValue: Boolean,
  notice: {
    type: Object,
    default: () => ({})
  },
  isPreview: {
    type: Boolean,
    default: false
  },
  currentIndex: {
    type: Number,
    default: 0
  },
  totalCount: {
    type: Number,
    default: 0
  }
});

const emit = defineEmits(['update:modelValue', 'close', 'prev', 'next']);

const visible = ref(false);

const currentNotice = ref<any>({});
const dontShowToday = ref(false);

watch(() => props.modelValue, (val) => {
  visible.value = val;
  if (val) dontShowToday.value = false; 
});

watch(visible, (val) => {
  emit('update:modelValue', val);
  if (!val) {
    saveHideSetting(); 
  }
});

watch(() => props.notice, (val) => {
  currentNotice.value = val;
}, { immediate: true });

const handlePrev = () => {
  saveHideSetting();
  emit('prev');
};

const handleNext = () => {
  saveHideSetting();
  emit('next');
};

const handleDontShowChange = (val: boolean) => {
  if (val) {
    saveHideSetting();
    visible.value = false;
  }
};

const saveHideSetting = () => {
  if (dontShowToday.value && !props.isPreview) {
    const expiry = new Date().getTime() + 24 * 60 * 60 * 1000;
    localStorage.setItem(`notice_hide_${currentNotice.value.id}`, expiry.toString());
  }
};
</script>

<style lang="scss">

.notice-dialog {
  overflow: visible !important;
  border-radius: 2px;

  .el-dialog__headerbtn {
    top: -32px !important;
    right: 1px !important; // Adjusted to right end
    width: 30px;
    height: 30px;
    background: transparent;
    transition: all 0.3s;
    display: flex;
    align-items: center;
    justify-content: center;

    &:hover {
      transform: rotate(90deg);
      .el-dialog__close {
        color: #ff4d4f !important;
      }
    }

    .el-dialog__close {
      color: #fff !important;
      font-size: 1.75rem !important;
      font-weight: bold;
    }
  }

  .dont-show-today-outside {
    position: absolute;
    top: -35px;
    left: 10px;
    z-index: 10;

    @media (max-width: 768px) {
      display: none !important;
    }

    .el-checkbox__label {
      color: #fff !important;
      font-size: 0.9rem;
      font-weight: 500;
    }

    .el-checkbox__inner {
      background-color: rgba(255, 255, 255, 0.2);
      border-color: #fff;
    }

    .el-checkbox__input.is-checked .el-checkbox__inner {
      background-color: #409EFF;
      border-color: #409EFF;
    }
  }

  // Remove padding from header to make it cleaner
  .el-dialog__header {
    padding-bottom: 0.625rem;
  }

  @media (max-width: 768px) {
    width: 100% !important;
    max-width: 100% !important;
    margin: 0 !important;
    border-radius: 0 !important;
    max-height: calc(100vh - 80px) !important;
    display: flex !important;
    flex-direction: column !important;

    .el-dialog__headerbtn {
      right: 10px !important;
    }

    .el-dialog__body {
      flex: 1 !important;
      overflow: hidden !important;
      display: flex !important;
      flex-direction: column !important;
      padding: 10px !important;
    }

    .notice-content {
      flex: 1 !important;
      max-height: none !important;
      overflow-y: auto !important;
    }
  }
}

.el-overlay-dialog:has(> .notice-dialog) {
  @media (max-width: 768px) {
    display: flex !important;
    align-items: center !important;
    justify-content: center !important;
    padding: 40px 0 !important;
    margin: 0 !important;
  }
}
</style>

<style scoped>
.notice-content {
  min-height: 200px;
  max-height: 500px;
  overflow-y: auto;
  padding: 0.625rem;
  line-height: 1.6;
}
.notice-content :deep(img) {
  max-width: 100%;
  height: auto;
}
.dialog-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.nav-actions {
  display: flex;
  align-items: center;
  gap: 0.625rem;
}
.page-info {
  font-size: 0.875rem;
  color: #909399;
  margin: 0 0.625rem;
}
.dont-show-today-mobile {
  display: none;
}
.mobile-close-btn {
  display: none;
}
@media (max-width: 768px) {
  .dont-show-today-mobile {
    display: block;
  }
  .mobile-close-btn {
    display: inline-flex;
  }
}
</style>

