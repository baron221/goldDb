<template>
<div>
    <input ref="excel-upload-input" class="excel-upload-input" type="file" accept=".xlsx, .xls" @change="handleClick">
    <div class="drop" @drop="handleDrop" @dragover="handleDragover" @dragenter="handleDragover">
      Drop excel file here or
      <el-button :loading="loading" style="margin-left: 1rem;" size="small" type="primary" @click="handleUpload">
        Browse
      </el-button>
    </div>
  </div>
</template>

<script>
import { defineComponent } from 'vue';
import * as XLSX from 'xlsx';

export default defineComponent({
  props: {
    beforeUpload: {
      type: Function,

      default: () => {}
    },
    onSuccess: {
      type: Function,

      default: () => {}
    }
  },
  data() {
    return {
      loading: false,
      excelData: {
        header: null,
        results: null
      }
    };
  },
  methods: {
    generateData({ header, results }) {
      this.excelData.header = header;
      this.excelData.results = results;
      if (this.onSuccess) {
        this.onSuccess(this.excelData);
      }
    },
    handleDrop(e) {
      e.stopPropagation();
      e.preventDefault();
      if (this.loading) return;
      const files = e.dataTransfer.files;
      if (files.length !== 1) {
        ElMessage.error('Only support uploading one file!');
        return;
      }
      const rawFile = files[0]; 

      if (!this.isExcel(rawFile)) {
        ElMessage.error('Only supports upload .xlsx, .xls, .csv suffix files');
        return false;
      }
      this.upload(rawFile);
      e.stopPropagation();
      e.preventDefault();
    },
    handleDragover(e) {
      e.stopPropagation();
      e.preventDefault();
      e.dataTransfer.dropEffect = 'copy';
    },
    handleUpload() {
      this.$refs['excel-upload-input'].click();
    },
    handleClick(e) {
      const files = e.target.files;
      const rawFile = files[0]; 
      if (!rawFile) return;
      this.upload(rawFile);
    },
    upload(rawFile) {
      this.$refs['excel-upload-input'].value = null; 

      if (!this.beforeUpload) {
        this.readerData(rawFile);
        return;
      }
      const before = this.beforeUpload(rawFile);
      if (before) {
        this.readerData(rawFile);
      }
    },
    readerData(rawFile) {
      this.loading = true;
      return new Promise((resolve) => {
        const reader = new FileReader();
        reader.onload = e => {
          const data = e.target.result;
          const workbook = XLSX.read(data, { type: 'array' });
          const firstSheetName = workbook.SheetNames[0];
          const worksheet = workbook.Sheets[firstSheetName];
          const header = this.getHeaderRow(worksheet);
          const results = XLSX.utils.sheet_to_json(worksheet);
          this.generateData({ header, results });
          this.loading = false;
          resolve();
        };
        reader.readAsArrayBuffer(rawFile);
      });
    },
    getHeaderRow(sheet) {
      const headers = [];
      const range = XLSX.utils.decode_range(sheet['!ref']);
      let C;
      const R = range.s.r;

      for (C = range.s.c; C <= range.e.c; ++C) { 
        const cell = sheet[XLSX.utils.encode_cell({ c: C, r: R })];

        let hdr = 'UNKNOWN ' + C; 
        if (cell && cell.t) hdr = XLSX.utils.format_cell(cell);
        headers.push(hdr);
      }
      return headers;
    },
    isExcel(file) {
      return /\.(xlsx|xls|csv)$/.test(file.name);
    }
  }
});
</script>

<style scoped>
.excel-upload-input{
  display: none;
  z-index: -9999;
}
.drop{
  border: 2px dashed #bbb;
  width: 100%;
  max-width: 600px;
  height: 160px;
  line-height: 160px;
  margin: 0 auto;
  font-size: 1.5rem;
  border-radius: 2px;
  text-align: center;
  color: #bbb;
  position: relative;
}
@media (max-width: 768px) {
  .drop {
    font-size: 1.1rem;
    padding: 0 10px;
    box-sizing: border-box;
  }
}
</style>

