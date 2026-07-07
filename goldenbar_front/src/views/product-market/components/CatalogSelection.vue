<template>
<div class="catalog-selection-container">
    <div class="selection-header">
      <h2 class="page-title">E-CATALOGUE</h2>
      <p class="page-subtitle">디지털 책자를 넘겨보며 원하시는 상품을 즉시 장바구니에 담아보세요.</p>
    </div>

    <div v-loading="loading" class="catalog-grid">
      <div
        v-for="catalog in catalogList"
        :key="catalog.id"
        class="catalog-book-card"
        @click="selectCatalog(catalog.id)"
      >
        <div class="book-cover-wrapper">
          <img v-if="catalog.photoUrl" :src="getApiBaseUrl() + catalog.photoUrl" class="book-cover-img" alt="cover" />
          <div v-else class="book-cover-placeholder">
            <span class="placeholder-title">{{ catalog.title }}</span>
          </div>
          <div class="book-spine"></div>
          <div class="book-cover-overlay">
            <span class="view-btn">책자 열기</span>
          </div>
        </div>
        <div class="book-info">
          <span class="security-badge" :class="catalog.securityClass === '보안' ? 'secure' : 'general'">
            {{ catalog.securityClass }}
          </span>
          <h3 class="book-title">{{ catalog.title }}</h3>
          <div class="book-meta">
            <span class="issue-no">{{ catalog.issueNo }}</span>
            <span class="page-count">{{ catalog.totalPages }} Pages</span>
          </div>
        </div>
      </div>
    </div>

    <div v-if="catalogList.length === 0 && !loading" class="empty-catalogs">
      <el-icon :size="60" color="#c5a880"><Document /></el-icon>
      <p>등록된 카탈로그 책자가 존재하지 않습니다.</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Document } from '@element-plus/icons-vue';

defineProps({
  catalogList: {
    type: Array as () => any[],
    required: true
  },
  loading: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits(['select']);

const selectCatalog = (id: number) => {
  emit('select', id);
};

const getApiBaseUrl = () => {
  return import.meta.env.VITE_APP_BASE_API || '';
};
</script>

<style lang="scss" scoped>
.catalog-selection-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2.5rem 1rem;
}

.selection-header {
  text-align: center;
  margin-bottom: 3.5rem;

  .page-title {
    font-size: 2.25rem;
    font-weight: 700;
    letter-spacing: 4px;
    color: #111111;
    margin: 0 0 0.5rem 0;
    position: relative;
    display: inline-block;

    &::after {
      content: '';
      position: absolute;
      bottom: -8px;
      left: 50%;
      transform: translateX(-50%);
      width: 40px;
      height: 2px;
      background-color: #c5a880;
    }
  }

  .page-subtitle {
    margin-top: 1rem;
    font-size: 0.95rem;
    color: #888888;
  }
}

.catalog-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 2.5rem;
}

.catalog-book-card {
  cursor: pointer;
  transition: transform 0.4s cubic-bezier(0.165, 0.84, 0.44, 1), box-shadow 0.4s ease;

  &:hover {
    transform: translateY(-8px);

    .book-cover-overlay {
      opacity: 1;
    }

    .book-spine {
      box-shadow: 2px 0 10px rgba(0, 0, 0, 0.2);
    }
  }
}

.book-cover-wrapper {
  position: relative;
  width: 100%;
  aspect-ratio: 1 / 1.414;
  background-color: #f5f4f0;
  border-radius: 2px;
  overflow: hidden;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.08);
  perspective: 1000px;
}

.book-cover-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.book-cover-placeholder {
  width: 100%;
  height: 100%;
  background: linear-gradient(135deg, #1e1e1e 0%, #2e2e2e 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1.5rem;
  box-sizing: border-box;

  .placeholder-title {
    color: #c5a880;
    font-size: 1.15rem;
    font-weight: 600;
    text-align: center;
    letter-spacing: 1px;
  }
}

.book-spine {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 15px;
  background: linear-gradient(to right, rgba(0,0,0,0.3) 0%, rgba(255,255,255,0.05) 50%, rgba(0,0,0,0.15) 100%);
  border-top-left-radius: 4px;
  border-bottom-left-radius: 4px;
  transition: box-shadow 0.4s ease;
}

.book-cover-overlay {
  position: absolute;
  inset: 0;
  background-color: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(2px);
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: opacity 0.3s ease;

  .view-btn {
    border: 1px solid #c5a880;
    color: #c5a880;
    background-color: rgba(0, 0, 0, 0.6);
    padding: 0.625rem 1.25rem;
    font-size: 0.9rem;
    font-weight: 500;
    letter-spacing: 1px;
    border-radius: 2px;
  }
}

.book-info {
  margin-top: 1rem;
  padding: 0 0.25rem;
}

.security-badge {
  display: inline-block;
  font-size: 0.725rem;
  padding: 0.15rem 0.45rem;
  border-radius: 2px;
  font-weight: 600;
  letter-spacing: 0.5px;
  margin-bottom: 0.5rem;

  &.secure {
    background-color: rgba(245, 108, 108, 0.1);
    color: #f56c6c;
    border: 1px solid rgba(245, 108, 108, 0.2);
  }

  &.general {
    background-color: rgba(103, 194, 58, 0.1);
    color: #67c23a;
    border: 1px solid rgba(103, 194, 58, 0.2);
  }
}

.book-title {
  font-size: 1.05rem;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0 0 0.35rem 0;
  line-height: 1.4;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.book-meta {
  display: flex;
  justify-content: space-between;
  font-size: 0.825rem;
  color: #888888;
  font-weight: 500;
}

.empty-catalogs {
  text-align: center;
  padding: 5rem 0;
  color: #a0a0a0;
  font-size: 0.95rem;

  p {
    margin-top: 1rem;
  }
}
</style>

