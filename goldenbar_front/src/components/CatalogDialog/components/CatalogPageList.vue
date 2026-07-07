<template>
<div>
    <div style="margin-bottom: 0.9375rem; display: flex; justify-content: space-between; align-items: center;">
      <span style="font-size: 0.875rem; color: #909399;">총 {{ temp.pages?.length || 0 }}개 페이지 등록됨</span>
      <div style="display: flex; gap: 0.5rem;">
        <el-button type="success" size="small" :icon="UploadFilled" @click="$emit('open-bulk')">대량 페이지 추가</el-button>
        <el-button type="primary" size="small" @click="$emit('add-page')">페이지 추가</el-button>
      </div>
    </div>

    <div class="page-card-container" ref="pageContainerRef">
      <div v-for="(page, index) in temp.pages" :key="page.id || page.photoUrl + index" class="page-compact-card">

        <div class="page-number-badge">{{ page.pageNumber }}p</div>

        <div class="page-card-image-wrapper">
          <el-image
            v-if="page.photoUrl"
            :src="getThumbUrl(page.photoUrl)"
            fit="cover"
            class="page-card-img"
          />
          <div v-else class="page-card-img-placeholder">
            <el-icon :size="20" style="color: #c0c4cc;"><Picture /></el-icon>
          </div>

          <div class="page-card-overlay">
            <div class="overlay-button-group">
              <el-button circle type="primary" size="small" :icon="Edit" title="수정" @click.stop="$emit('edit-page', { page, index })" />
              <el-button circle type="danger" size="small" :icon="Delete" title="삭제" @click.stop="$emit('remove-page', index)" />
            </div>
          </div>
        </div>

        <div class="page-card-info">
          <div class="page-card-desc" :title="page.description || '설명 없음'">
            {{ page.description || '설명 없음' }}
          </div>
          <div class="page-card-company" v-if="page.companyName" :title="page.companyName">
            {{ page.companyName }}
          </div>
          <div class="page-card-category" v-if="page.categoryLarge">
            {{ page.categoryLarge }}
            <span v-if="page.categoryMedium"> > {{ page.categoryMedium }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick, watch } from 'vue';
import { Delete, Edit, Picture, UploadFilled } from '@element-plus/icons-vue';
import Sortable from 'sortablejs';

const props = defineProps({
  temp: {
    type: Object,
    required: true
  },
  activeTab: {
    type: String,
    required: true
  }
});

const emit = defineEmits([
  'open-bulk',
  'add-page',
  'edit-page',
  'remove-page',
  'update-pages-order'
]);

const pageContainerRef = ref<HTMLElement | null>(null);
let sortableInstance: any = null;

const getThumbUrl = (url: string) => {
  if (!url) return '';
  if (url.startsWith('/uploads/')) {
    const parts = url.split('/');
    if (parts.length >= 4) {
      const fileName = parts.pop();
      return [...parts, 'thumb', fileName].join('/');
    }
  }
  return url;
};

const initSortable = () => {
  if (sortableInstance) {
    sortableInstance.destroy();
  }

  if (pageContainerRef.value) {
    sortableInstance = Sortable.create(pageContainerRef.value, {
      animation: 200,
      handle: '.page-compact-card',
      onEnd: (evt: any) => {
        const { oldIndex, newIndex } = evt;
        if (oldIndex !== undefined && newIndex !== undefined && oldIndex !== newIndex) {
          emit('update-pages-order', { oldIndex, newIndex });
        }
      }
    });
  }
};

watch(() => props.activeTab, (newTab) => {
  if (newTab === 'pages') {
    nextTick(() => {
      initSortable();
    });
  }
});

onMounted(() => {
  if (props.activeTab === 'pages') {
    initSortable();
  }
});

onUnmounted(() => {
  if (sortableInstance) {
    sortableInstance.destroy();
  }
});
</script>

