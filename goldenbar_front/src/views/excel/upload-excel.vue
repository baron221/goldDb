<template>
<div class="app-container">
    <upload-excel-component :on-success="handleSuccess" :before-upload="beforeUpload" />
    <base-table :data="tableData" border highlight-current-row style="width: 100%;margin-top: 1.25rem;">
      <el-table-column v-for="item of tableHeader" :key="item" :prop="item" :label="item" :excel-formatter="(row) => row[item]" />
    </base-table>
  </div>
</template>

<script>
import { defineComponent } from 'vue';
import UploadExcelComponent from '@/components/UploadExcel/index.vue';
import BaseTable from '@/components/BaseTable/index.vue';
import { ElMessage } from 'element-plus';

export default defineComponent({
  name: 'UploadExcel',
  components: { UploadExcelComponent, BaseTable },
  data() {
    return {
      tableData: [],
      tableHeader: []
    };
  },
  methods: {
    beforeUpload(file) {
      const isLt1M = file.size / 1024 / 1024 < 1;

      if (isLt1M) {
        return true;
      }

      ElMessage({
        message: 'Please do not upload files larger than 1m in size.',
        type: 'warning'
      });
      return false;
    },
    handleSuccess({ results, header }) {
      this.tableData = results;
      this.tableHeader = header;
    }
  }
});
</script>

