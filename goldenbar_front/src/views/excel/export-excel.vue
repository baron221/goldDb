<template>
<div class="app-container">

    <div class="excel-options">
      <FilenameOption v-model="filename" />
      <AutoWidthOption v-model="autoWidth" />
      <BookTypeOption v-model="bookType" />
      <el-button :loading="downloadLoading" type="primary" :icon="IconDocument" @click="handleDownload">
        Export Excel
      </el-button>
      <a href="https://vue3-element-admin-site.midfar.com/feature/component/excel.html" target="_blank">
        <el-tag type="info" size="large">Documentation</el-tag>
      </a>
    </div>

    <base-table ref="baseTableRef" v-loading="listLoading" :data="list" element-loading-text="Loading..." border fit highlight-current-row>
      <el-table-column align="center" label="Id" width="95" prop="id">
        <template v-slot="scope">
          {{ scope.$index }}
        </template>
      </el-table-column>
      <el-table-column label="Title" prop="title">
        <template v-slot="scope">
          {{ scope.row.title }}
        </template>
      </el-table-column>
      <el-table-column label="Author" width="110" align="center" prop="author">
        <template v-slot="scope">
          <el-tag>{{ scope.row.author }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="Readings" width="115" align="center" prop="pageviews">
        <template v-slot="scope">
          {{ scope.row.pageviews }}
        </template>
      </el-table-column>
      <el-table-column align="center" label="Date" width="220" prop="timestamp" :excel-formatter="(row) => parseTime(row.timestamp, '{y}-{m}-{d} {h}:{i}')">
        <template v-slot="scope">
          <el-icon><IconTimer /></el-icon>
          <span>{{ parseTime(scope.row.timestamp, '{y}-{m}-{d} {h}:{i}') }}</span>
        </template>
      </el-table-column>
    </base-table>
  </div>
</template>

<script>
import { defineComponent, markRaw, ref } from 'vue';
import { fetchList } from '@/api/article';
import { parseTime } from '@/utils';

import FilenameOption from './components/FilenameOption';
import AutoWidthOption from './components/AutoWidthOption';
import BookTypeOption from './components/BookTypeOption';
import BaseTable from '@/components/BaseTable/index.vue';
import { Document as IconDocument, Timer as IconTimer } from '@element-plus/icons-vue';

export default defineComponent({
  name: 'ExportExcel',
  components: { FilenameOption, AutoWidthOption, BookTypeOption, IconTimer, BaseTable },
  setup() {
    const baseTableRef = ref(null);
    return { baseTableRef };
  },
  data() {
    return {
      IconDocument: markRaw(IconDocument),
      list: null,
      listLoading: true,
      downloadLoading: false,
      filename: '',
      autoWidth: true,
      bookType: 'xlsx'
    };
  },
  created() {
    this.fetchData();
  },
  methods: {
    parseTime,
    fetchData() {
      this.listLoading = true;
      fetchList().then(response => {
        this.list = response.data.items;
        this.listLoading = false;
      });
    },
    handleDownload() {
      if (this.baseTableRef) {
        this.downloadLoading = true;
        this.baseTableRef.exportExcel(this.filename);
        this.downloadLoading = false;
      }
    }
  }
});
</script>

<style scoped>
.excel-options {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.625rem;
  margin-bottom: 1.25rem;
}
@media (max-width: 768px) {
  .excel-options {
    flex-direction: column;
    align-items: flex-start;
  }
}
.radio-label {
  font-size: 0.875rem;
  color: #606266;
  line-height: 40px;
  padding: 0 0.95rem 0 1.875rem;
}
</style>

