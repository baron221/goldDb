<template>
<div class="app-container">
    <el-tabs v-model="activeName">
      <el-tab-pane label="use clipboard  directly" name="directly">
        <div class="flex-container">
          <el-input v-model="inputData" placeholder="Please input" style="width:400px;max-width:100%;" />
          <el-button type="primary" :icon="Document" @click="handleCopy(inputData, $event)">
            copy
          </el-button>
        </div>
      </el-tab-pane>
      <el-tab-pane label="use clipboard by v-directive" name="v-directive">
        <div class="flex-container">
          <el-input v-model="inputData" placeholder="Please input" style="width:400px;max-width:100%;" />
          <el-button v-clipboard:copy="inputData" v-clipboard:success="clipboardSuccess" type="primary" :icon="Document">
            copy
          </el-button>
        </div>
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script>
import clip from '@/utils/clipboard'; 
import clipboard from '@/directive/clipboard'; 
import { defineComponent, markRaw } from 'vue';
import { Document } from '@element-plus/icons-vue';

export default defineComponent({
  name: 'ClipboardDemo',
  directives: {
    clipboard
  },
  data() {
    return {
      activeName: 'directly',
      inputData: 'https://element-plus.midfar.com',
      Document: markRaw(Document)
    };
  },
  methods: {
    handleCopy(text, event) {
      clip(text, event);
    },
    clipboardSuccess() {
      ElMessage({
        message: 'Copy successfully',
        type: 'success',
        duration: 1500
      });
    }
  }
});
</script>

<style scoped>
.flex-container {
  display: flex;
  align-items: center;
  gap: 0.625rem;
  flex-wrap: wrap;
}
</style>

