<template>
<div class="app-container">
    <el-button type="primary" @click="handleAddRole">New Role</el-button>

    <base-table :data="rolesList" style="width: 100%;margin-top: 1.875rem;">
      <el-table-column align="center" label="Role Key" prop="key" width="220" />
      <el-table-column align="center" label="Role Name" prop="name" width="220" />
      <el-table-column align="header-center" label="Description" prop="description" />
      <el-table-column align="center" label="Operations">
        <template v-slot="scope">
          <el-button type="primary" size="small" @click="handleEdit(scope)">Edit</el-button>
          <el-button type="danger" size="small" @click="handleDelete(scope)">Delete</el-button>
        </template>
      </el-table-column>
    </base-table>

    <base-popup draggable v-model="dialogVisible" :title="dialogType === 'edit' ? 'Edit Role' : 'New Role'" class="responsive-dialog-600">
      <el-form :model="role" :label-width="isMobile ? '100%' : '80px'" :label-position="isMobile ? 'top' : 'left'">
        <el-form-item label="Name">
          <el-input v-model="role.name" placeholder="Role Name" />
        </el-form-item>
        <el-form-item label="Desc">
          <el-input v-model="role.description" :autosize="{ minRows: 2, maxRows: 4 }" type="textarea"
                    placeholder="Role Description" />
        </el-form-item>
        <el-form-item label="Menus">
          <el-tree ref="tree" :check-strictly="checkStrictly" :data="routesData" :props="defaultProps" show-checkbox
                   node-key="path" class="permission-tree" />
        </el-form-item>
      </el-form>
      <div style="text-align:right;">
        <el-button type="danger" @click="dialogVisible = false">Cancel</el-button>
        <el-button type="primary" @click="confirmRole">Confirm</el-button>
      </div>
    </base-popup>
  </div>
</template>

<script>
import path from 'path-browserify';
import { deepClone } from '@/utils';
import { getRoutes, getRoles, addRole, deleteRole, updateRole } from '@/api/role';
import { defineComponent } from 'vue';
import BaseTable from '@/components/BaseTable/index.vue';
import BasePopup from '@/components/BasePopup/index.vue';
import { useMobile } from '@/hooks/useMobile';

const defaultRole = {
  key: '',
  name: '',
  description: '',
  routes: []
};

export default defineComponent({
  components: { BaseTable, BasePopup },
  setup() {
    const { isMobile } = useMobile();
    return { isMobile };
  },
  data() {
    return {
      role: Object.assign({}, defaultRole),
      routes: [],
      rolesList: [],
      dialogVisible: false,
      dialogType: 'new',
      checkStrictly: false,
      defaultProps: {
        children: 'children',
        label: 'title'
      }
    };
  },
  computed: {
    routesData() {
      return this.routes;
    }
  },
  created() {

    this.getRoutes();
    this.getRoles();
  },
  methods: {
    async getRoutes() {
      const res = await getRoutes();
      this.serviceRoutes = res.data;
      this.routes = this.generateRoutes(res.data);
    },
    async getRoles() {
      const res = await getRoles();
      this.rolesList = res.data;
    },
    resolvePath(basePath, routePath) {

      if (routePath.indexOf('http://') === 0 || routePath.indexOf('https://') === 0) {
        return routePath;
      }
      if (basePath === '') {
        return path.resolve('/', routePath);
      }
      return path.resolve(basePath, routePath);
    },

    generateRoutes(routes, basePath = '/') {
      const res = [];

      for (let route of routes) {

        if (route.meta && route.meta.hidden) { continue; }

        const onlyOneShowingChild = this.onlyOneShowingChild(route.children, route);

        if (route.children && onlyOneShowingChild && !(route.meta && route.meta.alwaysShow)) {
          route = onlyOneShowingChild;
        }

        const data = {
          path: this.resolvePath(basePath, route.path),
          title: route.meta && route.meta.title

        };

        if (route.children) {
          data.children = this.generateRoutes(route.children, data.path);
        }
        res.push(data);
      }
      return res;
    },
    generateArr(routes) {
      let data = [];
      routes.forEach(route => {
        data.push(route);
        if (route.children) {
          const temp = this.generateArr(route.children);
          if (temp.length > 0) {
            data = [...data, ...temp];
          }
        }
      });
      return data;
    },
    handleAddRole() {
      this.role = Object.assign({}, defaultRole);
      if (this.$refs.tree) {
        this.$refs.tree.setCheckedNodes([]);
      }
      this.dialogType = 'new';
      this.dialogVisible = true;
    },
    handleEdit(scope) {
      this.dialogType = 'edit';
      this.dialogVisible = true;
      this.checkStrictly = true;
      this.role = deepClone(scope.row);
      this.$nextTick(() => {
        const routes = this.generateRoutes(this.role.routes);
        this.$refs.tree.setCheckedNodes(this.generateArr(routes));

        this.checkStrictly = false;
      });
    },
    handleDelete({ $index, row }) {
      ElMessageBox.confirm('Confirm to remove the role?', 'Warning', {
        confirmButtonText: 'Confirm',
        cancelButtonText: 'Cancel',
        type: 'warning'
      })
        .then(async() => {
          await deleteRole(row.key);
          this.rolesList.splice($index, 1);
          ElMessage({
            type: 'success',
            message: 'Delete succed!'
          });
        })
        .catch(err => { console.error(err); });
    },
    generateTree(routes, basePath = '/', checkedKeys) {
      const res = [];

      for (const route of routes) {
        const routePath = this.resolvePath(basePath, route.path);

        if (route.children) {
          route.children = this.generateTree(route.children, routePath, checkedKeys);
        }

        if (checkedKeys.includes(routePath) || (route.children && route.children.length >= 1)) {
          res.push(route);
        }
      }
      return res;
    },
    async confirmRole() {
      const isEdit = this.dialogType === 'edit';

      const checkedKeys = this.$refs.tree.getCheckedKeys();
      this.role.routes = this.generateTree(deepClone(this.serviceRoutes), '/', checkedKeys);

      if (isEdit) {
        await updateRole(this.role.key, this.role);
        for (let index = 0; index < this.rolesList.length; index++) {
          if (this.rolesList[index].key === this.role.key) {
            this.rolesList.splice(index, 1, Object.assign({}, this.role));
            break;
          }
        }
      } else {
        const { data } = await addRole(this.role);
        this.role.key = data.key;
        this.rolesList.push(this.role);
      }

      const { description, key, name } = this.role;
      this.dialogVisible = false;
      ElNotification({
        title: 'Success',
        dangerouslyUseHTMLString: true,
        message: `
            <div>Role Key: ${key}</div>
            <div>Role Name: ${name}</div>
            <div>Description: ${description}</div>
          `,
        type: 'success'
      });
    },

    onlyOneShowingChild(children = [], parent) {
      let onlyOneChild = null;
      const showingChildren = children.filter(item => {
        if (item.meta && item.meta.hidden) {
          return false;
        }
        return true;
      });

      if (showingChildren.length === 1) {
        onlyOneChild = showingChildren[0];
        onlyOneChild.path = this.resolvePath(parent.path, onlyOneChild.path);
        return onlyOneChild;
      }

      if (showingChildren.length === 0) {
        onlyOneChild = { ...parent, path: '', noShowingChildren: true };
        return onlyOneChild;
      }

      return false;
    }
  }
});
</script>

<style lang="scss" scoped>
.app-container {
  .roles-table {
    margin-top: 1.875rem;
  }

  .permission-tree {
    margin-bottom: 1.875rem;
  }
}

@media (max-width: 768px) {
  :deep(.responsive-dialog-600) {
    width: 90% !important;
  }
  :deep(.el-form-item) {
    flex-direction: column;
    align-items: flex-start;
  }
  :deep(.el-form-item__label) {
    text-align: left;
    margin-bottom: 4px;
  }
  :deep(.el-form-item__content) {
    width: 100%;
    margin-left: 0 !important;
  }
}
@media (min-width: 768px) {
  :deep(.responsive-dialog-600) {
    width: 600px !important;
  }
}
</style>

