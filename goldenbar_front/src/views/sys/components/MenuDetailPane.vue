<template>
<el-card v-if="currentMenu" shadow="never" class="detail-card">
    <template #header>
      <div class="card-header">
        <span>상세 설정: <el-tag>{{ localTemp.title || '신규 메뉴' }}</el-tag></span>
        <div class="header-right-actions">
          <template v-if="currentMenu && currentMenu.id">
            <el-button v-permission="'create'" type="primary" size="small" plain @click="$emit('create-sibling', currentMenu)">형제 추가</el-button>
            <el-button v-permission="'create'" type="primary" size="small" plain @click="$emit('create-sub', currentMenu)">하위 추가</el-button>
          </template>
          <el-button v-permission="'save'" type="success" size="small" @click="$emit('save')">최종 저장</el-button>
        </div>
      </div>
    </template>

    <el-tabs v-model="activeDetailTab">

      <el-tab-pane label="기본 정보" name="basic">
        <el-form :model="localTemp" label-width="100px" style="margin-top: 1.25rem;">
          <el-form-item label="상위 메뉴">
            <el-select v-model="localTemp.parentId" placeholder="루트 (최상위)" style="width: 100%" filterable clearable>
              <el-option :key="0" label="루트 (최상위)" value="" />
              <el-option v-for="item in flatMenuList" :key="item.id" :label="item.title" :value="item.id" />
            </el-select>
          </el-form-item>
          <el-row :gutter="20">
            <el-col :xs="24" :sm="12">
              <el-form-item label="메뉴명">
                <el-input v-model="localTemp.title" />
              </el-form-item>
            </el-col>
            <el-col :xs="24" :sm="12">
              <el-form-item label="아이콘">
                <el-popover placement="bottom" :width="500" trigger="click">
                  <template #reference>
                    <el-input v-model="localTemp.icon" placeholder="아이콘 선택" readonly style="cursor: pointer;">
                      <template #prefix>
                        <template v-if="localTemp.icon">
                          <el-icon v-if="elementIconList.includes(localTemp.icon)">
                            <component :is="elementIconComponents[localTemp.icon]" />
                          </el-icon>
                          <svg-icon v-else :icon-class="localTemp.icon" />
                        </template>
                      </template>
                      <template #suffix>
                        <i v-if="localTemp.icon" class="fas fa-circle-xmark" style="cursor: pointer;" @click.stop="localTemp.icon = ''"></i>
                      </template>
                    </el-input>
                  </template>
                  <div class="icon-picker-container">
                    <el-tabs v-model="activeIconTab">
                      <el-tab-pane label="SVG Icons" name="svg">
                        <div class="icon-picker">
                          <div
                            v-for="icon in svgIconList"
                            :key="icon"
                            class="icon-item"
                            :class="{ active: localTemp.icon === icon }"
                            @click="localTemp.icon = icon"
                          >
                            <svg-icon :icon-class="icon" />
                            <span class="icon-name">{{ icon }}</span>
                          </div>
                        </div>
                      </el-tab-pane>
                      <el-tab-pane label="Element Icons" name="element">
                        <div class="icon-picker">
                          <div
                            v-for="icon in elementIconList"
                            :key="icon"
                            class="icon-item"
                            :class="{ active: localTemp.icon === icon }"
                            @click="localTemp.icon = icon"
                          >
                            <el-icon>
                              <component :is="elementIconComponents[icon]" />
                            </el-icon>
                            <span class="icon-name">{{ icon }}</span>
                          </div>
                        </div>
                      </el-tab-pane>
                    </el-tabs>
                  </div>
                </el-popover>
              </el-form-item>
            </el-col>
          </el-row>
          <el-form-item label="라우트 경로">
            <el-input v-model="localTemp.path" placeholder="/example" />
          </el-form-item>
          <el-form-item label="컴포넌트">
            <el-input v-model="localTemp.component" placeholder="Layout 또는 views/..." />
          </el-form-item>
        </el-form>
      </el-tab-pane>

      <el-tab-pane label="고급 설정" name="advanced">
        <el-form :model="localTemp" label-width="100px" style="margin-top: 1.25rem;">
          <el-form-item label="라우트 이름">
            <el-input v-model="localTemp.name" placeholder="UserManagement" />
          </el-form-item>
          <el-form-item label="리다이렉트">
            <el-input v-model="localTemp.redirect" />
          </el-form-item>
          <el-row :gutter="20">
            <el-col :xs="24" :sm="12">
              <el-form-item label="정렬 순서">
                <el-input-number v-model="localTemp.sortOrder" :min="0" />
              </el-form-item>
            </el-col>

            <el-col :xs="24" :sm="12">
              <el-form-item label="설정 플래그">
                <el-tooltip content="라우트 경로에는 등록하되, 왼쪽 사이드바 메뉴 목록에서는 보이지 않도록 숨깁니다." placement="top">
                  <el-checkbox v-model="localTemp.hidden">사이드바 숨김</el-checkbox>
                </el-tooltip>
                <el-tooltip content="하위 메뉴가 1개만 있어도 상위 부모 폴더 메뉴를 생략하지 않고 항상 드롭다운 트리 구조로 노출합니다." placement="top">
                  <el-checkbox v-model="localTemp.alwaysShow">항상 노출</el-checkbox>
                </el-tooltip>
                <el-tooltip content="상단 탭 바(TagsView)에 탭을 고정하여, '전체 닫기'나 '다른 탭 닫기' 시에도 닫히지 않고 항상 켜두도록 설정합니다." placement="top">
                  <el-checkbox v-model="localTemp.affix">탭 고정</el-checkbox>
                </el-tooltip>
                <el-tooltip content="이전 화면의 컴포넌트 상태를 캐싱하여, 다른 페이지로 갔다가 다시 돌아와도 입력 값이나 스크롤 위치 등을 보존합니다." placement="top">
                  <el-checkbox v-model="localTemp.keepAlive">화면 유지</el-checkbox>
                </el-tooltip>
                <el-tooltip content="모바일 기기 및 해상도로 접속 시에도 메뉴 목록에 노출할지 여부를 제어합니다." placement="top">
                  <el-checkbox v-model="localTemp.isMobile">모바일 노출</el-checkbox>
                </el-tooltip>
                <el-tooltip content="동일한 메뉴 화면을 탭 바(TagsView)에 중복으로 여러 개 열 수 있도록 허용합니다." placement="top">
                  <el-checkbox v-model="localTemp.allowDuplicate">중복화면 허용</el-checkbox>
                </el-tooltip>
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
      </el-tab-pane>
    </el-tabs>
  </el-card>

  <el-card v-else shadow="never" class="empty-card">
    <div class="empty-state">
      <i class="fas fa-info-circle" style="font-size: 3.125rem; color: #909399;"></i>
      <p>목록에서 메뉴를 선택하거나 새 메뉴를 추가해 주세요.</p>
    </div>
  </el-card>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue';

interface Menu {
  id?: number | string;
  parentId?: number | string | null;
  title: string;
  icon?: string;
  path?: string;
  component?: string;
  name?: string;
  redirect?: string;
  sortOrder?: number;
  hidden?: boolean;
  alwaysShow?: boolean;
  affix?: boolean;
  keepAlive?: boolean;
  isMobile?: boolean;
  allowDuplicate?: boolean;
}

const props = defineProps<{
  temp: Menu;
  currentMenu: Menu | null;
  flatMenuList: Menu[];
  elementIconList: string[];
  elementIconComponents: Record<string, any>;
  svgIconList: string[];
}>();

const emit = defineEmits(['create-sibling', 'create-sub', 'save', 'update:temp']);

const activeDetailTab = ref('basic');
const activeIconTab = ref('svg');

const localTemp = reactive({ ...props.temp });

watch(() => props.temp, (newVal) => {
  Object.assign(localTemp, newVal);
}, { deep: true });

watch(localTemp, (newVal) => {
  emit('update:temp', { ...newVal });
}, { deep: true });
</script>

