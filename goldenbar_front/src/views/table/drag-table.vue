<template>
<div class="app-container">

    <base-table ref="dragTable" v-loading="listLoading" :data="list" row-key="id" style="width: 100%">
      <el-table-column align="center" label="ID" width="65" prop="id">
        <template v-slot="{row}">
          <span>{{ row.id }}</span>
        </template>
      </el-table-column>

      <el-table-column width="180px" align="center" label="Date" prop="timestamp" :excel-formatter="row => parseTime(row.timestamp, '{y}-{m}-{d} {h}:{i}')">
        <template v-slot="{row}">
          <span>{{ parseTime( row.timestamp, '{y}-{m}-{d} {h}:{i}') }}</span>
        </template>
      </el-table-column>

      <el-table-column min-width="300px" label="Title" prop="title">
        <template v-slot="{row}">
          <span>{{ row.title }}</span>
        </template>
      </el-table-column>

      <el-table-column width="110px" align="center" label="Author" prop="author">
        <template v-slot="{row}">
          <span>{{ row.author }}</span>
        </template>
      </el-table-column>

      <el-table-column width="100px" label="Importance" prop="importance">
        <template v-slot="{row}">
          <svg-icon v-for="n in row.importance" :key="n" icon-class="star" class="icon-star" />
        </template>
      </el-table-column>

      <el-table-column align="center" label="Readings" width="95" prop="pageviews">
        <template v-slot="{row}">
          <span>{{ row.pageviews }}</span>
        </template>
      </el-table-column>

      <el-table-column class-name="status-col" label="Status" width="110" prop="status">
        <template v-slot="{row}">
          <el-tag :type="statusFilter(row.status)">
            {{ row.status }}
          </el-tag>
        </template>
      </el-table-column>

      <el-table-column align="center" label="Drag" width="80">
        <template v-slot="{}">
          <svg-icon class="drag-handler" icon-class="drag" />
        </template>
      </el-table-column>
    </base-table>
    <div class="show-d">
      <el-tag>The default order :</el-tag> {{ oldList }}
    </div>
    <div class="show-d">
      <el-tag>The after dragging order :</el-tag> {{ newList }}
    </div>
  </div>
</template>

<script>
import { defineComponent } from 'vue';
import { fetchList } from '@/api/article';
import Sortable from 'sortablejs';
import { parseTime } from '@/utils';
import BaseTable from '@/components/BaseTable/index.vue';

export default defineComponent({
  name: 'DragTable',
  components: { BaseTable },
  data() {
    return {
      list: null,
      total: null,
      listLoading: true,
      listQuery: {
        page: 1,
        limit: 10
      },
      sortable: null,
      oldList: [],
      newList: []
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
      this.list = data.items;
      this.total = data.total;
      this.listLoading = false;
      this.oldList = this.list.map(v => v.id);
      this.newList = this.oldList.slice();
      this.$nextTick(() => {
        this.setSort();
      });
    },
    setSort() {

      const el = this.$refs.dragTable.tableRef.$el.querySelectorAll('.el-table__body-wrapper table tbody')[0];
      this.sortable = Sortable.create(el, {
        ghostClass: 'sortable-ghost', 
        setData: function(dataTransfer) {

          dataTransfer.setData('Text', '');
        },
        onEnd: evt => {
          const targetRow = this.list.splice(evt.oldIndex, 1)[0];
          this.list.splice(evt.newIndex, 0, targetRow);

          const tempIndex = this.newList.splice(evt.oldIndex, 1)[0];
          this.newList.splice(evt.newIndex, 0, tempIndex);
        }
      });
    }
  }
});
</script>

<style>
.sortable-ghost{
  opacity: .8;
  color: #fff!important;
  background: #42b983!important;
}
</style>

<style scoped>
.icon-star{
  margin-right: 0.125rem;
}
.drag-handler{
  width: 20px;
  height: 20px;
  cursor: pointer;
}
.show-d{
  margin-top: 0.9375rem;
}
</style>

