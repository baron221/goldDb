<template>
<div class="app-container">
    <div class="zip-options">
      <el-input v-model="filename" placeholder="Please enter the file name (default file)" style="width: 100%; max-width: 300px;" :prefix-icon="IconDocument" />
      <el-button :loading="downloadLoading" type="primary" :icon="IconDocument" @click="handleDownload">
        Export Zip
      </el-button>
    </div>
    <base-table v-loading="listLoading" :data="list" element-loading-text="데이터를 불러오는 중입니다...">
      <el-table-column align="center" label="ID" width="95">
        <template v-slot="scope">
          {{ scope.$index }}
        </template>
      </el-table-column>
      <el-table-column label="Title" prop="title">
        <template v-slot="scope">
          {{ scope.row.title }}
        </template>
      </el-table-column>
      <el-table-column label="Author" width="95" align="center" prop="author">
        <template v-slot="scope">
          <el-tag>{{ scope.row.author }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="Readings" width="115" align="center" prop="pageviews">
        <template v-slot="scope">
          {{ scope.row.pageviews }}
        </template>
      </el-table-column>
      <el-table-column align="center" label="Date" width="220" prop="display_time">
        <template v-slot="scope">
          <span class="col-date">
            <el-icon><IconTimer /></el-icon>
            <span>{{ scope.row.display_time }}</span>
          </span>
        </template>
      </el-table-column>
    </base-table>
  </div>
</template>

<script>
import { defineComponent } from 'vue';
import { fetchList } from '@/api/article';
import { Timer as IconTimer, Document as IconDocument } from '@element-plus/icons-vue';
import { markRaw } from 'vue';
import BaseTable from '@/components/BaseTable/index.vue';

export default defineComponent({
  name: 'ExportZip',
  components: {
    IconTimer,
    BaseTable
  },
  data() {
    return {
      IconDocument: markRaw(IconDocument),
      list: null,
      listLoading: true,
      downloadLoading: false,
      filename: ''
    };
  },
  created() {
    this.fetchData();
  },
  methods: {
    async fetchData() {
      this.listLoading = true;
      const { data } = await fetchList();
      this.list = data.items;
      this.listLoading = false;
    },
    handleDownload() {
      this.downloadLoading = true;
      import('@/vendor/Export2Zip').then(zip => {
        const tHeader = ['Id', 'Title', 'Author', 'Readings', 'Date'];
        const filterVal = ['id', 'title', 'author', 'pageviews', 'display_time'];
        const list = this.list;
        const data = this.formatJson(filterVal, list);
        zip.export_txt_to_zip(tHeader, data, this.filename, this.filename);
        this.downloadLoading = false;
      });
    },
    formatJson(filterVal, jsonData) {
      return jsonData.map(v => filterVal.map(j => v[j]));
    }
  }
});
</script>

<style scoped>
.zip-options {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.625rem;
  margin-bottom: 1.25rem;
}
@media (max-width: 768px) {
  .zip-options {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>

