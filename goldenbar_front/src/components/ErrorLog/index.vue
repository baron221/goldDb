<template>
<div v-if="errorLogs.length>0">
    <el-badge :is-dot="true" style="line-height: 25px;margin-top: -0.3125rem;" @click="dialogTableVisible=true">
      <el-button style="padding: 0.5rem 0.625rem;" size="small" type="danger">
        <svg-icon icon-class="bug" />
      </el-button>
    </el-badge>

    <base-popup draggable v-model="dialogTableVisible" width="80%" append-to-body>
      <template v-slot:header>
        <div class="text-title">
          <span style="padding-right: 0.625rem;">Error Log</span>
          <el-button size="small" type="primary" :icon="IconDelete" @click="clearAll">Clear All</el-button>
        </div>
      </template>
      <base-table :data="errorLogs">
        <el-table-column
          label="Message"
          :excel-formatter="(row) => `Msg: ${row.err.message}\nInfo: error in ${row.info}\nUrl: ${row.url}`"
        >
          <template #default="{row}">
            <div>
              <span class="message-title">Msg:</span>
              <el-tag type="danger">
                {{ row.err.message }}
              </el-tag>
            </div>
            <br>
            <div>
              <span class="message-title" style="padding-right: 0.625rem;">Info: </span>
              <el-tag type="warning">
                error in {{ row.info }}
              </el-tag>
            </div>
            <br>
            <div>
              <span class="message-title" style="padding-right: 1rem;">Url: </span>
              <el-tag type="success">
                {{ row.url }}
              </el-tag>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="Stack" prop="err.stack" />
      </base-table>
    </base-popup>
  </div>
</template>

<script>
import { defineComponent, markRaw } from 'vue';
import store from '@/store';
import { Delete as IconDelete } from '@element-plus/icons-vue';
import BasePopup from '@/components/BasePopup/index.vue';

export default defineComponent({
  name: 'ErrorLog',
  components: {
    BasePopup
  },
  data() {
    return {
      IconDelete: markRaw(IconDelete),
      dialogTableVisible: false
    };
  },
  computed: {
    errorLogs() {
      return store.errorLog().logs;
    }
  },
  methods: {
    clearAll() {
      this.dialogTableVisible = false;
      store.errorLog().clearErrorLog();
    }
  }
});
</script>

<style scoped>
.message-title {
  font-size: 1rem;
  color: #333;
  font-weight: bold;
  padding-right: 0.5rem;
}
</style>

