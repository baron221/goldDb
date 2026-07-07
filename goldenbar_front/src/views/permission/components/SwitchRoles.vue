<template>
<div>
    <div style="margin-bottom: 0.9375rem;">
      Your roles: {{ roles }}
    </div>
    Switch roles:
    <el-radio-group v-model="switchRoles">
      <el-radio-button value="editor">Editor</el-radio-button>
      <el-radio-button value="admin">Admin</el-radio-button>
    </el-radio-group>
  </div>
</template>

<script>
import { defineComponent } from 'vue';
import store from '@/store';

export default defineComponent({
  computed: {
    roles() {
      return store.user().roles;
    },
    switchRoles: {
      get() {
        return this.roles[0];
      },
      set(val) {
        store.user().changeRoles(val).then(() => {
          this.$emit('change');
        });
      }
    }
  }
});
</script>

