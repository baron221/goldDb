<template>
<div class="app-container">
    <switch-roles @change="handleRolesChange" />
    <div :key="key" style="margin-top: 1.875rem;">
      <div>
        <span v-permission="['admin']" class="permission-alert">
          Only
          <el-tag class="permission-tag" size="small">admin</el-tag> can see this
        </span>
        <el-tag v-permission="['admin']" class="permission-sourceCode" type="info">
          v-permission="['admin']"
        </el-tag>
      </div>

      <div>
        <span v-permission="['editor']" class="permission-alert">
          Only
          <el-tag class="permission-tag" size="small">editor</el-tag> can see this
        </span>
        <el-tag v-permission="['editor']" class="permission-sourceCode" type="info">
          v-permission="['editor']"
        </el-tag>
      </div>

      <div>
        <span v-permission="['admin', 'editor']" class="permission-alert">
          Both
          <el-tag class="permission-tag" size="small">admin</el-tag> and
          <el-tag class="permission-tag" size="small">editor</el-tag> can see this
        </span>
        <el-tag v-permission="['admin', 'editor']" class="permission-sourceCode" type="info">
          v-permission="['admin','editor']"
        </el-tag>
      </div>
    </div>

    <div :key="'checkPermission' + key" style="margin-top: 3.75rem;">
      <aside>
        In some cases, using v-permission will have no effect. For example: Element-UI's Tab component or
        el-table-column and other scenes that dynamically render dom. You can only do this with v-if.
        <br> e.g.
      </aside>

      <el-tabs type="border-card" style="width: 100%; max-width: 550px;">
        <el-tab-pane v-if="checkPermission(['admin'])" label="Admin">
          Admin can see this
          <el-tag class="permission-sourceCode" type="info">
            v-if="checkPermission(['admin'])"
          </el-tag>
        </el-tab-pane>

        <el-tab-pane v-if="checkPermission(['editor'])" label="Editor">
          Editor can see this
          <el-tag class="permission-sourceCode" type="info">
            v-if="checkPermission(['editor'])"
          </el-tag>
        </el-tab-pane>

        <el-tab-pane v-if="checkPermission(['admin', 'editor'])" label="Admin-OR-Editor">
          Both admin or editor can see this
          <el-tag class="permission-sourceCode" type="info">
            v-if="checkPermission(['admin','editor'])"
          </el-tag>
        </el-tab-pane>
      </el-tabs>
    </div>
  </div>
</template>

<script>
import { defineComponent } from 'vue';
import checkPermission from '@/utils/permission'; 
import SwitchRoles from './components/SwitchRoles';

export default defineComponent({
  name: 'DirectivePermission',
  components: { SwitchRoles },
  data() {
    return {
      key: 1 
    };
  },
  methods: {
    checkPermission,
    handleRolesChange() {
      this.key++;
    }
  }
});
</script>

<style lang="scss" scoped>
.app-container {
  :deep(.permission-alert) {
    width: 100%;
    max-width: 320px;
    box-sizing: border-box;
    margin-top: 0.9375rem;
    background-color: #f0f9eb;
    color: #67c23a;
    padding: 0.5rem 1rem;
    border-radius: 2px;
    display: inline-block;
  }

  :deep(.permission-sourceCode) {
    margin-left: 0.9375rem;
  }

  :deep(.permission-tag) {
    background-color: #ecf5ff;
  }
}
</style>

