<template>
<div class="app-container">
    <base-table v-loading="listLoading" :data="list" style="width: 100%">
      <el-table-column align="center" label="ID" width="80" prop="id">
        <template v-slot="{row}">
          <span>{{ row.id }}</span>
        </template>
      </el-table-column>

      <el-table-column width="180px" align="center" label="Date" prop="timestamp" :excel-formatter="row => parseTime(row.timestamp, '{y}-{m}-{d} {h}:{i}')">
        <template v-slot="{row}">
          <span>{{ parseTime(row.timestamp, '{y}-{m}-{d} {h}:{i}') }}</span>
        </template>
      </el-table-column>

      <el-table-column width="120px" align="center" label="Author" prop="author">
        <template v-slot="{row}">
          <span>{{ row.author }}</span>
        </template>
      </el-table-column>

      <el-table-column width="100px" label="Importance" prop="importance">
        <template v-slot="{row}">
          <svg-icon v-for="n in row.importance" :key="n" icon-class="star" class="meta-item__icon" />
        </template>
      </el-table-column>

      <el-table-column class-name="status-col" label="Status" width="110" prop="status">
        <template v-slot="{row}">
          <el-tag :type="statusFilter(row.status)">
            {{ row.status }}
          </el-tag>
        </template>
      </el-table-column>

      <el-table-column min-width="300px" label="Title" prop="title">
        <template v-slot="{row}">
          <template v-if="row.edit">
            <el-input v-model="row.title" class="edit-input" size="small" />
            <el-button
              class="cancel-btn"
              size="small"
              :icon="iconRefresh"
              type="warning"
              @click="cancelEdit(row)"
            >
              cancel
            </el-button>
          </template>
          <span v-else>{{ row.title }}</span>
        </template>
      </el-table-column>

      <el-table-column align="center" label="Actions" width="120">
        <template v-slot="{row}">
          <el-button
            v-if="row.edit"
            type="success"
            size="small"
            :icon="iconCircleCheck"
            @click="confirmEdit(row)"
          >
            Ok
          </el-button>
          <el-button
            v-else
            type="primary"
            size="small"
            :icon="iconEdit"
            @click="row.edit=!row.edit"
          >
            Edit
          </el-button>
        </template>
      </el-table-column>
    </base-table>
  </div>
</template>

<script>
import { defineComponent, markRaw } from 'vue';
import { fetchList } from '@/api/article';
import { parseTime } from '@/utils';
import { Refresh, CircleCheck, Edit } from '@element-plus/icons-vue';
import BaseTable from '@/components/BaseTable/index.vue';

export default defineComponent({
  name: 'InlineEditTable',
  components: { BaseTable },
  data() {
    return {
      iconRefresh: markRaw(Refresh),
      iconCircleCheck: markRaw(CircleCheck),
      iconEdit: markRaw(Edit),
      list: null,
      listLoading: true,
      listQuery: {
        page: 1,
        limit: 10
      }
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
    async getList() {
      this.listLoading = true;
      const { data } = await fetchList(this.listQuery);
      const items = data.items;
      this.list = items.map(v => {
        return {
          ...v,
          edit: false,
          originalTitle: v.title 
        };
      });
      this.listLoading = false;
    },
    cancelEdit(row) {
      row.title = row.originalTitle;
      row.edit = false;
      ElMessage({
        message: 'The title has been restored to the original value',
        type: 'warning'
      });
    },
    confirmEdit(row) {
      row.edit = false;
      row.originalTitle = row.title;
      ElMessage({
        message: 'The title has been edited',
        type: 'success'
      });
    }
  }
});
</script>

<style scoped>
.edit-input {
  padding-right: 6.25rem;
}
.cancel-btn {
  position: absolute;
  right: 15px;
  top: 10px;
}
</style>

