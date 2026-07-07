<template>
<div class="app-container">

    <el-button :loading="downloadLoading" style="margin-bottom: 1.25rem" type="primary" :icon="IconDocument" @click="handleDownload">Export</el-button>

    <base-table
      ref="baseTableRef"
      v-loading="listLoading"
      :data="list"
      element-loading-text="Loading"
      border
      fit
      highlight-current-row
    >
      <el-table-column align="center" label="Id" width="95" prop="id">
        <template v-slot="scope">
          {{ scope.$index }}
        </template>
      </el-table-column>
      <el-table-column label="Main Information" align="center">
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
import BaseTable from '@/components/BaseTable/index.vue';
import { Document as IconDocument, Timer as IconTimer } from '@element-plus/icons-vue';

export default defineComponent({
  name: 'MergeHeader',
  components: { IconTimer, BaseTable },
  setup() {
    const baseTableRef = ref(null);
    return { baseTableRef };
  },
  data() {
    return {
      IconDocument: markRaw(IconDocument),
      list: null,
      listLoading: true,
      downloadLoading: false
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
        this.baseTableRef.exportExcel('merge-header');
        this.downloadLoading = false;
      }
    }
  }
});
</script>

