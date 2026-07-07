<template>
<div class="app-container">
    <div class="excel-options">
      <el-input v-model="filename" placeholder="Please enter the file name (default excel-list)" style="width: 100%; max-width: 350px;" :prefix-icon="IconDocument" />
      <el-button :loading="downloadLoading" type="primary" :icon="IconDocument" @click="handleDownload">
        Export Selected Items
      </el-button>
      <a href="https://panjiachen.github.io/vue-element-admin-site/feature/component/excel.html" target="_blank">
        <el-tag type="info" size="large">Documentation</el-tag>
      </a>
    </div>
    <base-table
      ref="baseTableRef"
      v-loading="listLoading"
      :data="list"
      element-loading-text="열심히 로딩 중입니다"
      border
      fit
      highlight-current-row
      @selection-change="handleSelectionChange"
    >
      <el-table-column type="selection" align="center" />
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
      <el-table-column align="center" label="PDate" width="220" prop="display_time">
        <template v-slot="scope">
          <el-icon><IconTimer /></el-icon>
          <span>{{ scope.row.display_time }}</span>
        </template>
      </el-table-column>
    </base-table>
  </div>
</template>

<script>
import { defineComponent, markRaw, ref } from 'vue';
import { fetchList } from '@/api/article';
import BaseTable from '@/components/BaseTable/index.vue';
import { Document as IconDocument, Timer as IconTimer } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';

export default defineComponent({
  name: 'SelectExcel',
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
      multipleSelection: [],
      downloadLoading: false,
      filename: ''
    };
  },
  created() {
    this.fetchData();
  },
  methods: {
    fetchData() {
      this.listLoading = true;
      fetchList().then(response => {
        this.list = response.data.items;
        this.listLoading = false;
      });
    },
    handleSelectionChange(val) {
      this.multipleSelection = val;
    },
    handleDownload() {
      if (this.multipleSelection.length) {
        this.downloadLoading = true;

        import('@/vendor/Export2Excel').then(excel => {
          const tHeader = ['Id', 'Title', 'Author', 'Readings', 'Date'];
          const filterVal = ['id', 'title', 'author', 'pageviews', 'display_time'];
          const list = this.multipleSelection;
          const data = this.formatJson(filterVal, list);
          excel.export_json_to_excel({
            header: tHeader,
            data,
            filename: this.filename || 'excel-list'
          });
          this.baseTableRef.clearSelection();
          this.downloadLoading = false;
        });
      } else {
        ElMessage({
          message: 'Please select at least one item',
          type: 'warning'
        });
      }
    },
    formatJson(filterVal, jsonData) {
      return jsonData.map(v => filterVal.map(j => v[j]));
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
</style>

