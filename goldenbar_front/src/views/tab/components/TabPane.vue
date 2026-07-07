<template>
<base-table :data="list" :loading="loading" border fit highlight-current-row style="width: 100%">
    <el-table-column
      prop="id"
      align="center"
      label="ID"
      width="65"
    >
      <template v-slot="scope">
        <span>{{ scope.row.id }}</span>
      </template>
    </el-table-column>

    <el-table-column prop="timestamp" width="180px" align="center" label="Date" :excel-formatter="row => parseTime(row.timestamp, '{y}-{m}-{d} {h}:{i}')">
      <template v-slot="scope">
        <span>{{ parseTime(scope.row.timestamp,'{y}-{m}-{d} {h}:{i}') }}</span>
      </template>
    </el-table-column>

    <el-table-column prop="title" min-width="300px" label="Title" :excel-formatter="row => `${row.title} [${row.type}]` ">
      <template v-slot="{row}">
        <span>{{ row.title }}</span>
        <el-tag>{{ row.type }}</el-tag>
      </template>
    </el-table-column>

    <el-table-column prop="author" width="110px" align="center" label="Author">
      <template v-slot="scope">
        <span>{{ scope.row.author }}</span>
      </template>
    </el-table-column>

    <el-table-column prop="importance" width="120px" label="Importance" :excel-formatter="row => '★'.repeat(row.importance)">
      <template v-slot="scope">
        <svg-icon v-for="n in scope.row.importance" :key="n" icon-class="star" />
      </template>
    </el-table-column>

    <el-table-column prop="pageviews" align="center" label="Readings" width="95">
      <template v-slot="scope">
        <span>{{ scope.row.pageviews }}</span>
      </template>
    </el-table-column>

    <el-table-column prop="status" class-name="status-col" label="Status" width="110">
      <template v-slot="{row}">
        <el-tag :type="statusFilter(row.status)">
          {{ row.status }}
        </el-tag>
      </template>
    </el-table-column>
  </base-table>
</template>

<script>
import { defineComponent } from 'vue';
import { parseTime } from '@/utils';
import { fetchList } from '@/api/article';
import BaseTable from '@/components/BaseTable/index.vue';

export default defineComponent({
  components: { BaseTable },
  props: {
    type: {
      type: String,
      default: 'CN'
    }
  },
  data() {
    return {
      list: null,
      listQuery: {
        page: 1,
        limit: 5,
        type: this.type,
        sort: '+id'
      },
      loading: false
    };
  },
  created() {
    this.getList();
  },
  methods: {
    parseTime,
    statusFilter(status) {
      const statusMap = {
        published: 'success',
        draft: 'info',
        deleted: 'danger'
      };
      return statusMap[status];
    },
    getList() {
      this.loading = true;
      this.$emit('create'); 
      fetchList(this.listQuery).then(response => {
        this.list = response.data.items;
        this.loading = false;
      });
    }
  }
});
</script>

